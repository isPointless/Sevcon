/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.72$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/09/2008 23:16:48$
	$ModDate:23/09/2008 22:00:46$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	This window displays the counters log retrieved from a Sevcon node. 
    For each event retrieved, the event ID, description, first timestamp, 
    last timestamp and number of counts is displayed. It is possible to reset 
    individual or all counters and reconfigure the event counters

REFERENCES    

MODIFICATION HISTORY
    $Log:  36715: COUNTERS_LOG_WINDOW.cs 

   Rev 1.72    23/09/2008 23:16:48  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.71    17/03/2008 13:13:52  jw
 Logs null obejct handling complete. Still needs progrees bar steps linking to
 odSub timeout values


   Rev 1.70    14/03/2008 10:58:50  jw
 Log handling revised, Read of Log releated CAN items now done from GUI ( to
 allow later DI /Ixxat unlinking) . Processing of recevied logs code and event
 lists moved to sysInfo - reduces memory usage ( one method per application
 rather than per node)  Some newer Sevocn devices do not hav ethese logs. Also
 event list methods now have product code input - to allow us to have seperate
 Event lists for eg displays etc. Note:some error hanlding eg null detection
 still needed but check back in for working set with DI 


   Rev 1.69    12/03/2008 13:43:48  ak
 All DI threads have ThreadPriority increased to Normal from BelowNormal
 (needed when running VCI3)


   Rev 1.68    07/01/2008 22:58:26  ak
 Threading exceptions fixed.


   Rev 1.67    05/12/2007 22:13:00  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Data;
using System.Diagnostics;

namespace DriveWizard
{
	/// <summary>
	/// Summary description for COUNTERS_LOG_WINDOW.
	/// </summary>
	/// 
	public class COUNTERS_LOG_WINDOW : System.Windows.Forms.Form
	{
		#region controls declarations
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button resetBtn;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button updateCountersBtn;
		private System.Windows.Forms.ComboBox cboDatum;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem openMI;
		private System.Windows.Forms.MenuItem saveMI;
		private System.Windows.Forms.MenuItem printMI;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.ComponentModel.IContainer components;
        ToolBar toolbar = null;
        private ProgressBar progressBar1;

		#endregion controls declarations

		#region my definitions
		//datatable deifitions
		private DataRow row;
		public CountersTable counterTbl;
		public DataView counterView;
		//masterListTbl used as source data for the drop down list of evnets available on this node
		public MasterTable masterListTbl = null;  
		private SystemInfo sysInfo = null;
        private nodeInfo node = null;
		private string selectedNodeText = "";
		EventLogEntry [] log = new EventLogEntry[0];
		Thread DIThread = null;
		TSCounters tableStyle = null;
		bool viewOnly = true;
		string [] defaultEventNames;
		private System.Windows.Forms.StatusBar statusBar1;
		int dataGrid1DefaultHeight = 0;
        ushort requestedID = 0;
        ushort slotNumber = 0;
        SortedList nodeEventList = null;
        ArrayList uknownFaultIDs = new ArrayList();     //DR38000269 single list of unknown fault ids
        int eventIDRetrieval = 0;

        ArrayList eventCntrIDSubs = null;
        ODItemData resetEventLogSub = null;
        ODItemData eventLogSub  = null;
        ODItemData eventIDsSub = null;
        ODItemData eventNameSub = null;
        ODItemData allEventsForNodeSub = null;

        ///<summary>delagate for DI wrapper methods for single start thread method</summary>
        private delegate void wrapperDelegate();

		#endregion

		#region initialisation
		public COUNTERS_LOG_WINDOW(SystemInfo passed_systemInfo, int nodeNum, string nodeText, ToolBar passed_Toolbar)
		{
			InitializeComponent();
			sysInfo = passed_systemInfo;
            this.node = sysInfo.nodes[nodeNum];
			selectedNodeText = nodeText;
			this.toolbar = passed_Toolbar;

            MAIN_WINDOW.appendErrorInfo = false;
            resetEventLogSub = node.getODSubFromObjectType(SevconObjectType.OTHER_EVENTLOG_CTRL, 0x01);
            allEventsForNodeSub = node.getODSubFromObjectType(SevconObjectType.DOMAIN_UPLOAD, 0x0A);
            eventLogSub  = node.getODSubFromObjectType(SevconObjectType.DOMAIN_UPLOAD, SCCorpStyle.EventCountersSubObject);
            eventIDsSub = node.getODSubFromObjectType(SevconObjectType.EVENT_ID_DESCRIPTION, SCCorpStyle.EventIDSubObject);
            eventNameSub = node.getODSubFromObjectType(SevconObjectType.EVENT_ID_DESCRIPTION, SCCorpStyle.EventNameSubObject);
            #region identify how many event counters we have and stor their Dvent ID subs
            // get each OD item that has Object Type EVENT_COUNTER
            ArrayList eventCntrODitems = node.getODItemAndSubsFromObjectType(SevconObjectType.EVENT_COUNTER);
            eventCntrIDSubs = new ArrayList();
            foreach (ObjDictItem odItem in eventCntrODitems)
            {
                int index = ((ODItemData)odItem.odItemSubs[0]).indexNumber;
                ODItemData counterodSub = node.getODSub(index, 0x01);
                if (counterodSub != null)
                {
                    eventCntrIDSubs.Add(counterodSub);
                }
            }
            if (eventCntrIDSubs.Count == 0)
            {
                this.viewOnly = true;
            }
            #endregion identify how many event counters we have and stor their Dvent ID subs
            MAIN_WINDOW.appendErrorInfo = true;
		}

        private void COUNTERS_LOG_WINDOW_Load(object sender, System.EventArgs e)
		{
			this.sysInfo.formatDataGrid( dataGrid1);
			#region button colouring
			foreach (Control myControl in this.Controls)
			{
				if((myControl.GetType().ToString()) == "System.Windows.Forms.Button")
				{
					myControl.BackColor =  SCCorpStyle.buttonBackGround;
				}
			}
			#endregion button colouring
			dataGrid1DefaultHeight = this.dataGrid1.Height;
            this.statusBar1.Text = "Retrieving Event Counters Log";
            this.nodeEventList = new SortedList();
            if (eventLogSub != null)
            {
                if (allEventsForNodeSub != null)
                {
                    this.startDIThread("getAllEventIDsWrapper", getAllEventIDsWrapper);
                }
                else
                {
                    this.viewOnly = true;
                    createMasterList();
                    this.startDIThread("requestEventLogWrapper", requestEventLogWrapper);
                }
            }
            else
            {
                this.viewOnly = true;
                //we cannot get event log so just show user controls.
                this.showUserControls();
            }
		}
        
		private void createMasterList()
		{
			//create counterTbl 2 first as this is used in filling counterTbl 1
			#region counterTbl for combo
			if(viewOnly == false)
			{
				masterListTbl = new  MasterTable();
				MasterTable temptable = new MasterTable();
                ArrayList EventIDs = new ArrayList(nodeEventList.Keys);
                ArrayList EventNames = new ArrayList(nodeEventList.Values);
                for (int i = 0; i < nodeEventList.Count; i++)
				{
					row = temptable.NewRow();
					ushort EventNo = System.Convert.ToUInt16(EventIDs[i]);
					row[(int) (MasterCols.eventIDList)] = EventNo;
					string eventStr = EventNo.ToString("X");
					while(eventStr.Length<4)
					{
						eventStr = "0" + eventStr;
					}
					eventStr =  " (ID 0x" + eventStr + ")";
					row[(int) (MasterCols.NamesList)] = EventNames[i].ToString() + eventStr;  //add in the Event ID as hex string
					temptable.Rows.Add(row);
				}
				temptable.AcceptChanges();
				//now sort the view 
				temptable.DefaultView.Sort = MasterCols.NamesList.ToString();
				//then copy ionto the master table
				foreach (DataRowView temprow in temptable.DefaultView)
				{
					row = masterListTbl.NewRow();
					row[(int) (MasterCols.NamesList)] = temprow[(int) (MasterCols.NamesList)];
					row[(int) (MasterCols.eventIDList)] = temprow[(int) (MasterCols.eventIDList)];
					masterListTbl.Rows.Add(row);
				}
				masterListTbl.AcceptChanges();
			} //end if viewOnly == false
			#endregion
		}

		private void createCountersLogTable( )
		{
			counterTbl = new CountersTable("CountersTableName");
			counterView = new DataView(counterTbl);
			counterView.AllowNew = false;
		}
			
		private void fillCounterTable()
		{
            counterTbl.Clear();
			defaultEventNames = new String[log.Length];
			for(int i = 0;i<(log.Length);i++)
			{
				int minsAndSecs, mins, secs;
				int groupIDMask = 0x03C0, saveMask = 0x8000, FIFOMask = 0x6000;
				row = counterTbl.NewRow();
				int ID = (int) log[ i ].eventID;
				ushort eventNoSaveBit = log[ i ].eventID;
				//now get the event Number with the save bit stripped off
				eventNoSaveBit = (ushort) (eventNoSaveBit & 0x7FFF);
				string eventIdStr = eventNoSaveBit.ToString("X");
				while(eventIdStr.Length <4)
				{
					eventIdStr = "0" + eventIdStr;
				}
				defaultEventNames[i] = log[ i ].description + " (ID: 0x" + eventIdStr + ")";
				if(this.viewOnly == false)
				{
					row[(int) (CounterCol.Name)] = eventNoSaveBit;  //default to zero for empty slot
				}
				else
				{
					row[(int) (CounterCol.Name)] = defaultEventNames[i];//log[ i ].description;
				}
				row[(int) (CounterCol.SaveBit)] = false;  //default to false;

				if(eventNoSaveBit != 0)  //if this counter is used - otheriwse tableStyle nullText is inserted
				{
					ushort temp = (ushort) ((ID & groupIDMask) >> 6);
					ushort FIFO = log[ i ].eventID;
					#region determine FIFO for this event
					FIFO = (ushort) ((FIFO & FIFOMask) >> 13);
					switch (FIFO)
					{
						case 0:  //Non-Fifo
							row[(int) (CounterCol.Group)] = DriveWizard.SCCorpStyle.NonFifoGrpNames[temp];//.ToString();
							break;

						case 1:  //System FIFO
							row[(int) (CounterCol.Group)] = DriveWizard.SCCorpStyle.SystemFifoGrpNames[temp];//.ToString();
							break;

						case 2: //Fault FIFO
							row[(int) (CounterCol.Group)] = DriveWizard.SCCorpStyle.FaultFifoGrpNames[temp];//.ToString();
							break;

						case 3: //Other FIFO - currently SPARE in controller
							row[(int) (CounterCol.Group)] = DriveWizard.SCCorpStyle.SpareFifoGrpNames[temp];//.ToString();
							break;
					}
					#endregion determine FIFO for this event
					row[(int) (CounterCol.Count)] = log[ i ].counter.ToString();
					if(log[ i ].counter>0)  //contoller sending times when count is zero so blank out here
					{
						minsAndSecs = (int)(((int)log[ i ].firstMinsAndSecs) * 15);
						mins = minsAndSecs / 60;
						secs = minsAndSecs % 60;
						row[(int) (CounterCol.FirstTime)] = log[ i ].firstHours + ":" + mins.ToString( "00" ) + "." + secs.ToString( "00" );

						minsAndSecs = (int)(((int)log[ i ].lastMinsAndSecs) * 15);
						mins = minsAndSecs / 60;
						secs = minsAndSecs % 60;
						row[(int) (CounterCol.LastTime)] = log[ i ].firstHours + ":" + mins.ToString( "00" ) + "." + secs.ToString( "00" );
					}
					ID = log[ i ].eventID;
					row[(int) (CounterCol.SaveBit)] = System.Convert.ToBoolean((ID & saveMask) >> 15);
				}
				counterTbl.Rows.Add(row);
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

        #region DI Thread wrappers

        private void getAllEventIDsWrapper()
        {
            this.node.readODValue(allEventsForNodeSub);
        }
        private void requestEventLogWrapper()
        {
            node.readODValue(this.eventLogSub);
        }

        private void resetEventCounterLogWrapper()
        {
            node.writeODValue(resetEventLogSub, 0x01);
        }

        private void configureCountersWrapper()
        {
            node.writeODValue((ODItemData)(eventCntrIDSubs[slotNumber]), (long)requestedID);
        }

        //DR38000269 read & write missing event wrapper replaced by getMissingEventDescWrapper()
        // allowing all uknownFaultIDs to get retrieved in a single DI thread 
        private void getMissingEventDescWrapper()
        {            
            DIFeedbackCode feedback;

            for (eventIDRetrieval = 0; eventIDRetrieval < uknownFaultIDs.Count; eventIDRetrieval++)
            {
                ushort missingEventID = ((ushort)uknownFaultIDs[eventIDRetrieval]);
                feedback = node.writeODValue(eventIDsSub, (long)missingEventID);

                if (feedback == DIFeedbackCode.DISuccess)
                {
                    feedback = node.readODValue(eventNameSub);

                    if (feedback == DIFeedbackCode.DISuccess)
                    {
                        this.sysInfo.updateEventList((ushort)missingEventID, eventNameSub.currentValueString, node.productCode);
                    }
                }

                if (feedback == DIFeedbackCode.DINoResponseFromController)
                {
                    break;      // bomb out of loop if controller not responding
                }
            }
        }
        #endregion DI Thread wrappers

        #region DIThread timer
        private void timer1_Tick(object sender, System.EventArgs e)
        {
            if ((DIThread.ThreadState & System.Threading.ThreadState.Stopped) > 0)
            {
                timer1.Enabled = false; //kill timer first
                this.progressBar1.Value = this.progressBar1.Maximum;  //for aesthetics
                this.progressBar1.Visible = false;
                this.statusBar1.Text = "";
                string threadName = DIThread.Name;
                DIThread = null;

                switch (threadName)
                {
                    case "getAllEventIDsWrapper": //DR38000269
                        #region get all event names
                        sysInfo.insertErrorType("Failed to read events master list");
                        //If any uknownFaultIDs, start data retrieval thread & setup progress bar
                        sysInfo.processAllEventIDs(allEventsForNodeSub, out this.nodeEventList, out uknownFaultIDs);
                        if ((this.eventIDsSub != null) && (this.eventNameSub != null) && (uknownFaultIDs.Count > 0))
                        {
                            this.statusBar1.Text = "Getting missing event descriptions";
                            progressBar1.Maximum = uknownFaultIDs.Count;
                            progressBar1.Value = 0;
                            this.startDIThread("getMissingEventDescWrapper", getMissingEventDescWrapper);
                        }
                        else
                        {
                            if (this.sysInfo.systemAccess >= 2)
                            {
                                if (nodeEventList.Count > 1)  //I add one for zero(empty slot)
                                {
                                    viewOnly = false;
                                }
                            }

                            createMasterList();
                            
                            this.startDIThread("requestEventLogWrapper", requestEventLogWrapper);
                        }
                        break;
                        #endregion get all event names

                    case "resetEventCounterLogWrapper":
                        #region reset all event counters
                        this.startDIThread("requestEventLogWrapper", requestEventLogWrapper);
                        break;
                        #endregion reset all event counters

                    case "requestEventLogWrapper":
                        #region get event log
                        sysInfo.insertErrorType("Errors when getting event counters log");
                        this.sysInfo.processEventLog(this.eventLogSub, out log, out uknownFaultIDs); //DR38000269
                        this.showUserControls();
                        break;
                        #endregion get event log

                    case "configureCountersWrapper":
                        #region update counter event IDs
                        slotNumber++;
                        if (slotNumber < log.Length)
                        {
                            if (counterTbl.Rows[slotNumber][(int)CounterCol.Name].ToString() != "")  //null text
                            {
                                requestedID = System.Convert.ToUInt16(counterTbl.Rows[slotNumber][(int)CounterCol.Name].ToString());
                            }
                            if ((bool)(counterTbl.Rows[slotNumber][(int)CounterCol.SaveBit]) == true)
                            {
                                requestedID = (ushort)(requestedID | 0x8000);  //set the correct bit
                            }
                            this.startDIThread("configureCountersWrapper", configureCountersWrapper);
                        }
                        else
                        {
                            this.startDIThread("requestEventLogWrapper", requestEventLogWrapper);
                        }
                        break;
                        #endregion update counter event IDs

                    case "getMissingEventDescWrapper":
                        #region re-process with missing descriptions
                        //DR38000269 re-process the log now that unknown descriptors have been read
                        sysInfo.processAllEventIDs(allEventsForNodeSub, out this.nodeEventList, out this.uknownFaultIDs);
                        if (this.sysInfo.systemAccess >= 2)
                        {
                            if (nodeEventList.Count > 1)  //I add one for zero(empty slot)
                            {
                                viewOnly = false;
                            }
                        }
                        createMasterList();
                        createCountersLogTable();
                        this.pictureBox1.Visible = false;
                        this.startDIThread("requestEventLogWrapper", requestEventLogWrapper);
                        break;
                        #endregion re-process with missing descriptions
                }
            }
            else
            {
                #region Thread in progress
                switch (DIThread.Name)
                {
                    case "resetEventCounterLogWrapper":
                        if (this.progressBar1.Value < this.progressBar1.Maximum)
                        {
                            this.progressBar1.Value++;
                            this.progressBar1.Update();
                        }
                        break;

                    case "getMissingEventDescWrapper": //DR38000269 progress bar during unknownIDs retrieval
                        if ((progressBar1.Value < progressBar1.Maximum) && (eventIDRetrieval < progressBar1.Maximum))
                        {
                            progressBar1.Value = eventIDRetrieval;
                            progressBar1.Update();
                        }
                        break;
                }
                #endregion Thread in progress
            }
        }
        #endregion DIThread timer

        #region user interaction zone
        private void dataGrid1_Click(object sender, System.EventArgs e)
		{
			if(this.sysInfo.systemAccess<2)
			{
				this.label1.Text = "Insufficient access to change event counters";
				return;
			}
			//this method is to force one-click toggling of hte boolean Monitoring column
			//normally the first mouse click sets the focus to the cell
			//and the second click toggles the boolean value.
			//This is mildly annoying for the user - Single click changeover has a better feel.
			//this function also ensure that the row is selected regardless of which cell was clicked
			//locate mouse position relative to the dataGrid
			Point pt = this.dataGrid1.PointToClient(Control.MousePosition);
			DataGrid.HitTestInfo hti = this.dataGrid1.HitTest(pt);
			int myCol = (int)CounterCol.SaveBit;
			//check mouse was clicked over a cell
			if( (hti.Type == DataGrid.HitTestType.Cell) && (hti.Column == myCol))
			{
				if(this.counterTbl.Rows[hti.Row][(int) CounterCol.Name].ToString() != "") 
				{
					this.dataGrid1[hti.Row, hti.Column] = !((bool)this.dataGrid1[hti.Row, hti.Column]);  //force the toggle 
					RefreshRow(hti.Row);
				}
				else
				{
					this.dataGrid1[hti.Row, hti.Column] = false;
				}
			}
		}
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: RefreshRow
		///		 *  Description     : Calculates screen area of a user selected row.
		///		 *					  Then invalidates this area. Used when user slects a row to be included in group filtering 
		///		 *					  This combined with the overided paint function for the ColumnStyle 
		///		 *					  causes the selected row to be highglighted
		///		 *  Parameters      : Row number (datagrid)
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>
		private void RefreshRow(int row)
		{
			Rectangle rect = this.dataGrid1.GetCellBounds(row, 0);
			int myTop  = (int) rect.Top;
			rect = new Rectangle(rect.Left, myTop, this.dataGrid1.Width, rect.Height);
			this.dataGrid1.Invalidate(rect);
		}

		private void submitBtn_Click(object sender, System.EventArgs e)
		{
			hideUserControls();
			configureCounters();
		}
        private void configureCounters()
        {
            //do first slot - remainder are done each time our thread completes
            slotNumber = 0;
            if (counterTbl.Rows[slotNumber][(int)CounterCol.Name].ToString() != "")  //null text
            {
                requestedID = System.Convert.ToUInt16(counterTbl.Rows[slotNumber][(int)CounterCol.Name].ToString());
            }
            if ((bool)(counterTbl.Rows[slotNumber][(int)CounterCol.SaveBit]) == true)
            {
                requestedID = (ushort)(requestedID | 0x8000);  //set the correct bit
            }
            this.startDIThread("configureCountersWrapper", configureCountersWrapper);
        }


		private void resetBtn_Click(object sender, System.EventArgs e)
		{
			hideUserControls();
            this.progressBar1.Maximum = (resetEventLogSub.commsTimeout / this.timer1.Interval);
            this.startDIThread("resetEventCounterLogWrapper", resetEventCounterLogWrapper);
		}


		#endregion user interaction zone

		#region finalisation/exit
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

		private void closeBtn_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		private void COUNTERS_LOG_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.statusBar1.Text =  "Performing finalisation, please wait";
			#region disable all timers
			this.timer1.Enabled = false;
			#endregion disable all timers
			#region stop any active threads
            if (DIThread != null)
			{
                if ((DIThread.ThreadState & System.Threading.ThreadState.Stopped) == 0)
				{
                    DIThread.Abort();

                    if (DIThread.IsAlive == true)
					{
#if DEBUG
                        SystemInfo.errorSB.Append("\nFailed to close Thread: " + DIThread.Name + " on exit");
#endif
                        DIThread = null; //force GC dispose this thread
					}

				}
			}
			#endregion stop any active threads
			e.Cancel = false; //force this window to close
		}
		private void COUNTERS_LOG_WINDOW_Closed(object sender, System.EventArgs e)
		{
			#region reset window title and status bar
			this.statusBar1.Text = "";
			#endregion reset window title and status bar
		}

		#endregion finalisation /exit

        #region hide/show user controls
        private void hideUserControls()
        {
            this.dataGrid1.Enabled = false;
            this.updateCountersBtn.Visible = false;
            this.resetBtn.Visible = false;
        }

        private void showUserControls()
        {
            createCountersLogTable();
            this.pictureBox1.Visible = false;
            fillCounterTable();
            applyEventTableStyle();  //add the combobox column to the datagrid now that we have filled masterListTbl with master event list
            this.dataGrid1.DataSource = counterView;
            this.dataGrid1.Enabled = true;  //ignore any user clicks until all the data is in
            this.dataGrid1.ReadOnly = false;

            this.statusBar1.Text = "Displaying " + this.selectedNodeText + "event counters";
            this.dataGrid1.CaptionText = "Event Counters log for: " + selectedNodeText;
            //see whether to allow the user to configure counters/reset the counters)
            if ((this.eventIDsSub == null) || (this.log.Length == 0))
            {
                this.label1.Text = "Event Counters not available";
            }
            else if (this.allEventsForNodeSub == null)
            {
                this.label1.Text = "Event List not available";
            }
            else if (viewOnly == true)
            {
                this.Text = "DriveWizard: Event Counters view";
                this.label1.Text = "Event Counters(read only)";
            }
            else
            {
                if (this.sysInfo.systemAccess >= 2) 
                {
                        this.updateCountersBtn.Visible = true;
                        if (resetEventLogSub != null)
                        {
                            this.resetBtn.Visible = true;
                        }
                        this.Text = "DriveWizard: Event Counters configuration";
                        this.label1.Text = "Select Event Name to change";
                }
                else
                {
                    this.Text = "DriveWizard: Event Counters view";
                    this.label1.Text = "Event Counters (read only)";
                }
            }
            this.dataGrid1.Enabled = true;
            if (SystemInfo.errorSB.Length > 0)
            {
                sysInfo.displayErrorFeedbackToUser("");
            }
        }

        #endregion hide/show user controls

        #region minor methods

        private void dataGrid1_Resize(object sender, System.EventArgs e)
		{
			if(this.dataGrid1.Enabled == true)
			{
				applyEventTableStyle();
			}
		}
		private void applyEventTableStyle()
		{
			float [] percents = {0.4F, 0.2F, 0.1F, 0.1F, 0.1F, 0.1F};
			int [] colWidths  = new int[percents.Length];
			colWidths  = this.sysInfo.calculateColumnWidths(dataGrid1, percents, 0, dataGrid1DefaultHeight);
			tableStyle = new TSCounters(colWidths, masterListTbl, counterTbl.TableName,viewOnly, defaultEventNames);
			// Sets row height to accommodate the larger dimensions of the ComboBox 
			// using invisible combobox as datum
			tableStyle.PreferredRowHeight = this.cboDatum.Height + 1;
			//only apply tableStyle to non-minimized windows
			if(this.dataGrid1.ClientRectangle.Width >0) //needed because trying to clear/add a tableStyle to a minimized window causes exception
			{
				this.dataGrid1.TableStyles.Clear();
				this.dataGrid1.TableStyles.Add(tableStyle);//finally attahced the TableStyles to the datagrid
			}
		}

		#endregion minor methods

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(COUNTERS_LOG_WINDOW));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.resetBtn = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.updateCountersBtn = new System.Windows.Forms.Button();
            this.cboDatum = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.openMI = new System.Windows.Forms.MenuItem();
            this.saveMI = new System.Windows.Forms.MenuItem();
            this.printMI = new System.Windows.Forms.MenuItem();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(800, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // resetBtn
            // 
            this.resetBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.resetBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.resetBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.resetBtn.Location = new System.Drawing.Point(8, 486);
            this.resetBtn.Name = "resetBtn";
            this.resetBtn.Size = new System.Drawing.Size(140, 25);
            this.resetBtn.TabIndex = 21;
            this.resetBtn.Text = "&Reset all counters";
            this.resetBtn.Visible = false;
            this.resetBtn.Click += new System.EventHandler(this.resetBtn_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // updateCountersBtn
            // 
            this.updateCountersBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.updateCountersBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.updateCountersBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.updateCountersBtn.Location = new System.Drawing.Point(152, 486);
            this.updateCountersBtn.Name = "updateCountersBtn";
            this.updateCountersBtn.Size = new System.Drawing.Size(140, 25);
            this.updateCountersBtn.TabIndex = 20;
            this.updateCountersBtn.Text = "&Update Counters";
            this.updateCountersBtn.Visible = false;
            this.updateCountersBtn.Click += new System.EventHandler(this.submitBtn_Click);
            // 
            // cboDatum
            // 
            this.cboDatum.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDatum.Location = new System.Drawing.Point(432, 216);
            this.cboDatum.Name = "cboDatum";
            this.cboDatum.Size = new System.Drawing.Size(128, 21);
            this.cboDatum.TabIndex = 22;
            this.cboDatum.Visible = false;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(760, 25);
            this.label1.TabIndex = 19;
            this.label1.Text = "Please wait...";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(704, 486);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 25);
            this.button1.TabIndex = 18;
            this.button1.Text = "&Close window";
            this.button1.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 0;
            this.menuItem4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.openMI,
            this.saveMI,
            this.printMI});
            this.menuItem4.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
            this.menuItem4.Text = "&File";
            // 
            // openMI
            // 
            this.openMI.Index = 0;
            this.openMI.Text = "&Open log from file";
            // 
            // saveMI
            // 
            this.saveMI.Index = 1;
            this.saveMI.Text = "&Save log to file";
            // 
            // printMI
            // 
            this.printMI.Index = 2;
            this.printMI.Text = "&Print log";
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem4});
            // 
            // dataGrid1
            // 
            this.dataGrid1.AllowSorting = false;
            this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dataGrid1.CaptionText = "Event Counters";
            this.dataGrid1.DataMember = "";
            this.dataGrid1.Enabled = false;
            this.dataGrid1.FlatMode = true;
            this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Location = new System.Drawing.Point(0, 38);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.ParentRowsVisible = false;
            this.dataGrid1.ReadOnly = true;
            this.dataGrid1.RowHeadersVisible = false;
            this.dataGrid1.Size = new System.Drawing.Size(832, 442);
            this.dataGrid1.TabIndex = 17;
            this.dataGrid1.Click += new System.EventHandler(this.dataGrid1_Click);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 544);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(840, 22);
            this.statusBar1.TabIndex = 23;
            this.statusBar1.Text = "Requesting List of all known events";
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 521);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(840, 23);
            this.progressBar1.TabIndex = 24;
            // 
            // COUNTERS_LOG_WINDOW
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(840, 566);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.cboDatum);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGrid1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.resetBtn);
            this.Controls.Add(this.updateCountersBtn);
            this.Menu = this.mainMenu1;
            this.Name = "COUNTERS_LOG_WINDOW";
            this.Text = "COUNTERS_LOG_WINDOW";
            this.Load += new System.EventHandler(this.COUNTERS_LOG_WINDOW_Load);
            this.Closed += new System.EventHandler(this.COUNTERS_LOG_WINDOW_Closed);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.COUNTERS_LOG_WINDOW_Closing);
            this.Resize += new System.EventHandler(this.COUNTERS_LOG_WINDOW_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        private void COUNTERS_LOG_WINDOW_Resize(object sender, EventArgs e)
        {
            if (this.dataGrid1.Enabled == true)
            {
                applyEventTableStyle();
            }
        }
	}
}
