/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.356$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/10/2008 21:47:30$
	$ModDate:23/10/2008 19:01:50$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	The main window is displayed while DW is running. The user can select for it to show 
    either of two types of information:
    1)	a tree-view of connected nodes, allowing the expansion or collapse of each 
        node’s OD as defined by the device’s EDS file and XML file (with descriptions 
        and values displayed on the right hand pane). For Sevcon nodes, additional 
        pop-up windows appear to display specific OD object information in a more 
        visual way (e.g. logs, CANbus setup, PDO mapping).
    2)	a graphical PDO mapping screen, allowing the user to setup individual 
        and system PDO maps along with the associated communications parameters.
    
    A permanently visible status bar at the bottom of the screen provides status 
    information on node connection, user login, and text descriptions of any errors 
    or task progress. A toolbar located under the menu allows easy selection of common 
    tasks.


REFERENCES    

MODIFICATION HISTORY
    $Log:  36763: MAIN_WINDOW.cs 

   Rev 1.356    23/10/2008 21:47:30  ak
 Changes required to produce V2.6 msi


   Rev 1.355    08/10/2008 14:03:22  ak
 TRR COD0013 post-test fixes


   Rev 1.354    29/09/2008 21:11:18  ak
 Updates for CRR COD0013, ready for testing


   Rev 1.353    09/07/2008 22:18:40  ak
 V2.4 label; additional checks to prevent exceptions; remove "Logs" tree node
 if no login; all objects with a SEVCON SECTION which isn't in XML tree is
 re-assigned to "Unassigned" for display; DCF compare fixed; pre-fill PDO map
 modifications; pre-fil maps written to controller by calling
 updatePDOMappings(); additional bit split handling; manual reading of display
 NMT state for PDO screen as not updated by heartbeats; COB data auto-saved
 when new currCOB selected; PDODataBindings() modified to update when PDOs
 added/deleted to prevent exceptions


   Rev 1.352    22/04/2008 10:18:06  ak
 Version string updated to reflect generic test.


   Rev 1.351    08/04/2008 21:22:04  ak
 CONFIG_CHECKSUM is re-read each time the device panel is shown.


   Rev 1.350    17/03/2008 13:13:54  jw
 Logs null obejct handling complete. Still needs progrees bar steps linking to
 odSub timeout values


   Rev 1.349    14/03/2008 10:58:50  jw
 Log handling revised, Read of Log releated CAN items now done from GUI ( to
 allow later DI /Ixxat unlinking) . Processing of recevied logs code and event
 lists moved to sysInfo - reduces memory usage ( one method per application
 rather than per node)  Some newer Sevocn devices do not hav ethese logs. Also
 event list methods now have product code input - to allow us to have seperate
 Event lists for eg displays etc. Note:some error hanlding eg null detection
 still needed but check back in for working set with DI 


   Rev 1.348    13/03/2008 08:47:30  jw
 Reset log method  replaced by single ODWrite calls for simplicitiy andto
 allow progrees bar update to relate to odSub timeout for better feedback.


   Rev 1.347    12/03/2008 13:00:02  ak
 All DI Thread.Priority increased to Normal from BelowNormal (needed to run
 VCI3).


   Rev 1.346    28/02/2008 23:57:36  ak
 DR38000165: F5 refresh implementation


   Rev 1.345    26/02/2008 11:52:06  jw
 Define added for back compatibility- Office 2007 installation replaces the
 Excel dll with new file - some function calls are affected and new namespace
 path is required.


   Rev 1.344    21/02/2008 09:22:14  jw
 Regions added for clarity


   Rev 1.343    18/02/2008 15:39:46  jw
 Merge recovery form hibernation code into single method for VCI2 back
 compatibility. Imporved hibernation recovery ( VCI2 works OK, VCI3 -
 intiSocket throwing exception still)


   Rev 1.342    18/02/2008 14:22:14  jw
 VCI2 CAN methods renamed and merged for consistency with VCI3 - towards back
 compatibility


   Rev 1.341    18/02/2008 07:48:40  jw
 Static params changed to non-static on VCI for conformity with VCI2. VCI2
 badurate param renamed to be as per VCI3 - step towards full backwards
 compatibility


   Rev 1.340    15/02/2008 11:45:46  jw
 Params for CAN adapter hardware intialised an drunning changed to same ones
 used for VCI3 - step towards full backwards compatibility
 Reduncadnt code line commented out to reduce compiler warnigns


   Rev 1.339    14/02/2008 12:15:22  jw
 Code put back in to ensure sinlge instance application


   Rev 1.338    14/02/2008 09:31:48  jw
 Ensure that flashing icon updates


   Rev 1.337    13/02/2008 15:01:08  jw
 Redundant using removed


   Rev 1.336    12/02/2008 08:51:08  jw
 Ongoing VCI3 work. Options and Select profiel windows changed to simplify
 threading and improve feedback.  Prog bar vlaue determination line made
 exception proof. Max and current values used by progress bars determined
 within DI for encapsulation and values reflect activitiy better.


   Rev 1.335    07/02/2008 20:51:48  ak
 Same as V1.334 [updateDevicePanel() correction missed]


   Rev 1.334    04/02/2008 21:58:16  ak
 Tests with a display caused an exception (due to cap volt, battery volt &
 temperature subs being null), caused by the threading added to V1.333.


   Rev 1.333    31/01/2008 22:30:36  ak
 updateDevicePanel, readDataForDevicePanel, handleTreeNodeSelection &
 submitBtn_Click now threaded. Improved performance when comms failed.


   Rev 1.332    25/01/2008 10:47:12  jw
 VCI3 and Vista functionality files merge - more testing required


   Rev 1.331    18-01-2008 10:44:54  jw
 DR000235 Remove DW support for bitstrings. ConvertToFloat ( inc remove
 redundant input parameter)  and ConverToDouble modified


   Rev 1.330    14/01/2008 21:08:48  ak
 Bug fixes: progress bar in updateRowColoursArray removed, context menus "stop
 monitoring item" and "remove from DCF file" put back in, changes so that
 items are hooked under the tree views correctly & updated OK for DCF store
 and monitor store.


   Rev 1.329    19/12/2007 23:28:54  ak
 Row colours refreshed when a column in the data grid is sorted.


   Rev 1.328    12/12/2007 21:16:12  ak
 After a show/hide RO click, the selected node is re-found (or as close as
 possible if the node is removed) after the new tree view is displayed.
 Progress bar cross threading fix put in.
 Search facility now opens child windows only if "force read.." is checked.


   Rev 1.327    05/12/2007 22:12:46  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using DW_Ixxat;
using IWshRuntimeLibrary;
#if USING_OFFICE2007
using Microsoft.Office.Interop;
#endif

using Ixxat.Vci3;           //IXXAT conversion Jude
using Ixxat.Vci3.Bal;       //IXXAT conversion Jude
using Ixxat.Vci3.Bal.Can;   //IXXAT conversion Jude

namespace DriveWizard
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	/// 
	public class MAIN_WINDOW : System.Windows.Forms.Form
	{
		#region Component declarations
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem file_mi;
		private System.Windows.Forms.MenuItem main_hlp_mi;
		private System.Windows.Forms.MenuItem file_exit_mi;
		private System.Windows.Forms.MenuItem topics_hlp_mi;
		private System.Windows.Forms.MenuItem about_hlp_mi;
		private System.Windows.Forms.MenuItem update_hlp_mi;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Panel SystemPDOsPanel;
		private System.Windows.Forms.PictureBox PBRequestPreOP;
		private System.Windows.Forms.Splitter splitter2;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ComboBox CB_SPDO_priority;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.TextBox TB_SPDO_COBID;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.ComboBox CB_SPDOName;
		private System.Windows.Forms.Panel MainLHPanel;
		private System.Windows.Forms.NumericUpDown NUD_InhibitITme;
		private System.Windows.Forms.RadioButton RB_SyncRTR;
		private System.Windows.Forms.RadioButton RB_SyncAcyclic;
		private System.Windows.Forms.RadioButton RB_SyncCyclic;
		private System.Windows.Forms.RadioButton RB_AsynchNormal;
		private System.Windows.Forms.RadioButton RB_AsynchRTR;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.NumericUpDown NUD_TxNumSyncs;
		private System.Windows.Forms.NumericUpDown NUDEventTime;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label Lbl_COBInvalidTxTpye;
		private System.Windows.Forms.GroupBox GB_COBTxTriggers;
		private System.Windows.Forms.GroupBox GB_SyncTimings;
		private System.Windows.Forms.GroupBox GB_InhibitTime;
		private System.Windows.Forms.GroupBox GB_EventTIme;
		private System.Windows.Forms.GroupBox GB_AsynchTypes;
		private System.Windows.Forms.GroupBox GB_SyncTypes;
		private System.Windows.Forms.Panel Pnl_COBMappings;
		private System.Windows.Forms.Panel Pnl_COBTxConfig;
		private System.Windows.Forms.ToolTip toolTip2;
		private System.Windows.Forms.Splitter Splitter_PDORouteToMappings;
		private System.Windows.Forms.PictureBox PBRequestOperational;
		private System.Windows.Forms.PictureBox PBGraphing;
		private System.Windows.Forms.PictureBox PBHideShowRO;
		private System.Windows.Forms.PictureBox PBExpandTNode;
		private System.Windows.Forms.PictureBox PBCollapseTNodes;
		private System.Windows.Forms.ContextMenu CMenuGraphingIcon;
		private System.Windows.Forms.Label readOnlyLabel;
		private System.Windows.Forms.Label writeOnlyLabel;
		private System.Windows.Forms.Label readWriteLabel;
		private System.Windows.Forms.Label readwritepreopLabel;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Timer timer3;
		private System.Windows.Forms.Timer dataMonitoringTimer;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.StatusBarPanel statusBarPanel2;
		private System.Windows.Forms.StatusBarPanel statusBarPanel1;
		private System.Windows.Forms.Panel DeviceStatusPanel;
		private System.Windows.Forms.GroupBox DevSevconGB;
		private System.Windows.Forms.Label BattVoltLbl;
		private System.Windows.Forms.Label temperatureLbl;
		private System.Windows.Forms.GroupBox faultDtatusGB;
		private System.Windows.Forms.GroupBox devCANGB;
		private System.Windows.Forms.Label loginLbl;
		private System.Windows.Forms.GroupBox newDCFIdentityGB;
		private System.Windows.Forms.ComboBox revNoCB3;
		private System.Windows.Forms.ComboBox productCodeCB;
		private System.Windows.Forms.GroupBox DCFSourceGB;
		private System.Windows.Forms.Label vendorNameLbl;
		private System.Windows.Forms.Label NMTStateLbl;
		private System.Windows.Forms.Label MasterSlaveLbl;
		private System.Windows.Forms.Label serviceDueLbl;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Label CapVoltLbl;
		private System.Windows.Forms.Label label32;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.ImageList anim_icons;
		private System.Windows.Forms.MenuItem ToolsMenu;
		private System.Windows.Forms.StatusBarPanel statusBarPanel3;
		private System.Timers.Timer timer2;
		private System.Windows.Forms.MenuItem autoValidate;
		private System.Windows.Forms.MenuItem autoValidateMI;
		private System.Windows.Forms.MenuItem OptionsMI;
		private System.Windows.Forms.DataGrid devEmerDG;
		private System.Windows.Forms.DataGrid actFaultsDG;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.Timer splashTimer;
		private System.Windows.Forms.Panel DCFStatusPanel;
		private System.Windows.Forms.Label DCFRevNoLbl;
		private System.Windows.Forms.Label DCFVendorIDLbl;
		private System.Windows.Forms.Label DCFProductCodeLbl;
		private System.Windows.Forms.Label DCFFileLastModLbl;
		private System.Windows.Forms.Label DCFFileNameLbl;
		private System.Windows.Forms.Label DCFDeviceNameLbl;
		private System.Windows.Forms.Label DCFVendorNameLbl;
		private System.Windows.Forms.Label DCFCreatedByLbl;
		private System.Windows.Forms.GroupBox DCFfileInfoGB;
		private System.Windows.Forms.Label UserInstructionsLabel;
		private System.Windows.Forms.GroupBox DeviceBuildGB;
		private System.Windows.Forms.Label ExtROMChksumLbl;
		private System.Windows.Forms.Label IntROMChksumLbl;
		private System.Windows.Forms.Label serialNoLbl;
		private System.Windows.Forms.Label RevNumberLbl;
		private System.Windows.Forms.Label VendorIDLbl;
		private System.Windows.Forms.Label productCodeLbl;
		private System.Windows.Forms.Label SWVrLbl;
		private System.Windows.Forms.Label HWVerLbl;
		private System.Windows.Forms.Panel blankPanel;
		private System.Windows.Forms.Panel dataGridPanel;
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.Panel keyPanel;
		private System.Windows.Forms.Panel writeInPreOpPanel;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Panel panel7;
		private System.Windows.Forms.Panel readWriteInPreopPanel;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Panel panel9;
		private System.Windows.Forms.Label writeInPreOpLabel;
		private System.Windows.Forms.Panel writeOnlyPanel;
		private System.Windows.Forms.Panel readOnlyPanel;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.Panel readWritePanel;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Panel panel14;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.Panel systemStatusPanel;
		private System.Windows.Forms.GroupBox systemCANopenGB;
		private System.Windows.Forms.DataGrid emergencyDG;
		private System.Windows.Forms.DataGrid connectedDevicesDG;
		private System.Windows.Forms.Panel PDORoutingPanel;
		private System.Windows.Forms.Panel mainRHPanel;
		#endregion

		#region DriveWizard: Main Window declarations

		#region statics
		static internal bool appendErrorInfo = true;
		#endregion statics
		myMonitorStore monStore;
		internal static ArrayList masterEDSSections = new ArrayList();

        /// <summary>When SearchForm is open, this retains the number of matches found 
        /// during searches of the previous nodes on Treeview1.</summary>
        private int searchOtherNodesFoundTotal = 0;
        /// <summary>When SearchForm is open, this retains the index to the current node 
        /// in Treeview1 which is to be searched.</summary>
        private int searchTNIndex = 0;
        /// <summary>True when user initiated search is in progress.</summary>
        private bool searchInProgress = false;
		#region Software versions, web Address etc
		internal static string DW_Version = "V2";//format #### numeric
		internal static string DW_Release = "6";//format #### numeric2
		private string VersionDescription = "Alpha release";      //"Alpha release" for proper release
		internal static string SC_website = "http://www.sevcon.com/Pages/contacts.html";
		/// <summary>
		/// Application level config options object
		/// </summary>
		internal static DWConfig DWConfigFile = null;
		#endregion Software versions, web Address etc
		#region target PC directory paths
		internal static string ApplicationDirectoryPath = "";
		internal static string UserDirectoryPath  = "";
		#endregion target PC directory paths
		#region Data storage and display of CAN node OD contents
		internal static  DataSet DWdataset, ActFaultsDS;
		EstCommsTable connDevTable;
		private DataTable comparisonTable;
		private PPTableStyle tablestyle = null;
		internal static System.Drawing.Color [] colArray;
		internal static bool [] canWriteNow;
		internal static System.Drawing.Color [,] compColArray;
		actFualtsTableStyle devActFaultstablestyle;
		/// <summary>
		/// Jagged array containg the grid column widths expressed as percentages 
		/// There is one of these column widths array for each user access level (1 to 5) 
		/// </summary>
		private float [][] AccessLevels_GridColWidths;
		/// <summary>
		/// determines column widths for the main dataGrid
		/// </summary>
		internal static int currTblIndex = 0;
		private ArrayList childTreeNodeList;
		internal static int monitorCeiling = 10;  //spec was for four
		float [] connPercents = {0.25F, 0.35F, 0.1F, 0.15F, 0.15F};
		float [] devActFaultsPercents = {1F};
		int actFaultsDGDefaultHeight = 0;
		int connectedDevicesDGDefaultHeight = 0;
		int dataGrid1DefaultHeight = 0;
		int devEmerDGDefaultHeight = 0;
		int emergencyDGDefaultHeight = 0;
		#endregion Data storage and display of CAN node OD contents
		#region forms
        /// <summary>Hand to form when an instance of SearchForm is created (find item in treeview window).</summary>
        private Form SEARCH_FRM = null;
        
		private Form helpAbout = null;
		private Form helpContact = null;
		private Form DATA_MONITOR_FRM = null;
		private Form FAULT_LOG_FRM = null;
		private Form SYSTEM_LOG_FRM = null;
		private Form OP_LOGS_FRM = null;
		private Form COUNTERS_FRM = null;
		private Form SELF_CHAR_FRM = null;
		private Form CAN_BUS_CONFIG = null;
		private Form COB_PDO_FRM = null;
		private Form PROG_DEVICE_FRM = null;
		private Form options = null;
		private Form selectProfile = null;
		private Form splashscreen = null;
		#endregion forms
		#region Baud rates, User IDs, selected CAN node num and text
//		internal static ushort duplicateNodeIDNo = 0;
		internal static string [] baudrates = {"1 MHz", "800 kHz", "500 kHz", "250 kHz", "125 kHz", "50 kHz", "20 kHz", "10 kHz", "100 kHz", "autodetect baud"};
		internal static string [] UserIDs = {"No Access","User", "Customer Service", "Customer Production" , "Customer Engineer", "Sevcon Engineer"};
		#endregion Baud rates, User IDs, selected CAN node num and text
		#region switch for allowing user to work with virtual system
		internal static bool isVirtualNodes = true;
        internal static bool showMasterObjectsOnSlave = false;  //DR38000263
		#endregion switch for allowing user to work wit hvirtual system
		#region treeview drag and drop parameters
		private Graphics gTV = null;
		private Font mainWindowFont = new Font( "Arial", 8F);
		private string shadowText = "";
		private Size dragStrSize;
		private Rectangle removeDragStringRectTV = new Rectangle(0,0,0,0);
		#endregion treeview drag and drop parameters
		#region DI related parameters
		DriveWizard.SystemInfo sysInfo;
		/// <summary>
		/// Used where the feedback is created one one thread and handled on another
		/// </summary>
		private DIFeedbackCode globalfeedback;
		private int sectiontype = (int)SevconSectionType.NONE;
		private CANSectionType ThirdPartySection = CANSectionType.NONE;
		#endregion DI related parameters
		#region Named TreeNodes for other windows
		//device level treenodes
		private ArrayList FaultLogTN = new ArrayList();
		private ArrayList SystemLogTN = new ArrayList();
		private ArrayList OpLogTN =new ArrayList();
		private ArrayList CountersLogTN = new ArrayList();
		private ArrayList LogsTN = new ArrayList();
		string SelfCharMotor1Text = "Self Char. (motor 1)", 
			SelfCharMotor2Text = "Self Char. (motor 2)", 
			programmingTNText= "Re-Program Device", 
			FaultLogTNText= "Fault Log", 
			SystemLogTNText= "System Log",
			OpLogTNText= "Operational Log", 
			CountersLogTNText= "Event Log",
			LogsTNText= "Logs", DataLogTNText = "Data Log"; 
		string nodeCOBShortCutsText = "Comms objects setup wizard";
		//system level treenodes
		private TreeNode COBandPDOsTN = null;
		private TreeNode CANBUsConfigTN = null, SystemStatusTN = null;
		private TreeNode GraphCustList_TN, DCFCustList_TN;
		private TreeNode GraphBackupTN, SystemBackupTN, DCFBackUpTN;
        private treeNodeTag nodeTag = null;
		#endregion Named TreeNodes for other windows
		#region Threads
		private Thread	restartCANThread;
		private Thread DCFfileOpenThread;
		private Thread DCFdownloadToDeviceThread;
		private Thread DCFFileSaveThread;
		private Thread dataRetrievalThread;//, sectionRetrievalThread;

		#endregion Threads
		#region DCF related
		#endregion DCf related
		#region Emergency messages releated
		ArrayList emerAL;
		EmergencyTable emerTable;
		float [] emerPercents = {0.2F, 0.8F};
		float [] devEmerPercents = {1F};
		#endregion Emergency messages releated
		ushort noOfCustomLists = 2;  //currently just Graphing and DCF 
        /// <summary>Used for the progress bar value update for DCF & mon store to avoid cross threading.</summary>
        private int nodeValue;
        private int progressBarValueThreaded;

		#region System State flags
		bool [] nodeStateChangedFlags = new bool[8];
		bool nodeStateChanged = false;
		internal static bool findingSystem = false;
		internal static bool reEstablishCommsRequired = false;
		internal static bool mainWindowClosing = false;
		internal static bool UserInputInhibit = false;
		private bool timer3WasEnabled = false;
		#endregion System State flags
		#region Treeview navigation and filtering parameters
		TreeNode previousNode = null, nextRequiredNode = null;
		TreeNode currentTreenode = null;
		int treeNodeCountInLeg = 0;
		#region treeview scroll bar Right justify override
		private const int WM_HSCROLL = 276;
		private const int WM_VSCROLL = 277;
		private System.Windows.Forms.ToolBarButton tbb_Evas;
		private System.Windows.Forms.PictureBox pbEvas;
        /// <summary>Picture box used for the search/find item in treeview toolbar button.</summary>
        private System.Windows.Forms.PictureBox pbSearch;
		private System.Windows.Forms.ToolBarButton spacer1;
		private System.Windows.Forms.ToolBarButton spacer2;
		private System.Windows.Forms.ToolBarButton spacer3;
		private System.Windows.Forms.ToolBarButton spacer5;
		private System.Windows.Forms.ToolBarButton tbb_PreOPRequest;
		private System.Windows.Forms.ToolBarButton tbb_OpRequest;
		private System.Windows.Forms.ToolBarButton tbb_Graphing;
		private System.Windows.Forms.ToolBarButton tbb_HideShowRO;
		private System.Windows.Forms.ToolBarButton tbb_ExpandTnodes;
		private System.Windows.Forms.ToolBarButton tbb_collapseTNodes;
		private System.Windows.Forms.Label Lbl_productCode;
		private System.Windows.Forms.Label Lbl_RevisionNo;
		private System.Windows.Forms.Label configChkSumLbl;
        private System.Windows.Forms.ToolBarButton spacer4;
        /// <summary>Toolbar button used for the search/find item in treeview.</summary>
        private System.Windows.Forms.ToolBarButton tbb_SearchTree;
        /// <summary>Menu equivalent of SearchTree toolbar button (allows Ctrl-F shortcut).</summary>
        private MenuItem FindMI;
        private MenuItem miRefreshData;
        private ToolBarButton tbb_RefreshData;
        private PictureBox pbRefreshData;

		/// <summary>
		/// Used by SendMessage()
		/// </summary>
		private const int SB_LEFT = 6;
		[DllImport("user32.dll")]
		private static extern int SendMessage(IntPtr hWnd, int wMsg, 
			int wParam, int lParam);
		#endregion treeview scroll bar Right justify override
		private bool nodesRemovedFlag = false;
		private bool firstNodeSelectionAfterConnection = false;

        //DR38000269 single list of all unknown faultID text strings maintained
        public ArrayList unknownFaultIDs = new ArrayList(); 
		#endregion Treeview navigation and filtering parameters

		private struct displayChannels
		{
			private ushort _nodeID;
			internal ushort nodeID
			{
				get 
				{
					return _nodeID;
				}
				set
				{
					_nodeID = value;
				}
			}
			internal string nodeName;
		}

        /// <summary>
        /// Used to hold the complete tree node path of the currently selected node as a string.
        /// </summary>
        private ArrayList HideShowRONodeFullPath;

		bool splashRequired = true;
		int UsrLevelIndex = 0;
		TreeNode tempTreeNode; //made global to allow use in treaded method - ensureAllItemsInCustListHaveActValue
		internal static int DCFTblIndex = 99, GraphTblIndex = 99, compTblIndex = 99;
		/// <summary>
		/// Contains the 0x1018 information about the source (file or connected device) data in the DCF TreeNode
		/// </summary>
		#region DCF related
		private bool DCFFromfile = false;
		private int DCFSourceTableIndex = 0, DCFDestinationTableIndex = 0;
		#endregion DCF related
		/// <summary>
		/// arbitrary out of range OD Index - marks TreeNode as a SevconSection or CANopen section
		/// </summary>
		private int selectedDgRowInd = 0;
		private bool monitorTimerWasRunning = false;
		private string statusBarOverrideText = "";
		#region DCF Comparison parameters
		internal static bool DCFCompareActive = false;
		int [] compTableIndexes;
		private ArrayList compNodeIDs;
		private ArrayList devicesWithEDS;
		int DCFcompProgBarCounter = 0;
		private DCFCompareTableStyle compTableStyle = null;
		#region column width percentages
		private float [][] DataGridColWidths;
		#endregion column width percentages
		#endregion DCF Comparison parameters
		#region self char related
		int [,] motorProfilesPresent;
		#endregion self char related
		#region device status panel params
		string [] HWVersions, SWVersions, serviceDue, productCodes, RevNos, VendorIDs, VendorNames, serialNos, configChkSum, IntROMChksum, extROMChksum;
		#endregion device status panel params
		int [,] FaultLEDFlashRate;
		int tempTableIndex, tempDestTableIndex; //made global to allow use in treaded method - ensureAllItemsInCustListHaveActValue
		bool expandFlag = false;
		internal static ArrayList availableEDSInfo = null;
		internal static VehicleProfile currentProfile = null;
		int numConsecutiveNonResponsesWhenMonitoring = 0;
		#region PDO parameters
		//		private int currPDONodeIndex = -1;
		private Point [] genericHorizArrowPts,  genericVertArrowPts;
		private bool SystemPDOsScreenActive = false;
		private nodeInfo currTxPDOnode =null, currRxPDOnode = null;
		/// <summary>
		/// references to the screen panles representing CAN nodes for Tx'ing PDOs
		/// </summary>
		private ArrayList TxPDONodePanels;
		/// <summary>
		/// references to the screen panles representing CAN nodes for Rx'ing PDOs
		/// </summary>
		private ArrayList RxPDONodePanels;
		private ArrayList PDOableTreeNodes;
		private COBObject currCOB = null;
		/// <summary>
		/// list of system COBs which contain the active Internal PDO mapping value
		/// </summary>
		private ArrayList SysPDOsToHighLight = new ArrayList();
		/// <summary>
		/// Set to null unless focus is on an internla mapping eg user clicked on or dragged something to an intenal PDO
		/// </summary>
		private PDOMapping activeIntPDO = null;
		private ArrayList PDOableCANNodes;
		private Point routeStartPt = new Point(-1,-1);
		/// <summary>
		/// a temporary COBObject created after user selects TxNode
		/// </summary>
		private COBObject  PDOUnderConstruction = null;
		private Point PDOMousePos;  //set and updated in mousemove event handler
		private COBObject.PDOMapData tempTxData;
		private COBObject.PDOMapData tempRxData;
		private int tempTxScrIndex;
		private BindingManagerBase managerCB;
		private DriveWizard.sixtyFourBitsAsPanel currMapPnl;
		private int mapPnlIndex = 0;
		private int mappingIndex = 0;
		/// <summary>
		/// Referneces to the screen panles rperesenting VPDO I/O interfaces to real world
		/// Order is fixed as Dig I/Ps, Alg I/Ps, Motor, Dig O/Ps, Alg O/Ps
		/// </summary>
		private ArrayList VPDO_GBs;
		/// <summary>
		/// References to the two panels that intertface between the System PDO routing and VPDO expansion graphics
		/// </summary>
		private ArrayList InterfacePanels;
		/// <summary>
		/// Skeleton panel to test functionality - the actual dat aot be dispalyed is still TODO
		/// </summary>
		private ArrayList txSysPDOExpansionPnls = new ArrayList();
		private ArrayList rxSysPDOExpansionPnls = new ArrayList();
		private int PDOVertSpacer = 8;
		private int nodeIDOfPDORouteLegToRemove = -1;
		private bool wasHidingNodesBeforePDOScreenActive;

		/// <summary>
		/// distinguishes between user changing inhibit time and switching activeCOB
		/// </summary>
		//		private TreeNode TN_VPDODigIPs, TN_VPDOsAlgIPs, TN_VPDOsMotor, TN_VPDOsDigOPs, TN_VPDOsAlgOPs;
		private SevconObjectType activeVPDOType = SevconObjectType.NONE;
		private Label Lbl_TxHeader = new Label();
		private Label Lbl_RxHeader = new Label();
		private nodeInfo SevconMasterNode = null;
		/// <summary>
		/// the index of the VPDO list bx item clicked on for context menu - activeIntPDO is not reliable
		/// </summary>
		private int VPDOListBoxItemIndex = -1;  
		#endregion PDO parameters

		#region dataGrid Column percentage widths for each User level 
		float [] ColPercentsLvl5 = {0.39F, 0.1F, 0.07F, 0.07F, 0.06F, 0.1F, 0.1F, 0.1F};
		float [] ColPercentsLvl4 = {0.5F, 0.1F, 0.15F, 0.15F, 0.1F};
		float [] ColPercentsLvl3 = {0.5F, 0.1F, 0.15F, 0.15F, 0.1F};
		float [] ColPercentsLvl2 = {0.4F, 0.3F, 0.3F};
		float [] ColPercentsLvl1 = {0.4F, 0.3F, 0.3F};
		#endregion dataGrid Column percentage widths for each User level 

		#region help system
		// Import the MS HTML Help function defined in hhctrl.ocx
		[System.Runtime.InteropServices.DllImport("hhctrl.ocx")]
		static extern bool HtmlHelp(IntPtr handle, string file, int command, int data);

		// HTML Help Command to open a text only popup
		//internal const int HH_HELP_CONTEXT = 15;
		// HTML Help Command to display the table of contents
		internal const int HH_DISPLAY_TOC = 1;
		#endregion
		#endregion DriveWizard: Main Window declarations

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MAIN_WINDOW));
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.ToolsMenu = new System.Windows.Forms.MenuItem();
            this.OptionsMI = new System.Windows.Forms.MenuItem();
            this.FindMI = new System.Windows.Forms.MenuItem();
            this.miRefreshData = new System.Windows.Forms.MenuItem();
            this.main_hlp_mi = new System.Windows.Forms.MenuItem();
            this.topics_hlp_mi = new System.Windows.Forms.MenuItem();
            this.about_hlp_mi = new System.Windows.Forms.MenuItem();
            this.update_hlp_mi = new System.Windows.Forms.MenuItem();
            this.file_mi = new System.Windows.Forms.MenuItem();
            this.file_exit_mi = new System.Windows.Forms.MenuItem();
            this.autoValidateMI = new System.Windows.Forms.MenuItem();
            this.autoValidate = new System.Windows.Forms.MenuItem();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.statusBarPanel2 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanel1 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanel3 = new System.Windows.Forms.StatusBarPanel();
            this.timer2 = new System.Timers.Timer();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.PBRequestOperational = new System.Windows.Forms.PictureBox();
            this.PBRequestPreOP = new System.Windows.Forms.PictureBox();
            this.PBGraphing = new System.Windows.Forms.PictureBox();
            this.CMenuGraphingIcon = new System.Windows.Forms.ContextMenu();
            this.PBHideShowRO = new System.Windows.Forms.PictureBox();
            this.PBExpandTNode = new System.Windows.Forms.PictureBox();
            this.PBCollapseTNodes = new System.Windows.Forms.PictureBox();
            this.pbEvas = new System.Windows.Forms.PictureBox();
            this.pbSearch = new System.Windows.Forms.PictureBox();
            this.toolBar1 = new System.Windows.Forms.ToolBar();
            this.tbb_PreOPRequest = new System.Windows.Forms.ToolBarButton();
            this.tbb_OpRequest = new System.Windows.Forms.ToolBarButton();
            this.spacer1 = new System.Windows.Forms.ToolBarButton();
            this.tbb_Graphing = new System.Windows.Forms.ToolBarButton();
            this.spacer2 = new System.Windows.Forms.ToolBarButton();
            this.tbb_HideShowRO = new System.Windows.Forms.ToolBarButton();
            this.spacer3 = new System.Windows.Forms.ToolBarButton();
            this.tbb_ExpandTnodes = new System.Windows.Forms.ToolBarButton();
            this.tbb_collapseTNodes = new System.Windows.Forms.ToolBarButton();
            this.spacer5 = new System.Windows.Forms.ToolBarButton();
            this.tbb_Evas = new System.Windows.Forms.ToolBarButton();
            this.spacer4 = new System.Windows.Forms.ToolBarButton();
            this.tbb_SearchTree = new System.Windows.Forms.ToolBarButton();
            this.tbb_RefreshData = new System.Windows.Forms.ToolBarButton();
            this.anim_icons = new System.Windows.Forms.ImageList(this.components);
            this.label9 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.mainRHPanel = new System.Windows.Forms.Panel();
            this.DeviceStatusPanel = new System.Windows.Forms.Panel();
            this.DeviceBuildGB = new System.Windows.Forms.GroupBox();
            this.vendorNameLbl = new System.Windows.Forms.Label();
            this.ExtROMChksumLbl = new System.Windows.Forms.Label();
            this.IntROMChksumLbl = new System.Windows.Forms.Label();
            this.configChkSumLbl = new System.Windows.Forms.Label();
            this.serialNoLbl = new System.Windows.Forms.Label();
            this.RevNumberLbl = new System.Windows.Forms.Label();
            this.VendorIDLbl = new System.Windows.Forms.Label();
            this.productCodeLbl = new System.Windows.Forms.Label();
            this.SWVrLbl = new System.Windows.Forms.Label();
            this.HWVerLbl = new System.Windows.Forms.Label();
            this.DevSevconGB = new System.Windows.Forms.GroupBox();
            this.CapVoltLbl = new System.Windows.Forms.Label();
            this.BattVoltLbl = new System.Windows.Forms.Label();
            this.temperatureLbl = new System.Windows.Forms.Label();
            this.faultDtatusGB = new System.Windows.Forms.GroupBox();
            this.actFaultsDG = new System.Windows.Forms.DataGrid();
            this.devEmerDG = new System.Windows.Forms.DataGrid();
            this.label32 = new System.Windows.Forms.Label();
            this.serviceDueLbl = new System.Windows.Forms.Label();
            this.devCANGB = new System.Windows.Forms.GroupBox();
            this.loginLbl = new System.Windows.Forms.Label();
            this.NMTStateLbl = new System.Windows.Forms.Label();
            this.MasterSlaveLbl = new System.Windows.Forms.Label();
            this.UserInstructionsLabel = new System.Windows.Forms.Label();
            this.SystemPDOsPanel = new System.Windows.Forms.Panel();
            this.Splitter_PDORouteToMappings = new System.Windows.Forms.Splitter();
            this.Pnl_COBMappings = new System.Windows.Forms.Panel();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.Pnl_COBTxConfig = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.CB_SPDO_priority = new System.Windows.Forms.ComboBox();
            this.GB_COBTxTriggers = new System.Windows.Forms.GroupBox();
            this.GB_InhibitTime = new System.Windows.Forms.GroupBox();
            this.NUD_InhibitITme = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.GB_EventTIme = new System.Windows.Forms.GroupBox();
            this.NUDEventTime = new System.Windows.Forms.NumericUpDown();
            this.label13 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.GB_AsynchTypes = new System.Windows.Forms.GroupBox();
            this.RB_AsynchRTR = new System.Windows.Forms.RadioButton();
            this.RB_AsynchNormal = new System.Windows.Forms.RadioButton();
            this.GB_SyncTypes = new System.Windows.Forms.GroupBox();
            this.RB_SyncRTR = new System.Windows.Forms.RadioButton();
            this.RB_SyncCyclic = new System.Windows.Forms.RadioButton();
            this.RB_SyncAcyclic = new System.Windows.Forms.RadioButton();
            this.GB_SyncTimings = new System.Windows.Forms.GroupBox();
            this.Lbl_COBInvalidTxTpye = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.NUD_TxNumSyncs = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.TB_SPDO_COBID = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.CB_SPDOName = new System.Windows.Forms.ComboBox();
            this.PDORoutingPanel = new System.Windows.Forms.Panel();
            this.systemStatusPanel = new System.Windows.Forms.Panel();
            this.systemCANopenGB = new System.Windows.Forms.GroupBox();
            this.emergencyDG = new System.Windows.Forms.DataGrid();
            this.connectedDevicesDG = new System.Windows.Forms.DataGrid();
            this.blankPanel = new System.Windows.Forms.Panel();
            this.DCFStatusPanel = new System.Windows.Forms.Panel();
            this.newDCFIdentityGB = new System.Windows.Forms.GroupBox();
            this.Lbl_RevisionNo = new System.Windows.Forms.Label();
            this.Lbl_productCode = new System.Windows.Forms.Label();
            this.revNoCB3 = new System.Windows.Forms.ComboBox();
            this.productCodeCB = new System.Windows.Forms.ComboBox();
            this.DCFfileInfoGB = new System.Windows.Forms.GroupBox();
            this.DCFCreatedByLbl = new System.Windows.Forms.Label();
            this.DCFFileNameLbl = new System.Windows.Forms.Label();
            this.DCFFileLastModLbl = new System.Windows.Forms.Label();
            this.DCFSourceGB = new System.Windows.Forms.GroupBox();
            this.DCFVendorNameLbl = new System.Windows.Forms.Label();
            this.DCFDeviceNameLbl = new System.Windows.Forms.Label();
            this.DCFRevNoLbl = new System.Windows.Forms.Label();
            this.DCFVendorIDLbl = new System.Windows.Forms.Label();
            this.DCFProductCodeLbl = new System.Windows.Forms.Label();
            this.dataGridPanel = new System.Windows.Forms.Panel();
            this.submitBtn = new System.Windows.Forms.Button();
            this.keyPanel = new System.Windows.Forms.Panel();
            this.writeInPreOpPanel = new System.Windows.Forms.Panel();
            this.label12 = new System.Windows.Forms.Label();
            this.panel7 = new System.Windows.Forms.Panel();
            this.readwritepreopLabel = new System.Windows.Forms.Label();
            this.readWriteInPreopPanel = new System.Windows.Forms.Panel();
            this.label14 = new System.Windows.Forms.Label();
            this.panel9 = new System.Windows.Forms.Panel();
            this.writeInPreOpLabel = new System.Windows.Forms.Label();
            this.writeOnlyPanel = new System.Windows.Forms.Panel();
            this.readOnlyPanel = new System.Windows.Forms.Panel();
            this.readWriteLabel = new System.Windows.Forms.Label();
            this.writeOnlyLabel = new System.Windows.Forms.Label();
            this.readOnlyLabel = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.readWritePanel = new System.Windows.Forms.Panel();
            this.label21 = new System.Windows.Forms.Label();
            this.panel14 = new System.Windows.Forms.Panel();
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.MainLHPanel = new System.Windows.Forms.Panel();
            this.timer3 = new System.Windows.Forms.Timer(this.components);
            this.dataMonitoringTimer = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.splashTimer = new System.Windows.Forms.Timer(this.components);
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.pbRefreshData = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timer2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBRequestOperational)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBRequestPreOP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBGraphing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBHideShowRO)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBExpandTNode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBCollapseTNodes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbEvas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSearch)).BeginInit();
            this.mainRHPanel.SuspendLayout();
            this.DeviceStatusPanel.SuspendLayout();
            this.DeviceBuildGB.SuspendLayout();
            this.DevSevconGB.SuspendLayout();
            this.faultDtatusGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.actFaultsDG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.devEmerDG)).BeginInit();
            this.devCANGB.SuspendLayout();
            this.SystemPDOsPanel.SuspendLayout();
            this.Pnl_COBTxConfig.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.GB_COBTxTriggers.SuspendLayout();
            this.GB_InhibitTime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_InhibitITme)).BeginInit();
            this.GB_EventTIme.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUDEventTime)).BeginInit();
            this.GB_AsynchTypes.SuspendLayout();
            this.GB_SyncTypes.SuspendLayout();
            this.GB_SyncTimings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_TxNumSyncs)).BeginInit();
            this.systemStatusPanel.SuspendLayout();
            this.systemCANopenGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.emergencyDG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.connectedDevicesDG)).BeginInit();
            this.DCFStatusPanel.SuspendLayout();
            this.newDCFIdentityGB.SuspendLayout();
            this.DCFfileInfoGB.SuspendLayout();
            this.DCFSourceGB.SuspendLayout();
            this.dataGridPanel.SuspendLayout();
            this.keyPanel.SuspendLayout();
            this.writeInPreOpPanel.SuspendLayout();
            this.readWriteInPreopPanel.SuspendLayout();
            this.readWritePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.MainLHPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbRefreshData)).BeginInit();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.ToolsMenu,
            this.main_hlp_mi,
            this.file_mi,
            this.autoValidateMI});
            // 
            // ToolsMenu
            // 
            resources.ApplyResources(this.ToolsMenu, "ToolsMenu");
            this.ToolsMenu.Index = 0;
            this.ToolsMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.OptionsMI,
            this.FindMI,
            this.miRefreshData});
            // 
            // OptionsMI
            // 
            this.OptionsMI.Index = 0;
            resources.ApplyResources(this.OptionsMI, "OptionsMI");
            this.OptionsMI.Click += new System.EventHandler(this.OptionsMI_Click);
            // 
            // FindMI
            // 
            this.FindMI.Index = 1;
            resources.ApplyResources(this.FindMI, "FindMI");
            this.FindMI.Click += new System.EventHandler(this.FindMI_Click);
            // 
            // miRefreshData
            // 
            this.miRefreshData.Index = 2;
            resources.ApplyResources(this.miRefreshData, "miRefreshData");
            this.miRefreshData.Click += new System.EventHandler(this.refreshDataMI_Click);
            // 
            // main_hlp_mi
            // 
            this.main_hlp_mi.Index = 1;
            this.main_hlp_mi.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.topics_hlp_mi,
            this.about_hlp_mi,
            this.update_hlp_mi});
            this.main_hlp_mi.MergeOrder = 2;
            this.main_hlp_mi.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
            resources.ApplyResources(this.main_hlp_mi, "main_hlp_mi");
            // 
            // topics_hlp_mi
            // 
            this.topics_hlp_mi.Index = 0;
            resources.ApplyResources(this.topics_hlp_mi, "topics_hlp_mi");
            this.topics_hlp_mi.Click += new System.EventHandler(this.topics_hlp_mi_Click);
            // 
            // about_hlp_mi
            // 
            this.about_hlp_mi.Index = 1;
            resources.ApplyResources(this.about_hlp_mi, "about_hlp_mi");
            this.about_hlp_mi.Click += new System.EventHandler(this.about_hlp_mi_Click);
            // 
            // update_hlp_mi
            // 
            resources.ApplyResources(this.update_hlp_mi, "update_hlp_mi");
            this.update_hlp_mi.Index = 2;
            // 
            // file_mi
            // 
            this.file_mi.Index = 2;
            this.file_mi.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.file_exit_mi});
            this.file_mi.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
            resources.ApplyResources(this.file_mi, "file_mi");
            // 
            // file_exit_mi
            // 
            this.file_exit_mi.Index = 0;
            this.file_exit_mi.MergeOrder = 10;
            this.file_exit_mi.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
            resources.ApplyResources(this.file_exit_mi, "file_exit_mi");
            this.file_exit_mi.Click += new System.EventHandler(this.file_exit_mi_Click);
            // 
            // autoValidateMI
            // 
            this.autoValidateMI.Index = 3;
            this.autoValidateMI.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.autoValidate});
            this.autoValidateMI.MergeOrder = 20;
            resources.ApplyResources(this.autoValidateMI, "autoValidateMI");
            // 
            // autoValidate
            // 
            this.autoValidate.Index = 0;
            resources.ApplyResources(this.autoValidate, "autoValidate");
            // 
            // statusBar1
            // 
            this.statusBar1.ForeColor = System.Drawing.Color.MidnightBlue;
            resources.ApplyResources(this.statusBar1, "statusBar1");
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statusBarPanel2,
            this.statusBarPanel1,
            this.statusBarPanel3});
            this.statusBar1.ShowPanels = true;
            this.statusBar1.SizingGrip = false;
            // 
            // statusBarPanel2
            // 
            this.statusBarPanel2.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
            this.statusBarPanel2.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
            resources.ApplyResources(this.statusBarPanel2, "statusBarPanel2");
            // 
            // statusBarPanel1
            // 
            this.statusBarPanel1.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
            this.statusBarPanel1.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
            resources.ApplyResources(this.statusBarPanel1, "statusBarPanel1");
            // 
            // statusBarPanel3
            // 
            this.statusBarPanel3.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
            this.statusBarPanel3.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
            resources.ApplyResources(this.statusBarPanel3, "statusBarPanel3");
            // 
            // timer2
            // 
            this.timer2.SynchronizingObject = this;
            this.timer2.Elapsed += new System.Timers.ElapsedEventHandler(this.timer2_Elapsed);
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 0;
            this.toolTip1.ShowAlways = true;
            // 
            // PBRequestOperational
            // 
            resources.ApplyResources(this.PBRequestOperational, "PBRequestOperational");
            this.PBRequestOperational.Name = "PBRequestOperational";
            this.PBRequestOperational.TabStop = false;
            this.toolTip1.SetToolTip(this.PBRequestOperational, resources.GetString("PBRequestOperational.ToolTip"));
            this.PBRequestOperational.Click += new System.EventHandler(this.toolBarIcon_Click);
            this.PBRequestOperational.MouseEnter += new System.EventHandler(this.ToolBar_pictureBox_MouseEnter);
            // 
            // PBRequestPreOP
            // 
            resources.ApplyResources(this.PBRequestPreOP, "PBRequestPreOP");
            this.PBRequestPreOP.Name = "PBRequestPreOP";
            this.PBRequestPreOP.TabStop = false;
            this.toolTip1.SetToolTip(this.PBRequestPreOP, resources.GetString("PBRequestPreOP.ToolTip"));
            this.PBRequestPreOP.Click += new System.EventHandler(this.toolBarIcon_Click);
            this.PBRequestPreOP.MouseEnter += new System.EventHandler(this.ToolBar_pictureBox_MouseEnter);
            // 
            // PBGraphing
            // 
            this.PBGraphing.ContextMenu = this.CMenuGraphingIcon;
            resources.ApplyResources(this.PBGraphing, "PBGraphing");
            this.PBGraphing.Name = "PBGraphing";
            this.PBGraphing.TabStop = false;
            this.toolTip1.SetToolTip(this.PBGraphing, resources.GetString("PBGraphing.ToolTip"));
            this.PBGraphing.Click += new System.EventHandler(this.toolBarIcon_Click);
            this.PBGraphing.MouseEnter += new System.EventHandler(this.ToolBar_pictureBox_MouseEnter);
            // 
            // PBHideShowRO
            // 
            this.PBHideShowRO.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.PBHideShowRO, "PBHideShowRO");
            this.PBHideShowRO.Name = "PBHideShowRO";
            this.PBHideShowRO.TabStop = false;
            this.toolTip1.SetToolTip(this.PBHideShowRO, resources.GetString("PBHideShowRO.ToolTip"));
            this.PBHideShowRO.Click += new System.EventHandler(this.toolBarIcon_Click);
            this.PBHideShowRO.MouseEnter += new System.EventHandler(this.ToolBar_pictureBox_MouseEnter);
            // 
            // PBExpandTNode
            // 
            this.PBExpandTNode.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.PBExpandTNode, "PBExpandTNode");
            this.PBExpandTNode.Name = "PBExpandTNode";
            this.PBExpandTNode.TabStop = false;
            this.toolTip1.SetToolTip(this.PBExpandTNode, resources.GetString("PBExpandTNode.ToolTip"));
            this.PBExpandTNode.Click += new System.EventHandler(this.toolBarIcon_Click);
            this.PBExpandTNode.MouseEnter += new System.EventHandler(this.ToolBar_pictureBox_MouseEnter);
            // 
            // PBCollapseTNodes
            // 
            this.PBCollapseTNodes.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.PBCollapseTNodes, "PBCollapseTNodes");
            this.PBCollapseTNodes.Name = "PBCollapseTNodes";
            this.PBCollapseTNodes.TabStop = false;
            this.toolTip1.SetToolTip(this.PBCollapseTNodes, resources.GetString("PBCollapseTNodes.ToolTip"));
            this.PBCollapseTNodes.Click += new System.EventHandler(this.toolBarIcon_Click);
            this.PBCollapseTNodes.MouseEnter += new System.EventHandler(this.ToolBar_pictureBox_MouseEnter);
            // 
            // pbEvas
            // 
            this.pbEvas.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.pbEvas, "pbEvas");
            this.pbEvas.Name = "pbEvas";
            this.pbEvas.TabStop = false;
            this.toolTip1.SetToolTip(this.pbEvas, resources.GetString("pbEvas.ToolTip"));
            this.pbEvas.Click += new System.EventHandler(this.toolBarIcon_Click);
            this.pbEvas.MouseEnter += new System.EventHandler(this.ToolBar_pictureBox_MouseEnter);
            // 
            // pbSearch
            // 
            this.pbSearch.BackColor = System.Drawing.SystemColors.Control;
            this.pbSearch.ErrorImage = null;
            resources.ApplyResources(this.pbSearch, "pbSearch");
            this.pbSearch.Name = "pbSearch";
            this.pbSearch.TabStop = false;
            this.toolTip1.SetToolTip(this.pbSearch, resources.GetString("pbSearch.ToolTip"));
            this.pbSearch.Click += new System.EventHandler(this.toolBarIcon_Click);
            this.pbSearch.MouseEnter += new System.EventHandler(this.ToolBar_pictureBox_MouseEnter);
            // 
            // toolBar1
            // 
            this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.tbb_PreOPRequest,
            this.tbb_OpRequest,
            this.spacer1,
            this.tbb_Graphing,
            this.spacer2,
            this.tbb_HideShowRO,
            this.spacer3,
            this.tbb_ExpandTnodes,
            this.tbb_collapseTNodes,
            this.spacer5,
            this.tbb_Evas,
            this.spacer4,
            this.tbb_SearchTree,
            this.tbb_RefreshData});
            resources.ApplyResources(this.toolBar1, "toolBar1");
            this.toolBar1.CausesValidation = false;
            this.toolBar1.ImageList = this.anim_icons;
            this.toolBar1.Name = "toolBar1";
            // 
            // tbb_PreOPRequest
            // 
            resources.ApplyResources(this.tbb_PreOPRequest, "tbb_PreOPRequest");
            this.tbb_PreOPRequest.Name = "tbb_PreOPRequest";
            // 
            // tbb_OpRequest
            // 
            resources.ApplyResources(this.tbb_OpRequest, "tbb_OpRequest");
            this.tbb_OpRequest.Name = "tbb_OpRequest";
            // 
            // spacer1
            // 
            this.spacer1.Name = "spacer1";
            this.spacer1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // tbb_Graphing
            // 
            this.tbb_Graphing.DropDownMenu = this.CMenuGraphingIcon;
            resources.ApplyResources(this.tbb_Graphing, "tbb_Graphing");
            this.tbb_Graphing.Name = "tbb_Graphing";
            this.tbb_Graphing.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
            // 
            // spacer2
            // 
            this.spacer2.Name = "spacer2";
            this.spacer2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // tbb_HideShowRO
            // 
            this.tbb_HideShowRO.Name = "tbb_HideShowRO";
            // 
            // spacer3
            // 
            this.spacer3.Name = "spacer3";
            this.spacer3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // tbb_ExpandTnodes
            // 
            this.tbb_ExpandTnodes.Name = "tbb_ExpandTnodes";
            // 
            // tbb_collapseTNodes
            // 
            this.tbb_collapseTNodes.Name = "tbb_collapseTNodes";
            // 
            // spacer5
            // 
            this.spacer5.Name = "spacer5";
            this.spacer5.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // tbb_Evas
            // 
            this.tbb_Evas.Name = "tbb_Evas";
            // 
            // spacer4
            // 
            this.spacer4.Name = "spacer4";
            this.spacer4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // tbb_SearchTree
            // 
            this.tbb_SearchTree.Name = "tbb_SearchTree";
            // 
            // tbb_RefreshData
            // 
            this.tbb_RefreshData.Name = "tbb_RefreshData";
            // 
            // anim_icons
            // 
            this.anim_icons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("anim_icons.ImageStream")));
            this.anim_icons.TransparentColor = System.Drawing.Color.White;
            this.anim_icons.Images.SetKeyName(0, "");
            this.anim_icons.Images.SetKeyName(1, "");
            this.anim_icons.Images.SetKeyName(2, "");
            this.anim_icons.Images.SetKeyName(3, "");
            this.anim_icons.Images.SetKeyName(4, "");
            this.anim_icons.Images.SetKeyName(5, "");
            this.anim_icons.Images.SetKeyName(6, "");
            this.anim_icons.Images.SetKeyName(7, "");
            this.anim_icons.Images.SetKeyName(8, "");
            this.anim_icons.Images.SetKeyName(9, "");
            this.anim_icons.Images.SetKeyName(10, "");
            this.anim_icons.Images.SetKeyName(11, "");
            this.anim_icons.Images.SetKeyName(12, "");
            this.anim_icons.Images.SetKeyName(13, "");
            this.anim_icons.Images.SetKeyName(14, "");
            this.anim_icons.Images.SetKeyName(15, "");
            this.anim_icons.Images.SetKeyName(16, "");
            this.anim_icons.Images.SetKeyName(17, "");
            this.anim_icons.Images.SetKeyName(18, "");
            this.anim_icons.Images.SetKeyName(19, "");
            this.anim_icons.Images.SetKeyName(20, "");
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.PapayaWhip;
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // label11
            // 
            this.label11.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.PapayaWhip;
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.PapayaWhip;
            resources.ApplyResources(this.panel5, "panel5");
            this.panel5.Name = "panel5";
            // 
            // treeView1
            // 
            this.treeView1.AllowDrop = true;
            this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView1.ContextMenu = this.contextMenu1;
            resources.ApplyResources(this.treeView1, "treeView1");
            this.treeView1.ForeColor = System.Drawing.Color.Black;
            this.treeView1.FullRowSelect = true;
            this.treeView1.HideSelection = false;
            this.treeView1.ImageList = this.anim_icons;
            this.treeView1.ItemHeight = 16;
            this.treeView1.Name = "treeView1";
            this.treeView1.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCollapse);
            this.treeView1.DragLeave += new System.EventHandler(this.treeView1_DragLeave);
            this.treeView1.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeExpand);
            this.treeView1.DoubleClick += new System.EventHandler(this.treeView1_DoubleClick);
            this.treeView1.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeCollapse);
            this.treeView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseUp);
            this.treeView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView1_DragDrop);
            this.treeView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseDown);
            this.treeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterExpand);
            this.treeView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView1_ItemDrag);
            this.treeView1.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView1_DragOver);
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.DimGray;
            resources.ApplyResources(this.splitter1, "splitter1");
            this.splitter1.Name = "splitter1";
            this.splitter1.TabStop = false;
            // 
            // mainRHPanel
            // 
            this.mainRHPanel.BackColor = System.Drawing.Color.Gainsboro;
            this.mainRHPanel.Controls.Add(this.DeviceStatusPanel);
            this.mainRHPanel.Controls.Add(this.UserInstructionsLabel);
            this.mainRHPanel.Controls.Add(this.SystemPDOsPanel);
            this.mainRHPanel.Controls.Add(this.systemStatusPanel);
            this.mainRHPanel.Controls.Add(this.blankPanel);
            this.mainRHPanel.Controls.Add(this.DCFStatusPanel);
            this.mainRHPanel.Controls.Add(this.dataGridPanel);
            resources.ApplyResources(this.mainRHPanel, "mainRHPanel");
            this.mainRHPanel.ForeColor = System.Drawing.Color.MidnightBlue;
            this.mainRHPanel.Name = "mainRHPanel";
            // 
            // DeviceStatusPanel
            // 
            this.DeviceStatusPanel.BackColor = System.Drawing.Color.White;
            this.DeviceStatusPanel.Controls.Add(this.DeviceBuildGB);
            this.DeviceStatusPanel.Controls.Add(this.DevSevconGB);
            this.DeviceStatusPanel.Controls.Add(this.faultDtatusGB);
            this.DeviceStatusPanel.Controls.Add(this.devCANGB);
            resources.ApplyResources(this.DeviceStatusPanel, "DeviceStatusPanel");
            this.DeviceStatusPanel.Name = "DeviceStatusPanel";
            // 
            // DeviceBuildGB
            // 
            resources.ApplyResources(this.DeviceBuildGB, "DeviceBuildGB");
            this.DeviceBuildGB.Controls.Add(this.vendorNameLbl);
            this.DeviceBuildGB.Controls.Add(this.ExtROMChksumLbl);
            this.DeviceBuildGB.Controls.Add(this.IntROMChksumLbl);
            this.DeviceBuildGB.Controls.Add(this.configChkSumLbl);
            this.DeviceBuildGB.Controls.Add(this.serialNoLbl);
            this.DeviceBuildGB.Controls.Add(this.RevNumberLbl);
            this.DeviceBuildGB.Controls.Add(this.VendorIDLbl);
            this.DeviceBuildGB.Controls.Add(this.productCodeLbl);
            this.DeviceBuildGB.Controls.Add(this.SWVrLbl);
            this.DeviceBuildGB.Controls.Add(this.HWVerLbl);
            this.DeviceBuildGB.Name = "DeviceBuildGB";
            this.DeviceBuildGB.TabStop = false;
            // 
            // vendorNameLbl
            // 
            resources.ApplyResources(this.vendorNameLbl, "vendorNameLbl");
            this.vendorNameLbl.Name = "vendorNameLbl";
            // 
            // ExtROMChksumLbl
            // 
            resources.ApplyResources(this.ExtROMChksumLbl, "ExtROMChksumLbl");
            this.ExtROMChksumLbl.Name = "ExtROMChksumLbl";
            // 
            // IntROMChksumLbl
            // 
            resources.ApplyResources(this.IntROMChksumLbl, "IntROMChksumLbl");
            this.IntROMChksumLbl.Name = "IntROMChksumLbl";
            // 
            // configChkSumLbl
            // 
            resources.ApplyResources(this.configChkSumLbl, "configChkSumLbl");
            this.configChkSumLbl.Name = "configChkSumLbl";
            // 
            // serialNoLbl
            // 
            resources.ApplyResources(this.serialNoLbl, "serialNoLbl");
            this.serialNoLbl.Name = "serialNoLbl";
            // 
            // RevNumberLbl
            // 
            resources.ApplyResources(this.RevNumberLbl, "RevNumberLbl");
            this.RevNumberLbl.Name = "RevNumberLbl";
            // 
            // VendorIDLbl
            // 
            resources.ApplyResources(this.VendorIDLbl, "VendorIDLbl");
            this.VendorIDLbl.Name = "VendorIDLbl";
            // 
            // productCodeLbl
            // 
            resources.ApplyResources(this.productCodeLbl, "productCodeLbl");
            this.productCodeLbl.Name = "productCodeLbl";
            // 
            // SWVrLbl
            // 
            resources.ApplyResources(this.SWVrLbl, "SWVrLbl");
            this.SWVrLbl.Name = "SWVrLbl";
            // 
            // HWVerLbl
            // 
            resources.ApplyResources(this.HWVerLbl, "HWVerLbl");
            this.HWVerLbl.Name = "HWVerLbl";
            // 
            // DevSevconGB
            // 
            resources.ApplyResources(this.DevSevconGB, "DevSevconGB");
            this.DevSevconGB.Controls.Add(this.CapVoltLbl);
            this.DevSevconGB.Controls.Add(this.BattVoltLbl);
            this.DevSevconGB.Controls.Add(this.temperatureLbl);
            this.DevSevconGB.Name = "DevSevconGB";
            this.DevSevconGB.TabStop = false;
            // 
            // CapVoltLbl
            // 
            resources.ApplyResources(this.CapVoltLbl, "CapVoltLbl");
            this.CapVoltLbl.Name = "CapVoltLbl";
            // 
            // BattVoltLbl
            // 
            resources.ApplyResources(this.BattVoltLbl, "BattVoltLbl");
            this.BattVoltLbl.Name = "BattVoltLbl";
            // 
            // temperatureLbl
            // 
            resources.ApplyResources(this.temperatureLbl, "temperatureLbl");
            this.temperatureLbl.Name = "temperatureLbl";
            // 
            // faultDtatusGB
            // 
            resources.ApplyResources(this.faultDtatusGB, "faultDtatusGB");
            this.faultDtatusGB.Controls.Add(this.actFaultsDG);
            this.faultDtatusGB.Controls.Add(this.devEmerDG);
            this.faultDtatusGB.Controls.Add(this.label32);
            this.faultDtatusGB.Controls.Add(this.serviceDueLbl);
            this.faultDtatusGB.Name = "faultDtatusGB";
            this.faultDtatusGB.TabStop = false;
            // 
            // actFaultsDG
            // 
            this.actFaultsDG.AllowNavigation = false;
            resources.ApplyResources(this.actFaultsDG, "actFaultsDG");
            this.actFaultsDG.BackgroundColor = System.Drawing.Color.White;
            this.actFaultsDG.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.actFaultsDG.DataMember = "";
            this.actFaultsDG.FlatMode = true;
            this.actFaultsDG.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.actFaultsDG.Name = "actFaultsDG";
            this.actFaultsDG.ReadOnly = true;
            // 
            // devEmerDG
            // 
            this.devEmerDG.AllowNavigation = false;
            resources.ApplyResources(this.devEmerDG, "devEmerDG");
            this.devEmerDG.BackgroundColor = System.Drawing.Color.White;
            this.devEmerDG.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.devEmerDG.DataMember = "";
            this.devEmerDG.FlatMode = true;
            this.devEmerDG.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.devEmerDG.Name = "devEmerDG";
            this.devEmerDG.ReadOnly = true;
            // 
            // label32
            // 
            resources.ApplyResources(this.label32, "label32");
            this.label32.Name = "label32";
            // 
            // serviceDueLbl
            // 
            resources.ApplyResources(this.serviceDueLbl, "serviceDueLbl");
            this.serviceDueLbl.Name = "serviceDueLbl";
            // 
            // devCANGB
            // 
            resources.ApplyResources(this.devCANGB, "devCANGB");
            this.devCANGB.Controls.Add(this.loginLbl);
            this.devCANGB.Controls.Add(this.NMTStateLbl);
            this.devCANGB.Controls.Add(this.MasterSlaveLbl);
            this.devCANGB.Name = "devCANGB";
            this.devCANGB.TabStop = false;
            // 
            // loginLbl
            // 
            resources.ApplyResources(this.loginLbl, "loginLbl");
            this.loginLbl.Name = "loginLbl";
            // 
            // NMTStateLbl
            // 
            resources.ApplyResources(this.NMTStateLbl, "NMTStateLbl");
            this.NMTStateLbl.Name = "NMTStateLbl";
            // 
            // MasterSlaveLbl
            // 
            resources.ApplyResources(this.MasterSlaveLbl, "MasterSlaveLbl");
            this.MasterSlaveLbl.Name = "MasterSlaveLbl";
            // 
            // UserInstructionsLabel
            // 
            resources.ApplyResources(this.UserInstructionsLabel, "UserInstructionsLabel");
            this.UserInstructionsLabel.BackColor = System.Drawing.Color.White;
            this.UserInstructionsLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.UserInstructionsLabel.Name = "UserInstructionsLabel";
            // 
            // SystemPDOsPanel
            // 
            this.SystemPDOsPanel.BackColor = System.Drawing.Color.MistyRose;
            this.SystemPDOsPanel.Controls.Add(this.Splitter_PDORouteToMappings);
            this.SystemPDOsPanel.Controls.Add(this.Pnl_COBMappings);
            this.SystemPDOsPanel.Controls.Add(this.splitter2);
            this.SystemPDOsPanel.Controls.Add(this.Pnl_COBTxConfig);
            this.SystemPDOsPanel.Controls.Add(this.PDORoutingPanel);
            resources.ApplyResources(this.SystemPDOsPanel, "SystemPDOsPanel");
            this.SystemPDOsPanel.Name = "SystemPDOsPanel";
            // 
            // Splitter_PDORouteToMappings
            // 
            resources.ApplyResources(this.Splitter_PDORouteToMappings, "Splitter_PDORouteToMappings");
            this.Splitter_PDORouteToMappings.Name = "Splitter_PDORouteToMappings";
            this.Splitter_PDORouteToMappings.TabStop = false;
            // 
            // Pnl_COBMappings
            // 
            this.Pnl_COBMappings.AllowDrop = true;
            this.Pnl_COBMappings.BackColor = System.Drawing.Color.DarkViolet;
            resources.ApplyResources(this.Pnl_COBMappings, "Pnl_COBMappings");
            this.Pnl_COBMappings.Name = "Pnl_COBMappings";
            // 
            // splitter2
            // 
            resources.ApplyResources(this.splitter2, "splitter2");
            this.splitter2.Name = "splitter2";
            this.splitter2.TabStop = false;
            // 
            // Pnl_COBTxConfig
            // 
            this.Pnl_COBTxConfig.Controls.Add(this.groupBox2);
            resources.ApplyResources(this.Pnl_COBTxConfig, "Pnl_COBTxConfig");
            this.Pnl_COBTxConfig.Name = "Pnl_COBTxConfig";
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.CB_SPDO_priority);
            this.groupBox2.Controls.Add(this.GB_COBTxTriggers);
            this.groupBox2.Controls.Add(this.label19);
            this.groupBox2.Controls.Add(this.label22);
            this.groupBox2.Controls.Add(this.TB_SPDO_COBID);
            this.groupBox2.Controls.Add(this.label23);
            this.groupBox2.Controls.Add(this.CB_SPDOName);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // CB_SPDO_priority
            // 
            this.CB_SPDO_priority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.CB_SPDO_priority, "CB_SPDO_priority");
            this.CB_SPDO_priority.Name = "CB_SPDO_priority";
            // 
            // GB_COBTxTriggers
            // 
            this.GB_COBTxTriggers.Controls.Add(this.GB_InhibitTime);
            this.GB_COBTxTriggers.Controls.Add(this.GB_EventTIme);
            this.GB_COBTxTriggers.Controls.Add(this.GB_AsynchTypes);
            this.GB_COBTxTriggers.Controls.Add(this.GB_SyncTypes);
            this.GB_COBTxTriggers.Controls.Add(this.GB_SyncTimings);
            resources.ApplyResources(this.GB_COBTxTriggers, "GB_COBTxTriggers");
            this.GB_COBTxTriggers.Name = "GB_COBTxTriggers";
            this.GB_COBTxTriggers.TabStop = false;
            // 
            // GB_InhibitTime
            // 
            this.GB_InhibitTime.Controls.Add(this.NUD_InhibitITme);
            this.GB_InhibitTime.Controls.Add(this.label1);
            this.GB_InhibitTime.Controls.Add(this.label16);
            resources.ApplyResources(this.GB_InhibitTime, "GB_InhibitTime");
            this.GB_InhibitTime.Name = "GB_InhibitTime";
            this.GB_InhibitTime.TabStop = false;
            // 
            // NUD_InhibitITme
            // 
            this.NUD_InhibitITme.InterceptArrowKeys = false;
            resources.ApplyResources(this.NUD_InhibitITme, "NUD_InhibitITme");
            this.NUD_InhibitITme.Maximum = new decimal(new int[] {
            6550,
            0,
            0,
            0});
            this.NUD_InhibitITme.Name = "NUD_InhibitITme";
            this.NUD_InhibitITme.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label16
            // 
            resources.ApplyResources(this.label16, "label16");
            this.label16.Name = "label16";
            // 
            // GB_EventTIme
            // 
            this.GB_EventTIme.Controls.Add(this.NUDEventTime);
            this.GB_EventTIme.Controls.Add(this.label13);
            this.GB_EventTIme.Controls.Add(this.label15);
            resources.ApplyResources(this.GB_EventTIme, "GB_EventTIme");
            this.GB_EventTIme.Name = "GB_EventTIme";
            this.GB_EventTIme.TabStop = false;
            // 
            // NUDEventTime
            // 
            resources.ApplyResources(this.NUDEventTime, "NUDEventTime");
            this.NUDEventTime.Name = "NUDEventTime";
            this.NUDEventTime.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            // 
            // GB_AsynchTypes
            // 
            this.GB_AsynchTypes.Controls.Add(this.RB_AsynchRTR);
            this.GB_AsynchTypes.Controls.Add(this.RB_AsynchNormal);
            resources.ApplyResources(this.GB_AsynchTypes, "GB_AsynchTypes");
            this.GB_AsynchTypes.Name = "GB_AsynchTypes";
            this.GB_AsynchTypes.TabStop = false;
            // 
            // RB_AsynchRTR
            // 
            resources.ApplyResources(this.RB_AsynchRTR, "RB_AsynchRTR");
            this.RB_AsynchRTR.Name = "RB_AsynchRTR";
            // 
            // RB_AsynchNormal
            // 
            resources.ApplyResources(this.RB_AsynchNormal, "RB_AsynchNormal");
            this.RB_AsynchNormal.Name = "RB_AsynchNormal";
            // 
            // GB_SyncTypes
            // 
            this.GB_SyncTypes.Controls.Add(this.RB_SyncRTR);
            this.GB_SyncTypes.Controls.Add(this.RB_SyncCyclic);
            this.GB_SyncTypes.Controls.Add(this.RB_SyncAcyclic);
            resources.ApplyResources(this.GB_SyncTypes, "GB_SyncTypes");
            this.GB_SyncTypes.Name = "GB_SyncTypes";
            this.GB_SyncTypes.TabStop = false;
            // 
            // RB_SyncRTR
            // 
            resources.ApplyResources(this.RB_SyncRTR, "RB_SyncRTR");
            this.RB_SyncRTR.Name = "RB_SyncRTR";
            // 
            // RB_SyncCyclic
            // 
            resources.ApplyResources(this.RB_SyncCyclic, "RB_SyncCyclic");
            this.RB_SyncCyclic.Name = "RB_SyncCyclic";
            // 
            // RB_SyncAcyclic
            // 
            resources.ApplyResources(this.RB_SyncAcyclic, "RB_SyncAcyclic");
            this.RB_SyncAcyclic.Name = "RB_SyncAcyclic";
            // 
            // GB_SyncTimings
            // 
            this.GB_SyncTimings.Controls.Add(this.Lbl_COBInvalidTxTpye);
            this.GB_SyncTimings.Controls.Add(this.label17);
            this.GB_SyncTimings.Controls.Add(this.NUD_TxNumSyncs);
            resources.ApplyResources(this.GB_SyncTimings, "GB_SyncTimings");
            this.GB_SyncTimings.Name = "GB_SyncTimings";
            this.GB_SyncTimings.TabStop = false;
            // 
            // Lbl_COBInvalidTxTpye
            // 
            resources.ApplyResources(this.Lbl_COBInvalidTxTpye, "Lbl_COBInvalidTxTpye");
            this.Lbl_COBInvalidTxTpye.Name = "Lbl_COBInvalidTxTpye";
            // 
            // label17
            // 
            resources.ApplyResources(this.label17, "label17");
            this.label17.Name = "label17";
            // 
            // NUD_TxNumSyncs
            // 
            resources.ApplyResources(this.NUD_TxNumSyncs, "NUD_TxNumSyncs");
            this.NUD_TxNumSyncs.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
            this.NUD_TxNumSyncs.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NUD_TxNumSyncs.Name = "NUD_TxNumSyncs";
            this.NUD_TxNumSyncs.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label19
            // 
            resources.ApplyResources(this.label19, "label19");
            this.label19.Name = "label19";
            // 
            // label22
            // 
            resources.ApplyResources(this.label22, "label22");
            this.label22.Name = "label22";
            // 
            // TB_SPDO_COBID
            // 
            resources.ApplyResources(this.TB_SPDO_COBID, "TB_SPDO_COBID");
            this.TB_SPDO_COBID.Name = "TB_SPDO_COBID";
            // 
            // label23
            // 
            resources.ApplyResources(this.label23, "label23");
            this.label23.Name = "label23";
            // 
            // CB_SPDOName
            // 
            resources.ApplyResources(this.CB_SPDOName, "CB_SPDOName");
            this.CB_SPDOName.Name = "CB_SPDOName";
            // 
            // PDORoutingPanel
            // 
            this.PDORoutingPanel.AllowDrop = true;
            resources.ApplyResources(this.PDORoutingPanel, "PDORoutingPanel");
            this.PDORoutingPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.PDORoutingPanel.Name = "PDORoutingPanel";
            // 
            // systemStatusPanel
            // 
            this.systemStatusPanel.BackColor = System.Drawing.Color.White;
            this.systemStatusPanel.Controls.Add(this.systemCANopenGB);
            resources.ApplyResources(this.systemStatusPanel, "systemStatusPanel");
            this.systemStatusPanel.Name = "systemStatusPanel";
            // 
            // systemCANopenGB
            // 
            resources.ApplyResources(this.systemCANopenGB, "systemCANopenGB");
            this.systemCANopenGB.Controls.Add(this.emergencyDG);
            this.systemCANopenGB.Controls.Add(this.connectedDevicesDG);
            this.systemCANopenGB.Name = "systemCANopenGB";
            this.systemCANopenGB.TabStop = false;
            // 
            // emergencyDG
            // 
            resources.ApplyResources(this.emergencyDG, "emergencyDG");
            this.emergencyDG.BackColor = System.Drawing.Color.White;
            this.emergencyDG.BackgroundColor = System.Drawing.Color.White;
            this.emergencyDG.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.emergencyDG.DataMember = "";
            this.emergencyDG.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.emergencyDG.Name = "emergencyDG";
            // 
            // connectedDevicesDG
            // 
            resources.ApplyResources(this.connectedDevicesDG, "connectedDevicesDG");
            this.connectedDevicesDG.BackColor = System.Drawing.Color.White;
            this.connectedDevicesDG.BackgroundColor = System.Drawing.Color.White;
            this.connectedDevicesDG.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.connectedDevicesDG.DataMember = "";
            this.connectedDevicesDG.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.connectedDevicesDG.Name = "connectedDevicesDG";
            // 
            // blankPanel
            // 
            this.blankPanel.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.blankPanel, "blankPanel");
            this.blankPanel.Name = "blankPanel";
            // 
            // DCFStatusPanel
            // 
            this.DCFStatusPanel.BackColor = System.Drawing.Color.White;
            this.DCFStatusPanel.Controls.Add(this.newDCFIdentityGB);
            this.DCFStatusPanel.Controls.Add(this.DCFfileInfoGB);
            this.DCFStatusPanel.Controls.Add(this.DCFSourceGB);
            resources.ApplyResources(this.DCFStatusPanel, "DCFStatusPanel");
            this.DCFStatusPanel.Name = "DCFStatusPanel";
            // 
            // newDCFIdentityGB
            // 
            resources.ApplyResources(this.newDCFIdentityGB, "newDCFIdentityGB");
            this.newDCFIdentityGB.Controls.Add(this.Lbl_RevisionNo);
            this.newDCFIdentityGB.Controls.Add(this.Lbl_productCode);
            this.newDCFIdentityGB.Controls.Add(this.revNoCB3);
            this.newDCFIdentityGB.Controls.Add(this.productCodeCB);
            this.newDCFIdentityGB.Name = "newDCFIdentityGB";
            this.newDCFIdentityGB.TabStop = false;
            // 
            // Lbl_RevisionNo
            // 
            resources.ApplyResources(this.Lbl_RevisionNo, "Lbl_RevisionNo");
            this.Lbl_RevisionNo.Name = "Lbl_RevisionNo";
            // 
            // Lbl_productCode
            // 
            resources.ApplyResources(this.Lbl_productCode, "Lbl_productCode");
            this.Lbl_productCode.Name = "Lbl_productCode";
            // 
            // revNoCB3
            // 
            this.revNoCB3.Cursor = System.Windows.Forms.Cursors.IBeam;
            resources.ApplyResources(this.revNoCB3, "revNoCB3");
            this.revNoCB3.Name = "revNoCB3";
            this.revNoCB3.SelectionChangeCommitted += new System.EventHandler(this.DCF_combo_SelectionChangeCommitted);
            // 
            // productCodeCB
            // 
            resources.ApplyResources(this.productCodeCB, "productCodeCB");
            this.productCodeCB.Name = "productCodeCB";
            this.productCodeCB.SelectionChangeCommitted += new System.EventHandler(this.DCF_combo_SelectionChangeCommitted);
            // 
            // DCFfileInfoGB
            // 
            resources.ApplyResources(this.DCFfileInfoGB, "DCFfileInfoGB");
            this.DCFfileInfoGB.BackColor = System.Drawing.Color.White;
            this.DCFfileInfoGB.Controls.Add(this.DCFCreatedByLbl);
            this.DCFfileInfoGB.Controls.Add(this.DCFFileNameLbl);
            this.DCFfileInfoGB.Controls.Add(this.DCFFileLastModLbl);
            this.DCFfileInfoGB.ForeColor = System.Drawing.Color.MidnightBlue;
            this.DCFfileInfoGB.Name = "DCFfileInfoGB";
            this.DCFfileInfoGB.TabStop = false;
            // 
            // DCFCreatedByLbl
            // 
            resources.ApplyResources(this.DCFCreatedByLbl, "DCFCreatedByLbl");
            this.DCFCreatedByLbl.Name = "DCFCreatedByLbl";
            // 
            // DCFFileNameLbl
            // 
            resources.ApplyResources(this.DCFFileNameLbl, "DCFFileNameLbl");
            this.DCFFileNameLbl.Name = "DCFFileNameLbl";
            // 
            // DCFFileLastModLbl
            // 
            resources.ApplyResources(this.DCFFileLastModLbl, "DCFFileLastModLbl");
            this.DCFFileLastModLbl.Name = "DCFFileLastModLbl";
            // 
            // DCFSourceGB
            // 
            resources.ApplyResources(this.DCFSourceGB, "DCFSourceGB");
            this.DCFSourceGB.Controls.Add(this.DCFVendorNameLbl);
            this.DCFSourceGB.Controls.Add(this.DCFDeviceNameLbl);
            this.DCFSourceGB.Controls.Add(this.DCFRevNoLbl);
            this.DCFSourceGB.Controls.Add(this.DCFVendorIDLbl);
            this.DCFSourceGB.Controls.Add(this.DCFProductCodeLbl);
            this.DCFSourceGB.Name = "DCFSourceGB";
            this.DCFSourceGB.TabStop = false;
            // 
            // DCFVendorNameLbl
            // 
            resources.ApplyResources(this.DCFVendorNameLbl, "DCFVendorNameLbl");
            this.DCFVendorNameLbl.Name = "DCFVendorNameLbl";
            // 
            // DCFDeviceNameLbl
            // 
            resources.ApplyResources(this.DCFDeviceNameLbl, "DCFDeviceNameLbl");
            this.DCFDeviceNameLbl.Name = "DCFDeviceNameLbl";
            // 
            // DCFRevNoLbl
            // 
            resources.ApplyResources(this.DCFRevNoLbl, "DCFRevNoLbl");
            this.DCFRevNoLbl.Name = "DCFRevNoLbl";
            // 
            // DCFVendorIDLbl
            // 
            resources.ApplyResources(this.DCFVendorIDLbl, "DCFVendorIDLbl");
            this.DCFVendorIDLbl.Name = "DCFVendorIDLbl";
            // 
            // DCFProductCodeLbl
            // 
            resources.ApplyResources(this.DCFProductCodeLbl, "DCFProductCodeLbl");
            this.DCFProductCodeLbl.Name = "DCFProductCodeLbl";
            // 
            // dataGridPanel
            // 
            this.dataGridPanel.BackColor = System.Drawing.Color.White;
            this.dataGridPanel.Controls.Add(this.submitBtn);
            this.dataGridPanel.Controls.Add(this.keyPanel);
            this.dataGridPanel.Controls.Add(this.dataGrid1);
            resources.ApplyResources(this.dataGridPanel, "dataGridPanel");
            this.dataGridPanel.Name = "dataGridPanel";
            // 
            // submitBtn
            // 
            resources.ApplyResources(this.submitBtn, "submitBtn");
            this.submitBtn.BackColor = System.Drawing.SystemColors.Control;
            this.submitBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.submitBtn.Name = "submitBtn";
            this.submitBtn.UseVisualStyleBackColor = false;
            this.submitBtn.Click += new System.EventHandler(this.submitBtn_Click);
            // 
            // keyPanel
            // 
            resources.ApplyResources(this.keyPanel, "keyPanel");
            this.keyPanel.Controls.Add(this.writeInPreOpPanel);
            this.keyPanel.Controls.Add(this.readwritepreopLabel);
            this.keyPanel.Controls.Add(this.readWriteInPreopPanel);
            this.keyPanel.Controls.Add(this.writeInPreOpLabel);
            this.keyPanel.Controls.Add(this.writeOnlyPanel);
            this.keyPanel.Controls.Add(this.readOnlyPanel);
            this.keyPanel.Controls.Add(this.readWriteLabel);
            this.keyPanel.Controls.Add(this.writeOnlyLabel);
            this.keyPanel.Controls.Add(this.readOnlyLabel);
            this.keyPanel.Controls.Add(this.label20);
            this.keyPanel.Controls.Add(this.readWritePanel);
            this.keyPanel.Name = "keyPanel";
            // 
            // writeInPreOpPanel
            // 
            this.writeInPreOpPanel.BackColor = System.Drawing.Color.SaddleBrown;
            this.writeInPreOpPanel.Controls.Add(this.label12);
            this.writeInPreOpPanel.Controls.Add(this.panel7);
            resources.ApplyResources(this.writeInPreOpPanel, "writeInPreOpPanel");
            this.writeInPreOpPanel.Name = "writeInPreOpPanel";
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.Color.PapayaWhip;
            resources.ApplyResources(this.panel7, "panel7");
            this.panel7.Name = "panel7";
            // 
            // readwritepreopLabel
            // 
            this.readwritepreopLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.readwritepreopLabel, "readwritepreopLabel");
            this.readwritepreopLabel.Name = "readwritepreopLabel";
            // 
            // readWriteInPreopPanel
            // 
            this.readWriteInPreopPanel.BackColor = System.Drawing.Color.DarkGreen;
            this.readWriteInPreopPanel.Controls.Add(this.label14);
            this.readWriteInPreopPanel.Controls.Add(this.panel9);
            resources.ApplyResources(this.readWriteInPreopPanel, "readWriteInPreopPanel");
            this.readWriteInPreopPanel.Name = "readWriteInPreopPanel";
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            // 
            // panel9
            // 
            this.panel9.BackColor = System.Drawing.Color.PapayaWhip;
            resources.ApplyResources(this.panel9, "panel9");
            this.panel9.Name = "panel9";
            // 
            // writeInPreOpLabel
            // 
            resources.ApplyResources(this.writeInPreOpLabel, "writeInPreOpLabel");
            this.writeInPreOpLabel.Name = "writeInPreOpLabel";
            // 
            // writeOnlyPanel
            // 
            this.writeOnlyPanel.BackColor = System.Drawing.Color.Lavender;
            resources.ApplyResources(this.writeOnlyPanel, "writeOnlyPanel");
            this.writeOnlyPanel.Name = "writeOnlyPanel";
            // 
            // readOnlyPanel
            // 
            this.readOnlyPanel.BackColor = System.Drawing.Color.Gray;
            resources.ApplyResources(this.readOnlyPanel, "readOnlyPanel");
            this.readOnlyPanel.Name = "readOnlyPanel";
            // 
            // readWriteLabel
            // 
            this.readWriteLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.readWriteLabel, "readWriteLabel");
            this.readWriteLabel.Name = "readWriteLabel";
            // 
            // writeOnlyLabel
            // 
            this.writeOnlyLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.writeOnlyLabel, "writeOnlyLabel");
            this.writeOnlyLabel.Name = "writeOnlyLabel";
            // 
            // readOnlyLabel
            // 
            resources.ApplyResources(this.readOnlyLabel, "readOnlyLabel");
            this.readOnlyLabel.BackColor = System.Drawing.Color.Transparent;
            this.readOnlyLabel.Name = "readOnlyLabel";
            // 
            // label20
            // 
            resources.ApplyResources(this.label20, "label20");
            this.label20.Name = "label20";
            // 
            // readWritePanel
            // 
            this.readWritePanel.BackColor = System.Drawing.Color.DarkBlue;
            this.readWritePanel.Controls.Add(this.label21);
            this.readWritePanel.Controls.Add(this.panel14);
            resources.ApplyResources(this.readWritePanel, "readWritePanel");
            this.readWritePanel.Name = "readWritePanel";
            // 
            // label21
            // 
            resources.ApplyResources(this.label21, "label21");
            this.label21.Name = "label21";
            // 
            // panel14
            // 
            this.panel14.BackColor = System.Drawing.Color.PapayaWhip;
            resources.ApplyResources(this.panel14, "panel14");
            this.panel14.Name = "panel14";
            // 
            // dataGrid1
            // 
            resources.ApplyResources(this.dataGrid1, "dataGrid1");
            this.dataGrid1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGrid1.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dataGrid1.DataMember = "";
            this.dataGrid1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.ReadOnly = true;
            this.dataGrid1.SelectionBackColor = System.Drawing.SystemColors.Window;
            this.dataGrid1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid1_MouseMove);
            this.dataGrid1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid1_MouseDown);
            // 
            // MainLHPanel
            // 
            this.MainLHPanel.Controls.Add(this.treeView1);
            resources.ApplyResources(this.MainLHPanel, "MainLHPanel");
            this.MainLHPanel.Name = "MainLHPanel";
            // 
            // timer3
            // 
            this.timer3.Interval = 200;
            this.timer3.Tick += new System.EventHandler(this.timer3_Tick);
            // 
            // dataMonitoringTimer
            // 
            this.dataMonitoringTimer.Interval = 1000;
            this.dataMonitoringTimer.Tick += new System.EventHandler(this.dataMonitoringTimer_Tick);
            // 
            // splashTimer
            // 
            this.splashTimer.Tick += new System.EventHandler(this.splashTimer_Tick);
            // 
            // pbRefreshData
            // 
            this.pbRefreshData.BackColor = System.Drawing.SystemColors.Control;
            this.pbRefreshData.ErrorImage = null;
            resources.ApplyResources(this.pbRefreshData, "pbRefreshData");
            this.pbRefreshData.Name = "pbRefreshData";
            this.pbRefreshData.TabStop = false;
            this.pbRefreshData.Click += new System.EventHandler(this.toolBarIcon_Click);
            this.pbRefreshData.MouseEnter += new System.EventHandler(this.ToolBar_pictureBox_MouseEnter);
            // 
            // MAIN_WINDOW
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.Controls.Add(this.pbRefreshData);
            this.Controls.Add(this.pbEvas);
            this.Controls.Add(this.pbSearch);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.PBCollapseTNodes);
            this.Controls.Add(this.PBExpandTNode);
            this.Controls.Add(this.PBHideShowRO);
            this.Controls.Add(this.PBGraphing);
            this.Controls.Add(this.PBRequestPreOP);
            this.Controls.Add(this.PBRequestOperational);
            this.Controls.Add(this.mainRHPanel);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.MainLHPanel);
            this.Controls.Add(this.toolBar1);
            this.Controls.Add(this.statusBar1);
            this.ForeColor = System.Drawing.Color.MidnightBlue;
            this.IsMdiContainer = true;
            this.Menu = this.mainMenu1;
            this.Name = "MAIN_WINDOW";
            this.Load += new System.EventHandler(this.MAIN_WINDOW_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MAIN_WINDOW_Closing);
            this.Resize += new System.EventHandler(this.MAIN_WINDOW_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.timer2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBRequestOperational)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBRequestPreOP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBGraphing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBHideShowRO)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBExpandTNode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBCollapseTNodes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbEvas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSearch)).EndInit();
            this.mainRHPanel.ResumeLayout(false);
            this.DeviceStatusPanel.ResumeLayout(false);
            this.DeviceBuildGB.ResumeLayout(false);
            this.DevSevconGB.ResumeLayout(false);
            this.faultDtatusGB.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.actFaultsDG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.devEmerDG)).EndInit();
            this.devCANGB.ResumeLayout(false);
            this.SystemPDOsPanel.ResumeLayout(false);
            this.Pnl_COBTxConfig.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.GB_COBTxTriggers.ResumeLayout(false);
            this.GB_InhibitTime.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.NUD_InhibitITme)).EndInit();
            this.GB_EventTIme.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.NUDEventTime)).EndInit();
            this.GB_AsynchTypes.ResumeLayout(false);
            this.GB_SyncTypes.ResumeLayout(false);
            this.GB_SyncTimings.ResumeLayout(false);
            this.GB_SyncTimings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_TxNumSyncs)).EndInit();
            this.systemStatusPanel.ResumeLayout(false);
            this.systemCANopenGB.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.emergencyDG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.connectedDevicesDG)).EndInit();
            this.DCFStatusPanel.ResumeLayout(false);
            this.newDCFIdentityGB.ResumeLayout(false);
            this.newDCFIdentityGB.PerformLayout();
            this.DCFfileInfoGB.ResumeLayout(false);
            this.DCFSourceGB.ResumeLayout(false);
            this.dataGridPanel.ResumeLayout(false);
            this.keyPanel.ResumeLayout(false);
            this.writeInPreOpPanel.ResumeLayout(false);
            this.readWriteInPreopPanel.ResumeLayout(false);
            this.readWritePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.MainLHPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbRefreshData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		#region initialisation
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			#region permit single instance of DriveWizard 
			Process aProcess = Process.GetCurrentProcess();  //grabs this process ie this instance of DriveWizard
			string aProcName = aProcess.ProcessName;  //extracts its Name
            if (Process.GetProcessesByName(aProcName).Length > 1)
            { //and checks for anything else with the same name being run
                Application.ExitThread();  //if so the this instance immediately exits - before creating anything
            }
            else
            {
                Application.Run(new MAIN_WINDOW());
            }
			#endregion permit single instance of DriveWizard 
		}
		internal MAIN_WINDOW()
		{
			this.Visible = false;
			InitializeComponent();

			//actions are done whilst he splash screen is being displayed so start up time is minimised
			//generateEDSSectionsMasterList();
            //2005 has the filename as DriveWizard.EXE whereas 2003 has DriveWizard.exe
            //So convert to upper and get the indexPos of start of filename and then 
            //use this vlaue in th estring - so original strin gcasing remains intact
            // but we locate correct position regardless of version we are using
            string testStr = Application.ExecutablePath.ToUpper();
            int fileNamePos = testStr.LastIndexOf("DRIVEWIZARD.EXE");
            MAIN_WINDOW.ApplicationDirectoryPath = Application.ExecutablePath.Substring(0, fileNamePos);
			MAIN_WINDOW.UserDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SEVCON\Drive Wizard";
			copyInstallationFilesToUserDirectory();
            sysInfo = new SystemInfo();
			Microsoft.Win32.SystemEvents.PowerModeChanged +=new Microsoft.Win32.PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
			#region initialise System level Othe rWindow TreeNodes
			COBandPDOsTN = new TreeNode("Comms Objects setup wizard", (int) TVImages.COBExtPDO, (int) TVImages.COBExtPDO );
			COBandPDOsTN.Tag = new treeNodeTag(TNTagType.COBSCREEN,150 ,20, null);  //custom lists have node Id of 130 offset
			CANBUsConfigTN = new TreeNode("CAN Bus Configuration", (int)TVImages.BusConfig, (int)TVImages.BusConfig );
			CANBUsConfigTN.Tag = new treeNodeTag(TNTagType.BUSCONFIGSCREEN,150 ,20, null);  //custom lists have node Id of 130 offset
			#endregion initialise System level Othe rWindow TreeNodes
			this.splashTimer.Enabled = true;  //now permit checking of whether spashscreeen has been closed
			//note the string returned for the follwoing will be:
			//C:\documents and settings\<User name>\Application Data
			//The reason for using this is that not all users will be able to write to the Program Files
			///directory - this is a security feature of newer windows.
			///			//The application config file will be uised in the future to hold configuartion data
			//specific to that version and/or customer deployment. It will use dotnet config file c format for ease of later 
			//transfer to remote deployment/use
			//For this reason all files that the user creates/mdoeifes should be held here
			//judetempthis.sysInfo.notifyGUI +=new StateChangeListener(systemInfo_notifyGUI);
		}
		private void getinitialData()
		{  //in this  seperate thrread we can do anything that doe snot directly affec the GUI - these
		}
		private void generateEDSSectionsMasterList()
		{
            // must be called after connection to system as sevconSectionIDList updated dynamically in there
            masterEDSSections = sysInfo.sevconSectionIDList;
		}
		private void copyInstallationFilesToUserDirectory()
		{
			FileSystemInfo [] fInfos;
			#region extract the allUser language into allUsrsLang
			string desktopFolder = null;
			string allUsrsconfigFilePath = null;
			object allUsersDesktop = "AllUsersDesktop";
			WshShell shell = new WshShellClass();
			try
			{
				// This is in a Try block in case AllUsersDesktop is not supported
				desktopFolder = shell.SpecialFolders.Item(ref allUsersDesktop).ToString();
				// .Net does not give us access to the All Users Desktop special folder,
				// but we can get this using the Windows Scripting Host.
				if (desktopFolder != null)
				{
					int temp = desktopFolder.IndexOf(@"\Desktop");
					allUsrsconfigFilePath = desktopFolder.Substring(0,temp);
                       
                    if (Environment.OSVersion.Version.MajorRevision > 5)
                    {
                        allUsrsconfigFilePath = allUsrsconfigFilePath + @"\AppData\Roaming\SEVCON\Drive Wizard\DWConfig.xml";   //tsn
                    }
                    else
                    {
                        allUsrsconfigFilePath = allUsrsconfigFilePath + @"\Application Data\SEVCON\Drive Wizard\DWConfig.xml";
                    }
				}
			}
			catch 
			{
				allUsrsconfigFilePath = null;
			} 
			
			string allUsrsLang = "";
			ObjectXMLSerializer DWconfigXMLSerializer;
			if((allUsrsconfigFilePath != null) && (System.IO.File.Exists(allUsrsconfigFilePath)==true))// we have and all users file
			{
				try
				{
					DWConfig AllUsrsDWConfigFile = new DWConfig();
					DWconfigXMLSerializer = new ObjectXMLSerializer();	
					AllUsrsDWConfigFile = (DWConfig) DWconfigXMLSerializer.Load(AllUsrsDWConfigFile, allUsrsconfigFilePath);
					allUsrsLang = AllUsrsDWConfigFile.DWlanguage.lang;
				}
				catch
				{
					allUsrsLang = "";

				}
			}
			#endregion extract the allUser language into allUsrsLang
			#region now load the current users own config file
			DWConfigFile = new DWConfig();
			DWconfigXMLSerializer = new ObjectXMLSerializer();	
			if(System.IO.File.Exists(MAIN_WINDOW.UserDirectoryPath + @"\DWConfig.xml") == true)  //only attempt to fill from file if the file exists
			{
				try
				{
					DWConfigFile = (DWConfig) DWconfigXMLSerializer.Load(DWConfigFile, (string) MAIN_WINDOW.UserDirectoryPath + @"\DWConfig.xml");
				}
				catch
				{
					DWConfigFile = new DWConfig();  //use default copy so we can save later
				}
			}
			#endregion now load the current users own config file
			#region ensure that the all users language overrides the current users language
			if((allUsrsLang != "") && (DWConfigFile.DWlanguage.lang !=allUsrsLang))
			{
				DWConfigFile.DWlanguage.lang = allUsrsLang;  //easiet to always force this to be the same
				//do not save here - we only want to overwirte the language - not everything else
				//DWconfigXMLSerializer.Save(MAIN_WINDOW.DWConfigFile, MAIN_WINDOW.UserDirectoryPath + @"\DWConfig.xml");
			}
			#endregion ensure that the all users language overrides the current users language
			#region create any required user directories that do not exist yet
			try
			{
				Directory.CreateDirectory(MAIN_WINDOW.UserDirectoryPath + @"\profiles");
				Directory.CreateDirectory(MAIN_WINDOW.UserDirectoryPath + @"\EDS");
				Directory.CreateDirectory(MAIN_WINDOW.UserDirectoryPath + @"\DCF");
				Directory.CreateDirectory(MAIN_WINDOW.UserDirectoryPath + @"\DLD");
				Directory.CreateDirectory(MAIN_WINDOW.UserDirectoryPath + @"\IDS");
				Directory.CreateDirectory(MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR");
			}
			catch{}
			#endregion create any required user directories that do not exist yet
			//we need to 'download' any files that were in the installation pack 
			//and have not yet made it to the users corresponding editable
			//directory. Note we set overwrite to false so a user can safely 
			//modify one of these files after the installaiton 

			//This is needed because mnimal users can only write to their own directory and when 
			//installing we cannot access ALL exising users never mind the future ones

			#region vehicle profiles
			if(Directory.Exists(MAIN_WINDOW.ApplicationDirectoryPath + @"\profiles"))
			{//will only exist if we have installed profiles
				string [] installedVehProfiles = Directory.GetFiles(MAIN_WINDOW.ApplicationDirectoryPath + @"\profiles", "*.xml");
				foreach(string installedvehicleProfile in installedVehProfiles)
				{
					string filename = installedvehicleProfile.Substring(installedvehicleProfile.LastIndexOf(@"\") + 1);
					if(System.IO.File.Exists(MAIN_WINDOW.UserDirectoryPath + @"\profiles\" + filename) == false)
					{
						try
						{
							System.IO.File.Copy(installedvehicleProfile,MAIN_WINDOW.UserDirectoryPath + @"\profiles\" + filename, false);
						}
						catch{}
					}
				}
			}
			#endregion vehicle profiles

			#region EDS and TreeView XML files
			string [] installedEDSs = Directory.GetFiles(MAIN_WINDOW.ApplicationDirectoryPath + @"\EDS", "*.eds");
			string [] installedXMLs = Directory.GetFiles(MAIN_WINDOW.ApplicationDirectoryPath + @"\EDS", "*.xml");
			foreach(string installedEDS in installedEDSs)
			{
				#region ensure that all EDS files that wer einstalled also exist in the user directory (user can edit these)
				string filename = installedEDS.Substring(installedEDS.LastIndexOf(@"\") + 1);
				if(System.IO.File.Exists(MAIN_WINDOW.UserDirectoryPath +@"\EDS\" + filename) == false)
				{
					try
					{
						System.IO.File.Copy(installedEDS,MAIN_WINDOW.UserDirectoryPath +@"\EDS\" + filename, false);
					}
					catch{}
				}
				#endregion ensure that all EDS files that wer einstalled also exist in the user directory (user can edit these)
			}
			foreach(string installedXML in installedXMLs)
			{
				#region ensure that all treeview XML files that were installed also exist in the user directory (user can edit these)
				string filename = installedXML.Substring(installedXML.LastIndexOf(@"\") + 1);
				if(System.IO.File.Exists(MAIN_WINDOW.UserDirectoryPath + @"\EDS\" + filename)== false)
				{
					try
					{
						System.IO.File.Copy(installedXML,MAIN_WINDOW.UserDirectoryPath + @"\EDS\" + filename, false);
					}
					catch{}
				}
				#endregion ensure that all treeview XML files that were installed also exist in the user directory (user can edit these)
			}
			#endregion EDS and TreeView XML files

			#region DCFs
			if(Directory.Exists(MAIN_WINDOW.ApplicationDirectoryPath + @"\DCF"))
			{
				fInfos = new DirectoryInfo(MAIN_WINDOW.ApplicationDirectoryPath + @"\DCF").GetFileSystemInfos();
				foreach(FileSystemInfo fileOrDir in fInfos)
				{
					if((fileOrDir.Extension.ToUpper() == ".DCF") 
						&& (System.IO.File.Exists(MAIN_WINDOW.UserDirectoryPath + @"\DCF\" + fileOrDir.Name) == false))
					{
						try
						{
							System.IO.File.Copy(fileOrDir.FullName, MAIN_WINDOW.UserDirectoryPath + @"\DCF\" + fileOrDir.Name, false);
						}
						catch{}
					}
				}
			}
			#endregion DCFs

			#region download files
			if(Directory.Exists(MAIN_WINDOW.ApplicationDirectoryPath + @"\DLD"))
			{
				fInfos = new DirectoryInfo(MAIN_WINDOW.ApplicationDirectoryPath + @"\DLD").GetFileSystemInfos();
				foreach(FileSystemInfo fileOrDir in fInfos)
				{
					if((fileOrDir.Extension.ToUpper() == ".DLD") 
						&& (System.IO.File.Exists(MAIN_WINDOW.UserDirectoryPath + @"\DLD\" + fileOrDir.Name) == false))
					{
						try
						{
							System.IO.File.Copy(fileOrDir.FullName,MAIN_WINDOW.UserDirectoryPath + @"\DLD\" + fileOrDir.Name, false);
						}
						catch{}
					}
				}
			}
			#endregion download files

			#region EventIDs file
			if(Directory.Exists(MAIN_WINDOW.ApplicationDirectoryPath + @"\IDS"))
			{
				fInfos = new DirectoryInfo(MAIN_WINDOW.ApplicationDirectoryPath + @"\IDS").GetFileSystemInfos();
				foreach(FileSystemInfo fileOrDir in fInfos)
				{
                    //DR38000256 ProductIDs.xml added (containing identifiers on product range and variant)
					if(((fileOrDir.Extension.ToUpper() == ".TXT") || ((fileOrDir.Extension.ToUpper() == ".XML")))
						&& (System.IO.File.Exists(MAIN_WINDOW.UserDirectoryPath + @"\IDS\" + fileOrDir.Name) == false))
					{
						try
						{
							System.IO.File.Copy(fileOrDir.FullName,MAIN_WINDOW.UserDirectoryPath + @"\IDS\" + fileOrDir.Name, false);
						}
						catch{}
					}
                }
			}

			#endregion Event IDS file

			#region self char files
#if EXTERNAL_SELF_CHAR
			if(Directory.Exists(MAIN_WINDOW.ApplicationDirectoryPath + @"\SELFCHAR"))
			{
				fInfos = new DirectoryInfo(MAIN_WINDOW.ApplicationDirectoryPath + @"\SELFCHAR").GetFileSystemInfos();
				foreach(FileSystemInfo fileOrDir in fInfos)
				{
					if(System.IO.File.Exists(MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\" + fileOrDir.Name) == false)
					{ //copy everything - ignore file extensions for Self char
						try
						{  
							System.IO.File.Copy(fileOrDir.FullName,MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\" + fileOrDir.Name, false);
						}
						catch{}
					}
				}
			}
#endif
			#endregion self char files

		}
		/// <summary>
		/// Creates an ArrayList of 
		/// </summary>
		private bool getAvailEDSInfo()
		{
			DIFeedbackCode feedback;
			MAIN_WINDOW.availableEDSInfo = new ArrayList();
			string [] EDSfilePaths = new string[0];
			if(Directory.Exists(MAIN_WINDOW.UserDirectoryPath + @"\EDS"))
			{
				DirectoryInfo dInfo = new DirectoryInfo(MAIN_WINDOW.UserDirectoryPath + @"\EDS");
				FileSystemInfo [] fileInfos = dInfo.GetFileSystemInfos();
				foreach(FileSystemInfo fileInfo in fileInfos)
				{
					if( ((fileInfo.Attributes & FileAttributes.Temporary) <=0)  //not ttmeporary file
						&& (fileInfo.Name.IndexOf("~") == -1) //not all temp files get marked correctly - if you open in word - these files aren't removed unitl you close WOrd - so this line IS needed
						&& (fileInfo.Extension.ToUpper() == ".EDS")  //correct extension
						&& ((fileInfo.Attributes & FileAttributes.Directory) <=0)) //not a directory
					{
						try
						{
							FileStream tempfs = new FileStream( fileInfo.FullName, System.IO.FileMode.Open, FileAccess.Read);
							StreamReader tempsr = new StreamReader( tempfs );
						}
						catch(Exception e)
						{
							//Message box is OK here since first failure causes DriveWizard to Exit
							SystemInfo.errorSB.Append("\nCannot access open file: " + fileInfo.FullName + "\nException: " + e.Message + "\nPlease close this file and restart DriveWizard");
							return false;
						}
						AvailableNodesWithEDS setupNode = new AvailableNodesWithEDS();
						EDSorDCF EDS_DCF = new EDSorDCF(null, this.sysInfo);
						feedback = EDS_DCF.open( fileInfo.FullName, FileAccess.Read,  0);
						uint vendorID, productCode, revNo;
						EDS_DCF.readDeviceInfo(fileInfo.FullName, out vendorID, out productCode, out revNo);
						this.sysInfo.DCFnode.setDeviceIdentity(vendorID, productCode, revNo);
						setupNode.EDSFilePath = fileInfo.FullName;
						setupNode.productName = EDS_DCF.EDSdeviceInfo.productName;
						setupNode.productNumber = EDS_DCF.EDSdeviceInfo.productNumber;
						setupNode.revisionNumber = EDS_DCF.EDSdeviceInfo.revisionNumber;
						setupNode.vendorName = EDS_DCF.EDSdeviceInfo.vendorName;
						setupNode.vendorNumber = EDS_DCF.EDSdeviceInfo.vendorNumber;
						if(feedback == DIFeedbackCode.DISuccess)
						{
							MAIN_WINDOW.availableEDSInfo.Add(setupNode);
						}
					}
				}
			}
			return true;
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: MAIN_WINDOW_Load
		///		 *  Description     : Event Handler for form load event
		///		 *  Parameters      : System event arguments 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		///		 
		internal void formatControls(System.Windows.Forms.Control topControl )
		{
			#region format individual controls
			topControl.ForeColor = SCCorpStyle.SCForeColour;
			topControl.Font = new System.Drawing.Font("Arial",8F);
			topControl.BackColor = SCCorpStyle.SCBackColour; 
			if ( topControl.GetType().Equals( typeof( Button ) ) ) 
			{
				topControl.BackColor = SCCorpStyle.buttonBackGround;
				Button btn = (Button) topControl;
				btn.FlatStyle = FlatStyle.Flat;
			}
			else if ( topControl.GetType().Equals( typeof( DataGrid ) ) ) 
			{
				DataGrid myDG = (DataGrid) topControl;
				this.sysInfo.formatDataGrid(myDG);
				myDG.Resize +=new EventHandler(myDG_Resize);
			}
			else if ( topControl.GetType().Equals( typeof( VScrollBar ) ) ) 
			{
				topControl.VisibleChanged +=new EventHandler(dataGridVScrollBar_VisibleChanged);
			}
			else if ( topControl.GetType().Equals( typeof( HScrollBar ) ) ) 
			{
				topControl.Height = 0;
			}
			else if ( topControl.GetType().Equals( typeof( GroupBox ) ) ) 
			{
				topControl.Font = new System.Drawing.Font("Arial", 8F, FontStyle.Bold);
			}
			else if ( topControl.GetType().Equals( typeof( Splitter ) ) ) 
			{
				topControl.BackColor = Color.Black;
			}
			else if ( topControl.GetType().Equals( typeof( Cursor ) ) ) 
			{
#if DEMO_MODE
				this.Cursor = new Cursor(GetType(), MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\cursors\pyramid.cur");
#endif
			}
			#endregion format individual controls
			foreach(Control control in topControl.Controls) 
			{
				formatControls(control);
			}
		}


		private void MAIN_WINDOW_Load(object sender, System.EventArgs e)
		{
			bool EDSsReadOK = getAvailEDSInfo();
			if(EDSsReadOK == false)
			{
				this.Close();
				return;
			}
			#region apply Sevcon Corporate Style to this Window
			//we need to recursivley hunt through all the controls and their children
			//To dig out all the control types that we are interested in
			foreach(Control c in this.Controls)
			{
				formatControls(c);
			}
			#endregion apply Sevcon Corporate Style to this Window
			formatColourPanels();
#if DEMO_MODE
			try
			{
				this.Cursor = new Cursor(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\cursors\BIGARROW.CUR");
			}
			catch
			{}
#endif
			this.blankPanel.BringToFront();
			this.UserInstructionsLabel.BringToFront();
			#region initial Status bar display
			Icon icon;
			this.statusBar1.Height = (int)(SCCorpStyle.statusBarHeight); //fixed height
			#region add status bar icons
			try
			{
				icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\notloggedIn.ico");
				this.statusBarPanel1.Icon = icon;
			}
			catch
			{
				//do nothing - we just don't display the icon
			}
			this.statusBarPanel2.Text = "No CANopen nodes found";
			try
			{
				icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\notConnected.ico");
				this.statusBarPanel2.Icon = icon;
			}
			catch{}	//do nothing - we just don't display the icon

			#endregion add status bar icons
			fillGraphingContextMenu();
			#endregion initial Status bar display
			#region size and centrally locate the form with respect to the client screen resolution and dimensions
			this.Width = Math.Min(1000, (int)(SystemInformation.WorkingArea.Width * 0.9)); //772, 667
			this.Height = Math.Min(900, (int)(SystemInformation.WorkingArea.Height * 0.9));

			int screenCentreX = (int) (SystemInformation.WorkingArea.Width/2);
			int screenCentreY = (int) (SystemInformation.WorkingArea.Height/2);
			
			this.Location = new	Point(screenCentreX-(this.Width/2), screenCentreY-(this.Height/2));
			this.progressBar1.Top = this.statusBar1.Top- this.progressBar1.Height;
			#endregion size and centrally locate the form with respect to the client screen resolution and dimensions

			actFaultsDGDefaultHeight = actFaultsDG.Height;
			connectedDevicesDGDefaultHeight = connectedDevicesDG.Height;
			dataGrid1DefaultHeight = this.dataGrid1.Height;
			devEmerDGDefaultHeight = this.devEmerDG.Height;
			emergencyDGDefaultHeight = this.emergencyDG.Height;
			#region setup emergentcy mewssage table before we listen in to messages
			emerAL = new ArrayList();
			emerTable = new EmergencyTable();
			emerTable.DefaultView.AllowNew = false;
			this.emergencyDG.Height = 	Math.Min(this.emergencyDGDefaultHeight,
				((emerTable.DefaultView.Count + 2) * emergencyDG.PreferredRowHeight)
				+ (emerTable.DefaultView.Count-1) + 4);
			this.emergencyDG.Height = Math.Min(this.emergencyDG.Height, 50);
			emerTable.DefaultView.ListChanged +=new ListChangedEventHandler(emerTable_DefaultView_ListChanged);
			this.emergencyDG.DataSource = emerTable.DefaultView;
			#endregion setup emergentcy mewssage table before we listen in to messages

		}
		private void fillGraphingContextMenu()
		{
			MenuItem graphContextMenu_MI1 = new MenuItem("Calibrated graphing (stop vehicle)");
			graphContextMenu_MI1.Click +=new EventHandler(graphContextMenu_Click);
			MenuItem graphContextMenu_MI2 = new MenuItem("Non-calibrated graphing");
			graphContextMenu_MI2.Click +=new EventHandler(graphContextMenu_Click);
			MenuItem graphContextMenu_MI3 = new MenuItem("Plot data  already stored in Monitoring file");
			graphContextMenu_MI3.Enabled = false;
			graphContextMenu_MI3.Click +=new EventHandler(graphContextMenu_Click);
			this.CMenuGraphingIcon.MenuItems.Add( (int) GraphTypes.CALIBRATED, graphContextMenu_MI1);
			this.CMenuGraphingIcon.MenuItems.Add((int) GraphTypes.NON_CALIBRATED, graphContextMenu_MI2); //DR38000267
			this.CMenuGraphingIcon.MenuItems.Add((int) GraphTypes.FROM_FILE, graphContextMenu_MI3);
		}
		#endregion initialisation

		private void updateGraphcontextMenu()
		{
			this.CMenuGraphingIcon.MenuItems[0].Enabled = this.monStore.existsInCurrentSystem;
			this.CMenuGraphingIcon.MenuItems[1].Enabled = this.monStore.existsInCurrentSystem;
            //only enable to plot from file if a file is opened AND it contains any data points
			this.CMenuGraphingIcon.MenuItems[2].Enabled = (this.monStore.fromFile && monStore.dataInFile); 
		}
		#region PC Hibernation related
		private void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
		{
			if ( e.Mode == Microsoft.Win32.PowerModes.Resume )
			{
				/* When a windows process message is a power broadcast message which
				 * indicates that the power is resuming after a suspend then we can
				 * assume the laptop went into hibernation mode and this turned off
				 * the USB-CAN dongle.  Only reset the adapter if it was previously
				 * up and running
				 */
                if (this.sysInfo.CANcomms.VCI.CANAdapterRunning == true) 
                {
                    this.blankPanel.BringToFront();
                    this.UserInstructionsLabel.BringToFront();
                    this.hideUserControls();
                    this.ToolsMenu.Enabled = false;
                    this.main_hlp_mi.Enabled = false;
                    this.file_mi.Enabled = false;
                    this.UserInstructionsLabel.Text = "Recovering from Windows hibernation. Please wait";
                    this.statusBarPanel3.Text = "Recovering from Windows Hibernation";
                    disableAllActiveOwnedForms();

                    #region restart CAN after hibernating thread
                    restartCANThread = new Thread( new ThreadStart(restartCANWrapper) ); 
                    restartCANThread.Name = "RestartCANAfterHibernation";
                    restartCANThread.IsBackground = true;
                    restartCANThread.Priority = ThreadPriority.Normal;
#if DEBUG
                    System.Console.Out.WriteLine("Thread: " + restartCANThread.Name + " started");
#endif
                    restartCANThread.Start();

                    timer2.Enabled = true;
                    #endregion
                }
			}
		}
		private void disableAllActiveOwnedForms()
		{
			if (this.options !=  null)
			{
				this.options.Enabled = false;
				this.options.Hide();
			}
			if(helpContact != null)
			{
				this.helpContact.Enabled = false;
				this.helpContact.Hide();
			}
			if(helpAbout != null)
			{
				this.helpAbout.Enabled = false;
				this.helpAbout.Hide();
			}
			if(DATA_MONITOR_FRM != null)
			{
				this.DATA_MONITOR_FRM.Enabled = false;
				DATA_MONITOR_FRM.Hide();
			}
			if(FAULT_LOG_FRM != null)
			{
				this.FAULT_LOG_FRM.Enabled = false;
				this.FAULT_LOG_FRM.Hide();
			}
			if(SYSTEM_LOG_FRM != null)
			{
				this.SYSTEM_LOG_FRM.Enabled = false;
				SYSTEM_LOG_FRM.Hide();
			}
			if(OP_LOGS_FRM != null)
			{
				this.OP_LOGS_FRM.Enabled = false;
				OP_LOGS_FRM.Hide();
			}
			if(COUNTERS_FRM != null)
			{
				this.COUNTERS_FRM.Enabled = false;
				COUNTERS_FRM.Hide();
			}
			if(SELF_CHAR_FRM != null)
			{
				this.SELF_CHAR_FRM.Enabled = false;
				SELF_CHAR_FRM.Hide();
			}
			if(CAN_BUS_CONFIG != null)
			{
				this.CAN_BUS_CONFIG.Enabled = false;
				CAN_BUS_CONFIG.Hide();
			}
			if(COB_PDO_FRM != null)
			{
				this.COB_PDO_FRM.Enabled = false;
				COB_PDO_FRM.Hide();
			}
			if(PROG_DEVICE_FRM != null)
			{
				this.PROG_DEVICE_FRM.Enabled = false;
				PROG_DEVICE_FRM.Hide();
			}
			if(selectProfile != null)
			{
				this.selectProfile.Enabled = false;
				selectProfile.Hide();
			}
			if(splashscreen != null)
			{
				this.splashscreen.Enabled = false;
				splashscreen.Hide();
			}
		}
		private void enableAllActiveOwnedForms()
		{
			if (this.options !=  null)
			{
				this.options.Enabled = true;
				this.options.Show();
			}
			if(helpContact != null)
			{
				this.helpContact.Enabled = true;
				this.helpContact.Show();
			}
			if(helpAbout != null)
			{
				this.helpAbout.Enabled = true;
				this.helpAbout.Show();
			}
			if(DATA_MONITOR_FRM != null)
			{
				this.DATA_MONITOR_FRM.Enabled = true;
				DATA_MONITOR_FRM.Show();
			}
			if(FAULT_LOG_FRM != null)
			{
				this.FAULT_LOG_FRM.Enabled = true;
				this.FAULT_LOG_FRM.Show();
			}
			if(SYSTEM_LOG_FRM != null)
			{
				this.SYSTEM_LOG_FRM.Enabled = true;
				SYSTEM_LOG_FRM.Show();
			}
			if(OP_LOGS_FRM != null)
			{
				this.OP_LOGS_FRM.Enabled = true;
				OP_LOGS_FRM.Show();
			}
			if(COUNTERS_FRM != null)
			{
				this.COUNTERS_FRM.Enabled = true;
				COUNTERS_FRM.Show();
			}
			if(SELF_CHAR_FRM != null)
			{
				this.SELF_CHAR_FRM.Enabled = true;
				SELF_CHAR_FRM.Show();
			}
			if(CAN_BUS_CONFIG != null)
			{
				this.CAN_BUS_CONFIG.Enabled = true;
				CAN_BUS_CONFIG.Show();
			}
			if(COB_PDO_FRM != null)
			{
				this.COB_PDO_FRM.Enabled = true;
				COB_PDO_FRM.Show();
			}
			if(PROG_DEVICE_FRM != null)
			{
				this.PROG_DEVICE_FRM.Enabled = true;
				PROG_DEVICE_FRM.Show();
			}
			if(selectProfile != null)
			{
				this.selectProfile.Enabled = true;
				selectProfile.Show();
			}
			if(splashscreen != null)
			{
				this.splashscreen.Enabled = true;
				splashscreen.Show();
			}
		}
		private void restartCANWrapper()
		{
			this.sysInfo.CANcomms.restartCAN(true);
		}
		#endregion PC Hibernation related

		#region User Controls enable/disable methods
		private void showUserControls()
		{
			this.treeView1.EndUpdate();
			this.treeView1.EndUpdate(); //for some unknown reason dotnet likes this to be called twice - first one is 'ignored' sometimes
			this.treeView1.Enabled = true; //in case it had been disabled
			this.progressBar1.Visible = false;
			this.UserInstructionsLabel.Text = "";
            ToolsMenu.Enabled = true;       //DR38000266
			if(SystemPDOsScreenActive == false)
			{
				this.statusBarPanel3.Text = "";
				#region RH datagrid screen being shown
				string tempStr = this.AllType1FormsClosed();
				if (this.COB_PDO_FRM != null)
				{
					tempStr = "System PDO configuration";
				}
				if(this.treeView1.SelectedNode != null)  //B&B\
				{
					this.UserInstructionsLabel.Visible = true;  //System PDOs panel hides it
					treeNodeTag selNodeTag = (treeNodeTag) this.treeView1.SelectedNode.Tag;
					if((this.treeView1.SelectedNode.Parent == this.SystemStatusTN) 
						&& (selNodeTag.tableindex <this.sysInfo.nodes.Length))
					{
						#region CAN Device root node sleelcted
						updateDevicePanel();

                        if (dataRetrievalThread == null)
                        {
                            nodeTag = selNodeTag;
                            dataRetrievalThread = new Thread(new ThreadStart(retreiveDevicePanelData));
                            dataRetrievalThread.Name = "Device Panel Data Retrieval";
                            dataRetrievalThread.IsBackground = true;
                            dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
                            System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif
                            dataRetrievalThread.Start();
                            this.timer2.Enabled = true;
                        }

						this.DeviceStatusPanel.BringToFront();
						#endregion CAN Devcie root node sleelcted
					}
					else if (this.treeView1.SelectedNode == DCFCustList_TN)
					{
						updateDCFStatusPanel();
					}
					else if ( this.treeView1.SelectedNode == this.SystemStatusTN)
					{
						#region system status node selected
						this.systemStatusPanel.BringToFront();
						#endregion system status node selected
					}
					else if ( this.treeView1.SelectedNode == this.GraphCustList_TN)
					{
						this.blankPanel.BringToFront();
					}
					else if ((this.treeView1.SelectedNode.Text == this.programmingTNText)
						|| (this.treeView1.SelectedNode.Text == this.SelfCharMotor1Text)
						|| (this.treeView1.SelectedNode.Text == this.SelfCharMotor2Text))
					{
						if(tempStr != "")
						{
							//do nothing - do not allow forceDataGridResize to be called
							this.treeView1.Enabled = false;
							this.statusBarPanel3.Text = "Node selection disabled while " +  tempStr + " is active ";
						}
					}
					else
					{
						#region all other treenodes
						this.dataGridPanel.BringToFront();
						//the following two lines are necesarry 
						//- otherwise number of visible rows in datagrid is not updated 
						//and an empty datagrid is displayed
						this.dataGrid1.Invalidate();
						this.dataGrid1.Update();
						if(tempStr == "")
						{//re-allow user full control to enter data change NMT stat eetc
#if AUTOVALIDATE
						this.autoValidateMI.Visible = true;
#endif
							this.submitBtn.Visible = false;
							this.keyPanel.Visible = false;
							if(this.dataGrid1.VisibleRowCount>0)
							{
								if(MAIN_WINDOW.currTblIndex<this.sysInfo.nodes.Length)
								{
									this.submitBtn.Visible = true;
								}
								else
								{
									this.submitBtn.Visible = false;
								}
								this.keyPanel.Visible = true;
							}
							if(this.monitorTimerWasRunning == true)
							{
								this.monitorTimerWasRunning = false;
								this.dataMonitoringTimer.Enabled = true;
							}
						}
						else
						{//we have a type 1 form open - so limit user changes to OD/screen
							this.treeView1.Enabled = false;
							this.statusBarPanel3.Text = "Node selection disabled while " +  tempStr + " is active ";
						}
						#endregion all other treenodes
						this.EnsureVisibleWithoutRightScrolling(this.treeView1.SelectedNode); 
						this.forceDataGridResize(this.dataGrid1); 
					}
				}
				this.UserInstructionsLabel.BringToFront();
				#endregion RH datagrid screen being shown
			}
			else if (this.SystemPDOsScreenActive == true) 
			{
				#region COB/PDOs screen active
				foreach(Control cont in this.SystemPDOsPanel.Controls)
				{
					cont.ResumeLayout();
				}
				foreach(Control cont in this.SystemPDOsPanel.Controls)
				{
					cont.Refresh();
				}
				this.PDORoutingPanel.Invalidate();
				this.UserInstructionsLabel.Visible = false; 
				this.SystemPDOsPanel.BringToFront();
				#endregion COB/PDOs screen active
			}
			if(statusBarOverrideText != "")
			{
				this.statusBarPanel3.Text = statusBarOverrideText;
				statusBarOverrideText = "";
			}

            //don't re-enable user input if we're still retrieving data
            if (dataRetrievalThread == null)
            {
                MAIN_WINDOW.UserInputInhibit = false;  //do this before switching timer back on - allows timer to catch up quicker
            }

            if (timer3WasEnabled == true)
			{
				this.timer3.Enabled = true;
				timer3WasEnabled = false; //B&B
			}

            //If SearchForm is open, re-enable the user to select the next search.
            if (SEARCH_FRM != null)
            {
                ((SearchForm)this.SEARCH_FRM).enableNextSearch();
                searchInProgress = false;
			}

			if(SystemInfo.errorSB.Length>0)
			{
				this.sysInfo.displayErrorFeedbackToUser("Unable to retrieve all associated data");
			}
		}
		private void hideUserControls()
		{
			MAIN_WINDOW.UserInputInhibit = true;
			//- so switch off the timer until we are done
			if(this.timer3.Enabled == true)
			{
				timer3WasEnabled = true;
			}
			timer3.Enabled = false;
			this.blankPanel.BringToFront();
			this.UserInstructionsLabel.BringToFront();
			this.UserInstructionsLabel.Text = "Please wait...";
			if(this.dataMonitoringTimer.Enabled == true)
			{
				this.monitorTimerWasRunning = true;
				this.dataMonitoringTimer.Enabled = false;
			}
			this.treeView1.BeginUpdate();
			if(this.DCFCustList_TN != null)
			{
				this.DCFCustList_TN.BackColor = this.treeView1.BackColor;  //back to default = for drag Drop
			}
			if(this.GraphCustList_TN != null)
			{
				this.GraphCustList_TN.BackColor = this.treeView1.BackColor;  //back to default
			}
			this.statusBarPanel3.Text = "";  //reset any use guidance
			this.progressBar1.Value = this.progressBar1.Minimum;
			this.progressBar1.Visible = true;
			this.submitBtn.Visible = false;
			this.keyPanel.Visible = false;

            #region if search form open, stop user initiating more searches until user controls re-enabled
            if (SEARCH_FRM != null)
            {
                ((SearchForm)this.SEARCH_FRM).disableNextSearch();
            }
            #endregion if search form open, stop user initiating more searches until user controls re-enabled

            ToolsMenu.Enabled = false;  //DR38000266 prevent repeated F5-Refresh
#if AUTOVALIDATE
			this.autoValidateMI.Visible = false;
#endif
			if(this.SystemPDOsScreenActive == true)
			{
				foreach(Control cont in this.SystemPDOsPanel.Controls)
				{
					cont.SuspendLayout();
				}
			}
		}
		private void updateDevicePanel()
		{
			if(this.treeView1.SelectedNode == null) 
			{
				return;
			}
			treeNodeTag selNodeTag = (treeNodeTag) this.treeView1.SelectedNode.Tag;
			this.SWVrLbl.Text = SWVersions[selNodeTag.tableindex];
			this.HWVerLbl.Text = HWVersions[selNodeTag.tableindex];
			this.productCodeLbl.Text = this.productCodes[selNodeTag.tableindex];
			this.RevNumberLbl.Text = this.RevNos[selNodeTag.tableindex];
			this.VendorIDLbl.Text = this.VendorIDs[selNodeTag.tableindex];
			this.vendorNameLbl.Text = this.VendorNames[selNodeTag.tableindex];
			this.serialNoLbl.Text = this.serialNos[selNodeTag.tableindex];
			this.IntROMChksumLbl.Text = this.IntROMChksum[selNodeTag.tableindex];
			this.ExtROMChksumLbl.Text = this.extROMChksum[selNodeTag.tableindex];
			this.configChkSumLbl.Text = this.configChkSum[selNodeTag.tableindex];
			#region Master/Slave indication
			if(this.sysInfo.nodes[selNodeTag.tableindex].masterStatus == true)
			{
				MasterSlaveLbl.Text = "Master/Slave: Master";
			}
			else
			{
				if(this.sysInfo.nodes[selNodeTag.tableindex].isSevconApplication() == true)
				{
					MasterSlaveLbl.Text = "Master/Slave: Slave";
				}
				else
				{
					MasterSlaveLbl.Text = "Master/Slave: Unknown";
				}
			}
			#endregion Master/Slave indication
			this.NMTStateLbl.Text = "NMT state: " + this.sysInfo.nodes[selNodeTag.tableindex].nodeState.ToString();
			#region login status
			if(this.sysInfo.nodes[selNodeTag.tableindex].isSevconApplication() == true)
			{
				int userLvl = Math.Min((int)(this.sysInfo.nodes[selNodeTag.tableindex].accessLevel), 5);  //limit it to 5
				loginLbl.Text = "Login: " + MAIN_WINDOW.UserIDs[userLvl].ToString() + " (" + this.sysInfo.nodes[selNodeTag.tableindex].accessLevel.ToString() + ")";
			}
			else
			{
				loginLbl.Text = "Login: " + "n/a";
			}
			#endregion login status
			if(this.sysInfo.nodes[selNodeTag.tableindex].isSevconApplication() == true)
			{
				if(selNodeTag.tableindex <ActFaultsDS.Tables.Count )
				{
					devActFaultstablestyle.MappingName = ActFaultsDS.Tables[selNodeTag.tableindex].TableName;
					this.actFaultsDG.DataSource = ActFaultsDS.Tables[selNodeTag.tableindex].DefaultView;
				}
				this.actFaultsDG.Show();
			}
			else
			{
				this.actFaultsDG.Hide();
			}
			//the device emergency 
			DataView devEmerDV = new DataView(this.emerTable);
			devEmerDV.AllowNew = false;
			string nodeStr = this.sysInfo.nodes[selNodeTag.tableindex].nodeID.ToString();
			devEmerDV.RowFilter = "NodeID = '" + nodeStr + "'";
			this.devEmerDG.DataSource = devEmerDV;
			try
			{
                if ((selNodeTag.tableindex < this.sysInfo.nodes.Length) && (this.sysInfo.nodes[selNodeTag.tableindex].isSevconApplication() == true))
                {
                    // display previously found values until the dataRetrievalThread reads new values
                    if ((selNodeTag.tableindex < this.sysInfo.nodes.Length) && (this.sysInfo.nodes[selNodeTag.tableindex].isSevconApplication() == true))
                    {
                        // set labels to last known data until data thread retrieves latest values
                        if (sysInfo.nodes[selNodeTag.tableindex].battVoltSub != null)
                        {
                            double tempFlt = this.sysInfo.nodes[selNodeTag.tableindex].battVoltSub.currentValue * this.sysInfo.nodes[selNodeTag.tableindex].battVoltSub.scaling;
                            this.BattVoltLbl.Text = "Battery Voltage: \t" + tempFlt.ToString("F1") + " volts";                            
                        }
                        else
                        {
                            this.BattVoltLbl.Text = "Battery Voltage: \tNot available";
                        }
                        if (sysInfo.nodes[selNodeTag.tableindex].capvoltSub != null)
                        {
                            double tempFlt = this.sysInfo.nodes[selNodeTag.tableindex].capvoltSub.currentValue * this.sysInfo.nodes[selNodeTag.tableindex].capvoltSub.scaling;
                            this.CapVoltLbl.Text = "Capacitor Voltage: \t" + tempFlt.ToString("F1") + " volts";
                        }
                        else
                        {
                            this.CapVoltLbl.Text = "Capacitor Voltage: \tNot available";
                        }
                        if (sysInfo.nodes[selNodeTag.tableindex].temperatureSub != null)
                        {
                            double tempFlt = this.sysInfo.nodes[selNodeTag.tableindex].temperatureSub.currentValue * this.sysInfo.nodes[selNodeTag.tableindex].temperatureSub.scaling;
                            this.temperatureLbl.Text = "Temperature: \t" + tempFlt.ToString("F1") + " degrees C";
                        }
                        else
                        {
                            this.temperatureLbl.Text = "Temperature: \tNot available";
                        }
                        if (sysInfo.nodes[selNodeTag.tableindex].configChksumSub != null)
                        {
                            this.configChkSum[selNodeTag.tableindex] = "Config Chksum: " + "0x" + this.sysInfo.nodes[selNodeTag.tableindex].configChksumSub.currentValue.ToString("X").PadLeft(4, '0');
                            this.configChkSumLbl.Text = this.configChkSum[selNodeTag.tableindex];
                        }
                        else
                        {
                            this.configChkSumLbl.Text = "Not available";
                        }

                        DevSevconGB.Show();
                        serviceDueLbl.Text = serviceDue[selNodeTag.tableindex];
                        serviceDueLbl.Show();
                        this.configChkSumLbl.Visible = true;
                        this.IntROMChksumLbl.Visible = true;
                        this.ExtROMChksumLbl.Visible = true;
                    }
                    else
                    {
                        this.configChkSumLbl.Visible = false;  //can only extract using programming transfer
                        this.IntROMChksumLbl.Visible = false;
                        this.ExtROMChksumLbl.Visible = false;
                        DevSevconGB.Hide();
                        serviceDueLbl.Hide();
                    }
                }
			}
			catch(Exception ex1)
			{
				SystemInfo.errorSB.Append("Exception in updateDevicePanel(): " + ex1.Message + " " + ex1.InnerException);
			}
		}
		private void updateDCFStatusPanel()
		{
			if(this.DCFCustList_TN.Nodes.Count>0)
			{
				if(this.DCFFromfile == true)
				{
					this.DCFfileInfoGB.Show();
				}
				else
				{
					this.DCFfileInfoGB.Hide();
				}
				this.DCFStatusPanel.BringToFront();
			}
			else  //no DCF source - nothing to display
			{
				this.blankPanel.BringToFront();
			}
		}
		#endregion User Controls enable/disable methods

		#region help system rleated methods
		private void topics_hlp_mi_Click(object sender, System.EventArgs e)
		{
			string helpFile = ApplicationDirectoryPath + @"/DWHelp.chm";
			HtmlHelp(this.Handle,helpFile , HH_DISPLAY_TOC, 0);
		}


		private void about_hlp_mi_Click(object sender, System.EventArgs e)
		{
			if(helpAbout == null)
			{
				helpAbout = new HELP_ABOUT_WINDOW(DW_Version, DW_Release, VersionDescription);
				helpAbout.BackColor = SCCorpStyle.SCBackColour;
				helpAbout.ForeColor = SCCorpStyle.SCForeColour;
				this.helpAbout.Closed += new System.EventHandler(this.helpAboutclosed);
				helpAbout.Height = (int)this.Height/2;
				helpAbout.Width = (int)this.Height/2; 
				helpAbout.Show();
			}		
		}

		private void helpAboutclosed(object sender, System.EventArgs e)
		{
			helpAbout  = null;
		}
		private void helpContactClosed(object sender, System.EventArgs e)
		{
			helpContact  = null;
		}
		#endregion help system rleated methods

		#region finalisation/exit	
		private void file_exit_mi_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		private void MAIN_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			mainWindowClosing = true;
			
			for(int i = 0;i<this.sysInfo.nodes.Length;i++)
			{
				if(this.sysInfo.nodes[i].EVASRequired == true)
				{
					this.statusBarPanel3.Text = "Requesting EEPROM Save for Communications profile on Node ID " +this.sysInfo.nodes[i].nodeID.ToString(); 
					this.sysInfo.nodes[i].saveCommunicationParameters();  
					this.sysInfo.nodes[i].EVASRequired = false;
				}
			}
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("EEPROM save command Failed\n");
			}
			if(this.OwnedForms.Length>0)
			{
				for(int i = 0;i<this.OwnedForms.Length;i++)
				{
					this.OwnedForms[i].Close();
				}
			}
			#region abort any secondary threads
			#region restart CAN thread
			if ( restartCANThread != null )
			{
				try
				{
					restartCANThread.Abort();
				}
				catch {}  //do nothing
#if DEBUG
				if(restartCANThread.IsAlive == true)
				{
					SystemInfo.errorSB.Append("Failed to close Thread: " + restartCANThread.Name + " on exit");
				}
#endif	
			}
			#endregion restart CAN thread
			if(dataRetrievalThread != null)
			{
				try
				{
					dataRetrievalThread.Abort();
				}
				catch {}  //do nothing
#if DEBUG
				if(dataRetrievalThread.IsAlive == true)
				{
					SystemInfo.errorSB.Append("\nFailed to close Thread: " + dataRetrievalThread.Name + " on exit");
				}
#endif	
			}
			#endregion abort any secondary threads
			#region disable any timers
			this.timer2.Enabled = false;
			this.timer3.Enabled = false;
			this.splashTimer.Enabled = false;
			this.dataMonitoringTimer.Enabled = false;
			#endregion disable any timers
            if (this.sysInfo != null)
            {
                this.sysInfo.Dispose();
            }
            //save all application levle configuration settings
            ObjectXMLSerializer DWconfigXMLSerializer = new ObjectXMLSerializer();
            DWconfigXMLSerializer.Save(MAIN_WINDOW.DWConfigFile, MAIN_WINDOW.UserDirectoryPath + @"\DWConfig.xml");
			if(e.Cancel == true)
			{
#if DEBUG
				Message.Show("Application not closing");
#endif
				e.Cancel = false;
			}
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion
	
		#region treeview and table filling methods
		private void setupAndDisplayData()
		{
			//from this point in we are interested in state hcanges to connecte dnodes
			// and new nodes joining the bus
			this.sysInfo.addStateChangeListener( new StateChangeListener( stateChangeListener ) );
			this.SystemPDOsScreenActive = false;  //reset here
			this.SevconMasterNode = null;
			for(int i = 0;i<this.sysInfo.nodes.Length;i++)
			{ //we also need to check this whenever user changes this value
				this.sysInfo.nodes[i].identifyCommonlyAccessedOdSubs();  //setr up pointers to items we access regularly eg emergency
				if((this.sysInfo.nodes[i].isSevconApplication() == true ) 
					&& (this.sysInfo.nodes[i].masterStatus == true))
				{
					this.SevconMasterNode = this.sysInfo.nodes[i];
				}
			}
			this.hideUserControls();
			this.statusBarPanel3.Text = "Creating data structures";
			DCFTblIndex = (int) this.sysInfo.nodes.Length;
			GraphTblIndex = DCFTblIndex + 1;
			compTblIndex = GraphTblIndex + 1;
			if(DCFCompareActive == true)
			{
				DCFCompareActive = false; //prevents error scenario - we MUST recalculate number of compare cols when reconnecting since this rleates to number of nodes
				foreach(Control c in this.dataGrid1.Controls)
				{ //switch off the horizontal scroll bar
					if	(c.GetType().Equals( typeof( HScrollBar ) ) ) 
					{
						c.Height = 0;
						break;
					}
				}
			}

            if (dataRetrievalThread == null)
            {
                nodeTag = null;     //don't use it here
                dataRetrievalThread = new Thread(new ThreadStart(readDataForDevicePanels));
                dataRetrievalThread.Name = "First Device Panel Data Retrieval";
                dataRetrievalThread.IsBackground = true;
                dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
                System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif
                dataRetrievalThread.Start();
                this.timer2.Enabled = true;
            }
            
            if (dataRetrievalThread == null)
            {
                MAIN_WINDOW.UserInputInhibit = false;
                this.ToolsMenu.Enabled = true;

                //disable find and refresh menu items if there are no nodes connected
                if (treeView1.Nodes.Count > 0)
                {
                    this.FindMI.Enabled = true;
                    this.miRefreshData.Enabled = true;
                }
                else
                {
                    this.FindMI.Enabled = false;
                    this.miRefreshData.Enabled = false;
                }

                this.file_mi.Enabled = true;
                this.main_hlp_mi.Enabled = true;
                this.timer3.Enabled = true; //allow LED flashing etc
                this.treeView1.EndUpdate();//leave in - otherwise sometimes tree view stays blank 
            }
            
			//- possibly every beginUpdate has to be mathced to an endUpdate ???
			#region update icon image
			try
			{
				Bitmap myimage = new Bitmap (MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\seeRO.png");
				this.PBHideShowRO.Image = myimage;
			}
			catch
			{
				SystemInfo.errorSB.Append("\nFailed to open icon file: ");
				SystemInfo.errorSB.Append(MAIN_WINDOW.ApplicationDirectoryPath);
				SystemInfo.errorSB.Append( @"\Resources\icons\seeRO.png");
				SystemInfo.errorSB.Append("file missing, re-install DriveWizard");
			}
			#endregion update icon image
		}

		private void fillSystemStatusPanel()
		{
			#region Connected devices display

			connDevTable = new EstCommsTable();
			connDevTable.DefaultView.AllowNew = false;
			for (int i = 0;i< this.sysInfo.nodes.Length;i++)
			{

				DataRow row = connDevTable.NewRow();
				row[(int)CANCommsCol.Type] = this.sysInfo.nodes[i].EDS_DCF.EDSdeviceInfo.productName;
				row[(int)CANCommsCol.Manuf] = this.sysInfo.nodes[i].EDS_DCF.EDSdeviceInfo.vendorName;
				row[(int)CANCommsCol.NodeID] = this.sysInfo.nodes[i].nodeID.ToString();
				if(this.sysInfo.nodes[i].masterStatus == true)
				{
					row[(int)CANCommsCol.Master] = "Master";
				}
				row[(int)CANCommsCol.NMTState] = this.sysInfo.nodes[i].nodeState.ToString();
				connDevTable.Rows.Add(row);
			}
			connDevTable.DefaultView.AllowNew = false;
			try
			{ //this line causes and exception when we hit connect and then immediately minimise DriveWizard
				//yrt catch masks this and seemt ocuase no ill effects - 
				this.connectedDevicesDG.DataSource = connDevTable.DefaultView;
			}
			catch (Exception e)
			{
				string test = e.Message;
				//judetempMessage.Show(e.InnerException.Message);
			}
			connDevTable.DefaultView.ListChanged +=new ListChangedEventHandler(connDevTable_DefaultView_ListChanged);
			connectedDevicesDG.Height = Math.Min(this.connectedDevicesDGDefaultHeight,((connDevTable.DefaultView.Count + 2) * connectedDevicesDG.PreferredRowHeight) + (connDevTable.DefaultView.Count-1) + 4);
			createConnectedDevicesTableStyle();

			#endregion Connected devices display
		}
		private void createConnectedDevicesTableStyle()
		{
			int [] ColWidths  = new int[connPercents.Length];
			ColWidths  = this.sysInfo.calculateColumnWidths(connectedDevicesDG, connPercents, 0, connectedDevicesDGDefaultHeight);
			CANCommsTableStyle conntablestyle = new CANCommsTableStyle(ColWidths, true);
			this.connectedDevicesDG.TableStyles.Clear();
			this.connectedDevicesDG.TableStyles.Add(conntablestyle);

			ColWidths  = new int[emerPercents.Length];
			ColWidths  = this.sysInfo.calculateColumnWidths(emergencyDG, emerPercents, 0, emergencyDGDefaultHeight);
			EmerTableStyle emertablestyle = new EmerTableStyle(ColWidths);
			this.emergencyDG.TableStyles.Clear();
			this.emergencyDG.TableStyles.Add(emertablestyle);

			ColWidths  = new int[devEmerPercents.Length];
			ColWidths  = this.sysInfo.calculateColumnWidths(devEmerDG, devEmerPercents, 0, devEmerDGDefaultHeight);
			devEmerTableStyle devemertablestyle = new devEmerTableStyle(ColWidths);
			this.devEmerDG.TableStyles.Clear();
			this.devEmerDG.TableStyles.Add(devemertablestyle);

			ColWidths  = new int[devActFaultsPercents.Length];
			ColWidths  = this.sysInfo.calculateColumnWidths(actFaultsDG, devActFaultsPercents, 0, actFaultsDGDefaultHeight);
			devActFaultstablestyle = new actFualtsTableStyle(ColWidths);
			this.actFaultsDG.TableStyles.Clear();
			this.actFaultsDG.TableStyles.Add(devActFaultstablestyle);
		}
		private void fillTreeView()
		{
			#region clear out the TreeView add System node
			this.treeView1.Nodes.Clear();
			SystemStatusTN = new TreeNode("System", (int) TVImages.FLT,(int) TVImages.FLT);
			SystemStatusTN.Tag = new treeNodeTag(TNTagType.SYSTEMSTATUS,150 ,20, null);
			this.treeView1.Nodes.Add(this.SystemStatusTN);  //will always be in 
			#endregion clear out the TreeView add System node
			DWdataset = new DataSet("DWdataset");  //effectively clears out any existing tables
			this.dataGrid1.TableStyles.Clear();   //we are going to refill these - so clear to prevent duplicate mapping names
			try
			{
				this.actFaultsDG.TableStyles.Clear(); //we are going to refill these - so clear to prevent duplicate mapping names
			}
			catch(Exception e)
			{
				Message.Show("Excpetion: " +  e.Message + " inner: " + e.InnerException.Message);
			}
			this.dataGrid1.TableStyles.Clear(); //we must clear her eto ensure we refernec the correct one when finding it by index No - eg DCF ocmpare
			for(int tblNum= 0;tblNum<(sysInfo.nodes.Length + noOfCustomLists);tblNum++)
			{
				DWdataset.Tables.Add(new DWdatatable());
				DWdataset.Tables[tblNum].TableName = "table" + tblNum.ToString();
				if(tblNum<sysInfo.nodes.Length)
				{
					this.statusBarPanel3.Text = "Locating XML Tree for node " + sysInfo.nodes[tblNum].nodeID.ToString();
					#region DataTables for all the real or Virtual CAN Devcies
					DIFeedbackCode feedback = this.sysInfo.nodes[tblNum].findMatchingXMLFile();
                    if (feedback != DIFeedbackCode.DISuccess)
                    {
                        //DR38000265 Use a default XML file so that only the root node appears in the tree view
                        sysInfo.nodes[tblNum].XMLfilepath = MAIN_WINDOW.UserDirectoryPath.ToString() + @"\EDS\" + SCCorpStyle.DefaultXMLFile;
                    }

					//Load the customer object from the existing XML file (if any)...
					if ( System.IO.File.Exists( sysInfo.nodes[tblNum].XMLfilepath ) == true )
					{
						#region extract the treeview contents from file
						//not the most efficient method - we already did this to get this nodes mathcing xmlfile
						SevconTree treeObject = new SevconTree();
						ObjectXMLSerializer objectXMLSerializer = new ObjectXMLSerializer();
						try
						{
							treeObject = ( SevconTree ) objectXMLSerializer.Load( treeObject, sysInfo.nodes[tblNum].XMLfilepath );
						}
						catch( Exception e )
						{
							SystemInfo.errorSB.Append("\nFailed to load Tree Nodes from file");
							SystemInfo.errorSB.Append(sysInfo.nodes[tblNum].XMLfilepath);
							SystemInfo.errorSB.Append("Exception: ");
							SystemInfo.errorSB.Append(e.Message);
							treeObject = null;
						}
						this.statusBarPanel3.Text = "Reading XML Tree for node " + this.sysInfo.nodes[tblNum].nodeID.ToString();
						loadXMLTreeIntoForm( treeObject, tblNum ,sysInfo.nodes[tblNum]);  //use zero & defaults - for compatibility with DCf and Monitor list
						this.statusBarPanel3.Text = "Removing Dead End Nodes from XML Tree for node" + this.sysInfo.nodes[tblNum].nodeID.ToString();
                        do
						{
                            nodesRemovedFlag = false;  //set false before entry
                            //if we remove any node sin this pass then it will be set to true
                            removeRedundantSecionNodes(this.SystemStatusTN.Nodes[tblNum], tblNum);
						}
						while (nodesRemovedFlag == true);  //chekc at end set inside method
						#endregion extract the treeview contents from file
					}
					else
					{
						SystemInfo.errorSB.Append("\nFailed to load Tree Nodes from file" );
						SystemInfo.errorSB.Append(sysInfo.nodes[tblNum].XMLfilepath);
						SystemInfo.errorSB.Append("File Missing");
					}
					#endregion DataTables for all the real orVirtual CAN Devcies
				}
				else if (tblNum == MAIN_WINDOW.GraphTblIndex)
				{
					#region add monitoring store
					this.statusBarPanel3.Text = "Creating Monitoring store node";
					//add the list of currently seleted items
					GraphCustList_TN = new TreeNode("Monitoring store (empty)", (int) TVImages.Monitor, (int) TVImages.Monitor);
					GraphCustList_TN.Tag = new treeNodeTag(TNTagType.CANNODE,0,GraphTblIndex, null);  //custom lists have node Id of 130 offset
					this.treeView1.Nodes.Add(GraphCustList_TN);
					DataColumn[] keys = new DataColumn[3];
					//monitoring list can have multiple sources - hence it requires NodeId as a primary key
					keys[0] = DWdataset.Tables[tblNum].Columns[TblCols.Index.ToString()];
					keys[1] = DWdataset.Tables[tblNum].Columns[TblCols.sub.ToString()];
					keys[2] = DWdataset.Tables[tblNum].Columns[TblCols.NodeID.ToString()];  
					DWdataset.Tables[tblNum].PrimaryKey = keys;
					#endregion add monitoring store
				}
				else if (tblNum == MAIN_WINDOW.DCFTblIndex)
				{
					this.statusBarPanel3.Text = "Creating DCF store node";
					#region add DCF Custom List
					DCFCustList_TN  = new TreeNode("DCF store (empty)", (int) TVImages.UsrCust, (int) TVImages.UsrCust );
					DCFCustList_TN.Tag = new treeNodeTag(TNTagType.CANNODE,0, DCFTblIndex, null);  //use nodeId of 131 to denote this one later
					this.treeView1.Nodes.Add(DCFCustList_TN);
					#endregion add DCF Custom List
				}
				//sort by Index then sub - especially useful when adding out of order subs to DCF/Monitoring Store 
				DWdataset.Tables[tblNum].DefaultView.Sort = TblCols.Index.ToString() + "," + TblCols.sub.ToString();  //converts from EDS order to ordered by index number
				DWdataset.Tables[tblNum].DefaultView.AllowNew = false;
				DWdataset.Tables[tblNum].AcceptChanges();  //confirm and row changes before addid a change handler
				uint access = this.sysInfo.systemAccess;
				if(tblNum<this.sysInfo.nodes.Length)
				{
					if((this.sysInfo.nodes[tblNum].vendorID == SCCorpStyle.SevconID)
						&& ((this.sysInfo.nodes[tblNum].productVariant == SCCorpStyle.bootloader_variant)
						|| (this.sysInfo.nodes[tblNum].productVariant == SCCorpStyle.selfchar_variant_new)
						|| (this.sysInfo.nodes[tblNum].productVariant == SCCorpStyle.selfchar_variant_old)))
					{
						access = 5;	
					}
				}
				this.createPPTableStyle(DWdataset.Tables[tblNum].TableName, access);  //mapping name automatically hooks the correct tables style to the datasource
			}
			this.dataGrid1.PerformLayout(); 
			//add tree Nodes for hooks to other windows that are not necessarily in the XML file eg Programming
			updateDeviceLevelOtherWindowTreenodes();
			updateSystemLevelOtherWindowTreenodes();
			//add System level Tree Nodes eg CAN bus Configuration
			createComparisonTable();
			if(compNodeIDs.Count>0)
			{
				createDCFCompareTableStyle();
			}
		}
		private void updateDeviceLevelOtherWindowTreenodes()
		{
            TreeNode tempNode = null;
            for (int CANDeviceIndex = 0; CANDeviceIndex < sysInfo.nodes.Length; CANDeviceIndex++)
            {
                if (SystemStatusTN.Nodes.Count <= CANDeviceIndex)
                {
                    return; //problem num of CNA nodes in tree should equal those in sysInfo
                }
                #region self char
                #region determine whether we want a Self Char Node to be displayed
                bool selfCharEnabled = false;
                if ((sysInfo.nodes[CANDeviceIndex].isSevconApplication() == true)
                    && (this.sysInfo.systemAccess >= SCCorpStyle.AccLevel_SelfChar)
                    && (this.sysInfo.nodes[CANDeviceIndex].nodeState == NodeState.PreOperational))
                {
                    selfCharEnabled = true;
                }
                #endregion determine whether we want a Self Char Node to be displayed
                #region check if this device already has a motor 1 self char TreeNode
                TreeNode SCtreenode = new TreeNode(this.SelfCharMotor1Text, (int)TVImages.SelfChar, (int)TVImages.SelfChar);
                SCtreenode.Tag = new treeNodeTag(TNTagType.SELFCHARSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);
                if ((selfCharEnabled == true) && (this.motorProfilesPresent[CANDeviceIndex, 0] != 0)) //we want a self char treenode
                //|| (this.sysInfo.nodes[CANDeviceIndex].deviceVersion.productCode == SCCorpStyle.selfchar_variant_old)) //judetemp for faster self char debugging
                {
                    bool SCPresent = false;

                    for (int i = 0; i < this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Count; i++)
                    {
                        tempNode = this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i];
                        if (tempNode.Text == SCtreenode.Text)
                        {
                            SCPresent = true;
                            break;
                        }
                    }
                    //DR38000265 only display tree node if not already present & XML file found for device
                    if ((SCPresent == false) && (sysInfo.nodes[CANDeviceIndex].XMLFileMatchFound == true))
                    {
                        this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Add(SCtreenode);
                    }
                }
                else  //we don't want a Self Char motor 1 node - if we find one then remove it
                {
                    for (int i = 0; i < this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Count; i++)
                    {
                        tempNode = this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i];
                        if (tempNode.Text == SCtreenode.Text)
                        {
                            this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Remove(tempNode);
                            break;
                        }
                    }
                }
                #endregion check if this device already has a self char motor 1 TreeNode
                #region check if this device already has a motor 2 self char TreeNode
                SCtreenode = new TreeNode(this.SelfCharMotor2Text, (int)TVImages.SelfChar, (int)TVImages.SelfChar);
                SCtreenode.Tag = new treeNodeTag(TNTagType.SELFCHARSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);
                if ((selfCharEnabled == true) && (this.motorProfilesPresent[CANDeviceIndex, 1] != 0)) //we want a self char treenode
                {
                    bool SCPresent = false;
                    for (int i = 0; i < this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Count; i++)
                    {
                        tempNode = this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i];
                        if (tempNode.Text == SCtreenode.Text)
                        {
                            SCPresent = true;
                            break;
                        }
                    }
                    //DR38000265 only show if not already present & matching XML file found
                    if ((SCPresent == false) && (sysInfo.nodes[CANDeviceIndex].XMLFileMatchFound == true))
                    {
                        this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Add(SCtreenode);
                    }
                }
                else  //we don't want a Self Char motor 2 node - if we find one then remove it
                {
                    for (int i = 0; i < this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Count; i++)
                    {
                        tempNode = this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i];
                        if (tempNode.Text == SCtreenode.Text)
                        {
                            this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Remove(tempNode);
                            break;
                        }
                    }
                }
                #endregion check if this device already has a self char motor 2 TreeNode
                #endregion self char
                #region programming
                #region determine whether we want a programming node to be displayed
                bool reProgrammingEnabled = false;
                if (
                    (sysInfo.nodes[CANDeviceIndex].manufacturer == Manufacturer.SEVCON)
                    && (((sysInfo.nodes[CANDeviceIndex].isSevconApplication() == true)
                    && (sysInfo.nodes[CANDeviceIndex].accessLevel >= SCCorpStyle.AccLevel_Programming)
                    && (sysInfo.nodes[CANDeviceIndex].nodeState == NodeState.PreOperational))
                    || (sysInfo.nodes[CANDeviceIndex].productRange == (byte)SCCorpStyle.ProductRange.SEVCONDISPLAY)
                    || (sysInfo.nodes[CANDeviceIndex].productRange == (byte)SCCorpStyle.ProductRange.CALIBRATOR) //DR38000256
                    || (sysInfo.nodes[CANDeviceIndex].productVariant == SCCorpStyle.bootloader_variant)
                    || (sysInfo.nodes[CANDeviceIndex].productVariant == SCCorpStyle.selfchar_variant_old)
                    || (sysInfo.nodes[CANDeviceIndex].productVariant == SCCorpStyle.selfchar_variant_new)))
                {
                    reProgrammingEnabled = true;
                }
                #endregion determine whether we want a programming node to be displayed
                #region add or remove the reProgramming node as necessary
                TreeNode reProgNode = new TreeNode(programmingTNText, (int)TVImages.ReProgram, (int)TVImages.ReProgram);
                reProgNode.Tag = new treeNodeTag(TNTagType.PROGSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);
                if (reProgrammingEnabled == true) //we want this node in the tree
                {
                    bool reProgPresent = false;
                    for (int i = 0; i < this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Count; i++)
                    {
                        tempNode = SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i];
                        if (tempNode.Text == reProgNode.Text)
                        {
                            reProgPresent = true;
                            break;
                        }
                    }
                    //DR38000265 don't show the re-programming node if already present or a proper XML file match wasn't found
                    if ((reProgPresent == false) && (sysInfo.nodes[CANDeviceIndex].XMLFileMatchFound == true))
                    {
                        SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Add(reProgNode);
                    }
                }
                else  //prevent rePorgramming - if node is present then remove it
                {
                    for (int i = 0; i < this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Count; i++)
                    {
                        tempNode = SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i];
                        if (tempNode.Text == reProgNode.Text)
                        {
                            SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Remove(tempNode);
                            break;
                        }
                    }
                }
                #endregion add or remove the reProgramming node as necessary
                #endregion programming
                #region Logs
                if (sysInfo.nodes[CANDeviceIndex].isSevconApplication() == true)
                {
                    #region Logs node
                    TreeNode LogsNode = null;
                    for (int i = 0; i < this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Count; i++)
                    {
                        if (this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i].Text == LogsTNText)
                        {
                            // remove existing logs node (inherited from XML) if insufficient login access
                            if (this.sysInfo.nodes[CANDeviceIndex].accessLevel < SCCorpStyle.ReadAccessLevelMin)
                            {
                                this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i].Remove();
                            }
                            else
                            {
                                this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i].ImageIndex = (int)TVImages.triangles3Col;
                                this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i].SelectedImageIndex = (int)TVImages.triangles3Col;
                                //now see if we have already linked this treenode to a member of LogsTN
                                int LogsNodeIndex = 99;
                                for (int j = 0; j < this.LogsTN.Count; j++)
                                {
                                    LogsNode = (TreeNode)LogsTN[j];
                                    if (this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i] == LogsNode)
                                    {
                                        LogsNodeIndex = CANDeviceIndex;
                                        break;
                                    }
                                }
                                if (LogsNodeIndex == 99)
                                {
                                    LogsNode = this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes[i];
                                    LogsTNText = LogsNode.Text;  //keep a copy of the text with its casing for comparison later
                                    LogsNode.Tag = new treeNodeTag(TNTagType.LOGS, sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);  //Belt and Braces
                                    LogsTN.Add(LogsNode);
                                }
                                break;
                            }
                        }
                    }
                    if ((LogsNode == null)
                        && (this.sysInfo.nodes[CANDeviceIndex].hasLogs == true)
   						&& (this.sysInfo.nodes[CANDeviceIndex].accessLevel >= SCCorpStyle.ReadAccessLevelMin))
                    {
                        LogsNode = new TreeNode("Logs", (int)TVImages.logs, (int)TVImages.logs);
                        LogsNode.Tag = new treeNodeTag(TNTagType.LOGS, sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);
                        this.SystemStatusTN.Nodes[CANDeviceIndex].Nodes.Add(LogsNode);
                        LogsTN.Add(LogsNode);
                        LogsTNText = LogsNode.Text;  //keep a copy of the text with its casing for comparison later
                    }

                    #endregion Logs node
                    //so now we have a TreeNode Logs in the treeVIew and in the Array list
                    if (LogsNode != null)
                    {
                        #region Fault Log node
                        if (((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.PST)
                            && ((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.UNKNOWN))
                        {
                            for (int i = 0; i < LogsNode.Nodes.Count; i++)
                            {
                                if (LogsNode.Nodes[i].Text.ToUpper() == "FAULT LOG")
                                {
                                    tempNode = LogsNode.Nodes[i];
                                    LogsNode.Nodes[i].Tag = new treeNodeTag(TNTagType.FAULTLOGSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);  //B7B
                                    break;
                                }
                            }
                            if (tempNode == null)
                            {
                                tempNode = new TreeNode(FaultLogTNText, (int)TVImages.DefaultIco, (int)TVImages.DefaultIco);
                                tempNode.Tag = new treeNodeTag(TNTagType.FAULTLOGSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);
                                LogsNode.Nodes.Add(tempNode);
                            }
                            tempNode.ImageIndex = (int)TVImages.logs;
                            tempNode.SelectedImageIndex = (int)TVImages.logs;
                        }
                        #endregion Fault Log node
                        #region System Log node
                        if (((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.SEVCONDISPLAY)
                            && ((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.CALIBRATOR) //DR38000256
                            && ((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.PST)
                            && ((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.UNKNOWN))
                        {
                            tempNode = null;
                            for (int i = 0; i < LogsNode.Nodes.Count; i++)
                            {
                                if (LogsNode.Nodes[i].Text.ToUpper() == "SYSTEM LOG")
                                {
                                    tempNode = LogsNode.Nodes[i];
                                    LogsNode.Nodes[i].Tag = new treeNodeTag(TNTagType.SYSTEMLOGSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);  //B7B
                                    break;
                                }
                            }
                            if (tempNode == null)
                            {
                                tempNode = new TreeNode(SystemLogTNText, (int)TVImages.DefaultIco, (int)TVImages.DefaultIco);
                                tempNode.Tag = new treeNodeTag(TNTagType.SYSTEMLOGSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);
                                LogsNode.Nodes.Add(tempNode);
                            }
                            tempNode.ImageIndex = (int)TVImages.logs;
                            tempNode.SelectedImageIndex = (int)TVImages.logs;
                        }
                        #endregion System Log node
                        #region Op Log Node
                        if (((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.SEVCONDISPLAY)
                            && ((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.CALIBRATOR) //DR38000256
                            && ((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.PST)
                            && ((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.UNKNOWN))
                        {
                            tempNode = null;
                            for (int i = 0; i < LogsNode.Nodes.Count; i++)
                            {
                                if (LogsNode.Nodes[i].Text.ToUpper() == "OPERATIONAL LOG")
                                {
                                    tempNode = LogsNode.Nodes[i];
                                    LogsNode.Nodes[i].Tag = new treeNodeTag(TNTagType.OPLOGSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);  //B7B
                                    break;
                                }
                            }
                            if (tempNode == null)
                            {
                                tempNode = new TreeNode(OpLogTNText, (int)TVImages.DefaultIco, (int)TVImages.DefaultIco);
                                tempNode.Tag = new treeNodeTag(TNTagType.OPLOGSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);
                                LogsNode.Nodes.Add(tempNode);
                            }
                            tempNode.ImageIndex = (int)TVImages.logs;
                            tempNode.SelectedImageIndex = (int)TVImages.logs;
                        }
                        #endregion Op Log Node
                        #region Counters Log node
                        if (((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.SEVCONDISPLAY)
                            && ((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.CALIBRATOR) //DR38000256
                            && ((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.PST)
                            && ((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange != SCCorpStyle.ProductRange.UNKNOWN))
                        {
                            tempNode = null;
                            for (int i = 0; i < LogsNode.Nodes.Count; i++)
                            {
                                if (LogsNode.Nodes[i].Text.ToUpper() == "EVENT LOG")
                                {
                                    tempNode = LogsNode.Nodes[i];
                                    LogsNode.Nodes[i].Tag = new treeNodeTag(TNTagType.COUNTERLOGSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);  //B7B
                                    break;
                                }
                            }
                            if (tempNode == null)
                            {
                                tempNode = new TreeNode(CountersLogTNText, (int)TVImages.DefaultIco, (int)TVImages.DefaultIco);
                                tempNode.Tag = new treeNodeTag(TNTagType.COUNTERLOGSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);
                                LogsNode.Nodes.Add(tempNode);
                            }
                            tempNode.ImageIndex = (int)TVImages.logs;
                            tempNode.SelectedImageIndex = (int)TVImages.logs;
                        }
                        #endregion Counters Log node
                        #region Data Log node
                        if ((SCCorpStyle.ProductRange)sysInfo.nodes[CANDeviceIndex].productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY)
                        {
                            tempNode = null;
                            for (int i = 0; i < LogsNode.Nodes.Count; i++)
                            {
                                if (LogsNode.Nodes[i].Text.ToUpper() == "DATA LOG")
                                {
                                    tempNode = LogsNode.Nodes[i];
                                    LogsNode.Nodes[i].Tag = new treeNodeTag(TNTagType.DATALOGSSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);  //B7B
                                    break;
                                }
                            }
                            if (tempNode == null)
                            {
                                tempNode = new TreeNode(this.DataLogTNText, (int)TVImages.DefaultIco, (int)TVImages.DefaultIco);
                                tempNode.Tag = new treeNodeTag(TNTagType.DATALOGSSCREEN, this.sysInfo.nodes[CANDeviceIndex].nodeID, CANDeviceIndex, null);
                                LogsNode.Nodes.Add(tempNode);
                            }
                            tempNode.ImageIndex = (int)TVImages.logs;
                            tempNode.SelectedImageIndex = (int)TVImages.logs;
                        }
                        #endregion Counters Log node
                    }
                }
                #endregion Logs
            }
		}
		private void updateSystemLevelOtherWindowTreenodes()
		{
			TreeNode tempNode = null;
			#region External PDO mappping
			//Allow entry into externall PDO mapping providing that at least one CAN node
			//is either a third party or a SEVCON application node AND this node is in pre-op
			bool extPDOEnabled = false;
			for(int i = 0;i< this.sysInfo.nodes.Length;i++)
			{
				if ( ((this.sysInfo.nodes[i].isSevconApplication() == true) 
					&& (this.sysInfo.systemAccess>=SCCorpStyle.ReadAccessLevelMin)
					&& (this.sysInfo.nodes[i].nodeState == NodeState.PreOperational))
					|| (this.sysInfo.nodes[i].manufacturer == Manufacturer.THIRD_PARTY) )
				{
					extPDOEnabled = true;
					break;
				}
			}
			if(extPDOEnabled == true)
			{
				bool COBandPDOsTNPresent = false;
				for (int i = 0;i< this.SystemStatusTN.Nodes.Count;i++)
				{
					tempNode = this.SystemStatusTN.Nodes[i];
					if(tempNode.Text == COBandPDOsTN.Text)
					{
						COBandPDOsTNPresent =true;
						break;
					}
				}
				if(COBandPDOsTNPresent == false)
				{
					this.SystemStatusTN.Nodes.Add(COBandPDOsTN);
				}
			}
			else 
			{
				for (int i = 0;i< this.SystemStatusTN.Nodes.Count;i++)
				{
					tempNode = this.SystemStatusTN.Nodes[i];
					if(tempNode.Text == COBandPDOsTN.Text)
					{
						this.SystemStatusTN.Nodes.Remove(tempNode);
						break;
					}
				}
			}
			#endregion PDO mapping
			#region CAN Bus configuration
			bool CANBusConfigEnabled = false;
			if(this.sysInfo.nodes.Length == 0) 
			{
				CANBusConfigEnabled = true;
			}
			else
			{
				if (sysInfo.systemAccess >= SCCorpStyle.AccLevel_SevconCANbusConfig)
				{
					CANBusConfigEnabled = true;
					for(int nodeInd = 0;nodeInd<this.sysInfo.nodes.Length;nodeInd++)
					{//every SEVCON node nust be in pre-op chat enter the CAN bus configuration screen
						if((sysInfo.nodes[nodeInd].isSevconApplication() == true) 
							&& (sysInfo.nodes[nodeInd].nodeState != NodeState.PreOperational))
						{
							CANBusConfigEnabled = false;  // first non-Sevcion Application -> we inhibit treenode display and get out
							break;
						}
					}
				}
			}
			if( CANBusConfigEnabled == true)
			{  //must do full path here - the clone being used fo rhiding/showing read only items 
				//means that we 'lose' our instance of named TreeNdoes - since we ar eeffectively holding two 
				//copies of a single instance of CANBUsConfigTN etc
				bool CANBUsConfigTNPresent = false;
				for (int i = 0;i< this.SystemStatusTN.Nodes.Count;i++)
				{
					tempNode = this.SystemStatusTN.Nodes[i];
					if(tempNode.Text == this.CANBUsConfigTN.Text)
					{
						CANBUsConfigTNPresent = true;
						break;
					}
				}
				if(CANBUsConfigTNPresent == false)
				{
					this.SystemStatusTN.Nodes.Add(CANBUsConfigTN);
				}

			}
			else
			{
				for (int i = 0;i< this.SystemStatusTN.Nodes.Count;i++)
				{
					tempNode = this.SystemStatusTN.Nodes[i];
					if(tempNode.Text == this.CANBUsConfigTN.Text)
					{
						this.SystemStatusTN.Nodes.Remove(tempNode);
						break;
					}
				}
			}
			#endregion CAN Bus configuration

		}

		private void loadXMLTreeIntoForm(SevconTree treeView, int tableIndex , nodeInfo CANdevice)
		{
			
			TreeNode root; // the tree node representing the CANdevice or DCF or Monitoring
			#region create the device (or DCF) root TreeNode
			if(tableIndex == MAIN_WINDOW.DCFTblIndex)
			{
				#region DCF root node
				String rootString = CANdevice.deviceType + "(NodeID " + CANdevice.nodeID.ToString() + ")";
				root = new TreeNode(rootString, (int) TVImages.slOffOff, (int) TVImages.slOffOff);
				root.Tag = new treeNodeTag(TNTagType.CANNODE,CANdevice.nodeID,tableIndex, null);  //for CAN Nodes the odItem parameter becomes the Index for the Nod ein th eSystemm Inofr Ndodes array
				#endregion DCF root node
			}
			else if (tableIndex == MAIN_WINDOW.GraphTblIndex)
			{
				#region Monitoring List root node
				String rootString = CANdevice.deviceType + "(NodeID " + CANdevice.nodeID.ToString() + ")";
				root = new TreeNode(rootString, (int) TVImages.slOffOff, (int) TVImages.slOffOff);
				root.Tag = new treeNodeTag(TNTagType.CANNODE,CANdevice.nodeID, tableIndex, null);  //for CAN Nodes the odItem parameter becomes the Index for the Nod ein th eSystemm Inofr Ndodes array
				#endregion Monitoring List root node
			}
			else
			{
				#region CAN device Root node
				//node ID is needed to force all root node descriptions to be unique
				String rootString = CANdevice.deviceType + "(NodeID " + CANdevice.nodeID.ToString() + ")";
				if(CANdevice.masterStatus == true)  //ie this is a master
				{  //assume NTM stopped or bootup at start, and assume no faults
					root = new TreeNode(rootString, (int) TVImages.msOffOn, (int) TVImages.msOffOn);
				}
				else  //slave - use grey bounded icon
				{
					root = new TreeNode(rootString, (int) TVImages.slOffOn, (int) TVImages.slOffOn);
				}
				root.Tag = new treeNodeTag(TNTagType.CANNODE,CANdevice.nodeID, tableIndex, null);  //for CAN Nodes the odItem parameter becomes the Index for the Nod ein th eSystemm Inofr Ndodes array

				//judetemp - add an unassigned treenode to catch any EDS objects that are not in XML or Drive Wizard
				TreeNode TNnone = new TreeNode("Unassigned", (int) TVImages.UsrCust, (int) TVImages.UsrCust);
				TNnone.Tag = new treeNodeTag(TNTagType.XMLHEADER,CANdevice.nodeID, tableIndex, null);
				root.Nodes.Add(TNnone);
				//judetemp end
				#endregion CAN device Root node
			}
			#endregion create the device (or DCF) root TreeNode
			if(   (tableIndex == MAIN_WINDOW.GraphTblIndex)
				|| (tableIndex == MAIN_WINDOW.DCFTblIndex)
				|| (CANdevice.manufacturer == Manufacturer.SEVCON) 
				|| (CANdevice.manufacturer == Manufacturer.THIRD_PARTY))
			{
				#region fill TreeView with nodes
				this.statusBarPanel3.Text = "Reading XML sections";
#if DEBUG
                if (CANdevice.manufacturer == Manufacturer.SEVCON)
                {
                    #region get all sevcon SECTIONS that don't appear in the the XML tree
                    ArrayList sectionsNotInXML = new ArrayList();
                    for (int i = 0; i < sysInfo.sevconSectionIDList.Count; i++)
                    {
                        if (SevconSectionAppearsInXMLTree(treeView.treeStruct, sysInfo.sevconSectionIDList[i].ToString()) == false)
                        {
                            sectionsNotInXML.Add(sysInfo.sevconSectionIDList[i]);
                        }
                    }
                    #endregion get all sevcon SECTIONS that don't appear in the the XML tree

                    #region re-assign all objects with unshown sevcon SECTIONS to UNASSIGNED
                    //converting to unassigned means they're shown under "unassigned" tree node 
                    //in debug instead of dropping off the face of the earth 
                    //DR38000262 But deliberately let SECURITY drop off so  the password won't be displayed on DW
                    foreach (ObjDictItem odItem in CANdevice.objectDictionary)
                    {
                        ODItemData firstODsub = (ODItemData)odItem.odItemSubs[0];
                        int index = sectionsNotInXML.IndexOf(firstODsub.sectionTypeString);
                        if ((sectionsNotInXML.IndexOf(firstODsub.sectionTypeString) > 0)
                            && (firstODsub.sectionTypeString != SevconSectionType.SECURITY.ToString())) //DR38000262
                        {
                            firstODsub.sectionType = (int)SevconSectionType.UNASSIGNED;
                        }
                    }
                    #endregion re-assign all objects with unshown sevcon SECTIONS to UNASSIGNED
                }
#endif
				convertXMLToTreeNodes(treeView.treeStruct, root.Nodes, CANdevice);
				#endregion fill TreeView with nodes
			}
			#region add the correct root node
			if(tableIndex == MAIN_WINDOW.DCFTblIndex)
			{
				this.DCFCustList_TN.Nodes.Add(root);
			}
			else if (tableIndex == MAIN_WINDOW.GraphTblIndex)
			{
				if(this.GraphCustList_TN.Nodes.Contains(root) == false)
				{
					this.GraphCustList_TN.Nodes.Add(root);
				}
			}
			else
			{
				this.SystemStatusTN.Nodes.Add(root);
			}
			#endregion add the root node
		}

        private bool SevconSectionAppearsInXMLTree(XMLTreeLevel topObjNode, string sevconSection)
        {
            bool appearsInTree = false;

            foreach (XMLTreeLevel objNode in topObjNode.nextLevelAL)
            {
                if (objNode.nextLevelAL.Count > 0)
                {
                    if (objNode.nextLevelAL.Count == 1)
                    {
                        #region EDS section level
                        string temp = ((XMLTreeLevel)objNode.nextLevelAL[0]).Title.ToUpper();
                        XMLTreeLevel childObjNode = (XMLTreeLevel)objNode.nextLevelAL[0];
                        if (childObjNode.nextLevelAL.Count == 0)
                        {
                            if (temp == sevconSection)
                            {
                                return (true);
                            }
                        }
                        #endregion EDS section level
                    }
                    appearsInTree = SevconSectionAppearsInXMLTree(objNode, sevconSection);

                    if (appearsInTree == true)
                    {
                        return(appearsInTree);
                    }
                }
            }

            return (appearsInTree);
        }
		private void convertXMLToTreeNodes(XMLTreeLevel topObjNode, TreeNodeCollection treeNodes, nodeInfo CANdevice)
		{
			this.statusBarPanel3.Text = "Connecting OD items to XML tree";
			CANdevice.hasLogs = false;
			foreach(XMLTreeLevel objNode in topObjNode.nextLevelAL)
			{
				TreeNode tNode = null;
				if(objNode.nextLevelAL.Count>0)
				{
					if(objNode.Title == this.nodeCOBShortCutsText)
					{
						tNode = new TreeNode(objNode.Title,  (int) TVImages.COBExtPDO, (int) TVImages.COBExtPDO );
						tNode.Tag = new treeNodeTag( TNTagType.COBSCREEN, CANdevice.nodeID, CANdevice.nodeOrTableIndex , null);
					}
					else if(objNode.nextLevelAL.Count >1)
					{
						if(objNode.Title.ToUpper() == "LOGS")
						{
							CANdevice.hasLogs = true;
						}
						#region add multi EDS section tree node
						tNode = new TreeNode( objNode.Title, (int) TVImages.triangles3Col, (int) TVImages.triangles3Col ); //XML Section
						tNode.Tag = new treeNodeTag(TNTagType.XMLHEADER, CANdevice.nodeID, CANdevice.nodeOrTableIndex , null);
						foreach (XMLTreeLevel nextlvlnode in objNode.nextLevelAL)
						{
							if(nextlvlnode.nextLevelAL.Count == 0)
							{
								hookODItemsUnderSectionTreeNodes( nextlvlnode.Title.ToUpper(), tNode , CANdevice); //hook up with the Sevcon or CANopen sections from EDS
							}
						}
						#endregion add multi EDS section tree node
					}
					else if(objNode.nextLevelAL.Count ==1) 
					{
						#region EDS section level
						string temp = ((XMLTreeLevel)objNode.nextLevelAL[0]).Title.ToUpper();
						tNode = new TreeNode( objNode.Title, (int) TVImages.triangles1Col, (int) TVImages.triangles1Col ); //XML Section
						tNode.Tag = new treeNodeTag(TNTagType.EDSSECTION, CANdevice.nodeID, CANdevice.nodeOrTableIndex , null);
						XMLTreeLevel childObjNode = (XMLTreeLevel) objNode.nextLevelAL[0];
						if(childObjNode.nextLevelAL.Count == 0)
						{
							hookODItemsUnderSectionTreeNodes( temp, tNode , CANdevice); //hook up with the Sevcon or CANopen sections from EDS
						}
						#endregion EDS section level
					}
					convertXMLToTreeNodes(objNode, tNode.Nodes, CANdevice);
					treeNodes.Add(tNode);
				}
			}
		}
		private void hookODItemsUnderSectionTreeNodes( string passed_EDSSection, TreeNode parentNode, nodeInfo CANdevice)
		{
            this.progressBar1.Value = this.progressBar1.Minimum;
            this.progressBar1.Maximum = CANdevice.objectDictionary.Count;
            treeNodeTag parentTag = (treeNodeTag)parentNode.Tag;
            if (CANdevice.manufacturer == Manufacturer.SEVCON)
            {
                int SevconSection = (int)SevconSectionType.NONE;
                #region determine sevcon section type
                try
                {
                    SevconSection = sysInfo.sevconSectionIDList.IndexOf(passed_EDSSection);
                }
                catch
                {
                    //if we dont recognise the XML section then nothing we can do - DO NOT MARK IT UNASSIGNED!
                    // should never occur now changed over to dynamic extensible array
#if DEBUG
                    SystemInfo.errorSB.Append("\n Unrecognised EDS section node in XML file: ");
                    SystemInfo.errorSB.Append(passed_EDSSection);
#endif
                    return;
                }
                #endregion determine sevcon section type
                foreach (ObjDictItem odItem in CANdevice.objectDictionary)
                {
                    #region hook every OD itme in the CANnode under its correct TreeView node
                    this.progressBar1.Value++;
                    ODItemData firstODsub = (ODItemData)odItem.odItemSubs[0];
                    #region attempt to match a SevconSectionType in the EDS to an EDSSection from the XML file
                    if (firstODsub.sectionType == SevconSection)
                    {
                        #region skip if node is slave and item is Master display only (but not for DCF)& user opted not to show anyway
                        //DR38000263
                        if ( (CANdevice.nodeOrTableIndex < this.sysInfo.nodes.Length) && (firstODsub.displayOnMasterOnly == true)
                            && (CANdevice.masterStatus == false) && (MAIN_WINDOW.showMasterObjectsOnSlave == false) )
                        {
#if DEBUG
                            //always show unassigned objects
                            if (passed_EDSSection != "UNASSIGNED")
                            {
                                continue;  //move to next one
                            }
#endif
                        }
                        #endregion skip if node is slave and item is MAster dispaly only ( but not for DCF)
                        #region skip if user is not logged in and header row access ( set in DI) is>0
                        if (((CANdevice.nodeOrTableIndex < this.sysInfo.nodes.Length)
                            || ((CANdevice.nodeOrTableIndex == MAIN_WINDOW.DCFTblIndex) && (this.sysInfo.DCFnode.DCFChecksumOK == false))
                            )
                            && (this.sysInfo.systemAccess == 0)
                            && (firstODsub.accessLevel > 0)
                            )
                        {
#if DEBUG
                            //always show unassigned objects
                            if (passed_EDSSection != "UNASSIGNED")
                            {
                                continue;  //do not add heeade ror its subs
                            }
#endif
                        }
                        #endregion skip if user is not logged in and header row access ( set in DI) is>0
                        bool nonHeaderAdded = false;
                        DataRow headerRow = null;
                        TreeNode headerTNode = null;
                        foreach (ODItemData odSub in odItem.odItemSubs)
                        {
                            #region fill table rows
                            #region add tree nodes and decide whther rows should be added for subindexes
                            if (odSub == firstODsub)  //could be header if length >1 or could be real OD item = either way add as a Tree Node
                            {
                                #region create OD item node and add to tree
                                //this column has default of "" -> which is OK for a header row
                                headerTNode = new TreeNode(firstODsub.parameterName, (int)TVImages.triangleSingle, (int)TVImages.triangleSingle);
                                headerTNode.Tag = new treeNodeTag(TNTagType.ODITEM, parentTag.nodeID, parentTag.tableindex, ((ODItemData)odItem.odItemSubs[0]));
                                parentNode.Nodes.Add(headerTNode);
                                #endregion create OD item node and add to tree
                            }
                            if ((CANdevice.nodeOrTableIndex < this.sysInfo.nodes.Length)
                                && ((odSub.accessType == ObjectAccessType.Constant) || (odSub.accessType == ObjectAccessType.ReadOnly)))
                            {
                                if (odSub.accessLevel > CANdevice.accessLevel)
                                {
                                    continue;
                                }
                            }
                            if ((CANdevice.nodeOrTableIndex < this.sysInfo.nodes.Length)
                                && (CANdevice.accessLevel == 0)
                                && (odSub.accessLevel > 0))
                            {
                                continue;  //don't add row
                            }
                            if ((odSub.subNumber == 0) && (odSub.isNumItems == true))
                            {
                                continue;  //this has to be a 'number of entries' sub zero 
                            }
                            #endregion  add tree nodes and decide whther rows should be added for subindexes
                            #region create a table row from the DI dictionary data and, if OK, add to table
                            DataRow row = MAIN_WINDOW.DWdataset.Tables[CANdevice.nodeOrTableIndex].NewRow();
                            bool rowAddInhibit = false;
                            if ((CANdevice.nodeOrTableIndex == MAIN_WINDOW.DCFTblIndex) && (this.DCFFromfile == true))
                            {
                                rowAddInhibit = fillTableRowColumns(odSub, row, CANdevice.nodeOrTableIndex, true, true, CANdevice);
                            }
                            else
                            {
                                rowAddInhibit = fillTableRowColumns(odSub, row, CANdevice.nodeOrTableIndex, true, false, CANdevice);
                            }
                            if (rowAddInhibit == false)
                            {
                                if (odSub.subNumber != -1)
                                {
                                    nonHeaderAdded = true;
                                }
                                else
                                {
                                    headerRow = row;
                                }
                                //if(MAIN_WINDOW.DWdataset.Tables[CANdevice.nodeOrTableIndex].Rows.Contains(row) == false)
                                try
                                {
                                    MAIN_WINDOW.DWdataset.Tables[CANdevice.nodeOrTableIndex].Rows.Add(row);
                                    row.AcceptChanges();
                                }
                                catch (Exception e)
                                {
                                    #region error feedback
                                    SystemInfo.errorSB.Append("\nException when adding Row to node id:");
                                    SystemInfo.errorSB.Append(CANdevice.nodeID.ToString());
                                    SystemInfo.errorSB.Append(" item: ");
                                    SystemInfo.errorSB.Append(odSub.parameterName);
                                    SystemInfo.errorSB.Append(" (index Ox");
                                    SystemInfo.errorSB.Append(odSub.indexNumber.ToString("X").PadLeft(4, '0'));
                                    SystemInfo.errorSB.Append(" subIndex 0x");
                                    SystemInfo.errorSB.Append(odSub.subNumber.ToString("X"));
                                    SystemInfo.errorSB.Append(" Exception message:");
                                    SystemInfo.errorSB.Append(e.Message);
                                    SystemInfo.errorSB.Append(" , ");
                                    SystemInfo.errorSB.Append(e.InnerException);
                                    #endregion error feedback
                                }
                            }
                            #endregion create a table row from th edI dictionary data and if OK add to table
                            #endregion fill table rows
                        }
                        if ((nonHeaderAdded == false) && (headerRow != null))
                        {
                            #region remobve any isolated header rows
                            //take the header row and assoc TreeNode back out 
                            MAIN_WINDOW.DWdataset.Tables[CANdevice.nodeOrTableIndex].Rows.Remove(headerRow);
                            parentNode.Nodes.Remove(headerTNode);
                            #endregion remobve any isolated header rows
                        }
                    }
                    #endregion attempt to match a SevconSectionType in the EDS to an EDSSection from the XML file
                    #endregion hook every OD itme in the CANnode under its correct TreeView node
                }
            }
            else if (CANdevice.manufacturer == Manufacturer.THIRD_PARTY)
            {
                #region determine Third party section type
                CANSectionType ThirdPartySection = CANSectionType.NONE;
                try
                {
                    ThirdPartySection = (CANSectionType)Enum.Parse(typeof(CANSectionType), passed_EDSSection);
                }
                catch
                {
                    return; //nothing we can do 
                }
                #endregion determine Third party section type
                foreach (ObjDictItem odItem in CANdevice.objectDictionary)
                {
                    #region hook every OD itme in the CANnode under its correct TreeView node
                    this.progressBar1.Value++;
                    ODItemData firstODSub = (ODItemData)odItem.odItemSubs[0];
                    #region match generic CAN Open sections
                    foreach (ODItemData odSub in odItem.odItemSubs)
                    {
                        #region fill table rows
                        #region add tree nodes and decide whther rows should be added for subindexes
                        if (odSub == firstODSub)  //could be header if length >1 or could be real OD item = either way add as a Tree Node
                        {
                            #region create OD item node and add to tree
                            //this column has default of "" -> which is OK for a header row
                            TreeNode headerTNode = new TreeNode(firstODSub.parameterName, (int)TVImages.triangleSingle, (int)TVImages.triangleSingle);
                            headerTNode.Tag = new treeNodeTag(TNTagType.ODITEM,
                                parentTag.nodeID,
                                parentTag.tableindex, ((ODItemData)odItem.odItemSubs[0]));
                            parentNode.Nodes.Add(headerTNode);
                            #endregion create OD item node and add to tree
                        }
                        if ((odItem.odItemSubs.IndexOf(odSub) == 1) && (odSub.isNumItems == true)) //sub index 0 -> may be just number of items 
                        {
                            continue;  //this has to be a 'number of entries' sub zero 
                        }
                        #region determine whether to add table row for remaining sub-indexs
                        if ((CANdevice.nodeOrTableIndex < this.sysInfo.nodes.Length)
                            && (CANdevice.accessLevel == 0)
                            && (odSub.accessLevel > 0))
                        {
                            continue;  //don't show secure items to non-logged in users
                        }
                        #endregion determine whether to add table row for remaining sub-indexs
                        #endregion  add tree nodes and decide whther rows should be added for subindexes
                        #region create a table row from the DI dictionary data and, if OK, add to table
                        DataRow row = MAIN_WINDOW.DWdataset.Tables[CANdevice.nodeOrTableIndex].NewRow();
                        bool rowAddInhibit = false;
                        if ((CANdevice.nodeOrTableIndex == MAIN_WINDOW.DCFTblIndex) && (this.DCFFromfile == true))
                        {
                            rowAddInhibit = fillTableRowColumns(odSub, row, CANdevice.nodeOrTableIndex, true, true, CANdevice);
                        }
                        else
                        {
                            rowAddInhibit = fillTableRowColumns(odSub, row, CANdevice.nodeOrTableIndex, true, false, CANdevice);
                        }
                        if (rowAddInhibit == false) //&& (MAIN_WINDOW.DWdataset.Tables[CANdevice.nodeOrTableIndex].Rows.Contains(row) == false))
                        {
                            try
                            {
                                //DO NOT use loadDataRow - it does not load the enumeraitons correctly
                                MAIN_WINDOW.DWdataset.Tables[CANdevice.nodeOrTableIndex].Rows.Add(row);
                                row.AcceptChanges();
                            }
                            catch//(Exception e)
                            {
                                #region error feedback
                                //								SystemInfo.errorSB.Append("\nException when adding Row to node id:");
                                //								SystemInfo.errorSB.Append(CANdevice.nodeID.ToString());
                                //								SystemInfo.errorSB.Append(" item: ");
                                //								SystemInfo.errorSB.Append(odSub.parameterName);
                                //								SystemInfo.errorSB.Append(" (index Ox");
                                //								SystemInfo.errorSB.Append(odSub.indexNumber.ToString("X").PadLeft(4, '0'));
                                //								SystemInfo.errorSB.Append(" subIndex 0x") ;
                                //								SystemInfo.errorSB.Append(odSub.subNumber.ToString("X"));
                                //								SystemInfo.errorSB.Append(" Exception message:") ;
                                //								SystemInfo.errorSB.Append(e.Message);
                                //								SystemInfo.errorSB.Append(" , ");
                                //								SystemInfo.errorSB.Append(e.InnerException);
                                #endregion error feedback
                            }
                        }
                        #endregion create a table row from the dI dictionary data and if OK add to table
                        #endregion fill table rows
                    }
                    #endregion match generic CAN Open sections
                    #endregion hook every OD itme in the CANnode under its correct TreeView node
                }
            }
		}
		private bool fillTableRowColumns(ODItemData odSub, DataRow row,  int tableIndex, bool fillNonValueCols, bool fillActual, nodeInfo CANdevice)
		{
			bool rowAddInhibit = false;  //force single return point so we know event handler got switched off and then on again before exit
			ObjDictItem odItem = CANdevice.getODItemAndSubs(odSub.indexNumber);
			row[(int) (TblCols.odSub)] = odSub;
			if(fillNonValueCols == true)
			{
				#region handle columns that are NOT defualt, max, min, actual value
				if(odSub.subNumber!= -1) //not header row
				{
					row[(int) (TblCols.sub)] = "0x" + odSub.subNumber.ToString("X").PadLeft(3, '0');
					row[(int) (TblCols.units)] = odSub.units;  //for header tis is ""
				}
				row[(int) (TblCols.Index)] = "0x" + odSub.indexNumber.ToString("X").PadLeft(4,'0'); //always fill - it is a primary key
				row[(int) (TblCols.param)] = odSub.parameterName;
				row[(int) (TblCols.accessLevel)] = odSub.accessLevel.ToString();
				row[(int) (TblCols.accessType)] = odSub.accessType.ToString();
				row[(int) TblCols.NodeID] = CANdevice.nodeID;
				if((odSub.sectionType == (int)SevconSectionType.CANOPENSETUP)
					&& (odSub.objectName == (int)SevconObjectType.FORCE_TO_PREOP))
				{
					SCCorpStyle.AccLevel_PreOp = (uint) odSub.accessLevel;
				}
				#endregion handle columns that are NOT defualt, max, min, actual value
			}
			#region Min, Max, Default and Actual Value columns
			if(odSub.subNumber != -1)//not a header row
			{
				switch((CANopenDataType) odSub.dataType)
				{
						#region switch dataType
					case CANopenDataType.VISIBLE_STRING:
					case CANopenDataType.UNICODE_STRING:
					case CANopenDataType.OCTET_STRING:
						#region string
						if(fillNonValueCols == true)
						{
							row[(int) (TblCols.defVal)] = odSub.defaultValueString;
						}
						if(fillActual == true)
						{
							row[(int) TblCols.actValue] = odSub.currentValueString;
						}
						#endregion string
						break;
								
					case CANopenDataType.UNSIGNED16:
					case CANopenDataType.UNSIGNED24:
					case CANopenDataType.UNSIGNED32:
					case CANopenDataType.UNSIGNED40:
					case CANopenDataType.UNSIGNED48:
					case CANopenDataType.UNSIGNED56:
					case CANopenDataType.UNSIGNED64:
					case CANopenDataType.UNSIGNED8:
					case CANopenDataType.INTEGER16:
					case CANopenDataType.INTEGER24:
					case CANopenDataType.INTEGER32:
					case CANopenDataType.INTEGER40:
					case CANopenDataType.INTEGER48:
					case CANopenDataType.INTEGER56:
					case CANopenDataType.INTEGER64:
					case CANopenDataType.INTEGER8:
					case CANopenDataType.BOOLEAN:
						#region numerical
						if ((odSub.format == SevconNumberFormat.SPECIAL)
							|| ((CANopenDataType) odSub.dataType == CANopenDataType.BOOLEAN))
						{
							#region SPECIAL/BOOLEAN
							if(fillNonValueCols == true)
							{
								row[(int) (TblCols.defVal)] = sysInfo.getEnumeratedValue(odSub.formatList, odSub.defaultValue);
							}
							if(fillActual == true)
							{
								row[(int) (TblCols.actValue)] = sysInfo.getEnumeratedValue(odSub.formatList, odSub.currentValue);
							}

							#endregion SPECIAL/BOOLEAN
						}
						else if(odSub.format == SevconNumberFormat.BASE16)
						{
							#region BASE 16
							if(fillNonValueCols == true)
							{
								row[(int) (TblCols.defVal)] = "0x" + odSub.defaultValue.ToString("X");
								if((odSub.accessType != ObjectAccessType.Constant) 
									&& (odSub.accessType != ObjectAccessType.ReadOnly))
								{
									row[(int) (TblCols.lowVal)] = "0x" + odSub.lowLimit.ToString("X");
									row[(int) (TblCols.highVal)] = "0x" + odSub.highLimit.ToString("X");
								}
							}
							if(fillActual == true)
							{
								row[(int) (TblCols.actValue)] = "0x" + odSub.currentValue.ToString("X");;
							}
							#endregion BASE 16
						}
						else if (odSub.format == SevconNumberFormat.BASE10)
						{
							#region BASE 10
							if(fillNonValueCols == true)
							{
								row[(int) (TblCols.defVal)] = odSub.defaultValue * odSub.scaling;
								if((odSub.accessType != ObjectAccessType.Constant) 
									&& (odSub.accessType != ObjectAccessType.ReadOnly))
								{
									row[(int) (TblCols.lowVal)] = (odSub.lowLimit * odSub.scaling).ToString("G5");
									row[(int) (TblCols.highVal)] = (odSub.highLimit * odSub.scaling).ToString("G5");;
								}
							}
							if(fillActual == true)
							{
								row[(int) (TblCols.actValue)] = (odSub.currentValue * odSub.scaling).ToString("G5");;
							}
							#endregion BASE 10
						}
						else if (odSub.format == SevconNumberFormat.BIT_SPLIT)
						{
							#region bit split
							//do nothing - this is a bit split header row
							#endregion bit split
						}
						#endregion numerical
						break;

					case CANopenDataType.REAL32:
						#region REAL 32
						if(fillNonValueCols == true)
						{
							if( odSub.real32 != null )
							{
								row[(int) (TblCols.defVal)] = odSub.real32.defaultValue;
								if((odSub.accessType != ObjectAccessType.Constant) 
									&& (odSub.accessType != ObjectAccessType.ReadOnly))
								{
									row[(int) (TblCols.lowVal)] = (odSub.real32.lowLimit).ToString("G5");;
									row[(int) (TblCols.highVal)] = (odSub.real32.highLimit).ToString("G5");;
								}
							}
						}
						if(fillActual == true)
						{
							row[(int) (TblCols.actValue)] = (odSub.real32.currentValue).ToString("G5");;
						}
						#endregion REAL 32
						break;

					case CANopenDataType.REAL64:
						#region REAL 64
						if(fillNonValueCols == true)
						{
							if( odSub.real64 != null)
							{
								row[(int) (TblCols.defVal)] = odSub.real64.defaultValue;
								if((odSub.accessType != ObjectAccessType.Constant) 
									&& (odSub.accessType != ObjectAccessType.ReadOnly))
								{
									row[(int) (TblCols.lowVal)] = (odSub.real64.lowLimit).ToString("G5");;
									row[(int) (TblCols.highVal)] = (odSub.real64.highLimit).ToString("G5");;
								}
							}
						}
						if(fillActual == true)
						{
							row[(int) (TblCols.actValue)] = (odSub.real64.currentValue).ToString("G5");;
						}
						#endregion REAL 64
						break;
															
					case CANopenDataType.DOMAIN:
						//rowAddInhibit = true;
						if(fillActual == true)
						{
							row[(int) (TblCols.actValue)] = "DOMAIN";//sysInfo.getEnumeratedValue(odSub.formatList, odSub.currentValue);
						}
						break;

					case CANopenDataType.TIME_DIFFERENCE:
					case CANopenDataType.TIME_OF_DAY:
						#region TIME_DIFFERENCE or TIME_OF_DAY
						bool absTime = false;
						if( (CANopenDataType) odSub.dataType == CANopenDataType.TIME_OF_DAY)
						{
							absTime = true;
						}
						if(fillNonValueCols == true)
						{
							row[(int) (TblCols.defVal)] = getTime(odSub.defaultValue,absTime );
							if((odSub.accessType != ObjectAccessType.Constant) 
								&& (odSub.accessType != ObjectAccessType.ReadOnly))
							{
								row[(int) (TblCols.lowVal)] = getTime(odSub.lowLimit, absTime);
								row[(int) (TblCols.highVal)] = getTime(odSub.highLimit, absTime);
							}
						}
						if(fillActual == true)
						{
							row[(int) (TblCols.actValue)] = getTime(odSub.currentValue, absTime);
						}
						#endregion TIME_DIFFERENCE or TIME_OF_DAY
						break;

					case CANopenDataType.RESERVED1:
					case CANopenDataType.RESERVED2:
					case CANopenDataType.RESERVED3:
					case CANopenDataType.RESERVED4:
					case CANopenDataType.RESERVED5:
					case CANopenDataType.RESERVED6:
					default:
						SystemInfo.errorSB.Append("\nError. Reserved data type");
						rowAddInhibit = true;
						break;
						#endregion switch dataType 
				}
			}
			#endregion Min, Max, Default and Actual Value columns
			if((fillActual == true)
				&& ((CANopenDataType) odSub.dataType != CANopenDataType.DOMAIN)
				&& ((odSub.accessType == ObjectAccessType.WriteOnly)
				||(odSub.accessType == ObjectAccessType.WriteOnlyInPreOp)))
			{
				row[(int) (TblCols.actValue)] = ""; //mask out DI value since htis is a write only parameter
			}
			return rowAddInhibit;
		}

		private void updateRowColoursArray()
		{
			bool nodeInPreOp = true;
			uint UserAccLevel = 0;
			if(MAIN_WINDOW.DCFCompareActive == false)
			{  //gives confusing feedback otherwise
				this.statusBarPanel3.Text = "Applying Row colouring";
			}
			if(currTblIndex<DWdataset.Tables.Count)
			{
				#region set pre-op and user access 
				if(currTblIndex< this.sysInfo.nodes.Length)
				{
					if(sysInfo.nodes[currTblIndex].nodeState != NodeState.PreOperational)
					{
						nodeInPreOp = false;
					}
					UserAccLevel = sysInfo.nodes[currTblIndex].accessLevel;
				}
				else  //User Custom list
				{
					UserAccLevel = sysInfo.systemAccess;
					nodeInPreOp = false; //assume not in pre-op - arbitrary really - but have to jump one way or the other
				}
				#endregion set pre-op and user access 
				colArray = new Color[DWdataset.Tables[currTblIndex].DefaultView.Count]; 
				canWriteNow = new bool[DWdataset.Tables[currTblIndex].DefaultView.Count]; 
				int rowInd = 0;
                // NOTE: progressBar1 update removed (not needed as so quick & messes up DCF file load)
				foreach (DataRowView myRow in DWdataset.Tables[currTblIndex].DefaultView)
				{
					ODItemData odSub = (ODItemData) myRow[(int)TblCols.odSub];
					if((Boolean)myRow.Row[(int) TblCols.Monitor] == true)
					{
						colArray[rowInd] = SCCorpStyle.dgRowSelected;
					}
					else
					{
						if(odSub.subNumber == -1)
						{
							colArray[rowInd] = SCCorpStyle.headerRow;
						}
						else
						{
							switch(odSub.accessType)
							{
									#region give each row a correspondinf Color, for use by DataGridColumn Class Paint method
								case ObjectAccessType.ReadOnly:
								case ObjectAccessType.Constant:
									#region read only/consts
									colArray[rowInd] = SCCorpStyle.readOnly;
									break;
									#endregion read only/consts

								case ObjectAccessType.WriteOnly:
									#region write only
									colArray[rowInd] = SCCorpStyle.writeOnly;
									if(odSub.accessLevel <= UserAccLevel)
									{
										canWriteNow[rowInd] = true;
									}
									break;
									#endregion write only

								case ObjectAccessType.ReadReadWriteInPreOp:
								case ObjectAccessType.ReadWriteInPreOp:
								case ObjectAccessType.ReadWriteWriteInPreOp:
									#region read/writes in Pre-Op
									if(odSub.accessLevel > UserAccLevel)
									{
										colArray[rowInd] = SCCorpStyle.readOnly;
									}
									else
									{
										if(nodeInPreOp == true)
										{
											colArray[rowInd] = SCCorpStyle.readWrite;
											canWriteNow[rowInd] = true;
										}
										else
										{
											colArray[rowInd] = SCCorpStyle.readWriteInPreOp;
										}
									}
									break;
									#endregion read/writes in Pre-Op

								case ObjectAccessType.ReadReadWrite:
								case ObjectAccessType.ReadWrite:
								case ObjectAccessType.ReadWriteWrite:
									#region read/writes
									if(odSub.accessLevel > UserAccLevel)
									{
										colArray[rowInd] = SCCorpStyle.readOnly;
									}
									else
									{
										colArray[rowInd] = SCCorpStyle.readWrite;
										canWriteNow[rowInd] = true;
									}
									break;
									#endregion read/writes

								case ObjectAccessType.WriteOnlyInPreOp:
									#region write only in pre-op
									if(nodeInPreOp == true)
									{
										colArray[rowInd] = SCCorpStyle.writeOnly;
										canWriteNow[rowInd] = true;
									}
									else
									{
										colArray[rowInd] = SCCorpStyle.writeOnlyPlusPreOP;
									}
									break;
									#endregion write only in pre-op

								default:
									#region default 
									SystemInfo.errorSB.Append("\nFailed to color row No ");
									SystemInfo.errorSB.Append(rowInd.ToString());
									SystemInfo.errorSB.Append("Undefined object type");
									colArray[rowInd] = SCCorpStyle.readOnly;
									break;
									#endregion default 
									#endregion give each row a corresponding Color, for use by DataGridColumn Class Paint method
							}
						}
					}
					rowInd++;
				}
			}
		}

		#endregion treeview filling methods

		#region TreeView Node Selection
		internal void getChildODItems(TreeNode TopNode, bool firstEntry) 
		{ 
			if(firstEntry == true)
			{ //first time into this recursive method we need to reset the storage arraylist
				childTreeNodeList = new ArrayList(); 
			}
			//Recursive find 
			foreach(TreeNode node in TopNode.Nodes) 
			{ 
				treeNodeTag senderNodeTag = (treeNodeTag) node.Tag;
				if(senderNodeTag.assocSub != null)
				{
					childTreeNodeList.Add(senderNodeTag);
				}
				//Search all child nodes
				getChildODItems(node,false); 
			} 
			return; 
		} 

		private void treeView1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			string tempStr = this.AllType1FormsClosed();
			if (this.COB_PDO_FRM != null)
			{
				tempStr = "System PDO configuration";
			}
			if(tempStr != "")
			{
				this.statusBarPanel3.Text = "Node selection disabled while " + tempStr + " is active";
				return;
			}
			if(MAIN_WINDOW.UserInputInhibit == true) 
			{
				    this.statusBarPanel3.Text = "Node selection inhibited.  Wait for processing to complete";
    				return;
			}
			if(this.expandFlag == true)
			{
				this.expandFlag = false;
				return;
			}
			//This method is use dinstead of afterSelect because it is more responsive
			//Single Mouse CLcik (Right or Left)casues the tree nNode unde rhte mouse to become the seleted node
			//After Sleect does not get re-called if the currently selecte ditem is clicked again. 
			//We need to react to this to do things like re-open closed child window or give it focus
			TreeNode senderTreeNode = this.treeView1.GetNodeAt(this.treeView1.PointToClient(Cursor.Position));
            bool newNodeSelected = false; //DR38000264
            if (senderTreeNode != null) //we clicked over a node
			{
                //DR38000264 Is this a new node selected or reselection of the existing one?
                //but allow re-selection of special nodes
                if (   (senderTreeNode != treeView1.SelectedNode)
                    || (senderTreeNode.Text == this.SystemLogTNText)
					|| (senderTreeNode.Text == this.FaultLogTNText)
					|| (senderTreeNode.Text == this.CountersLogTNText)
					|| (senderTreeNode.Text == this.OpLogTNText)
					|| (senderTreeNode.Text == this.nodeCOBShortCutsText)
					|| (senderTreeNode.Text == this.CountersLogTNText)
                    || (senderTreeNode.Text == this.programmingTNText)
                    || (senderTreeNode.Text == this.LogsTNText)
                    || (senderTreeNode.Text == this.DataLogTNText)
                    )
                {
                    newNodeSelected = true;
                }
				this.treeView1.SelectedNode = senderTreeNode; //prevent node 'jumping' due to after select
				currentTreenode = senderTreeNode;
				if((e.Button == MouseButtons.Right) 
					&&((treeView1.SelectedNode.Text == this.SystemLogTNText)
					|| (treeView1.SelectedNode.Text == this.FaultLogTNText)
					|| (treeView1.SelectedNode.Text == this.CountersLogTNText)
					|| (treeView1.SelectedNode.Text == this.OpLogTNText)
					|| (treeView1.SelectedNode.Text == this.nodeCOBShortCutsText)
					|| ( treeView1.SelectedNode.Text == this.CountersLogTNText)))
				{
					if( this.treeView1.SelectedNode.IsExpanded == true)
					{
						this.treeView1.SelectedNode.Collapse();
					}
					else
					{
						this.treeView1.SelectedNode.Expand();
					}
				}
				else
				{
                    //DR38000264 only do if new node selected - takes perceivable process time to read root node battery
                    // voltages etc on entire node & prevents drag/drop until its finished.
                    // No point re-reading voltages etc which have recently been re-read anyway.
                    if (newNodeSelected == true)
                    {
                        this.handleTreeNodeSelection(senderTreeNode, false);  //in here we update treeView1.SelectedNode
                    }
				}
				if(( e.Button == MouseButtons.Right)
//					&& (this.treeNodeIsDeviceLevelSpecialNode(treeView1.SelectedNode) == false)
					&& (this.treeNodeIsSystemLevelSpecialNode(this.treeView1.SelectedNode) == false))
				{
					setupAnddisplayContextMenu();
				}
			}
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("Errors:");
			}
		}

		internal void isNodePathInTreeLeg(TreeNode TopNode, string nodePath) 
		{ 
			//Recursively find the nodePAth specifed if it exists
			foreach(TreeNode node in TopNode.Nodes) 
			{ 
				if(node.FullPath == nodePath)
				{
					currentTreenode = node;
					return;
				}
				else
				{
					if((node.Nodes.Count>0) && (currentTreenode == null))
					{
						isNodePathInTreeLeg(node,nodePath);
					}
				}
			} 
			return;
		} 
		internal void expandChildNodes(TreeNode TopNode) 
		{ 
			//Recursive find 
			foreach(TreeNode node in TopNode.Nodes) 
			{ 
				node.Expand();
				//recursively expand all child nodes
				expandChildNodes(node); 
			} 
			return; 
		} 
		private string AllType1FormsClosed()
		{
			string openWindow = "";
			if(SELF_CHAR_FRM != null)
			{
				openWindow = "Self characterization";
			}
			else if(CAN_BUS_CONFIG != null)
			{
				openWindow = "CAN bus configuration";
			}
			else if(PROG_DEVICE_FRM != null)
			{
				openWindow = "CAN node re-programming";
			}
			else if (options != null)
			{
				openWindow = "Options";
			}
			else if(selectProfile != null)
			{
				openWindow = "Select vehicle profile";
			}
			return openWindow;
		}
		private void handleTreeNodeSelection(TreeNode selNode, bool forceDatagridUpdate)
		{
			#region reject any changes in the last view first
			//this prevents any possibilty of submitting -old changes to controller
			if(MAIN_WINDOW.currTblIndex<this.sysInfo.nodes.Length)
			{
				foreach(DataRowView dvRow in MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].DefaultView)
				{
					dvRow.Row.RejectChanges();
				}
			}
			#endregion reject any changes in the last view first
			//change node seletion beofre hiding controls - better user feedback
			this.treeView1.SelectedNode = selNode;  //change the selected node to this one
			this.currentTreenode = this.treeView1.SelectedNode;
			hideUserControls();  //put here - becasue we still expand /collapse the node even if it is the same as previousNode
			treeNodeTag selNodeTag = (treeNodeTag) selNode.Tag;  //Tag tells us what this Treenode represents
			//Note We do this to ensure that any context menu options ar eapplied to the correct
			//table indexed by currTableIndex
			//note we MUST use Text here - using the node doesn't work due to node cloning that is done between
			//hide/show read only 
			if((this.treeView1.SelectedNode != this.previousNode )
				|| (forceDatagridUpdate == true)
				|| (this.treeNodeIsDeviceLevelSpecialNode(treeView1.SelectedNode) == true)
				|| (this.treeNodeIsSystemLevelSpecialNode(treeView1.SelectedNode) == true))
			{
				if((selNodeTag.tableindex<MAIN_WINDOW.DWdataset.Tables.Count) && (this.SystemPDOsScreenActive == false))
				{  
					#region Handle TreeView Legs with associated DataTable ie devices and CUstom lists
					if(selNodeTag.tableindex<this.sysInfo.nodes.Length)
					{	//limit text matching possiblities as far as apoosible because some node text is set up on external XML file
						#region real and virtual nodes

						if(this.sysInfo.nodes[selNodeTag.tableindex].isSevconApplication() == true)
						{
							#region SEVCON application related nodes that open other windows
							if (selNode.Text == this.FaultLogTNText)
							{
								#region fault log
								if(((SCCorpStyle.ProductRange)this.sysInfo.nodes[selNodeTag.tableindex].productRange != SCCorpStyle.ProductRange.SEVCONDISPLAY)
                                    && ((SCCorpStyle.ProductRange)this.sysInfo.nodes[selNodeTag.tableindex].productRange != SCCorpStyle.ProductRange.CALIBRATOR)) //DR38000256
								{ //judetemp = until we have screen set up
									if(FAULT_LOG_FRM == null) 
									{
										statusBarOverrideText = this.AllType1FormsClosed();
										if(statusBarOverrideText == "")
										{
                                            // only show new window if not a search initiated or the search calls for a forced update
                                            if ((searchInProgress == false) || (forceDatagridUpdate == true))
                                            {
											FAULT_LOG_FRM = new FAULT_LOG_WINDOW(sysInfo,selNodeTag.tableindex, "Node " + selNodeTag.nodeID.ToString(), this.toolBar1);
											this.FAULT_LOG_FRM.Closed +=new EventHandler(FAULT_LOG_FRM_Closed);
											setupAndDisplayChildWindow(FAULT_LOG_FRM);
										}
										}
										else
										{
											statusBarOverrideText = "Fault Log Screen not available, while " + statusBarOverrideText + " window open";
										}
									}
									else
									{
										FAULT_LOG_FRM.Focus();
									}
								}
								#endregion fault log
							}
							else if (selNode.Text == this.SystemLogTNText)
							{
								#region System Log
								if(SYSTEM_LOG_FRM == null)
								{
									statusBarOverrideText = this.AllType1FormsClosed();
									if(statusBarOverrideText == "")
									{
                                        if ((searchInProgress == false) || (forceDatagridUpdate == true))
                                        {
										SYSTEM_LOG_FRM = new SYSTEM_LOG_WINDOW(sysInfo,selNodeTag.tableindex, "Node " + selNodeTag.nodeID.ToString(), this.toolBar1);
										this.SYSTEM_LOG_FRM.Closed +=new EventHandler(SYSTEM_LOG_FRM_Closed);
										setupAndDisplayChildWindow(SYSTEM_LOG_FRM);
									}
									}
									else
									{
										statusBarOverrideText = "system Log Screen not available, while " + statusBarOverrideText + " window open";
									}

								}
								else
								{
									SYSTEM_LOG_FRM.Focus();
								}
								#endregion System Log
							}
							else if (selNode.Text ==  this.OpLogTNText)
							{
								#region Op logs
								if(this.OP_LOGS_FRM == null) 
								{
									statusBarOverrideText = this.AllType1FormsClosed();
									if(statusBarOverrideText == "")
									{
                                        if ((searchInProgress == false) || (forceDatagridUpdate == true))
                                        {
										OP_LOGS_FRM = new OP_LOGS_WINDOW(sysInfo,selNodeTag.tableindex, "Node " + selNodeTag.nodeID.ToString(), this.toolBar1);
										this.OP_LOGS_FRM.Closed+=new EventHandler(OP_LOGS_FRM_Closed); 
										setupAndDisplayChildWindow(OP_LOGS_FRM);
									}
									}
									else
									{
										statusBarOverrideText = "Operational Logs Screen not available, while " + statusBarOverrideText + " window open";
									}
								}
								else
								{
									OP_LOGS_FRM.Focus();
								}
								#endregion Op logs
							}
							else if (selNode.Text == this.CountersLogTNText)
							{
								#region Event Counters
								if(this.COUNTERS_FRM == null)
								{
									statusBarOverrideText = this.AllType1FormsClosed();
									if(statusBarOverrideText == "")
									{
                                        if ((searchInProgress == false) || (forceDatagridUpdate == true))
                                        {
										COUNTERS_FRM = new COUNTERS_LOG_WINDOW(sysInfo,selNodeTag.tableindex, "Node " + selNodeTag.nodeID.ToString(), this.toolBar1 );
										this.COUNTERS_FRM.Closed +=new EventHandler(COUNTERS_FRM_Closed);
										setupAndDisplayChildWindow(COUNTERS_FRM);
									}
									}
									else
									{
										statusBarOverrideText = "Event Counters Screen not available, while " + statusBarOverrideText + " window open";
									}

								}
								else
								{
									COUNTERS_FRM.Focus();
								}
								#endregion Event Counters
							}

							else if (selNode.Text ==  this.SelfCharMotor1Text)
							{
						
								#region Self Char
								if(this.SELF_CHAR_FRM == null)
								{
                                    if ((searchInProgress == false) || (forceDatagridUpdate == true))
                                    {
									confirmCloseOtherForms();

									if(this.OwnedForms.Length==0)
									{
										MAIN_WINDOW.UserInputInhibit = true;
										int MotorIndex = 0; //judetmep - need a mechanism to determine wheich motr in Vehicle Profile this should be
										SELF_CHAR_FRM = new SELF_CHARACTERISATION_WINDOW(sysInfo,selNodeTag.tableindex, MotorIndex);
										this.SELF_CHAR_FRM.Closed +=new EventHandler(SELF_CHAR_FRM_Closed);
										setupAndDisplayChildWindow(SELF_CHAR_FRM);
									}
								}
								}
								else
								{
									SELF_CHAR_FRM.Focus();
								}
								#endregion Self Char
							}
							else if (selNode.Text ==  this.SelfCharMotor2Text)
							{
								#region Self Char
								if(this.SELF_CHAR_FRM == null)
								{
                                    if ((searchInProgress == false) || (forceDatagridUpdate == true))
                                    {
									confirmCloseOtherForms();

									if(this.OwnedForms.Length==0)
									{
										MAIN_WINDOW.UserInputInhibit = true;
										int motorIndex = 1;
										SELF_CHAR_FRM = new SELF_CHARACTERISATION_WINDOW(sysInfo,selNodeTag.tableindex, motorIndex);
										this.SELF_CHAR_FRM.Closed +=new EventHandler(SELF_CHAR_FRM_Closed);
										setupAndDisplayChildWindow(SELF_CHAR_FRM);
									}
								}
								}
								else
								{
									SELF_CHAR_FRM.Focus();
								}
								#endregion Self Char
							}
							else if(selNode.Text == this.nodeCOBShortCutsText)
							{
								#region COMMs screen
                                //Reset SearchForm search instance when swapping from main to PDO setup screen
                                //as treeview1 may change.
                                if ((searchInProgress == false) || (forceDatagridUpdate == true))
							{
                                    if (SEARCH_FRM != null)
                                    {
                                        ((SearchForm)SEARCH_FRM).resetSearchInstance();
                                    }

                                    SystemPDOsScreenActive = true; //for re-entry
								#region data retrieval thread
								this.progressBar1.Value = this.progressBar1.Minimum;
								this.statusBarPanel3.Text = "Retrieving values from all connected nodes...";
								this.progressBar1.Maximum = this.sysInfo.totalItemsInAllODs;
								dataRetrievalThread = new Thread(new ThreadStart( this.sysInfo.readAllCOBItemsAndCreateCOBsInSystem )); 
								dataRetrievalThread.Name = "COBDataRetrieval";
								dataRetrievalThread.IsBackground = true;
                                dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
								System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif
								dataRetrievalThread.Start(); 
								this.timer2.Enabled = true;
								#endregion data retrieval thread
							}
								#endregion COMMs screen
							}
							#endregion SEVCON application related nodes that open other windows
						}
						if( (this.sysInfo.nodes[selNodeTag.tableindex].isSevconApplication() == true)
							|| (( this.sysInfo.nodes[selNodeTag.tableindex].manufacturer == Manufacturer.SEVCON)
							&& 
							(( sysInfo.nodes[selNodeTag.tableindex].productVariant == SCCorpStyle.bootloader_variant)
							|| (sysInfo.nodes[selNodeTag.tableindex].productRange == (byte)SCCorpStyle.ProductRange.SEVCONDISPLAY)
                            || (sysInfo.nodes[selNodeTag.tableindex].productRange == (byte)SCCorpStyle.ProductRange.CALIBRATOR) //DR38000256
							|| (sysInfo.nodes[selNodeTag.tableindex].productVariant == SCCorpStyle.selfchar_variant_old)
							|| (sysInfo.nodes[selNodeTag.tableindex].productVariant == SCCorpStyle.selfchar_variant_new))))
						{ //has to be SEVCON applicaiton, bootlaoder or self char to allow programming
							if(selNode.Text == programmingTNText)
							{
								#region device programming
								if(PROG_DEVICE_FRM == null)
								{
                                    if ((searchInProgress == false) || (forceDatagridUpdate == true))
								{
									confirmCloseOtherForms();
									if(this.OwnedForms.Length==0)
									{
										MAIN_WINDOW.UserInputInhibit = true;
										this.PROG_DEVICE_FRM = new DEVICE_PROGRAMMING_WINDOW(sysInfo, selNodeTag.tableindex);
										this.PROG_DEVICE_FRM.Closed +=new EventHandler(PROG_DEVICE_FRM_Closed);
										setupAndDisplayChildWindow(PROG_DEVICE_FRM);
                                        }
									}
								}
								else
								{
									PROG_DEVICE_FRM.Focus();
								}
								#endregion device programming
							}
						}
						#endregion real and virtual nodes
					}
					#region Personality Parameters Type display
					#region	update mapping name and DataGrid Datasource
					if((currTblIndex != selNodeTag.tableindex)
						|| (forceDatagridUpdate == true)
						|| (firstNodeSelectionAfterConnection ==true)
						|| (this.dataGrid1.DataSource == null))  
						//but NOT the datatable in use 
						//OR first time in and we have not yet got a data source
					{ 
						firstNodeSelectionAfterConnection = false;
						#region table index has changed
						if(DCFCompareActive == true)
						{
							if(selNodeTag.tableindex == MAIN_WINDOW.DCFTblIndex )
							{  //we want to look at the ocmpare table - 
								this.dataGrid1.DataSource = this.comparisonTable.DefaultView;
							}
							else
							{
								if(currTblIndex == MAIN_WINDOW.DCFTblIndex)
								{  //we were looking at a comparison table - now we are back to a normal table
									this.dataGrid1.DataSource  = MAIN_WINDOW.DWdataset.Tables[selNodeTag.tableindex].DefaultView;
									this.dataGrid1.Invalidate();
								}
							}
						}
						else
						{
							this.dataGrid1.DataSource  = MAIN_WINDOW.DWdataset.Tables[selNodeTag.tableindex].DefaultView;
						}
						try
						{
							setDataGridFont(selNodeTag.tableindex);
						}
						catch
						{
							//int breakonly = 9;
						}
                        // new table selected, delete old list change handler & add one for the new table 
                        if (currTblIndex < DWdataset.Tables.Count) // prevent exception if Tables has changed & no longer exists
                        {
                            DWdataset.Tables[currTblIndex].DefaultView.ListChanged -= new ListChangedEventHandler(DWdataset_DefaultView_ListChanged);
                        }
						MAIN_WINDOW.currTblIndex = selNodeTag.tableindex;  //update the current Table Index ready for comparison next time
                        DWdataset.Tables[currTblIndex].DefaultView.ListChanged += new ListChangedEventHandler(DWdataset_DefaultView_ListChanged);
						#endregion table index has changed
					}
					#endregion	update mapping name and DataGrid Datasource
					#region update DataView rowFilter
					//note we need to update the DCF table DataView even is Comparre is active
					//So when user switches compare off the vie wis up to date
					this.statusBarPanel3.Text = "Calculating data filters";
					try
					{ //needed becasue a huge row Filter WILL cause an exception
						MAIN_WINDOW.DWdataset.Tables[selNodeTag.tableindex].DefaultView.RowFilter = this.updateRowFiltering(selNode, selNodeTag);
					}
					catch
					{
						this.statusBarPanel3.Text = "Row filtering capacity exceeded, filtering switched off for this selection";
						MAIN_WINDOW.DWdataset.Tables[selNodeTag.tableindex].DefaultView.RowFilter = "";
					}
					if((selNodeTag.tableindex == MAIN_WINDOW.DCFTblIndex ) && (DCFCompareActive == true))
					{
						this.comparisonTable.DefaultView.RowFilter = MAIN_WINDOW.DWdataset.Tables[selNodeTag.tableindex].DefaultView.RowFilter;
					}
					#endregion update DataView rowFilter
					updateRowColoursArray(); //always needed because lefhand columns in Compare table also sourc their coolours from here	
					#region data retrieval for DCF compare
					if((selNodeTag.tableindex == MAIN_WINDOW.DCFTblIndex ) && (DCFCompareActive == true))
					{
						dataRetrievalThread = new Thread(new ThreadStart( UpdateDCFCompareData )); 
						dataRetrievalThread.Name = "DCF Comparison data retrieval";
						dataRetrievalThread.IsBackground = true;
                        dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
						System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif  
						this.progressBar1.Value = this.progressBar1.Minimum;
						this.progressBar1.Maximum = this.comparisonTable.DefaultView.Count;
						this.statusBarPanel3.Text = "Retrieving values from all connected nodes...";
						previousNode = this.treeView1.SelectedNode;
						previousNode = this.currentTreenode; 
						dataRetrievalThread.Start(); 
						this.timer2.Enabled = true;
						return;
					}
					#endregion data retrieval for DCF compare
					this.dataGrid1.CaptionText = selNode.FullPath;
					#region get the data from device if required
					string tempStr = this.AllType1FormsClosed();
					if((currTblIndex<this.sysInfo.nodes.Length) 
						&& (tempStr == "") 
						&& (this.COB_PDO_FRM == null)
						&& (this.SystemPDOsScreenActive == false))
					{
                        // If there is data associated with this sub or it is an XML header but the
                        // user has hit the refresh menu/F5/toolbar icon then read all data from device
						if 
                        ( 
                            (selNodeTag.assocSub != null) 
                         || ((selNodeTag.tagType == TNTagType.XMLHEADER) && (tbb_RefreshData.Enabled == false))
                        )
						{
                            if ( (searchInProgress == false) || (forceDatagridUpdate == true))
						    {
                                #region read individual OD items
                                this.progressBar1.Maximum = MAIN_WINDOW.DWdataset.Tables[selNodeTag.tableindex].DefaultView.Table.Rows.Count;
                                this.progressBar1.Value = this.progressBar1.Minimum;
                                sectiontype = (int)SevconSectionType.NONE;
                                ThirdPartySection = CANSectionType.NONE;
                                dataRetrievalThread = new Thread(new ParameterizedThreadStart(readTableData));
                                dataRetrievalThread.Name = "Table Data Retrieval";
                                dataRetrievalThread.IsBackground = true;
                                dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
                                System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif
                                dataRetrievalThread.Start(selNodeTag);
                                this.timer2.Enabled = true;

                                #endregion read individual OD items
                            }
						}
                        else if (selNodeTag.tagType == TNTagType.EDSSECTION)
                        {
                            if ((searchInProgress == false) || (forceDatagridUpdate == true))
                            {
                                #region Read whole section
                                #region user feedback
                                this.statusBarPanel3.Text = "Refreshing Data from "
                                    + this.sysInfo.nodes[selNodeTag.tableindex].deviceType
                                    + ", Node ID: " + this.sysInfo.nodes[selNodeTag.tableindex].nodeID;
                                this.progressBar1.Maximum = sysInfo.nodes[currTblIndex].objectDictionary.Count;
                                this.progressBar1.Value = this.progressBar1.Minimum;
                                #endregion user feedback
                                sectiontype = (int)SevconSectionType.NONE;
                                ThirdPartySection = CANSectionType.NONE;
                                foreach (DataRowView DVrow in MAIN_WINDOW.DWdataset.Tables[selNodeTag.tableindex].DefaultView)
                                {
                                    if (this.progressBar1.Value < this.progressBar1.Maximum)
                                    {
                                        this.progressBar1.Value++;
                                    }
                                    ODItemData odSub = (ODItemData)DVrow.Row[(int)TblCols.odSub];
                                    if ((this.sysInfo.nodes[currTblIndex].manufacturer == Manufacturer.SEVCON)
                                            && (odSub.sectionType != (int)SevconSectionType.NONE))
                                    {
                                        sectiontype = odSub.sectionType;
                                        #region start data section retrieval from device thread
                                        dataRetrievalThread = new Thread(new ThreadStart(readSevconDeviceSection));
                                        dataRetrievalThread.Name = "Section Data Retrieval";
                                        dataRetrievalThread.IsBackground = true;
                                        dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
                                        System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif
                                        dataRetrievalThread.Start();
                                        this.timer2.Enabled = true;
                                        #endregion start data section retrieval from device thread
                                        break; //do once
                                    }
                                    else if ((this.sysInfo.nodes[currTblIndex].manufacturer == Manufacturer.THIRD_PARTY)
                                        && (odSub.CANopenSectionType != CANSectionType.NONE))
                                    {
                                        ThirdPartySection = odSub.CANopenSectionType;
                                        #region start data section retrieval from device thread
                                        dataRetrievalThread = new Thread(new ThreadStart(readCANOpenDeviceSection));
                                        dataRetrievalThread.Name = "Section Data Retrieval";
                                        dataRetrievalThread.IsBackground = true;
                                        dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
                                        System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif
                                        dataRetrievalThread.Start();
                                        this.timer2.Enabled = true;
                                        #endregion start data section retrieval from device thread
                                        break;
                                    }
                                }

                                #endregion Read whole section
                            }
                        }
					}

					#endregion get the data from devcie if required
					#endregion Personality Parameters Type display
					#endregion Handle TreeView Legs with associated DataTable ie devices and CUstom lists
				}
				else
				{
					#region System Level child Windows eg COB routing and CAN bus Config
					if(selNode.Text == COBandPDOsTN.Text)
					{
                        if ((searchInProgress == false) || (forceDatagridUpdate == true))
					{
                        SystemPDOsScreenActive = true; //for re-entry
                        //Reset search instance before entering PDO screen
                        if (SEARCH_FRM != null)
                        {
                            ((SearchForm)SEARCH_FRM).resetSearchInstance();
                        }

						#region data retrieval thread
                        MAIN_WINDOW.UserInputInhibit = true;    //DR38000266 B&B - should already be true
						this.progressBar1.Value = this.progressBar1.Minimum;
						this.statusBarPanel3.Text = "Retrieving values from all connected nodes...";
						this.progressBar1.Maximum = this.sysInfo.totalItemsInAllODs;
						dataRetrievalThread = new Thread(new ThreadStart( this.sysInfo.readAllCOBItemsAndCreateCOBsInSystem )); 
						dataRetrievalThread.Name = "COBDataRetrieval";
						dataRetrievalThread.IsBackground = true;
                        dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
						System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif
						dataRetrievalThread.Start(); 
						this.timer2.Enabled = true;
						#endregion data retrieval thread
					}
					}
					else if (selNode.Text == CANBUsConfigTN.Text)
					{
						#region CAN Bus Config
						if(CAN_BUS_CONFIG == null)
						{
                            if ((searchInProgress == false) || (forceDatagridUpdate == true))
						{
							confirmCloseOtherForms();
							if(this.OwnedForms.Length==0)
							{
								MAIN_WINDOW.UserInputInhibit = true;
								CAN_BUS_CONFIG = new CAN_BUS_CONFIGURATION(sysInfo);
								CAN_BUS_CONFIG.Closed +=new EventHandler(CAN_BUS_CONFIG_Closed);
								setupAndDisplayChildWindow(CAN_BUS_CONFIG);
                                }
							}
						}
						else
						{
							CAN_BUS_CONFIG.Focus();
						}
						#endregion CAN Bus Config
					}
					else if(selNode.Text == this.SystemStatusTN.Text)
					{
					}
					#endregion System Level child Windows eg COB routing and CAN bus Config
				}
			}
			previousNode = this.treeView1.SelectedNode;
			previousNode = this.currentTreenode;  //judetmep
			if(dataRetrievalThread == null)
			{  //inhibit if a data retrieval thread is running - user controls will be switched back on once thread has completed
				showUserControls(); ///will alos expand/collapse node as required
			}
		}
		private void setDataGridFont(int tblInd)
		{
			if((DCFCompareActive == true) && (tblInd == MAIN_WINDOW.DCFTblIndex ))
			{
				#region comparison table
				//we want to look at the ocmpare table - 
				switch(this.comparisonTable.Columns.Count)
				{
					case 8:		
					case 9:
					case 10:
						this.dataGrid1.Font = new System.Drawing.Font("Arial", 9F);
						break;
					case 11:
					case 12:
					case 13:
						this.dataGrid1.Font = new System.Drawing.Font("Arial", 8F);
						break;
					default:
						this.dataGrid1.Font = new System.Drawing.Font("Arial", 10F);
						break;
				}
				#endregion comparison table
			}
			else if (tblInd >=this.sysInfo.nodes.Length)
			{
				#region table with no underlyung CAN devcie
				if(this.sysInfo.systemAccess >= 5) 
				{
					this.dataGrid1.Font = new System.Drawing.Font("Arial", 8F);
				}
				else if(this.sysInfo.systemAccess>= 3)
				{
					this.dataGrid1.Font = new System.Drawing.Font("Arial", 10F);
				}
				else
				{
					this.dataGrid1.Font = new System.Drawing.Font("Arial", 12F);
				}
				#endregion table with no underlyung CAN devcie
			}
			else 
			{
				#region table with associated CAN devcie
				if(tblInd>= this.sysInfo.nodes.Length)
				{
#if DEBUG
					//error condition - seen but cause as yet unknown
					Message.Show("Table index exceeded number of nodes.");
#endif
					this.dataGrid1.Font = new System.Drawing.Font("Arial", 8F);
				}
				else
				{
					if((this.sysInfo.systemAccess >= 5) 
						|| ((this.sysInfo.nodes[tblInd].isSevconDevice() == true)
						&& (this.sysInfo.nodes[tblInd].productVariant == SCCorpStyle.bootloader_variant)
						|| (this.sysInfo.nodes[tblInd].productVariant == SCCorpStyle.selfchar_variant_new)
						|| (this.sysInfo.nodes[tblInd].productVariant == SCCorpStyle.selfchar_variant_old)))
					{
						this.dataGrid1.Font = new System.Drawing.Font("Arial", 8F);
					}
					else if(this.sysInfo.systemAccess>= 3)
					{
						this.dataGrid1.Font = new System.Drawing.Font("Arial", 10F);
					}
					else
					{
						this.dataGrid1.Font = new System.Drawing.Font("Arial", 12F);
					}
				}
				#endregion table with associated CAN devcie
			}
		}

		private void updateTableFromDI()
		{
			DataRowState currentRowState;
			#region update Table from DI
			this.progressBar1.Value = this.progressBar1.Minimum;
			this.progressBar1.Maximum = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].DefaultView.Count;
			this.statusBarPanel3.Text = "Updating table from Device Interface";
			this.statusBar1.Update();
			foreach(DataRowView DVrow in MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].DefaultView)
			{  
				if(this.progressBar1.Value<this.progressBar1.Maximum)
				{
					this.progressBar1.Value++;
				}
				DataRow row = DVrow.Row;  //conver to a proper DataRow
				ODItemData odSub = (ODItemData) row[(int) TblCols.odSub];
				if(odSub.subNumber != -1)
				{
					currentRowState = row.RowState;
					//this method only called in th ecurrent table represents a real of virtual CAN device
					if((this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].manufacturer == Manufacturer.SEVCON) 
						&& (this.sectiontype != (int)SevconSectionType.NONE) 
						&& (this.sectiontype != odSub.sectionType))
					{
						continue;  //do not read from DI unless this is the seciton tha tDI extracted from the actual device
					}
					else if ((this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].manufacturer == Manufacturer.THIRD_PARTY)
						&&  (this.ThirdPartySection != CANSectionType.NONE)
						&& (odSub.CANopenSectionType != this.ThirdPartySection))
					{
						continue;
					}
					if(currTblIndex<this.sysInfo.nodes.Length)
					{
						MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged -= new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
						this.fillTableRowColumns(odSub,row,currTblIndex, false, true, this.sysInfo.nodes[MAIN_WINDOW.currTblIndex]);
						if 
                        (
                            (currentRowState == DataRowState.Unchanged)
                         || ((currentRowState == DataRowState.Modified) && (tbb_RefreshData.Enabled == false))
                        )
						{
                            //only accept changes if this is the only change ot this row - keep user changes marked
                            //or if row has been changed due to the user selected F5 refresh function
							DVrow.Row.AcceptChanges();  
						}
						MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
					}
				}
			}
			#endregion update Table from DI
		}
		private void treeView1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(MAIN_WINDOW.UserInputInhibit == true)
			{
				return;
			}
			this.treeView1.ContextMenu = null;
		}

		private void treeView1_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			expandFlag = true;
		}

		private void treeView1_BeforeCollapse(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			expandFlag = true;
		}

		private void treeView1_AfterCollapse(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			expandFlag = false;
		}

		private void treeView1_AfterExpand(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			expandFlag = false;
		}

		#endregion TreeView Node Selection

		#region Data Retrieval Thread Wrappers
        private void ensureAllItemsInCustListHaveActValue()
        {
            if (this.sysInfo.nodes[tempTableIndex].numConsecutiveNoResponse >= 3)
            {
                return;
            }
            foreach (TreeNode node in this.tempTreeNode.Nodes)//topNode.Nodes)
            {
                if (this.progressBar1.Value < this.progressBar1.Maximum)
                {
                    // progressBar1.Value now updated from nodeValue in timer2_Elapsed() 
                    // to avoid cross threading.
                    nodeValue++;
                }
                treeNodeTag DNtag = (treeNodeTag)node.Tag;
                if (DNtag.assocSub != null)  //only add actual OD items to the table
                {
                    #region get actual value from source node if requried
                    foreach (DataRow row in MAIN_WINDOW.DWdataset.Tables[DCFTblIndex].Rows)
                    {
                        ODItemData dcfRowOdSub = (ODItemData)row[(int)TblCols.odSub];
                        if ((dcfRowOdSub.indexNumber == DNtag.assocSub.indexNumber) && (dcfRowOdSub.subNumber != -1))
                        {
                            //notw get the source odSUb
                            ODItemData srcODSub = this.sysInfo.nodes[tempTableIndex].getODSub(dcfRowOdSub.indexNumber, dcfRowOdSub.subNumber);
                            if (srcODSub != null)
                            {
                                //								if(srcODSub.displayType != CANopenDataType.DOMAIN) //do not routinely read domains
                                //								{
                                this.sysInfo.nodes[tempTableIndex].readODValue(srcODSub);
                                //								}
                                if (this.sysInfo.nodes[tempTableIndex].numConsecutiveNoResponse >= 3)
                                {
                                    return;
                                }
                                copyOdSubValueToCustomList(srcODSub, dcfRowOdSub);
                                if (this.tempDestTableIndex == MAIN_WINDOW.DCFTblIndex)
                                {
                                    fillTableRowColumns(dcfRowOdSub, row, MAIN_WINDOW.DCFTblIndex, false, true, this.sysInfo.nodes[this.tempTableIndex]);
                                }
                                else if (this.tempDestTableIndex == MAIN_WINDOW.GraphTblIndex)
                                {
                                    fillTableRowColumns(dcfRowOdSub, row, MAIN_WINDOW.GraphTblIndex, false, true, this.sysInfo.nodes[this.tempTableIndex]);
                                }
                            }
                            else
                            {
                                //??
                            }
                        }
                    }
                    #endregion get actual value from source node if requried
                }
                if (node.Nodes.Count > 0)
                {
                    this.tempTreeNode = node;
                    ensureAllItemsInCustListHaveActValue();
                    if (this.sysInfo.nodes[tempTableIndex].numConsecutiveNoResponse >= 3)
                    {
                        return;
                    }
                }
            }
            return;
        }
        private void readSevconDeviceSection()
        {
            bool timer3WasEnabled = false;
            if (this.timer3.Enabled == true)
            {
                timer3WasEnabled = true;
                this.timer3.Enabled = false;
            }
            sysInfo.nodes[currTblIndex].readODValue(sectiontype, false); //we are not reading DOMAINS here
            if (timer3WasEnabled == true)
            {
                this.timer3.Enabled = true;
            }
        }
        private void readCANOpenDeviceSection()
        {
            bool timer3WasEnabled = false;
            if (this.timer3.Enabled == true)
            {
                timer3WasEnabled = true;
                this.timer3.Enabled = false;
            }
            sysInfo.nodes[currTblIndex].readODValue(ThirdPartySection, false);
            if (timer3WasEnabled == true)
            {
                this.timer3.Enabled = true;
            }
        }
        private void retreiveDevicePanelData()
        {
            try
            {
                if ((nodeTag.tableindex < this.sysInfo.nodes.Length) && (this.sysInfo.nodes[nodeTag.tableindex].isSevconApplication() == true))
                {
                    if (this.sysInfo.nodes[nodeTag.tableindex].battVoltSub != null)
                    {
                        this.sysInfo.nodes[nodeTag.tableindex].readODValue(this.sysInfo.nodes[nodeTag.tableindex].battVoltSub);
                    }
                    if (this.sysInfo.nodes[nodeTag.tableindex].capvoltSub != null)
                    {
                        this.sysInfo.nodes[nodeTag.tableindex].readODValue(this.sysInfo.nodes[nodeTag.tableindex].capvoltSub);
                    }
                    if (this.sysInfo.nodes[nodeTag.tableindex].temperatureSub != null)
                    {
                        this.sysInfo.nodes[nodeTag.tableindex].readODValue(this.sysInfo.nodes[nodeTag.tableindex].temperatureSub);
                    }
                    if (this.sysInfo.nodes[nodeTag.tableindex].configChksumSub != null)
                    {
                        this.sysInfo.nodes[nodeTag.tableindex].readODValue(this.sysInfo.nodes[nodeTag.tableindex].configChksumSub);
                    }
                }
            }
            catch (Exception ex1)
            {
                SystemInfo.errorSB.Append("Exception in updateDevicePanel(): " + ex1.Message + " " + ex1.InnerException);
            }
        }
        private void readDataForDevicePanels()
        {
            DIFeedbackCode feedback;
            #region create device info arrays
            HWVersions = new string[this.sysInfo.nodes.Length];
            SWVersions = new string[this.sysInfo.nodes.Length];
            serviceDue = new string[this.sysInfo.nodes.Length];
            productCodes = new string[this.sysInfo.nodes.Length];
            serialNos = new string[this.sysInfo.nodes.Length];
            RevNos = new string[this.sysInfo.nodes.Length];
            VendorIDs = new string[this.sysInfo.nodes.Length];
            VendorNames = new string[this.sysInfo.nodes.Length];
            this.configChkSum = new string[this.sysInfo.nodes.Length];
            this.IntROMChksum = new string[this.sysInfo.nodes.Length];
            this.extROMChksum = new string[this.sysInfo.nodes.Length];
            FaultLEDFlashRate = new int[this.sysInfo.nodes.Length, 2];
            ActFaultsDS = new DataSet();
            motorProfilesPresent = new int[this.sysInfo.nodes.Length, 2];
            #endregion create device info arrays
            for (int CANodeIndex = 0; CANodeIndex < this.sysInfo.nodes.Length; CANodeIndex++)
            {
                FaultLEDFlashRate[CANodeIndex, 0] = 99;  //initilaly set everything to no fault
                #region 0x1018 identity subs
                this.sysInfo.nodes[CANodeIndex].readDeviceIdentity();

                if (this.sysInfo.nodes[CANodeIndex].productCode != 0xFFFFFFFF) //ie not defualt
                {
                    this.productCodes[CANodeIndex] = "Product code: \t" + "0x" + this.sysInfo.nodes[CANodeIndex].productCode.ToString("X").PadLeft(8, '0');
                }
                else
                {
                    this.productCodes[CANodeIndex] = "Product code: \tNot available";
                }
                if (this.sysInfo.nodes[CANodeIndex].revisionNumber != 0xFFFFFFFF)
                {
                    this.RevNos[CANodeIndex] = "Revision No: \t" + "0x" + this.sysInfo.nodes[CANodeIndex].revisionNumber.ToString("X").PadLeft(8, '0');
                }
                else
                {
                    this.RevNos[CANodeIndex] = "Revision No: \tNot available";
                }
                if (this.sysInfo.nodes[CANodeIndex].vendorID != 0xFFFFFFFF)
                {
                    this.VendorIDs[CANodeIndex] = "CiA Vendor ID: \t" + "0x" + this.sysInfo.nodes[CANodeIndex].vendorID.ToString("X").PadLeft(8, '0');
                }
                else
                {
                    this.VendorIDs[CANodeIndex] = "CiA Vendor ID: \tNot available";
                }
                this.VendorNames[CANodeIndex] = "Vendor: \t" + this.sysInfo.nodes[CANodeIndex].EDS_DCF.EDSdeviceInfo.vendorName;
                if (this.sysInfo.nodes[CANodeIndex].EDS_DCF.EDSdeviceInfo.vendorName == "")
                {
                    if (this.sysInfo.nodes[CANodeIndex].vendorID == SCCorpStyle.SevconID)
                    {
                        this.VendorNames[CANodeIndex] = "Vendor: \tTechOps Sevcon ";
                    }
                    else
                    {
                        this.VendorNames[CANodeIndex] = "Vendor: \tNot availabe";
                    }
                }
                #endregion 0x1018 identity subs
                #region serial No
                long serNo = 0;
                MAIN_WINDOW.appendErrorInfo = false; //switch off error info - we are just checking - will be switched back on in method
                ODItemData serialNumSub = this.sysInfo.nodes[CANodeIndex].getODSub(0x1018, 4);
                MAIN_WINDOW.appendErrorInfo = true; //ensure it is back on
                if (serialNumSub != null)
                {
                    feedback = this.sysInfo.nodes[CANodeIndex].readODValue(serialNumSub);
                    if (feedback == DIFeedbackCode.DISuccess)
                    {
                        if (MAIN_WINDOW.isVirtualNodes == true)
                        {
                            if (serNo == 0xFFFFFFFF)  //  ie a virtual node with No EDS
                            {
                                this.serialNos[CANodeIndex] = "Serial No: \tNot available"; //
                            }
                        }
                        else
                        {
                            this.serialNos[CANodeIndex] = "Serial No: \t" + serialNumSub.currentValue.ToString();
                        }
                    }
                }
                else
                {
                    this.serialNos[CANodeIndex] = "Serial No: \tNot available";
                }
                #endregion serial No
                #region node HW version
                HWVersions[CANodeIndex] = "HW version: \tNot available";
                ODItemData HWversionSub = null;
                if (this.sysInfo.nodes[CANodeIndex].isSevconBootloader() == true)
                {  //in bootloader EDS HW veriosn is defined as numeric = it's a visilbe string in application
                    HWversionSub = this.sysInfo.nodes[CANodeIndex].getODSubFromObjectType(SevconObjectType.BOOT_HW_VERSION, 0);
                }
                else
                {
                    MAIN_WINDOW.appendErrorInfo = false; //switch off error info - we are just checking - will be switched back on in method
                    HWversionSub = this.sysInfo.nodes[CANodeIndex].getODSub(0x1009, 0);
                    MAIN_WINDOW.appendErrorInfo = true; //ensure it is back on
                }
                if (HWversionSub != null)
                {
                    feedback = this.sysInfo.nodes[CANodeIndex].readODValue(HWversionSub);
                    if (feedback == DIFeedbackCode.DISuccess)
                    {
                        HWVersions[CANodeIndex] = "HW version: \t" + "0x" + HWversionSub.currentValueString.PadLeft(8, '0');
                    }
                }
                #endregion node HW version
                #region node SW version
                SWVersions[CANodeIndex] = "SW version: \tNot avialable";
                ODItemData SWversionSub = null;
                if (this.sysInfo.nodes[CANodeIndex].isSevconBootloader() == true)
                {
                    SWversionSub = this.sysInfo.nodes[CANodeIndex].getODSubFromObjectType(SevconObjectType.BOOT_SW_VERSION, 0);
                }
                else
                {
                    MAIN_WINDOW.appendErrorInfo = false; //switch off error info - we are just checking - will be switched back on in method
                    SWversionSub = this.sysInfo.nodes[CANodeIndex].getODSub(0x100A, 0); //just checking
                    MAIN_WINDOW.appendErrorInfo = true; //ensure it is back on
                }
                if (SWversionSub != null)
                { //item exists in od
                    feedback = this.sysInfo.nodes[CANodeIndex].readODValue(SWversionSub);
                    if (feedback == DIFeedbackCode.DISuccess)
                    {
                        SWVersions[CANodeIndex] = "SW version: \t" + SWversionSub.currentValueString;
                    }
                }

                #endregion node SW version
                #region create an Active faults table for each node
                //each devcie will hace a table for actve faults but some my be empty = try changing unused to null later for memory save
                devActiveFaultsTable activeTable = new devActiveFaultsTable();
                activeTable.TableName = "actTable" + CANodeIndex.ToString();
                activeTable.DefaultView.AllowNew = false;
                #endregion create an Active faults table for each node
                // now applies for Sevcon displays
                if (this.sysInfo.nodes[CANodeIndex].isSevconApplication() == true)
                {
                    #region service due
                    serviceDue[CANodeIndex] = "Service due: Not available";         //tsn - 11dec07
                    MAIN_WINDOW.appendErrorInfo = false; //judetemp - we seem to have things eg DBT that don't have this
                    ODItemData serviceSub = this.sysInfo.nodes[CANodeIndex].getODSubFromObjectType(SevconObjectType.SERVICE_CONFIG, 4);
                    MAIN_WINDOW.appendErrorInfo = true; //switch back on
                    if (serviceSub != null)
                    {
                        feedback = this.sysInfo.nodes[CANodeIndex].readODValue(serviceSub);
                        if (feedback == DIFeedbackCode.DISuccess)
                        {
                            serviceDue[CANodeIndex] = "Service due: " + serviceSub.currentValue.ToString() + " hours";
                        }
                    }
                    #endregion service due
                    #region EEPROM format
                    // read here so ready for first time shown
                    this.configChkSum[CANodeIndex] = "Config Chksum: Not available";     //tsn - 11dec07
                    if (this.sysInfo.nodes[CANodeIndex].configChksumSub != null)
                    {
                        feedback = this.sysInfo.nodes[CANodeIndex].readODValue(this.sysInfo.nodes[CANodeIndex].configChksumSub);
                        if (feedback == DIFeedbackCode.DISuccess)
                        {
                            this.configChkSum[CANodeIndex] = "Config Chksum: " + "0x" + this.sysInfo.nodes[CANodeIndex].configChksumSub.currentValue.ToString("X").PadLeft(4, '0');
                        }
                    }
                    #endregion EEPROM format
                    #region internal ROM checksum
                    this.IntROMChksum[CANodeIndex] = "Int. ROM checksum:Not available";     //tsn - 11dec07
                    ODItemData intRomChkSub = this.sysInfo.nodes[CANodeIndex].getODSubFromObjectType(SevconObjectType.INTERNAL_ROM_CHECKSUM, 0);
                    if (intRomChkSub != null)
                    {
                        feedback = this.sysInfo.nodes[CANodeIndex].readODValue(intRomChkSub);
                        if (feedback == DIFeedbackCode.DISuccess)
                        {
                            this.IntROMChksum[CANodeIndex] = "Int. ROM checksum: " + "0x" + intRomChkSub.currentValue.ToString("X").PadLeft(4, '0');
                        }
                    }
                    #endregion internal ROM checksum
                    #region external ROM checksum
                    this.extROMChksum[CANodeIndex] = "Ext. ROM checksum:Not available";     //tsn - 11dec07
                    ODItemData extRomChkSub = this.sysInfo.nodes[CANodeIndex].getODSubFromObjectType(SevconObjectType.EXTERNAL_ROM_CHECKSUM, 0);
                    if (extRomChkSub != null)
                    {
                        feedback = this.sysInfo.nodes[CANodeIndex].readODValue(extRomChkSub);
                        if (feedback == DIFeedbackCode.DISuccess)
                        {
                            this.extROMChksum[CANodeIndex] = "Ext. ROM checksum: " + "0x" + extRomChkSub.currentValue.ToString("X").PadLeft(4, '0');
                        }
                    }
                    #endregion external ROM checksum
                    #region fill active Fualt tables for SEVCON nodes only (rest are spacers for easier dereferencing)
                    NodeFaultEntry[] activelog = new NodeFaultEntry[0];
                    MAIN_WINDOW.appendErrorInfo = false; //display doesn't have active fualt item => info only so ignore feedback
                    ODItemData activeFaultListSub = this.sysInfo.nodes[CANodeIndex].getODSubFromObjectType(SevconObjectType.DOMAIN_UPLOAD, SCCorpStyle.ActiveFaultListSubObject);
                    feedback = DIFeedbackCode.DISuccess;

                    if (activeFaultListSub != null) //prevent exception if null
                    {
                        feedback = this.sysInfo.nodes[CANodeIndex].readODValue(activeFaultListSub);
                        this.sysInfo.processActivefaultLog(activeFaultListSub, out activelog, out unknownFaultIDs);  //DR38000269

                        #region if we can read missing events and we have some - then get them
                        //DR38000269
                        //NOTE: this entire procedure is already on a thread so no further thread is needed
                        ODItemData eventIDsSub = sysInfo.nodes[CANodeIndex].getODSubFromObjectType(SevconObjectType.EVENT_ID_DESCRIPTION, SCCorpStyle.EventIDSubObject);
                        ODItemData eventNameSub = sysInfo.nodes[CANodeIndex].getODSubFromObjectType(SevconObjectType.EVENT_ID_DESCRIPTION, SCCorpStyle.EventNameSubObject);
                        if ((eventIDsSub != null) && (eventNameSub != null) && (unknownFaultIDs.Count > 0))
                        for ( int i = 0; i < unknownFaultIDs.Count; i++)
                        {
                            long uknownFaultID = (long)((ushort)unknownFaultIDs[i]);
                            feedback = sysInfo.nodes[CANodeIndex].writeODValue(eventIDsSub, uknownFaultID);
                            
                            if (feedback == DIFeedbackCode.DISuccess)
                            {
                                feedback = sysInfo.nodes[CANodeIndex].readODValue(eventNameSub);

                                if (feedback == DIFeedbackCode.DISuccess)
                                {
                                    sysInfo.updateEventList((ushort)uknownFaultID, eventNameSub.currentValueString, sysInfo.nodes[CANodeIndex].productCode);
                                }
                            }

                            if (feedback == DIFeedbackCode.DINoResponseFromController)
                            {
                                break;  // bomb out of loop if controller not responding
                            }
                        }

                        //now re-process the log with the new strings available
                        this.sysInfo.processActivefaultLog(activeFaultListSub, out activelog, out unknownFaultIDs);
                        #endregion if we can read missing events and we have some - then get them

                        //.requestLog(LogType.ActiveFaultsLog, out activelog);
                    }
                    MAIN_WINDOW.appendErrorInfo = true;
                    if (feedback == DIFeedbackCode.DISuccess)
                    {
                        for (int actFltPtr = 0; actFltPtr < activelog.Length; actFltPtr++)
                        {
                            DataRow row = activeTable.NewRow();
                            row[0] = activelog[actFltPtr].description;
                            activeTable.Rows.Add(row);
                        }
                        if (activelog.Length > 0)
                        {
                            int groupIDMask = 0x03C0;
                            this.FaultLEDFlashRate[CANodeIndex, 0] = (int)((activelog[0].eventID & groupIDMask) >> 6);
                        }
                    }
                    #endregion fill active Fualt tables for SEVCON nodes only (rest are spacers for easier dereferencing)
                    #region determine how many motors this node is contolling (for self char)
                    //eash espAC csan contain 0,1, or 2 motor profiles. Self characterization can be carried out on 
                    //either profile. So each device could have zero, 1 or 2 associated self char processes - these will be represented by 
                    //treenodes the ultimately write back to the corret profile area of the OD. (Further changes to self char also needed
                    int[] profileEnds = { 0x67FF, 0x6FFF, 0x77FF, 0x7FFF, 0x87FF, 0x8FFF, 0x97FF, 0x9FFF };
                    for (int profilePtr = 0; profilePtr < profileEnds.Length; profilePtr++)
                    {
                        MAIN_WINDOW.appendErrorInfo = false; //switch off error info - we are just checking - will be switched back on in method
                        ObjDictItem odItem = this.sysInfo.nodes[CANodeIndex].getODItemAndSubs(profileEnds[profilePtr]);
                        MAIN_WINDOW.appendErrorInfo = true; //ensure it is back on
                        if (odItem != null) //ie this profile exists inth eEDs - so let's see what flavour it is
                        {
                            ODItemData firstSub = (ODItemData)odItem.odItemSubs[0];
                            //							ODItemData ODsub = this.sysInfo.nodes[CANodeIndex].getODSub(profileEnds[profilePtr], 0);  //this is sub 2 due to bitsplittting - we have already filtered for Sevcon appplication

                            //if it exists then go and read it from device
                            ODItemData odSub;
                            if (firstSub.format == SevconNumberFormat.BIT_SPLIT)  //this will be a single sub item split into subindexes
                            {
                                odSub = this.sysInfo.nodes[CANodeIndex].getODSub(profileEnds[profilePtr], 1);  //this is sub 1 due to bitsplittting - we have already filtered for Sevcon appplication
                            }
                            else //not bitsplit in EDS
                            {
                                odSub = this.sysInfo.nodes[CANodeIndex].getODSub(profileEnds[profilePtr], 0);
                            }
                            if (odSub != null)
                            {
                                feedback = this.sysInfo.nodes[CANodeIndex].readODValue(odSub);
                            }
                            if (feedback == DIFeedbackCode.DISuccess)
                            {
                                if (firstSub.format == SevconNumberFormat.BIT_SPLIT)
                                {
                                    #region device type object is bisplit in EDS
                                    if (odSub.currentValue == 0x192)    //it is a motor profile
                                    {
                                        if (this.motorProfilesPresent[CANodeIndex, 0] == 0)
                                        {
                                            #region first motor profile
                                            this.motorProfilesPresent[CANodeIndex, 0] = profileEnds[profilePtr];
                                            if ((MAIN_WINDOW.currentProfile != null) && (MAIN_WINDOW.currentProfile.connectedMotors.Count == 0))
                                            {
                                                //add a motor
                                                VPMotor motor = new VPMotor();
                                                MAIN_WINDOW.currentProfile.connectedMotors.Add(motor);
                                            }
                                            #endregion first motor profile
                                        }
                                        else
                                        {
                                            #region second motor profile
                                            this.motorProfilesPresent[CANodeIndex, 1] = profileEnds[profilePtr];
                                            if ((MAIN_WINDOW.currentProfile != null) && (MAIN_WINDOW.currentProfile.connectedMotors.Count == 1))
                                            {
                                                //add a motor
                                                VPMotor motor = new VPMotor();
                                                MAIN_WINDOW.currentProfile.connectedMotors.Add(motor);
                                            }
                                            #endregion second motor profile
                                        }
                                    }
                                    #endregion device type object is bisplit in EDS
                                }
                                else
                                {
                                    #region device type object is not bitsplit in EDS
                                    int lowerPart = (int)odSub.currentValue & 0xFFFF; //extract lower teo bytes
                                    if (lowerPart == 0x192)  //it is a motor profile
                                    {
                                        if (this.motorProfilesPresent[CANodeIndex, 0] == 0)
                                        {
                                            #region first motor profile
                                            this.motorProfilesPresent[CANodeIndex, 0] = profileEnds[profilePtr];
                                            if ((MAIN_WINDOW.currentProfile != null) && (MAIN_WINDOW.currentProfile.connectedMotors.Count == 0))
                                            {
                                                //add a motor
                                                VPMotor motor = new VPMotor();
                                                MAIN_WINDOW.currentProfile.connectedMotors.Add(motor);
                                            }
                                            #endregion first motor profile
                                        }
                                        else
                                        {
                                            #region second motor profile
                                            this.motorProfilesPresent[CANodeIndex, 1] = profileEnds[profilePtr];
                                            if ((MAIN_WINDOW.currentProfile != null) && (MAIN_WINDOW.currentProfile.connectedMotors.Count == 1))
                                            {
                                                //add a motor
                                                VPMotor motor = new VPMotor();
                                                MAIN_WINDOW.currentProfile.connectedMotors.Add(motor);
                                            }
                                            #endregion second motor profile
                                        }
                                    }
                                    #endregion device type object is not bitsplit in EDS
                                }
                            }
                        }
                    }
                    #endregion determine how many motors this node is contolling (for self char)
                }
                ActFaultsDS.Tables.Add(activeTable);
            }
            if (SystemInfo.errorSB.Length > 0)
            {
                sysInfo.displayErrorFeedbackToUser("Unable to read all data for Device Status panel:");
            }
        }
        private void readTableData(object data)
        {
            treeNodeTag nodeTag = (treeNodeTag)data;
            progressBarValueThreaded = 0;

            foreach (DataRowView DVrow in MAIN_WINDOW.DWdataset.Tables[nodeTag.tableindex].DefaultView)
            {
                DIFeedbackCode feedback;
                DataRow row = DVrow.Row;  //conver to a proper DataRow
                ODItemData odSub = (ODItemData)row[(int)TblCols.odSub];
                progressBarValueThreaded++;
                if (odSub.subNumber != -1)
                {
                    if ((CANopenDataType)odSub.dataType != CANopenDataType.DOMAIN)
                    {
                        feedback = this.sysInfo.nodes[currTblIndex].readODValue(odSub);
                        if (feedback == DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                        {
                            // error message already build up due to DIThreeConsecutiveNonResponseFromDevice
                            break;
                        }
                    }
                    MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged -= new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
                    this.fillTableRowColumns(odSub, row, MAIN_WINDOW.DCFTblIndex, false, true, sysInfo.nodes[currTblIndex]);
                    MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
                }
            }
        }

        private void submitToDevice(object data)
        {
            treeNodeTag nodeTag = (treeNodeTag)data;
            DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
            
            for (int i = 0; i < DWdataset.Tables[nodeTag.tableindex].Rows.Count; i++)
			{
				#region clear row error flags
                if (DWdataset.Tables[nodeTag.tableindex].Rows[i].HasErrors)
				{
                    DWdataset.Tables[nodeTag.tableindex].Rows[i].RejectChanges();
                    DWdataset.Tables[nodeTag.tableindex].Rows[i].ClearErrors();
				}
				#endregion
                if (DWdataset.Tables[nodeTag.tableindex].Rows[i].RowState == DataRowState.Modified)
				{
					#region params for this user entry
                    string input = DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)].ToString();
					string inputUpperCase = input.ToUpper();
					long newValue = 0;
					double tempdouble = 0;
                    ODItemData odSub = (ODItemData)(DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.odSub)]);
					#endregion params for this user entry
					#region switch datatype
					switch ((CANopenDataType) odSub.dataType)
					{
						case CANopenDataType.VISIBLE_STRING:
						case CANopenDataType.UNICODE_STRING:
						case CANopenDataType.OCTET_STRING:
							#region string - no error checking required
                            if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                            {
                                feedback = this.sysInfo.nodes[nodeTag.tableindex].writeODValue(odSub, input);
                            }
							#region read back string 
							odSub.currentValueString = "";
							if((odSub.accessType != DriveWizard.ObjectAccessType.WriteOnly)
								&& (odSub.accessType != DriveWizard.ObjectAccessType.WriteOnlyInPreOp)
                                && (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice))
							{
                                feedback = this.sysInfo.nodes[nodeTag.tableindex].readODValue(odSub);
							}
							//display current vlaue regrdless of success - it's the best infor we have
                            DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = odSub.currentValueString;
							#endregion read back string 
							#endregion string - no error checking required
							break;

						case CANopenDataType.UNSIGNED8:
						case CANopenDataType.UNSIGNED16:
						case CANopenDataType.UNSIGNED24:
						case CANopenDataType.UNSIGNED32:
						case CANopenDataType.UNSIGNED40:
						case CANopenDataType.UNSIGNED48:
						case CANopenDataType.UNSIGNED56:
						case CANopenDataType.UNSIGNED64:
						case CANopenDataType.INTEGER8:
						case CANopenDataType.INTEGER16:
						case CANopenDataType.INTEGER24:
						case CANopenDataType.INTEGER32:
						case CANopenDataType.INTEGER40:
						case CANopenDataType.INTEGER48:
						case CANopenDataType.INTEGER56:
						case CANopenDataType.INTEGER64:
							#region numerical
							bool errorFound = false;
							if(odSub.format == SevconNumberFormat.SPECIAL)
							{
								#region SPECIAL
								//get the equivalent integer
								bool dereferencedOK = false;
								newValue = this.sysInfo.getValueFromEnumeration(input,  odSub.formatList, out dereferencedOK);
								if(dereferencedOK == true)
								{
									#region write and readback input value
                                    if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                    {
                                        feedback = this.sysInfo.nodes[nodeTag.tableindex].writeODValue(odSub, newValue);
                                    }
									#region read back SPECIAL
									if((odSub.accessType != DriveWizard.ObjectAccessType.WriteOnly)
										&& (odSub.accessType != DriveWizard.ObjectAccessType.WriteOnlyInPreOp))
									{
                                        if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                        {
                                            feedback = this.sysInfo.nodes[nodeTag.tableindex].readODValue(odSub);
                                        }
                                        DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = sysInfo.getEnumeratedValue(odSub.formatList, odSub.currentValue);
									}
									else  //write only so just blank the entry
									{
                                        DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = "";
									}
									#endregion read back SPECIAL
									#endregion write and readback input value
								}
								else
								{ 
									#region reject changes and append user feedback
									SystemInfo.errorSB.Append("/nCould not dereference enumertion:");
									SystemInfo.errorSB.Append(input);
                                    DWdataset.Tables[nodeTag.tableindex].Rows[i].RejectChanges();
									#endregion reject changes and append user feedback
								}
								#endregion SPECIAL
							}
							else if(odSub.format == SevconNumberFormat.BASE16)
							{
								#region BASE16 
								try
								{	//no scaling for base 16 format
									newValue = System.Convert.ToInt64(input,16);  //convert to long and base 10
								}
								catch
								{
									errorFound = true;
								}
								if(errorFound == false)
								{
                                    if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                    {
                                        feedback = this.sysInfo.nodes[nodeTag.tableindex].writeODValue(odSub, newValue);
                                    }
									#region read back BASE16
									if((odSub.accessType != DriveWizard.ObjectAccessType.WriteOnly)
										&& (odSub.accessType != DriveWizard.ObjectAccessType.WriteOnlyInPreOp))
									{
                                        if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                        {
                                            feedback = this.sysInfo.nodes[nodeTag.tableindex].readODValue(odSub);
                                        }
										//back into hex format for display
                                        DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = "0x" + odSub.currentValue.ToString("X");
									}
									else
									{
                                        DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = "";
									}
									#endregion read back BASE16
								}
								else
								{
									#region reject changes and append feedback
									SystemInfo.errorSB.Append("/nCould not convert input to hex:");
									SystemInfo.errorSB.Append(input);
                                    DWdataset.Tables[nodeTag.tableindex].Rows[i].RejectChanges();
									#endregion reject changes and append feedback
								}
								#endregion BASE16
							}
							else if (odSub.format == SevconNumberFormat.BASE10)
							{
								#region BASE10  Or third party
								#region test for hex input and convert to float
								bool hexFlag = false;
								if(inputUpperCase.IndexOf("0X") == 0)//is hex
								{
									#region hex input
									hexFlag = true;
									try
									{
										newValue = System.Convert.ToInt64(inputUpperCase,16);
										if(odSub.scaling != 1)
										{
											tempdouble = System.Convert.ToDouble(newValue);
											tempdouble = (tempdouble/odSub.scaling);
											newValue = System.Convert.ToInt64(tempdouble); //use this because a cast just truncates the value
										}
									}
									catch
									{
										#region set error flag and append user feedback
										SystemInfo.errorSB.Append("/nCould not convert input to hex:");
										SystemInfo.errorSB.Append(input);
										errorFound = true;
										#endregion set error flag and append user feedback
									}
									#endregion hex input
								}
								else
								{ 
									#region base 10 input
									if(odSub.scaling == 1)
									{
										try
										{
                                            newValue = long.Parse(DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)].ToString(), System.Globalization.NumberStyles.Any);
										}
										catch
										{
											#region set error flag and append user feedback
											SystemInfo.errorSB.Append("/nCould not convert input to int64:");
											SystemInfo.errorSB.Append(input);
											errorFound = true;
											#endregion set error flag and append user feedback
										}
									}
									else
									{ //only do float conversion if we have non-unity scaling
										try
										{
                                            tempdouble = double.Parse(DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)].ToString(), System.Globalization.NumberStyles.Any);
											tempdouble = (tempdouble/odSub.scaling);
											newValue = System.Convert.ToInt64(tempdouble); //use this because a cast just truncates the value
										}
										catch
										{
											#region set error flag and append user feedback
											SystemInfo.errorSB.Append("/nCould not convert input to float");
											SystemInfo.errorSB.Append(input);
											errorFound = true;
											#endregion set error flag and append user feedback
										}
									}
									#endregion base 10 input
								}
								#endregion
								if(errorFound == false)
								{
									#region attempt  to write to node
                                    if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                    {
                                        feedback = this.sysInfo.nodes[nodeTag.tableindex].writeODValue(odSub, newValue);
                                    }
									#endregion attempt  to write to node
									#region read back device value and display unluess write only
									if((odSub.accessType != DriveWizard.ObjectAccessType.WriteOnly)
										&& (odSub.accessType != DriveWizard.ObjectAccessType.WriteOnlyInPreOp))
									{
										#region get scaled value back from node
                                        if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                        {
                                            feedback = this.sysInfo.nodes[nodeTag.tableindex].readODValue(odSub);
                                        }
										if(odSub.scaling != 1)
										{
											if(hexFlag == true)
											{
												long temp = System.Convert.ToInt64(odSub.scaling * odSub.currentValue); //do coversion first - cant't have decimal places in hex
                                                DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = "0x" + temp.ToString("X");
											}
											else
											{
                                                DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = (odSub.scaling * odSub.currentValue).ToString("G5"); ;
											}
										}
										else
										{ //dispaly bakc tyo user in format he enetered vlaue i for read ability
											if(hexFlag == true)
											{
                                                DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = "0x" + odSub.currentValue.ToString("X");
											}
											else
											{
                                                DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = (odSub.currentValue).ToString("G5"); ;
											}
										}
										#endregion
									}
									else
									{ //write only so 'hide' our copy of current vlaue
                                        DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = "";
									}
									#endregion read back device value and display unluess write only
								}
								else
								{
									#region reject changes and append feedback
									SystemInfo.errorSB.Append("\nUnrecognised number format:");
									SystemInfo.errorSB.Append(odSub.format.ToString());
                                    DWdataset.Tables[nodeTag.tableindex].Rows[i].RejectChanges();
									#endregion reject changes and append feedback
								}
								#endregion BASE10
							}
							else if (odSub.format == SevconNumberFormat.BIT_SPLIT)
							{
								#region bit split - reject changes and append feedback
								//should not occur
								SystemInfo.errorSB.Append("\nUnexpected bit slit. Index:0x");
								SystemInfo.errorSB.Append(odSub.indexNumber.ToString("X"));
								SystemInfo.errorSB.Append(" sub:0x");
								SystemInfo.errorSB.Append(odSub.subNumber.ToString("X").PadLeft(3, '0'));
                                DWdataset.Tables[nodeTag.tableindex].Rows[i].RejectChanges();
								#endregion bit split - reject changes and append feedback
							}
							break;
							#endregion

						case CANopenDataType.BOOLEAN:
							#region boolean
							//get the equivalent integer
							bool booldereferencedOK = false;
							newValue = this.sysInfo.getValueFromEnumeration(input, odSub.formatList, out booldereferencedOK);
                            if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                            {
                                feedback = this.sysInfo.nodes[nodeTag.tableindex].writeODValue(odSub, newValue);
                            }
							if((odSub.accessType != DriveWizard.ObjectAccessType.WriteOnly)
								&& (odSub.accessType != DriveWizard.ObjectAccessType.WriteOnlyInPreOp))
							{
                                if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                {
                                    feedback = this.sysInfo.nodes[nodeTag.tableindex].readODValue(odSub);
                                }
                                DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = sysInfo.getEnumeratedValue(odSub.formatList, odSub.currentValue);
							}
							else  //write only so just blank the entry
							{
                                DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = "";
							}
							#endregion
							break;

						case CANopenDataType.REAL32:
							#region REAL 32
							if(odSub.real32 != null)
							{
                                tempdouble = double.Parse(DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)].ToString(), System.Globalization.NumberStyles.Any);
								#region attempt  to write to node
                                if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                {
                                    feedback = this.sysInfo.nodes[nodeTag.tableindex].writeODValue(odSub, tempdouble);
                                }
								#endregion attempt to read back from node
								if((odSub.accessType != DriveWizard.ObjectAccessType.WriteOnly)
									&& (odSub.accessType != DriveWizard.ObjectAccessType.WriteOnlyInPreOp))
								{ 
									#region get scaled value back from node
                                    if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                    {
                                        feedback = this.sysInfo.nodes[nodeTag.tableindex].readODValue(odSub);
                                    }
                                    DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = odSub.real32.currentValue.ToString();
									#endregion
								}
								else
								{
                                    DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = "";
								}
							}
							#endregion REAL 32
							break;

						case CANopenDataType.REAL64:
							#region REAL 64
							if(odSub.real64 != null)
							{
                                double tempDouble = double.Parse(DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)].ToString(), System.Globalization.NumberStyles.Any);
								#region attempt  to write to node
                                if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                {
                                    feedback = this.sysInfo.nodes[nodeTag.tableindex].writeODValue(odSub, tempDouble);
                                }
								#endregion attempt to read back from node
								if((odSub.accessType != DriveWizard.ObjectAccessType.WriteOnly)
									&& (odSub.accessType != DriveWizard.ObjectAccessType.WriteOnlyInPreOp))
								{
									#region get scaled value back from node
                                    if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                    {
                                        feedback = this.sysInfo.nodes[nodeTag.tableindex].readODValue(odSub);
                                    }
                                    DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = odSub.real64.currentValue.ToString();
									#endregion
								}
								else
								{
                                    DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = "";
								}
							}
							#endregion REAL 64
							break;

						case CANopenDataType.TIME_DIFFERENCE:
						case CANopenDataType.TIME_OF_DAY:
							bool absTime = false;
							//note we have assumed scaling of 1 for time since Sevcon doesn't use time variables
							if((CANopenDataType) odSub.dataType == CANopenDataType.TIME_OF_DAY)
							{
								absTime = true;
							}
							#region test for hex input and convert to float
							if(inputUpperCase.IndexOf("0X") == 0)//need at least three chars for hex text and for this test
							{
								#region hex input
								try
								{  
									newValue = (long) System.Convert.ToInt64(inputUpperCase,16);
								}
								catch
								{
									#region set error flag and append user feedback
									SystemInfo.errorSB.Append("\n Unable to convert to hex:");
									SystemInfo.errorSB.Append(input);
									errorFound = true;
									#endregion set error flag and append user feedback
								}
								#endregion hex input
							}
							else
							{
								#region base 10 input
								try
								{
                                    newValue = System.Int64.Parse(DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)].ToString(), System.Globalization.NumberStyles.Any);
								}
								catch
								{
									#region set error flag and append user feedback
									SystemInfo.errorSB.Append("\n Unable to convert to Int64:");
									SystemInfo.errorSB.Append(input);
									errorFound = true;
									#endregion set error flag and append user feedback
								}
								#endregion base 10 input
							}
							//we now have temp time expressed as Uint64 - we can send this to the node
							#region attempt  to write to node
                            if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                            {
                                feedback = this.sysInfo.nodes[nodeTag.tableindex].writeODValue(odSub, newValue);
                            }
							#endregion attempt  to write to node
							if((odSub.accessType != DriveWizard.ObjectAccessType.WriteOnly)
								&& (odSub.accessType != DriveWizard.ObjectAccessType.WriteOnlyInPreOp))
							{
								#region get time value back from node
                                if (feedback != DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice)
                                {
                                    feedback = this.sysInfo.nodes[nodeTag.tableindex].readODValue(odSub);
                                }
								//override the change handler sinc ewe are representing this number as a formatted string
								MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged -= new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
                                DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = this.getTime(odSub.currentValue, absTime);
								//OK switch the change handler back on to check user typing next time
								MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
								#endregion
							}
							else
							{
                                DWdataset.Tables[nodeTag.tableindex].Rows[i][(int)(TblCols.actValue)] = "";
							}
							break;
							#endregion

						default: //belt and braces
#if DEBUG
							#region user feedback and reject any changes
							//jude -swithced off for now - we have  abug whereby chaging one row can set other rows RowState to modified - no time to chase it down yet - needs real system
//							SystemInfo.errorSB.Append("\nError on index:0x");
//							SystemInfo.errorSB.Append(odSub.indexNumber.ToString("X"));
//							SystemInfo.errorSB.Append(" sub:0x");
//							SystemInfo.errorSB.Append(odSub.subNumber.ToString("X").PadLeft(3, '0'));
//							SystemInfo.errorSB.Append("\nInvalid Data type: " + ((CANopenDataType) odSub.dataType).ToString());
                            DWdataset.Tables[nodeTag.tableindex].Rows[i].RejectChanges();
							break;
							#endregion user feedback and reject any changes
#endif
					}  //end switch
					#endregion
                    DWdataset.Tables[nodeTag.tableindex].Rows[i].AcceptChanges();  //finally mark the row as unmodified
				}  //end of  if row is modified
            }
        }

		#endregion Data Retrieval Thread Wrappers

		#region TreeView Drag & Drop methods 
		private void treeView1_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
		{
			statusBarOverrideText = this.AllType1FormsClosed();
			string type1Wind = this.AllType1FormsClosed();
			if(type1Wind != "")
			{
				this.statusBarPanel3.Text = "Drag & Drop inhibited while " + type1Wind + " active";
				return;
			}
			if(MAIN_WINDOW.UserInputInhibit == true) 
			{
				this.statusBarPanel3.Text = "Drag & Drop inhibited during data processing";
				return;
			}
			treeNodeTag senderNodeTag = (treeNodeTag) treeView1.SelectedNode.Tag;
			if(this.SystemPDOsScreenActive == false)
			{
				//				TreeNode rootNode = this.treeView1.SelectedNode;
				//				while(rootNode.Parent!= null)
				//				{
				//					rootNode = rootNode.Parent;
				//				}
				//check for an unknown devcie
				if( ((senderNodeTag.tableindex<this.sysInfo.nodes.Length) 
					&& (this.sysInfo.nodes[senderNodeTag.tableindex].manufacturer == Manufacturer.UNKNOWN))  //Unknown CAN device
					|| (senderNodeTag.tableindex>=this.sysInfo.nodes.Length))  //cannot drag items in Custom Lists
				{
					return; //cannot drag an Maunifacturer.Unknown CANopen device anywhere
				}
				//determine whethe rthis node is suitable for dragging
				if( (this.treeNodeIsDeviceLevelSpecialNode(treeView1.SelectedNode) == true)
					|| (this.treeNodeIsSystemLevelSpecialNode(treeView1.SelectedNode) == true)
					||(this.treeNodeIsCustomListNode(treeView1.SelectedNode) == true))
				{
					return;  //non-draggable node
				}
			}
			else //ie this.SystemPDOsScreenActive == true
			{ 
				#region COB/SystemPDO screen
				if((this.activeIntPDO == null) && (this.currCOB == null))  //either we have a single active IntPDO or a System PDO - with assoc internal mappings
				{
					return;
				}
				if(this.activeIntPDO == null)
				{
					#region check currCOB is PDO
					if(this.currCOB.messageType != COBIDType.PDO) //which has to be a PDO
					{
						this.statusBarPanel3.Text = "Currnetly selected COB is not a DPO";
						return;
					}
					#endregion check currCOB is PDO
					#region chekc for zero Tx and Rx nodes -  should never happen
					if((this.currCOB.transmitNodes.Count == 0) && (this.currCOB.receiveNodes.Count == 0))
					{//with some routing to map into
						this.statusBarPanel3.Text = "Current PDO has no Tx node and no Rx node"; //should never happen
						return;
					}
					#endregion chekc for zero Tx and Rx nodes -  should never happen
					if(this.currCOB.transmitNodes.Count>1)
					{
						this.statusBarPanel3.Text = "Current PDO has multiple transmit CAN nodes";
						return;
					}
					#region check for multiple Rx or Tx CAN nodes
					if((senderNodeTag.tagType == TNTagType.PreSetPDO_PowerSteer)
						|| (senderNodeTag.tagType == TNTagType.PreSetPDO_Pump)
						|| (senderNodeTag.tagType == TNTagType.PreSetPDO_TractionLeft)
						|| (senderNodeTag.tagType == TNTagType.PreSetPDO_TractionRight))
					{
						if(this.currCOB.receiveNodes.Count>1)
						{
							this.statusBarPanel3.Text = "PrRe-Set PDOs cannot have multiple transmit CAN nodes";
							return;
						}
					}
					#endregion check for multiple Rx or Tx CAN nodes
					#region check Tree node type - Od items and prefilled PDOs only
					if((senderNodeTag.tagType != TNTagType.ODITEM) 
						&& (senderNodeTag.tagType != TNTagType.ODSUB)
						&& (senderNodeTag.tagType != TNTagType.PreSetPDO_PowerSteer)
						&& (senderNodeTag.tagType != TNTagType.PreSetPDO_Pump)
						&& (senderNodeTag.tagType != TNTagType.PreSetPDO_TractionLeft)
						&& (senderNodeTag.tagType != TNTagType.PreSetPDO_TractionRight))
					{
						this.statusBarPanel3.Text = "Only Od parameters and pre-filled Mappings can be dragged";
						return;
					}
					#endregion check Tree node type - Od items and prefilled PDOs only
					#region check for OD header
					if((senderNodeTag.assocSub != null) && (senderNodeTag.assocSub.subNumber == -1))
					{
						this.statusBarPanel3.Text = "Object Dictionary header items cannot be dragged";
						return;
					}
					#endregion check for OD header
				}
				#endregion COB/SystemPDO screen
			}
			gTV = treeView1.CreateGraphics();
			shadowText = treeView1.SelectedNode.Text;
			SizeF dragStringSizeF = gTV.MeasureString("shadowText", this.mainWindowFont);
			this.dragStrSize = new Size(((int) dragStringSizeF.Width) + 100,(int) dragStringSizeF.Height);
			removeDragStringRectTV = new Rectangle(0,0,0,0);
			treeView1.DoDragDrop(new DataObject("System.Windows.Forms.TreeNode", treeView1.SelectedNode), DragDropEffects.Copy);
			treeView1.Invalidate(removeDragStringRectTV);  //remove old text
		}

		private void treeView1_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			Point MousePt = new Point(0,0);
			MousePt = this.treeView1.PointToClient(new Point(e.X, e.Y));
			TreeNode targetNode = this.treeView1.GetNodeAt(MousePt.X, MousePt.Y);
			#region check for vlaid target node
			if((targetNode == null)
				|| ((targetNode != DCFCustList_TN)  && (targetNode != GraphCustList_TN)))
			{
				return;
			}
			#endregion check for vlaid target node
			TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
			this.hideUserControls();
			if(targetNode == DCFCustList_TN)
			{
				treeNodeTag drgdTNtag = (treeNodeTag) draggedNode.Tag;
				DCFSourceTableIndex = drgdTNtag.tableindex;
			}
			addItemToCustomList(draggedNode, targetNode, true);
			//do NOT switch to DCF, graph node here 
			//- data retrieval thread may still be running
			nextRequiredNode = targetNode;//once thread has done it will deal with it
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("");
			}
		}
		/// <summary>
		/// This methid is called regrdles of whether nodes were dragged to custom list or 
		/// </summary>
		/// <param name="draggedNode"></param>
		/// <param name="targetNode"></param>
		/// <param name="dragDrop"></param>
		private void addItemToCustomList(TreeNode draggedNode, TreeNode targetNode, bool dragDrop)
		{ //if monitoring then draggedNode is the odItem equivalent ndoe at this point
			treeNodeTag drgdTNtag = (treeNodeTag) draggedNode.Tag;
			#region setup parameter values dependent on Custom List
			int cListTblIndex = MAIN_WINDOW.GraphTblIndex;
			TreeNode nextCustListNode = this.GraphCustList_TN;  //start at the top
			string cListStr = "monitoring list";
			if(targetNode == DCFCustList_TN) //DCF Node is currently empty - so set up device Info
			{
				#region DCF
				cListTblIndex = MAIN_WINDOW.DCFTblIndex;
				nextCustListNode = DCFCustList_TN;  //start at the top
				cListStr = "DCF store";
				if(DCFCustList_TN.Nodes.Count == 0)
				{
					#region create a Device Infor structure for hte DCf independent of DCF source
					this.DCFSourceTableIndex = drgdTNtag.tableindex;
					this.sysInfo.DCFnode.cloneSourceToDCF( this.sysInfo.nodes[drgdTNtag.tableindex]);
					this.DCFCustList_TN.Text = "DCF store (Source: Node ID " + this.sysInfo.DCFnode.nodeID.ToString() + ")";
					#endregion create a Device Infor structure for hte DCf independent of DCF source
				}
				#endregion DCF
			}
			else if (targetNode == this.GraphCustList_TN) 
			{
				#region monitor Store
				if(GraphCustList_TN.Nodes.Count ==0)
				{
					monStore = new myMonitorStore();  //create a new monitoring store ready for filling
				}
				#endregion monitor Store
			}
			#endregion setup parameter values dependent on Custom List
			//first need to add draggedNode an all its non-duplicate parents to the custonm list
			ArrayList nodeRoute= new ArrayList();
			this.statusBarPanel3.Text = "Adding parent nodes to " + cListStr;
			#region add a shallow copy of the dragged node to the Arraylist nodeRoute
			//we only want to copy the node text not all the child nodes etc
			TreeNode tempNode = new TreeNode(draggedNode.Text,  draggedNode.ImageIndex,draggedNode.SelectedImageIndex);  
			//create a node tag for this new custom list node, using data from dragged node but change tableIndex to DCF
			tempNode.Tag = new treeNodeTag(drgdTNtag.tagType,drgdTNtag.nodeID, cListTblIndex, drgdTNtag.assocSub);
			TreeNode currentNode = draggedNode;
			#endregion add a shallow copy of the dragged node to the Arraylist
			#region add a shallow copy of the each node in the dragged nodes parent path to the ArrayList nodeRoute
			bool rootFound = false;
			int limiter = 0;
			while (rootFound == false)
			{
				if(currentNode.Parent != this.SystemStatusTN)
				{
					//we only want to copy the node text not all the child nodes etc
					tempNode = new TreeNode(currentNode.Parent.Text,  currentNode.Parent.ImageIndex,currentNode.Parent.SelectedImageIndex);  
					treeNodeTag parentTag = (treeNodeTag) currentNode.Parent.Tag;
					tempNode.Tag = new treeNodeTag(parentTag.tagType, parentTag.nodeID, cListTblIndex, null);
					nodeRoute.Add(tempNode);
					currentNode = currentNode.Parent;
				}
				else
				{
					tempNode.ImageIndex = (int) TVImages.msOffOff;
					tempNode.SelectedImageIndex = (int) TVImages.msOffOff;
					rootFound = true;
				}
				limiter++;
				if(limiter>=15)
				{
					SystemInfo.errorSB.Append("Failed to locate root node,Loop limit exceeded");
					rootFound = true; //B&B
					return;
				}
			}
			nodeRoute.Reverse(); //reverse the array list so that root node is now first
			#endregion add a shallow copy of the each node in the dragged nodes parent path to the ArrayList nodeRoute
			#region add each node in the dragged nodes upwards parent path ONLY if it is not already in the custom list
			for(int i = 0;i<nodeRoute.Count;i++)
			{
				tempNode = (TreeNode) nodeRoute[i];
				#region determine whether this node is already represented in the DCF Bucket
				bool alreadyInCustListStructure = false;
				foreach(TreeNode node in nextCustListNode.Nodes)
				{
					if (node.Text == tempNode.Text) //this node is alreadt in DCF structure 
					{
						alreadyInCustListStructure = true;
						nextCustListNode = node; //we have to use a node that is already attahced to the tree so we can see its child nodes
						break;
					}
				}
				#endregion determine whether this node is already represented in the DCF Bucket
				if((alreadyInCustListStructure == false) && (i<nodeRoute.Count) )
				{
					TreeNode newNode = new TreeNode();
					treeNodeTag tempNodeTag = (treeNodeTag) tempNode.Tag;
					newNode = (TreeNode) tempNode.Clone();
					newNode.Tag = new treeNodeTag(tempNodeTag.tagType,tempNodeTag.nodeID, cListTblIndex, tempNodeTag.assocSub);
					nextCustListNode.Nodes.Add(newNode);
					nextCustListNode = newNode;  //we can use this now that it has been added to the DCF structure
					if(nextCustListNode.Parent == this.GraphCustList_TN)
					{
						//we are adding a representation of a Connected node
						//check if this connected node is also represented in the 
						//Arraylist of monitoring 'CANnodes'. If not then add it
						bool CANNodeAlreadyinArrayList = false;
						foreach(nodeInfo mnNode in this.monStore.myMonNodes)
						{
							if(mnNode.nodeID == drgdTNtag.nodeID)
							{
								CANNodeAlreadyinArrayList = true;
								break;
							}
						}
						if(CANNodeAlreadyinArrayList == false)
						{
							#region create CANnode in monitoring store
							//nodeInfo - all we need to supply currently is the node ID and the 0x1018 info
							//we may extent this later to extend OD when we drag items to the monitoring store
							nodeInfo mnNode = new nodeInfo(MAIN_WINDOW.GraphTblIndex, drgdTNtag.nodeID, 
								this.sysInfo.nodes[drgdTNtag.tableindex].vendorID,
								this.sysInfo.nodes[drgdTNtag.tableindex].productCode,
								this.sysInfo.nodes[drgdTNtag.tableindex].revisionNumber,
                                this.sysInfo);  //DR38000256 sysInfo needed to access sevconProductDescriptions
							this.monStore.myMonNodes.Add(mnNode);
							#endregion create CANnode in monitoring store
						}
					}
				}
			}
			#endregion add each node in the dragged nodes upwards parent path ONLY if it is not already in the custom list
			this.statusBarPanel3.Text = "Adding child nodes to " + cListStr;
			#region Now Add the dragged node and ALL its child nodes to custom list
			//add only childnodes that are not duplicates
			//- need to check every child node under the dragged node
			addNonDuplicateChildNodes(draggedNode , nextCustListNode, cListTblIndex);
			#endregion Now Add the dragged node and ALL its child nodes to custom list
			this.statusBarPanel3.Text = "Adding rows to table";
			#region Finally add all the corresponding OD rows to the Custom table;
			DataRow row = DWdataset.Tables[cListTblIndex].NewRow();
			//create a new store
			childTreeNodeList = new ArrayList();
			if(drgdTNtag.assocSub != null)
			{ 
				childTreeNodeList.Add(drgdTNtag);  //add dragged node if it has an underlying OD item
			}
			else
			{
				//fill it with all cheild nodes that have a corresponding OD item
				this.getChildODItems(nextCustListNode, false);  //use false here because we need to add items before we start the recursive search
			}
			this.progressBar1.Value = this.progressBar1.Minimum;
			this.progressBar1.Maximum = childTreeNodeList.Count;
			for(int i= 0;i<childTreeNodeList.Count;i++)
			{
				#region update progress bar
				if(this.progressBar1.Value<this.progressBar1.Maximum)
				{
					progressBar1.Value++;
				}
				#endregion update progress bar
				treeNodeTag sourceTag = (treeNodeTag) childTreeNodeList[i];
				treeNodeTag custTNtag = new treeNodeTag(sourceTag.tagType, sourceTag.nodeID, cListTblIndex, sourceTag.assocSub);
				if(custTNtag.assocSub != null)  //only add actual OD items to the table
				{
					if(dragDrop == true) //whole object including subs needs to be dropped here
					{
						#region drag Drop addition
						ObjDictItem scrItem = this.sysInfo.nodes[drgdTNtag.tableindex].getODItemAndSubs(custTNtag.assocSub.indexNumber);
						if(targetNode == this.GraphCustList_TN)
						{
							#region add OD item to Monitoring store
							#region add od item to correct monitor store dictionary
							nodeInfo affectedMonNode = null;
							foreach(nodeInfo mnNode in this.monStore.myMonNodes)
							{
								if(mnNode.nodeID == custTNtag.nodeID)
								{ //add to correct pseudo CNAnode in monitoring store
									if(mnNode.objectDictionary == null)
									{
										mnNode.objectDictionary = new ArrayList();
									}
                                    
									MAIN_WINDOW.appendErrorInfo = false;
									mnNode.addODItemToDictionary(scrItem);
									affectedMonNode = mnNode;
									break;
								}
							}
							#endregion add item to correct monitor store dictionary
							foreach(DataRowView DRV in MAIN_WINDOW.DWdataset.Tables[drgdTNtag.tableindex].DefaultView)
							{
								ODItemData subToAdd = (ODItemData) DRV.Row[(int) TblCols.odSub];
								//do NOT use LoadDataRow here - it does not laod the enurations correctly
								#region primary keys to search for
								object[] myKeys = new object[3];
								myKeys[0] = DRV.Row[(int)TblCols.Index].ToString();
								myKeys[1] =DRV.Row[(int)TblCols.sub].ToString();
								myKeys[2] =DRV.Row[(int)TblCols.NodeID].ToString();
								#endregion primary keys to search for
								DataRow monRow = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].Rows.Find(myKeys);
								if(monRow == null) //row with same primary key does not exist in Monitoring store
								{
									monRow = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].NewRow();
									bool rowAddInhibit = this.fillTableRowColumns(subToAdd,monRow,MAIN_WINDOW.GraphTblIndex,true,true, affectedMonNode);
									ODItemData checkSub = (ODItemData)monRow[(int) TblCols.odSub];
									if(checkSub.displayType == CANopenDataType.DOMAIN)
									{
										rowAddInhibit = true; //don't monitor DOMAINS
									}
									if(rowAddInhibit == false)  //probably superfluous here 
									{
										MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].Rows.Add(monRow);
									}
								}
								#region Add Row to custim Tbale and THEN mark source row as monitored
								//add row to custom list BEFROE we markit a s being monitored - don't want it highlighted in custom list
								//just in source list
								#region mark source row as being monitored to ensure correct colouring
								if(subToAdd.subNumber != -1)  //not a header row
								{
									DataRowState currentRowState = DRV.Row.RowState;
									DRV.Row[(int) TblCols.Monitor] = true;
									if(currentRowState == DataRowState.Unchanged)
									{
										DRV.Row.AcceptChanges();
									}
								}
								#endregion mark source row as being monitored to ensure correct colouring
								#endregion Add Row to custim Tbale and THEN mark source row as monitored
							}
							#endregion Monitoring store
						}
						else if(targetNode == this.DCFCustList_TN)
						{
							#region add odItem to DCF node
							this.sysInfo.DCFnode.addODItemToDictionary(scrItem);
							foreach(DataRowView DRV in MAIN_WINDOW.DWdataset.Tables[drgdTNtag.tableindex].DefaultView)
							{
								if(((ODItemData) DRV.Row[(int) TblCols.odSub]).indexNumber == custTNtag.assocSub.indexNumber)
								{
									//need primary key here - this seems long winded but LoadDataRow DOES NOT add the enumerations
									#region primary keys to search for
									object[] myKeys = new object[2];
									myKeys[0] = DRV.Row[(int)TblCols.Index].ToString();
									myKeys[1] =DRV.Row[(int)TblCols.sub].ToString();
									#endregion primary keys to search for
									DataRow dcfRow = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].Rows.Find(myKeys);
									if(dcfRow == null) //row with same primary key does not exist in DCF
									{
										dcfRow = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].NewRow();
										ODItemData subToAdd = (ODItemData) DRV.Row[(int) TblCols.odSub];
										bool rowAddInhibit = false;
										if(subToAdd.displayType == CANopenDataType.DOMAIN)
										{
											StringBuilder infoStr = new StringBuilder();
											infoStr.Append(subToAdd.parameterName);
											infoStr.Append(" (0x");
											infoStr.Append(subToAdd.indexNumber.ToString("X").PadLeft(4, '0'));
											infoStr.Append(" subIndex 0x");
											infoStr.Append(subToAdd.subNumber.ToString("X"));
											infoStr.Append(") is a CANopen Domain. \nDomains may contain large amounts to of data and take significant time to upload. \nDo you want to add this item to the DCF store?");
											DialogResult result = Message.Show(this,infoStr.ToString(),
												"Read Domain item confirmation", 
												MessageBoxButtons.YesNo,
												MessageBoxIcon.Question, 
												MessageBoxDefaultButton.Button1);
											if(result == DialogResult.No)
											{
												rowAddInhibit = true;
											}
										}
										if(rowAddInhibit == false)
										{
											rowAddInhibit = this.fillTableRowColumns(subToAdd,dcfRow,MAIN_WINDOW.DCFTblIndex,true,true, sysInfo.DCFnode);
										}
										if(rowAddInhibit == false) 
										{
											MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].Rows.Add(dcfRow);
										}
									}
								}
							}
							#endregion add odItem to DCF node
						}
						#endregion drag Drop addition
					}
					else
					{  
						#region data grid context menu addition
						DataView tmpView = (DataView) this.dataGrid1.DataSource; //dig out the underlying DataView
						DataRow TblRow = tmpView[this.selectedDgRowInd].Row;  //independent of datagrid columns being displayed
						#region get the Index /Sub index primary key to search for
						object[] myKeys = new object[2];
						myKeys[0] =TblRow[(int) TblCols.Index].ToString();
						myKeys[1] = TblRow[(int) TblCols.sub].ToString();
						#endregion get the Index /Sub index primary key to search for

						DataRow SourceRow = MAIN_WINDOW.DWdataset.Tables[drgdTNtag.tableindex].Rows.Find(myKeys); 
						ODItemData odSubtoAdd = (ODItemData) SourceRow[(int) TblCols.odSub];
						myKeys[1] = "";
						DataRow headerRow = MAIN_WINDOW.DWdataset.Tables[drgdTNtag.tableindex].Rows.Find(myKeys); 
						MAIN_WINDOW.appendErrorInfo = false; //=> single subs have no header row so swithc of error string handling
						ODItemData headerSub = this.sysInfo.nodes[drgdTNtag.tableindex].getODSub(custTNtag.assocSub.indexNumber,-1);
						MAIN_WINDOW.appendErrorInfo = true;
						ODItemData numItemsSub = null;
						if(headerSub != null)
						{
							numItemsSub = this.sysInfo.nodes[drgdTNtag.tableindex].getODSub(custTNtag.assocSub.indexNumber,0);
							#region if this is not a genuine noddy number of items sub then reset it back to null 
							if( 
								((numItemsSub.accessType == ObjectAccessType.ReadOnly)
								|| (numItemsSub.accessType == ObjectAccessType.Constant)) 
								&& (numItemsSub.subNumber == 0)
								&& ((CANopenDataType) numItemsSub.dataType == CANopenDataType.UNSIGNED8)
								&& (headerSub.subNumber == -1))
							{
							}
							else
							{
								numItemsSub = null;
							}
							#endregion if this is not a genuine noddy number of items sub then reset it back to null 
						}
						if(SourceRow != null)
						{
							if(headerRow != null)
							{
								#region header and number of items 
								if(targetNode == this.GraphCustList_TN)
								{
									#region get primary key for graph table
									object[] myGrKeys = new object[3];
									myGrKeys[0] =TblRow[(int) TblCols.Index].ToString();
									myGrKeys[1] = "";//TblRow[(int) TblCols.sub].ToString();
									myGrKeys[2] = TblRow[(int) TblCols.NodeID].ToString();
									DataRow headGrRow = MAIN_WINDOW.DWdataset.Tables[cListTblIndex].Rows.Find(myGrKeys);
									#endregion get primary key for graph table
									#region extend the monitring CAN node's  dictionary as required
									foreach(nodeInfo mnNode in this.monStore.myMonNodes)
									{
										if(mnNode.nodeID.ToString() == TblRow[(int) TblCols.NodeID].ToString())
										{//sub is -1 because this is the header row
											mnNode.addSubToDictionary(headerSub);
											//now grab a pointer to the odSub in the monitoring dictionary - not the source one
											ODItemData grHeaderSub = mnNode.getODSub(headerSub.indexNumber, headerSub.subNumber);
											#region add header row to Graph table if it is not already in
											if(headGrRow == null)
											{
												headGrRow = MAIN_WINDOW.DWdataset.Tables[cListTblIndex].NewRow();
												bool rowAddInhibit = this.fillTableRowColumns(grHeaderSub,headGrRow,cListTblIndex,true,true, mnNode);
												if(rowAddInhibit == false)
												{
													MAIN_WINDOW.DWdataset.Tables[cListTblIndex].Rows.Add(headGrRow);
												}
											}
											#endregion add header row to Graph table if it is not already in
											if(numItemsSub != null)
											{
												mnNode.addSubToDictionary(numItemsSub);
												//don't add to table
											}
											break;
										}
									}
									#endregion extend the monitring CAN node's  dictionary as required
								}
								else if(targetNode == this.DCFCustList_TN)
								{
									#region get primary key for DCF table
									object[] myDCFKeys = new object[2];
									myDCFKeys[0] =TblRow[(int) TblCols.Index].ToString();
									myDCFKeys[1] = "";//TblRow[(int) TblCols.sub].ToString();
									#endregion get primary key for DCF table
									this.sysInfo.DCFnode.addSubToDictionary(headerSub);
									#region add header row to DCf tabl eif it is not already in
									DataRow headDCFRow = MAIN_WINDOW.DWdataset.Tables[cListTblIndex].Rows.Find(myDCFKeys);
									if(headDCFRow == null)
									{
										headDCFRow = MAIN_WINDOW.DWdataset.Tables[cListTblIndex].NewRow();
										bool rowAddInhibit = this.fillTableRowColumns(headerSub,headDCFRow,cListTblIndex,true,true, this.sysInfo.DCFnode);
										if(rowAddInhibit == false)
										{
											MAIN_WINDOW.DWdataset.Tables[cListTblIndex].Rows.Add(headDCFRow);
										}
									}
									#endregion add header row to DCf tabl eif it is not already in
									if(numItemsSub != null)
									{
										this.sysInfo.DCFnode.addSubToDictionary(numItemsSub);
										//do not add to table
									}
								}
								#endregion header and number of items 
							}
							if(targetNode == this.GraphCustList_TN)
							{
								#region extend the monitring CAN node's  dictionary as required
								foreach(nodeInfo mnNode in this.monStore.myMonNodes)
								{
									if(mnNode.nodeID == custTNtag.assocSub.CANnode.nodeID)
									{
                                        //Copy CANnode over if not already been done (prevent exceptions)
                                        if (odSubtoAdd.CANnode == null)
                                        {
                                            odSubtoAdd.CANnode = custTNtag.assocSub.CANnode;
                                        }
										mnNode.addSubToDictionary(odSubtoAdd); 
										#region get primary key for graph table
										object[] myGrKeys = new object[3];
										myGrKeys[0] =TblRow[(int) TblCols.Index].ToString();
										myGrKeys[1] = TblRow[(int) TblCols.sub].ToString();
										myGrKeys[2] = TblRow[(int) TblCols.NodeID].ToString();
										#endregion get primary key for graph table
										DataRow custRow = MAIN_WINDOW.DWdataset.Tables[cListTblIndex].Rows.Find(myGrKeys);
										if(custRow == null)
										{
											custRow = MAIN_WINDOW.DWdataset.Tables[cListTblIndex].NewRow();
											ODItemData graphSub = mnNode.getODSub(odSubtoAdd.indexNumber, odSubtoAdd.subNumber);
											bool rowAddInhibit = this.fillTableRowColumns(graphSub,custRow,cListTblIndex,true,true,mnNode);
											if(graphSub.displayType == CANopenDataType.DOMAIN)
											{
												rowAddInhibit = true; //don't monitor DOMAINS
											}
											if(rowAddInhibit == false)
											{
												MAIN_WINDOW.DWdataset.Tables[cListTblIndex].Rows.Add(custRow);
											}
										}
										break;
									}
								}
								#endregion extend the monitring CAN node's  dictionary as required
							}
							else if(targetNode == this.DCFCustList_TN)
							{
								this.sysInfo.DCFnode.addSubToDictionary(odSubtoAdd); 
								ODItemData dcfSub = this.sysInfo.DCFnode.getODSub(odSubtoAdd.indexNumber, odSubtoAdd.subNumber);
								#region get primary key for DCF table
								object[] myDCFKeys = new object[2];
								myDCFKeys[0] =TblRow[(int) TblCols.Index].ToString();
								myDCFKeys[1] =TblRow[(int) TblCols.sub].ToString();
								#endregion get primary key for DCF table
								DataRow custRow = MAIN_WINDOW.DWdataset.Tables[cListTblIndex].Rows.Find(myDCFKeys);
								if(custRow == null)
								{
									custRow = MAIN_WINDOW.DWdataset.Tables[cListTblIndex].NewRow();
									bool rowAddInhibit = this.fillTableRowColumns(dcfSub,custRow,cListTblIndex,true,true, this.sysInfo.DCFnode);
									if(rowAddInhibit == false)
									{
										MAIN_WINDOW.DWdataset.Tables[cListTblIndex].Rows.Add(custRow);
									}
								}
							}
							if(cListTblIndex == MAIN_WINDOW.GraphTblIndex)
							{
								DataRowState currentRowState  = SourceRow.RowState;
								SourceRow.Table.ColumnChanged -= new System.Data.DataColumnChangeEventHandler(this.table_ColumnChanged);
								SourceRow[(int) TblCols.Monitor] = true; //force correct display in source datagrid
								if(currentRowState == DataRowState.Unchanged)
								{
									SourceRow.AcceptChanges();
								}
								SourceRow.Table.ColumnChanged += new System.Data.DataColumnChangeEventHandler(this.table_ColumnChanged);
							}
						}
						#endregion data grid context menu addition
					}
				}
			}
			int moncount = 0;
			if(targetNode == this.GraphCustList_TN) 
			{
				#region monitoring
				foreach(DataRow gRow in MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].Rows)
				{
					ODItemData odSub= (ODItemData) gRow[(int) TblCols.odSub];
					if( odSub.subNumber != -1)  //not header
					{
						moncount++;
						if(moncount > MAIN_WINDOW.monitorCeiling)
						{
							this.statusBarOverrideText = "Only the first " + monitorCeiling.ToString() + " OD sub-items in the Monitoring store will be monitored in real time";
							break;
						}
					}
				}
				#endregion monitoring 
			}
			#endregion Finally add all the corresponding OD rows to the Custom table;
			this.statusBarPanel3.Text = "Getting item values from source device";
			//remove any nodes from Custom list that represent new screens
			do
			{
				nodesRemovedFlag = false;  //set false before entry
				this.removeSpecialDevcieLevelNodesFromCustomLists(this.DCFCustList_TN); 
			}
			while (nodesRemovedFlag == true);

			#region and finally (again) ensure that each item in the custom list has an actual value
			this.progressBar1.Value = this.progressBar1.Minimum;
            nodeValue = progressBar1.Minimum;
			//declare a tHrea dinstance so we can name it dependent on which custom list we are targetting
			this.dataRetrievalThread = new Thread(new ThreadStart( ensureAllItemsInCustListHaveActValue )); 
			if(cListTblIndex == MAIN_WINDOW.DCFTblIndex)
			{
                this.progressBar1.Maximum = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].DefaultView.Count;
				dataRetrievalThread.Name = "DCF Data Retrieval";
				this.tempTreeNode = this.DCFCustList_TN;
				tempDestTableIndex = MAIN_WINDOW.DCFTblIndex;
				this.fillDCFLabels();
			}
			else if(cListTblIndex == MAIN_WINDOW.GraphTblIndex)
			{
                this.progressBar1.Maximum = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].DefaultView.Count;
				dataRetrievalThread.Name = "Monitoring List Data Retrieval";
				this.tempTreeNode = this.GraphCustList_TN;
				tempDestTableIndex = MAIN_WINDOW.GraphTblIndex;
			}
			this.tempTableIndex = drgdTNtag.tableindex;
			this.progressBar1.Maximum = this.countTreeNodesInLeg(this.tempTreeNode, true) + 1;  //to include the root node itself
			#region start data section retrieval from device thread
			this.statusBarPanel3.Text = "Retrieving values from node";
			dataRetrievalThread.IsBackground = true;
            dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
			System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif  

			dataRetrievalThread.Start(); 
			this.timer2.Enabled = true;
			#endregion start data section retrieval from device thread

			#endregion and finally (again) ensure that each item in the custom list has an actual value
		}

		private void addNonDuplicateChildNodes(TreeNode sourceNode, TreeNode targetNode, int cListTblIndex)
		{
			bool duplicateFound = false;
			TreeNode nextTargetNode = null;
			foreach(TreeNode trgtChildNode in targetNode.Nodes)  //check each child node in the source
			{
				if(trgtChildNode.Text == sourceNode.Text)
				{
					//duplicate 
					duplicateFound = true;
					nextTargetNode = trgtChildNode;
					break;
				}
			}
			if(duplicateFound == false)
			{
				//take a shallow copy of the srcChildNode - ie without its chld nodes
				treeNodeTag  sourceNodeTag = (treeNodeTag) sourceNode.Tag;
				TreeNode addNode = new TreeNode();
				addNode = (TreeNode) sourceNode.Clone();
				addNode.Tag =  new treeNodeTag(sourceNodeTag.tagType, sourceNodeTag.nodeID, cListTblIndex, sourceNodeTag.assocSub);
				changeTableIndexInchildNodes(addNode,cListTblIndex);
				targetNode.Nodes.Add(addNode);
			}
			if((sourceNode.Nodes.Count>0) && (nextTargetNode != null))
			{
				//At this point we must have a comparable node in the destination treeleg
				foreach(TreeNode newSrcNode in sourceNode.Nodes)
				{
					addNonDuplicateChildNodes(newSrcNode ,nextTargetNode, cListTblIndex);
				}
			}
		}
	
		private void changeTableIndexInchildNodes(TreeNode topNode, int tableIndex)
		{
			foreach(TreeNode node in topNode.Nodes)
			{
				treeNodeTag nodeTag = (treeNodeTag) node.Tag;
				treeNodeTag tempTag = new treeNodeTag(nodeTag.tagType, nodeTag.nodeID, tableIndex,  nodeTag.assocSub);
				//				treeNodeTag tempTag = (treeNodeTag) node.Tag;
				//				tempTag.tableindex = tableIndex;
				//now copy back to node
				node.Tag = tempTag;
				if(node.Nodes.Count>0)
				{
					changeTableIndexInchildNodes(node, tableIndex);
				}
			}
		}
		private int countTreeNodesInLeg(TreeNode topNode, bool firstTime)
		{
			if(firstTime == true)
			{
				treeNodeCountInLeg = 0;
			}
			foreach(TreeNode node in topNode.Nodes)
			{
				treeNodeCountInLeg++;
				if(node.Nodes.Count>0)
				{
					countTreeNodesInLeg(node, false);
				}
			}
			return treeNodeCountInLeg;
		}

        private void treeView1_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			// Retrieve the client coordinates of the mouse position relative to the sender Control
			Point MousePt = treeView1.PointToClient(new Point(e.X, e.Y));
			#region remove old drag string text
			treeView1.Invalidate(removeDragStringRectTV);  //remove old text
			treeView1.Update();  //necessary becasue we are changing it within an Event handler
			//calculate new drag area
			Rectangle newDragStringRectTV 
				= new Rectangle(MousePt.X, MousePt.Y, dragStrSize.Width, dragStrSize.Height);
			gTV.DrawString(shadowText, this.mainWindowFont, Brushes.Black, newDragStringRectTV);
			//now set the removal area ready for next drag string Draw
			removeDragStringRectTV = new Rectangle(newDragStringRectTV.Location, dragStrSize);
			e.Effect = DragDropEffects.None; //default => mouse cursor shows that item cannot be dropped here.
			#endregion remove old drag string text
			TreeNode targetNode = treeView1.GetNodeAt(MousePt);
			TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
			treeNodeTag targetNodeTag, drgdTNtag ;
			if(targetNode != null)
			{
				GraphCustList_TN.BackColor = this.treeView1.BackColor;
				DCFCustList_TN.BackColor = this.treeView1.BackColor;
				targetNodeTag  = (treeNodeTag) targetNode.Tag;
				drgdTNtag = (treeNodeTag) draggedNode.Tag;
				if(targetNode == GraphCustList_TN)
				{
					#region Graphing Custom List Root Node
					//int indexNo = System.Convert.ToInt32(drgdTNtag.ODIndexNo,16);
					if(drgdTNtag.assocSub != null)
					{
						string droppedNodeString = GraphCustList_TN.Text + this.treeView1.PathSeparator + draggedNode.FullPath;
						if(GraphCustList_TN.Nodes.Count>0)
						{
							#region check if this node is already in the monitoring list
							//judetemp							if (nodeAlreadyInTargetBucket(GraphCustList_TN, droppedNodeString,draggedNode) == false)
						{
							GraphCustList_TN.BackColor = SCCorpStyle.SCHighlightColour;
							e.Effect = DragDropEffects.Copy;  //mouse cursor shows tha tItem can be dropped here
						}
							//							else
							//							{
							//								this.statusBarPanel3.Text = "Monitoring List already contains this Item";
							//							}
							#endregion check if this node is already in the monitoring list
						}
						else
						{
							GraphCustList_TN.BackColor = SCCorpStyle.SCHighlightColour;
							e.Effect = DragDropEffects.Copy;  //mouse cursor shows tha tItem can be dropped here
						}
					}
					else
					{
						this.statusBarPanel3.Text = "Only individial OD items and/or OD sub indexes can be dragged to the Monitoring List";
					}
					#endregion Graphing Custom List Root Node
				}
				else if (targetNode == this.DCFCustList_TN)  //can only drop at top levle now - DW sorts out positioning
				{
					#region DCF Custom List Root Node
					if(DCFCustList_TN.Nodes.Count > 0)
					{  //check that these nodes are from same node Id
						treeNodeTag alreadyInDCFNodeTag = (treeNodeTag) DCFCustList_TN.Nodes[0].Tag;
						if((this.sysInfo.DCFnode.nodeID != drgdTNtag.nodeID)
							|| (sysInfo.DCFnode.vendorID != sysInfo.nodes[drgdTNtag.tableindex].vendorID)
							|| (sysInfo.DCFnode.productCode != sysInfo.nodes[drgdTNtag.tableindex].productCode)
							|| (sysInfo.DCFnode.revisionNumber != sysInfo.nodes[drgdTNtag.tableindex].revisionNumber))
						{
							this.statusBarPanel3.Text = "DCF data must be from single source";
						}
						else
						{
							string droppedNodeString = DCFCustList_TN.Text + this.treeView1.PathSeparator + draggedNode.FullPath;
							//judetemp							if (nodeAlreadyInTargetBucket(DCFCustList_TN, droppedNodeString, draggedNode) == false)
						{
							DCFCustList_TN.BackColor = SCCorpStyle.SCHighlightColour;
							e.Effect = DragDropEffects.Copy;  //mouse cursor shows tha tItem can be dropped here
						}
							//							else
							//							{
							//								this.statusBarPanel3.Text = "DCF store already contains this node";
							//							}
						}
					}
					else
					{
						if (this.sysInfo.nodes[drgdTNtag.tableindex].isSevconBootloader() == true)
						{
							this.statusBarPanel3.Text = "Sevcon nodes running Bootloader software cannot be used as a DCF source";
						}
						else if (this.sysInfo.nodes[drgdTNtag.tableindex].isSevconSelfChar() == true)
						{
							this.statusBarPanel3.Text = "Sevcon nodes running self characterization software cannot be used as a DCF source";
						}
						else if((this.sysInfo.nodes[drgdTNtag.tableindex].manufacturer == Manufacturer.SEVCON)
							|| (this.sysInfo.nodes[drgdTNtag.tableindex].manufacturer == Manufacturer.THIRD_PARTY))
						{
							e.Effect = DragDropEffects.Copy;  //mouse cursor shows tha tItem can be dropped here
							DCFCustList_TN.BackColor = SCCorpStyle.SCHighlightColour;
						}
						else
						{
							this.statusBarPanel3.Text = "Nodes without an EDS file cannot be used as a DCF source";
						}
					}
					#endregion DCF Custom List Root Node
				}
			}
			long m_Ticks = 0;
			TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - m_Ticks);
			#region scroll up
			if (MousePt.Y < treeView1.ItemHeight) 
			{
				// if within one node of top, scroll quickly
				if (targetNode.PrevVisibleNode!= null) 
				{
					targetNode = targetNode.PrevVisibleNode;
				}
				targetNode.EnsureVisible();
				m_Ticks = DateTime.Now.Ticks;
			} 
			else if (MousePt.Y < (treeView1.ItemHeight * 2)) 
			{
				// if within two nodes of the top, scroll slowly
				if (ts.TotalMilliseconds > 250)
				{
					targetNode = targetNode.PrevVisibleNode;
					if (targetNode.PrevVisibleNode != null) 
					{
						targetNode = targetNode.PrevVisibleNode;
					}
					targetNode.EnsureVisible();
					m_Ticks = DateTime.Now.Ticks;
				}
			}
			#endregion scroll up

			#region scroll down
			//scroll down
			if (MousePt.Y > treeView1.ItemHeight) 
			{
				// if within one node of top, scroll quickly
				if (targetNode.NextVisibleNode!= null) 
				{
					targetNode = targetNode.NextVisibleNode;
				}
				targetNode.EnsureVisible();
				m_Ticks = DateTime.Now.Ticks;
			} 
			else if (MousePt.Y > (treeView1.ItemHeight * 2)) 
			{
				// if within two nodes of the top, scroll slowly
				if (ts.TotalMilliseconds > 250)
				{
					targetNode = targetNode.NextVisibleNode;
					if (targetNode.NextVisibleNode != null) 
					{
						targetNode = targetNode.NextVisibleNode;
					}
					targetNode.EnsureVisible();
					m_Ticks = DateTime.Now.Ticks;
				}
			}
			#endregion scroll down
		}
		private void treeView1_DragLeave(object sender, System.EventArgs e)
		{
			this.DCFCustList_TN.BackColor = this.treeView1.BackColor;  //back to default
			this.GraphCustList_TN.BackColor = this.treeView1.BackColor;  //back to default
			this.statusBarPanel3.Text = "";  //reset any use guidance
		}
		private TreeNode getWouldBeParentTreeNode( TreeNode rootNode, string targetPath)
		{
			foreach ( TreeNode node in rootNode.Nodes)
			{
				if (node.FullPath == targetPath)
				{
					return node.Parent;
				}
				if(node.Nodes.Count>0)
				{
					TreeNode parentNode = getWouldBeParentTreeNode(node, targetPath);
					if(parentNode != null)
					{
						return parentNode;
					}
				}
			}
			return null;
		}
		private TreeNode getAssocODItemTreeNode(TreeNode rootNode, int Index)
		{
			#region check the passed treenode first
			treeNodeTag rootnodetag  = (treeNodeTag) rootNode.Tag;
			if((rootnodetag.assocSub != null) && (rootnodetag.assocSub.indexNumber == Index))
			{
				return rootNode;
			}
			#endregion check the passed treenode first
			foreach ( TreeNode node in rootNode.Nodes)
			{
				treeNodeTag nodetag = (treeNodeTag) node.Tag;
				if((nodetag.assocSub != null) && (nodetag.assocSub.indexNumber == Index))// && (nodetag.subIndex == subindex))
				{
					return node;
				}
				if(node.Nodes.Count>0)
				{
					TreeNode clickedNode = getAssocODItemTreeNode(node, Index);
					if(clickedNode != null)
					{
						return clickedNode;
					}

				}
			}
			return null; //no mathcing node found
		}

        /// <summary>
        /// Searches treeview1 nodes recursively until it finds the required instance of the search
        /// string, only searching the passed node in the selected search areas.
        /// </summary>
        /// <param name="rootNode">Node to be searched.</param>
        /// <param name="searchType">Parameters to be searched.</param>
        /// <param name="searchString">String to be found.</param>
        /// <param name="searchIndex">If a search on ALL or INDEX, the OD index to be found.</param>
        /// <param name="searchInstance">Which match has to be returned (to allow successive calls
        /// to find the first, second, third match etc.</param>
        /// <param name="foundInstance">Ref value updated with number of instances found, needed
        /// if the required searchInstance isn't found to allow continuation of the search in 
        /// further nodes.</param>
        /// <returns>The node for which the required instance of searchString is found, null if not found.</returns>
        private TreeNode searchInTreeNode(TreeNode rootNode, SearchArea searchType, string searchString, int searchIndex, int searchInstance, ref int foundInstance)
        {
            #region check the passed treenode first
            treeNodeTag rootnodetag = (treeNodeTag)rootNode.Tag;
            bool parameterMatch = false;
            bool indexMatch = false;
            bool objectMatch = false;
            bool sectionMatch = false;
            bool xmlMatch = false;

            xmlMatch = rootNode.Text.ToUpper().Contains(searchString.ToUpper());

            #region only check parameter, index, object and section names if there's an associated sub
            if (rootnodetag.assocSub != null)
            {
                #region check parameter names for a match (including all sub indices if relevant)
                if 
                (
                    (rootnodetag.tagType == TNTagType.ODITEM) 
                  &&((searchType == SearchArea.ALL) || (searchType == SearchArea.PARAMETER_NAME))
                )
                {
                    int tableIndex = -1;
                    DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;

                    // check OD item's header for parameter name match first
                    parameterMatch = rootnodetag.assocSub.parameterName.ToUpper().Contains(searchString.ToUpper());

                    #region only check subs if a match hasn't already been found in the header sub
                    if (parameterMatch == false)
                    {
                        // if dcf or monitoring table indices, convert to real node index before searching the objDict
                        if (rootnodetag.tableindex > (this.sysInfo.nodes.Length - 1))
                        {
                            fbc = this.sysInfo.getNodeNumber(rootnodetag.nodeID, out tableIndex);
                        }
                        else // equates to a physical node
                        {
                            tableIndex = rootnodetag.tableindex;
                            fbc = DIFeedbackCode.DISuccess;
                                }

                        #region if translated into a real node in sysInfo, search the subs for this item
                        if (fbc == DIFeedbackCode.DISuccess)
                        {
                            ObjDictItem obj = sysInfo.nodes[tableIndex].getODItemAndSubs(rootnodetag.assocSub.indexNumber);

                            // only check each sub's parameter name for a match if subs exist
                            // NOTE: odItemSubs[0] is the same as assocSub above (ie header sub) so only search if
                            // more than one sub associated.
                            if ((obj != null) && (obj.odItemSubs.Count > 1))
                            {
                                for (int subIndex = 0; subIndex < obj.odItemSubs.Count; subIndex++)
                                {
                                    ODItemData sub = (ODItemData)obj.odItemSubs[subIndex];

                                    if (sub != null)
                                    {
                                        parameterMatch = sub.parameterName.ToUpper().Contains(searchString.ToUpper());

                                        // first match is good enough for any index/sub since all are displayed in the table together
                                        if (parameterMatch == true)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion if translated into a real node in sysInfo, search the subs for this item
                    }
                    #endregion only check subs if a match hasn't already been found in the header sub
                }
                #endregion check parameter names for a match (including all sub indices if relevant)

                indexMatch = ((searchIndex != 0) && (rootnodetag.assocSub.indexNumber == searchIndex));
                sectionMatch = 
                      ( 
                         (rootnodetag.assocSub.sectionType < sysInfo.sevconSectionIDList.Count)
                      && (sysInfo.sevconSectionIDList[rootnodetag.assocSub.sectionType].ToString().ToUpper().Contains(searchString.ToUpper()) == true) 
                      );

                objectMatch =
                    (
                        (rootnodetag.assocSub.objectName < sysInfo.sevconObjectIDList.Count)
                     && (sysInfo.sevconObjectIDList[rootnodetag.assocSub.objectName].ToString().ToUpper().Contains(searchString.ToUpper()) == true)
                    );
            }
            #endregion only check parameter, index, object and section names if there's an associated sub

            #region if a specified match is found in the root node, return the relevant node
            // A root node search only works for first search instance because after that,
            // the chances are that root node is sitting at a match from the last search anyway.
            if
            (
                (searchInstance == 1) 
             && ( 
                    ((searchType == SearchArea.ALL) && (xmlMatch || parameterMatch || indexMatch || objectMatch || sectionMatch))
                 || ((searchType == SearchArea.INDEX) && indexMatch)
                 || ((searchType == SearchArea.PARAMETER_NAME) && parameterMatch)
                 || ((searchType == SearchArea.SEVCON_OBJECT) && objectMatch)
                 || ((searchType == SearchArea.SEVCON_SECTION) && sectionMatch)
                 || ((searchType == SearchArea.XML_SECTION) && xmlMatch)
                )
            )
            {
                foundInstance++;

                if (foundInstance == searchInstance)
                {
                    return rootNode;
                }
            }
            #endregion if a specified match is found in the root node, return the relevant node
            #endregion check the passed treenode first

            #region Recursively search for the specified match in this node
            foreach (TreeNode node in rootNode.Nodes)
            {
                treeNodeTag nodetag = (treeNodeTag)node.Tag;
                parameterMatch = false;
                indexMatch = false;
                objectMatch = false;
                sectionMatch = false;

                xmlMatch = node.Text.ToUpper().Contains(searchString.ToUpper());

                #region only check parameter, index, object and section names if there's an associated sub
                if (nodetag.assocSub != null)
                {
                    #region check parameter names for a match (including all sub indices if relevant)
                    if
                    (
                        (nodetag.tagType == TNTagType.ODITEM)
                      && ((searchType == SearchArea.ALL) || (searchType == SearchArea.PARAMETER_NAME))
                    )
                    {
                        int tableIndex = -1;
                        DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;

                        // check OD item's header for parameter name match first
                        parameterMatch = nodetag.assocSub.parameterName.ToUpper().Contains(searchString.ToUpper());

                        #region only check subs if a match hasn't already been found
                        if (parameterMatch == false)
                        {
                            // if dcf or monitoring table indices, convert to real node index to search the objDict
                            if (nodetag.tableindex > (this.sysInfo.nodes.Length-1))
                            {
                                fbc = this.sysInfo.getNodeNumber(nodetag.nodeID, out tableIndex);
                            }
                            else // equates to a physical node already
                            {
                                tableIndex = nodetag.tableindex;
                                fbc = DIFeedbackCode.DISuccess;
                            }

                            #region if translated into a real node in sysInfo, search the subs for this item
                            if (fbc == DIFeedbackCode.DISuccess)
                            {
                                ObjDictItem obj = sysInfo.nodes[tableIndex].getODItemAndSubs(nodetag.assocSub.indexNumber);

                                // only check each sub's parameter name for a match if subs exist
                                if ((obj != null) && (obj.odItemSubs.Count > 1))
                                {
                                    for (int subIndex = 0; subIndex < obj.odItemSubs.Count; subIndex++)
                                    {
                                        ODItemData sub = (ODItemData)obj.odItemSubs[subIndex];

                                        if (sub != null)
                                        {
                                            parameterMatch = sub.parameterName.ToUpper().Contains(searchString.ToUpper());

                                            // first match good enough for any index/sub since all are displayed in the table together
                                            if (parameterMatch == true)
                                            {
                                                break; 
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion if translated into a real node in sysInfo, search the subs for this item
                        }
                        #endregion only check subs if a match hasn't already been found
                    }
                    #endregion check parameter names for a match (including all sub indices if relevant)

                    indexMatch = ((searchIndex != 0) && (nodetag.assocSub.indexNumber == searchIndex));
                    sectionMatch =
                          (
                             (nodetag.assocSub.sectionType < sysInfo.sevconSectionIDList.Count)
                          && (sysInfo.sevconSectionIDList[nodetag.assocSub.sectionType].ToString().ToUpper().Contains(searchString.ToUpper()) == true)
                          );

                    objectMatch =
                        (
                            (nodetag.assocSub.objectName < sysInfo.sevconObjectIDList.Count)
                         && (sysInfo.sevconObjectIDList[nodetag.assocSub.objectName].ToString().ToUpper().Contains(searchString.ToUpper()) == true)
                      );
                }
                #endregion only check parameter, index, object and section names if there's an associated sub

                #region if the specified match is found, return the relevant node
                if
                (
                    ((searchType == SearchArea.ALL) && (xmlMatch || parameterMatch || indexMatch || objectMatch || sectionMatch))
                 || ((searchType == SearchArea.INDEX) && indexMatch)
                 || ((searchType == SearchArea.PARAMETER_NAME) && parameterMatch)
                 || ((searchType == SearchArea.SEVCON_OBJECT) && objectMatch)
                 || ((searchType == SearchArea.SEVCON_SECTION) && sectionMatch)
                 || ((searchType == SearchArea.XML_SECTION) && xmlMatch)
                )
                {
                    foundInstance++;

                    if (foundInstance == searchInstance)
                    {
                        return node;
                    }
                }
                #endregion if the specified match is found, return the relevant node

                #region if no match is found but there are more nodes to search, keep looking.
                if (node.Nodes.Count > 0)
                {
                    TreeNode clickedNode = searchInTreeNode(node, searchType, searchString, searchIndex, searchInstance, ref foundInstance);

                    if (clickedNode != null)
                    {
                        return clickedNode; //match found so return it
                    }
                    #endregion if no match is found but there are more nodes to search, keep looking.
                }
            }
            #endregion Recursively search for the specified match in this node

            return null; //required match instance not found anywhere in this node
        }
		
		private void EnsureVisibleWithoutRightScrolling(TreeNode node)
		{
			// we do the standard call.. 
			node.EnsureVisible();
    
			// ..and afterwards we scroll to the left again
			SendMessage(treeView1.Handle, WM_HSCROLL,SB_LEFT , 0);
		}
		#endregion TreeView Drag & Drop methods 

		#region Change OD Parameter values methods
		private void submitBtn_Click(object sender, System.EventArgs e)
		{
			TreeNode selNode = this.treeView1.SelectedNode;
			treeNodeTag selNodeTag = (treeNodeTag) this.treeView1.SelectedNode.Tag;
			hideUserControls();
			MAIN_WINDOW.DWdataset.Tables[selNodeTag.tableindex].ColumnChanged -= new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);

            if (dataRetrievalThread == null)
            {
                nodeTag = selNodeTag;
                dataRetrievalThread = new Thread(new ParameterizedThreadStart(submitToDevice));
                dataRetrievalThread.Name = "Submit Data Download and Retrieval";
                dataRetrievalThread.IsBackground = true;
                dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
                System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif
                dataRetrievalThread.Start(selNodeTag);
                this.timer2.Enabled = true;
            }
            else
            {
                // clear all user changes
                for (int i = 0; i < DWdataset.Tables[selNodeTag.tableindex].Rows.Count; i++)
                {
                    if (DWdataset.Tables[selNodeTag.tableindex].Rows[i].RowState == DataRowState.Modified)
                    {
                        DWdataset.Tables[selNodeTag.tableindex].Rows[i].RejectChanges();
                    }
                }

                // warn the user
			    if(SystemInfo.errorSB.Length>0)
			    {
                    SystemInfo.errorSB.Append("Data thread error: could not write new values to the device.\n");
				    sysInfo.displayErrorFeedbackToUser("Not all values written/read back OK to/from device.\n");
				    this.statusBarPanel3.Text = "Not all values written/read back OK";
			    }
			    else
			    {
				    this.statusBarOverrideText = "All values written and read back OK";
			    }
            }
		}
		private void table_ColumnChanged(object sender, System.Data.DataColumnChangeEventArgs e)
		{
			if((MAIN_WINDOW.currTblIndex>=this.sysInfo.nodes.Length)  //custom list - B&B
				||(e.Column.Ordinal != (int) TblCols.actValue))
			{
				return;
			}
			MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged -= new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
			
			string inputString = (e.ProposedValue.ToString()); 
			#region null entry - causes old entry to be re-displayed
			if(inputString == "")
			{
				e.Row.RejectChanges();  //reject new value
				if(e.Row.HasErrors)
				{
					e.Row.ClearErrors(); //clear the error
				}
				e.ProposedValue = e.Row[(int) (TblCols.actValue)].ToString();  //bung in old value
				e.Row.AcceptChanges(); //and mark row as unmodified
				MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
				return;
			}
			#endregion
			ODItemData odSub = (ODItemData) e.Row[(int)(TblCols.odSub)];
			#region read only items check
			if ((  (odSub.subNumber == -1) 
				|| (odSub.accessType == ObjectAccessType.Constant) 
				|| (odSub.accessType == ObjectAccessType.ReadOnly))
				|| ((this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].nodeState != NodeState.PreOperational) 
				&& ((odSub.accessType == ObjectAccessType.ReadReadWriteInPreOp) 
				||  (odSub.accessType == ObjectAccessType.ReadWriteInPreOp) 
				||  (odSub.accessType == ObjectAccessType.ReadWriteWriteInPreOp)) 
				||  (odSub.accessType == ObjectAccessType.WriteOnlyInPreOp))
				)
			{
				e.Row.RejectChanges();  //reject new value
				if(e.Row.HasErrors)
				{
					e.Row.ClearErrors(); //clear the error
				}
				e.ProposedValue = e.Row[(int) (TblCols.actValue)].ToString();  //bung in old value
				e.Row.AcceptChanges(); //and mark row as unmodified
				MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
				return;
			}
			#endregion
			#region access level check
			if(this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].accessLevel < odSub.accessLevel)
			{
				e.Row.SetColumnError(e.Column, "Insufficient login level to overwrite this parameter - delete entry");
				Debug.Write( " Insufficient login level to overwrite this parameter - delete entry" );
				MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
				return;
			} //end if not high enough access
			#endregion
			//now test the actual value
			string errString = "";
			CANopenDataType datatype = (CANopenDataType) odSub.dataType;;
			#region switch datatype of this 'cell'
			switch (datatype)
			{
				case CANopenDataType.UNSIGNED8:
				case CANopenDataType.UNSIGNED16:
				case CANopenDataType.UNSIGNED24:
				case CANopenDataType.UNSIGNED32:
				case CANopenDataType.UNSIGNED40:
				case CANopenDataType.UNSIGNED48:
				case CANopenDataType.UNSIGNED56:
				case CANopenDataType.UNSIGNED64:
				case CANopenDataType.INTEGER8:
				case CANopenDataType.INTEGER16:
				case CANopenDataType.INTEGER24:
				case CANopenDataType.INTEGER32:
				case CANopenDataType.INTEGER40:
				case CANopenDataType.INTEGER48:
				case CANopenDataType.INTEGER56:
				case CANopenDataType.INTEGER64:
					#region integer
					if (odSub.format == SevconNumberFormat.BASE16)
					{
						errString = hexErrorHandling(odSub, inputString.ToUpper());
					}
					else if(odSub.format == SevconNumberFormat.BASE10) //BASE 10
					{
						errString = numericalErrorHandling(odSub, inputString.ToUpper());
					}
					else if (odSub.format == SevconNumberFormat.BIT_SPLIT) //BASE 10
					{
						//do nothing should not occur
						errString = "unexpected bit split format type. Please report";
					}
					//note no error handling is required gfor SPECIAL - since it it a drop down list
					#endregion integer
					break;
			
				case CANopenDataType.VISIBLE_STRING:
				case CANopenDataType.UNICODE_STRING:
				case CANopenDataType.OCTET_STRING:
					#region string
					if (inputString.Length > 255)
					{
						errString = "maximun permitted string length is 255 characters";
					}
					#endregion
					break;
			
				case CANopenDataType.REAL32:
				case CANopenDataType.REAL64:
					errString = floatErrorHandling(odSub,inputString.ToUpper());
					break;			

				case CANopenDataType.TIME_DIFFERENCE:
				case CANopenDataType.TIME_OF_DAY:
					errString = numericalErrorHandling(odSub, inputString.ToUpper());
					break;

				case CANopenDataType.BOOLEAN:
					//no error handling is required for boolen - these are now drop down lists
					break;

				default:  
					errString =  "Data Type handling for " +  datatype.ToString() + " not implementetd";
					break;
			} //end switch
			#endregion

			//implement error handling
			if(errString != "") 
			{
				e.Row.SetColumnError(e.Column, errString);
				Debug.Write( " " + errString );
				MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
				return;
			}
			//handle correct inputs
			e.Row.ClearErrors();  //reove error inidcator from row - ghost string will remain - can be got rid of by filtering DWdataset.Tables[selNodeTag.tableindex]!
			MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.currTblIndex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
		}

		private string numericalErrorHandling(ODItemData odSub, string inputString)
		{
			//first reject any invalid characters
			bool isInt = true;
			double inputDouble = 0;
			long inputLong = 0;
			CANopenDataType dataType = (CANopenDataType) odSub.dataType;
			if(inputString.IndexOf("0X") == 0)
			{
				#region hex entry
				try
				{
					inputLong = Convert.ToInt64(inputString, 16); //convert to base 10
				}
				catch
				{	
					return "Hexadecimal and numerical characters only";
				}
				#endregion hex entry
			}
			else  //float entry
			{
				if((dataType == CANopenDataType.TIME_DIFFERENCE) || (dataType == CANopenDataType.TIME_OF_DAY))
				{ 
					try
					{
						inputLong = System.Convert.ToInt64(inputString);
						isInt = true;
					}
					catch
					{
						return "Integer values only";
					}
				}
				else
				{ 
					try
					{
						if((odSub.scaling == 1) && (inputString.IndexOf(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) == -1))  
						{
							inputLong = System.Convert.ToInt64(inputString);
						}
						else
						{
							inputDouble = double.Parse(inputString, System.Globalization.NumberStyles.Any);
							isInt = false;
						}
					}
					catch
					{
						return "Numerical characters only";
					}
				}
			}
			//now look at actual value
			if((dataType != CANopenDataType.TIME_DIFFERENCE) && (dataType != CANopenDataType.TIME_OF_DAY))
			{
				if(isInt == true)
				{ //done like this to minmise float conversions to only itmes that must be scaled
					if(odSub.scaling != 1)
					{
						#region scaled
						double lowLimit = odSub.lowLimit* odSub.scaling;
						double highLimit = odSub.highLimit * odSub.scaling;
						if ( inputLong < lowLimit)
						{
							return  "Entry must be greater than minimum value (" + lowLimit.ToString() + ")";
						}
						if (inputLong > highLimit)
						{
							return "Entry must be less than maximum value (" + highLimit.ToString() + ")";
						}
						#endregion scaled
					}
					else
					{
						#region no scaling
						//note only integer types with scaling of 1 can be bitsplit
						long odSubLowLimit = odSub.lowLimit;
						long odSubHighLimit = odSub.highLimit;

						if(odSub.bitSplit != null)
						{
							odSubLowLimit = odSub.bitSplit.lowLimit;
							odSubHighLimit = odSub.bitSplit.highLimit;
						}
							if ( inputLong < odSubLowLimit)
							{
								return  "Entry must be greater than minimum value (" + odSub.lowLimit.ToString() + ")";
							}
							if (inputLong > odSubHighLimit)
							{
								return "Entry must be less than maximum value (" + odSub.highLimit.ToString() + ")";
							}
						#endregion no scaling
					}
				}
				else
				{
					if(odSub.scaling != 1)
					{
						#region scaled
						double lowLimit = odSub.lowLimit* odSub.scaling;
						double highLimit = odSub.highLimit * odSub.scaling;
						if ( inputDouble < lowLimit)
						{
							return  "Entry must be greater than minimum value (" + lowLimit.ToString() + ")";
						}
						if (inputDouble > highLimit)
						{
							return "Entry must be less than maximum value (" + highLimit.ToString() + ")";
						}
						#endregion scaled
					}
					else
					{
						#region no scaling
						if ( inputDouble < odSub.lowLimit)
						{
							return  "Entry must be greater than minimum value (" + odSub.lowLimit.ToString() + ")";
						}
						if (inputDouble > odSub.highLimit)
						{
							return "Entry must be less than maximum value (" + odSub.highLimit.ToString() + ")";
						}
						#endregion no scaling
					}
				}
			}
			return "";
		}

		private string hexErrorHandling(ODItemData odSub, string inputString)
		{
			long inputLong = 0;
			if(inputString.IndexOf("0X") == 0)//we have hex input
			{
				try
				{
					inputLong = Convert.ToInt64(inputString, 16); //convert to base 10
				}
				catch
				{
					return "Hexadecimal parameter , entry format is 0x##...";
				}
			}
			else  //hex formats MUST be entered in hex
			{
				return "Hexadecimal parameter , entry format is 0x##...";
			}
			//now look at actual value
			if ( inputLong < odSub.lowLimit )  //hex format has unity scaling
			{
				return  "Entry must be greater than minimum value (0x" + odSub.lowLimit.ToString("X") + ")";
			}
			if (inputLong > odSub.highLimit)
			{
				return "Entry must be less than maximum value (0x" + odSub.highLimit.ToString("X")  + ")";
			}
			return "";
		}

		private string floatErrorHandling(ODItemData odSub, string inputString)
		{
			float inputFloat = 0;
			double inputDouble = 0;
			if(inputString.IndexOf("0X") == 0)//we have hex input
			{
				#region hex entry
				if((CANopenDataType)odSub.dataType == CANopenDataType.REAL32)
				{
					#region real 32
					try
					{
						//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
						//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
						inputFloat = sysInfo.convertToFloat(inputString );
					}
					catch
					{
						return "Hexadecimal and float format (eg -4.56E+03 or 56.0) only";
					}
					#endregion real 32
				}
				else 
				{
					#region real 64
					inputDouble = sysInfo.convertToDouble(inputString);
					if(this.sysInfo.conversionOK == false)
					{
						return "Hexadecimal and float format (eg -4.56E+03 or 56.0) only";
					}
					#endregion real 64
				}
				#endregion hex entry
			}
			else  //float format entry
			{
				#region float entry
				//the number MUST be input in either hex or FLoating point format
				//If user enters say 65 - then DW has no way of knowing whther user intended this as a bitstring
				// or a base 10 number.
				if(inputString.IndexOf(".") ==-1)  
				{
					return "Hexadecimal and float format (eg -4.56E+03 or 56.0) only";
				}
				if((CANopenDataType)odSub.dataType == CANopenDataType.REAL32)
				{
					#region real 32
					try
					{
						//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
						//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
						inputFloat = sysInfo.convertToFloat(inputString );
					}
					catch
					{
						return "Hexadecimal and float format (eg -4.56E+03 or 56.0) only";
					}
					#endregion real 32
				}
				else if ((CANopenDataType)odSub.dataType == CANopenDataType.REAL64)
				{
					#region real 64
					inputDouble = sysInfo.convertToDouble(inputString);
					if(this.sysInfo.conversionOK == false)
					{
						return "Hexadecimal and float format (eg -4.56E+03 or 56.0) only";
					}
					#endregion real 64
				}
				else
				{
					return "Invalid data type. Float or Double expected";
				}
				#endregion float entry
			}
			//now check if we are within limts
			if((CANopenDataType)odSub.dataType == CANopenDataType.REAL32)
			{
				#region real 32
				if(inputFloat < odSub.real32.lowLimit)
				{
					return  "Entry must be greater than minimum value (" + odSub.real32.lowLimit.ToString() + ")";
				}
				if(inputFloat > odSub.real32.highLimit)
				{
					return "Entry must be less than maximum value (" + odSub.real32.highLimit.ToString() + ")";
				}
				#endregion real 32
			}
			else if ((CANopenDataType)odSub.dataType == CANopenDataType.REAL64)
			{
				#region real 64
				if(inputDouble < odSub.real64.lowLimit)
				{
					return  "Entry must be greater than minimum value (" + odSub.real64.lowLimit.ToString() + ")";
				}
				if(inputDouble > odSub.real64.highLimit)
				{
					return "Entry must be less than maximum value (" + odSub.real64.highLimit.ToString() + ")";
				}
				#endregion real 64
			}
			return "";
		}
		private string getFormatStr(SortedList formatList)
		{
			string tempStr = "";
			for(int i = 0;i<formatList.Count;i++)
			{
				string testStr = formatList[i].ToString().ToUpper();
				char [] enumChars = "ABCDEGHIJKLMNOPQRSTUVWXYZ!\"£$%^&*()_+=\\|,<>?/:;'@#~{[}]".ToCharArray(); //" and \ need extra \to be accepted
				int invalidCharIndex = testStr.IndexOfAny(enumChars);
				if(invalidCharIndex != -1)
				{

					#region get the enum numberical value - this will change later 
					string valStr = i.ToString();
					while (valStr.Length <4)
					{
						valStr = "0" + valStr;
					}
					#endregion get the enum numberical value - this will change later 
					tempStr += formatList[i].ToString();
					tempStr += "_";
					tempStr += valStr;
					tempStr += ":";
				}
			}
			//remove trailing colon
			if(tempStr.Length>1)
			{
				tempStr = tempStr.Remove((tempStr.Length-1), 1);
			}
			return tempStr;
		}
		#endregion CHange OD Parameter values methods

		#region dataGrid related methods
		private void createPPTableStyle(string passed_mappingName, uint passed_accessLevel)
		{
			UsrLevelIndex =  (int) (passed_accessLevel - 1);  //if login failed then use index 0(ie as level 1)
			if (UsrLevelIndex <0)
			{
				UsrLevelIndex = 0;
			}
			AccessLevels_GridColWidths 
				= new float [5] [] {ColPercentsLvl1,ColPercentsLvl2,ColPercentsLvl3,ColPercentsLvl4,ColPercentsLvl5};
			int [] colWidths  = new int[AccessLevels_GridColWidths[UsrLevelIndex].Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, AccessLevels_GridColWidths[UsrLevelIndex], 0, dataGrid1DefaultHeight);
			bool nodeInPreOp = true;
			if(this.sysInfo.nodes.Length>0)
			{
				if(this.sysInfo.nodes[0].nodeState != NodeState.PreOperational)
				{
					nodeInPreOp = false;
				}
			}
			tablestyle = new PPTableStyle(passed_mappingName, colWidths, nodeInPreOp, passed_accessLevel);
			this.dataGrid1.TableStyles.Add(tablestyle);//finally attahced the TableStyles to the datagrid
		} 

		private void handleResizeDataGrid(DataGrid myDG, int VScrollBarWidth)
		{
			if((findingSystem == false) && (myDG.TableStyles.Count>0))
			{
				int [] ColWidths = null;
				if(myDG == connectedDevicesDG)  
				{
					#region System status connected devices table
					ColWidths  = new int[connPercents.Length];
					ColWidths  = this.sysInfo.calculateColumnWidths(connectedDevicesDG, connPercents, VScrollBarWidth, connectedDevicesDGDefaultHeight);
					#endregion System status connected devices table
				}
				else if(myDG == emergencyDG)
				{
					#region System status emergency messages table
					ColWidths  = new int[emerPercents.Length];
					ColWidths  = this.sysInfo.calculateColumnWidths(emergencyDG, emerPercents, VScrollBarWidth, emergencyDGDefaultHeight);
					#endregion System status emergency messages table
				}
				else if(myDG ==devEmerDG)
				{
					#region Device Status emergency message sgenerated by this node table
					ColWidths  = new int[devEmerPercents.Length];
					ColWidths  = this.sysInfo.calculateColumnWidths(devEmerDG, devEmerPercents, VScrollBarWidth, devEmerDGDefaultHeight);
					#endregion Device Status emergency message sgenerated by this node table
				}
				else if(myDG == actFaultsDG)
				{
					#region Device Status Active Faults table
					ColWidths  = new int[devActFaultsPercents.Length];
					ColWidths  = this.sysInfo.calculateColumnWidths(actFaultsDG, devActFaultsPercents,VScrollBarWidth, actFaultsDGDefaultHeight);
					#endregion Device Status Active Faults table
				}
				else if(myDG == this.dataGrid1)
				{
					#region datagrid1
					if ((MAIN_WINDOW.currTblIndex == MAIN_WINDOW.DCFTblIndex)  
						&& (MAIN_WINDOW.DCFCompareActive == true))
					{//ensure we can have a negative index
						ColWidths = new int[DataGridColWidths[System.Math.Max(0,compNodeIDs.Count-1)].Length];
						ColWidths = this.sysInfo.calculateColumnWidths(dataGrid1, DataGridColWidths[System.Math.Max(0,compNodeIDs.Count-1)], VScrollBarWidth, dataGrid1DefaultHeight);
					}
					else
					{
						uint access = this.sysInfo.systemAccess;
						if(MAIN_WINDOW.currTblIndex<this.sysInfo.nodes.Length)
						{
							if((this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].vendorID == SCCorpStyle.SevconID)
								&& ((this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].productVariant == SCCorpStyle.bootloader_variant)
								|| (this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].productVariant == SCCorpStyle.selfchar_variant_new)
								|| (this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].productVariant == SCCorpStyle.selfchar_variant_old)))
							{
								access = 5;	
							}
						}
						UsrLevelIndex =  Math.Max(0,(int) (access - 1));  //if login failed then use index 0(ie as level 1)
						ColWidths  = new int[AccessLevels_GridColWidths[UsrLevelIndex].Length];
						ColWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, AccessLevels_GridColWidths[UsrLevelIndex],VScrollBarWidth, dataGrid1DefaultHeight);
					}
					#endregion datagrid1
				}
				else
				{
					return;
				}
				for (int i = 0;i<ColWidths.Length;i++)
				{
					if(myDG == this.dataGrid1)
					{
						#region datagrid 1
						if(MAIN_WINDOW.currTblIndex<this.sysInfo.nodes.Length)
						{
							myDG.TableStyles[MAIN_WINDOW.currTblIndex].GridColumnStyles[i].Width = ColWidths[i];
						}
						else if (MAIN_WINDOW.currTblIndex == MAIN_WINDOW.GraphTblIndex)
						{
							myDG.TableStyles[MAIN_WINDOW.GraphTblIndex].GridColumnStyles[i].Width = ColWidths[i];
						}
						else if (MAIN_WINDOW.currTblIndex == MAIN_WINDOW.DCFTblIndex)  
						{
							if(MAIN_WINDOW.DCFCompareActive == false)
							{
								myDG.TableStyles[MAIN_WINDOW.DCFTblIndex].GridColumnStyles[i].Width = ColWidths[i];
							}
							else
							{//add 2 now - monitoring it last table
								myDG.TableStyles[MAIN_WINDOW.compTblIndex].GridColumnStyles[i].Width = ColWidths[i];
							}
						}
						#endregion datagrid 1
					}
					else
					{
						myDG.TableStyles[0].GridColumnStyles[i].Width = ColWidths[i];
					}
				}
				myDG.Invalidate();
			}

		}
		private string updateRowFiltering(TreeNode selNode, treeNodeTag selNodeTag)
		{
			StringBuilder myFilter = new StringBuilder();
			#region construct section type filter
			if((selNodeTag.tagType == TNTagType.EDSSECTION)				|| (selNodeTag.tagType == TNTagType.ODITEM)
				|| (selNodeTag.tagType == TNTagType.XMLHEADER)		|| (selNodeTag.tagType == TNTagType.COUNTERLOGSCREEN)
				|| (selNodeTag.tagType == TNTagType.FAULTLOGSCREEN)	|| (selNodeTag.tagType == TNTagType.OPLOGSCREEN)
				|| (selNodeTag.tagType == TNTagType.DATALOGSSCREEN)
				|| (selNodeTag.tagType == TNTagType.LOGS)      || (selNodeTag.tagType == TNTagType.SELFCHARSCREEN))  //self char for future - prbalbly self char data to gi under here
			{
				#region get all child TreeNodes below selected node
				childTreeNodeList = new ArrayList();
				if(selNodeTag.assocSub != null)
				{
					childTreeNodeList.Add(selNodeTag);
				}
				getChildODItems(selNode, false); 
				this.progressBar1.Value = this.progressBar1.Minimum;
				this.progressBar1.Maximum = childTreeNodeList.Count;
				#endregion get all child TreeNodes below selected node
				myFilter.Append("(");
				for(int i = 0;i<childTreeNodeList.Count;i++)
				{
					if(this.progressBar1.Value<this.progressBar1.Maximum)
					{
						this.progressBar1.Value++;
					}
					treeNodeTag tag = (treeNodeTag) childTreeNodeList[i];
					if(tag.assocSub != null)
					{
						myFilter.Append(TblCols.Index.ToString() + " = '0x" + tag.assocSub.indexNumber.ToString("X").PadLeft(4, '0') + "'");
						if(i<childTreeNodeList.Count-1)
						{
							myFilter.Append(" OR ");
						}
					}
				}
				if(myFilter.ToString() == "(")
				{ //ensure no matches 
					myFilter.Append(TblCols.Index.ToString() + " = '-1' ");
				}
				myFilter.Append(")");
			}
			//now apply the row filter

			#endregion construct section type filter
			#region apply accessTypeFilter and accessLevelFilter if we are hiding read only items
			if(this.toolBar1.Buttons[6].Pushed == true) //i.e. we should currently be hiding the ROs
			{
				bool nodeInPreOp = true;
				if(selNodeTag.tableindex< this.sysInfo.nodes.Length)
				{
					if(this.sysInfo.nodes[selNodeTag.tableindex].nodeState !=NodeState.PreOperational)
					{
						nodeInPreOp = false;
					}
				}

				#region construct access Type filter
				if(myFilter.ToString() != "")
				{
					myFilter.Append(" AND ");
				}
				if(nodeInPreOp == true)
				{
					myFilter.Append(" ( " 
						+ TblCols.accessType.ToString()  + " = 'ReadWrite' OR " 
						+ TblCols.accessType.ToString()  + " = 'DWDisplayOnly' OR " 
						+ TblCols.accessType.ToString()  + " = 'ReadReadWrite' OR " 
						+ TblCols.accessType.ToString()  + " = 'ReadWriteWrite' OR " 
						+ TblCols.accessType.ToString()  + " = 'ReadReadWriteInPreOp' OR " 
						+ TblCols.accessType.ToString()  + " = 'ReadWriteInPreOp' OR " 
						+ TblCols.accessType.ToString()  + " = 'ReadWriteWriteInPreOp' OR " 
						+ TblCols.accessType.ToString()  + " = 'WriteOnlyInPreOp' OR " 
						+ TblCols.accessType.ToString()  + " = 'WriteOnly' )");
				}
				else
				{
					myFilter.Append(
						TblCols.accessLevel.ToString()+ "<= '4' AND ("
						+ TblCols.accessType.ToString()  + " = 'ReadWrite' OR " 
						+ TblCols.accessType.ToString()  + " = 'DWDisplayOnly' OR " 
						+ TblCols.accessType.ToString()  + " = 'ReadReadWrite' OR " 
						+ TblCols.accessType.ToString()  + " = 'ReadWriteWrite' OR " 
						+ TblCols.accessType.ToString()  + " = 'WriteOnly' )");
				}
				#endregion construct access Type filter
				#region construct access Level filter
				if(this.sysInfo.nodes[selNodeTag.tableindex].isSevconApplication() == true)
				{
					switch (sysInfo.systemAccess)
					{
						case 4:
							myFilter.Append(" AND ("  + TblCols.accessLevel.ToString() + "<= '4' )");
							break;
						case 3:
							myFilter.Append(" AND ("  + TblCols.accessLevel.ToString() + "<= '3' )");
							break;
						case 2:
							myFilter.Append(" AND ("  + TblCols.accessLevel.ToString() + "<= '2' )");
							break;
						case 1: 
							myFilter.Append(" AND ("  + TblCols.accessLevel.ToString() + "<= '1' )");
							break;
						default:
							break;
					}
				}
				#endregion construct access Level filter
			}
			#endregion apply accessTypeFilter and accessLevelFilter if we are hiding read only items
			if(selNodeTag.tableindex == MAIN_WINDOW.GraphTblIndex)
			{
				if(selNode != this.GraphCustList_TN)
				{
					if(myFilter.Length != 0)
					{
						myFilter.Append(" AND ");
					}
					myFilter.Append(
						"(" + TblCols.NodeID.ToString()  + " = " + selNodeTag.nodeID.ToString() + ")");
				}
			}
			this.progressBar1.Value = this.progressBar1.Minimum;
			this.statusBarPanel3.Text = "Applying Row filters";
			return myFilter.ToString();
		}

		private void RefreshRow(int row)
		{
			Rectangle rect = this.dataGrid1.GetCellBounds(row, 0);
			int myTop  = (int) rect.Top;
			rect = new Rectangle(rect.Right, myTop, this.dataGrid1.Width, rect.Height);
			this.dataGrid1.Invalidate(rect);
		}

		private void dataGrid1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			this.dataGrid1.ContextMenu = null; //default to prevent it being shown 
			if(MAIN_WINDOW.UserInputInhibit == true)
			{
				this.statusBarPanel3.Text = "Item selection inhibited when type 1 window is open";
			}
			string statusBarOverrideText = AllType1FormsClosed();
			if((statusBarOverrideText != "") || (this.COB_PDO_FRM != null))
			{
				return;
			}
			Point pt = this.dataGrid1.PointToClient(Control.MousePosition);
			DataGrid.HitTestInfo hti = this.dataGrid1.HitTest(pt);
			if(hti.Type == DataGrid.HitTestType.Cell)// && (MAIN_WINDOW.currTblIndex<this.sysInfo.nodes.Length))
			{
				DataView tmpView = (DataView) this.dataGrid1.DataSource;
				DataRow TblRow = tmpView[hti.Row].Row;  //independent of datagrid columns being displayed
				if(e.Button == MouseButtons.Right)
				{
					this.selectedDgRowInd = hti.Row;
					setupAndDisplayDataGridContextMenu(pt, TblRow);
				}
			}
		}
		private void dataGrid1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Point pt = this.dataGrid1.PointToClient(Control.MousePosition);
			DataGrid.HitTestInfo hti = this.dataGrid1.HitTest(pt);
			if(hti.Type == DataGrid.HitTestType.Cell)
			{
                if (MAIN_WINDOW.currTblIndex < this.sysInfo.nodes.Length)
                {
				DataView tmpView = (DataView) this.dataGrid1.DataSource;
				DataRow TblRow = tmpView[hti.Row].Row;  //independent of datagrid columns being displayed
				ODItemData sub = (ODItemData)TblRow[(int) TblCols.odSub];
				this.toolTip1.SetToolTip(this.dataGrid1, sub.tooltip);
                }
			}
			else
			{
				this.toolTip1.SetToolTip(this.dataGrid1, "");
			}
		}
		private bool rowCanBeMonitored(ODItemData odSub, bool dragDrop)
		{
			//we need to extract just the datatype part out of displayType for ewhen we have Arrays
			CANopenDataType dataType = (CANopenDataType) odSub.dataType;
			if(	
				((odSub.accessType != ObjectAccessType.WriteOnly) //cannot be monitored
				&& (odSub.accessType != ObjectAccessType.WriteOnlyInPreOp)
				&& ((dataType == CANopenDataType.INTEGER16)  
				||  (dataType == CANopenDataType.INTEGER24)
				||  (dataType == CANopenDataType.INTEGER32)
				||  (dataType == CANopenDataType.INTEGER40)
				||  (dataType == CANopenDataType.INTEGER48)
				||  (dataType == CANopenDataType.INTEGER56)
				||  (dataType == CANopenDataType.INTEGER64)
				||  (dataType == CANopenDataType.INTEGER8)
				||  (dataType == CANopenDataType.REAL32)
				||  (dataType == CANopenDataType.REAL64)
				||  (dataType == CANopenDataType.UNSIGNED16)
				||  (dataType == CANopenDataType.UNSIGNED24)
				||  (dataType == CANopenDataType.UNSIGNED32)
				||  (dataType == CANopenDataType.UNSIGNED40)
				||  (dataType == CANopenDataType.UNSIGNED48)
				||  (dataType == CANopenDataType.UNSIGNED56)
				||  (dataType == CANopenDataType.UNSIGNED64)
				||  (dataType == CANopenDataType.UNSIGNED8)
				||  (dataType == CANopenDataType.BOOLEAN))
				)
				|| (odSub.subNumber == -1)) //header row - eg datatype of RECORD , PDO_MAPPING etc
			{
				if( (dragDrop == true) //we can drag header rows
					|| ((dragDrop == false) && (odSub.subNumber != -1)) //but not indivicdually select them in datagrid
					) //header row - eg datatype of RECORD , PDO_MAPPING etc
				{
					return true;
				}
			}
			return false;
		}
		private void setupAndDisplayDataGridContextMenu(Point pt,  DataRow TblRow)
		{
            ODItemData odSub = null;
			MenuItem dataGridContextMenu_MI1_Mon, dataGridContextMenu_MI2_DCF;
			this.contextMenu1.MenuItems.Clear();

            if ((MAIN_WINDOW.currTblIndex != MAIN_WINDOW.DCFTblIndex) || (DCFCompareActive == false))
            {
                odSub = (ODItemData)TblRow[(int)TblCols.odSub];
            }

            if ((odSub != null) && (odSub.subNumber != -1)) //ie not a header row
			{
				if(MAIN_WINDOW.currTblIndex <this.sysInfo.nodes.Length)
				{
					#region set up monitoring menu option for attahced CAN node
					if(rowCanBeMonitored(odSub, false) == true) 
					{
						if( ((bool)TblRow[(int) TblCols.Monitor]) == false)
						{
							dataGridContextMenu_MI1_Mon = new MenuItem("Monitor this item");
						}
						else
						{
							dataGridContextMenu_MI1_Mon = new MenuItem("Stop monitoring this item");
						}
						dataGridContextMenu_MI1_Mon.Click +=new EventHandler(dataGridContextMenu_MI1_Mon_Click);
						this.contextMenu1.MenuItems.Add(dataGridContextMenu_MI1_Mon);
					}
					#endregion set up monitoring menu option for attahced CAN node

					#region setup DCF add/remove menu option for attahced CAN node
					bool DCFInhibit = false;
					if(DCFCustList_TN.Nodes.Count > 0)
					{  //check that these nodes are from same node Id
						treeNodeTag alreadyInDCFNodeTag = (treeNodeTag) DCFCustList_TN.Nodes[0].Tag;
						if((this.sysInfo.DCFnode.nodeID != sysInfo.nodes[MAIN_WINDOW.currTblIndex].nodeID)
							|| (sysInfo.DCFnode.vendorID != sysInfo.nodes[MAIN_WINDOW.currTblIndex].vendorID)
							|| (sysInfo.DCFnode.productCode != sysInfo.nodes[MAIN_WINDOW.currTblIndex].productCode)
							|| (sysInfo.DCFnode.revisionNumber != sysInfo.nodes[MAIN_WINDOW.currTblIndex].revisionNumber))
						{
							DCFInhibit = true;
						}
					}
					if (this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].isSevconBootloader() == true)
					{
						DCFInhibit = true;
					}
					else if (this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].isSevconSelfChar() == true)
					{
						DCFInhibit = true;
					}
					if(DCFInhibit == false)
					{
						object[] myKeys = new object[2];
						myKeys[0] = TblRow[(int)TblCols.Index].ToString();
						myKeys[1] = TblRow[(int)TblCols.sub].ToString();
						DataRow testRow = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].Rows.Find(myKeys);
						if(testRow != null) //this item is alread in the DCF
						{
							dataGridContextMenu_MI2_DCF = new MenuItem("Remove this item from DCF store");
						}
						else
						{
							dataGridContextMenu_MI2_DCF = new MenuItem("Add this item to DCF store");
						}
						dataGridContextMenu_MI2_DCF.Click +=new EventHandler(dataGridContextMenu_MI2_DCF_Click);
						this.contextMenu1.MenuItems.Add(dataGridContextMenu_MI2_DCF);
					}
						#endregion setup DCF add/remove menu option for attahced CAN node

					#region add menu items for Domains
					if(odSub.displayType == CANopenDataType.DOMAIN)
					{
                        if (odSub.sectionType == (int)SevconSectionType.DATALOGGING)
						{
							MenuItem exportDomainMI = new MenuItem("Read datalog and export to Excel file");
							exportDomainMI.Click +=new EventHandler(exportDomainMI_Click);
							this.contextMenu1.MenuItems.Add(exportDomainMI);
						}
                        else if (odSub.sectionType == (int)SevconSectionType.FAULTLOG)
						{
							MenuItem exportFLDomainMI = new MenuItem("Read faultlog and export to Excel file");
							exportFLDomainMI.Click +=new EventHandler(exportFLDomainMI_Click);
							this.contextMenu1.MenuItems.Add(exportFLDomainMI);
						}
					}
					#endregion add menu items for Domains
                }
					else if(MAIN_WINDOW.currTblIndex == MAIN_WINDOW.GraphTblIndex)
					{
						#region setup remove monitoring menu option for monitoring Table
						//TODO can only 
						dataGridContextMenu_MI1_Mon = new MenuItem("Stop monitoring this item");
						dataGridContextMenu_MI1_Mon.Click +=new EventHandler(dataGridContextMenu_MI1_Mon_Click);
						this.contextMenu1.MenuItems.Add(dataGridContextMenu_MI1_Mon);
						#endregion setup monitoring menu option for monitoring Table
					}
					else if (MAIN_WINDOW.currTblIndex == MAIN_WINDOW.DCFTblIndex)
					{
						#region setup DCF remove menu option for DCf table
						dataGridContextMenu_MI2_DCF = new MenuItem("Remove this item from DCF store");
						dataGridContextMenu_MI2_DCF.Click +=new EventHandler(dataGridContextMenu_MI2_DCF_Click);
						this.contextMenu1.MenuItems.Add(dataGridContextMenu_MI2_DCF);
						#endregion setup DCF remove menu option for DCf table
					}
				}

				if(this.contextMenu1.MenuItems.Count>0)
				{
					#region display the context menu
					this.dataGrid1.ContextMenu = this.contextMenu1;
					this.contextMenu1.Show(this.dataGrid1,pt);
					#endregion display the context menu
			}
		}
		private void dataGridVScrollBar_VisibleChanged(object sender, EventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.VScrollBar")
			{
				//we are only interested in the Vscrollbars on datagrids
				VScrollBar myVscroll = (VScrollBar) sender;
				if( myVscroll.Parent.GetType().Equals( typeof( DataGrid ) ) ) 
				{
					DataGrid myDG = (DataGrid) (myVscroll.Parent);
					if(myVscroll.Visible == true)
					{
						try
						{
							handleResizeDataGrid(myDG, Math.Max(myVscroll.Width, 0));
						}
						catch(Exception e9)
						{
							SystemInfo.errorSB.Append("handleResizeDataGrid() casued exception: " + e9.Message + " " + e9.InnerException);
						}
					}
					else
					{
						try
						{
							handleResizeDataGrid(myDG, 0);
						}
						catch(Exception e1)
						{
							SystemInfo.errorSB.Append("handleResizeDataGrid() casued exception: " + e1.Message + " " + e1.InnerException);
						}

					}
				}
			}
		}

		private void myDG_Resize(object sender, EventArgs e)
		{
			if(sender.GetType().Equals( typeof( DataGrid ) ) ) 
			{
				DataGrid myDG = (DataGrid) sender;
				int VScrollBarwidth = 0;
				foreach( Control c in myDG.Controls ) 
				{
					if ( c.GetType().Equals( typeof( VScrollBar ) ) ) 
					{
						if(c.Visible == true)
						{
							VScrollBarwidth = c.Width;  //remove width of scroll bar from overall calc
						}
						break;
					}
				}
				try
				{
					handleResizeDataGrid(myDG, VScrollBarwidth);
				}
				catch(Exception e3)
				{
					SystemInfo.errorSB.Append("handleResizeDataGrid() casued exception: " + e3.Message + " " + e3.InnerException);
				}

			}
		}
		#endregion dataGrid related methods

		#region delegates and timer elapsed methods
		internal void stateChangeListener( COBIDType messageType, int nodeID, int CANIndex, NodeState newNodeState, string emergencyMessage )
		{
			if ( messageType == COBIDType.ProducerHeartBeat )
			{
				#region Heartbeat message received
				if((newNodeState == NodeState.Bootup) 
					&& (DriveWizard.SEVCONProgrammer.programmingRequestFlag == false))
				{
					//change later to account for different tab sin the Options window
					if(( selectProfile == null) 
						&& (options == null) 
						&& (PROG_DEVICE_FRM ==null)
						&& (SELF_CHAR_FRM == null))  //restarting when finding system is OK
					{
						reEstablishCommsRequired = true;
						return;
					}
				}
				//node has changed state -> after we found system
				if(( selectProfile == null) && (options == null))  //restarting when finding system is OK
				{
					if((nodeID != 0)  && (this.sysInfo.nodes.Length>0))
					{
						this.nodeStateChangedFlags[CANIndex] = true;
						nodeStateChanged = true;
					}
				}
				DriveWizard.SEVCONProgrammer.programmingRequestFlag  = false; //reset the programming request flag
				#endregion Heartbeat message received
				return; //B&B for fast messages coming in 
			}
			else if (( messageType == COBIDType.EmergencyWithInhibit ) 
				|| (messageType == COBIDType.EmergencyNoInhibit))
			{
				emergencyMessage emer = new emergencyMessage(nodeID, emergencyMessage);
				emerAL.Add(emer);
				return;
                    }
                }

        /// <summary>
        /// Delegate called by the SearchForm when a user initiates a search of treeview1.
        /// </summary>
        /// <param name="searchType">The parameters to be searched for items in treeview1.</param>
        /// <param name="searchString">The string to be searched for.</param>
        /// <param name="searchInstance">The required match instance to be found.</param>
        /// <param name="forceUpdateFromDevice">True if the user required OD values to be read
        /// from the physical device before being displayed on the datagrid after a successful search.</param>
        internal void searchChangeListener(SearchArea searchType, string searchString, int searchInstance, bool forceUpdateFromDevice)
        {
            int foundInstance = 0;
            TreeNode foundTreeNode = null;
            TreeNode rootNode = null;
            TreeNode searchTN = null;
            int searchIndex = 0;

            searchInProgress = true;
            ((SearchForm)this.SEARCH_FRM).disableNextSearch();

            #region if the search is on an ODindex, first convert search string into an integer
            // Do here because we only want to do it once (the called search is recursive).
            if ((searchType == SearchArea.INDEX) || (searchType == SearchArea.ALL))
            {
                try
                {
                    if (searchString.ToUpper().Contains("0X") == true)
                    {   //hex 
                        searchIndex = System.Convert.ToInt32(searchString.Trim(), 16);
                    }
                    else //dec
                    {
                        searchIndex = System.Convert.ToInt32(searchString.Trim());
                    }
                }
                catch
                {
                    if (searchType == SearchArea.INDEX)
                    {
                        //No message box error displayed as already caught in SearchForm form.
                        return;
                    }
            }
            }
            #endregion if the search is on an ODindex, first convert search string into an integer

            #region is this the start of a new the search?
            if (searchInstance == 1)
            {
                searchOtherNodesFoundTotal = 0; // no instances found in previous nodes of treeview1
                searchTNIndex = 0;              // start search on the first node of treeView1
            }
            #endregion is this the start of a new the search?

            // Carry over instances found on previous nodes of treeView1 since the search covers 
            // all nodes.
            foundInstance = searchOtherNodesFoundTotal;

            #region search for the required instance in treeView1
            do
            {
                // rootNode initialised to current search node of treeView1
                rootNode = treeView1.Nodes[searchTNIndex];

                #region perform search
                if (rootNode != null)
                {
                    // find the root of this node
                    while (rootNode.Parent != null)
                    {
                        rootNode = rootNode.Parent;
                    }

                    //perform required type of search
                    foundTreeNode = searchInTreeNode(rootNode, searchType, searchString, searchIndex, searchInstance, ref foundInstance);
                }
                #endregion perform search

                #region get the next node to search if the required instance wasn't found in the current node
                if (foundTreeNode == null)
                {
                    if (searchTNIndex < (treeView1.Nodes.Count - 1))
                    {
                        // select the next node in treeView1 & remember the instances found up til now
                        searchTNIndex++;
                        searchTN = treeView1.Nodes[searchTNIndex];
                        searchOtherNodesFoundTotal = foundInstance;
                    }
                    else // nothing left to search so quit loop
                    {
                        break;
                    }
                }
                #endregion get the next node to search if the required instance wasn't found in the current node
            }
            while ((foundTreeNode == null) && (searchInstance != foundInstance));
            #endregion search for the required instance in treeView1

            #region if the reqd instance is found, select it on the tree view and show it to the user
            if (foundTreeNode != null)
            {
                this.handleTreeNodeSelection(foundTreeNode, forceUpdateFromDevice);
            }
            #endregion if the reqd instance is found, select it on the tree view and show it to the user
            #region else warn user & reset search back to start
            else
            {
                string message = "No more occurrences found in this node of the tree.";

                if (searchInstance == 1)
                {
                    message = "No occurrences found in this node of the tree.";
                }

                MessageBox.Show(message, "Search Window", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //SEARCH_FRM could've been closed by handleTreeNodeSelection()
                if (SEARCH_FRM != null)
                {
                    ((SearchForm)this.SEARCH_FRM).resetSearchInstance();
                }
            }
            #endregion else warn user & reset search back to start

            if ((dataRetrievalThread == null) && (SEARCH_FRM != null))
            {
                searchInProgress = false;       // also cleared when dataRetrievalThread stops
                ((SearchForm)this.SEARCH_FRM).enableNextSearch();
			}
		}

		private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if ( restartCANThread != null )
			{
				#region CAN Restart thread - restart sCAN dongle after Windows hibernation
				if( ( restartCANThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
					restartCANThread = null;
					//don't disable timer 2 here - in case DW was in middle of something when we hibernated
					this.ToolsMenu.Enabled = true;
					this.main_hlp_mi.Enabled = true;
					this.file_mi.Enabled = true;
					enableAllActiveOwnedForms();
                    if (SystemInfo.errorSB.Length > 0)
                    {
                        this.statusBarPanel3.Text = "Recovery from PC hibernation failed.";
                        this.showUserControls();
                        MAIN_WINDOW.reEstablishCommsRequired = true;
                    }
                    else
                    {
                        this.statusBarPanel3.Text = "Recovery from PC hibernation completed OK";
                        this.showUserControls();
                    }
					
                    
					
				}
				#endregion CAN Restart thread - restart sCAN dongle after PC hibernation
			}
			else if ( dataRetrievalThread != null )
			{
				#region retrieving all data for DCf or Data monitoring
				string threadName = dataRetrievalThread.Name;

                // progressBar1 value now updated from nodeValue to avoid cross-threading,
                // all the while the DCF or mon list thread is running.
                if ((threadName == "Monitoring List Data Retrieval") || (threadName == "DCF Data Retrieval"))
                {
                    if (nodeValue < this.progressBar1.Maximum)
                    {
                        progressBar1.Value = nodeValue;
                    }
                }

				if((dataRetrievalThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
#if DEBUG
                    System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " ended.");
#endif
					timer2.Enabled = false; //kill timer
					if(MAIN_WINDOW.currTblIndex<this.sysInfo.nodes.Length)
					{
						this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].numConsecutiveNoResponse = 0; //reset 
					}
					dataRetrievalThread = null;
					switch(threadName)
					{
						case "COBDataRetrieval":
							#region Comms data retrieval
							this.hideUserControls();
							this.currCOB = null;
							this.activeVPDOType = SevconObjectType.NONE;
							this.activeIntPDO = null;
							this.currMapPnl = null;
							this.currRxPDOnode = null;
							this.currTxPDOnode = null;
							expandTreeViewToIncludeALlPDOableItems();
							createPDOableNodesList();
							Array temp = System.Enum.GetValues(typeof(DriveWizard.COBIDPriority));
							CB_SPDO_priority.DataSource =  System.Enum.GetValues(typeof(DriveWizard.COBIDPriority));
							#region draw representation of nodes on screen
							setupPDOGraphics();
							setUpInterfacePanels();
							setUpInternalPDOPanels();
							setupSysPDOExtensionPanels();  //always do these 
							layoutPDOGraphics();
							layoutTxPDOInterfacePanels(); 
							layoutTxSysPDOExpansionPanels();
							layoutRxPDOInterfacePanels(); 
							layoutRxSysPDOExpansionPanels();
							setupCOBDataBindings();
							this.updatePDODataBindings(0);  //do this afer stting up currCOB - do full databinding
							#endregion draw representation of nodes on screen
							calculateScreenLinesForPDOCOBSInSystem();
							this.SystemPDOsScreenActive = true;
                            //Reset search instance on entering PDO to prevent possible exception 
                            //if outwith search instance on new tree view
                            if (SEARCH_FRM != null)
                            {
                                ((SearchForm)SEARCH_FRM).resetSearchInstance();
                            }
							switchOnPDOEventHandlers();
							showUserControls();
							if(SystemInfo.errorSB.Length>0)
							{
								sysInfo.displayErrorFeedbackToUser("Errors found when reading COBs in System");
							}
							#endregion Comms data retrieval
							return;

						case "Monitoring List Data Retrieval":
							#region Moniriting initial data retireval
							updateTableFromDI();
							//following lines are for after a drag drop operaiton 
							//we need to move focus to the DCF or Data monitirng but we mustn't do this until
							//the data gathering thread is complete
							if(this.nextRequiredNode != null)
							{
								currentTreenode = nextRequiredNode;
								this.nextRequiredNode  = null;  //prevent re-entry
								if(currentTreenode.Nodes.Count>0)
								{
									currentTreenode = currentTreenode.Nodes[0];
								}
								this.treeView1.SelectedNode = currentTreenode;
								this.handleTreeNodeSelection(currentTreenode, false);
							}
							else
							{
								this.updateRowColoursArray();
								showUserControls();
							}

							if(this.GraphCustList_TN.Nodes.Count>0)
							{
								this.GraphCustList_TN.Text = "Monitoring store";
								this.GraphCustList_TN.Expand();  //initially expand to show the something is there
								//finally set off the monitoring timer
								this.dataMonitoringTimer.Enabled = true;
							}

							this.monStore.fromFile = false;//once we drag items in then this option is no longth valid
							if(SystemInfo.errorSB.Length>0)
							{
								sysInfo.displayErrorFeedbackToUser("Not all sub index settings updated:");
							}
							break;
							#endregion Moniriting initial data retireval

						case "DCF Data Retrieval":
							#region DCF data retrieval
							updateTableFromDI();
							//following lines are for after a drag drop operaiton 
							//we need to move focus to the DCF or Data monitirng but we mustn't do this until
							//the data gathering thread is complete
							if(this.nextRequiredNode != null)
							{
								currentTreenode = nextRequiredNode;
								this.nextRequiredNode  = null;  //prevent re-entry
								if(currentTreenode.Nodes.Count>0)
								{
									currentTreenode = currentTreenode.Nodes[0];
								}
								this.treeView1.SelectedNode = currentTreenode;
								this.handleTreeNodeSelection(currentTreenode, false);
							}
							else
							{
								this.updateRowColoursArray();
								showUserControls();
							}
							if(SystemInfo.errorSB.Length>0)
							{
								sysInfo.displayErrorFeedbackToUser("Not all sub index settings updated:");
							}
							break;
							#endregion DCF data retrieval

						case "Section Data Retrieval":
							#region retrieving a whole section of data from a devcie
							updateTableFromDI();
                            tbb_RefreshData.Enabled = true;
							//following lines are for after a drag drop operaiton 
							//we need to move focus to the DC for Data monitirng but we mustn't do this until
							//the data gathering thread is complete
							if(this.nextRequiredNode != null)
							{
								currentTreenode = nextRequiredNode;
								this.nextRequiredNode  = null;  //prevent re-entry
								if(currentTreenode.Nodes.Count>0)
								{
									currentTreenode = currentTreenode.Nodes[0];
								}
								this.treeView1.SelectedNode = currentTreenode;
								this.handleTreeNodeSelection(currentTreenode, false);
							}
							else
							{
								showUserControls();
							}
							if(SystemInfo.errorSB.Length>0)
							{
								sysInfo.displayErrorFeedbackToUser("Not all sub index settings updated:");
							}
							#endregion retrieving a whole section of data from a devcie
							break;

                        case "Table Data Retrieval":
                            #region table data retrieval
                            updateTableFromDI();
                            tbb_RefreshData.Enabled = true;
                            showUserControls(); //will alos expand/collapse node as required
                            if (SystemInfo.errorSB.Length > 0)
                            {
                                sysInfo.displayErrorFeedbackToUser("Not all indices updated from the device:\n");
                            }
                            #endregion table data retrieval
                            break;

                        case "Device Panel Data Retrieval":
                            #region device panel data retrieval
                            if (this.sysInfo.nodes[nodeTag.tableindex].battVoltSub != null)
                            {
                                double tempFlt = this.sysInfo.nodes[nodeTag.tableindex].battVoltSub.currentValue * this.sysInfo.nodes[nodeTag.tableindex].battVoltSub.scaling;
                                this.BattVoltLbl.Text = "Battery Voltage: \t" + tempFlt.ToString("F1") + " volts";
                            }
                            if (this.sysInfo.nodes[nodeTag.tableindex].capvoltSub != null)
                            {
                                double tempFlt = this.sysInfo.nodes[nodeTag.tableindex].capvoltSub.currentValue * this.sysInfo.nodes[nodeTag.tableindex].capvoltSub.scaling;
                                this.CapVoltLbl.Text = "Capacitor Voltage: \t" + tempFlt.ToString("F1") + " volts";
                            }
                            if (this.sysInfo.nodes[nodeTag.tableindex].temperatureSub != null)
                            {
                                double tempFlt = this.sysInfo.nodes[nodeTag.tableindex].temperatureSub.currentValue * this.sysInfo.nodes[nodeTag.tableindex].temperatureSub.scaling;
                                this.temperatureLbl.Text = "Temperature: \t" + tempFlt.ToString("F1") + " degrees C";
                            }
                            if (sysInfo.nodes[nodeTag.tableindex].configChksumSub != null)
                            {
                                this.configChkSum[nodeTag.tableindex] = "Config Chksum: " + "0x" + this.sysInfo.nodes[nodeTag.tableindex].configChksumSub.currentValue.ToString("X").PadLeft(4, '0');
                                this.configChkSumLbl.Text = this.configChkSum[nodeTag.tableindex];
                            }

                            DevSevconGB.Show();
                            DeviceStatusPanel.Refresh();
                            this.DeviceStatusPanel.BringToFront();
                            MAIN_WINDOW.UserInputInhibit = false; // thread finished so now allow user to enter input
                            nodeTag = null; //finished with it
                            if (SystemInfo.errorSB.Length > 0)
                            {
                                sysInfo.displayErrorFeedbackToUser("Not all sub index settings updated:");
                            }
                            #endregion device panel data retrieval
                            break;

                        case "First Device Panel Data Retrieval":
                            #region first device panel data retrieval
                            fillTreeView();
                            fillSystemStatusPanel();
                            this.currentTreenode = this.SystemStatusTN;
                            handleTreeNodeSelection(currentTreenode, true);  //force datagrid to update correctly
                            for (int deviceIndex = 0; deviceIndex < this.sysInfo.nodes.Length; deviceIndex++)
                            {
                                //add event handlers for tables representing CAN devices only - everything else is filled automatically
                                MAIN_WINDOW.DWdataset.Tables[deviceIndex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
                                foreach (DataRow row in MAIN_WINDOW.DWdataset.Tables[deviceIndex].Rows)
                                {
                                    row.AcceptChanges();
                                }
                            }
                            this.toolBar1.Buttons[6].Pushed = false;  //when pushed we are hiding the read only items
                            this.firstNodeSelectionAfterConnection = true;
                            setupAnddisplayContextMenu();//for things that are reset/modded as part of reconnection eg ignore emegny messages

                            this.timer3.Enabled = true; //allow LED flashing etc
                            this.treeView1.EndUpdate();//leave in - otherwise sometimes tree view stays blank 
                            MAIN_WINDOW.UserInputInhibit = false;
                            this.ToolsMenu.Enabled = true;
                            //only enable find and refresh data menu items if nodes currently connected (prevent exception)
                            if (treeView1.Nodes.Count > 0)
                            {
                                this.FindMI.Enabled = true;
                                this.miRefreshData.Enabled = true;
                            }
                            else
                            {
                                this.FindMI.Enabled = false;
                                this.miRefreshData.Enabled = false;
                            }

                            this.file_mi.Enabled = true;
                            this.main_hlp_mi.Enabled = true;
                            this.progressBar1.Visible = true;
                            nodeTag = null; //finished with it
                            showUserControls();

                            if (SystemInfo.errorSB.Length > 0)
                            {
                                sysInfo.displayErrorFeedbackToUser("Errors found when connecting to nodes:\n");
                            }
                            #endregion first device panel data retrieval
                            break;

						case "DCF Comparison data retrieval":
							#region DCF compare Data Retrieval Thred
							this.progressBar1.Value = this.progressBar1.Minimum;
							this.dataGrid1.DataSource = comparisonTable.DefaultView;
							this.updateDeviceLevelOtherWindowTreenodes();
							this.updateSystemLevelOtherWindowTreenodes();
							this.timer3.Enabled = true;
							this.showUserControls();
							if(SystemInfo.errorSB.Length>0)
							{
								sysInfo.displayErrorFeedbackToUser("Not all sub index settings updated:");
							}
							break;
							#endregion DCF compare Data Retrieval Thred

						case "DCF compare full table creation":
							#region DCF full table creation
							this.progressBar1.Value = this.progressBar1.Minimum;
							this.dataGrid1.DataSource = comparisonTable.DefaultView;
							this.updateDeviceLevelOtherWindowTreenodes();
							this.updateSystemLevelOtherWindowTreenodes();
							this.timer3.Enabled = true;
							if( threadName == "DCF compare full table creation")
							{//only do this first time into DCF compare table
								//so many potetial clums we have to stick the horizontal scroll bar back in
								if(this.sysInfo.noOfNodesWithValidEDS>2)
								{
									foreach(Control c in this.dataGrid1.Controls)
									{
										if	(c.GetType().Equals( typeof( HScrollBar ) ) ) 
										{
											c.Height = 10;
											c.Visible = true;
											break;
										}
									}
								}
								forceDataGridResize(this.dataGrid1);
							}
							this.showUserControls();
							if((SystemInfo.errorSB.Length>0) || (globalfeedback != DIFeedbackCode.DISuccess))
							{
                                if (globalfeedback != DIFeedbackCode.DISuccess) //DR38000260 additional error feedback
                                {
                                    SystemInfo.errorSB.Append("\nDCF compare error: " + globalfeedback.ToString());
                                }
								sysInfo.displayErrorFeedbackToUser("Not all sub index settings updated:");
							}
							break;
							#endregion DCF full table creation

						case "Display DataLog Retrieval":
							#region Display datlog retrieval
							this.progressBar1.Value = this.progressBar1.Minimum;;
							this.timer3.Enabled = true;
							this.statusBarPanel3.Text = "Waiting for you to select a save file";
							this.showUserControls();
							this.processReceviedDataLog();
							if(SystemInfo.errorSB.Length>0)
							{
								sysInfo.displayErrorFeedbackToUser("Errors seen when retrieving Data log:");
							}
							break;
							#endregion Display datlog retrieval

						case "Display FaultLog Retrieval":
							#region Display FaultLog Retrieval
							this.progressBar1.Value = this.progressBar1.Minimum;;
							this.timer3.Enabled = true;
							this.statusBarPanel3.Text = "Waiting for you to select a save file";
							this.showUserControls();
							this.processReceviedFaultLog();
							if(SystemInfo.errorSB.Length>0)
							{
								sysInfo.displayErrorFeedbackToUser("Errors seen when retrieving Fault log:");
							}
							break;
							#endregion Display FaultLog Retrieval

                        case "Submit Data Download and Retrieval":
                            #region submit data download/retrieval
                            MAIN_WINDOW.DWdataset.Tables[nodeTag.tableindex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
                			
			                if(SystemInfo.errorSB.Length>0)
			                {
				                sysInfo.displayErrorFeedbackToUser("Not all values written/read back OK");
				                this.statusBarPanel3.Text = "Not all values written/read back OK";
			                }
			                else
			                {
				                this.statusBarOverrideText = "All values written and read back OK";
			                }
			                showUserControls();
                            #endregion submit data download/retrieval
                            break;

					}

				}
				else
				{
					switch(threadName)
					{
						case "COBDataRetrieval":
							#region Comms data retrieval
							if(SystemInfo.itemCounter1<this.progressBar1.Maximum)
							{
                                this.progressBar1.Value = SystemInfo.itemCounter1;
							}
							break;
							#endregion Comms data retrieval

						case "DCF Comparison data retrieval":
							#region DCF compare Data Retrieval Thred
							if(DCFcompProgBarCounter<this.progressBar1.Maximum)
							{
								this.progressBar1.Value = DCFcompProgBarCounter;
							}
							#endregion DCF compare Data Retrieval Thred
							break;
						case "DCF compare full table creation":
							#region DCF compare Data Retrieval Thred
							if(DCFcompProgBarCounter<this.progressBar1.Maximum)
							{
								this.progressBar1.Value = DCFcompProgBarCounter;
							}
							#endregion DCF compare Data Retrieval Thred
							break;

						case "Section Data Retrieval":
                            #region retrieving a whole section of data from a devcie
							if(DriveWizard.nodeInfo.currentODItem<=this.progressBar1.Maximum)
							{
								this.progressBar1.Value = DriveWizard.nodeInfo.currentODItem;
							}
							#endregion retrieving a whole section of data from a devcie
							break;

                        case "Table Data Retrieval":
                            #region retrieving a whole table of data from a devcie
                            if (progressBarValueThreaded <= this.progressBar1.Maximum)
							{
                                this.progressBar1.Value = progressBarValueThreaded;
                            }
                            #endregion retrieving a whole table of data from a devcie
                            break;
                            
                        case "Submit Data Download and Retrieval":
                            break;

                        case "Device Panel Data Retrieval":
                        case "First Device Panel Data Retrieval":
                            //progress bar not shown on node's root
                            break;

						case "Display DataLog Retrieval":
						case "Display FaultLog Retrieval": 
						{
							this.progressBar1.Maximum = (int) this.sysInfo.CANcomms.SDOReadDomainRxDataSize;
							this.progressBar1.Value = this.sysInfo.CANcomms.SDOReadDomainRxDataPtr;
							break;
						}
					}
				}
				#endregion retrieving all data for DCf or Data monitoring
			}
			else if ( DCFfileOpenThread != null )
			{
				#region DCF fileopen thread checking
				if((DCFfileOpenThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
					timer2.Enabled = false; //kill timer
					DCFfileOpenThread = null;
					this.progressBar1.Value = this.progressBar1.Minimum;
					checkDCfFileOpenOK();
				}
				else
				{
					//increment the progress bar
				}
				#endregion
			}
			else if ( DCFdownloadToDeviceThread != null )
			{
				#region DCF downloadthread checking
				if((DCFdownloadToDeviceThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
					timer2.Enabled = false; //kill timer
					checkDownload();
					DCFdownloadToDeviceThread = null;
					this.progressBar1.Value = this.progressBar1.Minimum;
					this.showUserControls(); 
				}
				else
				{
					if(this.sysInfo.DCFnode.itemBeingRead<this.progressBar1.Maximum)
					{
						this.progressBar1.Value = this.sysInfo.DCFnode.itemBeingRead;
					}
				}
				#endregion
			}
			else if (DCFFileSaveThread != null)
			{
				#region DCF save to file thread
				if((DCFFileSaveThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
					timer2.Enabled = false; //kill timer
					if(this.globalfeedback == DIFeedbackCode.DISuccess)
					{
						this.statusBarOverrideText = "File Saved OK";
					}
					else
					{
                        //DR38000260 additional error feedback
                        SystemInfo.errorSB.Append("\nReported failure: " + globalfeedback.ToString());
						sysInfo.displayErrorFeedbackToUser("Failed to save DCF data to file");
					}
					#region if the user changed the underlying EDS file then re-open from the DCF file
					this.showUserControls();
					#endregion if the user changed the underlying EDS file then re-open from the DCF file
				}
				else
				{
					if(this.sysInfo.DCFnode.itemBeingRead<this.progressBar1.Maximum)
					{
						this.progressBar1.Value = this.sysInfo.DCFnode.itemBeingRead;
					}
				}
				#endregion DCF save to file thread
			}
		}
		private void timer3_Tick(object sender, System.EventArgs e)
		{
			DIFeedbackCode feedback;
			this.timer3.Enabled = false;
            this.PBRequestPreOP.Refresh();
			if((MAIN_WINDOW.mainWindowClosing == true) || (findingSystem == true))
			{
				return;
			}
			if(reEstablishCommsRequired == true)
			{
				reEstablishCommsRequired = false;
				this.ResetConnectedDevices();
				return;
			}
			else if(this.AllType1FormsClosed() != "") //eg stop timer when programming active
			{
				this.timer3WasEnabled = true;  //remember ??
				return; //do here to check reEstablishCommsRequired first - we don't want it reading stuff whilst we are busy with other things
			}
			else if((findingSystem == false) && (this.FaultLEDFlashRate != null))
			{
				#region emergency message catch up and retrieve latest active faults
				if ( (this.SystemPDOsScreenActive == false)
                    && (sysInfo.CANcomms.VCI.messageType == CANMessageType.SDO)
					&& (emerAL != null) 
					&& (emerAL.Count>0)	)
				{
                    #region update the emergency message table
                    for (int i = 0; i < this.emerAL.Count; i++)
                    {
                        DataRow row = emerTable.NewRow();
                        emergencyMessage emer = (emergencyMessage)this.emerAL[i];
                        row[(int)EmerCols.NodeID] = emer._nodeID;
                        row[(int)EmerCols.Message] = emer._message;
                        emerTable.Rows.Add(row);
                        row.AcceptChanges();

                        #region upddate the actiuve faults table if required and the GreenELD Flash rate value
                        try
                        {
                            for (int CANIndex = 0; CANIndex < this.sysInfo.nodes.Length; CANIndex++)
                            {
                                if ((emer._nodeID == sysInfo.nodes[CANIndex].nodeID.ToString())) //DR38000191
                                {
                                    if (
                                        (sysInfo.nodes[CANIndex].isSevconApplication()
                                        && ((SCCorpStyle.ProductRange)sysInfo.nodes[CANIndex].productRange) != SCCorpStyle.ProductRange.SEVCONDISPLAY)
                                        )
                                    {
                                        NodeFaultEntry[] activelog = new NodeFaultEntry[0];
                                        MAIN_WINDOW.appendErrorInfo = false;
                                        //feedback = this.sysInfo.nodes[CANIndex].requestLog( LogType.ActiveFaultsLog, out activelog);
                                        ODItemData activeFaultListSub = this.sysInfo.nodes[CANIndex].getODSubFromObjectType(SevconObjectType.DOMAIN_UPLOAD, SCCorpStyle.ActiveFaultListSubObject);
                                        feedback = this.sysInfo.nodes[CANIndex].readODValue(activeFaultListSub);
                                        this.sysInfo.processActivefaultLog(activeFaultListSub, out activelog, out unknownFaultIDs); //DR38000269

                                        #region retrieve unknown fault descriptors if any
                                        //DR38000269 NOTE: this should probably be on another thread along with retrieving all active fault logs
                                        // but how do we know if the DI thread is already busy/required by another user invokation?
                                        if (unknownFaultIDs.Count > 0)
                                        {
                                            ODItemData eventIDsSub = sysInfo.nodes[CANIndex].getODSubFromObjectType(SevconObjectType.EVENT_ID_DESCRIPTION, SCCorpStyle.EventIDSubObject);
                                            ODItemData eventNameSub = sysInfo.nodes[CANIndex].getODSubFromObjectType(SevconObjectType.EVENT_ID_DESCRIPTION, SCCorpStyle.EventNameSubObject);

                                            if ((eventIDsSub != null) && (eventNameSub != null))
                                            {
                                                for (int id = 0; id < unknownFaultIDs.Count; id++)
                                                {
                                                    ushort missingEventID = ((ushort)unknownFaultIDs[id]);
                                                    feedback = sysInfo.nodes[CANIndex].writeODValue(eventIDsSub, (long)missingEventID);

                                                    if (feedback == DIFeedbackCode.DISuccess)
                                                    {
                                                        feedback = sysInfo.nodes[CANIndex].readODValue(eventNameSub);

                                                        if (feedback == DIFeedbackCode.DISuccess)
                                                        {
                                                            this.sysInfo.updateEventList((ushort)missingEventID, eventNameSub.currentValueString, sysInfo.nodes[CANIndex].productCode);
                                                        }
                                                    }

                                                    if (feedback == DIFeedbackCode.DINoResponseFromController)
                                                    {
                                                        break;      // bomb out of loop if controller not responding
                                                    }
                                                }
                                            }
                                        }
                                        #endregion retrieve unknown fault descriptors if any
                                        //DR38000269 re-process log with new descriptors available
                                        this.sysInfo.processActivefaultLog(activeFaultListSub, out activelog, out unknownFaultIDs);
                                        MAIN_WINDOW.appendErrorInfo = true;
                                        if (feedback == DIFeedbackCode.DISuccess)
                                        {
                                            #region clear and then fill table = needs beter code later
                                            ActFaultsDS.Tables[CANIndex].Rows.Clear();
                                            for (int actFltPtr = 0; actFltPtr < activelog.Length; actFltPtr++)
                                            {
                                                row = ActFaultsDS.Tables[CANIndex].NewRow();
                                                row[0] = activelog[actFltPtr].description;
                                                ActFaultsDS.Tables[CANIndex].Rows.Add(row);
                                            }
                                            #endregion fill table
                                            #region set correct LED flash rate
                                            if (activelog.Length > 0)
                                            {
                                                int groupIDMask = 0x03C0;
                                                if (FaultLEDFlashRate != null)
                                                {
                                                    this.FaultLEDFlashRate[i, 0] = (int)((activelog[0].eventID & groupIDMask) >> 6);
                                                }
                                            }
                                            else
                                            {
                                                this.FaultLEDFlashRate[i, 0] = 99;  //no fualts indicator
                                            }
                                            #endregion set correct LED flash rate
                                        }
                                    }
                                    else
                                    {
                                        #region non-Sevcon App
                                        //we have some fsort of fault but we don't have a proper flash rate for it
                                        if (FaultLEDFlashRate != null)
                                        {
                                            this.FaultLEDFlashRate[CANIndex, 0] = 100; //arbitrary flag value
                                        }
                                        #endregion non-Sevcon
                                    }
                                }
                            }
                        }
                        catch { }
                        #endregion upddate the actiuve faults table if required and the GreenELD Flash rate value
                    }
                    emerAL.Clear();
                    #endregion update the emergency message table
				}
				#endregion emergency message catch up and retrieve latest active faults

                for (int CANDeviceIndex = 0; CANDeviceIndex < this.sysInfo.nodes.Length; CANDeviceIndex++)
                {  //use the screenTreeNode length - we may not be dispalying all connected CANnodes in current context

                    foreach (TreeNode tNode in this.SystemStatusTN.Nodes)
                    {
                        treeNodeTag tNodeTag = (treeNodeTag)tNode.Tag;
                        if (tNodeTag.tableindex == CANDeviceIndex)
                        {
                            //with PDO treeview filtering we have lost the 1 to 1 relationship between 
                            //the CANNodes and the treevoew representations of th eCAN nodes - so for each
                            //CNAnode we need to find the corresponigng treeview node (if it exists)
                            int treeNodeIndex = SystemStatusTN.Nodes.IndexOf(tNode);
                            #region animate device icons for operational devcies
                            bool GreenLEDON = false, NMTLEDOn = false;
                            #region determine whether the fault LED should be on
                            int faultGroup = this.FaultLEDFlashRate[CANDeviceIndex, 0];
                            if (faultGroup == 99)
                            {
                                GreenLEDON = true;  //indicates no fualts
                            }
                            else if (faultGroup == 100)
                            {
                                if ((this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex == (int)TVImages.msOffOff)
                                    || (this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex == (int)TVImages.msOnOff)
                                    || (this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex == (int)TVImages.slOffOff)
                                    || (this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex == (int)TVImages.slOnOff))
                                {
                                    GreenLEDON = true; //50 50 MS toggle - since we have no fault group
                                }
                            }
                            else
                            { //may need to flash the faullt 'LED'
                                //LED timing is on for 400ms, off for 200ms. 
                                //Between each flash sequence, LED is off for 800ms.
                                if ((this.FaultLEDFlashRate[CANDeviceIndex, 1] >= (faultGroup * 3) + 4))
                                { //handle rollover
                                    this.FaultLEDFlashRate[CANDeviceIndex, 1] = 0;
                                }
                                if (this.FaultLEDFlashRate[CANDeviceIndex, 1] < (faultGroup * 3))
                                {
                                    //not in the end off phase
                                    if (((this.FaultLEDFlashRate[CANDeviceIndex, 1]) + 1) % 3 != 0)
                                    {
                                        GreenLEDON = true;
                                    }
                                }
                                this.FaultLEDFlashRate[CANDeviceIndex, 1]++;
                            }
                            #endregion determine whether the fault LED should be on
                            #region determine whether the NMT State LED should be on
                            if ((sysInfo.nodes[CANDeviceIndex].nodeState == NodeState.Operational)
                                || ((sysInfo.nodes[CANDeviceIndex].nodeState == NodeState.PreOperational)
                                && (this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex % 2 == 0)))//NMT LED was OFF
                            {
                                NMTLEDOn = true;
                            }
                            #endregion determine whether the NMT State LED should be on
                            //now put it all together to determin new image indexes for this device 
                            if (sysInfo.nodes[CANDeviceIndex].masterStatus == true)
                            {
                                #region config icon 'LEDs
                                if (NMTLEDOn == true)
                                {
                                    if (GreenLEDON == true)
                                    {
                                        this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex = (int)TVImages.msOnOn;
                                        this.SystemStatusTN.Nodes[treeNodeIndex].SelectedImageIndex = (int)TVImages.msOnOn;
                                    }
                                    else
                                    {
                                        this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex = (int)TVImages.msOnOff;
                                        this.SystemStatusTN.Nodes[treeNodeIndex].SelectedImageIndex = (int)TVImages.msOnOff;
                                    }
                                }
                                else
                                {
                                    if (GreenLEDON == true)
                                    {
                                        this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex = (int)TVImages.msOffOn;
                                        this.SystemStatusTN.Nodes[treeNodeIndex].SelectedImageIndex = (int)TVImages.msOffOn;
                                    }
                                    else
                                    {
                                        this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex = (int)TVImages.msOffOff;
                                        this.SystemStatusTN.Nodes[treeNodeIndex].SelectedImageIndex = (int)TVImages.msOffOff;
                                    }
                                }
                                #endregion config icon 'LEDs
                            }
                            else
                            {
                                #region config icon 'LEDs
                                if (NMTLEDOn == true)
                                {
                                    if (GreenLEDON == true)
                                    {
                                        this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex = (int)TVImages.slOnOn;
                                        this.SystemStatusTN.Nodes[treeNodeIndex].SelectedImageIndex = (int)TVImages.slOnOn;
                                    }
                                    else
                                    {
                                        this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex = (int)TVImages.slOnOff;
                                        this.SystemStatusTN.Nodes[treeNodeIndex].SelectedImageIndex = (int)TVImages.slOnOff;
                                    }
                                }
                                else
                                {
                                    if (GreenLEDON == true)
                                    {
                                        this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex = (int)TVImages.slOffOn;
                                        this.SystemStatusTN.Nodes[treeNodeIndex].SelectedImageIndex = (int)TVImages.slOffOn;
                                    }
                                    else
                                    {
                                        this.SystemStatusTN.Nodes[treeNodeIndex].ImageIndex = (int)TVImages.slOffOff;
                                        this.SystemStatusTN.Nodes[treeNodeIndex].SelectedImageIndex = (int)TVImages.slOffOff;
                                    }
                                }
                                #endregion config icon 'LEDs
                            }
                            #endregion animate device icons for operational devcies
                        }
                    }
                }
				if((nodeStateChanged == true) && (this.SystemPDOsScreenActive == false))
				{
					nodeStateChanged = false;
					this.hideUserControls();
					if(MAIN_WINDOW.DCFCompareActive == true)
					{
						#region update DCF compare table
						dataRetrievalThread = new Thread(new ThreadStart( UpdateDCFCompareData )); 
						dataRetrievalThread.Name = "DCF Comparison data retrieval";
						dataRetrievalThread.IsBackground = true;
                        dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
						System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif  
						this.progressBar1.Value = this.progressBar1.Minimum;
						this.progressBar1.Maximum = this.comparisonTable.DefaultView.Count;
						this.statusBarPanel3.Text = "Retrieving values from all connected nodes...";
						previousNode = this.treeView1.SelectedNode;
						previousNode = this.currentTreenode; 
						dataRetrievalThread.Start(); 
						this.timer2.Enabled = true;
						return;
						#endregion update DCF compare table
					}
					else
					{
						#region update colouring in tables
						this.updateRowColoursArray();
						this.updateDeviceLevelOtherWindowTreenodes();
						this.updateSystemLevelOtherWindowTreenodes();
						this.showUserControls();
						#endregion update colouring in tables
					}
				}
			}
			this.timer3.Enabled = true;
		}

		private void dataMonitoringTimer_Tick(object sender, System.EventArgs e)
		{
			DIFeedbackCode feedback;
			int monitorIndex = 0; 			//limti monitoring to first ten non header' items
			int grTblRowCtr = 0;
			DataRowState currentState;
			while ((monitorIndex<MAIN_WINDOW.monitorCeiling) && (grTblRowCtr< MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].Rows.Count))
			{
				DataRow gRow = MAIN_WINDOW.DWdataset.Tables[GraphTblIndex].Rows[grTblRowCtr];
				ODItemData gRowAssocSub = (ODItemData) gRow[(int) TblCols.odSub];
				#region skip headers
				if(gRowAssocSub.subNumber == -1)
				{
					grTblRowCtr++;
					continue; //skip this one 
				}
				#endregion skip headers
				long oldVal = gRowAssocSub.currentValue;
				int devIndex = 0;
                //some items may not be on a connected node so ignore (prevents exceptions)
                if (gRowAssocSub.CANnode != null)
                {
                    #region get affected Index, sub, node ID and source CNANodeIndex
                    feedback = this.sysInfo.getNodeNumber(gRowAssocSub.CANnode.nodeID, out devIndex);
                    if (feedback != DIFeedbackCode.DISuccess)
                    {
                        grTblRowCtr++;
                        continue; //skip this one //inform user ??
                    }
                    #endregion get affected Index, sub, node ID and source CNANodeIndex

                    if (DATA_MONITOR_FRM == null)
                    { //only read if this item is not alread being read as part of graphing - prevents blocking bus
                        feedback = gRowAssocSub.CANnode.readODValue(gRowAssocSub);
                    }
                    if (feedback == DIFeedbackCode.DINoResponseFromController)
                    {
                        numConsecutiveNonResponsesWhenMonitoring++;
                        if (numConsecutiveNonResponsesWhenMonitoring >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
                        {
                            this.dataMonitoringTimer.Enabled = false;
                            sysInfo.displayErrorFeedbackToUser("Device failed to respond three consecutive times - monitoring aborted");
                            removeMonitoringListNodes(this.GraphCustList_TN);
                            return;
                        }
                    }
                    else //judetemp this isn't quite correct we need to idetify that a message was sent by DI and some sort of response was Rx'd
                    {
                        numConsecutiveNonResponsesWhenMonitoring = 0;//reset
                    }
                    if (gRowAssocSub.currentValue != oldVal)
                    {  //only do this if the value has changed since last time
                        #region locate 'source' row
                        object[] myKeys = new object[2];
                        myKeys[0] = gRow[(int)TblCols.Index].ToString();
                        myKeys[1] = gRow[(int)TblCols.sub].ToString();
                        DataRow sourceRow = MAIN_WINDOW.DWdataset.Tables[devIndex].Rows.Find(myKeys);
                        if (sourceRow == null)
                        {
                            grTblRowCtr++;
                            continue;
                        }
                        #endregion locate 'source' row
                        #region update source table
                        currentState = sourceRow.RowState; //backup before we change it here
                        MAIN_WINDOW.DWdataset.Tables[devIndex].ColumnChanged -= new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
                        this.fillTableRowColumns(gRowAssocSub, sourceRow, devIndex, false, true, sysInfo.nodes[devIndex]);
                        if (currentState == DataRowState.Unchanged)
                        {
                            sourceRow.AcceptChanges();  //do not accept changes on rows that the user has input data to
                        }
                        MAIN_WINDOW.DWdataset.Tables[devIndex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
                        #endregion update source table
                        #region update Grpahing Table
                        //get the correct monitoring 'node' 
                        foreach (nodeInfo mnNode in this.monStore.myMonNodes)
                        {
                            if (mnNode.nodeID == gRowAssocSub.CANnode.nodeID) //is this adequate?? or should I also match 0x1018 params judetemp
                            {
                                this.fillTableRowColumns(gRowAssocSub, gRow, MAIN_WINDOW.GraphTblIndex, false, true, mnNode);
                                break; //found it so leave
                            }
                        }
                        #endregion update Grpahing Table
                    }
                }
				monitorIndex++;  //increment num items being monitored
				grTblRowCtr++;
			}
		}

		/// <summary>
		/// Not currently used - requires event to be raisied by DI
		/// </summary>
		/// <param name="messageType"></param>
		/// <param name="nodeID"></param>
		/// <param name="nodeNumber"></param>
		/// <param name="newNodeState"></param>
		/// <param name="emergencyMessage"></param>
		private void systemInfo_notifyGUI(COBIDType messageType, int nodeID, int nodeNumber, NodeState newNodeState, string emergencyMessage)
		{
			if ( messageType == COBIDType.ProducerHeartBeat )
			{
				#region Heartbeat message received
				if((newNodeState == NodeState.Bootup) && (DriveWizard.SEVCONProgrammer.programmingRequestFlag == false))
				{
					//change later to account for different tab sin the Options window
					if(( selectProfile == null) && (options == null))  //restarting when finding system is OK
					{
						reEstablishCommsRequired = true;
						return;
					}
				}
				//node has changed state -> after we found system
				if(( selectProfile == null) && (options == null))  //restarting when finding system is OK
				{
					if((nodeID != 0)  && (this.sysInfo.nodes.Length>0))
					{
						this.nodeStateChangedFlags[nodeNumber] = true;
						nodeStateChanged = true;
					}
				}
				DriveWizard.SEVCONProgrammer.programmingRequestFlag  = false; //reset the programming request flag
				#endregion Heartbeat message received
				return; //B&B for fast messages coming in 
			}
			else if (( messageType == COBIDType.EmergencyWithInhibit ) 
				|| (messageType == COBIDType.EmergencyNoInhibit))
			{
				emergencyMessage emer = new emergencyMessage(nodeID, emergencyMessage);
				emerAL.Add(emer);
				return;
			}
		}

		#endregion delegates and timer elapsed methods

		#region toolbar methods
		private void layoutToolBarPictureBoxes()
		{
			int leftOffset = 2;
			int topOffset = 3;
			#region toolbar picture boxes
			//note picture boxes ar eused because they allow use to use animated icons without
			//affecting the tooltip
			this.PBRequestPreOP.Left = this.toolBar1.Buttons[0].Rectangle.Left+leftOffset;  
			this.PBRequestPreOP.Top = this.toolBar1.Buttons[0].Rectangle.Top+topOffset+ 1;  

			this.PBRequestOperational.Left = this.toolBar1.Buttons[1].Rectangle.Left+leftOffset;  
			this.PBRequestOperational.Top = this.toolBar1.Buttons[1].Rectangle.Top+topOffset+ 1;  

			this.PBGraphing.Left = this.toolBar1.Buttons[3].Rectangle.Left+leftOffset + 1;  
			this.PBGraphing.Top = this.toolBar1.Buttons[3].Rectangle.Top+topOffset + 2;  

			this.PBHideShowRO.Left = this.toolBar1.Buttons[5].Rectangle.Left+leftOffset;  
			this.PBHideShowRO.Top = this.toolBar1.Buttons[5].Rectangle.Top+topOffset;  

			this.PBExpandTNode.Left = this.toolBar1.Buttons[7].Rectangle.Left+leftOffset;  
			this.PBExpandTNode.Top = this.toolBar1.Buttons[7].Rectangle.Top+topOffset;  

			this.PBCollapseTNodes.Left = this.toolBar1.Buttons[8].Rectangle.Left+leftOffset;  
			this.PBCollapseTNodes.Top = this.toolBar1.Buttons[8].Rectangle.Top+topOffset;  

			this.pbEvas.Left = this.tbb_Evas.Rectangle.Left+leftOffset;
			this.pbEvas.Top = this.tbb_Evas.Rectangle.Top+topOffset;  

            this.pbSearch.Left = this.tbb_SearchTree.Rectangle.Left + leftOffset;
            this.pbSearch.Top = this.tbb_SearchTree.Rectangle.Top + topOffset;

            this.pbRefreshData.Left = this.tbb_RefreshData.Rectangle.Left + leftOffset;
            this.pbRefreshData.Top = this.tbb_RefreshData.Rectangle.Top + topOffset;

			#endregion toolbar picture boxes
		}

		private void ToolBar_pictureBox_MouseEnter(object sender, System.EventArgs e)
		{
			string tempStr = this.AllType1FormsClosed();
			System.Windows.Forms.PictureBox myPB  = (PictureBox) sender;
			//use if else ladder becuase switch will not allow use to use the control name
			//because it is not a constant value
			if(myPB.Name == this.PBGraphing.Name)
			{
				#region graphing icon
				this.CMenuGraphingIcon.MenuItems[0].Enabled = false;
				this.CMenuGraphingIcon.MenuItems[1].Enabled = false;
				if(
					(restartCANThread == null) 
					|| (( restartCANThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
					)
				{
					if(this.sysInfo.nodes.Length==0)
					{
						this.toolTip1.SetToolTip(this.PBGraphing, "Disabled, no connected nodes");
						return;
					}
					if (this.COB_PDO_FRM != null)
					{
						tempStr = "System PDO configuration";
					}
					if(tempStr != "")
					{
						this.toolTip1.SetToolTip(this.PBGraphing, "Disabled while " + tempStr + " is active");
						return;
					}
					if((this.GraphCustList_TN == null) ||(this.GraphCustList_TN.Nodes.Count == 0))
					{
						this.toolTip1.SetToolTip(this.PBGraphing, "Disabled, monitoring Store is empty");
					}
					else if (DATA_MONITOR_FRM != null)
					{
						this.toolTip1.SetToolTip(this.PBGraphing, "Disabled, graphing screen already open");
					}
					else
					{
						this.toolTip1.SetToolTip(this.PBGraphing, "Graph first " + MAIN_WINDOW.monitorCeiling.ToString() + " items in monitoring store"); 
						if((this.sysInfo.sevconAppIsMaster == true)
							&& (this.sysInfo.systemAccess>=SCCorpStyle.AccLevel_PreOp))
						{
							this.CMenuGraphingIcon.MenuItems[0].Enabled = true;
						}
						this.CMenuGraphingIcon.MenuItems[1].Enabled = true;
					}
				}
				else if( restartCANThread != null)
				{
					this.toolTip1.SetToolTip(this.PBGraphing, "Disabled while recovering from Windows hibernation");
				}
				#endregion graphing icon
			}
			else if((myPB.Name == this.PBRequestOperational.Name) ||(myPB.Name == this.PBRequestPreOP.Name))
			{ //mop these two up together - their conditions will always be the same
				#region request operational icon/ request pre-op
				if((restartCANThread == null) 
					|| (( restartCANThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 ))
				{
					if(this.sysInfo.nodes.Length==0)
					{
						this.toolTip1.SetToolTip(this.PBRequestOperational, "Disabled, no connected nodes");
						this.toolTip1.SetToolTip(this.PBRequestPreOP, "Disabled, no connected nodes");
						return;
					}
					if (this.COB_PDO_FRM != null)
					{
						tempStr = "System PDO configuration";
					}
					if(tempStr != "")
					{
						this.toolTip1.SetToolTip(this.PBRequestOperational, "Disabled while " + tempStr + " is active");
						this.toolTip1.SetToolTip(this.PBRequestPreOP, "Disabled while " + tempStr + " is active");
						return;
					}
					if(this.sysInfo.systemAccess>=SCCorpStyle.AccLevel_PreOp)
					{
						if(this.sysInfo.sevconAppIsMaster == true)
						{
							this.toolTip1.SetToolTip(this.PBRequestOperational, "Request Operational NMT state");
							this.toolTip1.SetToolTip(this.PBRequestPreOP, "Request pre-operational NMT state");
						}
						else
						{
							this.toolTip1.SetToolTip(this.PBRequestOperational, "Request Sevcon nodes to enter Operational NMT state (non-Sevcon Bus Master)");
							this.toolTip1.SetToolTip(this.PBRequestPreOP, "Request Sevcon nodes to enter pre-operational NMT state (non-Sevcon Bus Master)");
						}
					}
					else 
					{ //this also covers no sevcon app nodes on bus
						this.toolTip1.SetToolTip(this.PBRequestOperational, "Disabled, Insufficient access");
						this.toolTip1.SetToolTip(this.PBRequestPreOP, "Disabled, Insufficient access");
					}

				}
				else if( restartCANThread != null)
				{
					this.toolTip1.SetToolTip(this.PBRequestOperational, "Disabled while recovering from Windows hibernation");
					this.toolTip1.SetToolTip(this.PBRequestPreOP, "Disabled while recovering from Windows hibernation");
				}
				#endregion request operational icon/ request pre-op
			}
			else if(myPB.Name == this.PBHideShowRO.Name)
			{
				#region Hide/Show Read only items icon
				if(
					(restartCANThread == null) 
					|| (( restartCANThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
					)
				{
					if(this.sysInfo.nodes.Length==0)
					{
						this.toolTip1.SetToolTip(this.PBRequestOperational, "Disabled, no connected nodes");
						return;
					}
					if (this.COB_PDO_FRM != null)
					{
						tempStr = "System PDO configuration";
					}
					if(tempStr != "")
					{
						this.toolTip1.SetToolTip(this.PBHideShowRO, "Disabled while " + tempStr + " is active");
						return;
					}

					if(this.toolBar1.Buttons[6].Pushed == true)   //currently hiding RO items
					{
						this.toolTip1.SetToolTip(this.PBHideShowRO, "Show read only items");
					}
					else
					{
						this.toolTip1.SetToolTip(this.PBHideShowRO, "Hide read only items");
					}
				}
				else if( restartCANThread != null)
				{
					this.toolTip1.SetToolTip(this.PBHideShowRO, "Disabled while recovering from Windows hibernation");
				}
				#endregion Hide/Show Read only items icon
			}
			else if(myPB.Name == this.PBCollapseTNodes.Name)
			{
				#region Collapse treenodes icon
				if(
					(restartCANThread == null) 
					|| (( restartCANThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
					)
				{
					if (this.COB_PDO_FRM != null)
					{
						tempStr = "System PDO configuration";
					}
					if(tempStr != "")
					{
						this.toolTip1.SetToolTip(this.PBCollapseTNodes, "Disabled while " + tempStr + " is active");
						return;
					}
					this.toolTip1.SetToolTip(this.PBCollapseTNodes, "Collapse all tree nodes");
				}
				else if( restartCANThread != null)
				{
					this.toolTip1.SetToolTip(this.PBCollapseTNodes, "Disabled while recovering from Windows hibernation");
				}
				#endregion Collapse treenodes icon
			}
			else if(myPB.Name == this.PBExpandTNode.Name)
			{
				#region Expand Treenodes icon
				if(
					(restartCANThread == null) 
					|| (( restartCANThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
					)
				{
					if (this.COB_PDO_FRM != null)
					{
						tempStr = "System PDO configuration";
					}
					if(tempStr != "")
					{
						this.toolTip1.SetToolTip(this.PBExpandTNode, "Disabled while " + tempStr + " is active");
						return;
					}
					this.toolTip1.SetToolTip(this.PBExpandTNode, "Expand selected tree node");
				}
				else if( restartCANThread != null)
				{
					this.toolTip1.SetToolTip(this.PBExpandTNode, "Disabled while recovering from Windows hibernation");
				}
				#endregion Expand Treenodes icon
			}
			else if(myPB.Name == this.pbEvas.Name)
			{
				this.tbb_Evas.Enabled = false;
				#region force EEPromn Save enable/disable
				#region hibernation recovery
				if( restartCANThread != null)
				{
					this.toolTip1.SetToolTip(this.pbEvas, "Disabled while recovering from Windows hibernation");
					return;
				}
				#endregion hibernation recovery

				#region no connected nodes
				if(this.sysInfo.nodes.Length==0)
				{
					this.toolTip1.SetToolTip(this.pbEvas, "Disabled, no connected nodes");
					this.toolTip1.SetToolTip(this.pbEvas, "Disabled, no connected nodes");
					return;
				}
				#endregion no connected nodes

				#region type 1 form open
				if(tempStr != "")
				{
					this.toolTip1.SetToolTip(this.pbEvas, "Disabled while " + tempStr + " is active");
					this.toolTip1.SetToolTip(this.pbEvas, "Disabled while " + tempStr + " is active");
					return;
				}
				#endregion type 1 form open

				#region decide if EVAS is required
				this.toolTip1.SetToolTip(this.pbEvas, "Disabled, EEProm save not required");
				foreach(nodeInfo node in this.sysInfo.nodes)
				{
					if(node.EVASRequired == true)
					{
						this.toolTip1.SetToolTip(this.pbEvas, "Force EEPROM save on CANopen nodes");
						this.tbb_Evas.Enabled = true;
						break;
					}
				}	
				#endregion decide if EVAS is required

				#endregion force EEPromn Save enable/disable
			}
            else if (myPB.Name == this.pbSearch.Name)
            {
                this.tbb_SearchTree.Enabled = false;

                #region hibernation recovery
                if (restartCANThread != null)
                {
                    this.toolTip1.SetToolTip(this.pbSearch, "Disabled while recovering from Windows hibernation");
                    return;
                }
                #endregion hibernation recovery

                #region no connected nodes
                if (this.sysInfo.nodes.Length == 0)
                {
                    this.toolTip1.SetToolTip(this.pbSearch, "Disabled, no connected nodes");
                    return;
			}
                #endregion no connected nodes

                #region instance of this form already open
                if (SEARCH_FRM != null)
                {
                    this.toolTip1.SetToolTip(this.pbSearch, "Disabled, search form already open");
                    return;
		}
                #endregion instance of this form already open

                #region type 1 form open
                if (tempStr != "")
                {
                    this.toolTip1.SetToolTip(this.pbSearch, "Disabled while " + tempStr + " is active");
                    return;
                }
                #endregion type 1 form open

                this.toolTip1.SetToolTip(this.pbSearch, "Search tree for item");
                this.tbb_SearchTree.Enabled = true;
            }
            else if (myPB.Name == this.pbRefreshData.Name)
            {
                this.toolTip1.SetToolTip(this.pbRefreshData, "Refresh data from device");
            }
		}
		private void toolBarIcon_Click(object sender, System.EventArgs e)
		{
			DIFeedbackCode feedback;
			this.timer3.Enabled = false;
			string tempStr = this.AllType1FormsClosed();
			if (this.COB_PDO_FRM != null)
			{
				tempStr = "System PDO configuration";
			}
			if((tempStr != "") || (MAIN_WINDOW.UserInputInhibit == true))
			{
				//do this becaue swithcing eventhandler off/on casues this to enter twice 
				//- affects Hide/show RO because this is a simple toggle - equivlaent of a double click
				return;
			}
			feedback = DIFeedbackCode.DISuccess;
            if (sender.GetType().ToString() == "System.Windows.Forms.PictureBox")
            {
                System.Windows.Forms.PictureBox senderPictureBox = (System.Windows.Forms.PictureBox)sender;

                #region suspend layout and updates while we update
                #region make sure we have a selected treenode and extract its tag
                if (this.treeView1.SelectedNode == null)
                {
                    this.treeView1.SelectedNode = this.SystemStatusTN;
                    this.currentTreenode = this.SystemStatusTN;
                }
                treeNodeTag selNodeTag = (treeNodeTag)this.treeView1.SelectedNode.Tag;
                #endregion make sure we have a selected treenode and extract its tag
                #endregion suspend layout and updates while we update
                if (senderPictureBox.Name == this.PBRequestPreOP.Name)
                {
                    #region request pre-op
                    if ((this.sysInfo.nodes.Length > 0)
                        && (this.sysInfo.systemAccess >= SCCorpStyle.AccLevel_PreOp))  //scenarion of no Sevocn apps will force SystemAccess to zero?
                    {
                        this.hideUserControls();
                        this.statusBarPanel3.Text = "Requesting pre-operational state";  //do after hideUserCOntrols() to prevent it being balnked
                        this.tbb_PreOPRequest.Pushed = true;
                        #region request pre-op
                        feedback = this.sysInfo.forceSystemIntoPreOpMode();
                        if (feedback != DIFeedbackCode.DISuccess)
                        {
                            SystemInfo.errorSB.Append("Failed to enter pre-operational mode");
                        }
                        #endregion request pre-op
                        //note we do not update the datagrid etc here. 
                        //We wait until the node tells us that it has achieved the new state
                        //flag set in delegate and then everything is updated under the timer
                        this.tbb_PreOPRequest.Pushed = false;
                        if (this.SystemPDOsScreenActive == true)
                        {
                            this.updatePDOScreenNodeIcons();
                            activateCOB(this.currCOB);  //force comms controls enabled to be re-evaluated
                        }
                        this.showUserControls();
                    }
                    #endregion request pre-op
                }
                else if (senderPictureBox.Name == this.PBRequestOperational.Name)
                {
                    #region request operational
                    if ((this.sysInfo.nodes.Length > 0)
                        && (this.sysInfo.systemAccess >= SCCorpStyle.AccLevel_PreOp))
                    {
                        this.hideUserControls();
                        this.statusBarPanel3.Text = "Requesting operational state";  //do after hideUserCOntrols() to prevent it being balnked
                        this.tbb_OpRequest.Pushed = true;
                        #region request operational
                        feedback = this.sysInfo.releaseSystemFromPreOpMode();
                        if (feedback != DIFeedbackCode.DISuccess)
                        {
                            SystemInfo.errorSB.Append("\nFailed to enter Operational mode");
                        }
                        #endregion request operational
                        //note we do not update the datagrid etc here. 
                        //We wait until the node tells us that it has achieved the new state
                        //flag set in delegate and then everything is updated under the timer
                        this.tbb_OpRequest.Pushed = false;
                        if (this.SystemPDOsScreenActive == true)
                        {
                            this.updatePDOScreenNodeIcons();
                            activateCOB(this.currCOB);  //force comms controls enabled to be re-evaluated
                        }
                        this.showUserControls();
                    }
                    #endregion request operational
                }
                else if (senderPictureBox.Name == this.PBGraphing.Name)
                {
                    #region display graphing screen context menu
                    if ((this.GraphCustList_TN.Nodes.Count > 0) && (DATA_MONITOR_FRM == null))
                    {
                        updateGraphcontextMenu();
                        Point pt = new Point(1, PBGraphing.Height);
                        this.PBGraphing.ContextMenu.Show(this.PBGraphing, pt);
                    }
                    #endregion display graphing screen
                }
                else if (senderPictureBox.Name == this.PBHideShowRO.Name)
                {
                    #region hide/show read only items
                    this.hideUserControls();
                    noteCurrentSelectedNode();

                    if (this.toolBar1.Buttons[6].Pushed == true)   //currently hiding RO items
                    {
                        #region show read only items
                        this.statusBarPanel3.Text = "Displaying read only items";
                        showReadOnlyNodes();
                        #endregion show read only items
                    }
                    else
                    {
                        #region hide read only items
                        this.statusBarPanel3.Text = "Hiding Read only items. Please wait";  //do after hideUserCOntrols() to prevent it being balnked
                        #region update icon image
                        try
                        {
                            Bitmap myimage = new Bitmap(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\hideRO.png");
                            this.PBHideShowRO.Image = myimage;
                        }
                        catch
                        {
#if DEBUG
                            SystemInfo.errorSB.Append("Missing file: " + MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\hideRO.png");
#endif
                        }
                        #endregion update icon image
                        #region save treenodes to backup
                        GraphBackupTN = (TreeNode)this.GraphCustList_TN.Clone();
                        this.SystemBackupTN = (TreeNode)this.SystemStatusTN.Clone();
                        this.DCFBackUpTN = (TreeNode)this.DCFCustList_TN.Clone();
                        #endregion save treenodes to backup
                        hideReadOnlyTreeNodes();
                        this.toolBar1.Buttons[6].Pushed = true;  //since we ar enow hiding read only items
                        #endregion hide read only items
                    }
                    //force sleected treenode to be a node that we know cannot have been hidden
                    //Also needs doing for showing read only items sionce we are restoring from clones
                    reshowCurrentSelectedNode();
                    this.showUserControls();
                    #endregion hide/show read only items
                }
                else if (senderPictureBox.Name == this.PBExpandTNode.Name)
                {
                    #region expand all nodes
                    //hiding user controls - calls begin Update and endUpdate - these improve performance
                    // by inhibiting screen redraw until all visible Tree nodes are defiend - otherwise
                    //very slow screen update
                    this.hideUserControls();
                    this.statusBarPanel3.Text = "Updating Table";  //do after hideUserCOntrols() to prevent it being balnked
                    this.treeView1.SelectedNode.Expand();
                    expandChildNodes(this.treeView1.SelectedNode);
                    this.showUserControls();
                    #endregion expand all nodes
                }
                else if (senderPictureBox.Name == this.PBCollapseTNodes.Name)
                {
                    #region collapse all tree nodes
                    //hideUSerCOntrol etc not needed for collapse
                    this.treeView1.CollapseAll();

                    //DR38000264 after collapsing tree, select the root node & update screen with system info
                    currentTreenode = SystemStatusTN;
                    this.handleTreeNodeSelection(currentTreenode, false);
                    #endregion collapse all tree nodes
                }
                else if (senderPictureBox.Name == this.pbEvas.Name)
                {
                    #region EEPROM save
                    if (this.tbb_Evas.Enabled == true)
                    {
                        foreach (nodeInfo node in this.sysInfo.nodes)
                        {
                            if (node.EVASRequired == true)
                            {
                                node.saveCommunicationParameters();
                            }
                        }
                    }
                    #endregion EEPROM save
                }
                else if (senderPictureBox.Name == this.pbSearch.Name)
                {
                    #region Search tree
                    if (this.tbb_SearchTree.Enabled == true)
                    {
                        openSearchForm();
                    }
                    #endregion Search tree
                }
                else if (senderPictureBox.Name == this.pbRefreshData.Name)
                {
                    if (this.tbb_RefreshData.Enabled == true)
                    {
                        this.refreshDataMI_Click(sender, e);
                    }
                }
            }
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("");
			}
			this.timer3.Enabled = true;
		}
		#endregion toolbar methods

		#region hide/show treenodes method
		void showReadOnlyNodes()
		{
			#region update icon image
			try
			{
				Bitmap myimage = new Bitmap (MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\seeRO.png");
				this.PBHideShowRO.Image = myimage;
			}
			catch
			{
#if DEBUG 
				SystemInfo.errorSB.Append("Missing file: " + MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\seeRO.png");
#endif
			}
			#endregion update icon image
			#region restore all hidden nodes from backup
			this.treeView1.Nodes.Clear();
			this.SystemStatusTN = (TreeNode) this.SystemBackupTN.Clone();
			this.treeView1.Nodes.Add(SystemStatusTN);
			this.GraphCustList_TN = (TreeNode) this.GraphBackupTN.Clone();
			this.treeView1.Nodes.Add(GraphCustList_TN);
			this.DCFCustList_TN = (TreeNode) this.DCFBackUpTN.Clone();
			this.treeView1.Nodes.Add(DCFCustList_TN);
			this.toolBar1.Buttons[6].Pushed = false;
			#endregion restore all hidden nodes from backup
		}
		private void hideReadOnlyTreeNodes()
		{
			for(int tblIndex = 0;tblIndex<this.sysInfo.nodes.Length;tblIndex++)
			{
				if(this.sysInfo.nodes[tblIndex].manufacturer != Manufacturer.UNKNOWN)
				{
					bool nodeInPreOp = false;
					if(this.sysInfo.nodes[tblIndex].nodeState == NodeState.PreOperational)
					{
						nodeInPreOp = true;
					}
					//first do a single pass to hide any ODitem nodes that 'contain' only
					//RO subs. This massivley speeds up hiding RO nodes because we only need to 
					//check OD Item nodes once
					hideReadOnlyTreeNodesForCANNode(this.SystemStatusTN.Nodes[tblIndex], tblIndex, nodeInPreOp);
					//So allwe can do is keep re-callling it until we get a 'clean' pass
					//ie one where we removed no nodes.
					do
					{
						nodesRemovedFlag = false;  //set false before entry
						//if we remove any node sin this pass then it will be set to true
						removeRedundantSecionNodes(this.SystemStatusTN.Nodes[tblIndex], tblIndex);
					}
					while (nodesRemovedFlag == true);  //chekc at end set inside method
				}
			}
		}

		internal void hideReadOnlyTreeNodesForCANNode(TreeNode TopNode, int tableIndex, bool isInPreOp)
		{//Recursive find 
			//two seperate counters needed because removeing nodes changes the collection count
			int nodeCount = TopNode.Nodes.Count;
			for(int nodePtr =0;nodePtr<nodeCount;nodePtr++)
			{ 
				if(TopNode.Nodes[nodePtr].Nodes.Count > 0) 
				{
					hideReadOnlyTreeNodesForCANNode(TopNode.Nodes[nodePtr], tableIndex, isInPreOp);  //check next level down
				}
				else //lowest level reached
				{
					bool OKToHideNode = true;
					string myStr = TopNode.Nodes[nodePtr].Text;
					treeNodeTag senderNodeTag = (treeNodeTag) TopNode.Nodes[nodePtr].Tag;
					if(senderNodeTag.assocSub != null)  //below this we know we can remove it if it is now the lowest level
					{
						bool indFound = false;
						byte nodeAccessLvl = this.sysInfo.nodes[tableIndex].accessLevel;
						foreach(DataRow row in MAIN_WINDOW.DWdataset.Tables[tableIndex].Rows)
						{
							#region only hide a node if all assoc subs are read only
							string accessType = row[(int) TblCols.accessType].ToString();
							byte objAccessLevel  = System.Convert.ToByte(row[(int) TblCols.accessLevel]);
							ODItemData rowAssocSub = (ODItemData) row[(int) TblCols.odSub];
							if(rowAssocSub.indexNumber == senderNodeTag.assocSub.indexNumber)
							{
								indFound = true;
								if(isInPreOp == true)
								{
									#region if read only row then remove the tree node
									if( (nodeAccessLvl>= objAccessLevel)
										&& ((accessType == "ReadWrite")
										|| (accessType == "ReadReadWrite")
										|| (accessType == "ReadWriteWrite")
										|| (accessType == "ReadReadWriteInPreOp")
										|| (accessType == "ReadWriteInPreOp")
										|| (accessType == "ReadWriteWriteInPreOp")
										|| (accessType == "WriteOnlyInPreOp")
										|| (accessType == "WriteOnly"))) 
									{
										OKToHideNode = false; //one or more items are read write
										break;
									}
									#endregion if read only row then remove the tree node
								}
								else  //not in pre-op
								{
									#region if read only row then remove the tree node
									if(( nodeAccessLvl>= objAccessLevel)
										&& ((accessType == "ReadWrite")
										|| (accessType == "ReadReadWrite")
										|| (accessType == "ReadWriteWrite")
										|| (accessType == "WriteOnly")))
									{
										OKToHideNode = false; //one or more items are read write
										break;
									}
									#endregion if read only row then remove the tree node
								}
							}
							else if(indFound == true)
							{  //we know rows are added in blocks of indexNo so this
								//must be start of next block - get out to improve performance
								break;
							}
							#endregion only hide a node if all assoc subs are read only
						}
					}
					else if((this.treeNodeIsDeviceLevelSpecialNode(TopNode.Nodes[nodePtr]) == true)
						|| (this.treeNodeIsSystemLevelSpecialNode(TopNode.Nodes[nodePtr]) == true)
						|| (TopNode.Nodes[nodePtr].Text == this.LogsTNText))
					{
						OKToHideNode = false; //node with underlying new screen - do not remove 
					}
					if(OKToHideNode == true)
					{
						TopNode.Nodes[nodePtr].Remove();  //nodePtr will be in right place next time because we removed an item from the collection
						nodeCount--;
						nodePtr--;
					}
				}
			}
			return;   //all nodes in this pass either passed OK or removed 
		}
		#endregion

		#region display child windows methods
		private void setupAndDisplayChildWindow(Form myform)
		{
			myform.Owner = this;
			myform.ForeColor = SCCorpStyle.SCForeColour;
#if DEMO_MODE
			try
			{
				myform.Cursor = new Cursor(GetType(), MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\cursors\BIGARROW.CUR");
			}
			catch
			{
				myform.Cursor = Cursors.Arrow;
			}
#endif
			myform.Location = this.PointToScreen(this.ClientRectangle.Location);
			myform.Top += this.toolBar1.Height;

			myform.Show();
		}
		private void showOptionsWindow()
		{
			MAIN_WINDOW.UserInputInhibit = true;
			this.progressBar1.Visible = false;
			this.options = new DriveWizard.OPTIONS_WINDOW(sysInfo, this.toolBar1, this.statusBar1);
			this.options.Owner = this;
			this.options.Location = this.PointToScreen(this.ClientRectangle.Location);
			this.options.Top += this.toolBar1.Height;
			this.options.Closed +=new EventHandler(options_Closed);
			this.options.ForeColor = SCCorpStyle.SCForeColour;
#if DEMO_MODE
			this.options.Cursor = this.Cursor;
#endif
			this.options.Show();
		}

		private void showSplashScreen()
		{
			MAIN_WINDOW.UserInputInhibit = true;
			this.splashscreen = new Form3();
			this.splashscreen.Owner = this;
			this.splashscreen.Closed +=new EventHandler(splashscreen_Closed); 
#if DEMO_MODE
			this.splashscreen.Cursor = this.Cursor;
#endif
			this.splashscreen.Show();
		}
		private void showProfileSelectWindow()
		{
			//find out how many Truck profiles are available
			//the file name minus the extension isthe profile name
			MAIN_WINDOW.UserInputInhibit = true;
			this.progressBar1.Visible = false;
			this.selectProfile = new SELECT_PROFILE(sysInfo,this.statusBar1,  this.toolBar1);
			this.selectProfile.Owner = this;
			this.selectProfile.Location = this.PointToScreen(this.ClientRectangle.Location);
			this.selectProfile.Top += this.toolBar1.Height;
			this.selectProfile.Closed +=new EventHandler(selectProfile_Closed);
			this.selectProfile.ForeColor = SCCorpStyle.SCForeColour;
#if DEMO_MODE
			this.selectProfile.Cursor = this.Cursor;
#endif
			this.selectProfile.Show();//.ShowDialog(this);
		}
		#endregion display child windows methods

		#region minor methods
		private void MAIN_WINDOW_Resize(object sender, System.EventArgs e)
		{
			layoutToolBarPictureBoxes();

			#region resize help screens
			if( helpAbout != null)
			{
				helpAbout.Height = (int)this.Height/2;
				helpAbout.Width = (int)this.Height/2; 
				helpAbout.Invalidate();
			}
			else if (helpContact != null)
			{
				helpContact.Height = System.Convert.ToInt32(this.Height/2);
				helpContact.Width = System.Convert.ToInt32(this.Height/2);
				helpContact.Invalidate();
			}
			#endregion resize help screens
		}

		private void OptionsMI_Click(object sender, System.EventArgs e)
		{
			if(this.options == null)
			{
				this.confirmCloseOtherForms();
				if( this.OwnedForms.Length == 0)
				{
					this.hideUserControls();
					showOptionsWindow();
				}
			}
		}

        /// <summary>
        /// Creates an instance of SearchForm, adds a search listener delegate and disables
        /// the menu and toolbar push button to prevent multiple instances of the form.
        /// </summary>
        private void openSearchForm()
        {
            this.tbb_SearchTree.Enabled = false;
            this.FindMI.Enabled = false;
            searchInProgress = false;
            this.SEARCH_FRM = new SearchForm();
            this.SEARCH_FRM.Owner = this;
            this.SEARCH_FRM.Closed += new EventHandler(SEARCH_FRM_Closed);
            ((SearchForm)this.SEARCH_FRM).addSearchChangeListener(new SearchChangeListener(searchChangeListener));
            this.SEARCH_FRM.Show();
        }

		internal void removeRedundantSecionNodes(TreeNode TopNode, int tableIndex)
		{
			ArrayList nodesToGo = new ArrayList();
			foreach(TreeNode node in TopNode.Nodes)
			{
				if(node.Nodes.Count>0)
				{
					removeRedundantSecionNodes(node, tableIndex);  //check next level down
				}
				else
				{
					treeNodeTag senderNodeTag = (treeNodeTag) node.Tag;
					if(tableIndex <this.sysInfo.nodes.Length)
					{//all secondary screen needed because this is used in hide/show read only items too
						if((senderNodeTag.assocSub == null) 
							&& (this.treeNodeIsDeviceLevelSpecialNode(node) == false)
							&& (this.treeNodeIsSystemLevelSpecialNode(node) == false))
						{
							nodesToGo.Add(node);
							nodesRemovedFlag = true;
						}
					}
					else  //custom list remove any redundant secondary windows eg Fault log too
					{
                        if (senderNodeTag.assocSub == null)
                        {
                            nodesToGo.Add(node);
                            nodesRemovedFlag = true;
                        }
					}
				}
			}
			foreach(TreeNode nodeToGo in nodesToGo)
			{
				TopNode.Nodes.Remove(nodeToGo);
			}

		}

		internal void removeSpecialDevcieLevelNodesFromCustomLists(TreeNode TopNode)
		{
			//we  need two seperate counters here because we are removing items that affect the overall 
			//loop count
			int nodeCount = TopNode.Nodes.Count;
			for(int nodePtr =0;nodePtr<nodeCount;nodePtr++)
			{
				if(TopNode.Nodes[nodePtr].Nodes.Count>0)
				{
					removeSpecialDevcieLevelNodesFromCustomLists(TopNode.Nodes[nodePtr]);  //check next level down
				}
				else
				{
					treeNodeTag senderNodeTag = (treeNodeTag) TopNode.Nodes[nodePtr].Tag;
					if(this.treeNodeIsDeviceLevelSpecialNode(TopNode.Nodes[nodePtr]) == true)
					{
						TopNode.Nodes[nodePtr].Remove();  //remove the topmost redundant node - which remove sits children as well
						nodeCount--;
						nodePtr--;
						nodesRemovedFlag = true;
					}
				}
			}
		}

		private void confirmCloseOtherForms()
		{
			if(this.OwnedForms.Length>0)
			{
				DialogResult result = Message.Show(this,"Click OK to close other DriveWizard windows",
					"Close other windows confirmation", 
					MessageBoxButtons.OKCancel,
					MessageBoxIcon.Question, 
					MessageBoxDefaultButton.Button1);
				if(result == DialogResult.OK)
				{
					for(int i = 0;i<this.OwnedForms.Length;i++)
					{
						this.OwnedForms[i].Close();
					}
					//do twice does not always close all owned forms the first time
					for(int i = 0;i<this.OwnedForms.Length;i++)
					{
						this.OwnedForms[i].Dispose();
					}
					if(this.dataMonitoringTimer.Enabled == true)
					{
						this.monitorTimerWasRunning = true; //so we know to swithc it back on again once the new window is closed again
						this.dataMonitoringTimer.Enabled = false;
					}
				}
			}
		}
		private void formatColourPanels()
		{
			readOnlyPanel.BackColor = SCCorpStyle.readOnly;
			readOnlyLabel.Font = new System.Drawing.Font("Arial", 8F);
			readWritePanel.BackColor = SCCorpStyle.readWrite;
			writeOnlyLabel.Font = new System.Drawing.Font("Arial", 8F);
			writeOnlyPanel.BackColor = SCCorpStyle.writeOnly;
			readWriteLabel.Font = new System.Drawing.Font("Arial", 8F);
			this.writeInPreOpPanel.BackColor = SCCorpStyle.writeOnlyPlusPreOP;
			readwritepreopLabel.Font = new System.Drawing.Font("Arial", 8F);
			this.readWriteInPreopPanel.BackColor = SCCorpStyle.readWriteInPreOp;
			writeInPreOpLabel.Font = new System.Drawing.Font("Arial", 8F);
		}
		private void forceDataGridResize(DataGrid dg)
		{
			int VScrollBarwidth = 0;
			foreach( Control c in dg.Controls ) 
			{
				if ( c.GetType().Equals( typeof( VScrollBar ) ) ) 
				{
					if(c.Visible == true)
					{
						VScrollBarwidth = c.Width;  //remove width of scroll bar from overall calc
					}
					break;
				}
			}
			try
			{
				handleResizeDataGrid(dg, VScrollBarwidth);
			}
			catch(Exception e4)
			{
				SystemInfo.errorSB.Append("handleResizeDataGrid() casued exception: " + e4.Message + " " + e4.InnerException);
			}

		}
		private bool treeNodeIsDeviceLevelSpecialNode(TreeNode testNode)
		{
			if((testNode.Text == this.SelfCharMotor1Text)
				//				||(testNode.Text == this.LogsTNText)
				|| ( testNode.Text == this.SelfCharMotor2Text)
				|| (testNode.Text == this.programmingTNText)
				|| (testNode.Text == this.SystemLogTNText)
				|| (testNode.Text == this.FaultLogTNText)
				|| (testNode.Text == this.CountersLogTNText)
				|| (testNode.Text == this.OpLogTNText)
				|| (testNode.Text == this.nodeCOBShortCutsText)
				|| ( testNode.Text == this.CountersLogTNText))
			{
				return true;
			}
			return false;
		}
		private bool treeNodeIsSystemLevelSpecialNode(TreeNode testNode)
		{
			//Use text not fullPAth since htis allows us to 
			//check nodes that hav eno yet been added to the treeView

			if((testNode.Text == this.COBandPDOsTN.Text)
				||(testNode.Text == this.CANBUsConfigTN.Text))
			{
				return true;
			}
			return false;
		}

		private bool treeNodeIsCustomListNode(TreeNode testNode)
		{
			if((testNode.Text == this.GraphCustList_TN.Text)
				||(testNode.Text == this.DCFCustList_TN.Text))
			{
				return true;
			}
			return false;
		}
		/// <summary>
		/// Converts a time in CNAOpen TIME format into US date and time format
		/// </summary>
		/// <param name="CANopenTime"></param>
		private string getTime(long CANopenTime, bool absTime)
		{
			string dateAndTime = "";
			//CAN Opem Time of Day and Time differnece format is:
			//Component ms is the time in milliseconds after midnight. Component days is the number of days since
			//January 1, 1984.
			//STRUCT OF
			//UNSIGNED28 ms,
			//VOID4 reserved,
			//UNSIGNED16 days
			//TIME_OF_DAY 
			CANopenTime = CANopenTime & 0x0000ffffffffffff;  //trim to 48 bits
			long CANopenTimeCopy = CANopenTime;
			uint CANopenms = (uint) (CANopenTimeCopy >> 20); 
			//now convert the ms to hours minutes, seconds and milliseconds
			int msInOneHour = 1000* 60 * 60;
			int hours = (int) CANopenms/(msInOneHour);
			int remainder = (int) CANopenms%(msInOneHour);
			int mins = remainder /60000;
			remainder = remainder% 60000;
			int secs = remainder/1000;
			int milliseconds = remainder%1000;
			//next convert the days to years, months 
			CANopenTimeCopy = CANopenTime;
			ushort CANopenDays = (ushort) (CANopenTimeCopy & 0xffff);
			
			//if this is a time idffernece then we cannot calculate months an dyears
			//since month and year length vary
			if(absTime == false)
			{
				dateAndTime = CANopenDays.ToString() + "d " 
					+ hours.ToString().PadLeft(2,'0') + ":" 
					+ mins.ToString().PadLeft(2,'0')  + ":" 
					+ secs.ToString().PadLeft(2,'0') + ":" 
					+ milliseconds.ToString().PadLeft(3,'0');
			}
			else
			{
				//this is an absolute date and time since January 1, 1984
				//first extract the years 
				int yearNo = 1984;
				bool yearsAdded = false;
				int tempDays = CANopenDays;
				while(yearsAdded == false)
				{
					if ((DateTime.IsLeapYear(yearNo) == true) && (tempDays>=366))
					{
						tempDays -= 366;
						yearNo++;
					}
					else if ((DateTime.IsLeapYear(yearNo) == false) && (tempDays>=365))
					{
						tempDays -=365;
						yearNo++;
					}
					else
					{
						yearsAdded = true;
					}
				}
				//tempDays nos contains left over days 
				bool monthsAdded = false;
				int monthNo = 1;  //1 is January
				int daysInMonth = 0;
				while(monthsAdded == false)
				{
					daysInMonth = DateTime.DaysInMonth(yearNo,monthNo);
					if(tempDays >= daysInMonth)
					{
						tempDays -= daysInMonth;
						monthNo++;
					}
					else
					{
						monthsAdded = true;
					}
				}
				//OK so now we have an absolute date - lets pass it back
				dateAndTime = 
					monthNo.ToString().PadLeft(2,'0') + ":"  //US format months first
					+ tempDays.ToString().PadLeft(2,'0') + ":" 
					+ yearNo.ToString()  + ", "
					+ hours.ToString().PadLeft(2,'0') + ":" 
					+ mins.ToString().PadLeft(2,'0')  + ":" 
					+ secs.ToString().PadLeft(2,'0') + ":" 
					+ milliseconds.ToString().PadLeft(3,'0');
			}
			return dateAndTime;
		}

		private int getSub(string subStr)
		{
			if((subStr == "-1") || (subStr == ""))
			{
				return -1;
			}
			else
			{
				return (System.Convert.ToInt32(subStr, 16));
			}
		}
		private void copyOdSubValueToCustomList(ODItemData srceODSub, ODItemData destODSub)
		{
			switch((CANopenDataType) srceODSub.dataType)
			{
					#region switch dataType
				case CANopenDataType.VISIBLE_STRING:
				case CANopenDataType.UNICODE_STRING:
				case CANopenDataType.OCTET_STRING:
					#region string
					destODSub.currentValueString = srceODSub.currentValueString;
					#endregion string
					break;
								
				case CANopenDataType.UNSIGNED16:
				case CANopenDataType.UNSIGNED24:
				case CANopenDataType.UNSIGNED32:
				case CANopenDataType.UNSIGNED40:
				case CANopenDataType.UNSIGNED48:
				case CANopenDataType.UNSIGNED56:
				case CANopenDataType.UNSIGNED64:
				case CANopenDataType.UNSIGNED8:
				case CANopenDataType.INTEGER16:
				case CANopenDataType.INTEGER24:
				case CANopenDataType.INTEGER32:
				case CANopenDataType.INTEGER40:
				case CANopenDataType.INTEGER48:
				case CANopenDataType.INTEGER56:
				case CANopenDataType.INTEGER64:
				case CANopenDataType.INTEGER8:
				case CANopenDataType.BOOLEAN:
					#region numerical
					destODSub.currentValue =srceODSub.currentValue;
					#endregion numerical
					break;

				case CANopenDataType.REAL32:
					#region REAL 32
					destODSub.real32.currentValue = srceODSub.real32.currentValue;
					#endregion REAL 32
					break;

				case CANopenDataType.REAL64:
					#region REAL 64
					destODSub.real64.currentValue = srceODSub.real64.currentValue;
					#endregion REAL 64
					break;
																
				case CANopenDataType.DOMAIN:
					destODSub.currentValueDomain =  srceODSub.currentValueDomain;
					break;

				default: 
				case CANopenDataType.TIME_DIFFERENCE:
				case CANopenDataType.TIME_OF_DAY:
				case CANopenDataType.RESERVED1:
				case CANopenDataType.RESERVED2:
				case CANopenDataType.RESERVED3:
				case CANopenDataType.RESERVED4:
				case CANopenDataType.RESERVED5:
				case CANopenDataType.RESERVED6:
					break;
					#endregion switch dataType 
			}

		}

        /// <summary>
        /// Converts the complete tree node path of the currently selected node in treeView1 and stores
        /// it as a string in HideShowRONodeFullPath.
        /// </summary>
        private void noteCurrentSelectedNode()
        {
            // Create a new array list to hold the tree node path of the currently selected node.
            HideShowRONodeFullPath = new ArrayList();
            TreeNode rootNode = treeView1.SelectedNode;

            // Progressively head up the tree, converting the name of each node to a string.
            // This is required because treeView1 will be completely rebuild and all pointers etc.
            // rendered useless. The tree node path is converted to a string to try and rematch later.
            // Note:HideShowRONodeFullPath stores the path in reverse order.
            if (rootNode != null)
            {
                while ((rootNode.Parent != null) && (rootNode.Text != ""))
                {
                    HideShowRONodeFullPath.Add(rootNode.Text);
                    rootNode = rootNode.Parent;
                }

                if (rootNode.Text != "")
                {
                    HideShowRONodeFullPath.Add(rootNode.Text);
                }
            }
        }

        /// <summary>
        /// Performs a tree node selection to re-show treeView1 at the node closest to the tree node path
        /// stored in HideShowRONodeFullPath.
        /// </summary>
        private void reshowCurrentSelectedNode()
        {
            TreeNode rootNode = null;
            TreeNode nextNode = null;

            // Can only reshow a previous node if one's been stored.
            if (HideShowRONodeFullPath.Count > 0)
            {
                #region find the root node on treeView1 ie "system", "Monitoring store" or "DCF store"
                for (int i = 0; i < treeView1.Nodes.Count; i++)
                {
                    // Note:HideShowRONodeFullPath stores the path in reverse order.
                    if (treeView1.Nodes[i].Text.Equals(HideShowRONodeFullPath[HideShowRONodeFullPath.Count - 1].ToString()))
                    {
                        rootNode = treeView1.Nodes[i];
                        break;  // found it!
			}
                }
                #endregion find the root node on treeView1 ie "system", "Monitoring store" or "DCF store"

                // If there's more nodes to drill down into...
                if (HideShowRONodeFullPath.Count > 1)
                {
                    #region Progressively find lower nodes until one cannot be found, or the complete path is found
                    for (int path = HideShowRONodeFullPath.Count - 2; path >= 0; path--)
                    {
                        nextNode = null;    // initialise to not found

                        // Check all the nodes on this level for a path name match.
                        // NOTE: DON'T drill down, check only at this level as it is an EXACT match 
                        // we're looking for.
                        foreach (TreeNode node in rootNode.Nodes)
                        {
                            if (node.Text.Equals(HideShowRONodeFullPath[path].ToString()) == true)
                            {
                                nextNode = node;
                                break;  // found it!
                            }
                        }

                        // If we've found it, update rootNode then this will be searched in the next loop iteration.
                        if (nextNode != null)
                        {
                            rootNode = nextNode;
                        }
                        // Otherwise, it doesn't exist anymore so quit & keep rootNode as the last match found.
                        else
                        {
                            break;  // best match we can get is in rootNode (last matched node)
                        }
                    }
                    #endregion Progressively find lower nodes until one cannot be found, or the complete path is found
                }
            }

            #region show the best matched node & if none, redisplay system tree and collapse it
            if (rootNode != null)
            {
                handleTreeNodeSelection(rootNode, true);
            }
            else
            {
                handleTreeNodeSelection(this.SystemStatusTN, true);
                treeView1.SelectedNode.Collapse();
		}
            #endregion show the best matched node & if none, redisplay system tree and collapse it
        }

		#endregion minor methods

		#region owned form closed event handlers
		private void FAULT_LOG_FRM_Closed(object sender, EventArgs e)
		{
			this.FAULT_LOG_FRM = null;
		}

		private void SYSTEM_LOG_FRM_Closed(object sender, EventArgs e)
		{
			this.SYSTEM_LOG_FRM = null;
		}

		private void OP_LOGS_FRM_Closed(object sender, EventArgs e)
		{
			this.OP_LOGS_FRM = null;
		}

		private void COUNTERS_FRM_Closed(object sender, EventArgs e)
		{
			this.COUNTERS_FRM = null;
		}

		private void SELF_CHAR_FRM_Closed(object sender, EventArgs e)
		{
			this.SELF_CHAR_FRM = null;
			if (MAIN_WINDOW.mainWindowClosing == true)
			{
				return;
			}
			MAIN_WINDOW.UserInputInhibit = false;
			if (MAIN_WINDOW.reEstablishCommsRequired == true)
			{
				MAIN_WINDOW.reEstablishCommsRequired = false;
				ResetConnectedDevices();
			}
			else
			{
				this.showUserControls();
			}
		}
		private void ResetConnectedDevices()
		{
			this.monitorTimerWasRunning = false;
			this.timer2.Enabled = false;
			this.timer3.Enabled = false;
			this.dataMonitoringTimer.Enabled = false; //B&B
			this.hideUserControls(); 
			this.progressBar1.Visible = false; //set to true in hideUserControls();
			this.statusBarPanel3.Text = "One or more connected devices has changed. Re-connection required";
			this.statusBar1.Panels[0].Text = "not connected";
			try
			{
				Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\notConnected.ico");
				this.statusBar1.Panels[0].Icon = icon;
			}
			catch 
			{
#if DEBUG
				SystemInfo.errorSB.Append("Missing file: " + MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\notConnected.ico");
#endif
			}			
			statusBar1.Panels[1].Text = "SEVCON: Not logged in";
			try
			{
				Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\notloggedIn.ico");
				this.statusBar1.Panels[1].Icon = icon;
			}
			catch 
			{
#if DEBUG
				SystemInfo.errorSB.Append("Missing file: " + MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\notloggedIn.ico");
#endif
			}
			if(DWConfigFile.vehicleprofiles != null && DWConfigFile.vehicleprofiles.Count>0)
			{
				showProfileSelectWindow();//we have one or more vehicle profiles for user to choose form
				this.Refresh();  //forces proper removal of under lying tree view bits
			}
			else
			{
				this.confirmCloseOtherForms();
				if(this.OwnedForms.Length == 0)
				{
					showOptionsWindow();// no profiles set up - so send user to setup screen
					this.Refresh();  //forces proper removal of under lying tree view bits
				}
			}
		}
		private void CAN_BUS_CONFIG_Closed(object sender, EventArgs e)
		{
			this.CAN_BUS_CONFIG = null;
			if (MAIN_WINDOW.mainWindowClosing == true)
			{
				return;
			}
			MAIN_WINDOW.UserInputInhibit = false;
			if (MAIN_WINDOW.reEstablishCommsRequired == true)
			{
				MAIN_WINDOW.reEstablishCommsRequired = false;
				ResetConnectedDevices();
			}
			else
			{
				this.showUserControls();
			}
		}

		private void COB_PDO_FRM_Closed(object sender, EventArgs e)
		{
			this.COB_PDO_FRM = null;
			if (MAIN_WINDOW.mainWindowClosing == true)
			{
				return;
			}
			MAIN_WINDOW.UserInputInhibit = false;
			if (MAIN_WINDOW.reEstablishCommsRequired == true)
			{
				MAIN_WINDOW.reEstablishCommsRequired = false;
				ResetConnectedDevices();
			}
			else
			{
				this.showUserControls();
			}
		}
		private void DATA_MON_WINDOW_Closed(object sender, EventArgs e)
		{
			this.DATA_MONITOR_FRM = null;
		}

        /// <summary>
        /// Called when SearchForm is closed, this re-enables the search menu item and
        /// toolbar button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SEARCH_FRM_Closed(object sender, EventArgs e)
		{
            this.SEARCH_FRM = null;
            this.tbb_SearchTree.Enabled = true;
            this.FindMI.Enabled = true;
            searchInProgress = false;
		}
        
		private void PROG_DEVICE_FRM_Closed(object sender, EventArgs e)
		{
			this.PROG_DEVICE_FRM = null;
			if (MAIN_WINDOW.mainWindowClosing == true)
			{
				return;
			}
			MAIN_WINDOW.UserInputInhibit = false;
			if (MAIN_WINDOW.reEstablishCommsRequired == true)
			{
				MAIN_WINDOW.reEstablishCommsRequired = false;
				ResetConnectedDevices();
			}
			else
			{
				this.showUserControls();
			}
		}
		private void selectProfile_Closed(object sender, EventArgs e)
		{
			this.selectProfile = null;
			if (MAIN_WINDOW.mainWindowClosing == true)
			{
				return;
			}
			if(SELECT_PROFILE.editProfilesRequired == true)
			{
				showOptionsWindow();
			}
			else
			{
				this.statusBarPanel3.Text = "Creating data layout";
				try
				{
					setupAndDisplayData();
				}
				catch (Exception ex)
				{
					Message.Show(ex.Message + ", " +ex.InnerException.Message);
					//judetemp - attempt to narrow down the occasional scroll bar exception that we see
				}
			}
		}
		private void options_Closed(object sender, EventArgs e)
		{
			this.options = null;
			if (MAIN_WINDOW.mainWindowClosing == true)
			{
				return;
			}
			MAIN_WINDOW.UserInputInhibit = false;
			if(SELECT_PROFILE.editProfilesRequired == true)  //we came in through edit or user tesed connection in Options window
			{
				this.statusBarPanel3.Text = "Creating data layout";
				SELECT_PROFILE.editProfilesRequired = false;
				setupAndDisplayData();
			}

            if (this.dataRetrievalThread == null)
            {
                this.ToolsMenu.Enabled = true;
                // find and refresh data menu items only enabled when nodes currently connected
                if (treeView1.Nodes.Count > 0)
                {
                    this.FindMI.Enabled = true;
                    this.miRefreshData.Enabled = true;
                }
                else
                {
                    this.FindMI.Enabled = false;
                    this.miRefreshData.Enabled = false;
                }
                this.file_mi.Enabled = true;
                this.main_hlp_mi.Enabled = true;
                this.timer3.Enabled = true;
                this.progressBar1.Visible = true;
                showUserControls();
            }
		}

		#endregion owned form closed event handlers

		#region DCF related methods
		private void updateDCFContextMenu()
		{
			contextMenu1.MenuItems.Clear();
			#region add 'offline' options
			MenuItem sourceDCfFromFile = new MenuItem("Fill DCF store from file");
			sourceDCfFromFile.Click +=new EventHandler(sourceDCfFromFile_Click);	
			contextMenu1.MenuItems.Add(sourceDCfFromFile);
			#endregion add 'offline' options
			if(DCFCustList_TN.Nodes.Count>0)
			{
				#region delete items from DCf menu option
				if(this.treeView1.SelectedNode.Parent != null)
				{
					MenuItem deleteMI = new MenuItem("Remove from the DCF store");
					deleteMI.Click +=new EventHandler(BranchDeleteMI_Click);
					contextMenu1.MenuItems.Add(deleteMI);
				}
				else
				{
					MenuItem deleteMI = new MenuItem("Empty the DCF store");
					deleteMI.Click +=new EventHandler(BranchDeleteMI_Click);
					contextMenu1.MenuItems.Add(deleteMI);
				}
				#endregion delete items from DCf menu option
				#region save to file Menu Option
				MenuItem saveToDCFFile = new MenuItem("&Save DCF store to file");
				contextMenu1.MenuItems.Add(saveToDCFFile);
				saveToDCFFile.Click +=new EventHandler(saveToDCFFile_Click);
				#endregion save to file Menu Option
				#region DCF comparision menu option
				string statusBarOverrideText = this.AllType1FormsClosed();
				if(statusBarOverrideText == "")
				{
					MenuItem compareDCF = new MenuItem("Com&pare DCF store values to connected CAN nodes");
					if(DCFCompareActive == true)
					{
						compareDCF.Checked = true;
					}
					contextMenu1.MenuItems.Add(compareDCF);
					compareDCF.Click+=new EventHandler(compareDCF_Click);
				}
				#endregion DCF comparision menu option
				#region download to connected node menu option
				if(this.sysInfo.nodes.Length>0)
				{
					MenuItem downloadDCFToDeviceMI = new MenuItem("&Download DCF store to connected CAN node");
					contextMenu1.MenuItems.Add(downloadDCFToDeviceMI);
					MenuItem [] devicesMI = new MenuItem[this.sysInfo.nodes.Length];
					ushort menuPtr = 0;
					for (int CANdevicePtr = 0;CANdevicePtr<this.sysInfo.nodes.Length;CANdevicePtr++)
					{//currently look for a complete 0x1018 match - awaiting info from CH and PS on what to relax for SEVCON controllers
						if((this.sysInfo.nodes[CANdevicePtr].manufacturer != Manufacturer.UNKNOWN)
							&& (sysInfo.nodes[CANdevicePtr].productCode == this.sysInfo.DCFnode.productCode)
							&& (sysInfo.nodes[CANdevicePtr].vendorID == this.sysInfo.DCFnode.vendorID)
							&& (sysInfo.nodes[CANdevicePtr].revisionNumber == this.sysInfo.DCFnode.revisionNumber))
						{
							if( (this.DCFFromfile == true)
								|| (( this.DCFFromfile == false) 
								&& (this.sysInfo.nodes[CANdevicePtr].nodeID !=this.sysInfo.nodes[DCFSourceTableIndex].nodeID))
								)
							{
								String menuString = sysInfo.nodes[CANdevicePtr].deviceType 
									+ "(CAN node " + sysInfo.nodes[CANdevicePtr].nodeID.ToString() + ")";
								devicesMI[menuPtr] = new MenuItem(menuString);
								devicesMI[menuPtr].Click +=new EventHandler(downloadToCANDeviceMI_Click);
								downloadDCFToDeviceMI.MenuItems.Add(devicesMI[menuPtr]);
								menuPtr++;
							}
						}
					}
					if(menuPtr == 0) //no suitable nodes to download to
					{
						downloadDCFToDeviceMI.Enabled = false;
					}
				}
				#endregion download to connected node menu option
			}
		}
		private void addNodeToDCF_Click(object sender, EventArgs e)
		{
			this.hideUserControls();
			treeNodeTag selNodeTag = (treeNodeTag) this.treeView1.SelectedNode.Tag;
			DCFSourceTableIndex = selNodeTag.tableindex;
			addItemToCustomList(this.treeView1.SelectedNode, this.DCFCustList_TN, true);
			//do NOT switch to DCF, graph node here 
			//- data retrieval thread may still be running
			nextRequiredNode = DCFCustList_TN;//once thread has done it will deal with it
		}
		#region populating DCF list from file
		private void sourceDCfFromFile_Click(object sender, EventArgs e)
		{
			openFileDialog1.FileName = "";
			//put this line before the setting InitalDirectory to maintain starting point on directory structure 
			if ( Directory.Exists( MAIN_WINDOW.UserDirectoryPath + @"\DCF" ) == false )
			{
				try
				{
					Directory.CreateDirectory( MAIN_WINDOW.UserDirectoryPath + @"\DCF" );
				}
				catch
				{
					SystemInfo.errorSB.Append("\nFailed to create DCF directory");
					return;
				}
			}
			openFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\DCF";
			openFileDialog1.Title = "Open DCF file on DriveWizard host";
			openFileDialog1.Filter = "DCF files (*.dcf)|*.dcf" ;
			openFileDialog1.DefaultExt = "dcf";
			openFileDialog1.ShowDialog(this);
			if(openFileDialog1.FileName != "")
			{
				#region opening DCf file
				this.sysInfo.DCFnode.EDSorDCFfilepath = openFileDialog1.FileName;
				this.DCFCustList_TN.Nodes.Clear();  //clear out the old data
                DCFCompareActive = true; // reset compare function so table is updated properly
                compareDCF_Click(sender, e);
				this.statusBarPanel3.Text = "Parsing selected file";
				this.hideUserControls();
				DCFfileOpenThread = new Thread(new ThreadStart( openDCFFile )); 
				DCFfileOpenThread.Name = "DCF Open from file Thread";
				DCFfileOpenThread.IsBackground = true;
                DCFfileOpenThread.Priority = ThreadPriority.Normal;
#if DEBUG
				System.Console.Out.WriteLine("Thread: " + DCFfileOpenThread.Name + " started");
#endif
				DCFfileOpenThread.Start(); 
				timer2.Enabled = true; //start timer

				#endregion opening DCf file
			}
		}
		private void openDCFFile()
		{
			globalfeedback = this.sysInfo.readODFromDCF(this.sysInfo.DCFnode.EDSorDCFfilepath);
		}
		private void checkDCfFileOpenOK()
		{  //handlde feedback back on the main thread
			if(globalfeedback == DIFeedbackCode.DISuccess)
			{
				DCFFromfile = true;
				this.DCFCustList_TN.Text = "DCF (Source: File)";
				this.fillCustListTreeNodesFromFile();
				fillDCFLabels();
				currentTreenode = DCFCustList_TN;
				if(currentTreenode.Nodes.Count>0)
				{
					currentTreenode = currentTreenode.Nodes[0];
				}
				this.treeView1.SelectedNode = currentTreenode;
				this.handleTreeNodeSelection(currentTreenode, false);
				this.statusBarPanel3.Text = "File read OK";
			}
			else
			{
				statusBarPanel3.Text = "DCF file read failed";  //DR38000260
                SystemInfo.errorSB.Append("\nError reported: " + globalfeedback.ToString());
				this.showUserControls();
			}
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("Errors found when opening DCF:");

			}
		}
		private void fillDCFLabels()
		{
			#region device info group Box
			this.DCFProductCodeLbl.Text = "Product code: 0x" + this.sysInfo.DCFnode.productCode.ToString("X").PadLeft(8,'0');
			this.DCFVendorIDLbl.Text = "Vendor ID: 0x" + this.sysInfo.DCFnode.vendorID.ToString("X").PadLeft(8, '0');
			this.DCFRevNoLbl.Text = "Revison No.: 0x" + this.sysInfo.DCFnode.revisionNumber.ToString("X").PadLeft(8, '0');
			this.DCFDeviceNameLbl.Text = "Name: " + this.sysInfo.DCFnode.EDS_DCF.EDSdeviceInfo.productName;
			this.DCFVendorNameLbl.Text = "Vendor: " + this.sysInfo.DCFnode.EDS_DCF.EDSdeviceInfo.vendorName;
			#endregion device info group Box
			//
			if(this.DCFFromfile == true)
			{
				#region DCF fileInfo Group box
				this.DCFFileLastModLbl.Text = "Modified: " + this.sysInfo.DCFnode.EDS_DCF.FileInfo.LastModDate +  " " + this.sysInfo.DCFnode.EDS_DCF.FileInfo.LastModTime ;
				this.DCFFileNameLbl.Text = "DCF File: " + this.sysInfo.DCFnode.EDSorDCFfilepath;
				this.DCFCreatedByLbl.Text = "Created by: " + this.sysInfo.DCFnode.EDS_DCF.FileInfo.createdBy;
			}
			#endregion DCF fileInfo Group box
			revNoCB3.Items.Clear();
			productCodeCB.Items.Clear();
			for(uint i = 0;i<MAIN_WINDOW.availableEDSInfo.Count;i++)
			{
				AvailableNodesWithEDS EDSinfo = (AvailableNodesWithEDS) MAIN_WINDOW.availableEDSInfo[(int)i];
				string prodStr = "0x" + EDSinfo.productNumber.ToString("X").PadLeft(8,'0');
				if(this.productCodeCB.Items.Contains(prodStr) == false)
				{
					this.productCodeCB.Items.Add(prodStr);
				}
				string revStr = "0x" + EDSinfo.revisionNumber.ToString("X").PadLeft(8,'0');
				if(this.revNoCB3.Items.Contains(revStr) == false)
				{
					this.revNoCB3.Items.Add(revStr);
				}
			}
			string DcfProductStr = "0x" + this.sysInfo.DCFnode.productCode.ToString("X").ToUpper().PadLeft(8, '0');
			this.productCodeCB.SelectedIndex = -1;
			foreach(object item in this.productCodeCB.Items)
			{
				string objectText = (string) item;
				if(objectText.ToUpper() == DcfProductStr.ToUpper())
				{
					this.productCodeCB.SelectedIndex = this.productCodeCB.Items.IndexOf(item);
				}
			}
			if(this.productCodeCB.SelectedIndex == -1)
			{
				this.productCodeCB.Items.Add(DcfProductStr);
				this.productCodeCB.SelectedIndex = this.productCodeCB.Items.Count-1;
			}
			string DCFRevStr = "0x" + this.sysInfo.DCFnode.revisionNumber.ToString("X").ToUpper().PadLeft(8,'0');
			this.revNoCB3.SelectedIndex = -1;
			foreach(object item in this.revNoCB3.Items)
			{
				string objStr = (string)item;
				if(objStr.ToUpper() == DCFRevStr.ToUpper())
				{
					this.revNoCB3.SelectedIndex = this.revNoCB3.Items.IndexOf(item);
				}
			}
			if(this.revNoCB3.SelectedIndex == -1)
			{
				this.revNoCB3.Items.Add(DCFRevStr);
				this.revNoCB3.SelectedIndex = this.revNoCB3.Items.Count-1;
			}
		}
		private void fillCustListTreeNodesFromFile()
		{
			treeNodeTag selNodeTag = (treeNodeTag) this.treeView1.SelectedNode.Tag;
			//first clear out the TreeView]
			if(selNodeTag.tableindex == MAIN_WINDOW.DCFTblIndex)
			{
				#region DCF setup paramenters
				this.DCFCustList_TN.Nodes.Clear();
				MAIN_WINDOW.DWdataset.Tables[DCFTblIndex].Clear();

				#region determine next XML filename or use defualt
				if (this.sysInfo.DCFnode.EDSorDCFfilepath != null)
				{
					//SevconTree treeObject = new SevconTree();
					this.statusBarPanel3.Text = "Locating XML Tree for DCF file";
					DIFeedbackCode feedback = this.sysInfo.DCFnode.findMatchingXMLFile();
					if(feedback != DIFeedbackCode.DISuccess)
					{
						SystemInfo.errorSB.Append("\nGeneric XML Treestructure missing - Please report");
					}
					//Load the customer object from the existing XML file (if any)...
					#region extract the treeview contents from file
					//Load the customer object from the XML file using our custom class...
					SevconTree treeView = new SevconTree();
					ObjectXMLSerializer	objectXMLSerializer = new ObjectXMLSerializer();
					treeView = ( SevconTree ) objectXMLSerializer.Load( treeView, this.sysInfo.DCFnode.XMLfilepath );
					if ( treeView == null )
					{
						SystemInfo.errorSB.Append("\nFailed to load Tree Nodes from file: '" + this.sysInfo.DCFnode.XMLfilepath );
					}
					else  //Load customer properties into the form...
					{
						if(selNodeTag.tableindex == MAIN_WINDOW.DCFTblIndex)
						{
							#region Update DCF 'node' device parmameters
							if((this.sysInfo.DCFnode.EDSorDCFfilepath == null) || ( this.sysInfo.DCFnode.EDSorDCFfilepath == ""))
							{
								this.sysInfo.DCFnode.manufacturer = Manufacturer.UNKNOWN;
							}
							else
							{
								if(this.sysInfo.DCFnode.vendorID == SCCorpStyle.SevconID)
								{
									this.sysInfo.DCFnode.manufacturer = Manufacturer.SEVCON;
								}
								else
								{
									this.sysInfo.DCFnode.manufacturer = Manufacturer.THIRD_PARTY;
								}
							}
							if(this.sysInfo.DCFnode.deviceType == "")
							{
								this.sysInfo.DCFnode.deviceType = "missing DCF device name";  
							}
							#endregion Update DCF 'node' device parmameters
						}
						try
						{
							loadXMLTreeIntoForm(treeView, selNodeTag.tableindex , this.sysInfo.DCFnode);
						}
						catch(Exception exy)
						{
#if DEBUG
							SystemInfo.errorSB.Append("\nException seen: " + exy.Message + "loadXMLTreeIntoForm()");
#endif
						}
						//now perform recursive flush of TreeNodesflush out any 
						this.statusBarPanel3.Text = "Locating and Removing Dead End Nodes from XML Template";
						#region remove redundanc dt nodes from DCF Store
						if(this.DCFCustList_TN.Nodes.Count>0)
						{
							do
							{
								nodesRemovedFlag = false;  //set false before entry
								//if we remove any node sin this pass then it will be set to true
								removeRedundantSecionNodes(this.DCFCustList_TN.Nodes[0], MAIN_WINDOW.DCFTblIndex);
							}
							while (nodesRemovedFlag == true);  //chekc at end set inside method
						}
						else
						{
							do
							{
								nodesRemovedFlag = false;  //set false before entry
								//if we remove any node sin this pass then it will be set to true
								removeRedundantSecionNodes(this.DCFCustList_TN, MAIN_WINDOW.DCFTblIndex);
							}
							while (nodesRemovedFlag == true);  //chekc at end set inside method
						}
						#endregion remove redundanc dt nodes from DCF Store
					}
					#endregion extract the treeview contents from file
				}
				else
				{
					SystemInfo.errorSB.Append("\nFailed to find expected XML file for DCF");
					this.sysInfo.DCFnode.XMLfilepath = MAIN_WINDOW.UserDirectoryPath + @"\EDS\" + SCCorpStyle.DefaultXMLFile; //DR38000265
				}
				#endregion

				#endregion DCF setup paramenters
			}
			else if ( selNodeTag.tableindex == MAIN_WINDOW.GraphTblIndex)
			{
				#region monitoring list parameters
				this.GraphCustList_TN.Nodes.Clear();//These are treenodes in the TreeView not CANopen nodes
				MAIN_WINDOW.DWdataset.Tables[GraphTblIndex].Clear();
				foreach(nodeInfo mnNode in this.monStore.myMonNodes)
				{
//					if(mnNode.isInCurrentSystem == false)
//					{
//						continue;
//					}
					//Load the customer object from the existing XML file (if any)...
					#region extract the treeview contents from file
					//Load the customer object from the XML file using our custom class...
					SevconTree treeView = new SevconTree();
					ObjectXMLSerializer	objectXMLSerializer = new ObjectXMLSerializer();

					try
					{
						treeView = ( SevconTree ) objectXMLSerializer.Load( treeView, mnNode.XMLfilepath );
					}
					catch//(Exception e)
					{
//						string test = e.InnerException.ToString();
					}
					if ( treeView == null )
					{
						SystemInfo.errorSB.Append("\nFailed to load Tree Nodes from file: '" + mnNode.XMLfilepath);
					}
					else  //Load customer properties into the form...
					{
						try
						{
							loadXMLTreeIntoForm(treeView, selNodeTag.tableindex , mnNode);
						}
						catch//(Exception exy)
						{
#if DEBUG
//							SystemInfo.errorSB.Append("\nException seen: " + exy.Message + "loadXMLTreeIntoForm()");
#endif
						}
						//now perform recursive flush of TreeNodesflush out any 
						this.statusBarPanel3.Text = "Locating and Removing Dead End Nodes from XML Template";
						#region remove redundant nodes from Monitoring Store
						do
						{
							nodesRemovedFlag = false;  //set false before entry
							//if we remove any nodes in this pass then it will be set to true
							removeRedundantSecionNodes(this.GraphCustList_TN, MAIN_WINDOW.GraphTblIndex);
						}
						while (nodesRemovedFlag == true);  //chekc at end set inside method
						#endregion remove redundant nodes from Monitoring Store
					}
					#endregion extract the treeview contents from file
				}
				#endregion monitoring list parameters
				this.GraphCustList_TN.Text = "Monitoring list (Source: File)";
			}
		}
		#endregion populating DCf list from file
		#region comparing files to connected nodes
		private void compareDCF_Click(object sender, EventArgs e)
		{
			this.hideUserControls();
			MenuItem compareMI = (MenuItem) sender;
			if (DCFCompareActive == false)
			{
				DCFCompareActive = true;
				dataRetrievalThread = new Thread(new ThreadStart( updateComparisonTable )); 
				dataRetrievalThread.Name = "DCF compare full table creation";
				dataRetrievalThread.IsBackground = true;
                dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
				System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif  
				this.progressBar1.Value = this.progressBar1.Minimum;
				this.progressBar1.Maximum = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].DefaultView.Count;
				this.statusBarPanel3.Text = "Retrieving values from all connected nodes...";
				dataRetrievalThread.Start(); 
				this.timer2.Enabled = true;
			}
			else
			{
				DCFCompareActive = false;
				foreach(Control c in this.dataGrid1.Controls)
				{
					if	(c.GetType().Equals( typeof( HScrollBar ) ) ) 
					{
						c.Height = 0;
						break;
					}
				}
				this.dataGrid1.DataSource = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].DefaultView;

				this.showUserControls();
				forceDataGridResize(this.dataGrid1);
			}
		}
		private void createComparisonTable()
		{
			#region get list of the SystemInfo.Nodes indexes (and their Node IDs) of all connected devices with an EDS
			devicesWithEDS = new ArrayList();
			compNodeIDs = new ArrayList();
			for(int i = 0;i<this.sysInfo.nodes.Length;i++)
			{
				if(sysInfo.nodes[i].manufacturer != Manufacturer.UNKNOWN)
				{
					devicesWithEDS.Add(i);
					compNodeIDs.Add(sysInfo.nodes[i].nodeID);
				}
			}
			#endregion get list of the SystemInfo.Nodes indexes (and their Node IDs) of all connected devices with an EDS
			#region create table for comparison data
			comparisonTable = new DataTable();
			comparisonTable.Columns.Add(DCFCompCol.Index.ToString(),typeof(System.String));
			comparisonTable.Columns[DCFCompCol.Index.ToString()].DefaultValue = "";  
			comparisonTable.Columns.Add(DCFCompCol.sub.ToString(),typeof(System.String));
			comparisonTable.Columns[DCFCompCol.sub.ToString()].DefaultValue = "";  
			comparisonTable.Columns.Add(DCFCompCol.param.ToString(), typeof(System.String));
			comparisonTable.Columns[DCFCompCol.param.ToString()].DefaultValue = "";  
			comparisonTable.Columns.Add(DCFCompCol.DCFValue.ToString(),typeof(System.String));
			comparisonTable.Columns[DCFCompCol.DCFValue.ToString()].DefaultValue = "";  

			#region add columns for nodes ot be compared
			if(devicesWithEDS.Count>=1)
			{
				comparisonTable.Columns.Add(DCFCompCol.Value0.ToString(),typeof(System.String));
			}
			if(devicesWithEDS.Count>=2)
			{
				comparisonTable.Columns.Add(DCFCompCol.Value1.ToString(),typeof(System.String));
			}
			if(devicesWithEDS.Count>=3)
			{
				comparisonTable.Columns.Add(DCFCompCol.Value2.ToString(),typeof(System.String));
			}
			if(devicesWithEDS.Count>=4)
			{
				comparisonTable.Columns.Add(DCFCompCol.Value3.ToString(),typeof(System.String));
			}
			if(devicesWithEDS.Count>=5)
			{
				comparisonTable.Columns.Add(DCFCompCol.Value4.ToString(),typeof(System.String));
			}
			if(devicesWithEDS.Count>=6)
			{
				comparisonTable.Columns.Add(DCFCompCol.Value5.ToString(),typeof(System.String));
			}
			if(devicesWithEDS.Count>=7)
			{
				comparisonTable.Columns.Add(DCFCompCol.Value6.ToString(),typeof(System.String));
			}
			if(devicesWithEDS.Count>=8)
			{
				comparisonTable.Columns.Add(DCFCompCol.Value7.ToString(),typeof(System.String));
			}
			for(int i = 0;i<this.comparisonTable.Columns.Count;i++)
			{
				comparisonTable.Columns[i].DefaultValue = "";
			}
			#endregion add columns for nodes ot be compared
			//now add a column to indicate whether the data values are same
			#endregion create table and add columns
			comparisonTable.DefaultView.AllowNew = false;
			comparisonTable.TableName = "DCFcomparisonTable";
		}
		private void createDCFCompareTableStyle()
		{
			#region DatGrid Colum percentage widths for each possible numbe rof connected devices
			//								Ind		sub		Name	DCF val	Conn Nodes
			float [] oneDeviceWithEDS =		{0.15F,	0.15F,	0.4F,	0.15F,	0.15F};
			float [] twoDevicesWithEDS =	{0.12F,	0.12F,	0.4F,	0.12F, 0.12F, 0.12F};
			float [] threeDevicesWithEDS =	{0.1F,	0.1F,	0.4F,	0.1F,	0.1F,0.1F, 0.1F};
			float [] fourDevicesWithEDS =	{0.1F,	0.1F,	0.3F,	0.1F,	0.1F, 0.1F, 0.1F, 0.1F};
			float [] fiveDevicesWithEDS =	{0.09F,	0.09F,	0.28F,	0.09F,	0.09F, 0.09F, 0.09F, 0.09F, 0.09F};
			float [] sixDevicesWithEDS =	{0.08F,	0.08F,	0.28F,	0.08F,	0.08F, 0.08F, 0.08F, 0.08F, 0.08F, 0.08F};
			float [] sevenDevicesWithEDS =	{0.07F, 0.07F,	0.3F,	0.07F,	0.07F, 0.07F, 0.07F, 0.07F, 0.07F, 0.07F, 0.07F};
			float [] eightDevicesWithEDS =	{0.063F,0.063F, 0.307F,	0.063F,	0.063F, 0.063F, 0.063F, 0.063F, 0.063F, 0.063F, 0.063F,  0.063F};
			//now create a jagged array of the above
			DataGridColWidths = new float [8][] {oneDeviceWithEDS,
													twoDevicesWithEDS,
													threeDevicesWithEDS,
													fourDevicesWithEDS,
													fiveDevicesWithEDS,
													sixDevicesWithEDS,
													sevenDevicesWithEDS,
													eightDevicesWithEDS};

			#endregion DatGrid Colum percentag ewidths for each possible numbe rof connected devices
			#region convert the correct percentage widths into actual widths based on DataGRid Screen size
			//now select the correct array of colimn widths depentdent on number of cmapre nodes passed from MAIN WINDOW
			int [] colWidths  = new int[DataGridColWidths[compNodeIDs.Count-1].Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, DataGridColWidths[compNodeIDs.Count-1], 0, dataGrid1DefaultHeight);

			#endregion convert the correct percentage widths into actual widths based on DataGRid Screen size
			this.compTableStyle = new DCFCompareTableStyle(comparisonTable.TableName, colWidths, compNodeIDs);
			this.dataGrid1.TableStyles.Add(this.compTableStyle);
		}

		private void updateComparisonTable()
		{
			int rowInd = 0;
			bool lastRowWasHeader = false;
			this.comparisonTable.Clear();  //remove any existing rows
			compTableIndexes = new int[devicesWithEDS.Count];
			MAIN_WINDOW.compColArray = new Color[compTableIndexes.Length, MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].DefaultView.Count]; 
			devicesWithEDS.CopyTo(compTableIndexes);
			DCFcompProgBarCounter = 0;
			foreach(DataRowView dvRow in MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].DefaultView)
			{
				DCFcompProgBarCounter++;
				DataRow compRow = comparisonTable.NewRow();
				ODItemData srceSub = (ODItemData) dvRow.Row[(int) TblCols.odSub];
				#region get the Index /Sub index primary key to search for

				object[] myKeys = new object[2];
				myKeys[0] = dvRow.Row[(int)TblCols.Index].ToString();
				myKeys[1] = dvRow.Row[(int)TblCols.sub].ToString();
				#endregion get the Index /Sub index primary key to search for
				#region see if the same Index/SUb combination item exists in each of the comparison CAN devices								
				#region check if the OD item exists in each of the connected (with EDS) devices
				compRow[ (int) DCFCompCol.Index] = myKeys[0];
				compRow[ (int) DCFCompCol.sub] = myKeys[1];
				compRow[ (int) DCFCompCol.param] = dvRow.Row[(int)TblCols.param].ToString();
				compRow[ (int) DCFCompCol.DCFValue] = dvRow.Row[(int)TblCols.actValue].ToString();
				#endregion check if the OD item exists in each of the connected (with EDS) devices
				for(int compInd = 0;compInd<devicesWithEDS.Count;compInd++)
				{
					DataRow row = MAIN_WINDOW.DWdataset.Tables[compTableIndexes[compInd]].Rows.Find(myKeys); 
					if(row != null)  //ie the OD item exists
					{
						#region get an actual value for each OD item to compare, put it in source table and in compare table
						long newLong = 0;
                        ODItemData odSub = this.sysInfo.nodes[compInd].getODSub(srceSub.indexNumber, srceSub.subNumber);
						if(odSub != null)
						{ 
							globalfeedback = this.sysInfo.nodes[compInd].readODValue(odSub);
							newLong = odSub.currentValue;
						}
						if(this.globalfeedback == DIFeedbackCode.DISuccess)
						{
							ODItemData odcompSub = sysInfo.nodes[compTableIndexes[compInd]].getODSub(srceSub.indexNumber, srceSub.subNumber); //judetemp - should be null check here
							this.fillTableRowColumns(odcompSub,row, compTableIndexes[compInd],false, true, sysInfo.nodes[compTableIndexes[compInd]]);
							compRow[ compInd + ((int)DCFCompCol.Value0)] = row[(int) TblCols.actValue].ToString();
						}
					    // else... DR38000260 globalfeedback dealt with at the end of the thread
						bool nodeInPreOp = true;
						uint UserAccLevel = 0;
						#region set pre-op and user access 
						if(sysInfo.nodes[compTableIndexes[compInd]].nodeState != NodeState.PreOperational)
						{
							nodeInPreOp = false;
						}
						UserAccLevel = sysInfo.nodes[compTableIndexes[compInd]].accessLevel;
						#endregion set pre-op and user access 
						if(odSub != null )
						{
						if(odSub.subNumber == -1)
						{
							compColArray[compInd,rowInd] = SCCorpStyle.headerRow;
						}
						else
						{
							switch(odSub.accessType)
							{
									#region give each row a correspondinf Color, for use by DataGridColumn Class Paint method
								case ObjectAccessType.ReadOnly:
								case ObjectAccessType.Constant:
									#region read only/consts
									compColArray[compInd,rowInd] = SCCorpStyle.readOnly;
									break;
									#endregion read only/consts

								case ObjectAccessType.WriteOnly:
									#region write only
									compColArray[compInd,rowInd] = SCCorpStyle.writeOnly;
									break;
									#endregion write only

								case ObjectAccessType.ReadReadWriteInPreOp:
								case ObjectAccessType.ReadWriteInPreOp:
								case ObjectAccessType.ReadWriteWriteInPreOp:
									#region read/writes in Pre-Op
									if(odSub.accessLevel > UserAccLevel)
									{
										compColArray[compInd,rowInd] = SCCorpStyle.readOnly;
									}
									else
									{
										if(nodeInPreOp == true)
										{
											compColArray[compInd,rowInd] = SCCorpStyle.readWrite;
										}
										else
										{
											compColArray[compInd,rowInd] = SCCorpStyle.readWriteInPreOp;
										}
									}
									break;
									#endregion read/writes in Pre-Op

								case ObjectAccessType.ReadReadWrite:
								case ObjectAccessType.ReadWrite:
								case ObjectAccessType.ReadWriteWrite:
									#region read/writes
									if(odSub.accessLevel > UserAccLevel)
									{
										compColArray[compInd,rowInd] = SCCorpStyle.readOnly;
									}
									else
									{
										compColArray[compInd,rowInd] = SCCorpStyle.readWrite;
									}
									break;
									#endregion read/writes

								case ObjectAccessType.WriteOnlyInPreOp:
									#region write only in pre-op
									if(nodeInPreOp == true)
									{
										compColArray[compInd,rowInd] = SCCorpStyle.writeOnly;
									}
									else
									{
										compColArray[compInd,rowInd] = SCCorpStyle.writeOnlyPlusPreOP;
									}
									break;
									#endregion write only in pre-op

								default:
									#region default 
									compColArray[compInd,rowInd] = SCCorpStyle.readOnly;
									SystemInfo.errorSB.Append("\nUndefined object type: " + odSub.accessType.ToString());
									break;
									#endregion default 
									#endregion give each row a corresponding Color, for use by DataGridColumn Class Paint method
								}
							}
						}
						else
						{
							//int stopher = 1;
						}
						#endregion get an actual value for each OD item to compare, put it in source table and in compare table
					}
					else
					{ //the OD item does not exist in this device
						#region this item does not exist in this node
						if(compRow[ (int) DCFCompCol.sub].ToString() == "")
						{
							compColArray[compInd,rowInd] = SCCorpStyle.headerRow;
						}
						else if	(((dvRow.Row[(int) TblCols.accessType].ToString() == ObjectAccessType.ReadOnly.ToString())
							|| (dvRow.Row[(int) TblCols.accessType].ToString() == ObjectAccessType.Constant.ToString())) 
							&& (dvRow.Row[(int) TblCols.sub].ToString() == "0")
							&& (lastRowWasHeader == true))
						{
							compRow[ compInd + ((int)DCFCompCol.Value0)] = "---";
							compColArray[compInd,rowInd] = SCCorpStyle.readOnly;  //grey it out
						}
						else
						{
							compRow[ compInd + ((int)DCFCompCol.Value0)] = "not in OD";
							compColArray[compInd,rowInd] = Color.Red;  
						}
						#endregion this item does not exist in this node
					}
				}
				#endregion see if the same Index/SUb combination item exists in each of the comparison CAN devices	
				comparisonTable.Rows.Add(compRow);	
				#region mark this row if it is a header row
				if(srceSub.subNumber ==-1)
				{
					lastRowWasHeader = true;
				}
				else
				{
					lastRowWasHeader = false;
				}
				#endregion mark this row if it is a header row
				rowInd++;
			}
		}
		private void UpdateDCFCompareData()
		{
			int rowInd = 0;
			bool lastRowWasHeader = false;
			DCFcompProgBarCounter = 0;
			foreach(DataRowView dvRow in this.comparisonTable.DefaultView)
			{
				DCFcompProgBarCounter++;
				#region get the Index /Sub index primary key to search for
				object[] myKeys = new object[2];
				int index  = System.Convert.ToInt32(dvRow.Row[(int)DCFCompCol.Index].ToString(), 16);
				myKeys[0] = "0x" + index.ToString("X").PadLeft(4, '0'); // format correctly so Find works
				myKeys[1] = ""; //defualt to header row indicator
				int sub = this.getSub(dvRow.Row[(int)DCFCompCol.sub].ToString());
				if(sub != -1)
				{
					myKeys[1] = "0x" + sub.ToString("X").PadLeft(3, '0');
				}
				#endregion get the Index /Sub index primary key to search for
                for (int compInd = 0;compInd<devicesWithEDS.Count;compInd++)
				{
                    #region set pre-op and user access
                    bool nodeInPreOp = true;
                    uint UserAccLevel = 0;

                    if (sysInfo.nodes[compTableIndexes[compInd]].nodeState != NodeState.PreOperational)
                    {
                        nodeInPreOp = false;
                    }
                    UserAccLevel = sysInfo.nodes[compTableIndexes[compInd]].accessLevel;
                    #endregion set pre-op and user access
                    DataRow row = MAIN_WINDOW.DWdataset.Tables[compTableIndexes[compInd]].Rows.Find(myKeys); 
					if(row != null)  //ie the OD item exists
					{
						ODItemData odSub = (ODItemData) row[(int) TblCols.odSub];
						if(odSub.subNumber == -1)
						{
							compColArray[compInd,rowInd] = SCCorpStyle.headerRow;
						}
						else
						{
							switch(odSub.accessType)
							{
									#region give each node cell a corresponding color, for use by DataGridColumn Class Paint method
								case ObjectAccessType.ReadOnly:
								case ObjectAccessType.Constant:
									#region read only/consts
									compColArray[compInd,rowInd] = SCCorpStyle.readOnly;
									break;
									#endregion read only/consts

								case ObjectAccessType.WriteOnly:
									#region write only
									compColArray[compInd,rowInd] = SCCorpStyle.writeOnly;
									break;
									#endregion write only

								case ObjectAccessType.ReadReadWriteInPreOp:
								case ObjectAccessType.ReadWriteInPreOp:
								case ObjectAccessType.ReadWriteWriteInPreOp:
									#region read/writes in Pre-Op
                                    if (odSub.accessLevel > UserAccLevel)
									{
										compColArray[compInd,rowInd] = SCCorpStyle.readOnly;
									}
									else
									{
                                        if (nodeInPreOp == true)
										{
											compColArray[compInd,rowInd] = SCCorpStyle.readWrite;
										}
										else
										{
											compColArray[compInd,rowInd] = SCCorpStyle.readWriteInPreOp;
										}
									}
									break;
									#endregion read/writes in Pre-Op

								case ObjectAccessType.ReadReadWrite:
								case ObjectAccessType.ReadWrite:
								case ObjectAccessType.ReadWriteWrite:
									#region read/writes
                                    if (odSub.accessLevel > UserAccLevel)
									{
										compColArray[compInd,rowInd] = SCCorpStyle.readOnly;
									}
									else
									{
										compColArray[compInd,rowInd] = SCCorpStyle.readWrite;
									}
									break;
									#endregion read/writes

								case ObjectAccessType.WriteOnlyInPreOp:
									#region write only in pre-op
									if(nodeInPreOp == true)
									{
										compColArray[compInd,rowInd] = SCCorpStyle.writeOnly;
									}
									else
									{
										compColArray[compInd,rowInd] = SCCorpStyle.writeOnlyPlusPreOP;
									}
									break;
									#endregion write only in pre-op

								default:
									#region default 
									compColArray[compInd,rowInd] = SCCorpStyle.readOnly;
									SystemInfo.errorSB.Append("\nUndefined object type: " + odSub.accessType.ToString());
									break;
									#endregion default 
									#endregion give each row a corresponding Color, for use by DataGridColumn Class Paint method
							}
						}
					}
					else
					{ //the OD item does not exist in this device
						#region this item does not exist in this node
						if(dvRow.Row[ (int) DCFCompCol.sub].ToString() == "")
						{
							compColArray[compInd,rowInd] = SCCorpStyle.headerRow;
						}
						else if( row == null)
						{
							dvRow.Row[ compInd + ((int)DCFCompCol.Value0)] = "not in OD";
							compColArray[compInd,rowInd] = Color.Red;  
						}
						else if	(((row[(int) TblCols.accessType].ToString() == ObjectAccessType.ReadOnly.ToString())
							|| (row[(int) TblCols.accessType].ToString() == ObjectAccessType.Constant.ToString())) 
							&& (row[(int) TblCols.sub].ToString() == "0")
							&& (lastRowWasHeader == true))
						{
							dvRow.Row[ compInd + ((int)DCFCompCol.Value0)] = "---";
							compColArray[compInd,rowInd] = SCCorpStyle.readOnly;  //grey it out
						}
						else
						{
							dvRow.Row[ compInd + ((int)DCFCompCol.Value0)] = "not in OD";
							compColArray[compInd,rowInd] = Color.Red;  
						}
						#endregion this item does not exist in this node
					}
				}
				#region mark this row if it is a header row
				if(sub ==-1)
				{
					lastRowWasHeader = true;
				}
				else
				{
					lastRowWasHeader = false;
				}
				#endregion mark this row if it is a header row
				rowInd++;
			}
		}
		#endregion comparing files to connected nodes
		#region saving to file
		private void saveToDCFFile_Click(object sender, EventArgs e)
		{
			DIFeedbackCode feedback;
			this.hideUserControls();
			#region saveFileDialog setup parameters
			//put this line before the setting InitalDirectory to maintain starting point on directory structure 
			saveFileDialog1.FileName = "";
			if ( Directory.Exists( MAIN_WINDOW.UserDirectoryPath + @"\DCF" ) == false )
			{
				try
				{
					Directory.CreateDirectory( MAIN_WINDOW.UserDirectoryPath + @"\DCF" );
				}
				catch{}
			}
			saveFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\DCF";
			saveFileDialog1.Title = "Save DCF file to DriveWizard host";
			saveFileDialog1.Filter = "DCF files (*.dcf)|*.dcf" ;
			saveFileDialog1.DefaultExt = "dcf";
			saveFileDialog1.ShowDialog();	
			#endregion saveFileDialog setup parameters
			if(saveFileDialog1.FileName != "")
			{
				this.statusBarPanel3.Text = "Saving DCF file";
				int sourceNodeIndex = 0;
				#region first decide whether the user has chosen to change the Product Code/Revision number for this DCF
				string DCFProdCodestr = "0X" + this.sysInfo.DCFnode.productCode.ToString("X").ToUpper().PadLeft(8,'0');
				string DCFRevNoStr = "0X" + this.sysInfo.DCFnode.revisionNumber.ToString("X").ToUpper().PadLeft(8, '0');
				if((this.productCodeCB.Text.ToUpper()!= DCFProdCodestr) || (this.revNoCB3.Text.ToUpper() != DCFRevNoStr))
				{
					DialogResult result = 
						Message.Show(this, 
						"Click YES to update product code and revision number in DCF file. \nNO to Retain current values", 
						"DCF product code and or Revision number changed", 
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
					if(result == DialogResult.Yes)
					{
						bool entryOK = true;
						uint prodCode = 0xFFFFFFFF, revNo = 0xFFFFFFFF;
						try
						{					
							prodCode = System.Convert.ToUInt32(this.productCodeCB.Text, 16);
							revNo = System.Convert.ToUInt32(this.revNoCB3.Text, 16);
						}
						catch
						{
							entryOK = false; 
							//TODO user feedback needed here
						}
						if(entryOK == true)
						{
							this.sysInfo.DCFnode.setDeviceIdentity(this.sysInfo.DCFnode.vendorID, prodCode, revNo);
						}
					}
				}
				#endregion first decide whether the user has chosen to change the underlying edS file for this DCF
				#region get all the underlying data from the DCF Treenode and put it into a collection
				ArrayList DCFItems = new ArrayList();
				treeNodeTag firstNodetag = (treeNodeTag) DCFCustList_TN.Nodes[0].Tag;
				feedback = this.sysInfo.getNodeNumber(firstNodetag.nodeID, out sourceNodeIndex);
				if(this.sysInfo.DCFnode.objectDictionary.Count<=0)
				{
					SystemInfo.errorSB.Append("\nDCF contains no associated Object Dictionary items.");
					this.showUserControls();
					return;
				}
				#endregion get all the underlying data from the DCF Treenode and put it into a collection
				#region determine whether to incldue the Backdoor Checksum
				bool includeDCFChecksum = false;
				if(((this.DCFFromfile== true) && (this.sysInfo.DCFnode.isSevconApplication() == true) && (this.sysInfo.systemAccess>=5))
					|| ((this.DCFFromfile == false) && (this.sysInfo.nodes[DCFSourceTableIndex].isSevconApplication() == true) && (this.sysInfo.nodes[DCFSourceTableIndex].accessLevel>=5)))
				{
					DialogResult result = 
						Message.Show(this, 
						"DCF files containing a DCF checksum OVERRIDE espAC security." 
						+  "\nAll OD items in the file can be written to a SEVCON device regardless of user access level."
						+  "\nIf in doubt, click Cancel to omit the checksum", 
						"Click OK to add DCF checksum, Cancel to omit checksum", 
						MessageBoxButtons.OKCancel,
						MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
					if(result == DialogResult.OK)
					{
						includeDCFChecksum = true;
					}
				}
				#endregion determine whether to incldue the Backdoor Checksum
				if((includeDCFChecksum == true) || (this.DCFFromfile == true))
				{
					this.sysInfo.DCFnode.accessLevel = 5;  //full access to wirte all parameters
				}
				else  //use the access elvle of the source node
				{
					this.sysInfo.DCFnode.accessLevel = this.sysInfo.DCFnode.DCFSourceNode.accessLevel;
				}
				this.sysInfo.DCFnode.EDSorDCFfilepath = saveFileDialog1.FileName;
				this.sysInfo.DCFnode.includeChecksum = includeDCFChecksum;
				#region start DCf filesave download thread
				this.progressBar1.Value  = this.progressBar1.Minimum;
				this.progressBar1.Maximum = this.sysInfo.DCFnode.objectDictionary.Count;
				DCFFileSaveThread = new Thread(new ThreadStart( saveDCFFile )); 
				DCFFileSaveThread.Name = "DCF file save Thread";
				DCFFileSaveThread.IsBackground = true;
                DCFFileSaveThread.Priority = ThreadPriority.Normal;
#if DEBUG
				System.Console.Out.WriteLine("Thread: " + DCFFileSaveThread.Name + " started");
#endif
				DCFFileSaveThread.Start(); 
				#endregion start download thread
				this.timer2.Enabled = true; //start timer
			}
			else
			{
				this.showUserControls();
			}
		}
		private void saveDCFFile()
		{
            this.globalfeedback = this.sysInfo.DCFnode.writeDCFfile(this.sysInfo.CANcomms.systemBaud);
		}
		#endregion
		#region downloading DCF Data to device
		private void downloadToCANDeviceMI_Click(object sender, EventArgs e)
		{
			MenuItem localMi = (MenuItem) sender;
			#region find sopouce and destination tableIndexes ( same as CANIndex)
			treeNodeTag rootTag = (treeNodeTag) this.DCFCustList_TN.Nodes[0].Tag;
			//DCFSource is set when we 
			DCFDestinationTableIndex = localMi.Index;
			#region adjust destination tableIndex to take account of non-compatible devices
			//now adjust destinationTableIndex to take account non-suitable destication devices
			int numCompatibleNodesFound = 0;
			for ( int i = 0;i< this.sysInfo.nodes.Length;i++)
			{
				if(
					(this.sysInfo.nodes[i].manufacturer != this.sysInfo.DCFnode.manufacturer)
					|| (this.sysInfo.nodes[i].productCode != this.sysInfo.DCFnode.productCode)
					|| (this.sysInfo.nodes[i].revisionNumber != this.sysInfo.DCFnode.revisionNumber)
					//following line ois to take account of  us not allowin guser to downlad back to same node source 
					||((this.sysInfo.nodes[i].nodeID == this.sysInfo.DCFnode.nodeID) && (this.DCFFromfile == false))
					)
				{   //'re-insert' the non-compatible node to correect the nodes[index]
					DCFDestinationTableIndex++;  
				}
				else
				{
					numCompatibleNodesFound++;
					if(numCompatibleNodesFound >localMi.Index)
					{  //we have found all non-compatibles that occured before our selected compatible node
						break;
					}
				}
			}
			#endregion adjust destination tableIndex to take account of non-compativble devices
			#endregion find sopouce and destination tableIndexes ( same as CANIndex)
			#region check whether destination node is in pre-op
			this.sysInfo.itemBeingWritten = 0;
			bool OKToDownload = false;
			if(this.sysInfo.nodes[DCFDestinationTableIndex].nodeState != NodeState.PreOperational)
			{
				string message = "Destination node is not in pre-operational NMT state. Some items may not be written. \nDo you wish to continue?";
				if ( MessageBox.Show(this, message , "Caution: Destination node is not in pre-op.", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2 ) == DialogResult.Yes )
				{
					OKToDownload = true;
				}
			}
			else
			{
				OKToDownload = true;
			}
			if(OKToDownload == true)
			{
				hideUserControls();
				this.statusBarPanel3.Text = "Downloading to device";
				#region start download thread
				this.sysInfo.itemBeingWritten = 0;
				this.progressBar1.Value  = this.progressBar1.Minimum;
				this.progressBar1.Maximum = this.sysInfo.DCFnode.objectDictionary.Count;
				DCFdownloadToDeviceThread = new Thread(new ThreadStart( downloadToDevice )); 
				DCFdownloadToDeviceThread.Name = "DCF Download Thread";
				DCFdownloadToDeviceThread.IsBackground = true;
                DCFdownloadToDeviceThread.Priority = ThreadPriority.Normal;
#if DEBUG
				System.Console.Out.WriteLine("Thread: " + DCFdownloadToDeviceThread.Name + " started");
#endif
				DCFdownloadToDeviceThread.Start(); 
				#endregion start download thread
				this.timer2.Enabled = true; //start timer
			}
			#endregion check whether destination node is in pre-op
		}

		private void downloadToDevice()
		{
			bool timer3WasEnabled = false;
			if(this.timer3.Enabled == true)
			{
				timer3WasEnabled = true;
				this.timer3.Enabled = false;
			}
			//feedback code is bound into abortMessage now
			this.sysInfo.writePartialDCFNodeToDevice(this.sysInfo.nodes[DCFDestinationTableIndex]);
			if(timer3WasEnabled == true)
			{
				this.timer3.Enabled = true;
			}
		}
		private void checkDownload()
		{
			if(SystemInfo.errorSB.Length == 0) //this way round to update label1 correctly
			{
				this.UserInstructionsLabel.Text = "Download this data to another node, open a DCF data file \nor upload DCF data from a device";
				this.statusBarOverrideText = "DCF downloaded OK";
				this.dataGrid1.CaptionText = "DCF information for node "  + this.sysInfo.DCFnode.nodeID.ToString();
			}
			else
			{
				this.statusBarPanel3.Text = "Failed to download DCF";
				//when writing multiple items abortMessage containes entire error from DI
				sysInfo.displayErrorFeedbackToUser("Failed to download all DCF items to device:\n");
			}
		}
	
		#endregion downloading DCF Data to device
		private void dataGridContextMenu_MI2_DCF_Click(object sender, EventArgs e)
		{
			string menuText = ((System.Windows.Forms.MenuItem) sender).Text;
			this.hideUserControls();
			if(menuText == "Remove this item from DCF store")
			{
				#region remove odSub from DCF
				#region get dcfRow
				DataView tmpView = (DataView) this.dataGrid1.DataSource; //dig out the underlying DataView
				//defualt to DCF table
				DataRow dcfRow = tmpView[this.selectedDgRowInd].Row;  //independent of datagrid columns being displayed
				if(MAIN_WINDOW.currTblIndex != MAIN_WINDOW.DCFTblIndex)
				{
					#region USer clicked on a table for a CAN Node - get the corresponding dcfROW
					#region get the Index /Sub index primary key to search for
					object[] myKeys = new object[2];
					myKeys[0] = tmpView[this.selectedDgRowInd].Row[(int)TblCols.Index].ToString();
					myKeys[1] = tmpView[this.selectedDgRowInd].Row[(int)TblCols.sub].ToString();
					#endregion get the Index /Sub index primary key to search for
					dcfRow = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].Rows.Find(myKeys); 
					if(dcfRow == null)
					{
						SystemInfo.errorSB.Append("Failed to locate table row - null row error");
						return;
					}
					#endregion USer clicked on a table for a CAN Node - get the corresponding dcfROW
				}
				#endregion get dcf row
				#region idetify the odSub that we want to remove from the DCF
				ODItemData subToRemove = (ODItemData) dcfRow[(int) TblCols.odSub]; //do this before removing the row
				#endregion idetify the odSub that we want to remove from the DCF
				#region identify corresponding header and numItems sub if they exist
				MAIN_WINDOW.appendErrorInfo = false;
				ODItemData numItemsSub = null;
				ODItemData headerSub = this.sysInfo.DCFnode.getODSub(subToRemove.indexNumber,-1);
				if(headerSub != null)
				{
					numItemsSub = this.sysInfo.DCFnode.getODSub(subToRemove.indexNumber,0);
					if((numItemsSub != null) && (numItemsSub.isNumItems == false))
					{
						numItemsSub = null;  //we found one of this writeable num items subs - not a real one 
					}
				}
				MAIN_WINDOW.appendErrorInfo = true;
				#endregion identify corresponding header and numImtes sub if they exist
				#region find header table row and decide whther we are removeing single sub or whole odITem (last sub for this odItem being removed)
				//check if any other non header or numITems rows for thisodITme reaim in DCF table
				DataRow headerRow = null;
				bool removewholeOdItem = true;
				foreach(DataRow otherDCFrow in DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].Rows)
				{
					ODItemData otherDCFodSUB = (ODItemData) otherDCFrow[(int) TblCols.odSub];
					if(otherDCFodSUB.subNumber == -1)
					{
						headerRow = otherDCFrow;
					}
					else if((otherDCFodSUB != subToRemove) && (otherDCFodSUB != numItemsSub) && (otherDCFodSUB != headerSub))
					{
						removewholeOdItem = false; //we have other subs for this odItme in DCf so leave header and numItmes subs alone
						//oh and leave the tree node in place
						break;
					}
				}
				#endregion find header table row and decide whther we are removeing single sub or whole odITem (last sub for this odItem being removed)
				#region remove subToRemove form DCF dictionary
				this.sysInfo.DCFnode.removeSubFromDictionary(subToRemove);
				#endregion remove subToRemove form DCF dictionary
				if(removewholeOdItem == true)
				{
					#region removeing all remaining parts of the odItem
					#region remove header and numItems sub from DCF dictionary is required
					if(headerSub != null)
					{
						this.sysInfo.DCFnode.removeSubFromDictionary(headerSub);
					}
					if(numItemsSub != null)
					{
						this.sysInfo.DCFnode.removeSubFromDictionary(numItemsSub);
					}
					#endregion  remove header and numItems sub from DCF dictionary is required
                }

					#region identify and remove assoc odItem TreeNode and any now redundant TreeNodes
					TreeNode headerTNode = getAssocODItemTreeNode(this.DCFCustList_TN, subToRemove.indexNumber); //get the loewst level DCF treenode for this odSub
					if (headerTNode != null)
					{
						headerTNode.Remove(); //need to remove this first for flushTreeViewDeadEnds to work
						do
						{
							nodesRemovedFlag = false;  //set false before entry
							//if we remove any nodes in this pass then it will be set to true
							removeRedundantSecionNodes(this.DCFCustList_TN, MAIN_WINDOW.DCFTblIndex);
						}
						while (nodesRemovedFlag == true);  //chekc at end set inside method
					}
					#endregion identify and remove assoc odItem TreeNode and any now redundant TreeNodes
					#region if DCF Bcuket is now empty and was sourced from file then allow items to be dragged in again
					if(this.DCFCustList_TN.Nodes.Count == 0)
					{
						this.DCFCustList_TN.Text = "DCF store (empty)";
						this.DCFFromfile = false;  //OK to drag stuff in again
					}
					#endregion if DCF Bcuket is empty and was sources from file then allow items to be dragged in again
					#region remove DCFtable header row
					if(headerRow != null)
					{
						DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].Rows.Remove(headerRow); //do last 
					}
					#endregion remove DCFtable header row
					#endregion removeing all remaining parts of the odItem
				
				#region finally remove DCF row for subToRemove - DO LAST -we lose pointer to subToRemove when we remove the row
				DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].Rows.Remove(dcfRow); //do last 
				#endregion finally remove DCF row for subToRemove - DO LAST -we lose pointer to subToRemove when we remove the row
				this.updateRowColoursArray();   //why? - compare maybe so leave in  
				this.showUserControls();
				#endregion remove odSub from DCF
			}
			else if (menuText == "Add this item to DCF store")
			{
				#region identify the treenode corresponding to this item or its header row (if applicable)
				DataView tmpView = (DataView) this.dataGrid1.DataSource; //dig out the underlying DataView
				DataRow TblRow = tmpView[this.selectedDgRowInd].Row;  //independent of datagrid columns being displayed
				ODItemData odSubtoAdd = (ODItemData) TblRow[(int) TblCols.odSub];

				TreeNode equivDraggedNode = getAssocODItemTreeNode(this.treeView1.SelectedNode, odSubtoAdd.indexNumber);
				if (equivDraggedNode != null)
				{
					this.addItemToCustomList(equivDraggedNode, this.DCFCustList_TN, false);
				}
				else
				{
					this.showUserControls();
				}
				#endregion identify the treenode corresponding to this item or its header row (if applicable)
			}
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("Not all sub index settings updated:");
			}
		}

		private void removeDCFTreeNodes(TreeNode nodeToRemove)
		{
			this.hideUserControls();
			//right mouse click now forces selection of the node so the code ovehead her eis less and pre-assumes that
			// the node we are interested in is the treeview selectedNode
			if(nodeToRemove == this.DCFCustList_TN) 
			{
				#region remove all DCF child nodes and clear DCF table
				nodeToRemove.Nodes.Clear();
				MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].Clear();
				this.sysInfo.DCFnode.objectDictionary = new ArrayList(); //clear it out 
				#endregion remove all DCF child nodes and clear DCF table
			}
			else 
			{
				#region clear out a single branch and its corresponding table rows
				//find corresponding rows in table and remove them
				treeNodeTag DNtag = (treeNodeTag) nodeToRemove.Tag;
				//create a collletion of rows to be deleted
				ArrayList rowsToGo = new ArrayList();
				childTreeNodeList = new ArrayList();
				if(DNtag.assocSub != null)
				{
					childTreeNodeList.Add(DNtag);
				}
				this.getChildODItems(nodeToRemove, false); 
				for(int i = 0;i<childTreeNodeList.Count;i++)
				{
					treeNodeTag tag = (treeNodeTag) childTreeNodeList[i];
					if( tag.assocSub != null)  //only consider TreeNodes with assoc OD items
					{
						ObjDictItem itemToRemove = this.sysInfo.DCFnode.getODItemAndSubs(tag.assocSub.indexNumber);
						if(itemToRemove != null)
						{
							this.sysInfo.DCFnode.removeODItemFromDictionary(itemToRemove);
						}
						foreach(DataRow row in MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].Rows)							
						{
							ODItemData odRowSub = (ODItemData) row[(int)TblCols.odSub];
							if(odRowSub.indexNumber == tag.assocSub.indexNumber)
							{
								rowsToGo.Add(row);
							}
						}
					}
				}
				//now step throught this collection - note Numbe rof itmes in this colection 
				//deosn't change when we delete items form anothe rocllection - avoids the dual 
				//counter scenario
				foreach(DataRow row in rowsToGo)
				{
					MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.DCFTblIndex].Rows.Remove(row);
				}
				#endregion clear out a single branch and its corresponding table rows
				bool Complete = false;
				while(Complete == false)
				{
					//now remove the tree Node and its children plus any redundant parent nodes
					TreeNode tempNode = nodeToRemove.Parent;
					if(tempNode != null)
					{
						nodeToRemove.Remove();
						nodeToRemove = tempNode;  //jump to it parent
					}
					//either we have reached the root node or this node is not redundant
					if((tempNode == null) || (tempNode.Nodes.Count != 0) )
					{
						Complete = true;
					}
				}
			}
			#region if DCF Bcuket is empty and was sources from file then allow items to be dragged in again
			if(this.DCFCustList_TN.Nodes.Count == 0)
			{
				this.DCFCustList_TN.Text = "DCF store (empty)";
				this.DCFFromfile = false;  //OK to drag stuff in again
			}
			#endregion if DCF Bcuket is empty and was sources from file then allow items to be dragged in again
			this.showUserControls();  //brings correct panel back to front
			this.dataGrid1.Refresh();  //needed or datagrid does not update
		}
		private void removeNodeFromDCF_Click(object sender, EventArgs e)
		{
			string path = this.treeView1.SelectedNode.FullPath;
			//remove the top part tof path and replace with 
			int temp = path.IndexOf(this.treeView1.PathSeparator);
			if(temp != -1)
			{
				path = this.DCFCustList_TN.Text + path.Substring(temp);
				this.currentTreenode = null;
				isNodePathInTreeLeg(this.DCFCustList_TN, path);
				if(this.currentTreenode != null)
				{
					this.removeDCFTreeNodes(this.currentTreenode); //set in isNodePathInTreeLeg
					//currentTreenode is set in isNodePathInTreeLeg() 
					//to be equivalent tree node in DCF Store
				}
			}
		}
		private void DCF_combo_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			if(sender.GetType().Equals( typeof( ComboBox ) ))
			{
				ComboBox CB = (ComboBox) sender;
				this.productCodeCB.SelectionChangeCommitted -=new EventHandler(DCF_combo_SelectionChangeCommitted);
				this.revNoCB3.SelectionChangeCommitted-=new EventHandler(DCF_combo_SelectionChangeCommitted);
				this.productCodeCB.SelectedIndex = CB.SelectedIndex;
				this.revNoCB3.SelectedIndex = CB.SelectedIndex;
				this.productCodeCB.SelectionChangeCommitted +=new EventHandler(DCF_combo_SelectionChangeCommitted);
				this.revNoCB3.SelectionChangeCommitted +=new EventHandler(DCF_combo_SelectionChangeCommitted);
			}
		}

		#endregion DCF related methods

		#region display Datalog related metohds
		private void exportDomainMI_Click(object sender, EventArgs e)
		{
			this.hideUserControls();
			foreach(nodeInfo node in sysInfo.nodes)
			{
				if((SCCorpStyle.ProductRange)node.productRange == (SCCorpStyle.ProductRange.SEVCONDISPLAY))
				{
					if((node.displayDataLogDomainSub != null) && (node.displayDatalogIntervalSub != null))
					{
						#region read domain and logging interval and display any read errors
						//put the domain read onto a seperate thread
							node.readODValue(node.displayDatalogIntervalSub);
						if(SystemInfo.errorSB.Length>0)
						{
							this.sysInfo.displayErrorFeedbackToUser("Unable to retrieve datalog");
						}
						else
						{
							#region data retrieval thread
							this.progressBar1.Value = this.progressBar1.Minimum;
							this.statusBarPanel3.Text = "Retrieving datalog...";
							this.progressBar1.Maximum = this.sysInfo.totalItemsInAllODs;
								dataRetrievalThread = new Thread(new ThreadStart( readDisplayDataLog)); 
							dataRetrievalThread.Name = "Display DataLog Retrieval";
							dataRetrievalThread.IsBackground = true;
                            dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
							System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif
							dataRetrievalThread.Start(); 
							this.timer2.Enabled = true;
							#endregion data retrieval thread
						}
						#endregion read domain and display any read errors
					}
					break;
				}
			}
		}

		private void exportFLDomainMI_Click(object sender, EventArgs e)
		{
			this.hideUserControls();
			foreach(nodeInfo node in sysInfo.nodes)
			{
				if((SCCorpStyle.ProductRange)node.productRange == (SCCorpStyle.ProductRange.SEVCONDISPLAY))
				{
					if(node.displayFaultlogDomainSub != null)
					{
						#region read domain and logging interval and display any read errors
						//put the domain read onto a seperate thread
						#region data retrieval thread
						this.progressBar1.Value = this.progressBar1.Minimum;
						this.statusBarPanel3.Text = "Retrieving faultlog...";
						this.progressBar1.Maximum = this.sysInfo.totalItemsInAllODs;
						dataRetrievalThread = new Thread(new ThreadStart( readDisplayFaultLog)); 
						dataRetrievalThread.Name = "Display FaultLog Retrieval";
						dataRetrievalThread.IsBackground = true;
                        dataRetrievalThread.Priority = ThreadPriority.Normal;
#if DEBUG
						System.Console.Out.WriteLine("Thread: " + dataRetrievalThread.Name + " started");
#endif
						dataRetrievalThread.Start(); 
						this.timer2.Enabled = true;
						#endregion data retrieval thread
						#endregion read domain and display any read errors
					}
					break;
				}
			}

		}


		private void readDisplayFaultLog( )
		{
			foreach(nodeInfo node in sysInfo.nodes)
			{
				if((SCCorpStyle.ProductRange)node.productRange == (SCCorpStyle.ProductRange.SEVCONDISPLAY))
				{
					node.readODValue(node.displayFaultlogDomainSub);
					break;
		}
			}
		}

		private void readDisplayDataLog( )
		{
			foreach(nodeInfo node in sysInfo.nodes)
			{
				if((SCCorpStyle.ProductRange)node.productRange == (SCCorpStyle.ProductRange.SEVCONDISPLAY))
				{
					node.readODValue(node.displayDataLogDomainSub);
					break;
				}
			}
		}
		private void processReceviedDataLog()
		{
            bool isMetric = true;  //DR38000271
			if(MAIN_WINDOW.isVirtualNodes == true)
			{
				this.sysInfo.displayErrorFeedbackToUser("Display Datalog upload not available in Virtual mode");
				return;
			}
			foreach(nodeInfo node in sysInfo.nodes)
			{
				if((SCCorpStyle.ProductRange)node.productRange == (SCCorpStyle.ProductRange.SEVCONDISPLAY))
				{
					if( (node.displayDatalogIntervalSub == null) 
						|| (node.displayDataLogDomainSub == null)
						|| (node.displayDataLogDomainSub.currentValueDomain == null))
					{
						SystemInfo.errorSB.Append("\nExpected datalog item not found. Please report");
						this.sysInfo.displayErrorFeedbackToUser("Error when processing datalog");
						return;

					}
					else if(node.displayDataLogDomainSub.currentValueDomain.Length <= 4) 
					{
						this.sysInfo.displayErrorFeedbackToUser("Display datalog is empty");
						return;
					}
					else if((node.displayDataLogDomainSub.currentValueDomain.Length % 68) != 0)
			{
						SystemInfo.errorSB.Append("\nInvalid dataLog length of ");
						SystemInfo.errorSB.Append(node.displayDataLogDomainSub.currentValueDomain.Length.ToString());
				this.sysInfo.displayErrorFeedbackToUser("Error when processing datalog");
				return;
			}
			ArrayList dataLog = new ArrayList();
			short offset = 108;  //TODO - this needs to be an OD itme in the display valueToAdd += (128 - abs(chartConfig->yMin));
					uint numDomainPages  = (uint) (node.displayDataLogDomainSub.currentValueDomain.Length/68); //jude change this to a define - or read it from OD
			#region fill the receiving structure
			uint byteCtr = 0;
			for(ushort i = 0;i<numDomainPages;i++)
			{
				DisplayDataLogEntry entry0 = new DisplayDataLogEntry();  //entry0 will contain the time stamp form dispaly - remaing timestamps are calculated from this one
				#region create a DateTime struct form the date and time date from display
						ushort year= ( ushort) node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
						year |= (ushort) ((node.displayDataLogDomainSub.currentValueDomain[byteCtr++]) <<8 );
						byte Month = node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
						byte Day = node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
						byte Hour = node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
						byte Minute = node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
						byte Second = node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
						try
						{//always create DataTime in try catch when using externally sourced data
				entry0.dateAndTime = new DateTime(year, Month, Day, Hour, Minute, Second);
						}
						catch
						{
							StringBuilder dataStr = new StringBuilder();
							dataStr.Append("\nInvalid date and time received: Date: (DD/MM/YY)");
							dataStr.Append(Day.ToString().PadLeft(2,'0'));
							dataStr.Append(":");
							dataStr.Append(Month.ToString().PadLeft(2,'0'));
							dataStr.Append(":");
							dataStr.Append(year.ToString());
							dataStr.Append (" time:");
							dataStr.Append(Hour.ToString().PadLeft(2,'0'));
							dataStr.Append(":");
							dataStr.Append(Minute.ToString().PadLeft(2,'0'));
							dataStr.Append(":");
							dataStr.Append(Second.ToString().PadLeft(2,'0'));
							SystemInfo.errorSB.Append(dataStr.ToString());
						}

				#endregion create a DateTime struct form the date and time date from display
				//the next byte from the display tells us how many valid entries we have
						byte valid_entry_count = node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
				if(valid_entry_count >0)
				{
					isMetric = true;
					ODItemData metricImpSub = node.displayDataLogDomainSub.CANnode.getODSub(0x2E00, 1);
					if(metricImpSub != null)
					{
								metricImpSub.CANnode.readODValue(metricImpSub);
						isMetric = System.Convert.ToBoolean(metricImpSub.currentValue);
					}
					
					#region add the parameter values to the first entry in this page
					if(isMetric == false)
					{
						//the next 60 bytes contain valid and non valid( zero entries) - we are only interested in the valid ones
								entry0.Param1 = convertToFarenheit( (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
						entry0.Param1 = (float) System.Math.Round(entry0.Param1, 5);

								sbyte param2 =  (sbyte) (node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
						entry0.Param2 = convertToFarenheit(param2 + offset); //remove display offset - needs to be OD item and read on the fly
						entry0.Param2 = (float) System.Math.Round(entry0.Param2, 5);

								entry0.Param3 = convertToFarenheit((sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
						entry0.Param3 = (float) System.Math.Round(entry0.Param3, 5);

								sbyte param4 =  (sbyte) (node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
						entry0.Param4 = convertToFarenheit(param4 + offset);
						entry0.Param4 = (float) System.Math.Round(entry0.Param4, 5);

								entry0.Param5 = convertToFarenheit((sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
						entry0.Param5 = (float) System.Math.Round(entry0.Param5, 5);

								sbyte param6 =  (sbyte) (node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
						entry0.Param6 = convertToFarenheit(param6 + offset);
						entry0.Param6 = (float) System.Math.Round(entry0.Param6, 5);
					}
					else
					{
								entry0.Param1 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
								entry0.Param2 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++] + offset;;
								entry0.Param3 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
								entry0.Param4 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++] + offset;
								entry0.Param5 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
								entry0.Param6 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++] + offset;
					}
							dataLog.Add(entry0);  //add this entry to the page
					#endregion add the parameter values to the first entry in this page
					//now create the remaing entries (if any on this page) - generate the correc titme and date and 
					for(int entryCtr = 1;entryCtr<valid_entry_count;entryCtr++)
					{
						#region create new entry and timestamp it
						DisplayDataLogEntry entry = new DisplayDataLogEntry();
						entry.dateAndTime = entry0.dateAndTime; //we can do this becasue entry0.dateAndTime is a struct
								entry.dateAndTime = entry.dateAndTime.AddSeconds((entryCtr * node.displayDatalogIntervalSub.currentValue));  //not add the time offset
						#endregion create new entry and timestamp it
						#region add the parameter vlaues
						if(isMetric == false)
						{
							//the next 60 bytes contain valid and non valid( zero entries) - we are only interested in the valid ones
									entry.Param1 = convertToFarenheit( (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
							entry.Param1 = (float) System.Math.Round(entry.Param1, 5);

									sbyte param2 =  (sbyte) (node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
							entry.Param2 = convertToFarenheit(param2 + offset); //remove display offset - needs to be OD item and read on the fly
							entry.Param2 = (float) System.Math.Round(entry.Param2, 5);

									entry.Param3 = convertToFarenheit((sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
							entry.Param3 = (float) System.Math.Round(entry.Param3, 5);

									sbyte param4 =  (sbyte) (node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
							entry.Param4 = convertToFarenheit(param4 + offset);
							entry.Param4 = (float) System.Math.Round(entry.Param4, 5);

									entry.Param5 = convertToFarenheit((sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
							entry.Param5 = (float) System.Math.Round(entry.Param5, 5);

									sbyte param6 =  (sbyte) (node.displayDataLogDomainSub.currentValueDomain[byteCtr++]);
							entry.Param6 = convertToFarenheit(param6 + offset);
							entry.Param6 = (float) System.Math.Round(entry.Param6, 5);
						}
						else
						{
									entry.Param1 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
									entry.Param2 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++] + offset;;
									entry.Param3 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
									entry.Param4 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++] + offset;
									entry.Param5 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++];
									entry.Param6 = (sbyte)node.displayDataLogDomainSub.currentValueDomain[byteCtr++] + offset;
						}
						#endregion add the parameter vlaues
						#region add the new entry to our page
						dataLog.Add(entry);  //add this entry to the page
						#endregion add the new entry to our page
					}
					byteCtr += (uint) ((10- valid_entry_count) * 6); //skip over any non-valid entries
				}
			}
			#endregion fill the receiving structure

			#region get the filename
			saveFileDialog1.FileName = "";
					if ( Directory.Exists( MAIN_WINDOW.UserDirectoryPath + @"\DisplayLogs\Data logs" ) == false )
			{
				try
				{
							Directory.CreateDirectory( MAIN_WINDOW.UserDirectoryPath + @"\DisplayLogs\Data logs" );
				}
				catch{}
			}
			#region tes twhethe rExcel is avaialble on host machine
			bool ExcelAvailable = true;
#if USING_OFFICE2007
            Microsoft.Office.Interop.Excel.Application excelApp = null;
#else
			Excel.Application excelApp = null;
#endif
			try
			{
#if USING_OFFICE2007
                excelApp = new Microsoft.Office.Interop.Excel.Application();
#else
                excelApp = new Excel.ApplicationClass();
#endif
			}
			catch
			{  
				ExcelAvailable = false;
			}
			#endregion tes twhethe rExcel is avaialble on host machine
			saveFileDialog1.FileName = ""; //reset
            if(ExcelAvailable == true)
			{
				saveFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\DisplayLogs\Data logs";
				saveFileDialog1.Title = "Save datalog file to DriveWizard host";
				saveFileDialog1.Filter = "excel files (*.xls)|*.xls" ;
				saveFileDialog1.DefaultExt = "xls";
				saveFileDialog1.CheckFileExists = false; //let excel do this 
				saveFileDialog1.ShowDialog();	
				if(saveFileDialog1.FileName != "")
				{
					#region create and sav ethe Excel file
	//				excelApp.Visible = true;  //judetemp - just for now
#if USING_OFFICE2007
                    Microsoft.Office.Interop.Excel.Workbook excelWorkbook = excelApp.Workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
                    Microsoft.Office.Interop.Excel.Sheets excelSheets = excelWorkbook.Worksheets;
					string currentSheet = "Sheet1";
                    Microsoft.Office.Interop.Excel.Worksheet excelWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelSheets.get_Item(currentSheet);

#else
                    Excel.Workbook excelWorkbook = excelApp.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                    Excel.Sheets excelSheets = excelWorkbook.Worksheets;
					string currentSheet = "Sheet1";
                    Excel.Worksheet excelWorksheet = (Excel.Worksheet)excelSheets.get_Item(currentSheet);
#endif
					//Excel.Range excelCell = (Excel.Range)excelWorksheet.get_Range("A1", "Z10");
                    //DR38000271
                    string[] metricUnits = { "", "", "deg C", "deg C", "deg C", "deg C", "deg C", "deg C" };
                    string[] imperialUnits = { "", "", "deg F", "deg F", "deg F", "deg F", "deg F", "deg F" };
                    string[] titles = { "Date", "Time", "Trac 1 Heatsink Temp", "Trac 1 Motor Temp", "Trac 2 Heatsink Temp", "Trac 2 Motor temp", "Pump Heatsink Temp", "Pump Motor Temp" };

                    for (int i = 0; i < titles.Length; i++)
                    {
                        excelApp.Cells[1, (i + 1)] = titles[i];

                        if (isMetric == true) //DR38000271
                        {
                            excelApp.Cells[2, (i + 1)] = metricUnits[i];
                        }
                        else
                        {
                            excelApp.Cells[2, (i + 1)] = imperialUnits[i];
                        }
                    }

                    uint rowNum = 3; //DR38000271 now data starts at row 3
					foreach( DisplayDataLogEntry entry in dataLog)
					{
						excelApp.Cells[rowNum, 1] = entry.dateAndTime.ToLongDateString();
						excelApp.Cells[rowNum, 2] = entry.dateAndTime.ToLongTimeString();
						excelApp.Cells[rowNum, 3] = entry.Param1;
						excelApp.Cells[rowNum, 4] = entry.Param2;
						excelApp.Cells[rowNum, 5] = entry.Param3;
						excelApp.Cells[rowNum, 6] = entry.Param4;
						excelApp.Cells[rowNum, 7] = entry.Param5;
						excelApp.Cells[rowNum, 8] = entry.Param6;
						rowNum++;
					}
					try
					{
#if USING_OFFICE2007
                        excelWorksheet.SaveAs(saveFileDialog1.FileName, Microsoft.Office.Interop.Excel.XlFileFormat.xlExcel2, null, null, false, false, true, null, null, false);
#else
                        excelWorksheet.SaveAs(saveFileDialog1.FileName, Excel.XlFileFormat.xlExcel2, null, null, false, false, true, null, null, false);
#endif
					}
					catch
					{
						this.sysInfo.displayErrorFeedbackToUser("Unable to save. CLose open file and retry");
					}
					excelApp.Quit();
					try
					{
						int test = 0;
						do
						{
							test = System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
						}
						while (test >0);
					}
					catch{} //we MUSt use try/cathc here we are using the null exception to tell us that we are done
					#endregion create and sav ethe Excel file
				}
			}
			else
			{
				//no excel so create a CSV file
				#region open file stream writer
				saveFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\DCF";
				saveFileDialog1.Title = "Save datalog file to DriveWizard host";
				saveFileDialog1.Filter = "txt files (*.txt)|*.txt" ;
				saveFileDialog1.DefaultExt = "txt";
				saveFileDialog1.ShowDialog();	
				if(saveFileDialog1.FileName != "")
				{

					FileStream	dataLogfs = new FileStream( saveFileDialog1.FileName, System.IO.FileMode.Create, FileAccess.Write,FileShare.Read );
					StreamWriter dataLogSw = null;
					if(dataLogfs != null) 
					{
						dataLogSw = new StreamWriter( dataLogfs );
					}
					if(dataLogSw != null)
					{
                        //DR38000271
						dataLogSw.WriteLine( "Date,Time,Trac 1 Heatsink Temp, Trac 1 Heatsink Temp,Pump Heatsink Temp,Trac 1 Motor temp,Trac 2 Motor Temp,Pump Motor Temp");
                        if(isMetric == true)
                        {
                            dataLogSw.WriteLine( " , , deg C, deg C, deg C, deg C, deg C, deg C ");
                        }
                        else
                        {
                            dataLogSw.WriteLine( " , , deg F, deg F, deg F, deg F, deg F, deg F ");
                        }

						dataLogSw.WriteLine();
						foreach( DisplayDataLogEntry entry in dataLog)
						{
							dataLogSw.Write(entry.dateAndTime.ToShortDateString());
							dataLogSw.Write(",");
							dataLogSw.Write(entry.dateAndTime.ToLongTimeString());
							dataLogSw.Write(",");
							dataLogSw.Write(entry.Param1);
							dataLogSw.Write(",");
							dataLogSw.Write(entry.Param2);
							dataLogSw.Write(",");
							dataLogSw.Write(entry.Param3);
							dataLogSw.Write(",");
							dataLogSw.Write(entry.Param4);
							dataLogSw.Write(",");
							dataLogSw.Write(entry.Param5);
							dataLogSw.Write(",");
							dataLogSw.Write(entry.Param6);
							dataLogSw.Write(",");
							dataLogSw.WriteLine();
						}
						#region flush and close streams
						dataLogSw.Flush();
						dataLogSw.Close();
						dataLogfs.Close();
						#endregion flush and close streams
						Message.Show("File saved OK. \nUse menu Data/Import Extenal Data in Excel. \nNote Data is comma seperated");
					}
				}
				#endregion
			}
			#endregion get the filename
					break;				
				}
				}
		}

		private void processReceviedFaultLog()
		{
			if(MAIN_WINDOW.isVirtualNodes == true)
			{
				this.sysInfo.displayErrorFeedbackToUser("Display Faultlog upload not available in Virtual mode");
				return;
			}

			#region match nodeIDs to display Client SDO channels
			//The display hast fixed SDO LCient channels the firs tchannel is always Trac 1 , secons Trac 2 and third 
			ArrayList nodeIDsBychannel = new ArrayList();
			foreach(nodeInfo node in sysInfo.nodes)
			{
				if((SCCorpStyle.ProductRange)node.productRange == (SCCorpStyle.ProductRange.SEVCONDISPLAY))
				{
					for(ushort offset = 0; offset < 7;offset++) //one less this time we are only interested in actual channels
					{
						displayChannels channelDef = new displayChannels();
						channelDef.nodeID = 0;
						ODItemData odSub = node.getODSub((0x1280 + offset),3);
						if(odSub != null)
						{
							node.readODValue(odSub);
							channelDef.nodeID = (ushort) odSub.currentValue;
						}
						switch (offset)
						{
							case 0:
								channelDef.nodeName = "Traction 1";
								break;
							case 1:
								channelDef.nodeName = "Traction 2";
								break;
							case 2:
								channelDef.nodeName = "Pump";
								break;
							case 3:
								channelDef.nodeName = "I/O unit 1";
								break;
							default:
								channelDef.nodeName = "";
								break;
						}
						nodeIDsBychannel.Add(channelDef);
					}
					displayChannels dispChnl = new displayChannels();
					dispChnl.nodeName = "Display";
					dispChnl.nodeID = (ushort)node.nodeID;
					nodeIDsBychannel.Add(dispChnl);
				}
			}
			#endregion match nodeIDs to display Client SDO channels

			foreach(nodeInfo node in sysInfo.nodes)
			{
				if((SCCorpStyle.ProductRange)node.productRange == (SCCorpStyle.ProductRange.SEVCONDISPLAY))
				{
					if((node.displayFaultlogDomainSub == null) || (node.displayFaultlogDomainSub.currentValueDomain == null))
					{
						SystemInfo.errorSB.Append("\nExpected OD item not found.");
						this.sysInfo.displayErrorFeedbackToUser("Error when processing display faultlog ");
				return;
			}
			ArrayList faultLog = new ArrayList();
					uint numFaultLogentries  = (uint) (node.displayFaultlogDomainSub.currentValueDomain.Length/8); 
					if(numFaultLogentries == 0)
					{
						this.sysInfo.displayErrorFeedbackToUser("Display faultlog is empty");
						return;
					}

			#region fill the receiving structure
			uint byteCtr = 0;
			for(ushort i = 0;i<numFaultLogentries;i++)
			{
				DisplayFaultLogEntry entry0 = new DisplayFaultLogEntry();  //entry0 will contain the time stamp form dispaly - remaing timestamps are calculated from this one
				#region create a DateTime struct form the date and time date from display

				//the fisrt two bytes contain the date
						ushort comp_FaultDate= ( ushort) node.displayFaultlogDomainSub.currentValueDomain[byteCtr++];
						comp_FaultDate |= (ushort) ((node.displayFaultlogDomainSub.currentValueDomain[byteCtr++]) <<8 );

				ushort year = (ushort) (comp_FaultDate >> 9);
				year+= 2000;
				byte Month =  (byte) ((comp_FaultDate & 0x01E0) >> 5);//odSub.currentValueDomain[byteCtr++];
				byte Day = (byte) (comp_FaultDate & 0x001F);//odSub.currentValueDomain[byteCtr++];

				//the next two bytes contin the fault ID
						entry0.faultID= ( ushort) node.displayFaultlogDomainSub.currentValueDomain[byteCtr++];
						entry0.faultID |= (ushort) ((node.displayFaultlogDomainSub.currentValueDomain[byteCtr++]) <<8 );

				//the next 4 bytes contian the NodeID, Count And Time
				//fisrt get the 32bit compressed vakue
						uint comp_NodeIDCountAndTime  = ( uint) node.displayFaultlogDomainSub.currentValueDomain[byteCtr++];
						comp_NodeIDCountAndTime  |= ( uint) ((node.displayFaultlogDomainSub.currentValueDomain[byteCtr++]) << 8);
						comp_NodeIDCountAndTime  |= ( uint) ((node.displayFaultlogDomainSub.currentValueDomain[byteCtr++]) << 16);
						comp_NodeIDCountAndTime  |= ( uint) ((node.displayFaultlogDomainSub.currentValueDomain[byteCtr++]) << 24);

				//and then extract the data

				entry0.nodeID = (byte) (comp_NodeIDCountAndTime >> 25);
				entry0.count = (byte)  ((comp_NodeIDCountAndTime & 0x01FE0000) >>  17);
				ushort Hour =  (byte) ((comp_NodeIDCountAndTime & 0x0001f000) >> 12);
				ushort Minute = (byte) ((comp_NodeIDCountAndTime & 0x00000FC0) >> 6);
				ushort Second = (byte) (comp_NodeIDCountAndTime & 0x0000003F);
						try
						{//always create DataTime in try catch when using externally sourced data
				entry0.dateAndTime = new DateTime(year, Month, Day, Hour, Minute, Second);
						}
						catch
						{
							StringBuilder dataStr = new StringBuilder();
							dataStr.Append("\nInvalid date and time received: Date: (DD/MM/YY)");
							dataStr.Append(Day.ToString().PadLeft(2,'0'));
							dataStr.Append(":");
							dataStr.Append(Month.ToString().PadLeft(2,'0'));
							dataStr.Append(":");
							dataStr.Append(year.ToString());
							dataStr.Append (" time:");
							dataStr.Append(Hour.ToString().PadLeft(2,'0'));
							dataStr.Append(":");
							dataStr.Append(Minute.ToString().PadLeft(2,'0'));
							dataStr.Append(":");
							dataStr.Append(Second.ToString().PadLeft(2,'0'));
							SystemInfo.errorSB.Append(dataStr.ToString());
						}
				#endregion create a DateTime struct form the date and time date from display
				faultLog.Add(entry0);
			}
			#endregion fill the receiving structure

			#region get the filename
			saveFileDialog1.FileName = "";
					if ( Directory.Exists( MAIN_WINDOW.UserDirectoryPath + @"\DisplayLogs\Fault logs" ) == false )
			{
				try
				{
							Directory.CreateDirectory( MAIN_WINDOW.UserDirectoryPath + @"\DisplayLogs\Fault logs" );
				}
				catch{}
			}
			#region tes twhethe rExcel is avaialble on host machine
			bool ExcelAvailable = true;
#if USING_OFFICE2007
            Microsoft.Office.Interop.Excel.Application excelApp = null;
            try
            {
                excelApp = new Microsoft.Office.Interop.Excel.ApplicationClass();
            }
            catch
            {
                ExcelAvailable = false;
            }

#else
            Excel.Application excelApp = null;
			try
			{
                excelApp = new Excel.ApplicationClass();
			}
			catch
			{  
				ExcelAvailable = false;
			}
#endif
			#endregion tes twhethe rExcel is avaialble on host machine
			saveFileDialog1.FileName = ""; //reset

					//now match the dislay channels to Node IDs
			if(ExcelAvailable == true)
			{
						saveFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\DisplayLogs\Fault logs";
				saveFileDialog1.Title = "Save datalog file to DriveWizard host";
				saveFileDialog1.Filter = "excel files (*.xls)|*.xls" ;
				saveFileDialog1.DefaultExt = "xls";
				saveFileDialog1.CheckFileExists = false; //let excel do this 
				saveFileDialog1.ShowDialog();	
				if(saveFileDialog1.FileName != "")
				{
					#region create and sav ethe Excel file
#if USING_OFFICE2007
                    //excelApp.Visible = true;  //judetemp - just for now
                    Microsoft.Office.Interop.Excel.Workbook excelWorkbook = excelApp.Workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
                    Microsoft.Office.Interop.Excel.Sheets excelSheets = excelWorkbook.Worksheets;
                    string currentSheet = "Sheet1";
                    Microsoft.Office.Interop.Excel.Worksheet excelWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelSheets.get_Item(currentSheet);
                    //Excel.Range excelCell = (Excel.Range)excelWorksheet.get_Range("A1", "Z10");

#else
					//				excelApp.Visible = true;  //judetemp - just for now
                    Excel.Workbook excelWorkbook = excelApp.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
Excel.Sheets excelSheets = excelWorkbook.Worksheets;
					string currentSheet = "Sheet1";
                    Excel.Worksheet excelWorksheet = (Excel.Worksheet)excelSheets.get_Item(currentSheet);
					//Excel.Range excelCell = (Excel.Range)excelWorksheet.get_Range("A1", "Z10");
#endif
							string [] titles = {"Date","Time","Node","Fault ID", "Count"};
					for(int i = 0;i< titles.Length;i++)
					{
						excelApp.Cells[1, (i+1)] = titles[i];
					}
					uint rowNum = 2;
					foreach( DisplayFaultLogEntry entry in faultLog)
					{
						excelApp.Cells[rowNum, 1] = entry.dateAndTime.ToLongDateString();
						excelApp.Cells[rowNum, 2] = entry.dateAndTime.ToLongTimeString();
								//the follow ing is not ideal - it should read the Client 
								excelApp.Cells[rowNum, 3] = "Node ID:" + entry.nodeID.ToString();
								foreach(displayChannels chnl in nodeIDsBychannel)
								{
									if(chnl.nodeID == entry.nodeID)
									{
										excelApp.Cells[rowNum, 3] = chnl.nodeName + "(" + entry.nodeID.ToString() + ")"; //use an descrptive string of the item for user friendliness
										break; //then get out
									}
								}
								excelApp.Cells[rowNum, 4] = "0x" + entry.faultID.ToString("X");
								excelApp.Cells[rowNum, 5] = entry.count;
						rowNum++;
					}
					try
					{
#if USING_OFFICE2007
						excelWorksheet.SaveAs(saveFileDialog1.FileName,Microsoft.Office.Interop.Excel.XlFileFormat.xlExcel2,null,null,false,false,true,null,null,false);
#else
					excelWorksheet.SaveAs(saveFileDialog1.FileName,Excel.XlFileFormat.xlExcel2,null,null,false,false,true,null,null,false);
#endif
                    }
					catch
					{
						this.sysInfo.displayErrorFeedbackToUser("Unable to save. CLose open file and retry");
					}
					excelApp.Quit();
					try
					{
						int test = 0;
						do
						{
							test = System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
						}
						while (test >0);
					}
					catch{} //we MUSt use try/cathc here we are using the null exception to tell us that we are done
					#endregion create and sav ethe Excel file
				}
			}
			else
			{
				//no excel so create a CSV file
				#region open file stream writer
				saveFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\DisplayLogs";
				saveFileDialog1.Title = "Save faultlog file to DriveWizard host";
				saveFileDialog1.Filter = "txt files (*.txt)|*.txt" ;
				saveFileDialog1.DefaultExt = "txt";
				saveFileDialog1.ShowDialog();	
				if(saveFileDialog1.FileName != "")
				{

					FileStream	faultLogfs = new FileStream( saveFileDialog1.FileName, System.IO.FileMode.Create, FileAccess.Write,FileShare.Read );
					StreamWriter faultLogSw = null;
					if(faultLogfs != null) 
					{
						faultLogSw = new StreamWriter( faultLogfs );
					}
					if(faultLogSw != null)
					{
						faultLogSw.WriteLine( "Date,Time,Fault ID, Count");
						faultLogSw.WriteLine();
						foreach( DisplayFaultLogEntry entry in faultLog)
						{
							faultLogSw.Write(entry.dateAndTime.ToShortDateString());
							faultLogSw.Write(",");
							faultLogSw.Write(entry.dateAndTime.ToLongTimeString());
							faultLogSw.Write(",");
									bool strFound = false;
									foreach(displayChannels chnl in nodeIDsBychannel)
									{
										if(chnl.nodeID == entry.nodeID)
										{
											faultLogSw.Write(chnl.nodeName);
											faultLogSw.Write("(");
											faultLogSw.Write(entry.nodeID.ToString());
											faultLogSw.Write(")");
											faultLogSw.Write(",");
											strFound = true;
											break; //then get out
										}
									}
									if(strFound == false)
									{
										faultLogSw.Write("Node ID:" + entry.nodeID.ToString());
										faultLogSw.Write(",");
									}
							faultLogSw.Write("0x" + entry.faultID.ToString("X"));
							faultLogSw.Write(",");
							faultLogSw.Write(entry.count);
							faultLogSw.Write(",");
							faultLogSw.WriteLine();
						}
						#region flush and close streams
						faultLogSw.Flush();
						faultLogSw.Close();
						faultLogfs.Close();
						#endregion flush and close streams
						Message.Show("File saved OK. \nUse menu Data/Import Extenal Data in Excel. \nNote Data is comma seperated");
					}
				}
				#endregion
			}
			#endregion get the filename
					break;
				}
			}

		}

		float convertToFarenheit(int valueInDegC)
		{
			float imperialValue = (float) (valueInDegC * 1.8);
			imperialValue += 32;
			return (imperialValue);
		}

		#endregion display Datalog related metohds

#if NEW_SYSTEM_PDOS
		#region System PDO Relatred Methods
		#region collate the CANNodes that are suitalble for COB setup/dispaly ie no Unknowns, bootloaders or Self Char
		private void createPDOableNodesList()
		{
			PDOableCANNodes = new ArrayList();
			for(int i = 0;i<this.sysInfo.nodes.Length;i++)
			{
				if(sysInfo.nodes[i].manufacturer == Manufacturer.THIRD_PARTY)
				{
					PDOableCANNodes.Add(this.sysInfo.nodes[i]);
					//					this.sysInfo.nodes[i].itemBeingRead = 0;
				}
				else if(sysInfo.nodes[i].manufacturer == Manufacturer.SEVCON)
				{
					if(this.sysInfo.nodes[i].isSevconApplication() == true) 
					{
						PDOableCANNodes.Add(sysInfo.nodes[i]); //add them al in - user could change the state - just don-t allow user to edit a non-pre op one
						//						this.sysInfo.nodes[i].itemBeingRead = 0;
					}
					else 
					{
						if((this.sysInfo.nodes[i].isSevconBootloader() == false) 
							&& (this.sysInfo.nodes[i].isSevconSelfChar() == false))
						{//PST etc
							PDOableCANNodes.Add(sysInfo.nodes[i]);
							//							this.sysInfo.nodes[i].itemBeingRead = 0;
						}
					}
				}
			}
			if(this.PDOableCANNodes.Count <= 0)
			{
				SystemInfo.errorSB.Append("\nNo nodes in system to setup PDOs for");
				this.exitPDOScreen();
				return;
			}
			createPrefilledPDOMappings(); //do even if not Sevcon master  - user can set up a slave on a bench
			if(this.SevconMasterNode != null)  //ie the Rx one will not be null weithe rhere
			{
				#region move the Sevcon master to begining of Arraylist - to force it to be th etop sdcreen node
				//although the DI now nove the first Sevcon master it sees to top of nodes array 
				//user could have changed the master status of any node since - really we need to chage nodes to an ArrayLsit and on
				//each access use IndexOf to make code independent of array order
				if(this.PDOableCANNodes.IndexOf(this.SevconMasterNode) != 0)
				{
					this.PDOableCANNodes.Remove(this.SevconMasterNode);
					this.PDOableCANNodes.Insert(0, this.SevconMasterNode);
				}
				#endregion move the Sevcon master to begining of Arraylist - to force it to be th etop sdcreen node
				AddInternalTxPDOReferencesToSystemPDOs();
				AddInternalRxPDOReferencesToSystemPDOs();
				this.currTxPDOnode = this.SevconMasterNode;
				this.currRxPDOnode = this.SevconMasterNode;
			}
			else 
			{//if no Sevcon master node then set curren tnode to be first node
				this.currTxPDOnode = (nodeInfo)this.PDOableCANNodes[0];
				this.currRxPDOnode = (nodeInfo)this.PDOableCANNodes[0];
			}
		}
		#endregion collate the CANNodes that are suitalble for COB setup/dispaly ie no Unknowns, bootloaders or Self Char
		#region create pre-filled mapping sets based on node type and number/location of motor profiles
		private void createPrefilledPDOMappings()
		{
			int numOfPreFilledPDOs = 4;   //currently 4 motr ones
			for(int i = 0;i<this.sysInfo.nodes.Length;i++)
			{
				if(this.sysInfo.nodes[i].isSevconApplication() == true)
				{
					if(this.sysInfo.nodes[i] == this.SevconMasterNode)
					{
						#region Sevcon master
						#region Tx PDO pre-set Master mappings
						//the Sevcon master needs the full set of pre-filled motor PDOs
						this.SevconMasterNode.preFilledTxMotorSPDOMappings = new ArrayList();
						int [] subsTx = {1,3,5};
						int [] indexes = new int [3];
						for(int j = 0;j<indexes.Length;j++)
						{
							indexes[j] = 0x2020;
						}
						this.SevconMasterNode.preFilledTxMotorSPDOMappings.Add(addMappFillings(SevconMasterNode, indexes, subsTx)); //always add even if empty - allows for easier unmapping later
						for(int j = 0;j<indexes.Length;j++)
						{
							indexes[j] = 0x2021;
						}
						this.SevconMasterNode.preFilledTxMotorSPDOMappings.Add(addMappFillings(SevconMasterNode, indexes, subsTx)); //always add even if empty - allows for easier unmapping later
						for(int j = 0;j<indexes.Length;j++)
						{
							indexes[j] = 0x2040;
						}
						this.SevconMasterNode.preFilledTxMotorSPDOMappings.Add(addMappFillings(SevconMasterNode, indexes, subsTx)); //always add even if empty - allows for easier unmapping later
						for(int j = 0;j<indexes.Length;j++)
						{
							indexes[j] = 0x2060;
						}
						this.SevconMasterNode.preFilledTxMotorSPDOMappings.Add(addMappFillings(SevconMasterNode, indexes, subsTx)); //always add even if empty - allows for easier unmapping later
						#endregion Tx PDO pre-set Master mappings
						#region Rx PDO pre-set MAster mappings
						this.SevconMasterNode.preFilledRxMotorSPDOMappings = new ArrayList();
						int [] subsRx = {2, 4, 6};
						for(int j = 0;j<indexes.Length;j++)
						{
							indexes[j] = 0x2020;
						}
						this.SevconMasterNode.preFilledRxMotorSPDOMappings.Add(addMappFillings(SevconMasterNode, indexes, subsRx)); //always add even if empty - allows for easier unmapping later
						for(int j = 0;j<indexes.Length;j++)
						{
							indexes[j] = 0x2021;
						}
						this.SevconMasterNode.preFilledRxMotorSPDOMappings.Add(addMappFillings(SevconMasterNode, indexes, subsRx)); //always add even if empty - allows for easier unmapping later
						for(int j = 0;j<indexes.Length;j++)
						{
							indexes[j] = 0x2040;
						}
						this.SevconMasterNode.preFilledRxMotorSPDOMappings.Add(addMappFillings(SevconMasterNode, indexes, subsRx)); //always add even if empty - allows for easier unmapping later
						for(int j = 0;j<indexes.Length;j++)
						{
							indexes[j] = 0x2060;
						}
						this.SevconMasterNode.preFilledRxMotorSPDOMappings.Add(addMappFillings(SevconMasterNode, indexes, subsRx)); //always add even if empty - allows for easier unmapping later
						#endregion Rx PDO pre-set MAster mappings
						#endregion Sevcon master
					}
					else
					{
						#region Sevcon App slaves
						this.sysInfo.nodes[i].preFilledTxMotorSPDOMappings = new ArrayList();
						if(this.motorProfilesPresent[i,0]>0) 
						{//a first motr is present the offset is 
							//now add the mappings
							int Offset = this.motorProfilesPresent[i,0] - 0x7FF;
							#region Tx PDO pre-set Slave mappings = 1st motor profile
							this.sysInfo.nodes[i].preFilledRxMotorSPDOMappings = new ArrayList();
							int [] subs = {0,0,0};
							int [] indexes = new int [3];
							indexes[0] = Offset + 0x40; //normally 0x6041 but can be offset for other motor profile areas
							indexes[1] =  Offset + 0xFF;
							indexes[2] =  Offset + 0x72;
							//for(int j = 0;j<numOfPreFilledPDOs;j++) only need one
							{
								this.sysInfo.nodes[i].preFilledRxMotorSPDOMappings.Add(addMappFillings(this.sysInfo.nodes[i], indexes, subs));
							}
							#endregion Tx PDO pre-set Slave mappings

							#region Rx PDO pre-set mappings - 1st motor profile
							this.sysInfo.nodes[i].preFilledTxMotorSPDOMappings = new ArrayList();
							indexes[0] = Offset + 0x41; //normally 0x6041 but can be offset for other motor profile areas
							indexes[1] =  Offset + 0x6C;
							indexes[2] =  Offset + 0x77;
							//for(int j = 0;j<numOfPreFilledPDOs;j++) only need one
							{
								this.sysInfo.nodes[i].preFilledTxMotorSPDOMappings.Add(addMappFillings(this.sysInfo.nodes[i], indexes, subs));
							}
							#endregion Rx PDO pre-set mappings - 1st motor profile
						}
						if(this.motorProfilesPresent[i,1]>0) //secind motor
						{
							#region Tx PDO pre-set Slave mappings - 2nd motor profile
							int Offset = this.motorProfilesPresent[i,1] - 0x7FF;
							this.sysInfo.nodes[i].preFilledRxMotorSPDOMappings = new ArrayList();
							int [] subs = {0,0,0};
							int [] indexes = new int [3];
							indexes[0] = Offset + 0x40; //normally 0x6041 but can be offset for other motor profile areas
							indexes[1] =  Offset + 0xFF;
							indexes[2] =  Offset + 0x72;
							//for(int j = 0;j<numOfPreFilledPDOs;j++) only need one
							{
								this.sysInfo.nodes[i].preFilledRxMotorSPDOMappings.Add(addMappFillings(this.sysInfo.nodes[i], indexes, subs));
							}
							#endregion Tx PDO pre-set Slave mappings - 2nd motor profile

							#region Rx PDO pre-set Slave Mappings - 2nd motor profile
							this.sysInfo.nodes[i].preFilledTxMotorSPDOMappings = new ArrayList();
							indexes[0] = Offset + 0x41; //normally 0x6041 but can be offset for other motor profile areas
							indexes[1] =  Offset + 0x6C;
							indexes[2] =  Offset + 0x77;
							//for(int j = 0;j<numOfPreFilledPDOs;j++) only need one
							{
								this.sysInfo.nodes[i].preFilledTxMotorSPDOMappings.Add(addMappFillings(this.sysInfo.nodes[i], indexes, subs));
							}
							#endregion  PDO pre0set Slave mappings - 2nd motor profile
						}
						#endregion Sevcon App slaves
					}
				}
			}
		}
		private ArrayList addMappFillings(nodeInfo node, int [] ODindexes , int [] subs)
		{
			ArrayList mappings = new ArrayList();
			for(int i = 0;i<subs.Length;i++)
			{
                ObjDictItem odItem = node.getODItemAndSubs(ODindexes[i]);
                if (odItem != null) //let's see what flavour it is
                {
                    ODItemData firstSub = (ODItemData)odItem.odItemSubs[0];
                    ODItemData odSub;
                    if (firstSub.format == SevconNumberFormat.BIT_SPLIT)  //this will be a single sub item split into subindexes
                    {
                        odSub = node.getODSub(ODindexes[i], -1);  //use root due to bitsplitting - we have already filtered for Sevcon appplication
                    }
                    else //not bitsplit in EDS
                    {
                        odSub = node.getODSub(ODindexes[i], subs[i]);
                    }

                    if (odSub != null)
                    {
                        mappings.Add(new PDOMapping(convertODItemToMappingValue(odSub), odSub.parameterName));
                    }
                    else
                    {
                        mappings.Clear();
                        return mappings; //error so get out
                    }
                }
			}
			return mappings;
		}

		#endregion create pre-filled mapping sets based on node type and number/location of motor profiles
		#region graphics setup and layout
		#region Setup screen controls
		private void setupPDOGraphics()
		{
			this.PDORoutingPanel.Controls.Clear();
			int scrNodeMinHeight = 10 * PDOVertSpacer;
			#region Transmit and Receive labels
			Graphics tempG = this.PDORoutingPanel.CreateGraphics();
			Lbl_TxHeader.Text = "Transmit";
			SizeF lblSize = tempG.MeasureString(Lbl_TxHeader.Text, Lbl_TxHeader.Font, 200);
			Lbl_TxHeader.Font = new Font("Arial", 10);
			Lbl_TxHeader.ForeColor = Color.Green;
			Lbl_TxHeader.AutoSize = true;
			Lbl_TxHeader.Top = 0;
			PDORoutingPanel.Controls.Add(Lbl_TxHeader);

			Lbl_RxHeader.Text = "Receive";
			lblSize = tempG.MeasureString(Lbl_RxHeader.Text, Lbl_RxHeader.Font, 200);
			Lbl_RxHeader.ForeColor = Color.DarkViolet;
			Lbl_RxHeader.Font = new Font("Arial", 10);
			Lbl_RxHeader.AutoSize = true;
			this.PDORoutingPanel.Controls.Add(Lbl_RxHeader);
			Lbl_RxHeader.Top = 0;
			PDORoutingPanel.Controls.Add(Lbl_RxHeader);
			tempG.Dispose();
			#endregion Transmit and Receive labels
			createGenericArrows();
			#region System PDOs
			this.PDORoutingPanel.Font = mainWindowFont;
			this.RxPDONodePanels = new ArrayList();
			this.TxPDONodePanels = new ArrayList();
			this.PDOMousePos = new Point(0,0);
			for(ushort i = 0; i< this.PDOableCANNodes.Count;i++)
			{
				nodeInfo node = (nodeInfo) PDOableCANNodes[i];
				#region create tx and rx Screen node panels
				Panel txNodePnl  = new Panel();
				txNodePnl.Name = "txNodePnl" + i.ToString();
				txNodePnl.Tag = 0;
				txNodePnl.BorderStyle = BorderStyle.FixedSingle;
				txNodePnl.Size = new Size(110,scrNodeMinHeight);
				Panel rxNodePnl = new Panel();
				rxNodePnl.Size = new Size(110,scrNodeMinHeight);
				rxNodePnl.BorderStyle = BorderStyle.FixedSingle;
				rxNodePnl.Tag = 0;
				#endregion create tx and rx Screen node panels
				#region create screen node labels
				#region Tx node Id label
				Label txNodeIdLbl = new Label();
				txNodeIdLbl.Text = "node ID " +  node.nodeID.ToString();
				txNodeIdLbl.Dock = DockStyle.Right;
				txNodeIdLbl.AutoSize = true;
				#endregion Tx node Id label
				#region device type label
				Label txTypeLbl = new Label();
				txTypeLbl.Dock = DockStyle.Bottom;
				txTypeLbl.Text = node.deviceType;
				txTypeLbl.AutoSize = true;
				#endregion device type label
				Label rxNodeIdLbl = new Label();
				rxNodeIdLbl.Dock = DockStyle.Right;
				rxNodeIdLbl.Text = "node ID " +  node.nodeID.ToString();
				rxNodeIdLbl.AutoSize = true;
				Label rxTypeLbl = new Label();
				rxTypeLbl.Dock = DockStyle.Bottom;
				rxTypeLbl.Text = node.deviceType;
				rxTypeLbl.AutoSize = true;
				#endregion create screen node labels
				#region create screen node Icons Picture boxes
				PictureBox txNodeIcon = new PictureBox();
				txNodeIcon.SizeMode = PictureBoxSizeMode.AutoSize;
				PictureBox rxNodeIcon = new PictureBox();
				rxNodeIcon.SizeMode = PictureBoxSizeMode.AutoSize;
				#endregion  create screen node Icons Picture boxes
				#region add icon and labels to screen node panels
				txNodePnl.Controls.Add(txNodeIdLbl);
				txNodePnl.Controls.Add(txTypeLbl);
				rxNodePnl.Controls.Add(rxNodeIdLbl);
				rxNodePnl.Controls.Add(rxTypeLbl);
				txNodePnl.Controls.Add(txNodeIcon);
				rxNodePnl.Controls.Add(rxNodeIcon);
				#endregion add icon and labels to screen node panels
				#region add screen node panels to form PDORouting Panel and to the correct arrayList
				this.PDORoutingPanel.Controls.Add(txNodePnl);
				this.TxPDONodePanels.Add(txNodePnl);
				this.PDORoutingPanel.Controls.Add(rxNodePnl);
				this.RxPDONodePanels.Add(rxNodePnl);
				#endregion add screen node panels to form PDORouting Panel and to the correct arrayList
			}
			updatePDOScreenNodeIcons();
			#endregion System PDOs
		}
		private void createGenericArrows()
		{
			#region setup generic Interfac ePanle to Node arrow thatvan be drawn as required
			//total width is 20 pixels - fixed gap between intercae panel and screen node
			this.genericHorizArrowPts = new Point[7];
			genericHorizArrowPts[0] = new Point(0,-6); //work clockwise from top left corner = 
			genericHorizArrowPts[1] = new Point(10, -6);  //go right 14 pixels
			genericHorizArrowPts[2] = new Point(10, -12); //top otf arror tip
			genericHorizArrowPts[3] = new Point(20, 0);  //arrow tip
			genericHorizArrowPts[4] = new Point(10, 12);  //
			genericHorizArrowPts[5] = new Point(10,6);
			genericHorizArrowPts[6] = new Point(0, 6);

			this.genericVertArrowPts = new Point[7];
			genericVertArrowPts[0] = new Point(-12,-10); //work clockwise from bottom point?
			genericVertArrowPts[1] = new Point(-6, -10);  //go right 14 pixels
			genericVertArrowPts[2] = new Point(-6,-20); //top otf arror tip
			genericVertArrowPts[3] = new Point(6,-20);  //arrow tip
			genericVertArrowPts[4] = new Point(6,-10);  //
			genericVertArrowPts[5] = new Point(12, -10);
			genericVertArrowPts[6] = new Point(0, 0);
			#endregion setup generic Interfac ePanle to Node arrow thatvan be drawn as required
		}
		private void updatePDOScreenNodeIcons()
		{
			for(ushort i = 0; i< this.PDOableCANNodes.Count;i++)
			{
				nodeInfo node = (nodeInfo) PDOableCANNodes[i];

				#region nodeIcon images
				Bitmap myimage;
				if(node.masterStatus == true)
				{
					if(node.nodeState == NodeState.PreOperational)
					{
						myimage = new Bitmap (MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\icons\PDOImages\AnimPreOpLEDMaster.gif");
					}
					else if(node.nodeState == NodeState.Operational)
					{
						myimage = new Bitmap (MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\icons\PDOImages\newCANNodeMasterOnOff2.png");
					}
					else
					{
						myimage = new Bitmap (MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\icons\PDOImages\newCANNodeMasterOffOff2.png");
					}
					//
				}
				else  //slave or unknown - therefore assumed to be slave
				{
					if(node.nodeState == NodeState.PreOperational)
					{
						myimage = new Bitmap (MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\icons\PDOImages\AnimPreOpLEDSlave.gif");

					}
					else if(node.nodeState == NodeState.Operational)
					{
						myimage = new Bitmap (MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\icons\PDOImages\newCANNodeSlaveOnOff2.png");
					}
					else
					{
						myimage = new Bitmap (MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\icons\PDOImages\newCANNodeSlaveOffOff2.png");
					}
				}
				
				foreach(Control cont in ((Panel)this.TxPDONodePanels[i]).Controls)
				{
					if(cont is PictureBox)
					{
						((PictureBox)cont).Image = myimage;
					}
				}
				foreach(Control cont in ((Panel)this.RxPDONodePanels[i]).Controls)
				{
					if(cont is PictureBox)
					{
						((PictureBox)cont).Image = myimage;
					}
				}
				#endregion nodeIcon images
	
			}
		}
		private void setUpInterfacePanels()
		{
			InterfacePanels = new ArrayList();
			#region Tx Interface panel
			Panel txInterfacePanel = new Panel();
			txInterfacePanel.Width = 20;
			txInterfacePanel.BorderStyle = BorderStyle.FixedSingle;
			Label lbl = new Label();
			txInterfacePanel.Paint +=new PaintEventHandler(txInterfacePanel_Paint);
			this.PDORoutingPanel.Controls.Add(txInterfacePanel);
			InterfacePanels.Add(txInterfacePanel);
			#endregion Tx Interface panel
			#region Rx Interface Panel
			Panel rxInterfacePanel = new Panel();
			rxInterfacePanel.Width = 20;  //fixed
			rxInterfacePanel.BorderStyle = BorderStyle.FixedSingle;
			rxInterfacePanel.Paint +=new PaintEventHandler(rxInterfacePanel_Paint);
			this.PDORoutingPanel.Controls.Add(rxInterfacePanel);
			InterfacePanels.Add(rxInterfacePanel);
			#endregion Rx Interface Panel
			if(this.SevconMasterNode != null)
			{
				#region VPDOTx Interface Panel
				Panel txVPDOIFacePnl = new Panel();
				txVPDOIFacePnl.Height = 20;
				txVPDOIFacePnl.BorderStyle = BorderStyle.FixedSingle;
				lbl.Text = "Output Internal PDOs for Sevcon master node ID" + this.SevconMasterNode.nodeID.ToString();
				lbl.AutoSize = true;
				txVPDOIFacePnl.Controls.Add(lbl);
				this.PDORoutingPanel.Controls.Add(txVPDOIFacePnl);
				InterfacePanels.Add(txVPDOIFacePnl);
				#endregion VPDOTx Interface Panel
				#region VPDORx Interface Panel
				Panel rxVPDOIFacePnl = new Panel();
				rxVPDOIFacePnl.Height = 20;
				rxVPDOIFacePnl.BorderStyle = BorderStyle.FixedSingle;
				lbl = new Label();
				lbl.Text = "Input Internal PDOs for Sevcon master node ID" + this.SevconMasterNode.nodeID.ToString();
				lbl.AutoSize = true;
				rxVPDOIFacePnl.Controls.Add(lbl);
				this.PDORoutingPanel.Controls.Add(rxVPDOIFacePnl);
				InterfacePanels.Add(rxVPDOIFacePnl);
				#endregion VPDORx Interface Panel
			}
		}
		private void setUpInternalPDOPanels()
		{
			//create the group boxes for sitting above - show them all the time
			if(this.SevconMasterNode != null)
			{
				VPDO_GBs = new ArrayList();
				for(int i = 0;i<5;i++)
				{
					switch(i)
					{
						case 0:
							addVPDOlb(this.SevconMasterNode.intPDOMaps.digOPMaps, "Dig O/Ps");
							break;
						case 1:
							addVPDOlb(this.SevconMasterNode.intPDOMaps.algOPMaps, "Alg O/Ps");
							break;
						case 2:
							addVPDOlb(this.SevconMasterNode.intPDOMaps.MotorMaps, "Motor");
							break;
						case 3:
							addVPDOlb(this.SevconMasterNode.intPDOMaps.digIPMaps, "Dig I/Ps");
							break;
						case 4:
							addVPDOlb(this.SevconMasterNode.intPDOMaps.algIPMaps, "Alg I/Ps");
							break;
					}
				}
			}		
		}

		private void setupSysPDOExtensionPanels()
		{
			this.PDORoutingPanel.SuspendLayout();
			#region remove any currently displayed  Tx SPDO expansion Groupboxes form the screen
			if(txSysPDOExpansionPnls.Count>0)
			{
				foreach(GroupBox gb in (ArrayList) this.txSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currTxPDOnode)])
				{
					this.PDORoutingPanel.Controls.Remove(gb);
				}
			}
			#endregion remove any currently displayed  Tx SPDO expansion Groupboxes form the screen
			#region remove any currently displayed  Rx SPDO expansion Groupboxes form the screen
			if(rxSysPDOExpansionPnls.Count>0)
			{
				foreach(GroupBox gb in (ArrayList) this.rxSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currRxPDOnode)])
				{
					this.PDORoutingPanel.Controls.Remove(gb);
				}
			}
			#endregion remove any currently displayed  Rx SPDO expansion Groupboxes form the screen
			//create neww ArrayLsits for the extension panles
			this.txSysPDOExpansionPnls = new ArrayList();
			this.rxSysPDOExpansionPnls = new ArrayList();
			for(int i = 0;i<this.PDOableCANNodes.Count;i++)
			{//create an ArrayList to contain the GroupBoxes displaying the mappings in each System PDO
				ArrayList GB_TxnodeMappings = new ArrayList();
				//add each ArrayLsit to the expansion PAnles ArrayList
				txSysPDOExpansionPnls.Add(GB_TxnodeMappings);
				ArrayList GB_RxnodeMappings = new ArrayList();
				rxSysPDOExpansionPnls.Add(GB_RxnodeMappings);
			}
			foreach(nodeInfo node in this.PDOableCANNodes)
			{
				foreach(COBObject COB in this.sysInfo.COBsInSystem)
				{
					if(COB.messageType == COBIDType.PDO)
					{
						foreach(COBObject.PDOMapData txData in COB.transmitNodes)
						{
							if(node.nodeID == txData.nodeID)
							{
								addSPDOLB(node, txData.SPDOMaps, true, COB);  //true for Tx
							}
						}
						foreach(COBObject.PDOMapData rxData in COB.receiveNodes)
						{
							if(node.nodeID == rxData.nodeID)
							{
								addSPDOLB(node, rxData.SPDOMaps, false, COB);  //true for Tx
							}
						}
					}
				}
			}
			this.PDORoutingPanel.ResumeLayout();
		}
		#region Communications SetUp Panel
		private void setupCOBDataBindings()
		{
			Binding bind = new Binding("Text", this.sysInfo.COBsInSystem, "requestedCOBID");
			bind.Format +=new ConvertEventHandler(bind_Format);
			this.TB_SPDO_COBID.DataBindings.Clear();
			this.TB_SPDO_COBID.DataBindings.Add(bind);

			this.RB_SyncCyclic.DataBindings.Clear();
			this.RB_SyncCyclic.DataBindings.Add(new Binding("Checked", this.sysInfo.COBsInSystem, "SyncCyclic"));

			this.RB_SyncAcyclic.DataBindings.Clear();
			this.RB_SyncAcyclic.DataBindings.Add(new Binding("Checked", this.sysInfo.COBsInSystem, "SyncAcylic"));

			this.RB_AsynchNormal.DataBindings.Clear();
			this.RB_AsynchNormal.DataBindings.Add(new Binding("Checked", this.sysInfo.COBsInSystem, "AsynchNormal"));

			this.RB_AsynchRTR.DataBindings.Clear();
			this.RB_AsynchRTR.DataBindings.Add(new Binding("Checked", this.sysInfo.COBsInSystem, "AsyncRTR"));

			this.RB_SyncRTR.DataBindings.Clear();
			this.RB_SyncRTR.DataBindings.Add(new Binding("Checked", this.sysInfo.COBsInSystem, "SyncRTR"));
			this.NUD_TxNumSyncs.DataBindings.Clear();
			//do NOT bind to the Control's Value property - known .NET issue
			this.NUD_TxNumSyncs.DataBindings.Add(new Binding("Text", this.sysInfo.COBsInSystem, "SyncTime"));

			this.NUD_InhibitITme.DataBindings.Clear();
			this.NUD_InhibitITme.DataBindings.Add(new Binding("Text", this.sysInfo.COBsInSystem, "inhibitTime"));

			this.NUDEventTime.DataBindings.Clear();
			this.NUDEventTime.DataBindings.Add(new Binding("Text", this.sysInfo.COBsInSystem, "eventTime"));

			#region system sync time
			foreach(COBObject COB in this.sysInfo.COBsInSystem)
			{
				if(COB.messageType == COBIDType.Sync)
				{
					if(COB.transmitNodes.Count>0)
					{
						COBObject.PDOMapData txData = (COBObject.PDOMapData) COB.transmitNodes[0];
						int CANNodeIndex = -1;
						this.sysInfo.getNodeNumber(txData.nodeID, out CANNodeIndex);
						if((CANNodeIndex>0) && (this.sysInfo.nodes[CANNodeIndex].syncTimeSub != null))
						{
							///hmm not the event time - 							this.NUDEventTime.Maximum = (int) (this.sysInfo.nodes[CANNodeIndex].syncTimeSub.currentValue /1000); //convert us to ms
						}
					}
					break;
				}
			}
			#endregion system sync time
		}
		/// <summary>
		/// This methosneeds to be called whenever the contents of COBsInSystme have changed eg COBID changed
		/// It does not need to be called when just switching th e'active' COB
		/// </summary>
		/// <param name="manPos"></param>
		private void updatePDODataBindings(int manPos)
		{
            this.CB_SPDOName.BindingContext = new BindingContext();  //required every time - otherwise we get exception when adding COBs to sytems
            managerCB = this.CB_SPDOName.BindingContext[this.sysInfo.COBsInSystem, "name"]; //set the manager 
            managerCB.Position = manPos;
            this.CB_SPDOName.DataSource = new ArrayList();
			this.CB_SPDOName.DataSource = this.sysInfo.COBsInSystem; //datasouce
			this.CB_SPDOName.DisplayMember =  "name";  //and pioitn to the correct property in the field

            if (manPos >= CB_SPDOName.Items.Count)
            {
                manPos = CB_SPDOName.Items.Count - 1;
            }
			this.CB_SPDOName.SelectedIndex = manPos;
			this.TB_SPDO_COBID.BindingContext = this.CB_SPDOName.BindingContext;  //set to same Binding context - is manager automatic?
			#region Tx Type Radio button
			this.RB_SyncCyclic.BindingContext = this.CB_SPDOName.BindingContext;
			this.RB_SyncAcyclic.BindingContext = this.CB_SPDOName.BindingContext;
			this.RB_AsynchNormal.BindingContext = this.CB_SPDOName.BindingContext;
			this.RB_AsynchRTR.BindingContext = this.CB_SPDOName.BindingContext;
			this.RB_SyncRTR.BindingContext = this.CB_SPDOName.BindingContext;
			#endregion Tx Type Radio button
			this.NUD_TxNumSyncs.BindingContext = this.CB_SPDOName.BindingContext; //this links this control to the correct index in th eCOBsInSystem ArrayList - we set up for the COB name and then all othe rrelevent controls use the same BInding Context
			this.NUD_InhibitITme.BindingContext = this.CB_SPDOName.BindingContext;
			this.NUDEventTime.BindingContext = this.CB_SPDOName.BindingContext;
			this.activateCOB((COBObject)this.sysInfo.COBsInSystem[manPos]);
		}

		#endregion Communications SetUp Panel
		private void addSPDOLB(nodeInfo node, ArrayList dSource, bool isTx, COBObject COB)
		{
			ArrayList GBs;
			if(isTx == true)
			{
				GBs  = (ArrayList) this.txSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(node)];
			}
			else
			{
				GBs  = (ArrayList) this.rxSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(node)];
			}
			GroupBox gb = new GroupBox();
			gb.ClientSize = new Size(gb.Width-2, gb.Height-4);
			gb.Text = COB.name;
			gb.MouseDown +=new MouseEventHandler(SPDOlb_MouseDown);
			gb.Tag = COB;
			ListBox lb = new ListBox();
			lb.Dock = DockStyle.Fill;
			lb.Width = 60;
			lb.BorderStyle = BorderStyle.None;
			lb.MouseDown +=new MouseEventHandler(SPDOlb_MouseDown);
			lb.BindingContext = new BindingContext();
			if(dSource.Count>0)
			{
				BindingManagerBase managerLB = lb.BindingContext[dSource,"mapName"]; //set the manager 
				lb.DataSource = dSource;
				lb.DisplayMember = "mapName";
				lb.ValueMember = "mapValue";
			}
			lb.SelectedIndex = -1; //judetemp
			lb.DrawMode = DrawMode.OwnerDrawFixed;
			lb.DrawItem +=new DrawItemEventHandler(SPDOlb_DrawItem);
			gb.Height = ((Panel)TxPDONodePanels[0]).Height;
			gb.Controls.Add(lb);
			GBs.Add(gb);
		}

		private void addVPDOlb(ArrayList dataSource, string gbText)
		{
			GroupBox gb = new GroupBox();
			gb.ClientSize = new Size(gb.Width-2, gb.Height-4);
			ListBox lb = new ListBox();
			lb.DrawMode = DrawMode.OwnerDrawFixed;
			lb.DrawItem +=new DrawItemEventHandler(VPDOlb_DrawItem);
			lb.AllowDrop = true;
			lb.IntegralHeight = false;
			lb.BorderStyle = BorderStyle.None;
			lb.BindingContext = new BindingContext();  //required every time - otherwise we get exception when adding COBs to sytems
			gb.Text = gbText;
			if(dataSource.Count>0)
			{
				BindingManagerBase managerLB = lb.BindingContext[dataSource,"mapName"]; //set the manager 
				lb.DataSource = dataSource; //datasouce
				lb.DisplayMember = "mapName";
				lb.ValueMember = "mapValue";
			}
			else //no items in listbox
			{
				//do nothing
			}
			lb.SelectedIndex = -1; 
			lb.Dock = DockStyle.Fill;
			lb.DragOver +=new DragEventHandler(VPDOlb_DragOver);
			lb.DragDrop +=new DragEventHandler(VPDOlb_DragDrop);
			lb.MouseDown +=new MouseEventHandler(VPDOlb_MouseDown);
			//			lb.MouseMove +=new MouseEventHandler(lb_MouseMove);
			gb.Controls.Add(lb);
			int VPDO_GBHeight = 100;
			gb.Height =  VPDO_GBHeight;
			VPDO_GBs.Add(gb);
			this.PDORoutingPanel.Controls.Add(gb);
			lb.Resize +=new EventHandler(VPDOlb_Resize);
		}
		#endregion Setup
		#region switch On/Off PDo event handlers
		private void switchOnPDOEventHandlers()
		{
			#region PDO routing and route selection event hanlders
			this.PDORoutingPanel.Paint +=new PaintEventHandler(PDORoutingPanel_Paint); 
			this.PDORoutingPanel.Resize +=new EventHandler(PDORoutingPanel_Resize);
			this.PDORoutingPanel.MouseMove +=new MouseEventHandler(PDORoutingPanel_MouseMove);
			this.PDORoutingPanel.MouseDown +=new MouseEventHandler(PDORoutingPanel_MouseDown);
			for(ushort i = 0; i< this.TxPDONodePanels.Count;i++)
			{
				Panel txScrNode = (Panel) this.TxPDONodePanels[i];
				txScrNode.MouseDown +=new MouseEventHandler(TxScreenNode_MouseDown);
				txScrNode.MouseUp +=new MouseEventHandler(TxScreenNode_MouseUp);  //need to do it this way - after the mouse down & hold the mouse up event is flagged back to the Picture box
				txScrNode.MouseMove +=new MouseEventHandler(TxScreenNode_MouseMove);
				foreach(Control cont in txScrNode.Controls)
				{
					cont.MouseDown +=new MouseEventHandler(TxScreenNode_MouseDown);
					cont.MouseUp +=new MouseEventHandler(TxScreenNode_MouseUp);  //need to do it this way - after the mouse down & hold the mouse up event is flagged back to the Picture box
					cont.MouseMove +=new MouseEventHandler(TxScreenNode_MouseMove);
				}
			}
			for(ushort i = 0; i< this.RxPDONodePanels.Count;i++)
			{
				Panel rxScrNode = (Panel) this.RxPDONodePanels[i];
				rxScrNode.MouseDown +=new MouseEventHandler(RxScrNode_MouseDown);
				foreach(Control cont in rxScrNode.Controls)
				{
					cont.MouseDown +=new MouseEventHandler(RxScrNode_MouseDown);
				}
			}
			#endregion PDO routing and route selection event hanlders
			#region mapping panel event handlers
			this.Pnl_COBMappings.Resize +=new EventHandler(Pnl_COBMappings_Resize);
			Pnl_COBMappings.MouseDown +=new MouseEventHandler(Pnl_COBMappings_MouseDown);
			#endregion mapping panel event handlers
			#region communcications parameters event handlers
			this.CB_SPDOName.SelectionChangeCommitted +=new EventHandler(CB_SPDOName_SelectionChangeCommitted);
			this.CB_SPDOName.KeyDown +=new KeyEventHandler(CB_SPDOName_KeyDown);
			this.CB_SPDO_priority.SelectionChangeCommitted +=new EventHandler(CB_SPDO_priority_SelectionChangeCommitted);
			this.RB_SyncCyclic.Click +=new EventHandler(RB_Click);
			this.RB_SyncAcyclic.Click +=new EventHandler(RB_Click);	//these Radio buttons have autoCheck set to true
			this.RB_AsynchNormal.Click +=new EventHandler(RB_Click);
			this.RB_AsynchRTR.Click +=new EventHandler(RB_Click);
			this.RB_SyncRTR.Click +=new EventHandler(RB_Click);
			TB_SPDO_COBID.KeyDown +=new KeyEventHandler(TB_SPDO_COBID_KeyDown);
			this.NUD_InhibitITme.KeyDown +=new KeyEventHandler(NUD_InhibitITme_KeyDown);
			this.NUD_InhibitITme.Leave +=new EventHandler(NUD_InhibitITme_Leave);
			this.NUDEventTime.KeyDown +=new KeyEventHandler(NUDEventTime_KeyDown);
			this.NUDEventTime.Leave +=new EventHandler(NUDEventTime_Leave);
			this.NUD_TxNumSyncs.KeyDown +=new KeyEventHandler(NUD_TxNumSyncs_KeyDown);
			this.NUD_TxNumSyncs.Leave +=new EventHandler(NUD_TxNumSyncs_Leave);
			#endregion communcications parameters event handlers
		}
		private void switchOffCOBPDOEventHandlers()
		{
			#region PDO routing and route selection event hanlders
			this.PDORoutingPanel.Paint -=new PaintEventHandler(PDORoutingPanel_Paint); 
			this.PDORoutingPanel.Resize -=new EventHandler(PDORoutingPanel_Resize);
			this.PDORoutingPanel.MouseMove -=new MouseEventHandler(PDORoutingPanel_MouseMove);
			this.PDORoutingPanel.MouseDown -=new MouseEventHandler(PDORoutingPanel_MouseDown);
			for(ushort i = 0; i< this.TxPDONodePanels.Count;i++)
			{
				Panel txScrNode = (Panel) this.TxPDONodePanels[i];
				txScrNode.MouseDown -=new MouseEventHandler(TxScreenNode_MouseDown);
				txScrNode.MouseUp -=new MouseEventHandler(TxScreenNode_MouseUp);  //need to do it this way - after the mouse down & hold the mouse up event is flagged back to the Picture box
				txScrNode.MouseMove -=new MouseEventHandler(TxScreenNode_MouseMove);
				foreach(Control cont in txScrNode.Controls)
				{
					cont.MouseDown -=new MouseEventHandler(TxScreenNode_MouseDown);
					cont.MouseUp -=new MouseEventHandler(TxScreenNode_MouseUp);  //need to do it this way - after the mouse down & hold the mouse up event is flagged back to the Picture box
					cont.MouseMove -=new MouseEventHandler(TxScreenNode_MouseMove);
				}
			}
			for(ushort i = 0; i< this.RxPDONodePanels.Count;i++)
			{
				Panel rxScrNode = (Panel) this.RxPDONodePanels[i];
				rxScrNode.MouseDown -=new MouseEventHandler(RxScrNode_MouseDown);
				foreach(Control cont in rxScrNode.Controls)
				{
					cont.MouseDown +=new MouseEventHandler(RxScrNode_MouseDown);
				}
			}
			#endregion PDO routing and route selection event hanlders
			#region mapping panel event handlers
			this.Pnl_COBMappings.Resize -=new EventHandler(Pnl_COBMappings_Resize);
			Pnl_COBMappings.MouseDown -=new MouseEventHandler(Pnl_COBMappings_MouseDown);
			#endregion mapping panel event handlers
			#region communcications parameters event handlers
			this.CB_SPDOName.SelectionChangeCommitted -=new EventHandler(CB_SPDOName_SelectionChangeCommitted);
			this.CB_SPDOName.KeyDown -=new KeyEventHandler(CB_SPDOName_KeyDown);
			this.CB_SPDO_priority.SelectionChangeCommitted -=new EventHandler(CB_SPDO_priority_SelectionChangeCommitted);
			this.RB_SyncCyclic.Click -=new EventHandler(RB_Click);
			this.RB_SyncAcyclic.Click -=new EventHandler(RB_Click);	//these Radio buttons have autoCheck set to true
			this.RB_AsynchNormal.Click -=new EventHandler(RB_Click);
			this.RB_AsynchRTR.Click -=new EventHandler(RB_Click);
			this.RB_SyncRTR.Click -=new EventHandler(RB_Click);
			TB_SPDO_COBID.KeyDown -=new KeyEventHandler(TB_SPDO_COBID_KeyDown);
			this.NUD_InhibitITme.KeyDown -=new KeyEventHandler(NUD_InhibitITme_KeyDown);
			this.NUD_InhibitITme.Leave -=new EventHandler(NUD_InhibitITme_Leave);
			this.NUDEventTime.KeyDown -=new KeyEventHandler(NUDEventTime_KeyDown);
			this.NUDEventTime.Leave -=new EventHandler(NUDEventTime_Leave);
			#endregion communcications parameters event handlers
		}
		#endregion switch On/Off PDo event handlers
		#region layout screen controls
		#region layout - System PDOs
		private void layoutPDOGraphics()
		{
			#region "Transmit" and Receive labels
			Lbl_TxHeader.Left =(int) ((this.PDORoutingPanel.ClientRectangle.Width/4) - (Lbl_TxHeader.Width/2));
			Lbl_RxHeader.Left =(int) (((this.PDORoutingPanel.ClientRectangle.Width * 3)/4) - (Lbl_RxHeader.Width/2));
			#endregion "Transmit" and Receive labels
			//we draw the PDO routing panel to cover most of the underlying RH Paenl - then we use the spitter bar and the 
			//scroll bars to see it 
			int availableHeight = this.ClientRectangle.Height - Lbl_RxHeader.Bottom - 22 ;
			int spacing =(int)  ((availableHeight * 0.8)/this.PDOableCANNodes.Count);  //80% of panel height  divide by nuber of PDOable nodes
			spacing = Math.Min(spacing, 12);  //clamp to 20- this is the highes we need the spacing to be
			int VPDOOffset = Lbl_TxHeader.Bottom;
			//layout permanent VPDO displays
			if(this.SevconMasterNode != null)
			{
				#region digital outputs	
				GroupBox GBdigOP = (GroupBox) this.VPDO_GBs[(int) IntPDOType.DIG_OPS];
				ListBox LBdigOP = (ListBox) GBdigOP.Controls[0];
				GBdigOP.Top = Lbl_TxHeader.Bottom;
				GBdigOP.Width = ((this.PDORoutingPanel.ClientRectangle.Width/2) - 8) /3;
				GBdigOP.Left = 2;
				#endregion digital outputs	
				#region analogue putputs
				GroupBox GBalgOP = (GroupBox) this.VPDO_GBs[(int) IntPDOType.ALG_OPs];
				GBalgOP.Top = Lbl_TxHeader.Bottom;
				GBalgOP.Width = ((this.PDORoutingPanel.ClientRectangle.Width/2) - 8) /3;
				GBalgOP.Left = GBdigOP.Right + 2;
				#endregion analogue putputs
				#region motor
				GroupBox GBMotor = (GroupBox) this.VPDO_GBs[(int) IntPDOType.MOTOR];
				GBMotor.Top = Lbl_TxHeader.Bottom;
				GBMotor.Width = ((this.PDORoutingPanel.ClientRectangle.Width/2) - 8) /3;
				GBMotor.Left =  GBalgOP.Right + 2;
				#endregion motor
				#region Tx VPDO interface panel
				((Panel) this.InterfacePanels[(int) IFacePnls.VPDOTx]).Top = GBMotor.Bottom;
				((Panel) this.InterfacePanels[(int) IFacePnls.VPDOTx]).Width = GBMotor.Right - GBdigOP.Left;
				((Panel) this.InterfacePanels[(int) IFacePnls.VPDOTx]).Left 
					= GBMotor.Right - ((Panel) this.InterfacePanels[(int) IFacePnls.VPDOTx]).Width;
				((Panel) this.InterfacePanels[(int) IFacePnls.VPDOTx]).Top = GBMotor.Bottom;
				#endregion Tx VPDO interface panel
				#region digital inputs
				GroupBox GBdigIP = (GroupBox) this.VPDO_GBs[(int) IntPDOType.DIG_IPs];
				GBdigIP.Top = Lbl_TxHeader.Bottom;
				GBdigIP.Left = (this.PDORoutingPanel.ClientRectangle.Width/2) + 2;
				GBdigIP.Width =  ((this.PDORoutingPanel.ClientRectangle.Width/2) - 6)/2;

				#endregion digital inputs
				#region analogue inputs
				GroupBox GBalgIP = (GroupBox) this.VPDO_GBs[(int) IntPDOType.ALG_IPs];
				GBalgIP.Top = Lbl_TxHeader.Bottom;
				GBalgIP.Left = GBdigIP.Right + 2;
				GBalgIP.Width = ((this.PDORoutingPanel.ClientRectangle.Width/2) - 6)/2;
				#endregion analogue inputs
				#region Rx VPDO Interface Panel
				((Panel) this.InterfacePanels[(int) IFacePnls.VPDORx]).Top = GBdigIP.Bottom;
				((Panel) this.InterfacePanels[(int) IFacePnls.VPDORx]).Width = GBalgIP.Right - GBdigIP.Left;
				((Panel) this.InterfacePanels[(int) IFacePnls.VPDORx]).Left = GBdigIP.Left; 
				#endregion Rx VPDO Interface Panel
				VPDOOffset = ((Panel)this.InterfacePanels[(int) IFacePnls.VPDOTx]).Bottom + 20; //20 is the arrow 
			}
			for(int i = 0;i<this.TxPDONodePanels.Count;i++)
			{
				Panel txScrNode = (Panel) this.TxPDONodePanels[i];
				txScrNode.Top =  (i*(spacing +txScrNode.Height)) + 2+ VPDOOffset;//+ Math.Max((int) (availableHeight/10), 30);
				txScrNode.Left = (int) (this.PDORoutingPanel.ClientRectangle.Width/2 - txScrNode.Width)/2;//(int) this.PDORoutingPanel.ClientRectangle.Width/3;
				txScrNode.Left = (int) Math.Max( txScrNode.Left, (this.PDORoutingPanel.ClientRectangle.Width/2)-100-txScrNode.Width);//clamp to within 80 pixels of middle line
			}
			for(int i = 0;i<this.RxPDONodePanels.Count;i++)
			{
				Panel rxScrNode = (Panel) this.RxPDONodePanels[i];
				rxScrNode.Top =  (i*(spacing + rxScrNode.Height)) + 2 + VPDOOffset;// Math.Max((int) (availableHeight/10), 30);
				rxScrNode.Left = (int) ( (((PDORoutingPanel.ClientRectangle.Width/2) - rxScrNode.Width)/2) + (this.PDORoutingPanel.ClientRectangle.Width/2));//(int)  ((this.PDORoutingPanel.ClientRectangle.Width * 2)/3);
				rxScrNode.Left = Math.Min(rxScrNode.Left, (PDORoutingPanel.ClientRectangle.Width/2) + 100);  //clamp to within 80 pixels of middle line
			}
		}
		private void calculateScreenLinesForPDOCOBSInSystem()
		{
			int HorizOffset = this.PDOVertSpacer;  //will auto adjust then for num of PDOable CANnodes
			int HorizSpacer = this.PDOVertSpacer;
			int maxHorizspace = ((Panel)this.RxPDONodePanels[0]).Left - ((Panel) this.TxPDONodePanels[0]).Right;
			#region set offsets to start values
			int [] txVertOffsets = new int[this.PDOableCANNodes.Count];
			for(int i = 0;i< txVertOffsets.Length;i++)
			{ //offset wrt the Rx nodes for improved read ability
				txVertOffsets[i] = PDOVertSpacer/2;
			}
			int [] rxVertOffsets = new int[this.PDOableCANNodes.Count];
			#endregion set offsets to start values
			foreach(COBObject COB in this.sysInfo.COBsInSystem)
			{
				if(COB.messageType == COBIDType.PDO)
				{
					COB.screenRoutes.Clear();//we are going to re-route so clear here
					#region createscreen routing points for all PDOs already in system
					if(COB.transmitNodes.Count <= 0)
					{  
						#region unproduced PDO
						foreach(COBObject.PDOMapData rxData in COB.receiveNodes)
						{
							#region identify the screen Rx node
							COBObject.screenRoutePoints screenRoute = new COBObject.screenRoutePoints();  //use TxNode ID zero to denote no Tx node
							foreach(nodeInfo node in this.PDOableCANNodes)
							{
								if(node.nodeID == rxData.nodeID)
								{
									screenRoute.RxNodeScreenIndex = this.PDOableCANNodes.IndexOf(node);
									break;
								}
							}
							#endregion identify the screen Rx node
							if(screenRoute.RxNodeScreenIndex>=0)  
							{  //only add if we have identified a corresponding rxScrNode  - B&B
								#region calculate screen route and add to object for paint method
								Panel rxScrNode = (Panel) this.RxPDONodePanels[screenRoute.RxNodeScreenIndex];
								rxVertOffsets[screenRoute.RxNodeScreenIndex] += PDOVertSpacer;
								//								screenRoute.endRect = new Rectangle(
								//									(rxScrNode.Left - 40),
								//									(rxScrNode.Top + rxVertOffsets[screenRoute.RxNodeScreenIndex]), 
								//									40, 
								//									2);
								screenRoute.endRect = new Rectangle(-40,(rxVertOffsets[screenRoute.RxNodeScreenIndex]), 40,	2);
								screenRoute.startRect = new Rectangle(0,0,0,0);
								screenRoute.midRect = new Rectangle(0,0,0,0);
								COB.screenRoutes.Add(screenRoute);
								#endregion calculate screen route and add to object for paint method
							}
						}
						#endregion unproduced PDO
					}
					foreach(COBObject.PDOMapData txData in COB.transmitNodes)
					{
						#region get the corresponding Tx Screen node
						COBObject.screenRoutePoints screenRoute = new COBObject.screenRoutePoints();
						foreach(nodeInfo node in this.PDOableCANNodes)
						{
							if(node.nodeID == txData.nodeID)
							{
								screenRoute.TxNodeScreenIndex = this.PDOableCANNodes.IndexOf(node);
								screenRoute.TxNodemapODIndex = txData.mapODIndex;
								break;
							}
						}
						Panel txScrNode = (Panel) this.TxPDONodePanels[screenRoute.TxNodeScreenIndex];
						txVertOffsets[screenRoute.TxNodeScreenIndex] += PDOVertSpacer; //increment the offset to scace the routes
						#endregion get the corresponding Tx Screen node
						if(COB.receiveNodes.Count <= 0) //unconsumed PDO
						{
							#region unconsumed PDO
	
							if( screenRoute.TxNodeScreenIndex >=0)
							{
								//								screenRoute.startRect = new Rectangle(txScrNode.Right, 
								//									(txScrNode.Top + txVertOffsets[screenRoute.TxNodeScreenIndex]), 
								//									40, 
								//									2);
								screenRoute.startRect = new Rectangle(0, (txVertOffsets[screenRoute.TxNodeScreenIndex]), 40,2);
								screenRoute.midRect = new Rectangle(0,0,0,0);
								screenRoute.endRect = new Rectangle(0,0,0,0);
								COB.screenRoutes.Add(screenRoute);
							}
							#endregion unconsumed PDO
						}
						else
						{
							#region routed PDO
							int txScrIndex = screenRoute.TxNodeScreenIndex;  //keep one copy to paste into each route we create
							foreach(COBObject.PDOMapData rxData in COB.receiveNodes)
							{//create one screenRoute object per Rx node
								screenRoute = new COBObject.screenRoutePoints();	
								screenRoute.TxNodeScreenIndex = txScrIndex;  //paste value in - one Tx node multiple rx Ndoes
								screenRoute.TxNodemapODIndex = txData.mapODIndex;
								screenRoute.RxNodemapODIndex = rxData.mapODIndex;
								#region identify the screen Rx node
								foreach(nodeInfo node in this.PDOableCANNodes)
								{
									if(node.nodeID == rxData.nodeID)
									{
										screenRoute.RxNodeScreenIndex = this.PDOableCANNodes.IndexOf(node);
										break;
									}
								}
								#endregion identify the screen Rx node
								if(screenRoute.RxNodeScreenIndex>=0)
								{
									Panel rxScrNode = (Panel) this.RxPDONodePanels[screenRoute.RxNodeScreenIndex];
									//does this route have the same Tx ODIndex as any of th epreviouse routes for this COB?
									rxVertOffsets[screenRoute.RxNodeScreenIndex] += PDOVertSpacer;
									screenRoute.startRect = new Rectangle(0,(txVertOffsets[screenRoute.TxNodeScreenIndex]),	HorizOffset,2);
									screenRoute.endRect = new Rectangle(HorizOffset,
										(rxScrNode.Top - txScrNode.Top + rxVertOffsets[screenRoute.RxNodeScreenIndex]), 
										(rxScrNode.Left - txScrNode.Right - HorizOffset), 2);
									#region midRect
									if((txScrNode.Top + txVertOffsets[screenRoute.TxNodeScreenIndex])
										> (rxScrNode.Top  + rxVertOffsets[screenRoute.RxNodeScreenIndex]))
									{ //line goes up
										screenRoute.midRect = new Rectangle( HorizOffset, 
											(rxScrNode.Top - txScrNode.Top + rxVertOffsets[screenRoute.RxNodeScreenIndex]),2, 
											(txScrNode.Top  - rxScrNode.Top + txVertOffsets[screenRoute.TxNodeScreenIndex] - rxVertOffsets[screenRoute.RxNodeScreenIndex] + 2));  //to go to bottom of connecting lone
									}
									else
									{ //line goes down or straight across
										screenRoute.midRect = new Rectangle( HorizOffset, (txVertOffsets[screenRoute.TxNodeScreenIndex]),2, 
											(rxScrNode.Top  - txScrNode.Top + rxVertOffsets[screenRoute.RxNodeScreenIndex] - txVertOffsets[screenRoute.TxNodeScreenIndex]));
									}
									#endregion midRect
									foreach(COBObject.screenRoutePoints ExistingScreenRoute in COB.screenRoutes )
									{
										if((ExistingScreenRoute.TxNodeScreenIndex == screenRoute.TxNodeScreenIndex)
											&& (ExistingScreenRoute.TxNodemapODIndex == screenRoute.TxNodemapODIndex))
										{
											screenRoute.startRect = ExistingScreenRoute.startRect;
											if((txScrNode.Top + txVertOffsets[screenRoute.TxNodeScreenIndex])
												> (rxScrNode.Top  + rxVertOffsets[screenRoute.RxNodeScreenIndex]))
											{ //line goes up

												screenRoute.midRect = new Rectangle( screenRoute.startRect.Right, 
													ExistingScreenRoute.midRect.Top,2, 
													(txScrNode.Top  - rxScrNode.Top + txVertOffsets[screenRoute.TxNodeScreenIndex] - rxVertOffsets[screenRoute.RxNodeScreenIndex] + 2));  //to go to bottom of connecting lone
											}
											else
											{
												screenRoute.midRect = new Rectangle( screenRoute.startRect.Right, 
													ExistingScreenRoute.midRect.Top,2, 
													(rxScrNode.Top  - txScrNode.Top + rxVertOffsets[screenRoute.RxNodeScreenIndex] - txVertOffsets[screenRoute.TxNodeScreenIndex]));
											}
											//the same CAN node tranmits this from (another?) ODIndex
										}
										if((ExistingScreenRoute.RxNodeScreenIndex == screenRoute.RxNodeScreenIndex)
											&& (ExistingScreenRoute.RxNodemapODIndex == screenRoute.RxNodemapODIndex))
										{
											screenRoute.endRect = ExistingScreenRoute.endRect;
											screenRoute.startRect.Width = ExistingScreenRoute.startRect.Width;
											if((txScrNode.Top + txVertOffsets[screenRoute.TxNodeScreenIndex])
												> (rxScrNode.Top  + rxVertOffsets[screenRoute.RxNodeScreenIndex]))
											{ //line goes up

												screenRoute.midRect = new Rectangle( screenRoute.startRect.Right, 
													ExistingScreenRoute.midRect.Top,2, 
													screenRoute.startRect.Bottom - ExistingScreenRoute.endRect.Top);  //to go to bottom of connecting lone
											}
											else
											{
												screenRoute.midRect = new Rectangle( screenRoute.startRect.Right, 
													ExistingScreenRoute.midRect.Top,2, 
													ExistingScreenRoute.startRect.Bottom - screenRoute.endRect.Top);
											}
										}
									}
									COB.screenRoutes.Add(screenRoute);
								}
							}
							HorizOffset += HorizSpacer;  
							if(HorizOffset >= (maxHorizspace - HorizSpacer) )
							{ //start again - squeeze them in between
								HorizSpacer = HorizSpacer/2;
								HorizOffset = HorizSpacer;
							}
							#endregion routed PDO
						}
					}
					#endregion createscreen routing points for all PDOs already in system
				} 
			}
		}
		#endregion layout - System PDOs
		#region layout - SPDO Interface Panels
		private void layoutTxPDOInterfacePanels()
		{
			Panel ipPnl = (Panel) this.InterfacePanels[(int) IFacePnls.SysPDOTx];
			Panel firstTxScreenNode = (Panel)this.TxPDONodePanels[0];
			ipPnl.Left = firstTxScreenNode.Left - ipPnl.Width - 20;
			ipPnl.Top = firstTxScreenNode.Top;
			#region measure the text we want to place in the interface panle
			Graphics tempG = ipPnl.CreateGraphics();
			StringFormat vertStringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
			string lbltxt = "Tx PDO mappings for node ID " + this.currTxPDOnode.nodeID.ToString();
			SizeF lblSizeF = tempG.MeasureString(lbltxt, mainWindowFont, 30, vertStringFormat);
			Size lblSizeInt = new Size((int) Math.Ceiling(lblSizeF.Width),(int) Math.Ceiling(lblSizeF.Height));  //do this to ensure we round up - Not truncate
			tempG.Dispose();
			#endregion measure the text we want to place in the interface panle
			#region set interface panle heigh to max of its text length, the screen nodes range and the SPOD mappigns GroupBoxes range
			Panel lastTxScreenNode = (Panel) this.TxPDONodePanels[this.TxPDONodePanels.Count - 1];
			//fix Interface panel height ot be max of height required for text and height of TxScreen nodes - readablity
			ipPnl.Height = Math.Max(lastTxScreenNode.Bottom - firstTxScreenNode.Top, lblSizeInt.Height + 6); //give text 6 pixels breathing space
			if(this.txSysPDOExpansionPnls.Count>0)
			{ //if we have  mappings being expandes then extend the Interfac epanel height ot max of these and screen nodes range - readability
				ArrayList currGBs = (ArrayList) this.txSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currTxPDOnode)];
				if(currGBs.Count>0)
				{
					int SPDOLBsHeight = ((GroupBox) currGBs[0]).Height - ((GroupBox) currGBs[currGBs.Count-1]).Height;
					ipPnl.Height = Math.Max(ipPnl.Height , SPDOLBsHeight);
				}
			}
			#endregion set interface panle heigh to max of its text length, the screen nodes range and the SPOD mappigns GroupBoxes range
		}
		private void layoutRxPDOInterfacePanels()
		{
			Panel opPnl = (Panel) this.InterfacePanels[(int)IFacePnls.SysPDORx];
			Panel firstRxScreenNode = (Panel)this.RxPDONodePanels[0];
			Panel lastRxScreenNode = (Panel) this.RxPDONodePanels[this.RxPDONodePanels.Count - 1];
			opPnl.Left = firstRxScreenNode.Right + 20;
			opPnl.Top = firstRxScreenNode.Top;
			#region measure the text we want to place in the interface panle
			Graphics tempG = opPnl.CreateGraphics();
			StringFormat vertStringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
			string lbltxt = "Rx PDO mappings for node ID " + this.currRxPDOnode.nodeID.ToString();
			SizeF lblSizeF = tempG.MeasureString(lbltxt, mainWindowFont, 30, vertStringFormat);
			Size lblSizeInt = new Size((int) Math.Ceiling(lblSizeF.Width),(int) Math.Ceiling(lblSizeF.Height));  //do this to ensure we round up - Not truncate
			tempG.Dispose();
			#endregion measure the text we want to place in the interface panle
			#region set interface panle heigh to max of its text length, the screen nodes range and the SPOD mappigns GroupBoxes range
			//fix Interface panel height ot be max of height required for text and height of TxScreen nodes - readablity
			opPnl.Height = Math.Max(lastRxScreenNode.Bottom - firstRxScreenNode.Top, lblSizeInt.Height + 6); //give text 6 pixels breathing space
			if(this.rxSysPDOExpansionPnls.Count>0)
			{ //if we have  mappings being expandes then extend the Interfac epanel height ot max of these and screen nodes range - readability
				ArrayList currGBs = (ArrayList) this.rxSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currRxPDOnode)];
				if(currGBs.Count>0)
				{
					int SPDOLBsHeight = ((GroupBox) currGBs[0]).Height - ((GroupBox) currGBs[currGBs.Count-1]).Height;
					opPnl.Height = Math.Max(opPnl.Height , SPDOLBsHeight);
				}
			}
			#endregion set interface panle heigh to max of its text length, the screen nodes range and the SPOD mappigns GroupBoxes range
		}

		private void txInterfacePanel_Paint(object sender, PaintEventArgs e)
		{
			Panel pnl = (Panel) sender;
			StringFormat vertStringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
			string lbltxt = "Tx PDO mappings for node ID " + this.currTxPDOnode.nodeID.ToString();
			SizeF lblSizeF = e.Graphics.MeasureString(lbltxt, mainWindowFont, 30, vertStringFormat);
			Size lblSizeInt = new Size((int) Math.Ceiling(lblSizeF.Width),(int) Math.Ceiling(lblSizeF.Height));  //do this to ensure we round up - Not truncate
			Rectangle lblBounds = new Rectangle((pnl.Width-lblSizeInt.Width)/2, (int)(pnl.Height-lblSizeInt.Height)/2, (int)lblSizeInt.Width, (int)lblSizeInt.Height);
			e.Graphics.DrawString(lbltxt, mainWindowFont, Brushes.Black,lblBounds, vertStringFormat);
		}

		private void rxInterfacePanel_Paint(object sender, PaintEventArgs e)
		{
			Panel pnl = (Panel) sender;
			StringFormat vertStringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
			string lbltxt = "Rx PDO mappings for node ID " + this.currRxPDOnode.nodeID.ToString();
			SizeF lblSizeF = e.Graphics.MeasureString(lbltxt , mainWindowFont, 30, vertStringFormat);
			Size lblSizeInt = new Size((int) Math.Ceiling(lblSizeF.Width),(int) Math.Ceiling(lblSizeF.Height));  //do this to ensure we round up - Not truncate
			Rectangle lblBounds = new Rectangle((pnl.Width-lblSizeInt.Width)/2, (int)(pnl.Height-lblSizeInt.Height)/2, (int)lblSizeInt.Width, (int)lblSizeInt.Height);
			e.Graphics.DrawString(lbltxt, mainWindowFont, Brushes.Black,lblBounds, vertStringFormat);
		}
		#endregion SPDO Interface Panels
		#region layout - SPDO expansion panels
		private void layoutTxSysPDOExpansionPanels()
		{
			Panel TxIfPnl = (Panel) this.InterfacePanels[(int) IFacePnls.SysPDOTx];
			int vertOffset = TxIfPnl.Top;

			//first remove any SPDO expansion panles from the screen
			foreach(ArrayList al in this.txSysPDOExpansionPnls)
			{
				foreach(GroupBox gb in al)
				{
					if(this.PDORoutingPanel.Controls.Contains(gb) == true)
					{
						this.PDORoutingPanel.Controls.Remove(gb);
					}
				}
			}

			//get the AL of expansion panels for the current Tx screen CANnode
			ArrayList GBs = (ArrayList) this.txSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currTxPDOnode)];
			foreach (GroupBox gb in GBs)
			{
				gb.Left = 10;//judetemp
				gb.Width = TxIfPnl.Left - 10;
				gb.Top = vertOffset;
				gb.Text = ((COBObject) gb.Tag).name; //we may have change dto PDO anem so endsure correct here
				vertOffset += this.PDOVertSpacer + gb.Height;
				this.PDORoutingPanel.Controls.Add(gb);
			}
		}
		private void layoutRxSysPDOExpansionPanels()
		{
			//get the AL of expansion panels for the current Tx screen CANnode
			Panel RxIfPnl = (Panel) this.InterfacePanels[(int) IFacePnls.SysPDORx];
			int vertOffset = RxIfPnl.Top;
			//first remove any TxExpansion panels that are currently on the screen
			foreach(ArrayList al in this.rxSysPDOExpansionPnls)
			{
				foreach(GroupBox gb in al)
				{
					if(this.PDORoutingPanel.Controls.Contains(gb) == true)
					{
						this.PDORoutingPanel.Controls.Remove(gb);
					}
				}
			}
			ArrayList GBsToAdd = (ArrayList) this.rxSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currRxPDOnode)];
			foreach (GroupBox gb in GBsToAdd)
			{
				gb.Left =  RxIfPnl.Right;
				gb.Width = this.PDORoutingPanel.ClientRectangle.Right - 10 - gb.Left;
				gb.Top = vertOffset;
				gb.Text = ((COBObject) gb.Tag).name; //we may have change dto PDO anem so endsure correct here
				vertOffset += this.PDOVertSpacer + gb.Height;
				this.PDORoutingPanel.Controls.Add(gb);
			}
		}
		#endregion SPDO expansion panels
		#endregion layout screen controls
		#endregion PDO Routing Panel graphics setup and layout
		#region TreeView setup/filtering meothods for PDO screen
		void expandTreeViewToIncludeALlPDOableItems()
		{
			wasHidingNodesBeforePDOScreenActive = false;
			if(this.tbb_HideShowRO.Pushed == true) //we are hiding nodes
			{
				wasHidingNodesBeforePDOScreenActive = true;
				showReadOnlyNodes();  //strat by ensuring that all nodes are in tree before we start the PDO filter
			}
			#region now do the PDO filter
			#region save treenodes to backup
			GraphBackupTN = (TreeNode) this.GraphCustList_TN.Clone();
			this.SystemBackupTN = (TreeNode) this.SystemStatusTN.Clone();
			this.DCFBackUpTN = (TreeNode) this.DCFCustList_TN.Clone();
			#endregion save treenodes to backup

			#region hide the readonly treenodes (which also hides underlying table rows)
			this.PDOableTreeNodes = new ArrayList();
			for(int tblIndex = 0;tblIndex<this.sysInfo.nodes.Length;tblIndex++)
			{
				if(this.sysInfo.nodes[tblIndex].manufacturer != Manufacturer.UNKNOWN)
				{
					//now expand tree
					showSubIndexNodes(this.SystemStatusTN.Nodes[tblIndex], tblIndex);
					//At this point all indexs and (non number of items) subs are contianed in the treeView
					//Now we need to recurively remove non-PDO mappable subs, subsequent 'emtpty' header treenodes and 
					//other window treenodes with no underlying PDO mappable subs this also inherently removes redundant section TreeNodes
					//hence we do the following until all the offending items have gone
					do
					{
						nodesRemovedFlag = false;  //set false before entry
						hideNonSystemPDOMapableItems(this.SystemStatusTN.Nodes[tblIndex], tblIndex);
					}
					while (nodesRemovedFlag == true);  //chekc at end set inside method
					this.PDOableTreeNodes.Add(this.SystemStatusTN.Nodes[tblIndex]);//store this to allow us to show/hide it
				}
			}
			#region remove redundant system level treenodes
			this.SystemStatusTN.Nodes.Clear();
			foreach(TreeNode tn in this.PDOableTreeNodes)
			{
				this.SystemStatusTN.Nodes.Add(tn);
			}
			this.DCFCustList_TN.Remove();
			this.GraphCustList_TN.Remove();
			#endregion remove redundant system level treenodes
			#endregion hide the readonly treenodes (which also hides underlying table rows)
			#endregion now do the PDO filter
		}
		private void hideNonSystemPDOMapableItems(TreeNode TopTreeNode, int tableIndex)
		{
			ArrayList nodesToGo = new ArrayList();  //use separatre Array list to 'mark' removeable items - cannot delete from a collection that we are stepping through - messes up the underlying dotNET couters
			foreach(TreeNode tNode in TopTreeNode.Nodes)
			{ 
				if(tNode.Nodes.Count > 0) 
				{
					hideNonSystemPDOMapableItems(tNode, tableIndex);  //check next level down
				}
				else //lowest level reached
				{
					treeNodeTag senderNodeTag = (treeNodeTag) tNode.Tag;
					if((senderNodeTag.assocSub == null) //other window at lewest tree level - remove
						|| (senderNodeTag.assocSub.PDOmappable == false)  //EDs marked as non PDO mappable
						|| (senderNodeTag.assocSub.indexNumber == -1)
						|| (senderNodeTag.assocSub.isNumItems == true)) //header row
					{
						#region other window or redundant section
						nodesToGo.Add(tNode);
						#endregion other window
					}
					else  //OD item
					{
						#region OD item
						if( (senderNodeTag.assocSub.accessType == ObjectAccessType.ReadReadWrite)
							|| (senderNodeTag.assocSub.accessType == ObjectAccessType.ReadReadWriteInPreOp)
							|| (senderNodeTag.assocSub.accessType == ObjectAccessType.Constant)
							|| (senderNodeTag.assocSub.accessType == ObjectAccessType.ReadOnly) )  //this mops up number of items
						{  //can only be placed in a Tx mapping
							tNode.ForeColor = Color.Green;
						}
						if( (senderNodeTag.assocSub.accessType == ObjectAccessType.ReadWriteWrite)
							|| (senderNodeTag.assocSub.accessType == ObjectAccessType.ReadWriteWriteInPreOp)
							|| (senderNodeTag.assocSub.accessType == ObjectAccessType.WriteOnly)
							|| (senderNodeTag.assocSub.accessType == ObjectAccessType.WriteOnlyInPreOp) )								
						{ //can only be placed in an Rx mapping
							tNode.ForeColor = Color.DarkViolet;
						}
						#endregion OD item
					}
				}
			}
			foreach(TreeNode nodeToGo in nodesToGo)
			{
				TopTreeNode.Nodes.Remove(nodeToGo);
				nodesRemovedFlag = true;
			}
		}
		private void showVPDOTreeNodes()
		{
			this.hideUserControls();
			this.SystemStatusTN.Nodes.Clear();
			TreeNode temp = (TreeNode) this.PDOableTreeNodes[PDOableCANNodes.IndexOf(this.SevconMasterNode)];
			TreeNode TN_VPDO = (TreeNode) temp.Clone();
			do
			{
				nodesRemovedFlag = false;  //set false before entry
				this.hideNonVPDOItems(TN_VPDO, this.getSevconMasterCANNodeIndex());
			}
			while (nodesRemovedFlag == true);  //chekc at end set inside method
			this.SystemStatusTN.Nodes.Add(TN_VPDO);
			this.SystemStatusTN.Nodes[0].Expand();
			this.showUserControls();
		}
		private void hideNonVPDOItems(TreeNode TopTreeNode, int tableIndex)
		{
			ArrayList nodesToGo = new ArrayList();
			foreach(TreeNode tNode in TopTreeNode.Nodes)
			{ 
				if(tNode.Nodes.Count > 0) 
				{
					hideNonVPDOItems(tNode, tableIndex);  //check next level down
				}
				else //lowest level reached
				{
					treeNodeTag senderNodeTag = (treeNodeTag) tNode.Tag;
					if(senderNodeTag.assocSub == null)  //other window at lewest tree level - remove
					{
						#region other window or redundant section
						nodesToGo.Add(tNode);
						#endregion other window
					}
					else  //OD item
					{
						#region OD item
						ObjDictItem odItem;
						ODItemData odSub = this.sysInfo.nodes[tableIndex].getODSub(senderNodeTag.assocSub.indexNumber,senderNodeTag.assocSub.subNumber, out odItem);
						if(odSub != null)  //B&B
						{
							if(odSub.objectName != (int)this.activeVPDOType)
							{
								nodesToGo.Add(tNode);
							}
							else if((odItem.odItemSubs.Count>1) && (odSub.subNumber != -1))  
							{ //there are multiple subs for the motor sut the user only drags in the ODIndex
								//- espAc does the reset ref email CEH 10/04/06
								//only keep the header row
								nodesToGo.Add(tNode);
							}
						}
						else
						{
							nodesToGo.Add(tNode);  //B&B
						}
						#endregion OD item
					}
				}
			}
			foreach(TreeNode nodeToGo in nodesToGo)
			{
				TopTreeNode.Nodes.Remove(nodeToGo);
				nodesRemovedFlag = true;
			}
		}

		private void hideNonTxPDOItems(TreeNode TopTreeNode, int tableIndex)
		{
			ArrayList nodesToGo = new ArrayList();
			foreach(TreeNode tNode in TopTreeNode.Nodes)
			{ 
				if(tNode.Nodes.Count > 0) 
				{
					hideNonTxPDOItems(tNode, tableIndex);  //check next level down
				}
				else //lowest level reached
				{
					treeNodeTag senderNodeTag = (treeNodeTag) tNode.Tag;
					if(senderNodeTag.assocSub == null)  //other window at lewest tree level - remove
					{
						#region other window or redundant section
						nodesToGo.Add(tNode);
						#endregion other window
					}
					else  //OD item
					{
						#region OD item
						ObjDictItem odItem;
						ODItemData odSub = this.sysInfo.nodes[tableIndex].getODSub(senderNodeTag.assocSub.indexNumber,senderNodeTag.assocSub.subNumber, out odItem);
						if(odSub != null)  //B&B
						{
							if((odSub.PDOmappable == false)
								|| ((odItem.odItemSubs.Count>1) && (odSub.subNumber <= 0)))
							{
								nodesToGo.Add(tNode);
							}
							else 
							{
								if( (odSub.accessType == ObjectAccessType.ReadReadWrite)
									|| (odSub.accessType == ObjectAccessType.ReadReadWriteInPreOp)
									|| (odSub.accessType == ObjectAccessType.Constant)
									|| (odSub.accessType == ObjectAccessType.ReadOnly) )
								{  //can only be placed in a Tx mapping
									tNode.ForeColor = Color.Green;
								}
								if( (odSub.accessType == ObjectAccessType.ReadWriteWrite)
									|| (odSub.accessType == ObjectAccessType.ReadWriteWriteInPreOp)
									|| (odSub.accessType == ObjectAccessType.WriteOnly)
									|| (odSub.accessType == ObjectAccessType.WriteOnlyInPreOp) )								
								{ //can only be placed in an Rx mapping - therefore bin it
									//nodesToGo.Add(tNode); jude changed for display testing TODO need requirement for this redefining
									tNode.ForeColor = Color.DarkViolet;
								}
							}
						}
						#endregion OD item
					}
				}
			}
			foreach(TreeNode nodeToGo in nodesToGo)
			{
				TopTreeNode.Nodes.Remove(nodeToGo);
				nodesRemovedFlag = true;
			}

		}
		/// <summary>
		/// Force treeview to show only treenodes whose lowest offspirnge can be placec in Rx mappings for the selected node
		/// </summary>
		private void hideNonRxPDOItems(TreeNode TopTreeNode, int tableIndex)
		{
			//two seperate counters needed because removeing nodes changes the collection count
			ArrayList nodesToGo = new ArrayList();
			foreach(TreeNode tNode in TopTreeNode.Nodes)
			{ 
				if(tNode.Nodes.Count > 0) 
				{
					hideNonRxPDOItems(tNode, tableIndex);  //check next level down
				}
				else //lowest level reached
				{
					treeNodeTag senderNodeTag = (treeNodeTag) tNode.Tag;
					if(senderNodeTag.assocSub == null)  //other window at lewest tree level - remove
					{
						#region other window or redundant section
						nodesToGo.Add(tNode);
						#endregion other window
					}
					else  //OD item
					{
						#region OD item
						ObjDictItem odItem; //needed
						ODItemData odSub = this.sysInfo.nodes[tableIndex].getODSub(senderNodeTag.assocSub.indexNumber,senderNodeTag.assocSub.subNumber, out odItem);
						if(odSub != null)  //B&B
						{
							if((odSub.PDOmappable == false)
								|| ((odItem.odItemSubs.Count>1) && (odSub.subNumber <= 0)))
							{
								nodesToGo.Add(tNode);
							}
							else 
							{
								if( (odSub.accessType == ObjectAccessType.ReadReadWrite)
									|| (odSub.accessType == ObjectAccessType.ReadReadWriteInPreOp)
									|| (odSub.accessType == ObjectAccessType.Constant)
									|| (odSub.accessType == ObjectAccessType.ReadOnly) )
								{  //can only be placed in a Tx mapping = there fore bin it
									nodesToGo.Add(tNode);
								}
								if( (odSub.accessType == ObjectAccessType.ReadWriteWrite)
									|| (odSub.accessType == ObjectAccessType.ReadWriteWriteInPreOp)
									|| (odSub.accessType == ObjectAccessType.WriteOnly)
									|| (odSub.accessType == ObjectAccessType.WriteOnlyInPreOp) )								
								{ //can only be placed in an Rx mapping 
									
									tNode.ForeColor = Color.DarkViolet;
								}
							}
						}
						#endregion OD item
					}
				}
			}
			foreach(TreeNode nodeToGo in nodesToGo)
			{
				TopTreeNode.Nodes.Remove(nodeToGo);
				nodesRemovedFlag = true;
			}

		}
	
		private void showSubIndexNodes(TreeNode TopTreeNode, int tblIndex)
		{
			foreach(TreeNode treenode in TopTreeNode.Nodes)
			{ 
				if(treenode.Nodes.Count > 0) 
				{
					showSubIndexNodes(treenode, tblIndex);  //check next level down
				}
				else
				{
					treeNodeTag senderNodeTag = (treeNodeTag) treenode.Tag;
					if(senderNodeTag.assocSub != null)  //below this we know we can remove it if it is now the lowest level
					{
						#region grab this itme from th enodes dictionary

						foreach(ObjDictItem odItem in this.sysInfo.nodes[tblIndex].objectDictionary)
						{
							ODItemData firstODSub = (ODItemData) odItem.odItemSubs[0];
							if(firstODSub.indexNumber == senderNodeTag.assocSub.indexNumber)
							{
								foreach(ODItemData sub in odItem.odItemSubs)
								{
									if((sub.subNumber == -1)
										|| (odItem.odItemSubs.Count == 1))
									{
										continue;  // we will already have a treenode
									}
									if(((sub.accessType == ObjectAccessType.ReadOnly)
										|| (sub.accessType == ObjectAccessType.Constant)) 
										&& (sub.subNumber == 0)
										&& (((CANopenDataType) sub.dataType) == CANopenDataType.UNSIGNED8)
										&& (firstODSub.subNumber == -1))
									{
										continue;  //number of itmes only = no Treenode required
									}
									else
									{
										TreeNode subNode = new TreeNode(sub.parameterName, (int)TVImages.DefaultIco, (int)TVImages.DefaultIco);
										subNode.Tag = new treeNodeTag(TNTagType.ODSUB, sysInfo.nodes[tblIndex].nodeID, tblIndex, sub);
										treenode.Nodes.Add(subNode);
									}
								}
								break;
							}
							
						}
						#endregion grab this itme from th enodes dictionary
					}
				}
			}
		}
		private void reshowSystemPDOTreeNodes()
		{
			if((this.activeVPDOType == SevconObjectType.DIGITAL_SIGNAL_OUT)
				|| (this.activeVPDOType == SevconObjectType.ANALOGUE_SIGNAL_OUT)
				|| (this.activeVPDOType == SevconObjectType.MOTOR_DRIVE)
				|| (this.activeVPDOType == SevconObjectType.DIGITAL_SIGNAL_IN)
				|| (this.activeVPDOType == SevconObjectType.ANALOGUE_SIGNAL_IN)
				|| (this.SystemStatusTN.Nodes.Count != this.PDOableCANNodes.Count))

			{ //only do if we need to 
				#region restore the treenodes
				this.treeView1.Nodes.Clear();
				this.treeView1.ImageList = this.anim_icons;
				this.treeView1.Nodes.Add(SystemStatusTN);
				SystemStatusTN.Nodes.Clear();
				foreach(TreeNode tn in this.PDOableTreeNodes)
				{
					this.SystemStatusTN.Nodes.Add(tn);
				}
				#endregion restore the treenodes
			}
		}
		private void addCompleteTxSpeedControlMotorPDOTreeNode(nodeInfo node, TreeNode parentTN)
		{
			string [] TNnodeNames = {"Traction Left", "Traction Right", "Pump", "Power Steer"};
            string[] TNSlaveNodeNames = { "Motor1", "Motor2", "Motor3", "Motor4" }; //appropriate slave names
			int [] TNODIndexVals = {0xFF0, 0xFF1, 0xFF2, 0xFF3};

			for (int i = 0;i<node.preFilledTxMotorSPDOMappings.Count;i++)
			{
				ArrayList mappingSet = (ArrayList) node.preFilledTxMotorSPDOMappings[i];
				if(mappingSet.Count>0) //empty onse have been cleared by error handling
				{
                    TreeNode TNtxPreFill;
                    if (node == this.SevconMasterNode)
                    {
                        TNtxPreFill = new TreeNode(TNnodeNames[i], (int)TVImages.SelfChar, (int)TVImages.SelfChar);
                    }
                    else
                    {
                        TNtxPreFill = new TreeNode(TNSlaveNodeNames[i], (int)TVImages.SelfChar, (int)TVImages.SelfChar);
                    }

					TNtxPreFill.Tag = new treeNodeTag((TNTagType)TNODIndexVals[i], node.nodeID, this.getCANNodeIndexOfCANNode(node), null);;
					parentTN.Nodes.Add(TNtxPreFill);
				}
			}
		}
		private void addCompleteRxSpeedControlMotorPDOTreeNode(nodeInfo node, TreeNode parentTN)
		{
			string [] TNnodeNames = {"Traction Left", "Traction Right", "Pump", "Power Steer"};
            string[] TNSlaveNodeNames = { "Motor1", "Motor2", "Motor3", "Motor4" }; //appropriate slave names
			int [] TNODIndexVals = {0xFF0, 0xFF1, 0xFF2, 0xFF3};
			for (int i = 0;i<node.preFilledRxMotorSPDOMappings.Count;i++)
			{
				ArrayList mappingSet = (ArrayList) node.preFilledRxMotorSPDOMappings[i];
				if(mappingSet.Count>0) //empty onse have been cleared by error handling
				{
                    TreeNode TNrxPreFill;
                    if (node == this.SevconMasterNode)
                    {
                        TNrxPreFill = new TreeNode(TNnodeNames[i], (int)TVImages.SelfChar, (int)TVImages.SelfChar);
                    }
                    else
                    {
                        TNrxPreFill = new TreeNode(TNSlaveNodeNames[i], (int)TVImages.SelfChar, (int)TVImages.SelfChar);
                    }
					TNrxPreFill.Tag = new treeNodeTag((TNTagType) TNODIndexVals[i], node.nodeID, this.getCANNodeIndexOfCANNode(node), null);
					parentTN.Nodes.Add(TNrxPreFill);
				}
			}
		}
		private void showRxSPDOItemsForThisCANNode(nodeInfo rxCANNode)
		{
			this.hideUserControls();
			this.SystemStatusTN.Nodes.Clear();
			int CANNodeIndex = this.getCANNodeIndexOfCANNode(rxCANNode);
			//clone the TreeNode 
			#region add any OD items that can be mapped into Rx PDO
			//			TreeNode temp = (TreeNode) this.PDOableTreeNodes[PDOableCANNodes.IndexOf(rxCANNode)];
			TreeNode TN_RxOnly = null; 
			foreach(TreeNode tn in this.PDOableTreeNodes)
			{ //need to do this - can't guarantee that treendoes are in same order as the screen nodes
				treeNodeTag tnTag = (treeNodeTag) tn.Tag;
				if(tnTag.nodeID == rxCANNode.nodeID)
				{
					TN_RxOnly = (TreeNode) tn.Clone();
				}
			}
			if( TN_RxOnly == null)
			{
				return;
			}
			#region bin all the non Rx Itesm
			do
			{
				nodesRemovedFlag = false;  //set false before entry
				this.hideNonRxPDOItems(TN_RxOnly, CANNodeIndex );
			}
			while (nodesRemovedFlag == true);  //chekc at end set inside method
			#endregion bin all the non Rx Itesm
			this.SystemStatusTN.Nodes.Add(TN_RxOnly);
			#endregion add any OD items that can be mapped into Rx PDO

			#region add any Rx pre-filled PDO mappings
			if((rxCANNode.isSevconApplication() == true) //pre-fill are only availalbe for Sevcon Apps
				&& (rxCANNode.preFilledRxMotorSPDOMappings.Count>0)//that are either Master OR a slave with assoc Motor Profile
				&& (this.currCOB.transmitNodes.Count <= 1) //Zero or One is OK - this allows for setting up isolted device
				&& (this.currCOB.receiveNodes.Count == 1)) // this is the Rx end so one onyl is permitted
			{
				if(this.currCOB.transmitNodes.Count == 1)
				{
					#region if we want to are route this PDO as a wrap around - the the Tx CAN ndoe also has to be suitable Sevcon app
					COBObject.PDOMapData txData = (COBObject.PDOMapData) this.currCOB.transmitNodes[0];
					foreach( nodeInfo txNode in this.PDOableCANNodes)
					{ //chekc that the Tx CNA node for this PDO is a Sevcon app that is either a master OR a slave with assoc Motor profile
						if((txData.nodeID == txNode.nodeID)	&& (txNode.isSevconApplication() == true) 
							&& (txNode.preFilledTxMotorSPDOMappings.Count>0))
						{
							COBObject.PDOMapData rxData = (COBObject.PDOMapData) this.currCOB.receiveNodes[0];
							if((SevconMasterNode != null) 
								&& ((txNode == this.SevconMasterNode) || (rxCANNode == this.SevconMasterNode)))
							{
								TreeNode TNprefill = new TreeNode("Pre-filled Rx PDO mapping sets", (int) TVImages.COBExtPDO, (int) TVImages.COBExtPDO);
								TNprefill.Tag = new treeNodeTag(TNTagType.XMLHEADER, rxCANNode.nodeID, CANNodeIndex, null);
								addCompleteRxSpeedControlMotorPDOTreeNode(rxCANNode, TNprefill);
								this.SystemStatusTN.Nodes[0].Nodes.Insert(0, TNprefill);
								this.SystemStatusTN.Nodes[0].Nodes[0].Expand(); //and the pre-fills if there are any
								break;
							}
						}
					}
					#endregion if we want to are route this PDO as a wrap around - the the Tx CAN ndoe also has to be suitable Sevcon app
				}
				else if (this.currCOB.transmitNodes.Count == 0)
				{
					#region need to allow user to set up PDOs on a isolated device
					TreeNode TNprefill = new TreeNode("Pre-filled Rx PDO mapping sets", (int) TVImages.COBExtPDO, (int) TVImages.COBExtPDO);
					TNprefill.Tag = new treeNodeTag(TNTagType.XMLHEADER, rxCANNode.nodeID, CANNodeIndex, null);
					addCompleteRxSpeedControlMotorPDOTreeNode(rxCANNode, TNprefill);
					this.SystemStatusTN.Nodes[0].Nodes.Insert(0, TNprefill);
					this.SystemStatusTN.Nodes[0].Nodes[0].Expand(); //and the pre-fills if there are any
					#endregion need to allow user to set up PDOs on a isolated device
				}
			}
			#endregion add any Rx pre-filled PDO mappings
			this.SystemStatusTN.Nodes[0].Expand();//expan the CAN device Treenode
			this.showUserControls();
		}
		private void showTxSPDOItemsForThisCANNode(nodeInfo txCANnode)
		{
			this.hideUserControls();
			this.SystemStatusTN.Nodes.Clear();
			#region add any OD items that can be mapped into Rx PDO
			int CANNodeIndex = this.getCANNodeIndexOfCANNode(txCANnode);
			//clone the TreeNode 
			TreeNode TN_TxOnly = null;
			foreach(TreeNode tn in this.PDOableTreeNodes)
			{ //need to do this - can't guarantee that treendoes are in same order as the screen nodes
				treeNodeTag tnTag = (treeNodeTag) tn.Tag;
				if(tnTag.nodeID == txCANnode.nodeID)
				{
					TN_TxOnly = (TreeNode) tn.Clone();
				}
			}
			if(TN_TxOnly == null)
			{
				return;
			}
			#region bin all the non Tx Itesm
			do
			{
				nodesRemovedFlag = false;  //set false before entry
				this.hideNonTxPDOItems(TN_TxOnly, CANNodeIndex );
			}
			while (nodesRemovedFlag == true);  //chekc at end set inside method
			#endregion bin all the non Tx Itesm
			this.SystemStatusTN.Nodes.Add(TN_TxOnly);
			#endregion add any OD items that can be mapped into Rx PDO

			#region add any Tx pre-filled PDO mappings
			if((txCANnode.isSevconApplication() == true)   //pre-fill are only availalbe for Sevcon Apps
				&& (txCANnode.preFilledTxMotorSPDOMappings.Count>0) //that are either Master OR a slave with assoc Motor Profile
				&& (this.currCOB.transmitNodes.Count == 1) //this is Tx side so must have one Tx node for this COB
				&& (this.currCOB.receiveNodes.Count <= 1)) //Allow zero or one - so we can also set up devcies in isolation
			{
				if(this.currCOB.receiveNodes.Count == 1)
				{ 
					#region if we are routing through then the Rx CAN node has to be a suitable sevcon App
					COBObject.PDOMapData rxData = (COBObject.PDOMapData) this.currCOB.receiveNodes[0];
					foreach( nodeInfo rxNode in this.PDOableCANNodes)
					{ //check that the Rx CAN node for this PDO is a Sevcon app that is either a master OR a slave with assoc Motor profile
						if((rxData.nodeID == rxNode.nodeID)
							&& (rxNode.isSevconApplication() == true) 
							&& (rxNode.preFilledTxMotorSPDOMappings.Count>0))
						{
							if((this.SevconMasterNode != null) 
								&& ((txCANnode == this.SevconMasterNode) || (rxNode == this.SevconMasterNode)))
							{
								TreeNode TNprefill = new TreeNode("Pre-filled Tx PDO mapping sets", (int) TVImages.COBExtPDO, (int) TVImages.COBExtPDO);
								TNprefill.Tag = new treeNodeTag(TNTagType.XMLHEADER, txCANnode.nodeID, CANNodeIndex, null);
								addCompleteTxSpeedControlMotorPDOTreeNode(txCANnode, TNprefill);
								this.SystemStatusTN.Nodes[0].Nodes.Insert(0, TNprefill);
								this.SystemStatusTN.Nodes[0].Nodes[0].Expand();  //and the pre-fills if there are any
								break;
							}
						}
					}
					#endregion if we are routing through then the Rx CAN node has to be a suitable sevcon App
				}
				else if(this.currCOB.receiveNodes.Count == 0)
				{
					#region we need to allow the user to set up an isolated device
					TreeNode TNprefill = new TreeNode("Pre-filled Tx PDO mapping sets", (int) TVImages.COBExtPDO, (int) TVImages.COBExtPDO);
					TNprefill.Tag = new treeNodeTag(TNTagType.XMLHEADER, txCANnode.nodeID, CANNodeIndex, null);
					addCompleteTxSpeedControlMotorPDOTreeNode(txCANnode, TNprefill);
					this.SystemStatusTN.Nodes[0].Nodes.Insert(0, TNprefill);
					this.SystemStatusTN.Nodes[0].Nodes[0].Expand();  //and the pre-fills if there are any
					#endregion we need to allow the user to set up an isolated device
				}
			}
			#endregion add any Rx pre-filled PDO mappings
			this.SystemStatusTN.Nodes[0].Expand();//expan the CAN device Treenode
			this.showUserControls();
		}

		#endregion TreeView setup for PDO screen
		#region PDO routing Panel event hanlders
		private void PDORoutingPanel_MouseDown(object sender, MouseEventArgs e)
		{
			ContextMenu CM_PDORoutingPanel = new ContextMenu();;
			if(e.Button ==MouseButtons.Right)
			{
				MenuItem miExit = new MenuItem("Exit Communications set up");
				miExit.Click +=new EventHandler(COBSetupExit_Click);
				CM_PDORoutingPanel.MenuItems.Add(miExit);
			}
			Rectangle mouseRect = new Rectangle(e.X-1, e.Y-1,2,2);
			#region test for intersction with a System PDO route line
			foreach(COBObject COB in this.sysInfo.COBsInSystem)
			{
				foreach(COBObject.screenRoutePoints COBRoute in COB.screenRoutes)
				{
					#region create route rectangles that incorporate the offset to their accosiated screen node
					//we MUST do it this way for autoscroll to work properly - thenthe routes are always snapped back to their
					//assoc screen nodes on repaint
					Panel txScrNode = null, rxScrNode = null;
					Rectangle offsetStartRect = new Rectangle(0,0,0,0);
					Rectangle offsetMidRect = new Rectangle(0,0,0,0);
					Rectangle offsetEndRect = new Rectangle(0,0,0,0);
					if(COBRoute.TxNodeScreenIndex != -1)
					{//referneces are tot hetxScrNode Top right Corner
						txScrNode = (Panel) this.TxPDONodePanels[COBRoute.TxNodeScreenIndex];
						offsetStartRect = new Rectangle(COBRoute.startRect.X + txScrNode.Right, COBRoute.startRect.Y + txScrNode.Top, COBRoute.startRect.Width, COBRoute.startRect.Height);
						offsetMidRect = new Rectangle(COBRoute.midRect.X + txScrNode.Right, COBRoute.midRect.Y + txScrNode.Top, COBRoute.midRect.Width, COBRoute.midRect.Height);
						offsetEndRect = new Rectangle(COBRoute.endRect.X + txScrNode.Right, COBRoute.endRect.Y + txScrNode.Top, COBRoute.endRect.Width, COBRoute.endRect.Height);
					}
					else if(COBRoute.RxNodeScreenIndex != -1)
					{  //unprdcuced PDO  = screen reference sare to the rxScrNode topLeft corner
						rxScrNode = (Panel) this.RxPDONodePanels[COBRoute.RxNodeScreenIndex];
						offsetEndRect = new Rectangle(COBRoute.endRect.X + rxScrNode.Left, COBRoute.endRect.Y + rxScrNode.Top, COBRoute.endRect.Width, COBRoute.endRect.Height);
					}
					#endregion create route rectangles that incorporate the offset to their accosiated screen node
					if((offsetStartRect.IntersectsWith(mouseRect) == true)
						|| (offsetMidRect.IntersectsWith(mouseRect) == true)
						|| (offsetEndRect.IntersectsWith(mouseRect) == true))
					{
						bool TxNodeChangeFlag = false, RxNodeChangeFlag = false;
						if(e.Button ==MouseButtons.Right)
						{
							#region add SystemPDO specific Context menu items and show context menu
							MenuItem MI_removethisPDO = new MenuItem("Remove PDO: " + COB.name + " completely");
							MI_removethisPDO.Click +=new EventHandler(MI_removethisPDO_Click);
							CM_PDORoutingPanel.MenuItems.Add(MI_removethisPDO);
							#endregion add SystemPDO specific Context menu items and show context menu
						}
						#region highlight correct part of treeview as required
						if((offsetStartRect.IntersectsWith(mouseRect) == true)	&& (COBRoute.TxNodeScreenIndex != -1))
						{
							#region intersection wit hTx end fo SPDO route
							if((e.Button == MouseButtons.Right) && ((COB.receiveNodes.Count + COB.transmitNodes.Count)>1))
							{ //if COB has only one branch then we MUST call the remove PDO completely method 
								MenuItem MI_removeThisTxNodeFromThisPDO = new MenuItem("Remove this transmit branch from PDO, " + COB.name);
								nodeIDOfPDORouteLegToRemove = ((nodeInfo) this.PDOableCANNodes[COBRoute.TxNodeScreenIndex]).nodeID;
								MI_removeThisTxNodeFromThisPDO.Click +=new EventHandler(MI_removeThisTxNodeFromThisPDO_Click);
								CM_PDORoutingPanel.MenuItems.Add(MI_removeThisTxNodeFromThisPDO);
							}
							if( (nodeInfo) this.PDOableCANNodes[COBRoute.TxNodeScreenIndex] != this.currTxPDOnode)
							{
								TxNodeChangeFlag = true;
							}
							#endregion intersection wit hTx end fo SPDO route
						}
						else if((offsetEndRect.IntersectsWith(mouseRect) == true) && (COBRoute.RxNodeScreenIndex != -1))
						{
							#region intersection with Rx end of SPDO route
							if((e.Button == MouseButtons.Right)  && ((COB.receiveNodes.Count + COB.transmitNodes.Count)>1))
							{ //if COB has only one branch then we MUST call the remove PDO completely method
								MenuItem MI_removeThisRxNodeFromThisPDO = new MenuItem("Remove this receive branch from PDO, " + COB.name);
								nodeIDOfPDORouteLegToRemove = ((nodeInfo) this.PDOableCANNodes[COBRoute.RxNodeScreenIndex]).nodeID;
								MI_removeThisRxNodeFromThisPDO.Click +=new EventHandler(MI_removeThisRxNodeFromThisPDO_Click);
								CM_PDORoutingPanel.MenuItems.Add(MI_removeThisRxNodeFromThisPDO);
							}
							if((nodeInfo) this.PDOableCANNodes[COBRoute.RxNodeScreenIndex] != this.currRxPDOnode)
							{
								RxNodeChangeFlag = true;
							}
							#endregion intersection with Rx end of SPDO route
						}
						#endregion highlight correct part of treeview as required
						if(e.Button ==MouseButtons.Right)
						{//do at end when all bits have beed added
							CM_PDORoutingPanel.Show(this.PDORoutingPanel, new Point(e.X, e.Y));
						}
						if((TxNodeChangeFlag == true)
							|| (RxNodeChangeFlag == true)
							|| (COB != this.currCOB))
						{  //something chaged so redo routes and mapings
							this.PDORoutingPanel.Resize -=new EventHandler(PDORoutingPanel_Resize);
							this.hideUserControls();
							deActivateInternalPDOs();
							if(TxNodeChangeFlag == true)
							{
								showTxSPDOItemsForThisCANNode((nodeInfo) this.PDOableCANNodes[COBRoute.TxNodeScreenIndex]);
								this.activateTxScreenNode((nodeInfo)this.PDOableCANNodes[COBRoute.TxNodeScreenIndex]);
							}
							if(RxNodeChangeFlag == true)
							{
								showRxSPDOItemsForThisCANNode((nodeInfo) this.PDOableCANNodes[COBRoute.RxNodeScreenIndex]);
								this.activateRxScreenNode((nodeInfo) this.PDOableCANNodes[COBRoute.RxNodeScreenIndex]);
							}
							if(COB != this.currCOB)
							{
								activateCOB(COB);
							}
							this.calculateScreenLinesForPDOCOBSInSystem();
							this.drawAndFillMappings();
							this.layoutPDOMappedBitsPanel();
							this.showUserControls();
							this.PDORoutingPanel.Resize +=new EventHandler(PDORoutingPanel_Resize);
						}
						return;
					}
				}
			}
			#endregion test for intersction with a route line
			#region all other parts of panel
			//if we get to here effectively deselect all PDOs by setting COB to first enrty - usuallty emergency
			if(this.currCOB != (COBObject) this.sysInfo.COBsInSystem[0])
			{
				this.PDORoutingPanel.Resize -=new EventHandler(PDORoutingPanel_Resize);
				this.hideUserControls();
				deActivateInternalPDOs();
				activateCOB((COBObject) this.sysInfo.COBsInSystem[0]);
				this.showUserControls();
				this.PDORoutingPanel.Resize +=new EventHandler(PDORoutingPanel_Resize);
			}
			if(e.Button ==MouseButtons.Right)
			{
				//show only generic context menu items
				CM_PDORoutingPanel.Show(this.PDORoutingPanel, new Point(e.X, e.Y));
			}
			
			#endregion all other parts of panel
		}
		private void PDORoutingPanel_MouseMove(object sender, MouseEventArgs e)
		{
			//check for intersection with Internal PDO
			Rectangle mouseRect = new Rectangle(e.X - 1, e.Y - 1,2,2);
			foreach(COBObject COB in this.sysInfo.COBsInSystem)
			{
				if(COB.messageType == COBIDType.PDO)
				{
					foreach(COBObject.screenRoutePoints COBRoute in COB.screenRoutes)
					{
						#region create route rectangles that incorporate the offset to their accosiated screen node
						//we MUST do it this way for autoscroll to work properly - thenthe routes are always snapped back to their
						//assoc screen nodes on repaint
						Panel txScrNode = null, rxScrNode = null;
						Rectangle offsetStartRect = new Rectangle(0,0,0,0);
						Rectangle offsetMidRect = new Rectangle(0,0,0,0);
						Rectangle offsetEndRect = new Rectangle(0,0,0,0);
						if(COBRoute.TxNodeScreenIndex != -1)
						{//referneces are tot hetxScrNode Top right Corner
							txScrNode = (Panel) this.TxPDONodePanels[COBRoute.TxNodeScreenIndex];
							offsetStartRect = new Rectangle(COBRoute.startRect.X + txScrNode.Right, COBRoute.startRect.Y + txScrNode.Top, COBRoute.startRect.Width, COBRoute.startRect.Height);
							offsetMidRect = new Rectangle(COBRoute.midRect.X + txScrNode.Right, COBRoute.midRect.Y + txScrNode.Top, COBRoute.midRect.Width, COBRoute.midRect.Height);
							offsetEndRect = new Rectangle(COBRoute.endRect.X + txScrNode.Right, COBRoute.endRect.Y + txScrNode.Top, COBRoute.endRect.Width, COBRoute.endRect.Height);
						}
						else if(COBRoute.RxNodeScreenIndex != -1)
						{  //unprdcuced PDO  = screen reference sare to the rxScrNode topLeft corner
							rxScrNode = (Panel) this.RxPDONodePanels[COBRoute.RxNodeScreenIndex];
							offsetEndRect = new Rectangle(COBRoute.endRect.X + rxScrNode.Left, COBRoute.endRect.Y + rxScrNode.Top, COBRoute.endRect.Width, COBRoute.endRect.Height);
						}
						#endregion create route rectangles that incorporate the offset to their accosiated screen node
						if(offsetStartRect.IntersectsWith(mouseRect))
						{
							COBObject.PDOMapData txData = (COBObject.PDOMapData) COB.transmitNodes[0];
							StringBuilder sb = new StringBuilder();
							sb.Append(COB.name);
							sb.Append(": ");
							sb.Append("transmitted signals:");
							foreach(PDOMapping map in txData.SPDOMaps)
							{
								sb.Append("\n" + map.mapName);
							}
							this.toolTip1.SetToolTip(this.PDORoutingPanel, sb.ToString());
							return;
						}
                        //DR38000270 RxNodeScreenIndex value of -1 caused an exception
                        if ((offsetEndRect.IntersectsWith(mouseRect)) && (COBRoute.RxNodeScreenIndex != -1))
						{
							nodeInfo node = (nodeInfo) this.PDOableCANNodes[COBRoute.RxNodeScreenIndex];
							foreach(COBObject.PDOMapData rxData in COB.receiveNodes)
							{
								if(node.nodeID == rxData.nodeID)
								{//we have found corresponding rxData
									StringBuilder sb = new StringBuilder();
									sb.Append(COB.name);
									sb.Append(": ");
									sb.Append("unmapped to:");
									foreach(PDOMapping map in rxData.SPDOMaps)
									{
										sb.Append("\n" + map.mapName);
									}
									this.toolTip1.SetToolTip(this.PDORoutingPanel, sb.ToString());
									return;
								}
							}
						}
					}
				}
			}
			this.toolTip1.SetToolTip(this.PDORoutingPanel, "");
		}

		#endregion PDO routing Panel event hanlders
		#region event handlers for Screen representations of CAN nodes
		private void TxScreenNode_MouseDown(object sender, MouseEventArgs e)
		{
			#region get the correct scrNode panel
			Panel txScrNode;
			Control control = (Control) sender;
			if(control is System.Windows.Forms.Panel)
			{
				txScrNode = (Panel) sender;
			}
			else if (control.Parent is System.Windows.Forms.Panel)
			{
				txScrNode = (Panel) control.Parent;
			}
			else
			{
				return; //error condition
			}
			#endregion get the correct scrNode panel
			#region get the index for this screen node
			tempTxScrIndex = this.TxPDONodePanels.IndexOf(txScrNode);
			#endregion get the index for this screen node
			if(this.PDOableCANNodes.IndexOf(this.currTxPDOnode) != this.TxPDONodePanels.IndexOf(txScrNode))
			{  //user has clicked on different screen node
				this.hideUserControls();
				activateTxScreenNode((nodeInfo)  this.PDOableCANNodes[this.TxPDONodePanels.IndexOf(txScrNode)]);
				this.showUserControls();
			}
			#region abort if thisnode alreadt transmits its max or 9 PDOs
			//technically 9 is a Sevcon limit - but it makes sense to apply it to all nodes
			nodeInfo thisCANNode = (nodeInfo) this.PDOableCANNodes[tempTxScrIndex];
			if(thisCANNode.disabledTxPDOIndexes.Count <1)  //we can only route to disabels ones - the rest are in use ( some may be fixed)
			{
				this.statusBarPanel3.Text = "No more PDOs can be transmitted by this node";
				return;
			}
			#endregion abort if thisnode alreadt transmits 9 PDOs
			#region abort if this is Sevcon application && not in pre-op
			nodeInfo correspCANNode = (nodeInfo) this.PDOableCANNodes[tempTxScrIndex];
			if(correspCANNode.isSevconApplication() == true)
			{
                if (((SCCorpStyle.ProductRange) correspCANNode.productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY)
                    && (correspCANNode.nodeState == NodeState.Unknown))
                {
                    DIFeedbackCode feedback = correspCANNode.readODValue(correspCANNode.nmtStateodSub);
                    if ((feedback == DIFeedbackCode.DISuccess) && ((NodeState)correspCANNode.nmtStateodSub.currentValue != NodeState.PreOperational))
                    {
                        this.statusBarPanel3.Text = "Sevcon controllers must be in pre-op to edit PDOs";
					    return;
                    }
                }
                else if (correspCANNode.nodeState != NodeState.PreOperational)
                {
                    if (((SCCorpStyle.ProductRange)correspCANNode.productRange != SCCorpStyle.ProductRange.SEVCONDISPLAY)
                        && ((SCCorpStyle.ProductRange)correspCANNode.productRange != SCCorpStyle.ProductRange.PST))
                    {
                        this.statusBarPanel3.Text = "Sevcon controllers must be in pre-op to edit PDOs";
                        return;
                    }
                }
			}
			#endregion abort if this is Sevcon application && not in pre-op
			#region abort if this is Sevcon app and user access is too low
			if((correspCANNode.isSevconApplication() == true) && (correspCANNode.accessLevel<SCCorpStyle.AccLevel_SevconPDOWrite))
			{
				this.statusBarPanel3.Text = "Insufficient access to edit PDOs on Sevcon node";
				return;
			}
			#endregion abort if this is Sevcon app and user access is too low
			#region create temporary txData to use when routing is complete or for Tx only PDO creation
			tempTxData = new DriveWizard.COBObject.PDOMapData();
			tempTxData.nodeID = (int) ((nodeInfo) (this.PDOableCANNodes[tempTxScrIndex])).nodeID;
			#endregion create temporary txData to use when routing is complete or for Tx only PDO creation
			if(e.Button == MouseButtons.Right)
			{
				ContextMenu cm = new ContextMenu();
				MenuItem miAddUnconsumedPDO = new MenuItem("Add transmit only PDO to this CAN node");
				if( ((nodeInfo)  this.PDOableCANNodes[this.TxPDONodePanels.IndexOf(txScrNode)]).disabledTxPDOIndexes.Count>0)
				{
					miAddUnconsumedPDO.Click +=new EventHandler(miAddUnconsumedPDO_Click);
				}
				else
				{
					miAddUnconsumedPDO.Text = "No spare TxPDO slots";
					miAddUnconsumedPDO.Enabled = false; //show but disable it 
				}
				cm.MenuItems.Add(miAddUnconsumedPDO);
				cm.Show(txScrNode, new Point(e.X, e.Y));
			}
			else
			{
				if(routeStartPt.X <0)
				{
					#region abort if the currCOB already has one or more Other Tx nodes defined
					if((this.currCOB != null)
						&& (this.currCOB.messageType == COBIDType.PDO) 
						&& (this.currCOB.transmitNodes.Count>0))
					{
						if(this.currCOB.transmitNodes.Count>1)
						{//duplicate COBID on PDO - runaway... runaway
							return;
						}
						COBObject.PDOMapData txData = (COBObject.PDOMapData) this.currCOB.transmitNodes[0];
						if(txData.nodeID != ((nodeInfo) this.PDOableCANNodes[tempTxScrIndex]).nodeID)
						{
							this.statusBarPanel3.Text = "Active PDO already has a Transmit CAN node";
							//this COB already has a tranmit node defined runaway ...runaway
							return;
						}
					}
					#endregion abort if the currCOB already has one or more Other Tx nodes defined
					#region adding a new route to an existing PDO
					txScrNode.BackColor = Color.Green;
					if(this.currCOB != null)
					{
						foreach(COBObject.PDOMapData existTxData in this.currCOB.transmitNodes)  //do a foreach - ie no exception if lengt his zero
						{
							if(existTxData.nodeID == tempTxData.nodeID)
							{	//We ARE adding to existing PDO
								foreach(COBObject.screenRoutePoints route in  currCOB.screenRoutes)
								{//could be duplicate Tx nodes - make sure wetge tthe correct one
									if(route.TxNodeScreenIndex == tempTxScrIndex)
									{
										routeStartPt = new Point(route.startRect.X + ((Panel)this.TxPDONodePanels[tempTxScrIndex]).Right,  route.startRect.Y + ((Panel)this.TxPDONodePanels[tempTxScrIndex]).Top);
										return;
									}
								}
							}
						}
					}
					#endregion adding a new route to an existing PDO
					#region New PDO
					this.PDOUnderConstruction = new COBObject();
					PDOUnderConstruction.transmitNodes.Add(tempTxData);
					routeStartPt = new Point(txScrNode.Right, (int)(txScrNode.Top + (txScrNode.Height/2)));
					#endregion New PDO
				}
			}
		}
		private void RxScrNode_MouseDown(object sender, MouseEventArgs e)
		{
			#region identify which screen node user wants to change to
			Panel rxScrNode;
			Control control = (Control) sender;
			if(control is System.Windows.Forms.Panel)
			{
				rxScrNode = (Panel) sender;
			}
			else if (control.Parent is System.Windows.Forms.Panel)
			{
				rxScrNode = (Panel) control.Parent;
			}
			else
			{
				return; //error condition
			}
			#endregion identify which screen node user wants to change to
			if(this.RxPDONodePanels.Contains(rxScrNode) == true)
			{
				#region Rx Panel
				if(this.PDOableCANNodes.IndexOf(this.currRxPDOnode) != this.RxPDONodePanels.IndexOf(rxScrNode))
				{
					this.hideUserControls();
					activateRxScreenNode((nodeInfo)  this.PDOableCANNodes[this.RxPDONodePanels.IndexOf(rxScrNode)]);
					this.showUserControls();
				}
				#endregion Rx Panel
			}
            #region get the index for this screen node
            int tempRxScrIndex = this.RxPDONodePanels.IndexOf(rxScrNode);
            #endregion get the index for this screen node
            #region abort if this is Sevcon application && not in pre-op
            nodeInfo correspCANNode = (nodeInfo)this.PDOableCANNodes[tempRxScrIndex];
            if (correspCANNode.isSevconApplication() == true)
            {
                if (((SCCorpStyle.ProductRange)correspCANNode.productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY)
                    && (correspCANNode.nodeState == NodeState.Unknown))
                {
                    DIFeedbackCode feedback = correspCANNode.readODValue(correspCANNode.nmtStateodSub);
                    if ((feedback == DIFeedbackCode.DISuccess) && ((NodeState)correspCANNode.nmtStateodSub.currentValue != NodeState.PreOperational))
                    {
                        this.statusBarPanel3.Text = "Sevcon controllers must be in pre-op to edit PDOs";
                        return;
                    }
                }
                else if (correspCANNode.nodeState != NodeState.PreOperational)
                {
                    if (((SCCorpStyle.ProductRange)correspCANNode.productRange != SCCorpStyle.ProductRange.SEVCONDISPLAY)
                        && ((SCCorpStyle.ProductRange)correspCANNode.productRange != SCCorpStyle.ProductRange.PST))
                    {
                        this.statusBarPanel3.Text = "Sevcon controllers must be in pre-op to edit PDOs";
                        return;
                    }
                }
            }
            #endregion abort if this is Sevcon application && not in pre-op
            #region abort if this is Sevcon app and user access is too low
            if ((correspCANNode.isSevconApplication() == true) && (correspCANNode.accessLevel < SCCorpStyle.AccLevel_SevconPDOWrite))
            {
                this.statusBarPanel3.Text = "Insufficient access to edit PDOs on Sevcon node";
                return;
            }
            #endregion abort if this is Sevcon app and user access is too low

            if(e.Button == MouseButtons.Right)  
			{
				tempRxData = new DriveWizard.COBObject.PDOMapData();
				tempRxData.nodeID = (int) ((nodeInfo) (PDOableCANNodes[PDOableCANNodes.IndexOf(this.currRxPDOnode)])).nodeID;
				ContextMenu cm = new ContextMenu();
				MenuItem miAddUnproducedPDO = new MenuItem("Add receive only PDO to this CAN node");
				if(((nodeInfo) PDOableCANNodes[PDOableCANNodes.IndexOf(this.currRxPDOnode)]).disabledRxPDOIndexes.Count>0)
				{
					miAddUnproducedPDO.Click +=new EventHandler(miAddUnproducedPDO_Click);
				}
				else
				{
					miAddUnproducedPDO.Text = "No spare RxPDO slots";
					miAddUnproducedPDO.Enabled = false; //show but disable it 
				}
				cm.MenuItems.Add(miAddUnproducedPDO);
				cm.Show(rxScrNode, new Point(e.X, e.Y));
			}
		}
		private void TxScreenNode_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(routeStartPt.X >=0)  
			{//we are currently trying to rout a PDO
				routeStartPt = new Point(-1,-1);  //reset regardless of sucessful routing
				foreach(Panel txScrNode in this.TxPDONodePanels)
				{
					if(txScrNode.BackColor != Color.White)
					{
						txScrNode.BackColor = Color.White;
						break; //there will be one only - get out as quick as possible - good practice
					}
				}
				this.PDORoutingPanel.Invalidate();  //paint over elastic line
				Rectangle mouseRect = new Rectangle(PDOMousePos, new Size(4,4));
				for(int i = 0;i<this.RxPDONodePanels.Count;i++)
				{
					Panel rxScrNode = (Panel) this.RxPDONodePanels[i];
					Rectangle rxScrNodeRect = new Rectangle(rxScrNode.Location, rxScrNode.Size);  //the recatngle encopassing a rxScreen node
					if(rxScrNodeRect.IntersectsWith(mouseRect) == true)
					{
						#region mouse was released over a Rx Screen node
						nodeInfo thisRxCANnode = (nodeInfo) this.PDOableCANNodes[i];
						nodeInfo thisTxCANnode = (nodeInfo) this.PDOableCANNodes[tempTxScrIndex];

                        #region if display get node state from NMT object
                        bool displayInPreOp = false;
                        bool displayNode = false;
                        if ((thisRxCANnode.isSevconApplication() == true) 
                           && ((SCCorpStyle.ProductRange)thisRxCANnode.productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY)
                           && (thisRxCANnode.nodeState == NodeState.Unknown))
                        {
                            displayNode = true;
                            DIFeedbackCode feedback = thisRxCANnode.readODValue(thisRxCANnode.nmtStateodSub);
                            if ((feedback == DIFeedbackCode.DISuccess) && ((NodeState)thisRxCANnode.nmtStateodSub.currentValue == NodeState.PreOperational))
                            {
                                displayInPreOp = true;
                            }
                        }
                        #endregion if display get node state from NMT object

						#region create COBObject.PDOMapData rxData and txData ready for adding to currCOB as necessary
						COBObject.PDOMapData rxData = new DriveWizard.COBObject.PDOMapData();
						rxData.nodeID = (int) ((nodeInfo) (this.PDOableCANNodes[i])).nodeID;
						#endregion create COBObject.PDOMapData rxData and txData ready for adding to currCOB as necessary
						if(thisRxCANnode.disabledRxPDOIndexes.Count<1)
						{
							#region check if this CAN node can receive any more PDOs
							this.statusBarPanel3.Text = "This node cannot receive any more PDOs";
							return;
							#endregion check if this CAN node can receive any more PDOs
						}
						else if((displayNode == true) && (displayInPreOp == false))
                        {
                            if ((NodeState)thisRxCANnode.nmtStateodSub.currentValue == NodeState.Unknown)
                            {
                                this.statusBarPanel3.Text = "Sevcon display NMT state unknown.";
                                return;
                            }
                            else if ((NodeState)thisRxCANnode.nmtStateodSub.currentValue != NodeState.PreOperational)
                            {
                                this.statusBarPanel3.Text = "Sevcon nodes must be in pre-op to edit PDOs";
                                return;
                            }
						}
						else if((thisRxCANnode.isSevconApplication() == true) && (thisRxCANnode.accessLevel<SCCorpStyle.AccLevel_SevconPDOWrite))
						{
							#region abort if insufficient access on Rx node
							this.statusBarPanel3.Text = "Insuffucuent access to edit PDOs on this Sevcon node";
							return;
							#endregion abort if insufficient access on Rx node
						}
						else
						{
							#region check for attempting to Tx and Rx on same node
							if(rxData.nodeID == this.tempTxData.nodeID)
							{
								#region attmept to Tx and Rx on same node
								this.statusBarPanel3.Text = "Cannot transmit and receive on same CAN node";
								return;  //cannot rout a PDO back to the same node
								#endregion attmept to Tx and Rx on same node
							}
							#endregion check for attempting to Tx and Rx on same node
							#region check if the currCOB aready has route to this rx Node 
							if((this.currCOB != null)  ////B&B for rare condition of no COBs in system
								&& (currCOB.transmitNodes.Count>0))   //we must skip if this us an unproduced PDO
							{
								bool alreadyRouted = false;
								foreach(COBObject.PDOMapData existRx in this.currCOB.receiveNodes)
								{
									if(existRx.nodeID == rxData.nodeID)  //already Routed
									{
										alreadyRouted = true;
										break;
									}
								}
								if(alreadyRouted == true)
								{
									this.statusBarPanel3.Text = "This CAN node already recieves this PDO";
									return; //goto tidy up and repaint
								}
							}
							#endregion - if the currCOB aready has route to this rx Node then abort
							if(this.PDOUnderConstruction != null) 
							{ 
								#region new PDO or we are linking to unproduced PDO Rxd by this node
								foreach(COBObject.PDOMapData existRxData in this.currCOB.receiveNodes)
								{
									if(existRxData.nodeID == rxData.nodeID)
									{
										#region the currentCOB has an unproduced PDO which is Rxd by this rx Node
										COBObject.PDOMapData txData  = (COBObject.PDOMapData) PDOUnderConstruction.transmitNodes[0];
										currCOB.transmitNodes.Add(txData);  //we need to add the Tx node data to the currentCOB - rx data is already in
										this.addSPDOLB(thisTxCANnode, this.tempTxData.SPDOMaps, true, this.currCOB);
										#region pass data to DI
										thisTxCANnode.addTxNodeToPDO(currCOB);  //wite to CANnode
										if(SystemInfo.errorSB.Length>0)
										{
											sysInfo.displayErrorFeedbackToUser("Failed to add transmit node to " + currCOB.name);
										}
										#endregion pass data to DI
										this.PDOUnderConstruction = null;  //get rid of this - we are extending existing unproduced PDO
										break;
										#endregion unproduced PDO which is Rxd by this rx Node
									}
								}
								if(this.PDOUnderConstruction != null) //will have been set to null if we are linking to an unproduced DPO
								{
									#region newly created PDO
									createUniqueNameAndCOIDForAddedPDO("new PDO");
									#region setup empty mappings for this Rx node in COB and add to Rx nodedata Arraylist
									PDOUnderConstruction.receiveNodes.Add(rxData);  //txData is added on Mouse Down
									#endregion  setup empty mappings for this Rx node in COB and add to Rx nodedata Arraylist
									this.sysInfo.COBsInSystem.Add(PDOUnderConstruction); //Note: all other properties - use COBObject defaults
									this.addSPDOLB(thisTxCANnode, this.tempTxData.SPDOMaps, true, PDOUnderConstruction);
									this.addSPDOLB(thisRxCANnode, rxData.SPDOMaps, false, PDOUnderConstruction);
									#region ask DI to update nodes
									thisTxCANnode.addTxNodeToPDO(PDOUnderConstruction);
									thisRxCANnode.addRxNodeToPDO(PDOUnderConstruction);
									if(SystemInfo.errorSB.Length>0)
									{
										sysInfo.displayErrorFeedbackToUser("Failed to add all nodes to " + PDOUnderConstruction.name);
									}
									#endregion ask DI to update nodes
									#endregion add newly created PDO to System
								}
								#endregion new PDO or we are linking to unproduced PDO Rxd by this node
							}
							else
							{ 
								#region modified PDO
								currCOB.receiveNodes.Add(rxData);  //cannot be null here
								this.addSPDOLB(thisRxCANnode, rxData.SPDOMaps, false, this.currCOB);
								#region pass data to DI
								thisRxCANnode.addRxNodeToPDO(currCOB);
								
								#endregion pass data to DI
								if(currCOB.transmitNodes.Count<=0)
								{  //when modifiying a PDO we inly add the transmit node if this is an unproduced PDO
									currCOB.transmitNodes.Add(this.tempTxData);  //this roted OK so we are safe to add the tranmist data to this COB if we need to
									this.addSPDOLB(thisTxCANnode, this.tempTxData.SPDOMaps, true, this.currCOB);
									#region pass data to DI
									thisTxCANnode.addTxNodeToPDO(currCOB);
									#endregion pass data to DI
								}
								if(SystemInfo.errorSB.Length>0)
								{
									sysInfo.displayErrorFeedbackToUser("Failed to add all nodes to " + currCOB.name);
								}
								#endregion modified PDO
							}
						}
						#region update the screen calculations and display to user
						this.hideUserControls();
						if(PDOUnderConstruction != null) //we added a new PDO to COBs Arraylist - hav eto updatre all our dat abindings
						{
							this.updatePDODataBindings(this.sysInfo.COBsInSystem.IndexOf(PDOUnderConstruction)); //true becase COBsInSystem has changed => need to do full rebind
							this.PDOUnderConstruction = null;  //ready for next time
						}
						this.deActivateInternalPDOs();
						this.activateRxScreenNode(thisRxCANnode);
						calculateScreenLinesForPDOCOBSInSystem(); //does full positioning of all sreen routes - Note:we have to make the offsets contiguous again
						drawAndFillMappings();
						this.layoutPDOMappedBitsPanel();
						this.layoutTxSysPDOExpansionPanels();
						this.layoutRxSysPDOExpansionPanels();
						this.showUserControls(); //put this her e- even if we didn't connect we need to remove the elastic line
						#endregion update the screen calculations and display to user
						#endregion mouse was release over a Rx Screen node
					}
				}
				this.PDOUnderConstruction = null;  //ready for next time
			}
		}
		private void TxScreenNode_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(routeStartPt.X >=0) 
			{
				#region identify the Screen node panel from the sender control
				Panel txScrNode;
				Control control = (Control) sender;
				if(control is System.Windows.Forms.Panel)
				{
					txScrNode = (Panel) sender;
					//update the elastic line end point = will be drawn from Paint overrride method
					this.PDOMousePos = new Point(e.X + txScrNode.Left, e.Y + txScrNode.Top);
				}
				else if (control.Parent is System.Windows.Forms.Panel)
				{
					txScrNode = (Panel) control.Parent;
					//update the elastic line end point
					//need to adjust for where control is inside its Parent the screen node panel
					this.PDOMousePos = new Point(e.X + txScrNode.Left + control.Left, e.Y + txScrNode.Top + control.Top);
				}
				else
				{
					return; //error condition
				}
				#endregion identify the Screen node panel from the sender control
				//only invaldate the System PDo routing area - improve performance
				Rectangle invRect = new Rectangle(txScrNode.Right, 0, ((Panel)this.RxPDONodePanels[0]).Left, this.PDORoutingPanel.Height);
				this.PDORoutingPanel.Invalidate(invRect);  //this is OK here - we dont' need tohide/show user controls - remove old line
			}
		}

		#endregion event handlers for Screen representations of CAN nodes
		#region Context menu event handlers for Screen representations of CAN nodes
		private void miAddUnconsumedPDO_Click(object sender, EventArgs e)
		{
			#region New PDO
			this.PDOUnderConstruction = new COBObject();
			PDOUnderConstruction.transmitNodes.Add(tempTxData);
			createUniqueNameAndCOIDForAddedPDO("new unconsumed PDO");
			this.addSPDOLB(this.currTxPDOnode, this.tempTxData.SPDOMaps, true, this.currCOB);
			this.sysInfo.COBsInSystem.Add(PDOUnderConstruction); //Note: all other properties - use COBObject defaults
			#region ask DI to update nodes
			currTxPDOnode.addTxNodeToPDO(PDOUnderConstruction);
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("Failed to add transmit node to " + PDOUnderConstruction.name);
			}
			#endregion ask DI to update nodes
			this.hideUserControls();
			this.updatePDODataBindings(this.sysInfo.COBsInSystem.IndexOf(PDOUnderConstruction)); //true becase COBsInSystem has changed => need to do full rebind
			this.deActivateInternalPDOs();
			this.activateCOB(PDOUnderConstruction);
			this.PDOUnderConstruction = null;  //ready for next time
			calculateScreenLinesForPDOCOBSInSystem(); //does full positioning of all sreen routes - Note:we have to make the offsets contiguous again
			drawAndFillMappings();
			this.layoutPDOMappedBitsPanel();
			this.layoutTxSysPDOExpansionPanels();
			this.showTxSPDOItemsForThisCANNode(this.currTxPDOnode); //update the treeview filter
			this.showUserControls();
			#endregion New PDO
		}

		private void miAddUnproducedPDO_Click(object sender, EventArgs e)
		{
			#region New PDO
			this.PDOUnderConstruction = new COBObject();
			PDOUnderConstruction.receiveNodes.Add(tempRxData);
			createUniqueNameAndCOIDForAddedPDO("new unproduced PDO");
			this.addSPDOLB(this.currRxPDOnode, this.tempRxData.SPDOMaps, false, this.currCOB);
			this.sysInfo.COBsInSystem.Add(PDOUnderConstruction); //Note: all other properties - use COBObject defaults
			#region ask DI to update nodes
			currRxPDOnode.addRxNodeToPDO(PDOUnderConstruction);
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("Failed to add receive CAN node to " + PDOUnderConstruction.name);
			}
			#endregion ask DI to update nodes
			this.hideUserControls();
			this.updatePDODataBindings(this.sysInfo.COBsInSystem.IndexOf(PDOUnderConstruction)); //true becase COBsInSystem has changed => need to do full rebind
			this.deActivateInternalPDOs();
			this.activateCOB(PDOUnderConstruction);
			this.PDOUnderConstruction = null;  //ready for next time
			calculateScreenLinesForPDOCOBSInSystem(); //does full positioning of all sreen routes - Note:we have to make the offsets contiguous again
			drawAndFillMappings();
			this.layoutPDOMappedBitsPanel();
			this.layoutRxSysPDOExpansionPanels();
			this.showRxSPDOItemsForThisCANNode(this.currRxPDOnode); //update the treeview filter
			this.showUserControls();
			#endregion New PDO
		}
		#endregion Context menu event handlers for Screen representations of CAN nodes
		#region activate/decativate specific COB, Screen CAN node  and Internal PDO
		private void deActivateInternalPDOs()
		{
			#region deselect any active internal PDO
			this.activeVPDOType = SevconObjectType.NONE;
			this.activeIntPDO = null;
			this.SysPDOsToHighLight.Clear();
			reshowSystemPDOTreeNodes();
			#endregion deselect any active internal PDO
		}
		private void activateCOB(COBObject selectedCOB)
        {
            #region before activating new cob, ensure any changed values are saved to old COB first
            // but not if the currCOB has been deleted
            if ((currCOB != null) && (currCOB != selectedCOB) && (sysInfo.COBsInSystem.IndexOf(currCOB) != -1))
            {
                //TODO add in cobid auto save - causes problems when new PDO added & bindings change
                //KeyEventArgs e = new KeyEventArgs(Keys.Return);
                //TB_SPDO_COBID_KeyDown(null, e);
                NUD_InhibitITme_Leave(null, (EventArgs)null);
                NUDEventTime_Leave(null, (EventArgs)null);
                NUD_TxNumSyncs_Leave(null, (EventArgs)null);
            }
            #endregion before activating new cob, ensure any changed values are saved to old COB first

            #region activate the System PDO
            this.currCOB = selectedCOB;

            if (this.CB_SPDOName.SelectedIndex != this.sysInfo.COBsInSystem.IndexOf(selectedCOB))//this.sysInfo.COBsInSystem.IndexOf(selectedCOB))
            {
                if (this.sysInfo.COBsInSystem.IndexOf(selectedCOB) >= CB_SPDOName.Items.Count)
                {
                    this.CB_SPDOName.SelectedIndex = CB_SPDOName.Items.Count - 1;
                }
                else
                {
                    this.CB_SPDOName.SelectedIndex = this.sysInfo.COBsInSystem.IndexOf(selectedCOB);//this.managerCB.Position;
                }
            }
            updateTxTimerDIsplay();

			this.TB_SPDO_COBID.Enabled = true;
			this.CB_SPDO_priority.Enabled = true;
			this.NUD_TxNumSyncs.Enabled = true;
			this.NUD_InhibitITme.Enabled = true;
			this.NUD_TxNumSyncs.Enabled = true;
			this.NUDEventTime.Enabled = true;
			this.RB_AsynchNormal.Enabled = true;
			this.RB_AsynchRTR.Enabled = true;
			this.RB_SyncAcyclic.Enabled = true;
			this.RB_SyncCyclic.Enabled = true;
			this.RB_SyncRTR.Enabled = true;
			CB_SPDO_priority.SelectedIndex = (int) this.currCOB.priority;  //the data source is an enureation not the COBsInSystem
			#region force controls to read only that user has insuffient access to
			#region chekc that all affected nodes are in pre-op
			foreach(COBObject.PDOMapData txData in currCOB.transmitNodes)
			{
				foreach(nodeInfo CANNode in this.PDOableCANNodes)
				{//some COb stuff allows changing in operational - it makes sense to force the user into good practic eof only chanigng these in pre-op - so that's what DriveWizar dis going to do
                    bool displayNodeInPreOp = false;
					if((txData.nodeID == CANNode.nodeID)&& (CANNode.isSevconApplication() == true))
					{//
						#region Sevcon apps can neither Tx or Rx RTR types - so disable these controls
						this.RB_AsynchRTR.Enabled = false;
						this.RB_SyncRTR.Enabled = false;
						#endregion Sevcon apps can neither Tx or Rx RTR types - so disable these controls
                        #region if display, read NMT state from OD as no hbs
                        if (((SCCorpStyle.ProductRange)CANNode.productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY) 
                            && (CANNode.nodeState == NodeState.Unknown))
                        {
                            DIFeedbackCode feedback = CANNode.readODValue(CANNode.nmtStateodSub);
                            displayNodeInPreOp = false;
                            if (feedback == DIFeedbackCode.DISuccess)
                            {
                                if ((NodeState)CANNode.nmtStateodSub.currentValue == NodeState.PreOperational)
                                {
                                    displayNodeInPreOp = true;
                                }
                            }
                        }
                        #endregion if display, read NMT state from OD as no hbs

                        if 
                        (
                            (CANNode.accessLevel < SCCorpStyle.AccLevel_SevconPDOWrite)
                          ||(((SCCorpStyle.ProductRange)CANNode.productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY) && (displayNodeInPreOp == false))
                          ||(((SCCorpStyle.ProductRange)CANNode.productRange != SCCorpStyle.ProductRange.SEVCONDISPLAY) && (CANNode.nodeState != NodeState.PreOperational))
						)
						{
							#region disable controls that require higher access level or pre-op
							this.TB_SPDO_COBID.Enabled = false;
							this.CB_SPDO_priority.Enabled = false;
							this.RB_AsynchNormal.Enabled = false;
							this.RB_SyncAcyclic.Enabled = false;
							this.RB_SyncCyclic.Enabled = false;
							//next itmes ar eonly applicable to the Tx end
							this.NUD_TxNumSyncs.Enabled = false;
							this.NUD_InhibitITme.Enabled = false;
							this.NUD_TxNumSyncs.Enabled = false;
							this.NUDEventTime.Enabled = false;
							#endregion disable controls that require higher accesslevle or pre-op
						}
						if(CANNode.accessLevel < SCCorpStyle.AccLevel_SevconPDOWrite)
						{
							this.statusBarPanel3.Text = "Insufficent access to edit PDOs on Sevcon nodes";
						}
						else if(CANNode.nodeState != NodeState.PreOperational) 
						{
                            this.statusBarPanel3.Text = "Sevcon nodes must be in pre-op to edit PDOs";
						}
						break; //found the corresponding node for this txData
					}
				}
				break;//first txData only - any more are error condition
			}
			foreach(COBObject.PDOMapData rxData in currCOB.receiveNodes)
			{
				foreach(nodeInfo CANNode in this.PDOableCANNodes)
				{
                    bool displayNodeInPreOp = false;
					if((rxData.nodeID == CANNode.nodeID)&& (CANNode.isSevconApplication() == true) )
					{
						#region Sevcon apps can neither Tx or Rx RTR types - so disable these controls
						this.RB_AsynchRTR.Enabled = false;
						this.RB_SyncRTR.Enabled = false;
						#endregion Sevcon apps can neither Tx or Rx RTR types - so disable these controls
                        #region if display, read NMT state from OD as no hbs
                        if (((SCCorpStyle.ProductRange)CANNode.productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY)
                            && (CANNode.nodeState == NodeState.Unknown))
                        {
                            DIFeedbackCode feedback = CANNode.readODValue(CANNode.nmtStateodSub);
                            displayNodeInPreOp = false;
                            if (feedback == DIFeedbackCode.DISuccess)
                            {
                                if ((NodeState)CANNode.nmtStateodSub.currentValue == NodeState.PreOperational)
                                {
                                    displayNodeInPreOp = true;
                                }
                            }
                        }
                        #endregion if display, read NMT state from OD as no hbs
                        if
                        (
                            (CANNode.accessLevel < SCCorpStyle.AccLevel_SevconPDOWrite)
                          || (((SCCorpStyle.ProductRange)CANNode.productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY) && (displayNodeInPreOp == false))
                          || (((SCCorpStyle.ProductRange)CANNode.productRange != SCCorpStyle.ProductRange.SEVCONDISPLAY) && (CANNode.nodeState != NodeState.PreOperational))
                        )
						{ //param changes that requre th eRx node to be in pre-op with sufficient access
							#region disable controls that require higher access level or pre-op
							this.TB_SPDO_COBID.Enabled = false;
							this.CB_SPDO_priority.Enabled = false;
							this.NUD_TxNumSyncs.Enabled = false;
							this.RB_AsynchNormal.Enabled = false;
							this.RB_SyncAcyclic.Enabled = false;
							this.RB_SyncCyclic.Enabled = false;
							#endregion disable controls that require higher access level or pre-op
						}
						break; //found the corresponding node so go to next rxData
					}
				}
			}
			#endregion chekc that all affected nodes are in pre-op
			#endregion force controls to read only that user has insuffient access to
			if(currCOB.messageType == COBIDType.PDO)
			{
				if(this.Pnl_COBMappings.Visible == false)
				{
					this.Pnl_COBMappings.Visible = true;
					Splitter_PDORouteToMappings.Visible = true;
				}
				this.drawAndFillMappings();
				layoutPDOMappedBitsPanel();
			}
			else
			{
				if(this.Pnl_COBMappings.Visible == true)
				{
					this.Pnl_COBMappings.Visible = false;
					Splitter_PDORouteToMappings.Visible = false;
				}
			}

			//when user selects new PDO we must first reset GroupBox forecolurs
			//to black the prevents data hangover when user eg clicks on an unproduced PDO route
			#region reset GroupBox forecolours
			foreach(ArrayList GBsForNode in this.txSysPDOExpansionPnls)
			{
				foreach(GroupBox gb in GBsForNode)
				{
					gb.ForeColor = Color.Black;
				}
			}
			foreach(ArrayList  GBsForNode in this.rxSysPDOExpansionPnls)
			{
				foreach(GroupBox gb in GBsForNode)
				{
					gb.ForeColor = Color.Black;
				}
			}
			#endregion reset GroupBox forecolours
			#region activate scrren node corresponding to firs tnode in currCOB.transmitNodes
			foreach(COBObject.PDOMapData txData in this.currCOB.transmitNodes)
			{
				foreach(nodeInfo node in this.PDOableCANNodes)
				{
					if(node.nodeID == txData.nodeID)
					{
						this.activateTxScreenNode(node);
						break;
					}
				}
				break; //first one only
			}
			#endregion activate scrren node corresponding to firs tnode in currCOB.transmitNodes
			#region activate scrren node corresponding to firs tnode in currCOB.receiveNodes
			foreach(COBObject.PDOMapData rxData in this.currCOB.receiveNodes)
			{
				foreach(nodeInfo node in this.PDOableCANNodes)
				{
					if(node.nodeID == rxData.nodeID)
					{
						this.activateRxScreenNode(node);
						break;
					}
				}
				break; //first one only
			}
			#endregion activate scrren node corresponding to firs tnode in currCOB.receiveNodes
			#endregion activate the System PDO
		}
		private void activateTxScreenNode(nodeInfo newcurrTxPDOnode)
		{
			#region remove the expansion panels form the screnn for the old currTxPDOnode
			ArrayList nodeGbs = (ArrayList) this.txSysPDOExpansionPnls[ this.PDOableCANNodes.IndexOf(currTxPDOnode)];
			foreach(GroupBox gb in nodeGbs)
			{
				this.PDORoutingPanel.Controls.Remove(gb);
			}
			#endregion remove the expansion panels form the screnn for the old currTxPDOnode
			this.currTxPDOnode = newcurrTxPDOnode;
			layoutTxPDOInterfacePanels(); 
			layoutTxSysPDOExpansionPanels();
			if(txSysPDOExpansionPnls.Count>0)
			{//needed - could be unproduced COB
				ArrayList GBsForCurrentTxNode = (ArrayList)this.txSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currTxPDOnode)];
				foreach(GroupBox gb in GBsForCurrentTxNode)
				{
					if(gb.Text == this.currCOB.name)
					{
						gb.ForeColor = Color.Red;
					}
					else
					{
						gb.ForeColor = Color.Black;
					}
				}
			}
		}
		private void activateRxScreenNode(nodeInfo newCurrRxPDOnode)
		{
			#region remove the expansion panels form the screen for the old currRxPDOnode
			foreach(GroupBox gb in (ArrayList) this.rxSysPDOExpansionPnls[ this.PDOableCANNodes.IndexOf(currRxPDOnode)])
			{
				this.PDORoutingPanel.Controls.Remove(gb);
			}
			#endregion remove the expansion panels form the screnn for the old currRxPDOnode
			//now we are safe to chang eot new screen node
			this.currRxPDOnode = newCurrRxPDOnode;
			layoutRxPDOInterfacePanels(); 
			layoutRxSysPDOExpansionPanels();
			if(rxSysPDOExpansionPnls.Count>0)
			{//needed - could be unproduced COB
				ArrayList GBsForCurrentRxNode = (ArrayList)this.rxSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currRxPDOnode)];
				foreach(GroupBox gb in GBsForCurrentRxNode)
				{
					if(((COBObject) gb.Tag) == this.currCOB)
					{
						gb.ForeColor = Color.Red;
					}
					else
					{
						gb.ForeColor = Color.Black;
					}
				}
			}
		}

		#endregion activate/decativate specific COB, Screen CAN node and Internal PDO
		#region COmmunications parameters Event handlers
		private void CB_SPDOName_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			this.hideUserControls();
			deActivateInternalPDOs();
			activateCOB((COBObject) this.sysInfo.COBsInSystem[this.CB_SPDOName.SelectedIndex]);
			this.showUserControls();
		}
		private void updateTxTimerDIsplay()
		{
			if(currCOB != null)
			{
				if(this.currCOB.messageType == COBIDType.PDO)
				{
					//visible for all PDO types
					this.GB_EventTIme.Text = "Event time";
					this.GB_COBTxTriggers.Visible = true;
					//switch these off for start 
					this.GB_SyncTypes.Visible = true;
					this.GB_AsynchTypes.Visible = true;
					this.GB_EventTIme.Visible = false;
					this.GB_InhibitTime.Visible = false;
					this.GB_SyncTimings.Visible = false;
					if(this.currCOB.AsynchNormal == true)
					{
						this.GB_EventTIme.Visible = true;
						this.GB_InhibitTime.Visible = true;
					}
					else if (this.currCOB.SyncAcylic == true)
					{
						this.GB_InhibitTime.Visible = true;
					}
					else if( this.currCOB.SyncCyclic == true)
					{
						this.GB_SyncTimings.Visible = true;
					}
				}
				else 
				{
					this.GB_SyncTimings.Visible = false;
					this.GB_InhibitTime.Visible = false;
					this.GB_EventTIme.Visible = false;
					this.GB_SyncTypes.Visible = false;
					this.GB_AsynchTypes.Visible = false;
					this.GB_COBTxTriggers.Visible = false;

					if( this.currCOB.messageType == COBIDType.EmergencyWithInhibit)
					{
						this.GB_COBTxTriggers.Visible = true;
						this.GB_InhibitTime.Visible = true;
					}
					else if(this.currCOB.messageType == COBIDType.ProducerHeartBeat)
					{
						this.GB_COBTxTriggers.Visible = true;
						this.GB_EventTIme.Visible = true;
						this.GB_EventTIme.Text = "Heartbeat time";
					}
				}
			}
		}
		private void CB_SPDOName_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(this.currCOB != null)
			{
				if( (((Keys)e.KeyValue) == Keys.Return) || (((Keys)e.KeyValue) == Keys.Enter) )
				{
					this.PDORoutingPanel.Resize -=new EventHandler(PDORoutingPanel_Resize);
					this.hideUserControls();
					currCOB.name = this.CB_SPDOName.Text;
					this.updatePDODataBindings(sysInfo.COBsInSystem.IndexOf(this.currCOB));
					this.layoutRxSysPDOExpansionPanels(); //force name update ion expansion panels
					this.layoutTxSysPDOExpansionPanels();//force name change on expansion panles
					this.showUserControls();
					this.PDORoutingPanel.Resize +=new EventHandler(PDORoutingPanel_Resize);
				}
				else if (((Keys)e.KeyValue) == Keys.Escape) 
				{
					this.CB_SPDOName.Text = currCOB.name;
				}
			}
		}
		private void TB_SPDO_COBID_KeyDown(object sender, KeyEventArgs e)
		{
			if(this.currCOB != null)
			{
				if( (((Keys)e.KeyValue) == Keys.Return) || (((Keys)e.KeyValue) == Keys.Enter) )
				{  //we cannot use binding parse because it does not get called when user hits return 
					try
					{
						if(this.TB_SPDO_COBID.Text.ToUpper().IndexOf("0X") == 0)
						{
							this.currCOB.requestedCOBID = System.Convert.ToInt32(this.TB_SPDO_COBID.Text, 16);
						}
						else
						{
							this.currCOB.requestedCOBID = System.Convert.ToInt32(this.TB_SPDO_COBID.Text);
						}
					}
					catch
					{
						SystemInfo.errorSB.Append("\nInvalid COBID value");
						this.currCOB.requestedCOBID = this.currCOB.COBID;
						this.updatePDODataBindings(sysInfo.COBsInSystem.IndexOf(this.currCOB));  //needed to force text box to rebind and thus clrear the text
						return;
					}
					foreach(COBObject COB in sysInfo.COBsInSystem)
					{
						if(COB == this.currCOB)
						{
							continue; //don't chekc it against itself
						}
						if(COB.COBID == this.currCOB.requestedCOBID)
						{
							//duplicate COBID - reject
							SystemInfo.errorSB.Append("\nDuplicate COBID rejected");
							this.currCOB.requestedCOBID = this.currCOB.COBID;
							this.updatePDODataBindings(sysInfo.COBsInSystem.IndexOf(this.currCOB));  //needed to force text box to rebind and thus clrear the text
						}
					}
					if(this.currCOB.requestedCOBID != this.currCOB.COBID)
					{
						this.sysInfo.changeCOBIDOnAllNodes(currCOB);
						this.updatePDODataBindings(sysInfo.COBsInSystem.IndexOf(this.currCOB));
					}
					if(SystemInfo.errorSB.Length>0)
					{
						sysInfo.displayErrorFeedbackToUser("Unable to change COBID of " + this.currCOB.name);
					}
				}
				else if (((Keys)e.KeyValue) == Keys.Escape) 
				{ //and we need ot handle the user hitting escape
					this.currCOB.requestedCOBID = this.currCOB.COBID;
					this.updatePDODataBindings(sysInfo.COBsInSystem.IndexOf(this.currCOB));  //needed to force text box to rebind and thus clrear the text
				}
			}
		}

		private void bind_Format(object sender, ConvertEventArgs e)
		{ //formated the COBID for display in text Box
			try
			{
				ushort temp =  System.Convert.ToUInt16(e.Value.ToString());
				e.Value = "0x" + (temp.ToString("X").PadLeft(4,'0'));
			}
			catch {}  //do nothing???
		}
		private void CB_SPDO_priority_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if(this.currCOB != null) 
			{
				if(this.CB_SPDO_priority.SelectedIndex >= this.CB_SPDO_priority.Items.Count-1)
				{
					this.CB_SPDO_priority.SelectedIndex = this.CB_SPDO_priority.Items.Count - 2;  //last 'real' priority
				}
				int [] unusedCOBIDs = new int[1]; //we only need one here
				this.sysInfo.getUnusedCOBIDs((COBIDPriority) this.CB_SPDO_priority.SelectedItem, unusedCOBIDs);
				this.currCOB.requestedCOBID = unusedCOBIDs[0];
				this.sysInfo.changeCOBIDOnAllNodes(currCOB); 
				this.updatePDODataBindings(this.CB_SPDOName.SelectedIndex);  //we hav echanged contents of COBsInSystem so we must do full rebind
				if(SystemInfo.errorSB.Length>0)
				{
					sysInfo.displayErrorFeedbackToUser("Unable to change priority on " + this.currCOB.name);
				}
			}
		}
		private void RB_Click(object sender, EventArgs e)
		{
			if(this.currCOB != null)
			{
				RadioButton rb = (RadioButton) sender;
				//don't use swithc here - the case has to be a constant 
				//- we dont want to hard code the control's name so use if else ladder for comparisond=s
				if (rb.Name == this.RB_SyncCyclic.Name)
				{
					this.currCOB.TxType = this.currCOB.SyncTime;
				}
				else if(rb.Name == this.RB_SyncAcyclic.Name)
				{
					this.currCOB.TxType = 0;
				}
				else if (rb.Name == this.RB_AsynchNormal.Name)
				{
					this.currCOB.TxType = 255; //could use 254 but espAC uses 255 - should make no difeerence to any CANopen compliant device
				}
				else if( rb.Name == this.RB_AsynchRTR.Name) //not used by espAC but valid for CANopen devices
				{
					this.currCOB.TxType = 253;
				}
				else if ( rb.Name == this.RB_SyncRTR.Name) //not used by espAC but valid for CANopen devices
				{
					this.currCOB.TxType = 252;
				}
				this.sysInfo.changeTxTypeOnAllNodes(currCOB);
				this.updatePDODataBindings(sysInfo.COBsInSystem.IndexOf(this.currCOB)); //this MUST be set true if we change currCOB - forces the bindings to 'refresh' with then ew data
				if(SystemInfo.errorSB.Length>0)
				{
					sysInfo.displayErrorFeedbackToUser("Unable to change transmission type on " + this.currCOB.name);
				}
			}
		}
		private void NUD_InhibitITme_KeyDown(object sender, KeyEventArgs e)
		{ //DO NOT use valueChanged event - it fires with every increment of the up/Down arrow or when th eCurrCOB is changed
			if(this.currCOB != null)
			{
				if( (((Keys)e.KeyValue) == Keys.Return) || (((Keys)e.KeyValue) == Keys.Enter) )
				{  //we cannot use binding parse because it does not get called when user hits return 
					currCOB.inhibitTime = (int) this.NUD_InhibitITme.Value;
					foreach(COBObject.PDOMapData txData in this.currCOB.transmitNodes)
					{
						int CANNodeIndex;
						this.sysInfo.getNodeNumber(txData.nodeID, out CANNodeIndex);
						this.sysInfo.nodes[CANNodeIndex].ChangeInhibitTime(this.currCOB);
						if(SystemInfo.errorSB.Length>0)
						{
							sysInfo.displayErrorFeedbackToUser("Failed to change inhibit time on " + this.currCOB.name);
						}

					}
				}
				else if (((Keys)e.KeyValue) == Keys.Escape) 
				{
					this.NUD_InhibitITme.Value = this.currCOB.inhibitTime;
				}
			}
		}
		private void NUD_InhibitITme_Leave(object sender, EventArgs e)
		{
			if(this.NUD_InhibitITme.Value != this.currCOB.inhibitTime )
			{//we have changed 
				currCOB.inhibitTime = (int) this.NUD_InhibitITme.Value;
				foreach(COBObject.PDOMapData txData in this.currCOB.transmitNodes)
				{
					int CANNodeIndex;
					this.sysInfo.getNodeNumber(txData.nodeID, out CANNodeIndex);
					this.sysInfo.nodes[CANNodeIndex].ChangeInhibitTime(this.currCOB);
					if(SystemInfo.errorSB.Length>0)
					{
						sysInfo.displayErrorFeedbackToUser("Failed to change inhibit time on " + this.currCOB.name);
					}
				}
			}
		}

		private void NUDEventTime_KeyDown(object sender, KeyEventArgs e)
		{
			if(this.currCOB != null)
			{
				if( (((Keys)e.KeyValue) == Keys.Return) || (((Keys)e.KeyValue) == Keys.Enter) )
				{  //we cannot use binding parse because it does not get called when user hits return 
					this.currCOB.eventTime = (int) this.NUDEventTime.Value;
					foreach(COBObject.PDOMapData txData in this.currCOB.transmitNodes)
					{
						int CANNodeIndex;
						this.sysInfo.getNodeNumber(txData.nodeID, out CANNodeIndex);
						this.sysInfo.nodes[CANNodeIndex].ChangeEventTime(this.currCOB);
						if(SystemInfo.errorSB.Length>0)
						{
							sysInfo.displayErrorFeedbackToUser("Failed to change event time for " + this.currCOB.name);
						}
					}
				}
				else if (((Keys)e.KeyValue) == Keys.Escape) 
				{
					this.NUDEventTime.Value =  this.currCOB.eventTime;
				}
			}
		}

		private void NUDEventTime_Leave(object sender, EventArgs e)
		{
			if(this.currCOB.eventTime !=  this.NUDEventTime.Value)
			{
				this.currCOB.eventTime = (int) this.NUDEventTime.Value;
				foreach(COBObject.PDOMapData txData in this.currCOB.transmitNodes)
				{
					int CANNodeIndex;
					this.sysInfo.getNodeNumber(txData.nodeID, out CANNodeIndex);
					this.sysInfo.nodes[CANNodeIndex].ChangeEventTime(this.currCOB);
					if(SystemInfo.errorSB.Length>0)
					{
						sysInfo.displayErrorFeedbackToUser("Failed to change event time for " + this.currCOB.name);
					}
				}
			}
		}

		private void NUD_TxNumSyncs_KeyDown(object sender, KeyEventArgs e)
		{
			if(this.currCOB != null)
			{
				if( (((Keys)e.KeyValue) == Keys.Return) || (((Keys)e.KeyValue) == Keys.Enter) )
				{  //we cannot use binding parse because it does not get called when user hits return 
					this.currCOB.SyncTime =  (int) this.NUD_TxNumSyncs.Value;
					this.sysInfo.changeTxTypeOnAllNodes(this.currCOB);
					if(SystemInfo.errorSB.Length>0)
					{
						sysInfo.displayErrorFeedbackToUser("Unable to change Sync time on " + this.currCOB.name);
					}
				}
				else if (((Keys)e.KeyValue) == Keys.Escape) 
				{
					this.NUD_TxNumSyncs.Value =  this.currCOB.SyncTime;
				}
			}
		}

		private void NUD_TxNumSyncs_Leave(object sender, EventArgs e)
		{
			if(this.currCOB.SyncTime !=  this.NUD_TxNumSyncs.Value)
			{
				this.currCOB.SyncTime = (int) this.NUD_TxNumSyncs.Value;
				this.sysInfo.changeTxTypeOnAllNodes(this.currCOB);
				if(SystemInfo.errorSB.Length>0)
				{
					sysInfo.displayErrorFeedbackToUser("Unable to change Sync time on " + this.currCOB.name);
				}
			}
		}

		#endregion COmmunications parameters Event handlers
		#region SPDO Mappings Panel Event Hanlders
		private void drawAndFillMappings()
		{
			this.Pnl_COBMappings.Controls.Clear();
			if(this.currCOB.messageType == COBIDType.PDO)
			{
				Label title = new Label();
				title.Text = "CAN frame Bit usage for: " + this.currCOB.name;
				title.Font = new Font("Arial", 10);
				title.AutoSize = true;
				this.Pnl_COBMappings.Controls.Add(title);
				foreach(COBObject.PDOMapData txData in this.currCOB.transmitNodes)
				{
					#region Tx label and mapping panel
					Label pnlLabel = new Label();
					pnlLabel.Text = "Transmit mappings for CAN node Id " + txData.nodeID.ToString();
					pnlLabel.AutoSize = true;
					sixtyFourBitsAsPanel myPanel = new sixtyFourBitsAsPanel();
					myPanel.DragOver +=new DragEventHandler(SysPDOMappingPanel_DragOver);
					myPanel.DragDrop +=new DragEventHandler(SysPDOMappingPanel_DragDrop); 
					//do a manual drag - Panel does not implement ItemDrag
					myPanel.MouseDown +=new MouseEventHandler(SysPDOMappingPanel_MouseDown);
					myPanel.MouseMove +=new MouseEventHandler(SysPDOMappingPanel_MouseMove);
					myPanel.MouseEnter +=new EventHandler(SysPDOMappingPanel_MouseEnter);
					myPanel.isTx = true;
					this.Pnl_COBMappings.Controls.Add(pnlLabel);
					this.Pnl_COBMappings.Controls.Add(myPanel);

					#region filling existing data
					foreach(PDOMapping map in txData.SPDOMaps)
					{
						myPanel.addMapping(map);
					}
					#endregion filling existing data

					break;  //only do first Tx node
					#endregion Tx label and mapping panel
				}
				foreach(COBObject.PDOMapData rxData in this.currCOB.receiveNodes)
				{
					#region Rx labels and mapping panels
					Label pnlLabel = new Label();
					pnlLabel.Text = "Receive mappings for CAN node Id " + rxData.nodeID.ToString();
					pnlLabel.AutoSize = true;
					sixtyFourBitsAsPanel myPanel = new sixtyFourBitsAsPanel();
					myPanel.pnlIndex = this.currCOB.receiveNodes.IndexOf(rxData);
					myPanel.DragOver +=new DragEventHandler(SysPDOMappingPanel_DragOver);
					myPanel.DragDrop +=new DragEventHandler(SysPDOMappingPanel_DragDrop); 
					myPanel.MouseMove +=new MouseEventHandler(SysPDOMappingPanel_MouseMove);
					myPanel.MouseDown +=new MouseEventHandler(SysPDOMappingPanel_MouseDown);
					myPanel.MouseEnter +=new EventHandler(SysPDOMappingPanel_MouseEnter);
					#region filling existing data
					foreach(PDOMapping map in rxData.SPDOMaps)
					{
						myPanel.addMapping(map);
					}
					#endregion filling existing data
					this.Pnl_COBMappings.Controls.Add(pnlLabel);
					this.Pnl_COBMappings.Controls.Add(myPanel);

					#endregion Rx labels and mapping panels
				}
			}
		}

		private void layoutPDOMappedBitsPanel()
		{
			int currOffset = 0;
			foreach(Control cont in this.Pnl_COBMappings.Controls)
			{
				if(this.Pnl_COBMappings.Controls.IndexOf(cont) == 0) //title label
				{
					currOffset = cont.Height;
					continue;
				}
				if(cont is Label)
				{
					currOffset += this.PDOVertSpacer;
				}	
				else
				{
					cont.Width = this.Pnl_COBMappings.Width - 20; //10 pixels Right an dleft anchor spacing
				}
				cont.Left = 10;
				cont.Top = currOffset;
				currOffset += cont.Height;
			}
		}
		/// <summary>
		/// USer clicked on COB mapping sscreen panle but NOTon a specific mapping panel - so show all the PDO anble Treendoes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Pnl_COBMappings_MouseDown(object sender, MouseEventArgs e)
		{
			this.SystemStatusTN.Nodes.Clear();
			foreach(TreeNode tn in this.PDOableTreeNodes)
			{
				this.SystemStatusTN.Nodes.Add(tn);
			}
		}

		private void SysPDOMappingPanel_DragOver(object sender, DragEventArgs e)
		{
            DriveWizard.sixtyFourBitsAsPanel mapPnl = (DriveWizard.sixtyFourBitsAsPanel)sender;
            #region get info from dragged node
            TreeNode draggedTNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode", true);
            treeNodeTag dragTNTag = (treeNodeTag)draggedTNode.Tag;
            e.Effect = DragDropEffects.None;
            #endregion get info from dragged node
            if (this.currCOB.transmitNodes.Count > 1)
            {
                this.statusBarPanel3.Text = "PDO has multiple transmit nodes, correct this before editing";
                return;
            }
            #region pre-filled PDOs
            if ((dragTNTag.tagType == TNTagType.PreSetPDO_TractionLeft)
                || (dragTNTag.tagType == TNTagType.PreSetPDO_TractionRight)
                || (dragTNTag.tagType == TNTagType.PreSetPDO_Pump)
                || (dragTNTag.tagType == TNTagType.PreSetPDO_PowerSteer))
            {
                if (mapPnl.isTx == true)
                {
                    #region Predefined Tx mapping sets
                    COBObject.PDOMapData txData = (COBObject.PDOMapData)currCOB.transmitNodes[0];
                    if (txData.nodeID == dragTNTag.nodeID)
                    {
                        #region check for a free RXPDO slot for the feedbackPDO
                        if (this.currCOB.transmitNodes.Count > 1)
                        {
                            this.statusBarPanel3.Text = "Multiple Tranmit nodes for this PDO";
                            return;
                        }
                        foreach (nodeInfo CANnode in this.PDOableCANNodes)
                        {
                            if (CANnode.nodeID == txData.nodeID)
                            {
                                if (CANnode.disabledRxPDOIndexes.Count < 1)
                                {
                                    this.statusBarPanel3.Text = "Insufficient free receive PDOs on this node";
                                    return;
                                }
                                break;
                            }
                        }
                        #endregion check for a free RXPDO slot for the feedbackPDO
                        e.Effect = DragDropEffects.Copy;
                        this.statusBarPanel3.Text = "";
                    }
                    else
                    {
                        this.statusBarPanel3.Text = "Dragged item node Id (" + dragTNTag.nodeID.ToString() + "), Tx mapping node ID (" + txData.nodeID.ToString() + ")";
                    }
                    #endregion Predefined Tx mappings
                }
                else
                {
                    #region pre-defined Rx mapping sets
                    foreach (COBObject.PDOMapData rxData in this.currCOB.receiveNodes)
                    {
                        if (mapPnl.pnlIndex == this.currCOB.receiveNodes.IndexOf(rxData))
                        {  //hook up this correct RxNode data to this screen panel
                            if (rxData.nodeID == dragTNTag.nodeID)
                            {
                                #region check for multiple receive nodes
                                if (this.currCOB.receiveNodes.Count > 1)
                                {
                                    this.statusBarPanel3.Text = "Pre-filled PDOs cannot be used with multiple receive nodes";
                                    return;
                                }
                                #endregion check for multiple receive nodes
                                #region check for a free transmit PDO slot on this node
                                foreach (nodeInfo CANnode in this.PDOableCANNodes)
                                {
                                    if (CANnode.nodeID == rxData.nodeID)
                                    {
                                        if (CANnode.disabledTxPDOIndexes.Count < 1)
                                        {
                                            this.statusBarPanel3.Text = "Insufficient free transmit PDOs on this node";
                                            return;
                                        }
                                        break;
                                    }
                                }
                                #endregion check for a free transmit PDO slot on this node
                                e.Effect = DragDropEffects.Copy;
                                this.statusBarPanel3.Text = "";
                            }
                            else
                            {//error condition
                                this.statusBarPanel3.Text = "Dragged item node Id (" + dragTNTag.nodeID.ToString() + "), Rx mapping node ID (" + rxData.nodeID.ToString() + ")";
                                return;
                            }
                        }
                    }
                    #endregion pre-defined Rx mapping sets
                }
                return;
            }
            #endregion pre-filled PDOs
            if (dragTNTag.assocSub != null) //b&b
            {
                e.Effect = DragDropEffects.All;
                this.statusBarPanel3.Text = "";
                #region determine the mapValue that we are trying to drop into a mapping
                long mapVal = this.convertODItemToMappingValue(dragTNTag.assocSub);
                #endregion determine the mapValue that we are trying to drop into a mapping
                #region limits applicable to both Tx and Rx Panels
                if (this.sysInfo.nodes[dragTNTag.tableindex].isSevconApplication() == true)
                {
                    if (this.sysInfo.nodes[dragTNTag.tableindex].accessLevel < SCCorpStyle.AccLevel_SevconPDOWrite)
                    {
                        this.statusBarPanel3.Text = "Insufficient access level to edit PDOs on Sevcon node ID " + sysInfo.nodes[dragTNTag.tableindex].nodeID.ToString();
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                    if (((SCCorpStyle.ProductRange)this.sysInfo.nodes[dragTNTag.tableindex].productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY)
                        && (sysInfo.nodes[dragTNTag.tableindex].nodeState == NodeState.Unknown))
                    {
                        #region display special handling (read NMT state from OD as no hbs)
                        DIFeedbackCode feedback = sysInfo.nodes[dragTNTag.tableindex].readODValue(sysInfo.nodes[dragTNTag.tableindex].nmtStateodSub);
                        if (feedback == DIFeedbackCode.DISuccess)
                        {
                            if ((NodeState)sysInfo.nodes[dragTNTag.tableindex].nmtStateodSub.currentValue == NodeState.Unknown)
                            {
                                e.Effect = DragDropEffects.None;
                                this.statusBarPanel3.Text = "Sevcon display NMT state unknown.";
                                return;
                            }
                            else if ((NodeState)sysInfo.nodes[dragTNTag.tableindex].nmtStateodSub.currentValue != NodeState.PreOperational)
                            {
                                this.statusBarPanel3.Text = "Sevcon nodes must be in pre-op to edit PDOs";
                                e.Effect = DragDropEffects.None;
                                return;
                            }
                        }
                        #endregion display special handling (read NMT state from OD as no hbs)
                    }
                    else if ((this.sysInfo.nodes[dragTNTag.tableindex].nodeState != NodeState.PreOperational)
                        && ((SCCorpStyle.ProductRange)this.sysInfo.nodes[dragTNTag.tableindex].productRange != SCCorpStyle.ProductRange.SEVCONDISPLAY))
                    {
                        this.statusBarPanel3.Text = "Sevcon nodes must be in pre-op to edit PDOs";
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                }
                #endregion limits applicable to both Tx and Rx Panels
                if (mapPnl.isTx == true)
                { //check if this OD item can be placed in a transmit mapping
                    #region Tx mapping panel
                    #region check for exceeding 64 bits
                    if (mapPnl.fillParams.Count > 0)
                    {
                        sixtyFourBitsAsPanel.fillParam fp = (sixtyFourBitsAsPanel.fillParam)mapPnl.fillParams[mapPnl.fillParams.Count - 1];
                        if ((fp.startBit + fp.numBits + dragTNTag.assocSub.dataSizeInBits) > 64)
                        {	//error condition
                            this.statusBarPanel3.Text = draggedTNode.Text
                                + " uses "
                                + dragTNTag.assocSub.dataSizeInBits.ToString()
                                + " bits. This mapping has "
                                + ((int)(64 - (fp.startBit + fp.numBits))).ToString()
                                + " spare bits";
                            e.Effect = DragDropEffects.None;
                            return;
                        }
                    }
                    #endregion check for exceeding 64 bits
                    #region check if this item is already in this map
                    foreach (COBObject.PDOMapData txData in this.currCOB.transmitNodes)
                    {  //use foreach => there could be nothing in trnsmitnodes - so casting first element is not on
                        if (txData.nodeID != dragTNTag.nodeID)
                        {//error condition
                            this.statusBarPanel3.Text = "Dragged item node Id (" + dragTNTag.nodeID.ToString() + "), Tx mapping node ID (" + txData.nodeID.ToString() + ")";
                            e.Effect = DragDropEffects.None;
                            return;
                        }
                        foreach (PDOMapping map in txData.SPDOMaps)
                        {
                            if (map.mapValue == mapVal)
                            {
                                this.statusBarPanel3.Text = "This mapping already containes " + draggedTNode.Text;
                                e.Effect = DragDropEffects.None;
                                return;
                            }
                        }
                        break;  //get out after first txData - ie we only look at first transmit node
                    }
                    #endregion check if this item is already in this map
                    #region check for non-readalbe object - cannot be mapped into a Tx mapping
                    //judetmep - requiremnt has been relaxed - leave code commented out for now - remove when we have concensus
                    //					if( (dragTNTag.assocSub.accessType != ObjectAccessType.ReadReadWrite)
                    //						&& (dragTNTag.assocSub.accessType != ObjectAccessType.ReadReadWriteInPreOp)
                    //						&& (dragTNTag.assocSub.accessType != ObjectAccessType.ReadWrite)
                    //						&& (dragTNTag.assocSub.accessType != ObjectAccessType.ReadWriteInPreOp)
                    //						&& (dragTNTag.assocSub.accessType != ObjectAccessType.Constant)
                    //						&& (dragTNTag.assocSub.accessType != ObjectAccessType.ReadOnly) )
                    //					{
                    //						this.statusBarPanel3.Text = draggedTNode.Text + " cannot be placed in Transmit mapping";
                    //						e.Effect = DragDropEffects.None;
                    //						return;
                    //					}
                    #endregion check for non-readalbe object - cannot be mapped into a Tx mapping
                    #region check for twice Tx mapped on Sevcon application
                    if (this.sysInfo.nodes[dragTNTag.tableindex].isSevconApplication() == true)
                    {
                        #region check for exceeding 8 mappings
                        foreach (COBObject.PDOMapData txData in this.currCOB.transmitNodes)
                        {  //use foreach => there could be nothing in trnsmitnodes - so casting first element is not on
                            if (txData.SPDOMaps.Count > 8)
                            {
                                this.statusBarPanel3.Text = "Maximum of 8 mappings (including spacers) allowed";
                                e.Effect = DragDropEffects.None;
                                return;
                            }
                            break;  //get out after first txData - ie we only look at first transmit node
                        }
                        #endregion check for exceeding 8 mappings
                        //twice mapped is an IXXAT library problem and so this contraint is only applied to Sevcon applications
                        int numTimesAlreadyMmapped = 0;
                        foreach (COBObject COB in this.sysInfo.COBsInSystem)
                        {
                            if (COB.messageType == COBIDType.PDO)
                            {
                                foreach (COBObject.PDOMapData txData in COB.transmitNodes)
                                {
                                    foreach (PDOMapping map in txData.SPDOMaps)
                                    {
                                        if ((map.mapValue == mapVal) && (++numTimesAlreadyMmapped >= 2))
                                        {
                                            this.statusBarPanel3.Text = draggedTNode.Text + " is already transmitted twice from this CAN node";
                                            e.Effect = DragDropEffects.None;
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion check for twice Tx mapped on Sevcon application
                    #endregion Tx mapping panel
                }
                else  //Rx panel
                {
                    #region Rx mapping panel
                    #region check if this item is already in this map
                    foreach (COBObject.PDOMapData rxData in this.currCOB.receiveNodes)
                    {
                        if (mapPnl.pnlIndex == this.currCOB.receiveNodes.IndexOf(rxData))
                        {  //hook up this correct RxNode data to this screen panel
                            if (rxData.nodeID != dragTNTag.nodeID)
                            {//error condition
                                this.statusBarPanel3.Text = "Dragged item node Id (" + dragTNTag.nodeID.ToString() + "), Rx mapping node ID (" + rxData.nodeID.ToString() + ")";
                                e.Effect = DragDropEffects.None;
                                return;

                            }
                            foreach (PDOMapping myMap in rxData.SPDOMaps)
                            {
                                if (myMap.mapValue == mapVal)
                                {
                                    this.statusBarPanel3.Text = "This mapping already contains " + draggedTNode.Text;
                                    e.Effect = DragDropEffects.None;
                                    return;
                                }
                            }
                            break; //found it - get out 
                        }
                    }
                    foreach (COBObject COB in this.sysInfo.COBsInSystem)
                    {
                        foreach (COBObject.PDOMapData rxData in COB.receiveNodes)
                        {
                            if ((rxData.nodeID == this.SevconMasterNode.nodeID)
                                && ((dragTNTag.assocSub.objectName == (int)SevconObjectType.DIGITAL_SIGNAL_IN)
                                || (dragTNTag.assocSub.objectName == (int)SevconObjectType.ANALOGUE_SIGNAL_IN)
                                || (dragTNTag.assocSub.objectName == (int)SevconObjectType.MOTOR_DRIVE)))
                            {
                                #region check for itmes that must have asingle PDO orVPDO source
                                //create the map that this would create
                                #region check the other SPDOs being received by the Sevocn master node
                                foreach (PDOMapping map in rxData.SPDOMaps)
                                {
                                    if (mapVal == map.mapValue)
                                    {
                                        this.statusBarPanel3.Text = dragTNTag.assocSub.parameterName + " must have single SPDO or VPDO source";
                                        e.Effect = DragDropEffects.None;
                                        return;
                                    }
                                }
                                #endregion check the other SPDOs being received by the Sevocn master node
                                #region check VPDOs
                                if (dragTNTag.assocSub.objectName == (int)SevconObjectType.DIGITAL_SIGNAL_IN)
                                {
                                    foreach (PDOMapping map in this.SevconMasterNode.intPDOMaps.digIPMaps)
                                    {
                                        #region check if we have reached the disabled ones
                                        int mapIndex = this.SevconMasterNode.intPDOMaps.digIPMaps.IndexOf(map);
                                        if (mapIndex >= SevconMasterNode.intPDOMaps.numEnabledDigIPMaps)
                                        {
                                            break; //only check the enabled ones 
                                        }
                                        #endregion check if we have reached the disabled ones
                                        int equivIntMap = (int)(mapVal >> 16);
                                        if (equivIntMap == map.mapValue)
                                        {
                                            this.statusBarPanel3.Text = dragTNTag.assocSub.parameterName + " must have single SPDO or VPDO source";
                                            e.Effect = DragDropEffects.None;
                                            return;
                                        }
                                    }
                                }
                                if (dragTNTag.assocSub.objectName == (int)SevconObjectType.ANALOGUE_SIGNAL_IN)
                                {
                                    foreach (PDOMapping map in this.SevconMasterNode.intPDOMaps.algIPMaps)
                                    {
                                        #region check if we have reached the disabled ones
                                        int mapIndex = this.SevconMasterNode.intPDOMaps.algIPMaps.IndexOf(map);
                                        if (mapIndex >= SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps)
                                        {
                                            break; //only check the enabled ones 
                                        }
                                        #endregion check if we have reached the disabled ones
                                        int equivIntMap = (int)(mapVal >> 16);
                                        if (equivIntMap == map.mapValue)
                                        {
                                            this.statusBarPanel3.Text = dragTNTag.assocSub.parameterName + " must have single SPDO or VPDO source";
                                            e.Effect = DragDropEffects.None;
                                            return;
                                        }
                                    }
                                }
                                if (dragTNTag.assocSub.objectName == (int)SevconObjectType.MOTOR_DRIVE)
                                {
                                    foreach (PDOMapping map in this.SevconMasterNode.intPDOMaps.MotorMaps)
                                    {
                                        #region check if we have reached the disabled ones
                                        int mapIndex = this.SevconMasterNode.intPDOMaps.MotorMaps.IndexOf(map);
                                        if (mapIndex >= SevconMasterNode.intPDOMaps.numEnabledMotorMaps)
                                        {
                                            break; //only check the enabled ones 
                                        }
                                        #endregion check if we have reached the disabled ones
                                        int equivIntMap = (int)(mapVal >> 16);
                                        if (equivIntMap == map.mapValue)
                                        {
                                            this.statusBarPanel3.Text = dragTNTag.assocSub.parameterName + " must have single SPDO or VPDO source";
                                            e.Effect = DragDropEffects.None;
                                            return;
                                        }
                                    }
                                }
                                #endregion check VPDOs
                                #endregion check for itmes that must have asingle PDO orVPDO source
                            }
                        }
                    }
                    #endregion check if this item is already in this map
                    #region check for non-writeable items - cannot be placed in a receive mapping
                    if ((dragTNTag.assocSub.accessType != ObjectAccessType.ReadWriteWrite)
                        && (dragTNTag.assocSub.accessType != ObjectAccessType.ReadWriteWriteInPreOp)
                        && (dragTNTag.assocSub.accessType != ObjectAccessType.ReadWrite)
                        && (dragTNTag.assocSub.accessType != ObjectAccessType.ReadWriteInPreOp)
                        && (dragTNTag.assocSub.accessType != ObjectAccessType.WriteOnly)
                        && (dragTNTag.assocSub.accessType != ObjectAccessType.WriteOnlyInPreOp))
                    {
                        this.statusBarPanel3.Text = draggedTNode.Text + " cannot be placed in Receive mapping";
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                    #endregion check for non-writeable items - cannot be placed in a receive mapping
                    #region check for exceeding 8 mappings  on Sevcon unit
                    if (this.sysInfo.nodes[dragTNTag.tableindex].isSevconApplication() == true)
                    {
                        foreach (COBObject.PDOMapData rxData in this.currCOB.receiveNodes)
                        {
                            if (mapPnl.pnlIndex == this.currCOB.receiveNodes.IndexOf(rxData))
                            {  //hook up this correct RxNode data to this screen panel
                                if (rxData.SPDOMaps.Count > 8)
                                {
                                    this.statusBarPanel3.Text = "Maximum of 8 mappings (including spacers) allowed";
                                    e.Effect = DragDropEffects.None;
                                    return;
                                }
                                break; //found it - get out 
                            }
                        }
                    }
                    #endregion check for exceeding 8 mappings
                    #region check whether the required spacer will take us ove rthe limt
                    Point myClientPpoint = new Point(0, 0);  //may to do in mouse over 
                    myClientPpoint = mapPnl.PointToClient(new Point(e.X, e.Y));
                    int startBit = myClientPpoint.X / (mapPnl.bitWidth);
                    #region determine how many spacers would be required to drop here
                    int bitsUsedInPanel = 0;
                    int numSpacersNeeded = 0;
                    foreach (sixtyFourBitsAsPanel.fillParam fp in mapPnl.fillParams)
                    {
                        bitsUsedInPanel += fp.numBits;
                    }
                    int bitsUsedByDummySpacers = 0;
                    if (startBit > bitsUsedInPanel + 1)
                    {
                        startBit -= bitsUsedInPanel;//we just want the 'gap' between mappings end and where user wants to start the next one
                        if (startBit / 32 > 0)
                        {
                            numSpacersNeeded++;
                            bitsUsedByDummySpacers += 32;
                            startBit -= 32;
                        }
                        if (startBit / 16 > 0)
                        {
                            numSpacersNeeded++;
                            bitsUsedByDummySpacers += 16;
                            startBit -= 16;
                        }
                        if (startBit / 8 > 0)
                        {
                            numSpacersNeeded++;
                            bitsUsedByDummySpacers += 8;
                            startBit -= 8;
                        }
                        //now add any 1 bits dummys that are required
                        for (int i = 0; i < startBit; i++)
                        {
                            numSpacersNeeded++;
                            bitsUsedByDummySpacers += 1;
                        }
                    }
                    #endregion determine how many spacers would be required to drop here
                    foreach (COBObject.PDOMapData rxData in this.currCOB.receiveNodes)
                    {
                        if (mapPnl.pnlIndex == this.currCOB.receiveNodes.IndexOf(rxData))
                        {  //hook up this correct RxNode data to this screen panel
                            if (rxData.SPDOMaps.Count + numSpacersNeeded >= 8)
                            {
                                this.statusBarPanel3.Text = "Maximum of 8 mappings (including spacers) allowed (num spacers = " + numSpacersNeeded.ToString() + ")";
                                e.Effect = DragDropEffects.None;
                                return;
                            }
                            else if (mapPnl.fillParams.Count > 0)
                            {
                                sixtyFourBitsAsPanel.fillParam fp = (sixtyFourBitsAsPanel.fillParam)mapPnl.fillParams[mapPnl.fillParams.Count - 1];
                                if ((fp.startBit
                                    + fp.numBits
                                    + dragTNTag.assocSub.dataSizeInBits
                                    + bitsUsedByDummySpacers) > 64)
                                {	//error condition
                                    if (bitsUsedByDummySpacers > 0)
                                    {
                                        this.statusBarPanel3.Text = draggedTNode.Text
                                            + " uses "
                                            + dragTNTag.assocSub.dataSizeInBits.ToString()
                                            + " bits plus "
                                            + bitsUsedByDummySpacers.ToString()
                                            + " bits in spacers. This mapping has "
                                            + ((int)(64 - (fp.startBit + fp.numBits))).ToString()
                                            + " spare bits";
                                        e.Effect = DragDropEffects.None;
                                        return;
                                    }
                                    else
                                    {
                                        this.statusBarPanel3.Text = draggedTNode.Text
                                            + " uses "
                                            + dragTNTag.assocSub.dataSizeInBits.ToString()
                                            + " bits. This mapping has "
                                            + ((int)(64 - (fp.startBit + fp.numBits))).ToString()
                                            + " spare bits";
                                        e.Effect = DragDropEffects.None;
                                        return;
                                    }
                                }

                            }

                            break; //found it - get out 
                        }
                    }

                    #endregion check whether the required spacer will take us ove rthe limt
                    #endregion Rx mapping panel
                }
            }
        }
		private void SysPDOMappingPanel_DragDrop(object sender, DragEventArgs e)
		{
            DriveWizard.sixtyFourBitsAsPanel mappingPnl = (DriveWizard.sixtyFourBitsAsPanel)sender;
            #region get data from dragged treenode
            TreeNode draggedTNode;
            treeNodeTag dragTNTag;
            draggedTNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode", true);
            dragTNTag = (treeNodeTag)draggedTNode.Tag;
            #endregion get data from dragged treenode
            COBObject.PDOMapData currCOBrxData = null; //null needed
            COBObject.PDOMapData currCOBtxData = null; //null needed
            this.hideUserControls();

            if ((dragTNTag.tagType == TNTagType.PreSetPDO_TractionLeft)
                || (dragTNTag.tagType == TNTagType.PreSetPDO_TractionRight)
                || (dragTNTag.tagType == TNTagType.PreSetPDO_Pump)
                || (dragTNTag.tagType == TNTagType.PreSetPDO_PowerSteer))
            {
                #region pre-filled pdo
                string currCOBName = "";
                string newCOBName = "";
                #region clear out maps for currCOB
                if (currCOB.transmitNodes.Count == 1)
                {
                    currCOBtxData = (COBObject.PDOMapData)this.currCOB.transmitNodes[0];
                    currCOBtxData.SPDOMaps.Clear();//first clear out any existing tx mappings
                }
                if (currCOB.receiveNodes.Count == 1)
                {
                    currCOBrxData = (COBObject.PDOMapData)this.currCOB.receiveNodes[0];
                    currCOBrxData.SPDOMaps.Clear();
                }
                #endregion clear out maps for currCOB
                #region empty mapping pnl data
                DriveWizard.sixtyFourBitsAsPanel txMapPnl = null;
                DriveWizard.sixtyFourBitsAsPanel rxMapPnl = null;
                foreach (Control cont in this.Pnl_COBMappings.Controls)
                {
                    if (cont is DriveWizard.sixtyFourBitsAsPanel)
                    {
                        DriveWizard.sixtyFourBitsAsPanel mapPnl = (DriveWizard.sixtyFourBitsAsPanel)cont;
                        if (mapPnl.isTx == true)
                        {
                            txMapPnl = mapPnl;
                        }
                        else
                        {
                            rxMapPnl = mapPnl;
                        }
                        mapPnl.fillParams.Clear();  //clear out the screen mapped bit representations 
                    }
                }
                #endregion empty mapping pnl data
                #region get the Tx and Rx CAN nodes for currCOB
                nodeInfo currCOBtxCANnode = null;
                nodeInfo currCOBrxCANNode = null;
                foreach (nodeInfo node in this.PDOableCANNodes)
                {
                    if ((currCOBtxData != null) && (node.nodeID == currCOBtxData.nodeID))
                    {
                        currCOBtxCANnode = node;
                    }
                    if ((currCOBrxData != null) && (node.nodeID == currCOBrxData.nodeID))
                    {
                        currCOBrxCANNode = node;
                    }
                }
                #endregion get the Tx and Rx CAN nodes for currCOB
                #region detemine which pre-filled PDO we are using
                int preFillPDOIndex = -1;
                if (dragTNTag.tagType == TNTagType.PreSetPDO_TractionLeft)
                {
                    #region traction left
                    preFillPDOIndex = 0;
                    if (currCOBtxCANnode == this.SevconMasterNode)
                    {
                        currCOBName = "Traction left control PDO";
                        newCOBName = "Traction left feedback PDO";
                    }
                    else //slave
                    {
                        newCOBName = "Traction left control PDO";
                        currCOBName = "Traction left feedback PDO";
                    }
                    #endregion traction left
                }
                else if (dragTNTag.tagType == TNTagType.PreSetPDO_TractionRight)
                {
                    #region traction right
                    preFillPDOIndex = 1;
                    if (currCOBtxCANnode == this.SevconMasterNode)
                    {
                        currCOBName = "Traction right control PDO";
                        newCOBName = "Traction right feedback PDO";
                    }
                    else //slave
                    {
                        newCOBName = "Traction right control PDO";
                        currCOBName = "Traction right feedback PDO";
                    }
                    #endregion traction right
                }
                else if (dragTNTag.tagType == TNTagType.PreSetPDO_Pump)
                {
                    #region Pump
                    preFillPDOIndex = 2;
                    if (currCOBtxCANnode == this.SevconMasterNode)
                    {
                        currCOBName = "Pump control PDO";
                        newCOBName = "Pump feedback PDO";
                    }
                    else //slave
                    {
                        newCOBName = "Pump control PDO";
                        currCOBName = "Pump feedback PDO";
                    }
                    #endregion Pump
                }
                else if (dragTNTag.tagType == TNTagType.PreSetPDO_PowerSteer)
                {
                    #region power steer
                    preFillPDOIndex = 3;
                    if (currCOBtxCANnode == this.SevconMasterNode)
                    {
                        currCOBName = "Power steer control PDO";
                        newCOBName = "Power steer feedback PDO";
                    }
                    else //slave
                    {
                        newCOBName = "Power steer control PDO";
                        currCOBName = "Power steer feedback PDO";
                    }
                    #endregion power steer
                }
                this.currCOB.name = currCOBName;  //eventuially a  unique check will be needed
                #endregion detemine which pre-filled PDO we are using

                #region add Tx maps (and screen mappings) to currCOBtxData
                if (currCOBtxCANnode != null)
                {
                    foreach (PDOMapping map in (ArrayList)currCOBtxCANnode.preFilledTxMotorSPDOMappings[preFillPDOIndex])
                    {
                        currCOBtxData.SPDOMaps.Add(map);
                        txMapPnl.addMapping(map);
                    }
                    #region update the expansion panel mappings
                    foreach (GroupBox gb in (ArrayList)this.txSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currTxPDOnode)])
                    {
                        if ((COBObject)gb.Tag == this.currCOB)
                        {
                            ListBox lb = (ListBox)gb.Controls[0];
                            if (currCOBtxData.SPDOMaps.Count > 0)  //dotNet hates empty datasources - seesm to screw up a backgorund position parameter
                            {
                                if (lb.DataSource == null)
                                {//first time we add an item to the dataSource
                                    BindingManagerBase managerLB = lb.BindingContext[currCOBtxData.SPDOMaps, "mapName"]; //set the manager 
                                    lb.DataSource = currCOBtxData.SPDOMaps;
                                    lb.DisplayMember = "mapName";
                                    lb.ValueMember = "mapValue";
                                }
                                CurrencyManager m_cm = (CurrencyManager)lb.BindingContext[currCOBtxData.SPDOMaps];
                                m_cm.Refresh();
                            }
                            break;
                        }
                    }
                    #endregion update the expansion panel mappings
                    #region update VPDO referneces for each affected COB
                    if ((this.SevconMasterNode != null) && (currCOBtxData.nodeID == this.SevconMasterNode.nodeID))
                    {
                        AddInternalTxPDOReferencesToSystemPDOs();
                    }
                    #endregion update VPDO referneces for each affected COB
                    #region update DI
                    this.sysInfo.nodes[dragTNTag.tableindex].updatePDOMappings(currCOB, true, 0);
                    #endregion unpdate DI
                }
                #endregion add maps to currCOBtxData
                #region add	Rx maps and mappings to currCOBrxData
                if (currCOBrxCANNode != null)
                {
                    foreach (PDOMapping map in (ArrayList)currCOBrxCANNode.preFilledRxMotorSPDOMappings[preFillPDOIndex])
                    {
                        currCOBrxData.SPDOMaps.Add(new PDOMapping(map.mapValue, map.mapName));
                        rxMapPnl.addMapping(map);
                    }
                    #region update expansion panels
                    foreach (GroupBox gb in (ArrayList)this.rxSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currRxPDOnode)])
                    {
                        if (((COBObject)gb.Tag) == this.currCOB)
                        {
                            if (currCOBrxData.SPDOMaps.Count > 0)  //avoid binding to empty data sources - dotnet hates this
                            {
                                ListBox lb = (ListBox)gb.Controls[0];
                                if (lb.DataSource == null)
                                {
                                    BindingManagerBase managerLB = lb.BindingContext[currCOBrxData.SPDOMaps, "mapName"]; //set the manager 
                                    lb.DataSource = currCOBrxData.SPDOMaps;
                                    lb.DisplayMember = "mapName";
                                    lb.ValueMember = "mapValue";
                                }
                                CurrencyManager m_cm = (CurrencyManager)lb.BindingContext[currCOBrxData.SPDOMaps];
                                m_cm.Refresh();
                            }
                            break;
                        }
                    }
                    #endregion update expansion panels
                    #region Update VPDO referneces in the COBs
                    if ((this.SevconMasterNode != null)
                        && (currCOBrxData.nodeID == this.SevconMasterNode.nodeID))
                    {
                        AddInternalRxPDOReferencesToSystemPDOs();
                    }
                    #endregion Update VPDO referneces in the COBs
                    #region update DI
                    this.sysInfo.nodes[dragTNTag.tableindex].updatePDOMappings(currCOB, false, 0);
                    #endregion unpdate DI
                }
                #endregion add	Rx maps and mappings to currCOBrxData

                int currCOBIndex = this.sysInfo.COBsInSystem.IndexOf(this.currCOB);
                this.activateCOB((COBObject)this.sysInfo.COBsInSystem[currCOBIndex]);
                if (SystemInfo.errorSB.Length > 0)
                {
                    sysInfo.displayErrorFeedbackToUser("Failed to add all nodes to " + currCOB.name);
                }
                this.PDOUnderConstruction = null;  //ready for next time
                this.deActivateInternalPDOs();
                calculateScreenLinesForPDOCOBSInSystem(); //does full positioning of all sreen routes - Note:we have to make the offsets contiguous again
                drawAndFillMappings();
                this.layoutPDOMappedBitsPanel();
                this.layoutTxSysPDOExpansionPanels();
                this.layoutRxSysPDOExpansionPanels();
                #endregion pre-filled pdo
            }
            else
            {
                #region normal OD item
                if (mappingPnl.isTx == true)
                {
                    #region Tx mapping drop
                    currCOBtxData = (COBObject.PDOMapData)this.currCOB.transmitNodes[0];
                    #region normal OD item
                    ObjDictItem odDraggedItem;
                    ODItemData odDraggedSub = this.sysInfo.nodes[dragTNTag.tableindex].getODSub(dragTNTag.assocSub.indexNumber, dragTNTag.assocSub.subNumber, out odDraggedItem);
                    if (odDraggedSub != null)
                    {
                        #region calculte the mapping we want to add
                        //long mapVal = convertODItemToMappingValue(sysInfo.nodes[dragTNTag.tableindex].dictionary.data[odItem.ind][odItem.sub]);
                        #endregion calculte the mapping we want to add
                        if (odDraggedSub.bitSplit != null)
                        {
                            #region bit split mapping - add all related bitsplit subs
                            int bitsUsedInPseudoSubs = 0;
                            foreach (ODItemData odSub in odDraggedItem.odItemSubs)
                            {
                                if ((odSub.bitSplit != null) && (odSub.bitSplit.realSubNo == odDraggedSub.bitSplit.realSubNo))
                                {//part of same bitsplit
                                    #region create & add bitsplit mapping
                                    PDOMapping map = new PDOMapping(this.convertODItemToMappingValue(odSub), odSub.parameterName);
                                    currCOBtxData.SPDOMaps.Add(map);
                                    mappingPnl.addMapping(map);
                                    bitsUsedInPseudoSubs += (int)odSub.dataSizeInBits;
                                    #endregion create & add bitsplit mapping
                                }
                            }
                            int origDataSizeInBits = sysInfo.nodes[dragTNTag.tableindex].getOrigDataSizeInBits((CANopenDataType)odDraggedSub.dataType);
                            if ((origDataSizeInBits - bitsUsedInPseudoSubs) > 0)
                            { //we have spare bits that are not defined as bit splits but need to be transmitted in PDO 
                                #region create non-defined bits mapping
                                long mapVal = ((long)dragTNTag.assocSub.indexNumber) << 16;
                                mapVal += (odDraggedSub.bitSplit.realSubNo << 8);
                                mapVal += (origDataSizeInBits - bitsUsedInPseudoSubs);
                                PDOMapping map = new PDOMapping(mapVal, SCCorpStyle.nonDefinedBitsText);
                                currCOBtxData.SPDOMaps.Add(map);
                                mappingPnl.addMapping(map);
                                #endregion create non-defined bits mapping
                            }
                            #endregion bit split mapping - add all related bitsplit subs
                        }
                        else
                        {
                            #region non-bitsplit item
                            PDOMapping map = new PDOMapping(this.convertODItemToMappingValue(odDraggedSub), odDraggedSub.parameterName);
                            currCOBtxData.SPDOMaps.Add(map);
                            mappingPnl.addMapping(map);
                            #endregion non-bitsplit item
                        }
                    }
                    #endregion normal OD item
                    #endregion Tx mapping drop
                }
                else
                {
                    #region rx mapping drop
                    #region determine which CAN frame datafield bit (on screen) that the user dropped the mouse over
                    Point myClientPpoint = new Point(0, 0);  //may to do in mouse over 
                    myClientPpoint = mappingPnl.PointToClient(new Point(e.X, e.Y));
                    int startBit = myClientPpoint.X / (mappingPnl.bitWidth);
                    #endregion determine which CAN frame datafield bit (on screen) that the user dropped the mouse over
                    currCOBrxData = (COBObject.PDOMapData)this.currCOB.receiveNodes[mappingPnl.pnlIndex];
                    #region normal addtion of mapping plus spacers
                    //see how many bits are already filled - idealy change the COBObject - but hold off for now
                    int bitsUsedInPanel = 0;
                    PDOMapping map;
                    ObjDictItem odDraggedItem;
                    ODItemData odDraggedSub = this.sysInfo.nodes[dragTNTag.tableindex].getODSub(dragTNTag.assocSub.indexNumber, dragTNTag.assocSub.subNumber, out odDraggedItem);
                    if (odDraggedSub != null)
                    {
                        #region handle dummy spacers first
                        foreach (sixtyFourBitsAsPanel.fillParam fp in mappingPnl.fillParams)
                        {
                            bitsUsedInPanel += fp.numBits;
                        }
                        if (startBit > bitsUsedInPanel)
                        {   //this will NEED to be moved to the Drag OVER - adding dummys can tak eus OVER the 8 permitted for Sevcon
                            #region add dummy spacers
                            //we need dummys to be inserted
                            //first see how many
                            startBit -= bitsUsedInPanel;//we just want the 'gap' between mappings end and where user wants to start the next one
                            if (startBit / 32 > 0)
                            {
                                //we can bput a 32 bit dummy in 
                                map = new PDOMapping(SCCorpStyle.spacer32bit, "32 bit spacer");
                                currCOBrxData.SPDOMaps.Add(map);
                                mappingPnl.addMapping(map);
                                startBit -= 32;
                            }
                            if (startBit / 16 > 0)
                            {
                                //we can bput a 32 bit dummy in 
                                map = new PDOMapping(SCCorpStyle.spacer16bit, "16 bit spacer");
                                currCOBrxData.SPDOMaps.Add(map);
                                mappingPnl.addMapping(map);
                                startBit -= 16;
                            }
                            if (startBit / 8 > 0)
                            {
                                startBit -= 8;
                                map = new PDOMapping(SCCorpStyle.spacer8bit, "8 bit spacer");
                                currCOBrxData.SPDOMaps.Add(map);
                                mappingPnl.addMapping(map);
                            }
                            //now add any 1 bits dummys that are required
                            for (int i = 0; i < startBit; i++)
                            {
                                map = new PDOMapping(SCCorpStyle.spacer1bit, "1 bit spacer");
                                currCOBrxData.SPDOMaps.Add(map);
                                mappingPnl.addMapping(map);
                            }
                            #endregion add dummy spacers
                        }
                        #endregion handle dummy spacers first
                        if (odDraggedSub.bitSplit != null)
                        {
                            #region bit split mapping - add all related bitsplit subs
                            int bitsUsedInPseudoSubs = 0;
                            foreach (ODItemData odSub in odDraggedItem.odItemSubs)
                            {
                                if ((odSub.bitSplit != null) && (odSub.bitSplit.realSubNo == odDraggedSub.bitSplit.realSubNo))
                                {
                                    #region create & add bitsplit mapping
                                    map = new PDOMapping(convertODItemToMappingValue(odSub), odSub.parameterName);
                                    currCOBrxData.SPDOMaps.Add(map);
                                    mappingPnl.addMapping(map);
                                    bitsUsedInPseudoSubs += (int)odSub.dataSizeInBits;
                                    #endregion create & add bitsplit mapping
                                }
                            }
                            int origDataSizeInBits = sysInfo.nodes[dragTNTag.tableindex].getOrigDataSizeInBits((CANopenDataType)odDraggedSub.dataType);
                            if ((origDataSizeInBits - bitsUsedInPseudoSubs) > 0)
                            { //we have spare bits that are not defined as bit splits but need to be transmitted in PDO 
                                #region create non-defined bits mapping
                                long mapVal = ((long)dragTNTag.assocSub.indexNumber) << 16;
                                mapVal += (odDraggedSub.bitSplit.realSubNo << 8);
                                mapVal += (origDataSizeInBits - bitsUsedInPseudoSubs);
                                map = new PDOMapping(mapVal, SCCorpStyle.nonDefinedBitsText);
                                currCOBrxData.SPDOMaps.Add(map);
                                mappingPnl.addMapping(map);
                                #endregion create non-defined bits mapping
                            }
                            #endregion bit split mapping - add all related bitsplit subs
                        }
                        else
                        {
                            #region non-bitsplit item
                            map = new PDOMapping(convertODItemToMappingValue(odDraggedSub), odDraggedSub.parameterName);
                            currCOBrxData.SPDOMaps.Add(map);
                            mappingPnl.addMapping(map);
                            #endregion non-bitsplit item
                        }
                    }
                    #endregion normal addtion of mapping plus spacers
                    #endregion rx mapping drop
                }
                #endregion normal OD item
                if (currCOBtxData != null)
                {
                    #region update the expansion panel mappings
                    foreach (GroupBox gb in (ArrayList)this.txSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currTxPDOnode)])
                    {
                        if (((COBObject)gb.Tag) == this.currCOB)
                        {
                            ListBox lb = (ListBox)gb.Controls[0];
                            if (currCOBtxData.SPDOMaps.Count > 0)  //dotNet hates empty datasources - seesm to screw up a backgorund position parameter
                            {
                                if (lb.DataSource == null)
                                {//first time we add an item to the dataSource
                                    BindingManagerBase managerLB = lb.BindingContext[currCOBtxData.SPDOMaps, "mapName"]; //set the manager 
                                    lb.DataSource = currCOBtxData.SPDOMaps;
                                    lb.DisplayMember = "mapName";
                                    lb.ValueMember = "mapValue";
                                }
                                CurrencyManager m_cm = (CurrencyManager)lb.BindingContext[currCOBtxData.SPDOMaps];
                                m_cm.Refresh();
                            }
                            break;
                        }
                    }
                    #endregion update the expansion panel mappings
                    #region update VPDO referneces for each affected COB
                    if ((this.SevconMasterNode != null) && (currCOBtxData.nodeID == this.SevconMasterNode.nodeID))
                    {
                        AddInternalTxPDOReferencesToSystemPDOs();
                    }
                    #endregion update VPDO referneces for each affected COB
                    #region pass data to DI
                    this.sysInfo.nodes[dragTNTag.tableindex].updatePDOMappings(this.currCOB, true, 0);
                    if (SystemInfo.errorSB.Length > 0)
                    {
                        sysInfo.displayErrorFeedbackToUser("Failed to Update mappings on COBID 0x" + currCOB.COBID.ToString("X").PadLeft(4, '0'));
                        //need to reread the PDO data from CANopen device
                    }
                    #endregion pass data to DI
                }
                if (currCOBrxData != null)
                {
                    #region update expansion panels
                    foreach (GroupBox gb in (ArrayList)this.rxSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(this.currRxPDOnode)])
                    {
                        if (((COBObject)gb.Tag) == this.currCOB)
                        {
                            if (currCOBrxData.SPDOMaps.Count > 0)  //avoid binding to empty data sources - dotnet hates this
                            {
                                ListBox lb = (ListBox)gb.Controls[0];
                                if (lb.DataSource == null)
                                {
                                    BindingManagerBase managerLB = lb.BindingContext[currCOBrxData.SPDOMaps, "mapName"]; //set the manager 
                                    lb.DataSource = currCOBrxData.SPDOMaps;
                                    lb.DisplayMember = "mapName";
                                    lb.ValueMember = "mapValue";
                                }
                                CurrencyManager m_cm = (CurrencyManager)lb.BindingContext[currCOBrxData.SPDOMaps];
                                m_cm.Refresh();
                            }
                            break;
                        }
                    }
                    #endregion update expansion panels
                    #region Update VPDO referneces in the COBs
                    if ((this.SevconMasterNode != null)
                        && (currCOBrxData.nodeID == this.SevconMasterNode.nodeID))
                    {
                        AddInternalRxPDOReferencesToSystemPDOs();
                    }
                    #endregion Update VPDO referneces in the COBs
                    #region pass to DI
                    this.sysInfo.nodes[dragTNTag.tableindex].updatePDOMappings(this.currCOB, false, mappingPnl.pnlIndex);
                    if (SystemInfo.errorSB.Length > 0)
                    {
                        sysInfo.displayErrorFeedbackToUser("Failed to Update mappings on COBID 0x" + currCOB.COBID.ToString("X").PadLeft(4, '0'));
                    }
                    #endregion pass to DI
                }
            }

            this.showUserControls();
        }
		private void SysPDOMappingPanel_MouseDown(object sender, MouseEventArgs e)
		{
			DriveWizard.sixtyFourBitsAsPanel mapPnl = (DriveWizard.sixtyFourBitsAsPanel) sender;
			#region - expand correct CNA node in treeView
			this.hideUserControls();
			if(mapPnl.isTx == true)
			{
				#region Tx System PDO mapping panel
				foreach(COBObject.PDOMapData txData in this.currCOB.transmitNodes)
				{
					foreach(nodeInfo node in this.PDOableCANNodes)
					{
						if(node.nodeID == txData.nodeID)
						{
							this.showTxSPDOItemsForThisCANNode(node);
							break;
						}
					}
					break;//once only
				}
				#endregion Tx System PDO mapping panel
			}
			else
			{
				#region Rx System PDO mapping panels
				foreach(COBObject.PDOMapData rxData in this.currCOB.receiveNodes)
				{
					if(mapPnl.pnlIndex == this.currCOB.receiveNodes.IndexOf(rxData))
					{
						foreach(nodeInfo node in this.PDOableCANNodes)
						{
							if(node.nodeID == rxData.nodeID)
							{
								showRxSPDOItemsForThisCANNode(node);
								break; //once only
							}
						}
						break;
					}
				}
				#endregion Rx System PDO mapping panels
			}
			#endregion - expand correct CNA node in treeView
			if(e.Button == MouseButtons.Right)
			{
				#region display context menu
				int bitMouseIsOver = e.X/mapPnl.bitWidth;
				foreach(sixtyFourBitsAsPanel.fillParam fp in mapPnl.fillParams)
				{
					if((fp.startBit <= bitMouseIsOver )	&& (bitMouseIsOver<(fp.startBit + fp.numBits)))
					{
						mapPnl.currfillParam = fp;
						currMapPnl = mapPnl;
						currMapPnl.currfillParam = fp;
						mapPnlIndex = mapPnl.pnlIndex;
						mappingIndex = mapPnl.fillParams.IndexOf(fp);
						MenuItem MIDeleteMapping = new MenuItem("Remove mapping: " + fp.paramName);
						MIDeleteMapping.Click +=new EventHandler(MIDeleteMapping_Click);
						this.contextMenu1.MenuItems.Clear();
						this.contextMenu1.MenuItems.Add(MIDeleteMapping);
						this.contextMenu1.Show(mapPnl, new Point(e.X, e.Y));
						return; //leave
					}
				}
				#endregion display context menu
			}
			this.showUserControls();
		}
		private void SysPDOMappingPanel_MouseMove(object sender, MouseEventArgs e)
		{
			DriveWizard.sixtyFourBitsAsPanel mapPnl = (DriveWizard.sixtyFourBitsAsPanel) sender;
			int bitMouseIsOver = (e.X/mapPnl.bitWidth);
			foreach(sixtyFourBitsAsPanel.fillParam fp in mapPnl.fillParams)
			{
				if((fp.startBit <= bitMouseIsOver +1 )	&& (bitMouseIsOver<(fp.startBit + fp.numBits )))
				{
					if(mapPnl.currfillParam != fp)
					{
						mapPnl.currfillParam = fp;
						this.toolTip1.SetToolTip(mapPnl, mapPnl.currfillParam.paramName);
						mapPnl.Invalidate();   //OK - no need to hide/show user controls
						return; //get out quick
					}
					else
					{
						return; //no change get out
					}
				}
			}
			mapPnl.currfillParam = null;// if we ge tto here we ar enot over a mapping
			mapPnl.Invalidate(); //OK - no need to hide/show user controls
		}

		private void SysPDOMappingPanel_MouseEnter(object sender, EventArgs e)
		{
			this.toolTip1.ShowAlways = true;
		}

		private void MIDeleteMapping_Click(object sender, EventArgs e)
		{
			DIFeedbackCode fbc;
			this.hideUserControls();
			bool isBitSplit = false;
			if(currMapPnl.isTx == true)
			{
				foreach(COBObject.PDOMapData txData in this.currCOB.transmitNodes)
				{
					#region check if the mapping we want to remove is bitsplit - we can only remove all of them
					#region get mapping that user wants to remove
					PDOMapping map = (PDOMapping) txData.SPDOMaps[mappingIndex];
	
					int UsrSelectedODIndex = (int)(map.mapValue >> 16);
					int UsrSelectedODSub = (int)(map.mapValue & 0xff00);
					UsrSelectedODSub = UsrSelectedODSub >> 8;
					int UsrSelectedRealSub = 0;
					#endregion get mapping that user wants to remove
					#region extract the CANNodeIndex
					int CANNodeindex;
					fbc = this.sysInfo.getNodeNumber(txData.nodeID, out CANNodeindex);
					if(fbc != DIFeedbackCode.DISuccess)
					{
						this.showUserControls();
						return;
					}
					#endregion extract the CANNodeIndex
					if(map.mapName == SCCorpStyle.nonDefinedBitsText)
					{
						UsrSelectedRealSub = UsrSelectedODSub;
						isBitSplit = true;
					}
					else
					{
						#region get the underlying mapped OD object
						ODItemData odmappedSub = this.sysInfo.nodes[CANNodeindex].getODSub(UsrSelectedODIndex, UsrSelectedODSub);
						if(odmappedSub != null)
						{
							if(odmappedSub.bitSplit != null)
							{ //the mapping that the user wants to remove is bitsplit - so we need ot remove them all
								//get the 'real' sub index - which we can use to match any other bitSplit items  in the PDO to
								UsrSelectedRealSub = odmappedSub.bitSplit.realSubNo;
								isBitSplit = true;
							}
						}
						#endregion get the underlying mapped OD object
					}
					if(isBitSplit == true)
					{
						#region identify and remove all the othe rmappings for this bitsplit
						ArrayList mapsToGo = new ArrayList();
						foreach(PDOMapping otherMap in txData.SPDOMaps)
						{
							int otherMapODIndex = (int) (otherMap.mapValue >> 16);
							int otherMapODSub = (int)(otherMap.mapValue & 0xff00);
							otherMapODSub = otherMapODSub >> 8;
							if(otherMap.mapName == SCCorpStyle.nonDefinedBitsText)
							{
								if((otherMapODIndex == UsrSelectedODIndex) && (otherMapODSub == UsrSelectedRealSub))
								{
									mapsToGo.Add(otherMap);
								}
							}
							else
							{
								ODItemData otherODSub = this.sysInfo.nodes[CANNodeindex].getODSub(otherMapODIndex, otherMapODSub);
								if((otherODSub != null) 
									&& (otherODSub.bitSplit != null)
									&& (otherMapODIndex == UsrSelectedODIndex) //could be same sub on a differnet ODindex
									&& (otherODSub.bitSplit.realSubNo == UsrSelectedRealSub))
								{ //this mapping is part of the same bitsplit
									mapsToGo.Add(otherMap);
								}
							}
						}
						foreach(PDOMapping mapToGo in mapsToGo)
						{
							if(txData.SPDOMaps.Contains(mapToGo) == true)
							{
								txData.SPDOMaps.Remove(mapToGo);
							}
						}
						#endregion identify and remove all the othe rmappings for this bitsplit
					}
					else
					{  
						#region remove normal non-bitsplit mapping
						txData.SPDOMaps.RemoveAt(mappingIndex);
						#endregion remove normal non-bitsplit mapping
					}
					#endregion chekc if the mapping we want to remove is bitsplit - we can only remove all of them
					#region clear and refill the mappings panle from txData.SPDOMaps
					currMapPnl.fillParams.Clear();
					foreach(PDOMapping mp in txData.SPDOMaps)
					{
						currMapPnl.addMapping(mp);
					}
					#endregion clear and refill the mappings panle from txData.SPDOMaps
					int nodeInd = 0;
					fbc = this.sysInfo.getNodeNumber(txData.nodeID, out nodeInd);
					if(fbc == DIFeedbackCode.DISuccess)
					{
						this.sysInfo.nodes[nodeInd].updatePDOMappings(this.currCOB, true, 0);
					}
					break;  //first one only for Tx
				}
			}
			else
			{
				foreach(COBObject.PDOMapData rxData in this.currCOB.receiveNodes)
				{
					if(this.mapPnlIndex == this.currCOB.receiveNodes.IndexOf(rxData))
					{//this is the correct rxData for this panel
						//now find the mapping
						#region check if the mapping we want to remove is bitsplit - we can only remove all of them
						#region get mapping that user wants to remove
						PDOMapping map = (PDOMapping) rxData.SPDOMaps[mappingIndex];
						#endregion get mapping that user wants to remove
						if((map.mapValue == SCCorpStyle.spacer32bit) || ( map.mapValue == SCCorpStyle.spacer16bit)
							|| (map.mapValue == SCCorpStyle.spacer8bit)|| (map.mapValue == SCCorpStyle.spacer1bit))
						{
							#region CANopen spacer map
							rxData.SPDOMaps.RemoveAt(mappingIndex);
							#endregion CANopen spacer map
						}
						else
						{
							#region normal OD mapping
							int UsrSelectedODIndex = (int)(map.mapValue >> 16);
							int UsrSelectedODSub = (int)(map.mapValue & 0xff00);
							UsrSelectedODSub = UsrSelectedODSub >> 8;
							int UsrSelectedRealSub = 0;
							
							#region extract the CANNodeIndex
							int CANNodeindex;
							fbc = this.sysInfo.getNodeNumber(rxData.nodeID, out CANNodeindex);
							if(fbc != DIFeedbackCode.DISuccess)
							{
								this.showUserControls();
								return;
							}
							#endregion extract the CANNodeIndex

							if(map.mapName == SCCorpStyle.nonDefinedBitsText)
							{
								UsrSelectedRealSub = UsrSelectedODSub;
								isBitSplit = true;
							}
							else
							{
								#region get the underlying mapped OD object
								ODItemData mappedODSub = this.sysInfo.nodes[CANNodeindex].getODSub(UsrSelectedODIndex,UsrSelectedODSub );
								if((mappedODSub != null) && (mappedODSub.bitSplit != null))//exists in dictionary and is bitsplit
								{ //the mapping that the user wants to remove is bitsplit - so we need ot remove them all
									//get the 'real' sub index - which we can use to match any other bitSplit items  in the PDO to
									UsrSelectedRealSub = mappedODSub.bitSplit.realSubNo;
									isBitSplit = true;
								}
								#endregion get the underlying mapped OD object
							}
							if(isBitSplit == true)
							{
								#region identify and remove all the othe rmappings for this bitsplit
								ArrayList mapsToGo = new ArrayList();
								foreach(PDOMapping otherMap in rxData.SPDOMaps)
								{
									int otherMapODIndex = (int) (otherMap.mapValue >> 16);
									int otherMapODSub = (int)(otherMap.mapValue & 0xff00);
									otherMapODSub = otherMapODSub >> 8;
									if(otherMap.mapName == SCCorpStyle.nonDefinedBitsText)
									{
										if((otherMapODIndex == UsrSelectedODIndex) && (otherMapODSub == UsrSelectedRealSub))
										{
											mapsToGo.Add(otherMap);
										}
									}
									else
									{
										ODItemData otherODsub = this.sysInfo.nodes[CANNodeindex].getODSub(otherMapODIndex, otherMapODSub);
										if(otherODsub != null) 
										{//object exists in dictionary
											if((otherODsub.bitSplit != null)
												&& (otherMapODIndex == UsrSelectedODIndex) //could be same sub on a differnet ODindex
												&& (otherODsub.bitSplit.realSubNo == UsrSelectedRealSub))
											{ //this mapping is part of the same bitsplit
												mapsToGo.Add(otherMap);
											}
										}
									}
								}
								foreach(PDOMapping mapToGo in mapsToGo)
								{
									if(rxData.SPDOMaps.Contains(mapToGo) == true)  //B&B
									{
										rxData.SPDOMaps.Remove(mapToGo);
									}
								}
								#endregion identify and remove all the othe rmappings for this bitsplit
							}
							else
							{  
								#region remove normal non-bitsplit mapping
								rxData.SPDOMaps.RemoveAt(mappingIndex);
								#endregion remove normal non-bitsplit mapping
							}
							#endregion normal OD mapping
						}
						#endregion check if the mapping we want to remove is bitsplit - we can only remove all of them

						#region clear and refill the mappings panle from rxData.SPDOMaps
						currMapPnl.fillParams.Clear();
						foreach(PDOMapping newMap in rxData.SPDOMaps) 
						{
							currMapPnl.addMapping(newMap);
						}
						int nodeInd = 0;
						fbc = this.sysInfo.getNodeNumber(rxData.nodeID, out nodeInd);
						if(fbc == DIFeedbackCode.DISuccess)
						{
							this.sysInfo.nodes[nodeInd].updatePDOMappings(this.currCOB, false,this.currCOB.receiveNodes.IndexOf(rxData));
						}
						#endregion clear and refill the mappings panle from rxData.SPDOMaps
						break;  //found it so leave
					}
				}
			}
			if(SystemInfo.errorSB.Length>0)
			{
				this.sysInfo.displayErrorFeedbackToUser("Errors occurred when deleting mapping");
			}
			this.showUserControls();
		}

		private void Pnl_COBMappings_Resize(object sender, EventArgs e)
		{
			layoutPDOMappedBitsPanel();
		}
		#endregion SPDO Mappings Panel Event Hanlders
		#region link VPDO signals to each SPDO
		private void AddInternalTxPDOReferencesToSystemPDOs()
		{
			foreach(COBObject COB in this.sysInfo.COBsInSystem)
			{
				if(COB.messageType == COBIDType.PDO)
				{
					#region relevent to PDO tpye COBS only
					#region clear the assoc int mappings lists
					COB.assocMotor.Clear();
					COB.assocDigOPs.Clear();
					COB.assocAlgOPs.Clear();
					#endregion clear the assoc int mappings lists
					foreach(COBObject.PDOMapData txData in COB.transmitNodes)
					{
						if(txData.nodeID == this.SevconMasterNode.nodeID) //ie we are looking at the master CAN node
						{  
							foreach(PDOMapping SysPDOMap in txData.SPDOMaps)
							{
								int SysMapIndex = (int)(SysPDOMap.mapValue>>16);
								#region motor
								foreach(PDOMapping intPDOMap in this.SevconMasterNode.intPDOMaps.MotorMaps)
								{
									if(SysMapIndex == intPDOMap.mapValue)
									{
										COB.assocMotor.Add( intPDOMap);
									}
								}
								#endregion motor
								#region digital outputs
								foreach(PDOMapping intPDOMap in this.SevconMasterNode.intPDOMaps.digOPMaps)
								{
									if(SysMapIndex == intPDOMap.mapValue)
									{
										COB.assocDigOPs.Add( intPDOMap);
									}
								}
								#endregion digital outputs
								#region analogue outputs
								foreach(PDOMapping intPDOMap in this.SevconMasterNode.intPDOMaps.algOPMaps)
								{
									if(SysMapIndex == intPDOMap.mapValue)
									{
										COB.assocAlgOPs.Add( intPDOMap);
									}
								}
								#endregion analogue outputs
							}
							break;
						}
					}
					#endregion relevent to PDO tpye COBS only
				}
			}
		}
		private void AddInternalRxPDOReferencesToSystemPDOs()
		{

			foreach(COBObject COB in this.sysInfo.COBsInSystem)
			{
				if(COB.messageType == COBIDType.PDO)
				{
					#region relevent to PDO tpye COBS only
					#region clear the assoc int mappings lists
					COB.assocDigIPs.Clear();
					COB.assocAlgIPs.Clear();
					#endregion clear the assoc int mappings lists
					foreach(COBObject.PDOMapData rxData in COB.receiveNodes)
					{
						if(rxData.nodeID == SevconMasterNode.nodeID) //we are only interested in the Sevcon master - if it exists
						{
							foreach(PDOMapping SysPDOMap in rxData.SPDOMaps)
							{
								int SysMapIndex = (int)(SysPDOMap.mapValue>>16);
								#region digital Inputs
								foreach(PDOMapping intPDOMap in this.SevconMasterNode.intPDOMaps.digIPMaps)
								{
									if(SysMapIndex == intPDOMap.mapValue)
									{
										COB.assocDigIPs.Add( intPDOMap);
									}
								}
								#endregion digital Inputs
								#region analogue inputs
								foreach(PDOMapping intPDOMap in this.SevconMasterNode.intPDOMaps.algIPMaps)
								{
									if(SysMapIndex == intPDOMap.mapValue)
									{
										COB.assocAlgIPs.Add( intPDOMap);
									}
								}
								#endregion analogue inputs
							}
							break;
						}
					}
					#endregion relevent to PDO tpye COBS only
				}
			}
		}
		#endregion link VPDO signals to each SPDO
		#region PDO Routing Painting methods
		private void PDORoutingPanel_Paint(object sender, PaintEventArgs e)
		{
			#region painting elastic lines
			if(routeStartPt.X >=0) 
			{
				this.drawPDORoutes(e.Graphics);
				e.Graphics.DrawLine(Pens.Black,routeStartPt ,this.PDOMousePos);  //offset to control is set when we start the line
				return;
			}
			#endregion painting elastic lines
			#region draw arrow to current PDO node
			Panel currTxScrNode = (Panel) this.TxPDONodePanels[this.PDOableCANNodes.IndexOf(this.currTxPDOnode)];
			Panel currRxScrNode = (Panel) this.RxPDONodePanels[this.PDOableCANNodes.IndexOf(this.currRxPDOnode)];
			Point [] txArrowPts = new Point[7];
			Point [] txArrowShadowPts = new Point[7];
			Point [] rxArrowPts = new Point[7];
			Point [] rxArrowShadowPts = new Point[7];
			for(int i = 0;i<this.genericHorizArrowPts.Length;i++)
			{
				txArrowPts[i]  = new Point(genericHorizArrowPts[i].X + currTxScrNode.Left - 20, 
					genericHorizArrowPts[i].Y+ currTxScrNode.Top + (currTxScrNode.Height/2) );
				txArrowShadowPts[i] = new Point(txArrowPts[i].X + 2 , txArrowPts[i].Y + 2 );
				rxArrowPts[i] = new Point(genericHorizArrowPts[i].X + currRxScrNode.Right ,
					genericHorizArrowPts[i].Y+ currRxScrNode.Top + (currRxScrNode.Height/2));
				rxArrowShadowPts[i] = new Point(rxArrowPts[i].X + 2 , rxArrowPts[i].Y + 2);
			}

			e.Graphics.FillPolygon(Brushes.Gray, txArrowShadowPts);
			e.Graphics.FillPolygon( Brushes.White, txArrowPts);
			e.Graphics.DrawPolygon( Pens.Black, txArrowPts);

			e.Graphics.FillPolygon(Brushes.Gray, rxArrowShadowPts);
			e.Graphics.FillPolygon( Brushes.White, rxArrowPts);
			e.Graphics.DrawPolygon( Pens.Black, rxArrowPts);

			Point [] txVertArrowPts = new Point[7];
			Point [] txVertArrowShadowPts = new Point[7];
			Point [] rxVertArrowPts = new Point[7];
			Point [] rxVertArrowShadowPts = new Point[7];
			if(this.SevconMasterNode != null)
			{
				for(int i = 0;i<this.genericVertArrowPts.Length;i++)
				{
					txVertArrowPts[i]  = new Point(
						genericVertArrowPts[i].X +  ((Panel)this.TxPDONodePanels[0]).Left + ((Panel)this.TxPDONodePanels[0]).Width/2, 
						genericVertArrowPts[i].Y+ ((Panel)this.TxPDONodePanels[0]).Top );
					txVertArrowShadowPts[i] = new Point(txVertArrowPts[i].X + 2, txVertArrowPts[i].Y + 2);
					rxVertArrowPts[i] = new Point(genericVertArrowPts[i].X + ((Panel)this.RxPDONodePanels[0]).Left + ((Panel)this.RxPDONodePanels[0]).Width/2, 
						genericVertArrowPts[i].Y+ + ((Panel)this.RxPDONodePanels[0]).Top);
					rxVertArrowShadowPts[i] = new Point(rxVertArrowPts[i].X + 2, rxVertArrowPts[i].Y + 2);
				}
				e.Graphics.FillPolygon(Brushes.Gray, txVertArrowShadowPts);
				e.Graphics.FillPolygon( Brushes.White, txVertArrowShadowPts);
				e.Graphics.DrawPolygon( Pens.Black, txVertArrowPts);

				e.Graphics.FillPolygon(Brushes.Gray, rxVertArrowShadowPts);
				e.Graphics.FillPolygon( Brushes.White, rxVertArrowPts);
				e.Graphics.DrawPolygon( Pens.Black, rxVertArrowPts);
			}
			#endregion draw arrow to current PDO node
			this.drawPDORoutes(e.Graphics);
			e.Graphics.DrawLine(Pens.Navy, this.PDORoutingPanel.ClientRectangle.Width/2, 0, this.PDORoutingPanel.ClientRectangle.Width/2,this.PDORoutingPanel.Height); 
			#region apply panel shadows - do last to be in front of routes (but behind controls)
			foreach(Control cont in this.PDORoutingPanel.Controls)
			{
				if(cont is System.Windows.Forms.Panel)
				{
					if((cont.Visible == true) 
						&& (this.TxPDONodePanels.Contains(cont) == false)
						&& ( this.RxPDONodePanels.Contains(cont) == false))
					{
						e.Graphics.FillRectangle(Brushes.Gray,cont.Left + 2, cont.Top+2, cont.Width, cont.Height);
					}
				}
				if((this.SevconMasterNode!= null) && (cont is GroupBox))
				{
					if((VPDO_GBs != null) && (this.VPDO_GBs.Contains(cont) == false)) //prevent exception
					{
						e.Graphics.FillRectangle(Brushes.Gray,cont.Left + 2, cont.Top+4, cont.Width, cont.Height-2);
					}
				}

			}
			for(int i = 0;i< this.PDOableCANNodes.Count;i++)
			{
				Panel txPnl = (Panel) this.TxPDONodePanels[i];
				Panel rxPnl = (Panel) this.RxPDONodePanels[i];
				nodeInfo node = (nodeInfo) this.PDOableCANNodes[i];
				e.Graphics.FillRectangle(Brushes.Green,txPnl.Left + 2, txPnl.Top+2, txPnl.Width, txPnl.Height);
				e.Graphics.FillRectangle(Brushes.DarkViolet,rxPnl.Left + 2, rxPnl.Top+2, rxPnl.Width, rxPnl.Height);
			}
			#endregion apply panle shadows
		}

		private void PDORoutingPanel_Resize(object sender, EventArgs e)
		{
			this.hideUserControls();
			layoutPDOGraphics();
			calculateScreenLinesForPDOCOBSInSystem();
			layoutTxPDOInterfacePanels(); 
			layoutTxSysPDOExpansionPanels();
			layoutRxPDOInterfacePanels(); 
			layoutRxSysPDOExpansionPanels();
			this.showUserControls();
		}
		private void drawPDORoutes(Graphics sysPDOGraphics)
		{
			foreach(COBObject COB in this.sysInfo.COBsInSystem)
			{ //od the greyout out ones first
				foreach( COBObject.screenRoutePoints permLine in COB.screenRoutes)
				{
					Panel txScrNode = null, rxScrNode = null;
					if(permLine.TxNodeScreenIndex != -1)
					{
						txScrNode = (Panel) this.TxPDONodePanels[permLine.TxNodeScreenIndex];
					}
					if(permLine.RxNodeScreenIndex != -1)
					{
						rxScrNode = (Panel) this.RxPDONodePanels[permLine.RxNodeScreenIndex];
					}

					if((this.activeIntPDO != null ) 
						|| (SysPDOsToHighLight.Contains(COB) == false)
						|| COB != this.currCOB)
					{  //when we repaint all the routes are offset to the screenTx (orRx for unproduced PDOs) 
						//thismeans that we can use autoscroll on the panel and still have our 'hand' drawn lins in the correct postion
						//since the autoscroll functionality has alread pladec the controls in the correct palce
						// all we need to do is hook our routes to the correct screen node
						if(txScrNode != null)
						{
							sysPDOGraphics.FillRectangle(Brushes.Gray, permLine.startRect.X + txScrNode.Right, permLine.startRect.Y + txScrNode.Top, permLine.startRect.Width, permLine.startRect.Height);
							sysPDOGraphics.FillRectangle(Brushes.Gray, permLine.midRect.X + txScrNode.Right, permLine.midRect.Y + txScrNode.Top, permLine.midRect.Width, permLine.midRect.Height);
							sysPDOGraphics.FillRectangle(Brushes.Gray, permLine.endRect.X + txScrNode.Right, permLine.endRect.Y + txScrNode.Top, permLine.endRect.Width, permLine.endRect.Height);

							//now draw end arrowhead
							if(rxScrNode != null)
							{//put arrow at end of endRect
								Point [] arrowPts = new Point[4];
								arrowPts[0] = new Point(permLine.endRect.Right -6 + + txScrNode.Right, permLine.endRect.Top-4 +  txScrNode.Top);
								arrowPts[1] = new Point(permLine.endRect.Right -6+ txScrNode.Right, permLine.endRect.Bottom +4 + txScrNode.Top);
								arrowPts[2] = new Point(permLine.endRect.Right + txScrNode.Right, permLine.endRect.Top + txScrNode.Top);
								arrowPts[3] = new Point(permLine.endRect.Right + txScrNode.Right, permLine.endRect.Bottom + txScrNode.Top);
								sysPDOGraphics.FillPolygon(Brushes.Gray, arrowPts);
							}
							else
							{//put arrow at end of start recttangle
								Point [] arrowPts = new Point[4];
								arrowPts[0] = new Point(permLine.startRect.Right -6 + + txScrNode.Right, permLine.startRect.Top-4 +  txScrNode.Top);
								arrowPts[1] = new Point(permLine.startRect.Right -6+ txScrNode.Right, permLine.startRect.Bottom +4 + txScrNode.Top);
								arrowPts[2] = new Point(permLine.startRect.Right + txScrNode.Right, permLine.startRect.Top + txScrNode.Top);
								arrowPts[3] = new Point(permLine.startRect.Right + txScrNode.Right, permLine.startRect.Bottom + txScrNode.Top);
								sysPDOGraphics.FillPolygon(Brushes.Gray, arrowPts);
							}

						}
						else if(rxScrNode != null)
						{
							sysPDOGraphics.FillRectangle(Brushes.Gray, permLine.endRect.X + rxScrNode.Left, permLine.endRect.Y + rxScrNode.Top, permLine.endRect.Width, permLine.endRect.Height);
							//now draw end arrowhead
							Point [] arrowPts = new Point[4];
							arrowPts[0] = new Point(permLine.endRect.Right -6 + rxScrNode.Left, permLine.endRect.Top-4+ rxScrNode.Top);
							arrowPts[1] = new Point(permLine.endRect.Right -6 + rxScrNode.Left, permLine.endRect.Bottom+4+ rxScrNode.Top);
							arrowPts[2] = new Point(permLine.endRect.Right + rxScrNode.Left, permLine.endRect.Top + rxScrNode.Top);
							arrowPts[3] = new Point(permLine.endRect.Right + rxScrNode.Left, permLine.endRect.Bottom + rxScrNode.Top);
							sysPDOGraphics.FillPolygon(Brushes.Gray, arrowPts);

						}
					}
				}
			}
			//now do any to be coloured in REd
			foreach(COBObject COB in this.sysInfo.COBsInSystem)
			{
				if(
					((this.activeIntPDO != null ) && (SysPDOsToHighLight.Contains(COB) == true))
					|| (COB == this.currCOB)
					)
				{
					foreach( COBObject.screenRoutePoints permLine in COB.screenRoutes)
					{
						Panel txScrNode = null, rxScrNode = null;
						if(permLine.TxNodeScreenIndex != -1)
						{
							txScrNode = (Panel) this.TxPDONodePanels[permLine.TxNodeScreenIndex];
						}
						if(permLine.RxNodeScreenIndex != -1)
						{
							rxScrNode = (Panel) this.RxPDONodePanels[permLine.RxNodeScreenIndex];
						}
						if(txScrNode != null)
						{
							sysPDOGraphics.FillRectangle(Brushes.Red, permLine.startRect.X + txScrNode.Right, permLine.startRect.Y + txScrNode.Top, permLine.startRect.Width, permLine.startRect.Height);
							sysPDOGraphics.FillRectangle(Brushes.Red, permLine.midRect.X + txScrNode.Right, permLine.midRect.Y + txScrNode.Top, permLine.midRect.Width, permLine.midRect.Height);
							sysPDOGraphics.FillRectangle(Brushes.Red, permLine.endRect.X + txScrNode.Right, permLine.endRect.Y + txScrNode.Top, permLine.endRect.Width, permLine.endRect.Height);

							if(rxScrNode != null)
							{//put arrow at end of endRect
								Point [] arrowPts = new Point[4];
								arrowPts[0] = new Point(permLine.endRect.Right -6 + + txScrNode.Right, permLine.endRect.Top-4 +  txScrNode.Top);
								arrowPts[1] = new Point(permLine.endRect.Right -6+ txScrNode.Right, permLine.endRect.Bottom +4 + txScrNode.Top);
								arrowPts[2] = new Point(permLine.endRect.Right + txScrNode.Right, permLine.endRect.Top + txScrNode.Top);
								arrowPts[3] = new Point(permLine.endRect.Right + txScrNode.Right, permLine.endRect.Bottom + txScrNode.Top);
								sysPDOGraphics.FillPolygon(Brushes.Red, arrowPts);
							}
							else
							{//put arrow at end of start recttangle
								Point [] arrowPts = new Point[4];
								arrowPts[0] = new Point(permLine.startRect.Right -6 + + txScrNode.Right, permLine.startRect.Top-4 +  txScrNode.Top);
								arrowPts[1] = new Point(permLine.startRect.Right -6+ txScrNode.Right, permLine.startRect.Bottom +4 + txScrNode.Top);
								arrowPts[2] = new Point(permLine.startRect.Right + txScrNode.Right, permLine.startRect.Top + txScrNode.Top);
								arrowPts[3] = new Point(permLine.startRect.Right + txScrNode.Right, permLine.startRect.Bottom + txScrNode.Top);
								sysPDOGraphics.FillPolygon(Brushes.Red, arrowPts);
							}

						}
						else if(rxScrNode != null)
						{
							sysPDOGraphics.FillRectangle(Brushes.Red, permLine.endRect.X + rxScrNode.Left, permLine.endRect.Y + rxScrNode.Top, permLine.endRect.Width, permLine.endRect.Height);
							//now draw end arrowhead
							Point [] arrowPts = new Point[4];
							arrowPts[0] = new Point(permLine.endRect.Right -6 + rxScrNode.Left, permLine.endRect.Top-4+ rxScrNode.Top);
							arrowPts[1] = new Point(permLine.endRect.Right -6 + rxScrNode.Left, permLine.endRect.Bottom+4+ rxScrNode.Top);
							arrowPts[2] = new Point(permLine.endRect.Right + rxScrNode.Left, permLine.endRect.Top + rxScrNode.Top);
							arrowPts[3] = new Point(permLine.endRect.Right + rxScrNode.Left, permLine.endRect.Bottom + rxScrNode.Top);
							sysPDOGraphics.FillPolygon(Brushes.Red, arrowPts);
						}
					}
				}
			}
		}
		#endregion PDO Routing Painting methods
		#region SPDO routes Context Menu event handlers
		private void MI_removethisPDO_Click(object sender, EventArgs e)
		{
			int nextCOBIndex = Math.Max(0,this.sysInfo.COBsInSystem.IndexOf(this.currCOB)- 1);
			//remove each node 'leg' seperately this allow code reuse when we wan tto detete single legs
			foreach(COBObject.PDOMapData txData in this.currCOB.transmitNodes)
			{
				this.removeSPDOExtensionPanel(txData.nodeID, true);
			}
			foreach(COBObject.PDOMapData rxData in this.currCOB.receiveNodes)
			{
				this.removeSPDOExtensionPanel(rxData.nodeID, false);
			}
			this.layoutRxSysPDOExpansionPanels();
			this.layoutTxSysPDOExpansionPanels();
			this.sysInfo.deletePDOMapAndComms(this.currCOB);
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("Failed to remove " + currCOB.name);
				this.updatePDODataBindings(nextCOBIndex);
			}
			this.sysInfo.COBsInSystem.Remove(this.currCOB);
			if(this.sysInfo.COBsInSystem.Count>0)
			{  //move to the next one down
				this.updatePDODataBindings(nextCOBIndex);
				calculateScreenLinesForPDOCOBSInSystem(); //does full positioning of all sreen routes - Note:we have to make the offsets contiguous again
				this.activateCOB((COBObject) this.sysInfo.COBsInSystem[nextCOBIndex]);
			}
		}
		private void MI_removeThisTxNodeFromThisPDO_Click(object sender, EventArgs e)
		{
			foreach(COBObject.PDOMapData txData in this.currCOB.transmitNodes)
			{
				if(txData.nodeID == nodeIDOfPDORouteLegToRemove)
				{
					for(int i = 0;i<this.sysInfo.nodes.Length;i++)
					{
						if(this.sysInfo.nodes[i].nodeID == nodeIDOfPDORouteLegToRemove)
						{//this order of these is important
							this.hideUserControls();
							this.removeSPDOExtensionPanel(nodeIDOfPDORouteLegToRemove, true);
							this.layoutTxSysPDOExpansionPanels();
							this.sysInfo.nodes[i].removeCANNodeFromPDO(this.currCOB, true, this.currCOB.transmitNodes.IndexOf(txData));
							if(SystemInfo.errorSB.Length>0)
							{
								sysInfo.displayErrorFeedbackToUser("Failed to remove node ID " + sysInfo.nodes[i].nodeID.ToString() + " from " + currCOB.name);
							}
							//change the TxScrren node
							this.currCOB.transmitNodes.Remove(txData); //do after DI has finished
							if(this.currCOB.transmitNodes.Count>0)
							{
								COBObject.PDOMapData newtxData = (COBObject.PDOMapData) this.currCOB.transmitNodes[0];
								foreach(nodeInfo CANnode in this.PDOableCANNodes)
								{
									if(newtxData.nodeID == CANnode.nodeID)
									{ 
										this.activateTxScreenNode(CANnode);
									}
								}
							}
							this.nodeIDOfPDORouteLegToRemove = -1;
							this.calculateScreenLinesForPDOCOBSInSystem();
							this.drawAndFillMappings();
							this.layoutPDOMappedBitsPanel();
							this.showUserControls();
							return;  //found it so leave
						}
					}
				}
			}
		}
		private void MI_removeThisRxNodeFromThisPDO_Click(object sender, EventArgs e)
		{
			foreach(COBObject.PDOMapData rxData in this.currCOB.receiveNodes)
			{
				if(rxData.nodeID == nodeIDOfPDORouteLegToRemove)
				{
					for(int i = 0;i<this.sysInfo.nodes.Length;i++)
					{
						if(this.sysInfo.nodes[i].nodeID == nodeIDOfPDORouteLegToRemove)
						{
							this.hideUserControls();
							this.removeSPDOExtensionPanel(nodeIDOfPDORouteLegToRemove, false);
							this.layoutRxSysPDOExpansionPanels();
							this.sysInfo.nodes[i].removeCANNodeFromPDO(this.currCOB, false, this.currCOB.receiveNodes.IndexOf(rxData));
							if(SystemInfo.errorSB.Length>0)
							{
								sysInfo.displayErrorFeedbackToUser("Failed to remove receive node ID " + sysInfo.nodes[i].nodeID.ToString() + " from " + currCOB.name);
							}
							this.currCOB.receiveNodes.Remove(rxData);//do after DI has finished
							if(this.currCOB.receiveNodes.Count>0)
							{
								COBObject.PDOMapData newrxData = (COBObject.PDOMapData) this.currCOB.receiveNodes[0];
								foreach(nodeInfo CANnode in this.PDOableCANNodes)
								{
									if(newrxData.nodeID == CANnode.nodeID)
									{ 
										this.activateRxScreenNode(CANnode);
									}
								}
							}
							this.nodeIDOfPDORouteLegToRemove = -1;  //B&B
							this.calculateScreenLinesForPDOCOBSInSystem();
							this.drawAndFillMappings();
							this.layoutPDOMappedBitsPanel();
							this.showUserControls();
							return;  //found it so leave
						}
					}
				}
			}
		}

		private void removeSPDOExtensionPanel(int passed_nodeID, bool isTx)
		{
			foreach(nodeInfo CANnode in this.PDOableCANNodes)
			{
				if(passed_nodeID == CANnode.nodeID)
				{
					ArrayList GBsForCANNode;
					if(isTx == true)
					{
						GBsForCANNode = (ArrayList) this.txSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(CANnode)];
					}
					else
					{
						GBsForCANNode = (ArrayList) this.rxSysPDOExpansionPnls[this.PDOableCANNodes.IndexOf(CANnode)];
					}
					foreach(GroupBox gb in GBsForCANNode)
					{
						if(((COBObject) gb.Tag) == this.currCOB)  //slightly dodgy since this isn't defined as unique - realy ought to use COBId as tag for GroupBox - databind to tag??
						{
							//now remove it from the screen? 
							foreach(Control cont in this.PDORoutingPanel.Controls)
							{
								if(cont == gb)
								{
									this.PDORoutingPanel.Controls.Remove(gb); //we can remove ofrm a colloection in a foreach loop providing we reomve max or 1 tiem only then leave - otherwise it screws the loop counter aand we need to use seperate collection
									break; 
								}
							}
							GBsForCANNode.Remove(gb); /// now remove for our array list
							break; //we should only have one
						}
					}
					break;
				}
			}
		}
		#endregion SPDO routes Context Menu event handlers
		#region SPDO Expansion Panels ListBoxes Event hanlders
		private void SPDOlb_DrawItem(object sender, DrawItemEventArgs e)
		{
			ListBox lb = (ListBox) sender;
			if( (lb.DataSource == null)
				|| (e.Index >= ((ArrayList) lb.DataSource).Count ) )
			{
				return;  //has to be null until we have added an item - other wise bindingContext fails
			}
			Brush myBrush = Brushes.Black;
			Brush myBackBrush = Brushes.White;
			if( this.activeIntPDO != null)
			{
				long mapVal = ((PDOMapping)((ArrayList)lb.DataSource)[e.Index]).mapValue;
				int intMapEquivalent = (int) (mapVal>>16); //extract jus the ODIndex tha tis used for uinternal mapping
				if(this.activeIntPDO.mapValue == intMapEquivalent)
				{
					myBrush = Brushes.Red;
				}
			}
			#region paint this item and its background in correct colours
			string itemText = ((PDOMapping)((ArrayList)lb.DataSource)[e.Index]).mapName;
			e.Graphics.FillRectangle(myBackBrush, e.Bounds);
			e.Graphics.DrawString(itemText, e.Font, myBrush,e.Bounds,StringFormat.GenericDefault);
			#endregion paint this item and its background in correct colours
		}
		private void SPDOlb_MouseDown(object sender, MouseEventArgs e)
		{
			GroupBox gb;
			if(sender is GroupBox)
			{
				gb = (GroupBox) sender;
			}
			else
			{
				ListBox lb = (ListBox) sender;
				gb = (GroupBox) lb.Parent;
			}
			foreach(COBObject COB in this.sysInfo.COBsInSystem)
			{
				if(COB.messageType == COBIDType.PDO)
				{
					if(COB == (COBObject) gb.Tag)
					{
						this.hideUserControls();
						this.activateCOB(COB);
						this.showUserControls();
						break;
					}
				}
			}
		}
		#endregion SPDO Expansion Panels ListBoxes Event hanlders
		#region VPDO Context menu Itme sevent handlers
		private void miRemoveIntMapping_Click(object sender, EventArgs e)
		{
			if(this.activeVPDOType == SevconObjectType.DIGITAL_SIGNAL_OUT)
			{
                writeNewInternalMapping(new PDOMapping(SCCorpStyle.dummyValue_DigOP, "not mapped"));
			}
			else if(this.activeVPDOType == SevconObjectType.ANALOGUE_SIGNAL_OUT)
			{
                writeNewInternalMapping(new PDOMapping(SCCorpStyle.dummyValue_AlgOP, "not mapped"));
			}

			else if(this.activeVPDOType == SevconObjectType.DIGITAL_SIGNAL_IN)
			{
                writeNewInternalMapping(new PDOMapping(SCCorpStyle.dummyValue_DigIP, "not mapped"));
			}
			else if(this.activeVPDOType == SevconObjectType.ANALOGUE_SIGNAL_IN)
			{
                writeNewInternalMapping(new PDOMapping(SCCorpStyle.dummyValue_AlgIP, "not mapped")); ; 
			}
			else if(this.activeVPDOType == SevconObjectType.MOTOR_DRIVE)
			{
                writeNewInternalMapping(new PDOMapping(SCCorpStyle.dummyValue_Motor, "not mapped")); ; 
			}
		}
	
		private void writeNewInternalMapping(PDOMapping mapping)
		{
            if (VPDOListBoxItemIndex < 0)
            {
                return;
            }
            int CANNodeIndex = this.getSevconMasterCANNodeIndex();
            DIFeedbackCode feedback;
            if (this.activeVPDOType == SevconObjectType.DIGITAL_SIGNAL_OUT)
            {
                #region digital outputs
                #region get map Index to be modified
                int mapIndex = this.VPDOListBoxItemIndex;//this.SevconMasterNode.intPDOMaps.digOPMaps.IndexOf(activeIntPDO);
                if (mapIndex < 0) //B&B
                {
                    return;
                }
                #endregion get map Index to be modified
                //re-assign the active mapping - we will have lost the pointer reference
                #region write to DI
                //add one to mapIndex - zero is num enabled items
                ODItemData digOutMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_DIG_OUT_MAPPING, mapIndex + 1);
                if (digOutMapSub != null) //the mapping sub exists
                {
                    feedback = this.SevconMasterNode.writeODValue(digOutMapSub, mapping.mapValue);
                    if (feedback != DIFeedbackCode.DISuccess)
                    {
                        #region get what is actually mapped in and display that
                        feedback = this.SevconMasterNode.readODValue(digOutMapSub); //read what the mapping actually contains
                        mapping.mapValue = digOutMapSub.currentValue; //reset to value read from device
                        //get the name of the object that is really mapped in if we can 
                        MAIN_WINDOW.appendErrorInfo = false;
                        ODItemData actualMappedItem = this.SevconMasterNode.getODSub((int)mapping.mapValue, 0);
                        MAIN_WINDOW.appendErrorInfo = true;
                        if (actualMappedItem != null)
                        {
                            mapping.mapName = actualMappedItem.parameterName;
                        }
                        else
                        { //B&B
                            mapping.mapName = "Invalid mapping 0x" + mapping.mapValue.ToString("X").PadLeft(4, '0');
                        }
                        #endregion get what is actually mapped in and display that
                    }
                    #region update groupBox and associated bindings
                    GroupBox gb = (GroupBox)this.VPDO_GBs[(int)IntPDOType.DIG_OPS];
                    ListBox lb = (ListBox)gb.Controls[0];
                    SevconMasterNode.intPDOMaps.digOPMaps.Remove(activeIntPDO);
                    SevconMasterNode.intPDOMaps.digOPMaps.Insert(mapIndex, mapping);
                    if (lb.DataSource == null)
                    {
                        BindingManagerBase managerLB = lb.BindingContext[this.SevconMasterNode.intPDOMaps.digOPMaps, "mapName"]; //set the manager 
                        lb.DataSource = this.SevconMasterNode.intPDOMaps.digOPMaps;
                        lb.DisplayMember = "mapName";
                        lb.ValueMember = "mapValue";
                    }
                    CurrencyManager m_cm = (CurrencyManager)lb.BindingContext[this.SevconMasterNode.intPDOMaps.digOPMaps];
                    m_cm.Refresh();
                    #endregion update groupBox and associated bindings
                    AddInternalTxPDOReferencesToSystemPDOs();
                    //we have to do this again - we will have lost the pointers
                    this.activateIntPDO((PDOMapping)SevconMasterNode.intPDOMaps.digOPMaps[mapIndex], lb);
                }
                #endregion write to DI
                #endregion digital outputs
            }
            else if (this.activeVPDOType == SevconObjectType.ANALOGUE_SIGNAL_OUT)
            {
                #region get map Index to be modified
                int mapIndex = this.VPDOListBoxItemIndex;//this.SevconMasterNode.intPDOMaps.algOPMaps.IndexOf(activeIntPDO);
                if (mapIndex < 0) //B&B
                {
                    return;
                }
                #endregion get map Index to be modified
                //re-assign the active mapping - we will have lost the pointer reference
                #region write to DI
                //add one to mapIndex - zero is num enabled items
                ODItemData algOutMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_ALG_OUT_MAPPING, mapIndex + 1);
                if (algOutMapSub != null)
                {
                    feedback = this.SevconMasterNode.writeODValue(algOutMapSub, mapping.mapValue);
                    if (feedback != DIFeedbackCode.DISuccess)
                    {
                        #region get what is actually mapped in and display that
                        feedback = this.SevconMasterNode.readODValue(algOutMapSub);
                        mapping.mapValue = algOutMapSub.currentValue;
                        //get the name of the object that is really mapped in if we can 
                        MAIN_WINDOW.appendErrorInfo = false;
                        ODItemData actualMappedItem = this.SevconMasterNode.getODSub((int)mapping.mapValue, 0);
                        MAIN_WINDOW.appendErrorInfo = true;
                        if (actualMappedItem != null)
                        {
                            mapping.mapName = actualMappedItem.parameterName;
                        }
                        else
                        { //B&B
                            mapping.mapName = "Invalid mapping 0x" + mapping.mapValue.ToString("X").PadLeft(4, '0');
                        }
                        #endregion get what is actually mapped in and display that
                    }
                    #region update groupBox and associated bindings
                    GroupBox gb = (GroupBox)this.VPDO_GBs[(int)IntPDOType.ALG_OPs];
                    ListBox lb = (ListBox)gb.Controls[0];
                    SevconMasterNode.intPDOMaps.algOPMaps.Remove(activeIntPDO);
                    SevconMasterNode.intPDOMaps.algOPMaps.Insert(mapIndex, mapping);
                    if (lb.DataSource == null)
                    {
                        BindingManagerBase managerLB = lb.BindingContext[this.SevconMasterNode.intPDOMaps.algOPMaps, "mapName"]; //set the manager 
                        lb.DataSource = this.SevconMasterNode.intPDOMaps.algOPMaps;
                        lb.DisplayMember = "mapName";
                        lb.ValueMember = "mapValue";
                    }
                    CurrencyManager m_cm = (CurrencyManager)lb.BindingContext[this.SevconMasterNode.intPDOMaps.algOPMaps];
                    m_cm.Refresh();
                    #endregion update groupBox and associated bindings
                    AddInternalTxPDOReferencesToSystemPDOs();
                    //we have to do this again - we will have lost the pointer reference
                    this.activateIntPDO((PDOMapping)this.SevconMasterNode.intPDOMaps.algOPMaps[mapIndex], lb);
                }
                #endregion write to DI
            }
            else if (this.activeVPDOType == SevconObjectType.MOTOR_DRIVE)
            {
                #region get map Index to be modified
                int mapIndex = this.VPDOListBoxItemIndex;//this.SevconMasterNode.intPDOMaps.MotorMaps.IndexOf(activeIntPDO);
                if (mapIndex < 0) //B&B
                {
                    return;
                }
                #endregion get map Index to be modified
                //re-assign the active mapping - we will have lost the pointer reference
                #region write to DI
                //add one to mapIndex - zero is num enabled items
                ODItemData motorMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_MOTOR_MAPPING, mapIndex + 1);
                if (motorMapSub != null)
                {
                    feedback = this.SevconMasterNode.writeODValue(motorMapSub, mapping.mapValue);
                    if (feedback != DIFeedbackCode.DISuccess)
                    {
                        #region get what is actually mapped in and display that
                        feedback = this.SevconMasterNode.readODValue(motorMapSub);
                        mapping.mapValue = motorMapSub.currentValue;
                        //get the name of the object that is really mapped in if we can 
                        MAIN_WINDOW.appendErrorInfo = false; //just checking if this one exists
                        ODItemData actualMappedItem = this.SevconMasterNode.getODSub((int)mapping.mapValue, 0);
                        MAIN_WINDOW.appendErrorInfo = true;
                        if (actualMappedItem != null)
                        {
                            mapping.mapName = actualMappedItem.parameterName;
                        }
                        else
                        { //B&B
                            mapping.mapName = "Invalid mapping 0x" + mapping.mapValue.ToString("X").PadLeft(4, '0');
                        }
                        #endregion get what is actually mapped in and display that
                    }
                    #region update groupBox and associated bindings
                    GroupBox gb = (GroupBox)this.VPDO_GBs[(int)IntPDOType.MOTOR];
                    ListBox lb = (ListBox)gb.Controls[0];
                    SevconMasterNode.intPDOMaps.MotorMaps.Remove(activeIntPDO);
                    SevconMasterNode.intPDOMaps.MotorMaps.Insert(mapIndex, mapping);
                    if (lb.DataSource == null)
                    {
                        BindingManagerBase managerLB = lb.BindingContext[this.SevconMasterNode.intPDOMaps.MotorMaps, "mapName"]; //set the manager 
                        lb.DataSource = this.SevconMasterNode.intPDOMaps.MotorMaps;
                        lb.DisplayMember = "mapName";
                        lb.ValueMember = "mapValue";
                    }
                    CurrencyManager m_cm = (CurrencyManager)lb.BindingContext[this.SevconMasterNode.intPDOMaps.MotorMaps];
                    m_cm.Refresh();
                    #endregion update groupBox and associated bindings
                    AddInternalTxPDOReferencesToSystemPDOs();
                    //we have to do this again - we will have lost the pointer reference
                    this.activateIntPDO((PDOMapping)this.SevconMasterNode.intPDOMaps.MotorMaps[mapIndex], lb);
                }
                #endregion write to DI
            }
            else if (this.activeVPDOType == SevconObjectType.DIGITAL_SIGNAL_IN)
            {
                #region get map Index to be modified
                int mapIndex = this.VPDOListBoxItemIndex;//this.SevconMasterNode.intPDOMaps.digIPMaps.IndexOf(activeIntPDO);
                if (mapIndex < 0) //B&B
                {
                    return;
                }
                #endregion get map Index to be modified
                //re-assign the active mapping - we will have lost the pointer reference
                #region write to DI
                //add one to mapIndex - zero is num enabled items
                ODItemData digInMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_DIG_IN_MAPPING, mapIndex + 1);
                if (digInMapSub != null)
                {
                    feedback = this.SevconMasterNode.writeODValue(digInMapSub, mapping.mapValue);
                    if (feedback != DIFeedbackCode.DISuccess)
                    {
                        #region get what is actually mapped in and display that
                        feedback = this.SevconMasterNode.readODValue(digInMapSub);
                        mapping.mapValue = digInMapSub.currentValue;
                        MAIN_WINDOW.appendErrorInfo = false; //just checking if this one exists
                        ODItemData actualMappedItem = this.SevconMasterNode.getODSub((int)mapping.mapValue, 0);
                        MAIN_WINDOW.appendErrorInfo = true;
                        if (actualMappedItem != null)
                        {
                            mapping.mapName = actualMappedItem.parameterName;
                        }
                        else
                        { //B&B
                            mapping.mapName = "Invalid mapping 0x" + mapping.mapValue.ToString("X").PadLeft(4, '0');
                        }
                        #endregion get what is actually mapped in and display that
                    }
                }
                #region update groupBox and associated bindings
                GroupBox gb = (GroupBox)this.VPDO_GBs[(int)IntPDOType.DIG_IPs];
                ListBox lb = (ListBox)gb.Controls[0];
                SevconMasterNode.intPDOMaps.digIPMaps.Remove(activeIntPDO);
                SevconMasterNode.intPDOMaps.digIPMaps.Insert(mapIndex, mapping);
                if (lb.DataSource == null)
                {
                    BindingManagerBase managerLB = lb.BindingContext[this.SevconMasterNode.intPDOMaps.digIPMaps, "mapName"]; //set the manager 
                    lb.DataSource = this.SevconMasterNode.intPDOMaps.digIPMaps;
                    lb.DisplayMember = "mapName";
                    lb.ValueMember = "mapValue";
                }
                CurrencyManager m_cm = (CurrencyManager)lb.BindingContext[this.SevconMasterNode.intPDOMaps.digIPMaps];
                m_cm.Refresh();
                #endregion update groupBox and associated bindings
                AddInternalRxPDOReferencesToSystemPDOs();
                //we have to do this again - we will have lost the pointer reference
                this.activateIntPDO((PDOMapping)this.SevconMasterNode.intPDOMaps.digIPMaps[mapIndex], lb);
                #endregion write to DI
            }
            else if (this.activeVPDOType == SevconObjectType.ANALOGUE_SIGNAL_IN)
            {
                #region get map Index to be modified
                int mapIndex = this.VPDOListBoxItemIndex;//this.SevconMasterNode.intPDOMaps.algIPMaps.IndexOf(activeIntPDO);
                if (mapIndex < 0)
                {
                    return; //is not here
                }
                #endregion get map Index to be modified
                //re-assign the active mapping - we will have lost the pointer reference
                #region write to DI
                //add one to mapIndex - zero is num enabled items
                ODItemData algInMapSub = SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_ALG_IN_MAPPING, mapIndex + 1);
                if (algInMapSub != null)
                {
                    feedback = this.SevconMasterNode.writeODValue(algInMapSub, mapping.mapValue);
                    if (feedback != DIFeedbackCode.DISuccess)
                    {
                        #region get what is actually mapped in and display that
                        feedback = this.SevconMasterNode.readODValue(algInMapSub);
                        mapping.mapValue = algInMapSub.currentValue;
                        MAIN_WINDOW.appendErrorInfo = false; //just checking if this one exists
                        ODItemData actualMappedItem = this.SevconMasterNode.getODSub((int)mapping.mapValue, 0);
                        MAIN_WINDOW.appendErrorInfo = true;
                        if (actualMappedItem != null)
                        {
                            mapping.mapName = actualMappedItem.parameterName;
                        }
                        else
                        { //B&B
                            mapping.mapName = "Invalid mapping 0x" + mapping.mapValue.ToString("X").PadLeft(4, '0');
                        }
                        #endregion get what is actually mapped in and display that
                    }
                    #region update groupBox and associated bindings
                    SevconMasterNode.intPDOMaps.algIPMaps.Remove(activeIntPDO);
                    SevconMasterNode.intPDOMaps.algIPMaps.Insert(mapIndex, mapping);
                    GroupBox gb = (GroupBox)this.VPDO_GBs[(int)IntPDOType.ALG_IPs];
                    ListBox lb = (ListBox)gb.Controls[0];
                    if (lb.DataSource == null)
                    {
                        BindingManagerBase managerLB = lb.BindingContext[this.SevconMasterNode.intPDOMaps.algIPMaps, "mapName"]; //set the manager 
                        lb.DataSource = this.SevconMasterNode.intPDOMaps.algIPMaps;
                        lb.DisplayMember = "mapName";
                        lb.ValueMember = "mapValue";
                    }
                    CurrencyManager m_cm = (CurrencyManager)lb.BindingContext[this.SevconMasterNode.intPDOMaps.algIPMaps];
                    m_cm.Refresh();  //sorts ofurt the list box
                    #endregion update groupBox and associated bindings
                    AddInternalRxPDOReferencesToSystemPDOs();
                    //we have to do this again - we will have lost the pointer reference
                    this.activateIntPDO((PDOMapping)this.SevconMasterNode.intPDOMaps.algIPMaps[mapIndex], lb);
                }
                #endregion write to DI
            }
        }
		private void MIenableDisableIntPDOs_Click(object sender, EventArgs e)
		{
			if(VPDOListBoxItemIndex<1)
			{
				return;
			}
			DIFeedbackCode feedback;
			MenuItem mi = (MenuItem) sender;
			int newNumEnabled = VPDOListBoxItemIndex + 1;
			this.hideUserControls();
			int CANIndexOfSevconMaster  = this.getSevconMasterCANNodeIndex();
			if(this.activeVPDOType == SevconObjectType.DIGITAL_SIGNAL_OUT)
			{
				#region digital outputs
				#region determine reqested num of enabled mappings
				if(mi.Text.ToUpper().IndexOf("ENABLE") != -1)
				{
					this.SevconMasterNode.intPDOMaps.numEnabledDigOPMaps = VPDOListBoxItemIndex +1;
				}
				else
				{
					this.SevconMasterNode.intPDOMaps.numEnabledDigOPMaps = VPDOListBoxItemIndex;
				}
				#endregion determine reqested num of enabled mappings
				#region write to DI
				ODItemData digOutMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_DIG_OUT_MAPPING, 0);
				if(digOutMapSub != null)
				{
					feedback = this.SevconMasterNode.writeODValue(digOutMapSub, (long) SevconMasterNode.intPDOMaps.numEnabledDigOPMaps);
					if(feedback != DIFeedbackCode.DISuccess)
					{
						feedback = this.SevconMasterNode.readODValue(digOutMapSub);//read the vlaue
						if(feedback == DIFeedbackCode.DISuccess)
						{
							this.SevconMasterNode.intPDOMaps.numEnabledDigOPMaps = (int) digOutMapSub.currentValue;
							GroupBox gb = (GroupBox)this.VPDO_GBs[(int) IntPDOType.DIG_OPS];
							ListBox lb = (ListBox) gb.Controls[0];
							if((SevconMasterNode.intPDOMaps.numEnabledDigOPMaps >0) && (mi.Text.ToUpper().IndexOf("ENABLE") != -1))
							{
								this.activateIntPDO((PDOMapping) lb.Items[this.SevconMasterNode.intPDOMaps.numEnabledDigOPMaps-1], lb);
							}
							else
							{
								this.deActivateInternalPDOs();
								lb.Invalidate();
							}
						}
					}
				}
				#endregion write to DI
				#endregion digital outputs
			}
			else if(this.activeVPDOType == SevconObjectType.ANALOGUE_SIGNAL_OUT)
			{
				#region analogue outputs
				#region determine reqested num of enabled mappings
				if(mi.Text.ToUpper().IndexOf("ENABLE") != -1)
				{
					SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps = VPDOListBoxItemIndex +1;
				}
				else
				{
					SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps = VPDOListBoxItemIndex;
				}
				#endregion determine reqested num of enabled mappings
				#region write to DI
				ODItemData algOutMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_ALG_OUT_MAPPING, 0);
				if(algOutMapSub != null)
				{
					feedback = this.SevconMasterNode.writeODValue(algOutMapSub, (long) SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps);
					if(feedback != DIFeedbackCode.DISuccess)
					{
						feedback = this.SevconMasterNode.readODValue(algOutMapSub); //read the vlaue form device
						if(feedback == DIFeedbackCode.DISuccess)
						{
							this.SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps = (int) algOutMapSub.currentValue;
							GroupBox gb = (GroupBox)this.VPDO_GBs[(int) IntPDOType.ALG_OPs];
							ListBox lb = (ListBox) gb.Controls[0];
							if((SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps >0)&& (mi.Text.ToUpper().IndexOf("ENABLE") != -1))
							{
								this.activateIntPDO((PDOMapping) lb.Items[this.SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps-1], lb);
							}
							else
							{
								this.deActivateInternalPDOs();
								lb.Invalidate();
							}

						}
					}
				}
				#endregion write to DI
				#endregion analogue outputs
			}
			else if(this.activeVPDOType == SevconObjectType.MOTOR_DRIVE)
			{
				#region motor
				if(newNumEnabled>this.SevconMasterNode.intPDOMaps.numEnabledMotorMaps)
				{
					#region do single source check
					foreach(PDOMapping mapToBeEnabled in this.SevconMasterNode.intPDOMaps.MotorMaps)
					{
						#region only check the ones the user wants to set enabled
						int mapIndex = this.SevconMasterNode.intPDOMaps.MotorMaps.IndexOf(mapToBeEnabled);
						if((mapIndex<this.SevconMasterNode.intPDOMaps.numEnabledMotorMaps)
							|| (mapToBeEnabled.mapValue == SCCorpStyle.dummyValue_Motor))
						{
							continue; //no need to check the existing enabled ones
						}
						if(mapIndex >= newNumEnabled)
						{
							break; //these ones are to remain disabled - no need to check them
						}
						#endregion only check the ones the user wants to set enabled
						#region check for conflict with existing enabled VPDO
						foreach(PDOMapping enabledMap in this.SevconMasterNode.intPDOMaps.MotorMaps)
						{
							int enabledMapIndex = this.SevconMasterNode.intPDOMaps.MotorMaps.IndexOf(enabledMap);
							if(enabledMapIndex>=newNumEnabled-1)
							{
								break; //we checked them all and found no single source conflicts
							}
							if((mapToBeEnabled.mapValue == enabledMap.mapValue) 
								&& (enabledMapIndex != mapIndex))
							{
								this.statusBarPanel3.Text = mapToBeEnabled.mapName + " is already mapped to Motor Map " + (mapIndex+1).ToString();
								this.showUserControls();
								return;
							}
						}
						#endregion check for conflict with existing enabled VPDO
						#region check for conflict with an SPDO
						//detemine the equivlainnt SPDO map for this odITem
						MAIN_WINDOW.appendErrorInfo = false;
						ODItemData odSub = this.SevconMasterNode.getODSub((int) mapToBeEnabled.mapValue, 0);
						MAIN_WINDOW.appendErrorInfo = true;
						long equivSPDOMap = this.convertODItemToMappingValue(odSub);
						foreach(COBObject COB in this.sysInfo.COBsInSystem)
						{
							if ((COB.messageType == COBIDType.PDO) && (COB.receiveNodes.Count>0))
							{
								COBObject.PDOMapData rxData = (COBObject.PDOMapData) COB.receiveNodes[0];
								if(rxData.nodeID == this.SevconMasterNode.nodeID)
								{
									foreach(PDOMapping existingMap in rxData.SPDOMaps)
									{
										if( existingMap.mapValue == equivSPDOMap)
										{
											this.statusBarPanel3.Text = mapToBeEnabled.mapName + " is already mapped in SPDO: " + COB.name;
											this.showUserControls();
											return;
										}
									}
								}
							}
						}
						#endregion check for conflict with an SPDO
					}
					#endregion do single source check
				}
				#region determine reqested num of enabled mappings
				if(mi.Text.ToUpper().IndexOf("ENABLE") != -1)
				{
					SevconMasterNode.intPDOMaps.numEnabledMotorMaps = VPDOListBoxItemIndex +1;
				}
				else
				{
					SevconMasterNode.intPDOMaps.numEnabledMotorMaps = VPDOListBoxItemIndex;
				}
				#endregion determine reqested num of enabled mappings
				#region write to DI
				ODItemData motorMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_MOTOR_MAPPING, 0);
				if(motorMapSub != null)
				{
					feedback = this.SevconMasterNode.writeODValue(motorMapSub,  (long) SevconMasterNode.intPDOMaps.numEnabledMotorMaps);
					if(feedback != DIFeedbackCode.DISuccess)
					{
						feedback = this.SevconMasterNode.readODValue(motorMapSub);//read the vlaue form device
						if(feedback == DIFeedbackCode.DISuccess)
						{
							this.SevconMasterNode.intPDOMaps.numEnabledMotorMaps = (int) motorMapSub.currentValue;
							GroupBox gb = (GroupBox)this.VPDO_GBs[(int) IntPDOType.MOTOR];
							ListBox lb = (ListBox) gb.Controls[0];
							if((SevconMasterNode.intPDOMaps.numEnabledMotorMaps >0) && (mi.Text.ToUpper().IndexOf("ENABLE") != -1))
							{
								this.activateIntPDO((PDOMapping) lb.Items[this.SevconMasterNode.intPDOMaps.numEnabledMotorMaps-1], lb);
							}
							else
							{
								this.deActivateInternalPDOs();
								lb.Invalidate();
							}
						}
					}
				}
				#endregion write to DI
				#endregion motor
			}
			else if(this.activeVPDOType == SevconObjectType.DIGITAL_SIGNAL_IN)
			{
				if(newNumEnabled>this.SevconMasterNode.intPDOMaps.numEnabledDigIPMaps)
				{
					#region do single source check
					foreach(PDOMapping mapToBeEnabled in this.SevconMasterNode.intPDOMaps.digIPMaps)
					{
						#region only check the ones the user wants to set enabled
						int mapIndex = this.SevconMasterNode.intPDOMaps.digIPMaps.IndexOf(mapToBeEnabled);
						if((mapIndex<this.SevconMasterNode.intPDOMaps.numEnabledDigIPMaps)
							||(mapToBeEnabled.mapValue == SCCorpStyle.dummyValue_DigIP))
						{
							continue; //no need to check the existing enabled ones
						}
						if(mapIndex >= newNumEnabled)
						{
							break; //these ones are to remain disabled - no need to check them
						}
						#endregion only check the ones the user wants to set enabled
						#region check for conflict with existing enabled VPDO
						foreach(PDOMapping enabledMap in this.SevconMasterNode.intPDOMaps.digIPMaps)
						{
							int enabledMapIndex = this.SevconMasterNode.intPDOMaps.digIPMaps.IndexOf(enabledMap);
							if(enabledMapIndex>=newNumEnabled-1) 
							{
								break; //we checked them all and found no single source conflicts
							}
							if((mapToBeEnabled.mapValue == enabledMap.mapValue)  
								&& (mapIndex != enabledMapIndex))  //do not compare to itself
							{
								this.statusBarPanel3.Text = mapToBeEnabled.mapName + " is already mapped to VPDO digital Input " + (mapIndex +1).ToString();
								this.showUserControls();
								return;
							}
						}
						#endregion check for conflict with existing enabled VPDO
						#region check for conflict with an SPDO
						//detemine the equivlainnt SPDO map for this odITem
						MAIN_WINDOW.appendErrorInfo = false;
						ODItemData odSub = this.SevconMasterNode.getODSub((int) mapToBeEnabled.mapValue, 0);
						MAIN_WINDOW.appendErrorInfo = true;
						long equivSPDOMap = this.convertODItemToMappingValue(odSub);
						foreach(COBObject COB in this.sysInfo.COBsInSystem)
						{
							if ((COB.messageType == COBIDType.PDO) && (COB.receiveNodes.Count>0))
							{
								COBObject.PDOMapData rxData = (COBObject.PDOMapData) COB.receiveNodes[0];
								if(rxData.nodeID == this.SevconMasterNode.nodeID)
								{
									foreach(PDOMapping existingMap in rxData.SPDOMaps)
									{
										if( existingMap.mapValue == equivSPDOMap)
										{
											this.statusBarPanel3.Text = mapToBeEnabled.mapName + " is already mapped to SPDO: " + COB.name;
											this.showUserControls();
											return;
										}
									}
								}
							}
						}
						#endregion check for conflict with an SPDO
					}
					#endregion do single source check
				}
				#region digital inputs
				#region we passed the siongle source check so determine reqested num of enabled mappings
				if(mi.Text.ToUpper().IndexOf("ENABLE") != -1)
				{
					SevconMasterNode.intPDOMaps.numEnabledDigIPMaps = VPDOListBoxItemIndex +1;
				}
				else
				{
					SevconMasterNode.intPDOMaps.numEnabledDigIPMaps = VPDOListBoxItemIndex;
				}
				#endregion determine reqested num of enabled mappings
				#region write to DI
				ODItemData digInMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_DIG_IN_MAPPING, 0);
				if(digInMapSub != null)
				{
					feedback = this.SevconMasterNode.writeODValue(digInMapSub, (long) SevconMasterNode.intPDOMaps.numEnabledDigIPMaps);
					if(feedback != DIFeedbackCode.DISuccess)
					{
						feedback = this.SevconMasterNode.readODValue(digInMapSub);
						if(feedback == DIFeedbackCode.DISuccess)
						{
							this.SevconMasterNode.intPDOMaps.numEnabledDigIPMaps = (int) digInMapSub.currentValue;
							GroupBox gb = (GroupBox)this.VPDO_GBs[(int) IntPDOType.DIG_IPs];
							ListBox lb = (ListBox) gb.Controls[0];
							if((SevconMasterNode.intPDOMaps.numEnabledDigIPMaps >0) && (mi.Text.ToUpper().IndexOf("ENABLE") != -1))
							{
								this.activateIntPDO((PDOMapping) lb.Items[this.SevconMasterNode.intPDOMaps.numEnabledDigIPMaps-1], lb);
							}
							else
							{
								this.deActivateInternalPDOs();
								lb.Invalidate();
							}
						}
					}
				}
				#endregion write to DI
				#endregion digital inputs
			}
			else if(this.activeVPDOType == SevconObjectType.ANALOGUE_SIGNAL_IN)
			{
				if(newNumEnabled>this.SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps)
				{
					#region check for single source
					foreach(PDOMapping mapToBeEnabled in this.SevconMasterNode.intPDOMaps.algIPMaps)
					{
						#region only check the ones the user wants to set enabled
						int mapIndex = this.SevconMasterNode.intPDOMaps.algIPMaps.IndexOf(mapToBeEnabled);
						if((mapIndex<this.SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps)
							|| (mapToBeEnabled.mapValue == SCCorpStyle.dummyValue_AlgIP))
						{
							continue; //no need to check the existing enabled ones
						}
						if(mapIndex >= newNumEnabled)
						{
							break; //these ones are to remain disabled - no need to check them
						}
						#endregion only check the ones the user wants to set enabled
						#region check for conflict with existing enabled VPDO
						foreach(PDOMapping enabledMap in this.SevconMasterNode.intPDOMaps.algIPMaps)
						{
							int existingMapIndex = this.SevconMasterNode.intPDOMaps.algIPMaps.IndexOf(enabledMap);
							if(existingMapIndex>=newNumEnabled - 1)
							{
								break; //we checked them all and found no single source conflicts
							}
							if((mapToBeEnabled.mapValue == enabledMap.mapValue) 
								&& (existingMapIndex != mapIndex))
							{
								this.statusBarPanel3.Text = mapToBeEnabled.mapName + " is already mapped to VPDO analogue input " + (mapIndex +1).ToString();
								this.showUserControls();
								return;
							}
						}
						#endregion check for conflict with existing enabled VPDO
						#region check for conflict with an SPDO
						//detemine the equivlainnt SPDO map for this odITem
						MAIN_WINDOW.appendErrorInfo = false;
						ODItemData odSub = this.SevconMasterNode.getODSub((int) mapToBeEnabled.mapValue, 0);
						MAIN_WINDOW.appendErrorInfo = true;
						long equivSPDOMap = this.convertODItemToMappingValue(odSub);
						foreach(COBObject COB in this.sysInfo.COBsInSystem)
						{
							if ((COB.messageType == COBIDType.PDO) && (COB.receiveNodes.Count>0))
							{
								COBObject.PDOMapData rxData = (COBObject.PDOMapData) COB.receiveNodes[0];
								if(rxData.nodeID == this.SevconMasterNode.nodeID)
								{
									foreach(PDOMapping existingMap in rxData.SPDOMaps)
									{
										if( existingMap.mapValue == equivSPDOMap)
										{
											this.statusBarPanel3.Text = mapToBeEnabled.mapName + " is already mapped to SPDO: " + COB.name;
											this.showUserControls();
											return;
										}
									}
								}
							}
						}
						#endregion check for conflict with an SPDO
					}
					#endregion check for single source
				}
				#region analogue inputs
				#region determine reqested num of enabled mappings
				if(mi.Text.ToUpper().IndexOf("ENABLE") != -1)
				{
					SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps = VPDOListBoxItemIndex +1;
				}
				else
				{
					SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps = VPDOListBoxItemIndex;
				}
				#endregion determine reqested num of enabled mappings
				#region write to DI
				ODItemData algInMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_ALG_IN_MAPPING, 0);
				if(algInMapSub != null)
				{
					feedback = this.SevconMasterNode.writeODValue(algInMapSub, (long) SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps);
					if(feedback != DIFeedbackCode.DISuccess)
					{
						feedback = SevconMasterNode.readODValue(algInMapSub);
						if(feedback == DIFeedbackCode.DISuccess)
						{
							this.SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps = (int) algInMapSub.currentValue;
						}
					}
					GroupBox gb = (GroupBox)this.VPDO_GBs[(int) IntPDOType.ALG_IPs];
					ListBox lb = (ListBox) gb.Controls[0];
					if((SevconMasterNode.intPDOMaps.numEnabledMotorMaps >0) && (mi.Text.ToUpper().IndexOf("ENABLE") != -1))
					{
						this.activateIntPDO((PDOMapping) lb.Items[this.SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps-1], lb);
					}
					else
					{
						this.deActivateInternalPDOs();
						lb.Invalidate();
					}
				}
				#endregion write to DI
				#endregion analogue inputs
			}
			this.showUserControls();
		}
		#endregion VPDO Context menu Itme sevent handlers
		#region VPDO ListBoxes event handlers
		private void VPDOlb_DragOver(object sender, DragEventArgs e)
		{
			TreeNode draggedNode;
			treeNodeTag nodeTag;
			ListBox lb = (ListBox) sender;
			e.Effect =  DragDropEffects.None;
			try
			{
				draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
				nodeTag = (treeNodeTag) draggedNode.Tag;
			}
			catch
			{
				return;
			}
			Point mousePt = new Point(e.X, e.Y);
			Rectangle mouseRect = new Rectangle(mousePt, new Size(5,5));
			for (int i = 0;i<lb.Items.Count;i++)
			{
				Rectangle itemRect = lb.GetItemRectangle(i);
				if(itemRect.IntersectsWith(mouseRect) == true)
				{
					this.VPDOListBoxItemIndex = i;
				}
			}
			
			if((lb.DataSource == this.SevconMasterNode.intPDOMaps.digOPMaps)
				&& (this.SevconMasterNode.intPDOMaps.digOPMaps.Count>0) //needed to prevent binding exception if this has zero members
				&& (this.activeVPDOType == SevconObjectType.DIGITAL_SIGNAL_OUT))
			{
				//no need to check for single source on Outputs
				e.Effect =  DragDropEffects.Copy;
			}
			else if((lb.DataSource == this.SevconMasterNode.intPDOMaps.algOPMaps)
				&& (this.activeVPDOType == SevconObjectType.ANALOGUE_SIGNAL_OUT))
			{
				//no need to check for single source on Outputs
				e.Effect =  DragDropEffects.Copy;
			}
			else if((lb.DataSource == this.SevconMasterNode.intPDOMaps.MotorMaps)
				&& (this.activeVPDOType == SevconObjectType.MOTOR_DRIVE))
			{
				#region check for single source
				foreach(PDOMapping map in this.SevconMasterNode.intPDOMaps.MotorMaps)
				{
					#region check if we have reached the disabled ones
					int mapIndex = this.SevconMasterNode.intPDOMaps.MotorMaps.IndexOf(map);
					if(mapIndex >= SevconMasterNode.intPDOMaps.numEnabledMotorMaps)
					{
						break; //only check the enabled ones 
					}
					#endregion check if we have reached the disabled ones
					if(nodeTag.assocSub.indexNumber == map.mapValue)
					{
						this.statusBarPanel3.Text = nodeTag.assocSub.parameterName + " is already mapped to an enabled VPDO";
						return;
					}
				}
				//detemine the equivlainnt SPDO map for this odITem
				long equivSPDOMap = this.convertODItemToMappingValue(nodeTag.assocSub);
				foreach(COBObject COB in this.sysInfo.COBsInSystem)
				{
					if ((COB.messageType == COBIDType.PDO) && (COB.receiveNodes.Count>0))
					{
						COBObject.PDOMapData rxData = (COBObject.PDOMapData) COB.receiveNodes[0];
						if(rxData.nodeID == this.SevconMasterNode.nodeID)
						{
							foreach(PDOMapping map in rxData.SPDOMaps)
							{
								if( map.mapValue == equivSPDOMap)
								{
									this.statusBarPanel3.Text = nodeTag.assocSub.parameterName + " is already mapped to a SPDO";
									return;
								}
							}
						}
					}
				}
				#endregion check for single source
				e.Effect =  DragDropEffects.Copy;
			}
			else if((lb.DataSource == this.SevconMasterNode.intPDOMaps.digIPMaps)
				&& (this.activeVPDOType == SevconObjectType.DIGITAL_SIGNAL_IN))
			{
				#region check for single source
				foreach(PDOMapping map in this.SevconMasterNode.intPDOMaps.digIPMaps)
				{
					#region check if we have reached the disabled ones
					int mapIndex = this.SevconMasterNode.intPDOMaps.digIPMaps.IndexOf(map);
					if(mapIndex >= SevconMasterNode.intPDOMaps.numEnabledDigIPMaps)
					{
						break; //only check the enabled ones 
					}
					#endregion check if we have reached the disabled ones
					if(nodeTag.assocSub.indexNumber == map.mapValue)
					{
						this.statusBarPanel3.Text = nodeTag.assocSub.parameterName + " is already mapped to an enabled VPDO";
						return;
					}
				}
				//detemine the equivlainnt SPDO map for this odITem
				long equivSPDOMap = this.convertODItemToMappingValue(nodeTag.assocSub);
				foreach(COBObject COB in this.sysInfo.COBsInSystem)
				{
					if ((COB.messageType == COBIDType.PDO) && (COB.receiveNodes.Count>0))
					{
						COBObject.PDOMapData rxData = (COBObject.PDOMapData) COB.receiveNodes[0];
						if(rxData.nodeID == this.SevconMasterNode.nodeID)
						{
							foreach(PDOMapping map in rxData.SPDOMaps)
							{
								if( map.mapValue == equivSPDOMap)
								{
									this.statusBarPanel3.Text = nodeTag.assocSub.parameterName + " is already mapped to a SPDO";
									return;
								}
							}
						}
					}
				}
				#endregion check for single source
				e.Effect =  DragDropEffects.Copy;
			}
			else if((lb.DataSource == this.SevconMasterNode.intPDOMaps.algIPMaps)
				&& (this.activeVPDOType == SevconObjectType.ANALOGUE_SIGNAL_IN))
			{
				#region check for single source
				foreach(PDOMapping map in this.SevconMasterNode.intPDOMaps.algIPMaps)
				{
					#region check if we have reached the disabled ones
					int mapIndex = this.SevconMasterNode.intPDOMaps.algIPMaps.IndexOf(map);
					if(mapIndex >= SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps)
					{
						break; //only check the enabled ones 
					}
					#endregion check if we have reached the disabled ones

					if(nodeTag.assocSub.indexNumber == map.mapValue)
					{
						this.statusBarPanel3.Text = nodeTag.assocSub.parameterName + " is single source signal and is already mapped to an enabled VPDO";
						return;
					}
				}
				//detemine the equivlainnt SPDO map for this odITem
				long equivSPDOMap = this.convertODItemToMappingValue(nodeTag.assocSub);
				foreach(COBObject COB in this.sysInfo.COBsInSystem)
				{
					if ((COB.messageType == COBIDType.PDO) && (COB.receiveNodes.Count>0))
					{
						COBObject.PDOMapData rxData = (COBObject.PDOMapData) COB.receiveNodes[0];
						if(rxData.nodeID == this.SevconMasterNode.nodeID)
						{
							foreach(PDOMapping map in rxData.SPDOMaps)
							{
								if( map.mapValue == equivSPDOMap)
								{
									this.statusBarPanel3.Text = nodeTag.assocSub.parameterName + " is single source signal and is already mapped to a SPDO";
									return;
								}
							}
						}
					}
				}
				#endregion check for single source
				e.Effect =  DragDropEffects.Copy;
			}
		}

		private void VPDOlb_DragDrop(object sender, DragEventArgs e)
		{
            #region extract mouse and dragged treenode data
            ListBox lb = (ListBox)sender;
            Point AdjustedmousePt = lb.PointToClient(new Point(e.X, e.Y));
            TreeNode senderTreeNode = null;
            treeNodeTag senderTag;
            try
            {
                senderTreeNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode", true);
                senderTag = (treeNodeTag)senderTreeNode.Tag;

            }
            catch
            {
                return;  //couldn't mould the data into a TreeNode
            }
            #endregion extract mouse and dragged treenode data
            int mapIndex = lb.IndexFromPoint(AdjustedmousePt.X, AdjustedmousePt.Y);
            this.VPDOListBoxItemIndex = lb.IndexFromPoint(AdjustedmousePt.X, AdjustedmousePt.Y); //judetmep -if we make this class wide the we don't need to rely on activeIntPDO
            if (lb.DataSource == this.SevconMasterNode.intPDOMaps.digOPMaps)
            {
                activateIntPDO((PDOMapping)this.SevconMasterNode.intPDOMaps.digOPMaps[mapIndex], lb); //do this first
                if (SevconMasterNode.intPDOMaps.numEnabledDigOPMaps < (mapIndex + 1))
                {  //we need to updat ethe number of enabled items
                    this.setNumEnabledIntMaps(this.SevconMasterNode.intPDOMaps.digOPMaps, mapIndex + 1);
                }
                writeNewInternalMapping(new PDOMapping(senderTag.assocSub.indexNumber, senderTag.assocSub.parameterName));
                AddInternalRxPDOReferencesToSystemPDOs(); //the update one to many ref in System PDOs
                if (SystemInfo.errorSB.Length > 0)
                {
                    SystemInfo.errorSB.Insert(0, "\n Failed to update internal digital output mapping");
                }
            }
            else if (lb.DataSource == this.SevconMasterNode.intPDOMaps.algOPMaps)
            {
                activateIntPDO((PDOMapping)this.SevconMasterNode.intPDOMaps.algOPMaps[mapIndex], lb);
                if (SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps < (mapIndex + 1))
                {  //we need to updat ethe number of enabled items
                    this.setNumEnabledIntMaps(this.SevconMasterNode.intPDOMaps.algOPMaps, mapIndex + 1);
                }
                writeNewInternalMapping(new PDOMapping((long)senderTag.assocSub.indexNumber, senderTreeNode.Text));
                AddInternalRxPDOReferencesToSystemPDOs(); //the update one to many ref in System PDOs
                if (SystemInfo.errorSB.Length > 0)
                {
                    SystemInfo.errorSB.Insert(0, "\n Failed to update internal analogue output mapping");
                }
            }
            else if (lb.DataSource == this.SevconMasterNode.intPDOMaps.MotorMaps)
            {
                activateIntPDO((PDOMapping)this.SevconMasterNode.intPDOMaps.MotorMaps[mapIndex], lb);
                if (SevconMasterNode.intPDOMaps.numEnabledMotorMaps < (mapIndex + 1))
                {  //we need to updat ethe number of enabled items
                    this.setNumEnabledIntMaps(this.SevconMasterNode.intPDOMaps.MotorMaps, mapIndex + 1);
                }
                writeNewInternalMapping(new PDOMapping(senderTag.assocSub.indexNumber, senderTag.assocSub.parameterName));
                AddInternalRxPDOReferencesToSystemPDOs(); //the update one to many ref in System PDOs
                if (SystemInfo.errorSB.Length > 0)
                {
                    SystemInfo.errorSB.Insert(0, "\n Failed to update internal motor mapping");
                }

            }
            else if (lb.DataSource == this.SevconMasterNode.intPDOMaps.digIPMaps)
            {
                activateIntPDO((PDOMapping)this.SevconMasterNode.intPDOMaps.digIPMaps[mapIndex], lb);
                if (SevconMasterNode.intPDOMaps.numEnabledDigIPMaps < (mapIndex + 1))
                {  //we need to updat ethe number of enabled items
                    this.setNumEnabledIntMaps(this.SevconMasterNode.intPDOMaps.digIPMaps, mapIndex + 1);
                }
                writeNewInternalMapping(new PDOMapping((long)senderTag.assocSub.indexNumber, senderTag.assocSub.parameterName));
                AddInternalRxPDOReferencesToSystemPDOs(); //the update one to many ref in System PDOs
                if (SystemInfo.errorSB.Length > 0)
                {
                    SystemInfo.errorSB.Insert(0, "\n Failed to update internal digital input mapping");
                }
            }
            else if (lb.DataSource == this.SevconMasterNode.intPDOMaps.algIPMaps)
            {
                activateIntPDO((PDOMapping)this.SevconMasterNode.intPDOMaps.algIPMaps[mapIndex], lb);
                if (SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps < (mapIndex + 1))
                {  //we need to updat ethe number of enabled items
                    this.setNumEnabledIntMaps(this.SevconMasterNode.intPDOMaps.algIPMaps, mapIndex + 1);
                }
                writeNewInternalMapping(new PDOMapping(senderTag.assocSub.indexNumber, senderTag.assocSub.parameterName));
                AddInternalRxPDOReferencesToSystemPDOs(); //the update one to many ref in System PDOs
                if (SystemInfo.errorSB.Length > 0)
                {
                    SystemInfo.errorSB.Insert(0, "\n Failed to update internal analogue input mapping");
                }
            }
            if (SystemInfo.errorSB.Length > 0)
            {
                this.statusBarPanel3.Text = "Failed to change internal PDO mapping";
                sysInfo.displayErrorFeedbackToUser("");
            }
        }
        private void setNumEnabledIntMaps(ArrayList mapsList, int newNumEnabled)
        {
            DIFeedbackCode feedback;
            if (mapsList == this.SevconMasterNode.intPDOMaps.digOPMaps)
            {
                #region digital Ouptuts
                ODItemData digOutMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_DIG_OUT_MAPPING, 0);
                if (digOutMapSub != null)
                {
                    feedback = SevconMasterNode.writeODValue(digOutMapSub, newNumEnabled);
                    if (feedback == DIFeedbackCode.DISuccess)
                    {
                        this.SevconMasterNode.intPDOMaps.numEnabledDigOPMaps = newNumEnabled;
                    }
                    else
                    {//read back value form device

                        feedback = SevconMasterNode.readODValue(digOutMapSub);
                        if (feedback == DIFeedbackCode.DISuccess)
                        {
                            this.SevconMasterNode.intPDOMaps.numEnabledDigOPMaps = (int)digOutMapSub.currentValue;
                        }
                    }
                }
                #endregion digital Ouptuts
            }
            else if (mapsList == this.SevconMasterNode.intPDOMaps.algOPMaps)
            {
                #region analogue outputs
                ODItemData algOutMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_ALG_OUT_MAPPING, 0);
                if (algOutMapSub != null)
                {
                    feedback = this.SevconMasterNode.writeODValue(algOutMapSub, newNumEnabled);
                    if (feedback == DIFeedbackCode.DISuccess)
                    {
                        this.SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps = newNumEnabled;
                    }
                    else
                    {
                        feedback = this.SevconMasterNode.readODValue(algOutMapSub);
                        if (feedback == DIFeedbackCode.DISuccess)
                        {
                            this.SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps = (int)algOutMapSub.currentValue;
                        }
                    }
                }
                #endregion analogue outputs
            }
            else if (mapsList == this.SevconMasterNode.intPDOMaps.MotorMaps)
            {
                #region motor
                ODItemData motorMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_MOTOR_MAPPING, 0);
                if (motorMapSub != null)
                {
                    feedback = SevconMasterNode.writeODValue(motorMapSub, newNumEnabled);
                    if (feedback == DIFeedbackCode.DISuccess)
                    {
                        this.SevconMasterNode.intPDOMaps.numEnabledMotorMaps = newNumEnabled;
                    }
                    else
                    {
                        feedback = this.SevconMasterNode.readODValue(motorMapSub);
                        if (feedback == DIFeedbackCode.DISuccess)
                        {
                            this.SevconMasterNode.intPDOMaps.numEnabledMotorMaps = (int)motorMapSub.currentValue;
                        }
                    }
                }
                #endregion motor
            }
            else if (mapsList == this.SevconMasterNode.intPDOMaps.digIPMaps)
            {
                #region digital inputs
                ODItemData digInMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_DIG_IN_MAPPING, 0);
                if (digInMapSub != null)
                {
                    feedback = this.SevconMasterNode.writeODValue(digInMapSub, newNumEnabled);
                    if (feedback == DIFeedbackCode.DISuccess)
                    {
                        this.SevconMasterNode.intPDOMaps.numEnabledDigIPMaps = newNumEnabled;
                    }
                    else
                    {//read back value form device

                        feedback = this.SevconMasterNode.readODValue(digInMapSub);
                        if (feedback == DIFeedbackCode.DISuccess)
                        {
                            this.SevconMasterNode.intPDOMaps.numEnabledDigIPMaps = (int)digInMapSub.currentValue;
                        }
                    }
                }
                #endregion digital inputs
            }
            else if (mapsList == this.SevconMasterNode.intPDOMaps.algIPMaps)
            {
                #region analogue inputs
                ODItemData algInMapSub = this.SevconMasterNode.getODSubFromObjectType(SevconObjectType.LOCAL_ALG_IN_MAPPING, 0);
                if (algInMapSub != null)
                {
                    feedback = this.SevconMasterNode.writeODValue(algInMapSub, newNumEnabled);
                    if (feedback == DIFeedbackCode.DISuccess)
                    {
                        this.SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps = newNumEnabled;
                    }
                    else
                    {//read back value form device
                        feedback = SevconMasterNode.readODValue(algInMapSub);
                        if (feedback == DIFeedbackCode.DISuccess)
                        {
                            this.SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps = (int)algInMapSub.currentValue;
                        }
                    }
                }
                #endregion analogue inputs
            }
		}
		private void activateIntPDO(PDOMapping newActIntPDO, ListBox lb)
		{
			this.activeIntPDO = newActIntPDO;
			SysPDOsToHighLight.Clear(); //now do the one to many referencing for this active cob
			if(lb.DataSource == this.SevconMasterNode.intPDOMaps.digOPMaps)
			{
				#region digital Outputs
				foreach(COBObject COB in this.sysInfo.COBsInSystem)
				{
					if((COB.messageType == COBIDType.PDO) && (COB.assocDigOPs.Contains(newActIntPDO) == true))
					{ //we have found a SystemPDO that contains this internal dig i/P mapping
						if(SysPDOsToHighLight.Contains(COB) == false)
						{ 
							SysPDOsToHighLight.Add(COB);//add once only
						}
					}
				}
				#endregion digital Outputs
			}
			else if(lb.DataSource == this.SevconMasterNode.intPDOMaps.algOPMaps)
			{
				#region analogue outputs
				foreach(COBObject COB in this.sysInfo.COBsInSystem)
				{
					if((COB.messageType == COBIDType.PDO) && (COB.assocAlgOPs.Contains(newActIntPDO) == true))
					{ 
						if(SysPDOsToHighLight.Contains(COB) == false)
						{ 
							SysPDOsToHighLight.Add(COB);//add once only
							break;
						}
					}
				}
				#endregion analogue outputs
			}
			else if(lb.DataSource == this.SevconMasterNode.intPDOMaps.MotorMaps)
			{
				#region motor
				foreach(COBObject COB in this.sysInfo.COBsInSystem)
				{
					if((COB.messageType == COBIDType.PDO) && (COB.assocMotor.Contains(newActIntPDO) == true))
					{ //we have found a SystemPDO that contains this internal dig i/P mapping
						if(SysPDOsToHighLight.Contains(COB) == false)
						{ 
							SysPDOsToHighLight.Add(COB);//add once only
						}
					}
				}
				#endregion motor
			}
			else if(lb.DataSource == this.SevconMasterNode.intPDOMaps.digIPMaps)
			{
				#region digital inputs
				foreach(COBObject COB in this.sysInfo.COBsInSystem)
				{
					if((COB.messageType == COBIDType.PDO) && (COB.assocDigIPs.Contains(newActIntPDO) == true))
					{ //we have found a SystemPDO that contains this internal dig i/P mapping
						if(SysPDOsToHighLight.Contains(COB) == false)
						{ 
							SysPDOsToHighLight.Add(COB);//add once only
						}
					}
				}
				#endregion digital inputs
			}
			else if(lb.DataSource == this.SevconMasterNode.intPDOMaps.algIPMaps)
			{
				#region analogue inputs
				foreach(COBObject COB in this.sysInfo.COBsInSystem)
				{
					if((COB.messageType == COBIDType.PDO) && (COB.assocAlgIPs.Contains(newActIntPDO) == true))
					{ //we have found a SystemPDO that contains this internal dig i/P mapping
						if(SysPDOsToHighLight.Contains(COB) == false)
						{ 
							SysPDOsToHighLight.Add(COB);//add once only
						}
					}
				}
				#endregion analogue inputs
			}

            if (SysPDOsToHighLight.Count > 0)
            {//this current COB becomes the first added associated SystemPDO
                activateCOB((COBObject)SysPDOsToHighLight[0]);
            }
            else //no associated SystemPDOs to highlight
            {
                activateCOB((COBObject)this.sysInfo.COBsInSystem[0]);
            }
            lb.Invalidate();
		}
		private void VPDOlb_Resize(object sender, EventArgs e)
		{
			ListBox lb = (ListBox) sender;
			lb.BeginUpdate();
			Graphics tempG = lb.CreateGraphics();
			int maxWidth = 0;
			if(lb.DataSource != null)  //will be null if there are no entries
			{
				ArrayList al = (ArrayList) lb.DataSource;
				foreach( PDOMapping intMap in al)
				{
					SizeF size = tempG.MeasureString(intMap.mapName,lb.Font);
					maxWidth = Math.Max(maxWidth, (int)size.Width);
				}
			}
			if(maxWidth > lb.Width)
			{
				lb.HorizontalScrollbar = true;
			}
			else  
			{
				lb.HorizontalScrollbar = false;
			}
			lb.EndUpdate();
			lb.EndUpdate();
		}

		private void VPDOlb_DrawItem(object sender, DrawItemEventArgs e)
		{
			ListBox lb = (ListBox) sender;
			if(lb.DataSource == null)
			{
				return;
			}
			Brush myBrush = Brushes.Black;
			Brush myBackBrush = Brushes.White;
			#region check if this item should be marked as disabled
			if(
                ((SevconMasterNode != null) && (SevconMasterNode.intPDOMaps != null))
              &&
                (
				((lb.DataSource == SevconMasterNode.intPDOMaps.digOPMaps) && (e.Index>=SevconMasterNode.intPDOMaps.numEnabledDigOPMaps ))
				|| ((lb.DataSource == SevconMasterNode.intPDOMaps.algOPMaps) && (e.Index>=SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps ))
				|| ((lb.DataSource == this.SevconMasterNode.intPDOMaps.MotorMaps) && (e.Index>=this.SevconMasterNode.intPDOMaps.numEnabledMotorMaps))
				|| ((lb.DataSource == this.SevconMasterNode.intPDOMaps.digIPMaps) && (e.Index>=this.SevconMasterNode.intPDOMaps.numEnabledDigIPMaps ))
				|| ((lb.DataSource == this.SevconMasterNode.intPDOMaps.algIPMaps) && (e.Index>=this.SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps ))
				)
               )
			{
				myBrush = Brushes.LightGray;
				myBackBrush = Brushes.WhiteSmoke;
			}
			#endregion  check if this item should be marked as disabled
			#region now check whether this itme should be marked as currently active
			if(this.activeIntPDO != null)
			{
				if(this.activeIntPDO == (PDOMapping) lb.Items[e.Index])
				{
					myBrush = Brushes.Red;
				}
			}
			else //could be 'active' becuase  it it mapped into the curren tSystem PDO
			{
				foreach(PDOMapping intMap in this.currCOB.assocDigOPs)
				{
					if( (PDOMapping)lb.Items[e.Index] == intMap)
					{
						myBrush = Brushes.Red;
					}
				}
				foreach(PDOMapping intMap in this.currCOB.assocAlgOPs)
				{
					if( (PDOMapping)lb.Items[e.Index] == intMap)
					{
						myBrush = Brushes.Red;
					}
				}
				foreach(PDOMapping intMap in this.currCOB.assocMotor)
				{
					if( (PDOMapping)lb.Items[e.Index] == intMap)
					{
						myBrush = Brushes.Red;
					}
				}
				foreach(PDOMapping intMap in this.currCOB.assocDigIPs)
				{
					if( (PDOMapping)lb.Items[e.Index] == intMap)
					{
						myBrush = Brushes.Red;
					}
				}
				foreach(PDOMapping intMap in this.currCOB.assocAlgIPs)
				{
					if( (PDOMapping)lb.Items[e.Index] == intMap)
					{
						myBrush = Brushes.Red;
					}
				}
			}
			#endregion now check whether this itme should be marked as currently active
			#region paint this item and its background in correct colours
			string itemText = ((PDOMapping)((ArrayList)lb.DataSource)[e.Index]).mapName;
			e.Graphics.FillRectangle(myBackBrush, e.Bounds);
			e.Graphics.DrawString(itemText, e.Font, myBrush,e.Bounds,StringFormat.GenericDefault);
			#endregion paint this item and its background in correct colours
		}

		private void VPDOlb_MouseDown(object sender, MouseEventArgs e)
		{
			ListBox lb = (ListBox) sender;
			VPDOListBoxItemIndex = lb.IndexFromPoint(e.X, e.Y); 
			if(VPDOListBoxItemIndex<0)
			{
				return;
			}
			ContextMenu CM_ListBox = new ContextMenu();
			MenuItem MIenableDisableIntPDOs = null;
			MenuItem miRemoveIntMapping =null;
			MenuItem miExit;
			if(e.Button ==MouseButtons.Right)
			{
				#region context menu
				miExit = new MenuItem("Exit Communications set up");
				miExit.Click +=new EventHandler(COBSetupExit_Click);
				CM_ListBox.MenuItems.Add(miExit);
				miRemoveIntMapping = new MenuItem("Remove this mapping");
				miRemoveIntMapping.Click +=new EventHandler(miRemoveIntMapping_Click); 
				MIenableDisableIntPDOs = new MenuItem();
				MIenableDisableIntPDOs.Click +=new EventHandler(MIenableDisableIntPDOs_Click);
				#endregion context menu
			}
			#region digital OPs
			if(lb.DataSource == this.SevconMasterNode.intPDOMaps.digOPMaps)
			{
				if((e.Button ==MouseButtons.Left)  
					&& (VPDOListBoxItemIndex<this.SevconMasterNode.intPDOMaps.numEnabledDigOPMaps))
				{
					this.activateIntPDO((PDOMapping) lb.Items[VPDOListBoxItemIndex], lb);
				}
				if(this.activeVPDOType != SevconObjectType.DIGITAL_SIGNAL_OUT)
				{
					#region show only VPDO DO items
					this.activeVPDOType = SevconObjectType.DIGITAL_SIGNAL_OUT;
					this.showVPDOTreeNodes();
					#endregion show only VPDO DO items
				}
				if(e.Button == MouseButtons.Right)
				{ 
					#region setup and display the Internal PDO specific Context menu items
					if(this.SevconMasterNode.intPDOMaps.numEnabledDigOPMaps<=VPDOListBoxItemIndex)
					{
						MIenableDisableIntPDOs.Text = "Enable internal PDOs upto to dig o/p " + (VPDOListBoxItemIndex + 1).ToString();
					}
					else 
					{
						MIenableDisableIntPDOs.Text = "Disable internal PDOs from dig o/p input " + (VPDOListBoxItemIndex + 1).ToString();
					}
					#endregion setup and display the Context menu
				}
			}
				#endregion digital OPs
				#region analogue OPs
			else if(lb.DataSource == this.SevconMasterNode.intPDOMaps.algOPMaps)
			{
				if((e.Button ==MouseButtons.Left)  
					&& (VPDOListBoxItemIndex<this.SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps))
				{
					this.activateIntPDO((PDOMapping) lb.Items[VPDOListBoxItemIndex], lb);
				}
				#region User Clicked on a Analogue O/P routing line
				if(this.activeVPDOType != SevconObjectType.ANALOGUE_SIGNAL_OUT)
				{
					#region show only VPDO AO items
					this.activeVPDOType = SevconObjectType.ANALOGUE_SIGNAL_OUT;
					this.showVPDOTreeNodes();
					#endregion show only VPDO AO items
				}
				if(e.Button == MouseButtons.Right)
				{ 
					#region setup and display the Internal PDO specific Context menu items
					if(this.SevconMasterNode.intPDOMaps.numEnabledAlgOPMaps <=VPDOListBoxItemIndex)
					{
						MIenableDisableIntPDOs.Text = "Enable internal PDOs upto to alg o/p " + (VPDOListBoxItemIndex + 1).ToString();
					}
					else 
					{
						MIenableDisableIntPDOs.Text = "Disable internal PDOs from alg o/p " + (VPDOListBoxItemIndex + 1).ToString();
					}
					#endregion setup and display the Context menu
				}
				#endregion User Clicked on a Analogue O/P routing line
			}
				#endregion analogue OPs
				#region motor
			else if(lb.DataSource == this.SevconMasterNode.intPDOMaps.MotorMaps)
			{
				#region motor
				if((e.Button ==MouseButtons.Left)  
					&& (VPDOListBoxItemIndex<this.SevconMasterNode.intPDOMaps.numEnabledMotorMaps))
				{
					this.activateIntPDO((PDOMapping) lb.Items[VPDOListBoxItemIndex], lb);
				}
				if(this.activeVPDOType != SevconObjectType.MOTOR_DRIVE)
				{
					#region show only VPDO motor items
					this.activeVPDOType = SevconObjectType.MOTOR_DRIVE;
					this.showVPDOTreeNodes();
					#endregion show only VPDO motor items
				}
				if(e.Button == MouseButtons.Right)
				{ 
					#region setup and display the Internal PDO specific Context menu items
					if(this.SevconMasterNode.intPDOMaps.numEnabledMotorMaps<=VPDOListBoxItemIndex)
					{
						MIenableDisableIntPDOs.Text = "Enable internal PDOs upto to motor " + (VPDOListBoxItemIndex + 1).ToString();
					}
					else 
					{
						MIenableDisableIntPDOs.Text = "Disable internal PDOs from motor " + (VPDOListBoxItemIndex + 1).ToString();
					}
					#endregion setup and display the Context menu
				}
				#endregion motor
			}
				#endregion motor
				#region dig IPs
			else if(lb.DataSource == this.SevconMasterNode.intPDOMaps.digIPMaps)
			{
				if((e.Button ==MouseButtons.Left)  
					&& (VPDOListBoxItemIndex<this.SevconMasterNode.intPDOMaps.numEnabledDigIPMaps))
				{
					this.activateIntPDO((PDOMapping) lb.Items[VPDOListBoxItemIndex], lb);
				}
				if(this.activeVPDOType != SevconObjectType.DIGITAL_SIGNAL_IN)
				{
					#region show only VPDO DI items
					this.activeVPDOType = SevconObjectType.DIGITAL_SIGNAL_IN;
					this.showVPDOTreeNodes();
					#endregion show only VPDO DI items
				}
				if(e.Button == MouseButtons.Right)
				{ 
					#region setup and display the Internal PDO specific Context menu items
					if((this.SevconMasterNode.intPDOMaps.numEnabledDigIPMaps<=VPDOListBoxItemIndex))
					{
						MIenableDisableIntPDOs.Text = "Enable internal PDOs upto to dig i/p " + (VPDOListBoxItemIndex + 1).ToString();
					}
					else 
					{
						MIenableDisableIntPDOs.Text = "Disable internal PDOs from dig i/p " + (VPDOListBoxItemIndex + 1).ToString();
					}
					#endregion setup and display the Context menu
				}
			}
				#endregion dig IPs
				#region alg IPs
			else if(lb.DataSource == this.SevconMasterNode.intPDOMaps.algIPMaps)
			{
				#region User Clicked on an Analogue I/P routing line
				if((e.Button ==MouseButtons.Left)  
					&& (VPDOListBoxItemIndex<this.SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps))
				{
					this.activateIntPDO((PDOMapping) lb.Items[VPDOListBoxItemIndex], lb);
				}
				if(this.activeVPDOType != SevconObjectType.ANALOGUE_SIGNAL_IN)
				{
					#region show only VPDO AI items
					this.activeVPDOType = SevconObjectType.ANALOGUE_SIGNAL_IN;
					this.showVPDOTreeNodes();
					#endregion show only VPDO AI items
				}
				if(e.Button == MouseButtons.Right)
				{ 
					#region setup and display the Internal PDO specific Context menu items
					if(this.SevconMasterNode.intPDOMaps.numEnabledAlgIPMaps <=VPDOListBoxItemIndex)
					{
						MIenableDisableIntPDOs.Text = "Enable internal PDOs upto to alg i/p " + (VPDOListBoxItemIndex + 1).ToString();
					}
					else 
					{
						MIenableDisableIntPDOs.Text = "Disable internal PDOs from alg i/p " + (VPDOListBoxItemIndex + 1).ToString();
					}
					#endregion setup and display the Context menu
				}
				#endregion User Clicked on an Analogue I/P routing line
			}
			#endregion alg IPs
			if(e.Button == MouseButtons.Right)
			{
				#region context menu
				CM_ListBox.MenuItems.Add(miRemoveIntMapping);
				CM_ListBox.MenuItems.Add(MIenableDisableIntPDOs);
				CM_ListBox.Show(lb, new Point(e.X, e.Y));
				#endregion context menu
			}
		}
		#endregion VPDO List obxes event handlers
		#region Exiting PDO screen clean up methods
		private void COBSetupExit_Click(object sender, EventArgs e)
		{
			exitPDOScreen();
		}
		private void exitPDOScreen()
		{
			this.hideUserControls();
			for(int i = 0;i<this.sysInfo.nodes.Length;i++)
			{
				if(this.sysInfo.nodes[i].EVASRequired == true)
				{
					this.statusBarPanel3.Text = "Requesting EEPROM Save for Communications profile on Node ID " +this.sysInfo.nodes[i].nodeID.ToString(); 
					this.sysInfo.nodes[i].saveCommunicationParameters();  
					this.sysInfo.nodes[i].EVASRequired = false;
				}
			}
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("EEPROM save command Failed");
			}
			this.showReadOnlyNodes();
			if(wasHidingNodesBeforePDOScreenActive == true)
			{
				this.hideReadOnlyTreeNodes();
			}
			this.SystemPDOsScreenActive = false;
			this.treeView1.SelectedNode = this.SystemStatusTN;
			switchOffCOBPDOEventHandlers();

            #region reset search if SearchForm open before exiting PDO screen
            //Required since treeview1 may change when swapping back from PDO setup
            //to the main screen.
            if (SEARCH_FRM != null)
            {
                ((SearchForm)SEARCH_FRM).resetSearchInstance();
            }
            #endregion reset search if SearchForm open before exiting PDO screen

			this.showUserControls();
		}
		#endregion Exiting PDO screen clean up methods
		#region minor and generic methods
		private long convertODItemToMappingValue(ODItemData item)
		{
			StringBuilder mappingStr = new StringBuilder();
			mappingStr.Append(item.indexNumber.ToString("X").PadLeft(4,'0'));
			//if this item has been bit split then we must ensure we use the real sub number not the pseudo one
            if (item.format == SevconNumberFormat.BIT_SPLIT)
            {
                int splitSub = 0; // pretend it's sub 0 instead of DW usage of -1
                mappingStr.Append(splitSub.ToString("X").PadLeft(2, '0'));                
            }
            else
            {
                mappingStr.Append(item.subNumber.ToString("X").PadLeft(2, '0'));
            }
			mappingStr.Append(item.dataSizeInBits.ToString("X").PadLeft(2, '0'));
			long mapVal = System.Convert.ToInt64(mappingStr.ToString(), 16);
			return mapVal;
		}
		private int getSevconMasterCANNodeIndex()
		{
			for(int CANNodeIndex = 0;CANNodeIndex<this.sysInfo.nodes.Length;CANNodeIndex++)
			{//have to do it this way becasue - nodes is and Array not and ArrayList
				if(this.sysInfo.nodes[CANNodeIndex] == this.SevconMasterNode)
				{
					return CANNodeIndex;
				}
			}
			return -1;
		}
		private int getCANNodeIndexOfCANNode(nodeInfo node)
		{
			for(int CANNodeIndex = 0;CANNodeIndex<this.sysInfo.nodes.Length;CANNodeIndex++)
			{//have to do it this way becasue - nodes is and Array not and ArrayList
				if(this.sysInfo.nodes[CANNodeIndex] == node)
				{
					return CANNodeIndex;
				}
			}
			return -1;
		}
		private void createUniqueNameAndCOIDForAddedPDO(string suggestedName)
		{
			#region create a unique name for this new PDO

			
			int PDOnum = 1;
			bool uniqueNameFound = false;
			while (uniqueNameFound == false)  //forever loop 
			{
				#region on first loop - see if we cna use just the suggested name
				if(PDOnum == 1)
				{
					uniqueNameFound = true; //
					foreach(COBObject COB in this.sysInfo.COBsInSystem)
					{
						if(COB.name == suggestedName)
						{
							uniqueNameFound = false;
							break;  //this name has already beed used
						}
					}
					if(uniqueNameFound == true)
					{
						PDOUnderConstruction.name = suggestedName;
						break; //get out now
					}
				}
				#endregion on first loop - see if we cna use just the suggested name
				#region otherwise append PDOnum and see if this is unique
				#region compare suggested name to every COB name that has already been used
				uniqueNameFound = true; //
				//foreach(string cobname in this.CB_SPDOName.Items)
				foreach(COBObject COB in this.sysInfo.COBsInSystem)
				{
					if(COB.name == suggestedName + PDOnum.ToString()) //uniquename identified
					{
						uniqueNameFound = false;
						break;  //this name has already beed used
					}
				}
				#endregion compare suggested name to every COB name that has already been used
				#endregion otherwise append PDOnum and see if this is unique
				if(uniqueNameFound == true)
				{
					PDOUnderConstruction.name = suggestedName + PDOnum.ToString();
				}
				else
				{
					PDOnum++;
				}
			}
			#endregion create a unique name for this new PDO
			#region get a free COBID for this new PDO
			int [] unusedCOBIDs = new int[1]; //we only need one here
			this.sysInfo.getUnusedCOBIDs(COBIDPriority.low, unusedCOBIDs);
			PDOUnderConstruction.COBID = unusedCOBIDs[0];
			PDOUnderConstruction.requestedCOBID = unusedCOBIDs[0];
			PDOUnderConstruction.messageType = COBIDType.PDO;
			#endregion get a free COBID for this new PDO
		}
		#endregion minor and generic methods
		#endregion System PDO rleated methods
#endif
		#region monitoring store related methods 
		#region saving monitoring store
		#endregion saving monitoring store
		private void updateMonitorStoreContextMenu()
		{
			contextMenu1.MenuItems.Clear();
			if(this.DATA_MONITOR_FRM == null)  //for now once graphing form is open for user to use this
			{
				//'offline' options
				#region Open Old Monitor List
				MenuItem openMI = new MenuItem("Fill Monitoring store from file");
				openMI.Click +=new EventHandler(openStoredGraphListMI_Click);
				contextMenu1.MenuItems.Add(openMI);
				#endregion Open old monitor List

				//options that require at least one item in the monitoring store
				if(this.GraphCustList_TN.Nodes.Count>0)
				{
					#region delete items from Monstore menu option
					if(this.treeView1.SelectedNode.Parent != null)
					{
						MenuItem deleteMI = new MenuItem("Remove this node/branch");
						deleteMI.Click +=new EventHandler(BranchDeleteMI_Click);
						contextMenu1.MenuItems.Add(deleteMI);
					}
					else
					{
						MenuItem deleteMI = new MenuItem("Empty the Monitoring store");
						deleteMI.Click +=new EventHandler(BranchDeleteMI_Click);
						contextMenu1.MenuItems.Add(deleteMI);
					}
					#endregion delete items from DCf menu option
					#region Save monitor List to File
					MenuItem saveMI = new MenuItem("Save Monitoring store to file");
					saveMI.Click +=new EventHandler(saveGraphListMI_Click);
					contextMenu1.MenuItems.Add(saveMI);
					#endregion save monitor List to file
				}
				else
				{
					if(this.monStore != null)
					{
						this.monStore.fromFile = false;
					}
				}
			}
		}

		private void openStoredGraphListMI_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.FileName = "";  //ensure that initial directory works correctly
			if ( Directory.Exists( MAIN_WINDOW.UserDirectoryPath + @"\MONITOR" ) == false )
			{
				try
				{
					Directory.CreateDirectory( MAIN_WINDOW.UserDirectoryPath + @"\MONITOR" );
				}
				catch{}
			}
			openFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\MONITOR";
			openFileDialog1.Title = "Open monitored data file on DriveWizard host";
			openFileDialog1.Filter = "Data files (*.xml)|*.xml" ;
			openFileDialog1.DefaultExt = "xml";
			openFileDialog1.ShowDialog(this);

			if(openFileDialog1.FileName != "")
			{
				//may need to thread this later
				this.dataMonitoringTimer.Enabled = false;
				this.hideUserControls();
                //Clear out any previous monitor store stuff from tree node & monStore
                BranchDeleteMI_Click(sender, e);
				this.monStore = new myMonitorStore();
					ObjectXMLSerializer xmlSerializer = new ObjectXMLSerializer();

                //read in new monStore stuff from file
				try
				{
					monStore = (myMonitorStore) xmlSerializer.Load(this.monStore, openFileDialog1.FileName);
				}
				catch(Exception ex)
				{
					SystemInfo.errorSB.Append("\nException seen: " + ex.Message); //TODO - handle this better
					this.showUserControls();
					return;
				}

                //alter to ensure loop repeated to find match for each node in myMonNodes
				this.monStore.existsInCurrentSystem = false;
                nodeInfo matchingNode = null;
                foreach (nodeInfo mnNode in this.monStore.myMonNodes)
                {
                    mnNode.isInCurrentSystem = false;

                    foreach (nodeInfo connectedNode in this.sysInfo.nodes)
                    {
                        if ((connectedNode.nodeID == mnNode.nodeID)
                            && (connectedNode.vendorID == mnNode.vendorID)
                            && (connectedNode.productCode == mnNode.productCode)
                            && (connectedNode.revisionNumber == mnNode.revisionNumber)
                            && (mnNode.findMatchingEDSFile() == DIFeedbackCode.DISuccess)
                            && (mnNode.findMatchingXMLFile() == DIFeedbackCode.DISuccess))
                        {
                            mnNode.isInCurrentSystem = true;
                            this.monStore.existsInCurrentSystem = true; //set the top level flag - for graphing context menu
                            mnNode.XMLfilepath = connectedNode.XMLfilepath;  //we should mathc up th exml on the curren tusrs system - not the system the monitoing store was stored on 
                            matchingNode = connectedNode;
                            break; // match found
                        }
                    }

                    //do this here it will vary form graph save to reconnection
                    mnNode.nodeOrTableIndex = MAIN_WINDOW.GraphTblIndex;

                    foreach (ObjDictItem odItem in mnNode.objectDictionary)
                    {
                        foreach (ODItemData odSub in odItem.odItemSubs)
                        {
                            if (mnNode.isInCurrentSystem == true)
                            {
                                odSub.CANnode = matchingNode;
                            }
                            //Keep Yaxis.Max from file (offline nodes at time of monitoring subsequently mess up scaling)
                            //if ((odSub.subNumber != -1) && (odSub.isNumItems == false) && (odSub.screendataPoints.Count == 0))
                            //{ //no data points so we need to apply a max and min
                            //    this.monStore.graph.Yaxis.Max = Math.Max(odSub.highLimit, this.monStore.graph.Yaxis.Max);
                            //}
                            // note whether this file has data (to enable plot from file menu option)
                            if ((odSub.subNumber != -1) && (odSub.isNumItems == false) && (odSub.screendataPoints.Count > 0))
                            {
                                monStore.dataInFile = true;
                            }

                            // Find the objectName equivalent to objectNameString from the sevconObjectIDList
                            // as not automatically populated by serialized XML file.
                            if ((odSub.objectNameString != "") && (odSub.objectNameString != "NONE"))
                            {
                                odSub.objectName = sysInfo.DCFnode.EDS_DCF.readSevconObjectType(odSub.objectNameString.ToUpper());
                            }

                            // Find the sectionType equivalent to sectionTypeString from the sevconSectionIDList
                            // as not automatically populated by serialized XML file.
                            if ((odSub.sectionTypeString != "") && (odSub.sectionTypeString != "NONE"))
                            {
                                odSub.sectionType = sysInfo.DCFnode.EDS_DCF.readSectionType(odSub.sectionTypeString.ToUpper());
                            }
                        }

                        //don't call until after sectionType and objectName are derived
                        fillCustListTreeNodesFromFile();
                    }
                }
   
				if(	this.monStore.graph.Xaxis.Max == 0)
				{
					this.monStore.graph.Xaxis.Max = 300;
					this.monStore.graph.Xaxis.DivValue = 20;
				}
				this.monStore.graph.Yaxis.DivValue = this.monStore.graph.Yaxis.Max/this.monStore.graph.Yaxis.DivValue;
				
				currentTreenode = this.GraphCustList_TN;
				if(currentTreenode.Nodes.Count>0)
				{
					currentTreenode = currentTreenode.Nodes[0];
				}
				this.treeView1.SelectedNode = currentTreenode;
				this.handleTreeNodeSelection(currentTreenode, false);
				if(MAIN_WINDOW.DWdataset.Tables[GraphTblIndex].Rows.Count>0)
				{
                    //only start monitoring timer if nodes from file exist on system (prevent exceptions)
                    if (this.monStore.existsInCurrentSystem == true)
                    {
                        this.dataMonitoringTimer.Enabled = true;
                    }
					this.monStore.fromFile = true;
				}
				else
				{
					this.statusBarPanel3.Text = "No monitorable items found";
					SystemInfo.errorSB.Append("\nNo corresponding OD items can be found on connected CAN nodes. \nOpen or create a new monitoring list");
                    if (SystemInfo.errorSB.Length > 0)
                    {
                        sysInfo.displayErrorFeedbackToUser("\nFailed to add monitoring items from file");
                    }
				}
			}
		}
		private void saveGraphListMI_Click(object sender, System.EventArgs e)
		{
			this.hideUserControls();
			saveFileDialog1.FileName = "";  //endsure that we opena t correct directory
			if ( Directory.Exists( MAIN_WINDOW.UserDirectoryPath + @"\MONITOR" ) == false )
			{
				try
				{
					Directory.CreateDirectory( MAIN_WINDOW.UserDirectoryPath + @"\MONITOR" );
				}
				catch{}
			}
			saveFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\MONITOR";
			saveFileDialog1.Title = "Save Monitoring data to DriveWizard host";
			saveFileDialog1.Filter = "data files (*.xml)|*.xml" ;
			saveFileDialog1.DefaultExt = "xml";
			saveFileDialog1.ShowDialog(this);	
			if(saveFileDialog1.FileName != "")
			{
				#region Monitoring List and Plot data save
				if(monStore != null)
				{
					ObjectXMLSerializer xmlSerializer = new ObjectXMLSerializer();
					try
					{
						xmlSerializer.Save(this.monStore, saveFileDialog1.FileName);
					}
					catch(Exception ex)
					{
						SystemInfo.errorSB.Append("\nEnsure that file is not open and try again \nReported error: " + ex.Message); 
						this.showUserControls();
						return;
					}
				}
				#endregion Monitoring List and Plot data save
			}
			this.showUserControls();
		}
		private void addNodeToMonitorStore_Click(object sender, EventArgs e)
		{
			this.hideUserControls();
			addItemToCustomList(this.treeView1.SelectedNode, this.GraphCustList_TN, true);
			//do NOT switch to DCF, graph node here 
			//- data retrieval thread may still be running
			nextRequiredNode = this.GraphCustList_TN;//once thread has done it will deal with it
		}

		private void removeNodeFromMonitorStore_Click(object sender, EventArgs e)
		{
			if(this.treeView1.Nodes.Contains(GraphCustList_TN) == false)
			{
				return;
			}
			string path = this.treeView1.SelectedNode.FullPath;
			//remove the top part tof path and replace with 
			int temp = path.IndexOf(this.treeView1.PathSeparator);
			if(temp != -1)
			{
				path = this.GraphCustList_TN.Text + path.Substring(temp);
				this.currentTreenode = null;
				isNodePathInTreeLeg(this.GraphCustList_TN, path);
				if(currentTreenode != null)
				{
					this.removeMonitoringListNodes(this.currentTreenode); 
					//currentTreenode is set in isNodePathInTreeLeg() 
					//to be equivalent tree node in Monitoring Store
				}
			}
		}
		private void dataGridContextMenu_MI1_Mon_Click(object sender, EventArgs e)
		{
			string menuText = ((MenuItem) sender).Text;
			this.hideUserControls();
			DataView tmpView = (DataView) this.dataGrid1.DataSource; //dig out the underlying DataView
			DataRow sourceRow =  tmpView[this.selectedDgRowInd].Row;
			if (menuText == "Stop monitoring this item")
			{
				#region find monitoringRow, source row and odSubToRemove
				DataRow monitoringRow = tmpView[this.selectedDgRowInd].Row;
				ODItemData odSubToRemove = (ODItemData) monitoringRow[(int) TblCols.odSub];
				string errorText = "Failed to remove " + odSubToRemove.parameterName;
				if(MAIN_WINDOW.currTblIndex != MAIN_WINDOW.GraphTblIndex)
				{
					#region User clicked on a CAN node table - find the equiv row in monStore
					#region search primary keys
					object[] myKeys = new object[3];
					myKeys[0] = tmpView[this.selectedDgRowInd].Row[(int)TblCols.Index].ToString();
					myKeys[1] = tmpView[this.selectedDgRowInd].Row[(int)TblCols.sub].ToString();
					myKeys[2] = tmpView[this.selectedDgRowInd].Row[(int)TblCols.NodeID].ToString();
					#endregion search primary keys
					monitoringRow = MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].Rows.Find(myKeys);
					if(monitoringRow == null)
					{
						SystemInfo.errorSB.Append("Failed to dereference row - row missing");
						return;
					}
					#endregion User clicked on a CAN node table - find the equiv row in monStore
				}
				else
				{
					#region user clicked on monstore table find equivalent source row
					#region primary keys to search for
					object[] myKeys = new object[2];
					myKeys[0] = monitoringRow[(int)TblCols.Index].ToString();
					myKeys[1] = monitoringRow[(int)TblCols.sub].ToString();
					#endregion primary keys to search for
					sourceRow = MAIN_WINDOW.DWdataset.Tables[odSubToRemove.CANnode.nodeOrTableIndex].Rows.Find(myKeys);
					if(sourceRow == null)
					{
						SystemInfo.errorSB.Append("Failed to dereference cell - row not found");
						return;
					}
					#endregion user clicked on monstore table find equivalent source row
				}
				#endregion find monitoringRow and source row and odSubToRemove
				foreach(nodeInfo mnNode in this.monStore.myMonNodes)
				{
					if(mnNode.nodeID  == odSubToRemove.CANnode.nodeID)
					{  //this is the affected CNa node in mon store
						#region remove odSub from affected node in mon store
						#region identify corresponding header and numItems sub if they exist
						MAIN_WINDOW.appendErrorInfo = false;
						ODItemData numItemsSub = null;
						ODItemData headerSub = mnNode.getODSub(odSubToRemove.indexNumber,-1);
						if(headerSub != null)
						{
							numItemsSub = mnNode.getODSub(odSubToRemove.indexNumber,0);
							if((numItemsSub != null) && (numItemsSub.isNumItems == false))
							{
								numItemsSub = null;  //we found one of this writeable num items subs - not a real one 
								headerSub = null;
							}
						}
						MAIN_WINDOW.appendErrorInfo = true;
						#endregion identify corresponding header and numImtes sub if they exist

						#region find header table row and decide whther we are removeing single sub or whole odITem (last sub for this odItem being removed)
						//check if any other non header or numITems rows for thisodITme reaim in DCF table
						DataRow headerRow = null;
						bool removewholeOdItem = true;
						foreach(DataRow otherMonRow in DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].Rows)
						{
							ODItemData otherMonodSUB = (ODItemData) otherMonRow[(int) TblCols.odSub];
							if(otherMonodSUB.CANnode.nodeID != odSubToRemove.CANnode.nodeID)
							{
								continue;
							}
							if(otherMonodSUB.subNumber == -1)
							{
								headerRow = otherMonRow;
							}
							else if((otherMonodSUB != odSubToRemove) && (otherMonodSUB != numItemsSub) && (otherMonodSUB != headerSub))
							{
								removewholeOdItem = false; //we have other subs for this odItme in DCf so leave header and numItmes subs alone
								//oh and leave the tree node in place
								break;
							}
						}
						#endregion find header table row and decide whther we are removeing single sub or whole odITem (last sub for this odItem being removed)

						#region remove subToRemove form DCF dictionary
						mnNode.removeSubFromDictionary(odSubToRemove);
						#endregion remove subToRemove form DCF dictionary
						if(removewholeOdItem == true)
						{
							#region removeing all remaining parts of the odItem
							#region remove header and numItems sub from mnNode dictionary is required
							if(headerSub != null)
							{
								mnNode.removeSubFromDictionary(headerSub);
							}
							if(numItemsSub != null)
							{
								mnNode.removeSubFromDictionary(numItemsSub);
							}
							#endregion  remove header and numItems sub from mnNode dictionary is required
                        }

							#region identify and remove assoc odItem TreeNode and any now redundant TreeNodes
							TreeNode headerTNode = getAssocODItemTreeNode(this.GraphCustList_TN, odSubToRemove.indexNumber); //get the loewst level DCF treenode for this odSub
							if (headerTNode != null)
							{
								headerTNode.Remove(); //need to remove this first for flushTreeViewDeadEnds to work
								do
								{
									nodesRemovedFlag = false;  //set false before entry
									//if we remove any nodes in this pass then it will be set to true
									removeRedundantSecionNodes(this.GraphCustList_TN, MAIN_WINDOW.GraphTblIndex);
								}
								while (nodesRemovedFlag == true);  //chekc at end set inside method
							}
							#endregion identify and remove assoc odItem TreeNode and any now redundant TreeNodes
							#region Handle if Monitoring store is now empty 
							if(this.GraphCustList_TN.Nodes.Count == 0)
							{
								this.GraphCustList_TN.Text = "Monitoring store (empty)";
								this.dataMonitoringTimer.Enabled = false;
							}
							#endregion handle if Monitoring store is empty 
							
							#region remove DCFtable header row
							if(headerRow != null)
							{
								DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].Rows.Remove(headerRow); //do last 
							}
							#endregion remove DCFtable header row
							#endregion removeing all remaining parts of the odItem
						
						#region finally remove monitoring row for subToRemove - DO LAST -we lose pointer to subToRemove when we remove the row
						DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].Rows.Remove(monitoringRow); //do last 
						#endregion finally remove monitoring row for subToRemove - DO LAST -we lose pointer to subToRemove when we remove the row
						#region mark source row as not monitored
						DataRowState currentRowState = sourceRow.RowState;
						sourceRow[(int)TblCols.Monitor] = false;
						if(currentRowState == DataRowState.Unchanged)
						{
							sourceRow.AcceptChanges();
						}
						#endregion mark source row as not monitored
						this.updateRowColoursArray();
						showUserControls();
						#region user feedback
						if(SystemInfo.errorSB.Length>0)
						{
							sysInfo.displayErrorFeedbackToUser(errorText);
						}
						#endregion user feedback
						return;
						#endregion remove odSub from affected node in mon store
					}
				}
			}
			else if(menuText == "Monitor this item")
			{
				DataRow SourceRow = tmpView[this.selectedDgRowInd].Row;  //independent of datagrid columns being displayed
				ODItemData rowAssocSub = (ODItemData) SourceRow[(int)TblCols.odSub];
				string NodeID = SourceRow[(int)TblCols.NodeID].ToString();
				#region add another row to the monitoring list
				DataRowState currentRowState = SourceRow.RowState;
				SourceRow[(int)TblCols.Monitor] = true;
				if(currentRowState == DataRowState.Unchanged)
				{
					SourceRow.AcceptChanges();
				}
				MAIN_WINDOW.colArray[selectedDgRowInd] = SCCorpStyle.dgRowSelected; 
				RefreshRow(selectedDgRowInd); // and force repaint of the row by invalidating it
				//When we take index column off the datagrid 
				//We will need non visible text box to allow upwards searchingby key
				TreeNode equivOdItemSub = null;
				if(this.treeView1.SelectedNode != null)
				{
					treeNodeTag selNodeTag = (treeNodeTag) this.treeView1.SelectedNode.Tag;
					equivOdItemSub = getAssocODItemTreeNode(this.treeView1.SelectedNode, rowAssocSub.indexNumber);
				}
				else
				{
					equivOdItemSub = getAssocODItemTreeNode(this.SystemStatusTN.Nodes[MAIN_WINDOW.currTblIndex], rowAssocSub.indexNumber );
				}
				if(equivOdItemSub != null)
				{
					int monCount = 0;
					foreach(DataRow monRow in MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].Rows)
					{
						ODItemData odSub = (ODItemData) monRow[(int) TblCols.odSub];
						if(odSub.subNumber != -1)
						{
							monCount++;
							if(monCount > MAIN_WINDOW.monitorCeiling)
							{
								this.statusBarPanel3.Text = "Only the first " + monitorCeiling.ToString() + " OD sub-items in the monitoring store will be monitoried in real time";
								break;
							}
						}
					}
					addItemToCustomList(equivOdItemSub, this.GraphCustList_TN, false);
				}
				#endregion add another row to the monitoring list
			}
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("Not all sub index settings updated:");
			}
		}

		private void removeMonitoringListNodes(TreeNode nodeToRemove)
		{
			DIFeedbackCode feedback;
			this.hideUserControls();
			TreeNode firstNode = nodeToRemove;
			treeNodeTag	selectedNodeTag = (treeNodeTag) this.treeView1.SelectedNode.Tag;
			treeNodeTag	removeNodeTag = (treeNodeTag) nodeToRemove.Tag;
			int sourceCANNodeIndex = 99;
			//right mouse click now forces selection of the node so the code ovehead her eis less and pre-assumes that
			// the node we are interested in is the treeview selectedNode
			if(nodeToRemove == this.GraphCustList_TN)
			{
				#region remove all Grapohing child nodes, untick th eosurce rows and and clear table
				for(int rowInd = 0;rowInd < MAIN_WINDOW.DWdataset.Tables[GraphTblIndex].Rows.Count;rowInd++)
				{
					#region locate the source rows  and 'untick' them
					feedback = this.sysInfo.getNodeNumber((short) MAIN_WINDOW.DWdataset.Tables[GraphTblIndex].Rows[rowInd][(int)TblCols.NodeID], out sourceCANNodeIndex);
					if(feedback == DIFeedbackCode.DISuccess)
					{
						object[] mySourceKeys = new object[2];
						mySourceKeys[0] = MAIN_WINDOW.DWdataset.Tables[GraphTblIndex].Rows[rowInd][(int)TblCols.Index].ToString();
						mySourceKeys[1] = MAIN_WINDOW.DWdataset.Tables[GraphTblIndex].Rows[rowInd][(int)TblCols.sub].ToString();
						DataRow sourceRow = MAIN_WINDOW.DWdataset.Tables[sourceCANNodeIndex].Rows.Find(mySourceKeys);
						if(sourceRow != null)
						{
							ODItemData srceOdsub = (ODItemData) sourceRow[(int) TblCols.odSub];
							#region switch off data monitoring timer and then switch off column error handling for the source table
							if(this.dataMonitoringTimer.Enabled == true)
							{
								this.dataMonitoringTimer.Enabled = false;
								this.monitorTimerWasRunning = true;
							}
							MAIN_WINDOW.DWdataset.Tables[sourceCANNodeIndex].ColumnChanged -= new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
							#endregion switch off data monitoring timer and then switch off column error handling for the source table
							DataRowState currentRowState = sourceRow.RowState;
							sourceRow[(int) TblCols.Monitor] = false;
							if(currentRowState == DataRowState.Unchanged)
							{
								sourceRow.AcceptChanges();  //prevent issues when user is submitting data
							}
							foreach(nodeInfo mnNode in this.monStore.myMonNodes)
							{
								if(mnNode.nodeID  == (short) MAIN_WINDOW.DWdataset.Tables[GraphTblIndex].Rows[rowInd][(int)TblCols.NodeID])
								{
									ODItemData monSubToGo = mnNode.getODSub(srceOdsub.indexNumber, srceOdsub.subNumber);
									if(monSubToGo != null)
									{
										mnNode.removeSubFromDictionary(monSubToGo);
										if(monSubToGo.subNumber == -1)
										{
											#region if header sub remove any numOfItemsSub that is not in table - but has to be in dictinoary
											MAIN_WINDOW.appendErrorInfo = false; //switch off error info - we are just checking 
											ODItemData numItemsSub = mnNode.getODSub(monSubToGo.indexNumber, 0);
											MAIN_WINDOW.appendErrorInfo = true; //ensure it is back on
											if(( numItemsSub != null)
												&& ((numItemsSub.accessType == ObjectAccessType.ReadOnly)
												|| (numItemsSub.accessType == ObjectAccessType.Constant)) 
												&& ((CANopenDataType) numItemsSub.dataType == CANopenDataType.UNSIGNED8))
											{ //this is a genuine num items sub we should remove it too
												mnNode.removeSubFromDictionary(numItemsSub);
											}
											#endregion if header sub remove any numOfItemsSub that is not in table - but has to be in dictinoary
										}
									}
								}
							}
							#region now switch column error handling back on and then restart data monitoring timer if required
							MAIN_WINDOW.DWdataset.Tables[sourceCANNodeIndex].ColumnChanged += new System.Data.DataColumnChangeEventHandler(table_ColumnChanged);
							if(this.monitorTimerWasRunning == true)
							{
								this.dataMonitoringTimer.Enabled = true;
								this.monitorTimerWasRunning = false;
							}
							#endregion now switch column error handling back on and then restart data monitoring timer if required
						}
					}
					#endregion locate the source rows  and 'untick' them
				}
				nodeToRemove.Nodes.Clear();
				MAIN_WINDOW.DWdataset.Tables[GraphTblIndex].Clear();
				#endregion remove all Grapohing child nodes, untick th eosurce rows and and clear table
			}
			else //child node
			{
				//find all corresponding row in graph table and remove them
				//find all corresponding rows in source table and switch moniting off
				#region handle underlying dat atables
				childTreeNodeList = new ArrayList();
				if(removeNodeTag.assocSub != null)
				{
					childTreeNodeList.Add(removeNodeTag);  //add dragged node if it has an underlying OD item
				}
				//fill it with all child nodes that have a corresponding OD item
				this.getChildODItems(nodeToRemove, false);
				for(int i= 0;i<childTreeNodeList.Count;i++)
				{
					treeNodeTag thisTag = (treeNodeTag) childTreeNodeList[i];
					foreach(nodeInfo monNode in this.monStore.myMonNodes)
					{
						if(monNode.nodeID == thisTag.nodeID)
						{
							ObjDictItem odItem = monNode.getODItemAndSubs(thisTag.assocSub.indexNumber);
							if(odItem != null)
							{
								monNode.removeODItemFromDictionary(odItem);
								if(monNode.objectDictionary.Count<= 0)
								{
									monStore.myMonNodes.Remove(monNode);
								}
							}
							break; //found it so get out
						}
					}
					ArrayList rowsToRemove = new ArrayList();
					foreach(DataRow monRow in MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].Rows)
					{
						ODItemData assocRowSub = (ODItemData) monRow[(int) TblCols.odSub];
						if((assocRowSub.CANnode.nodeID == thisTag.nodeID) 
							&&	(thisTag.assocSub.indexNumber == assocRowSub.indexNumber))
						{
							rowsToRemove.Add(monRow); //mark graphing row for removal in seperate loop - needed 
							#region locate source row and switch off minitoring flag
							object[] myDeviceKeys = new object[2];
							myDeviceKeys[0] = monRow[(int) TblCols.Index].ToString(); 
							myDeviceKeys[1] = monRow[(int) TblCols.sub].ToString();
							//we need to swtich off monitprigin on a source row
							DataRow sourceRow = MAIN_WINDOW.DWdataset.Tables[assocRowSub.CANnode.nodeOrTableIndex].Rows.Find(myDeviceKeys); 
							if(sourceRow != null)
							{
								sourceRow[(int) TblCols.Monitor] = false;
								sourceRow.AcceptChanges();
							}
							#endregion locate source row and switch off minitoring flag
						}
					}
					foreach(DataRow rowToGo in rowsToRemove)
					{
						MAIN_WINDOW.DWdataset.Tables[GraphTblIndex].Rows.Remove(rowToGo);
					}
				}
				#endregion handle underlying dat atables
				bool Complete = false;
				while(Complete == false)
				{
					//now remove the tree Node and its children plus any redundant parent nodes
					TreeNode tempNode = nodeToRemove.Parent;
					if(tempNode != null)
					{
						nodeToRemove.Remove();
						nodeToRemove = tempNode;
					}
					//either we have reached the root node or this noe is not redundant
					if((tempNode == null) || (tempNode.Nodes.Count != 0) )
					{
						Complete = true;
					}
				}
			}
			if(this.GraphCustList_TN.Nodes.Count == 0)
			{
				this.dataMonitoringTimer.Enabled = false;
				this.GraphCustList_TN.Text = "Monitoring store (empty)";
			}
			if(selectedNodeTag.tableindex<this.sysInfo.nodes.Length)
			{
				//ensure that the datgrid gets recolourd correclt if the treeview
				//selected node is under a CAN device
				this.updateRowColoursArray();
			}
			this.showUserControls();
			this.dataGrid1.Refresh();  //needed or datagrid does not update
		}
		#endregion monitoring store related methods 

		#region context menu related methods
		private void setupAnddisplayContextMenu()
		{
			treeNodeTag selNodeTag = (treeNodeTag) this.treeView1.SelectedNode.Tag;
			//Point newPoint = new Point((treeView1.SelectedNode.Bounds.Location.X + this.treeView1.SelectedNode.Bounds.Width), this.treeView1.SelectedNode.Bounds.Location.Y);
			Point newPoint = new Point((treeView1.SelectedNode.Bounds.Location.X + 10), this.treeView1.SelectedNode.Bounds.Location.Y+ 10);
			contextMenu1.MenuItems.Clear();
			this.treeView1.ContextMenu = this.contextMenu1;  //link to a context menu
			if(SystemPDOsScreenActive == true)
			{
				return;
			}
			string path = this.treeView1.SelectedNode.FullPath;
			int temp = path.IndexOf(this.treeView1.PathSeparator);
			if(selNodeTag.tableindex <this.sysInfo.nodes.Length)
			{
				#region Context menu set up for CAN node
				if((this.sysInfo.nodes[selNodeTag.tableindex].isSevconApplication() == true)
					&& ((SCCorpStyle.ProductRange)this.sysInfo.nodes[selNodeTag.tableindex].productRange != (SCCorpStyle.ProductRange.SEVCONDISPLAY))
                    && ((SCCorpStyle.ProductRange)this.sysInfo.nodes[selNodeTag.tableindex].productRange != (SCCorpStyle.ProductRange.CALIBRATOR))) //DR38000256
				{ //not yet implemented for display TODO
					#region logs reset menu item
					MenuItem resetAllLogsMI = new MenuItem("Reset All Logs for this CAN node");
					resetAllLogsMI.Click +=new EventHandler(resetAllLogsMI_Click);
					if(this.sysInfo.nodes[selNodeTag.tableindex].accessLevel<SCCorpStyle.AccLevel_ResetLogs)
					{
						resetAllLogsMI.Enabled = false;
					}
					contextMenu1.MenuItems.Add(resetAllLogsMI);
					#endregion logs reset menu item
				}
				#region ignore/record emergency messages for the node
				MenuItem MIignoreEmer;
				if(this.sysInfo.ignoreEmerFromNodeID.Contains(this.sysInfo.nodes[selNodeTag.tableindex].nodeID) == false)
				{
					MIignoreEmer = new MenuItem("Ignore emergency messages from this node");
				}
				else
				{
					MIignoreEmer = new MenuItem("Handle emergency messages from this node");
				}
				MIignoreEmer.Click +=new EventHandler(MIignoreEmer_Click);
				contextMenu1.MenuItems.Add(MIignoreEmer);
				#endregion ignore/record emergency messages for the node

				if(selNodeTag.assocSub != null)
				{
					#region Monitor Store Menu Items
					MenuItem addNodeToMonitorStore = new MenuItem("Add to the Monitoring Store");
					addNodeToMonitorStore.Click +=new EventHandler(addNodeToMonitorStore_Click);
					//we are talking sinlge Objec there - so it is ok to add monitoring MIs
					//first check whether this object is actually in the Monitring store
					this.contextMenu1.MenuItems.Add(addNodeToMonitorStore);
					#endregion Monitor Store Menu Items
					#region add/remove form DCF menu item
					if( ( this.DCFCustList_TN.Nodes.Count == 0) 
						||((this.sysInfo.DCFnode.nodeID == sysInfo.nodes[selNodeTag.tableindex].nodeID)
						&& (sysInfo.DCFnode.vendorID == sysInfo.nodes[selNodeTag.tableindex].vendorID)
						&& (sysInfo.DCFnode.productCode == sysInfo.nodes[selNodeTag.tableindex].productCode)
						&& (sysInfo.DCFnode.revisionNumber == sysInfo.nodes[selNodeTag.tableindex].revisionNumber))
						)
					{
						#region DCF store context menu items
						#region add to DCF
						MenuItem addNodeToDCF = new MenuItem("Add to the DCF store");
						addNodeToDCF.Click +=new EventHandler(addNodeToDCF_Click);
						this.contextMenu1.MenuItems.Add(addNodeToDCF);
						#endregion add to DCF
						#region remove from DCF
						//see if DCF ndoe contain a node with th esame nodepath as this one
						//remove the top part tof path and replace with 
						path = this.treeView1.SelectedNode.FullPath;
						temp = path.IndexOf(this.treeView1.PathSeparator);
						if(temp != -1)
						{
							path = this.DCFCustList_TN.Text + path.Substring(temp);
							this.currentTreenode = null;
							isNodePathInTreeLeg(this.DCFCustList_TN, path);
							if(this.currentTreenode != null)
							{
								MenuItem removeNodeFromDCF = new MenuItem("Remove from the DCF store");
								removeNodeFromDCF.Click +=new EventHandler(removeNodeFromDCF_Click);
								this.contextMenu1.MenuItems.Add(removeNodeFromDCF);
							}
						}
						#endregion Remove from DCF
						#endregion DCF store context menu items
					}
					#endregion add/remove form DCF menu item
				}
				else  //single or multiple EDS sections
				{
					#region add/ remove form DCF menu item
					//don't add anything for monitoring
					if((this.DCFCustList_TN.Nodes.Count == 0)
						|| ((this.sysInfo.DCFnode.nodeID == sysInfo.nodes[selNodeTag.tableindex].nodeID)
						&& (sysInfo.DCFnode.vendorID == sysInfo.nodes[selNodeTag.tableindex].vendorID)
						&& (sysInfo.DCFnode.productCode == sysInfo.nodes[selNodeTag.tableindex].productCode)
						&& (sysInfo.DCFnode.revisionNumber == sysInfo.nodes[selNodeTag.tableindex].revisionNumber)))
					{
						if(this.treeNodeIsDeviceLevelSpecialNode(treeView1.SelectedNode) == false)
						{
							#region DCF storeMenu items
							#region add to DCF
							MenuItem addNodeToDCF = new MenuItem("Add to the DCF store");
							addNodeToDCF.Click +=new EventHandler(addNodeToDCF_Click);
							this.contextMenu1.MenuItems.Add(addNodeToDCF);
							#endregion add to DCF
							#region Remove from DCF
							path = this.treeView1.SelectedNode.FullPath;
							temp = path.IndexOf(this.treeView1.PathSeparator);
							if(temp != -1)
							{
								path = this.DCFCustList_TN.Text + path.Substring(temp);
								this.currentTreenode = null;
								isNodePathInTreeLeg(this.DCFCustList_TN, path);
								if(this.currentTreenode != null)
								{
									MenuItem removeNodeFromDCF = new MenuItem("Remove from the DCF store");
									removeNodeFromDCF.Click +=new EventHandler(removeNodeFromDCF_Click);
									this.contextMenu1.MenuItems.Add(removeNodeFromDCF);
								}
							}
							#endregion Remove from DCF
							#endregion DCF storeMenu items
						}
					}
					#endregion add/ remove form DCF menu item
				}
				if(temp != -1)
				{
					#region remove node from monitoring store menu item
					path = this.treeView1.SelectedNode.FullPath;
					temp = path.IndexOf(this.treeView1.PathSeparator);
					path = this.GraphCustList_TN.Text + path.Substring(temp);
					this.currentTreenode = null;
					isNodePathInTreeLeg(this.GraphCustList_TN, path);
					if(this.currentTreenode != null)
					{
						MenuItem removeNodeFromMonitorStore = new MenuItem("Remove from the monitoring store");
						removeNodeFromMonitorStore.Click +=new EventHandler(removeNodeFromMonitorStore_Click);
						this.contextMenu1.MenuItems.Add(removeNodeFromMonitorStore);
					}
					#endregion remove node from monitoring store menu item
				}
				
				#region display datalog domain export
				if((SCCorpStyle.ProductRange)this.sysInfo.nodes[selNodeTag.tableindex].productRange == (SCCorpStyle.ProductRange.SEVCONDISPLAY))
				{
					if(this.treeView1.SelectedNode.Text.ToUpper() == this.DataLogTNText.ToUpper())
					{
						MenuItem MIReadAndExportDatalog = new MenuItem("Read datalog and export to Excel file");
						MIReadAndExportDatalog.Click +=new EventHandler(exportDomainMI_Click);
						contextMenu1.MenuItems.Add(MIReadAndExportDatalog);
					}
					else if(this.treeView1.SelectedNode.Text.ToUpper() == this.FaultLogTNText.ToUpper())
					{
						MenuItem MIReadAndExportFaultlog = new MenuItem("Read faultlog and export to Excel file");
						MIReadAndExportFaultlog.Click +=new EventHandler(exportFLDomainMI_Click);
						contextMenu1.MenuItems.Add(MIReadAndExportFaultlog);

					}
				}
				#endregion display datalog domain export

				#endregion Context menu set up for CAN node
			}
			else if(selNodeTag.tableindex == MAIN_WINDOW.GraphTblIndex) 
			{ //user is somewhere in the monitoring store sub-tree
				#region Context menu setup for the Nodes in Monitoring Store leg
				updateMonitorStoreContextMenu();
				#endregion Context menu setup for the Nodes in Monitoring Store leg
			}
			else if(selNodeTag.tableindex == MAIN_WINDOW.DCFTblIndex)
			{
				#region context menu setup for nodes in the DCF leg
				updateDCFContextMenu();
				#endregion context menu setup for nodes in the DCF leg
			}
			else if (this.treeView1.SelectedNode.FullPath.IndexOf(this.COBandPDOsTN.Text) != -1)
			{
				#region context menu setup for System PDOs node
				#endregion context menu setup for System PDOs node
			}
			else if (this.treeView1.SelectedNode.FullPath.IndexOf(this.CANBUsConfigTN.Text) != -1)
			{
				#region context emnu setup for CAN bus config Node
				#endregion context emnu setup for CAN bus config Node
			}
			contextMenu1.Show(treeView1, newPoint);
			this.treeView1.ContextMenu = null;
		}

		#region Logs
		private void resetAllLogsMI_Click(object sender, EventArgs e)
		{
			string message = "This will request reset of all logs for this CAN node. Do you wish to continue?";
			string caption = "WARNING";
			MessageBoxButtons buttons = MessageBoxButtons.YesNo;
			DialogResult result;

			// Displays the Message of redirects if Autovalidating.
			result = Message.Show(this, message, caption, buttons,
				MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);

			if(result == DialogResult.Yes)
			{
                ODItemData odSub = null;
				treeNodeTag selNodeTag = (treeNodeTag) this.treeView1.SelectedNode.Tag;
				this.statusBarPanel3.Text = "Requesting fault log reset, please wait";
				//this.sysInfo.nodes[selNodeTag.tableindex].resetLog(LogType.FaultLog);
                odSub = this.sysInfo.nodes[selNodeTag.tableindex].getODSubFromObjectType(SevconObjectType.FAULTS_FIFO_CTRL, 0x01);
                if (odSub != null)
                {
                    this.sysInfo.nodes[selNodeTag.tableindex].writeODValue(odSub, 0x01);
                }
	
				if(SystemInfo.errorSB.Length>0)
				{
					SystemInfo.errorSB.Append("\nFailed to reset fault log.");
				}
				this.statusBarPanel3.Text = "Requesting system log reset, please wait";
				this.statusBar1.Update();
                odSub = this.sysInfo.nodes[selNodeTag.tableindex].getODSubFromObjectType(SevconObjectType.SYSTEM_FIFO_CTRL, 0x01);
                if (odSub != null)
                {
                    this.sysInfo.nodes[selNodeTag.tableindex].writeODValue(odSub, 0x01);
                } 
                //this.sysInfo.nodes[selNodeTag.tableindex].resetLog(LogType.SystemLog);	


				this.statusBarPanel3.Text = "Requesting Customer Operational log reset, please wait";
				this.statusBar1.Update();
				//this.sysInfo.nodes[ selNodeTag.tableindex].resetLog(LogType.CustOpLog);
                odSub = this.sysInfo.nodes[selNodeTag.tableindex].getODSubFromObjectType(SevconObjectType.CUST_OPERATIONAL_MONITOR, 0x01);
                if (odSub != null)
                {
                    this.sysInfo.nodes[selNodeTag.tableindex].writeODValue(odSub, 0x01);
                }

				if(this.sysInfo.systemAccess>= SCCorpStyle.AccLevel_ResetSevconOpLog)  
				{
					this.statusBarPanel3.Text = "Requesting Sevcon Operational log reset, please wait";
					//this.sysInfo.nodes[selNodeTag.tableindex].resetLog(LogType.SEVCONOpLog);	
                    odSub = this.sysInfo.nodes[selNodeTag.tableindex].getODSubFromObjectType(SevconObjectType.SEVCON_OPERATIONAL_MONITOR, 0x01);
                    if (odSub != null)
                    {
                        this.sysInfo.nodes[selNodeTag.tableindex].writeODValue(odSub, 0x01);
                    }

				}
				else
				{
				}
				this.statusBarPanel3.Text = "Requesting Event Counters log reset, please wait";
				this.statusBar1.Update();
				//this.sysInfo.nodes[selNodeTag.tableindex].resetLog( LogType.EventLog);
                odSub = this.sysInfo.nodes[selNodeTag.tableindex].getODSubFromObjectType(SevconObjectType.OTHER_EVENTLOG_CTRL, 0x01);
                if (odSub != null)
                {
                    this.sysInfo.nodes[selNodeTag.tableindex].writeODValue(odSub, 0x01);
                }	
				if(SystemInfo.errorSB.Length>0)
				{
					this.statusBarPanel3.Text = "Some Logs may not have reset";
				}
				else
				{
					this.statusBarPanel3.Text = "All logs reset OK";
				}
			}		
		}
		#endregion logs
		private void BranchDeleteMI_Click(object sender, EventArgs e)
		{
			if(MAIN_WINDOW.currTblIndex == MAIN_WINDOW.GraphTblIndex)
			{
				TreeNode parentNode = this.treeView1.SelectedNode.Parent;
				removeMonitoringListNodes(this.treeView1.SelectedNode);
				if(parentNode != null)
				{
					this.treeView1.SelectedNode = parentNode;
				}
				else
				{
					this.treeView1.SelectedNode = this.GraphCustList_TN;
				}
				this.handleTreeNodeSelection(this.treeView1.SelectedNode, true);
			}
			else if(MAIN_WINDOW.currTblIndex == MAIN_WINDOW.DCFTblIndex)
			{
				TreeNode parentNode = this.treeView1.SelectedNode.Parent;
				removeDCFTreeNodes(this.treeView1.SelectedNode);
				if(parentNode != null)
				{
					this.treeView1.SelectedNode = parentNode;
				}
				else
				{
					this.treeView1.SelectedNode = this.DCFCustList_TN;
				}
				this.handleTreeNodeSelection(this.treeView1.SelectedNode, true);
			}
		}
		#endregion context menu related methods

		#region SplashScreen
		private void splashscreen_Closed(object sender, EventArgs e)
		{
			this.splashscreen = null;
		}

		private void splashTimer_Tick(object sender, System.EventArgs e)
		{
			if(this.splashRequired == true)
			{
				this.Hide();
				this.showSplashScreen();
				this.splashRequired = false;
				return;
			}
			if(this.splashscreen == null)  
			{
				this.splashTimer.Enabled = false;
				this.Show(); //allow MAin window to be seen
				#region Get DW Config file and detemrine whether we have vehicle profiles already set up to choose from
				if((DWConfigFile.vehicleprofiles != null) && (DWConfigFile.vehicleprofiles.Count>0))
				{
					foreach(string str in DWConfigFile.failedVehicleprofiles)
					{
						SystemInfo.errorSB.Append("\nVehicle profile: " + str);
					}
					if(SystemInfo.errorSB.Length>0)
					{
						sysInfo.displayErrorFeedbackToUser("The following vehicle profiles are invalid and will be ignored");
					}
					showProfileSelectWindow();//we have one or more vehicle profiles for user to choose form
				}
				else
				{
					this.confirmCloseOtherForms();
					if(this.OwnedForms.Length==0)
					{
						showOptionsWindow();// no profiles set up - so send user to setup screen
					}
				}

				#endregion Get DW Config file and detemrine whether we have vehicle profiles already set up to choose from
			}
		}
		#endregion SplashScreen

        private void DWdataset_DefaultView_ListChanged(object sender, ListChangedEventArgs e)
        {
            // The list has changed (sorted on column etc), so redo the colours to match.
            if (this.dataRetrievalThread == null)
            {
                this.updateRowColoursArray();
            }
        }

		private void emerTable_DefaultView_ListChanged(object sender, ListChangedEventArgs e)
		{
			DataView myView = (DataView) sender;
			this.emergencyDG.Height = 	Math.Min(this.emergencyDGDefaultHeight,
				((myView.Count + 2) * emergencyDG.PreferredRowHeight)
				+ (myView.Count-1) + 4);
		}

		private void connDevTable_DefaultView_ListChanged(object sender, ListChangedEventArgs e)
		{
			DataView myView = (DataView) sender;
			this.connectedDevicesDG.Height = 	Math.Min(this.connectedDevicesDGDefaultHeight,
				((myView.Count + 2) * connectedDevicesDG.PreferredRowHeight)
				+ (myView.Count-1) + 4);
		}

		private void graphContextMenu_Click(object sender, EventArgs e)
		{
			if(this.DATA_MONITOR_FRM == null)
			{
				MenuItem mi = (MenuItem) sender;
				if(mi.Index == (int)GraphTypes.CALIBRATED)
				{
					this.DATA_MONITOR_FRM = new DATA_DISPLAY_WINDOW(sysInfo, this.monStore, GraphTypes.CALIBRATED);
				}
				else if(mi.Index == (int)GraphTypes.NON_CALIBRATED) //DR38000267
				{
					this.DATA_MONITOR_FRM = new DATA_DISPLAY_WINDOW(sysInfo, this.monStore, GraphTypes.NON_CALIBRATED);
				}
				else if(mi.Index == (int)GraphTypes.FROM_FILE)
				{
					this.DATA_MONITOR_FRM = new DATA_DISPLAY_WINDOW(sysInfo, this.monStore, GraphTypes.FROM_FILE);
				}
				this.DATA_MONITOR_FRM.Closed +=new EventHandler(DATA_MON_WINDOW_Closed);
				setupAndDisplayChildWindow(DATA_MONITOR_FRM);
			}

		}
		private void treeView1_DoubleClick(object sender, System.EventArgs e)
		{
			string tempStr = this.AllType1FormsClosed();
			if (this.COB_PDO_FRM != null)
			{
				tempStr = "System PDO configuration";
			}
			if(tempStr != "")
			{
				this.statusBarPanel3.Text = "Node expansion/collapse disabled while " + tempStr + " is active";
				return;
			}
		}

		private void MIignoreEmer_Click(object sender, EventArgs e)
		{
			MenuItem thisMI = (MenuItem) sender;
			if(thisMI.Text =="Ignore emergency messages from this node")
			{
				if(this.sysInfo.ignoreEmerFromNodeID.Contains(this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].nodeID) == false)
				{
					this.sysInfo.ignoreEmerFromNodeID.Add(this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].nodeID);
				}
			}
			else
			{
				if(this.sysInfo.ignoreEmerFromNodeID.Contains(this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].nodeID) == true)
				{
					this.sysInfo.ignoreEmerFromNodeID.Remove(this.sysInfo.nodes[MAIN_WINDOW.currTblIndex].nodeID);
				}
			}
		}

        /// <summary>
        /// Find menu item clicked, so create & open an instance of SearchForm.
        /// (same action taken as for the search pushbutton in the toolbar but
        /// the menu item allows the Ctrl-F shortcut expected by users).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindMI_Click(object sender, EventArgs e)
        {
            openSearchForm();
        }

        private void refreshDataMI_Click(object sender, EventArgs e)
        {
            treeNodeTag selNodeTag = (treeNodeTag) treeView1.SelectedNode.Tag;  //Tag tells us what this Treenode represents

            // only refresh data for nodes containing data (not open new windows etc)
            if ((selNodeTag.tagType == TNTagType.XMLHEADER) || (selNodeTag.tagType == TNTagType.EDSSECTION)
                || (selNodeTag.tagType == TNTagType.ODITEM) || (selNodeTag.tagType == TNTagType.ODSUB)
                || (selNodeTag.tagType == TNTagType.LOGS))
            {
                this.tbb_RefreshData.Enabled = false; // will be set back to true after data retrieved
                this.handleTreeNodeSelection(treeView1.SelectedNode, true);
            }
        }

        

		//		private void lb_MouseMove(object sender, MouseEventArgs e)
		//		{
		//			ListBox lb = (ListBox) sender;
		//			VPDOListBoxItemIndex = lb.IndexFromPoint(e.X, e.Y); 
		//			if((VPDOListBoxItemIndex>0) && (VPDOListBoxItemIndex<lb.Items.Count))
		//			{
		//				lb.SelectedItem = lb.Items[VPDOListBoxItemIndex];
		//			}
		//		}

	}
	#region Emergency Message Data Table
	internal class EmergencyTable : DataTable
	{
		internal EmergencyTable()
		{
			this.Columns.Add(EmerCols.NodeID.ToString(), typeof(System.String));// Add the Column to the table //ie pump, LH controller etc
			this.Columns.Add(EmerCols.Message.ToString(), typeof(System.String));// Add the column to the table. Node baud rate
		}
	}
	#endregion Emergency Message Data Table

	#region emergency message tablestyle
	internal class EmerTableStyle : SCbaseTableStyle
	{
		internal EmerTableStyle (int [] ColWidths)
		{
			//table style level parameters
			this.AllowSorting = false;

			SCbaseRODataGridTextBoxColumn c1 = new SCbaseRODataGridTextBoxColumn((int) (EmerCols.NodeID));
			c1.MappingName = EmerCols.NodeID.ToString();
			c1.HeaderText = "Node ID";
			c1.Width = ColWidths[(int) (EmerCols.NodeID)];
			c1.NullText = "Unknown";
			GridColumnStyles.Add(c1);

			SCbaseRODataGridTextBoxColumn c2 = new SCbaseRODataGridTextBoxColumn((int) (EmerCols.Message));
			c2.MappingName = EmerCols.Message.ToString();
			c2.HeaderText = "Message";
			c2.Width = ColWidths[(int) (EmerCols.Message)];
			c2.NullText = "";
			GridColumnStyles.Add(c2);

			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();

		}

	}
	#endregion emergency message tablestyle

	#region table style for CAN device generated Emergeny message table
	internal class devEmerTableStyle:SCbaseTableStyle
	{
		internal devEmerTableStyle(int [] ColWidths)
		{
			this.AllowSorting = false;

			SCbaseRODataGridTextBoxColumn c2 = new SCbaseRODataGridTextBoxColumn((int) (EmerCols.Message));
			c2.MappingName = devEmerCols.Message.ToString();
			c2.HeaderText = "Message";
			c2.Width = ColWidths[(int) (devEmerCols.Message)];
			c2.NullText = "";
			GridColumnStyles.Add(c2);

			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();

		}
	}
	#endregion table style for CAN device active Faults table

	internal class devActiveFaultsTable : DataTable
	{
		internal devActiveFaultsTable()
		{
			this.Columns.Add(devActiveFaults.Description.ToString(), typeof(System.String));// Add the column to the table. Node baud rate
		}
	}
	#region table style for active Faults
	public class actFualtsTableStyle:SCbaseTableStyle
	{
		public actFualtsTableStyle(int [] ColWidths)
		{
			this.AllowSorting = false;

			SCbaseRODataGridTextBoxColumn c2 = new SCbaseRODataGridTextBoxColumn((int) devActiveFaults.Description);
			c2.MappingName = devActiveFaults.Description.ToString();
			c2.HeaderText = "Description";
			c2.Width = ColWidths[(int) (devActiveFaults.Description)];
			c2.NullText = "";
			GridColumnStyles.Add(c2);

			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();

		}
	}
	#endregion  table style for active Faults
}
