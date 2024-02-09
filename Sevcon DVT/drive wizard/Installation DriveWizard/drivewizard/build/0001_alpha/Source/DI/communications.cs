/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.94$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:18/03/2008 23:49:04$
	$ModDate:18/03/2008 21:01:16$

ORIGINAL AUTHOR
    Alison King

DESCRIPTION
	This object builds up the required SDO messages which
    are then sent via the CANbus by the VCI object.  It 
    also performs functions to find the system by listening 
    in to the bus.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36685: communications.cs 

   Rev 1.94    18/03/2008 23:49:04  ak
 awaitResponse() and awaitBlockSegmentResponse() changed to downgrade the DI
 thread temporarily to BelowNormal.


   Rev 1.93    12/03/2008 13:04:36  ak
 Added Thread.Sleep(1) to awaitResponse() and awaitBlockResponse() to sleep
 the DI thread temporarily and allow the IXXAT thread and receive delegate a
 chance to run.


   Rev 1.92    25/02/2008 16:17:54  jw
 Number of segments in Block Downlaod promoted to class level. New code to
 chekc that all block segment resposnes have been received before sending out
 endSegmentRequest ( with timeout) 


   Rev 1.91    21/02/2008 09:37:46  jw
 Call to redundant resetTImeStamp method removed


   Rev 1.90    21/02/2008 09:28:24  jw
 removal of VCI2 methods


   Rev 1.89    19/02/2008 15:28:30  jw
 New VCI class which is compatible with both VCI3 and VCI2 ( replaces
 conditional compilation)


   Rev 1.88    18/02/2008 15:39:26  jw
 Merge recovery form hibernation code into single method for VCI2 back
 compatibility. Imporved hibernation recovery ( VCI2 works OK, VCI3 -
 intiSocket throwing exception still)


   Rev 1.87    18/02/2008 14:18:14  jw
 VCI2 CAN methods renamed and merged for consistency with VCI3 - towards back
 compatibility


   Rev 1.86    18/02/2008 11:40:04  jw
 VCI2 CAN methods renamed and merged for consistency with VCI3 - towards back
 compatibility


   Rev 1.85    18/02/2008 09:28:52  jw
 VCI2 CAN methods moved form communications to VCI 2 and renamed for
 consistency with VCI3 - towards back compatibility


   Rev 1.84    18/02/2008 07:48:08  jw
 Static params changed to non-static on VCI for conformity with VCI2. VCI2
 badurate param renamed to be as per VCI3 - step towards full backwards
 compatibility


   Rev 1.83    15/02/2008 12:38:08  jw
 TxData and the Monitiring params were static in VCI3. By passing VCI as
 object into the received message event handler we can make then instance
 variables as per VCI2 - closer to back compatibility


   Rev 1.82    15/02/2008 11:42:32  jw
 Params for Cna adapter hardwar eintialised an drunning change to same ones
 used for VCI3 - step towards full backwards compatibility


   Rev 1.81    14/02/2008 09:28:04  jw
 Intiialsing Baud and Mask should prevent reception of rtr messages


   Rev 1.80    13/02/2008 14:58:52  jw
 More VCI3. Now works generally OK but more testing/optimisation required. 


   Rev 1.79    12/02/2008 08:49:30  jw
 Ongoing VCI3 work. Options and Select profiel windows changed to simplify
 threading and improve feedback.  Prog bar vlaue determination line made
 exception proof. Max and current values used by progress bars determined
 within DI for encapsulation and values reflect activitiy better.


   Rev 1.78    06/02/2008 08:15:14  jw
 DR38000239. One of SDORead methode renamed and simlipied for clarity. Other
 SDO read was grouping no response with invalid responses - now seperated


   Rev 1.77    25/01/2008 10:46:44  jw
 VCI3 and Vista functionality files merge - more testing required


   Rev 1.76    21/01/2008 12:03:12  jw
 File merge for VCI3/ Vista. These changes are those to go in all builds


   Rev 1.75    05/12/2007 21:13:42  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using DW_Ixxat;
using Ixxat.Vci3;           //IXXAT conversion Jude
using Ixxat.Vci3.Bal;       //IXXAT conversion Jude
using Ixxat.Vci3.Bal.Can;   //IXXAT conversion Jude


namespace DriveWizard
{
	#region Measured Parameter vlaue and Timestamp - used for PDO graphing
	public struct DataPoint 
	{
		public DataPoint (uint passed_timeStamp, long passed_value)
		{
			timeStamp = passed_timeStamp;
			measuredValue = passed_value;
		}
		public uint		timeStamp;	
		public long     measuredValue;	
	};
	#endregion Measured Parameter vlaue and Timestamp - used for PDO graphing
	#region reference to a single OD sub and its CAN node
	/// <summary>
	/// Each instance of this struct defines the information needed to perform high resolution
	/// graphical monitoring for one item being monitored.  Only if the item is PDO mappable can
	/// PDO maps be consumed for high resolution graphing (otherwise must use SDOs). The node
	/// the item is on, it's OD index and sub for live monitoring, which COBID the PDO is on and
	/// how to extract the data from the PDO.
	/// </summary>
	public class ODItemAndNode
	{
		public ODItemAndNode()
		{
		}
		internal ODItemData ODparam = null;	
		internal nodeInfo node = null;
	};
	#endregion reference to a single OD sub and its CAN node

	#region enumerated type definitions
	/*
	 * Enumerated type for the Sevcon fault logs. The formatting of logs received from
	 * an espAC are specific to the type and this knowledge is assumed by DW to reconstruct
	 * the data  i.e. used to determine log specific handling.
	 */
	///<summary>Sevcon log types received from espACs using a domain object.</summary>
	public enum LogType { FaultLog, SystemLog, EventLog, CustOpLog, SEVCONOpLog, ActiveFaultsLog };
	
	#endregion
#if DEBUGGING_DCF
	internal class odRef
	{
		public odRef()
		{
			data = new byte[8];
			for(int i = 0;i<8;i++)
			{
				data[i] = 0;
			}
		}
		internal int index;
		internal int sub;
		internal long val;
		internal byte [] data;
	}
#endif
	/// <summary>
	/// Communications object provides the upper layer to build up the required
	/// SDO messages to get sent on the CANbus by the VCI object.  It also
	/// performs functions to find the system by listening in to the bus.
	/// </summary>
	public class communications
	{
#if DEBUGGING_DCF
	internal ArrayList tempItemsInDCF;
#endif
		internal ODItemData currentODSub = null;
		#region private variable declarations
        private SystemInfo sysInfo;
        public cVCI VCI = null;

		/* CAN message receive timeout currently being used.  Normally, the first operation performed
		 * by DW is to detect the baud rate so initialise for that.
		 */
		private	static int	rxTimeout = SCCorpStyle.TimeoutListenIn;

		/* Flag which is set when the timer delegate function has expired to indicate timeout occurred.
		 */
		private static bool timerExpired = false;

		/* The CAN message response timer for detecting a timeout must run on a separate thread.
		 * Initialise one ready.  Used as a single shot timer with a duration of 500ms.
		 */
		private static		System.Threading.Timer responseTimer; 

        private int noOfBlockSegments = 0;
		#endregion

		#region property declarations

		#region list of nodes found on the system
		/* List of node IDs that responded when trying to find the system by transmitting an SDO to
		 * every possible node [1...127].
		 */
		private int []	_nodeList = new int[0];
		///<summary>List of nodes that responded when trying to find the system.</summary>
		public int [] nodeList
		{
			get
			{
				return ( _nodeList );
			}

			set
			{
				_nodeList = value;
			}
		}
		#endregion

		#region found system baud rate
		/* System baud rate which has been found or is set to _unknown if failed to detect
		 * or not tried yet.
		 */
		private BaudRate	_systemBaud;
		///<summary>Baud rate found when trying to find the connected system.</summary>
		public BaudRate systemBaud
		{
			get
			{
				return ( _systemBaud );
			}
            set
            {
                _systemBaud = value;

            }
		}
		#endregion

        #region baud rate under test during auto detect
        /* Baud rate under test (used for auto detect).  This is a property so that the GUI
		 * can see which baud rate is being tested to update the progress bar.
		 */
        private BaudRate _baudUnderTest = 0;
        ///<summary>Baud rate currently being tested (used for auto detecting system).</summary>
        public BaudRate baudUnderTest
        {
            get
            {
                return (_baudUnderTest);
            }
        }
        #endregion

        #region SDO read domain progress bar indication
        /* Expected number of bytes to be received from the controller.  This is sent in the
		 * initiate download response so is extracted and used for subsequent packets.
		 */
		private uint _SDOReadDomainRxDataSize = 0;
		/// <summary> Expected data domain size for the SDO read</summary>
		public uint SDOReadDomainRxDataSize
		{
			get
			{
				return ( _SDOReadDomainRxDataSize );
			}

			set
			{
				_SDOReadDomainRxDataSize = 0;
			}
		}

		/* Keeps a running count of how many data bytes have already been received via
		 * SDO upload segments.  Allows reconstruction of the rxData array between
		 * receiving the multiple segments.
		 */
		private int _SDOReadDomainRxDataPtr = 0;
		/// <summary> Current number of data bytes read for this SDO read domain request</summary>
		public int SDOReadDomainRxDataPtr
		{
			get
			{
				return ( _SDOReadDomainRxDataPtr );
			}

			set
			{
				_SDOReadDomainRxDataPtr = 0;
			}
		}
		#endregion
		#endregion

		#region constructor & destructor
		//-------------------------------------------------------------------------
		// Name			: constructor
		// Description     : Initialisation specific code performed.
		// Parameters      : None
		// Used Variables  : responseTimer - comms timeout timer for SDO no response detection
		// Preconditions   : An object of the communications type is created.
		// Post-conditions : The object created is initialised.
		// Return value    : None
		//-------------------------------------------------------------------------
		///<summary>Communications class constructor.</summary>
		public communications(SystemInfo passed_systemInfo)
        {
            VCI = new cVCI();

#if DEBUGGING_DCF
			tempItemsInDCF = new ArrayList();
#endif
            // initialise the communication response timer (detecting no response timeouts to SDOs)
			responseTimer = new System.Threading.Timer( new TimerCallback( threadTimerExpired ), null, SCCorpStyle.TimeoutListenIn, Timeout.Infinite );

#if DEBUG
            System.Console.WriteLine("Comms timer created: " + System.DateTime.Now.TimeOfDay.ToString());
#endif
            this.sysInfo = passed_systemInfo;
		}

		//---------------------------------------------------------------------------
		//  Name			 : destructor
		//  Description     : Performs specific clearing up when this instance of the
		//					   object is destroyed.
		//	 Parameters		 : None
		//  Used Variables  : responseTimer - comms timeout timer for SDO no response detection
		//  Preconditions   : This object is being destroyed.
		//  Post-conditions : All necessary cleaning up has been performed.
		//  Return value    : None
		//---------------------------------------------------------------------------
		///<summary>Communications destructor which disposes of class timers.</summary>
		~communications()
		{
			if ( responseTimer != null )
			{
				responseTimer.Dispose();

				if ( responseTimer != null )
				{
					responseTimer = null;
				}
				VCI = null;
			}
		}
		#endregion

        #region detecting baud rate by listening to bus traffic
        internal void detectBaud()
        {
            VCI.stopHbeatAndEmerThreadTimer();
            VCI.messageType = CANMessageType.IDList;
            VCI.nodeIDsFound = new ArrayList();
            SystemInfo.itemCounter1 = 0;
            this._systemBaud = this.VCI.detectBaudRate();
        }
        
        #endregion detecting baud rate by listening to bus traffic

        #region restart CAN adapter after hibernation

        ///<summary>Initialises the acceptance mask and filter to accept all messages then restarts the IXXAT adapter.</summary>
		/// <param name="baud">reqiured baud rate to run the CANbus at</param>
		/// <param name="acceptanceMask">acceptance mask to limit rx packets to only those of 
		/// interest to DW (DW runs quicker with less rx interrupts)</param>
		/// <param name="acceptanceFilter">acceptance filter for rx packets</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		//---------------------------------------------------------------------------
		//  Name			: restartCAN()
		//  Description     : Restarts the IXXAT adapter after use by SCWiz or after
		//					  hibernation. This opens the adapter, initialises the buffers
		//					  and baud & acceptance masks.  Then the IXXAT adapter is 
		//					  restarted (operational mode) with an optional time delay,
		//					  needed only after a hibernation period.
		// Parameters      : afterHibernation - whether it was called to restart the CAN
		//						after the PC was in hibernation mode or not
		// Used Variables  : VCI - Visual component interface object for the IXXAT adapter
		//					  VCIRunning - flag to indicate if adapter is running (the user
		//								   could be working offline)
		//					  IXXATAdapterOpenedOK - flag to indicate if the adapter hardware has
		//									been opened for use
		//					  timerExpired - flag to indicate when 500ms timer has expred
		// Preconditions   : The GUI has detected that the laptop has gone into hibernate/
		//					  suspend mode and it has now resumed.  This turns the power off
		//					  to the IXXAT adapter and causes numerous handled exceptions.
		//					  This routine closes the existing VCI and opens another so
		//					  that DW can resume normal operation.  Without this, the user
		//					  had to shut down DW, remove & replace the IXXAT adapter in
		//					  the USB port and sometimes even restart the PC.  This routine
		//					  circumvents those activities.
		// Post-conditions : The IXXAT adapter has been closed down and reopened and
		//					  reconfigured to the previous settings following the resume.
		// Return value    : None
		//---------------------------------------------------------------------------
		///<summary>Reinitialises and restarts the IXXAT adapter (after hibernation or SCWiz).</summary>
		/// <param name="afterHibernation">whether it was called to restart the CAN after the PC
		/// was in hibernation mode or not</param>
        public void restartCAN(bool afterHibernation)
        {
            VCI.closeCANAdapterHW();
            VCI.SetupCAN(_systemBaud, 0x00, 0x00);
        }
        

        #endregion

		#region find system
		//--------------------------------------------------------------------------
		//  Name			: findNodeIDs()
		//  Description     : This function initialises the IXXAT dongle to the userBaud
		//					  rate and sets the acceptance mask & filter to filter out
		//					  any non-SDO messages received.  It then sends 127 error register
		//					  messages in one go without waiting for any individual replies.
		//					  Then it waits for 3s for any valid replies in order to 
		//					  find out what nodes exist.  This list is checked for duplicate
		//					  messages and then collates a final nodeList as representing
		//					  the connected system.
		//  Parameters      : userBaud - user selected baud rate
		//  Used Variables  : _nodeList - property containing the final list of nodes 
		//								  found on this system
		//					  VCI.getIDList - flag used to determine level of error checking
		//								  used by the VCI on received messages
		//					  timerExpired - flag to indicate when comms timer has expired
		//  Preconditions   : The user has selected a baud or it has been automatically detected.
		//  Post-conditions : The _nodeList property contains a list of all the node IDs of
		//					  those nodes that were found on the connected CANbus system.
		//  Return value    : fbc indicates success or gives a failure reason
		//--------------------------------------------------------------------------
		///<summary>Finds the connected devices node IDs by transmitting error register SDOs to nodes 1 to 127 to see what replies it receives.</summary>
		/// <param name="userBaud">user selected baud rate</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
        public void findNodeIDs()
        {
            SystemInfo.itemCounterMax = 6;
            SystemInfo.itemCounter1 = 0;
            #region initialise parameters
            // data array to hold received data when the SDO is sent
            byte[] rxData = new byte[8];
            #endregion initialise parameters

            #region comments
            /* 
			* Cycle through all node IDs to see which get a response.  
			* This determines the nodes on the system (at current baud rate).
			* Set the acceptance filter and mask to accept 0x580 to 0x5ff (expected
			* SDO response COB-IDs) - required as receiving everything slows down 
			* DW operation significantly when there is high CAN traffic.
			*/
            #endregion comments
            #region comments
            /* 
				 * Get node ID list - means the VCI receive interrupt routine doesn't perform
				 * the usual stringent checks. Accepts abort messages and all sorts as any
				 * old kind or reply means that that node ID is valid.
				 */
            #endregion comments
            VCI.stopHbeatAndEmerThreadTimer();
            VCI.messageType = CANMessageType.IDList;
            VCI.nodeIDsFound = new ArrayList();
            #region comments
            /* Transmit all ID error register requests without waiting for a response.
				 * This is to speed the process up a bit for the poor, waiting user.
				 */
            #endregion comments

            #region check for responses
            for (int nodeID = 1; nodeID < 128; nodeID++)
            {
                for (ushort i = 0; i < SCCorpStyle.SDONoResponseRetries; i++)
                {
                    //DR38000239 JW method name changed for clarity and simplified
                    DIFeedbackCode feedback = checkIfNodeConnected(nodeID, ref rxData, false);
                    if (feedback != DIFeedbackCode.DINoResponseFromController)
                    {
                        break;
                    }
                }
            }
            #endregion check for responses
            #region comments
            /* Overall delay to receive responses from any nodes but split into
				 * six 0.5s chunks so that we can update the GUI progress bar.
				 * This gives a 3s period for any node who wants to to reply when the
				 * bus is quite (we've clogged it with all our error reg transmits).
				 */
            #endregion comments
            rxTimeout = SCCorpStyle.VCI3TimeOutFindNodeIDs;
            for (int delayPeriods = 0; delayPeriods < 6; delayPeriods++)
            {
                resetTimer();
                while (timerExpired == false) ;
                SystemInfo.itemCounter1++;
            }
            // clear flag in VCI so normal SDO message checking will be applied now.

            VCI.messageType = CANMessageType.SDO;
            // put the comms timer value to the normal value for SDO messages.
            rxTimeout = SCCorpStyle.TimeoutDefault;
            // copy over found nodes
            _nodeList = new int[VCI.nodeIDsFound.Count];
            VCI.nodeIDsFound.CopyTo(_nodeList);
            if (this.VCI.nodeIDsFound.Count == 0)
            {
                SystemInfo.errorSB.Append("\nNo nodes found at selected baud rate. \nCheck connections and retry");
            }
            return;
        }

		#endregion

		#region SDOWrite (overloaded)
		//--------------------------------------------------------------------------
		//  Name			: SDOWrite()
		//  Description     : This function takes the newValue, builds up an SDO message
		//				      to send to the node of nodeID which then writes this value
		//					  to the OD item in the controller's OD of index and subIndex.
		//					  dataSize is needed to determine the underlaying data type for
		//					  this OD item, to determine how many SDOs are required and
		//					  to reformat the number correctly. 
		//					  This function is overloaded for a string data type for newValue.
		//					  This function is overloaded for a byte array data type for newValue.
		//  Parameters      : nodeID - node ID of the controller on the network who's OD
		//							   needs updating
		//					  index - index of the OD item to be updated
		//					  subIndex - sub index of the OD item to be updated
		//					  dataSize - size in bytes of the new value to be written
		//								 to the controller (no. of bytes to transmit)
		//					  newValue - new value to be written to the controller OD item
		//					  commsTimeout - communication timer timeout to be used with
		//						this object before a no response is declared
		//  Used Variables  : None
		//  Preconditions   : The GUI has gathered the nodeID, index, subIndex of the OD item
		//					  to change and the newValue which has already been range checked.
		//  Post-conditions : The item of index and subIndex in the controller of ID nodeID
		//					  has had it's value updated to newValue (unless failed).
		//  Return value    : fbc indicating success or a failure reason
		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an SDO to write a new long value to the required OD item in the nodeID device.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs updating</param>
		/// <param name="index">index of the OD item to be updated</param>
		/// <param name="subIndex">sub index of the OD item to be updated</param>
		/// <param name="dataSize">size in bytes of the new value to be written to the 
		/// controller (number of bytes to transmit)</param>
		/// <param name="newValue">new value to be written to the controller OD item</param>
		/// <param name="commsTimeout">communication timer timeout to be used with
		/// this object before a no response is declared</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode SDOWrite( int nodeID, int index, int subIndex, uint dataSize, long newValue, int commsTimeout )
		{
			#region debug code
#if DEBUGGING_DCF
			odRef myItem = new odRef();
			myItem.index = index;
			myItem.sub = subIndex;
			myItem.val = newValue;

			myItem.data[0] = (byte)newValue;
			myItem.data[1] = (byte)( newValue >> 8 );
			myItem.data[2] = (byte)( newValue >> 16 );
			myItem.data[3] = (byte)( newValue >> 24 );


			tempItemsInDCF.Add(myItem);
#endif
			#endregion debug code

			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;

			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				return ( DIFeedbackCode.DISuccess );
			}
			/* When an SDO write requires multiple segments to download the new value, each
			 * subsequent SDO segment download in the sequence must have the toggle bit toggled.
			 * This is so the controller who is receiving this can detect if SDOs are out
			 * of sequence or have gone missing etc.
			 */
			bool toggleBit = false;

			/* Used to keep track of how many bytes have already been transmitted if the SDO
			 * write requires multiple SDO segments to download the new value.
			 */
			int txBytePtr = 0;

			// set the communication timer to specific timeout required for this object
			rxTimeout = commsTimeout;

			// Start sequence by building & sending an initiate SDO download request
			initiateDownloadRequest( nodeID, index, subIndex, dataSize, newValue );

			/* Until the entire SDO sequence to update this newValue has been completed,
			 * keep waiting for a reply from the last SDO message sent.  Then if a new
			 * SDO is required, build the next SDO message and send it.
			 */
			do
			{
				// what kind of SDO message response did we get?
                switch (VCI.rxResponseType)
				{
						// response from the controller to the initiate download SDO just sent
					case SDOMessageTypes.InitiateDownloadResponse:
					{
						/* If newValue is more than 4 bytes long then we need to send 
						 * it down in segments. Send the first download segment.
						 */
						if ( dataSize > 4 )
						{
							initiateDownloadSegment( nodeID, toggleBit, newValue, dataSize, ref txBytePtr );
							toggleBit = !toggleBit;
						}
							// Finished as it fitted in one expedited SDO.
						else
						{
							fbc = DIFeedbackCode.DISuccess;
						}
						break;
					}

						// response from the controller to the last downloaded segment SDO
					case SDOMessageTypes.DownloadSegmentResponse:
					{
						// Any more segments required? Build it up & send it.
						if ( ( dataSize - txBytePtr ) > 0 )
						{
							initiateDownloadSegment( nodeID, toggleBit, newValue, dataSize, ref txBytePtr );
							toggleBit = !toggleBit;
						}
						else // no - then finished download successfully.
						{
							fbc = DIFeedbackCode.DISuccess;
						}
						break;
					}

						// response from the controller to the last message is an abort message
					case SDOMessageTypes.AbortResponse:
					{
						long abortCode = 0;

						// extract the abort code from the message (4 bytes) back into a 32 bit no.
						for( int i = 0; i < 4; i++ )
						{
							abortCode <<= 8;
                            abortCode += VCI.rxMsg.data[7 - i];
						}

						// recast as enum type to give user a clue what number actually means.
						fbc = (DIFeedbackCode)abortCode;
						break;
					}

						/* No response (timed out), unknown response type or invalid in the 
						 * context of sending a download request SDO message.
						 */
					case SDOMessageTypes.NoResponse:
					case SDOMessageTypes.UnknownResponseType:
					case SDOMessageTypes.InvalidResponse:
					default:
					{
						fbc = DIFeedbackCode.DINoResponseFromController;
						break;
					}
				} // end of switch statement
			}
				// quit loop on a failure or finished SDO message sequence (DISuccess).
			while ( fbc == DIFeedbackCode.DIGeneralFailure );

			return ( fbc );
		}

		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an SDO to write a new string value to the required OD item in the nodeID device.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs updating</param>
		/// <param name="index">index of the OD item to be updated</param>
		/// <param name="subIndex">sub index of the OD item to be updated</param>
		/// <param name="dataSize">size in bytes of the new value to be written to the 
		/// controller (number of bytes to transmit)</param>
		/// <param name="newValue">new value to be written to the controller OD item</param>
		/// <param name="commsTimeout">communication timer timeout to be used with
		/// this object before a no response is declared</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode SDOWrite( int nodeID, int index, int subIndex, uint dataSize, string newValue, int commsTimeout )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;

			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				return ( DIFeedbackCode.DISuccess );
			}
			/* When an SDO write requires multiple segments to download the new value, each
			 * subsequent SDO segment download in the sequence must have the toggle bit toggled.
			 * This is so the controller who is receiving this can detect if SDOs are out
			 * of sequence or have gone missing etc.
			 */
			bool toggleBit = false;

			/* Used to keep track of how many bytes have already been transmitted if the SDO
			 * write requires multiple SDO segments to download the new value.
			 */
			int txBytePtr = 0;

			// set the communication timer to specific timeout required for this object
			rxTimeout = commsTimeout;

			// Start sequence by building & sending an initiate SDO download request
			initiateDownloadRequest( nodeID, index, subIndex, dataSize, newValue );

			/* Until the entire SDO sequence to update this newValue has been completed,
			 * keep waiting for a reply from the last SDO message sent.  Then if a new
			 * SDO is required, build the next SDO message and send it.
			 */
			do
			{
				// what kind of SDO message response did we get?
                switch (VCI.rxResponseType)
				{
						// response from the controller to the initiate download SDO just sent
					case SDOMessageTypes.InitiateDownloadResponse:
					{
						/* If newValue is more than 4 bytes long then we need to send 
						 * it down in segments. Send the first download segment.
						 */
						if ( dataSize > 4 )
						{
							initiateDownloadSegment( nodeID, toggleBit, newValue, dataSize, ref txBytePtr );
							toggleBit = !toggleBit;
						}
							// Finished as it fitted in one expedited SDO.
						else
						{
							fbc = DIFeedbackCode.DISuccess;
						}

						break;
					}

						// response from the controller to the last downloaded segment SDO
					case SDOMessageTypes.DownloadSegmentResponse:
					{
						// Any more segments required? Build it up & send it.
						if ( ( dataSize - txBytePtr ) > 0 )
						{
							initiateDownloadSegment( nodeID, toggleBit, newValue, dataSize, ref txBytePtr );
							toggleBit = !toggleBit;
						}
						else // no - then finished download successfully.
						{
							fbc = DIFeedbackCode.DISuccess;
						}
						break;
					}

						// response from the controller to the last message is an abort message
					case SDOMessageTypes.AbortResponse:
					{
						long abortCode = 0;

						// extract the abort code from the message (4 bytes) back into a 32 bit no.
						for( int i = 0; i < 4; i++ )
						{
							abortCode <<= 8;
                            abortCode += VCI.rxMsg.data[7 - i];
						}

						// recast as enum type to give user a clue what number actually means.
						fbc = (DIFeedbackCode)abortCode;
						break;
					}

						/* No response (timed out), unknown response type or invalid in the 
						 * context of sending a download request SDO message.
						 */
					case SDOMessageTypes.NoResponse:
					case SDOMessageTypes.UnknownResponseType:
					case SDOMessageTypes.InvalidResponse:
					default:
					{
						fbc = DIFeedbackCode.DINoResponseFromController;
						break;
					}
				} // end of switch statement
			}
				// quit loop on a failure or finished SDO message sequence (DISuccess).
			while ( fbc == DIFeedbackCode.DIGeneralFailure );

			return ( fbc );
		}

		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an SDO to write a new byte array value to the required OD item in the nodeID device.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs updating</param>
		/// <param name="index">index of the OD item to be updated</param>
		/// <param name="subIndex">sub index of the OD item to be updated</param>
		/// <param name="newValue">new value to be written to the controller OD item</param>
		/// <param name="commsTimeout">communication timer timeout to be used with
		/// this object before a no response is declared</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode SDOWrite( int nodeID, int index, int subIndex, ref byte[] newValue, int commsTimeout )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				return ( DIFeedbackCode.DISuccess );
			}
			/* When an SDO write requires multiple segments to download the new value, each
			 * subsequent SDO segment download in the sequence must have the toggle bit toggled.
			 * This is so the controller who is receiving this can detect if SDOs are out
			 * of sequence or have gone missing etc.
			 */
			bool toggleBit = false;

			/* Used to keep track of how many bytes have already been transmitted if the SDO
			 * write requires multiple SDO segments to download the new value.
			 */
			int txBytePtr = 0;

			/* Data size is not passed as a parameter for the data array type because the length
			 * of the array tells us the length of data to be transmitted.
			 */
			int dataSize = newValue.Length;

			// set the communication timer to specific timeout required for this object
			rxTimeout = commsTimeout;

			// Start sequence by building & sending an initiate SDO download request
			initiateDownloadRequest( nodeID, index, subIndex, ref newValue );

			/* Until the entire SDO sequence to update this newValue has been completed,
			 * keep waiting for a reply from the last SDO message sent.  Then if a new
			 * SDO is required, build the next SDO message and send it.
			 */
			do
			{
				// what kind of SDO message response did we get?
                switch (VCI.rxResponseType)
				{
						// response from the controller to the initiate download SDO just sent
					case SDOMessageTypes.InitiateDownloadResponse:
					{
						/* If newValue is more than 4 bytes long then we need to send 
						 * it down in segments. Send the first download segment.
						 */
						if ( dataSize > 4 )
						{
							initiateDownloadSegment( nodeID, toggleBit, ref newValue, ref txBytePtr );
							toggleBit = !toggleBit;
						}
							// Finished as it fitted in one expedited SDO.
						else
						{
							fbc = DIFeedbackCode.DISuccess;
						}

						break;
					}

						// response from the controller to the last downloaded segment SDO
					case SDOMessageTypes.DownloadSegmentResponse:
					{
						// Any more segments required? Build it up & send it.
						if ( ( dataSize - txBytePtr ) > 0 )
						{
							initiateDownloadSegment( nodeID, toggleBit, ref newValue, ref txBytePtr );
							toggleBit = !toggleBit;
						}
						else // no - then finished download succesfully.
						{
							fbc = DIFeedbackCode.DISuccess;
						}
						break;
					}

						// response from the controller to the last message is an abort message
					case SDOMessageTypes.AbortResponse:
					{
						long abortCode = 0;

						// extract the abort code from the message (4 bytes) back into a 32 bit no.
						for( int i = 0; i < 4; i++ )
						{
							abortCode <<= 8;
                            abortCode += VCI.rxMsg.data[7 - i];
						}

						// recast as enum type to give user a clue what number actually means.
						fbc = (DIFeedbackCode)abortCode;
						break;
					}

						/* No response (timed out), unknown response type or invalid in the 
						 * context of sending a download request SDO message.
						 */
					case SDOMessageTypes.NoResponse:
					case SDOMessageTypes.UnknownResponseType:
					case SDOMessageTypes.InvalidResponse:
					default:
					{
						fbc = DIFeedbackCode.DINoResponseFromController;
						break;
					}
				} // end of switch statement
			}
				// quit loop on a failure or finished SDO message sequence (DISuccess).
			while ( fbc == DIFeedbackCode.DIGeneralFailure );

			return ( fbc );
		}
			
		//--------------------------------------------------------------------------
		//  Name			: SDOWriteBlock()
		//  Description     : This function takes the newValue byte array, builds up an SDO message
		//				      to send to the node of nodeID which then writes this value
		//					  to the OD item in the controller's OD of index and subIndex.
		//					  This uses the block download protocol to speed up the firmware
		//					  programming sequence.  It specifically does NOT wait for
		//					  responses to each segment to increase the throughput of data.
		//  Parameters      : nodeID - node ID of the controller on the network who's OD
		//							   needs updating
		//					  index - index of the OD item to be updated
		//					  subIndex - sub index of the OD item to be updated
		//					  newValue - new value byte array to be written to the controller OD item
		//					  commsTimeout - communication timer timeout to be used with
		//						this object before a no response is declared
		//  Used Variables  : None
		//  Preconditions   : The GUI has gathered the nodeID, index, subIndex of the OD item
		//					  to change and the newValue which has already been range checked.
		//  Post-conditions : The item of index and subIndex in the controller of ID nodeID
		//					  has had it's value updated to newValue (unless failed).
		//  Return value    : fbc indicating success or a failure reason
		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an SDO to write a new byte array value to the required OD item in the nodeID device
		///using SDO download block transfers. Intended for longer array sizes to speed up the communications time.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs updating</param>
		/// <param name="index">index of the OD item to be updated</param>
		/// <param name="subIndex">sub index of the OD item to be updated</param>
		/// <param name="newValue">new value to be written to the controller OD item</param>
		/// <param name="commsTimeout">communication timer timeout to be used with
		/// this object before a no response is declared</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode SDOBlockWrite( int nodeID, int index, int subIndex, ref byte[] newValue, int commsTimeout )
		{
			#region local variable declarations and initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			byte [][] blocks = new byte[1][];		// keep compiler happy - will be resized later

			/* Used to keep track of how many bytes have already been transmitted if the SDO
			 * write requires multiple SDO segments to download the new value.
			 */
			int txBytePtr = 0;			
			int sequenceNumber;				// segment sequence number within this block
			byte maxDeviceBlocks = 0;

			rxTimeout = commsTimeout;	// set the communication timer to specific timeout required for this object
			#endregion

			#region if no data then it is not valid to perform a block transfer so return
			if ( newValue.Length <= 0 )
			{
				return ( fbc );
			}
			#endregion

			#region Virtual Nodes
			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				return ( DIFeedbackCode.DISuccess );
            }
            #endregion Virtual Nodes

            /* Start sequence by building & sending an initiate SDO block download request. 
			 * We need to find out the max number of segments in a block that the device can handle.
			 */
			fbc = initiateBlockDownloadRequest( nodeID, index, subIndex, newValue.Length, out maxDeviceBlocks );

			if ( fbc == DIFeedbackCode.DISuccess )
			{
				if ( newValue.Length > ( 7 * maxDeviceBlocks ) )
				{ 
					fbc = DIFeedbackCode.DISDOBlockTooLarge;
				}
				else
				{
					#region initialise sequence and transmit byte pointer ready or segment block transfer
					sequenceNumber = 1;		// set sequence number back to 1 at the start of every new block tranmission
					txBytePtr = 0;			// reset back to zero ready to track transmissions
                    VCI.BlockSgmentResponses = 0;
					#endregion
					#region calculate the number of segments there will be in this block
					noOfBlockSegments = newValue.Length / 7;

					if ( ( newValue.Length % 7 ) > 0 )
					{
                        noOfBlockSegments++;
					}
					#endregion
					#region build and send each of the SDO block segments
					do
					{
						// deliberately ignore responses from controller during a block transfer to speed it up
						initiateBlockDownloadSegment( nodeID, ref sequenceNumber, ref newValue, ref txBytePtr );
						sequenceNumber++;
					}
                    while (sequenceNumber <= this.noOfBlockSegments);
					#endregion					
					#region finish the block download and check if successful
                    
                    resetTimer();
                    fbc = this.awaitBlockSegmentResponses();
                    if (fbc == DIFeedbackCode.DISuccess)
                    {
                        fbc = endBlockDownloadRequest(nodeID, ref newValue, ref txBytePtr);
                    }
					#endregion
				}
			}

			return ( fbc );
		}


		#endregion

		#region SDORead
		//--------------------------------------------------------------------------
        //  Name			: checkIfNodeConnected()
		//  Description     : This function is used to build up only ERROR REGISTER
		//					  messages then transmits it with/without waiting for a valid
		//					  reply from the controller.
		//  Parameters      : nodeID - node ID of the controller on the network who's OD
		//							   needs read from
		//					  rxData - array passed by reference to effectively return back
		//							  the raw data bytes which have been read from the 
		//							  controller 
        //                    waitForResponse - flag to determine whether to await response
		//  Used Variables  : None
		//  Preconditions   : Only used by DW to build up error register requests so
		//					  the connected system is trying to be found.
		//  Post-conditions : An error register SDO message has been transmitted but
		//					  has not waited for a valid reply or a timeout condition
		//					  (used for speed when detecting a system).
		//  Return value    : fbc indicates success or give a failure reason
		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an SDO to read the error register object in the nodeID device.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs read from</param>
		/// <param name="type">Sevcon message type object of the item to be read (used to work out the 
		/// index and sub of the item to be read but is independent of items moving as it follows the EDS)</param>
		/// <param name="rxData">array passed by reference to effectively return back the raw data bytes 
		/// which have been read from the controller</param>
		/// <param name="commsTimeout">communication timer timeout to be used with this object 
		/// before a no response is declared</param>
		/// <param name="waitForResponse">whether to wait for a response from the device or return
		/// immediately</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
        /// DR38000239 JW method name changed for clarity and simplified
        public DIFeedbackCode checkIfNodeConnected(int nodeID, ref byte[] rxData, bool waitForResponse)
		{
			// flag to indicate to another function whether DW needs to wait for a valid reply or not
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;

			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				return ( DIFeedbackCode.DISuccess );
			}
            // set the communication timer to default
            rxTimeout = SCCorpStyle.TimeoutDefault;

            initiateUploadRequest( nodeID, 0x1001, 0x00, waitForResponse );

            if ( waitForResponse == true )
			{
                if(VCI.rxResponseType  == SDOMessageTypes.NoResponse)
				{
					fbc = DIFeedbackCode.DINoResponseFromController;
				}
				else
				{
					// any kind of response tells us that the node is physically out there
					//Filtering for the correct Index and Sub is carried out in VCI2/3
					fbc = DIFeedbackCode.DISuccess;
				}
			}
			else
			{
				fbc = DIFeedbackCode.DISuccess;
			}
			return ( fbc );
		}

		//--------------------------------------------------------------------------
		//  Name			: SDORead()
		//  Description     : Builds up and sends a CAN SDO message to default SDO COBID
		//					  for node of nodeID to request the current value of OD item
		//					  of index and sub index given.
		//  Parameters      : nodeID - node ID of the controller on the network who's OD
		//							   needs read from
		//					  index - index of the OD item to be read
		//					  subIndex - sub index of the OD item to be read
		//					  dataSize - size in bytes of the expected value to be read
		//								 from the controller (no. of bytes to receive)
		//					  rxData - array of bytes to hold the raw value read from the
		//							   controller, prior to formatting into the expected
		//							   data type for updating the DW's replica of the OD.
		//					  commsTimeout - communication timer timeout to be used with
		//						this object before a no response is declared
		//  Used Variables  : None
		//  Preconditions   : The GUI has selected a valid nodeID and index & sub of
		//					  the item it needs to know the current value of from the
		//					  controller.
		//  Post-conditions : The rxData byte array contains the raw data bytes received from
		//					  the controller representing the current value of the item of 
		//					  the given index and subIndex.
		//  Return value    : fbc indicates success or gives a failure reason.
		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an SDO to request the current value of OD item of index and subIndex in device nodeID.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs read from</param>
		/// <param name="index">index of the OD item to be read</param>
		/// <param name="subIndex">sub index of the OD item to be read</param>
		/// <param name="dataSize">size in bytes of the expected value to be read from the 
		/// controller (no. of bytes to receive)</param>
		/// <param name="rxData">array of bytes to hold the raw value read from the controller, prior to
		/// formatting into the expected data type for updating the DW's replica of the OD.</param>
		/// <param name="commsTimeout">communication timer timeout to be used with this object 
		/// before a no response is declared</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
        public DIFeedbackCode SDORead(int nodeID, int index, int subIndex, uint dataSize, ref byte[] rxData, int commsTimeout)
		{
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				return ( DIFeedbackCode.DISuccess );
			}

			/* Flag to indicate that when we transmit an SDO, we do want to wait until there is a valid
			 * reply or the comms timer has timed out.
			 */
			bool waitForResponse = true;

			/* When an SDO write requires multiple segments to download the new value, each
			 * subsequent SDO segment download in the sequence must have the toggle bit toggled.
			 * This is so the controller who is receiving this can detect if SDOs are out
			 * of sequence or have gone missing etc.
			 */
			bool toggleBit = false;

			// this SDO packet receive data size
			uint rxDataSize = 0;

			// this expedited SDO packet receive data byte pointer
			int rxDataPtr = 0;

			// initialise SDO read for domain receive data (also indicates for progress bar)
			_SDOReadDomainRxDataSize = 0;
			_SDOReadDomainRxDataPtr = 0;
			// set the communication timer to specific timeout required for this object
			rxTimeout = commsTimeout;

			// Start sequence by building & sending an initiate SDO upload request
			initiateUploadRequest( nodeID, index, subIndex, waitForResponse );

			/* Until the entire SDO sequence to read the latest value from the controller,
			 * keep waiting for a reply from the last SDO message sent.  Then if a new
			 * SDO is required, build the next SDO message and send it.
			 */
			do
			{
				// what kind of SDO message response did we get?
                switch (VCI.rxResponseType)
				{
						#region initiate upload response expedited 
						// response from the controller to the initiate upload SDO just sent
					case SDOMessageTypes.InitiateUploadResponseExpedited:
					{
						/* If number of bytes not data in this expedited SDO is indicated then
						 * calculate the length of the valid data in this message.
						 * Expedited SDOs can contain up to a max of 4 bytes data after
						 * extracting the header type information.
						 */
                        if ((VCI.rxMsg.data[0] & 0x03) == 0x03)
                        {

                            rxDataSize = (uint)(4 - ((VCI.rxMsg.data[0] & 0x0c) >> 2));
                        }
							// Max data size in expedited SDO is 4.
						else
						{
							rxDataSize = dataSize;

							if ( dataSize > 4 )
							{
								rxDataSize = 4;
							}
						}

						// resize array now true length known
						rxData = new byte[ rxDataSize ];
						
						/* Copy the number of valid data bytes from this message into rxData array.
						 * Update rxData Ptr for piecing in following messages.
						 */
						for( int i = 0; i < rxDataSize; i++ )
						{
							if ( rxDataPtr < dataSize )
							{
                                rxData[rxDataPtr++] = VCI.rxMsg.data[i + 4];
							}
						}

						// Finished so indicate success to quit the loop.
						fbc = DIFeedbackCode.DISuccess;
						break;
					}
						#endregion

						#region initiate upload response
						// response from the controller is an initiate upload response message
					case SDOMessageTypes.InitiateUploadResponse:
					{
						// Extract from the message the expected no. of bytes to be received.
						for( int i = 0; i < 4; i++ )
						{
							rxDataSize <<= 8;
                            rxDataSize += VCI.rxMsg.data[7 - i];
						}

						_SDOReadDomainRxDataSize = rxDataSize;
						/* Now we know how many data bytes to expect, resize the rxData array.
						 * Then send the first initiate segment request to get first real data.
						 */
						try
						{
							rxData = new byte[ _SDOReadDomainRxDataSize ];
							dataSize = _SDOReadDomainRxDataSize;
							initiateUploadSegment( nodeID, toggleBit );
							toggleBit = !toggleBit;
						}
						catch ( Exception )
						{
							fbc = DIFeedbackCode.DIOutOfODMemory;
						}

						break;
					}
						#endregion

						#region upload segment response
						// controller response was an upload segment response
					case SDOMessageTypes.UploadSegmentResponse:
					{
						/* Extract how many bytes in this message are not valid data.
						 * An SDO upload segment response can hold a maximum of 7 data
						 * bytes so extract from 7 to get number of valid bytes we
						 * have to extract from this message.  If 0, then 7 bytes are valid.
						 */
                        if ((VCI.rxMsg.data[0] & 0x0e) == 0x00)
						{
							rxDataSize = 7;
						}
						else
						{
                            rxDataSize = (uint)(7 - ((VCI.rxMsg.data[0] & 0x0e) >> 1));
						}

						/* Copy over the next load of data bytes from the SDO into the
						 * next contingous slot of the rxData array.  Update rxDataPtr
						 * ready for subsequent messages.
						 */
						for( int i = 0; i < rxDataSize; i++ )
						{
							if ( _SDOReadDomainRxDataPtr < dataSize )
							{
                                rxData[_SDOReadDomainRxDataPtr++] = VCI.rxMsg.data[i + 1];
							}
								/* 
								 * overrun so allocate more memory & copy existing array over.
								 * Not very efficient but shouldn't happen if size indicated
								 * correctly in initiate upload response.
								 */
							else
							{
								try
								{
									dataSize = (uint) (rxData.Length + rxDataSize - i);
									_SDOReadDomainRxDataSize = dataSize;
									byte [] newArray = new byte[ dataSize ];
									rxData.CopyTo( newArray, 0 );
									rxData = newArray;
								}
								catch ( Exception )
								{
									fbc = DIFeedbackCode.DIOutOfODMemory;
									break;
								}
							}
						}

						// No more segments to come? Finished! Indicate success to quit loop.
                        if ((VCI.rxMsg.data[0] & 0x01) == 0x01)
						{
							fbc = DIFeedbackCode.DISuccess;
						}
							// Not finished so build up & transmit the next upload segment request.
						else
						{
							initiateUploadSegment( nodeID, toggleBit );
							toggleBit = !toggleBit;
						}
						break;
					}
						#endregion

						#region abort response
						// response from the controller to the last message is an abort message
					case SDOMessageTypes.AbortResponse:
					{
						long abortCode = 0;

						// extract the abort code from the message (4 bytes) back into a 32 bit no.
						for( int i = 0; i < 4; i++ )
						{
							abortCode <<= 8;
                            abortCode += VCI.rxMsg.data[7 - i];
						}

						// recast as enum type to give user a clue what number actually means.
						fbc = (DIFeedbackCode)abortCode;
						break;
					}
						#endregion

						#region no response, unknown response or invalid response
						/* No response (timed out), unknown response type or invalid in the 
						 * context of sending a upload request SDO message.
						 */
					case SDOMessageTypes.NoResponse:
					case SDOMessageTypes.UnknownResponseType:
					case SDOMessageTypes.InvalidResponse:
					default:
					{
						fbc = DIFeedbackCode.DINoResponseFromController;
						break;
					}
						#endregion
				} // end of switch statement
			}
				// quit loop on a failure or finished SDO message sequence (DISuccess).
			while ( fbc == DIFeedbackCode.DIGeneralFailure );

			return ( fbc );
		}		
		

		#region block upload - untested code not currently called
		public DIFeedbackCode SDOBlockRead( int nodeID, int index, int subIndex, ref byte[] rxData, int commsTimeout )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			int blockSize = 0;
			byte sequenceNumber = 1;
			bool moreBlocksExpected = true;
			int maxBlockSize = 889;
			int rxBytePtr = 0;

			rxTimeout = commsTimeout;	// set the communication timer to specific timeout required for this object

			#region GUI mockup stuff
			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				return ( DIFeedbackCode.DISuccess );
			}
			#endregion

			// initiate block upload by sending a request
			fbc = initiateBlockUploadRequest( nodeID, index, subIndex, out blockSize );

			if ( fbc == DIFeedbackCode.DISuccess )
			{
				#region size rxData for expected data length to be received in block upload
				if ( blockSize > 0 )
				{
					rxData = new byte[ blockSize ];
				}
				else
				{
					rxData = new byte[ maxBlockSize ];
				}
				#endregion

				// request the first segment upload
				requestBlockUploadSegment( nodeID );

				#region for as long as more segments in the block are expected, upload the last segment and confirm it (once checked)
				do
				{
					fbc = blockUploadSegment( nodeID, sequenceNumber, ref rxData, ref rxBytePtr, out moreBlocksExpected );

					if ( fbc == DIFeedbackCode.DISuccess )
					{
						sequenceNumber++;		// update to next segment expected
					}
					else
					{
						break;					// something went wrong so abort task
					}
				}
				while ( moreBlocksExpected == true );
				#endregion

				#region get the end of block upload and perform checks against data received
				if ( fbc == DIFeedbackCode.DISuccess )
				{
					fbc = endBlockUploadRequest( nodeID, blockSize, ref rxData, rxBytePtr );
				}
				#endregion
			}

			return ( fbc );
		}

		#endregion

		#endregion
		
		#region LSS
		//--------------------------------------------------------------------------
		//  Name			: LSSReadSwitchModeGlobal()
		//  Description     : Sends an LSS message to the single attached device to
		//					  either switch the device into LSS mode or back into
		//					  normal mode.
		//  Parameters      : LSSConfigMode - true if want to put device into LSS mode,
		//						false if we want to put it back into normal mode
		//  Used Variables  : hardwareOpened - indicates that the IXXAT adapter was
		//						opened and initialised, OK for use
		//					  VCI.messageType - set to the message type to be listened
		//						in to by the VCI object
		//					  VCI object - object to interface with the IXXAT hardware
		//  Preconditions   : The IXXAT adapter must have been initialised and running,
		//					  the user has been told to connect only one device to the CAN.
		//  Post-conditions : The connected device has been commanded to enter LSS config
		//					  mode or leave it, dependent on the LSSConfigMode parameter.
		//  Return value    : fbc indicates success or gives a failure reason.
		//--------------------------------------------------------------------------
		///<summary>Transmits an SDO message to command the single connected node to enter or leave LSS config mode.</summary>
		/// <param name="LSSConfigMode">true if want to put device into LSS mode, 
		/// false if we want to put it back into normal mode</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode LSSReadSwitchModeGlobal( bool LSSConfigMode )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DITransmitCommsFailure;
			byte	length = 8;				// length of data within the LSS
			uint	id = SCCorpStyle.LSSCOBID;			// COB-ID of the LSS
			byte [] data = new byte[ 8 ];	// data byte to be transmitted
			#endregion
			
			// set the receive interrupt to detect LSS messages
            VCI.messageType = CANMessageType.LSS;
			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
			if ( VCI.CANAdapterHWIntialised == true )
			{
				/* DS306 Switch Mode Global data bytes are
				 *		byte 0		0x04
				 *		byte 1		0x01 for configure mode and 0x00 for normal mode
				 *		bytes 2-7	0x00 (reserved)
				 */
				data[0] = 0x04;

				if ( LSSConfigMode == true )
				{
					data[1] = 0x01;		// put into LSS configuration mode
				}
				else
				{
					data[1] = 0x00;		// put back into normal mode
				}

				// pad the rest of the data bytes with zero
				for ( int db = 2; db < length; db++ )
				{
					data[ db ] = 0x00;
				}

				// transmit our newly build LSS message to the device
				if ( VCI.transmit( id, length, data ) == VCIFeedbackCode.VCI_OK )
				{
					fbc = DIFeedbackCode.DISuccess;
				}

				// Don't wait for a response as this is an unacknowledged service.
			
			} // end of if hardware is open

			// set the receive interrupt to detect SDO messages (default)
            VCI.messageType = CANMessageType.SDO;
			return ( fbc );
		}

		//--------------------------------------------------------------------------
		//  Name			: LSSInquireNodeID()
		//  Description     : Requests the node ID of the LSS node device attached.
		//  Parameters      : nodeID - out parameter containing the node ID of the LSS
		//						device, if it was retrieved OK
		//  Used Variables  : hardwareOpened - indicates that the IXXAT adapter was
		//						opened and initialised, OK for use
		//					  VCI.messageType - set to the message type to be listened
		//						in to by the VCI object
		//					  VCI object - object to interface with the IXXAT hardware
		//					  VCI.rxResponseType - reply message type, received in the
		//						VCI receive interrupt in response to the last DI request
		//  Preconditions   : There is a single device attached via the CAN which is
		//					  in LSS mode.
		//  Post-conditions : nodeID contains the node ID of the LSS device if it
		//					  was retrieved from the device OK.
		//  Return value    : fbc indicates success or gives a failure reason.
		//--------------------------------------------------------------------------
		///<summary>Requests the node ID of the LSS device attached to the CAN.</summary>
		/// <param name="nodeID">out parameter containing the node ID of the LSS device, if
		/// it was retrieved OK</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode LSSInquireNodeID(out ushort nodeID )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			byte	length = 8;				// length of data within the LSS
			uint	id = SCCorpStyle.LSSCOBID;			// COB-ID of the LSS
			byte [] data = new byte[ 8 ];	// data byte to be transmitted

			nodeID = 0;				// assume node ID of zero until device replies
			#endregion

			// set the receive interrupt to detect LSS messages
            VCI.messageType = CANMessageType.LSS;
			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				/* DS306 Inquire Node ID data bytes are
				 *		byte 0		0x5e
				 *		bytes 1-7	0x00 (reserved)
				 */
				data[0] = 0x5e;

				// pad remaining bytes with zero
				for ( int db = 1; db < length; db++ )
				{
					data[ db ] = 0x00;
				}

				// transmit our newly build LSS message to the device
				VCI.transmit( id, length, data );

				// Wait for a response.
				resetTimer();
				awaitResponse();

				#region switch statement to analyse the response
                switch (VCI.rxResponseType)
				{
						#region LSS response - check function code and extract the node ID from data
					case SDOMessageTypes.LSSResponse:
					{
                        if (VCI.rxMsg.data[0] == 0x5e)
						{
                            nodeID = VCI.rxMsg.data[1];
                            fbc = DIFeedbackCode.DISuccess;
						}
						else
						{
							fbc = DIFeedbackCode.DIInvalidResponse;
						}
						break;
					}
						#endregion

						#region no response - setup feeback code
					case SDOMessageTypes.NoResponse:
					{
						fbc = DIFeedbackCode.DINoResponseFromController;
						break;
					}
						#endregion

						#region invalid response and default - setup feeback code
					case SDOMessageTypes.InvalidResponse:
					default:
					{
						fbc = DIFeedbackCode.DIInvalidResponse;
						break;
					}
						#endregion
				}
				#endregion
					
			} // end of if hardware is open

			// set the receive interrupt to detect SDO messages (default)
            VCI.messageType = CANMessageType.SDO;
			return ( fbc );
		}
		
		//--------------------------------------------------------------------------
		//  Name			: LSSConfigureNodeID()
		//  Description     : Sets the node ID of the LSS device attached to
		//					  the value passed as a parameter.
		//  Parameters      : requestedNodeID - node ID the LSS node is required to be
		//  Used Variables  : hardwareOpened - indicates that the IXXAT adapter was
		//						opened and initialised, OK for use
		//					  VCI.messageType - set to the message type to be listened
		//						in to by the VCI object
		//					  VCI object - object to interface with the IXXAT hardware
		//					  VCI.rxResponseType - reply message type, received in the
		//						VCI receive interrupt in response to the last DI request
		//  Preconditions   : There is a single device attached via the CAN which is
		//					  in LSS mode.
		//  Post-conditions : The LSS device node ID has been changed to requestedNodeID
		//					  if successful.
		//  Return value    : fbc indicates success or gives a failure reason.
		//--------------------------------------------------------------------------
		///<summary>Sets the node ID of the attached LSS device to requestedNodeID.</summary>
		/// <param name="requestedNodeID">node ID the LSS node is required to be</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode LSSConfigureNodeID( ushort requestedNodeID )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			byte	length = 8;				// length of data within the LSS
			uint	id = SCCorpStyle.LSSCOBID;			// COB-ID of the LSS
			byte [] data = new byte[ 8 ];	// data byte to be transmitted
			#endregion
			
			// set the receive interrupt to detect LSS messages
            VCI.messageType = CANMessageType.LSS;
			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				/* DS306 Configure Node ID data bytes are
				 *		byte 0		0x11
				 *		byte 1		node ID
				 *		bytes 2-7	0x00 (reserved)
				 */
				data[0] = 0x11;
				data[1] = (byte)requestedNodeID;

				// pad remaining data bytes with zero
				for ( int db = 2; db < length; db++ )
				{
					data[ db ] = 0x00;
				}

				// transmit our newly build LSS to device
				VCI.transmit( id, length, data );

				// Wait for a response.
				resetTimer();
				awaitResponse();

				#region switch statement to analyse the response
                switch (VCI.rxResponseType)
				{
						#region LSS response - check function code and db0 & db1 as expected
					case SDOMessageTypes.LSSResponse:
					{
                        if (VCI.rxMsg.data[0] == 0x11)
                        {
                            if (VCI.rxMsg.data[1] == 0)
                            {
								fbc = DIFeedbackCode.DISuccess;
							}
							else
							{
								fbc = DIFeedbackCode.DILSSNodeIDSetErrorCode;
							}
						}
						else
						{
							fbc = DIFeedbackCode.DIInvalidResponse;
						}

						break;
					}
						#endregion

						#region no response - setup feeback code
					case SDOMessageTypes.NoResponse:
					{
						fbc = DIFeedbackCode.DINoResponseFromController;
						break;
					}
						#endregion

						#region invalid response and default - setup feeback code
					case SDOMessageTypes.InvalidResponse:
					default:
					{
						fbc = DIFeedbackCode.DIInvalidResponse;
						break;
					}
						#endregion
				}
				#endregion
					
			} // end of if hardware is open

			// set the receive interrupt to detect SDO messages (default)
            VCI.messageType = CANMessageType.SDO;
			return ( fbc );
		}

		//--------------------------------------------------------------------------
		//  Name			: LSSConfigureBitTimingParameters()
		//  Description     : Configures the baud rate of the LSS device attached to the
		//					  value indicated by the bitTimingIndex.
		//  Parameters      : bitTimingIndex - index from CAN standard tables to indicate
		//						which baud rate is being requested
		//  Used Variables  : hardwareOpened - indicates that the IXXAT adapter was
		//						opened and initialised, OK for use
		//					  VCI.messageType - set to the message type to be listened
		//						in to by the VCI object
		//					  VCI object - object to interface with the IXXAT hardware
		//					  VCI.rxResponseType - reply message type, received in the
		//						VCI receive interrupt in response to the last DI request
		//  Preconditions   : There is a single device attached via the CAN which is
		//					  in LSS mode.
		//  Post-conditions : The LSS device's baud rate has been changed to the rate
		//					  indicated by bitTimingIndex if successful.
		//  Return value    : fbc indicates success or gives a failure reason.
		//--------------------------------------------------------------------------
		///<summary>Sets the baud rate of the attached LSS device to that represented by bitTimingIndex.</summary>
		/// <param name="bitTimingIndex">index from CAN standard tables to indicate 
		/// which baud rate is being requested</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode LSSConfigureBitTimingParameters( int bitTimingIndex )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			byte	length = 8;				// length of data within the LSS
			uint	id = SCCorpStyle.LSSCOBID;			// COB-ID of the LSS
			byte [] data = new byte[ 8 ];	// data byte to be transmitted
			#endregion
			
			// set the receive interrupt to detect LSS messages
            VCI.messageType = CANMessageType.LSS;

			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				/* DS306 Configure Bit Timing Parameter data bytes are
				 *		byte 0		0x13
				 *		byte 1		0x00 (standard baudrate table)
				 *		byte 2		0=1M through to 8=10K
				 *		bytes 3-7	0x00 (reserved)
				 */
				data[0] = 0x13;
				data[1] = 0x00;
				data[2] = (byte)bitTimingIndex;

				// pad remaining data bytes with zero
				for ( int db = 3; db < length; db++ )
				{
					data[ db ] = 0x00;
				}

				// transmit our newly build LSS message to the device
				VCI.transmit( id, length, data );

				// Wait for a response.
				resetTimer();
				awaitResponse();

				#region switch statement to analyse the response
                switch (VCI.rxResponseType)
				{
						#region LSS response - anaylse function code and response byte
					case SDOMessageTypes.LSSResponse:
					{
						// expected function code?
                        if (VCI.rxMsg.data[0] == 0x13)	
						{
							#region accepted or rejected new baud rate?
                            switch (VCI.rxMsg.data[1])
							{
									#region LSS device accepted new baud rate
								case 0:
								{
									fbc = DIFeedbackCode.DISuccess;
									break;
								}
									#endregion
									#region LSS device rejected new baud rate
								case 1:
								{
									fbc = DIFeedbackCode.DILSSInadmissableBaudRate;
									break;
								}
									#endregion
									#region unexpected response
								default:
								{
									fbc = DIFeedbackCode.DILSSBaudSetErrorCode;
									break;
								}
									#endregion
							}
							#endregion
						}
						else
						{
							fbc = DIFeedbackCode.DIInvalidResponse;
						}
						break;
					}
						#endregion

						#region no response - setup feeback code
					case SDOMessageTypes.NoResponse:
					{
						fbc = DIFeedbackCode.DINoResponseFromController;
						break;
					}
						#endregion

						#region invalid response and default - setup feeback code
					case SDOMessageTypes.InvalidResponse:
					default:
					{
						fbc = DIFeedbackCode.DIInvalidResponse;
						break;
					}
						#endregion
				}
				#endregion
					
			} // end of if hardware is open

			// set the receive interrupt to detect SDO messages (default)
            VCI.messageType = CANMessageType.SDO;
			return ( fbc );
		}

		
		//--------------------------------------------------------------------------
		//  Name			: LSSStoreConfiguration()
		//  Description     : Requests the attached LSS device to store it's latest
		//					  configuration to memory (ie usually node ID and baud rate).
		//  Parameters      : None
		//  Used Variables  : hardwareOpened - indicates that the IXXAT adapter was
		//						opened and initialised, OK for use
		//					  VCI.messageType - set to the message type to be listened
		//						in to by the VCI object
		//					  VCI object - object to interface with the IXXAT hardware
		//					  VCI.rxResponseType - reply message type, received in the
		//						VCI receive interrupt in response to the last DI request
		//  Preconditions   : There is a single device attached via the CAN which is
		//					  in LSS mode. The device's baud rate and node ID has been 
		//					  changed by the DI.
		//  Postconditions	: The LSS device saves the new settings to memory or indicates
		//					  a failure reason.
		//  Return value    : fbc indicates success or gives a failure reason.
		//--------------------------------------------------------------------------
		///<summary>Requests the attached LSS device to save it's new settings to memory.</summary>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode LSSStoreConfiguration()
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			byte	length = 8;				// length of data within the LSS
			uint	id = SCCorpStyle.LSSCOBID;			// COB-ID of the LSS
			byte [] data = new byte[ 8 ];	// data byte to be transmitted
			#endregion
            VCI.messageType = CANMessageType.LSS;

			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				/* DS306 Store Configuration data bytes are
				 *		byte 0		0x17
				 *		bytes 1-7	0x00 (reserved)
				 */
				data[0] = 0x17;

				// pad remaining data bytes with zero
				for ( int db = 1; db < length; db++ )
				{
					data[ db ] = 0x00;
				}

				// transmit our newly build LSS message to the device
				VCI.transmit( id, length, data );

				// Wait for a response.
				resetTimer();
				awaitResponse();

				#region switch statement to analyse the response
                switch (VCI.rxResponseType)
                {
                    #region LSS response - anaylse function code and response byte
                    case SDOMessageTypes.LSSResponse:
                        {
                            // if expected function code then analyse associated data byte
                            if (VCI.rxMsg.data[0] == 0x17)
                            {
                                #region analyse the data byte associated
                                switch (VCI.rxMsg.data[1])
                                {
                                    #region success
                                    case 0:
                                        {
                                            fbc = DIFeedbackCode.DISuccess;
                                            break;
                                        }
                                    #endregion
                                    #region device unable to save new settings
                                    case 1:
                                        {
                                            fbc = DIFeedbackCode.DILSSDeviceUnableToSave;
                                            break;
                                        }
                                    #endregion
                                    #region device unable to access it's storage to save new settings
                                    case 2:
                                        {
                                            fbc = DIFeedbackCode.DILSSDeviceUnableToAccessStorage;
                                            break;
                                        }
                                    #endregion
                                    #region unknown reply data byte => error code
                                    default:
                                        {
                                            fbc = DIFeedbackCode.DILSSSaveErrorCode;
                                            break;
                                        }
                                    #endregion
                                }
                                    #endregion
                            }
                            // else unexpected function code so must be invalid response
                            else
                            {
                                fbc = DIFeedbackCode.DIInvalidResponse;
                            }
                            break;
                        }
                    #endregion

                    #region no response - setup feeback code
                    case SDOMessageTypes.NoResponse:
                        {
                            fbc = DIFeedbackCode.DINoResponseFromController;
                            break;
                        }
                    #endregion

                    #region invalid response and default - setup feeback code
                    case SDOMessageTypes.InvalidResponse:
                    default:
                        {
                            fbc = DIFeedbackCode.DIInvalidResponse;
                            break;
                        }
                    #endregion
                }
				#endregion
					
			} // end of if hardware is open

			// set the receive interrupt to detect SDO messages (default)
            VCI.messageType = CANMessageType.SDO;
			return ( fbc );
		}
		#endregion

		#region PDO monitoring
		//--------------------------------------------------------------------------
		//  Name			: startPDOMonitoring()
		//  Description     : Initialises the VCI object ready to receive and analyse
		//					  PDOs in order to graph their data trends.
		//  Parameters      : list - list of OD items to acquire PDO data for. Selected
		//						by the user and calculations augment with which PDO
		//						each is in and how to extract the value from it.
		//  Used Variables  : VCI object - object to interface with the IXXAT hardware
		//  Preconditions   : list has been selected by the user and the DI has already
		//					  build/checked these are all PDO mapped and calculated how
		//					  to extract the data from the PDOs.
		//  Post-conditions : VCI object has been initialised, ready for monitoring
		//					  PDOs.
		//  Return value    : fbc indicates success or gives a failure reason.
		//--------------------------------------------------------------------------
		///<summary>Initialises the VCI object and IXXAT adapter for aquiring and analysing data in PDOs.</summary>
		/// <param name="list">list of OD items to acquire PDO data for. Selected by the user and 
		/// calculations augment with which PDO each is in and how to extract the value from it</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public void startPDOMonitoring( )
		{
            foreach (ODItemAndNode item in VCI.OdItemsBeingMonitored)
			{
				item.ODparam.lastPlottedPtIndex = 0;
				item.ODparam.measuredDataPoints = new ArrayList();
				item.ODparam.screendataPoints = new ArrayList();
			}

            // we want VCI receive interrupt to analyse PDOs
            VCI.messageType = CANMessageType.PDO;

			// monitoring is not paused; we want to acquire PDO data to extract and add to VCI.monitoringData
			VCI.monitoringPaused = false;
		}

		//--------------------------------------------------------------------------
		//  Name			: pausePDOMonitoring()
		//  Description     : Causes the VCI object to stop analysing received PDOs
		//					  and no longer extracts the relevant list data values
		//					  and no longer adds this to the VCI.monitorData array.
		//  Parameters      : None
		//  Used Variables  : VCI object - object to interface with the IXXAT hardware
		//  Preconditions   : The system has been found and the GUI is doing graphical
		//					  monitoring (DI collates PDOs). Now the user has selected
		//					  to stop graphing.
		//  Post-conditions : The VCI stops analysing received PDOs and doesn't add 
		//					  any new data to VCI.monitorData.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Stops the VCI object from analysing PDOs and no longer adds data to VCI.monitorData.</summary>
		public void pausePDOMonitoring()
		{
			// no longer want to analyse PDOs to add data to VCI.monitorData
			VCI.monitoringPaused = true;

            // set the receive interrupt to detect SDO messages (default)
            VCI.messageType = CANMessageType.SDO;
		}

		//--------------------------------------------------------------------------
		//  Name			: restartPDOMonitoring()
		//  Description     : Restarts the VCI object analysing PDOs which now adds 
		//					  data to VCI.monitorData.
		//  Parameters      : None
		//  Used Variables  : VCI object - object to interface with the IXXAT hardware
		//  Preconditions   : The system has been found and the GUI is doing graphical
		//					  monitoring (DI collates PDOs). The user previously 
		//					  selected to stop graphing and has now selected to restart.
		//  Post-conditions : The VCI restarts analysing received PDOs and adds 
		//					  any new data to VCI.monitorData.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Restarts the VCI object analysing PDOs which now adds data to VCI.monitorData.</summary>
        public void restartPDOMonitoring()
        {
            // set the receive interrupt to detect PDO messages
            VCI.messageType = CANMessageType.PDO;
            // monitoring is not paused; we want to acquire PDO data to extract and add to VCI.monitoringData
            VCI.monitoringPaused = false;
        }

		#endregion

		#region private functions
		//--------------------------------------------------------------------------
		//  Name			: initiateUploadRequest()
		//  Description     : This function builds up the required initiate upload 
		//					  request SDO and transmits it.  It waits for a reply
		//					  or timeout if required.
		//  Parameters      : nodeID - node ID of the controller on the network who's OD
		//							   needs reading
		//					  index - index of the OD item to be read
		//					  subIndex - sub index of the OD item to be read
		//					  waitForResponse - flag to indicate whether this process should
		//								wait for a reply from the controller (or timeout)
		//								or whether to return immediately
		//  Used Variables  : hardwareOpened - flag indicates if IXXAT dongle is initialised
		//									   and running
		//					  DefaultSDOBaseAddress - constant used to calculate the COB-ID
		//										required for the SDO to talk to node of nodeID
		//  Preconditions   : The system has been found & the EDS read so that the nodeID, index
		//					  and sub have all been checked to be valid.
		//  Post-conditions : The initiate upload request SDO has been built & transmitted
		//					  according to the parameters passed.  A reply has been waited
		//					  for or not dependent on waitForResponse flag setting.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an initiate SDO upload with optional wait for a response.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs reading</param>
		/// <param name="index">index of the OD item to be read</param>
		/// <param name="subIndex">sub index of the OD item to be read</param>
		/// <param name="waitForResponse">flag to indicate whether this process should wait for
		/// a reply from the controller (or timeout) or whether to return immediately</param>
		private void initiateUploadRequest( int nodeID, int index, int subIndex, bool waitForResponse )
		{
			#region local variable declarations and initialisation
			byte	length;					// length of data within the SDO
			uint	id;						// COB-ID of the SDO
			byte [] data = new byte[ 8 ];	// data byte to be transmitted
			#endregion
			
			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				// length is always 8 for SDOs
				length = 8;
				
				/* DS301 specification
				 * 
				 *		byte  0 bits 7..5	- ccs = 2	(initiate upload command)
				 *		byte  0 bits 4..0	- X			( not used )
				 *		bytes 1..3			- m			( index & sub index )
				 *		bytes 4..7			- reserved
				 */
				data[0] = 0x40;							// initiate upload request
				data[1] = (byte)index;
				data[2] = (byte)( (ushort)index >> 8 );
				data[3] = (byte)subIndex;
				data[4] = 0x00;
				data[5] = 0x00;
				data[6] = 0x00;
				data[7] = 0x00;

				// transmit our newly build SDO
				VCI.transmit( id, length, data );

				// Wait for a response or a timeout if requested.
				if ( waitForResponse == true )
				{
					resetTimer();
					awaitResponse();
				}
			} // end of if hardware is open
		}

		//--------------------------------------------------------------------------
		//  Name			: initiateUploadSegment()
		//  Description     : Builds up and transmits the initiate upload segment SDO
		//					  message then waits for a reply (or timeout).
		//  Parameters      : nodeID - node ID of the controller on the network who's OD
		//							   needs reading
		//					  toggleBit - toggle bit that has to be sent with the segment
		//								SDO message
		//  Used Variables  : hardwareOpened - flag indicates if IXXAT dongle is initialised
		//									   and running
		//					  DefaultSDOBaseAddress - constant used to calculate the COB-ID
		//										required for the SDO to talk to node of nodeID
		//  Preconditions   : An initiate upload SDO has already been sent and a valid
		//					  reply has been received.
		//  Post-conditions : The initiate upload segment SDO has been built & transmitted
		//					  with the required toggle bit.  A reply has been waited
		//					  for or until it has timed out.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an initiate upload segment SDO and waits for a reply.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs reading</param>
		/// <param name="toggleBit">toggle bit that has to be sent with the segment SDO message</param>
		private void initiateUploadSegment( int nodeID, bool toggleBit )
		{
			#region local variable declarations and initialisation
			byte	length;					// length of data within the SDO
			uint	id;						// COB-ID of the SDO
			byte [] data = new byte[ 8 ];	// data byte to be transmitted
			#endregion
			
			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				// length is always 8 for SDOs
				length = 8;
				
				/* DS301 specification
				 * 
				 *		byte  0 bits 7..5	- ccs = 3	(initiate upload segment command)
				 *		byte  0 bit  4		- t			( toggle bit )
				 *		byte  0 bits 3..0	- X			( not used )
				 *		bytes 1..7			- reserved
				 */
				data[0] = 0x60;	 

				// set the toggle bit to required value passed as a parameter
				if ( toggleBit == true )
				{
					data[0] |= 0x10;
				}

				// reserved and DS spec says to always set them to 0.
				data[1] = 0x00;
				data[2] = 0x00;
				data[3] = 0x00;
				data[4] = 0x00;
				data[5] = 0x00;
				data[6] = 0x00;
				data[7] = 0x00;

				// transmit to SDO built and wait for a reply or time out.
				VCI.transmit( id, length, data );
				resetTimer();
				awaitResponse();
			}
		}

		//--------------------------------------------------------------------------
		//  Name			: initiateDownloadRequest()
		//  Description     : This function builds up the required initiate download 
		//					  request SDO and transmits it.  It waits for a reply
		//					  or until it times out.
		//					  This function is overloaded for string data types.
		//					  This function is overloaded for byte array data types.
		//  Parameters      : nodeID - node ID of the controller on the network who's OD
		//							   needs writing to
		//					  index - index of the OD item to be written to
		//					  subIndex - sub index of the OD item to be written to
		//					  dataSize - no. of bytes of data for this OD item
		//					  newValue - newValue which is to be written to the OD item in
		//								 the controller's OD of index & sub
		//  Used Variables  : hardwareOpened - flag indicates if IXXAT dongle is initialised
		//									   and running
		//					  DefaultSDOBaseAddress - constant used to calculate the COB-ID
		//										required for the SDO to talk to node of nodeID
		//  Preconditions   : The system has been found & the EDS read so that the nodeID, index
		//					  and sub have all been checked to be valid and a newValue has been
		//					  selected by the user and range checked by the GUI.
		//  Post-conditions : The initiate download request SDO has been built & transmitted
		//					  according to the parameters passed.  A reply has been waited
		//					  for or until the comms timer has timed out.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an initiate download SDO for a long value and waits for a response.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs writing to</param>
		/// <param name="index">index of the OD item to be written to</param>
		/// <param name="subIndex">sub index of the OD item to be written to</param>
		/// <param name="dataSize">no. of bytes of data for this OD item</param>
		/// <param name="newValue">newValue which is to be written to the OD item in the controller's
		/// OD of index and sub</param>
		private void initiateDownloadRequest( int nodeID, int index, int subIndex, uint dataSize, long newValue )
		{
			#region local variable declarations and initialisation
			byte	length;					// length of data within the SDO
			uint	id;						// COB-ID of the SDO
			byte [] data = new byte[ 8 ];	// data byte to be transmitted
			#endregion
			
			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				// length is always 8 for SDOs
				length = 8;
				
				/* DS301 specification
				 * 
				 *		byte  0 bits 7..5	- ccs = 1	(initiate download command)
				 *		byte  0 bit  4		- X			( not used )
				 *		byte  0 bits 3..2	- n			( only valid if e=1 & s=1 otherwise 0   )
				 *										( else no. bytes in d that don't contain)
				 *										( valid data							)
				 *		byte  0 bit 1		- e			( transfer type 1=expedited ) 
				 *		byte  0 bit 0		- s			( size indicator 1=size indicated )
				 *		bytes 1..3			- m			( index and sub )
				 *		bytes 4..7			- d			( data )
				 */
				data[0] = 0x20;							
				data[1] = (byte)index;
				data[2] = (byte)( (ushort)index >> 8 );
				data[3] = (byte)subIndex;

				// will it fit in an expedited message?
				if ( dataSize <= 4 )
				{
					data[0] |= 0x02;						// e bit = expedited
					data[0] |= 0x01;						// s bit = size indicated
					data[0] |= (byte)( ( 4 - dataSize ) << 2 );	// n = no bytes with no data

					// fits in an expedited message so put newValue into data bytes d
					data[4] = (byte)newValue;
					data[5] = (byte)( newValue >> 8 );
					data[6] = (byte)( newValue >> 16 );
					data[7] = (byte)( newValue >> 24 );
				}
					// no - send the size of the data to be sent in the following segments
				else
				{
					data[0] |= 0x01;					// s bit = size indicated
					data[4] = (byte)dataSize;
					data[5] = (byte)( dataSize >> 8 );
					data[6] = 0x00;
					data[7] = 0x00;
				}

				// transmit to SDO built and wait for a reply or time out.
				VCI.transmit( id, length, data );
				resetTimer();
				awaitResponse();
			} // end of if hardware opened
		}

		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an initiate download SDO for a string value and waits for a response.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs writing to</param>
		/// <param name="index">index of the OD item to be written to</param>
		/// <param name="subIndex">sub index of the OD item to be written to</param>
		/// <param name="dataSize">no. of bytes of data for this OD item</param>
		/// <param name="newValue">newValue which is to be written to the OD item in the controller's
		/// OD of index and sub</param>
		private void initiateDownloadRequest( int nodeID, int index, int subIndex, uint dataSize, string newValue )
		{
			#region local variable declarations and initialisation
			byte	length;					// length of data within the SDO
			uint	id;						// COB-ID of the SDO
			byte [] data = new byte[ 8 ];	// data byte to be transmitted
			byte [] newValueArray = new byte [ newValue.Length ];	// create array of new value length
			uint i = 0;
			#endregion
			
			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// convert to a byte array
				if ( newValue != null )
				{
					foreach ( char c in newValue )
					{
						newValueArray[ i++ ] = (byte)c;
					}
				}

				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				// length is always 8 for SDOs
				length = 8;
				
				/* DS301 specification
				 * 
				 *		byte  0 bits 7..5	- ccs = 1	(initiate download command)
				 *		byte  0 bit  4		- X			( not used )
				 *		byte  0 bits 3..2	- n			( only valid if e=1 & s=1 otherwise 0   )
				 *										( else no. bytes in d that don't contain)
				 *										( valid data							)
				 *		byte  0 bit 1		- e			( transfer type 1=expedited ) 
				 *		byte  0 bit 0		- s			( size indicator 1=size indicated )
				 *		bytes 1..3			- m			( index and sub )
				 *		bytes 4..7			- d			( data )
				 */
				data[0] = 0x20;							
				data[1] = (byte)index;
				data[2] = (byte)( (ushort)index >> 8 );
				data[3] = (byte)subIndex;

				// will it fit in an expedited message?
				if ( dataSize <= 4 )
				{
					data[0] |= 0x02;						// e bit = expedited
					data[0] |= (byte)( ( 4 - dataSize ) << 2 );	// n = no bytes with no data

					// copy required number of data bytes over into transmit array
					for ( i = 0; i < dataSize; i++ )
					{
						data[ 4 + i ] =  newValueArray[ i ];
					}

					// ensure the rest of the array is 0x00
					for ( i = dataSize; i < 4; i++ )
					{
						data[ 4 + i ] = 0x00;
					}
				}
					// no - send the size of the data to be sent in the following segments
				else
				{
					data[0] |= 0x01;					// s bit = segmented
					data[4] = (byte)dataSize;			// data size indication
					data[5] = (byte)( dataSize >> 8 );
					data[6] = (byte)( dataSize >> 16 );
					data[7] = (byte)( dataSize >> 24 );
				}

				// transmit to SDO built and wait for a reply or time out.
				VCI.transmit( id, length, data );
				resetTimer();
				awaitResponse();
			}
		}

		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an initiate download SDO for a byte array value and waits for a response.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs writing to</param>
		/// <param name="index">index of the OD item to be written to</param>
		/// <param name="subIndex">sub index of the OD item to be written to</param>
		/// <param name="newValue">newValue which is to be written to the OD item in the controller's
		/// OD of index and sub</param>
		private void initiateDownloadRequest( int nodeID, int index, int subIndex, ref byte [] newValue )
		{
			#region local variable declarations and initialisation
			byte	length;					// length of data within the SDO
			uint	id;						// COB-ID of the SDO
			byte [] data = new byte[ 8 ];	// data byte to be transmitted
			int		i = 0;
			int		dataSize = newValue.Length;	// data size is the length of the array
			#endregion

			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				// length is always 8 for SDOs
				length = 8;
				
				/* DS301 specification
				 * 
				 *		byte  0 bits 7..5	- ccs = 1	(initiate download request ccs = 1)
				 *		byte  0 bit  4		- X			( not used )
				 *		byte  0 bits 3..2	- n			( only valid if e=1 & s=1 otherwise 0   )
				 *										( else no. bytes in d that don't contain)
				 *										( valid data							)
				 *		byte  0 bit 1		- e			( transfer type 1=expedited ) 
				 *		byte  0 bit 0		- s			( size indicator 1=size indicated )
				 *		bytes 1..3			- m			( index and sub )
				 *		bytes 4..7			- d			( data )
				 */
				data[0] = 0x20;
				data[1] = (byte)index;
				data[2] = (byte)( (ushort)index >> 8 );
				data[3] = (byte)subIndex;

				// will it fit in an expedited message?
				if ( dataSize <= 4 )
				{
					data[0] |= 0x01;					// s bit = size indicated
					data[0] |= 0x02;					// e bit = expedited
					data[0] |= (byte)( ( 4 - dataSize ) << 2 );	// n = no bytes with no data

					for ( i = 0; i < dataSize; i++ )
					{
						data[ 4 + i ] =  newValue[ i ];
					}

					// ensure the rest of the array is 0x00
					for ( i = dataSize; i < 4; i++ )
					{
						data[ 4 + i ] = 0x00;
					}
				}
					// no - send the size of the data to be sent in the following segments
				else
				{
					data[0] |= 0x01;					// s bit = size indicated
					data[4] = (byte)dataSize;			// contains data length
					data[5] = (byte)( dataSize >> 8 );
					data[6] = (byte)( dataSize >> 16 );
					data[7] = (byte)( dataSize >> 24 );
				}

				// transmit to SDO built and wait for a reply or time out.
				VCI.transmit( id, length, data );
				resetTimer();
				awaitResponse();
			}
		}


		//--------------------------------------------------------------------------
		//  Name			: initiateDownloadSegment()
		//  Description     : This function builds up the next segmented SDO in the
		//					  current download message and transmits it.  It waits
		//					  for a reply or until it times out.
		//					  (NB if data >7 bytes then require multiple SDOs to send).
		//					  This function is overloaded for string data types.
		//					  This function is overloaded for byte array data types.
		//  Parameters      : nodeID - node ID of the controller on the network who's OD
		//							   needs writing to
		//					  toggleBit - bit which must be toggled between segments, used
		//								  for error checking to ensure no missed or
		//							      out of sequence segments etc.
		//					  dataSize - no. of bytes of data for this OD item
		//					  newValue - newValue which is to be written to the OD item in
		//								 the controller's OD of index & sub
		//					  txBytePtr - pointer to the first byte in the array of data
		//								  which is to be transmitted in this segmented SDO
		//  Used Variables  : hardwareOpened - flag indicates if IXXAT dongle is initialised
		//									   and running
		//					  DefaultSDOBaseAddress - constant used to calculate the COB-ID
		//										required for the SDO to talk to node of nodeID
		//  Preconditions   : The system has been found & the EDS read so that the nodeID, index
		//					  and sub have all been checked to be valid and a newValue has been
		//					  selected by the user and range checked by the GUI.  Also
		//					  The initiate download request SDO has already been sent
		//					  and a valid reply was received.
		//  Post-conditions : The next segment SDO in the current download message sequence
		//					  has been build, transmitted and a reply/timeout has been 
		//					  received.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Builds and transmits the next segmented SDO to download a new long value and waits for a response.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs writing to</param>
		/// <param name="toggleBit">bit which must be toggled between segments, used for error
		/// checking to ensure no missed or out of sequence segments etc</param>
		/// <param name="dataSize">no. of bytes of data for this OD item</param>
		/// <param name="newValue">newValue which is to be written to the OD item in the controller's
		/// OD of index and sub</param>
		/// <param name="txBytePtr">pointer to the first byte in the array of data which is
		/// to be transmitted in this segmented SDO</param>
		private void initiateDownloadSegment( int nodeID, bool toggleBit, long newValue, uint dataSize, ref int txBytePtr )
		{
			#region local variable declarations and initialisation
			byte	length;						// length of data within the SDO
			uint	id;							// COB-ID of the SDO
			byte [] data = new byte[ 8 ];		// data byte to be transmitted
			uint		dataBytesInThisSegment = 7;	// default it 7 data bytes in a segment
			#endregion
			
			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				// length is always 8 for SDOs
				length = 8;
				
				/* DS301 specification
				 * 
				 *		byte  0 bits 7..5	- ccs = 0	(download segment request ccs = 0)
				 *		byte  0 bit  4		- t			( toggle bit )
				 *		byte  0 bits 3..1	- n			( n=0 if no segment size indicated      )
				 *										( else no. bytes in d that don't contain)
				 *										( valid data							)
				 *		byte  0 bit 0		- c			( indicates whether more segments to come )
				 *		bytes 1..7			- d			( data )
				 */
				data[0] = 0x00;
				
				// update toggle bit as passed parameter
				if ( toggleBit == true )
				{
					data[0] |= 0x10;
				}

				/* If data still to be sent is more than 7 bytes then there are more segments
				 * to follow this one.
				 */
				if ( ( dataSize - txBytePtr - 7 ) > 0 )
				{
				}
				else	// only the last segment can have less than 7 data bytes
				{
					data[0] |= 0x01;					// no more segments to be transmitted
					dataBytesInThisSegment = (uint) (dataSize - txBytePtr);
				}

				// fill n with the number of bytes in d that don't contain data
				data[0] |= (byte)( ( 7 - dataBytesInThisSegment ) << 1 );
				// copy relevant part of data over into the transmit array
				for ( int i = 0; i < dataBytesInThisSegment; i++ )
				{
					data[ i + 1 ] = (byte)( newValue >> ( 8 * txBytePtr) );
					txBytePtr++;
				}
				
				// transmit to SDO built and wait for a reply or time out.
				VCI.transmit( id, length, data );
				resetTimer();
				awaitResponse();
			} // end of if hardware opened
		}

		//--------------------------------------------------------------------------
		///<summary>Builds and transmits the next segmented SDO to download a new string value and waits for a response.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs writing to</param>
		/// <param name="toggleBit">bit which must be toggled between segments, used for error
		/// checking to ensure no missed or out of sequence segments etc</param>
		/// <param name="dataSize">no. of bytes of data for this OD item</param>
		/// <param name="newValue">newValue which is to be written to the OD item in the controller's
		/// OD of index and sub</param>
		/// <param name="txBytePtr">pointer to the first byte in the array of data which is
		/// to be transmitted in this segmented SDO</param>
		private void initiateDownloadSegment( int nodeID, bool toggleBit, string newValue, uint dataSize, ref int txBytePtr )
		{
			#region local variable declarations and initialisation
			byte	length;						// length of data within the SDO
			uint	id;							// COB-ID of the SDO
			byte [] data = new byte[ 8 ];		// data byte to be transmitted
			uint		dataBytesInThisSegment = 7;	// default it 7 data bytes in a segment
			uint i = 0;
			byte [] newValueArray = new byte [ newValue.Length ];	// allocate memory to convert to byte array from string
			#endregion
			
			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// convert to a byte array from a string.
				if ( newValue != null )
				{
					foreach ( char c in newValue )
					{
						newValueArray[ i++ ] = (byte)c;
					}
				}

				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				// length is always 8 for SDOs
				length = 8;
				
				/* DS301 specification
				 * 
				 *		byte  0 bits 7..5	- ccs = 0	(download segment request ccs = 0)
				 *		byte  0 bit  4		- t			( toggle bit )
				 *		byte  0 bits 3..1	- n			( n=0 if no segment size indicated      )
				 *										( else no. bytes in d that don't contain)
				 *										( valid data							)
				 *		byte  0 bit 0		- c			( indicates whether more segments to come )
				 *		bytes 1..7			- d			( data )
				 */
				data[0] = 0x00;							// download segment request ccs = 0
				
				// update toggle bit as passed parameter
				if ( toggleBit == true )
				{
					data[0] |= 0x10;
				}

				/* If data still to be sent is more than 7 bytes then there are more segments
				 * to follow this one.
				 */
				if ( ( dataSize - txBytePtr - 7 ) > 0 )
				{
				}
				else	// only the last segment that may have less than 7 data bytes
				{
					data[0] |= 0x01;					// no more segments to be transmitted
					dataBytesInThisSegment = (uint) (dataSize - txBytePtr);
				}

				// fill n with the number of bytes in d that don't contain data
				data[0] |= (byte)( ( 7 - dataBytesInThisSegment ) << 1 );

				// copy relevant part of data over into the transmit array
				for ( i = 0; i < dataBytesInThisSegment; i++ )
				{
					data[ i + 1 ] = newValueArray[ txBytePtr ];
					txBytePtr++;
				}

				// ensure the rest of the array is 0x00
				for ( i = dataBytesInThisSegment; i < 7; i++ )
				{
					data[ i + 1 ] = 0x00;
				}

				// transmit to SDO built and wait for a reply or time out.
				VCI.transmit( id, length, data );
				resetTimer();
				awaitResponse();
			} // end of if hardware opened
		}

		//--------------------------------------------------------------------------
		///<summary>Builds and transmits the next segmented SDO to download a new byte array value and waits for a response.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs writing to</param>
		/// <param name="toggleBit">bit which must be toggled between segments, used for error
		/// checking to ensure no missed or out of sequence segments etc</param>
		/// <param name="newValue">newValue which is to be written to the OD item in the controller's
		/// OD of index and sub</param>
		/// <param name="txBytePtr">pointer to the first byte in the array of data which is
		/// to be transmitted in this segmented SDO</param>
		private void initiateDownloadSegment( int nodeID, bool toggleBit, ref byte[] newValue, ref int txBytePtr )
		{
			#region local variable declarations and initialisation
			byte	length;						// length of data within the SDO
			uint	id;							// COB-ID of the SDO
			byte [] data = new byte[ 8 ];		// data byte to be transmitted
			int		dataBytesInThisSegment = 7;	// default it 7 data bytes in a segment
			int i = 0;
			byte [] newValueArray = new byte [ newValue.Length ];	// allocate memory to convert to byte array from string
			int dataSize = newValue.Length;
			#endregion

			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				// length is always 8 for SDOs
				length = 8;
				
				/* DS301 specification
				 * 
				 *		byte  0 bits 7..5	- ccs = 0	(download segment request ccs = 0)
				 *		byte  0 bit  4		- t			( toggle bit )
				 *		byte  0 bits 3..1	- n			( n=0 if no segment size indicated      )
				 *										( else no. bytes in d that don't contain)
				 *										( valid data							)
				 *		byte  0 bit 0		- c			( indicates whether more segments to come )
				 *		bytes 1..7			- d			( data )
				 */
				data[0] = 0x00;							// download segment request ccs = 0
				
				// update toggle bit as passed parameter
				if ( toggleBit == true )
				{
					data[0] |= 0x10;
				}

				/* If data still to be sent is more than 7 bytes then there are more segments
				 * to follow this one.
				 */
				if ( ( dataSize - txBytePtr - 7 ) > 0 )
				{
				}
				else	// only the last segment that may have less than 7 data bytes
				{
					data[0] |= 0x01;					// no more segments to be transmitted
					dataBytesInThisSegment = dataSize - txBytePtr;
				}

				// fill n with the number of bytes in d that don't contain data
				data[0] |= (byte)( ( 7 - dataBytesInThisSegment ) << 1 );

				// copy relevant part of data over into the transmit array
				for ( i = 0; i < dataBytesInThisSegment; i++ )
				{
					data[ i + 1 ] = newValue[ txBytePtr ];
					txBytePtr++;
				}

				// ensure the rest of the array is 0x00
				for ( i = dataBytesInThisSegment; i < 7; i++ )
				{
					data[ i + 1 ] = 0x00;
				}

				// transmit to SDO built and wait for a reply or time out.
				VCI.transmit( id, length, data );
				resetTimer();
				awaitResponse();
			} // end of if hardware opened
		}
		
		
		#region block download

		//--------------------------------------------------------------------------
		//  Name			: initiateBlockDownloadRequest()
		//  Description     : This function builds up the required initiate block download 
		//					  request SDO and transmits it.  It waits for a reply
		//					  or until it times out.
		//  Parameters      : nodeID - node ID of the controller on the network who's OD
		//							   needs writing to
		//					  index - index of the OD item to be written to
		//					  subIndex - sub index of the OD item to be written to
		//					  dataSize - no. of bytes of data for this block
		//					  maxDeviceBlocks - returned max segments in a block allowed by the controller
		//  Used Variables  : hardwareOpened - flag indicates if IXXAT dongle is initialised
		//									   and running
		//					  DefaultSDOBaseAddress - constant used to calculate the COB-ID
		//										required for the SDO to talk to node of nodeID
		//  Preconditions   : The system has been found & the EDS read so that the nodeID, index
		//					  and sub have all been checked to be valid and a newValue has been
		//					  selected by the user and range checked by the GUI.
		//  Post-conditions : The initiate download request SDO has been built & transmitted
		//					  according to the parameters passed.  A reply has been waited
		//					  for or until the comms timer has timed out.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an initiate block download SDO request for a byte array and waits for a response.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs writing to</param>
		/// <param name="index">index of the OD item to be written to</param>
		/// <param name="subIndex">sub index of the OD item to be written to</param>
		/// <param name="dataSize">no. of bytes of data for this OD item</param>
		/// <param name="maxDeviceBlocks">returned value indicating the max segments in a block allowed by the controller </param>
		/// OD of index and sub</param>
		private DIFeedbackCode initiateBlockDownloadRequest( int nodeID, int index, int subIndex, int dataSize, out byte maxDeviceBlocks )
		{
			#region local variable declarations and initialisation
			byte	length;						// length of data within the SDO
			uint	id;							// COB-ID of the SDO
			byte [] data = new byte[ 8 ];		// data byte to be transmitted
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;

			maxDeviceBlocks = 0;
			#endregion

			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				// length is always 8 for SDOs
				length = 8;
				
				/* Build transmit packet according to DS301 specification
				 * DS301 specification
				 * 
				 *		byte 0 bits 7..5 	- ccs = 6	( download block request ccs = 6 )
				 *		byte 0 bits 4..3 	- x			( not used always 0 )
				 *		byte 0 bit  2       - cc		( 1 => client supports calculating CRC)
				 *		byte 0 bit  1		- c			( 1 => size indicated ) 
				 *		byte 0 bit  0		- cs		( 0 => initiate download request )
				 *		bytes 1..3			- m			( multiplexor - represents index and sub-index )
				 *		bytes 4..7			- size		( download size in bytes, byte 4 has lsb )
				 */
				data[0] = 0xc2;							// initiate block download request ccs as above
				data[1] = (byte)index;
				data[2] = (byte)( (ushort)index >> 8 );
				data[3] = (byte)subIndex;
				data[4] = (byte)dataSize;
				data[5] = (byte)( dataSize >> 8 );
				data[6] = (byte)( dataSize >> 16 );
				data[7] = (byte)( dataSize >> 24 );
				
				// transmit to SDO built and wait for a reply or time out.
				VCI.transmit( id, length, data );
				resetTimer();
				awaitResponse();

				// what kind of SDO reply did we get?
                switch (VCI.rxResponseType)
				{
						#region initiate download block response (expected)
					case SDOMessageTypes.InitiateBlockDownloadResponse:
					{
                        maxDeviceBlocks = VCI.rxMsg.data[4];
						fbc = DIFeedbackCode.DISuccess;
						break;
					}
						#endregion

						#region abort message
						// response from the controller to the last message is an abort message
					case SDOMessageTypes.AbortResponse:
					{
						long abortCode = 0;

						// extract the abort code from the message (4 bytes) back into a 32 bit no.
						for( int i = 0; i < 4; i++ )
						{
							abortCode <<= 8;
                            abortCode += VCI.rxMsg.data[7 - i];
						}

						// recast as enum type to give user a clue what number actually means.
						fbc = (DIFeedbackCode)abortCode;
						break;
					}
						#endregion

						#region no response, invalid or unknown type of response
						/* No response (timed out), unknown response type or invalid in the 
							 * context of sending a download request SDO message.
							 */
					case SDOMessageTypes.NoResponse:
					case SDOMessageTypes.UnknownResponseType:
					case SDOMessageTypes.InvalidResponse:
					default:
					{
						fbc = DIFeedbackCode.DINoResponseFromController;
						break;
					}
						#endregion
				} // end of switch statement

			} // end of if hardware opened

			return ( fbc );
		}

		//--------------------------------------------------------------------------
		//  Name			: initiateBlockDownloadSegment()
		//  Description     : This function builds up the required segment of the current
		//					  SDO download block and transmits it.  It deliberately does
		//					  not wait for a reply to speed up download times.
		//  Parameters      : nodeID - node ID of the controller on the network who's OD
		//							   needs writing to
		//					  sequenceNumber - current segment number in this block
		//					  newValue - byte array being downloaded with this block
		//					  txBytePtr - points to the next byte within newValue[] which
		//								is to be downloaded in this segment
		//  Used Variables  : hardwareOpened - flag indicates if IXXAT dongle is initialised
		//									   and running
		//					  DefaultSDOBaseAddress - constant used to calculate the COB-ID
		//										required for the SDO to talk to node of nodeID
		//  Preconditions   : The intiate SDO block download had a valid reply and the block
		//					  size is compatible with the controller.
		//  Post-conditions : The next segment in the current block has been transmitted
		//					  according to the parameters passed.  No reply has been
		//					  waited for.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Builds and transmits a block download SDO segment for the next 7 bytes of data in the newValue byte array 
		/// and does not wait for a response.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs writing to</param>
		/// <param name="newValue">byte array being downloaded with this block</param>
		/// <param name="sequenceNumber">current segment number in this block</param>
		/// <param name="txBytePtr">points to the next byte within newValue[] which is to be downloaded in this segment</param>
		/// OD of index and sub</param>
		private void initiateBlockDownloadSegment( int nodeID, ref int sequenceNumber, ref byte[] newValue, ref int txBytePtr )
		{
			#region local variable declarations and initialisation
			uint	id;							// COB-ID of the SDO
			byte	length = 8;					// dlc always 8
			byte [] data = new byte[ 8 ];		// data byte to be transmitted
			int		dataBytesInThisSegment = 7;	// default it 7 data bytes in a segment
			int i = 0;
			byte [] newValueArray = new byte [ newValue.Length ];	// allocate memory to convert to byte array from string
			int dataSize = newValue.Length;
			#endregion

			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				#region Sequence number goes 1..127 If > 127 then wrap back to the start.
				int seqNo = sequenceNumber;

				if ( sequenceNumber >= 128 )
				{
					seqNo = 1;
				}
				#endregion

				#region build up transmit message for this segment according to DS301
				/* DS301 specification
				 * 
				 *		byte  0 bit 7		- c			( 0 => more segments to be downloaded )
				 *										( 1 => no more segments to be downloaded )
				 *		byte  0 bit 6..0 	- seqno		( sequence number, 1..127 )
				 *		bytes 1..7			- seg-data	( segment data )
				*/

				/* If data still to be sent is more than 7 bytes then there are more segments of block
				 * to follow this one.
				 */
				if ( ( dataSize - txBytePtr - 7 ) > 0 )
				{
					data[0] = ( 0x00 << 7 );					// more segments to be transmitted
					dataBytesInThisSegment = 7;
				}
				else	// only the last segment that may have less than 7 data bytes
				{
					data[0] = ( 0x01 << 7 );					// no more segments to follow
					dataBytesInThisSegment = dataSize - txBytePtr;	
				}

				// fill seqno with the sequence number of this segment in the block transfer
				data[0] |= (byte)seqNo;

				// copy relevant part of data over into the transmit array
				for ( i = 0; i < dataBytesInThisSegment; i++ )
				{
					data[ i + 1 ] = newValue[ txBytePtr ];
					txBytePtr++;
				}

				// ensure the rest of the array is 0x00
				for ( i = dataBytesInThisSegment; i < 7; i++ )
				{
					data[ i + 1 ] = 0x00;
				}
				#endregion

				// transmit to SDO built and don't wait for a reply
				VCI.transmit( id, length, data );

				// deliberately don't wait for a reply to speed up the block transfer
			} // end of if hardware opened
		}
	
		//--------------------------------------------------------------------------
		//  Name			: endBlockDownloadRequest()
		//  Description     : This function builds up the current block download
		//					  SDO end request and transmits it and waits for a response.
		//  Parameters      : nodeID - node ID of the controller on the network who's OD
		//							   needs writing to
		//					  newValue - byte array being downloaded with this block
		//					  txBytePtr - points to the next byte within newValue[] which
		//								is to be downloaded in this segment
		//  Used Variables  : hardwareOpened - flag indicates if IXXAT dongle is initialised
		//									   and running
		//					  DefaultSDOBaseAddress - constant used to calculate the COB-ID
		//										required for the SDO to talk to node of nodeID
		//  Preconditions   : The intiate SDO block download had a valid reply and the block
		//					  size is compatible with the controller and all the block
		//					  segments have been transmitted.
		//  Post-conditions : The end SDO block download message has been transmitted
		//					  according to the parameters passed and a response waited for
		//					  and interpreted.
		//  Return value    : fbc - indicates success or gives a failure reason
		//--------------------------------------------------------------------------
		///<summary>Builds and transmits an end block download request and waits for a response which is 
		/// interpreted.</summary>
		/// <param name="nodeID">node ID of the controller on the network who's OD needs writing to</param>
		/// <param name="newValue">byte array being downloaded with this block</param>
		/// <param name="txBytePtr">indicates the next byte to be transmitted</param>
		/// <returns></returns>
		private DIFeedbackCode endBlockDownloadRequest( int nodeID, ref byte[] newValue, ref int txBytePtr )
		{
			#region local variable declarations and initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			byte	length;						// length of data within the SDO
			uint	id;							// COB-ID of the SDO
			byte [] data = new byte[ 8 ];		// data byte to be transmitted
			#endregion

			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				#region build up end SDO block download message
				// length is always 8 for SDOs
				length = 8;

				// calculate the number of unused bytes in the last SDO download segment sent
				byte unusedBytesInLastSegment = (byte)( ( 7 - (byte)( newValue.Length % 7 ) ) % 7 );

				/* DS301 specification for end block download
				 * 
				 *		byte 0 bits 7..5 	- ccs = 6	( download block request ccs = 6 )
				 *		byte 0 bits 4..2 	- n			( number of data bytes not used )
				 *		byte 0 bit  1       - x			( not used - always 0 )
				 *		byte 0 bit  0		- cs		( always 1 )
				 *		bytes 1..2			- crc		( CRC if used or set to 0 otherwise )
				 *		bytes 3..7			- reserved	( always 0 )
				*/
				data[0] = 0xc0;							// initiate block download request ccs as above

				data[0] |= (byte)( unusedBytesInLastSegment << 2 );
				data[0] |= 0x01;
				data[1] = 0x00;		// would be CRC lsb
				data[2] = 0x00;		// would be CRC msb
				data[3] = 0x00;
				data[4] = 0x00;
				data[5] = 0x00;
				data[6] = 0x00;
				data[7] = 0x00;
				#endregion

				// transmit to SDO built and wait for a reply or time out.
				VCI.transmit( id, length, data );
				resetTimer();

				#region filter through all received SDO messages until we get the expected response, an abort message or a timeout
				do
				{
					// wait for the next reply from the controller
					awaitResponse();

					// what kind of SDO message response did we get?
                    switch (VCI.rxResponseType)
					{
							#region download block segment response
						case SDOMessageTypes.DownloadBlockSegmentResponse:
						{
							// Ignore these as these are in response to all the segments sent previously.
							// We are only interested in the end SDO block download response.
							break;
						}
							#endregion

							#region end download block response (successful reply)
						case SDOMessageTypes.EndDownloadBlockResponse:
						{
							//  should check CRC matches once this is included but no code space in controller bootloader
							fbc = DIFeedbackCode.DISuccess;
							break;
						}
							#endregion

							#region abort response
							// response from the controller to the last message is an abort message
						case SDOMessageTypes.AbortResponse:
						{
							long abortCode = 0;

							// extract the abort code from the message (4 bytes) back into a 32 bit no.
							for( int i = 0; i < 4; i++ )
							{
								abortCode <<= 8;
                                abortCode += VCI.rxMsg.data[7 - i];
							}

							// recast as enum type to give user a clue what number actually means.
							fbc = (DIFeedbackCode)abortCode;
							break;
						}
							#endregion

							#region no response (timed out)
							/* Unknown and invalid are ignored as a segment response could be contrued as invalid
							 * due to the fact that segments were sent without waiting for a response and we are now 
							 * waiting for an end download block response.
							 */
						case SDOMessageTypes.NoResponse:
						default:
						{
							fbc = DIFeedbackCode.DINoResponseFromController;
							break;
						}
							#endregion
					} // end of switch statement

					// quit loop if we get an abort code - something went wrong
                    if (VCI.rxResponseType == SDOMessageTypes.AbortResponse)
					{
						break;
					}
				}
				while 
					( 
					( timerExpired == false ) 
					&& ( fbc != DIFeedbackCode.DISuccess )
					&& ( fbc != DIFeedbackCode.DINoResponseFromController )
					);
				#endregion
			} // end of if hardware opened

			return ( fbc );
		}
		
		#endregion block download

		#region block upload - untested code not currently called
		private DIFeedbackCode initiateBlockUploadRequest( int nodeID, int index, int subIndex, out int blockSize )
		{
			#region local variable declarations and initialisation
			byte	length;						// length of data within the SDO
			uint	id;							// COB-ID of the SDO
			byte [] data = new byte[ 8 ];		// data byte to be transmitted
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;

			blockSize = 0;
			#endregion

			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				// length is always 8 for SDOs
				length = 8;
				
				/* DS301 specification
				 * 
				 *		byte 0 bits 7..5 	- ccs = 5	( upload block request ccs = 5 )
				 *		byte 0 bits 4..3 	- x			( not used always 0 )
				 *		byte 0 bit  2		- cc		( 0 => client CRC not supported, 1 = supported )
				 *		byte 0 bit  1..0    - cs		( 0 => initiate upload request )
				 *		bytes 1..3			- m			( multiplexor - represents index and sub-index )
				 *		bytes 4	     		- blksize	( number of segments per block )
				 *		byte  5				- pst		( 0 => not allowed to change transfer protocol )
				 *		byte  6..7			- x			( not used always 0 )
				 */
				data[0] = 0x50;							// initiate block ipload request ccs as above
				data[1] = (byte)index;
				data[2] = (byte)( (ushort)index >> 8 );
				data[3] = (byte)subIndex;
				data[4] = SCCorpStyle.maxSDOBlockSize;							// max block size
				data[5] = 0x00;
				data[6] = 0x00;
				data[7] = 0x00;
				
				// transmit to SDO built and wait for a reply or time out.
				VCI.transmit( id, length, data );
				resetTimer();
				awaitResponse();

				// what kind of SDO message response did we get?
                switch (VCI.rxResponseType)
				{
					case SDOMessageTypes.InitiateBlockUploadResponse:
					{
						// if block size indicated
                        if ((VCI.rxMsg.data[0] & 0x20) == 0x20)
                        {
                            blockSize = VCI.rxMsg.data[4]
                                + (VCI.rxMsg.data[5] << 8)
                                + (VCI.rxMsg.data[6] << 16)
                                + (VCI.rxMsg.data[7] << 24);
						}
						else
						{
							blockSize = 0;
						}

						fbc = DIFeedbackCode.DISuccess;
						break;
					}

						// response from the controller to the last message is an abort message
					case SDOMessageTypes.AbortResponse:
					{
						long abortCode = 0;

						// extract the abort code from the message (4 bytes) back into a 32 bit no.
						for( int i = 0; i < 4; i++ )
						{
							abortCode <<= 8;
                            abortCode += VCI.rxMsg.data[7 - i];
						}

						// recast as enum type to give user a clue what number actually means.
						fbc = (DIFeedbackCode)abortCode;
						break;
					}

						/* No response (timed out), unknown response type or invalid in the 
							 * context of sending a download request SDO message.
							 */
					case SDOMessageTypes.NoResponse:
					case SDOMessageTypes.UnknownResponseType:
					case SDOMessageTypes.InvalidResponse:
					default:
					{
						fbc = DIFeedbackCode.DINoResponseFromController;
						break;
					}
				} // end of switch statement
			} // end of if hardware opened

			return ( fbc );
		}

		private void requestBlockUploadSegment( int nodeID )
		{
			#region local variable declarations and initialisation
			uint	id;							// COB-ID of the SDO
			byte	length = 8;					// dlc always 8
			byte [] data = new byte[ 8 ];		// data byte to be transmitted
			#endregion

			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// COB-ID is the nodeID plus default
				id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

				/* DS301 specification
				 * 
				 *		byte 0 bits 7..5 	- ccs = 5	( upload block request ccs = 5 )
				 *		byte 0 bits 4..2 	- x			( not used always 0 )
				 *		byte 0 bit  1..0    - cs		( 3 => start upload )
				 *		bytes 1..7			- x			( not used always 0 )
				 */
				data[0] = 0xa3;
				data[1] = 0x00;
				data[2] = 0x00;
				data[3] = 0x00;
				data[4] = 0x00;
				data[5] = 0x00;
				data[6] = 0x00;
				data[7] = 0x00;
				
				// transmit of SDO built but don't wait for a reply.
				VCI.transmit( id, length, data );
			}
		}

		private DIFeedbackCode blockUploadSegment( int nodeID, byte sequenceNumber, ref byte[] rxData, ref int rxBytePtr, out bool moreBlocksExpected )
		{
			#region local variable declarations and initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			uint	id;							// COB-ID of the SDO
			byte	length = 8;					// dlc always 8
			byte [] data = new byte[ 8 ];		// data byte to be transmitted
			int		dataBytesInThisSegment = 7;	// default it 7 data bytes in a segment

			moreBlocksExpected = true;
			#endregion

			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// wait for a reply
				resetTimer();
				awaitResponse();

				// what kind of SDO message response did we get?
                switch (VCI.rxResponseType)
				{
					case SDOMessageTypes.UploadBlockSegmentResponse:
					{
						// if block size indicated
                        if ((VCI.rxMsg.data[0] & 0x01) == 0x01)
						{
							moreBlocksExpected = false;
						}

                        byte seqNo = (byte)((VCI.rxMsg.data[0] & 0xfe) >> 1);

						if ( seqNo != sequenceNumber )
						{
							fbc = DIFeedbackCode.DIGeneralFailure;
						}
						else
						{
							fbc = DIFeedbackCode.DISuccess;
							
							// copy next segment of received data over into overall received data buffer
							for ( int i = 0; i < dataBytesInThisSegment; i++ )
							{
								if ( rxBytePtr < rxData.Length )
								{
                                    rxData[rxBytePtr++] = VCI.rxMsg.data[i + 1];
								}
							}
						}

						break;
					}

						// response from the controller to the last message is an abort message
					case SDOMessageTypes.AbortResponse:
					{
						long abortCode = 0;

						// extract the abort code from the message (4 bytes) back into a 32 bit no.
						for( int i = 0; i < 4; i++ )
						{
							abortCode <<= 8;
                            abortCode += VCI.rxMsg.data[7 - i];
						}

						// recast as enum type to give user a clue what number actually means.
						fbc = (DIFeedbackCode)abortCode;
						break;
					}

						/* No response (timed out), unknown response type or invalid in the 
							 * context of sending a download request SDO message.
							 */
					case SDOMessageTypes.NoResponse:
					case SDOMessageTypes.UnknownResponseType:
					case SDOMessageTypes.InvalidResponse:
					default:
					{
						fbc = DIFeedbackCode.DINoResponseFromController;
						break;
					}
				} // end of switch statement

				if ( fbc == DIFeedbackCode.DISuccess )
				{
					// COB-ID is the nodeID plus default
					id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

					/* DS301 specification
					 * 
					 *		byte 0 bits 7..5 	- ccs = 5	( upload block request ccs = 5 )
					 *		byte 0 bits 4..2 	- x			( not used always 0 )
					 *		byte 0 bit  1..0    - cs		( 2 => block upload response )
					 *		byte 1				- ackseq	( sequence number of segment received )
					 *		byte 2				- blksize	( number of blocks to be used by the server )
					 *		bytes 3..7			- reserved	( not used always 0 )
					 */
					data[0] = 0xa2;
					data[1] = sequenceNumber;
					data[2] = 0xfe;
					data[3] = 0x00;
					data[4] = 0x00;
					data[5] = 0x00;
					data[6] = 0x00;
					data[7] = 0x00;
				
					// transmit of SDO built but don't wait for a rely (none expected).
					VCI.transmit( id, length, data );
				}
			} // end of if hardware opened

			return ( fbc );
		}

		private DIFeedbackCode endBlockUploadRequest( int nodeID, int blockSize, ref byte[] rxData, int rxBytePtr )
		{
			#region local variable declarations and initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			uint	id;							// COB-ID of the SDO
			byte	length = 8;					// dlc always 8
			byte [] data = new byte[ 8 ];		// data byte to be transmitted
			#endregion

			/* Only bother building up & transmitting an SDO if the IXXAT dongle is available
			 * and has been opened and is operational.
			 */
            if (VCI.CANAdapterHWIntialised == true)
			{
				// wait for a reply
				resetTimer();
				awaitResponse();

				// what kind of SDO message response did we get?
                switch (VCI.rxResponseType)
				{
					case SDOMessageTypes.EndBlockUploadResponse:
					{
						// if the block size is unknown then resize rxData array to match the size of the data actually received
						if ( blockSize == 0 )
						{
							// if there were unused bytes in the last segment then remove them from the end of rxData

                            if ((VCI.rxMsg.data[0] & 0x1c) > 0x00)
                            {
                                byte unusedBytes = (byte)((VCI.rxMsg.data[0] & 0x1c) >> 2);
								rxBytePtr -= unusedBytes;

								if ( rxBytePtr < 0 )
								{
									rxBytePtr = 0;
								}
							}
			
							if ( rxBytePtr > 0 )
							{
								byte [] sizedRxData = new byte[ rxBytePtr ];

								for ( int i = 0; i < rxBytePtr; i++ )
								{
									sizedRxData[ i ] = rxData[ i ];
								}

								rxData = sizedRxData;
							}
						}

						// check CRC of data when calculation is in
						fbc = DIFeedbackCode.DISuccess;
						break;
					}

						// response from the controller to the last message is an abort message
					case SDOMessageTypes.AbortResponse:
					{
						long abortCode = 0;

						// extract the abort code from the message (4 bytes) back into a 32 bit no.
						for( int i = 0; i < 4; i++ )
						{
							abortCode <<= 8;
                            abortCode += VCI.rxMsg.data[7 - i];
						}

						// recast as enum type to give user a clue what number actually means.
						fbc = (DIFeedbackCode)abortCode;
						break;
					}

						/* No response (timed out), unknown response type or invalid in the 
							 * context of sending a download request SDO message.
							 */
					case SDOMessageTypes.NoResponse:
					case SDOMessageTypes.UnknownResponseType:
					case SDOMessageTypes.InvalidResponse:
					default:
					{
						fbc = DIFeedbackCode.DINoResponseFromController;
						break;
					}
				} // end of switch statement

				if ( fbc == DIFeedbackCode.DISuccess )
				{
					// COB-ID is the nodeID plus default
					id = (uint)nodeID + SCCorpStyle.DefaultSDOBaseAddress;

					/* DS301 specification
					 * 
					 *		byte 0 bits 7..5 	- ccs = 5	( upload block ccs = 5 )
					 *		byte 0 bits 4..1 	- x			( not used always 0 )
					 *		byte 0 bit  0		- cs		( 1 => end block upload request )
					 *		bytes 1..7			- reserved	( not used always 0 )
					 */
					data[0] = 0xa1;
					data[1] = 0x00;
					data[2] = 0x00;
					data[3] = 0x00;
					data[4] = 0x00;
					data[5] = 0x00;
					data[6] = 0x00;
					data[7] = 0x00;
				
					// transmit of SDO built but don't wait for a rely (none expected).
					VCI.transmit( id, length, data );
				}
				
			}

			return ( fbc );
		}

		#endregion

		#endregion

		#region comms timer
		//--------------------------------------------------------------------------
		//  Name			: setCommsTimeout() 
		//  Description     : Sets the communications timer timeout value to the new
		//					  value passed as a parameter. Determines the period of
		//					  time which must pass without a CAN message reply before
		//					  a DINoResponse failure code is set.
		//  Parameters      : newTimeout - timeout to be used for any subsequent CAN
		//						messages transmitted, in milliseconds
		//  Used Variables  : rxTimeout - static variable which is used to reload
		//						the response timer
		//  Preconditions   : The system is initialised and found and the GUI knows
		//					  a specific timeout is needed for the next SDO request
		//					  it is about to make.
		//  Post-conditions : The rxTimeout has been set to the value needed for the
		//					  next SDO request.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Sets the communications timer timeout to the new value.</summary>
		/// <param name="newTimeout">timeout to be used for any subsequent CAN messages
		/// transmitted, in milliseconds</param>
		public void setCommsTimeout( int newTimeout )
		{
			rxTimeout = newTimeout;
		}

		//--------------------------------------------------------------------------
		//  Name			: threadTimerExpired() 
		//  Description     : Delegate function which is called when the thread
		//					  timer has expired.
		//  Parameters      : state - required by .NET but not used
		//  Used Variables  : timerExpired - flag set for checking in rest of this
		//									 object
		//  Preconditions   : The timer has been initialised and has run until it
		//					  has expired.
		//  Post-conditions : timerExpired flag has been set to true.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Delegate function which is called when the thread timer has expired.</summary>
		/// <param name="state">required by .NET but not used</param>
		static void threadTimerExpired( Object state )
		{
			timerExpired = true;
#if DEBUG
            System.Console.Out.WriteLine("comms timeout expired(int): " + System.DateTime.Now.TimeOfDay.ToString());
            System.Console.Out.WriteLine("rxTimeout: " + rxTimeout.ToString());
#endif
		}

		//--------------------------------------------------------------------------
		//  Name			: resetTimer() 
		//  Description     : This function restarts the comms timer by clearing out
		//					  the expired flag and restarting the thread timer.
		//					  Used as a one shot timer with rxTimeout duration.
		//  Parameters      : None
		//  Used Variables  : timerExpired - flag used to indicate when the timer
		//								has expired
		//				      rxTimeout - static variable which is used to reload
		//						the response timer
		//  Preconditions   : The timer has already been created.  It may or may
		//					  not be currently running.
		//  Post-conditions : The timer has been restarted with the rxTimeout value
		//					  and the timerExpired flag has been cleared reading for
		//					  this new run of the timer.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Restarts the communication timer for the last set rxTimeout period.</summary>
		private void resetTimer()
		{
			timerExpired = false;
			responseTimer.Change( rxTimeout, Timeout.Infinite );
		}

		//--------------------------------------------------------------------------
		//  Name			: awaitResponse() 
		//  Description     : This process blocks the current thread until either
		//					  some kind of response has been received from the controller
		//					  after the last SDO message DW transmitted OR the comms
		//					  timer has timed out.
		//  Parameters      : None
		//  Used Variables  : timerExpired - flag set by timer delegate to indicate 
		//								when the timer has expired
		//					  VCI.rxResponseType - VCI object flag which indicates if
		//								the type of message that has been received
		//							    in response to the last SDO that DW sent
		//  Preconditions   : The timer has already been created and it is
		//					  currently running.
		//  Post-conditions : The timer has expired or some kind of response has 
		//					  been received by the VCI object after the last SDO was sent.
		//  Return value    : None
		//-------------------------------------------------------------------------
		///<summary>Blocks current thread unitl either SDO response received or the communications timer has expired.</summary>
		private void awaitResponse()
		{
            ThreadPriority origPriority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            do
            {
                // periodically halt this thread to give the IXXAT thread a chance to run on slower PCs
                Thread.Sleep(1);      
            }
            while
            (
                (timerExpired == false)
             && (VCI.rxResponseType == SDOMessageTypes.NoResponse)
            );

            Thread.CurrentThread.Priority = origPriority;

#if DEBUG
            if (timerExpired == true)
            {
                System.Console.Out.WriteLine("comms timeout expired.");
            }
#endif
		}
		#endregion

        private DIFeedbackCode awaitBlockSegmentResponses()
        {
            DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
            ThreadPriority origPriority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            do
            {
                // periodically halt this thread to give the IXXAT thread a chance to run on slower PCs
                Thread.Sleep(1);
            }
            while (
                (timerExpired == false)
                && (VCI.BlockSgmentResponses < this.noOfBlockSegments)
                ) ;

            Thread.CurrentThread.Priority = origPriority;

            if (timerExpired == true)
            {
#if DEBUG
                System.Console.Out.WriteLine("block comms timeout expired.");  
#endif
                // give one last try in case both happened simultaneously due to thread racing
                if (VCI.BlockSgmentResponses < this.noOfBlockSegments)
                {
                    fbc = DIFeedbackCode.DIMissingSequenceResponse;
                }
            }

            return (fbc);
        }
		#region communications delegates

		//--------------------------------------------------------------------------
		//  Name			: addNewHeartbeatDelegate()
		//  Description     : Passes the on hew heartbeat delegate function pointer
		//					  through to the VCI (because it is private to the CANcomms
		//					  object).
		//  Parameters      : onh - indicates which function has to be called on the
		//						heartbeat event
		//  Used Variables  : None
		//  Preconditions   : The SystemInfo object has it's delegates setup.
		//  Post-conditions : The VCI object now points to the function delegated
		//					  by onh.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Adds the new heartbeat delegate to the VCI, delegated by onh.</summary>
		/// <param name="onh">indicates which function has to be called on the heartbeat event</param>
		public void addNewHeartbeatDelegate( onNewHeartbeat onh )
		{
			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				//we must have real devices also connected - but if we ar ein virtual we should ignore them
				return;
			}
			VCI.addNewHeartbeatDelegate( onh );
		}

		//--------------------------------------------------------------------------
		//  Name			: addNewEmergencyDelegate()
		//  Description     : Passes the on new emergency delegate function pointer
		//					  through to the VCI (because it is private to the CANcomms
		//					  object).
		//  Parameters      : one - indicates which function has to be called on the
		//						emergency event
		//  Used Variables  : None
		//  Preconditions   : The SystemInfo object has it's delegates setup.
		//  Post-conditions : The VCI object now points to the function delegated
		//					  by one.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Adds the new emergency delegate to the VCI, delegated by one.</summary>
		/// <param name="one">indicates which function has to be called on the emergency event</param>
		public void addNewEmergencyDelegate( onNewEmergency one )
		{
			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				//we must have real devices also connected - but if we ar ein virtual we should ignore them
				return;
			}
			VCI.addNewEmergencyDelegate( one );
		}
		#endregion

	}
}
