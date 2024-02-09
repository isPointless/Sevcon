# maximise the window
wm state . zoomed

# initialise the protocol cobids
co_map_cobid_to_protocol pdo  0x0181
co_map_cobid_to_protocol pdo  0x0281
co_map_cobid_to_protocol pdo  0x0381
co_map_cobid_to_protocol pdo  0x0481
co_map_cobid_to_protocol pdo  0x0182
co_map_cobid_to_protocol pdo  0x0282
co_map_cobid_to_protocol pdo  0x0382
co_map_cobid_to_protocol pdo  0x0482
co_map_cobid_to_protocol pdo  0x0483
co_map_cobid_to_protocol nmt  0x0701
co_map_cobid_to_protocol nmt  0x0702
co_map_cobid_to_protocol nmt  0x0703
co_map_cobid_to_protocol nmt  0x0704
co_map_cobid_to_protocol sync 0x0080
co_map_cobid_to_protocol emcy 0x0081
co_map_cobid_to_protocol emcy 0x0082


# display any received emcy messages in Info window

# show the information window and added fault colour tags
show_info_window true
add_infomsg_colour_tag fault_set_1     red
add_infomsg_colour_tag fault_cleared_1 green
add_infomsg_colour_tag fault_set_2     magenta
add_infomsg_colour_tag fault_cleared_2 blue

# create fault message array
set personal_faults(0x4541) "FAN fault"
set personal_faults(0x45C1) "BDI Warning"
set personal_faults(0x45C2) "BDI Cutout"
set personal_faults(0x45C3) "Low battery"
set personal_faults(0x45C4) "High battery"
set personal_faults(0x45C5) "High capacitor"
set personal_faults(0x45C6) "Vbat below rated min"
set personal_faults(0x45C7) "Vbat above rated max"
set personal_faults(0x45C8) "Vcap above rated max"
set personal_faults(0x4601) "Device too cold"
set personal_faults(0x4602) "Device too hot"
set personal_faults(0x4603) "Motor in thermal c/b"
set personal_faults(0x4681) "Preop"
set personal_faults(0x4781) "Anon EMCY Warning"
set personal_faults(0x47C1) "Service Required"
set personal_faults(0x4881) "Seat"
set personal_faults(0x4882) "Two Dir Fault"
set personal_faults(0x4883) "SRO Fault"
set personal_faults(0x4884) "Sequence Fault"
set personal_faults(0x4885) "FS1 Recycle Fault"
set personal_faults(0x4941) "Low oil"
set personal_faults(0x4981) "Throttle Fault"
set personal_faults(0x4B81) "Anon EMCY Drv Inhibit"
set personal_faults(0x4C41) "Too Many Slaves"
set personal_faults(0x4F01) "Bad State"
set personal_faults(0x4F02) "EMCY Fail"
set personal_faults(0x4F41) "Internal Fault"
set personal_faults(0x4F42) "Out of Memory"
set personal_faults(0x4F43) "General DSP Error"
set personal_faults(0x4F44) "Timer Error"
set personal_faults(0x4F45) "Queue Error"
set personal_faults(0x4F46) "Sched Error"
set personal_faults(0x4F47) "DSP Heartbeat"
set personal_faults(0x4F48) "IO SS Error"
set personal_faults(0x4F49) "GIO SS Error"
set personal_faults(0x4F4A) "LCM SS Error"
set personal_faults(0x4F4B) "LCP SS Error"
set personal_faults(0x4F4C) "OBD SS Error"
set personal_faults(0x4F4D) "VEHAPP SS Error"
set personal_faults(0x4F4E) "DMC SS Error"
set personal_faults(0x4F4F) "TracApp SS Error"
set personal_faults(0x4F50) "New PF Detected"
set personal_faults(0x4F51) "DSP not detected"
set personal_faults(0x4F52) "DSP Comms Error"
set personal_faults(0x4F53) "App Mgr SS Error"
set personal_faults(0x4F54) "Autozero range"
set personal_faults(0x4F55) "DSP param"
set personal_faults(0x4F81) "Anon EMCY Severe"
set personal_faults(0x5041) "Bad NVM Data"
set personal_faults(0x5042) "VPDO out of range"
set personal_faults(0x5043) "Param fixed range error"
set personal_faults(0x5044) "Param dyn range error"
set personal_faults(0x5081) "Invalid Steer Switches"
set personal_faults(0x5101) "Line Contactor o/c"
set personal_faults(0x5102) "Line Contactor welded"
set personal_faults(0x5181) "Dig In Wire Off"
set personal_faults(0x5182) "Alg In Wire Off"
set personal_faults(0x5183) "Alg Out Over I"
set personal_faults(0x5184) "Alg Out On with No FS"
set personal_faults(0x5185) "Alg Out Off with FS"
set personal_faults(0x51C1) "Power Supply Interrupt"
set personal_faults(0x51C2) "Precharge fail"
set personal_faults(0x52C1) "Encoder fault"
set personal_faults(0x52C2) "Motor overcurrent fault"
set personal_faults(0x5301) "CANbus Fault"
set personal_faults(0x5302) "No Bootup"
set personal_faults(0x5303) "LPRX CAN"
set personal_faults(0x5304) "LPTX CAN"
set personal_faults(0x5305) "HPRX CAN"
set personal_faults(0x5306) "HPTX CAN"
set personal_faults(0x5307) "CAN Overrun"
set personal_faults(0x5308) "CAN Off"
set personal_faults(0x5309) "Nodeguard Error"
set personal_faults(0x530A) "PDO Short"
set personal_faults(0x530B) "HBeat Error"
set personal_faults(0x530C) "CANopen Device State"
set personal_faults(0x530D) "CAN Error State"
set personal_faults(0x530E) "SDO Handle Error"
set personal_faults(0x530F) "SDO Timeout"
set personal_faults(0x5310) "SDO Abort"
set personal_faults(0x5311) "SDO State Error"
set personal_faults(0x5312) "SDO Toggle Error"
set personal_faults(0x5313) "SDO Rx Error"
set personal_faults(0x5314) "SDO Length Error"
set personal_faults(0x5315) "SDO Tx Error"
set personal_faults(0x5316) "CANopen Ev Unknown"
set personal_faults(0x5317) "SDO Bad Source"
set personal_faults(0x5318) "SDO Bad Error No"
set personal_faults(0x5319) "Mtr Slv in Wrong State"
set personal_faults(0x5341) "Wrong DSP protocol"
set personal_faults(0x5342) "Osc WDog Tripped"
set personal_faults(0x5343) "Flt o/flow"
set personal_faults(0x5344) "DSP SPI"
set personal_faults(0x5381) "Anon EMCY V Severe"
set personal_faults(0x54C1) "DSP Overvoltage"
set personal_faults(0x54C2) "DSP PF Fault"
set personal_faults(0x54C3) "Mosfet s/c M1 Top"
set personal_faults(0x54C4) "Mosfet s/c M1 Bottom"
set personal_faults(0x54C5) "Mosfet s/c M2 Top"
set personal_faults(0x54C6) "Mosfet s/c M2 Bottom"
set personal_faults(0x54C7) "Mosfet s/c M3 Top"
set personal_faults(0x54C8) "Mosfet s/c M3 Bottom"
set personal_faults(0x54C9) "Mosfet tests incomplete"
set personal_faults(0x5741) "Invalid Rating"
set personal_faults(0x5781) "Anon EMCY RTB"

# also initialises vehicle interface array
array set veh_if_fault_desc [array get personal_faults]

# emcy message handler
proc personal_emcy_handler {args} {
    global personal_faults
    
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
        if {[info exists personal_faults($fault_id)]} {
            set fault_desc $personal_faults($fault_id)
        } else {
            set fault_desc "<Unknown>"
        }
        
        # calculate data/time stamp
        set timestamp [clock format [clock seconds] -format {%H:%M:%S, %d/%m/%y}]
        
        # if the error code is 0x0000, then this indicates the fault has reset.
        if {$error_code != 0x0000} {
            infomsg "Node $nodeid fault ($fault_id, $fault_desc) set at $timestamp. Data ($db1 $db2 $db3). CANopen Error Code: $error_code" fault_set_[set nodeid]
        } else {
            infomsg "Node $nodeid fault ($fault_id, $fault_desc) cleared at $timestamp. Data ($db1 $db2 $db3). CANopen Error Code: $error_code" fault_cleared_[set nodeid]
        }
    }
}

# replace cobid handler for EMCY message
co_add_cobid_handler 0x0081 personal_emcy_handler
co_add_cobid_handler 0x0082 personal_emcy_handler


#
# customer passwords. Change as necessary for individual customers
#
set customer_passwords(1)   0xc8eb
set customer_passwords(2)   0xb607
set customer_passwords(3)   0x6b54
set customer_passwords(4)   0x4bdf

# set can baud to newbaud
set newbaud 100
set canbaud($newbaud) 1
set_baud
dvtConPuts help "CANbus baud rate forced to $newbaud kBaud"

co_map_cobid_to_protocol emcy 0x0081 ;      # EMCYs for node 1
co_map_cobid_to_protocol emcy 0x0082 ;      # EMCYs for node 2
co_map_cobid_to_protocol emcy 0x0083 ;      # EMCYs for node 3
