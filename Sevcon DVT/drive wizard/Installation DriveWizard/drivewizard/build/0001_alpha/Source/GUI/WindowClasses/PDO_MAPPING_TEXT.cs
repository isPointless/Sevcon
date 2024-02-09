/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.105$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:13:22$
	$ModDate:05/12/2007 22:08:40$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION

REFERENCES    

MODIFICATION HISTORY
    $Log:  36791: PDO_MAPPING_TEXT.cs 

   Rev 1.105    05/12/2007 22:13:22  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Drawing;
using System.Text;

namespace DriveWizard
{
	#region enumerated table colum names
	enum COBIDcols { COBID, TxNodeID, Type, InhibitTime, EventTime,  RxNode1, RxNode2, RxNode3, RxNode4, RxNode5, RxNode6, RxNode7, OrigCOBID, OrigTXNode, Toggle,CAN_11_OR_29, ExtendedIDbits, COB_Valid};
	enum NodeIDCols { NodeName, NodeNum};
	enum COBIDListCols { COBIDName, COBIDNum};
	internal enum PDOMappingCols1 {TxNode, RxNode1, RxNode2, RxNode3, COBID, TxNodeID};
	internal enum PDOMappingCols2 {RxNode4, RxNode5, RxNode6, RxNode7, COBID, TxNodeID};
	internal enum AllPDOsCols {ParamNamesList, IDNum, dataLength};
	#endregion enumerated table colum names

	/// <summary>
	/// Summary description for CAN_BUS_CONFIGURATION.
	/// </summary>
	public class PDO_MAPPING_TEXT : System.Windows.Forms.Form
	{
		#region controls declarations
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.DataGrid dataGrid2;
		private System.Windows.Forms.DataGrid dataGrid3;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel headerPanel;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.Label label26;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Label label2;
		#endregion controls declarations

		#region my definitions
		private SystemInfo localSystemInfo;
		#region threads
		private Thread PDOdataRetrievalThread = null, PDOdataRefreshThread = null;
		#endregion threads
		#region DataSets, DataTables, tableStyles and DataViews
		private DataSet myDataSet = null;
		private NodeNumsListTable NodeNumsTable = null;
		public  COBIDsListTable COBIDsTable = null;
		public COBIDTable COBIDRoutingTable = null;
		private PDOMappingTable1 PDOtable1; 
		private PDOMappingTable2 PDOtable2; 
		public  AllPDOItemsTable [] PDOMasterTablesTx, PDOMasterTablesRx;
		private DataView COBRoutingView, PDOMap1View, PDOMap2View;
		private COBID_TS tablestyle = null;
		private PDO1_TS tablestyle2 = null;
		private PDO2_TS tablestyle3 = null;
		#endregion DataSets, DataTables, tableStyles and DataViews

		private bool viewOnly = true;
		uint [] SC_SingleNodeMapping;  ///used to store the total number of mapped bits in a PDO mapping to determine 
		ushort maxSEVCONPDOMappingsAllowed = 8;
		private string [] PDOMap1ColHeadings = {"Tx node (0x","1st Rx node (0x","2nd Rx node (0x", "3rd Rx node3 (0x"};
		private string [] PDOMap2ColHeadings = {"4th Rx node (0x","5th Rx node (0x","6th Rx node (0x", "7th Rx node (0x"};
		public static uint COBDeletionMarker =  9999;  //used to indicate a COB to be deleted
		#region tablestyle column width proportions
		float [] COBRoutingPercents = new float[0];
		float [] PDOMap1Percents = new float[0];
		float [] PDOMap2Percents= new float[0];
		#endregion tablestyle column width proportions
		ushort MaxSEVCONPDOMapSlots = 8;
		ushort MaxSEVCONPDOsAllowed = 9;
		ushort MaxThirdPartyPDOsAllowed = 64;
		ArrayList myBitsUsed;
		private DIFeedbackCode feedback;
		bool myNewRow = false;
		public static int numNodesPDO = 0;
		bool changeDatagrid1FocusRowFlag = false;
		bool AllNodesInPreOp;
		public static string PDOComboErrorString = "";
		public static bool DuplicateCOBFlag = false;
		private System.Windows.Forms.StatusBar statusBar1;
		private int PDOcolumnFromCobRouteTable = 0;
		int dataGrid1DefaultHeight = 0;
		private ArrayList PDOableNodes = new ArrayList();
		#endregion my definitions

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
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.submitBtn = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.dataGrid3 = new System.Windows.Forms.DataGrid();
			this.dataGrid2 = new System.Windows.Forms.DataGrid();
			this.dataGrid1 = new System.Windows.Forms.DataGrid();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.headerPanel = new System.Windows.Forms.Panel();
			this.label22 = new System.Windows.Forms.Label();
			this.label26 = new System.Windows.Forms.Label();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(664, 488);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(120, 25);
			this.button1.TabIndex = 0;
			this.button1.Text = "&Close window";
			this.button1.Click += new System.EventHandler(this.closeBtn_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(776, 30);
			this.label1.TabIndex = 1;
			// 
			// timer1
			// 
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(0, 520);
			this.progressBar1.Maximum = 0;
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(792, 23);
			this.progressBar1.TabIndex = 3;
			// 
			// submitBtn
			// 
			this.submitBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.submitBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.submitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.submitBtn.Location = new System.Drawing.Point(8, 488);
			this.submitBtn.Name = "submitBtn";
			this.submitBtn.Size = new System.Drawing.Size(120, 25);
			this.submitBtn.TabIndex = 4;
			this.submitBtn.Text = "&Submit";
			this.submitBtn.Visible = false;
			this.submitBtn.Click += new System.EventHandler(this.submitBtn_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.dataGrid3);
			this.panel1.Controls.Add(this.dataGrid2);
			this.panel1.Controls.Add(this.dataGrid1);
			this.panel1.Location = new System.Drawing.Point(16, 72);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(760, 406);
			this.panel1.TabIndex = 6;
			this.panel1.Resize += new System.EventHandler(this.panel1_Resize);
			// 
			// dataGrid3
			// 
			this.dataGrid3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGrid3.DataMember = "";
			this.dataGrid3.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid3.Location = new System.Drawing.Point(0, 294);
			this.dataGrid3.Name = "dataGrid3";
			this.dataGrid3.ReadOnly = true;
			this.dataGrid3.Size = new System.Drawing.Size(760, 112);
			this.dataGrid3.TabIndex = 5;
			this.dataGrid3.Visible = false;
			this.dataGrid3.Click += new System.EventHandler(this.PDOMapdataGrid_Click);
			// 
			// dataGrid2
			// 
			this.dataGrid2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGrid2.DataMember = "";
			this.dataGrid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid2.Location = new System.Drawing.Point(0, 110);
			this.dataGrid2.Name = "dataGrid2";
			this.dataGrid2.ReadOnly = true;
			this.dataGrid2.Size = new System.Drawing.Size(760, 152);
			this.dataGrid2.TabIndex = 4;
			this.dataGrid2.Visible = false;
			this.dataGrid2.Click += new System.EventHandler(this.PDOMapdataGrid_Click);
			this.dataGrid2.Enter += new System.EventHandler(this.dataGrid2_Enter);
			// 
			// dataGrid1
			// 
			this.dataGrid1.AllowSorting = false;
			this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGrid1.DataMember = "";
			this.dataGrid1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid1.Location = new System.Drawing.Point(8, -16);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.Size = new System.Drawing.Size(744, 312);
			this.dataGrid1.TabIndex = 3;
			this.dataGrid1.Visible = false;
			this.dataGrid1.Click += new System.EventHandler(this.dataGrid1_Click);
			this.dataGrid1.Paint += new System.Windows.Forms.PaintEventHandler(this.dataGrid1_Paint);
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.panel2.Controls.Add(this.panel3);
			this.panel2.Controls.Add(this.label2);
			this.panel2.Controls.Add(this.headerPanel);
			this.panel2.Controls.Add(this.label22);
			this.panel2.Controls.Add(this.label26);
			this.panel2.Location = new System.Drawing.Point(0, 520);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(560, 24);
			this.panel2.TabIndex = 7;
			this.panel2.Visible = false;
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.Color.Black;
			this.panel3.Location = new System.Drawing.Point(144, 4);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(16, 16);
			this.panel3.TabIndex = 69;
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Location = new System.Drawing.Point(168, 4);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 16);
			this.label2.TabIndex = 70;
			this.label2.Text = "Dynamic PDO";
			this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// headerPanel
			// 
			this.headerPanel.BackColor = System.Drawing.Color.Gainsboro;
			this.headerPanel.Location = new System.Drawing.Point(48, 4);
			this.headerPanel.Name = "headerPanel";
			this.headerPanel.Size = new System.Drawing.Size(16, 16);
			this.headerPanel.TabIndex = 67;
			// 
			// label22
			// 
			this.label22.BackColor = System.Drawing.Color.Transparent;
			this.label22.Location = new System.Drawing.Point(64, 4);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(80, 16);
			this.label22.TabIndex = 68;
			this.label22.Text = "Fixed PDO";
			this.label22.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label26
			// 
			this.label26.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label26.Location = new System.Drawing.Point(0, 4);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(40, 16);
			this.label26.TabIndex = 66;
			this.label26.Text = "Key:";
			this.label26.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 542);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(792, 23);
			this.statusBar1.TabIndex = 8;
			// 
			// PDO_MAPPING_TEXT
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.button1;
			this.ClientSize = new System.Drawing.Size(792, 565);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.submitBtn);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button1);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "PDO_MAPPING_TEXT";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.PDO_MAPPING_TEXT_Closing);
			this.Load += new System.EventHandler(this.PDO_MAPPING_TEXT_Load);
			this.Closed += new System.EventHandler(this.PDO_MAPPING_TEXT_Closed);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
		public PDO_MAPPING_TEXT(ref SystemInfo systemInfo, bool AllNodesInPreOpFlag)
		{
			InitializeComponent();
			localSystemInfo = systemInfo;
			PDOableNodes = new ArrayList();
			int nodesNotRequiringPreOP = 0;
			foreach(nodeInfo node in this.localSystemInfo.nodes)
			{
				if(node.manufacturer == Manufacturer.THIRD_PARTY)
				{
					PDOableNodes.Add(node.nodeID);
					node.itemBeingRead = 0;
					nodesNotRequiringPreOP++;
				}
				else if(node.manufacturer == Manufacturer.SEVCON)
				{
					if(node.isSevconApplication() == true) 
					{

						if(node.nodeState == NodeState.PreOperational)
						{
							PDOableNodes.Add(node.nodeID);
							node.itemBeingRead = 0;
						}
					}
					else
					{
						if((node.isSevconBootloader() == false) 
							&& (node.isSevconSelfChar() == false))
						{
							PDOableNodes.Add(node.nodeID);
							node.itemBeingRead = 0;
							nodesNotRequiringPreOP++;
						}
					}
				}
			}
			numNodesPDO = PDOableNodes.Count;//this.localSystemInfo.noOfNodesWithValidEDS - this.localSystemInfo.noOfSevconBootLoaderNodes;
			processNumNodes();
			AllNodesInPreOp = AllNodesInPreOpFlag;
			//if there is a Sevcon app - then we must be logged i

			if(nodesNotRequiringPreOP == PDOableNodes.Count) //do not check for access or pre-op
			{
				viewOnly = false;
			}
			else
			{
				//requirements for if a SEVOCN node is present
				for(int i = 0;i<this.localSystemInfo.nodes.Length;i++)
				{
					if ((localSystemInfo.nodes[i].isSevconApplication()==true)		
						&& (localSystemInfo.systemAccess>=SCCorpStyle.AccLevel_SevconPDOWrite) 
						&& (AllNodesInPreOpFlag == true))
					{
						viewOnly = false;
						break;
					}
				}
			}
			SC_SingleNodeMapping = new uint[MaxSEVCONPDOMapSlots]; //SEVCON devices limit mappings to maxumum of 8.
			myBitsUsed = new ArrayList();
		}

		private void processNumNodes()
		{
			switch (numNodesPDO)
			{
				case 3:
					#region 3 connected nodes
					COBRoutingPercents = new float[7];
					COBRoutingPercents[(int) COBIDcols.COBID] = 0.15F;
					COBRoutingPercents[(int) COBIDcols.TxNodeID] = 0.15F;
					COBRoutingPercents[(int) COBIDcols.Type] = 0.12F;
					COBRoutingPercents[(int) COBIDcols.InhibitTime] = 0.13F;
					COBRoutingPercents[(int) COBIDcols.EventTime] = 0.15F;
					COBRoutingPercents[(int) COBIDcols.RxNode1] = 0.15F;
					COBRoutingPercents[(int) COBIDcols.RxNode2] = 0.15F;

					PDOMap1Percents = new float[3];
					PDOMap1Percents[0] = 0.33F;
					PDOMap1Percents[1] = 0.33F;
					PDOMap1Percents[2] = 0.33F;
					#endregion 3 connected nodes
					break;
				case 4:
					#region 4 connected nodes
					COBRoutingPercents = new float[8];
					COBRoutingPercents[(int) COBIDcols.COBID] = 0.15F;
					COBRoutingPercents[(int) COBIDcols.TxNodeID] = 0.15F;
					COBRoutingPercents[(int) COBIDcols.Type] = 0.12F;
					COBRoutingPercents[(int) COBIDcols.InhibitTime] = 0.13F;
					COBRoutingPercents[(int) COBIDcols.EventTime] = 0.15F;
					COBRoutingPercents[(int) COBIDcols.RxNode1] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.RxNode2] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.RxNode3] = 0.1F;
					PDOMap1Percents = new float[4];
					for (int i =0 ;i<4;i++)
					{
						PDOMap1Percents[i] = 0.25F;
					}
					#endregion 4 connected nodes
					break;
				case 5:
					#region 5 connected nodes
					COBRoutingPercents = new float[9];
					COBRoutingPercents[(int) COBIDcols.COBID] = 0.15F;
					COBRoutingPercents[(int) COBIDcols.TxNodeID] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.Type] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.InhibitTime] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.EventTime] = 0.15F;
					COBRoutingPercents[(int) COBIDcols.RxNode1] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.RxNode2] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.RxNode3] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.RxNode4] = 0.1F;
					PDOMap1Percents = new float[4];
					for (int i =0 ;i<4;i++)
					{
						PDOMap1Percents[i] = 0.25F;
					}
					PDOMap2Percents = new float[1];
					PDOMap2Percents[0] = 0.5F;
					#endregion 5 connected nodes
					break;
				case 6:
					#region 6 connected nodes
					COBRoutingPercents = new float[10];
					COBRoutingPercents[(int) COBIDcols.COBID] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.TxNodeID] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.Type] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.InhibitTime] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.EventTime] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.RxNode1] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.RxNode2] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.RxNode3] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.RxNode4] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.RxNode5] = 0.1F;
					PDOMap1Percents = new float[4];
					for (int i =0 ;i<4;i++)
					{
						PDOMap1Percents[i] = 0.25F;
					}
					PDOMap2Percents = new float[2];
					PDOMap2Percents[0] = 0.5F;
					PDOMap2Percents[1] = 0.5F;
					#endregion 6 connected nodes
					break;
				case 7:
					#region 7 connected nodes
					COBRoutingPercents = new float[11];
					COBRoutingPercents[(int) COBIDcols.COBID] = 0.1F;
					COBRoutingPercents[(int) COBIDcols.TxNodeID] = 0.09F;
					COBRoutingPercents[(int) COBIDcols.Type] = 0.09F;
					COBRoutingPercents[(int) COBIDcols.InhibitTime] = 0.09F;
					COBRoutingPercents[(int) COBIDcols.EventTime] = 0.09F;
					COBRoutingPercents[(int) COBIDcols.RxNode1] = 0.09F;
					COBRoutingPercents[(int) COBIDcols.RxNode2] = 0.091F;
					COBRoutingPercents[(int) COBIDcols.RxNode3] = 0.09F;
					COBRoutingPercents[(int) COBIDcols.RxNode4] = 0.09F;
					COBRoutingPercents[(int) COBIDcols.RxNode5] = 0.09F;
					COBRoutingPercents[(int) COBIDcols.RxNode6] = 0.09F;
					PDOMap1Percents = new float[4];
					for (int i =0 ;i<4;i++)
					{
						PDOMap1Percents[i] = 0.25F;
					}
					PDOMap2Percents = new float[3];
					PDOMap2Percents[0] = 0.33F;
					PDOMap2Percents[1] = 0.33F;
					PDOMap2Percents[2] = 0.33F;
					#endregion 7 connected nodes
					break;
				case 8:
					#region 8 connected nodes
					COBRoutingPercents = new float[12];
					COBRoutingPercents[(int) COBIDcols.COBID] = 0.09F;
					COBRoutingPercents[(int) COBIDcols.TxNodeID] = 0.09F;
					COBRoutingPercents[(int) COBIDcols.Type] = 0.08F;
					COBRoutingPercents[(int) COBIDcols.InhibitTime] = 0.08F;
					COBRoutingPercents[(int) COBIDcols.EventTime] = 0.08F;
					COBRoutingPercents[(int) COBIDcols.RxNode1] = 0.08F;
					COBRoutingPercents[(int) COBIDcols.RxNode2] = 0.08F;
					COBRoutingPercents[(int) COBIDcols.RxNode3] = 0.08F;
					COBRoutingPercents[(int) COBIDcols.RxNode4] = 0.08F;
					COBRoutingPercents[(int) COBIDcols.RxNode5] = 0.08F;
					COBRoutingPercents[(int) COBIDcols.RxNode6] = 0.08F;
					COBRoutingPercents[(int) COBIDcols.RxNode7] = 0.084F;
					PDOMap1Percents = new float[4];
					PDOMap2Percents = new float[4];
					for (int i =0 ;i<4;i++)
					{
						PDOMap1Percents[i] = 0.25F;
						PDOMap2Percents[i] = 0.25F;
					}
					#endregion 8 connected nodes
					break;
				default:  //includes 1, 2 nodes
					#region 1, 2 or default connected nodes
					COBRoutingPercents = new float[6];
					COBRoutingPercents[(int) COBIDcols.COBID] = 0.17F;
					COBRoutingPercents[(int) COBIDcols.TxNodeID] = 0.17F;
					COBRoutingPercents[(int) COBIDcols.Type] = 0.16F;
					COBRoutingPercents[(int) COBIDcols.InhibitTime] = 0.16F;
					COBRoutingPercents[(int) COBIDcols.EventTime] = 0.17F;
					COBRoutingPercents[(int) COBIDcols.RxNode1] = 0.17F;
					PDOMap1Percents = new float[2];
					PDOMap1Percents[0] = 0.5F;
					PDOMap1Percents[1] = 0.5F;
					#endregion 1, 2 or default connected nodes
					break;
			}
		}
		private void linkMappingLists(DataRow activeCOBIDRow)
		{
			PDOMapComboColumn thisCol = null;
			int nodeID = 0;
			if(numNodesPDO>=1)  //action is same for one or two nodes
			{
				#region link Tx node to correct drop down list
				nodeID = System.Convert.ToInt32(activeCOBIDRow[(int) COBIDcols.TxNodeID]);
				thisCol = (PDOMapComboColumn)this.dataGrid2.TableStyles[0].GridColumnStyles[(int) PDOMappingCols1.TxNode];
				if(this.PDOableNodes.Contains(nodeID))
				{
					thisCol.objSource = this.PDOMasterTablesTx[PDOableNodes.IndexOf(nodeID)];
				}
				else
				{
					thisCol.objSource = null;  //there should be no list
				}
				#endregion link Tx node to correct drop down list
				#region Rx node 1
				nodeID = System.Convert.ToInt32(activeCOBIDRow[(int) COBIDcols.RxNode1]);
				thisCol = (PDOMapComboColumn)this.dataGrid2.TableStyles[0].GridColumnStyles[(int) PDOMappingCols1.RxNode1];
				if(this.PDOableNodes.Contains(nodeID))
				{
					thisCol.objSource = this.PDOMasterTablesRx[PDOableNodes.IndexOf(nodeID)];
				}
				else
				{
					thisCol.objSource = null;
				}
				#endregion Rx node 1
			}
			if(numNodesPDO>=3)
			{
				#region Rx node 2
				nodeID = System.Convert.ToInt32(activeCOBIDRow[(int) COBIDcols.RxNode2]);
				thisCol = (PDOMapComboColumn)this.dataGrid2.TableStyles[0].GridColumnStyles[(int) PDOMappingCols1.RxNode2];
				if(this.PDOableNodes.Contains(nodeID))
				{
					thisCol.objSource = this.PDOMasterTablesRx[PDOableNodes.IndexOf(nodeID)];
				}
				else
				{
					thisCol.objSource = null;
				}
				#endregion Rx node 2
			}
			if(numNodesPDO>=4)
			{
				#region Rx node 3
				nodeID = System.Convert.ToInt32(activeCOBIDRow[(int) COBIDcols.RxNode3]);
				thisCol = (PDOMapComboColumn)this.dataGrid2.TableStyles[0].GridColumnStyles[(int) PDOMappingCols1.RxNode3];
				if(this.PDOableNodes.Contains(nodeID))
				{
					thisCol.objSource = this.PDOMasterTablesRx[PDOableNodes.IndexOf(nodeID)];
				}
				else
				{
					thisCol.objSource = null;
				}
				#endregion Rx node 3
			}
			if(numNodesPDO>=5)
			{
				#region Rx node 4
				nodeID = System.Convert.ToInt32(activeCOBIDRow[(int) COBIDcols.RxNode4]);
				thisCol = (PDOMapComboColumn)this.dataGrid3.TableStyles[0].GridColumnStyles[(int) PDOMappingCols2.RxNode4];
				if(this.PDOableNodes.Contains(nodeID))
				{
					thisCol.objSource = this.PDOMasterTablesRx[PDOableNodes.IndexOf(nodeID)];
				}
				else
				{
					thisCol.objSource = null;
				}
				#endregion Rx node 4
			}
			if(numNodesPDO>=6)
			{
				#region Rx node 5
				nodeID = System.Convert.ToInt32(activeCOBIDRow[(int) COBIDcols.RxNode5]);
					thisCol = (PDOMapComboColumn)this.dataGrid3.TableStyles[0].GridColumnStyles[(int) PDOMappingCols2.RxNode5];
				if(this.PDOableNodes.Contains(nodeID))
				{
					thisCol.objSource = this.PDOMasterTablesRx[PDOableNodes.IndexOf(nodeID)];
				}
				else
				{
					thisCol.objSource = null;
				}
				#endregion Rx node 5
			}
			if(numNodesPDO>=7)
			{
				#region Rx node 6
				nodeID = System.Convert.ToInt32(activeCOBIDRow[(int) COBIDcols.RxNode6]);
					thisCol = (PDOMapComboColumn)this.dataGrid3.TableStyles[0].GridColumnStyles[(int) PDOMappingCols2.RxNode6];
				if(this.PDOableNodes.Contains(nodeID))
				{
					thisCol.objSource = this.PDOMasterTablesRx[PDOableNodes.IndexOf(nodeID)];
				}
				else
				{
					thisCol.objSource = null;
				}
				#endregion Rx node 6
			}
			if(numNodesPDO==8)  //cannot be greater than
			{
				#region Rx node 7
				nodeID = System.Convert.ToInt32(activeCOBIDRow[(int) COBIDcols.RxNode7]);
				thisCol = (PDOMapComboColumn)this.dataGrid3.TableStyles[0].GridColumnStyles[(int) PDOMappingCols2.RxNode7];
				if(this.PDOableNodes.Contains(nodeID))
				{
					thisCol.objSource = this.PDOMasterTablesRx[PDOableNodes.IndexOf(nodeID)];
				}
				else
				{
					thisCol.objSource = null;
				}
				#endregion Rx node 7
			}
			this.dataGrid2.Invalidate();
			if(this.dataGrid3.Visible == true)
			{
				this.dataGrid3.Invalidate();
			}
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: PDO_MAPPING_TEXT_Load
		///		 *  Description     : Event HAndler for form load event
		///		 *  Parameters      : System event arguments 
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		
		private void PDO_MAPPING_TEXT_Load(object sender, System.EventArgs e)
		{
			#region initial, fixed  parameters for dataGrids
			foreach(Control c in this.Controls)
			{
				formatControls(c);
			}
			dataGrid1.Width = this.panel1.Width;
			dataGrid2.Width = dataGrid1.Width;
			dataGrid3.Width = dataGrid1.Width;
			dataGrid1.Top = 0;
			this.dataGrid1.Left = 0;
			this.dataGrid2.Left = 0;
			this.dataGrid3.Left = 0;
			#endregion initial, fixed  parameters for dataGrids
		
			dataGrid1DefaultHeight = this.dataGrid1.Height;

			NodeNumsTable = new NodeNumsListTable();
			COBIDsTable = new COBIDsListTable();
			this.dataGrid1.CaptionText = "Communication Objects (COBs) for connected CANopen system";
			this.label1.Text = "Please wait";

			statusBar1.Text = "Retrieving data from connected nodes";
			if(this.viewOnly == false)
			{
				this.Text = "DriveWizard: PDO Mapping configuration";
			}
			else
			{
				this.Text = "DriveWizard: PDO Mapping (read only)";
			}
			#region data retrieval thread
			PDOdataRetrievalThread = new Thread(new ThreadStart( getData )); 
			PDOdataRetrievalThread.Name = "COBIDDataRetrieval";
			PDOdataRetrievalThread.IsBackground = true;
			PDOdataRetrievalThread.Priority = ThreadPriority.BelowNormal;
#if DEBUG
			System.Console.Out.WriteLine("Thread: " + PDOdataRetrievalThread.Name + " started");
#endif

			PDOdataRetrievalThread.Start(); 
			this.timer1.Enabled = true;
			#endregion data retrieval thread
		}
		public void formatControls(System.Windows.Forms.Control topControl )
		{
			#region format individual controls
			topControl.ForeColor = SCCorpStyle.SCForeColour;
			topControl.Font = new System.Drawing.Font("Arial",8F);
//			topControl.BackColor = SCCorpStyle.SCBackColour; 
			if ( topControl.GetType().Equals( typeof( Button ) ) ) 
			{
				topControl.BackColor = SCCorpStyle.buttonBackGround;
				Button btn = (Button) topControl;
				btn.FlatStyle = FlatStyle.Flat;
			}
			else if ( topControl.GetType().Equals( typeof( DataGrid ) ) ) 
			{
				DataGrid myDG = (DataGrid) topControl;
				SCCorpStyle.formatDataGrid(ref myDG);
//				myDG.Resize +=new EventHandler(myDG_Resize);
			}
			else if ( topControl.GetType().Equals( typeof( VScrollBar ) ) ) 
			{
				topControl.VisibleChanged +=new EventHandler(myControl_VisibleChanged);
			}
			else if ( topControl.GetType().Equals( typeof( HScrollBar ) ) ) 
			{
				topControl.Height = 0;
			}
			else if ( topControl.GetType().Equals( typeof( GroupBox ) ) ) 
			{
				topControl.Font = new System.Drawing.Font("Arial", 8F, FontStyle.Bold);
			}
			else if ( topControl.GetType().Equals( typeof( Cursor ) ) ) 
			{
#if DEMO_MODE
				this.Cursor = new Cursor(GetType(), MAIN_WINDOW.ApplicationDirectoryPath + @"\Resources\cursors\pyramid.cur");
#endif
			}
			#endregion format individual controls
			foreach(Control control in topControl.Controls) 
			{
				formatControls(control);
			}
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (PDOdataRetrievalThread != null)
			{
				if((PDOdataRetrievalThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
					#region displayretrieved data
					PDOdataRetrievalThread = null;  //prevent re-entry
					timer1.Enabled = false; //kill timer
					createCOBRoutAndPDOTables();
					addDataRelations();
					fillCOBAndPDOTables();
					clearAndSetdataGridBindings();
					#region apply PDO Map Dataview Filtering for current COBID (dataGrid row 0)
					if(COBIDRoutingTable.Rows.Count>0)
					{
						this.PDOMap1View.RowFilter =  PDOMappingCols1.COBID.ToString() + " = '" + COBIDRoutingTable.Rows[0][(int) COBIDcols.COBID].ToString() + "'";
						this.PDOMap2View.RowFilter =  PDOMappingCols2.COBID.ToString() + " = '" + COBIDRoutingTable.Rows[0][(int) COBIDcols.COBID].ToString() + "'";
					}
					#endregion apply PDO Map Dataview Filtering for current COBID (dataGrid row 0)

					#region make dataGrids Visible
					this.dataGrid1.Visible = true;
					this.dataGrid2.Visible = true;
					if(numNodesPDO >4)
					{
						this.dataGrid3.Visible = true;
					}
					#endregion make dataGrids Visible
					createTableStyles();
					//link PDO columns with correct drop down list.
					//Step through each column in the uppr PDO mapping table
					if(this.COBIDRoutingTable.Rows.Count>0)
					{
						linkMappingLists(this.COBIDRoutingTable.Rows[0]);
					}
					addTableEventHanlders();
					statusBar1.Text = "Displaying COB routing and assoc. PDO Mapping";
					applyTableStyles();
					showUserControls();
					#endregion displayretrieved data
				}
				else
				{
					#region increment the progress bar
					this.progressBar1.Value = this.localSystemInfo.itemCounter;
					#endregion increment the progress bar
				}
			}
			else if (this.PDOdataRefreshThread != null)
			{
				if((PDOdataRefreshThread.ThreadState & System.Threading.ThreadState.Stopped) > 0 )
				{
					#region PDO refresh thread
					PDOdataRefreshThread = null;
					//need to recreate table otheriwes get exception due to data relationship
					createCOBRoutAndPDOTables();
					addDataRelations();
					fillCOBAndPDOTables();
					clearAndSetdataGridBindings();
					#region apply PDO Map Dataview Filtering for current COBID (dataGrid row 0)
					if(COBIDRoutingTable.Rows.Count>0)
					{
						this.PDOMap1View.RowFilter =  PDOMappingCols1.COBID.ToString() + " = '" + COBIDRoutingTable.Rows[0][(int) COBIDcols.COBID].ToString() + "'";
						this.PDOMap2View.RowFilter =  PDOMappingCols2.COBID.ToString() + " = '" + COBIDRoutingTable.Rows[0][(int) COBIDcols.COBID].ToString() + "'";
					}
					#endregion apply PDO Map Dataview Filtering for current COBID (dataGrid row 0)
					#region make datagirds visible
					this.dataGrid1.Visible = true;
					this.dataGrid2.Visible = true;
					if(numNodesPDO >4)
					{
						this.dataGrid3.Visible = true;
					}
					#endregion make datagirds visible
					createTableStyles();
					addTableEventHanlders();
					statusBar1.Text = "Displaying COB routing and assoc. PDO Mapping";
					applyTableStyles();
					statusBar1.Text = "done";
					this.label1.Text = "";
					showUserControls();
					#endregion PDO refresh thread
				}
				else
				{
					#region increment the progress bar
					this.progressBar1.Value = this.localSystemInfo.itemCounter;
					#endregion increment the progress bar
				}
			}
		}
		private void clearAndSetdataGridBindings()
		{
			dataGrid1.DataBindings.Clear();
			dataGrid1.SetDataBinding(myDataSet,"parentTable");
			dataGrid2.DataBindings.Clear();
			dataGrid2.SetDataBinding(myDataSet,"parentTable.parent2Child1");
			if(numNodesPDO >4) //this line needed to prevent Datagri3 being forced to VIsible
			{
				dataGrid3.DataBindings.Clear();
				dataGrid3.SetDataBinding(myDataSet,"parentTable.parent2Child2");
			}
		}
		private void addTableEventHanlders()
		{
			this.PDOtable1.ColumnChanged += new DataColumnChangeEventHandler(PDOtable1_ChangedHandler);
			this.PDOtable1.ColumnChanging +=new DataColumnChangeEventHandler(PDOtable1_ColumnChanging);
			if(numNodesPDO >4)
			{
				this.PDOtable2.ColumnChanged += new DataColumnChangeEventHandler(PDOtable2_ChangedHandler);
				this.PDOtable2.ColumnChanging +=new DataColumnChangeEventHandler(PDOtable2_ColumnChanging);
			}
			this.COBIDRoutingTable.ColumnChanged += new DataColumnChangeEventHandler(COBIDRoutingTable_ChangedHandler);
			this.COBIDRoutingTable.ColumnChanging +=new DataColumnChangeEventHandler(COBIDRoutingTable_ColumnChanging);
			this.dataGrid1.CurrentCellChanged += new System.EventHandler(this.dataGrid1_CurrentCellChanged);
		}

		private void applyTableStyles()
		{
			this.applyTableStyleCOBRouting();
			this.applyTableStylePDO1();
			if(numNodesPDO>4)
			{
				this.applyTableStylePDO2();
			}
		}
		private void getData()
		{
			//check the current system
			readCOBsInSystem();
			//create the master list of permitted COBIDs
			createCOBIDMasterList();
			//create a master list of node numbers
			createNodeNumsMasterTable();
			createMasterParameterLists();
		}

		private void readCOBsInSystem()
		{
			this.progressBar1.Visible = true;
			this.progressBar1.Value = this.progressBar1.Minimum;
			this.progressBar1.Maximum = this.localSystemInfo.totalItemsInAllODs;  //we are going to do the whole lot here
			this.localSystemInfo.readAllCOBItemsAndCreateCOBsInSystem();
		}

		#region create tables to fill Combo Lists
		private void createMasterParameterLists()
		{
			DataRow row;
			StringBuilder mappingStr = new StringBuilder();
			//Each connected node requires its own list since each node may have a different OD. 
			//Forced dat aused for now
			//the list od PDO mappables will be differnet depending on whether th enode is Txing or Rxing
			PDOMasterTablesTx = new AllPDOItemsTable[numNodesPDO];
			PDOMasterTablesRx = new AllPDOItemsTable[numNodesPDO];

			int tblCtr = 0;
			foreach(nodeInfo node in this.localSystemInfo.nodes)
			{
				if(this.PDOableNodes.Contains(node.nodeID) == true)
				{ //only interested in nodes we have ascertianed can be System PDOed
					PDOMasterTablesTx[tblCtr] = new  AllPDOItemsTable();
					#region add not used row to Tx and Rx varinats for this CANnode
					row = this.PDOMasterTablesTx[tblCtr].NewRow();
					row[(int) AllPDOsCols.ParamNamesList] = "not used";
					row[(int) AllPDOsCols.IDNum] = 0x0;
					PDOMasterTablesTx[tblCtr].Rows.Add(row);

					PDOMasterTablesRx[tblCtr] = new  AllPDOItemsTable();
					row = this.PDOMasterTablesRx[tblCtr].NewRow();
					row[(int) AllPDOsCols.ParamNamesList] = "not used";
					row[(int) AllPDOsCols.IDNum] = 0x0;
					PDOMasterTablesRx[tblCtr].Rows.Add(row);

					#endregion add not used row
					foreach(ODItemData[] odItem in node.dictionary.data)
					{
						foreach(ODItemData sub in odItem)
						{
							#region if this item is PDO mappable
							if(sub.PDOmappable == true)  //this item needs to be added to the masterlist
							{
								#region tx Parameter list
								if( (sub.accessType == ObjectAccessType.ReadReadWrite)
									|| (sub.accessType == ObjectAccessType.ReadReadWriteInPreOp)
									|| (sub.accessType == ObjectAccessType.ReadWrite)
									|| (sub.accessType == ObjectAccessType.ReadWriteInPreOp)
									|| (sub.accessType == ObjectAccessType.Constant)
									|| (sub.accessType == ObjectAccessType.ReadOnly) )
								{
									mappingStr = new StringBuilder();
									row = this.PDOMasterTablesTx[tblCtr].NewRow();
									mappingStr.Append(sub.indexNumber.ToString("X").PadLeft(4,'0'));
									mappingStr.Append(sub.subNumber.ToString("X").PadLeft(2, '0'));
									#region switch datatype
									switch(sub.dataType)
									{
										case 0x01: 
											#region boolean
											mappingStr.Append("01");
											row[(int) AllPDOsCols.dataLength] = 0x01;
											#endregion Boolean
											break;

										case 0x02:
										case 0x05:
											#region Int 8, UInt 8
											mappingStr.Append("08");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x08;
											#endregion Int 8, UInt 8
											break;

										case 0x03:  
										case 0x06:
											#region Int16, UInt16
											mappingStr.Append("10");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x10;
											#endregion Int16, UInt16
											break;

										case 0x10:
										case 0x16:
											#region Int 24, UInt 24
											mappingStr.Append("18");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x18;
											#endregion Int 24, UInt 24
											break;

										case 0x04:
										case 0x07:
											#region Int 32, Uint 32
											mappingStr.Append("20");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x20;
											#endregion Int 32, Uint 32
											break;

										case 0x12:
										case 0x18:
											#region Int40, UInt 40 
											mappingStr.Append("28");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x28;
											#endregion Int40, UInt 40 
											break;

										case 0x13:
										case 0x19:
											#region Int48 UInt 48
											mappingStr.Append("30");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x30;
											#endregion Int48 UInt 48
											break;

										case 0x14:
										case 0x1A:
											#region Int56 , UInt 56
											mappingStr.Append("38");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x38;
											#endregion Int56 , UInt 56
											break;
								
										case 0x15:
										case 0x1B:
											#region Int64, UInt 64
											mappingStr.Append("40");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x40;
											#endregion Int64, UInt 64
											break;

										default:
											mappingStr = new StringBuilder(); //so we know not to add row
											break;
									}
									#endregion switch datatype
									if(mappingStr.Length>0)
									{
										//do not append the param name - other wise converiosn will fail
										row[(int) AllPDOsCols.ParamNamesList] = sub.parameterName + " 0x" + mappingStr.ToString();
										try
										{
											row[(int) AllPDOsCols.IDNum] = System.Convert.ToUInt32(mappingStr.ToString(), 16);
											PDOMasterTablesTx[tblCtr].Rows.Add(row);
										}
										catch (Exception ex2)
										{
											Message.Show("An error has occurred when adding rows to PDO list. Error Code: " + ex2.ToString()
												+ "\n this window will close");
											this.Close();
										}
									}
								}
								#endregion  tx Parameter list

								#region rx Parameter list
								if( (sub.accessType == ObjectAccessType.ReadWriteWrite)
									|| (sub.accessType == ObjectAccessType.ReadWriteWriteInPreOp)
									|| (sub.accessType == ObjectAccessType.ReadWrite)
									|| (sub.accessType == ObjectAccessType.ReadWriteInPreOp)
									|| (sub.accessType == ObjectAccessType.WriteOnly)
									|| (sub.accessType == ObjectAccessType.WriteOnlyInPreOp) )
								{
									mappingStr = new StringBuilder();
									row = this.PDOMasterTablesRx[tblCtr].NewRow();
									mappingStr.Append(sub.indexNumber.ToString("X").PadLeft(4,'0'));
									mappingStr.Append(sub.subNumber.ToString("X").PadLeft(2,'0'));
									#region switch datatype
									switch(sub.dataType)
									{
										case 0x01: 
											#region boolean
											mappingStr.Append("01");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x01;
											#endregion Boolean
											break;

										case 0x02:
										case 0x05:
											#region Int 8, UInt 8
											mappingStr.Append("08");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x08;
											#endregion Int 8, UInt 8
											break;

										case 0x03:  
										case 0x06:
											#region Int16, UInt16
											mappingStr.Append("10");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x10;
											#endregion Int16, UInt16
											break;

										case 0x10:
										case 0x16:
											#region Int 24, UInt 24
											mappingStr.Append("18");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x18;
											#endregion Int 24, UInt 24
											break;

										case 0x04:
										case 0x07:
											#region Int 32, Uint 32
											mappingStr.Append("20");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x20;
											#endregion Int 32, Uint 32
											break;

										case 0x12:
										case 0x18:
											#region Int40, UInt 40 
											mappingStr.Append("28");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x28;
											#endregion Int40, UInt 40 
											break;

										case 0x13:
										case 0x19:
											#region Int48 UInt 48
											mappingStr.Append("30");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x30;
											#endregion Int48 UInt 48
											break;

										case 0x14:
										case 0x1A:
											#region Int56 , UInt 56
											mappingStr.Append("38");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x38;
											#endregion Int56 , UInt 56
											break;
								
										case 0x15:
										case 0x1B:
											#region Int64, UInt 64
											mappingStr.Append("40");  //append dataLength
											row[(int) AllPDOsCols.dataLength] = 0x40;
											#endregion Int64, UInt 64
											break;

										default:
											mappingStr = new StringBuilder();//so we know not to add row
											break;
									}
									#endregion switch datatype
									if(mappingStr.Length>0)
									{
										//do not append param name - other wise the conversion will fail
										row[(int) AllPDOsCols.ParamNamesList] = sub.parameterName + " 0x" + mappingStr.ToString();
										try
										{
											row[(int) AllPDOsCols.IDNum] = System.Convert.ToUInt32(mappingStr.ToString(), 16);
											PDOMasterTablesRx[tblCtr].Rows.Add(row);
										}
										catch (Exception ex3)
										{
											Message.Show("An error has occurred when adding rows to PDO list. Error Code: " + ex3.ToString()
												+ "\n this window will close");
											this.Close();
										}
									}
								}
								#endregion rxNode
							}
							#endregion if this item is PDO mappable
						}
					}
					#region comments
					//note CiA spec limts the spcares we can use to indexes in range 0001h - 0007h
					//these are
					//0004 DEFTYPE INTEGER32 = no point in using this one - we alearedy have Unsigned 32
					//0002 DEFTYPE INTEGER8					
					//0003 DEFTYPE INTEGER16
					#endregion comments
					
					#region add 'spacer' items to Rx table
					#region 1 bit spacer
					//0001 DEFTYPE BOOLEAN
					row = this.PDOMasterTablesRx[tblCtr].NewRow();
					row[(int) AllPDOsCols.ParamNamesList] = "1 bit spacer 0x00010001";
					row[(int) AllPDOsCols.IDNum] = 0x00010001;
					row[(int) AllPDOsCols.dataLength] = 0x01;  //zero the data length - for later calculations
					PDOMasterTablesRx[tblCtr].Rows.Add(row);
					#endregion 1 bit spacer
					#region 1 byte spacer
					//0005 DEFTYPE UNSIGNED8
					row = this.PDOMasterTablesRx[tblCtr].NewRow();
					row[(int) AllPDOsCols.ParamNamesList] = "1 byte spacer 0x00020008";
					row[(int) AllPDOsCols.IDNum] = 0x00050008;
					row[(int) AllPDOsCols.dataLength] = 0x08;  //zero the data length - for later calculations
					PDOMasterTablesRx[tblCtr].Rows.Add(row);
					#endregion 1 byte spacer
					#region 2 byte spacer
					//0006 DEFTYPE UNSIGNED16
					row = this.PDOMasterTablesRx[tblCtr].NewRow();
					row[(int) AllPDOsCols.ParamNamesList] = "2 byte spacer 0x00030010";
					row[(int) AllPDOsCols.IDNum] = 0x00060010;
					row[(int) AllPDOsCols.dataLength] = 0x10;  //zero the data length - for later calculations
					PDOMasterTablesRx[tblCtr].Rows.Add(row);
					#endregion 2 byte spacer
					#region 4 byte spacer
					//0007 DEFTYPE UNSIGNED32
					row = this.PDOMasterTablesRx[tblCtr].NewRow();
					row[(int) AllPDOsCols.ParamNamesList] = "4 byte spacer 0x00040020";
					row[(int) AllPDOsCols.IDNum] = 0x00070020;
					row[(int) AllPDOsCols.dataLength] = 0x20;  //zero the data length - for later calculations
					PDOMasterTablesRx[tblCtr].Rows.Add(row);
					#endregion 4 byte spacer
					#endregion add 'spacer' items
					PDOMasterTablesTx[tblCtr].AcceptChanges();  
					PDOMasterTablesRx[tblCtr].AcceptChanges(); 
					tblCtr++; //increment the table counter here
				}
			}
		}
		private void createNodeNumsMasterTable()
		{
			DataRow row;

			#region add default of zero to indicate that this node is not used
			row = NodeNumsTable.NewRow();
			row[(int) NodeIDCols.NodeName] = " not used";
			row[(int) NodeIDCols.NodeNum] = 0;
			NodeNumsTable.Rows.Add(row);
			#endregion add default of zero to indicate that this node is not used
			#region determine which nodes are connected
			foreach(int nodeID in PDOableNodes)
			{
				int nodeInd;
				this.localSystemInfo.getNodeNumber(nodeID, out nodeInd);
				string devType = this.localSystemInfo.nodes[nodeInd].deviceType;
				string nodeName = "Node " + nodeID.ToString() + " " + devType;
				row = NodeNumsTable.NewRow();
				row[(int) NodeIDCols.NodeName] = nodeName;
				row[(int) NodeIDCols.NodeNum] = nodeID;
				NodeNumsTable.Rows.Add(row);
			}
			#endregion determine which nodes are connected
		}

		private void createCOBIDMasterList()
		{
			DataRow row;
			#region add 5 suggested low priority COBIDs
			//try requesting 5 low prioirty cob IDs
			int [] lowPriorityCOBIDs = new int[5];
			this.localSystemInfo.selectPDOCOBIDs( COBIDPriority.low, lowPriorityCOBIDs);
			for(int i = 0;i<5;i++)
			{
				row = COBIDsTable.NewRow();
				string COBIDStr = lowPriorityCOBIDs[i].ToString("X").ToUpper();
				while (COBIDStr.Length<4)
				{
					COBIDStr = "0" + COBIDStr;
				}
				row[(int) COBIDListCols.COBIDName] = "0x" + COBIDStr + " (low)";
				row[(int) COBIDListCols.COBIDNum] = lowPriorityCOBIDs[i];
				COBIDsTable.Rows.Add(row);
			}
			#endregion add 5 suggested low priority COBIDs
			#region add 5 suggested medium low priority COBIDs
			//try requesting 5 low prioirty cob IDs
			int [] medlowPriorityCOBIDs = new int[5];
			this.localSystemInfo.selectPDOCOBIDs(COBIDPriority.mediumLow, medlowPriorityCOBIDs);
			for(int i = 0;i<5;i++)
			{
				row = COBIDsTable.NewRow();
				string COBIDStr = medlowPriorityCOBIDs[i].ToString("X").ToUpper();
				while (COBIDStr.Length<4)
				{
					COBIDStr = "0" + COBIDStr;
				}
				row[(int) COBIDListCols.COBIDName] = "0x" + COBIDStr + " (med low)";
				row[(int) COBIDListCols.COBIDNum] = medlowPriorityCOBIDs[i];
				COBIDsTable.Rows.Add(row);
			}
			#endregion add 5 suggested medium low priority COBIDs
			#region add 5 suggested medium low priority COBIDs
			//try requesting 5 low prioirty cob IDs
			int [] medhighPriorityCOBIDs = new int[5];
			this.localSystemInfo.selectPDOCOBIDs(COBIDPriority.mediumHigh, medhighPriorityCOBIDs);
			for(int i = 0;i<5;i++)
			{
				row = COBIDsTable.NewRow();
				string COBIDStr = medhighPriorityCOBIDs[i].ToString("X").ToUpper();
				while (COBIDStr.Length<4)
				{
					COBIDStr = "0" + COBIDStr;
				}
				row[(int) COBIDListCols.COBIDName] = "0x" + COBIDStr + " (med high)";
				row[(int) COBIDListCols.COBIDNum] = medhighPriorityCOBIDs[i];
				COBIDsTable.Rows.Add(row);
			}
			#endregion add 5 suggested medium low priority COBIDs
			#region add 5 suggested high priority COBIDs
			//try requesting 5 low prioirty cob IDs
			int [] highPriorityCOBIDs = new int[5];
			this.localSystemInfo.selectPDOCOBIDs(COBIDPriority.high, highPriorityCOBIDs);
			for(int i = 0;i<5;i++)
			{
				row = COBIDsTable.NewRow();
				string COBIDStr = highPriorityCOBIDs[i].ToString("X").ToUpper();
				while (COBIDStr.Length<4)
				{
					COBIDStr = "0" + COBIDStr;
				}
				row[(int) COBIDListCols.COBIDName] = "0x" + COBIDStr + " (high)";
				row[(int) COBIDListCols.COBIDNum] = highPriorityCOBIDs[i];
				COBIDsTable.Rows.Add(row);
			}
			#endregion add 5 suggested high priority COBIDs
			#region delete COB row
			row = COBIDsTable.NewRow();
			row[(int) COBIDListCols.COBIDName] = "Delete COB";
			row[(int) COBIDListCols.COBIDNum] = COBDeletionMarker;
			COBIDsTable.Rows.Add(row);
			#endregion delete COB row
		}
		#endregion create tables to fill Combo Lists
		private void createCOBRoutAndPDOTables()
		{
			#region table and DataView initalisation
			myDataSet = new DataSet();
			this.myDataSet.EnforceConstraints = false;  //just switcxh them off for ease of coding - may swithc them back on later
			COBIDRoutingTable = new COBIDTable();
			COBIDRoutingTable.TableName = "parentTable";
			COBRoutingView = new DataView(COBIDRoutingTable);
			if(this.viewOnly == false)
			{
				COBRoutingView.AllowNew = true;  //allow new COB IDs
				COBRoutingView.AllowDelete = true; 
			}
			else
			{
				COBRoutingView.AllowNew = false;  //allow new COB IDs
				COBRoutingView.AllowDelete = false; 
			}
			PDOtable1 = new PDOMappingTable1();
			PDOtable1.TableName = "childTable1";
			PDOMap1View = new DataView(PDOtable1);
			PDOMap1View.AllowNew = false;
			PDOtable2 = new PDOMappingTable2();  //need to declare it to prevenyt compiler issues
			PDOtable2.TableName = "childTable2";
			PDOMap2View = new DataView(PDOtable2);
			PDOMap2View.AllowNew = false;
			this.myDataSet.Tables.Clear(); //clear out any old table swhen rereading after error
			this.myDataSet.Tables.Add(COBIDRoutingTable);
			this.myDataSet.Tables.Add(PDOtable1);
			this.myDataSet.Tables.Add(PDOtable2);
			#endregion table and DataView initalisation
		}
		private void fillCOBAndPDOTables()
		{
			#region clear any existing rows - could be a refresh command
			this.COBIDRoutingTable.Rows.Clear();
			this.COBIDRoutingTable.AcceptChanges();
			this.PDOtable1.Rows.Clear();
			PDOtable1.AcceptChanges();
			this.PDOtable2.Rows.Clear();
			PDOtable2.AcceptChanges();
			#endregion clear any existing rows - could be a refresh command
			DataRow PDOtbl1row, PDOtbl2row;
			foreach(COBObject cob in this.localSystemInfo.COBsInSystem)
			{
				if(cob.messageType == COBIDType.PDO)
				{
					if(cob.transmitNodes.Count>1)
					{
						DuplicateCOBFlag = true;
					}
					#region cob routing
					DataRow cobRow = COBIDRoutingTable.NewRow();
					cobRow[(int) COBIDcols.COBID] = cob.requestedCOBID; //this one can change with user request
					cobRow[(int) COBIDcols.OrigCOBID] = cob.COBID;     //but not this one
					//this is used to tell DI that COB ID only has changed ie not a new COB
					foreach(COBObject.PDOMapData txNodeData in cob.transmitNodes)  //this allows us to display duplicate COBIDs to user- error condition
					{
						cobRow[(int) COBIDcols.TxNodeID] = txNodeData.nodeID;
						cobRow[(int) COBIDcols.OrigTXNode] = txNodeData.nodeID;  //stroe the COBID that was passed to GUI from DI
					}
					cobRow[(int) COBIDcols.Type] =  cob.TxType;
					cobRow[(int) COBIDcols.InhibitTime] = cob.inhibitTime;
					cobRow[(int) COBIDcols.EventTime] = cob.eventTime;
					foreach(COBObject.PDOMapData rxNodeData in cob.receiveNodes)
					{
						cobRow[cob.receiveNodes.IndexOf(rxNodeData) + (int) COBIDcols.RxNode1] = rxNodeData.nodeID;
					}
					COBIDRoutingTable.Rows.Add(cobRow);  
					#endregion cob routing
					#region PDO Mapping
					for(int rowIndexer = 0;rowIndexer<MaxSEVCONPDOMapSlots;rowIndexer++)  ///each PDO had a single COBID and 8 mapping slots (may be more than 8 for for third party)
					{
						#region add add 8 rows to the PDO mapping tables and copy the COB data over
						PDOtbl1row = PDOtable1.NewRow();
						PDOtbl2row = PDOtable2.NewRow();
						PDOtbl1row[(int) PDOMappingCols1.COBID] = System.Convert.ToUInt32(cobRow[(int) COBIDcols.COBID]);
						PDOtbl2row[(int) PDOMappingCols2.COBID] = System.Convert.ToUInt32(cobRow[(int) COBIDcols.COBID]);
						PDOtbl1row[(int) PDOMappingCols1.TxNodeID] = System.Convert.ToUInt16(cobRow[(int) COBIDcols.TxNodeID]);
						PDOtbl2row[(int) PDOMappingCols2.TxNodeID] = System.Convert.ToUInt16(cobRow[(int) COBIDcols.TxNodeID]);
						PDOtable1.Rows.Add(PDOtbl1row);
						PDOtable2.Rows.Add(PDOtbl2row);
						#endregion add add 8 rows to the PDO mapping tables and copy the COB data over
					}
					#region now add the Tx mappings
					foreach(COBObject.PDOMapData txData in cob.transmitNodes)
					{
						foreach(long mapVal in txData.mapVals)
						{
							PDOtable1.Rows[txData.mapVals.IndexOf(mapVal)][(int) PDOMappingCols1.TxNode] = mapVal;
						}
					}
					#endregion now add the Tx mappings
					#region add the Rx mappings
					foreach(COBObject.PDOMapData rxData in cob.receiveNodes)
					{
						int nodeInd = cob.receiveNodes.IndexOf(rxData);  //used to dertmine which mapping table to use - two datagrids used for better screen layout
						foreach(long mapVal in rxData.mapVals)
						{
							if(nodeInd < 3)
							{
								PDOtable1.Rows[rxData.mapVals.IndexOf(mapVal)][nodeInd + 1] = mapVal;  //add offset one for TxColumn
							}
							else
							{
								PDOtable2.Rows[rxData.mapVals.IndexOf(mapVal)][nodeInd -3] = mapVal;  //minus offset 3 becasue first 3 Rx nodes ar edisplayed in upper table
							}
						}
					}
					#endregion add the Rx mappings
					updatePDOBitsUsed(localSystemInfo.COBsInSystem.IndexOf(cob));
					#endregion PDO Mapping
					#region accept changes 
					this.COBIDRoutingTable.AcceptChanges();
					PDOtable1.AcceptChanges();
					PDOtable2.AcceptChanges();
					#endregion accept changes 
				}
			}
		}
		private void addDataRelations()
		{
			try
			{
				DataColumn[] parentCols1 = new DataColumn[2];
				DataColumn[] childCols1 = new DataColumn[2];

				DataColumn[] parentCols2 = new DataColumn[2];
				DataColumn[] childCols2 = new DataColumn[2];

				parentCols1[0]=myDataSet.Tables["parentTable"].Columns["COBID"];
				parentCols1[1]=myDataSet.Tables["parentTable"].Columns["TxNodeID"];
				childCols1[0] = myDataSet.Tables["childTable1"].Columns["COBID"];
				childCols1[1] = myDataSet.Tables["childTable1"].Columns["TxNodeID"];

				parentCols2[0]=myDataSet.Tables["parentTable"].Columns["COBID"];
				parentCols2[1]=myDataSet.Tables["parentTable"].Columns["TxNodeID"];
				childCols2[0] = myDataSet.Tables["childTable2"].Columns["COBID"];
				childCols2[1] = myDataSet.Tables["childTable2"].Columns["TxNodeID"];
				DataRelation myDataRelation1 = new DataRelation("parent2Child1", parentCols1, childCols1, false);
				DataRelation myDataRelation2 = new DataRelation("parent2Child2", parentCols2, childCols2,false);
				myDataSet.Tables["ChildTable1"].ParentRelations.Clear();
				myDataSet.Tables["ChildTable1"].ParentRelations.Add(myDataRelation1);
				myDataSet.Tables["ChildTable2"].ParentRelations.Clear();
				myDataSet.Tables["ChildTable2"].ParentRelations.Add(myDataRelation2);
			}
			catch(Exception ex)
			{
				Message.Show("Unable to create data relationships: " + ex.Message);
			}
		}
		private void createTableStyles()
		{
			createCOBRoutingTableStyle();
			createPDOMap1TableStyle();
			if(numNodesPDO>4)
			{
				createPDOMap2TableStyle();
			}
		}
	
		#endregion initialisation

		#region show/hide User Controls methods
		private void hideUserControls()
		{
			this.submitBtn.Visible = false;
			this.dataGrid1.Enabled = false;
			this.dataGrid2.Enabled = false;
			if(numNodesPDO>4)
			{
				this.dataGrid3.Enabled = false;
			}
			this.label1.Text = "Please wait";
		}
		private void showUserControls()
		{
			if(SystemInfo.errorSB.Length>0)
			{
				Form ERROR_SCREEN = new ErrorMessageWindow(SystemInfo.errorSB.ToString());
				SystemInfo.errorSB.Length = 0;
				DialogResult res = ERROR_SCREEN.ShowDialog();
				if(res == DialogResult.OK)
				{
					//do nothing till user hits the OK button
				}
			}
			if(this.viewOnly == false)
			{
				this.submitBtn.Visible = true;
				this.label1.Text = "View and modify PDO Communcations Objects and their parameter mappings";
			}
			else
			{
				this.label1.Text = "View PDO Communcations Objects and their parameter mappings";
				if(this.AllNodesInPreOp == false)
				{
					statusBar1.Text = "System must be in pre-operational to modify settings";
				}
				else
				{
					statusBar1.Text = "Access level too low to modify settings";
				}
			}
			this.dataGrid1.Enabled = true;
			this.dataGrid2.Enabled = true;
			if(numNodesPDO>4)
			{
				this.dataGrid3.Enabled = true;
			}
			this.progressBar1.Visible = false;
			this.panel1.Visible = true;
		}
		#endregion show/hide User Controls methods

		#region user interaction zone
		private void PDOtable1_ColumnChanging(object sender, DataColumnChangeEventArgs e)
		{
			this.label1.Text = PDO_MAPPING_TEXT.PDOComboErrorString;
			//total bits are recalculated here rather than the changed event so that we can
			//still 'see' both old and new values
			switch (e.Column.Ordinal)
			{
				case (int) PDOMappingCols1.TxNode: 
				case (int) PDOMappingCols1.RxNode1: 
				case (int) PDOMappingCols1.RxNode2: 
				case (int) PDOMappingCols1.RxNode3: 
					#region Tx or RX node
					ushort bitsUsed = 0;
					#region did user change this to "not used"
					if(System.Convert.ToInt32(e.ProposedValue) == 0)
					{
						bool notUsedFlag = false;
						this.PDOtable1.ColumnChanged -= new DataColumnChangeEventHandler(PDOtable1_ChangedHandler);
						this.PDOtable1.ColumnChanging -=new DataColumnChangeEventHandler(PDOtable1_ColumnChanging);
						foreach(DataRowView row in this.PDOMap1View)
						{
							if(row.IsEdit == true)  //this is the row the user just set to not used
							{
								notUsedFlag = true;
							}
							if(notUsedFlag == true)
							{
								row.Row[e.Column.Ordinal] = 0;
							}
						}
						this.PDOtable1.ColumnChanged += new DataColumnChangeEventHandler(PDOtable1_ChangedHandler);
						this.PDOtable1.ColumnChanging +=new DataColumnChangeEventHandler(PDOtable1_ColumnChanging);
					}
					#endregion did user change this to "not used"
					#region first add up number of existing bits in this mapping
					foreach(DataRowView row in this.PDOMap1View)
					{
						uint existingBits = System.Convert.ToUInt32(row[e.Column.Ordinal]);
						if(existingBits>0x0)  //used
						{
							bitsUsed += ((ushort) (existingBits & 0xff)); //filter off the last byte which contains th edataLength and then add to total
						}
					}
					//now take away bits in ths row ready for numbe rof Bits check
					uint bitsInThisRow = System.Convert.ToUInt32(e.Row[e.Column.Ordinal]);
					bitsUsed -= ((ushort) (bitsInThisRow & 0xff));
					#endregion first add up number of existing bits in this mapping

					uint requestedBits = System.Convert.ToUInt32(e.ProposedValue);
					if(requestedBits>0x0) //ie user has not entered no used
					{
						bitsUsed += ((ushort) (requestedBits  & 0xff));
					}
					if(bitsUsed > 0x40)  //we would have excessive bits in this mapping
					{
						e.Row.SetColumnError(e.Column.Ordinal, "Invalid mapping, datalength exceeds 8 bytes");
						e.ProposedValue = 0;  
					}
					else
					{
						#region comments
						//the PDO mapping is now within limits so we need to clear all the column errors in 
						//this column. But (and this is the good bit) we can only clear rows by error due to database 
						//type functionality. 
						//So what we do is to grab all the current column error strings 
						//Then we clear all the errors in the table
						//Finally we put all the errors back EXCEPT the ones in this column
						#endregion comments
						#region find all current column errrors, store them and then clear all errors from the table
						//first find all the column errors for this table (ignore COB ID column at end)
						string [,] currErrors = new string[this.PDOtable1.Columns.Count-1 , PDOMap1View.Count]; 
						int j = 0;
						foreach(DataRowView row in this.PDOMap1View)
						{
							for(int i = 0;i<this.PDOtable1.Columns.Count-1;i++) 
							{
								currErrors[i,j] = row.Row.GetColumnError(i);
							}
							row.Row.ClearErrors(); //all the errors in this row have been stroed - so we can clea rhtem from the table
							j++;  //finally point to next row
						}
						#endregion find all current column errrors, stroe them and then clear all errors from the table
						#region re-set the current column errors in all except this column
						j = 0;
						foreach(DataRowView row in this.PDOMap1View)
						{
							for(int i = 0;i<this.PDOtable1.Columns.Count-1;i++) 
							{

								if(i != e.Column.Ordinal)  //not this column
								{
									if(currErrors[i,j] != "")
									{
										row.Row.SetColumnError(i, currErrors[i,j]);  //replace column errors for other nodes
									}
								}
							}
							j++;  //finally point to next row
						}
						#endregion re-set the current column errors in all except this column
					}
					#endregion Tx or RX node
					break;

				default:
					break;
			}
		}
		private void PDOtable2_ColumnChanging(object sender, DataColumnChangeEventArgs e)
		{
			this.label1.Text = PDO_MAPPING_TEXT.PDOComboErrorString;
			//total bits are recalculated here rather than the changed event so that we can
			//still 'see' both old an dnew values
			switch (e.Column.Ordinal)
			{
				case (int) PDOMappingCols2.RxNode4: 
				case (int) PDOMappingCols2.RxNode5: 
				case (int) PDOMappingCols2.RxNode6: 
				case (int) PDOMappingCols2.RxNode7: 
					#region did user change this to "not used"
					if(System.Convert.ToInt32(e.ProposedValue) == 0)
					{
						bool notUsedFlag = false;
						this.PDOtable2.ColumnChanged -= new DataColumnChangeEventHandler(PDOtable2_ChangedHandler);
						this.PDOtable2.ColumnChanging -=new DataColumnChangeEventHandler(PDOtable2_ColumnChanging);
						foreach(DataRowView row in this.PDOMap2View)
						{
							if(row.IsEdit == true)  //this is the row the user just set to not used
							{
								notUsedFlag = true;
							}
							if(notUsedFlag == true)
							{
								row.Row[e.Column.Ordinal] = 0;
							}
						}
						this.PDOtable2.ColumnChanged += new DataColumnChangeEventHandler(PDOtable2_ChangedHandler);
						this.PDOtable2.ColumnChanging +=new DataColumnChangeEventHandler(PDOtable2_ColumnChanging);
					}
					#endregion did user change this to "not used"
					#region test for excessive bits in the mapping

					#region add up number of existing bits requested for this mapping
					ushort bitsUsed = 0;
					foreach(DataRowView row in this.PDOMap2View)
					{
						uint existingBits = System.Convert.ToUInt32(row[e.Column.Ordinal]);
						if(existingBits>0x0)  //used
						{
							bitsUsed += ((ushort) (existingBits & 0xff)); //filter off the last byte which contains th edataLength and then add to total
						}
					}
					//now take away bits in ths row ready for numbe rof Bits check
					uint bitsInThisRow = System.Convert.ToUInt32(e.Row[e.Column.Ordinal]);
					bitsUsed -= ((ushort) (bitsInThisRow & 0xff));
					

					uint requestedBits = System.Convert.ToUInt32(e.ProposedValue);
					if(requestedBits>0x0) //ie user has not entered no used
					{
						bitsUsed += ((ushort) (requestedBits  & 0xff));
					}
					#endregion add up number of existing bits requested for this mapping
					if(bitsUsed > 0x40)  //0x40 is the 8 bytes in a CAN frame - hence same for SC and 3rd Party
					{
						e.Row.SetColumnError(e.Column.Ordinal, "Invalid mapping, datalength exceeds 8 bytes");
						e.ProposedValue = 0; 
					}
					else
					{
						//the PDO mapping is now within limits so we need to clear all the column errors in 
						//this column. But (and this is the good bit) we can only clear rows by error due to database 
						//type functionality. 
						//So what we do is to grab all the current column error strings 
						//Then we clear all the errors in the table
						//Finally we put all the errors back EXCEPT the ones in this column
						#region find all current column errrors, store them and then clear all errors from the table
						//first find all the column errors for this table (ignore COB ID column at end)
						string [,] currErrors = new string[this.PDOtable2.Columns.Count-1 , PDOMap1View.Count]; 
						int j = 0;
						foreach(DataRowView row in this.PDOMap2View)
						{
							for(int i = 0;i<this.PDOtable2.Columns.Count-1;i++) 
							{
								currErrors[i,j] = row.Row.GetColumnError(i);
							}
							row.Row.ClearErrors(); //all the errors in this row have been stored - so we can clear them from the table
							j++;  //finally point to next row
						}
						#endregion find all current column errrors, stroe them and then clear all errors from the table
						#region re-set the current column errors in all except this column
						j = 0;
						foreach(DataRowView row in this.PDOMap2View)
						{
							for(int i = 0;i<this.PDOtable2.Columns.Count-1;i++) 
							{

								if(i != e.Column.Ordinal)  //not this column
								{
									if(currErrors[i,j] != "")
									{
										row.Row.SetColumnError(i, currErrors[i,j]);  //replace column errors for other nodes
									}
								}
							}
							j++;  //finally point to next row
						}
						#endregion re-set the current column errors in all except this column
					}
					#endregion test for excessive bits in the mapping
					break;

				default:
					break;
			}
		}

		private void PDOtable1_ChangedHandler(object sender, System.Data.DataColumnChangeEventArgs e)
		{
			//changing COB IDs are handled by the COB routing table changed
			if((e.Column.Ordinal != (int) PDOMappingCols1.COBID) && (e.Column.Ordinal != (int) PDOMappingCols1.TxNodeID))
			{
				if((bool)COBIDRoutingTable.Rows[dataGrid1.CurrentRowIndex][(int) COBIDcols.Toggle] == true)
				{
					COBIDRoutingTable.Rows[dataGrid1.CurrentRowIndex][(int) COBIDcols.Toggle] = false;
				}
				else
				{
					COBIDRoutingTable.Rows[dataGrid1.CurrentRowIndex][(int) COBIDcols.Toggle] = true;
				}
				#region update column headers
				//update column headers here since they use table and not proposed values
				updatePDOMappColumnHeaders();
				this.dataGrid2.Invalidate();  //force iimediate updat eofd culumn headers displiayed on screen
				#endregion update column headers
				this.linkMappingLists(COBIDRoutingTable.Rows[dataGrid1.CurrentRowIndex]);
			}

		}
		private void PDOtable2_ChangedHandler(object sender, System.Data.DataColumnChangeEventArgs e)
		{
			//changing COB IDs are handled by the COB routing table changed
			if((e.Column.Ordinal != (int) PDOMappingCols2.COBID) && (e.Column.Ordinal != (int) PDOMappingCols2.TxNodeID))
			{
				if((bool)COBIDRoutingTable.Rows[dataGrid1.CurrentRowIndex][(int) COBIDcols.Toggle] == true)
				{
					COBIDRoutingTable.Rows[dataGrid1.CurrentRowIndex][(int) COBIDcols.Toggle] = false;
				}
				else
				{
					COBIDRoutingTable.Rows[dataGrid1.CurrentRowIndex][(int) COBIDcols.Toggle] = true;
				}
				#region update column headers
				//update column headers here since they use table and not proposed values
				updatePDOMappColumnHeaders();
				this.dataGrid3.Invalidate(); //this forces immediate updat eof olumn headers
				#endregion update column headers
				this.linkMappingLists(COBIDRoutingTable.Rows[dataGrid1.CurrentRowIndex]);
			}
		}

		private void COBIDRoutingTable_ColumnChanging(object sender, DataColumnChangeEventArgs e)
		{
			//this.COBIDRoutingTable.ColumnChanging -=new DataColumnChangeEventHandler(COBIDRoutingTable_ColumnChanging);
			string errorString  = "";
			switch (e.Column.Ordinal)
			{
				case ((int) COBIDcols.TxNodeID):
					#region tx Node
					if(System.Convert.ToUInt16(e.ProposedValue) != 0)  //do not error check it,  if it is being set to not used
					{
						#region check for Tx node being set to same Node ID as an RX node (error condition)
						for(int colIndex = ((int) COBIDcols.RxNode1);colIndex<= ((int) COBIDcols.RxNode7);colIndex++)
						{
							if(System.Convert.ToUInt16(e.ProposedValue) == System.Convert.ToUInt16(e.Row[colIndex]))
							{
								statusBar1.Text = "Tx node rejected. Tx node must be different to the Rx nodes";
								e.ProposedValue = e.Row[(int) e.Column.Ordinal]; //change this column to prev value
								errorString = "Tx node rejected. Tx node must be different to the Rx nodes";
								break;
							}
						}
						#endregion check for Tx node being set to same Node ID as an RX node (error condition)
						if( errorString != "")
						{
							#region check for max number of PDOs for this node exceeded
							int proposedNodeID = System.Convert.ToUInt16(e.ProposedValue);
							int nodeRef = 0;
							int maxPDOs = 0;
							feedback = this.localSystemInfo.getNodeNumber(proposedNodeID, out nodeRef);
							if(this.localSystemInfo.nodes[nodeRef].isSevconApplication()==true)	
							{
								maxPDOs = this.MaxSEVCONPDOsAllowed;
							}
							else
							{
								maxPDOs = this.MaxThirdPartyPDOsAllowed;
							}
							int nodeCount = 0;
							for(int i = 0;i<this.COBIDRoutingTable.Rows.Count;i++)
							{
								//count up how many times we have already used this node
								if(System.Convert.ToUInt16(COBIDRoutingTable.Rows[i][e.Column.Ordinal]) == proposedNodeID)
								{
									nodeCount++;
								}
							}
							if(nodeCount >= maxPDOs)
							{
								statusBar1.Text = "Tx node rejected. Only " + maxPDOs.ToString() + " Tx PDOs are available on this node";
								e.ProposedValue = e.Row[(int) e.Column.Ordinal]; //change this column to not used
								errorString = "Tx node rejected. Only " + maxPDOs.ToString() + " Tx PDOs are available on this node";
								break;
							}
							#endregion check for max number of PDOs for this node exceeded
						}
					}
					if(errorString == "")  //change was not rejected so force 
					{
						#region change TX Node ID in PDO tables
						foreach(DataRowView row in this.PDOMap1View)
						{
							row.Row[(int) PDOMappingCols1.TxNodeID] = e.ProposedValue;
						}
						foreach(DataRowView row in this.PDOMap2View)
						{
							row.Row[(int) PDOMappingCols2.TxNodeID] = e.ProposedValue;
						}
						#endregion change TX Node ID in PDO tables
						//judetemp						updatePDOMappColumnHeaders();
					}
					#endregion tx Node
					break;

				case ((int) COBIDcols.RxNode1):
				case ((int) COBIDcols.RxNode2):
				case ((int) COBIDcols.RxNode3):
				case ((int) COBIDcols.RxNode4):
				case ((int) COBIDcols.RxNode5):
				case ((int) COBIDcols.RxNode6):
				case ((int) COBIDcols.RxNode7):
					#region Rx nodes
					if(System.Convert.ToUInt16(e.ProposedValue) != 0) //user always allowed to reset node to - not used
					{
						#region check if selected node is valid
						if(System.Convert.ToUInt16(e.ProposedValue) == System.Convert.ToUInt16(e.Row[(int) COBIDcols.TxNodeID]))
						{
							e.ProposedValue = e.Row[(int) e.Column.Ordinal]; //change this column to not used
							errorString = "Rx node rejected. Rx node must be different to the Tx node";
						}
						#endregion check if selected node is valid
					}
					else
					{
						#region when COB Routing node set to not used then clear the associated mappings
						if(e.Column.Ordinal<((int) COBIDcols.RxNode4))
						{
							foreach (DataRowView row in this.PDOMap1View)
							{
								row.Row[e.Column.Ordinal - (int) (COBIDcols.RxNode1) + 1 ] = 0; //+1 for Tx node
							}
						}
						else
						{
							foreach (DataRowView row in this.PDOMap2View)
							{
								row.Row[e.Column.Ordinal - (int) (COBIDcols.RxNode4)] = 0;
							}
						}
						#endregion when COB Routing node set to not used then clear the associated mappings
					}
					if(errorString == "") //if not rejected then adjust both PDO counts
					{
						#region check for max number of PDOs for this node exceeded
						int proposedNodeID = System.Convert.ToUInt16(e.ProposedValue);
						int nodeRef = 0;
						int maxPDOs = 0;
						feedback = this.localSystemInfo.getNodeNumber(proposedNodeID, out nodeRef);
						if(this.localSystemInfo.nodes[nodeRef].isSevconApplication()==true)	
						{
							maxPDOs = this.MaxSEVCONPDOsAllowed;
						}
						else
						{
							maxPDOs = this.MaxThirdPartyPDOsAllowed;
						}
						int nodeCount = 0;
						for(int i = 0;i<this.COBIDRoutingTable.Rows.Count;i++)
						{
							//count up how many times we have already used this node
							if(System.Convert.ToUInt16(COBIDRoutingTable.Rows[i][e.Column.Ordinal]) == proposedNodeID)
							{
								nodeCount++;
							}
						}
						if(nodeCount >= maxPDOs)
						{
							statusBar1.Text = "Rx node rejected. Only " + maxPDOs.ToString() + " Rx PDOs are available on this node";
							e.ProposedValue = e.Row[(int) e.Column.Ordinal]; //change this column to not used
							errorString = "Rx node rejected. Only " + maxPDOs.ToString() + " Rx PDOs are available on this node";
						}
						#endregion check for max number of PDOs for this node exceeded
					}
					#endregion Rx nodes
					break;

				case ((int) COBIDcols.Type):
					#region Type processing
					errorString = verifyNumericalInput(e.ProposedValue.ToString(), 0xFF, true);
					if(errorString != "")
					{
						statusBar1.Text = "Inserted value of " + e.ProposedValue.ToString() + " was invalid";
						e.ProposedValue = System.Convert.ToUInt32(e.Row[(int) e.Column.Ordinal]).ToString(); //change this column to not used
					}
					#endregion Type processing
					break;

				case ((int) COBIDcols.InhibitTime):
					#region inhibit time processing
					errorString = verifyNumericalInput(e.ProposedValue.ToString(), 0xFFFF, false);
					if(errorString != "")
					{
						statusBar1.Text = "Inserted value of " + e.ProposedValue.ToString() + " was invalid";
						e.ProposedValue = System.Convert.ToUInt32(e.Row[(int) e.Column.Ordinal]).ToString(); //change this column to not used
					}
					#endregion inhibit time processing
					break;

				case ((int) COBIDcols.EventTime):
					#region Event time checks
					errorString = verifyNumericalInput(e.ProposedValue.ToString(), 0xFFFF, false);
					if(errorString != "")
					{
						statusBar1.Text = "Inserted value of " + e.ProposedValue.ToString() + " was invalid";
						e.ProposedValue = System.Convert.ToUInt32(e.Row[(int) e.Column.Ordinal]).ToString(); //change this column to not used
					}

					#endregion Event time checks
					break;

				case ((int) COBIDcols.COBID):
					//Combo col can make row Unchanged at his point 
					//But row will beocome Modified on exit ie prior to user
					//Clicking Submit button
					#region swtitch the row state
					string COBIDToDeleteString = System.Convert.ToUInt32(e.Row[(int) e.Column.Ordinal]).ToString("X");
				switch(e.Row.RowState)
				{
					case DataRowState.Unchanged:
					case DataRowState.Modified:
						#region Unchanged or Modified
						if((System.Convert.ToUInt32(e.ProposedValue)) == COBDeletionMarker) //requset to delete row
						{
							#region user request to delete this row
							string abortMessage = "";
							string message = "Remove this Commumications Object?";
							DialogResult result = Message.Show(this, message, "Confirm", MessageBoxButtons.YesNo,
								MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
							if(result == DialogResult.No)
							{
								e.ProposedValue = System.Convert.ToUInt32(e.Row[(int) e.Column.Ordinal]); //change this column to not used
								statusBar1.Text = "Deletion request for 0x" + COBIDToDeleteString + " cancelled by user";
							}
							else if (result == DialogResult.Yes)
							{
								statusBar1.Text = "Requesting COB deletion";
								this.hideUserControls();
								#region delete rows
								//DI needs to be told to delete row
								
								feedback = this.localSystemInfo.deletePDOMapAndComms(System.Convert.ToInt32(e.Row[(int) COBIDcols.OrigCOBID]));
								if(feedback != DIFeedbackCode.DISuccess)
								{
									StringBuilder sb = new StringBuilder();
									//DI row delete failed so leave our copy in the table 
									e.ProposedValue = System.Convert.ToUInt32(e.Row[(int) e.Column.Ordinal]); //change this column to not used
									statusBar1.Text = "Deletion request denied by System";
									sb.Append("Unable to delete PDO (0x");
									sb.Append(COBIDToDeleteString);
									sb.Append(")\nFeedback code: ");
									sb.Append(feedback.ToString());
									if(abortMessage != "")
									{
										sb.Append("\nError code: ");
										sb.Append(abortMessage);
									}
									Message.Show(sb.ToString());;
								}
								else //success so remove GUI row
								{
									#region delete PDO Mapping rows
									foreach(DataRowView row in this.PDOMap1View)
									{
										row.Row.Delete();
									}
									foreach(DataRowView row in this.PDOMap2View)
									{
										row.Row.Delete();
									}
									int rowCounter = 0;
									int rowPtr = 0;
									int rowCount = this.PDOtable1.Rows.Count;
									while(rowCounter<rowCount)
									{
										if(PDOtable1.Rows[rowPtr].RowState == DataRowState.Deleted)
										{
											PDOtable1.Rows[rowPtr].AcceptChanges();
										}
										else
										{
											rowPtr++;
										}
										rowCounter++;
									}
									rowCounter = 0;
									rowPtr = 0;
									rowCount = this.PDOtable2.Rows.Count;
									while(rowCounter<rowCount)
									{
										if(PDOtable2.Rows[rowPtr].RowState == DataRowState.Deleted)
										{
											PDOtable2.Rows[rowPtr].AcceptChanges();
										}
										else
										{
											rowPtr++;
										}
										rowCounter++;
									}
									#endregion delete PDO Mapping rows
									e.Row.Delete();  //now delete this parent row
									e.Row.AcceptChanges();  //then accept changes on the row 
									statusBar1.Text = "COB 0x"+  COBIDToDeleteString + " deleted";
								}
								#endregion delete rows
								this.updatePDOMappColumnHeaders();  //to force datagrid2, 3 Captions to be changed//
								this.showUserControls();
							}
							#endregion user request to delete this row
							this.applyTableStyleCOBRouting();  //resizes the grid
						}
						else 
						{
							#region change COB IB of existing COB
							uint currCOB = System.Convert.ToUInt32(e.Row[(int)COBIDcols.COBID]);
							foreach(DataRowView row in this.PDOMap1View)
							{
								row.Row[(int) PDOMappingCols1.COBID] = e.ProposedValue;
							}
							foreach(DataRowView row in this.PDOMap2View)
							{
								row.Row[(int) PDOMappingCols2.COBID] = e.ProposedValue;
							}
							changeDatagrid1FocusRowFlag = true;
							#endregion change COB IB of existing COB
						}
						#endregion Unchanged or Modified
						break;

					case DataRowState.Added:
						#region added
						if((System.Convert.ToUInt32(e.ProposedValue)) == COBDeletionMarker) //requset to delete row
						{
							#region user request to delete this row
							string message = "Remove this Commumications Object?";
							DialogResult result = Message.Show(this, message, "Confirm", MessageBoxButtons.YesNo,
								MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
							if(result == DialogResult.No)
							{
								e.ProposedValue = System.Convert.ToUInt32(e.Row[(int) COBIDcols.COBID]);
								statusBar1.Text = "Deletion request for 0x" + COBIDToDeleteString + " cancelled by user";
							}
							else if (result == DialogResult.Yes)
							{
								this.hideUserControls();
								#region delete GUI row only
								#region disable PDO Mapping Event handlers
								#endregion disable PDO Mapping Event handlers
								#region delete PDO Mapping rows
								foreach(DataRowView row in this.PDOMap1View)
								{
									row.Row.RejectChanges(); 
								}
								foreach(DataRowView row in this.PDOMap2View)
								{
									row.Row.RejectChanges(); 
								}
								#endregion delete PDO Mapping rows
								e.Row.RejectChanges();
								#endregion delete GUI row only
								statusBar1.Text = "COB 0x"+  COBIDToDeleteString + " deleted";
								this.updatePDOMappColumnHeaders();  //to force datagrid2, 3 Captions to be changed///
								this.showUserControls();
							}
							#endregion user request to delete this row
							this.applyTableStyleCOBRouting();  //resizes the grid
						}
						else 
						{
							#region change COB IB of existing COB
							uint currCOB = System.Convert.ToUInt32(e.Row[(int)COBIDcols.COBID]);
							foreach(DataRowView row in this.PDOMap1View)
							{
								row.Row[(int) PDOMappingCols1.COBID] = e.ProposedValue;
							}
							foreach(DataRowView row in this.PDOMap2View)
							{
								row.Row[(int) PDOMappingCols2.COBID] = e.ProposedValue;
							}
							changeDatagrid1FocusRowFlag = true;
							#endregion change COB IB of existing COB
						}
						#endregion added
						break;

					case DataRowState.Detached:
						#region DataRowState.Detached
						if(System.Convert.ToInt32(e.ProposedValue) != COBDeletionMarker)  
						{
							if((System.Convert.ToUInt32(e.ProposedValue)) != 0)
							{
								#region user request to add a new COB ID
								try
								{
									this.COBIDRoutingTable.Rows.Add(e.Row);
								}
								catch(Exception ex6)
								{
									Message.Show(ex6.ToString());
									Message.Show("An error has occurred when adding rows to COB Routing table. Error Code: " + ex6.ToString()
										+ "\n this window will close");
									this.Close();
								}

								//now add rows to the PDO mapping tables
								DataRow row;
								for(ushort j = 0;j<8;j++)  ///each PDO had a single COBID and 8 mapping slots (may be more than 8 for for third party)
								{
									row = this.PDOtable1.NewRow();
									row[(int) PDOMappingCols1.COBID] = System.Convert.ToUInt32(e.ProposedValue);
									try
									{
										this.PDOtable1.Rows.Add(row);
									}
									catch(Exception ex7)
									{
										Message.Show("An error has occurred when adding rows to PDO table 1. Error Code: " + ex7.ToString()
											+ "\n this window will close");
										this.Close();
									}
									row = this.PDOtable2.NewRow();
									row[(int) PDOMappingCols2.COBID] = System.Convert.ToUInt32(e.ProposedValue);
									try
									{
										this.PDOtable2.Rows.Add(row);
									}
									catch (Exception ex1)
									{
										Message.Show("An error has occurred when adding rows to PDO table 1. Error Code: " + ex1.ToString()
											+ "\n this window will close");
										this.Close();
									}
								}
								this.applyTableStyleCOBRouting();  //resizes the grid
								this.myNewRow = true;
								#endregion user request to add a new COB ID
							}
						}
						#endregion DataRowState.Detached
						break;

					default:
						break;
				}
					#endregion swtitch the row state
					break;

				default:
					break;
			}
		
			#region errorString display/clear
			if (errorString != "")
			{
				e.Row.SetColumnError(e.Column, errorString);
			}
			else
			{
				#region find all current column errrors, store them and then clear all errors from the table
				//first find all the column errors for this table (ignore COB ID column at end)
				string [] currErrors = new string[this.tablestyle.GridColumnStyles.Count]; 
				for(int i = 0; i<this.tablestyle.GridColumnStyles.Count;i++)
				{
					currErrors[i] = e.Row.GetColumnError(i);
				}
				e.Row.ClearErrors(); //all the errors in this row have been stroed - so we can clea rhtem from the table
				#endregion find all current column errrors, stroe them and then clear all errors from the table
				#region re-set the current column errors in all except this column
				for(int i = 0; i<this.tablestyle.GridColumnStyles.Count;i++)
				{
					if(i != e.Column.Ordinal) //not this column
					{
						e.Row.SetColumnError(i, currErrors[i]);  //replace column errors for other nodes
					}
				}
				#endregion re-set the current column errors in all except this column
				this.statusBar1.Text = "";
			}
			#endregion errorString display/clear
		}
		private void COBIDRoutingTable_ChangedHandler(object sender, System.Data.DataColumnChangeEventArgs e)
		{
			string errorString = "";
			if(DriveWizard.COBRoutingComboColumn.nonUniqueCobID == true)
			{
				if(DuplicateCOBFlag == true)
				{
					errorString = "CANopen violation. Change all COB IDs to unique values";
				}
				else
				{
					errorString = "Non-unique COB ID replaced";
				}
				DriveWizard.COBRoutingComboColumn.nonUniqueCobID = false;
			}
			#region process according to the column type (switch)
			PDOcolumnFromCobRouteTable = 0;  //reset to default
			switch (e.Column.Ordinal)
			{
				case ((int) COBIDcols.TxNodeID):

					errorString = e.Row.GetColumnError(e.Column.Ordinal);
					PDOcolumnFromCobRouteTable = 0;  //datagrid2 column number to move to 
					updatePDOMappColumnHeaders();
					resetDataGrid1FocusRow();
					break;

				case ((int) COBIDcols.RxNode1):
					PDOcolumnFromCobRouteTable = 1;  //datagrid2, 3 column number to move to 
					errorString = e.Row.GetColumnError(e.Column.Ordinal);
					break;

				case ((int) COBIDcols.RxNode2):
					PDOcolumnFromCobRouteTable = 2;  //datagrid2, 3 column number to move to 
					errorString = e.Row.GetColumnError(e.Column.Ordinal);
					break;

				case ((int) COBIDcols.RxNode3):
					PDOcolumnFromCobRouteTable = 3;  //datagrid, 32 column number to move to 
					errorString = e.Row.GetColumnError(e.Column.Ordinal);
					break;

				case ((int) COBIDcols.RxNode4):
					PDOcolumnFromCobRouteTable = 0;  //datagrid3 column number to move to 
					errorString = e.Row.GetColumnError(e.Column.Ordinal);
					break;

				case ((int) COBIDcols.RxNode5):
					PDOcolumnFromCobRouteTable = 1;  //datagrid2, 3 column number to move to 
					errorString = e.Row.GetColumnError(e.Column.Ordinal);
					break;

				case ((int) COBIDcols.RxNode6):
					PDOcolumnFromCobRouteTable = 2;  //datagrid2, 3 column number to move to 
					errorString = e.Row.GetColumnError(e.Column.Ordinal);
					break;

				case ((int) COBIDcols.RxNode7):
					PDOcolumnFromCobRouteTable = 3;  //datagrid, 32 column number to move to 
					errorString = e.Row.GetColumnError(e.Column.Ordinal);
					break;

				case ((int) COBIDcols.COBID):
					#region switch RowState
				switch (e.Row.RowState)
				{
					case DataRowState.Modified:
					case DataRowState.Unchanged:  
					case DataRowState.Added:
						updatePDOMappColumnHeaders();
						//Combo col can make row Unchanged at his point 
						//But row will beocome Modified on exit ie prior to user
						//Clicking Submit button
						if( changeDatagrid1FocusRowFlag == true)
						{
							changeDatagrid1FocusRowFlag = false;
							resetDataGrid1FocusRow();
						}
						break;

					default:
						break;
				}
					#endregion switch RowState
					break;

				default:
					return;
			}
			#endregion process according to the column type (switch)
			#region errorString display/clear
			if (errorString != "")
			{
				e.Row.SetColumnError(e.Column, errorString);
				statusBar1.Text = errorString;
			}
			else
			{
				#region find all current column errrors, store them and then clear all errors from the table
				//first find all the column errors for this table (ignore COB ID column at end)
				string [] currErrors = new string[this.tablestyle.GridColumnStyles.Count]; 
				for(int i = 0; i<this.tablestyle.GridColumnStyles.Count;i++)
				{
					currErrors[i] = e.Row.GetColumnError(i);
				}
				e.Row.ClearErrors(); //all the errors in this row have been stroed - so we can clea rhtem from the table
				#endregion find all current column errrors, stroe them and then clear all errors from the table
				#region re-set the current column errors in all except this column
				for(int i = 0; i<this.tablestyle.GridColumnStyles.Count;i++)
				{
					if(i != e.Column.Ordinal) //not this column
					{
						e.Row.SetColumnError(i, currErrors[i]);  //replace column errors for other nodes
					}
				}
				#endregion re-set the current column errors in all except this column
			}
			#endregion errorString display/clear
			this.linkMappingLists(e.Row);
		}
		private string verifyNumericalInput(string input, uint MaxAllowed, bool isTypeCol)
		{
			if(input.Length == 0)
			{
				return "Numerical or Hexadeicmal input required";
			}
			input = input.ToUpper();  //reduces invalid char checking
			string inputcopy = input;
			if(inputcopy.Length>2)
			{
				inputcopy = inputcopy.Substring(0,2);
			}
			if(inputcopy == "0X")//we have hex input
			{
				input = input.Remove(0,2); //remove the 0X
				if(input.Length>4)  //max is 0xFFFF 
				{
					return "Out of range, maximum value is 0x" + MaxAllowed.ToString("X");
				}
				char [] invalidChars = "GHIJKLMNOPQRSTUVWXYZ!\"£$%^&*()_+=\\|,<>?/:;'@#~{[}]".ToCharArray(); //" and \ need extra \to be accepted
				int invalidCharIndex = input.IndexOfAny(invalidChars);
				if(invalidCharIndex != -1)
				{
					return  "Numerical or Hexadeicmal input required";
				}
				else
				{
					int inputValue = Convert.ToInt32(input, 16); //convert to base 10 number
					if( inputValue> MaxAllowed)
					{
						return "Out of range, maximum value is " + MaxAllowed.ToString();
					}
					if(inputValue<0)
					{
						return "Out of range, minimum value is zero";
					}
					if(isTypeCol == true)
					{
						if((inputValue >0xF0) && (inputValue<0xFC))
						{
							return " Values 0xF1 to 0xFC are currently Reserved by CiA";
						}
					}
				}
			}
			else  //decimal input
			{
				char [] invalidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ!\"£$%^&*()_+=\\|,<>?/:;'@#~{[}]".ToCharArray(); //" and \ need extra \to be accepted
				int invalidCharIndex = input.IndexOfAny(invalidChars);
				if(invalidCharIndex != -1)
				{
					return "Numerical or Hexadeicmal input required";
				}
				else
				{
					if(input.Length>5)  //max decimal is 65535
					{
						return "Out of range, maximum value is " + MaxAllowed.ToString();
					}
					int inputValue = Convert.ToInt32(input); //convert to base 10 number
					if( inputValue> MaxAllowed)
					{
						return "Out of range, maximum value is " + MaxAllowed.ToString();
					}
					if(inputValue<0)
					{
						return "Out of range, minimum value is zero";
					}
					if(isTypeCol == true)
					{
						if((inputValue >240) && (inputValue<252))
						{
							return " Values 241 to 251 are currently Reserved by CiA";
						}
					}
				}
			}
			return "";
		}
		private void dataGrid1_Click(object sender, System.EventArgs e)
		{
			Point pt = this.dataGrid1.PointToClient(Control.MousePosition);
			DataGrid.HitTestInfo hti = this.dataGrid1.HitTest(pt);
			this.label1.Text = PDO_MAPPING_TEXT.PDOComboErrorString;  //update the information on user clicking on datagrid
			//check mouse was clicked over a cell
			//in a cell, completed data loading and not the end 'blank ' row
			if((hti.Type == DataGrid.HitTestType.Cell) && (this.timer1.Enabled == false) && (this.COBIDRoutingTable.Rows.Count>this.dataGrid1.CurrentRowIndex))
			{
				if(hti.Column != ((int) COBIDcols.COB_Valid))  
				{
					return;
				}
				this.dataGrid1[hti.Row, hti.Column] = !((bool)this.dataGrid1[hti.Row, hti.Column]);  //force the toggle 
				this.linkMappingLists(this.COBIDRoutingTable.Rows[hti.Row]);
			}
		}

		private void PDOMapdataGrid_Click(object sender, System.EventArgs e)
		{
			this.label1.Text = PDO_MAPPING_TEXT.PDOComboErrorString;;
		}
		private void updatePDOBitsUsed (int COBRoutingRowNum)
		{		
			ushort [] bitsUsed1 = new ushort[4];
			ushort [] bitsUsed2 = new ushort[4];

			foreach(DataRowView row in this.PDOMap1View)
			{
				for(int j = 0;j<4;j++)
				{
					uint bitsTotal = System.Convert.ToUInt32(row[j]);
					if(bitsTotal>0x0)  //used
					{
						bitsUsed1[j] += (ushort) (bitsTotal & 0xff);
					}
				}
			}
			foreach(DataRowView row in this.PDOMap2View)
			{
				for(int j = 0;j<4;j++)
				{
					uint bitsTotal = System.Convert.ToUInt32(row[j]);
					if(bitsTotal>0x0)  //used
					{
						bitsUsed2[j] += (ushort)(bitsTotal & 0xff);
					}
				}
			}
		}
		private void updatePDOMappColumnHeaders()
		{
			if((this.COBIDRoutingTable.Rows.Count>0) 
				&& (dataGrid1.CurrentRowIndex !=-1) 
				&&(this.COBIDRoutingTable.Rows.Count>this.dataGrid1.CurrentRowIndex) )
			{
				string currentCOBID = COBIDRoutingTable.Rows[dataGrid1.CurrentRowIndex][(int) COBIDcols.COBID].ToString();
				string currentTXNodeID = COBIDRoutingTable.Rows[dataGrid1.CurrentRowIndex][(int) COBIDcols.TxNodeID].ToString();
				#region redo the PDO views here inc ase we have just deleted a row
				this.PDOMap1View.RowFilter =  PDOMappingCols1.COBID.ToString() + " = '" + currentCOBID + "' AND " + PDOMappingCols1.TxNodeID.ToString() + " = '" + currentTXNodeID + "'";
				this.PDOMap2View.RowFilter =  PDOMappingCols2.COBID.ToString() + " = '" + currentCOBID + "' AND " + PDOMappingCols2.TxNodeID.ToString() + " = '" + currentTXNodeID + "'";
				#endregion redo the PDO views here inc ase we have just deleted a row

				#region total the bits used and display
				ushort [] bitsUsed1 = new ushort[4];
				ushort [] bitsUsed2 = new ushort[4];
				foreach(DataRowView row in this.PDOMap1View)
				{
					for(int j = 0;j<4;j++)
					{
						uint bitsTotal = System.Convert.ToUInt32(row[j]);
						if(bitsTotal>0x0)  //used
						{
							bitsUsed1[j] += (ushort) (bitsTotal & 0xff);
						}
					}
				}
				foreach(DataRowView row in this.PDOMap2View)
				{
					for(int j = 0;j<4;j++)
					{
						uint bitsTotal = System.Convert.ToUInt32(row[j]);
						if(bitsTotal>0x0)  //used
						{
							bitsUsed2[j] += (ushort)(bitsTotal & 0xff);
						}
					}
				}
				#endregion total the bits used and display

				#region updatecolumn headers
				for(int i = 0;i<this.PDOMap1Percents.Length;i++)
				{
					tablestyle2.GridColumnStyles[i].HeaderText = this.PDOMap1ColHeadings[i] + bitsUsed1[i].ToString("X") + " bits used)";
				}
				if(this.PDOMap2Percents != null)
				{
					for(int i = 0;i<this.PDOMap2Percents.Length;i++)
					{
						tablestyle3.GridColumnStyles[i].HeaderText = this.PDOMap2ColHeadings[i] + bitsUsed2[i].ToString("X")+ " bits used)";
					}
				}
				#endregion updatecolumn headers

				#region now do the datagrid captions
				uint COBValue = System.Convert.ToUInt32(this.COBIDRoutingTable.Rows[this.dataGrid1.CurrentRowIndex]["COBID"]);
				string currentCOBIDStr = COBValue.ToString("X");
				while(currentCOBIDStr.Length<4)
				{
					currentCOBIDStr = "0" + currentCOBIDStr;
				}
				dataGrid2.CaptionText = "PDO Mapping for COB ID 0x" + currentCOBIDStr;
				dataGrid3.CaptionText = "PDO Mapping for COB ID 0x" + currentCOBIDStr + " (continued)";
				#endregion  now do the datagrid captions

				//update the links between the PDO mapping table son screen and the correc tunderlying Drop down list 
			}
			else  //we are on an undefined or detached row 
			{
				this.dataGrid2.CaptionText = "No COB selected";
				this.dataGrid3.CaptionText = "No COB selected";
			}
			
		}
		private void submitBtn_Click(object sender, System.EventArgs e)
		{
			SystemInfo.errorSB = new StringBuilder();
			if(DuplicateCOBFlag == true)
			{
				#region handle duplicate COBIDs
				this.label1.Text = "Please wait";
				statusBar1.Text = "Requesting deletion of duplicate Commumication Objects";

				SystemInfo.errorSB.Length = 0;
				foreach(COBObject COB in this.localSystemInfo.COBsInSystem)
				{
					if((COB.messageType == COBIDType.PDO)//we only delete duplicate PDOs - what to do with other is undefined 
						&& (COB.transmitNodes.Count>1))
					{//ie a duplicate PDO COB - try and delete them all 
					feedback = this.localSystemInfo.deletePDOMapAndComms(COB.requestedCOBID);
					}
				}
				if(SystemInfo.errorSB.Length>0)
				{
					//judetemp - display the Error screen here
					SystemInfo.errorSB.Length = 0;  //reset the errSB B&B
				}
				else
				{
					Message.Show("DriveWizard has requested nodes to remove non-compliant COBs. \n Now Cycle power and then and login to Sevcon nodes");
				}
				DriveWizard.MAIN_WINDOW.reEstablishCommsRequired = true; 
				this.Close();
				#endregion handle duplicate COBIDs
			}
			else
			{
				bool dataRejected = false;
				#region comments
				//Click on Submit button will error check each COB and its associated PDO Mappings

				//Process will be
				//1 .Update Status Bar to show which COB ID is being handled
				//2. Check the COB routing parameters
				//3. If the COB Routing has an error, Stop and tell user
				//4. If COB routing checks passed, then Check the PDO maps 
				//5. If any PDO map fails checks then don't send anything, tell user
				//6. If every thing passed then transmit COB routing info and associated mapping info to DI/controller
				#endregion comments
				hideUserControls();
				#region Check COB ID Routing rows
				int OrigCOBCount = COBIDRoutingTable.Rows.Count; //take a snap shot because if we delete rows then
				//COBIDRoutingTable.Rows.Count will change so a for loop here would fail for multiple row deletes
				int COBIndexer = 0;
				int rowCounter = 0;
				bool addedRowDeleted = false;
				while(rowCounter<OrigCOBCount)
				{
					string SubmitCOBIDString = System.Convert.ToUInt32(COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.COBID]).ToString("X");
					string submitErrorString = "";
					if(SubmitCOBIDString == "0")  //user added a row but did not select a COB ID
					{
						submitErrorString = "COB ID not defined";
					}
					if(this.COBIDRoutingTable.Rows[COBIndexer].RowState != DataRowState.Unchanged)
					{
						#region create stroage object for this COB and change PDO rowfilter to select mappping for this COB
						COBObject submitCOB = new COBObject();
						this.PDOMap1View.RowFilter =  PDOMappingCols1.COBID.ToString() + " = '" + this.COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.COBID].ToString() + "'";
						this.PDOMap2View.RowFilter =  PDOMappingCols2.COBID.ToString() + " = '" + this.COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.COBID].ToString() + "'";
						//cob Id number as a hex string for error meesages
						string COBIDStr = System.Convert.ToUInt32(this.COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.COBID]).ToString("X");
						#endregion create stroage objec tfor this COB and change PDO rowfilter to select mappping for this COB
						#region clear row error flags
						if(this.COBIDRoutingTable.Rows[COBIndexer].HasErrors)
						{
							this.COBIDRoutingTable.Rows[COBIndexer].ClearErrors();
						}
						#endregion
						#region comments
						//each modified row must meet the following requirements to be accepted as a valid COB definition
						//1. Tx node must bbe defined
						//2. The COB ID must be unique
						//3. What has to be connected ot the bus - eg Master, Tx node etc etc ????
						#endregion comments
						#region perform error checking of added and modified COB Routing Table rows
						#region test for defined Tx node
						ushort txNode = System.Convert.ToUInt16(COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.TxNodeID]);
						if( txNode == 0)
						{
							submitErrorString += "\n COBID 0x" + COBIDStr + " not updated. Tx node not defined";
						}
							#endregion test for defined Tx node
						else
						{
							int loopLimit = System.Math.Max(2, (numNodesPDO-1)); //single node can be Tx and Rx
							for (int i = 0 ;i<loopLimit;i++)  //otherwise number of Rx nodes will be numNodesPDO - 1 
							{
								if(txNode == (System.Convert.ToUInt16(this.COBIDRoutingTable.Rows[COBIndexer][(int) (COBIDcols.RxNode1) + i]) ))
								{
									submitErrorString = "\n COBID 0x" + COBIDStr + " not updated. Tx node cannot also be an Rx node";
								}
							}
						}

						#endregion perform error checking of COB Routing Table rows
						if(submitErrorString != "")  //problem with COB Routing info - do not send data to DI
						{
							#region GUI Error handling of failed rows
							Message.Show(submitErrorString);  
							if(COBIDRoutingTable.Rows[COBIndexer].RowState == DataRowState.Modified)
							{
								#region RejectChanges() on COB Routing and associated PDO mapping rows that we are not sending to the DI
								for (int i= 0;i<this.PDOtable1.Rows.Count;i++)
								{
									if(PDOtable1.Rows[i][(int) PDOMappingCols1.COBID].ToString() 
										== this.COBIDRoutingTable.Rows[ COBIndexer][(int) COBIDcols.COBID].ToString())
									{
										PDOtable1.Rows[i].RejectChanges();
									}
								}
								for (int i= 0;i<this.PDOtable2.Rows.Count;i++)
								{
									if(PDOtable2.Rows[i][(int) PDOMappingCols2.COBID].ToString() 
										== this.COBIDRoutingTable.Rows[ COBIndexer][(int) COBIDcols.COBID].ToString())
									{
										PDOtable2.Rows[i].RejectChanges();
									}
								}
								this.COBIDRoutingTable.Rows[COBIndexer].RejectChanges();
								#endregion RejectChanges() on COB Routing and associated PDO mapping rows
							}
							else if(COBIDRoutingTable.Rows[COBIndexer].RowState == DataRowState.Added)
							{
								#region delete all the added rows
								for (int i= 0;i<this.PDOtable1.Rows.Count;i++)
								{
									if(PDOtable1.Rows[i][(int) PDOMappingCols1.COBID].ToString() 
										== this.COBIDRoutingTable.Rows[ COBIndexer][(int) COBIDcols.COBID].ToString())
									{
										PDOtable1.Rows[i].Delete();
									}
								}
								for (int i= 0;i<this.PDOtable2.Rows.Count;i++)
								{
									if(PDOtable2.Rows[i][(int) PDOMappingCols2.COBID].ToString() 
										== this.COBIDRoutingTable.Rows[ COBIndexer][(int) COBIDcols.COBID].ToString())
									{
										PDOtable2.Rows[i].Delete();
									}
								}
								COBIDRoutingTable.Rows[COBIndexer].Delete();
								addedRowDeleted = true;
								#endregion delete all the added rows
							}
							#region PDO tables
							int rowIndexer = 0;
							int rowCount = this.PDOtable1.Rows.Count;
							while(rowIndexer<rowCount)
							{
								if(this.PDOtable1.Rows[rowIndexer].RowState == DataRowState.Deleted)
								{
									PDOtable1.Rows[rowIndexer].AcceptChanges();
								}
								rowIndexer++;
							}
							rowIndexer = 0;
							rowCount = this.PDOtable2.Rows.Count;
							while(rowIndexer<rowCount)
							{
								if(this.PDOtable2.Rows[rowIndexer].RowState == DataRowState.Deleted)
								{
									PDOtable2.Rows[rowIndexer].AcceptChanges();
								}
								rowIndexer++;
							}
							#endregion APDO Tables
							#endregion GUI Error handling of failed rows
						}
						else
						{
							#region fill submitCOB
							submitCOB.requestedCOBID = System.Convert.ToInt32(this.COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.COBID]);
							#region type column
							string myType = COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.Type].ToString().ToUpper();
							int tempTest =0;
							if(myType.Length>2)
							{
								myType = myType.Substring(0,2);
							}
							if(myType == "0X")//we have hex input
							{
								myType = COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.Type].ToString().Remove(0,2);
								tempTest = System.Convert.ToInt32(myType, 16);
							}
							else
							{
								myType = COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.Type].ToString();
								tempTest = System.Convert.ToInt32(myType);
							}
							submitCOB.TxType = tempTest;
							#endregion type column
							#region Inhibit time
							string myInhibit = COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.InhibitTime].ToString().ToUpper();
							if(myInhibit.Length>2)
							{
								myInhibit = myInhibit.Substring(0,2);
							}
							if(myInhibit == "0X")//we have hex input
							{
								myInhibit = COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.InhibitTime].ToString().Remove(0,2);
								tempTest = System.Convert.ToInt32(myInhibit, 16);
							}
							else
							{
								myInhibit = COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.InhibitTime].ToString();
								tempTest = System.Convert.ToInt32(myInhibit);
							}
							submitCOB.inhibitTime = tempTest;
							#endregion Inhibit time
							#region Event time
							string myEvent = COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.EventTime].ToString().ToUpper();
							if(myEvent.Length>2)
							{
								myEvent = myEvent.Substring(0,2);
							}
							if(myEvent == "0X")//we have hex input
							{
								myEvent = COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.EventTime].ToString().Remove(0,2);
								tempTest = System.Convert.ToInt32(myEvent, 16);
							}
							else
							{
								myEvent = COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.EventTime].ToString();
								tempTest = System.Convert.ToInt32(myEvent);
							}
							submitCOB.eventTime = tempTest;
							#endregion Event time
							#region create transmitNodes array list and add node ID and mappings
							COBObject.PDOMapData txData = new COBObject.PDOMapData();
							txData.nodeID = System.Convert.ToInt32(this.COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.TxNodeID]);
							foreach(DataRowView row in this.PDOMap1View)
							{
								long temp = System.Convert.ToInt64(row.Row[(int)PDOMappingCols1.TxNode]);
								if(temp != 0)
								{
									txData.mapVals.Add(temp);
								}
								else
								{ //end of mappings reached - have to be contiguous
									break;
								}
							}
							submitCOB.transmitNodes.Add(txData);
							#endregion  create transmitNodes array list
							#region create the receiveNodes ArrayList and add nodeID and map data
							for(int i = (int)COBIDcols.RxNode1; i<(int) COBIDcols.RxNode7;i++)
							{
								if(System.Convert.ToUInt16(this.COBIDRoutingTable.Rows[COBIndexer][i]) != 0)
								{
									COBObject.PDOMapData rxData = new COBObject.PDOMapData();
									rxData.nodeID = System.Convert.ToInt32(this.COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.RxNode1 + i]);
									if(i <3)
									{
										#region map data in upper table
										foreach(DataRowView row in this.PDOMap1View)
										{
											long temp = System.Convert.ToInt64(row.Row[i + 1]);  //add one for the Tx column
											if(temp != 0)
											{
												rxData.mapVals.Add(temp);
											}
											else
											{ //end of mappings reached - have to be contiguous
												break;
											}
										}
										#endregion map data in upper table
									}
									else
									{
										#region map data in lower table
										foreach(DataRowView row in this.PDOMap2View)
										{
											long temp = System.Convert.ToInt64(row.Row[i - 3]);  //first three Rx nodes are hanadled by upper table
											if(temp != 0)
											{
												rxData.mapVals.Add(temp);
											}
											else
											{  //we have reached end of mappings
												break;
											}
										}
										#endregion map data in lower table
									}
									submitCOB.receiveNodes.Add(rxData);
								}
								else
								{
									break;  //on first zero - get out
								}
							}
							#endregion  create the receiveNodes ArrayList
							#endregion fill submitCOB
						}
						#region pass data to DI
						if(submitErrorString == "")
						{
							statusBar1.Text = "Requesting system changes";
							this.label1.Text = "Please Wait";
							string abortMessage = "";
							if( 
								(COBIDRoutingTable.Rows[COBIndexer].RowState == DataRowState.Modified)
								&& (
								(System.Convert.ToUInt32(COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.COBID]) !=
								System.Convert.ToUInt32(COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.OrigCOBID]) )
								|| (System.Convert.ToUInt16(COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.TxNodeID]) !=
								System.Convert.ToUInt16(COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.OrigTXNode]) )
								)
								)
							{
								#region modified COB ID
								feedback = this.localSystemInfo.modifyExistingPDOMapAndComms(ref submitCOB );
								if(feedback != DIFeedbackCode.DISuccess)
								{
									Message.Show("Unable to change COB ID. Error code: " + feedback.ToString() + " " + abortMessage);
									dataRejected = true;
								}
								#endregion modified COB ID
							}
							else if(
								(COBIDRoutingTable.Rows[COBIndexer].RowState == DataRowState.Added) 
								|| (COBIDRoutingTable.Rows[COBIndexer].RowState == DataRowState.Modified 
								&&
								(System.Convert.ToUInt32(COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.COBID]) ==
								System.Convert.ToUInt32(COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.OrigCOBID]))
								&& (System.Convert.ToUInt16(COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.TxNodeID]) ==
								System.Convert.ToUInt16(COBIDRoutingTable.Rows[COBIndexer][(int) COBIDcols.OrigTXNode])))
								)
							{
								#region added COB or COB ID not changed
								feedback = this.localSystemInfo.writePDOMapAndComms(ref submitCOB);
								if(feedback != DIFeedbackCode.DISuccess)
								{
									StringBuilder sb = new StringBuilder();
									sb.Append("Error writing COBID 0x");
									sb.Append(COBIDStr);
									sb.Append(": " );
									sb.Append(feedback.ToString() );
									if(abortMessage != "")
									{
										sb.Append("\nabortMessage: " );
										sb.Append(abortMessage);
									}
									Message.Show(sb.ToString());
									dataRejected = true;
								}
								else
								{
									Message.Show("COB 0x" + SubmitCOBIDString + " updated OK");
								}
								#endregion added COB or COB ID not changed
							}
							else
							{
								Message.Show("Unhandled PDO modification .Please report");
							}
						}
						#endregion pass data to DI
					}
					else
					{
						Message.Show("No changes required to COB ID 0x" + SubmitCOBIDString);
					}
					statusBar1.Text = "";
					this.label1.Text = "";
					if(addedRowDeleted == false)
					{
						COBIndexer++;
					}
					else
					{
						addedRowDeleted = false;
					}
					rowCounter++;
				}
				#endregion  Check COB ID Routing rows
				if(dataRejected == true)
				{
					#region re-fill tables from DI data
					this.label1.Text = "Please wait";
					statusBar1.Text = "Reading controller data";
					#region data retrieval thread
					PDOdataRefreshThread = new Thread(new ThreadStart( readCOBsInSystem )); 
					PDOdataRefreshThread.Name = "COBIDDataRetrieval";
					PDOdataRefreshThread.IsBackground = true;
					PDOdataRefreshThread.Priority = ThreadPriority.BelowNormal;
#if DEBUG
					System.Console.Out.WriteLine("Thread: " + PDOdataRefreshThread.Name + " started");
#endif

					PDOdataRefreshThread.Start(); 
					this.timer1.Enabled = true;
					#endregion data retrieval thread

					#endregion re-fill tables from DI data
				}
				else
				{
					COBIDRoutingTable.AcceptChanges();
					PDOtable1.AcceptChanges();
					PDOtable2.AcceptChanges();
				}
				if(PDOdataRefreshThread == null)
				{
					showUserControls();
				}
			}
		}
		private void dataGrid1_CurrentCellChanged(object sender, System.EventArgs e)
		{
			updatePDOMappColumnHeaders(); 
			if((dataGrid1.CurrentRowIndex>=0) && (dataGrid1.CurrentRowIndex<this.COBIDRoutingTable.Rows.Count))
			{
				this.linkMappingLists(this.COBIDRoutingTable.Rows[dataGrid1.CurrentRowIndex]);
			}
		}

		#endregion user interaction zone

		#region minor methods
		private void createCOBRoutingTableStyle()
		{
			int [] colWidths  = new int[COBRoutingPercents.Length];
			colWidths  = SCCorpStyle.calculateColumnWidths(ref this.dataGrid1, COBRoutingPercents, 0, dataGrid1DefaultHeight);
			tablestyle = new COBID_TS(colWidths, COBIDsTable, NodeNumsTable, viewOnly, ref COBIDRoutingTable);
			tablestyle.MappingName = COBIDRoutingTable.TableName;
			dataGrid1.TableStyles.Clear();
			dataGrid1.TableStyles.Add(tablestyle);
		}
		private void applyTableStyleCOBRouting()
		{
			int [] colWidths  = new int[COBRoutingPercents.Length];
			colWidths  = SCCorpStyle.calculateColumnWidths(ref this.dataGrid1, COBRoutingPercents, 0, dataGrid1DefaultHeight);
			for (int i = 0;i<COBRoutingPercents.Length;i++)
			{
				tablestyle.GridColumnStyles[i].Width = colWidths[i];
			}
			//Add 3 to Rows.Count - For Caption row, Column headers row and the 'new' row at bottom of table
			float preferredHeight = (int) ((this.COBIDRoutingTable.Rows.Count + 3 ) * this.dataGrid1.PreferredRowHeight) + 30;  //add 2 rows for caption an dcolumn header rows
			if(numNodesPDO<=4)
			{
				this.dataGrid1.Height = System.Convert.ToInt32(System.Math.Min(preferredHeight, (panel1.Height/2)));  //we get the smaller of the two to force scroll bars as necessary
			}
			else
			{
				this.dataGrid1.Height = System.Convert.ToInt32(System.Math.Min(preferredHeight, (panel1.Height/3)));  //we get the smaller of the two to force scroll bars as necessary
			}

		}
		private void createPDOMap1TableStyle()
		{
			int [] colWidths  = new int[PDOMap1Percents.Length];
			colWidths  = SCCorpStyle.calculateColumnWidths(ref this.dataGrid1, PDOMap1Percents, 0, dataGrid1DefaultHeight);
			tablestyle2 = new PDO1_TS(colWidths, viewOnly, PDOMap1ColHeadings);  
			tablestyle2.MappingName = this.PDOtable1.TableName;
			dataGrid2.TableStyles.Clear();
			dataGrid2.TableStyles.Add(tablestyle2);
		}
		private void applyTableStylePDO1()
		{
			int [] colWidths  = new int[PDOMap1Percents.Length];
			colWidths  = SCCorpStyle.calculateColumnWidths(ref this.dataGrid1, PDOMap1Percents, 0, dataGrid1DefaultHeight);
			for (int i = 0;i<PDOMap1Percents.Length;i++)
			{
				tablestyle2.GridColumnStyles[i].Width = colWidths[i];
			}
			//Add 2 to Rows.Count - For Caption row & Column headers row
			float preferredHeight = (int) ((maxSEVCONPDOMappingsAllowed + 2 ) * this.dataGrid2.PreferredRowHeight) + 30;  //add 2 rows for caption an dcolumn header rows
			if(numNodesPDO<=4)
			{
				this.dataGrid2.Height = System.Convert.ToInt32(System.Math.Min(preferredHeight, (panel1.Height/2)-20));  //we get the smaller of the two to force scroll bars as necessary
				this.dataGrid2.Top = (this.panel1.Height/2)+ 20;  //slight seperation from COB Routing grid, for readbility
			}
			else
			{
				this.dataGrid2.Height = System.Convert.ToInt32(System.Math.Min(preferredHeight, (panel1.Height/3)-20));  //we get the smaller of the two to force scroll bars as necessary
				this.dataGrid2.Top = (this.panel1.Height/3)+ 20;  //slight seperation from COB Routing grid, for readbility
			}
		}
		private void createPDOMap2TableStyle()
		{
			int [] colWidths  = new int[PDOMap2Percents.Length];
			colWidths  = SCCorpStyle.calculateColumnWidths(ref this.dataGrid1, PDOMap2Percents, 0, dataGrid1DefaultHeight);
			tablestyle3 = new PDO2_TS(colWidths, viewOnly, PDOMap2ColHeadings); 
			tablestyle3.MappingName = this.PDOtable2.TableName;
			dataGrid3.TableStyles.Clear();
			dataGrid3.TableStyles.Add(tablestyle3);
		}
		private void applyTableStylePDO2()
		{
			int [] colWidths  = new int[PDOMap2Percents.Length];
			colWidths  = SCCorpStyle.calculateColumnWidths(ref this.dataGrid1, PDOMap2Percents, 0,dataGrid1DefaultHeight);
			for (int i = 0;i<PDOMap2Percents.Length;i++)
			{
				tablestyle3.GridColumnStyles[i].Width = colWidths[i];
			}
			//Add 2 to Rows.Count - For Caption row & Column headers row
			float preferredHeight = (int) ((maxSEVCONPDOMappingsAllowed + 2 ) * this.dataGrid3.PreferredRowHeight) + 30;  //add 2 rows for caption an dcolumn header rows
			this.dataGrid3.Height = System.Convert.ToInt32(System.Math.Min(preferredHeight, (panel1.Height/3)-20));  //we get the smaller of the two to force scroll bars as necessary
			this.dataGrid3.Top = this.dataGrid2.Bottom-2;  //-2 is to avoid thick line between the grids
		}
		
		private void resetDataGrid1FocusRow()
		{
			#region move focus to correct row in COB routing datagrid
			this.dataGrid1.Invalidate();
			this.dataGrid1.Focus();
			int currRow = this.dataGrid1.CurrentRowIndex;
			this.dataGrid1.CurrentCell = new DataGridCell((currRow + 1), this.dataGrid1.CurrentCell.ColumnNumber);
			this.dataGrid1.Focus();
			this.dataGrid1.CurrentCell = new DataGridCell((currRow), this.dataGrid1.CurrentCell.ColumnNumber);
			#endregion move focus to correct row in COB routing datagrid
		}
		private void dataGrid1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			//this is the only way to get the new new to be reflected in the lower 
			//datagrids because the new datagrid blank row HAS to be added first before
			// we change rows and then back to current row to force lower grids to update
			// If you're reading this in 6 months Jude then No it won't work by catching an earlier event  or
			// by moving up a row instead of down a row - believe me I tried them all 
			//- this is the only event that works (even though I know it is bad form to override Paint)

			if(this.myNewRow == true)
			{
				this.myNewRow = false;
				this.dataGrid1.CurrentRowIndex--;
				this.dataGrid1.CurrentRowIndex++;
			}
		}
		private void panel1_Resize(object sender, System.EventArgs e)
		{
			if(this.viewOnly == true)
			{
				this.dataGrid1.ReadOnly = false;
			}
			if((timer1.Enabled == false) && (COBIDsTable != null))
			{
				this.applyTableStyleCOBRouting();
				this.applyTableStylePDO1();
				if(numNodesPDO>4)
				{
					this.applyTableStylePDO2();
				}
			}
			if(this.viewOnly == true)
			{
				this.dataGrid1.ReadOnly = true;
			}

		}

		#endregion minor methods

		#region finalisation/exit
		private void closeBtn_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		private void PDO_MAPPING_TEXT_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			statusBar1.Text = "Performing finalisation, please wait";
			#region disable all timers
			this.timer1.Enabled = false;  //diable all timers before leaving
			#endregion disable all timers
			#region thread aborts
			if(PDOdataRetrievalThread != null)
			{
				if((PDOdataRetrievalThread.ThreadState & System.Threading.ThreadState.Stopped) == 0 )
				{
					PDOdataRetrievalThread.Abort();

					if(PDOdataRetrievalThread.IsAlive == true)
					{
						#if DEBUG
						Message.Show("Failed to close Thread: " + PDOdataRetrievalThread.Name + " on exit");
						#endif
						PDOdataRetrievalThread = null;
					}
				}
			}
			#endregion thread aborts
			e.Cancel = false; //force this window to close
			SCCorpStyle.activeRow = 10000;  //deliberate out of range value
		}
		private void PDO_MAPPING_TEXT_Closed(object sender, System.EventArgs e)
		{
			#region reset window title and status bar
			this.statusBar1.Text = "";
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

		private void dataGrid2_Enter(object sender, System.EventArgs e)
		{
			dataGrid2.CurrentCell = new DataGridCell(this.dataGrid2.CurrentRowIndex, PDOcolumnFromCobRouteTable);  
		}

		private void myControl_VisibleChanged(object sender, EventArgs e)
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
						handleResizeDataGrid(ref myDG, myVscroll.Width);
					}
					else
					{
						handleResizeDataGrid(ref myDG, 0);
					}
				}
			}
		}
		private void handleResizeDataGrid(ref DataGrid myDG, int VScrollBarWidth)
		{
			if(myDG.TableStyles.Count>0)
			{
				int [] ColWidths = null;
				if(myDG == this.dataGrid1)  
				{
					#region System status connected devices table
					ColWidths  = new int[COBRoutingPercents.Length];
					ColWidths  = SCCorpStyle.calculateColumnWidths(ref this.dataGrid1, COBRoutingPercents, VScrollBarWidth, this.dataGrid1.Height);
					#endregion System status connected devices table
				}
				else if(myDG == this.dataGrid2)
				{
					#region System status emergency messages table
					ColWidths  = new int[this.PDOMap1Percents.Length];
					ColWidths  = SCCorpStyle.calculateColumnWidths(ref this.dataGrid2, PDOMap1Percents, VScrollBarWidth, this.dataGrid2.Height);
					#endregion System status emergency messages table
				}
				else if(myDG == this.dataGrid3)
				{
					#region Device Status emergency message sgenerated by this node table
					ColWidths  = new int[this.PDOMap2Percents.Length];
					ColWidths  = SCCorpStyle.calculateColumnWidths(ref this.dataGrid3, PDOMap2Percents, VScrollBarWidth, this.dataGrid3.Height);
					#endregion Device Status emergency message sgenerated by this node table
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

	#region COBID routing table class
	public class COBIDTable: DataTable
	{
		public COBIDTable()
		{
			this.Columns.Add(COBIDcols.COBID.ToString(),typeof(System.UInt32));
			this.Columns[COBIDcols.COBID.ToString()].DefaultValue = 0;  

			this.Columns.Add(COBIDcols.TxNodeID.ToString(),typeof(System.UInt16));
			this.Columns[COBIDcols.TxNodeID.ToString()].DefaultValue = 0;  //zero means node not used

			this.PrimaryKey = new DataColumn[] {this.Columns[COBIDcols.COBID.ToString()],this.Columns[COBIDcols.TxNodeID.ToString()]};
			//note string used to allow full erro checking with enhnaced user feed back 
			// - if Uint use dthe /net just removes invalid entries
			//eg alpha chars with no feedback given
			this.Columns.Add(COBIDcols.Type.ToString(),typeof(System.String));
			this.Columns[COBIDcols.Type.ToString()].DefaultValue = "0";

			this.Columns.Add(COBIDcols.InhibitTime.ToString(), typeof(System.String));
			this.Columns[COBIDcols.InhibitTime.ToString()].DefaultValue = "0";

			this.Columns.Add(COBIDcols.EventTime.ToString(), typeof(System.String));
			this.Columns[COBIDcols.EventTime.ToString()].DefaultValue = "0";

			this.Columns.Add(COBIDcols.RxNode1.ToString(),typeof(System.UInt16));
			this.Columns[COBIDcols.RxNode1.ToString()].DefaultValue = 0;  //zero means node not used

			this.Columns.Add(COBIDcols.RxNode2.ToString(),typeof(System.UInt16));
			this.Columns[COBIDcols.RxNode2.ToString()].DefaultValue = 0;  //zero means node not used

			this.Columns.Add(COBIDcols.RxNode3.ToString(),typeof(System.UInt16));
			this.Columns[COBIDcols.RxNode3.ToString()].DefaultValue = 0;  //zero means node not used

			this.Columns.Add(COBIDcols.RxNode4.ToString(),typeof(System.UInt16));
			this.Columns[COBIDcols.RxNode4.ToString()].DefaultValue = 0;  //zero means node not used

			this.Columns.Add(COBIDcols.RxNode5.ToString(),typeof(System.UInt16));
			this.Columns[COBIDcols.RxNode5.ToString()].DefaultValue = 0;  //zero means node not used

			this.Columns.Add(COBIDcols.RxNode6.ToString(),typeof(System.UInt16));
			this.Columns[COBIDcols.RxNode6.ToString()].DefaultValue = 0;  //zero means node not used

			this.Columns.Add(COBIDcols.RxNode7.ToString(),typeof(System.UInt16));
			this.Columns[COBIDcols.RxNode7.ToString()].DefaultValue = 0;  //zero means node not used

			this.Columns.Add(COBIDcols.OrigCOBID.ToString(),typeof(System.UInt32));
			this.Columns[COBIDcols.OrigCOBID.ToString()].DefaultValue = 0;  

			this.Columns.Add(COBIDcols.OrigTXNode.ToString(),typeof(System.UInt16));
			this.Columns[COBIDcols.OrigTXNode.ToString()].DefaultValue = 0;  //zero means node not used

			this.Columns.Add(COBIDcols.Toggle.ToString(),typeof(System.Boolean));
			this.Columns[COBIDcols.Toggle.ToString()].DefaultValue = false;

			this.Columns.Add(COBIDcols.CAN_11_OR_29.ToString(),typeof(System.Boolean));
			this.Columns[COBIDcols.CAN_11_OR_29.ToString()].DefaultValue = false;

			//The extended bits are defined as a string to allow hexadecimal format display
			this.Columns.Add(COBIDcols.ExtendedIDbits.ToString(),typeof(System.String));
			this.Columns[COBIDcols.ExtendedIDbits.ToString()].DefaultValue = "0x00000";

			this.Columns.Add(COBIDcols.COB_Valid.ToString(),typeof(System.Boolean));
			this.Columns[COBIDcols.COB_Valid.ToString()].DefaultValue = false;

		}
	}
	#endregion COBID routing table class

	#region PDO Mapping Table1
	public class PDOMappingTable1 : DataTable
	{
		public PDOMappingTable1()
		{
			this.Columns.Add(PDOMappingCols1.TxNode.ToString(), typeof(System.UInt32));
			this.Columns[PDOMappingCols1.TxNode.ToString()].DefaultValue = 0x0;
			this.Columns.Add(PDOMappingCols1.RxNode1.ToString(),typeof(System.UInt32));
			this.Columns[PDOMappingCols1.RxNode1.ToString()].DefaultValue = 0x0;
			this.Columns.Add(PDOMappingCols1.RxNode2.ToString(),typeof(System.UInt32));
			this.Columns[PDOMappingCols1.RxNode2.ToString()].DefaultValue = 0x0;
			this.Columns.Add(PDOMappingCols1.RxNode3.ToString(),typeof(System.UInt32));
			this.Columns[PDOMappingCols1.RxNode3.ToString()].DefaultValue = 0x0;
			this.Columns.Add(PDOMappingCols1.COBID.ToString(),  typeof(System.UInt32));
			this.Columns[PDOMappingCols1.COBID.ToString()].DefaultValue = 0;
			this.Columns.Add(PDOMappingCols1.TxNodeID.ToString(),typeof(System.UInt16));
			this.Columns[PDOMappingCols1.TxNodeID.ToString()].DefaultValue = 0;  //zero means node not used
		}
	}
	#endregion PDO Mapping Table1

	#region PDO Mapping Table2
	public class PDOMappingTable2 : DataTable
	{
		public PDOMappingTable2()
		{
			this.Columns.Add(PDOMappingCols2.RxNode4.ToString(),typeof(System.UInt32));
			this.Columns[PDOMappingCols2.RxNode4.ToString()].DefaultValue = 0x0;
			this.Columns.Add(PDOMappingCols2.RxNode5.ToString(),typeof(System.UInt32));
			this.Columns[PDOMappingCols2.RxNode5.ToString()].DefaultValue = 0x0;
			this.Columns.Add(PDOMappingCols2.RxNode6.ToString(),typeof(System.UInt32));
			this.Columns[PDOMappingCols2.RxNode6.ToString()].DefaultValue = 0x0;
			this.Columns.Add(PDOMappingCols2.RxNode7.ToString(),typeof(System.UInt32));
			this.Columns[PDOMappingCols2.RxNode7.ToString()].DefaultValue = 0x0;
			this.Columns.Add(PDOMappingCols2.COBID.ToString(),  typeof(System.UInt32));
			this.Columns[PDOMappingCols2.COBID.ToString()].DefaultValue = 0;
			this.Columns.Add(PDOMappingCols2.TxNodeID.ToString(),  typeof(System.UInt16));
			this.Columns[PDOMappingCols2.TxNodeID.ToString()].DefaultValue = 0;
		}
	}
	#endregion PDO Mapping Table1

	#region COBIDsListTable
	public class COBIDsListTable:DataTable
	{
		public COBIDsListTable()
		{
			this.Columns.Add(COBIDListCols.COBIDName.ToString(),typeof(System.String));
			this.Columns.Add(COBIDListCols.COBIDNum.ToString(),typeof(System.UInt16));
		}
	}
	#endregion COBIDsListTable

	#region NodeNumsListTable
	public class NodeNumsListTable: DataTable
	{
		public NodeNumsListTable()
		{
			this.Columns.Add(NodeIDCols.NodeName.ToString(),typeof(System.String));
			this.Columns.Add(NodeIDCols.NodeNum.ToString(),typeof(System.UInt16));
		}
	}
	#endregion NodeNumsListTable

	#region ALL PDO mappable Items
	public class AllPDOItemsTable : DataTable
	{
		public AllPDOItemsTable()
		{
			this.Columns.Add(AllPDOsCols.ParamNamesList.ToString(), typeof(System.String));
			this.Columns.Add(AllPDOsCols.IDNum.ToString(),typeof(System.UInt32));
			this.Columns.Add(AllPDOsCols.dataLength.ToString(), typeof(System.UInt16));
		}
	}
	#endregion ALL PDO mappable Items

	#region COBID TableStyle class
	public class COBID_TS : SCbaseTableStyle
	{
		public COBID_TS (int [] ColWidths, COBIDsListTable COBIDsTable, NodeNumsListTable NodeNumsTable, bool viewOnly, ref COBIDTable COBIDRoutingTable)
		{
			this.ReadOnly = false;  //read only tablestyle only - individula columns can override this - not related to SEVCON access, num nodes etc

			#region add User interaction Combo Styl ecolumns
				COBRoutingComboColumn COBIDCol = new  COBRoutingComboColumn(ref COBIDsTable, COBIDListCols.COBIDName.ToString(), COBIDListCols.COBIDNum.ToString(), ref COBIDRoutingTable, viewOnly);
				COBIDCol.MappingName = COBIDcols.COBID.ToString();
				COBIDCol.HeaderText = "COB ID";
				COBIDCol.Width = ColWidths[(int)COBIDcols.COBID];
				GridColumnStyles.Add(COBIDCol);

				COBRoutingComboColumn TxNodeCol = new  COBRoutingComboColumn(NodeNumsTable, NodeIDCols.NodeName.ToString(), NodeIDCols.NodeNum.ToString(), (int) COBIDcols.TxNodeID, viewOnly);
				TxNodeCol.MappingName = COBIDcols.TxNodeID.ToString();
				TxNodeCol.HeaderText = "Tx Node";
				TxNodeCol.Width = ColWidths[(int)COBIDcols.TxNodeID];
				GridColumnStyles.Add(TxNodeCol);
				#endregion add User interaction Combo Styl ecolumns

			#region tpye, inhibit an devent time columns
				
			#region add type column
			if(viewOnly == false)
			{
				DPODataGridTextBoxColumn TypeCol = new DPODataGridTextBoxColumn(false, (int) COBIDcols.Type);
				TypeCol.MappingName = COBIDcols.Type.ToString();
				TypeCol.HeaderText = "Type";
				TypeCol.Width = ColWidths[(int)COBIDcols.Type];
				GridColumnStyles.Add(TypeCol);	
			}
			else
			{
				DPODataGridTextBoxColumn TypeCol = new DPODataGridTextBoxColumn(true, (int) COBIDcols.Type);
				TypeCol.MappingName = COBIDcols.Type.ToString();
				TypeCol.HeaderText = "Type";
				TypeCol.Width = ColWidths[(int)COBIDcols.Type];
				GridColumnStyles.Add(TypeCol);	
			}
			#endregion add type column

			#region add inhibit time column
			if(viewOnly == false)
			{
				DPODataGridTextBoxColumn InhibitCol = new DPODataGridTextBoxColumn(false, (int) COBIDcols.InhibitTime);
				InhibitCol.MappingName = COBIDcols.InhibitTime.ToString();
				InhibitCol.HeaderText = "Inhibit/ms";
				InhibitCol.Width = ColWidths[(int)COBIDcols.InhibitTime];
				GridColumnStyles.Add(InhibitCol);	
			}
			else
			{
				DPODataGridTextBoxColumn InhibitCol = new DPODataGridTextBoxColumn(true, (int) COBIDcols.InhibitTime);
				InhibitCol.MappingName = COBIDcols.InhibitTime.ToString();
				InhibitCol.HeaderText = "Inhibit/ms";
				InhibitCol.Width = ColWidths[(int)COBIDcols.InhibitTime];
				GridColumnStyles.Add(InhibitCol);	
			}
			#endregion add inhibit time column

			#region add event time column
			if(viewOnly == false)
			{
				DPODataGridTextBoxColumn EventTimeCol = new DPODataGridTextBoxColumn(false, (int) COBIDcols.EventTime);
				EventTimeCol.MappingName = COBIDcols.EventTime.ToString();
				EventTimeCol.HeaderText = "Event/ms";
				EventTimeCol.Width = ColWidths[(int)COBIDcols.EventTime];
				GridColumnStyles.Add(EventTimeCol);	
			}
			else
			{
				DPODataGridTextBoxColumn EventTimeCol = new DPODataGridTextBoxColumn(true, (int) COBIDcols.EventTime);
				EventTimeCol.MappingName = COBIDcols.EventTime.ToString();
				EventTimeCol.HeaderText = "Event/ms";
				EventTimeCol.Width = ColWidths[(int)COBIDcols.EventTime];
				GridColumnStyles.Add(EventTimeCol);	
			}
			#endregion add event time column

			#endregion tpye, inhibit an devent time columns

			#region RxNode columns
			string [] RxNames = {"1st Rx Node", "2nd Rx Node", "3rd Rx Node", "4th Rx Node" , "5th Rx Node", "6th Rx Node", "7th Rx Node"};
			for(int i = 0;i<(ColWidths.Length-5);i++)
			{
				COBRoutingComboColumn Node1Col = new  COBRoutingComboColumn(NodeNumsTable, NodeIDCols.NodeName.ToString(), NodeIDCols.NodeNum.ToString(), (i+5), viewOnly);
				Node1Col.MappingName = "RxNode" + (i+1).ToString();
				Node1Col.HeaderText = RxNames[i];
				Node1Col.Width = ColWidths[i+5];
				GridColumnStyles.Add(Node1Col);
			}
			#endregion RxNode columns
			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();

		}
	}
	#endregion COBID TableStyle class

	#region PDO Mapping1 tablestyle
	public class PDO1_TS : SCbaseTableStyle
	{
		public PDO1_TS(int [] ColWidths, bool viewOnly, string [] colHeaderText)
		{
			this.AllowSorting = false;
			for(int i = 0;i<ColWidths.Length;i++)
			{
				PDOMapComboColumn myCol;
				if(DriveWizard.PDO_MAPPING_TEXT.numNodesPDO == 1)  //single connected node - hence mastertable is Length 1
				{
					myCol = new  PDOMapComboColumn(i, viewOnly);
				}
				else
				{
					myCol = new  PDOMapComboColumn(i, viewOnly);
				}

				if(i == 0)
				{
					myCol.MappingName = "TxNode";
				}
				else
				{
					myCol.MappingName = "RxNode" + i.ToString();
				}
				myCol.HeaderText = colHeaderText[i];
				myCol.Width = ColWidths[i];
				myCol.ReadOnly = true;
				GridColumnStyles.Add(myCol);
			}
			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();

		}
	}
	#endregion PDO Mapping1 tablestyle

	#region PDO Mapping2 tablestyle
	public class PDO2_TS : SCbaseTableStyle
	{
		public PDO2_TS(int [] ColWidths, bool viewOnly, string [] colHeaderText)
		{
			this.AllowSorting = false;
			for(int i = 0;i<ColWidths.Length;i++)
			{
				PDOMapComboColumn myCol = new  PDOMapComboColumn((int) PDOMappingCols2.RxNode4, viewOnly);
				myCol.MappingName = "RxNode" + (i+4).ToString();
				myCol.HeaderText = colHeaderText[i];
				myCol.Width = ColWidths[i];
				myCol.ReadOnly = true;
				GridColumnStyles.Add(myCol);
			}
			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();

		}
	}
	#endregion PDO2 Mapping tablestyle

	#region COB routing ComboColumn CLass
	public class COBRoutingComboColumn : System.Windows.Forms.DataGridTextBoxColumn 
	{
		private ComboBox _cboColumn;
		private object _objSource = null;
		private string _strMember;
		private string _strValue;
		private int _columnOrdinal;
		private bool _viewonly;
		#region parameters relating to COB routing table onlt
		private COBIDsListTable _COBIDsTable = null;
		private COBIDTable _COBIDRoutingTable = null;  
		public static bool nonUniqueCobID = false;  //used for reporting back to Form class so column error can be set/cleared
		private ErrorProvider comboErrorProvider;
		#endregion parameters relating to COB routing table onlt
		
		private bool _bIsComboBound = false; // remember if this combobox is bound to its datagrid
		private int _iRowNum;  //the row number
		private CurrencyManager _cmSource;

		/// <summary>
		/// initialize the combobox column and take note of the data source/member/value used to fill the combobox
		/// </summary>
		/// <param name="objSource">bind Source for the combobox (typical is a DataTable object)</param>
		/// <param name="strMember">bind for the combobox DisplayMember (typical is a Column Name within the Source)</param>
		/// <param name="strValue">bind for the combobox ValueMember (typical is a Column Name within the Source)</param>
		
		#region constructor for the COB ID column in the COB Routing table grid
		public COBRoutingComboColumn(ref COBIDsListTable COBIDsTable, string strMember, string strValue, ref COBIDTable COBIDRoutingTable, bool viewOnly)
		{
			_objSource = COBIDsTable;
			_strMember = strMember;
			_strValue = strValue;
			_COBIDsTable = COBIDsTable;
			_COBIDRoutingTable = COBIDRoutingTable;
			_viewonly = viewOnly;
			// create a new combobox object
			_cboColumn = new ComboBox();
			comboErrorProvider = new ErrorProvider();
			#region link combo to its data source
			_cboColumn.DataSource = _COBIDsTable;
			_cboColumn.DisplayMember = _strMember;
			_cboColumn.ValueMember = _strValue;
			#endregion link combo to its data source
			_cboColumn.DropDownStyle = ComboBoxStyle.DropDown;//allow both drop down slection an duser text input
			// Setting ReadOnly changes the behavior of the column so the 'leave' event fires whenever we 
			// change cell. The default behavior will not fire the 'leave' event when we up-arrow or 
			// down-arrow to the next row.
			this.ReadOnly = true;
			_cboColumn.Leave += new EventHandler(cboColumn_Leave); 
			_cboColumn.SelectionChangeCommitted += new EventHandler(cboColumn_ChangeCommit); 
			_cboColumn.KeyPress +=new KeyPressEventHandler(_cboColumn_KeyPress);
			// make sure the combobox is invisible until we've set its correct position and dimensions
			_cboColumn.Visible = false;
		}
		#endregion constructor for the COB ID column in the COB Routing table grid

		#region contructtor for all other coumns in COB Routing grid
		public COBRoutingComboColumn(object objSource, string strMember, string strValue, int columnOrdinal , bool viewOnly)
		{
			_objSource = objSource;
			_strMember = strMember;
			_strValue = strValue;
			_columnOrdinal = columnOrdinal;
			_viewonly = viewOnly;
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
		#endregion generic constructor

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			if((this._viewonly == true)  || (DriveWizard.PDO_MAPPING_TEXT.DuplicateCOBFlag == true)) //non adjustable
			{
				return;
			}
			SCCorpStyle.activeRow = rowNum;
			DataRowView myView = (DataRowView) source.Current;
			if((System.Convert.ToInt32(myView.Row[0].ToString()) == 0) && (this._columnOrdinal>0))
			{
				PDO_MAPPING_TEXT.PDOComboErrorString = "You must enter COBID first";
				return;
			}
			#region return if Rx nodes to lef tnot yet filled in
			if((this._columnOrdinal > (int) COBIDcols.RxNode1) && (this._columnOrdinal<=(int) COBIDcols.RxNode7))
			{
				for(int i = (int) COBIDcols.RxNode1;i<this._columnOrdinal;i++)
					if(System.Convert.ToUInt16(myView.Row[i]) == 0)
					{
						PDO_MAPPING_TEXT.PDOComboErrorString = "Fill Rx nodes from the left";
						return;  //prevent creation of combo since since user must fill Rx nodes from the left
					}
			}
			#endregion return if Rx nodes to lef tnot yet filled in
			PDO_MAPPING_TEXT.PDOComboErrorString = "";  //reset the string - no reason to prevent cell edit
			// get current cell value and use this dig out the corresponding string prior to making combo Visible
			object tempObject = this.GetColumnValueAtRow(source, rowNum);
			// the navigation path to the datagrid only exists after the column is added to the Styles
			if (_bIsComboBound == false) 
			{
				_bIsComboBound = true; //set the indicator 
				this.DataGridTableStyle.DataGrid.Controls.Add(_cboColumn); // and bind combo to its datagrid 
			}
			// this data is used when the combo box loses focus
			_iRowNum = rowNum;
			_cmSource = source;
			// synchronize the font size to the text box
			_cboColumn.Font = this.TextBox.Font;
			// set the combobox to the dimensions of the cell (do this each time because the user may have resized this column)
			_cboColumn.Bounds = bounds;
			//set the combobox back ground to sleection colour
			_cboColumn.BackColor = SCCorpStyle.dgRowSelected;
			// do not paint the control until we've set the correct position in the items list
			_cboColumn.BeginUpdate();
			// note: on the very first time this routine is called you MUST set the column as visible 
			// ahead of setting a position in the items collection. otherwise the combobox will not be
			// populated and the call to set the SelectedValue cannot succeed
			_cboColumn.Visible = true;
			// use the object to set the combobox. the null detection is primarily aimed at the addition of a
			// new row (where it is possible a default column-row content has not been defined)
			if (tempObject.GetType() != typeof(System.DBNull)) 
			{
				_cboColumn.SelectedValue = tempObject;
			} 
			else 
			{
				_cboColumn.SelectedIndex = 0;
			}
			// we've set the combobox so we can now paint the control and move focus onto it
			_cboColumn.EndUpdate();
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
			#region COB Routing table
			if(_COBIDRoutingTable != null)  //we are tralking COB ID column in COB ID Routing table here
			{
				comboErrorProvider.SetError(_cboColumn, "");
				uint UserRequestedCOBID = System.Convert.ToUInt32(_cboColumn.SelectedValue);
				checkForNonUniqueCobID(UserRequestedCOBID);
				if(nonUniqueCobID == true)
				{
					uint newValue = 0;
					bool COBFound = locateNextUniqueCOBID(out newValue);
					if(COBFound == true)
					{
						objValue = newValue;
						this.SetColumnValueAtRow(_cmSource, _iRowNum, objValue);
					}
					else
					{
						Message.Show("No free COB could be found");
					}
				}
				else
				{
					this.SetColumnValueAtRow(_cmSource, _iRowNum, objValue);
				}
			}
				#endregion COB Routing table
			#region any other table
			else  //The changed Combo is NOT the COB ID column in the COB Routing table
			{
				this.SetColumnValueAtRow(_cmSource, _iRowNum, objValue);
			}
			#endregion any other table
			_cboColumn.Visible = false;
		}
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			string defaultStr = "";
			DataRow[] aRowA;
			// retrieve the value at the current column-row within the source for this column
			object tempObject = this.GetColumnValueAtRow(source, rowNum);
			uint rawCOBId = 0;
			try 
			{
				rawCOBId = System.Convert.ToUInt32(tempObject);
			}
			catch
			{
				defaultStr = "Invalid value" + tempObject.ToString();
			}
			//now see if value is in table
			if(defaultStr == "")
			{
				try
				{
					aRowA = ((DataTable)_objSource).Select(_strValue + " = " + tempObject.ToString());
					defaultStr = aRowA[0][_strMember].ToString();  
				}
				catch
				{
					if(tempObject.ToString() == "0")  //default on new row - waiting of ruser input
					{
						defaultStr = "Select COB ID";
					}
					else if((rawCOBId >=0x181) && (rawCOBId <=0x57F))
					{
						defaultStr = "0x" + rawCOBId.ToString("X");
					}
					else
					{
						defaultStr = "Invalid value: 0x" + rawCOBId.ToString("X");
					}
				}
			}
			// Now paint the cell. 
			Rectangle rect = bounds;
			if(rowNum == SCCorpStyle.activeRow)
			{
				backBrush = new SolidBrush(SCCorpStyle.dgRowSelected);
			}
			g.FillRectangle(backBrush, rect); 
			// vertical offset to account for frame of combobox
			rect.Y += 2;
			g.DrawString(defaultStr, this.TextBox.Font, foreBrush, rect); 
		}
		private void _cboColumn_KeyPress(object sender, KeyPressEventArgs e)
			//Check for and display errors once the user hits enter or return
			//this allows the user to correct errors prior to leaving the combobox
			//it also forces a non-unique COB to be use d(if one is available) 
		{
			if((((Keys)e.KeyChar) == Keys.Enter) || (((Keys)e.KeyChar) == Keys.Return))
			{
				string errorString = "";
				uint UserRequestedCOBID = 0;
				comboErrorProvider.SetError(_cboColumn, "");
				string input = _cboColumn.SelectedText.ToUpper();
				if (input.Length<3)
				{
					errorString = "hexadecimal format (0x###) required";
				}
				else
				{
					string inputcopy = input;
					inputcopy = inputcopy.Substring(0,2);
					if(inputcopy == "0X")//we have hex input
					{
						#region test for valid chars and range
						input = input.Remove(0,2); //remove the 0X

						char [] invalidChars = "GHIJKLMNOPQRSTUVWXYZ!\"£$%^&*()_-+=\\|,<>?/:;'@#~{[}]".ToCharArray(); //" and \ need extra \to be accepted
						int invalidCharIndex = input.IndexOfAny(invalidChars);
						if(invalidCharIndex != -1)
						{
							errorString = "Hexadecimal characters only";
						}
						else if(input.Length>4)
						{
							errorString = "Out of range, maximum value is 0x57F";
						}
						else
						{
							UserRequestedCOBID = Convert.ToUInt32(input, 16); //convert to base 10 number
							if( UserRequestedCOBID> 0x57F)
							{
								errorString = "Out of range, maximum value for a PDO is 0x0x57F";
							}
							if(UserRequestedCOBID <0x181)
							{
								errorString = "Out of range, minimum value for a PDO is 0x181";
							}
						}
						#endregion test for valid chars and range
					}
					else  //did not start with 0x
					{
						errorString = "hexadecimal format (0x###) required";
					}
				}
				if(errorString == "")
				{
					#region check for and handle non-unique COB ID input
					checkForNonUniqueCobID(UserRequestedCOBID);
					if(nonUniqueCobID == true)
					{
						uint newValue = 0;
						bool COBFound = locateNextUniqueCOBID(out newValue);
						if(COBFound == true)
						{
							UserRequestedCOBID = newValue;
							this.SetColumnValueAtRow(_cmSource, _iRowNum, UserRequestedCOBID);
						}
						else
						{
							Message.Show("No free COB could be found");
						}
					}
					else
					{
						this.SetColumnValueAtRow(_cmSource, _iRowNum, UserRequestedCOBID);
					}
					comboErrorProvider.SetError(_cboColumn, "");
					#endregion check for and handle non-unique COB ID input
					#region add new string to source table if required
					bool matchFound = false;
					for(int i = 0; i< _COBIDsTable.Rows.Count;i++)
					{
						if(UserRequestedCOBID == System.Convert.ToUInt16(_COBIDsTable.Rows[i][(int) COBIDListCols.COBIDNum]))
						{
							matchFound = true;
						}
					}
					if(matchFound == false)
					{
						DataRow row = _COBIDsTable.NewRow();
						string COBIDStr = UserRequestedCOBID.ToString("X");
						while (COBIDStr.Length<4)
						{
							COBIDStr = "0" + COBIDStr;
						}
						row[(int) COBIDListCols.COBIDName] = "0x" + COBIDStr;
						row[(int) COBIDListCols.COBIDNum] = System.Convert.ToUInt16(UserRequestedCOBID);
						_COBIDsTable.Rows.Add(row);
					}
					#endregion add new string to source table if required
					_cboColumn.Visible = false;
				}
				else
				{
					comboErrorProvider.SetError(_cboColumn, errorString);
				}
			}

		}

		private void checkForNonUniqueCobID(uint UserRequestedCOBID)
		{
			#region check if this COB ID has already been used
			int i = 0;
			nonUniqueCobID = false;  
			while((nonUniqueCobID == false) && (i<_COBIDRoutingTable.Rows.Count))
			{
				if(UserRequestedCOBID == System.Convert.ToUInt32(_COBIDRoutingTable.Rows[i][(int) COBIDcols.COBID]) )
				{
					nonUniqueCobID = true;
					break;
				}
				else
				{
					i++;
				}
			}
			#endregion check if this COB ID has already been used
		}
		private bool locateNextUniqueCOBID(out uint objValue)
		{
			#region locate next unused COB ID and replace user request with this one
			//find next non-used COB ID
			objValue = 0;
			int k = 0;
			bool nextUnusedCOBFound = false;
			while((nextUnusedCOBFound == false) && (k<_COBIDsTable.Rows.Count))  //not found an unused COB and not at end of COB ID master list
			{
				//get next value in master COB list
				uint testvalue = System.Convert.ToUInt32(_COBIDsTable.Rows[k][(int) COBIDListCols.COBIDNum]);
				if(testvalue == DriveWizard.PDO_MAPPING_TEXT.COBDeletionMarker)
				{
					testvalue = System.Convert.ToUInt32(_COBIDsTable.Rows[++k][(int) COBIDListCols.COBIDNum]);  //get next value
				}
				//now see if this value has already been used in the COB Routing table
				int j = 0;
				bool COBUsed = false;
				while((COBUsed == false) && (j<_COBIDRoutingTable.Rows.Count))  //not found unused COB and not got to end of the COB routing table
				{
					if(testvalue == System.Convert.ToUInt32(_COBIDRoutingTable.Rows[j][(int) COBIDcols.COBID]) ) //has test value been used?
					{
						COBUsed = true;
						break;  //force the while test
					}
					else
					{
						if(++j>=_COBIDRoutingTable.Rows.Count)  //increment and then check for end of table
						{
							nextUnusedCOBFound = true;  //we are at end of table and no match was found so we can use this value
							objValue = testvalue;
							break;
						}
					}
				}
				k++;
			}
			#endregion locate next unused COB ID and replace user request with this one
			return nextUnusedCOBFound;
		}
	}
	#endregion COB routing ComboColumn CLass

	#region PDO Mapping ComboColumn CLass
	public class PDOMapComboColumn : System.Windows.Forms.DataGridTextBoxColumn 
	{
		private ComboBox _cboColumn;
		private object _objSource = null;
		public object objSource
		{
			set
			{
				_objSource = value;
			}
		}
		public string _strMember = AllPDOsCols.ParamNamesList.ToString();
		public string _strValue = AllPDOsCols.IDNum.ToString();
		private int _columnOrdinal;  //identify column position in tableStyle/Dataview
		private bool _bIsComboBound = false; // remember if this combobox is bound to its datagrid
		private int _iRowNum;  //the row number
		private CurrencyManager _cmSource;
		private bool _viewOnly = false;
		/// <summary>
		/// initialize the combobox column and take note of the data source/member/value used to fill the combobox
		/// </summary>
		/// <param name="objSource">bind Source for the combobox (typical is a DataTable object)</param>
		/// <param name="strMember">bind for the combobox DisplayMember (typical is a Column Name within the Source)</param>
		/// <param name="strValue">bind for the combobox ValueMember (typical is a Column Name within the Source)</param>

		public PDOMapComboColumn(int columnOrdinal, bool viewOnly )
		{
			_viewOnly = viewOnly;
			_columnOrdinal = columnOrdinal;  //we need to know whci node we are talking about
			_cboColumn = new ComboBox();// create a new combobox object for each edit
			// set the data link to the source, member and value displayed by this combobox
			_cboColumn.DropDownStyle = ComboBoxStyle.DropDownList;
			this.ReadOnly = true;  //force leave event handler to fire on use of up/down arrows
			_cboColumn.Leave += new EventHandler(cboColumn_Leave);
			_cboColumn.SelectionChangeCommitted += new EventHandler(cboColumn_ChangeCommit);
			_cboColumn.Visible = false;//ensure invisible until we've set its correct position and dimensions
		}
		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			//if(_objSource == null)
			{
//				return;
			}
			// this data is used when the combo box loses focus
			_cboColumn.DataSource = _objSource;
			_cboColumn.DisplayMember = _strMember;
			_cboColumn.ValueMember = _strValue;
			_iRowNum = rowNum;
			_cmSource = source;
			if((_viewOnly == true) || (DriveWizard.PDO_MAPPING_TEXT.DuplicateCOBFlag == true))
			{
				return;
			}
			DataRowView myRowView = (DataRowView) source.Current;
			DataView myDataView = myRowView.DataView;
			DataRow parRow;
			string myRelName;
			if(myDataView.Table.TableName == "childTable1")
			{
				myRelName = myDataView.Table.DataSet.Relations[0].RelationName;
				parRow = myRowView.Row.GetParentRow(myRelName);
				if(_columnOrdinal == 0)
				{
					if(System.Convert.ToUInt16(parRow[(int) COBIDcols.TxNodeID]) == 0)
					{
						PDO_MAPPING_TEXT.PDOComboErrorString = "First select a Tx Node";
						return;//no Tx node defined in COB routing so prevent addition of mapping data
					}
				}
				else
				{
					if(System.Convert.ToUInt16(parRow[(int) (COBIDcols.RxNode1 + _columnOrdinal - 1)]) == 0) //-1 for Tx node
					{
						PDO_MAPPING_TEXT.PDOComboErrorString = "First select an Rx Node";
						return;//no corresponding Rx node defined in COB routing so prevent addition of mapping data
					}
				}
			}
			else
			{
				PDO_MAPPING_TEXT.PDOComboErrorString = "";
				myRelName = myDataView.Table.DataSet.Relations[1].RelationName;
				parRow = myRowView.Row.GetParentRow(myRelName);
				if(System.Convert.ToUInt16(parRow[(int) (COBIDcols.RxNode4 + _columnOrdinal)]) == 0)
				{
					PDO_MAPPING_TEXT.PDOComboErrorString = "First select an Rx Node";
					return;//no corresponding Rx node defined in COB routing so prevent addition of mapping data
				}

			}
			#region ensure contiguous mapping with no offset
			int j = 0;
			foreach(DataRowView row in myDataView)
			{
				if((j>=rowNum) || (rowNum == 0))
				{
					break;
				}
				else
				{
					if(System.Convert.ToUInt32(row.Row[_columnOrdinal]) ==  0)  //non contiguous
					{
						PDO_MAPPING_TEXT.PDOComboErrorString = "PDO Mappings must be contiguous with no offset";
						return;  
					}
					else
					{
						j++;
					}

				}
			}
			#endregion ensure contigous with no offset
			#region once only code
			// the navigation path to the datagrid only exists after the column is added to the Styles
			if (_bIsComboBound == false) 
			{
				_bIsComboBound = true; //set the indicator 
				this.DataGridTableStyle.DataGrid.Controls.Add(_cboColumn); // and bind combo to its datagrid 
			}
			#endregion once only code
			#region setup, draw and give focus to Combo
			_cboColumn.Font = this.TextBox.Font; // synchronize the font size to the text box
			// get current cell value and use this dig out the corresponding string prior to making combo Visible
			object tempObject = this.GetColumnValueAtRow(source, rowNum);
			// set the combobox to the dimensions of the cell (do this each time because the user may have resized this column)
			_cboColumn.Bounds = bounds;
			// do not paint the control until we've set the correct position in the items list
			_cboColumn.BeginUpdate();
			// note: on the very first time this routine is called you MUST set the column as visible 
			// ahead of setting a position in the items collection. otherwise the combobox will not be
			// populated and the call to set the SelectedValue cannot succeed
			_cboColumn.Visible = true;
			// use the object to set the combobox. the null detection is primarily aimed at the addition of a
			// new row (where it is possible a default column-row content has not been defined)
			_cboColumn.SelectedValue = tempObject;
			// we've set the combobox so we can now paint the control and move focus onto it
			_cboColumn.EndUpdate();
			_cboColumn.Focus();
			#endregion setup, draw and give focus to Combo
		}
		public void cboColumn_Leave(object sender, EventArgs e) 
		{
			_cboColumn.Visible = false;
		}

		public void cboColumn_ChangeCommit(object sender, EventArgs e)
		{
			object objValue = _cboColumn.SelectedValue;
			this.SetColumnValueAtRow(_cmSource, _iRowNum, objValue);
			_cboColumn.Visible = false;
		}
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			string defaultStr = "";
			//if(this._objSource != null)
			{
				DataRow[] aRowA = null;
				// retrieve the value at the current column-row within the source for this column
				object tempObject = this.GetColumnValueAtRow(source, rowNum);
				Type aType = tempObject.GetType();
				try
				{
					aRowA = ((DataTable)this._objSource).Select(_strValue + " = " + tempObject.ToString());
					defaultStr = aRowA[0][_strMember].ToString();  
				}
				catch
				{
					if(tempObject.ToString() == "0")
					{
						defaultStr = "not used";
					}
					else
					{
						defaultStr = "Invalid Object. 0x" + System.Convert.ToUInt64(tempObject).ToString("X");
					}
				}
			}
			// Now paint the cell. 
			Rectangle rect = bounds;
			g.FillRectangle(backBrush, rect); 
			// vertical offset to account for frame of combobox
			rect.Y += 2;
			g.DrawString(defaultStr, this.TextBox.Font, foreBrush, rect); 
		}
	}
	#endregion PDO Mapping ComboColumn CLass

	#region Text Box column Class for PDO tableStyles
	public class DPODataGridTextBoxColumn : DataGridTextBoxColumn
	{
		bool _viewonly;
		private int _columnOrdinal;
		public DPODataGridTextBoxColumn(bool viewonly, int columnOrdinal)
		{
			_viewonly = viewonly;
			_columnOrdinal = columnOrdinal;
		}
		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			if((_viewonly == true) || (DriveWizard.PDO_MAPPING_TEXT.DuplicateCOBFlag == true))
			{
				return;
			}
			SCCorpStyle.activeRow = rowNum;
			DataRowView myView = (DataRowView) source.Current;
			int tesp = System.Convert.ToInt32(myView.Row[0].ToString());
			if((tesp == 0) && (this._columnOrdinal>0))
			{
				PDO_MAPPING_TEXT.PDOComboErrorString = "You must enter COBID first";
				return;
			}
			PDO_MAPPING_TEXT.PDOComboErrorString = ""; //reset the string - no reason to prevent cell edit
			base.Edit(source, rowNum,bounds, readOnly, instantText,cellIsVisible);
		}
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			if(rowNum == SCCorpStyle.activeRow)
			{
				backBrush = new SolidBrush(SCCorpStyle.dgRowSelected);
			}
		base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
		}
	}
	#endregion Text BOx column for PDO tableStyles
}
	


