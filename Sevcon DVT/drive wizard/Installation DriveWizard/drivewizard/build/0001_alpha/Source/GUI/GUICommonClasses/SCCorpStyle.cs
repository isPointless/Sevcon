/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.117$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/09/2008 23:15:34$
	$ModDate:22/09/2008 22:58:24$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	Definitions used throughout Drive Wizard, defined in a central repository.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36702: SCCorpStyle.cs 

   Rev 1.117    23/09/2008 23:15:34  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.116    12/03/2008 12:59:02  ak
 SDONoResponseRetries set back to 3.


   Rev 1.115    25/02/2008 16:26:22  jw
 New parameter NumAllowedBlockFails which is number of Sequnce Blaock fails
 that can occur before DW reverts to segmented download for remainder of the
 programming cycle. Trade off between sequence timeouts and slowed segmented
 transfer


   Rev 1.114    21/02/2008 09:22:40  jw
 VCI2 param removed


   Rev 1.113    19/02/2008 15:30:02  jw
 Find Node ID timeout needs to be different to listenIn timeout.  VCI3 can
 cope with shorter Find Node IDs timeout that VCI2 wihich gives better
 performance


   Rev 1.112    18/02/2008 14:18:38  jw
 VCI2 CAN methods renamed and merged for consistency with VCI3 - towards back
 compatibility


   Rev 1.111    13/02/2008 14:59:56  jw
 Number of retries reduced


   Rev 1.110    25/01/2008 10:46:54  jw
 VCI3 and Vista functionality files merge - more testing required


   Rev 1.109    22/01/2008 12:31:06  ak
 SDOMaxConsecutiveNoResponses added so that "3" is not strewn throughout the
 code.


   Rev 1.108    05/12/2007 21:26:40  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing; //contains Font definition
using System.Windows.Forms;
using System.Data;
using System.Collections;

namespace DriveWizard
{
	#region enumerated types
	internal enum TVImages 
	{
		slOffOff, slOnOff, slOffOn, slOnOn,
		msOffOff, msOnOff, msOffOn, msOnOn,
		triangles3Col, triangles1Col, triangleSingle, 
		COBExtPDO, UsrCust, BusConfig, FLT, Monitor,
		IntPDOs, ReProgram, SelfChar, logs, DefaultIco};
	public enum myIcons {HEADER, TOGGLE, HANDBRAKE, LED , SINEWAVE, NOTMAPPED, KEYSWITCH, HORNSWITCH, FOOTPEDAL, FORWARDSW, REVERSESW, PUMP, INCHFW, INCHREV, HIGHSPEEDSW, SPANNER, STEERINGWHEEL, FAN, CLOCK, BUZZER};

	public enum TNTagType {SYSTEMSTATUS, COBSCREEN, BUSCONFIGSCREEN, SELFCHARSCREEN, PROGSCREEN, 
		LOGS, FAULTLOGSCREEN, SYSTEMLOGSCREEN, OPLOGSCREEN, COUNTERLOGSCREEN, DATALOGSSCREEN, CANNODE, 
		XMLHEADER, EDSSECTION, ODITEM, ODSUB, 
		PreSetPDO_TractionLeft =0xFF0, PreSetPDO_TractionRight = 0xFF1, 
		PreSetPDO_Pump = 0xFF2, PreSetPDO_PowerSteer = 0xFF3};
	#endregion enumerated types

	/// <summary>
	/// Summary description for CorporateStyleCLass.
	/// </summary>
	/// 
	#region structure definitions
	#region TreeNodeTag Structure definition
	public class treeNodeTag 
	{
		internal TNTagType tagType;
		internal int nodeID;
		internal int tableindex;
		internal ODItemData assocSub;
		internal treeNodeTag(TNTagType tagtype, int NodeID, int tableIndex, ODItemData odSub)
		{
			tagType = tagtype;
			nodeID = NodeID;
			tableindex = tableIndex;
			assocSub = odSub;
		}
	};
	#endregion TreeNodeTag Structure definition

	internal struct emergencyMessage
	{
		internal string _nodeID;
		internal string _message;
		internal emergencyMessage(int nodeid, string message)
		{
			if((nodeid <=0) || (nodeid>=128))
			{
				_nodeID = "unknown";
			}
			else
			{
				_nodeID = nodeid.ToString();
			}
			_message = message;
		}
	};
	#endregion structure definitions

	internal class SCCorpStyle
	{
		internal const ushort SDONoResponseRetries = 3;
        internal const ushort SDOMaxConsecutiveNoResponses = 3;
		internal const ushort maxConnectedDevices = 8; 
		#region Device identity constants (vendor IDs, product ranges, variants etc)
		#region Sevcon vendor ID number
		internal const uint SevconID =	0x001e;
		#endregion

		#region Recognised Product Variants
		internal const byte bootloader_variant = 0xFF;
		internal const byte selfchar_variant_old = 0x00;
		internal const byte selfchar_variant_new = 0xFE;
		internal const byte App_variant_lowlimit = 0x00;
		internal const byte App_variant_highlimit = 0xF0;
		#endregion Recognised Product Variants 

		#region product ranges
        //DR3800256 cater for newer products
		internal enum ProductRange
		{
			UNKNOWN,
			ESPAC,
			PST,
			SEVCONDISPLAY,
			NANO,
            HIPAC,
            DUAL_ESPAC,
            GEN4,
            CALIBRATOR
		};
		#endregion

		#region device type strings
		internal enum DeviceType
		{
			CONTROLLER,
			BOOTLOADER,
			THIRD_PARTY,
			UNKNOWN
		};

		internal static string [] DeviceTypeStrings = { "Controller", "Bootloader", "Third party", "Unknown" };
        internal static string[] SevconProductRanges = { "<Unknown Product>", "espAC", "PST", "Display", "Nano", "HipAC 10", "Dual Motor", "Gen4", "Calibrator" }; //DR3800256 
		internal static string [] SevconProductVariants = { "<Unknown Variant>", "AC", "" };
		#endregion

		/// <summary>Highest level of access when logging on to a Sevcon node. </summary>
		internal const int HighestAccessLevel = 5;

		#endregion
		public static ArrayList VirtualCANnodes;
		#region Sevcon DI constants definitions

		public const int ODheader = -1;
		#region communication profile constant definitions
		public const int CommsProfileStart =		0x1000;
		public const int SyncCOBIDIndex =			0x1005;
		public const int TimeStampCOBIDIndex =		0x1012;
		public const int EmcyCOBIDItem =			0x1014;
		public const int ProducerHeartBeatTime =	0x1017;
		public const int EmcyConsumerCOBIDItem =	0x1028;
		public const int ServerSDOSetupMin =		0x1200;
		public const int ServerSDOSetupMax =		0x127f;
		public const int ClientSDOSetupMin =		0x1280;
		public const int ClientSDOSetupMax =		0x12ff;
		//PDO area - receive
		public const int PDORxCommsSetupMin =		0x1400;
		public const int PDORxCommsSetupMax =		0x15ff;
		public const int PDORxMappingMin =			0x1600;
		public const int PDORxMappingMax =			0x17ff;
		//PDO area - transmit
		public const int PDOTxCommsSetupMin =		0x1800;
		public const int PDOTxCommsSetupMax =		0x19ff;
		public const int PDOTxMappingMin =			0x1a00;
		public const int PDOTxMappingMax =			0x1bff;
		//	
		public const int CommsProfileEnd =			0x1fff;



		internal const uint Bit31Mask =						0x80000000;
		internal const uint Bit30Mask =						0x40000000;
		internal const uint Bit29Mask =						0x20000000;
		internal const uint Bit30To0Mask =					0x7FFFFFFF;
		internal const uint Bits28To0Mask =					0x1fffffff;
		internal const uint Bits10To0Mask =					0x000007ff;
		internal const uint Bit15To0Mask =					0x00007fff;
		internal const uint Bits23To16Mask =					0x00ff0000;

		public const int ServerSDOReceiveCOBIDSubIndex =	0x01;
		public const int ServerSDOTransmitCOBIDSubIndex =	0x02;
		public const int ClientSDOTransmitCOBIDSubIndex =	0x01;
		public const int ClientSDOReceiveCOBIDSubIndex =	0x02;

		public const int PDOCommsNoSubsSubIndex =			0x00;
		public const int PDOCommsCOBIDSubIndex =			0x01;
		public const int PDOCommsTxTypeSubIndex =			0x02;
		public const int PDOCommsInhibitTimeSubIndex =		0x03;
		public const int PDOCommsEventTimeSubIndex =		0x05;
		public const int PDOMapNoSubsSubIndex =				0x00;
		public const int PDOMapBaseMapSubIndex =			0x01;

		public const int PDOToCOBIDObjectOffset =			0x200;

		public const int eventTimeMax =						0x7fff;
		public const int inhibitTimeMax =					0x7fff;
		#endregion

		#region save communication profile constant definitions
		public const long SaveBackwardsValue =		0x65766173;  //EVAS
		#endregion

		#region readin node ID, baud rate and login constant defintions
		public const int BaudRateSubItem =					0x02;
		public const int NodeIDSubItem =					0x01;

		/*
		 * objFlags meanings in EDS/DCF which are used when writing an entire object dictionary
		 * from a DCF file to a controller or reading an entire OD from a controller to write to
		 * a file.  This is necessary because, for example, you do not want to download a fault
		 * log which was read from one node down to another controller (it is erroneous 
		 * information in this context and it makes no sense to mislead the user).
		 */
		public const int	RefuseWriteOnDownload		= 0x01;
		public const int	RefuseReadOnScan			= 0x02;
		#endregion

		#region descriptor for unknown event IDs
		public const string UnknownIDDescriptor = "<unknown ID>";	
		#endregion

        //DR38000265 use CANOPEN.XML by default if no matching XML file found for a device
        public const string DefaultXMLFile = "CANOPEN.XML";
		#region logs and DOMAIN_UPLOAD sub object definitions
		public const int	SystemFIFOSubObject = 1;		
		public const int	FaultsFIFOSubObject = 2;
		public const int	EventCountersSubObject = 3;
		public const int	CustomerOpMonitorSubObject = 4;
		public const int	SevconOpMonitorSubObject = 5;
		public const int	ActiveFaultListSubObject = 6;
		public const int	EventIDListSubObject = 10;

		public const int FaultsFIFOLogLengthSubIndex = 2;
		public const int SystemFIFOLogLengthSubIndex = 2;
		public const int ActiveFaultFIFOLogLengthSubIndex = 1;

		// known size of the each fault log entry (as defined in FIFOLogEntry data structure)
		public const int sizeOfFaultLogEntry = 8;

		// known size of the each event log entry (as defined in EventLogEntry data structure)
		public const int sizeOfEventLogEntry = 12;

		// known size of the each op log entry (as defined in OperationalLog data structure)
		public const int sizeOfOpLogEntry = 24;

		// active faults retrieved is simple stream of UInt16s for each fault ID
		public const long sizeOfActiveFaultEntry = 2;

		public const int eventIDSubObject = 1;
		public const int grouptFilterSubObject = 3;
		public const int ResetLogSubObject = 1;
		public const int ClearToFaultSeverityLevelSubObj = 0;

		// valid event IDs are held as a simple stream of UInt16s for each ID
		public const long sizeOfEventIDEntry = 2;







		#endregion

		#region EVENT_ID_DESCRIPTION sub object definitions
		public const int	EventIDSubObject = 1;
		public const int	EventNameSubObject = 2;
		#endregion

		#region nothing to transmit error (CAN General error message sent when no domain info to transmit)
		public const int NothingToTransmit = 2;
		public const string NothingToTransmitString = "NOTHING TO TRANSMIT";
		#endregion

		#region communications class constant definitions
		/* Communications timeout in milliseconds.  When a CAN message is transmitted, this
		 * is the timeout to wait for a valid reply before giving up and indicating to the 
		 * user a fault. This is the default timeout used for most SDOs unless otherwise
		 * selected by the GUI.
		 */
		public const int			TimeoutDefault = 3000;			// in ms 

		/* Timeout used when resetting the fault log on a Sevcon controller.  This is
		 * because the time to clear a potentially long log from the EEPROM can take
		 * a longer time than standard SDOs.
		 * Actual time taken 14s for a full log of 70 but in case log size increases,
		 * add a bit of margin.
		 */
		public const int			TimeoutLogReset = 20000;	// in ms
		
		/* Timeout used when programming the DSP.  Again, the Sevcon controller take longer 
		 * than the usual SDO to respond due to the time taken to actually program the DSP
		 * record.
		 */
		public const int			TimeoutDSPProgramming = 40000;	// in ms

		/* Listen in period of time in millisends.  Used when trying to auto-detect the Baud rate
		 * used on the CANbus.  DW will listen at each of the CANopen defined baud rates for this
		 * period of time to see if it can detect any valid traffic.
		 */
		public	const	int			TimeoutListenIn = 1000;		// in ms

        // DR38000255 was 10 but misses nodes SDO responses under heavy comms conditions
        internal const int          VCI3TimeOutFindNodeIDs = 500; 
		/* Default SDO server used by DW, according to CANopen specs.  To communicate with a
			 * given node on the CANbus, add the node ID to the default SDO base address.
			 */
		public	const	uint		DefaultSDOBaseAddress = 0x600;

		/* When transmitting a long string of data that will not fit into one SDO messages,
			 * the multiple of CAN SDO messages have one byte of command data followed by
			 * 7 bytes of segment data.  This allows calculation of how many SDOs are needed.
			 */
		public	const   byte		sizeOfSegmentData = 7;

		// COB-ID used for DW to transmit to a controller for all LSS communications
		public const	ushort		LSSCOBID = 0x07e5;
		public const	ushort		LSSResponseCOBID = 0x07e4;
		public const	uint		COBForEmergencyMinimum = 0x80;
		public const	uint		COBForEmergencyMaximum = 0xff;
		public const	int			sizeOfTransmitBuffer = 200;
		public const	int			sizeOfReceiveBuffer  = 2048;
		public const	int			maxSDOBlockSize		 = 127;
		public const	int			MaximumNumberOfNodes = 128;
		public const	int			CANMessageDataLengthMax = 8;
		public const	int			NodeCOBIDToNodeIDOffset = 0x80;
		public const	int			COBIDToNodeIDOffsetHeartbeat = 0x700;
		#endregion

		#region constants definitions for deviceVersion
		/* 
			 * Mandatory OD item definitions for identity object & subs.
			 * Must use this as at this point we do not even know which EDS file
			 * to look at in order to determine the relevant index & sub.
			 */
		public const int identityObjectIndex				= 0x1018;
		public const int vendorIDObjectSubIndex				= 0x01;
		public const int productCodeObjectSubIndex			= 0x02;
		public const int revisionNumberObjectSubIndex		= 0x03;
		public const int sizeOfVendorID						= 4;		// 4 bytes
		public const int sizeOfProductCode					= 4;		// 4 bytes
		public const int sizeOfRevisionNumber				= 4;		// 4 bytes
		#endregion

		#region LSS constant declarations
		public const bool LSSConfigureMode = true;
		public const bool LSSNormalMode = false;
		#endregion

		#region data monitoring constants
		public const int maxDataMonitoringPoints = 10000;
		#endregion

		#region NMT state
		public enum NMTState
		{		
			Stopped = 4,
			Operational = 5,
			PreOperational = 127
		};			
		#endregion

		#region DCF backdoor object subs definition
		public const int	BackDoorSubForIndex = 1;
		public const int	BackDoorSubForSubIndex = 2;
		public const int	BackDoorSubForNewValue = 3;
		public const int	BackDoorSubForKey = 4;
		public const int	BackDoorSubForFeedbackCode = 5;
		#endregion
		#endregion  Sevcon DI constants definitions

		#region Sevcon corporate font property 
		private static Font localfont = new Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(2)));
		public static Font SCfont
		{
			get 
			{
				return localfont; 
			}
		}

		#endregion

		#region Event Group Names
		public static string [] FaultFifoGrpNames  = 
		{"Startup Faults", 
		"Configuration Faults", 
		"Driver Procedure Faults", 
		"Power Frame Faults",
		"Contactor Open Circuit Faults", 
		"Contactor Short Circuit Faults", 
		"Analogue Input Faults", 
		"Battery Faults",
		"Temperature Faults", 
		"Contactor Coil Short Circuit Faults", 
		"Pre-Operational", 
		"Motor faults", 
		"CANbus Faults", 
		"Internal Faults", 
		"Peripheral Faults", 
		"Servicing"};
		public static string [] SystemFifoGrpNames  = {"User", "Software", "Hardware", "System", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>"};
		public static string [] NonFifoGrpNames    = {"<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>"};
		public static string [] SpareFifoGrpNames  = {"<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>", "<Unused>"};
		#endregion Event Group Names

		#region Background and foreground colours
		private static Color localbackColour = System.Drawing.Color.White;

		public	static Color SCBackColour
		{
			get 
			{
				return localbackColour; 
			}
		}

		private static Color localHighLightColour = System.Drawing.Color.LemonChiffon; 
		public	static Color SCHighlightColour
		{
			get 
			{
				return localHighLightColour; 
			}
		}
		#endregion

		#region corporate forecolour
		private static Color localForeColour = System.Drawing.Color.Black;
		public	static Color SCForeColour
		{
			get 
			{
				return localForeColour; 
			}
		}
		#endregion

		#region dataGrids
		public static int activeRow = 10000;
		public static Color dgBackColour = System.Drawing.Color.White;  //same as form colour
		public static Color dgRowBackColour = System.Drawing.Color.White;
		public static Color dgForeColour = System.Drawing.Color.Black;
		public static Color dgblockColour = System.Drawing.Color.LightSteelBlue;
		public static Color dgHeaderColour = System.Drawing.Color.Lavender;
		public	static Color dgRowSelected= System.Drawing.Color.LemonChiffon; 
		//row font colours
		public static Color readOnly = Color.Gray;
		public static Color writeOnly = Color.Crimson;
		public static Color readWrite = Color.MediumBlue;
		public static Color headerRow = Color.Black;
		public static Color headerRowBackcol = Color.Lavender;
		public static Color readOnlyRowBackCol = Color.WhiteSmoke;
		public static Color writeOnlyPlusPreOP = Color.SaddleBrown;
		public static Color readWriteInPreOp = Color.Magenta;
		public static Color buttonBackGround = dgblockColour;
		#endregion

		#region screen dimensions
		//Determine percentage of Desktop that DW occupies
		//the percentage of the client width that is dedicated to the main user area
		private static double _leftPanelPercentage = 0.95; //
		//use a fixed height for hte status bar to ensure that icons/text are legible
		private static int _statusBarHeight = 32;
		private static int _toolBarHeight = 32;
		private static double _borderPaddingPercent = 0.02;

		public static double leftPanelPercentage 
		{
			get 
			{
				return _leftPanelPercentage; 
			}
		}
		public static int statusBarHeight 
		{
			get 
			{
				return _statusBarHeight; 
			}
		}
		public static int toolBarHeight 
		{
			get 
			{
				return _toolBarHeight; 
			}
		}
		public static double borderPaddingPercent 
		{
			get 
			{
				return _borderPaddingPercent; 
			}
		}
		#endregion screen dimensions

		#region accesslevels
		/* To read objects from the controller OD, the user must be logged on to a Sevcon
		 * node.  As long as their access level is greater than zero, they can read the
		 * item.  This is transparent to 3rd party devices (accessLevel set to 255).
		 */
		public const int   ReadAccessLevelMin	= 0x01;
		public static uint AccLevel_SelfChar = 4;
		public static uint AccLevel_Programming = 2;
		public static uint AccLevel_SevconCANbusConfig = 2;
		public static uint AccLevel_SevconPDOWrite = 4;
		public static uint AccLevel_ResetLogs = 2;
		public static uint AccLevel_ResetSevconOpLog = 5;
		//the following item is re-checked when we connect to a system
		public static uint AccLevel_PreOp = 1;
		public static uint AccLevel_VPDOs_Write = 4;
		#endregion accesslevels

		#region Internla PDO Dummy values
		public const int dummyValue_DigIP = 0x21FF;
		public const int dummyValue_AlgIP = 0x22FF;
		public const int dummyValue_Motor = 0x20FF;
		public const int dummyValue_DigOP = 0x23FF;
		public const int dummyValue_AlgOP = 0x24FF;
		#endregion Internla PDO Dummy values

		#region CANOpen SPDO spacer mapping values
		public const long spacer32bit = 0x00070020;
		public const long spacer16bit = 0x00060010;
		public const long spacer8bit = 0x00050008;
		public const long spacer1bit = 0x00010001;
		public const string nonDefinedBitsText = "Bitsplit: non defined bits";
		public const string SaveCommsWarning = "\nWARNING\nCAN Node Eeprom will NOT be updated until you leave the COB screen or exit DriveWizard";
		#endregion CANOpen SPDO spacer mapping values

        //DR38000172
        internal static ushort NumAllowedBlockFails = 10; //How many  block fails before dropping into segmented transfer
		public SCCorpStyle()
		{
			SCCorpStyle.VirtualCANnodes = new ArrayList();
		}
	
	}

	public class SCbaseTableStyle :DataGridTableStyle
	{
		public float [] columnPercents = null;
		public SCbaseTableStyle()
		{
			this.RowHeadersVisible = false;
			this.HeaderForeColor = SCCorpStyle.dgForeColour;
			this.GridLineColor = SCCorpStyle.dgForeColour;
			this.ForeColor = SCCorpStyle.dgForeColour;
			this.HeaderBackColor = SCCorpStyle.dgblockColour;
		}
		public int [] calculateColumnWidths(float availableWidth)
		{
			int [] colWidths = new int[columnPercents.Length];
			float totalWidthInt = 0;
			for (int i=0;i<columnPercents.Length;i++)
			{
				colWidths[i] = System.Convert.ToInt32(availableWidth * (columnPercents[i]));
				totalWidthInt +=colWidths[i];
			}
			//mop up any difference in the last column
			colWidths[colWidths.Length-1] -= (int) (totalWidthInt - availableWidth);
			return colWidths;
		}

		public void adjustRightAlignedColumns()
		{
			foreach(DataGridColumnStyle col in this.GridColumnStyles)
			{
				if(col.Alignment == HorizontalAlignment.Right)
				{
					col.HeaderText += " \t";
				}
			}
		}
	}

	//The following class is a basic copy of the build in Text box column
	//Data Grid column class. The only significant difference is that the Edit method is overriddn to just return
	//The advantage of this class ofr read only columns is that it is clearer to the user thaat the column is read only becaseu
	// no highlighting of the cell occurs - it behaves just like a label
	//DO NOT use this base class for writeable columns
	//Thi sclass is useful where the datagrid must be enabled eg scroll bars needed, some cols writeable
	//But where this specif column needs to be read only
	public class SCbaseRODataGridTextBoxColumn : DataGridTextBoxColumn
	{
		private int columnIndex = 0;
		public SCbaseRODataGridTextBoxColumn(int colNum)
		{
			columnIndex = colNum;
		}
		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			return;
		}
	}

	//The following can be used for user selectable voolean cells providing that the actual 
	//setting of the cell value is handled in a dataGrid Click even thandler since this
	//bypasses the column edit method. 
	public class SCbaseRODataGridBoolColumn : DataGridBoolColumn
	{
		private int columnIndex = 0;
		public bool _readonly = true;  //note this is NOT the same as readOnly passed to the Edit method
		public SCbaseRODataGridBoolColumn(int colNum)
		{
			columnIndex = colNum;
		}
		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			if(_readonly == false)
			{
				object currentValue = this.GetColumnValueAtRow(source,rowNum);
				this.SetColumnValueAtRow(source, rowNum, !((bool) currentValue)); //toggle to sell vlaue
				this.DataGridTableStyle.DataGrid.Invalidate(bounds);  //update() does not work here - use invalidate()
			}
			else  //this cell is not editable -determined by GUI conditions
			{
				return;
			}
		}  
	}
}
