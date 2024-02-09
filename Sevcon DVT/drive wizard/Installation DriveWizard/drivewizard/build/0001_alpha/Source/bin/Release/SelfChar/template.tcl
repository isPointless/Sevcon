#
# Proc to load motor data into controller
#

proc load_scwiz_settings { node_id } {

	# electrical characteristics
		
	puts "Writing 0x6090 1 " ; sdo_wnx $node_id 0x6090 1  [ af <!pulses_per_rev>       		32.0  ] ;# Pulses per rev
	puts "Writing 0x6090 2 " ; sdo_wnx $node_id 0x6090 2  [ af 1                       		32.0  ] ;# Pulse factor
	puts "Writing 0x6076 0 " ; sdo_wnx $node_id 0x6076 0  [ af <!motor_rated_torque>   		32.0  ] ;# Motor rated torque (mNm)
	puts "Writing 0x6410 2 " ; sdo_wnx $node_id 0x6410 2  [ af <!max_stator_current> 		16.0  ] ;# Max stator current
	puts "Writing 0x6410 3 " ; sdo_wnx $node_id 0x6410 3  [ af <!min_magnetising_current> 	8.8   ] ;# Min magnetising current
	puts "Writing 0x6410 4 " ; sdo_wnx $node_id 0x6410 4  [ af <!max_magnetising_current> 	10.6  ] ;# Max magnetising current
	puts "Writing 0x6410 5 " ; sdo_wnx $node_id 0x6410 5  [ af <!pole_pairs> 				16.0  ] ;# Pole pairs
	puts "Writing 0x6410 6 " ; sdo_wnx $node_id 0x6410 6  [ af <!stator_resistance> 		4.12  ] ;# Stator resistance
	puts "Writing 0x6410 7 " ; sdo_wnx $node_id 0x6410 7  [ af <!rated_stator_current> 		16.0  ] ;# Rated stator current
	puts "Writing 0x6410 8 " ; sdo_wnx $node_id 0x6410 8  [ af <!rotor_resistance> 			4.12  ] ;# Rotor resistance
	puts "Writing 0x6410 9 " ; sdo_wnx $node_id 0x6410 9  [ af <!magnetising_inductance> 	0.16  ] ;# Magnetising inductance
	puts "Writing 0x6410 10" ; sdo_wnx $node_id 0x6410 10 [ af <!stator_leakage_inductance> -4.20 ] ;# Stator leakage inductance
	puts "Writing 0x6410 11" ; sdo_wnx $node_id 0x6410 11 [ af <!rotor_leakage_inductance> 	-4.20 ] ;# Rotor leakage inductance
	puts "Writing 0x6410 12" ; sdo_wnx $node_id 0x6410 12 [ af <!nominal_battery_voltage> 	8.8   ] ;# Nominal battery voltage
	puts "Writing 0x6410 13" ; sdo_wnx $node_id 0x6410 13 [ af <!current_control_prop_gain> 1.15  ] ;# Current control prop gain
	puts "Writing 0x6410 14" ; sdo_wnx $node_id 0x6410 14 [ af <!torque_control_alpha> 		1.15  ] ;# Torque control alpha
	puts "Writing 0x6410 15" ; sdo_wnx $node_id 0x6410 15 [ af <!current_control_int_gain> 	1.15  ] ;# Current control int gain
	puts "Writing 0x6410 16" ; sdo_wnx $node_id 0x6410 16 [ af <!reactive_gain_boost> 		0.16  ] ;# Reactive gain boost
	puts "Writing 0x6410 17" ; sdo_wnx $node_id 0x6410 17 [ af <!rpm_limiter_gain> 			16.0  ] ;# RPM limiter gain
	
	# Flux map
	
	puts "Writing 0x4610 1 " ; sdo_wnx $node_id 0x4610 1  [ af <!flux_map_td1>		12.4  ] ;# Torque point
	puts "Writing 0x4610 2 " ; sdo_wnx $node_id 0x4610 2  [ af <!flux_map_im1>		10.6  ] ;# Mag current point
	puts "Writing 0x4610 3 " ; sdo_wnx $node_id 0x4610 3  [ af <!flux_map_td2>		12.4  ] ;# Torque point
	puts "Writing 0x4610 4 " ; sdo_wnx $node_id 0x4610 4  [ af <!flux_map_im2>		10.6  ] ;# Mag current point
	puts "Writing 0x4610 5 " ; sdo_wnx $node_id 0x4610 5  [ af <!flux_map_td3>		12.4  ] ;# Torque point
	puts "Writing 0x4610 6 " ; sdo_wnx $node_id 0x4610 6  [ af <!flux_map_im3>		10.6  ] ;# Mag current point
	puts "Writing 0x4610 7 " ; sdo_wnx $node_id 0x4610 7  [ af <!flux_map_td4>		12.4  ] ;# Torque point
	puts "Writing 0x4610 8 " ; sdo_wnx $node_id 0x4610 8  [ af <!flux_map_im4>		10.6  ] ;# Mag current point
	puts "Writing 0x4610 9 " ; sdo_wnx $node_id 0x4610 9  [ af <!flux_map_td5>		12.4  ] ;# Torque point
	puts "Writing 0x4610 10" ; sdo_wnx $node_id 0x4610 10 [ af <!flux_map_im5>		10.6  ] ;# Mag current point
	puts "Writing 0x4610 11" ; sdo_wnx $node_id 0x4610 11 [ af <!flux_map_td6>		12.4  ] ;# Torque point
	puts "Writing 0x4610 12" ; sdo_wnx $node_id 0x4610 12 [ af <!flux_map_im6>		10.6  ] ;# Mag current point
	puts "Writing 0x4610 13" ; sdo_wnx $node_id 0x4610 13 [ af <!flux_map_td7>		12.4  ] ;# Torque point
	puts "Writing 0x4610 14" ; sdo_wnx $node_id 0x4610 14 [ af <!flux_map_im7>		10.6  ] ;# Mag current point
	puts "Writing 0x4610 15" ; sdo_wnx $node_id 0x4610 15 [ af <!flux_map_td8>		12.4  ] ;# Torque point
	puts "Writing 0x4610 16" ; sdo_wnx $node_id 0x4610 16 [ af <!flux_map_im8>		10.6  ] ;# Mag current point
	puts "Writing 0x4610 17" ; sdo_wnx $node_id 0x4610 17 [ af <!flux_map_td9>		12.4  ] ;# Torque point
	puts "Writing 0x4610 18" ; sdo_wnx $node_id 0x4610 18 [ af <!flux_map_im9>		10.6  ] ;# Mag current point
	
	
	# Power limit map

	puts "Writing 0x4611 1 " ; sdo_wnx $node_id 0x4611 1  [ af <!p_limit_map_tmax1>  12.4 ] ;# Torque point	
	puts "Writing 0x4611 2 " ; sdo_wnx $node_id 0x4611 2  [ af <!p_limit_map_rpm1>   16.0 ] ;# Speed point
	puts "Writing 0x4611 3 " ; sdo_wnx $node_id 0x4611 3  [ af <!p_limit_map_tmax2>  12.4 ] ;# Torque point	
	puts "Writing 0x4611 4 " ; sdo_wnx $node_id 0x4611 4  [ af <!p_limit_map_rpm2>   16.0 ] ;# Speed point
	puts "Writing 0x4611 5 " ; sdo_wnx $node_id 0x4611 5  [ af <!p_limit_map_tmax3>  12.4 ] ;# Torque point	
	puts "Writing 0x4611 6 " ; sdo_wnx $node_id 0x4611 6  [ af <!p_limit_map_rpm3>   16.0 ] ;# Speed point
	puts "Writing 0x4611 7 " ; sdo_wnx $node_id 0x4611 7  [ af <!p_limit_map_tmax4>  12.4 ] ;# Torque point	
	puts "Writing 0x4611 8 " ; sdo_wnx $node_id 0x4611 8  [ af <!p_limit_map_rpm4>   16.0 ] ;# Speed point
	puts "Writing 0x4611 9 " ; sdo_wnx $node_id 0x4611 9  [ af <!p_limit_map_tmax5>  12.4 ] ;# Torque point	
	puts "Writing 0x4611 10" ; sdo_wnx $node_id 0x4611 10 [ af <!p_limit_map_rpm5>   16.0 ] ;# Speed point
	puts "Writing 0x4611 11" ; sdo_wnx $node_id 0x4611 11 [ af <!p_limit_map_tmax6>  12.4 ] ;# Torque point	
	puts "Writing 0x4611 12" ; sdo_wnx $node_id 0x4611 12 [ af <!p_limit_map_rpm6>   16.0 ] ;# Speed point
	puts "Writing 0x4611 13" ; sdo_wnx $node_id 0x4611 13 [ af <!p_limit_map_tmax7>  12.4 ] ;# Torque point	
	puts "Writing 0x4611 14" ; sdo_wnx $node_id 0x4611 14 [ af <!p_limit_map_rpm7>   16.0 ] ;# Speed point
	puts "Writing 0x4611 15" ; sdo_wnx $node_id 0x4611 15 [ af <!p_limit_map_tmax8>  12.4 ] ;# Torque point	
	puts "Writing 0x4611 16" ; sdo_wnx $node_id 0x4611 16 [ af <!p_limit_map_rpm8>   16.0 ] ;# Speed point
	puts "Writing 0x4611 17" ; sdo_wnx $node_id 0x4611 17 [ af <!p_limit_map_tmax9>  12.4 ] ;# Torque point	
	puts "Writing 0x4611 18" ; sdo_wnx $node_id 0x4611 18 [ af <!p_limit_map_rpm9>   16.0 ] ;# Speed point
	
	
	#Torque cutback map
	
	puts "Writing 0x4612 1 " ; sdo_wnx $node_id 0x4612 1  [ af <!torque_cut_map_v1>  10.6 ] ;# Voltage point
	puts "Writing 0x4612 2 " ; sdo_wnx $node_id 0x4612 2  [ af <!torque_cut_map_g1>  1.15 ] ;# Torque gain
	puts "Writing 0x4612 3 " ; sdo_wnx $node_id 0x4612 3  [ af <!torque_cut_map_v2>  10.6 ] ;# Voltage point
	puts "Writing 0x4612 4 " ; sdo_wnx $node_id 0x4612 4  [ af <!torque_cut_map_g2>  1.15 ] ;# Torque gain
	puts "Writing 0x4612 5 " ; sdo_wnx $node_id 0x4612 5  [ af <!torque_cut_map_v3>  10.6 ] ;# Voltage point
	puts "Writing 0x4612 6 " ; sdo_wnx $node_id 0x4612 6  [ af <!torque_cut_map_g3>  1.15 ] ;# Torque gain
	puts "Writing 0x4612 7 " ; sdo_wnx $node_id 0x4612 7  [ af <!torque_cut_map_v4>  10.6 ] ;# Voltage point
	puts "Writing 0x4612 8 " ; sdo_wnx $node_id 0x4612 8  [ af <!torque_cut_map_g4>  1.15 ] ;# Torque gain
	puts "Writing 0x4612 9 " ; sdo_wnx $node_id 0x4612 9  [ af <!torque_cut_map_v5>  10.6 ] ;# Voltage point
	puts "Writing 0x4612 10" ; sdo_wnx $node_id 0x4612 10 [ af <!torque_cut_map_g5>  1.15 ] ;# Torque gain
	puts "Writing 0x4612 11" ; sdo_wnx $node_id 0x4612 11 [ af <!torque_cut_map_v6>  10.6 ] ;# Voltage point
	puts "Writing 0x4612 12" ; sdo_wnx $node_id 0x4612 12 [ af <!torque_cut_map_g6>  1.15 ] ;# Torque gain
	puts "Writing 0x4612 13" ; sdo_wnx $node_id 0x4612 13 [ af <!torque_cut_map_v7>  10.6 ] ;# Voltage point
	puts "Writing 0x4612 14" ; sdo_wnx $node_id 0x4612 14 [ af <!torque_cut_map_g7>  1.15 ] ;# Torque gain
	puts "Writing 0x4612 15" ; sdo_wnx $node_id 0x4612 15 [ af <!torque_cut_map_v8>  10.6 ] ;# Voltage point
	puts "Writing 0x4612 16" ; sdo_wnx $node_id 0x4612 16 [ af <!torque_cut_map_g8>  1.15 ] ;# Torque gain
	puts "Writing 0x4612 17" ; sdo_wnx $node_id 0x4612 17 [ af <!torque_cut_map_v9>  10.6 ] ;# Voltage point
	puts "Writing 0x4612 18" ; sdo_wnx $node_id 0x4612 18 [ af <!torque_cut_map_g9>  1.15 ] ;# Torque gain
	
	
	# Field weakening
	
	puts "Writing 0x4613 1 " ; sdo_wnx $node_id 0x4613 1  [ af <!fw_map_g1>      6.10 ] ;# Gain point
	puts "Writing 0x4613 2 " ; sdo_wnx $node_id 0x4613 2  [ af <!fw_map_rpm1>    16.0 ] ;# Speed point
	puts "Writing 0x4613 3 " ; sdo_wnx $node_id 0x4613 3  [ af <!fw_map_g2>      6.10 ] ;# Gain point
	puts "Writing 0x4613 4 " ; sdo_wnx $node_id 0x4613 4  [ af <!fw_map_rpm2>    16.0 ] ;# Speed point
	puts "Writing 0x4613 5 " ; sdo_wnx $node_id 0x4613 5  [ af <!fw_map_g3>      6.10 ] ;# Gain point
	puts "Writing 0x4613 6 " ; sdo_wnx $node_id 0x4613 6  [ af <!fw_map_rpm3>    16.0 ] ;# Speed point
	puts "Writing 0x4613 7 " ; sdo_wnx $node_id 0x4613 7  [ af <!fw_map_g4>      6.10 ] ;# Gain point
	puts "Writing 0x4613 8 " ; sdo_wnx $node_id 0x4613 8  [ af <!fw_map_rpm4>    16.0 ] ;# Speed point
	puts "Writing 0x4613 9 " ; sdo_wnx $node_id 0x4613 9  [ af <!fw_map_g5>      6.10 ] ;# Gain point
	puts "Writing 0x4613 10" ; sdo_wnx $node_id 0x4613 10 [ af <!fw_map_rpm5>    16.0 ] ;# Speed point
	puts "Writing 0x4613 11" ; sdo_wnx $node_id 0x4613 11 [ af <!fw_map_g6>      6.10 ] ;# Gain point
	puts "Writing 0x4613 12" ; sdo_wnx $node_id 0x4613 12 [ af <!fw_map_rpm6>    16.0 ] ;# Speed point
	puts "Writing 0x4613 13" ; sdo_wnx $node_id 0x4613 13 [ af <!fw_map_g7>      6.10 ] ;# Gain point
	puts "Writing 0x4613 14" ; sdo_wnx $node_id 0x4613 14 [ af <!fw_map_rpm7>    16.0 ] ;# Speed point
	puts "Writing 0x4613 15" ; sdo_wnx $node_id 0x4613 15 [ af <!fw_map_g8>      6.10 ] ;# Gain point
	puts "Writing 0x4613 16" ; sdo_wnx $node_id 0x4613 16 [ af <!fw_map_rpm8>    16.0 ] ;# Speed point
	puts "Writing 0x4613 17" ; sdo_wnx $node_id 0x4613 17 [ af <!fw_map_g9>      6.10 ] ;# Gain point
	puts "Writing 0x4613 18" ; sdo_wnx $node_id 0x4613 18 [ af <!fw_map_rpm9>    16.0 ] ;# Speed point

}

puts "Self char data generated on <!date> at <!time>"
puts "File sourced. Type load_scwiz_settings (node_id) to load data."

#
