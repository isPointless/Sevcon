###############################################################################
# (C) COPYRIGHT Sevcon Limited 2004
# 
# CCL Project Reference C6944 - Magpie
# 
# FILE
#     $ProjectName:DVT$
#     $Revision:1.30$
#     $Author:jw$
# 
# ORIGINAL AUTHOR
#     Martin Cooper
# 
# DESCRIPTION
#     Implements a simple CANopen SDO client.
# 
# REFERENCES
#     C6944-TM-191, C6944-TM-171
# 
# MODIFICATION HISTORY
#     $Log:  26967: sdo_client.tcl 
# 
#     Rev 1.30    14/10/2008 11:56:56  jw
#  Expedited domain responses should be <=4 bytes - was <4 bytes
# 
#     Rev 1.29    09/10/2008 17:05:20  ceh
#  Added variable type column to table so that formatted reads can determine if
#  a variable is signed and format it appropriately. 
#  Also sdo_rns modified to handle strings of less than 4 characters.
# 
#     Rev 1.28    14/05/2007 15:13:14  ps
#  Added sdo_copy function for copying driveability profiles, PDO
#  configurations, etc...
# 
#     Rev 1.27    23/04/2007 11:27:00  ceh
#  Updated help text to standard format.
# 
#     Rev 1.26    04/04/2007 15:33:24  ceh
#  Display any printable character. (+ symbol was being ignored).
# 
#     Rev 1.25    13/02/2007 17:14:22  ceh
#  Added new procs to perform formatted sdo reads and to perform multi-profile
#  reads and writes.
# 
#     Rev 1.24    14/11/2006 17:03:20  ps
#  Change to allow input to sdo_abort_decode to include the "Abort ..." text.
#  Also include warning about the dangers of comitting too many PDOs to EEPROM.
# 
#     Rev 1.23    08/11/2006 13:40:32  ceh
#  Added module revision registering.
# 
#     Rev 1.22    24/08/2006 09:24:28  ceh
#  Modified sdo_ro so that it outputs subindices even if there are more than
#  that suggested by subindex 0. Similar to CMP's mod, except it does it each
#  time, rather than being optional. Useful for objects with configurable number
#  of subindices (VPDOs, etc)
# 
#     Rev 1.21    30/05/2006 13:10:40  ceh
#  Updated CLI handling to cope with multiple CLI slaves
# 
#     Rev 1.20    31/01/2006 14:46:54  cmp
#  force sdo_ro to read outside the range indeicated by sidx 0 (optional)
# 
#     Rev 1.19    07/12/2005 09:47:16  cmp
#  filter cli resposne to remove any unprintable characters
# 
#    Rev 1.18    08/06/2005 09:29:20  ceh
#  Only display truncated string warning in sdo_rns if debug_canbus is set.

# 
#    Rev 1.17    25/05/2005 11:15:38  cmp
#  check correct dll version is availble

# 
#    Rev 1.16    23/05/2005 15:55:24  cmp
#  sdo_client low level calls now call the dll,  sdo_timeout read-writes
#  redirected to the dll

# 
#    Rev 1.15    21/10/2004 14:25:04  ceh
#  sdo_ro now also displays decimal version of values.

# 
#    Rev 1.14    16/09/2004 17:21:44  ceh
#  Fix sdo_ro to handle objects with WO subindices.

# 
#    Rev 1.13    15/09/2004 14:32:40  ceh
#  Fix sdo_ro to handle objects with missing sub-indices. (CANopen allows this.)

# 
#    Rev 1.12    09/09/2004 14:08:50  ceh
#  Added new command (sdo_ro) to read all subindices of an object.

# 
#    Rev 1.11    04/08/2004 09:50:26  cmp
#  reduced sdo timeout back to 1s

# 
#    Rev 1.10    29/07/2004 10:55:20  ceh
#  sdo_rns truncates strings when it encounters a NULL terminator and prints a
#  warning message

# 
#    Rev 1.9    06/07/2004 12:19:10  mdc
#  Added 16-bit SDO read and write (useful for self-characterisation).

# 
#    Rev 1.8    24/06/2004 15:35:52  cmp
#  increased timeout to 5000 mS

# 
#    Rev 1.7    23/06/2004 10:35:34  cmp
#  increased sdo timeout to 2S

# 
#    Rev 1.6    27/05/04 22:39:00  mdc
#  Added help text.

# 
#    Rev 1.5    27/05/2004 16:55:46  mdc
#  Correction to segmented transfers - first four bytes are the overall length.

# 
#    Rev 1.4    06/05/2004 14:47:56  mdc
#  sdo_wn, sdo_wnx, sdo_rn and sdo_rnx commands added.

# 
#    Rev 1.3    29/04/2004 18:10:04  mdc
#  Updated after tests with the USB-to-CAN Compact: data bytes to can send are
#  in decimal.

# 
#    Rev 1.2    28/04/2004 17:52:26  mdc
#  Expedited and normal SDO upload added.

# 
#    Rev 1.1    27/04/04 22:38:36  mdc
#  Added expedited SDO downloads.

# 
#    Rev 1.0    27/04/2004 17:57:04  mdc
#  SDO client for the DVT system.

# 
###############################################################################

# register module
register DVT [info script] {$Revision:1.30$}

# The math package is needed for the "min" function
package require math

# sdo_timeout is the period in milliseconds which the client waits for a response
# from the sever.  This can be overriden if needed.



#
# modify the sdo_timeout time
#
proc change_timeout {args} {
    global sdo_timeout        
    if {[lindex $args 2] == "write"} {
        Ccan set_timeout $sdo_timeout        
    } elseif {[lindex $args 2] == "read"} {
        set sdo_timeout [Ccan get_timeout]        
    }    
}
    
trace add variable sdo_timeout write {change_timeout}
trace add variable sdo_timeout read {change_timeout}
global sdo_timeout
set sdo_timeout 1000


###############################################################################
# sdo_w
#
# Performs an SDO write (a download in CANopen language).
#
# cobid_out    COB-ID                        (hex or decimal)
# cobid_in     COB-ID                        (hex or decimal)
# idx          Index into the server's OD    (hex or decimal)
# sidx         Subindex                      (hex or decimal)
# data         The bytes to download         (hex or decimal)
#
# Example:
#  sdo_w 0x1111 0x2222 0x1234 0x56 1 2 3 4 5 6 7 8 9 10 11 12
#
# The SDO client performs an expedited transfer if the length
# of the data is four bytes or less; otherwise, a segmented transfer
# is used.
#
# All SDO packets are 8 bytes long.  They are padded with zeros if
# there is insufficient data to fill the data fields.
#
proc sdo_w {cobid_out cobid_in idx sidx args} {
   global sdo_server_response
   global cobid_handler_cobid cobid_handler_handler
   global sdo_timeout
   global debug_canopen

    if {[package present Dvtint] != 2.0} {
        puts stderr "This combination of sdo_client.tcl and DVTint.dll is incompatible, get DVTint.dll 2.0+"
        error "Cannot perform sdo write,  dll too old"
    }
   
      
   set l_tot [llength $args]

   if {$l_tot==0} {
      if {$debug_canopen} {dvtConPuts debug "No data to write"}
      return "No data to write"        
   }

   # Make sure the args are in decimal (the DLL doesn't like the
   # 0x prefix at present.
   set bytes ""
   foreach arg $args {
      if {$arg > 255} {
         if {$debug_canopen} {dvtConPuts debug "Invalid hex byte ($arg)"}
         return "Invalid hex byte"        
      }
      lappend bytes [expr $arg]
   }
   
    # call dvtint.dll to perform sdo read
    set result [Ccan sdo_w $cobid_out $cobid_in $idx $sidx $bytes]
    
    # modify endianness if read aborted
    if {[string match Abort* $result]} {
       set d [merge_hex [string range $result 6 end]]
       if {$debug_canopen} {dvtConPuts debug "Abort $d - [sdo_abort_decode $d]"}
       set result "Abort $d"
    }
    return $result


   return $result
}


###############################################################################
# sdo_client_handler
#
# This proc is called from co_poll when the correct COB-ID is received.
#
proc sdo_client_handler {msg} {
   global sdo_server_response
   
   set sdo_server_response $msg
}



###############################################################################
# sdo_r
#
# Performs an SDO read (an upload in CANopen language).
#
# cobid_out    COB-ID                        (hex or decimal)
# cobid_in     COB-ID                        (hex or decimal)
# idx          Index into the server's OD    (hex or decimal)
# sidx         Subindex                      (hex or decimal)
#
# Example:
#  sdo_r 0x1111 0x2222 0x1234 0x56
#
# All SDO packets are 8 bytes long.  They are padded with zeros if
# there is insufficient data to fill the data fields.
#
proc sdo_r {cobid_out cobid_in idx sidx} {
    
    global debug_canopen
   
    if {[package present Dvtint] != 2.0} {
        puts stderr "This combination of sdo_client.tcl and DVTint.dll is incompatible, get DVTint.dll 2.0+"
        error "Cannot perform sdo read,  dll too old"
    }
 
    # call dvtint.dll to perform sdo read
    set result [Ccan sdo_r $cobid_out $cobid_in $idx $sidx]
    
    # modify endianness if read aborted
    if {[string match Abort* $result]} {
       set d [merge_hex [string range $result 6 end]]
       if {$debug_canopen} {dvtConPuts debug "Abort $d - [sdo_abort_decode $d]"}
       set result "Abort $d"
    }
    return $result
}


###############################################################################
# sdo_rn
#
# As sdo_r but connects to the default SDO server on the node ID specified.  By 
# using the default SDO server (server 1), the COB-IDs can be inferred from the
# Node ID.
#
proc sdo_rn {nodeid idx sidx} {
   set r [sdo_r [expr 0x600+$nodeid] [expr 0x580+$nodeid] $idx $sidx]
   
   return $r
}


###############################################################################
# sdo_wn
#
# As sdo_w but connects to the default SDO server on the node ID specified.  By 
# using the default SDO server (server 1), the COB-IDs can be inferred from the
# Node ID.
#
proc sdo_wn {nodeid idx sidx args} {
   set c "sdo_w [expr 0x600+$nodeid] [expr 0x580+$nodeid] $idx $sidx [join $args]"   
   set r [eval $c]
   
   return $r
}


###############################################################################
# sdo_rnx
#
# As sdo_rn but interprets the uploaded data as a single hex number.  The width of
# the hex number is determined by the number of bytes returned from the server.
# The endianness of the server's response is converted to the usual hex
# representation.
#
set helptext(sdo_rnx) "Performs an SDO read (upload) of a numeric object"
set helptextd(sdo_rnx) "
SYNOPSIS
   sdo_rnx nodeid idx sidx

DESCRIPTION
Reads a single index, subindex and returns the value as a hex number.
"
proc sdo_rnx {nodeid idx sidx} {
   set r [sdo_rn $nodeid $idx $sidx]
   
   # If r doesn't start "0x", consider it an error in sdo_r
   if {[string range $r 0 1] == "0x"} {
   
      # If more than four bytes were returned, this indicates a segmented transfer.  In
      # this case, the first four bytes indicate the length
      if {[llength $r] > 4} {
         # A segmented response - the "real" data starts at position 4
         set server_length [merge_hex [split [lrange $r 0 3  ]]]
         set server_data   [merge_hex [split [lrange $r 4 end]]]
         
         if {$server_length != [expr [llength $r]-4]} {
            if {$debug_canopen} {dvtConPuts debug "sdo_rnx: server length disagrees with returned data"}
         }
         
         return $server_data
      } else {
         # An expedited response - concatenate the hex bytes into a single hex number
         return [merge_hex [split $r]]
      }
      
   } else {
   
      # Pass on the error message from sdo_r
      return $r
      
   }
   
}


###############################################################################
# sdo_rnx_fmt
#
# Like sdo_rnx but also shows object name and formated value
#
set helptext(sdo_rnx_fmt) "Performs an SDO read (upload) of a numeric object and supplies name and formatted value"
set helptextd(sdo_rnx_fmt) "
SYNOPSIS
   sdo_rnx_fmt nodeid idx sidx

DESCRIPTION
Like sdo_rnx but also shows object name and formatted value, using data defined in object_data.tcl
"
proc sdo_rnx_fmt {nodeid idx sidx} {
    set r [sdo_rnx $nodeid $idx $sidx]

    # If r doesn't start "0x", consider it an error in sdo_rnx
    if {[string range $r 0 1] == "0x"} {

        # get object data
        set obj_data [get_object_data $idx $sidx]
        
        # append index and subindex to name and get other object information
        set name    [lindex $obj_data 0]
        set scaling [lindex $obj_data 1]
        set unit    [lindex $obj_data 3]
        set type    [lindex $obj_data 4]
        set enum    [lindex $obj_data 5]
        
        # check for an unrecognised object
        if {[string range $name 0 6] == "Unknown"} {
            return $r
        } else {
            
            # if enum is not set, return scaled value, otherwise return enuerated value
            if {$enum == ""} {
                # check using type if number is signed or not. If so, apply correct sign. type has format I/U 8/16/32
                # so I8 = signed 8-bit, U32 = unsigned 32-bit, etc
                if {[string range $type 0 0] == "I"} {
                    switch [string range $type 1 end] {
                        8       {set max 127;        set type_max 256.0       }
                        16      {set max 32767;      set type_max 65536.0     }
                        32      {set max 2147483647; set type_max 4294967296.0}
                        default {set max 0;          set type_max 0.0         }
                    }
                    
                    if {$r > $max} {
                        set val [expr double($r) - $type_max]
                    } else {
                        set val [expr $r]
                    }
                } else {
                    set val [expr $r]
                }
                return "$name: [expr $val * $scaling][set unit] ($r)"
            } else {
                # return enumeration
                set value [lindex $enum $r]
                if {$value != ""} {
                    return "$name: $value ($r)"
                } else {
                    return "$name: Index ($r) too big"
                }
            }
        }
        
    } else {

        # Pass on the error message from sdo_r
        return $r
    }
}


###############################################################################
# dm_sdo_rnx
#
# as sdo_rmx but for multi-profile devices

set helptext(dm_sdo_rnx) "Performs an SDO read (upload) of a numeric object from multiple profiles"
set helptextd(dm_sdo_rnx) "
SYNOPSIS
   dm_sdo_rnx nodeid idx sidx

DESCRIPTION
As sdo_rnx but data is read from each profile listed in dm_profile_list
"
proc dm_sdo_rnx {nodeid idx sidx} {
    
    if {[set idx_list [dm_index_list $idx]] == "Error"} {
        return "Error"
    }
    
    set reply ""
    foreach obj_idx $idx_list {
        append reply "$obj_idx,$sidx: [sdo_rnx $nodeid $obj_idx $sidx]\n"
    }
    
    return $reply
}


###############################################################################
# dm_sdo_rnx_fmt
#
# as sdo_rnx_fmt but for multi-profile devices

set helptext(dm_sdo_rnx_fmt) "Performs an SDO read (upload) of a numeric object from multiple profiles and supplies name and formatted value"
set helptextd(dm_sdo_rnx_fmt) "
SYNOPSIS
   dm_sdo_rnx_fmt nodeid idx sidx

DESCRIPTION
Like dm_sdo_rnx but also shows object name and formatted value, using data defined in object_data.tcl
"
proc dm_sdo_rnx_fmt {nodeid idx sidx} {
    
    if {[set idx_list [dm_index_list $idx]] == "Error"} {
        return "Error"
    }
    
    set reply ""
    foreach obj_idx $idx_list {
        append reply "$obj_idx,$sidx: [sdo_rnx_fmt $nodeid $obj_idx $sidx]\n"
    }
    
    return $reply
}


###############################################################################
# sdo_ro
#
# Reads all object subindices in the format returned by sdo_rnx.
#
set helptext(sdo_ro) "Performs an SDO read (upload) of all subindices of an object"
set helptextd(sdo_ro) "
SYNOPSIS
   sdo_ro nodeid idx ?start? ?use_fmt?

DESCRIPTION
Reads all object subindices in the format returned by sdo_rnx. start is used to specify
the subindex to start from. use_fmt is only used by sdo_ro_fmt and should not be set.
"
proc sdo_ro {nodeid idx {start 0} {use_fmt 0}} {
   global debug_canopen
   
   # a list of possible abort codes where subindices do not exist
   # Application s/w CANopen stacks return 0x06090011 for none existent
   # subindices, whereas bootloaders return 0x0602000.
   set abort_list [list "Abort 0x06090011" "Abort 0x06020000"]
   
   # check if there is anything at sidx 1. If not, then sidx 0 contains
   # the object value, otherwise sidx 0 contains the number of subindices.
   # If error returned indicates sidx 1 is a WO object, then read sidx 0
   # anyway.
   set r [sdo_rnx $nodeid $idx 1]
   
   if {[lsearch $abort_list $r] != -1} {
      # only 1 subindex
      set n_sidx 1
   
   } elseif {([string range $r 0 1] != "0x") && ($r != "Abort 0x06010001")} {
      # an error has occurred, so just return it
      if {$debug_canopen} {dvtConPuts debug "sdo_ro: unable to check for presence of sidx 1"}
      return $r
   
   } else {
       set r [sdo_rnx $nodeid $idx 0]
       if {[string range $r 0 1] != "0x"} {
          # an error has occurred, so just return it
          if {$debug_canopen} {dvtConPuts debug "sdo_ro: unable to read number of subindices"}
          return $r
      
       } elseif {[expr $r] > 255} {
          # some other error message returned. Number of sidxs should be less than 255.
          if {$debug_canopen} {dvtConPuts debug "sdo_ro: unable to read number of subindices"}
          return $r
       
       } else {
          set n_sidx [expr $r + 1]
       }
   }
   
   
   # loop round reading out each subindex
   set output ""
   set err ""
   set rd ""
   set sidx $start
   set finished 0
   
   # wait until all items are read
   while {!$finished} {
      if {$err == ""} {
         # check if a formatted read should be used
         if {!$use_fmt} {
            set r [sdo_rnx $nodeid $idx $sidx]
         } else {
            set r [sdo_rnx_fmt $nodeid $idx $sidx]
         }
         
         # check for an error being return. Ignore sub-index does not exist errors since
         # sometimes subindices can be skipped in objects.
         if {[lsearch $abort_list $r] != -1} {
            
            # subindex is not found. If sidx is >= than n_sidx, then we've found the end
            # of the object so break out and return. Output a warning if we've read out more
            # subindices than we expected. There might be a bug in the OD
            if {$sidx < $n_sidx} {
                set r "<Subindex does not exist>"
            } else {
                if {$sidx > $n_sidx} {
                    dvtConPuts stderr "WARNING: Read [expr $sidx - 1] subindices, but subindex 0 indicates only [expr $n_sidx - 1] was expected."
                }
                set finished 1
            }
         
         } elseif {$r == "Abort 0x06010001"} {
            set r "<Subindex is write only>"
         } elseif {([string range $r 0 1] != "0x") && !$use_fmt} {
            set err $r
         } elseif {!$use_fmt} {
            set rd [expr $r]
         }
         
      } else {
         set r ""
         set rd ""
         
         if {$sidx >= $n_sidx} {
             set finished 1
         }
      }
      
      if {!$finished} {
         if {$rd != ""} {
            append output "$idx,$sidx: $r ($rd)\n"
         } else {
            append output "$idx,$sidx: $r\n"
         }
         set rd ""
      }
      
      incr sidx
   }
   
   return $output
}


###############################################################################
# sdo_copy
#
# Copies the contents of one object into another, providing all destination
# objects are of the same type of the source. Useful for duplicating items such
# as driveability profiles and PDO parameters
#
set helptext(sdo_copy)  "Copies the contents of one object into another"
set helptextd(sdo_copy) "Copies the contents of one object into another"
proc sdo_copy { source_node source_object dest_node dest_object } {
    
    # First some checks to ensure destination is same format as source
    set num_objects [ sdo_rnx $source_node $source_object 0 ]
    if { $num_objects != [ sdo_rnx $dest_node $dest_object 0 ] } {
        return [ error "Cannot do sdo_copy - size mismatch between node $source_node object $source_object and node $dest_node object $dest_object" ]
    }
    
    for { set n 1 } { $n <= $num_objects } { incr n } {
        if { [ string length [ sdo_rnx $source_node $source_object $n ] ] != [ string length [ sdo_rnx $dest_node $dest_object $n ] ] } {
            return [ error "Cannot do sdo_copy - size mismatch between node $source_node object $source_object sub $n and node $dest_node object $dest_object sub $n" ]
        }
    }
    
    # Copy data across
    for { set n 1 } { $n <= $num_objects } { incr n } {
        sdo_wnx $dest_node $dest_object $n [ sdo_rnx $source_node $source_object $n ]
    }
    
    return ""
}

###############################################################################
# sdo_ro_fmt
#
# Reads all object subindices in the format returned by sdo_rnx.
#
set helptext(sdo_ro_fmt) "Performs an formatted SDO read (upload) of all subindices of an object"
set helptextd(sdo_ro_fmt) "
SYNOPSIS
   sdo_ro_fmt nodeid idx ?start?

DESCRIPTION
Reads all object subindices in the format returned by sdo_rnx_fmt. start is used to specify
the subindex to start from. 
"
proc sdo_ro_fmt {nodeid idx {start 0}} {
    return [sdo_ro $nodeid $idx $start 1]
}


###############################################################################
# dm_sdo_ro
#
# as sdo_ro but for multi-profile devices

set helptext(dm_sdo_ro) "Performs an SDO read (upload) of all subindices of an object from multiple profiles"
set helptextd(dm_sdo_ro) "
SYNOPSIS
   dm_sdo_ro nodeid idx ?start?

DESCRIPTION
As sdo_ro but data is read from each profile listed in dm_profile_list. start is used to specify
the subindex to start from. 
"
proc dm_sdo_ro {nodeid idx {start 0}} {
    
    if {[set idx_list [dm_index_list $idx]] == "Error"} {
        return "Error"
    }
    
    set reply ""
    foreach obj_idx $idx_list {
        append reply "[sdo_ro $nodeid $obj_idx $start]\n"
    }
    
    return $reply
}


###############################################################################
# dm_sdo_ro_fmt
#
# as sdo_ro_fmt but for multi-profile devices

set helptext(dm_sdo_ro_fmt) "Performs an formatted SDO read (upload) of all subindices of an object from multiple profiles"
set helptextd(dm_sdo_ro_fmt) "
SYNOPSIS
   dm_sdo_ro_fmt nodeid idx ?start?

DESCRIPTION
As sdo_ro_fmt but data is read from each profile listed in dm_profile_list. start is used to specify
the subindex to start from. 
"
proc dm_sdo_ro_fmt {nodeid idx {start 0}} {
    
    if {[set idx_list [dm_index_list $idx]] == "Error"} {
        return "Error"
    }
    
    set reply ""
    foreach obj_idx $idx_list {
        append reply "[sdo_ro_fmt $nodeid $obj_idx $start]\n"
    }
    
    return $reply
}


###############################################################################
# sdo_wnx
#
# As sdo_w but accepts a hex value (in the format 0x1234 etc) as parameter.  It
# generates an SDO download of the appropriate type according to the number of
# hex bytes supplied.  In other words:
#
#  sdo_wnx nodeid idx sidx 0x11           - generates an expedited 1-byte transfer
#  sdo_wnx nodeid idx sidx 0x1122         - generates an expedited 2-byte transfer
#  sdo_wnx nodeid idx sidx 0x112222       - generates an expedited 3-byte transfer
#  sdo_wnx nodeid idx sidx 0x11222244     - generates an expedited 4-byte transfer
#  sdo_wnx nodeid idx sidx 0x1122224455   - generates an segmented 5-byte transfer
#  etc
#
# If a decimal value is supplied, the optional size parameter indicates how many
# bytes are sent.
#
set helptext(sdo_wnx) "Performs an SDO write (download) of a numeric value"
set helptextd(sdo_wnx) "
SYNOPSIS
   sdo_wnx nodeid idx sidx value ?width?

DESCRIPTION
Writes a single value to an index, subindex. If the data is a hex number (ie, has an 
0x prefix), the number of bytes downloaded is determined by the number of hex characters 
in the number. If the data is a decimal number, the number of bytes to be downloaded is 
specified in the width argument.
"
proc sdo_wnx {nodeid idx sidx value args} {
   global debug_canopen
   
   # Convert value into a hex number of the correct width.
   if {[string range $value 0 1] == "0x"} {
      # The user has supplied a hex value
      if {[llength $args] != 0} {
         if {$debug_canopen} {dvtConPuts debug "sdo_wnx: width ignored with a hex value"}
      }
      
      if {([string length $value]&1) == 1} {
         if {$debug_canopen} {dvtConPuts debug "sdo_wnx: badly formatted hex number (must have even number of digits)"}
         return "Error"
      }
      
      set h $value
   } else {
      # The user has supplied a non-hex value.  Convert it to a hex number of the correct
      # length
      if {[llength $args] == 0} {
         if {$debug_canopen} {dvtConPuts debug "sdo_wnx: no width specified for decimal value"}
         return "Error"
      }
      
      set h [format 0x%0[expr [lindex $args 0]*2]X $value]
   }          
   
   # Convert 0x1234 to 0x34 0x12 etc
   set h [unmerge_hex $h]
   
   set r [sdo_wn $nodeid $idx $sidx [join $h]]
   
   return $r
}


###############################################################################
# dm_sdo_wnx
#
# as sdo_wnx but for multi-profile devices

set helptext(dm_sdo_wnx) "Performs an SDO write (download) of a numeric value to multiple profiles"
set helptextd(dm_sdo_wnx) "
SYNOPSIS
   dm_sdo_wnx nodeid idx sidx value ?width?

DESCRIPTION
As sdo_wnx but data is written to each profile listed in dm_profile_list
"
proc dm_sdo_wnx {nodeid idx sidx value args} {
    
    if {[set idx_list [dm_index_list $idx]] == "Error"} {
        return "Error"
    }
    
    set reply ""
    foreach obj_idx $idx_list {
        append reply "$obj_idx,$sidx: [sdo_wnx $nodeid $obj_idx $sidx $value $args]\n"
    }
    
    return $reply
}


###############################################################################
# sdo_rns
#
# As sdo_rn but interprets the uploaded data as a CANopen VISIBLE_STRING.  The first
# four bytes are interpreted as the length of the string.  
#
set helptext(sdo_rns) "Performs an SDO read (upload) of a string"
set helptextd(sdo_rns) "
SYNOPSIS
   sdo_rns nodeid idx sidx

DESCRIPTION
Reads a string from index, subindex.
"
proc sdo_rns {nodeid idx sidx} {
   global debug_canopen
   
   set r [sdo_rn $nodeid $idx $sidx]
   
   # Correct results start with 0x.  Error returns don't
   if {[string range $r 0 1] != "0x"} {
      return $r
   }
   
   # Determine whether we received an expedited or a segmented response
   set length [llength $r]
   if {$length <= 4} {
      set string ""
      foreach c $r {
         if {$c == 0x00} {
             if {$debug_canopen && ([string length $string] < [expr $length - 1])} {
                dvtConPuts debug "Warning: String truncated by NULL terminator. Use sdo_rn to see the entire message."
             }
             #break
         }
         # add any printable character to string
         set ch [format %c $c]
         if {[string is print -strict $ch] || ($c == 10)} {
           append string $ch
         }
      }
   } else {
       
       # Split up the received bytes
       set server_length [lrange $r 0 3  ]
       set server_data   [lrange $r 4 end]
       
       set server_length [merge_hex $server_length]
       if {[llength $server_data] != $server_length} {
          dvtConPuts stderr "sdo_rns: server length ([expr $server_length]) disagrees with returned data ([llength $server_data])"
       }
       
       set string ""
       foreach c $server_data {
          if {$c == 0x00} {
              if {$debug_canopen && ([string length $string] < [expr $server_length - 1])} {
                 dvtConPuts debug "Warning: String truncated by NULL terminator. Use sdo_rn to see the entire message."
              }
              
              #break
          }
          
          # add any printable character to string
          set ch [format %c $c]
          if {[string is print -strict $ch] || ($c == 10)} {
            append string $ch
          }
       }
    }   
    
    return $string
}


###############################################################################
#
# sdo_wn16
#
# Write a series of 16-bit words to a UUT.  
#
# As sdo_wn, but the data are interpreted as 16-bit words.  The 16-bit data can be
# specified in either hex or decimal.  Little endian or big-endian can be specified,
# with big endian as the default.
#
# EXAMPLE
#    sdo_wn16 1 0x467E 0 1 9 3 0 100 1 1 2 2 3 3 125
#    sdo_wn16 1 0x467E 0 1 9 3 0 100 1 1 2 2 3 3 125 big
#    sdo_wn16 1 0x467E 0 1 9 3 0 100 1 1 2 2 3 3 125 little
proc sdo_wn16 {nid idx sidx args} {
   
   set bytes ""
   
   # Assume big endian unless otherwise specified at the end of the arguments
   set endian "big"
   if {[lindex $args end] == "big"   } {set endian "big";    set args [lreplace args end end]}
   if {[lindex $args end] == "little"} {set endian "little"; set args [lreplace args end end]}
   
   foreach arg $args {      
      if {$arg<0 || $arg>65535} {
         dvtConPuts stderr "sdo_wn16: word out of range"
         return
      }
      
      if {$endian=="little"} {
         append bytes "[format 0x%02X [expr [expr $arg]%256]] [format 0x%02X [expr [expr $arg]/256]] "
      } else {
         append bytes "[format 0x%02X [expr [expr $arg]/256]] [format 0x%02X [expr [expr $arg]%256]] "
      } 
   }
   
   sdo_wn $nid $idx $sidx $bytes
}


###############################################################################
#
# sdo_rn16
#
# Read 16-bit words from a UUT. 
#
# As sdo_rn but inteprets the data as 16-bit integers.  Little endian
# or big-endian can be specified, with big endian as the default.
# 
# EXAMPLE
#    sdo_rn16 1 0x467F 0        
#    sdo_rn16 1 0x467F 0 big    
#    sdo_rn16 1 0x467F 0 little 

proc sdo_rn16  {nid idx sidx {endian big}} {
   set r [sdo_rn $nid $idx $sidx]
   
   # Correct results start with 0x.  Error returns don't
   if {[string range $r 0 1] != "0x"} {
      # This looks like an error
      dvtConPuts stderr  $r
      return
   }
   
   if {[llength $r] <= 4} {
      # This was an expedited transfer, so there is no length in the answer
      set server_length [llength $r]
      set server_data   $r
   } else {
      # This was an expedited transfer, so there is a length in the first four bytes of the answer
      set server_length [lrange $r 0 3  ]
      set server_data   [lrange $r 4 end]
      set server_length [merge_hex $server_length]
      
      if {[llength $server_data] != $server_length} {
         dvtConPuts stderr "sdo_rn16: server length ([expr $server_length]) disagrees with returned data ([llength $server_data])"
         return
      }
      
   }
      
   if {$server_length&1 == 1} {
      dvtConPuts stderr "sdo_rn16: server length is odd"
      return
   }
      
   # Form the bytes up into words using the specified endianness
   set bytes16 ""
   for {set i 0} {$i<$server_length} {incr i 2} {
      if {$endian=="little"} {
         set b16 "[expr [lindex $server_data [expr $i+1]]*256 + [lindex $server_data [expr $i+0]]]"
      } else {
         set b16 "[expr [lindex $server_data [expr $i+0]]*256 + [lindex $server_data [expr $i+1]]]"
      }
      append bytes16 "[format 0x%04X $b16] "
   }
   
   puts $bytes16
}


###############################################################################
# This is a valid DCTT-OL UUT Request for self-characterisation and is included
# here for convenience.  See S-020 v0.4 for details of the format.
proc sdo_wn16_test {} {
   sdo_wn16 1 0x467E 0 0x0001 0x000C 0x0004 0x0003 0x012C 0x0000 0x0618 0x0009 0x0000 0x005A 0xF9E8 0x000C 0x0000 0x0078 0x0227
}


###############################################################################
# merge_hex
#
# in        A string containing hex numbers (eg, 0xAA 0xBB 0xCC 0xDD etc) of any length
#
# returns   0xDDCCBBAA etc
#
set helptext(merge_hex) "Converts \"0xAA 0xBB\" into \"0xBBAA\" etc"
set helptextd(merge_hex) "
SYNOPSIS
   merge_hex in

DESCRIPTION
Converts a list of hex numbers (with leading 0x) into one hex value. Assumes
little endian format. So \"0xAA 0xBB\" converts into \"0xBBAA\"
"

proc merge_hex { in } {
   set out "0x"
   
   for {set i 0} {$i<[llength $in]} {incr i} {
      append out [string range [lindex $in [expr [llength $in]-$i-1]] 2 3]
   }
   
   return $out
}


###############################################################################
# unmerge_hex
#
# in        A single hex number of any length (eg, 0x12345678)
#
# returns   A string containing the hex bytes separate (0x78 0x56 0x34 0x12 etc)
#
set helptext(unmerge_hex) "Converts \"0xBBAA\" into \"0xAA 0xBB\" etc"
set helptextd(unmerge_hex) "
SYNOPSIS
   unmerge_hex in

DESCRIPTION
Splits a hex number into a list of hex bytes. Assumes little endian format. 
So \"0xBBAA\" converts into \"0xAA 0xBB\"
"
proc unmerge_hex { in } {
   set in [string range $in 2 end]
   set out ""
   
   for {set i [string length $in]} {$i>0} {incr i -2} {
      append out "0x[string range $in [expr $i-2] [expr $i-1]] "
   }
   
   # Remove the trailing space
   set out [string range $out 0 end-1]
   
   return $out
}


###############################################################################
# mirror
#
# in  A string of words (such as hex bytes, for example)
#
# returns a string with the words reversed in left-right manner
#
# Example:
#
#  mirror a b c d e f g
#  g f e d c b a
set helptext(mirror) "Converts \"a b c d\" into \"d c b a\" etc"

proc mirror {args} {

   set args [join $args]
   
   if {[llength $args] == 1} {
      return $args
   } elseif {[llength $args] == 2} {
      return "[lindex $args 1] [lindex $args 0]"
   } else {
      set mid [expr int([llength $args]/2)]
      set l [lrange $args $mid end]
      set r [lrange $args 0 [expr $mid-1]]
      return "[mirror $l] [mirror $r]"
   }
}


set sad(0x05030000) "Toggle bit not alternated."
set sad(0x05040000) "SDO protocol timed out."
set sad(0x05040001) "Client/server command specifier not valid or unknown."
set sad(0x05040002) "Invalid block size (block mode only)."
set sad(0x05040003) "Invalid sequence number (block mode only)."
set sad(0x05040004) "CRC error (block mode only)."
set sad(0x05040005) "Out of memory."
set sad(0x06010000) "Unsupported access to an object."
set sad(0x06010001) "Attempt to read a write only object."
set sad(0x06010002) "Attempt to write a read only object."
set sad(0x06020000) "Object does not exist in the object dictionary."
set sad(0x06040041) "Object cannot be mapped to the PDO."
set sad(0x06040042) "The number and length of the objects to be mapped would exceed PDO length."
set sad(0x06040043) "General parameter incompatibility reason."
set sad(0x06040047) "General internal incompatibility in the device."
set sad(0x06060000) "Access failed due to an hardware error."
set sad(0x06070010) "Data type does not match, length of service parameter does not match"
set sad(0x06070012) "Data type does not match, length of service parameter too high"
set sad(0x06070013) "Data type does not match, length of service parameter too low"
set sad(0x06090011) "Sub-index does not exist."
set sad(0x06090030) "Value range of parameter exceeded (only for write access)."
set sad(0x06090031) "Value of parameter written too high."
set sad(0x06090032) "Value of parameter written too low."
set sad(0x06090036) "Maximum value is less than minimum value."
set sad(0x08000000) "general error"
set sad(0x08000020) "Data cannot be transferred or stored to the application."
set sad(0x08000021) "Data cannot be transferred or stored to the application because of local control. Have you ran out of PDOs? DO NOT EXECUTE A STORE OR YOU WILL LOCK THE CAN ON YOUR UNIT!!!"
set sad(0x08000022) "Data cannot be transferred or stored to the application because of the present device state."
set sad(0x08000023) "Object dictionary dynamic generation fails or no object dictionary is present (e.g. object dictionary is generated from file and generation fails because of an file error)."

set helptext(sdo_abort_decode) "Returns description of SDO abort error"
set helptextd(sdo_abort_decode) "
SYNOPSIS
   sdo_abort_decode abort_code

DESCRIPTION
Returns description of SDO abort error
"
proc sdo_abort_decode {abort_code} {
   global sad
   
   if { $abort_code == "Timeout" } {
       return "Comms timeout. Incorrect node ID? Check for comms in CAN window."
   }
   
   # Allow proc to handle input in the format "Abort 0x12345678"
   if { [ string length $abort_code ] > 10 } {
       set l [ string length $abort_code ]
       set abort_code [ string range $abort_code [ expr $l-10 ] [ expr $l - 1 ] ]
   }
   
   if {[info exists sad($abort_code)]} {
      return "\"$sad($abort_code)\""
   }
   
   return "(invalid abort code)"
}


###############################################################################
# dm_index_list
#
# returns a list of indices based on the base index (for profile 1) and the profile list
proc dm_index_list {idx} {
    global dm_profile_list
    
    # check that a profile list has been defined
    if {![info exists dm_profile_list]} {
        dvtConPuts stderr "dm_profile_list is not defined. This must be defined as a list of node IDs to use for dm_sdo_xxx procs."
        return "Error"
    }
    
    # check if index is in profile 1 range, and whether it points to manufacturer (0x4600-0x467f) or profile area (0x6000-0x67ff)
    if {($idx >= 0x4600) && ($idx < 0x4680)} {
        set start 0x4600
        set mul   0x0080
    } elseif {($idx >= 0x6000) && ($idx < 0x6800)} {
        set start 0x6000
        set mul   0x0800
    } else {
        dvtConPuts stderr "Index ($idx) is not in range for profile 1 (0x4600-0x467F or 0x6000-0x67ff)"
        return "Error"
    }
    
    set indices ""
    foreach motor $dm_profile_list {
        set obj_offset    [expr $idx - $start]
        set profile_start [expr $start + (($motor - 1) * $mul)]
        
        lappend indices [format "0x%04x" [expr $profile_start + $obj_offset]]
    }
    
    return $indices
}

###############################################################################
