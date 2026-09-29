package ifneeded rbc 0.1 "
    # This package always requires Tk
    [list package require Tk]
    [list load [file join $dir rbc01.dll] Rbc]
    # Library files are in a subdirectory during builds/tests
    if { ! [file exists [file join $dir graph.tcl]] } {
	[list set ::rbc_library [file join $dir library]]
	[list source [file join $dir library graph.tcl]]
    } else {
	[list set ::rbc_library $dir]
	[list source [file join $dir graph.tcl]]
    }
"
