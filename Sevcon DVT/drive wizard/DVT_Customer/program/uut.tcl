###############################################################################
# (C) COPYRIGHT Sevcon Limited 2003
# 
# CCL Project Reference C6944 - Magpie
# 
# FILE
#     $ProjectName:DVT$
#     $Revision:1.131$
#     $Author:ceh$
# 
# ORIGINAL AUTHOR
#     Chris Hauxwell
# 
# DESCRIPTION
#     General Tcl scripts to perform common tasks on the unit under test (UUT)
#     This file is shared with the customer version of the DVT. Only put procs
#     in here which are safe for use in the customer DVT. Put Sevcon engineering
#     only procs into uut_engineering.tcl
# 
# REFERENCES
# 
# MODIFICATION HISTORY
#     $Log:  28485: uut.tcl 
# 
#     Rev 1.131    21/10/2008 16:30:44  ceh
#  Split uut.tcl into this file and uut_engineering.tcl. This is so uut.tcl can
#  now be shared with the customer DVT. Only scripts appropriate for use with
#  the customer should be put into this file now.
# 
#     Rev 1.130    17/10/2008 08:43:56  ceh
#  Check for end of tpdos when when reading PDO configuration object in
#  map_in_tpdo
# 
#     Rev 1.129    16/10/2008 12:07:12  cmp
#  Corrected bug in af which does meant that it misinterpreted -4.20 as 4.20
# 
#     Rev 1.128    15/10/2008 14:43:42  ceh
#  Added new proc to read operational monitor via the domain object. Useful for
#  software which doesn't support the standard monitored items.
# 
#     Rev 1.127    15/10/2008 11:08:56  ceh
#  When checking for unused TPDOs in map_in_tpdo, also check for disabled COBIDs.
# 
#     Rev 1.126    15/10/2008 10:47:44  cmp
#  correct format of watchdog dump data file name (so that it sorts on year
#  first) added formatted dump data to output
# 
#     Rev 1.125    18/09/2008 17:03:20  ceh
#  Inhibit error from login if there is no password object. Also fixed bug in
#  CAN CLI window function
# 
#     Rev 1.124    18/09/2008 16:41:44  ceh
#  Added auto login feature. To use just call enable_auto_login from personal.tcl
# 
#     Rev 1.123    12/08/2008 11:39:40  ceh
#  Added check_dsp_params proc. This peeks out the params_list tables to check
#  if all required items have been received. See help for more information.
# 
#     Rev 1.122    16/07/2008 17:26:54  ps
#  Added proc to allow quick configuration for different power frame ratings
# 
#     Rev 1.121    04/07/2008 14:23:10  ps
#  Added proc allowing all logs to be cleared
# 
#     Rev 1.120    16/06/2008 13:32:34  cmp
#  correct operation of peek16_fmt with formats other than 8.8
# 
#     Rev 1.119    04/06/2008 10:41:28  cmp
#  limit gen4 detailed trace capture to 904 items
# 
#     Rev 1.118    17/03/2008 16:53:00  cmp
#  remove debug
# 
#     Rev 1.117    17/03/2008 16:47:22  cmp
#  modify upload_dsp_trace_fast to support 16 bit numbers and allow cli to be
#  disabled
# 
#     Rev 1.116    13/12/2007 09:53:10  ceh
#  Added is_nodeid_valid proc. Similar to check_valid_nodeid but returns result
#  rather than just aborts scripts. Added utility functions to set serial
#  number, hardware version and EEPROM format in first 32-bytes of EEPROM of
#  unit (or just in file if only file is to be updated). See help for more info.
# 
#     Rev 1.115    22/11/2007 15:57:56  ceh
#  Modified init_peek_addresses to also calculate the address of static
#  variables in functions.
# 
#     Rev 1.114    02/11/2007 12:09:04  ps
#  Modified get_hours_count to show all counters if requested
# 
#     Rev 1.113    01/11/2007 12:15:10  ps
#  Update get_event_name - don't crash if non-hex fault ID is passed to proc.
# 
#     Rev 1.112    22/10/2007 14:52:56  ps
#  Don't store <Event ID Unknown> in event_ids.txt
# 
#     Rev 1.111    10/10/2007 11:51:30  ceh
#  Updated init_peek_addresses() so that it can calculate static variables
#  addresses as well as global addresses for the 281X DSPs.
# 
#     Rev 1.110    02/08/2007 11:41:48  cmp
#  updated CANopen general error codes
# 
#     Rev 1.109    24/07/2007 11:58:54  cmp
#  modify apply format to support signned numbers improve argument error
#  checking, improved function help to
# 
#     Rev 1.107    6/7/2007 12:45:28 PM  cmp
#  removed puts from monitor nodes
# 
#     Rev 1.106    01/06/2007 12:16:20  cmp
#  create node attached monitor
# 
#     Rev 1.105    09/05/2007 09:17:14  ps
#  Added node hours to get_hours_count function
# 
#     Rev 1.104    30/04/2007 14:30:12  ps
#  Added pulsing hours count to get_hours_count function.
# 
#     Rev 1.103    23/04/2007 11:28:30  ceh
#  Updated help text to standard format. Deleted unused request line open/closed
#  procs.
# 
#     Rev 1.102    22/03/2007 14:26:16  ps
#  If controller crashes due to hardware watchdog, show call stack of last
#  sucessful watchdog kick.
# 
#     Rev 1.101    19/03/2007 17:11:22  ceh
#  Removed COBID 0x0000 from initialisation code. This is added automatically to
#  the NMT protocol list in canopen.tcl.
# 
#     Rev 1.100    13/03/2007 15:37:44  ps
#  Automatically associate COBIDs 0x0000 and 0x0701-0x077F with NMT protocol.
# 
#     Rev 1.99    13/03/2007 09:11:02  ps
#  Moved EMCY handler into its own module
# 
#     Rev 1.98    12/03/2007 12:53:28  ceh
#  Added fault handler for EMCY protocol messages. Also removed alias
#  descriptions, which are not aliases.
# 
#     Rev 1.97    13/02/2007 17:14:52  ceh
#  Modified upload_dsp_trace_fast to be compatible with Diablo/Chinook software
# 
#     Rev 1.96    08/02/2007 18:32:44  cmp
#  update get operational monitor with 12.4 voltage format
# 
#     Rev 1.95    10/01/2007 14:29:16  cmp
#  Corrected event counter first/lst time seen calculation
# 
#     Rev 1.94    03/01/2007 13:55:02  cmp
#  updated get_event_name to use nodeid instead of assuming node id 1 is present
# 
#     Rev 1.93    30/11/2006 12:19:26  ceh
#  Modified peek procs so that it can handle 2812 host as well
# 
#     Rev 1.92    14/11/2006 17:20:08  ps
#  Added new proc map_in_tpdo for quick mapping of TPDOs for graphing purposes
# 
#     Rev 1.91    08/11/2006 13:40:32  ceh
#  Added module revision registering.
# 
#     Rev 1.90    20/10/2006 16:42:30  ps
#  Improvements to upload_dsp_trace_fast. Now checks for invalid node IDs and
#  checks if node has triggered trace.
# 
#     Rev 1.89    05/10/2006 12:15:58  cmp
#  modified get operational state to read 0x5110, 0 instead of reading heart
#  beats
# 
#     Rev 1.88    29/09/2006 09:24:50  ps
#  Corrected bug in get_fault_log, which previously could only read fault log
#  from node 1.
# 
#     Rev 1.87    29/09/2006 08:32:52  ceh
#  Added \n to clipboard paste for dsp_trace_upload_fast. Also included update
#  call to see that something is happening.
# 
#     Rev 1.86    20/09/2006 10:34:40  ps
#  Added new proc upload_dsp_trace_fast
# 
#     Rev 1.85    31/08/2006 15:33:16  ceh
#  Limit read access level to 5
# 
#     Rev 1.84    29/06/2006 16:50:44  ceh
#  Modified watchdog data capture to output correct information when task
#  watchdog occurs. PC and call stack are not recorded, but inactive critical
#  tasks are.
# 
#     Rev 1.83    22/06/2006 09:22:56  ps
#  Added proc to extract hours counters
# 
#     Rev 1.82    30/05/2006 13:10:40  ceh
#  Updated CLI handling to cope with multiple CLI slaves
# 
#     Rev 1.81    17/05/2006 13:12:36  ps
#  Made improvements to how we signal watchdog data has been logged.
# 
#     Rev 1.80    28/04/2006 13:03:22  ps
#  Reduced timeout for get_operational_state as any test that was using rcy_key
#  was taking too long to run. Changed msg to infomsg in rcy_key so test can
#  continue without having to unlock a locked screen.
# 
#     Rev 1.79    26/04/2006 12:40:50  ps
#  Improved rcy_key. Uses get_operational_state to check if key switch relay is
#  welded.
# 
#     Rev 1.78    19/04/2006 12:26:00  ps
#  Updated to show negative stack data as introduced by ESP102.
# 
#     Rev 1.77    12/04/2006 12:31:12  ps
#  Added global variable watchdog_data_saved to allow other tasks to monitor for
#  watchdog events.
# 
#     Rev 1.76    06/04/2006 10:00:42  ceh
#  Modified can_cli proc so that it doesn't attempt to read the old CLI debug
#  out domain object. CLI response is now output automatically. Modified peek
#  procs for new peek code. can_cli_handler now updates cli_response as well.
# 
#     Rev 1.75    04/04/2006 11:08:24  cmp
#  Added code to handle CAN based CLI output
# 
#     Rev 1.74    03/04/2006 10:08:58  ceh
#  Added decode_event_id
# 
#     Rev 1.73    29/03/2006 15:00:08  ceh
#  Corrected case of error codes in get_abort_code()
# 
#     Rev 1.72    28/03/2006 15:48:32  ceh
#  Added DVT commands to read operational monitors.
# 
#     Rev 1.71    27/03/2006 11:30:14  ceh
#  Add index number to fault log items
# 
#     Rev 1.70    18/03/2006 15:55:56  ps
#  Updated fault log functions to handle service log also.
# 
#     Rev 1.69    10/03/2006 16:39:48  cmp
#  added missing $
# 
#     Rev 1.68    09/03/2006 15:34:44  cmp
#  removed extra " 
# 
#     Rev 1.67    09/03/2006 15:31:38  cmp
#  implemented workaround if code reports incorrect login level during login
# 
#     Rev 1.66    09/03/2006 11:09:28  ps
#  Added functions to read and clear the event log.
# 
#     Rev 1.65    07/03/2006 08:53:36  cmp
#  make emcy timeout global variable
# 
#     Rev 1.64    06/03/2006 17:30:04  ps
#  Correct bug in get_fault_log.
# 
#     Rev 1.63    06/03/2006 17:25:38  ps
#  Changed show_fault_log proc to get_fault_log, and return entire fault log to
#  calling proc.
# 
#     Rev 1.62    06/03/2006 15:47:46  ps
#  Updated flts to use get_event_name
# 
#     Rev 1.61    06/03/2006 15:26:54  ps
#  Added new procs to show and clear fault log. Added new get_event_name proc,
#  which caches event names in local file.
# 
#     Rev 1.60    28/02/2006 17:49:20  ps
#  Added proc locate tpdo to determine COB and bit position an OD item is sent on
# 
#     Rev 1.59    23/02/2006 14:12:24  cmp
#  added get_fault_id proc
# 
#     Rev 1.58    09/02/2006 13:58:14  cmp
#  add task watchdog reason
# 
#     Rev 1.57    30/01/2006 15:45:38  ps
#  Ensure user is logged in when uploading trace
# 
#     Rev 1.56    30/01/2006 15:32:26  ps
#  Added extra waits to peek32 command to improve reliability
# 
#     Rev 1.55    16/01/2006 16:15:28  ceh
#  Attempt log in with new password first since the password algorithm has been
#  enabled in the code.
# 
#     Rev 1.54    14/12/2005 16:47:38  cmp
#  added input validation to user input_box
# 
#     Rev 1.53    07/12/2005 09:51:32  cmp
#  fixed peek16 for peeking host parameters
# 
#     Rev 1.52    02/11/2005 10:17:42  ps
#  Added find_dwork proc to help locate items in DWork register e.g. number of
#  poles.
# 
#     Rev 1.51    10/10/2005 09:53:16  ps
#  Added trace buffer upload function
# 
#     Rev 1.50    01/09/2005 18:09:12  ps
#  Updated peek16 command to use CANopen interface when a node ID is specified
#  as opposed to CAN-CLI. (requires host software update).
# 
#     Rev 1.49    26/07/2005 14:35:16  cmp
#  corrected rounding error in af
# 
#    Rev 1.48    08/06/2005 09:29:40  ceh
#  Corrected login so that log out works correctly.

# 
#    Rev 1.47    23/05/2005 15:54:02  cmp
#  extend timeout for new login 

# 
#    Rev 1.46    11/05/2005 12:37:56  ceh
#  Added new login functionality. If login fails, system now attempts to login
#  using a real password. Watchdog capture function now displays more
#  information from trace buffer. Add can_cli capability to poke functions.

# 
#    Rev 1.45    10/05/2005 08:16:38  cmp
#  added ma#  information from trace buffer. Add can_cli capability to poke functions.

# 
#    Rev 1.45    10/05/2005 08:16:38  cmp
#  added max optional argument to apply format, returns max value of format if
#  third non-"" argument is supplied

# 
#    Rev 1.44    06/05/2005 09:36:20  ceh
#  Fixed watchdog capture bug. No longer shows an error if a new watchdog trace
#  before the previous completed.

# 
#    Rev 1.43    29/04/2005 12:02:08  ceh
#  Display assert line.

# 
#    Rev 1.42    29/04/2005 11:01:18  ceh
#  Added new watchdog reasons.

# 
#    Rev 1.41    14/04/2005 14:02:52  cmp
#  added CAN capability to peek commands

# 
#    Rev 1.40    31/03/2005 18:36:54  cmp
#  corrected range check on apply format

# 
#    Rev 1.39    31/03/2005 17:43:26  cmp
#  make login try 3 times before giving up

# 
#    Rev 1.38    16/03/2005 10:13:58  ceh
#  Added poke commands for host

# 
#    Rev 1.37    21/02/2005 14:32:50  cmp
#  added node id to emcy protocol decode

# 
#    Rev 1.36    09/02/2005 16:38:46  cmp
#  allow a default text to be displayed in the input box this should be moved
#  from uut really....

# 
#    Rev 1.35    20/12/2004 10:56:38  cmp
#  added dir check to watchdog capture

# 
#    Rev 1.34    15/12/2004 14:34:18  ceh
#  Changed set_e2_format proc to set node id to 1. Changed get_nmt_state to look
#  at 0x5110 object which reports the NMT state directly. Added
#  verify_eeprom_block to automatically check blocks of eeprom data in the
#  protected area.

# 
#    Rev 1.33    09/12/2004 10:26:38  cmp
#  correct operation of apply format and remove format

# 
#    Rev 1.32    02/12/2004 09:08:30  cmp
#  force apply format to alway assume its formatting to an int.  This then means
#  that the largest number for 0.16 is 0.499... and 1.15 is 0.999... etc

# 
#    Rev 1.31    30/11/2004 09:09:46  cmp
#  corrected file proc return at first missing file in init_peek_addresses

# 
#    Rev 1.30    29/11/2004 11:54:28  ceh
#  Added Host peek functionality

# 
#    Rev 1.29    19/11/2004 08:52:52  ceh
#  Fixed store so that sdo_timeout is restored after store command is sent.
#  Also, made nodeid default to 1 where possible.

# 
#    Rev 1.28    18/11/2004 09:54:22  cmp
#  moved watchdog capture to uut.tcl

# 
#    Rev 1.27    12/11/2004 13:14:24  ceh
#  Added peek functions

# 
#    Rev 1.26    12/11/2004 09:09:12  cmp
#  added EMCY debug data global output

# 
#    Rev 1.25    27/10/2004 17:04:56  cmp
#  added motor setup funcitons

# 
#    Rev 1.24    21/10/2004 14:25:34  cmp
#  added remove/apply format and config motor setup, need help adding and
#  possibly some refinement !!

# 
#    Rev 1.23    18/10/2004 12:50:16  ceh
#  Added command (get_abort_reason or gar) to return abort reason when CANopen
#  General Abort code is return.

# 
#    Rev 1.22    15/10/2004 09:19:48  cmp
#  added aliases and get_current_faults proc

# 
#    Rev 1.21    17/09/2004 10:15:38  ceh
#  Added can_cli command.

# 
#    Rev 1.20    10/09/2004 10:35:04  cmp
#  1) extended sdo_timeout for store from 5s to20s 
#  2) added emcy_sevcon_fault
#  3) made use of End Of Transmission in clear faults

# 
#    Rev 1.19    02/09/2004 14:51:46  ceh
#  make fault code visible globally

# 
#    Rev 1.18    18/08/2004 08:37:16  ps
#  Added get_cli_response proc.

# 
#    Rev 1.17    13/08/2004 13:20:22  cmp
#  modifed store command to extend sdo_timeout

# 
#    Rev 1.16    06/08/2004 10:38:14  cmp
#  added 8p82V proc which converts 8.8 hex format to a float
#  also added some helptext

# 
#    Rev 1.15    06/08/2004 09:12:36  ps
#  Added proc to check to see if a given fault is set on a given node.

# 
#    Rev 1.14    06/08/2004 09:08:54  cmp
#  Changed subindices in check_status fro checking fault indication to suit
#  latest build of software

# 
#    Rev 1.13    04/08/2004 09:22:56  cmp
#  imprvoved fault detection routines

# 
#    Rev 1.12    28/07/2004 13:23:14  cmp
#  increased delay during cli clear faults to account for new slower cli

# 
#    Rev 1.11    27/07/2004 14:56:32  ceh
#  Changed force_pre_op() so that it logs in / out if necessary.

# 
#    Rev 1.10    22/07/2004 15:08:50  ceh
#  Added store command to write to 0x1010

# 
#    Rev 1.9    15/07/2004 20:44:28  ceh
#  Corrected bug in nmt_state_handler and removed some redundant puts.

# 
#    Rev 1.8    13/07/2004 15:48:34  cmp
#  added 
#  1) fault_clearance via CANbus
#  2) led count check
#  3) generic emcy telegram handler
#  4) generic fault indication check
#  5) drive inhibition check

# 
#    Rev 1.7    06/07/2004 17:24:38  cmp
#  fixed array bug in standown proc

# 
#    Rev 1.6    06/07/2004 16:29:46  ceh
#  Extended force_pre_op proc so that it can handle slaves as well as masters.

# 
#    Rev 1.5    05/07/2004 15:24:08  ceh
#  force_pre_op proc now returns either "OP" or "PRE" when called with ?
#  argument.

# 
#    Rev 1.4    05/07/2004 15:20:58  cmp
#  added 
#  1) standown util which deactivates all the uut digital io 
#  2) msg which displays a user message

# 
#    Rev 1.3    02/07/2004 12:39:04  cmp
#  added helptext and recylcle key command

# 
#    Rev 1.2    18/06/2004 14:47:00  cmp
#  added nvm format bytes setup

# 
#    Rev 1.1    11/06/2004 15:17:56  cmp
#  made existing functions return something if the pass or fail and use the
#  error Tcl proc if therers a critical problem
#  Also added wait and clear faults

# 
#    Rev 1.0    19/05/2004 21:37:10  ceh
#  New script for general controller related scripts.

# 
# 
###############################################################################

# register module
register DVT [info script] {$Revision:1.131$}



#
# NAME
#   login 
# 
# DESCRIPTION
#   Customer specific version of the login script. This calls customer_login
#   to actually perform the logging in action.
#
#   NOTE: DO NOT ADD GENERAL LOGIN SCRIPT HERE
#
# RETURNS/MODIFIES
#   Success or error message
#
set helptext(login) "Logs into the controller."
set helptexta(login) "lg"
set helptextd(login) "
SYNOPSIS
    login ?node_id? ?level? ?userid?
    
DESCRIPTION
Logs into a device. node_id defaults to 1 if not specified. level defaults to 4 (OEM access). 
If set to ? displays current access level. Setting to 0 logs out. userid is the user id. Defaults to 0.
"

interp alias {} lg {} login

proc login {{node_id 1} {level 4} {userid 0}} {
    
    global debug_canopen sdo_timeout customer_passwords
    
    # check that passwords have been defined in personal.tcl
    if {![info exists customer_passwords]} {
        dvtConPuts stderr "Error. No customer passwords defined."
        return Error
    }
        
    set old_timeout [set sdo_timeout]
    set sdo_timeout 2000
    
    # check node_id
    check_valid_nodeid $node_id
    
    # check level
    if {[string equal $level "?"]} {
        set lev [sdo_rnx $node_id 0x5000 1]
        dvtConPuts "Access Level: $lev"
        return $lev
    } elseif {![string is integer $level]} {
        dvtConPuts stderr "level must be an integer"
        return Error
    } elseif {[expr (($level < 0) || ($level > 4))]} {
        dvtConPuts stderr "level must be between 0 and 4"
        return Error
    }

    
    # if level is set to 0, then this means we are to log out. Just set
    # password to 0, unless this is a valid password, in which case set it
    # to 1.
    if {$level == 0} {
        set result [sdo_wnx $node_id 0x5000 2 0x0000]
        if {$result == "OK"} {
            set result [sdo_wnx $node_id 0x5000 2 0x0001]
        }
        set result "OK"
    
    } else {
    
        # write the user id first. It will abort since the password is
        # incorrect. But ignore this.
        set userid   [format "0x%04x" [expr $userid]]
        sdo_wnx $node_id 0x5000 3 $userid
    
        # write password
        set password $customer_passwords($level)
        set result [sdo_wnx $node_id 0x5000 2 $password]
        if {$result != "OK"} {
            dvtConPuts stderr "Login failed. SDO write to 0x5000,2 returned $result"            
            return "SDO write to 0x5000,2 returned $result"
        }
    }
    
    # output actual log in level
    set actual_level [sdo_rnx $node_id 0x5000 1]
    if {$actual_level != "0x$level"} {
        dvtConPuts stderr "Login error. SDO read from 0x5000,1 returned $actual_level\nGot this far, assuming logged in OK, be warned though!!"        
        return "OK"
    }
    if { $debug_canopen } { dvtConPuts debug "Logged into node $node_id with access level $level" }
    
    set sdo_timeout $old_timeout
    return $result
}


#
# NAME
#   force_pre_op
# 
# DESCRIPTION
#   Attempts to change the NMT state to either operational or pre-operational. If no
#   arguments are passed it toggles the current state.
#   NOTE: This checks object 0x5800 to see if this node is a master or slave. Master
#         nodes are forced to change state via the 0x2800 object, Slaves are forced
#         to change state via NMT messages. When a Master changes state it also
#         changes all the slave states so this function should not be called for
#         slave devices if there is a master already in the system.
#
# RETURNS/MODIFIES
#   Success or error message
#
set helptext(force_pre_op) "Changes the NMT state to either Operational or Pre-Operational."
set helptexta(force_pre_op) "fpo"
set helptextd(force_pre_op) "
SYNOPSIS
    force_pre_op ?node_id? ?state?

DESCRIPTION
Changes NMT state to Operational or Pre-Operational. node_id defaults to 1 if not specified. 
state can be set to OP, PRE, TOG, ? or nothing. If state is not set, current NMT state is 
toggled. If state ? current NMT state is returned.
"
interp alias {} fpo {} force_pre_op

proc force_pre_op {args} {
    global debug_canopen
    set result "OK"
    
    
    set state "TOG"
    if {[llength $args] == 0} {
        set node_id 1
    } elseif {[llength $args] == 1} {
        set node_id [lindex $args 0]
    } elseif {[llength $args] == 2} {
        set node_id [lindex $args 0]
        set state   [string toupper [lindex $args 1]]
    } else {
        error "Invalid number of arguments. Usage: force_pre_op \[<node_id>\] \[OP|PRE|?\]"
    }

    # check for a valid state
    switch $state {
        "OP" - "PRE" - "TOG" - "?" {}
        default                    {error "state ($state) must be OP, PRE, ? or not specified."}
    }
    

    # check node_id
    check_valid_nodeid $node_id
    
    
    # we need to login in a level 1, if not already logged in.
    set old_access_level [login $node_id ?]
    if {![string is integer $old_access_level]} {
        error "Unable to read current access level. Returned $old_access_level"
    } elseif {($old_access_level < 0) || ($old_access_level > 5)} {
        error "Current access level invalid. Returned $old_access_level"
    }
    
    if {$old_access_level == 0} {
        if {[set r [login $node_id 1]] != "OK" } { error "failed login: $r"}
    }
    
    # check if this is a master or slave node
    set master [sdo_rnx $node_id 0x5800 0]
    if {($master != "0x00") && ($master != "0x01")} {error "Unable to read Master/Slave configuration. Returned $master"}
    
    if {$master} {
        # master stuff
    
        # return current state if required
        if {$state == "?"} {
            set r [sdo_rnx $node_id 0x2800 0]
            if {$r == "0x00"} {
                set result "OP"
            } elseif {$r == "0x01"} {
                set result "PRE"
            }
        
        } else {
        
            # read the current state
            set actual_state [sdo_rnx $node_id 0x2800 0]
            if {($actual_state != "0x00") && ($actual_state != "0x01")} {
                error "Unable to read current NMT state from 0x2800,0. Read returned $actual_state"
            }

            if {($actual_state == "0x00") && ($state == "OP")} {
                dvtConPuts debug "Already in operational"
            
            } elseif {($actual_state == "0x01") && ($state == "PRE")} {
                dvtConPuts debug "Already in pre-operational"
            
            } else {
                # we've filtered out changes to the same state, so just invert actual_state and write it back.
                if {[expr $actual_state == 0x00]} {
                    set actual_state 0x01
                } else {
                    set actual_state 0x00
                }

                set result [sdo_wnx $node_id 0x2800 0 $actual_state]
                if {$result != "OK"} {
            
                    if {$debug_canopen} {
                        dvtConPuts debug "NMT state change failed. SDO write to 0x2800,0 returned $result"
                    }
                }
             
                if {$debug_canopen} {    
                    if {[expr $actual_state == 0x00]} {
                        dvtConPuts debug "NMT state changed to Operational"
                    } else {
                        dvtConPuts debug "NMT state changed to Pre-Operational"
                    }
                }
            }
        }
        
    } else {
        # slave stuff. 
        
        # get nmt state
        if {[set nmt_state [get_nmt_state $node_id]] == "Error"} {error "Unable to read NMT state"}
        
        # return current state if required
        if {$state == "?"} {
            set result $nmt_state
        
        } else {
            
            # read the current state
            if {$nmt_state == $state} {
                dvtConPuts debug "Already in required state ($state)"
            
            } else {
                # handle toggling
                if {$state == "TOG"} {
                    if {$nmt_state == "OP"} {
                        set state "PRE"
                    } else {
                        set state "OP"
                    }
                }
            
                # send NMT message to change state to that required
                if {$state == "OP"} {
                    can send "0x0000 1 $node_id"
                } elseif {$state == "PRE"} {
                    can send "0x0000 128 $node_id"
                } else {
                    error "Invalid state ($state)"
                }
            
                # wait a while then check the state has changed.
                wait 500
            
                # read back NMT state to ensure it has changed.
                if {[set nmt_state [get_nmt_state $node_id]] == "Error"} {error "Unable to read NMT state"}
            
                if {$nmt_state != $state} {
                    error "NMT state change failed. Expected $state, got $nmt_state."
                }
             
                if {$debug_canopen} {    
                    if {$state == "OP"} {
                        dvtConPuts debug "NMT state changed to Operational"
                    } else {
                        dvtConPuts debug "NMT state changed to Pre-Operational"
                    }
                }
            }
        }
    }

    
    # log out if required
    if {$old_access_level == 0} {
        if {[set r [login $node_id 0]] != "OK" } { error "failed logout: $r"}
    }

    return $result
}


#
# NAME
#   store
# 
# DESCRIPTION
#   Writes to the store object (0x1010,1) to initiate a store command. It requires
#   only one argument, the node id.
#
# RETURNS/MODIFIES
#   Success or error message
#
set helptext(store) "Stores configuration into EEPROM and/or flash."
set helptextd(store) "
SYNOPSIS
    store ?node_id?
    
DESCRIPTION
Stores configuration into EEPROM and/or flash. Most controllers, where possible,
store updates in EEPROM as soon as an object is written to. Sometimes this is not
possible (if parameters are handled by Ixxat library or stored in flash). This 
command will force values to be stored.
"
proc store {{nodeid 1}} {
    global sdo_timeout
    set temp $sdo_timeout
    set sdo_timeout 20000
    set r [sdo_wnx $nodeid 0x1010 1 0x65766173]
    set sdo_timeout $temp
    return $r
}


    
#
# NAME
#   check_valid_nodeid
# 
# DESCRIPTION
#   Checks that the passed node_id is valid. ie:
#       an integer and in the range 1 to 127.
#
# RETURNS/MODIFIES
#
proc check_valid_nodeid {{node_id 1}} {
    # check node_id
    if {![string is integer $node_id]} {
        error "node_id must be an integer"
    } elseif {[expr (($node_id < 1) || ($node_id > 127))]} {
        error "node_id must be between 1 and 127"
    }    
}


    
#
# NAME
#   is_nodeid_valid
# 
# DESCRIPTION
#   Checks that the passed node_id is valid. ie:
#       an integer and in the range 1 to 127.
#
#   Similar to check_valid_nodeid, but this returns an
#   error rather than just throwing an exception which
#   stops any proc from executing.
#
# RETURNS/MODIFIES
#   true if OK, otherwise false
#
proc is_nodeid_valid {node_id} {
    return [expr ![catch {check_valid_nodeid $node_id}]]
}


#
# Wait : is an improved implementation of after xx which doesnt suspend the other tcl processes whilst the
# specified time elapses
#
# performs a delay in mS without blocking peripheral Tcl activity
proc wait { delay } {
    after $delay set t1 1
    vwait t1
}


# Determines if a given fault is active on a given node. Returns 1 if
# the fault can be located in the fault list, else 0.

proc is_fault_set { node_id fault_code } {
    set test_index 0
    
    while { 1 } {
        
        sdo_wnx $node_id 0x5300 2 $test_index 2
        
        set fault_set_value [ sdo_rnx $node_id 0x5300 3 ]
        if { [ catch { expr ( $fault_set_value ) } fault_set ] } {
            return "Error: Can't read fault information"
        } 
        
        if { [ expr ( $fault_set == $fault_code ) ] } {
            return 1
        }
        
        if { [ expr ( $fault_set == 0 ) ] } {
            return 0
        }
        
        incr test_index
    }
    
}
#
# Proc
#   get_operational_state
#
# Description
#  Checks the operational state of a node by looking at the heartbeat message
#
# Returns one of the following:
#  operational bootup preoperational stopped unknown off-bus
proc get_operational_state { node_id } {
    
    # read nmt state from OD    
    catch {expr [sdo_rnx $node_id 0x5110 0]} result
        
    switch -- $result {
        0       { set oper_state bootup         }
        127     { set oper_state preoperational }
        5       { set oper_state operational    }       
        4       { set oper_state stopped        }       
        default { set oper_state off-bus        }
    }
    
    return $oper_state
}


#
# msg
#
# Displays an info ok only message box containing the passed message"
proc msg { txt {type ok}} {
    
    if {$type == "ok"} {
        set i [tk_messageBox -title "Operator Intervention..." \
                 -icon info \
                 -type ok \
                 -message $txt]
    } elseif {$type == "retry"} {
        set i [tk_dialog .dlg "Operator Intervention..."  $txt "" 2 pass retry fail]
        switch $i {
            0 {set i pass}
            1 {set i retry}
            2 {set i fail}
        }                         
             
    } elseif {$type == "cancel"} {
        set i [tk_messageBox -title "Operator Intervention..." \
                 -icon info \
                 -type okcancel \
                 -message $txt]
             
    } else {
        error "invalid message box type should be ok or retry"
    }
    return $i
}


proc input_box { txt {default ""} {type any} } {
    
    global inputtext
    global cmd
    
    set inputtext $default
    set cmd ""
    
    
    # create input window
    toplevel .i
    wm attributes .i -topmost 1 -toolwindow 1
    wm resizable .i 0 0
    wm title .i "User Input Required....."
    
    # place widgets
    label .i.text -text $txt -font {"Arial" 14}
    entry .i.input -textvariable inputtext -width 60
    button .i.ok -text "ok" -command {set cmd ok} -width 10
    button .i.cancel -text "cancel" -command {set cmd cancel} -width 10
    grid .i.text -column 0 -row 0 -sticky w
    grid .i.input -column 0 -row 1 -sticky w
    grid .i.ok -column 0 -row 2 -sticky w -padx 110
    grid .i.cancel -column 0 -row 2 -sticky e -padx 110
    
    # limit what can be typed into the input box based on type
    switch $type {
        xdigit  {.i.input configure -validate key -vcmd {return [string is xdigit  %P]}}
        integer {.i.input configure -validate key -vcmd {return [string is integer %P]}}
        alnum   {.i.input configure -validate key -vcmd {return [string is alnum   %P]}}
        alpha   {.i.input configure -validate key -vcmd {return [string is alpha   %P]}}
        double  {.i.input configure -validate key -vcmd {return [string is double  %P]}}
        hex     {.i.input configure -validate key -vcmd {if {![catch {expr %P}] || ("%P" == "0x") || ("%P" == "{}")} {return 1} else {return 0}}}
    }
    
    focus .i.input
    
    bind .i.input <Return> {
        set cmd ok
    }
    
    bind .i.input <Escape> {
        set cmd cancel
    }
    
  
    # wait for user response
    vwait cmd
    
    #tidy up global variables
    set i $inputtext
    set c $cmd
    unset inputtext
    unset cmd
    
    # remove window and return user reponse
    destroy .i
    switch $c {
        ok {return $i}
        cancel {return "cancel"}
    }
}


proc get_numeric_ip {txt} {
    if {[string is double [set r [input_box $txt]]]} {
        return $r
    } else {
        msg "Please Enter Valid Numeric Input"
        get_numeric_ip $txt
    }
}
    
#
# NAME
#   get_nmt_state (+ nmt_state_handler)
# 
# DESCRIPTION
#   returns the NMT state of the specified node. There are two methods of doing this:
#
#       1. Read object 0x5110,0. This returns the current NMT state, or
#
#       2. If this object does not exist, monitor the heartbeats or node guarding replies. 
#          This function assumes that something else is generating guard messages if node 
#          guarding is enabled.
#
#          The following procedure is used:
#              1. Configure a handler for the nmt control reply (COBID=0x0700+NodeID).
#              2. When a message is received, extract state (in bits 0..6)
#              3. Return state
#
# RETURNS/MODIFIES
#   NMT state or Error if unable to determine state
#
global NMT_STATE_INVALID; set NMT_STATE_INVALID 0xFF
global nmt_state;         set nmt_state         $NMT_STATE_INVALID

proc get_nmt_state {{nodeid 1}} {
    global nmt_state
    global NMT_STATE_INVALID
    
    # check for 0x5110,0
    set nmt_state [format "0x%02X" [sdo_rnx $nodeid 0x5110 0]]
    
    # if object does not exist, attempt to read from heartbeat/node guarding
    if {[catch {expr $nmt_state}]} {
        set nmt_state $NMT_STATE_INVALID
        
        set cobid [format "0x07%02X" $nodeid]
        co_add_cobid_handler $cobid nmt_state_handler
    
        set nmt_state       $NMT_STATE_INVALID
        set start_time_ms   [clock clicks -milliseconds]
        set elapsed_time_ms 0
    
        while {($nmt_state == $NMT_STATE_INVALID) && ($elapsed_time_ms < 5000)} {
            update
            set elapsed_time_ms [expr ([clock clicks -milliseconds] - $start_time_ms)]
        }

        co_del_cobid_handler $cobid
    }

    switch $nmt_state {
        "0x00"  {return "BOOT" }
        "0x04"  {return "STOP" }
        "0x05"  {return "OP"   }
        "0x7F"  {return "PRE"  }
    }
    
    dvtConPuts stderr "Error. Unable to read NMT state (read $nmt_state)"
    return "Error"
}


proc nmt_state_handler {args} {
    global nmt_state
    
    set packet [split $args " {}"]
    
    # mask out toggle bit (bit 7) and assign to NMT state
    set nmt_state [format "0x%02X" [expr ([lindex $packet 8] & 0x7F)]]
}


#
# decode_fault_id
#
#   Disects a fault ID into its constituent parts
proc decode_fault_id { fault_id } {
    
    set event_type [ expr ( ($fault_id & 0xe000) >> 13 ) ]
    set severity   [ expr ( ($fault_id & 0x1c00) >> 10 ) ]
    set flashes    [ expr ( ($fault_id & 0x03c0) >>  6 ) ]
    set id         [ expr ( ($fault_id & 0x003f)       ) ]
    
    if { $event_type != 2 } {
        return "Not a valid fault ID"
    }
    
    return "Level $severity, [ expr $flashes ]FF, ID $id"
}

#
# decode_event_id
#
#   Disects an event ID into its constituent parts
proc decode_event_id { event_id } {
    set save_bit   [ expr ( ($event_id & 0x8000) >> 15 ) ]
    set fifo_id    [ expr ( ($event_id & 0x6000) >> 13 ) ]
    set level      [ expr ( ($event_id & 0x1c00) >> 10 ) ]
    set group      [ expr ( ($event_id & 0x03c0) >>  6 ) ]
    set id         [ expr ( ($event_id & 0x003f)       ) ]

    set fifo       [lindex [list "None" "System" "Faults" "Invalid"] $fifo_id]
    
    return "Fifo: $fifo ($fifo_id), Level: $level, Group: $group, ID: $id, Save: $save_bit"
}


#
# NAME
#   get_current_fault
# 
# DESCRIPTION
#   get current faults from controller in a list
#
# RETURNS/MODIFIES
#      current faults on system
#

set helptext(get_current_faults) "returns a human readable list of faults,  numbers and their priority, active on the uut"
set helptexta(get_current_faults) "flts"
set helptextd(get_current_faults) "
SYNOPSIS
    get_current_faults ?node_id?

DESCRIPTION
Returns a human readable list of current active faults.
"

interp alias {} flts {} get_current_faults

proc get_current_faults {{node_id 1}} {
    
    set no [sdo_rnx $node_id 0x5300 1]
    
    set sevcon_fault ""
    for {set i 0} {$i < $no} {incr i} {
        if {[set r [sdo_wnx $node_id 0x5300 2 [format 0x%04x $i]]] == "OK"} {
            # read currently active fault               
            set sevcon_code [sdo_rnx $node_id 0x5300 3]
            append sevcon_fault "$i : $sevcon_code [get_event_name $sevcon_code $node_id]\n"
        } else {
            error "error reading" 
        }
    }
    return $sevcon_fault
}


#
# NAME
#   get_abort_reason
# 
# DESCRIPTION
#   Reads object 5310h,0 to determine the reason for a general abort CANopen msg.
#
# RETURNS/MODIFIES
#   Reason for abort message
#

interp alias {} gar {} get_abort_reason

set helptext(get_abort_reason) "returns reason for general abort CANopen message"
set helptexta(get_abort_reason) "gar"
set helptextd(get_abort_reason) "
SYNOPSIS
    get_abort_reason ?node_id?

DESCRIPTION
Returns reason for general abort CANopen message (0x80000000)
"

set uut_abort_codes(0x0000) "No abort reason"
set uut_abort_codes(0x0001) "General abort reason"
set uut_abort_codes(0x0002) "Nothing to transmit"
set uut_abort_codes(0x0003) "Invalid service (srvc)"
set uut_abort_codes(0x0004) "Unit is not in pre-operational"
set uut_abort_codes(0x0005) "Unit is not in operational"
set uut_abort_codes(0x0006) "Unit cannot go to pre-operational"
set uut_abort_codes(0x0007) "Unit cannot go to operational"
set uut_abort_codes(0x0008) "Access level is too low"
set uut_abort_codes(0x0009) "Login failed"
set uut_abort_codes(0x000a) "Value too low"
set uut_abort_codes(0x000b) "Value too high"
set uut_abort_codes(0x000c) "Value invalid"
set uut_abort_codes(0x000d) "EEPROM update failed"
set uut_abort_codes(0x000e) "Cannot reset service time"
set uut_abort_codes(0x000f) "Cannot reset log"
set uut_abort_codes(0x0010) "Cannot read log"
set uut_abort_codes(0x0011) "Invalid command ('s''a''v''e') supplied for store command"
set uut_abort_codes(0x0012) "Could not enter Bootloader mode"
set uut_abort_codes(0x0013) "DSP module rejected update"
set uut_abort_codes(0x0014) "GIO module rejected update"
set uut_abort_codes(0x0015) "DCF backdoor write failed"
set uut_abort_codes(0x0016) "Peek failed"
set uut_abort_codes(0x0017) "Cannot write to DSP"
set uut_abort_codes(0x0018) "Cannot read from DSP"
set uut_abort_codes(0x0019) "Peek has timed out."
set uut_abort_codes(0x0020) "XC164 checkcum is not complete, or failed to id memory type"

proc get_abort_reason {{node_id 1}} {
    global uut_abort_codes
    
    set abort_code [sdo_rnx $node_id 0x5310 0]
    
    if {[info exists uut_abort_codes($abort_code)]} {
        return $uut_abort_codes($abort_code)
    }
    
    return "Invalid abort code ($abort_code) returned."
}


#
#  applies fixed point format to a floating point argument
#
#       Usage apply format 12.23422 8.8
#           returns 0x0C3C


set helptext(apply_format) "Apply format converts a floating point number in to a fixed point hex equivalent"
set helptexta(apply_format) "af"
set helptextd(apply_format) "
SYNOPSIS
    apply_format value format

    Where 
        value is a real value less than the format can suppprt
        format is the fixed point scaliing in the form ?s?integer.frac
            - the s is optional and indicates if the formatted output should be a 
              signed number or unsigned number.  This effects the maximum value of 
              which the number can support.

DESCRIPTION
Converts a floating point number into a fixed point hex number with the appropriate format applied.
For example \"af  12.23422  8.8\"  returns 0x0C3b. 
For example \"af -12.23422  8.8\"  returns 0x0C3b. 
For example \"af -12.23422 s8.8\"  returns 0xf3c5. 
"

interp alias {} af {} apply_format

proc apply_format {val f {show_max ""}} {
    
    # check format is of the correct format
    if {![regexp -nocase -- {(s?)(\-?[0-9]+)\.(\-?[0-9]+)} $f dummy signed integer fraction]} {
        error "invalid format supplied must be in the form of ?s?int.frac"
    }
    set length [expr ($fraction + $integer) / 4]    
    
    if {[expr fmod($length,1)] != 0} {
        error "$f gives a length of   [expr ($fraction + $integer)] which is invalid"       
    }

    if {$signed != ""} {
        #This assumes a signed int and therefore one bit of the integer part is a sign bit and therefore cannot be used
        set max [expr pow(2,$integer-1) - 1 + ((pow(2,$fraction) - 1 )/ pow(2,$fraction))]
    } else {
        #This assumes an unsigned int 
        set max [expr pow(2,$integer) - 1 + ((pow(2,$fraction) - 1 )/ pow(2,$fraction))]
        set val [expr abs($val)]
    }
    
    
    if {[expr abs([format %3.12f $val])] > [format %3.12f $max]} {
        error "max value for this format is $max you supplied $val ([format %0.3f [expr $max - abs($val)]] too big !!)"
    }    
    
    if {$show_max != ""} {
        return $max
    }
    
    set mask 0x
    append mask [string repeat f $length]    

    set r [expr int($val*pow(2,$fraction)) & $mask]
    return [format 0x%0[set length]x $r]
}


set helptext(remove_format) "Remove format converts a fixed point hex to its floating point decimal equivalent"
set helptexta(remove_format) "rf"
set helptextd(remove_format) "
SYNOPSIS
    remove_format value format

    Where 
        value is a hex value
        format is the fixed point scaliing in the form ?s?integer.frac
            - the s is optional and indicates if the hex input is a 
              signed number or unsigned number.  This affects the interpretation of 
              the input value and therfore the result


DESCRIPTION
Converts a fixed point hex number into a decimal number with the appropriate format applied.
Usage rf
For example \"rf 0x0C3C  8.8\" returns 12.234375.
For example \"rf 0x0C3C s8.8\" returns 12.234375.
For example \"rf 0xa343  8.8\" returns 163.26171875.
For example \"rf 0xa343 s8.8\" returns -92.73828125.
"

interp alias {} rf {} remove_format

proc remove_format {val f} {
    
    # check format is of the correct format
    if {![regexp -nocase -- {(s?)([0-9]+)\.([0-9]+)} $f dummy signed integer fraction]} {
        error "invalid format supplied must be in the form of ?s?int.frac"
    }

    #determine length of supplied format
    set length [expr $fraction + $integer]    

    if {$signed!="" && [expr $val >= pow(2,$length)/2]} {
        return [expr -1 * (pow(2,$length)-$val)/pow(2,$fraction)]
    } else {
        return [expr $val / pow(2,$fraction)]
    }
}


#
set helptext(locate_tpdo) "Locates TPDO configured to output index and subindex"
set helptextd(locate_tpdo) "
SYNOPSIS
    locate_tpdo node_id index subindex

DESCRIPTION
Locates TPDO configured to output index and subindex
"
proc locate_tpdo { node_id index subindex } {
    
    set test_index 0x1a00
    
    while { 1 } {
        
        # First determine how many sub-indices are used
        set sub_index_count [ sdo_rnx $node_id $test_index 0 ]
        set start_bit_count 0
        
        if { [ string range $sub_index_count 0 1 ] != "0x" } {
            return "Not transmitted"
        }
        
        # Check each TDPO to see if it is sending the data we want
        for { set test_sub_index 1 } { $test_sub_index <= $sub_index_count } { incr test_sub_index } {
            
            set r [ sdo_rnx $node_id $test_index $test_sub_index ]
            
            set i 0x[ string range $r 2 5 ]
            set s 0x[ string range $r 6 7 ]
            set l 0x[ string range $r 8 9 ]
            
            # Found it. Determine COBID, and bit position.
            if { ( $i == $index ) && ( $s == $subindex ) } {
                set cobid [ expr [ sdo_rnx $node_id [ expr ( $test_index + 0x1800 - 0x1a00 ) ] 1 ] ]
                set cobid 0x[ format %x $cobid ]
                return [ list $cobid $start_bit_count [ expr $l ] ]
            }
            
            incr start_bit_count $l
            
        }
        
        incr test_index
    }
    
}

#
#  get_event_name
#
#   Returns the name of an event from file. If event name is not in file, gets
#   it from the controller and saves it to the file.
proc get_event_name { event_id {nodeid 1}} {
    
    if { ![ string is integer $event_id ] } {
        return "Non integer event $event_id"
    }
    
    if { [ string length [ string trim $event_id ] ] == 0 } {
        return "Invalid event ID"
    }
    
    # Convert the number to hex
    set event_id [ format 0x%04x $event_id ]
    
    # First ensure event_id is in the format 0x---- 
    if { [ string length $event_id ] == 4 } {
        set event_id 0x$event_id
    }
    
    # Check event ID is OK
    if { ! [string is integer $event_id] } {
        return "$event_id is not a valid event ID"
    }
    if { ( $event_id < 0x0000 ) || ( $event_id > 0xffff ) } {
        return "$event_id is out of range"
    }
    set event_id [ string tolower $event_id ]
    
    # Open the file
    if { [ file exists "event_ids.txt" ] } {
        set event_file [ open "event_ids.txt" r+ ]
    } else {
        set event_file [ open "event_ids.txt" w+ ]
    }
    
    # Look for the event ID in the file
    while { ![eof $event_file] } {
        
        gets $event_file event_line
        #puts "Reading line $event_line"
                        
        if { [ string range $event_line 0 5 ] == $event_id } {
            #puts "Match found"
            close $event_file
            return [ string range $event_line 7 100 ]
        }
        
    }
    
    # Not found in file. Read from the controller and store in file
    # for later use. Don't store error messages though.
    sdo_wnx $nodeid 0x5610 1 $event_id
    set event_name [ sdo_rns $nodeid 0x5610 2 ]
    if { (!([string range $event_name 0 6] == "Timeout" )) && (!([string range $event_name 0 4] == "Abort" )) && ( $event_name != "<Event ID Unknown>" ) } {
        puts $event_file "$event_id $event_name"
    }
    close $event_file
    return $event_name  
}


#
#  get_fault_log
#
#   Lists all entries in the fault/system log
set helptext(get_fault_log) "Returns the fault/system log for a particular node"
set helptextd(get_fault_log) "
SYNOPSIS
    get_fault_log node_id ?log_type?

DESCRIPTION
Returns the fault/system log for a particular node. log_type is set to \"faults\"
to read the fault log or \"system\" to read the system log. Defaults to faults.
"
proc get_fault_log { node_id { log_type faults } } {
    
    set control_index 0x4110
    set select_index 0x4111
    set output_index 0x4112
    
    if { $log_type == "system" } {
        set control_index 0x4100
        set select_index 0x4101
        set output_index 0x4102
    }
    
    # lg $node_id
    
    set length_of_log [ sdo_rnx $node_id $control_index 2]
    
    set response ""
    
    for { set i 0 } { $i < $length_of_log } { incr i } {
        
        sdo_wnx $node_id $select_index 0 $i 2
        
        set fault_id [ sdo_rnx $node_id $output_index 1 ]
        set hours    [ expr [ sdo_rnx $node_id $output_index 2 ] + [ sdo_rnx $node_id $output_index 3 ] / 240.0 ]
        set db_0     [ sdo_rnx $node_id $output_index 4 ]
        set db_1     [ sdo_rnx $node_id $output_index 5 ]
        set db_2     [ sdo_rnx $node_id $output_index 6 ]
        
        set hours    [ expr ( round ( $hours * 100 ) / 100.0 ) ]
        
        set fault_name [ get_event_name $fault_id $node_id]
        
        append response "[expr $i+1]. $fault_id, $db_0 $db_1 $db_2, $hours - [ decode_fault_id $fault_id ] - $fault_name\n"
        
    }

    return $response    
}


#
#  clear_fault_log
#
#   Clears any fault log entries
set helptext(clear_fault_log) "Clears all faults in the fault log up to a given level"
set helptextd(clear_fault_log) "
SYNOPSIS
    clear_fault_log node_id fault_level ?log_type?

DESCRIPTION
Clears all faults in the fault log up to a given level. fault_level is the fault
severity level to clear up to. It can be in the range 1..5. log_type is set 
to \"faults\" to clear the fault log or \"system\" to clear read the system log. 
Defaults to faults.
"
proc clear_fault_log { node_id fault_level { log_type faults } } { 
    
    set control_index 0x4110
    
    if { $log_type == "system" } {
        set control_index 0x4100
    }

    lg $node_id $fault_level
    sdo_wnx $node_id $control_index 1 0x01
    
}


#
#  clear_all_logs
#
#   Clears any fault log entries
set helptext(clear_all_logs) "Clears all faults, events, counters and timers up to a given level"
set helptextd(clear_all_logs) "
SYNOPSIS
    clear_all_logs node_id fault_level

DESCRIPTION
Clears all faults, events, counters and timers up to a specified level. Level 5 will
completely reset all logged information, and reset all hours counters to zero.
"
proc clear_all_logs { node_id fault_level } {
    
    lg $node_id $fault_level
    
    set counter 0
    
    if { [ sdo_wnx $node_id 0x4000 1 0x01 ] != "OK" } {
        return "Failed to write reset command."
    }

    puts "Clearing logs, please wait..."
        
    while { [ sdo_rnx $node_id 0x4000 1 ] == 0x01 } {
        
        wait 1000
        incr counter
        if { $counter > 30 } {
            return "Timeout while clearing logs."
        }
    }
    
    return "OK" 
}



#
#  get_event_log
#
#   Returns the contents of the event log of a particular node
set helptext(get_event_log) "Returns the contents of the event log of a particular node"
set helptextd(get_event_log) "
SYNOPSIS
    get_event_log node_id

DESCRIPTION
Returns the contents of the event log of a particular node
"
proc get_event_log { node_id } {
    
    set response ""
    
    for { set index 0x4201 } { $index <= 0x420A } { incr index } {
        
        set ev          [ sdo_rnx $node_id $index 1 ]
        set event_id    [ format 0x%04x [ expr ( $ev & 0x7FFF ) ] ]
        set saved       [ expr ( ( $ev & 0x8000 ) == 0x8000 ) ]
        set event_name  [ get_event_name $event_id $node_id]
        set first_hours [ expr ( round ( ([ sdo_rnx $node_id $index 2 ] + [ sdo_rnx $node_id $index 3 ] / 240.0) * 100.0 ) ) / 100.0 ]
        set last_hours  [ expr ( round ( ([ sdo_rnx $node_id $index 4 ] + [ sdo_rnx $node_id $index 5 ] / 240.0) * 100.0 ) ) / 100.0 ]
        set count       [ expr [ sdo_rnx $node_id $index 6 ] ]
        
        if { ( $event_id != 0 ) || ( $count != 0 ) || ( $first_hours != 0 ) || ( $last_hours != 0 ) } {
            if { $saved } {
                append response "0x[ format %4x $index]: $event_id, $first_hours, $last_hours\t$count * $event_name (saved)\n"
            } else {
                append response "0x[ format %4x $index]: $event_id, $first_hours, $last_hours\t$count * $event_name\n"
            }
        }
    }
    
    return $response
}

#
#  clear_event_log
#
#   Clears any event log entries
set helptext(clear_event_log) "Clears all faults in the event log up to a given level"
set helptextd(clear_event_log) "
SYNOPSIS
    clear_event_log node_id fault_level

DESCRIPTION
Clears all events in the event log up to a given level. fault_level is the event
level to clear up to. 
"
proc clear_event_log { node_id fault_level } { 
    lg $node_id $fault_level
    sdo_wnx $node_id 0x4200 1 0x01
    
}


#
#  get_operational_monitor
#
#   Returns the contents of the operational monitor
set helptext(get_operational_monitor) "Returns the contents of the operational monitor"
set helptextd(get_operational_monitor) "
SYNOPSIS
    get_operational_monitor node_id ?mon_id?

DESCRIPTION
Returns the contents of the operational monitor. mon_id specifies if the customer or
Sevcon monitor log should be read. set mon_id to \"s\" or \"sevcon\" for the
Sevcon log, otherwise set to anything else for the customer log.
"
proc get_operational_monitor {node_id {mon_id customer}} {
    
    # select log
    set mon_id [string tolower $mon_id]
    if {($mon_id == "s") || ($mon_id == "sevcon")} {
        set index 0x4301
    } else {
        set index 0x4300
    }
    
    
    # load description table
    set mon_list [list \
        [list 2   "Minimum Battery"     12.4 unsigned] \
        [list 3   "Maximum Battery"     12.4 unsigned] \
        [list 4   "Minimum Capacitor"   12.4 unsigned] \
        [list 5   "Maximum Capacitor"   12.4 unsigned] \
        [list 6   "Minimum I1"          16.0   signed] \
        [list 7   "Maximum I1"          16.0   signed] \
        [list 8   "Minimum I2"          16.0   signed] \
        [list 9   "Maximum I2"          16.0   signed] \
        [list 10  "Reverse Speed"       16.0   signed] \
        [list 11  "Forward Speed"       16.0   signed] \
        [list 12  "Minimum Temperature" 16.0   signed] \
        [list 13  "Maximum Temperature" 16.0   signed] \
    ]
    
    
    # read log from controller
    set response ""
    foreach item $mon_list {
        set sidx [lindex $item 0]
        set name [lindex $item 1]
        set fmt  [lindex $item 2]
        set sign [lindex $item 3]
        
        set value [sdo_rnx $node_id $index $sidx]
        if {[catch {expr $value}]} {
            append response "$name: Error ($value)\n"
        } else {
            if {($sign == "signed") && ($value > 0x7FFF)} {
                set value [expr $value - 0x10000]
            }
            set value [rf $value $fmt]
            
            append response "$name: $value\n"
        }
    }
    
    return $response
}


#
#  get_operational_monitor_domain
#
#   Returns the contents of the operational monitor via the domain objects
set helptext(get_operational_monitor_domain) "Returns the contents of the operational monitor"
set helptextd(get_operational_monitor_domain) "
SYNOPSIS
    get_operational_monitor_domain node_id ?mon_id?

DESCRIPTION
Returns the contents of the operational monitor from the domain (0x5600) object. 
mon_id specifies if the customer or Sevcon monitor log should be read. set mon_id 
to \"s\" or \"sevcon\" for the Sevcon log, otherwise set to anything else for the 
customer log.

NOTE: This method is better for units which do not support the standard set of
      monitored values. (Eg Units with additional motors).
"
proc get_operational_monitor_domain {node_id {mon_id customer}} {
    
    # select log
    set mon_id [string tolower $mon_id]
    if {($mon_id == "s") || ($mon_id == "sevcon")} {
        set subindex 5
    } else {
        set subindex 4
    }
    
    # read product code and check it is read correctly
    set product [sdo_rnx $node_id 0x1018 2]
    if {[catch {expr $product} r]} {
        return [error "ERROR: Unable to read product code. Returned - [sdo_abort_decode $r]"]
    }
    
    # using the product code, work out the structure of the monitor data. Most
    # monitors have Vbat, Vcap, 2*Motor currents, Speed and Temperature. There
    # are some specials though:
    switch [string range $product 0 5] {
        0x0402 - 0x0403 {
            # aichi 24V AC+DC
            # Monitor contains Vbat, Vcap, 2 * motor current, 2 unused monitors (meant to be additional currents),
            # 2 speeds and heatsink temperature
            set mon_list {
                {"Minimum Battery"     12.4 unsigned}
                {"Maximum Battery"     12.4 unsigned}
                {"Minimum Capacitor"   12.4 unsigned}
                {"Maximum Capacitor"   12.4 unsigned}
                {"AC Minimum I1"       16.0   signed}
                {"AC Maximum I1"       16.0   signed}
                {"DC Minimum I2"       16.0   signed}
                {"DC Maximum I2"       16.0   signed}
                {"Unused"              16.0   signed}
                {"Unused"              16.0   signed}
                {"Unused"              16.0   signed}
                {"Unused"              16.0   signed}
                {"Enc 1 Reverse Speed" 16.0   signed}
                {"Enc 1 Forward Speed" 16.0   signed}
                {"Enc 2 Reverse Speed" 16.0   signed}
                {"Enc 2 Forward Speed" 16.0   signed}
                {"Minimum Temperature" 16.0   signed}
                {"Maximum Temperature" 16.0   signed}
            }
        }
        
        default {
            # default monitor data
            # Monitor contains Vbat, Vcap, 2 * motor current, 1 speed and heatsink temperature
            set mon_list {
                {"Minimum Battery"     12.4 unsigned}
                {"Maximum Battery"     12.4 unsigned}
                {"Minimum Capacitor"   12.4 unsigned}
                {"Maximum Capacitor"   12.4 unsigned}
                {"AC Minimum I1"       16.0   signed}
                {"AC Maximum I1"       16.0   signed}
                {"AC Minimum I2"       16.0   signed}
                {"AC Maximum I2"       16.0   signed}
                {"Reverse Speed"       16.0   signed}
                {"Forward Speed"       16.0   signed}
                {"Minimum Temperature" 16.0   signed}
                {"Maximum Temperature" 16.0   signed}
            }
        }
    }
    
    
    # read domain
    set domain [sdo_rn $node_id 0x5600 $subindex]
    
    # search through domain matching it to data. First 4 bytes in domain are just domain length
    set idx 4
    set response ""
    foreach item $mon_list {
        set name  [lindex $item 0]
        set fmt   [lindex $item 1]
        set sign  [lindex $item 2]
        
        set value [merge_hex [lrange $domain $idx [expr $idx+1]]]
        if {[catch {expr $value} r]} {
            append response "$name: Error ($value) - Reason: $r\n"
        } else {
            if {($sign == "signed") && ($value > 0x7FFF)} {
                set value [expr $value - 0x10000]
            }
            set formatted_value [rf $value $fmt]
            
            append response "$name: $formatted_value\n"
        }
        
        incr idx 2
    }
    
    return $response
}

#
#  clear_operational_monitor
#
#   Clears the operational monitors
set helptext(clear_operational_monitor) "Clears the operational monitors"
set helptextd(clear_operational_monitor) "
SYNOPSIS
    clear_operational_monitor node_id ?mon_id?

DESCRIPTION
Clears the contents of the operational monitor. mon_id specifies if the customer or
Sevcon monitor log should be cleared. set mon_id to \"s\" or \"sevcon\" for the
Sevcon log, otherwise set to anything else for the customer log.
"
proc clear_operational_monitor {node_id {mon_id customer}} {

    # select log
    set mon_id [string tolower $mon_id]
    if {($mon_id == "s") || ($mon_id == "sevcon")} {
        return [sdo_wnx $node_id 0x4301 1 0x01]
    } else {
        return [sdo_wnx $node_id 0x4300 1 0x01]
    }
    
}



co_add_protocol_handler nmt monitor_nodes

# node has not produced nmt message for heartbeat_timeout, must have gone off bus
global nodes_present;
set heartbeat_timeout 1000
proc monitor_nodes {args} {
    global nodes_present heartbeat_timeout
    set nodeid [expr [lindex $args 0 4] - 0x700]
    if {($nodeid > 0) && ($nodeid < 127)} {
        # kill previous watch
        catch {after cancel $nodes_present($nodeid)}
        # add node id to global nodes present list
        set nodes_present($nodeid) [after $heartbeat_timeout remove_node [subst $nodeid]]
    }
}

proc remove_node {nodeid} {
    global nodes_present
    catch {unset nodes_present($nodeid)} err
}

#
#
#
set helptext(nodes_detected) "Returns a list of detected nodes"
set helptextd(nodes_detected) "
SYNOPSIS
    nodes_detected

DESCRIPTION
listens out for heartbeat messages and maintains a list of nodes present on the CANbus requires that
0x7xx nmt messages are delcared as part of the nmt protocol to work "

proc nodes_detected {} {
    global nodes_present
    return [array names nodes_present]
}


#
#  Proc to read hours count from espAC
#
set helptext(get_hours_count) "Returns hours counter"
set helptextd(get_hours_count) "
SYNOPSIS
    get_hours_count ?node_id? ?counter?
    
DESCRIPTION
Returns value stored in hours counter. Default is to return key hours of node ID 1. 
counter may be an hours type or an object dictionary index. Valid counters are key,
traction, pump, steer, work or charge.
"

proc get_hours_count {{node_id 1} {counter key}} {
    
    if { $counter== "all" } {
        set counts ""
        foreach k { key traction pump steer work charge pulsing node } {
            append counts "$k - [ get_hours_count $node_id $k ] \n"
        }
        return $counts
    }
    
    set hours 0
    set mins 0
    
    switch -- $counter {
        "key"                           { set counter 0x2781 }
        "traction"                      { set counter 0x2782 }
        "pump"                          { set counter 0x2783 }
        "steer" - "powersteer" - "ps"   { set counter 0x2784 }
        "work"                          { set counter 0x2785 }
        "charge"                        { set counter 0x2786 }
        "pulsing" - "pulse" - "motor1"  { set counter 0x4601 }
        "node"                          { set counter 0x5200 }
    }
    
    if { ( $counter >= 0x2781 && $counter <= 0x2786 ) || $counter == 0x4601 || $counter == 0x5200 } {
        set hours [ sdo_rnx $node_id $counter 1 ]
        set mins  [ sdo_rnx $node_id $counter 2 ]
        return [ expr $hours + ( $mins / 240.0 ) ]
    } else {
        return [ error "Unknown hours counter $counter" ]
    }
    
}



#
# map_in_tpdo
#
# Procedure to find space for an item in a TPDO and maps it in
#

set helptext(map_in_tpdo) "Procedure to find space for an item in a TPDO and maps it in."
set helptextd(map_in_tpdo) "
SYNOPSIS
    map_in_tpdo node_id index sub_index ?cob_id_to_use?

DESCRIPTION
Used to add items to a node's TPDO list. Useful for quickly adding in items to be graphed.
cob_id_to_use allows the item to be mapped to a specific COBID. If not specified, item is
mapped in first big enough space.
"

proc map_in_tpdo { node_id index sub_index {cob_id_to_use 0x7ff} } {
        
    lg $node_id
    
    # Find out the length of the bject we are tryin to map in
    set l [ sdo_rnx $node_id $index $sub_index ]
    if { [ string is integer $l ] == 0 } {
        return [ error "Could not map in object $index $sub_index, got $l when trying to determine length of object - [ sdo_abort_decode $l ]" ]
    }
    
    set length_of_object [ expr ( ( [ string length $l ] - 2 ) * 4 ) ]
    # puts "Size of $index $sub_index is $length_of_object bits"
    
    # Look through each TPDO for space to map in the object. If it is already mapped
    # in then return OK straight away.
    
    set current_tpdo_config_index  0x1800
    set current_tpdo_mapping_index 0x1a00
    set tpdo_index_to_use  0x0000
    
    set end_of_tpdos 0
    set cobid_disabled 0
    
    while { $end_of_tpdos == 0 } {
        
        # read communication object and check COBID is not disabled
        set tpdo_cobid [ sdo_rnx $node_id $current_tpdo_config_index 1 ]
        if { $tpdo_cobid == "Abort 0x06020000" } {
            set end_of_tpdos 1
            set last_tpdo_index [ expr ( $current_tpdo_mapping_index - 1 ) ]
        
        } else {
            if {[catch {expr $tpdo_cobid} r]} {
                return "ERROR: Unable to read cobid from communication object ([format {0x%04x} $current_tpdo_config_index]). Returned: $r"
            }
        
        
            # if COBID bit 31 is set to disable TPDO, then we can ignore the mapping since it doesn't matter. We will overwrite it anyway
            if {[expr $tpdo_cobid & 0x80000000] == 0} {
        
                # puts "Checking for space in object $current_tpdo_mapping_index"
        
                set n_maps [ sdo_rnx $node_id $current_tpdo_mapping_index 0 ]
        
                if { [ string is integer $n_maps ] } {
            
                    set size_of_this_tpdo 0
            
                    for { set x 1 } { $x <= $n_maps } { incr x } {
                
                        set a [ sdo_rnx $node_id $current_tpdo_mapping_index $x ]
                        if { [ string is integer $a ] == 0 } {
                            return [ error "Problems interrogating existing TPDO $current_tpdo_mapping_index $x, got $a -  - [ sdo_abort_decode $a ]" ]
                        } else {
                            set t_index     "0x[ string range $a 2 5 ]"
                            set t_sub_index "0x[ string range $a 6 7 ]"
                            set t_length    "0x[ string range $a 8 9 ]"
                    
                            # puts "Found a TPDO for object $t_index $t_sub_index, size $t_length bits"
                    
                            if { [ expr ( $t_index == $index && $t_sub_index == $sub_index ) ] == 1 } {
                                # Already mapped in
                                return "OK - already mapped at object [ af $current_tpdo_mapping_index 16.0 ] $x"
                            }
                    
                            incr size_of_this_tpdo $t_length
                    
                        }
                
                    }
            
                    # puts "This TPDO transmits $size_of_this_tpdo bits"

                    if { $tpdo_index_to_use == "0x0000" } {            
                        if { [ expr ( $size_of_this_tpdo + $length_of_object ) <= 64 ] } {
                            set r [ sdo_rnx $node_id $current_tpdo_mapping_index [ expr ( [ sdo_rnx $node_id $current_tpdo_mapping_index 0 ] ) + 1 ] ]
                            if { [ string is integer $r ] } {
                                set tpdo_index_to_use $current_tpdo_mapping_index
                            }
                        }
                    }
            
                }  else {
                    if { $n_maps == "Abort 0x06020000" } {
                        set end_of_tpdos 1
                        set last_tpdo_index [ expr ( $current_tpdo_mapping_index - 1 ) ]
                        # puts "Ooops - found end of TPDOs"
                    } else {
                        # puts "Something went wrong here"
                        return [ error "Problems reading existing TPDOs, got $n_maps -  - [ sdo_abort_decode $n_maps ]" ]                
                    }
                }
            } else {
                # set index to use to disabled TPDO.
                if { $tpdo_index_to_use == "0x0000" } {            
                    set tpdo_index_to_use $current_tpdo_mapping_index
                    set cobid_disabled 1
                }
                break
            }
    
            incr current_tpdo_mapping_index
            incr current_tpdo_config_index
        }
    }
    
    
    if { $tpdo_index_to_use == "0x0000" } {
        return [ error "No space in TPDOs objects to map $index $sub_index" ]
    }
    
    # sort out map index and subindex. If COBID is disabled, always use sub 1 of map
    set map_index [format "0x%04x" $tpdo_index_to_use]
    if {$cobid_disabled} {
        set map_subindex 1
    } else {
        set map_subindex [ sdo_rnx $node_id $tpdo_index_to_use 0 ]
        incr map_subindex
    }
    set map_item [ expr ( ( $index * 0x10000 ) | ( $sub_index * 0x100 ) | ( $length_of_object ) ) ]
    # puts "Adding TPDO for $index $sub_index length $length_of_object (code $map_item) at $map_index $map_subindex"    
    
    set additional ""
    
    # Set up the mapping
    set r [ sdo_wnx $node_id $map_index 0 0x00 ]
    if { $r != "OK" } { return [ error "Problem setting $map_index 0 to 0 in order to map $index $sub_index, got $r -  - [ sdo_abort_decode $r ]" ] }
    set r [ sdo_wnx $node_id $map_index $map_subindex $map_item 4 ]
    if { $r != "OK" } { return [ error "Problem setting $map_index $map_subindex to $map_item in order to map $index $sub_index, got $r -  - [ sdo_abort_decode $r ]" ] }
    set r [ sdo_wnx $node_id $map_index 0 $map_subindex 1 ]
    if { $r != "OK" } { return [ error "Problem setting $map_index 0 to $map_subindex in order to map $index $sub_index, got $r -  - [ sdo_abort_decode $r ]" ] }
    
    # Check the TPDO parameters
    set param_index [ expr $map_index - 0x200 ]
    set cob_id [ sdo_rnx $node_id $param_index 1 ]
    if { $cob_id < 0x0100 || $cob_id > 0x5ff } {
        if { $cob_id_to_use == 0x7ff } {        
            set new_cobid [ get_unused_cobid ]
        } else {
            set new_cobid $cob_id_to_use
        }
        set r [ sdo_wnx $node_id $param_index 1 [ expr $new_cobid ] 4 ]
        if { $r != "OK" } { return [ error "Problem setting $param_index 1 to $new_cobid in order to map $index $sub_index, got $r - [ sdo_abort_decode $r ]" ] }
        set additional "Also set new COBID for this object to be $new_cobid. "
    }
    
    if { [ sdo_rnx $node_id $param_index 2 ] != 0x01 } {
        set r [ sdo_wnx $node_id $param_index 2 0x01 ]
        if { $r != "OK" } { return [ error "Problem setting transmission type, got $r - [ sdo_abort_decode $r ]" ] }
    }
    
    if { [ sdo_rnx $node_id $param_index 3 ] != 0x00 } {
        set r [ sdo_wnx $node_id $param_index 3 0x00 ]
        if { $r != "OK" } { return [ error "Problem setting inhibit time, got $r - [ sdo_abort_decode $r ]" ] }
    }
    
    if { [ sdo_rnx $node_id $param_index 5 ] != 0x00 } {
        set r [ sdo_wnx $node_id $param_index 5 0x00 ]
        if { $r != "OK" } { return [ error "Problem setting event timer, got $r - [ sdo_abort_decode $r ]" ] }
    }
    
    
    # puts [ sdo_ro $node_id $map_index ]
    
    return "OK - mapped into object [ af $map_index 16.0 ] $map_subindex but may need you to execute store $node_id. $additional"
}

proc get_unused_cobid { } {
    
    # Avoid COBIDs with letters in them (graphing subsystem doesn't like them)
    set has_letters 1
    while { $has_letters } {
        
        set proposed_cobid [ af [ expr ( round ( rand() * 0x380 ) + 0x100 ) ] 16.0 ]
        
        set has_letters 0
        foreach n { 2 3 4 5 } {
            if { [ string is alpha [ string range $proposed_cobid $n $n ] ] } {
                set has_letters 1
            }
        }
    }
    
    foreach node_id { 1 2 3 4 64 } {
        
        set r [ sdo_rnx $node_id 0x1018 0 ]
        
        if { $r != "Timeout" } {
            
            # Check TPDOs
            set index 0x1800
            set checking 1
            
            while { $checking } {
                set r [ sdo_rnx $node_id $index 1 ]
                
                if { [ string is integer $r ] } {
                    if { $r == $proposed_cobid } {
                        # puts "$proposed_cobid is being used."
                        return [ get_unused_cobid ]
                    }
                } else {
                    set checking 0
                }
                incr index
            }
                        
        }
        
    }
    
    return $proposed_cobid
}



# this proc uploads all fault IDs supported by the controller and their associated
# names (as strings). It then creates a list which can be used by uut_emcy_handler proc

set helptext(upload_fault_id_list) "Uploads all fault IDs and descriptions from a controller."
set helptextd(upload_fault_id_list) "upload_fault_id_list node_id                    Uploads all fault IDs and descriptions from a controller and writes to Information window."

proc upload_fault_id_list {{nodeid 1}} {
    # upload event id list from controller. All IDs above 0x4000 are fault IDs.
    set events [sdo_rn $nodeid 0x5600 10]
    
    # check for a valid return
    if {[catch {expr [lindex $events 0]}]} {
        dvtConPuts stderr "Error. Unable to read event list. Returned $events"
        return Error
    }
    
    # events are returned as a domain. The 1st 4 bytes are the number of data bytes
    # returned by sdo_rn. Use this to calculate the number of events
    set n_events [expr [merge_hex [lrange $events 0 3]] / 2]
    
    if {$n_events != [expr ([llength $events] - 4) / 2]} {
        dvtConPuts stderr "Error. Number of events ($n_events) does not match that expected ([expr ([llength $events] - 4) / 2])."
        return Error
    }
    
    set byte_index 4
    for {set i 0} {$i < $n_events} {incr i} {
        set event_id [format "0x%04x" [expr ([lindex $events [expr $byte_index + 1]]*256) + [lindex $events $byte_index]]]
        
        if {$event_id >= 0x4000} {
            # upload string associate with the event id
            sdo_wnx $nodeid 0x5610 1 $event_id
            infomsg "set uut_faults($event_id) \"[sdo_rns $nodeid 0x5610 2]\""
        }
        
        incr byte_index 2
    }
    
    return OK
}

#
# Set some standard network management COB-IDs
#
for { set x 0x701 } { $x <= 0x77f } { incr x } { 
    co_map_cobid_to_protocol nmt $x 
}

