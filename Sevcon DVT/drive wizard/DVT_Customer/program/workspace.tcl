# window state
wm state . zoomed


# set CAN baud rate
if {$can_available} {
    set newbaud 100
    set canbaud($newbaud) 1
    set_baud
}

# set CAN types
set can_show_type(unknown) 1
set can_show_type(tx) 1
set can_show_type(sync) 1
set can_show_type(emcy) 1
set can_show_type(sdo) 1
set can_show_type(debug) 1
set can_show_type(nmt) 1
set can_show_type(pdo) 1


# set CAN baud buttons state
display_can_baud_buttons 0


# information window
show_info_window 1


# restore tk_getOpenFile's last directory
if {![file isdirectory {C:/arnaud/SEVCON mAS/DVT_Customer/program}]} {
    set dvt_last_selected_dir [pwd]
} else {
    set dvt_last_selected_dir {C:/arnaud/SEVCON mAS/DVT_Customer/program}
}

return
