/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.22$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:13:18$
	$ModDate:05/12/2007 22:11:00$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION

REFERENCES    

MODIFICATION HISTORY
    $Log:  48688: VPDOs.cs 

   Rev 1.22    05/12/2007 22:13:18  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Drawing.Drawing2D;

namespace DriveWizard
{
		/// <summary>
	/// Summary description for VPDOs.
	/// </summary>
	public class VPDOs : System.Windows.Forms.Form
	{
		public enum tabControls { DigIPs, AlgIPs, DigOPs, AlgOPs, Motor};

		#region form controls
		private System.Windows.Forms.Button closeBtn;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage DigitalInputs;
		private System.Windows.Forms.TabPage DigitalOutputs;
		private System.Windows.Forms.TabPage AnalogueInputs;
		private System.Windows.Forms.TabPage AnalogueOutputs;
		private System.Windows.Forms.TabPage Motor;
		private System.Windows.Forms.TreeView DigIPs_TV;
		private System.Windows.Forms.TreeView DigOPs_TV;
		private System.Windows.Forms.TreeView AnlgIPs_TV;
		private System.Windows.Forms.TreeView AnlgOPs_TV;
		private System.Windows.Forms.TreeView MotorSigs_TV;
		private System.Windows.Forms.ListView DigitalIPS_LV;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Splitter splitter2;
		private System.Windows.Forms.Splitter splitter3;
		private System.Windows.Forms.Splitter splitter4;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.Splitter splitter5;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ListView DigitalOPs_LV;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ListView AnalogIPs_LV;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ListView AnalogOPs_LV;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ListView motor_LV;
		private System.Windows.Forms.Label label1;
		private System.ComponentModel.IContainer components;
		#endregion form controls

		#region my definitions
		private SystemInfo localSystemInfo;
		private string selectedNodeText = "";
		private int nodeIndex = 0;
		private DIFeedbackCode feedback;
		private Hashtable digIPS_HT = new Hashtable();
		private Hashtable digOPs_HT = new Hashtable();
		private Hashtable analIps_HT = new Hashtable();
		private Hashtable analOPs_HT = new Hashtable();
		private Hashtable motor_HT = new Hashtable();
		private long maxDigitalInputs = 0, MaxDigitalOPs = 0, MaxAnalogueIPs = 0, MaxAnalogueOutputs = 0, MaxMotorItems = 0;
		private ListViewItem MouseOverLVItem = null, selectedItemForContextMenu = null;
		private Graphics gLV = null, gTV = null;
		private Font shadowFont;
		private string shadowText = "";
		private Size dragStringSize;
		private Rectangle removeDragStringRectLV = new Rectangle(0,0,0,0);
		private Rectangle removeDragStringRectTV = new Rectangle(0,0,0,0);
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.ContextMenu contextMenu1;
		ToolBar toolbar = null;
		private System.Windows.Forms.Timer timer1;
		Thread ODDataRetrievalThread;
		ProgressBar progressbar;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.ImageList VPDOIcons;
		string errorMessage = "";
		#endregion my definitions

		#region Initialisation
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(VPDOs));
			this.closeBtn = new System.Windows.Forms.Button();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.DigitalInputs = new System.Windows.Forms.TabPage();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.DigitalIPS_LV = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.VPDOIcons = new System.Windows.Forms.ImageList(this.components);
			this.DigIPs_TV = new System.Windows.Forms.TreeView();
			this.AnalogueInputs = new System.Windows.Forms.TabPage();
			this.AnalogIPs_LV = new System.Windows.Forms.ListView();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.splitter4 = new System.Windows.Forms.Splitter();
			this.AnlgIPs_TV = new System.Windows.Forms.TreeView();
			this.DigitalOutputs = new System.Windows.Forms.TabPage();
			this.DigitalOPs_LV = new System.Windows.Forms.ListView();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.DigOPs_TV = new System.Windows.Forms.TreeView();
			this.AnalogueOutputs = new System.Windows.Forms.TabPage();
			this.AnalogOPs_LV = new System.Windows.Forms.ListView();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.splitter3 = new System.Windows.Forms.Splitter();
			this.AnlgOPs_TV = new System.Windows.Forms.TreeView();
			this.Motor = new System.Windows.Forms.TabPage();
			this.motor_LV = new System.Windows.Forms.ListView();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.splitter5 = new System.Windows.Forms.Splitter();
			this.MotorSigs_TV = new System.Windows.Forms.TreeView();
			this.label1 = new System.Windows.Forms.Label();
			this.submitBtn = new System.Windows.Forms.Button();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.tabControl1.SuspendLayout();
			this.DigitalInputs.SuspendLayout();
			this.AnalogueInputs.SuspendLayout();
			this.DigitalOutputs.SuspendLayout();
			this.AnalogueOutputs.SuspendLayout();
			this.Motor.SuspendLayout();
			this.SuspendLayout();
			// 
			// closeBtn
			// 
			this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.closeBtn.Location = new System.Drawing.Point(608, 376);
			this.closeBtn.Name = "closeBtn";
			this.closeBtn.Size = new System.Drawing.Size(144, 25);
			this.closeBtn.TabIndex = 17;
			this.closeBtn.Text = "&Close window";
			this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabControl1.Controls.Add(this.DigitalInputs);
			this.tabControl1.Controls.Add(this.AnalogueInputs);
			this.tabControl1.Controls.Add(this.DigitalOutputs);
			this.tabControl1.Controls.Add(this.AnalogueOutputs);
			this.tabControl1.Controls.Add(this.Motor);
			this.tabControl1.Location = new System.Drawing.Point(0, 64);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(760, 312);
			this.tabControl1.TabIndex = 25;
			this.tabControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
			// 
			// DigitalInputs
			// 
			this.DigitalInputs.Controls.Add(this.splitter1);
			this.DigitalInputs.Controls.Add(this.DigitalIPS_LV);
			this.DigitalInputs.Controls.Add(this.DigIPs_TV);
			this.DigitalInputs.Location = new System.Drawing.Point(4, 28);
			this.DigitalInputs.Name = "DigitalInputs";
			this.DigitalInputs.Size = new System.Drawing.Size(752, 280);
			this.DigitalInputs.TabIndex = 0;
			this.DigitalInputs.Text = "Digital Inputs";
			// 
			// splitter1
			// 
			this.splitter1.BackColor = System.Drawing.Color.Black;
			this.splitter1.Location = new System.Drawing.Point(280, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(4, 280);
			this.splitter1.TabIndex = 32;
			this.splitter1.TabStop = false;
			// 
			// DigitalIPS_LV
			// 
			this.DigitalIPS_LV.AllowDrop = true;
			this.DigitalIPS_LV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.DigitalIPS_LV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.columnHeader1});
			this.DigitalIPS_LV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DigitalIPS_LV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.DigitalIPS_LV.LabelWrap = false;
			this.DigitalIPS_LV.LargeImageList = this.VPDOIcons;
			this.DigitalIPS_LV.Location = new System.Drawing.Point(280, 0);
			this.DigitalIPS_LV.MultiSelect = false;
			this.DigitalIPS_LV.Name = "DigitalIPS_LV";
			this.DigitalIPS_LV.Size = new System.Drawing.Size(472, 280);
			this.DigitalIPS_LV.SmallImageList = this.VPDOIcons;
			this.DigitalIPS_LV.StateImageList = this.VPDOIcons;
			this.DigitalIPS_LV.TabIndex = 31;
			this.DigitalIPS_LV.View = System.Windows.Forms.View.List;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Mappable DIgital Inputs";
			this.columnHeader1.Width = 472;
			// 
			// VPDOIcons
			// 
			this.VPDOIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.VPDOIcons.ImageSize = new System.Drawing.Size(24, 24);
			this.VPDOIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("VPDOIcons.ImageStream")));
			this.VPDOIcons.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// DigIPs_TV
			// 
			this.DigIPs_TV.AllowDrop = true;
			this.DigIPs_TV.Dock = System.Windows.Forms.DockStyle.Left;
			this.DigIPs_TV.ImageList = this.VPDOIcons;
			this.DigIPs_TV.ItemHeight = 24;
			this.DigIPs_TV.Location = new System.Drawing.Point(0, 0);
			this.DigIPs_TV.Name = "DigIPs_TV";
			this.DigIPs_TV.Size = new System.Drawing.Size(280, 280);
			this.DigIPs_TV.TabIndex = 30;
			// 
			// AnalogueInputs
			// 
			this.AnalogueInputs.Controls.Add(this.AnalogIPs_LV);
			this.AnalogueInputs.Controls.Add(this.splitter4);
			this.AnalogueInputs.Controls.Add(this.AnlgIPs_TV);
			this.AnalogueInputs.Location = new System.Drawing.Point(4, 28);
			this.AnalogueInputs.Name = "AnalogueInputs";
			this.AnalogueInputs.Size = new System.Drawing.Size(752, 280);
			this.AnalogueInputs.TabIndex = 2;
			this.AnalogueInputs.Text = "Analogue Inputs";
			// 
			// AnalogIPs_LV
			// 
			this.AnalogIPs_LV.AllowDrop = true;
			this.AnalogIPs_LV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.AnalogIPs_LV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						   this.columnHeader5});
			this.AnalogIPs_LV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AnalogIPs_LV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.AnalogIPs_LV.LabelWrap = false;
			this.AnalogIPs_LV.LargeImageList = this.VPDOIcons;
			this.AnalogIPs_LV.Location = new System.Drawing.Point(284, 0);
			this.AnalogIPs_LV.MultiSelect = false;
			this.AnalogIPs_LV.Name = "AnalogIPs_LV";
			this.AnalogIPs_LV.Size = new System.Drawing.Size(468, 280);
			this.AnalogIPs_LV.SmallImageList = this.VPDOIcons;
			this.AnalogIPs_LV.StateImageList = this.VPDOIcons;
			this.AnalogIPs_LV.TabIndex = 32;
			this.AnalogIPs_LV.View = System.Windows.Forms.View.List;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Mappable DIgital Inputs";
			this.columnHeader5.Width = 468;
			// 
			// splitter4
			// 
			this.splitter4.BackColor = System.Drawing.Color.Black;
			this.splitter4.Location = new System.Drawing.Point(280, 0);
			this.splitter4.Name = "splitter4";
			this.splitter4.Size = new System.Drawing.Size(4, 280);
			this.splitter4.TabIndex = 30;
			this.splitter4.TabStop = false;
			// 
			// AnlgIPs_TV
			// 
			this.AnlgIPs_TV.AllowDrop = true;
			this.AnlgIPs_TV.Dock = System.Windows.Forms.DockStyle.Left;
			this.AnlgIPs_TV.ImageList = this.VPDOIcons;
			this.AnlgIPs_TV.Location = new System.Drawing.Point(0, 0);
			this.AnlgIPs_TV.Name = "AnlgIPs_TV";
			this.AnlgIPs_TV.Size = new System.Drawing.Size(280, 280);
			this.AnlgIPs_TV.TabIndex = 28;
			// 
			// DigitalOutputs
			// 
			this.DigitalOutputs.Controls.Add(this.DigitalOPs_LV);
			this.DigitalOutputs.Controls.Add(this.splitter2);
			this.DigitalOutputs.Controls.Add(this.DigOPs_TV);
			this.DigitalOutputs.Location = new System.Drawing.Point(4, 28);
			this.DigitalOutputs.Name = "DigitalOutputs";
			this.DigitalOutputs.Size = new System.Drawing.Size(752, 280);
			this.DigitalOutputs.TabIndex = 1;
			this.DigitalOutputs.Text = "Low current outputs";
			// 
			// DigitalOPs_LV
			// 
			this.DigitalOPs_LV.AllowDrop = true;
			this.DigitalOPs_LV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.DigitalOPs_LV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.columnHeader3});
			this.DigitalOPs_LV.ContextMenu = this.contextMenu1;
			this.DigitalOPs_LV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DigitalOPs_LV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.DigitalOPs_LV.LabelWrap = false;
			this.DigitalOPs_LV.LargeImageList = this.VPDOIcons;
			this.DigitalOPs_LV.Location = new System.Drawing.Point(284, 0);
			this.DigitalOPs_LV.MultiSelect = false;
			this.DigitalOPs_LV.Name = "DigitalOPs_LV";
			this.DigitalOPs_LV.Size = new System.Drawing.Size(468, 280);
			this.DigitalOPs_LV.SmallImageList = this.VPDOIcons;
			this.DigitalOPs_LV.StateImageList = this.VPDOIcons;
			this.DigitalOPs_LV.TabIndex = 32;
			this.DigitalOPs_LV.View = System.Windows.Forms.View.List;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Mappable DIgital Inputs";
			this.columnHeader3.Width = 468;
			// 
			// splitter2
			// 
			this.splitter2.BackColor = System.Drawing.Color.Black;
			this.splitter2.Location = new System.Drawing.Point(280, 0);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new System.Drawing.Size(4, 280);
			this.splitter2.TabIndex = 30;
			this.splitter2.TabStop = false;
			// 
			// DigOPs_TV
			// 
			this.DigOPs_TV.AllowDrop = true;
			this.DigOPs_TV.Dock = System.Windows.Forms.DockStyle.Left;
			this.DigOPs_TV.ImageList = this.VPDOIcons;
			this.DigOPs_TV.Location = new System.Drawing.Point(0, 0);
			this.DigOPs_TV.Name = "DigOPs_TV";
			this.DigOPs_TV.Size = new System.Drawing.Size(280, 280);
			this.DigOPs_TV.TabIndex = 28;
			// 
			// AnalogueOutputs
			// 
			this.AnalogueOutputs.Controls.Add(this.AnalogOPs_LV);
			this.AnalogueOutputs.Controls.Add(this.splitter3);
			this.AnalogueOutputs.Controls.Add(this.AnlgOPs_TV);
			this.AnalogueOutputs.Location = new System.Drawing.Point(4, 28);
			this.AnalogueOutputs.Name = "AnalogueOutputs";
			this.AnalogueOutputs.Size = new System.Drawing.Size(752, 280);
			this.AnalogueOutputs.TabIndex = 3;
			this.AnalogueOutputs.Text = "Analogue Outputs";
			// 
			// AnalogOPs_LV
			// 
			this.AnalogOPs_LV.AllowDrop = true;
			this.AnalogOPs_LV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.AnalogOPs_LV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						   this.columnHeader4});
			this.AnalogOPs_LV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AnalogOPs_LV.GridLines = true;
			this.AnalogOPs_LV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.AnalogOPs_LV.LabelWrap = false;
			this.AnalogOPs_LV.LargeImageList = this.VPDOIcons;
			this.AnalogOPs_LV.Location = new System.Drawing.Point(284, 0);
			this.AnalogOPs_LV.MultiSelect = false;
			this.AnalogOPs_LV.Name = "AnalogOPs_LV";
			this.AnalogOPs_LV.Size = new System.Drawing.Size(468, 280);
			this.AnalogOPs_LV.SmallImageList = this.VPDOIcons;
			this.AnalogOPs_LV.StateImageList = this.VPDOIcons;
			this.AnalogOPs_LV.TabIndex = 32;
			this.AnalogOPs_LV.View = System.Windows.Forms.View.List;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Mappable DIgital Inputs";
			this.columnHeader4.Width = 468;
			// 
			// splitter3
			// 
			this.splitter3.BackColor = System.Drawing.Color.Black;
			this.splitter3.Location = new System.Drawing.Point(280, 0);
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = new System.Drawing.Size(4, 280);
			this.splitter3.TabIndex = 30;
			this.splitter3.TabStop = false;
			// 
			// AnlgOPs_TV
			// 
			this.AnlgOPs_TV.AllowDrop = true;
			this.AnlgOPs_TV.Dock = System.Windows.Forms.DockStyle.Left;
			this.AnlgOPs_TV.ImageList = this.VPDOIcons;
			this.AnlgOPs_TV.Location = new System.Drawing.Point(0, 0);
			this.AnlgOPs_TV.Name = "AnlgOPs_TV";
			this.AnlgOPs_TV.Size = new System.Drawing.Size(280, 280);
			this.AnlgOPs_TV.TabIndex = 28;
			// 
			// Motor
			// 
			this.Motor.Controls.Add(this.motor_LV);
			this.Motor.Controls.Add(this.splitter5);
			this.Motor.Controls.Add(this.MotorSigs_TV);
			this.Motor.Location = new System.Drawing.Point(4, 28);
			this.Motor.Name = "Motor";
			this.Motor.Size = new System.Drawing.Size(752, 280);
			this.Motor.TabIndex = 4;
			this.Motor.Text = "Motor";
			// 
			// motor_LV
			// 
			this.motor_LV.AllowDrop = true;
			this.motor_LV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.motor_LV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader2});
			this.motor_LV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.motor_LV.GridLines = true;
			this.motor_LV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.motor_LV.LabelWrap = false;
			this.motor_LV.LargeImageList = this.VPDOIcons;
			this.motor_LV.Location = new System.Drawing.Point(284, 0);
			this.motor_LV.MultiSelect = false;
			this.motor_LV.Name = "motor_LV";
			this.motor_LV.Size = new System.Drawing.Size(468, 280);
			this.motor_LV.SmallImageList = this.VPDOIcons;
			this.motor_LV.StateImageList = this.VPDOIcons;
			this.motor_LV.TabIndex = 32;
			this.motor_LV.View = System.Windows.Forms.View.List;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Mappable DIgital Inputs";
			this.columnHeader2.Width = 468;
			// 
			// splitter5
			// 
			this.splitter5.BackColor = System.Drawing.Color.Black;
			this.splitter5.Location = new System.Drawing.Point(280, 0);
			this.splitter5.Name = "splitter5";
			this.splitter5.Size = new System.Drawing.Size(4, 280);
			this.splitter5.TabIndex = 3;
			this.splitter5.TabStop = false;
			// 
			// MotorSigs_TV
			// 
			this.MotorSigs_TV.AllowDrop = true;
			this.MotorSigs_TV.Dock = System.Windows.Forms.DockStyle.Left;
			this.MotorSigs_TV.ImageList = this.VPDOIcons;
			this.MotorSigs_TV.Location = new System.Drawing.Point(0, 0);
			this.MotorSigs_TV.Name = "MotorSigs_TV";
			this.MotorSigs_TV.Size = new System.Drawing.Size(280, 280);
			this.MotorSigs_TV.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(864, 32);
			this.label1.TabIndex = 27;
			// 
			// submitBtn
			// 
			this.submitBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.submitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.submitBtn.Location = new System.Drawing.Point(8, 376);
			this.submitBtn.Name = "submitBtn";
			this.submitBtn.Size = new System.Drawing.Size(192, 25);
			this.submitBtn.TabIndex = 28;
			this.submitBtn.Text = "&Submit Changes to node";
			this.submitBtn.Click += new System.EventHandler(this.button1_Click);
			// 
			// timer1
			// 
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 415);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(759, 22);
			this.statusBar1.TabIndex = 29;
			// 
			// VPDOs
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(759, 437);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.closeBtn);
			this.Controls.Add(this.submitBtn);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "VPDOs";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "VPDOs";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.VPDOs_Closing);
			this.Load += new System.EventHandler(this.VPDOs_Load);
			this.Closed += new System.EventHandler(this.VPDOs_Closed);
			this.tabControl1.ResumeLayout(false);
			this.DigitalInputs.ResumeLayout(false);
			this.AnalogueInputs.ResumeLayout(false);
			this.DigitalOutputs.ResumeLayout(false);
			this.AnalogueOutputs.ResumeLayout(false);
			this.Motor.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
		public VPDOs( ref SystemInfo systemInfo, int nodeNum, string nodeText, ToolBar passed_ToolBar, ProgressBar passed_progressbar )
		{
			InitializeComponent();
			localSystemInfo = systemInfo;
			this.nodeIndex = nodeNum;
			this.selectedNodeText = nodeText;
			this.toolbar = passed_ToolBar;
			this.progressbar = passed_progressbar;
			if((this.localSystemInfo.nodes[nodeIndex].accessLevel < SCCorpStyle.AccLevel_VPDOs_Write)
				|| (this.localSystemInfo.nodes[nodeIndex].nodeState != NodeState.PreOperational))
			{
				#region read only mode
				if(this.localSystemInfo.nodes[nodeIndex].accessLevel < SCCorpStyle.AccLevel_VPDOs_Write)
				{
					this.statusBar1.Text = "READ ONLY- Insufficient access to modify settings";
				}
				else  if(this.localSystemInfo.nodes[nodeIndex].nodeState != NodeState.PreOperational)
				{
					this.statusBar1.Text = "READ ONLY- Node must be in pre-operational to modify settings";
				}
				this.submitBtn.Visible = false;
				this.label1.Text = "View Internal PDO Connections";
				#endregion read only mode
			}
			else
			{
				#region switch on event handlers for Drag Drop, deleter entries etc
				this.DigitalIPS_LV.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView_KeyDown);
				this.DigitalIPS_LV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDown);
				this.DigitalIPS_LV.DragOver += new System.Windows.Forms.DragEventHandler(this.listView_DragOver);
				this.DigitalIPS_LV.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView_DragDrop);
				this.DigitalIPS_LV.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView_DragEnter);
				this.DigitalIPS_LV.DragLeave += new System.EventHandler(this.listView_DragLeave);
				this.DigitalIPS_LV.SelectedIndexChanged += new System.EventHandler(this.DigitalIPS_LV_SelectedIndexChanged);

				this.DigIPs_TV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
				this.DigIPs_TV.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeView_DragOver);
				this.DigIPs_TV.DragEnter += new System.Windows.Forms.DragEventHandler(this.TreeView_DragEnter);
				this.DigIPs_TV.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
				this.DigIPs_TV.DragLeave += new System.EventHandler(this.TreeView_DragLeave);
				this.DigIPs_TV.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeView_DragDrop);
				this.DigIPs_TV.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.TreeViewDragDrop_GiveFeedback);

				this.AnalogIPs_LV.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView_KeyDown);
				this.AnalogIPs_LV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDown);
				this.AnalogIPs_LV.DragOver += new System.Windows.Forms.DragEventHandler(this.listView_DragOver);
				this.AnalogIPs_LV.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView_DragDrop);
				this.AnalogIPs_LV.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView_DragEnter);
				this.AnalogIPs_LV.DragLeave += new System.EventHandler(this.listView_DragLeave);

				this.AnlgIPs_TV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
				this.AnlgIPs_TV.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeView_DragOver);
				this.AnlgIPs_TV.DragEnter += new System.Windows.Forms.DragEventHandler(this.TreeView_DragEnter);
				this.AnlgIPs_TV.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
				this.AnlgIPs_TV.DragLeave += new System.EventHandler(this.TreeView_DragLeave);
				this.AnlgIPs_TV.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeView_DragDrop);
				this.AnlgIPs_TV.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.TreeViewDragDrop_GiveFeedback);

				this.DigitalOPs_LV.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView_KeyDown);
				this.DigitalOPs_LV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDown);
				this.DigitalOPs_LV.DragOver += new System.Windows.Forms.DragEventHandler(this.listView_DragOver);
				this.DigitalOPs_LV.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView_DragDrop);
				this.DigitalOPs_LV.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView_DragEnter);
				this.DigitalOPs_LV.DragLeave += new System.EventHandler(this.listView_DragLeave);

				this.DigOPs_TV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
				this.DigOPs_TV.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeView_DragOver);
				this.DigOPs_TV.DragEnter += new System.Windows.Forms.DragEventHandler(this.TreeView_DragEnter);
				this.DigOPs_TV.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
				this.DigOPs_TV.DragLeave += new System.EventHandler(this.TreeView_DragLeave);
				this.DigOPs_TV.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeView_DragDrop);
				this.DigOPs_TV.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.TreeViewDragDrop_GiveFeedback);

				this.AnalogOPs_LV.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView_KeyDown);
				this.AnalogOPs_LV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDown);
				this.AnalogOPs_LV.DragOver += new System.Windows.Forms.DragEventHandler(this.listView_DragOver);
				this.AnalogOPs_LV.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView_DragDrop);
				this.AnalogOPs_LV.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView_DragEnter);
				this.AnalogOPs_LV.DragLeave += new System.EventHandler(this.listView_DragLeave);

				this.AnlgOPs_TV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
				this.AnlgOPs_TV.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeView_DragOver);
				this.AnlgOPs_TV.DragEnter += new System.Windows.Forms.DragEventHandler(this.TreeView_DragEnter);
				this.AnlgOPs_TV.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
				this.AnlgOPs_TV.DragLeave += new System.EventHandler(this.TreeView_DragLeave);
				this.AnlgOPs_TV.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeView_DragDrop);
				this.AnlgOPs_TV.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.TreeViewDragDrop_GiveFeedback);

				this.motor_LV.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView_KeyDown);
				this.motor_LV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDown);
				this.motor_LV.DragOver += new System.Windows.Forms.DragEventHandler(this.listView_DragOver);
				this.motor_LV.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView_DragDrop);
				this.motor_LV.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView_DragEnter);
				this.motor_LV.DragLeave += new System.EventHandler(this.listView_DragLeave);

				this.MotorSigs_TV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
				this.MotorSigs_TV.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeView_DragOver);
				this.MotorSigs_TV.DragEnter += new System.Windows.Forms.DragEventHandler(this.TreeView_DragEnter);
				this.MotorSigs_TV.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
				this.MotorSigs_TV.DragLeave += new System.EventHandler(this.TreeView_DragLeave);
				this.MotorSigs_TV.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeView_DragDrop);
				this.MotorSigs_TV.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.TreeViewDragDrop_GiveFeedback);
				#endregion switch on event handlers for Drag Drop, deleter entries etc
				this.label1.Text = "Select Signal and drag it to required connection.\nTo remove a connection , select it and then press Delete key";
				this.statusBar1.Text = "";
				MenuItem contMI = new MenuItem("&Delete this connection");
				this.contextMenu1.MenuItems.Add(contMI);
			}
		}
		private void VPDOs_Load(object sender, System.EventArgs e)
		{
			this.Text = "DriveWizard: Internal PDO configuration";
			#region first add 'empty'value to Hash tables
			int dummyValue = 0;
			feedback = this.localSystemInfo.nodes[ nodeIndex ].dictionary.getDummyVPDO( SevconObjectType.LOCAL_DIG_IN_MAPPING, out dummyValue );
			if(feedback == DIFeedbackCode.DISuccess)
			{
				this.digIPS_HT.Add(dummyValue, "Not mapped");
			}
			else
			{
				this.digIPS_HT.Add(0x21FF, "Not mapped"); //judetemp use hard coded defualt for now
			}
			feedback = this.localSystemInfo.nodes[ nodeIndex ].dictionary.getDummyVPDO( SevconObjectType.LOCAL_DIG_OUT_MAPPING, out dummyValue );
			if(feedback == DIFeedbackCode.DISuccess)
			{
				this.digOPs_HT.Add(dummyValue, "Not mapped");
			}
			else
			{
				this.digOPs_HT.Add(0x23FF, "Not mapped"); //judetemp use hard coded defualt for now
			}
			feedback = this.localSystemInfo.nodes[ nodeIndex ].dictionary.getDummyVPDO( SevconObjectType.LOCAL_ALG_IN_MAPPING, out dummyValue );
			if(feedback == DIFeedbackCode.DISuccess)
			{
				this.analIps_HT.Add(dummyValue, "Not mapped");
			}
			else
			{
				this.analIps_HT.Add(0x22FF, "Not mapped"); //judetemp use hard coded defualt for now
			}
			feedback = this.localSystemInfo.nodes[ nodeIndex ].dictionary.getDummyVPDO( SevconObjectType.LOCAL_ALG_OUT_MAPPING, out dummyValue );
			if(feedback == DIFeedbackCode.DISuccess)
			{
				this.analOPs_HT.Add(dummyValue, "Not mapped");
			}
			else
			{
				this.analOPs_HT.Add(0x24FF, "Not mapped"); //judetemp use hard coded defualt for now
			}
		
			feedback = this.localSystemInfo.nodes[ nodeIndex ].dictionary.getDummyVPDO( SevconObjectType.LOCAL_MOTOR_MAPPING, out dummyValue );
			if(feedback == DIFeedbackCode.DISuccess)
			{
				this.motor_HT.Add(dummyValue, "Not mapped");
			}
			else
			{
				this.motor_HT.Add(0x20FF, "Not mapped"); //judetemp use hard coded defualt for now
			}
			#endregion add 'empty'value to Hash tables
			this.VPDOIcons.TransparentColor = Color.White;
			this.shadowFont = new Font( "Arial", 10);
			getData_forTreeView();
			this.progressbar.Visible = true;
			#region data retrieval thread start
			ODDataRetrievalThread = new Thread(new ThreadStart( getODData )); 
			ODDataRetrievalThread.Name = "Internal PDO Data retrieval";
			ODDataRetrievalThread.IsBackground = true;
			ODDataRetrievalThread.Priority = ThreadPriority.BelowNormal;
#if DEBUG
			System.Console.Out.WriteLine("Thread: " + ODDataRetrievalThread.Name + " started");
#endif
			ODDataRetrievalThread.Start(); 
			timer1.Enabled = true;
			#endregion
		}
		private void getODData()
		{
			errorMessage = "";  //reset
			string abortMessage;
			feedback = this.localSystemInfo.readODItemValue(this.nodeIndex, SevconObjectType.LOCAL_DIG_IN_MAPPING,out abortMessage);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				errorMessage += "Failed to retrieve all values from device\nFeedbackcode: " + feedback.ToString();
			}
			feedback = this.localSystemInfo.readODItemValue(this.nodeIndex, SevconObjectType.LOCAL_DIG_OUT_MAPPING,out abortMessage);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				errorMessage += "\nFeedbackcode: " + feedback.ToString();
			}
			feedback = this.localSystemInfo.readODItemValue(this.nodeIndex, SevconObjectType.LOCAL_ALG_IN_MAPPING,out abortMessage);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				errorMessage += "\nFeedbackcode: " + feedback.ToString();
			}
			feedback = this.localSystemInfo.readODItemValue(this.nodeIndex, SevconObjectType.LOCAL_ALG_OUT_MAPPING,out abortMessage);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				errorMessage += "\nFeedbackcode: " + feedback.ToString();
			}
			feedback = this.localSystemInfo.readODItemValue(this.nodeIndex, SevconObjectType.LOCAL_MOTOR_MAPPING,out abortMessage);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				errorMessage += "\nFeedbackcode: " + feedback.ToString();
			}
		}
		private void getData_forTreeView()
		{
			TreeNode vehicleNode = null, tractionNode = null, pumpNode = null, psteerNode = null, miscNode = null;
			foreach(ODItemData[] myEDSRow in localSystemInfo.nodes[nodeIndex].dictionary.data)
			{
				if(myEDSRow[0].objectName == SevconObjectType.DIGITAL_SIGNAL_IN)
				{
					#region create Tree for Digital Inputs
					int nodeInd = 0;
					switch(myEDSRow[0].sectionType)
					{
						case SevconSectionType.TRACTIONDI:
							#region traction
							if(this.DigIPs_TV.Nodes.Contains(tractionNode) == false)
							{
								tractionNode = new TreeNode("Traction", 0, 0);
								this.DigIPs_TV.Nodes.Add(tractionNode);
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].ImageIndex = (int) myIcons.HEADER;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes.Count - 1;
							if(myEDSRow[0].parameterName.ToUpper().IndexOf("HANDBRAKE") != -1)
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.HANDBRAKE;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.HANDBRAKE;
							}
							else if (myEDSRow[0].parameterName.ToUpper().IndexOf("FOOTBRAKE") != -1)
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.FOOTPEDAL;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.FOOTPEDAL;
							}
							else if (myEDSRow[0].parameterName.ToUpper().IndexOf("FS1") != -1)
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.FOOTPEDAL;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.FOOTPEDAL;
							}
							else if (myEDSRow[0].parameterName.ToUpper()== "FORWARD SWITCH")
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.FORWARDSW;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.FORWARDSW;
							}
							else if (myEDSRow[0].parameterName.ToUpper() == "REVERSE SWITCH")
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.REVERSESW;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.REVERSESW;
							}
							else if (myEDSRow[0].parameterName.ToUpper()== "INCH FORWARD SWITCH")
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.INCHFW;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.INCHFW;
							}
							else if (myEDSRow[0].parameterName.ToUpper() == "INCH REVERSE SWITCH")
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.INCHREV;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.INCHREV;
							}
							else if (myEDSRow[0].parameterName.ToUpper() == "HIGH SPEED SWITCH")
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.HIGHSPEEDSW;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.HIGHSPEEDSW;
							}
							else
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.TOGGLE;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.TOGGLE;
							}
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion traction
							break;

						case SevconSectionType.PUMPDI:
							#region pump
							if(this.DigIPs_TV.Nodes.Contains(pumpNode) == false)
							{
								pumpNode = new TreeNode("Pump", 0, 0);
								this.DigIPs_TV.Nodes.Add(pumpNode);
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(pumpNode)].ImageIndex = (int) myIcons.HEADER;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(pumpNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(pumpNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(pumpNode)].Nodes.Count - 1;
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.PUMP;
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.PUMP;
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion pump
							break;

						case SevconSectionType.VEHICLEDI:
							#region vehicle
							if(this.DigIPs_TV.Nodes.Contains(vehicleNode) == false)
							{
								vehicleNode = new TreeNode("Vehicle", 0, 0);
								this.DigIPs_TV.Nodes.Add(vehicleNode);
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(vehicleNode)].ImageIndex = (int) myIcons.HEADER;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(vehicleNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes.Count - 1;
							if(myEDSRow[0].parameterName.ToUpper().IndexOf("KEY SWITCH") != -1)
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.KEYSWITCH;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.KEYSWITCH;
							}
							else if(myEDSRow[0].parameterName.ToUpper().IndexOf("HORN SWITCH") != -1)
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.HORNSWITCH;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.HORNSWITCH;
							}
							else
							{
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.TOGGLE;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.TOGGLE;
							}
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion vehicle
							break;

						case SevconSectionType.PSTEERDI:
							#region power steer
							if(this.DigIPs_TV.Nodes.Contains(psteerNode) == false)
							{
								psteerNode = new TreeNode("Power Steer", 0, 0);
								this.DigIPs_TV.Nodes.Add(psteerNode);
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(psteerNode)].ImageIndex = (int) myIcons.HEADER;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(psteerNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(psteerNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(psteerNode)].Nodes.Count - 1;
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.STEERINGWHEEL;
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.STEERINGWHEEL;
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion power steer
							break;

						default:
							#region default
							if(this.DigIPs_TV.Nodes.Contains(miscNode) == false)
							{
								miscNode = new TreeNode("Miscellaneous", 0, 0);
								this.DigIPs_TV.Nodes.Add(miscNode);
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(miscNode)].ImageIndex = (int) myIcons.HEADER;
								this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(miscNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}

							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(miscNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(miscNode)].Nodes.Count - 1;
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.TOGGLE;
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.TOGGLE;
							this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion default
							break;
					}
					this.digIPS_HT.Add(myEDSRow[0].indexNumber, myEDSRow[0].parameterName);  //now add to Hashtable to allow derferecing int he destination List view
					#endregion create Tree for Digital Inputs
				}
				else if (myEDSRow[0].objectName == SevconObjectType.DIGITAL_SIGNAL_OUT)
				{
					#region create tree for Digital Outputs
					int nodeInd = 0;
					switch(myEDSRow[0].sectionType)
					{
						case SevconSectionType.TRACTIONDO:
							#region traction
							if(this.DigOPs_TV.Nodes.Contains(tractionNode) == false)
							{
								tractionNode = new TreeNode("Traction", 0, 0);
								this.DigOPs_TV.Nodes.Add(tractionNode);
								this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(tractionNode)].ImageIndex = (int) myIcons.HEADER;
								this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(tractionNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(tractionNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(tractionNode)].Nodes.Count - 1;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.CLOCK;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.CLOCK;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion traction
							break;

						case SevconSectionType.PUMPDO:
							#region pump
							if(this.DigOPs_TV.Nodes.Contains(pumpNode) == false)
							{
								pumpNode = new TreeNode("Pump", 0, 0);
								this.DigOPs_TV.Nodes.Add(pumpNode);
								this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(pumpNode)].ImageIndex = (int) myIcons.HEADER;
								this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(pumpNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(pumpNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(pumpNode)].Nodes.Count - 1;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.TOGGLE;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.TOGGLE;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion pump
							break;

						case SevconSectionType.VEHICLEDO:
							#region vehicle
							if(this.DigOPs_TV.Nodes.Contains(vehicleNode) == false)
							{
								vehicleNode = new TreeNode("Vehicle", 0, 0);
								this.DigOPs_TV.Nodes.Add(vehicleNode);
								this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(vehicleNode)].ImageIndex = (int) myIcons.HEADER;
								this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(vehicleNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes.Count - 1;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.TOGGLE;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.TOGGLE;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion vehicle
							break;

						case SevconSectionType.PSTEERDO:
							#region power steer
							if(this.DigOPs_TV.Nodes.Contains(psteerNode) == false)
							{
								psteerNode = new TreeNode("Power Steer", 0, 0);
								this.DigOPs_TV.Nodes.Add(psteerNode);
								this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(psteerNode)].ImageIndex = (int) myIcons.HEADER;
								this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(psteerNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(psteerNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(psteerNode)].Nodes.Count - 1;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.STEERINGWHEEL;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.STEERINGWHEEL;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion power steer
							break;

						default:
							#region default
							if(this.DigOPs_TV.Nodes.Contains(miscNode) == false)
							{
								miscNode = new TreeNode("Miscellaneous", 0, 0);
								this.DigOPs_TV.Nodes.Add(miscNode);
								this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(miscNode)].ImageIndex = (int) myIcons.HEADER;
								this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(miscNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}

							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(miscNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(miscNode)].Nodes.Count - 1;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.TOGGLE;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.TOGGLE;
							this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion default
							break;
					}
					this.digOPs_HT.Add(myEDSRow[0].indexNumber, myEDSRow[0].parameterName);  //now add to Hashtable to allow derferecing int he destination List view
					#endregion create tree for Digital Outputs
				}
				else if (myEDSRow[0].objectName == SevconObjectType.ANALOGUE_SIGNAL_IN)
				{
					#region create tree for Analogue Inputs
					int nodeInd = 0;
					switch(myEDSRow[0].sectionType)
					{
						case SevconSectionType.TRACTIONAI:
							#region traction
							if(this.AnlgIPs_TV.Nodes.Contains(tractionNode) == false)
							{
								tractionNode = new TreeNode("Traction", 0, 0);
								this.AnlgIPs_TV.Nodes.Add(tractionNode);
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(tractionNode)].ImageIndex = (int) myIcons.HEADER;
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(tractionNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(tractionNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(tractionNode)].Nodes.Count - 1;
							if (myEDSRow[0].parameterName.ToUpper().IndexOf("FOOTBRAKE") != -1)
							{
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.FOOTPEDAL;
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.FOOTPEDAL;
							}
							else
							{
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.SINEWAVE;
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.SINEWAVE;
							}
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion traction
							break;

						case SevconSectionType.PUMPAI:
							#region pump
							if(this.AnlgIPs_TV.Nodes.Contains(pumpNode) == false)
							{
								pumpNode = new TreeNode("Pump", 0, 0);
								this.AnlgIPs_TV.Nodes.Add(pumpNode);
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(pumpNode)].ImageIndex = (int) myIcons.HEADER;
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(pumpNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(pumpNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(pumpNode)].Nodes.Count - 1;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.PUMP;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.PUMP;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion pump
							break;

						case SevconSectionType.VEHICLEAI:
							#region vehicle
							if(this.AnlgIPs_TV.Nodes.Contains(vehicleNode) == false)
							{
								vehicleNode = new TreeNode("Vehicle", 0, 0);
								this.AnlgIPs_TV.Nodes.Add(vehicleNode);
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(vehicleNode)].ImageIndex = (int) myIcons.HEADER;
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(vehicleNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes.Count - 1;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.SINEWAVE;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.SINEWAVE;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion vehicle
							break;

						case SevconSectionType.PSTEERAI:
							#region power steer
							if(this.AnlgIPs_TV.Nodes.Contains(psteerNode) == false)
							{
								psteerNode = new TreeNode("Power Steer", 0, 0);
								this.AnlgIPs_TV.Nodes.Add(psteerNode);
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(psteerNode)].ImageIndex = (int) myIcons.HEADER;
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(psteerNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(psteerNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(psteerNode)].Nodes.Count - 1;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.STEERINGWHEEL;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.STEERINGWHEEL;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion power steer
							break;

						default:
							#region default
							if(this.AnlgIPs_TV.Nodes.Contains(miscNode) == false)
							{
								miscNode = new TreeNode("Miscellaneous", 0, 0);
								this.AnlgIPs_TV.Nodes.Add(miscNode);
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(miscNode)].ImageIndex = (int) myIcons.HEADER;
								this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(miscNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(miscNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(miscNode)].Nodes.Count - 1;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.SINEWAVE;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].SelectedImageIndex =  (int) myIcons.SINEWAVE;
							this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion default
							break;
					}		
					this.analIps_HT.Add(myEDSRow[0].indexNumber, myEDSRow[0].parameterName);  //now add to Hashtable to allow derferecing int he destination List view
					#endregion create tree for Analogue Inputs
				}
				else if(myEDSRow[0].objectName == SevconObjectType.ANALOGUE_SIGNAL_OUT)
				{
					#region create tree for Analogue Outputs
					int nodeInd = 0;
					switch(myEDSRow[0].sectionType)
					{
						case SevconSectionType.TRACTIONAO:
							#region traction
							if(this.AnlgOPs_TV.Nodes.Contains(tractionNode) == false)
							{
								tractionNode = new TreeNode("Traction", 0, 0);
								this.AnlgOPs_TV.Nodes.Add(tractionNode);
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(tractionNode)].ImageIndex = (int) myIcons.HEADER;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(tractionNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(tractionNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(tractionNode)].Nodes.Count - 1;
							if(myEDSRow[0].parameterName.ToUpper().IndexOf("FAN") != -1)
							{
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.FAN;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.FAN;
							}
							else
							{
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.SINEWAVE;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.SINEWAVE;
							}
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion traction
							break;

						case SevconSectionType.PUMPAO:
							#region pump
							if(this.DigOPs_TV.Nodes.Contains(pumpNode) == false)
							{
								pumpNode = new TreeNode("Pump", 0, 0);
								this.AnlgOPs_TV.Nodes.Add(pumpNode);
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(pumpNode)].ImageIndex = (int) myIcons.HEADER;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(pumpNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(pumpNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(pumpNode)].Nodes.Count - 1;
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.PUMP;
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.PUMP;
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion pump
							break;

						case SevconSectionType.VEHICLEAO:
							#region vehicle
							if(this.AnlgOPs_TV.Nodes.Contains(vehicleNode) == false)
							{
								vehicleNode = new TreeNode("Vehicle", 0, 0);
								this.AnlgOPs_TV.Nodes.Add(vehicleNode);
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].ImageIndex = (int) myIcons.HEADER;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes.Count - 1;
							if(myEDSRow[0].parameterName.ToUpper().IndexOf("EXTERNAL LED") != -1)
							{
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.LED;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.LED;
							}
							else if (myEDSRow[0].parameterName.ToUpper().IndexOf("HORN") != -1)
							{
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.HORNSWITCH;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.HORNSWITCH;
							}
							else if (myEDSRow[0].parameterName.ToUpper() == "SERVICE DUE")
							{
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.SPANNER;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.SPANNER;
							}
							else if (myEDSRow[0].parameterName.ToUpper() == "ALARM BUZZER")
							{
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.BUZZER;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.BUZZER;
							}
							else
							{
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.SINEWAVE;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.SINEWAVE;
							}
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(vehicleNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion vehicle
							break;

						case SevconSectionType.PSTEERAO:
							#region power steer
							if(this.AnlgOPs_TV.Nodes.Contains(psteerNode) == false)
							{
								psteerNode = new TreeNode("Power Steer", 0, 0);
								this.AnlgOPs_TV.Nodes.Add(psteerNode);
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(psteerNode)].ImageIndex = (int) myIcons.HEADER;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(psteerNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(psteerNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(psteerNode)].Nodes.Count - 1;
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.STEERINGWHEEL;
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.STEERINGWHEEL;
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion power steer
							break;

						default:
							#region default
							if(this.AnlgOPs_TV.Nodes.Contains(miscNode) == false)
							{
								miscNode = new TreeNode("Miscellaneous", 0, 0);
								this.AnlgOPs_TV.Nodes.Add(miscNode);
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(miscNode)].ImageIndex  = (int) myIcons.HEADER;
								this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(miscNode)].SelectedImageIndex = (int) myIcons.HEADER;
							}

							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(miscNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(miscNode)].Nodes.Count - 1;
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.SINEWAVE;
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.SINEWAVE;
							this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion default
							break;
					}
					this.analOPs_HT.Add(myEDSRow[0].indexNumber, myEDSRow[0].parameterName);  //now add to Hashtable to allow derferecing int he destination List view
					#endregion create tree for Analogue Outputs
				}
				else if(myEDSRow[0].objectName == SevconObjectType.MOTOR_DRIVE)
				{
					#region create tree for Motor Drive Signals
					int nodeInd = 0;
					switch(myEDSRow[0].sectionType)
					{
						case SevconSectionType.TRACTIONCONFIG:
							#region traction
							if(this.MotorSigs_TV.Nodes.Contains(tractionNode) == false)
							{
								tractionNode = new TreeNode("Traction", 0, 0);
								this.MotorSigs_TV.Nodes.Add(tractionNode);
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(tractionNode)].ImageIndex  = (int) myIcons.HEADER;
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(tractionNode)].SelectedImageIndex  = (int) myIcons.HEADER;
							}
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(tractionNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(tractionNode)].Nodes.Count - 1;
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.SINEWAVE;
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.SINEWAVE;
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(tractionNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion traction
							break;

						case SevconSectionType.PUMPCONFIG:
							#region pump
							if(this.DigOPs_TV.Nodes.Contains(pumpNode) == false)
							{
								pumpNode = new TreeNode("Pump", 0, 0);
								this.MotorSigs_TV.Nodes.Add(pumpNode);
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(pumpNode)].ImageIndex  = (int) myIcons.HEADER;
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(pumpNode)].SelectedImageIndex  = (int) myIcons.HEADER;
							}
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(pumpNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(pumpNode)].Nodes.Count - 1;
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.PUMP;
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.PUMP;
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(pumpNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion pump
							break;

						case SevconSectionType.PSTEERCONFIG:
							#region power steer
							if(this.MotorSigs_TV.Nodes.Contains(psteerNode) == false)
							{
								psteerNode = new TreeNode("Power Steer", 0, 0);
								this.MotorSigs_TV.Nodes.Add(psteerNode);
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(psteerNode)].ImageIndex  = (int) myIcons.HEADER;
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(psteerNode)].SelectedImageIndex  = (int) myIcons.HEADER;
							}
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(psteerNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(psteerNode)].Nodes.Count - 1;
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.STEERINGWHEEL;
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.STEERINGWHEEL;
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(psteerNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion power steer
							break;

						default:
							#region default
							if(this.MotorSigs_TV.Nodes.Contains(miscNode) == false)
							{
								miscNode = new TreeNode("Miscellaneous", 0, 0);
								this.MotorSigs_TV.Nodes.Add(miscNode);
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(miscNode)].ImageIndex  = (int) myIcons.HEADER;
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(miscNode)].SelectedImageIndex  = (int) myIcons.HEADER;
							}
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(miscNode)].Nodes.Add(myEDSRow[0].parameterName);
							nodeInd = this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(miscNode)].Nodes.Count - 1;
							if(myEDSRow[0].parameterName.ToUpper().IndexOf("PUMP") != -1)
							{
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.PUMP;
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.PUMP;
							}
							else if(myEDSRow[0].parameterName.ToUpper().IndexOf("POWER STEER") != -1)
							{
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.STEERINGWHEEL;
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.STEERINGWHEEL;
							}
							else
							{
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].ImageIndex = (int) myIcons.SINEWAVE;
								this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].SelectedImageIndex = (int) myIcons.SINEWAVE;
							}
							this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(miscNode)].Nodes[nodeInd].Tag = myEDSRow[0].indexNumber;
							#endregion default
							break;
					}
					try
					{
						this.motor_HT.Add(myEDSRow[0].indexNumber, myEDSRow[0].parameterName);  //now add to Hashtable to allow derferecing int he destination List view
					}
					catch (Exception e)
					{
						Message.Show("Unable to add 0x" + myEDSRow[0].indexNumber.ToString("X") + " " + myEDSRow[0].parameterName + " to Motor HT:  " + e.Message);
					}

					#endregion create tree for Motor Drive Signals
				}
			}
		}

		private void getData_forListView()
		{
			foreach(ODItemData[] myEDSRow in localSystemInfo.nodes[nodeIndex].dictionary.data)
			{
				SevconObjectType currentObject = SevconObjectType.NONE;
				foreach(ODItemData myData in myEDSRow)
				{
					#region locate start of mapping regions
					if(myData.objectName == SevconObjectType.LOCAL_DIG_IN_MAPPING)
					{
						currentObject = SevconObjectType.LOCAL_DIG_IN_MAPPING;
						this.maxDigitalInputs = myEDSRow.Length-2;//judetemp
					}
					else if(myData.objectName == SevconObjectType.LOCAL_DIG_OUT_MAPPING)
					{
						currentObject = SevconObjectType.LOCAL_DIG_OUT_MAPPING;
						this.MaxDigitalOPs = myEDSRow.Length-2;//judetemp
					}
					else if(myData.objectName == SevconObjectType.LOCAL_ALG_IN_MAPPING)
					{
						currentObject = SevconObjectType.LOCAL_ALG_IN_MAPPING;
						this.MaxAnalogueIPs = myEDSRow.Length-2;//judetemp
					}
					else if(myData.objectName == SevconObjectType.LOCAL_ALG_OUT_MAPPING)
					{
						currentObject = SevconObjectType.LOCAL_ALG_OUT_MAPPING;
						this.MaxAnalogueOutputs = myEDSRow.Length-2;//judetemp
					}
					else if(myData.objectName == SevconObjectType.LOCAL_MOTOR_MAPPING)
					{
						currentObject = SevconObjectType.LOCAL_MOTOR_MAPPING;
						this.MaxMotorItems = myEDSRow.Length-2;//judetemp
					}
						#endregion locate start of mapping regions
					if (currentObject == SevconObjectType.LOCAL_DIG_IN_MAPPING)
					{
						#region add exisitng Dig I/p mapped itmes and their 'slots'
						if(myData.subNumber>0)
						{
							int keyInt = System.Convert.ToInt32(myData.currentValue);
							int numItems = this.DigitalIPS_LV.Items.Count;
							if(digIPS_HT[keyInt] == null)
							{
								this.DigitalIPS_LV.Items.Add(myData.parameterName + ": " + "Invalid value", 0); 
							}
							else
							{
								this.DigitalIPS_LV.Items.Add(myData.parameterName + ": " + digIPS_HT[keyInt].ToString(), 0);  
							}
							this.DigitalIPS_LV.Items[numItems].Tag = keyInt;
							#region add icon
							bool nodeFound = false;
							foreach( TreeNode Tnode in this.DigIPs_TV.Nodes)
							{
								foreach(TreeNode childNode in this.DigIPs_TV.Nodes[DigIPs_TV.Nodes.IndexOf(Tnode)].Nodes)
								{
									if(System.Convert.ToInt32(childNode.Tag) == keyInt)
									{
										this.DigitalIPS_LV.Items[numItems].ImageIndex = childNode.ImageIndex;
										nodeFound = true;
										break;
									}
								}
								if(nodeFound == true)
								{
									break;
								}
								else
								{
									this.DigitalIPS_LV.Items[numItems].ImageIndex = (int) myIcons.NOTMAPPED;
								}
							}
							#endregion add icon
							if(myData.subNumber >= this.maxDigitalInputs)
							{
								currentObject = SevconObjectType.NONE;
							}
						}
						#endregion add exisitng mapped itmes and their 'slots'
					}
					else if (currentObject == SevconObjectType.LOCAL_DIG_OUT_MAPPING)
					{
						#region add exisitng Dig O/p mapped itmes and their 'slots'
						if(myData.subNumber>0)
						{
							int keyInt = System.Convert.ToInt32(myData.currentValue);
							int numItems = this.DigitalOPs_LV.Items.Count;
							if(digOPs_HT[keyInt] == null)
							{
								this.DigitalOPs_LV.Items.Add(myData.parameterName + ": " + "Invalid value", 0); 
							}
							else
							{
								this.DigitalOPs_LV.Items.Add(myData.parameterName + ": " + this.digOPs_HT[keyInt].ToString(), 0);  
							}
							this.DigitalOPs_LV.Items[numItems].Tag = keyInt;
							#region add icon
							bool nodeFound = false;
							foreach( TreeNode Tnode in this.DigOPs_TV.Nodes)
							{
								foreach(TreeNode childNode in this.DigOPs_TV.Nodes[DigOPs_TV.Nodes.IndexOf(Tnode)].Nodes)
								{
									if(System.Convert.ToInt32(childNode.Tag) == keyInt)
									{
										this.DigitalOPs_LV.Items[numItems].ImageIndex = childNode.ImageIndex;
										nodeFound = true;
										break;
									}
								}
								if(nodeFound == true)
								{
									break;
								}
								else
								{
									this.DigitalOPs_LV.Items[numItems].ImageIndex = (int) myIcons.NOTMAPPED;
								}
							}
							#endregion add icon
							if(myData.subNumber >= this.MaxDigitalOPs)
							{
								currentObject = SevconObjectType.NONE;
							}
						}
						#endregion add exisitng mapped itmes and their 'slots'
					}
					else if(currentObject == SevconObjectType.LOCAL_ALG_IN_MAPPING)
					{
						#region add existing alalogue i/p slots and their contents
						if(myData.subNumber>0)
						{
							int keyInt = System.Convert.ToInt32(myData.currentValue);
							int numItems = this.AnalogIPs_LV.Items.Count;
							if(analIps_HT[keyInt] == null)
							{
								this.AnalogIPs_LV.Items.Add(myData.parameterName + ": " + "Invalid value", 0); 
							}
							else
							{
								this.AnalogIPs_LV.Items.Add(myData.parameterName + ": " + this.analIps_HT[keyInt].ToString(), 0);  
							}
							this.AnalogIPs_LV.Items[numItems].Tag = keyInt;
							#region add icon
							bool nodeFound = false;
							foreach( TreeNode Tnode in this.AnlgIPs_TV.Nodes)
							{
								foreach(TreeNode childNode in this.AnlgIPs_TV.Nodes[AnlgIPs_TV.Nodes.IndexOf(Tnode)].Nodes)
								{
									if(System.Convert.ToInt32(childNode.Tag) == keyInt)
									{
										this.AnalogIPs_LV.Items[numItems].ImageIndex = childNode.ImageIndex;
										nodeFound = true;
										break;
									}
								}
								if(nodeFound == true)
								{
									break;
								}
								else
								{
									this.AnalogIPs_LV.Items[numItems].ImageIndex = (int) myIcons.NOTMAPPED;
								}
							}
							#endregion add icon
							if(myData.subNumber >= this.MaxAnalogueIPs)
							{
								currentObject = SevconObjectType.NONE;
							}
						}
						#endregion add existing alalogue i/p slots and their contents
					}
					else if (currentObject == SevconObjectType.LOCAL_ALG_OUT_MAPPING) 
					{
						#region add existing analogue o/p slots and their contents
						if(myData.subNumber>0)
						{
							int keyInt = System.Convert.ToInt32(myData.currentValue);
							int numItems = this.AnalogOPs_LV.Items.Count;
							if(analOPs_HT[keyInt] == null)
							{
								this.AnalogOPs_LV.Items.Add(myData.parameterName + ": " + "Invalid value", 0); 
							}
							else
							{
								this.AnalogOPs_LV.Items.Add(myData.parameterName + ": " + this.analOPs_HT[keyInt].ToString(), 0);  
							}
							this.AnalogOPs_LV.Items[numItems].Tag = keyInt;
							#region add icon
							bool nodeFound = false;
							foreach( TreeNode Tnode in this.AnlgOPs_TV.Nodes)
							{
								foreach(TreeNode childNode in this.AnlgOPs_TV.Nodes[AnlgOPs_TV.Nodes.IndexOf(Tnode)].Nodes)
								{
									if(System.Convert.ToInt32(childNode.Tag) == keyInt)
									{
										this.AnalogOPs_LV.Items[numItems].ImageIndex = childNode.ImageIndex;
										nodeFound = true;
										break;
									}
								}
								if(nodeFound == true)
								{
									break;
								}
								else
								{
									this.AnalogOPs_LV.Items[numItems].ImageIndex = (int) myIcons.NOTMAPPED;
								}
							}
							#endregion add icon
							if(myData.subNumber >= this.MaxAnalogueOutputs)
							{
								currentObject = SevconObjectType.NONE;
							}
						}
						#endregion add existing analogue o/p slots and their contents
					}
					else if (currentObject == SevconObjectType.LOCAL_MOTOR_MAPPING)
					{
						#region add existing motor drive slots and their contents
						if(myData.subNumber>0)
						{
							int keyInt = System.Convert.ToInt32(myData.currentValue);
							int numItems = this.motor_LV.Items.Count;
							if(this.motor_HT[keyInt] == null)
							{
								this.motor_LV.Items.Add(myData.parameterName + ": " + "Invalid value", 0); 
							}
							else
							{
								this.motor_LV.Items.Add(myData.parameterName + ": " + this.motor_HT[keyInt].ToString(), 0);  
							}
							this.motor_LV.Items[numItems].Tag = keyInt;
							#region add icon
							bool nodeFound = false;
							foreach( TreeNode Tnode in this.MotorSigs_TV.Nodes)
							{
								foreach(TreeNode childNode in this.MotorSigs_TV.Nodes[MotorSigs_TV.Nodes.IndexOf(Tnode)].Nodes)
								{
									if(System.Convert.ToInt32(childNode.Tag) == keyInt)
									{
										this.motor_LV.Items[numItems].ImageIndex = childNode.ImageIndex;
										nodeFound = true;
										break;
									}
								}
								if(nodeFound == true)
								{
									break;
								}
								else
								{
									this.motor_LV.Items[numItems].ImageIndex = (int) myIcons.NOTMAPPED;
								}
							}
							#endregion add icon
							if(myData.subNumber >= this.MaxMotorItems)
							{
								currentObject = SevconObjectType.NONE;
							}
						}
						#endregion add existing motor drive slots and their contents
					}
				}
			}
		}
		#endregion Initialisation

		#region user interaction
		private void listView_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.ListView")
			{
				ListView sendingLV = (ListView) sender;
				gLV = sendingLV.CreateGraphics();
				//rectagle that stores which bit to rub out when grey drag string is moved
				removeDragStringRectLV = new Rectangle(0,0,0,0);  //initla width, height zero so will have no impact first time round
			}
		}
		private void listView_DragLeave(object sender, System.EventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.ListView")
			{
				ListView sendingLV = (ListView) sender;
				sendingLV.Invalidate(removeDragStringRectLV);  //remove old text
				sendingLV.Update();
			}
		}

		private void listView_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.ListView")
			{
				ListView sendingLV = (ListView) sender;  
				Point myClientPpoint = new Point(0,0);
				myClientPpoint = sendingLV.PointToClient(new Point(e.X, e.Y));
				sendingLV.Invalidate(removeDragStringRectLV);  //remove old text?
				sendingLV.Update();
				Rectangle newDragStringRectLV = new Rectangle(myClientPpoint.X, myClientPpoint.Y, dragStringSize.Width, dragStringSize.Height);
				gLV.DrawString(shadowText, this.shadowFont, Brushes.Gray, newDragStringRectLV);
				//copy over so we know what to rub out next time around
				removeDragStringRectLV = new Rectangle(newDragStringRectLV.Location, dragStringSize);  
				ListViewItem UserSelectedLVItem = sendingLV.GetItemAt(myClientPpoint.X, myClientPpoint.Y);
				if(UserSelectedLVItem != null)
				{
					if(UserSelectedLVItem != MouseOverLVItem)
					{
						foreach(ListViewItem senderLVItem in sendingLV.Items)
						{
							if(senderLVItem.BackColor == SCCorpStyle.dgblockColour)
							{
								senderLVItem.BackColor = Color.White;
								break;
							}
						}
						UserSelectedLVItem.BackColor = SCCorpStyle.dgblockColour;
					}
					MouseOverLVItem = UserSelectedLVItem;
				}
				//' Check for the custom DataFormat ListViewItem array.
				if( e.Data.GetDataPresent("System.Windows.Forms.TreeNode") )
				{
					e.Effect = DragDropEffects.All;
				}
				else
				{
					e.Effect = DragDropEffects.None;
				}
			}
		}
		private void listView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.ListView")
			{
				TreeNode myTreeNode;
				try
				{
					myTreeNode = (TreeNode) e.Data.GetData("System.Windows.Forms.TreeNode", true);
				}
				catch
				{
					return;  //couldn't mould the data into a TreeNode
				}
				ListView sendingLV = (ListView) sender;
				if(MouseOverLVItem != null)  //set in drag over to tell us which item mouse is over if any
				{
					int colonPos = MouseOverLVItem.Text.IndexOf(':');
					this.MouseOverLVItem.Text = MouseOverLVItem.Text.Substring(0,(colonPos+2));  //+2 to include the space
					//now add a space and 
					MouseOverLVItem.Text +=  myTreeNode.Text;
					MouseOverLVItem.ImageIndex = myTreeNode.ImageIndex;
					MouseOverLVItem.Tag = myTreeNode.Tag;
					MouseOverLVItem = null; //reset
				}
				//clear any hung over grey drag strings
				sendingLV.Invalidate(removeDragStringRectLV);  //remove old text
				sendingLV.Update();
			}
		}

		private void listView_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			ListView sendingLV = (ListView) sender;
			if(sendingLV.SelectedItems.Count>0)
			{
				if( ((Keys)e.KeyValue) == Keys.Delete)
				{
					this.selectedItemForContextMenu = sendingLV.SelectedItems[0];
					findAndDeleteconnection();
				}
			}
		}

		private void findAndDeleteconnection()
		{
			int colonPos = selectedItemForContextMenu.Text.IndexOf(':');
			selectedItemForContextMenu.Text = selectedItemForContextMenu.Text.Substring(0,(colonPos+2));  //+2 to include the space
			//now add a space and 
			selectedItemForContextMenu.Text +=  "Not mapped";
			selectedItemForContextMenu.ImageIndex = (int) myIcons.NOTMAPPED;  //insert 'empty' icon
			int dummyValue = 0;
			switch(this.tabControl1.SelectedIndex)
			{
				case (int) tabControls.DigIPs:  //digital inputs 
					#region digital inputs
					feedback = this.localSystemInfo.nodes[ nodeIndex ].dictionary.getDummyVPDO( SevconObjectType.LOCAL_DIG_IN_MAPPING, out dummyValue );
					if(feedback == DIFeedbackCode.DISuccess)
					{
						selectedItemForContextMenu.Tag = dummyValue;
					}
					else
					{
						selectedItemForContextMenu.Tag = 0x21FF;  //judetemp use hard coded defualt for now
					}
					#endregion digital inputs
					break;
				case (int) tabControls.DigOPs: //digital outputs
					#region digital ouptputs
					feedback = this.localSystemInfo.nodes[ nodeIndex ].dictionary.getDummyVPDO( SevconObjectType.LOCAL_DIG_OUT_MAPPING, out dummyValue );
					if(feedback == DIFeedbackCode.DISuccess)
					{
						selectedItemForContextMenu.Tag = dummyValue;
					}
					else
					{
						selectedItemForContextMenu.Tag = 0x23FF; //judetemp use hard coded defualt for now
					}
					#endregion digital ouptputs
					break;
				case (int) tabControls.AlgIPs:  //analogue inputs
					#region analogue inputs
					feedback = this.localSystemInfo.nodes[ nodeIndex ].dictionary.getDummyVPDO( SevconObjectType.LOCAL_ALG_IN_MAPPING, out dummyValue );
					if(feedback == DIFeedbackCode.DISuccess)
					{
						selectedItemForContextMenu.Tag = dummyValue;
					}
					else
					{
						selectedItemForContextMenu.Tag = 0x22FF; //judetemp use hard coded defualt for now
					}
					#endregion analogue inputs
					break;
				case (int)tabControls.AlgOPs: //analogue o/ps
					#region analogue outpus
					feedback = this.localSystemInfo.nodes[ nodeIndex ].dictionary.getDummyVPDO( SevconObjectType.LOCAL_ALG_OUT_MAPPING, out dummyValue );
					if(feedback == DIFeedbackCode.DISuccess)
					{
						selectedItemForContextMenu.Tag = dummyValue;
					}
					else
					{
						selectedItemForContextMenu.Tag = 0x24FF; //judetemp use hard coded defualt for now
					}
					#endregion analogue outpus
					break;
				case (int)tabControls.Motor:  //motor
				default:
					#region motor params
					feedback = this.localSystemInfo.nodes[ nodeIndex ].dictionary.getDummyVPDO( SevconObjectType.LOCAL_MOTOR_MAPPING, out dummyValue );
					if(feedback == DIFeedbackCode.DISuccess)
					{
						selectedItemForContextMenu.Tag = dummyValue; 
					}
					else
					{
						selectedItemForContextMenu.Tag = 0x20FF; //judetemp use hard coded defualt for now
					}
					#endregion motor params
					break;
			}

		}
		private void treeView_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.TreeView")
			{
				TreeView sendingTV = (TreeView) sender;
				if((sendingTV.SelectedNode.GetNodeCount(true)) == 0) //only lowest lavel can be dragged
				{
					statusBar1.Text = "";
					gTV = sendingTV.CreateGraphics();
					shadowText = sendingTV.SelectedNode.Text;
					SizeF dragStringSizeF = gTV.MeasureString("shadowText", this.shadowFont);
					this.dragStringSize = new Size(((int) dragStringSizeF.Width) + 100,(int) dragStringSizeF.Height);
					removeDragStringRectTV = new Rectangle(0,0,0,0);
					sendingTV.DoDragDrop(new DataObject("System.Windows.Forms.TreeNode", sendingTV.SelectedNode), DragDropEffects.All);
					sendingTV.Invalidate(removeDragStringRectTV);  //remove old text
					sendingTV.Update();
				}
				else
				{
					statusBar1.Text = "Header nodes cannot be dragged";
				}
			}
		}
		private void TreeViewDragDrop_GiveFeedback(object sender, System.Windows.Forms.GiveFeedbackEventArgs e)
		{
			e.UseDefaultCursors = false;
			if(sender.GetType().ToString() == "System.Windows.Forms.TreeView")
			{
				this.Cursor = System.Windows.Forms.Cursors.Hand;
			}
			else
			{
				this.Cursor = System.Windows.Forms.Cursors.No;
			}
		}

		private void TreeView_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.TreeView")
			{
				TreeView sendingTV = (TreeView) sender;
				Point myClientPpoint = new Point(0,0);
				myClientPpoint = sendingTV.PointToClient(new Point(e.X, e.Y));
				Rectangle newDragStringRectTV = new Rectangle(myClientPpoint.X, myClientPpoint.Y, dragStringSize.Width, dragStringSize.Height);
				sendingTV.Invalidate(removeDragStringRectTV);  //remove old text?
				sendingTV.Update();
				gTV.DrawString(shadowText, this.shadowFont, Brushes.Gray, newDragStringRectTV);
				removeDragStringRectTV = new Rectangle(newDragStringRectTV.Location, dragStringSize);
			}
		}

		private void TreeView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			return;
		}

		private void TreeView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.TreeView")
			{
				TreeView myTV = (TreeView) sender;
				TreeNode myNode = myTV.GetNodeAt(myTV.PointToClient(Cursor.Position));
				if (myNode != null)
				{
					myTV.SelectedNode = myNode;
				}
			}
		}

		private void TreeView_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.TreeView")
			{
				removeDragStringRectTV = new Rectangle(0,0,0,0);
			}
		}

		private void TreeView_DragLeave(object sender, System.EventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.TreeView")
			{
				TreeView sendingTV = (TreeView) sender;
				sendingTV.Invalidate(removeDragStringRectTV);  //remove old text
				sendingTV.Update();
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			string errorString = "";
			string abortMessage = "";
			//write all vlaues to controller here

			this.statusBar1.Text = "Submitting changes to node please wait";
			#region digital inputs
			//overwrite sub 0 for now until controller code changed
			feedback = this.localSystemInfo.writeODItemValue(this.nodeIndex, 
				SevconObjectType.LOCAL_DIG_IN_MAPPING, 0, maxDigitalInputs, out abortMessage);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				errorString += "\nUnable to update number of Digital Input mappings. \nFeed back code: " + feedback.ToString() + "\nError code: " + abortMessage;
			}
			for(int i = 0;i<this.maxDigitalInputs;i++)
			{
				int myValue = System.Convert.ToInt32(this.DigitalIPS_LV.Items[i].Tag);
				feedback = this.localSystemInfo.writeODItemValue(this.nodeIndex, SevconObjectType.LOCAL_DIG_IN_MAPPING, (i+1),myValue, out abortMessage);
				if(feedback != DIFeedbackCode.DISuccess)
				{
					errorString += "\nUnable change Digital Input mapping. For: " + DigitalIPS_LV.Items[i].Text 
						+ "\nFeed back code: " + feedback.ToString() + "\nError code: " + abortMessage;
				}
			}
			#endregion digital inputs

			#region digital outputs
			//overwrite sub 0 for now until controller code changed
			feedback = this.localSystemInfo.writeODItemValue(this.nodeIndex, 
				SevconObjectType.LOCAL_DIG_OUT_MAPPING, 0, this.MaxDigitalOPs, out abortMessage);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				errorString += "\nUnable to update number of digital Output mappings. \nFeed back code: " + feedback.ToString() + "\nError code: " + abortMessage;
			}
			for(int i = 0;i<this.MaxDigitalOPs;i++)
			{
				int myValue = System.Convert.ToInt32(this.DigitalOPs_LV.Items[i].Tag);
				feedback = this.localSystemInfo.writeODItemValue(this.nodeIndex, 
					SevconObjectType.LOCAL_DIG_OUT_MAPPING, (i+1), myValue, out abortMessage);
				if(feedback != DIFeedbackCode.DISuccess)
				{
					errorString += "\nUnable change Digital Output mapping. For: " + this.DigitalOPs_LV.Items[i].Text 
						+ "\nFeed back code: " + feedback.ToString() + "\nError code: " + abortMessage;
				}
			}
			#endregion digital outputs

			#region analogue inputs
			//overwrite sub 0 for now until controller code changed
			feedback = this.localSystemInfo.writeODItemValue(this.nodeIndex, 
				SevconObjectType.LOCAL_ALG_IN_MAPPING, 0, this.MaxAnalogueIPs, out abortMessage);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				errorString += "\nUnable to update number of analogue input mappings. \nFeed back code: " + feedback.ToString() 
					+ "\nError code: " + abortMessage;
			}
			for(int i = 0;i<this.MaxAnalogueIPs;i++)
			{
				int myValue = System.Convert.ToInt32(AnalogIPs_LV.Items[i].Tag);
				feedback = this.localSystemInfo.writeODItemValue(this.nodeIndex, 
					SevconObjectType.LOCAL_ALG_IN_MAPPING, (i+1), myValue, out abortMessage);
				if(feedback != DIFeedbackCode.DISuccess)
				{
					errorString += "\nUnable change Analogue Input mapping. For:" + this.AnalogIPs_LV.Items[i].Text 
						+ "\nFeed back code: " + feedback.ToString() + "\nError code: " + abortMessage;
				}
			}
			#endregion analogue outputs

			#region analogue outputs
			//overwrite sub 0 for now until controller code changed
			feedback = this.localSystemInfo.writeODItemValue(this.nodeIndex, 
				SevconObjectType.LOCAL_ALG_OUT_MAPPING, 0, this.MaxAnalogueOutputs, out abortMessage);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				errorString += "\nUnable to update number of analogiue Output mappings. \nFeed back code: " + feedback.ToString() 
					+ "\nError code: " + abortMessage;
			}
			for(int i = 0;i<this.MaxAnalogueOutputs;i++)
			{
				int myValue = System.Convert.ToInt32(this.AnalogOPs_LV.Items[i].Tag);
				feedback = this.localSystemInfo.writeODItemValue(this.nodeIndex, 
					SevconObjectType.LOCAL_ALG_OUT_MAPPING, (i+1), myValue, out abortMessage);
				if(feedback != DIFeedbackCode.DISuccess)
				{
					errorString += "\nUnable change Analogue Input mapping. For:" 
						+ this.AnalogOPs_LV.Items[i].Text 
						+ "\nFeed back code: " + feedback.ToString() 
						+ "\nError code: " + abortMessage;
				}
			}
			#endregion analogue outputs

			#region Motor params
			//overwrite sub 0 for now until controller code changed
			feedback = this.localSystemInfo.writeODItemValue(this.nodeIndex, 
				SevconObjectType.LOCAL_MOTOR_MAPPING, 0, this.MaxMotorItems, out abortMessage);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				errorString += "\nUnable to update number of motor mappings. \nFeed back code: " + feedback.ToString() 
					+ "\nError code: " + abortMessage;
			}
			for(int i = 0;i<this.MaxMotorItems;i++)
			{
				int myValue = System.Convert.ToInt32(this.motor_LV.Items[i].Tag);
				feedback = this.localSystemInfo.writeODItemValue(this.nodeIndex, 
					SevconObjectType.LOCAL_MOTOR_MAPPING, (i+1), myValue, out abortMessage);
				if(feedback != DIFeedbackCode.DISuccess)
				{
					errorString += "\nUnable change Motor mapping. For:" 
						+ this.motor_LV.Items[i].Text 
						+ "\nFeed back code: " + feedback.ToString() 
						+ "\nError code: " + abortMessage;
				}
			}
			#endregion motor params
			if(errorString != "")
			{
				Message.Show(errorString); //collate all errros into one message box
				this.statusBar1.Text = "Request failed";
			}
			else
			{
				Message.Show("Internal PDOs successfully set up. \nWhen you have finished, close this window. \nCycle power on device and switch it to operational \nto verify that Internal PDOs have been successfully mapped.\nMonitor emergency messages");
				this.statusBar1.Text = "Internal PDOs set up OK";
			}
		}

		private void ListView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.ListView")
			{
				ListView sendingLV = (ListView) sender;
				if((sendingLV.SelectedItems.Count>0) && ( e.Button == MouseButtons.Right))
				{
					selectedItemForContextMenu = sendingLV.SelectedItems[0];
					this.contextMenu1.MenuItems[0].Click +=new EventHandler(CM_deletetMI_Click);
					Point newPoint = new Point((sendingLV.SelectedItems[0].Bounds.Location.X + sendingLV.SelectedItems[0].Bounds.Width - 100), sendingLV.SelectedItems[0].Bounds.Location.Y);
					this.contextMenu1.Show(sendingLV, newPoint);
				}
			}
		}

		private void CM_deletetMI_Click(object sender, EventArgs e)
		{
			findAndDeleteconnection();
		}

		private void DigitalIPS_LV_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(sender.GetType().ToString() == "System.Windows.Forms.ListView")
			{
				ListView sendingLV = (ListView) sender;
				foreach(ListViewItem senderLVItem in sendingLV.Items)
				{
					if(senderLVItem.BackColor == SCCorpStyle.dgblockColour)
					{
						senderLVItem.BackColor = Color.White;
						break;
					}
				}
			}
		}

		#endregion user interaction

		#region finalisation
		private void closeBtn_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		private void VPDOs_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			statusBar1.Text = "Performing finalisation, please wait";
			#region disable all timers

			#endregion disable all timers
			#region stop all threads

			#endregion stop all threadss
			if(gTV != null)
			{
				gTV.Dispose();
			}
			if(gLV != null)
			{
				gLV.Dispose();
			}
			this.progressbar.Visible = false;
			#region disable all timers
			this.timer1.Enabled = false;
			#endregion disable all timers
			#region stop all threads
			if(ODDataRetrievalThread != null)
			{
				if((ODDataRetrievalThread.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
				{
					ODDataRetrievalThread.Abort();

					if(ODDataRetrievalThread.IsAlive == true)
					{
#if DEBUG
						Message.Show("Failed to close Thread: " + ODDataRetrievalThread.Name + " on exit");
#endif	
						ODDataRetrievalThread = null;
					}
				}
			}
			#endregion stop all threadss
			e.Cancel = false; //force this window to close
		}

		private void VPDOs_Closed(object sender, System.EventArgs e)
		{
			#region reset window title and status bar
			statusBar1.Text = "";
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
		#endregion finalisation

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (ODDataRetrievalThread != null)
			{
				if((ODDataRetrievalThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
					timer1.Enabled = false; //kill timer
					if(this.errorMessage != "")
					{
						Message.Show(this.errorMessage);
					}
					getData_forListView();
					progressbar.Value = progressbar.Minimum;
					this.progressbar.Visible = false;
				}
				else
				{
					progressbar.Value = this.localSystemInfo.nodes[ nodeIndex ].itemBeingRead;
				}
			}

		}
	}
}
