/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.52$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/09/2008 23:16:42$
	$ModDate:22/09/2008 23:02:58$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
    Windows Form class.  
    This form contains all the user controls to search for units connected to the CANopen bus. 
    Users can either listen in to the bus or can select to send out an are you there message at 
    any of the permitted CANopen frequencies. Where a node is found its Manufacturer type 
    is established by comparing it to the EDS files in the DriveWizard EDS directory.
    Node ar eone of SEVCON, THIRD PARTY (has EDS but is not SEVCON, or UNKNOWN (No EDS) SEVCON 
    nodes may be further identifed with a device type  e.g. PUMP, BOOTLOADER etc.
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  36711: CAN_BUS_CONFIGURATION.cs 

   Rev 1.52    23/09/2008 23:16:42  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.51    12/03/2008 13:43:46  ak
 All DI threads have ThreadPriority increased to Normal from BelowNormal
 (needed when running VCI3)


   Rev 1.50    21/01/2008 12:03:02  jw
 File merge for VCI3/ Vista. These changes are those to go in all builds


   Rev 1.49    05/12/2007 22:13:20  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.Threading;

namespace DriveWizard
{

	#region enumerated types
	public enum nodes {Node1 , Node2, Node3, Node4, Node5, Node6, Node7, Node8};  //up to SCCorpStyle.maxConnectedDevices nodes can be connected to the bus
	#endregion  enumerated types
	/// <summary>
	/// Summary description for CAN_BUS_CONFIGURATION.
	/// </summary>
	public class CAN_BUS_CONFIGURATION : System.Windows.Forms.Form
	{
		#region form controls definition
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboBox2;
		#endregion
		private System.ComponentModel.IContainer components;
		
		#region my definitions
		private SystemInfo sysInfo;
		DIFeedbackCode feedback;
		ConfigBusTable nodesTable;
		AllPossibleNodeIDsTable nodesList;
		DataView nodesView;
		ConfigBusTableStyle tablestyle = null;
		string [] baudRates = {"1M", "800k", "500k", "250k", "125k", "50k", "20k", "10k"}; 
		string [] baudRates_SEVCON = {"1M", "500k", "250k", "125k", "50k", "20k", "10k" , "100k"};
		private System.Windows.Forms.Button submitBtn;
		float [] nodePercents = {0.125F, 0.125F, 0.125F, 0.125F, 0.125F, 0.125F, 0.125F, 0.125F};
		int numberOfSevconAppNodes = 0;
		bool reconnectionRequired = false;
		BaudRate userSelectedBaud = BaudRate._1M;
		ushort nodeID = 0;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.StatusBar statusBar1;
		Thread LSSThread = null;
		int [] SEVCONNodeIDS = null;
		int dataGrid1DefaultHeight = 0;
		//class wide becuase they are retrieved on a background thread
		ushort oldNodeID = 0;
		BaudRate oldBaud = BaudRate._unknown;
		#endregion

		#region intialisation
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: CAN_BUS_CONFIGURATION
		///		 *  Description     : Constructor
		///		 *  Parameters      : none
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		public CAN_BUS_CONFIGURATION(SystemInfo passed_systemInfo)
		{
			InitializeComponent();
			sysInfo = passed_systemInfo; //local reference 
			SEVCONNodeIDS = new int[SCCorpStyle.maxConnectedDevices];
		}
		private void CAN_BUS_CONFIGURATION_Load(object sender, System.EventArgs e)
		{
			createAllNodesList();
			for(int i = 0;i<this.sysInfo.nodes.Length;i++)
			{
				if ( sysInfo.nodes[i].isSevconApplication() == true )
				{
					SEVCONNodeIDS[i] = sysInfo.nodes[i].nodeID;
					this.numberOfSevconAppNodes++;
				}
			}
			if(this.numberOfSevconAppNodes==0)  //has to be application 
				//bootloader and self char do not have the OD entries for baud rate and Node ID
			{
				this.Text = "DriveWizard: Single Node baud rate and CAN Node ID configuration";
				this.comboBox2.Visible = true;
				this.comboBox2.SelectedIndex = 0;
				this.dataGrid1.Visible = false;
				this.label1.Text = "Ensure that only one node is connected to the CANopen bus.\nSelect a unique CAN Node ID and Submit";
				#region handle no sevcon units present
				comboBox1.DataSource = baudRates; //start off assuming that we have no SEVCON nodes
                //Jude File merge for VCI3 - single systme wide baud rate
				switch (this.sysInfo.CANcomms.systemBaud)
				{ //string [] baudRates = {"1M", "800k", "500k", "250k", "125k", "50k", "20k", "10k"}; 
					case BaudRate._1M:
						this.comboBox1.SelectedIndex = 0;
						break;
					case BaudRate._800K:
						this.comboBox1.SelectedIndex = 1;
						break;
					case BaudRate._500K:
						this.comboBox1.SelectedIndex = 2;
						break;
					case BaudRate._250K:
						this.comboBox1.SelectedIndex = 3;
						break;
					case BaudRate._125K:
						this.comboBox1.SelectedIndex = 4;
						break;
					case BaudRate._50K:
						this.comboBox1.SelectedIndex = 5;
						break;
					case BaudRate._20K:
						this.comboBox1.SelectedIndex = 6;
						break;
					case BaudRate._10K:
						this.comboBox1.SelectedIndex = 7;
						break;
					default:
						this.comboBox1.SelectedIndex = 0;
						break;
				}
				#endregion handle no sevcon units present
			}
			else
			{
				this.comboBox2.Visible = false;
				comboBox1.DataSource  = this.baudRates_SEVCON;
				this.Text = "DriveWizard: SEVCON nodes, baud rate and CAN Node ID Configuration";
				this.sysInfo.formatDataGrid(this.dataGrid1);
				createNodeTable();
				fillNodeTable();
				createCANConfigTableStyle();
				applyCANConfigTableStyle();
				this.dataGrid1.Visible = true;
				#region get the current buadRate
                //Jude VCI3 file merge - single system wide baud
				switch (this.sysInfo.CANcomms.systemBaud)
				{ //baudRates_SEVCON = {"1M", "500k", "250k", "125k", "50k", "20k", "10k"};
					case BaudRate._1M:
						this.comboBox1.SelectedIndex = 0;
						break;
					case BaudRate._500K:
						this.comboBox1.SelectedIndex = 1;
						break;
					case BaudRate._250K:
						this.comboBox1.SelectedIndex = 2;
						break;
					case BaudRate._125K:
						this.comboBox1.SelectedIndex = 3;
						break;
					case BaudRate._50K:
						this.comboBox1.SelectedIndex = 4;
						break;
					case BaudRate._20K:
						this.comboBox1.SelectedIndex = 5;
						break;
					case BaudRate._10K:
						this.comboBox1.SelectedIndex = 6;
						break;
					default:
						this.comboBox1.SelectedIndex = 0;
						break;
				}
				#endregion get the current buadRate
				this.nodesTable.ColumnChanged +=new DataColumnChangeEventHandler(nodesTable_ColumnChanged);
			}
			foreach (Control myControl in this.Controls)
			{
				#region button colouring
				if((myControl.GetType().ToString()) == "System.Windows.Forms.Button")
				{
					myControl.BackColor =  SCCorpStyle.buttonBackGround;
				}
				#endregion button colouring
			}
			dataGrid1DefaultHeight = this.dataGrid1.Height;

		}
		private void createAllNodesList()
		{
			DataRow row;
			nodesList = new AllPossibleNodeIDsTable();

			#region add not connected value
			row = nodesList.NewRow();
			row["NodeIDName"] = "not used";
			row["NodeID"] = 0;
			nodesList.Rows.Add(row);
			#endregion add not connected value
			for (int i = 1;i<128;i++)
			{
				row = nodesList.NewRow();
				row["NodeIDName"] = i.ToString();
				row["NodeID"] = i;
				nodesList.Rows.Add(row);
				this.comboBox2.Items.Add(i.ToString());
			}

		}
		private void createNodeTable()
		{
			nodesTable = new ConfigBusTable();
			nodesView = new DataView(nodesTable);
			nodesView.AllowNew = false;
			nodesView.AllowDelete = false;
			this.dataGrid1.DataSource = nodesView;
		}
		private void fillNodeTable()
		{
			DataRow row = nodesTable.NewRow();
			for (int i = 0;i<this.sysInfo.nodes.Length;i++)
			{
				row[i] = this.sysInfo.nodes[i].nodeID;
			}
			nodesTable.Rows.Add(row);
			this.nodesTable.AcceptChanges();
		}
		private void createCANConfigTableStyle()
		{
			int [] colWidths  = new int[nodePercents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, nodePercents, 0, dataGrid1DefaultHeight);
			tablestyle = new ConfigBusTableStyle(colWidths, nodesList, SEVCONNodeIDS);
		}
		private void applyCANConfigTableStyle()
		{
			int [] colWidths  = new int[nodePercents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, nodePercents,0, dataGrid1DefaultHeight);
			for (int i = 0;i<nodePercents.Length;i++)
			{
				tablestyle.GridColumnStyles[i].Width = colWidths[i];
			}	
			this.dataGrid1.TableStyles.Add(tablestyle);//finally attahced the TableStyles to the datagrid
		}
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.dataGrid1 = new System.Windows.Forms.DataGrid();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.submitBtn = new System.Windows.Forms.Button();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(496, 208);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(120, 25);
			this.button1.TabIndex = 0;
			this.button1.Text = "&Close window ";
			this.button1.Click += new System.EventHandler(this.closeBtn_Click);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(616, 56);
			this.label1.TabIndex = 1;
			this.label1.Text = "Select required CAN Node IDs and system Baud Rate";
			// 
			// dataGrid1
			// 
			this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGrid1.DataMember = "";
			this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid1.Location = new System.Drawing.Point(8, 80);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.ReadOnly = true;
			this.dataGrid1.Size = new System.Drawing.Size(621, 80);
			this.dataGrid1.TabIndex = 2;
			this.dataGrid1.Visible = false;
			this.dataGrid1.Resize += new System.EventHandler(this.dataGrid1_Resize);
			// 
			// comboBox1
			// 
			this.comboBox1.Location = new System.Drawing.Point(192, 168);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(288, 24);
			this.comboBox1.TabIndex = 3;
			this.comboBox1.Text = "comboBox1";
			this.comboBox1.SelectionChangeCommitted += new System.EventHandler(this.comboBox1_SelectionChangeCommitted);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 168);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(184, 32);
			this.label2.TabIndex = 4;
			this.label2.Text = "Select required baud rate";
			// 
			// submitBtn
			// 
			this.submitBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.submitBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.submitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.submitBtn.Location = new System.Drawing.Point(8, 208);
			this.submitBtn.Name = "submitBtn";
			this.submitBtn.Size = new System.Drawing.Size(120, 25);
			this.submitBtn.TabIndex = 5;
			this.submitBtn.Text = "&Submit";
			this.submitBtn.Click += new System.EventHandler(this.submitBtn_Click);
			// 
			// comboBox2
			// 
			this.comboBox2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.comboBox2.ItemHeight = 16;
			this.comboBox2.Location = new System.Drawing.Point(8, 88);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(256, 24);
			this.comboBox2.TabIndex = 7;
			this.comboBox2.Text = "comboBox2";
			this.comboBox2.Visible = false;
			// 
			// timer1
			// 
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 258);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(634, 22);
			this.statusBar1.TabIndex = 8;
			// 
			// CAN_BUS_CONFIGURATION
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(634, 280);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.comboBox2);
			this.Controls.Add(this.submitBtn);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.dataGrid1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button1);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "CAN_BUS_CONFIGURATION";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "CAN_BUS_CONFIGURATION";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.CAN_BUS_CONFIGURATION_Closing);
			this.Load += new System.EventHandler(this.CAN_BUS_CONFIGURATION_Load);
			this.Closed += new System.EventHandler(this.CAN_BUS_CONFIGURATION_Closed);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region user interaction zone
		private void submitBtn_Click(object sender, System.EventArgs e)
		{
			hideUserControls();
			userSelectedBaud = BaudRate._1M;
			if(this.comboBox1.SelectedIndex != -1)
			{
				string baudString = "_" +  this.comboBox1.SelectedItem.ToString().ToUpper();
				try
				{
					userSelectedBaud = (BaudRate)Enum.Parse(typeof(BaudRate),baudString);
				}
				catch
				{
					userSelectedBaud = BaudRate._1M;
				}
			}
			if(this.numberOfSevconAppNodes>0)
			{
				#region some nodes present
				if(this.nodesTable.Rows[0].RowState == DataRowState.Modified)
				{
					reconnectionRequired = true;
					#region set Node IDs
					ushort [] nodeIDs = new ushort[this.sysInfo.nodes.Length];
					for(int i = 0;i<this.sysInfo.nodes.Length;i++)
					{
						nodeIDs[i] = System.Convert.ToUInt16(this.nodesTable.Rows[0][i]);
					}
					feedback = this.sysInfo.setSystemNodes(nodeIDs);
					if(feedback == DIFeedbackCode.DISuccess)
					{
						SystemInfo.errorSB.Append("\nNode ID change request accepted by SEVCON nodes. ");
					}
					#endregion set Node IDs
				}
				#region SEVCON nodes
						for(int i = 0;i<this.sysInfo.nodes.Length;i++)
						{
							if(System.Convert.ToUInt16(this.nodesTable.Rows[0][i]) != 0)
							{
								feedback = this.sysInfo.setSystemBaudRate(userSelectedBaud);
								if(feedback == DIFeedbackCode.DISuccess)
								{
									SystemInfo.errorSB.Append("\nBaud rate changed to " + userSelectedBaud.ToString().Substring(1));
									SystemInfo.errorSB.Append("\nCycle Power at keyswitch BEFORE reconnecting");
									this.sysInfo.displayErrorFeedbackToUser("Success");
									this.reconnectionRequired = true;
									this.Close();
									return;
								}
							}
						}
						#endregion SEVCON nodes
				#endregion some nodes present
				showUserControls();
				if(SystemInfo.errorSB.Length>0)
				{
					SystemInfo.errorSB.Append("\nCycle Power at keyswitch BEFORE reconnecting");
					this.sysInfo.displayErrorFeedbackToUser("");
				}
				this.Close();
			}
			else
			{
				#region LSS
				reconnectionRequired = true;
				this.statusBar1.Text = "Attempting to find node.Please wait";
				//call LSS function here
				userSelectedBaud = (BaudRate)this.comboBox1.SelectedIndex;
				nodeID = System.Convert.ToUInt16(this.comboBox2.SelectedIndex+1);
				DialogResult result = Message.Show( null, 
					"Only one node must be connected to DriveWizard.\nDo you wish to continue?", 
					"LAYER SETTING SERVICE",
					System.Windows.Forms.MessageBoxButtons.YesNo,
					System.Windows.Forms.MessageBoxIcon.Question,
					System.Windows.Forms.MessageBoxDefaultButton.Button1 );
				if ( result == System.Windows.Forms.DialogResult.Yes )
				{ //the threading for this should be that we kick of the thread each time we search at a single baud rate -judetemp - furthe rchanges needed
					LSSThread = new Thread( new ThreadStart(LSSWrapper) ); 
					LSSThread.Name = "LSS hint for any connected nodes";
					LSSThread.IsBackground = true;
                    LSSThread.Priority = ThreadPriority.Normal;
#if DEBUG
					System.Console.Out.WriteLine("Thread: " + LSSThread.Name + " started");
#endif
					LSSThread.Start(); 
					timer1.Enabled = true;
				}
				#endregion LSS
			}
		}

		private void LSSWrapper()
		{
			LSS nodeLSS = new LSS();
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
            //DR38000268 get LSS working
            MAIN_WINDOW.isVirtualNodes = false;     // ensure not virtual while looking for nodes
			ushort foundNodeID = 0;                 // new var for LSS return value, prevent overwriting user req nodeID
			BaudRate oldBaudrate = BaudRate._unknown;
			#region try and find out what the node ID is for the single connected device
			for ( BaudRate b = BaudRate._1M; b <= BaudRate._10K; b++ )
			{
                fbc = nodeLSS.searchForNodeIDAtSpecifiedBaud(b, out foundNodeID, this.sysInfo.CANcomms);
				if(fbc != DIFeedbackCode.DISuccess)
				{
					System.Windows.Forms.DialogResult result = Message.Show( null, "Please cycle the power on the device then select Yes to continue or select No to abort.", "FAILED TO FIND LSS NODE AT " + b.ToString(), 
						System.Windows.Forms.MessageBoxButtons.YesNo, 
						System.Windows.Forms.MessageBoxIcon.None, System.Windows.Forms.MessageBoxDefaultButton.Button1 );
					// halts code execution until the user has cycled power on device
					if ( result != System.Windows.Forms.DialogResult.Yes )
					{
						break;
					}
				}
				else
				{
					oldBaudrate = b;
                    break;      //found it!
				}
			}
			#endregion


            if (fbc == DIFeedbackCode.DISuccess)
            {
                fbc = nodeLSS.setNodeLayerSettings(nodeID, userSelectedBaud, this.sysInfo.CANcomms);
            }
		}
		private void comboBox1_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			BaudRate userSeletedBaud = (BaudRate)this.comboBox1.SelectedIndex;
		}
		#endregion

		#region minor methods
		private void hideUserControls()
		{
			if(this.numberOfSevconAppNodes>0)
			{
				this.dataGrid1.Enabled = false;
			}
			if(this.sysInfo.nodes.Length ==0)
			{
				this.comboBox2.Enabled =false;
			}

			this.submitBtn.Visible = false;
			this.comboBox1.Enabled = false;
		}
		private void showUserControls()
		{
			if(this.numberOfSevconAppNodes>0)
			{
				this.dataGrid1.Enabled = true;
			}
			if(this.sysInfo.nodes.Length ==0)
			{
				this.comboBox2.Enabled = true;
			}
			this.comboBox1.Enabled = true;
			this.submitBtn.Visible = true;
		}
		#endregion minor methods

		#region finalisation/exit
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: closeBtn_Click
		///		 *  Description     : Event handler for the button used to close this window
		///		 *  Parameters      : system generated
		///		 *  Used Variables  : none
		///		 *  Preconditions   : none - any will be dealt with in a window closing even thandler 
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void closeBtn_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		private void CAN_BUS_CONFIGURATION_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(reconnectionRequired == true)
			{
				MAIN_WINDOW.reEstablishCommsRequired = true;  //judetmep - more subtlety needed here
			}
			this.statusBar1.Text = "Performing finalisation, please wait";
			#region abort any threads here
			if(LSSThread != null)
			{
				if((LSSThread.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
				{
					LSSThread.Abort();

					if(LSSThread.IsAlive == true)
					{
#if DEBUG
						Message.Show("Failed to close Thread: " + LSSThread.Name + " on exit");
#endif
						LSSThread = null;
					}
				}
			}
			#endregion abort any threads here
			#region stop all timers
			this.timer1.Enabled = false;
			#endregion stop all timers
			e.Cancel = false; //force this window to close
		}

		private void CAN_BUS_CONFIGURATION_Closed(object sender, System.EventArgs e)
		{
			#region reset window title and status bar
			#endregion reset window title and status bar
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

		private void dataGrid1_Resize(object sender, System.EventArgs e)
		{
			if(tablestyle != null)
			{
				applyCANConfigTableStyle();
			}
		}

		private void nodesTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
		{
			string errorString = "";
			for(int i = 0; i< this.sysInfo.nodes.Length;i++)
			{
				if(e.Column.Ordinal != i)
				{
					if(System.Convert.ToUInt16(e.ProposedValue) == System.Convert.ToUInt16(this.nodesTable.Rows[0][i]))
					{
						e.Row.RejectChanges();
						e.ProposedValue = System.Convert.ToUInt16(this.nodesTable.Rows[0][i]);
						errorString = "NodeIDs must be unique";
					}
				}
			}
			if(errorString != "")
			{
				e.Row.SetColumnError(e.Column.Ordinal, errorString);
			}
			else
			{
				e.Row.ClearErrors();
			}
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if ( this.LSSThread != null )
			{
				if( ( LSSThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
					StringBuilder userFeedbackStr = new StringBuilder();
					timer1.Enabled = false;
					if(feedback != DIFeedbackCode.DISuccess)
					{
						userFeedbackStr.Append("Attempt to set node ID and baudrate failed. Feedback code:" + feedback.ToString());
					}
					else
					{
						userFeedbackStr.Append("node ID changed from ");
						userFeedbackStr.Append(this.oldNodeID.ToString());
						userFeedbackStr.Append(" to ");
						userFeedbackStr.Append(this.nodeID.ToString());
						userFeedbackStr.Append(", Baud changed from ");
						userFeedbackStr.Append(this.baudRates[(int)this.oldBaud]);
						userFeedbackStr.Append(" to ");
						userFeedbackStr.Append(this.baudRates[(int)this.userSelectedBaud]);
					}
					this.statusBar1.Text = "";
					userFeedbackStr.Append("\nCycle Power at keyswitch BEFORE reconnecting");
					this.sysInfo.displayErrorFeedbackToUser("");
					this.Close();

				}
			}
		}
	}

	#region Configure CANbus Data Table
	public class ConfigBusTable : DataTable
	{
		public ConfigBusTable()
		{
			this.Columns.Add(nodes.Node1.ToString(), typeof(System.UInt16));
			this.Columns[nodes.Node1.ToString()].DefaultValue = 0;
			this.Columns.Add(nodes.Node2.ToString(), typeof(System.UInt16));
			this.Columns[nodes.Node2.ToString()].DefaultValue = 0;
			this.Columns.Add(nodes.Node3.ToString(), typeof(System.UInt16));
			this.Columns[nodes.Node3.ToString()].DefaultValue = 0;
			this.Columns.Add(nodes.Node4.ToString(), typeof(System.UInt16));
			this.Columns[nodes.Node4.ToString()].DefaultValue = 0;
			this.Columns.Add(nodes.Node5.ToString(), typeof(System.UInt16));
			this.Columns[nodes.Node5.ToString()].DefaultValue = 0;
			this.Columns.Add(nodes.Node6.ToString(), typeof(System.UInt16));
			this.Columns[nodes.Node6.ToString()].DefaultValue = 0;
			this.Columns.Add(nodes.Node7.ToString(), typeof(System.UInt16));
			this.Columns[nodes.Node7.ToString()].DefaultValue = 0;
			this.Columns.Add(nodes.Node8.ToString(), typeof(System.UInt16));
			this.Columns[nodes.Node8.ToString()].DefaultValue = 0;
		}
	}
	#endregion  Configure CANbus Data Table

	#region AllPossibleNodeIDsTable
	public class AllPossibleNodeIDsTable : DataTable
	{
		public AllPossibleNodeIDsTable()
		{
			this.Columns.Add("NodeIDName", typeof(System.String));
			this.Columns.Add("NodeID", typeof(System.UInt16));
		}
	}

	#endregion AllPossibleNodeIDsTable

	#region Configure CANbus Table Style
	public class ConfigBusTableStyle : SCbaseTableStyle
	{
		public ConfigBusTableStyle (int [] ColWidths, AllPossibleNodeIDsTable nodesList, int [] SEVCONNodeIDS)
		{
			string [] nodesNames = {"1st node", "2nd node", "3rd Node", "4th node", "5th node", "6th node", "7th node", "8th node"};
			for(int i = 0;i<SCCorpStyle.maxConnectedDevices;i++)
			{
				if(SEVCONNodeIDS[i]  != 0)
				{
					BusConfigComboColumn c1 = new BusConfigComboColumn(nodesList, "NodeIDName", "NodeID", i);
					c1.MappingName = "Node" + (i+1).ToString();
					c1.HeaderText = nodesNames[i];
					c1.Width = ColWidths[i];
					GridColumnStyles.Add(c1);
				}
				else
				{
					SCbaseRODataGridTextBoxColumn c1 = new SCbaseRODataGridTextBoxColumn(i);
					c1.MappingName = "Node" + (i+1).ToString();
					c1.HeaderText = nodesNames[i];
					c1.Width = ColWidths[i];
					GridColumnStyles.Add(c1);

				}
			}
			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();
		}

	}

	#endregion Configure CANbus Table Style

	#region CANbus config ComboColumn CLass
	public class BusConfigComboColumn : System.Windows.Forms.DataGridTextBoxColumn 
	{
		private ComboBox _cboColumn;
		private object _objSource = null;
		private string _strMember;
		private string _strValue;
		private int _columnOrdinal;
		
		private bool _bIsComboBound = false; // remember if this combobox is bound to its datagrid
		private int _iRowNum;  //the row number
		private CurrencyManager _cmSource;

		/// <summary>
		/// initialize the combobox column and take note of the data source/member/value used to fill the combobox
		/// </summary>
		/// <param name="objSource">bind Source for the combobox (typical is a DataTable object)</param>
		/// <param name="strMember">bind for the combobox DisplayMember (typical is a Column Name within the Source)</param>
		/// <param name="strValue">bind for the combobox ValueMember (typical is a Column Name within the Source)</param>


		public BusConfigComboColumn(object objSource, string strMember, string strValue, int columnOrdinal )
		{
			_objSource = objSource;
			_strMember = strMember;
			_strValue = strValue;
			_columnOrdinal = columnOrdinal;

			// create a new combobox object
			_cboColumn = new ComboBox();
			// set the data link to the source, member and value displayed by this combobox
			_cboColumn.DataSource = _objSource;
			_cboColumn.DisplayMember = _strMember;
			_cboColumn.ValueMember = _strValue;
			_cboColumn.DropDownStyle = ComboBoxStyle.DropDownList;
			// Setting ReadOnly changes the behavior of the column so the 'leave' event fires whenever we 
			// change cell. The default behavior will not fire the 'leave' event when we up-arrow or 
			// down-arrow to the next row.
			this.ReadOnly = true;
			// we need to know when the combo box is getting closed so we can update the source data and
			// hide the combobox control
			_cboColumn.Leave += new EventHandler(cboColumn_Leave);
			_cboColumn.SelectionChangeCommitted += new EventHandler(cboColumn_ChangeCommit);

			// make sure the combobox is invisible until we've set its correct position and dimensions
			_cboColumn.Visible = false;
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			if(System.Convert.ToUInt16(this.GetColumnValueAtRow(source, rowNum)) == 0)  //non connected no user interaction
			{
				return;
			}
			#region filterout non-unique Node IDs
			//just as soon as I've worked out how to do it
			#endregion filterout non-unique Node IDs

			#region once only actions
			// the navigation path to the datagrid only exists after the column is added to the Styles
			if (_bIsComboBound == false) 
			{
				_bIsComboBound = true; //set the indicator 
				this.DataGridTableStyle.DataGrid.Controls.Add(_cboColumn); // and bind combo to its datagrid 
			}
			#endregion once only actions
			_iRowNum = rowNum;  //used in commit handler
			_cmSource = source; //used in commit handler
			_cboColumn.Font = this.TextBox.Font; //synchronise the font
			// get current cell value and use this dig out the corresponding string prior to making combo Visible
			object tempObject = this.GetColumnValueAtRow(source, rowNum);
			// set the combobox to the dimensions of the cell (do this each time because the user may have resized this column)
			_cboColumn.Bounds = bounds;
			// do not paint the control until we've set the correct position in the items list
			_cboColumn.BeginUpdate();
			// set the column as visible 
			// ahead of setting a position in the items collection. otherwise the combobox will not be
			// populated and the call to set the SelectedValue cannot succeed
			_cboColumn.Visible = true;
			// use the object to set the combobox. the null detection is primarily aimed at the addition of a
			// new row (where it is possible a default column-row content has not been defined)
			_cboColumn.SelectedValue = tempObject;
			_cboColumn.EndUpdate();  //allows painting to go ahead
			_cboColumn.Focus();
		}

		public void cboColumn_Leave(object sender, EventArgs e) 
		{
			_cboColumn.Visible = false;
		}

		// this method is called to draw the box without a highlight (ie when the cell is in unselected state)
		public void cboColumn_ChangeCommit(object sender, EventArgs e)
		{
			object objValue = _cboColumn.SelectedValue;
			this.SetColumnValueAtRow(_cmSource, _iRowNum, objValue);
			_cboColumn.Visible = false;
		}
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			object tempObject = this.GetColumnValueAtRow(source, rowNum);
			DataRow[] aRowA = ((DataTable)_objSource).Select(_strValue + " = " + tempObject.ToString());
			string defaultStr = aRowA[0][_strMember].ToString();  
			Rectangle rect = bounds;
			g.FillRectangle(backBrush, rect); 
			rect.Y += 2; // vertical offset to account for frame of combobox
			g.DrawString(defaultStr, this.TextBox.Font, foreBrush, rect); 
		}
	}
	#endregion CANbus config ComboColumn CLass
}
