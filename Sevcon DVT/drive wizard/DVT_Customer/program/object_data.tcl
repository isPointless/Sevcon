###############################################################################
# (C) COPYRIGHT Sevcon 2004
# 
# CCL Project Reference C6944 - Magpie
# 
# FILE
#     $Revision:1.22$
#     $Author:ps$
#     $ProjectName:DVT$ 
# 
# ORIGINAL AUTHOR
#     Chris Hauxwell
# 
# DESCRIPTION
#     Contains general information about various objects
# 
# REFERENCES    
# 
# MODIFICATION HISTORY
#     $Log:  100639: object_data.tcl 
# 
#     Rev 1.22    29/10/2008 12:13:46  ps
#  Added electrical frequency and junction power estimation debug values
# 
#     Rev 1.21    09/10/2008 17:04:28  ceh
#  Added variable type column to table so that formatted reads can determine if
#  a variable is signed and format it appropriately. Added new objects (0x2910,
#  0x2920..25, 0x2930) to table.
# 
#     Rev 1.20    25/09/2008 19:26:26  ps
#  Corrected scaling for slip
# 
#     Rev 1.19    13/08/2008 11:54:46  cmp
#  changed i2t fact and lim to cap and track temp
# 
#     Rev 1.18    23/06/2008 12:20:32  ceh
#  Added handbrake switch
# 
#     Rev 1.17    10/06/2008 10:17:08  ceh
#  Added signal voltage objects.
# 
#     Rev 1.16    28/05/2008 11:05:48  ceh
#  Added steering object, 0x2913
# 
#     Rev 1.15    20/05/2008 12:21:00  ps
#  Added torque demand and speed limit objects from motor profile 0.
# 
#     Rev 1.14    19/05/2008 14:26:52  ceh
#  Added missing items from 0x4600 and 0x4700 objects. Corrected format of 8-bit
#  Vbat and Vcap (to 0.25V/bit).
# 
#     Rev 1.13    15/05/2008 13:36:52  ps
#  Added 0x6040 and 0x60FF - normally process inputs and therefore not supposed
#  to be mapped to TPDOs, but added to this list as useful for debugging.
# 
#     Rev 1.12    23/04/2008 15:11:06  ceh
#  Added motor object (0x2020/21/40/60) inputs (statusword, actual velocity and
#  torque) so they can also be output via TPDOs for test.
# 
#     Rev 1.11    14/04/2008 14:32:58  ceh
#  Added 2nd line contactor (0x2407) and analogue output (0x6C11)
# 
#     Rev 1.10    11/02/2008 10:29:44  ceh
#  Added 0x5100,8 - 12V supply in 8.8 format
# 
#     Rev 1.9    08/01/2008 12:23:52  ceh
#  Added new objects for DC Pump and 8-bit Vbat+Vcap
# 
#     Rev 1.8    15/11/2007 13:52:02  ceh
#  Added extra objects for DC pump (Nano) and 2nd AC motor controller on Diablo.
# 
#     Rev 1.7    17/10/2007 09:37:00  cmp
#  added extra motor (aichi) thermister measurement
# 
#     Rev 1.6    20/09/2007 15:18:22  ceh
#  Added extra objects for Aichi.
# 
#     Rev 1.5    19/09/2007 14:14:46  ps
#  Added extra debug for I2t functions
# 
#     Rev 1.4    29/08/2007 08:41:56  ceh
#  Added motor debug objects (0x4602, 0x4702 and 0x4782)
# 
#     Rev 1.3    17/08/2007 11:32:10  ceh
#  Added global variables to show/hide Id/Iqs (+slip, mod index, etc) and
#  control/statuswords. Also, added motor data objects (0x6410, 0x4641, etc) and
#  motor lookup tables.
# 
#     Rev 1.2    13/06/2007 17:46:10  ceh
#  Added slope angle
# 
#     Rev 1.1    28/02/2007 12:25:22  ceh
#  Added registration
#
###############################################################################

# register module
register DVT [info script] {$Revision:1.22$}


#
#  Proc to convert an index and sub-index to a meaningful
#  name
#
proc get_object_data { index subindex } {
    global log_idqs log_cw_sw
    
    set index    [format "0x%04x" $index   ]
    set subindex [format "0x%02x" $subindex]
    set c "$index $subindex"
    
    # set to 1 to show Id/qs and controlwords+statuswords
    if {![info exists log_idqs]} {
        set log_idqs 1
    }
    if {![info exists log_cw_sw]} {
        set log_cw_sw 1
    }

    #
    #  Fields are:
    #    Index  Sub                      Name,                                           scaling,                       include      units,    type,    enum
    #                                                                                                                   in_graph
    switch $c {
        "0x6040 0x00" { return [ list   "Mtr1 cw"                                       "[ expr 1.0/1.0  ]"            "$log_cw_sw"  ""        "U16"    ""] }     
        "0x6041 0x00" { return [ list   "Mtr1 sw"                                       "[ expr 1.0/1.0  ]"            "$log_cw_sw"  ""        "U16"    ""] }
        "0x7841 0x00" { return [ list   "Mtr3 sw"                                       "[ expr 1.0/1.0  ]"            "$log_cw_sw"  ""        "U16"    ""] }
        "0x4600 0x01" { return [ list   "Mtr1 Slip (rad/s)"                             "[ expr 1.0/256.0 ]"           "$log_idqs"   "rads/s"  "I16"    ""] }
        "0x4600 0x03" { return [ list   "Mtr1 Temp PTC 1"                               "[ expr 1.0/1.0 ]"             "$log_idqs"   "C"       "I16"    ""] }
        "0x4600 0x04" { return [ list   "Mtr1 Temp Sw"                                  "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     "Off On"] }
        "0x4600 0x05" { return [ list   "Mtr1 Id_ref"                                   "[ expr 1.0/16.0 ]"            "$log_idqs"   "A"       "I16"    ""] }
        "0x4600 0x06" { return [ list   "Mtr1 Iq_ref"                                   "[ expr 1.0/16.0 ]"            "$log_idqs"   "A"       "I16"    ""] }
        "0x4600 0x07" { return [ list   "Mtr1 Id"                                       "[ expr 1.0/16.0 ]"            "$log_idqs"   "A"       "I16"    ""] }
        "0x4600 0x08" { return [ list   "Mtr1 Iq"                                       "[ expr 1.0/16.0 ]"            "$log_idqs"   "A"       "I16"    ""] }
        "0x4600 0x09" { return [ list   "Mtr1 Ud"                                       "[ expr 1.0/256.0 ]"           "$log_idqs"   "V"       "I16"    ""] }
        "0x4600 0x0a" { return [ list   "Mtr1 Uq"                                       "[ expr 1.0/256.0 ]"           "$log_idqs"   "V"       "I16"    ""] }
        "0x4600 0x0b" { return [ list   "Mtr1 Mod Index"                                "[ expr 100.0/255.0 ]"         "$log_idqs"   "%"       "I16"    ""] }
        "0x4600 0x0c" { return [ list   "Mtr1 I(RMS)"                                   "[ expr 1.0/1.0 ]"             "$log_idqs"   "A"       "I16"    ""] }
        "0x4600 0x0d" { return [ list   "Mtr1 V(RMS)"                                   "[ expr 1.0/256.0 ]"           "$log_idqs"   "V"       "I16"    ""] }
        "0x4600 0x0e" { return [ list   "Mtr1 w2"                                       "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x4600 0x0f" { return [ list   "Mtr1 w_elec"                                   "[ expr 1.0/16.0 ]"            "1"           "rads/s"  "I16"    ""] }
        "0x4700 0x01" { return [ list   "Mtr2 Slip (rad/s)"                             "[ expr 1.0/256.0 ]"           "$log_idqs"   "rads/s"  "I16"    ""] }
        "0x4700 0x04" { return [ list   "Mtr2 Temp Sw"                                  "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     "Off On"] }
        "0x4700 0x03" { return [ list   "Mtr2 Mtr Temp PTC"                             "[ expr 1.0/1.0 ]"             "$log_idqs"   "C"       "I16"    ""] }
        "0x4700 0x05" { return [ list   "Mtr2 Id_ref"                                   "[ expr 1.0/16.0 ]"            "$log_idqs"   "A"       "I16"    ""] }
        "0x4700 0x06" { return [ list   "Mtr2 Iq_ref"                                   "[ expr 1.0/16.0 ]"            "$log_idqs"   "A"       "I16"    ""] }
        "0x4700 0x07" { return [ list   "Mtr2 Id"                                       "[ expr 1.0/16.0 ]"            "$log_idqs"   "A"       "I16"    ""] }
        "0x4700 0x08" { return [ list   "Mtr2 Iq"                                       "[ expr 1.0/16.0 ]"            "$log_idqs"   "A"       "I16"    ""] }
        "0x4700 0x09" { return [ list   "Mtr2 Ud"                                       "[ expr 1.0/256.0 ]"           "$log_idqs"   "V"       "I16"    ""] }
        "0x4700 0x0a" { return [ list   "Mtr2 Uq"                                       "[ expr 1.0/256.0 ]"           "$log_idqs"   "V"       "I16"    ""] }
        "0x4700 0x0b" { return [ list   "Mtr2 Mod Index"                                "[ expr 100.0/255.0 ]"         "$log_idqs"   "%"       "I16"    ""] }
        "0x4700 0x0c" { return [ list   "Mtr2 I(RMS)"                                   "[ expr 1.0/1.0 ]"             "$log_idqs"   "A"       "I16"    ""] }
        "0x4700 0x0d" { return [ list   "Mtr2 V(RMS)"                                   "[ expr 1.0/256.0 ]"           "$log_idqs"   "V"       "I16"    ""] }
        "0x4700 0x0e" { return [ list   "Mtr1 w2"                                       "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x6071 0x00" { return [ list   "Mtr1 T_dem"                                    "[ expr 1.0/1.0 ]"             "1"           ""        "I16"    ""] }
        "0x6077 0x00" { return [ list   "Mtr1 T_act"                                    "[ expr 1.0/1.0 ]"             "1"           ""        "I16"    ""] }
        "0x6080 0x00" { return [ list   "Mtr1 w_lim"                                    "[ expr 1.0/1.0 ]"             "1"           "RPM"     "U32"    ""] }
        "0x7077 0x00" { return [ list   "Mtr2 Tact"                                     "[ expr 1.0/1.0 ]"             "1"           ""        "I16"    ""] }
        "0x606c 0x00" { return [ list   "Mtr1 w"                                        "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x60ff 0x00" { return [ list   "Mtr1 w ref"                                    "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x706c 0x00" { return [ list   "Mtr2 w"                                        "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x70ff 0x00" { return [ list   "Mtr2 w ref"                                    "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x786b 0x00" { return [ list   "Mtr3 act duty"                                 "[ expr 1.0/100.0 ]"           "1"           "%"       "I32"    ""] }
        "0x78ff 0x00" { return [ list   "Mtr3 tgt duty"                                 "[ expr 1.0/100.0 ]"           "1"           "%"       "I32"    ""] }
        "0x786c 0x00" { return [ list   "Mtr3 w"                                        "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x4780 0x01" { return [ list   "Mtr3 act duty"                                 "[ expr 1.0/100.0 ]"           "1"           "%"       "I16"    ""] }
        "0x4780 0x02" { return [ list   "Mtr3 I"                                        "[ expr 1.0/1.0 ]"             "1"           "A"       "I16"    ""] }
        "0x4783 0x01" { return [ list   "Mtr3 tgt duty"                                 "[ expr 1.0/100.0 ]"           "1"           "%"       "I16"    ""] }
        "0x4783 0x02" { return [ list   "Mtr3 ramp rate"                                "[ expr 1.0/1.0 ]"             "1"           "ms"      "U16"    ""] }
        "0x2020 0x01" { return [ list   "L Trac cw"                                     "[ expr 1.0/1.0 ]"             "$log_cw_sw"  ""        "U16"    ""] }
        "0x2020 0x02" { return [ list   "L Trac sw"                                     "[ expr 1.0/1.0 ]"             "$log_cw_sw"  ""        "U16"    ""] }
        "0x2020 0x03" { return [ list   "L Trac w_ref/max"                              "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x2020 0x04" { return [ list   "L Trac act spd"                                "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x2020 0x05" { return [ list   "L Trac Tmax/tgt"                               "[ expr 1.0/1.0 ]"             "1"           ""        "I16"    ""] }
        "0x2020 0x06" { return [ list   "L Trac act trq"                                "[ expr 1.0/1.0 ]"             "1"           ""        "I16"    ""] }
        "0x2021 0x01" { return [ list   "R Trac cw"                                     "[ expr 1.0/1.0 ]"             "$log_cw_sw"  ""        "U16"    ""] }
        "0x2021 0x02" { return [ list   "R Trac sw"                                     "[ expr 1.0/1.0 ]"             "$log_cw_sw"  ""        "U16"    ""] }
        "0x2021 0x03" { return [ list   "R Trac w_ref/max"                              "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x2021 0x04" { return [ list   "R Trac act spd"                                "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x2021 0x05" { return [ list   "R Trac Tmax/tgt"                               "[ expr 1.0/1.0 ]"             "1"           ""        "I16"    ""] }
        "0x2021 0x06" { return [ list   "R Trac act trq"                                "[ expr 1.0/1.0 ]"             "1"           ""        "I16"    ""] }
        "0x2040 0x01" { return [ list   "Pump cw"                                       "[ expr 1.0/1.0 ]"             "$log_cw_sw"  ""        "U16"    ""] }
        "0x2040 0x02" { return [ list   "Pump sw"                                       "[ expr 1.0/1.0 ]"             "$log_cw_sw"  ""        "U16"    ""] }
        "0x2040 0x03" { return [ list   "Pump w_ref"                                    "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x2040 0x04" { return [ list   "Pump act spd"                                  "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x2040 0x05" { return [ list   "Pump Tmax"                                     "[ expr 1.0/1.0 ]"             "1"           ""        "I16"    ""] }
        "0x2040 0x06" { return [ list   "Pump act trq"                                  "[ expr 1.0/1.0 ]"             "1"           ""        "I16"    ""] }
        "0x2060 0x01" { return [ list   "PSteer cw"                                     "[ expr 1.0/1.0 ]"             "$log_cw_sw"  ""        "U16"    ""] }
        "0x2060 0x02" { return [ list   "PSteer sw"                                     "[ expr 1.0/1.0 ]"             "$log_cw_sw"  ""        "U16"    ""] }
        "0x2060 0x03" { return [ list   "PSteer w_ref"                                  "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x2060 0x04" { return [ list   "PSteer act spd"                                "[ expr 1.0/1.0 ]"             "1"           "RPM"     "I32"    ""] }
        "0x2060 0x05" { return [ list   "PSteer Tmax"                                   "[ expr 1.0/1.0 ]"             "1"           ""        "I16"    ""] }
        "0x2060 0x06" { return [ list   "PSteer act trq"                                "[ expr 1.0/1.0 ]"             "1"           ""        "I16"    ""] }
        "0x2125 0x00" { return [ list   "Handbrake"                                     "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     ""] }
        "0x5100 0x01" { return [ list   "Vbat"                                          "[ expr 1.0/16.0 ]"            "1"           "V"       "U16"    ""] }
        "0x5100 0x02" { return [ list   "Ibat"                                          "[ expr 1.0/256.0 ]"           "1"           "A"       "I16"    ""] }
        "0x5100 0x03" { return [ list   "Vcap"                                          "[ expr 1.0/16.0 ]"            "1"           "V"       "U16"    ""] }
        "0x5100 0x04" { return [ list   "Heatsink"                                      "[ expr 1.0/1.0 ]"             "1"           "C"       "I8"     ""] }
        "0x5100 0x06" { return [ list   "Vbat (8bit)"                                   "[ expr 1.0/4.0 ]"             "1"           "V"       "U8"     ""] }
        "0x5100 0x07" { return [ list   "Vcap (8bit)"                                   "[ expr 1.0/4.0 ]"             "1"           "V"       "U8"     ""] }
        "0x5100 0x08" { return [ list   "12V Supply"                                    "[ expr 1.0/256.0 ]"           "1"           "V"       "U8"     ""] }
        "0x5320 0x00" { return [ list   "Highest Pri Fault"                             "[ expr 1.0/1.0 ]"             "1"           "C"       "U16"    ""] }
        "0x2620 0x00" { return [ list   "Throttle val"                                  "[ expr 1.0/32767.0 ]"         "1"           ""        "I16"    ""] }
        "0x2621 0x00" { return [ list   "Footbrake val"                                 "[ expr 1.0/32767.0 ]"         "1"           ""        "I16"    ""] }
        "0x2622 0x00" { return [ list   "Economy val"                                   "[ expr 1.0/32767.0 ]"         "1"           ""        "I16"    ""] }
        "0x2623 0x00" { return [ list   "Steer angle"                                   "[ expr 90.0/32767.0 ]"        "1"           "Deg"     "I16"    ""] }
        "0x2624 0x00" { return [ list   "Steer angle (deg)"                             "[ expr 1.0/100.0 ]"           "1"           "Deg"     "I16"    ""] }
        "0x2625 0x00" { return [ list   "Tilt angle"                                    "[ expr 1.0/100.0 ]"           "1"           "Deg"     "I16"    ""] }
        "0x2640 0x00" { return [ list   "Pump throt 1"                                  "[ expr 1.0/32767.0 ]"         "1"           ""        "I16"    ""] }
        "0x2641 0x00" { return [ list   "Pump throt 2"                                  "[ expr 1.0/32767.0 ]"         "1"           ""        "I16"    ""] }
        "0x2220 0x00" { return [ list   "Throttle Vlt"                                  "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2221 0x00" { return [ list   "FBrake Vlt"                                    "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2222 0x00" { return [ list   "Economy Vlt"                                   "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2223 0x00" { return [ list   "Steer Pot Vlt"                                 "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2240 0x00" { return [ list   "Pump Throt1 Vlt"                               "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2241 0x00" { return [ list   "Pump Throt2 Vlt"                               "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2400 0x00" { return [ list   "Line cont"                                     "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2401 0x00" { return [ list   "Ext LED"                                       "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2402 0x00" { return [ list   "Alarm buzz"                                    "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2403 0x00" { return [ list   "Horn"                                          "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2404 0x00" { return [ list   "Lights"                                        "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2405 0x00" { return [ list   "Service LED"                                   "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2407 0x00" { return [ list   "Line cont 2"                                   "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2420 0x00" { return [ list   "Electrobrake"                                  "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2421 0x00" { return [ list   "Motor Fan"                                     "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2422 0x00" { return [ list   "Motor isolator"                                "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2423 0x00" { return [ list   "Hi/Lo spd ind"                                 "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2440 0x00" { return [ list   "Pump contact"                                  "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2460 0x00" { return [ list   "Steer contact"                                 "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x2720 0x01" { return [ list   "Drvablty profs"                                "[ expr 1.0/1.0 ]"             "1"           ""        "U16"    ""] }
        "0x2720 0x02" { return [ list   "Trac state"                                    "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     "OFF FOOTBRAKING DRIVE STOP_AT_CD_RATE CD_DECEL CD_ACCEL NTRL_BRK_OR_COAST HILL_HOLD IDLE POWER_OFF CONTROLLED_ROLL_OFF "] }
        "0x2720 0x03" { return [ list   "Dir chng state"                                "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     "NO_DIR_SEL_NO_LDD FWD_SEL_LDD_FWD NO_DIR_SEL_LDD_FWD REV_SEL_LDD_FWD FWD_SEL_LDD_REV NO_DIR_SEL_LDD_REV REV_SEL_LDD_REV"] }
        "0x2720 0x04" { return [ list   "SRO state"                                     "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     "NO_FS1_NO_DIR =NO_FS1_DIR_ACTIVE FS1_ACTIVE_NO_DIR FS1_ACTIVE_DIR_ACTIVE SRO_FAULT"] }
        "0x2720 0x05" { return [ list   "Seat state"                                    "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     "SEAT_SWITCH_INACTIVE SEAT_SWITCH_ACTIVE IN_DRIVE SEAT_INACTIVE_IN_DRIVE SEAT_FAULT"] }
        "0x2720 0x15" { return [ list   "Chrg state"                                    "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     ""] }
        "0x6800 0x01" { return [ list   "Dig ins 0-7"                                   "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     ""] }
        "0x6800 0x02" { return [ list   "Dig ins 8-15"                                  "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     ""] }
        "0x6800 0x03" { return [ list   "Dig ins 16-23"                                 "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     ""] }
        "0x6800 0x04" { return [ list   "Dig ins 24-31"                                 "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     ""] }
        "0x2132 0x00" { return [ list   "Overload"                                      "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     ""] }
        "0x2134 0x00" { return [ list   "Tilt switch"                                   "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     ""] }
        "0x2135 0x00" { return [ list   "Platform Up"                                   "[ expr 1.0/1.0 ]"             "1"           ""        "U8"     ""] }
        "0x5602 0x01" { return [ list   "DSP Debug A"                                   "[ expr 1.0/65536.0 ]"         "1"           ""        "I32"    ""] }
        "0x5602 0x02" { return [ list   "DSP Debug B"                                   "[ expr 1.0/65536.0 ]"         "1"           ""        "I32"    ""] }
        "0x5602 0x03" { return [ list   "DSP Debug C"                                   "[ expr 1.0/65536.0 ]"         "1"           ""        "I32"    ""] }
        "0x5602 0x04" { return [ list   "DSP Debug D"                                   "[ expr 1.0/65536.0 ]"         "1"           ""        "I32"    ""] }
        "0x2721 0x00" { return [ list   "Speed"                                         "[ expr 1.0/256.0 ]"           "1"           ""        "I16"    ""] }
        "0x6c01 0x01" { return [ list   "Alg in 1"                                      "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c01 0x02" { return [ list   "Alg in 2"                                      "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c01 0x03" { return [ list   "Alg in 3"                                      "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c01 0x04" { return [ list   "Alg in 4"                                      "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c01 0x05" { return [ list   "Alg in 5"                                      "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c01 0x06" { return [ list   "Alg in 6"                                      "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c01 0x07" { return [ list   "Alg in 7"                                      "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c01 0x08" { return [ list   "Alg in 8"                                      "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c11 0x01" { return [ list   "Alg out 1"                                     "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c11 0x02" { return [ list   "Alg out 2"                                     "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c11 0x03" { return [ list   "Alg out 3"                                     "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c11 0x04" { return [ list   "Alg out 4"                                     "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c11 0x05" { return [ list   "Alg out 5"                                     "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c11 0x06" { return [ list   "Alg out 6"                                     "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c11 0x07" { return [ list   "Alg out 7"                                     "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6c11 0x08" { return [ list   "Alg out 8"                                     "[ expr 1.0/256.0 ]"           "1"           "V"       "U16"    ""] }
        "0x6078 0x00" { return [ list   "Mtr1 I"                                        "[ expr 1.0/1.0 ]"             "1"           "A"       "I16"    ""] }
        "0x7878 0x00" { return [ list   "Mtr3 I"                                        "[ expr 1.0/1.0 ]"             "1"           "A"       "I16"    ""] }
        "0x2790 0x01" { return [ list   "BDI"                                           "[ expr 1.0/1.0 ]"             "1"           "%"       "U8"     ""] }
        "0x5100 0x05" { return [ list   "24VDCDC"                                       "[ expr 1.0/16.0 ]"            "1"           "V"       "U16"    ""] }
        "0x2625 0x00" { return [ list   "Slope angle"                                   "[ expr 1.0/100.0 ]"           "1"           "Deg"     "I16"    ""] }
        "0x4602 0x01" { return [ list   "Mtr1 Temp/Trq CB"                              "[ expr 1.0/256.0 ]"           "1"           ""        "U16"    ""] }
        "0x4602 0x02" { return [ list   "Mtr1 Tj1"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4602 0x03" { return [ list   "Mtr1 Tj2"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4602 0x04" { return [ list   "Mtr1 Tj3"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4602 0x05" { return [ list   "Mtr1 Tj4"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4602 0x06" { return [ list   "Mtr1 Tj5"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4602 0x07" { return [ list   "Mtr1 Tj6"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4602 0x08" { return [ list   "Mtr1 Temp Est"                                 "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4602 0x09" { return [ list   "Mtr1 HSink Temp"                               "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I8"     ""] }
        "0x4602 0x0a" { return [ list   "Mtr1 Overall Temp"                             "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4602 0x0b" { return [ list   "Mtr1 Trq Dem"                                  "[ expr 1.0/64.0 ]"            "1"           "Nm"      "I16"    ""] }
        "0x4602 0x0c" { return [ list   "Mtr1 Trq Act"                                  "[ expr 1.0/64.0 ]"            "1"           "Nm"      "I16"    ""] }
        "0x4602 0x0d" { return [ list   "Mtr1 Trq Max"                                  "[ expr 1.0/64.0 ]"            "1"           "Nm"      "I16"    ""] }
        "0x4602 0x0e" { return [ list   "Mtr1 Trq Pow Lim"                              "[ expr 1.0/64.0 ]"            "1"           "Nm"      "I16"    ""] }
        "0x4602 0x0f" { return [ list   "Mtr1 Vtrip"                                    "[ expr 1.0/16.0 ]"            "1"           "V"       "U16"    ""] }
        "0x4602 0x10" { return [ list   "Mtr1 Vcutback"                                 "[ expr 1.0/32768.0 ]"         "1"           ""        "U16"    ""] }
        "0x4602 0x11" { return [ list   "Mtr1 Vbat"                                     "[ expr 1.0/16.0 ]"            "1"           "V"       "U16"    ""] }
        "0x4602 0x12" { return [ list   "Mtr1 Vcap"                                     "[ expr 1.0/16.0 ]"            "1"           "V"       "U16"    ""] }
        "0x4602 0x13" { return [ list   "Mtr1 Cap Temp"                                 "[ expr 1.0/1.0 ]"             "1"           "Deg"     "U16"    ""] }
        "0x4602 0x14" { return [ list   "Mtr1 Track Temp"                               "[ expr 1.0/1.0 ]"             "1"           "Deg"     "U16"    ""] }       
        "0x4603 0x01" { return [ list   "Mtr1 Temp PTC 2"                               "[ expr 1.0/1.0 ]"             "1"           "A"       "I16"    ""] }
        "0x4604 0x01" { return [ list   "Mtr1 Pj1"                                      "[ expr 1.0/256.0 ]"           "1"           "W"       "I16"    ""] }
        "0x4604 0x02" { return [ list   "Mtr1 Pj2"                                      "[ expr 1.0/256.0 ]"           "1"           "W"       "I16"    ""] }
        "0x4604 0x03" { return [ list   "Mtr1 Pj3"                                      "[ expr 1.0/256.0 ]"           "1"           "W"       "I16"    ""] }
        "0x4604 0x04" { return [ list   "Mtr1 Pj4"                                      "[ expr 1.0/256.0 ]"           "1"           "W"       "I16"    ""] }
        "0x4604 0x05" { return [ list   "Mtr1 Pj5"                                      "[ expr 1.0/256.0 ]"           "1"           "W"       "I16"    ""] }
        "0x4604 0x06" { return [ list   "Mtr1 Pj6"                                      "[ expr 1.0/256.0 ]"           "1"           "W"       "I16"    ""] }
        "0x4702 0x01" { return [ list   "Mtr2 Temp/Trq CB"                              "[ expr 1.0/256.0 ]"           "1"           ""        "U16"    ""] }
        "0x4702 0x02" { return [ list   "Mtr2 Tj1"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4702 0x03" { return [ list   "Mtr2 Tj2"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4702 0x04" { return [ list   "Mtr2 Tj3"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4702 0x05" { return [ list   "Mtr2 Tj4"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4702 0x06" { return [ list   "Mtr2 Tj5"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4702 0x07" { return [ list   "Mtr2 Tj6"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4702 0x08" { return [ list   "Mtr2 Temp Est"                                 "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4702 0x09" { return [ list   "Mtr2 HSink Temp"                               "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I8"     ""] }
        "0x4702 0x0a" { return [ list   "Mtr2 Overall Temp"                             "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4702 0x0b" { return [ list   "Mtr2 Trq Dem"                                  "[ expr 1.0/64.0 ]"            "1"           "Nm"      "I16"    ""] }
        "0x4702 0x0c" { return [ list   "Mtr2 Trq Act"                                  "[ expr 1.0/64.0 ]"            "1"           "Nm"      "I16"    ""] }
        "0x4702 0x0d" { return [ list   "Mtr2 Trq Max"                                  "[ expr 1.0/64.0 ]"            "1"           "Nm"      "I16"    ""] }
        "0x4702 0x0e" { return [ list   "Mtr2 Trq Pow Lim"                              "[ expr 1.0/64.0 ]"            "1"           "Nm"      "I16"    ""] }
        "0x4702 0x0f" { return [ list   "Mtr2 Vtrip"                                    "[ expr 1.0/16.0 ]"            "1"           "V"       "U16"    ""] }
        "0x4702 0x10" { return [ list   "Mtr2 Vcutback"                                 "[ expr 1.0/32768.0 ]"         "1"           ""        "U16"    ""] }
        "0x4702 0x11" { return [ list   "Mtr2 Vbat"                                     "[ expr 1.0/16.0 ]"            "1"           "V"       "U16"    ""] }
        "0x4702 0x12" { return [ list   "Mtr2 Vcap"                                     "[ expr 1.0/16.0 ]"            "1"           "V"       "U16"    ""] }
        "0x4782 0x01" { return [ list   "Mtr3 Temp/Trq CB"                              "[ expr 1.0/256.0 ]"           "1"           ""        "U16"    ""] }
        "0x4782 0x02" { return [ list   "Mtr3 Tj1"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4782 0x03" { return [ list   "Mtr3 Temp Est"                                 "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        "0x4782 0x04" { return [ list   "Mtr3 Max I"                                    "[ expr 1.0/1.0 ]"             "1"           "A"       "I16"    ""] }
        "0x4782 0x05" { return [ list   "Mtr3 Vcutback"                                 "[ expr 1.0/32768.0 ]"         "1"           ""        "U16"    ""] }
        "0x4782 0x06" { return [ list   "Mtr3 Tj2"                                      "[ expr 1.0/1.0 ]"             "1"           "Deg"     "I16"    ""] }
        
        "0x2910 0x01" { return [ list   "Throttle flags"                                "[ expr 1.0/1.0 ]"             "0"           ""        "U8"     ""] }
        "0x2910 0x02" { return [ list   "Throttle Input Characteristic"                 "[ expr 1.0/1.0 ]"             "0"           ""        "U8"     "User_defined Linear Curved Crawl "] }
        "0x2910 0x03" { return [ list   "Throttle 1 start voltage"                      "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x2910 0x04" { return [ list   "Throttle 1 start value"                        "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }
        "0x2910 0x05" { return [ list   "Throttle 1 end voltage"                        "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x2910 0x06" { return [ list   "Throttle 1 end value"                          "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }
        "0x2910 0x07" { return [ list   "Throttle 2 start voltage"                      "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x2910 0x08" { return [ list   "Throttle 2 start value"                        "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }
        "0x2910 0x09" { return [ list   "Throttle 2 end voltage"                        "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x2910 0x0a" { return [ list   "Throttle 2 end value"                          "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }
        "0x2910 0x0b" { return [ list   "User Char Pt 1 voltage"                        "[ expr 1.0/65535.0 ]"         "0"           ""        "U16"    ""] }
        "0x2910 0x0c" { return [ list   "User Char Pt 1 value"                          "[ expr 1.0/65535.0 ]"         "0"           ""        "U16"    ""] }
        "0x2910 0x0d" { return [ list   "User Char Pt 2 voltage"                        "[ expr 1.0/65535.0 ]"         "0"           ""        "U16"    ""] }
        "0x2910 0x0e" { return [ list   "User Char Pt 2 value"                          "[ expr 1.0/65535.0 ]"         "0"           ""        "U16"    ""] }
        "0x2910 0x0f" { return [ list   "User Char Pt 3 voltage"                        "[ expr 1.0/65535.0 ]"         "0"           ""        "U16"    ""] }
        "0x2910 0x10" { return [ list   "User Char Pt 3 value"                          "[ expr 1.0/65535.0 ]"         "0"           ""        "U16"    ""] }

        "0x2913 0x01" { return [ list   "Steer pot left voltage"                        "[ expr 1.0/256.0 ]"           "0"           "V"       "U16"    ""] }
        "0x2913 0x02" { return [ list   "Steer pot right voltage"                       "[ expr 1.0/256.0 ]"           "0"           "V"       "U16"    ""] }
        "0x2913 0x03" { return [ list   "Steer pot zero voltage"                        "[ expr 1.0/256.0 ]"           "0"           "V"       "U16"    ""] }
        "0x2913 0x04" { return [ list   "Cutback Map Pt 1 Angle"                        "[ expr 90.0/32767.0 ]"        "0"           "Deg"     "I16"    ""] }
        "0x2913 0x05" { return [ list   "Cutback Map Pt 1 Speed"                        "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }
        "0x2913 0x06" { return [ list   "Cutback Map Pt 2 Angle"                        "[ expr 90.0/32767.0 ]"        "0"           "Deg"     "I16"    ""] }
        "0x2913 0x07" { return [ list   "Cutback Map Pt 2 Speed"                        "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }
        "0x2913 0x08" { return [ list   "Cutback Map Pt 3 Angle"                        "[ expr 90.0/32767.0 ]"        "0"           "Deg"     "I16"    ""] }
        "0x2913 0x09" { return [ list   "Cutback Map Pt 3 Speed"                        "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }
        "0x2913 0x0a" { return [ list   "Cutback Map Pt 4 Angle"                        "[ expr 90.0/32767.0 ]"        "0"           "Deg"     "I16"    ""] }
        "0x2913 0x0b" { return [ list   "Cutback Map Pt 4 Speed"                        "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }
        "0x2913 0x0c" { return [ list   "Steering Map Pt 1 Angle"                       "[ expr 90.0/32767.0 ]"        "0"           "Deg"     "I16"    ""] }
        "0x2913 0x0d" { return [ list   "Steering Map Pt 1 Speed"                       "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }
        "0x2913 0x0e" { return [ list   "Steering Map Pt 2 Angle"                       "[ expr 90.0/32767.0 ]"        "0"           "Deg"     "I16"    ""] }
        "0x2913 0x0f" { return [ list   "Steering Map Pt 2 Speed"                       "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }
        "0x2913 0x10" { return [ list   "Steering Map Pt 3 Angle"                       "[ expr 90.0/32767.0 ]"        "0"           "Deg"     "I16"    ""] }
        "0x2913 0x11" { return [ list   "Steering Map Pt 3 Speed"                       "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }
        "0x2913 0x12" { return [ list   "Steering Map Pt 4 Angle"                       "[ expr 90.0/32767.0 ]"        "0"           "Deg"     "I16"    ""] }
        "0x2913 0x13" { return [ list   "Steering Map Pt 4 Speed"                       "[ expr 1.0/32767.0 ]"         "0"           ""        "I16"    ""] }

        "0x2920 0x01" { return [ list   "Maximum drive torque"                          "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2920 0x02" { return [ list   "Maximum direction change torque"               "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2920 0x03" { return [ list   "Maximum neutral brake torque"                  "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2920 0x04" { return [ list   "Maximum footbraking torque"                    "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2920 0x05" { return [ list   "Maximum forward speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2920 0x06" { return [ list   "Maximum reverse speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2920 0x07" { return [ list   "Drive acceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2920 0x08" { return [ list   "Direction change acceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2920 0x09" { return [ list   "Neutral brake acceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2920 0x0a" { return [ list   "Footbraking acceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2920 0x0b" { return [ list   "Drive deceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2920 0x0c" { return [ list   "Direction change deceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2920 0x0d" { return [ list   "Neutral brake deceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2920 0x0e" { return [ list   "Footbraking deceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }

        "0x2921 0x01" { return [ list   "Maximum drive torque"                          "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2921 0x02" { return [ list   "Maximum direction change torque"               "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2921 0x03" { return [ list   "Maximum neutral brake torque"                  "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2921 0x04" { return [ list   "Maximum footbraking torque"                    "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2921 0x05" { return [ list   "Maximum forward speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2921 0x06" { return [ list   "Maximum reverse speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2921 0x07" { return [ list   "Drive acceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2921 0x08" { return [ list   "Direction change acceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2921 0x09" { return [ list   "Neutral brake acceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2921 0x0a" { return [ list   "Footbraking acceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2921 0x0b" { return [ list   "Drive deceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2921 0x0c" { return [ list   "Direction change deceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2921 0x0d" { return [ list   "Neutral brake deceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2921 0x0e" { return [ list   "Footbraking deceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }

        "0x2922 0x01" { return [ list   "Maximum drive torque"                          "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2922 0x02" { return [ list   "Maximum direction change torque"               "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2922 0x03" { return [ list   "Maximum neutral brake torque"                  "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2922 0x04" { return [ list   "Maximum footbraking torque"                    "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2922 0x05" { return [ list   "Maximum forward speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2922 0x06" { return [ list   "Maximum reverse speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2922 0x07" { return [ list   "Drive acceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2922 0x08" { return [ list   "Direction change acceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2922 0x09" { return [ list   "Neutral brake acceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2922 0x0a" { return [ list   "Footbraking acceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2922 0x0b" { return [ list   "Drive deceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2922 0x0c" { return [ list   "Direction change deceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2922 0x0d" { return [ list   "Neutral brake deceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2922 0x0e" { return [ list   "Footbraking deceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }

        "0x2923 0x01" { return [ list   "Maximum drive torque"                          "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2923 0x02" { return [ list   "Maximum direction change torque"               "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2923 0x03" { return [ list   "Maximum neutral brake torque"                  "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2923 0x04" { return [ list   "Maximum footbraking torque"                    "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2923 0x05" { return [ list   "Maximum forward speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2923 0x06" { return [ list   "Maximum reverse speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2923 0x07" { return [ list   "Drive acceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2923 0x08" { return [ list   "Direction change acceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2923 0x09" { return [ list   "Neutral brake acceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2923 0x0a" { return [ list   "Footbraking acceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2923 0x0b" { return [ list   "Drive deceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2923 0x0c" { return [ list   "Direction change deceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2923 0x0d" { return [ list   "Neutral brake deceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2923 0x0e" { return [ list   "Footbraking deceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }

        "0x2924 0x01" { return [ list   "Maximum drive torque"                          "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2924 0x02" { return [ list   "Maximum direction change torque"               "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2924 0x03" { return [ list   "Maximum neutral brake torque"                  "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2924 0x04" { return [ list   "Maximum footbraking torque"                    "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2924 0x05" { return [ list   "Maximum forward speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2924 0x06" { return [ list   "Maximum reverse speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2924 0x07" { return [ list   "Drive acceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2924 0x08" { return [ list   "Direction change acceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2924 0x09" { return [ list   "Neutral brake acceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2924 0x0a" { return [ list   "Footbraking acceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2924 0x0b" { return [ list   "Drive deceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2924 0x0c" { return [ list   "Direction change deceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2924 0x0d" { return [ list   "Neutral brake deceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2924 0x0e" { return [ list   "Footbraking deceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }

        "0x2925 0x01" { return [ list   "Maximum drive torque"                          "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2925 0x02" { return [ list   "Maximum direction change torque"               "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2925 0x03" { return [ list   "Maximum neutral brake torque"                  "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2925 0x04" { return [ list   "Maximum footbraking torque"                    "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2925 0x05" { return [ list   "Maximum forward speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2925 0x06" { return [ list   "Maximum reverse speed"                         "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        "0x2925 0x07" { return [ list   "Drive acceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2925 0x08" { return [ list   "Direction change acceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2925 0x09" { return [ list   "Neutral brake acceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2925 0x0a" { return [ list   "Footbraking acceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2925 0x0b" { return [ list   "Drive deceleration rate"                       "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2925 0x0c" { return [ list   "Direction change deceleration rate"            "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2925 0x0d" { return [ list   "Neutral brake deceleration rate"               "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        "0x2925 0x0e" { return [ list   "Footbraking deceleration rate"                 "[ expr 1.0/1.0 ]"             "0"           " RPM/s"  "U16"    ""] }
        
        "0x2930 0x01" { return [ list   "Roll-off enable"                               "[ expr 1.0/1.0 ]"             "0"           ""        "U16"    "Disabled Enabled"] }
        "0x2930 0x02" { return [ list   "Maximum roll-off torque"                       "[ expr 1.0/10.0 ]"            "0"           " %"      "U16"    ""] }
        "0x2930 0x03" { return [ list   "Maximum roll-off speed"                        "[ expr 1.0/1.0 ]"             "0"           " RPM"    "U16"    ""] }
        
        "0x4641 0x02" { return [ list   "Maximum Stator Current (Is_max)"               "[ expr 1.0/1.0 ]"             "0"           " A(RMS)" "U16"    ""] }
        "0x4641 0x03" { return [ list   "Minimum Magnetising Current (Im_min)"          "[ expr 1.0/256.0 ]"           "0"           " A(RMS)" "U16"    ""] }
        "0x4641 0x04" { return [ list   "Maximum Magnetising/Direct Current (Id_max)"   "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4641 0x05" { return [ list   "Number of Pole Pairs (np)"                     "[ expr 1.0/1.0 ]"             "0"           ""        "U16"    ""] }
        "0x4641 0x06" { return [ list   "Stator Resistance (Rs)"                        "[ expr 1000.0/4096.0 ]"       "0"           " mOhms"  "U16"    ""] }
        "0x4641 0x07" { return [ list   "Rated Stator Current"                          "[ expr 1.0/1.0]"              "0"           " A(RMS)" "U16"    ""] }
        "0x4641 0x08" { return [ list   "Rotor Resistance (Rr)"                         "[ expr 1000.0/4096.0 ]"       "0"           " mOhms"  "U16"    ""] }
        "0x4641 0x09" { return [ list   "Magnetising Inductance (Lm)"                   "[ expr 1000000.0/65536.0 ]"   "0"           " uH"     "U16"    ""] }
        "0x4641 0x0a" { return [ list   "Stator Leakage Inductance (Lls)"               "[ expr 1000000.0/1048576.0 ]" "0"           " uH"     "U16"    ""] }
        "0x4641 0x0b" { return [ list   "Rotor Leakage Inductance (Llr)"                "[ expr 1000000.0/1048576.0 ]" "0"           " uH"     "U16"    ""] }
        "0x4641 0x0c" { return [ list   "Nominal battery voltage"                       "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4641 0x0d" { return [ list   "Current control proportional gain (Kp)"        "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4641 0x0e" { return [ list   "Torque control integral gain"                  "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4641 0x0f" { return [ list   "Current control integral gain"                 "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4641 0x10" { return [ list   "Reactive Gain Boost"                           "[ expr 1.0/65536.0 ]"         "0"           " H"      "U16"    ""] }
        "0x4641 0x11" { return [ list   "RPM Limiter Gain"                              "[ expr 1.0/65536.0 ]"         "0"           ""        "U16"    ""] }
        "0x4641 0x12" { return [ list   "Voltage Constant (Ke)"                         "[ expr 1.0/32768.0 ]"         "0"           " V/rads" "U16"    ""] }
        "0x4641 0x13" { return [ list   "Im rated"                                      "[ expr 1.0/16.0 ]"            "0"           " A"      "U16"    ""] }
        "0x4641 0x14" { return [ list   "Maximum drive slip"                            "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x4641 0x15" { return [ list   "Maximum brake slip"                            "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x4641 0x16" { return [ list   "Openloop start speed"                          "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x4641 0x17" { return [ list   "Openloop exit speed"                           "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x4641 0x18" { return [ list   "Frequency Slope"                               "[ expr 1.0/1.0 ]"             "0"           ""        "U16"    ""] }
        "0x4641 0x19" { return [ list   "Frequency control Kp"                          "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x4641 0x1a" { return [ list   "Frequency control Ki"                          "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x4641 0x1b" { return [ list   "Slip control Kp"                               "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4641 0x1c" { return [ list   "Slip control Ki"                               "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4641 0x1d" { return [ list   "Slip control factor"                           "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x4641 0x1e" { return [ list   "Max drive mod index"                           "[ expr 100.0/4096.0 ]"        "0"           ""        "U16"    ""] }
        "0x4641 0x1f" { return [ list   "Max brake mod index"                           "[ expr 100.0/4096.0 ]"        "0"           ""        "U16"    ""] }
        "0x4641 0x20" { return [ list   "Ls/Ls'"                                        "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x4641 0x21" { return [ list   "Voltage Control Kp"                            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4641 0x22" { return [ list   "Voltage Control Ki"                            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4641 0x23" { return [ list   "Start of Frequency Cutback"                    "[ expr 1.0/4096.0 ]"          "0"           " %"      "U16"    ""] }
        "0x4641 0x24" { return [ list   "Start of Voltage Cutback"                      "[ expr 1.0/4096.0 ]"          "0"           " %"      "U16"    ""] }
        "0x4641 0x25" { return [ list   "Id recovery rate"                              "[ expr 1.0/1.0 ]"             "0"           " A/s"    "U16"    ""] }
        
        "0x4741 0x02" { return [ list   "Maximum Stator Current (Is_max)"               "[ expr 1.0/1.0 ]"             "0"           " A(RMS)" "U16"    ""] }
        "0x4741 0x03" { return [ list   "Minimum Magnetising Current (Im_min)"          "[ expr 1.0/256.0 ]"           "0"           " A(RMS)" "U16"    ""] }
        "0x4741 0x04" { return [ list   "Maximum Magnetising/Direct Current (Id_max)"   "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4741 0x05" { return [ list   "Number of Pole Pairs (np)"                     "[ expr 1.0/1.0 ]"             "0"           ""        "U16"    ""] }
        "0x4741 0x06" { return [ list   "Stator Resistance (Rs)"                        "[ expr 1000.0/4096.0 ]"       "0"           " mOhms"  "U16"    ""] }
        "0x4741 0x07" { return [ list   "Rated Stator Current"                          "[ expr 1.0/1.0]"              "0"           " A(RMS)" "U16"    ""] }
        "0x4741 0x08" { return [ list   "Rotor Resistance (Rr)"                         "[ expr 1000.0/4096.0 ]"       "0"           " mOhms"  "U16"    ""] }
        "0x4741 0x09" { return [ list   "Magnetising Inductance (Lm)"                   "[ expr 1000000.0/65536.0 ]"   "0"           " uH"     "U16"    ""] }
        "0x4741 0x0a" { return [ list   "Stator Leakage Inductance (Lls)"               "[ expr 1000000.0/1048576.0 ]" "0"           " uH"     "U16"    ""] }
        "0x4741 0x0b" { return [ list   "Rotor Leakage Inductance (Llr)"                "[ expr 1000000.0/1048576.0 ]" "0"           " uH"     "U16"    ""] }
        "0x4741 0x0c" { return [ list   "Nominal battery voltage"                       "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4741 0x0d" { return [ list   "Current control proportional gain (Kp)"        "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4741 0x0e" { return [ list   "Torque control integral gain"                  "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4741 0x0f" { return [ list   "Current control integral gain"                 "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4741 0x10" { return [ list   "Reactive Gain Boost"                           "[ expr 1.0/65536.0 ]"         "0"           " H"      "U16"    ""] }
        "0x4741 0x11" { return [ list   "RPM Limiter Gain"                              "[ expr 1.0/65536.0 ]"         "0"           ""        "U16"    ""] }
        "0x4741 0x12" { return [ list   "Voltage Constant (Ke)"                         "[ expr 1.0/32768.0 ]"         "0"           " V/rads" "U16"    ""] }
        "0x4741 0x13" { return [ list   "Im rated"                                      "[ expr 1.0/16.0 ]"            "0"           " A"      "U16"    ""] }
        "0x4741 0x14" { return [ list   "Maximum drive slip"                            "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x4741 0x15" { return [ list   "Maximum brake slip"                            "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x4741 0x16" { return [ list   "Openloop start speed"                          "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x4741 0x17" { return [ list   "Openloop exit speed"                           "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x4741 0x18" { return [ list   "Frequency Slope"                               "[ expr 1.0/1.0 ]"             "0"           ""        "U16"    ""] }
        "0x4741 0x19" { return [ list   "Frequency control Kp"                          "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x4741 0x1a" { return [ list   "Frequency control Ki"                          "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x4741 0x1b" { return [ list   "Slip control Kp"                               "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4741 0x1c" { return [ list   "Slip control Ki"                               "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4741 0x1d" { return [ list   "Slip control factor"                           "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x4741 0x1e" { return [ list   "Max drive mod index"                           "[ expr 100.0/4096.0 ]"        "0"           ""        "U16"    ""] }
        "0x4741 0x1f" { return [ list   "Max brake mod index"                           "[ expr 100.0/4096.0 ]"        "0"           ""        "U16"    ""] }
        "0x4741 0x20" { return [ list   "Ls/Ls'"                                        "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x4741 0x21" { return [ list   "Voltage Control Kp"                            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4741 0x22" { return [ list   "Voltage Control Ki"                            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4741 0x23" { return [ list   "Start of Frequency Cutback"                    "[ expr 1.0/4096.0 ]"          "0"           " %"      "U16"    ""] }
        "0x4741 0x24" { return [ list   "Start of Voltage Cutback"                      "[ expr 1.0/4096.0 ]"          "0"           " %"      "U16"    ""] }
        "0x4741 0x25" { return [ list   "Id recovery rate"                              "[ expr 1.0/1.0 ]"             "0"           " A/s"    "U16"    ""] }
        
        "0x6410 0x02" { return [ list   "Maximum Stator Current (Is_max)"               "[ expr 1.0/1.0 ]"             "0"           " A(RMS)" "U16"    ""] }
        "0x6410 0x03" { return [ list   "Minimum Magnetising Current (Im_min)"          "[ expr 1.0/256.0 ]"           "0"           " A(RMS)" "U16"    ""] }
        "0x6410 0x04" { return [ list   "Maximum Magnetising/Direct Current (Id_max)"   "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x6410 0x05" { return [ list   "Number of Pole Pairs (np)"                     "[ expr 1.0/1.0 ]"             "0"           ""        "U16"    ""] }
        "0x6410 0x06" { return [ list   "Stator Resistance (Rs)"                        "[ expr 1000.0/4096.0 ]"       "0"           " mOhms"  "U16"    ""] }
        "0x6410 0x07" { return [ list   "Rated Stator Current"                          "[ expr 1.0/1.0]"              "0"           " A(RMS)" "U16"    ""] }
        "0x6410 0x08" { return [ list   "Rotor Resistance (Rr)"                         "[ expr 1000.0/4096.0 ]"       "0"           " mOhms"  "U16"    ""] }
        "0x6410 0x09" { return [ list   "Magnetising Inductance (Lm)"                   "[ expr 1000000.0/65536.0 ]"   "0"           " uH"     "U16"    ""] }
        "0x6410 0x0a" { return [ list   "Stator Leakage Inductance (Lls)"               "[ expr 1000000.0/1048576.0 ]" "0"           " uH"     "U16"    ""] }
        "0x6410 0x0b" { return [ list   "Rotor Leakage Inductance (Llr)"                "[ expr 1000000.0/1048576.0 ]" "0"           " uH"     "U16"    ""] }
        "0x6410 0x0c" { return [ list   "Nominal battery voltage"                       "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x6410 0x0d" { return [ list   "Current control proportional gain (Kp)"        "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x6410 0x0e" { return [ list   "Torque control integral gain"                  "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x6410 0x0f" { return [ list   "Current control integral gain"                 "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x6410 0x10" { return [ list   "Reactive Gain Boost"                           "[ expr 1.0/65536.0 ]"         "0"           " H"      "U16"    ""] }
        "0x6410 0x11" { return [ list   "RPM Limiter Gain"                              "[ expr 1.0/65536.0 ]"         "0"           ""        "U16"    ""] }
        "0x6410 0x12" { return [ list   "Voltage Constant (Ke)"                         "[ expr 1.0/32768.0 ]"         "0"           " V/rads" "U16"    ""] }
        "0x6410 0x13" { return [ list   "Im rated"                                      "[ expr 1.0/16.0 ]"            "0"           " A"      "U16"    ""] }
        "0x6410 0x14" { return [ list   "Maximum drive slip"                            "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x6410 0x15" { return [ list   "Maximum brake slip"                            "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x6410 0x16" { return [ list   "Openloop start speed"                          "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x6410 0x17" { return [ list   "Openloop exit speed"                           "[ expr 1.0/1.0 ]"             "0"           " rad/s"  "U16"    ""] }
        "0x6410 0x18" { return [ list   "Frequency Slope"                               "[ expr 1.0/1.0 ]"             "0"           ""        "U16"    ""] }
        "0x6410 0x19" { return [ list   "Frequency control Kp"                          "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x6410 0x1a" { return [ list   "Frequency control Ki"                          "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x6410 0x1b" { return [ list   "Slip control Kp"                               "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x6410 0x1c" { return [ list   "Slip control Ki"                               "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x6410 0x1d" { return [ list   "Slip control factor"                           "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x6410 0x1e" { return [ list   "Max drive mod index"                           "[ expr 100.0/4096.0 ]"        "0"           ""        "U16"    ""] }
        "0x6410 0x1f" { return [ list   "Max brake mod index"                           "[ expr 100.0/4096.0 ]"        "0"           ""        "U16"    ""] }
        "0x6410 0x20" { return [ list   "Ls/Ls'"                                        "[ expr 1.0/256.0 ]"           "0"           ""        "U16"    ""] }
        "0x6410 0x21" { return [ list   "Voltage Control Kp"                            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x6410 0x22" { return [ list   "Voltage Control Ki"                            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x6410 0x23" { return [ list   "Start of Frequency Cutback"                    "[ expr 1.0/4096.0 ]"          "0"           " %"      "U16"    ""] }
        "0x6410 0x24" { return [ list   "Start of Voltage Cutback"                      "[ expr 1.0/4096.0 ]"          "0"           " %"      "U16"    ""] }
        "0x6410 0x25" { return [ list   "Id recovery rate"                              "[ expr 1.0/1.0 ]"             "0"           " A/s"    "U16"    ""] }

        "0x4610 0x01" { return [ list   "Flux Map - Pt 1 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4610 0x02" { return [ list   "Flux Map - Pt 1 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4610 0x03" { return [ list   "Flux Map - Pt 2 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4610 0x04" { return [ list   "Flux Map - Pt 2 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4610 0x05" { return [ list   "Flux Map - Pt 3 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4610 0x06" { return [ list   "Flux Map - Pt 3 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4610 0x07" { return [ list   "Flux Map - Pt 4 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4610 0x08" { return [ list   "Flux Map - Pt 4 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4610 0x09" { return [ list   "Flux Map - Pt 5 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4610 0x0a" { return [ list   "Flux Map - Pt 5 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4610 0x0b" { return [ list   "Flux Map - Pt 6 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4610 0x0c" { return [ list   "Flux Map - Pt 6 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4610 0x0d" { return [ list   "Flux Map - Pt 7 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4610 0x0e" { return [ list   "Flux Map - Pt 7 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4610 0x0f" { return [ list   "Flux Map - Pt 8 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4610 0x10" { return [ list   "Flux Map - Pt 8 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4610 0x11" { return [ list   "Flux Map - Pt 9 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4610 0x12" { return [ list   "Flux Map - Pt 9 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }

        "0x4611 0x01" { return [ list   "Power Limit Map - Pt 1 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4611 0x02" { return [ list   "Power Limit Map - Pt 1 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4611 0x03" { return [ list   "Power Limit Map - Pt 2 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4611 0x04" { return [ list   "Power Limit Map - Pt 2 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4611 0x05" { return [ list   "Power Limit Map - Pt 3 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4611 0x06" { return [ list   "Power Limit Map - Pt 3 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4611 0x07" { return [ list   "Power Limit Map - Pt 4 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4611 0x08" { return [ list   "Power Limit Map - Pt 4 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4611 0x09" { return [ list   "Power Limit Map - Pt 5 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4611 0x0a" { return [ list   "Power Limit Map - Pt 5 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4611 0x0b" { return [ list   "Power Limit Map - Pt 6 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4611 0x0c" { return [ list   "Power Limit Map - Pt 6 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4611 0x0d" { return [ list   "Power Limit Map - Pt 7 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4611 0x0e" { return [ list   "Power Limit Map - Pt 7 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4611 0x0f" { return [ list   "Power Limit Map - Pt 8 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4611 0x10" { return [ list   "Power Limit Map - Pt 8 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4611 0x11" { return [ list   "Power Limit Map - Pt 9 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4611 0x12" { return [ list   "Power Limit Map - Pt 9 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }

        "0x4612 0x01" { return [ list   "Voltage Cutback Map - Pt 1 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4612 0x02" { return [ list   "Voltage Cutback Map - Pt 1 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4612 0x03" { return [ list   "Voltage Cutback Map - Pt 2 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4612 0x04" { return [ list   "Voltage Cutback Map - Pt 2 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4612 0x05" { return [ list   "Voltage Cutback Map - Pt 3 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4612 0x06" { return [ list   "Voltage Cutback Map - Pt 3 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4612 0x07" { return [ list   "Voltage Cutback Map - Pt 4 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4612 0x08" { return [ list   "Voltage Cutback Map - Pt 4 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4612 0x09" { return [ list   "Voltage Cutback Map - Pt 5 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4612 0x0a" { return [ list   "Voltage Cutback Map - Pt 5 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4612 0x0b" { return [ list   "Voltage Cutback Map - Pt 6 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4612 0x0c" { return [ list   "Voltage Cutback Map - Pt 6 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4612 0x0d" { return [ list   "Voltage Cutback Map - Pt 7 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4612 0x0e" { return [ list   "Voltage Cutback Map - Pt 7 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4612 0x0f" { return [ list   "Voltage Cutback Map - Pt 8 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4612 0x10" { return [ list   "Voltage Cutback Map - Pt 8 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4612 0x11" { return [ list   "Voltage Cutback Map - Pt 9 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4612 0x12" { return [ list   "Voltage Cutback Map - Pt 9 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }

        "0x4614 0x01" { return [ list   "Saturation Map - Pt 1 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4614 0x02" { return [ list   "Saturation Map - Pt 1 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4614 0x03" { return [ list   "Saturation Map - Pt 2 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4614 0x04" { return [ list   "Saturation Map - Pt 2 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4614 0x05" { return [ list   "Saturation Map - Pt 3 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4614 0x06" { return [ list   "Saturation Map - Pt 3 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4614 0x07" { return [ list   "Saturation Map - Pt 4 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4614 0x08" { return [ list   "Saturation Map - Pt 4 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4614 0x09" { return [ list   "Saturation Map - Pt 5 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4614 0x0a" { return [ list   "Saturation Map - Pt 5 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4614 0x0b" { return [ list   "Saturation Map - Pt 6 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4614 0x0c" { return [ list   "Saturation Map - Pt 6 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4614 0x0d" { return [ list   "Saturation Map - Pt 7 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4614 0x0e" { return [ list   "Saturation Map - Pt 7 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4614 0x0f" { return [ list   "Saturation Map - Pt 8 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4614 0x10" { return [ list   "Saturation Map - Pt 8 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4614 0x11" { return [ list   "Saturation Map - Pt 9 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4614 0x12" { return [ list   "Saturation Map - Pt 9 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }

        "0x4710 0x01" { return [ list   "Flux Map - Pt 1 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4710 0x02" { return [ list   "Flux Map - Pt 1 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4710 0x03" { return [ list   "Flux Map - Pt 2 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4710 0x04" { return [ list   "Flux Map - Pt 2 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4710 0x05" { return [ list   "Flux Map - Pt 3 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4710 0x06" { return [ list   "Flux Map - Pt 3 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4710 0x07" { return [ list   "Flux Map - Pt 4 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4710 0x08" { return [ list   "Flux Map - Pt 4 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4710 0x09" { return [ list   "Flux Map - Pt 5 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4710 0x0a" { return [ list   "Flux Map - Pt 5 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4710 0x0b" { return [ list   "Flux Map - Pt 6 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4710 0x0c" { return [ list   "Flux Map - Pt 6 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4710 0x0d" { return [ list   "Flux Map - Pt 7 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4710 0x0e" { return [ list   "Flux Map - Pt 7 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4710 0x0f" { return [ list   "Flux Map - Pt 8 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4710 0x10" { return [ list   "Flux Map - Pt 8 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4710 0x11" { return [ list   "Flux Map - Pt 9 Torque"                        "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4710 0x12" { return [ list   "Flux Map - Pt 9 Im"                            "[ expr 1.0/64.0 ]"            "0"           " A(RMS)" "U16"    ""] }

        "0x4711 0x01" { return [ list   "Power Limit Map - Pt 1 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4711 0x02" { return [ list   "Power Limit Map - Pt 1 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4711 0x03" { return [ list   "Power Limit Map - Pt 2 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4711 0x04" { return [ list   "Power Limit Map - Pt 2 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4711 0x05" { return [ list   "Power Limit Map - Pt 3 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4711 0x06" { return [ list   "Power Limit Map - Pt 3 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4711 0x07" { return [ list   "Power Limit Map - Pt 4 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4711 0x08" { return [ list   "Power Limit Map - Pt 4 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4711 0x09" { return [ list   "Power Limit Map - Pt 5 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4711 0x0a" { return [ list   "Power Limit Map - Pt 5 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4711 0x0b" { return [ list   "Power Limit Map - Pt 6 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4711 0x0c" { return [ list   "Power Limit Map - Pt 6 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4711 0x0d" { return [ list   "Power Limit Map - Pt 7 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4711 0x0e" { return [ list   "Power Limit Map - Pt 7 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4711 0x0f" { return [ list   "Power Limit Map - Pt 8 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4711 0x10" { return [ list   "Power Limit Map - Pt 8 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }
        "0x4711 0x11" { return [ list   "Power Limit Map - Pt 9 Max Torque"             "[ expr 1.0/16.0 ]"            "0"           " Nm"     "U16"    ""] }
        "0x4711 0x12" { return [ list   "Power Limit Map - Pt 9 Speed"                  "[ expr 1.0/1.0  ]"            "0"           " RPM"    "U16"    ""] }

        "0x4712 0x01" { return [ list   "Voltage Cutback Map - Pt 1 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4712 0x02" { return [ list   "Voltage Cutback Map - Pt 1 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4712 0x03" { return [ list   "Voltage Cutback Map - Pt 2 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4712 0x04" { return [ list   "Voltage Cutback Map - Pt 2 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4712 0x05" { return [ list   "Voltage Cutback Map - Pt 3 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4712 0x06" { return [ list   "Voltage Cutback Map - Pt 3 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4712 0x07" { return [ list   "Voltage Cutback Map - Pt 4 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4712 0x08" { return [ list   "Voltage Cutback Map - Pt 4 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4712 0x09" { return [ list   "Voltage Cutback Map - Pt 5 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4712 0x0a" { return [ list   "Voltage Cutback Map - Pt 5 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4712 0x0b" { return [ list   "Voltage Cutback Map - Pt 6 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4712 0x0c" { return [ list   "Voltage Cutback Map - Pt 6 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4712 0x0d" { return [ list   "Voltage Cutback Map - Pt 7 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4712 0x0e" { return [ list   "Voltage Cutback Map - Pt 7 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4712 0x0f" { return [ list   "Voltage Cutback Map - Pt 8 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4712 0x10" { return [ list   "Voltage Cutback Map - Pt 8 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4712 0x11" { return [ list   "Voltage Cutback Map - Pt 9 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4712 0x12" { return [ list   "Voltage Cutback Map - Pt 9 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }

        "0x4714 0x01" { return [ list   "Saturation Map - Pt 1 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4714 0x02" { return [ list   "Saturation Map - Pt 1 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4714 0x03" { return [ list   "Saturation Map - Pt 2 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4714 0x04" { return [ list   "Saturation Map - Pt 2 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4714 0x05" { return [ list   "Saturation Map - Pt 3 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4714 0x06" { return [ list   "Saturation Map - Pt 3 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4714 0x07" { return [ list   "Saturation Map - Pt 4 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4714 0x08" { return [ list   "Saturation Map - Pt 4 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4714 0x09" { return [ list   "Saturation Map - Pt 5 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4714 0x0a" { return [ list   "Saturation Map - Pt 5 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4714 0x0b" { return [ list   "Saturation Map - Pt 6 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4714 0x0c" { return [ list   "Saturation Map - Pt 6 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4714 0x0d" { return [ list   "Saturation Map - Pt 7 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4714 0x0e" { return [ list   "Saturation Map - Pt 7 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4714 0x0f" { return [ list   "Saturation Map - Pt 8 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4714 0x10" { return [ list   "Saturation Map - Pt 8 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }
        "0x4714 0x11" { return [ list   "Saturation Map - Pt 9 Im"                      "[ expr 1.0/16.0 ]"            "0"           " A(RMS)" "U16"    ""] }
        "0x4714 0x12" { return [ list   "Saturation Map - Pt 9 Lm"                      "[ expr 1.0/64.0 ]"            "0"           " puH"    "U16"    ""] }

        "0x4792 0x01" { return [ list   "Voltage Cutback Map - Pt 1 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4792 0x02" { return [ list   "Voltage Cutback Map - Pt 1 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4792 0x03" { return [ list   "Voltage Cutback Map - Pt 2 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4792 0x04" { return [ list   "Voltage Cutback Map - Pt 2 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4792 0x05" { return [ list   "Voltage Cutback Map - Pt 3 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4792 0x06" { return [ list   "Voltage Cutback Map - Pt 3 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4792 0x07" { return [ list   "Voltage Cutback Map - Pt 4 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4792 0x08" { return [ list   "Voltage Cutback Map - Pt 4 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4792 0x09" { return [ list   "Voltage Cutback Map - Pt 5 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4792 0x0a" { return [ list   "Voltage Cutback Map - Pt 5 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4792 0x0b" { return [ list   "Voltage Cutback Map - Pt 6 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4792 0x0c" { return [ list   "Voltage Cutback Map - Pt 6 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4792 0x0d" { return [ list   "Voltage Cutback Map - Pt 7 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4792 0x0e" { return [ list   "Voltage Cutback Map - Pt 7 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4792 0x0f" { return [ list   "Voltage Cutback Map - Pt 8 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4792 0x10" { return [ list   "Voltage Cutback Map - Pt 8 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
        "0x4792 0x11" { return [ list   "Voltage Cutback Map - Pt 9 Voltage"            "[ expr 1.0/16.0 ]"            "0"           " V"      "U16"    ""] }
        "0x4792 0x12" { return [ list   "Voltage Cutback Map - Pt 9 Cutback"            "[ expr 1.0/32768.0 ]"         "0"           ""        "U16"    ""] }
    }
    
    return [ list "Unknown object $index, $subindex" "1" "0" "" "U8" ""]
}
