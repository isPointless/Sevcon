/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.74$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:13:06$
	$ModDate:05/12/2007 22:00:32$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	

REFERENCES    

MODIFICATION HISTORY
    $Log:  36799: PERSONALITY_PARAMS_WINDOW.cs 

   Rev 1.74    05/12/2007 22:13:06  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections; 
using System.ComponentModel;
using System.Windows.Forms;
using System.Data; 
using System.IO; //for file IO
using System.Data.OleDb; //access to data source (Excel file)
using System.Configuration; //to access strings contained in configuration file
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System.Threading;


namespace DriveWizard
{
	#region enumerated types
	public enum PPCol {Index, sub, param, PDOMap, defVal, lowVal, highVal, actValue, units, accessType,accessLevel,displayType, sectionType, objectType, scaling, fullValue, fullMax, fullMin, format, numberFormat};
	#endregion enumerated types

	public struct UserSelectedODList
	{
		///<summary>CAN node ID = allows us to re-locate this item accross Drive Wizard instances</summary>
		public int		NodeID;						

		///<summary>OD index of item to be monitored</summary>
		public int		index;						
		
		///<summary>OD sub-index of item to be monitored</summary>
		public int		subIndex;
		///<summary>OD parameter name of item to be monitored</summary>
	};

	public struct nodeTag 
	{
		public int treeLevel;
		public int ODIndexNo;
		public int subIndex;
		public int nodeID;
		public int tableindex;
		public string GroupName;
		public nodeTag(int level,int ODInd, int sub, int NodeID, int tableIndex, string groupStr)
		{
			treeLevel = level;
			ODIndexNo = ODInd;
			subIndex = sub;
			nodeID = NodeID;
			tableindex = tableIndex;
			GroupName = groupStr;
		}
		public nodeTag(int level,int NodeID, int tableIndex, string groupStr)
		{
			treeLevel = level;
			ODIndexNo = 0;
			subIndex = 0;
			nodeID = NodeID;
			tableindex = tableIndex;
			GroupName = groupStr;
		}
	};

	#region PERSONALITY_PARAMS_WINDOW class
	/// <summary>
	/// Summary description for PERSONALITY_PARAMS_WINDOW.
	/// </summary>
	
	public class PERSONALITY_PARAMS_WINDOW : System.Windows.Forms.Form
	{
		#region form controls definition
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.Panel writeOnlyPanel;
		private System.Windows.Forms.Panel readOnlyPanel;
		private System.Windows.Forms.Panel headerPanel;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Panel readWritePanel;
		private System.Windows.Forms.Panel keyPanel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Panel writeInPreOpPanel;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Button closeBtn;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Label label11;
		private System.Timers.Timer timer2;
		private System.Windows.Forms.Panel readWriteInPreopPanel;
		#endregion

		#region my defintions
		private PPDataTable table;
		private SystemInfo localSystemInfo;
		private int selectednodeID = 0;
		private string selectedNodeText;
		public DataView dataview;
		private Thread thread;
		private int nodeIndex = 0;
		Hashtable sectionNames_ht = new Hashtable();
		public static System.Drawing.Color [] colArray;
		private bool PreOpInvoked = false;
		private DIFeedbackCode feedback;
		private PPTableStyle tablestyle;
		float [] percents = {0.06F, 0.06F, 0.28F, 0.08F, 0.10F, 0.10F, 0.10F, 0.16F, 0.06F};
		public static string DomainRequest = "";
		private System.Windows.Forms.MenuItem viewMI;
		private System.Windows.Forms.MenuItem hideROItemsMI;
		Manufacturer nodeType;
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
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.viewMI = new System.Windows.Forms.MenuItem();
			this.hideROItemsMI = new System.Windows.Forms.MenuItem();
			this.dataGrid1 = new System.Windows.Forms.DataGrid();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.submitBtn = new System.Windows.Forms.Button();
			this.closeBtn = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.keyPanel = new System.Windows.Forms.Panel();
			this.writeInPreOpPanel = new System.Windows.Forms.Panel();
			this.label9 = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.label11 = new System.Windows.Forms.Label();
			this.readWriteInPreopPanel = new System.Windows.Forms.Panel();
			this.label10 = new System.Windows.Forms.Label();
			this.panel4 = new System.Windows.Forms.Panel();
			this.label8 = new System.Windows.Forms.Label();
			this.writeOnlyPanel = new System.Windows.Forms.Panel();
			this.readOnlyPanel = new System.Windows.Forms.Panel();
			this.headerPanel = new System.Windows.Forms.Panel();
			this.label7 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.readWritePanel = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.timer2 = new System.Timers.Timer();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			this.keyPanel.SuspendLayout();
			this.writeInPreOpPanel.SuspendLayout();
			this.readWriteInPreopPanel.SuspendLayout();
			this.readWritePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.timer2)).BeginInit();
			this.SuspendLayout();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.viewMI});
			// 
			// viewMI
			// 
			this.viewMI.Index = 0;
			this.viewMI.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				   this.hideROItemsMI});
			this.viewMI.Text = "&View";
			// 
			// hideROItemsMI
			// 
			this.hideROItemsMI.Index = 0;
			this.hideROItemsMI.Text = "&Hide read only items";
			this.hideROItemsMI.Click += new System.EventHandler(this.hideROItemsMI_Click);
			// 
			// dataGrid1
			// 
			this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dataGrid1.DataMember = "";
			this.dataGrid1.FlatMode = true;
			this.dataGrid1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid1.Location = new System.Drawing.Point(8, 48);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.ReadOnly = true;
			this.dataGrid1.Size = new System.Drawing.Size(1000, 619);
			this.dataGrid1.TabIndex = 11;
			this.dataGrid1.Resize += new System.EventHandler(this.dataGrid1_Resize);
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(8, 707);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(1000, 25);
			this.progressBar1.TabIndex = 13;
			// 
			// submitBtn
			// 
			this.submitBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.submitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.submitBtn.Location = new System.Drawing.Point(8, 675);
			this.submitBtn.Name = "submitBtn";
			this.submitBtn.Size = new System.Drawing.Size(192, 25);
			this.submitBtn.TabIndex = 15;
			this.submitBtn.Text = "&Submit Changes to device";
			this.submitBtn.Visible = false;
			this.submitBtn.Click += new System.EventHandler(this.submitBtn_Click);
			// 
			// closeBtn
			// 
			this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.closeBtn.Location = new System.Drawing.Point(864, 675);
			this.closeBtn.Name = "closeBtn";
			this.closeBtn.Size = new System.Drawing.Size(144, 25);
			this.closeBtn.TabIndex = 16;
			this.closeBtn.Text = "&Close window";
			this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(672, 24);
			this.label1.TabIndex = 17;
			// 
			// comboBox1
			// 
			this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox1.Location = new System.Drawing.Point(720, 8);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(288, 24);
			this.comboBox1.TabIndex = 18;
			this.comboBox1.Text = "Show All";
			this.comboBox1.Visible = false;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// timer1
			// 
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// keyPanel
			// 
			this.keyPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.keyPanel.Controls.Add(this.writeInPreOpPanel);
			this.keyPanel.Controls.Add(this.label11);
			this.keyPanel.Controls.Add(this.readWriteInPreopPanel);
			this.keyPanel.Controls.Add(this.label8);
			this.keyPanel.Controls.Add(this.writeOnlyPanel);
			this.keyPanel.Controls.Add(this.readOnlyPanel);
			this.keyPanel.Controls.Add(this.headerPanel);
			this.keyPanel.Controls.Add(this.label7);
			this.keyPanel.Controls.Add(this.label4);
			this.keyPanel.Controls.Add(this.label3);
			this.keyPanel.Controls.Add(this.label5);
			this.keyPanel.Controls.Add(this.label6);
			this.keyPanel.Controls.Add(this.readWritePanel);
			this.keyPanel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.keyPanel.Location = new System.Drawing.Point(8, 707);
			this.keyPanel.Name = "keyPanel";
			this.keyPanel.Size = new System.Drawing.Size(704, 24);
			this.keyPanel.TabIndex = 21;
			this.keyPanel.Visible = false;
			// 
			// writeInPreOpPanel
			// 
			this.writeInPreOpPanel.BackColor = System.Drawing.Color.Brown;
			this.writeInPreOpPanel.Controls.Add(this.label9);
			this.writeInPreOpPanel.Controls.Add(this.panel3);
			this.writeInPreOpPanel.Location = new System.Drawing.Point(536, 8);
			this.writeInPreOpPanel.Name = "writeInPreOpPanel";
			this.writeInPreOpPanel.Size = new System.Drawing.Size(16, 16);
			this.writeInPreOpPanel.TabIndex = 66;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(-20, -72);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(72, 32);
			this.label9.TabIndex = 65;
			this.label9.Text = "Read/Write";
			this.label9.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.Color.PapayaWhip;
			this.panel3.Location = new System.Drawing.Point(-36, -72);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(32, 32);
			this.panel3.TabIndex = 64;
			// 
			// label11
			// 
			this.label11.BackColor = System.Drawing.Color.Transparent;
			this.label11.Location = new System.Drawing.Point(408, 8);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(136, 16);
			this.label11.TabIndex = 69;
			this.label11.Text = "Read/Write in Pre-Op";
			this.label11.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// readWriteInPreopPanel
			// 
			this.readWriteInPreopPanel.BackColor = System.Drawing.Color.DarkGreen;
			this.readWriteInPreopPanel.Controls.Add(this.label10);
			this.readWriteInPreopPanel.Controls.Add(this.panel4);
			this.readWriteInPreopPanel.Location = new System.Drawing.Point(392, 8);
			this.readWriteInPreopPanel.Name = "readWriteInPreopPanel";
			this.readWriteInPreopPanel.Size = new System.Drawing.Size(16, 16);
			this.readWriteInPreopPanel.TabIndex = 68;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(-20, -72);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(72, 32);
			this.label10.TabIndex = 65;
			this.label10.Text = "Read/Write";
			this.label10.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.Color.PapayaWhip;
			this.panel4.Location = new System.Drawing.Point(-36, -72);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(32, 32);
			this.panel4.TabIndex = 64;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(552, 8);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(128, 16);
			this.label8.TabIndex = 66;
			this.label8.Text = "Write only in Pre-Op";
			this.label8.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// writeOnlyPanel
			// 
			this.writeOnlyPanel.BackColor = System.Drawing.Color.Lavender;
			this.writeOnlyPanel.Location = new System.Drawing.Point(224, 8);
			this.writeOnlyPanel.Name = "writeOnlyPanel";
			this.writeOnlyPanel.Size = new System.Drawing.Size(16, 16);
			this.writeOnlyPanel.TabIndex = 58;
			// 
			// readOnlyPanel
			// 
			this.readOnlyPanel.BackColor = System.Drawing.Color.Gray;
			this.readOnlyPanel.Location = new System.Drawing.Point(144, 8);
			this.readOnlyPanel.Name = "readOnlyPanel";
			this.readOnlyPanel.Size = new System.Drawing.Size(16, 16);
			this.readOnlyPanel.TabIndex = 57;
			// 
			// headerPanel
			// 
			this.headerPanel.BackColor = System.Drawing.Color.PapayaWhip;
			this.headerPanel.Location = new System.Drawing.Point(56, 8);
			this.headerPanel.Name = "headerPanel";
			this.headerPanel.Size = new System.Drawing.Size(16, 16);
			this.headerPanel.TabIndex = 64;
			// 
			// label7
			// 
			this.label7.BackColor = System.Drawing.Color.Transparent;
			this.label7.Location = new System.Drawing.Point(72, 8);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(80, 16);
			this.label7.TabIndex = 65;
			this.label7.Text = "Header row";
			this.label7.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label4
			// 
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.Location = new System.Drawing.Point(320, 8);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(72, 16);
			this.label4.TabIndex = 63;
			this.label4.Text = "Read/Write";
			this.label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label3
			// 
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Location = new System.Drawing.Point(240, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 16);
			this.label3.TabIndex = 62;
			this.label3.Text = "Write only";
			this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label5
			// 
			this.label5.BackColor = System.Drawing.Color.Transparent;
			this.label5.Location = new System.Drawing.Point(160, 8);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(72, 16);
			this.label5.TabIndex = 61;
			this.label5.Text = "Read only";
			this.label5.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label6
			// 
			this.label6.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label6.Location = new System.Drawing.Point(8, 8);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 16);
			this.label6.TabIndex = 60;
			this.label6.Text = "Key:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// readWritePanel
			// 
			this.readWritePanel.BackColor = System.Drawing.Color.PapayaWhip;
			this.readWritePanel.Controls.Add(this.label2);
			this.readWritePanel.Controls.Add(this.panel1);
			this.readWritePanel.Location = new System.Drawing.Point(304, 8);
			this.readWritePanel.Name = "readWritePanel";
			this.readWritePanel.Size = new System.Drawing.Size(16, 16);
			this.readWritePanel.TabIndex = 59;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(-20, -72);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 32);
			this.label2.TabIndex = 65;
			this.label2.Text = "Read/Write";
			this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.PapayaWhip;
			this.panel1.Location = new System.Drawing.Point(-36, -72);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(32, 32);
			this.panel1.TabIndex = 64;
			// 
			// timer2
			// 
			this.timer2.Enabled = true;
			this.timer2.Interval = 1000;
			this.timer2.SynchronizingObject = this;
			this.timer2.Elapsed += new System.Timers.ElapsedEventHandler(this.timer2_Elapsed);
			// 
			// PERSONALITY_PARAMS_WINDOW
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(1013, 760);
			this.Controls.Add(this.keyPanel);
			this.Controls.Add(this.dataGrid1);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.closeBtn);
			this.Controls.Add(this.submitBtn);
			this.Controls.Add(this.progressBar1);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Menu = this.mainMenu1;
			this.Name = "PERSONALITY_PARAMS_WINDOW";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Personality parameters";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.PERSONALITY_PARAMS_WINDOW_Closing);
			this.Load += new System.EventHandler(this.PERSONALITY_PARAMS_WINDOW_Load);
			this.Closed += new System.EventHandler(this.PERSONALITY_PARAMS_WINDOW_Closed);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			this.keyPanel.ResumeLayout(false);
			this.writeInPreOpPanel.ResumeLayout(false);
			this.readWriteInPreopPanel.ResumeLayout(false);
			this.readWritePanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.timer2)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region intialisation
		/*--------------------------------------------------------------------------
		 *  Name			: PERSONALITY_PARAMS_WINDOW()
		 *  Description     : Constructor function for form. Set up of any initial variables
		 *					  that are available prior to th eform load event.
		 *  Parameters      : systemInfo class, CANopen node number and a descriptive
		 *					  string about the current CANopen node.
		 *  Used Variables  : none
		 *  Preconditions   : This form is only available when at least one SEVCON or 3rd party node is 
		 *					  connected.  SEVCON nodes can only be selected when the user has logged in.
		 *  Return value    : none
		 *--------------------------------------------------------------------------*/
		public PERSONALITY_PARAMS_WINDOW( ref SystemInfo systemInfo, int nodeNum, string nodeText, StatusBar passed_StatusBar, ToolBar passed_ToolBar )
		{
			InitializeComponent();
			localSystemInfo = systemInfo;
			selectednodeID = nodeNum;
			this.selectedNodeText = nodeText;
			feedback = this.localSystemInfo.getNodeNumber(nodeNum, out nodeIndex );
			if(feedback != DIFeedbackCode.DISuccess)
			{
				Message.Show("Unable to dereference node. This window will close");
				this.Close();
			}
			nodeType = this.localSystemInfo.nodes[nodeIndex].manufacturer;
			this.statusbar = passed_StatusBar;
			this.toolbar = passed_ToolBar;
			#region comboBox hash table
			string [] sectionNames = {"Show All", "Battery Application", "Communication Profile", "Generic I/O Profile", "Identity", "Logging", "Motor1 Profile", "Node Parameters", "Node Status", "Operating System Prompt", "PDO Mapping", "Power Steer Application", "Pump Application", "Security", "Store", "Traction Application", "Vehicle Application"};
			sectionNames_ht.Add(sectionNames[0], "Show All");
//			sectionNames_ht.Add(sectionNames[1], (SevconSectionType.BATTERY_APPLICATION.ToString()));
//			sectionNames_ht.Add(sectionNames[2], (SevconSectionType.COMMUNICATION_PROFILE.ToString()));
//			sectionNames_ht.Add(sectionNames[3], (SevconSectionType.GENERIC_IO_PROFILE.ToString()));
//			sectionNames_ht.Add(sectionNames[4], (SevconSectionType.IDENTITY.ToString()));
//			sectionNames_ht.Add(sectionNames[5], (SevconSectionType.LOGGING.ToString()));
//			sectionNames_ht.Add(sectionNames[6], (SevconSectionType.MOTOR1_PROFILE.ToString()));
//			sectionNames_ht.Add(sectionNames[7], (SevconSectionType.NODE_PARAMETERS.ToString()));
//			sectionNames_ht.Add(sectionNames[8], (SevconSectionType.NODE_STATUS.ToString()));
//			sectionNames_ht.Add(sectionNames[9], (SevconSectionType.OS_PROMPT.ToString()));
//			sectionNames_ht.Add(sectionNames[10], (SevconSectionType.PDO_MAPPING.ToString()));
//			sectionNames_ht.Add(sectionNames[11], (SevconSectionType.PSTEER_APPLICATION.ToString()));
//			sectionNames_ht.Add(sectionNames[12], (SevconSectionType.PUMP_APPLICATION.ToString()));
//			sectionNames_ht.Add(sectionNames[13], (SevconSectionType.SECURITY.ToString()));
//			sectionNames_ht.Add(sectionNames[14], (SevconSectionType.STORE.ToString()));
//			sectionNames_ht.Add(sectionNames[15], (SevconSectionType.TRACTION_APPLICATION.ToString()));
//			sectionNames_ht.Add(sectionNames[16], (SevconSectionType.VEHICLE_APPLICATION.ToString()));
			comboBox1.DataSource = sectionNames;
			#endregion
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: PERSONALITY_PARAMS_WINDOW_Load
		///		 *  Description     : Event HAndler for form load event
		///		 *  Parameters      : System event arguments 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void PERSONALITY_PARAMS_WINDOW_Load(object sender, System.EventArgs e)
		{
			this.Text = "Drive Wizard: Node parameter configuration";
			feedback =  this.localSystemInfo.getNodeNumber( selectednodeID, out nodeIndex );
			if(feedback != DIFeedbackCode.DISuccess)
			{
				Message.Show("Unable to reference " + this.selectedNodeText  + " data. \nError code: " +  feedback.ToString() );
				this.Close();
			}
			SCCorpStyle.formatDataGrid(ref this.dataGrid1);
			SCCorpStyle.formatColourPanels(ref this.readOnlyPanel, ref this.readWritePanel, ref this.writeOnlyPanel, ref this.headerPanel, ref this.writeInPreOpPanel, ref this.readWriteInPreopPanel);
			#region button colouring
			foreach (Control myControl in this.Controls)
			{
				if((myControl.GetType().ToString()) == "System.Windows.Forms.Button")
				{
					myControl.BackColor = SCCorpStyle.buttonBackGround;
				}
			}
			#endregion button colouring
			this.table = new PPDataTable();
			this.dataview = new DataView(this.table);
			this.dataview.AllowNew = false;
			//this.dataGrid1.DataSource = this.dataview;
			this.progressBar1.Maximum = this.localSystemInfo.nodes[ nodeIndex ].noOfItemsInOD;
			#region data retrieval thread start
			thread = new Thread(new ThreadStart( getODData )); 
			thread.Name = "PersonalityParametersTable";
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.BelowNormal;
#if DEBUG
			System.Console.Out.WriteLine("Thread: " + thread.Name + " started");
#endif
			thread.Start(); 
			timer1.Enabled = true;
			#endregion
		}
		private void getEDSData()
		{
			string abortMessage = "";;
			long ODvalue = 0;
			DataRow row;
			statusbar.Panels[2].Text = "Retrieving data from EDS";
			this.progressBar1.Value = this.progressBar1.Minimum;

			if(this.localSystemInfo.nodes[nodeIndex].isSevconApplication()==true)		//AJK,01/06/05
			{
				#region check if we are in pre-op
				feedback = this.localSystemInfo.readODItemValue(this.selectednodeID, SevconObjectType.NMT_STATE, 0x0, out ODvalue, out abortMessage);
				if(feedback == DIFeedbackCode.DISuccess)
				{
					if(ODvalue == 127) //pre-operational
					{
						PreOpInvoked = true;
					}
					else
					{
						PreOpInvoked = false;
					}
				}
				#endregion check if we are in pre-op
			}
#if GUIMOCKUP
			if ( MAIN_WINDOW.MockUpOffline == true )
			{//in virtual mode use the node state - since this is what we 'fiddled'
				if (this.localSystemInfo.nodes[nodeIndex].nodeState == NodeState.PreOperational)
				{
					this.PreOpInvoked = true;
				}
				else 
				{
					this.PreOpInvoked = false;
				}
			}
#endif
			foreach(ODItemData[] myEDSRow in localSystemInfo.nodes[nodeIndex].dictionary.data)
			{
				foreach(ODItemData myData in myEDSRow)
				{
					row = this.table.NewRow();
					row[(int) (PPCol.Index)] = myData.indexNumber.ToString("X");
					row[(int) (PPCol.param)] = myData.parameterName;
					row[(int) (PPCol.units)] = myData.units;
					row[(int) (PPCol.displayType)] = myData.displayType;
					row[(int) (PPCol.sectionType)] = myData.sectionType.ToString();
					row[(int) (PPCol.objectType)] = myData.objectName.ToString();
					row[(int) (PPCol.accessLevel)] = myData.accessLevel.ToString();
					row[(int) (PPCol.accessType)] = myData.accessType.ToString();
					row[(int) (PPCol.scaling)] = myData.scaling.ToString();
					row[(int) (PPCol.format)] = myData.format;
					row[(int) (PPCol.numberFormat)] = myData.format.ToString();
					if(myData.accessType == ObjectAccessType.DWDisplayOnly)
					{
						row[(int) (PPCol.sub)] =  "";
						row[(int) (PPCol.PDOMap)] = "";
						row[(int) (PPCol.defVal)] = "";
						row[(int) (PPCol.lowVal)] = "";
						row[(int) (PPCol.highVal)] = "";
					}
					else
					{
						row[(int) (PPCol.sub)] = myData.subNumber.ToString();
						if((bool) (myData.PDOmappable == true))
						{
							row[(int) (PPCol.PDOMap)] = "yes";
						}
						else
						{
							row[(int) (PPCol.PDOMap)] = "no";
						}
						switch(myData.displayType)
						{
							case DriveWizard.CANopenDataType.VISIBLE_STRING:
							case DriveWizard.CANopenDataType.UNICODE_STRING:
							case DriveWizard.CANopenDataType.OCTET_STRING:
								#region string
								if(myData.defaultValue == 0)
								{
									row[(int) (PPCol.defVal)] = "";
								}
								else
								{
									row[(int) (PPCol.defVal)] = myData.defaultValue.ToString();
								}
								if(myData.lowLimit == 0)
								{
									row[(int) (PPCol.lowVal)] = "";	
								}
							
								else
								{
									row[(int) (PPCol.lowVal)] = myData.lowLimit.ToString();
								}
								if(myData.highLimit == 0)
								{
									row[(int) (PPCol.highVal)] = "";
								}
								else
								{
									row[(int) (PPCol.highVal)] = myData.highLimit.ToString();
								}
								#endregion
								break;

							case DriveWizard.CANopenDataType.UNSIGNED16:
							case DriveWizard.CANopenDataType.UNSIGNED24:
							case DriveWizard.CANopenDataType.UNSIGNED32:
							case DriveWizard.CANopenDataType.UNSIGNED40:
							case DriveWizard.CANopenDataType.UNSIGNED48:
							case DriveWizard.CANopenDataType.UNSIGNED56:
							case DriveWizard.CANopenDataType.UNSIGNED64:
							case DriveWizard.CANopenDataType.UNSIGNED8:
							case DriveWizard.CANopenDataType.INTEGER16:
							case DriveWizard.CANopenDataType.INTEGER24:
							case DriveWizard.CANopenDataType.INTEGER32:
							case DriveWizard.CANopenDataType.INTEGER40:
							case DriveWizard.CANopenDataType.INTEGER48:
							case DriveWizard.CANopenDataType.INTEGER56:
							case DriveWizard.CANopenDataType.INTEGER64:
							case DriveWizard.CANopenDataType.INTEGER8:
								#region numerical
								if (myData.format == DriveWizard.SevconNumberFormat.SPECIAL)
								{
									#region SPECIAL
									row[(int) (PPCol.numberFormat)] = myData.formatList;
									row[(int) (PPCol.defVal)] = getEnumValue(myData.formatList, System.Convert.ToInt32(myData.defaultValue));
									row[(int) (PPCol.lowVal)] = getEnumValue(myData.formatList, System.Convert.ToInt32(myData.lowLimit));
									row[(int) (PPCol.highVal)] = getEnumValue(myData.formatList, System.Convert.ToInt32(myData.highLimit));
									#endregion SPECIAL
								}
								else if(myData.format == DriveWizard.SevconNumberFormat.BASE16)
								{
									#region  BASE 16
									row[(int) (PPCol.defVal)] = "0x" + myData.defaultValue.ToString("X");
									row[(int) (PPCol.lowVal)] = "0x" + myData.lowLimit.ToString("X");
									row[(int) (PPCol.highVal)] = "0x" + myData.highLimit.ToString("X");
									#endregion  BASE 16
								}
								else
								{
									#region BASE 10 or third party
									if(myData.scaling == 1)
									{   //avoid integer to float conversions for integer numbers
										row[(int) (PPCol.defVal)] = myData.defaultValue.ToString();
										row[(int) (PPCol.lowVal)] = myData.lowLimit.ToString();
										row[(int) (PPCol.fullMin)] = myData.lowLimit.ToString(); //used for comparisons
										row[(int) (PPCol.highVal)] = myData.highLimit.ToString();
										row[(int) (PPCol.fullMax)] = myData.highLimit.ToString();  //used for comparisons
									}
									else
									{
										float scaledValue = Convert.ToSingle(myData.defaultValue * myData.scaling);
										row[(int) (PPCol.defVal)] = scaledValue.ToString();
										scaledValue = Convert.ToSingle(myData.lowLimit * myData.scaling);
										row[(int) (PPCol.lowVal)] = scaledValue.ToString();
										row[(int) (PPCol.fullMin)] = scaledValue.ToString(); //used for comparisons
										scaledValue = Convert.ToSingle(myData.highLimit * myData.scaling);
										row[(int) (PPCol.highVal)] = scaledValue.ToString();
										row[(int) (PPCol.fullMax)] = scaledValue.ToString();  //used for comparisons
									}
									#endregion BASE 10 or third party
								}
								#endregion
								break;

							case DriveWizard.CANopenDataType.BOOLEAN:
								#region boolean
								if (myData.format == DriveWizard.SevconNumberFormat.SPECIAL)
								{
									#region SPECIAL
									row[(int) (PPCol.numberFormat)] = myData.formatList;
									row[(int) (PPCol.defVal)] = getEnumValue(myData.formatList, System.Convert.ToInt32(myData.defaultValue));
									row[(int) (PPCol.lowVal)] = getEnumValue(myData.formatList, System.Convert.ToInt32(myData.lowLimit));
									row[(int) (PPCol.highVal)] = getEnumValue(myData.formatList, System.Convert.ToInt32(myData.highLimit));
									#endregion SPECIAL
								}
								else
								{
									string formatString  = "false_0000:true_0001";
									row[(int) (PPCol.numberFormat)] = formatString;
									row[(int) (PPCol.defVal)] = getEnumValue(formatString, System.Convert.ToInt32(myData.defaultValue));
									row[(int) (PPCol.lowVal)] = getEnumValue(formatString, System.Convert.ToInt32(myData.lowLimit));
									row[(int) (PPCol.highVal)] = getEnumValue(formatString, System.Convert.ToInt32(myData.highLimit));
								}
								#endregion boolean
								break;

							case DriveWizard.CANopenDataType.DOMAIN:
								#region domain
								row[(int) (PPCol.defVal)] = "Domain";
								row[(int) (PPCol.lowVal)] = "Domain";
								row[(int) (PPCol.highVal)] = "Domain";
								#endregion
								break;

							default:
								#region default
								row[(int) (PPCol.defVal)] = "";
								row[(int) (PPCol.lowVal)] = "";
								row[(int) (PPCol.highVal)] = "";
								#endregion
								break;
						}
					}
					//add actual values in later
					this.table.Rows.Add(row);
				}
				this.progressBar1.Value++;
			}
			this.table.AcceptChanges();  //mark all rows as unchanged to see later changes
		}
		private void getODData()
		{
			statusbar.Panels[2].Text = "Checking Contoller Operating State of: " + this.selectedNodeText;
			if(this.localSystemInfo.nodes[nodeIndex].nodeState == NodeState.PreOperational)
			{
				PreOpInvoked = true;
			}
			else
			{
				PreOpInvoked = false;
			}
			statusbar.Panels[2].Text = "Retrieving data from " + this.selectedNodeText;
			feedback = localSystemInfo.readEntireOD( selectednodeID);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				Message.Show("Attempt to read Object Dictionary failed. \nError code: " + feedback.ToString());
			}
		}
		private void InsertActualValuesIntoTable()
		{
			getEDSData();  //put here to shorten the form load event handler and show window earlier - revisit
			int row = 0;
			statusbar.Panels[2].Text = "Inserting " + this.selectedNodeText + " OD values into table";
			this.progressBar1.Value = this.progressBar1.Minimum;
			foreach(ODItemData[] myEDSRow in localSystemInfo.nodes[nodeIndex].dictionary.data)
			{
				foreach(ODItemData myData in myEDSRow)
				{
					if((myData.accessType == ObjectAccessType.DWDisplayOnly) || (myData.accessType == ObjectAccessType.WriteOnly))
					{
						this.table.Rows[row][(int) (PPCol.actValue)] = "";
					}
					else
					{
						switch(myData.displayType)
						{
							case DriveWizard.CANopenDataType.VISIBLE_STRING:
							case DriveWizard.CANopenDataType.UNICODE_STRING:
							case DriveWizard.CANopenDataType.OCTET_STRING:
								this.table.Rows[row][(int) (PPCol.actValue)] = myData.currentValueString;  
								break;

							case DriveWizard.CANopenDataType.UNSIGNED16:
							case DriveWizard.CANopenDataType.UNSIGNED24:
							case DriveWizard.CANopenDataType.UNSIGNED32:
							case DriveWizard.CANopenDataType.UNSIGNED40:
							case DriveWizard.CANopenDataType.UNSIGNED48:
							case DriveWizard.CANopenDataType.UNSIGNED56:
							case DriveWizard.CANopenDataType.UNSIGNED64:
							case DriveWizard.CANopenDataType.UNSIGNED8:
							case DriveWizard.CANopenDataType.INTEGER16:
							case DriveWizard.CANopenDataType.INTEGER24:
							case DriveWizard.CANopenDataType.INTEGER32:
							case DriveWizard.CANopenDataType.INTEGER40:
							case DriveWizard.CANopenDataType.INTEGER48:
							case DriveWizard.CANopenDataType.INTEGER56:
							case DriveWizard.CANopenDataType.INTEGER64:
							case DriveWizard.CANopenDataType.INTEGER8:
								#region numerical
								if( myData.format == DriveWizard.SevconNumberFormat.SPECIAL)
								{
									this.table.Rows[row][(int)(PPCol.actValue)] = this.getEnumValue(myData.formatList, System.Convert.ToInt32(myData.currentValue));
								}
								else if(myData.format == DriveWizard.SevconNumberFormat.BASE16)
								{
									this.table.Rows[row][(int) (PPCol.actValue)]  = "0x" + myData.currentValue.ToString("X");
								}
								else
								{
									float scaledValue = Convert.ToSingle(myData.currentValue * myData.scaling);
									this.table.Rows[row][(int) (PPCol.actValue)] = scaledValue.ToString();
									this.table.Rows[row][(int) (PPCol.fullValue)] = scaledValue.ToString();
								}
								#endregion
								break;

							case DriveWizard.CANopenDataType.BOOLEAN:
								if(myData.format == DriveWizard.SevconNumberFormat.SPECIAL)
								{
									this.table.Rows[row][(int) (PPCol.actValue)] = this.getEnumValue(myData.formatList, System.Convert.ToInt32(myData.currentValue));
								}
								else
								{
									table.Rows[row][(int) (PPCol.actValue)] = Convert.ToBoolean(myData.currentValue).ToString().ToLower();
								}
								break;

							case DriveWizard.CANopenDataType.DOMAIN:
								table.Rows[row][(int) (PPCol.actValue)]  = "Domain";
								break;

							default:
								table.Rows[row][(int) (PPCol.actValue)]  = "";
								break;
						}
					}
					row++;
				}
				this.progressBar1.Value++;
			}
			this.table.AcceptChanges();  //mark all rows as unchanged to see later changes
			#region remove 'number of entries' rows
			int col = (int)(PPCol.sub);
			for(int rowNo=0; rowNo<(this.table.Rows.Count);rowNo++)
			{
				if( (this.table.Rows[rowNo][(int) (PPCol.accessType)].ToString() == DriveWizard.ObjectAccessType.ReadOnly.ToString())
					|| (this.table.Rows[rowNo][(int) (PPCol.accessType)].ToString() == DriveWizard.ObjectAccessType.Constant.ToString())
					|| (this.table.Rows[rowNo][(int) (PPCol.accessType)].ToString() == DriveWizard.ObjectAccessType.DWDisplayOnly.ToString()))
				{

					if( (this.table.Rows[rowNo][col].ToString() != "") && (rowNo<this.table.Rows.Count-1) && (this.table.Rows[rowNo+1][col].ToString() != "") )
					{
						if(this.table.Rows[rowNo][col].ToString() == "0") //sub zero and not last row
						{
							if( this.table.Rows[rowNo+1][col].ToString() == "1")
							{
								this.table.Rows[rowNo].Delete();
							}
						}
					}
				}
				else if 
					( (table.Rows[rowNo][(int) (PPCol.accessType)].ToString()  == DriveWizard.ObjectAccessType.WriteOnly.ToString())  
					|| (table.Rows[rowNo][(int) (PPCol.accessType)].ToString() == DriveWizard.ObjectAccessType.WriteOnlyInPreOp.ToString()) 
					)
				{
					uint ObjectAccessReq  = System.Convert.ToUInt32(this.table.Rows[rowNo][(int) (PPCol.accessLevel)]);
					if(ObjectAccessReq>this.localSystemInfo.systemAccess)
					{
						this.table.Rows[rowNo].Delete();  //if we don't have access to wrtie to a write only object then ther eis no point in displaying it
					}
				}

			}
			#endregion
			this.table.AcceptChanges();  //mark all rows as unchanged to see later changes
			this.dataview.ListChanged += new System.ComponentModel.ListChangedEventHandler(dataViewChangeHandler);
			this.dataview.Sort = PPCol.Index.ToString();  //converts from EDS order to ordered by index number
			//event handler declared after initial input of vlaues - otherwise it checks these too
			this.table.ColumnChanged += new System.Data.DataColumnChangeEventHandler(this.table_ColumnChanged);

			dataGrid1.CaptionText = "Personality Parameters for " + this.selectedNodeText;
			createPPTableStyle();
			applyPPTableStyle();
			statusbar.Panels[2].Text = "";
			showUserControls();
		}
		#endregion

		#region user interaction zone
		/// <summary>
		/// Performs and final data verification that could not be done earlier.  
		/// Transmits all accepted changes to controller checks feedback code and reads each value back.  
		/// Also for sleected known related SVCON items re-reads and updates releated items that 
		/// may also have changed in controller. 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void submitBtn_Click(object sender, System.EventArgs e)
		{
			string abortMessage = "";
			string errorMessage = "";
			hideUserControls();
			for(int i = 0;i<this.table.Rows.Count;i++)
			{
				#region clear row error flags
				if(this.table.Rows[i].HasErrors)
				{
					this.table.Rows[i].RejectChanges();
					this.table.Rows[i].ClearErrors();
				}
				#endregion
				if(this.table.Rows[i].RowState == DataRowState.Modified)
				{
					#region get index and subIndex
					int index = System.Convert.ToInt32((this.table.Rows[i][(int)(PPCol.Index)].ToString()), 16);
					int sub = System.Convert.ToInt32(this.table.Rows[i][(int)(PPCol.sub)]);
					#endregion
					string newValueString = this.table.Rows[i][(int)(PPCol.actValue)].ToString();
					long newValue = 0;
					CANopenDataType datatype = (CANopenDataType)(this.table.Rows[i][(int) (PPCol.displayType)]);
					#region switch datatype
					switch (datatype)
					{
						case CANopenDataType.VISIBLE_STRING:
						case CANopenDataType.UNICODE_STRING:
						case CANopenDataType.OCTET_STRING:
							#region string
							feedback = this.localSystemInfo.writeODItemValue(this.selectednodeID, index, sub, newValueString, out abortMessage);
							if ( feedback != DIFeedbackCode.DISuccess )
							{
								errorMessage += "\nUnable to write value " +  newValue.ToString() + " to 0x: " + index.ToString("X")+ "sub: " + sub.ToString() + ". \nError code: " + feedback.ToString() + " " + abortMessage;
							}
							#region read back string 
							string myString = "";
							if(table.Rows[i][(int) (PPCol.accessType)].ToString() != DriveWizard.ObjectAccessType.WriteOnly.ToString())
							{
								feedback = this.localSystemInfo.readODItemValue(selectednodeID, index, sub , out myString, out abortMessage);
								if(feedback == DIFeedbackCode.DISuccess)
								{
									table.Rows[i][(int) (PPCol.actValue)] = myString;
								}
								else
								{
									errorMessage += "\nUnable to read 0x: " + index.ToString("X")+ "sub: " + sub.ToString() + ". \nError code: " + feedback.ToString() + " " + abortMessage;
								}
							}
							else  //write only so just blank the entry
							{
								table.Rows[i][(int) (PPCol.actValue)] = "";
							}
							#endregion read back string 
							#endregion string 
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
							DriveWizard.SevconNumberFormat myIntFormat = (SevconNumberFormat)(table.Rows[i][(int) (PPCol.format)]);
							if(myIntFormat == SevconNumberFormat.SPECIAL)
							{
								#region SPECIAL
								//get the equivalent integer
								bool dereferencedOK = false;
								string inputString = table.Rows[i][(int)(PPCol.actValue)].ToString();
								string formatStr = table.Rows[i][(int) (PPCol.numberFormat)].ToString();
								newValue = getValueFromEnumeration(inputString,  formatStr, out dereferencedOK);
								if(dereferencedOK == true)
								{
									#region write and readback input value
									feedback = this.localSystemInfo.writeODItemValue(this.selectednodeID, index, sub, newValue, out abortMessage);
									if ( feedback != DIFeedbackCode.DISuccess )
									{
										errorMessage += "\nUnable to write value " +  newValue.ToString() + " to 0x: " + index.ToString("X")+ "sub: " + sub.ToString() + ". \nError code: " + feedback.ToString() + " " + abortMessage;
									}
									#region read back SPECIAL
									long myValue = 0;
									if(table.Rows[i][(int) (PPCol.accessType)].ToString() != DriveWizard.ObjectAccessType.WriteOnly.ToString())
									{
										feedback = this.localSystemInfo.readODItemValue(selectednodeID, index, sub , out myValue, out abortMessage);
										if(feedback == DIFeedbackCode.DISuccess)
										{
											this.table.Rows[i][(int)(PPCol.actValue)] = this.getEnumValue(formatStr,myValue);
										}
										else
										{
											errorMessage += "\nUnable to read 0x: " + index.ToString("X")+ "sub: " + sub.ToString() + ". \nError code: " + feedback.ToString() + " " + abortMessage;
										}
									}
									else  //write only so just blank the entry
									{
										table.Rows[i][(int) (PPCol.actValue)] = "";
									}
									#endregion read back SPECIAL
									#endregion write and readback input value
								}
								#endregion SPECIAL
							}
							else if(myIntFormat == SevconNumberFormat.BASE16)
							{
								#region BASE16
								string inputString = table.Rows[i][(int)(PPCol.actValue)].ToString().Remove(0,2);
								newValue = System.Convert.ToInt64(inputString,16);  //convert to long and base 10
								feedback = this.localSystemInfo.writeODItemValue(this.selectednodeID, index, sub, newValue, out abortMessage);
								if ( feedback != DIFeedbackCode.DISuccess )
								{
									errorMessage += "\nUnable to write value " +  newValue.ToString() + " to 0x: " + index.ToString("X")+ "sub: " + sub.ToString() + ". \nError code: " + feedback.ToString() + " " + abortMessage;
								}
								#region read back BASE16
								long myValue = 0;
								if(table.Rows[i][(int) (PPCol.accessType)].ToString() != DriveWizard.ObjectAccessType.WriteOnly.ToString())
								{

									feedback = this.localSystemInfo.readODItemValue(selectednodeID, index, sub , out myValue, out abortMessage);
									if(feedback == DIFeedbackCode.DISuccess)
									{
										//back into hex format for display
										this.table.Rows[i][(int)(PPCol.actValue)]  =  "0x" + myValue.ToString("X");
									}
									else
									{
										errorMessage += "\nUnable to read 0x: " + index.ToString("X")+ "sub: " + sub.ToString() + ". \nError code: " + feedback.ToString() + " " + abortMessage;
									}
								}
								else  //write only so just blank the entry
								{
									table.Rows[i][(int) (PPCol.actValue)] = "";
								}
								#endregion read back BASE16
								#endregion BASE16
							}
							else
							{
								#region BASE10  Or third party
								float tempDecimal = 0;
								bool hexFlag = false;
								#region test for hex input and convert to float
								string hexTest1 = "", hexTest2 = "";
								hexTest1 = this.table.Rows[i][(int)(PPCol.actValue)].ToString().ToUpper();
								if(hexTest1.Length>2) //need at least three chars for hex text and for this test
								{
									hexTest1 = hexTest1.Substring(0,1);
									hexTest2 = this.table.Rows[i][(int)(PPCol.actValue)].ToString().ToUpper();
									hexTest2 = hexTest2.Substring(1,1);
									if( (hexTest1 == "0") && (hexTest2 == "X") )
									{
										hexTest1 = this.table.Rows[i][(int)(PPCol.actValue)].ToString().ToUpper();
										hexTest1.Remove(0,2);  //string out the 0x
										//use integer vlaeus to prevent loss of accuracy when converting to and back form a float
										newValue = System.Convert.ToInt64(hexTest1,16);
										hexFlag = true;
									}
								}
								#endregion
								if(hexFlag == false)
								{
									tempDecimal = float.Parse(table.Rows[i][(int)(PPCol.actValue)].ToString());
									if(table.Rows[i][(int)(PPCol.scaling)].ToString() != "1") 
									{
										float scaling = float.Parse(table.Rows[i][(int)(PPCol.scaling)].ToString());
										float newDec =  (float)(tempDecimal/scaling);
										newValue = (long) newDec;
									}
									else
									{
										newValue = (long) (tempDecimal);
									}
								}
								#region attempt  to write to node
								feedback = this.localSystemInfo.writeODItemValue(this.selectednodeID, index, sub, newValue, out abortMessage);
								if ( feedback != DIFeedbackCode.DISuccess )
								{
									errorMessage += "\nUnable to write value " +  newValue.ToString() + " to 0x: " + index.ToString("X")+ "sub: " + sub.ToString() + ". \nError code: " + feedback.ToString() + " " + abortMessage;
								}
								#endregion attempt  to write to node
								if(table.Rows[i][(int) (PPCol.accessType)].ToString() == DriveWizard.ObjectAccessType.WriteOnly.ToString())
								{
									table.Rows[i][(int) (PPCol.actValue)] = "";
								}
								else if(
									(feedback == DIFeedbackCode.CANSubIndexDoesNotExist)
									||(feedback == DIFeedbackCode.CANObjectDoesNotExistInOD)
									)  //may be more to add here later
								{
									table.Rows[i].RejectChanges();
								}
								else
								{
									#region get scaled value back from node
									long myValue = 0;
									feedback = this.localSystemInfo.readODItemValue(selectednodeID, index, sub , out myValue, out abortMessage);
									if(feedback == DIFeedbackCode.DISuccess)
									{
										float scaledValue = float.Parse(table.Rows[i][(int)(PPCol.scaling)].ToString());
										scaledValue *= myValue;
										table.Rows[i][(int) (PPCol.actValue)] = scaledValue.ToString();
										table.Rows[i][(int) (PPCol.fullValue)] = scaledValue.ToString();
									}
									else
									{
										errorMessage += "\nUnable to read 0x: " + index.ToString("X")+ "sub: " + sub.ToString() + ". \nError code: " + feedback.ToString() + " " + abortMessage;
									}
									#endregion
								}
								#endregion BASE10
							}
							break;
							#endregion
						case CANopenDataType.BOOLEAN:
							#region boolean
							//get the equivalent integer
							bool booldereferencedOK = false;
							string boolinputString = table.Rows[i][(int)(PPCol.actValue)].ToString();
							string boolformatStr = table.Rows[i][(int) (PPCol.numberFormat)].ToString();
							newValue = getValueFromEnumeration(boolinputString,  boolformatStr, out booldereferencedOK);
							feedback = this.localSystemInfo.writeODItemValue(this.selectednodeID, index, sub, newValue, out abortMessage);
							if ( feedback == DIFeedbackCode.DISuccess )
							{
								long myValue = 0;
								if(table.Rows[i][(int) (PPCol.accessType)].ToString() != DriveWizard.ObjectAccessType.WriteOnly.ToString())
								{
									feedback = this.localSystemInfo.readODItemValue(selectednodeID, index, sub , out myValue, out abortMessage);
									if(feedback == DIFeedbackCode.DISuccess)
									{
										this.table.Rows[i][(int)(PPCol.actValue)] = this.getEnumValue(boolformatStr,myValue);
									}
									else
									{
										errorMessage += "\nUnable to read 0x: " + index.ToString("X")+ "sub: " + sub.ToString() + ". \nError code: " + feedback.ToString() + " " + abortMessage;
									}
								}
								else  //write only so just blank the entry
								{
									table.Rows[i][(int) (PPCol.actValue)] = "";
								}
								#region if change was request pre-op then we must also read & update the NMT_State
								if(table.Rows[i][(int) PPCol.objectType].ToString() == DriveWizard.SevconObjectType.FORCE_TO_PREOP.ToString())
								{
									long myNMTValue = 0;
									for(int j = 0;j<this.table.Rows.Count;j++)
									{
										if(table.Rows[j][(int) PPCol.objectType].ToString() == DriveWizard.SevconObjectType.NMT_STATE.ToString())
										{
											#region read and display the NMT state OD item
											//need to switch off the change handler to force re-display of NMT_STATE which is read only
											this.table.ColumnChanged -= new System.Data.DataColumnChangeEventHandler(this.table_ColumnChanged);
											feedback = this.localSystemInfo.readODItemValue(this.selectednodeID, SevconObjectType.NMT_STATE, 0, out myNMTValue, out abortMessage);
											if(feedback == DIFeedbackCode.DISuccess)
											{
												string formatStr = table.Rows[j][(int) (PPCol.numberFormat)].ToString();
												this.table.Rows[j][(int)(PPCol.actValue)] = this.getEnumValue(formatStr,myNMTValue);  //it is enumerated type
												this.table.Rows[j].AcceptChanges(); //prevent detection of this as a changed row
											}
											else
											{
												errorMessage += "\nUnable to update NTM State. \nError code: " + feedback.ToString() + " " + abortMessage;
											}
											this.table.ColumnChanged += new System.Data.DataColumnChangeEventHandler(this.table_ColumnChanged);
											break;
											#endregion read and display the NMT state OD item
										}
									
									}
									#region update preop variable
									if(this.localSystemInfo.nodes[nodeIndex].nodeState == NodeState.PreOperational)
									{
										PreOpInvoked = true;
									}
									else
									{
										PreOpInvoked = false;
									}
									#endregion update preop variable
								}
								#endregion if change was request pre-op then we must also read & update the NMT_State
							}
							#endregion
							break;

						default: // unhandled data types for V1
							break;
					}  //end switch
					#endregion
					table.Rows[i].AcceptChanges();  //finally mark the row as unmodified
				}  //end of  if row is modified
			}
			if(errorMessage != "")
			{
				Message.Show (errorMessage);
				statusbar.Panels[2].Text = "Displaying last confirmed values received from node";
			}
			else
			{
				statusbar.Panels[2].Text = "All values written and read back OK";
			}
			showUserControls();
		}

		private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			updateRowFiltering();
		}

		private void table_ColumnChanged(object sender, System.Data.DataColumnChangeEventArgs e)
		{
			string inputString = (e.ProposedValue.ToString()).ToUpper(); 
			#region null entry - causes old entry to be re-displayed
			if(inputString == "")
			{
				e.Row.RejectChanges();  //reject new value
				if(e.Row.HasErrors)
				{
					e.Row.ClearErrors(); //clear the error
				}
				e.ProposedValue = e.Row[(int) (PPCol.actValue)].ToString();  //bung in old value
				e.Row.AcceptChanges(); //and mark row as unmodified
				return;
			}
			#endregion
			#region read only items check
			if (
				(  
				(e.Row[(int) (PPCol.accessType)].ToString() == ObjectAccessType.DWDisplayOnly.ToString()) 
				|| (e.Row[(int) (PPCol.accessType)].ToString() == ObjectAccessType.Constant.ToString()) 
				|| (e.Row[(int) (PPCol.accessType)].ToString() == ObjectAccessType.ReadOnly.ToString()) 
				)

				|| ( 
				(this.PreOpInvoked == false) 
				&&
				( (e.Row[(int) (PPCol.accessType)].ToString() == ObjectAccessType.ReadReadWriteInPreOp.ToString()) 
				|| (e.Row[(int) (PPCol.accessType)].ToString() == ObjectAccessType.ReadWriteInPreOp.ToString()) 
				|| (e.Row[(int) (PPCol.accessType)].ToString() == ObjectAccessType.ReadWriteWriteInPreOp.ToString()) 
				|| (e.Row[(int) (PPCol.accessType)].ToString() == ObjectAccessType.WriteOnlyInPreOp.ToString()) )
				)
				)
			{
				e.Row.RejectChanges();  //reject new value
				if(e.Row.HasErrors)
				{
					e.Row.ClearErrors(); //clear the error
				}
				e.ProposedValue = e.Row[(int) (PPCol.actValue)].ToString();  //bung in old value
				e.Row.AcceptChanges(); //and mark row as unmodified
				return;
			}
			#endregion
			#region access level check
			int reqAccess = Convert.ToInt16(e.Row[(int) (PPCol.accessLevel)]);
			if(this.localSystemInfo.systemAccess < reqAccess)
			{
				e.Row.SetColumnError(e.Column, "Insufficient login level to overwrite this parameter - delete entry");
				Debug.Write( " Insufficient login level to overwrite this parameter - delete entry" );
				return;
			} //end if not high enough access
			#endregion
			//now test the actual value
			string errString = "";
			string datatypeString = e.Row[(int) (PPCol.displayType)].ToString();
			CANopenDataType datatype = (CANopenDataType)Enum.Parse(typeof(CANopenDataType), datatypeString);
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
					DriveWizard.SevconNumberFormat myIntFormat = (SevconNumberFormat) (e.Row[(int) (PPCol.format)]);
					if (myIntFormat == SevconNumberFormat.BASE16)
					{
						errString = hexErrorHandling(e.Row, inputString);
					}
					else if(myIntFormat == SevconNumberFormat.BASE10) //BASE 10
					{
						errString = numericalErrorHandling(e.Row, inputString);
					}
					//note no error handling is required gfor SPECIAL - since it it a drop down list
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
				case CANopenDataType.TIME_DIFFERENCE:
				case CANopenDataType.TIME_OF_DAY:
					errString =  "Data Type handling for " +  datatype.ToString() + " not implementetd";
					break;			

				case CANopenDataType.BOOLEAN:
					//no error handling is required for boolen - these are now drop down lists
					break;

				default:  
					break;
			} //end switch
			#endregion

			//implement error handling
			if(errString != "") 
			{
				e.Row.SetColumnError(e.Column, errString);
				Debug.Write( " " + errString );
				return;
			}
			//handle correct inputs
			e.Row.ClearErrors();  //reove error inidcator from row - ghost string will remain - can be got rid of by filtering table!
		}

		private string numericalErrorHandling(DataRow row, string inputString)
		{
			//first reject any invalid characters
			string inputcopy = inputString;  //take a copy of input										
			string inputcopy2 = inputString;  //take a copy of input	
			float inputValue;
			if(inputcopy2.Length>1)
			{
				inputcopy2 = inputcopy2.Substring(1,1);
			}
			inputcopy = inputcopy.Substring(0,1);
			if((inputcopy2 == "X") && (inputcopy == "0"))//we have hex input
			{
				if(inputcopy.Length>1)
				{
					inputcopy = inputString.Remove(0,2); //take copy and remove the 0X
				}
				char [] invalidChars = "GHIJKLMNOPQRSTUVWXYZ!\"£$%^&*()_+=\\|,<>?/:;'@#~{[}]".ToCharArray(); //" and \ need extra \to be accepted
				int invalidCharIndex = inputcopy.IndexOfAny(invalidChars);
				if(invalidCharIndex != -1)
				{
					return "Hexadecimal and numerical characters only";
				}
				else
				{
					inputValue = Convert.ToInt64(inputString, 16); //convert to base 10
				}
			}
			else  //float entry
			{
				inputcopy = inputString; //get full string back again
				char [] invalidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ!\"£$%^&*()_+=\\|,<>?/:;'@#~{[}]".ToCharArray(); //" and \ need extra \to be accepted
				int invalidCharIndex = inputcopy.IndexOfAny(invalidChars);
				if(invalidCharIndex != -1)
				{
					return "Numerical characters only";
				}
				else
				{
					inputValue = (float) Convert.ToDouble(inputString);  //use double to prevent conversion to scientific notation prior to doing compare - causes exception
				}
			}
			//now look at actual value
			string tempStr = row[(int)(PPCol.fullMin)].ToString();
			if ( inputValue < Convert.ToSingle(tempStr) )
			{
				return  "Entry must be greater than minimum value";
			}
			tempStr = row[(int)(PPCol.fullMax)].ToString();
			if (inputValue > Convert.ToSingle(tempStr))
			{
				return "Entry must be less than maximum value";
			}
			return "";
		}

		private string hexErrorHandling(DataRow row, string inputString)
		{
			//first reject any invalid characters
			string inputcopy = inputString;  //take a copy of input										
			string inputcopy2 = inputString;  //take a copy of input	
			long inputValue;
			if(inputcopy2.Length>1)
			{
				inputcopy2 = inputcopy2.Substring(1,1);
			}
			inputcopy = inputcopy.Substring(0,1);
			if((inputcopy2 == "X") && (inputcopy == "0"))//we have hex input
			{
				if(inputcopy.Length>1)
				{
					inputcopy = inputString.Remove(0,2); //take copy and remove the 0X
				}
				char [] invalidChars = "GHIJKLMNOPQRSTUVWXYZ!\"£$%^&*()_+=\\|,<>?/:;'@#~{[}]".ToCharArray(); //" and \ need extra \to be accepted
				int invalidCharIndex = inputcopy.IndexOfAny(invalidChars);
				if(invalidCharIndex != -1)
				{
					return "Hexadecimal parameter , entry formast is 0x####";
				}
				else
				{
					inputValue = Convert.ToInt64(inputString, 16); //convert to base 10
				}
			}
			else  //float entry
			{
				return "Hexadecimal parameter , entry format is 0x##...";
			}
			//now look at actual value
			string lowStr = row[(int)(PPCol.lowVal)].ToString().Remove(0,2);
			long lowLim  = Convert.ToInt64(lowStr, 16);
			if ( inputValue < lowLim )
			{
				return  "Entry must be greater than minimum value";
			}
			string highStr = row[(int)(PPCol.highVal)].ToString().Remove(0,2);
			long highLim = Convert.ToInt64(highStr, 16);
			if (inputValue > highLim)
			{
				return "Entry must be less than maximum value";
			}
			return "";
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (thread != null)
			{
				if((thread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
					timer1.Enabled = false; //kill timer
					InsertActualValuesIntoTable();
					this.progressBar1.Value = this.progressBar1.Minimum;
					this.toolbar.ButtonClick +=new ToolBarButtonClickEventHandler(toolbar_ButtonClick);
				}
				else
				{
					this.progressBar1.Value = this.localSystemInfo.nodes[ nodeIndex ].itemBeingRead;
				}
			}
		}
		private void dataViewChangeHandler(object sender,ListChangedEventArgs e )
		{
			colArray = new Color[this.dataview.Count]; 
			colArray = SCCorpStyle.ApplyRowColours(this.dataview, this.PreOpInvoked, this.localSystemInfo.systemAccess);
		}
		#endregion

		#region minor methods
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
		private string getEnumValue(string formatString, long Value)
		{
			string enumStr = "";
			string [] test = formatString.Split(':');
			string valueStr = Value.ToString();
			while(valueStr.Length<4)
			{
				valueStr = "0" + valueStr;
			}
			for(int i = 0;i<test.Length;i++)
			{
				if(test[i].IndexOf(valueStr) != -1)
				{
					enumStr = test[i].Remove(test[i].Length-5, 5);
					break;
				}
			}
			if (enumStr == "")  //enumerated string not found so use the value itself
			{
				enumStr = Value.ToString();
			}
			return enumStr;
		}
		private long getValueFromEnumeration(string inputString, string formatString, out bool dereferencedOK)
		{
			long enumValue = 0;
			dereferencedOK = false;
			string [] enumStrings = formatString.Split(':');
			for(int i = 0;i<enumStrings.Length;i++)
			{
				if(enumStrings[i].IndexOf(inputString) != -1)
				{
					string valString = enumStrings[i].Substring(enumStrings[i].Length-4, 4);  //get the number at the end
					enumValue = System.Convert.ToInt64(valString);
					dereferencedOK = true;
					return enumValue;
				}
			}
			return enumValue; 
		}
		private void showUserControls()
		{
			this.label1.Text = "Enter new values, then Submit to " + this.selectedNodeText;
			if(nodeType == Manufacturer.SEVCON)
			{
				this.comboBox1.Visible = true;
			}
			this.submitBtn.Visible = true;
			this.keyPanel.Visible = true;
			this.dataGrid1.ReadOnly = false;
			this.progressBar1.Visible = false;
		}
		private void hideUserControls()
		{
			//check feed back codes. Inform user - colours?
			this.comboBox1.Visible = false;
			this.submitBtn.Visible = false;
			statusbar.Panels[2].Text = "Submitting values to " + this.selectedNodeText;
		}
		private void dataGrid1_Resize(object sender, System.EventArgs e)
		{
			if((thread != null) && ((thread.ThreadState & System.Threading.ThreadState.Stopped) > 0 ))
			{
				applyPPTableStyle();
			}
		}
		private void createPPTableStyle()
		{
//			int [] colWidths  = new int[percents.Length];
//			colWidths  = SCCorpStyle.calculateColumnWidths(dataGrid1.ClientRectangle.Width, percents, 2);
//			tablestyle = new PPTableStyle(colWidths, this.PreOpInvoked, this.localSystemInfo.systemAccess);
//			this.dataGrid1.TableStyles.Add(tablestyle);//finally attahced the TableStyles to the datagrid
//			this.dataGrid1.DataSource = this.dataview;
		}
		private void applyPPTableStyle()
		{
			int [] colWidths  = new int[percents.Length];
			colWidths  = SCCorpStyle.calculateColumnWidths(dataGrid1.ClientRectangle.Width, percents, 3);
			for(int i = 0;i<colWidths.Length;i++)
			{
				tablestyle.GridColumnStyles[i].Width = colWidths[i];
			}
		}
		private void hideROItemsMI_Click(object sender, System.EventArgs e)
		{
			if(this.hideROItemsMI.Text  == "&Hide read only items")
			{
				this.hideROItemsMI.Text = "&Show read only items";
			}
			else
			{
				this.hideROItemsMI.Text  = "&Hide read only items";
			}
			updateRowFiltering();
		}

		private void updateRowFiltering()
		{
			string sectionFilter = "";
			string accessTypeFilter = "";
			string accessLevelFilter = "";
			#region construct section type filter
			if(this.comboBox1.SelectedItem.ToString() == "Show All")
			{
				//this.dataview.RowFilter = PPCol.sectionType.ToString() + " LIKE '%'";
				sectionFilter = PPCol.sectionType.ToString() + " LIKE '%' ";
			}
			else
			{
				//two switch statements are needed here because the RowFilter is an SQL statment 
				//and so can only contain column name variables. 
				//if and switches needsed to implement non-column RowFilter parameters
				//string Filter = sectionNames_ht[comboBox1.SelectedItem.ToString()].ToString();
				sectionFilter = PPCol.sectionType.ToString() + " = '" + sectionNames_ht[comboBox1.SelectedItem.ToString()].ToString() + "' ";
			}
			#endregion construct section type filter
			if(this.hideROItemsMI.Text  == "&Show read only items") //i.e. we should currently be hiding the ROs
			{
				#region construct access Type filter
				if(this.PreOpInvoked == true)
				{
					accessTypeFilter = " AND ("
						+ PPCol.accessType.ToString()  + " = 'ReadWrite' OR " 
						+ PPCol.accessType.ToString()  + " = 'DWDisplayOnly' OR " 
						+ PPCol.accessType.ToString()  + " = 'ReadReadWrite' OR " 
						+ PPCol.accessType.ToString()  + " = 'ReadWriteWrite' OR " 
						+ PPCol.accessType.ToString()  + " = 'ReadReadWriteInPreOp' OR " 
						+ PPCol.accessType.ToString()  + " = 'ReadWriteInPreOp' OR " 
						+ PPCol.accessType.ToString()  + " = 'ReadWriteWriteInPreOp' OR " 
						+ PPCol.accessType.ToString()  + " = 'WriteOnlyInPreOp' OR " 
						+ PPCol.accessType.ToString()  + " = 'WriteOnly' )";
				}
				else
				{
					accessTypeFilter = " AND "  + PPCol.accessLevel.ToString() + "<= '4' AND ("
						+ PPCol.accessType.ToString()  + " = 'ReadWrite' OR " 
						+ PPCol.accessType.ToString()  + " = 'DWDisplayOnly' OR " 
						+ PPCol.accessType.ToString()  + " = 'ReadReadWrite' OR " 
						+ PPCol.accessType.ToString()  + " = 'ReadWriteWrite' OR " 
						+ PPCol.accessType.ToString()  + " = 'WriteOnly' )";
				}
				#endregion construct access Type filter
				#region construct access Level filter
				switch (localSystemInfo.systemAccess)
				{
					case 5:
						accessLevelFilter = "";
						break;
					case 4:
						accessLevelFilter = " AND "  + PPCol.accessLevel.ToString() + "<= '4' ";
						break;
					case 3:
						accessLevelFilter = " AND "  + PPCol.accessLevel.ToString() + "<= '3' ";
						break;
					case 2:
						accessLevelFilter = " AND "  + PPCol.accessLevel.ToString() + "<= '2' ";
						break;
					case 1: 
						accessLevelFilter = " AND "  + PPCol.accessLevel.ToString() + "<= '1' ";
						break;
				}
				#endregion construct access Level filter
			}
			this.dataview.RowFilter = sectionFilter	+ accessLevelFilter + accessTypeFilter;
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
		/// Clean up any resources being used.
		/// </summary>
		private void PERSONALITY_PARAMS_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			statusbar.Panels[2].Text = "Performing finalisation, please wait";
			#region disable all timers
			this.timer1.Enabled = false;
			this.timer2.Enabled = false;
			#endregion disable all timers
			#region stop all threads
			if(thread != null)
			{
				if((thread.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
				{
					thread.Abort();

					if(thread.IsAlive == true)
					{
#if DEBUG
						Message.Show("Failed to close Thread: " + thread.Name + " on exit");
#endif	
						thread = null;
					}
				}
			}
			#endregion stop all threadss
			e.Cancel = false; //force this window to close
		}
		private void PERSONALITY_PARAMS_WINDOW_Closed(object sender, System.EventArgs e)
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

		#region auto validation
		private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if( (this.localSystemInfo.autoTest != null) && ( AutoValidate.staticValidationRunning == true ) )
			{
				timer2Elapsed();
			}
			else
			{
				timer2.Enabled = false;
			}
		}
		[Conditional ("AUTOVALIDATE")]
		private void timer2Elapsed()
		{
			if ( this.localSystemInfo.autoTest.validationRunning == false )
			{
				return;
			}

			this.timer2.Enabled = false;

			if ( ( timer1.Enabled == false ) && ( localSystemInfo.autoTest.eds != null ) )
			{
				switch ( this.localSystemInfo.autoTest.persParamState )
				{
					case PersParmState.TEST_INIT:
					{
						string abortMessage;

						Debug.WriteLine( "WRITE NEW VALUES TO EACH OBJECT" );
						Debug.WriteLine( "Set each individual index & sub with values of min-1, min, default, max, max+1." );
						Debug.WriteLine( "" );
						Debug.WriteLine( ", index & sub, invalid low value, low value, high value, invalid high value, default" );
						Debug.Flush();
						this.localSystemInfo.autoTest.persParamRow = 0;

						feedback = this.localSystemInfo.forceSystemIntoPreOpMode( out abortMessage);

						if ( feedback != DIFeedbackCode.DISuccess )
						{
							Debug.WriteLine( ",Failed to put controller in pre-op. Test abandoned." );
							this.localSystemInfo.autoTest.persParamState = PersParmState.END_TEST;
						}
						else
						{
							this.localSystemInfo.autoTest.persParamState = PersParmState.ENTER_INVALID_LOW_VALUE;
						}
						break;
					}

					case PersParmState.ENTER_INVALID_LOW_VALUE:
					{
						this.timer2.Interval = 100;
						setValue();

						if ( this.localSystemInfo.autoTest.invalidValue == true )
						{
							this.localSystemInfo.autoTest.persParamState = PersParmState.SELECT_ITEM_FOR_TEST;
						}
						else
						{
							this.localSystemInfo.autoTest.persParamState = PersParmState.ENTER_LOWEST_VALUE;
						}
						break;
					}

					case PersParmState.ENTER_LOWEST_VALUE:
					{
						if ( this.comboBox1.Visible == false )
						{
							return;
						}
						else
						{
							setValue();
							this.localSystemInfo.autoTest.persParamState = PersParmState.ENTER_HIGHEST_VALUE;
						}
						break;
					}

					case PersParmState.ENTER_DEFAULT_VALUE:
					{
						if ( this.comboBox1.Visible == false )
						{
							return;
						}
						else
						{
							setValue();
							this.localSystemInfo.autoTest.persParamState = PersParmState.SELECT_ITEM_FOR_TEST;
						}
						break;
					}

					case PersParmState.ENTER_HIGHEST_VALUE:
					{
						if ( this.comboBox1.Visible == false )
						{
							return;
						}
						else
						{
							setValue();
							this.localSystemInfo.autoTest.persParamState = PersParmState.ENTER_INVALID_HIGH_VALUE;
						}
						break;
					}

					case PersParmState.ENTER_INVALID_HIGH_VALUE:
					{
						if ( this.comboBox1.Visible == false )
						{
							return;
						}
						else
						{
							setValue();
							this.localSystemInfo.autoTest.persParamState = PersParmState.ENTER_DEFAULT_VALUE;
						}
						break;
					}

					case PersParmState.SELECT_ITEM_FOR_TEST:
					{
						if ( this.localSystemInfo.autoTest.persParamRow < ( this.table.Rows.Count - 1 ) )
						{
							this.localSystemInfo.autoTest.persParamRow++;
							this.localSystemInfo.autoTest.persParamState = PersParmState.ENTER_INVALID_LOW_VALUE;
						}
						else
						{
							this.localSystemInfo.autoTest.persParamState = PersParmState.END_TEST;
						}
						break;
					}

					case PersParmState.END_TEST:
					{
						this.timer2.Enabled = false;
						Debug.WriteLine( "Close personality window." );
						Debug.WriteLine( "" );
						this.closeBtn.Visible = true;
						this.closeBtn.Focus();
						this.closeBtn.PerformClick();
						this.localSystemInfo.autoTest.testState = ValidateState.MAIN_WINDOW_PERS_PARM_COMPLETED;
						this.localSystemInfo.autoTest.persParamState++;
						break;
					}

					case PersParmState.WAIT_FOR_WINDOW_TO_CLOSE:
					{
						break;
					}
				}
			}

			if ( this.localSystemInfo.autoTest.persParamState != PersParmState.WAIT_FOR_WINDOW_TO_CLOSE )
			{
				this.timer2.Enabled = true;
			}
		}

		[Conditional ("AUTOVALIDATE")]
		private void setValue()
		{
			TestDetails details = new TestDetails();
			float scaledValue;
			long testValue;
			int persParamRow = this.localSystemInfo.autoTest.persParamRow;

			string indexString = this.dataGrid1[persParamRow,0].ToString();
			string subString = this.dataGrid1[persParamRow,1].ToString();
			long indexAndSub = System.Convert.ToInt32( indexString, 16 );
			this.localSystemInfo.autoTest.invalidValue = false;

			// Don't test PDO mappings (rx)
			if ( (indexAndSub >= 0x1600) && (indexAndSub <= 0x1608) )
			{
				this.localSystemInfo.autoTest.invalidValue = true;
				return;
			}

			// Don't test PDO mappings (tx)
			if ( (indexAndSub >= 0x1A00) && (indexAndSub <= 0x1A08) )
			{
				this.localSystemInfo.autoTest.invalidValue = true;
				return;
			}

			indexAndSub = ( indexAndSub << 16 );

			if ( subString != "" )
			{
				indexAndSub += System.Convert.ToInt32( subString, 16 );
			}

			// Don't test the following because they mess up the controller & the auto
			// test cannot continue.
			if 
				( 
				(indexAndSub == 0x58000000)			// master/slave selection
				|| (indexAndSub == 0x28000000)		// force into pre-op
				|| (indexAndSub == 0x59000001)		// node ID
				|| (indexAndSub == 0x54000000)		// force into bootloader mode
				|| (indexAndSub == 0x50000002)		// user ID for login
				|| (indexAndSub == 0x50000003)		// user password for login
				|| (indexAndSub == 0x28500001)		// force node reset
				)
			{
				this.localSystemInfo.autoTest.invalidValue = true;
				return;
			}
							
			int item = this.localSystemInfo.autoTest.eds.IndexOfKey( indexAndSub );

			if ( item != -1 )
			{
				details = (TestDetails)this.localSystemInfo.autoTest.eds.GetByIndex( item );

				details.accessType = details.accessType.ToUpper();
				details.accessType = details.accessType.Trim();

				if ( details.accessType == "RO" )
				{
					this.localSystemInfo.autoTest.invalidValue = true;
					return;
				}

				switch ( this.localSystemInfo.autoTest.persParamState )
				{
					case PersParmState.ENTER_INVALID_LOW_VALUE:
					{
						Debug.WriteLine( "" );
						Debug.Write( "," + indexString + " sub:" + subString );
						testValue = details.minValue - 1;

						if ( details.enumType == true )
						{
							Debug.Write( ",-" );
						}
						else
						{
							if ( details.scaling != 1.0F )
							{
								scaledValue = (System.UInt64)testValue * details.scaling;
								Debug.Write( "," + scaledValue.ToString() );
								this.dataGrid1[persParamRow,7] = scaledValue.ToString();
							}
							else
							{
								Debug.Write( "," + testValue.ToString("X") );
								this.dataGrid1[persParamRow,7] = "0x" + testValue.ToString("X");
							}
						}
						
						break;
					}

					case PersParmState.ENTER_LOWEST_VALUE:
					{
						testValue = details.minValue;

						if ( details.scaling != 1.0F )
						{
							scaledValue = testValue * details.scaling;
							Debug.Write( "," + scaledValue.ToString() );
							this.dataGrid1[persParamRow,7] = scaledValue.ToString();
						}
						else
						{
							Debug.Write( "," + testValue.ToString("X") );
							this.dataGrid1[persParamRow,7] = "0x" + testValue.ToString("X");
						}
						break;
					}

					case PersParmState.ENTER_HIGHEST_VALUE:
					{
						testValue = details.maxValue;

						if ( details.scaling != 1.0F )
						{
							scaledValue = testValue * details.scaling;
							Debug.Write( "," + scaledValue.ToString() );
							this.dataGrid1[persParamRow,7] = scaledValue.ToString();
						}
						else
						{
							Debug.Write( "," + testValue.ToString("X") );
							this.dataGrid1[persParamRow,7] = "0x" + testValue.ToString("X");
						}
						break;
					}

					case PersParmState.ENTER_INVALID_HIGH_VALUE:
					{
						testValue = details.maxValue + 1;

						if ( details.enumType == true )
						{
							Debug.Write( ",-" );
						}
						else
						{
							if ( details.scaling != 1.0F )
							{
								scaledValue = testValue * details.scaling;
								Debug.Write( "," + scaledValue.ToString() );
								this.dataGrid1[persParamRow,7] = scaledValue.ToString();
							}
							else
							{
								Debug.Write( "," + testValue.ToString("X") );
								this.dataGrid1[persParamRow,7] = "0x" + testValue.ToString("X");
							}
						}
						break;
					}

					case PersParmState.ENTER_DEFAULT_VALUE:
					{
						testValue = details.defaultValue;

						if ( details.scaling != 1.0F )
						{
							scaledValue = testValue * details.scaling;
							Debug.Write( "," + scaledValue.ToString() );
							this.dataGrid1[persParamRow,7] = scaledValue.ToString();
						}
						else
						{
							Debug.Write( "," + testValue.ToString("X") );
							this.dataGrid1[persParamRow,7] = "0x" + testValue.ToString("X");
						}
						break;
					}
				}
			}

			// Simulating submit button press
			this.submitBtn.Focus();
			this.submitBtn.PerformClick();
		}
		#endregion

		private void toolbar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
		{
			string abortMessage = "";
			#region update the values
			long myValueForceToPReOP = 0, myValueNMTState = 0;
			for(int i = 0;i<this.table.Rows.Count;i++)
			{
				if(table.Rows[i][(int) PPCol.objectType].ToString() == DriveWizard.SevconObjectType.FORCE_TO_PREOP.ToString())
				{
					feedback = this.localSystemInfo.readODItemValue(this.selectednodeID, SevconObjectType.FORCE_TO_PREOP, 0, out myValueForceToPReOP, out abortMessage);
					if(feedback == DIFeedbackCode.DISuccess)
					{
						string formatStr = table.Rows[i][(int) (PPCol.numberFormat)].ToString();
						string testing = this.getEnumValue(formatStr,myValueForceToPReOP);  //it is enumerated type
						this.table.Rows[i][(int)(PPCol.actValue)] = this.getEnumValue(formatStr,myValueForceToPReOP);  //it is enumerated type
					}
				}
				else if (table.Rows[i][(int) PPCol.objectType].ToString() == DriveWizard.SevconObjectType.NMT_STATE.ToString())
				{
					feedback = this.localSystemInfo.readODItemValue(this.selectednodeID, SevconObjectType.NMT_STATE, 0, out myValueNMTState, out abortMessage);
					if(feedback == DIFeedbackCode.DISuccess)
					{
						//do this here because the node NOde State has not yet caught up
						if(myValueNMTState == SCCorpStyle.NMTState.PreOperational.GetHashCode())
						{
							this.PreOpInvoked = true;
						}
						else
						{
							this.PreOpInvoked = false;
						}
#if GUIMOCKUP
						if ( MAIN_WINDOW.MockUpOffline == true )
						{
							if (e.Button == this.toolbar.Buttons[0])
							{
this.PreOpInvoked = true;
							}
							else if (e.Button == this.toolbar.Buttons[1])
							{
								this.PreOpInvoked = false;
							}
						}
#endif
						string formatStr = table.Rows[i][(int) (PPCol.numberFormat)].ToString();
						string test = this.getEnumValue(formatStr,myValueNMTState);  //it is enumerated type
						this.table.ColumnChanged -= new System.Data.DataColumnChangeEventHandler(this.table_ColumnChanged);
						this.table.Rows[i][(int)(PPCol.actValue)] = this.getEnumValue(formatStr,myValueNMTState);  //it is enumerated type
						this.table.ColumnChanged += new System.Data.DataColumnChangeEventHandler(this.table_ColumnChanged);
					}
				}
			}
			#endregion update the values
			colArray = new Color[this.dataview.Count]; 
			colArray = SCCorpStyle.ApplyRowColours(this.dataview, this.PreOpInvoked, this.localSystemInfo.systemAccess);
			this.dataGrid1.Invalidate();
			this.createPPTableStyle();  //force pre-op to be updated
		}
	}

	#endregion PERSONALITY_PARAMS_WINDOW class	

	#region DataTable for Personality Parameter setting (text)
	public class PPDataTable : DataTable
	{
		public PPDataTable()
		{
			this.Columns.Add(PPCol.Index.ToString(),typeof(System.String));
			this.Columns.Add(PPCol.sub.ToString(),typeof(System.String));
			this.Columns.Add(PPCol.param.ToString(), typeof(System.String));
			this.Columns.Add(PPCol.PDOMap.ToString(),typeof(System.String));
			this.Columns.Add(PPCol.defVal.ToString(), typeof(System.String));
			this.Columns.Add(PPCol.lowVal.ToString(),  typeof(System.String));
			this.Columns.Add(PPCol.highVal.ToString(),typeof(System.String));
			this.Columns.Add(PPCol.actValue.ToString(),typeof(System.String));
			this.Columns.Add(PPCol.units.ToString(), typeof(System.String));
			this.Columns.Add(PPCol.accessType.ToString(), typeof(System.String)); //non visible column
			this.Columns.Add(PPCol.accessLevel.ToString(), typeof(System.String)); //non visible column
			this.Columns.Add(PPCol.displayType.ToString(), typeof(DriveWizard.CANopenDataType)); //non visible column
			this.Columns.Add(PPCol.sectionType.ToString(), typeof(System.String)); //use string for combo boxes to work properly
			this.Columns.Add(PPCol.objectType.ToString(), typeof(System.String)); //use string for combo boxes to work properly
			this.Columns.Add(PPCol.scaling.ToString(), typeof(System.String)); //non visible column
			this.Columns.Add(PPCol.fullValue.ToString(), typeof(System.String)); //non visible column - used to store non-truncated controller value
			this.Columns.Add(PPCol.fullMax.ToString(), typeof(System.String));  //needed because we can't convert a string back to a float
			this.Columns.Add(PPCol.fullMin.ToString(), typeof(System.String)); //ditto
			this.Columns.Add(PPCol.format.ToString(), typeof(DriveWizard.SevconNumberFormat));
			this.Columns.Add(PPCol.numberFormat.ToString(), typeof(System.String));
			DataColumn[] keys = new DataColumn[2];
			keys[0] = this.Columns[0];
			keys[1] = this.Columns[1];
			this.PrimaryKey = keys;

		}
	}
	#endregion

	#region PP Formattable TextBox Column
	//This class is overridden to allow both text box and Combo box style entry of values. 
	//Combo box is suitable for enumerated types
	// text entry is used for all others
	public class PPFormattableTextBoxColumn : DataGridTextBoxColumn
	{
		private ComboBox enumCombo = null;
		private bool _bIsComboBound = false; // remember if combobox is bound to datagrid
		private int _iRowNum = 0;  //the row number
		private CurrencyManager _cmSource;
		public bool _preOpInvoked = false;
		private bool _readOnlyColumn = false;
		private uint _systemAccess = 0;
		public PPFormattableTextBoxColumn(bool PreOpInvoked, uint systemAccess, bool readOnlyColumn)
		{
			_preOpInvoked = PreOpInvoked;
			_systemAccess = systemAccess;
			_readOnlyColumn = readOnlyColumn;
			if(_readOnlyColumn == false)
			{
				enumCombo = new ComboBox();
				enumCombo.DropDownStyle = ComboBoxStyle.DropDownList;
				enumCombo.Visible = false;
			}
		}
		public PPFormattableTextBoxColumn()
		{
		}
		protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
		{
			if(DriveWizard.PERSONALITY_PARAMS_WINDOW.colArray != null)
			{
				foreBrush = new SolidBrush(DriveWizard.PERSONALITY_PARAMS_WINDOW.colArray[rowNum]);
				if(DriveWizard.PERSONALITY_PARAMS_WINDOW.colArray[rowNum].ToString() == SCCorpStyle.headerRow.ToString())
				{
					backBrush = new SolidBrush(SCCorpStyle.headerRowBackcol);
				}
				else if (DriveWizard.PERSONALITY_PARAMS_WINDOW.colArray[rowNum].ToString() == SCCorpStyle.readOnly.ToString())
				{
					backBrush = new SolidBrush(SCCorpStyle.readOnlyRowBackCol);
				}
			}
			base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			DataRowView myView = (DataRowView) source.Current;
			_iRowNum = rowNum;
			_cmSource = source;
			if(this._readOnlyColumn == true)  //not the actual value column 
			{
				return;
			}
			#region first check if a DOMAIN entry was clicked
			//			if(myView.Row[(int) PPCol.actValue].ToString().ToUpper() == "DOMAIN")
			//			{
			//				if(myView.Row[(int) PPCol.sectionType].ToString() == DriveWizard.SevconSectionType.NODE_STATUS.ToString())
			//				{
			//					//subs are fixed so we can reference them directly
			//					uint sub = System.Convert.ToUInt32(myView.Row[(int) PPCol.sub]);
			//						string userMessage = "";
			//					switch(sub)
			//					{
			//						case 1:
			//						userMessage = "Do you want to view the System Log?";
			//							break;
			//
			//						case 2:
			//						case 6:
			//							userMessage = "Do you want to view Fault Log data?";
			//							break;
			//
			//						case 3:
			//							userMessage = "Do you want to view the Event Counters log?";
			//							break;
			//
			//						case 4:
			//						case 5:
			//							userMessage = "Do you want to view the Operational Logs?";
			//							break;
			//
			//						default:
			//							break;
			//					}
			//					if(userMessage != "")
			//					{
			//						Form f = this.DataGridTableStyle.DataGrid.FindForm();
			//						string message = userMessage + "\nClose this window (changes will be lost) and view sleected log?. Are you sure?";
			//						DialogResult result = Message.Show(f, message, "Confirm", MessageBoxButtons.YesNo,
			//							MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
			//						if(result == DialogResult.No)
			//						{
			//							DriveWizard.PERSONALITY_PARAMS_WINDOW.DomainRequest = "";
			//							//do nothing 
			//						}
			//						else if(result == DialogResult.Yes)
			//						{
			//							return;
			//						}
			//					}
			//				}
			//			}
			#endregion first check if a DOMAIN entry was clicked
			#region detect any cell that is currently read only to the user and if so return to prevent cell editing
			ObjectAccessType accessType = (ObjectAccessType)Enum.Parse(typeof(ObjectAccessType), (myView.Row[(int) (PPCol.accessType)].ToString()));
			uint objectAccess = System.Convert.ToUInt32(myView.Row[(int) PPCol.accessLevel]);
			object tempObject = this.GetColumnValueAtRow(source, rowNum);

			#region switch accessType
			switch (accessType)
			{
				case ObjectAccessType.ReadReadWriteInPreOp:
				case ObjectAccessType.ReadWriteInPreOp:
				case ObjectAccessType.ReadWriteWriteInPreOp:
					//check here whether the node is in pre-op 
					if(_preOpInvoked == false) //cannot wirte to these unless we are in pre-op
					{
						return;  
					}
					else if(objectAccess > this._systemAccess)  //user does not have enough access
					{
						return;
					}
					break;

				case ObjectAccessType.ReadReadWrite:
				case ObjectAccessType.ReadWrite:
				case ObjectAccessType.ReadWriteWrite:
					if(objectAccess > this._systemAccess)  //user does not have enough access
					{
						return;
					}
					break;

				default:  
					return;  //any other types do not allow user access - return here to preven tany text entry in cell
			}
			#endregion switch accessType
			#endregion detect any cell that is currently read only to the user and if so return to prevent cell editing

			CANopenDataType dataType = (CANopenDataType)Enum.Parse(typeof(CANopenDataType), (myView.Row[(int) PPCol.displayType].ToString()));
			if((myView.Row[(int) PPCol.format].ToString() == "2") || (dataType == CANopenDataType.BOOLEAN))
			{
				#region handle enumerated types
				this.ReadOnly = true;  //make this column at this row read only since we are placing combo on top
				#region one time only settings
				// navigation path to the datagrid only exists after the column is added to the Styles
				if (_bIsComboBound == false) 
				{
					_bIsComboBound = true; //set the indicator 
					this.DataGridTableStyle.DataGrid.Controls.Add(enumCombo); // and bind combo to its datagrid 
					enumCombo.Leave +=new EventHandler(enumCombo_Leave); 
					enumCombo.SelectionChangeCommitted +=new EventHandler(enumCombo_SelectionChangeCommitted);
				}
				#endregion one time only settings
				#region create combo drop down list for this item
				string wholeStr = myView.Row[(int) PPCol.numberFormat].ToString();
				string [] enumStrs = wholeStr.Split(':');
				ushort [] enumValues = new ushort[enumStrs.Length];
				ArrayList comboData = new ArrayList()    ;
				for (int i = 0;i<enumValues.Length;i++)
				{
					string textOnly = enumStrs[i].Remove(enumStrs[i].Length-5, 5);  //remove the 'value' part of the enum string
					comboData.Add(new comboSource(textOnly, (ushort) i));
				}

				enumCombo.DataSource = comboData;
				enumCombo.DisplayMember = "enumStr";
				enumCombo.ValueMember = "enumValue";
				#endregion create combo drop down list for this item
				enumCombo.Font = this.TextBox.Font;				// synchronize the font size to the text box
				enumCombo.Bounds = bounds;  //size combobox to current grid cell size
				enumCombo.BeginUpdate();  //suspend cell painting
				enumCombo.Visible = true;
				enumCombo.SelectedIndex = 0;
				enumCombo.EndUpdate(); //resume cell painting
				enumCombo.Focus();
				#endregion handle enumerated types
			}
			else  //not boolean or enumerated
			{
				this.ReadOnly = false;
				base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
			}
		}

		private void enumCombo_Leave(object sender, EventArgs e)
		{
			enumCombo.Visible = false;
		}

		private void enumCombo_SelectionChangeCommitted(object sender, EventArgs e)
		{
			#region extract correct text value
			ArrayList _comboData = new ArrayList();
			_comboData = (ArrayList) enumCombo.DataSource;
			object objValue = _comboData[enumCombo.SelectedIndex].ToString();
			#endregion extract correct text value
			this.SetColumnValueAtRow(_cmSource, _iRowNum, objValue);
			enumCombo.Visible = false;
		}
	}
	#endregion

	#region combo class
	public class comboSource
	{
		private string _enumStr ;
		private ushort _enumValue ;
    
		public  comboSource(string enumStr, ushort enumValue)
		{
			this._enumStr = enumStr;
			this._enumValue = enumValue;
		}
		public string enumStr
		{
			get
			{
				return _enumStr;
			}
		}

		public ushort enumValue
		{
        
			get
			{
				return _enumValue ;
			}
		}

		public override string ToString()
		{
			return this.enumStr;
		}
	}

	#endregion combo class
}
