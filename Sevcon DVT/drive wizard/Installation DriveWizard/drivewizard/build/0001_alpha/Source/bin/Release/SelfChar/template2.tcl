variable param_data
array set param_data {  ppr     <!pulses_per_rev>
                        Trat    <!motor_rated_torque>
                        Ismax   <!max_stator_current>
                        Immin   <!min_magnetising_current>
                        immax   <!max_magnetising_current>
                        Np      <!pole_pairs>
                        Rs      <!stator_resistance>
                        Israt   <!rated_stator_current>
                        Rr      <!rotor_resistance>
                        Lm      <!magnetising_inductance>
                        Lls     <!stator_leakage_inductance>
                        Llr     <!rotor_leakage_inductance>
                        Vb      <!nominal_battery_voltage>
                        Kp      <!current_control_prop_gain>
                        a       <!torque_control_alpha>
                        B       <!current_control_beta>
                        RGB     <!reactive_gain_boost>
                        RLG     <!rpm_limiter_gain>
                    }

                    
#
# flux_map_xinfo - Torque demands
#   
variable flux_map_xdata  
vector create flux_map_xdata(9)               
flux_map_xdata set       {  <!flux_map_td1>
                            <!flux_map_td2>
                            <!flux_map_td3>
                            <!flux_map_td4>
                            <!flux_map_td5>
                            <!flux_map_td6>
                            <!flux_map_td7>
                            <!flux_map_td8>
                            <!flux_map_td9>
                         }                
#
# flux_map_yinfo - Mag currents
#      
variable flux_map_ydata     
vector create flux_map_ydata(9)                                                    
flux_map_ydata set  {  <!flux_map_im1>
                       <!flux_map_im2> 
                       <!flux_map_im3>
                       <!flux_map_im4>
                       <!flux_map_im5>
                       <!flux_map_im6>
                       <!flux_map_im7>
                       <!flux_map_im8>
                       <!flux_map_im9>
                    }                 
                          
#                         
# Power limit map lookup  
#
variable plim_map_xdata                                                                             
vector create plim_map_xdata(9)                                                    
plim_map_xdata set  {   <!p_limit_map_rpm1>       
                        <!p_limit_map_rpm2>     
                        <!p_limit_map_rpm3>    
                        <!p_limit_map_rpm4>    
                        <!p_limit_map_rpm5>    
                        <!p_limit_map_rpm6>    
                        <!p_limit_map_rpm7>    
                        <!p_limit_map_rpm8>    
                        <!p_limit_map_rpm9>    
                     }                               
variable plim_map_ydata                       
vector create plim_map_ydata(9)                                                    
plim_map_ydata set  {   <!p_limit_map_tmax1> 
                        <!p_limit_map_tmax2> 
                        <!p_limit_map_tmax3> 
                        <!p_limit_map_tmax4>
                        <!p_limit_map_tmax5>
                        <!p_limit_map_tmax6>
                        <!p_limit_map_tmax7> 
                        <!p_limit_map_tmax8>
                        <!p_limit_map_tmax9>
                     }                                                                                                                
                    
                                                                    
#                          
# Torque Cutback map       
#    
variable tcut_map_xdata     
vector create tcut_map_xdata(9)                                                    
tcut_map_xdata set  { <!torque_cut_map_v1> 
                      <!torque_cut_map_v2>
                      <!torque_cut_map_v3>
                      <!torque_cut_map_v4>
                      <!torque_cut_map_v5>
                      <!torque_cut_map_v6>
                      <!torque_cut_map_v7>
                      <!torque_cut_map_v8>
                      <!torque_cut_map_v9>
                    }                                                                                                              

variable tcut_map_ydata                         
vector create tcut_map_ydata(9)                                                    
tcut_map_ydata set  {  <!torque_cut_map_g1>
                       <!torque_cut_map_g2>
                       <!torque_cut_map_g3>
                       <!torque_cut_map_g4>
                       <!torque_cut_map_g5>
                       <!torque_cut_map_g6>
                       <!torque_cut_map_g7>
                       <!torque_cut_map_g8>
                       <!torque_cut_map_g9>
                        }                          
                                                                                                                                                 
                                                                    
#                                                                                                           
# Field weakening map                                                                                       
#  
variable fw_map_xdata  
vector create fw_map_xdata(9)                                                                                                                                                        
fw_map_xdata set {   <!fw_map_rpm1>   
                     <!fw_map_rpm2> 
                     <!fw_map_rpm3>
                     <!fw_map_rpm4>
                     <!fw_map_rpm5>
                     <!fw_map_rpm6>
                     <!fw_map_rpm7>
                     <!fw_map_rpm8>
                     <!fw_map_rpm9>
                  }                                                                                                               

variable fw_map_ydata   
vector create fw_map_ydata(9)                                                                                                                                                                                                                                                                                       
fw_map_ydata set  {  <!fw_map_g1>
                     <!fw_map_g2>
                     <!fw_map_g3>
                     <!fw_map_g4>
                     <!fw_map_g5>
                     <!fw_map_g6>
                     <!fw_map_g7>
                     <!fw_map_g8>
                     <!fw_map_g9>
                  }                                                                                                                   
                                                                                                                                                                                
                                                                                                                                                                                
                                                                                                                                                                                
#
