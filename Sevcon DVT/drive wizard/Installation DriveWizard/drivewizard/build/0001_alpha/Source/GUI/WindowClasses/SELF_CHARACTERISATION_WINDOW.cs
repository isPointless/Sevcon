/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.120$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/09/2008 23:17:46$
	$ModDate:23/09/2008 21:21:46$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	This class is responsible for user interface for the Self characterization process. 
	Self Characterization is the process of the controller driving and reading values 
    back from a motor to allow fine control of a specific motor type. 

REFERENCES    

MODIFICATION HISTORY
    $Log:  36803: SELF_CHARACTERISATION_WINDOW.cs 

   Rev 1.120    23/09/2008 23:17:46  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.119    12/03/2008 13:43:54  ak
 All DI threads have ThreadPriority increased to Normal from BelowNormal
 (needed when running VCI3)


   Rev 1.118    21/02/2008 15:03:46  jw
 Bug fix. Route found whereby DW can send WholeDataChecksum after
 condetransfer failed.. Now prevented


   Rev 1.117    18/02/2008 09:28:42  jw
 VCI2 CAN methods moved form communications to VCI 2 and renamed for
 consistency with VCI3 - towards back compatibility


   Rev 1.116    25/01/2008 10:47:32  jw
 VCI3 and Vista functionality files merge - more testing required


   Rev 1.115    21/01/2008 12:03:00  jw
 File merge for VCI3/ Vista. These changes are those to go in all builds


   Rev 1.114    05/12/2007 22:13:06  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Text;
using NumericalRecipes.MinimizationOrMaximizationOfFunctions;

namespace DriveWizard
{
	#region enumerated types
	public enum selfCharCol {Name, UserValue, LM, Units }; 
	public enum TestState
	{
		ENTER_PLATE_DATA,
		ENTER_MOTOR_LIMITS,
		ENTER_LINE_CONTACTOR,
		ENTER_TESTIFILES,
		REENTER_PLATE_DATA,
		REENTER_MOTOR_LIMITS,
		REENTER_LINE_CONTACTOR,
		REENTER_TESTIFILES,
		PROGRAMMING_SC_CODE,
		OPEN_LOOP_TEST1,
		NO_LOAD_TEST2,  //applicable to internal Self char only
		CLOSED_LOOP_TEST3,  //applicable to internal Self char only
		SELF_CHAR_TESTS_COMPLETE,
		PROGRAMMING_ORIGINAL_CODE,
		WRITE_RESULTS_TO_NODE
	};
	public enum SCRequestResponseType {NONE, DCTT_OL_Vd, DCTT_OL_Vq, DCTT_CL, NLT_Start, NLT_w, NLT_stop};
	public enum TabPages {GENERAL,  OPEN_LOOP, CLOSED_LOOP, NO_LOAD_TEST, POWER_FRAME, SCWIZ_ONLY};

	//note th efollowing enum is mimmiced in SCWiz - any changes to this MUST be 
	//reflected in the corresponding enum in SCWiz
	public enum SCWizargs
	{
		//CAN Open information
		DW_baud,
		DW_NodeID_SCnode,
		DW_NodeID_LineContactor,
		//Vehicle Data
		DW_LC_PullInVolts,
		DW_LC_PullInms,
		DW_LC_holdVolts,
		DW_LC_Output,
		//Motor Name Plate Data
		DW_EncoderPulsesPerRev,
		DW_EncoderPolarity,
		DW_NoOfPolePairs, 
		DW_RatedLineVoltage_rms,
		DW_RatedPhaseCurr_rms,
		DW_RatedSpeed_Mech,
		DW_RatedFreq_Elec,
		DW_RatedPower_Watts,
		DW_RatedPowerFactor,
		//Test and Power Frame profiles
		DW_TestProfileFilepath,
		DW_PowerFrameProfilefilePath,
		//Debugging
		DW_DebugLevel, 
		DW_UseMatLab,
		DW_showDebug
	};

	#endregion enumerated types

	#region SELF_CHARACTERISATION form class
	/// <summary>
	/// Summary description for SELF_CHARACTERISATION class
	/// This class is responsible for user interface for the Selfc characterization process. 
	/// Self Characterization is th eporcess of the controller driveing and re-ding vlaues back from a motor
	/// to allow fine control of a specific motor type. 
	/// The GUI consitss of
	/// 1. Loading winddow
	/// 2. Enabling user to enter name plate data from the motor
	/// 3. Re=programming the controller with automatic/user selectable self characterization software
	/// 4. cntrolling the Self characteirsation process (number crunching done elsewhere
	/// 5. Displaying resutls ot user
	/// 6. Enabling user to update controller with results of self characterization (stored in DCF file)
	/// 7. Allowing user to re-ender namepalte data and re-run self characterization
	/// 8. Allowing user to opt to exit self char at any time and displaying appropriate warnigns etc
	/// 9. At user selecting end of self characteirsation - re-programming the controller with 
	/// </summary>
	public class SELF_CHARACTERISATION_WINDOW : System.Windows.Forms.Form
	{
		#region controls declarations
		private System.Timers.Timer timer1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Timer progTimer;
		private System.Windows.Forms.TextBox textBox9;
		private System.Windows.Forms.TextBox textBox8;
		private System.Windows.Forms.TextBox textBox4;
		private System.Windows.Forms.TextBox textBox3;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Panel MainPanel;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.Panel PnlDataGrids;
		private System.Windows.Forms.Panel Pnl_ConfirmNamePlateData;
		private System.Windows.Forms.Panel PnlMotorLimits;
		private System.Windows.Forms.Panel PnlUsrLabels;
		private System.Windows.Forms.Panel PnlLineContactors;
		private System.Windows.Forms.Panel PnlTestFiles;
		private System.Windows.Forms.TextBox TB_DW_LC_PullInms;
		private System.Windows.Forms.TextBox TB_DW_LC_holdVolts;
		private System.Windows.Forms.TextBox TB_DW_LC_PullInVolts;
		private System.Windows.Forms.Button restartTestButton;
		private System.Windows.Forms.CheckBox CB_DW_EncoderPolarity;
		private System.Windows.Forms.TextBox TB_DW_RatedPowerFactor;
		private System.Windows.Forms.TextBox TB_DW_RatedFreq_Elec;
		private System.Windows.Forms.TextBox TB_DW_RatedLineVoltage_rms;
		private System.Windows.Forms.TextBox TB_DW_RatedSpeed_Mech;
		private System.Windows.Forms.TextBox TB_DW_RatedPower_Watts;
		private System.Windows.Forms.TextBox TB_DW_NoOfPolePairs;
		private System.Windows.Forms.TextBox TB_DW_RatedPhaseCurr_rms;
		private System.Windows.Forms.TextBox TB_DW_EncoderPulsesPerRev;
		private System.Windows.Forms.CheckBox CB_PowerFactorUnknown;
		private System.Windows.Forms.Label Lbl_UsrProgrammingFeedback;
		private System.Windows.Forms.ComboBox CB_LineContactorNo;
		private System.Windows.Forms.Panel Pnl_Buttons;
		private System.Windows.Forms.Button Btn_Back;
		private System.Windows.Forms.Button Btn_ChangeFile;
		private System.Windows.Forms.Label Lbl_LChodlvoltage;
		private System.Windows.Forms.Button Btn_Close;
		private System.Windows.Forms.Button Btn_Next;
		private System.Windows.Forms.ComboBox CB_LCNodeID;
		private System.Windows.Forms.Label Lbl_LCpullinTime;
		private System.Windows.Forms.Label Lbl_LCpullinV;
		private System.Windows.Forms.Label Lbl_LCNodeID;
		private System.Windows.Forms.Label Lbl_LCnumber;
		private System.Windows.Forms.Label Lbl_ratedPowerFactor;
		private System.Windows.Forms.Label Lbl_ratedPower;
		private System.Windows.Forms.Label Lbl_ratedLinevolts;
		private System.Windows.Forms.Label Lbl_ratedPhaseCurrent;
		private System.Windows.Forms.Label Lbl_ratedMechSpeed;
		private System.Windows.Forms.Label Lbl_ratedElecFreq;
		private System.Windows.Forms.Label LblNumPolePairs;
		private System.Windows.Forms.Label Lbl_encodePulses;
		private System.Windows.Forms.Label Lbl_Nm1;
		private System.Windows.Forms.Label Lbl_MaxTorque;
		private System.Windows.Forms.Label Lbl_rpm1;
		private System.Windows.Forms.Label Lbl_MaxMechSpeed;
		private System.Windows.Forms.Label Lbl_rpm2;
		private System.Windows.Forms.Label Lbl_A1;
		private System.Windows.Forms.Label Lbl_V3;
		private System.Windows.Forms.Label Lbl_W1;
		private System.Windows.Forms.Label Lbl_V2;
		private System.Windows.Forms.Label Lbl_V1;
		private System.Windows.Forms.Label Lbl_ms1;
		private System.Windows.Forms.Label Lbl_Hz1;
		private System.Windows.Forms.TextBox TB_MaxSpeed;
		private System.Windows.Forms.TextBox TB_MaxTorque;
		private System.Windows.Forms.Button Btn_addLC;
		private System.Windows.Forms.Button Btn_deleteLC;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.NumericUpDown NUD_testIterations;
		private System.Windows.Forms.NumericUpDown NUD_OL_MaxI;
		private System.Windows.Forms.NumericUpDown NUD_PowerFactor;
		private System.Windows.Forms.NumericUpDown NUD_torqueFactor;
		private System.Windows.Forms.NumericUpDown NUD_speedFactor;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.Label Lbl_UserInstructions;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TreeView TV_OL_testpoints;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.TreeView TV_CL_TestPoints;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TreeView TV_NLT_testPts;
		private System.Windows.Forms.TextBox TB_NLT_maxIdToIqRatio;
		private System.Windows.Forms.TextBox TB_NLT_W_rate;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button Btn_TestProfile;
		private System.Windows.Forms.Button Btn_PwrFrameProfile;
		private System.Windows.Forms.Label LblPowerFrameProfile;
		private System.Windows.Forms.Label LblTestProfile;
		private System.Windows.Forms.ContextMenu CM_NLT;
		private System.Windows.Forms.ContextMenu CM_OLoop_and_CLoop;
		private System.Windows.Forms.ContextMenu contextMenu2;
		private System.Windows.Forms.TabPage openloop;
		private System.Windows.Forms.TabPage closedloop;
		private System.Windows.Forms.TabPage noload;
		private System.Windows.Forms.TabPage powerframe;
		private System.Windows.Forms.TabPage scwizprofiles;
		private System.Windows.Forms.TabPage general;
		private System.Windows.Forms.MenuItem MIsaveTestProfile;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.TextBox TB_PF_maxBattVolts;
		private System.Windows.Forms.TextBox TB_PF_minBattVolts;
		private System.Windows.Forms.TextBox TB_PF_nomBattVolts;
		private System.Windows.Forms.TextBox TB_PF_maxIs;
		private System.Windows.Forms.TextBox TB_GEN_maxSpeedRatio;
		private System.Windows.Forms.TextBox TB_GEN_maxTorqueRatio;
		private System.Windows.Forms.TextBox TB_GEN_maxPowerRatio;
		private System.Windows.Forms.TextBox TB_OL_maxPercentCurr;
		private System.Windows.Forms.TextBox TB_OL_numIters;
		private System.Windows.Forms.TextBox TB_OL_numTestPointApps;
		private System.Windows.Forms.TextBox TB_NLT_numSamples;
		private System.Windows.Forms.TextBox TB_NLT_numSmaplesFlux;
		private System.Windows.Forms.TextBox TB_NLT_numSamplesSteadyState;
		#endregion controls declarations

		#region my declarations
		#region local copies of parameters passed from main window
		private SystemInfo sysInfo;
		private int motorIndex = 0;
		private int nodeIndex ;//cannot use MAIN_WIND.currIndex - it is set after we open this window
		#endregion local copies of parameters passed from main window
		#region datatables, dataviews, tablestyles etc
		private ProgTable table = null;
		private ProgTableStyle tablestyle = null;
		private float [] Programmingpercents = {0.33F, 0.33F, 0.33F};
		#endregion datatables, dataviews, tablestyles etc
		#region local copy of programming info passed back from Programming Component
		private string hostAppController = "", DSPAppController = "";
		private string [] downloadParams = null;
		private bool [] DldFiletestResults = null;
		private string [] controllerBootParams = null;
		#endregion local copy of programming info passed back from Programming Component
		#region threads
		private Thread selfCharDataRetrievalThread = null, dataThread = null;
		private Thread writeDCFToDeviceThread = null;
		#endregion threads
		#region parameters relating to controller programming
		private string [] NodeParams = {"Host Application Version", "DSP Application Version", "Host Boot Version", "DSP Boot Version", "Node HW/compatible HW versions",   "Non-compatible HW" ,"EEPROM format", "Node Processors", "Memory Spaces"};
		bool formClosing = false;
		private SEVCONProgrammer programmer = null;
		private progStep Prog_Step = progStep.None;
		private ushort errorHandlingLevel = 5;  //determin how much error handling of code goes on
		private bool programmedOK = true;
		#endregion parameters relating to controller programming
		private TestState testState;
		private DIFeedbackCode feedback;
		string DldFilename = "";
		int dataGrid1DefaultHeight = 0;
		private string DIErrorInfo = "";
		private VPMotor currentMotor = null;
		private CurrencyManager currManager = null;
		private ArrayList nodesAL;
		private TreeNode selectedTreeNode = null;  //for profiles testpoints

#if EXTERNAL_SELF_CHAR
		string [] SCWiz_argv = new string[21];  //length 21 in DW sine the first element in SCWiz is app filename which is inserted automatically
		int SCWizExitcode = -1;
		Process [] ExtSCProcess = null;  //array becaus emore than one instance could be active
		uint appRevisionNumber,  appProductCode;
		string appEDSfileName = "";
		bool monitoringExtProc = false, extProcComplete = false, DCFFileCreatedOK = false;
		private System.Windows.Forms.Label Lbl_TP;
		private System.Windows.Forms.Label Lbl_PF;
#else
		private selfCharClass mySelfChar;
#endif
		#endregion  my declarations

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Timers.Timer();
            this.progTimer = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.Pnl_Buttons = new System.Windows.Forms.Panel();
            this.restartTestButton = new System.Windows.Forms.Button();
            this.Btn_ChangeFile = new System.Windows.Forms.Button();
            this.Btn_Next = new System.Windows.Forms.Button();
            this.Btn_Back = new System.Windows.Forms.Button();
            this.Btn_Close = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.PnlUsrLabels = new System.Windows.Forms.Panel();
            this.Lbl_UserInstructions = new System.Windows.Forms.Label();
            this.PnlTestFiles = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.general = new System.Windows.Forms.TabPage();
            this.TB_GEN_maxPowerRatio = new System.Windows.Forms.TextBox();
            this.TB_GEN_maxTorqueRatio = new System.Windows.Forms.TextBox();
            this.TB_GEN_maxSpeedRatio = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.openloop = new System.Windows.Forms.TabPage();
            this.TB_OL_numTestPointApps = new System.Windows.Forms.TextBox();
            this.TB_OL_numIters = new System.Windows.Forms.TextBox();
            this.TB_OL_maxPercentCurr = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.TV_OL_testpoints = new System.Windows.Forms.TreeView();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.closedloop = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.TV_CL_TestPoints = new System.Windows.Forms.TreeView();
            this.noload = new System.Windows.Forms.TabPage();
            this.TB_NLT_numSamplesSteadyState = new System.Windows.Forms.TextBox();
            this.TB_NLT_numSmaplesFlux = new System.Windows.Forms.TextBox();
            this.TB_NLT_numSamples = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.TV_NLT_testPts = new System.Windows.Forms.TreeView();
            this.TB_NLT_maxIdToIqRatio = new System.Windows.Forms.TextBox();
            this.TB_NLT_W_rate = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.powerframe = new System.Windows.Forms.TabPage();
            this.TB_PF_maxIs = new System.Windows.Forms.TextBox();
            this.TB_PF_nomBattVolts = new System.Windows.Forms.TextBox();
            this.TB_PF_minBattVolts = new System.Windows.Forms.TextBox();
            this.TB_PF_maxBattVolts = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.scwizprofiles = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Lbl_PF = new System.Windows.Forms.Label();
            this.Lbl_TP = new System.Windows.Forms.Label();
            this.Btn_TestProfile = new System.Windows.Forms.Button();
            this.Btn_PwrFrameProfile = new System.Windows.Forms.Button();
            this.LblPowerFrameProfile = new System.Windows.Forms.Label();
            this.LblTestProfile = new System.Windows.Forms.Label();
            this.PnlLineContactors = new System.Windows.Forms.Panel();
            this.Btn_deleteLC = new System.Windows.Forms.Button();
            this.Btn_addLC = new System.Windows.Forms.Button();
            this.Lbl_LCnumber = new System.Windows.Forms.Label();
            this.Lbl_LCpullinTime = new System.Windows.Forms.Label();
            this.Lbl_LCpullinV = new System.Windows.Forms.Label();
            this.CB_LineContactorNo = new System.Windows.Forms.ComboBox();
            this.Lbl_V2 = new System.Windows.Forms.Label();
            this.Lbl_V1 = new System.Windows.Forms.Label();
            this.TB_DW_LC_PullInms = new System.Windows.Forms.TextBox();
            this.Lbl_LCNodeID = new System.Windows.Forms.Label();
            this.CB_LCNodeID = new System.Windows.Forms.ComboBox();
            this.TB_DW_LC_holdVolts = new System.Windows.Forms.TextBox();
            this.TB_DW_LC_PullInVolts = new System.Windows.Forms.TextBox();
            this.Lbl_LChodlvoltage = new System.Windows.Forms.Label();
            this.Lbl_ms1 = new System.Windows.Forms.Label();
            this.Pnl_ConfirmNamePlateData = new System.Windows.Forms.Panel();
            this.Lbl_Hz1 = new System.Windows.Forms.Label();
            this.Lbl_rpm2 = new System.Windows.Forms.Label();
            this.Lbl_A1 = new System.Windows.Forms.Label();
            this.Lbl_V3 = new System.Windows.Forms.Label();
            this.CB_DW_EncoderPolarity = new System.Windows.Forms.CheckBox();
            this.Lbl_W1 = new System.Windows.Forms.Label();
            this.TB_DW_RatedPowerFactor = new System.Windows.Forms.TextBox();
            this.TB_DW_RatedFreq_Elec = new System.Windows.Forms.TextBox();
            this.TB_DW_RatedLineVoltage_rms = new System.Windows.Forms.TextBox();
            this.TB_DW_RatedSpeed_Mech = new System.Windows.Forms.TextBox();
            this.TB_DW_RatedPower_Watts = new System.Windows.Forms.TextBox();
            this.TB_DW_NoOfPolePairs = new System.Windows.Forms.TextBox();
            this.TB_DW_RatedPhaseCurr_rms = new System.Windows.Forms.TextBox();
            this.TB_DW_EncoderPulsesPerRev = new System.Windows.Forms.TextBox();
            this.CB_PowerFactorUnknown = new System.Windows.Forms.CheckBox();
            this.Lbl_ratedPowerFactor = new System.Windows.Forms.Label();
            this.Lbl_ratedPower = new System.Windows.Forms.Label();
            this.Lbl_ratedLinevolts = new System.Windows.Forms.Label();
            this.Lbl_ratedPhaseCurrent = new System.Windows.Forms.Label();
            this.Lbl_ratedMechSpeed = new System.Windows.Forms.Label();
            this.Lbl_ratedElecFreq = new System.Windows.Forms.Label();
            this.LblNumPolePairs = new System.Windows.Forms.Label();
            this.Lbl_encodePulses = new System.Windows.Forms.Label();
            this.PnlDataGrids = new System.Windows.Forms.Panel();
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.Lbl_UsrProgrammingFeedback = new System.Windows.Forms.Label();
            this.PnlMotorLimits = new System.Windows.Forms.Panel();
            this.Lbl_Nm1 = new System.Windows.Forms.Label();
            this.TB_MaxSpeed = new System.Windows.Forms.TextBox();
            this.Lbl_MaxTorque = new System.Windows.Forms.Label();
            this.Lbl_rpm1 = new System.Windows.Forms.Label();
            this.TB_MaxTorque = new System.Windows.Forms.TextBox();
            this.Lbl_MaxMechSpeed = new System.Windows.Forms.Label();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.MIsaveTestProfile = new System.Windows.Forms.MenuItem();
            this.NUD_testIterations = new System.Windows.Forms.NumericUpDown();
            this.NUD_OL_MaxI = new System.Windows.Forms.NumericUpDown();
            this.NUD_PowerFactor = new System.Windows.Forms.NumericUpDown();
            this.NUD_torqueFactor = new System.Windows.Forms.NumericUpDown();
            this.NUD_speedFactor = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.CM_NLT = new System.Windows.Forms.ContextMenu();
            this.CM_OLoop_and_CLoop = new System.Windows.Forms.ContextMenu();
            this.contextMenu2 = new System.Windows.Forms.ContextMenu();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.timer1)).BeginInit();
            this.MainPanel.SuspendLayout();
            this.Pnl_Buttons.SuspendLayout();
            this.PnlUsrLabels.SuspendLayout();
            this.PnlTestFiles.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.general.SuspendLayout();
            this.openloop.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.closedloop.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.noload.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.powerframe.SuspendLayout();
            this.scwizprofiles.SuspendLayout();
            this.panel1.SuspendLayout();
            this.PnlLineContactors.SuspendLayout();
            this.Pnl_ConfirmNamePlateData.SuspendLayout();
            this.PnlDataGrids.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.PnlMotorLimits.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_testIterations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_OL_MaxI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_PowerFactor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_torqueFactor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_speedFactor)).BeginInit();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 200;
            this.timer1.SynchronizingObject = this;
            this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Elapsed);
            // 
            // progTimer
            // 
            this.progTimer.Tick += new System.EventHandler(this.progTimer_Tick);
            // 
            // textBox9
            // 
            this.textBox9.Location = new System.Drawing.Point(432, 72);
            this.textBox9.Name = "textBox9";
            this.textBox9.Size = new System.Drawing.Size(64, 20);
            this.textBox9.TabIndex = 16;
            // 
            // textBox8
            // 
            this.textBox8.Location = new System.Drawing.Point(432, 96);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(64, 20);
            this.textBox8.TabIndex = 15;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(216, 24);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(64, 20);
            this.textBox4.TabIndex = 8;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(136, 72);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(64, 20);
            this.textBox3.TabIndex = 5;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(136, 48);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(64, 20);
            this.textBox2.TabIndex = 4;
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.Pnl_Buttons);
            this.MainPanel.Controls.Add(this.progressBar1);
            this.MainPanel.Controls.Add(this.statusBar1);
            this.MainPanel.Controls.Add(this.PnlUsrLabels);
            this.MainPanel.Controls.Add(this.PnlTestFiles);
            this.MainPanel.Controls.Add(this.PnlLineContactors);
            this.MainPanel.Controls.Add(this.Pnl_ConfirmNamePlateData);
            this.MainPanel.Controls.Add(this.PnlDataGrids);
            this.MainPanel.Controls.Add(this.PnlMotorLimits);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Padding = new System.Windows.Forms.Padding(1);
            this.MainPanel.Size = new System.Drawing.Size(588, 442);
            this.MainPanel.TabIndex = 46;
            // 
            // Pnl_Buttons
            // 
            this.Pnl_Buttons.Controls.Add(this.restartTestButton);
            this.Pnl_Buttons.Controls.Add(this.Btn_ChangeFile);
            this.Pnl_Buttons.Controls.Add(this.Btn_Next);
            this.Pnl_Buttons.Controls.Add(this.Btn_Back);
            this.Pnl_Buttons.Controls.Add(this.Btn_Close);
            this.Pnl_Buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Pnl_Buttons.Location = new System.Drawing.Point(1, 360);
            this.Pnl_Buttons.Name = "Pnl_Buttons";
            this.Pnl_Buttons.Size = new System.Drawing.Size(586, 32);
            this.Pnl_Buttons.TabIndex = 50;
            // 
            // restartTestButton
            // 
            this.restartTestButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.restartTestButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.restartTestButton.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.restartTestButton.Location = new System.Drawing.Point(306, 0);
            this.restartTestButton.Name = "restartTestButton";
            this.restartTestButton.Size = new System.Drawing.Size(102, 32);
            this.restartTestButton.TabIndex = 66;
            this.restartTestButton.Text = "&Restart Tests";
            this.restartTestButton.Visible = false;
            this.restartTestButton.Click += new System.EventHandler(this.restartTestButton_Click);
            // 
            // Btn_ChangeFile
            // 
            this.Btn_ChangeFile.Dock = System.Windows.Forms.DockStyle.Left;
            this.Btn_ChangeFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_ChangeFile.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Btn_ChangeFile.Location = new System.Drawing.Point(198, 0);
            this.Btn_ChangeFile.Name = "Btn_ChangeFile";
            this.Btn_ChangeFile.Size = new System.Drawing.Size(108, 32);
            this.Btn_ChangeFile.TabIndex = 65;
            this.Btn_ChangeFile.Text = "Change &File";
            this.Btn_ChangeFile.Visible = false;
            this.Btn_ChangeFile.Click += new System.EventHandler(this.changeFileBtn_Click);
            // 
            // Btn_Next
            // 
            this.Btn_Next.Dock = System.Windows.Forms.DockStyle.Left;
            this.Btn_Next.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Next.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Btn_Next.Location = new System.Drawing.Point(96, 0);
            this.Btn_Next.Name = "Btn_Next";
            this.Btn_Next.Size = new System.Drawing.Size(102, 32);
            this.Btn_Next.TabIndex = 64;
            this.Btn_Next.Text = "&Next";
            this.Btn_Next.Click += new System.EventHandler(this.Btn_Next_Click);
            // 
            // Btn_Back
            // 
            this.Btn_Back.Dock = System.Windows.Forms.DockStyle.Left;
            this.Btn_Back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Back.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Btn_Back.Location = new System.Drawing.Point(0, 0);
            this.Btn_Back.Name = "Btn_Back";
            this.Btn_Back.Size = new System.Drawing.Size(96, 32);
            this.Btn_Back.TabIndex = 61;
            this.Btn_Back.Text = "&Back";
            this.Btn_Back.Click += new System.EventHandler(this.BtnBack_Click);
            // 
            // Btn_Close
            // 
            this.Btn_Close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Btn_Close.Dock = System.Windows.Forms.DockStyle.Right;
            this.Btn_Close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Close.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Btn_Close.Location = new System.Drawing.Point(490, 0);
            this.Btn_Close.Name = "Btn_Close";
            this.Btn_Close.Size = new System.Drawing.Size(96, 32);
            this.Btn_Close.TabIndex = 60;
            this.Btn_Close.Text = "&Close";
            this.Btn_Close.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(1, 392);
            this.progressBar1.Maximum = 20;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(586, 25);
            this.progressBar1.TabIndex = 48;
            this.progressBar1.Visible = false;
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(1, 417);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(586, 24);
            this.statusBar1.TabIndex = 46;
            // 
            // PnlUsrLabels
            // 
            this.PnlUsrLabels.Controls.Add(this.Lbl_UserInstructions);
            this.PnlUsrLabels.Dock = System.Windows.Forms.DockStyle.Top;
            this.PnlUsrLabels.Location = new System.Drawing.Point(1, 1);
            this.PnlUsrLabels.Name = "PnlUsrLabels";
            this.PnlUsrLabels.Size = new System.Drawing.Size(586, 25);
            this.PnlUsrLabels.TabIndex = 55;
            // 
            // Lbl_UserInstructions
            // 
            this.Lbl_UserInstructions.AutoSize = true;
            this.Lbl_UserInstructions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Lbl_UserInstructions.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lbl_UserInstructions.ForeColor = System.Drawing.Color.Black;
            this.Lbl_UserInstructions.Location = new System.Drawing.Point(0, 0);
            this.Lbl_UserInstructions.Name = "Lbl_UserInstructions";
            this.Lbl_UserInstructions.Size = new System.Drawing.Size(104, 16);
            this.Lbl_UserInstructions.TabIndex = 7;
            this.Lbl_UserInstructions.Text = "user instructions";
            // 
            // PnlTestFiles
            // 
            this.PnlTestFiles.BackColor = System.Drawing.SystemColors.Control;
            this.PnlTestFiles.Controls.Add(this.tabControl1);
            this.PnlTestFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PnlTestFiles.Location = new System.Drawing.Point(1, 1);
            this.PnlTestFiles.Name = "PnlTestFiles";
            this.PnlTestFiles.Size = new System.Drawing.Size(586, 440);
            this.PnlTestFiles.TabIndex = 69;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.general);
            this.tabControl1.Controls.Add(this.openloop);
            this.tabControl1.Controls.Add(this.closedloop);
            this.tabControl1.Controls.Add(this.noload);
            this.tabControl1.Controls.Add(this.powerframe);
            this.tabControl1.Controls.Add(this.scwizprofiles);
            this.tabControl1.Location = new System.Drawing.Point(0, 32);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(586, 360);
            this.tabControl1.TabIndex = 1;
            // 
            // general
            // 
            this.general.Controls.Add(this.TB_GEN_maxPowerRatio);
            this.general.Controls.Add(this.TB_GEN_maxTorqueRatio);
            this.general.Controls.Add(this.TB_GEN_maxSpeedRatio);
            this.general.Controls.Add(this.label4);
            this.general.Controls.Add(this.label5);
            this.general.Controls.Add(this.label6);
            this.general.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.general.Location = new System.Drawing.Point(4, 25);
            this.general.Name = "general";
            this.general.Size = new System.Drawing.Size(578, 331);
            this.general.TabIndex = 0;
            this.general.Text = "General";
            // 
            // TB_GEN_maxPowerRatio
            // 
            this.TB_GEN_maxPowerRatio.Location = new System.Drawing.Point(192, 88);
            this.TB_GEN_maxPowerRatio.Name = "TB_GEN_maxPowerRatio";
            this.TB_GEN_maxPowerRatio.Size = new System.Drawing.Size(56, 22);
            this.TB_GEN_maxPowerRatio.TabIndex = 8;
            this.TB_GEN_maxPowerRatio.Text = "textBox1";
            // 
            // TB_GEN_maxTorqueRatio
            // 
            this.TB_GEN_maxTorqueRatio.Location = new System.Drawing.Point(192, 56);
            this.TB_GEN_maxTorqueRatio.Name = "TB_GEN_maxTorqueRatio";
            this.TB_GEN_maxTorqueRatio.Size = new System.Drawing.Size(56, 22);
            this.TB_GEN_maxTorqueRatio.TabIndex = 7;
            this.TB_GEN_maxTorqueRatio.Text = "textBox1";
            // 
            // TB_GEN_maxSpeedRatio
            // 
            this.TB_GEN_maxSpeedRatio.Location = new System.Drawing.Point(192, 24);
            this.TB_GEN_maxSpeedRatio.Name = "TB_GEN_maxSpeedRatio";
            this.TB_GEN_maxSpeedRatio.Size = new System.Drawing.Size(56, 22);
            this.TB_GEN_maxSpeedRatio.TabIndex = 6;
            this.TB_GEN_maxSpeedRatio.Text = "textBox1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 16);
            this.label4.TabIndex = 5;
            this.label4.Text = "Max power ratio";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(102, 16);
            this.label5.TabIndex = 4;
            this.label5.Text = "Max torque ratio";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 24);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 16);
            this.label6.TabIndex = 3;
            this.label6.Text = "Max speed ratio";
            // 
            // openloop
            // 
            this.openloop.Controls.Add(this.TB_OL_numTestPointApps);
            this.openloop.Controls.Add(this.TB_OL_numIters);
            this.openloop.Controls.Add(this.TB_OL_maxPercentCurr);
            this.openloop.Controls.Add(this.label7);
            this.openloop.Controls.Add(this.groupBox1);
            this.openloop.Controls.Add(this.label8);
            this.openloop.Controls.Add(this.label9);
            this.openloop.Controls.Add(this.label10);
            this.openloop.Location = new System.Drawing.Point(4, 22);
            this.openloop.Name = "openloop";
            this.openloop.Size = new System.Drawing.Size(578, 334);
            this.openloop.TabIndex = 1;
            this.openloop.Text = "Open Loop";
            this.openloop.Visible = false;
            // 
            // TB_OL_numTestPointApps
            // 
            this.TB_OL_numTestPointApps.Location = new System.Drawing.Point(208, 96);
            this.TB_OL_numTestPointApps.Name = "TB_OL_numTestPointApps";
            this.TB_OL_numTestPointApps.Size = new System.Drawing.Size(64, 22);
            this.TB_OL_numTestPointApps.TabIndex = 12;
            this.TB_OL_numTestPointApps.Text = "textBox1";
            // 
            // TB_OL_numIters
            // 
            this.TB_OL_numIters.Location = new System.Drawing.Point(208, 64);
            this.TB_OL_numIters.Name = "TB_OL_numIters";
            this.TB_OL_numIters.Size = new System.Drawing.Size(64, 22);
            this.TB_OL_numIters.TabIndex = 11;
            this.TB_OL_numIters.Text = "textBox1";
            // 
            // TB_OL_maxPercentCurr
            // 
            this.TB_OL_maxPercentCurr.Location = new System.Drawing.Point(208, 24);
            this.TB_OL_maxPercentCurr.Name = "TB_OL_maxPercentCurr";
            this.TB_OL_maxPercentCurr.Size = new System.Drawing.Size(64, 22);
            this.TB_OL_maxPercentCurr.TabIndex = 10;
            this.TB_OL_maxPercentCurr.Text = "textBox1";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 104);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(194, 16);
            this.label7.TabIndex = 8;
            this.label7.Text = "No of times to apply Test Points";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TV_OL_testpoints);
            this.groupBox1.Location = new System.Drawing.Point(296, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(304, 352);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Test Points (Right Click to Edit)";
            // 
            // TV_OL_testpoints
            // 
            this.TV_OL_testpoints.Location = new System.Drawing.Point(8, 24);
            this.TV_OL_testpoints.Name = "TV_OL_testpoints";
            this.TV_OL_testpoints.Size = new System.Drawing.Size(288, 320);
            this.TV_OL_testpoints.TabIndex = 5;
            this.TV_OL_testpoints.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 64);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(171, 16);
            this.label8.TabIndex = 3;
            this.label8.Text = "Number of OL test iterations";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(272, 24);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(20, 16);
            this.label9.TabIndex = 2;
            this.label9.Text = "%";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 24);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(168, 16);
            this.label10.TabIndex = 0;
            this.label10.Text = "Max percantage overcurrent";
            // 
            // closedloop
            // 
            this.closedloop.Controls.Add(this.groupBox3);
            this.closedloop.Location = new System.Drawing.Point(4, 22);
            this.closedloop.Name = "closedloop";
            this.closedloop.Size = new System.Drawing.Size(578, 334);
            this.closedloop.TabIndex = 3;
            this.closedloop.Text = "Closed Loop";
            this.closedloop.Visible = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.TV_CL_TestPoints);
            this.groupBox3.Location = new System.Drawing.Point(16, 32);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(464, 320);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Tesp Points ( Right click to edit)";
            // 
            // TV_CL_TestPoints
            // 
            this.TV_CL_TestPoints.Location = new System.Drawing.Point(8, 24);
            this.TV_CL_TestPoints.Name = "TV_CL_TestPoints";
            this.TV_CL_TestPoints.Size = new System.Drawing.Size(448, 288);
            this.TV_CL_TestPoints.TabIndex = 4;
            this.TV_CL_TestPoints.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // noload
            // 
            this.noload.Controls.Add(this.TB_NLT_numSamplesSteadyState);
            this.noload.Controls.Add(this.TB_NLT_numSmaplesFlux);
            this.noload.Controls.Add(this.TB_NLT_numSamples);
            this.noload.Controls.Add(this.groupBox2);
            this.noload.Controls.Add(this.TB_NLT_maxIdToIqRatio);
            this.noload.Controls.Add(this.TB_NLT_W_rate);
            this.noload.Controls.Add(this.label16);
            this.noload.Controls.Add(this.label15);
            this.noload.Controls.Add(this.label14);
            this.noload.Controls.Add(this.label13);
            this.noload.Controls.Add(this.label12);
            this.noload.Location = new System.Drawing.Point(4, 22);
            this.noload.Name = "noload";
            this.noload.Size = new System.Drawing.Size(578, 334);
            this.noload.TabIndex = 2;
            this.noload.Text = "No Load Test";
            this.noload.Visible = false;
            // 
            // TB_NLT_numSamplesSteadyState
            // 
            this.TB_NLT_numSamplesSteadyState.Location = new System.Drawing.Point(240, 136);
            this.TB_NLT_numSamplesSteadyState.Name = "TB_NLT_numSamplesSteadyState";
            this.TB_NLT_numSamplesSteadyState.Size = new System.Drawing.Size(96, 22);
            this.TB_NLT_numSamplesSteadyState.TabIndex = 14;
            this.TB_NLT_numSamplesSteadyState.Text = "textBox1";
            // 
            // TB_NLT_numSmaplesFlux
            // 
            this.TB_NLT_numSmaplesFlux.Location = new System.Drawing.Point(240, 104);
            this.TB_NLT_numSmaplesFlux.Name = "TB_NLT_numSmaplesFlux";
            this.TB_NLT_numSmaplesFlux.Size = new System.Drawing.Size(96, 22);
            this.TB_NLT_numSmaplesFlux.TabIndex = 13;
            this.TB_NLT_numSmaplesFlux.Text = "textBox1";
            // 
            // TB_NLT_numSamples
            // 
            this.TB_NLT_numSamples.Location = new System.Drawing.Point(240, 72);
            this.TB_NLT_numSamples.Name = "TB_NLT_numSamples";
            this.TB_NLT_numSamples.Size = new System.Drawing.Size(96, 22);
            this.TB_NLT_numSamples.TabIndex = 12;
            this.TB_NLT_numSamples.Text = "textBox1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.TV_NLT_testPts);
            this.groupBox2.Location = new System.Drawing.Point(344, 16);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(248, 360);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Speed Points (Right click to edit)";
            // 
            // TV_NLT_testPts
            // 
            this.TV_NLT_testPts.Location = new System.Drawing.Point(16, 32);
            this.TV_NLT_testPts.Name = "TV_NLT_testPts";
            this.TV_NLT_testPts.Size = new System.Drawing.Size(224, 312);
            this.TV_NLT_testPts.TabIndex = 0;
            this.TV_NLT_testPts.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // TB_NLT_maxIdToIqRatio
            // 
            this.TB_NLT_maxIdToIqRatio.Location = new System.Drawing.Point(240, 168);
            this.TB_NLT_maxIdToIqRatio.Name = "TB_NLT_maxIdToIqRatio";
            this.TB_NLT_maxIdToIqRatio.Size = new System.Drawing.Size(96, 22);
            this.TB_NLT_maxIdToIqRatio.TabIndex = 10;
            // 
            // TB_NLT_W_rate
            // 
            this.TB_NLT_W_rate.AcceptsReturn = true;
            this.TB_NLT_W_rate.AcceptsTab = true;
            this.TB_NLT_W_rate.Location = new System.Drawing.Point(240, 40);
            this.TB_NLT_W_rate.MaxLength = 10;
            this.TB_NLT_W_rate.Name = "TB_NLT_W_rate";
            this.TB_NLT_W_rate.Size = new System.Drawing.Size(96, 22);
            this.TB_NLT_W_rate.TabIndex = 6;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(16, 168);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(122, 16);
            this.label16.TabIndex = 4;
            this.label16.Text = "Maximum Id/Iq ratio";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(16, 136);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(218, 16);
            this.label15.TabIndex = 3;
            this.label15.Text = "Num samples for steady state settle";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(16, 104);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(212, 16);
            this.label14.TabIndex = 2;
            this.label14.Text = "Num samples for flux change settle";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(16, 72);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(120, 16);
            this.label13.TabIndex = 1;
            this.label13.Text = "Number of samples";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 40);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(46, 16);
            this.label12.TabIndex = 0;
            this.label12.Text = "w_rate";
            // 
            // powerframe
            // 
            this.powerframe.Controls.Add(this.TB_PF_maxIs);
            this.powerframe.Controls.Add(this.TB_PF_nomBattVolts);
            this.powerframe.Controls.Add(this.TB_PF_minBattVolts);
            this.powerframe.Controls.Add(this.TB_PF_maxBattVolts);
            this.powerframe.Controls.Add(this.label11);
            this.powerframe.Controls.Add(this.label17);
            this.powerframe.Controls.Add(this.label18);
            this.powerframe.Controls.Add(this.label19);
            this.powerframe.Location = new System.Drawing.Point(4, 22);
            this.powerframe.Name = "powerframe";
            this.powerframe.Size = new System.Drawing.Size(578, 334);
            this.powerframe.TabIndex = 4;
            this.powerframe.Text = "PowerFrame";
            this.powerframe.Visible = false;
            // 
            // TB_PF_maxIs
            // 
            this.TB_PF_maxIs.Location = new System.Drawing.Point(216, 160);
            this.TB_PF_maxIs.Name = "TB_PF_maxIs";
            this.TB_PF_maxIs.Size = new System.Drawing.Size(64, 22);
            this.TB_PF_maxIs.TabIndex = 11;
            this.TB_PF_maxIs.Text = "textBox1";
            // 
            // TB_PF_nomBattVolts
            // 
            this.TB_PF_nomBattVolts.Location = new System.Drawing.Point(216, 120);
            this.TB_PF_nomBattVolts.Name = "TB_PF_nomBattVolts";
            this.TB_PF_nomBattVolts.Size = new System.Drawing.Size(64, 22);
            this.TB_PF_nomBattVolts.TabIndex = 10;
            this.TB_PF_nomBattVolts.Text = "textBox1";
            // 
            // TB_PF_minBattVolts
            // 
            this.TB_PF_minBattVolts.Location = new System.Drawing.Point(216, 80);
            this.TB_PF_minBattVolts.Name = "TB_PF_minBattVolts";
            this.TB_PF_minBattVolts.Size = new System.Drawing.Size(64, 22);
            this.TB_PF_minBattVolts.TabIndex = 9;
            this.TB_PF_minBattVolts.Text = "textBox1";
            // 
            // TB_PF_maxBattVolts
            // 
            this.TB_PF_maxBattVolts.Location = new System.Drawing.Point(216, 40);
            this.TB_PF_maxBattVolts.Name = "TB_PF_maxBattVolts";
            this.TB_PF_maxBattVolts.Size = new System.Drawing.Size(64, 22);
            this.TB_PF_maxBattVolts.TabIndex = 8;
            this.TB_PF_maxBattVolts.Text = "textBox1";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(24, 168);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(152, 32);
            this.label11.TabIndex = 3;
            this.label11.Text = "Maximum Is current";
            // 
            // label17
            // 
            this.label17.Location = new System.Drawing.Point(24, 128);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(144, 24);
            this.label17.TabIndex = 2;
            this.label17.Text = "Nominal Battery Volts";
            // 
            // label18
            // 
            this.label18.Location = new System.Drawing.Point(24, 40);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(168, 32);
            this.label18.TabIndex = 1;
            this.label18.Text = "Maximum Battery Volts";
            // 
            // label19
            // 
            this.label19.Location = new System.Drawing.Point(24, 80);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(168, 32);
            this.label19.TabIndex = 0;
            this.label19.Text = "Minimum Battery volts";
            // 
            // scwizprofiles
            // 
            this.scwizprofiles.Controls.Add(this.panel1);
            this.scwizprofiles.Location = new System.Drawing.Point(4, 22);
            this.scwizprofiles.Name = "scwizprofiles";
            this.scwizprofiles.Size = new System.Drawing.Size(578, 334);
            this.scwizprofiles.TabIndex = 5;
            this.scwizprofiles.Text = "SCWiz Profiles";
            this.scwizprofiles.Visible = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.Lbl_PF);
            this.panel1.Controls.Add(this.Lbl_TP);
            this.panel1.Controls.Add(this.Btn_TestProfile);
            this.panel1.Controls.Add(this.Btn_PwrFrameProfile);
            this.panel1.Controls.Add(this.LblPowerFrameProfile);
            this.panel1.Controls.Add(this.LblTestProfile);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(578, 334);
            this.panel1.TabIndex = 70;
            // 
            // Lbl_PF
            // 
            this.Lbl_PF.Location = new System.Drawing.Point(12, 160);
            this.Lbl_PF.Name = "Lbl_PF";
            this.Lbl_PF.Size = new System.Drawing.Size(150, 19);
            this.Lbl_PF.TabIndex = 5;
            this.Lbl_PF.Text = "Power Frame Profile:";
            // 
            // Lbl_TP
            // 
            this.Lbl_TP.Location = new System.Drawing.Point(12, 38);
            this.Lbl_TP.Name = "Lbl_TP";
            this.Lbl_TP.Size = new System.Drawing.Size(138, 19);
            this.Lbl_TP.TabIndex = 4;
            this.Lbl_TP.Text = "Test Profile:";
            // 
            // Btn_TestProfile
            // 
            this.Btn_TestProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_TestProfile.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Btn_TestProfile.Location = new System.Drawing.Point(336, 120);
            this.Btn_TestProfile.Name = "Btn_TestProfile";
            this.Btn_TestProfile.Size = new System.Drawing.Size(216, 25);
            this.Btn_TestProfile.TabIndex = 3;
            this.Btn_TestProfile.Text = "Change Test Profile";
            this.Btn_TestProfile.Click += new System.EventHandler(this.changeTPfilepathBtn_Click);
            // 
            // Btn_PwrFrameProfile
            // 
            this.Btn_PwrFrameProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_PwrFrameProfile.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Btn_PwrFrameProfile.Location = new System.Drawing.Point(336, 264);
            this.Btn_PwrFrameProfile.Name = "Btn_PwrFrameProfile";
            this.Btn_PwrFrameProfile.Size = new System.Drawing.Size(216, 25);
            this.Btn_PwrFrameProfile.TabIndex = 2;
            this.Btn_PwrFrameProfile.Text = "Change Power Frame Profile";
            this.Btn_PwrFrameProfile.Click += new System.EventHandler(this.ChangePFRFilepath_Click);
            // 
            // LblPowerFrameProfile
            // 
            this.LblPowerFrameProfile.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblPowerFrameProfile.Location = new System.Drawing.Point(16, 192);
            this.LblPowerFrameProfile.Name = "LblPowerFrameProfile";
            this.LblPowerFrameProfile.Size = new System.Drawing.Size(552, 57);
            this.LblPowerFrameProfile.TabIndex = 1;
            this.LblPowerFrameProfile.Text = "not found";
            // 
            // LblTestProfile
            // 
            this.LblTestProfile.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblTestProfile.Location = new System.Drawing.Point(8, 57);
            this.LblTestProfile.Name = "LblTestProfile";
            this.LblTestProfile.Size = new System.Drawing.Size(560, 57);
            this.LblTestProfile.TabIndex = 0;
            this.LblTestProfile.Text = "not found";
            // 
            // PnlLineContactors
            // 
            this.PnlLineContactors.BackColor = System.Drawing.SystemColors.Control;
            this.PnlLineContactors.Controls.Add(this.Btn_deleteLC);
            this.PnlLineContactors.Controls.Add(this.Btn_addLC);
            this.PnlLineContactors.Controls.Add(this.Lbl_LCnumber);
            this.PnlLineContactors.Controls.Add(this.Lbl_LCpullinTime);
            this.PnlLineContactors.Controls.Add(this.Lbl_LCpullinV);
            this.PnlLineContactors.Controls.Add(this.CB_LineContactorNo);
            this.PnlLineContactors.Controls.Add(this.Lbl_V2);
            this.PnlLineContactors.Controls.Add(this.Lbl_V1);
            this.PnlLineContactors.Controls.Add(this.TB_DW_LC_PullInms);
            this.PnlLineContactors.Controls.Add(this.Lbl_LCNodeID);
            this.PnlLineContactors.Controls.Add(this.CB_LCNodeID);
            this.PnlLineContactors.Controls.Add(this.TB_DW_LC_holdVolts);
            this.PnlLineContactors.Controls.Add(this.TB_DW_LC_PullInVolts);
            this.PnlLineContactors.Controls.Add(this.Lbl_LChodlvoltage);
            this.PnlLineContactors.Controls.Add(this.Lbl_ms1);
            this.PnlLineContactors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PnlLineContactors.Location = new System.Drawing.Point(1, 1);
            this.PnlLineContactors.Name = "PnlLineContactors";
            this.PnlLineContactors.Size = new System.Drawing.Size(586, 440);
            this.PnlLineContactors.TabIndex = 57;
            // 
            // Btn_deleteLC
            // 
            this.Btn_deleteLC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_deleteLC.Location = new System.Drawing.Point(432, 51);
            this.Btn_deleteLC.Name = "Btn_deleteLC";
            this.Btn_deleteLC.Size = new System.Drawing.Size(75, 31);
            this.Btn_deleteLC.TabIndex = 85;
            this.Btn_deleteLC.Text = "&Delete";
            this.Btn_deleteLC.Click += new System.EventHandler(this.Btn_deleteLC_Click);
            // 
            // Btn_addLC
            // 
            this.Btn_addLC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_addLC.Location = new System.Drawing.Point(342, 51);
            this.Btn_addLC.Name = "Btn_addLC";
            this.Btn_addLC.Size = new System.Drawing.Size(75, 31);
            this.Btn_addLC.TabIndex = 84;
            this.Btn_addLC.Text = "&Add";
            this.Btn_addLC.Click += new System.EventHandler(this.Btn_addLC_Click);
            // 
            // Lbl_LCnumber
            // 
            this.Lbl_LCnumber.Location = new System.Drawing.Point(18, 51);
            this.Lbl_LCnumber.Name = "Lbl_LCnumber";
            this.Lbl_LCnumber.Size = new System.Drawing.Size(132, 18);
            this.Lbl_LCnumber.TabIndex = 83;
            this.Lbl_LCnumber.Text = "Line contactor number";
            // 
            // Lbl_LCpullinTime
            // 
            this.Lbl_LCpullinTime.Location = new System.Drawing.Point(18, 101);
            this.Lbl_LCpullinTime.Name = "Lbl_LCpullinTime";
            this.Lbl_LCpullinTime.Size = new System.Drawing.Size(114, 19);
            this.Lbl_LCpullinTime.TabIndex = 82;
            this.Lbl_LCpullinTime.Text = "Pull in time";
            // 
            // Lbl_LCpullinV
            // 
            this.Lbl_LCpullinV.Location = new System.Drawing.Point(18, 76);
            this.Lbl_LCpullinV.Name = "Lbl_LCpullinV";
            this.Lbl_LCpullinV.Size = new System.Drawing.Size(114, 19);
            this.Lbl_LCpullinV.TabIndex = 81;
            this.Lbl_LCpullinV.Text = "Pull in voltage";
            // 
            // CB_LineContactorNo
            // 
            this.CB_LineContactorNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CB_LineContactorNo.Location = new System.Drawing.Point(150, 51);
            this.CB_LineContactorNo.Name = "CB_LineContactorNo";
            this.CB_LineContactorNo.Size = new System.Drawing.Size(75, 24);
            this.CB_LineContactorNo.TabIndex = 79;
            this.CB_LineContactorNo.SelectionChangeCommitted += new System.EventHandler(this.CB_LineContactorNo_SelectionChangeCommitted);
            // 
            // Lbl_V2
            // 
            this.Lbl_V2.Location = new System.Drawing.Point(228, 126);
            this.Lbl_V2.Name = "Lbl_V2";
            this.Lbl_V2.Size = new System.Drawing.Size(17, 22);
            this.Lbl_V2.TabIndex = 78;
            this.Lbl_V2.Text = "V";
            this.Lbl_V2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_V1
            // 
            this.Lbl_V1.Location = new System.Drawing.Point(228, 76);
            this.Lbl_V1.Name = "Lbl_V1";
            this.Lbl_V1.Size = new System.Drawing.Size(17, 22);
            this.Lbl_V1.TabIndex = 77;
            this.Lbl_V1.Text = "V";
            this.Lbl_V1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TB_DW_LC_PullInms
            // 
            this.TB_DW_LC_PullInms.Location = new System.Drawing.Point(150, 101);
            this.TB_DW_LC_PullInms.Name = "TB_DW_LC_PullInms";
            this.TB_DW_LC_PullInms.Size = new System.Drawing.Size(75, 22);
            this.TB_DW_LC_PullInms.TabIndex = 76;
            // 
            // Lbl_LCNodeID
            // 
            this.Lbl_LCNodeID.Location = new System.Drawing.Point(18, 152);
            this.Lbl_LCNodeID.Name = "Lbl_LCNodeID";
            this.Lbl_LCNodeID.Size = new System.Drawing.Size(126, 21);
            this.Lbl_LCNodeID.TabIndex = 75;
            this.Lbl_LCNodeID.Text = "Controlling node ID:";
            this.Lbl_LCNodeID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CB_LCNodeID
            // 
            this.CB_LCNodeID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CB_LCNodeID.Location = new System.Drawing.Point(150, 152);
            this.CB_LCNodeID.Name = "CB_LCNodeID";
            this.CB_LCNodeID.Size = new System.Drawing.Size(75, 24);
            this.CB_LCNodeID.TabIndex = 74;
            // 
            // TB_DW_LC_holdVolts
            // 
            this.TB_DW_LC_holdVolts.Location = new System.Drawing.Point(150, 126);
            this.TB_DW_LC_holdVolts.Name = "TB_DW_LC_holdVolts";
            this.TB_DW_LC_holdVolts.Size = new System.Drawing.Size(75, 22);
            this.TB_DW_LC_holdVolts.TabIndex = 73;
            // 
            // TB_DW_LC_PullInVolts
            // 
            this.TB_DW_LC_PullInVolts.Location = new System.Drawing.Point(150, 76);
            this.TB_DW_LC_PullInVolts.Name = "TB_DW_LC_PullInVolts";
            this.TB_DW_LC_PullInVolts.Size = new System.Drawing.Size(75, 22);
            this.TB_DW_LC_PullInVolts.TabIndex = 72;
            // 
            // Lbl_LChodlvoltage
            // 
            this.Lbl_LChodlvoltage.Location = new System.Drawing.Point(18, 120);
            this.Lbl_LChodlvoltage.Name = "Lbl_LChodlvoltage";
            this.Lbl_LChodlvoltage.Size = new System.Drawing.Size(126, 22);
            this.Lbl_LChodlvoltage.TabIndex = 71;
            this.Lbl_LChodlvoltage.Text = "Hold voltage";
            this.Lbl_LChodlvoltage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_ms1
            // 
            this.Lbl_ms1.Location = new System.Drawing.Point(228, 101);
            this.Lbl_ms1.Name = "Lbl_ms1";
            this.Lbl_ms1.Size = new System.Drawing.Size(24, 22);
            this.Lbl_ms1.TabIndex = 66;
            this.Lbl_ms1.Text = "ms";
            this.Lbl_ms1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Pnl_ConfirmNamePlateData
            // 
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_Hz1);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_rpm2);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_A1);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_V3);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.CB_DW_EncoderPolarity);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_W1);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.TB_DW_RatedPowerFactor);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.TB_DW_RatedFreq_Elec);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.TB_DW_RatedLineVoltage_rms);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.TB_DW_RatedSpeed_Mech);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.TB_DW_RatedPower_Watts);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.TB_DW_NoOfPolePairs);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.TB_DW_RatedPhaseCurr_rms);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.TB_DW_EncoderPulsesPerRev);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.CB_PowerFactorUnknown);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_ratedPowerFactor);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_ratedPower);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_ratedLinevolts);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_ratedPhaseCurrent);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_ratedMechSpeed);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_ratedElecFreq);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.LblNumPolePairs);
            this.Pnl_ConfirmNamePlateData.Controls.Add(this.Lbl_encodePulses);
            this.Pnl_ConfirmNamePlateData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Pnl_ConfirmNamePlateData.Location = new System.Drawing.Point(1, 1);
            this.Pnl_ConfirmNamePlateData.Name = "Pnl_ConfirmNamePlateData";
            this.Pnl_ConfirmNamePlateData.Size = new System.Drawing.Size(586, 440);
            this.Pnl_ConfirmNamePlateData.TabIndex = 54;
            // 
            // Lbl_Hz1
            // 
            this.Lbl_Hz1.Location = new System.Drawing.Point(270, 192);
            this.Lbl_Hz1.Name = "Lbl_Hz1";
            this.Lbl_Hz1.Size = new System.Drawing.Size(24, 23);
            this.Lbl_Hz1.TabIndex = 48;
            this.Lbl_Hz1.Text = "Hz";
            this.Lbl_Hz1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_rpm2
            // 
            this.Lbl_rpm2.Location = new System.Drawing.Point(270, 88);
            this.Lbl_rpm2.Name = "Lbl_rpm2";
            this.Lbl_rpm2.Size = new System.Drawing.Size(32, 23);
            this.Lbl_rpm2.TabIndex = 47;
            this.Lbl_rpm2.Text = "rpm";
            this.Lbl_rpm2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_A1
            // 
            this.Lbl_A1.Location = new System.Drawing.Point(270, 160);
            this.Lbl_A1.Name = "Lbl_A1";
            this.Lbl_A1.Size = new System.Drawing.Size(16, 22);
            this.Lbl_A1.TabIndex = 46;
            this.Lbl_A1.Text = "A";
            this.Lbl_A1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_V3
            // 
            this.Lbl_V3.Location = new System.Drawing.Point(270, 63);
            this.Lbl_V3.Name = "Lbl_V3";
            this.Lbl_V3.Size = new System.Drawing.Size(16, 22);
            this.Lbl_V3.TabIndex = 45;
            this.Lbl_V3.Text = "V";
            this.Lbl_V3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CB_DW_EncoderPolarity
            // 
            this.CB_DW_EncoderPolarity.Location = new System.Drawing.Point(270, 38);
            this.CB_DW_EncoderPolarity.Name = "CB_DW_EncoderPolarity";
            this.CB_DW_EncoderPolarity.Size = new System.Drawing.Size(80, 22);
            this.CB_DW_EncoderPolarity.TabIndex = 44;
            this.CB_DW_EncoderPolarity.Text = "invert";
            // 
            // Lbl_W1
            // 
            this.Lbl_W1.Location = new System.Drawing.Point(272, 112);
            this.Lbl_W1.Name = "Lbl_W1";
            this.Lbl_W1.Size = new System.Drawing.Size(32, 21);
            this.Lbl_W1.TabIndex = 43;
            this.Lbl_W1.Text = "kW";
            this.Lbl_W1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TB_DW_RatedPowerFactor
            // 
            this.TB_DW_RatedPowerFactor.Location = new System.Drawing.Point(192, 216);
            this.TB_DW_RatedPowerFactor.MaxLength = 4;
            this.TB_DW_RatedPowerFactor.Name = "TB_DW_RatedPowerFactor";
            this.TB_DW_RatedPowerFactor.Size = new System.Drawing.Size(75, 22);
            this.TB_DW_RatedPowerFactor.TabIndex = 42;
            this.TB_DW_RatedPowerFactor.Text = "0.9";
            // 
            // TB_DW_RatedFreq_Elec
            // 
            this.TB_DW_RatedFreq_Elec.Location = new System.Drawing.Point(192, 192);
            this.TB_DW_RatedFreq_Elec.MaxLength = 3;
            this.TB_DW_RatedFreq_Elec.Name = "TB_DW_RatedFreq_Elec";
            this.TB_DW_RatedFreq_Elec.Size = new System.Drawing.Size(75, 22);
            this.TB_DW_RatedFreq_Elec.TabIndex = 41;
            this.TB_DW_RatedFreq_Elec.Text = "115";
            // 
            // TB_DW_RatedLineVoltage_rms
            // 
            this.TB_DW_RatedLineVoltage_rms.Location = new System.Drawing.Point(192, 63);
            this.TB_DW_RatedLineVoltage_rms.MaxLength = 3;
            this.TB_DW_RatedLineVoltage_rms.Name = "TB_DW_RatedLineVoltage_rms";
            this.TB_DW_RatedLineVoltage_rms.Size = new System.Drawing.Size(75, 22);
            this.TB_DW_RatedLineVoltage_rms.TabIndex = 40;
            this.TB_DW_RatedLineVoltage_rms.Text = "34";
            // 
            // TB_DW_RatedSpeed_Mech
            // 
            this.TB_DW_RatedSpeed_Mech.Location = new System.Drawing.Point(192, 88);
            this.TB_DW_RatedSpeed_Mech.MaxLength = 4;
            this.TB_DW_RatedSpeed_Mech.Name = "TB_DW_RatedSpeed_Mech";
            this.TB_DW_RatedSpeed_Mech.Size = new System.Drawing.Size(75, 22);
            this.TB_DW_RatedSpeed_Mech.TabIndex = 39;
            this.TB_DW_RatedSpeed_Mech.Text = "3372";
            // 
            // TB_DW_RatedPower_Watts
            // 
            this.TB_DW_RatedPower_Watts.Location = new System.Drawing.Point(192, 112);
            this.TB_DW_RatedPower_Watts.MaxLength = 3;
            this.TB_DW_RatedPower_Watts.Name = "TB_DW_RatedPower_Watts";
            this.TB_DW_RatedPower_Watts.Size = new System.Drawing.Size(75, 22);
            this.TB_DW_RatedPower_Watts.TabIndex = 38;
            this.TB_DW_RatedPower_Watts.Text = "48";
            // 
            // TB_DW_NoOfPolePairs
            // 
            this.TB_DW_NoOfPolePairs.Location = new System.Drawing.Point(192, 136);
            this.TB_DW_NoOfPolePairs.MaxLength = 2;
            this.TB_DW_NoOfPolePairs.Name = "TB_DW_NoOfPolePairs";
            this.TB_DW_NoOfPolePairs.Size = new System.Drawing.Size(75, 22);
            this.TB_DW_NoOfPolePairs.TabIndex = 37;
            this.TB_DW_NoOfPolePairs.Text = "2";
            // 
            // TB_DW_RatedPhaseCurr_rms
            // 
            this.TB_DW_RatedPhaseCurr_rms.Location = new System.Drawing.Point(192, 160);
            this.TB_DW_RatedPhaseCurr_rms.MaxLength = 3;
            this.TB_DW_RatedPhaseCurr_rms.Name = "TB_DW_RatedPhaseCurr_rms";
            this.TB_DW_RatedPhaseCurr_rms.Size = new System.Drawing.Size(75, 22);
            this.TB_DW_RatedPhaseCurr_rms.TabIndex = 36;
            this.TB_DW_RatedPhaseCurr_rms.Text = "110";
            // 
            // TB_DW_EncoderPulsesPerRev
            // 
            this.TB_DW_EncoderPulsesPerRev.Location = new System.Drawing.Point(192, 38);
            this.TB_DW_EncoderPulsesPerRev.MaxLength = 3;
            this.TB_DW_EncoderPulsesPerRev.Name = "TB_DW_EncoderPulsesPerRev";
            this.TB_DW_EncoderPulsesPerRev.Size = new System.Drawing.Size(75, 22);
            this.TB_DW_EncoderPulsesPerRev.TabIndex = 35;
            this.TB_DW_EncoderPulsesPerRev.Text = "80";
            // 
            // CB_PowerFactorUnknown
            // 
            this.CB_PowerFactorUnknown.Location = new System.Drawing.Point(270, 216);
            this.CB_PowerFactorUnknown.Name = "CB_PowerFactorUnknown";
            this.CB_PowerFactorUnknown.Size = new System.Drawing.Size(80, 21);
            this.CB_PowerFactorUnknown.TabIndex = 34;
            this.CB_PowerFactorUnknown.Text = "unknown";
            // 
            // Lbl_ratedPowerFactor
            // 
            this.Lbl_ratedPowerFactor.Location = new System.Drawing.Point(6, 216);
            this.Lbl_ratedPowerFactor.Name = "Lbl_ratedPowerFactor";
            this.Lbl_ratedPowerFactor.Size = new System.Drawing.Size(180, 21);
            this.Lbl_ratedPowerFactor.TabIndex = 33;
            this.Lbl_ratedPowerFactor.Text = "Rated power factor";
            this.Lbl_ratedPowerFactor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_ratedPower
            // 
            this.Lbl_ratedPower.Location = new System.Drawing.Point(6, 112);
            this.Lbl_ratedPower.Name = "Lbl_ratedPower";
            this.Lbl_ratedPower.Size = new System.Drawing.Size(180, 21);
            this.Lbl_ratedPower.TabIndex = 32;
            this.Lbl_ratedPower.Text = "Rated power";
            this.Lbl_ratedPower.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_ratedLinevolts
            // 
            this.Lbl_ratedLinevolts.Location = new System.Drawing.Point(6, 63);
            this.Lbl_ratedLinevolts.Name = "Lbl_ratedLinevolts";
            this.Lbl_ratedLinevolts.Size = new System.Drawing.Size(180, 22);
            this.Lbl_ratedLinevolts.TabIndex = 31;
            this.Lbl_ratedLinevolts.Text = "Rated line voltage (rms)";
            this.Lbl_ratedLinevolts.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_ratedPhaseCurrent
            // 
            this.Lbl_ratedPhaseCurrent.Location = new System.Drawing.Point(6, 160);
            this.Lbl_ratedPhaseCurrent.Name = "Lbl_ratedPhaseCurrent";
            this.Lbl_ratedPhaseCurrent.Size = new System.Drawing.Size(180, 22);
            this.Lbl_ratedPhaseCurrent.TabIndex = 30;
            this.Lbl_ratedPhaseCurrent.Text = "Rated phase current (rms)";
            this.Lbl_ratedPhaseCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_ratedMechSpeed
            // 
            this.Lbl_ratedMechSpeed.Location = new System.Drawing.Point(6, 88);
            this.Lbl_ratedMechSpeed.Name = "Lbl_ratedMechSpeed";
            this.Lbl_ratedMechSpeed.Size = new System.Drawing.Size(180, 23);
            this.Lbl_ratedMechSpeed.TabIndex = 29;
            this.Lbl_ratedMechSpeed.Text = "Rated speed (mechanical)";
            this.Lbl_ratedMechSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_ratedElecFreq
            // 
            this.Lbl_ratedElecFreq.Location = new System.Drawing.Point(6, 192);
            this.Lbl_ratedElecFreq.Name = "Lbl_ratedElecFreq";
            this.Lbl_ratedElecFreq.Size = new System.Drawing.Size(180, 23);
            this.Lbl_ratedElecFreq.TabIndex = 28;
            this.Lbl_ratedElecFreq.Text = "Rated frequency (electrical)";
            this.Lbl_ratedElecFreq.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LblNumPolePairs
            // 
            this.LblNumPolePairs.Location = new System.Drawing.Point(6, 136);
            this.LblNumPolePairs.Name = "LblNumPolePairs";
            this.LblNumPolePairs.Size = new System.Drawing.Size(180, 22);
            this.LblNumPolePairs.TabIndex = 27;
            this.LblNumPolePairs.Text = "Number of pole pairs";
            this.LblNumPolePairs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_encodePulses
            // 
            this.Lbl_encodePulses.Location = new System.Drawing.Point(6, 38);
            this.Lbl_encodePulses.Name = "Lbl_encodePulses";
            this.Lbl_encodePulses.Size = new System.Drawing.Size(180, 22);
            this.Lbl_encodePulses.TabIndex = 26;
            this.Lbl_encodePulses.Text = "Encoder pulses per rev";
            this.Lbl_encodePulses.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PnlDataGrids
            // 
            this.PnlDataGrids.Controls.Add(this.dataGrid1);
            this.PnlDataGrids.Controls.Add(this.Lbl_UsrProgrammingFeedback);
            this.PnlDataGrids.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PnlDataGrids.Location = new System.Drawing.Point(1, 1);
            this.PnlDataGrids.Name = "PnlDataGrids";
            this.PnlDataGrids.Size = new System.Drawing.Size(586, 440);
            this.PnlDataGrids.TabIndex = 51;
            // 
            // dataGrid1
            // 
            this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid1.DataMember = "";
            this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Location = new System.Drawing.Point(8, 40);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.PreferredRowHeight = 20;
            this.dataGrid1.Size = new System.Drawing.Size(570, 312);
            this.dataGrid1.TabIndex = 0;
            // 
            // Lbl_UsrProgrammingFeedback
            // 
            this.Lbl_UsrProgrammingFeedback.Dock = System.Windows.Forms.DockStyle.Top;
            this.Lbl_UsrProgrammingFeedback.Location = new System.Drawing.Point(0, 0);
            this.Lbl_UsrProgrammingFeedback.Name = "Lbl_UsrProgrammingFeedback";
            this.Lbl_UsrProgrammingFeedback.Size = new System.Drawing.Size(586, 17);
            this.Lbl_UsrProgrammingFeedback.TabIndex = 46;
            // 
            // PnlMotorLimits
            // 
            this.PnlMotorLimits.Controls.Add(this.Lbl_Nm1);
            this.PnlMotorLimits.Controls.Add(this.TB_MaxSpeed);
            this.PnlMotorLimits.Controls.Add(this.Lbl_MaxTorque);
            this.PnlMotorLimits.Controls.Add(this.Lbl_rpm1);
            this.PnlMotorLimits.Controls.Add(this.TB_MaxTorque);
            this.PnlMotorLimits.Controls.Add(this.Lbl_MaxMechSpeed);
            this.PnlMotorLimits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PnlMotorLimits.Location = new System.Drawing.Point(1, 1);
            this.PnlMotorLimits.Name = "PnlMotorLimits";
            this.PnlMotorLimits.Size = new System.Drawing.Size(586, 440);
            this.PnlMotorLimits.TabIndex = 7;
            // 
            // Lbl_Nm1
            // 
            this.Lbl_Nm1.Location = new System.Drawing.Point(276, 69);
            this.Lbl_Nm1.Name = "Lbl_Nm1";
            this.Lbl_Nm1.Size = new System.Drawing.Size(30, 22);
            this.Lbl_Nm1.TabIndex = 32;
            this.Lbl_Nm1.Text = "Nm";
            this.Lbl_Nm1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TB_MaxSpeed
            // 
            this.TB_MaxSpeed.Location = new System.Drawing.Point(192, 44);
            this.TB_MaxSpeed.MaxLength = 5;
            this.TB_MaxSpeed.Name = "TB_MaxSpeed";
            this.TB_MaxSpeed.Size = new System.Drawing.Size(75, 22);
            this.TB_MaxSpeed.TabIndex = 31;
            this.TB_MaxSpeed.Text = "48";
            // 
            // Lbl_MaxTorque
            // 
            this.Lbl_MaxTorque.Location = new System.Drawing.Point(6, 63);
            this.Lbl_MaxTorque.Name = "Lbl_MaxTorque";
            this.Lbl_MaxTorque.Size = new System.Drawing.Size(102, 21);
            this.Lbl_MaxTorque.TabIndex = 30;
            this.Lbl_MaxTorque.Text = "Max torque";
            this.Lbl_MaxTorque.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Lbl_rpm1
            // 
            this.Lbl_rpm1.Location = new System.Drawing.Point(276, 44);
            this.Lbl_rpm1.Name = "Lbl_rpm1";
            this.Lbl_rpm1.Size = new System.Drawing.Size(36, 22);
            this.Lbl_rpm1.TabIndex = 29;
            this.Lbl_rpm1.Text = "rpm";
            this.Lbl_rpm1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TB_MaxTorque
            // 
            this.TB_MaxTorque.Location = new System.Drawing.Point(192, 69);
            this.TB_MaxTorque.MaxLength = 4;
            this.TB_MaxTorque.Name = "TB_MaxTorque";
            this.TB_MaxTorque.Size = new System.Drawing.Size(75, 22);
            this.TB_MaxTorque.TabIndex = 28;
            this.TB_MaxTorque.Text = "48";
            this.TB_MaxTorque.WordWrap = false;
            // 
            // Lbl_MaxMechSpeed
            // 
            this.Lbl_MaxMechSpeed.Location = new System.Drawing.Point(6, 44);
            this.Lbl_MaxMechSpeed.Name = "Lbl_MaxMechSpeed";
            this.Lbl_MaxMechSpeed.Size = new System.Drawing.Size(180, 22);
            this.Lbl_MaxMechSpeed.TabIndex = 27;
            this.Lbl_MaxMechSpeed.Text = "Max speed (mechanical)";
            this.Lbl_MaxMechSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2,
            this.MIsaveTestProfile});
            this.menuItem1.Text = "SelfChar";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.Text = "Open Test from file";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // MIsaveTestProfile
            // 
            this.MIsaveTestProfile.Index = 1;
            this.MIsaveTestProfile.Text = "Save Test Profile Data to file";
            this.MIsaveTestProfile.Click += new System.EventHandler(this.MIsaveTestProfile_Click);
            // 
            // NUD_testIterations
            // 
            this.NUD_testIterations.Location = new System.Drawing.Point(168, 56);
            this.NUD_testIterations.Name = "NUD_testIterations";
            this.NUD_testIterations.Size = new System.Drawing.Size(64, 20);
            this.NUD_testIterations.TabIndex = 3;
            // 
            // NUD_OL_MaxI
            // 
            this.NUD_OL_MaxI.Location = new System.Drawing.Point(184, 32);
            this.NUD_OL_MaxI.Name = "NUD_OL_MaxI";
            this.NUD_OL_MaxI.Size = new System.Drawing.Size(80, 20);
            this.NUD_OL_MaxI.TabIndex = 1;
            // 
            // NUD_PowerFactor
            // 
            this.NUD_PowerFactor.Location = new System.Drawing.Point(144, 80);
            this.NUD_PowerFactor.Name = "NUD_PowerFactor";
            this.NUD_PowerFactor.Size = new System.Drawing.Size(56, 20);
            this.NUD_PowerFactor.TabIndex = 8;
            // 
            // NUD_torqueFactor
            // 
            this.NUD_torqueFactor.Location = new System.Drawing.Point(144, 56);
            this.NUD_torqueFactor.Name = "NUD_torqueFactor";
            this.NUD_torqueFactor.Size = new System.Drawing.Size(56, 20);
            this.NUD_torqueFactor.TabIndex = 7;
            // 
            // NUD_speedFactor
            // 
            this.NUD_speedFactor.Location = new System.Drawing.Point(144, 32);
            this.NUD_speedFactor.Name = "NUD_speedFactor";
            this.NUD_speedFactor.Size = new System.Drawing.Size(56, 20);
            this.NUD_speedFactor.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 22);
            this.label3.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 22);
            this.label2.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 22);
            this.label1.TabIndex = 3;
            // 
            // SELF_CHARACTERISATION_WINDOW
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(588, 442);
            this.Controls.Add(this.MainPanel);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.Menu = this.mainMenu1;
            this.Name = "SELF_CHARACTERISATION_WINDOW";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "DriveWizard: Motor self characterization";
            this.Load += new System.EventHandler(this.SELF_CHARACTERISATION_WINDOW_Load);
            this.Closed += new System.EventHandler(this.SELF_CHARACTERISATION_WINDOW_Closed);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.SELF_CHARACTERISATION_WINDOW_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.timer1)).EndInit();
            this.MainPanel.ResumeLayout(false);
            this.Pnl_Buttons.ResumeLayout(false);
            this.PnlUsrLabels.ResumeLayout(false);
            this.PnlUsrLabels.PerformLayout();
            this.PnlTestFiles.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.general.ResumeLayout(false);
            this.general.PerformLayout();
            this.openloop.ResumeLayout(false);
            this.openloop.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.closedloop.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.noload.ResumeLayout(false);
            this.noload.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.powerframe.ResumeLayout(false);
            this.powerframe.PerformLayout();
            this.scwizprofiles.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.PnlLineContactors.ResumeLayout(false);
            this.PnlLineContactors.PerformLayout();
            this.Pnl_ConfirmNamePlateData.ResumeLayout(false);
            this.Pnl_ConfirmNamePlateData.PerformLayout();
            this.PnlDataGrids.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.PnlMotorLimits.ResumeLayout(false);
            this.PnlMotorLimits.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_testIterations)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_OL_MaxI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_PowerFactor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_torqueFactor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_speedFactor)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: SELF_CHARACTERISATION_WINDOW
		///		 *  Description     : Form constructor takes local coies of passed parameters
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		///			 </summary>
		public SELF_CHARACTERISATION_WINDOW(SystemInfo systemInfo,int passed_nodeIndex, int passed_motorIndex)
		{
			InitializeComponent();
			sysInfo = systemInfo;
			this.nodeIndex = passed_nodeIndex;
			motorIndex = passed_motorIndex;
			#region apply Sevcon Corporate Style to this Window
			//we need to recursivley hunt through all the controls and their children
			//To dig out all the control types that we are interested in
			foreach(Control c in this.Controls)
			{
				this.formatControls(c);
			}
			#endregion apply Sevcon Corporate Style to this Window
			#region setup Nodes comboBox data source Display string and underlying value members
			nodesAL = new ArrayList();
			for(int i = 0;i<this.sysInfo.nodes.Length;i++)
			{
				string temp = "Node " + this.sysInfo.nodes[i].nodeID.ToString();
				comboSource source = new comboSource(temp, (uint) this.sysInfo.nodes[i].nodeID);
				nodesAL.Add(source);
			}
			this.CB_LCNodeID.DataSource = nodesAL;
			this.CB_LCNodeID.DisplayMember = "enumStr";  //"enumStr" from comboSource CLass
			this.CB_LCNodeID.ValueMember = "enumValue";  //"enumValue" from comboSource CLass
			//this.CB_LCNodeID.SelectedIndex = this.nodeIndex; //this is OK here becaue our combo contains all connected nodes - regrdless of Manf
			// At some point we need to use DW to try and detect the node contianing Line COntactor
			// anfd use this best guess as the initial combobox index
			#endregion setup Nodes comboBox data source Display string and underlying value members
			createInitialTable(); //create/clear the prgramming data table
#if EXTERNAL_SELF_CHAR
			this.appProductCode = this.sysInfo.nodes[nodeIndex].productCode;
			this.appRevisionNumber = this.sysInfo.nodes[nodeIndex].revisionNumber;
			this.appEDSfileName  = this.sysInfo.nodes[nodeIndex].EDSorDCFfilepath;
			//this tab pages are for reading/stroing profiles in XML format
			this.tabControl1.TabPages.Remove(general);
			this.tabControl1.TabPages.Remove(openloop);
			this.tabControl1.TabPages.Remove(closedloop);
			this.tabControl1.TabPages.Remove(noload);
			this.tabControl1.TabPages.Remove(powerframe);
			this.mainMenu1.MenuItems[0].Visible = false;//hide the XML open/save options
#else
			//hide the SCWiz file selection tab - we have to remove becuase Hide() doesn't work
			this.tabControl1.TabPages.Remove(scwizprofiles);
			this.tabControl1.SelectedTab = general;
			mySelfChar = new selfCharClass(ref this.sysInfo, this.nodeIndex);
			bindProfileControls();
			setupContextMenus();
#endif
			if(this.sysInfo.nodes[this.nodeIndex].productCode != SCCorpStyle.selfchar_variant_old)
			{
				this.gotoENTER_PLATE_DATATestState();
			}
			else
			{
				this.gotoREENTER_PLATE_DATATestState();  //judetemp - for faster debugging
			}
			getMotorSettingsFromVehicleProfile();
		}

		public void formatControls(System.Windows.Forms.Control topControl )
		{
			#region format individual controls
			topControl.ForeColor = SCCorpStyle.SCForeColour;
			topControl.Font = new System.Drawing.Font("Arial", 10F);
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
			}
			else if ( topControl.GetType().Equals( typeof( HScrollBar ) ) ) 
			{
				topControl.Height = 0;
			}
			else if ( topControl.GetType().Equals( typeof( GroupBox ) ) ) 
			{
				topControl.Font = new System.Drawing.Font("Arial", 10F, FontStyle.Bold);
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
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: SELF_CHARACTERISATION_WINDOW_Load
		///		 *  Description     : Event HAndler for form load event
		///		 *  Parameters      : System event arguments 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void bindProfileControls()
		{
#if EXTERNAL_SELF_CHAR
#else
			#region general Tab
			#region max SpeedRatio
			this.TB_GEN_maxSpeedRatio.DataBindings.Clear();
			Binding bind = new Binding("Text",this.mySelfChar.scprofile.general,"maxSpeedRatio");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_GEN_maxSpeedRatio.DataBindings.Add(bind);
			#endregion max SpeedRatio
			#region max torque ratio
			this.TB_GEN_maxTorqueRatio.DataBindings.Clear();
			bind = new Binding("Text",this.mySelfChar.scprofile.general,"maxTorqueRatio");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_GEN_maxTorqueRatio.DataBindings.Add(bind);
			#endregion max torque ratio
			#region max power ratio
			this.TB_GEN_maxPowerRatio.DataBindings.Clear();
			bind = new Binding("Text",this.mySelfChar.scprofile.general,"maxPowerRatio");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_GEN_maxPowerRatio.DataBindings.Add(bind);
			#endregion max power ratio
			#endregion general Tab

			#region Open Loop Tab
			#region max percentage overcurrent
			this.TB_OL_maxPercentCurr.DataBindings.Clear();
			bind = new Binding("Text", this.mySelfChar.scprofile.openloop, "percentOverCurrent");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_OL_maxPercentCurr.DataBindings.Add(bind);
			#endregion max percentage overcurrent
			#region numberof test iterations
			this.TB_OL_numIters.DataBindings.Clear();
			bind = new Binding("Text", this.mySelfChar.scprofile.openloop, "numTestIterations");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_OL_numIters.DataBindings.Add(bind);
			#endregion numberof test iterations
			#region numtimes to apply test points
			this.TB_OL_numTestPointApps.DataBindings.Clear();
			bind = new Binding("Text", this.mySelfChar.scprofile.openloop, "numTestPointApplications");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_OL_numTestPointApps.DataBindings.Add(bind);
			#endregion numtimes to apply test points
			this.updateOLTestPoints();
			#endregion Open Loop Tab

			#region closed loop
			updateCLTestPoints();
			#endregion closed loop

			#region No Load Test
			#region number of smaples
			this.TB_NLT_numSamples.DataBindings.Clear();
			bind = new Binding("Text", this.mySelfChar.scprofile.noloadtest, "numSamples");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_NLT_numSamples.DataBindings.Add(bind);
			#endregion number of samples
			#region number of samples for flux change settle
			this.TB_NLT_numSmaplesFlux.DataBindings.Clear();
bind = new Binding("Text", this.mySelfChar.scprofile.noloadtest, "numSamplesForFluxChangeSettle");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_NLT_numSmaplesFlux.DataBindings.Add(bind);
			#endregion number of samples for flux change settle
			#region number of samples for steady state settle
			this.TB_NLT_numSamplesSteadyState.DataBindings.Clear();
bind = new Binding("Text", this.mySelfChar.scprofile.noloadtest, "numSamplesForSteadyStateSettle");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_NLT_numSamplesSteadyState.DataBindings.Add(bind);
			#endregion number of smaples for steady state settle
			#region max Id to Iq ratio
			this.TB_NLT_maxIdToIqRatio.DataBindings.Clear();
			bind = new Binding("Text", this.mySelfChar.scprofile.noloadtest, "maxIdIqRatio");
			bind.Format +=new ConvertEventHandler(this.floatBindingFormat);
			this.TB_NLT_maxIdToIqRatio.DataBindings.Add(bind);
#endregion max Id to Iq ratio
			#region w rate
			this.TB_NLT_W_rate.DataBindings.Clear();
			bind = new Binding("Text", this.mySelfChar.scprofile.noloadtest, "w_rate");
			bind.Format +=new ConvertEventHandler(floatBindingFormat);
			this.TB_NLT_W_rate.DataBindings.Add(bind);
			#endregion w rate
			updateNLTTestPoints();
			#endregion No Load test
			#region Power Frame
			this.TB_PF_maxBattVolts.DataBindings.Clear();
			bind = new Binding("Text", this.mySelfChar.scprofile.powerframe, "maxBattVolts");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_PF_maxBattVolts.DataBindings.Add(bind);
			this.TB_PF_minBattVolts.DataBindings.Clear();
			bind = new Binding("Text", this.mySelfChar.scprofile.powerframe, "minBattVolts");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_PF_minBattVolts.DataBindings.Add(bind);
			this.TB_PF_nomBattVolts.DataBindings.Clear();
			bind = new Binding("Text", this.mySelfChar.scprofile.powerframe, "nomBattVolts");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_PF_nomBattVolts.DataBindings.Add(bind);
			this.TB_PF_maxIs.DataBindings.Clear();
			bind = new Binding("Text", this.mySelfChar.scprofile.powerframe, "maxIs");
			bind.Format +=new ConvertEventHandler(ushortBindingFormat);
			this.TB_PF_maxIs.DataBindings.Add(bind);
			#endregion Power Frame
#endif
		}
		private void SELF_CHARACTERISATION_WINDOW_Load(object sender, System.EventArgs e)
		{
			dataGrid1DefaultHeight = this.dataGrid1.Height;
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: createProgrammingTS
		///		 *  Description     : Creates Table Style for the Porgarmming table display
		///								this table style is then applied on window resize 
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		
		private void createProgrammingTS()
		{
			int [] colWidths  = new int[Programmingpercents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, Programmingpercents, 0, dataGrid1DefaultHeight);
			tablestyle = new ProgTableStyle(colWidths);
			this.dataGrid1.TableStyles.Add(tablestyle);
		}

		private void getMotorSettingsFromVehicleProfile()
		{
			if(	MAIN_WINDOW.currentProfile == null)
			{//should never happen - but if it does then this will recover the situation
				MAIN_WINDOW.currentProfile = new VehicleProfile();
			}
			Binding oBinding;
			currentMotor = (VPMotor) MAIN_WINDOW.currentProfile.connectedMotors[motorIndex];
			#region motor plate data
			oBinding = new Binding("Text", this.currentMotor.platedata, "encoderPulsesPerRev");
			//The event handler is added
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_DW_EncoderPulsesPerRev.DataBindings.Add(oBinding);

			oBinding = new Binding("Text", this.currentMotor.platedata, "ratedLineVoltageRMS");
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_DW_RatedLineVoltage_rms.DataBindings.Add(oBinding);

			oBinding = new Binding("Text", this.currentMotor.platedata, "ratedSpeedMechanicalRPM");
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_DW_RatedSpeed_Mech.DataBindings.Add(oBinding);

			oBinding = new Binding("Text", this.currentMotor.platedata, "ratedPowerkW");
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_DW_RatedPower_Watts.DataBindings.Add(oBinding);

			oBinding = new Binding("Text", this.currentMotor.platedata, "numOfPolePairs");
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_DW_NoOfPolePairs.DataBindings.Add(oBinding);

			oBinding = new Binding("Text", this.currentMotor.platedata, "ratedPhaseCurrentrmsA");
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_DW_RatedPhaseCurr_rms.DataBindings.Add(oBinding);

			oBinding = new Binding("Text", this.currentMotor.platedata, "ratedFrequencyElectricalHz");
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_DW_RatedFreq_Elec.DataBindings.Add(oBinding);

			oBinding = new Binding("Text", this.currentMotor.platedata, "ratedPowerFactor");
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.floatBindingFormat);
			this.TB_DW_RatedPowerFactor.DataBindings.Add(oBinding);
			#endregion motor plate data

			#region motor limits
			oBinding = new Binding("Text", this.currentMotor.motorLimits, "maxSpeedrpm");
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_MaxSpeed.DataBindings.Add(oBinding);

			oBinding = new Binding("Text", this.currentMotor.motorLimits, "maxTorqueNm");
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_MaxTorque.DataBindings.Add(oBinding);
			#endregion motor limits

			if(MAIN_WINDOW.currentProfile.lineContactors.Count>0)
			{
				this.addLCControlsDatabinding();
				for(int i=0;i<MAIN_WINDOW.currentProfile.lineContactors.Count;i++)
				{
					this.CB_LineContactorNo.Items.Add((object) (i+1));
				}
				this.CB_LineContactorNo.SelectedIndex = 0;
				this.CB_LCNodeID.SelectedIndex = -1;
				VPLineContactor myLC = (VPLineContactor)MAIN_WINDOW.currentProfile.lineContactors[0];
				for(int i = 0;i<nodesAL.Count;i++)
				{
					comboSource source = (comboSource) nodesAL[i];
					if(myLC.nodeID == source.enumStr)
					{
						this.CB_LCNodeID.SelectedIndex = i;
					}
				}

			}
			else
			{
				this.CB_LineContactorNo.Enabled = false;
				this.TB_DW_LC_PullInVolts.Enabled = false;
				this.TB_DW_LC_PullInms.Enabled = false;
				this.TB_DW_LC_holdVolts.Enabled = false;
				this.CB_LineContactorNo.Enabled = false;
				this.CB_LCNodeID.Enabled =false;
				this.Btn_deleteLC.Enabled = false;
			}
			#region profile filepaths
			//default file
			oBinding = new Binding("Text", currentMotor, "testProfileFilepath");
			//The event handler is added
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.stringBindingFormat);
			this.LblTestProfile.DataBindings.Add(oBinding);

			oBinding = new Binding("Text", currentMotor, "powerframeProfileFilepath");
			//The event handler is added
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.stringBindingFormat);
			this.LblPowerFrameProfile.DataBindings.Add(oBinding);
			#endregion profile filepaths
		}

		private void ushortBindingFormat(object sender, System.Windows.Forms.ConvertEventArgs e) 
		{
			//the following automtically rejects anything that can't be converted to a ushort
			// no need for try/catch etc
			e.Value = System.UInt16.Parse(e.Value.ToString());
		}

		private void floatBindingFormat(object sender, System.Windows.Forms.ConvertEventArgs e) 
		{
			Binding _myBind = (Binding)sender;
			if(_myBind.Control.Name == this.TB_DW_RatedPowerFactor.Name)
			{
				e.Value = Math.Min(1,System.Single.Parse(e.Value.ToString()));
			}
			else
			{
				//the following automtically rejects anything that can't be converted to a ushort
				// no need for try/catch etc
				e.Value = System.Single.Parse(e.Value.ToString());
			}
		}
		private void stringBindingFormat(object sender, System.Windows.Forms.ConvertEventArgs e) 
		{
#if EXTERNAL_SELF_CHAR
			Binding _bind = (Binding) sender;
			if( _bind.Control.Name == this.LblTestProfile.Name)
			{
				#region Test profile file
				if(System.IO.File.Exists(e.Value.ToString()) == true)
				{
					e.Value = e.Value.ToString();

					//now replace spaces with <SPACE> - SCWiz input parameters use space character as delimiter
					//so we cannot have any spaces in the filepath passed across
					string FilePath = this.currentMotor.testProfileFilepath;
					FilePath = FilePath.Replace(" " , @"<SPACE>");
					SCWiz_argv[(int) SCWizargs.DW_TestProfileFilepath] = FilePath;

				}
				else
				{
					e.Value = "Default File not found";

					SCWiz_argv[(int) SCWizargs.DW_TestProfileFilepath] = "fileNotFound";

				}
				#endregion Test profile file
			}
			else if(_bind.Control.Name == this.LblPowerFrameProfile.Name)
			{
				#region Power frame profile file
				if(System.IO.File.Exists(e.Value.ToString()) == true)
				{
					//need to repace spaces with known char sequences
					string FilePath = MAIN_WINDOW.UserDirectoryPath + @"\SelfChar\test.wzp";
					FilePath = FilePath.Replace(" " , @"<SPACE>");
					SCWiz_argv[(int) SCWizargs.DW_PowerFrameProfilefilePath] = FilePath;//
				}
				else
				{
					e.Value = "Default File not found";
					SCWiz_argv[(int) SCWizargs.DW_PowerFrameProfilefilePath] = "fileNotFound";
				}
				#endregion Power frame profile file
			}
			else if(_bind.Control.Name == this.CB_LCNodeID.Name)
			{
				for(int i = 0;i<this.CB_LCNodeID.Items.Count;i++)
				{
					if(e.Value.ToString() == this.CB_LCNodeID.Items[i].ToString())
					{
						return;
					}
				}
				e.Value = this.CB_LCNodeID.Items[0].ToString();
			}
#endif
		}

		private void addLCControlsDatabinding()
		{
			currManager = (CurrencyManager)this.BindingContext[MAIN_WINDOW.currentProfile.lineContactors];
			currManager.Position = 0; // Set the initial Position of the control.
			#region vehicle data - line contactors
			Binding oBinding;
			oBinding = new Binding("Text", MAIN_WINDOW.currentProfile.lineContactors, "pullInVolts");
			//The event handler is added
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_DW_LC_PullInVolts.DataBindings.Add(oBinding);

			oBinding = new Binding("Text", MAIN_WINDOW.currentProfile.lineContactors, "pullInms");
			//The event handler is added
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_DW_LC_PullInms.DataBindings.Add(oBinding);

			oBinding = new Binding("Text", MAIN_WINDOW.currentProfile.lineContactors, "holdVolts");
			//The event handler is added
			oBinding.Format += new System.Windows.Forms.ConvertEventHandler(this.ushortBindingFormat);
			this.TB_DW_LC_holdVolts.DataBindings.Add(oBinding);

			oBinding = new Binding("SelectedItem", MAIN_WINDOW.currentProfile.lineContactors, "nodeID");
			this.CB_LCNodeID.DataBindings.Add(oBinding);
			#endregion vehicle data - line contactors
		}
		#endregion
		
		#region show/hide User controls
		private void hideUserControls()
		{
			this.CB_LineContactorNo.Enabled = false;
		}
		private void showUserControls()
		{
			this.CB_LineContactorNo.Enabled = true;
		}
		#endregion show/hide User controls

		#region user interaction zone
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: Btn_Next_Click
		///		 *  Description     : Event Handler for the Continue Button 
		///		 *  Parameters      : System event arguments
		///		 *  Used Variables  : none - button is hidden unless user needs acess to it
		///								Actions depend on testState and , when programming, 
		///								on  Prog_Step
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void Btn_Next_Click(object sender, System.EventArgs e)
		{
			switch ( testState )
			{
				case TestState.ENTER_PLATE_DATA:
					if(plateDataOK() == true)
					{
						this.gotoENTER_MOTOR_LIMITSTestState();
					}
					break;

				case TestState.REENTER_PLATE_DATA:
					if(plateDataOK() == true)
					{
						this.gotoREENTER_MOTOR_LIMITSTestState();
					}
					break;

				case TestState.ENTER_MOTOR_LIMITS:
					if(motorLimitsOK() == true)
					{
						this.gotoENTER_LINE_CONTACTORTestState();
					}
					break;

				case TestState.REENTER_MOTOR_LIMITS:
					if(motorLimitsOK() == true)
					{
						this.gotoREENTER_LINE_CONTACTORTestState();
					}
					break;

				case TestState.ENTER_LINE_CONTACTOR:
					if(lineContactorDataOK() == true)
					{
						this.gotoENTER_TESTIFILESTestState();

					}
					break;

				case TestState.REENTER_LINE_CONTACTOR:
					if(lineContactorDataOK() == true)
					{
						this.gotoREENTER_TESTIFILESTestState();
					}
					break;

				case TestState.ENTER_TESTIFILES:
				case TestState.REENTER_TESTIFILES:
					if(testFilesOK() == true)
					{
#if EXTERNAL_SELF_CHAR
						#region Store copy of user data in SCWiz_argv 

						//judetemp = note these text boxes will require error handling at some point - but not yet
						//CANopen data
                        //Jude VCI3 file merge - single baud parameter
						#region calculate numerical baud rate value
						switch(this.sysInfo.CANcomms.systemBaud)
						{
							case BaudRate._1M:
								SCWiz_argv[(int)SCWizargs.DW_baud] = "1000000";
								break;

							case BaudRate._800K:
								SCWiz_argv[(int)SCWizargs.DW_baud] = "800000";
								break;

							case BaudRate._500K:
								SCWiz_argv[(int)SCWizargs.DW_baud] = "500000";
								break;

							case BaudRate._250K:
								SCWiz_argv[(int)SCWizargs.DW_baud] = "250000";
								break;

							case BaudRate._125K:
								SCWiz_argv[(int)SCWizargs.DW_baud] = "125000";
								break;

							case BaudRate._100K:
								SCWiz_argv[(int)SCWizargs.DW_baud] = "100000";
								break;

							case BaudRate._50K:
								SCWiz_argv[(int)SCWizargs.DW_baud] = "50000";
								break;

							case BaudRate._20K:
								SCWiz_argv[(int)SCWizargs.DW_baud] = "20000";
								break;

							case BaudRate._10K:
								SCWiz_argv[(int)SCWizargs.DW_baud] = "10000";
								break;

							default:
                                //Jude VCI3 file merge - single baud param
								Message.Show("Unhandled baud rate: " +  this.sysInfo.CANcomms.systemBaud.ToString() + ". This window will Close.");
								this.Close();
								return;
						}
						#endregion caluculate numericla baud rate value

						SCWiz_argv[(int)SCWizargs.DW_NodeID_SCnode] = this.sysInfo.nodes[this.nodeIndex].nodeID.ToString();
						SCWiz_argv[(int)SCWizargs.DW_NodeID_LineContactor] = this.CB_LCNodeID.SelectedValue.ToString(); 

						//Vehicle Data
						SCWiz_argv[(int) SCWizargs.DW_LC_PullInVolts] = this.TB_DW_LC_PullInVolts.Text;
						SCWiz_argv[(int) SCWizargs.DW_LC_PullInms] = this.TB_DW_LC_PullInms.Text;
						SCWiz_argv[(int) SCWizargs.DW_LC_holdVolts] = this.TB_DW_LC_holdVolts.Text;
						SCWiz_argv[(int) SCWizargs.DW_LC_Output] = "0";//this.TB_DW_LC_Output.Text;

						//motor name plate data
						SCWiz_argv[(int) SCWizargs.DW_EncoderPulsesPerRev] = this.TB_DW_EncoderPulsesPerRev.Text;
						SCWiz_argv[(int) SCWizargs.DW_EncoderPolarity] = this.CB_DW_EncoderPolarity.Checked.ToString().ToLower();
						SCWiz_argv[(int) SCWizargs.DW_NoOfPolePairs] = this.TB_DW_NoOfPolePairs.Text;
						SCWiz_argv[(int) SCWizargs.DW_RatedLineVoltage_rms] = this.TB_DW_RatedLineVoltage_rms.Text;
						SCWiz_argv[(int) SCWizargs.DW_RatedSpeed_Mech] = this.TB_DW_RatedSpeed_Mech.Text;
						SCWiz_argv[(int) SCWizargs.DW_RatedPower_Watts] = this.TB_DW_RatedPower_Watts.Text;
						SCWiz_argv[(int) SCWizargs.DW_RatedPhaseCurr_rms] = this.TB_DW_RatedPhaseCurr_rms.Text;
						SCWiz_argv[(int) SCWizargs.DW_RatedFreq_Elec] = this.TB_DW_RatedFreq_Elec.Text;
						if(CB_PowerFactorUnknown.Checked == true)
						{
							SCWiz_argv[(int) SCWizargs.DW_RatedPowerFactor] = "-1";  //used as unknown value in SCWiz
						}
						else
						{
							SCWiz_argv[(int) SCWizargs.DW_RatedPowerFactor] = this.TB_DW_RatedPowerFactor.Text;
						}
						SCWiz_argv[(int) SCWizargs.DW_DebugLevel] = "2";
						SCWiz_argv[(int) SCWizargs.DW_showDebug] = "true";//this.CB_DisplayDebuggingInSCWiz.Checked.ToString();
						SCWiz_argv[(int) SCWizargs.DW_UseMatLab] = "true";//this.CB_useMatLab.Checked.ToString();
						#endregion Store copy of user data in SCWiz_argv 
#endif
						#region user entering data state
						if ( MessageBox.Show(this, 
							"Ensure that the motor and battery are connected correctly to the node. \nKeep clear of all electrical terminals during the test. \n\nIs it OK for the test to start?" ,
							"WARNING: Tests are about to start.",
							MessageBoxButtons.OKCancel,
							MessageBoxIcon.Question,
							MessageBoxDefaultButton.Button2 ) == DialogResult.OK )
						{
							this.Btn_Back.Visible = false;
							#region store user data to vehicle profile file for use as defaults
							ObjectXMLSerializer vpXMLSerializer = new ObjectXMLSerializer();
							vpXMLSerializer.Save(MAIN_WINDOW.currentProfile, MAIN_WINDOW.currentProfile.ProfilePath);
							#endregion store user data to vehicle profile file  for use as defaults

							if(testState == TestState.ENTER_TESTIFILES)
							{

								#region set up for reprogramming CAN node with Self Char software
								this.createProgrammingTS();
								this.Lbl_UsrProgrammingFeedback.Text = "Stage 1: Programming Sevcon controller with Self Characterization software";
								if(MAIN_WINDOW.isVirtualNodes == true)
								{
									#region virtual System
									this.hideUserControls();
									DialogResult res 
										= Message.Show(this,"End of current Virtual Nodes simulation for Self Characterization. \nClick OK to close window",
										"Virtual Nodes Limit", 
										MessageBoxButtons.OK,
										MessageBoxIcon.Information,
										MessageBoxDefaultButton.Button1);
									if(res ==DialogResult.OK)
									{
										this.Close();
									}
									#endregion
								}
								else
								{
									this.Btn_Back.Visible = false;
									gotoPROGRAMMING_SC_CODETestState();
									startProgrammingprocess();
								}
								#endregion set up for reprogramming CAN node with application software
							}
							else
							{
								#region set up for re-running the SC tests
								//user wants to re-run SC tests
								testState = TestState.OPEN_LOOP_TEST1;
#if EXTERNAL_SELF_CHAR
								launchExtProcess();
#else
								passPreTestDataToController();
								this.mySelfChar.openLoopTest();
								testState = TestState.CLOSED_LOOP_TEST3;

#endif
								#endregion set up for re-running the SC tests
							}
						}
						else
						{
							//user selected cancel
							if(this.testState == TestState.ENTER_TESTIFILES)
							{
								this.gotoENTER_PLATE_DATATestState();
							}
							else
							{
								this.gotoREENTER_PLATE_DATATestState();
							}
						}
						#endregion user entering data state
					}
					break;

				case TestState.PROGRAMMING_SC_CODE:
				case TestState.PROGRAMMING_ORIGINAL_CODE:
					#region prgramming steps
					if(Prog_Step == progStep.s02_AppVersAvailable)
					{
						#region user has opted to enter boot mode
						feedback = sysInfo.nodes[nodeIndex].readDeviceIdentity( );
						if(feedback != DIFeedbackCode.DISuccess)  
						{
							SystemInfo.errorSB.Insert(0, "\nFailed to read node identity parameters");
							this.programmer.fatalErrorOccured = true;  //to force exit from programming
							this.processErrorString();
							return;
						}
						if(sysInfo.nodes[nodeIndex].productVariant != SCCorpStyle.bootloader_variant )
						{
							Prog_Step = progStep.s04_JumpingToBoot;
						}
						else 
						{
							Prog_Step = progStep.s03_AppVersionsConfirmed;  //we are alreading in bootloader so move on
						}
						this.Btn_Next.Visible = false;
						this.progressBar1.Visible = true;
						this.progTimer.Enabled = true;
						#endregion user has opted to enter boot mode
					}
					else if (Prog_Step == progStep.s07_BootVersAvailable) 
					{
						#region user has opted to select a download file
						//if we get this far then we know that we are in boot so set nodeState
						//to UNKONW to allow us to see the application boot up message - DI
						//only flags changfes in NodeState hence we set to UNKNOWN now ready for
						//programming end
						this.sysInfo.nodes[nodeIndex].nodeState = NodeState.Unknown;

						Prog_Step = progStep.s08_SelectingDldFile;
						#region selecting download file
						this.statusBar1.Text = "Displaying download files";
						this.DldFilename = "";
						openFileDialog1.FileName = "";  //force restore directory to false???
						if(Directory.Exists(MAIN_WINDOW.UserDirectoryPath + @"\DLD") == false)
						{
							try
							{
								Directory.CreateDirectory(MAIN_WINDOW.UserDirectoryPath + @"\DLD");
							}
							catch
							{
								Message.Show("Unable to create directory");
								return;
							}
						}
						openFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\DLD";
						if(testState ==TestState.PROGRAMMING_SC_CODE)
						{
							openFileDialog1.Title = "Open Self Characterization download file";
						}
						else
						{
							//ensure that we are back in correct directory
							Directory.SetCurrentDirectory(MAIN_WINDOW.UserDirectoryPath);
							openFileDialog1.Title = "Open Controller Application download file";
						}

						openFileDialog1.Filter = "download files (*.dld)|*.dld" ;
						this.Btn_Next.Visible = false;
						#endregion
						openFileDialog1.ShowDialog(this);
						if(openFileDialog1.FileName != "") //user has selected valid filename
						{
							//next step is automatic so Timer on and user button off
							this.Btn_ChangeFile.Visible = false;
							this.progressBar1.Visible = true;
							this.progTimer.Enabled = true;
							this.Btn_Next.Visible = true;
							DldFilename = openFileDialog1.FileName;
							Prog_Step = progStep.s09_ParsingDldFile;
						}
						else
						{
							if(testState ==TestState.PROGRAMMING_SC_CODE)
							{
								this.Lbl_UserInstructions.Text = "Select Self Characterization download file";
							}
							else
							{
								this.Lbl_UserInstructions.Text = "Select Controller Application download file";
							}
							this.Btn_ChangeFile.Text = "Select file";
							this.Btn_ChangeFile.Visible = true;
							this.statusBar1.Text = "";
						}
						#endregion user has opted to select a download file
					}
					else if(Prog_Step == progStep.s09_ParsingDldFile)
					{
						#region user has opted to accept this download file
						Prog_Step = progStep.s11_codeDownload;
						this.progressBar1.Visible = true;
						this.progTimer.Enabled = true;
						#endregion user has opted to accept this download file
					}
					else if (Prog_Step == progStep.s10_updateDownloadfileCheck)
					{
						#region user has opted to download code from selected downlaod file
						this.Btn_ChangeFile.Visible = false;
						this.Btn_Next.Visible = false;
						Prog_Step = progStep.s11_codeDownload; //judetemp
						this.progressBar1.Visible = true;
						this.progTimer.Enabled = true;
						#endregion user has opted to download code from selected downlaod file
					}
					#endregion prgramming steps
					break;

				case TestState.OPEN_LOOP_TEST1:
#if EXTERNAL_SELF_CHAR
					#region launching scWiz
					launchExtProcess();
					#endregion launching scWiz
#else
					passPreTestDataToController();
					mySelfChar.openLoopTest();
					testState = TestState.NO_LOAD_TEST2;
					this.Btn_Next.Text = "&Next";
					this.Btn_Next.Visible = true;
#endif
					break;

				case TestState.NO_LOAD_TEST2:
#if EXTERNAL_SELF_CHAR
#else
					mySelfChar.noLoadTest();
#endif
					testState = TestState.CLOSED_LOOP_TEST3;
					break;

				case TestState.CLOSED_LOOP_TEST3:
#if EXTERNAL_SELF_CHAR
#else
					mySelfChar.closedLoopTest();
#endif
					testState = TestState.SELF_CHAR_TESTS_COMPLETE;
					this.restartTestButton.Visible = true;
					break;

				case TestState.SELF_CHAR_TESTS_COMPLETE:
					#region putting original softwar eback in
					this.table.Clear();  
					for (int i = 0; i<NodeParams.Length; i++) 	
					{
						DataRow row = this.table.NewRow();
						row[(int)(progCols.Parameter)] = NodeParams[i];
						row[(int)(progCols.Node)] = "---";
						row[(int)(progCols.dldFile)] = "---";
						this.table.Rows.Add(row);
					}
					this.gotoPROGRAMMING_ORIGINAL_CODETestState();
#if EXTERNAL_SELF_CHAR
					//force the 0x1018 pararmes to reflect a normal app instead of self char
					this.DCFFileCreatedOK = overwriteDeviceInfoInOuputDCFFile();
#endif
					startProgrammingprocess();
					#endregion putting original software back in
					break;

				case TestState.WRITE_RESULTS_TO_NODE:
					#region USer has opted to write results to node
					this.Btn_Next.Visible = false;
					this.restartTestButton.Visible = false;
#if EXTERNAL_SELF_CHAR
					Directory.SetCurrentDirectory(MAIN_WINDOW.UserDirectoryPath);
					this.statusBar1.Text = "Writing new values to, Node  " + this.sysInfo.nodes[this.nodeIndex].nodeID.ToString();
					feedback = this.sysInfo.readODFromDCF(MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\DATA\results.dcf");
					if(feedback != DIFeedbackCode.DISuccess)
					{
						Message.Show("Error reading DCF file. \n Errocode: " + feedback.ToString());
					}
					else
					{
						this.progressBar1.Value = this.progressBar1.Minimum;
						this.progressBar1.Maximum = this.sysInfo.DCFnode.objectDictionary.Count;
						this.progressBar1.Visible = true;
					#region data retrieval thread start
						this.sysInfo.itemBeingWritten = 0;  //force back to zero before we start judetemp
						writeDCFToDeviceThread = new Thread(new ThreadStart( writeDCFToDevice )); 
						writeDCFToDeviceThread.Name = "PersonalityParametersTable";
						writeDCFToDeviceThread.IsBackground = true;
						writeDCFToDeviceThread.Priority = ThreadPriority.Normal;
#if DEBUG
						System.Console.Out.WriteLine("Thread: " + writeDCFToDeviceThread.Name + " started");
#endif
						timer1.Enabled = true;
						writeDCFToDeviceThread.Start(); 
					#endregion
					}
#else
					Message.Show("Write results for Internal self char to node here");
#endif
					#endregion USer has opted to write results to node
					break;

				default:
					break;
			}
		}

		private void BtnBack_Click(object sender, System.EventArgs e)
		{
			switch ( testState )
			{
				case TestState.ENTER_MOTOR_LIMITS:
					this.gotoENTER_PLATE_DATATestState();
					break;

				case TestState.REENTER_MOTOR_LIMITS:
					this.gotoREENTER_PLATE_DATATestState();
					break;

				case TestState.ENTER_LINE_CONTACTOR:
					this.gotoENTER_MOTOR_LIMITSTestState();
					break;

				case TestState.REENTER_LINE_CONTACTOR:
					this.gotoREENTER_MOTOR_LIMITSTestState();
					break;

				case TestState.ENTER_TESTIFILES:
					this.gotoENTER_LINE_CONTACTORTestState();
					break;

				case TestState.REENTER_TESTIFILES:
					this.gotoREENTER_LINE_CONTACTORTestState();
					break;

			}		
		}

		private void launchExtProcess()
		{
#if EXTERNAL_SELF_CHAR
			#region real devices
			this.statusBar1.Text = "Starting external process";
			this.Lbl_UserInstructions.Text = "Please wait";
			if(ExtSCProcess == null)
			{
                this.sysInfo.CANcomms.VCI.closeCANAdapterHW();
				try
				{
					//change to the self char directory for SCWiz
					Directory.SetCurrentDirectory(MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR");
					if(Directory.Exists(MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\data") == false)
					{  //giv eSCWiz a fighting chance by ensuraing that the dat adirectory exists
						Directory.CreateDirectory(MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\data");
					}
					StringBuilder SCWizargsSB = new StringBuilder();
					for(int i = 0;i<this.SCWiz_argv.Length;i++)
					{
						SCWizargsSB.Append(SCWiz_argv[i]);
						SCWizargsSB.Append(" ");  //divide with spaces
					}
					Process.Start(@"scwizNoGUI.exe", SCWizargsSB.ToString());  //this one should be relative becuase we changed the directory path
					ExtSCProcess = Process.GetProcessesByName("scwizNoGUI");
					for(int i = 0;i<ExtSCProcess.Length;i++)
					{
						ExtSCProcess[i].EnableRaisingEvents = true;
						ExtSCProcess[i].Exited +=new EventHandler(externalSCExited);
					}
					this.statusBar1.Text = "External process running";
					this.Lbl_UsrProgrammingFeedback.Text = "Stages 2 to 4: Extenal Self Characterization Application running";
					this.Lbl_UserInstructions.Text = "";
					this.Btn_Next.Text = "Abort ext. process";
					this.monitoringExtProc = true;
					this.progTimer.Enabled = true;
				}
				catch(Exception myEx)
				{
					Directory.SetCurrentDirectory(MAIN_WINDOW.UserDirectoryPath);
					this.sysInfo.CANcomms.restartCAN(false);
					SystemInfo.errorSB.Append("\nUnable to start external self characterization application:\n" + myEx.Message);
					this.processErrorString();
				}
			}
			else
			{
				if ( MessageBox.Show(this, "Force external process to end?" , "WARNING: Tests may not be complete", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2 ) == DialogResult.OK )
				{
					for(int i = 0;i<ExtSCProcess.Length;i++)
					{
						ExtSCProcess[i].Kill();
						this.Btn_Next.Text = "&Next";

					}
				}
			}
			#endregion real devices
#endif
		}
		private void restartTestButton_Click(object sender, System.EventArgs e)
		{
			this.gotoREENTER_PLATE_DATATestState();
		}

		private void changeFileBtn_Click(object sender, System.EventArgs e)
		{
#if EXTERNAL_SELF_CHAR
			this.Lbl_UserInstructions.Text = "";
			for(int i=0 ; i<this.table.Rows.Count ; i++)
			{
				this.table.Rows[i][(int) (progCols.dldFile)] = "";  //reset table
			}
			Prog_Step = progStep.s08_SelectingDldFile;
			this.statusBar1.Text =  "Displaying download files";
			#region selecting download file
			//put this line before the setting InitalDirectory to maintain starting point on directory structure 
			openFileDialog1.FileName = "";
			DldFilename = "";  //reset this one too
			openFileDialog1.RestoreDirectory = true;
			openFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\DLD";
			openFileDialog1.Title = "Select another file";
			openFileDialog1.Filter = "download files (*.dld)|*.dld" ;
			openFileDialog1.ShowDialog(this);
			this.Btn_ChangeFile.Visible = true;  //incase user does not select file
			#endregion
			if(openFileDialog1.FileName != "")
			{
				this.statusBar1.Text = "Parsing selected download file";
				this.Btn_ChangeFile.Visible = false;
				this.Btn_Next.Visible = false;
				this.restartTestButton.Visible = false;
				//next step is automatic so Timer on and user button off
				this.progressBar1.Visible = true;
				this.progTimer.Enabled = true;
				DldFilename = openFileDialog1.FileName;
				Prog_Step = progStep.s09_ParsingDldFile;
			}
			else
			{
				this.Btn_Next.Visible = false;
				this.restartTestButton.Visible = false;
			}
#endif
		}

		private void progTimer_Tick(object sender, System.EventArgs e)
		{
			
#if EXTERNAL_SELF_CHAR
			if(this.monitoringExtProc == true)   //prevent progTiemr beinfg stopped until the ext porcess has stopped
			{
				if(this.extProcComplete == true)
				{
					#region handle completion of SCwiz
					this.progTimer.Enabled = false;
					this.extProcComplete = false;
					this.monitoringExtProc = false;
					this.Btn_Next.Visible = false;  //make invisible while we re-establish CAN comms via IXXAT
					Directory.SetCurrentDirectory(MAIN_WINDOW.UserDirectoryPath);
					this.Btn_Close.Enabled = false;  //reduce chances of IXXAT restart clash
					this.statusBar1.Text = "Closing external application and restarting CAN communications";
					this.sysInfo.CANcomms.restartCAN(false);
					this.Lbl_UserInstructions.Text = "Continue to restore original software in node " 
						+ this.sysInfo.nodes[this.nodeIndex].nodeID.ToString() + " or Restart to re-do tests";
					this.statusBar1.Text = "External application exited with exitcode: " + SCWizExitcode.ToString();
					testState = TestState.SELF_CHAR_TESTS_COMPLETE;
					if(this.SCWizExitcode == 0)
					{
						Lbl_UsrProgrammingFeedback.Text = "Self Characterization Completed OK";
					}
					else
					{
						Lbl_UsrProgrammingFeedback.Text = "Error reported by Self Characterization process";
					}
					this.restartTestButton.Visible = true;
					this.Btn_Next.Text = "&Next";
					this.Btn_Next.Visible = true;
					//	this.Stat1checkBox.Checked = true;
					//	this.Stat2checkBox.Checked = true;
					//	this.RotcheckBox.Checked = true;
					this.Btn_Close.Enabled = true; //reduce chances of IXXAT restart clash
					return;
					#endregion handle completion of SCWiz
				}
						}


			else
#endif
			{
				this.progTimer.Enabled = false;
				bool timerStopFlag = false;
				if (dataThread != null)
				{
					switch (Prog_Step)
					{
						case progStep.s10A_UploadEEPROM:
							#region EEPROM upload
							#region disable checkboxes and datagrid once EEPROM upload starts
							//now read the values and translate to no GUI parameters
							this.dataGrid1.Enabled = false;
							#endregion disable checkboxes once programming starts

							Prog_Step = progStep.s10B_WritingEEPROMBackupfile;
							//now start the upload thread off
							dataThread = new Thread(new ThreadStart( programmer.uploadEEPROM )); 
							dataThread.Name = "Uploading EEPROM";
							dataThread.IsBackground = true;
                            dataThread.Priority = ThreadPriority.Normal;
#if DEBUG
							System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
							dataThread.Start(); 
							#endregion EEPROM upload
							break;

						case progStep.s10B_WritingEEPROMBackupfile:
							#region writing backup file
							//first see if we have finished uploadeing EEPRom yet
							if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
							{
								//just move to next step - no EEPROM download in self char
								this.Prog_Step = progStep.s11_codeDownload;
							}
							#endregion writing backup file
							break;

						case progStep.s11_codeDownload:  //place frist to speed up programming
							#region code download
							Prog_Step = progStep.s12_ProgrammingComplete;
								dataThread = new Thread(new ThreadStart( codeTransferWrapper)); 
								dataThread.Name = "Downloading code";
								dataThread.IsBackground = true;
                                dataThread.Priority = ThreadPriority.Normal;
#if DEBUG
								System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
								dataThread.Start(); 
							#endregion code download
							break;

						case progStep.s01_GettingAppVers:
							#region getting app versions if available
							if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
							{
								timerStopFlag = true; //knock the timer off becasue user confirm is required
								this.Btn_Next.Visible = true;
								this.table.Rows[(int) (DeviceParams.HostAppVer)][(int)(progCols.Node)] = hostAppController; 
								this.table.Rows[(int) (DeviceParams.DSPAppVer)][(int) (progCols.Node)] = DSPAppController;
								Prog_Step = progStep.s02_AppVersAvailable;
							}
							#endregion getting app versions if available
							break;

						case progStep.s04_JumpingToBoot:
							#region jumping to boot
							this.Prog_Step = progStep.s05_waitingForInBootConfirm;
							#region data retrieval thread start
							dataThread = new Thread(new ThreadStart( setBootModeWrapper)); 
							dataThread.Name = "Force into Boot mode";
							dataThread.IsBackground = true;
                            dataThread.Priority = ThreadPriority.Normal;
#if DEBUG
							System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
							dataThread.Start(); 
							#endregion
							#endregion jumping to boot
							break;

						case progStep.s05_waitingForInBootConfirm:
							#region waiting for controlle rto enter bootloader
							if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )  //now in bootmode
							{
								this.Prog_Step = progStep.s03_AppVersionsConfirmed;  
							}
							#endregion waiting for controlle rto enter bootloader
							break;

						case progStep.s03_AppVersionsConfirmed:
							#region thread for retrieving Bootloader EDS informnation
							Prog_Step = progStep.s06_GettingBootVers;  //go to next step before setting the thread away
							dataThread = new Thread(new ThreadStart( getBootEDSWrapper )); 
							dataThread.Name = "Get EDS Info";
							dataThread.IsBackground = true;
                            dataThread.Priority = ThreadPriority.Normal;
#if DEBUG
							System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
							dataThread.Start(); 
							#endregion thread for retrieving Bootloader EDS informnation
							break;

						case progStep.s06_GettingBootVers:
							#region Thread for retrieving general bootloader information
							if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
							{
								Prog_Step = progStep.s07_BootVersAvailable;
								dataThread = new Thread(new ThreadStart( getBootInfoWrapper )); 
								dataThread.Name = "Get Boot Info";
								dataThread.IsBackground = true;
                                dataThread.Priority = ThreadPriority.Normal;
#if DEBUG
								System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
								dataThread.Start(); 
							}
							#endregion Thread for retrieving general bootloader information
							break;

						case progStep.s07_BootVersAvailable:
							#region displaying boot information
							if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
							{
								if(controllerBootParams != null)
								{
									this.table.Rows[(int) (DeviceParams.HostBootVer)][(int) (progCols.Node)] = controllerBootParams[0];
									this.table.Rows[(int) (DeviceParams.DSPBootVer)][(int) (progCols.Node)] = controllerBootParams[1];
									this.table.Rows[(int) (DeviceParams.HardwareVersion)][(int) (progCols.Node)] = controllerBootParams[5];
									this.table.Rows[(int) (DeviceParams.NCHardware)][(int) (progCols.Node)] = "not applicable";
									this.table.Rows[(int) (DeviceParams.DeviceMemSpaces)][(int) (progCols.Node)] = controllerBootParams[3];
									this.table.Rows[(int) (DeviceParams.DeviceProcs)][(int) (progCols.Node)] = controllerBootParams[2];
									this.table.Rows[(int) (DeviceParams.EEPROMformat)][(int) (progCols.Node)] = controllerBootParams[4];
								}
								timerStopFlag = true;
								this.Btn_Next.Visible = true;
							}
							#endregion displaying boot information
							break;

						case progStep.s09_ParsingDldFile:
							#region checking download file thread
							Prog_Step  = progStep.s10_updateDownloadfileCheck;
							dataThread = new Thread(new ThreadStart( compareDldFileToDeviceWrapper )); 
							dataThread.Name = "Checking Download file";
							dataThread.IsBackground = true;
                            dataThread.Priority = ThreadPriority.Normal;
#if DEBUG
							System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
							dataThread.Start(); 
							#endregion checking download file thread
							break;

						case progStep.s10_updateDownloadfileCheck:
							#region waiting for checking downlaod file to complete
							if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
							{
								if(downloadParams != null)
								{
									table.Rows[(int) (DldFileParams.HostAppVer)][(int)(progCols.dldFile)] = downloadParams[0];
									table.Rows[(int) (DldFileParams.DSPAppVer)][(int)(progCols.dldFile)]  = downloadParams[1];
									table.Rows[(int) (DldFileParams.CompHWBuilds)][(int)(progCols.dldFile)] = downloadParams[2];
									table.Rows[(int) (DldFileParams.NonCompHWBuilds)][(int)(progCols.dldFile)] = downloadParams[3];
									table.Rows[(int) (DldFileParams.EEPROMformat)][(int)(progCols.dldFile)]  = downloadParams[4];
									table.Rows[(int) (DldFileParams.NewCodeMemSpaces)][(int)(progCols.dldFile)]  = downloadParams[5];

									table.Rows[(int) (DldFileParams.spare1)][(int)(progCols.dldFile)] = "not applicable";
									table.Rows[(int) (DldFileParams.spare2)][(int)(progCols.dldFile)] = "not applicable";
									table.Rows[(int) (DldFileParams.spare3)][(int)(progCols.dldFile)] = "not applicable";
								}
								this.Btn_ChangeFile.Text = "Change file";
								this.Btn_ChangeFile.Visible = true;
								this.Btn_Next.Visible = true;
								timerStopFlag= true;
							}
							#endregion waiting for checking downlaod file to complete
							break;

						case progStep.s12_ProgrammingComplete:
							#region programming complete
							if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
							{
                                //jude 21/Feb/08 bug - we should only send final chacksum if code transfer went OK
                                if (programmer.fatalErrorOccured == false)
                                {

                                    dataThread = new Thread(new ThreadStart(completeDownloadWrapper));
                                    dataThread.Name = "Completing download";
                                    dataThread.IsBackground = true;
                                    dataThread.Priority = ThreadPriority.Normal;
#if DEBUG
                                    System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
                                    dataThread.Start();
                                }
								Prog_Step  = progStep.s13_WaitingForExit;
							}
							#endregion programming complete
							break;

						case progStep.s13_WaitingForExit:
							#region waiting for exit
							SystemInfo.errorSB.Length = 0;//judetemp - force the SC programming to pass - no boot up
							//errorString = "";  //judetemp - force the SC programming to pass - no boot up
							programmer.fatalErrorOccured = false; //judetemp - force the SC programming to pass - no boot up
							if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
							{
								timerStopFlag = true; //kill this timer becasue we are all done
								if(programmer.fatalErrorOccured == false)
								{
									SEVCONProgrammer.statusMessage = "programmed OK";
									if(this.testState == TestState.PROGRAMMING_SC_CODE)
									{
										#region End of bunging SC code in
										SEVCONProgrammer.statusMessage = "Controller Self Characterization Software Programmed OK"; 
										SEVCONProgrammer.userInstructionMessage = "Select Continue to start Self Characterization tests";
										this.testState = TestState.OPEN_LOOP_TEST1;
										SEVCONProgrammer.progBarValue = 0; //to set progress bar back
										//	this.SCProgramCB.Checked = true;
										this.Btn_Next.Visible = true;
										//No logggin in here - Self char has no login object yet
										#endregion End of bunging SC code in
									}
									else if(this.testState == TestState.PROGRAMMING_ORIGINAL_CODE)
									{
										#region end of shoving original software back in
										this.programmedOK = true;  //for closing event
										SEVCONProgrammer.statusMessage = "Controller Original Software re-programmed OK"; 
										if(SystemInfo.errorSB.Length>0) 
										{
											sysInfo.displayErrorFeedbackToUser("Non fatal errors occurred during programming");
										}
#if EXTERNAL_SELF_CHAR
										if((this.DCFFileCreatedOK == true) && (System.IO.File.Exists(MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\DATA\results.dcf")))
										{
											SEVCONProgrammer.userInstructionMessage =  "Continue to update node with Self Characterization results, or Close this window";
											this.Btn_Next.Text = "&Write results to node";
											this.testState = TestState.WRITE_RESULTS_TO_NODE;
											this.Btn_Next.Visible = true;
										}
										else
										{
											SEVCONProgrammer.userInstructionMessage = "External process failed to create DCF file: " + MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\DATA\results.dcf" + "\nClose this window";
										}
#else
									Message.Show("Process check that internal Self Char produced some writeable results here");
									bool InternalSelfCharTestResultsOK = true;
									if(InternalSelfCharTestResultsOK == true)
									{

										SEVCONProgrammer.userInstructionMessage =  "Continue to update node with Self Characterization results, or Close this window";
										this.Btn_Next.Text = "&Write results to node";
										this.Btn_Next.Visible = true;
										this.testState = TestState.WRITE_RESULTS_TO_NODE;
									}
									else
									{
										//failure message and actions here
									}
#endif
										#region add a 5 second delay - known issue on controller correct product code may not be avialable eimmediately
										long startTimeSecs = System.DateTime.Now.Ticks;
										long timeDiff = 0;
										while(timeDiff<50000000)  //2 seconds
										{
											timeDiff = (System.DateTime.Now.Ticks) - startTimeSecs;
										}
										#endregion add a temprary delay - known issue on controller prduct cod emay not be avialable eimmediately
										#region get 0x1018 identifty, login and force to pre-op ready to write results to CNAnode
										feedback = sysInfo.nodes[nodeIndex].readDeviceIdentity( );
										if(feedback == DIFeedbackCode.DISuccess)
										{
											programmer.getInfoFromBootloaderEDS();  //match an EDS
											if(programmer.fatalErrorOccured == false)
											{
												#region login to node & force to pre-op
												uint usrID = System.UInt32.Parse(MAIN_WINDOW.currentProfile.login.userid);
												uint psswrd = System.UInt32.Parse(MAIN_WINDOW.currentProfile.login.password);
												feedback = this.sysInfo.loginToNode(nodeIndex, usrID, psswrd);
												if(feedback == DIFeedbackCode.DISuccess)
												{
													feedback = this.sysInfo.forceSystemIntoPreOpMode();   //judetemp - we hav eto be in pre-op to ge tthe results into the node
												}
												#endregion login to node & force to pre-op
												Lbl_UsrProgrammingFeedback.Text = "Stage 6: Self Characterization Results now available for transmission to Controller";
												SEVCONProgrammer.statusMessage= "";  //force it to be overwirtten at end
											}
											#endregion get 0x1018 identifty, login and force to pre-op ready to write results to CNAnode
										}
										if(SystemInfo.errorSB.Length>0)
										{
											this.sysInfo.displayErrorFeedbackToUser("\nUnable to re-connect and login to device. Cycle power and retry");
										}
										#endregion end of shoving original software back in
									}	
								}
							}
							#endregion waiting for exit
							break;

						default:
							break;
					}
				}
				if((timerStopFlag == false) && (this.formClosing == false) && (programmer.fatalErrorOccured == false))
				{
					this.progressBar1.Visible = true;
					this.progTimer.Enabled = true;
				}
				else if(timerStopFlag == true)
				{
					this.progressBar1.Visible = false;
				}
				this.progressBar1.Minimum = SEVCONProgrammer.progBarMin;
				this.progressBar1.Maximum = SEVCONProgrammer.progBarMax;
				this.progressBar1.Value = SEVCONProgrammer.progBarValue;
				this.Lbl_UserInstructions.Text = SEVCONProgrammer.userInstructionMessage;
				this.statusBar1.Text = SEVCONProgrammer.statusMessage;
				if(programmer.fatalErrorOccured == true)
				{
					this.processErrorString();
				}
			}
		}
		private void startProgrammingprocess()
		{
			Prog_Step = progStep.s01_GettingAppVers;
			if(this.testState == TestState.PROGRAMMING_SC_CODE)
			{
				programmedOK = false;
			}
			this.Lbl_UserInstructions.Text = "Please wait"; //clear whilst DW is working
			this.Btn_Next.Visible = false;  //hide while timer is running
			this.statusBar1.Text = "Preparing for node programming"; 
			this.progressBar1.Visible = true;
			programmer = new SEVCONProgrammer(this.nodeIndex, sysInfo, errorHandlingLevel);
			programmer._preserveEEContentsAndFormat = true;  //prevent any EEPROM format conversion druing self char
			programmer._preserveEEPROMContents = true; //prevent any writing to EEPRM during self char
			#region data retrieval thread start
			dataThread = new Thread(new ThreadStart( getAppsVerisonsWrapper )); 
			dataThread.Name = "Get Application Verisons";
			dataThread.IsBackground = true;
            dataThread.Priority = ThreadPriority.Normal;
#if DEBUG
			System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
			dataThread.Start(); 
			this.progressBar1.Visible = true;
			this.progTimer.Enabled = true;
			#endregion
		}

		private void createInitialTable()
		{
			DataRow row;
			this.table = new ProgTable();
			for (int i = 0; i<NodeParams.Length; i++) 	
			{
				row = this.table.NewRow();
				row[(int)(progCols.Parameter)] = NodeParams[i];
				row[(int)(progCols.Node)] = "---";
				row[(int)(progCols.dldFile)] = "---";
				this.table.Rows.Add(row);
			}
			this.table.DefaultView.AllowNew = false;
			dataGrid1.DataSource = this.table;
			dataGrid1.Height = ((dataGrid1.VisibleRowCount + 2) * dataGrid1.PreferredRowHeight) + (dataGrid1.VisibleRowCount-1);
		}

		#region profile data reading/editing/saving

		/// <summary>
		/// Relevant to Internal Self CHar only
		/// </summary>
		private void updateOLTestPoints()
		{
#if EXTERNAL_SELF_CHAR
#else
			this.TV_OL_testpoints.Nodes.Clear(); //empty the TreeView 
			ushort ptCtr = 1;
			foreach(PointF tstPt in this.mySelfChar.scprofile.openloop.testPoints)
			{
				//convert the percentage in the porfile to a real voltage
				double percentV = tstPt.Y * 100;
				double actVolt = tstPt.Y *this.mySelfChar.scprofile.powerframe.nomBattVolts;
				string voltStr = actVolt.ToString("G4");
				this.TV_OL_testpoints.Nodes.Add(new TreeNode("Volts: " + voltStr + "V (" + percentV.ToString() + "%) ," + "Time: " + tstPt.X.ToString()) +"s");
				ptCtr++;
			}
#endif
		}
		/// <summary>
		/// Relevant to Internal Self Char only
		/// </summary>
		private void updateCLTestPoints()
		{
#if EXTERNAL_SELF_CHAR
#else
			this.TV_CL_TestPoints.Nodes.Clear();
			foreach(SCProf_ClosedLoop.SCProf_CLtest cltest in this.mySelfChar.scprofile.closedloop.CLtests)
			{
				TreeNode testName = new TreeNode(cltest.testName);  //will be CLA or CLB 
				ushort ptNum = 1;
				foreach(PointF tstPt in cltest.testpoints)
				{
					testName.Nodes.Add(new TreeNode("Current: " + tstPt.X.ToString() + "A, " + "Speed: " + tstPt.Y.ToString()) + "rpm");
					ptNum++;
				}
				TV_CL_TestPoints.Nodes.Add(testName);
			}
			foreach(TreeNode node in this.TV_CL_TestPoints.Nodes)
			{
				node.Expand();
			}
#endif
		}
/// <summary>
/// Relevant to Internal Self CHar only
/// </summary>
		private void updateNLTTestPoints()
		{
#if EXTERNAL_SELF_CHAR
#else
			this.TV_NLT_testPts.Nodes.Clear();
			ushort ptCtr = 1;
			foreach( float spdPt in this.mySelfChar.scprofile.noloadtest.speedpoints)
			{
				this.TV_NLT_testPts.Nodes.Add(new TreeNode("speed pt " + ptCtr.ToString() + ": " + spdPt.ToString())+"rpm");
				ptCtr++;
			}
#endif
		}
		private void setupContextMenus()
		{
			#region Open and CLosed Loop
			MenuItem editTpt = new MenuItem("Edit this test point");
			editTpt.Click +=new EventHandler(editTpt_Click);
			MenuItem addTptBel = new MenuItem("Add new test point below");
			addTptBel.Click +=new EventHandler(addTpt_Click);
			MenuItem addTptAbv = new MenuItem("Add new test point above");
			addTptAbv.Click +=new EventHandler(addTpt_Click);
			MenuItem delTpt = new MenuItem("Delete this test point");
			delTpt.Click +=new EventHandler(delTpt_Click);
			this.CM_OLoop_and_CLoop.MenuItems.Add(editTpt);
			this.CM_OLoop_and_CLoop.MenuItems.Add(addTptBel);
			this.CM_OLoop_and_CLoop.MenuItems.Add(addTptAbv);
			this.CM_OLoop_and_CLoop.MenuItems.Add(delTpt);
			#endregion Open and CLosed Loop

			#region No Load Test
			MenuItem NLTeditTpt = new MenuItem("Edit this speed point");
			NLTeditTpt.Click +=new EventHandler(editTpt_Click);
			MenuItem NLTaddTptBel = new MenuItem("Add new speed point below");
			NLTaddTptBel.Click +=new EventHandler(addTpt_Click);
			MenuItem NLTaddTptAbv = new MenuItem("Add new speed point above");
			NLTaddTptAbv.Click +=new EventHandler(addTpt_Click);
			MenuItem NLTdelTpt = new MenuItem("Delete this speed point");
			NLTdelTpt.Click +=new EventHandler(delTpt_Click);
			this.CM_NLT.MenuItems.Add(NLTeditTpt);
			this.CM_NLT.MenuItems.Add(NLTaddTptBel);
			this.CM_NLT.MenuItems.Add(NLTaddTptAbv);
			this.CM_NLT.MenuItems.Add(NLTdelTpt);
			#endregion No Load Test
		}


		#region context menu event handlers
		private void TreeView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{
				selectedTreeNode = null;
				switch (this.tabControl1.SelectedIndex)
				{
					case (int)TabPages.OPEN_LOOP:
						#region open loop
						selectedTreeNode = this.TV_OL_testpoints.GetNodeAt(this.TV_OL_testpoints.PointToClient(Cursor.Position));
						if(selectedTreeNode != null)  //we clicked over a node
						{
							this.TV_OL_testpoints.SelectedNode = selectedTreeNode;
							this.CM_OLoop_and_CLoop.Show(this.selectedTreeNode.TreeView, this.selectedTreeNode.TreeView.PointToClient(Cursor.Position));
						}
						#endregion open loop
						break;
					case (int) TabPages.CLOSED_LOOP:
						#region closed loop
						selectedTreeNode = this.TV_CL_TestPoints.GetNodeAt(this.TV_CL_TestPoints.PointToClient(Cursor.Position));
						if(selectedTreeNode != null)  //we clicked over a node
						{
							this.TV_CL_TestPoints.SelectedNode = selectedTreeNode;
							if(this.selectedTreeNode.Parent!= null) //only show for 2nd levle test points
							{
								this.CM_OLoop_and_CLoop.Show(this.selectedTreeNode.TreeView, this.selectedTreeNode.TreeView.PointToClient(Cursor.Position));
							}
						}
						break;
						#endregion closed loop

					case (int)TabPages.NO_LOAD_TEST:
						#region no load test
						selectedTreeNode = this.TV_NLT_testPts.GetNodeAt(this.TV_NLT_testPts.PointToClient(Cursor.Position));
						if(selectedTreeNode != null)  //we clicked over a node
						{
							this.TV_NLT_testPts.SelectedNode = selectedTreeNode;
							this.CM_NLT.Show(this.TV_NLT_testPts, this.TV_NLT_testPts.PointToClient(Cursor.Position));
						}
						break;
						#endregion no load test
					default:
						return;
				}
			}
		}
		/// <summary>
		/// Relevant ot Internal self Char only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void editTpt_Click(object sender, EventArgs e)
		{
#if EXTERNAL_SELF_CHAR
#else
			switch (this.tabControl1.SelectedIndex)
			{
				case (int)TabPages.OPEN_LOOP:
					Form frm = new SC_TESTPOINT_DIALOG(TabPages.OPEN_LOOP);
					frm.ShowDialog();
					//then copy to our array of points
					this.mySelfChar.scprofile.openloop.testPoints[this.selectedTreeNode.Index] = DriveWizard.SC_TESTPOINT_DIALOG.testPointData;
					this.updateOLTestPoints();
					break;
				case (int) TabPages.CLOSED_LOOP:
					frm = new SC_TESTPOINT_DIALOG(TabPages.CLOSED_LOOP);
					frm.ShowDialog();
					ArrayList CLtests = (ArrayList) this.mySelfChar.scprofile.closedloop.CLtests;
					SCProf_ClosedLoop.SCProf_CLtest cltst = (SCProf_ClosedLoop.SCProf_CLtest) CLtests[this.selectedTreeNode.Parent.Index];
					ArrayList CLtestPoints = (ArrayList) cltst.testpoints;
					CLtestPoints[this.selectedTreeNode.Index] = DriveWizard.SC_TESTPOINT_DIALOG.testPointData;
					this.updateCLTestPoints();
					break;
				case (int)TabPages.NO_LOAD_TEST:
					frm = new SC_TESTPOINT_DIALOG(TabPages.NO_LOAD_TEST);
					frm.ShowDialog();
					this.mySelfChar.scprofile.noloadtest.speedpoints[this.selectedTreeNode.Index] = DriveWizard.SC_TESTPOINT_DIALOG.testPointData.Y;
					updateNLTTestPoints();
					break;
			}
#endif
		}
		/// <summary>
		/// Relevant to Internla Self Char only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void delTpt_Click(object sender, EventArgs e)
		{
#if EXTERNAL_SELF_CHAR
#else
			switch (this.tabControl1.SelectedIndex)
			{
				case (int)TabPages.OPEN_LOOP:
					this.mySelfChar.scprofile.openloop.testPoints.RemoveAt(this.selectedTreeNode.Index);
					this.updateOLTestPoints();
					break;
				case (int) TabPages.CLOSED_LOOP:
					ArrayList clTests = (ArrayList)this.mySelfChar.scprofile.closedloop.CLtests;
					SCProf_ClosedLoop.SCProf_CLtest cltst = (SCProf_ClosedLoop.SCProf_CLtest) clTests[this.selectedTreeNode.Parent.Index];
					ArrayList CLtestPoints = (ArrayList) cltst.testpoints;
					CLtestPoints.RemoveAt(this.selectedTreeNode.Index);
					this.updateCLTestPoints();
					break;
				case (int)TabPages.NO_LOAD_TEST:
					this.mySelfChar.scprofile.noloadtest.speedpoints.RemoveAt(this.selectedTreeNode.Index);
					this.updateNLTTestPoints();
					break;
			}
#endif
		}

		private void addTpt_Click(object sender, EventArgs e)
		{
#if EXTERNAL_SELF_CHAR
#else
			bool insertBelowSelectedTestPoint = false;
			MenuItem senderMI = (MenuItem) sender;
			if(senderMI.Text.ToLower().IndexOf("below") != -1)
			{
				insertBelowSelectedTestPoint = true;
			}
			switch (this.tabControl1.SelectedIndex)
			{
				case (int)TabPages.OPEN_LOOP:
					#region open loop
					Form frm = new SC_TESTPOINT_DIALOG(TabPages.OPEN_LOOP);
					frm.ShowDialog();
					if(insertBelowSelectedTestPoint == true) 
					{
						if(this.selectedTreeNode.Index>=this.TV_OL_testpoints.Nodes.Count)
						{//append
							this.mySelfChar.scprofile.openloop.testPoints.Add(DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
						}
						else 
						{//insert below
							this.mySelfChar.scprofile.openloop.testPoints.Insert(this.selectedTreeNode.Index+1, DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
						}
					}
					else
					{ //insert above
						this.mySelfChar.scprofile.openloop.testPoints.Insert(this.selectedTreeNode.Index, DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
					}
					updateOLTestPoints();
					break;
					#endregion open loop
				case (int) TabPages.CLOSED_LOOP:
					#region closed loop
					frm = new SC_TESTPOINT_DIALOG(TabPages.CLOSED_LOOP);
					frm.ShowDialog();
					ArrayList CLtests = (ArrayList) this.mySelfChar.scprofile.closedloop.CLtests;
					SCProf_ClosedLoop.SCProf_CLtest cltst = (SCProf_ClosedLoop.SCProf_CLtest) CLtests[this.selectedTreeNode.Parent.Index];
					ArrayList CLtestPoints = (ArrayList) cltst.testpoints;
					if(insertBelowSelectedTestPoint == true)
					{
						if(this.selectedTreeNode.Index>=this.selectedTreeNode.Parent.Nodes.Count)
						{//append
							CLtestPoints.Add(DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
						}
						else
						{//insert below
							CLtestPoints.Insert(this.selectedTreeNode.Index+1, DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
						}
					}
					else
					{ //insert above
						CLtestPoints.Insert(this.selectedTreeNode.Index, DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
					}
					updateCLTestPoints();
					break;
					#endregion closed loop
				case (int)TabPages.NO_LOAD_TEST:
					#region no load test
					frm = new SC_TESTPOINT_DIALOG(TabPages.NO_LOAD_TEST);
					frm.ShowDialog();
					if(insertBelowSelectedTestPoint ==true)
					{
						if(this.selectedTreeNode.Index>=this.TV_NLT_testPts.Nodes.Count)
						{ //append
							this.mySelfChar.scprofile.noloadtest.speedpoints.Add(DriveWizard.SC_TESTPOINT_DIALOG.testPointData.Y);
						}
						else
						{ //insert below
							this.mySelfChar.scprofile.noloadtest.speedpoints.Insert(this.selectedTreeNode.Index+1, DriveWizard.SC_TESTPOINT_DIALOG.testPointData.Y);
						}
					}
					else
					{ //insert above
						this.mySelfChar.scprofile.noloadtest.speedpoints.Insert(this.selectedTreeNode.Index, DriveWizard.SC_TESTPOINT_DIALOG.testPointData.Y);
					}
					this.updateNLTTestPoints();
					break;
					#endregion no load test
			}
#endif
		}
		#endregion context menu event handlers


		#endregion profile data reading/editing/saving

		#region External Self Char only
		private bool overwriteDeviceInfoInOuputDCFFile()
		{
#if EXTERNAL_SELF_CHAR
			StreamWriter srWrite;
			StreamReader srRead;
			if(File.Exists(MAIN_WINDOW.UserDirectoryPath + @"\SelfChar\data\results.dcf") == false)
			{
				Message.Show("External process output DCF file does not exist");
				return false;
			}
			try
			{
				srRead = new StreamReader(MAIN_WINDOW.UserDirectoryPath + @"\SelfChar\data\results.dcf", System.Text.Encoding.ASCII );
				srWrite = new StreamWriter(MAIN_WINDOW.UserDirectoryPath + @"\SelfChar\data\temp.dcf",false); 
			}
			catch
			{
				Message.Show("Unable to modify DCF file. Product code and Revision number must be changed manually");
				return false;
			}
			string input = "";
			while ( ( input = srRead.ReadLine() ) != null ) 
			{
				if(input.IndexOf("ProductNumber") != -1)
				{
					#region correct product code and revision number
					string prodCodeStr = this.appProductCode.ToString("X");
					while (prodCodeStr.Length<8)
					{
						prodCodeStr = "0" + prodCodeStr;
					}
					srWrite.WriteLine("ProductNumber=0x" + prodCodeStr);
					string revNumStr = this.appRevisionNumber.ToString("X");
					while(revNumStr.Length<8)
					{
						revNumStr = "0" + revNumStr;
					}
					srWrite.WriteLine("RevisionNumber=0x"+ revNumStr);
					input = srRead.ReadLine(); //step over the Revision numberline
					while ((input = srRead.ReadLine())!= null)
					{
						srWrite.WriteLine(input);//write rest of file to output 
					}
					break;
					#endregion correct product code and revision number
				}
				else
				{
					srWrite.WriteLine(input);
				}
			}
			#region close the Stream readers and delete temporary file
			srRead.Close();
			srWrite.Close();
			try
			{
				File.Copy(MAIN_WINDOW.UserDirectoryPath + @"\SelfChar\data\temp.dcf", MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\DATA\results.dcf", true);
				File.Delete(MAIN_WINDOW.UserDirectoryPath + @"\SelfChar\data\temp.dcf");
			}
			catch(Exception e)
			{
				Message.Show("Unable to modfiy Self Characterization DCF file.Error: " + e.Message);
				return false;
			}
			#endregion close the Stream readers and delete temporary file
			return true;
#else
			return true;  //for compiler only
#endif
		}
		private void writeDCFToDevice()
		{
#if EXTERNAL_SELF_CHAR
			string abortMessage = ""; //reset the error message
			this.sysInfo.itemBeingWritten = 0;
			DIErrorInfo = "";
			//feedback code is bound into abortMessage now
			this.sysInfo.writePartialDCFNodeToDevice( this.sysInfo.nodes[nodeIndex]);
			if(abortMessage != "")
			{
				DIErrorInfo = "Failed to download DCF:" + abortMessage;
			}

			//			feedback = this.sysInfo.writeEntireODFromDCF(nodeIndex);
#endif
		}
		private void externalSCExited(object sender, EventArgs e)
		{
#if EXTERNAL_SELF_CHAR
			this.SCWizExitcode = ExtSCProcess[0].ExitCode;
			this.extProcComplete = true;
			ExtSCProcess = null;  //for window closing methods
			this.progTimer.Enabled = true;  //to reestablish comms with IXATT
#endif
		}
		#endregion External Self Char only
		#endregion

		#region finalisation/exit
		private void exitSelfChar_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
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

		private void SELF_CHARACTERISATION_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.formClosing = true;
			#region remember program timer state and then disable it for now
			bool timerWasrunning = false;
			if(this.progTimer.Enabled == true)
			{
				timerWasrunning = true;
			}
			this.progTimer.Enabled = false;  //kill it for now
			#endregion remember program time rstate and then disable it for now
			if(this.programmedOK == false)  //programming not successfully completed
			{
				#region programming not successfully completed
				if(DriveWizard.SEVCONProgrammer.CodeHasBeenSent == true)
				{
					#region code has been sent
					if(programmer.fatalErrorOccured == false)  //no errors haver occurred
					{
						string message = "Data has been overwritten. This SEVCON node be unusable unless both programming cycles are completed. Are you sure?";
						DialogResult result = Message.Show(this, message, "Warning", MessageBoxButtons.YesNo,
							MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
						if(result == DialogResult.No)
						{
							#region Go back to where we left off
							if(timerWasrunning == true)	
							{
								this.progTimer.Enabled  = true;
							}
							this.formClosing = false;
							e.Cancel = true;
							return;
							#endregion Go back to where we left off
						}
					}
					#endregion code has been sent
				}
				else  //no code downloaded yet
				{
					if(programmer.fatalErrorOccured == false)
					{
						#region no errors reported - give user choice to continue
						if(this.testState != TestState.PROGRAMMING_SC_CODE)
						{
							DialogResult result2 = Message.Show(this, "Node may still contain self Charactersation software and be unuasable. Are you sure?",
								"Warning", MessageBoxButtons.YesNo,	MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
							if(result2 == DialogResult.No)
							{
								#region Go back to where we left off
								if(timerWasrunning == true)	
								{
									this.progTimer.Enabled = true;
								}
								this.formClosing = false;
								e.Cancel = true;
								return;
								#endregion Go back to where we left off
							}
						}
						#endregion no errors reported - give user choice to continue
					}
					#region  try to force application to start
					if (sysInfo.nodes[nodeIndex].productVariant == SCCorpStyle.bootloader_variant)
					{
						//attempt to pull the node out of programming mode by writing  zero wholeDataChecksum to it
						programmer.forceNodeOutOfBoot();
					}
					#endregion  try to force application to start
				}
				#endregion programming not successfully completed
			}
			//if we get this far then this window is definately closing - so tidy up and get out
			e.Cancel = false;   //will force this window to close on ending this function
			this.statusBar1.Text = "Performing finalisation, please wait";
			#region kill any external processes and grab dongle back
#if EXTERNAL_SELF_CHAR
			Directory.SetCurrentDirectory(MAIN_WINDOW.UserDirectoryPath);
			try
			{
				if(ExtSCProcess != null)
				{
					if(ExtSCProcess.Length>0)
					{
						this.Lbl_UserInstructions.Text = "Please wait";
						this.statusBar1.Text = "Closing external application and restarting CAN communications";
						for(int i = 0;i<ExtSCProcess.Length;i++)
						{
							ExtSCProcess[i].Kill();  //close etc doesn't work here
							ExtSCProcess[i].Exited -=new EventHandler(externalSCExited); //prevent the handler running
						}
						this.sysInfo.CANcomms.restartCAN(false);
						this.Lbl_UserInstructions.Text = "";
						this.statusBar1.Text =  "Ready";
					}
				}
			}
			catch
			{

			}
#endif
			#endregion kill any external processes and grab dongle back
			#region diable all timers
			this.timer1.Enabled = false;
			this.progTimer.Enabled = false;
			#endregion diable all timers
			#region stop all threads
			if(selfCharDataRetrievalThread != null)
			{
				if((selfCharDataRetrievalThread.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
				{
					selfCharDataRetrievalThread.Abort();

					if(selfCharDataRetrievalThread.IsAlive == true)
					{
#if DEBUG
						Message.Show("Failed to close Thread: " + selfCharDataRetrievalThread.Name + " on exit");
#endif
						selfCharDataRetrievalThread = null;
					}
				}
			}
			#endregion stop all threads
			#region dispose of Programming component
			if(this.programmer != null)
			{
				programmer.Dispose();
			}
			#endregion dispose of Programming component
			if((DriveWizard.SEVCONProgrammer.CodeHasBeenSent == true)
				|| (DriveWizard.SEVCONProgrammer.EDSFileChanged == true))
			{
				MAIN_WINDOW.reEstablishCommsRequired = true;
				DriveWizard.SEVCONProgrammer.EDSFileChanged = false;
				DriveWizard.SEVCONProgrammer.CodeHasBeenSent = false;  //reset this flag here
			}
			else
			{
				//refresh product code from node
				feedback = sysInfo.nodes[nodeIndex].readDeviceIdentity( );
				if((this.sysInfo.nodes[nodeIndex].productVariant == SCCorpStyle.bootloader_variant )
					||  (this.sysInfo.nodes[nodeIndex].productVariant == SCCorpStyle.selfchar_variant_old)
					||  (this.sysInfo.nodes[nodeIndex].productVariant == SCCorpStyle.selfchar_variant_new)
					||  (feedback != DIFeedbackCode.DISuccess))  
				{
					MAIN_WINDOW.reEstablishCommsRequired = true;
				}
			}
		}
		private void SELF_CHARACTERISATION_WINDOW_Closed(object sender, System.EventArgs e)
		{
			#region reset window title and status bar
			this.statusBar1.Text = "";
			#endregion reset window title and status bar	 		
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		#endregion

		#region minor functions
		private void gotoENTER_LINE_CONTACTORTestState()
		{
			this.PnlLineContactors.BringToFront();
			this.Lbl_UserInstructions.Text = "Confirm characteristics for all line contactors connected to this vehicle";
			testState = TestState.ENTER_LINE_CONTACTOR;
		}
		private void gotoENTER_MOTOR_LIMITSTestState()
		{
			this.PnlMotorLimits.BringToFront();
			this.Lbl_UserInstructions.Text = "Confim maximum operating limits for <MOTOR NAME> motor during testing";
			testState = TestState.ENTER_MOTOR_LIMITS;
		}
		private void gotoENTER_PLATE_DATATestState()
		{
			this.Pnl_ConfirmNamePlateData.BringToFront();
			this.testState = TestState.ENTER_PLATE_DATA;
			this.Lbl_UserInstructions.Text = "Confirm the motor plate data for <MOTORNAME> motor";

		}
		private void gotoENTER_TESTIFILESTestState()
		{
			this.PnlTestFiles.BringToFront();
			this.Lbl_UserInstructions.Text = "Confirm profiles to be used during testing";
			testState = TestState.ENTER_TESTIFILES;
		}
		private void gotoREENTER_LINE_CONTACTORTestState()
		{
			this.PnlLineContactors.BringToFront();
			this.Lbl_UserInstructions.Text = "Confirm characteristics for all line contacotrs connected to this vehicle";
			testState = TestState.REENTER_LINE_CONTACTOR;
		}
		private void gotoREENTER_MOTOR_LIMITSTestState()
		{
			this.PnlMotorLimits.BringToFront();
			this.Lbl_UserInstructions.Text = "Confim maximum operating limits for <MOTOR NAME> motor during testing";
			testState = TestState.REENTER_MOTOR_LIMITS;
		}
		private void gotoREENTER_PLATE_DATATestState()
		{
			this.Pnl_ConfirmNamePlateData.BringToFront();
			this.testState = TestState.REENTER_PLATE_DATA;
			this.Lbl_UserInstructions.Text = "Confirm the motor plate data for <MOTORNAME> motor";
			this.Btn_Back.Visible = true;
			this.restartTestButton.Visible = false;
		}
		private void gotoREENTER_TESTIFILESTestState()
		{
			this.PnlTestFiles.BringToFront();
			this.Lbl_UserInstructions.Text = "Confirm profiles to be used during testing";
			testState = TestState.REENTER_TESTIFILES;
		}
		private void gotoPROGRAMMING_SC_CODETestState()
		{
			this.PnlDataGrids.BringToFront();
			this.Lbl_UserInstructions.Text = "Programming CANNode with Self Characterization software";
			testState = TestState.PROGRAMMING_SC_CODE;
		}

		private void gotoPROGRAMMING_ORIGINAL_CODETestState()
		{
			this.Btn_Next.Visible = false;
			this.restartTestButton.Visible = false;
			this.PnlDataGrids.BringToFront();
			this.Lbl_UsrProgrammingFeedback.Text = "Stage 5: Re-programing Sevcon controller with normal Application software";
			this.testState = TestState.PROGRAMMING_ORIGINAL_CODE;
		}


		private bool plateDataOK()
		{
			//Perform 
			//note we have to do this the hard way -Databindings require an object's 
			//porperty to be bound to a propety of a window control. Sadly 
			//			this.currentMotor.platedata.encoderPulsesPerRev = this.TB_DW_EncoderPulsesPerRev.Text;
			return true;
		}

		private bool motorLimitsOK()
		{
			return true;
		}
		private bool lineContactorDataOK()
		{
			return true;
		}
		private bool testFilesOK()
		{
			return true;
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			setupCheckBoxDisplays
		///		 *  Description		Method to determine the initialcheckstae and enabled state of 
		///		 *					the EEPROM related checkboxes. Requirments for this functionality are not yet finalised 
		///		 *					and so these checkboxes are currently set to be non visible
		///		 *  Parameters      none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		/// 

		private void processErrorString()
		{
			if(programmer.fatalErrorOccured == true)
			{
				SystemInfo.errorSB.Append("\nThis window will close");
				this.sysInfo.displayErrorFeedbackToUser("\nFatal errors occured suring programming");
				this.Close();  //now close this window
				return;
			}
			else
			{

			}
		}

		#endregion

		#region methods relating to Internal Self Char only

		
		private void menuItem2_Click(object sender, System.EventArgs e)
		{
#if EXTERNAL_SELF_CHAR
#else
			#region only used for internal self char
			openFileDialog1.FileName = "";  //ensure that initial directory works correctly
			if ( Directory.Exists( MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES" ) == false )
			{
				try
				{
					Directory.CreateDirectory( MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES" );
				}
				catch{}
			}
			openFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES";
			openFileDialog1.Title = "Open existing Self Char Test Profile";
			openFileDialog1.Filter = "Data files (*.xml)|*.xml" ;
			openFileDialog1.DefaultExt = "xml";
			openFileDialog1.ShowDialog(this);

			if(openFileDialog1.FileName != "")
			{
				ObjectXMLSerializer xmlSerializer = new ObjectXMLSerializer();
				try
				{
					this.mySelfChar.scprofile = (SCprofile) xmlSerializer.Load(this.mySelfChar.scprofile, openFileDialog1.FileName);
				}
				catch(Exception ex)
				{
					Message.Show("Unable to load Self CHar Test profile. Ensure file is closed and retry \nErrocr code: " + ex.Message); 
					return;
				}
				//we MUST re-bind after opening XML
			}
			this.bindProfileControls();
			#endregion only used for internal self char
#endif
		}
		

		#endregion methods relating to Internal Self Char only
		#region datathread wrappers
		private void codeTransferWrapper()
		{
			programmer.downloadCodeControl(DldFilename);
		}
		private void completeDownloadWrapper()
		{
			programmer.completeDownloadControl();
		}
		private void compareDldFileToDeviceWrapper()
		{
			programmer.CompareDLDFileToDevice(DldFilename, out downloadParams, out DldFiletestResults);
		}
		private void getAppsVerisonsWrapper()
		{
			programmer.getAppVersions(out hostAppController, out DSPAppController);
		}
		private void setBootModeWrapper()
		{
            //DR38000172 need nodeIndex so that can set CAN filters to listen only to SDOs from this node
            programmer.setBootMode(this.nodeIndex);
		}
		private void getBootInfoWrapper()
		{
			programmer.getBootInfo(out controllerBootParams);

		}
		private void getBootEDSWrapper()
		{
			programmer.getInfoFromBootloaderEDS();
		}
		#endregion datathread wrappers

		#region methods relevent to Externla self CHar only
		private void changeTPfilepathBtn_Click(object sender, System.EventArgs e)
		{
			#region external self char only
#if EXTERNAL_SELF_CHAR
			openFileDialog1.FileName = "";
			openFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\SelfChar";
			openFileDialog1.Title = "Select Test Profile file";
			openFileDialog1.Filter = "download files (*.wzt)|*.wzt" ;
			openFileDialog1.ShowDialog(this);

			if(openFileDialog1.FileName.ToLower().IndexOf(".wzt") != -1)
			{
				this.LblTestProfile.Text = openFileDialog1.FileName;
				this.currentMotor.testProfileFilepath = openFileDialog1.FileName;
				string filePath = openFileDialog1.FileName;
				filePath = filePath.Replace(" " ,@"<SPACE>");
				SCWiz_argv[(int) SCWizargs.DW_TestProfileFilepath] = filePath;// openFileDialog1.FileName;
			}
#endif
#endregion externla self char only
		}

		private void ChangePFRFilepath_Click(object sender, System.EventArgs e)
		{
			#region external self char only
#if EXTERNAL_SELF_CHAR
			openFileDialog1.FileName = "";
			openFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\SelfChar";
			openFileDialog1.Title = "Select Power Frame Profile file";
			openFileDialog1.Filter = "download files (*.wzp)|*.wzp";
			openFileDialog1.ShowDialog(this);

			if(openFileDialog1.FileName.ToLower().IndexOf(".wzp") != -1)
			{
				this.LblPowerFrameProfile.Text = openFileDialog1.FileName;
				this.currentMotor.powerframeProfileFilepath = openFileDialog1.FileName;
				string filePath = openFileDialog1.FileName;
				filePath = filePath.Replace(" " ,@"<SPACE>");
				SCWiz_argv[(int) SCWizargs.DW_PowerFrameProfilefilePath] = filePath;
			}
#endif
			#endregion external self char only
		}


		#endregion methods relevent to Externla self CHar only
		public void passPreTestDataToController()
		{
			DIFeedbackCode feedback;
			this.sysInfo.nodes[this.nodeIndex].readDeviceIdentity();
			sysInfo.nodes[this.nodeIndex].findMatchingEDSFile();
			if(SystemInfo.errorSB.Length>0)
			{
				this.sysInfo.displayErrorFeedbackToUser("Fatal error: \n");
				this.Close();
				return;
			}
			sysInfo.nodes[this.nodeIndex].readEDSfile();
			StringBuilder errMsg = new StringBuilder();
			//note we are sending dat afor the fist Line contacotr only currently
			//changes to the controller are needed to handle additional line contactors
			if(MAIN_WINDOW.currentProfile.lineContactors.Count<=0)
			{
				Message.Show("No line contactors set up for this vehicle");
				return;
			}
			VPLineContactor lc = (VPLineContactor) MAIN_WINDOW.currentProfile.lineContactors[0];
			Message.Show("passing line contactor info to controller");
			ODItemData odSub = this.sysInfo.nodes[nodeIndex].getODSubFromObjectType(SevconObjectType.CONTACTOR_PARAM, 1);
			if(odSub != null)
			{
				feedback = sysInfo.nodes[nodeIndex].writeODValue(odSub, (long)lc.pullInVolts);
				if(feedback != DIFeedbackCode.DISuccess)
				{
					//TODO
				}
			}
			odSub = this.sysInfo.nodes[nodeIndex].getODSubFromObjectType(SevconObjectType.SELFCHAR_LINECONTACTOR, 2);
			if(odSub != null)
			{
				long newVal = (long) (odSub.scaling * lc.pullInms);
				feedback = this.sysInfo.nodes[nodeIndex].writeODValue(odSub, newVal);
				if(feedback != DIFeedbackCode.DISuccess)
				{
				}
			}
			odSub =  this.sysInfo.nodes[nodeIndex].getODSubFromObjectType(SevconObjectType.CONTACTOR_PARAM, 3);
			if(odSub != null)
			{
				feedback = this.sysInfo.nodes[nodeIndex].writeODValue(odSub, (long) lc.holdVolts);
				if(feedback != DIFeedbackCode.DISuccess)
				{
				}
			}
		}
		private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
#if EXTERNAL_SELF_CHAR
			#region write DCF to CAN device
			if (writeDCFToDeviceThread != null)
			{
				if((writeDCFToDeviceThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
					timer1.Enabled = false; //kill timer
					if(DIErrorInfo.Length == 0) //this way round to update label1 correctly
					{
						this.statusBar1.Text = "Save to Node completed OK";
						this.Lbl_UserInstructions.Text = "Close this window";
						Lbl_UsrProgrammingFeedback.Text = "Complete: Node updated with Self Characterization parameters";
					}
					else
					{
						this.statusBar1.Text = "DCF download failed";
						this.statusBar1.Update();
						//when writing multiple items abortMessage containes entire error from DI
						Message.Show(DIErrorInfo);
					}
					this.progressBar1.Visible = false;
				}
				else
				{
					if(this.sysInfo.itemBeingWritten<this.progressBar1.Maximum)
					{
						this.progressBar1.Value = this.sysInfo.itemBeingWritten;
					}
				}
			}
			#endregion write DCF to CAN device
#endif
		}

		private void handleResizeDataGrid(DataGrid myDG)
		{
			if(myDG.TableStyles.Count>0)
			{
				int [] ColWidths = null;
				if(myDG == this.dataGrid1)  
				{
					ColWidths  = new int[this.Programmingpercents.Length];
					ColWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, Programmingpercents, 0, dataGrid1DefaultHeight);
				}
				else
				{
					return;
				}
				for (int i = 0;i<ColWidths.Length;i++)
				{
				{
					myDG.TableStyles[0].GridColumnStyles[i].Width = ColWidths[i];
				}
				}	
				myDG.Invalidate();
			}

		}
		private void Btn_addLC_Click(object sender, System.EventArgs e)
		{
			VPLineContactor lineContactor = new VPLineContactor();
			MAIN_WINDOW.currentProfile.lineContactors.Add(lineContactor);

			if(MAIN_WINDOW.currentProfile.lineContactors.Count ==1) //ie the one we have just added
			{
				this.CB_LineContactorNo.Enabled = true;
				this.TB_DW_LC_holdVolts.Enabled = true;
				this.TB_DW_LC_PullInms.Enabled = true;
				this.TB_DW_LC_PullInVolts.Enabled = true;
				this.CB_LCNodeID.Enabled = true;
				this.Btn_deleteLC.Enabled = true;
				if(this.currManager == null)
				{
					this.addLCControlsDatabinding();
				}
			}
			object newItem = this.CB_LineContactorNo.Items.Count+1;
			this.CB_LineContactorNo.Items.Add(newItem);
			this.CB_LineContactorNo.SelectedIndex = this.CB_LineContactorNo.Items.Count-1;
			this.CB_LCNodeID.SelectedIndex = -1; //set to 'none selected'
			
		}

		private void Btn_deleteLC_Click(object sender, System.EventArgs e)
		{
			if((this.CB_LineContactorNo.SelectedIndex != -1) //sometihng is selected
				&& (MAIN_WINDOW.currentProfile.lineContactors.Count>this.CB_LineContactorNo.SelectedIndex))
			{
				hideUserControls();
				MAIN_WINDOW.currentProfile.lineContactors.RemoveAt(this.CB_LineContactorNo.SelectedIndex);
				#region store user data to vehicle profile file for use as defaults
				ObjectXMLSerializer vpXMLSerializer = new ObjectXMLSerializer();
				vpXMLSerializer.Save(MAIN_WINDOW.currentProfile, MAIN_WINDOW.currentProfile.ProfilePath);
				#endregion store user data to vehicle profile file  for use as defaults
				this.CB_LineContactorNo.Items.RemoveAt(this.CB_LineContactorNo.Items.Count-1); //remove highest number
				if(this.CB_LineContactorNo.Items.Count>0)
				{
					this.CB_LineContactorNo.SelectedIndex = 0;
				}
				else
				{
					this.CB_LineContactorNo.Enabled = false;
					this.TB_DW_LC_holdVolts.Enabled = false;
					this.TB_DW_LC_PullInms.Enabled = false;
					this.TB_DW_LC_PullInVolts.Enabled = false;
					this.CB_LCNodeID.Enabled = false;
					this.CB_LineContactorNo.Enabled = false;
					this.Btn_deleteLC.Enabled = false;
				}
				showUserControls();
			}
		}

		private void CB_LineContactorNo_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			if(this.CB_LineContactorNo.SelectedIndex != -1)
			{
				this.currManager.Position = this.CB_LineContactorNo.SelectedIndex;
				this.TB_DW_LC_holdVolts.Refresh();
				this.TB_DW_LC_PullInms.Refresh();
				this.TB_DW_LC_PullInVolts.Refresh();
				VPLineContactor myLC = (VPLineContactor)MAIN_WINDOW.currentProfile.lineContactors[CB_LineContactorNo.SelectedIndex];
				for(int i = 0;i<nodesAL.Count;i++)
				{
					comboSource source = (comboSource) nodesAL[i];
					if(myLC.nodeID == source.enumStr)
					{
						this.CB_LCNodeID.SelectedIndex = i;
						return;
					}
				}
				//if we failed to match any then set combo to'none selected'
				this.CB_LCNodeID.SelectedIndex = -1;
			}
		}
		#endregion SELF_CHARACTERISATION form class

		/// <summary>
		/// Relevant to internal Self char only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MIsaveTestProfile_Click(object sender, System.EventArgs e)
		{
#if EXTERNAL_SELF_CHAR
#else
			#region internal self char only
			saveFileDialog1.FileName = "";  //endsure that we opena t correct directory
			if ( Directory.Exists( MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES" ) == false )
			{
				try
				{
					Directory.CreateDirectory( MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES" );
				}
				catch{}
			}
			saveFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES";
			saveFileDialog1.Title = "Save Data to Self Char Test Profile";
			saveFileDialog1.Filter = "data files (*.xml)|*.xml" ;
			saveFileDialog1.DefaultExt = "xml";
			saveFileDialog1.ShowDialog(this);	
			if(saveFileDialog1.FileName != "")
			{
				ObjectXMLSerializer xmlSerializer = new ObjectXMLSerializer();
				try
				{
					xmlSerializer.Save(this.mySelfChar.scprofile, this.saveFileDialog1.FileName);
				}
				catch(Exception ex)
				{
					Message.Show("Unable to save Self Char Test profile. Ensure file is closed and retry \nError code: " + ex.Message); 
					return;
				}
			}
			#endregion internal self char only
#endif
		}
	}
	/// <summary>
	/// Object used to retreive and store self char data and to control the self char tes tprocess
	/// will replace SCWiz in longer term
	/// </summary>
	public class selfCharClass
	{
		#region Self char data class
		System.Threading.Timer myTimer;
		Thread OLthread = null;
		SCRequestResponseType currTestType = SCRequestResponseType.DCTT_OL_Vd;
		#region parameter defines
		private SystemInfo sysInfo;
		private int nodeIndex;
		private VPMotor motor;
		public  SCprofile scprofile;
		private OLResponse OLresp;
		private OLRequest OLReq;
		private byte [] ReqMessage;
		/// <summary>
		/// This is the point after which we consider idealised curve should have
		//decayed to zero - used to get mean of acutal value in this region
		//which is then an offset to be removed
		/// </summary>
		private float startOfDecayOffset = 0.4F;
		public static float TIME_STEP = 0.0005F;
		private Amebsa amebsa;
		
		#region Open Loop Parameters
		//double  [] J_CL, Tr, Rs, Rr,Lm, Ls, Lr, Lls , Llr;
		int indexK1Pos = 0, indexK1Neg = 0;
		int indexK2Pos = 0, indexK2Neg = 0;  //Indexes made class wide due to useage
		int indexK3Pos = 0, indexK3Neg =0;
		int currIndexK1 = 0, currIndexK2 = 0, currindexK3 = 0;
		double [] expCurve_IValues;
		ArrayList calcKpsId = new ArrayList(), calcKisId = new ArrayList();
		ArrayList calcKpsIq = new ArrayList(), calcKisIq = new ArrayList();
		#endregion Open Loop Parameters

		#region modifed platedata params
		double Vs_rated ;
		double ws_rated;
		double T_rated;

		#endregion modifed platedata params

		#region max operating range params
		double N_max;
		double P_max;
		double T_max;
		#endregion max operating range params

		#region modifed power frame params
		float OL_overcurrent;
		double Vs_max;   // rms phase equivalent
		double Im_rated;
		//maximum Is_qd controller currents
		double Id_max, Iq_max;
		#endregion modifed power frame params
		#endregion parameter defines

		#region constructor
		public selfCharClass(SystemInfo sysInfo, int passed_nodeIndex)
		{
			//user input data will besourced form the current vehicel profile.
			//this profile may be corrected to matcht he current connected truck as appropriate
			//eg is profile ahs two motros but truck only has one
			this.sysInfo = sysInfo;
			nodeIndex = passed_nodeIndex;
			this.scprofile = new SCprofile();
			this.scprofile.applyDefaults(this.sysInfo.nodes[nodeIndex].productVoltage,
				this.sysInfo.nodes[nodeIndex].productCurrent);
			motor = (VPMotor) MAIN_WINDOW.currentProfile.connectedMotors[0]; //judetemp use zero until we have mechanism
		}
			
		#endregion constructor

		#region Open Loop
		public bool openLoopTest()
		{
			setupOLVaLues();
			this.scprofile.applyVoltageOffSets(this.Vs_rated);
			System.Console.Out.Write("\n\nOL Vd Test");
			System.Console.Out.Write("\n----------");
			#region Tx/Rx controller dat on seperate thread
			myTimer = new System.Threading.Timer( new TimerCallback( SCthreadTimerExpired ), null, 250, 250 );
			OLthread = new Thread(new ThreadStart( createAndSendOLRequestMessage )); 
			OLthread.Name = "OL Vd data retrieval";
			OLthread.IsBackground = true;
            OLthread.Priority = ThreadPriority.Normal;
#if DEBUG
			System.Console.Out.WriteLine("Thread: " + OLthread.Name + " started");
#endif
			OLthread.Start(); 
			#endregion Tx/Rx controller dat on seperate thread
			return true; //pass
		}

		private void SCthreadTimerExpired( Object state )
		{
			if(OLthread != null)
			{
				if((OLthread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
					#region Vd
OLthread = null;
					processOLResults();
					#endregion Vd
					#region Vq
					this.currTestType = SCRequestResponseType.DCTT_OL_Vq;
					System.Console.Out.Write("\n\nOL Vq Test");
					System.Console.Out.Write("\n----------");
					createAndSendOLRequestMessage();
					processOLResults();
					#endregion Vq
					#region all OL tests done
					#region calculate mean Kp for both OL tests and check for symetry
					double meanKpId = 0, meanKpIq = 0;
					foreach(double kp in calcKpsId)
					{
						meanKpId +=(kp/(calcKpsId.Count));;
					}
					foreach(double kp in calcKpsIq)
					{
						meanKpIq += (kp/(calcKpsIq.Count));
					}
					if ((meanKpId/meanKpIq < 0.9) || (meanKpId/meanKpIq > 1.1))
					{
						SystemInfo.errorSB.Append("\nError - Kp_d and Kp_q are not symmetric - check wiring");
					}
					#endregion calculate mean Kp for both OL tests and check for symetry
					StringBuilder sb = new StringBuilder();
					sb.Append("\n\nKp results");
					sb.Append("\n---------");
					sb.Append("\nMean Kp for OL Vd test = ");
					sb.Append(meanKpId.ToString());
					sb.Append("\nMean Kp for OL Vq test = ");
					sb.Append(meanKpIq.ToString());
					System.Console.Out.WriteLine(sb.ToString());

					#endregion all tests done
					myTimer = null;
				}
			}
		}
		private void setupOLVaLues()
		{
			StringBuilder sb =new StringBuilder();
			#region modify plate data
			//modify motor name-plate data  as required
			Vs_rated =  (this.motor.platedata.ratedLineVoltageRMS/Math.Sqrt(3));  //jude-checked
			ws_rated =  (2 * Math.PI * this.motor.platedata.ratedFrequencyElectricalHz);//jude - checked
			T_rated = ((this.motor.platedata.ratedPowerkW  * 1000)
				/(this.motor.platedata.ratedSpeedMechanicalRPM*(Math.PI/30))); //jude-checked

			//get maximum operating range data
			N_max = this.motor.platedata.ratedSpeedMechanicalRPM * this.scprofile.general.maxSpeedRatio; //jude-checked
			P_max = this.motor.platedata.ratedPowerkW * 1000 * this.scprofile.general.maxPowerRatio;  //jude-checked
			T_max = T_rated * this.scprofile.general.maxTorqueRatio;  //jude-checked
			#endregion modify plate data

			#region powerframe data
			//judetemp - get this in correct place
			OL_overcurrent = ((float)this.scprofile.openloop.percentOverCurrent + 100)/100; //jude-checked
			Vs_max = (this.scprofile.powerframe.maxBattVolts/Math.Sqrt(6));   // rms phase equivalent jude-checked
			//in DW this.motor.platedata.ratedPowerFactor default is 0.98 not zero
			Im_rated = this.motor.platedata.ratedPhaseCurrentrmsA*(1-this.motor.platedata.ratedPowerFactor); //jude-checked
			//maximum Is_qd controller currents
			Id_max =  (2*Im_rated); //jude-checked
			Iq_max =  Math.Sqrt(Math.Pow(this.scprofile.powerframe.maxIs, 2) 
				- Math.Pow(Id_max,2)); //jude-checked
			#endregion powerframe data

			#region output plate data to console
			sb.Append("\n\nMotor Plate Data");
			sb.Append("\n----------------");
			sb.Append("\nnp (num pole pairs) = ");
			sb.Append(this.motor.platedata.numOfPolePairs.ToString());
			sb.Append("\nppr ( encoder pulses per rev) = ");
			sb.Append(this.motor.platedata.encoderPulsesPerRev);
			sb.Append("\nVs_rated = ");
			sb.Append(Vs_rated.ToString());
			sb.Append("\nIs_rated? (rated stator/phase current) = ");
			sb.Append(this.motor.platedata.ratedPhaseCurrentrmsA.ToString());
			sb.Append("\nws_rated = ");
			sb.Append(ws_rated.ToString());
			sb.Append("\nN_Rated = ");
			sb.Append(this.motor.platedata.ratedSpeedMechanicalRPM.ToString());
			sb.Append(" rpm");
			sb.Append("\nP_rated = ");
			sb.Append((this.motor.platedata.ratedPowerkW * 1000).ToString());
			sb.Append( " Watts");
			sb.Append("\nT_rated = ");
			sb.Append(T_rated.ToString());
			sb.Append(" Nm");
			sb.Append("\nPF_rated = ");
			sb.Append(this.motor.platedata.ratedPowerFactor);
			//DW extra data
			sb.Append("\nDriveWizard extra data:");
			sb.Append("\nN_max  (max spreed/rpm= ");
			sb.Append(N_max.ToString());
			sb.Append("\nP_max = ");
			sb.Append(P_max.ToString());
			sb.Append(" Watts");
			sb.Append("\nT_max = ");
			sb.Append(T_max.ToString());
			sb.Append(" Nm");
			#endregion output plate data to console

			#region output Powerframe data to console
			sb.Append("\n\nPower Frame Data");
			sb.Append("\n----------------");
			sb.Append("\nPercentage permitted over current = ");
			sb.Append (OL_overcurrent.ToString());
			sb.Append("\nVs_max = ");
			sb.Append(Vs_max.ToString());
			sb.Append("\nIs_max = ");
			sb.Append(this.scprofile.powerframe.maxIs);
			sb.Append("\nIm_rated = ");
			sb.Append(Im_rated.ToString());
			sb.Append("\nId_max = ");
			sb.Append(Id_max.ToString());
			sb.Append("\nIq_max = ");
			sb.Append(Iq_max.ToString());
			System.Console.Out.Write(sb.ToString());
			#endregion output Powerframe data to console
		}

		private void createAndSendOLRequestMessage()
		{
			if(this.currTestType == SCRequestResponseType.DCTT_OL_Vd)
			{//create new request object in controller readable format
				OLReq = new OLRequest(currTestType, this.scprofile.openloop, Id_max);
			}
			else 
			{
				OLReq = new OLRequest(currTestType, this.scprofile.openloop, Iq_max);
			}
			//stuff the data intoa byte array for transmission across the CANbus
			ReqMessage = OLReq.convertToByteArrayForTx();
			#region send request and wait for reply
			/* AJK, 27/07/05 
			 * Change to block downloads for quicker speed.  If failed (ie get an abort code) then revert
			 * to old method for backwards compatibility.
			*/
			getOLDataFromController();
			#endregion send request and wait foor reply
		}
		private void getOLDataFromController()
		{
			DIFeedbackCode feedback;
			//attempt block transfer
			ODItemData testReqSub = this.sysInfo.nodes[this.nodeIndex].getODSubFromObjectType(SevconObjectType.SELFCHAR_TESTREQUEST, 0);
			ODItemData testStatusSub = this.sysInfo.nodes[nodeIndex].getODSubFromObjectType(SevconObjectType.SELFCHAR_TESTSTATUS, 0);
			ODItemData testResponseSub = this.sysInfo.nodes[nodeIndex].getODSubFromObjectType(SevconObjectType.SELFCHAR_TESTRESPONSE,0);

			feedback = this.sysInfo.nodes[this.nodeIndex].writeODValueBlock(testReqSub, ReqMessage );
			if ( feedback != DIFeedbackCode.DISuccess )
			{
				//if controller can't do block transfer - then use segmented message
				System.Console.Out.WriteLine("\nTransmitting OL Request");
				feedback = this.sysInfo.nodes[nodeIndex].writeODValue(testReqSub, ReqMessage);
			}
			if(feedback != DIFeedbackCode.DISuccess)
			{
				Message.Show(feedback.ToString());
			}
			else
			{
				if((testStatusSub != null) && (testResponseSub != null))
				{
					long newVal = 0xffff;
					while(newVal == 0xffff)
					{
						feedback = this.sysInfo.nodes[nodeIndex].readODValue(testStatusSub);
						newVal = testStatusSub.currentValue;
						if(newVal == 0) //we have data?
						{
							if(testResponseSub != null)
							{
								feedback = this.sysInfo.nodes[nodeIndex].readODValue(testResponseSub);
								if(feedback == DIFeedbackCode.DISuccess)
								{
									System.Console.Out.WriteLine("Reading OL response");
									OLresp = new OLResponse(testResponseSub.currentValueDomain, this.scprofile.openloop.testPoints, this.scprofile.openloop.numTestIterations);
								}
							}
						}
						else if(newVal != 0xffff)
						{
							SystemInfo.errorSB.Append("\nController reported error: Ox" + newVal.ToString("X").PadLeft(4,'0'));;
						}
					}
				}
			}
		}
		private void removeOffsetFromOLResults(ArrayList results, int startIndex, int TestSetIndex)
		{
			#region waveform diagrams
			//1. Applied Voltage waveforn
			//units are 500us sample periods  [] show TestPoint indexes
			// <- 12 ->[0]
			//  ______
			// |      |      [1]
			// |      |<- 90->     <--   120 ---->
			// |      |_______      ______________[3]
			//                |    |
			//                |    |
			//                |____|[2]
			//				  <-12->

			//returned Current waveform - K points show for +ve and -ve pulses
			//    
			//   /|K1 = first point after peak 		
			// |/ <\-60%->|K3 = 60% of time between K1 and K2 - nominal decay to zero point - used to calculate Id and Iq offset to be applied
			// |    \~~~~~~~~~K2 point that occurs at end of applied poistive cylce    
			//                \  /~~~~~~~~~~~~~~~~~
			//                 \ |<--60%-->K3      K2
			//                  \|K1   
			//			         \
			#endregion waveform diagrams
			PointF K1Pos, K2Pos, K3Pos, K1Neg, K2Neg, K3Neg;
			double decayOffset,  cumulativeVal;
			#region calculate end indexes of positive pulse and whole test set
			float noOfSamplesInPositivePulse = 
				((PointF)this.scprofile.openloop.testPoints[0 + TestSetIndex]).X
				+ ((PointF)this.scprofile.openloop.testPoints[1 + TestSetIndex]).X;
			float numSamplesInThisTestSet =
				((PointF)this.scprofile.openloop.testPoints[0 + TestSetIndex]).X
				+ ((PointF)this.scprofile.openloop.testPoints[1 + TestSetIndex]).X
				+ ((PointF)this.scprofile.openloop.testPoints[2 + TestSetIndex]).X
				+ ((PointF)this.scprofile.openloop.testPoints[3 + TestSetIndex]).X;
			#endregion calculate end indexes of positive pulse and whole test set
			#region calculate K1 points and indexes
			float previousPositivePeak = 0, previousNegativePeak = 0;
			for(int resultsPtr = startIndex;resultsPtr<(startIndex + numSamplesInThisTestSet);resultsPtr++)
			{  //Are we less than the response point corresponing to switch over on the input?
				PointF pt = (PointF) results[resultsPtr];
				if(resultsPtr<startIndex + (int)(noOfSamplesInPositivePulse/this.OLresp.decimationRate))
				{
					#region positive pulse
					if(pt.Y>= previousPositivePeak)
					{
						previousPositivePeak = pt.Y;
						if(resultsPtr < results.Count-1)
						{
							indexK1Pos = resultsPtr +1; //Ki is first point after the peak ie first point in the 'exponential' decay
							K1Pos = (PointF) results[indexK1Pos]; //grap the K1 X,Y point
						}
					}
					#endregion positive pulse
				}
				else 
				{
					#region negative pulse
					if(pt.Y<=previousNegativePeak)  //note less than = this is a negative peak
					{
						previousNegativePeak = pt.Y;
						if(resultsPtr < results.Count-1)
						{
							indexK1Neg = resultsPtr+1; //index afetr peak
							K1Neg = (PointF) results[indexK1Neg]; //and its corresponding X,Y point
						}
					}
					#endregion negative pulse
				}
			}
			#endregion calculate K1 points and indexes 
			#region calculate K2 points and indexes 
			indexK2Pos = startIndex + (int)(noOfSamplesInPositivePulse/this.OLresp.decimationRate)-1;//minus 1 coz it is an ArrayList index
			K2Pos = (PointF) results[indexK2Pos];
			indexK2Neg = startIndex + (int)(numSamplesInThisTestSet/this.OLresp.decimationRate)-1;  //minus 1 coz it is an ArrayList index
			K2Neg = (PointF) results[indexK2Neg];
			#endregion calculate K2 points and indexes 
			#region calculate K3 points and indexes
			indexK3Pos =(int) Math.Floor((indexK2Pos - (this.startOfDecayOffset *(indexK2Pos - indexK1Pos))));
			K3Pos = (PointF) results[indexK3Pos];
			indexK3Neg =(int)  (indexK2Neg - (this.startOfDecayOffset *(indexK2Neg - indexK1Neg)));
			K3Neg = (PointF) results[indexK3Neg];
			#endregion calculate K3 points and indexes
			#region calculate and apply the Positive Decay Offset
			#region calculate offset to apply
			cumulativeVal = 0;
			for(int i = indexK3Pos;i< indexK2Pos;i++)
			{  //cast back to PointF and add on the Y value
				cumulativeVal += ((PointF) results[i]).Y;
			}
			//offset is the total divides by number of samples in this part of the waveform
			decayOffset = cumulativeVal/(indexK2Pos - indexK3Pos);
			#endregion claculate offset to apply
			#region apply offset
			for(int i = indexK1Pos;i<=indexK2Pos;i++)  //move positve results Down by calcuated offset
			{
				PointF beforePt = (PointF) results[i];
				PointF afterPt = new PointF(beforePt.X, beforePt.Y-(float)decayOffset);
				results.RemoveAt(i);
				results.Insert(i,afterPt);
			}
			#endregion apply offset
			#endregion calculate and apply the Positive Decay Offset for Id
			#region calculate and apply the Negative Decay Offset
			#region calculate offset to apply
			cumulativeVal = 0;
			for(int i = indexK3Neg;i<=indexK2Neg;i++)
			{  //cast back to PointF and add on the Y value
				cumulativeVal += ((PointF) results[i]).Y;
			}
			//offset is the total divides by number of samples in this part of the waveform
			decayOffset = cumulativeVal/(indexK2Neg - indexK3Neg);
			#endregion claculate offset to apply
			#region apply offset
			//indexK2Pos is last sample in positve pulse so add one for first sample in the negative pulse
			for(int i=indexK1Neg;i<=indexK2Neg;i++)  //move negate results Up by calcuated offset
			{//minus the offset - is is minus a minus ie plus
				PointF beforePt = (PointF) results[i];
				PointF afterPt = new PointF(beforePt.X, beforePt.Y-(float)decayOffset);  
				results.RemoveAt(i);
				results.Insert(i,afterPt);
			}
			#endregion apply offset
			#endregion calculate and apply the Positive Decay Offset for Id
		}
		private bool processOLResults()
		{
			StringBuilder sb;
			const double EXPFIT_TOL  = 0.04;
			const double KP_ADJUSTMENT  = 0.5;
			double [] x0 = new double[2];
			ArrayList expPoints = new ArrayList();
			ArrayList ResultsBeforeOffsetRemoval =  new ArrayList();
			ArrayList ResultsAfterOffsetRemoval = new ArrayList();
			expCurve_IValues = new double[this.OLresp.IdResultPoints.Count];
			ArrayList [] seriesToPlot;
			string [] plotTraceNamesForLegend;
			Form graph;
			 //force this to be multiple of 4 in property
			this.currIndexK1 = 0;//ensure reset i.e. before Iq starts
			this.currIndexK2 = 0;//ensure reset
			this.currindexK3 = 0;  //ensure reset
			amebsa = new Amebsa();  //from referenced dll
			amebsa.Iteration = 2000; //iter in SCWiz - iter is set to fixed #define MAXITS (2000)
			amebsa.Best = 1.0e9;  //yb in SCWiz
			int arrayPtr = 0;
			seriesToPlot = new ArrayList[5];
			plotTraceNamesForLegend = new string[5];
			int startIndex = 0;
			if(this.currTestType == SCRequestResponseType.DCTT_OL_Vd)
			{
				#region OL Vd
				#region grab copy of Id before we take away the offset - just for graph
				foreach (PointF pt in this.OLresp.IdResultPoints)
				{
					ResultsBeforeOffsetRemoval.Add(pt); //we need to copy by value NOT ref here to kep before and after points
				}
				#endregion grab copy of Id before we tkae away the offset
				for(int currIteration = 0;currIteration<this.OLReq.numReapeats;currIteration++)
				{
					System.Console.Out.WriteLine("\n-----------------------");
					System.Console.Out.WriteLine("Id: Test iteration number " + (currIteration+1).ToString() + " of " + this.OLReq.numReapeats.ToString());
					//each test set encompasses a positve and negative applied pulse
					int numTestSets = (this.scprofile.openloop.testPoints.Count/4);
					for(int currTestSet = 0;currTestSet<numTestSets;currTestSet++)//for each testset
					{
						//add exp zero points - for points prior to K1
						removeOffsetFromOLResults(this.OLresp.IdResultPoints, startIndex,(currTestSet*4) ); //now remove the offset
						int numSamplesInThisTestSet = (int)
							(((PointF)this.scprofile.openloop.testPoints[0 + (currTestSet*4)]).X
							+ ((PointF)this.scprofile.openloop.testPoints[1 + (currTestSet*4)]).X
							+ ((PointF)this.scprofile.openloop.testPoints[2 + (currTestSet*4)]).X
							+ ((PointF)this.scprofile.openloop.testPoints[3 + (currTestSet*4)]).X);

						#region take copy of results after offset removal - for graphing only
						for(int i =startIndex;i<startIndex + numSamplesInThisTestSet;i++)
						{
							PointF pt = (PointF) this.OLresp.IdResultPoints[i];
							ResultsAfterOffsetRemoval.Add(pt); //we need to copy by value NOT ref here to kep before and after points
						}
						#endregion take copy of results after offset removal - for graphing only
						#region Write K vclaues to console
						System.Console.Out.WriteLine("Positive: K1 index = " + indexK1Pos.ToString()
							+ ", K2 index  = " + indexK2Pos.ToString() 
							+ ", K3 index = " + indexK3Pos);
						System.Console.Out.WriteLine("Negative: K1 index = " + indexK1Neg.ToString()
							+ ", K2 index  = " + indexK2Neg.ToString() 
							+ ", K3 index = " + indexK3Neg);
						#endregion Write K vclaues to console
						#region fit exponential to positive results of this test set
						//set current K points to positive area - to keep funcId, funcIq signatures intact
						currIndexK1 = this.indexK1Pos;
						currIndexK2 = this.indexK2Pos;
						currindexK3 = this.indexK3Pos;
						x0[0] = ((PointF)this.OLresp.IdResultPoints[indexK1Pos]).Y;//fx[0]; =  fx[i-k1] = (double)(command->u.uutresponse.uut_response[6+2*i])/16.0f - id0;
						x0[1] = 10 * TIME_STEP;//10*TSAMPLE;
						double positiveCostFunc = fitExponential(x0);
						#endregion fit exponential to positive results of this test set

						#region fit exponential to negative results
						currIndexK1 = this.indexK1Neg;
						currIndexK2 = this.indexK2Neg;
						currindexK3 = this.indexK3Neg;
						x0[0] = ((PointF)this.OLresp.IdResultPoints[indexK1Neg]).Y;//fx[0]; =  fx[i-k1] = (double)(command->u.uutresponse.uut_response[6+2*i])/16.0f - id0;
						x0[1] = 10 * TIME_STEP;//10*10*TSAMPLE;
						double negativeCostFunc = fitExponential(x0);
						#endregion fit exponential to negative results

						#region check results
						//if both cost functions are within tolerance then we will calculate a Kp and Ki for this test
						double higheCF = Math.Max(positiveCostFunc,negativeCostFunc);
#if DEBUG
						System.Console.Out.WriteLine("Higher cost funct (J0) = " + higheCF.ToString());
#endif
						if ( (Math.Max(positiveCostFunc,negativeCostFunc)) 
							< (Math.Pow((EXPFIT_TOL * scprofile.powerframe.maxIs),2)) )
						{
							#region calculate the positve and negative reactances
							//divide applied volts by resulting current at point K1
							//Z01 in SCWiz
							float inputVolts = ((PointF)this.scprofile.openloop.testPoints[currTestSet * 4]).Y;
							float outputCurrent = ((PointF) this.OLresp.IdResultPoints[this.indexK1Pos]).Y;
							double reactancePos = inputVolts/outputCurrent;
							//calculate reactance
							float neginputVolts = ((PointF)this.scprofile.openloop.testPoints[(currTestSet * 4) + 2]).Y;
							float negoutputCurrent = ((PointF) this.OLresp.IdResultPoints[this.indexK1Neg]).Y;
							double reactanceNeg = -1 * (neginputVolts/negoutputCurrent);  //judetemp = bit naugthy
							//reactanceNeg *= -1;
							#endregion calculate the positve and negative reactances
							#region send results to console
							sb = new StringBuilder();
							sb.Append("\nThis.scprofile.powerframe.maxIs = ");
							sb.Append(this.scprofile.powerframe.maxIs.ToString());
							sb.Append("\nVs_max = ");
							sb.Append(Vs_max.ToString());
							sb.Append("\npositve reactance (Z01) = ");
							sb.Append(reactancePos.ToString());
							sb.Append("\nnegative reactance (Z02)  = ");
							sb.Append(reactanceNeg.ToString());
#if DEBUG
							System.Console.Out.Write(sb.ToString());
#endif
							#endregion send results to console
							#region calculate Kp and Ki from this test set and append to Kp,Ki ArrayLists
							double Kp =  ((this.scprofile.powerframe.maxIs/Vs_max) 
								* (reactancePos+reactanceNeg)/2.0) * KP_ADJUSTMENT;
							if(Kp <0)
							{
								// error - gain wrong sign
#if DEBUG
								System.Console.Out.WriteLine("\nError - Kp is negative - check wiring");
//judetemp								return false;
#endif
							}
#if DEBUG
							System.Console.Out.WriteLine("\nThis Kp = " + Kp.ToString());
#endif
							calcKpsId.Add(Kp);  //OK so add to list
							double Ki = Kp * (TIME_STEP / (10 * TIME_STEP));
#if DEBUG
							System.Console.Out.WriteLine("This Ki = " + Ki.ToString());
#endif
							calcKisId.Add(Ki);
							#endregion calculate Kp and Ki from this test set and append to Kp,Ki ArrayLists
						}
						else
						{
							System.Console.Out.WriteLine("\nError - cannot fit data to exponential");// error - cannot fit data
//judetemp							return false;						
						}
						#endregion check results

						startIndex = startIndex + (int)numSamplesInThisTestSet;
					}
				}
				#region create ArrayList of points form calculated best fit
				for(int i = 0;i<this.OLresp.IdResultPoints.Count;i++)
				{
					float xcoord = ((PointF) (this.OLresp.IdResultPoints[i])).X;
					expPoints.Add(new PointF(xcoord,(float)expCurve_IValues[i]));
				}
				#endregion create ArrayList of points form calculated best fit
				#region graph results at end of OL
				System.Console.Out.WriteLine("Graphing results");
				seriesToPlot[arrayPtr] =this.OLresp.TestPointsAppliedByDSP;
				plotTraceNamesForLegend[arrayPtr++] = "Applied OL Vd (not to scale)";

				seriesToPlot[arrayPtr] = ResultsBeforeOffsetRemoval;
				plotTraceNamesForLegend[arrayPtr++] = "Id (measured)";

				seriesToPlot[arrayPtr] = ResultsAfterOffsetRemoval;
				plotTraceNamesForLegend[arrayPtr++] = "Id (offset removed)";

				seriesToPlot[arrayPtr] = expPoints;
				plotTraceNamesForLegend[arrayPtr++] = "Id (Best fit exponential)";

				seriesToPlot[arrayPtr] = OLresp.IqResultPoints;
				plotTraceNamesForLegend[arrayPtr++] = "Iq (measured)";

				graph = new DriveWizard.DATA_DISPLAY_WINDOW(seriesToPlot, plotTraceNamesForLegend, GraphTypes.SELF_CHAR_OLVd);
				graph.ShowDialog();

				#endregion graph results at end of OL
				#endregion OL Vd
			}
			else if(this.currTestType == SCRequestResponseType.DCTT_OL_Vq)
			{
				#region grab copy of Iq before we tkae away the offset
				foreach (PointF pt in this.OLresp.IqResultPoints)
				{
					ResultsBeforeOffsetRemoval.Add(pt); //we need to copy by value NOT ref here to kep before and after points
				}
				#endregion grab copy of Id before we tkae away the offset
				for(int currIteration = 0;currIteration<this.OLReq.numReapeats;currIteration++)
				{//each test set encompasses a positve and negative applied pulse
					System.Console.Out.WriteLine("\n-----------------------");
					System.Console.Out.WriteLine("Iq: Test iteration number " + (currIteration+1).ToString() + " of " + this.OLReq.numReapeats.ToString());
					int numTestSets = (this.scprofile.openloop.testPoints.Count/4);
					for(int currTestSet = 0;currTestSet<numTestSets;currTestSet++)//for each testset
					{
						removeOffsetFromOLResults(this.OLresp.IqResultPoints, startIndex, (currTestSet*4));
						int numSamplesInThisTestSet = (int)
							(((PointF)this.scprofile.openloop.testPoints[0 + (currTestSet*4)]).X
							+ ((PointF)this.scprofile.openloop.testPoints[1 + (currTestSet*4)]).X
							+ ((PointF)this.scprofile.openloop.testPoints[2 + (currTestSet*4)]).X
							+ ((PointF)this.scprofile.openloop.testPoints[3 + (currTestSet*4)]).X);

						#region take copy of results after offset removal - for graphing only
						for(int i =startIndex;i<startIndex + numSamplesInThisTestSet;i++)
						{
							PointF pt = (PointF) this.OLresp.IqResultPoints[i];
							ResultsAfterOffsetRemoval.Add(pt); //we need to copy by value NOT ref here to kep before and after points
						}
						#endregion take copy of results after offset removal - for graphing only
						#region write K values to console
						System.Console.Out.WriteLine("Positive: K1 index = " + indexK1Pos.ToString()
							+ ", K2 index  = " + indexK2Pos.ToString() 
							+ ", K3 index = " + indexK3Pos);
						System.Console.Out.WriteLine("Negative: K1 index = " + indexK1Neg.ToString()
							+ ", K2 index  = " + indexK2Neg.ToString() 
							+ ", K3 index = " + indexK3Neg);
						#endregion write K values to console
						#region fit exponential to positive results
						//set current K points to positive area - to keep funcId, funcIq signatures intact
						currIndexK1 = this.indexK1Pos;
						currIndexK2 = this.indexK2Pos;
						currindexK3 = this.indexK3Pos;
						x0[0] = ((PointF)this.OLresp.IqResultPoints[indexK1Pos]).Y;//fx[0]; =  fx[i-k1] = (double)(command->u.uutresponse.uut_response[6+2*i])/16.0f - id0;
						x0[1] = 10 * TIME_STEP;//10*TSAMPLE;
						double positiveCostFunc = fitExponential(x0);
						#endregion fit exponential to positive results

						#region fit exponential to negative results
						currIndexK1 = this.indexK1Neg;
						currIndexK2 = this.indexK2Neg;
						currindexK3 = this.indexK3Neg;
						x0[0] = ((PointF)this.OLresp.IqResultPoints[indexK1Neg]).Y;//fx[0]; =  fx[i-k1] = (double)(command->u.uutresponse.uut_response[6+2*i])/16.0f - id0;
						x0[1] = 10 * TIME_STEP;
						double negativeCostFunc = fitExponential(x0);
						#endregion fit exponential to positive results

						#region check results
						System.Console.Out.WriteLine("Higher cost funct (J0) = " + Math.Max(positiveCostFunc,negativeCostFunc).ToString());
						//if both cost functions are within tolerance then we will calculate a Kp and Ki for this test
						if ( (Math.Max(positiveCostFunc,negativeCostFunc)) 
							< (Math.Pow((EXPFIT_TOL * scprofile.powerframe.maxIs),2)) )
						{
							#region calculate the positve and negative reactances
							//divide applied volts by resulting current at point K1
							//Z01 in SCWiz
							float inputVolts = ((PointF)this.scprofile.openloop.testPoints[currTestSet * 4]).Y;
							float outputCurrent = ((PointF) this.OLresp.IqResultPoints[this.indexK1Pos]).Y;
							double reactancePos = inputVolts/outputCurrent;
							//calculate reactance
							float neginputVolts = ((PointF)this.scprofile.openloop.testPoints[(currTestSet * 4) + 2]).Y; //add 2 for fist test point in negative [ulse
							float negoutputCurrent = ((PointF) this.OLresp.IqResultPoints[this.indexK1Neg]).Y;
							double reactanceNeg = -1*(neginputVolts/negoutputCurrent);  //judetemp = bit naugthy
							#endregion calculate the positve and negative reactances
							#region send results to console
							sb = new StringBuilder();
							sb.Append("\nThis.scprofile.powerframe.maxIs = ");
							sb.Append(this.scprofile.powerframe.maxIs.ToString());
							sb.Append("\nVs_max = ");
							sb.Append(Vs_max.ToString());
							sb.Append("\npositve reactance (Z01) = ");
							sb.Append(reactancePos.ToString());
							sb.Append("\nnegative reactance (Z02)  = ");
							sb.Append(reactanceNeg.ToString());
							System.Console.Out.Write(sb.ToString());
							#endregion send results to console

							#region calculate Kp and Ki from this test set and append to Kp,Ki ArrayLists
							double Kp =  ((this.scprofile.powerframe.maxIs/Vs_max) 
								* (reactancePos+reactanceNeg)/2.0) * KP_ADJUSTMENT;
							if(Kp <0)
							{
								// error - gain wrong sign
								System.Console.Out.WriteLine("Error - Iq Kp is negative - check wiring");
//judetemp								return false;
							}
							System.Console.Out.WriteLine("\nThis Kp = " + Kp.ToString());
							calcKpsIq.Add(Kp);  //OK so add to list
							double Ki = Kp * (TIME_STEP / (10 * TIME_STEP));//Ts1[OL_NumIterationsLeft] = this what SCWiz sets it to
							System.Console.Out.WriteLine("This Ki = " + Ki.ToString());
							calcKisIq.Add(Ki);
							#endregion calculate Kp and Ki from this test set and append to Kp,Ki ArrayLists
						}
						else
						{
							System.Console.Out.WriteLine("Error - cannot fit data to exponential");// error - cannot fit data
//judetemp							return false;						
						}
						#endregion check results
						startIndex = startIndex + (int)numSamplesInThisTestSet;
					}
				}
				#region create ArrayList of points form calculated best fit
				for(int i = 0;i<this.OLresp.IqResultPoints.Count;i++)
				{
					float xcoord = ((PointF) (this.OLresp.IqResultPoints[i])).X;
					expPoints.Add(new PointF(xcoord,(float)expCurve_IValues[i])); 
				}
				#endregion create ArrayList of points form calculated best fit

				#region graph results at end of OL
				System.Console.Out.WriteLine("Graphing results");
				seriesToPlot[arrayPtr] =this.OLresp.TestPointsAppliedByDSP;
				plotTraceNamesForLegend[arrayPtr++] = "Applied OL Vq (not to scale)";

				seriesToPlot[arrayPtr] = ResultsBeforeOffsetRemoval;
				plotTraceNamesForLegend[arrayPtr++] = "Iq (measured)";

				seriesToPlot[arrayPtr] = ResultsAfterOffsetRemoval;
				plotTraceNamesForLegend[arrayPtr++] = "Iq (offset removed)";

				seriesToPlot[arrayPtr] = expPoints;
				plotTraceNamesForLegend[arrayPtr++] = "Iq (Best fit exponential)";

				seriesToPlot[arrayPtr] = OLresp.IdResultPoints;
				plotTraceNamesForLegend[arrayPtr++] = "Id (measured)";

				graph = new DriveWizard.DATA_DISPLAY_WINDOW(seriesToPlot, plotTraceNamesForLegend, GraphTypes.SELF_CHAR_OLVq);
				graph.ShowDialog();
				#endregion graph results at end of OL
			}
			return true;
		}
		private double fitExponential(double [] x0)
		{
			#region parmeters required by the universal standard amebsa method
			//ndim is 
			const int ndim = 2;
			//temptr is annealing temperature - not relevant(?) to what we are doing and so is set to zero in SCWiz
			const double temptr = 0;
			#endregion parmeters required by the universal standard amebsa method
			const double ftol = 1.0e-9;
			double dx;
			double[,] p = new double[3,2];//= matrix(1,ndim+1,1,ndim);
			double [] pb = {1,ndim}; //was vector in Scwiz
			double [] y = new double[3];//{1,ndim+1};  //was vector in SCwiz - need 3 for for loop?
			/* starting conditions */
			int randMax = 0x7FFF;  //RAND_MAX is defined as this in SCWiz - appears on intellisense tooltip Jude
			#region from scWiz
			/* starting conditions */
			dx = 0.3;
			//			p[1][1] = x0[0]*(1 - dx*rand()/RAND_MAX);  p[1][2] = x0[1]*(1 - dx*rand()/RAND_MAX);
			//			p[2][1] = x0[0]*(1 - dx*rand()/RAND_MAX);  p[2][2] = x0[1]*(1 + dx*rand()/RAND_MAX);
			//			p[3][1] = x0[0]*(1 + dx*rand()/RAND_MAX);  p[3][2] = x0[1]*(1 + dx*rand()/RAND_MAX);

			//note
			//			x0[0] = ((PointF)this.OLresp.IdResultPoints[indexK1Pos]).Y;//fx[0]; =  fx[i-k1] = (double)(command->u.uutresponse.uut_response[6+2*i])/16.0f - id0;
			//			x0[1] = 10 * TIME_STEP;//10*10*TSAMPLE;
			#endregion from scWiz
			Random myRandom = new Random();
			p[0,0] = x0[0]* (1 - dx*myRandom.NextDouble()/randMax);
			p[0,1] = x0[1]* (1 - dx*myRandom.NextDouble()/randMax);
			p[1,0] = x0[0]* (1 - dx*myRandom.NextDouble()/randMax);
			p[1,1] = x0[1]* (1 + dx*myRandom.NextDouble()/randMax);
			p[2,0] = x0[0]* (1 + dx*myRandom.NextDouble()/randMax);
			p[2,1] = x0[1]* (1 + dx*myRandom.NextDouble()/randMax);

			/* Get 3 values for least mean squares */
			for (int i = 0; i < ndim+1; i++)
			{
				double [] temp = {p[i,0], p[i,1]};  //needed due to differenct in array syntax beteen C and C#
				//C# has two distinct multidimensional array types
				y[i] = funcId(temp);
			}

			NumericalRecipes.Delegates.FunctionDoubleAToDouble myFuncId 
				= new NumericalRecipes.Delegates.FunctionDoubleAToDouble(funcId);
			NumericalRecipes.Delegates.FunctionDoubleAToDouble myFuncIq 
				= new NumericalRecipes.Delegates.FunctionDoubleAToDouble(funcIq);

			#region feedbakc to console
			StringBuilder sb =new StringBuilder();
			sb.Append("\nBeforeAmebsa: Starting Simplex algo ... ");
			sb.Append(", x0[0] = ");
			sb.Append(x0[0].ToString());
			sb.Append(", x0[1] = ");
			sb.Append(x0[1].ToString());
			System.Console.Out.WriteLine(sb.ToString());
			#endregion feedbakc to console
			//the SCWiz amebsa function is :amebsa(p, y, ndim, pb, &yb, ftol, func, &iter, 0);
			if(this.currTestType == SCRequestResponseType.DCTT_OL_Vd)
			{
				amebsa.amebsa(p, y, ndim, pb, ftol, myFuncId, temptr);
			}
			else
			{
				amebsa.amebsa(p, y, ndim, pb, ftol, myFuncIq, temptr);
			}

			#region feedback to console
			sb =new StringBuilder();
			sb.Append("AfterAmebsa: iter = ");
			sb.Append(2000-amebsa.Iteration).ToString();
			sb.Append(", yb = ");
			sb.Append(amebsa.Best.ToString());
			sb.Append(", pb[0] = ");
			sb.Append(pb[0].ToString());
			sb.Append(", pb[1] = ");
			sb.Append(pb[1].ToString());
			System.Console.Out.WriteLine(sb.ToString());
			#endregion feedback to console

		// save best values
			x0[0] = pb[0];
			x0[1] = pb[0];

			// return cost function
			return amebsa.Best /(this.currIndexK2 - this.currIndexK1);
		}

		private double funcId(double [] x)
		{
			#region from SCWIz
			//			int i;
			//			double J, err;
			//			J = 0;
			//			for (i = 0; i < npts; i++)
			//			{
			//				expCurve_IValues[i] = x[1]*exp(-(i+1)*d*TSAMPLE/x[2]);
			//				err = fx[i] - expCurve_IValues[i];
			//				J += err * err;
			//			}
			#endregion from SCWiz
			double J = 0;
			//we are only interested in the decay part of the curve
			for (int i = this.currIndexK1; i<=this.currIndexK2; i++)
			{
				expCurve_IValues[i] = x[0]* Math.Exp(-(i-this.currIndexK1+1)*(OLresp.decimationRate*TIME_STEP)/x[1]);
				double err = ((PointF)this.OLresp.IdResultPoints[i]).Y - expCurve_IValues[i];
				J += Math.Pow(err,2); //square the error 
			}
			return J;
		}

		private double funcIq(double [] x)
		{
			double J = 0;
			//we are only interested in the decay part of the curve
			for (int i = this.currIndexK1; i<=this.currIndexK2; i++)
			{
				expCurve_IValues[i] = x[0]* Math.Exp(-(i-this.currIndexK1+1)*(OLresp.decimationRate*TIME_STEP)/x[1]);
				double err = ((PointF)this.OLresp.IqResultPoints[i]).Y - expCurve_IValues[i];
				J += Math.Pow(err,2); //square the error 
			}
			return J;
		}

		#endregion Open Loop

		#region Closed Loop
		private void setupCLValues()
		{
			//modify a copy of the generic CL test opoints for this controller
			//store resulting data for use in calculations
			ArrayList CLTestData = new ArrayList();
			for(int CLtstPtr = 0;CLtstPtr<this.scprofile.closedloop.CLtests.Count;CLtstPtr++)
			{
				SCProf_ClosedLoop.SCProf_CLtest srcTest =  (SCProf_ClosedLoop.SCProf_CLtest)(this.scprofile.closedloop.CLtests[CLtstPtr]);
				ArrayList temp  = new ArrayList();
				for(int j = 0;j<srcTest.testpoints.Count;j++)
				{
					PointF genericTestPoint = (PointF)srcTest.testpoints[j];
					genericTestPoint.Y = (float) (genericTestPoint.Y * this.motor.platedata.ratedPhaseCurrentrmsA);
					//.X or time is unmodified
					temp.Add(genericTestPoint);
				}
				CLTestData.Add(temp);
			}
		}
		#endregion Closed Loop
		//this test is performed second
		#region No Load Test
		private void setUpNLTVlaues()
		{
			/* NLT parameters */
			//				/* ws rate limit to prevent pull-out */
			
			
			double ws_rated = (2 * Math.PI * this.motor.platedata.ratedFrequencyElectricalHz);
			double ws_rate =  this.scprofile.noloadtest.w_rate * ws_rated;
			//				nlt_nsam = command->u.start.test_data.nlt.nsamples;
			//				nlt_settle1 = command->u.start.test_data.nlt.settle1;
			//				nlt_settle2 = command->u.start.test_data.nlt.settle2;
			//				nlt_maxIdq_ratio = command->u.start.test_data.nlt.maxIdq_ratio;
			//				nws = command->u.start.test_data.nlt.Npts;
			//				ws[0] = command->u.start.test_data.nlt.ws1 * ws_rated;
			//				ws[1] = command->u.start.test_data.nlt.ws2 * ws_rated;
			//				ws[2] = command->u.start.test_data.nlt.ws3 * ws_rated;
			//				ws[3] = command->u.start.test_data.nlt.ws4 * ws_rated;
			//				ws[4] = command->u.start.test_data.nlt.ws5 * ws_rated;
			//				ws[5] = command->u.start.test_data.nlt.ws6 * ws_rated;
			//				ws[6] = command->u.start.test_data.nlt.ws7 * ws_rated;
			//				ws[7] = command->u.start.test_data.nlt.ws8 * ws_rated;
			//
		}
		public bool noLoadTest()
		{
			Message.Show("Performing Internal No Load Test.\nNext to continue");
			return true; //assume passed for now
		}
		#endregion No Load Test
		//this test is performed last
		public bool closedLoopTest()
		{
			Message.Show("Performing Internal Closed loop Test.\nNext to continue");
			return true; //assume passed for now
		}
		#endregion Self char data class

		#region Self char Test Request and Response message formats
		#region OL messages
		public class OLRequest
		{
			public SCRequestResponseType RequestType;
			private ushort requestLength;
			private uint checksum = 0;
			private ushort checksum16;
			private ushort numPointInWaveForm;
			public ushort numReapeats;
			private ushort IMax;  //Id or Iq as appropiriate
			public ArrayList TxdTestPoints;  //just a list of floats Volts interleaved with time for transmission to controller

			public OLRequest(SCRequestResponseType ReqType, DriveWizard.SCProf_OpenLoop OLProfile, double passed_Imax)
			{
				this.RequestType = ReqType;
				checksum += (ushort) RequestType;
				this.requestLength = System.Convert.ToUInt16(4 + (OLProfile.testPoints.Count*2) );
				checksum += this.requestLength;
				this.numPointInWaveForm = (ushort) (OLProfile.numTestPointApplications * System.Convert.ToUInt16(OLProfile.testPoints.Count)); 
				checksum += this.numPointInWaveForm;
				this.numReapeats = OLProfile.numTestIterations;
				checksum += this.numReapeats;
				this.IMax = (ushort)Math.Floor(passed_Imax*16);//Id_max converted to 12.4 format 
				checksum += this.IMax;
				TxdTestPoints = new ArrayList();
				for(int i = 0;i<OLProfile.numTestPointApplications;i++)
				{
					foreach(PointF OLTestPoint in OLProfile.testPoints)
					{
						ushort volts = (ushort) (OLTestPoint.Y * 256);  //volts needs to bein 12:4 format
						checksum += volts;
						ushort numSamples =  (ushort)(OLTestPoint.X);      /* Ns [1LSB=500us] */
						checksum += numSamples;
						TxdTestPoints.Add(volts);
						TxdTestPoints.Add(numSamples);
					}
				}
				checksum16 = (ushort) checksum;//System.Convert.ToUInt16(checksum % 0x1000);  //0x32620
			}
			public byte [] convertToByteArrayForTx()
			{
				byte [] msg = new byte[(7 + this.TxdTestPoints.Count) * 2]; //twice as many bytes as ushorts
				int bytePtr = 0;

				msg[bytePtr++] = (byte) 0;//(this.RequestType>>8);  //all possible values are less than 8
				msg[bytePtr++] = (byte) this.RequestType;

				msg[bytePtr++] =  (byte) (this.requestLength>>8);
				msg[bytePtr++] = (byte) this.requestLength;
			
				msg[bytePtr++] = (byte) (this.numPointInWaveForm>>8);
				msg[bytePtr++] = (byte) this.numPointInWaveForm;

				msg[bytePtr++] = (byte) (this.numReapeats>>8);
				msg[bytePtr++] = (byte) this.numReapeats;

				msg[bytePtr++] = (byte) (this.IMax>>8);
				msg[bytePtr++] = (byte) this.IMax;

				bytePtr+= 2; //step over RFU

				for (int i = 0;i<this.TxdTestPoints.Count;i++)
				{
					ushort testpoint = (ushort) this.TxdTestPoints[i];
					msg[bytePtr++] = (byte)(testpoint>>8);
					msg[bytePtr++] = (byte) testpoint;
				}
				msg[bytePtr++] = (byte) (this.checksum16>>8);
				msg[bytePtr++] = (byte) this.checksum16;
				return msg;
			}


		}
		public class OLResponse
		{
			public bool responseError = false;
			public ushort responseType;
			private ushort responseLength;
			public ushort resultCode;
			public ushort numPointsInWaveForm;
			public ushort decimationRate;
			public ArrayList IdResultPoints;
			public ArrayList IqResultPoints;
			public ArrayList TestPointsAppliedByDSP;
			public ArrayList cornerPts;
			public OLResponse(byte [] rawResp, ArrayList OLpulseWaveform, int numRepeats)
			{
				TestPointsAppliedByDSP = new ArrayList();
				cornerPts = new ArrayList();
				int btePtr = 0;

				this.responseType = rawResp[btePtr++];	//auusume upper byte in first
				this.responseType = (ushort)((responseType<<8) + rawResp[btePtr++]);

				#region unused parts of response
				//response length should equal rawResp.Length
				//validation not required - we can just ignore this
				this.responseLength =  rawResp[btePtr++];	
				this.responseLength = (ushort)((responseLength<<8) + rawResp[btePtr++]);

				//should be zero if results are OK
				this.resultCode =  rawResp[btePtr++];	
				this.resultCode = (ushort)((resultCode<<8) + rawResp[btePtr++]);

				//this is the same as OLReq.numPointInWaveForm -> agin no rela use to use here
				this.numPointsInWaveForm = rawResp[btePtr++];	//auusume upper byte in first
				this.numPointsInWaveForm = (ushort)((numPointsInWaveForm<<8) + rawResp[btePtr++]);
				#endregion unused parts of response

				#region decimation rate
				this.decimationRate = rawResp[btePtr++];	//auusume upper byte in first
				this.decimationRate = (ushort)((decimationRate<<8) + rawResp[btePtr++]);
				if(this.decimationRate == 0) //prevent divide by zero
				{
					this.responseError = true;
					return;
				}
				#endregion decimation rate
				#region RFU
				btePtr +=2; //skip over RFU
				#endregion RFU

				float actualSamplePeriod = TIME_STEP * this.decimationRate;
				IdResultPoints = new ArrayList();
				IqResultPoints = new ArrayList();
				int sampleCounter = 0;
				bool forcingResults = true;
				if(forcingResults == false)
				{
					#region real measured results
					while (btePtr<rawResp.Length) //note NO checksum
						//for(int i = btePtr;i< rawResp.Length;i++)
					{
						//on each OL test Id and Iq results are interleaved
						//we expect one to show response curve and other to behave itself nicely
						#region convert bytes into Id (float) and add to Id ArrayList
						ushort Idint = rawResp[btePtr++];	//auusume upper byte in first
						Idint = (ushort)((Idint<<8) + rawResp[btePtr++]);
						float IdF = (Idint/16); //convert back from 12:4 format
						this.IdResultPoints.Add(new PointF((sampleCounter * actualSamplePeriod),IdF));
						//this.IdResultPoints.Add(new PointF((sampleCounter * actualSamplePeriod),10)); //judetemp - force value
						#endregion convert bytes into Id (float) and add to Id ArrayList

						#region convert bytes into Iq (float)and add to Iq ArrayList
						ushort Iqint  = rawResp[btePtr++];	//auusume upper byte in first
						Iqint = (ushort)((Iqint<<8) + rawResp[btePtr++]);
						float IqF = (Iqint /16); //convert back from 12:4 format
						this.IqResultPoints.Add(new PointF((sampleCounter * actualSamplePeriod),IqF));
						//this.IqResultPoints.Add(new PointF((sampleCounter * actualSamplePeriod),-10)); //judetemp - force value
						#endregion convert bytes into Iq (float)and add to Iq ArrayList
						sampleCounter++;  //4 received bytes is equal to one pair of samples ( Id and Iq)
					}

					//Note there is NO checksum - spec is wrong
					//units are 500us sample periods
					// <- 12 ->
					//  ______
					// |      |
					// |      |<- 90 ->    <--   120 ---->
					// |      |_______      ______________
					//                |    |
					//                |    |
					//                |____|
					//				  <-12->
					#endregion real measured results
				}
				else //we are using known results from real motor connected
				{
					#region forced results
					SCRequestResponseType respType = (SCRequestResponseType)responseType;
					string ForcedIdfilePath = "";
					if(respType == SCRequestResponseType.DCTT_OL_Vd)
					{
						ForcedIdfilePath = MAIN_WINDOW.ApplicationDirectoryPath + @"\SelfChar\forcedOL_IdtestTResults140206.txt";
					}
					else
					{
						ForcedIdfilePath = MAIN_WINDOW.ApplicationDirectoryPath + @"\SelfChar\forcedOL_IqtestTResults140206.txt";
					}
					FileStream fs = new FileStream( ForcedIdfilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read );
					StreamReader sr = new StreamReader( fs );
					string input;
					sampleCounter = 0;
					while ( ( input = sr.ReadLine() ) != null )
					{
						#region read file
						input = input.Trim();
						if(input == "")
						{
							continue; //ignore empty lines
						}
						else
						{
							//get the sample num - we dont use this
							string[] results = input.Split('\t');
							float Idf = System.Convert.ToSingle(results[1])/16f;
							this.IdResultPoints.Add(new PointF((sampleCounter * actualSamplePeriod),Idf));
							float Iqf = System.Convert.ToSingle(results[2])/16f;
							this.IqResultPoints.Add(new PointF((sampleCounter * actualSamplePeriod),Iqf));
							sampleCounter++;
						}
						#endregion read file
					}
					#region close the stream reader and file stream reader
					// Close the stream reader if it was opened.
					if ( sr != null )
					{
						sr.Close();
					}

					// Close the file stream if it was opened.
					if ( fs != null )
					{
						fs.Close();
					}
					#endregion
					#endregion forced results
				}
				int numSamplesSoFar = 0;
				for(int iter = 0;iter<numRepeats; iter++)
				{
					foreach(PointF inputPt in OLpulseWaveform)  //from test profile
					{
						//get number of samples in this part of applied pulse
						//use ceiling - otherwise we endup one less than we want
						int numSamplesInThisPt = (int) (double)(inputPt.X/this.decimationRate);  
						for(int i=numSamplesSoFar;i<(numSamplesInThisPt + numSamplesSoFar);i++)
						{ //addeach 'sample' point to the extrapolated ArrayList
							PointF pt = new PointF( (float)(i * 0.0005F * this.decimationRate), inputPt.Y);
							TestPointsAppliedByDSP.Add(pt); 
						}
						numSamplesSoFar += numSamplesInThisPt; //start point for next batch
					}
				}

			}
		}

		#endregion OL messages

		#region CL messages
		public class CLRequest
		{
			public SCRequestResponseType RequestType;
//			private ushort requestLength;
//			private uint checksum = 0;
//			private ushort checksum16;
//			private ushort numPointInWaveForm;
//			private ushort numReapeats;
//			private ushort Kp; //Fixed point 6.10
//			private ushort Ki; //Fixed point 10.6 
//			ArrayList testPoints;

			public CLRequest()
			{
			}
		}
		public class CLResponse
		{
			public bool responseError = false;
			public ushort responseType;
//			private ushort responseLength;

			public ushort resultCode;
			public ushort numPointsInWaveForm;
			public ushort decimationRate;
			public ArrayList VdResults;
			public ArrayList IdResults;

			public CLResponse()
			{
			}
		}
		#endregion CL messages

		#region NLT messages
		public class NLTStartRequest
		{
			public SCRequestResponseType RequestType;
//			private ushort requestLength;
//			private uint checksum = 0;
//			private ushort checksum16;
//
//			private ushort w_rate; //fixed point 8.8
//			private ushort Vlimit; //fixed poing signed 8.8
//			private ushort wm_settling_spec1;
//			private ushort wm_settling_spec2;
//			private ushort Nsamples;
			public NLTStartRequest()
			{
			}
		}
		public class NLTStartResponse
		{
			public NLTStartResponse()
			{
			}
		}
		public class NLT_wRequest
		{
			public SCRequestResponseType RequestType;
//			private ushort requestLength;
//			private uint checksum = 0;
//			private ushort checksum16;
//
//			private ushort numPointsInWaveform;
//			private ushort omega;
			public ArrayList fluxLevels;
			public NLT_wRequest()
			{
			}
		}
		public class NLT_OmegaResponse
		{
			public NLT_OmegaResponse()
			{

				//TODO
			}
		}
		public class NLT_stopRequest
		{
			public SCRequestResponseType RequestType;
//			private ushort requestLength;
//			private uint checksum = 0;
//			private ushort checksum16;

			//nothing else except RFU
			public NLT_stopRequest()
			{
			}
		}
		public class NLT_stopResponse
		{
			public NLT_stopResponse()
			{
			}
		}
		#endregion NLT messages
		#endregion Self char Test Request and Response message formats
	}
	
}


