/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.80$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/09/2008 23:17:10$
	$ModDate:23/09/2008 11:12:16$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
    This window displays a single Sevcon node’s active fault log, allowing the user 
    to view and reset logs dependent on their access level. It allows whole logs to 
    get reset, all faults up to a specified logging level to get reset, or individual 
    faults user-selected faults to get reset. The user can also configure fault groups 
    to define what faults are recorded in the log in future.
 * 
REFERENCES    

MODIFICATION HISTORY
    $Log:  36739: FAULT_LOG_WINDOW.cs 

   Rev 1.80    23/09/2008 23:17:10  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.79    17/03/2008 13:13:52  jw
 Logs null obejct handling complete. Still needs progrees bar steps linking to
 odSub timeout values


   Rev 1.78    14/03/2008 10:58:50  jw
 Log handling revised, Read of Log releated CAN items now done from GUI ( to
 allow later DI /Ixxat unlinking) . Processing of recevied logs code and event
 lists moved to sysInfo - reduces memory usage ( one method per application
 rather than per node)  Some newer Sevocn devices do not hav ethese logs. Also
 event list methods now have product code input - to allow us to have seperate
 Event lists for eg displays etc. Note:some error hanlding eg null detection
 still needed but check back in for working set with DI 


   Rev 1.77    13/03/2008 08:46:46  jw
 Some common ErrorSB handling  tasks moved form GUI files to sysInfo to reduce
 code size and complexity


   Rev 1.76    12/03/2008 13:43:52  ak
 All DI threads have ThreadPriority increased to Normal from BelowNormal
 (needed when running VCI3)


   Rev 1.75    11/03/2008 09:37:04  jw
 All CAN data transfer now done via single seperate thread for ease of
 disconnecting feedback path later. 
 Controller change. Active fualts can now only be reset by the controlle r(CEH
 confirmed) . User controls/code modified to suit


   Rev 1.74    15/01/2008 12:30:04  ak
 User feedback on log retrieval failures improved.


   Rev 1.73    05/12/2007 22:13:12  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Diagnostics;
using System.Text;

namespace DriveWizard
{
	#region enumerated type declarations
	public enum FIFOCol {Name, ID, Time, DB1, DB2, DB3, Group, Level};
	public enum ActiveFaults {Name, ID, Group, Level, clearActiveFault};
	public enum GroupFilters {Group, GroupNo, saveEventsInGroup};
	public enum FaultWindowStates {ACTIVE, FAULTLOG, FAULTFILTERS, NONE};
	#endregion enumerated type declarations

	#region FAULT_LOG_WINDOW class
	/// <summary>
	/// Summary description for FAULT_LOG_WINDOW class
	/// This class is responsible for user interface to the active faults log, and the Fault Log
	/// Level of user interaction is dependent on the users access level and extends to
	/// viewing, and resetting whole logs. Reseting individual active faults either by selection of fualts 
	/// or by resetting all faults up to a specified logging level. The user can configure which fault groups 
	/// should be recorded in the fault log
	/// </summary>
	public class FAULT_LOG_WINDOW : System.Windows.Forms.Form
	{
		#region controls declarations
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.ComboBox tableSelectCombo;
		private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.StatusBar statusBar1;
        private ToolBar toolbar = null;
		#endregion

		#region my declarations
		private DataView activeView, faultView, filterView;
		public static FIFOTable faultlogTable;
		public static ActiveFaultsTable activeTable = null;
		private FIFOGroupFilterTable groupfilterTable;
		private SystemInfo sysInfo = null;
        private nodeInfo node = null;
		private string selectedNodeText = "";
		private FIFOLogEntry [] faultLog = new FIFOLogEntry[0];
		private NodeFaultEntry [] activelog = new NodeFaultEntry[0];
		#region threads
        private Thread DIThread;
		#endregion threads
		private string [] tablechoices = {"Active Faults", "Fault Log", "Group Filtering"};
		public static bool [] activeResetArray;
		public static bool [] faultFiltersArray;
		public static FaultWindowStates faultWindowState = FaultWindowStates.NONE;
        private ArrayList unknownFaultIDs = new ArrayList(); //DR38000269 one list of all unknown IDs maintained
        private ushort numGroupFilters = 16;
        private const int ProgressBarTimeoutMax = 251;
		//used to extract relevent bits from event IDs
		private int groupIDMask = 0x03C0, logLevelMask = 0x1C00;
		FIFOGroupFilterTableStyle TSgroupfilter = null;
		actFaultTableStyle TSactivelog  = null;
		FIFOTableStyle TSfaultlog  = null;
		float [] filterPercents = {0.5F, 0.2F, 0.3F};
		float [] activePercents = {0.5F, 0.15F, 0.2F, 0.15F};
		private System.Windows.Forms.ProgressBar progressBar1;
		float [] faultlogPercents = {0.3F, 0.1F, 0.13F, 0.07F, 0.07F, 0.07F, 0.15F, 0.11F};
		string [] faultLevels = {"0: None","1: Warning", "2: Drv inhibit", "3: Severe", "4: V. Severe",  "5: RtB"};
		int dataGrid1DefaultHeight = 0;
        #region OD objects
        ODItemData hoursSub = null;
        ODItemData minsSecsSub = null;
        ODItemData resetFaultLogSub = null;
        ODItemData fltLogfilterSub = null;
        ODItemData faultLogSub = null;
        ODItemData activeFaultListSub = null;
        ODItemData eventIDsSub = null;
        ODItemData eventNameSub = null;
        #endregion OD objects

        UInt16 Groupfilter = 0;

        ///<summary>delagate for DI wrapper methods for single start thread method</summary>
        private delegate void wrapperDelegate();

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.closeBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.submitBtn = new System.Windows.Forms.Button();
            this.tableSelectCombo = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGrid1
            // 
            this.dataGrid1.AllowNavigation = false;
            this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dataGrid1.DataMember = "";
            this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Location = new System.Drawing.Point(8, 48);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.ParentRowsVisible = false;
            this.dataGrid1.PreferredColumnWidth = 125;
            this.dataGrid1.ReadOnly = true;
            this.dataGrid1.RowHeadersVisible = false;
            this.dataGrid1.Size = new System.Drawing.Size(1035, 501);
            this.dataGrid1.TabIndex = 2;
            this.dataGrid1.Resize += new System.EventHandler(this.dataGrid1_Resize);
            this.dataGrid1.Click += new System.EventHandler(this.dataGrid1_Click);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem9});
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 0;
            this.menuItem9.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
            this.menuItem9.Text = "&File";
            // 
            // closeBtn
            // 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Location = new System.Drawing.Point(923, 565);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(120, 25);
            this.closeBtn.TabIndex = 5;
            this.closeBtn.Text = "&Close window";
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1027, 25);
            this.label1.TabIndex = 6;
            // 
            // timer1
            // 
            this.timer1.Interval = 200;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // submitBtn
            // 
            this.submitBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.submitBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.submitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.submitBtn.Location = new System.Drawing.Point(8, 617);
            this.submitBtn.Name = "submitBtn";
            this.submitBtn.Size = new System.Drawing.Size(216, 25);
            this.submitBtn.TabIndex = 9;
            this.submitBtn.Text = "&Submit";
            this.submitBtn.Visible = false;
            this.submitBtn.Click += new System.EventHandler(this.submitResetBtn_Click);
            // 
            // tableSelectCombo
            // 
            this.tableSelectCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tableSelectCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tableSelectCombo.Enabled = false;
            this.tableSelectCombo.Location = new System.Drawing.Point(8, 557);
            this.tableSelectCombo.Name = "tableSelectCombo";
            this.tableSelectCombo.Size = new System.Drawing.Size(216, 24);
            this.tableSelectCombo.TabIndex = 11;
            this.tableSelectCombo.SelectionChangeCommitted += new System.EventHandler(this.tableSelectCombo_SelectionChangeCommitted);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(851, 617);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(192, 24);
            this.label2.TabIndex = 12;
            this.label2.Text = "Key time:  00:00:00";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label2.Visible = false;
            // 
            // timer2
            // 
            this.timer2.Enabled = true;
            this.timer2.Interval = 200;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(0, 649);
            this.progressBar1.Maximum = 0;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1056, 24);
            this.progressBar1.TabIndex = 15;
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 666);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(1056, 24);
            this.statusBar1.TabIndex = 16;
            // 
            // FAULT_LOG_WINDOW
            // 
            this.AcceptButton = this.submitBtn;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(1056, 690);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tableSelectCombo);
            this.Controls.Add(this.submitBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.dataGrid1);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Menu = this.mainMenu1;
            this.MinimumSize = new System.Drawing.Size(450, 316);
            this.Name = "FAULT_LOG_WINDOW";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Fault Log";
            this.Load += new System.EventHandler(this.FAULT_LOG_WINDOW_Load);
            this.Closed += new System.EventHandler(this.FAULT_LOG_WINDOW_Closed);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.FAULT_LOG_WINDOW_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
/// <summary>
///		/*--------------------------------------------------------------------------
///		 *  Name			: FAULT_LOG_WINDOW
///		 *  Description     : Constructor
///		 *  Parameters      : reference to the (single) SystemInfo Object, 
///		 *					  the Node Number and descritive text of the current node
///		 *  Used Variables  : 
///		 *  Preconditions   :  
///		 *  Post-conditions : 
///		 *  Return value    : n/a
///		 *--------------------------------------------------------------------------*/
///		 </summary>
		public FAULT_LOG_WINDOW(SystemInfo passed_systemInfo, int nodeNum, string nodeText, ToolBar passed_toolbar)
		{
			InitializeComponent();  //autogenerated do not modify
			sysInfo = passed_systemInfo; //local reference 
            this.node = sysInfo.nodes[nodeNum]; 
			selectedNodeText = nodeText;
			this.sysInfo.formatDataGrid(this.dataGrid1);
			tableSelectCombo.DataSource = tablechoices;
			this.toolbar = passed_toolbar;
			this.statusBar1.Text = "Retreiving fault log information...";
			activeTable = new ActiveFaultsTable();
			faultlogTable = new FIFOTable();
			groupfilterTable = new FIFOGroupFilterTable();
			this.progressBar1.Maximum = 0;			// maximum is not known until first SDO response is received

            #region get OD items
            MAIN_WINDOW.appendErrorInfo = false;
            hoursSub = node.getODSubFromObjectType(SevconObjectType.CONTROLLER_HOURS,0x1);
            minsSecsSub = node.getODSubFromObjectType(SevconObjectType.CONTROLLER_HOURS, 0x2);
            fltLogfilterSub = node.getODSubFromObjectType(SevconObjectType.FAULTS_FIFO_CTRL, 0x03);
            resetFaultLogSub = node.getODSubFromObjectType(SevconObjectType.FAULTS_FIFO_CTRL, 0x01);
            faultLogSub = node.getODSubFromObjectType(SevconObjectType.DOMAIN_UPLOAD, SCCorpStyle.FaultsFIFOSubObject);
            activeFaultListSub = node.getODSubFromObjectType(SevconObjectType.DOMAIN_UPLOAD,SCCorpStyle.ActiveFaultListSubObject);
            eventIDsSub = node.getODSubFromObjectType(SevconObjectType.EVENT_ID_DESCRIPTION, SCCorpStyle.EventIDSubObject);
            eventNameSub = node.getODSubFromObjectType(SevconObjectType.EVENT_ID_DESCRIPTION, SCCorpStyle.EventNameSubObject);
            MAIN_WINDOW.appendErrorInfo = true;
            #endregion get OD items

            #region button colouring
            foreach (Control myControl in this.Controls)
			{
				if((myControl.GetType().ToString()) == "System.Windows.Forms.Button")
				{
					myControl.BackColor = SCCorpStyle.buttonBackGround;
				}
			}
			#endregion button colouring

		}
/// <summary>
///		/*--------------------------------------------------------------------------
///		 *  Name			: FAULT_LOG_WINDOW_Load
///		 *  Description     : Event Handler for window On laod event
///		 *  Parameters      : event arguments
///		 *  Used Variables  : 
///		 *  Preconditions   :  
///		 *  Post-conditions : 
///		 *  Return value    : none
///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void FAULT_LOG_WINDOW_Load(object sender, System.EventArgs e)
		{
			dataGrid1DefaultHeight = this.dataGrid1.Height;
            createActiveLogTable(); //will change to souce data from DI
            createGroupFilterTable();
            createFaultLogTable(); //will change to souce data from DI
            createActiveTableStyle();
            createFaultlogTableStyle();
            createFilterTableStyle();
            if (this.activeFaultListSub != null)
            {
                this.statusBar1.Text = "Getting Active Log Log";
                this.startDIThread("requestActiveLogWrapper", requestActiveLogWrapper);
            }
            else
            {
                faultWindowState = FaultWindowStates.ACTIVE;
                this.showUserControls();
//                showActiveLog();
            }
        }

        #endregion initialisation

        #region DI Thread timer
        /// <summary>
        ///		/*--------------------------------------------------------------------------
        ///		 *  Name			: Timer 1 elapsed event handler
        ///		 *  Description     : Used for monitoring the initial dat agathering form DI which is on 
        ///		 *					  a seperate faultLogDataRetrievalThread  to allow Window to be  displayed 
        ///		 *					  whilst this faultLogDataRetrievalThread is still running
        ///		 *  Parameters      : Event args
        ///		 *  Used Variables  : 
        ///		 *  Preconditions   :  
        ///		 *  Post-conditions : 
        ///		 *  Return value    : none
        ///		 *--------------------------------------------------------------------------*/
        /// </summary>
        private void timer1_Tick(object sender, System.EventArgs e)
        {
            if (DIThread != null)
            {
                if ((DIThread.ThreadState & System.Threading.ThreadState.Stopped) > 0)
                {
                    int i = 0;
                    #region DI Thread complete
                    timer1.Enabled = false; //kill timer first
                    this.progressBar1.Value = this.progressBar1.Maximum;  //for aesthetics
                    this.progressBar1.Visible = false;
                    this.statusBar1.Text = "";
                    string threadName = DIThread.Name;
                    DIThread = null;
                    switch (threadName)
                    {
                        case "requestActiveLogWrapper":
                            {
                                #region get Active Log
                                sysInfo.insertErrorType("Failed to read Active Faults");
                                sysInfo.processActivefaultLog(this.activeFaultListSub, out activelog, out unknownFaultIDs); //DR38000269
                                faultWindowState = FaultWindowStates.ACTIVE;
                                //DR38000269 If we can read missing events and we have some - then get them
                                if ((this.eventIDsSub != null) && (this.eventNameSub != null) && (unknownFaultIDs.Count > 0))
                                {
                                    this.statusBar1.Text = "Getting missing event descriptions";
                                    this.startDIThread("getMissingEventDescWrapper", getMissingEventDescWrapper);
                                }
                                else
                                {
                                    showUserControls();
                                }
                                #endregion get Active Log
                                break;
                            }

                        case "getGroupFiltersWrapper":
                            #region get Group filters
                            {
                                sysInfo.insertErrorType("Failed to read group filters");
                                showUserControls();
                                break;
                            }
                            #endregion get Group filters

                        case "requestFaultLogWrapper":
                            #region Get fault log
                            {
                                sysInfo.processFIFOLog(this.faultLogSub, out faultLog, out unknownFaultIDs); //DR38000269
                                #region get the text descriptor associated with this eventID number
                                //DR38000269 If we can read missing events and we have some - then get them
                                if ((this.eventIDsSub != null) && (this.eventNameSub != null) && (unknownFaultIDs.Count > 0))
                                {
                                    this.statusBar1.Text = "Getting missing event descriptions";
                                    this.startDIThread("getMissingEventDescWrapper", getMissingEventDescWrapper);
                                }
                                else
                                {
                                    showUserControls();
                                }
                                #endregion
                                break;
                            }
                            #endregion Get fault log

                        case "resetFaultLogWrapper":
                            #region reset fault log
                            {
                                this.startDIThread("requestFaultLogWrapper", requestFaultLogWrapper);
                                break;
                            }
                            #endregion reset fault log

                        case "setGroupFiltersWrapper":
                            #region Update filters
                            sysInfo.insertErrorType("Failed to update group filters");
                            this.statusBar1.Text = "Getting Group filters";
                            this.startDIThread("getGroupFiltersWrapper", getGroupFiltersWrapper);
                            break;
                            #endregion Update filters

                        //DR38000269 write & readMissingEventDescWrapper replaced with single getMissingEventDescWrapper
                        //which retrieves ALL unknownFaultIDs in one single DI thread
                        case "getMissingEventDescWrapper":
                            #region read missing description

                            if (faultWindowState == FaultWindowStates.ACTIVE)
                            {
                                #region getting event name for active fault
                                //DR38000269 re-process active fault log now that new descriptors have been read
                                sysInfo.processActivefaultLog(this.activeFaultListSub, out activelog, out unknownFaultIDs);
                                showUserControls();
                                #endregion getting event name for active fault
                            }
                            else
                            {
                                #region getting event name for fault log fault
                                {
                                    //DR38000269 re-process the log now new descriptors have been read
                                    sysInfo.processFIFOLog(this.faultLogSub, out faultLog, out unknownFaultIDs);
                                    this.fillFaultTable();
                                    #region future stretch potential
                                    //and then switch them back on again 
                                    //faultView.ListChanged += new System.ComponentModel.ListChangedEventHandler(faultViewChangeHandler);
                                    //faultlogTable.ColumnChanged += new System.Data.DataColumnChangeEventHandler(faultLogTableChangeHandler);
                                    #endregion future stretch potential
                                    showUserControls();
                                }
                                #endregion getting event name for fault log fault
                            }
                            break;
                            #endregion read missing description

                        case "getKeyHoursWrapper":
                            if (SystemInfo.errorSB.Length == 0)
                            {
                                this.startDIThread("getKeyMinsSecsWrapper", getKeyMinsSecsWrapper);
                            }
                            else
                            {
                                label2.Text = "Key time: not available";
                                this.statusBar1.Text = "Failed to Read Key hours";
                                //switch our user feedback window back on

                            }
                            break;

                        case "getKeyMinsSecsWrapper":
                            if (SystemInfo.errorSB.Length == 0)
                            {
                                // only bother to build the key hours if all the data has been retrieved OK
                                int myMins = (int)(((int)minsSecsSub.currentValue) * 15);
                                int mins = (int)(myMins / 60);
                                int secs = (int)(myMins % 60);
                                #region build + display time string
                                StringBuilder ksb = new StringBuilder();
                                ksb.Append("Key time: ");
                                ksb.Append(hoursSub.currentValue.ToString());
                                ksb.Append(":");
                                ksb.Append(mins.ToString("00"));
                                ksb.Append(".");
                                ksb.Append(secs.ToString("00"));
                                label2.Text = ksb.ToString();
                                #endregion build time string
                            }
                            else
                            {
                                label2.Text = "Key time: not available";
                                sysInfo.clearErrorSB();
                                this.statusBar1.Text = "Failed to Read Key Minutes and Seconds";
                            }

                            //switch our user feedback window back on
                            break;

                    }

                    #endregion DI Thread complete
                }
                else
                {
                    #region DI Thread running - update progress bar
                    if (this.sysInfo.CANcomms.SDOReadDomainRxDataPtr == 0)
                    {
                        progressBar1.Maximum = ProgressBarTimeoutMax;
                        if (this.progressBar1.Value < progressBar1.Maximum)
                        {
                            progressBar1.Value++;
                        }
                    }
                    else
                    {
                        if ((this.progressBar1.Maximum != 0) && (this.progressBar1.Maximum != ProgressBarTimeoutMax))
                        {
                            //increment progress bar
                            if (this.sysInfo.CANcomms.SDOReadDomainRxDataPtr >= this.progressBar1.Maximum)
                            {
                                this.progressBar1.Value = this.progressBar1.Minimum;
                                this.progressBar1.Maximum = 0;
                            }
                            else
                            {
                                this.progressBar1.Value = this.sysInfo.CANcomms.SDOReadDomainRxDataPtr;
                            }
                        }
                        else
                        {
                            this.progressBar1.Maximum = (int)Math.Min(0x7fffffff, this.sysInfo.CANcomms.SDOReadDomainRxDataSize);
                        }
                    }
                    #endregion DI Thread running - update progress bar
                }
            }
        }

        #endregion DI Thread timer

        #region Start Thread Method
        //We are moving towards putting all data read/writes on a seperate thread.
        //This will allow us to 'disconnect' the VCI Received/Transmit handlers form the upper layers
        //So instead of the DI waiting for responses using while loops the resposne can be checked in the GUI
        //Timer which can also be used for timeout cdetection. When complete should improve responsiveness
        // and provide permant soluion to programming issues. Also allows better control of user feedback

        private void startDIThread(string threadName, wrapperDelegate delegateWrapper)
        {
            #region start request DIThread
            this.progressBar1.Value = this.progressBar1.Minimum;
            this.progressBar1.Visible = true;
            DIThread = new Thread(new ThreadStart(delegateWrapper));
            DIThread.Name = threadName;
            DIThread.IsBackground = true;
            DIThread.Priority = ThreadPriority.Normal;
#if DEBUG
            System.Console.Out.WriteLine("Thread: " + DIThread.Name + " started");
#endif
            DIThread.Start();
            timer1.Enabled = true;
            #endregion
        }

        #endregion Start Thread Method

        #region DI Thread wrappers

        /// <summary>
        ///		/*--------------------------------------------------------------------------
        ///		 *  Name			: requestActiveLogWrapper
        ///		 *  Description     : Requests the specifed log form the controller via DI interface
        ///		 *  Parameters      : 
        ///		 *  Used Variables  : 
        ///		 *  Preconditions   :  
        ///		 *  Post-conditions : 
        ///		 *  Return value    : none
        ///		 *--------------------------------------------------------------------------*/
        /// </summary>
        /// 
        private void requestActiveLogWrapper()
        {
            node.readODValue(this.activeFaultListSub);
        }

        /// <summary>
        ///		/*--------------------------------------------------------------------------
        ///		 *  Name			: requestFaultLogWrapper
        ///		 *  Description     : Requests the specifed log form the controller via DI interface. 
        ///		 *					  This process is threaded and so this method acts as a wrapper 
        ///		 *					  for the inner method which requires parameters.
        ///		 *  Parameters      : none
        ///		 *  Used Variables  : 
        ///		 *  Preconditions   :  
        ///		 *  Post-conditions : 
        ///		 *  Return value    : none
        ///		 *--------------------------------------------------------------------------*/
        /// </summary>
        private void requestFaultLogWrapper()
        {
            //DR38000259 Clear out the currentValue from the faultLogSub prior to re-reading this.
            //If the log has been reset & is empty, reading it returns a CANGeneralError with the
            //reason "<Unable to read Sevcon abort code>" which is normal for an empty log.
            //However, the old currentValueDomain value remains causing DW to erroneously still
            //display the previous log values (making it look like the log wasn't reset).
            faultLogSub.currentValueDomain = null;
            DIFeedbackCode feedback = node.readODValue(this.faultLogSub);

            if (feedback != DIFeedbackCode.DISuccess) //DR38000260 additional error reporting
            {
                SystemInfo.errorSB.Append("\nUnable to retreive fault log for node ID " + node.nodeID.ToString());
            }
        }

        private void getGroupFiltersWrapper()
        {
            node.readODValue(fltLogfilterSub);
        }

        private void setGroupFiltersWrapper()
        {
            node.writeODValue(fltLogfilterSub, Groupfilter);
        }

        private void resetFaultLogWrapper()
        {
            node.writeODValue(resetFaultLogSub, 0x01);
        }

        private void getKeyHoursWrapper()
        {
            node.readODValue(hoursSub);
        }
        private void getKeyMinsSecsWrapper()
        {
            node.readODValue(minsSecsSub);
        }

        // DR38000269 read & writeMissingEventDescWrapper replaced with getMissingEventDescWrapper()
        // means all unknownFaultIDs can be retrieved on a single DI thread together
        private void getMissingEventDescWrapper()
        {
            DIFeedbackCode feedback;
            for (int i = 0; i < unknownFaultIDs.Count; i++)
            {
                ushort faultID = ((ushort)unknownFaultIDs[i]);
                feedback = node.writeODValue(eventIDsSub, (long)faultID); //write fault ID

                if (feedback == DIFeedbackCode.DISuccess)
                {
                    feedback = node.readODValue(eventNameSub); //read back text string

                    if (feedback == DIFeedbackCode.DISuccess) // update overall maintained event list
                    {
                        this.sysInfo.updateEventList(faultID, eventNameSub.currentValueString, node.productCode);
                    }
                }

                if (feedback == DIFeedbackCode.DINoResponseFromController)
                {
                    break;      // bomb out of loop if controller not responding
                }
            }
        }
        #endregion DI Thread wrappers

		#region Fault log methods

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: createFaultLogTable
		///		 *  Description     : Creates and fills the Fault log table
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void createFaultLogTable()
		{
			faultView = new DataView(faultlogTable);
			fillFaultTable();
			faultView.AllowNew = false; //prevents empty row at bottom of datagrid = which can casue array index out of bounds exceptions
			faultView.ListChanged += new System.ComponentModel.ListChangedEventHandler(faultViewChangeHandler);
			faultlogTable.ColumnChanged += new System.Data.DataColumnChangeEventHandler(faultLogTableChangeHandler);
		}

		private void fillFaultTable()
		{
            faultlogTable.Clear();
			DataRow row;
			foreach ( FIFOLogEntry entry in faultLog )
			{
				int minsAndSecs, mins, secs;
				int ID = 0;
				row = faultlogTable.NewRow();
				row[(int) (FIFOCol.Name)] = entry.description;
				row[(int) (FIFOCol.ID)] = getEventNo(entry.eventID);
				#region time column
				minsAndSecs = (int)(((int)entry.minsAndSecs) * 15);
				mins = minsAndSecs / 60;
				secs = minsAndSecs % 60;
				row[(int) (FIFOCol.Time)] = entry.hours.ToString("000000") + ":" + mins.ToString( "00" ) + "." + secs.ToString( "00" );
				#endregion
				#region databyte columns
				row[(int) (FIFOCol.DB1)] = getDataByte(entry.db1);
				row[(int) (FIFOCol.DB2)] = getDataByte(entry.db2);
				row[(int) (FIFOCol.DB3)] = getDataByte(entry.db3);
				#endregion
				row[(int) (FIFOCol.Group)] = getGroupName(entry.eventID);
				#region logging level column
				ID = entry.eventID;
				int levelInt = (int) ((ID & logLevelMask)>> 10);
				if(levelInt< this.faultLevels.Length)
				{
					row[(int) (FIFOCol.Level)] = this.faultLevels[levelInt];//(int) ((ID & logLevelMask)>> 10);
				}
				else
				{
					SystemInfo.errorSB.Append("\nUnrecognised fault level: " + levelInt.ToString() + "\nPlease report");
					row[(int) (FIFOCol.Level)] = levelInt.ToString();
				}
				#endregion
				faultlogTable.Rows.Add(row);
			}
			faultlogTable.AcceptChanges();
			faultView.Sort = FIFOCol.Time.ToString() + " DESC";
		}

		private void faultLogTableChangeHandler(object sender, System.Data.DataColumnChangeEventArgs e)
		{
			//no action required at present
			//will be called in response to user changing any fault log vlaues
			//Leave in for future stretch potential
		}
		private void faultViewChangeHandler(object sender,ListChangedEventArgs e )
		{
			//no action required at present
			//will be called when ever this table view is changed eg filter applied or 
			//datagrid is sorted. Leave in in for future stretch potential
		}
		#endregion Fault log

		#region active Log

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: createActiveLogTable
		///		 *  Description     : Creates and fills the Fault log table
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void createActiveLogTable()
		{
			fillActiveTable();
			activeView = new DataView(activeTable);
			activeResetArray = new bool[activeView.Count];
			for(ushort i = 0;i<activeView.Count;i++)
				{
					activeResetArray[i] = false;
				}
			activeView.AllowNew = false; //prevents empty row at bottom of datagrid = which can cause array index out of bounds exceptions
			activeView.ListChanged += new System.ComponentModel.ListChangedEventHandler(activeViewChangeHandler);
			activeTable.ColumnChanged += new System.Data.DataColumnChangeEventHandler(activeTableChangeHandler);
		}
		private void fillActiveTable()
		{
			DataRow row;
			int ID = 0;
			for(int i = 0;i<activelog.Length;i++)
			{
				row = activeTable.NewRow();

				row[(int) (ActiveFaults.Name)] = activelog[i].description;
				#region event ID
                row[(int)(ActiveFaults.ID)] = getEventNo(activelog[i].eventID);
				#endregion event ID
                row[(int)(ActiveFaults.Group)] = getGroupName(activelog[i].eventID);
				row[(int) (ActiveFaults.clearActiveFault)] = false;
				#region logging level column
                ID = activelog[i].eventID;
				int tempInt =  (int) ((ID & logLevelMask)>> 10);
				if(tempInt<faultLevels.Length)
				{
					row[(int) (ActiveFaults.Level)] = this.faultLevels[tempInt];//(int) ((ID & logLevelMask)>> 10);
				}
				else
				{
					SystemInfo.errorSB.Append("\nUnrecognised fault level: " + tempInt.ToString() + "\nPlease report");
					row[(int) (ActiveFaults.Level)] = tempInt.ToString();
				}
				#endregion
				activeTable.Rows.Add(row);
			}
			activeTable.AcceptChanges();
		}
		private void activeTableChangeHandler(object sender, System.Data.DataColumnChangeEventArgs e)
		{
			this.dataGrid1.SuspendLayout();  //prevent exception if user continuously fast clicks on the cell
			activeTable.AcceptChanges();
			int i=0;
			foreach (DataRowView myRow in activeView)
			{
				activeResetArray[i++] = (bool) (myRow[(int) (ActiveFaults.clearActiveFault)]);
			}
			this.dataGrid1.ResumeLayout(); //calculations complete so we can allow the datagrid to re-draw the affected row
		}
		private void activeViewChangeHandler(object sender,ListChangedEventArgs e )
		{
			this.dataGrid1.SuspendLayout();  //prevent exception if user continuously fast clicks on the cell
			activeResetArray = new bool[this.activeView.Count];
			int i=0;
			foreach (DataRowView myRow in activeView)
			{
				activeResetArray[i++] = (bool) (myRow[(int) (ActiveFaults.clearActiveFault)]);
			}
			this.dataGrid1.ResumeLayout(); //calculations complete so we can allow the datagrid to re-draw the affected row
		}
		#endregion active Log

		#region group filters
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: requestGroupFilter
		///		 *  Description     : Requests the specifed data from the controller via DI interface. 
		///		 *					  This process is threaded and so this method acts as a wrapper 
		///		 *					  for the inner method which requires parameters.
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>


		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: createGroupFilterTable
		///		 *  Description     : Creates and fills the Group Filter table
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void createGroupFilterTable()
		{
            if (filterView != null)
            {
                filterView.ListChanged -= new System.ComponentModel.ListChangedEventHandler(filterViewChangeHandler);
                groupfilterTable.ColumnChanged -= new System.Data.DataColumnChangeEventHandler(filterTableChangeHandler);
            }
            fillFilterTable();
			filterView = new DataView(groupfilterTable);
			filterView.AllowNew = false; //prevents empty row at bottom of datagrid = which can casue array index out of bounds exceptions
			filterView.ListChanged += new System.ComponentModel.ListChangedEventHandler(filterViewChangeHandler);
			groupfilterTable.ColumnChanged += new System.Data.DataColumnChangeEventHandler(filterTableChangeHandler);
		}
		private void fillFilterTable()
		{
			DataRow row;
            groupfilterTable.Clear();
			faultFiltersArray = new bool[numGroupFilters]; //get zeroed when table is cleared due to zero length dataview
			for(ushort i = 0;i<this.numGroupFilters;i++)
			{
				row = groupfilterTable.NewRow();
				faultFiltersArray[i] = getFilterValue(i);
				row[(int) GroupFilters.Group] =  SCCorpStyle.FaultFifoGrpNames[i];
				row[(int) (GroupFilters.GroupNo)] = getGroupNo(i);
				row[(int) GroupFilters.saveEventsInGroup] = getFilterValue(i);
				groupfilterTable.Rows.Add(row);
			}
			groupfilterTable.AcceptChanges();
		}
		private void filterTableChangeHandler(object sender, System.Data.DataColumnChangeEventArgs e)
		{
			this.dataGrid1.SuspendLayout();  //prevent exception if user continuously fast clicks on the cell
			this.groupfilterTable.AcceptChanges();
			faultFiltersArray = new bool[this.filterView.Count];
			int i=0;
			foreach (DataRowView myRow in filterView)
			{
				faultFiltersArray[i++] = (bool) (myRow[(int) (GroupFilters.saveEventsInGroup)]);
			}
			this.dataGrid1.ResumeLayout(); //calculations complete so we can allow the datagrid to re-draw the affected row
		}
		private void filterViewChangeHandler(object sender,ListChangedEventArgs e )
		{
			this.dataGrid1.SuspendLayout();  //prevent exception if user continuously fast clicks on the cell
			faultFiltersArray = new bool[this.filterView.Count];
			int i=0;
			foreach (DataRowView myRow in filterView)
			{
				faultFiltersArray[i++] = (bool) (myRow[(int) (GroupFilters.saveEventsInGroup)]);
			}
			this.dataGrid1.ResumeLayout(); //calculations complete so we can allow the datagrid to re-draw the affected row
		}
		#endregion group filters


		#region user interaction zone
/// <summary>
///		/*--------------------------------------------------------------------------
///		 *  Name			: submitResetBtn_Click
///		 *  Description     : Event handler for the Submit button
///		 *  Parameters      : event args
///		 *  Used Variables  : 
///		 *  Preconditions   :  
///		 *  Post-conditions : 
///		 *  Return value    : none
///		 *--------------------------------------------------------------------------*/
/// </summary>
		private void submitResetBtn_Click(object sender, System.EventArgs e)
		{
			switch (faultWindowState)
			{
				case FaultWindowStates.FAULTLOG:  //reset fault log
					#region reset fault log
                    this.statusBar1.Text = "Requesting fault Log reset";
                    hideUserControls();
                    this.startDIThread("resetFaultLogWrapper", resetFaultLogWrapper);

					#region future stretch potential
					//If user interction is include din th efuture then we will need to switch off the additional event handlers
					//whilst the log is being changed
					//faultView.ListChanged -= new System.ComponentModel.ListChangedEventHandler(faultViewChangeHandler);
					//faultlogTable.ColumnChanged -= new System.Data.DataColumnChangeEventHandler(faultLogTableChangeHandler);
					#endregion future stretch potential

					#endregion reset fault log
					break;

				case FaultWindowStates.FAULTFILTERS:  //submit new group filtering
                    if (fltLogfilterSub != null)
                    {
                        #region submit new group filtering
                        this.statusBar1.Text = "Updating filters";
                        hideUserControls();
                        uint _Groupfilter = 0;
                        for (int i = 0; i < numGroupFilters; i++)
                        {
                            if ((bool)(groupfilterTable.Rows[i][(int)GroupFilters.saveEventsInGroup]) == true)
                            {
                                uint temp = 1;
                                temp = temp << i;
                                _Groupfilter += temp;
                            }
                        }
                        Groupfilter = System.Convert.ToUInt16(_Groupfilter);
                        this.startDIThread("setGroupFiltersWrapper", setGroupFiltersWrapper);
                        #endregion submit new group filtering
                    }
                    else 
                    {
                        this.statusBar1.Text = "Fault Log filters not available";
                    }
					break;
			} //ends switch
			if(SystemInfo.errorSB.Length>0)
			{
				this.sysInfo.displayErrorFeedbackToUser("Errors when resetting log");
			}
		}

/// <summary>
///		/*--------------------------------------------------------------------------
///		 *  Name			: dataGrid1_Click
///		 *  Description     : Event handler for when user clicks left mouse button anywhere
///		 *					  on the datagrid component
///		 *  Parameters      : event args
///		 *  Used Variables  : 
///		 *  Preconditions   :  
///		 *  Post-conditions : 
///		 *  Return value    : none
///		 *--------------------------------------------------------------------------*/
/// </summary>
        private void dataGrid1_Click(object sender, System.EventArgs e)
        {
            DataGrid.HitTestInfo hti;
            Point pt;
            int myCol;
            if ((faultWindowState == FaultWindowStates.FAULTFILTERS) && (this.fltLogfilterSub != null))
            {
                pt = this.dataGrid1.PointToClient(Control.MousePosition);
                hti = this.dataGrid1.HitTest(pt);
                myCol = (int)GroupFilters.saveEventsInGroup;
                if ((hti.Type == DataGrid.HitTestType.Cell) && (hti.Column == myCol) && this.sysInfo.systemAccess > 1)
                {
                    this.dataGrid1[hti.Row, myCol] = !(bool)this.dataGrid1[hti.Row, myCol];  //force the toggle 
                    RefreshRow(hti.Row);
                }
            }
        }
/// <summary>
///		/*--------------------------------------------------------------------------
///		 *  Name			: RefreshRow
///		 *  Description     : Calculates screen area of a user selected row.
///		 *					  Then invalidates this area. Used when user slects a row to be included in group filtering 
///		 *					  This combined with the overided paint function for the ColumnStyle 
///		 *					  causes the selected row to be highglighted
///		 *  Parameters      : Row number (datagrid)
///		 *  Used Variables  : 
///		 *  Preconditions   :  
///		 *  Post-conditions : 
///		 *  Return value    : none
///		 *--------------------------------------------------------------------------*/
/// </summary>
		private void RefreshRow(int row)
		{
			Rectangle rect = this.dataGrid1.GetCellBounds(row, 0);
			int myTop  = (int) rect.Top;
			rect = new Rectangle(rect.Left, myTop, this.dataGrid1.Width, rect.Height);
			this.dataGrid1.Invalidate(rect);
		}
/// <summary>
///		/*--------------------------------------------------------------------------
///		 *  Name			: comboBox1_SelectedIndexChanged
///		 *  Description     : Event HAndler called whenever combo box selected item changes
///		 *  Parameters      : Event args
///		 *  Used Variables  : 
///		 *  Preconditions   :  
///		 *  Post-conditions : 
///		 *  Return value    : none
///		 *--------------------------------------------------------------------------*/
/// </summary>
		private void tableSelectCombo_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			switch (this.tableSelectCombo.SelectedIndex)
			{
				case  0:  
					#region active faults
                    faultWindowState = FaultWindowStates.ACTIVE;
                    this.statusBar1.Text = "Getting active faults";
                    hideUserControls();
                    this.startDIThread("requestActiveLogWrapper", requestActiveLogWrapper);
					#endregion active faults
					break;

				case 1:  
					#region fault log
					faultWindowState = FaultWindowStates.FAULTLOG;
                    this.statusBar1.Text = "Getting Fault Log";
                    hideUserControls();
                    if (this.faultLogSub != null)
                    {
                        this.startDIThread("requestFaultLogWrapper", requestFaultLogWrapper);
                    }
                    else
                    {
                        this.showUserControls();
                    }
					#endregion fault log
					break;

				case 2:  
					#region group filtering
                    faultWindowState = FaultWindowStates.FAULTFILTERS;
                    if (fltLogfilterSub != null)
                    {
                        this.statusBar1.Text = "Getting filters";
                        hideUserControls();
                        this.startDIThread("getGroupFiltersWrapper", getGroupFiltersWrapper);
                    }
                    else
                    {
                        this.statusBar1.Text = "Fault Log filters not available";
                        this.showUserControls();
                    }
					#endregion group filtering
					break;
			}
		}

/// <summary>
///		/*--------------------------------------------------------------------------
///		 *  Name			: timer2_Tick
///		 *  Description     : Event Handler called whenever timer 2 elapses. 
///		 *					  This timer is used to regularylt get and display 
///		 *					  the current controller KEy hours
///		 *  Parameters      : Event args
///		 *  Used Variables  : 
///		 *  Preconditions   :  
///		 *  Post-conditions : 
///		 *  Return value    : none
///		 *--------------------------------------------------------------------------*/
///		 	 </summary>
/// <summary>
		private void timer2_Tick(object sender, System.EventArgs e)
		{
            //give any System log/filter retrieval priority
            if (this.DIThread == null)
            {
                this.label2.Visible = true;
                //if for the hours sub does not exist then quitly ignore it
                if ((hoursSub != null) && (this.minsSecsSub != null))
                {
                    timer2.Interval = 14000; //after first time in extend period - controller only updates key time every 15 secs
                    this.statusBar1.Text = "Getting Key hours";
                    this.startDIThread("getKeyHoursWrapper", getKeyHoursWrapper);
                }
                else
                {
                    label2.Text = "Key time: not available";
                    timer2.Enabled = false;
                    this.statusBar1.Text = "key hours not available";
                }

            }
		}

		#endregion

        #region hide/show user controls

        private void showFaultLog()
        {
            this.fillFaultTable();
            #region future stretch potential
            //and then switch them back on again 
            //faultView.ListChanged += new System.ComponentModel.ListChangedEventHandler(faultViewChangeHandler);
            //faultlogTable.ColumnChanged += new System.Data.DataColumnChangeEventHandler(faultLogTableChangeHandler);
            #endregion future stretch potential
        }

        private void showActiveLog()
        {
            activeResetArray = new bool[activelog.Length];
            for (ushort i = 0; i < activelog.Length; i++)
            {
                activeResetArray[i] = false;
            }
            activeTable.Clear();
            this.fillActiveTable();
            if (activeView != null)
            {
                activeView.ListChanged += new System.ComponentModel.ListChangedEventHandler(activeViewChangeHandler);
                activeTable.ColumnChanged += new System.Data.DataColumnChangeEventHandler(activeTableChangeHandler);
            }
        }
        private void showUserControls()
        {
            label1.Text = "";
            this.tableSelectCombo.Enabled = true;
            this.progressBar1.Visible = false;
            switch (faultWindowState)
            {
                case FaultWindowStates.ACTIVE:
                    #region activefaults
                    this.showActiveLog();
                    this.Text = "DriveWizard: Active Faults";
                    dataGrid1.DataSource = this.activeView;
                    activeView.Sort = ActiveFaults.Level.ToString() + " DESC";
                    applyActiveTableStyle();

                    if (this.activeFaultListSub == null)
                    {
                        this.statusBar1.Text = "Active faults not available";
                        this.label1.Text = "Active faults not available";
                    }
                    else if (SystemInfo.errorSB.Length == 0)
                    {
                        this.statusBar1.Text = "Displaying " + selectedNodeText + " active faults.";
                        if (this.activelog.Length > 0)
                        {
                            this.label1.Text = "Showing Active Faults";
                        }
                        else
                        {
                            this.label1.Text = "No Active Faults";
                        }
                    }
                    else
                    {
                        this.statusBar1.Text = "Failed to retrieve " + selectedNodeText + " active faults.";
                        this.label1.Text = "Failed to retrieve active faults.";
                    }
                    this.dataGrid1.CaptionText = "Active Faults on " + selectedNodeText;
                    break;
                    #endregion activefaults

                case FaultWindowStates.FAULTLOG:
                    #region Fault log
                    showFaultLog();
                    this.Text = "DriveWizard: Fault Log";
                    this.dataGrid1.DataSource = this.faultView;
                    faultView.Sort = FIFOCol.Time.ToString() + " DESC";
                    applyFaultlogTableStyle();
                    if (this.faultLogSub == null)
                    {
                        this.statusBar1.Text = "Fault log not available";
                        this.label1.Text = "Fault log not available";
                    }
                    else if (SystemInfo.errorSB.Length == 0)
                    {
                        this.dataGrid1.ReadOnly = false;
                        this.statusBar1.Text = "Displaying " + selectedNodeText + " fault log.";
                        if ((this.sysInfo.systemAccess > 1) && (this.faultLog.Length > 0) && (resetFaultLogSub != null))
                        {
                            this.submitBtn.Text = "&Reset this log";
                            this.submitBtn.Visible = true;
                            if (SystemInfo.errorSB.Length == 0)
                            {
                                this.label1.Text = "View or reset this fault log";
                            }
                        }
                        else
                        {
                            if (this.faultLog.Length > 0)
                            {
                                this.label1.Text = "Fault log (read only)";
                            }
                            else
                            {
                                this.label1.Text = "Fault log is empty";
                            }
                        }
                    }
                    else
                    {
                        this.statusBar1.Text = "Failed to retrieve " + selectedNodeText + " fault log.";
                        this.label1.Text = "Failed to retrieve current fault log.";
                    }
                    this.dataGrid1.CaptionText = "Fault log for " + selectedNodeText;
                    this.label2.Visible = true;
                    break;
                    #endregion Fault log

                case FaultWindowStates.FAULTFILTERS:
                    #region filters
                    createGroupFilterTable();
                    dataGrid1.DataSource = this.filterView;
                    filterView.Sort = GroupFilters.GroupNo.ToString(); ;
                    applyFilterTableStyle();
                    this.dataGrid1.CaptionText = "Group Filtering for " + selectedNodeText;
                    this.Text = "DriveWizard: Fault group filtering configuration";
                    if (this.fltLogfilterSub == null)
                    {
                        this.label1.Text = "Filters not available";
                    }
                    else if (SystemInfo.errorSB.Length == 0)
                    {
                        this.dataGrid1.ReadOnly = false;
                        this.statusBar1.Text = "Displaying " + selectedNodeText + " group filtering.";
                        if (this.sysInfo.systemAccess > 1)
                        {
                            this.dataGrid1.Click += new System.EventHandler(this.dataGrid1_Click);
                            this.submitBtn.Text = "&Update Group Filtering";
                            this.submitBtn.Visible = true;
                            if (SystemInfo.errorSB.Length == 0)
                            {
                                this.label1.Text = "Select events by Fault Group to store in node Fault Log. Click Update Group Filtering button to confirm selection";
                            }
                        }
                    }
                    else
                    {
                        this.statusBar1.Text = "Failed to retrieve " + selectedNodeText + " group filtering.";
                        this.label1.Text = "Failed to retrieve current group filtering configuration.";
                    }
                    break;
                    #endregion filters

            }
            if (SystemInfo.errorSB.Length > 0)
            {
                sysInfo.displayErrorFeedbackToUser("");
            }
        }

        private void hideUserControls()
        {
            this.label1.Text = "";
            this.submitBtn.Visible = false;
            this.dataGrid1.ReadOnly = true;
            this.tableSelectCombo.Enabled = false;
            this.dataGrid1.Click -= new System.EventHandler(this.dataGrid1_Click);
        }

#endregion hide/show user controls

		#region minor methods

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: dataGrid1_Resize
		///		 *  Description     : Event handler called when datagrid has been resized. 
		///		 *					  Used for updating column widths.
		///		 *  Parameters      : Event args
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		///			 </summary>
		private void dataGrid1_Resize(object sender, System.EventArgs e)
		{
			switch (faultWindowState)
			{
				case FaultWindowStates.FAULTFILTERS:
					applyFilterTableStyle();
					break;

				case FaultWindowStates.FAULTLOG:
					applyFaultlogTableStyle();
					break;

				case FaultWindowStates.ACTIVE:
					applyActiveTableStyle();
					break;

				default:
					break;
			}
		}

		private string getEventNo(ushort fullID)
		{
			string eventStr = "";
			int temp = (int) (fullID & 0x7FFF);
			eventStr = temp.ToString("X");
			while(eventStr.Length<4)
			{
				eventStr = "0" + eventStr;
			}
			eventStr = "0x" + eventStr;
			return eventStr;
		}
		private string getGroupName(ushort fullID)
		{
			int groupNo = (int) ((fullID & groupIDMask) >> 6);
			return SCCorpStyle.FaultFifoGrpNames[groupNo];
		}
		private string getDataByte(Byte dataByte)
		{
			string tempStr = dataByte.ToString("X");
			if(tempStr.Length<2)
			{
				tempStr = "0" + tempStr;
			}
			return "0x" + tempStr;
		}
		private string getGroupNo(ushort GroupIndex)
		{
			string tempStr = GroupIndex.ToString();
			if(tempStr.Length<2)
			{
				tempStr = "0" + tempStr;
			}
			return tempStr; 
		}

		private bool getFilterValue(ushort filterNo)
		{
            uint temp;
            if (fltLogfilterSub != null)
            {
                temp = (uint)fltLogfilterSub.currentValue;
            }
            else
            {
                temp = 0;
            }
			uint temp1 = temp;
			temp = temp1 << (31-filterNo);// remove any 1's to the left
			temp1 = temp;
			temp = temp1 >> 31; //now shift back to the right
			return ((bool)(temp >= 1));
		}

		private void createFilterTableStyle()
		{
			int [] colWidths  = new int[filterPercents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, filterPercents, 0, dataGrid1DefaultHeight);
			TSgroupfilter = new FIFOGroupFilterTableStyle(colWidths); //re-construct the table style
			this.dataGrid1.TableStyles.Add(TSgroupfilter); // and apply it to the Datagrid
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: applyFilterTableStyle
		///		 *  Description     : Datagrid column widths are a function of the TableStyle not the datagrid 
		///		 *					  So non-even columns are not autoomatically resized when the datagrid is resized. 
		///		 *					  This method calculates column widths from the datagrid Client size and 
		///		 *					  the percentage for each column. Slight rounding down is needed to prevent appearance of 
		///		 *					  scroll bar at some sizes. New TableStyle is then constructed using these values.
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void applyFilterTableStyle()
		{
			int [] colWidths  = new int[filterPercents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, filterPercents, 0, dataGrid1DefaultHeight);
			for (int i = 0;i<filterPercents.Length;i++)
			{
				TSgroupfilter.GridColumnStyles[i].Width = colWidths[i];
			}
		}
		private void createActiveTableStyle()
		{
			int [] colWidths  = new int[activePercents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, activePercents, 0, dataGrid1DefaultHeight);
			TSactivelog = new actFaultTableStyle(colWidths); //re-construct the table style

		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: applyActiveTableStyle
		///		 *  Description     : Datagrid column widths are a function of the TableStyle not the datagrid 
		///		 *					  So non-even columns are not autoomatically resized when the datagrid is resized. 
		///		 *					  This method calculates column widths from the datagrid Client size and 
		///		 *					  the percentage for each column. Slight rounding down is needed to prevent appearance of 
		///		 *					  scroll bar at some sizes. New TableStyle is then constructed using these values.
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void applyActiveTableStyle()
		{
			int [] colWidths  = new int[activePercents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, activePercents, 0, dataGrid1DefaultHeight);
			for (int i = 0;i<activePercents.Length;i++)
			{
				TSactivelog.GridColumnStyles[i].Width = colWidths[i];
			}
			this.dataGrid1.TableStyles.Add(TSactivelog); // and apply it to the Datagrid
		}
		private void createFaultlogTableStyle()
		{
			int [] colWidths  = new int[faultlogPercents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, faultlogPercents,0, dataGrid1DefaultHeight);
			TSfaultlog = new FIFOTableStyle(colWidths); //re-construct the table style
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: applyFaultlogTableStyle
		///		 *  Description     : Datagrid column widths are a function of the TableStyle not the datagrid 
		///		 *					  So non-even columns are not autoomatically resized when the datagrid is resized. 
		///		 *					  This method calculates column widths from the datagrid Client size and 
		///		 *					  the percentage for each column. Slight rounding down is needed to prevent appearance of 
		///		 *					  scroll bar at some sizes. New TableStyle is then constructed using these values.
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void applyFaultlogTableStyle()
		{
			int [] colWidths  = new int[faultlogPercents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, faultlogPercents, 0, dataGrid1DefaultHeight);
			for (int i = 0;i<faultlogPercents.Length;i++)
			{
				TSfaultlog.GridColumnStyles[i].Width = colWidths[i];
			}
			this.dataGrid1.TableStyles.Add(TSfaultlog); // and apply it to the Datagrid
		}


		#endregion minor methods

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

/// <summary>
/// 	/*--------------------------------------------------------------------------
///		 *  Name			: timer2_Tick
///		 *  Description     : Clean up any resources being used.
///		 *  Parameters      : disposing flag
///		 *  Used Variables  : 
///		 *  Preconditions   :  
///		 *  Post-conditions : 
///		 *  Return value    : none
///		 *--------------------------------------------------------------------------*/
/// </summary>
/// 
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
/// <summary>
/// 	/*--------------------------------------------------------------------------
///		 *  Name			: FAULT_LOG_WINDOW_Closing
///		 *  Description     : Event handler. 
///							  Any processes that MUST be done before the window closes are done here
///		 *  Parameters      : disposing flag
///		 *  Used Variables  : 
///		 *  Preconditions   :  
///		 *  Post-conditions : 
///		 *  Return value    : none
///		 *--------------------------------------------------------------------------*/
///		 /// </summary>
		private void FAULT_LOG_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.statusBar1.Text = "Performing finalisation, please wait.";
            if (SystemInfo.errorSB.Length > 0)
            {
                this.sysInfo.displayErrorFeedbackToUser("Errors with logs");
            }

            #region disable all timers
			this.timer1.Enabled = false;
			this.timer2.Enabled = false;
            //We need to 'remove' any failed hours messages in erroSB before exiting
            //do it here so we can still add Thread closure failure in if required
            sysInfo.clearErrorSB();
			#endregion disable all timers
			#region stop all threads
			#region data retreival thread
			if(this.DIThread != null)
			{
                if ((DIThread.ThreadState & System.Threading.ThreadState.Stopped) == 0)
				{
                    DIThread.Abort();

                    if (DIThread.IsAlive == true)
					{
#if DEBUG
                        SystemInfo.errorSB.Append("\nFailed to close Thread: " + DIThread.Name + " on exit");
#endif
                        DIThread = null;
					}
				}
			}
			#endregion data retreival thread
			#endregion stop all threads
			faultWindowState = FaultWindowStates.NONE;  //reset ready for next entry into this window
			e.Cancel = false; //force this window to close
		}
		private void FAULT_LOG_WINDOW_Closed(object sender, System.EventArgs e)
		{
			#region reset window title and status bar
			this.statusBar1.Text = "";
			#endregion reset window title and status bar		
		}

		#endregion

	}
	#endregion FAULT_LOG_WINDOW class

	#region FIFO DataTable class
	public class FIFOTable : DataTable
	{
		public FIFOTable()
		{
			//the following code format avoids fixed column indecees to allow easy removal/insertion of columns without
			//throwing up errors
			//for a column to be shwon in datagrid, it must be defined as a column with the same mapping name as the corresponsding
			//tableStyles column.  All possible columns are defined hare and those now required for a window can be removed from grid by removing 
			//appropriate tableStyle column entry.
			this.Columns.Add(FIFOCol.Name.ToString(),typeof(System.String));
			this.Columns.Add(FIFOCol.ID.ToString(), typeof(System.String));
			this.Columns.Add(FIFOCol.Time.ToString(),typeof(System.String));
			this.Columns.Add(FIFOCol.DB1.ToString(), typeof(System.String));
			this.Columns.Add(FIFOCol.DB2.ToString(),typeof(System.String));
			this.Columns.Add(FIFOCol.DB3.ToString(), typeof(System.String));
			this.Columns.Add(FIFOCol.Group.ToString(), typeof(System.String));
			this.Columns[FIFOCol.Group.ToString()].DefaultValue = "unknown";
			this.Columns.Add(FIFOCol.Level.ToString(), typeof(System.String));
			this.Columns[FIFOCol.Level.ToString()].DefaultValue = "unknown";
		}
	}
	#endregion FIFO DataTable class

	#region Faults FIFO TableStyle class
	/// <summary>
	/// Summary description for FIFOTableStyle.
	/// </summary>
	public class FIFOTableStyle : SCbaseTableStyle
	{
		public FIFOTableStyle(int [] ColWidths)
			{
				createEvNameCol(ColWidths);
				createEvIDCol(ColWidths);
				createEvTimeCol(ColWidths);
				createEvDByte1Col(ColWidths);
				createEvDByte2Col(ColWidths);
				createEvDByte3Col(ColWidths);
				createEvGroupCol(ColWidths);
				createEvLevelCol(ColWidths);
			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();

			}

		private void createEvNameCol(int [] ColWidths)
			{
				SCbaseRODataGridTextBoxColumn EvNameCol = new SCbaseRODataGridTextBoxColumn((int) FIFOCol.Name);
				EvNameCol.MappingName = FIFOCol.Name.ToString();
				EvNameCol.HeaderText = "Event Name";
				EvNameCol.Width = ColWidths[(int) FIFOCol.Name];
				EvNameCol.ReadOnly = true;
				GridColumnStyles.Add(EvNameCol);
			}

		private void createEvIDCol(int [] ColWidths)
			{
				SCbaseRODataGridTextBoxColumn EvIDCol = new SCbaseRODataGridTextBoxColumn((int) FIFOCol.ID);
				EvIDCol.MappingName = FIFOCol.ID.ToString();
				EvIDCol.HeaderText = "Fault ID";
				EvIDCol.Width = ColWidths[(int) FIFOCol.ID];
				EvIDCol.ReadOnly = true;
				EvIDCol.Alignment = HorizontalAlignment.Right;
				GridColumnStyles.Add(EvIDCol);
			}

		private void createEvTimeCol(int [] ColWidths)
			{
				SCbaseRODataGridTextBoxColumn firstTimeCol = new SCbaseRODataGridTextBoxColumn((int) FIFOCol.Time);
				firstTimeCol.MappingName = FIFOCol.Time.ToString();
				firstTimeCol.HeaderText = "Logged at"; 
				firstTimeCol.Width = ColWidths[(int) FIFOCol.Time];
				firstTimeCol.ReadOnly = true;
				firstTimeCol.Alignment = HorizontalAlignment.Right;
				GridColumnStyles.Add(firstTimeCol);
			}
		private void createEvDByte1Col(int [] ColWidths)
			{
				SCbaseRODataGridTextBoxColumn EvDByte1Col = new SCbaseRODataGridTextBoxColumn((int) FIFOCol.DB1);
				EvDByte1Col.MappingName = FIFOCol.DB1.ToString();
				EvDByte1Col.HeaderText = "DB1";
				EvDByte1Col.Width = ColWidths[(int) FIFOCol.DB1];
				EvDByte1Col.ReadOnly = true;
				EvDByte1Col.Alignment = HorizontalAlignment.Right;
				GridColumnStyles.Add(EvDByte1Col);
			}
		private void createEvDByte2Col(int [] ColWidths)
			{
				SCbaseRODataGridTextBoxColumn EvDByte2Col = new SCbaseRODataGridTextBoxColumn((int) FIFOCol.DB2);
				EvDByte2Col.MappingName = FIFOCol.DB2.ToString();
				EvDByte2Col.HeaderText = "DB2";
				EvDByte2Col.Width = ColWidths[(int) FIFOCol.DB2];
				EvDByte2Col.ReadOnly =true;
				EvDByte2Col.Alignment = HorizontalAlignment.Right;
				GridColumnStyles.Add(EvDByte2Col);
			}
		private void createEvDByte3Col(int [] ColWidths)
			{
				SCbaseRODataGridTextBoxColumn EvDByte3Col = new SCbaseRODataGridTextBoxColumn((int) FIFOCol.DB3);
				EvDByte3Col.MappingName = FIFOCol.DB3.ToString();
				EvDByte3Col.HeaderText = "DB3";
				EvDByte3Col.Width = ColWidths[(int) FIFOCol.DB3];
				EvDByte3Col.ReadOnly = true;
				EvDByte3Col.Alignment = HorizontalAlignment.Right;
				GridColumnStyles.Add(EvDByte3Col);
			}
		private void createEvGroupCol(int [] ColWidths)
			{
				SCbaseRODataGridTextBoxColumn c1 = new SCbaseRODataGridTextBoxColumn((int) FIFOCol.Group);
				c1.MappingName = FIFOCol.Group.ToString();
				c1.HeaderText = "LED Flash/Group";
				c1.ReadOnly = true;
				c1.Width = ColWidths[(int) FIFOCol.Group];
				GridColumnStyles.Add(c1);
			}

		private void createEvLevelCol(int [] ColWidths)
			{
				SCbaseRODataGridTextBoxColumn col = new SCbaseRODataGridTextBoxColumn((int) FIFOCol.Level);
				col.MappingName = FIFOCol.Level.ToString();
				col.HeaderText = "Level";
				col.Width = ColWidths[(int) FIFOCol.Level];
				col.ReadOnly = true;
				GridColumnStyles.Add(col);
			}
	}
	#endregion Faults FIFO TableStyle class

	#region Active Faults Datatable class
	public class ActiveFaultsTable: DataTable
	{
		public ActiveFaultsTable()
		{
			this.Columns.Add(ActiveFaults.Name.ToString(),typeof(System.String));
			this.Columns.Add(ActiveFaults.ID.ToString(), typeof(System.String));
			this.Columns.Add(ActiveFaults.Group.ToString(), typeof(System.String));
			this.Columns.Add(ActiveFaults.Level.ToString(), typeof(System.String));
			this.Columns.Add(ActiveFaults.clearActiveFault.ToString(), typeof(System.Boolean));
		}
	}
	#endregion

	#region activefault tableStyle class
	public class actFaultTableStyle : SCbaseTableStyle
	{
		public actFaultTableStyle(int [] ColWidths)
		{
			LogFormattableTextBoxColumn EvNameCol = new LogFormattableTextBoxColumn((int) ActiveFaults.Name);
			EvNameCol.MappingName = ActiveFaults.Name.ToString();
			EvNameCol.HeaderText = "Event Name";
			EvNameCol.Width = ColWidths[(int) ActiveFaults.Name];
			EvNameCol.ReadOnly = true;
			GridColumnStyles.Add(EvNameCol);

			LogFormattableTextBoxColumn EvIDCol = new LogFormattableTextBoxColumn((int) ActiveFaults.ID);
			EvIDCol.MappingName = ActiveFaults.ID.ToString();
			EvIDCol.HeaderText = "Event ID";
			EvIDCol.Width =  ColWidths[(int) ActiveFaults.ID];
			EvIDCol.ReadOnly = true;
			GridColumnStyles.Add(EvIDCol);

			LogFormattableTextBoxColumn EvGroupCol = new LogFormattableTextBoxColumn((int) ActiveFaults.Group);
			EvGroupCol.MappingName = ActiveFaults.Group.ToString();
			EvGroupCol.HeaderText = "Group";
			EvGroupCol.Width =  ColWidths[(int) ActiveFaults.Group];
			EvGroupCol.ReadOnly = true;
			GridColumnStyles.Add(EvGroupCol);

			LogFormattableTextBoxColumn col = new LogFormattableTextBoxColumn((int) ActiveFaults.Level);
			col.MappingName = ActiveFaults.Level.ToString();
			col.HeaderText = "Level";
			col.Width =  ColWidths[(int) ActiveFaults.Level];
			col.ReadOnly = true;
			GridColumnStyles.Add(col);

			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();

		}
	}
	#endregion

	#region FIFO Groupfiltering table class
	public class FIFOGroupFilterTable : DataTable
	{
		public FIFOGroupFilterTable()
		{
			this.Columns.Add(GroupFilters.Group.ToString(), typeof(System.String));
			this.Columns.Add(GroupFilters.GroupNo.ToString(), typeof(System.String));
			this.Columns.Add(GroupFilters.saveEventsInGroup.ToString(),typeof(System.Boolean));
		}
	}
	#endregion

	#region TableStyle for FIFO Group filtering class
	public class FIFOGroupFilterTableStyle : SCbaseTableStyle
	{
		public FIFOGroupFilterTableStyle(int [] ColWidths)
		{
			LogFormattableTextBoxColumn NameCol = new LogFormattableTextBoxColumn((int)GroupFilters.Group);
			NameCol.MappingName = GroupFilters.Group.ToString();
			NameCol.HeaderText = "Group Filter";
			NameCol.Width = ColWidths[(int)GroupFilters.Group];
			NameCol.ReadOnly = true;
			GridColumnStyles.Add(NameCol);

			LogFormattableTextBoxColumn GroupNo = new LogFormattableTextBoxColumn((int)GroupFilters.GroupNo);
			GroupNo.MappingName = GroupFilters.GroupNo.ToString();
			GroupNo.HeaderText = "Group Number";
			GroupNo.Width = ColWidths[(int)GroupFilters.GroupNo];
			GroupNo.ReadOnly = true;
			GridColumnStyles.Add(GroupNo);

			LogFormattableBooleanColumn SetCol = new LogFormattableBooleanColumn((int) GroupFilters.saveEventsInGroup);
			SetCol.MappingName = GroupFilters.saveEventsInGroup.ToString();
			SetCol.HeaderText = "Record events in this group?";
			SetCol.Width = ColWidths[(int) GroupFilters.saveEventsInGroup];
			SetCol.Alignment = HorizontalAlignment.Center; 
			GridColumnStyles.Add(SetCol);
			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();

		}
	}
	#endregion

	#region logs LogFormattableTextBoxColumn
	public class LogFormattableTextBoxColumn : SCbaseRODataGridTextBoxColumn
	{
		public LogFormattableTextBoxColumn(int columIndex) : base(columIndex)
		{
		}
		//used to fire an event to retrieve formatting info and then draw the cell with this formatting info
        protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
        {
            switch (FAULT_LOG_WINDOW.faultWindowState)
            {
                case FaultWindowStates.ACTIVE:
                    if (rowNum < FAULT_LOG_WINDOW.activeResetArray.Length)
                    {
                        if ((FAULT_LOG_WINDOW.activeResetArray != null) && (FAULT_LOG_WINDOW.activeResetArray[rowNum] == true))
                        {
                            backBrush = new SolidBrush(SCCorpStyle.dgRowSelected);
                        }
                    }
                    break;

                case FaultWindowStates.FAULTFILTERS:
                    if (rowNum < FAULT_LOG_WINDOW.faultFiltersArray.Length)
                    {
                        if ((FAULT_LOG_WINDOW.faultFiltersArray != null) && (FAULT_LOG_WINDOW.faultFiltersArray[rowNum] == true))
                        {
                            backBrush = new SolidBrush(SCCorpStyle.dgRowSelected);
                        }
                    }
                    break;
            }
            base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
        }
	}

	#endregion logs LogFormattableTextBoxColumn

	#region Logs Formattable Bool Column
    public class LogFormattableBooleanColumn : SCbaseRODataGridBoolColumn
    {
        public LogFormattableBooleanColumn(int columnIndex)
            : base(columnIndex)
        {

        }
        //used to fire an event to retrieve formatting info and then draw the cell with this formatting info
        protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
        {
            switch (FAULT_LOG_WINDOW.faultWindowState)
            {
                case FaultWindowStates.ACTIVE:
                    if (rowNum < FAULT_LOG_WINDOW.activeResetArray.Length)
                    {
                        if ((FAULT_LOG_WINDOW.activeResetArray != null) && (FAULT_LOG_WINDOW.activeResetArray[rowNum] == true))
                        {
                            backBrush = new SolidBrush(SCCorpStyle.dgRowSelected);
                        }
                    }
                    break;

                case FaultWindowStates.FAULTFILTERS:
                    if (rowNum < FAULT_LOG_WINDOW.faultFiltersArray.Length)
                    {
                        if ((FAULT_LOG_WINDOW.faultFiltersArray != null) && (FAULT_LOG_WINDOW.faultFiltersArray[rowNum] == true))
                        {
                            backBrush = new SolidBrush(SCCorpStyle.dgRowSelected);
                        }
                    }
                    break;
            }
            base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
        }
    }
	#endregion
}

