/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.156$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/09/2008 23:16:58$
	$ModDate:23/09/2008 10:35:06$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	This window shows the graphing of selected OD parameters using data retrieved 
    from connected nodes (using SDOs or PDOs). Formatting of the timebase, X and Y 
    axis min, max and intervals and graph legends & colours is possible. Data logged 
    can be saved to file and later retrieved for viewing, formatting and printing.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36719: DATA_DISPLAY_WINDOW.cs 

   Rev 1.156    23/09/2008 23:16:58  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.155    09/07/2008 21:40:56  ak
 Constructor fixed, interval truncation error fixed, min/max SPECIAL format
 conversion fixed, LegendNames added (stored/read from file),
 getStoredPointsFromFile() reads offline data also, updateMonStoreData() added
 to update monitoring store with collated data during graphing for a file save
 or prior to exiting the data display window, 


   Rev 1.154    12/03/2008 13:43:50  ak
 All DI threads have ThreadPriority increased to Normal from BelowNormal
 (needed when running VCI3)


   Rev 1.153    21/02/2008 09:09:10  jw
 Minor bug fix for number conversion fo rmin and max ( was ussing row text
 whihc can be "") now uses odSub min and max values.   Timestamps values
 scaled for Ixxat VCI3 driver


   Rev 1.152    15/02/2008 12:43:38  jw
 TxData and the Monitiring params were static in VCI3. By passing VCI as
 object into the received message event handler we can make then instance
 variables as per VCI2 - closer to back compatibility


   Rev 1.151    25/01/2008 10:47:16  jw
 VCI3 and Vista functionality files merge - more testing required


   Rev 1.150    05/12/2007 22:12:56  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Diagnostics;
using Ixxat;

namespace DriveWizard
{
	#region enumerated types

	public enum MonitorStates {  GRAPHING, GRAPHING_PAUSED, GRAPH_FROM_FILE, PRINTING_GRAPH, NONE};
	public enum GraphTypes { CALIBRATED, NON_CALIBRATED, FROM_FILE, SELF_CHAR_OLVd, SELF_CHAR_OLVq };
	#endregion enumerated types

	#region Data Display form class
	/// <summary>
	/// Summary description for DATA_DISPLAY_WINDOW.
	/// </summary>
	public class DATA_DISPLAY_WINDOW : System.Windows.Forms.Form
	{
		#region form controls definition
		private System.Windows.Forms.Button closeBtn;
		private System.Windows.Forms.CheckBox MarkersCB;
		private System.Windows.Forms.Panel graphLegendPanel;
		private System.Windows.Forms.Panel graphUpdatePanel;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ComboBox TimeBaseComboBox;
		private System.Windows.Forms.Button updatePanelCloseBtn;
		private System.Windows.Forms.TextBox YDivTB;
		private System.Windows.Forms.TextBox YMaxTB;
		private System.Windows.Forms.TextBox YMinTB;
		private System.Windows.Forms.Button graphUpdateBtn;
		private System.Windows.Forms.TextBox XDivTB;
		private System.Windows.Forms.TextBox XMaxTB;
		private System.Windows.Forms.TextBox XMinTB;
		private System.Windows.Forms.TextBox MainTitleTB;
		private System.Windows.Forms.TextBox YAxLabelTB;
		private System.Windows.Forms.TextBox XAxLabelTB;
		private System.Windows.Forms.CheckBox XAxisGridCB;
		private System.Windows.Forms.CheckBox YAxisGridCB;
		private System.Windows.Forms.Button refreshBtn;
		private System.Windows.Forms.Button GraphingPauseBtn;
		private System.Windows.Forms.Label UserInstructionsLabel;
		private System.Windows.Forms.Label CustomisationPanelLabel;
		private System.Windows.Forms.Label XAxisTitleLabel;
		private System.Windows.Forms.Label YAxisTitleLabel;
		private System.Windows.Forms.Label GraphTitleLabel;
		private System.Windows.Forms.Label XAxisDivLabel;
		private System.Windows.Forms.Label XAxisMaxLabel;
		private System.Windows.Forms.Label XAxisMinLabel;
		private System.Windows.Forms.Label YAxisDivLabel;
		private System.Windows.Forms.Label YAxisMaxLabel;
		private System.Windows.Forms.Label YaxisMinLabel;
		private System.Windows.Forms.Label YAxisLabel;
		private System.Windows.Forms.Label XAxisLabel;
		private System.Windows.Forms.ComboBox PrioritySelectionCB;
		private System.Windows.Forms.Label priorityLabel;
		private System.Windows.Forms.Label intervalAchievedLabel;
		private System.Timers.Timer NMTStateChangetimer;
		private System.Windows.Forms.ErrorProvider errorProvider1;
		private System.Windows.Forms.Timer monitoringTimer;
		private System.Windows.Forms.Label MonitoringIntervalLabel;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem saveMI;
		private System.Windows.Forms.MenuItem pageSetupMI;
		private System.Windows.Forms.MenuItem printPreviewMI;
		private System.Windows.Forms.MenuItem printMI;
		private System.Windows.Forms.MenuItem graphingMI;
		private System.Windows.Forms.MenuItem GraphSetupPanelMI;
		private System.Windows.Forms.MenuItem LegenPanelMI;
		private System.Windows.Forms.Timer InitialTimer;
		#endregion

		#region my definitions
		private enum NMTStateChangeFlags 
		{
			None, 
			SetUpPDOsEnterPreOp, 
			SetUpPDOsEnterOp, 
			changeTimeBaseEnterPreOp, 
			changeTimeBaseEnterOp, 
			changePriorityEnterPreOP, 
			changePriorityEnterOp, 
			removePDOsEnteringPreOP,  
			revertingToNonCalibGraphing};
		#region parameters passed from Main Window
		private SystemInfo sysInfo;
		#endregion parameters passed from Main Window

		#region Display Values for Combo boxes
		private string [] SDOtimebases = {"50ms", "100ms", "200ms", "500ms", "1s", "2s", "3s"};
		private string [] PDOtimebases = {"5ms", "10ms", "20ms", "50ms", "100ms", "200ms", "500ms", "1s", "2s", "3s"};
		private string [] sectionNames = {"Show All", "Battery Application", "Communication Profile", 
											 "Generic I/O Profile", "Identity", "Logging", "Motor1 Profile", 
											 "Node Parameters", "Node Status", "Operating System Prompt", 
											 "PDO Mapping", "Power Steer Application", "Pump Application", 
											 "Security", "Store", "Traction Application", "Vehicle Application"};
		private Hashtable sectionNames_ht = new Hashtable();
		#endregion Display Values for Combo boxes
		#region threads
		private Thread dataRetrievalThread, monitoringThread, fileOpenThread;
		#endregion threads

		private DIFeedbackCode feedback = DIFeedbackCode.DICodeUnset;
		private MonitorStates monitorState = MonitorStates.NONE;
		private uint userSelectedTimeInterval = 100, TimeIntervaIBeingUsed = 100;	
		private int TimeIntervaIComboIndex = 1;
		COBIDPriority userSelectedPriority = COBIDPriority.mediumHigh, PriorityBeingUsed = COBIDPriority.mediumHigh;
		private int PriorityComboIndex = 1;
		#region parameters for graphing
		private bool CalibratedMonitoringPossible = false;
		public static System.Drawing.Color [] plotColours;
		private bool legendPanelLock = false, updatePanelLock = false;
		int legendPanelstartX = 0, legendPanelstartY = 0;
		int updatePanelstartX = 0, updatePanelstartY = 0;
		private	PointF [] axesPointsScreen, axesPointsPrint;
		Brush [] plotBrushes = null;
		Pen [] colouredPens = null, colouredPensNoMarker = null,blackPens = null;
		private	float topOffset = 40, bottomOffset = 80, rightOffset = 20;
		private float elapsedtime = 0;  //actual values
		private Font axesLabelFont, markFont, mainLabelFont;
		private StringFormat YAxisStringFormat;

		#region layout parameters for screen Graphics Object
		Graphics gScreen = null;
		private SizeF YaxisLabelSizeScreen, XAxisLabelSizeScreen, MainLabelSizeScreen;
		private RectangleF YAxeslabelBoundsScreen, XAxeslabelBoundsScreen, MainLabelBoundsScreen;
		private PointF [] XDivStartPointsScreen, XDivEndPointsScreen, YDivStartPointsScreen, YDivEndPointsScreen;
		private string [] XAxisDivLabels, YAxisDivLabels;
		private PointF ChartOriginScreen;
		private float screenMaxX, screenMaxY, screenMinX, screenMinY;
		float XDivInPixels = 0, YDivInPixels= 0, XscalingScreen, YScalingScreen;
		private RectangleF ChartClipAreaScreen;
		#endregion layout parameters for screen Graphics Object
		#region layout parameters for print Graphics object
		Graphics gPrint = null;
		private SizeF YaxisLabelSizePrint, XAxisLabelSizePrint, MainLabelSizePrint;
		private RectangleF YAxeslabelBoundsPrint, XAxeslabelBoundsPrint, MainLabelBoundsPrint, LegendBoundsPrint;
		private PointF [] XDivPointsPrint, YDivPointsPrint;
		private PointF ChartOriginPrint;
		private int AxisTickLengthInPrint = 0;
		private int paperWidth, paperHeight;
		float XDivInPrint = 0, YDivInPrint= 0, XscalingPrint, YScalingPrint;
		private RectangleF ChartAreaPrint;
		#endregion layout parameters for print Graphics object
		#region parameters for calculating axis max, mins and divisions (Graphics independent, numerical)
		private PointF prevScreenPoint;
		private ushort  numXDivs , numYDivs; 
		#endregion parameters for caluclating axis max, mins and divisions (Graphics independent, numerical)
		#region Prameters for legend boxes
		Panel [] LegendColours;
		Label [] LegendNames;
		private float legendHeight;
		#endregion Prameters for legend boxes

		SizeF DataLableStrSize;
		#endregion parameters for graphing
		#region parameters for Save, Print data
		PageSetupDialog setupDlg;
		PrintDialog printDlg;
		PrintDocument printDoc;
		PrintPreviewDialog previewDlg;
		#endregion parameters for Save, Print data
		NMTStateChangeFlags NMTFlag = NMTStateChangeFlags.None;
		ushort NMTTimeOutCounter = 0;
		GraphTypes graphTypeRequested;
		//		StringBuilder errorSB;
		bool windowCanCloseFlag = false;
		DataView nonHeaderRows = null;
		ArrayList [] OfflineScreenDataPoints = new ArrayList[0];
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.MenuItem menuItem2;
		myMonitorStore monStore;
		string [] SCParameterNamesForLegend;
		private System.Windows.Forms.CheckBox chkBoxclearData;
		#region parameters for graphing self char results
		#endregion parameters for graphing self char results
		int numOfItemsToplot = 0;
		int initialNumDataPoints = 200;

		bool maxYDivCoincidentWithAxisMax = false, minYDivCoincidentWithAxisMin = false;
		bool maxXDivCoincidentWithAxisMax = false; //min is always zero on X axis
		int zeroIndex;
		float screenZeroY;
		#endregion
		
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.closeBtn = new System.Windows.Forms.Button();
            this.UserInstructionsLabel = new System.Windows.Forms.Label();
            this.graphUpdatePanel = new System.Windows.Forms.Panel();
            this.chkBoxclearData = new System.Windows.Forms.CheckBox();
            this.CustomisationPanelLabel = new System.Windows.Forms.Label();
            this.MarkersCB = new System.Windows.Forms.CheckBox();
            this.YAxisGridCB = new System.Windows.Forms.CheckBox();
            this.XAxisGridCB = new System.Windows.Forms.CheckBox();
            this.XAxisTitleLabel = new System.Windows.Forms.Label();
            this.XAxLabelTB = new System.Windows.Forms.TextBox();
            this.YAxLabelTB = new System.Windows.Forms.TextBox();
            this.YAxisTitleLabel = new System.Windows.Forms.Label();
            this.MainTitleTB = new System.Windows.Forms.TextBox();
            this.GraphTitleLabel = new System.Windows.Forms.Label();
            this.XAxisDivLabel = new System.Windows.Forms.Label();
            this.XAxisMaxLabel = new System.Windows.Forms.Label();
            this.XAxisMinLabel = new System.Windows.Forms.Label();
            this.YAxisDivLabel = new System.Windows.Forms.Label();
            this.YAxisMaxLabel = new System.Windows.Forms.Label();
            this.YaxisMinLabel = new System.Windows.Forms.Label();
            this.updatePanelCloseBtn = new System.Windows.Forms.Button();
            this.YDivTB = new System.Windows.Forms.TextBox();
            this.YMaxTB = new System.Windows.Forms.TextBox();
            this.YMinTB = new System.Windows.Forms.TextBox();
            this.YAxisLabel = new System.Windows.Forms.Label();
            this.XAxisLabel = new System.Windows.Forms.Label();
            this.graphUpdateBtn = new System.Windows.Forms.Button();
            this.XDivTB = new System.Windows.Forms.TextBox();
            this.XMaxTB = new System.Windows.Forms.TextBox();
            this.XMinTB = new System.Windows.Forms.TextBox();
            this.graphLegendPanel = new System.Windows.Forms.Panel();
            this.TimeBaseComboBox = new System.Windows.Forms.ComboBox();
            this.MonitoringIntervalLabel = new System.Windows.Forms.Label();
            this.refreshBtn = new System.Windows.Forms.Button();
            this.GraphingPauseBtn = new System.Windows.Forms.Button();
            this.PrioritySelectionCB = new System.Windows.Forms.ComboBox();
            this.priorityLabel = new System.Windows.Forms.Label();
            this.intervalAchievedLabel = new System.Windows.Forms.Label();
            this.NMTStateChangetimer = new System.Timers.Timer();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider();
            this.monitoringTimer = new System.Windows.Forms.Timer(this.components);
            this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.saveMI = new System.Windows.Forms.MenuItem();
            this.pageSetupMI = new System.Windows.Forms.MenuItem();
            this.printPreviewMI = new System.Windows.Forms.MenuItem();
            this.printMI = new System.Windows.Forms.MenuItem();
            this.graphingMI = new System.Windows.Forms.MenuItem();
            this.GraphSetupPanelMI = new System.Windows.Forms.MenuItem();
            this.LegenPanelMI = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.InitialTimer = new System.Windows.Forms.Timer(this.components);
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.graphUpdatePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NMTStateChangetimer)).BeginInit();
            this.SuspendLayout();
            // 
            // closeBtn
            // 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.BackColor = System.Drawing.SystemColors.Control;
            this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.closeBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.closeBtn.Location = new System.Drawing.Point(664, 496);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(104, 24);
            this.closeBtn.TabIndex = 10;
            this.closeBtn.Text = "&Close window";
            // 
            // UserInstructionsLabel
            // 
			this.UserInstructionsLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.UserInstructionsLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.UserInstructionsLabel.Location = new System.Drawing.Point(8, 16);
            this.UserInstructionsLabel.Name = "UserInstructionsLabel";
            this.UserInstructionsLabel.Size = new System.Drawing.Size(640, 30);
            this.UserInstructionsLabel.TabIndex = 2;
            this.UserInstructionsLabel.Text = "Please wait";
            this.UserInstructionsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // graphUpdatePanel
            // 
            this.graphUpdatePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.graphUpdatePanel.BackColor = System.Drawing.SystemColors.Control;
            this.graphUpdatePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.graphUpdatePanel.Controls.Add(this.chkBoxclearData);
            this.graphUpdatePanel.Controls.Add(this.CustomisationPanelLabel);
            this.graphUpdatePanel.Controls.Add(this.MarkersCB);
            this.graphUpdatePanel.Controls.Add(this.YAxisGridCB);
            this.graphUpdatePanel.Controls.Add(this.XAxisGridCB);
            this.graphUpdatePanel.Controls.Add(this.XAxisTitleLabel);
            this.graphUpdatePanel.Controls.Add(this.XAxLabelTB);
            this.graphUpdatePanel.Controls.Add(this.YAxLabelTB);
            this.graphUpdatePanel.Controls.Add(this.YAxisTitleLabel);
            this.graphUpdatePanel.Controls.Add(this.MainTitleTB);
            this.graphUpdatePanel.Controls.Add(this.GraphTitleLabel);
            this.graphUpdatePanel.Controls.Add(this.XAxisDivLabel);
            this.graphUpdatePanel.Controls.Add(this.XAxisMaxLabel);
            this.graphUpdatePanel.Controls.Add(this.XAxisMinLabel);
            this.graphUpdatePanel.Controls.Add(this.YAxisDivLabel);
            this.graphUpdatePanel.Controls.Add(this.YAxisMaxLabel);
            this.graphUpdatePanel.Controls.Add(this.YaxisMinLabel);
            this.graphUpdatePanel.Controls.Add(this.updatePanelCloseBtn);
            this.graphUpdatePanel.Controls.Add(this.YDivTB);
            this.graphUpdatePanel.Controls.Add(this.YMaxTB);
            this.graphUpdatePanel.Controls.Add(this.YMinTB);
            this.graphUpdatePanel.Controls.Add(this.YAxisLabel);
            this.graphUpdatePanel.Controls.Add(this.XAxisLabel);
            this.graphUpdatePanel.Controls.Add(this.graphUpdateBtn);
            this.graphUpdatePanel.Controls.Add(this.XDivTB);
            this.graphUpdatePanel.Controls.Add(this.XMaxTB);
            this.graphUpdatePanel.Controls.Add(this.XMinTB);
            this.graphUpdatePanel.Location = new System.Drawing.Point(344, 8);
            this.graphUpdatePanel.Name = "graphUpdatePanel";
            this.graphUpdatePanel.Size = new System.Drawing.Size(424, 328);
            this.graphUpdatePanel.TabIndex = 24;
            this.graphUpdatePanel.Visible = false;
            // 
            // chkBoxclearData
            // 
			this.chkBoxclearData.Location = new System.Drawing.Point(256, 176);
            this.chkBoxclearData.Name = "chkBoxclearData";
			this.chkBoxclearData.Size = new System.Drawing.Size(152, 16);
            this.chkBoxclearData.TabIndex = 42;
            this.chkBoxclearData.Text = "Clear data on restart";
            // 
            // CustomisationPanelLabel
            // 
			this.CustomisationPanelLabel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.CustomisationPanelLabel.Location = new System.Drawing.Point(56, 8);
            this.CustomisationPanelLabel.Name = "CustomisationPanelLabel";
            this.CustomisationPanelLabel.Size = new System.Drawing.Size(320, 16);
            this.CustomisationPanelLabel.TabIndex = 41;
            this.CustomisationPanelLabel.Text = "Customisation Panel";
            this.CustomisationPanelLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // MarkersCB
            // 
            this.MarkersCB.Checked = true;
            this.MarkersCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MarkersCB.Location = new System.Drawing.Point(96, 168);
            this.MarkersCB.Name = "MarkersCB";
            this.MarkersCB.TabIndex = 40;
            this.MarkersCB.Text = "Data markers";
            // 
            // YAxisGridCB
            // 
            this.YAxisGridCB.Checked = true;
            this.YAxisGridCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.YAxisGridCB.Location = new System.Drawing.Point(288, 144);
            this.YAxisGridCB.Name = "YAxisGridCB";
            this.YAxisGridCB.TabIndex = 39;
            this.YAxisGridCB.Text = "Gridlines";
            // 
            // XAxisGridCB
            // 
            this.XAxisGridCB.Checked = true;
            this.XAxisGridCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.XAxisGridCB.Location = new System.Drawing.Point(96, 144);
            this.XAxisGridCB.Name = "XAxisGridCB";
            this.XAxisGridCB.TabIndex = 38;
            this.XAxisGridCB.Text = "Gridlines";
            // 
            // XAxisTitleLabel
            // 
            this.XAxisTitleLabel.Location = new System.Drawing.Point(24, 264);
            this.XAxisTitleLabel.Name = "XAxisTitleLabel";
            this.XAxisTitleLabel.Size = new System.Drawing.Size(88, 23);
            this.XAxisTitleLabel.TabIndex = 37;
            this.XAxisTitleLabel.Text = "X Axis Label";
            this.XAxisTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // XAxLabelTB
            // 
            this.XAxLabelTB.Location = new System.Drawing.Point(112, 264);
            this.XAxLabelTB.MaxLength = 50;
            this.XAxLabelTB.Name = "XAxLabelTB";
            this.XAxLabelTB.Size = new System.Drawing.Size(304, 22);
            this.XAxLabelTB.TabIndex = 36;
			this.XAxLabelTB.Text = "";
            // 
            // YAxLabelTB
            // 
            this.YAxLabelTB.Location = new System.Drawing.Point(112, 232);
            this.YAxLabelTB.MaxLength = 50;
            this.YAxLabelTB.Name = "YAxLabelTB";
            this.YAxLabelTB.Size = new System.Drawing.Size(304, 22);
            this.YAxLabelTB.TabIndex = 35;
			this.YAxLabelTB.Text = "";
            // 
            // YAxisTitleLabel
            // 
            this.YAxisTitleLabel.Location = new System.Drawing.Point(24, 232);
            this.YAxisTitleLabel.Name = "YAxisTitleLabel";
            this.YAxisTitleLabel.Size = new System.Drawing.Size(88, 23);
            this.YAxisTitleLabel.TabIndex = 34;
            this.YAxisTitleLabel.Text = "Y Axis Label";
            this.YAxisTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainTitleTB
            // 
            this.MainTitleTB.BackColor = System.Drawing.SystemColors.Window;
            this.MainTitleTB.Location = new System.Drawing.Point(112, 200);
            this.MainTitleTB.MaxLength = 50;
            this.MainTitleTB.Name = "MainTitleTB";
            this.MainTitleTB.Size = new System.Drawing.Size(304, 22);
            this.MainTitleTB.TabIndex = 33;
			this.MainTitleTB.Text = "";
            // 
            // GraphTitleLabel
            // 
            this.GraphTitleLabel.Location = new System.Drawing.Point(40, 200);
            this.GraphTitleLabel.Name = "GraphTitleLabel";
            this.GraphTitleLabel.Size = new System.Drawing.Size(72, 23);
            this.GraphTitleLabel.TabIndex = 32;
            this.GraphTitleLabel.Text = "Graph Title";
            this.GraphTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // XAxisDivLabel
            // 
            this.XAxisDivLabel.Location = new System.Drawing.Point(40, 112);
            this.XAxisDivLabel.Name = "XAxisDivLabel";
            this.XAxisDivLabel.Size = new System.Drawing.Size(64, 23);
            this.XAxisDivLabel.TabIndex = 31;
            this.XAxisDivLabel.Text = "Division";
            this.XAxisDivLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // XAxisMaxLabel
            // 
            this.XAxisMaxLabel.Location = new System.Drawing.Point(40, 80);
            this.XAxisMaxLabel.Name = "XAxisMaxLabel";
            this.XAxisMaxLabel.Size = new System.Drawing.Size(64, 23);
            this.XAxisMaxLabel.TabIndex = 30;
            this.XAxisMaxLabel.Text = "Max";
            this.XAxisMaxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // XAxisMinLabel
            // 
            this.XAxisMinLabel.Location = new System.Drawing.Point(40, 48);
            this.XAxisMinLabel.Name = "XAxisMinLabel";
            this.XAxisMinLabel.Size = new System.Drawing.Size(64, 23);
            this.XAxisMinLabel.TabIndex = 29;
            this.XAxisMinLabel.Text = "Min";
            this.XAxisMinLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // YAxisDivLabel
            // 
            this.YAxisDivLabel.Location = new System.Drawing.Point(232, 112);
            this.YAxisDivLabel.Name = "YAxisDivLabel";
            this.YAxisDivLabel.Size = new System.Drawing.Size(64, 23);
            this.YAxisDivLabel.TabIndex = 28;
            this.YAxisDivLabel.Text = "Division";
            this.YAxisDivLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // YAxisMaxLabel
            // 
            this.YAxisMaxLabel.Location = new System.Drawing.Point(224, 80);
            this.YAxisMaxLabel.Name = "YAxisMaxLabel";
            this.YAxisMaxLabel.Size = new System.Drawing.Size(64, 23);
            this.YAxisMaxLabel.TabIndex = 27;
            this.YAxisMaxLabel.Text = "Max";
            this.YAxisMaxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // YaxisMinLabel
            // 
            this.YaxisMinLabel.Location = new System.Drawing.Point(224, 48);
            this.YaxisMinLabel.Name = "YaxisMinLabel";
            this.YaxisMinLabel.Size = new System.Drawing.Size(64, 23);
            this.YaxisMinLabel.TabIndex = 26;
            this.YaxisMinLabel.Text = "Min";
            this.YaxisMinLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // updatePanelCloseBtn
            // 
            this.updatePanelCloseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.updatePanelCloseBtn.BackColor = System.Drawing.SystemColors.Control;
            this.updatePanelCloseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.updatePanelCloseBtn.Location = new System.Drawing.Point(272, 296);
            this.updatePanelCloseBtn.Name = "updatePanelCloseBtn";
            this.updatePanelCloseBtn.Size = new System.Drawing.Size(140, 25);
            this.updatePanelCloseBtn.TabIndex = 25;
            this.updatePanelCloseBtn.Text = "&Close this panel";
            // 
            // YDivTB
            // 
            this.YDivTB.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.YDivTB.Location = new System.Drawing.Point(296, 112);
            this.YDivTB.MaxLength = 10;
            this.YDivTB.Name = "YDivTB";
            this.YDivTB.Size = new System.Drawing.Size(96, 22);
            this.YDivTB.TabIndex = 24;
			this.YDivTB.Text = "";
            // 
            // YMaxTB
            // 
            this.YMaxTB.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.YMaxTB.Location = new System.Drawing.Point(296, 80);
            this.YMaxTB.MaxLength = 10;
            this.YMaxTB.Name = "YMaxTB";
            this.YMaxTB.Size = new System.Drawing.Size(96, 22);
            this.YMaxTB.TabIndex = 23;
			this.YMaxTB.Text = "";
            // 
            // YMinTB
            // 
            this.YMinTB.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.YMinTB.Location = new System.Drawing.Point(296, 48);
            this.YMinTB.MaxLength = 10;
            this.YMinTB.Name = "YMinTB";
            this.YMinTB.Size = new System.Drawing.Size(96, 22);
            this.YMinTB.TabIndex = 22;
			this.YMinTB.Text = "";
            // 
            // YAxisLabel
            // 
			this.YAxisLabel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.YAxisLabel.Location = new System.Drawing.Point(344, 24);
            this.YAxisLabel.Name = "YAxisLabel";
            this.YAxisLabel.Size = new System.Drawing.Size(48, 23);
            this.YAxisLabel.TabIndex = 21;
            this.YAxisLabel.Text = "Y Axis";
            this.YAxisLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // XAxisLabel
            // 
			this.XAxisLabel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.XAxisLabel.Location = new System.Drawing.Point(144, 24);
            this.XAxisLabel.Name = "XAxisLabel";
            this.XAxisLabel.Size = new System.Drawing.Size(48, 23);
            this.XAxisLabel.TabIndex = 20;
            this.XAxisLabel.Text = "X Axis";
            this.XAxisLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // graphUpdateBtn
            // 
            this.graphUpdateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.graphUpdateBtn.BackColor = System.Drawing.SystemColors.Control;
            this.graphUpdateBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.graphUpdateBtn.Location = new System.Drawing.Point(8, 296);
            this.graphUpdateBtn.Name = "graphUpdateBtn";
            this.graphUpdateBtn.Size = new System.Drawing.Size(140, 25);
            this.graphUpdateBtn.TabIndex = 19;
            this.graphUpdateBtn.Text = "&Update Graph";
            // 
            // XDivTB
            // 
            this.XDivTB.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.XDivTB.Location = new System.Drawing.Point(112, 112);
            this.XDivTB.MaxLength = 10;
            this.XDivTB.Name = "XDivTB";
            this.XDivTB.Size = new System.Drawing.Size(96, 22);
            this.XDivTB.TabIndex = 18;
			this.XDivTB.Text = "";
            // 
            // XMaxTB
            // 
            this.XMaxTB.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.XMaxTB.Location = new System.Drawing.Point(112, 80);
            this.XMaxTB.MaxLength = 10;
            this.XMaxTB.Name = "XMaxTB";
            this.XMaxTB.Size = new System.Drawing.Size(96, 22);
            this.XMaxTB.TabIndex = 17;
			this.XMaxTB.Text = "";
            // 
            // XMinTB
            // 
            this.XMinTB.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.XMinTB.Location = new System.Drawing.Point(112, 48);
            this.XMinTB.MaxLength = 10;
            this.XMinTB.Name = "XMinTB";
            this.XMinTB.Size = new System.Drawing.Size(96, 22);
            this.XMinTB.TabIndex = 16;
			this.XMinTB.Text = "";
            // 
            // graphLegendPanel
            // 
            this.graphLegendPanel.BackColor = System.Drawing.SystemColors.Control;
            this.graphLegendPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.graphLegendPanel.Location = new System.Drawing.Point(8, 8);
            this.graphLegendPanel.Name = "graphLegendPanel";
            this.graphLegendPanel.Size = new System.Drawing.Size(344, 264);
            this.graphLegendPanel.TabIndex = 27;
            this.graphLegendPanel.Visible = false;
            // 
            // TimeBaseComboBox
            // 
            this.TimeBaseComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TimeBaseComboBox.Location = new System.Drawing.Point(208, 496);
            this.TimeBaseComboBox.Name = "TimeBaseComboBox";
            this.TimeBaseComboBox.Size = new System.Drawing.Size(88, 24);
            this.TimeBaseComboBox.TabIndex = 28;
            this.TimeBaseComboBox.Visible = false;
            // 
            // MonitoringIntervalLabel
            // 
            this.MonitoringIntervalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MonitoringIntervalLabel.Location = new System.Drawing.Point(144, 496);
            this.MonitoringIntervalLabel.Name = "MonitoringIntervalLabel";
            this.MonitoringIntervalLabel.Size = new System.Drawing.Size(56, 24);
            this.MonitoringIntervalLabel.TabIndex = 29;
            this.MonitoringIntervalLabel.Text = "Interval:";
            this.MonitoringIntervalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.MonitoringIntervalLabel.Visible = false;
            // 
            // refreshBtn
            // 
            this.refreshBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshBtn.BackColor = System.Drawing.SystemColors.Control;
            this.refreshBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refreshBtn.Location = new System.Drawing.Point(560, 496);
            this.refreshBtn.Name = "refreshBtn";
            this.refreshBtn.Size = new System.Drawing.Size(88, 24);
            this.refreshBtn.TabIndex = 30;
            this.refreshBtn.Text = "Refresh";
            this.refreshBtn.Visible = false;
            // 
            // GraphingPauseBtn
            // 
            this.GraphingPauseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GraphingPauseBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.GraphingPauseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.GraphingPauseBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.GraphingPauseBtn.Location = new System.Drawing.Point(8, 488);
            this.GraphingPauseBtn.Name = "GraphingPauseBtn";
            this.GraphingPauseBtn.Size = new System.Drawing.Size(75, 25);
            this.GraphingPauseBtn.TabIndex = 31;
            this.GraphingPauseBtn.Text = "&Start";
            this.GraphingPauseBtn.Visible = false;
            // 
            // PrioritySelectionCB
            // 
            this.PrioritySelectionCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PrioritySelectionCB.Location = new System.Drawing.Point(408, 496);
            this.PrioritySelectionCB.Name = "PrioritySelectionCB";
            this.PrioritySelectionCB.Size = new System.Drawing.Size(104, 24);
            this.PrioritySelectionCB.TabIndex = 32;
            this.PrioritySelectionCB.Visible = false;
            // 
            // priorityLabel
            // 
            this.priorityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.priorityLabel.Location = new System.Drawing.Point(344, 496);
            this.priorityLabel.Name = "priorityLabel";
            this.priorityLabel.Size = new System.Drawing.Size(56, 23);
            this.priorityLabel.TabIndex = 33;
            this.priorityLabel.Text = "Priority:";
            this.priorityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.priorityLabel.Visible = false;
            // 
            // intervalAchievedLabel
            // 
            this.intervalAchievedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.intervalAchievedLabel.Location = new System.Drawing.Point(304, 496);
            this.intervalAchievedLabel.Name = "intervalAchievedLabel";
            this.intervalAchievedLabel.Size = new System.Drawing.Size(240, 24);
            this.intervalAchievedLabel.TabIndex = 34;
            this.intervalAchievedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.intervalAchievedLabel.Visible = false;
            // 
            // NMTStateChangetimer
            // 
            this.NMTStateChangetimer.SynchronizingObject = this;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 524);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(772, 22);
            this.statusBar1.TabIndex = 35;
            this.statusBar1.Text = "statusBar1";
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.graphingMI,
            this.menuItem2});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.saveMI,
            this.pageSetupMI,
            this.printPreviewMI,
            this.printMI});
            this.menuItem1.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
            this.menuItem1.Text = "&File";
            // 
            // saveMI
            // 
            this.saveMI.Enabled = false;
            this.saveMI.Index = 0;
            this.saveMI.Text = "&Save data to monitoring file";
            // 
            // pageSetupMI
            // 
            this.pageSetupMI.Enabled = false;
            this.pageSetupMI.Index = 1;
            this.pageSetupMI.Text = "Page set&up";
            // 
            // printPreviewMI
            // 
            this.printPreviewMI.Enabled = false;
            this.printPreviewMI.Index = 2;
            this.printPreviewMI.Text = "Print p&review";
            // 
            // printMI
            // 
            this.printMI.Enabled = false;
            this.printMI.Index = 3;
            this.printMI.Text = "&Print graph";
            // 
            // graphingMI
            // 
            this.graphingMI.Index = 1;
            this.graphingMI.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.GraphSetupPanelMI,
            this.LegenPanelMI});
            this.graphingMI.Text = "&Graph";
            this.graphingMI.Visible = false;
            // 
            // GraphSetupPanelMI
            // 
            this.GraphSetupPanelMI.Index = 0;
            this.GraphSetupPanelMI.Text = "Show &Customisation Panel";
            // 
            // LegenPanelMI
            // 
            this.LegenPanelMI.Index = 1;
            this.LegenPanelMI.Text = "Show &Legend Panel";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 2;
            this.menuItem2.Text = "";
            // 
            // InitialTimer
            // 
            this.InitialTimer.Interval = 50;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
            // 
            // DATA_DISPLAY_WINDOW
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(772, 546);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.graphUpdatePanel);
            this.Controls.Add(this.intervalAchievedLabel);
            this.Controls.Add(this.priorityLabel);
            this.Controls.Add(this.PrioritySelectionCB);
            this.Controls.Add(this.GraphingPauseBtn);
            this.Controls.Add(this.refreshBtn);
            this.Controls.Add(this.MonitoringIntervalLabel);
            this.Controls.Add(this.TimeBaseComboBox);
            this.Controls.Add(this.UserInstructionsLabel);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.graphLegendPanel);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Menu = this.mainMenu1;
            this.Name = "DATA_DISPLAY_WINDOW";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DriveWizard: Parameter Monitoring";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DATA_DISPLAY_WINDOW_MouseDown);
            this.graphUpdatePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.NMTStateChangetimer)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		#region intialisation
		/*--------------------------------------------------------------------------
		 *  Name			: DATA_DISPLAY_WINDOW()
		 *  Description     : Constructor function for form. Set up of any initial variables
		 *					  that are available prior to th eform load event.
		 *  Parameters      : systemInfo class, CANopen node number and a descriptive
		 *					  string about the current CANopen node.
		 *  Used Variables  : none
		 *  Preconditions   : This form is only available when at least one SEVCON or 3rd party node is 
		 *					  connected.  SEVCON nodes can only be selected when the user has logged in.
		 *  Return value    : none
		 *--------------------------------------------------------------------------*/
		public DATA_DISPLAY_WINDOW(SystemInfo passed_systemInfo, myMonitorStore passed_monitorStore, GraphTypes passed_graphType)
		{
			// Required for Windows Form Designer support
			InitializeComponent();
			setUpEventHandlers();
			formatControls();
			sysInfo = passed_systemInfo;
			#region Priortiy Selector ComboBox
			this.PrioritySelectionCB.Items.Add(COBIDPriority.high.ToString());
			this.PrioritySelectionCB.Items.Add(COBIDPriority.mediumHigh.ToString());
			this.PrioritySelectionCB.Items.Add(COBIDPriority.mediumLow.ToString());
			this.PrioritySelectionCB.Items.Add(COBIDPriority.low.ToString());
			this.PrioritySelectionCB.SelectedIndex = 1; //medium high
			this.PriorityComboIndex = 1;
			#endregion Priortiy Selector ComboBox

            sysInfo.CANcomms.VCI.OdItemsBeingMonitored = new ArrayList();

            monStore = passed_monitorStore;
            #region create the VCI list from the Montiro Store
            // fix: ensure only items from the monitor store which exist on the currently connected
            // nodes are added to the VCI.OdItemsBeingMonitored
            foreach (nodeInfo monNode in monStore.myMonNodes)
			{
                bool matchingNodeFound = false;
				nodeInfo node = null;
				for(int i = 0;i< this.sysInfo.nodes.Length;i++)
				{
					if( monNode.nodeID == this.sysInfo.nodes[i].nodeID)
					{
						node = sysInfo.nodes[i];//create the refernece
                        matchingNodeFound = true;
						break;
					}
				}

                if (matchingNodeFound == true)
                {
                    foreach (ObjDictItem odItem in monNode.objectDictionary)
                    {
                        foreach (ODItemData mnODSub in odItem.odItemSubs)
                        {
                            if ((mnODSub.subNumber >= 0) && (mnODSub.isNumItems == false))
                            {
                                ODItemAndNode subAndNode = new ODItemAndNode();
                                subAndNode.node = node;
                                subAndNode.ODparam = node.getODSub(mnODSub.indexNumber, mnODSub.subNumber);
                                if ((subAndNode.ODparam != null) && (sysInfo.CANcomms.VCI.OdItemsBeingMonitored.Count < MAIN_WINDOW.monitorCeiling))
                                {
                                    sysInfo.CANcomms.VCI.OdItemsBeingMonitored.Add(subAndNode);
                                }
                            }
                        }
                    }
                }
			}
			#endregion create the VCI list from the Montiro Store
			#region clear out any old datapoints for these OD items
            foreach (ODItemAndNode subAndNode in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
            {
				subAndNode.ODparam.measuredDataPoints = new ArrayList();
				subAndNode.ODparam.screendataPoints = new ArrayList();
				subAndNode.ODparam.lastPlottedPtIndex = 0;
			}
			#endregion clear out any old datapoints for these OD items
            numOfItemsToplot = sysInfo.CANcomms.VCI.OdItemsBeingMonitored.Count;
           graphTypeRequested = passed_graphType;
			setUpUIFromMonitoringStore();
			setupGraphPrintingIntialParameters();
		}

		public DATA_DISPLAY_WINDOW(ArrayList [] passed_dataseries,  string [] passed_legendNames, GraphTypes passed_graphType)
		{//This constructor for SC graphing
			// Required for Windows Form Designer support
			InitializeComponent();
			setUpEventHandlers();
			formatControls();
			graphTypeRequested = passed_graphType;
			this.OfflineScreenDataPoints = passed_dataseries;
			this.SCParameterNamesForLegend = passed_legendNames;
			this.monStore = new myMonitorStore();  //create a local monitring store for use form now on
			#region set up initial values in monStore
			this.monStore.graph.Xaxis.AxisLabel = "Time/s";
			this.monStore.graph.Yaxis.AxisLabel = "Applied Voltage/V";
			if(passed_graphType == GraphTypes.SELF_CHAR_OLVd)
			{
				this.monStore.graph.MainTitle = "Self Char Open Loop Test (Vd)";
			}
			else if(passed_graphType == GraphTypes.SELF_CHAR_OLVq)
			{
				this.monStore.graph.MainTitle = "Self Char Open Loop Test (Vq)";
			}
			#region get max and mins of Input, Id and Iq
			for(int i = 0;i< this.OfflineScreenDataPoints.Length;i++)
			{
				foreach(PointF pt in OfflineScreenDataPoints[i])
				{
					this.monStore.graph.Xaxis.Max = Math.Max(pt.X, this.monStore.graph.Xaxis.Max);
					this.monStore.graph.Xaxis.Min = Math.Min(pt.X, this.monStore.graph.Xaxis.Min );
					this.monStore.graph.Yaxis.Max = Math.Max(pt.Y, this.monStore.graph.Yaxis.Max );
					this.monStore.graph.Yaxis.Min = Math.Min(pt.Y, this.monStore.graph.Yaxis.Min);
				}
			}
			#endregion get max and mins of Input, Id and Iq
			#region now 'scale' the input to fit nicely in graph
			float minInput= 0, maxInput= 0;
			foreach(PointF inputPt in this.OfflineScreenDataPoints[0])
			{
				minInput = Math.Min(inputPt.Y, minInput);
				maxInput = Math.Max(inputPt.Y, maxInput);
			}
			//we have to multiply postive & -ve by same mulitlipler 
			//the smaller swing should lie just above/below the Id,Iq waveforms
			//so get abs values for comparison
			minInput = Math.Abs(minInput);
			maxInput = Math.Abs(maxInput);
			float grAbsMax = Math.Abs(this.monStore.graph.Yaxis.Max);
			float grAbsMin = Math.Abs(this.monStore.graph.Yaxis.Min);
			float YaxisMultiplier = 1.05F * (Math.Max(grAbsMax,grAbsMin)/Math.Min(minInput,maxInput));
			ArrayList modifiedIpPts = new ArrayList();
			foreach(PointF inputPt in this.OfflineScreenDataPoints[0])
			{
				modifiedIpPts.Add(new PointF(inputPt.X, (inputPt.Y*YaxisMultiplier) ));  //judetemp add 5% to see it ouyside the 
			}
			//now copy back
			this.OfflineScreenDataPoints[0] = modifiedIpPts;
			#endregion now 'scale' the input to fit nicely in graph
			#region then update the Y Axis max an dmin to take accound to new scaled input plus a samll buffer
			foreach(PointF pt in OfflineScreenDataPoints[0])
			{
				this.monStore.graph.Yaxis.Max = Math.Max(pt.Y, this.monStore.graph.Yaxis.Max );
				this.monStore.graph.Yaxis.Min = Math.Min(pt.Y, this.monStore.graph.Yaxis.Min);
			}
			this.monStore.graph.Yaxis.Max *=1.05F;
			this.monStore.graph.Yaxis.Min *=1.05F;
			#endregion then update the Y Axis max an dmin to take accound to new scaled input plus a samll buffer
			this.monStore.graph.Yaxis.DivValue = (float) Math.Ceiling(this.monStore.graph.Yaxis.Max-this.monStore.graph.Yaxis.Min)/20;  //to ensure not too many divs
			this.monStore.graph.Xaxis.DivValue =  (float)Math.Ceiling(this.monStore.graph.Xaxis.Max-this.monStore.graph.Xaxis.Min)/20;  //to ensure not too many divs
			#endregion set up initial values in monStore
			this.numOfItemsToplot = OfflineScreenDataPoints.Length;
			setUpUIFromMonitoringStore();
			setupGraphPrintingIntialParameters();
		}
		private void formatControls()
		{
			#region button colouring
			foreach (Control myControl in this.Controls)
			{
				if((myControl.GetType().ToString()) == "System.Windows.Forms.Button")
				{
					myControl.BackColor =  SCCorpStyle.buttonBackGround;
				}
				else if ((myControl.GetType().ToString()) == "System.Windows.Forms.Panel")
				{
					foreach (Control mypanelControl in myControl.Controls)
					{
						if((mypanelControl.GetType().ToString()) == "System.Windows.Forms.Button")
						{
							mypanelControl.BackColor =  SCCorpStyle.buttonBackGround;
						}
					}
				}
			}
			#endregion button colouring
		}
		public void setUpUIFromMonitoringStore()
		{
			this.MainTitleTB.Text = this.monStore.graph.MainTitle;
			this.MarkersCB.Checked = this.monStore.graph.ShowDataMarkers;
			this.XAxisGridCB.Checked = this.monStore.graph.Xaxis.LinesVisible;
			this.YAxisGridCB.Checked = this.monStore.graph.Yaxis.LinesVisible;
			#region Y axis params
			this.YAxLabelTB.Text = this.monStore.graph.Yaxis.AxisLabel;
			this.YMinTB.Text = this.monStore.graph.Yaxis.Min.ToString();
			this.YMaxTB.Text = this.monStore.graph.Yaxis.Max.ToString();
			this.YDivTB.Text = this.monStore.graph.Yaxis.DivValue.ToString();
			#endregion Y axis params
			#region X axis params
			XAxLabelTB.Text = this.monStore.graph.Xaxis.AxisLabel;
			this.XMinTB.Text = this.monStore.graph.Xaxis.Min.ToString();
			this.XMaxTB.Text = this.monStore.graph.Xaxis.Max.ToString();
			this.XDivTB.Text = this.monStore.graph.Xaxis.DivValue.ToString();
			#endregion X axis params
		}

		public void DDmonitorListSaveRequestListener( string receivedMessage )
		{
			//Message.Show(receivedMessage);
		}		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: DATA_DISPLAY_WINDOW_Load
		///		 *  Description     : Event Handler for form load event
		///		 *  Parameters      : System event arguments 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void DATA_DISPLAY_WINDOW_Load(object sender, System.EventArgs e)
		{
			this.InitialTimer.Enabled = true;
		}

		private void setupGraphPrintingIntialParameters()
		{
			#region print Dialog bits
			printDoc = new PrintDocument();
			printDoc.DefaultPageSettings.Landscape = true;
			printDoc.DefaultPageSettings.Margins.Left = 100;
			printDoc.DefaultPageSettings.Margins.Top = 100;
			printDoc.DefaultPageSettings.Margins.Right = 100;
			printDoc.DefaultPageSettings.Margins.Bottom = 100;
			printDoc.DocumentName = "DriveWizard Graph";  //will be displayed in Windows 'currently printing' (under Printer Settings) dialog
			printDlg = new PrintDialog();
			printDoc.DocumentName = "Print Document";
			printDlg.Document = printDoc;
			printDlg.AllowSomePages = false;
			printDlg.AllowPrintToFile = false;
			printDlg.AllowSelection = false;
			printDoc.PrintPage  += new PrintPageEventHandler(PrntPgEventHandler);

			setupDlg = new PageSetupDialog();
			setupDlg.Document = printDoc;
			setupDlg.AllowMargins = false;//judetemp - know printer vs print proeview margin difference - option to change margins removed forhtis version true;
			setupDlg.AllowOrientation = true;
			setupDlg.AllowPaper = true;
			setupDlg.AllowPrinter = false;
			setupDlg.MinMargins.Left = 100;
            setupDlg.MinMargins.Right = 100;
            setupDlg.MinMargins.Top = 100;
            setupDlg.MinMargins.Bottom = 100;

			previewDlg = new PrintPreviewDialog();
			previewDlg.Document = printDoc;
			printDoc.DefaultPageSettings.Margins.Left = setupDlg.MinMargins.Left;
			printDoc.DefaultPageSettings.Margins.Right = setupDlg.MinMargins.Right;
			printDoc.DefaultPageSettings.Margins.Top = setupDlg.MinMargins.Top;
			printDoc.DefaultPageSettings.Margins.Bottom = setupDlg.MinMargins.Bottom;
			#endregion print Dialog bits
		}

		private bool checkIfAllItemsArePDOMappable()
		{
			this.sysInfo.COBsInSystem = new ArrayList();//resetr this - note we MUSt reread whenever we do claib grpahing or PDO set up screen
			#region check which CANnodes we want to set up TxPDos from ( who cares what state the othe rnodes are in)
			this.sysInfo.readAllCOBItemsAndCreateCOBsInSystem(); //we MUST check them all - to prevent possbility of duplicate COBID
			ArrayList TxCANNodes = new ArrayList();
            foreach (ODItemAndNode monItem in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
            {
				if((TxCANNodes.Contains(monItem.node) == false) && (monItem.node.isSevconApplication() == true))   
				{ //check each Sevcon app node once to see if a mapping wouuld take us over IXATT limit of two Tx mappings per OD item
					TxCANNodes.Add(monItem.node);
					//just read and add then nodes we are interesred in for calibrated graphing 
					DIFeedbackCode feedback = monItem.node.checkPDOsAlreadyMapped(this.sysInfo.COBsInSystem);
					if(feedback != DIFeedbackCode.DISuccess)
					{
						this.statusBar1.Text = "Sevcon application - parameter already mapped twice";
						return false;
					}
				}
			}
			#endregion check which CANnodes we want to set up TxPDos from ( who cares what state the othe rnodes are in)

            foreach (ODItemAndNode subAndNode in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
            {
				#region does user have sufficient access on this CAN node
				if(subAndNode.node.accessLevel<SCCorpStyle.AccLevel_SevconPDOWrite)
				{
					return false;
				}
				#endregion does user have sufficient access on this CAN node

				#region can this object be mapped into a Tx PDO ?
				if(subAndNode.ODparam.PDOmappable == false)
				{
					return false;
				}
				if( 
					(subAndNode.ODparam.accessType != ObjectAccessType.ReadReadWrite)
					&& (subAndNode.ODparam.accessType != ObjectAccessType.ReadReadWriteInPreOp)
					&& (subAndNode.ODparam.accessType != ObjectAccessType.ReadWrite)
					&& (subAndNode.ODparam.accessType != ObjectAccessType.ReadWriteInPreOp)
					&& (subAndNode.ODparam.accessType != ObjectAccessType.Constant)
					&& (subAndNode.ODparam.accessType != ObjectAccessType.ReadOnly) 
					)
				{
					return false;
				}
				#endregion can this object be mapped into a Tx PDO ?
			}
			return true;
		}
		#endregion

		#region user controls hide/show methods
		private void hideUserControls()
		{
			this.UserInstructionsLabel.Text = "Please wait";
			GraphingPauseBtn.Visible = false;
			refreshBtn.Visible = false;

			this.PrioritySelectionCB.Visible = false;
			priorityLabel.Visible = false;
			this.intervalAchievedLabel.Visible = false;
			this.TimeBaseComboBox.Visible = false;
			this.MonitoringIntervalLabel.Visible = false;

			this.saveMI.Enabled = false;
			this.printMI.Enabled = false;
			this.graphingMI.Visible = false;
			this.pageSetupMI.Enabled = false;
			this.printPreviewMI.Enabled = false;
		}
		private void showUserControls()
		{
			switch (monitorState)
			{
				case MonitorStates.GRAPHING_PAUSED:
					#region GRAPHING_PAUSED
					this.graphLegendPanel.Visible = true;
					this.graphUpdatePanel.Visible = true;
					this.UserInstructionsLabel.Visible = false;
					this.GraphingPauseBtn.Visible = true;
					this.GraphingPauseBtn.Text = "&Start";
					this.refreshBtn.Visible = true;	
					this.TimeBaseComboBox.Visible = true;
					this.MonitoringIntervalLabel.Visible = true;
					if(this.CalibratedMonitoringPossible == true)
					{
						this.PrioritySelectionCB.Visible = true;
						priorityLabel.Visible = true;
						intervalAchievedLabel.Visible = false;
						statusBar1.Text = "Calibrated graphing (paused)";
					}
					else
					{
						statusBar1.Text = "Non-Calibrated graphing (paused)";
					}
					this.graphingMI.Visible = true;
					this.saveMI.Enabled = true;
					this.printMI.Enabled = true;
					this.pageSetupMI.Enabled = true;
					this.printPreviewMI.Enabled = true;
					#endregion GRAPHING_PAUSED
					break;

				case MonitorStates.GRAPH_FROM_FILE:
					#region GRAPH_FROM_FILE
					this.UserInstructionsLabel.Visible = false;
					this.graphingMI.Visible = true;
					this.printMI.Enabled = true;
					this.pageSetupMI.Enabled = true;
					this.printPreviewMI.Enabled = true;
					this.graphUpdatePanel.Visible = true;
					this.graphLegendPanel.Visible = true;
					this.saveMI.Enabled = true;
					this.refreshBtn.Visible = true;	
					if((this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVd)
						||(this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVq))
					{
						statusBar1.Text = "Displaying Self Char OL Vd resutls";
					}
					else
					{
						statusBar1.Text = "Displaying data from stored file"; //now starts in pasued state
					}
					#endregion GRAPH_FROM_FILE
					break;

				case MonitorStates.GRAPHING:
					#region GRAPHING
					this.graphLegendPanel.Visible = false;
					this.graphUpdatePanel.Visible = false;
					this.UserInstructionsLabel.Visible = false;
					this.GraphingPauseBtn.Visible = true;
					this.GraphingPauseBtn.Text = "&Pause";
					this.graphingMI.Visible = true;
					this.refreshBtn.Visible = true;	
					if(this.CalibratedMonitoringPossible == true)
					{
						intervalAchievedLabel.Visible = true;
						statusBar1.Text = "Calibrated graphing (" + monitoringTimer.Interval.ToString() + " ms)";
					}
					else
					{
						statusBar1.Text = "Non-calibrated graphing. Monitoring at approx. " + monitoringTimer.Interval.ToString() + " ms";
					}
					#endregion GRAPHING
					break;

				case MonitorStates.PRINTING_GRAPH:
					break;

				default:
					break;

			}
		}
		#endregion user controls hide/show methods

		#region user interaction
		private void GraphingPauseBtn_Click(object sender, System.EventArgs e)
		{
			//swithc timer off first for improved reaction
			this.monitoringTimer.Enabled = false;
			hideUserControls();
			if(monitorState == MonitorStates.GRAPHING)
			{
				#region GRAPHING
				if(this.CalibratedMonitoringPossible == true)
				{
					this.PrioritySelectionCB.Visible = true;  //needs to be done here 
					this.sysInfo.CANcomms.pausePDOMonitoring(); //tells DI to look for incoming SDOs
				}
				monitorState = MonitorStates.GRAPHING_PAUSED;
				if(this.graphTypeRequested == GraphTypes.NON_CALIBRATED)
				{ //we need to run the timer to get proper elapsed time during pause - so we get correct 'gap' as per PDO
					this.monitoringTimer.Enabled = true;
				}
				showUserControls();
				#endregion GRAPHING
			}
			else
			{
				resumeGraphing();  //monitoringTimer is enabled in here
			}

		}
		private void resumeGraphing()
		{
			if(this.graphTypeRequested == GraphTypes.CALIBRATED)
			{
				if(this.chkBoxclearData.Checked == false)
				{
					this.sysInfo.CANcomms.restartPDOMonitoring();
				}
				else
				{
					this.sysInfo.CANcomms.startPDOMonitoring();
				}
				this.monitoringTimer.Enabled = true;
			}
			else if (this.graphTypeRequested == GraphTypes.NON_CALIBRATED)
			{
				this.monitoringTimer.Enabled = false; //pause for now - was running to update elapsed time during the pause
				if(this.chkBoxclearData.Checked == true)
				{
					this.elapsedtime = 0;
                    foreach (ODItemAndNode item in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
                    {
						item.ODparam.lastPlottedPtIndex = 0;
						item.ODparam.measuredDataPoints = new ArrayList();
						item.ODparam.screendataPoints = new ArrayList();
					}
				}
			}
			else if ((this.graphTypeRequested == GraphTypes.FROM_FILE)
				|| (this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVd)
				|| (this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVq))
			{
				for(int rowIndex = 0;rowIndex<this.OfflineScreenDataPoints.Length;rowIndex++)
				{
					this.OfflineScreenDataPoints[rowIndex].Clear();
				}
			}
			monitorState = MonitorStates.GRAPHING;
			this.monitoringTimer.Enabled = true;
			showUserControls();
			this.Invalidate();
		}

		private void TimeBaseComboBox_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			#region set monitorRTimebase
			switch (this.TimeBaseComboBox.SelectedItem.ToString())
			{
				case "5ms":
					userSelectedTimeInterval = 5;
					break;
				case "10ms":
					userSelectedTimeInterval = 10;
					break;
				case "20ms":
					userSelectedTimeInterval = 20;
					break;
				case "50ms":
					userSelectedTimeInterval = 50;
					break;
				case "100ms":
					userSelectedTimeInterval = 100;
					break;
				case "200ms":
					userSelectedTimeInterval = 200;
					break;
				case "500ms":
					userSelectedTimeInterval = 500;
					break;
				case "1s":
					userSelectedTimeInterval = 1000;
					break;
				case "2s":
					userSelectedTimeInterval = 2000;
					break;
				case "3s":
					userSelectedTimeInterval = 3000;
					break;
				default:
					userSelectedTimeInterval = 100;
					if(this.CalibratedMonitoringPossible == true)
					{
						this.TimeBaseComboBox.SelectedIndex = 4;  //reset to 100ms
					}
					else
					{
						this.TimeBaseComboBox.SelectedIndex = 1;  //reset to 100ms
					}
					break;
			}
			if(userSelectedTimeInterval == TimeIntervaIBeingUsed)
			{
				return;
			}
			#endregion set monitorRTimebase
			if(this.CalibratedMonitoringPossible == true)
			{
				#region PDO monitoring
				feedback = this.sysInfo.forceSystemIntoPreOpMode();
				if(feedback!=DIFeedbackCode.DISuccess)
				{
					#region display failure message to user
					userSelectedTimeInterval = TimeIntervaIBeingUsed;//reset
					this.TimeBaseComboBox.SelectedIndex = TimeIntervaIComboIndex;
					#endregion display failure message to user
				}
				else //success so continue
				{
					this.hideUserControls();
					this.NMTFlag = NMTStateChangeFlags.changeTimeBaseEnterPreOp;
					this.NMTTimeOutCounter = 0; //reset the timeout counter
					this.NMTStateChangetimer.Enabled = true;
				}
				#endregion PDO monitoring
			}
			else  //SDO graphing
			{
				#region SDO monitoring
				this.monitoringTimer.Interval = (int) userSelectedTimeInterval;
				if(monitorState == MonitorStates.GRAPHING_PAUSED)
				{
					#region recalculate the X Axis and force graph redraw
					calculateInitialXAxisMaxAndMin();
					initialiseUserGraphEntryPaneXAxis();
					this.Invalidate();
					#endregion recalculate the X Axis and force graph redraw
				}
				TimeIntervaIBeingUsed = userSelectedTimeInterval;  //store the new timebase
				TimeIntervaIComboIndex = this.TimeBaseComboBox.SelectedIndex;  //and its selection value
				#endregion SDO monitoring
			}
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("Failed to Change monitoring time interval");
			}
		}
		private void PrioritySelectionCB_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			userSelectedPriority = (COBIDPriority) Enum.Parse(typeof(COBIDPriority), PrioritySelectionCB.SelectedItem.ToString());
			this.statusBar1.Text = "Requesting vehicle to go to Pre-operational";
			feedback = this.sysInfo.forceSystemIntoPreOpMode();
			if(feedback!=DIFeedbackCode.DISuccess)
			{
				this.PrioritySelectionCB.SelectedIndex = this.PriorityComboIndex;
				this.statusBar1.Text = "Pre-Op request failed";
			}
			else //success so continue
			{
				this.hideUserControls();
				this.NMTFlag = NMTStateChangeFlags.changePriorityEnterPreOP;
				this.NMTTimeOutCounter = 0; //reset the timeout counter
				this.NMTStateChangetimer.Enabled = true;
			}
			//creating the new PDOs is done once we have checked that all nodes are now in pre-op
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("Failed to change Priority");
			}
		}
		#endregion

		#region timer elapsed handlers
		private void InitialTimer_Tick(object sender, System.EventArgs e)
		{
			this.InitialTimer.Enabled = false;//run once only
			this.hideUserControls();
			if ( this.graphTypeRequested == GraphTypes.FROM_FILE)
			{
				#region open previously saved file
				nonHeaderRows = new DataView(MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.GraphTblIndex]);
				nonHeaderRows.RowFilter = @"sub <> ''";  //in SQL <> is same as !=
				this.OfflineScreenDataPoints = new ArrayList[System.Math.Min(MAIN_WINDOW.monitorCeiling,nonHeaderRows.Count)];
				for(int i = 0;i<this.OfflineScreenDataPoints.Length;i++)
				{
					this.OfflineScreenDataPoints[i] = new ArrayList(); //prevent nulls
				}
				this.monitorState = MonitorStates.GRAPH_FROM_FILE;

				#region draw graph from file data
				getStoredPlotPointsFromFile();
				CreateGraphicsTools();
				updateLegendPanel();
				updateAxesParametersFromUpdatePanelVlaues();
				calcLabelSizeScreen();
				this.showUserControls();
				#endregion draw graph from file data
				#endregion open previously saved file
			}
			else if ((this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVd)
				|| (this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVq))
			{
				this.monitorState = MonitorStates.GRAPH_FROM_FILE; //basically indicates static data
				#region draw graph from file data
				CreateGraphicsTools();
				updateLegendPanel();
				updateAxesParametersFromUpdatePanelVlaues();
				calcLabelSizeScreen();
				this.showUserControls();
				#endregion draw graph from file data

			}
			else if (graphTypeRequested == GraphTypes.CALIBRATED)
			{
				#region attempt to set up calibrated graphing
				this.statusBar1.Text =  "Checking if calibrated graphing is possible";
				CalibratedMonitoringPossible = false;  //willl be set true only if we successfully set up PDO monitoring
				if( checkIfAllItemsArePDOMappable() == true)
				{
					this.statusBar1.Text = "reading Communications profile for all nodes ";
					setupForPDOMonitoring();
					if(SystemInfo.errorSB.Length>0)
					{   
                        SystemInfo.errorSB.Append("\nFailed to set up Calibrated graphing.\n Switching to non-calibrated graphing.");
                        this.startNonCalibratedGraphing(); //DR38000267 restart so time axis reset back to 0s
                    }
				}
				else
				{
                    //DR38000260 always report error
                    SystemInfo.errorSB.Append("\nNot all items are suitable for Calibrated graphing.\n Switching to non-calibrated graphing.");
                    this.startNonCalibratedGraphing();
				}
				#endregion attempt to set up calibrated graphing
			}
			else if(this.graphTypeRequested == GraphTypes.NON_CALIBRATED)
			{
				this.startNonCalibratedGraphing();
			}
		}

		private void NMTStateChangetimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			string insertText = "";
			#region check for timeout
			if(NMTTimeOutCounter >= 50)
			{
				#region timeout expired failure
				this.NMTTimeOutCounter = 0;
				this.NMTStateChangetimer.Enabled = false;
				switch(this.NMTFlag)
				{
					case NMTStateChangeFlags.SetUpPDOsEnterPreOp:
						insertText = "Enter NMT Pre-Operational state timeout occurred. Switching to non-calibrated graphing ";
						this.startNonCalibratedGraphing();
						return;

					case NMTStateChangeFlags.changeTimeBaseEnterPreOp:
						#region revert to previous interval and tell user
						insertText = "Enter NMT pre-operational state timeout occurred. \nReverting to previous time interval and non-claibrated graphing";
						userSelectedTimeInterval = TimeIntervaIBeingUsed;//reset
						this.TimeBaseComboBox.SelectedIndex = TimeIntervaIComboIndex;
						#endregion revert to previous interval and tell user
						startNonCalibratedGraphing(); 
						break;

					case NMTStateChangeFlags.changePriorityEnterPreOP:
						#region Revert to previous priority and tell user
						insertText = "Enter NMT pre-operational state timeout occurred. \nReverting to  previous priority and non-claibrated graphing";
						this.userSelectedPriority = this.PriorityBeingUsed;
						this.PrioritySelectionCB.SelectedIndex = this.PriorityComboIndex;
						#endregion Revert to previous priority and tell user
						startNonCalibratedGraphing(); 
						
						break;

					case NMTStateChangeFlags.SetUpPDOsEnterOp:
					case NMTStateChangeFlags.changeTimeBaseEnterOp:
					case NMTStateChangeFlags.revertingToNonCalibGraphing:
					case NMTStateChangeFlags.changePriorityEnterOp:
						insertText = "Enter NMT Operational state timeout occurred. \nSwitching to non-calibrated graphing ";
						startNonCalibratedGraphing();  //we only get PDOs inOperational so revert to nonCalibrated
						return;  //use retun NOT break to prevent renetry whiles message Box is being displayed.

					case NMTStateChangeFlags.removePDOsEnteringPreOP:
						insertText = "Enter NMT Pre-Operational state timeout occurred ";
						windowCanCloseFlag = true;  //allow closure regardless of success
						this.statusBar1.Text = "Window closing";
						this.Close();
						return;

					default:
						//do nothing can happen if tiemr cdoes a couple of extra loops before exit is seen eg if MessageBox is on screen
						break;
				}
				#endregion timeout expired failure
				return;
			}
			#endregion check for timeout

			#region check whether all node are in required NMT state
			if (checkIfAllNodesAreinRequiredState() == false)
			{
				NMTTimeOutCounter++;
				return;
			}
			#endregion check whether all node are in required NMT state

			#region all nodes in required state so take appropriate actions
			this.NMTStateChangetimer.Enabled = false;
			this.NMTTimeOutCounter = 0;
			switch(this.NMTFlag)
			{
				case NMTStateChangeFlags.SetUpPDOsEnterPreOp:
					#region SetUpPDOsEnterPreOp
					if (requestPDOSetup() == false)
					{
						return;  //get out since we have revertd to non-claibrated graphing
					}
					break;
					#endregion SetUpPDOsEnterPreOp

				case NMTStateChangeFlags.SetUpPDOsEnterOp:
					#region Nodes now operational ready to do Calibrated graphing
					this.statusBar1.Text = "Setting up calibrated graphing";
					this.CalibratedMonitoringPossible = true;
					monitorState = MonitorStates.GRAPHING_PAUSED;
					initialiseGraphicsParams();
					this.TimeIntervaIComboIndex = 4; //to point to 100ms
					feedback = this.sysInfo.setupCOBReceivePArameters();
					if(feedback != DIFeedbackCode.DISuccess)
					{
						insertText =  "Unable to setup Revive Parameters for all Requested items. Some may not be received";
					}
					startPDODataRetrievalThread();
					this.TimeBaseComboBox.DataSource = PDOtimebases;
					this.TimeBaseComboBox.SelectedIndex = TimeIntervaIComboIndex;
					monitorState = MonitorStates.GRAPHING;
					this.monitoringTimer.Enabled = true;
					showUserControls();
					this.graphLegendPanel.Visible = true;
					this.graphUpdatePanel.Visible = true;
					return;
					#endregion Nodes now operational ready to do Calibrated graphing

				case NMTStateChangeFlags.changeTimeBaseEnterPreOp:
					#region successfully entererd pre-op so ask DI to change the PDO frequency
					this.sysInfo.CANcomms.pausePDOMonitoring();
					this.statusBar1.Text = "Requesting new PDO Tx interval";
					feedback = this.sysInfo.changeMonitoringTimebase(userSelectedTimeInterval);
					if(feedback == DIFeedbackCode.DISuccess)
					{
						this.NMTFlag = NMTStateChangeFlags.changeTimeBaseEnterOp;
					}
					else //failure
					{
						sysInfo.displayErrorFeedbackToUser("Unable to change calibrated monitoring interval, reverting to previous interval");
						this.statusBar1.Text = "Unable to change calibrated monitoring interval, reverting to previous interval";
						userSelectedTimeInterval = TimeIntervaIBeingUsed;//revert to previous timebase
						this.TimeBaseComboBox.SelectedIndex = TimeIntervaIComboIndex;
					}
					this.statusBar1.Text = "Requesting vehicle to go to Operational";
					feedback = this.sysInfo.releaseSystemFromPreOpMode();
					if(feedback == DIFeedbackCode.DISuccess)
					{
						this.NMTStateChangetimer.Enabled = true;
					}
					else //failure
					{
						insertText =  "Enter NMT operational state request rejected, switching to non-calibrated graphing";
						this.statusBar1.Text = "Enter NMT operational state request rejected";
						this.startNonCalibratedGraphing();
						return;
					}
					#endregion successfully entererd pre-op so try and change the PDO frequency
					break;

				case NMTStateChangeFlags.changeTimeBaseEnterOp:
					#region we have successfully re-entered operational - so update the graphics etc
					TimeIntervaIBeingUsed = userSelectedTimeInterval;  //update the stored timebase 
					TimeIntervaIComboIndex = this.TimeBaseComboBox.SelectedIndex;  //update the stored ComboBox selected index
					this.monitoringTimer.Interval = (int) userSelectedTimeInterval;
					#region recalculate the X Axis and force graph redraw
					calculateInitialXAxisMaxAndMin();
					initialiseUserGraphEntryPaneXAxis();
					this.Invalidate();
					#endregion recalculate the X Axis and force graph redraw
					this.resumeGraphing();
					#endregion we have successfully reentered operational - so update the graphics etc
					break;

				case NMTStateChangeFlags.changePriorityEnterPreOP:
					#region we have successfully entered pre-op so ask DI to change priority
					this.sysInfo.CANcomms.pausePDOMonitoring();
					this.statusBar1.Text = "Requesting  new priority";
					feedback = this.sysInfo.changeMonitoringPriority(this.userSelectedPriority);
					if(feedback == DIFeedbackCode.DISuccess)
					{
						this.NMTFlag = NMTStateChangeFlags.changePriorityEnterOp;
					}
					else //failure
					{
						insertText = "Unable to change priority";
						this.statusBar1.Text = "Unable to change priority";
						this.PrioritySelectionCB.SelectedIndex = this.PriorityComboIndex; //revert to before request
						this.userSelectedPriority = this.PriorityBeingUsed;
					}
					this.statusBar1.Text = "Requesting vehicle to go to Operational";
					feedback = this.sysInfo.releaseSystemFromPreOpMode();
					if(feedback == DIFeedbackCode.DISuccess)
					{
						this.NMTStateChangetimer.Enabled = true;
					}
					else
					{
						insertText = "Enter NMT operational staterequest rejected, switching to non-calibrated graphing";
						this.startNonCalibratedGraphing();
						return;
					}
					#endregion we have successfully entered pre-op so ask DI to change priority
					break;

				case NMTStateChangeFlags.changePriorityEnterOp:
					#region Success - so update the stored priority params with the user's ones
					this.PriorityBeingUsed = this.userSelectedPriority;  //update the stored priority 
					this.PriorityComboIndex = this.PrioritySelectionCB.SelectedIndex;  //update the stored ComboBox selected index
					#endregion Success - so update the stored priority params with the user's ones
					this.resumeGraphing();
					if(SystemInfo.errorSB.Length>0)  //the DI has flagged errors 
					{
						Form errFrm = new ErrorMessageWindow(SystemInfo.errorSB.ToString());
						DialogResult res = errFrm.ShowDialog();
						if(res == DialogResult.OK)
						{
							//do nothing this is just to freeze processing here unitl User hits OK
						}
						SystemInfo.errorSB.Length = 0; //ensure reset after use
					}
					break;

				case NMTStateChangeFlags.removePDOsEnteringPreOP:
					#region we are now back in pre-op so try and remove the PDOs that DW added
					feedback = this.sysInfo.restorePDOAndCOBIDConfiguration( );
					if(feedback != DIFeedbackCode.DISuccess)
					{
						insertText = "Unable to remove added PDOs. Close this window and launch System PDOs guided process";
					}
					#endregion we have suceesfuly got back into pre-op so try and remove the PDOs that we added
					windowCanCloseFlag = true;  //allow closure regardless of success
					this.Close();
					return;
			
				case NMTStateChangeFlags.revertingToNonCalibGraphing:
					startNonCalibratedGraphing();
					return;

				default:
					//do nothing - can occur if this timer does a couple more loops than requred when reverting to non-calibrated graphing
					break;
			}
			#endregion all nodes in required state so take appropriate actions
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser(insertText);
			}
		}

		private bool checkIfAllNodesAreinRequiredState()
		{
			int lastNodeID = 0, CANnodeIndex = 0;
            foreach (ODItemAndNode monItem in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
			{
				if( monItem.node.nodeID != lastNodeID)
				{ //not already checked this CAN node
					#region check if all affected nodes have entered correct NMT state
					this.sysInfo.getNodeNumber(monItem.node.nodeID, out CANnodeIndex);
					switch(this.NMTFlag)
					{
						case NMTStateChangeFlags.SetUpPDOsEnterPreOp:
						case NMTStateChangeFlags.changeTimeBaseEnterPreOp:
						case NMTStateChangeFlags.removePDOsEnteringPreOP:
						case NMTStateChangeFlags.changePriorityEnterPreOP:
							#region check if all nodes in Montiring list are now in pre op
							if(this.sysInfo.nodes[CANnodeIndex].nodeState != NodeState.PreOperational)
							{
								return false;  //not all the required nodes are in pre-op yet
							}
							#endregion check if all nodes in Montiring list are now in pre op
							break;

						default:  //assume waiting for operational
							#region check if all nodes in Montiring list are now in operational
							if(this.sysInfo.nodes[CANnodeIndex].nodeState != NodeState.Operational)
							{
								return false ;  //not all the required nodes are in pre-op yet
							}
							#endregion check if all nodes in Montiring list are now in operational
							break;
					}
					#endregion check if all affected nodes have entered correct NMT state
				}
				lastNodeID = monItem.node.nodeID;
			}
			return true;
		}
		private void monitoringTimer_Tick(object sender, System.EventArgs e)
		{
			this.monitoringTimer.Enabled = false;
			#region data graphing
			PointF newPoint = new PointF();
			if(CalibratedMonitoringPossible == false)
			{
				#region SDO Monitoring
				#region increment the X (time) value for each dataseries
				newPoint.X = this.elapsedtime;
				float intervalF = (float)this.monitoringTimer.Interval;
				this.elapsedtime += (intervalF/1000);
				if(monitorState == MonitorStates.GRAPHING_PAUSED)
				{   //during pasued SDO graphing we still need tupdate the elapsed time - so get out here
					this.monitoringTimer.Enabled = true;
					return;
				}
				#endregion
				#region transform Graphics
				gScreen.SetClip(ChartClipAreaScreen);
				gScreen.RotateTransform(180); 
				gScreen.TranslateTransform(-ChartOriginScreen.X, ChartOriginScreen.Y);
				gScreen.ScaleTransform(-1, 1);
				gScreen.TranslateTransform(0, -(this.ClientRectangle.Height));  
				#endregion transform Graphics
				ArrayList itemsToRemove = new ArrayList();
                foreach (ODItemAndNode subAndNode in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
				{
                    int rowIndex = sysInfo.CANcomms.VCI.OdItemsBeingMonitored.IndexOf(subAndNode);
                    int dataIndex = sysInfo.CANcomms.VCI.OdItemsBeingMonitored.IndexOf(subAndNode);


					CANopenDataType datatype = (CANopenDataType) subAndNode.ODparam.dataType;
					feedback = subAndNode.node.readODValue(subAndNode.ODparam);
					if(feedback == DIFeedbackCode.DISuccess)
					{
						newPoint.Y = (float) subAndNode.ODparam.currentValue;
						if(subAndNode.ODparam.scaling != 1)
						{
							newPoint.Y = (float)(newPoint.Y * subAndNode.ODparam.scaling);
						}
						subAndNode.ODparam.screendataPoints.Add(newPoint);
						#region updata prevScreenPoint
						if(subAndNode.ODparam.screendataPoints.Count>1)
						{
							prevScreenPoint = (PointF) subAndNode.ODparam.screendataPoints[(subAndNode.ODparam.screendataPoints.Count-2)];
						}
						else
						{
							prevScreenPoint = (PointF) subAndNode.ODparam.screendataPoints[0];  //make last point the same as the new point
						}
						#endregion
						if(this.monStore.graph.ShowDataMarkers == true)
						{
							#region drawline
							if((datatype == CANopenDataType.BOOLEAN)
								|| (subAndNode.ODparam.bitSplit != null))
							{
								//draw a digital type plot
								gScreen.DrawLine(this.colouredPens[rowIndex], 
									(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);

								gScreen.DrawLine(this.colouredPens[rowIndex], 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
							}
							else
							{
								gScreen.DrawLine(this.colouredPens[rowIndex], 
									(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
							}
							#endregion drawline
						}
						else
						{
							#region drawine with no markers
							if((datatype == CANopenDataType.BOOLEAN)
								|| (subAndNode.ODparam.bitSplit != null))
							{
								//draw a digital type plot
								gScreen.DrawLine(this.colouredPensNoMarker[rowIndex], 
									(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);

								gScreen.DrawLine(this.colouredPensNoMarker[rowIndex], 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
							}
							else
							{
								gScreen.DrawLine(this.colouredPensNoMarker[rowIndex], 
									(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
							}
							#endregion drawine with no markers
						}
					}
					else
					{
						itemsToRemove.Add(subAndNode);
					}
				}
				#region remove transform graphics
				gScreen.RotateTransform(180); 
				gScreen.TranslateTransform(ChartOriginScreen.X,ChartOriginScreen.Y);
				gScreen.ScaleTransform(-1, 1);
				gScreen.TranslateTransform(0, -(this.ClientRectangle.Height));
				gScreen.SetClip(this.ClientRectangle);
				#endregion remove transform graphics
				if(itemsToRemove.Count>0)
				{
					foreach(ODItemAndNode itemToGo in itemsToRemove)
					{
                        sysInfo.CANcomms.VCI.OdItemsBeingMonitored.Remove(itemToGo);
						SystemInfo.errorSB.Append("\nFailed to monitor " + itemToGo.ODparam.parameterName + " removed from list");
					}

                    numOfItemsToplot = sysInfo.CANcomms.VCI.OdItemsBeingMonitored.Count;
					this.updateLegendPanel();

                    if (sysInfo.CANcomms.VCI.OdItemsBeingMonitored.Count <= 0)
					{
						SystemInfo.errorSB.Append("\n\n No items can be monitored. Close this screen and check connections and bus traffic");
						this.monitoringTimer.Enabled = false; //no point in continuing
					}
					else
					{
						this.monitoringTimer.Enabled = true;
					}
					sysInfo.displayErrorFeedbackToUser("Some Items no monitorable\n");
					return; //return here so if we want to switch the timer off we can
				}
				#endregion SDO Monitoring
			}
			else //we can use calibrated graphing
			{ 
				#region PDO monitoring (calibrated graphing)
                foreach (ODItemAndNode subAndNode in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
				{
                    int rowIndex = sysInfo.CANcomms.VCI.OdItemsBeingMonitored.IndexOf(subAndNode);
					if(subAndNode.ODparam.measuredDataPoints.Count>0)
					{
						DataPoint firstDataPt = (DataPoint) subAndNode.ODparam.measuredDataPoints[0];
						while(subAndNode.ODparam.lastPlottedPtIndex< subAndNode.ODparam.measuredDataPoints.Count -1)
						{  //while we still have measured data points not converted to screen Points
							DataPoint thisDataPt = (DataPoint) subAndNode.ODparam.measuredDataPoints[subAndNode.ODparam.lastPlottedPtIndex];
							#region calculate an display achieved interval for this ODitem
							float lastInterval = 0;
                            if ((subAndNode.ODparam.measuredDataPoints.Count > 1) && (subAndNode.ODparam.lastPlottedPtIndex > 0))
                            {
                                DataPoint prevDataPt = (DataPoint)subAndNode.ODparam.measuredDataPoints[subAndNode.ODparam.lastPlottedPtIndex - 1];
                                //Resolution is in s so multipy by 1000 for ms
                                lastInterval = ((long)((thisDataPt.timeStamp - prevDataPt.timeStamp)) * 1000) / sysInfo.CANcomms.VCI.TimeStampResolution;
                            }
							this.intervalAchievedLabel.Text = "Interval achieved: " + lastInterval.ToString("G5") + "ms";
							#endregion calculate an display achieved interval for this ODitem
							#region calculate screen X coordinate
                            newPoint.X = ((float)((thisDataPt.timeStamp - firstDataPt.timeStamp)) / sysInfo.CANcomms.VCI.TimeStampResolution);
							#endregion calculate screen X coordinate
							#region calculate the screen Y coordinate
							if(subAndNode.ODparam.scaling != 1)
							{
								newPoint.Y = (float) (thisDataPt.measuredValue * subAndNode.ODparam.scaling);
							}
							else  //no need to apply scaling 
							{
								newPoint.Y = thisDataPt.measuredValue;
							}
							#endregion calculate the screen Y coordinate
							#region add screen point to the list for the affected odItem
							subAndNode.ODparam.screendataPoints.Add(newPoint);
							if(subAndNode.ODparam.screendataPoints.Count<2)
							{
								prevScreenPoint = (PointF) subAndNode.ODparam.screendataPoints[0];  //make last point the same as the new point
							}
							else
							{
								prevScreenPoint = (PointF) subAndNode.ODparam.screendataPoints[(subAndNode.ODparam.screendataPoints.Count-2)];
							}
							#endregion  add screen point to the list for the affected odItem
							#region transform Graphics = after text write to screen
							gScreen.SetClip(ChartClipAreaScreen);
							gScreen.RotateTransform(180); 
							gScreen.TranslateTransform(-ChartOriginScreen.X, ChartOriginScreen.Y);
							gScreen.ScaleTransform(-1, 1);
							gScreen.TranslateTransform(0, -(this.ClientRectangle.Height));
							#endregion transform Graphics
							CANopenDataType datatype = (CANopenDataType) subAndNode.ODparam.dataType;
							#region draw screen line to this point and data markers (if required)
							if(this.monStore.graph.ShowDataMarkers == true)
							{
								#region drawline
								if((datatype == CANopenDataType.BOOLEAN)
									|| (subAndNode.ODparam.bitSplit != null))
								{
									//draw a digital type plot
									gScreen.DrawLine(this.colouredPens[rowIndex], 
										(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
										(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
										(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);

									gScreen.DrawLine(this.colouredPens[rowIndex], 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen,
										(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
										(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
								}
								else
								{
									gScreen.DrawLine(this.colouredPens[rowIndex], 
										(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
										(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
										(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
								}
								#endregion drawline
							}
							else
							{
								#region drawine with no markers
								if((datatype == CANopenDataType.BOOLEAN)
									|| (subAndNode.ODparam.bitSplit != null))
								{
									//draw a digital type plot
									gScreen.DrawLine(this.colouredPensNoMarker[rowIndex], 
										(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
										(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
										(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);

									gScreen.DrawLine(this.colouredPensNoMarker[rowIndex], 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen,
										(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
										(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
								}
								else
								{
									gScreen.DrawLine(this.colouredPensNoMarker[rowIndex], 
										(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
										(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
										(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
								}
								#endregion drawine with no markers
							}
							#endregion draw screen line to this point and data markers (if required)
							#region remove transform graphics
							gScreen.RotateTransform(180); 
							gScreen.TranslateTransform(ChartOriginScreen.X,ChartOriginScreen.Y);
							gScreen.ScaleTransform(-1, 1);
							gScreen.TranslateTransform(0, -(this.ClientRectangle.Height));
							gScreen.SetClip(this.ClientRectangle);
							#endregion remove transform graphics
							subAndNode.ODparam.lastPlottedPtIndex++;
						}
					}
				}
				#endregion PDO monitoring (calibrated graphing)	
			}
			#endregion data graphing
			this.monitoringTimer.Enabled = true;
		}
		#endregion timer elapsed handlers

		#region Graphing methods
		private void initialiseGraphicsParams()
		{
			CreateGraphicsTools();
			calculateInitialYAxisMaxAnMix();
			calculateInitialXAxisMaxAndMin();
			calcLabelSizeScreen();
			initialiseUserGraphEntryPaneXAxis();
			initialiseUserGraphEntryPaneYAxisandTitles();
			updateLegendPanel();
		}
		private void CreateGraphicsTools()
		{
			gScreen = this.CreateGraphics();//create the Graphics object to cover this clientRectangle
			CustomLineCap [] myCustomCaps = new CustomLineCap[10];

			#region Brushes and Pens
			#region create graphics paths for the custom end styles (datamarkers)
			#region filled and non-filled rectangles
			GraphicsPath pathfillRect = new GraphicsPath(); 
			Rectangle tempRect = new Rectangle(-3,-3,6,6);
			pathfillRect.AddRectangle(tempRect);
			myCustomCaps[0] = new CustomLineCap(pathfillRect, null); 
			tempRect = new Rectangle(-3,-3,6,6);
			pathfillRect.AddRectangle(tempRect);
			myCustomCaps[1] = new CustomLineCap(null, pathfillRect); 
			#endregion filled and non-filled rectangles

			#region hollow bow tie 
			GraphicsPath bowTie90 = new GraphicsPath(); 
			bowTie90.AddLine(new Point(-3,-3), new Point(3,-3)); 
			bowTie90.AddLine(new Point(3,-3), new Point(-3,3)); 
			bowTie90.AddLine(new Point(-3,3), new Point(3,3)); 
			bowTie90.AddLine(new Point(3,3), new Point(-3,-3)); 
			myCustomCaps[3] = new CustomLineCap(null, bowTie90); 

			GraphicsPath bowTiefilled = new GraphicsPath(); 
			bowTiefilled.AddLine(new Point(-4,-4), new Point(4,-4)); 
			bowTiefilled.AddLine(new Point(4,-4), new Point(-4,4)); 
			bowTiefilled.AddLine(new Point(-4,4), new Point(4,4)); 
			bowTiefilled.AddLine(new Point(4,4), new Point(-4,-4)); 
			myCustomCaps[2] = new CustomLineCap(bowTiefilled, null); 
			#endregion bow tie

			#region diamond
			GraphicsPath diamondPath = new GraphicsPath();
			diamondPath.AddLine(new Point(0,-4), new Point(4, 0)); 
			diamondPath.AddLine(new Point(4, 0), new Point(0,4)); 
			diamondPath.AddLine(new Point(0,4), new Point(-4, 0)); 
			diamondPath.AddLine(new Point(-4, 0), new Point(0, -4)); 
			myCustomCaps[4] = new CustomLineCap(diamondPath, null); 
			GraphicsPath diamondPath2 = new GraphicsPath();
			diamondPath2.AddLine(new Point(0,-5), new Point(5, 0)); 
			diamondPath2.AddLine(new Point(5, 0), new Point(0,5)); 
			diamondPath2.AddLine(new Point(0,5), new Point(-5, 0)); 
			diamondPath2.AddLine(new Point(-5, 0), new Point(0, -5)); 

			myCustomCaps[5] = new CustomLineCap(null, diamondPath2); 
			#endregion diamond

			#region circle
			GraphicsPath circlePath = new GraphicsPath();
			circlePath.AddEllipse(-4, -4, 8, 8);
			myCustomCaps[6] = new CustomLineCap(circlePath, null); 
			circlePath.AddEllipse(-4, -4, 8, 8);
			myCustomCaps[7] = new CustomLineCap(null, circlePath); 
			#endregion circle
			
			#region triangle
			GraphicsPath bowTie90Path = new GraphicsPath(); 
			bowTie90Path.AddLine(new Point(0,0), new Point(4,4)); 
			bowTie90Path.AddLine(new Point(4,4), new Point(4,-4)); 
			bowTie90Path.AddLine(new Point(4,-4), new Point(0,0)); 
			myCustomCaps[8] = new CustomLineCap(null, bowTie90Path); 
			GraphicsPath trianglePath = new GraphicsPath(); 
			trianglePath.AddLine(new Point(0,0), new Point(-4,4)); 
			trianglePath.AddLine(new Point(-4,-4), new Point(0,0)); 
			myCustomCaps[9] = new CustomLineCap( null, trianglePath); 
			#endregion triangle

			GraphicsPath colouredPenPath = new GraphicsPath(); 
			Rectangle tempRect2 = new Rectangle(-1,-1,2,2);
			colouredPenPath.AddRectangle(tempRect2);
			CustomLineCap colouredEndCap = new CustomLineCap(colouredPenPath, null); 

			#endregion create graphics paths for the custom end styles (datamarkers)
			#region plot colours
			plotColours = new System.Drawing.Color[10];  //ceiling is currently 10
			plotColours[0] = Color.Blue;
			plotColours[1] = Color.Green;
			plotColours[2] = Color.Red;
			plotColours[3] = Color.DarkGray;
			plotColours[4] = Color.Brown;
			plotColours[5] = Color.Orange;
			plotColours[6] = Color.Turquoise;
			plotColours[7] = Color.Black;
			plotColours[8] = Color.DarkBlue;
			plotColours[9] = Color.Magenta;
			#endregion plot colours

			plotBrushes = new SolidBrush[this.numOfItemsToplot];
			colouredPens = new Pen[this.numOfItemsToplot];
			colouredPensNoMarker = new Pen[this.numOfItemsToplot];
			blackPens = new Pen[numOfItemsToplot];
			for(int i = 0;i<numOfItemsToplot;i++)
			{
				plotBrushes[i] = new SolidBrush(plotColours[i]);
				colouredPens[i] = new Pen(plotColours[i],1);
				colouredPens[i].CustomEndCap = colouredEndCap; 
				colouredPensNoMarker[i] = new Pen(plotColours[i],1);
				blackPens[i] = new Pen(Color.Black,1);
				blackPens[i].CustomEndCap = myCustomCaps[i]; 
			}
			#endregion Brushes and Pens
			#region Fonts
			axesLabelFont = new Font( "Arial", 10,	FontStyle.Regular);  
			mainLabelFont = new Font( "Arial", 12,	FontStyle.Bold);
			markFont = new Font( "Arial", 8);
			YAxisStringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
			#endregion Fonts
			axesPointsScreen = new PointF[3];
			prevScreenPoint = new PointF(0,0);
		}

		private void updateAxesParametersFromUpdatePanelVlaues()
		{
			#region update Graph from Update Panel
			string errorString = "";
			#region Y axis min value
			float minYBackup = this.monStore.graph.Yaxis.Min;
			try
			{
				this.monStore.graph.Yaxis.Min = float.Parse(this.YMinTB.Text, System.Globalization.NumberStyles.Any);
			}
			catch
			{
				errorString = "'" + this.YMinTB.Text +"' entry is invalid. \nIncorrect format";
			}
			this.errorProvider1.SetError(YMinTB, errorString);  //do this before  changing textBox text
			if(errorString != "")
			{
				this.monStore.graph.Yaxis.Min = minYBackup;  //reset 
				this.YMinTB.Text = minYBackup.ToString();  //and display old value
			}
			#endregion Y axis min value

			#region X axis min value
			errorString = "";
			float minXBackup = this.monStore.graph.Xaxis.Min;
			try
			{
				this.monStore.graph.Xaxis.Min = float.Parse(this.XMinTB.Text, System.Globalization.NumberStyles.Any);
			}
			catch
			{
				errorString = "'" + this.XMinTB.Text +"' entry is invalid. \nIncorrect format";
			}
			this.errorProvider1.SetError(XMinTB, errorString);  //do this before  changing textBox text
			if(errorString != "")
			{
				this.monStore.graph.Xaxis.Min = minXBackup;  //reset 
				this.XMinTB.Text = minXBackup.ToString();  //and display old value
			}
			#endregion X axis min value

			#region Y Axis Max value
			float maxYBackup = this.monStore.graph.Yaxis.Max;
			errorString = "";
			try
			{
				this.monStore.graph.Yaxis.Max = float.Parse(this.YMaxTB.Text, System.Globalization.NumberStyles.Any);
			}
			catch
			{
				errorString = "'" + this.YMaxTB.Text +"' entry is invalid. \nIncorrect format";
			}
			if(this.monStore.graph.Yaxis.Max <= this.monStore.graph.Yaxis.Min )
			{
				errorString = "'" + this.YMaxTB.Text +"' entry is invalid. \nMaximum must be greater than minimum";
			}
			this.errorProvider1.SetError(YMaxTB, errorString);  //do this before  changing textBox text
			if(errorString != "")
			{
				this.monStore.graph.Yaxis.Max = maxYBackup;  //reset 
				this.YMaxTB.Text = maxYBackup.ToString();  //and display old value
			}
			#endregion Y Axis Max value
			
			#region X axis maximum value
			float maxXBackup = this.monStore.graph.Xaxis.Max;
			errorString = "";
			try
			{
				this.monStore.graph.Xaxis.Max = float.Parse(this.XMaxTB.Text, System.Globalization.NumberStyles.Any);
			}
			catch
			{
				errorString = "'" + this.XMaxTB.Text +"' entry is invalid. \nIncorrect format";
			}
			if(this.monStore.graph.Xaxis.Max <=this.monStore.graph.Xaxis.Min)
			{
				errorString = "'" + this.XMaxTB.Text +"' entry is invalid. \nMaximum must be greater than minimum";
			}
			this.errorProvider1.SetError(XMaxTB, errorString);  //do this before  changing textBox text
			if(errorString != "")
			{
				this.monStore.graph.Xaxis.Max = maxXBackup;  //reset 
				this.XMaxTB.Text = maxXBackup.ToString();  //and display old value
			}
			#endregion X axis maximum value

			#region X axis divisions
			errorString = "";
			float XDivBackup = this.monStore.graph.Xaxis.DivValue;
			try
			{//System.Globalization.NumberStyles.Any ensure we accept other dcimal point formates eg the comma
				this.monStore.graph.Xaxis.DivValue = float.Parse(this.XDivTB.Text, System.Globalization.NumberStyles.Any);
			}
			catch
			{
				errorString = "'" + this.XDivTB.Text +"' entry is invalid. \nIncorrect format";
			}
			this.errorProvider1.SetError(XDivTB, errorString);  //do this before  changing textBox text
			if(errorString != "")
			{
				this.monStore.graph.Xaxis.DivValue = XDivBackup;  //reset 
				this.XDivTB.Text = XDivBackup.ToString();  //and display old value
			}
			#endregion X axis divisions

			#region Yaxis divisions
			errorString = "";
			float YdivBackup = this.monStore.graph.Yaxis.DivValue;
			try
			{
				this.monStore.graph.Yaxis.DivValue = float.Parse(this.YDivTB.Text, System.Globalization.NumberStyles.Any);
			}
			catch
			{
				errorString = "'" + this.YDivTB.Text +"' entry is invalid. \nIncorrect format";
			}
			this.errorProvider1.SetError(YDivTB, errorString);  //do this before  changing textBox text
			if(errorString != "")
			{
				this.monStore.graph.Yaxis.DivValue = YdivBackup;  //reset 
				this.YDivTB.Text = YdivBackup.ToString();  //and display old value
			}
			#endregion Yaxis divisions

			#region error handle min Y after max Y entered
			errorString = "";
			if(this.monStore.graph.Yaxis.Min>=this.monStore.graph.Yaxis.Max)
			{
				errorString = "'" + this.YMinTB.Text +"' entry is invalid. \nMinimum must be less than maximum";
			}
			this.errorProvider1.SetError(YMinTB, errorString);  //do this before  changing textBox text
			if(errorString != "")
			{
				this.monStore.graph.Yaxis.Min = minYBackup;  //reset 
				this.YMinTB.Text = minYBackup.ToString();  //and display old value
			}
			#endregion error handle min Y after max Y entered

			#region error handle min X after max X entered
			errorString = "";
			if(this.monStore.graph.Xaxis.Min>=this.monStore.graph.Xaxis.Max)
			{
				errorString = "'" + this.XMinTB.Text +"' entry is invalid. \nMinimum must be less than maximum";
			}
			if(this.monStore.graph.Xaxis.Min <0)
			{
				errorString = "'" + this.XMinTB.Text +"' entry is invalid. \nMinimum value is zero";
			}
			this.errorProvider1.SetError(XMinTB, errorString);  //do this before  changing textBox text
			if(errorString != "")
			{
				this.monStore.graph.Xaxis.Min = minXBackup;  //reset 
				this.XMinTB.Text = minXBackup.ToString();  //and display old value
			}
			#endregion error handle min X after max X entered

			#region errorhandle X Divisions after min and max checked
			errorString = "";
			if(this.monStore.graph.Xaxis.DivValue >= (this.monStore.graph.Xaxis.Max-this.monStore.graph.Xaxis.Min))
			{
				errorString = "'" + this.XDivTB.Text +"' entry is invalid. \nDivision must be less than axis range";
			}
			if(this.monStore.graph.Xaxis.DivValue<=0)
			{
				errorString = "'" + this.XDivTB.Text +"' entry is invalid. \nDivision must be greater than zero";
			}
			else if (((this.monStore.graph.Xaxis.Max-this.monStore.graph.Xaxis.Min)/this.monStore.graph.Xaxis.DivValue) >30)
			{
				errorString = "'" + this.XDivTB.Text +"' entry is invalid. \nMaximum of 30 divisions exceeded";
			}
			this.errorProvider1.SetError(XDivTB, errorString);  //do this before  changing textBox text
			if(errorString != "")
			{
				this.monStore.graph.Xaxis.DivValue = (this.monStore.graph.Xaxis.Max-this.monStore.graph.Xaxis.Min)/30;
				this.XDivTB.Text = this.monStore.graph.Xaxis.DivValue.ToString();
			}
			#endregion errorhandle X Divisions after min and max checked

			#region errorhandle Y Divisions after min and max checked
			errorString = "";
			if(this.monStore.graph.Yaxis.DivValue >= (this.monStore.graph.Yaxis.Max-this.monStore.graph.Yaxis.Min))
			{
				errorString = "'" + this.YDivTB.Text +"' entry is invalid. \nDivision must be less than axis range";
			}
			if(this.monStore.graph.Yaxis.DivValue<=0)
			{
				errorString = "'" + this.YDivTB.Text +"' entry is invalid. \nDivision must be greater than zero";
			}
			else if( ((this.monStore.graph.Yaxis.Max-this.monStore.graph.Yaxis.Min)/this.monStore.graph.Yaxis.DivValue) >25)
			{
				errorString = "'" + this.YDivTB.Text +"' entry is invalid. \nMaximum of 25 divisions exceeded";
			}
			this.errorProvider1.SetError(YDivTB, errorString);  //do this before  changing textBox text
			if(errorString != "")
			{
				this.monStore.graph.Yaxis.DivValue = (this.monStore.graph.Yaxis.Max-this.monStore.graph.Yaxis.Min)/20;  //to ensure not too many divs
				this.YDivTB.Text = this.monStore.graph.Yaxis.DivValue.ToString();

			}
			#endregion errorhandle Y Divisions after min and max checked

			#region updat elable sand markers in Mon Store
			this.monStore.graph.ShowDataMarkers = this.MarkersCB.Checked;
			this.monStore.graph.Yaxis.AxisLabel = this.YAxLabelTB.Text;
			this.monStore.graph.Xaxis.AxisLabel = this.XAxLabelTB.Text;
			this.monStore.graph.MainTitle = this.MainTitleTB.Text;
			this.monStore.graph.ShowDataMarkers = this.MarkersCB.Checked;
			this.monStore.graph.Xaxis.LinesVisible = this.XAxisGridCB.Checked;
			this.monStore.graph.Yaxis.LinesVisible = this.YAxisGridCB.Checked;
			#endregion updat elable sand markers in Mon Store
			#endregion update Graph from Update Panel
		}
		private void initialiseUserGraphEntryPaneXAxis()
		{
			this.XMinTB.Text = this.monStore.graph.Xaxis.Min.ToString();
			this.XMinTB.ForeColor = SCCorpStyle.dgForeColour;
			this.XMaxTB.Text = this.monStore.graph.Xaxis.Max.ToString();
			this.XMaxTB.ForeColor = SCCorpStyle.dgForeColour;
			this.XDivTB.Text = this.monStore.graph.Xaxis.DivValue.ToString();
			this.XDivTB.ForeColor = SCCorpStyle.dgForeColour;
		}
		private void initialiseUserGraphEntryPaneYAxisandTitles()
		{
			this.YMinTB.Text = this.monStore.graph.Yaxis.Min.ToString();
			this.YMinTB.ForeColor = SCCorpStyle.dgForeColour;
			this.YMaxTB.Text = this.monStore.graph.Yaxis.Max.ToString();
			this.YMaxTB.ForeColor = SCCorpStyle.dgForeColour;
			this.YDivTB.Text = this.monStore.graph.Yaxis.DivValue.ToString();
			this.YDivTB.ForeColor = SCCorpStyle.dgForeColour;

			this.graphUpdatePanel.BackColor = SCCorpStyle.dgBackColour;
			this.graphUpdatePanel.ForeColor = SCCorpStyle.dgForeColour;
		}

		private void calculateInitialYAxisMaxAnMix()
		{
			foreach (DataRowView row in MAIN_WINDOW.DWdataset.Tables[MAIN_WINDOW.GraphTblIndex].DefaultView)
			{
				ODItemData odSub = (ODItemData)  row[(int) (TblCols.odSub)];
				if(odSub.subNumber == -1)
				{
					continue;  //skip any header type rows
				}
				switch (odSub.format)
				{
					case SevconNumberFormat.BASE10:
						#region BASE10
						try
						{
                            this.monStore.graph.Yaxis.Max = System.Math.Max(this.monStore.graph.Yaxis.Max, ((float)(odSub.highLimit * odSub.scaling)));
                            this.monStore.graph.Yaxis.Min = System.Math.Min(this.monStore.graph.Yaxis.Min, ((float)(odSub.lowLimit * odSub.scaling)));
						}
						catch (Exception ex)
						{
#if DEBUG
							SystemInfo.errorSB.Append("\nAxis Min/Max calculation. Exception seen: " + ex.Message);
#endif
						}
						break;
						#endregion BASE10

					case SevconNumberFormat.BASE16:
						#region BASE16
						this.monStore.graph.Yaxis.Max = System.Math.Max(this.monStore.graph.Yaxis.Max, ((float)(odSub.highLimit * odSub.scaling)));
						this.monStore.graph.Yaxis.Min = System.Math.Min(this.monStore.graph.Yaxis.Min, ((float)(odSub.lowLimit * odSub.scaling)));
						break;
						#endregion BASE16

					case SevconNumberFormat.SPECIAL:
						#region SPECIAL FORMAT
						//split the format string into its constituent strings 
						string [] split = odSub.formatList.Split(':');
						bool minFound = false, maxFound = false;
						for(int i=0;i<split.Length;i++)
						{
							#region attemt to get a numeric value from the enumeration string
							if((minFound == false) && (split[i].IndexOf(row[(int)(TblCols.lowVal)].ToString()) != -1))
							{
								minFound = true;
								//extract the numeric part of the format string (last 4 chars)
								string valStr = split[i].Substring(0, split[i].IndexOf('='));
								this.monStore.graph.Yaxis.Min = System.Math.Min(this.monStore.graph.Yaxis.Min, System.Convert.ToInt32(valStr));
							}
							if ((maxFound == false) && (split[i].IndexOf(row[(int)(TblCols.highVal)].ToString()) != -1))
							{
								maxFound = true;
								//extract the numeric part of the format string (last 4 chars)
								string valStr = split[i].Substring(0, split[i].IndexOf('='));
								this.monStore.graph.Yaxis.Max = System.Math.Max(this.monStore.graph.Yaxis.Max,System.Convert.ToInt32(valStr));
							}
							if((minFound == true) && (maxFound == true))
							{
								break;  //leave this for loop
							}
							#endregion attemt to get a numeric value from the enumeration string
						}
						if(minFound == false)
						{
							#region try a sraight numeric conversion
							try
							{
								this.monStore.graph.Yaxis.Min = System.Math.Min(this.monStore.graph.Yaxis.Min,System.Convert.ToInt64(row[(int)(TblCols.lowVal)].ToString()));
							}
							catch
							{}//do nothing
							#endregion try a sraight numeric conversion
						}
						if(maxFound == false)
						{
							#region try a sraight numeric conversion
							//try a sraight numeric conversion
							try
							{
								this.monStore.graph.Yaxis.Max = System.Math.Max(this.monStore.graph.Yaxis.Max, System.Convert.ToInt64(row[(int)(TblCols.highVal)].ToString()));
							}
							catch
							{} //do nothing
							#endregion try a sraight numeric conversion
						}
						break;
						#endregion SPECIAL FORMAT

					default:
						#region error condition
						SystemInfo.errorSB.Append("\nError non-numerical parameter - check RowFilter");
						this.Close();
						break;
						#endregion error condition
				}
			}
			if((this.monStore.graph.Yaxis.Max-this.monStore.graph.Yaxis.Min) ==0)  //prevent any possibility of divide by zero
			{
				this.monStore.graph.Yaxis.Max = 1;
				this.monStore.graph.Yaxis.Min = 0;
			}
			this.monStore.graph.Yaxis.DivValue = (this.monStore.graph.Yaxis.Max-this.monStore.graph.Yaxis.Min)/20;
			this.YDivTB.Text = this.monStore.graph.Yaxis.DivValue.ToString();
			this.YMaxTB.Text = this.monStore.graph.Yaxis.Max.ToString();
			this.YMinTB.Text = this.monStore.graph.Yaxis.Min.ToString();
		}

		private void calculateInitialXAxisMaxAndMin()
		{
			//set up X axis on basis of current monitoringTimer intervals
			//this.elapsedtime = 0; 
			float intervalF= 0F;
			if(this.CalibratedMonitoringPossible == false)
			{
				intervalF = (float) (this.monitoringTimer.Interval);
			}
			else
			{
				intervalF = (float) (this.userSelectedTimeInterval);
			}
			
			this.monStore.graph.Xaxis.Min = 0;
			this.monStore.graph.Xaxis.Max = (intervalF/1000) * initialNumDataPoints;//allow 200 data points to fit on screen 
			this.monStore.graph.Xaxis.DivValue = (int) (this.monStore.graph.Xaxis.Max/(initialNumDataPoints/20));//will give us 20 X axis markers
			this.XMaxTB.Text = this.monStore.graph.Xaxis.Max.ToString();
			this.XMinTB.Text = this.monStore.graph.Xaxis.Min.ToString();
			this.XDivTB.Text = this.monStore.graph.Xaxis.DivValue.ToString();
		}
		private void calcLabelSizeScreen()
		{
			YaxisLabelSizeScreen = gScreen.MeasureString(this.YAxLabelTB.Text, axesLabelFont, 30, YAxisStringFormat);
			XAxisLabelSizeScreen = gScreen.MeasureString(this.XAxLabelTB.Text,axesLabelFont);
			MainLabelSizeScreen = gScreen.MeasureString(this.MainTitleTB.Text,mainLabelFont);
		}
		private void calculateAxesScreen()
		{
			ushort AxisTickLengthInPixels = 10;
			ushort YAxisTickLabelsAllowanceInPixels = 90, XAxisTickLabelsAllowanceInPixels =30;
			positionLabelsScreen();
			
			#region Salient points in screen coordinates
			//calculate all the salient chart points in terms of creen coordinates
			screenMaxX = (float) this.ClientRectangle.Right - rightOffset;
			screenMaxY = (float) this.ClientRectangle.Top+topOffset; 
			screenMinX = (float) (this.YAxeslabelBoundsScreen.Right + YAxisTickLabelsAllowanceInPixels); 
			screenMinY = (float) (this.XAxeslabelBoundsScreen.Top - XAxisTickLabelsAllowanceInPixels); 
			axesPointsScreen[0] = new PointF(screenMinX,screenMaxY );  //topYaxis
			axesPointsScreen[1] = new PointF(screenMinX,screenMinY );  //BottonYaxis - Left Xaxis
			axesPointsScreen[2] = new PointF(screenMaxX, screenMinY);  //BottonYaxis - Left Xaxis
			ChartOriginScreen = new PointF(screenMinX, (float) (this.ClientRectangle.Height-screenMinY));
			ChartClipAreaScreen = new RectangleF(screenMinX,screenMaxY,(screenMaxX-screenMinX),(screenMinY-screenMaxY));
			#endregion salient points in screen co-ordinates

			#region X Axis scaling
			//calculate points array for divisions
			float tempy = ((this.monStore.graph.Xaxis.Max-this.monStore.graph.Xaxis.Min)/this.monStore.graph.Xaxis.DivValue);  //to ensure correct maths - was rounding incorrectly
			numXDivs = (ushort) Math.Max(System.Convert.ToUInt16(tempy), (ushort)1);// 2 ensures that we alway have a marker at X Axis min point
			XDivInPixels = (screenMaxX- screenMinX)/tempy;

			//check if we need an end marker - 
			//we only need it it if isn't the same value as the last div point
			this.maxXDivCoincidentWithAxisMax = false;
			if( (this.monStore.graph.Xaxis.DivValue * numXDivs) == this.monStore.graph.Xaxis.Max)
			{
				this.maxXDivCoincidentWithAxisMax = true; //used later for print Axis calcs
				#region last division point is coincident with X axis max
				XDivStartPointsScreen = new PointF[numXDivs+ 1]; //+1 for min marker
				XDivEndPointsScreen = new PointF[numXDivs + 1]; 
				XAxisDivLabels = new string[numXDivs + 1];
				#region calulate division points and labels
				//the last point and scale max are coincident - so 'lose' the screen max
				for(int i = 0;i<XDivStartPointsScreen.Length;i++) 
				{
					XDivStartPointsScreen[i].X = (int) ((i * XDivInPixels) + screenMinX);
					XDivEndPointsScreen[i].X = XDivStartPointsScreen[i].X;
					XDivStartPointsScreen[i].Y = screenMinY;
					XDivEndPointsScreen[i].Y = screenMinY+AxisTickLengthInPixels;
					XAxisDivLabels[i] = (this.monStore.graph.Xaxis.DivValue * i).ToString("G5");
				}
				#endregion calulate division points and labels
				#endregion last division point is coincident with X axis max
			}
			else
			{
				#region last division point is NOT coincident with X axis max
				XDivStartPointsScreen = new PointF[numXDivs+ 2];  //+2 to include min + max value marker
				XDivEndPointsScreen = new PointF[numXDivs + 2]; 
				XAxisDivLabels = new string[numXDivs + 2];
				#region calulate division points and labels
				for(int i = 0;i<XDivStartPointsScreen.Length-1;i++) 
				{
					XDivStartPointsScreen[i].X = (int) ((i * XDivInPixels) + screenMinX);
					XDivEndPointsScreen[i].X = XDivStartPointsScreen[i].X;
					XDivStartPointsScreen[i].Y = screenMinY;
					XDivEndPointsScreen[i].Y = screenMinY+AxisTickLengthInPixels;
                    XAxisDivLabels[i] = (this.monStore.graph.Xaxis.Min + (this.monStore.graph.Xaxis.DivValue * i)).ToString("G5");
				}
				#endregion calulate division points and labels
				#region calculate point and label for max X axis
				//the last one needs to be at the max value
				XDivStartPointsScreen[XDivStartPointsScreen.Length-1].X = this.screenMaxX;
				XDivEndPointsScreen[XDivStartPointsScreen.Length-1].X = this.screenMaxX;
				XDivStartPointsScreen[XDivStartPointsScreen.Length-1].Y = screenMinY;
				XDivEndPointsScreen[XDivStartPointsScreen.Length-1].Y = screenMinY+AxisTickLengthInPixels;
				XAxisDivLabels[XDivStartPointsScreen.Length-1] = this.monStore.graph.Xaxis.Max.ToString("G5");
				#endregion calculate point for max X axis
				#endregion last division point is NOT coincident with X axis max
			}
			//scaling to convert value to length in pixels 
			XscalingScreen = ((XDivStartPointsScreen[XDivStartPointsScreen.Length-1].X) -(XDivStartPointsScreen[0].X))/(this.monStore.graph.Xaxis.Max-this.monStore.graph.Xaxis.Min);  
			#endregion

			#region Y Axis scaling
			if(this.monStore.graph.Yaxis.Min<0)
			{//we need to scale up and down form the zero line
				#region Minimum Y is negative - measure Divs form zero line
				float negProportionOfAxis =  -1 * ((this.monStore.graph.Yaxis.Min) /(this.monStore.graph.Yaxis.Max - this.monStore.graph.Yaxis.Min));
				float posProportionOfAxis = 1- negProportionOfAxis;

				//find the zero level 
				screenZeroY =  screenMinY - ((screenMinY - screenMaxY) * negProportionOfAxis);
				
				//create the Y axis divs i two parts - positive and negative
				int totalNumMarkers = 1; //the zero line
				maxYDivCoincidentWithAxisMax = false;
				minYDivCoincidentWithAxisMin = false;
				float posTempy = ((this.monStore.graph.Yaxis.Max)/this.monStore.graph.Yaxis.DivValue);
				int numPosYDivs = (int) Math.Max( Math.Floor((double)posTempy),1);  //min of 1 division
				totalNumMarkers += numPosYDivs;
				YDivInPixels = (screenZeroY - screenMaxY)/posTempy;
				if( ((this.monStore.graph.Yaxis.DivValue * numPosYDivs)) == this.monStore.graph.Yaxis.Max)
				{
					maxYDivCoincidentWithAxisMax = true;
				}
				else
				{
					totalNumMarkers += 1;  //add a marker at the Axis max 
				}
				float negTempy = ((-1 * this.monStore.graph.Yaxis.Min)/this.monStore.graph.Yaxis.DivValue);
				int numNegYDivs = (int) Math.Max( Math.Floor((double)negTempy),1);  //min of 1 division
				zeroIndex = numNegYDivs; 
				totalNumMarkers += numNegYDivs;
				YDivInPixels = ( screenMinY - screenZeroY)/negTempy;
				if((this.monStore.graph.Yaxis.DivValue * numNegYDivs)  ==  (-1 * this.monStore.graph.Yaxis.Min))
				{
					minYDivCoincidentWithAxisMin = true;
				}
				else
				{
					totalNumMarkers += 1;  //add a marker at the Axis min 
					zeroIndex +=1;
				}
				YDivStartPointsScreen = new PointF[totalNumMarkers]; //+1 for min marker
				YDivEndPointsScreen = new PointF[totalNumMarkers]; 
				YAxisDivLabels = new string[totalNumMarkers];
				
				#region add the negative markers
				if(minYDivCoincidentWithAxisMin == true)
				{
					#region all points up to zero index
					for(int i = 0;i<zeroIndex;i++)
					{
						YDivStartPointsScreen[i].Y = (int) (screenZeroY + ((zeroIndex - i) * YDivInPixels)) ;
						YDivEndPointsScreen[i].Y = YDivStartPointsScreen[i].Y;
						YDivStartPointsScreen[i].X = screenMinX;
						YDivEndPointsScreen[i].X = screenMinX -AxisTickLengthInPixels;
						float labelValue  =  -1 * (this.monStore.graph.Yaxis.DivValue * (zeroIndex - i));
						this.YAxisDivLabels[i] = labelValue.ToString("G5");
					}
					#endregion all points up to zero index
				}
				else
				{
					#region min point manually set to axis min
					YDivStartPointsScreen[0].Y = (int) screenMinY ;
					YDivEndPointsScreen[0].Y = (int) screenMinY;
					YDivStartPointsScreen[0].X = screenMinX;
					YDivEndPointsScreen[0].X = screenMinX -AxisTickLengthInPixels;
					this.YAxisDivLabels[0] = this.monStore.graph.Yaxis.Min.ToString("G5");
					#endregion min point manually set to axis min
					#region remaining points up to zero index
					for(int i = 1;i<zeroIndex;i++)
					{
						YDivStartPointsScreen[i].Y = (int) (screenZeroY + ((zeroIndex - i) * YDivInPixels)) ;
						YDivEndPointsScreen[i].Y = YDivStartPointsScreen[i].Y;
						YDivStartPointsScreen[i].X = screenMinX;
						YDivEndPointsScreen[i].X = screenMinX -AxisTickLengthInPixels;
						float labelValue  =  -1 * (this.monStore.graph.Yaxis.DivValue * (zeroIndex - i));
						this.YAxisDivLabels[i] = labelValue.ToString("G5");
					}
					#endregion remaining points up to zero index
				}
				#endregion add the negative markers

				#region add the zero marker
				YDivStartPointsScreen[zeroIndex].Y = (int) screenZeroY ;
				YDivEndPointsScreen[zeroIndex].Y = YDivStartPointsScreen[zeroIndex].Y;
				YDivStartPointsScreen[zeroIndex].X = screenMinX;
				YDivEndPointsScreen[zeroIndex].X = screenMinX -AxisTickLengthInPixels;
				this.YAxisDivLabels[zeroIndex] = "0";
				#endregion add the zero marker

				#region add the positibve markers
				if(maxYDivCoincidentWithAxisMax == true)
				{
					for(int i = zeroIndex + 1;i<YDivStartPointsScreen.Length;i++)
					{
						YDivStartPointsScreen[i].Y = (int) (screenZeroY - ((i - zeroIndex)* YDivInPixels)) ;
						YDivEndPointsScreen[i].Y = YDivStartPointsScreen[i].Y;
						YDivStartPointsScreen[i].X = screenMinX;
						YDivEndPointsScreen[i].X = screenMinX -AxisTickLengthInPixels;
						float labelValue  = (this.monStore.graph.Yaxis.DivValue * (i-zeroIndex));
						this.YAxisDivLabels[i] = labelValue.ToString("G5");
					}
				}
				else
				{
					for(int i = zeroIndex + 1;i<YDivStartPointsScreen.Length-1;i++)
					{
						YDivStartPointsScreen[i].Y = (int) (screenZeroY - ((i - zeroIndex) * YDivInPixels)) ;
						YDivEndPointsScreen[i].Y = YDivStartPointsScreen[i].Y;
						YDivStartPointsScreen[i].X = screenMinX;
						YDivEndPointsScreen[i].X = screenMinX -AxisTickLengthInPixels;
						float labelValue  =  (this.monStore.graph.Yaxis.DivValue * (i - zeroIndex));
						this.YAxisDivLabels[i] = labelValue.ToString("G5");
					}
					YDivStartPointsScreen[YDivStartPointsScreen.Length-1].Y = (int) screenMaxY;
					YDivEndPointsScreen[YDivStartPointsScreen.Length-1].Y = (int) screenMaxY;
					YDivStartPointsScreen[YDivStartPointsScreen.Length-1].X = screenMinX;
					YDivEndPointsScreen[YDivStartPointsScreen.Length-1].X = screenMinX -AxisTickLengthInPixels;
					this.YAxisDivLabels[YDivStartPointsScreen.Length-1] = this.monStore.graph.Yaxis.Max.ToString("G5");
				}
				#endregion add the positibve markers

				#endregion Minimum Y is negative - measure Divs form zero line
			}
			else
			{
				tempy = ((this.monStore.graph.Yaxis.Max-this.monStore.graph.Yaxis.Min)/this.monStore.graph.Yaxis.DivValue);
				numYDivs = (ushort) Math.Max( System.Convert.ToUInt16(tempy),(ushort)1);  //min of 1 division
				YDivInPixels = ((screenMinY- screenMaxY)/tempy);
				if( (this.monStore.graph.Yaxis.DivValue * numYDivs) == this.monStore.graph.Yaxis.Max - this.monStore.graph.Yaxis.Min)
				{
					maxYDivCoincidentWithAxisMax = true;
					#region last division point is coincident with Y axis max
					YDivStartPointsScreen = new PointF[numYDivs+ 1]; //+1 for min marker
					YDivEndPointsScreen = new PointF[numYDivs + 1]; 
					YAxisDivLabels = new string[numYDivs + 1];
					#region calulate division points and labels
					#endregion calulate division points and labels
					for(int i = 0;i<YDivStartPointsScreen.Length;i++)
					{
						float nextLabelVal = (this.monStore.graph.Yaxis.DivValue * (i+1)) + this.monStore.graph.Yaxis.Min;
						float labelValue  = (this.monStore.graph.Yaxis.DivValue * i) + this.monStore.graph.Yaxis.Min;
						if((labelValue<0.00001) && (labelValue>-0.00001))
						{
							labelValue = 0; //rounding for nice screen dispaly
						}
						YDivStartPointsScreen[i].Y = (int) (screenMinY - (i * YDivInPixels)) ;
						YDivEndPointsScreen[i].Y = YDivStartPointsScreen[i].Y;
						YDivStartPointsScreen[i].X = screenMinX;
						YDivEndPointsScreen[i].X = screenMinX -AxisTickLengthInPixels;
						this.YAxisDivLabels[i] = labelValue.ToString("G5");
					}
					#endregion last division point is coincident with Y axis max
				}
				else
				{
					#region last division point is NOT coincident with Y axis max
					YDivStartPointsScreen = new PointF[numYDivs+ 2]; //+1 for min marker
					YDivEndPointsScreen = new PointF[numYDivs + 2]; 
					YAxisDivLabels = new string[numYDivs + 2];
					for(int i = 0;i<YDivStartPointsScreen.Length-1;i++)
					{
						YDivStartPointsScreen[i].Y = (int) (screenMinY - (i * YDivInPixels)) ;
						YDivEndPointsScreen[i].Y = YDivStartPointsScreen[i].Y;
						YDivStartPointsScreen[i].X = screenMinX;
						YDivEndPointsScreen[i].X = screenMinX -AxisTickLengthInPixels;
						float labelValue  = (this.monStore.graph.Yaxis.DivValue * i)  + this.monStore.graph.Yaxis.Min;;
						if((labelValue<0.00001) && (labelValue>-0.00001))
						{
							labelValue = 0; //rounding for nice screen dispaly
						}
						this.YAxisDivLabels[i] = labelValue.ToString("G5");
					}
					YDivStartPointsScreen[YDivStartPointsScreen.Length-1].Y = (int) screenMaxY;
					YDivEndPointsScreen[YDivStartPointsScreen.Length-1].Y = YDivStartPointsScreen[YDivStartPointsScreen.Length-1].Y;
					YDivStartPointsScreen[YDivStartPointsScreen.Length-1].X = screenMinX;
					YDivEndPointsScreen[YDivStartPointsScreen.Length-1].X = screenMinX -AxisTickLengthInPixels;
					this.YAxisDivLabels[YDivStartPointsScreen.Length-1] =  this.monStore.graph.Yaxis.Max.ToString("G5");
					#endregion last division point is NOT coincident with Y axis max
				}
			}
			//scaling to convert value to length in pixels 
			YScalingScreen = ((YDivStartPointsScreen[0].Y) - (YDivStartPointsScreen[YDivStartPointsScreen.Length-1].Y))
				/(this.monStore.graph.Yaxis.Max-this.monStore.graph.Yaxis.Min);
			#endregion Y Axis scaling
		}
		private void positionLabelsScreen()
		{
			//centre the labels correctly
			float YMiddle = (this.ClientRectangle.Top + (this.ClientRectangle.Height/2) - (YaxisLabelSizeScreen.Height/2));
			YAxeslabelBoundsScreen = new RectangleF(this.ClientRectangle.Left,YMiddle,YaxisLabelSizeScreen.Width,YaxisLabelSizeScreen.Height);
			float XMiddle = (this.ClientRectangle.Width - XAxisLabelSizeScreen.Width)/2;
			XAxeslabelBoundsScreen = new RectangleF(XMiddle, (this.ClientRectangle.Height- this.bottomOffset), XAxisLabelSizeScreen.Width, XAxisLabelSizeScreen.Height);
			MainLabelBoundsScreen = new RectangleF(XMiddle, this.ClientRectangle.Top+10, MainLabelSizeScreen.Width, MainLabelSizeScreen.Height);
		}
		private void refreshBtn_Click(object sender, System.EventArgs e)
		{
			this.Invalidate();
		}

		private void startNonCalibratedGraphing()
		{
            //DR38000267 need to pause graphing in case already running,then select non-calib before perform new setup
            monitorState = MonitorStates.GRAPHING_PAUSED;
            this.graphTypeRequested = GraphTypes.NON_CALIBRATED; //overwrite so we can do allow window close

			#region make sure that interval is correct and >= non-claibrating minimum
			this.TimeBaseComboBox.DataSource = SDOtimebases;
			this.TimeBaseComboBox.SelectedIndex = TimeIntervaIComboIndex;

			if(this.TimeIntervaIBeingUsed <100)
			{
				TimeIntervaIBeingUsed = 100;
				this.TimeBaseComboBox.SelectedIndex = 0; //get this index correct
				this.TimeIntervaIComboIndex = 0;
				#region recalculate the X Axis and force graph redraw
				calculateInitialXAxisMaxAndMin();
				initialiseUserGraphEntryPaneXAxis();
				this.Invalidate();
				#endregion recalculate the X Axis and force graph redraw
			}
			#endregion make sure that interval is correct and >= non-claibrating minimum
			this.graphLegendPanel.Visible = true;
			this.graphUpdatePanel.Visible = true;
			this.stopPDODataRetrivalThread();
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("");
			}
			this.NMTFlag = NMTStateChangeFlags.None;
			monitorState = MonitorStates.GRAPHING_PAUSED;
			initialiseGraphicsParams();
			this.TimeIntervaIComboIndex = 1;
			monitorState = MonitorStates.GRAPHING;
			this.monitoringTimer.Enabled = true;
			showUserControls();
		}
		#endregion graphing methods

		#region Legend Panel methods
		private void updateLegendPanel()
		{
			if(this.graphLegendPanel.Controls.Count>0)
			{
				this.graphLegendPanel.Controls.Clear();
			}
			LegendColours = new Panel[numOfItemsToplot];
			LegendNames = new Label[numOfItemsToplot];
			this.graphLegendPanel.Height = (numOfItemsToplot *25) + 85;
			Label legendTitle = new Label();
			legendTitle.Text = "&Legend Panel";
			legendTitle.Location = new System.Drawing.Point(0, 0);
			legendTitle.Size = new Size(graphLegendPanel.Width, 30);
			legendTitle.TextAlign = ContentAlignment.TopCenter;
			legendTitle.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			legendTitle.MouseDown +=new MouseEventHandler(panel2_MouseDown);
			legendTitle.MouseMove +=new MouseEventHandler(panel2_MouseMove);
			legendTitle.MouseUp +=new MouseEventHandler(panel2_MouseUp);
			this.graphLegendPanel.Controls.Add(legendTitle);
			#region Legend Panel Close Button
			Button legendBtn = new Button();
			legendBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			legendBtn.BackColor = SCCorpStyle.buttonBackGround;
			legendBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			legendBtn.Name = "legendBtn";
			legendBtn.Size = new System.Drawing.Size(140, 25);
			legendBtn.TabIndex = 26;
			legendBtn.Text = "&Close this panel";
			legendBtn.Click += new System.EventHandler(this.legendBtn_Click);
			legendBtn.Location = new System.Drawing.Point(this.graphLegendPanel.Width-legendBtn.Width-10, this.graphLegendPanel.Height-35);
			this.graphLegendPanel.Controls.Add(legendBtn);
			#endregion Legend Panel Close Button
			for(int i = 0;i<numOfItemsToplot;i++)
			{
				LegendColours[i] = new Panel();
				LegendColours[i].Location = new System.Drawing.Point(10, ((i*25)+ 30));
				LegendColours[i].Name = "colourpanel" + i.ToString();
				LegendColours[i].Size = new System.Drawing.Size(20,20);
				LegendColours[i].BackColor = plotColours[i];
				LegendColours[i].MouseDown +=new MouseEventHandler(panel2_MouseDown);
				LegendColours[i].MouseMove +=new MouseEventHandler(panel2_MouseMove);
				LegendColours[i].MouseUp +=new MouseEventHandler(panel2_MouseUp);
				this.graphLegendPanel.Controls.Add(LegendColours[i]);
				LegendNames[i] = new Label();
				LegendNames[i].Location = new System.Drawing.Point(30, ((i*25)+ 30));
				LegendNames[i].Name = "legendLabel" + i.ToString();
				LegendNames[i].Size = new System.Drawing.Size(400,20);
				if ((this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVd)
					||(this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVq))
				{
					LegendNames[i].Text = this.SCParameterNamesForLegend[i];
				}
                else if (graphTypeRequested == GraphTypes.FROM_FILE)
                {
                    // retrieve data series legends from the file (as offline nodes, can't recalculate easily)
                    if (i < monStore.myLegends.Count)
                    {
                        LegendNames[i].Text = (string)this.monStore.myLegends[i];
                    }
                }
                else
                {
                    LegendNames[i].Text = "Node "
                        + ((ODItemAndNode)sysInfo.CANcomms.VCI.OdItemsBeingMonitored[i]).node.nodeID.ToString()
                        + ": " + ((ODItemAndNode)sysInfo.CANcomms.VCI.OdItemsBeingMonitored[i]).ODparam.parameterName;

                }
				LegendNames[i].MouseDown +=new MouseEventHandler(panel2_MouseDown);
				LegendNames[i].MouseMove +=new MouseEventHandler(panel2_MouseMove);
				LegendNames[i].MouseUp +=new MouseEventHandler(panel2_MouseUp);
				this.graphLegendPanel.Controls.Add(LegendNames[i]);
			}
			this.graphLegendPanel.Invalidate();
		}
		#endregion Legend Panel methods

		#region Graph Update Panel methods
		private void graphUpdateBtn_Click(object sender, System.EventArgs e)
		{
			updateAxesParametersFromUpdatePanelVlaues();
			calcLabelSizeScreen();
			this.Invalidate();
		}
		private void updatePanelCloseBtn_Click(object sender, System.EventArgs e)
		{
			this.graphUpdatePanel.Visible = false;
			this.Invalidate();
		}

		#endregion Graph Update Panel methods

		#region graph painting methods
		private void DATA_DISPLAY_WINDOW_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			if(this.monitorState != MonitorStates.NONE)
			{
				this.SuspendLayout();
				calculateAxesScreen();
				drawAxesScreen();
				this.ResumeLayout();
			}
		}
		private void plotAllDataScreen()
		{
			#region transform Graphics
			gScreen.SetClip(ChartClipAreaScreen);
			gScreen.RotateTransform(180); 
			gScreen.TranslateTransform(-ChartOriginScreen.X, ChartOriginScreen.Y);
			gScreen.ScaleTransform(-1, 1);
			gScreen.TranslateTransform(0, -(this.ClientRectangle.Height));
			#endregion transform Graphics
			if((this.graphTypeRequested == GraphTypes.CALIBRATED)
				|| (this.graphTypeRequested == GraphTypes.NON_CALIBRATED))
			{
                foreach (ODItemAndNode subAndNode in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
				{
                    int dataIndex = sysInfo.CANcomms.VCI.OdItemsBeingMonitored.IndexOf(subAndNode);
				CANopenDataType datatype = (CANopenDataType) subAndNode.ODparam.dataType;
					bool first = true;
					foreach(PointF newPoint in subAndNode.ODparam.screendataPoints)
					{
						if(first == true)
						{
							prevScreenPoint = newPoint;
							first = false;
						}
						//the following lines need to be identicat to the corresponding lines for real time monitoring in Timer 1 handler 
						if(this.monStore.graph.ShowDataMarkers == true)
						{
							if((datatype == CANopenDataType.BOOLEAN)
								|| (subAndNode.ODparam.bitSplit != null))
							{
								//draw a digital type plot
								gScreen.DrawLine(this.colouredPens[dataIndex], 
									(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);

								gScreen.DrawLine(this.colouredPens[dataIndex], 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
							}
							else
							{
								gScreen.DrawLine(this.colouredPens[dataIndex], 
									(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
							}
						}
						else
						{
							if((datatype == CANopenDataType.BOOLEAN)
								|| (subAndNode.ODparam.bitSplit != null))
							{
								//draw a digital type plot
								gScreen.DrawLine(this.colouredPensNoMarker[dataIndex], 
									(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);

								gScreen.DrawLine(this.colouredPensNoMarker[dataIndex], 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
							}
							else
							{
								gScreen.DrawLine(this.colouredPensNoMarker[dataIndex], 
									(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,
									(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, 
									(newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
							}
						}
						prevScreenPoint = newPoint;
					}
				}
			}
			else if((this.graphTypeRequested == GraphTypes.FROM_FILE)
				|| (this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVd)
				|| (this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVq))
			{
				for(int seriesIndex=0;seriesIndex< this.OfflineScreenDataPoints.Length;seriesIndex++)
				{
					bool first = true;
					foreach(PointF newPoint in this.OfflineScreenDataPoints[seriesIndex])
					{
						if(first == true)
						{
							prevScreenPoint = newPoint;
							first = false;
						}
						//the following lines need to be identicat to the corresponding lines for real time monitoring in Timer 1 handler 
						if(this.monStore.graph.ShowDataMarkers == true)
						{
							gScreen.DrawLine(this.colouredPens[seriesIndex], (prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, (newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, (newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
						}
						else
						{
							gScreen.DrawLine(this.colouredPensNoMarker[seriesIndex], (prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingScreen ,(prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, (newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, (newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen);
						}
						prevScreenPoint = newPoint;
					}
				}
			}
			#region remove transform graphics
			gScreen.RotateTransform(180);
			gScreen.TranslateTransform(ChartOriginScreen.X,ChartOriginScreen.Y);
			gScreen.ScaleTransform(-1, 1);
			gScreen.TranslateTransform(0, -(this.ClientRectangle.Height));
			gScreen.SetClip(this.ClientRectangle);
			#endregion remove transform graphics
		}

		private void drawAxesScreen()
		{
			#region MainTitles an dAxes
			gScreen.DrawString(this.MainTitleTB.Text, mainLabelFont, Brushes.Black, MainLabelBoundsScreen);
			gScreen.DrawLines(Pens.Black, axesPointsScreen);
			#endregion Main titles and Axes
			#region yAxis
			gScreen.DrawString(this.YAxLabelTB.Text, axesLabelFont, Brushes.Black,YAxeslabelBoundsScreen,YAxisStringFormat);
			for(int i=0;i<YDivStartPointsScreen.Length;i++)
			{
				gScreen.DrawLine(Pens.Black, YDivStartPointsScreen[i],YDivEndPointsScreen[i]);
				if(this.YAxisGridCB.Checked == true)
				{
					if(this.YAxisDivLabels[i] == "0")
					{
						gScreen.DrawLine(Pens.Black, YDivStartPointsScreen[i].X, YDivStartPointsScreen[i].Y, screenMaxX, YDivEndPointsScreen[i].Y);
					}
					else
					{
						gScreen.DrawLine(Pens.Gray, YDivStartPointsScreen[i].X, YDivStartPointsScreen[i].Y, screenMaxX, YDivEndPointsScreen[i].Y);
					}
				}
				int linelength = (int) (YDivEndPointsScreen[i].X - YDivStartPointsScreen[i].X);
				SizeF strsize = gScreen.MeasureString(this.YAxisDivLabels[i], markFont);
				gScreen.DrawString(this.YAxisDivLabels[i],markFont,Brushes.Black, (YDivStartPointsScreen[i].X-linelength-(strsize.Width)-20), YDivStartPointsScreen[i].Y-(strsize.Height/2));  //-3 is to centre align 
			}
			#endregion Y axis
			#region X axis 
			#region axis title
			gScreen.DrawString(this.XAxLabelTB.Text, axesLabelFont, Brushes.Black,XAxeslabelBoundsScreen); 
			#endregion axis title
			for(int i=0;i<XDivStartPointsScreen.Length;i++)
			{
				gScreen.DrawLine(Pens.Black, XDivStartPointsScreen[i],XDivEndPointsScreen[i]);
				
				if(this.XAxisGridCB.Checked == true)
				{
					gScreen.DrawLine(Pens.Gray, XDivStartPointsScreen[i].X,XDivStartPointsScreen[i].Y, XDivEndPointsScreen[i].X, screenMaxY);
				}
				gScreen.DrawLine(Pens.Black, XDivStartPointsScreen[i],XDivEndPointsScreen[i]);
				SizeF strsize = gScreen.MeasureString(this.XAxisDivLabels[i], markFont);
				gScreen.DrawString(this.XAxisDivLabels[i],markFont,Brushes.Black, XDivStartPointsScreen[i].X-(strsize.Width/2), XDivStartPointsScreen[i].Y+strsize.Height);  //+3 is to centre align
			}
			#endregion X axis
			plotAllDataScreen();
		}

		#endregion graph painting methods

		#region graph from file methods
		private void getStoredPlotPointsFromFile()
		{
            ushort dataSeriesPtr = 0;
            foreach (nodeInfo mnNode in this.monStore.myMonNodes)
            {
                //must be able to plot offline data also, so doesn't matter if node exists on the current system
                //for (int i = 0; i < this.sysInfo.nodes.Length; i++)
                //{
                //    if (mnNode.nodeID == this.sysInfo.nodes[i].nodeID)
                //    {
                        foreach (ObjDictItem odItem in mnNode.objectDictionary)
                        {
                            #region step through the ODItems in the monitoring file
                            foreach (ODItemData odSub in odItem.odItemSubs)
                            {  //subNumber greater or equal and got some data points in the file
                                if ((odItem.odItemSubs.Count == 1) || (odItem.odItemSubs.Count > 1 && odSub.subNumber >= 1) && (odSub.screendataPoints.Count > 0))
                                {
                                    this.OfflineScreenDataPoints[dataSeriesPtr++] = odSub.screendataPoints;
                                }
                            }
                            #endregion step through the ODItems in the monitoring file
                        }
            //            break;
            //        }
            //    }
            }
            //need to set to avoid exception when creating pens
            this.numOfItemsToplot = dataSeriesPtr;
		}
		#endregion graph from file methods

		#region finalisation/exit
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: closeBtn_Click
		///		 *  Description     : Event handler for the button used to close this window
		///		 *  Parameters      : system generated
		///		 *  Used Variables  : none
		///		 *  Preconditions   : none - any will be dealt with in a window closing event handler 
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void closeBtn_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void DATA_DISPLAY_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
            if ((this.monitoringTimer.Enabled == true) && (this.graphTypeRequested != GraphTypes.FROM_FILE))
            {
                updateMonStoreData();   //make sure any new plotted data is updated to monStore before leaving
            }

			if((windowCanCloseFlag == true) || (this.graphTypeRequested != GraphTypes.CALIBRATED))
			{
				#region abort all threads
				#region dataRetrievalThread abort
				if(dataRetrievalThread != null)
				{
					if((dataRetrievalThread.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
					{
						dataRetrievalThread.Abort();

						if(dataRetrievalThread.IsAlive == true)
						{
#if DEBUG
							SystemInfo.errorSB.Append("\nFailed to close Thread: " + dataRetrievalThread.Name + " on exit");
#endif
							dataRetrievalThread = null; //force the GC to run
						}
					}
				}
				#endregion dataRetrievalThread abort
				#region fileOpenThread abort
				if(fileOpenThread != null)
				{
					if((fileOpenThread.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
					{
						fileOpenThread.Abort();

						if(fileOpenThread.IsAlive == true)
						{
#if DEBUG
							SystemInfo.errorSB.Append("\nFailed to close Thread: " + fileOpenThread.Name + " on exit");
#endif
							fileOpenThread = null; //force GC to run
						}
					}
				}
				#endregion fileOpenThread abort
				#endregion abort all threads
				#region dispose of all GDI items
				#region brushes and pens disppose
				if (plotBrushes != null)
				{
					for(int i=0;i<this.plotBrushes.Length;i++)
					{
						if(this.plotBrushes[i] != null)
						{
							plotBrushes[i].Dispose();
						}
						if(this.colouredPens[i] != null)
						{
							this.colouredPens[i].Dispose();
						}
						if(this.colouredPensNoMarker[i] != null)
						{
							this.colouredPensNoMarker[i].Dispose();
						}
						if(this.blackPens[i] != null)
						{
							this.blackPens[i].Dispose();
						}
					}
					plotBrushes = null;
				}
				#endregion brushes and pens disppose
				#region graphics dispose
				if(this.gScreen != null)
				{
					gScreen.Dispose();
				}
				if(this.gPrint != null)
				{
					gPrint.Dispose();
				}
				#endregion graphics dispose
				#endregion dispose of all GDI items
				#region disable all timers
				this.NMTStateChangetimer.Enabled = false;
				this.InitialTimer.Enabled = false;
				this.monitoringTimer.Enabled = false;
				#endregion disable all timers
				if(SystemInfo.errorSB.Length>0)
				{
					sysInfo.displayErrorFeedbackToUser("Window closing");
				}
				e.Cancel = false; //force this window to close
			}
			else if ((this.graphTypeRequested == GraphTypes.CALIBRATED) && (windowCanCloseFlag == false))
			{
				e.Cancel = true; //prevent closure until we have completed actions - do before stopMonitoringAndRestorePDOs()
				//we must ensure that any added PDOs are stripped back out again
				this.statusBar1.Text = "Performing finalisation, please wait";
				stopMonitoringAndRestorePDOs();  //
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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

		#region minor methods
		private void LegendPanelMI_Click(object sender, System.EventArgs e)
		{
			this.graphLegendPanel.Visible = true;
		}
		private void GraphSetupPanelMI_Click(object sender, System.EventArgs e)
		{
			this.graphUpdatePanel.Visible = true;
		}
		private void legendBtn_Click(object sender, System.EventArgs e)
		{
			this.graphLegendPanel.Visible = false;
			this.Invalidate();
		}

		private void setUpEventHandlers()
		{
			this.refreshBtn.Click += new System.EventHandler(this.refreshBtn_Click);
			this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
			this.updatePanelCloseBtn.Click += new System.EventHandler(this.updatePanelCloseBtn_Click);
			this.graphUpdateBtn.Click += new System.EventHandler(this.graphUpdateBtn_Click);
			this.TimeBaseComboBox.SelectionChangeCommitted += new System.EventHandler(this.TimeBaseComboBox_SelectionChangeCommitted);
			this.GraphingPauseBtn.Click += new System.EventHandler(this.GraphingPauseBtn_Click);
			this.PrioritySelectionCB.SelectionChangeCommitted += new System.EventHandler(this.PrioritySelectionCB_SelectionChangeCommitted);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DATA_DISPLAY_WINDOW_Closing);
			this.Load += new System.EventHandler(this.DATA_DISPLAY_WINDOW_Load);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.DATA_DISPLAY_WINDOW_Paint);
			this.saveMI.Click += new System.EventHandler(this.saveMI_Click);
			this.pageSetupMI.Click += new System.EventHandler(this.pageSetupMI_Click);
			this.printPreviewMI.Click += new System.EventHandler(this.printPreviewMI_Click);
			this.printMI.Click += new System.EventHandler(this.printMI_Click);
			this.GraphSetupPanelMI.Click += new System.EventHandler(this.GraphSetupPanelMI_Click);
			this.LegenPanelMI.Click += new System.EventHandler(this.LegendPanelMI_Click);
			this.NMTStateChangetimer.Elapsed += new System.Timers.ElapsedEventHandler(this.NMTStateChangetimer_Elapsed);
			this.monitoringTimer.Tick += new System.EventHandler(this.monitoringTimer_Tick);
			this.InitialTimer.Tick += new System.EventHandler(this.InitialTimer_Tick);
			this.CustomisationPanelLabel.MouseDown +=new MouseEventHandler(panel1_MouseDown); 
			this.CustomisationPanelLabel.MouseMove +=new MouseEventHandler(panel1_MouseMove);
			this.CustomisationPanelLabel.MouseUp +=new MouseEventHandler(panel1_MouseUp);
		}
		#endregion minor methods

		#region Panel Dragging methods
		private void panel2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			this.graphLegendPanel.Anchor = ((AnchorStyles)System.Windows.Forms.AnchorStyles.None);
			legendPanelLock = true;
			legendPanelstartX = e.X;
			legendPanelstartY = e.Y;
		}
		private void panel2_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			legendPanelLock = false;
		}

		private void panel2_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(this.legendPanelLock == true) 
			{
				this.SuspendLayout();
				if(e.X>=legendPanelstartX)
				{
					this.graphLegendPanel.Left += (e.X - legendPanelstartX);
				}
				else
				{
					this.graphLegendPanel.Left -= (legendPanelstartX - e.X);
				}
				if(e.Y>=legendPanelstartY)
				{
					this.graphLegendPanel.Top += (e.Y - legendPanelstartY);
					
				}
				else
				{
					this.graphLegendPanel.Top -= (legendPanelstartY - e.Y);
					
				}
				this.ResumeLayout();
			}
		}

		private void panel1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			this.updatePanelLock = true;
			updatePanelstartX = e.X;
			updatePanelstartY = e.Y;
		}

		private void panel1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			this.updatePanelLock = false;
		}

		private void panel1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(this.updatePanelLock == true) 
			{
				this.SuspendLayout();
				if(e.X>=updatePanelstartX)
				{
					this.graphUpdatePanel.Left += (e.X - updatePanelstartX);
				}
				else
				{
					this.graphUpdatePanel.Left -= (updatePanelstartX - e.X);
				}
				if(e.Y>=updatePanelstartY)
				{
					this.graphUpdatePanel.Top += (e.Y - updatePanelstartY);
					
				}
				else
				{
					this.graphUpdatePanel.Top -= (updatePanelstartY - e.Y);
					
				}
				this.ResumeLayout();
			}

		}
		#endregion Panel Dragging methods

		#region PDO graphical monitoring (DI calls)
		private void setupForPDOMonitoring()
		{
			NMTFlag = NMTStateChangeFlags.SetUpPDOsEnterPreOp;
			if( checkIfAllNodesAreinRequiredState() == true)
			{
				requestPDOSetup(); //ignore return value we don't need to act on it here - just when called from the timer
			}
			else
			{
				#region request pre-operational// need to be in pre-op to write new PDO maps
				feedback = this.sysInfo.forceSystemIntoPreOpMode( );
				if(feedback != DIFeedbackCode.DISuccess)
				{
					feedback = this.sysInfo.releaseSystemFromPreOpMode();
					this.NMTFlag = NMTStateChangeFlags.revertingToNonCalibGraphing;
				}
				else
				{
					this.NMTFlag = NMTStateChangeFlags.SetUpPDOsEnterPreOp;
				}
				this.NMTStateChangetimer.Enabled = true;
				#endregion request pre-operational// need to be in pre-op to write new PDO maps
			}
		}
		private bool requestPDOSetup()
		{
			bool setupOK = false;
			#region ask DI to set up the PDOs
			this.statusBar1.Text = "Configuring PDO mappings";
			feedback = this.sysInfo.setupMonitorPDOs( this.userSelectedPriority, userSelectedTimeInterval);
			if(feedback == DIFeedbackCode.DISuccess) 
			{
				this.NMTFlag = NMTStateChangeFlags.SetUpPDOsEnterOp;
				setupOK = true;
			}
			else //failed
			{
				this.NMTFlag = NMTStateChangeFlags.revertingToNonCalibGraphing;  //try and remove any PDOs we added
			}
			#endregion ask DI to set up the PDOs
			#region request NMT Operational
			this.statusBar1.Text = "Requesting Vehicle to go to Operational";
			feedback = this.sysInfo.releaseSystemFromPreOpMode( );
			if(feedback == DIFeedbackCode.DISuccess)
			{
				this.NMTStateChangetimer.Enabled = true;
			}
			else
			{
				sysInfo.displayErrorFeedbackToUser("Enter NMT operational state request rejected. Switching to non-calibrated graphing ");
				this.startNonCalibratedGraphing();
				setupOK = false; //in case we set it true after setupMonitorPDOs()
			}
			#endregion request NMT Operational
			return setupOK;
		}
		private void startPDODataRetrievalThread()
		{
			// call enterMonitoring() on a dataRetrievalThread so that it can be terminated when finished graphing
			monitoringThread = new Thread( new ThreadStart( monitoringWrapper ) ); 
			monitoringThread.Name = "MonitoringDataGatheringThread";
			monitoringThread.IsBackground = true;
            monitoringThread.Priority = ThreadPriority.Normal;
#if DEBUG
			System.Console.Out.WriteLine("Thread: " + monitoringThread.Name + " started");
#endif
			monitoringThread.Start(); 
		}

		private void stopPDODataRetrivalThread()
		{
			if(monitoringThread != null)
			{
				this.monitoringThread.Abort();
				if(this.monitoringThread.IsAlive== true)
				{
					this.monitoringThread = null;
				}
			}
		}
		private void monitoringWrapper()
		{
			this.sysInfo.enterPDOMonitoring();
		}

		private void stopMonitoringAndRestorePDOs()
		{
			this.monitoringTimer.Enabled = false;  //stop reading data
			this.stopPDODataRetrivalThread();
			// finished data monitoring so tidy up (switch back to listening for SDOs not PDOs)
			this.sysInfo.CANcomms.pausePDOMonitoring();  
			// put system back into preop so can re write original PDO maps back to nodes
			feedback = this.sysInfo.forceSystemIntoPreOpMode(  );			
			// re write original PDO maps back to nodes (remove graphical monitoring PDOs )
			if ( feedback == DIFeedbackCode.DISuccess )
			{
				this.NMTFlag = NMTStateChangeFlags.removePDOsEnteringPreOP;
				this.NMTTimeOutCounter = 0;
				this.NMTStateChangetimer.Enabled = true;
				return;
			}
			else
			{
				sysInfo.displayErrorFeedbackToUser("Enter NMT pre-operational state request rejected.");
				this.windowCanCloseFlag = true;
			}
		}
		#endregion

		#region Save to File Methods
		private void saveMI_Click(object sender, System.EventArgs e)
		{
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
		}
		private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.hideUserControls();

            //update any new data in the monStore (no new data would come from a file)
            if (this.graphTypeRequested != GraphTypes.FROM_FILE)
            {
                updateMonStoreData();
            }

			ObjectXMLSerializer xmlSerializer = new ObjectXMLSerializer();
			try
			{
				xmlSerializer.Save(this.monStore, saveFileDialog1.FileName);
			}
			catch(Exception ex)
			{
				SystemInfo.errorSB.Append("\nError saving Monitor file. \nEnsure file is not open and tyy again" + ex.Message); 
				//always show user contro;s and return after non-fatal problem
				this.showUserControls();
				return;
			}
			this.showUserControls();
		}
        private void updateMonStoreData()
        {
            this.monStore.graph.MainTitle = this.MainTitleTB.Text;
            this.monStore.graph.Xaxis.AxisLabel = this.XAxLabelTB.Text;
            this.monStore.graph.Xaxis.LinesVisible = this.XAxisGridCB.Checked;
            #region clear out any old data for monitor items of unconnected nodes
            foreach (nodeInfo CANnode in this.monStore.myMonNodes)
            {
                bool nodeConnected = false;
                for (int j = 0; j < sysInfo.nodes.Length; j++)
                {
                    if (CANnode.nodeID == sysInfo.nodes[j].nodeID)
                    {
                        nodeConnected = true;
                        break;
                    }
                }

                if ((nodeConnected == false) && (CANnode.objectDictionary != null))
                {
                    foreach (ObjDictItem odItem in CANnode.objectDictionary)
                    {
                        foreach (ODItemData odSub in odItem.odItemSubs)
                        {
                            odSub.screendataPoints = null;
                        }
                    }
                }
            }
            monStore.myLegends.Clear();
            #endregion clear out any old data for monitor items of unconnected nodes

            //copy dataseries to monitorstore
            for (int i = 0; i < sysInfo.CANcomms.VCI.OdItemsBeingMonitored.Count; i++)
            {
                monStore.myLegends.Add(LegendNames[i].Text);    //save data series legend
                int ind = ((ODItemAndNode)sysInfo.CANcomms.VCI.OdItemsBeingMonitored[i]).ODparam.indexNumber;
                int sub = ((ODItemAndNode)sysInfo.CANcomms.VCI.OdItemsBeingMonitored[i]).ODparam.subNumber;
                int nodeId = ((ODItemAndNode)sysInfo.CANcomms.VCI.OdItemsBeingMonitored[i]).ODparam.CANnode.nodeID;

                foreach (nodeInfo CANnode in this.monStore.myMonNodes)
                {
                    if (CANnode.nodeID == nodeId)
                    {
                        ODItemData odSub = CANnode.getODSub(ind, sub);
                        if (odSub != null)
                        {
                            odSub.screendataPoints = ((ODItemAndNode)sysInfo.CANcomms.VCI.OdItemsBeingMonitored[i]).ODparam.screendataPoints;
                        }
                        break;
                    }
                }
            }
        }
		#endregion Save to File Methods

		#region printing methods
		private void printMI_Click(object sender, System.EventArgs e)
		{
			this.hideUserControls();
			this.monitorState = MonitorStates.PRINTING_GRAPH;
			this.showUserControls();
			if (printDlg.ShowDialog(this) == DialogResult.OK)
			{
				#region create the graphics and print them
				// Print the document
				printDoc.Print();
				#endregion create the graphics and print them
			}
			this.monitorState = MonitorStates.GRAPHING_PAUSED;
			this.showUserControls();
		}
		public void PrntPgEventHandler(object sender,PrintPageEventArgs ppeArgs)
		{
			// Get the Graphics object associated with the printer
			gPrint = ppeArgs.Graphics;
			#region calculate dimensions of paper
			paperWidth = printDoc.DefaultPageSettings.PaperSize.Width;
			paperHeight = printDoc.DefaultPageSettings.PaperSize.Height;
			if(printDoc.DefaultPageSettings.Landscape == true)
			{ //swap height and width if we are plotting in landscape
				paperWidth = printDoc.DefaultPageSettings.PaperSize.Height;
				paperHeight = printDoc.DefaultPageSettings.PaperSize.Width;
			}
			#endregion calculate dimensions of paper
			calcLabelSizePrint();
			calculateAxesPrint();
			printLegendPanel();
			drawAxesPrint();
		}
		
		private void calcLabelSizePrint()
		{
			YaxisLabelSizePrint = gPrint.MeasureString(this.YAxLabelTB.Text, axesLabelFont, 30, YAxisStringFormat);
			XAxisLabelSizePrint = gPrint.MeasureString(this.XAxLabelTB.Text,axesLabelFont);
			MainLabelSizePrint = gPrint.MeasureString(this.MainTitleTB.Text,mainLabelFont);
			#region axis data label sizes
			float Labeltest = 8888888.888888888F;
			DataLableStrSize = gScreen.MeasureString(Labeltest.ToString("G5"), markFont);
			#endregion axis data label sizes
			AxisTickLengthInPrint = System.Convert.ToUInt16(YaxisLabelSizePrint.Width/2);
			#region legend
			float textHeight = gPrint.MeasureString("test", this.markFont).Height;
			int multiplier = (this.OfflineScreenDataPoints.Length/2) + (this.OfflineScreenDataPoints.Length%2);
			legendHeight = textHeight * multiplier;
			float legendWidth = paperWidth-printDoc.DefaultPageSettings.Margins.Left-printDoc.DefaultPageSettings.Margins.Right;
			LegendBoundsPrint = new RectangleF(printDoc.DefaultPageSettings.Margins.Left, this.paperHeight-printDoc.DefaultPageSettings.Margins.Bottom-legendHeight, legendWidth, legendHeight);
			#endregion legend

			YAxeslabelBoundsPrint = new RectangleF(printDoc.DefaultPageSettings.Margins.Left,((paperHeight - YaxisLabelSizePrint.Height)/2),YaxisLabelSizePrint.Width,YaxisLabelSizePrint.Height);
			XAxeslabelBoundsPrint = new RectangleF(((paperWidth - XAxisLabelSizePrint.Width)/2), 
				(paperHeight - printDoc.DefaultPageSettings.Margins.Bottom - LegendBoundsPrint.Height - textHeight - XAxisLabelSizePrint.Height), 
				XAxisLabelSizePrint.Width, 
				XAxisLabelSizePrint.Height);
			MainLabelBoundsPrint = new RectangleF(((paperWidth - MainLabelSizePrint.Width)/2), 
				printDoc.DefaultPageSettings.Margins.Top, 
				MainLabelSizePrint.Width, 
				MainLabelSizePrint.Height);
		}
		private void calculateAxesPrint()
		{
			#region calculate area left for the chart - ChartAreaPrint
			float textHeight = gPrint.MeasureString("test", this.markFont).Height;
			float graphWidth = paperWidth 
				- printDoc.DefaultPageSettings.Margins.Left 
				- printDoc.DefaultPageSettings.Margins.Right 
				- (int)YaxisLabelSizePrint.Width
				- (int)DataLableStrSize.Width
				- AxisTickLengthInPrint;
			float graphHeight = paperHeight			
				- printDoc.DefaultPageSettings.Margins.Top
				- printDoc.DefaultPageSettings.Margins.Bottom 
				- (int)MainLabelSizePrint.Height 
				- (int)XAxisLabelSizePrint.Height 
				- (int)DataLableStrSize.Height
				- AxisTickLengthInPrint
				- legendHeight
				- textHeight;  //Space for "KEY:" text
			float graphLeft = printDoc.DefaultPageSettings.Margins.Left 
				+ (int)YaxisLabelSizePrint.Width 
				+ (int) DataLableStrSize.Width
				+ AxisTickLengthInPrint;
			float graphTop = printDoc.DefaultPageSettings.Margins.Top 
				+ (int)MainLabelSizePrint.Height;
			ChartAreaPrint = new RectangleF(graphLeft,graphTop, graphWidth, graphHeight);
			#endregion calculate area left for the chart - ChartAreaPrint

			#region calculate the graph origin and axis limits points
			axesPointsPrint = new PointF[3];
			axesPointsPrint[0] = new PointF(ChartAreaPrint.Left,ChartAreaPrint.Top );  //topYaxis
			axesPointsPrint[1] = new PointF(ChartAreaPrint.Left,ChartAreaPrint.Bottom );  //BottonYaxis - Left Xaxis
			axesPointsPrint[2] = new PointF(ChartAreaPrint.Right, ChartAreaPrint.Bottom);  //BottonYaxis - Left Xaxis
			ChartOriginPrint = new PointF(ChartAreaPrint.Left, (float) (ChartAreaPrint.Height-ChartAreaPrint.Bottom));
			#endregion calculate the graph origin and axis limits points

			#region X Axis scaling
			//calculate points array for divisions  
			XDivPointsPrint = new PointF[this.XDivStartPointsScreen.Length];  //just tie up with screen
			XDivInPrint = XDivInPixels * (ChartAreaPrint.Width/(screenMaxX- screenMinX)); //scale screen params for print Graphics
			if(this.maxXDivCoincidentWithAxisMax == true)
			{
				for(int i = 0;i<XDivPointsPrint.Length;i++)
				{
					XDivPointsPrint[i].X = (int) ((i * XDivInPrint) + ChartAreaPrint.Left);
					XDivPointsPrint[i].Y = ChartAreaPrint.Bottom;
				}
			}
			else
			{
				for(int i = 0;i<XDivPointsPrint.Length-1 ;i++)
				{
					XDivPointsPrint[i].X = (int) ((i * XDivInPrint) + ChartAreaPrint.Left);
					XDivPointsPrint[i].Y = ChartAreaPrint.Bottom;
				}
				XDivPointsPrint[XDivPointsPrint.Length-1].X = ChartAreaPrint.Right;
				XDivPointsPrint[XDivPointsPrint.Length-1].Y = ChartAreaPrint.Bottom;
			}
			//scaling to convert value to length in dpi? 
			XscalingPrint = ((XDivPointsPrint[XDivPointsPrint.Length-1].X) -(XDivPointsPrint[0].X))/(this.monStore.graph.Xaxis.Max-this.monStore.graph.Xaxis.Min);  
			#endregion

			#region Y Axis scaling
			YDivPointsPrint = new PointF[this.YDivStartPointsScreen.Length];  //there needs to be one more mark than division to get a mark at each end
			YDivInPrint = YDivInPixels * (ChartAreaPrint.Height/(screenMinY- screenMaxY)); //scale screen params for print Graphics

			if(this.monStore.graph.Yaxis.Min<0)
			{
				float negProportionOfAxis =  -1 * ((this.monStore.graph.Yaxis.Min) /(this.monStore.graph.Yaxis.Max - this.monStore.graph.Yaxis.Min));
				//find the zero level 
				float printZeroY  =  ChartAreaPrint.Bottom - (ChartAreaPrint.Height * negProportionOfAxis);

				//we could have to manually put in min and/or Max points
				if(this.minYDivCoincidentWithAxisMin == true)
				{
					for(int i = 0;i<zeroIndex;i++)
					{
						YDivPointsPrint[i].Y = (int) (printZeroY + ((zeroIndex - i) * YDivInPrint));
						YDivPointsPrint[i].X = ChartAreaPrint.Left;
					}
				}
				else
				{
					YDivPointsPrint[0].Y = (int) ChartAreaPrint.Bottom;
					YDivPointsPrint[0].X = ChartAreaPrint.Left;

					for(int i = 1;i<zeroIndex;i++)
					{
						YDivPointsPrint[i].Y = (int) (printZeroY + ((zeroIndex - i) * YDivInPrint));
						YDivPointsPrint[i].X = ChartAreaPrint.Left;
					}
				}
				
				YDivPointsPrint[zeroIndex].Y = printZeroY;
				YDivPointsPrint[zeroIndex].X = ChartAreaPrint.Left;

				if(this.maxYDivCoincidentWithAxisMax == true)
				{
					for(int i = zeroIndex + 1;i<YDivPointsPrint.Length;i++)
					{
						YDivPointsPrint[i].Y = (int) (printZeroY - ( (i - zeroIndex) * YDivInPrint));
						YDivPointsPrint[i].X = ChartAreaPrint.Left;
					}
				}
				else
				{
					for(int i = zeroIndex + 1;i<YDivPointsPrint.Length-1;i++)
					{
						YDivPointsPrint[i].Y =  (int) (printZeroY - ( (i - zeroIndex) * YDivInPrint));
						YDivPointsPrint[i].X = ChartAreaPrint.Left;
					}
					YDivPointsPrint[YDivPointsPrint.Length-1].Y = ChartAreaPrint.Top;
					YDivPointsPrint[YDivPointsPrint.Length-1].X = ChartAreaPrint.Left;
				}
			}
			else
			{
				for(int i = 0;i<YDivPointsPrint.Length-1;i++)
				{
					YDivPointsPrint[i].Y = (int) (ChartAreaPrint.Bottom - ( i * YDivInPrint));
					YDivPointsPrint[i].X = ChartAreaPrint.Left;
				}
				if(this.maxYDivCoincidentWithAxisMax == true)
				{
					YDivPointsPrint[YDivPointsPrint.Length-1].Y = (int) (ChartAreaPrint.Bottom - ( YDivPointsPrint.Length-1 * YDivInPrint));
					YDivPointsPrint[YDivPointsPrint.Length-1].X = ChartAreaPrint.Left;
				}
				else
				{
					YDivPointsPrint[YDivPointsPrint.Length-1].Y = (int) ChartAreaPrint.Top;
					YDivPointsPrint[YDivPointsPrint.Length-1].X = ChartAreaPrint.Left;
				}
			}
			//scaling to convert value to length in pixels 
			YScalingPrint = ((YDivPointsPrint[0].Y) - (YDivPointsPrint[YDivPointsPrint.Length-1].Y))/(this.monStore.graph.Yaxis.Max-this.monStore.graph.Yaxis.Min);
			#endregion Y Axis scaling
		}
		private void drawAxesPrint()
		{
			#region Main and Axis Titles and Axes lines
			gPrint.DrawString(this.MainTitleTB.Text, mainLabelFont, Brushes.Black, MainLabelBoundsPrint);
			gPrint.DrawString(this.YAxLabelTB.Text, axesLabelFont, Brushes.Black,YAxeslabelBoundsPrint,YAxisStringFormat);
			gPrint.DrawString(this.XAxLabelTB.Text, axesLabelFont, Brushes.Black,XAxeslabelBoundsPrint); 
			gPrint.DrawLines(Pens.Black, axesPointsPrint); 
			#endregion Main titles and Axes
			#region yAxis
			#region find width of longest string used for data markers
			SizeF maxString = new SizeF(0F,0F);
			for(int i=0;i<YDivPointsPrint.Length;i++)
			{
				float divValue = this.monStore.graph.Yaxis.Min + (i * this.monStore.graph.Yaxis.DivValue);
				SizeF labelSize  =  gPrint.MeasureString(divValue.ToString("G5"), markFont);
				if(labelSize.Width > maxString.Width)
				{
					maxString.Width = labelSize.Width;
				}
			}
			float dataLabeltweak = DataLableStrSize.Width - maxString.Width;
			#endregion find width of longest string used for data markers
			for(int i=0;i<YDivPointsPrint.Length;i++)
			{
				PointF tempPoint = new PointF((YDivPointsPrint[i].X - AxisTickLengthInPrint), YDivPointsPrint[i].Y);
				gPrint.DrawLine(Pens.Black, YDivPointsPrint[i],tempPoint);
				#region Y Axis gridlines
				if(this.YAxisGridCB.Checked == true)
				{
					PointF tempEndPoint = new PointF((paperWidth - printDoc.DefaultPageSettings.Margins.Right), YDivPointsPrint[i].Y);
					gPrint.DrawLine(Pens.Gray, YDivPointsPrint[i], tempEndPoint);
				}
				#endregion Y Axis gridlines
				SizeF labelSize  =  gPrint.MeasureString(YAxisDivLabels[i], markFont);
				gPrint.DrawString(YAxisDivLabels[i],	markFont,Brushes.Black, 
					(printDoc.DefaultPageSettings.Margins.Left + YAxeslabelBoundsPrint.Width + dataLabeltweak),  //X pos
					YDivPointsPrint[i].Y-(labelSize.Height/2));  //Y pos - centred around data mark
			}
			#endregion Y axis
			#region X axis 
			for(int i=0;i<XDivPointsPrint.Length;i++)
			{
				#region X axis gridlines
				if(this.XAxisGridCB.Checked == true)
				{
					PointF tempEndPoint = new PointF(XDivPointsPrint[i].X, ChartAreaPrint.Top);
					gPrint.DrawLine(Pens.Gray, XDivPointsPrint[i],tempEndPoint); //gridlines
				}
				#endregion X axis gridlines
				SizeF labelSize  =  gPrint.MeasureString(this.XAxisDivLabels[i], markFont);
				gPrint.DrawString(this.XAxisDivLabels[i],	markFont,Brushes.Black, 
					(XDivPointsPrint[i].X-(labelSize.Width/2)),
					(XAxeslabelBoundsPrint.Top - labelSize.Height));
				PointF tempPoint = new PointF(XDivPointsPrint[i].X, XDivPointsPrint[i].Y + AxisTickLengthInPrint - 2); //-2 for minor aesthetic tweak
				gPrint.DrawLine(Pens.Gray, XDivPointsPrint[i],tempPoint); //tick marks
			}
			#endregion X axis
			plotAllDataPrint();
		}

		private void printLegendPanel()
		{
			PointF leftPt, rightPt;
			int lineLength = 20;
			float textHeight = gPrint.MeasureString("test", this.markFont).Height;
			gPrint.DrawString("KEY:", this.markFont, Brushes.Black, LegendBoundsPrint.Left + 5, LegendBoundsPrint.Top - textHeight);

			if((this.graphTypeRequested == GraphTypes.CALIBRATED)
				||( this.graphTypeRequested == GraphTypes.NON_CALIBRATED))
			{
                foreach (ODItemAndNode subAndNode in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
				{
                    int i = sysInfo.CANcomms.VCI.OdItemsBeingMonitored.IndexOf(subAndNode);

					leftPt = new PointF(LegendBoundsPrint.Left + 5 + ((i%2) * (this.LegendBoundsPrint.Width/2)), this.LegendBoundsPrint.Top + ((i/2) * textHeight));
					rightPt = new PointF(leftPt.X + lineLength, leftPt.Y);
					if(this.printDoc.PrinterSettings.SupportsColor == false)
					{
						gPrint.DrawLine(blackPens[i],leftPt.X, leftPt.Y+(textHeight/2), rightPt.X, rightPt.Y+(textHeight/2));
					}
					else
					{
						#region coloured printing
						if(this.monStore.graph.ShowDataMarkers == true)
						{
							gPrint.DrawLine(this.colouredPens[i],leftPt, rightPt);
						}
						else
						{
							gPrint.DrawLine(this.colouredPensNoMarker[i],leftPt, rightPt);
						}
						#endregion coloured printing
					}
					gPrint.DrawString(subAndNode.ODparam.parameterName,	this.markFont, Brushes.Black, rightPt.X + 5, rightPt.Y);
				}
			}
			else if(( this.graphTypeRequested == GraphTypes.FROM_FILE)
				|| (this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVd)
				|| (this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVq))
			{
				for(int i = 0;i<this.OfflineScreenDataPoints.Length;i++)
				{
					leftPt = new PointF(LegendBoundsPrint.Left + 5 + ((i%2) * (this.LegendBoundsPrint.Width/2)), this.LegendBoundsPrint.Top + ((i/2) * textHeight));
					rightPt = new PointF(leftPt.X + lineLength, leftPt.Y);
					if(this.printDoc.PrinterSettings.SupportsColor == false)
					{
						gPrint.DrawLine(blackPens[i],leftPt.X, leftPt.Y+(textHeight/2), rightPt.X, rightPt.Y+(textHeight/2));
					}
					else
					{
						#region coloured printing
						if(this.monStore.graph.ShowDataMarkers == true)
						{
							gPrint.DrawLine(this.colouredPens[i],leftPt, rightPt);
						}
						else
						{
							gPrint.DrawLine(this.colouredPensNoMarker[i],leftPt, rightPt);
						}
						#endregion coloured printing
					}
					gPrint.DrawString(nonHeaderRows[i].Row[(int) TblCols.param].ToString(),
						this.markFont, Brushes.Black, rightPt.X + 5, rightPt.Y);
				}
			}
		}
		private void plotAllDataPrint()
		{
			#region transform Graphics
			gPrint.SetClip(ChartAreaPrint);  //prevent 
			gPrint.RotateTransform(180); 
			gPrint.TranslateTransform(-ChartOriginPrint.X, ChartOriginPrint.Y);
			gPrint.ScaleTransform(-1, 1);
			gPrint.TranslateTransform(0, -(this.printDoc.DefaultPageSettings.Bounds.Height));
			#endregion transform Graphics

			if((this.graphTypeRequested == GraphTypes.CALIBRATED)
				|| (this.graphTypeRequested == GraphTypes.NON_CALIBRATED))
			{
                foreach (ODItemAndNode subAndNode in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
				{
					CANopenDataType datatype = (CANopenDataType) subAndNode.ODparam.dataType;
                    int dataIndex = sysInfo.CANcomms.VCI.OdItemsBeingMonitored.IndexOf(subAndNode);

					bool first = true;
					int Yoffset = (int) (paperHeight - ChartAreaPrint.Bottom + ChartAreaPrint.Top);
					foreach(PointF newPoint in subAndNode.ODparam.screendataPoints)
					{
						if(first == true)
						{
							prevScreenPoint = newPoint;
							first = false;
						}
						//the following lines need to be identicat to the corresponding lines for real time monitoring in Timer 1 handler 
						if(this.printDoc.PrinterSettings.SupportsColor == false)
						{
							#region drawlines - black liens with markers
							if((datatype == CANopenDataType.BOOLEAN)
								|| (subAndNode.ODparam.bitSplit != null))
							{
								gPrint.DrawLine(
									this.blackPens[dataIndex], 
									(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingPrint ,
									((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset,  
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint, 
									((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset
									);
								gPrint.DrawLine(
									this.blackPens[dataIndex], 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint,
									((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset, 
									(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint, 
									((newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset
									);
							}
							else
							{
								gPrint.DrawLine(this.blackPens[dataIndex],  
									((prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingPrint),
									((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset, 
									((newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint), 
									((newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset);  
							}
							
							#endregion drawlines - black liens with markers
						}
						else
						{
							if(this.monStore.graph.ShowDataMarkers == true)
							{
								#region drawlines with markers
								if((datatype == CANopenDataType.BOOLEAN)
									|| (subAndNode.ODparam.bitSplit != null))
								{
									gPrint.DrawLine(
										this.colouredPens[dataIndex], 
										(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingPrint ,
										((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset,  
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint, 
										((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset
										);

									gPrint.DrawLine(
										this.colouredPens[dataIndex], 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint,
										((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset, 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint, 
										((newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset
										);
								}
								else
								{
									gPrint.DrawLine(this.colouredPens[dataIndex],  
										((prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingPrint),
										((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset, 
										((newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint), 
										((newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset);  
								}
								#endregion drawlines with markers
							}
							else
							{
								#region draw lines - no markers
								if((datatype == CANopenDataType.BOOLEAN)
									|| (subAndNode.ODparam.bitSplit != null))
								{
									gPrint.DrawLine(
										this.colouredPensNoMarker[dataIndex], 
										(prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingPrint ,
										((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset,  
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint, 
										((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset
										);

									gPrint.DrawLine(
										this.colouredPensNoMarker[dataIndex], 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint,
										((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset, 
										(newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint, 
										((newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset
										);
								}
								else
								{
									gPrint.DrawLine(this.colouredPensNoMarker[dataIndex],  
										((prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingPrint),
										((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset, 
										((newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint), 
										((newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset);  
								}
								#endregion draw lines - no markers
							}
						}
						prevScreenPoint = newPoint;
					}

				}
			}
			else if ((this.graphTypeRequested == GraphTypes.FROM_FILE)
				|| (this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVd)
				|| (this.graphTypeRequested == GraphTypes.SELF_CHAR_OLVq))
			{
				for(int rowIndex=0;rowIndex< this.OfflineScreenDataPoints.Length;rowIndex++)
				{
					bool first = true;
					int Yoffset = (int) (paperHeight - ChartAreaPrint.Bottom + ChartAreaPrint.Top);
					foreach(PointF newPoint in this.OfflineScreenDataPoints[rowIndex])
					{
						if(first == true)
						{
							prevScreenPoint = newPoint;
							first = false;
						}
						//the following lines need to be identicat to the corresponding lines for real time monitoring in Timer 1 handler 
						if(this.printDoc.PrinterSettings.SupportsColor == false)
						{
							#region drawlines - black liens with markers
							gPrint.DrawLine(this.blackPens[rowIndex],  
								((prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingPrint),
								((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset, 
								((newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint), 
								((newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset);  
							#endregion drawlines - black liens with markers
						}
						else
						{
							if(this.monStore.graph.ShowDataMarkers == true)
							{
								#region drawlines with markers
								gPrint.DrawLine(this.colouredPens[rowIndex],  
									((prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingPrint),
									((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset, 
									((newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint), 
									((newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset);  
								#endregion drawlines with markers
							}
							else
							{
								#region draw lines - no markers
								gPrint.DrawLine(this.colouredPensNoMarker[rowIndex],  
									((prevScreenPoint.X-this.monStore.graph.Xaxis.Min)*XscalingPrint),
									((prevScreenPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint)+ Yoffset, 
									((newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingPrint), 
									((newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingPrint) + Yoffset);  
								#endregion draw lines - no markers
							}
						}
						prevScreenPoint = newPoint;
					}
				}
			}
			#region remove transform graphics
			gPrint.RotateTransform(180);
			gPrint.TranslateTransform(ChartOriginPrint.X,ChartOriginPrint.Y);
			gPrint.ScaleTransform(-1, 1);
			gPrint.TranslateTransform(0, -(this.printDoc.DefaultPageSettings.Bounds.Height));
			#endregion remove transform graphics
			gPrint.SetClip(this.printDoc.DefaultPageSettings.Bounds);
		}

		private void pageSetupMI_Click(object sender, System.EventArgs e)
		{
			if (setupDlg.ShowDialog(this) == DialogResult.OK)
			{
				printDoc.DefaultPageSettings = setupDlg.PageSettings;
				printDoc.PrinterSettings = setupDlg.PrinterSettings;
			}
		}
		private void printPreviewMI_Click(object sender, System.EventArgs e)
		{
			previewDlg.UseAntiAlias = true;
			previewDlg.WindowState = FormWindowState.Normal;
			previewDlg.ShowDialog(this);
		}

		#endregion printing methods

		private void TimeBaseComboBox_DropDown(object sender, System.EventArgs e)
		{
			this.UserInstructionsLabel.Text = "Please wait";
			GraphingPauseBtn.Visible = false;
			refreshBtn.Visible = false;
			this.priorityLabel.Visible = false;
			this.PrioritySelectionCB.Visible = false;
			this.intervalAchievedLabel.Visible = false;
			this.MonitoringIntervalLabel.Visible = false;
			this.saveMI.Enabled = false;
			this.printMI.Enabled = false;
			this.graphingMI.Visible = false;
			this.pageSetupMI.Enabled = false;
			this.printPreviewMI.Enabled = false;
		}

		private void savefieldialog_FileOk(object sender, CancelEventArgs e)
		{
//			Message.Show("save Graphing list");
		}

		private void DATA_DISPLAY_WINDOW_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{
				bool graphicTransformRemoved = false;
				this.PointToScreen(new Point(e.X, e.Y));
				Rectangle mouseWindow = new Rectangle(e.X-2, e.Y-2, 4,4);
				Graphics gMouse = this.CreateGraphics();

				#region transform Graphics
				//				gScreen.SetClip(ChartClipAreaScreen);
				//				gScreen.RotateTransform(180); 
				//				gScreen.TranslateTransform(-ChartOriginScreen.X, ChartOriginScreen.Y);
				//				gScreen.ScaleTransform(-1, 1);
				//				gScreen.TranslateTransform(0, -(this.ClientRectangle.Height));

				gMouse.SetClip(mouseWindow);
				gMouse.RotateTransform(180); 
				gMouse.TranslateTransform(-ChartOriginScreen.X, ChartOriginScreen.Y);
				gMouse.ScaleTransform(-1, 1);
				gMouse.TranslateTransform(0, -(this.ClientRectangle.Height));
				#endregion transform Graphics

				for(int rowIndex=0;rowIndex< this.OfflineScreenDataPoints.Length;rowIndex++)
				{
					foreach(PointF newPoint in this.OfflineScreenDataPoints[rowIndex])
					{
						RectangleF pointWindow = new RectangleF((newPoint.X-this.monStore.graph.Xaxis.Min) *XscalingScreen, (newPoint.Y-this.monStore.graph.Yaxis.Min)*YScalingScreen, 1, 1);
						if( gMouse.ClipBounds.IntersectsWith(pointWindow) == true)
						{
							SizeF mySize = gScreen.MeasureString(newPoint.X.ToString("G5") + ", " + newPoint.Y.ToString("G5"),axesLabelFont);
							graphicTransformRemoved = true;
							gScreen.DrawString(newPoint.X.ToString("G5") + ", " + newPoint.Y.ToString("G5"),markFont,Brushes.Red,this.screenMinX + pointWindow.X, this.screenMinY - pointWindow.Y-mySize.Height-3);
							break;
						}
						if(graphicTransformRemoved == true)
						{
							break;  //one point at a time 
						}
					}
				}
			}
		}

	}
	#endregion Data Display form class
}



