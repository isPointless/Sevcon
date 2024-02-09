###############################################################################
# (C) COPYRIGHT Sevcon Limited 2003
# 
# CCL Project Reference C6944 - Magpie
# 
# FILE
#     $ProjectName:DVT$
#     $Revision:1.0$
#     $Author:ps$
# 
# ORIGINAL AUTHOR
#     Paul Shipley
# 
# DESCRIPTION
#     General Tcl scripts to handle and alert user to EMCY messages
# 
# REFERENCES
# 
# MODIFICATION HISTORY
#     $Log:  104368: emcy.tcl 
# 
#     Rev 1.0    13/03/2007 09:11:40  ps
#  Added updated EMCY handler
# 
#     Rev 1.0    07/02/2007 12:32:42  ps
#  Adding various new DVT scripts
# 
# register module
register DVT-PROD [info script] {$Revision:1.0$}

# emcy message handler
proc emcy_handler {args} {
    
    # split packet data and read out error code, fault id and data bytes
    set packet [split $args " {}"]
    
    set nodeid [expr [lindex $packet 5] - 0x0080]
    set error_code "[lindex $packet 9][string range [lindex $packet 8] 2 3]"
    set fault_id   "[lindex $packet 12][string range [lindex $packet 11] 2 3]"
    
    # if there is no fault set, then all faults have been cleared.
    if {($fault_id == 0x0000) && ($error_code == 0x0000)} {
        infomsg "/nNo faults set/n" fault
    } else {
        
        # read error register and data bytes
        set error_reg  [lindex $packet 10]
        set db1        [lindex $packet 13]
        set db2        [lindex $packet 14]
        set db3        [lindex $packet 15]
        
        # set fault description
        set fault_desc [ get_event_name $fault_id $nodeid ]
        
        # calculate data/time stamp
        set timestamp [clock format [clock seconds] -format {%H:%M:%S, %d/%m/%y}]
        
        # if the error code is 0x0000, then this indicates the fault has reset.
        if {$error_code != 0x0000} {
            infomsg "Node $nodeid fault ($fault_id, $fault_desc) set at $timestamp. Data ($db1 $db2 $db3). CANopen Error Code: $error_code" fault_set
        } else {
            infomsg "Node $nodeid fault ($fault_id, $fault_desc) cleared at $timestamp. Data ($db1 $db2 $db3). CANopen Error Code: $error_code" fault_cleared
        }
    }
}

co_add_protocol_handler emcy emcy_handler
add_infomsg_colour_tag fault_set red
add_infomsg_colour_tag fault_cleared green

# Map EMCY COB-IDs to the handler
for { set cobid 0x0081 } { $cobid <= 0x00ff } { incr cobid } {
    co_map_cobid_to_protocol emcy $cobid
}

