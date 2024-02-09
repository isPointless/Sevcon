/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.66$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:13:14$
	$ModDate:05/12/2007 22:05:36$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION

REFERENCES    

MODIFICATION HISTORY
    $Log:  36735: ESTABLISH_CAN_COMMS_WINDOW.cs 

   Rev 1.66    05/12/2007 22:13:14  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace DriveWizard
{
	#region enumerated types
	public enum CANCommsCol {Manuf , Type, NodeID};
	#endregion  enumerated types

	#region Establish CAN Comms Form Class
	/// <summary>
	/// Summary description for ESTABLISH_CAN_COMMS_WINDOW.
	/// </summary>
	public class ESTABLISH_CAN_COMMS_WINDOW : System.Windows.Forms.Form
	{
		#region controls declarations
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Timer timer1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Button testBtn;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Button loginBtn;
		private System.Windows.Forms.ErrorProvider errorProvider1;
		private System.Windows.Forms.ErrorProvider errorProvider2;
		private System.Windows.Forms.ToolTip nodeTip;
		private System.Windows.Forms.ToolTip psswrdtip;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ToolTip userTip;
		private System.Windows.Forms.Button closeBtn;
		private System.Timers.Timer timer2;
		#endregion

		#region my declarations
		private EstCommsTable table;
		private SystemInfo localSystemInfo;
		private Thread tListenIn = null, tFindNodes = null, tUserBaud = null, nodeDataRetrieval = null;
		private int underTest = 0;
		private BaudRate userBaud = BaudRate._1M;
		private DIFeedbackCode feedback = DIFeedbackCode.DICodeUnset;
		private bool singleNode = false;
		string selectedNodeText = "";
		int nodeIndex = 0;
		string [] baudRates = {"1M", "800k", "500k", "250k", "125k", "50k", "20k", "10k", "Auto detect" };
		#region login declarations
		uint requestUserIDNo;
		uint passwordNumericValue;
		string [] LoginIDs;
		private int selectednodeID = 201;
		#endregion login declarations
		public static bool ODsRead = false;   //set rue to indicate that the ODs have been read once in this DW applicaiton run
		private StatusBar statusbar = null;
		private ToolBar toolbar = null;
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
			this.dataGrid1 = new System.Windows.Forms.DataGrid();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.closeBtn = new System.Windows.Forms.Button();
			this.testBtn = new System.Windows.Forms.Button();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.timer2 = new System.Timers.Timer();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.loginBtn = new System.Windows.Forms.Button();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider();
			this.errorProvider2 = new System.Windows.Forms.ErrorProvider();
			this.nodeTip = new System.Windows.Forms.ToolTip(this.components);
			this.psswrdtip = new System.Windows.Forms.ToolTip(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.userTip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.timer2)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.CausesValidation = false;
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(791, 25);
			this.label1.TabIndex = 22;
			this.label1.Text = "Search for connected nodes";
			// 
			// dataGrid1
			// 
			this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGrid1.DataMember = "";
			this.dataGrid1.FlatMode = true;
			this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid1.Location = new System.Drawing.Point(8, 48);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.ParentRowsVisible = false;
			this.dataGrid1.PreferredColumnWidth = 325;
			this.dataGrid1.ReadOnly = true;
			this.dataGrid1.RowHeadersVisible = false;
			this.dataGrid1.Size = new System.Drawing.Size(799, 248);
			this.dataGrid1.TabIndex = 20;
			this.dataGrid1.TabStop = false;
			this.dataGrid1.Resize += new System.EventHandler(this.dataGrid1_Resize);
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(8, 576);
			this.progressBar1.Maximum = 8;
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(799, 25);
			this.progressBar1.Step = 1;
			this.progressBar1.TabIndex = 23;
			// 
			// closeBtn
			// 
			this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeBtn.CausesValidation = false;
			this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.closeBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.closeBtn.Location = new System.Drawing.Point(687, 544);
			this.closeBtn.Name = "closeBtn";
			this.closeBtn.Size = new System.Drawing.Size(121, 25);
			this.closeBtn.TabIndex = 21;
			this.closeBtn.TabStop = false;
			this.closeBtn.Text = "&Close window";
			this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
			// 
			// testBtn
			// 
			this.testBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.testBtn.CausesValidation = false;
			this.testBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.testBtn.Location = new System.Drawing.Point(720, 304);
			this.testBtn.Name = "testBtn";
			this.testBtn.Size = new System.Drawing.Size(88, 25);
			this.testBtn.TabIndex = 23;
			this.testBtn.TabStop = false;
			this.testBtn.Text = "&Search";
			this.testBtn.Click += new System.EventHandler(this.testBtn_Click);
			// 
			// timer1
			// 
			this.timer1.Interval = 200;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// comboBox1
			// 
			this.comboBox1.CausesValidation = false;
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.Location = new System.Drawing.Point(240, 304);
			this.comboBox1.MaxDropDownItems = 10;
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(192, 26);
			this.comboBox1.TabIndex = 1;
			this.comboBox1.SelectionChangeCommitted += new System.EventHandler(this.comboBox1_SelectionChangeCommitted);
			// 
			// label2
			// 
			this.label2.CausesValidation = false;
			this.label2.Location = new System.Drawing.Point(8, 304);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(216, 24);
			this.label2.TabIndex = 24;
			this.label2.Text = "Manually Enter Baud Rate?";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// timer2
			// 
			this.timer2.Enabled = true;
			this.timer2.Interval = 250;
			this.timer2.SynchronizingObject = this;
			this.timer2.Elapsed += new System.Timers.ElapsedEventHandler(this.timer2_Elapsed);
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.CausesValidation = false;
			this.label3.Location = new System.Drawing.Point(16, 496);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 25);
			this.label3.TabIndex = 11;
			this.label3.Text = "User ID";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label3.Visible = false;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label4.CausesValidation = false;
			this.label4.Location = new System.Drawing.Point(360, 496);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(80, 23);
			this.label4.TabIndex = 10;
			this.label4.Text = "Password";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label4.Visible = false;
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.textBox1.Location = new System.Drawing.Point(440, 496);
			this.textBox1.MaxLength = 5;
			this.textBox1.Name = "textBox1";
			this.textBox1.PasswordChar = '*';
			this.textBox1.Size = new System.Drawing.Size(256, 26);
			this.textBox1.TabIndex = 3;
			this.textBox1.Text = "";
			this.nodeTip.SetToolTip(this.textBox1, "Numeric Password between 1 and 65535");
			this.textBox1.Visible = false;
			this.textBox1.Validating += new System.ComponentModel.CancelEventHandler(this.textBox1_Validating);
			// 
			// comboBox2
			// 
			this.comboBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.comboBox2.Location = new System.Drawing.Point(80, 496);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(256, 26);
			this.comboBox2.TabIndex = 2;
			this.userTip.SetToolTip(this.comboBox2, "Enter or select your User Identification");
			this.comboBox2.Visible = false;
			this.comboBox2.Validating += new System.ComponentModel.CancelEventHandler(this.comboBox2_Validating);
			// 
			// loginBtn
			// 
			this.loginBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.loginBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.loginBtn.Location = new System.Drawing.Point(8, 544);
			this.loginBtn.Name = "loginBtn";
			this.loginBtn.Size = new System.Drawing.Size(80, 25);
			this.loginBtn.TabIndex = 13;
			this.loginBtn.TabStop = false;
			this.loginBtn.Text = "&Login";
			this.loginBtn.Visible = false;
			this.loginBtn.Click += new System.EventHandler(this.loginBtn_Click);
			// 
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
			// 
			// errorProvider2
			// 
			this.errorProvider2.ContainerControl = this;
			// 
			// ESTABLISH_CAN_COMMS_WINDOW
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
			this.ClientSize = new System.Drawing.Size(812, 612);
			this.Controls.Add(this.loginBtn);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.dataGrid1);
			this.Controls.Add(this.comboBox2);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.testBtn);
			this.Controls.Add(this.closeBtn);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Name = "ESTABLISH_CAN_COMMS_WINDOW";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "ESTABLISH_CAN_COMMS_WINDOW";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ESTABLISH_CAN_COMMS_WINDOW_Closing);
			this.Load += new System.EventHandler(this.ESTABLISH_CAN_COMMS_WINDOW_Load);
			this.Closed += new System.EventHandler(this.ESTABLISH_CAN_COMMS_WINDOW_Closed);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.timer2)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
		/*--------------------------------------------------------------------------
		 *  Name			: ESTABLISH_CAN_COMMS_WINDOW()
		 *  Description     : Constructor function for form. Set up of any initial variables
		 *					  that are available prior to th eform load event.
		 *  Parameters      : systemInfo class, CANopen node number and a descriptive
		 *					  string about the current CANopen node.
		 *  Used Variables  : none
		 *  Preconditions   : This form is always available
		 *  Return value    : none
		 *--------------------------------------------------------------------------*/
		public ESTABLISH_CAN_COMMS_WINDOW(string [] UserIDs, ref SystemInfo systemInfo, StatusBar passed_StatusBar, ToolBar passed_ToolBar )
		{
			// Required for Windows Form Designer support
			InitializeComponent();
			localSystemInfo = systemInfo;
			LoginIDs = new string[5];
			Array.Copy(UserIDs,1, LoginIDs, 0, 5);
			this.statusbar = passed_StatusBar;
			this.toolbar = passed_ToolBar;
		}
		#region login - single node constructor
		public ESTABLISH_CAN_COMMS_WINDOW(int nodeNum, string nodeText, string [] UserIDs, ref SystemInfo systemInfo)
		{
			InitializeComponent();
			localSystemInfo = systemInfo;
			LoginIDs = new string[5];
			Array.Copy(UserIDs,1, LoginIDs, 0, 5);
			//the following are only relevant to single node login
			this.selectednodeID = nodeNum;
			selectedNodeText = nodeText;
			singleNode = true;
		}
		#endregion login - single node constructor
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: ESTABLISH_CAN_COMMS_WINDOW_Load
		///		 *  Description     : Event Handler for form load event
		///		 *  Parameters      : System event arguments 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void ESTABLISH_CAN_COMMS_WINDOW_Load(object sender, System.EventArgs e)
		{
			SCCorpStyle.formatDataGrid(ref this.dataGrid1);
			#region button colouring
			foreach (Control myControl in this.Controls)
			{
				if((myControl.GetType().ToString()) == "System.Windows.Forms.Button")
				{
					myControl.BackColor = SCCorpStyle.buttonBackGround;
				}
			}
			#endregion button colouring
			comboBox1.DataSource = baudRates;
			#region login
			comboBox2.Items.AddRange(LoginIDs);
			requestUserIDNo = 1; //reflects default comboBox Text display 
			passwordNumericValue = 0;	//reset password to invalid value to start 
			#endregion Login

			this.table = new EstCommsTable();
			dataGrid1.DataSource = this.table;
			this.fillDataTable();
			if(	this.singleNode == true)
			{
				showUserControls("SINGLENODE");
				this.Text = "Drive Wizard: Login to single SEVCON node";
				this.AcceptButton  = this.loginBtn;
			}
			else
			{
				this.Text = "Drive Wizard: Test CAN communications and SEVCON nodes login";
				showUserControls("BUSTEST");
				if(this.localSystemInfo.noOfSevconApplicationNodes>0)
				{
					this.AcceptButton  = this.loginBtn;
				}
				else
				{
					this.AcceptButton = this.testBtn;
				}
				//this.comboBox1.Focus();
			}

		}
		#endregion

		#region user interaction zone
		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if ( tListenIn != null )
			{
				#region listen in 
				if ( this.localSystemInfo.CANcomms.baudUnderTest.GetHashCode() > underTest )
				{
					underTest = this.localSystemInfo.CANcomms.baudUnderTest.GetHashCode();
					progressBar1.Value = underTest;
				}
				//if thread stopped
				if ( ( tListenIn.ThreadState & System.Threading.ThreadState.Stopped ) > 0 )
				{
					if ( this.localSystemInfo.CANcomms.systemBaud == BaudRate._unknown )
					{
						timer1.Enabled = false;
						tListenIn = null;
						statusbar.Panels[2].Text = "Failed to autodetect baud rate";
						label1.Text = "Check connections and search again. Or select baud rate to test";
						showUserControls("BUSTEST");
					}
					else
					{
						statusbar.Panels[2].Text = "Searching for connected nodes";
						underTest = 0;
						tListenIn = null;
						this.progressBar1.Value = this.progressBar1.Minimum;  //reset progress bar becaseu showUserCOntrols not called here
						#region findsystem thread
						// setup findSystem thread and make it a background task then start thread running
						tFindNodes = new Thread( new ThreadStart( listenForNodes ));				
						tFindNodes.Name = "FindNodes";
						tFindNodes.Priority = ThreadPriority.Normal;
						tFindNodes.IsBackground = true;
#if DEBUG
						System.Console.Out.WriteLine("Thread: " + tFindNodes.Name + " started");
#endif
						tFindNodes.Start();
						#endregion findsystem thread
					}
				}
				#endregion listen in 
			}
			else if ( tFindNodes != null )
			{
				#region auto hunting for nodes
				if ( this.localSystemInfo.CANcomms.nodeUnderTest > underTest )
				{
					underTest = this.localSystemInfo.CANcomms.nodeUnderTest;
					progressBar1.Value = underTest; //progress bar runs 0 to 8
				}
				//if thread stopped
				if ( ( tFindNodes.ThreadState & System.Threading.ThreadState.Stopped ) > 0 )
				{
					timer1.Enabled = false;
					tFindNodes = null;

					if ( localSystemInfo.noOfNodes == 0 ) 
					{
						statusbar.Panels[2].Text = "No nodes found";
						showUserControls("BUSTEST");
					}
					else
					{
						fillDataTable();
						string baudStr = this.localSystemInfo.baudRate.ToString();
						baudStr = baudStr.Substring(1, baudStr.Length-1);
						if(this.localSystemInfo.noOfNodes == 1)
						{
							statusbar.Panels[0].Text = "CANbus: " + this.localSystemInfo.noOfNodes + " node, " + baudStr + " baud";
							try
							{
								Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\CommsOKGreen.ico");
								statusbar.Panels[0].Icon = icon;
							}
							catch 
							{
								//do nothing , just don't display the icon
							}

						}
						else //just change "node" to "nodes"
						{
							statusbar.Panels[0].Text = "CANbus: " + this.localSystemInfo.noOfNodes + " nodes, " + baudStr + " baud";
							try
							{
								Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\CommsOKGreen.ico");
								statusbar.Panels[0].Icon = icon;
							}
							catch 
							{
								//do nothing , just don't display the icon
							}
						}
						if(this.localSystemInfo.noOfSevconApplicationNodes>0)
						{
							this.AcceptButton = this.loginBtn;
						}
					}
				}
				#endregion auto hunting for nodes
			}
			else if ( tUserBaud != null )
			{
				#region searching at user selected baud rate
				if ( this.localSystemInfo.CANcomms.nodeUnderTest > underTest )
				{
					underTest = this.localSystemInfo.CANcomms.nodeUnderTest;
					progressBar1.Value = underTest;  
				}
				//if thread stopped
				if ( ( tUserBaud.ThreadState & System.Threading.ThreadState.Stopped ) > 0 )
				{
					timer1.Enabled = false;
					tUserBaud = null;
					if ( localSystemInfo.noOfNodes == 0 ) 
					{
						statusbar.Panels[2].Text = "No nodes found at " + this.comboBox1.SelectedText;
						label1.Text = "Incorrect Baud rate, retry with another baud rate";
						showUserControls("BUSTEST");
					}
					else
					{
						fillDataTable();
						string baudStr = this.localSystemInfo.baudRate.ToString();
						baudStr = baudStr.Substring(1, baudStr.Length-1);
						if(this.localSystemInfo.noOfNodes == 1)
						{
							statusbar.Panels[0].Text = "CANbus: " + this.localSystemInfo.noOfNodes + " node, " + baudStr + " baud";
							try
							{
								Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\CommsOKGreen.ico");
								statusbar.Panels[0].Icon = icon;
							}
							catch 
							{
								//do nothing , just don't display the icon
							}
						}
						else
						{
							statusbar.Panels[0].Text = "CANbus: " + this.localSystemInfo.noOfNodes + " nodes, " + baudStr + " baud";
							try
							{
								Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\CommsOKGreen.ico");
								statusbar.Panels[0].Icon = icon;
							}
							catch 
							{
								//do nothing , just don't display the icon
							}

						}
						if(((localSystemInfo.noOfNodesWithValidEDS 
							- localSystemInfo.noOfSevconBootLoaderNodes)>0)
							&& (localSystemInfo.noOfSevconApplicationNodes==0)
							&& ODsRead == false)
						{
							#region get data from first node with EDS
							//first skip over any nodes wit hno EDS - cannot read their OD
							nodeIndex = 0; // B&B
							while( ( this.localSystemInfo.nodes[nodeIndex].manufacturer == Manufacturer.UNKNOWN) 
								&& nodeIndex<this.localSystemInfo.noOfNodes)
							{
								nodeIndex++;
							}
							if(nodeIndex<this.localSystemInfo.noOfNodes)
							{
								this.localSystemInfo.nodes[nodeIndex].itemBeingRead = 0;
								this.progressBar1.Value = this.progressBar1.Minimum;
								this.progressBar1.Maximum = this.localSystemInfo.nodes[nodeIndex].noOfItemsInOD;
								statusbar.Panels[2].Text = "Retrieving data from Node " + this.localSystemInfo.nodes[nodeIndex].nodeID.ToString();
								nodeDataRetrieval = new Thread( new ThreadStart( getNodeData ));				
								nodeDataRetrieval.Name = "Getting data from nodes";
								nodeDataRetrieval.Priority = ThreadPriority.Normal;
								nodeDataRetrieval.IsBackground = true;
#if DEBUG
								System.Console.Out.WriteLine("Thread: " + nodeDataRetrieval.Name + " started");
#endif
								this.timer1.Enabled = true;
								nodeDataRetrieval.Start();
							}
							#endregion get data from first node with EDS
						}
						else
						{
							showUserControls("BUSTEST");
						}
					}
				}
				#endregion searching at user selected baud rate
			}
			else if ( nodeDataRetrieval != null)
			{
				if ( ( nodeDataRetrieval.ThreadState & System.Threading.ThreadState.Stopped ) > 0 )
				{
					#region read ODs from remaing nodes
					if(feedback != DIFeedbackCode.DISuccess)
					{
						//??handled by DI = > status message here
					}
					do
					{
						nodeIndex++;
						if(nodeIndex>= this.localSystemInfo.noOfNodes)
						{
							break;
						}
					}
					while(this.localSystemInfo.nodes[nodeIndex].manufacturer == Manufacturer.UNKNOWN);						
					if(nodeIndex<this.localSystemInfo.noOfNodes)
					{
						this.localSystemInfo.nodes[nodeIndex].itemBeingRead = 0;
						this.progressBar1.Value = this.progressBar1.Minimum;
						this.progressBar1.Maximum = this.localSystemInfo.nodes[nodeIndex].noOfItemsInOD;
						statusbar.Panels[2].Text = "Retrieving data from Node " + this.localSystemInfo.nodes[nodeIndex].nodeID.ToString();
						#region get data from nodes with EDS
						nodeDataRetrieval = new Thread( new ThreadStart( getNodeData ));				
						nodeDataRetrieval.Name = "Getting dataf form nodes";
						nodeDataRetrieval.Priority = ThreadPriority.Normal;
						nodeDataRetrieval.IsBackground = true;
#if DEBUG
						System.Console.Out.WriteLine("Thread: " + nodeDataRetrieval.Name + " started");
#endif
						nodeDataRetrieval.Start();
						#endregion get dat afrom node swith EDS					}
					}
					else
					{
						#region all ODs now read
						this.timer1.Enabled = false; 
						//showUserControls("BUSTEST");
						nodeDataRetrieval = null;
						statusbar.Panels[2].Text = "";
						ODsRead = true;
						if(this.localSystemInfo.noOfSevconApplicationNodes>0)  
						{ //we must have logged in successfully to be here
							this.Close();
						}
						else
						{
							showUserControls("BUSTEST");
						}
						#endregion all ODs now read
					}
					#endregion read ODs from remaing nodes
				}
				else if(this.localSystemInfo.nodes[ nodeIndex ].itemBeingRead <this.progressBar1.Maximum)
				{
					this.progressBar1.Value = this.localSystemInfo.nodes[ nodeIndex ].itemBeingRead;
				}
			}
		}

		private void listenForNodes()
		{
			string abortMessage = "";
			this.progressBar1.Maximum = 8;
			feedback = this.localSystemInfo.findSystem( out abortMessage );
			if(feedback != DriveWizard.DIFeedbackCode.DISuccess)
			{
				if ( feedback == DIFeedbackCode.DIFailedToDetectBaudRate )
				{
					statusbar.Panels[2].Text = "Failed to autodetect baud rate";
					this.label1.Text = "Select manual baud rate below";
				}
				else
				{
					Message.Show("Unable to search for CANopen nodes. \nError code: " + feedback.ToString() + " " + abortMessage);
				}
			}
		}

		private void getNodeData()
		{
			feedback = this.localSystemInfo.readEntireOD(this.localSystemInfo.nodes[nodeIndex].nodeID);
		}
		private void listenForBaudRate()
		{
			this.progressBar1.Maximum = 8;
			feedback = this.localSystemInfo.listenInForBaudRate();

			if(feedback != DriveWizard.DIFeedbackCode.DISuccess)
			{
				if((feedback == DIFeedbackCode.DIFailedToInitialiseHardware)  || (feedback == DIFeedbackCode.DIFailedToStartHardware))
				{
					Message.Show( "Drive Wizard was unable to re-initialise the USB-CAN adapter.\n" 
						+ "Please check that your adapter is connected to the correct USB port of your PC and\n"
						+ "that the drivers have been installed for this port.\n"
						+ "If all LEDs on the adapter are extinguished, disconnect and reconnect the adapter\n"
						+ "to the PC then retry.\n"
						); 
				}
				else if ( feedback != DIFeedbackCode.DIFailedToDetectBaudRate )
				{
					Message.Show("Unable to search at baud rate. \nError code: " + feedback.ToString());
				}

			}
		}
		private void listenForUserSelectedBaudRate()
		{
			string abortMessage = "";
			this.progressBar1.Maximum = 8;
			feedback = localSystemInfo.findSystemAtUserBaudRate( out abortMessage );

			if(feedback != DriveWizard.DIFeedbackCode.DISuccess)
			{
				if((feedback == DIFeedbackCode.DIFailedToInitialiseHardware)  || (feedback == DIFeedbackCode.DIFailedToStartHardware))
				{
					Message.Show( "Drive Wizard was unable to re-initialise the USB-CAN adapter.\n" 
						+ "Please check that your adapter is connected to the correct USB port of your PC and\n"
						+ "that the drivers have been installed for this port.\n"
						+ "If all LEDs on the adapter are extinguished, disconnect and reconnect the adapter\n"
						+ "to the PC then retry.\n"
						); 
				}
				else if ( feedback == DIFeedbackCode.DIFailedToDetectBaudRate )
				{
					statusbar.Panels[2].Text = "Failed to autodetect baud rate";
					this.label1.Text = "Select manual baud rate below";
				}
				else
				{
					Message.Show("Unable to search at selected baud rate. \nError code: " + feedback.ToString() + " " + abortMessage);
				}
			}
		}

		private void testBtn_Click(object sender, System.EventArgs e)
		{
			ODsRead = false; //reset because the DI Data structures will be cleared and refilled 
			hideUserControls();
			this.table.Clear();
			clearToolbarNodeData();
			statusbar.Panels[0].Text = "CANbus: 0 nodes";
			try
			{
				Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\CommsNotOK.ico");
				statusbar.Panels[0].Icon = icon;
			}
			catch 
			{
				//do nothing , just don't display the icon
			}


			statusbar.Panels[1].Text = "SEVCON: Not logged in";
			try
			{
				Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\accessGrey.ico");
				statusbar.Panels[1].Icon = icon;
			}
			catch 
			{
				//do nothing , just don't display the icon
			}

			underTest = 0;
			if(this.comboBox1.SelectedIndex == 8)
			{
				statusbar.Panels[2].Text = "Attempting to detect baud rate by listening to CANbus";
				#region listen in to bus traffic thread
				tListenIn = new Thread( new ThreadStart( listenForBaudRate ) );				
				tListenIn.Name = "ListenIn";
				tListenIn.Priority = ThreadPriority.Normal;
				tListenIn.IsBackground = true;
#if DEBUG
				System.Console.Out.WriteLine("Thread: " + tListenIn.Name + " started");
#endif
				tListenIn.Start();
				timer1.Enabled = true;
				#endregion listen in to bus traffic thread
			}
			else
			{
				statusbar.Panels[2].Text = "Searching for connected nodes at " + this.comboBox1.SelectedItem.ToString();
				#region interrogate bus at user selected baud rate
				userBaud = (BaudRate) comboBox1.SelectedIndex;
				this.localSystemInfo.userBaudRate = userBaud;
				tUserBaud = new Thread( new ThreadStart( listenForUserSelectedBaudRate ) );				
				tUserBaud.Name = "UserBaud";
				tUserBaud.Priority = ThreadPriority.Normal;
				tUserBaud.IsBackground = true;
#if DEBUG
				System.Console.Out.WriteLine("Thread: " + tUserBaud.Name + " started");
#endif
				tUserBaud.Start();
				timer1.Enabled = true;			
				#endregion interrogate bus at user selected baud rate
			}
		}

		private void fillDataTable()
		{
			DataRow row;
			statusbar.Panels[2].Text = "Filling table";
			for (int i = 0; i< ( localSystemInfo.noOfNodes ); i++)  //have to use field not property here 
			{
				row = this.table.NewRow(); //create new row
				row[(int) (CANCommsCol.Manuf)]  = localSystemInfo.nodes[i].manufacturer.ToString();
				row[(int) (CANCommsCol.Type)]   = localSystemInfo.nodes[ i ].deviceType;
				row[(int) (CANCommsCol.NodeID)] = localSystemInfo.nodes[ i ].nodeID;
				this.table.Rows.Add(row);  //add row to table
			}
			statusbar.Panels[2].Text = "Displaying connected node information";
			fillToolbarNodeData();
			if(this.localSystemInfo.noOfSevconApplicationNodes>0)
			{
				showUserControls("LOGIN");
			}
		}
		private void comboBox1_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			this.AcceptButton = this.testBtn;
		}

		#endregion

		#region login methods
		private void loginBtn_Click(object sender, System.EventArgs e)
		{
			string abortMessage = "";
			bool cancel = false;

			// (AJK) Check the user has entered a password before trying to log in
			try
			{
				validatingPasswordCode();	
			}
			catch(Exception ex)
			{
				// Cancel the event and select the text to be corrected by the user.
				cancel = true;
				textBox1.Select(0, textBox1.Text.Length);
				// Set the ErrorProvider error with the text to display. 
				this.errorProvider1.SetError(textBox1, ex.Message);
				this.textBox1.Focus();
			}
			finally
			{
				this.AcceptButton = this.loginBtn;
			}

			if ( cancel == false )
			{
				hideUserControls();
				statusbar.Panels[2].Text = "Waiting for response to access request";
				if (this.selectednodeID == 201)  //system login
				{
					#region login to system
					feedback = this.localSystemInfo.loginToSystem(requestUserIDNo, passwordNumericValue, out abortMessage);
					if ( feedback != DIFeedbackCode.DISuccess )
					{
						Message.Show("Login failed. \nError code: " + feedback.ToString() + " " + abortMessage );
					}
					#endregion login to system
				}
				else 
				{
					#region login to single node
					feedback = this.localSystemInfo.loginToNode(this.selectednodeID, requestUserIDNo, passwordNumericValue, out abortMessage );
					if ( feedback != DIFeedbackCode.DISuccess )
					{
						Message.Show("Login failed. \nError code: " + feedback.ToString() + " " + abortMessage);
					}
					#endregion login to single node
				}
				updateToolbarTooltipText();
				if(feedback == DIFeedbackCode.DISuccess)
				{
					if(this.localSystemInfo.systemAccess ==0)
					{
						#region update user feedback
						statusbar.Panels[1].Text = "SEVCON: Not logged in";
						try
						{
							Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\accessGrey.ico");
							statusbar.Panels[1].Icon = icon;
						}
						catch 
						{
							//do nothing , just don't display the icon
						}
						statusbar.Panels[2].Text = "Login Failed";
						label1.Text = "Login to access SEVCON nodes";
						#endregion update user feedback
					}
					else
					{
						#region update user feedback
						statusbar.Panels[1].Text = MAIN_WINDOW.UserIDs[this.localSystemInfo.systemAccess].ToString();
						try
						{
							Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\resources\access.ico");
							statusbar.Panels[1].Icon = icon;
						}
						catch 
						{
							//do nothing , just don't display the icon
						}
						#endregion update user feedback
					}
					#region update the toolbar
					if(this.localSystemInfo.systemAccess>= SCCorpStyle.AccLevel_PreOp)
					{
						toolbar.Buttons[0].Enabled = true;
						toolbar.Buttons[1].Enabled = true;
						toolbar.Buttons[0].ToolTipText = "Request System Pre-Operational state";
						toolbar.Buttons[1].ToolTipText = "Request System Operational State";
					}
					else
					{
						toolbar.Buttons[0].Enabled = false;
						toolbar.Buttons[1].Enabled = false;
						toolbar.Buttons[0].ToolTipText = "Insufficient acccess";
						toolbar.Buttons[1].ToolTipText = "Insufficient acccess";
					}
					#endregion update the toolbar
					if(ODsRead == false)
					{
						#region get data from first node with EDS
						//first skip over any nodes wit hno EDS - cannot read their OD
						nodeIndex = 0; // B&B
						while( ( this.localSystemInfo.nodes[nodeIndex].manufacturer == Manufacturer.UNKNOWN) 
							&& nodeIndex<this.localSystemInfo.noOfNodes)
						{
							nodeIndex++;
						}
						if(nodeIndex<this.localSystemInfo.noOfNodes)
						{
							#region update user feedback
							this.localSystemInfo.nodes[nodeIndex].itemBeingRead = 0;
							this.progressBar1.Value = this.progressBar1.Minimum;
							this.progressBar1.Maximum = this.localSystemInfo.nodes[nodeIndex].noOfItemsInOD;
							statusbar.Panels[2].Text = "Retrieving data from Node " + this.localSystemInfo.nodes[nodeIndex].nodeID.ToString();
							#endregion update user feedback
							#region start data retrieval thread
							nodeDataRetrieval = new Thread( new ThreadStart( getNodeData ));				
							nodeDataRetrieval.Name = "Getting dataf form nodes";
							nodeDataRetrieval.Priority = ThreadPriority.Normal;
							nodeDataRetrieval.IsBackground = true;
#if DEBUG
							System.Console.Out.WriteLine("Thread: " + nodeDataRetrieval.Name + " started");
#endif
							this.timer1.Enabled = true;
							nodeDataRetrieval.Start();
							#endregion start data retrieval thread
						}
						#endregion get data from first node with EDS	
					}
					else
					{
						this.Close();
					}
					return;
				}
			}
			showUserControls("LOGIN");
			this.AcceptButton = this.loginBtn;
		}
	
		private void comboBox2_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				validatingUserIDCode();
			}

			catch
			{
				// Cancel the event and select the text to be corrected by the user.
				e.Cancel = true;
				// Set the ErrorProvider error with the text to display. 
				errorProvider2.SetError(comboBox2, "Select a User ID");
			}
		}

		private void textBox1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.AcceptButton = this.loginBtn;
			try
			{
				validatingPasswordCode();
			}

			catch(Exception ex)
			{
				// Cancel the event and select the text to be corrected by the user.
				e.Cancel = true;
				textBox1.Select(0, textBox1.Text.Length);
				// Set the ErrorProvider error with the text to display. 
				this.errorProvider1.SetError(textBox1, ex.Message);
			}

		}
		private void validatingPasswordCode()
		{
			if (textBox1.Text.Length == 0) //nothing enetered by user
			{
				label1.Text = "Password Required";
				throw new Exception("Password Required");
			}
			else // confirm that all characters are numeric
			{
				textBox1.Text.ToCharArray();
				for(int i = 0;i<textBox1.Text.Length;i++)
				{
					if((textBox1.Text[i]<'0') || (textBox1.Text[i]>'9'))	
					{
						label1.Text = "Enter a numeric password";
						statusbar.Panels[2].Text = "Non-numeric password rejected";
						throw new Exception("Invalid character");
					}
				}
				passwordNumericValue = System.UInt32.Parse(textBox1.Text);
				if ((passwordNumericValue<1) || (passwordNumericValue>65535))
				{
					label1.Text = "Enter a numeric password between 1 and 65535";	
					statusbar.Panels[2].Text = "Out of range password rejected";
					throw new Exception("Password value out of range");
				}
			}
			//If we get this far the password must be valid
			label1.Text = "";
			statusbar.Panels[2].Text = "";
			errorProvider1.SetError(textBox1, "");
		}
		private void validatingUserIDCode()
		{
			//check that entered ID matches a predefined one
			bool validUser = false;
			for(uint i=0;i<(LoginIDs.Length);i++)
			{
				if(comboBox2.Text == LoginIDs[i])
				{
					validUser = true;
					requestUserIDNo = i + 1;  //since zero corresponds to No access
				}
			}
			if(validUser == false)
			{
				label1.Text = "Select or enter a valid User ID";
				requestUserIDNo = 0;
				throw new Exception("Select or enter a valid User ID");
			}
			//if we get this far then a valid User ID has been selected
			label1.Text = "";
			errorProvider2.SetError(comboBox2, "");
		}

		#endregion login methods

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

		private void ESTABLISH_CAN_COMMS_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			statusbar.Panels[2].Text = "Performing finalisation, please wait";
			this.textBox1.CausesValidation = false;
			this.comboBox2.CausesValidation = false;
			#region disable timers
			this.timer1.Enabled = false;
			this.timer2.Enabled = false;
			#endregion disable timers
			#region stop threads
			#region listen In thread
			if(tListenIn != null)
			{
				if((tListenIn.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
				{
					tListenIn.Abort();

					if(tListenIn.IsAlive == true)
					{
#if DEBUG
						Message.Show("Failed to close Thread: " + tListenIn.Name + " on exit");
#endif	
						tListenIn = null;
					}
				}
			}
			#endregion listen In thread
			#region Find Nodes thread
			if(tFindNodes != null)
			{
				if((tFindNodes.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
				{
					tFindNodes.Abort();
					if(tFindNodes.IsAlive == true)
					{
#if DEBUG
						Message.Show("Failed to close Thread: " + tFindNodes.Name + " on exit");
#endif	
						tFindNodes = null;
					}
				}
			}
			#endregion Find Nodes thread
			#region User Baud thread
			if(tUserBaud != null)
			{
				if((tUserBaud.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
				{
					tUserBaud.Abort();
					if(tUserBaud.IsAlive == true)
					{
#if DEBUG
						Message.Show("Failed to close Thread: " + tUserBaud.Name + " on exit");
#endif	
						tUserBaud = null;
					}
				}
			}
			#endregion User Baud thread
			#endregion stop threads
			e.Cancel = false;  
		}
		private void ESTABLISH_CAN_COMMS_WINDOW_Closed(object sender, System.EventArgs e)
		{
			#region reset window title and status bar
			statusbar.Panels[2].Text = "";
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

		#region minor methods
		private void hideUserControls()
		{
			this.label1.Text = "Please wait";  //blank user instructions whilst DW is searching
			this.testBtn.Visible = false;
			this.comboBox1.Visible = false;
			this.label2.Visible = false;

			this.comboBox2.Visible = false;
			this.textBox1.Visible = false;
			this.label3.Visible = false;
			this.label4.Visible = false;
			this.loginBtn.Visible = false;
			this.dataGrid1.CaptionText = "";
			progressBar1.Value = this.progressBar1.Minimum;
		}
		private void showUserControls(string activityType)
		{
			progressBar1.Value = this.progressBar1.Minimum;  //reset bar	
			switch (activityType)
			{
				case "LOGIN":
					this.dataGrid1.CaptionText = "Nodes connected to the CANopen bus";
					label1.Text = "Login to access SEVCON nodes";
					this.textBox1.Text = ""; //clear the password field
					this.comboBox2.Visible = true;
					this.textBox1.Visible = true;
					this.label2.Visible = true;
					this.label3.Visible = true;
					this.label4.Visible = true;
					this.loginBtn.Visible = true;
					this.comboBox2.SelectedIndex = 0;  //back to user
					this.testBtn.Visible = true;
					this.comboBox1.Visible = true;
					this.comboBox1.SelectedIndex = 8;
					break;

				case "BUSTEST":
					this.dataGrid1.CaptionText = "Nodes connected to the CANopen bus";
					this.testBtn.Visible = true;
					this.comboBox1.Visible = true;
					this.comboBox1.SelectedIndex = 8;
					this.label2.Visible = true;
					if(this.localSystemInfo.noOfSevconApplicationNodes==0)
					{
						this.AcceptButton = this.testBtn;
					}
					else
					{
						this.AcceptButton = this.loginBtn;
					}
					break;

				case "SINGLENODE":
					statusbar.Panels[2].Text = "Attempting login to " + selectedNodeText;
					this.AcceptButton = this.loginBtn;
					label1.Text = "Login to access " + selectedNodeText;
					this.textBox1.Text = ""; //clear the password field
					this.comboBox2.Visible = true;
					this.textBox1.Visible = true;
					this.label2.Visible = true;
					this.label3.Visible = true;
					this.label4.Visible = true;
					this.loginBtn.Visible = true;
					this.comboBox2.SelectedIndex = 0;  //back to user
					break;

				default:
					Message.Show("Unrecognised test type. Please report");
					this.Close();
					break;
			}
		}
		private void clearToolbarNodeData()
		{
			toolbar.Buttons[0].Enabled = false;
			toolbar.Buttons[1].Enabled = false;
			toolbar.Buttons[0].ToolTipText = "Insufficient acccess";
			toolbar.Buttons[1].ToolTipText = "Insufficient acccess";
			for(int i = 2;i<10;i++)  //include the op & pre-op buttons here
			{
				toolbar.Buttons[i].Visible = false;
				toolbar.Buttons[i].ToolTipText = "";
			}
		}
		private void fillToolbarNodeData()
		{
			toolbar.Buttons[0].Enabled = false; //no access restrictions for third parties
			toolbar.Buttons[1].Enabled = false;
			toolbar.Buttons[0].ToolTipText = "Insufficient acccess";
			toolbar.Buttons[1].ToolTipText = "Insufficient acccess";
			for(int i = 2;i<this.localSystemInfo.noOfNodes+2;i++)
			{
				if(this.localSystemInfo.nodes[i-2].manufacturer == Manufacturer.THIRD_PARTY)
				{
					toolbar.Buttons[0].Enabled = true; //no access restrictions for third parties
					toolbar.Buttons[1].Enabled = true;
					toolbar.Buttons[0].ToolTipText = "Request System Pre-Operational state";
					toolbar.Buttons[1].ToolTipText = "Request System Operational State";
				}
				toolbar.Buttons[i].Visible = true;
				updateToolbarTooltipText();
			}
		}
		private void applyTableStyle()
		{
			float [] percents = {0.3F, 0.4F, 0.3F};
			int [] colWidths  = new int[percents.Length];
			colWidths  = SCCorpStyle.calculateColumnWidths(dataGrid1.ClientRectangle.Width, percents, 2);
			CANCommsTableStyle tablestyle = new CANCommsTableStyle(colWidths);
			this.dataGrid1.TableStyles.Add(tablestyle);//finally attahced the TableStyles to the datagrid
		}
		private void dataGrid1_Resize(object sender, System.EventArgs e)
		{
			applyTableStyle();
		}
		private void updateToolbarTooltipText()
		{
			#region update login data on toolbar tool tips
			for (int i = 2;i<this.localSystemInfo.noOfNodes+2;i++)
			{
				toolbar.Buttons[i].ToolTipText 
					= this.localSystemInfo.nodes[i-2].manufacturer.ToString()//manufacturer
					+ ", " + localSystemInfo.nodes[i-2].deviceType 
					+ ", Node "  + this.localSystemInfo.nodes[i-2].nodeID;	//Node ID
				//if((localSystemInfo.nodes[i-2].manufacturer == Manufacturer.SEVCON) 
				//	&& ((localSystemInfo.nodes[i-2].deviceVersion.productVariant >0x00
				//	&& localSystemInfo.nodes[i-2].deviceVersion.productVariant <0xF0)))
				if(localSystemInfo.nodes[i-2].isSevconApplication()==true)		// AJK,31/05/05
				{
					if(localSystemInfo.nodes[i-2].accessLevel == 0)
					{
						toolbar.Buttons[i].ToolTipText  +=  ", not logged in";
					}
					else if ( (localSystemInfo.nodes[i-2].accessLevel-1) < LoginIDs.Length)
					{
						toolbar.Buttons[i].ToolTipText  += ", " + LoginIDs[(localSystemInfo.nodes[i-2].accessLevel)-1];
					}
				}
			}
			#endregion update login data on toolbar tool tips
		}
		#endregion minor methods

		#region auto validate
		private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if( (this.localSystemInfo.autoTest != null) && ( AutoValidate.staticValidationRunning == true ) )
			{
				timer2Elapsed();
			}
			else
			{
				this.timer2.Enabled = false;
			}
		}

		[Conditional ("AUTOVALIDATE")]
		private void timer2Elapsed()
		{
			if ( this.localSystemInfo.autoTest.validationRunning == true )
			{
				this.localSystemInfo.autoTest.testFeedback = feedback;

				switch ( this.localSystemInfo.autoTest.testState )
				{
					case ValidateState.ESTABLISH_CAN_COMMS_WINDOW:
					{	
						Debug.WriteLine( "" );
						Debug.WriteLine ( "TEST (1a) AUTO DETECT SYSTEM" );
						Debug.Write( "Select auto detect.," );
						this.localSystemInfo.autoTest.testFeedback = DIFeedbackCode.DICodeUnset;
						this.testBtn.Visible = true;
						this.testBtn.Focus();
						this.testBtn.PerformClick();
						this.localSystemInfo.autoTest.testState = ValidateState.ESTABLISHED_COMMS_BAUD;
						break;
					}

					case ValidateState.ESTABLISHED_COMMS_BAUD:
					{
						if ( this.localSystemInfo.autoTest.testFeedback != DIFeedbackCode.DICodeUnset )
						{
							if ( this.localSystemInfo.autoTest.testFeedback == DIFeedbackCode.DISuccess )
							{
								this.localSystemInfo.autoTest.testState = ValidateState.ESTABLISHED_COMMS;
								Debug.WriteLine( "Baud rate found.,OK" );
								feedback = DIFeedbackCode.DICodeUnset;
							}
							else
							{
								if ( this.localSystemInfo.autoTest.testFeedback == DIFeedbackCode.DIFailedToDetectBaudRate )
								{
									Debug.WriteLine( "Baud rate not found.,FAIL" );
									Debug.WriteLine( "" );
									Debug.WriteLine( "TEST (1b) MANUALLY DETECT SYSTEM ON 1M BAUD" );
									Debug.Write( "Set to 1M.," );
									this.comboBox1.Visible = true;
									this.comboBox1.SelectedIndex = 0;
									this.localSystemInfo.autoTest.testState = ValidateState.SET_COMMS_1M;
									feedback = DIFeedbackCode.DICodeUnset;
									
									// Select test button click.
									this.testBtn.Visible = true;
									this.testBtn.Focus();
									this.testBtn.PerformClick();
								}
								else
								{
									this.timer2.Enabled = false;
									Debug.WriteLine( "Error: failed to connect.  Reason : " + feedback.ToString() );
									Debug.WriteLine( "Close establish comms window." );
									this.closeBtn.Visible = true;
									this.closeBtn.Focus();
									this.closeBtn.PerformClick();
								}
							}
						}
						break;
					}
					case ValidateState.SET_COMMS_1M:
					{
						if ( this.localSystemInfo.autoTest.testFeedback != DIFeedbackCode.DICodeUnset )
						{
							Debug.Write( "Simulate find nodes at 1M baud.," );
							this.localSystemInfo.autoTest.testState = ValidateState.ESTABLISHED_COMMS;
						}
						break;
					}

					case ValidateState.ESTABLISHED_COMMS:
					{
						if ( this.localSystemInfo.autoTest.testFeedback != DIFeedbackCode.DICodeUnset )
						{
							if ( this.localSystemInfo.autoTest.testFeedback != DIFeedbackCode.DISuccess )
							{
								Debug.WriteLine( "Error: failed to connect.  Reason : " + feedback.ToString() );							
							}
							else
							{
								Debug.WriteLine( "OK,Baud," + this.localSystemInfo.baudRate.ToString() );
								Debug.WriteLine( ",No. of nodes," + this.localSystemInfo.noOfNodes.ToString() );
								Debug.Write( ",Node IDs found," );

								for ( int i = 0; i < this.localSystemInfo.noOfNodes; i++ )
								{
									Debug.Write( this.localSystemInfo.nodes[i].nodeID.ToString() + "," );
								}

								this.localSystemInfo.autoTest.testState = ValidateState.FOUND_SYSTEM;
								Debug.WriteLine( "" );
							}

							Debug.WriteLine( "" );
							Debug.WriteLine( "TEST (1c) LOG ON AT LEVEL 5" );
							Debug.WriteLine( "Enter password level 5.,OK" );
							this.comboBox2.Visible = true;
							this.comboBox2.SelectedIndex = 0;
							this.textBox1.Visible = true;
							this.textBox1.Focus();
							this.textBox1.Text = "5";
							Debug.WriteLine( "Validate entered password.,OK" );
							validatingPasswordCode();
							Debug.Write( "Login submit." );
							this.loginBtn.Visible = true;
							this.loginBtn.Focus();
							this.loginBtn.PerformClick();
							this.localSystemInfo.autoTest.testState = ValidateState.ATTEMPT_LOGIN;
							this.localSystemInfo.autoTest.testFeedback = DIFeedbackCode.DICodeUnset;
						}
						break;
					}
					case ValidateState.ATTEMPT_LOGIN:
					{
						if ( this.localSystemInfo.autoTest.testFeedback != DIFeedbackCode.DICodeUnset )
						{
							if ( this.localSystemInfo.autoTest.testFeedback != DIFeedbackCode.DISuccess )
							{
								this.localSystemInfo.autoTest.testState = ValidateState.FAILED_TO_LOGIN;
								Debug.WriteLine( "FAIL," + feedback.ToString() );
							}
							else
							{
								this.localSystemInfo.autoTest.testState = ValidateState.LOGGED_IN;
								Debug.WriteLine( "OK,level 5" );
							}

							Debug.WriteLine( "Close establish comms window." );
							this.localSystemInfo.autoTest.testFeedback = DIFeedbackCode.DICodeUnset;
							this.closeBtn.Visible = true;
							this.closeBtn.Focus();
							this.closeBtn.PerformClick();
							Debug.WriteLine( "" );
							this.timer2.Enabled = false;
						}
						break;
					}
				}
			}		
			else
			{
				this.timer2.Enabled = false;
			}
		}
		#endregion
	}
	#endregion Establish CAN Comms Form Class

}
