/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.79$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/09/2008 23:18:00$
	$ModDate:23/09/2008 11:32:36$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	This window displays a system log retrieved from a Sevcon node. For each event 
    in the log, the event ID, description and time when it occurred with up to three 
    event specific data bytes are shown. It is possible to reset the log and set/modify 
    the group filters.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36811: SYSTEM_LOG_WINDOW.cs 

   Rev 1.79    23/09/2008 23:18:00  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.78    17/03/2008 13:13:58  jw
 Logs null obejct handling complete. Still needs progrees bar steps linking to
 odSub timeout values


   Rev 1.77    14/03/2008 10:58:54  jw
 Log handling revised, Read of Log releated CAN items now done from GUI ( to
 allow later DI /Ixxat unlinking) . Processing of recevied logs code and event
 lists moved to sysInfo - reduces memory usage ( one method per application
 rather than per node)  Some newer Sevocn devices do not hav ethese logs. Also
 event list methods now have product code input - to allow us to have seperate
 Event lists for eg displays etc. Note:some error hanlding eg null detection
 still needed but check back in for working set with DI 


   Rev 1.76    13/03/2008 08:57:48  jw
 Some common ErrorSB handling  tasks moved form GUI files to sysInfo to reduce
 code size and complexity


   Rev 1.75    12/03/2008 13:43:58  ak
 All DI threads have ThreadPriority increased to Normal from BelowNormal
 (needed when running VCI3)


   Rev 1.74    11/03/2008 09:37:36  jw
 Some code simplifications  and minor fixes improvments 


   Rev 1.73    07/03/2008 09:01:16  jw
 Start Thread meoths replace by single method that takes delagate meothd  and
 threadName as input parameters.  Simplifes code.


   Rev 1.72    06/03/2008 13:07:02  jw
 All DI data transfer is not on seperate thread to allow future unlinking of
 DI to Ixxat data requests.  Error message /User feedback improved.


   Rev 1.71    15/02/2008 11:44:52  jw
 Reduncadnt code line removed to reduce compiler warnigns


   Rev 1.70    22/01/2008 23:17:00  ak
 Improved handling when the controller isn't communicating.


   Rev 1.69    05/12/2007 22:13:10  ak
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
	public enum SystemLogWindowStates {SYSTEMLOG, SYSTEMLOGFILTERS, NONE};
	//FIFO Cols, GroupFilters - defined in Fault Log file
	#endregion enumerated type declarations

	/// <summary>
	/// Summary description for SYSTEM_LOG_WINDOW.
	/// </summary>
	public class SYSTEM_LOG_WINDOW : System.Windows.Forms.Form
	{
		#region control declarations
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Timer timer2;
		private System.Windows.Forms.Label label2;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.StatusBar statusBar1;
        ToolBar toolbar = null;
		#endregion
		
		#region my declarations
		private FIFOTable systemlogTable;
		private DataView SystemLogView, filterView;
		private SystemInfo sysInfo;
		private string selectedNodeText = "";
        private nodeInfo node = null;
		private FIFOGroupFilterTable sysgroupfilterTable;
		private FIFOLogEntry [] systemLog = new FIFOLogEntry[0];
		private Thread DIThread = null;
		private int groupIDMask = 0x03C0, logLevelMask = 0x1C00;
		internal static SystemLogWindowStates systemLogWindowState = SystemLogWindowStates.NONE;
		public static bool [] systemFilterArray;
		private ushort numGroupFilters = 16;
		float [] filterPercents = {0.5F, 0.2F, 0.3F};
	    float [] eventPercents = {0.3F, 0.1F, 0.13F, 0.07F, 0.07F, 0.07F, 0.18F, 0.08F};
		SysGroupFilterTableStyle filterTableStyle = null;
		SysFIFOTableStyle tablestyle = null;
		int dataGrid1DefaultHeight = 0;

        #region OD items
        private ODItemData hoursSub = null;
        private ODItemData minsSecsSub = null;
        private ODItemData sysLogFilterSub = null;
        private ODItemData resetSysLogSub = null;
        private ODItemData systemLogSub = null;
        ODItemData eventIDsSub = null;
        ODItemData eventNameSub = null;
        #endregion OD items

        int missingEventPtr = 0;
        private ArrayList unknownFaultIDs = new ArrayList(); //DR38000269 single list of all unknown faultsIDs kept


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
            this.label1 = new System.Windows.Forms.Label();
            this.closeBtn = new System.Windows.Forms.Button();
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.submitBtn = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(755, 25);
            this.label1.TabIndex = 11;
            // 
            // closeBtn
            // 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Location = new System.Drawing.Point(659, 447);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(120, 25);
            this.closeBtn.TabIndex = 10;
            this.closeBtn.Text = "&Close window";
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // dataGrid1
            // 
            this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dataGrid1.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
            this.dataGrid1.CaptionText = "System Log";
            this.dataGrid1.DataMember = "";
            this.dataGrid1.FlatMode = true;
            this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Location = new System.Drawing.Point(8, 56);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.ParentRowsVisible = false;
            this.dataGrid1.PreferredColumnWidth = 200;
            this.dataGrid1.ReadOnly = true;
            this.dataGrid1.RowHeadersVisible = false;
            this.dataGrid1.Size = new System.Drawing.Size(771, 375);
            this.dataGrid1.TabIndex = 7;
            this.dataGrid1.Resize += new System.EventHandler(this.dataGrid1_Resize);
            this.dataGrid1.Click += new System.EventHandler(this.dataGrid1_Click);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem3,
            this.menuItem8});
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 0;
            this.menuItem3.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
            this.menuItem3.Text = "&File";
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 1;
            this.menuItem8.MergeOrder = 2;
            this.menuItem8.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
            this.menuItem8.Text = "&Help";
            // 
            // submitBtn
            // 
            this.submitBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.submitBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.submitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.submitBtn.Location = new System.Drawing.Point(16, 490);
            this.submitBtn.Name = "submitBtn";
            this.submitBtn.Size = new System.Drawing.Size(192, 25);
            this.submitBtn.TabIndex = 13;
            this.submitBtn.Text = "&Reset log";
            this.submitBtn.Visible = false;
            this.submitBtn.Click += new System.EventHandler(this.submitBtn_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBox1.Enabled = false;
            this.comboBox1.Location = new System.Drawing.Point(16, 448);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(192, 24);
            this.comboBox1.TabIndex = 14;
            this.comboBox1.Text = "comboBox1";
            this.comboBox1.SelectionChangeCommitted += new System.EventHandler(this.comboBox1_SelectionChangeCommitted);
            // 
            // timer2
            // 
            this.timer2.Enabled = true;
            this.timer2.Interval = 200;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(515, 490);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(264, 24);
            this.label2.TabIndex = 15;
            this.label2.Text = "Key time:  00:00:00";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label2.Visible = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(0, 556);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(792, 24);
            this.progressBar1.TabIndex = 16;
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 558);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(792, 22);
            this.statusBar1.TabIndex = 17;
            this.statusBar1.Text = "statusBar1";
            // 
            // SYSTEM_LOG_WINDOW
            // 
            this.AcceptButton = this.submitBtn;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(792, 580);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.dataGrid1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.submitBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.closeBtn);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Menu = this.mainMenu1;
            this.Name = "SYSTEM_LOG_WINDOW";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "System Log";
            this.Load += new System.EventHandler(this.SYSTEM_LOG_WINDOW_Load);
            this.Closed += new System.EventHandler(this.SYSTEM_LOG_WINDOW_Closed);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.SYSTEM_LOG_WINDOW_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		#region intialisation
		public SYSTEM_LOG_WINDOW(SystemInfo passed_systemInfo, int nodeNum, string nodeText, ToolBar passed_toolbar)
		{
			InitializeComponent();
			sysInfo = passed_systemInfo;
            this.node = this.sysInfo.nodes[nodeNum];
			selectedNodeText = nodeText;
			this.sysInfo.formatDataGrid(this.dataGrid1);
			string [] tablechoices = {"System Log", "Group Filtering"};
			comboBox1.DataSource = tablechoices;
			this.toolbar = passed_toolbar;

            #region get OD items
            MAIN_WINDOW.appendErrorInfo = false;
            hoursSub = node.getODSubFromObjectType(SevconObjectType.CONTROLLER_HOURS, 0x1);
            minsSecsSub = node.getODSubFromObjectType(SevconObjectType.CONTROLLER_HOURS, 0x2);
            sysLogFilterSub = node.getODSubFromObjectType(SevconObjectType.SYSTEM_FIFO_CTRL, 0x03);
            resetSysLogSub =  node.getODSubFromObjectType(SevconObjectType.SYSTEM_FIFO_CTRL,  0x01);
            systemLogSub = node.getODSubFromObjectType(SevconObjectType.DOMAIN_UPLOAD, SCCorpStyle.SystemFIFOSubObject);
            eventIDsSub = node.getODSubFromObjectType(SevconObjectType.EVENT_ID_DESCRIPTION, SCCorpStyle.EventIDSubObject);
            eventNameSub = node.getODSubFromObjectType(SevconObjectType.EVENT_ID_DESCRIPTION, SCCorpStyle.EventNameSubObject);
            MAIN_WINDOW.appendErrorInfo = true;
            #endregion get OD items

        }
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: SYSTEM_LOG_WINDOW_Load
		///		 *  Description     : Event HAndler for form load event
		///		 *  Parameters      : System event arguments 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void SYSTEM_LOG_WINDOW_Load(object sender, System.EventArgs e)
		{
			this.statusBar1.Text = "Retrieving system log information";
			this.systemlogTable = new FIFOTable();
			sysgroupfilterTable = new FIFOGroupFilterTable();
			this.progressBar1.Maximum = 0;			// maximum is not known until first SDO response is received
			#region button colouring
			foreach (Control myControl in this.Controls)
			{
				if((myControl.GetType().ToString()) == "System.Windows.Forms.Button")
				{
					myControl.BackColor = SCCorpStyle.buttonBackGround;
				}
			}
			#endregion button colouring
			dataGrid1DefaultHeight = this.dataGrid1.Height;
            this.createEventTableStyle();
            if (this.systemLogSub != null)
            {
                this.statusBar1.Text = "Getting System Log";
                startDIThread("requestLogWrapper", requestLogWrapper);
            }
            else
            {
                systemLogWindowState = SystemLogWindowStates.SYSTEMLOG;
                this.showUserControls();
            }
        }


        #endregion initialisation


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

        #region thread wrappers

        private void requestLogWrapper()
        {
            //DR38000259 Clear out the currentValue from the systemLogSub prior to re-reading this.
            //If the log has been reset & is empty, reading it returns a CANGeneralError with the
            //reason "<Unable to read Sevcon abort code>" which is normal for an empty log.
            //However, the old currentValueDomain value remains causing DW to erroneously still
            //display the previous log values (making it look like the log wasn't reset).
            systemLogSub.currentValueDomain = null;
            DIFeedbackCode feedback = node.readODValue(systemLogSub);

            if (feedback != DIFeedbackCode.DISuccess) //DR38000260
            {
                SystemInfo.errorSB.Append("\nUnable to retreive system log for node ID " + node.nodeID.ToString());
            }
        }

        private void ResetSystemLogWrapper()
        {
            node.writeODValue(resetSysLogSub, 0x01);
        }

        private void getKeyHoursWrapper()
        {
            node.readODValue(hoursSub);
        }
        private void getKeyMinsSecsWrapper()
        {
            node.readODValue(minsSecsSub);
        }

        private void setGroupFiltersWrapper()
        {
            node.writeODValue(sysLogFilterSub, Groupfilter);
        }
        private void getGroupFiltersWrapper()
        {
            node.readODValue(sysLogFilterSub);
        }

        // DR38000269 read & writeMissingEventDescWrapper() replaced by getMissingEventDescWrapper()
        // so that all unknownFaultIDs can be retrieved in a single DI thread
        private void getMissingEventDescWrapper()
        {
            DIFeedbackCode feedback;
            for (int i = 0; i < unknownFaultIDs.Count; i++)
            {
                ushort faultID = ((ushort)unknownFaultIDs[i]);
                feedback = node.writeODValue(eventIDsSub, (long)faultID); //write unknown ID

                if (feedback == DIFeedbackCode.DISuccess)
                {
                    feedback = node.readODValue(eventNameSub); //read back string

                    if (feedback == DIFeedbackCode.DISuccess)
                    {
                        // update overall maintained list
                        this.sysInfo.updateEventList(faultID, eventNameSub.currentValueString, node.productCode);
                    }
                }

                if (feedback == DIFeedbackCode.DINoResponseFromController)
                {
                    break;      // bomb out of loop if controller not responding
                }
            }
        }
        #endregion thread wrappers

        #region timer ticks
        private void timer1_Tick(object sender, System.EventArgs e)
		{
            #region DIThread
			if(DIThread != null)
			{
                string threadName = DIThread.Name;
				if((DIThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
                    this.DIThread = null;
                    timer1.Enabled = false; //we will restart if required
                    this.progressBar1.Value = this.progressBar1.Maximum;  //for aesthetics
                    progressBar1.Visible = false;
                    this.statusBar1.Text = "";
                    #region specific thread handling
                    switch (threadName)
                    {
                        case "requestLogWrapper":
                            #region request system log
                            systemLogWindowState = SystemLogWindowStates.SYSTEMLOG;
                            sysInfo.insertErrorType("Failed to read System log");
                            sysInfo.processFIFOLog(systemLogSub, out systemLog, out unknownFaultIDs); //DR38000269
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
                            break;
                            #endregion request system log

                        case "getMissingEventDescWrapper":
                            //DR38000269 re-process log now new descriptors have been read
                            sysInfo.processFIFOLog(systemLogSub, out systemLog, out unknownFaultIDs);
                            this.createLogTable();
                            if (systemLogWindowState == SystemLogWindowStates.NONE)
                            {
                                systemLogWindowState = SystemLogWindowStates.SYSTEMLOG;
                            }
                            showUserControls();
                            break;

                        case "getGroupFiltersWrapper":
#region get group filters
                            sysInfo.insertErrorType("Failed to read group filters");
                            showUserControls();
                            break;
                        #endregion get group filters

                        case "setGroupFiltersWrapper":
                            #region set group filters
                            sysInfo.insertErrorType("Failed to update group filters");
                              this.statusBar1.Text = "Getting System Log filters";
                             this.startDIThread("getGroupFiltersWrapper", getGroupFiltersWrapper);
                            break;
                        #endregion set group filters

                        case "ResetSystemLogWrapper":
                            #region reset system log
                            sysInfo.insertErrorType("Failed to reset System log");
                               this.statusBar1.Text = "Getting System Log ";
                               this.startDIThread("requestLogWrapper", requestLogWrapper);
                            break;
                        #endregion reset system log

                        case "getKeyHoursWrapper":
                            #region get key hours
                            if (SystemInfo.errorSB.Length == 0)
                            {
                                this.statusBar1.Text = "Getting Key minutes and seconds";
                                this.startDIThread("getKeyMinsSecsWrapper", getKeyMinsSecsWrapper);
                            }
                            else
                            {
                                label2.Text = "Key time: not available";
                                this.statusBar1.Text = "Failed to Read Key hours";
                                sysInfo.clearErrorSB();
                            }
                            break;
                        #endregion get key hours

                        case "getKeyMinsSecsWrapper":
                            #region get key mins/secs
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
                                this.statusBar1.Text = "Failed to Read Key hours";
                            }
                            
                            //switch our user feedback window back on
                            break;
                        #endregion get key mins/secs

                    }
                    #endregion specific thread handling
                }
				else
				{
					//increment progress bar
                    #region specific thread end handling
                    switch (threadName)
                    {
                        case "requestLogWrapper":
                            if ( this.progressBar1.Maximum != 0 )
				            {
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
					            this.progressBar1.Maximum = (int) Math.Min(0x7fffffff,  this.sysInfo.CANcomms.SDOReadDomainRxDataSize);//
				            }
                            break;
                    }
                    #endregion specific thread end handling
                }
			}
            #endregion DIThread
        }



        private void timer2_Tick(object sender, System.EventArgs e)
        {
            //give any System log/filter retrieval priority
            if (this.DIThread == null)
            {
                this.label2.Visible = true; ;
                //if for the hours sub does not exist then quitly ignore it
                if ((hoursSub != null) && (this.minsSecsSub != null))
                {
                    timer2.Interval = 10000; //after first time in extend period - controller only updates key time every 15 secs
                    this.statusBar1.Text = "Getting key hours";
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

        #endregion timer ticks

        #region System Log

        private void createLogTable()
        {
            this.statusBar1.Text = "filling log tables";
            SystemLogView = new DataView(systemlogTable);
            fillLogTable();
            SystemLogView.AllowNew = false; //prevents empty row at bottom of datagrid = which can casue array index out of bounds exceptions
            SystemLogView.ListChanged += new System.ComponentModel.ListChangedEventHandler(sysViewChangeHandler);
            createEventTableStyle();
            this.statusBar1.Text = "";
        }


		private void fillLogTable()
		{
            systemlogTable.Clear();
			DataRow row;
			for(int i = 0;i<systemLog.Length;i++)
			{
				int minsAndSecs, mins, secs;
				row = systemlogTable.NewRow();
				row[(int) (FIFOCol.Name)] = systemLog[ i ].description;
				row[(int) (FIFOCol.ID)] = getEventNo(systemLog[ i ].eventID);
				row[(int) (FIFOCol.Group)] = getGroupName(systemLog[ i ].eventID);
				minsAndSecs = (int)(((int)systemLog[ i ].minsAndSecs) * 15);
				mins = minsAndSecs / 60;
				secs = minsAndSecs % 60;
				row[(int) (FIFOCol.Time)] = systemLog[ i ].hours.ToString("00000") + ":" + mins.ToString("00") + "." + secs.ToString("00");
				row[(int) (FIFOCol.DB1)] = getDataByte(systemLog[ i ].db1);
				row[(int) (FIFOCol.DB2)] = getDataByte(systemLog[ i ].db2);
				row[(int) (FIFOCol.DB3)] = getDataByte(systemLog[ i ].db3);
				#region logging level column
				int ID = systemLog[ i ].eventID;
				row[(int) (FIFOCol.Level)] = (int) ((ID & logLevelMask)>> 10);
				#endregion
				systemlogTable.Rows.Add(row);
			}
			systemlogTable.AcceptChanges();
			this.SystemLogView.Sort = FIFOCol.Time.ToString() + " DESC";
		}

		private void sysViewChangeHandler(object sender,ListChangedEventArgs e )
		{
			//no action required at present
			//will be called when ever this table view is changed eg filter applied or 
			//datagrid is sorted. Leave in in for future stretch potential
		}

		#endregion System Log

		#region System Group Filtering

        private void createGroupFilterTable()
        {
            this.statusBar1.Text = "filling filter table";
            sysgroupfilterTable.ColumnChanged -= new System.Data.DataColumnChangeEventHandler(sysfilterTableChangeHandler);
            sysgroupfilterTable.Clear();
            if (filterView != null)
            {
                filterView.ListChanged -= new System.ComponentModel.ListChangedEventHandler(sysfilterViewChangeHandler);
            }
            fillFilterTable();
            filterView = new DataView(sysgroupfilterTable);
            filterView.AllowNew = false; //prevents empty row at bottom of datagrid = which can casue array index out of bounds exceptions
            filterView.ListChanged += new System.ComponentModel.ListChangedEventHandler(sysfilterViewChangeHandler);
            sysgroupfilterTable.ColumnChanged += new System.Data.DataColumnChangeEventHandler(sysfilterTableChangeHandler);
            createFilterTableStyle();
            this.statusBar1.Text = "";
        }
		private void fillFilterTable()
		{
			DataRow row;
			systemFilterArray = new bool[numGroupFilters]; //get zeroed when table is cleared due to zero length dataview
			for(ushort i = 0;i<numGroupFilters;i++)
			{
				row = sysgroupfilterTable.NewRow();
				systemFilterArray[i] = getFilterValue(i);
				row[(int) GroupFilters.Group] =  SCCorpStyle.SystemFifoGrpNames[i];
				row[(int) GroupFilters.GroupNo] = getGroupNo(i);
				row[(int) GroupFilters.saveEventsInGroup] = getFilterValue(i);
				sysgroupfilterTable.Rows.Add(row);
			}
			sysgroupfilterTable.AcceptChanges();
		}
		private void sysfilterTableChangeHandler(object sender, System.Data.DataColumnChangeEventArgs e)
		{
			this.dataGrid1.SuspendLayout();  //prevent exception if user continuously fast clicks on the cell
			sysgroupfilterTable.AcceptChanges();
			systemFilterArray = new bool[numGroupFilters];
			int i=0;
			foreach (DataRowView myRow in filterView)
			{
				systemFilterArray[i++] = (bool) (myRow[(int) (GroupFilters.saveEventsInGroup)]);
			}
			this.dataGrid1.ResumeLayout(); //calculations complete so we can allow the datagrid to re-draw the affected row
		}
		private void sysfilterViewChangeHandler(object sender,ListChangedEventArgs e )
		{
			this.dataGrid1.SuspendLayout();  //prevent exception if user continuously fast clicks on the cell
			systemFilterArray = new bool[this.filterView.Count];
			int i=0;
			foreach (DataRowView myRow in filterView)
			{
				systemFilterArray[i++] = (bool) (myRow[(int) (GroupFilters.saveEventsInGroup)]);
			}
			this.dataGrid1.ResumeLayout(); //calculations complete so we can allow the datagrid to re-draw the affected row
		}

		#endregion System Group Filtering


		#region user interaction zone
        private void submitBtn_Click(object sender, System.EventArgs e)
        {
            hideUserControls();
            switch (systemLogWindowState)
            {
                case SystemLogWindowStates.SYSTEMLOG:
                    #region future stretch potential
                    //If user interction is include din th efuture then we will need to switch off the additional event handlers
                    //whilst the log is being changed
                    //SystemLogView.ListChanged -= new System.ComponentModel.ListChangedEventHandler(sysViewChangeHandler);
                    #endregion future stretch potential
                    this.statusBar1.Text = "Requesting System Log Reset";
                    this.startDIThread("ResetSystemLogWrapper", ResetSystemLogWrapper);

                    #region future stretch potential
                    //and then switch them back on again 
                    //SystemLogView.ListChanged += new System.ComponentModel.ListChangedEventHandler(sysViewChangeHandler);
                    #endregion future stretch potential
                    break;

                case SystemLogWindowStates.SYSTEMLOGFILTERS:
                    if (sysLogFilterSub != null)
                    {
                        this.statusBar1.Text = "Updating System Log filters";
                        uint _Groupfilter = 0;
                        for (int i = 0; i < numGroupFilters; i++)
                        {
                            if ((bool)(sysgroupfilterTable.Rows[i][(int)GroupFilters.saveEventsInGroup]) == true)
                            {
                                uint temp = 1;
                                temp = temp << i;
                                _Groupfilter += temp;
                            }
                        }
                        Groupfilter = System.Convert.ToUInt16(_Groupfilter);
                        this.startDIThread("setGroupFiltersWrapper", setGroupFiltersWrapper);
                    }
                    else
                    {
                        this.statusBar1.Text = "System Log filters not available";
                    }
                    break;
            }
        }


		private void dataGrid1_Click(object sender, System.EventArgs e)
		{
			switch (systemLogWindowState)
			{
				case SystemLogWindowStates.SYSTEMLOG:
					//do nothing at present - no user interaction with this log
					break;

				case SystemLogWindowStates.SYSTEMLOGFILTERS:
					//this method is to force one-click toggling of hte boolean Monitoring column
					//normally the first mouse click sets the focus to the cell
					//and the second click toggles the boolean value.
					//This is mildly annoying for the user - Single click changeover has a better feel.
					//this function also ensure that the row is selected regardless of which cell was clicked
					//locate mouse position relative to the dataGrid
                    if (this.sysLogFilterSub != null)
                    {
                        Point pt = this.dataGrid1.PointToClient(Control.MousePosition);
                        DataGrid.HitTestInfo hti = this.dataGrid1.HitTest(pt);
                        //check mouse was clicked over a cell
                        int myCol = (int)GroupFilters.saveEventsInGroup;
                        if ((hti.Type == DataGrid.HitTestType.Cell) && (hti.Column == myCol) && this.sysInfo.systemAccess > 1)
                        {
                            this.dataGrid1[hti.Row, myCol] = !(bool)this.dataGrid1[hti.Row, myCol];  //force the toggle 
                            RefreshRow(hti.Row); // and force repaint of the row by invalidating it
                        }
                    }
					break;

			}
		}
		private void RefreshRow(int row)
		{
			try
			{
				Rectangle rect = this.dataGrid1.GetCellBounds(row, 0);  //just to get the row height
				int myTop  = (int) rect.Top;
				rect = new Rectangle(this.dataGrid1.Left, myTop, this.dataGrid1.Width, rect.Height);
				this.dataGrid1.Invalidate(rect);
			}
			catch(Exception ex)
			{
                SystemInfo.errorSB.Append("Exception number 0001: ");
                SystemInfo.errorSB.Append(ex.Message.ToString());
                SystemInfo.errorSB.Append(". Please restart application and report exception.\n");
                this.sysInfo.displayErrorFeedbackToUser("Unexpected exception.\n");
			} //empty catch
			finally
			{
				//no final actions required
			}

		}

		private void comboBox1_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			hideUserControls();
			switch (comboBox1.SelectedIndex)
			{
				case 0:  
					systemLogWindowState = SystemLogWindowStates.SYSTEMLOG;
                    if (this.systemLogSub != null)
                    {
                        this.statusBar1.Text = "Getting System Log";
                        this.startDIThread("requestLogWrapper", requestLogWrapper);
                    }
                    else
                    {
                        this.showUserControls();
                    }
					break;

				case 1: 
					systemLogWindowState = SystemLogWindowStates.SYSTEMLOGFILTERS;
                    if (this.sysLogFilterSub != null)
                    {
                        this.statusBar1.Text = "Getting System Log filters";
                        this.startDIThread("getGroupFiltersWrapper", getGroupFiltersWrapper);
                    }
                    else
                    {
                        this.showUserControls();
                    }
					break;
			}
		}
		#endregion

        #region hide/show user controls
        private void hideUserControls()
        {
            this.comboBox1.Enabled = false;
            this.submitBtn.Visible = false;
            this.dataGrid1.Enabled = false;
            this.dataGrid1.Click -= new System.EventHandler(this.dataGrid1_Click);
        }

        private void showUserControls()
        {
            label1.Text = "";
            this.dataGrid1.Enabled = true;  //enable once all data is in
            if (dataGrid1.TableStyles.Count > 0)
            {
                dataGrid1.TableStyles.Clear();
            }
            this.comboBox1.Enabled = true;
            switch (systemLogWindowState)
            {
                case SystemLogWindowStates.SYSTEMLOG:
                    #region system log
                    this.createLogTable();
                    this.dataGrid1.DataSource = this.SystemLogView;
                    SystemLogView.Sort = FIFOCol.Time.ToString() + " DESC";
                    applyEventTableStyle();
                    dataGrid1.CaptionText = "System log for " + selectedNodeText;
                    if (this.systemLogSub == null)
                    {
                        this.label1.Text = "Log not available";
                        this.statusBar1.Text = "Log not available";
                    }
                    else if (SystemInfo.errorSB.Length == 0) 
                    {
                        this.statusBar1.Text = "Displaying " + selectedNodeText + " System Log.";
                        if ((this.sysInfo.systemAccess >= 2) && (resetSysLogSub != null) && (this.systemLog.Length >0))
                        {
                            this.submitBtn.Text = "&Reset log";
                            this.submitBtn.Visible = true;
                            this.label1.Text = "View or reset the System Log.";
                            this.Text = "DriveWizard: System Log configuration";
                        }
                        else
                        {
                            if (this.systemLog.Length == 0)
                            {
                                this.label1.Text = "System Log is empty";
                            }
                            else
                            {
                                this.label1.Text = "View or reset the System Log.";
                            }
                            this.Text = "DriveWizard: System Log (read only)";
                        }
                    }
                    else
                    {
                        this.statusBar1.Text = "Unable to retrieve " + selectedNodeText + " System Log.";
                    }
                    this.label2.Visible = true;
                    this.dataGrid1.ReadOnly = true;

                    #endregion system log
                    break;

                case SystemLogWindowStates.SYSTEMLOGFILTERS:
                    #region system group filters
                    createGroupFilterTable();
                    dataGrid1.DataSource = this.filterView;
                    filterView.Sort = GroupFilters.GroupNo.ToString();
                    applyFilterTableStyle();
                    dataGrid1.CaptionText = "Group Filtering for " + selectedNodeText;
                    if (this.sysLogFilterSub == null)
                    {
                        this.label1.Text = "Filters not available";
                        this.statusBar1.Text = "Filters not available";
                        this.dataGrid1.ReadOnly = true;
                    }
                    else if (SystemInfo.errorSB.Length == 0)
                    {
                        this.statusBar1.Text = "Displaying Group filtering for " + selectedNodeText;
                        if (this.sysInfo.systemAccess < 2)
                        {
                            this.submitBtn.Visible = false;
                            this.Text = "DriveWizard: System Log Group filtering (read only)";
                        }
                        else
                        {
                            this.dataGrid1.Click += new System.EventHandler(this.dataGrid1_Click);
                            this.submitBtn.Text = "&Update Group Filtering";
                            this.submitBtn.Visible = true;
                            this.label1.Text = "Select events by Group to store in System Log. Click Update Group Filtering button to confirm selection";
                            this.Text = "DriveWizard: System Log Group filtering configuration";
                        }
                    }
                    else
                    {
                        this.statusBar1.Text = "Unable to retrieve Group filtering for " + selectedNodeText;
                        this.dataGrid1.ReadOnly = true;
                    }
                    this.label2.Visible = false;
                    #endregion system group filters
                    break;
            }
            if (SystemInfo.errorSB.Length > 0)
            {
                sysInfo.displayErrorFeedbackToUser("");
            }
        }

        #endregion hide/show user controls

        #region minor methods
        private void dataGrid1_Resize(object sender, System.EventArgs e)
		{
			switch (systemLogWindowState)
			{
				case SystemLogWindowStates.SYSTEMLOG:
					applyEventTableStyle();
					break;

				case SystemLogWindowStates.SYSTEMLOGFILTERS:
					applyFilterTableStyle();
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
			return SCCorpStyle.SystemFifoGrpNames[groupNo];
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
             if (sysLogFilterSub != null)
             {
                 temp = (uint)sysLogFilterSub.currentValue;
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
			filterTableStyle = new SysGroupFilterTableStyle(colWidths);
		}
		private void applyFilterTableStyle()
		{
			int [] colWidths  = new int[filterPercents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, filterPercents, 0, dataGrid1DefaultHeight);
			for (int i = 0;i<filterPercents.Length;i++)
			{
				filterTableStyle.GridColumnStyles[i].Width = colWidths[i];
			}
			//filterTableStyle.MappingName = sysgroupfilterTable.TableName;
			this.dataGrid1.TableStyles.Add(filterTableStyle);//finally attahced the TableStyles to the datagrid
		}
		private void createEventTableStyle()
		{
			int [] colWidths  = new int[eventPercents.Length];
			dataGrid1DefaultHeight = dataGrid1.Height;
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, eventPercents, 0, dataGrid1DefaultHeight);
			tablestyle = new SysFIFOTableStyle(colWidths);
		}
		private void applyEventTableStyle()
		{
			int [] colWidths  = new int[eventPercents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1 , eventPercents, 0,dataGrid1DefaultHeight);
			for (int i = 0;i<eventPercents.Length;i++)
			{
				tablestyle.GridColumnStyles[i].Width = colWidths[i];
			}
			this.dataGrid1.TableStyles.Add(tablestyle);//finally attach the TableStyles to the datagrid
		}

		#endregion minor methods

		#region finalisation/exit
		private void SYSTEM_LOG_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			statusBar1.Text = "Performing finalisation, please wait";
            //We need to 'remove' any failed hours messages in erroSB before exiting
            //do it here so we can still add Thread closure failure in if required
            sysInfo.clearErrorSB();
			#region disable all timers
			this.timer1.Enabled = false;
			this.timer2.Enabled = false;
			#endregion disable all timers
			#region stop all threads
			if(DIThread != null)
			{
				if((DIThread.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
				{
					DIThread.Abort();

					if(DIThread.IsAlive == true)
					{
#if DEBUG
						SystemInfo.errorSB.Append("Failed to close Thread: ");
                        SystemInfo.errorSB.Append(DIThread.Name);
                        SystemInfo.errorSB.Append(" on exit.\n");
#endif	
						DIThread = null;
					}

                    if (DIThread != null)
                    {
                        if ((DIThread.ThreadState & System.Threading.ThreadState.Stopped) == 0)
                        {
                            DIThread.Abort();

                            if (DIThread.IsAlive == true)
                            {
#if DEBUG
                                SystemInfo.errorSB.Append("Failed to close Thread: ");
                                SystemInfo.errorSB.Append(DIThread.Name);
                                SystemInfo.errorSB.Append(" on exit.\n");
#endif
                                DIThread = null;
                            }
                        }
                    }
				}
			}
			#endregion stop all threads
            systemLogWindowState = SystemLogWindowStates.NONE;
			e.Cancel = false; //force this window to close
		}
		private void SYSTEM_LOG_WINDOW_Closed(object sender, System.EventArgs e)
		{
			#region reset window title and status bar
			statusBar1.Text = "";
			#endregion reset window title and status bar	 		
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
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
	}

	#region System FIFO TableStyle
	/// <summary>
	/// Summary description for FIFOTableStyle.
	/// </summary>
	public class SysFIFOTableStyle :SCbaseTableStyle
	{
		public SysFIFOTableStyle(int [] ColWidths)
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
			SCbaseRODataGridTextBoxColumn EvNameCol = new SCbaseRODataGridTextBoxColumn((int)FIFOCol.Name);
			EvNameCol.MappingName = FIFOCol.Name.ToString();
			EvNameCol.HeaderText = "Event Name";
			EvNameCol.Width = ColWidths[(int)FIFOCol.Name];
			GridColumnStyles.Add(EvNameCol);
		}

		private void createEvIDCol(int [] ColWidths)
		{
			SCbaseRODataGridTextBoxColumn EvIDCol = new SCbaseRODataGridTextBoxColumn((int)FIFOCol.ID);
			EvIDCol.MappingName = FIFOCol.ID.ToString();
			EvIDCol.HeaderText = "Event";
			EvIDCol.Width = ColWidths[(int)FIFOCol.ID];
			EvIDCol.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(EvIDCol);
		}

		private void createEvGroupCol(int [] ColWidths)
		{
			SCbaseRODataGridTextBoxColumn c1 = new SCbaseRODataGridTextBoxColumn((int)FIFOCol.Group);
			c1.MappingName = FIFOCol.Group.ToString();
			c1.HeaderText = "Group";
			c1.Width = ColWidths[(int)FIFOCol.Group];
			c1.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(c1);
		}

		private void createEvTimeCol(int [] ColWidths)
		{
			SCbaseRODataGridTextBoxColumn firstTimeCol = new SCbaseRODataGridTextBoxColumn((int)FIFOCol.Time);
			firstTimeCol.MappingName = FIFOCol.Time.ToString();
			firstTimeCol.HeaderText = "Logged at"; 
			firstTimeCol.Width = ColWidths[(int)FIFOCol.Time];
			firstTimeCol.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(firstTimeCol);
		}
		private void createEvDByte1Col(int [] ColWidths)
		{
			SCbaseRODataGridTextBoxColumn EvDByte1Col = new SCbaseRODataGridTextBoxColumn((int)FIFOCol.DB1);
			EvDByte1Col.MappingName = FIFOCol.DB1.ToString();
			EvDByte1Col.HeaderText = "DB1";
			EvDByte1Col.Width = ColWidths[(int)FIFOCol.DB1];
			EvDByte1Col.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(EvDByte1Col);
		}
		private void createEvDByte2Col(int [] ColWidths)
		{
			//insert a tickbox column to allow user to indicate which parameters s/he wishes to monitor
			SCbaseRODataGridTextBoxColumn EvDByte2Col = new SCbaseRODataGridTextBoxColumn((int)FIFOCol.DB2);
			EvDByte2Col.MappingName = FIFOCol.DB2.ToString();
			EvDByte2Col.HeaderText = "DB2";
			EvDByte2Col.Width = ColWidths[(int)FIFOCol.DB2];
			EvDByte2Col.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(EvDByte2Col);
		}
		private void createEvDByte3Col(int [] ColWidths)
		{
			SCbaseRODataGridTextBoxColumn EvDByte3Col = new SCbaseRODataGridTextBoxColumn((int)FIFOCol.DB3);
			EvDByte3Col.MappingName = FIFOCol.DB3.ToString();
			EvDByte3Col.HeaderText = "DB3";
			EvDByte3Col.Width = ColWidths[(int)FIFOCol.DB3];
			EvDByte3Col.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(EvDByte3Col);
		}
		private void createEvLevelCol(int [] ColWidths)
		{
			SCbaseRODataGridTextBoxColumn col = new SCbaseRODataGridTextBoxColumn((int)FIFOCol.Level);
			col.MappingName = FIFOCol.Level.ToString();
			col.HeaderText = "Level";
			col.Width = ColWidths[(int)FIFOCol.Level];
			col.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(col);

		}
	}
	#endregion

	#region TableStyle for FIFO Group filtering class
	public class SysGroupFilterTableStyle : SCbaseTableStyle
	{
		public SysGroupFilterTableStyle(int [] ColWidths)
		{
			SysLogFormattableTextBoxColumn NameCol = new SysLogFormattableTextBoxColumn((int)GroupFilters.Group);
			NameCol.MappingName = GroupFilters.Group.ToString();
			NameCol.HeaderText = "Group Filter";
			NameCol.Width = ColWidths[(int)GroupFilters.Group];
			NameCol.ReadOnly = true;
			GridColumnStyles.Add(NameCol);

			SysLogFormattableTextBoxColumn GroupNo = new SysLogFormattableTextBoxColumn((int)GroupFilters.GroupNo);
			GroupNo.MappingName = GroupFilters.GroupNo.ToString();
			GroupNo.HeaderText = "Group Number";
			GroupNo.Width = ColWidths[(int)GroupFilters.GroupNo];
			GroupNo.ReadOnly = true;
			GridColumnStyles.Add(GroupNo);

			SysLogFormattableBooleanColumn SetCol = new SysLogFormattableBooleanColumn((int) GroupFilters.saveEventsInGroup);
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
	//formattable columns for logs
	#region logs SysLogFormattableTextBoxColumn
    public class SysLogFormattableTextBoxColumn : SCbaseRODataGridTextBoxColumn
    {
        public SysLogFormattableTextBoxColumn(int columnIndex)
            : base(columnIndex)
        {
        }
        //used to fire an event to retrieve formatting info and then draw the cell with this formatting info
        protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
        {
            switch (SYSTEM_LOG_WINDOW.systemLogWindowState)
            {
                case SystemLogWindowStates.SYSTEMLOGFILTERS:
                    if (rowNum < SYSTEM_LOG_WINDOW.systemFilterArray.Length)
                    {
                        if ((SYSTEM_LOG_WINDOW.systemFilterArray != null) && (SYSTEM_LOG_WINDOW.systemFilterArray[rowNum] == true))
                        {
                            backBrush = new SolidBrush(SCCorpStyle.dgRowSelected);
                        }
                    }
                    break;
            }
            base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
        }
    }

	#endregion logs SysLogFormattableTextBoxColumn

	#region Logs Formattable Bool Column
	public class SysLogFormattableBooleanColumn : SCbaseRODataGridBoolColumn
	{
		public SysLogFormattableBooleanColumn(int colNum): base (colNum)
		{
		}
		//used to fire an event to retrieve formatting info and then draw the cell with this formatting info
        protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
        {
            switch (SYSTEM_LOG_WINDOW.systemLogWindowState)
            {
                case SystemLogWindowStates.SYSTEMLOGFILTERS:
                    if (rowNum < SYSTEM_LOG_WINDOW.systemFilterArray.Length)
                    {
                        if ((SYSTEM_LOG_WINDOW.systemFilterArray != null) && (SYSTEM_LOG_WINDOW.systemFilterArray[rowNum] == true))
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
