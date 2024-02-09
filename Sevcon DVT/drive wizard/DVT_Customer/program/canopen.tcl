###############################################################################
# (C) COPYRIGHT Sevcon Limited 2004
# 
# CCL Project Reference C6944 - Magpie
# 
# FILE
#     $ProjectName:DVT$
#     $Revision:1.46$
#     $Author:cmp$
# 
# ORIGINAL AUTHOR
#     Martin Cooper
# 
# DESCRIPTION
#     Simple CANopen functionality.
#
#
#     ==================
#     RECEIVE PROCESSING
#     ==================
#
#     When a CAN message arrives in the DLL, it is read by co_poll.  There are
#     then three concurrent models for processing the message:
#
#     1. Processing by CANopen protocol
#     =================================
#     The user (typically a test script) tells this CANopen code which COB_IDs
#     are used for which CANopen protocols.  Using this information, the protocol
#     (SYNC, EMCY, PDO, SDO etc) of the received message can be determined.  A
#     default handler for that protocol is called.  If the user has specified any
#     user handlers for that protocol, they are called too.
#
#     If a COB_ID arrives which cannot be decoded into a protocol, a default
#     and user handlers, if specified, are called.
#
#     The default handlers for each protocol are contained within this file.  Their
#     action is simple - for example, to insert the message into the CAN window in
#     the GUI.
#
#     User-specific handlers may take any test-specific action required.  These are
#     specified in the usr_handler array.
#
#     The parameter to each handler is the message as pulled off the FIFO.  This
#     is a string in the following format:
#     
#     index  description      type
#     0      items remaining  an integer
#     1      timestamp        an integer
#     2      RTR              either 0 or 1
#     3      overrun          either 0 or 1
#     4      COB_ID           in the format 0xNNNN 
#     5      data             an opening brace, each byte in the format 0xNN, and a closing brace
#
#     It is possible to have more than one user handler for each protocol.
#
#
#     2. Processing by instantiated simulated slaves
#     ==============================================
#     Each received CAN message is forwarded to any simulated slaves which have been
#     created. The slaves implement simple Shiroko functionality.  A slaves is created
#     with "co_slave create ...".  A slaves is "turned on" with "co_slave init".  The status
#     of the slaves is reported by co_status.
#
#
#     3. Processing by COB-ID
#     =======================
#     The user can specify handlers to be called on a per-COB-ID basis.  The COB-ID handlers 
#     are stored in the cobid_handlers list.
#
#     Handlers are associated with COB-IDs with the co_add_cobid_handler proc, and deleted
#     with the co_del_cobid_handler proc.
#
#     It is possible to have more than one user handler for each protocol.
#
#
#     ================
#     SIMULATED SLAVES
#     ================
#
#     Slaves are created with "co_slave create...".  They have default node IDs and fixed TPDO
#     and RPDO mapping.  A slave will update its outputs if the correct RPDO is received.
#     The slave can be prompted into sending a TPDO in two ways:
#     1. Interactively, by pressing the "tx" button in the slave's window
#     2. In a script, by calling co_slave tx_tpdo <slave> <tpdo_num>
#
#     The slave's inputs can be changed in one of two ways:
#     1. Interactively, by editing the fields in the window.
#     2. In a script, by calling co_slave with appropriate parameters.
#
#
#     ===========================
#     "PUBLIC" PROCS IN THIS FILE
#     ===========================
#
#     "Public" procs in this file are prefixed with "co_" (CANopen).  They are:
#
#     co_status                  Prints the current status of the CANopen software, including the
#                                COB_ID-to-Protocol mappings, the names of currently specified
#                                user handlers for each protocol, and the state of any slaves.
#
#     co_poll                    Polls the receive FIFO and initiates all the receive action.
#
#     co_reset                   Sets up default COB_IDs for each protocol and removes all user
#                                handlers and slaves.  This proc is called when this file is loaded.
#
#     co_slave                   Creates and manipulates slaves.
#
#     co_add_cobid_handler       Associates user-handlers with a COB-ID.
#
#     co_del_cobid_handler       Removes the association between a COB-ID and a user-handler.
#
#     co_add_protocol_handler    Associates user-handlers with a protocol.
#
#     co_del_protocol_handler    Removes the association between a protocol and a user-handler.
#
#     co_map_cobid_to_protocol   Maps a COB-ID to a protocol.
#
#     co_unmap_cobid_to_protocol Unmaps a COB-ID to a protocol.
#
#
# 
# REFERENCES
#     C6944-TM-191, C6944-TM-171
# 
# MODIFICATION HISTORY
#     $Log:  26882: canopen.tcl 
# 
#     Rev 1.46    28/08/2008 11:46:10  cmp
#  added new slave array member slave_x(nmt_chng_holdoff ) to delay nmt state
#  changes (no gui controls tho)
# 
#     Rev 1.45    23/06/2008 12:20:14  ceh
#  Ensure after task is correctly cancelled, when slave is destroyed. Added
#  error register byte to EMCY message (leave as 0x00 though).
# 
#     Rev 1.44    17/04/2008 15:49:34  ceh
#  Don't set an invalid PDO mapping fault if a normally received value
#  (statusword, actual velocity or torque) is mapped to a TPDO for debug
# 
#     Rev 1.43    16/04/2008 12:11:20  ceh
#  Added R/TPDO COBIDs to motor data window and removed debug puts. Kill all
#  slaves when 1 slave window is closed. Restored capacitor precharge (0x5180)
#  to OD. Corrected type so TPDOs are transmitted correctly.
# 
#     Rev 1.42    04/04/2008 16:06:48  cmp
#  corrected order of m motor pdo declaration in motor slave creation, also
#  added auto config items to slave OD and function to retreve values from slave
#  OD
# 
#     Rev 1.41    25/03/2008 14:15:38  ceh
#  Added new option to co_slave command, auto_detect. When co_slave auto_detect
#  is called, the DVT interrogates the node to determine what slaves and PDOs it
#  is expecting. It then creates suitable slaves with PDOs and SDO servers
#  already configured.
# 
#     Rev 1.40    13/12/2007 13:28:22  ceh
#  Its now possible to set the motor TPDOs+RPDOs COBIDs to any desired level by
#  using {cont RPDO_COBID TPDO_COBID} when specifying type.
# 
#     Rev 1.39    12/11/2007 13:40:06  ceh
#  Set SSDO2 c->s and s->c COBIDs to None if not used to prevent SDO handler
#  crashing.
# 
#     Rev 1.30    12/11/2007 13:17:12  pg
#  Updated to bring in line with engineering file
# 
#     Rev 1.38    09/11/2007 14:10:30  ceh
#  Added optional 2nd SDO server to slave nodes. See help for details.
# 
#     Rev 1.37    28/09/2007 12:43:04  cmp
#  extend functionality of hex2bin
# 
#     Rev 1.36    07/06/2007 15:45:02  ceh
#  Made DSP402 FSM more realistic. Added ability to force DSP402 FSM to error
#  state, allow slow statusword change (sets bit in statusword) or to stay in
#  initialising state. PDOs are synchronous by default. 
# 
#     Rev 1.35    23/04/2007 11:21:08  ceh
#  Updated help text to standard format.
# 
#     Rev 1.34    04/03/2007 13:04:46  cmp
#  add OD revision and vendor id
# 
#     Rev 1.33    08/11/2006 13:40:26  ceh
#  Added module revision registering.
# 
#     Rev 1.32    09/08/2006 14:32:06  ps
#  Prevent divide by zero when calculating bus load
# 
#     Rev 1.31    13/07/2006 16:58:24  cmp
#  added handler (and colour) for debug CLI messages, also added sync handler
#  calback option for slaves
# 
#     Rev 1.30    04/07/2006 11:22:20  cmp
#  added bus load
# 
#     Rev 1.29    14/03/2006 09:34:46  ceh
#  Added new functionality to CANopen slaves for generic testing. They can now:
#  1. Transmit EMCY telegrams.
#  2. Simulate non-Sevcon nodes.
#  3. Reply with an abort message to SDO messages.
#  4. Simulate a bad SDO state error.
# 
#     Rev 1.28    08/03/2006 16:59:44  cmp
#  fixed precharge bug
# 
#     Rev 1.27    20/09/2005 08:39:42  ceh
#  Make cob-id uppercase when adding to CAN type list, otherwise they are not
#  filtered from the CAN window properly.
# 
#    Rev 1.26    07/03/2005 12:01:30  cmp
#  modify to allow caps precharge to be tested

# 
#    Rev 1.25    21/02/2005 14:33:58  cmp
#  Added delta t for recieved messages

# 
#    Rev 1.24    01/11/2004 09:48:12  cmp
#  corrected typo in can_del_protocol_handler

# 
#    Rev 1.23    28/10/2004 18:18:44  ceh
#  Multiple handlers can now be specified for individual COBIDs and Protocols.

# 
#    Rev 1.22    09/07/2004 10:38:44  ceh
#  Fixed bug when setting target torque from PDO.

# 
#    Rev 1.19    01/07/2004 15:50:58  cmp
#  corrected typo

# 
#    Rev 1.18    01/07/2004 12:01:42  cmp
#  reverted hex2bin back to its original implentation, put error checking the
#  slave rpdo arm
#  made tpdo mapping the same as rpdo mapping
#  added comments to suit
#  removed "default" cob id maps

# 
#    Rev 1.17    24/06/2004 15:38:30  cmp
#  added 
#  1. dsp402 state machine 
#  2. syncrounous pdos
#  3. actual velocity and actual torque

# 
#    Rev 1.16    23/06/2004 15:18:04  cmp
#  added some motor remote slave motor stuff

# 
#    Rev 1.15    23/06/2004 10:37:04  cmp
#  changed fixed RPDO mapping

# 
#    Rev 1.14    18/06/2004 14:40:36  cmp
#  keep slaves "topmost"

# 
#    Rev 1.13    11/06/2004 15:13:52  cmp
#  added emcy telegram decode
#  fixed error in slave sdo client
#  chaged co_slave init to key_on
#  added key_off which forces slave back to intialising

# 
#    Rev 1.12    14/05/2004 17:30:36  mdc
#  1. Added help (helptext and helptextd arrays).
#  2. Fixed bug in co_add_cobid_handler when replacing old handler.
#  3. Added can queue length and overflow detection.
#  4. RTR bit in received frames printed in CAN window.

# 
#    Rev 1.11    14/05/2004 11:00:14  cmp
#  corrected heartbeat variable typo

# 
#    Rev 1.10    11/05/2004 22:42:50  mdc
#  1. co_init_slaves renamed "co_slave init ...".
#  2. Slaves produce heartbeats.  Default period 500ms.

# 
#    Rev 1.9    06/05/2004 14:48:30  mdc
#  Slave server performs expedited SDO upload.

# 
#    Rev 1.8    05/05/2004 19:21:10  mdc
#  Added slave SDO server - performs expedited downloads only at present.

# 
#    Rev 1.7    30/04/2004 09:15:34  mdc
#  Updates to co_slave and addition of co_add/del_protocol_handler and
#  co_(un)map_cobid_to_protocol.

# 
#    Rev 1.6    29/04/2004 08:57:42  cmp
#  Corrected typo errors and gave run_slaves access to can_debug 

# 
#    Rev 1.5    28/04/04 22:03:24  mdc
#  Added slave TPDO1 functionality.

# 
#    Rev 1.4    28/04/2004 17:47:08  mdc
#  Added co_add_cobid_handler and co_del_cobid_handler as a nicer way of
#  attaching procs to COB-IDs.

# 
#    Rev 1.3    27/04/2004 17:56:44  mdc
#  Added "callback-by-COB-ID" functionality.  This is used by the SDO client.

# 
#    Rev 1.2    26/04/04 22:45:38  mdc
#  Slave RPDO implemented.  [can poll] returns -1 if no data.

# 
#    Rev 1.1    26/04/2004 17:16:14  cmp
#  added check for reset comms - all nodes NMT CS, added  sdo 602 and 0x08a as
#  EMCY message

# 
#    Rev 1.0    25/04/04 22:34:40  mdc
#  Implements CANopen functionality.

# 
# 
###############################################################################

# register module
register DVT [info script] {$Revision:1.46$}

#
# co_reset
#
# Sets up default COB_IDs for each protocol and removes all user handlers and
# slaves
#
proc co_reset {} {
   global cob_id_decode usr_handler nslaves
   global cobid_handlers
   global debug_canopen
   
   # The COB_ID-to-protocol mapping.  Some are fixed by the CANopen specs (eg,
   # NMT).  Others are updated when slaves are created (eg, pdo, sdo).  Any of
   # the elements can be updated at any time by the user.
   set cob_id_decode(nmt)        {0x0000}
   set cob_id_decode(sync)       {}
   set cob_id_decode(emcy)       {}
   set cob_id_decode(pdo)        {}
   set cob_id_decode(sdo)        {}
   set cob_id_decode(debug)      {}
   set cob_id_decode(unknown)    {}
   
   # Start off with no user handlers
   set usr_handler(nmt)       ""
   set usr_handler(sync)      ""
   set usr_handler(emcy)      ""
   set usr_handler(pdo)       ""
   set usr_handler(sdo)       ""
   set usr_handler(debug)     ""
   set usr_handler(unknown)   ""
   
   # Remove any user-defined COB-ID handlers
   if {[array exists cobid_handlers]} {
      array unset cobid_handlers
   }

   kill_slaves
      
   set debug_canopen 0
   
   # No return value
   return
}

proc kill_slaves {} {
   global nslaves

   # Remove any slaves
   if {[info exists nslaves]} {
      for {set i 0} {$i < $nslaves} {incr i} {
         global slave_${i}
         catch {after cancel [set slave_${i}(hbafterid)]}
         catch {unset slave_${i}}
         catch {destroy .slave_${i}}
      }
   }
   set nslaves 0

}



#
# co_status
#
# Prints information about the current CANopen settings
#
set helptext(co_status) "Displays useful information about the state of the CANopen module"
set helptextd(co_status) "
SYNOPSIS
   co_status

DESCRIPTION
Displays useful information about the state of the CANopen module
"
proc co_status {} {
   global cob_id_decode usr_handler nslaves
   global cobid_handlers
   
   puts "protocol user-handlers        COB_ID(s)"
   puts "-------- -------------------- ---------"
   
   foreach cob_type [array names cob_id_decode] {
      
      puts -nonewline "[format %-8s $cob_type] [format %-20s $usr_handler($cob_type)] "
      
      # The "unknown" handler mops up all other COB-IDs
      if {$cob_type == "unknown"} {
         puts  "-"
      } else {
         puts  "$cob_id_decode($cob_type)"
      }
   }
   
   puts ""
   puts "slave  nodeID NMTstate           OD"
   puts "------ ------ --------           --"
   for {set i 0} {$i<$nslaves} {incr i} {
      global slave_${i}
      
      puts -nonewline "[format %-8s [set slave_${i}(name)]] "
      puts -nonewline "[format 0x%02X [set slave_${i}(nodeid)]] "
      puts -nonewline "[format %-18s [set slave_${i}(nmtstate)]] "
      puts [set slave_${i}(od)]
   }

   puts ""
   puts "COB-ID handlers"
   puts "------ -------"
   foreach cobid [array names cobid_handlers] {
      puts "$cobid $cobid_handlers($cobid)"
   }
   
   puts ""
}


# co_add_cobid_handler
#
# Associates a handler (a callback) with a COB-ID.  More than one handler can
# be associated with a cobid
#
# cobid     The COB-ID, in hex or decimal
# handler   The name of a Tcl proc to handle the COB-ID.  The Tcl proc should be
#           of the form:
#
#              proc cobid_handler {msg} { ...}
#
#           where msg is a CAN message in the format returned from "can poll".
#
set helptext(co_add_cobid_handler) "Associates a user-handler with a COB-ID. More than one handler can be associated with a cobid"
set helptextd(co_add_cobid_handler) "
SYNOPSIS
   co_add_cobid_handler cobid handler

DESCRIPTION
Associates a handler (a callback) with a COB-ID.  More than one handler can be associated with a cobid

cobid     The COB-ID, in hex or decimal
handler   The name of a Tcl proc to handle the COB-ID.  The Tcl proc should be
          of the form:

             proc cobid_handler \{msg\} \{ ...\}

          where msg is a CAN message in the format returned from \"can poll\".
"

proc co_add_cobid_handler {cobid handler} {
   global cobid_handlers
   global debug_canopen

   # Make sure the format of the COB-ID is correct
   set cobid [format 0x%04X $cobid]
   
   # append the handler to the cobid handler array if not already there
   if {[info exists cobid_handlers($cobid)]} {
      if {[lsearch $cobid_handlers($cobid) $handler] == -1} {
         if {$debug_canopen} {dvtConPuts debug "Handler ($handler) added to COB-ID $cobid"}
         lappend cobid_handlers($cobid) $handler
      }
   } else {
      if {$debug_canopen} {dvtConPuts debug "Handler ($handler) added to COB-ID $cobid"}
      set cobid_handlers($cobid) $handler
   }
   
   return
}


# co_del_cobid_handler
#
# Removes the association between COB-ID and one or more handlers. If no handler
# is specified, all handlers are removed, otherwise only the specified handler is
# removed.
#
set helptext(co_del_cobid_handler) "Removes the association between a COB-ID and a user-handler."
set helptextd(co_del_cobid_handler) "
SYNOPSIS
   co_del_cobid_handler cobid ?handler?

DESCRIPTION
Removes the association between COB-ID and one or more handlers. If no handler
is specified, all handlers are removed, otherwise only the specified handler is
removed.
"

proc co_del_cobid_handler {cobid {handler ""}} {
   global cobid_handlers
   global debug_canopen

   # Make sure the format of the COB-ID is correct
   set cobid [format 0x%04X $cobid]
   
   # check if cobid has any handlers.
   if {[info exists cobid_handlers($cobid)]} {
      # if handler == "", remove all of them
      if {$handler == ""} {
         unset cobid_handlers($cobid)
      } else {
         # search for the handler
         if {[set pos [lsearch $cobid_handlers($cobid) $handler]] == -1} {
            if {$debug_canopen} {dvtConPuts debug "Handler $handler is not mapped to COB-ID $cobid"}
         } else {
            if {$debug_canopen} {dvtConPuts debug "Handler $handler removed from COB-ID $cobid"}
            set cobid_handlers($cobid) [lreplace $cobid_handlers($cobid) $pos $pos]
         }

         if {$cobid_handlers($cobid) == ""} {
            unset cobid_handlers($cobid)
         }
      }
   } else {
      if {$debug_canopen} {dvtConPuts debug "No existing handlers for COB-ID $cobid"}
   }
   
   return
}


# co_add_protocol_handler
#
# Associates a handler (a callback) with a protocol.  If a handler already exists
# for the specified protocol, it is replaced and the name of the old handler is 
# returned.  This allows the user to replace the old handler afterwards if needed.
#
# protocol  The protocol - one of nmt, sync, emcy, pdo, sdo, debug or unknown
#
# handler   The name of a Tcl proc to handle the COB-ID.  The Tcl proc should be
#           of the form:
#
#              proc protocol_handler {msg} { ...}
#
#           where msg is a CAN message in the format returned from "can poll".
#
set helptext(co_add_protocol_handler) "Associates a user-handler with a protocol. More than one handler can be associated with a cobid"
set helptextd(co_add_protocol_handler) "
SYNOPSIS
   co_add_protocol_handler protocol handler

DESCRIPTION
Associates a handler (a callback) with a protocol.  If a handler already exists for the specified protocol, 
it is replaced and the name of the old handler is returned.  This allows the user to replace the old handler 
afterwards if needed.

protocol  The protocol - one of nmt, sync, emcy, pdo, sdo, debug or unknown
handler   The name of a Tcl proc to handle the COB-ID.  The Tcl proc should be
          of the form:

             proc protocol_handler \{msg\} \{ ...\}

          where msg is a CAN message in the format returned from \"can poll\".
"

proc co_add_protocol_handler {protocol handler} {
   global usr_handler
   global debug_canopen

   if {![info exist usr_handler($protocol)]} {
      dvtConPuts stderr "Invalid protocol: $protocol"
      return Error
   }
   
   if {[lsearch $usr_handler($protocol) $handler] == -1} {
      if {$debug_canopen} {dvtConPuts debug "Handler ($handler) added to protocol $protocol"}
      lappend usr_handler($protocol) $handler
   }
   
   return
}


# co_del_protocol_handler
#
# Removes the association between protocol and a handler.
#
set  helptext(co_del_protocol_handler) "Removes the association between a protocol and a user-handler."
set helptextd(co_del_protocol_handler) "
SYNOPSIS
   co_del_protocol_handler protocol ?handler?

DESCRIPTION
Removes the association between protocol and one or more handlers. If no handler
is specified, all handlers are removed, otherwise only the specified handler is
removed.
"

proc co_del_protocol_handler {protocol {handler ""}} {
   global usr_handler
   global debug_canopen
   
   if {![info exist usr_handler($protocol)]} {
      dvtConPuts stderr "Invalid protocol: $protocol"
      return Error
   }

   # if handler == "", remove all of them
   if {$handler == ""} {
      set usr_handler($protocol) ""
   } else {
      # search for the handler
      if {[set pos [lsearch $usr_handler($protocol) $handler]] == -1} {
         if {$debug_canopen} {dvtConPuts debug "Handler $handler is not mapped to protocol $protocol"}
      } else {
         if {$debug_canopen} {dvtConPuts debug "Handler $handler removed from protocol $protocol"}
         set usr_handler($protocol) [lreplace $usr_handler($protocol) $pos $pos]
      }
   }
   
   return
}


# co_map_cobid_to_protocol
#
# Maps a COB-ID to a protocol.
#
set helptext(co_map_cobid_to_protocol) "Maps a COB-ID to a protocol."
set helptextd(co_map_cobid_to_protocol) "
SYNOPSIS
   co_map_cobid_to_protocol protocol cobid

DESCRIPTION
Maps a COB-ID to a protocol.

cobid     The COB-ID, in hex or decimal
protocol  The protocol - one of nmt, sync, emcy, pdo, sdo, debug or unknown
"

proc co_map_cobid_to_protocol {protocol cobid} {
   global cob_id_decode
   global debug_canopen
   
   if {![info exist cob_id_decode($protocol)]} {
      dvtConPuts stderr "Invalid protocol: $protocol"
      return Error
   }
   
   if {$protocol == "unknown"} {
      dvtConPuts stderr "Cannot map COB-IDs to the \"unknown\" protocol"
      return Error
   }
   
   set p [lsearch $cob_id_decode($protocol) $cobid]
   if {$p != -1} {
      dvtConPuts stderr "COB-ID $cobid is already mapped to protocol $protocol"
      return Error
   }
   # Make sure the cobid is formatted 0xNNNN
   set cobid [format 0x%04X $cobid]
    
   set cob_id_decode($protocol) [lappend cob_id_decode($protocol) $cobid]
   
   return
}


# co_unmap_cobid_to_protocol
#
# Unmaps a COB-ID to a protocol.
#
set helptext(co_unmap_cobid_to_protocol) "Unmaps a COB-ID to a protocol."
set helptextd(co_unmap_cobid_to_protocol) "
SYNOPSIS
   co_unmap_cobid_to_protocol protocol cobid

DESCRIPTION
Unmaps a COB-ID to a protocol.

cobid     The COB-ID, in hex or decimal
protocol  The protocol - one of nmt, sync, emcy, pdo, sdo, debug or unknown
"

proc co_unmap_cobid_to_protocol {protocol cobid} {
   global cob_id_decode
   global debug_canopen
   
   if {![info exist cob_id_decode($protocol)]} {
      dvtConPuts stderr "Invalid protocol: $protocol"
      return Error
   }
   
   if {$protocol == "unknown"} {
      dvtConPuts stderr "Cannot unmap COB-IDs to the \"unknown\" protocol"
      return Error
   }
   
   # Make sure the cobid is formatted 0xNNNN
   set cobid [format 0x%04x $cobid]
   
   set p [lsearch $cob_id_decode($protocol) $cobid]
   if {$p == -1} {
      dvtConPuts stderr "COB-ID $cobid is not mapped to protocol $protocol"
      return Error
   }
   
   set cob_id_decode($protocol) [lreplace cob_id_decode($protocol) $p $p]
   
   return
}


#
# createslave
#
# Create a simple CANopen slave, something like a simulated Shiroko).  Each
# slave is implemented as an array called slave_X, where X is 0 for the first
# slave, etc.
#
# Parameters
#  slavename   Any valid Tcl name
#  nid         A node id in the range 1 to 127
#  type        if this paramter is included and is "cont" then the slave will 
#              implement the dsp402 state machine and respond appropriately
#              to control words
#
# Each slave has its own window showing some important informatino and the state
# of its inputs and outputs.  The slave has one RPDO, one TPDO and one SDO server.
# All these have fixed mappings and default COB-IDs.  The relevant COB-IDs are
# added to the cob_id_decode lists automatically.
#
# Notes for Tcl programmers:
# 1. This sort of thing might be easier in [incrTcl] rather than standard Tcl.
# 2. Although a list of arrays would be a good representation for a collection of
#    slaves, Tcl doesn't allow arrays to be list elements.
#
proc createslave {slavename nid motor_type ssdo2 pdos} {
   global nslaves cob_id_decode
     
   set nid [format 0x%02X $nid]   
   
   if { ([expr $nid] < 1) || ([expr $nid] > 127) } {
      dvtConPuts stderr "Invalid node ID"
      return
   }
   
   # TODO - Could check that the Node ID is not already in use
     
   # Create a new slave "object".  Each slave is represented as an array.  The
   # slave is initially set to the NMT "initialising" state.  It goes to
   # pre-operational when "co_slave init" is called.
   global slave_${nslaves}
   set slave_${nslaves}(name)           $slavename
   set slave_${nslaves}(nodeid)         $nid
   set slave_${nslaves}(vendor_id)      0x0000001E
   set slave_${nslaves}(prod_code)      0x00000000
   set slave_${nslaves}(rev_no)         0x00000000
   set slave_${nslaves}(HB_ptime)       0x1f4

   set slave_${nslaves}(sevcon_node)    1
   set slave_${nslaves}(nmtstate)       NMT_INITIALISING
   set slave_${nslaves}(cobid_ssdo1_cs) "0x[format %04X [expr 0x600+$nid]]"
   set slave_${nslaves}(cobid_ssdo1_sc) "0x[format %04X [expr 0x580+$nid]]"
   set slave_${nslaves}(nmt_chng_holdoff) 0
   
   # create secondary server if required
   if {$ssdo2 != ""} {
      if {[llength $ssdo2] != 2} {
         dvtConPuts stderr "Invalid secondary server configuration. Must be a 32-bit tx and rx COBID specified. Got $ssdo2"
         return
      }
      
      if {[catch {expr [lindex $ssdo2 0]}]} {
         dvtConPuts stderr "Invalid client->server COBID. Must be a 32-bit COBID. Got [lindex $ssdo2 0]"
         return
      }
      set slave_${nslaves}(cobid_ssdo2_cs) [format 0x%04X [lindex $ssdo2 0]]
      
      if {[catch {expr [lindex $ssdo2 1]}]} {
         dvtConPuts stderr "Invalid server->client COBID. Must be a 32-bit COBID. Got [lindex $ssdo2 1]"
         return
      }
      set slave_${nslaves}(cobid_ssdo2_sc) [format 0x%04X [lindex $ssdo2 1]]
   } else {
      set slave_${nslaves}(cobid_ssdo2_cs) NONE
      set slave_${nslaves}(cobid_ssdo2_sc) NONE
   }
   
   # sort out PDOs. pdos is a list of PDOs. If blank we default to the standard PDO configuration,
   if {$pdos == ""} {
      # generic PDOs - TPDO with 3x16-bit numbers (representing AINs) and 1x8-bit number (representing
      # DINs) and an RPDO with 3x16-bit numbers (representing AOUTs) and 1x8-bit number (representing
      # DOUTs) 
      lappend pdos [list tpdo [format 0x%04X [expr 0x180+$nid]] 0 [list "AIN1  2" "AIN2  2" "AIN3  2" "DIN  1"]]
      lappend pdos [list rpdo [format 0x%04X [expr 0x200+$nid]] 0 [list "AOUT1 2" "AOUT2 2" "AOUT3 2" "DOUT 1"]]
      
      # motor control PDOs (ignored if type != cont)
      lappend pdos [list tpdo [format 0x%04X [expr 0x280+$nid]] 1 [list "cw 2 cw" "tgt_max_trq 2 tgt_max_trq" "tgt_max_spd 4 tgt_max_spd"]]
      lappend pdos [list rpdo [format 0x%04X [expr 0x300+$nid]] 1 [list "sw 2 sw" "act_trq     2 act_trq"     "act_spd     4 act_spd"    ]]
   }
   
   
   # pdos is a list of lists, each of which contains:
   #     "type cobid mtr "name1 length1" "name2 length2" ... "namex lengthx""
   #
   # where:
   #     type  = rpdo or tpdo
   #     cobid = COBID
   #     mtr   = 1 if PDO belongs to motor control and is only displayed if cont is set
   #     namex + lengthx = name of PDO data and length in bytes (eg "AIN1 2" for input)
   set slave_${nslaves}(n_tpdos) 0
   set slave_${nslaves}(n_rpdos) 0
   foreach item $pdos {
      set type  [lindex $item 0]
      set cobid [lindex $item 1]
      set mtr   [lindex $item 2]
      set data  [lindex $item 3]
      
      if {$type == "tpdo"} {
         incr slave_${nslaves}(n_tpdos)
         set pdo_idx [set slave_${nslaves}(n_tpdos)]
        
         set slave_${nslaves}(tpdo[set pdo_idx]_cobid) $cobid
         set slave_${nslaves}(tpdo[set pdo_idx]_mtr)   $mtr
         set slave_${nslaves}(tpdo[set pdo_idx]_data)  $data
        
      } else {
         incr slave_${nslaves}(n_rpdos)
         set pdo_idx [set slave_${nslaves}(n_rpdos)]
        
         set slave_${nslaves}(rpdo[set pdo_idx]_cobid) $cobid
         set slave_${nslaves}(rpdo[set pdo_idx]_mtr)   $mtr
         set slave_${nslaves}(rpdo[set pdo_idx]_data)  $data
      }
   }
   
   set slave_${nslaves}(hbafterid)      ""
   set slave_${nslaves}(key_tog)        0
   set slave_${nslaves}(sdo_abort)      0
   set slave_${nslaves}(sdo_bad_state)  0
   set slave_${nslaves}(sync_handler)   ""
   
   # EMCY telegram
   set slave_${nslaves}(emcy_cobid)         "0x[format %04X [expr 0x080+$nid]]"
   set slave_${nslaves}(emcy_co_err_code)   "0x0000"
   set slave_${nslaves}(emcy_sev_code)      "0x0000"
   set slave_${nslaves}(emcy_db)            "0 0 0"

   
   # set up extra slave info if we are a motor controller otherwise we're an IO node
   set type [lindex $motor_type 0]
   if { $type == "cont" } {
        set slave_${nslaves}(cont_word)     "0x0000"
        set slave_${nslaves}(targ_v)        "0x00000000"
        set slave_${nslaves}(actual_v)      "0x00000000"        
        set slave_${nslaves}(targ_torq)     "0x0000"
        set slave_${nslaves}(actual_torq)   "0x0000"
        set slave_${nslaves}(stat_word)     "0x0000"
        set slave_${nslaves}(402_state)     "MOTOR_NOT_READY_TO_SWITCH_ON"
        set slave_${nslaves}(402_error)     0
        set slave_${nslaves}(allow_slow_sw) 0
        set slave_${nslaves}(stick_in_init) 0
    }   
    
   # create a system to enable or disable syncronous pdos
   set slave_${nslaves}(sync_pdo) 1
    

   # Create the slave's (very limited) object dictionary. This is a list of
   # lists, each of which has three elements.  The first element is the
   # object's index.  This is itself a list of {index, subindex}.  The
   # second element is the object's value.  The third element is either "ro"
   # "rw", indicating whether the object is read-only or read-write.
   #
   # The objects are at present:
   #  1014, 0 - EMCY COB-ID
   #  1017, 0 - Producer heartbeat time
   #  1018, 1 - Vendor ID
   #  5900, 1 - Node ID
   #  5180, 0 - Cap precharge
   #
   # Objects do not exist for the SDO and PDO communication parameters, nor for
   # the simulated inputs and outputs.
   co_update_od_list slave_$nslaves
   
   # Create a new window to show the state of this slave
   toplevel .slave_${nslaves}
   label .slave_${nslaves}.nodeidlabel -text "Node ID"
   label .slave_${nslaves}.nodeid -textvariable slave_${nslaves}(nodeid)
   label .slave_${nslaves}.vendoridlabel -text "Vendor ID"
   label .slave_${nslaves}.vendorid -textvariable slave_${nslaves}(vendor_id) -width 10
   checkbutton .slave_${nslaves}.sevcon_node -text "Sevcon Node" -variable slave_${nslaves}(sevcon_node) -command "set_vendor_id slave_$nslaves"
   
   label .slave_${nslaves}.tclarraylabel -text "Tcl array"
   label .slave_${nslaves}.tclarray -text slave_${nslaves}
   
   label .slave_${nslaves}.nmtstatelabel -text "NMT state"
   label .slave_${nslaves}.nmtstate -textvariable slave_${nslaves}(nmtstate) 
   
   label .slave_${nslaves}.label_ssdo1 -text "SSDO1"
   label .slave_${nslaves}.label_cs -text "c->s"
   label .slave_${nslaves}.cobid_ssdo1_cs -textvariable slave_${nslaves}(cobid_ssdo1_cs) 
   label .slave_${nslaves}.label_sc -text "s->c"
   label .slave_${nslaves}.cobid_ssdo1_sc -textvariable slave_${nslaves}(cobid_ssdo1_sc) 
   checkbutton .slave_${nslaves}.sdo_abort -text "Abort SDO" -variable slave_${nslaves}(sdo_abort) 
   checkbutton .slave_${nslaves}.sdo_bad_state -text "Bad SDO State" -variable slave_${nslaves}(sdo_bad_state) 

   # create secondary server if required
   if {$ssdo2 != ""} {
      label .slave_${nslaves}.label_ssdo2 -text "SSDO2"
      label .slave_${nslaves}.label_cs2 -text "c->s"
      label .slave_${nslaves}.cobid_ssdo2_cs -textvariable slave_${nslaves}(cobid_ssdo2_cs) 
      label .slave_${nslaves}.label_sc2 -text "s->c"
      label .slave_${nslaves}.cobid_ssdo2_sc -textvariable slave_${nslaves}(cobid_ssdo2_sc) 
   }
   
   #add check button to respond to sync messages
   label .slave_${nslaves}.chklbl -text "do sync pdos"
   checkbutton .slave_${nslaves}.sync -variable slave_${nslaves}(sync_pdo) 
   set cmd "co_slave key_on [subst slave_$nslaves]"
   
   button .slave_${nslaves}.key -text "Key On" -command $cmd
   
   # reset motor TPDO and RPDO cobid
   set motor_tpdo_cobid ""
   set motor_rpdo_cobid ""   
   
   # Add any generic RPDOs and TPDOs to main window
   for {set i 1} {$i <= [set slave_${nslaves}(n_rpdos)]} {incr i} {
      if {![set slave_${nslaves}(rpdo[set i]_mtr)]} {
         label .slave_${nslaves}.label_rpdo[set i] -text "RPDO[set i]"
         label .slave_${nslaves}.cobid_rpdo[set i] -textvariable slave_${nslaves}(rpdo[set i]_cobid)
      
         foreach item [set slave_${nslaves}(rpdo[set i]_data)] {
            set name [lindex $item 0]
            set size [lindex $item 1]
            
            set slave_${nslaves}(rpdo[set i]_[set name]) [merge_hex [string repeat "0x00 " $size]]
            
            label .slave_${nslaves}.rpdo[set i]_[set name]      -textvariable slave_${nslaves}(rpdo[set i]_[set name]) -font {"Courier New" 8}
            label .slave_${nslaves}.rpdo[set i]_[set name]_desc -text $name                                            -font {"Courier New" 8}
         }
      } else {
         set motor_rpdo_cobid [set slave_${nslaves}(rpdo[set i]_cobid)]
      }
   }
   
   
   for {set i 1} {$i <= [set slave_${nslaves}(n_tpdos)]} {incr i} {
      if {![set slave_${nslaves}(tpdo[set i]_mtr)]} {
         label .slave_${nslaves}.label_tpdo[set i] -text "TPDO[set i]"
         label .slave_${nslaves}.cobid_tpdo[set i] -textvariable slave_${nslaves}(tpdo[set i]_cobid)
         
         foreach item [set slave_${nslaves}(tpdo[set i]_data)] {
            set name [lindex $item 0]            
            set size [lindex $item 1]
             
            set slave_${nslaves}(tpdo[set i]_[set name]) [merge_hex [string repeat "0x00 " $size]]
            entry .slave_${nslaves}.tpdo[set i]_[set name]      -textvariable slave_${nslaves}(tpdo[set i]_[set name]) -font {"Courier New" 8} -width 6
            label .slave_${nslaves}.tpdo[set i]_[set name]_desc -text $name                                            -font {"Courier New" 8}
         }
      } else {
         set motor_tpdo_cobid [set slave_${nslaves}(tpdo[set i]_cobid)]
      }
   }
   
   if {[set slave_${nslaves}(n_tpdos)] > 0} {
      button .slave_${nslaves}.tx_tpdo1 -text "Tx TPDO"
   }

   # EMCY telegram
   label .slave_${nslaves}.label_emcy       -text "EMCY"
   label .slave_${nslaves}.emcy_cobid       -textvariable slave_${nslaves}(emcy_cobid)
   
   label .slave_${nslaves}.label_emcy_ec    -text "Err Code"
   entry .slave_${nslaves}.emcy_co_err_code -textvariable slave_${nslaves}(emcy_co_err_code) -font {"Courier New" 8} -width 6
   label .slave_${nslaves}.label_emcy_sc    -text "Sev Code"
   entry .slave_${nslaves}.emcy_sev_code    -textvariable slave_${nslaves}(emcy_sev_code)    -font {"Courier New" 8} -width 6
   label .slave_${nslaves}.label_emcy_db    -text "Data"
   entry .slave_${nslaves}.emcy_db          -textvariable slave_${nslaves}(emcy_db)          -font {"Courier New" 8} -width 6
   
   button .slave_${nslaves}.tx_emcy         -text "Tx EMCY" -command "emcytx slave_$nslaves"
   
   if { $type == "cont" } {
       # Add inputs to the slave
       labelframe  .slave_${nslaves}.motor               -text "motor data (RPDO=$motor_rpdo_cobid, TPDO=$motor_tpdo_cobid)"
       label       .slave_${nslaves}.motor.lbl_cont_word -text "control word"
       label       .slave_${nslaves}.motor.cont_word     -textvariable slave_${nslaves}(cont_word) -font {"Courier New" 8}
       label       .slave_${nslaves}.motor.lbl_tv        -text "target velocity"
       label       .slave_${nslaves}.motor.targ_v        -textvariable slave_${nslaves}(targ_v) -font {"Courier New" 8}
       label       .slave_${nslaves}.motor.lbl_av        -text "actual velocity"
       entry       .slave_${nslaves}.motor.act_v         -textvariable slave_${nslaves}(actual_v) -font {"Courier New" 8} -width 12
       label       .slave_${nslaves}.motor.lbl_at        -text "actual torque"
       entry       .slave_${nslaves}.motor.act_t         -textvariable slave_${nslaves}(actual_torq) -font {"Courier New" 8} -width 12       
       label       .slave_${nslaves}.motor.lbl_tt        -text "target torque"
       label       .slave_${nslaves}.motor.targ_t        -textvariable slave_${nslaves}(targ_torq) -font {"Courier New" 8}
       label       .slave_${nslaves}.motor.lbl_sw        -text "status word"  
       label       .slave_${nslaves}.motor.stat_word     -textvariable slave_${nslaves}(stat_word) -font {"Courier New" 8}
       label       .slave_${nslaves}.motor.state         -textvariable slave_${nslaves}(402_state) -font {"Courier New" 8} -width 25 -justify left
       label       .slave_${nslaves}.motor.precharge     -text "caps pre charge" -image .redball -compound left
       checkbutton .slave_${nslaves}.motor.stick_in_init -text "Stay in init state"      -variable slave_${nslaves}(stick_in_init)
       checkbutton .slave_${nslaves}.motor.allow_slow_sw -text "Allow slow statusword"   -variable slave_${nslaves}(allow_slow_sw)
       checkbutton .slave_${nslaves}.motor.fault_state   -text "Click for fault" -variable slave_${nslaves}(402_error) -command ".slave_${nslaves}.motor.fault_state configure -state disabled; .slave_${nslaves}.motor.fault_state configure -text {Fault Set}"
   }
   
   # When the "tx" button is pressed, we pass the widget's pathname to slavetx.  This organises the
   # transmission of the PDO.
   bind .slave_${nslaves}.tx_tpdo1 <Button-1> {slavetx %W}
   
   set row_n 1
   grid configure .slave_${nslaves}.nodeidlabel    -row $row_n -column 1 -padx 10  -sticky w
   grid configure .slave_${nslaves}.nodeid         -row $row_n -column 2           -sticky w
   grid configure .slave_${nslaves}.vendoridlabel  -row $row_n -column 3           -sticky w
   grid configure .slave_${nslaves}.vendorid       -row $row_n -column 4           -sticky w
   grid configure .slave_${nslaves}.sevcon_node    -row $row_n -column 5           -sticky w
   grid configure .slave_${nslaves}.chklbl         -row $row_n -column 7           -sticky w
   grid configure .slave_${nslaves}.sync           -row $row_n -column 7 -padx 40  -sticky w
   incr row_n
   
   grid configure .slave_${nslaves}.key            -row $row_n -column 7 -padx 40  -sticky w
   
   grid configure .slave_${nslaves}.tclarraylabel  -row $row_n -column 1 -padx 10  -sticky w
   grid configure .slave_${nslaves}.tclarray       -row $row_n -column 2           -sticky w
   incr row_n
   
   grid configure .slave_${nslaves}.nmtstatelabel  -row $row_n -column 1 -padx 10  -sticky w
   grid configure .slave_${nslaves}.nmtstate       -row $row_n -column 2           -sticky w
   incr row_n
   
   grid configure .slave_${nslaves}.label_ssdo1    -row $row_n -column 1 -padx 10  -sticky w
   grid configure .slave_${nslaves}.cobid_ssdo1_cs -row $row_n -column 2           -sticky w
   grid configure .slave_${nslaves}.label_cs       -row $row_n -column 3           -sticky w
   grid configure .slave_${nslaves}.cobid_ssdo1_sc -row $row_n -column 4           -sticky w
   grid configure .slave_${nslaves}.label_sc       -row $row_n -column 5           -sticky w
   grid configure .slave_${nslaves}.sdo_abort      -row $row_n -column 6           -sticky w
   grid configure .slave_${nslaves}.sdo_bad_state  -row $row_n -column 7           -sticky w
   incr row_n
   
   # create secondary server if required
   if {$ssdo2 != ""} {
       grid configure .slave_${nslaves}.label_ssdo2    -row $row_n -column 1 -padx 10  -sticky w
       grid configure .slave_${nslaves}.cobid_ssdo2_cs -row $row_n -column 2           -sticky w
       grid configure .slave_${nslaves}.label_cs2      -row $row_n -column 3           -sticky w
       grid configure .slave_${nslaves}.cobid_ssdo2_sc -row $row_n -column 4           -sticky w
       grid configure .slave_${nslaves}.label_sc2      -row $row_n -column 5           -sticky w
       incr row_n
   }
   
   # add in RPDOs and TPDOs
   for {set i 1} {$i <= [set slave_${nslaves}(n_rpdos)]} {incr i} {
      if {![set slave_${nslaves}(rpdo[set i]_mtr)]} {
         grid configure .slave_${nslaves}.label_rpdo[set i]    -row $row_n -rowspan 2 -column 1 -padx 10  -sticky w
         grid configure .slave_${nslaves}.cobid_rpdo[set i]    -row $row_n -rowspan 2 -column 2           -sticky w
         
         set col_n 3
         foreach item [set slave_${nslaves}(rpdo[set i]_data)] {
            grid configure .slave_${nslaves}.rpdo[set i]_[lindex $item 0]       -row $row_n            -column $col_n -sticky w
            grid configure .slave_${nslaves}.rpdo[set i]_[lindex $item 0]_desc  -row [expr $row_n + 1] -column $col_n -sticky w
            incr col_n
         }
         
         incr row_n 2
      }
   }
   
   for {set i 1} {$i <= [set slave_${nslaves}(n_tpdos)]} {incr i} {
      if {![set slave_${nslaves}(tpdo[set i]_mtr)]} {
         grid configure .slave_${nslaves}.label_tpdo[set i]    -row $row_n -rowspan 2 -column 1 -padx 10  -sticky w
         grid configure .slave_${nslaves}.cobid_tpdo[set i]    -row $row_n -rowspan 2 -column 2           -sticky w
         
         set col_n 3
         foreach item [set slave_${nslaves}(tpdo[set i]_data)] {
            grid configure .slave_${nslaves}.tpdo[set i]_[lindex $item 0]       -row $row_n            -column $col_n -sticky w
            grid configure .slave_${nslaves}.tpdo[set i]_[lindex $item 0]_desc  -row [expr $row_n + 1] -column $col_n -sticky w
            incr col_n
         }
         
         incr row_n 2
      }
   }
   
   if {[set slave_${nslaves}(n_tpdos)] > 0} {
      grid configure .slave_${nslaves}.tx_tpdo1       -row $row_n -column 7 -padx 10 -ipadx 10
      incr row_n
   }
   
   grid configure .slave_${nslaves}.label_emcy          -row $row_n -rowspan 2 -column 1 -padx 10  -sticky w
   grid configure .slave_${nslaves}.emcy_cobid          -row $row_n -rowspan 2 -column 2   -sticky w
   grid configure .slave_${nslaves}.label_emcy_ec       -row $row_n -column 3   -sticky w
   grid configure .slave_${nslaves}.label_emcy_sc       -row $row_n -column 4   -sticky w
   grid configure .slave_${nslaves}.label_emcy_db       -row $row_n -column 5   -sticky w
   incr row_n
   
   grid configure .slave_${nslaves}.emcy_co_err_code    -row $row_n -column 3   -sticky w
   grid configure .slave_${nslaves}.emcy_sev_code       -row $row_n -column 4   -sticky w
   grid configure .slave_${nslaves}.emcy_db             -row $row_n -column 5   -sticky w
   grid configure .slave_${nslaves}.tx_emcy             -row $row_n -column 7   -padx 10 -ipadx 10
   incr row_n

   grid configure .slave_${nslaves}.nmtstate -columnspan 4
   
   if { $type == "cont" } {
       grid configure .slave_${nslaves}.motor               -row $row_n -columnspan 8      -sticky ew
       grid configure .slave_${nslaves}.motor.lbl_cont_word -row 0 -column 0  -padx 10     -sticky w
       grid configure .slave_${nslaves}.motor.cont_word     -row 0 -column 1               -sticky w
       grid configure .slave_${nslaves}.motor.precharge     -row 0 -column 3               -sticky w       
       grid configure .slave_${nslaves}.motor.lbl_tv        -row 1 -column 0  -padx 10     -sticky w
       grid configure .slave_${nslaves}.motor.targ_v        -row 1 -column 1               -sticky w
       grid configure .slave_${nslaves}.motor.lbl_av        -row 1 -column 2  -padx 20     -sticky w
       grid configure .slave_${nslaves}.motor.act_v         -row 1 -column 2  -padx 110    -sticky w
       grid configure .slave_${nslaves}.motor.stick_in_init -row 1 -column 3               -sticky w
       grid configure .slave_${nslaves}.motor.lbl_tt        -row 2 -column 0  -padx 10     -sticky w
       grid configure .slave_${nslaves}.motor.targ_t        -row 2 -column 1               -sticky w
       grid configure .slave_${nslaves}.motor.lbl_at        -row 2 -column 2  -padx 20     -sticky w
       grid configure .slave_${nslaves}.motor.act_t         -row 2 -column 2  -padx 110    -sticky w
       grid configure .slave_${nslaves}.motor.allow_slow_sw -row 2 -column 3               -sticky w
       grid configure .slave_${nslaves}.motor.lbl_sw        -row 3 -column 0  -padx 10     -sticky w
       grid configure .slave_${nslaves}.motor.stat_word     -row 3 -column 1               -sticky w
       grid configure .slave_${nslaves}.motor.state         -row 3 -column 2  -padx 20     -sticky w
       grid configure .slave_${nslaves}.motor.fault_state   -row 3 -column 3               -sticky w
       incr row_n
   }
   
   wm title .slave_${nslaves} "Slave \"[set slave_${nslaves}(name)]\""
   wm resizable .slave_${nslaves}  0 0
   wm attributes .slave_${nslaves} -topmost 1
   
   # kill all slaves when any slave window is closed. We must kill all slaves due to the way
   # they are named using consequetive numbers (ie slave0, slave1, etc). Destroying 1 slave
   # would require a lot of rewriting of this module.
   wm protocol .slave_${nslaves} WM_DELETE_WINDOW "kill_slaves; destroy .slave_${nslaves}"
      
   # Update the COB ID decoding lists
   for {set i 1} {$i <= [set slave_${nslaves}(n_rpdos)]} {incr i} {
      # decode all generic PDOs and motor PDOs if type is cont (motor controller)
      if {![set slave_${nslaves}(rpdo[set i]_mtr)] || ($type == "cont")} {
         lappend cob_id_decode(pdo) [set slave_${nslaves}(rpdo[set i]_cobid)]
      }
   }
   
   for {set i 1} {$i <= [set slave_${nslaves}(n_tpdos)]} {incr i} {
      # decode all generic PDOs and motor PDOs if type is cont (motor controller)
      if {![set slave_${nslaves}(tpdo[set i]_mtr)] || ($type == "cont")} {
         lappend cob_id_decode(pdo) [set slave_${nslaves}(tpdo[set i]_cobid)]
      }
   }
   
   lappend cob_id_decode(sdo) [set slave_${nslaves}(cobid_ssdo1_sc)]
   lappend cob_id_decode(sdo) [set slave_${nslaves}(cobid_ssdo1_cs)]
   
   # map secondary server if required
   if {$ssdo2 != ""} {
       lappend cob_id_decode(sdo) [set slave_${nslaves}(cobid_ssdo2_sc)]
       lappend cob_id_decode(sdo) [set slave_${nslaves}(cobid_ssdo2_cs)]
   }

   incr nslaves

   return "slave_[expr $nslaves-1]"
}


# co_update_od_list
#
# Updates the od list. This is a list of lists, each of which has three 
# elements.  The first element is the object's index.  This is itself a 
# list of {index, subindex}.  The second element is the object's value.  
# The third element is either "ro" "rw", indicating whether the object 
# is read-only or read-write.
#
# The objects are at present:
#  1014, 0 - EMCY COB-ID
#  1017, 0 - Producer heartbeat time
#  1018, 1 - Vendor ID
#  5900, 1 - Node ID
#  5180, 0 - Cap precharge
#
#  5810, 1:7 - added slave autoconfig items
#
# Objects do not exist for the SDO and PDO communication parameters, nor for
# the simulated inputs and outputs.
proc co_update_od_list {slave} {
    global $slave
    set [set slave](od) [list \
        [list [list 0x1014 0x00] [set [set slave](emcy_cobid)]  rw]\
        [list [list 0x1017 0x00] [set [set slave](HB_ptime)]    rw]\
        [list [list 0x1018 0x01] [set [set slave](vendor_id)]   ro]\
        [list [list 0x1018 0x02] [set [set slave](prod_code)]   ro]\
        [list [list 0x1018 0x03] [set [set slave](rev_no)]      ro]\
        [list [list 0x5900 0x01] [set [set slave](nodeid)]      ro]\
        [list [list 0x5180 0x00] 0x00                           rw]\
        [list [list 0x5811 0x01] 0x00                           rw]\
        [list [list 0x5811 0x02] 0x00                           rw]\
        [list [list 0x5811 0x03] 0x00                           rw]\
        [list [list 0x5811 0x04] 0x00                           rw]\
        [list [list 0x5811 0x05] 0x00                           rw]\
        [list [list 0x5811 0x06] 0x00                           rw]\
        [list [list 0x5811 0x07] 0x00                           rw]\
    ]
}



set can_q 0
set can_qmax 0
set can_ov ""

#
# co_poll
#
# Polls the FIFO and determines the protocol of the CANopen message retrieved from its
# COB_ID.  This can be SYNC, EMCY, PDO etc.  A handler with a name of the form
# process_sync is called.  The mapping from COB_ID to protocol is contained in the
# cob_id_decode array.
#
# This proc is normally called on a timed basis.
#
# The can poll returns this data:
#       -items remaining    an integer
#       -timestamp          an integer (in 100us ticks)
#       -RTR                either 0 or 1
#       -Over run           either 0 or 1
#       -cob id             in the format 0xNNNN
#       -data               an opening brace, each byte in the format 0xNN, and a closing brace
#
# These fields in the msg are treated as follows:
#
#  items remaining      Used to update can_q.  This appears on the GUI.
#  timestamp            Displayed in the can window on the GUI
#  RTR                  Displayed in the can window on the GUI
#  overflow             Used to update can_ov.  This appears on the GUI.
#  cob_id and data      Displayed in the can window on the GUI
#

set bits_recieved 0
set CAN_load_timer 0 

proc co_poll {} {
   global cob_id_decode usr_handler cobid_handlers bits_recieved CAN_load_timer
   global can_q can_qmax can_ov
   
   # Poll the FIFO
   set msg [can poll]
   
   # Get items remaining and keep track of max size of the queue.  The two
   # can_q variables are available to the GUI.
   
   #set can_q [lindex $msg 0]
   if {[lindex $msg 0] > $can_qmax} {
      set can_qmax [lindex $msg 0]
   }
   
   # Was a message retrieved from the FIFO in the DLL?
   while { [lindex $msg 0] != -1 } {
      
      # start a timer
      if {$bits_recieved == 0} {
        set CAN_load_timer [clock clicks -millisecond]
      }
      
      # count bits recived we build this from an average value 
      #
      # no stuff bits         total bits = 47 +  8 * DLC 
      # max stuff bits        total bits = 55 + 10 * DLC
      #
      #     we'll assume that we have an average number of stuff bits
      
      incr bits_recieved [expr 51 + (9 * [llength [lindex $msg 5]])]
      
      set cob_id [lindex $msg 4]
      set type   unknown
      
      # Check for queue overflow.
      if {[lindex $msg 3] != 0} {
         set can_ov OV
      }
   
      # Work out what the COB_ID means
      foreach cob_type [array names cob_id_decode] {
         if {[string first $cob_id $cob_id_decode($cob_type)] >= 0} {
            set type $cob_type
            break
         }
      }
      
      # Call COB-ID callbacks if any have been specified
      if {[info exists cobid_handlers($cob_id)]} {
          foreach fn $cobid_handlers($cob_id) {
             eval $fn [list $msg]
          }
      }
      
      # Call appropriate default proc for this type of CANopen object
      process_$type $msg
      
      # Call a user handler if there is one
      foreach fn $usr_handler($type) {
         $fn $msg
      }

      # Pass this received message to all slaves which have been created
      run_slaves $cob_id [lindex $msg 5]
      
      # Give Tcl a look-in
      update
      
      # Get the next message from the FIFO
      set msg [can poll]
   } 
}


proc calc_CAN_load {} {
    
    global bits_recieved CAN_load_timer canbaud can_q
    
    foreach b [array names canbaud] {
        if {$canbaud($b)} { set baud $b; break}
    }
    set te [expr [clock clicks -millisecond] - $CAN_load_timer]
    
    if { ( $te > 0 ) && ( $baud > 0 ) } {
    
        set bit_rate_now [expr (double($bits_recieved) * 1000) / $te]        
        set bus_load [expr double($bit_rate_now) / ($baud * 10)]
    
    } else {
        
        set bit_rate_now 0        
        set bus_load 100
        
    }
    
    set can_q [format %2.2f%% $bus_load]    
    #infomsg "bits recieved $bits_recieved, te = $te bit_rate_now $bit_rate_now : baud = [expr $baud * 1000] Load = ${bus_load}%"
    
    set bits_recieved 0
    
    after 1000 calc_CAN_load 
}
calc_CAN_load

# record a delta t for each type of message
global time_of_last

set time_of_last(unknown) 0
set time_of_last(sync)    0
set time_of_last(emcy)    0
set time_of_last(pdo)     0
set time_of_last(sdo)     0
set time_of_last(nmt)     0


proc process_unknown {msg} {
       
   global time_of_last 
   # Do standard processing for unknown messages
   canmsg "[lindex $msg 4] [format %8d [lindex $msg 1]]  - [format d%4.1f [expr ([lindex $msg 1] - $time_of_last(unknown)) * 0.1]] [lindex $msg 2] Unknown {[lindex $msg 5]}" unknown
   set time_of_last(unknown) [lindex $msg 1]
   
}

proc process_sync {msg} {
   global time_of_last 
   # Do standard SYNC processing
   canmsg "[lindex $msg 4] [format %8d [lindex $msg 1]] - [format d%4.1f [expr ([lindex $msg 1] - $time_of_last(sync)) * 0.1]] [lindex $msg 2] SYNC " sync
   set time_of_last(sync) [lindex $msg 1]
}

proc process_emcy {msg} {
   global time_of_last 
   # Do standard EMCY processing
   canmsg "[lindex $msg 4] [format %8d [lindex $msg 1]] - [format d%4.1f [expr ([lindex $msg 1] - $time_of_last(emcy)) * 0.1]] [lindex $msg 2] EMCY [decode_emcy [lindex $msg 5]]" emcy
   set time_of_last(emcy) [lindex $msg 1]
}

proc process_pdo {msg} {
   global time_of_last 
   # Do standard PDO processing
   canmsg "[lindex $msg 4] [format %8d [lindex $msg 1]] - [format d%4.1f [expr ([lindex $msg 1] - $time_of_last(pdo)) * 0.1]] [lindex $msg 2] PDO {[lindex $msg 5]}" pdo
   set time_of_last(pdo) [lindex $msg 1]
}


proc process_debug {msg} {
   global time_of_last 
   # Do standard debug processing
   canmsg "[lindex $msg 4] [format %8d [lindex $msg 1]] - [format d%4.1f [expr ([lindex $msg 1] - $time_of_last(pdo)) * 0.1]] [lindex $msg 2] DEBUG {[lindex $msg 5]}" debug
   set time_of_last(debug) [lindex $msg 1]
}


proc process_sdo {msg} {
   global time_of_last 
   # Do standard SDO processing - run an SDO server if the user has created one
   canmsg "[lindex $msg 4] [format %8d [lindex $msg 1]] - [format d%4.1f [expr ([lindex $msg 1] - $time_of_last(sdo)) * 0.1]] SDO {[lindex $msg 5]}" sdo
   set time_of_last(sdo) [lindex $msg 1]
}

proc process_nmt {msg} {
   global time_of_last 
   # Do standard NMT processing
   canmsg "[lindex $msg 4] [format %8d [lindex $msg 1]] - [format d%4.1f [expr ([lindex $msg 1] - $time_of_last(nmt)) * 0.1]] [lindex $msg 2] NMT {[lindex $msg 5]}" nmt
   set time_of_last(nmt) [lindex $msg 1]
}


# Called once per slave at the slave's heartbeat producer period.  Generates a 
# heartbeat message from a slave.
# slavenum is 1..nslaves
proc heartbeat_slave {slavenum} {
   global slave_${slavenum}

   set h(NMT_INITIALISING)    0x00
   set h(NMT_OPERATIONAL)     0x05
   set h(NMT_STOPPED)         0x04
   set h(NMT_PREOPERATIONAL)  0x7F
   
   set n [set slave_${slavenum}(nmtstate)]
   
   # check precharge state
   set idx 0x5180
   set sidx 0x00
   # Search through the slave's OD for the specified index and subindex.
   set cmd "lsearch -regexp \$slave_${slavenum}(od) {.*{.*$idx $sidx} .* r.}"
   set pos [eval $cmd]
   
   # Get precharge state
   set t [lindex [set slave_${slavenum}(od)] $pos]
   set t [lindex $t 1]
   if {[info exists slave_${slavenum}(cont_word)] && [winfo exists .slave_${slavenum}.motor.precharge]} {
        if {[expr $t]} {
            .slave_${slavenum}.motor.precharge configure -image .greenball
        } else {
            .slave_${slavenum}.motor.precharge configure -image .redball
        }
   }
   
   # If the slave is in INITIALISING, do nothing
   if {$n == "NMT_INITIALISING"} {
      return
   }
   
   set idx 0x1017
   set sidx 0x00
   # Search through the slave's OD for the specified index and subindex.
   set cmd "lsearch -regexp \$slave_${slavenum}(od) {.*{.*$idx $sidx} .* r.}"
   set pos [eval $cmd]
   
   # Get the heartbeat producer time
   set t [lindex [set slave_${slavenum}(od)] $pos]
   set t [lindex $t 1]
   
   if {$t != 0} {
      # Send a heartbeat message
      can send "[format 0x%04X [expr 1792+[set slave_${slavenum}(nodeid)]]] $h($n)"
      
      # Repeat again after the required period
      # remember the after id so we can cancel it on a reset.
      set slave_${slavenum}(hbafterid) [after $t "heartbeat_slave $slavenum"]
   } else {
      # Check heartbeat producer time again after 1000ms
      set slave_${slavenum}(hbafterid) [after 1000 "heartbeat_slave $slavenum"]
   }
}


# Simulate a CANopen slave
proc run_slaves {cob_id data} {
   global nslaves debug_canopen cob_id_decode
   
  for {set i 0} {$i<$nslaves} {incr i} {
      global slave_${i}
      
      
      # If the slave is in INITIALISING, ignore all CAN traffic
      if {[set slave_${i}(nmtstate)] == "NMT_INITIALISING"} {
         continue
      }
      
      set nodeid [set slave_${i}(nodeid)]
      set name   [set slave_${i}(name)]
      
      # Implement this message for this slave      
      if {$cob_id == 0x0000} {       
         # COB-ID = 0x0000 - this is an NMT message
         # check the if the requested node id belongs to the current slave, if so act...
         # if the id is 0 (all nodes) always respond
         if {([lindex $data 1] == $nodeid) || ([lindex $data 1] == 0x00) } {
            switch [lindex $data 0] {
               0x01 {
                  # This is START_REMOTE_NODE
                  after [set slave_${i}(nmt_chng_holdoff)] [subst "set slave_${i}(nmtstate) NMT_OPERATIONAL"]
                  if {$debug_canopen} {dvtConPuts debug "Slave $name has gone to OPERATIONAL"}
               }
               0x02 {
                  # This is STOP_REMOTE_NODE
                  after[set ${i}(nmt_chng_holdoff)] [subst "set slave_${i}(nmtstate) NMT_STOPPED"]
                  if {$debug_canopen} {dvtConPuts debug "Slave $name has gone to STOPPED"}
               }
               0x80 {
                  # This is ENTER_PREOPERATIONAL
                  after [set slave_${i}(nmt_chng_holdoff)] [subst "set slave_${i}(nmtstate) NMT_PREOPERATIONAL"]
                  if {$debug_canopen} {dvtConPuts debug "Slave $name has gone to PREOPERATIONAL"}
               }
               0x82 {
                  # This is a RESET COMMS CS
                  # this is an illegal state after reset normally overwritten immeditately
                  set slave_${i}(nmtstate) NMT_STOPPED
                  after [set slave_${i}(nmt_chng_holdoff)] [subst "set slave_${i}(nmtstate) NMT_PREOPERATIONAL"]
                  # Send the bootup message
                  set nodeid [set slave_${i}(nodeid)]
                  can send "[format 0x%04X [expr 0x700+$nodeid]] 0x00"

                  if {$debug_canopen} {dvtConPuts debug "Slave $name has gone to PREOPERATIONAL"}
                  
               }
               default {
                  if {$debug_canopen} {dvtConPuts stderr "Slave $name has received unrecognised CS in NMT message ([lindex $data 0])"}
               }
            }
         } else {
            # The node ID in the NMT packet does not match this slave's node ID
         }
      } elseif {$cob_id == [set slave_${i}(cobid_ssdo1_cs)]} {
         # Run slave default SDO server.  This is in it's own proc
         slave_sdo_server $i $data [set slave_${i}(cobid_ssdo1_sc)]
      
      } elseif {$cob_id == [set slave_${i}(cobid_ssdo2_cs)]} {
         # Run slave secondary SDO server.  This is in it's own proc
         slave_sdo_server $i $data [set slave_${i}(cobid_ssdo2_sc)]
      
      } elseif { ([lsearch $cob_id_decode(sync) $cob_id] != -1) && [set slave_${i}(sync_pdo)] } {
          # a sync has been recieved and we're configured to respond
          
          # transmit IO info
          
          #
          # if defined use external sync handler, otherwise just call standard PDO handler.
          #
          if {[set slave_${i}(sync_handler)] != ""} {
              [set slave_${i}(sync_handler)] slave_${i} $cob_id $data
          } else {
              co_slave tx_tpdo slave_${i} $cob_id $data
          }
          
      } else {
         # check to see if this is an RPDO
         if {[set slave_${i}(nmtstate)] == "NMT_OPERATIONAL"} {
            for {set j 1} {$j <= [set slave_${i}(n_rpdos)]} {incr j} {
               # check for matching COBID
               if {[set slave_${i}(rpdo[set j]_cobid)] == $cob_id} {
                  # for generic rpdos, just unmap the data directly
                  if {![set slave_${i}(rpdo[set j]_mtr)]} {
                
                     set idx 0
                     foreach item [set slave_${i}(rpdo[set j]_data)] {
                        set name  [lindex $item 0]
                        set size  [lindex $item 1]
                        
                        set value [merge_hex [lrange $data $idx [expr $idx+$size-1]]]
                        set slave_${i}(rpdo[set j]_[set name]) $value
                        
                        incr idx $size
                     }
                  } else {
                     # this is a motor controller so unmap received data according to the data type
                     set idx 0
                     foreach item [set slave_${i}(rpdo[set j]_data)] {
                        set size  [lindex $item 1]
                        set type  [lindex $item 2]
                    
                        set value [merge_hex [lrange $data $idx [expr $idx+$size-1]]]
                        
                        switch $type {
                           cw          {set slave_${i}(cont_word) $value}
                           tgt_max_trq {set slave_${i}(targ_torq) $value}
                           tgt_max_spd {set slave_${i}(targ_v) $value}
                        }
                        
                        incr idx $size
                     }
                     
                     # decode inbound control word and run dsp 402 fsm 
                     if { [set slave_${i}(402_error)] } {
                         set slave_${i}(402_state) [402_fsm $i MOTOR_FAULT FAULT]
                         set slave_${i}(stat_word) [get_status_word $i MOTOR_FAULT]
                
                     } else {
                         if { [expr [set slave_${i}(cont_word)] & 0x0007] == 0x0006 } {
                             # shut down command recieved
                             set slave_${i}(402_state) [402_fsm $i [set slave_${i}(402_state)] SHUTDOWN]
                         } elseif { [expr [set slave_${i}(cont_word)] & 0x000F] == 0x0007 } {
                             # switchon1 recieved
                             set slave_${i}(402_state) [402_fsm $i [set slave_${i}(402_state)] SWITCH_ON1]
                             set slave_${i}(402_state) [402_fsm $i [set slave_${i}(402_state)] DISABLE_OPERATION]
                         } elseif { [expr [set slave_${i}(cont_word)] & 0x000F] == 0x000F } {
                             # switchon2 recieved
                             set slave_${i}(402_state) [402_fsm $i [set slave_${i}(402_state)] SWITCH_ON2]
                             set slave_${i}(402_state) [402_fsm $i [set slave_${i}(402_state)] ENABLE_OPERATION]
                         } elseif { [expr [set slave_${i}(cont_word)] & 0x0002] == 0x0000 } {
                             # disable voltage recieved        
                             set slave_${i}(402_state) [402_fsm $i [set slave_${i}(402_state)] DISABLE_VOLTAGE]
                         } elseif { [ expr [set slave_${i}(cont_word)] & 0x0006] == 0x0002} {
                             # quick stop received
                             set slave_${i}(402_state) [402_fsm $i [set slave_${i}(402_state)] QUICK_STOP]
                
                             # if the state has changed to Quick Stop Active start a timer which will
                             # change the state to Switch On Disable after 3s.
                             if {[set slave_${i}(402_state)] == "MOTOR_QUICK_STOP_ACTIVE"} {
                                 global quick_stop_ids
                                 lappend quick_stop_ids "slave_$i"
                                 after 3000 {
                                     global quick_stop_ids
                   
                                     set slave_id [lindex $quick_stop_ids 0]
                                     set i        [string range $slave_id 6 end]
                                     set quick_stop_ids [lreplace quick_stop_ids 0 0]
                   
                                     global $slave_id
                                     if { [set [set slave_id](402_error)]] } {
                                         set [set slave_id](402_state) [402_fsm $i MOTOR_FAULT FAULT]
                                         set [set slave_id](stat_word) [get_status_word $i MOTOR_FAULT]
                           
                                     } elseif {[set [set slave_id](402_state)] == "MOTOR_QUICK_STOP_ACTIVE"} {
                                         set [set slave_id](402_state) [402_fsm $i [set [set slave_id](402_state)] QUICK_STOP_COMPLETE]
                                         set [set slave_id](stat_word) [get_status_word $i [set [set slave_id](402_state)]]
                                     }
                                 }
                             }
                         } else {
                             return "Invalid controlword recieved"
                         }            
                         set slave_${i}(stat_word) [get_status_word $i [set slave_${i}(402_state)]]
                     }
                  }
                  
                  break;
               }          
            }          
         }          
      }      
   }
}

# slavenum is 1..nslaves
# data is a list of up to 8 data bytes
proc slave_sdo_server {slavenum data sc_cobid} {
   global nslaves debug_canopen   
   global slave_${slavenum}
   
   set nodeid [set slave_${slavenum}(nodeid)]
   set name   [set slave_${slavenum}(name)]
   
   if {$debug_canopen} {dvtConPuts debug "Slave $name has received an SDO - $data"}
   
   set ccs [expr [lindex $data 0]>>5]
   
   switch $ccs {
      1 {
         # Initiate SDO Download
         set n [expr ([lindex $data 0]>>2)&3]
         set e [expr ([lindex $data 0]>>1)&1]
         set s [expr ([lindex $data 0]   )&1]
         
         if {$s==0} {
            if {$debug_canopen} {dvtConPuts debug "Slave $name cannot process SDO download with s=0"}
            return
         }
         if {$e==0} {
            if {$debug_canopen} {dvtConPuts debug "Slave $name cannot process SDO download with e=0"}
            return
         }
         
         set idx  [merge_hex [lrange $data 1 2]]
         set sidx [lindex $data 3]
         set value [merge_hex [lrange $data 4 [expr 7-$n]]]
         
         # Somewhat obscure syntax to perform a regular expression search through the
         # slave's OD for the specified index and subindex.
         set cmd "lsearch -regexp \$slave_${slavenum}(od) {.*{.*$idx $sidx} .* r.}"
         set pos [eval $cmd]
         
         # if configured to abort on SDO received, just send a general abort code (0x08000000)
         if {[set slave_${slavenum}(sdo_abort)]} {
            can send "$sc_cobid [expr 0x80] [expr [lindex $data 1]] [expr [lindex $data 2]] 0 0 0 8"
            if {$debug_canopen} {dvtConPuts debug "Abort message sent for object $idx $sidx"}
            
         } elseif {$pos==-1} {
            # The object doesn't exist - return a 0x0602 0000 abort code.  These slaves don't
            # differentiate between objects not existing and sub-indices not existing.
            can send "$sc_cobid [expr 0x80] [expr [lindex $data 1]] [expr [lindex $data 2]] 0 0 2 6"
            if {$debug_canopen} {dvtConPuts debug "Object $idx $sidx doesnt exist for download"}
         } else {
            if {[lindex [lindex [set slave_${slavenum}(od)] $pos] 2] == "rw"} {
               # Check the length in the SDO download packet matches the length in the OD
               if {[expr 2*(4 - $n)+2] == [string length [lindex [lindex [set slave_${slavenum}(od)] $pos] 1]] } {
                  # Update the slave's OD with the new value.  This is done by replacing the list element
                  # with a new list element containing the new data value.
                  set cmd "lreplace \$slave_${slavenum}(od) $pos $pos \[list \[list $idx $sidx\] $value rw\]"
                  set slave_${slavenum}(od) [eval $cmd]
                  
                  # Send an ACK, unless bad state is set, in which case send an ACK with an invalid state.
                  if {[set slave_${slavenum}(sdo_bad_state)]} {
                      can send "$sc_cobid 0x20 [lindex $data 1] [lindex $data 2] [lindex $data 3] 0x00 0x00 0x00 0x00"
                  } else {
                      can send "$sc_cobid 0x60 [lindex $data 1] [lindex $data 2] [lindex $data 3] 0x00 0x00 0x00 0x00"
                  }
                  
               } else {
                  # Incorrect length - return a 0x0607 0010 abort code
                  can send "$sc_cobid [expr 0x80] [expr [lindex $data 1]] [expr [lindex $data 2]] 16 0 7 6"
                  if {$debug_canopen} {dvtConPuts debug "Incorrect Length Specified for object $idx $sidx, expected [string length [lindex [lindex [set slave_${slavenum}(od)] $pos] 1]] got [expr 2*(4 - $n)+2]"}
               }
            } else {
               # The object is not writeable - return an 0x0601 0002 abort code
               can send "$sc_cobid [expr 0x80] [expr [lindex $data 1]] [expr [lindex $data 2]] [expr [lindex $data 3]] 2 0 1 6"
               if {$debug_canopen} {dvtConPuts debug "Object $idx $sidx is not writable"}
            }
         }
      }
      2 {
         # Initiate SDO Upload

         set idx  [merge_hex [lrange $data 1 2]]
         set sidx [lindex $data 3]
         
         # Somewhat obscure syntax to perform a regular expression search through the
         # slave's OD for the specified index and subindex.
         set cmd "lsearch -regexp \$slave_${slavenum}(od) {.*{.*$idx $sidx} .* r.}"
         set pos [eval $cmd]
         
         if {$pos==-1} {
            # The object doesn't exist - return a 0x0602 0000 abort code.  These slaves don't
            # differentiate between objects not existing and sub-indices not existing.
            can send "$sc_cobid [expr 0x80] [expr [lindex $data 1]] [expr [lindex $data 2]] [expr [lindex $data 3]] 0 0 2 6"
             if {$debug_canopen} {dvtConPuts debug "Object $idx $sidx doesnt exist for upload"}
            
         } else {
            
            # Make the four d bytes
            set o [lindex [set slave_${slavenum}(od)] $pos]
            set d [unmerge_hex [lindex $o 1]]
            set n [expr 4-[llength $d]]
            set d [lrange "$d 0x00 0x00 0x00" 0 3]
            
#            if {$n<1} {
#               if {$debug_canopen} {dvtConPuts "Slave cannot perform segmented upload on object $idx, $sidx
#               \no : $o\nd : $d\nn : $n"}
#               return "Error"
#            } elseif {$n==0} {
#               if {$debug_canopen} {dvtConPuts "Object $idx, $sidx appears to have zero length"}
#               return "Error"
#            }
            
            # Work out the first (control) byte - assuming expedited only
            set b0 [expr 0x40 | ($n<<2) | 0x03]
             if {$debug_canopen} {dvtConPuts debug "Slave $name responds with $sc_cobid $b0 [expr [lindex $data 1]] [expr [lindex $data 2]] [expr [lindex $data 3]] $d to upload request"}
            
            can send "$sc_cobid $b0 [expr [lindex $data 1]] [expr [lindex $data 2]] [expr [lindex $data 3]] $d"
         }
      }
      4 {
            if {$debug_canopen} {dvtConPuts debug "client has aborted previous request to Slave $name"}
      }
      default {
         if {$debug_canopen} {dvtConPuts debug "Slave $name cannot process SDO with ccs=$ccs"}
      }
   }
   
   return
}


# hex2bin
#
# Converts a hexadecimal input parameter in the form 0xNN to an 8-character
# binary equivalent.
#
set helptext(hex2bin) "Converts 8-bit hex number (in 0xNN format) to 8 character binary."
set helptextd(hex2bin) "
SYNOPSIS
   hex2bin hex_number

DESCRIPTION
Converts 8-bit hex number (in 0xNN format) to 8 character binary.
"

proc hex2bin { h } {
   set hexlookup(0) 0000
   set hexlookup(1) 0001
   set hexlookup(2) 0010
   set hexlookup(3) 0011
   set hexlookup(4) 0100
   set hexlookup(5) 0101
   set hexlookup(6) 0110
   set hexlookup(7) 0111
   set hexlookup(8) 1000
   set hexlookup(9) 1001
   set hexlookup(A) 1010
   set hexlookup(B) 1011
   set hexlookup(C) 1100
   set hexlookup(D) 1101
   set hexlookup(E) 1110
   set hexlookup(F) 1111
    
   set op ""
   for {set i 2} {$i < [string length $h]} {incr i} {
       append op $hexlookup([string toupper [string index $h $i]])
   }
   return $op
}

# dec2bin
#
# Converts a number in the range 0 to 255 to binary.
#
set helptext(dec2bin) "Converts 8-bit decimal number to 8 character binary."
set helptextd(dec2bin) "
SYNOPSIS
   dec2bin hex_number

DESCRIPTION
Converts 8-bit decimal number to 8 character binary.
"
proc dec2bin { d } {
   set h [format 0x%02X $d]
   
   return [hex2bin $h]
}




proc slavetx {wpath} {
   set wpath_l [split $wpath .]
   set slave [lindex $wpath_l 1]
   set tpdo_num [string index [lindex $wpath_l 2] end]
   
   puts "TPDO slave:$slave tpdo_num:$tpdo_num"
   co_slave tx_tpdo $slave $tpdo_num
}



# emcytx
#
# Transmits the EMCY telegram for the slave
#
proc emcytx {slave} {
    global $slave

    set cobid    [set [set slave](emcy_cobid)]
    set err_code "0x[format %04X [expr [set [set slave](emcy_co_err_code)]]]"
    set sev_code "0x[format %04X [expr [set [set slave](emcy_sev_code)]]]"
    set dbs      [set [set slave](emcy_db)]


    for {set i 0} {$i < 3} {incr i} {
        set db($i) [lindex $dbs $i]
        if {$db($i) == ""} {set db($i) 0}
        
        set db($i) "0x[format %02X [expr $db($i)]]"
    }   
    
    # send EMCY message. Just leave error byte (byte 2) as 0 since nothing uses it
    can send "$cobid [unmerge_hex $err_code] 0x00 [unmerge_hex $sev_code] $db(0) $db(1) $db(2)"
}



# set_vendor_id
#
# Changes the vendor ID between Sevcon and an unknown one
#
proc set_vendor_id {slave} {
    global $slave

    if {[set [set slave](sevcon_node)]} {
        set [set slave](vendor_id) 0x0000001E
    } else {
        set [set slave](vendor_id) 0x00000000
        set [set slave](prod_code) 0x00000000
        set [set slave](rev_no)    0x00000000
    }
    
    # update object dictionary
   co_update_od_list $slave
}


# co_slave
#
# This is "user-friendly" function to manipulate slaves.
#
# TODO - give the slaves the ability to send synchronous PDOs (in run_slave?)
#
set helptext(co_slave) "Creates and manipulates simulated CANopen slaves"
set helptextd(co_slave) "
SYNOPSIS
    co_slave action slave ?args?

DESCRIPTION
Creates and manipulates simulated CANopen slaves. Following actions are allowed:

    co_slave create       <name> <nodeid> <?cont?> <?ssdo2?> Creates a slave.  Returns a slave handle. The 
                                                             optional 3rd parameter (cont) can be specified 
                                                             to create the slave as a motor controller with 
                                                             DSP402 functionality + TPDOs+RPDOs. The optional 
                                                             4th parameter (ssdo2) can be used to specify 
                                                             server->client and client->server COBIDs for a 
                                                             second SDO server.
    
    co_slave auto_detect  <nodeid> <?pdos_to_ignore?>        Interrogates a master node to work out what slaves
                                                             it is expecting to find, then creates suitable 
                                                             slaves. The functionality expects the master node
                                                             to already be powered up. It also destroys any
                                                             existing slaves. If there are any PDOs already handled
                                                             by other scripts (eg Aichi Simulator), then they
                                                             can be ignored by providing a list of COBIDs to ignore
                                                             during the PDO search.
    
    co_slave tx_tpdo      <slave> <tpdo_num>                 Causes the slave to transmit the TPDO
                                                             number specified by tpdo_num.

    co_slave set_ai       <slave> <ai> <ai_value>            Sets an analog input on the slave.

    co_slave set_di       <slave> <di_value>                 Sets all 8 digital inputs on the slave.

    co_slave set_di_bit   <slave> <bit> <1|0>                Sets bit <bit> to either 1 or 0.  Bit is
                                                             in the range 0 to 7.

    co_slave get_di       <slave>                            Reads the eight digital inputs and returns
                                                             the value in hex.

    co_slave get_ai       <slave> <ai>                       Reads the specified analog input and returns
                                                             the value in hex..

    co_slave get_ao       <slave> <ao>                       Reads the specified analog input and returns
                                                             the value in hex.

    co_slave get_do       <slave>                            Reads the 8 digital outputs.

Slaves are created with co_slave create and are then referred to by the handle returned from
this proc.  The slave must be \"turned on\" with \"co_slave key on\" which will place it in
pre-operational and cause a bootup message to be sent.  The slave then has a simple 
implementation of the NMT slave state machine.  The slave will generate heartbeats if these
are enabled in its OD.

Slaves can be auto-generated from the configuration of the master using the auto-detect
option. If co_slave is called with the auto_detect switch, the DVT will interrogate the node 
(using the node ID passed (which must be a master)) and will work out what PDOs are mapped and 
what slaves are required. It will also handle any secondary SDO servers which are required. One 
slave is created for each motor mapped. One of the slaves handles all the other generic RPDOs 
and TPDOs. PDOs can be ignored during auto-detect by specifying an optional list of COBIDs. 
These PDOs must be handled by another script (eg Aichi Simulator). For example:
    
    co_slave auto_detect 1                      <== Creates slaves for all PDOs used by node 1
    co_slave auto_detect 1 {0x191}              <== Creates slaves for all PDOs used by node 1,
                                                    except for 0x191.

                                                    

If the auto_detect switch is not used, then slaves default to the following set up.

Each slave has a simple implementation of the Generic I/O Profile with these properties:
- eight digital outputs
- three analog outputs, numbered 1 to 3
- eight digital inputs
- three analog inputs, numbered 1 to 3

The mapping for RPDO1 is
   bytes 0, 1: analog input/ouptut 1
   bytes 2, 3: analog input/ouptut 2
   bytes 4, 5: analog input/ouptut 3
   byte  6   : eight digital inputs/outputs
   byte  7   : unused
   
if the slave is created as a controller slave then the mapping of the second tpdo is as follows   

The mapping for TPDO2 and RPDO2 is
   bytes 0, 1      : control/status word
   bytes 2, 3      : target/actual torque
   bytes 4, 5, 6, 7: target/actual velocity


   
To add a second SDO server, specify the server->client and client->server SDOs as a list as the
4th parameter in co_slave create.

For example, the following command create slaves:
    co_slave create test 2                       <== node called test, node id 2
    co_slave create pump 3 cont                  <== motor control node called pump, node id 3
    co_slave create pump 3 cont \{0x503 0x483\}  <== motor control node called pump, node id 3,
                                                     with second server, c->s 0x503 s->c 0x483
"

proc co_slave {cmd slave args} {
   global debug_canopen
   
   if {$cmd != "auto_detect"} {
      global $slave
   }
   
   switch $cmd {
      create {
         # The process of creating a slave is so big that it gets its own proc
         #puts "[lindex $args 0] : [lindex $args 1]"
         return [createslave $slave [lindex $args 0] [lindex $args 1] [lindex $args 2] [lindex $args 3]]
         
      }
      
      auto_detect {
         # kill any existing slaves
         kill_slaves
         
         # the first parameter is the master node ID not slave name
         set nodeid $slave
         
         # the next parameter is an (optional) list of PDOs to ignore during slave PDO mapping. This is useful if another script
         # is already being used to handle the some of the PDOs (eg Aichi Simulator)
         set pdos_to_ignore ""
         if {[llength $args] > 0} {
            
            # reformat all PDOs to ignore as 32-bit hex numbers to make searching easier later
            set pdos_to_ignore ""
            foreach pdo [lindex $args 0] {
                lappend pdos_to_ignore [format "0x%08x" $pdo]
            }
         }
         
         # check that the master is powered and is a master (otherwise there are no slaves)
         if {[sdo_rnx $nodeid 0x1018 0] == "Timeout"} {
            puts "ERROR: Master node ($nodeid) is not communicating on the CANbus. Check node is powered up and retry"
            return Error
         }
         
         lg $nodeid
         if {[sdo_rnx $nodeid 0x5800 0] != 0x01} {
            puts "ERROR: Unable node $nodeid is not configured as a master (0x5800,0 != 0x01). Reconfigure node and retry"
            return Error
         }
         
         # read out slave node IDs
         set n_slaves [sdo_rnx $nodeid 0x2810 0]
         if {$n_slaves == 0} {
            puts "ERROR: No slaves defined in 0x2810,0."
            return Error
         }
         
         puts "Found $n_slaves slaves...."
         set cs_idx 0x1280
         set slave_list ""
         for {set sidx 1} {$sidx <= $n_slaves} {incr sidx} {
            set slave_nid [sdo_rnx $nodeid 0x2810 $sidx]
            set slave_name "slave_[set slave_nid]"
            
            # check for a valid client-server configuration
            if {[set nid [sdo_rnx $nodeid $cs_idx 3]] != $slave_nid} {
                puts "ERROR: Slave node id in $cs_idx, 3 (=$nid) does not match that in 0x2810, $sidx (=$slave_nid). Reconfigure and retry."
                return Error
            }
            
            # read client-server and server-client COBIDs
            set cs_cobid [sdo_rnx $nodeid $cs_idx 1]
            set sc_cobid [sdo_rnx $nodeid $cs_idx 2]
            
            # add new slave to slave list
            set slave_cobids($slave_nid) "$cs_cobid $sc_cobid"
            
            # increment client->server index
            set cs_idx [format "0x%04x" [expr $cs_idx + 1]]
         }
         
         # get list of slave nodeids in ascending order
         set nid_list [lsort [array names slave_cobids]]
         set nid_list_idx 0
         set slave_nid [lindex $nid_list $nid_list_idx]
         
         # ok, now we have a list of slaves, this is the tricky bit. We must try and assign PDOs to these slaves
         # to satisfy all the masters PDO requirements. General RPDOs are easy since we can just give all of these
         # to one node (the master doesn't care who's transmitting them, so long as someone is). The motor control
         # ones are trickier since the slave needs to run the motor control DSP402 FSM, and each slave (at the 
         # moment) can only handle 1 motor control. The way we will so this is to look at all the master RPDOs
         # and find out what motors are mapped in. We then match these to the numbers of slaves (hopefully, there
         # should be enough slaves for 1 motor each) by assigning IDs in the order left traction, right traction,
         # pump then power steer. We must also take care to ensure that mapped motor objects have all data both
         # transmitted and received. Motors with just data transmitted are not mapped, the transmitted data is 
         # probably just for debug
         
         set sidx_ids "na cw sw tgt_max_spd act_spd tgt_max_trq act_trq"
         
         set mapped_to_nodeid(left)   0
         set mapped_to_nodeid(right)  0
         set mapped_to_nodeid(pump)   0
         set mapped_to_nodeid(steer)  0
         set mapped_to_nodeid(none)   0
         
         # loop twice, once for RPDOs and once for TPDOs
         foreach comm {rpdo tpdo} {
            if {$comm == "rpdo"} {
               set pdo_comm_idx 0x1400
               set pdo_map_idx  0x1600
               set slave_comm   tpdo
            } else {
               set pdo_comm_idx 0x1800
               set pdo_map_idx  0x1a00
               set slave_comm   rpdo
            }
            
            # read out sub 0 of the communication object (0x14XX). If a value is returned, the PDO exists.
            while {![catch {expr [sdo_rnx $nodeid $pdo_comm_idx 0]}]} {
               
               # check that bit 31 of COBID is not set and number of mapped items is greater than 0 for an RPDO to be mapped
               set cobid  [sdo_rnx $nodeid $pdo_comm_idx 1] 
               set n_maps [sdo_rnx $nodeid $pdo_map_idx  0] 
               
               # check to see if the PDO map is enabled (bit 31 not set and number of maps > 0), also check to see if this
               # cobid has been marked as one to ignore.
               if {((0x80000000 & $cobid) == 0) && ($n_maps > 0) && ([lsearch $pdos_to_ignore $cobid] == -1)} {
                   # look through all maps to see what is mapped in
                   set motor_type ""
                   set pdo ""
                   set n_bits 0
                   for {set sidx 1} {$sidx <= $n_maps} {incr sidx} {
                       set map [sdo_rnx $nodeid $pdo_map_idx $sidx]
                       set map_idx  "0x[string range $map 2 5]"
                       set map_sidx "0x[string range $map 6 7]"
                       set map_size "0x[string range $map 8 9]"
                        
                        
                       # check for any booleans mapped. We must roll-up all booleans into one or more bytes
                       if {$map_size == 0x01} {
                          incr n_bits
                       } else {
                          # check to see if we got some booleans mapped mapped in previously
                          if {$n_bits > 0} {
                              set n_bytes [expr (($n_bits - 1) / 8) + 1]
                              lappend pdo [list "booleans" $n_bytes ""]
                              set n_bits 0
                          }
                          
                          # check for motor type. Don't set type if the subindex is for a RW item (ie status, actual velocity
                          # or actual torque) set in a TPDO since this won't be part of the motor mapping and is probably just
                          # being output for debug purposes.
                          if {($comm == "tpdo") && (($map_sidx == 0x02) || ($map_sidx == 0x04) || ($map_sidx == 0x06))} {
                              set type none
                          } else {
                              switch $map_idx {
                                  0x2020  {set type left }
                                  0x2021  {set type right}
                                  0x2040  {set type pump }
                                  0x2060  {set type steer}
                                  default {set type none }
                              }
                          }
                          
                    
                          # if a PDO contains any motor mappings, then the entire PDO must be reserved for the motor, otherwise
                          # things get really horrible and complicated and we can't handle it. 
                          if {$motor_type == ""} {
                              set motor_type $type
                          } elseif {$motor_type != $type} {
                              puts "ERROR: PDO map ($pdo_map_idx) is not reserved for just one motor. Found mappings for $motor_type and $type. System cannot handle this."
                              return Error
                          }
                    
                          # build up PDO map info. Motor stuff has a data type (cw, sw, etc) so the slave knows how to unmap it. 
                          # NOTE: For tpdos (from master) a motor is not mapped unless its rpdos (which are checked first) are also mapped.
                          if {$comm == "rpdo"} {
                              if {($motor_type != "none") && ($mapped_to_nodeid($motor_type) == 0)} {
                                  set mapped_to_nodeid($motor_type) $slave_nid
                                  incr nid_list_idx
                                  set slave_nid [lindex $nid_list $nid_list_idx]
                              }
                          }
                       
                          if {($motor_type != "none") && ($mapped_to_nodeid($motor_type) != 0)} {
                              set id [lindex $sidx_ids [expr $map_sidx]]
                              lappend pdo [list "[set motor_type]_[set id]" [expr $map_size / 8] $id]
                          } else {
                              lappend pdo [list "[set map_idx]_[expr $map_sidx]" [expr $map_size / 8] ""]
                          }
                       }
                   }
                   
                   # check to see if we had some booleans at the end of the loop
                   if {$n_bits > 0} {
                       set n_bytes [expr (($n_bits - 1) / 8) + 1]
                       lappend pdo [list "booleans" $n_bytes ""]
                   }
                
                   # add to pdo list. Remember that rpdos for the master are tpdos on the slave and vice versa
                   if {$motor_type == "none"} {
                       lappend pdos([lindex $nid_list 0]) [list $slave_comm $cobid 0 $pdo]
                   } else {
                       lappend pdos($mapped_to_nodeid($motor_type)) [list $slave_comm $cobid 1 $pdo]
                   }
               }

               # increase index's
               set pdo_comm_idx [format "0x%04x" [expr $pdo_comm_idx+1]]
               set pdo_map_idx  [format "0x%04x" [expr $pdo_map_idx +1]]
            }
         }
         
         # now create the slaves. Make all slaves motor controllers
         foreach slave_nid $nid_list {
             # create slave using this data. Make all slaves motor controllers
             if {[info exists pdos($slave_nid)]} {
                set result [createslave "slave_[set slave_nid]" $slave_nid cont $slave_cobids($slave_nid) $pdos($slave_nid)]
             } else {
                set result [createslave "slave_[set slave_nid]" $slave_nid cont $slave_cobids($slave_nid) ""]
             }
             
             if {$result == ""} {
                 puts "ERROR: Unable to create slave for $slave_nid using client-server COBIDs (cs=$cs_cobid, sc=$sc_cobid). createslave returned $result."
                 return Error
             }
        
             puts "Created slave_[set slave_nid] (nodeid=$slave_nid). Client->server COBIDs are $slave_cobids($slave_nid)."
         }
      }
      
      key_on {
          
         if {![set [set slave](key_tog)]} {
             .[set slave].key configure -text "Key Off"
             set [set slave](key_tog) 1
             # Change slave state to preoperational
             set ${slave}(nmtstate) NMT_PREOPERATIONAL
             
             # Send the bootup message
             set nodeid [set ${slave}(nodeid)]
             can send "[format 0x%04X [expr 1792+$nodeid]] 0x00"
             
             # Start producing heartbeats.  Assumes slave arrays are called slave_X
             set slavenum [string range $slave 6 end]
             heartbeat_slave $slavenum
         } else {
             set [set slave](key_tog) 0
             .[set slave].key configure -text "Key On"
             co_slave key_off $slave
         }
      }
      
      key_off {
         .[set slave].key configure -text "Key On"
          
         # Change slave state to preoperational
         set ${slave}(nmtstate) NMT_INITIALISING
         
         # put motor slave into initialising
         if {[info exists ${slave}(402_state)]} {
             set ${slave}(cont_word)     "0x0000"
             set ${slave}(targ_v)        "0x00000000"
             set ${slave}(actual_v)      "0x00000000"        
             set ${slave}(targ_torq)     "0x0000"
             set ${slave}(actual_torq)   "0x0000"
             set ${slave}(stat_word)     "0x0000"
             set ${slave}(402_state)     "MOTOR_NOT_READY_TO_SWITCH_ON"
         }
         
         
         # disable caps precharge
         set idx  0x5180
         set sidx 0x00
         set value 0x00
         set cmd "lsearch -regexp \[set ${slave}(od) \] {.*{.*$idx $sidx} .* r.}"
         set pos [eval $cmd]
         
         set cmd "lreplace \[set ${slave}(od) \] $pos $pos \[list \[list $idx $sidx\] $value rw\]"
         set ${slave}(od) [eval $cmd]
         
      }
      
      tx_tpdo {
         # transmit all TPDOs
         if {[set ${slave}(nmtstate)] == "NMT_OPERATIONAL"} {
            for {set i 1} {$i <= [set ${slave}(n_tpdos)]} {incr i} {
               set bytes [set ${slave}(tpdo[set i]_cobid)]
            
               # for generic TPDOs, just map the data in and transmit
               if {![set ${slave}(tpdo[set i]_mtr)]} {
                
                  foreach item [set ${slave}(tpdo[set i]_data)] {
                     set name  [lindex $item 0]
                     set size  [lindex $item 1]
                     set value [set ${slave}(tpdo[set i]_[set name])]
                    
                     # check for part entered data
                     if {([string length $value] != [expr 2+($size*2)]) || ([catch {expr $value}])} {
                        set bytes ""
                        break;
                     } else {
                        for {set b 0} {$b < $size} {incr b} {
                           lappend bytes [expr $value & 0xFF]
                           set value     [expr $value >> 8]
                        }
                     }
                  }
               } else {
                  # this is a motor controller so we are just transmitting statusword, actual speeds+torques
                
                  foreach item [set ${slave}(tpdo[set i]_data)] {
                     set size  [lindex $item 1]
                     set type  [lindex $item 2]
                     
                     switch $type {
                        sw      {set value [set ${slave}(stat_word)]}
                        act_trq {set value [set ${slave}(actual_torq)]}
                        act_spd {set value [set ${slave}(actual_v)]}
                        default {set value 0}
                     }
                    
                     # check for part entered data
                     if {([string length $value] != [expr 2+($size*2)]) || ([catch {expr $value}])} {
                        set bytes ""
                        break;
                     } else {
                        for {set b 0} {$b < $size} {incr b} {
                           lappend bytes [expr $value & 0xFF]
                           set value     [expr $value >> 8]
                        }
                     }
                  }
               }
            
               # send to CANbus
               if {$bytes != ""} {
                  can send $bytes
               }
            }          
         }          
      }
      set_ai {
         # TODO - Check values for ai and ai_value
         set ai [lindex $args 0]
         set ai_value [format 0x%04X [lindex $args 1]]
         eval set ${slave}(ai${ai}_hex) $ai_value
      }
      set_di {
         # TODO - Check values for di_value
         set di_value [hex2bin [format 0x%02X [lindex $args 0]]]
         eval set ${slave}(di_binary) $di_value
      }
      set_di_bit {
         # TODO - Check values for bit and newbit
         set bit [lindex $args 0]
         set newbit [lindex $args 1]
         set di_value [string replace [set ${slave}(di_binary)] [expr 7-$bit] [expr 7-$bit] $newbit]
         eval set ${slave}(di_binary) $di_value
      }
      get_di {
         # TODO - Check values for di_value
         return [bin2hex [set ${slave}(di_binary)]]
      }
      get_ai {
         # TODO - Check values for ai
         set ai [lindex $args 0]
         return [set ${slave}(ai${ai}_hex)]
      }
      get_ao {
         # TODO - Check values for ao
         set ao [lindex $args 0]
         return [set ${slave}(ao${ao}_hex)]
      }
      get_do {
         return [set ${slave}(do_binary)]
      }      
      default {
         dvtConPuts stderr "Unknown option to slave  $slave, ($cmd)"
         return error
      }
   }
   
   return
}

# bin2hex
# 
# Converts an arbitrary-length binary string into a hex number of the
# form 0xNN.  The minimum number of hex digits required to maintain the
# value is used.
#
set helptext(bin2hex) "Converts binary number to hex number in format 0xN..N."
set helptextd(bin2hex) "
SYNOPSIS
   bin2hex binary_number

DESCRIPTION
Converts binary number to hex number in format 0xN..N. Number of N's depends
on number of binary digits
"
proc bin2hex {b} {
   set blookup(0000) 0
   set blookup(0001) 1
   set blookup(0010) 2
   set blookup(0011) 3
   set blookup(0100) 4
   set blookup(0101) 5
   set blookup(0110) 6
   set blookup(0111) 7
   set blookup(1000) 8
   set blookup(1001) 9
   set blookup(1010) A
   set blookup(1011) B
   set blookup(1100) C
   set blookup(1101) D
   set blookup(1110) E
   set blookup(1111) F
   
   # Add leading zeros as necessary to make an integer multiple of 4 bits
   if {[string length $b]%4 != 0} {
      set b "[string repeat 0 [expr 4-([string length $b]%4)] ]$b"
   }
   
   set h "0x"
   for {set i 0} {$i<[string length $b]} {incr i 4} {
      set h "$h$blookup([string range $b $i [expr $i+3]])"
   }
   return $h
}

#
#
# Decode emcy
#
#

proc decode_emcy { data } {
    
    set msg ""
        
    set emcy_msg(0x00)     "Error Reset or No Error"               ;#00xx 
    set emcy_msg(0x10)     "Generic Error"                         ;#10xx 
    set emcy_msg(0x20)     "Current"                               ;#20xx 
    set emcy_msg(0x21)     "Current, device input side"            ;#21xx 
    set emcy_msg(0x22)     "Current inside the device"             ;#22xx 
    set emcy_msg(0x23)     "Current, device output side"           ;#23xx 
    set emcy_msg(0x30)     "Voltage"                               ;#30xx 
    set emcy_msg(0x31)     "Mains Voltage"                         ;#31xx 
    set emcy_msg(0x32)     "Voltage inside the device"             ;#32xx 
    set emcy_msg(0x33)     "Output Voltage"                        ;#33xx 
    set emcy_msg(0x40)     "Temperature"                           ;#40xx 
    set emcy_msg(0x41)     "Ambient Temperature"                   ;#41xx 
    set emcy_msg(0x42)     "Device Temperature"                    ;#42xx 
    set emcy_msg(0x50)     "Device Hardware"                       ;#50xx 
    set emcy_msg(0x60)     "Device Software"                       ;#60xx 
    set emcy_msg(0x61)     "Internal Software"                     ;#61xx 
    set emcy_msg(0x62)     "User Software"                         ;#62xx 
    set emcy_msg(0x63)     "Data Set"                              ;#63xx 
    set emcy_msg(0x70)     "Additional Modules"                    ;#70xx 
    set emcy_msg(0x80)     "Monitoring"                            ;#80xx 
    set emcy_msg(0x81)     "Communication"                         ;#81xx 
    set emcy_msg_81(0x10)  "CAN Overrun (Objects lost)"            ;#8110 
    set emcy_msg_81(0x20)  "CAN in Error Passive Mode"             ;#8120 
    set emcy_msg_81(0x30)  "Life Guard Error or Heartbeat Error"   ;#8130 
    set emcy_msg_81(0x40)  "recovered from bus off"                ;#8140 
    set emcy_msg_81(0x50)  "Transmit COB-ID collision"             ;#8150 
    set emcy_msg(0x82)     "Protocol Error"                        ;#82xx 
    set emcy_msg_82(0x10)  "PDO not processed due to length error" ;#8210 
    set emcy_msg_82(0x20)  "PDO length exceeded"                   ;#8220 
    set emcy_msg(0x90)     "External Error"                        ;#90xx 
    set emcy_msg(0xF0)     "Additional Functions"                  ;#F0xx 
    set emcy_msg(0xFF)     "Device specific"                       ;#FFxx 
    
    set sevcon_code "[lindex $data 3] [lindex $data 4]"    
    set co_error_code [lindex $data 1]
    set co_sub_code [lindex $data 0]
    
    if {[lsearch [array names emcy_msg] $co_error_code] != -1} {        
        if {$co_error_code == "0x81"} {        
            set msg "$emcy_msg(0x81) "
            if {[lsearch [array names emcy_msg_81] $co_sub_code] != -1} {                    
                 append msg "$emcy_msg(0x81) : $emcy_msg_81($co_sub_code)"
             }
             append msg " Sevcon Code: [merge_hex $sevcon_code] "             
        } elseif {$co_error_code  == "0x82"} {
            set msg "$emcy_msg(0x82) "
            if {[lsearch [array names emcy_msg_82] $co_sub_code] != -1} {                    
                 append msg "$emcy_msg(0x82) : $emcy_msg_82($co_sub_code)"
             }
             append msg " Sevcon Code: [merge_hex $sevcon_code] "             
        } else {
            set msg "$emcy_msg([lindex $data 1]). Sevcon code: [merge_hex $sevcon_code]"
        }   
    } else {
        set msg "unknown error code $co_error_code Sevcon code: [merge_hex $sevcon_code]"
    }
    return $msg
}


# drives and motion controls state machine and control word decoder see dsp402,  adpated from winio test
# harness
proc 402_fsm { slave current_state 402_event } {
    global slave_${slave}
    
    set new_state $current_state
    
    # perform any required initialisation first
    if {$current_state == "MOTOR_START"} {
        set new_state MOTOR_NOT_READY_TO_SWITCH_ON
    }
    
    if {$current_state == "MOTOR_NOT_READY_TO_SWITCH_ON"} {
    
        # stick in initialisation state if checkbox checked
        if {![set slave_${slave}(stick_in_init)]} {
            set new_state  MOTOR_SWITCH_ON_DISABLED
        }
    }
    
    # run the normal FSM
    switch $current_state {       

         MOTOR_START - MOTOR_NOT_READY_TO_SWITCH_ON {
                # do nothing here
            }
    
        MOTOR_SWITCH_ON_DISABLED {
                if {$402_event == "SHUTDOWN"} {
                    set new_state  MOTOR_READY_TO_SWITCH_ON
                }
            }
    
        MOTOR_READY_TO_SWITCH_ON {
                if {$402_event == "SWITCH_ON1"} {
                    set new_state  MOTOR_SWITCHED_ON
                } elseif {$402_event == "SWITCH_ON2"} {
                    set new_state MOTOR_OPERATION_ENABLE
                } elseif {($402_event == "DISABLE_VOLTAGE") || ($402_event == "QUICK_STOP")} {
                    set new_state MOTOR_SWITCH_ON_DISABLED
                }
            }
    
        MOTOR_SWITCHED_ON {
                if { $402_event == "ENABLE_OPERATION" } {
                    set new_state  MOTOR_OPERATION_ENABLE
                } elseif { $402_event == "SHUTDOWN" } {
                    set new_state MOTOR_READY_TO_SWITCH_ON
                } elseif {($402_event == "DISABLE_VOLTAGE") || ($402_event == "QUICK_STOP")} {
                    set new_state MOTOR_SWITCH_ON_DISABLED
                }
            }
    
        MOTOR_OPERATION_ENABLE {
                if { $402_event == "DISABLE_OPERATION" } {
                    set new_state MOTOR_SWITCHED_ON
                } elseif { $402_event == "SHUTDOWN" } {
                    set new_state MOTOR_READY_TO_SWITCH_ON
                } elseif {$402_event == "DISABLE_VOLTAGE"} {
                    set new_state MOTOR_SWITCH_ON_DISABLED
                } elseif {$402_event == "QUICK_STOP"} {
                    set new_state MOTOR_QUICK_STOP_ACTIVE
                }
            }
    
        MOTOR_QUICK_STOP_ACTIVE {
                if {$402_event == "QUICK_STOP_COMPLETE"} {
                    set new_state MOTOR_SWITCH_ON_DISABLED
                } elseif {$402_event == "DISABLE_VOLTAGE"} {
                    set new_state MOTOR_SWITCH_ON_DISABLED
                }
            }
        
        MOTOR_FAULT {
                # do nothing
            }
    
    
        default:
            {
                error "Invalid DSP402 state : $current_state"
            }
    
    }
    
    return $new_state
}


# procedure to convert out dsp402 state into a valid status word
proc get_status_word { slave 402_state } {
    global slave_${slave}
    
    # set main state
    set  decode_state(MOTOR_START)                      0x0000
    set  decode_state(MOTOR_NOT_READY_TO_SWITCH_ON)     0x0000
    set  decode_state(MOTOR_SWITCH_ON_DISABLED)         0x0040
    set  decode_state(MOTOR_READY_TO_SWITCH_ON)         0x0021
    set  decode_state(MOTOR_SWITCHED_ON)                0x0023
    set  decode_state(MOTOR_OPERATION_ENABLE)           0x0027
    set  decode_state(MOTOR_QUICK_STOP_ACTIVE)          0x0007
    set  decode_state(MOTOR_FAULT)                      0x0008
    set  sw $decode_state($402_state) 
    
    # add any other flags
    if {[set slave_${slave}(allow_slow_sw)]} {
        set sw [expr $sw | 0x4000]
    }
    
    return [format 0x%04X $sw]
}

# routine to read a slaves OD
proc co_read_slave_od_item {slave idx sidx} {

   global $slave
   # Search through the slave's OD for the specified index and subindex.
   set pos [lsearch -regexp [set ${slave}(od)] ".*{.*$idx $sidx} .* r." ]

   # Get value from od
   return [lindex [set ${slave}(od)] $pos 1]
}


co_reset

