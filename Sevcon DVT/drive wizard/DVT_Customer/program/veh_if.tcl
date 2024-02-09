###############################################################################
# (C) COPYRIGHT Sevcon Limited 2004
# 
# CCL Project Reference C6944 - Magpie
# 
# FILE
#     $ProjectName:DVT$
#     $Revision:1.26$
#     $Author:ceh$
# 
# ORIGINAL AUTHOR
#     Chris Hauxwell
# 
# DESCRIPTION
#     Vehicle Interface functionality. Acts as a basic test and debug tool
# 
# REFERENCES
# 
# MODIFICATION HISTORY
#     $Log:  42345: veh_if.tcl 
# 
#     Rev 1.26    20/10/2008 16:51:44  ceh
#  Source object_data.tcl to pick up any changes. Also a bit of housekeeping.
# 
#     Rev 1.25    20/02/2008 14:01:30  ceh
#  Fixed bug causing faults set in a previous trace to be added to new traces.
# 
#     Rev 1.24    11/02/2008 10:39:34  ceh
#  Force cobid to be upper case so that hex numbers match in pdo handler. 
# 
#     Rev 1.23    21/02/2007 15:03:20  ceh
#  Check for window already being open is now done by dvt.tcl before this file
#  is sourced, to prevent screwing up an existing logging session
# 
#     Rev 1.22    08/11/2006 13:40:34  ceh
#  Added module revision registering.
# 
#     Rev 1.21    01/09/2006 11:19:34  ceh
#  Updated so that functions used to add variables output a warning if the same
#  item is added twice. This can happen with the new veh_if_cfg.tcl file if an
#  item is mapped more than once.
# 
#     Rev 1.20    04/08/2006 13:09:52  ps
#  Automatically determine item to be graphed from TPDO maps on controller
# 
#     Rev 1.19    07/07/2006 09:44:30  cmp
#  stack objects in multiple comumns in lew of bigger monitors
# 
#     Rev 1.18    08/03/2006 08:16:16  ps
#  Removed event IDs button - was causing problems.
# 
#     Rev 1.17    06/03/2006 15:27:58  ps
#  Updated to get fault IDs from get_event_name, instead of the (usually
#  inaccurate) veh_if_fault_desc array.
# 
#     Rev 1.16    28/02/2006 08:18:18  ceh
#  Added fault logging to traces.
# 
#    Rev 1.15    16/06/2005 11:40:16  ps
#  Change colour of sliders to increase contrast

# 
#    Rev 1.14    31/05/2005 21:22:42  ceh
#  Disable reset and chart icons during logging to prevent accidental reset of
#  data.

# 
#    Rev 1.13    18/05/2005 09:16:04  ceh
#  Display scaling in chart and logging file

# 
#    Rev 1.12    11/05/2005 12:40:40  ceh
#  Added multi-node support. Heartbeats and faults can now be seen for more than
#  one node. Also, modified chart so that lines can be hidden by clicking on the
#  legend. Its a bit of a hack at the moment since it just changes the trace
#  colour to white. There is a hide command, but it doesn't work in the this
#  version of BLT. There is a patch apparently, but I can't find it yet.

# 
#    Rev 1.11    06/05/2005 09:36:56  ceh
#  Automatically creates veh_if_log directory if it doesn't exist.

# 
#    Rev 1.10    04/05/2005 18:17:56  ceh
#  Added heartbeating and NMT state detection to Vehicle Interface

# 
#    Rev 1.9    04/05/2005 14:08:34  ceh
#  Added heartbeat handler to vehicle interface so that a controller restart can
#  be detected. We reset all actives faults when this occurs.

# 
#    Rev 1.8    29/04/2005 11:07:04  ceh
#  Flag error if Vehicle Interface window is already open when user attempts to
#  start interface.

# 
#    Rev 1.7    05/04/2005 17:57:28  ceh
#  Added functionality to log data to a file

# 
#    Rev 1.6    16/03/2005 12:11:26  ceh
#  Fixed bug which caused an error when zooming in on the graph

# 
#    Rev 1.5    14/12/2004 09:55:20  ceh
#  Changed script to automatically generate I/O configuration file if it doesn't
#  exist.

# 
#    Rev 1.4    10/12/2004 14:41:36  ceh
#  Added graphing capability. Requires BLT package.

# 
# 
###############################################################################

# register module
if {$veh_if_register} {
    register DVT [info script] {$Revision:1.26$}
    set veh_if_register 0
    return
}

set veh_if_ver     {$Revision:1.26$}

# default I/O configuration file
set veh_io_file_sample_contents "
# Digital Inputs.
# Usage: veh_if_add_di \"<name>\" {pdo <cobid> <bit_start> <n_bits> <uint|int> <trace> <trace_colour>}

# The following assumes 0x6800,1 is mapped into PDO Cob-ID 0x201, bits 0-7
veh_if_add_di \"Forward\" {pdo 0x0201 0 1 uint 0 black}
veh_if_add_di \"Reverse\" {pdo 0x0201 1 1 uint 0 black}
veh_if_add_di \"FS1\"     {pdo 0x0201 2 1 uint 0 black}
veh_if_add_di \"Seat\"    {pdo 0x0201 3 1 uint 0 black}


# Analogue Inputs.
# Usage: veh_if_add_ai \"<name>\" <start_voltage> <end_voltage> <scaling> {pdo <cobid> <bit_start> <n_bits> <uint|int> <trace> <trace_colour>}

# The following assumes 0x6C01,1 is mapped into PDO Cob-ID 0x201, bits 8-24
veh_if_add_ai \"Throttle\" 0.00 5.00 \[expr 1.0/0x100\] {pdo 0x0201 8 16 uint 0 black}


# Digital Outputs.
# Usage: veh_if_add_do \"<name>\" {pdo <cobid> <bit_start> <n_bits> <uint|int> <trace> <trace_colour>}
# eg. veh_if_add_do \"Hours Meter\" {pdo 0x0301 0 1 uint 0 black}


# Analogue Outputs.
# Usage: veh_if_add_ao \"<name>\" <start_voltage> <end_voltage> <scaling> {pdo <cobid> <bit_start> <n_bits> <uint|int> <trace> <trace_colour>}
# eg. veh_if_add_ao \"Line\" 0.00 48.00 \[expr 1.0/0x100\] {pdo 0x0301 0 16 uint 0 black}


# Watches.
# Usage: veh_if_add_watch \"<name>\" <start_value> <end_value> <scaling> {pdo <cobid> <bit_start> <n_bits> <uint|int> <trace> <trace_colour>}
# eg. veh_if_add_watch \"Torque\" 0.00 50.00 \[expr 20.0/1000\] {pdo 0x0202 0 16 int 0 black}



# Nodes
# Usage: veh_if_add_node <nodeid> <emcy_cobid>
# eg. veh_if_add_node 1 0x0081
veh_if_add_node 1 0x0081

return
"


set col_depth 9


# clear output globals
global veh_if_values; array unset veh_if_values
global veh_if_scales  array unset veh_if_scales


# help and about
proc veh_if_help {} {
    dvtConPuts stderr "No help available yet"
}

proc veh_if_about {} {
    global veh_if_ver
    dvtConPuts stderr "Vehicle Interface: [string map {$ ""} $veh_if_ver]"
}

# CAN pause
set veh_if_can_pause_state 0
proc veh_if_can_pause {} {
    global veh_if_can_pause_state
    dvtConPuts stderr "CAN interface not available yet.\nDEBUG Check state = $veh_if_can_pause_state"
}

# exit proc. Removes mappings, etc
proc veh_if_exit {} {
    global config_filename
    global veh_if_config
    global veh_if_task_id
    global veh_pdos veh_emcys veh_hbeats
    
    # remove cobid handlers for PDOs, EMCYs and Heartbeats
    if {[info exists veh_pdos]} {
        foreach cobid [array names veh_pdos] {
            co_del_cobid_handler $cobid veh_if_pdo_handler
        }
    }
    
    if {[info exists veh_emcys]} {
        foreach cobid $veh_emcys {
            co_del_cobid_handler $cobid veh_if_emcy_handler
        }
    }
    
    if {[info exists veh_hbeats]} {
        foreach cobid $veh_hbeats {
            co_del_cobid_handler $cobid veh_if_heartbeat_handler
        }
    }
    
    
    # save current settings to file
    if {[catch {open $config_filename w} cfid]} {
        dvtConPuts stderr "Unable to store current setup to $config_filename"
    } else {
        foreach name [array names veh_if_config] {
            puts $cfid "set veh_if_config($name) $veh_if_config($name)"
        }

        close $cfid
    }

    # cancel the polling task
    if {[info exists veh_if_task_id]} {
        after cancel $veh_if_task_id
    }
    
    # destroy the window
    destroy .veh_if
}

# PDO handler
proc veh_if_pdo_handler {args} {
    global veh_pdos veh_if_values veh_if_scales veh_if_tracing
    
    set packet [split $args " {}"]
    
    set cobid [lindex $packet 5]
    foreach pdo_map $veh_pdos($cobid) {
        set name  [lindex $pdo_map 0]
        set start [lindex $pdo_map 1]
        set end   [lindex $pdo_map 2]
        set shift [lindex $pdo_map 3]
        set mask  [lindex $pdo_map 4]
        set type  [lindex $pdo_map 5]
        set trace [lindex $pdo_map 6]
    
        set value 0
        for {set index $end} {$index >= $start} {incr index -1} {
            set value [expr ($value << 8) + [lindex $packet [expr $index + 8]]]
        }
        set value [expr ($value >> $shift) & $mask]
    
        # check for -ve number. 
        switch $type {
            int8  {if {$value > 0x7F}       {set value [expr $value-0x100]}}
            int16 {if {$value > 0x7FFF}     {set value [expr $value-0x10000]}}
            int32 {if {$value > 0x7FFFFFFF} {set value [expr $value-0x100000000]}}
        }
        
        # apply scaling and update value in window
        set veh_if_values($name) [expr $value * $veh_if_scales($name)]

        # if trace required, update array
        if {$trace && $veh_if_tracing} {
            veh_if_trace_value $name $veh_if_values($name)
        }
    }
}


# EMCY handler
proc veh_if_emcy_handler {args} {
    global veh_if_node_faults
    
    # split packet data and read out cobid, error code, fault id and data bytes
    set packet [split $args " {}"]
    
    set cobid      [lindex $packet 5]
    set error_code "[lindex $packet 9][string range [lindex $packet 8] 2 3]"
    set fault_id   "[lindex $packet 12][string range [lindex $packet 11] 2 3]"
    
    global veh_if_active_faults_[set cobid]
    
    
    # read the fault description from the OD
    set fault_desc [ get_event_name $fault_id ]
    
    # if fault is being cleared, remove it from the active list, otherwise add it.
    if {$error_code == 0x0000} {
        if {[info exists veh_if_active_faults_[set cobid]($fault_id)]} {
            unset veh_if_active_faults_[set cobid]($fault_id)
            
            veh_if_log_fault [set cobid]_[set fault_desc] 0
        }
    } else {
        set error_reg  [lindex $packet 10]
        set db1        [lindex $packet 13]
        set db2        [lindex $packet 14]
        set db3        [lindex $packet 15]
        
        set veh_if_active_faults_[set cobid]($fault_id) "Fault $fault_id ($fault_desc) (D: $db1 $db2 $db3) set"
        
        veh_if_log_fault [set cobid]_[set fault_desc] 1
    }
    
    
    # check if there are any active faults. If not, display OK message, otherwise display the error code for the most severe fault
    if {[array size veh_if_active_faults_[set cobid]] == 0} {
        .veh_if.io.nodes.[set cobid]_fault configure -bg lightgreen
        set veh_if_node_faults($cobid) "No faults set"
    } else {
        .veh_if.io.nodes.[set cobid]_fault configure -bg salmon
        set veh_if_node_faults($cobid) [set veh_if_active_faults_[set cobid]([lindex [lsort -decreasing [array names veh_if_active_faults_[set cobid]]] 0])]
    }
}


# Fault log
if {[array exists veh_if_fault_last_states]} {array unset veh_if_fault_last_states}

proc veh_if_log_fault {name state} {
    global veh_if_fault_last_states
    global veh_if_trace_start_time
    global veh_if_fault_logging_enabled
    
    # if fault logging is enabled and we are currently tracing, record faults
    if {$veh_if_fault_logging_enabled} {
    
        # replace spaces in name with _
        set name [string map {" " _} $name]
    
        # work out x and y value names and make them global
        set x_values veh_if_[set name]_x
        set y_values veh_if_[set name]_y
    
        global $x_values
        global $y_values
    
        # check if this fault has been logged before. If not, create a new trace for it now
        if {![info exists veh_if_fault_last_states($name)]} {

            veh_if_create_trace $name black
            set veh_if_fault_last_states($name) "None"
        
            # log start time as first event
            lappend $x_values $veh_if_trace_start_time
            lappend $y_values [expr !$state]
        }
    
        # trace the fault if its state has changed. Added 2 items for each fault to make it
        # look like a square wave
        if {$veh_if_fault_last_states($name) != $state} {
            
            set now [clock clicks -milliseconds]
        
            lappend $x_values $now
            lappend $y_values [expr !$state]
        
            lappend $x_values $now
            lappend $y_values $state
        
            set veh_if_fault_last_states($name) $state
        }
    }
}


# proc to complete fault log. Required to get good square waveforms
proc veh_if_end_fault_log {} {
    global veh_if_fault_last_states
    
    if {[array exists veh_if_fault_last_states]} {
        set now [clock clicks -milliseconds]
        
        foreach name [array names veh_if_fault_last_states] {
            set x_values veh_if_[set name]_x
            set y_values veh_if_[set name]_y
    
            global $x_values
            global $y_values
            
            lappend $x_values $now
            lappend $y_values $veh_if_fault_last_states($name)
        }
    }
}

# Heartbeat message handler
proc veh_if_heartbeat_handler {args} {
    global veh_if_prev_state
    global veh_if_hbeat_img
    global veh_if_node_faults
    global veh_if_node_states
    global veh_hbeats
    global veh_emcys 
    
    # read cobid. 
    set cobid [lindex [lindex $args 0] 4]
    
    # convert state to decimal number
    set state [expr [lindex [lindex $args 0] 5]]
    
    # if boot up received, delete all active faults
    # if op or pre-op received, change statusbar state
    if {$state != $veh_if_prev_state($cobid)} {
        switch $state {
            0   {
                    # we need to work out the emcy cobid for the node to which this heartbeat belongs.
                    # fairly easy, since the emcy cobid is in the same position in veh_emcys as
                    # the hbeat cobid is in veh_hbeats.
                    set i [lsearch $veh_hbeats $cobid]
                    if {$i == -1} {
                        dvtConPuts stderr "ERROR: Unable to locate $cobid in veh_hbeats list. Cannot reset faults on bootup."
                    } else {
                        set emcy_cobid [lindex $veh_emcys $i]
                        if {$emcy_cobid == ""} {
                            dvtConPuts stderr "ERROR: Unable to locate emcy cobid in veh_emcys list. Cannot reset faults on bootup."
                        } else {
                            global veh_if_active_faults_$emcy_cobid
                            if {[array exists veh_if_active_faults_$emcy_cobid]} {array unset veh_if_active_faults_$emcy_cobid}
                            .veh_if.io.nodes.[set emcy_cobid]_fault configure -bg lightgreen
                            set veh_if_node_faults($emcy_cobid) "No faults set"

                            .veh_if.io.nodes.[set cobid]_state configure -bg salmon
                            .veh_if.io.nodes.[set cobid]_state configure -image .pre_op_gif
                            set veh_if_node_states($cobid) "Boot"
                        }
                    }
                }
            
            5   {
                    .veh_if.io.nodes.[set cobid]_state configure -bg lightgreen
                    .veh_if.io.nodes.[set cobid]_state configure -image .op_gif
                    set veh_if_node_states($cobid) "Op"
                }
            
            127 {
                    .veh_if.io.nodes.[set cobid]_state configure -bg salmon
                    .veh_if.io.nodes.[set cobid]_state configure -image .pre_op_gif
                    set veh_if_node_states($cobid) "Pre"
                }
            
            default {
                    .veh_if.io.nodes.[set cobid]_state configure -bg salmon
                    .veh_if.io.nodes.[set cobid]_state configure -image .pre_op_gif
                    set veh_if_node_states($cobid) "???"
                }
        }
        
        set veh_if_prev_state($cobid) $state
    }
    
    # toggle heartbeat image on each heartbeat
    if {$veh_if_hbeat_img($cobid) == 1} {
        set veh_if_hbeat_img($cobid) 2
    } else {
        set veh_if_hbeat_img($cobid) 1
    }
    .veh_if.io.nodes.[set cobid]_hbeat configure -image .hbeat[set veh_if_hbeat_img($cobid)]_gif
}


# function to map SDO or PDO comms to update window
set n_pdos  0
set n_sdos  0
proc veh_if_map_comms {name comms} {
    global veh_pdos
    global veh_sdos
    global veh_emcys
    global veh_hbeats
    global n_pdos n_sdos n_nodes veh_if_n_comm_types
    global veh_if_fault_logging_enabled
    
    set comm_len [llength $comms]
    if {$comm_len == 0} {
        dvtConPuts stderr "Invalid communication setup ($comms)"
        return Error
    } else {
        set comm_type [string tolower [lindex $comms 0]]
        switch $comm_type {
            pdo {
                if {$comm_len != 7} {
                    dvtConPuts stderr "PDO setup in wrong format. Usage: pdo <cobid> <start_bit> <n_bits> <int|uint> <trace> <trace_colour>. Got $comms"
                    return Error
                } else {
                    set cobid     [format 0x%04X [lindex $comms 1]]
                    set start_bit [lindex $comms 2]
                    set n_bits    [lindex $comms 3]
                    set type      [lindex $comms 4]
                    set trace     [lindex $comms 5]
                    set colour    [lindex $comms 6]
                    
                    if {[catch {expr $start_bit + $n_bits} total]} {
                        dvtConPuts stderr "Invalid start_bit ($start_bit) or n_bits ($n_bits)"
                        return Error
                    } elseif {$total > 64} {
                        dvtConPuts stderr "start_bit ($start_bit) + n_bits ($n_bits) must be less than 64"
                        return Error
                    }
                    
                    # add a handler for the cobid. Remember any currently mapped handler. We
                    # will need to call this in our handler to keep everything working properly.
                    co_add_cobid_handler $cobid veh_if_pdo_handler

                    # convert pdo map into something easier to handle. ie Start and end bytes in packet
                    # number of bits to shift right to align the start to bit 0 and a mask to remove any
                    # trailing bits from the last byte. ie. If start_bit was 12 and n_bits was 2, we would
                    # have a start_byte of 1 (12/8) and end_byte of 1 (14/8), shift of 4 (12%8) and mask = 0x03.
                    set mask 0
                    for {set index 0} {$index < $n_bits} {incr index} {
                        set mask [expr ($mask * 2) + 1]
                    }
                    
                    # set int type. This tells the PDO handler how to deal with signed numbers
                    set int_type none
                    if {$type == "int"} {
                        switch $n_bits {
                            8  {set int_type int8}
                            16 {set int_type int16}
                            32 {set int_type int32}
                        }
                    }
                    
                    # create trace
                    if {$trace} {
                        veh_if_create_trace $name $colour
                    }
                    
                    # store data about PDO mapping
                    lappend veh_pdos($cobid) [list $name [expr $start_bit / 8] [expr ($start_bit + $n_bits - 1) / 8] [expr $start_bit % 8] $mask $int_type $trace]
                    incr n_pdos
                }
            }
            
            sdo {
                if {$comm_len != 7} {
                    dvtConPuts stderr "SDO setup in wrong format. Usage: sdo <nodeid> <idx> <sidx> <int|uint> <trace> <trace_colour>. Got $comms"
                    return Error
                } else {
                    set nodeid [lindex $comms 1]
                    set idx    [lindex $comms 2]
                    set sidx   [lindex $comms 3]
                    set type   [lindex $comms 4]
                    set trace  [lindex $comms 5]
                    set colour [lindex $comms 6]

                    # set int/uint flag.
                    if {$type == "int"} {
                        set int_type 1
                    } else {
                        set int_type 0
                    }

                    # create trace
                    if {$trace} {
                        veh_if_create_trace $name $colour
                    }
                    
                    # store sdo data
                    lappend veh_sdos($name) [list $nodeid $idx $sidx $int_type $trace]
                    incr n_sdos
                }
            }
            
            emcy {
                if {$comm_len != 3} {
                    dvtConPuts stderr "EMCY setup in wrong format. Usage: emcy <cobid> <log_enable>. Got $comms"
                    return Error
                } else {
                    set cobid [lindex $comms 1]
                    set veh_if_fault_logging_enabled [lindex $comms 2]
                    
                    # add a handler for the cobid.
                    co_add_cobid_handler $cobid veh_if_emcy_handler

                    lappend veh_emcys  $cobid
                }
            }
            
            hbeat {
                if {$comm_len != 2} {
                    dvtConPuts stderr "Heartbeat setup in wrong format. Usage: hbeat <cobid>. Got $comms"
                    return Error
                } else {
                    set cobid [lindex $comms 1]
                    
                    # add a handler for the heartbeat messages.
                    co_add_cobid_handler $cobid veh_if_heartbeat_handler

                    lappend veh_hbeats $cobid
                }
            }
            
            default {
                dvtConPuts stderr "Communication type (1st item) must be pdo, sdo, emcy or hbeat. Is $comm_type"
                return Error
            }
        }
    }
    
    # display number of PDOs, SDOs and nodes
    set veh_if_n_comm_types "$n_pdos PDOs, $n_sdos SDOs, $n_nodes nodes"
    
    return
}

# function to poll objects using SDOs
proc veh_if_poll {} {
    global veh_sdos
    global bad_can_comms
    global veh_if_config veh_if_values veh_if_scales
    global veh_if_task_id
    
    set start_time_ms [clock clicks -milliseconds]
    
    foreach name [array names veh_sdos] {
        foreach item $veh_sdos($name) {
            set nodeid [lindex $item 0]
            set idx    [lindex $item 1]
            set sidx   [lindex $item 2]
            set int    [lindex $item 3]
            set trace  [lindex $item 4]
        
            set r [sdo_rnx $nodeid $idx $sidx]
            if {[catch {expr $r} result]} {
                if {!$bad_can_comms} {set bad_can_comms 1; .veh_if.statusbar.label_can_status configure -text "CAN Error" -bg salmon -image .cross_gif}
                set veh_if_task_id [after $veh_if_config(sample_rate) veh_if_poll]
                return
            } else {
                if {$bad_can_comms} {set bad_can_comms 0; .veh_if.statusbar.label_can_status configure -text "CAN OK" -bg lightgreen -image .tick_gif}
            
                # set value
                set value [expr $r]
                
                # check for -ve number. 
                if {$int} {
                    switch [string length $r] {
                        4  {if {$value > 0x7F}       {set value [expr $value-0x100]}}
                        6  {if {$value > 0x7FFF}     {set value [expr $value-0x10000]}}
                        10 {if {$value > 0x7FFFFFFF} {set value [expr $value-0x100000000]}}
                    }
                }
                
                # apply scaling
                set value [expr $value * $veh_if_scales($name)]
                
                # update sdo value
                set veh_if_values($name) [expr $r * $veh_if_scales($name)]
                
                # if trace required, update array
                if {$trace} {
                    veh_if_trace_value $name $veh_if_values($name)
                }
            }
        }
    }       
    
    set time_ms [expr ($veh_if_config(sample_rate) - ([clock clicks -milliseconds] - $start_time_ms))]
    if {$time_ms < 0} {set time_ms 0}
    
    set veh_if_task_id [after $time_ms veh_if_poll]
}


# function to add digital inputs
set n_di 0
set di_col 0 
proc veh_if_add_di {name comms} {
    global n_di di_col col_depth
    global veh_if_values
    global veh_if_scales
    
    if {![info exists veh_if_values($name)]} {
        if {[veh_if_map_comms $name $comms] != ""} {return}
    
        set lbl .veh_if.io.di.[string tolower $name]
        set veh_if_values($name) 0
        set veh_if_scales($name) 1
    
        checkbutton $lbl -text $name -state disabled -disabledforeground black -variable veh_if_values($name) -width 20  -anchor w
        grid configure $lbl -row $n_di -column $di_col -sticky w
    
        incr n_di
    
        if {$n_di > $col_depth} {
            set n_di 0
            incr di_col
        }
    } else {
        dvtConPuts stderr "Warning. $name already added as a digital input. Possible duplication in configuration"
    }
    
    return
}

# function to add analogue inputs
set n_ai 0
set ai_col 0
proc veh_if_add_ai {name min max scaling comms} {
    global n_ai ai_col col_depth
    global veh_if_values
    global veh_if_scales

    if {![info exists veh_if_values($name)]} {
        if {[veh_if_map_comms $name $comms] != ""} {return}
    
        set lbl .veh_if.io.ai.[string tolower $name]
        set veh_if_values($name) $min
        set veh_if_scales($name) $scaling
    
        label [set lbl]_label -text "$name ([format "%.4f" $scaling])"
        grid configure [set lbl]_label -row $n_ai -column $ai_col -sticky e
    
        scale [set lbl]_scale -from $min -to $max -orient horizontal -troughcolor blue -sliderlength 5 -sliderrelief flat -showvalue 0 -state disabled -variable veh_if_values($name) -resolution 0.01 -width 10 -length 100
        grid configure [set lbl]_scale -row $n_ai -column [expr $ai_col + 1] -sticky w
    
        label [set lbl]_value -textvariable veh_if_values($name) -width 10 -anchor w
        grid configure [set lbl]_value -row $n_ai -column [expr $ai_col + 2] -sticky w
    
        incr n_ai
    
        if {$n_ai > $col_depth} {
            set n_ai 0
            incr ai_col 3
        }
    } else {
        dvtConPuts stderr "Warning. $name already added as an analogue input. Possible duplication in configuration"
    }
    
    return
}

# function to add digital outputs
set n_do 0
set do_col 0
proc veh_if_add_do {name comms} {
    global n_do do_col col_depth
    global veh_if_values
    global veh_if_scales
    
    if {![info exists veh_if_values($name)]} {
        if {[veh_if_map_comms $name $comms] != ""} {return}
    
        set lbl .veh_if.io.do.[string tolower $name]
        set veh_if_values($name) 0
        set veh_if_scales($name) 1
    
        checkbutton $lbl -text $name -state disabled -disabledforeground black -variable veh_if_values($name) -width 10 -anchor w
        grid configure $lbl -row $n_do -column $do_col -sticky w
        
        incr n_do
    
        if {$n_do > $col_depth} {
            set n_do 0
            incr do_col
        }
    } else {
        dvtConPuts stderr "Warning. $name already added as a digital output. Possible duplication in configuration"
    }
    
    return
}

# function to add analogue outputs
set n_ao 0
set ao_col 0
proc veh_if_add_ao {name min max scaling comms} {
    global n_ao ao_col col_depth
    global veh_if_values
    global veh_if_scales
    
    if {![info exists veh_if_values($name)]} {
        if {[veh_if_map_comms $name $comms] != ""} {return}
    
        set lbl .veh_if.io.ao.[string tolower $name]
        set veh_if_values($name) $min
        set veh_if_scales($name) $scaling
    
        label [set lbl]_label -text $name -width 7 -anchor w
        grid configure [set lbl]_label -row $n_ao -column $ao_col -sticky e
    
        scale [set lbl]_scale -from $min -to $max -orient horizontal -troughcolor red -sliderlength 5 -sliderrelief flat -showvalue 0 -state disabled -variable veh_if_values($name) -width 10 -length 100
        grid configure [set lbl]_scale -row $n_ao -column [expr $ao_col + 1] -sticky e
    
        label [set lbl]_value -textvariable veh_if_values($name) -width 10 -anchor e
        grid configure [set lbl]_value -row $n_ao -column [expr $ao_col + 2] -sticky e

        incr n_ao
    
        if {$n_ao > $col_depth} {
            set n_ao 0
            incr ao_col 3
        }
    } else {
        dvtConPuts stderr "Warning. $name already added as an analogue output. Possible duplication in configuration"
    }
    
    return
}

# function to add watch variables
set n_watches 0
set watch_col 0
proc veh_if_add_watch {name min max scaling comms} {
    global n_watches watch_col col_depth
    global veh_if_values
    global veh_if_scales

    if {![info exists veh_if_values($name)]} {
        if {[veh_if_map_comms $name $comms] != ""} {return}
    
        set lbl .veh_if.io.watch.[string tolower $name]
        set veh_if_values($name) $min
        set veh_if_scales($name) $scaling
    
        label [set lbl]_label -text "$name ([format "%.4f" $scaling])"
        grid configure [set lbl]_label -row $n_watches -column $watch_col -sticky e
    
        scale [set lbl]_scale -from $min -to $max -orient horizontal -troughcolor yellow -sliderlength 5 -sliderrelief flat -showvalue 0 -state disabled -variable veh_if_values($name) -resolution 0.01 -width 10 -length 100
        grid configure [set lbl]_scale -row $n_watches -column [expr $watch_col + 1] -sticky e
    
        label [set lbl]_value -textvariable veh_if_values($name) -width 10 -anchor w
        grid configure [set lbl]_value -row $n_watches -column [expr $watch_col + 2] -sticky w

        incr n_watches
    
        if {$n_watches > $col_depth} {
            set n_watches 0
            incr watch_col 3
        }
    } else {
        dvtConPuts stderr "Warning. $name already added as a watch. Possible duplication in configuration"
    }
    
    return
}


# function to add digital inputs
set n_nodes 0
proc veh_if_add_node {nodeid emcy_cobid {log_faults 1}} {
    global n_nodes
    global veh_if_node_states
    global veh_if_node_faults
    global veh_if_prev_state
    global veh_if_hbeat_img
    
    set name "node_$nodeid"
    set hbeat_cobid [format "0x%04x" [expr 0x700 + $nodeid]]
    
    incr n_nodes
    if {[veh_if_map_comms $name "emcy $emcy_cobid $log_faults"] != ""} {return}
    if {[veh_if_map_comms $name "hbeat $hbeat_cobid"] != ""} {return}
    
    set lbl_emcy   .veh_if.io.nodes.$emcy_cobid
    set lbl_hbeat  .veh_if.io.nodes.$hbeat_cobid
    
    label [set lbl_hbeat]_label -text "Node $nodeid" -width 7 -anchor w
    grid configure [set lbl_hbeat]_label -row $n_nodes -column 0 -sticky w
    
    set veh_if_hbeat_img($hbeat_cobid) 1
    label [set lbl_hbeat]_hbeat -text "HBeat" -width 50 -height 13 -anchor w -image .hbeat1_gif -compound left -relief sunken
    grid configure [set lbl_hbeat]_hbeat -row $n_nodes -column 1 -sticky w -padx 2 -pady 2
    
    set veh_if_prev_state($hbeat_cobid)  255
    set veh_if_node_states($hbeat_cobid) "Boot"
    label [set lbl_hbeat]_state -textvariable veh_if_node_states($hbeat_cobid) -width 45 -height 13 -anchor w -image .pre_op_gif -background salmon -compound left -relief sunken
    grid configure [set lbl_hbeat]_state -row $n_nodes -column 2 -sticky w -padx 2 -pady 2
    
    set veh_if_node_faults($emcy_cobid) "No faults set"
    label [set lbl_emcy]_fault -textvariable veh_if_node_faults($emcy_cobid) -width 55 -anchor center -background lightgreen -relief sunken
    grid configure [set lbl_emcy]_fault -row $n_nodes -column 3 -sticky w -padx 2 -pady 2
}

# function to add fault information. NOTE: This has been replaced by veh_if_add_node
proc veh_if_add_fault {comms} {
    dvtConPuts stderr "ERROR: veh_if_add_fault\{\} is no longer supported. Use veh_if_add_node\{\} instead. veh_if_add_node\{\} will be called with nodeid=1 and emcy_cobid passed in"
    veh_if_add_node 1 [lindex $comms 1]
}

# function to add trace to chart
global veh_if_vectors
if {[array exists veh_if_vectors]} {array unset veh_if_vectors}
proc veh_if_create_trace {name colour} {
    global veh_if_vectors
    
    set x_values veh_if_[set name]_x
    set y_values veh_if_[set name]_y
    
    global $x_values
    global $y_values
    
    set $x_values ""
    set $y_values ""

    set veh_if_vectors($name) [list $x_values $y_values $colour]
}

# function to start trace
global veh_if_trace_start_time
set veh_if_trace_start_time [clock clicks -milliseconds]
proc veh_if_reset_trace {} {
    global veh_if_vectors
    global veh_if_trace_start_time
    global veh_if_fault_last_states
    
    # delete all current elements
    if {[.veh_if.chart.cht element names] != ""} {
        foreach el [.veh_if.chart.cht element names] {
            .veh_if.chart.cht element delete $el
        }
    }
    
    # clear existing lists
    foreach name [array names veh_if_vectors] {
        set x_values [lindex $veh_if_vectors($name) 0]
        set y_values [lindex $veh_if_vectors($name) 1]
        
        global $x_values
        global $y_values
        
        set $x_values ""
        set $y_values ""
    }

    # delete array of faults so they do not get logged again
    if {[array exists veh_if_fault_last_states]} {
        foreach fault [array names veh_if_fault_last_states] {
            if {[info exists veh_if_vectors($fault)]} {
                unset veh_if_vectors($fault)
            }
        }
        array unset veh_if_fault_last_states
    }
    
    # set start time
    set veh_if_trace_start_time [clock clicks -milliseconds]
}

# function to start tracing
set veh_if_tracing 0
proc veh_if_start_trace {} {
    global veh_if_tracing
    set veh_if_tracing 1
    .veh_if.statusbar.label_logging configure -text "Logging" -bg lightgreen -image .tick_gif
    .veh_if.toolbar.start_trace_button configure -state disabled
    .veh_if.toolbar.draw_chart_button  configure -state disabled
    .veh_if.toolbar.reset_trace_button configure -state disabled
    .veh_if.toolbar.pause_trace_button configure -state normal
}

# function to pause tracing
proc veh_if_pause_trace {} {
    global veh_if_tracing 
    set veh_if_tracing 0
    .veh_if.statusbar.label_logging configure -text "Not logging" -bg salmon -image .cross_gif
    .veh_if.toolbar.start_trace_button configure -state normal
    .veh_if.toolbar.draw_chart_button  configure -state normal
    .veh_if.toolbar.reset_trace_button configure -state normal
    .veh_if.toolbar.pause_trace_button configure -state disabled
    
    # log recorded data
    veh_if_end_fault_log
    veh_if_log_data_to_file
}

# function to draw chart
proc veh_if_draw_chart {} {
    global veh_if_vectors
    global veh_if_trace_start_time
    
    # delete all current elements
    if {[.veh_if.chart.cht element names] != ""} {
        foreach el [.veh_if.chart.cht element names] {
            .veh_if.chart.cht element delete $el
        }
    }
    
    # redraw with new elements
    foreach name [array names veh_if_vectors] {
        set x_values [lindex $veh_if_vectors($name) 0]
        set y_values [lindex $veh_if_vectors($name) 1]
        set colour   [lindex $veh_if_vectors($name) 2]
        
        global $x_values
        global $y_values
        
        # offset x_values with start time and divide by 1000 to convert into seconds
        set fmt_x_values ""
        foreach x [set $x_values] {
            lappend fmt_x_values [expr ($x - $veh_if_trace_start_time)/1000.0]
        }

        # create chart element
        .veh_if.chart.cht element create .veh_if.chart.el_[set name] -xdata $fmt_x_values -ydata [set $y_values] -symbol "" -color $colour -label $name
    }
}


# function to log data to file
proc veh_if_log_data_to_file {} {
    global veh_if_config
    global veh_if_vectors
    global veh_if_trace_start_time
    global veh_if_scales
    
    # check if logging to file is enabled
    if {$veh_if_config(logging_file_opt)} {
        
        # work out if there is anything to log first of all
        foreach name [array names veh_if_vectors] {
            
            # read x and y value array names from vector array
            set x_value_array_name [lindex $veh_if_vectors($name) 0]
            set y_value_array_name [lindex $veh_if_vectors($name) 1]
            
            # make them global
            global $x_value_array_name $y_value_array_name

            # if names do not yet exist, we mustn't have logged any data so don't open any files
            if {![info exist $x_value_array_name] || ![info exist $y_value_array_name]} {puts "$x_value_array_name or $y_value_array_name not found";return}
        }
        
        # work out the file name. If increment is enabled, append date and time to file 
        # name to ensure it is unique. If there is no extension, make it a .txt
        if {$veh_if_config(logging_incr_file_name)} {
            set fname [split $veh_if_config(logging_fname) .]
            lset fname 0 "[lindex $fname 0]_[clock format [clock seconds] -format {%d%m%y_%H%M%S}]"
            set fname [join $fname .]
        } else {
            set fname $veh_if_config(logging_fname)
        }
        
        # check that the directory exists, if not create it
        if {![file isdirectory ../veh_if_log]} {
            file mkdir ../veh_if_log
        }
        
        # append .txt extension if required and add directory location
        if {[llength [split $fname .]] <= 1} {
            append fname ".txt"
        } 
        set fname "../veh_if_log/$fname"
        
        # we have the filename, so open the file and log the data
        if {[catch {open $fname w} fid]} {
            dvtConPuts stderr "Unable to log data to $fname."
            return
        } else {
            foreach name [array names veh_if_vectors] {
                if {[info exists veh_if_scales($name)]} {
                    puts $fid "$name ($veh_if_scales($name)):"
                } else {
                    puts $fid "$name:"
                }

                # read x and y value array names from vector array
                set x_value_array_name [lindex $veh_if_vectors($name) 0]
                set y_value_array_name [lindex $veh_if_vectors($name) 1]
                   
                # get x and y values. Format time (x) and write to the log file
                set x_values [set $x_value_array_name]
                set y_values [set $y_value_array_name]
                for {set i 0} {$i < [llength $x_values]} {incr i} {
                    set x [expr ([lindex $x_values $i] - $veh_if_trace_start_time)/1000.0]
                    set y [lindex $y_values $i]
                    
                    puts -nonewline $fid "$x\t\t$y"
                }
                puts $fid "\n"
            }
            
            close $fid
        }
    }
}


# function to update trace data
proc veh_if_trace_value {name value} {
    set x_values veh_if_[set name]_x
    set y_values veh_if_[set name]_y
    
    global $x_values
    global $y_values
    
    lappend $x_values [clock clicks -milliseconds]
    lappend $y_values $value
}

# command to change the sample rate
proc veh_if_can_sample_rate {} {
    global veh_if_config
    
    set result [input_box2 "Enter sample rate in ms" "Sample Rate (ms)" 20 integer]
    if {$result == ""} {
        return
    } else {
        set veh_if_config(sample_rate) $result
    }
}

# command to change the sample rate
proc input_box2 {txt {title "User input"} {width 60} {type any} {win .input}} {
    global ipbox2_input
    global ipbox2_cmd
    
    set ipbox2_input ""
    set ipbox2_cmd ""
    
    # create input window
    toplevel $win
    wm attributes $win -topmost 1 -toolwindow 1
    wm resizable $win 0 0
    wm title $win $title
    
    # place widgets
    label $win.text    -text $txt
    entry $win.input   -textvariable ipbox2_input -width $width
    button $win.ok     -text "OK"     -command {set ipbox2_cmd "OK"}     -width 8
    button $win.cancel -text "Cancel" -command {set ipbox2_cmd "Cancel"} -width 8
    
    grid $win.text           -columnspan 2 -sticky w -pady 2
    grid $win.input          -columnspan 2 -sticky w -pady 2
    grid $win.ok $win.cancel               -sticky e -pady 2 -padx 2
    
    focus $win.input
    
    # limit what can be typed into the input box based on type
    switch $type {
        xdigit  {$win.input configure -validate key -vcmd {return [string is xdigit  %P]}}
        integer {$win.input configure -validate key -vcmd {return [string is integer %P]}}
        alnum   {$win.input configure -validate key -vcmd {return [string is alnum   %P]}}
        alpha   {$win.input configure -validate key -vcmd {return [string is alpha   %P]}}
        double  {$win.input configure -validate key -vcmd {return [string is double  %P]}}
        hex     {$win.input configure -validate key -vcmd {if {![catch {expr %P}] || ("%P" == "0x") || ("%P" == "{}")} {return 1} else {return 0}}}
    }
    
    bind $win.input <Return> {set ipbox2_cmd "OK"}
    bind $win.input <Escape> {set ipbox2_cmd "Cancel"}
    wm protocol $win WM_DELETE_WINDOW {set ipbox2_cmd "Cancel"}
    
    # wait for user response. TODO - use wait instead?
    vwait ipbox2_cmd
    
    # remove window and return user reponse
    if {$ipbox2_cmd == "OK"} {
        set result $ipbox2_input
    } else {
        set result ""
    }
    
    # remove window and unset globals
    destroy $win
    unset ipbox2_input
    unset ipbox2_cmd
    
    return $result
}





# draw main window
set veh_if_n_comm_types "0 PDOs, 0 SDOs, 0 EMCYs"
proc veh_if_gen_window {} {
    global veh_if_n_comm_types 
    
    toplevel .veh_if
    wm title .veh_if "Vehicle Interface"
    wm resizable .veh_if 0 0
    wm protocol .veh_if WM_DELETE_WINDOW veh_if_exit


    # generate Menubar
    menu .veh_if.menubar
    .veh_if.menubar add cascade -label File -menu .veh_if.menubar.file
    .veh_if.menubar add cascade -label UUT  -menu .veh_if.menubar.uut
    .veh_if.menubar add cascade -label Help -menu .veh_if.menubar.help

    # File menu
    menu .veh_if.menubar.file -tearoff 0
    .veh_if.menubar.file add command -label Exit -command veh_if_exit -accel "Alt+F4"

    # UUT menu
    menu .veh_if.menubar.uut -tearoff 0
    .veh_if.menubar.uut add checkbutton -label Pause            -command veh_if_can_pause -variable veh_if_can_pause_state
    .veh_if.menubar.uut add separator
    .veh_if.menubar.uut add command     -label "Sample Rate"    -command veh_if_can_sample_rate


    # Help menu
    menu .veh_if.menubar.help -tearoff 0
    .veh_if.menubar.help add command -label Help -command veh_if_help
    .veh_if.menubar.help add command -label About -command veh_if_about

    .veh_if configure -menu .veh_if.menubar


    # toolbar
    frame .veh_if.toolbar -relief flat -borderwidth 1
    pack  .veh_if.toolbar -side left -fill y

    # start CANbus sampling
    button .veh_if.toolbar.start_button \
        -image .connected_gif \
        -command {veh_if_can_pause start} \
        -state disabled

    set_balloon .veh_if.toolbar.start_button "Start SDO Sampling"

    pack .veh_if.toolbar.start_button -side left -padx 1

    # stop CANbus sampling
    button .veh_if.toolbar.stop_button \
        -image .disconnected_gif \
        -command {veh_if_can_pause stop}

    set_balloon .veh_if.toolbar.stop_button "Stop SDO Sampling"

    pack .veh_if.toolbar.stop_button -side left -padx 1

    # separator
    label .veh_if.toolbar.sep1 -image .separator_gif
    pack .veh_if.toolbar.sep1 -side left -padx 1
    
    # reset trace data for chart
    button .veh_if.toolbar.reset_trace_button \
        -image .reset_gif \
        -command {veh_if_reset_trace}

    set_balloon .veh_if.toolbar.reset_trace_button "Reset Trace"

    pack .veh_if.toolbar.reset_trace_button -side left -padx 1

    # start tracing data for chart
    button .veh_if.toolbar.start_trace_button \
        -image .play_gif \
        -command {veh_if_start_trace}

    set_balloon .veh_if.toolbar.start_trace_button "Start Trace"

    pack .veh_if.toolbar.start_trace_button -side left -padx 1

    # pause tracing data for chart
    button .veh_if.toolbar.pause_trace_button \
        -image .pause_gif \
        -state disabled \
        -command {veh_if_pause_trace}

    set_balloon .veh_if.toolbar.pause_trace_button "Pause Trace"

    pack .veh_if.toolbar.pause_trace_button -side left -padx 1

    # draw chart
    button .veh_if.toolbar.draw_chart_button \
        -image .chart_gif \
        -command {veh_if_draw_chart}

    set_balloon .veh_if.toolbar.draw_chart_button "Draw Chart"

    pack .veh_if.toolbar.draw_chart_button -side left -padx 1

    # separator
    label .veh_if.toolbar.sep3 -image .separator_gif
    pack .veh_if.toolbar.sep3 -side left -padx 1

    # set sample rate
    button .veh_if.toolbar.sample_rate_button \
        -image .clock_gif \
        -command veh_if_can_sample_rate

    set_balloon .veh_if.toolbar.sample_rate_button "Change SDO Sample Rate"

    pack .veh_if.toolbar.sample_rate_button -side left -padx 1

    # separator
    label .veh_if.toolbar.sep2 -image .separator_gif
    pack .veh_if.toolbar.sep2 -side left -padx 1
    
    
    # create i/o frames
    frame .veh_if.io -relief groove -borderwidth 2
    pack  .veh_if.io -side left -fill y

    labelframe .veh_if.io.di    -borderwidth 2 -relief groove -text "Digital Inputs"
    labelframe .veh_if.io.ai    -borderwidth 2 -relief groove -text "Analogue Inputs"
    labelframe .veh_if.io.do    -borderwidth 2 -relief groove -text "Digital Outputs"
    labelframe .veh_if.io.ao    -borderwidth 2 -relief groove -text "Analogue Outputs"
    labelframe .veh_if.io.watch -borderwidth 2 -relief groove -text "Watch"
    labelframe .veh_if.io.nodes -borderwidth 2 -relief groove -text "Node Status"

    
    
    # create chart 
    frame .veh_if.chart -relief groove -borderwidth 2
    
    global veh_if_chart_values veh_if_chart_sel_coords
    set veh_if_chart_values "Cursor: 0.000,0.000"
    set veh_if_chart_sel_coords(x1) -1
    set veh_if_chart_sel_coords(y1) -1
    set veh_if_chart_sel_coords(x2) -1
    set veh_if_chart_sel_coords(y2) -1
    set veh_if_chart_sel_coords(marker) ""
    
    # draw chart and configure crosshairs and axis
    graph .veh_if.chart.cht -height 200
    .veh_if.chart.cht crosshairs configure -hide no -color salmon
    .veh_if.chart.cht axis configure x -title Time
    
    # bind mouse button events to perform the following tasks:
    #   Left mouse button  - 1st click - Start marker to selection area to zoom in on
    #   Left mouse button  - 2nd click - Zoom in on selected area and delete marker.
    #   Right mouse button             - If marker is active, cancel it otherwise zoom out to show complete chart
    #   Mouse Motion                   - Redraw marker and log coordinates.
    
    # left mouse button
    bind .veh_if.chart.cht <ButtonPress-1> {
        # is this the first left mouse click? If so, start the marker.
        if {$veh_if_chart_sel_coords(x1) == -1} {
            set x_coord [%W axis invtransform x %x]
            set y_coord [%W axis invtransform y %y]

            # handle x-axis
            set min [%W axis cget x -min]
            if {$min == ""} {set min [lindex [%W axis limits x] 0]}
            set max [%W axis cget x -max]
            if {$max == ""} {set max [lindex [%W axis limits x] 1]}
            
            # check if clicked outside the plot area. If yes, ignore it
            if {($x_coord <= $max) && ($x_coord >= $min)} {
            
                # handle y-axis
                set min [%W axis cget y -min]
                if {$min == ""} {set min [lindex [%W axis limits y] 0]}
                set max [%W axis cget y -max]
                if {$max == ""} {set max [lindex [%W axis limits y] 1]}
            
                # check if clicked outside the plot area. If yes, ignore it
                if {($y_coord <= $max) && ($y_coord >= $min)} {
            
                    set veh_if_chart_sel_coords(x1) $x_coord
                    set veh_if_chart_sel_coords(y1) $y_coord
                    set veh_if_chart_sel_coords(x2) $x_coord
                    set veh_if_chart_sel_coords(y2) $y_coord
            
                    set veh_if_chart_sel_coords(marker) [%W marker create line -dashes 2]
                }
            }
        
        # else zoom in on selected area and delete marker
        } else {
            if {$veh_if_chart_sel_coords(x1) < $veh_if_chart_sel_coords(x2)} {
                %W axis configure x -min $veh_if_chart_sel_coords(x1) -max $veh_if_chart_sel_coords(x2)
            } elseif {$veh_if_chart_sel_coords(x1) != $veh_if_chart_sel_coords(x2)} {
                %W axis configure x -min $veh_if_chart_sel_coords(x2) -max $veh_if_chart_sel_coords(x1)
            }
            
            if {$veh_if_chart_sel_coords(y1) < $veh_if_chart_sel_coords(y2)} {
                %W axis configure y -min $veh_if_chart_sel_coords(y1) -max $veh_if_chart_sel_coords(y2)
            } elseif {$veh_if_chart_sel_coords(y1) != $veh_if_chart_sel_coords(y2)} {
                %W axis configure y -min $veh_if_chart_sel_coords(y2) -max $veh_if_chart_sel_coords(y1)
            }
        
            set veh_if_chart_sel_coords(x1) -1
            set veh_if_chart_sel_coords(y1) -1
            set veh_if_chart_sel_coords(x2) -1
            set veh_if_chart_sel_coords(y2) -1
            
            %W marker delete $veh_if_chart_sel_coords(marker)
        }
    }
    
    # right mouse button
    bind .veh_if.chart.cht <ButtonPress-3> {
        # if marker exists, cancel it
        if {[%W marker exists $veh_if_chart_sel_coords(marker)]} {
            set veh_if_chart_sel_coords(x1) -1
            set veh_if_chart_sel_coords(y1) -1
            set veh_if_chart_sel_coords(x2) -1
            set veh_if_chart_sel_coords(y2) -1
            
            %W marker delete $veh_if_chart_sel_coords(marker)
            
        # else zoom out to show the entire chart
        } else {
            %W axis configure x -min {} -max {}
            %W axis configure y -min {} -max {}
        }
    }
    
    # mouse motion
    bind .veh_if.chart.cht <Motion> {
        # transform x and y coords from screen values to axis values
        set x_coord [%W axis invtransform x %x]
        set y_coord [%W axis invtransform y %y]
        
        set min [%W axis cget x -min]
        if {$min == ""} {set min [lindex [%W axis limits x] 0]}
        set max [%W axis cget x -max]
        if {$max == ""} {set max [lindex [%W axis limits x] 1]}
        if {$x_coord > $max} {set x_coord $max}
        if {$x_coord < $min} {set x_coord $min}
        
        set min [%W axis cget y -min]
        if {$min == ""} {set min [lindex [%W axis limits y] 0]}
        set max [%W axis cget y -max]
        if {$max == ""} {set max [lindex [%W axis limits y] 1]}
        if {$y_coord > $max} {set y_coord $max}
        if {$y_coord < $min} {set y_coord $min}
        
        # redraw crosshairs
        %W crosshairs configure -position @%x,%y
        
        # display cursor coordinates
        set veh_if_chart_values "Cursor: [format %%0.3f $x_coord],[format %%0.3f $y_coord]"

        # if one is active, redraw marker
        if {[%W marker exists $veh_if_chart_sel_coords(marker)]} {
            set veh_if_chart_sel_coords(x2) $x_coord
            set veh_if_chart_sel_coords(y2) $y_coord
            
            set coord_list [list \
                $veh_if_chart_sel_coords(x1) $veh_if_chart_sel_coords(y1) \
                $veh_if_chart_sel_coords(x1) $veh_if_chart_sel_coords(y2) \
                $veh_if_chart_sel_coords(x2) $veh_if_chart_sel_coords(y2) \
                $veh_if_chart_sel_coords(x2) $veh_if_chart_sel_coords(y1) \
                $veh_if_chart_sel_coords(x1) $veh_if_chart_sel_coords(y1) \
            ]
            
            %W marker configure $veh_if_chart_sel_coords(marker) -coords $coord_list
        }
    }
    
    # bind left click on legend to hide/display information
    .veh_if.chart.cht legend bind all <ButtonPress-1> {
        global veh_if_vectors
        
        set elem [%W legend get current]
        if {[%W element cget $elem -labelrelief] == "flat"} {
            %W element configure $elem -labelrelief raised
            %W element configure $elem -color white
        
        } else {
            %W element configure $elem -labelrelief flat
            
            set elem_name [string range $elem 17 [string length $elem]]
            %W element configure $elem -color [lindex $veh_if_vectors($elem_name) 2]
        }
    }
    
    pack .veh_if.chart.cht -fill both

    label .veh_if.chart.xy_values -textvariable veh_if_chart_values
   
    grid configure .veh_if.chart.cht       -column 0 -row 0 -sticky nsew
    grid configure .veh_if.chart.xy_values -column 0 -row 1 -sticky nse
    
    
    # add window for logging options
    labelframe .veh_if.logging    -borderwidth 2 -relief groove -text "Logging Options"
    
    checkbutton .veh_if.logging.file_opt -text "Log to file" -width 10 -anchor w -variable veh_if_config(logging_file_opt)
    checkbutton .veh_if.logging.incr_file_name -text "Increment file name" -width 10 -anchor w -variable veh_if_config(logging_incr_file_name)
    entry .veh_if.logging.fname   -textvariable veh_if_config(logging_fname) -width 30 -textvariable veh_if_config(logging_fname)
    label .veh_if.logging.fname_lbl -text "Filename:"
    
    grid configure .veh_if.logging.file_opt         -row 0 -column 0 -sticky w  
    grid configure .veh_if.logging.incr_file_name   -row 1 -column 0 -sticky w  
    grid configure .veh_if.logging.fname_lbl        -row 0 -column 1 -sticky w -padx 15 
    grid configure .veh_if.logging.fname            -row 1 -column 1 -sticky w -padx 15 
    
    
    # add statusbar
    frame .veh_if.statusbar -relief flat -borderwidth 1
    pack  .veh_if.statusbar -side left -fill y

    label .veh_if.statusbar.label_can_status \
       -height 15 \
       -width 85 \
       -anchor w \
       -text "CAN OK" \
       -background lightgreen \
       -image .tick_gif \
       -relief sunken \
       -compound left \
       -padx 2

    pack .veh_if.statusbar.label_can_status -side left -padx 1

    label .veh_if.statusbar.label_logging \
       -height 15 \
       -width 85 \
       -anchor w \
       -text "Not logging" \
       -background salmon \
       -image .cross_gif \
       -relief sunken \
       -compound left \
       -padx 2


    pack .veh_if.statusbar.label_logging -side left -padx 1

    label .veh_if.statusbar.label_sample_rate \
       -height 15 \
       -width 65 \
       -anchor w \
       -textvariable veh_if_config(sample_rate) \
       -image .clock_gif \
       -relief sunken \
       -compound left \
       -padx 2


    pack .veh_if.statusbar.label_sample_rate -side left -padx 1

    label .veh_if.statusbar.label_pdos_sdos \
       -height 15 \
       -width 155 \
       -anchor w \
       -textvariable veh_if_n_comm_types \
       -image .info_gif \
       -relief sunken \
       -compound left \
       -padx 2


    pack .veh_if.statusbar.label_pdos_sdos -side left -padx 1

    # add to window
    grid configure .veh_if.toolbar   -column 0 -row 0 -sticky nsew -padx 2 -pady 2 -columnspan 2
    grid configure .veh_if.io        -column 0 -row 1 -sticky news -padx 2 -pady 2 -columnspan 2 -rowspan 3
    grid configure .veh_if.io.di     -column 0 -row 1 -sticky news -padx 5 -pady 5
    grid configure .veh_if.io.ai     -column 1 -row 1 -sticky news -padx 5 -pady 5
    grid configure .veh_if.io.do     -column 0 -row 2 -sticky news -padx 5 -pady 5
    grid configure .veh_if.io.ao     -column 1 -row 2 -sticky news -padx 5 -pady 5
    grid configure .veh_if.io.watch  -column 0 -row 3 -sticky news -padx 5 -pady 5 -columnspan 2
    grid configure .veh_if.io.nodes  -column 0 -row 4 -sticky news -padx 5 -pady 5 -columnspan 2
    grid configure .veh_if.chart     -column 0 -row 5 -sticky nsew -padx 5 -pady 5 -columnspan 2
    grid configure .veh_if.logging   -column 0 -row 7 -sticky nsw  -padx 5 -pady 5 -columnspan 2
    grid configure .veh_if.statusbar -column 0 -row 8 -sticky nsew -padx 2 -pady 2 -columnspan 2
    
    return
}


#script starts here


# check that vehicle I/O file exists. If not, generate a sample file automatically.
# set veh_io_filename "veh_io.cfg"
# if {![file exists $veh_io_filename]} {
#     dvtConPuts stderr "Unable to locate vehicle I/O file ($veh_io_filename)."
#     
#     # save current settings to file
#     if {[catch {open $veh_io_filename w} fid]} {
#         dvtConPuts stderr "Unable to generate sample vehicle I/O file. Vehicle Interface cannot execute."
#         return
#     } else {
#         puts $fid $veh_io_file_sample_contents
#         close $fid
# 
#         dvtConPuts "Sample file has been automatically generated. See $veh_io_filename."
#     }
# }

# check that CANbus is active before displaying the dialog box
if {!$can_available} {
    dvtConPuts stderr "Vehicle Interface requires CANbus to be available"
    return
}

# set default values for configuration, then source configuration file
set config_filename "veh_if.cfg"
set veh_if_config(sample_rate)              10000
set veh_if_config(logging_file_opt)         0
set veh_if_config(logging_incr_file_name)   1
set veh_if_config(logging_fname)            "log.txt"
catch {source $config_filename}

# initialise variables
array unset veh_pdos
array unset veh_sdos
set bad_can_comms 0

# draw main window
veh_if_gen_window

# re-source object_data.tcl to automatically pick up any updates, especially those just made for temporary debug purposes
if {[catch {source object_data.tcl} r]} {
    dvtConPuts stderr "Unable to source object_data.tcl. Reason: $r"
    return
}

# source auto configuration setup script to configure interface for all configured TPDOs.
if {[catch {source veh_if_cfg.tcl} r]} {
    dvtConPuts stderr "Unable to source veh_if_cfg.tcl. Reason: $r"
    return
}

# draw chart to show legend
veh_if_draw_chart

# start the polling
veh_if_poll

return
