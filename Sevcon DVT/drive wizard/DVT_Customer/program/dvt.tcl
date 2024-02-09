###############################################################################
# (C) COPYRIGHT Sevcon Limited 2003
# 
# CCL Project Reference C6944 - Magpie
# 
# FILE
#     $ProjectName:DVT_Customer$
#     $Revision:1.8$
#     $Author:ceh$
# 
# ORIGINAL AUTHOR
#     Martin Cooper
# 
# DESCRIPTION
#     Tcl GUI to run limited version of Design Verification Test System for
#     use by customers
# 
# REFERENCES
#     C6944-TM-171
# 
# MODIFICATION HISTORY
#     $Log:  80588: dvt.tcl 
# 
#     Rev 1.8    15/11/2007 14:21:22  ceh
#  Updated dll load proc to try and load in both VCI3 and 2. Added load.tcl to
#  source list at start up.
# 
#     Rev 1.7    10/10/2007 15:18:32  ceh
#  Updated files to be more like the main DVT versions.
# 
#     Rev 1.6    13/06/2007 17:30:38  ceh
#  Added emcy handler software
# 
#     Rev 1.5    10/05/2007 13:30:20  ceh
#  Added VCI 3
# 
#     Rev 1.4    28/02/2007 12:30:40  ceh
#  General updates to being up to date with latest common module definitions. 
# 
#     Rev 1.3    08/11/2006 13:49:48  ceh
#  Added module revision registering.
# 
#     Rev 1.2    01/09/2006 12:34:14  ceh
#  Added reduced version of uut.tcl to allow logging in etc.
# 
#     Rev 1.1    14/06/2006 13:12:50  ceh
#  Added CMPs new find proc
# 
#     Rev 1.0    14/06/2006 13:01:46  Supervisor
#  New DVT file for customer use only
# 
# 
###############################################################################

set toolname    "C6944 Shiroko Design Verification Test System - Customer Version"


# procedure to register files with the DVT.
proc register {section fname revision} {
    global revision_list_[set section]
    global total_revision_major_[set section]
    global total_revision_minor_[set section]
    
    # revision is in the format $Revision:1.8$
    set rev [split $revision :$.]
    set major [lindex $rev 2]
    set minor [lindex $rev 3]
    
    if {![info exists revision_list_[set section]]} {
        set total_revision_major_[set section] $major
        set total_revision_minor_[set section] $minor
    
        lappend revision_list_[set section] [list $fname $major.$minor]
    
    } else {
        # only register file if not already done so
        if {[string first $fname [set revision_list_[set section]]] == -1} {
            incr total_revision_major_[set section] $major
            incr total_revision_minor_[set section] $minor
    
            lappend revision_list_[set section] [list $fname $major.$minor]
        }
    }
    
    return
}

# procedure to list all registered files
proc revision_list {{section ALL}} {
    
    if {$section == "ALL"} {
        uplevel #0 {set var_list [lsearch -all -inline [info var] "revision_list_*"]}
        global var_list
        foreach var $var_list {
            lappend section_list [string range $var 14 end]
        }
    } else {
        set section_list $section
    }
    
    set revs ""
    foreach sec $section_list {
        global revision_list_[set sec]
    
        if {![info exists revision_list_[set sec]]} {
            dvtConPuts stderr "Section $sec is invalid."
            return "Error"
        } else {
            append revs "$sec ([total_revision $sec]):\n"
            set i 1
            foreach item [lsort [set revision_list_[set sec]]] {
                append revs "$i. [lindex $item 0] [lindex $item 1]\n"
                incr i
            }
        }
        
        append revs "\n"
    }
    
    return $revs
}

# procedure to return revision from all files
proc total_revision {section} {
    global total_revision_major_[set section]
    global total_revision_minor_[set section]
    
    if {![info exists total_revision_major_[set section]]} {
        dvtConPuts stderr "Section $section is invalid."
        return "Error"
    } else {
        return "[set total_revision_major_[set section]].[set total_revision_minor_[set section]]"
    }
}

# register the main DVT window
register DVT "dvt.tcl" {$Revision:1.8$}


# Possible CAN message types and colours for the CAN window
set cantypes(types)   {pdo  sdo  nmt      emcy  sync   tx    debug       unknown}
set cantypes(colours) {blue cyan magenta  red   orange black chocolate4  firebrick}
array set canbaud       [list 1000 1      100 0        500 0        250 0        125 0        50 0       20 0       10 0      ]
array set canbaud_names [list 1000 "1MHz" 100 "100kHz" 500 "500kHz" 250 "250kHz" 125 "125kHz" 50 "50kHz" 20 "20kHz" 10 "10kHz"]
set oldbaud 1000

# Possible console types and colours
set contypes(types)    {stderr stdout stdin help      debug   }
set contypes(colours)  {red    black  blue  darkgreen darkcyan}

# Possible types and colours for the CLI window
set clitypes(types)    {in    out}
set clitypes(colours)  {blue  red}

# Possible types and colours for the Information window. Not user specific ones should be appended to this list
set infotypes(types)   {none }
set infotypes(colours) {black}

# default editor
set editor notepad.exe

#availble hardware types
set hw_class_types {cont dyno uut_supp motor spare misc uut}


# images for buttons
source gif_data.tcl
image create photo .connected_gif    -format GIF -data $connect_img_data
image create photo .disconnected_gif -format GIF -data $disconnect_img_data
image create photo .separator_gif    -format GIF -data $separator_img_data
image create photo .construct_gif    -format GIF -data $construct_img_data
image create photo .copy_gif         -format GIF -data $copy_img_data
image create photo .cut_gif          -format GIF -data $cut_img_data
image create photo .delete_gif       -format GIF -data $delete_img_data
image create photo .open_gif         -format GIF -data $open_img_data
image create photo .paste_gif        -format GIF -data $paste_img_data
image create photo .tick_gif         -format GIF -data $tick_img_data
image create photo .cross_gif        -format GIF -data $cross_img_data
image create photo .stop_gif         -format GIF -data $stop_img_data
image create photo .clock_gif        -format GIF -data $clock_img_data
image create photo .vehicle_gif      -format GIF -data $vehicle_img_data
image create photo .info_gif         -format GIF -data $info_img_data
image create photo .reset_gif        -format GIF -data $reset_img_data
image create photo .chart_gif        -format GIF -data $chart_img_data
image create photo .play_gif         -format GIF -data $play_img_data
image create photo .pause_gif        -format GIF -data $pause_img_data
image create photo .graph_gif        -format GIF -data $graph_btn_data
image create photo .hbeat1_gif       -format GIF -data $hbeat1_img_data
image create photo .hbeat2_gif       -format GIF -data $hbeat2_img_data
image create photo .op_gif           -format GIF -data $op_img_data
image create photo .pre_op_gif       -format GIF -data $pre_img_data
image create photo .dock_gif         -format GIF -data $dock_img_data


# initialise statusbar labels
set label_text_com1 " COM1 open"

# system time
set system_time [clock format [clock seconds] -format "%a %d %b %Y  %H:%M:%S"]


# proc to update clock in status bar
proc run_clock {} {
  global system_time
  set system_time [clock format [clock seconds] -format "%a %d %b %Y  %H:%M:%S"]
  after 1000 run_clock
}



# 
# Generation of GUI & Configuration
#
proc genwindow {} {
   global con 
   global toolname toolver
   global cantypes can_show_type can_show_dvt_packets can_show_baud_buttons
   global clitypes cli_show_type canbaud
   global infotypes
   global contypes
   global debug_canopen
   global hw_class_types
   global canbaud_names oldbaud
   
   wm title . $toolname

   # toolbar
   frame .toolbar -relief flat -borderwidth 1
   pack  .toolbar -side left -fill y

   # Source
   button .toolbar.source_button \
       -image .open_gif \
       -command dvtConsoleSource
    
   set_balloon .toolbar.source_button "Source"
   
   pack .toolbar.source_button -side left -padx 1

   # Clear all windows
   button .toolbar.all_clear_button \
       -text " All" \
       -image .delete_gif \
       -compound left \
       -height 14 \
       -command {clear_all_windows}
   
   set_balloon .toolbar.all_clear_button "Clear all windows"

   pack .toolbar.all_clear_button -side left -padx 1

   # separator
   label .toolbar.sep1 -image .separator_gif
   pack .toolbar.sep1 -side left -padx 1

   # Cut
   button .toolbar.cut_button \
       -image .cut_gif \
       -command {event generate .conarea.txt <<Cut>> }
   
   set_balloon .toolbar.cut_button "Cut"

   pack .toolbar.cut_button -side left -padx 1

   # Copy
   button .toolbar.copy_button \
       -image .copy_gif \
       -command {event generate .conarea.txt <<Copy>> }
   
   set_balloon .toolbar.copy_button "Copy"

   pack .toolbar.copy_button -side left -padx 1

   # Paste
   button .toolbar.paste_button \
       -image .paste_gif \
       -command {event generate .conarea.txt <<Paste>> }
   
   set_balloon .toolbar.paste_button "Paste"

   pack .toolbar.paste_button -side left -padx 1

   # separator
   label .toolbar.sep2 -image .separator_gif
   pack .toolbar.sep2 -side left -padx 1

   # CAN window clear
   button .toolbar.can_clear_button \
       -text " CAN" \
       -image .delete_gif \
       -compound left \
       -height 14 \
       -command {.can.text delete 1.0 "end-1 chars"}
   
   set_balloon .toolbar.can_clear_button "Clear CAN Window"

   pack .toolbar.can_clear_button -side left -padx 1

   # separator
   label .toolbar.sep3 -image .separator_gif
   pack .toolbar.sep3 -side left -padx 1

   # CLI window clear
   button .toolbar.cli_clear_button \
       -text " CLI" \
       -image .delete_gif \
       -compound left \
       -height 14 \
       -command {cli_clear_all}
   
   set_balloon .toolbar.cli_clear_button "Clear CLI Window"

   pack .toolbar.cli_clear_button -side left -padx "1 5"

   # COM1 open
   button .toolbar.open_button_com1 \
       -image .connected_gif \
       -command {opencom 1} \
       -state disabled
   
   set_balloon .toolbar.open_button_com1 "Open COM1"

   pack .toolbar.open_button_com1 -side left -padx 1

   # COM1 close
   button .toolbar.close_button_com1 \
       -image .disconnected_gif \
       -command {closecom 1}
   
   set_balloon .toolbar.close_button_com1 "Close COM1"

   pack .toolbar.close_button_com1 -side left -padx "1 5"

   # CLI window dockall
   button .toolbar.cli_dockall_button \
       -text " Dock" \
       -image .dock_gif \
       -compound left \
       -height 14 \
       -command {.cli.tnb tab dockall}
   
   set_balloon .toolbar.cli_dockall_button "Dock all CLI windows"

   pack .toolbar.cli_dockall_button -side left -padx 1

   # separator
   label .toolbar.sep6 -image .separator_gif
   pack .toolbar.sep6 -side left -padx 1

   # Info window clear
   button .toolbar.info_clear_button \
       -text " Info" \
       -image .delete_gif \
       -compound left \
       -height 14 \
       -state disabled \
       -command {.info.text delete 1.0 "end-1 chars"}
   
   set_balloon .toolbar.info_clear_button "Clear Information Window"

   pack .toolbar.info_clear_button -side left -padx 1

   # separator
   label .toolbar.sep7 -image .separator_gif
   pack .toolbar.sep7 -side left -padx 1

   # Vehicle Interface
   button .toolbar.veh_if_button \
       -image .vehicle_gif \
       -command {if {[winfo exists .veh_if]} {raise .veh_if} else {source veh_if.tcl}}
   
   set_balloon .toolbar.veh_if_button "Vehicle Interface"

   pack .toolbar.veh_if_button -side left -padx 1

   # source veh i/f with register flag set so that it is registered, but not run
   global veh_if_register; set veh_if_register 1
   source veh_if.tcl
   set veh_if_register 0

   # separator
   label .toolbar.sep8 -image .separator_gif
   pack .toolbar.sep8 -side left -padx 1

   # Help
   entry .toolbar.help_entry -width 20 -fg grey
   bind  .toolbar.help_entry <Return>   {set hlp_txt [%W get]; if {($hlp_txt == "") || ([string tolower $hlp_txt] == "help")} {help " "} else {help $hlp_txt}}
   bind  .toolbar.help_entry <FocusIn>  {if {[%W get] == "Search for help here."} {%W delete 0 end} else {%W selection range 0 end}; %W configure -fg black}
   bind  .toolbar.help_entry <FocusOut> {if {[%W get] == ""} {%W insert 0 "Search for help here."; %W configure -fg grey} else {%W selection clear}}
   .toolbar.help_entry insert 0 "Search for help here."
   
   set_balloon .toolbar.help_entry "Help"

   pack .toolbar.help_entry  -side left -padx 1
   
   
   # statusbar
   frame .statusbar -relief flat -borderwidth 1
   pack  .statusbar -side left -fill y

   label .statusbar.label_com1 \
       -height 15 \
       -width 85 \
       -anchor w \
       -textvariable label_text_com1 \
       -background salmon \
       -image .connected_gif \
       -relief sunken \
       -compound left

   pack .statusbar.label_com1 -side left -padx 1

   label .statusbar.label_can_state \
       -height 15 \
       -width 95 \
       -anchor w \
       -text " CANbus Offline" \
       -background salmon \
       -image .cross_gif \
       -relief sunken \
       -compound left

   pack .statusbar.label_can_state -side left -padx 1

   label .statusbar.clock_display \
       -anchor e \
       -textvariable system_time \
       -relief sunken \
       -compound left

   pack .statusbar.clock_display -side right
   run_clock

   # Create the CAN frame
   frame       .can -relief groove -borderwidth 2 -padx 5 -pady 5
   label       .can.name -text "CAN - $canbaud_names($oldbaud)" 
   text        .can.text -height 5 -width 60 -yscrollcommand {.can.scry set} -xscrollcommand {.can.scrx set} -wrap none
   scrollbar   .can.scrx -command ".can.text xview" -orient horizontal
   scrollbar   .can.scry -command ".can.text yview" -orient vertical
   
   # The CAN stats frame
   frame       .can.stats -relief groove -borderwidth 0
   label       .can.stats.q_label      -text "Bus Load"
   label       .can.stats.q            -textvariable can_q -width 5
   label       .can.stats.qmax_label   -text "Qmax"
   label       .can.stats.qmax         -textvariable can_qmax -width 5
   label       .can.stats.ov           -textvariable can_ov -width 5 -fg red
   
   # buttons to change baud rate
   set i 0
   frame .can.bauds -relief groove -borderwidth 0
   foreach baud [lsort -integer [array names canbaud_names]] {
       button .can.bauds.$baud -text $canbaud_names($baud) -width 7 -command "set canbaud($baud) 1; set_baud"
       grid configure .can.bauds.$baud -column 0 -row $i
       incr i
   }
   .can.bauds.1000 configure -relief sunken -state disabled -background green
   
   grid configure .can.stats.q_label      -column 0 -row 0
   grid configure .can.stats.q            -column 0 -row 1 -padx 5
   grid configure .can.stats.qmax_label   -column 0 -row 2
   grid configure .can.stats.qmax         -column 0 -row 3 -padx 5
   grid configure .can.stats.ov           -column 0 -row 4 -padx 5 -pady 10
   
   grid configure .can.name   -column 0 -row 0  -sticky nsew
   grid configure .can.text   -column 0 -row 1  -sticky nsew
   grid configure .can.scrx   -column 0 -row 2  -sticky nsew
   grid configure .can.scry   -column 1 -row 1  -sticky nsew
   grid configure .can.bauds  -column 2 -row 1 -rowspan 2 -sticky nsew
   grid configure .can.stats  -column 3 -row 1 -rowspan 2 -sticky nsew
   
   grid rowconfigure    .can 0 -weight 0
   grid rowconfigure    .can 1 -weight 1 
   grid rowconfigure    .can 2 -weight 0 
   grid columnconfigure .can 0 -weight 1
   grid columnconfigure .can 1 -weight 0
   grid columnconfigure .can 2 -weight 0
   grid columnconfigure .can 3 -weight 0
   
   .can.text configure -font {"Courier New" 8}
   
   # remember the baud frame settings so we can hide/show it and initialise the status
   global canbaud_frame_info
   array set canbaud_frame_info [grid info .can.bauds]
   if {![info exists can_show_baud_buttons]} {
      set can_show_baud_buttons 0
   }
   display_can_baud_buttons $can_show_baud_buttons
   
   # Set up the tags for the CAN window
   set index 0
   foreach type $cantypes(types) {
      .can.text tag configure $type -foreground [lindex $cantypes(colours) $index]
      incr index
   }


   # Create the CLI text frame
   frame       .cli -relief groove -borderwidth 2 -padx 5 -pady 5
   label       .cli.name -text "CLI" 
   blt::tabnotebook .cli.tnb
   .cli.tnb configure -width  500
   .cli.tnb configure -height 5
   
   # format and size window
   grid configure .cli.name   -column 0 -row 0  -sticky nsew
   grid configure .cli.tnb    -column 0 -row 1  -sticky nsew

   grid rowconfigure    .cli 0 -weight 0
   grid rowconfigure    .cli 1 -weight 1
   grid columnconfigure .cli 0 -weight 1
   grid columnconfigure .cli 1 -weight 0
   
   # create a tabbed interface for the CLI. Create at least one window
   create_new_cli_window 1
   


   # Create the information window
   frame       .info -relief groove -borderwidth 2 -padx 5 -pady 5
   label       .info.name -text "Information" 
   text        .info.text -height 5 -width 60 -yscrollcommand {.info.scry set} -xscrollcommand {.info.scrx set} -wrap none
   scrollbar   .info.scrx -command ".info.text xview" -orient horizontal
   scrollbar   .info.scry -command ".info.text yview" -orient vertical
   
   grid configure .info.name   -column 0 -row 0  -sticky nsew
   grid configure .info.text   -column 0 -row 1  -sticky nsew
   grid configure .info.scrx   -column 0 -row 2  -sticky nsew
   grid configure .info.scry   -column 1 -row 1  -sticky nsew
   
   grid rowconfigure    .info 0 -weight 0
   grid rowconfigure    .info 1 -weight 1 
   grid rowconfigure    .info 2 -weight 0 
   grid columnconfigure .info 0 -weight 1
   grid columnconfigure .info 1 -weight 0
   
   .info.text configure -font {"Courier New" 8}
   
   # Set up the tags for the Information window
   set index 0
   foreach type $infotypes(types) {
      add_infomsg_colour_tag $type [lindex $infotypes(colours) $index]
      incr index
   }
   
   
   # Create the interpreter (console) window
   frame .conarea  -relief groove -borderwidth 2 -padx 5 -pady 5
   set con [text .conarea.txt  -yscrollcommand ".conarea.sb set" -width 80 -height 5]
   scrollbar .conarea.sb -command ".conarea.txt yview" 
   pack .conarea.txt -side left  -expand true -fill both
   pack .conarea.sb  -side right -fill both        
   $con configure -font {"Verdana" 10}


   # Create the window. Initially, set conarea to cover info window as well.
   grid configure .toolbar   -column 0 -row 0 -columnspan 2  -padx 2 -pady 2 -sticky nsew
   grid configure .can       -column 0 -row 1 -columnspan 1  -padx 2 -pady 2 -sticky nsew
   grid configure .cli       -column 1 -row 1 -columnspan 1  -padx 2 -pady 2 -sticky nsew
   grid configure .info      -column 0 -row 2 -columnspan 2  -padx 2 -pady 2 -sticky nsew
   grid configure .conarea   -column 0 -row 2 -columnspan 2  -padx 2 -pady 2 -sticky nsew -rowspan 2
   grid configure .statusbar -column 0 -row 4 -columnspan 2  -padx 2 -pady 2 -sticky nsew

   set con $con
   $con mark set limit insert
   $con mark gravity limit left
   
   # Add sensible window resize behaviour
   grid rowconfigure    . 0 -weight 0
   grid rowconfigure    . 1 -weight 2
   grid rowconfigure    . 2 -weight 1
   grid rowconfigure    . 3 -weight 3
   grid columnconfigure . 0 -weight 1
   grid columnconfigure . 1 -weight 1
   
   # Set up the tags for the console window
   set index 0
   foreach type $contypes(types) {
      if { ($type == "stderr") || ($type == "debug")} {
          $con tag configure $type -foreground [lindex $contypes(colours) $index] -font {"verdana bold" 10}
      } else {
          $con tag configure $type -foreground [lindex $contypes(colours) $index]
      }
      incr index
   }
              
   # Key bindings
   bind $con <Alt-Key-F4> {
     dvtexit 
     break
   }
        
   foreach left {Control-b Left} {
      bind $con <$left> {
         if {[%W compare insert == promptEnd]} {
            break
         }
      %W mark set insert insert-1c
      break
      }
   }
   
   foreach right {Control-f Right} {
      bind $con <$right> {
         %W mark set insert insert+1c
         break
      }
   }

   bind $con <Return> {
      %W mark set insert {end - 1c}
      dvtConsoleInsert %W "\n"
      dvtEvalCommand 
      break
   }
   
   bind $con <BackSpace> {
      if {[%W tag nextrange sel 1.0 end] != ""} {
         %W delete sel.first sel.last
      } elseif {[%W compare insert > limit]} {
         %W delete insert-1c
         %W see insert
      }
   break
   }
   
   bind $con <Up> {
      dvtConsoleHistory prev
      break
   }
   
   bind $con <Down> {
      dvtConsoleHistory next
      break
   }

   bind $con <Key> {
      if [%W compare insert < limit] {
         %W mark set insert end
      }
      dvtConsoleInsert %W %A
      break
   }
   
   foreach left {Control-a Home} {
      bind $con <$left> {
         if {[%W compare insert < promptEnd]} {
            %W mark set insert "insert linestart"
         } else {
            %W mark set insert promptEnd
         }
      break
      }
   }
   
   bind $con <Control-k> {
      %W delete promptEnd end
      break
   }

   bind $con <Delete> {
      if {[string compare [%W tag nextrange sel 1.0 end] ""]} {
         %W tag remove sel sel.first promptEnd
      } else {
         if {[%W compare insert < promptEnd]} {
            break
         }
      }
   }

   foreach right {Control-e End} {
      bind $con <$right> {
         %W mark set insert {insert lineend}
         break
      }
   }

   bind $con <<Cut>> {
      # Same as the copy event
      if {![catch {set data [%W get sel.first sel.last]}]} {
         clipboard clear -displayof %W
         clipboard append -displayof %W $data
         }
      break
   }

   # Close COM port on program exit
   bind $con <Destroy> {
      global com_chid
      dvtexit
      catch { close $com_chid }
   }

   bind $con  <<Copy>> {
      if {![catch {set data [%W get sel.first sel.last]}]} {
         clipboard clear -displayof %W
         clipboard append -displayof %W $data
      }
   }
   
   bind $con <<Paste>> {
      catch {
         set clip [selection get -displayof %W -selection CLIPBOARD]
         set list [split $clip \n\r]
         dvtConsoleInsert %W [lindex $list 0]
         foreach x [lrange $list 1 end] {
            %W mark set insert {end - 1c}
            dvtConsoleInsert %W "\n"
            dvtConsoleHistory
            dvtConsoleInsert %W $x
         }
      }
      break
   }
        
   
   # Generate Menubar
   menu .menubar
   .menubar add cascade -label File -menu .menubar.file
   .menubar add cascade -label Edit -menu .menubar.edit
   .menubar add cascade -label CAN  -menu .menubar.can
   .menubar add cascade -label CLI  -menu .menubar.cli
   .menubar add cascade -label "Info Window" -menu .menubar.info
   .menubar add cascade -label Vehicle -menu .menubar.vehicle
   .menubar add cascade -label Help -menu .menubar.help
   
   # File menu
   menu .menubar.file -tearoff 1
   .menubar.file add command -label Source -command dvtConsoleSource
   .menubar.file add command -label "Clear All" -command clear_all_windows
   .menubar.file add separator
   .menubar.file add command -label Exit -command exit -accel "Alt+F4"
   
   # Edit menu
   menu .menubar.edit -tearoff 1
   .menubar.edit add command -label Cut -command {event generate .conarea.txt <<Cut>> } -accel "Ctrl+X"
   .menubar.edit add command -label Copy -command {event generate .conarea.txt <<Copy>> } -accel "Ctrl+C"
   .menubar.edit add command -label Paste -command {event generate .conarea.txt <<Paste>> } -accel "Ctrl+V"

   # CAN menu
   menu .menubar.can -tearoff 1
   .menubar.can add command -label Clear -command {.can.text delete 1.0 "end-1 chars"} 
   .menubar.can add checkbutton -variable debug_canopen -label "Debug CANopen"
   .menubar.can add separator
   
   foreach type $cantypes(types) {
      set can_show_type($type) 1
      .menubar.can add checkbutton -variable can_show_type($type) -label "Show [string toupper $type] Packets"
   }
   
   # add DVT packets item
   set can_show_dvt_packets 1
  .menubar.can add separator
  .menubar.can add checkbutton -variable can_show_dvt_packets -label "Show DVT Packets"
  
  .menubar.can add separator
  .menubar.can add command -label "Show None" -command {
                                                           foreach type [array names can_show_type] {
                                                               global can_show_type
                                                               set can_show_type($type) 0
                                                           }
                                                           
                                                           global can_show_dvt_packets
                                                           set can_show_dvt_packets 0
                                                       } 
                                                       
  .menubar.can add command -label "Show All" -command {
                                                           foreach type [array names can_show_type] {
                                                               global can_show_type
                                                               set can_show_type($type) 1
                                                           }
                                                           
                                                           global can_show_dvt_packets
                                                           set can_show_dvt_packets 1
                                                       } 


  .menubar.can add separator
   
   foreach baud [lsort -integer [array names canbaud]] {
      .menubar.can add checkbutton -variable canbaud($baud) -label "CAN Baud - $baud" -command set_baud
   }

   # CAN baud quick buttons
   .menubar.can add separator
   .menubar.can add checkbutton -variable can_show_baud_buttons -label "Show CAN baud buttons" -command {display_can_baud_buttons $can_show_baud_buttons}
                                                                                                     
   # Serial menu
   menu .menubar.cli -tearoff 1
   .menubar.cli add command -label Clear -command {cli_clear_all}
   .menubar.cli add command -label "Open COM" -command opencom
   .menubar.cli add command -label "Close COM" -command closecom
   .menubar.cli add separator

   foreach type $clitypes(types) {
      set cli_show_type($type) 1
      .menubar.cli add checkbutton -variable cli_show_type($type) -label "Show [string toupper $type] Messages"
   }
   
   # Info Window menu
   global wininfo_state
   if {![info exists wininfo_state]} {
        set wininfo_state 0
   }
   menu .menubar.info -tearoff 0
   .menubar.info add checkbutton -variable wininfo_state -label "Show" -command {global wininfo_state; show_info_window $wininfo_state}
   .menubar.info add separator
   .menubar.info add command -label Clear -command {.info.text delete 1.0 "end-1 chars"} -state disabled
   
   
   # Vehicle menu
   menu .menubar.vehicle -tearoff 0
   .menubar.vehicle add command -label Interface -command {if {[winfo exists .veh_if]} {raise .veh_if} else {source veh_if.tcl}}
   
   
   # Help menu
   menu .menubar.help -tearoff 0
   .menubar.help add command -label Help -command help
   .menubar.help add command -label "Help on Help" -command help_on_help
   .menubar.help add command -label About -command about
   
   . configure -menu .menubar
   


   # The CAN pop-up menu
   menu .canpop -tearoff 0
   .canpop add command -label Save -command savecanwindow
   .canpop add command -label Clear -command {.can.text delete 1.0 "end-1 chars"}
    
   bind .can.text <Button-3> {
      tk_popup .canpop %X %Y
   }
   
   #short cuts to dissapeear the info window   
    bind .conarea <Double-Button-1> {
        global infotoggle
        toggle
        show_info_window $infotoggle
    }
    
    bind .info <Double-Button-1> {
        global infotoggle
        toggle
        show_info_window $infotoggle
    }   
}


#
# proc to show/hide CAN baud buttons
#
proc display_can_baud_buttons {state} {
    global canbaud_frame_info can_show_baud_buttons
    if {$state} {
        foreach cfg_item [array names canbaud_frame_info] {
            grid configure .can.bauds $cfg_item $canbaud_frame_info($cfg_item)
        }
        set can_show_baud_buttons 1
    } else {
        grid forget .can.bauds
        set can_show_baud_buttons 0
    }
}

#
# proc to add new CLI windows
#
proc create_new_cli_window {nodeid} {
   global clitypes cli_nodeid_to_tab
   
   # check if window already exists
    foreach tab [.cli.tnb tab names] {
        # All windows are labelled "Node <nodeid>"
        set win_name [.cli.tnb tab cget $tab -text]
        
        # if window is found, exit without doing anything else
        if {[string range $win_name 5 end] == $nodeid} {
            return
        }
    }
   
   # create a tabbed interface for the CLI. 
   set id [.cli.tnb insert end -text "Node [set nodeid]"]
   set cli_nodeid_to_tab($nodeid) $id

   # create a frame holding the text and scroll bars
   frame       .cli.tnb.win[set nodeid]      -relief groove -padx 5 -pady 5
   text        .cli.tnb.win[set nodeid].text -height 19 -width 90 -yscrollcommand ".cli.tnb.win[set nodeid].scry set" -xscrollcommand ".cli.tnb.win[set nodeid].scrx set" -wrap none
   scrollbar   .cli.tnb.win[set nodeid].scrx -command ".cli.tnb.win[set nodeid].text xview" -orient horizontal
   scrollbar   .cli.tnb.win[set nodeid].scry -command ".cli.tnb.win[set nodeid].text yview" -orient vertical

   grid configure .cli.tnb.win[set nodeid].text   -column 0 -row 0  -sticky nsew
   grid configure .cli.tnb.win[set nodeid].scrx   -column 0 -row 1  -sticky nsew
   grid configure .cli.tnb.win[set nodeid].scry   -column 1 -row 0  -sticky nsew

   .cli.tnb.win[set nodeid].text configure -font {"Courier New" 8}
   
   # add to tab
   .cli.tnb tab configure $id -window .cli.tnb.win[set nodeid]
   .cli.tnb configure -tearoff yes

    # Set up the tags for the CLI window
   set index 0
   foreach type $clitypes(types) {
      .cli.tnb.win[set nodeid].text tag configure $type -foreground [lindex $clitypes(colours) $index]
      incr index
   }
   
   # set up window to size correctly.
   grid rowconfigure    .cli.tnb.win[set nodeid] 0 -weight 1
   grid rowconfigure    .cli.tnb.win[set nodeid] 1 -weight 0 
   grid columnconfigure .cli.tnb.win[set nodeid] 0 -weight 1
   grid columnconfigure .cli.tnb.win[set nodeid] 1 -weight 0

   
   # The CLI pop-up menu
   menu .clipop[set nodeid] -tearoff 0
   .clipop[set nodeid] add command -label Save -command "savecliwindow $nodeid"
   .clipop[set nodeid] add command -label Clear -command "cli_clear $nodeid"
   
   bind .cli.tnb.win[set nodeid].text <Button-3> "tk_popup .clipop[set nodeid] %X %Y"
}


#
# set baud to user requirement
#
proc set_baud {} {
    global canbaud canbaud_names oldbaud
    
    # the newbaud rate is set in canbaud() prior to this function being called. So there
    # should be 2 baudrates set, the new one and the old one. If we can't find a new set
    # baudrate, then the old one must still be set.
    set new_baud "null"
    foreach baud [array names canbaud] {
        if {($baud != $oldbaud) && $canbaud($baud)} {
            set new_baud $baud
            break
        }
    }
   
    if {$new_baud != "null"} {
        .can.bauds.$oldbaud configure -relief raised -state normal -background SystemButtonFace
        
        # update CAN baud rate
        set canbaud($oldbaud) 0
        can stop
        can cfg_baud $new_baud
        can start            
        set oldbaud $new_baud
        
        #update window name to show baudrate
        .can.name configure -text "CAN - $canbaud_names($new_baud)" 
        .can.bauds.$new_baud configure -relief sunken -state disabled -background green
    } else {
        # force baudrate to be reselected
        set canbaud($oldbaud) 1
    }
}


global infotoggle
set infotoggle true


proc toggle {} {
    global infotoggle    
    if {$infotoggle} { 
        set infotoggle false
    } else { 
        set infotoggle true
    }
}




# proc to clear the CAN, CLI and Information windows.
proc clear_all_windows {} {
   .can.text delete 1.0 "end-1 chars"
   cli_clear_all
   .info.text delete 1.0 "end-1 chars"
}


# proc to display the info window. We hide the info window by moving the conarea to row
# 2 and making it span 2 rows. This covers the info window.
proc show_info_window {state} {
    global wininfo_state
    if {$state} {
        grid configure .conarea -row 3 -rowspan 1
        .toolbar.info_clear_button configure -state normal
        .menubar.info entryconfigure [.menubar.info index "Clear"] -state normal
        set wininfo_state 1
    } else {
        grid configure .conarea -row 2 -rowspan 2
        .toolbar.info_clear_button configure -state disabled
        .menubar.info entryconfigure [.menubar.info index "Clear"] -state disabled
        set wininfo_state 0
    }
}

# proc to update tags used to change msg colours in info window. Should be called by functions requiring the info window
proc add_infomsg_colour_tag {type colour} {
   global infotypes
   
   lappend infotypes(types)   $type
   lappend infotypes(colours) $colour
   
   .info.text tag configure $type -foreground $colour
}


# help         - Lists all commands in the help system with a brief summary of each
# help key     - List all commands containing the key in their name or help text.  If there
#                 is a command called key, print detailed help if there is any.
proc help { {key " "} } {
   global helptext helptexta helptextd
   
   # help text
   set help_window_text ""

   # if check if key is a command or a command alias
   set found 0
   if {[info exists helptext($key)]} {
      incr found
   } else {
      foreach cmd [array names helptexta] {
         if {$helptexta($cmd) == $key} {
            incr found
            set key $cmd
            break
         }
      }
   }
   
   if {$found > 0} {
      # Detailed help
      if {[info exists helptexta($key)]} {
         append help_window_text "[format %-30s $key] -- [format %-10s $helptexta($key)] -- $helptext($key)\n"
      } else {
         append help_window_text "[format %-44s $key] -- $helptext($key)\n"
      }
      
      # Also print detailed help, if the good Tcl programmer has provided any
      if {[info exists helptextd($key)]} {
         append help_window_text "$helptextd($key)\n"
      }
   } else {
      # List commands
      foreach topic [lsort [array names helptext]] {
         if {([string first $key $topic] != -1) || ([string first $key $helptext($topic)] != -1)} {
            incr found        
            if {[info exists helptexta($topic)]} {
               append help_window_text "[format %-30s $topic] -- [format %-10s $helptexta($topic)] -- $helptext($topic)\n"
            } else {
               append help_window_text "[format %-44s $topic] -- $helptext($topic)\n"
            }
         }
      }
   }
   
   if {$found == 0} {
      tk_messageBox -icon info -title "Help" -message "No help on $key.  Type \"help\" for a list of commands"
   }
   
   if {$key == " "} {
      append help_window_text "\nType \"help subject\" for more information\n"
   }
   
   if {$help_window_text != ""} {
       set help_window_text "Command                        -- Alias      -- Description\n$help_window_text"
       output_win $help_window_text "Help" lightblue black {"Courier" 8}
   }
   
   return
}


# help_on_help - Explains how help works and how to add new items to help.
proc help_on_help {} {

set txt "
Help on Help
------------

Help can be easily added to any command in the DVT. To use simply add the appropriate text to one of 
the following global arrays:

    - helptext(name)    - Contains summarised help text.
    - helptexta(name)   - Optional. Use if a command has an alias (eg lg for login)
    - helptextd(name)   - Contains detailed help text.


Example:
set helptext(login) \"Logs into the controller.\"
set helptexta(login) \"lg\"
set helptextd(login) \"
SYNOPSIS
    login ?node_id? ?level? ?userid?
    
DESCRIPTION
Logs into a device. node_id defaults to 1 if not specified. level defaults to 5 (SEVCON access). 
If set to ? displays current access level. Setting to 0 logs out. userid is the user id. Defaults to 0.
\"


Help Etiquette:
    - Help text should only be used on commonly used commands entered from the command line. Don't use 
      this for commands only intended for scripts. These commands should have a suitably descriptive 
      function header in comments.
    
    - The summarised help text should fit on one line.
    
    - Aliases must be listed in the alias array.
    
    - Detailed descriptions must use the following format:
            SYNOPSIS
                name arg1 arg2 ?optional_arg1? ?optional_arg2?
            
            DESCRIPTION
            Detailed description here including all arguments.
    
        Optional arguments should be delimited with ?
"
    
    output_win $txt "Help on Help" cyan black {"Courier" 8}
}


proc about {} {
   global toolname toolver

   dvtConPuts "$toolname [string map {$ ""} $toolver]"
   if {[info exists can_availble]} {
       can ver
   }
   dvtConPuts "Tcl patch level: [info patchlevel]"
   dvtConPuts "Tcl library:     [info library]"
}


# -----------------------------------------------------------------------------
# savecanwindow - save the text in the can window to a file
#
proc savecanwindow {} {
   set filename [tk_getSaveFile -defaultextension .txt -parent . \
      -title "Select a file" \
      -initialfile "can.txt" \
      -filetypes {{"Text Files" .txt} {"All Files" *}}]

   if {$filename != ""} {      
      if { [catch { open $filename w } fid ] } {
         dvtConPuts stderr "$filename could not be opened"
      } else {
         set t [.can.text get 1.0 end]
         fputs $fid $t
         close $fid
      }
   }
}


# -----------------------------------------------------------------------------
# savecliwindow - save the text in the can window to a file
#
proc savecliwindow {nodeid} {
   set filename [tk_getSaveFile -defaultextension .txt -parent . \
      -title "Select a file" \
      -initialfile "cli[set nodeid].txt" \
      -filetypes {{"Text Files" .txt} {"All Files" *}}]
      
   if {$filename != ""} {      
      if { [catch { open $filename w } fid ] } {
         dvtConPuts stderr "$filename could not be opened"
      } else {
         # save selected window to a file
         set t [.cli.tnb.win[set nodeid].text get 1.0 end]
         fputs $fid $t
         close $fid
      }
   }
}


# -----------------------------------------------------------------------------
# clearall - clear all windows
#
proc clearall {} {
   .con.txt     delete 1.0 "end-1 chars"
    cli_clear_all
   .can.text    delete 1.0 "end-1 chars"
}


# -----------------------------------------------------------------------------
# canmsg
#
# canmsg ?-nonewline? string ?chan?

proc canmsg {args} {
   global cantypes can_show_type
  
   if {[llength $args] > 3} {
      error "invalid arguments to canmsg"
   }
   
   if {[llength $args] == 0} {
      error "invalid arguments to canmsg"
   }
   
   # Look for "-nonewline" as the first argument
   set newline "\n"
   
   if {[string match "-nonewline" [lindex $args 0]]} {
      set newline ""
      set args [lreplace $args 0 0]
   }
   
   # Work out the channel
   if {[llength $args] == 1} {
      set chan ""
      set string [lindex $args 0]$newline
   } else {
      set chan [lindex $args 1]
      set string [lindex $args 0]$newline
   }
   
   if {![string compare $string ""]} {
      return
   }
   
   if {[lsearch $cantypes(types) $chan] == -1} {
      .can.text insert end "$string"
      .can.text see end
   } elseif {$can_show_type($chan)} {   
      .can.text insert end $string $chan
      .can.text see end
   } else {
      # This type is turned off
   }
}


# -----------------------------------------------------------------------------
# cli_clear
#
# Clears selected CLI window
proc cli_clear {nodeid} {
    .cli.tnb.win[set nodeid].text delete 1.0 "end-1 chars"
}


# -----------------------------------------------------------------------------
# cli_clear_all
#
# Clears all CLI windows
proc cli_clear_all {args} {
    foreach tab [.cli.tnb tab names] {
        # All windows are labelled "Node <nodeid>"
        set win_name [.cli.tnb tab cget $tab -text]
        set nodeid [string range $win_name 5 end]
        
        .cli.tnb.win[set nodeid].text delete 1.0 "end-1 chars"
    }
}


# -----------------------------------------------------------------------------
# climsg
#
# climsg ?-nonewline? string ?chan? ?nodeid?
proc climsg {args} {
   global clitypes cli_show_type cli_nodeid_to_tab
  
   if {[llength $args] > 4} {
      error "invalid arguments to climsg"
   }
   
   if {[llength $args] == 0} {
      error "invalid arguments to climsg"
   }
   
   # Look for "-nonewline" as the first argument
   set newline "\n"
   
   if {[string match "-nonewline" [lindex $args 0]]} {
      set newline ""
      set args [lreplace $args 0 0]
   }
   
   # Work out the channel
   if {[llength $args] < 2} {
      set chan ""
      set string [lindex $args 0]$newline
   } else {
      set chan [lindex $args 1]
   }
   
   # work out the nodeid
   if {[llength $args] < 3} {
      set nodeid 1
   } else {
      set nodeid [lindex $args 2]
      
      # check if window exists
      if {![winfo exists .cli.tnb.win[set nodeid]]} {return}
   }
   
   # set string
   set string [lindex $args 0]$newline
   
   if {![string compare $string ""]} {
      return
   }
   
   if {[lsearch $clitypes(types) $chan] == -1} {
      .cli.tnb.win[set nodeid].text insert end "$string"
      .cli.tnb.win[set nodeid].text see end
   } elseif {$cli_show_type($chan)} {   
      .cli.tnb.win[set nodeid].text insert end $string $chan
      .cli.tnb.win[set nodeid].text see end
   } else {
      # This type is turned off
   }

   # focus on window receiving data
   .cli.tnb select $cli_nodeid_to_tab($nodeid)
}

#
# cli short entry cut
#
proc clittx { } {
    global clitxstr
    cli $clitxstr
}


# -----------------------------------------------------------------------------
# infomsg
#
# infomsg ?-nonewline? string ?chan?

proc infomsg {args} {
   global infotypes
   
   if {[llength $args] > 3} {
      error "invalid arguments to infomsg"
   }
   
   if {[llength $args] == 0} {
      error "invalid arguments to infomsg"
   }
   
   # Look for "-nonewline" as the first argument
   set newline "\n"
   
   if {[string match "-nonewline" [lindex $args 0]]} {
      set newline ""
      set args [lreplace $args 0 0]
   }
   
   # Work out the channel
   if {[llength $args] == 1} {
      set chan ""
      set string [lindex $args 0]$newline
   } else {
      set chan [lindex $args 1]
      set string [lindex $args 0]$newline
   }
   
   if {![string compare $string ""]} {
      return
   }
   
   if {[lsearch $infotypes(types) $chan] == -1} {
      .info.text insert end "$string"
      .info.text see end
   } else {   
      .info.text insert end $string $chan
      .info.text see end
   }
}

# -----------------------------------------------------------------------------
# dvtEvalCommand - evaluate command entered in console window. 
#                - copied from console.tcl
        
proc dvtEvalCommand {} {
   global con
   
   set ranges [$con tag ranges input]
   set cmd ""
   
   if {[llength $ranges]} {
      set pos 0
      while {[string compare [lindex $ranges $pos] ""]} {
         set start [lindex $ranges $pos]
         set end [lindex $ranges [incr pos]]
         append cmd [$con get $start $end]
         incr pos
      }
   }

   if {![string compare $cmd ""]} {
      dvtConsolePrompt
   } elseif {[info complete $cmd]} {
      $con mark set output end
      $con tag delete input
      set result [dvtEval $cmd]
      if {[string compare $result ""]} {
         dvtConPuts $result
      }
      dvtConsoleHistory reset
      dvtConsolePrompt
   } else {
      dvtConsolePrompt partial
   }
   
   $con yview -pickplace insert
}

# -----------------------------------------------------------------------------
# dvtEval - evaluate command "command". If unfinished then print '>'s, if suffixed
#         - with '&' then run in background
#


proc dvtEval {command} {
   global con
   
   # don't need many globals, because we use uplevel #0
   $con mark set insert end
   
   if {$command != "\n"} {
      history add $command
   }
   
   if {[lindex $command end] == "&"} {
      puts -nonewline "Process executing in background. ID = "
      set command [lreplace $command end end]
      set command [linsert $command 0 after 1]
   }
   
   if {[catch {uplevel #0 $command} result]} {
      $con insert insert $result stderr
   } else {
      $con insert insert $result info
   }
   
   if {[$con compare insert != "insert linestart"]} {
      $con insert insert \n
   }
   
   $con see insert
   $con mark set limit insert
   
   return
}

# -----------------------------------------------------------------------------
# dvtConPuts - output to console window
#
# args are:
#  dvtConPuts ?-nonewline? chan string
#
# chan can be from $contypes(types)
#
proc dvtConPuts {args} {
   global con contypes
   
   if {[llength $args] > 3} {
      error "invalid arguments"
   }
   
   if {[llength $args] == 0} {
      error "invalid arguments"
   }
   
   set newline "\n"
   
   if {[string match "-nonewline" [lindex $args 0]]} {
      set newline ""
      set args [lreplace $args 0 0]
   }
   
   # if only one arg passed its a Console output
   if {[llength $args] == 1} {
      set chan stdout
      set string [lindex $args 0]$newline
   } else {
      set chan [lindex $args 0]
      set string [lindex $args 1]$newline
   }
   
   # search for the colourful output channel
   if {[lsearch $contypes(types) $chan] != -1} {
      $con mark gravity limit right
      $con insert end $string $chan
      $con see end
      $con mark gravity limit left
   } else {
      # asssume that what has been passed is a file IO channel 
      fputs $chan $string
      
      #$con mark gravity limit right
      #$con insert end $string 
      #$con see end
      #$con mark gravity limit left
   }       
}

proc dvtConsoleSource {} {
   set filename [tk_getOpenFile -defaultextension .tcl -parent . \
      -title "Select a file to source" \
      -filetypes {{"Tcl Scripts" .tcl} {"All Files" *}}]
      
   if {[string compare $filename ""]} {
      set cmd [list source $filename]
      if {[catch {dvtEval $cmd} result]} {
         dvtConPuts stderr "$result\n"
      }
   }
}

proc ls {{args "*.*"}} {
   dvtConPuts stdout [exec dir $args]
}

# tkConsoleInsert --
# Insert a string into a text at the point of the insertion cursor.
# If there is a selection in the text, and it covers the point of the
# insertion cursor, then delete the selection before inserting.  Insertion
# is restricted to the prompt area.
#
# Arguments:
# w -           The text window in which to insert the string
# s -           The string to insert (usually just a single character)

proc dvtConsoleInsert {w s} {
   if {![string compare $s ""]} {
      return
   }
   
   catch {
      if {[$w compare sel.first <= insert] && [$w compare sel.last >= insert]} {
         $w tag remove sel sel.first promptEnd
         $w delete sel.first sel.last
      }
   } 
   
   if {[$w compare insert < limit]} {
      $w mark set insert end       
   }
   
   $w insert insert $s {input stdin}
   $w see insert
}

# tkConsoleOutput --
#
# This routine is called directly by ConsolePutsCmd to cause a string
# to be displayed in the console.
#
# Arguments:
# dest -        The output tag to be used
# string -      The string to be displayed

proc dvtConsoleOutput {dest string} {
    global con
    $con insert output $string $dest
    $con see insert
}

#
# dvtConsoleHistory - keep a track of console commands in the history
#

set histNum 1
proc dvtConsoleHistory {cmd} {
   global histNum con
   
   switch $cmd {
      prev {
         incr histNum -1
         if {$histNum == 0} {
            set cmd {history event [expr {[history nextid] -1}]}
         } else {
            set cmd "history event $histNum"
         }
         if {[catch {eval $cmd} cmd]} {
            incr histNum
            return
         }
         $con delete limit end
         $con insert limit $cmd {input stdin}
      }
      next {
         incr histNum
         if {$histNum == 0} {
            set cmd {history event [expr {[history nextid] -1}]}
         } elseif {$histNum > 0} {
            set cmd ""
            set histNum 1
         } else {
            set cmd "history event $histNum"
         }
         if {[string compare $cmd ""]} {
            catch {eval $cmd} cmd
         }
         $con delete limit end
         $con insert limit $cmd {input stdin}
      }
      reset {
         set histNum 1
      }
   }
}


# tkConsolePrompt --
# This procedure draws the prompt.  If tcl_prompt1 or tcl_prompt2
# exists in the main interpreter it will be called to generate the 
# prompt.  Otherwise, a hard coded default prompt is printed.
#
# Arguments:
# partial -     Flag to specify which prompt to print.

proc dvtConsolePrompt {{partial normal}} {
   global con
   
   if {![string compare $partial "normal"]} {
      set temp [$con index "end - 1 char"]
      $con mark set output end
      dvtConPuts -nonewline "dvt([history nextid]) % "
   } else {
      set temp [$con index output]
      $con mark set output end
      dvtConPuts -nonewline "> "
   }
   
   flush stdout
   
   $con mark set output $temp
   $con mark set insert end
   $con mark set promptEnd insert
   $con mark gravity promptEnd left
}


#
# bgerror - used to print a background error (Tcl)
#
#proc bgerror {err_msg} {
#   puts bgerr "$err_msg"
#   after cancel idle_routine
#   after 50 idle_routine
#}

#
# stopped - used to test the stop_loop variable.
#           e.g. while !stopped
#
proc stopped {} {
        global stop_loop
        update
        return $stop_loop
}
proc notstopped {} {
        global stop_loop
        update
        return [expr !$stop_loop ]
}

#
# edit - run the default editor on file filename
#
proc edit {{fname}} {
   global editor
   uplevel #0 exec $editor $fname
}


#
# Send "cmd" out to the serial port
#
proc cli { cmd } {
   global com_chid
   global cli_response
   global cli_delay
   
   set cli_response ""
    
   if {$cli_delay == 0} {
      # No delay between characters
      fputs $com_chid $cmd\r
      flush $com_chid
   } else {
      # Insert delay between characters
      for {set i 0} {$i<[string length $cmd]} {incr i} {
         fputs -nonewline $com_chid [string index $cmd $i]
         after $cli_delay
      }
      fputs $com_chid \r
      after $cli_delay
   }
   
   climsg $cmd out
}


# Open the COM port if possible
proc opencom {{comport 1}} {
   global com_chid
   global label_text_com$comport
   
   if { [catch { open com$comport: RDWR } r ] } {
      dvtConPuts stderr "COM$comport could not be opened"
   } else {
      .toolbar.open_button_com$comport configure -state disabled
      .toolbar.close_button_com$comport configure -state normal
      .statusbar.label_com$comport configure -image .connected_gif -background lightgreen
      set label_text_com$comport " COM$comport open"

      set com_chid $r
      climsg "COM$comport opened"
      fconfigure $com_chid -mode 57600,n,8,1 -buffering none -blocking false -translation {auto binary}
      fileevent $com_chid readable "cli_read"
   }
}

#
# Process can messages as nessescary and pass to dll
#
proc can { args } {
    global can_show_dvt_packets
    
    #capture can send to the can window
    if {[lindex $args 0] == "send"} {
        set op ""
        foreach b [lindex $args 1] {
            append op "[format 0x%02x $b] "
        }
        
        if {$can_show_dvt_packets} {
            canmsg $op tx
        }
    }     
    
    eval Ccan $args
}

#
# Process the daq commands as nessescary, ie dont allow digital read from a
# port assigned as an output (it will turn on)
#
proc daq { args } {
    global daq_available
    if {$daq_available} { 
       eval Cdaq $args
    } else { 
       eval tcl_daq $args
    }    
}



# Close the COM port
proc closecom {{comport 1}} {
   global com_chid
   global label_text_com$comport
   
   if { [catch { close $com_chid } r ] } {
      dvtConPuts stderr "COM$comport: could not be closed"
   } else {
      .toolbar.open_button_com$comport configure -state normal
      .toolbar.close_button_com$comport configure -state disabled
      .statusbar.label_com$comport configure -image .disconnected_gif -background salmon
      set label_text_com$comport " COM$comport closed"

      climsg "COM$comport closed"
   }
}


# Load the DLL if possible
proc loaddll {} {
    set dll_list {{dvtint_vci3.dll "VCI 3"} {dvtint.dll "VCI 2"}}
        
    # try and load each DLL in turn. Run with the first successful load. If nothing loads OK, run dll sim
    foreach dll $dll_list {
        set dll_file [lindex $dll 0]
        set dll_name [lindex $dll 1]
        
        if {![catch {load $dll_file Dvtint} err]} {
            if { [catch {package require Dvtint 2.0} err] } {
                dvtConPuts stderr "Unable to load CAN DLL. Error: $err"
            }
            dvtConPuts help "Successfully loaded $dll_name."
            return
        } else {
            dvtConPuts stderr "Failed to load $dll_name."
        }
    }
    
    # if we got this far, something went wrong. Load DLL SIM instead
    dvtConPuts stderr "Unable to load any CAN DLL."
    source dllsim.tcl
}


# save history list to file so it can be restored next time we load
proc save_history {} {
    set n_entries [history nextid]
    if {$n_entries > 0} {
        # open history file
        if {![catch {open "history.tcl" w} fid]} {
        
            # find out the max number of history items
            set history_len [history keep]
            if {$n_entries > $history_len} {
                set start [expr $n_entries - $history_len]
            } else {
                set start 1
            }
            
            # write commands to the history file
            for {set i $start} {$i < $n_entries} {incr i} {
                fputs $fid "history add \{[history event $i]\}"
            }
            
            close $fid
        }
    }
}

# save workspace to file so it can be restored later
proc save_workspace {} {
    global oldbaud can_show_type wininfo_state can_show_baud_buttons dvt_last_selected_dir
    
    # get window data as soon as possible
    set win_state    [wm state .]
    set win_geometry [winfo geometry .]
    
    # open workspace file
    if {![catch {open "workspace.tcl" w} fid]} {
        # window state
        fputs $fid "# window state"
        fputs $fid "wm state . $win_state"
        if {$win_state != "zoomed"} {
            fputs $fid "wm geometry . $win_geometry"
        }
        fputs $fid "\n"
        
        # last selected CAN rate
        fputs $fid "# set CAN baud rate"
        fputs $fid "if \{\$can_available\} \{"
        fputs $fid "    set newbaud $oldbaud"
        fputs $fid "    set canbaud(\$newbaud) 1"
        fputs $fid "    set_baud"
        fputs $fid "\}\n"
        
        # restore CAN show types
        fputs $fid "# set CAN types"
        foreach type [array names can_show_type] {
            fputs $fid "set can_show_type($type) $can_show_type($type)"
        }
        fputs $fid "\n"
        
        # restore CAN baud buttons
        fputs $fid "# set CAN baud buttons state"
        fputs $fid "display_can_baud_buttons $can_show_baud_buttons"
        fputs $fid "\n"
        
        # restore info window state
        fputs $fid "# information window"
        fputs $fid "show_info_window $wininfo_state"
        fputs $fid "\n"
        
        # save tk_getOpenFile's last directory
        if {(![info exists dvt_last_selected_dir]) || (![file isdirectory $dvt_last_selected_dir])} {
            set dvt_last_selected_dir [pwd]
        }
        fputs $fid "# restore tk_getOpenFile's last directory"
        fputs $fid "if \{!\[file isdirectory \{$dvt_last_selected_dir\}\]\} \{"
        fputs $fid "    set dvt_last_selected_dir \[pwd\]"
        fputs $fid "\} else \{"
        fputs $fid "    set dvt_last_selected_dir \{$dvt_last_selected_dir\}"
        fputs $fid "\}\n"
        
        fputs $fid "return"
        close $fid
    }
}


#release hardware on exit
proc dvtexit { } {
    global idlepollid daqidlepollid daq_available
   
    # save workspace configuration
    save_workspace
    
    # save history list
    save_history
    
    if {$daq_available} {        
#        set_do key off
    }
    
    #stop 10ms idle poll
    after cancel $idlepollid    

    #stop daq idle poll
    after cancel $daqidlepollid    

    #close can bus if availble
    if {![catch {can stop} err]} {
        can close
    }     
    #release daq hardware if available
    catch {daq release}
    #close dvt
    fexit
}


proc check_can_cli_dump {} {

    #check number of files in dump directory 
    set fnames [glob -nocomplain -directory "../can_cli_dump" *.txt]
    set total_size 0
    foreach fn $fnames {
        incr total_size [file size $fn]
    }

    # if there is more than 100Mb, flag a warning
    if {$total_size > 100000000} {
        dvtConPuts help "\n\nWarning: There is $total_size bytes of data in the can_cli_dump directory."
        dvtConPuts help "         Use clean_can_cli_dump to delete directory contents.\n"
    }
    
    # stick comma's into number
    while {[regsub {^([-+]?\d+)(\d\d\d)} $total_size "\\1,\\2" total_size]} {}
    
    return "Bytes used: $total_size"
}


set helptext(clean_can_cli_dump) "Deletes all .txt files in the CLI/CAN dump directory"
set helptextd(clean_can_cli_dump) "
SYNOPSIS
   clean_can_cli_dump

DESCRIPTION
Deletes all .txt files in the CLI/CAN dump directory
"

proc clean_can_cli_dump {} {
    set fnames [glob -nocomplain -directory "../can_cli_dump" *.txt]
    foreach fn $fnames {
        file delete $fn
    }
    
    return "Done"
}


# 
# Displays information in a newly created window. Parameters are:
#   - wintext   - Text to display
#   - title     - Title. Defaults to window name if not specified
#   - bg        - Background colour. Defaults to light blue
#   - fg        - Text colour. Defaults to black
#   - font      - Font. Defaults to Arial, 10
#   - win       - Window name. Defaults to auto generated name
set winid 0
proc output_win {wintext {title Auto} {bg lightblue} {fg black} {font {"Arial" 10}} {win Auto}} {
    global winid
    
    if {$win == "Auto"} {
        set win ".auto_$winid"
        incr winid
    } elseif {[string range $win 0 0] != "."} {
        set win ".$win"
    }
    
    if {$title == "Auto"} {
        set title $win
    }
    
    toplevel $win
    wm title $win $title
    wm attributes $win -topmost 1

    text      $win.text -height 15 -width 60 -yscrollcommand "$win.scry set" -xscrollcommand "$win.scrx set" -wrap none -bg $bg -fg $fg -font $font
    scrollbar $win.scrx -command "$win.text xview" -orient horizontal
    scrollbar $win.scry -command "$win.text yview" -orient vertical

    grid configure $win.text  -column 0 -row 0  -sticky nsew
    grid configure $win.scrx  -column 0 -row 1  -sticky nsew
    grid configure $win.scry  -column 1 -row 0  -sticky nsew

    grid rowconfigure    $win 0 -weight 1
    grid rowconfigure    $win 1 -weight 0 
    grid columnconfigure $win 0 -weight 1
    grid columnconfigure $win 1 -weight 0

    $win.text insert end $wintext
    $win.text configure -state disabled
}


# load BLT package
package require BLT
namespace import blt::*
#namespace import -force blt::tile::*

package require tcom

# some aliases for useful commands
rename puts fputs
rename exit fexit
interp alias {} puts {} dvtConPuts
interp alias {} exit {} dvtexit
interp alias {} h {} history
interp alias {} ! {} h r
interp alias {} jobs {} after info
interp alias {} kill {} after cancel


# keep loads of commands in the history
history keep 100
   
#
# draw the window
#
source balloon.tcl
genwindow


# handle any command line arguments
set extra_source_files ""
for {set i 0} {$i < $argc} {incr i} {
    set arg [lindex $argv $i]
    
    switch $arg {
        "/s"   {incr i; lappend extra_source_files [lindex $argv $i]}
    }
}


#
# change tk_getOpenFile so that it remembers the last selected directory
#
if {[info commands orig_tk_getOpenFile] == ""} {
    rename tk_getOpenFile orig_tk_getOpenFile
}

proc tk_getOpenFile {args} {
    global dvt_last_selected_dir

    # some error checking
    if {(![info exists dvt_last_selected_dir]) || (![file isdirectory $dvt_last_selected_dir])} {
        set dvt_last_selected_dir [pwd]
    }

    # open dialog box with the last directory, unless another default directory has been set. The list
    # command on last selected dir is to handle file names with spaces.
    if {[lsearch $args -initialdir] != -1} {
        set fn [eval orig_tk_getOpenFile $args]
    } elseif {$args != ""} {
        set fn [eval orig_tk_getOpenFile $args -initialdir [list $dvt_last_selected_dir]]
    } else {
        set fn [eval orig_tk_getOpenFile -initialdir [list $dvt_last_selected_dir]]
    }
    
    if {[file exists $fn]} {
        set dvt_last_selected_dir [file dirname $fn]
    }
    
    return $fn
}



# Other initialisation
opencom 1
loaddll
source idle.tcl
source canopen.tcl
source sdo_client.tcl
source uut.tcl
source emcy.tcl
source load.tcl
source object_data.tcl

#no DAQ available
set do_daq_poll 0
set daq_available 0


# check the can_cli_dump directory is not filling up with stuff
check_can_cli_dump


# Use a default inter-character delay of 20ms on the serial port
set cli_delay 20


#Initialise CAN adapter
if {![catch {can init} err]} {
    set can_available 1
    # Initialise the CAN parts of the DLL if availble
    can start
    
    # CANbus is online
    .statusbar.label_can_state configure -text " CANbus Online" -image .tick_gif -background lightgreen
} else {
    dvtConPuts stderr $err
    canmsg "unable to intialise CAN adapter" unknown
    set can_available 0
}


# load workspace
if {[catch {source workspace.tcl} err]} {
    if {$err != "couldn't read file \"workspace.tcl\": no such file or directory"} {
        dvtConPuts stderr "Error loading workspace.tcl: $err"
    }
}


# Load personal setting and configuration
if {[catch {source personal.tcl} err]} {
    dvtConPuts stderr "personal.tcl contains errors and has not been completely sourced!!\n$err"
}


# load each additional file specified on command line
foreach fname $extra_source_files {
    if {[catch {source $fname} err]} {
        dvtConPuts stderr "Unable to source $fname. Error: $err"
    }
}


# load previous history
if {[catch {source history.tcl} err]} {
    if {$err != "couldn't read file \"history.tcl\": no such file or directory"} {
        dvtConPuts stderr "Error loading history.tcl: $err"
    }
}


#
# utility to find that damn proc you know exists 
#

set helptext(find) "searches available procs and variables for keywords"
set helptexta(find) "f"
set helptextd(find) "
SYNOPSIS
    find pattern ?detail?

DESCRIPTION
Allows you to search for a proc or variable you know exists. Set detail to 
1 to display proc contents as well

eg i want to set up some pdos but i can only remember \"config\"
type \"find config\" will list all procs with config in the name
"

interp alias {} f {} find
proc find {text {detail 0}} {
    
    puts "Searching loaded procedure names..." 
    foreach t [lsearch -all [info proc] *${text}*] {
         puts "[lindex [info proc] $t]" 
         if {$detail} {
            puts [info body [lindex [info proc] $t]]
         }
     }
     
    puts "\n\nSearching gloabal variables..." 
    set cmd "foreach t \[lsearch -all \[info var\] *${text}*\] \{
            
            puts \"searching for ${text} found \[lindex \[info var\] \$t\]\"
    \}"
    uplevel #0 $cmd 
}


# Output a welcome message in the console window
set toolver [total_revision DVT]
dvtConPuts "$toolname [string map {$ ""} $toolver]"
dvtConPuts ""
dvtConPuts "Type \"help\" for help"


# give console the focus
focus $con

# Start polling for CAN messages
idlepoll

#start processing window updates at 300mS
daqidlepoll

#maintain can and cli windows
win_size_poll

# Generate the first command prompt
dvtConsolePrompt
