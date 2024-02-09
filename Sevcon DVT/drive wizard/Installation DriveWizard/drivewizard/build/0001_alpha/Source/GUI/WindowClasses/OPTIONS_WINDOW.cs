/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.81$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:08/10/2008 14:03:04$
	$ModDate:08/10/2008 10:33:32$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
    This window allows the user to view, add, delete, modify and test CANbus 
    connection profiles by setting/modifying the system baud rate, selecting 
    to check for only previously found nodes or test for all possible nodes. 
    Nodes found/previously found are displayed to the user, giving node 
    descriptions and vendors obtained from available EDS files on the hard drive.
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  47599: OPTIONS_WINDOW.cs 

   Rev 1.81    08/10/2008 14:03:04  ak
 TRR COD0013 post-test fixes


   Rev 1.80    23/09/2008 23:17:30  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.79    12/03/2008 13:00:14  ak
 All DI Thread.Priority increased to Normal from BelowNormal (needed to run
 VCI3).


   Rev 1.78    18/02/2008 14:18:50  jw
 VCI2 CAN methods renamed and merged for consistency with VCI3 - towards back
 compatibility


   Rev 1.77    18/02/2008 09:28:40  jw
 VCI2 CAN methods moved form communications to VCI 2 and renamed for
 consistency with VCI3 - towards back compatibility


   Rev 1.76    13/02/2008 15:01:28  jw
 More VCI3. Now works generally OK but more testing/optimisation required. 


   Rev 1.75    12/02/2008 08:46:06  jw
 Ongoing VCI3 work. Options and Select profiel windows changed to simplify
 threading and improve feedback.  Prog bar vlaue determination line made
 exception proof. Max and current values used by progress bars determined
 within DI for encapsulation and values reflect activitiy better.


   Rev 1.74    25/01/2008 10:52:16  jw
 VCI3 and Vista functionality files merge - more testing required


   Rev 1.73    21/01/2008 12:03:02  jw
 File merge for VCI3/ Vista. These changes are those to go in all builds


   Rev 1.72    05/12/2007 22:13:04  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
//using System.Xml;
using System.Data;
using System.Threading;
using DW_Ixxat;
using Ixxat.Vci3;           //IXXAT conversion Jude
using Ixxat.Vci3.Bal;       //IXXAT conversion Jude
using Ixxat.Vci3.Bal.Can;   //IXXAT conversion Jude


namespace DriveWizard
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	/// 
	public class OPTIONS_WINDOW : System.Windows.Forms.Form
	{
		#region Controls definitions
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.RadioButton radioButton4;
		private System.Windows.Forms.RadioButton radioButton3;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button addProfile_btn;
		private System.Windows.Forms.Button deleteProfileBtn;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox profileNameTB;
		private System.Windows.Forms.TextBox passwordTB;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox UserIdTB;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.ComboBox baudrateCB;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.Button testConnectBtn;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.CheckedListBox AlllProfilesChkdLB;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button closeBtn;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.Label connectionLabel;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.CheckBox virtualCB;
		private System.Windows.Forms.HelpProvider helpProvider1;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.RadioButton RBEnglish;
		private System.Windows.Forms.RadioButton RBFrench;
		private System.Windows.Forms.RadioButton RBJapanese;
		private System.Windows.Forms.RadioButton RBKorean;
		private System.Windows.Forms.StatusBar statusBar1;
		#endregion Controls definitions

		#region mydefinitions
		#region profile related
		float [] percents = {0.07F, 0.36F, 0.2F, 0.2F, 0.1F, 0.07F};
		virtualSetupTableStyle virtualSetupTS = null;
		virtualSetupTable setUpTable = null;
		VehicleProfile selectedProfile = null;
		private SystemInfo sysInfo;
		private ToolBar toolbar = null;
        private Thread tConnect = null;
		#endregion profile related
		DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
		string profilesDirPath = MAIN_WINDOW.UserDirectoryPath + @"\profiles\";
		StatusBar parentSB = null;
		int dataGrid1DefaultHeight = 0;
		ObjectXMLSerializer vpXMLSerializer;
        private CheckBox showAllcb;     //DR38000263 allow user to select to see all objects on slave
		ArrayList allVehicleProfiles;
		#endregion mydefinitions

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OPTIONS_WINDOW));
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.closeBtn = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.showAllcb = new System.Windows.Forms.CheckBox();
            this.virtualCB = new System.Windows.Forms.CheckBox();
            this.connectionLabel = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.baudrateCB = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.profileNameTB = new System.Windows.Forms.TextBox();
            this.passwordTB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.UserIdTB = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.AlllProfilesChkdLB = new System.Windows.Forms.CheckedListBox();
            this.deleteProfileBtn = new System.Windows.Forms.Button();
            this.addProfile_btn = new System.Windows.Forms.Button();
            this.testConnectBtn = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.RBKorean = new System.Windows.Forms.RadioButton();
            this.RBJapanese = new System.Windows.Forms.RadioButton();
            this.RBFrench = new System.Windows.Forms.RadioButton();
            this.RBEnglish = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.tabPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButton2
            // 
            this.radioButton2.Enabled = false;
            this.radioButton2.Location = new System.Drawing.Point(216, 84);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(128, 24);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Text = "Korean";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(810, 494);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Vehicle Profiles";
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Controls.Add(this.closeBtn);
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.testConnectBtn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(810, 600);
            this.panel1.TabIndex = 0;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(8, 464);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(792, 24);
            this.progressBar1.TabIndex = 40;
            // 
            // closeBtn
            // 
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Location = new System.Drawing.Point(688, 432);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(112, 24);
            this.closeBtn.TabIndex = 38;
            this.closeBtn.Text = "&Close Window";
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.showAllcb);
            this.groupBox4.Controls.Add(this.virtualCB);
            this.groupBox4.Controls.Add(this.connectionLabel);
            this.groupBox4.Controls.Add(this.checkBox1);
            this.groupBox4.Controls.Add(this.dataGrid1);
            this.groupBox4.Controls.Add(this.baudrateCB);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(8, 224);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(800, 208);
            this.groupBox4.TabIndex = 37;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Selected Profile: Connection Settings";
            // 
            // showAllcb
            // 
            this.showAllcb.AutoSize = true;
            this.showAllcb.Location = new System.Drawing.Point(240, 40);
            this.showAllcb.Name = "showAllcb";
            this.showAllcb.Size = new System.Drawing.Size(258, 17);
            this.showAllcb.TabIndex = 42;
            this.showAllcb.Text = "Show master objects on slave controllers";
            this.showAllcb.UseVisualStyleBackColor = true;
            this.showAllcb.CheckedChanged += new System.EventHandler(this.showAllcb_CheckedChanged);
            // 
            // virtualCB
            // 
            this.virtualCB.Location = new System.Drawing.Point(616, 48);
            this.virtualCB.Name = "virtualCB";
            this.virtualCB.Size = new System.Drawing.Size(176, 24);
            this.virtualCB.TabIndex = 41;
            this.virtualCB.Text = "Connect as virtual nodes";
            this.virtualCB.CheckedChanged += new System.EventHandler(this.virtualCB_CheckedChanged);
            // 
            // connectionLabel
            // 
            this.connectionLabel.Location = new System.Drawing.Point(16, 24);
            this.connectionLabel.Name = "connectionLabel";
            this.connectionLabel.Size = new System.Drawing.Size(360, 16);
            this.connectionLabel.TabIndex = 15;
            // 
            // checkBox1
            // 
            this.checkBox1.Location = new System.Drawing.Point(240, 56);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(320, 16);
            this.checkBox1.TabIndex = 14;
            this.checkBox1.Text = "Search for all connected nodes at this baud";
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // dataGrid1
            // 
            this.dataGrid1.CaptionText = "CAN Node Connection Table";
            this.dataGrid1.DataMember = "";
            this.dataGrid1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Location = new System.Drawing.Point(8, 72);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.Size = new System.Drawing.Size(784, 128);
            this.dataGrid1.TabIndex = 13;
            this.dataGrid1.Click += new System.EventHandler(this.dataGrid1_Click);
            // 
            // baudrateCB
            // 
            this.baudrateCB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baudrateCB.Location = new System.Drawing.Point(8, 48);
            this.baudrateCB.MaxDropDownItems = 10;
            this.baudrateCB.Name = "baudrateCB";
            this.baudrateCB.Size = new System.Drawing.Size(216, 21);
            this.baudrateCB.TabIndex = 8;
            this.baudrateCB.Text = "Baud Rate Selection";
            this.baudrateCB.SelectionChangeCommitted += new System.EventHandler(this.baudrateCB_SelectionChangeCommitted);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.profileNameTB);
            this.groupBox3.Controls.Add(this.passwordTB);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.UserIdTB);
            this.groupBox3.Location = new System.Drawing.Point(8, 136);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(800, 80);
            this.groupBox3.TabIndex = 36;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Selected Profile: User Settings";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(8, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 16);
            this.label4.TabIndex = 26;
            this.label4.Text = "Profile name";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // profileNameTB
            // 
            this.profileNameTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.profileNameTB.Location = new System.Drawing.Point(112, 24);
            this.profileNameTB.Name = "profileNameTB";
            this.profileNameTB.Size = new System.Drawing.Size(280, 20);
            this.profileNameTB.TabIndex = 25;
            this.profileNameTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.profileNameTB_KeyDown);
            this.profileNameTB.Leave += new System.EventHandler(this.profileNameTB_Leave);
            // 
            // passwordTB
            // 
            this.passwordTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.passwordTB.Location = new System.Drawing.Point(344, 48);
            this.passwordTB.MaxLength = 5;
            this.passwordTB.Name = "passwordTB";
            this.passwordTB.PasswordChar = '*';
            this.passwordTB.Size = new System.Drawing.Size(136, 20);
            this.passwordTB.TabIndex = 24;
            this.passwordTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.passwordTB_KeyDown);
            this.passwordTB.Leave += new System.EventHandler(this.passwordTB_Leave);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(264, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 16);
            this.label2.TabIndex = 23;
            this.label2.Text = "Passcode";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 16);
            this.label1.TabIndex = 22;
            this.label1.Text = "Customer ID";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // UserIdTB
            // 
            this.UserIdTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UserIdTB.Location = new System.Drawing.Point(112, 48);
            this.UserIdTB.MaxLength = 5;
            this.UserIdTB.Name = "UserIdTB";
            this.UserIdTB.Size = new System.Drawing.Size(136, 20);
            this.UserIdTB.TabIndex = 21;
            this.UserIdTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UserIdTB_KeyDown);
            this.UserIdTB.Leave += new System.EventHandler(this.UserIdTB_Leave);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.AlllProfilesChkdLB);
            this.groupBox2.Controls.Add(this.deleteProfileBtn);
            this.groupBox2.Controls.Add(this.addProfile_btn);
            this.groupBox2.Location = new System.Drawing.Point(8, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(800, 128);
            this.groupBox2.TabIndex = 35;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Current Vehicle Profiles";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(464, 16);
            this.label3.TabIndex = 38;
            this.label3.Text = "Set profile as default by checking it";
            // 
            // AlllProfilesChkdLB
            // 
            this.AlllProfilesChkdLB.Location = new System.Drawing.Point(8, 48);
            this.AlllProfilesChkdLB.Name = "AlllProfilesChkdLB";
            this.AlllProfilesChkdLB.Size = new System.Drawing.Size(616, 79);
            this.AlllProfilesChkdLB.TabIndex = 37;
            this.AlllProfilesChkdLB.SelectedIndexChanged += new System.EventHandler(this.AlllProfilesChkdLB_SelectedIndexChanged);
            this.AlllProfilesChkdLB.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox1_ItemCheck);
            this.AlllProfilesChkdLB.DoubleClick += new System.EventHandler(this.checkedListBox1_DoubleClick);
            // 
            // deleteProfileBtn
            // 
            this.deleteProfileBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.deleteProfileBtn.Location = new System.Drawing.Point(632, 88);
            this.deleteProfileBtn.Name = "deleteProfileBtn";
            this.deleteProfileBtn.Size = new System.Drawing.Size(152, 32);
            this.deleteProfileBtn.TabIndex = 34;
            this.deleteProfileBtn.Text = "&Delete selected profile";
            this.deleteProfileBtn.Click += new System.EventHandler(this.deleteProfileBtn_Click);
            // 
            // addProfile_btn
            // 
            this.addProfile_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addProfile_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addProfile_btn.Location = new System.Drawing.Point(632, 48);
            this.addProfile_btn.Name = "addProfile_btn";
            this.addProfile_btn.Size = new System.Drawing.Size(152, 32);
            this.addProfile_btn.TabIndex = 33;
            this.addProfile_btn.Text = "&Add new profile";
            this.addProfile_btn.Click += new System.EventHandler(this.addProfileBtn_Click_1);
            // 
            // testConnectBtn
            // 
            this.testConnectBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.testConnectBtn.Location = new System.Drawing.Point(560, 432);
            this.testConnectBtn.Name = "testConnectBtn";
            this.testConnectBtn.Size = new System.Drawing.Size(120, 24);
            this.testConnectBtn.TabIndex = 28;
            this.testConnectBtn.Text = "&Test Connection";
            this.testConnectBtn.Click += new System.EventHandler(this.testConnectBtn_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.helpProvider1.SetHelpKeyword(this.tabControl1, "vehicleprofiles");
            this.helpProvider1.SetHelpNavigator(this.tabControl1, System.Windows.Forms.HelpNavigator.TableOfContents);
            this.tabControl1.ItemSize = new System.Drawing.Size(60, 18);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.helpProvider1.SetShowHelp(this.tabControl1, true);
            this.tabControl1.Size = new System.Drawing.Size(818, 520);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox5);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(810, 494);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Language";
            this.tabPage2.Visible = false;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.RBKorean);
            this.groupBox5.Controls.Add(this.RBJapanese);
            this.groupBox5.Controls.Add(this.RBFrench);
            this.groupBox5.Controls.Add(this.RBEnglish);
            this.groupBox5.Location = new System.Drawing.Point(12, 32);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(546, 227);
            this.groupBox5.TabIndex = 0;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Select new language";
            // 
            // RBKorean
            // 
            this.RBKorean.Location = new System.Drawing.Point(12, 120);
            this.RBKorean.Name = "RBKorean";
            this.RBKorean.Size = new System.Drawing.Size(108, 19);
            this.RBKorean.TabIndex = 3;
            this.RBKorean.Text = "Korean";
            // 
            // RBJapanese
            // 
            this.RBJapanese.Location = new System.Drawing.Point(12, 88);
            this.RBJapanese.Name = "RBJapanese";
            this.RBJapanese.Size = new System.Drawing.Size(100, 19);
            this.RBJapanese.TabIndex = 2;
            this.RBJapanese.Text = "Japanese";
            // 
            // RBFrench
            // 
            this.RBFrench.Location = new System.Drawing.Point(12, 57);
            this.RBFrench.Name = "RBFrench";
            this.RBFrench.Size = new System.Drawing.Size(100, 19);
            this.RBFrench.TabIndex = 1;
            this.RBFrench.Text = "French";
            // 
            // RBEnglish
            // 
            this.RBEnglish.Location = new System.Drawing.Point(12, 25);
            this.RBEnglish.Name = "RBEnglish";
            this.RBEnglish.Size = new System.Drawing.Size(100, 19);
            this.RBEnglish.TabIndex = 0;
            this.RBEnglish.Text = "English";
            // 
            // radioButton4
            // 
            this.radioButton4.Enabled = false;
            this.radioButton4.Location = new System.Drawing.Point(16, 88);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(128, 24);
            this.radioButton4.TabIndex = 3;
            this.radioButton4.Text = "Japanese";
            // 
            // radioButton3
            // 
            this.radioButton3.Enabled = false;
            this.radioButton3.Location = new System.Drawing.Point(216, 40);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(128, 24);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.Text = "French";
            // 
            // radioButton1
            // 
            this.radioButton1.Checked = true;
            this.radioButton1.Enabled = false;
            this.radioButton1.Location = new System.Drawing.Point(16, 40);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(128, 24);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "English (US)";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton4);
            this.groupBox1.Controls.Add(this.radioButton3);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Location = new System.Drawing.Point(16, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(600, 192);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 534);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(818, 24);
            this.statusBar1.TabIndex = 42;
            // 
            // OPTIONS_WINDOW
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(818, 558);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.HelpButton = true;
            this.helpProvider1.SetHelpKeyword(this, "vehicleprofile");
            this.helpProvider1.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.TableOfContents);
            this.helpProvider1.SetHelpString(this, "\"help string?\"");
            this.Name = "OPTIONS_WINDOW";
            this.helpProvider1.SetShowHelp(this, true);
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OPTIONS_WINDOW_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.OPTIONS_WINDOW_Closing);
            this.tabPage1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		#region Intitialisation
		public OPTIONS_WINDOW(SystemInfo passed_sysInfo, ToolBar passed_ToolBar, StatusBar passed_statusBar)
		{
			InitializeComponent();
			vpXMLSerializer = new ObjectXMLSerializer();			
			this.toolbar = passed_ToolBar;
			this.sysInfo = passed_sysInfo;
			parentSB = passed_statusBar;
			#region apply SC styling
			foreach(Control c in this.Controls)
			{
				formatControls(c);
			}
			#endregion apply SC styling
			#region profiles load
			this.baudrateCB.DataSource = MAIN_WINDOW.baudrates;  //do this first or selected index changed gets called before we have filled the combo
			this.baudrateCB.SelectedIndex = 0;  //to prevent us ever having a selected item of null value
			if(setUpTable == null)
			{
				this.createSetUptable();
			}
			//now get the actual profiles
			allVehicleProfiles = new ArrayList();
			ArrayList faultyProfiles = new ArrayList();
			int profilePtr = 0;
			for(int i = 0;i<MAIN_WINDOW.DWConfigFile.vehicleprofiles.Count;i++)
			{
				VehicleProfile profile = new VehicleProfile();
				try
				{
					profile = (VehicleProfile) this.vpXMLSerializer.Load(profile, (string) MAIN_WINDOW.DWConfigFile.vehicleprofiles[i] );
				}
				catch
				{
					faultyProfiles.Add(MAIN_WINDOW.DWConfigFile.vehicleprofiles[i]);
					continue;
				}
				//set the path after serialization - ProfilePaht is rest during serialization - despite the ignore Attribute
				profile.ProfilePath = (string) MAIN_WINDOW.DWConfigFile.vehicleprofiles[i];
				profile.baud.baudrate = (BaudRate) Enum.Parse(typeof(BaudRate), profile.baud.rate);
				allVehicleProfiles.Add(profile);
				this.AlllProfilesChkdLB.Items.Add(profile.ProfileName);
				if(MAIN_WINDOW.DWConfigFile.activeProfilePath.profile == profile.ProfilePath)
				{
					this.AlllProfilesChkdLB.SetSelected(profilePtr,true);
					this.AlllProfilesChkdLB.SetItemChecked(profilePtr,true);
					this.selectedProfile = profile;
				}
				profilePtr++;
			}
			foreach(string faultyProfile in faultyProfiles)
			{
				foreach(string allprof in MAIN_WINDOW.DWConfigFile.vehicleprofiles)
				{
					if(allprof == faultyProfile)
					{ 
						MAIN_WINDOW.DWConfigFile.vehicleprofiles.Remove(allprof); //OK because we remove one only then leave = -does not mess up the internal counter
						break;
					}
				}
				DialogResult result = Message.Show(this,
					"Profile " + faultyProfile + " is invalid.\nDo you wish to delete the associated file?",
					"Invalid vehicle profile", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
				if(result == DialogResult.Yes)
				{
					if(System.IO.File.Exists(faultyProfile))
					{
						try
						{
							System.IO.File.Delete(faultyProfile);
						}
						catch( Exception e )
						{
							#region user feedback
							SystemInfo.errorSB.Append("\n Unable to deleted file: ");
							SystemInfo.errorSB.Append("\n");
							SystemInfo.errorSB.Append(faultyProfile);
							SystemInfo.errorSB.Append("Reported error: ");
							SystemInfo.errorSB.Append(e.Message);
							#endregion user feedback
						}
					}
				}
			}
			if(this.selectedProfile == null)
			{
				if(this.AlllProfilesChkdLB.Items.Count>0)
				{
					this.AlllProfilesChkdLB.SetSelected(0, true);
					this.selectedProfile = (VehicleProfile) this.allVehicleProfiles[0];
				}
			}
			if(this.AlllProfilesChkdLB.Items.Count== 0)//there is notihng to select
			{
				disableUserControls(false);  //keep the add profile button enabled - disable everytihng else
			}
			#endregion profiles load
			MAIN_WINDOW.isVirtualNodes = false;  //prevnets editing of nodes table until vitural checkbox is checked
			this.helpProvider1.HelpNamespace = MAIN_WINDOW.ApplicationDirectoryPath + @"\DWHelp.chm";  //judetemp
			this.helpProvider1.SetHelpKeyword(this,"Advanced Users: Overview");
			//this.helpProvider1.SetHelpNavigator(this.AlllProfilesChkdLB,HelpNavigator.TableOfContents);
			this.helpProvider1.SetHelpNavigator(this.AlllProfilesChkdLB, HelpNavigator.Topic);
			this.helpProvider1.SetHelpKeyword(this.AlllProfilesChkdLB, "judekeyword");
			
			markSelectedLanguage();

		}
		private void markSelectedLanguage()
		{
			if(MAIN_WINDOW.DWConfigFile.DWlanguage.lang.ToUpper() == "ENGLISH")
			{
				this.RBEnglish.Checked = true;
			}
			else if(MAIN_WINDOW.DWConfigFile.DWlanguage.lang.ToUpper() == "FRENCH")
			{
				this.RBFrench.Checked = true;
			}
			else if (MAIN_WINDOW.DWConfigFile.DWlanguage.lang.ToUpper() == "JAPANESE")
			{
				this.RBJapanese.Checked = true;
			}
			else if (MAIN_WINDOW.DWConfigFile.DWlanguage.lang.ToUpper() == "KOREAN")
			{
				this.RBKorean.Checked = true;
			}
		}
		private void formatControls(System.Windows.Forms.Control topControl )
		{
			#region format individual controls
			topControl.ForeColor = SCCorpStyle.SCForeColour;
			topControl.Font = new System.Drawing.Font("Arial", 8F);
			if ( topControl.GetType().Equals( typeof( Button ) ) ) 
			{
				topControl.BackColor = SCCorpStyle.buttonBackGround;
			}
			else if ( topControl.GetType().Equals( typeof( DataGrid ) ) ) 
			{
				DataGrid myDG = (DataGrid) topControl;
				this.sysInfo.formatDataGrid(myDG);
				myDG.Resize +=new EventHandler(myDG_Resize);
			}
			else if ( topControl.GetType().Equals( typeof( VScrollBar ) ) ) 
			{
				topControl.VisibleChanged +=new EventHandler(topControl_VisibleChanged); 
			}
			else if ( topControl.GetType().Equals( typeof( HScrollBar ) ) ) 
			{
//judetemp				topControl.Height = 0;
			}
			else if ( topControl.GetType().Equals( typeof( GroupBox ) ) ) 
			{
				topControl.Font = new System.Drawing.Font("Arial", 8F, FontStyle.Bold);
			}
			#endregion format individual controls
			foreach(Control control in topControl.Controls) 
			{
				formatControls(control);
			}
		}
		private void OPTIONS_WINDOW_Load(object sender, System.EventArgs e)
		{
			dataGrid1DefaultHeight = this.dataGrid1.Height;
			if(SystemInfo.errorSB.Length>0)
			{
				sysInfo.displayErrorFeedbackToUser("Errors occured when reading available profiles");
			}
		}
		private void refreshSetupTable()
		{
			#region clear out the existing rows
			this.setUpTable.Clear();
			for (int i = 0;i<SCCorpStyle.maxConnectedDevices;i++)
			{
				DataRow row = this.setUpTable.NewRow();
				this.setUpTable.Rows.Add(row);
			}
			#endregion clear out the existing rows
			for(int i = 0;i<selectedProfile.myCANNodes.Count;i++)
			{
				VPCanNode cannode = (VPCanNode) selectedProfile.myCANNodes[i];
				this.setUpTable.Rows[i][(int) virtualSetupCols.nodeID] = cannode.nodeid;
				this.setUpTable.Rows[i][(int) virtualSetupCols.productCode] = cannode.productcode;
				this.setUpTable.Rows[i][(int) virtualSetupCols.revisionNo] = cannode.revisionnumber;
				this.setUpTable.Rows[i][(int) virtualSetupCols.vendorID] = cannode.vendorid;
				this.setUpTable.Rows[i][(int) virtualSetupCols.Master] = cannode.master;
				//now get the underlying index No for the EDS filename
				for (int j= 0;j<MAIN_WINDOW.availableEDSInfo.Count;j++)
				{
					AvailableNodesWithEDS EDSdevInfo = (AvailableNodesWithEDS) MAIN_WINDOW.availableEDSInfo[j];
					if((EDSdevInfo.productNumber == cannode.productcode)
						&& (EDSdevInfo.revisionNumber == cannode.revisionnumber)
						&& (EDSdevInfo.vendorNumber == cannode.vendorid))
					{
						this.setUpTable.Rows[i][(int) virtualSetupCols.EDS] = j+1;  //will be decodeds into text overlay within the column class
						break;
					}
				}
			}
			this.dataGrid1.Invalidate();
		}
		#endregion Intitialisation

		#region Profiles Methods
		#region button press event handlers
		private void addProfileBtn_Click_1(object sender, System.EventArgs e)
		{
			if(selectedProfile != null)
			{
				this.vpXMLSerializer.Save(selectedProfile, selectedProfile.ProfilePath);
			}
			bool uniqueFileNameFound = false;
			int fileOffset = 0;
			string newfilename = "";
			while(uniqueFileNameFound == false)
			{
				newfilename = "new profile" + fileOffset.ToString() + ".xml";
				bool test = File.Exists(profilesDirPath + newfilename);
				if(File.Exists(profilesDirPath + newfilename) == false)
				{
					uniqueFileNameFound = true;
					break;
				}
				if(fileOffset++>100)
				{
					//sometihng went wrong - mop up and get out
					SystemInfo.errorSB.Append("\nunable to create new profile. Delete some profiles and then try again");
					return;
				}
			}
			VehicleProfile vehicleProfile = new VehicleProfile();  //dummy name will be exchanged for proper file when user enters a pukka name
			//now create the actual xml file
			this.vpXMLSerializer.Save(vehicleProfile, profilesDirPath + newfilename);
			vehicleProfile.ProfilePath = profilesDirPath + newfilename;
			//add a reference to this profile to this and the MAIN_WINDOW ArrayLists
			MAIN_WINDOW.DWConfigFile.vehicleprofiles.Add(vehicleProfile.ProfilePath);   //put it in a loose, extendable collection of profiles
			this.allVehicleProfiles.Add(vehicleProfile);
			this.selectedProfile = vehicleProfile;
			//first deselect all items
			for(int i= 0;i<this.AlllProfilesChkdLB.Items.Count;i++)
			{
				this.AlllProfilesChkdLB.SetSelected(i, false);
			}
			this.AlllProfilesChkdLB.Items.Add(selectedProfile.ProfileName);
			this.AlllProfilesChkdLB.SetSelected(this.AlllProfilesChkdLB.Items.Count-1, true);
			this.enableUserControls();
		}
		private void deleteProfileBtn_Click(object sender, System.EventArgs e)
		{
			if(this.AlllProfilesChkdLB.SelectedIndex != -1)  //belt and braces
			{
				string message = "This will permanently remove the selected profile";
				DialogResult result = Message.Show(this, message, "Warning", MessageBoxButtons.OKCancel,
					MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
				if(result == DialogResult.Cancel)
				{
					return;
				}
				else if (result == DialogResult.OK)
				{
					VehicleProfile profile = (VehicleProfile) this.allVehicleProfiles[this.AlllProfilesChkdLB.SelectedIndex];
					int profileInd = this.AlllProfilesChkdLB.SelectedIndex;
					MAIN_WINDOW.DWConfigFile.vehicleprofiles.RemoveAt(profileInd);
					this.allVehicleProfiles.RemoveAt(profileInd);
					this.AlllProfilesChkdLB.Items.RemoveAt(profileInd);
					try
					{
						File.Delete(profile.ProfilePath);  //delete the underlying XML file
					}
					catch(Exception mye)
					{
						#region user feedback
						SystemInfo.errorSB.Append(profile.ProfilePath);
						SystemInfo.errorSB.Append("\nReported error: ");
						SystemInfo.errorSB.Append(mye.Message);
						SystemInfo.errorSB.Append("\n");
						sysInfo.displayErrorFeedbackToUser("Unable to delete profile file\n");
						#endregion user feedback
					}
				}
				if(this.AlllProfilesChkdLB.Items.Count>0)
				{
					this.AlllProfilesChkdLB.SelectedItem = this.AlllProfilesChkdLB.Items[0];
				}
			}
		}
		private void updateNodesFromTable()
		{
			setUpTable.AcceptChanges();
			this.selectedProfile.myCANNodes = new ArrayList();
			foreach (DataRow sourceRow in setUpTable.Rows)  
			{
				uint nodeID = System.Convert.ToUInt32(sourceRow[(int) virtualSetupCols.nodeID].ToString());
				if((nodeID >0) && (nodeID <128))  //only add vlaid nodes - note we use out of range value (0xffffffff) to denote 'none'
				{
					VPCanNode cannode = new VPCanNode();
					//add this to the list
					cannode.vendorid = System.Convert.ToUInt32(sourceRow[(int) virtualSetupCols.vendorID].ToString());
					cannode.productcode = System.Convert.ToUInt32(sourceRow[(int) virtualSetupCols.productCode].ToString());
					cannode.revisionnumber = System.Convert.ToUInt32(sourceRow[(int) virtualSetupCols.revisionNo].ToString());
					cannode.nodeid = nodeID;//System.Convert.ToUInt32(sourceRow[(int) virtualSetupCols.nodeID].ToString());
					cannode.master = System.Convert.ToBoolean(sourceRow[(int) virtualSetupCols.Master].ToString());  
					this.selectedProfile.myCANNodes.Add(cannode);
				}
			}
		}
		private void testConnectBtn_Click(object sender, System.EventArgs e)
		{
			if(this.selectedProfile.ProfilePath != this.selectedProfile.XMLFilePath)
			{
				this.changeProfileName();
			}
			this.setUpTable.AcceptChanges();
			DriveWizard.MAIN_WINDOW.findingSystem = true;
			DriveWizard.SELECT_PROFILE.editProfilesRequired = true;  //to force treeview update on exit.
			this.TopMost = false;  //allow error wndos to be displayed
			disableUserControls(true);
            resetUserFeedback();
			MAIN_WINDOW.isVirtualNodes = this.virtualCB.Checked;
			connect();
		}
		#endregion button press event handlers
		#region profile name Text box
		private void profileNameTB_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if( (((Keys)e.KeyValue) == Keys.Return) || (((Keys)e.KeyValue) == Keys.Enter) )
			{
				changeProfileName();
			}
			else if (((Keys)e.KeyValue) == Keys.Escape) 
			{
				this.profileNameTB.Text = this.selectedProfile.ProfileName;
			}
		}

		private void profileNameTB_Leave(object sender, System.EventArgs e)
		{
			changeProfileName();
		}
		#endregion profile name Text box
		private void changeProfileName()
		{
			if(this.AlllProfilesChkdLB.SelectedIndex != -1)
			{
				#region change file name to match the profile name
				string newpath = this.profilesDirPath + this.profileNameTB.Text + ".xml";
				if(newpath != this.selectedProfile.ProfilePath)
				{
					if(File.Exists(this.selectedProfile.ProfilePath))
					{
						try
						{
							File.Move(this.selectedProfile.ProfilePath, newpath);  //rename the xml file for finding next time
							//change the corresponding DWConfig list item
						}
						catch( Exception ex)
						{
							SystemInfo.errorSB.Append("\nUnable to re-name profile file\n");
							SystemInfo.errorSB.Append("Reported error: ");
							SystemInfo.errorSB.Append(ex.Message);
							this.profileNameTB.Text = this.selectedProfile.ProfileName ;
							return; 
						}
					}
					else
					{
						//error condition - mop up and get out
						SystemInfo.errorSB.Append("\nUnable to change profile name. XML file not found");
						return;
					}
					MAIN_WINDOW.DWConfigFile.vehicleprofiles[this.AlllProfilesChkdLB.SelectedIndex] = newpath;
					this.selectedProfile.ProfilePath = newpath;
					#region update checked list box item
					int temp = this.AlllProfilesChkdLB.SelectedIndex;
					if(temp != -1)
					{
						//Sadly re-naming the checkedist item can only be done by removal and reinsertion
						bool wasChecked = this.AlllProfilesChkdLB.GetItemChecked(temp);
						string tempString = this.profileNameTB.Text;
						this.AlllProfilesChkdLB.Items.RemoveAt(temp);
						this.AlllProfilesChkdLB.Items.Insert(temp, tempString);
						this.AlllProfilesChkdLB.SetItemChecked(temp, wasChecked);
					}
					#endregion update checked list box item
				}
				#endregion change file name to match the profile name
			}
		}
		private void baudrateCB_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			if(selectedProfile != null)
			{
				this.selectedProfile.baud.baudrate = (BaudRate) this.baudrateCB.SelectedIndex;
				this.selectedProfile.baud.rate = this.selectedProfile.baud.baudrate.ToString();  //judetemp
				this.selectedProfile.baud.rateString = this.baudrateCB.SelectedItem.ToString();
				if(this.selectedProfile.baud.baudrate == BaudRate._unknown)
				{
					this.selectedProfile.baud.searchAll = true;  //save back to profile for consistency
					this.checkBox1.Checked = true;
					this.checkBox1.Enabled = false;
				}
				else
				{
					this.checkBox1.Enabled = true;
				}
			this.vpXMLSerializer.Save(selectedProfile, selectedProfile.ProfilePath);
			}
		}

		private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
		{
			if(selectedProfile != null)
			{
				this.selectedProfile.baud.searchAll = this.checkBox1.Checked;
				this.vpXMLSerializer.Save(selectedProfile, selectedProfile.ProfilePath);
			}
		}
		private void virtualCB_CheckedChanged(object sender, System.EventArgs e)
		{
			if(this.virtualCB.Checked == true) //virtual nodes
			{
				MAIN_WINDOW.isVirtualNodes = true;
				this.selectedProfile.connectAsVirtual = true;  //to remeber when we switch between profiles
				this.connectionLabel.Text = "Click table to setup virtual nodes";
			}
			else
			{
				MAIN_WINDOW.isVirtualNodes = false;
				this.selectedProfile.connectAsVirtual = false;
				this.connectionLabel.Text = "";
			}
		}
        //DR38000263 - new check box allows user to explicity select whether to view master objects on slave
        private void showAllcb_CheckedChanged(object sender, EventArgs e)
        {
            //remember to update when we switch between profiles
            if (selectedProfile != null)
            {
                MAIN_WINDOW.showMasterObjectsOnSlave = showAllcb.Checked;
                this.selectedProfile.showMasterObjectsOnSlave = showAllcb.Checked;
                this.vpXMLSerializer.Save(selectedProfile, selectedProfile.ProfilePath);
            }
        }

		#region User Id text Box
		private void UserIdTB_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if( (((Keys)e.KeyValue) == Keys.Return) || (((Keys)e.KeyValue) == Keys.Enter) )
			{
				if(selectedProfile != null)
				{
					this.selectedProfile.login.userid = this.UserIdTB.Text;
					this.vpXMLSerializer.Save(selectedProfile, selectedProfile.ProfilePath);
				}
			}
			else if (((Keys)e.KeyValue) == Keys.Escape) 
			{
				this.UserIdTB.Text = this.selectedProfile.login.userid;
			}
		}

		private void UserIdTB_Leave(object sender, System.EventArgs e)
		{
			if(selectedProfile != null)
			{
				this.selectedProfile.login.userid = this.UserIdTB.Text;
				this.vpXMLSerializer.Save(selectedProfile, selectedProfile.ProfilePath);
			}
		}

		#endregion User ID textbox
		#region password Text Box
		private void passwordTB_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if( (((Keys)e.KeyValue) == Keys.Return) || (((Keys)e.KeyValue) == Keys.Enter) )
			{
				if(selectedProfile != null)
				{
					this.selectedProfile.login.password = this.passwordTB.Text;
					this.vpXMLSerializer.Save(selectedProfile, selectedProfile.ProfilePath);
				}
			}
			else if (((Keys)e.KeyValue) == Keys.Escape) 
			{
				this.passwordTB.Text = this.selectedProfile.login.password;
			}
		}

		private void passwordTB_Leave(object sender, System.EventArgs e)
		{
			if(selectedProfile != null)
			{
				this.selectedProfile.login.password = this.passwordTB.Text;
				this.vpXMLSerializer.Save(selectedProfile, selectedProfile.ProfilePath);
			}
		}

		#endregion password Text Box
		private void OPTIONS_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.progressBar1.Value = progressBar1.Minimum;
			this.progressBar1.Visible = false;
		}

		#region CheckBox List
		private void checkedListBox1_DoubleClick(object sender, System.EventArgs e)
		{
			return;
		}

		private void checkedListBox1_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			this.AlllProfilesChkdLB.ItemCheck -=new ItemCheckEventHandler(checkedListBox1_ItemCheck);  //prevent re-entry
			for(int i = 0;i<this.AlllProfilesChkdLB.Items.Count;i++)
			{
				this.AlllProfilesChkdLB.SetItemChecked(i, false);
			}
			this.AlllProfilesChkdLB.SetItemChecked(e.Index, true);
			//now overwite the Config CLass active profile - will be saved on application exit
			MAIN_WINDOW.DWConfigFile.activeProfilePath.profile = this.selectedProfile.ProfilePath;
			ObjectXMLSerializer DWconfigXMLSerializer = new ObjectXMLSerializer();
			DWconfigXMLSerializer.Save(MAIN_WINDOW.DWConfigFile, MAIN_WINDOW.UserDirectoryPath + @"\DWConfig.xml");
			this.AlllProfilesChkdLB.ItemCheck +=new ItemCheckEventHandler(checkedListBox1_ItemCheck);
		}
		#endregion CheckBox List
		#endregion Profiles Methods

		#region CAN bus connection methods
        private void connect()
        {
            MAIN_WINDOW.currentProfile = this.selectedProfile;
            DriveWizard.MAIN_WINDOW.findingSystem = true;
            //clearout old system ready to connect
            this.sysInfo.clearSystem();
            #region Virtual Nodes
            if (this.virtualCB.Checked == true)
            {
                updateNodesFromTable();
                SCCorpStyle.VirtualCANnodes = new ArrayList();
                SCCorpStyle.VirtualCANnodes = this.selectedProfile.myCANNodes;
            }
            #endregion Virtual Nodes
            this.progressBar1.Visible = true;

            if (sysInfo.CANcomms.VCI.CANAdapterHWIntialised == false)
            {
                statusBar1.Text = "Initializing CAN adapter";
            }
            if (selectedProfile.baud.baudrate == BaudRate._unknown)
            {
                listenForBaudWrapper();
            }
            else
            {
                this.sysInfo.CANcomms.systemBaud = (BaudRate)selectedProfile.baud.baudrate;

                if (sysInfo.CANcomms.VCI.CANAdapterBaud != this.sysInfo.CANcomms.systemBaud)
                {
                    statusBar1.Text = "Setting CAN adapter baud rate";
                    sysInfo.CANcomms.VCI.SetupCAN(sysInfo.CANcomms.systemBaud, 0x00, 0x00); //DR38000268 filter correction
                }

                if (this.selectedProfile.baud.searchAll == false)  //
                {
                    refindLastSystemWrapper();
                }
                else  //can be set true in above clause if can't detec tlast known system 
                {
                    searchAllAtUserBaudWrapper();
                }
            }
        }

        private void listenForBaudWrapper()
        {
            this.statusBar1.Text = "Listening to bus traffic";
            parentSB.Panels[2].Text = "Listening to bus traffic";
            #region listen in to bus traffic thread
            tConnect = new Thread(new ThreadStart(sysInfo.listenInForBaudRate));
            tConnect.Name = "ListenIn";
            tConnect.Priority = ThreadPriority.Normal;
            tConnect.IsBackground = true;
#if DEBUG
            System.Console.Out.WriteLine("Thread: " + tConnect.Name + " started");
#endif
            timer1.Enabled = true;
            tConnect.Start();
            #endregion listen in to bus traffic thread
        }
        private void refindLastSystemWrapper()
        {
            this.statusBar1.Text = "Searching for previous nodes";
            parentSB.Panels[2].Text = "Searching for previous nodes";
            SystemInfo.itemCounter1 = 0;

            tConnect = new Thread(new ThreadStart(sysInfo.refindLastSystem));
            tConnect.Name = "RefindLast";
            tConnect.Priority = ThreadPriority.Normal;
            tConnect.IsBackground = true;
#if DEBUG
            System.Console.Out.WriteLine("Thread: " + tConnect.Name + " started");
#endif
            timer1.Enabled = true;	
            tConnect.Start();
        }
		private void searchAllAtUserBaudWrapper()
		{
			#region interrogate bus at user selected baud rate
            statusBar1.Text = "Searching for nodes at " + this.baudrateCB.SelectedItem.ToString();
            parentSB.Panels[2].Text = "Searching for nodes at " + this.baudrateCB.SelectedItem.ToString();
            this.progressBar1.Value = this.progressBar1.Minimum;  //reset progress bar becaseu showUserCOntrols not called here
            tConnect = new Thread(new ThreadStart(sysInfo.findSystemAtUserBaudRate));
            tConnect.Name = "UserBaud";
            tConnect.Priority = ThreadPriority.Normal;
            tConnect.IsBackground = true;
#if DEBUG
            System.Console.Out.WriteLine("Thread: " + tConnect.Name + " started");
#endif
			timer1.Enabled = true;
            tConnect.Start();
			#endregion interrogate bus at user selected baud rate
		}

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            if (tConnect != null)
            {
                //update the progress bar
                this.progressBar1.Maximum = SystemInfo.itemCounterMax;
                this.progressBar1.Value = Math.Min(SystemInfo.itemCounter1, progressBar1.Maximum);
                this.progressBar1.Update();
                //if thread stopped
                if ((tConnect.ThreadState & System.Threading.ThreadState.Stopped) > 0)
                {
                    timer1.Enabled = false;
                    switch (tConnect.Name)
                    {
                        case "ListenIn":
                            #region auto hunting for nodes
                            if (this.sysInfo.CANcomms.systemBaud == BaudRate._unknown)
                            {
                                statusBar1.Text = "Failed to autodetect baud rate";
                            }
                            else
                            {
                                //We know our baud rate so lets find our units
                                selectedProfile.baud.baudrate = this.sysInfo.CANcomms.systemBaud;
                                selectedProfile.baud.rate = MAIN_WINDOW.currentProfile.baud.baudrate.ToString();
                                selectedProfile.baud.rateString = MAIN_WINDOW.baudrates[(int)MAIN_WINDOW.currentProfile.baud.baudrate];
                                MAIN_WINDOW.currentProfile = this.selectedProfile;
                                this.baudrateCB.SelectedIndex = (int)MAIN_WINDOW.currentProfile.baud.baudrate;
                                tConnect = null;
                                searchAllAtUserBaudWrapper();
                                return; //do not update the feedback yet
                            }
                            break;
                            #endregion auto hunting for nodes
                    }
                    tConnect = null;
                    UpdateUserFeedback();
                    UpdateAndSaveProfileTable();
                    enableUserControls();
                }
            }
        }


		#endregion CAN bus connection methods

		#region finalisation
		private void closeBtn_Click(object sender, System.EventArgs e)
		{
			this.Hide();
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
		#endregion finalisation

		#region minor methods
		private void UpdateUserFeedback()
		{
            if (this.sysInfo.nodes.Length == 0)
            {
                resetUserFeedback();
                #region User status mesaage
                if (sysInfo.CANcomms.systemBaud == BaudRate._unknown)
                {
                    statusBar1.Text = "No CAN bus traffic detected";
                    parentSB.Panels[2].Text = "No CAN bus traffic detected";
                }
                else
                {
                    statusBar1.Text = "No nodes found at " + MAIN_WINDOW.currentProfile.baud.rateString + " baud";
                    parentSB.Panels[2].Text = "No nodes found at " + MAIN_WINDOW.currentProfile.baud.rateString + " baud";
                }
                #endregion User status mesaage
            }
            else
            {
                #region CANbus feedback
                if (feedback == DIFeedbackCode.DISuccess)
                {
                    this.statusBar1.Text = "System found OK";
                }
                else
                {
                    this.statusBar1.Text = "Not all nodes found. Error code: " + feedback.ToString();
                }
                if (this.sysInfo.nodes.Length == 1)
                {
                    #region display user feedback for single CAN node found
                    string realOrVirtual = " baud ";
                    if (MAIN_WINDOW.isVirtualNodes == true)
                    {
                        realOrVirtual = " baud (virtual)";
                    }
                    else
                    {
                        realOrVirtual = " baud (real)";
                    }
                    this.parentSB.Panels[0].Text = MAIN_WINDOW.currentProfile.baud.rateString + realOrVirtual;
                    try
                    {
                        Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\ConnectedOK.ico");
                        parentSB.Panels[0].Icon = icon;
                    }
                    catch
                    {
#if DEBUG
                        Message.Show("Missing file: " + MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\ConnectedOK.ico");
#endif
                    }
                    #endregion display user feedback for single CAN node found
                }
                else //just change "node" to "nodes"
                {
                    #region display user feedback for multiple CNa nodes found
                    string realOrVirtual = " baud ";
                    if (MAIN_WINDOW.isVirtualNodes == true)
                    {
                        realOrVirtual = " baud (virtual)";
                    }
                    else
                    {
                        realOrVirtual = " baud (real)";
                    }
                    this.parentSB.Panels[0].Text = MAIN_WINDOW.currentProfile.baud.rateString + realOrVirtual;
                    try
                    {
                        Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\ConnectedOK.ico");
                        parentSB.Panels[0].Icon = icon;
                    }
                    catch
                    {
#if DEBUG
                        Message.Show("Missing file: " + MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\ConnectedOK.ico");
#endif
                    }
                    #endregion dispaly user feedback for multiple CNa nodes found
                }
                #endregion CANbus feedback
                #region login  & feedback
                //first determine whether we need to login
                bool loginRequired = false;  //ensure we login only once - remember this cna be a counter event
                for (int i = 0; i < this.sysInfo.nodes.Length; i++)
                {
                    if (sysInfo.nodes[i].isSevconApplication() == true)
                    {
                        loginRequired = true;
                        break;
                    }
                }
                if (loginRequired == true)
                {
                    this.statusBar1.Text = "Logging in to controllers";
                    autoLogin();
                    if (this.sysInfo.systemAccess == 0)
                    {
                        #region update user feedback
                        parentSB.Panels[1].Text = "SEVCON: Login Failed";
                        try
                        {
                            Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\notloggedIn.ico");
                            parentSB.Panels[1].Icon = icon;
                        }
                        catch
                        {
#if DEBUG
                            Message.Show("Missing file: " + MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\notloggedIn.ico");
#endif
                        }
                        statusBar1.Text = "Login Failed";
                        #endregion update user feedback
                    }
                    else
                    {
                        #region update user feedback
                        parentSB.Panels[1].Text = MAIN_WINDOW.UserIDs[this.sysInfo.systemAccess].ToString();
                        try
                        {
                            Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\loggedIn.ico");
                            parentSB.Panels[1].Icon = icon;
                        }
                        catch
                        {
#if DEBUG
                            Message.Show("Missing file: " + MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\loggedIn.ico");
#endif
                        }
                        statusBar1.Text = "Login OK";
                        #endregion update user feedback
                    }
                }
                #endregion login feedback
                #region User status message
                this.parentSB.Panels[2].Text = "";
                #endregion User status message
                DriveWizard.MAIN_WINDOW.findingSystem = false;
            }
            if (SystemInfo.errorSB.Length > 0)
            {
                sysInfo.displayErrorFeedbackToUser("Connect Error:");   //DR38000260
            }
            else if (sysInfo.CANcomms.nodeList.Length == 0)
            {
                sysInfo.displayErrorFeedbackToUser("There are no connected nodes. \nCheck CAN bus connections");
            }
			updateToolbar();
		}
        private void resetUserFeedback()
        {
            #region CANbus feedback
            parentSB.Panels[0].Text = "not connected";
            try
            {
                Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\notConnected.ico");
                parentSB.Panels[0].Icon = icon;
            }
            catch
            {
#if DEBUG
                Message.Show("Missing file: " + MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\notConnected.ico");
#endif
            }
            #endregion CANbus feedback
            #region login feedback
            parentSB.Panels[1].Text = "SEVCON: Not logged in";
            try
            {
                Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\notloggedIn.ico");
                parentSB.Panels[1].Icon = icon;
            }
            catch
            {
#if DEBUG
                Message.Show("Missing file: " + MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\notloggedIn.ico");
#endif
            }
            #endregion login feedback
            statusBar1.Text = "";
            parentSB.Panels[2].Text = "";

        }
		private void UpdateAndSaveProfileTable()
		{
			if(MAIN_WINDOW.isVirtualNodes == false)
			{
				this.selectedProfile.myCANNodes = new ArrayList();  //covers undefined scenario as well
					for(int i = 0;i<this.sysInfo.nodes.Length;i++)
					{
						VPCanNode cannode = new VPCanNode();
						cannode.vendorid = this.sysInfo.nodes[i].vendorID;
						cannode.productcode = this.sysInfo.nodes[i].productCode;
						cannode.revisionnumber = this.sysInfo.nodes[i].revisionNumber;
						cannode.EDSorDCFfilepath = this.sysInfo.nodes[i].EDSorDCFfilepath;
						cannode.master = this.sysInfo.nodes[i].masterStatus;
						cannode.nodeid = (uint)this.sysInfo.nodes[i].nodeID;
						this.selectedProfile.myCANNodes.Add(cannode);
					}
				this.refreshSetupTable();
			}
			this.vpXMLSerializer.Save(this.selectedProfile, this.selectedProfile.ProfilePath);
		}
		private void autoLogin()
		{
			feedback = DIFeedbackCode.DISuccess;
			uint numericUserID = 0;  
			uint numericpassword = 0;
			try
			{
				numericUserID = System.Convert.ToUInt32(selectedProfile.login.userid);
				numericpassword = System.Convert.ToUInt32(selectedProfile.login.password);
			}
			catch
			{
				SystemInfo.errorSB.Append("\nInvalid User ID or password entered");
				feedback = DIFeedbackCode.DIGeneralFailure;
			}
			if(feedback == DIFeedbackCode.DISuccess)
			{
				feedback = this.sysInfo.loginToSystem(numericUserID,numericpassword);
			}
			if(feedback != DIFeedbackCode.DISuccess) //eithe rconversion or login failurr
			{
				sysInfo.displayErrorFeedbackToUser("Failed to login to one or more SEVCON devices");
			}
		}
		
		private void disableUserControls(bool disableAddButton)
		{
			if(disableAddButton == true)  //someone hit the connect button
			{
				this.addProfile_btn.Enabled = false;
			}
			else
			{
				this.addProfile_btn.Enabled = true;
			}
			this.deleteProfileBtn.Enabled = false;
			this.testConnectBtn.Enabled = false;
			this.passwordTB.Enabled = false;
			this.UserIdTB.Enabled = false;
			this.baudrateCB.Enabled = false;
			this.profileNameTB.Enabled = false;
			this.checkBox1.Enabled = false;
            this.showAllcb.Enabled = false;     //DR38000263
			this.progressBar1.Value = this.progressBar1.Minimum;
			this.AlllProfilesChkdLB.Enabled = false;
			this.virtualCB.Enabled = false;
		}
		private void enableUserControls()
		{
			this.addProfile_btn.Enabled = true;
			this.deleteProfileBtn.Enabled = true;
			this.testConnectBtn.Enabled = true;
			this.passwordTB.Enabled = true;
			this.UserIdTB.Enabled = true;
			this.baudrateCB.Enabled = true;
			this.profileNameTB.Enabled = true;
			this.checkBox1.Enabled = true;
            this.showAllcb.Enabled = true;      //DR38000263
			this.progressBar1.Value = this.progressBar1.Minimum;
			this.progressBar1.Visible = false;
			this.AlllProfilesChkdLB.Enabled = true;
			this.tabControl1.Enabled = true;
			this.virtualCB.Enabled = true;
		}

		private void disablePreOpAndOpToolbarBtns()
		{
			#region set back to defualt vlaues to start
			toolbar.Buttons[0].Enabled = false; //no access restrictions for third parties
			toolbar.Buttons[1].Enabled = false;
			#endregion set back to defualt vlaues to start
		}
		private void enablePreOpAndOpToolbarBtns()
		{
			#region set back to defualt vlaues to start
			toolbar.Buttons[0].Enabled = true;
			toolbar.Buttons[1].Enabled = true;
			#endregion set back to defualt values to start
		}
		private void updateToolbar()
		{
			this.disablePreOpAndOpToolbarBtns();
			#region Pre-Op and Op request buttons
			for(int i = 2;i<this.sysInfo.nodes.Length+2;i++)
			{
				if(this.sysInfo.nodes[i-2].manufacturer == Manufacturer.THIRD_PARTY)
				{
					enablePreOpAndOpToolbarBtns();
					break;
				}

				else if ((this.sysInfo.nodes[i-2].isSevconApplication()==true)
					&&   (this.sysInfo.systemAccess>= SCCorpStyle.AccLevel_PreOp))
				{
					enablePreOpAndOpToolbarBtns();
					break;
				}
			}
			#endregion Pre-Op and Op request buttons
		}
		#endregion minor methods

		private void dataGrid1_Click(object sender, System.EventArgs e)
		{
			DataGrid.HitTestInfo hti;
			Point pt;
			pt = this.dataGrid1.PointToClient(Control.MousePosition);
			hti = this.dataGrid1.HitTest(pt);
			if((hti.Type == DataGrid.HitTestType.Cell)  
				&& (hti.Column == (int) virtualSetupCols.Master)
				&&(this.virtualCB.CheckState == CheckState.Checked))
			{
				SCbaseRODataGridBoolColumn masterCol = (SCbaseRODataGridBoolColumn)	this.virtualSetupTS.GridColumnStyles[(int) virtualSetupCols.Master];
				bool masterStatus = System.Convert.ToBoolean(this.setUpTable.Rows[hti.Row][(int) virtualSetupCols.Master]);
				if (System.Convert.ToUInt32(this.setUpTable.Rows[hti.Row][(int) virtualSetupCols.nodeID]) != 0)
				{
					#region modify the value of the master status cell only if user has selected a valid Node Id
					masterCol._readonly = false;
					if(masterStatus == true)
					{
						this.setUpTable.Rows[hti.Row][(int) virtualSetupCols.Master] = false;
					}
					else
					{
						this.setUpTable.Rows[hti.Row][(int) virtualSetupCols.Master] = true;
					}
					masterCol._readonly = true;
					#endregion modify the value of the maste rstatus cell only if user has selected a valid Node Id
				}
			}
		}
		private void createSetUptable()
		{
			this.setUpTable = new virtualSetupTable();  //effectively clears any old table and adds eight rows
			this.dataGrid1.DataSource = this.setUpTable.DefaultView;
			int [] colWidths  = new int[percents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, percents, 0, dataGrid1DefaultHeight);
			this.virtualSetupTS = new virtualSetupTableStyle(colWidths);
			virtualSetupTS.MappingName = setUpTable.TableName;
			this.dataGrid1.TableStyles.Clear();
			this.dataGrid1.TableStyles.Add(this.virtualSetupTS);
		}

		private void AlllProfilesChkdLB_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(this.AlllProfilesChkdLB.SelectedItems.Count>0)  //gets called once when old one is deselected an again for new one to be selected
			{
				//do this before we change to new selected profile
				if(this.selectedProfile != null)
				{
				updateNodesFromTable();
				}
				int selInd = this.AlllProfilesChkdLB.SelectedIndex;
				selectedProfile = (VehicleProfile) this.allVehicleProfiles[this.AlllProfilesChkdLB.SelectedIndex];
				#region update detail fields
				this.profileNameTB.Text = this.selectedProfile.ProfileName;
				this.UserIdTB.Text = selectedProfile.login.userid.ToString();
				this.passwordTB.Text = selectedProfile.login.password.ToString();
				this.baudrateCB.SelectedIndex = 0;
				for(ushort i= 0;i<this.baudrateCB.Items.Count;i++)
				{
					if(this.baudrateCB.Items[i].ToString() == selectedProfile.baud.rateString)
					{
						this.baudrateCB.SelectedIndex = i;
						break;
					}
				}
				this.baudrateCB.SelectedItem = this.selectedProfile.baud.rate;
				if(this.selectedProfile.baud.baudrate == BaudRate._unknown)
				{
					this.selectedProfile.baud.searchAll = true;  //save back to profile for consistency
					this.checkBox1.Checked = true;
					this.checkBox1.Enabled = false;
				}
				else
				{
					this.checkBox1.Enabled = true;
				}
				this.checkBox1.Checked = this.selectedProfile.baud.searchAll;
				this.refreshSetupTable();
				this.virtualCB.Checked = this.selectedProfile.connectAsVirtual;

                //DR38000263 setup "show master objects on slave" checkbox according to profile
                this.showAllcb.Checked = this.selectedProfile.showMasterObjectsOnSlave;
				#endregion update detail fields
			}
		}

		private void topControl_VisibleChanged(object sender, EventArgs e)
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
							handleResizeDataGrid(myDG, myVscroll.Width);
						}
						catch(Exception e5)
						{
							SystemInfo.errorSB.Append("handleResizeDataGrid() casued exception: " + e5.Message + " " + e5.InnerException);
						}
					}
					else
					{
						try
						{
							handleResizeDataGrid(myDG, 0);
						}
						catch(Exception e6)
						{
							SystemInfo.errorSB.Append("handleResizeDataGrid() casued exception: " + e6.Message + " " + e6.InnerException);
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
					if ( c.GetType().Equals( typeof( VScrollBar ) ) ) 
					{
						if(c.Visible == true)
						{
							VScrollBarwidth = c.Width;  //remove width of scroll bar from overall calc
						}
						break;
					}
				try
				{
					handleResizeDataGrid(myDG, VScrollBarwidth);
				}
				catch(Exception e7)
				{
					SystemInfo.errorSB.Append("handleResizeDataGrid() casued exception: " + e7.Message + " " + e7.InnerException);
				}

			}
		}
		private void handleResizeDataGrid(DataGrid myDG, int VScrollBarWidth)
		{
			if(myDG.TableStyles.Count>0)
			{
				int [] ColWidths = null;
				if(myDG == this.dataGrid1) 
				{
					ColWidths  = new int[percents.Length];
					ColWidths  = this.sysInfo.calculateColumnWidths(this.dataGrid1, percents, VScrollBarWidth, dataGrid1DefaultHeight);
				}
				else
				{
					return;
				}
				for (int i = 0;i<ColWidths.Length;i++)
				{
					myDG.TableStyles[0].GridColumnStyles[i].Width = ColWidths[i];
				}	
				myDG.Invalidate();
		}
        }
	}

	public class virtualSetupTableStyle : SCbaseTableStyle
	{
		public virtualSetupTableStyle(int [] colWidths)
		{
			ArrayList myList = new ArrayList();
			comboSource noneEntry = new comboSource("none", 0xFFFFFFFF);
			myList.Add(noneEntry);
			for(ushort j = 1;j<128;j++)
			{
				comboSource source = new comboSource(j.ToString(), j);
				myList.Add(source);
			}
			virtualComboCol nodeIDcol = new virtualComboCol((int) virtualSetupCols.nodeID,myList);
			nodeIDcol.MappingName = virtualSetupCols.nodeID.ToString();
			nodeIDcol.HeaderText = "node ID";
			nodeIDcol.Width = colWidths[(int) virtualSetupCols.nodeID];  
			GridColumnStyles.Add(nodeIDcol);

			ArrayList EDSFileNamesAL = new ArrayList();
			EDSFileNamesAL.Add(noneEntry);
			ArrayList VendorIDsAL = new ArrayList();
			VendorIDsAL.Add(noneEntry);
			ArrayList prodCodesAL = new ArrayList();
			prodCodesAL.Add(noneEntry);
			ArrayList revNosAL = new ArrayList();
			revNosAL.Add(noneEntry);
			for(ushort j = 1;j<MAIN_WINDOW.availableEDSInfo.Count+1;j++)
			{
				AvailableNodesWithEDS EDSdevInfo = (AvailableNodesWithEDS) MAIN_WINDOW.availableEDSInfo[j-1];
				int startInd = EDSdevInfo.EDSFilePath.LastIndexOf(@"\") + 1;
				string filename = EDSdevInfo.EDSFilePath.Substring(startInd);
				int extIndex = filename.IndexOf('.');
				filename = filename.Substring(0,extIndex);
				comboSource source = new comboSource(filename, j);
				EDSFileNamesAL.Add(source);
				source = new comboSource(EDSdevInfo.vendorName, EDSdevInfo.vendorNumber);
				VendorIDsAL.Add(source);
				source = new comboSource(EDSdevInfo.productName, EDSdevInfo.productNumber);
				prodCodesAL.Add(source);
				source = new comboSource("0x" + EDSdevInfo.revisionNumber.ToString("X").PadLeft(8,'0'),EDSdevInfo.revisionNumber);
				revNosAL.Add(source);
			}
			virtualComboCol EDScol = new virtualComboCol((int) virtualSetupCols.EDS,EDSFileNamesAL);
			EDScol.MappingName = virtualSetupCols.EDS.ToString();
			EDScol.HeaderText = "EDS filename";
			EDScol.Width = colWidths[(int) virtualSetupCols.EDS];  
			GridColumnStyles.Add(EDScol);
			
			virtualComboCol vendorIDCol = new virtualComboCol((int) virtualSetupCols.vendorID,VendorIDsAL);
			vendorIDCol.MappingName = virtualSetupCols.vendorID.ToString();
			vendorIDCol.HeaderText = "Vendor";
			vendorIDCol.Width = colWidths[(int) virtualSetupCols.vendorID];  
			GridColumnStyles.Add(vendorIDCol);

			virtualComboCol prodcodeCol = new virtualComboCol((int) virtualSetupCols.productCode,prodCodesAL);
			prodcodeCol.MappingName = virtualSetupCols.productCode.ToString();
			prodcodeCol.HeaderText = "product name";
			prodcodeCol.Width = colWidths[(int) virtualSetupCols.productCode];  
			GridColumnStyles.Add(prodcodeCol);

			virtualComboCol revNoCol = new virtualComboCol((int) virtualSetupCols.revisionNo,revNosAL);
			revNoCol.MappingName = virtualSetupCols.revisionNo.ToString();
			revNoCol.HeaderText = "revision No";
			revNoCol.Width = colWidths[(int) virtualSetupCols.revisionNo];  
			GridColumnStyles.Add(revNoCol);

			SCbaseRODataGridBoolColumn masterCol = new SCbaseRODataGridBoolColumn((int) virtualSetupCols.Master);
			masterCol.MappingName = virtualSetupCols.Master.ToString();
			masterCol.HeaderText = "Master";
			masterCol.Width = colWidths[(int) virtualSetupCols.Master];
			GridColumnStyles.Add(masterCol);
		}
	}
	public class virtualSetupTable : DataTable
	{
		public virtualSetupTable()
		{
			this.Columns.Add(virtualSetupCols.nodeID.ToString(), typeof(System.UInt32));// Add the Column to the table //ie pump, LH controller etc
			this.Columns[virtualSetupCols.nodeID.ToString()].DefaultValue = 0xFFFFFFFF;
			this.Columns.Add(virtualSetupCols.EDS.ToString(), typeof(System.UInt32));// Add the column to the table. Node baud rate
			this.Columns[virtualSetupCols.EDS.ToString()].DefaultValue = 0xFFFFFFFF;
			this.Columns.Add(virtualSetupCols.vendorID.ToString(), typeof(System.UInt32));
			this.Columns[virtualSetupCols.vendorID.ToString()].DefaultValue = 0xFFFFFFFF;
			this.Columns.Add(virtualSetupCols.productCode.ToString(), typeof(System.UInt32));
			this.Columns[virtualSetupCols.productCode.ToString()].DefaultValue = 0xFFFFFFFF;
			this.Columns.Add(virtualSetupCols.revisionNo.ToString(), typeof(System.UInt32));
			this.Columns[virtualSetupCols.revisionNo.ToString()].DefaultValue = 0xFFFFFFFF;
			this.Columns.Add(virtualSetupCols.Master.ToString(), typeof(System.Boolean));
			this.Columns[virtualSetupCols.Master.ToString()].DefaultValue = false;
			this.Columns[virtualSetupCols.Master.ToString()].AllowDBNull = false;
			for(int i =0;i<SCCorpStyle.maxConnectedDevices;i++)
			{
				DataRow row = this.NewRow();
				this.Rows.Add(row);
			}
			this.AcceptChanges();
			this.DefaultView.AllowDelete = false;
			this.DefaultView.AllowNew = false;
		}
	}
}
