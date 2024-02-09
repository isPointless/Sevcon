/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.135$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:29/09/2008 21:11:26$
	$ModDate:23/09/2008 21:21:50$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	Window class for formatting , display and user interface for 
    programming SEVOCN controller devcies

REFERENCES    

MODIFICATION HISTORY
    $Log:  36727: DEVICE_PROGRAMMING_WINDOW.cs 

   Rev 1.135    29/09/2008 21:11:26  ak
 Updates for CRR COD0013, ready for testing


   Rev 1.134    07/04/2008 21:43:42  ak
 Tidy up file stream & stream reader/writer


   Rev 1.133    25/03/2008 10:13:56  ak
 retrieveDataFromController() now allows retries. Testing found that peaks in
 slow PC loading could lead to CANUnknownCommandSpecifier being returned by
 the controller.


   Rev 1.132    19/03/2008 09:07:40  jw
 Copy DR38000244 into develpment code ( was in but moved to better location to
 ensure catch all scenarios)


   Rev 1.131    12/03/2008 12:59:54  ak
 All DI Thread.Priority increased to Normal from BelowNormal (needed to run
 VCI3).


   Rev 1.130    25/02/2008 16:27:30  jw
 Handling for new parameter NumAllowedBlockFails which is number of Sequnce
 Blaock fails that can occur before DW reverts to segmented download for
 remainder of the programming cycle. Trade off between sequence timeouts and
 slowed segmented transfer


   Rev 1.129    21/02/2008 15:03:44  jw
 Bug fix. Route found whereby DW can send WholeDataChecksum after
 condetransfer failed.. Now prevented


   Rev 1.128    15/02/2008 11:44:16  jw
 Reduncadnt code line removed to reduce compiler warnigns


   Rev 1.127    05/12/2007 22:13:00  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Text;


namespace DriveWizard
{
	#region enumerated type declarations
	public enum progCols {Parameter, Node, dldFile};  //used in both prog form a and prog data table creation classes
    
    //DR38000258 added generic memory spaces for programming compatability with DVT dld files
	public enum MemSpaces {HostIntROM, DSPROM, EEPROM, HostExtROM, GenMemSpace};			

    public enum DeviceParams      {HostAppVer,        DSPAppVer,           HostBootVer,         DSPBootVer,         HardwareVersion,             NCHardware ,     EEPROMformat,    DeviceProcs,         DeviceMemSpaces};
	public enum progStep {s01_GettingAppVers, s02_AppVersAvailable, s03_AppVersionsConfirmed, s04_JumpingToBoot, s05_waitingForInBootConfirm, s06_GettingBootVers, s07_BootVersAvailable, s08_SelectingDldFile, s09_ParsingDldFile,s10_updateDownloadfileCheck, s10A_UploadEEPROM, s10B_WritingEEPROMBackupfile, s11_codeDownload, s12_ProgrammingComplete, s13_WaitingForExit, None};
	public enum DldFileParams      {HostAppVer,          DSPAppVer,         spare1,              spare2,             CompHWBuilds              ,  NonCompHWBuilds,   EEPROMformat,   spare3,      NewCodeMemSpaces} ;
	public enum DldTestRes {HWTest,  MemTest, CanCovertEEProm, EEPROMSupplied, EEPROMFormatsMatch};

	#endregion enumerated type declarations

	#region form class
	/// <summary>
	/// Summary description for DEVICE_PROGRAMMING_WINDOW class
	/// This class is responsible for user interface for programming SEVCON devices
	/// Programming a device requires access level of 2 and for the device to be inpre-operational
	/// On completion of programming the user is required to re-establish COmms and Login
	/// since programmed device could have changed Node ID and will not recognise the user 
	/// as being logged in.
	/// </summary>
	public class DEVICE_PROGRAMMING_WINDOW : System.Windows.Forms.Form
	{
		#region form controls declarations		
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem programMi;
		private System.Windows.Forms.MenuItem viewEepromMi;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.Button continueBtn;
		private System.Windows.Forms.Button closeBtn;  //from OD
		private System.Windows.Forms.Button changeFileBtn;
		private System.Windows.Forms.Timer progTimer;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.CheckBox preserveEEContentsAndFormatChkBox;
		private System.Windows.Forms.CheckBox preserveEEContentsChkBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		#endregion

		#region my declarations
		private SEVCONProgrammer programmer = null;
		private string [] NodeParams = {"Host Application Version", "DSP Application Version", "Host Boot Version", "DSP Boot Version", "Node HW/compatible HW versions",   "Non-compatible HW" ,"EEPROM format", "Node Processors", "Memory Spaces"};
		private progStep Prog_Step = progStep.None;
		private SystemInfo sysInfo;
		private ProgTable table;
		string DldFilename = "";
		private DIFeedbackCode feedback;
		Thread dataThread;
		private bool programmedOK = true;
		private string EEPROMSaveFile = "";
		private string [] downloadParams = null;
		private bool [] DldFiletestResults = null;
		private string hostAppController = "", DSPAppController = "";  //used for writing EEPROM backup file
		private string [] controllerBootParams = null;
		private ushort errorHandlingLevel = 0;  //determin how much error handling of code goes on - zero is no error handling
		private int CANDeviceIndex = 0;
		bool formClosing = false;
		private System.Windows.Forms.StatusBar statusBar1;
		int dataGrid1DefaultHeight = 0;
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
			this.label1 = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.progTimer = new System.Windows.Forms.Timer(this.components);
			this.continueBtn = new System.Windows.Forms.Button();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.closeBtn = new System.Windows.Forms.Button();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.programMi = new System.Windows.Forms.MenuItem();
			this.viewEepromMi = new System.Windows.Forms.MenuItem();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.label2 = new System.Windows.Forms.Label();
			this.changeFileBtn = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.preserveEEContentsAndFormatChkBox = new System.Windows.Forms.CheckBox();
			this.preserveEEContentsChkBox = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// dataGrid1
			// 
			this.dataGrid1.AllowSorting = false;
			this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGrid1.BackgroundColor = System.Drawing.Color.White;
			this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dataGrid1.CaptionBackColor = System.Drawing.Color.MidnightBlue;
			this.dataGrid1.CausesValidation = false;
			this.dataGrid1.DataMember = "";
			this.dataGrid1.FlatMode = true;
			this.dataGrid1.ForeColor = System.Drawing.Color.MidnightBlue;
			this.dataGrid1.HeaderForeColor = System.Drawing.Color.MidnightBlue;
			this.dataGrid1.Location = new System.Drawing.Point(8, 120);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.ParentRowsForeColor = System.Drawing.Color.MidnightBlue;
			this.dataGrid1.PreferredColumnWidth = 300;
			this.dataGrid1.PreferredRowHeight = 15;
			this.dataGrid1.ReadOnly = true;
			this.dataGrid1.RowHeadersVisible = false;
			this.dataGrid1.RowHeaderWidth = 80;
			this.dataGrid1.Size = new System.Drawing.Size(1040, 324);
			this.dataGrid1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(21, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(704, 81);
			this.label1.TabIndex = 8;
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(0, 594);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(1065, 30);
			this.progressBar1.Step = 0;
			this.progressBar1.TabIndex = 10;
			this.progressBar1.Visible = false;
			// 
			// progTimer
			// 
			this.progTimer.Interval = 50;
			this.progTimer.Tick += new System.EventHandler(this.progTimer_Tick);
			// 
			// continueBtn
			// 
			this.continueBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.continueBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.continueBtn.Location = new System.Drawing.Point(11, 553);
			this.continueBtn.Name = "continueBtn";
			this.continueBtn.Size = new System.Drawing.Size(106, 32);
			this.continueBtn.TabIndex = 11;
			this.continueBtn.Text = "C&ontinue";
			this.continueBtn.Click += new System.EventHandler(this.continue1_btn_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
			// 
			// closeBtn
			// 
			this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.closeBtn.Location = new System.Drawing.Point(858, 553);
			this.closeBtn.Name = "closeBtn";
			this.closeBtn.Size = new System.Drawing.Size(192, 32);
			this.closeBtn.TabIndex = 12;
			this.closeBtn.Text = "&Cancel programming";
			this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.programMi});
			// 
			// programMi
			// 
			this.programMi.Index = 0;
			this.programMi.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.viewEepromMi});
			this.programMi.Text = "&File";
			// 
			// viewEepromMi
			// 
			this.viewEepromMi.Enabled = false;
			this.viewEepromMi.Index = 0;
			this.viewEepromMi.Text = "&View uploaded EEPROM parameters";
			this.viewEepromMi.Click += new System.EventHandler(this.viewEepromMi_Click);
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Location = new System.Drawing.Point(11, 472);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(1039, 32);
			this.label2.TabIndex = 15;
			// 
			// changeFileBtn
			// 
			this.changeFileBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.changeFileBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.changeFileBtn.Location = new System.Drawing.Point(677, 553);
			this.changeFileBtn.Name = "changeFileBtn";
			this.changeFileBtn.Size = new System.Drawing.Size(170, 32);
			this.changeFileBtn.TabIndex = 16;
			this.changeFileBtn.Text = "Change &File";
			this.changeFileBtn.Visible = false;
			this.changeFileBtn.Click += new System.EventHandler(this.changeFileBtn_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.preserveEEContentsAndFormatChkBox);
			this.panel1.Controls.Add(this.preserveEEContentsChkBox);
			this.panel1.Location = new System.Drawing.Point(442, 10);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(608, 101);
			this.panel1.TabIndex = 19;
			// 
			// preserveEEContentsAndFormatChkBox
			// 
			this.preserveEEContentsAndFormatChkBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.preserveEEContentsAndFormatChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.preserveEEContentsAndFormatChkBox.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.preserveEEContentsAndFormatChkBox.Location = new System.Drawing.Point(11, 51);
			this.preserveEEContentsAndFormatChkBox.Name = "preserveEEContentsAndFormatChkBox";
			this.preserveEEContentsAndFormatChkBox.Size = new System.Drawing.Size(586, 30);
			this.preserveEEContentsAndFormatChkBox.TabIndex = 21;
			this.preserveEEContentsAndFormatChkBox.Text = "Preserve device EEPROM format and contents";
			this.preserveEEContentsAndFormatChkBox.Visible = false;
			// 
			// preserveEEContentsChkBox
			// 
			this.preserveEEContentsChkBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.preserveEEContentsChkBox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.preserveEEContentsChkBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.preserveEEContentsChkBox.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.preserveEEContentsChkBox.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
			this.preserveEEContentsChkBox.Location = new System.Drawing.Point(11, 10);
			this.preserveEEContentsChkBox.Name = "preserveEEContentsChkBox";
			this.preserveEEContentsChkBox.Size = new System.Drawing.Size(586, 41);
			this.preserveEEContentsChkBox.TabIndex = 19;
			this.preserveEEContentsChkBox.Text = "Preserve device EEPROM contents but update format";
			this.preserveEEContentsChkBox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.preserveEEContentsChkBox.Visible = false;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label3.Location = new System.Drawing.Point(8, 513);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(1039, 31);
			this.label3.TabIndex = 20;
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 619);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(1056, 30);
			this.statusBar1.TabIndex = 21;
			this.statusBar1.Text = "statusBar1";
			// 
			// DEVICE_PROGRAMMING_WINDOW
			// 
			this.AcceptButton = this.continueBtn;
			this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
			this.ClientSize = new System.Drawing.Size(1056, 649);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.changeFileBtn);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.closeBtn);
			this.Controls.Add(this.continueBtn);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.dataGrid1);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Menu = this.mainMenu1;
			this.Name = "DEVICE_PROGRAMMING_WINDOW";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Node Programming";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DEVICE_PROGRAMMING_WINDOW_Closing);
			this.Load += new System.EventHandler(this.DEVICE_PROGRAMMING_WINDOW_Load);
			this.Closed += new System.EventHandler(this.DEVICE_PROGRAMMING_WINDOW_Closed);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
		/*--------------------------------------------------------------------------
		 *  Name			: DEVICE_PROGRAMMING_WINDOW()
		 *  Description     : Constructor function for form. Set up of any initial variables
		 *					  that are available prior to th eform load event.
		 *  Parameters      : systemInfo class, CANopen node number and a descriptive
		 *					  string about the current CANopen node.
		 *  Used Variables  : none
		 *  Preconditions   : This form is only available when at least one SEVCON node is 
		 *					  connected.  SEVCON nodes can be running application or BOOTLOADER code
		 *  Return value    : none
		 *--------------------------------------------------------------------------*/
		public DEVICE_PROGRAMMING_WINDOW(SystemInfo passed_systemInfo, int nodeNum)
		{
			InitializeComponent();
			this.sysInfo = passed_systemInfo;
			this.sysInfo.formatDataGrid(this.dataGrid1);
			this.CANDeviceIndex = nodeNum;
			programmer = new SEVCONProgrammer(CANDeviceIndex, sysInfo, errorHandlingLevel);
			this.dataGrid1.CaptionText = "Programming node " + this.sysInfo.nodes[nodeNum].nodeID.ToString();
			this.sysInfo.CANcomms.setCommsTimeout( SCCorpStyle.TimeoutDSPProgramming );
			this.progressBar1.Value = this.progressBar1.Minimum;
			createInitialTable();
			Prog_Step = progStep.s01_GettingAppVers;
			this.Text = "DriveWizard: SEVCON node software update";
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
		///		 *  Name			: DEVICE_PROGRAMMING_WINDOW_Load
		///		 *  Description     : Event HAndler for form load event
		///		 *  Parameters      : System event arguments 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void DEVICE_PROGRAMMING_WINDOW_Load(object sender, System.EventArgs e)
		{
			dataGrid1DefaultHeight = this.dataGrid1.Height;
			float [] percents = {0.3F, 0.35F, 0.35F};
			int [] colWidths  = new int[percents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, percents, 0, dataGrid1DefaultHeight);
			ProgTableStyle tablestyle = new ProgTableStyle(colWidths);
			this.dataGrid1.TableStyles.Add(tablestyle);
			startProgrammingprocess();
		}

		private void startProgrammingprocess()
		{
			SystemInfo.errorSB.Length = 0; //effectively reset it
			#region data retrieval thread start
			dataThread = new Thread(new ThreadStart( getAppsVerisonsWrapper )); 
			dataThread.Name = "Get Application Verisons";
            dataThread.IsBackground = true;
            dataThread.Priority = ThreadPriority.Normal;

#if CAN_TRAFFIC_DEBUG
			System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
			this.progressBar1.Visible = true;
			dataThread.Start(); 
			this.progTimer.Enabled = true;
			#endregion
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: createInitialTable
		///		 *  Description     : Creates and fills the device info table with initial data
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
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
			dataGrid1.DataSource = this.table;
		}
		#endregion

		#region main user interaction zone
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: progTimer_Tick
		///		 *  Description     : Event handler for the main form timer. The threads ofr all 'automatic'
		///		 *						actions are started and checked here. Threads which the user starts 
		///		 *						are kicked off on the Continue button event handler
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void progTimer_Tick(object sender, System.EventArgs e)
		{
			this.progTimer.Enabled = false;
			bool timerStopFlag = false;
			if (dataThread != null)
			{
				switch (Prog_Step)
				{
					case progStep.s01_GettingAppVers:
						#region getting app versions if available
						if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
						{
							timerStopFlag = true; //knock the timer off becasue user confirm is required
							this.table.Rows[(int) (DeviceParams.HostAppVer)][(int)(progCols.Node)] = hostAppController; 
							this.table.Rows[(int) (DeviceParams.DSPAppVer)][(int) (progCols.Node)] = DSPAppController;
							Prog_Step = progStep.s02_AppVersAvailable;
						}
						#endregion getting app versions if available
						break;

					//progStep.s02_AppVersAvailable 
					//is completely handled by Continue button

					case progStep.s03_AppVersionsConfirmed:
						#region thread for retrieving Bootloader EDS informnation
						Prog_Step = progStep.s06_GettingBootVers;  //go to next step before setting the thread away
						dataThread = new Thread(new ThreadStart( getBootEDSWrapper )); 
						dataThread.Name = "Get EDS Info";
                        dataThread.IsBackground = true;
                        dataThread.Priority = ThreadPriority.Normal;
#if CAN_TRAFFIC_DEBUG
						System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
						dataThread.Start(); 
						#endregion thread for retrieving Bootloader EDS informnation
						break;

					case progStep.s04_JumpingToBoot:
						#region jumping to boot
						this.Prog_Step = progStep.s05_waitingForInBootConfirm;
						#region data retrieval thread start
						dataThread = new Thread(new ThreadStart( setBootModeWrapper)); 
						dataThread.Name = "Force into Boot mode";
                        dataThread.IsBackground = true;
                        dataThread.Priority = ThreadPriority.Normal;
#if CAN_TRAFFIC_DEBUG
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

					case progStep.s06_GettingBootVers:
						#region Thread for retrieving general bootloader information
						if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
						{
							Prog_Step = progStep.s07_BootVersAvailable;
							dataThread = new Thread(new ThreadStart( getBootInfoWrapper )); 
							dataThread.Name = "Get Boot Info";
                            dataThread.IsBackground = true;
                            dataThread.Priority = ThreadPriority.Normal;
#if CAN_TRAFFIC_DEBUG
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
							this.continueBtn.Visible = true;
							timerStopFlag = true;
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
#if CAN_TRAFFIC_DEBUG
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
							setupCheckBoxDisplays();
							this.changeFileBtn.Text = "Change file";
							this.changeFileBtn.Visible = true;
							this.continueBtn.Visible = true;
							timerStopFlag= true;
						}
						#endregion waiting for checking downlaod file to complete
						break;

					case progStep.s10A_UploadEEPROM:
						#region EEPROM upload
						#region disable checkboxes and datagrid once EEPROM upload starts
						this.preserveEEContentsAndFormatChkBox.Enabled = false;
						this.preserveEEContentsChkBox.Enabled = false;
						//now read the values and translate to no GUI parameters
						programmer._preserveEEContentsAndFormat = this.preserveEEContentsAndFormatChkBox.Checked;
						programmer._preserveEEPROMContents = preserveEEContentsChkBox.Checked;
						this.dataGrid1.Enabled = false;
						#endregion disable checkboxes once programming starts

						Prog_Step = progStep.s10B_WritingEEPROMBackupfile;
						//now start the upload thread off
						this.viewEepromMi.Enabled = true;
						dataThread = new Thread(new ThreadStart( programmer.uploadEEPROM )); 
						dataThread.Name = "Uploading EEPROM";
                        dataThread.IsBackground = true;
                        dataThread.Priority = ThreadPriority.Normal;
#if CAN_TRAFFIC_DEBUG
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
								EEPROMSaveFile = MAIN_WINDOW.UserDirectoryPath + @"\DLD\EEPROMbackup.dld";// .....use a default file name
								this.statusBar1.Text = "EE Backup to: " + EEPROMSaveFile;
								programmer.writeEEPROMDataToBackupFile(EEPROMSaveFile);
								this.label3.Text = "EEPROM Backup file: " + EEPROMSaveFile;
							//now move to next step
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
#if CAN_TRAFFIC_DEBUG
							System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
							dataThread.Start(); 
						#endregion code download
						break;

					case progStep.s12_ProgrammingComplete:
						#region programming complete
                        if ((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0)
                        {
                            dataThread = new Thread(new ThreadStart(completeDownloadWrapper));
                            dataThread.Name = "Completing download";
                            dataThread.IsBackground = true;
                            dataThread.Priority = ThreadPriority.Normal;
#if CAN_TRAFFIC_DEBUG
								System.Console.Out.WriteLine("Thread: " + dataThread.Name + " started");
#endif
                            dataThread.Start();
                            Prog_Step = progStep.s13_WaitingForExit;
                        }
						#endregion programming complete
						break;

					case progStep.s13_WaitingForExit:
						#region waiting for exit
						if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
						{
							timerStopFlag = true; //kill this timer becasue we are all done
							if(programmer.fatalErrorOccured == false)
							{
								this.programmedOK = true;  //for closing event
								this.closeBtn.Text = "&Close this window";
								SEVCONProgrammer.statusMessage =  "Programmed OK";
								if(SystemInfo.errorSB.Length>0)
								{
									this.sysInfo.displayErrorFeedbackToUser("\nNon Fatal error occurred during programming");
								}
							}
							SEVCONProgrammer.userInstructionMessage = "Close this window";
						}
						#endregion waiting for exit
						break;

					default:
						break;
				}
			}
			if((timerStopFlag == false) && (this.formClosing == false) && (programmer.fatalErrorOccured == false))
			{
				this.progTimer.Enabled = true;
			}
			else if(timerStopFlag == true)
			{
				this.progressBar1.Visible = false;
			}
			#region user feedback update
			this.progressBar1.Minimum = SEVCONProgrammer.progBarMin;
			this.progressBar1.Maximum = SEVCONProgrammer.progBarMax;
			this.progressBar1.Value = SEVCONProgrammer.progBarValue;
			this.label1.Text = SEVCONProgrammer.userInstructionMessage;
			this.statusBar1.Text =  SEVCONProgrammer.statusMessage;
			#endregion user feedback update
			if(programmer.fatalErrorOccured == true)
			{
				this.processErrorString();
			}
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: progTimer_Tick
		///		 *  Description     : Event handler for the Continue button. Threads which require user cofirmation 
		///		 *						are kicked off on in this function and the timer started 
		///		 *						to allow completion of these threads to be checkied in the timer Event handler
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void continue1_btn_Click(object sender, System.EventArgs e)
		{
			if(Prog_Step == progStep.s02_AppVersAvailable)
			{
				#region user has opted to enter boot mode
				feedback = sysInfo.nodes[CANDeviceIndex].readDeviceIdentity( );
				if(feedback != DIFeedbackCode.DISuccess)
				{
					SystemInfo.errorSB.Insert(0, "\nFailed to read node identity parameters");
					programmer.fatalErrorOccured = true;
					this.processErrorString();
				}
				if(sysInfo.nodes[CANDeviceIndex].productVariant != SCCorpStyle.bootloader_variant ) 
				{
					Prog_Step = progStep.s04_JumpingToBoot;
				}
				else 
				{
					Prog_Step = progStep.s03_AppVersionsConfirmed;  //we are alreading in bootloader so move on
				}
				this.continueBtn.Visible = false;
				this.progressBar1.Visible = true;
				this.progTimer.Enabled = true;
				#endregion user has opted to enter boot mode
			}
			else if (Prog_Step == progStep.s07_BootVersAvailable) 
			{
				#region user has opted to select a download file
				//if we get this far then we know tha twe are in boot so set nodeState
				//to UNKONW to allow us to see the application boot up message - DI
				//only flags changfes in NodeState hence we set to UNKNOWN now ready for
				//programming end
				this.sysInfo.nodes[CANDeviceIndex].nodeState = NodeState.Unknown;

				Prog_Step = progStep.s08_SelectingDldFile;
				#region selecting download file
				this.statusBar1.Text = "Displaying download files";
				//put this line before the setting InitalDirectory to maintain starting point on directory structure 
				if(Directory.Exists(MAIN_WINDOW.UserDirectoryPath + @"\DLD") == false)
				{
					Directory.CreateDirectory(MAIN_WINDOW.UserDirectoryPath + @"\DLD");
				}
				openFileDialog1.RestoreDirectory = true;
				openFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\DLD";
				openFileDialog1.Title = "Open download file";
				openFileDialog1.Filter = "download files (*.dld)|*.dld" ;
				this.continueBtn.Visible = false;
				#endregion
				string oldDldFilename = DldFilename;
				DldFilename = "";
				openFileDialog1.ShowDialog(this);
				if(this.DldFilename != "") //user has selected valid new filename
				{
					//next step is automatic so Timer on and user button off
					this.changeFileBtn.Visible = false;
					this.progressBar1.Visible = true;
					this.progTimer.Enabled = true;
					this.continueBtn.Visible = true;
				}
				else
				{
					DldFilename = oldDldFilename;  //revert to previous
					this.label1.Text = "Select a download file";
					this.changeFileBtn.Text = "Select file";
					this.changeFileBtn.Visible = true;
					this.statusBar1.Text = "";
				}
				#endregion user has opted to select a download file
			}
			else if (Prog_Step == progStep.s10_updateDownloadfileCheck)
			{
				#region user has opted to download code from selected downlaod file
				this.changeFileBtn.Visible = false;
				this.continueBtn.Visible = false;  
				Prog_Step = progStep.s10A_UploadEEPROM;
				this.progressBar1.Visible = true;
				this.progTimer.Enabled = true;
				#endregion user has opted to download code from selected downlaod file
			}
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: changeFileBtn_Click
		///		 *  Description     : Event handler for button that allows the user to change a selected file.
		///		 *						This button is only viisible when such a choice is valid
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void changeFileBtn_Click(object sender, System.EventArgs e)
		{
			this.label1.Text = "";
			for(int i=0 ; i<this.table.Rows.Count ; i++)
			{
				this.table.Rows[i][(int) (progCols.dldFile)] = "";  //reset table
			}
			Prog_Step = progStep.s08_SelectingDldFile;
			this.statusBar1.Text = "Displaying download files";
			#region selecting download file
			//put this line before the setting InitalDirectory to maintain starting point on directory structure 
			openFileDialog1.RestoreDirectory = true;
			if(Directory.Exists(MAIN_WINDOW.UserDirectoryPath + @"\DLD") == false)
			{
				try
				{
					Directory.CreateDirectory(MAIN_WINDOW.UserDirectoryPath + @"\DLD");
				}
				catch {}
			}
			openFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\DLD";
			openFileDialog1.Title = "Open download file";
			openFileDialog1.Filter = "download files (*.dld)|*.dld" ;
			string oldDldFilename = DldFilename;
			DldFilename = "";// reset the file name
			DialogResult result = openFileDialog1.ShowDialog(this);
			this.changeFileBtn.Visible = true;  //incase user does not select file

            //DR38000246 revert to old filename if one was selected when the user cancels
            if ((result == DialogResult.Cancel) && (oldDldFilename != ""))
            {
                DldFilename = oldDldFilename;
            }

			if (this.DldFilename != "")
			{
				this.statusBar1.Text = "Parsing selected download file";
				#endregion
				//next step is automatic so Timer on and user button off
				this.progressBar1.Value = this.progressBar1.Minimum;
				this.progressBar1.Visible = true;
				this.progTimer.Enabled = true;
				this.continueBtn.Visible = false;
				this.changeFileBtn.Visible = false;  //swithc off while this file is being read
                Prog_Step = progStep.s09_ParsingDldFile; //DR38000246 move onto parsing step
			}
			else
			{
				DldFilename = oldDldFilename;  //revert to previous
				this.continueBtn.Visible = false;
			}
		}
		#endregion

		#region Selecting and Reading Downloadfile
		private void compareDldFileToDeviceWrapper()
		{
			programmer.CompareDLDFileToDevice(DldFilename, out downloadParams, out DldFiletestResults);
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: openFileDialog1_FileOk
		///		 *  Description     : Event handler fired when the user confirms file seelction in the 
		///		 *						file open dilog 1. Note this event is fired on user pressing OK - so
		///		 *						empty file strings mut be handled
		///		 *  Parameters      : system event argumnents
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>

		private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			DldFilename = openFileDialog1.FileName;
			label2.Text = "Download file: " + DldFilename;
			this.progTimer.Enabled = true; //force
			Prog_Step = progStep.s09_ParsingDldFile;
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: rewind
		///		 *  Description     : Method to move file readers an dwriters bajc to start of the open file.	
		///		 *  Parameters      : system event argumnents
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		#endregion Selecting and Reading Downloadfile

		#region code transmission 
		private void getAppsVerisonsWrapper()
		{
			programmer.getAppVersions(out hostAppController, out DSPAppController);
			DriveWizard.SEVCONProgrammer.EDSFileChanged = false; //needed because this is set true in 
			//programmer for the benefit of self char - forces user to reconnect on exit
			//whereas at this point we know that our EDS will still be the app EDS
			//so the finalisation code retains full integrity - when EDS file AHS changed
			//we will still pick it up
		}
		private void setBootModeWrapper()
		{
			this.programmedOK = false;  //once we request entering bootloader then we will hav eto reconnect on exit
			programmer.setBootMode(CANDeviceIndex); //DR38000172 need nodeID to turn on SDO filtering
		}
		private void getBootInfoWrapper()
		{
			programmer.getBootInfo(out controllerBootParams);

		}
		private void getBootEDSWrapper()
		{
			programmer.getInfoFromBootloaderEDS();
		}

		private void codeTransferWrapper()
		{
			programmer.downloadCodeControl(DldFilename);
		}
		private void completeDownloadWrapper()
		{
			programmer.completeDownloadControl();
		}
		#endregion code transmission 

		#region viewing/saving EEPROM
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: saveFileDialog1_FileOk
		///		 *  Description     : Event Handler for closure of the EEPROM backup file selection dialog. 
		///		 *					: Note this event is fired even if user did not select a filename so this scenario must be handled
		///		 *  Parameters      : Esystem event arguments
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : 
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.EEPROMSaveFile = saveFileDialog1.FileName;  //store user requested filename
			this.progTimer.Enabled = true;  //set the timer away again to start downloading code
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: viewEepromMi_Click
		///		 *  Description     : Event handler for user request to view the current EEPROM backup file
		///		 *					: Opens the bakcup file in text format
		///		 *  Parameters      : system event arguments
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : 
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void viewEepromMi_Click(object sender, System.EventArgs e)
		{
			try 
			{
				// Call the Process.Start method to open the default browser with a URL:
				System.Diagnostics.Process.Start(EEPROMSaveFile);
			}
			catch (System.Security.SecurityException err)
			{
				SystemInfo.errorSB.Append("\nOperating System was unable to open requested file");
				SystemInfo.errorSB.Append("\n Operating system reported ");
				SystemInfo.errorSB.Append( err.Message);
				this.sysInfo.displayErrorFeedbackToUser("Failed to open EEPROM backup file");
			}
		}
		#endregion viewing/saving EEPROM

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
            // DR3800172 leaving prog window so set IXXAT filters/masks back to all CANIDs
            // Turn filters off since programming is now finished
            // acceptance mask      = XXXX XXXX XXXX XXXX (X=don't care)
            // acceptance filter    = 0000 0000 0000 0000
            // ie accept IDs of     = XXXX XXXX XXXX XXXX (all CANIDs)
            sysInfo.CANcomms.VCI.SetupCAN(sysInfo.CANcomms.systemBaud, 0x00, 0x00);

			this.Close(); 
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: Dispose
		///		 *  Description     : Disposes of this class
		///		 *  Parameters      : system generated
		///		 *  Used Variables  : none
		///		 *  Preconditions   : none - any will be dealt with in a window closing event handler 
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
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
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: DEVICE_PROGRAMMING_WINDOW_Closing
		///		 *  Description     : Event handler fired when this window is closing (but not yet closed)
		///		 *					: Detemines programming state and issues warnings as necessary to the user. 
		///		 *					: If user confirms exit then tries to pull controller back to working state (if possible) 
		///		 *					: If user cancels exit then cancles the closing event.
		///		 *					:		 
		///		 *  Parameters      : system generated
		///		 *  Used Variables  : none
		///		 *  Preconditions   : none - any will be dealt with in a window closing event handler 
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void DEVICE_PROGRAMMING_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
				if(DriveWizard.SEVCONProgrammer.CodeHasBeenSent == true)
				{
					if(programmer.fatalErrorOccured == false)  //no errors haver occurred
					{
						string message = "Data has been overwritten. This SEVCON node be unusable unless programming is completed. Are you sure?";
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
				}
				else  //no code downloaded yet
				{
					if(programmer.fatalErrorOccured == false)
					{
						DialogResult result2 = Message.Show(this, "SEVCON Node has not been updated. Are you sure?",
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
					#region  either error occurer before code sent or user wants to quit - try to force application to start
					if (this.sysInfo.nodes[CANDeviceIndex].productVariant == SCCorpStyle.bootloader_variant)
					{
						//attempt to pull the node out of programming mode by writing  zero wholeDataChecksum to it
						programmer.forceNodeOutOfBoot();
					}
					#endregion either error occurer before code sent or user wants to quit - try to force application to start
				}
			}
			//if we get this far then this window is definately closing - so tidy up and get out
			e.Cancel = false;   //will force this window to close on ending this function
			this.statusBar1.Text = "Performing finalisation, please wait";
			#region dispose of Programming component
			if(this.programmer != null)
			{
				programmer.Dispose();
			}
			#endregion dispose of Programming component
			//stop any active threads
			#region stop all threads
			if(this.dataThread != null)
			{
				if((dataThread.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
				{
					dataThread.Abort();  //stop any data threads

					if(dataThread.IsAlive == true)
					{
#if DEBUG
						SystemInfo.errorSB.Append("\nFailed to close Thread: " + dataThread.Name + " on exit");
#endif
						dataThread = null;
					}
				}
			}
			#endregion stop all threads
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
				feedback = sysInfo.nodes[CANDeviceIndex].readDeviceIdentity();
				if((this.sysInfo.nodes[CANDeviceIndex].productVariant == SCCorpStyle.bootloader_variant )
					||  (feedback != DIFeedbackCode.DISuccess))  
				{
					MAIN_WINDOW.reEstablishCommsRequired = true;  //if controller could be in boot then we have to -reestablish CAN comms
				}
			}
			e.Cancel = false; //force this window to close
		}
		private void DEVICE_PROGRAMMING_WINDOW_Closed(object sender, System.EventArgs e)
		{
			#region reset window title and status bar
			this.statusBar1.Text = "";
			#endregion reset window title and status bar		
		}
		#endregion

		#region Utils, error handling and GUI functions
		private void processErrorString()
		{
			this.progTimer.Enabled = false;
			bool TODO = true;
			if(TODO)//will be TODO Fatal programming error
			{
                // DR38000172 Turn filters off since programming aborted
                sysInfo.CANcomms.VCI.SetupCAN(sysInfo.CANcomms.systemBaud, 0x00, 0x00);
			    SystemInfo.errorSB.Append("\nThis Window will close");
			    this.sysInfo.displayErrorFeedbackToUser("");
			    this.Close();  //now close this window
			    return;
			}
			else
			{
				this.sysInfo.displayErrorFeedbackToUser("Programmed OK. Minor problems seen when programming");
			}
			return;
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
		private void setupCheckBoxDisplays()
		{
			this.preserveEEContentsAndFormatChkBox.Text = "Preserve device EEPROM format and contents";
			this.preserveEEContentsChkBox.Text = "Preserve device EEPROM contents but update format";
			this.preserveEEContentsChkBox.Visible = true;
			this.preserveEEContentsAndFormatChkBox.Visible = true;
			#region EEPROM data is contained in Download pack
			//now handle them output test results:
			if(DldFiletestResults[(int) DldTestRes.EEPROMSupplied] == true)
			{  //give user choice of what to do about eeprom
				this.preserveEEContentsChkBox.Checked = false;  //default to false
				this.preserveEEContentsChkBox.Enabled = true;
				this.preserveEEContentsAndFormatChkBox.Checked = false;  //default to false
				this.preserveEEContentsAndFormatChkBox.Enabled = true;
				if((DldFiletestResults[(int) DldTestRes.EEPROMFormatsMatch] == true) 
					|| (DldFiletestResults[(int) DldTestRes.CanCovertEEProm] == false))
				{
					this.preserveEEContentsAndFormatChkBox.Visible = false;
					this.preserveEEContentsChkBox.Text = "Preserve EEPROM in device (do not download from file)";
				}
			}
			#endregion EEPROM data is contained in Download pack
			#region no EEPROM data in Download pack
			else  //there is no EEPROM code in the download pack - but there may be a new format specified
			{
				if(this.DldFiletestResults[(int)DldTestRes.EEPROMFormatsMatch] == true)
				{  //No changes can be doen to device - so no user choices available - but need to set defaults for use later
					this.preserveEEContentsAndFormatChkBox.Checked = true;
					this.preserveEEContentsAndFormatChkBox.Visible = false;
					this.preserveEEContentsChkBox.Checked = true;  
					this.preserveEEContentsChkBox.Visible = false; 
				}
				else
				{
					if(this.DldFiletestResults[(int)DldTestRes.CanCovertEEProm] == false)
					{
						//there is nothing we can do with the EEPROM - the formats don't match but 
						this.preserveEEContentsChkBox.Checked = true;  //B&B  have to preserve EEPROM contents
						this.preserveEEContentsChkBox.Enabled =  false; 
						this.preserveEEContentsAndFormatChkBox.Checked = true;
						this.preserveEEContentsAndFormatChkBox.Enabled = false;
						this.preserveEEContentsChkBox.Visible = false; 
						SystemInfo.errorSB.Append("DriveWizard is unable to convert the EEPROM format. Conversion file may be missing");
					}
					else  //format can be converted 
					{
						this.preserveEEContentsAndFormatChkBox.Checked = false;
						this.preserveEEContentsAndFormatChkBox.Enabled = true;
						//User has to preserve the contents - there is nothing to overwrite them with - so hide this one
						this.preserveEEContentsChkBox.Checked = true;  //B&B  have to preserve EEPROM contents
						this.preserveEEContentsChkBox.Visible = false; //user choice is only whether to preserve the EE format
					}
				}
			}
			#endregion no EEPROM data in Download pack
		}
		#endregion

	}
	#endregion form class
	
	#region Programming Data Table Class
	public class ProgTable :DataTable
	{
		public ProgTable()
		{
			this.Columns.Add(progCols.Parameter.ToString(), typeof(System.String));// Add the Column to the table
			this.Columns.Add(progCols.Node.ToString(), typeof(System.String));// Add the column to the table.
			this.Columns.Add(progCols.dldFile.ToString(), typeof(System.String));// Add the column to the table.
		}
	}
	#endregion Programming Data Table Class

	#region programming TableStyle
	public class ProgTableStyle : SCbaseTableStyle
	{
		public ProgTableStyle (int [] ColWidths)
		{
			this.AllowSorting = false;
			SCbaseRODataGridTextBoxColumn c1 = new SCbaseRODataGridTextBoxColumn((int) (progCols.Parameter));
			c1.MappingName = progCols.Parameter.ToString();
			c1.HeaderText = "Parameter";
			c1.Width = ColWidths[(int) (progCols.Parameter)];
			GridColumnStyles.Add(c1);

			SCbaseRODataGridTextBoxColumn c2 = new SCbaseRODataGridTextBoxColumn((int) (progCols.Node));
			c2.MappingName = progCols.Node.ToString();
			c2.HeaderText = "Node";
			c2.Width = ColWidths[(int) (progCols.Node)];
			c2.NullText = "";
			GridColumnStyles.Add(c2);

			SCbaseRODataGridTextBoxColumn c3 = new SCbaseRODataGridTextBoxColumn((int) (progCols.dldFile));
			c3.MappingName = progCols.dldFile.ToString();
			c3.HeaderText = "Download file";
			c3.Width = ColWidths[(int) (progCols.dldFile)];
			c3.NullText = "";
			GridColumnStyles.Add(c3);
			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();

		}
	}
	#endregion programming TableStyle

	#region programmer class
	public class SEVCONProgrammer
	{
		#region declarations
		private enum dataType {bool_t = 1, uint8_t = 8, char_t = 8};

		internal struct EEFileHeader
		{
			///<summary>URL of this file</summary>
			public string		filepath;						
			///<summary>EE Format that this file will convert from</summary>
			public uint		baselineFormat;						
			///<summary>EE Format that this file will contert to</summary>
			public uint		updatedFormat;
		};
		private enum Processors {Host, DSP};

		public bool _preserveEEContentsAndFormat, _preserveEEPROMContents;
		#region static parmeters used by calling form GUI but generated within methods here
		public static bool CodeHasBeenSent = false;  //make this an instantly avialable static value
		public static bool EDSFileChanged = false;
		public static int progBarValue = 0, progBarMin= 0, progBarMax = 0;
		public static string statusMessage = "", userInstructionMessage = "";
		public static bool programmingRequestFlag = false;  //to prevent node reset warning when reset is result of our request to reprogram
		#endregion static parmeters used by calling form GUI but generated within methods here

		#region parameters extracted form the controller
		private uint EEPROMdeviceLength = 0;
		private int numMemSpacesused = 50;  //DR38000258 additional memory spaces added for future expansion
		/// <summary>
		/// EEPROM format in the espAC prior to programming 
		/// </summary>
		private string deviceEEFormat = ""; 
		private string hostAppController, DSPAppController;
		int noMemSpacesInController = 0;
		uint deviceHWBuild = 0;
		#endregion parameters extracted form the controller

		#region parameters used for file access
		private FileStream	_fs, _fsWrite;
		private StreamWriter _srWrite;
		private StreamReader	_sr;
		#endregion parameters used for file access

		#region parameters used for general node comms and error handling form DI
		private DIFeedbackCode _feedback;
		private Hashtable espAC_ErrorCodes_ht = new Hashtable();//16 bit unsigned int 
		#endregion parameters used for general node comms and error handling form DI

		#region parameters passed from the calling form
		private SystemInfo sysInfo;
		ushort _errorHandlingLevel;
		#endregion parameters passed from the calling form

		internal bool fatalErrorOccured = false;
		private MemSpaces memType;
		private string [] memoryStrings = {"Host internal ROM", "DSP ROM", "EEPROM", "Host External ROM", "Generic (ROM/FLASH/EEPROM)" }; // DR38000258
		byte [] ODProgBytes;
		private SevconObjectType SEVCONType;
		private int blockNum = 0;
		byte [] CopyOfDeviceEEPROM, convertedDeviceEEPROM;

		private ushort wholeDataChecksum = 0;
		private string [] newCode;
		/// <summary>
		/// EEPROM format specifed in the programming downlaod file
		/// </summary>
		private string dldEEFormat = "";
		int [] numRecords;
		int numLinesInDldFile = 0;
		bool _EEPROMFormatsMatch = false;
		bool _canConvertEEPROMFormat = false;
		ArrayList EEFormatFileHeaders, EEFormatPath;
		/// <summary>
		/// Array index of SystemInfo.Nodes of device being programmed
		/// </summary>
		int progDeviceIndex = 0;

		/// <summary>Always try to use block downloads once per download sequence whilst also checking for backwards compatibility. </summary>
		private bool useDownloadBlocks = true;
        private ushort numBlockFails = 0;
		#endregion declarations
		
		#region constructor
		public SEVCONProgrammer(int passed_nodeIndex, SystemInfo passed_localSystemInfo, ushort errorHandlingLevel)
		{
			progDeviceIndex = passed_nodeIndex;
			sysInfo = passed_localSystemInfo;
			_errorHandlingLevel = errorHandlingLevel;
			#region espAC error codes
			espAC_ErrorCodes_ht.Add("1", "ERRORCODE_NEED_32BIT_ADDRESS");
			espAC_ErrorCodes_ht.Add("2", "ERRORCODE_ADDRESS_OUT_OF_RANGE");
			espAC_ErrorCodes_ht.Add("3", "ERRORCODE_ADDRESS_NOT_IN_PAGE");
			espAC_ErrorCodes_ht.Add("4", "ERRORCODE_EXPECTING_START_ADDRESS");
			espAC_ErrorCodes_ht.Add("5", "ERRORCODE_DATA_MEMSPACE_CHANGED");
			espAC_ErrorCodes_ht.Add("6", "ERRORCODE_DATA_NOT_IN_PAGE");
			espAC_ErrorCodes_ht.Add("7", "ERRORCODE_EXPECTING_DATA");
			espAC_ErrorCodes_ht.Add("8", "ERRORCODE_NEED_16BIT_SUM");
			espAC_ErrorCodes_ht.Add("9", "ERRORCODE_CHECKSUM_MEMSPACE_CHANGED");
			espAC_ErrorCodes_ht.Add("A", "ERRORCODE_CHECKSUM_INVALID");
			espAC_ErrorCodes_ht.Add("B", "ERRORCODE_BOOTLOADER_OVERWRITE");
			espAC_ErrorCodes_ht.Add("C", "ERRORCODE_MISSING_DATALENGTH");
			espAC_ErrorCodes_ht.Add("D", "ERRORCODE_MEMORY_SPACE_LENGTH_CHANGED");
			espAC_ErrorCodes_ht.Add("E", "ERRORCODE_16BIT_LENGTH_REQURIED");
			espAC_ErrorCodes_ht.Add("F", "ERRORCODE_DATALENGTH_NOT_MULTIPLE_OF_PAGESIZE");
			espAC_ErrorCodes_ht.Add("10", "ERRORCODE_DATALENGTH_EXCEEDS_MEMORY_SPACE");
			espAC_ErrorCodes_ht.Add("11", "ERRORCODE_DATALENGTH_EXCEEDS_MAX_BLOCKSIZE");
			espAC_ErrorCodes_ht.Add("12", "ERRORCODE_CANPROTOCOL_ERROR");
			espAC_ErrorCodes_ht.Add("13", "ERRORCODE_CANNOT_WRITE_ZERO_BYTES");
			espAC_ErrorCodes_ht.Add("14", "ERRORCODE_CANNOT_WRITE_CHEKCSUM");
			espAC_ErrorCodes_ht.Add("101", "ERRORCODE_AT49F_VERIFY_ERROR");
			espAC_ErrorCodes_ht.Add("201", "ERRORCODE_XC16X16F_VERIFY_FAIL");
			espAC_ErrorCodes_ht.Add("202", "ERRORCODE_XC16X16F_FLASH_FAIL");
			espAC_ErrorCodes_ht.Add("301", "ERRORCODE_EEPROM_WRITE_FAIL");
			espAC_ErrorCodes_ht.Add("302", "ERRORCODE_EEPROM_READBACK_FAIL");
			espAC_ErrorCodes_ht.Add("303" , "ERRORCODE_EEPROM_VERIFY_FAIL");
			espAC_ErrorCodes_ht.Add("304" , "ERRORCODE_EEPROM_READ_FAIL");
			espAC_ErrorCodes_ht.Add("305" , "ERRORCODE_BAD_NOTIFICATION");
			espAC_ErrorCodes_ht.Add("306" , "ERRORCODE_DSP_NOT_RESPONDING");
			espAC_ErrorCodes_ht.Add("307", "ERRORCODE_DSP_NOT_INITIALISED");
			espAC_ErrorCodes_ht.Add("308" , "ERRORCODE_CANNOT_SEND_PROGRAM_TO_DSP");
			espAC_ErrorCodes_ht.Add("30B" , "ERRORCODE_CANNOT_RESET_DSP");
			espAC_ErrorCodes_ht.Add("30C" , "ERRORCODE_CANNOT_COMMIT_SOFTWARE_TO_DSP");
			espAC_ErrorCodes_ht.Add("30E" , "ERRORCODE_DSP_BUSY");
			espAC_ErrorCodes_ht.Add("30F" , "ERRORCODE_CANNOT_GET_DSP_RELEASE_NUMBER");
			espAC_ErrorCodes_ht.Add("310" , "ERRORCODE_CANNOT_GET_DSP_ROM_START_ADDRESS");
			espAC_ErrorCodes_ht.Add("311" , "ERRORCODE_CANNOT_GET_DSP_ROM_DEVICE_LENGTH");
			espAC_ErrorCodes_ht.Add("312" , "ERRORCODE_DSP_APPLICATION_DID_NOT_START");
			espAC_ErrorCodes_ht.Add("FF81" , "ERRORCODE_UNKNOWN_SUBINDEX_INTERNAL");
			espAC_ErrorCodes_ht.Add("FF82" , "ERRORCODE_PROGRAM_PAGE_INTERNAL");
			espAC_ErrorCodes_ht.Add("FF83" , "ERRORCODE_FLASH_EEPROM_INTERNAL");
			espAC_ErrorCodes_ht.Add("FF84" , "ERRORCODE_CALLBACK_INTERNAL");
			#endregion
		}
		#endregion constructor

		#region retrieval of node build status information prior to programming
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: getAppVersions
		///		 *  Description     : Method to request the host and DSP aapplication software versions from the controller 
		///		 *					: and to handle if the controller is already in bootloader
		///		 *					: this method also checks whethe rth controlle ris in pre-op 
		///		 *					: which is a requirement for programming to commence
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		public void getAppVersions(out string _hostVerController, out string _DSPVerController)
		{
			_hostVerController = "N/A - node in boot mode"; 
			_DSPVerController = "N/A - node in boot mode"; 
			statusMessage = "Retrieving Application Software versions";
			userInstructionMessage = "Please wait";
			//need to re-read device veriosn here for self characterization 2nd programming
			//otherwise DI still contains bootloader 1018 info
			_feedback = sysInfo.nodes[progDeviceIndex].readDeviceIdentity( );
			if(_feedback == DIFeedbackCode.DISuccess)
			{
				getInfoFromBootloaderEDS();
				if(this.fatalErrorOccured == true) //we didn't open th ebootlaoder correctly
				{
					_hostVerController = "Unknown"; 
					hostAppController = _hostVerController;
					_DSPVerController = "Unknown"; 
					DSPAppController = _DSPVerController;
					userInstructionMessage = "";
					statusMessage = "";
					return;
				}
				if(sysInfo.nodes[ progDeviceIndex ].productVariant == SCCorpStyle.bootloader_variant ) 
				{
					#region display that node is in bootloader
					_hostVerController = "N/A - node in boot mode"; 
					hostAppController = _hostVerController;
					_DSPVerController = "N/A - node in boot mode"; 
					DSPAppController = _DSPVerController;
					userInstructionMessage = "Continue or Cancel";
					statusMessage = "";
					return;
					#endregion display that node is in bootloader
				}
				else
				{
					if((sysInfo.nodes[ progDeviceIndex ].productVariant == SCCorpStyle.selfchar_variant_old)
						||(sysInfo.nodes[ progDeviceIndex ].productVariant == SCCorpStyle.selfchar_variant_new))
					{  //temp code because SC s/w doe snot contain th esowtware versions in its OD yet
						_hostVerController = "Unknown (Self Char)"; 
						hostAppController = _hostVerController;
						_DSPVerController = "Unknown (Self Char)"; 
						DSPAppController = _DSPVerController;
						ODItemData manSwVerSub = this.sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SevconObjectType.MANU_SW_VERSION, 0x00);
						if(manSwVerSub != null)
						{
							_feedback = this.sysInfo.nodes[progDeviceIndex].readODValue(manSwVerSub);
							if(_feedback == DIFeedbackCode.DISuccess)
							{
								#region store application verison from controller
								extractVersions( manSwVerSub.currentValueString, out _hostVerController, out _DSPVerController);
								hostAppController = _hostVerController;
								DSPAppController = _DSPVerController;
								userInstructionMessage = "Check application software versions then Continue or Cancel";
								statusMessage = "";
								#endregion store application verison from controller
							}
						}
						userInstructionMessage = "Continue or Cancel";
						statusMessage = "";
						return;
					}
					else
					{
						//get the current softwar everiosn - application code
						_hostVerController = "Unknown"; 
						hostAppController = _hostVerController;
						_DSPVerController = "Unknown"; 
						DSPAppController = _DSPVerController;
						userInstructionMessage = "";
						statusMessage = "";
						ODItemData manSwVerSub = this.sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SevconObjectType.MANU_SW_VERSION, 0x00);
						if(manSwVerSub != null)
						{
							_feedback = this.sysInfo.nodes[progDeviceIndex].readODValue(manSwVerSub);
							if(_feedback == DIFeedbackCode.DISuccess)
							{
								#region store application verison from controller
								extractVersions( manSwVerSub.currentValueString, out _hostVerController, out _DSPVerController);
								hostAppController = _hostVerController;
								DSPAppController = _DSPVerController;
								userInstructionMessage = "Check application software versions then Continue or Cancel";
								statusMessage = "";
								#endregion store application verison from controller
							}
							else
							{
								#region dig out the error description and pass it back 
								if(_feedback == DIFeedbackCode.CANGeneralError)  //general abort from Pauls's code ??
								{
									SystemInfo.errorSB.Append(get_espAC_Error());
								}
								#endregion dig out the error description and pass it back 
								return;
							}
						}
					}
				}
			}
			else //unable to get 1018 info from  controller
			{
				#region pass error data back to caller
				_hostVerController = "Unknown"; 
				hostAppController = _hostVerController;
				_DSPVerController = "Unknown"; 
				DSPAppController = _DSPVerController;
				userInstructionMessage = "";
				statusMessage = "";
				this.fatalErrorOccured = true; //Cannot continue
				#endregion pass error data back to caller
				return;
			}
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: setBootMode
		///		 *  Description     : Method to request the controller to enter boot loader
		///		 *					: and to verify success/fail of this
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
        public void setBootMode(int CANDeviceIndex) //DR38000172 need nodeID to turn on SDO filtering
		{
			long ODvalue = 0;
			statusMessage = "Node " 
				+ this.sysInfo.nodes[this.progDeviceIndex].nodeID.ToString() 
				+ " entering  boot loader (takes up to 1 minute)";
			userInstructionMessage = "Please wait";
			long startTimeSecs = System.DateTime.Now.Ticks;
			DriveWizard.SEVCONProgrammer.programmingRequestFlag = true;  //used in MAin Window to supress bootup warning
			ODvalue = 0xB0;//  //we are now using 0xB0 - this allows us to limit entry into programming at device end - display nowONLY enters bootloader if it gets 0xB0 - more secure than any old non zero vlaue
			if((sysInfo.nodes[progDeviceIndex].productVariant == SCCorpStyle.selfchar_variant_old)
				|| (sysInfo.nodes[progDeviceIndex].productVariant == SCCorpStyle.selfchar_variant_new))
			{
				this.sysInfo.nodes[progDeviceIndex].nodeState = NodeState.Unknown;  //JW -> SC SW does not generate heartbeats
				this.sysInfo.nodes[progDeviceIndex].accessLevel = 5; //JW -> SC no login so DI was not sending following message
				getInfoFromBootloaderEDS();  
				if(this.fatalErrorOccured == true)
				{
					return;
				}
			}
			ODItemData bootInit = sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SevconObjectType.BOOTLOADER_INIT, 0);
			MAIN_WINDOW.appendErrorInfo = false;
			_feedback = sysInfo.nodes[progDeviceIndex].writeODValue(bootInit, ODvalue);  
			MAIN_WINDOW.appendErrorInfo = true;
			if((_feedback != DIFeedbackCode.DINoResponseFromController)  && (_feedback != DIFeedbackCode.DISuccess))//thie controller does not respond  - hmm may do now??
			{
				SystemInfo.errorSB.Append("\nDevice failed to start bootloader code");
				this.fatalErrorOccured = true;
				return;
			}
			while(1<2)  
			{
				if (this.sysInfo.nodes[progDeviceIndex].nodeState == NodeState.Bootup)
				{
					_feedback = sysInfo.nodes[progDeviceIndex].readDeviceIdentity( );
					if(_feedback != DIFeedbackCode.DISuccess)
					{
						this.fatalErrorOccured = true;
					}
					break; //DR38000172 allow to switch CAN filters (done below)
				}
				else
				{
					long timeDiff = (System.DateTime.Now.Ticks) - startTimeSecs;
					if(timeDiff >= 150000000)  //15 seconds - each tick os 100 nano seconds
					{
						//we didn't get bootup message so check the prodect code
						_feedback = sysInfo.nodes[progDeviceIndex].readDeviceIdentity( );
						if(_feedback != DIFeedbackCode.DISuccess)
						{
							this.fatalErrorOccured = true;
						}
						else
						{
							if (sysInfo.nodes[progDeviceIndex].productVariant == SCCorpStyle.bootloader_variant)
							{
								this.sysInfo.nodes[progDeviceIndex].nodeState = NodeState.Bootup;
							}
							else
							{
								SystemInfo.errorSB.Append("\nDevice failed to enter bootloader within 15 seconds");
								this.fatalErrorOccured = true;
							}
						}
						statusMessage = "Retrieving information from bootloader";
                        break; //DR38000172 allow filter setup below
					}
				}
			}

            if (fatalErrorOccured == false) //DR38000172
            {
                // Turn filters on because if the bus is too busy, it can cause problems for DW block downloads
                // acceptance mask      = 1111 1111 1XXX XXXX (X=don't care)
                // acceptance filter    = 0000 0101 1000 0000 + programming node ID
                // ie accept IDs of     = 0000 0101 1??? ???? (only programming node SDO replies, range 0x580 to 0x5ff)
                // was all SDOs but works better with just prog node SDOs 
                //sysInfo.CANcomms.VCI.SetupCAN(sysInfo.CANcomms.systemBaud, 0xffffff80, (0x00000580)); 
                sysInfo.CANcomms.VCI.SetupCAN(sysInfo.CANcomms.systemBaud, 0xffffffff, (0x00000580 + (uint)sysInfo.nodes[CANDeviceIndex].nodeID));
            }
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: getBootInfo
		///		 *  Description     : Requests information form the bootloader OD. Note this information
		///		 *					: is only availabe to DW when the controller is in bootloader
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		public void getBootInfo(out string [] controllerBootParams)
		{
			controllerBootParams = new string[6];
			controllerBootParams[0] = ""; //hostBootVer = ""
			controllerBootParams[1] = ""; //dspBootVer = ""
			controllerBootParams[2] = ""; //number of processors fitted in controller
			controllerBootParams[3] = ""; //memry spaces in controller
			controllerBootParams[4] = "0x"; //eeprom format in controller
			controllerBootParams[5] = "0x"; //hardware version

			#region bootloader software versions
			ODItemData bootSwVerSub = this.sysInfo.nodes[progDeviceIndex].getODSubFromObjectType( SevconObjectType.BOOT_SW_VERSION, 0x00);
			if(bootSwVerSub != null)
			{
				_feedback = sysInfo.nodes[progDeviceIndex].readODValue(bootSwVerSub);
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					controllerBootParams[0] = "Unknown"; //hostBootVer = ""
					controllerBootParams[1] = "Unknown"; //dspBootVer = ""
					return; //non-fatal error
				}
				extractVersions( bootSwVerSub.currentValueString, out controllerBootParams[0], out controllerBootParams[1]);
			}
			#endregion bootloader sfotware versions
			//later versions of DriveWizard may need to check for non-compatible bootloader software versions here
			#region controller hardware verison
			ODItemData booHWVerSub = this.sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SevconObjectType.BOOT_HW_VERSION , 0x00);
			if(booHWVerSub != null)
			{
				_feedback = sysInfo.nodes[progDeviceIndex].readODValue(booHWVerSub);
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					return; //non-fatal error
				}
				deviceHWBuild = System.Convert.ToUInt32(booHWVerSub.currentValue);  //used in checking download file
			}
			string tempStr = deviceHWBuild.ToString("X").PadLeft(8,'0');
			controllerBootParams[5]  += tempStr;
			#endregion controller hardware verison
			#region controller memory spaces
			//now find the memory spaces
			ODItemData availMemSub = this.sysInfo.nodes[ progDeviceIndex].getODSubFromObjectType(SevconObjectType.AVAIL_MEM_SPACES, 0x00);
			if(availMemSub != null)
			{
				_feedback = sysInfo.nodes[progDeviceIndex].readODValue(availMemSub);
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					return;  //non fatal error
				}
				controllerBootParams[3] = this.convertToMemNames(System.Convert.ToUInt16(availMemSub.currentValue));
				noMemSpacesInController = System.Convert.ToUInt16(availMemSub.currentValue);
			}
			#endregion controller memory spaces
			#region controller processors
			ODItemData availProdSub = sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SevconObjectType.AVAIL_PROCESSORS, 0x00);
			if(availProdSub != null)
			{
				_feedback = sysInfo.nodes[progDeviceIndex].readODValue(availProdSub);
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					return; //non fatal error
				}
				controllerBootParams[2] = this.convertToProcNames(System.Convert.ToUInt16(availProdSub.currentValue));
			}
			#endregion controller processors
			#region EEPROM format
			getEEPROMFormatFromDevice();
			controllerBootParams[4] = deviceEEFormat;
			if(this.fatalErrorOccured == true) 
			{
				return;
			}
			#endregion EEPROM format
			statusMessage = "Displaying bootloader data";
			userInstructionMessage = "Select Continue or Cancel";
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: getInfoFromBootloaderEDS
		///		 *  Description     : Attmepts to open the bootloader EDS and mathc this to the controller
		///		 *					: if this fials then re-tries using the older veriosn of bootloader EDS 
		///		 *					: (possibilty of older bootloders in the field). This information is used to locate
		///		 *					: information in the bootloader OD
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		public void getInfoFromBootloaderEDS()
		{
			statusMessage = "Searching for compatible EDS file";
			userInstructionMessage = "Please wait";
			DriveWizard.SEVCONProgrammer.EDSFileChanged = true;
			_feedback = sysInfo.nodes[progDeviceIndex].findMatchingEDSFile();
			if(_feedback == DIFeedbackCode.DISuccess)
			{
				_feedback = sysInfo.nodes[progDeviceIndex].readEDSfile();  
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					this.fatalErrorOccured = true;
				}
			}
			else
			{
				this.fatalErrorOccured = true;
			}
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: getEEPROMFormatFromDevice
		///		 *  Description     : Reads the EEPROM format
		///		 *					: if this fails then re-tries using the older veriosn of bootloader EDS 
		///		 *					: (possibilty of older bootloders in the field). This information is used to locate
		///		 *					: information in the bootloader OD
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void getEEPROMFormatFromDevice()
		{
			ushort EEFormatSizeInBytes = 4;
			ushort maxBlockSize = 0, pageSize = 0;
			uint deviceStartAddress = 0;
			SEVCONType = SevconObjectType.BOOT_MEMORY_SPACE_EEPROM;
			getDeviceParameters(SEVCONType, out maxBlockSize, out deviceStartAddress, out EEPROMdeviceLength, out pageSize);
			if(this.fatalErrorOccured == true)
			{
				return;
			}
			if(maxBlockSize >= EEFormatSizeInBytes)  //whole format will fit in one block
			{
				ushort blocksize = 0;
				if( (EEFormatSizeInBytes>=pageSize) && ((EEFormatSizeInBytes % pageSize) == 0))  //we can round down to less than maxBlocksize
				{
					blocksize = (ushort) EEFormatSizeInBytes;
				}
				else
				{
					blocksize = (ushort) (maxBlockSize - (maxBlockSize % pageSize));  //round down to nearest pages size
				}
				sendBlockStartAddress(SEVCONType,deviceStartAddress);
				if(this.fatalErrorOccured == true)
				{
					return;
				}
				sendBlockDataLength(SEVCONType, blocksize);
				if(this.fatalErrorOccured == true)
				{
					return;
				}
				byte [] ODUploadBytes = new byte[blocksize];
				retreiveDataFromController(SEVCONType, ODUploadBytes); 
				if(this.fatalErrorOccured == true)
				{
					return;
				}
				//now convert the 32 bits into a string
				System.UInt32 tempUInt32 = 0;
				// Little endian format so can use the same conversion for all integers.
				for ( int i = 3; i >= 0; i-- )
				{
					tempUInt32 <<= 8;
					tempUInt32 += ODUploadBytes[i];
				}
				deviceEEFormat = tempUInt32.ToString("X"); //convert to hex string 
				deviceEEFormat = "0x" + deviceEEFormat.PadLeft(4,'0');
			}
			else  //need more than one block will not occur with current EEPROM so flag as error for now
			{
				SystemInfo.errorSB.Append("\nUnrecognised EEPROM type");
				this.fatalErrorOccured = true;
				return;	
			}
		}
		#endregion retrieval of controller build status information prior to programming

		#region Selecting and Reading Downloadfile
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			CompareDLDFileToDevice
		///		 *  Description		Method to perform basic comparison of the selected downlaod file 
		///		 *					contents and the data in the device to be programmed. USed to highligh to the user possible 'missing' 
		///		 *					memory space code and software/hardware mismatches which could cuase proramming to fail. 
		///		*					Warnings only are given - the final decision to program/or not is left to the user
		///		 *  Parameters      none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		public void CompareDLDFileToDevice(string DldFilename, out string [] downloadParams, out bool [] testResults )
		{
			string input, strUpperCase, tempStr;
			userInstructionMessage = "Please wait";
			statusMessage = "Parsing the download file";
			downloadParams = new string[6];
			downloadParams[0] = ""; //host version in download file
			downloadParams[1] = ""; //DSP version in download file
			downloadParams[2] = "No information in file"; //compatible HW
			downloadParams[3] = "No information in file"; //non compatible HW
			downloadParams[4] = "0x";  //EEPROM format
			downloadParams[5] = ""; //newCodeMemSpaces
			testResults  = new bool [5];
			testResults[0] = false; //0 = Hardware test Result
			testResults[1] = false; //1 = memTestResult
			testResults[2] = false; //2 = canConvertEEProm format 			
			testResults[3] = false;  //3 = EEPROMSupplied
			testResults[4] = false; //4 - EEPROM formats match

            try //DR38000172 improved error tolerance
            {
                numLinesInDldFile = 0;
                _fs = new FileStream(DldFilename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                _sr = new StreamReader(_fs);

                #region setup progress bar min/max values
                //get info from Downloadfile
                rewind(DldFilename);
                //count the number of lines in this file for the progress bar
                while ((input = _sr.ReadLine()) != null)  //while not eof
                {
                    numLinesInDldFile++;
                }
                progBarValue = progBarMin;
                progBarMax = numLinesInDldFile;
                rewind(DldFilename);
                #endregion setup progress bar min/max values

                while ((input = _sr.ReadLine()) != null)  //while not eof
                {
                    progBarValue++;
                    #region Host and DSP versions icontained in download file
                    strUpperCase = input.ToUpper();
                    if (strUpperCase.IndexOf("+DOWNLOAD_VERSIONS") != -1) //found first section that we are intereseted in
                    {
                        while ((input = _sr.ReadLine()) != null) //not eof
                        {
                            progBarValue++;
                            strUpperCase = input.ToUpper();  //change to uppercase
                            strUpperCase = strUpperCase.Trim();
                            if (strUpperCase.IndexOf("+") != -1)  //new section
                            {
                                break;
                            }
                            else if ((strUpperCase.Length > 0) && (strUpperCase.IndexOf("//") == -1)) //not an empty line
                            {
                                if (strUpperCase.IndexOf("H") != -1)  //host verison
                                {
                                    downloadParams[0] = input;
                                }
                                else if (strUpperCase.IndexOf("D") != -1)  //DSP verison
                                {
                                    downloadParams[1] = input;
                                }
                            }
                        }
                    #endregion
                        #region Compatible hardware
                        //GOOD_HARDWARE section
                        tempStr = "";
                        while ((input = _sr.ReadLine()) != null) //not eof
                        {
                            progBarValue++;
                            input = input.Trim();
                            if (input.IndexOf("+") != -1)  //new section
                            {
                                break;
                            }
                            else if ((input.Length > 0) && (input.IndexOf("//") == -1)) //not an empty line or comment
                            {
                                tempStr = tempStr + input + ", ";
                                string temp = input.Remove(0, 2); //strip out the 0x
                                ushort GoodHW = Convert.ToUInt16(temp, 16);
                                if (deviceHWBuild == GoodHW)
                                {
                                    testResults[0] = true;
                                }
                            }
                        }
                        if (tempStr.Length > 2)
                        {
                            tempStr = tempStr.Remove((tempStr.Length - 2), 2);  //remove trailing comma and space
                        }
                        if (tempStr != "")
                        {
                            downloadParams[2] = tempStr;
                        }
                        #endregion Compatible hardware
                        #region Non-compatible hardware
                        //BAD_HARDWARE section
                        tempStr = "";
                        while ((input = _sr.ReadLine()) != null) //not eof
                        {
                            progBarValue++;
                            input = input.Trim();
                            if (input.IndexOf("+") != -1)  //new section
                            {
                                break;
                            }
                            else if ((input.Length > 0) && (input.IndexOf("//") == -1)) //not an empty line or comment
                            {
                                tempStr = tempStr + input + ", ";
                                string temp = input.Remove(0, 2); //strip out the 0x
                                ushort BadHW = Convert.ToUInt16(temp, 16);
                                if (deviceHWBuild == BadHW)
                                {
                                    testResults[0] = false;
                                }
                            }
                        }
                        if (tempStr.Length > 2)
                        {
                            tempStr = tempStr.Remove((tempStr.Length - 2), 2);  //remove trailing comma and space
                        }
                        if (tempStr != "")
                        {
                            downloadParams[3] = tempStr;
                        }
                        #endregion Non-compatible hardware
                        #region EEPROM Format in Download file
                        tempStr = "";
                        _EEPROMFormatsMatch = false;
                        _canConvertEEPROMFormat = false;
                        while ((input = _sr.ReadLine()) != null) //not eof
                        {
                            progBarValue++;
                            input = input.Trim();
                            if (input.IndexOf("+") != -1)  //new section
                            {
                                break;
                            }
                            else if ((input.Length > 0) && (input.IndexOf("//") == -1)) //not an empty line or comment
                            {
                                #region extract EE format from downlaod file
                                while (input.Length < 6)  //format problem
                                {
                                    input = input.Insert(2, "0");
                                }
                                dldEEFormat = input;  // used later to check if we can convert the EEPROM
                                #endregion extract EE format from downlaod file
                                downloadParams[4] = input;
                                if (dldEEFormat == deviceEEFormat)
                                {
                                    #region EE format in dld file is  same as in espAC
                                    _canConvertEEPROMFormat = true; //belt and braces
                                    _EEPROMFormatsMatch = true;
                                    #endregion EE format in dld file is  same as in espAC
                                    break;
                                }
                                else
                                {
                                    #region check if we can conver the EEPROM formats
                                    _canConvertEEPROMFormat = false;  //default
                                    //first get a 'list of all the files in this directory 
                                    if (Directory.Exists(MAIN_WINDOW.ApplicationDirectoryPath + @"\EEPROM\EEFORMAT") == false)
                                    {
                                        Directory.CreateDirectory(MAIN_WINDOW.ApplicationDirectoryPath + @"\EEPROM\EEformat");
                                    }
                                    else
                                    {
                                        if (deviceEEFormat != "")
                                        {
                                            uint deviceEEFormatNum = System.Convert.ToUInt32(deviceEEFormat, 16);  //get a base 10 number
                                            uint dldfileEEformatNum = System.Convert.ToUInt32(this.dldEEFormat, 16);
                                            getEEFormatFileHeaders();
                                            uint dupBaseline = 0, dupUpdated = 0;
                                            if (duplicateHeadersTestPassedOK(out dupBaseline, out dupUpdated) == true)
                                            {
                                                EEFormatPath = new ArrayList();
                                                if (this.EEFormatFileHeaders.Count > 0)
                                                {
                                                    //find out how many files have the current espAC EEFormat as their baseine
                                                    //and see if any of them has a path throught to the file format
                                                    bool pathOK = false;
                                                    for (int i = 0; i < EEFormatFileHeaders.Count; i++)
                                                    {
                                                        #region search for a valid converison path
                                                        //cast back to structure from Arraylist item (object)
                                                        EEFileHeader currHeader = (EEFileHeader)EEFormatFileHeaders[i];
                                                        if (currHeader.baselineFormat == deviceEEFormatNum)
                                                        {
                                                            //we may have multiple files that have baseline equal t controller baseline 
                                                            //so we need to try each one until we get success
                                                            //start by adding first file with baseline equzl to controller 
                                                            EEFormatPath.Add(currHeader);
                                                            if (currHeader.updatedFormat != dldfileEEformatNum) //are we there yet?
                                                            {
                                                                try
                                                                {
                                                                    pathOK = getEEFormatPath(currHeader.updatedFormat, dldfileEEformatNum);
                                                                }
                                                                catch
                                                                {
                                                                    pathOK = false;
                                                                }
                                                                if (pathOK == true)
                                                                {
                                                                    _canConvertEEPROMFormat = true;
                                                                    break;  //we have found a valid EE Format path
                                                                }
                                                                else
                                                                {
                                                                    EEFormatPath.Clear();
                                                                }
                                                            }
                                                            else //we have found a valid EE Format path with a single file
                                                            {
                                                                pathOK = true;
                                                                _canConvertEEPROMFormat = true;
                                                                break;  //we have found a valid EE Format path with a single file
                                                            }
                                                        }
                                                        #endregion search for a valid converison path
                                                    } //end for loop
                                                    if (pathOK == false)
                                                    {
                                                        SystemInfo.errorSB.Append("\nNo suitable conversion path in available EEPROM format files");
                                                        this.fatalErrorOccured = true;
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    SystemInfo.errorSB.Append("\nNo EEPROM format files found");
                                                    this.fatalErrorOccured = true;
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                SystemInfo.errorSB.Append("\nDuplicate EEPROM format files found. Baseline format = "
                                                    + dupBaseline.ToString()
                                                    + ", updated format = "
                                                    + dupUpdated.ToString());
                                                this.fatalErrorOccured = true;
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            _canConvertEEPROMFormat = false;
                                            this._EEPROMFormatsMatch = false;
                                            SystemInfo.errorSB.Append("\nEEPROM version number not available from the CAN device");
                                            this.fatalErrorOccured = true;
                                            return;
                                        }
                                    }
                                    #endregion 	check if we can conver the EEPROM formats
                                    break;
                                }
                            }
                        }
                        #endregion EEPROM Format in Download file
                        #region finding memory spaces section
                        //Memory spaces sections
                        ushort mems = 0;
                        ushort[] bitmasks = { 0x0001, 0x0002, 0x0004, 0x0008 }; //DR38000258 correct bit
                        //clear the new code strings
                        int MemNum = 99;  //out of range value
                        tempStr = "";
                        while ((input = _sr.ReadLine()) != null) //not eof
                        {
                            input = input.Trim();
                            if (input.IndexOf("+FILE_CHECKSUM") != -1)  //end of target areas section
                            {
                                break;
                            }
                            else if ((input.Length > 0) && (input.IndexOf("//") == -1) && (input.IndexOf("+") == -1)) //not an empty line or comment
                            {
                                if (input.IndexOf(":") == -1) //not code
                                {
                                    MemNum = Int16.Parse(input);  //convert to 16 bit integer;
                                    memType = this.getMemSpace(MemNum);  //DR38000258 additional mem spaces
                                    if (memType == MemSpaces.EEPROM)
                                    {
                                        testResults[3] = true;
                                    }

                                    //DR38000258 ignore mem spaces above first four during check ie generic spaces can be any type
                                    if (MemNum < bitmasks.Length) //numMemSpacesused) //within defined memory spaces
                                    {
                                        mems += bitmasks[MemNum];
                                    }
                                    tempStr += memType.ToString() + ", ";
                                }
                            }
                            progBarValue++;
                        }
                        if (tempStr.Length > 2)
                        {
                            tempStr = tempStr.Remove((tempStr.Length - 2), 2);  //remove trailing comma and space
                        }
                        downloadParams[5] = tempStr;
                        if (mems == noMemSpacesInController)
                        {
                            testResults[1] = true;
                        }
                    }
                    progBarValue = progBarMin;
                        #endregion
                }
                //copy to class leval parameter
                testResults[2] = _canConvertEEPROMFormat;
                testResults[4] = _EEPROMFormatsMatch;

                if (this._errorHandlingLevel > 0)
                {
                    #region HW and memory checks/warnings
                    if ((testResults[0] != true) || (testResults[1] != true))
                    {
                        tempStr = "Warning: ";
                        if (testResults[0] != true)
                        {
                            tempStr += "Possible non-compatable hardware ";
                            if (testResults[1] != true)
                            {
                                tempStr += "and memory spaces do not match. ";
                                userInstructionMessage = tempStr + "Select Continue to program node "
                                    + this.sysInfo.nodes[this.progDeviceIndex].nodeID.ToString();
                            }
                            userInstructionMessage = tempStr;
                        }
                        else if (testResults[1] != true)
                        {
                            tempStr += "Possible memory spaces do not match. "; //DR38000258 not necessarily a mismatch anymore
                            userInstructionMessage = tempStr + "Select Continue to program node "
                                + this.sysInfo.nodes[this.progDeviceIndex].nodeID.ToString();
                        }
                        if (this._canConvertEEPROMFormat == false)
                        {
                            userInstructionMessage += "\nUnable to convert EEPROM formats, EEPROM data will not be downloaded to device";
                        }
                    }
                    else
                    {
                        userInstructionMessage = "Select Continue to program node "
                            + this.sysInfo.nodes[this.progDeviceIndex].nodeID.ToString();
                    }
                    #endregion HW and memory checks/warnings
                }
                else
                {
                    userInstructionMessage = "Select Continue to program node "
                        + this.sysInfo.nodes[this.progDeviceIndex].nodeID.ToString();
                }
                statusMessage = "Displaying data from selected download file";

                _sr.Close();
                _fs.Close();
            }
            catch (Exception e) //DR38000172
            {
                this.fatalErrorOccured = true;
                SystemInfo.errorSB.Append("Failed to parse file." + "\nSystem Error: " + e.Message + "\n");
            }
		}

		/// <summary>
		/// 	/*--------------------------------------------------------------------------
		///		 *  Name			getEEFormatFileHeaders
		///		 *  Description		Extracts the header lline form each EEPROM format file
		///		 *					From this line extract this baselien format and updated format
		///		 *					These plus the file path are place din a EEFileHeader struct
		///		 *					These structs are stored in arrayLis EEFormatFileHeaders
		///		 *  Parameters      none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void getEEFormatFileHeaders()
		{
			EEFormatFileHeaders = new ArrayList();  //create the Array list 
			//first get a list of all the eeprom format files in directory
			string [] eepromConversionFiles = Directory.GetFiles(MAIN_WINDOW.ApplicationDirectoryPath + @"\EEPROM\EEformat","*.tcl");
			//now extract and interpret the header lines
			for(int fileIndex = 0;fileIndex<eepromConversionFiles.Length;fileIndex++)
			{
				#region open streams
				FileStream fs = null;;
				//try to open the file 
				try
				{
					fs = new FileStream(eepromConversionFiles[fileIndex],FileMode.Open, FileAccess.Read);
				}
				catch 
				{
					Message.Show("file: " + eepromConversionFiles[fileIndex] 
						+ " could not be opened for read access. Contact your system administrator");
					fs.Close();  //ensure that stream is closed
					return;
				}
				StreamReader sr = new StreamReader(fs);
				#endregion open streams
				string input = "";
				while((input = sr.ReadLine()) != null)  //not EOF
				{
					string inputLower = input.ToLower();  //make case insensitive
					//create a new structure to hold fileref, baseline and updated
					if((inputLower.IndexOf("ee_header") != -1) 
						&& (inputLower.IndexOf("#") == -1))//found the header line
					{	
						EEFileHeader header = new EEFileHeader(); 
						#region extract header information and place it in an ArrayList
						inputLower.Replace("ox", "0x");  //change any ox's to 0x(zero)
						char [] seperator = {' ', '\t'};
						inputLower = inputLower.Substring(inputLower.IndexOf("ee_header")+ 9);
						string [] inputSplit = inputLower.Split(seperator);
						ushort splitIndex = 0;
						#region skip whitespace (tabs and spaces)
						while((splitIndex<inputSplit.Length) &&  (inputSplit[splitIndex].Length == 0))
						{
							splitIndex++; //skip over white space
						}
						#endregion skip whitespace (tabs and spaces)
						#region extract baseline
						try
						{
							if(inputSplit[splitIndex].IndexOf("0x") != -1)
							{
								header.baselineFormat = System.Convert.ToUInt32(inputSplit[splitIndex],16);
							}
							else
							{
								header.baselineFormat = System.Convert.ToUInt32(inputSplit[splitIndex]);
							}
							splitIndex++;
						}
						catch
						{
                            sr.Close();
                            fs.Close();  //close streams
							SystemInfo.errorSB.Append("\nFile: ");
							SystemInfo.errorSB.Append(eepromConversionFiles[fileIndex]);
							SystemInfo.errorSB.Append(" failed ee_header validation: "
								+ inputSplit[splitIndex] 
								+ " is invalid.\n This file will be ignored");
						}
						#endregion extract baseline
						#region skip whitespace (tabs and spaces)
						while((splitIndex<inputSplit.Length) && (inputSplit[splitIndex].Length == 0))
						{
							splitIndex++; //skip over white space elements
						}
						#endregion skip whitespace (tabs and spaces)
						#region extract updated
						try
						{
							if(inputSplit[splitIndex].IndexOf("0x") != -1)
							{
								header.updatedFormat = System.Convert.ToUInt32(inputSplit[splitIndex],16);
							}
							else
							{
								header.updatedFormat = System.Convert.ToUInt32(inputSplit[splitIndex]);
							}
							splitIndex++;
						}
						catch
						{
                            sr.Close();
                            fs.Close();  //close streams
							Message.Show("file: " + eepromConversionFiles[fileIndex] 
								+ " failed ee_header validation: "
								+ " inputSplit[splitIndex] "
								+ " is invalid.\nThis file will be ignored");
						}
						#endregion extract updated
						if((header.baselineFormat>=0) && (header.updatedFormat>0))  //both numbers are valid jude We can now convert from zero
						{
							header.filepath = eepromConversionFiles[fileIndex];
							EEFormatFileHeaders.Add(header);  //add this header to our list
						}
						else
						{ //inform user
							if(header.baselineFormat < 0)
							{
								Message.Show("file: " + eepromConversionFiles[fileIndex] 
									+ " failed ee_header validation "
									+ "'convert from' format must be >= 0"
									+ "\nThis file will be ignored");
							}
							else if (header.updatedFormat<= 0)
							{
								Message.Show("file: " + eepromConversionFiles[fileIndex] 
									+ " failed ee_header validation "
									+ " 'convert to' format must be >0."
									+ "\nThis file will be ignored");
							}
						}
                        sr.Close();
                        fs.Close();  //close streams
						break;  //move to next file
						#endregion extract header information and place it in an ArrayList
					}
				}
			}
		}
		private bool getEEFormatPath(uint startFormat, uint endFormat)
		{ //endFormat does not change passed because it needed to be converted form string first
			//Recursive find 
			for(int i = 0;i<EEFormatFileHeaders.Count;i++)
			{
				EEFileHeader header = (EEFileHeader) EEFormatFileHeaders[i];
				if(header.baselineFormat == startFormat) 
				{
					//first check that we are not adding a straight reverse step here - causes endless loop
					bool reverseFlag = false;
					for (int j = 0;j< EEFormatFileHeaders.Count;j++)
					{
						EEFileHeader prevHeader = (EEFileHeader) EEFormatFileHeaders[j];
						if (header.updatedFormat == prevHeader.baselineFormat)
						{
							reverseFlag = true;
							break;
						}
					}
					if(reverseFlag == false)
					{
						//add to path list
						EEFormatPath.Add(header);
						if( header.updatedFormat == endFormat)
						{
							return true;
						}
						//make the end format the new start to find next link in th echain
						//then run this again to search for a connecting link
						if(getEEFormatPath(header.updatedFormat, endFormat) == true)
						{
							return true;
						}
					}
				}
			}
			return false;  //we were unable to create a complete path
		}
		/// <summary>
		/// 		/// 
		/// </summary>
		/// <returns></returns>
		private bool duplicateHeadersTestPassedOK(out uint dupBaseline, out uint dupUpdated)
		{
			dupBaseline = 0;
			dupUpdated = 0;
			for(int i = 0;i<this.EEFormatFileHeaders.Count;i++)
			{
				//get next header out of arryList
				EEFileHeader currHeader = (EEFileHeader) EEFormatFileHeaders[i];
				//check against the remaininng items in the ArrayList
				for(int j = (i+1);j<this.EEFormatFileHeaders.Count;j++)
				{
					EEFileHeader testHeader = (EEFileHeader) EEFormatFileHeaders[j];
					if((currHeader.baselineFormat == testHeader.baselineFormat)
						&& (currHeader.updatedFormat == testHeader.updatedFormat))
					{
						dupBaseline = currHeader.baselineFormat;
						dupUpdated = testHeader.updatedFormat;
						return false;
					}
				}
			}
			return true;
		}
		internal void rewind(string DldFilename)
		{
			if ( _fs.CanSeek )
			{
				_fs.Seek( 0, System.IO.SeekOrigin.Begin );
				_sr = null;
				_sr = new StreamReader( _fs );

			}
				// Stream not seekable so must close and reopen to go back to the start of the stream.
			else
			{
				_fs.Close();
				_fs = new FileStream( DldFilename, System.IO.FileMode.Open, System.IO.FileAccess.Read );
				_sr = new StreamReader( _fs );
			}
		}

		#endregion Selecting and Reading Downloadfile

		#region retrieval of EEPROM data from node
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: getEEPROMFormatFromDevice
		///		 *  Description     : Reads the EEPROM format
		///		 *					: if this fails then re-tries using the older veriosn of bootloader EDS 
		///		 *					: (possibilty of older bootloders in the field). This information is used to locate
		///		 *					: information in the bootloader OD
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		public void uploadEEPROM()
		{
			#region upload the contents of the EEPROM from the device - see Chris's comments EEPROM to ALWAYS be uploaded now
			SEVCONType = SevconObjectType.BOOT_MEMORY_SPACE_EEPROM;
			UploadDataFromDevice(SevconObjectType.BOOT_MEMORY_SPACE_EEPROM);
			if(this.fatalErrorOccured == true)
			{
				return;
			}
			progBarValue = progBarMin;
			#endregion upload the contents of the EEPROM from the device - see Chris's comments EEPROM to ALWAYS be uploaded now
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: UploadDataFromDevice
		///		 *  Description     : Generic method to upload data form controller. Currnetly used to upload EEPROm data only
		///		 *					: but could be used for other data if requiired
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void UploadDataFromDevice(SevconObjectType SEVCONType)
		{
			ushort maxBlockSize = 0, pageSize = 0;;
			uint deviceStartAddress = 0,  deviceLength = 0;
			ushort blockDataLength = 0;
			getDeviceParameters(SEVCONType, out maxBlockSize, out deviceStartAddress, out deviceLength, out pageSize);
			if(this.fatalErrorOccured == true)
			{
				return;
			}
			ushort normalBlockSize = (ushort) (maxBlockSize - (maxBlockSize % pageSize));  //round down to nearest pages size
			progBarMax = (int) deviceLength;
			progBarValue = progBarMin;
			CopyOfDeviceEEPROM = new byte[deviceLength];
			uint bytesRemaining = deviceLength;
			uint blockStartAddress = deviceStartAddress;
			while(bytesRemaining>0)
			{
				sendBlockStartAddress(SEVCONType,blockStartAddress);
				if(this.fatalErrorOccured == true)
				{
					return;
				}
				//calculate the expected blockDataLength
				if (bytesRemaining >= normalBlockSize)
				{
					blockDataLength = normalBlockSize;
				}
				else //the last block will be smaller
				{
					blockDataLength = (ushort) bytesRemaining;  //remainder of node space
				}
				sendBlockDataLength(SEVCONType, blockDataLength);
				if(this.fatalErrorOccured == true)
				{
					return;
				}
				byte [] ODUploadBytes = new byte[blockDataLength];
				retreiveDataFromController(SEVCONType, ODUploadBytes); 
				if(this.fatalErrorOccured == true)
				{
					return;
				}
				Array.Copy(ODUploadBytes, 0, CopyOfDeviceEEPROM, (blockStartAddress- deviceStartAddress),ODUploadBytes.Length);
				#region update user feedback
				progBarValue = System.Convert.ToInt32(blockStartAddress- deviceStartAddress);
				statusMessage =  "EEPROM: Receiving data byte 0x" + (blockStartAddress- deviceStartAddress).ToString("X") + " of 0x" + deviceLength.ToString("X") + "bytes"; 
				#endregion update user feedback
				blockStartAddress += blockDataLength;  //ready for next loop
				bytesRemaining -= blockDataLength;
			}	//end while
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: retreiveDataFromController
		///		 *  Description     : Low level method used to actually get the databytes from the controller
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: ODUploadBytes - reference copy of the array used to hold the databytes form the controller.
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void retreiveDataFromController(SevconObjectType SEVCONType, byte [] ODUploadBytes)
		{
            ODItemData dataSub = this.sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SEVCONType, 0x07);

            if (dataSub != null)  //if it is null then fatalErrorOccured is already set true
            {
                // allow retries as heavy loading on slow pc can lead to CANUnknownCommandSpecifier
                for (uint retries = 0; retries < SCCorpStyle.NumAllowedBlockFails; retries++)
                {
                    _feedback = sysInfo.nodes[progDeviceIndex].readODValue(dataSub);

                    if (_feedback == DIFeedbackCode.DISuccess)
                    {
                        for (int i = 0; i < ODUploadBytes.Length; i++)
                        {
                            ODUploadBytes[i] = dataSub.currentValueDomain[i];
                        }
                        break;
                    }
                }

                if (_feedback != DIFeedbackCode.DISuccess)
                {
                    this.fatalErrorOccured = true;
                }
            }
            else
            {
                this.fatalErrorOccured = true;
            }
		}

		#endregion retrieval of EEPROM data from node

		#region tranmitting block intitial characteristics and data
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: sendBlockStartAddress
		///		 *  Description     : Transmits the start address of the next block of data to be sent to the controller
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: blockStartAddress - 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void sendBlockStartAddress(SevconObjectType SEVCONType, uint blockStartAddress)
		{
			ODItemData blockStartSub = sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SEVCONType, 0x05);
			if(blockStartSub != null)
			{
				_feedback = sysInfo.nodes[progDeviceIndex].writeODValue(blockStartSub, (long)blockStartAddress);
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					this.fatalErrorOccured = true; 
				}
			}
			else
			{
				this.fatalErrorOccured = true; 
			}
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: sendBlockDataLength
		///		 *  Description     : Transmits the length in bytes of the next block of data to be sent to the controller
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: blockDataLength - 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void sendBlockDataLength(SevconObjectType SEVCONType, ushort blockDataLength)
		{
			ODItemData blockLengthSub = sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SEVCONType, 0x06);
			if(blockLengthSub != null)
			{
				_feedback = sysInfo.nodes[progDeviceIndex].writeODValue(blockLengthSub, (long)blockDataLength);
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					this.fatalErrorOccured = true; 
				}
			}
			else
			{
				this.fatalErrorOccured = true; 
			}
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: sendDataBlock
		///		 *  Description     : Transmits the nest block of data to the controller
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: reference ODProgBytes[] -  byte array contiaing code data to be set to the controller
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void sendDataBlock(SevconObjectType SEVCONType, byte [] ODProgBytes)
		{
			blockNum++;
			ODItemData odSub = sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SEVCONType, 0x07);
			/* AJK, 27/07/05 
			 * Change to block downloads for quicker speed.  If failed (ie get an abort code) then revert
			 * to old method for backwards compatibility.
			*/
			if ( useDownloadBlocks == true )
			{
				MAIN_WINDOW.appendErrorInfo = false;
				_feedback = sysInfo.nodes[progDeviceIndex].writeODValueBlock(odSub, ODProgBytes);
				MAIN_WINDOW.appendErrorInfo = true;
				
				if ( _feedback != DIFeedbackCode.DISuccess )
				{
                    numBlockFails++;
                    //DR38000172 Read anything - if we start a segmented write immediately after a block 
                    //fail then the controller gets confused and sends an InvalidCommandSpecifier error.
                    //Reading something/anything seems to clear out the previous block failure.
                    _feedback = sysInfo.nodes[progDeviceIndex].readODValue(SevconObjectType.AVAIL_PROCESSORS, false);
                    _feedback = sysInfo.nodes[progDeviceIndex].writeODValue(odSub, ODProgBytes);

                    if (numBlockFails > SCCorpStyle.NumAllowedBlockFails)
                    {
                        useDownloadBlocks = false;
                    }
				}
			}
			else
			{
				_feedback = sysInfo.nodes[progDeviceIndex].writeODValue(odSub, ODProgBytes);
			}
			if(_feedback != DIFeedbackCode.DISuccess)
			{
				SystemInfo.errorSB.Append("\nUnable to send block number " +  blockNum.ToString() + " to " + memoryStrings[(int) memType].ToString());
				this.fatalErrorOccured = true;
			}
		}

		#endregion tranmitting block intitial characteristics and data

		#region retrieving pre code Transmission node characteristics
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: getDeviceParameters
		///		 *  Description     : Retrieves the programming constraint parameters formt he bootloader OD. Thi sis to make DriveWizard
		///							: independent of the memory hardwar eused in a controller.
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: Hardware parmeters for this memory space 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void getDeviceParameters(SevconObjectType SEVCONType,  out ushort maxBlockSize, out uint deviceStartAddress, out uint deviceLength,out ushort pageSize)
		{
			maxBlockSize = 0;	pageSize = 0;	deviceStartAddress = 0;	deviceLength = 0;
			getMaxBlockSize(SEVCONType, out maxBlockSize);
			if(this.fatalErrorOccured == true)
			{
				return;
			}
			getPageSize(SEVCONType, out pageSize);
			if(this.fatalErrorOccured == true)
			{
				return;
			}
			getDeviceStartAddress(SEVCONType, out deviceStartAddress);
			if(this.fatalErrorOccured == true)
			{
				return;
			}
			getDeviceLength(SEVCONType, out deviceLength);	
			if(this.fatalErrorOccured == true)
			{
				return;
			}
			if(pageSize > maxBlockSize)
			{
				SystemInfo.errorSB.Append("\nPage size exceeds maximum block size(" + this.memType.ToString() + ")");
				this.fatalErrorOccured = true;
				return;
			}
			if(deviceLength < maxBlockSize)
			{
				SystemInfo.errorSB.Append("\nBlocksize exceeds memory space size(" + this.memType.ToString() + ")");
				this.fatalErrorOccured = true;
				return;
			}
			if(((deviceLength%pageSize) >0) && (memType != MemSpaces.DSPROM)) //DSP we need to 'trim' last 4 bytes otherwise this test is valid
			{
				SystemInfo.errorSB.Append("\nMismatch between page size and memory space length(" + this.memType.ToString() + ")");
				this.fatalErrorOccured = true;
				return;
			} 
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: getMaxBlockSize
		///		 *  Description     : Retrieves the maximum number of bytes that the controller can recieve for this memory space in one go
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: out maxBlockSize
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void getMaxBlockSize(SevconObjectType SEVCONType, out ushort maxBlockSize)
		{
			maxBlockSize = 0;
			ODItemData odSub = this.sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SEVCONType, 0x01);
			if(odSub != null)
			{
				_feedback = this.sysInfo.nodes[progDeviceIndex].readODValue(odSub);
				if(_feedback == DIFeedbackCode.DISuccess)
				{
					try
					{
						maxBlockSize = System.Convert.ToUInt16(odSub.currentValue);
					}
					catch
					{
						SystemInfo.errorSB.Append("\nInvalid max block Size for " + memoryStrings[(int) memType].ToString());
						this.fatalErrorOccured = true; 
						return;
					}
					if(maxBlockSize<= 0) //in practice can only be zero
					{
						SystemInfo.errorSB.Append("\nInvalid max block Size for " + memoryStrings[(int) memType].ToString());
						this.fatalErrorOccured = true; 
					}
				}
				else
				{
					SystemInfo.errorSB.Append("\nUnable to read " + memoryStrings[(int) memType].ToString() + " Max Block size.");
					this.fatalErrorOccured = true; 
				}
			}
			else
			{
				this.fatalErrorOccured = true; 
			}
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: getPageSize
		///		 *  Description     : Retrieves the smallest quatum of data that the controller can recieve - blcok size must be a multiple of page size
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: out pageSize
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void getPageSize(SevconObjectType SEVCONType, out ushort pageSize)
		{
			pageSize = 0;
			ODItemData pgSizeSub = this.sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SEVCONType, 0x02);
			if(pgSizeSub != null)
			{
				_feedback = this.sysInfo.nodes[progDeviceIndex].readODValue(pgSizeSub);
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					SystemInfo.errorSB.Append("\nUnable to read " + memoryStrings[(int) memType].ToString() + " Page size.");
					this.fatalErrorOccured = true;
					return;
				}
				pageSize =  System.Convert.ToUInt16(pgSizeSub.currentValue);
			}
			if(pageSize<= 0)
			{
				SystemInfo.errorSB.Append("\nInvalid page size for " + memoryStrings[(int) memType].ToString() + "\nvalue: " + pageSize.ToString());
				this.fatalErrorOccured = true;
				return;
			}
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: getDeviceStartAddress
		///		 *  Description     : Retrieves the start address of this memory space
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: out deviceStartAddress
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void getDeviceStartAddress(SevconObjectType SEVCONType, out uint deviceStartAddress)
		{
			deviceStartAddress =0;
			ObjDictItem odItem;
			ODItemData odSub = this.sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SEVCONType, 0x03, out odItem);
			if(odSub != null)
			{
				_feedback = this.sysInfo.nodes[progDeviceIndex].readODValue(odSub);
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					this.fatalErrorOccured = true;
					SystemInfo.errorSB.Append("\nUnable to read " + memoryStrings[(int) memType].ToString() + " Memory space start address.");
					return;
				}
				deviceStartAddress =  (System.Convert.ToUInt32(odSub.currentValue));
			}
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: getDeviceLength
		///		 *  Description     : Retrieves the length in bytes of the memory sopace. Used with the deviceStartAddress 
		///		 *					: to determine the device end address
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: out deviceLength
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void getDeviceLength(SevconObjectType SEVCONType, out uint deviceLength)
		{
			deviceLength = 0;
			ODItemData odSub = this.sysInfo.nodes[progDeviceIndex].getODSubFromObjectType( SEVCONType, 0x04);
			if(odSub != null)
			{
				_feedback = this.sysInfo.nodes[progDeviceIndex].readODValue(odSub);
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					SystemInfo.errorSB.Append("\nUnable to read " + memoryStrings[(int) memType].ToString() + " length.");
					this.fatalErrorOccured = true;
					return;
				}
				deviceLength =  (System.Convert.ToUInt32(odSub.currentValue));
			}
			if(deviceLength <= 0)
			{
				SystemInfo.errorSB.Append("\nInvalid device length for " + memoryStrings[(int) memType].ToString() + "\nvalue: " + deviceLength.ToString());;
				this.fatalErrorOccured = true;
				return;
			}
		}

		#endregion retrieving pre code Transmission device characteristics

		#region code transmission
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: sendProgResetcommand
		///		 *  Description     : Sends command to controller to tell the controlle rto expect new programming data. 
		///		 *					: This is required by controller in some internal modes and so is send by DriveWizard default 
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: nonZeroValue - any non zero value which triggers controlle rto expect new code transfer
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void sendProgResetcommand(SevconObjectType SEVCONType, ushort nonZeroValue)
		{
			long ODvalue = (long)nonZeroValue;
			statusMessage = "Waiting for node to respond to Reset command";
			ODItemData odSub = sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SEVCONType, 0x0);
			if(odSub != null)
			{
				_feedback = sysInfo.nodes[progDeviceIndex].writeODValue(odSub, ODvalue);
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					this.fatalErrorOccured = true;
				}
			}
			else
			{
				this.fatalErrorOccured = true; 
			}
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: downloadCodeControl
		///		 *  Description     : Top level methoid to control the download of code to all affected memory spaces
		///		 *  Parameters      : 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		public void downloadCodeControl(string DldFilename)
		{
			extractCodeFromDldFile(DldFilename);
			statusMessage =  "";
			useDownloadBlocks = true;		// AJK, try and use block downloads at least once per download
			//once we have sent the reset command we assume that node should not be pulle dout of boot in the even tof programming error
			CodeHasBeenSent = true;
			sendProgResetcommand(SevconObjectType.BOOT_RESET, 0xFF);  //send this every time in case bootloader is in its 'failed to program' mode
			if(this.fatalErrorOccured == true)
			{
				return;
			}
			#region downloadingcode
			statusMessage = "Downloading code";
			//deal with the EEPROM first to give user chance to 'back out ' if we can't do a required conversion
			#region EEPROM handling
			if(this._preserveEEContentsAndFormat == false)   //if its true then we do nothing to the EEPROM
			{
				bool EEPROMConvertedOK = false;
				if(this._preserveEEPROMContents == true) //we are sourcing from controller regardless of any code in file
				{
					#region user wants to keep their controller EEPROM values
					if(( _EEPROMFormatsMatch == false)  && (this._canConvertEEPROMFormat == true))
					{
						EEPROMConvertedOK = convertEEPROM();
						if(EEPROMConvertedOK == true)
						{
							downloadConvertedEEPROM();  //send converted EEPORM back to controller
							if(this.fatalErrorOccured == true)
							{
								return;
							}
						}
						else
						{
							SystemInfo.errorSB.Append("\nCould not convert EEPROM format. EEPROM was not ovewrwritten");  //non fatal
							//tell user that EEPROM conversion failed and so EEPROM won't be overwritten
						}
					}
					#endregion user wants to keep their controller EEPROM values
				}
				else  //source from downlaod file - to get here it MUST contain EEPROM code - handled by checkbox setup
				{
					#region user is happy to overwrite their devcie EEPROM 
					int i = (int) MemSpaces.EEPROM;
					SendDataToSingleMemorySpace(SevconObjectType.BOOT_MEMORY_SPACE_EEPROM, i);
					if(this.fatalErrorOccured == true)
					{
						return;
					}
					#endregion user is happy to overwrite their devcie EEPROM if new EEPROM code is in dopwnload file
				}
			}
			#endregion EEPROM handling
			//flags to indicate when sections of download are complete
			for(int i=0;i<numMemSpacesused;i++)
			{
				if(i == (int)MemSpaces.EEPROM) //DR38000258
				{
					continue;   //skip this iteration of the loop - EEPROM now done first
				}
				if( (newCode[i].Length>0) )
				{
					#region select SEVCONType TextString
                    memType = this.getMemSpace(i);      //DR38000258

                    switch (memType)
					{
						case MemSpaces.HostIntROM:
							SEVCONType = SevconObjectType.BOOT_MEMORY_SPACE_HOST_INT_ROM;
							break;
						case MemSpaces.DSPROM:
							SEVCONType = SevconObjectType.BOOT_MEMORY_SPACE_DSP_ROM;
							break;
						case MemSpaces.HostExtROM:
							SEVCONType = SevconObjectType.BOOT_MEMORY_SPACE_HOST_EXT_ROM;
							break;
						case MemSpaces.EEPROM:
							SystemInfo.errorSB.Append("\nUnexpected request to write to EEPROM");
							this.fatalErrorOccured = true;
							return;
                        case MemSpaces.GenMemSpace: //DR38000258
                            //convert from dld TARGET_MEMORY (#) to equivalent Sevcon object name
                            try
                            {
                                int memSpace = ((int)(SevconObjectType.BOOT_MEMORY_SPACE_5)) + (i - 5);
                                SEVCONType = (SevconObjectType)memSpace;
                            }
                            catch
                            {
                                SystemInfo.errorSB.Append("\nUnknown memory space in download file.");
                                this.fatalErrorOccured = true;
                            }
                            break;

						default:
							SystemInfo.errorSB.Append("\nUnable to  write to " + memType.ToString() + "  start address");
							this.fatalErrorOccured = true;
							return;
					}
					#endregion
					blockNum = 0;
					SendDataToSingleMemorySpace(SEVCONType, i);
					if(this.fatalErrorOccured == true)
					{
						return;
					}
				}
			}
			#endregion
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: SendDataToSingleMemorySpace
		///		 *  Description     : Controls the downloading of socde to the specified memory space
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: memorySpace integer indicating the current target memory space
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void SendDataToSingleMemorySpace(SevconObjectType SEVCONType,int memorySpace)
		{
			#region initialise data transfer
			ushort maxBlockSize = 0, pageSize = 0;
			uint deviceStartAddress = 0,  deviceLength = 0;
			memType = getMemSpace(memorySpace); //DR38000258
			getDeviceParameters(SEVCONType, out maxBlockSize, out deviceStartAddress, out deviceLength, out pageSize);
			if(this.fatalErrorOccured == true)
			{
				return;
			}
			int numPages = (int) (deviceLength / pageSize) ;//add one to cover any remainder
			//jude 10/May/07 DSP device length cna now not be an integer multiple of page size
			//However in this scenario the last partial page will never be written to.
			//So to ensure that code works OK we need to check for this and round device length down to 
			// the nearest interger multiple
			if(deviceLength % pageSize != 0)
			{
				deviceLength = (uint) (pageSize * numPages); //round it down
			}
			uint deviceEndAddress = deviceStartAddress + deviceLength;
			
			byte [,] pageArrays = new byte[numPages, pageSize];  //creat blank array for a single page
			bool [] pageChanged = new bool [numPages];
			uint [] pageStarts = new uint[numPages]; 
			for(int i=0;i<numPages;i++)
			{
				pageChanged[i] = false;
				pageStarts[i] = (uint)  (i * pageSize);
			}
			int pageNum =0;  //marker for which page array to stuff data into
			#endregion
			#region parse Intel HEX data
			string record = "", recordType = "", recordLength = "00", recordAddress = "0000", data = "", upperAddr = "0000";
			statusMessage = this.memoryStrings[(int) memType] +  ": Creating data blocks";
			progBarValue = progBarMin;
			progBarMax = numRecords[memorySpace];
			int newCodePtr = 1; //ignore leading colon
			while(newCodePtr<newCode[memorySpace].Length)
			{
				int nextColon = newCode[memorySpace].IndexOf(':',newCodePtr);  //find next colon
				if(nextColon != -1)  //not last record
				{
					record = newCode[memorySpace].Substring(newCodePtr,(nextColon-newCodePtr));
					newCodePtr =nextColon +1;
				}
				else  //last record
				{
					record = newCode[memorySpace].Substring(newCodePtr,(newCode[memorySpace].Length - newCodePtr));
					newCodePtr = newCode[memorySpace].Length;
				}
				recordLength =  record.Substring(0,2);  //get the record length
				recordAddress = record.Substring(2,4);  //get lower 2 bytes of start address for this record
				recordType = record.Substring(6,2); //get record type

				string recordCopy = record;  //take a copy that we can butcher
				recordCopy = recordCopy.Substring(recordCopy.Length-2);  //and trim off the record checksum
				int byteptr = 0;
				if(this._errorHandlingLevel>0)
				{
					#region record checksum test
					byte recordChecksum = Convert.ToByte(recordCopy,16);  //extract record checksum
					byte calcChecksum =0;
					int test = (record.Length-2)/2;  //1 byte = 2 chars, omit the checksum byte
					for(int i=0;i<(test);i++)
					{
						string recordCopy2 = record;  //take copy of original record
						recordCopy2 = recordCopy2.Substring(byteptr,2); //now butcher the copy
						calcChecksum += Convert.ToByte(recordCopy2,16); //and mush it into the checksum
						byteptr +=2;  //point to the next (victim) byte
					}
					calcChecksum = (byte) (0x100 - calcChecksum);  //modulo 256 - invert all bits to retrieve the proper calculated checksum 
					if(calcChecksum != recordChecksum) //same as Rxd checksum?
					{
						SystemInfo.errorSB.Append("\nInvalid record checksumin record " + record);;
						this.fatalErrorOccured = true;
						return;
					}
					#endregion end checksum test
				}
				data = record.Remove(0,8);  //leave just the data bytes
				data = data.Remove((data.Length-2), 2);  //remove record checksum

				#region switch record type
				switch (recordType)
				{
					case "00": //data record
						#region data record
						uint addr = Convert.ToUInt32((upperAddr + recordAddress), 16);
						if(this._errorHandlingLevel>0)
						{
							#region error check record
							if(record.Length<10) //prevent possible exception
							{
								SystemInfo.errorSB.Append("\nInvalid Data record, for Address: 0x" + upperAddr + recordAddress);
								this.fatalErrorOccured = true;
								return;
							}
							if (addr > deviceEndAddress)
							{
								SystemInfo.errorSB.Append("\nTarget address exceeds " + this.memoryStrings[(int) memType] + " length");	
								this.fatalErrorOccured = true;
								return;
							}
							else if (addr < deviceStartAddress)
							{
								SystemInfo.errorSB.Append("\nTarget address preceeds start of " + this.memoryStrings[(int) memType]);	
								this.fatalErrorOccured = true;
								return;
							}
							#endregion error check record
						}
						uint relativeAddr = addr - deviceStartAddress;
						pageNum = Convert.ToInt32(relativeAddr/pageSize);
						//write to addresses
						byteptr = 0;
						int pageAddr = 0;
						for(int i=0;i<(data.Length/2);i++)  //each byte takes up two characters of data array
						{
							pageAddr = Convert.ToInt32((relativeAddr + i) - pageStarts[pageNum]);  //convert to zero offset address in the appropriate page array
							if(pageAddr >= pageSize)  //cuts into next page array
							{
								pageNum++; //move to next array 
								pageAddr = pageAddr - pageSize;  //remove offset 
							}
							pageArrays[pageNum , pageAddr] = Convert.ToByte((data.Substring(byteptr,2)),16); //add data to array
							pageChanged[pageNum] = true;  //flag this page array as changed
							byteptr+=2;  //move to next data byte
						}
						#endregion data record
						break;

					case "04": //extended linear address record
						#region extended linear address record
						if(this._errorHandlingLevel>0)
						{
							#region error checking
							if(record.Length != 14)
							{
								SystemInfo.errorSB.Append("\nInvalid Extended Linear Address record");
								this.fatalErrorOccured = true;
								return;
							}
							if((recordLength != "02") || (data.Length != 4) || (recordAddress != "0000"))
							{
								SystemInfo.errorSB.Append("\nInvalid Extended Linear Address record");
								this.fatalErrorOccured = true;
								return;
							}
							#endregion error checking
						}
						upperAddr = data;
						#endregion extended linear address record
						break;

					case "01": //eof
						#region eof
						if(this._errorHandlingLevel>0)
						{
							#region error checking
							if(record.Length != 10)
							{
								SystemInfo.errorSB.Append("\nInvalid EOF marker");
								this.fatalErrorOccured = true;
								return;
							}
							if((recordLength != "00") || (data.Length != 0) || (recordAddress != "0000"))
							{
								SystemInfo.errorSB.Append("\nInvalid EOF marker");
								this.fatalErrorOccured = true;
								return;
							}
							#endregion error checking
						}
						#endregion eof
						break;

					default:
						#region default error handling
						SystemInfo.errorSB.Append("\nInvalid record type: " + recordType);
						this.fatalErrorOccured = true;
						return;
						#endregion default error handling
				}
				#endregion
				progBarValue++;
			}  //end while loop
			#endregion parse Intel HEX data
			#region write code to device
			#region status bar and progress bar
			statusMessage =  "Sending code to: " + memoryStrings[(int) memType].ToString();
			progBarValue = progBarMin;
			progBarMax = numPages;
			#endregion status bar and progress bar
			
			uint MaxNumPagesInBlock = (uint) (maxBlockSize/pageSize);
			uint numBytesLeft = deviceLength;  //not sent any yet
			pageNum = 0;
			while(pageNum<numPages)
			{
				#region if(pageChanged[pageNum] == true)  
				if (pageChanged[pageNum] == true)  
				{
					uint blockStartAddress = pageStarts[pageNum] + deviceStartAddress;
					#region calculate number of pages to be sent in this block
					ushort numPagesInThisBlock = 0; 
					int numPagesLeft = (int) (numBytesLeft/pageSize);
					if((numBytesLeft%pageSize) != 0)  //does remainder divide equally into pages
					{
						numPagesLeft = (int) (numBytesLeft/pageSize) + 1;
					}
					uint MaxNumPagesInThisBlock = (uint) System.Math.Min(MaxNumPagesInBlock, numPagesLeft);  
					for(int j=pageNum;j<(pageNum+MaxNumPagesInThisBlock);j++)
					{
						if(pageChanged[j] == true)  //see if next page has changed
						{
							numPagesInThisBlock++;	
						}
						else
						{
							break;  //page has not changed so this marks the end of the block
						}
					}
					#endregion calculate number of page sto be sent in this block
					ushort blockDataLength = (ushort)(numPagesInThisBlock * pageSize);
					#region diagnostic code
#if CAN_TRAFFIC_DEBUG
					System.Diagnostics.Debug.WriteLine("Tx Block Start Addr; 0x" +  blockStartAddress.ToString("X"));
#endif
					sendBlockStartAddress(SEVCONType, blockStartAddress);
					if(this.fatalErrorOccured == true)
					{
						return;
					}
#if CAN_TRAFFIC_DEBUG
					System.Diagnostics.Debug.WriteLine("Tx Block Length; 0x" +  blockDataLength.ToString("X"));
#endif
					#endregion diagnostic code
					sendBlockDataLength(SEVCONType, blockDataLength);
					if(this.fatalErrorOccured == true)
					{
						return;
					}
					ODProgBytes = new byte[blockDataLength];  //convert string to byte array
					ushort checksumofBlockData = 0;
					for(int i=0;i<numPagesInThisBlock;i++)
					{
						for(int j=0;j<pageSize;j++)
						{
							ODProgBytes[((i*pageSize)+j)] = pageArrays[pageNum,j];
							checksumofBlockData += pageArrays[pageNum,j];  //to reduce statements in this inner loop
						}
						pageNum++;
					}
					progBarValue = pageNum;
					#region removal of last 4 physical bytes for DSP if necessary 
					//27/09/06 decided no longer needed comment  out until furthernotice
					//if needed to be put back in then it should be debugged pageArrays line may well be extending beyond array Jude 27/09/06
//					if(( pageNum>=(numPages-1))  && (memType == MemSpaces.DSPROM) )  //If this is the last physical page on the DSP
//					{
//						//DSP cannot program the last 4 of its bytes so we remove them prior to sending to 
//						//prevent DSP flagging an error
//
//						//first update the checksum of the block of data only
//						for(int k=0;k<4;k++)
//						{
//							//byte [,] pageArrays = new byte[numPages, pageSize];  //creat blank array for a single page
//							checksumofBlockData -= pageArrays[pageNum,(pageArrays.Length-k-1)];
//						}
//						//now remove the last 4 bytes form the data to send
//						int newLength = ODProgBytes.Length-4;
//						byte [] tempByteArray = new byte[newLength];  //crate tempory stroe of the reduced length
//						System.Array.Copy(ODProgBytes,0,tempByteArray,0,newLength);  //copy the data except that last 4 bytes into the temporay stroe
//						ODProgBytes = new byte[newLength];  //now effectively reduce the lengt hof the array to send by re-instantiating it
//						tempByteArray.CopyTo(ODProgBytes,0); //finally drop the data back in
//					}
					#endregion removal of last 4 physical bytes for DSP if necessary 
					uint blockchecksum = addBytesTogether(blockStartAddress);
					blockchecksum += addBytesTogether(blockDataLength);
					blockchecksum += checksumofBlockData;  
					wholeDataChecksum += checksumofBlockData;
					statusMessage = memoryStrings[(int) memType].ToString() + ": " + "Transmitting data page 0x" + pageNum.ToString("X") + " of 0x" + numPages.ToString("X");
					#region diagnostic code
#if CAN_TRAFFIC_DEBUG
					System.Diagnostics.Debug.WriteLine("-----Tx Block data------");
#endif
					#endregion diagnostic code
					sendDataBlock(SEVCONType, ODProgBytes);
					if(this.fatalErrorOccured == true)
					{
						return;
					}
					numBytesLeft -= (uint) ODProgBytes.Length;
					ushort check = (ushort) blockchecksum;  //knock checksum down to a ushort
					check = (ushort) (0xFFFF - check);  //0xFFFF to match Paul's code
					#region diagnostic code
#if CAN_TRAFFIC_DEBUG
					System.Diagnostics.Debug.WriteLine("Tx Block Checksum; 0x" +  check.ToString("X"));
#endif
					#endregion diagnostic code
					sendBlockCheckSum(SEVCONType,  check);
					if(this.fatalErrorOccured == true)
					{
						return;
					}
				}
					#endregion
					#region else this page has not changed - no need to transmit
				else
				{
#if CAN_TRAFFIC_DEBUG
//judetemp					System.Console.Out.WriteLine("Page " + pageNum + " changed? = " + pageChanged[pageNum].ToString());
#endif
					pageNum++;  //move to next page
					statusMessage = SEVCONType.ToString() + ": " + "data page 0x" + pageNum.ToString("X") + " of 0x" + numPages.ToString("X") + ", Tx not required";
					progBarValue = pageNum;
				}
				#endregion
			} //end while 
			#endregion
		}


		private void downloadConvertedEEPROM()
		{
			#region diagnostic code
#if CAN_TRAFFIC_DEBUG
			System.Diagnostics.Debug.WriteLine("Downlaoding Converted EEPROM");
#endif
			#endregion diagnostic code

			#region initialise data transfer
			ushort maxBlockSize = 0, pageSize = 0;
			uint deviceStartAddress = 0,  deviceLength = 0;
			getDeviceParameters(SevconObjectType.BOOT_MEMORY_SPACE_EEPROM, out maxBlockSize, out deviceStartAddress, out deviceLength, out pageSize);
			if(this.fatalErrorOccured == true)
			{
				return;
			}
			uint deviceEndAddress = deviceStartAddress + deviceLength;
			ushort normalBlockSize = (ushort) (maxBlockSize - (maxBlockSize % pageSize));  //round down to nearest pages size
			#endregion
			#region write code to device
			#region status bar and progress bar
			statusMessage =  "Sending converted EEPROM back to device";
			progBarValue = progBarMin;
			progBarMax = this.convertedDeviceEEPROM.Length;
			#endregion status bar and progress bar

			uint MaxNumPagesInBlock = (uint) (maxBlockSize/pageSize);
			uint numBytesLeft = deviceLength;  //not sent any yet
			uint blockStartAddress = deviceStartAddress;
			while(numBytesLeft>0)
			{
				int numPagesLeft = (int) (numBytesLeft/pageSize);
				if((numBytesLeft%pageSize) != 0)  //does remainder divide equally into pages
				{
					numPagesLeft = (int) (numBytesLeft/pageSize) + 1;
				}
				uint MaxNumPagesInThisBlock = (uint) System.Math.Min(MaxNumPagesInBlock, numPagesLeft);  
				ushort blockDataLength = (ushort)(MaxNumPagesInThisBlock * pageSize);
				sendBlockStartAddress(SEVCONType, blockStartAddress);
				if(this.fatalErrorOccured == true)
				{
					return;
				}
				#region diagnostic code
#if CAN_TRAFFIC_DEBUG
				System.Diagnostics.Debug.WriteLine("Tx Block Start Addr; 0x" +  blockStartAddress.ToString("X"));
#endif
				#endregion diagnostic code
				ODProgBytes = new byte[blockDataLength];  //creat byte arry of correct length
				Array.Copy(this.convertedDeviceEEPROM,(blockStartAddress-deviceStartAddress), ODProgBytes,0, blockDataLength);  //copy data in
				numBytesLeft -= blockDataLength;
				#region diagnostic code
#if CAN_TRAFFIC_DEBUG
				System.Diagnostics.Debug.WriteLine("Tx Block Length; 0x" +  blockDataLength.ToString("X"));
#endif
				#endregion diagnostic code
				sendBlockDataLength(SEVCONType, blockDataLength);
				if(this.fatalErrorOccured == true)
				{
					return;
				}
				#region calculate blcok chacksum and update wholedata checksum
				ushort checksumofBlockData = 0;
				for(int i=0;i<blockDataLength;i++)
				{
					checksumofBlockData += ODProgBytes[i];  
				}
				uint blockchecksum = addBytesTogether(blockStartAddress);
				blockchecksum += addBytesTogether(blockDataLength);
				blockchecksum += checksumofBlockData;  
				wholeDataChecksum += checksumofBlockData;
				#endregion calculate blcok chacksum and update wholedata checksum
				statusMessage = "EEPROM: " + "Transmitting byte 0x" + blockStartAddress.ToString("X") + " of 0x" + convertedDeviceEEPROM.Length.ToString("X");
				progBarValue = (int) blockStartAddress;  
				#region diagnostic code
#if CAN_TRAFFIC_DEBUG
				System.Diagnostics.Debug.WriteLine("-----Tx Block data------");
#endif
				#endregion diagnostic code
				sendDataBlock(SEVCONType, ODProgBytes);
				if(this.fatalErrorOccured == true)
				{
					return;
				}
				ushort check = (ushort) blockchecksum;  //knock checksum down to a ushort
				check = (ushort) (0xFFFF - check);  //0xFFFF to match Paul's code
				#region diagnostic code
#if CAN_TRAFFIC_DEBUG
				System.Diagnostics.Debug.WriteLine("Tx Block Checksum; 0x" +  check.ToString("X"));
#endif
				#endregion diagnostic code
				sendBlockCheckSum(SEVCONType,  check);
				if(this.fatalErrorOccured == true)
				{
					return;
				}
				blockStartAddress +=  blockDataLength;  //ready for next time
			} //end while 
			progBarValue = progBarMin;
			#endregion  write code to device
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: sendBlockCheckSum
		///		 *  Description     : Sends the DW calculated checksum for the last transmitted datablock. 
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: blockChecksum  - DW calculated checksum for the last Txd block
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void sendBlockCheckSum(SevconObjectType SEVCONType, uint blockChecksum)
		{
			ODItemData blockChkSub = sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SEVCONType, 0x08);
			if(blockChkSub != null)
			{
				_feedback = sysInfo.nodes[progDeviceIndex].writeODValue(blockChkSub, (long)blockChecksum);
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					this.fatalErrorOccured = true;
				}
			}
			else
			{
				this.fatalErrorOccured = true; 
			}
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: convertEEPROM
		///		 *  Description     : Not curren tused. Once requirements of EEPROM formatting are complete,
		///		 *					: will be used to perform conversion of EEPROM data from one format to another
		///		 *  Parameters      : 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : 
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private bool convertEEPROM()
		{
			byte [] tempBytes;
			//create a byte array to hold a temporary copy of the EEPROM
			convertedDeviceEEPROM = new byte[CopyOfDeviceEEPROM.Length];  //same length as that uploaded
			Array.Copy(this.CopyOfDeviceEEPROM, convertedDeviceEEPROM, CopyOfDeviceEEPROM.Length);  //start with identical copy
			for(int fileIndex = 0;fileIndex<this.EEFormatPath.Count;fileIndex++)
			{
				EEFileHeader fileHeader = (EEFileHeader) EEFormatPath[fileIndex];
				#region open filestream
				FileStream fs = null;
				try
				{
					fs = new FileStream(fileHeader.filepath, FileMode.Open, FileAccess.Read);
				}
				catch
				{
                    fs.Close();
					return false;
				}
				StreamReader sr = new StreamReader(fs);
				#endregion open filestream
				string input = "";
				char [] seperator = {' ', '\t'};
				#region setup some markers to test that commands in file appear int the correct order
				//1:Header line – can occur anywhere in the file but for readability should be first line. 
				//2. Move commands all move commands MUST be placed before ALL set, scale and checksum commands. DW to test compliance with this.
				//3. set and scale commands must all BEFORE all checksum commands. DW to test compliance with this. DW to test this 
				//(set and scale commands can be interspersed)
				bool ee_SetOrScaleOccured = false, ee_checksumOccurred = false;
				#endregion setup some markers to test that commands in file appear int the correct order
				while((input = sr.ReadLine()) != null)
				{
					if(input.IndexOf("#") != -1)
					{
						continue;  //go to next iteration
					}
					else
					{
						string inputLower = input.ToLower();
						inputLower = inputLower.Replace("ox", "0x");
						int splitIndex = 0;
						if(inputLower.IndexOf("ee_move") != -1)
						{
							#region move this data type in eeprom
							if((ee_SetOrScaleOccured == true) || (ee_checksumOccurred == true))
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation . EEPROM will not be converted." 
									+ "\nMove commands must occur before other commands"
									+ "\nFailed line: " + input);
								return false;
							}
							int moveFrom = -1, moveTo = -1, moveSize = -1;
							//next remove everything up to end of "ee_header" including leading whitespace
							inputLower = inputLower.Substring(inputLower.IndexOf("ee_move")+ 7);
							string [] inputSplit = inputLower.Split(seperator);
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) &&  (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract moveFrom
							try
							{
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									moveFrom = System.Convert.ToInt32(inputSplit[splitIndex],16);
								}
								else
								{
									moveFrom = System.Convert.ToInt32(inputSplit[splitIndex]);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract moveFrom
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) && (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space elements
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract moveTo
							try
							{
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									moveTo = System.Convert.ToInt32(inputSplit[splitIndex],16);
								}
								else
								{
									moveTo = System.Convert.ToInt32(inputSplit[splitIndex]);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract moveSize
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) && (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space elements
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract moveSize
							try
							{
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									moveSize = System.Convert.ToInt32(inputSplit[splitIndex],16);
								}
								else
								{
									moveSize = System.Convert.ToInt32(inputSplit[splitIndex]);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract moveSize
							#region check that each parameter has been overwirtten with valid vlaue
							if(( moveFrom == -1) || ( moveTo == -1 )|| (moveSize == -1))
							{
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion check that each parameter has been overwirtten with valid vlaue
							#region  perform the conversion
							Array.Copy(CopyOfDeviceEEPROM,moveFrom, convertedDeviceEEPROM,moveTo,moveSize);
							#endregion  perform the conversion
							#endregion move this data type in eeprom
						}
						else if(inputLower.IndexOf("ee_set") != -1) 
						{
							#region change value of this EEPROM parameter
							if(ee_checksumOccurred == true)
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation . EEPROM will not be converted." 
									+ "\nSet commands must occur before checksum commands"
									+ "\nFailed line: " + input);
								return false;
							}
							ee_SetOrScaleOccured = true;  //set the marker to testing order compliance
							double setValue = 0;
							bool setValueOverwritten = false;
							int setAddr = -1; //start with invalid
							string setType = "";
							inputLower = inputLower.Substring(inputLower.IndexOf("ee_set")+ 6);
							string [] inputSplit = inputLower.Split(seperator);
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) &&  (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract setAddr
							try
							{
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									setAddr = System.Convert.ToInt32(inputSplit[splitIndex],16);
								}
								else
								{
									setAddr = System.Convert.ToInt32(inputSplit[splitIndex]);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract setAddr
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) && (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space elements
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract setType
							if(splitIndex < inputSplit.Length)
							{
								setType = inputSplit[splitIndex];
							}
							splitIndex++;
							#endregion extract setType
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) && (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space elements
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract setValue
							try
							{
								setValueOverwritten = true; //needed because we have no defualt invalid vlaue for a double
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									int temp = System.Convert.ToInt32(inputSplit[splitIndex],16);
									setValue = System.Convert.ToDouble(temp);
								}
								else
								{
									setValue = Double.Parse(inputSplit[splitIndex] ,System.Globalization.NumberStyles.Any);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract setValue
							#region check that each parameter has been overwritten
							if((setValueOverwritten == false) || (setAddr == -1) || (setType == ""))
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion check that each parameter has been overwritten
							#region perform the conversion
							try
							{
								switch(setType)
								{
									case "bool_t":
									case "uint8_t":
									case "char_t":
										convertedDeviceEEPROM[setAddr] = System.Convert.ToByte(setValue);
										break;

									case "int8_t":
										convertedDeviceEEPROM[setAddr] 
											=(byte) System.Convert.ToSByte(setValue);  //second cast is necessary 
										break;

									case "uint16_t":
										System.UInt16 tempUint16 = System.Convert.ToUInt16(setValue);
										tempBytes = new byte[2];
										// copy relevant part of data over into the transmit array
										for ( int i = 0; i < tempBytes.Length; i++ )
										{
											tempBytes[ i ] = (byte)( tempUint16 >> ( 8 * i) );
										}
										Array.Copy(tempBytes,0, convertedDeviceEEPROM, setAddr, tempBytes.Length);
										break;

									case "int16_t":
										System.Int16 tempint16 = System.Convert.ToInt16(setValue);
										tempBytes = new byte[2];
										// copy relevant part of data over into the transmit array
										for ( int i = 0; i < tempBytes.Length; i++ )
										{
											tempBytes[ i ] = (byte)( tempint16 >> ( 8 * i) );
										}
										Array.Copy(tempBytes,0, convertedDeviceEEPROM, setAddr, tempBytes.Length);
										break;
									case "uint32_t":
									case "time_ms_t":
										System.UInt32 tempUInt32 = System.Convert.ToUInt32(setValue);
										tempBytes = new byte[4];
										// copy relevant part of data over into the transmit array
										for ( int i = 0; i < tempBytes.Length; i++ )
										{
											tempBytes[ i ] = (byte)( tempUInt32 >> ( 8 * i) );
										}
										Array.Copy(tempBytes,0, convertedDeviceEEPROM, setAddr, tempBytes.Length);
										break;

									case "int32_t":
										System.Int32 tempInt32 = System.Convert.ToInt32(setValue);
										tempBytes = new byte[4];
										// copy relevant part of data over into the transmit array
										for ( int i = 0; i < tempBytes.Length; i++ )
										{
											tempBytes[ i ] = (byte)( tempInt32 >> ( 8 * i) );
										}
										Array.Copy(tempBytes,0, convertedDeviceEEPROM, setAddr, tempBytes.Length);
										break;

									default:
                                        sr.Close();
                                        fs.Close();  //close streams
										Message.Show("file: " + fileHeader.filepath 
											+ "\nfailed validation. EEPROM will not be converted." 
											+ "\nUnrecognised data type"
											+ "\nFailed line: " + input);
										return false;
								}
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion perform the conversion
							#endregion change value of this EEPROM parameter
						}
						else if(inputLower.IndexOf("ee_scale") != -1) 
						{
							#region scale this parameter
							if(ee_checksumOccurred == true)
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation . EEPROM will not be converted." 
									+ "\nSet commands must occur before checksum commands"
									+ "\nFailed line: " + input);
								return false;
							}
							ee_SetOrScaleOccured = true;
							int scaleAddr = -1;
							string scaleType = "";
							double scaleValue = 0;
							bool scaleValueOverwritten = false;
							double scaleOffset = -1;
							bool scaleOffsetOverwritten = false;
							inputLower = inputLower.Substring(inputLower.IndexOf("ee_scale")+ 8);
							string [] inputSplit = inputLower.Split(seperator);
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) &&  (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract scaleAddr
							try
							{
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									scaleAddr = System.Convert.ToInt32(inputSplit[splitIndex],16);
								}
								else
								{
									scaleAddr = System.Convert.ToInt32(inputSplit[splitIndex]);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract scaleAddr
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) && (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space elements
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract scaleType
							if(splitIndex < inputSplit.Length)
							{
								scaleType = inputSplit[splitIndex];
							}
							splitIndex++;
							#endregion extract scaleType
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) && (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space elements
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract scaleValue
							try
							{
								scaleValueOverwritten = true;
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									int temp = System.Convert.ToInt32(inputSplit[splitIndex],16);
									scaleValue = System.Convert.ToDouble(temp);

								}
								else
								{
									scaleValue = Double.Parse(inputSplit[splitIndex],System.Globalization.NumberStyles.Any);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract scaleValue
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) && (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space elements
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract scaleOffset
							try
							{
								scaleOffsetOverwritten = true;
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									scaleOffset = System.Convert.ToInt32(inputSplit[splitIndex],16);
								}
								else
								{
									scaleOffset = System.Convert.ToInt32(inputSplit[splitIndex]);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract scaleOffset
							#region check that each expected value has been overwritten
							if((	scaleAddr == -1) || (scaleType == "") 
								|| (scaleValueOverwritten == false) 
								|| (scaleOffsetOverwritten == false))
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}

							#endregion check that each expected value has been overwritten
							#region perform the conversion
							try
							{
								#region switch
								switch(scaleType)
								{
									case "uint8_t":
									case "char_t":
										#region unsigned byte
										System.Byte tempUByte = convertedDeviceEEPROM[scaleAddr];
										// Little endian format so can use the same conversion for all integers.
										convertedDeviceEEPROM[scaleAddr] = System.Convert.ToByte(Math.Floor(tempUByte * scaleValue) + scaleOffset);
										#endregion unsigned byte
										break;

									case "int8_t":
										#region signed byte
										//following line must be cast and NOT Convert method - otheriwse any byte starting with F cannot be ocnverted
										System.SByte tempSByte = (SByte) (convertedDeviceEEPROM[scaleAddr]);
										convertedDeviceEEPROM[scaleAddr] 
											=(byte) System.Convert.ToSByte(Math.Floor(tempSByte * scaleValue) + scaleOffset);  //second cast is necessary 
										#endregion signed byte
										break;

									case "uint16_t":
										#region unsigned 16 bit
										System.UInt16 tempUInt16 = 0;
										// Little endian format so can use the same conversion for all integers.
										for ( int i = 1; i >= 0; i-- )
										{
											tempUInt16 <<= 8;
											tempUInt16 += convertedDeviceEEPROM[ scaleAddr +i ];
										}
										tempUInt16 = System.Convert.ToUInt16(Math.Floor(tempUInt16 * scaleValue) + scaleOffset);
										tempBytes = new byte[2];
										// copy relevant part of data over into the transmit array
										for ( int i = 0; i < tempBytes.Length; i++ )
										{
											tempBytes[ i ] = (byte)( tempUInt16 >> ( 8 * i) );
										}
										Array.Copy(tempBytes,0, convertedDeviceEEPROM, scaleAddr, tempBytes.Length);
										#endregion unsigned 16 bit
										break;

									case "int16_t":
										#region int 16
										System.Int16 tempInt16 = 0;
										// Little endian format so can use the same conversion for all integers.
										for ( int i = 1; i >= 0; i-- )
										{
											tempInt16 <<= 8;
											tempInt16 += convertedDeviceEEPROM[ scaleAddr +i ];
										}
										tempInt16 = System.Convert.ToInt16(Math.Floor(tempInt16 * scaleValue) + scaleOffset);
										tempBytes = new byte[2];
										// copy relevant part of data over into the transmit array
										for ( int i = 0; i < tempBytes.Length; i++ )
										{
											tempBytes[ i ] = (byte)( tempInt16 >> ( 8 * i) );
										}
										Array.Copy(tempBytes,0, convertedDeviceEEPROM, scaleAddr, tempBytes.Length);
										#endregion int 16
										break;

									case "uint32_t":
									case "time_ms_t":
										#region unsigned 32
										System.UInt32 tempUInt32 = 0;
										// Little endian format so can use the same conversion for all integers.
										for ( int i = 3; i >= 0; i-- )
										{
											tempUInt32 <<= 8;
											tempUInt32 += convertedDeviceEEPROM[ scaleAddr +i ];
										}
										tempUInt32 = System.Convert.ToUInt32(Math.Floor(tempUInt32 * scaleValue) + scaleOffset);
										tempBytes = new byte[4];
										// copy relevant part of data over into the transmit array
										for ( int i = 0; i < tempBytes.Length; i++ )
										{
											tempBytes[ i ] = (byte)( tempUInt32 >> ( 8 * i) );
										}
										Array.Copy(tempBytes,0, convertedDeviceEEPROM, scaleAddr, tempBytes.Length);
										#endregion unsigned 32
										break;

									case "int32_t":
										#region signed 32 bit
										System.Int32 tempInt32 = 0;
										// Little endian format so can use the same conversion for all integers.
										for ( int i = 3; i >= 0; i-- )
										{
											tempInt32 <<= 8;
											tempInt32 += convertedDeviceEEPROM[ scaleAddr +i ];
										}
										tempInt32 = System.Convert.ToInt32(Math.Floor(tempInt32 * scaleValue) + scaleOffset);
										tempBytes = new byte[4];
										// copy relevant part of data over into the transmit array
										for ( int i = 0; i < tempBytes.Length; i++ )
										{
											tempBytes[ i ] = (byte)( tempInt32 >> ( 8 * i) );
										}
										Array.Copy(tempBytes,0, convertedDeviceEEPROM, scaleAddr, tempBytes.Length);
										#endregion signed 32 bit
										break;

									case "bool_t":
									default:
										#region handle error condition
										fs.Close();  //close streams
										sr.Close();
										Message.Show("file: " + fileHeader.filepath 
											+ "\nfailed validation. EEPROM will not be converted." 
											+ "\nFailed line: " + input);
										return false;
										#endregion handle error condition
								}
								#endregion switch
							}
							catch
							{
								#region handle error condition
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
								#endregion handle error condition
							}
							#endregion perform the conversion
							#endregion scale this parameter
						}
						else if (inputLower.IndexOf("ee_checksum") != -1)
						{
							#region apply a checksum algorithm to an area of eeprom
							ee_checksumOccurred = true;
							int chksumAlgorithm = -1, chksumStart = -1, chksumEnd = -1, chksumIndex = -1;
							//next remove everything up to end of "ee_checksum" including leading whitespace
							inputLower = inputLower.Substring(inputLower.IndexOf("ee_checksum")+ 11);  
							string [] inputSplit = inputLower.Split(seperator);
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) &&  (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract chksumAlgorithm
							try
							{
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									chksumAlgorithm = System.Convert.ToInt32(inputSplit[splitIndex],16);
								}
								else
								{
									chksumAlgorithm = System.Convert.ToInt32(inputSplit[splitIndex]);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract chksumAlgorithm
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) && (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space elements
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract chksumStart
							try
							{
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									chksumStart = System.Convert.ToInt32(inputSplit[splitIndex],16);
								}
								else
								{
									chksumStart = System.Convert.ToInt32(inputSplit[splitIndex]);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract chksumStart
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) && (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space elements
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract chksumEnd
							try
							{
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									chksumEnd = System.Convert.ToInt32(inputSplit[splitIndex],16);
								}
								else
								{
									chksumEnd = System.Convert.ToInt32(inputSplit[splitIndex]);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract chksumEnd
							#region skip whitespace (tabs and spaces)
							while((splitIndex<inputSplit.Length) && (inputSplit[splitIndex].Length == 0))
							{
								splitIndex++; //skip over white space elements
							}
							#endregion skip whitespace (tabs and spaces)
							#region extract chksumIndex
							try
							{
								if(inputSplit[splitIndex].IndexOf("0x") != -1)
								{
									chksumIndex = System.Convert.ToInt32(inputSplit[splitIndex],16);
								}
								else
								{
									chksumIndex = System.Convert.ToInt32(inputSplit[splitIndex]);
								}
								splitIndex++;
							}
							catch
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion extract chksumIndex
							#region check that each parameter has been overwirtten with valid vlaue
							if(( chksumAlgorithm == -1) || ( chksumStart == -1 )
								|| (chksumEnd == -1)	    || (chksumIndex == -1))
							{
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion check that each parameter has been overwirtten with valid vlaue
							#region apply the checksum algorithm
							if(	chksumAlgorithm == 1)  //normal checksum
							{
								int checksumValue = 0;
								byte byteChecksum = 0;
								for(int i = chksumStart;i<=chksumEnd;i++)
								{
									checksumValue += convertedDeviceEEPROM[i];
								}
								checksumValue = 0x100 - (checksumValue & 0xFF);
								byteChecksum = (byte) (checksumValue);
								try
								{
									convertedDeviceEEPROM[chksumIndex] = byteChecksum;
								}
								catch
								{
                                    sr.Close();
                                    fs.Close();  //close streams
									Message.Show("file: " + fileHeader.filepath 
										+ "\nfailed validation. EEPROM will not be converted." 
										+ "\nFailed line: " + input);
									return false;
								}
							}
							else if(chksumAlgorithm == 2)  //EEPROM format checksum
							{
								//Checksum is initialised to 0x3A5C. 
								//Bytes from ‘start’ to ‘end’ are added to the checksum 
								//and the result is AND’ed with 0xFFFF. 
								//Result is a word which is written to checksum_index
								//in little endian format. 
								int checksumEEformat = 0x3A5C;
								for(int i = chksumStart;i<=chksumEnd;i++)
								{
									checksumEEformat += convertedDeviceEEPROM[i];
								}
								checksumEEformat = checksumEEformat & 0xFFFF;
								System.Int16 tempint16 = System.Convert.ToInt16(checksumEEformat);
								tempBytes = new byte[2];
								// copy relevant part of data over into the transmit array
								for ( int i = 0; i < tempBytes.Length; i++ )
								{
									tempBytes[ i ] = (byte)( tempint16 >> ( 8 * i) );
								}
								Array.Copy(tempBytes,0, convertedDeviceEEPROM, chksumIndex, tempBytes.Length);
							}
							else  //unhandled algorithm
							{
                                sr.Close();
                                fs.Close();  //close streams
								Message.Show("file: " + fileHeader.filepath 
									+ "\nfailed validation. (Invalid chekcsum algorithm) EEPROM will not be converted." 
									+ "\nFailed line: " + input);
								return false;
							}
							#endregion apply the checksum algorithm
							#endregion apply a checksum algorithm to an area of eeprom
						}
					}
				}
				//At the end of converison of this file 
				//copy whole converted array back to the source array
				//ie the ouptup of this file becomes the inpout to the next file
				//now copy back to source ready for any later move commands
				Array.Copy(convertedDeviceEEPROM, 0,CopyOfDeviceEEPROM, 0,convertedDeviceEEPROM.Length);
			}
			return true;
		}
	
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: extractCodeFromDldFile
		///		 *  Description     : Gets the code data from the Downlaod file, checks it an dstroes it read for downloading to controller
		///		 *					: Takes some toime so this porcess is now done AFTER user confirms download file. 
		///		 *  Parameters      : 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		
		private void extractCodeFromDldFile(string DldFilename)
		{
			string input = "";
			rewind(DldFilename);
			progBarValue = progBarMin;
			progBarMax = numLinesInDldFile;
			statusMessage = "Retrieving code data from download file";
			userInstructionMessage = "Please wait";
			newCode = new string [numMemSpacesused];  //DR38000258 inc'd to 50 for generic spaces
			numRecords = new int [numMemSpacesused];
			for(int i=0;i<numMemSpacesused;i++)
			{
				newCode[i] = "";
				numRecords[i] = 0;
			}
			//Memory spaces sections
            ushort[] bitmasks = { 0x0001, 0x0002, 0x0004, 0x0008 };  //DR38000258 correct bit
			statusMessage =  "Reading and storing code data";
			progBarValue = progBarMin;
			StringBuilder myStrBuilder = new StringBuilder();
			while (  (input = _sr.ReadLine()) != null )  //while not eof
			{
				progBarValue++;
				if ( input.IndexOf( "+TARGET_MEMORY_SPACE" ) != -1 ) //found first section that we are intereseted in
				{
					#region finding & storing code section
					//clear the new code strings
					int MemNum = 0;  
					while( (input = _sr.ReadLine()) != null) //not eof
					{
						input = input.Trim();
						if((input.IndexOf('+')!= -1 ) ||(input.Length <= 0) ||(input.IndexOf("//")!= -1 ) )
						{
							if(input.IndexOf("+FILE_CHECKSUM")!= -1 )  //end of target areas section
							{
								break;
							}
							if(input.IndexOf("+ENDCODE") != -1)
							{
								newCode[MemNum] = myStrBuilder.ToString();
							}
							continue;
						}
						if(input.IndexOf(':')== -1 ) //not code
						{
							MemNum = Int16.Parse(input);  //convert to 16 bit integer;
							myStrBuilder = new StringBuilder();
						}
						else 
						{
							if(MemNum<numMemSpacesused)
							{
								myStrBuilder.Append(input);
								numRecords[MemNum]++;
							}
						}
						progBarValue++;
					}
					progBarValue = progBarMin;
					#endregion finding & storing code section
				}
			}
		}

		#endregion code transmission

		#region code transfer finalisation
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: completeDownloadControl
		///		 *  Description     : Wrapper function used to allow the download finalisation to be run on a seperate thread. 
		///		 *  Parameters      : 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : 
		///		 *--------------------------------------------------------------------------*/
		/// </summary>

        public void completeDownloadControl()
        {
            //DR38000244 19/Mar/08 JW prevent any possibility of DW sending final checksum if progrramming failed
            //Copied into devleopment code
            if (this.fatalErrorOccured == false)
            {
                userInstructionMessage = "Please wait";
                statusMessage = "Waiting for application to launch (may take up to 1 minute)";
                sendFinalChecksum(SevconObjectType.BOOT_COMPLETE, wholeDataChecksum);
            }
        }
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: sendFinalChecksum
		///		 *  Description     : Transmits  final checksum to the controller once all data has been transmitted
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: wholeDataChecksum
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : 
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void sendFinalChecksum(SevconObjectType SEVCONType, uint wholeDataChecksum)
		{
			ushort controllerWholeDataChecksum = 0;//debug only judetemp
			long ODvalue = (long)wholeDataChecksum; //wholeDataChecksum;

            //DR38000172
            // Turn filters off since programming is now finished so we can detect the heartbeat
            // ie accept IDs of     = XXXX XXXX XXXX XXXX (all CANIDs)
            sysInfo.CANcomms.VCI.SetupCAN(sysInfo.CANcomms.systemBaud, 0x00, 0x00);
            Thread.Sleep(200);      //give time for new filter to take effect

			ODItemData chkSumSub = sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SEVCONType, 0x0);
			if(chkSumSub != null)
			{
				DriveWizard.SEVCONProgrammer.programmingRequestFlag = true;  //used in MAin Window to supress bootup warning
#if CAN_TRAFFIC_DEBUG
			System.Diagnostics.Debug.WriteLine("Tx Final Checksum; 0x" +  wholeDataChecksum.ToString("X"));
#endif
				MAIN_WINDOW.appendErrorInfo = false; //we won't get a response if write is OK
				_feedback = sysInfo.nodes[progDeviceIndex].writeODValue(chkSumSub, ODvalue);
				MAIN_WINDOW.appendErrorInfo = true;
				if((_feedback != DIFeedbackCode.DINoResponseFromController) && (_feedback != DIFeedbackCode.DISuccess))
				{
					if(_feedback == DIFeedbackCode.CANGeneralError)
					{
						//chekc for scenario in which the user has sent no code at all - ie just extracted EEPORM contens
						//known facet of device bootloader that we will see 'wrong' chekcum in this scenario
						if((this._EEPROMFormatsMatch == true)  && (this.newCode.Length == 0))//
						{
							getControllerWholeDataChecksum(SevconObjectType.BOOT_COMPLETE, out controllerWholeDataChecksum);  //debug only judetemp
							_feedback = sysInfo.nodes[progDeviceIndex].writeODValue(chkSumSub, controllerWholeDataChecksum);
							if(_feedback != DIFeedbackCode.DINoResponseFromController)
							{
							}
						}
						else
						{
							SystemInfo.errorSB.Append("\nUnable to release controller from bootloader. Error code:");
							SystemInfo.errorSB.Append(get_espAC_Error());
							//now get the controlle rrepoted checksum - don't do before -- it overwirtes the feedback code
							getControllerWholeDataChecksum(SevconObjectType.BOOT_COMPLETE, out controllerWholeDataChecksum);  //debug only judetemp
							return;
						}
					}
				}
				long timeDiff = 0;
				long startTimeSecs = System.DateTime.Now.Ticks;
				while(1<2)  
				{
					timeDiff = (System.DateTime.Now.Ticks) - startTimeSecs;
					if((this.sysInfo.nodes.Length == 0) //in case a bootup messag ehas caused nodes to be cleared
						|| (this.sysInfo.nodes[progDeviceIndex].nodeState == NodeState.Bootup) 
						|| (this.sysInfo.nodes[progDeviceIndex].nodeState == NodeState.PreOperational)
						|| (this.sysInfo.nodes[progDeviceIndex].nodeState == NodeState.Operational))
					{
						return;
					}
					if(timeDiff >= 150000000)  //15 seconds
					{
						SystemInfo.errorSB.Append("\nApplication has not started with 15 seconds. This node's baud rate may have changed");  //SC now does bootup message
						return;
					}
				}
			}
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: getControllerWholeDataChecksum
		///		 *  Description     : Once DW has written its calculated whole data checksum to the controller,
		///		 *					: the controller calculated whole data checksum can be read back form the sma elocation. 
		///		 *					: This function is used for debugging purposes only 
		///		 *  Parameters      : SEVCONType - used to uniquely identify an object in the OD whilst being OD Index independent
		///		 *					: out wholeDataChecksum
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : 
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void getControllerWholeDataChecksum(SevconObjectType SEVCONType, out ushort controllerWholeDataChecksum)
		{
			controllerWholeDataChecksum = 0;
			ODItemData wholeDataChkSumSub = this.sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SEVCONType, 0x0);
			if(wholeDataChkSumSub != null)
			{
				_feedback = this.sysInfo.nodes[progDeviceIndex].readODValue(wholeDataChkSumSub);
#if CAN_TRAFFIC_DEBUG
			System.Diagnostics.Debug.WriteLine("Final Checksum reported by Controller; 0x" +  ODvalue.ToString("X"));
#endif 
				if(_feedback != DIFeedbackCode.DISuccess)
				{
					SystemInfo.errorSB.Append( "Unable to read ");
					SystemInfo.errorSB.Append(SEVCONType.ToString());
					SystemInfo.errorSB.Append( " final wholeDataChecksum.\nError code: "); 
					if(_feedback == DIFeedbackCode.CANGeneralError)  //general abort from Pauls's code ??
					{
						SystemInfo.errorSB.Append(get_espAC_Error());
					}
					else
					{
						SystemInfo.errorSB.Append(_feedback.ToString());
					}
					return;
				}
				controllerWholeDataChecksum = System.Convert.ToUInt16(wholeDataChkSumSub.currentValue);
			}
		}

		#endregion code transfer finalisation
	
		#region viewing/saving EEPROM
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: writeEEPROMDataToBackupFile
		///		 *  Description     : Creates a disk file in Downlaod file format of the current contents of controller EEPROM. 
		///		 *					: This format allows the original EEPROM data to be re-sent to the controller
		///		 *  Parameters      : 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : 
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		public void writeEEPROMDataToBackupFile(string EEPROMSaveFile)
		{
			string sectionSeperator = "//-------------------------------------------";
            try
            {
                _fsWrite = new FileStream(EEPROMSaveFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                _srWrite = new StreamWriter(_fsWrite);
            }
            catch
            {
         		Message.Show("File: " + EEPROMSaveFile 
						+ " could not be opened for write access. Contact your system administrator");
                _srWrite.Close();   // close streams
                _fsWrite.Close();
                return;
            }

			FileInfo fileInfo = new FileInfo(EEPROMSaveFile);
			statusMessage = "Copying EEPROM to file";
			#region pre-log to adding EEPROM code
			_srWrite.WriteLine("+FILE_VERSION");			
			_srWrite.WriteLine();
			_srWrite.WriteLine("//fileversion - n/a - EEPROM backup");					
			_srWrite.WriteLine();

			_srWrite.WriteLine("//Software versions contained in this file");
			_srWrite.WriteLine();
			_srWrite.WriteLine("+DOWNLOAD_VERSIONS");
			_srWrite.WriteLine();
			_srWrite.WriteLine(hostAppController);					
			_srWrite.WriteLine(DSPAppController);	

			_srWrite.WriteLine();
			_srWrite.WriteLine(sectionSeperator);

			_srWrite.WriteLine("//Target compatibility section");
			_srWrite.WriteLine();
			_srWrite.WriteLine("//Known compatible and non-compatible target hardware versions");
			_srWrite.WriteLine();
			_srWrite.WriteLine("+GOOD_HARDWARE");
			_srWrite.WriteLine();
			_srWrite.WriteLine("+BAD_HARDWARE");
			_srWrite.WriteLine();
			_srWrite.WriteLine(sectionSeperator);
			_srWrite.WriteLine("+EEPROM_FORMAT");
			_srWrite.WriteLine(deviceEEFormat);  //insert device EEProm format
			_srWrite.WriteLine();
			_srWrite.WriteLine(sectionSeperator);
			_srWrite.WriteLine();
			_srWrite.WriteLine("//Host Internal ROM code section");
			_srWrite.WriteLine();
			_srWrite.WriteLine("+TARGET_MEMORY_SPACE");
			_srWrite.WriteLine();
			_srWrite.WriteLine("+CODE"); 
			_srWrite.WriteLine();
			_srWrite.WriteLine("+ENDCODE");
			_srWrite.WriteLine();
			_srWrite.WriteLine(sectionSeperator);

			_srWrite.WriteLine("//DSP code section");
			_srWrite.WriteLine();
			_srWrite.WriteLine("+TARGET_MEMORY_SPACE");
			_srWrite.WriteLine();
			_srWrite.WriteLine("+CODE"); 
			_srWrite.WriteLine();
			_srWrite.WriteLine("+ENDCODE");
			_srWrite.WriteLine(sectionSeperator);
			_srWrite.WriteLine();

			_srWrite.WriteLine("//EEPROM code section");
			_srWrite.WriteLine();
			_srWrite.WriteLine("+TARGET_MEMORY_SPACE");
			_srWrite.WriteLine();
			_srWrite.WriteLine("2");  //add EEPROM memtype coz we have EEPROM to backup here
			_srWrite.WriteLine("");
			_srWrite.WriteLine("+CODE");
			_srWrite.WriteLine();
			#endregion pre-log to adding EEPROM code

			#region EEPROM
			ushort addr = 0;
			byte dLength = 0;
			string dataStr = "";
			_srWrite.WriteLine(":020000040000FA");
			for(int byteIndex = 0;byteIndex<this.CopyOfDeviceEEPROM.Length;byteIndex++)
			{
				#region gather data
				string tempStr = this.CopyOfDeviceEEPROM[byteIndex].ToString("X");
					if(tempStr.Length<2)
					{
						tempStr = "0" + tempStr;  //
					}
				dataStr += tempStr;
				dLength++;
				#endregion data
				if( (dLength>=0x20)  || (byteIndex >= CopyOfDeviceEEPROM.Length) )//record full or last record
				{
					#region convert datalength to two char string
					string DLengthStr = dLength.ToString("X");
					if(DLengthStr.Length <2)
					{
						DLengthStr = "0" + DLengthStr;
					}
					#endregion convert datalength to two char string

					#region convert address to 4 char string
					string addrStr = addr.ToString("X");
					while(addrStr.Length<4)
					{addrStr = "0" + addrStr;}
					#endregion convert address to 4 char string

					string record = DLengthStr + addrStr + "00" + dataStr;
					#region calculate and append record wholeDataChecksum
					//now calculate and add the record wholeDataChecksum
					int byteptr = 0;
					byte recChkSum = 0;
					string chkSumStr = "";
					for(int i=0;i<((record.Length)/2);i++)  //knock off 1 for the colon
					{
						string recordCopy = record;  //take copy of original record
						recordCopy = recordCopy.Substring(byteptr,2); //now butcher the copy
						recChkSum += Convert.ToByte(recordCopy,16); //and mush it into the wholeDataChecksum
						byteptr +=2;  //point to the next (victim) byte
					}
					recChkSum = (byte) (0x100 - recChkSum);
					chkSumStr = recChkSum.ToString("X");
					if(chkSumStr.Length<2)
					{
						chkSumStr = "0" + chkSumStr;
					}
					#endregion calculate and append record wholeDataChecksum
					record = ":" + record + chkSumStr;  //prefix with colon and append the record wholeDataChecksum
					_srWrite.WriteLine(record);
					#region update parameters ready for the next cricuit of the inner for loop
					addr += dLength;  //increment the address ready for next loop
					dataStr = ""; //reset the data string
					dLength = 0; //and reset the datalength
					#endregion update parameters ready for the next cricuit of the inner for loop
				}
			}
			#endregion
			_srWrite.WriteLine(":00000001FF");
			_srWrite.WriteLine();
			#region post-adding EEPROM code file writes
			_srWrite.WriteLine("+ENDCODE");
			_srWrite.WriteLine(sectionSeperator);
			_srWrite.WriteLine();

			_srWrite.WriteLine("//Host External ROM code section");
			_srWrite.WriteLine();
			_srWrite.WriteLine("+TARGET_MEMORY_SPACE");
			_srWrite.WriteLine();
			_srWrite.WriteLine("+CODE"); 
			_srWrite.WriteLine();
			_srWrite.WriteLine("+ENDCODE");
			_srWrite.WriteLine(sectionSeperator);
			_srWrite.WriteLine();
			#endregion post-adding EEPROM code file writes
			#region new backup section
			//			srWrite.WriteLine("+BACKUP");
			//			srWrite.WriteLine("Host Application software version: " + this.hostVer);
			//			srWrite.WriteLine("DSP application software version: " + this.dspVer);
			//			srWrite.WriteLine("Host bootloader software version: " + this.hostBootVer);
			//			srWrite.WriteLine("DSP bootloader software version: " + this.dspBootVer);
			//			srWrite.WriteLine("Controller hardware version: " + this.hardwareVer);
			//			srWrite.WriteLine("Controller EEPROM format: " + this.deviceEEFormat);
			//			srWrite.WriteLine(sectionSeperator);
			//			srWrite.WriteLine();
			_srWrite.WriteLine("+FILE_CHECKSUM");
			_srWrite.WriteLine();
			_srWrite.WriteLine("+EOF");
			#endregion new backup section
			_srWrite.Close();
			_fsWrite.Close();
		}

		#endregion viewing/saving EEPROM

		#region Utils
		private void extractVersions( string ODvalue, out string localhostVer, out string localdspVer)
		{
			ODvalue = ODvalue.ToUpper();
			if(((SCCorpStyle.ProductRange) this.sysInfo.nodes[progDeviceIndex].productRange  == SCCorpStyle.ProductRange.SEVCONDISPLAY)
                || ((SCCorpStyle.ProductRange) this.sysInfo.nodes[progDeviceIndex].productRange  == SCCorpStyle.ProductRange.CALIBRATOR)) //DR38000256
			{

				localhostVer = ODvalue;
				localdspVer = "---";
			}
			else
			{
				string temp = ODvalue;
				if(temp.Length>=10)
				{
					localhostVer = temp.Substring(0,10);
				}
				else
				{
					localhostVer = "not available from node";
				}
				temp = ODvalue;
				if(temp.Length>=21)
				{
					localdspVer = temp.Substring(11,10);
				}
				else
				{
					localdspVer = "not available from node";
				}
			}
		}
		public string convertToMemNames(ushort setBits)
		{
			string memNames = "";
			ushort localSetBits = setBits;
		
			if ((localSetBits & 0x0001) >0)
				memNames += MemSpaces.HostIntROM.ToString() + ", ";
			
			localSetBits = setBits;
			if ((localSetBits & 0x0002)  >0)
				memNames += MemSpaces.DSPROM.ToString() + ", ";
			
			localSetBits = setBits;
			if ((localSetBits & 0x0004)  >0)
				memNames += MemSpaces.EEPROM.ToString() + ", ";

			localSetBits = setBits;
			if ((localSetBits & 0x0008)  >0)
				memNames += MemSpaces.HostExtROM.ToString() + ", ";

			if(memNames.Length>0)
				memNames = memNames.Remove((memNames.Length-2),2);  //remove trailing comma and space
			return memNames;
		}
		public string convertToProcNames(ushort setBits)
		{
			string procNames = "";
			ushort localSetBits = setBits;
			if ((localSetBits & 0x0001) >0)
				procNames += Processors.Host.ToString() + ", ";
			localSetBits = setBits;
			if ((localSetBits & 0x0002)  >0)
				procNames += Processors.DSP.ToString() + ", ";
			if(procNames.Length>0)
				procNames = procNames.Remove((procNames.Length-2),2);  //remove trailing comma and space
			return procNames;
		}

		private ushort addBytesTogether(uint input)
		{
			uint temp;
			ushort sum = 0;
			uint [] masks = {0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000};
			
			for(int i = 0;i<4;i++)
			{
				temp = input;
				temp = temp & masks[i];
				temp = temp >> (8*i);
				sum += (ushort) (temp);
			}
			return sum;
		}

		private ushort addBytesTogether(ushort input)
		{
			ushort temp = 0, sum = 0;
			ushort [] masks = {0x00FF, 0xFF00};
			for(int i=0;i<2;i++)
			{
				temp = input;
				temp = (ushort) (temp & masks[i]);
				ushort shift = (ushort) (8*i);
				temp = (ushort) (temp >> shift);
				sum += (ushort) (temp);
			}
			return sum;
		}

		private string get_espAC_Error()
		{
			ODItemData bootErrSUb = this.sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SevconObjectType.BOOT_ERROR_CODE, 0x00);
			if(bootErrSUb != null)
			{
				_feedback = sysInfo.nodes[progDeviceIndex].readODValue(bootErrSUb);
#if CAN_TRAFFIC_DEBUG
			System.Diagnostics.Debug.WriteLine("EspAC error Code from controller; 0x" +  bootErrSUb.currentValue.ToString("X"));
#endif
				if(_feedback == DIFeedbackCode.DISuccess)
				{
					string temp = bootErrSUb.currentValue.ToString("X").TrimStart('0').ToUpper();
				
					if(espAC_ErrorCodes_ht[temp] == null)
					{
						return "Missing espAC Error Code: " + temp;
					}
					else
					{
						return espAC_ErrorCodes_ht[temp].ToString(); //find out what the espAc has to say about this error
					}
				}
				else
				{
					return "Unable to read node error code" + _feedback.ToString();
				}
			}
			return "No information from node";
		}
		public void forceNodeOutOfBoot()
		{
			ODItemData bootCompleteSub = sysInfo.nodes[progDeviceIndex].getODSubFromObjectType(SevconObjectType.BOOT_COMPLETE, 0x0);
			DIFeedbackCode fbc = sysInfo.nodes[progDeviceIndex].writeODValue(bootCompleteSub, 0x00);  //don't seek error messages causes circular logic
			if(fbc != DIFeedbackCode.DISuccess)
			{
				return;
			}
		}

        // DR38000258 converts memorySpace into enum type, where all values 5 
        // or greater are Generic Space (inline with DVT dld files with TARGET_MEMORY)
        private MemSpaces getMemSpace(int memorySpace)
        {
            MemSpaces memSpace = MemSpaces.GenMemSpace;

            if (memorySpace < (int)MemSpaces.GenMemSpace)
            {
                memSpace = (MemSpaces)memorySpace;
            }

            return (memSpace);
        }
		#endregion Utils

		#region SEVCONProgrammer finalisation
		public void Dispose()
		{
			//close all streams
			if(_fsWrite  != null)
			{
				_fsWrite.Close();
			}
			if(_fs != null)
			{
				_fs.Close();
			}
			if(_srWrite != null)
			{
				_srWrite.Close();
			}
			if(_sr != null)
			{
				_sr.Close();
			}
		}
		#endregion SEVCONProgrammer finalisation
	}
	#endregion programmer class
	
}
