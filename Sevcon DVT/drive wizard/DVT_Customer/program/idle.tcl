###############################################################################
# (C) COPYRIGHT Sevcon Limited 2003
# 
# CCL Project Reference C6944 - Magpie
# 
# FILE
#     $ProjectName:DVT$
#     $Revision:1.15$
#     $Author:ceh$
# 
# ORIGINAL AUTHOR
#     Martin Cooper
# 
# DESCRIPTION
#     Controls the idle actions for CAN and the CLI.  The can polling
#     proc is called regularly to retrieve received messages from the
#     FIFO in the DLL.  Characters arriving on the serial port are
#     sent to the CLI window and appended to the response string.
# 
# REFERENCES
#     C6944-TM-171
# 
# MODIFICATION HISTORY
#     $Log:  26182: idle.tcl 
# 
#     Rev 1.15    08/11/2006 13:40:30  ceh
#  Added module revision registering.
# 
#     Rev 1.14    30/05/2006 13:10:40  ceh
#  Updated CLI handling to cope with multiple CLI slaves
# 
#    Rev 1.13    10/09/2004 10:32:30  cmp
#  added End Of Transmission check for cli operations

# 
#    Rev 1.12    16/07/2004 10:58:04  cmp
#  removed daq availble in daq idle poll

# 
#    Rev 1.11    09/07/2004 12:45:04  cmp
#  modify date format to sort properly

# 
#    Rev 1.10    18/06/2004 14:44:14  cmp
#  added can and cli window maintainence to prevent to much data accumulating in
#  these windows

# 
#    Rev 1.9    14/05/2004 11:02:16  cmp
#  only update daq info if checked in the main menu

# 
#    Rev 1.8    12/05/2004 21:11:00  cmp
#  added daqidlepoll which updates daq reads to a window every 300mS

# 
#    Rev 1.7    11/05/2004 21:02:44  cmp
#  use can_available variable

# 
#    Rev 1.6    11/05/2004 08:44:10  cmp
#  fixed can available detection

# 
#    Rev 1.5    07/05/2004 10:46:20  cmp
#  commented out if { info exists can_available } as it was preventing co_poll
#  being called ??

# 
#    Rev 1.4    05/05/2004 20:36:22  cmp
#  updated to facilitate io mapping

# 
#    Rev 1.3    04/05/2004 19:56:00  cmp
#  dvt now starts if CAN adapter is not present, also exits properly negating
#  the need to "kill" wish84.exe after exit

# 
#    Rev 1.2    25/04/04 22:37:22  mdc
#  CANopen functionality moved to canopen.tcl.

# 
#    Rev 1.0    06/04/2004 13:44:20  mdc
#  Initial CLI read function added, and the shell for the CAN poll proc.

# 
# 
###############################################################################

# register module
register DVT [info script] {$Revision:1.15$}

set idlepoll_period 10
set daqidlepoll_period 300

# initialise End of Transmission to True
set EOT 1


# This is first called after the GUI has initialised
proc idlepoll {} {
   global idlepoll_period idlepollid can_available 

   # check that the can adapter has successfully initialised before calling can open 
   # functions
   if { $can_available } {
       co_poll
   }
   set idlepollid [after $idlepoll_period idlepoll]
}


#daq idle poll
proc daqidlepoll {} {
   global daqidlepoll_period daq_available daqidlepollid do_daq_poll

   if { $do_daq_poll} {
       daq_poll
   }
   
   set daqidlepollid [after $daqidlepoll_period daqidlepoll]
}


# This is called when characters arrive on the serial port
proc cli_read {} {
   global com_chid
   global cli_response EOT
   
   if {[catch {read $com_chid} r]} {
      return
   }

   if {[set pos [string first [format %c 0x04] $r]] != -1} {
       set EOT 1
       set r [string replace $r $pos $pos ""]
   }
   climsg -nonewline $r in           
   
   append cli_response $r
}


# check can and cli windows are not getting too big 
# if they are then save off the oldest half to the harddrive.
proc win_size_poll {} {
    set cli_full 10000
    set can_full 10000
    
    set can_size [llength [.can.text get 1.0 "end -1 chars"]]
    
    set fn "../can_cli_dump"
    if {![file isdirectory $fn]} {
        file mkdir $fn    
    }


    # save each CLI window into a different section of the file
    foreach tab [.cli.tnb tab names] {
        # All windows are labelled "Node <nodeid>"
        set win_name [.cli.tnb tab cget $tab -text]
        set nodeid [string range $win_name 5 end]

        set cli_size [llength [.cli.tnb.win[set nodeid].text get 1.0 "end -1 chars"]]
        
        if {$cli_size > $cli_full} {
            set fn "../can_cli_dump/cli[set nodeid]_[clock format [clock seconds] -format {%d%m%y_%H%M%S}].txt"
        
            set cli_dump [.cli.tnb.win[set nodeid].text get 1.0 [expr $cli_size / 2].0]
            .cli.tnb.win[set nodeid].text delete 1.0 [expr $cli_size / 2].0    
            
            set fileid [open $fn w]
            seek $fileid 0 start
            fputs $fileid $cli_dump
            close $fileid
        }
    }
    
    if {$can_size > $can_full} {
        set fn "../can_cli_dump/can_[clock format [clock seconds] -format {%d%m%y_%H%M%S}].txt"
        set can_dump [.can.text get 1.0 [expr $can_size / 2].0]
        .can.text delete 1.0 [expr $can_size / 2].0 
        set fileid [open $fn w]
        seek $fileid 0 start
        fputs $fileid $can_dump
        close $fileid
    }
    after 1000 win_size_poll
}

