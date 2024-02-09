/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.26$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 21:13:44$
	$ModDate:05/12/2007 21:09:56$

ORIGINAL AUTHOR
    Alison King

DESCRIPTION
	Auto testing interface class for DriveWizard

REFERENCES    

MODIFICATION HISTORY
    $Log:  39613: AutoValidate.cs 

   Rev 1.26    05/12/2007 21:13:44  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DriveWizard
{
	/// <summary>
	/// Test Details structure: data structure defined to hold the pertinent data required
	/// from the EDS when reading each object dictionary item.  This is required for personality
	/// parameter screens etc, where we need to simulate data entry.
	/// </summary>
	public struct TestDetails
	{
		/// <summary>minimum valid value for this OD item, read from the EDS</summary>
		public long minValue;

		/// <summary>maximum valid value for this OD item, read from the EDS</summary>
		public long maxValue;
		
		/// <summary>default value for this OD item, read from the EDS</summary>
		public long defaultValue;
		
		/// <summary>access type (RO, WO etc) for this OD item, read from the EDS</summary>
		public string accessType;
		
		/// <summary> scaling factor to be applied to this OD item, read from the EDS</summary>
		public float scaling;
		
		/// <summary> whether this OD item is an Sevcon enumerated display format, read from the EDS</summary>
		public bool enumType;
	}

	/// <summary>
	/// Validate state enumerated type: states used by the AutoValidate class to sequence
	/// the tests.  These states generally refer to what screen is being tested, whether 
	/// the user is logged in etc.
	/// </summary>
	public enum ValidateState
	{
		MAIN_WINDOW_NO_SYSTEM_ESTABLISHED,
		ESTABLISH_CAN_COMMS_WINDOW,
		ESTABLISHED_COMMS_BAUD,
		SET_COMMS_1M,
		ESTABLISHED_COMMS,
		FOUND_SYSTEM,
		LOGIN_WINDOW,
		ATTEMPT_LOGIN,
		LOGGED_IN,
		FAILED_TO_LOGIN,
		PERSONALITY_PARAMETERS_WINDOW,
		MAIN_WINDOW_PERS_PARM_COMPLETED,
		DCF_WINDOW,
		COMPLETED_DCF_TEST,
		FAULTLOG_WINDOW,
		COMPLETED_FAULTLOG_TESTS,
		SYSTEMLOG_WINDOW,
		COMPLETED_SYSTEMLOG_TESTS,
		EVENTLOG_WINDOW,
		COMPLETED_EVENTLOG_TESTS,
		OPLOG_WINDOW,
		COMPLETED_OPLOG_TESTS,
		MONITORING_WINDOW,
		COMPLETED_MONITORING_TESTS,
		FINISHED_AUTO_TEST
	};

	/// <summary>
	/// Personality Parameters window state: defines sub states for this window to
	/// sequence through entering invalid and valid entries for each item in the object
	/// dictionary.
	/// </summary>
	public enum PersParmState
	{
		TEST_INIT,
		ENTER_INVALID_LOW_VALUE,
		ENTER_LOWEST_VALUE,
		ENTER_HIGHEST_VALUE,
		ENTER_INVALID_HIGH_VALUE,
		ENTER_DEFAULT_VALUE,
		SELECT_ITEM_FOR_TEST,
		END_TEST,
		WAIT_FOR_WINDOW_TO_CLOSE
	};

	/// <summary>
	/// DCF window sub state: used to sequence through when to upload an object dictionary
	/// from the controller, to download a DCF file to a controller etc.
	/// </summary>
	public enum DCFState
	{
		TEST_INITIATE,
		UPLOAD_FROM_CONTROLLER,
		OPEN_DCF_FILE,
		DOWNLOAD_DCF_TO_CONTROLLER,
		END_TEST,
		WAIT_FOR_WINDOW_TO_CLOSE
	};

	/// <summary>
	/// Logs window sub state: used to sequence through the retrieval of all the different
	/// fault log types and setting of filters and clearing logs.
	/// </summary>
	public enum LogsState
	{
		TEST_INITIATE,
		RETRIEVE_FAULT_LOG,
		CLEAR_LOG,
		GROUP_FILTERS,
		END_TEST,
		WAIT_FOR_WINDOW_TO_CLOSE
	};

	/// <summary>
	/// Monitor window sub state: used to sequence through the selection of different object
	/// dictionary items for monitoring, testing the monitoring and then back for reselection
	/// of the next OD items for monitor.
	/// </summary>
	public enum MonitorState
	{
		TEST_INITIATE,
		SELECTION_FOR_MONITOR,
		CLICK_MONITOR_BUTTON,
		MONITOR_DISPLAY,
		END_TEST,
		WAIT_FOR_WINDOW_TO_CLOSE
	};

	/// <summary>
	/// AutoValidate class:  This class is the co-ordinator class for the auto validation
	/// testing.  This contains all the test states to oversee which test is being carried
	/// out and it also reads the EDS file in a completely different manner in order to
	/// exercise the personality parameters screen.
	/// </summary>
	public class AutoValidate
	{
		/// <summary> Overall test state, indicating whether we've logged in and which DW screen
		/// is being tested at the moment.</summary>
		public ValidateState testState = ValidateState.MAIN_WINDOW_NO_SYSTEM_ESTABLISHED;

		/// <summary> Indicates whether there is an auto validation test currently in progress or not</summary>
		public bool validationRunning = false;
		
		/// <summary> Indicates whether there is an auto validation test currently in progress or not, as a static value</summary>
		public static bool staticValidationRunning = false;
		
		/// <summary> Personality parameters window test sub state 
		/// - which test within this window is currently running</summary>
		public PersParmState persParamState = PersParmState.TEST_INIT;
		
		/// <summary> DCF window test sub state
		/// - which test within this window is currently running</summary>
		public DCFState dcfState = DCFState.TEST_INITIATE;
		
		/// <summary> Fault logs window test sub state
		/// - which test within this window is currently running</summary>
		public LogsState logsState = LogsState.TEST_INITIATE;
		
		/// <summary> Data monitoring window test sub state
		/// - which test within this window is currently running</summary>
		public MonitorState monitorState = MonitorState.TEST_INITIATE;
		
		/// <summary> Data monitoring window: the last row in the data table selected for 
		/// monitoring in the last group of 10 selected (ready to pick up for the next selection 
		/// group)</summary>
		public int lastItemInGridSelected = 0;
		
		/// <summary> Data monitoring window: the start row of the data table selected for 
		/// monitoring in the last group of 10 selected (ready to deselect before the next 
		/// selection  group)</summary>
		public int startOfMonitorGroup = 0;
		
		/// <summary> Personality parameters window: Indicates whether this object dictionary
		/// item is an invalid one to set to min,max,default etc.  For example, it is not valid
		/// to change the node ID or baud rate half way through the auto test.</summary>
		public bool invalidValue = false;
		
		// not used?
		//public bool error = false;
		
		/// <summary> Which row in the data table of the personality parameters window is currently
		/// being tested</summary>
		public int persParamRow = 0;
		
		/// <summary> Wait timer used during the retrieval of fault logs</summary>
		public int waitTimer;

		/// <summary> Each window's feedback code (copied over) so that we can interpret the feedback
		/// code delivered by the DI to the GUI following each test</summary>
		public DIFeedbackCode testFeedback = DIFeedbackCode.DICodeUnset;
		
		/// <summary> Reading of the EDS file as a sorted list, deliberately different manner to main DW code</summary>
		public System.Collections.SortedList eds = null;
		
		// timer used to flush to debug out file buffer into the file
		private static	System.Threading.Timer flushTimer;

		//-------------------------------------------------------------------------
		/// <summary>
		/// Auto validate constructor which sets up the debug file flush timer and adds the debug listeners.
		/// </summary>
		//-------------------------------------------------------------------------
		public AutoValidate()
		{
			try
			{
				flushTimer = new System.Threading.Timer( new TimerCallback( threadTimerExpired ), null, 250, 250 );
#if DEBUG
				System.Console.WriteLine( "Flush timer for auto testing started." );
#endif
				Debug.Listeners.Add( new TextWriterTraceListener( "Debug.out" ) );
				Debug.WriteLine( "Opened debug file." );
			}
			catch(Exception debugException)
			{
#if DEBUG
				Message.Show( "Could not open 'debug.out' file. Exception: " + debugException.Message );
#endif
			}
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Auto validate destructor which kills the debug file flush timer.
		/// </summary>
		//-------------------------------------------------------------------------
		~AutoValidate()
		{
			if ( flushTimer != null )
			{
				flushTimer.Dispose();

				if ( flushTimer != null )
				{
					flushTimer = null;
				}

#if DEBUG
				System.Console.WriteLine( "Flush timer for auto testing disposed." );
#endif
			}
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// This function is called every time the flushTimer timer expires. It is used to
		/// flush the debug out file buffer to the file to ensure no test result data is lost.
		/// </summary>
		/// <param name="state">object state, required by windows but not used by DW</param>
		//-------------------------------------------------------------------------
		static void threadTimerExpired( Object state )
		{
			Debug.Flush();
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Reads the EDS file defining this node and stores the pertinent information into the indexList
		/// sorted list.
		/// </summary>
		/// <param name="localSystemInfo">system information object, used to find the relevant EDS file</param>
		/// <param name="nodeNo">Node to perform the auto test on</param>
		//-------------------------------------------------------------------------
		[Conditional ("AUTOVALIDATE")]
		public void readEDS( ref SystemInfo localSystemInfo, int nodeNo )
		{
			#region local variable declarations and initialisation
			StreamReader sr;
			FileStream	fs;
			string input;
			string [] split = null;
			#endregion

			#region update debug.out file with test step being executed
			Debug.WriteLine( "" );
			Debug.WriteLine("TEST (2a) READING EDS FILE INTO AN ARRAY LIST" );
			#endregion

			#region read the EDS file details into indexList sorted list if valid nodeNo
			if ( localSystemInfo.nodes.Length >= nodeNo )
			{
				// create a new sorted list which replaces any older ones
				SortedList indexList = new SortedList();

				#region Only read the file it it already exists
				if ( File.Exists( localSystemInfo.nodes[ nodeNo ].EDSorDCFfilepath ) )
				{
					try
					{
						#region open the EDS file stream for reading, read it all into a buffer and then close streams
						fs = new FileStream( localSystemInfo.nodes[ nodeNo ].EDSorDCFfilepath, System.IO.FileMode.Open, System.IO.FileAccess.Read );
						sr = new StreamReader( fs );

						input = sr.ReadToEnd();

						if ( sr != null )
						{
							sr.Close();
						}

						if ( fs != null )
						{
							fs.Close();
						}
						#endregion

						#region split the entire file buffer on each new line and convert to upper case
						split = input.Split( '\n' );

						for ( int i = 0; i < split.Length; i++ )
						{
							split[ i ] = split[ i ].Trim('\r' );
							split[ i ] = split[ i ].ToUpper();
							#endregion
						}

						#region local variable declaration and variable initialisation
						string temp;
						int index, sub;
						long indexAndSub;
						TestDetails indexDetails;
						indexDetails = new TestDetails();
						indexDetails.accessType = "";
						indexDetails.defaultValue = 0;
						indexDetails.minValue = 0;
						indexDetails.maxValue = 0;
						indexDetails.scaling = 1.0F;
						indexDetails.enumType = false;
						index = 0;
						sub = 0;
						#endregion

						#region for each line in the EDS file
						for ( int i = 0; i < split.Length; i++ )
						{
							#region if start of a new object description
							if ( split[ i ].StartsWith("[") && ( split[ i ][5] == ']' )  )
							{
								/* if the max or min value is non-zero then add the 
								 * new object description details to the index list
								 */
								if ( ( indexDetails.maxValue != 0 ) || ( indexDetails.maxValue != 0 ) )
								{
									indexAndSub = (index << 16) + sub;
									indexList.Add( indexAndSub, indexDetails );
								}

								// extract the new object index from the text & convert to an integer
								temp = split[i].Substring( 1, 4 );
								index = System.Convert.ToInt32( temp, 16 );

								#region initialise indexDetails to default values, ready for new object to be read
								sub = 0;
								indexDetails = new TestDetails();
								indexDetails.accessType = "";
								indexDetails.defaultValue = 0;
								indexDetails.minValue = 0;
								indexDetails.maxValue = 0;
								indexDetails.scaling = 1.0F;
								indexDetails.enumType = false;
								#endregion
							}
							#endregion
							#region else if start of a sub object description
							else if ( split[ i ].StartsWith("[") && ( split[ i ].IndexOf( "SUB" ) != -1 ) )
							{
								/* if the max or min value is non-zero then add the 
								 * new object description details to the index list
								 */
								if ( ( indexDetails.maxValue != 0 ) || ( indexDetails.maxValue != 0 ) )
								{
									indexAndSub = (index << 16) + sub;
									indexList.Add( indexAndSub, indexDetails );
								}


								// extract the new object index from the text & convert to an integer
								temp = split[i].Substring( 1, 4 );
								index = System.Convert.ToInt32( temp, 16 );

								// extract the new object sub index from the text & convert to an integer
								sub = -1;
								int pos = split[ i ].IndexOf( "]" );
								if ( pos != -1 )
								{
									temp = split[i].Substring( 8, (pos - 8) );
									sub = System.Convert.ToInt32( temp, 16 );
								}

								#region initialise indexDetails to default values, ready for new object to be read
								indexDetails = new TestDetails();
								indexDetails.accessType = "";
								indexDetails.defaultValue = 0;
								indexDetails.minValue = 0;
								indexDetails.scaling = 1.0F;
								indexDetails.maxValue = 0;
								indexDetails.enumType = false;
								#endregion
							}
							#endregion
							#region else if current object's low limit definition, extract value & convert to a long
							else if ( split[ i ].IndexOf( "LOWLIMIT=" ) != -1 )
							{
								split[ i ] = split[ i ].TrimEnd( ' ' );
								int pos = split[ i ].IndexOf( "=" );
								temp = split[ i ].Substring( ( pos + 1 ), ( split[ i ].Length - pos - 1 ) );
								if ( split[ i ].IndexOf( "0X" ) != -1 )
								{
									indexDetails.minValue = System.Convert.ToInt64( temp, 16 );
								}
								else
								{
									indexDetails.minValue = System.Convert.ToInt64( temp, 10 );
								}
							}
							#endregion
							#region else if current object's high limit definition, extract value & convert to a long
							else if ( split[ i ].IndexOf( "HIGHLIMIT=" ) != -1 )
							{
								split[ i ] = split[ i ].TrimEnd( ' ' );
								int pos = split[ i ].IndexOf( "=" );
								temp = split[ i ].Substring( ( pos + 1 ), ( split[ i ].Length - pos - 1 ) );
								if ( split[ i ].IndexOf( "0X" ) != -1 )
								{
									indexDetails.maxValue = System.Convert.ToInt64( temp, 16 );
								}
								else
								{
									indexDetails.maxValue = System.Convert.ToInt64( temp, 10 );
								}
							}
							#endregion
							#region else if current object's default definition, extract value & convert to a long
							else if ( split[ i ].IndexOf( "DEFAULTVALUE=" ) != -1 )
							{
								split[ i ] = split[ i ].TrimEnd( ' ' );
								int pos = split[ i ].IndexOf( "=" );
								temp = split[ i ].Substring( ( pos + 1 ), ( split[ i ].Length - pos - 1 ) );
								
								if ( split[ i ].IndexOf( "NODEID" ) != -1 )
								{
								}
								else if ( split[ i ].IndexOf( "0X" ) != -1 )
								{
									indexDetails.defaultValue = System.Convert.ToInt64( temp, 16 );
								}
								else
								{
									indexDetails.defaultValue = System.Convert.ToInt64( temp, 10 );
								}
							}
							#endregion
							#region else if current object's access type definition, extract and convert
							else if ( split[ i ].IndexOf( "ACCESSTYPE=" ) != -1 )
							{
								split[ i ] = split[ i ].TrimEnd( ' ' );
								int pos = split[ i ].IndexOf( "=" );
								temp = split[ i ].Substring( ( pos + 1 ), ( split[ i ].Length - pos - 1 ) );
								indexDetails.accessType = temp;
							}
							#endregion
							#region else if current object's scaling factor, extract value and convert to a float
							else if ( split[ i ].IndexOf( "SEVCONFIELD SCALING=" ) != -1 )
							{
								split[ i ] = split[ i ].TrimEnd( ' ' );
								int pos = split[ i ].IndexOf( "=" );
								temp = split[ i ].Substring( ( pos + 1 ), ( split[ i ].Length - pos - 1 ) );
								indexDetails.scaling = (float)(System.Convert.ToDouble( temp ));
							}
							#endregion
								#region else if current object's number display format, extract and determine if numerical or enumerated
							else if ( split[ i ].IndexOf( "SEVCONFIELD NUMBER_FORMAT=" ) != -1 )
							{
								if ( split[ i ].IndexOf( "BASE" ) == -1 )
								{
									indexDetails.enumType = true;
								}
							}
							#endregion
						}
						#endregion

						#region if either a max or min value was read from the file, add the index and sub to the sorted list
						if ( ( indexDetails.maxValue != 0 ) || ( indexDetails.maxValue != 0 ) )
						{
							indexAndSub = (index << 16) + sub;
							indexList.Add( indexAndSub, indexDetails );
						}
						#endregion

						#region make indexList visible to rest of DW and update debug.out file with test result
						this.eds = indexList;
						Debug.WriteLine( "Read EDS file.,OK" );
						Debug.WriteLine( "" );
						#endregion
					}
					catch ( Exception e )
					{
						MessageBox.Show( e.Message );
					}
				}
				#endregion
				#region update debug.out file with test results indicating EDS file read failure
				else
				{
					Debug.WriteLine( "Read EDS file.,FAIL,EDS file did not exist." );
					Debug.WriteLine( "" );
				}
				#endregion
			}
			#endregion

			#region initialise parameters for the personality parameters window test (next)
			persParamRow = 0;
			persParamState = PersParmState.TEST_INIT;
			#endregion
		}

		#region not used by DriveWizard
//		[Conditional ("AUTOVALIDATE")]
//		public void altLE()
//		{
//			// ALT key press & release
//			keybd_event( 0x12, 0x45, 0, (IntPtr)0 );
//			keybd_event( 0x12, 0x45, KEYEVENTF_KEYUP, (IntPtr)0 );
//
//			// 'L' key press & release
//			keybd_event( 0x4C, 0x45, 0, (IntPtr)0 );
//			keybd_event( 0x4C, 0x45, KEYEVENTF_KEYUP, (IntPtr)0 );
//
//			// 'E' key press & release
//			keybd_event( 0x45, 0x45, 0, (IntPtr)0 );
//			keybd_event( 0x45, 0x45, KEYEVENTF_KEYUP, (IntPtr)0 );
//		}
//
//		[Conditional ("AUTOVALIDATE")]
//		public void altS()
//		{
//			// ALT key press & release
//			keybd_event( 0x12, 0x45, 0, (IntPtr)0 );
//			keybd_event( 0x12, 0x45, KEYEVENTF_KEYUP, (IntPtr)0 );
//							
//			// 'S' key press & release
//			keybd_event( 0x53, 0x45, 0, (IntPtr)0 );
//			keybd_event( 0x53, 0x45, KEYEVENTF_KEYUP, (IntPtr)0 );
//		}
//
//		//user32.dll
//		//   // Declares managed prototypes for unmanaged functions.
//		//   [ DllImport( "User32.dll", EntryPoint="MessageBox", 
//		//      CharSet=CharSet.Auto )]
//		//   public static extern int MsgBox( int hWnd, String text, String caption, 
//		//      uint type );
//
//		[ DllImport( "User32.dll", CharSet=CharSet.Auto )]
//		public static extern void keybd_event( [In]byte bVk, byte bScan, [In]UInt32 flags, [In]IntPtr extraInfo );
//
//		private const UInt32 KEYEVENTF_EXTENDEDKEY = 0x0001;
//		private const UInt32 KEYEVENTF_KEYUP = 0x0002;
//
	}
	#endregion
}
