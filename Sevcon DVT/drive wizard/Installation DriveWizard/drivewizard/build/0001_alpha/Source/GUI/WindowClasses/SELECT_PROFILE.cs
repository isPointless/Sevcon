/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.71$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:08/10/2008 14:03:14$
	$ModDate:08/10/2008 10:33:28$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	This window displays all the CANbus connection profiles previously copied/configured 
    by the current user (defining a CAN system baud and connected node IDs), allowing the 
    user the option to connect with a selected profile or to edit or test a file (which opens 
    the OPTIONS window). 

REFERENCES    

MODIFICATION HISTORY
    $Log:  50144: SELECT_PROFILE.cs 

   Rev 1.71    08/10/2008 14:03:14  ak
 TRR COD0013 post-test fixes


   Rev 1.70    23/09/2008 23:17:44  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.69    12/03/2008 13:00:18  ak
 All DI Thread.Priority increased to Normal from BelowNormal (needed to run
 VCI3).


   Rev 1.68    18/02/2008 14:18:50  jw
 VCI2 CAN methods renamed and merged for consistency with VCI3 - towards back
 compatibility


   Rev 1.67    18/02/2008 09:28:40  jw
 VCI2 CAN methods moved form communications to VCI 2 and renamed for
 consistency with VCI3 - towards back compatibility


   Rev 1.66    15/02/2008 11:44:38  jw
 Reduncadnt code line removed to reduce compiler warnigns


   Rev 1.65    13/02/2008 15:01:34  jw
 More VCI3. Now works generally OK but more testing/optimisation required. 


   Rev 1.64    12/02/2008 08:46:14  jw
 Ongoing VCI3 work. Options and Select profiel windows changed to simplify
 threading and improve feedback.  Prog bar vlaue determination line made
 exception proof. Max and current values used by progress bars determined
 within DI for encapsulation and values reflect activitiy better.


   Rev 1.63    25/01/2008 10:47:32  jw
 VCI3 and Vista functionality files merge - more testing required


   Rev 1.62    21/01/2008 12:03:02  jw
 File merge for VCI3/ Vista. These changes are those to go in all builds


   Rev 1.61    13/12/2007 20:43:54  ak
 sysInfo.refindLastSystem() in connect() is now called on a thread. If the
 expected nodes's aren't there, 5 retries at 3s timeouts each leads to 15s of
 DW "hanging". The thread allows the user to change their mind and close the
 window.


   Rev 1.60    05/12/2007 22:12:44  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Data;
using Ixxat.Vci3;           //IXXAT conversion Jude
using Ixxat.Vci3.Bal;       //IXXAT conversion Jude
using Ixxat.Vci3.Bal.Can;   //IXXAT conversion Jude

namespace DriveWizard
{
	public class SELECT_PROFILE : System.Windows.Forms.Form
	{
		#region Form Controls definitions
		private System.Windows.Forms.ComboBox comboBox1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Button editProfilesBtn;
		private System.Windows.Forms.Button connectBtn;
		private System.Windows.Forms.Button OfflineBtn;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.CheckBox virtualCB;
		private System.Windows.Forms.StatusBar statusBar1;
		#endregion Form Controls definitions

		#region my definitions
		DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
		private SystemInfo sysInfo;
		private ToolBar toolbar = null;
		private Thread tConnect;
		private int indexOfDefault = 0;
		System.Windows.Forms.StatusBar parentSB = null;
		public static bool editProfilesRequired = false;
		private System.Windows.Forms.CheckBox CBdefault;
		ObjectXMLSerializer vpXMLSerializer;
		#endregion my definitions

		#region initialisation

		public SELECT_PROFILE( SystemInfo passed_systemInfo, StatusBar passedSB, ToolBar passed_ToolBar)
		{
			InitializeComponent();
			sysInfo = passed_systemInfo;
			this.toolbar = passed_ToolBar;
			parentSB = passedSB;
			editProfilesRequired = false;
			MAIN_WINDOW.currentProfile = null;
			#region format controls to SEVCON style
			foreach (Control myControl in this.Controls)
			{
				if((myControl.GetType().ToString()) == "System.Windows.Forms.Button")
				{
					myControl.BackColor = SCCorpStyle.buttonBackGround;
				}
			}
			#endregion format controls to SEVCON style
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SELECT_PROFILE));
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.connectBtn = new System.Windows.Forms.Button();
            this.editProfilesBtn = new System.Windows.Forms.Button();
            this.OfflineBtn = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.virtualCB = new System.Windows.Forms.CheckBox();
            this.CBdefault = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Location = new System.Drawing.Point(8, 48);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(408, 21);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.SelectionChangeCommitted += new System.EventHandler(this.comboBox1_SelectionChangeCommitted);
            // 
            // connectBtn
            // 
            this.connectBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.connectBtn.Location = new System.Drawing.Point(8, 112);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(104, 48);
            this.connectBtn.TabIndex = 1;
            this.connectBtn.Text = "&Connect";
            this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
            // 
            // editProfilesBtn
            // 
            this.editProfilesBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.editProfilesBtn.Location = new System.Drawing.Point(160, 112);
            this.editProfilesBtn.Name = "editProfilesBtn";
            this.editProfilesBtn.Size = new System.Drawing.Size(104, 48);
            this.editProfilesBtn.TabIndex = 2;
            this.editProfilesBtn.Text = "&Edit profiles";
            this.editProfilesBtn.Click += new System.EventHandler(this.editProfileBtn_Click_1);
            // 
            // OfflineBtn
            // 
            this.OfflineBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OfflineBtn.Location = new System.Drawing.Point(312, 112);
            this.OfflineBtn.Name = "OfflineBtn";
            this.OfflineBtn.Size = new System.Drawing.Size(104, 48);
            this.OfflineBtn.TabIndex = 3;
            this.OfflineBtn.Text = "Work &Offline";
            this.OfflineBtn.Click += new System.EventHandler(this.workOfflineBtn_Click_1);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(408, 24);
            this.label1.TabIndex = 4;
            this.label1.Text = "Choose a vehicle profile to connect to the CANbus";
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 188);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(423, 22);
            this.statusBar1.TabIndex = 5;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(0, 168);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(424, 24);
            this.progressBar1.TabIndex = 6;
            // 
            // virtualCB
            // 
            this.virtualCB.Location = new System.Drawing.Point(8, 80);
            this.virtualCB.Name = "virtualCB";
            this.virtualCB.Size = new System.Drawing.Size(224, 24);
            this.virtualCB.TabIndex = 7;
            this.virtualCB.Text = "Connect as virtual nodes";
            // 
            // CBdefault
            // 
            this.CBdefault.Location = new System.Drawing.Point(253, 80);
            this.CBdefault.Name = "CBdefault";
            this.CBdefault.Size = new System.Drawing.Size(160, 24);
            this.CBdefault.TabIndex = 8;
            this.CBdefault.Text = "Use as default";
            this.CBdefault.CheckedChanged += new System.EventHandler(this.CBdefault_CheckedChanged);
            // 
            // SELECT_PROFILE
            // 
            this.AcceptButton = this.connectBtn;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(423, 210);
            this.Controls.Add(this.CBdefault);
            this.Controls.Add(this.virtualCB);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OfflineBtn);
            this.Controls.Add(this.editProfilesBtn);
            this.Controls.Add(this.connectBtn);
            this.Controls.Add(this.comboBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(430, 170);
            this.Name = "SELECT_PROFILE";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Select Vehicle Profile";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.SELECT_PROFILE_Closing);
            this.Load += new System.EventHandler(this.SELECT_PROFILE_Load);
			this.Closed += new System.EventHandler(this.SELECT_PROFILE_Closed);
            this.ResumeLayout(false);

		}
		#endregion
		private void SELECT_PROFILE_Load(object sender, System.EventArgs e)
		{
			this.Focus();
			for(int i = 0;i<MAIN_WINDOW.DWConfigFile.vehicleprofiles.Count;i++)
			{
				string filepath = (string) MAIN_WINDOW.DWConfigFile.vehicleprofiles[i];
				int fileNamestart = filepath.LastIndexOf(@"\");
				string ProfileName = filepath.Substring(fileNamestart+ 1);
				ProfileName = ProfileName.TrimEnd(".xml".ToCharArray());
				this.comboBox1.Items.Add(ProfileName);
				if(MAIN_WINDOW.DWConfigFile.activeProfilePath.profile == filepath)
				{
					this.comboBox1.SelectedIndex = i;
					indexOfDefault = i;
					this.CBdefault.CheckedChanged -=new EventHandler(CBdefault_CheckedChanged);
					this.CBdefault.Checked = true;
					this.CBdefault.CheckedChanged +=new EventHandler(CBdefault_CheckedChanged);
				}
			}
			if(this.comboBox1.SelectedIndex == -1)  //should not occur
			{
				this.comboBox1.SelectedIndex = 0;
			}
		}

		#endregion initialisation

		#region user interaction
		private void workOfflineBtn_Click_1(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void editProfileBtn_Click_1(object sender, System.EventArgs e)
		{
			editProfilesRequired = true;
			this.Close();
		}

		private void connectBtn_Click(object sender, System.EventArgs e)
		{
			//try setting up new vehicle profile
			vpXMLSerializer = new ObjectXMLSerializer();			
			//Load the customer object from the XML file using our custom class...
			MAIN_WINDOW.currentProfile = new VehicleProfile();
			try
			{
				MAIN_WINDOW.currentProfile = (VehicleProfile) this.vpXMLSerializer.Load(MAIN_WINDOW.currentProfile, (string) MAIN_WINDOW.DWConfigFile.vehicleprofiles[this.comboBox1.SelectedIndex] );
			}
			catch
			{
				//judetemp - following code is untested
				Message.Show("Profile could not be read. Choose another profile or Edit Profiles");
				MAIN_WINDOW.DWConfigFile.vehicleprofiles.RemoveAt(this.comboBox1.SelectedIndex);
                MAIN_WINDOW.showMasterObjectsOnSlave = false; //DR38000263
				this.comboBox1.Items.RemoveAt(this.comboBox1.SelectedIndex);  //judetemp - may not be needed
				if(this.comboBox1.Items.Count>0)
				{
					this.comboBox1.SelectedIndex = 0; //revert to first one
				}
				else
				{
					Message.Show("No usable profiles. You must create a new profile");
					editProfilesRequired = true;
					this.Close();
				}
				return;
			}
			//set the foollowing after serialization -> ProfilePath is reset during serialization
			MAIN_WINDOW.currentProfile.ProfilePath = (string) MAIN_WINDOW.DWConfigFile.vehicleprofiles[this.comboBox1.SelectedIndex];
			MAIN_WINDOW.currentProfile.baud.baudrate = (BaudRate) Enum.Parse(typeof(BaudRate), MAIN_WINDOW.currentProfile.baud.rate); //serialization doe snot handle enums properly
            MAIN_WINDOW.showMasterObjectsOnSlave = MAIN_WINDOW.currentProfile.showMasterObjectsOnSlave;  //DR38000263
			disablePreOpAndOpToolbarBtns();
			this.disableUserControls();
			if(this.virtualCB.Checked == true)
			{
				MAIN_WINDOW.isVirtualNodes = true;
				//now overwrite the virtual nodes in SCCorpStyle for now
				SCCorpStyle.VirtualCANnodes = new ArrayList();
				SCCorpStyle.VirtualCANnodes = MAIN_WINDOW.currentProfile.myCANNodes;
			}
			else
			{
				MAIN_WINDOW.isVirtualNodes = false;
			}

			//now decide which sort of connetion to attempt - most commonly used one first
			connect();
		}

		private void connect()
		{
			DriveWizard.MAIN_WINDOW.findingSystem = true;
			this.sysInfo.clearSystem();
            this.statusBar1.Text = "Initializing CAN adapter";
            if (sysInfo.CANcomms.VCI.CANAdapterHWIntialised == false)
            {
                statusBar1.Text = "Initializing CAN adapter";
            }
            if (MAIN_WINDOW.currentProfile.baud.baudrate == BaudRate._unknown)
            {
                listenForBaudWrapper();
            }
			else
			{
                sysInfo.CANcomms.systemBaud = MAIN_WINDOW.currentProfile.baud.baudrate;

                if (sysInfo.CANcomms.VCI.CANAdapterBaud != this.sysInfo.CANcomms.systemBaud)
                {
                    statusBar1.Text = "Setting CAN adapter baud rate";
                    sysInfo.CANcomms.VCI.SetupCAN(sysInfo.CANcomms.systemBaud, 0x00, 0x00); //DR38000268 filter correction
                }

                if (MAIN_WINDOW.currentProfile.baud.searchAll == false)
				{
                    refindLastSystemWrapper();
				}
				else  
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
            statusBar1.Text = "Searching for nodes at " + MAIN_WINDOW.currentProfile.baud.rateString;
            parentSB.Panels[2].Text = "Searching for nodes at " + MAIN_WINDOW.currentProfile.baud.rateString;
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
                                MAIN_WINDOW.currentProfile.baud.baudrate = this.sysInfo.CANcomms.systemBaud;
                                MAIN_WINDOW.currentProfile.baud.rate = MAIN_WINDOW.currentProfile.baud.baudrate.ToString();
                                MAIN_WINDOW.currentProfile.baud.rateString = MAIN_WINDOW.baudrates[(int)MAIN_WINDOW.currentProfile.baud.baudrate];
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

		#endregion user interaction

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
                        realOrVirtual = " (virtual)";
                    }
                    else
                    {
                        realOrVirtual = " (real)";
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
				MAIN_WINDOW.currentProfile.myCANNodes.Clear(); //clear out old list of nodes
				MAIN_WINDOW.currentProfile.myCANNodes.Clear(); //clear out old list of nodes
				for(int i = 0;i<this.sysInfo.nodes.Length;i++)
				{
					VPCanNode cannode = new VPCanNode();
					cannode.vendorid = this.sysInfo.nodes[i].vendorID;
					cannode.productcode = this.sysInfo.nodes[i].productCode;
					cannode.revisionnumber = this.sysInfo.nodes[i].revisionNumber;
					cannode.EDSorDCFfilepath = this.sysInfo.nodes[i].EDSorDCFfilepath;
					cannode.master = this.sysInfo.nodes[i].masterStatus;
					cannode.nodeid = (uint)this.sysInfo.nodes[i].nodeID;
					MAIN_WINDOW.currentProfile.myCANNodes.Add(cannode);
				}
			}
			this.vpXMLSerializer.Save(MAIN_WINDOW.currentProfile,MAIN_WINDOW.currentProfile.ProfilePath);
		}

		private void autoLogin()
		{
			//first determine whether we need to login
			bool SevconAppNodePresent = false;
			for( int i = 0;i< this.sysInfo.nodes.Length;i++)
			{
				if(sysInfo.nodes[i].isSevconApplication()==true)
				{
					SevconAppNodePresent = true;
					break;
				}
			}
			if(SevconAppNodePresent == true)
			{
				uint numericUserID = 0;  //don't default to zero - it is a valid User Id
				uint numericpassword = 0;
				try
				{
					numericUserID = System.Convert.ToUInt32(MAIN_WINDOW.currentProfile.login.userid);
					numericpassword = System.Convert.ToUInt32(MAIN_WINDOW.currentProfile.login.password);
				}
				catch
				{
				}
				feedback = this.sysInfo.loginToSystem(numericUserID,numericpassword);
				if(feedback == DIFeedbackCode.DISuccess)
				{
					if(this.sysInfo.systemAccess ==0)
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
						parentSB.Panels[2].Text = "Login Failed";
						#endregion update user feedback
					}
					else
					{
						#region update user feedback
						parentSB.Panels[1].Text = MAIN_WINDOW.UserIDs[this.sysInfo.systemAccess].ToString();
						try
						{
							Icon icon = new Icon(MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\loggedIn.ico");
							this.parentSB.Panels[1].Icon = icon;
						}
						catch 
						{
#if DEBUG
							Message.Show("Missing file: " + MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\icons\loggedIn.ico");
#endif
						}
						#endregion update user feedback
					}
				}
				else
				{
					SystemInfo.errorSB.Insert(0,"Failed to login to one or more SEVCON devices\n" );
					Form errMsg = new ErrorMessageWindow(SystemInfo.errorSB.ToString());
					SystemInfo.errorSB = new System.Text.StringBuilder();
					errMsg.ShowDialog();
				}
				#region update the toolbar
				if(this.sysInfo.systemAccess>= SCCorpStyle.AccLevel_PreOp)
				{
					this.enablePreOpAndOpToolbarBtns();
				}
				#endregion update the toolbar
			}
		}

		private void enableUserControls()
		{
			progressBar1.Value = this.progressBar1.Minimum;
			this.progressBar1.Visible = false;
			this.connectBtn.Enabled = true;
			this.editProfilesBtn.Enabled = true;
			this.comboBox1.Enabled = true;
			this.OfflineBtn.Enabled = true;
			this.virtualCB.Enabled = true;
			this.CBdefault.Enabled = true;
			if(this.sysInfo.nodes.Length == 0)
			{
				label1.Text = "No connected devices found. Check connections or edit this profile";
			}
			else
			{
				label1.Text = "";
                //We found something so close and move on
                this.Hide();
                this.Close();
			}
		}
		private void disableUserControls()
		{
			this.connectBtn.Enabled = false;
			this.editProfilesBtn.Enabled = false;
			this.OfflineBtn.Enabled = false;
			this.comboBox1.Enabled = false;
			progressBar1.Visible = true;
			progressBar1.Value = progressBar1.Minimum;
			this.virtualCB.Enabled = false;
			this.CBdefault.Enabled = false;
			this.label1.Text = "Please wait";
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
			toolbar.Buttons[0].Enabled = true; //no access restrictions for third parties
			toolbar.Buttons[1].Enabled = true;
			#endregion set back to defualt vlaues to start
		}
		private void updateToolbar()
		{
			disablePreOpAndOpToolbarBtns();
			#region Pre-Op and Op request buttons
			for(int i = 2;i<this.sysInfo.nodes.Length+2;i++)
			{
				if(this.sysInfo.nodes[i-2].manufacturer == Manufacturer.THIRD_PARTY)
				{
					enablePreOpAndOpToolbarBtns(); //no access restrictions for third parties
					break;
				}

				else if((this.sysInfo.nodes[i-2].isSevconApplication()==true)		
					&&   (this.sysInfo.systemAccess>= SCCorpStyle.AccLevel_PreOp))
				{
					enablePreOpAndOpToolbarBtns();
					break;
				}
			}
			#endregion Pre-Op and Op request buttons
			#region Node Status buttons
			#endregion Node Status buttons
		}
		#endregion minor methods

		#region finalisation
		private void SELECT_PROFILE_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			progressBar1.Visible = false;
			this.statusBar1.Text = "";
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

		private void comboBox1_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			this.CBdefault.CheckedChanged -=new EventHandler(CBdefault_CheckedChanged);
			if(this.comboBox1.SelectedIndex == indexOfDefault)
			{
				this.CBdefault.Checked = true;  //this is the current default
			}
			else
			{
				this.CBdefault.Checked = false;  //not the default
			}
			this.CBdefault.CheckedChanged +=new EventHandler(CBdefault_CheckedChanged);
		}

		private void CBdefault_CheckedChanged(object sender, EventArgs e)
		{
			if(this.CBdefault.CheckState == CheckState.Checked)
			{
				this.indexOfDefault = this.comboBox1.SelectedIndex;  //user wants this one to be the default.
			}
			else if (this.CBdefault.CheckState == CheckState.Unchecked)
			{
				this.indexOfDefault = 0;  //o				
			}
			string filepath = (string) MAIN_WINDOW.DWConfigFile.vehicleprofiles[indexOfDefault];
			MAIN_WINDOW.DWConfigFile.activeProfilePath.profile = filepath;

			ObjectXMLSerializer DWconfigXMLSerializer = new ObjectXMLSerializer();
			DWconfigXMLSerializer.Save(MAIN_WINDOW.DWConfigFile, MAIN_WINDOW.UserDirectoryPath + @"\DWConfig.xml");
		}

		private void SELECT_PROFILE_Closed(object sender, System.EventArgs e)
		{
//			int test = 9;
		}
	}
}

