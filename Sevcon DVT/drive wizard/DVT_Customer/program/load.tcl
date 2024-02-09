###############################################################################
# (C) COPYRIGHT Sevcon 2004
# 
# CCL Project Reference C6944 - Magpie
# 
# FILE
#     $Revision:1.30$
#     $Author:cmp$
#     $ProjectName:DVT$ 
# 
# ORIGINAL AUTHOR
#     Martin Cooper
# 
# DESCRIPTION
#     Tcl script(s) to assist with loading programs into flash using the
#     bootloader programs.
# 
# REFERENCES    
#     C6944-TM-187
# 
# MODIFICATION HISTORY
#     $Log:  43683: load.tcl 
# 
#     Rev 1.30    10/09/2008 09:21:44  cmp
#  allow list to be passed to bts and bte
# 
#     Rev 1.29    12/11/2007 14:20:48  ceh
#  Pass node ID to bootloader reset proc otherwise it only works on node 1
# 
#     Rev 1.28    23/10/2007 16:54:14  cmp
#  added Gen4 as valid bootloader type
# 
#     Rev 1.27    28/09/2007 10:19:18  ps
#  Change bts command to use catch statement so it doesn't abort if lg or fpo
#  doesn't work, as will be the case with displays, SCwiz, prod test code, etc...
# 
#     Rev 1.26    27/09/2007 14:34:34  cmp
#  Program multiple nodes in load_dld without having to reparse the dld file.
# 
#     Rev 1.25    02/08/2007 10:53:48  ceh
#  Made DLD filename optional. If no DLD filename is specified, tk_getOpenFile
#  is opened allowing user to select file.
# 
#     Rev 1.24    12/07/2007 10:34:26  ceh
#  Added extra memory spaces for DSP Zeffer, etc. Fixed some wrong variable
#  names.
# 
#     Rev 1.23    11/07/2007 13:00:14  cmp
#  remove debug from load proc
# 
#     Rev 1.22    11/07/2007 12:54:02  cmp
#  now includes dld download functionality
# 
#     Rev 1.21    23/04/2007 11:25:12  ceh
#  Updated help text to standard format.
# 
#     Rev 1.20    08/11/2006 13:40:30  ceh
#  Added module revision registering.
# 
#     Rev 1.19    13/09/2006 09:53:44  cmp
#  
#  allow developement versions of bootloader
# 
#     Rev 1.17    30/08/2006 13:53:34  ceh
#  Added abort load and support for Chinook bootloader
# 
#     Rev 1.16    30/06/2006 13:00:36  ceh
#  Added proc to create a file filled with user specified data. Useful for
#  erasing configuration memory spaces
# 
#     Rev 1.15    26/06/2006 14:20:08  ceh
#  Added support for Zeffer/Aichi external flash
# 
#     Rev 1.14    10/05/2006 16:13:52  ceh
#  Updated load proc to be able to erase program memory sectors.
# 
#     Rev 1.13    19/01/2006 18:05:20  cmp
#  flush CAN fifo to purge bad sdo's left behind
# 
#     Rev 1.12    30/12/2005 12:53:38  cmp
#  undo changes of V1.11 - as per V1.10
# 
#     Rev 1.10    07/12/2005 09:45:36  cmp
#  corrected invalid brace type for file open catch; now allows spaces to be
#  included
# 
#     Rev 1.9    18/11/2005 08:37:32  cmp
#  modify version validation as [expr 0009] evaluates to an invalid octal number
#  !!
# 
#     Rev 1.8    27/10/2005 15:38:32  ceh
#  Added Zeffer bootloader support
# 
#    Rev 1.7    25/05/2005 11:01:18  cmp
#  added update to save routine

# 
#    Rev 1.6    23/05/2005 15:58:14  cmp
#  call update whilst downloading

# 
#    Rev 1.5    09/05/2005 11:22:08  ps
#  Fixed variable name bug in load proc

# 
#    Rev 1.4    09/05/2005 10:26:08  ps
#  Changed bootloader to take advantage of segmented SDO upload in recent
#  release of bootloader H0002.0009.

# 
#    Rev 1.3    22/02/2005 09:09:56  ps
#  Removed debugging code for "endpaddinglength".

# 
#    Rev 1.2    16/02/2005 13:40:42  ps
#  Allow user to select the same file for xc164-int and xc164-ext.

# 
#    Rev 1.1    19/11/2004 08:42:20  ceh
#  Added code to restore sdo_timeout variable after load/save. Also, use
#  dvtConPuts to print information.

# 
#    Rev 1.0    08/11/2004 10:07:34  ceh
#  Moved load.tcl from scripts sub-directory to program sub-directory.

# 
#    Rev 1.8    19/10/2004 13:39:10  ps
#  Increased timeout for DSP - can take a long time to erase.

# 
#    Rev 1.7    27/09/2004 13:34:46  ps
#  Read smaller blocks when saving data

# 
#    Rev 1.6    11/08/2004 13:48:46  ps
#  Added procs for bootloader_start and bootloader_end

# 
#    Rev 1.5    01/07/2004 15:59:04  ps
#  Extended SDO timeout when dealing with EEPROM

# 
#    Rev 1.4    30/06/2004 18:07:54  ps
#  Added command to save data from controller back to file (useful for copying
#  contents of EEPROM around).

# 
#    Rev 1.3    28/05/2004 12:32:14  ps
#  Various updates.

# 
#    Rev 1.2    28/05/2004 10:33:00  mdc
#  More various improvements.

# 
#    Rev 1.2    28/05/2004 10:30:42  mdc
#  Various improvements.

#
###############################################################################

# register module
register DVT [info script] {$Revision:1.30$}


###############################################################################
#
# load
#
# PARAMETERS
# ==========
# nodeid       - The Node ID.
# section      - Memory section to load file into
# binfilename  - The name of the file containing the binary data.  This is
#                assumed to start at address zero, and so will contain lots
#                of zeroes before the device start address.
#
# RETURNS
# =======
# The checksum of the written data.
#
# DESCRIPTION
# ===========
# Loads a program into a Shiroko controller using the bootloaders in the Host
# and DSP.  The Host and DSP must be running the bootloader when this proc is
# called.
#
set helptext(load) "Load a binary file into any of the memory spaces - assumes bootloader is running"
set helptextd(load) "
SYNOPSIS
    load nodeid section binfilename
    
DESCRIPTION
Downloads binfilename to the specified section on node nodeid. Section can be one of
    dsp             - DSP slave on espAC (inc Dialbo/Chinook)
    eeprom          - EEPROM
    xc164-int       - Host XC164 internal flash 
    xc164-ext       - Host XC164 external flash 
    at49bv-ext      - Host 2812 external flash
    dsp-zeffer      - Host 2812 internal flash
    flash-config-a  - Host 2812 flash config A sector
    flash-config-b  - Host 2812 flash config B sector
    display         - CANopen display flash

If binfilename is set to ERASE=?, section is erased (set to all ?. eg ?=0 sets memory to all 0s).
"

proc load {nodeid section binfilename {delete_temp_files TRUE}} {
   global unit_variant eeprom_1_byte
   
   global sdo_timeout
   set current_sdo_timeout $sdo_timeout

   global abort_load
   set abort_load 0
   
   #
   # Flush CAN Fifo before attempting loading a binary
   #
   if {[package present Dvtint] == 2.0} {
       Ccan flush
   }

   # Sub-indices for each object are (from TM-187):
   # 1   R  uint16   Max Block Size in bytes
   # 2   R  uint16   Page Size in bytes
   # 3   R  uint32   Device start address
   # 4   R  uint32   Device Length
   # 5   W  uint32   Start Address 
   # 6   W  uint16   Data Length
   # 7   W  uint8[]  Data
   # 8   W  uint16   Checksum

   # Set up some parameters according to the section
   # being programmed.  These are:
   #
   #  od_idx            - The relevant object index.
   #  minbinsize        - The minimum acceptable binfile size.  This is used to check
   #                       that the binfile looks realistic.
   #  new_sdo_timeout   - The SDO timeout to use during writes.  Because the erase
   #                       and program times can be quite long, the SDO timeout is
   #                       increased from the normal value.
   switch $section {
      xc164-int {
         set od_idx           0x5A20
         set minbinsize       0x0010
         set new_sdo_timeout  10000
      }
      dsp {
         set od_idx           0x5A21
         set minbinsize       0x0100
         set new_sdo_timeout  40000
      }
      eeprom {
         set od_idx           0x5A22
         set minbinsize       0x0001
         set new_sdo_timeout  10000
      }
      xc164-ext {
         set od_idx           0x5A23
         set minbinsize       0x0001
         set new_sdo_timeout  15000
      }
      at49bv-ext {
         set od_idx           0x5A23
         set minbinsize       0x0001
         set new_sdo_timeout  30000
      }
      dsp-zeffer {
         set od_idx           0x5A24
         set minbinsize       0x0100
         set new_sdo_timeout  60000
      }
      flash-config-a {
         set od_idx           0x5A25
         set minbinsize       0x0100
         set new_sdo_timeout  30000
      }
      flash-config-b {
         set od_idx           0x5A26
         set minbinsize       0x0100
         set new_sdo_timeout  30000
      }
      display {
          set od_idx          0x5A23
          set minbinsize      0x0100
          set new_sdo_timeout 15000
      }
      default {
         dvtConPuts stderr "Unknown section for programming: $section"
         return "Error"
      }
   }
   
   # Check the node exists and is communicating by reading its sofware version.
   if {[identify_bootloader $nodeid] != "OK"} {
      dvtConPuts stderr "Can't communicate with node ID $nodeid, or bootloader not running"
      return "Error"
   }
     
   
   # read memory data from controller
   set blocksize [sdo_rnx $nodeid $od_idx 1]
   if {[catch {expr $blocksize}]} {
      dvtConPuts stderr "Unable to read block size from $od_idx, 1. Returned $blocksize"
      return "Error"
   }
   
   set pagelength [sdo_rnx $nodeid $od_idx 2]
   if {[catch {expr $pagelength}]} {
      dvtConPuts stderr "Unable to read page length from $od_idx, 2. Returned $pagelength"
      return "Error"
   }
   
   set devicestartaddress [sdo_rnx $nodeid $od_idx 3]
   if {[catch {expr $devicestartaddress}]} {
      dvtConPuts stderr "Unable to read device start address from $od_idx, 3. Returned $devicestartaddress"
      return "Error"
   }
   
   set devicelength [sdo_rnx $nodeid $od_idx 4]
   if {[catch {expr $devicelength}]} {
      dvtConPuts stderr "Unable to read device length from $od_idx, 4. Returned $devicelength"
      return "Error"
   }
   
   # calculate device end address
   set deviceendaddress [ expr ( $devicestartaddress + $devicelength ) ]

   # open binfilename or check for an erase request
   set tempbinfilename "NONE"
   if {![file exists $binfilename]} {
      
      if {[string range $binfilename 0 5] == "ERASE="} {
      
         set value [string range $binfilename 6 end]
         if {[catch {expr $value} erase_value]} {
            dvtConPuts stderr "Erase value ($value) must be decimal or hex."
            return "Error"
         }
         
         # range check erase value
         if {($erase_value < 0) || ($erase_value > 255)} {
            dvtConPuts stderr "Erase value ($value) must be a byte value."
            return "Error"
         }
         
         # create temporary bin file and populate with erase values
         set tempbinfilename "erasetemp_[clock seconds].bin"
         if {[catch "open $tempbinfilename w" fid]} {
             dvtConPuts stderr "File $tempbinfilename could not be opened for writing"
             return "Error"
         }
         # ensure no LF translation is performed
         fconfigure $fid -translation binary
        
         # first write a load of zeros to the file up to the start address
         set current_location 0
         while {$current_location < $devicestartaddress} {
             incr current_location
             fputs -nonewline $fid \x00
         }
        
         # now fill with the erase values
         set erase_value [binary format c $erase_value]
         while {$current_location < $deviceendaddress} {
             incr current_location
             fputs -nonewline $fid $erase_value
         }
    
         # once done close file and assign temp file name to binfilename so load can be performed correctly
         close $fid
         set binfilename $tempbinfilename
      
      } else {
          dvtConPuts stderr "Can't find file: $binfilename"
          return "Error"
      }
   }
   
   if {[file size $binfilename] < [expr $devicestartaddress+$minbinsize]} {
      dvtConPuts stderr "This file ([file size $binfilename] bytes) looks too small for this section"
      return "Error"
   }
   
   if {[file size $binfilename] > [expr $devicelength + $devicestartaddress]} {
      dvtConPuts stderr "Warning: This file ([file size $binfilename] bytes) looks too big for this section ([expr $devicelength + $devicestartaddress]) but programming as much as we can anyway"
   }
   
   set startaddress [format 0x%08X $devicestartaddress]
   set sectionchecksum 0
   
   # Open the file
   if {[catch {open $binfilename r} fid]} {
      dvtConPuts stderr "File $binfilename could not be opened for reading\n$fid"
      return "Error"
   }
   
   # Ensure no LF translation is performed
   fconfigure $fid -translation binary
   
   # Discard bytes from the start of the file to the device start address
   set b [read $fid $devicestartaddress]
   if {[string length $b] != $devicestartaddress} {
      dvtConPuts stderr "File $binfilename does not contain enough data"
      close $fid
      return "Error"
   }

   # Start the packet counter
   set packet 1
   
   set fileposition $devicestartaddress
   
   while { (![eof $fid]) && ($fileposition < ($deviceendaddress-$pagelength) ) && !$abort_load } {
     
      set n [expr int($blocksize/$pagelength)*$pagelength ]

      if { $fileposition + $n > $deviceendaddress } {
          set blocksize [ expr ( $deviceendaddress - $fileposition ) ]
          set n [expr int($blocksize/$pagelength)*$pagelength ]
      }

      incr fileposition $n
      
      # Get some bytes from the binary file, up to the pagelength
      set b [read $fid $n]
      
      # Find out how many bytes were read from the file
      set packetlength [string length $b]

      if {$packetlength == 0} {
            continue
      }
      
      # Convert the binary to individual bytes (in decimal)
      binary scan $b c* bytes
      
      # Convert the binary to hex bytes
      set hex ""
      foreach byte $bytes {
         append hex "[format 0x%02X [expr $byte&0xFF]] "
      }
      
      # If there were insufficient bytes for a multiple of full pages,
      # append zeroes to the end.
      if {$packetlength%$pagelength != 0} {
        set endpaddinglength [expr $pagelength-$packetlength%$pagelength ]
      } else {
        set endpaddinglength 0
      }

      append hex [string repeat "0x00 " $endpaddinglength]
      incr packetlength $endpaddinglength
      set packetlength [format 0x%04X $packetlength]

      # Update the packet and section checksums.  During debugging, the expected
      # packet checksum can be read from subindex 8.
      set packetchecksum 0
      incr packetchecksum [expr "0x[string range $startaddress 2 3]"]
      incr packetchecksum [expr "0x[string range $startaddress 4 5]"]
      incr packetchecksum [expr "0x[string range $startaddress 6 7]"]
      incr packetchecksum [expr "0x[string range $startaddress 8 9]"]
      incr packetchecksum [expr "0x[string range $packetlength 2 3]"]
      incr packetchecksum [expr "0x[string range $packetlength 4 5]"]
      
      foreach byte $hex {
         incr packetchecksum  [expr $byte]
         incr sectionchecksum [expr $byte]
      }
      
      # Inform user of progress - could have a progress bar later
      if {$packet == 1} {
         dvtConPuts "[format %6d $packet]  Address: [format 0x%06X $startaddress] Len: [format 0x%06X $packetlength] - NOTE: Programming of the first block includes device erase cycle which can take some time."
      } else {
         dvtConPuts "[format %6d $packet]  Address: [format 0x%06X $startaddress] Len: [format 0x%06X $packetlength]"
      }
      
      # Write the start address to the OD (with a short SDO timeout)
      set sdo_timeout 1000
      set result [sdo_wnx $nodeid $od_idx 5 $startaddress]
      update
      if {$result != "OK"} {dvtConPuts stderr "Error ($result) writing to start address ($startaddress) for object $od_idx."; close $fid; set sdo_timeout $current_sdo_timeout; bootloader_reset $nodeid; return "Error"}
      
      # Write the length to the OD (with a short SDO timeout)
      set result [sdo_wnx $nodeid $od_idx 6 [format 0x%04X $packetlength]]
      update
      if {$result != "OK"} {dvtConPuts stderr "Error ($result) writing to data length ($packetlength) for object $od_idx."; close $fid; set sdo_timeout $current_sdo_timeout; bootloader_reset $nodeid; return "Error"}
     
      # Write the packet data to the OD (with a short SDO timeout)
      set result [sdo_wn $nodeid $od_idx 7 $hex]
      update
      if {$result != "OK"} {dvtConPuts stderr "Error ($result) writing to data for object $od_idx."; close $fid; set sdo_timeout $current_sdo_timeout; bootloader_reset $nodeid; return "Error"}
      
      # Write this packet's checksum to the OD (with a long SDO timeout)
      set sdo_timeout $new_sdo_timeout
      set packetchecksum [expr (0xFFFF-$packetchecksum)%0x10000]
      set packetchecksum [format 0x%04X $packetchecksum]
      
      set result [sdo_wnx $nodeid $od_idx 8 $packetchecksum]
      update
      if {$result != "OK"} {dvtConPuts stderr "Error ($result) writing to checksum ($packetchecksum) for object $od_idx."; close $fid; set sdo_timeout $current_sdo_timeout; bootloader_reset $nodeid; return "Error"}
      
      # Update the start address for the next packet
      set startaddress [format 0x%08X [incr startaddress $packetlength]]
      
      incr packet
   }
   
   close $fid
   bootloader_reset $nodeid

   # delete temporary file for erasing
   if {($tempbinfilename != "NONE") && $delete_temp_files} {
        file delete $tempbinfilename
   }
   
   set sdo_timeout $current_sdo_timeout
   
   if {$abort_load} {
      dvtConPuts stderr "Load aborted by user"
      return "Error"
   }
   
   return $sectionchecksum
}


###############################################################################
#
# save
#
# PARAMETERS
# ==========
# nodeid       - The Node ID.
# section      - Either "xc164-int", "xc164-ext", "dsp", "eeprom" or "at49bv-ext".
# binfilename  - The name of the file where the data will be written to.
#
# RETURNS
# =======
# No return value.
#
# DESCRIPTION
# ===========
# Reads information back from the Shiroko controller and stores it in a file.
# Useful for transferring data from one controller to another. Only works with
# memory spaces that are readable (at time of writing, only allowed to read
# from EEPROM).
#
set helptext(save) "Reads data from a memory space and stores it in a file - assumes controller is in bootloader"
set helptextd(save) "
SYNOPSIS
    save nodeid section binfilename
    
DESCRIPTION
Uploads from the specified section on node nodeid to binfilename. Section can be one of
    dsp             - DSP slave on espAC (inc Dialbo/Chinook)
    eeprom          - EEPROM
    xc164-int       - Host XC164 internal flash 
    xc164-ext       - Host XC164 external flash 
    at49bv-ext      - Host 2812 external flash
    dsp-zeffer      - Host 2812 internal flash
    flash-config-a  - Host 2812 flash config A sector
    flash-config-b  - Host 2812 flash config B sector
    display         - CANopen display flash
"

proc save {node_id section binfilename} {
    global unit_variant eeprom_1_byte

    global abort_save
    set abort_save 0
    
    # Sub-indices for each object are (from TM-187):
    # 1   R  uint16   Max Block Size in bytes
    # 2   R  uint16   Page Size in bytes
    # 3   R  uint32   Device start address
    # 4   R  uint32   Device Length
    # 5   W  uint32   Start Address 
    # 6   W  uint16   Data Length
    # 7   W  uint8[]  Data
    # 8   W  uint16   Checksum
    
    switch $section {
        xc164-int {
            set od_idx           0x5A20
        }
        dsp {
            set od_idx           0x5A21
        }
        eeprom {
            set od_idx           0x5A22
        }
        xc164-ext {
            set od_idx           0x5A23
        }
        at49bv-ext {
            set od_idx           0x5A23
        }
        dsp-zeffer {
            set od_idx           0x5A24
        }
        flash-config-a {
            set od_idx           0x5A25
        }
        flash-config-b {
            set od_idx           0x5A26
        }
        default {
            dvtConPuts stderr "Unknown section for reading: $section"
            return "Error"
        }
    }
    
    # Check the node exists and is communicating by reading its sofware version.
    if {[identify_bootloader $node_id] != "OK"} {
        dvtConPuts stderr "Can't communicate with node ID $nodeid, or bootloader not running"
        return "Error"
    }
     
    
    set block_size           [sdo_rnx $node_id $od_idx 1]
    set page_length          [sdo_rnx $node_id $od_idx 2]
    set device_start_address [sdo_rnx $node_id $od_idx 3]
    set device_length        [sdo_rnx $node_id $od_idx 4]
    
    # Attempt to open a file for writing
    if {[catch "open $binfilename w" fileid]} {
        dvtConPuts stderr "File $binfilename could not be opened for writing"
        return "Error"
    }
    # Ensure no LF translation is performed
    fconfigure $fileid -translation binary
        
    set current_location    0
    
    # First write a load of zeros to the file up to the start address
    while { $current_location < $device_start_address } {
        incr current_location
        fputs -nonewline $fileid \x00
    }
        
    # Calculate the amount to read, which should be the greatest number
    # which is a multiple of the page size, but not greater than the 
    # block size or number of bytes remaining.
    # For host revisions earlier than 0009, only read one byte at a time.
    
    if {$eeprom_1_byte} {
        set amount_to_read $page_length
        set amount_remaining $device_length
    } else {
        set amount_to_read $block_size
        set amount_remaining [ expr ( $device_start_address + $device_length - $current_location ) ]
        set amount_to_read [ expr ( $amount_to_read - ( $amount_to_read % $page_length ) ) ]
    }
            
    # Now read the data from the memory space until we reach the end
    set packet 1
    while { ($amount_remaining > 0) && (!$abort_save) } {

        if { $amount_to_read > $amount_remaining } {
            set amount_to_read $amount_remaining
        }
               
        # Inform user of progress - could have a progress bar later
        dvtConPuts "[format %6d $packet]  Address: [format 0x%06X $current_location] Len: [format 0x%06X $amount_to_read]"
        
        # Send the request to the device
        sdo_wnx $node_id $od_idx 5 $current_location 4
        sdo_wnx $node_id $od_idx 6 $amount_to_read 2
        
        # Read from the device
        set hexdata [ sdo_rnx $node_id $od_idx 7 ]
        update
                
        # Write each byte to the file
        set bytes_to_write $amount_to_read
        while { $bytes_to_write > 0 } {
            set hex_byte [ string range $hexdata [ expr ( $bytes_to_write * 2 ) ] [ expr ( $bytes_to_write * 2 + 1 ) ]  ]
            fputs -nonewline $fileid [ binary format H2 $hex_byte ]
            incr bytes_to_write -1
        }
        
        # Moving on...
        set current_location [ expr ( $current_location + $amount_to_read ) ]
        
        incr amount_remaining [ expr ( 0 - $amount_to_read ) ]
        incr packet
    }
    
    # Close the file and we're done.
    close $fileid
   
    if {$abort_save} {
        dvtConPuts stderr "Save aborted by user"
        return "Error"
    }
    
    return
}



###############################################################################
#
# create_memory_section_file
#
# PARAMETERS
# ==========
# start_address  - Start address of memory section
# length         - Number of bytes in section
# binfilename    - Binary file name
# fill_val       - Value to fill unused memory with
# initial_values - A list of initial values to load into section
#
# RETURNS
# =======
# OK if file was created successfully
#
# DESCRIPTION
# ===========
# Creates a memory section file for the specified memory range. Initial
# values are set to initial_values, then allow subsequent entrys are
# set to fill_val
#
set helptext(create_memory_section_file) "Creates a memory section file for the specified memory range"
set helptextd(create_memory_section_file) "
SYNOPSIS
    create_memory_section_file start_address length binfilename fill_val initial_values
    
DESCRIPTION
Creates a memory section file for the specified memory range. Initial values are set 
to initial_values, then allow subsequent entrys are set to fill_val. Useful for creating
blank files for EEPROM and flash configuration sections
"

proc create_memory_section_file {start_address length binfilename fill_val initial_values} {
   
    # calculate end address
    set end_address [expr $start_address + $length]
    
    # check for value fill value and initial values
    set temp_list $initial_values
    lappend temp_list $fill_val
    foreach item $temp_list {
        # check for hex/dec value
        if {[catch {expr $item} item_value]} {
            dvtConPuts stderr "Fill value or one of the initial values is not decimal or hex. It is $item"
            return "Error"
        }

        # range check fill value
        if {($item_value < 0) || ($item_value > 255)} {
            dvtConPuts stderr "Fill value or one of the initial values is out of range (0..255). It is $item"
            return "Error"
        }
    }

    # open file and configure for binary output
    if {[catch "open $binfilename w" fid]} {
        dvtConPuts stderr "File $binfilename could not be opened for writing"
        return "Error"
    }
    fconfigure $fid -translation binary

    # first write a load of zeros to the file up to the start address
    set current_location 0
    while {$current_location < $start_address} {
        incr current_location
        fputs -nonewline $fid \x00
    }

    # now load initial value
    foreach init_val $initial_values {
        set init_val [binary format c [expr $init_val]]
        if {$current_location < $end_address} {
            incr current_location
            fputs -nonewline $fid $init_val
        }
    }

    # now load the rest with the fill value
    set fill_value [binary format c [expr $fill_val]]
    while {$current_location < $end_address} {
        incr current_location
        fputs -nonewline $fid $fill_value
    }

    # once done close file 
    close $fid
    return "OK"
}


###############################################################################
#
# identify_bootloader
#
# PARAMETERS
# ==========
# node_id       - The Node ID.
#
# RETURNS
# =======
# OK if unit is running bootloader code, otherwise Error. Also sets the following 
# globals based on the bootloader version and revision:
#   - unit_variant  - Shiroko or Zeffer
#   - eeprom_1_byte - TRUE if EEPROM can only be read 1 byte at a time
#
proc identify_bootloader {{node_id 1}} {
    global unit_variant eeprom_1_byte
    
    set sw_version  [sdo_rns $node_id 0x5a10 0]
    
    set prefix      [string range $sw_version 0 0]
    set application [string range $sw_version 1 4]
    set revision    [string range $sw_version 6 9]
    
    if {![string is digit $application]} {
        dvtConPuts stderr "Application ($application) in $sw_version is invalid"
        return "Error"
    }
    
    if {![string is digit $revision] && ($revision != "----")} {
        dvtConPuts stderr "Revision ($revision) in $sw_version is invalid"
        return "Error"
    }
     
    switch $prefix {
        "H"     {
                    if {$application != 2} {
                        dvtConPuts stderr "Not a valid bootloader software version ($sw_version)"
                        return "Error"
                    }
                    
                    set unit_variant "Shiroko"
                    if {$revision < 9} {
                        set eeprom_1_byte TRUE
                    } else {
                        set eeprom_1_byte FALSE
                    }
                }
                
        "D"     {
                    set valid_applications(3)    "Zeffer"
                    set valid_applications(0303) "Zeffer"
                    set valid_applications(0401) "Chinook"
                    set valid_applications(0701) "Gen4"                    
                    if {[lsearch [array names valid_applications] $application] == -1} {
                        dvtConPuts stderr "Not a valid bootloader software version ($sw_version)"
                        return "Error"
                    }
                    
                    set unit_variant $valid_applications($application)
                    set eeprom_1_byte FALSE
                }
                
        default {
                    dvtConPuts stderr "Prefix ($prefix) in $sw_version is invalid"
                    return "Error"
                }
    }
    
    dvtConPuts "Bootloader: $unit_variant ($sw_version). EEPROM read limited to 1 byte: $eeprom_1_byte"
    return "OK"
}



###############################################################################
#
# bootloader_start
#
# PARAMETERS
# ==========
# node_id       - The Node ID.
#
# RETURNS
# =======
# Nothing
#
# DESCRIPTION
# ===========
# Requests the bootloader software to start
#
set helptext(bootloader_start) "Puts unit into bootloader mode"
set helptexta(bootloader_start) "bts"
set helptextd(bootloader_start) "
SYNOPSIS
    bootloader_start ?node_id_list?

DESCRIPTION
Puts unit into bootloader mode. node_id defaults to 1 if not specified.  Will take a list of node ids
to put into bootloader mode
"

interp alias {} bts {} bootloader_start

proc bootloader_start {{node_id_list 1}} {
    
    foreach node_id $node_id_list {
        if { [ catch { login $node_id } ] } { puts "Error logging in to node $node_id" }
        if { [ catch { force_pre_op $node_id PRE } ] } { puts "Error putting node $node_id into preop " }
        if { [ catch { sdo_wnx $node_id 0x5400 0 0xB0 } ] } { puts "" }
    }
    
}



###############################################################################
#
# bootloader_end
#
# PARAMETERS
# ==========
# node_id       - The Node ID.
#
# RETURNS
# =======
# Nothing
#
# DESCRIPTION
# ===========
# Requests the bootloader should exit and will run the software that
# has been programmed into the UUT. Warning - will commit the software
# that has been programmed - if the software is unable to reinitiate
# the bootloader then you must use the backdoor to fix it!!!
#
set helptext(bootloader_end) "Exits unit from bootloader mode"
set helptexta(bootloader_end) "bte"
set helptextd(bootloader_end) "
SYNOPSIS
    bootloader_end ?node_id_list?

DESCRIPTION
Requests the bootloader should exit and will run the software that has been programmed 
into the UUT.   Will take a list of node ids to exit bootloader mode. 
Warning - will commit the software that has been programmed - if the 
software is unable to reinitiate the bootloader then you must use the backdoor to fix it!!!
"

interp alias {} bte {} bootloader_end

proc bootloader_end {{node_id_list 1}} {
    foreach node_id $node_id_list {
        sdo_wnx $node_id 0x5a31 0 0xffff
        set sum [ sdo_rnx $node_id 0x5a31 0 ]
        sdo_wnx $node_id 0x5a31 0 $sum    
    }
}


###############################################################################
#
# bootloader_reset
#
# PARAMETERS
# ==========
# node_id       - The Node ID.
#
# RETURNS
# =======
# OK if bootloader is reset OK for the next load sequence
#
# DESCRIPTION
# ===========
# Resets the bootloader sequence for any subsequent downloads
#
proc bootloader_reset {{node_id 1}} {
    if {[sdo_wnx $node_id 0x5a32 0 0x01] == "OK"} {
        return "OK"
    } else {
        return "Error"
    }
}
    



###############################################################################
#
# load
#
# PARAMETERS
# ==========
# nodeid       - The Node ID.
# section      - Memory section to load file into
# binfilename  - The name of the file containing the binary data.  This is
#                assumed to start at address zero, and so will contain lots
#                of zeroes before the device start address.
#
# RETURNS
# =======
# The checksum of the written data.
#
# DESCRIPTION
# ===========
# Loads a program into a Shiroko controller using the bootloaders in the Host
# and DSP.  The Host and DSP must be running the bootloader when this proc is
# called.
#
set helptext(load_dld) "Load a dld file into  memory spaces specifed- assumes bootloader is running"
set helptextd(load_dld) "
SYNOPSIS
    load_dld nodeid dld_filename
    
DESCRIPTION
Downloads dld to the specified target memory space on node nodeid. Section can be any or all of
    dsp             - DSP slave on espAC (inc Dialbo/Chinook)
    eeprom          - EEPROM
    xc164-int       - Host XC164 internal flash 
    xc164-ext       - Host XC164 external flash 
    at49bv-ext      - Host 2812 external flash
    dsp-zeffer      - Host 2812 internal flash
    flash-config-a  - Host 2812 flash config A sector
    flash-config-b  - Host 2812 flash config B sector
    display         - CANopen display flash
"

proc load_dld {nodeids {dldfilename ""}} {
    global sdo_timeout
    set current_sdo_timeout $sdo_timeout

    global abort_load
    set abort_load 0
   
    #
    # Flush CAN Fifo before attempting loading a binary
    #
    if {[package present Dvtint] == 2.0} {
        Ccan flush
    }

    #define memory ranges codes with meaning
    set mem_space_map_str {0 "xc164-int" 1 "dsp" 2 "eeprom" 3 "xc164-ext" 4 "at49bv-ext" 5 "dsp-zeffer" 6 "flash-config-a" 7 "flash-config-b" 8 "display"}

    # if no DLD filename is specified open using tk_getOpenFile
    if {$dldfilename == ""} {
        set dldfilename [tk_getOpenFile -filetypes {{"Download packs" {*.dld}} {"All files" {*.*}}}]
    }
    
    # load DLD filename
    if {![file exists $dldfilename]} {return "cant find $dldfilename"}
    if {[catch {set fid [open $dldfilename]} err]} {
        return "unable to open file $err"
    }
    
    set dld [read $fid]
    close $fid

    # scan dld (text loaded into RAM) for the pattern which identifies the hex code, see how many code sections there are 
    # and note the indicies within the text array of the pattern matches.
    set target_code [regexp  -all -indices -inline -- {\+TARGET_MEMORY_SPACE[ \n]+([0-8])\n+\+CODE[ \n]+([:|A-F|a-f|0-9|\n]+)[ \n]+\+ENDCODE} $dld]

    if {$target_code == ""} {puts "Found no hex code data to work with in $dldfilename ??!!"; return}

    for {set i 0} {$i < [llength $target_code]} {incr i 3} {
        
        #
        # extract info from regex search
        #
        set memory_range                     [string range $dld [lindex $target_code [expr ($i+1)] 0] [lindex $target_code [expr ($i+1)] 1]]
        set mem_space_code($memory_range)    [string range $dld [lindex $target_code [expr ($i+2)] 0] [lindex $target_code [expr ($i+2)] 1]]
    }


    infomsg "found [llength [array names mem_space_code]] memory ranges in $dldfilename (modified [clock format [file mtime $dldfilename] -format {%D - %T}]): "

    # identify memory ranges and exctract code for each
    foreach target_memory_space [array names mem_space_code] {
        if  {[string is digit [set str [string map $mem_space_map_str $target_memory_space]]]} {
            puts "Unknown memory space $target_memory_space"
            return
        } else {
            infomsg \t...$str
        }
    }
    
        
   # Sub-indices for each object are (from TM-187):
   # 1   R  uint16   Max Block Size in bytes
   # 2   R  uint16   Page Size in bytes
   # 3   R  uint32   Device start address
   # 4   R  uint32   Device Length
   # 5   W  uint32   Start Address 
   # 6   W  uint16   Data Length
   # 7   W  uint8[]  Data
   # 8   W  uint16   Checksum

   # Set up some parameters according to the section
   # being programmed.  These are:
   #
   #  od_idx            - The relevant object index.
   #  minbinsize        - The minimum acceptable binfile size.  This is used to check
   #                       that the binfile looks realistic.
   #  new_sdo_timeout   - The SDO timeout to use during writes.  Because the erase
   #                       and program times can be quite long, the SDO timeout is
   #                       increased from the normal value.
    foreach nodeid $nodeids {
        foreach target_memory_space [array names mem_space_code] {
            infomsg -nonewline [format %-40s "programming [string map $mem_space_map_str $target_memory_space] on node $nodeid"]  
        
            switch [string map $mem_space_map_str $target_memory_space] {
               xc164-int {
                  set od_idx           0x5A20
                  set minbinsize       0x0010
                  set new_sdo_timeout  10000
               }
               dsp {
                  set od_idx           0x5A21
                  set minbinsize       0x0100
                  set new_sdo_timeout  40000
               }
               eeprom {
                  set od_idx           0x5A22
                  set minbinsize       0x0001
                  set new_sdo_timeout  10000
               }
               xc164-ext {
                  set od_idx           0x5A23
                  set minbinsize       0x0001
                  set new_sdo_timeout  15000
               }
               at49bv-ext {
                  set od_idx           0x5A23
                  set minbinsize       0x0001
                  set new_sdo_timeout  30000
               }
               dsp-zeffer {
                  set od_idx           0x5A24
                  set minbinsize       0x0100
                  set new_sdo_timeout  60000
               }
               flash-config-a {
                  set od_idx           0x5A25
                  set minbinsize       0x0100
                  set new_sdo_timeout  30000
               }
               flash-config-b {
                  set od_idx           0x5A26
                  set minbinsize       0x0100
                  set new_sdo_timeout  30000
               }
               display {
                   set od_idx          0x5A23
                   set minbinsize      0x0100
                   set new_sdo_timeout 15000
               }
               default {
                  dvtConPuts stderr "Unknown section for programming: $section"
                  return "Error"
               }
            }
        
            # Check the node exists and is communicating by reading its sofware version.
            #if {[identify_bootloader $nodeid] != "OK"} {
            #   dvtConPuts stderr "Can't communicate with node ID $nodeid, or bootloader not running"
            #   return "Error"
            #}
              
            # read memory data from controller
            set blocksize [sdo_rnx $nodeid $od_idx 1]
            #set blocksize 0x378
            if {[catch {expr $blocksize}]} {
               dvtConPuts stderr "Unable to read block size from $od_idx, 1. Returned $blocksize"
               return "Error"
            }
        
            set pagelength [sdo_rnx $nodeid $od_idx 2]
            #set pagelength 0x0080
            if {[catch {expr $pagelength}]} {
               dvtConPuts stderr "Unable to read page length from $od_idx, 2. Returned $pagelength"
               return "Error"
            }
        
            #set devicestartaddress 0x00c04000
            set devicestartaddress [sdo_rnx $nodeid $od_idx 3]
            if {[catch {expr $devicestartaddress}]} {
               dvtConPuts stderr "Unable to read device start address from $od_idx, 3. Returned $devicestartaddress"
               return "Error"
            }
        
            set devicelength [sdo_rnx $nodeid $od_idx 4]
            #set devicelength 0x0001c000
            if {[catch {expr $devicelength}]} {
               dvtConPuts stderr "Unable to read device length from $od_idx, 4. Returned $devicelength"
               return "Error"
            }
        
            # calculate device end address
            set deviceendaddress [ expr ( $devicestartaddress + $devicelength ) ]
        
        
            set startaddress [format 0x%08X $devicestartaddress]
            set sectionchecksum 0
        
            # convert hex to binary array 
            global bin_list
            catch {unset bin_list}
            array set bin_list [hex_to_bin $mem_space_code($target_memory_space)]
                
            # binary array should be a complete multiple of pages, but pad out if its not.

            # Start the packet counter
            set packet 1
            set codeposition [expr $devicestartaddress]
        
            # if the cs doesnt exist create it, if it does exist and we.re programming xc164 space leave it
            # to accumulate both memory spaces
            set espAC_checksum($target_memory_space) 0
        
            # find largest address...
            set max_addr 0
            foreach addr [array names bin_list] {
                if {$addr>$max_addr} {
                    set max_addr $addr
                }
            }

        
            # and packout to a whole page
            set c 0
            while {[expr ($max_addr-$devicestartaddress) % $pagelength != 0]} {
                incr max_addr
                incr c
                set bin_list($max_addr) 00
            }
        
            set section_size [array size bin_list]
            set progress     0
            set progress_now 0
                    
            while { ($codeposition < $max_addr) && !$abort_load } {
                      
                set n [expr int($blocksize/$pagelength)*$pagelength ]

                if { $codeposition + $n > $max_addr } {
                   set blocksize [ expr ( $max_addr - $codeposition ) ]
                   set n [expr int($blocksize/$pagelength)*$pagelength ]
                }
               
                # Get some bytes from the binary array up to the page length
                set hex ""
                for {set i $codeposition} {$i < [expr $codeposition + $n]} {incr i} {
                    append hex [format {0x%02x } $bin_list($i)]
                }

                incr codeposition $n
                incr progress $n
               
                set packetlength [format 0x%04X [llength $hex]]
                # Find out how many bytes were read from the file
                if {$packetlength == 0x0000} {
                      continue
                }

                # Update the packet and section checksums.  During debugging, the expected
                # packet checksum can be read from subindex 8.
                set packetchecksum 0
                incr packetchecksum [expr "0x[string range $startaddress 2 3]"]
                incr packetchecksum [expr "0x[string range $startaddress 4 5]"]
                incr packetchecksum [expr "0x[string range $startaddress 6 7]"]
                incr packetchecksum [expr "0x[string range $startaddress 8 9]"]
                incr packetchecksum [expr "0x[string range $packetlength 2 3]"]
                incr packetchecksum [expr "0x[string range $packetlength 4 5]"]
               
                foreach byte $hex {
                   incr packetchecksum  [expr $byte]
                   incr sectionchecksum [expr $byte]
                   incr espAC_checksum($target_memory_space)  [expr $byte]
                }
               
                # Inform user of progress - could have a progress bar later
                #if {$packet == 1} {
                #   dvtConPuts "[format %6d $packet]  Address: [format 0x%06X $startaddress] Len: [format 0x%06X $packetlength] - NOTE: Programming of the first block includes device erase cycle which can take some time."
                #} else {
                #   dvtConPuts "[format %6d $packet]  Address: [format 0x%06X $startaddress] Len: [format 0x%06X $packetlength]"
                #}

               
                # Write the start address to the OD (with a short SDO timeout)
                set sdo_timeout 1000
                set result [sdo_wnx $nodeid $od_idx 5 $startaddress]
                update
                if {$result != "OK"} {dvtConPuts stderr "Error ($result) writing to start address ($startaddress) for object $od_idx."; set sdo_timeout $current_sdo_timeout; bootloader_reset $nodeid; return "Error"}
               
                # Write the length to the OD (with a short SDO timeout)
                set result [sdo_wnx $nodeid $od_idx 6 [format 0x%04X $packetlength]]
                update
                if {$result != "OK"} {dvtConPuts stderr "Error ($result) writing to data length ($packetlength) for object $od_idx.";  set sdo_timeout $current_sdo_timeout; bootloader_reset $nodeid; return "Error"}
              
                # Write the packet data to the OD (with a short SDO timeout)
                set result [sdo_wn $nodeid $od_idx 7 $hex]
                update
                if {$result != "OK"} {dvtConPuts stderr "Error ($result) writing to data for object $od_idx.";  set sdo_timeout $current_sdo_timeout; bootloader_reset $nodeid; return "Error"}
               
                # Write this packet's checksum to the OD (with a long SDO timeout)
                set sdo_timeout $new_sdo_timeout
                set packetchecksum [expr (0xFFFF-$packetchecksum)%0x10000]
                set packetchecksum [format 0x%04X $packetchecksum]
               
                set result [sdo_wnx $nodeid $od_idx 8 $packetchecksum]
                update
                if {$result != "OK"} {dvtConPuts stderr "Error ($result) writing to checksum ($packetchecksum) for object $od_idx.";  set sdo_timeout $current_sdo_timeout; bootloader_reset $nodeid; return "Error"}
               
                # Update the start address for the next packet
                set startaddress [format 0x%08X [incr startaddress $packetlength]]
               
                incr packet
                if {[expr double($progress)/$section_size * 100] > $progress_now} {
                    infomsg -nonewline "."
                    incr progress_now 5
                }
            }
        
            #
            # todo checksum calculation
            #
            if {([string range [string map $mem_space_map_str $target_memory_space] 1 5] == "xc164")} {
                set xc164_cs 0
                if {[info exists espAC_checksum(0)]} {
                    incr xc164_cs $espAC_checksum(0)
                }
                if {[info exists espAC_checksum(3)]} {
                    incr xc164_cs $espAC_checksum(3)
                }
                #infomsg "$result ([af [expr (0xFFFF-$xc164_cs) % 10000] 17.0] )"
        } else {
                #infomsg "$result ([af [expr (0xFFFF-$espAC_checksum($target_memory_space)) % 10000] 17.0] )"
            }

            infomsg $result

            bootloader_reset $nodeid

            set sdo_timeout $current_sdo_timeout
        
            if {$abort_load} {
               dvtConPuts stderr "Load aborted by user"
            return "Error"
            }

        }                                                 
        infomsg "block checksum [af $sectionchecksum 32.0]"
     }
}





#
# proc converts standard intel hex to binary array
#
proc hex_to_bin {hex_code} {
    set hex_code [split $hex_code \n]
    set hex_code_len [llength $hex_code]
    if {$hex_code_len == 0} {
        error "not enough data $hex_code_len "; 
    } 
        
    set line_no 0    
    set finished 0
    set err 0
    set address_msb -1
    set tot_byte_count 0
    
    while { ($line_no <= $hex_code_len) && !$finished} {
        set line [lindex $hex_code $line_no]

        set start_code    [string range $line 0     0    ]
        set byte_count "0x[string range $line 1     2    ]"
        set address    "0x[string range $line 3     6    ]"
        set type       "0x[string range $line 7     8    ]"
        set data          [string range $line 9     end-2]
        set checksum   "0x[string range $line end-1 end  ]"
        
        # handle type:
        #   0x00 - Data bytes
        #   0x01 - End of hex record
        #   0x04 - New section
        switch $type {
            0x00 {
                    # sanity check byte_address incase its not been set first
                    if {$address_msb == -1} {
                        error "Data received before new section started"
                        set finished 1
                        set err 1
                    }
                    
                    # calculate byte address msb + address
                    set byte_address [expr $address_msb + $address]
                    
                    # split data bytes into seperate values
                    for {set byte 0} {$byte < $byte_count} {incr byte} { 
                
                        # add to list if in range
                        set byte_list($byte_address) "0x[string range $data [expr $byte * 2] [expr ($byte * 2) + 1]]"
                        incr byte_address
                        incr tot_byte_count
                    }
            
                 }
    
            0x01 {
                    # finished so break out of while loop
                    set finished 1
                 }
    
            0x04 {
                    # calculate address msb as data shifted into top word
                    set address_msb [expr 0x$data << 16]
                 }

            default {
                    set finished 1
                    error "Error: Unexpected type ($type)"
                 }
        }
        incr line_no
    }
    return [array get byte_list]
}
