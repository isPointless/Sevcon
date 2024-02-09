#
#  Add the string append function
#
if {[catch {string append}]} then {
    rename string STRING_ORIGINAL
    proc string {cmd args} {
	switch -regexp -- $cmd {
	    ^a(p(p(e(n(d)?)?)?)?)?$ {
		uplevel [list join $args {}]
	    }
	    default {
		if {[catch {
		    set result [uplevel [list STRING_ORIGINAL $cmd] $args]
		} err]} then {
		    return -code error\
			[STRING_ORIGINAL map\
			     [list\
				  STRING_ORIGINAL string\
				  ": must be bytelength,"\
				  ": must be append, bytelength,"]\
			     $err]
		} else {
		    set result
		}
	    }
	}
    }
}


#
# debug output
#
proc veh_if_cfg_debug_puts {str} {
    global veh_if_cfg_debug
    
    # output data if debug flag exists and is set
    if {[info exists veh_if_cfg_debug]} {
        if {$veh_if_cfg_debug} {
            dvtConPuts debug $str
        }
    }
}


set colours [ list  gray {light grey}                                               \
    navy {cornflower blue} {dark slate blue} {slate blue}                           \
    {medium slate blue} {light slate blue} {medium blue} {royal blue}               \
    blue {dodger blue} {deep sky blue} {sky blue} {light sky blue}                  \
    {steel blue} {light steel blue} {light blue} {powder blue}                      \
    {pale turquoise} {dark turquoise} {medium turquoise} turquoise                  \
    cyan {light cyan} {cadet blue} {medium aquamarine} aquamarine                   \
    {dark green} {dark olive green} {dark sea green} {sea green}                    \
    {medium sea green} {light sea green} {pale green} {spring green}                \
    {lawn green} green chartreuse {medium spring green} {green yellow}              \
    {lime green} {yellow green} {forest green} {olive drab} {dark khaki}            \
    khaki {pale goldenrod} {light goldenrod yellow} {light yellow} yellow           \
    gold {light goldenrod} goldenrod {dark goldenrod} {rosy brown}                  \
    {indian red} {saddle brown} sienna peru burlywood beige wheat                   \
    {sandy brown} tan chocolate firebrick brown {dark salmon} salmon                \
    {light salmon} orange {dark orange} coral {light coral} tomato                  \
    {orange red} red {hot pink} {deep pink} pink {light pink}                       \
    {pale violet red} maroon {medium violet red} {violet red}                       \
    magenta violet plum orchid {medium orchid} {dark orchid} {dark violet}          \
    {blue violet} purple {medium purple} thistle snow2 ]  
    
# Repeat this operation for the following nodes:
foreach node_id [ nodes_detected ] {
    
    veh_if_cfg_debug_puts "Checking node $node_id"
    
    if { [ get_operational_state $node_id ] != "off-bus" } {
        
        veh_if_cfg_debug_puts "Node found - checking maps"

        veh_if_add_node $node_id [ af [ expr $node_id + 0x80 ] 16.0 ] 1
                
        # Look at each object in the TPDO mappings
        set index 0x1a00
        set tpdos_finished 0
        
        while { $tpdos_finished == 0 } {
                        
            # Get the first mapping
            set num_maps [ sdo_rnx $node_id $index 0 ]
            
            # Check the mapping is valid
            if { [string range $num_maps 0 4] != "Abort" } {
                                
                veh_if_cfg_debug_puts "Checking TPDOs from index $index"
                
                # Get the COB ID
                set cob_id [ sdo_rnx $node_id [ expr $index - 0x200 ] 1 ]
                
                # only import enabled TPDOs
                if {($num_maps > 0) && (($cob_id & 0x80000000) == 0)} {
                    
                    veh_if_cfg_debug_puts "Importing $num_maps maps"
                    
                    # convert COBID to 16-bit number
                    set cob_id [ af [ expr $cob_id ] 17.0 ]
                    
                    # Read in mappings
                    set bit_position 0
                    
                    for { set i 1 } { $i <= $num_maps } { incr i } {
                        
                        # Chop out the relevant parts of the mapping entries
                        set map_raw   [ sdo_rnx $node_id $index $i ]
                        set map_index [ string append "0x" [ string range $map_raw 2 5 ] ]
                        set map_sub   [ string append "0x" [ string range $map_raw 6 7 ] ]
                        set map_len   [ string append "0x" [ string range $map_raw 8 9 ] ]
                        
                        set obj_data  [ get_object_data $map_index $map_sub ]
                        
                        set name      [ string append $node_id ", " [ lindex $obj_data 0 ] ]
                        
                        veh_if_cfg_debug_puts "Map $i - source $map_index, $map_sub - $map_len bits, $name"

                        set col [ lindex $colours [ expr int( rand() * [llength $colours] ) ] ]
                                                
                        if { [ lindex $obj_data 2 ] } {
                            veh_if_add_watch    $name        -100000.0  100000.0  [ lindex $obj_data 1 ]  		" pdo $cob_id [ expr $bit_position ] [ expr $map_len ] int 1 {$col}  "
                        }
                        
                        veh_if_cfg_debug_puts "Bit position $bit_position Map len $map_len"
                        set bit_position [ expr $bit_position + $map_len ]
                    }
                    
                }
                
                # Move on to next map
                set index [ af [ expr $index + 1 ] 17.0 ]
                
            } else {
                
                # No more maps on this node
                set tpdos_finished 1
                veh_if_cfg_debug_puts "Can't find any more TPDOs"
            }
            
        }
                
    } else {
        
        veh_if_cfg_debug_puts "Node not detected"
        
    }
    
}


