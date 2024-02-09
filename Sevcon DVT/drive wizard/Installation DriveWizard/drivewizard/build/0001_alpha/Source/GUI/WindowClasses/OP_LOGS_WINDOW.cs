/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.54$
	$Version:$
	$Author:jw$
	$ProjectName:DriveWizard$ 

	$RevDate:17/03/2008 13:13:58$
	$ModDate:17/03/2008 13:11:36$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	This window displays the operational logs (Sevcon and customer) retrieved from 
    a Sevcon node, showing min & max currents, voltages and temperatures. It is possible 
    to reset these logs.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36783: OP_LOGS_WINDOW.cs 

   Rev 1.54    17/03/2008 13:13:58  jw
 Logs null obejct handling complete. Still needs progrees bar steps linking to
 odSub timeout values


   Rev 1.53    14/03/2008 10:58:54  jw
 Log handling revised, Read of Log releated CAN items now done from GUI ( to
 allow later DI /Ixxat unlinking) . Processing of recevied logs code and event
 lists moved to sysInfo - reduces memory usage ( one method per application
 rather than per node)  Some newer Sevocn devices do not hav ethese logs. Also
 event list methods now have product code input - to allow us to have seperate
 Event lists for eg displays etc. Note:some error hanlding eg null detection
 still needed but check back in for working set with DI 


   Rev 1.52    13/03/2008 08:57:30  jw
 All CAN data transfer now done on single sperate thread to improve inteface
 in event of no response and to all eventual removal of DI wait for response


   Rev 1.51    12/03/2008 13:43:54  ak
 All DI threads have ThreadPriority increased to Normal from BelowNormal
 (needed when running VCI3)


   Rev 1.50    05/12/2007 22:12:42  ak
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


namespace DriveWizard
{
	#region enumerated types
	#endregion enumerated types

	#region Operational Logs form Class
/// <summary>
/// 
/// </summary>
	public class OP_LOGS_WINDOW : System.Windows.Forms.Form
	{
		#region controls declarations
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button closeBtn;
		private System.ComponentModel.IContainer components;
        private System.Windows.Forms.StatusBar statusBar1;
        private ProgressBar progressBar1;
        private ToolBar toolbar = null;
        private System.Windows.Forms.Button resetCustBtn;  //judetemp - change to formatting??
		#endregion

		#region my declarations
		#region local copy of parameters passed from MDIParent
		private SystemInfo sysInfo = null;
        private nodeInfo node = null;
//		private int nodeIndex = 0;
		private string selectedNodeText = "";
		#endregion local copy of parameters passed from MDIParent
		private DWOpLogTable table;
		#region threads
		private Thread DIThread = null;
		#endregion threads
		private OperationalLog [] custLog = new OperationalLog[0];
		private OperationalLog []  SevconLog = new OperationalLog[0];
		private DriveWizard.OperationalLogFormatting scaling;
        private Button resetSevconBtn;
		private int dataGrid1DefaultHeight = 0;
        private bool getBothLogs = true;

        private ODItemData resetCustLogSub = null;
        private ODItemData resetSevLogSub = null; 
        private ODItemData custOpLogSub = null;
        private Label label2;
        private ODItemData sevOpLogSub = null;

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
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.menuItem10 = new System.Windows.Forms.MenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.resetCustBtn = new System.Windows.Forms.Button();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.resetSevconBtn = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(11, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(763, 16);
            this.label1.TabIndex = 16;
            // 
            // closeBtn
            // 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeBtn.Location = new System.Drawing.Point(659, 338);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(120, 25);
            this.closeBtn.TabIndex = 15;
            this.closeBtn.Text = "&Close window";
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // dataGrid1
            // 
            this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dataGrid1.CaptionBackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dataGrid1.CaptionForeColor = System.Drawing.Color.MidnightBlue;
            this.dataGrid1.CaptionText = "Operational Logs";
            this.dataGrid1.DataMember = "";
            this.dataGrid1.Enabled = false;
            this.dataGrid1.FlatMode = true;
            this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Location = new System.Drawing.Point(8, 58);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.ParentRowsVisible = false;
            this.dataGrid1.PreferredColumnWidth = 250;
            this.dataGrid1.ReadOnly = true;
            this.dataGrid1.RowHeadersVisible = false;
            this.dataGrid1.Size = new System.Drawing.Size(771, 236);
            this.dataGrid1.TabIndex = 12;
            this.dataGrid1.Resize += new System.EventHandler(this.dataGrid1_Resize);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem5});
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 0;
            this.menuItem5.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem6,
            this.menuItem7,
            this.menuItem10});
            this.menuItem5.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
            this.menuItem5.Text = "&File";
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 0;
            this.menuItem6.Text = "&Open Log from file";
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 1;
            this.menuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem8,
            this.menuItem9});
            this.menuItem7.Text = "&Save log to file";
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 0;
            this.menuItem8.Text = "Customer Log";
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 1;
            this.menuItem9.Text = "SEVCON Log";
            // 
            // menuItem10
            // 
            this.menuItem10.Index = 2;
            this.menuItem10.Text = "&Print log";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // resetCustBtn
            // 
            this.resetCustBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.resetCustBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.resetCustBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.resetCustBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetCustBtn.Location = new System.Drawing.Point(8, 338);
            this.resetCustBtn.Name = "resetCustBtn";
            this.resetCustBtn.Size = new System.Drawing.Size(192, 25);
            this.resetCustBtn.TabIndex = 17;
            this.resetCustBtn.Text = "&Reset Customer log";
            this.resetCustBtn.Visible = false;
            this.resetCustBtn.Click += new System.EventHandler(this.resetCustBtn_Click);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 391);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(792, 22);
            this.statusBar1.TabIndex = 19;
            // 
            // resetSevconBtn
            // 
            this.resetSevconBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.resetSevconBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.resetSevconBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.resetSevconBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetSevconBtn.Location = new System.Drawing.Point(216, 338);
            this.resetSevconBtn.Name = "resetSevconBtn";
            this.resetSevconBtn.Size = new System.Drawing.Size(192, 25);
            this.resetSevconBtn.TabIndex = 18;
            this.resetSevconBtn.Text = "&Reset Sevcon log";
            this.resetSevconBtn.Visible = false;
            this.resetSevconBtn.Click += new System.EventHandler(this.resetSevconBtn_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 368);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(792, 23);
            this.progressBar1.TabIndex = 20;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(11, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(768, 18);
            this.label2.TabIndex = 21;
            this.label2.Text = "label2";
            // 
            // OP_LOGS_WINDOW
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
            this.ClientSize = new System.Drawing.Size(792, 413);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.resetSevconBtn);
            this.Controls.Add(this.resetCustBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.dataGrid1);
            this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Menu = this.mainMenu1;
            this.Name = "OP_LOGS_WINDOW";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Operational Logs";
            this.Load += new System.EventHandler(this.OP_LOGS_WINDOW_Load);
            this.Closed += new System.EventHandler(this.OP_LOGS_WINDOW_Closed);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.OP_LOGS_WINDOW_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: OP_LOGS_WINDOW
		///		 *  Description     : constructor for form
		///		 *  Parameters      : Event args
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		///			 </summary>
        public OP_LOGS_WINDOW(SystemInfo passed_systemInfo, int nodeNum, string nodeText, ToolBar passed_ToolBar)
        {
            InitializeComponent();
            sysInfo = passed_systemInfo;
            node = sysInfo.nodes[nodeNum];
            selectedNodeText = nodeText;
            this.sysInfo.formatDataGrid(this.dataGrid1);
            dataGrid1.CaptionText = "Operational logs for " + nodeText;
            this.toolbar = passed_ToolBar;
            MAIN_WINDOW.appendErrorInfo = false;
            resetCustLogSub = sysInfo.nodes[nodeNum].getODSubFromObjectType(SevconObjectType.CUST_OPERATIONAL_MONITOR, 0x01);
            resetSevLogSub = sysInfo.nodes[nodeNum].getODSubFromObjectType(SevconObjectType.SEVCON_OPERATIONAL_MONITOR, 0x01);
            custOpLogSub = node.getODSubFromObjectType(SevconObjectType.DOMAIN_UPLOAD, SCCorpStyle.CustomerOpMonitorSubObject);
            sevOpLogSub = node.getODSubFromObjectType(SevconObjectType.DOMAIN_UPLOAD, SCCorpStyle.SevconOpMonitorSubObject); 
            MAIN_WINDOW.appendErrorInfo = true;
        }
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: OP_LOGS_WINDOW_Load
		///		 *  Description     : Event Handler for form load event
		///		 *  Parameters      : System event arguments 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void OP_LOGS_WINDOW_Load(object sender, System.EventArgs e)
		{
			this.Text = "DriveWizard: Operational Logs";
			table = new DWOpLogTable();
			dataGrid1.DataSource = table;
			applyTableStyle();
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
            this.progressBar1.Maximum = 0;
            if (custOpLogSub != null)
            {
                this.startDIThread("requestcustOpLogWrapper", requestcustOpLogWrapper);
            }
            else if( this.sevOpLogSub != null)
            {
                this.startDIThread("requestSevOpLogWrapper", requestSevOpLogWrapper);
            }
            else
            {
                this.showUserControls();
            }

		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: createLogTable
		///		 *  Description     : creates and fills datatable form Data received from DI		///		 *  Parameters      : Event args
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		///			 </summary>
		private void createLogTable()
		{
			DataRow row;
			this.table.Clear();
			string [] OpParamNames = {"Battery Volts", "Capacitor Volts", "Motor current 1 ", "Motor current 2", "Motor speed", "Temperature"};
			Int16 custLogMin = 0, custLogMax = 0, sevconLogMin = 0, sevconLogMax = 0;
			double scalingValue = 1;

			#region add MIN and MAX header row
			row = table.NewRow();
			row[(int) OpLogCol.OpName] = "";
			row[ (int) OpLogCol.CustMin] = "MIN";
			row[ (int) OpLogCol.SCMin] = "MIN";
			row[ (int) OpLogCol.CustMax] = "MAX";
			row[ (int) OpLogCol.SCMax] = "MAX";
			table.Rows.Add(row);
			#endregion add MIN and MAX header row
			
			if ( ( custLog.Length > 0 ) || ( SevconLog.Length > 0 ) )
			{
				//add data to this row
				for(int i = 0;i<OpParamNames.Length;i++)
				{
					row = table.NewRow();
					row[(int) (OpLogCol.OpName)] = OpParamNames[i];
					switch ( i )
					{
						case 0:
							#region battery volts
                            scalingValue = scaling.batteryVoltsScaling;
                            if (custLog.Length > 0)
                            {
                                custLogMin = custLog[0].batteryVoltsMin;
                                custLogMax = custLog[0].batteryVoltsMax;
                                if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.CustMin] = (scalingValue * custLogMin).ToString("G6") + " V";
                                    row[(int)OpLogCol.CustMax] = (scalingValue * custLogMax).ToString("G6") + " V";
                                }
                                else
                                {
                                    row[(int)OpLogCol.CustMin] = custLogMin.ToString("G6") + " V";
                                    row[(int)OpLogCol.CustMax] = custLogMax.ToString("G6") + " V";
                                }
                            }
                            if (SevconLog.Length > 0)
                            {
                                sevconLogMin = SevconLog[0].batteryVoltsMin;
                                sevconLogMax = SevconLog[0].batteryVoltsMax;
                                if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.SCMin] = (scalingValue * sevconLogMin).ToString("G6") + " V";
                                    row[(int)OpLogCol.SCMax] = (scalingValue * sevconLogMax).ToString("G6") + " V";
                                }
                                else
                                {
                                    row[(int)OpLogCol.SCMin] = sevconLogMin.ToString("G6") + " V";
                                    row[(int)OpLogCol.SCMax] = sevconLogMax.ToString("G6") + " V";
                                }
                            }
							#endregion battery volts
							break;
						case 1:
							#region Capacitor volts
                            scalingValue = scaling.capacitorVoltsScaling;
                            if (custLog.Length > 0)
                            {
                                custLogMin = custLog[0].capacitorVoltsMin;
                                custLogMax = custLog[0].capacitorVoltsMax;
                                if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.CustMin] = (scalingValue * custLogMin).ToString("G6") + " V";
                                    row[(int)OpLogCol.CustMax] = (scalingValue * custLogMax).ToString("G6") + " V";
                                }
                                else
                                {
                                    row[(int)OpLogCol.CustMin] = custLogMin.ToString("G6") + " V";
                                    row[(int)OpLogCol.CustMax] = custLogMax.ToString("G6") + " V";
                                }
                            }
                            if (SevconLog.Length > 0)
                            {
                                sevconLogMin = SevconLog[0].capacitorVoltsMin;
                                sevconLogMax = SevconLog[0].capacitorVoltsMax;
                                if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.SCMin] = (scalingValue * sevconLogMin).ToString("G6") + " V";
                                    row[(int)OpLogCol.SCMax] = (scalingValue * sevconLogMax).ToString("G6") + " V";
                                }
                                else
                                {
                                    row[(int)OpLogCol.SCMin] = sevconLogMin.ToString("G6") + " V";
                                    row[(int)OpLogCol.SCMax] = sevconLogMax.ToString("G6") + " V";
                                }
                            }
							#endregion Capacitor volts
							break;
						case 2:
							#region Motor Current 1
                            scalingValue = scaling.motorCurrent1Scaling;
                            if (custLog.Length > 0)
                            {
                                custLogMin = custLog[0].motorCurrent1Min;
                                custLogMax = custLog[0].motorCurrent1Max;
                                if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.CustMin] = (scalingValue * custLogMin).ToString("G6") + " A";
                                    row[(int)OpLogCol.CustMax] = (scalingValue * custLogMax).ToString("G6") + " A";
                                }
                                else
                                {
                                    row[(int)OpLogCol.CustMin] = custLogMin.ToString("G6") + " A";
                                    row[(int)OpLogCol.CustMax] = custLogMax.ToString("G6") + " A";
                                }
                            }
                            if (SevconLog.Length > 0)
                            {
                                sevconLogMin = SevconLog[0].motorCurrent1Min;
                                sevconLogMax = SevconLog[0].motorCurrent1Max;
                                if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.SCMin] = (scalingValue * sevconLogMin).ToString("G6") + " A";
                                    row[(int)OpLogCol.SCMax] = (scalingValue * sevconLogMax).ToString("G6") + " A";
                               }
                                else
                                {
                                    row[(int)OpLogCol.SCMin] = sevconLogMin.ToString("G6") + " A";
                                    row[(int)OpLogCol.SCMax] = sevconLogMax.ToString("G6") + " A";
                               }
                            }
							#endregion Motor Current 1
							break;
						case 3:
							#region Motor Current 2
                            scalingValue = scaling.motorCurrent2Scaling;
                            if (custLog.Length > 0)
                            {
                                custLogMin = custLog[0].motorCurrent2Min;
                                custLogMax = custLog[0].motorCurrent2Max;
                                if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.CustMin] = (scalingValue * custLogMin).ToString("G6") + " A";
                                    row[(int)OpLogCol.CustMax] = (scalingValue * custLogMax).ToString("G6") + " A";
                                }
                                else
                                {
                                    row[(int)OpLogCol.CustMin] = custLogMin.ToString("G6") + " A";
                                    row[(int)OpLogCol.CustMax] = custLogMax.ToString("G6") + " A";
                                }
                            }
                            if (SevconLog.Length > 0)
                            {
                               sevconLogMin = SevconLog[0].motorCurrent2Min;
                                sevconLogMax = SevconLog[0].motorCurrent2Max;
                                if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.SCMin] = (scalingValue * sevconLogMin).ToString("G6") + " A";
                                    row[(int)OpLogCol.SCMax] = (scalingValue * sevconLogMax).ToString("G6") + " A";
                                }
                                else
                                {
                                    row[(int)OpLogCol.SCMin] = sevconLogMin.ToString("G6") + " A";
                                    row[(int)OpLogCol.SCMax] = sevconLogMax.ToString("G6") + " A";
                               }
                            }
 							#endregion
							break;
						case 4:
							#region Motor Speed
                            scalingValue = scaling.motorSpeedScaling;

                            if (custLog.Length > 0)
                            {
                                custLogMin = custLog[0].motorSpeedMin;
                                custLogMax = custLog[0].motorSpeedMax;
                                if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.CustMin] = (scalingValue * custLogMin).ToString("G6") + " RPM";
                                    row[(int)OpLogCol.CustMax] = (scalingValue * custLogMax).ToString("G6") + " RPM";
                               }
                                else
                                {
                                    row[(int)OpLogCol.CustMin] = custLogMin.ToString("G6") + " RPM";
                                    row[(int)OpLogCol.CustMax] = custLogMax.ToString("G6") + " RPM";
                                }
                            }
                            if (SevconLog.Length > 0)
                            {
                                sevconLogMin = SevconLog[0].motorSpeedMin;
                                sevconLogMax = SevconLog[0].motorSpeedMax;
                               if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.SCMin] = (scalingValue * sevconLogMin).ToString("G6") + " RPM";
                                    row[(int)OpLogCol.SCMax] = (scalingValue * sevconLogMax).ToString("G6") + " RPM";
                                }
                                else
                                {
                                    row[(int)OpLogCol.SCMin] = sevconLogMin.ToString("G6") + " RPM";
                                    row[(int)OpLogCol.SCMax] = sevconLogMax.ToString("G6") + " RPM";
                                }
                            }
							#endregion
							break;
						case 5:
							#region temperature

                            scalingValue = scaling.temperatureScaling;
                            if (custLog.Length > 0)
                            {
                                custLogMin = custLog[0].temperatureMin;
                                custLogMax = custLog[0].temperatureMax;
                                if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.CustMin] = (scalingValue * custLogMin).ToString("G6") + " degC";
                                    row[(int)OpLogCol.CustMax] = (scalingValue * custLogMax).ToString("G6") + " degC";
                                }
                                else
                                {
                                    row[(int)OpLogCol.CustMin] = custLogMin.ToString("G6") + " degC";
                                    row[(int)OpLogCol.CustMax] = custLogMax.ToString("G6") + " degC";
                                }
                            }
                            if (SevconLog.Length > 0)
                            {
                                sevconLogMin = SevconLog[0].temperatureMin;
                                sevconLogMax = SevconLog[0].temperatureMax;

                                if (scalingValue != 1.0F)
                                {
                                    row[(int)OpLogCol.SCMin] = (scalingValue * sevconLogMin).ToString("G6") + " degC";
                                    row[(int)OpLogCol.SCMax] = (scalingValue * sevconLogMax).ToString("G6") + " degC";
                                }
                                else
                                {
                                    row[(int)OpLogCol.SCMin] = sevconLogMin.ToString("G6") + " degC";
                                    row[(int)OpLogCol.SCMax] = sevconLogMax.ToString("G6") + " degC";
                               }
                            }
							#endregion
							break;
					} //end switch
					#region insert 'not recorded' enums
					if((row[ (int) OpLogCol.CustMin].ToString().IndexOf("32767") != -1 )
						|| (row[ (int) OpLogCol.CustMin].ToString().IndexOf("-32768") != -1))
					{
						row[ (int) OpLogCol.CustMin] = "Not recorded";
					}
					if((row[ (int) OpLogCol.CustMax].ToString().IndexOf("32767") != -1 )
						|| (row[ (int) OpLogCol.CustMax].ToString().IndexOf("-32768") != -1))
					{
						row[ (int) OpLogCol.CustMax] = "Not recorded";
					}
					if((row[ (int) OpLogCol.SCMin].ToString().IndexOf("32767") != -1 )
						|| (row[ (int) OpLogCol.SCMin].ToString().IndexOf("-32768") != -1))

					{
						row[ (int) OpLogCol.SCMin] = "Not recorded";
					}
					if((row[ (int) OpLogCol.SCMax].ToString().IndexOf("32767") != -1 )
						|| (row[ (int) OpLogCol.SCMax].ToString().IndexOf("-32768") != -1))

					{
						row[ (int) OpLogCol.SCMax] = "Not recorded";
					}
					#endregion insert 'not recorded' enums
					table.Rows.Add(row);
				}
			}
		}

		#endregion

        #region DI Thread wrappers

        private void requestSevOpLogWrapper()
        {
            node.readODValue(this.sevOpLogSub);
        }

        private void requestcustOpLogWrapper()
        {
            node.readODValue(this.custOpLogSub);
        }

        private void ResetCustLogWrapper()
        {
            node.writeODValue(resetCustLogSub, 0x01);
        }

        private void ResetSevLogWrapper()
        {
            node.writeODValue(resetSevLogSub, 0x01);
        }


        #endregion DI Thread wrappers

        #region user interaction zone
        /// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: applyTableStyle
		///		 *  Description     : Reapplies the correct column witdths etc on window resize
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		///			 </summary>
		private void applyTableStyle()
		{
			float [] percents = {0.4F, 0.15F, 0.15F, 0.15F, 0.15F};
			int [] colWidths  = new int[percents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, percents, 0, dataGrid1DefaultHeight);
			OpLogTableStyle tablestyle = new OpLogTableStyle(colWidths);
			this.dataGrid1.TableStyles.Add(tablestyle);//finally attahced the TableStyles to the datagrid
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: dataGrid1_Resize
		///		 *  Description     : Event Handler for resizing of datagrid
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		///			 </summary>
		private void dataGrid1_Resize(object sender, System.EventArgs e)
		{
			applyTableStyle();
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: timer1_Tick
		///		 *  Description     : Event Handler for timer1 tick. Checks for completion of data retrieval thread
		///								and, on completion, puts data into talbe and displays it to user
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		///			 </summary>
		private void timer1_Tick(object sender, System.EventArgs e)
		{
            if ((DIThread.ThreadState & System.Threading.ThreadState.Stopped) > 0)
            {
                timer1.Enabled = false; //kill timer
                progressBar1.Visible = false;
              this.statusBar1.Text = "";
                string threadName = this.DIThread.Name;
                DIThread = null;
                switch( threadName)
                {
                    case "requestcustOpLogWrapper":
                        #region Getting customer log
                        {
                            sysInfo.processOperationalLog(this.custOpLogSub, LogType.CustOpLog, out custLog, out scaling);
                            sysInfo.insertErrorType("Failed to get Customer Log");
                            if (getBothLogs == true)
                            {
                                getBothLogs = false; //do once
                                this.progressBar1.Maximum = 0; //for domains
                                if (this.sevOpLogSub != null)
                                {
                                    this.startDIThread("requestSevOpLogWrapper", requestSevOpLogWrapper);
                                }
                                else
                                {
                                    showUserControls();
                                }
                            }
                            else
                            {
                                showUserControls();
                            }
                            break;
                        }
                    #endregion Getting customer log

                    case "requestSevOpLogWrapper":
                        #region getting Sevcon Op log
                        {
                            sysInfo.insertErrorType("Failed to get Sevcon Log");
                            sysInfo.processOperationalLog(this.sevOpLogSub, LogType.SEVCONOpLog, out SevconLog, out scaling);
                            showUserControls();
                            break;
                        }
                        #endregion getting Sevcon Op log

                    case "ResetCustLogWrapper":
                        #region Reset Cust Op log
                        {
                            sysInfo.insertErrorType("Failed to reset cusotmer Log");
                            this.progressBar1.Maximum = 0; //for domains
                            this.startDIThread("requestcustOpLogWrapper", requestcustOpLogWrapper);
                            break;
                        }
                    #endregion Reset Cust Op log

                    case "ResetSevLogWrapper":
                        #region Reset Sev Op log
                        {
                            sysInfo.insertErrorType("Failed to reset Sevcon Log");
                            this.progressBar1.Maximum = 0; //for domains
                            this.startDIThread("requestSevOpLogWrapper", requestSevOpLogWrapper);
                            break;
                        }
                    #endregion Reset Sev Op log
                }
            }
            else
            {
                //increment progress bar
                #region DI thread in progress
                string threadName = this.DIThread.Name;
                switch (threadName)
                {
                    case "requestSevOpLogWrapper":
                    case "requestcustOpLogWrapper":
                        if (this.progressBar1.Maximum != 0)
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
                            //we can only do this once we have starrted transfer
                            this.progressBar1.Maximum = (int)Math.Min(0x7fffffff, this.sysInfo.CANcomms.SDOReadDomainRxDataSize);//
                        }
                        break;

                    default:
                        if (this.progressBar1.Value < this.progressBar1.Maximum)
                        {
                            progressBar1.Value++;
                        }
                        break;
                }
                #endregion DI thread in progress

            }
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: resetCustBtn_Click
		///		 *  Description     : Event Handler for user request to reset the customer dcopy of the operational log
		///								and, on completion, puts data into talbe and displays it to user
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		///			 </summary>
		private void resetCustBtn_Click(object sender, System.EventArgs e)
		{
			hideUserControls();
            this.progressBar1.Maximum = 0xff; //needs to be odSub timeout/ timer period
            this.startDIThread("ResetCustLogWrapper", ResetCustLogWrapper);
	}


		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: resetCustBtn_Click
		///		 *  Description     : Event Handler for user request to reset the SEVCON copy of the operational log
		///								and, on completion, puts data into talbe and displays it to user
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		///			 </summary>
		private void resetSevconBtn_Click(object sender, System.EventArgs e)
		{
			hideUserControls();
            this.startDIThread("ResetSevLogWrapper", ResetSevLogWrapper);
			}


		#endregion

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

		#region minor methods



            /// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: hideUserControls
		///		 *  Description     : USed to hide all user controls (except exit controls) 
		///								whilst DriveWizard is processing data. This is to minimise scope for 
		///								data and/or thread conflict
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		///			 </summary>
		private void hideUserControls()
		{
			this.resetCustBtn.Visible = false;
			this.resetSevconBtn.Visible = false;
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: showUserControls
		///		 *  Description     : USed to hide all user controls (except exit controls) 
		///								whilst DriveWizard is processing data. This is to minimise scope for 
		///								data and/or thread conflict
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		///	 </summary>
		///
        private void showUserControls()
        {
            this.table.Clear();
            this.createLogTable();

            if (custOpLogSub == null)
            {
                this.label1.Text = "Customer log not available";
            }
            else
            {
                if (this.sysInfo.systemAccess > 1)
                {
                    if (this.resetCustLogSub != null)
                    {
                        this.resetCustBtn.Visible = true;
                        this.label1.Text = "View or reset customer log";
                    }
                    else
                    {
                        this.label1.Text = "View Customer log";
                    }
                }
            }
            if (this.sevOpLogSub == null)
            {
                this.label2.Text = "Sevcon log not available";
            }
            else
            {
                if (this.sysInfo.systemAccess >= 5)
                {
                    if (resetSevLogSub != null)
                    {
                        this.resetSevconBtn.Visible = true;
                        this.label2.Text = "View or reset Sevcon log";
                    }
                    else
                    {
                        this.label2.Text = "View Sevcon log";
                    }
                }
                else
                {
                    this.label2.Text = "View Sevcon log";
                }
            }
            if (SystemInfo.errorSB.Length == 0)
            {
                this.statusBar1.Text = "Displaying Operational Logs";
            }
            else
            {
                sysInfo.displayErrorFeedbackToUser("");
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="panelNo"></param>
		/// <param name="panelText"></param>

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
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
		private void OP_LOGS_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			statusBar1.Text = "Performing finalisation, please wait";
			#region disable all timers
				this.timer1.Enabled = false;
			#endregion disable all timers
			#region stop all threads
			if(DIThread != null)
			{
				if( ( DIThread.ThreadState & System.Threading.ThreadState.Stopped ) == 0 )
				{
					DIThread.Abort();

					if(DIThread.IsAlive == true)
					{
						#if DEBUG
						SystemInfo.errorSB.Append("\nFailed to close Thread: " + DIThread.Name + " on exit");
						#endif	
						DIThread = null;
					}
				}
			}
			#endregion stop all threads
			e.Cancel = false; //force this window to close
		}
		private void OP_LOGS_WINDOW_Closed(object sender, System.EventArgs e)
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
		#endregion
	}
	#endregion Operational Logs form Class

	#region DW Op Logs baseline DataTable 
	public class DWOpLogTable : DataTable
	{
		// AJK, 04/08/04 changed data styles to unsigned 16
		// AJK, 11/08/04 changed data styles to strings (scaling applied to values)
		public DWOpLogTable()
		{
			this.Columns.Add(OpLogCol.OpName.ToString(), typeof(System.String));
            this.Columns[OpLogCol.OpName.ToString()].DefaultValue = "";
			this.Columns.Add(OpLogCol.CustMin.ToString(),typeof(System.String));
            this.Columns[OpLogCol.CustMin.ToString()].DefaultValue = "";
			this.Columns.Add(OpLogCol.CustMax.ToString(),typeof(System.String));
            this.Columns[OpLogCol.CustMax.ToString()].DefaultValue = "";
			this.Columns.Add(OpLogCol.SCMin.ToString(),typeof(System.String));
            this.Columns[OpLogCol.SCMin.ToString()].DefaultValue = "";
			this.Columns.Add(OpLogCol.SCMax.ToString(),typeof(System.String));
            this.Columns[OpLogCol.SCMax.ToString()].DefaultValue = "";
		}
	}
	#endregion

	#region DW Operational Logs baseline TableStyle 
	public class OpLogTableStyle : SCbaseTableStyle
	{
		public OpLogTableStyle (int [] ColWidths)
		{
			//table style level parameters
			this.AllowSorting = false;

			SCbaseRODataGridTextBoxColumn OpNameCol = new SCbaseRODataGridTextBoxColumn((int) (OpLogCol.OpName));
			OpNameCol.MappingName = OpLogCol.OpName.ToString();
			OpNameCol.HeaderText = "Parameter Name";
			OpNameCol.Width = ColWidths[(int) (OpLogCol.OpName)];
			GridColumnStyles.Add(OpNameCol);

			SCbaseRODataGridTextBoxColumn CustMin = new SCbaseRODataGridTextBoxColumn((int) (OpLogCol.CustMin));
			CustMin.MappingName = OpLogCol.CustMin.ToString();
			CustMin.HeaderText = "Customer Log";
			CustMin.Width = ColWidths[(int) (OpLogCol.CustMin)];
			CustMin.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(CustMin);

			SCbaseRODataGridTextBoxColumn CustMax = new SCbaseRODataGridTextBoxColumn((int) (OpLogCol.CustMax));
			CustMax.MappingName = OpLogCol.CustMax.ToString();
			CustMax.HeaderText = "";
			CustMax.Width = ColWidths[(int) (OpLogCol.CustMax)];
			CustMax.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(CustMax);

			SCbaseRODataGridTextBoxColumn SCMin = new SCbaseRODataGridTextBoxColumn((int) (OpLogCol.SCMin));
			SCMin.MappingName = OpLogCol.SCMin.ToString();
			SCMin.HeaderText = "SEVCON Log"; 
			SCMin.Width = ColWidths[(int) (OpLogCol.SCMin)];
			SCMin.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(SCMin);

			SCbaseRODataGridTextBoxColumn SCMax = new SCbaseRODataGridTextBoxColumn((int) (OpLogCol.SCMax));
			SCMax.MappingName = OpLogCol.SCMax.ToString();
			SCMax.HeaderText = ""; 
			SCMax.Width = ColWidths[(int) (OpLogCol.SCMax)];
			SCMax.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(SCMax);
			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();

		}
	}
	#endregion
}
