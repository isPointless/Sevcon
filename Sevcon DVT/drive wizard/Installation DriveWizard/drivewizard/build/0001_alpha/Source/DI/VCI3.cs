//-----------------------------------------------------------------------------
//  SEVCON Limited. Copyright 2004                                          
//                                                                          
//  File Name       : VCI3.cs
//  Description     : VCI interface class for the IXATT USB-CAN adapter.
//                                                                          
//  Modification History                                                    
//    AJK,	05/08/04,	UK0139,		- Original
//    JW / TSN  - Updated to VCI ver. 3.1
//----------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.Threading;
using DriveWizard;
using Ixxat.Vci3;           
using Ixxat.Vci3.Bal;       
using Ixxat.Vci3.Bal.Can;


namespace DW_Ixxat
{
    #region enumerated types

    #region SDO message types
    /* SDO message types: types of SDOs that are used between the GUI and the controller
	 * over the CANbus for communication.  For example, an upload is a request by DW for
	 * object information from the controller's OD.  This requires a request from DW and
	 * the controller may reply with an expedited upload response (all data fits in
	 * one comms packet) or it may use several upload segment requests and responses
	 * for the controller to send the infomation up to DW, 7 bytes at a time.
	 * There are similar SDO types for downloading information (DW writes new values to
	 * a specific object in the controller's OD).
	 * 
	 * NOTE: DW always initiates the SDO request and the controller replies.
	 * 
	 * If there is no response from the controller or the controller rejects the request,
	 * there are no responses, unknown responses invalid and abort response message types.
	 */
    /// <summary>
    /// SDO message types: types of SDOs that are used between the GUI and the controller
    /// over the CANbus for communication.
    /// </summary>
    public enum SDOMessageTypes
    {
        NoResponse,
        InitiateUploadRequest,
        InitiateUploadResponseExpedited,
        InitiateUploadResponse,
        UploadSegmentRequest,
        UploadSegmentResponse,
        InitiateDownloadRequest,
        InitiateDownloadResponse,
        DownloadSegmentRequest,
        DownloadSegmentResponse,
        InitiateBlockDownloadResponse,
        DownloadBlockSegmentResponse,
        EndDownloadBlockResponse,
        InitiateBlockUploadResponse,
        UploadBlockSegmentResponse,
        EndBlockUploadResponse,
        UnknownResponseType,
        AbortResponse,
        InvalidResponse,
        LSSResponse
    };
    #endregion

    #region VCI feedback codes
    /// <summary>
    /// VCI feedback codes.  When calling the VCI DLL functions, these return feedback
    /// codes which are defined here to improve readability of the code.  It is a
    /// failure according to DW if the value is not VCI_OK.
    /// </summary>
    public enum VCIFeedbackCode
    {
        VCI_ERR = 0,
        VCI_OK = 1,
        VCI_HWSW_ERR = -1,
        VCI_SUPP_ERR = -2,
        VCI_PARA_ERR = -3,
        VCI_RES_ERR = -4,
        VCI_QUE_ERR = -5,
        VCI_TX_ERR = -6
    };
    #endregion

    #region CAN message types
    /// <summary>
    /// Different types of CAN messages that DriveWizard will selectively listen to and
    /// analyse in it's VCI receive interrupt.
    /// SDO, LSS and PDO are standard CAN message types.
    /// EMCY and heartbeat messages are always listened to.
    /// IDList is a DW created type which is used to find the nodes on the system when
    /// a block of SDO requests are sent together and all responses waited for and listed.
    /// </summary>
    public enum CANMessageType
    {
        SDO,
        LSS,
        PDO,
        IDList
    };
    #endregion

    #endregion

    #region DriveWizard structure definition
    #region CAN message structure definition
    /// <summary>
    /// CAN message data structure definition.
    /// Structure used to hold the received or transmitted CAN message data with formatting. 
    /// </summary>
    public struct myCANMessage
    {
        
        /// <summary>in 100us/bit from time the adapter was started (relative stamping).</summary>
        public uint timeStamp;

        /// <summary>COB ID of message received or transmitted.</summary>
        public uint id;

        /// <summary>data length which is always 8 for all SDO message types.</summary>
        public byte length;

        /// <summary>data bytes which are received or transmitted.  The format of this is
        /// dependent on the SDO message type. See DS301.</summary>
        public byte[] data;
    };
    #endregion

    #region heartbeat message information structure definition
    /// <summary>Heartbeat message information. </summary>
    public struct HbInfo
    {
        /// <summary>from the COBID we can extract the node ID of the device that sent the heartbeat message</summary>
        public uint COBID;

        /// <summary> data byte 1 contains the current state of the device that sent this heartbeat message </summary>
        public int db1;
    };
    #endregion
    #endregion

    // TaskInfo contains data that will be passed to the callback
    // method.
    public class TaskInfo
    {
        public RegisteredWaitHandle Handle = null;
        public string OtherInfo = "default";
    }

    /// <summary>
    /// VCI class. Single class for both VCI3 and VCI2. Inherists form VCI_MAPPER for VCI2
    /// this class maps in all the IXXAT VCI DLL functions.
    /// On construction, the Ixxat driver version is detected. Differnet code is run for each driver version.
    /// cVCI class handles the interface between the DriveWizard DI and the IXXAT CAN adapter via the VCI DLLs.
    /// </summary>
    public class cVCI  
    {
        /// <summary>
        /// Reference to the used VCI device.
        /// </summary>
        static IVciDevice mDevice;

        /// <summary>
        /// Reference to the CAN controller.
        /// </summary>
        static ICanControl mCanCtl;

        /// <summary>
        /// Reference to the CAN message communication channel.
        /// </summary>
        static ICanChannel mCanChn;

        /// <summary>
        /// Reference to the message writer of the CAN message channel.
        /// </summary>
        static ICanMessageWriter mWriter;

        /// <summary>
        /// Reference to the message reader of the CAN message channel.
        /// </summary>
        static ICanMessageReader mReader;

        /// <summary>
        /// Event that's set if at least one message was received.
        /// </summary>
        internal static AutoResetEvent mRxEvent;

        WaitOrTimerCallback myThreadMethod;

        /// <summary>
        /// Reference to Bus Access Layer
        /// </summary>
        static IBalObject bal = null;

        /// <summary>Used to allow rx handler to close</summary>
        internal static int mMustQuit = 0;
        internal static TaskInfo mTaskInfo = new TaskInfo();

        #region last Txd CAN message
        private myCANMessage txMsg;
        #endregion last Txd CAN message

        #region emergency and heartbeat variables for FIFOs and delegates
        private static onNewEmergency emcyEventDelegate = null;
        private static readonly int sizeOfFIFOs = 100;
        private static int emcyFIFOIn = 0;
        private static int emcyFIFOOut = 0;
        private static byte[][] emcyFIFO = new byte[sizeOfFIFOs][];

        private static onNewHeartbeat hbEventDelegate = null;
        private static int hbFIFOIn = 0;
        private static int hbFIFOOut = 0;
        private static HbInfo[] hbFIFO = new HbInfo[sizeOfFIFOs];

        // separate timer thread and timeout for processing EMCY and heartbeat FIFO queues
        private static readonly int EMCYAndHBTimeoutCheck = 500;		// in ms
        private static System.Threading.Timer threadTimer;

        /// <summary>Retain last heartbeat state for all possible node IDs</summary>
        static int[] lastHeartbeatState = new int[128];
        #endregion

#if POST_ACCESS	//timing test code
		int pinValue = 0x00;
#endif


        /// <summary>
        /// The values of Bt0, Bt1 DW uses for each baud rate 
        /// in a form suitable for inputing to Ixxat's detectBaud method
        /// </summary>
        private CanBitrate[] DWbitTimings;

        public ArrayList OdItemsBeingMonitored; //IXXAT conversion Jude

        #region monitoring data (fixed array length) containing all PDO received relevant information.
        /* Fixed length data array, used to hold all the relevant data received from the incoming
		     * PDO CAN messages, needed for graphical monitoring.
		     * This data structure is designed to have a timestamp and a variable length array
		     * of long values (as it is unknown until runtime user selection how many objects have
		     * to be monitored).
		     * Memory is allocated at a fixed length for speed as using a hash table was found to
		     * be too slow.
		     */
        public ArrayList MonitoringCOBs; //IXXAT conversion Jude
        #endregion

        public ArrayList nodeIDsFound = new ArrayList();//IXXAT conversion Jude

        #region CAN adapter running OK
        /* Flag used for the DI to keep track of whether the IXXAT adapter has been initialised
		     * and is running.  Needed when restarting and reinitialising the adapter.
		     */
        ///<summary>Indicates whether the IXXAT adapter is currently initialised and running.</summary>
        private bool _CANAdapterRunning = false;
        internal bool CANAdapterRunning
        {
            get
            {
                return (_CANAdapterRunning);
            }
        }
        #endregion CAN adapter running OK

        #region CAN adaptor HW initialised
        private bool _CANAdapterHWIntialised = false;
        internal bool CANAdapterHWIntialised
        {
            get
            {
                return (_CANAdapterHWIntialised);
            }
        }
        #endregion CAN adaptor HW initialised

        internal ushort BlockSgmentResponses = 0;
        #region VCI3 properties/fields

        #region VCI3 detectbaud rate
        internal BaudRate detectBaudRate()
        {
            CanBitrate ixxatBr;
            BaudRate detectedBaud = BaudRate._unknown;
            ushort detectBaudTimeoutInms = 10000;

            if (this._CANAdapterHWIntialised == false)
            {
                this.openCANAdapterHW();
            }
            if (this._CANAdapterHWIntialised == true)
            {
                this.resetCAN();
                ixxatBr = DWbitTimings[mCanCtl.DetectBaud(detectBaudTimeoutInms, this.DWbitTimings)];
                try
                {
                    detectedBaud = (BaudRate)Enum.Parse(typeof(BaudRate), ixxatBr.Name);
                }
                catch
                {
                    detectedBaud = BaudRate._unknown;
                }
                if (detectedBaud != BaudRate._unknown)
                {
                    this.SetupCAN(detectedBaud, (uint)CanAccCode.All, (uint)CanAccCode.All);
                }
            }
            return detectedBaud;
        }
        #endregion VCI3 detectbaud rate

        internal uint TimeStampResolution = 1;
        #endregion VCI3 properties/fields

        #region last received CAN message
        /// <summary>
        /// last message that was received from any of the attached controllers. Read only
        /// </summary>
        internal myCANMessage rxMsg; //don't make a property for now - causes problems in event handler
        #endregion last received CAN message

        #region Baudrate currently being used by CANadapter
        internal BaudRate CANAdapterBaud = BaudRate._unknown;
        #endregion Baudrate currently being used by CANadapter

        #region SDO message type of the last CAN SDO message received by the interrupt
        /* Contains the SDO message type of the last message that was received from any of the
		 * attached controllers. This is a read only property.
		 */
        private SDOMessageTypes _rxResponseType;
        ///<summary>SDO message type of the last CAN SDO message received by the interrupt.</summary>
        public SDOMessageTypes rxResponseType
        {
            get
            {
                return (_rxResponseType);
            }
            set
            {
                _rxResponseType = value;
            }
        }
        #endregion

        #region expected CAN message type to be checked for and analysed in the receive interrupt
        /* Contains the message type expected in the next receive interrupt, which needs verified
		 * and analysed.  Allows for specific message checking, dependent on the last message
		 * transmitted by DriveWizard.
		 */
        private CANMessageType _messageType = CANMessageType.SDO;
        ///<summary>Expected CAN message type to be checked for and analysed in the receive interrupt.</summary>
        public CANMessageType messageType
        {
            get
            {
                return (_messageType);
            }

            set
            {
                _messageType = value;
            }
        }
        #endregion

        #region List of PDO COBIDs to be analysed and have data extracted and put into monitoringData
        /* Contains the list of COBIDs of PDOs being transmitted on the CAN system which are known
		 * to contain information needed for graphical monitoring.  These are checked every time
		 * a PDO is received to quickly determine which new PDO receive message must be analysed
		 * and have data extracted from them to put into the monitoringData FIFO.
		 */
        //tsn
        private Hashtable _PDOCOBList;
        ///<summary>List of PDO COBIDs to be analysed and have data extracted and put into monitoringData.</summary>
        public Hashtable PDOCOBList
        {
            get
            {
                return (_PDOCOBList);
            }

            set
            {
                _PDOCOBList = value;
            }
        }
        #endregion

        #region monitoring paused (set so no longer analyse received PDOs to add data to monitorData)
        private bool _monitoringPaused = false;
        ///<summary>Monitoring paused (set so no longer analyse received PDOs to add data to monitorData).</summary>
        public bool monitoringPaused
        {
            get
            {
                return (_monitoringPaused);
            }

            set
            {
                _monitoringPaused = value;
            }
        }
        #endregion

        #region monitorData FIFOIn
        private long _FIFOIn = 0;           //tsn
        ///<summary>monitorData array pointer to put new received and extracted PDO data in.</summary>
        public long monitorDataFIFOIn
        {
            get
            {
                return (_FIFOIn);
            }
        }
        #endregion

        #region delegate initialisation
        //-------------------------------------------------------------------------
        //  Name			: addNewHeartbeatDelegate()
        //  Description     : A delegate function must be called when the VCI receives
        //					  a heartbeat message.  This is used to interpret which node
        //					  on the system produced it and what it's current NMT state
        //					  is.
        //  Parameters      : onh - indicates which function has to be called on the
        //						heartbeat event
        //  Used Variables  : hbEventDelegate - keeps a copy of onh 
        //  Preconditions   : onh has been created and there the delegate function
        //					  it points to has been defined elsewhere in the code
        //  Post-conditions : hbEventDelegate set to indicate the function to be called
        //  Return value    : None
        //----------------------------------------------------------------------------
        ///<summary>Sets up which function must be called when a heartbeat message is received.</summary>
        /// <param name="onh">indicates which function has to be called on the heartbeat event</param>
        public void addNewHeartbeatDelegate(onNewHeartbeat onh)
        {
            hbEventDelegate = onh;
        }

        //-------------------------------------------------------------------------
        //  Name			: addNewEmergencyDelegate()
        //  Description     : A delegate function must be called when the VCI receives
        //					  an EMCY message.  This is used to interpret the fault
        //					  being reported is.
        //  Parameters      : one - indicates which function has to be called on the
        //						emergency event
        //  Used Variables  : emcyEventDelegate - keeps a copy of one
        //  Preconditions   : one has been created and there the delegate function
        //					  it points to has been defined elsewhere in the code
        //  Post-conditions : emcyEventDelegate set to indicate the function to be called
        //  Return value    : None
        //----------------------------------------------------------------------------
        ///<summary>Sets up which function must be called when an emergency message is received.</summary>
        /// <param name="one">indicates which function has to be called on the emergency event</param>
        public void addNewEmergencyDelegate(onNewEmergency one)
        {
            emcyEventDelegate = one;
        }
        #endregion

        #region Heartbeat/Emrgency timer methods
        /// <summary>
        /// It is better to hold off starting this timer until connection is complete
        /// </summary>
        internal void startHbeatAndEmerThreadTimer()
        {
            // start the thread timer (every 500ms) to check the heartbeat and emergency message FIFOs
            threadTimer = new System.Threading.Timer(new System.Threading.TimerCallback(threadTimerExpired), null, EMCYAndHBTimeoutCheck, EMCYAndHBTimeoutCheck);
        }
        /// <summary>
        /// Halt the HBeatand Emer timer until re-connection is complete
        /// </summary>
        internal void stopHbeatAndEmerThreadTimer()
        {
            if (threadTimer != null)
            {
                threadTimer.Dispose();
            }
        }

        //-------------------------------------------------------------------------
        //  Name			: threadTimerExpired() 
        //  Description		: Delegate function which is called when the thread
        //					  timer has expired.
        //  Parameters		: state - required by .NET but not used
        //  Used Variables	: hbEventDelegate - function delegate for a heartbeat
        //						event. Handle to function to be called to process hb.
        //				      hbFIFOIn - in pointer to heartbeat FIFO
        //					  hbFIFOOut - out pointer to heartbeat FIFO
        //					  hbFIFO - FIFO containing heartbeat message data
        //					  emcyEventDelegate - function delegate for an emergency
        //						event. Handle to function to be called to process
        //						the emergency message.
        //				      emcyFIFOIn - in pointer to emergency FIFO
        //					  emcyFIFOOut - out pointer to emergency FIFO
        //					  emcyFIFO - FIFO containing emergency message data
        //  Preconditions	: The timer has been initialised and has run until it
        //					  has expired.
        //  Post-conditions	: If there are any unprocessed heartbeat or emergency
        //					  messages in their respective FIFOs, their
        //					  delegate function is called and the FIFOOut pointer
        //					  incremented.
        //  Return value	: None
        //----------------------------------------------------------------------------
        ///<summary>Called when the EMCY and heartbeat timer expires, to process the latest CAN messages in the FIFOs.</summary>
        /// <param name="state">required by .NET but not used</param>
        static void threadTimerExpired(Object state)
        {
            #region heartbeat FIFO checks and calling of delegate
            if (hbEventDelegate != null)
            {
                // process any new heartbeat messages in the FIFO
                while (hbFIFOIn != hbFIFOOut)
                {
                    int temp = hbFIFOOut;

                    /* Increment FIFOOut before calling delegate 
                     * (as gets confused due to time taken in delegate function).
                     */
                    if (++hbFIFOOut >= sizeOfFIFOs)
                    {
                        hbFIFOOut = 0;
                    }

                    // launch delegate to process the new message and notify user
                    hbEventDelegate((int)hbFIFO[temp].COBID, hbFIFO[temp].db1);
                }
            }
            #endregion

            #region emergency FIFO checks and calling of delegate
            if (emcyEventDelegate != null)
            {
                // process any new emergency messages in the FIFO
                while (emcyFIFOIn != emcyFIFOOut)
                {
                    int temp = emcyFIFOOut;

                    /* Increment FIFOOut before calling delegate 
                     * (as gets confused due to time taken in delegate function).
                     */
                    if (++emcyFIFOOut >= sizeOfFIFOs)
                    {
                        emcyFIFOOut = 0;
                    }
                    // launch delegate to process the new message and notify user
                    emcyEventDelegate(emcyFIFO[temp]);
                }
            }
            #endregion
        }

        #endregion Heartbeat/Emrgency timer methods

        #region constructor and destructor
        //-------------------------------------------------------------------------
        //  Name			: constructor
        //  Description     : Called when an instance of the VCI class is instantiated.
        //  Parameters      : None
        //  Used Variables  : txMsg - allocates memory for the tx structure data
        //				      _rxMsg - allocates memory for the rx structure data
        //  Preconditions   : DW requires an instance of this object.
        //  Post-conditions : The rx and tx message data buffers have memory allocated
        //					  which covers the maximum data size of a CAN message.
        //  Return value    : None
        //----------------------------------------------------------------------------
        ///<summary>Called to instantiate an instance of the VCI class.</summary>
        public cVCI()
        {

            createDriveWizardBitTimings();
            // create memory for CAN transmit & receive message data bytes
            txMsg.data = new byte[SCCorpStyle.CANMessageDataLengthMax];
            rxMsg.data = new byte[SCCorpStyle.CANMessageDataLengthMax];

            // set the receive interrupt to detect SDO messages (default)
            _messageType = CANMessageType.SDO;

            // initialise memory for the emergency message FIFO
            for (int e = 0; e < emcyFIFO.Length; e++)
            {
                emcyFIFO[e] = new byte[9];
            }

            // initialise node heartbeat state to an unknown value (needed to filter out repetitive hbs)
            for (int node = 0; node < 128; node++)
            {
                lastHeartbeatState[node] = 1;       // ie invalid (0=boot, 4=stopped, 5=op, 127=pre-op)
            }
        }

        #region Bit Timings for baud rates
        /// <summary>
        /// Called from consturctor only
        /// The baud rate on the hardware is set using two bittimings Bt0 and Bt1
        /// Setting these up in an array of CanBitBate objects allows us to use them for
        /// Setting the baud rate and also the ar ein the correct form for using Ixxats new
        /// detectBaud method. The name is set to the DW BaudRate enumeraitons equivalent string to 
        /// allows us to parse the name to get the corresponding enumeration
        /// </summary>
        private void createDriveWizardBitTimings()
        {
            DWbitTimings = new CanBitrate[10];
            DWbitTimings[0] = new CanBitrate(CanBitrate.Cia10KBit.Btr0, CanBitrate.Cia10KBit.Btr1, "_10K"); //0x31, 0x1c, "_10K");
            DWbitTimings[1] = new CanBitrate(CanBitrate.Cia20KBit.Btr0, CanBitrate.Cia20KBit.Btr1, "_20K"); // 0x18, 0x1c, "_20K");
            DWbitTimings[2] = new CanBitrate(CanBitrate.Cia50KBit.Btr0, CanBitrate.Cia50KBit.Btr1, "_50K"); //0x09, 0x1c, "_50K");
            DWbitTimings[3] = new CanBitrate(0x04, 0x1c, "_100K");
            DWbitTimings[4] = new CanBitrate(CanBitrate.Cia125KBit.Btr0, CanBitrate.Cia125KBit.Btr1, "_125K"); //0x03, 0x1c, "_125K");
            DWbitTimings[5] = new CanBitrate(CanBitrate.Cia125KBit.Btr0, CanBitrate.Cia125KBit.Btr1, "_125K"); //0x03, 0x1c, "_125K");
            DWbitTimings[6] = new CanBitrate(CanBitrate.Cia250KBit.Btr0, CanBitrate.Cia250KBit.Btr1, "_250K"); //0x01, 0x1c, "_250K");
            DWbitTimings[7] = new CanBitrate(CanBitrate.Cia500KBit.Btr0, CanBitrate.Cia500KBit.Btr1, "_500K"); //0x00, 0x1c, "_500K");
            DWbitTimings[8] = new CanBitrate(CanBitrate.Cia800KBit.Btr0, CanBitrate.Cia800KBit.Btr1, "_800K"); //0x03, 0x16, "_800");
            DWbitTimings[9] = new CanBitrate(CanBitrate.Cia1000KBit.Btr0, CanBitrate.Cia1000KBit.Btr1, "_1M"); //0x00, 0x14, "_1M");
        }
        #endregion Bit Timings for baud rates

        //-------------------------------------------------------------------------
        //  Name			: destructor
        //  Description     : Called when an instance of the VCI class is destructed.
        //  Parameters      : None
        //  Used Variables  : threadTimer - timer called periodically to analyse the
        //						heartbeat and emergency FIFOs and launch any function
        //					    delegates
        //  Preconditions   : An instance of this class has already been created and
        //					  is no longer required.
        //  Post-conditions : The threadTimer has been disposed of.
        //  Return value    : None
        //----------------------------------------------------------------------------
        ///<summary>Called to destruct an instance of the VCI class.</summary>
        ~cVCI()                             
        {
            if (threadTimer != null)
            {
                threadTimer.Dispose();
            }
        }
        #endregion

        #region VCI Hardware initialisation and closure
        //-------------------------------------------------------------------------
        //  Name			: openCANAdapterHW()
        //  Description     : This function opens the IXXAT adapter and returns a 
        //					  handle to it if it is successful.  And the CAN message
        //					  receive interrupt handler is setup.
        //  Parameters      : None
        //  Used Variables  : receiveQueuedata - receive interrupt handler
        //					  xatCallback - callback function handle for receive data
        //					  BoardHdl - holds board handle returned when IXXAT opened
        //  Preconditions   : The IXXAT board is not opened for use.
        //  Post-conditions : The IXXAT board is opened and the receive interrupt handler
        //					  is set up or there is a failure code.
        //  Return value    : Board handler or failure code if unsuccessful.
        //----------------------------------------------------------------------------
        ///<summary>Opens the IXXAT adapter and sets up the receive interrupt.</summary>
        /// <returns>Board handler or failure code if unsuccessful</returns>
        public void openCANAdapterHW()
        {
#if PORT_ACCESS	//timing test code
            //resetPins();
#endif
            if (this._CANAdapterHWIntialised == false)
            {
                try
                {
                    this._CANAdapterHWIntialised = true;
                    if (SelectDevice() == false)
                    {
                        this._CANAdapterHWIntialised = false;
                    }
                    if (InitSocket(0) == false)
                    {
                        this._CANAdapterHWIntialised = false;

                        // tell receive thread not to quit
                        Interlocked.Exchange(ref mMustQuit, 0);
                    }
                    TimeStampResolution = mCanChn.ClockFrequency / mCanChn.TimeStampCounterDivisor;
                }
                catch
                {
                    this._CANAdapterHWIntialised = false;
                }
                finally
                {
                    if (this._CANAdapterHWIntialised == false)
                    {
                        #region user feedback
                        SystemInfo.errorSB.Append("DriveWizard was unable to re-initialise the USB-CAN adapter. ");
                        SystemInfo.errorSB.Append("\nPlease check your USB port connection");
                        SystemInfo.errorSB.Append("\nand that the drivers have been installed for this port.");
                        SystemInfo.errorSB.Append("\nIf all LEDs on the adapter are extinguished, disconnect and reconnect the adapter\nto the PC");
                        SystemInfo.errorSB.Append("\nand then retry.\n");
                        #endregion user feedback
                    }
                    //TODO
                }
            }
        }

        #region VCI3 HW Initialisation/Closure methods
        #region Device selection

        //************************************************************************
        /// <summary>
        ///   Selects the first CAN adapter.
        /// </summary>
        //************************************************************************
        static bool SelectDevice()
        {
            bool retVal = true;
            IVciDeviceManager deviceManager = null;
            IVciDeviceList deviceList = null;
            IEnumerator deviceEnum = null;

            try
            {
                //
                // Get device manager from VCI server
                //
                deviceManager = VciServer.GetDeviceManager();

                //
                // Get the list of installed VCI devices
                //
                deviceList = deviceManager.GetDeviceList();

                //
                // Get enumerator for the list of devices
                //
                deviceEnum = deviceList.GetEnumerator();

                //
                // Get first device
                //
                //deviceEnum.MoveNext();                            //tsn - 11dec07
                //mDevice = deviceEnum.Current as IVciDevice;

                if (deviceEnum.MoveNext())
                {
                    mDevice = deviceEnum.Current as IVciDevice;
                }
                else
                {
                    DisposeVciObject(mDevice);
                }
            }
            catch (Exception)
            {
                retVal = false;
                SystemInfo.errorSB.Append("Error: No VCI device installed");
            }
            finally
            {
                //
                // Dispose device manager ; it's no longer needed.
                //
                DisposeVciObject(deviceManager);

                //
                // Dispose device list ; it's no longer needed.
                //
                DisposeVciObject(deviceList);

                //
                // Dispose device list ; it's no longer needed.
                //
                DisposeVciObject(deviceEnum);
            }
            return (retVal);
        }

        #endregion

        #region Opening socket

        //************************************************************************
        /// <summary>
        ///   Opens the specified socket, creates a message channel, initializes
        ///   and starts the CAN controller.
        /// </summary>
        /// <param name="canNo">
        ///   Number of the CAN controller to open.
        /// </param>
        //************************************************************************
        static bool InitSocket(Byte canNo)
        {
            bool retVal = true;
            IBalObject bal = null;

            try
            {
                //
                // Open bus access layer
                //
                bal = mDevice.OpenBusAccessLayer();

                //
                // Open a message channel for the CAN controller
                //
                mCanChn = bal.OpenSocket(canNo, typeof(ICanChannel)) as ICanChannel;
                

                // Initialize the message channel
                // Receive buffer = 1024 CAN messages
                // Transmit buffer = 256 CAN messages
                // Exclusive use of channel = false
                // Note: by reading the mReader.Capacity or mWriter.Capacity you can see what
                // count is allocated. Usually rounded up from that requested, up to a max of 
                // 8394 for a request of 0x1fff 
                mCanChn.Initialize(1024, 256, false);

                // Get a message reader object
                mReader = mCanChn.GetMessageReader();
                // Initialize message reader
                mReader.Threshold = 1;

                // Get a message wrtier object
                mWriter = mCanChn.GetMessageWriter();
                // Initialize message writer
                mWriter.Threshold = 1;

                // Create and assign the event that's set if at least one message
                // was received.
                mRxEvent = new AutoResetEvent(false);
                mReader.AssignEvent(mRxEvent);

                // Activate the message channel
                mCanChn.Activate();
                
                //
                // Open the CAN controller
                //
                mCanCtl = bal.OpenSocket(canNo, typeof(ICanControl)) as ICanControl;

            }
            catch (Exception e)
            {
                retVal = false;
                SystemInfo.errorSB.Append(" Failed to initialize socket: Ixxat error code: " + e.Message + " " + e.InnerException);
            }
            finally
            {
                //
                // Dispose bus access layer
                //
                DisposeVciObject(bal);
            }
            return (retVal);
        }

        #endregion

        #region Dispose HW object
        /// <summary>
        ///   This method tries to dispose the specified object.
        /// </summary>
        /// <param name="obj">
        ///   Reference to the object to be disposed.
        /// </param>
        /// <remarks>
        ///   The VCI interfaces provide access to native driver resources. 
        ///   Because the .NET garbage collector is only designed to manage memory, 
        ///   but not native OS and driver resources the application itself is 
        ///   responsible to release these resources via calling 
        ///   IDisposable.Dispose() for the obects obtained from the VCI API 
        ///   when these are no longer needed. 
        ///   Otherwise native memory and resource leaks may occure.  
        /// </remarks>
        static void DisposeVciObject(object obj)
        {
            if (null != obj)
            {
                IDisposable dispose = obj as IDisposable;
                if (null != dispose)
                {
                    dispose.Dispose();
                    obj = null;
                }
            }
        }
        #endregion Dispose HW object

        #endregion VCI3 HW Initialisation/Closure methods

        #region Setup CAN adapter method
        //-------------------------------------------------------------------------
        //  Name			: SetupCAN()
        //  Description     : This function initialises the IXXAT adapter to the
        //					  baud rate and acceptance and filter masks that are
        //					  passed as parameters.
        //  Parameters      : baud - enumerated type indicating the baud rate which
        //							 the adapter has to run at.
        //					  acceptanceMask - acceptance mask to be used by the adapter
        //									   to filter out and reduce the interrupt 
        //									   loading on the PC processor.
        //					  acceptanceFilter - acceptance filter to be used by the
        //									   adapter
        //  Used Variables  : None
        //  Preconditions   : None
        //  Post-conditions : The IXXAT adapter has been configured.
        //  Return value    : None
        //----------------------------------------------------------------------------
        ///<summary>Initialises the IXXAT adapter CAN baud rate, acceptance mask and filter.</summary>
        /// <param name="baud">enumerated type indicating the baud rate which the adapter has to run at</param>
        /// <param name="acceptanceMask">acceptance mask to be used by the adapter to filter out and 
        /// reduce the interrupt loading on the PC processor</param>
        /// <param name="acceptanceFilter">acceptance filter to be used by the adapter</param>
        /// <returns>feedback indicates success or gives a failure reason</returns>
        public void SetupCAN(BaudRate baud, uint acceptanceMask, uint acceptanceFilter)
        {
            //Enusr that our adapter hardware is currnetly initialised (nothing to setup in virtual DR38000254)
            if (MAIN_WINDOW.isVirtualNodes == false)
            {
                if (this._CANAdapterHWIntialised == false)
                {
                    this.openCANAdapterHW();
                }
                if (this._CANAdapterHWIntialised == true)
                {
                    this.stopCAN();

                    #region convert baud into an Ixxat CanBitRate object
                    CanBitrate myBitRate = DWbitTimings[9];  //default to 1Meg
                    foreach (CanBitrate canBr in this.DWbitTimings)
                    {
                        string test = baud.ToString();
                        if (canBr.Name == baud.ToString())
                        {
                            myBitRate = canBr;
                            break;
                        }
                    }
                    #endregion convert baud into an Ixxat CanBitRate object

                    #region try to set the baud rate on the adapter hardware
                    try
                    {
                        //// Initialize the CAN controller
                        mCanCtl.InitLine(CanOperatingModes.Standard, myBitRate);
                        CANAdapterBaud = baud;
                    }
                    catch (Exception e1)
                    {
                        SystemInfo.errorSB.Append("Unable to set CAN adapter baud rate: Exception: ");              //tsn
                        SystemInfo.errorSB.Append(e1.Message);
                        CANAdapterBaud = BaudRate._unknown;
                    }
                    #endregion try to set the baud rate on the adapter hardware

                    #region try and set the accpetance filter and mask
                    try
                    {
                        // Set the acceptance filter - were wrong way round (DR38000268)
                        // NOTE: CanFilter is set to standard as filtering doesn't work in Extended mode
                        acceptanceMask = (acceptanceMask << 1);    //make RTR in bit0 don't care
                        acceptanceFilter = (acceptanceFilter << 1);         // shift as RTR is in bit 0
                        mCanCtl.SetAccFilter(CanFilter.Std, acceptanceFilter, acceptanceMask);
                    }
                    catch (Exception e2)
                    {
                        SystemInfo.errorSB.Append("Unable to set CAN adapter acceptance filters: Exception: ");     //tsn
                        SystemInfo.errorSB.Append(e2.Message);
                    }
                    #endregion try and set the accpetance filter and mask

                    this.startCAN();
                }
            }
        }
        #endregion Setup CAN adapter method

        #region Close HW
        //-------------------------------------------------------------------------
        //  Name			: Close()
        //  Description     : This function closes the IXXAT adapter which was 
        //					  previously opened with BoardHdl handle.
        //  Parameters      : None
        //  Used Variables  : BoardHdl - handle given when the IXXAT adapter was opened
        //  Preconditions   : The IXXAT adapter was already opened with BoardHdl.
        //  Post-conditions : The IXXAT adapter is closed and no longer running.
        //  Return value    : None
        //----------------------------------------------------------------------------
        ///<summary>Closes the IXXAT adapter previously opened.</summary>
        public void closeCANAdapterHW()
        {
            if (this._CANAdapterHWIntialised == true)
            {
                this.resetCAN();
                this._CANAdapterHWIntialised = false;
                try
                {
                    // tell receive thread to quit
                    Interlocked.Exchange(ref mMustQuit, 1);

                    // Wait for termination of receive thread
                    if (mTaskInfo.Handle != null)
                    {
                        mTaskInfo.Handle.Unregister(null);
                        mTaskInfo.Handle = null;
                        mTaskInfo.OtherInfo = "";
                    }

                    //Jude all Hardware handles must be roperlyy disposed to avoid memory leaks 
                    //(garbage collector won't mop up handles to OS resources) 
                    //and also to allow recovery after hibernation
                    DisposeVciObject(bal);
                    //jude - try setting these to null for hibernation recovery
                    DisposeVciObject(mDevice);
                    DisposeVciObject(mCanCtl);
                    DisposeVciObject(mWriter);
                    DisposeVciObject(mReader);
                    DisposeVciObject(mCanChn);
                }
                catch (Exception e)
                {
                    SystemInfo.errorSB.Append("\nFailed to close IXXAT adapter.");
                    SystemInfo.errorSB.Append(" error code");
                    SystemInfo.errorSB.Append(e.Message);
                }
            }
        }
        #endregion Close HW

        #endregion VCI Hardware initialisation and closure

        #region start, stop, reset CAN interface
        ///<summary>Resets the IXXAT adapter to put it back into the pre-operational state (to allow reconiguration).</summary>
        /// <returns>feedback indicates success or gives a failure reason</returns>
        public void resetCAN()
        {
            try
            {
                mCanCtl.ResetLine();
                this._CANAdapterRunning = false;
            }
            catch (Exception e)
            {
                SystemInfo.errorSB.Append("Could not reset IXXAT adapter.\nException: ");
                SystemInfo.errorSB.Append(e.Message);
            }
        }


        ///<summary>Starts the IXXAT adapter running (ie puts into the operational mode).</summary>
        /// Name			: startCAN()
        ///  Description     : Starts the IXXAT adapter and puts it into operational
        ///					  mode ie running state after it has been initialised.
        ///  Parameters      : None
        ///  Used Variables  : BoardHdl - handle given when the IXXAT adapter was opened
        ///  Preconditions   : The IXXAT adapter has been opened, the receive interrupt
        ///					  handler has been setup, the receive and transmit buffers
        ///					  have been configured and the acceptance mask and filter
        ///					  has been setup.
        ///  Post-conditions : The IXXAT adapter is running.
        ///  Return value    : fbc indicating whether the IXXAT was started OK or giving
        ///					  a failure reason.
        /// <returns>feedback indicates success or gives a failure reason</returns>
        public void startCAN()
        {
            try
            {
                AutoResetEvent myEvent = new AutoResetEvent(false);
                myThreadMethod = new System.Threading.WaitOrTimerCallback(handleRxMsg);

                // add to thread pool & force handleRxMsg() at least every 500ms
                mTaskInfo.Handle =
                ThreadPool.RegisterWaitForSingleObject(mRxEvent, myThreadMethod,
                                    (object)this, 500, false);

                mTaskInfo.OtherInfo = "rx task";
                mCanCtl.StartLine();        //tsn

                // raise event and start thread
                mRxEvent.Set();
                this._CANAdapterRunning = true;
            }
            catch (Exception e)
            {
                this._CANAdapterRunning = false;
                SystemInfo.errorSB.Append("\nCould not start CAN adapter");
                SystemInfo.errorSB.Append("Error code ");
                SystemInfo.errorSB.Append(e.Message);
            }
        }

        #region VCI3 specific methods
        /// <summary>
        /// SImilar to resetCAN except that any current transmits are
        /// completed before stopping th eCNA bus
        /// </summary>
        internal void stopCAN()
        {
            //stopCAN is only applicable to VCI3
            try
            {
                mCanCtl.StopLine();
                this._CANAdapterRunning = false;
            }
            catch (Exception e)
            {
                this._CANAdapterRunning = true;
                SystemInfo.errorSB.Append("\nCould not stop IXXAT adapter");
                SystemInfo.errorSB.Append("\nIXXAT error code: ");
                SystemInfo.errorSB.Append(e.Message);
            }
        }
        #endregion VCI3 specific methods


        #endregion start, stop, reset CAN interface

        #region receive handlers

        #region VCI3 Receive handler
        static void handleRxMsg(object VCIin, bool signaled)
        {
            if (mMustQuit == 0)
            {
                //applicable to VCI3 only
                cVCI VCI = (cVCI)VCIin;
                CanMessage canMessage;

                if (mReader.ReadMessage(out canMessage) == true)
                {
                    // process this CAN message from the receive FIFO
                    if (canMessage.FrameType == CanMsgFrameType.Data)
                    {
                        #region switch message type
                        // show data frames

                        #region check heartbeat message for controller state (put in FIFO for processing in timer thread)
                        #region LSS
                        //DR38000268 - move LSS here otherwise since it's 0x7e4, it erroneously goes into (>0x700 & < 0x7ff) below
                        if (canMessage.Identifier == SCCorpStyle.LSSResponseCOBID)
                        {
                            #region LSS
                            VCI.rxResponseType = SDOMessageTypes.LSSResponse;
                            #region copy received data over
                            for (int b = 0; b < SCCorpStyle.CANMessageDataLengthMax; b++)
                            {
                                VCI.rxMsg.data[b] = canMessage[b];
                            }
                            #endregion
                            #endregion LSS
                        }
                        #endregion LSS
                        else if ((canMessage.Identifier > 0x700) && (canMessage.Identifier <= 0x7FF))
                        {
                            // if valid data then copy COBID and first data byte into next FIFO slot
                            if (canMessage.DataLength >= 1)
                            {
                                int test = (int)(canMessage.Identifier - 0x700);
                                //judetemp we can only detect expected heartbeats - PDOs can have valid COBIDs in the heartbeat range
                                // and only add hbs to fifo that indicate new state - taken out as op & pre-op changes getting missed sometimes
                                if ((VCI.nodeIDsFound.Contains(test) == true)) /*&& (lastHeartbeatState[test] != (int)canMessage[0])) */
                                {
                                    hbFIFO[hbFIFOIn].COBID = canMessage.Identifier;
                                    hbFIFO[hbFIFOIn].db1 = canMessage[0];
                                    lastHeartbeatState[test] = canMessage[0];       // update this node's hb state
                                    // increment fifo in with wrap around
                                    if (++hbFIFOIn >= sizeOfFIFOs)
                                    {
                                        hbFIFOIn = 0;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region check for EMCY messages for faults (put in FIFO for processing in timer thread)
                        else if ((canMessage.Identifier > SCCorpStyle.COBForEmergencyMinimum)
                            && (canMessage.Identifier <= SCCorpStyle.COBForEmergencyMaximum))
                        {
                            // if valid data then copy 8 data bytes into next FIFO slot
                            if (canMessage.DataLength == 8)
                            {
                                for (byte i = 0; i < 8; i++)
                                {
                                    emcyFIFO[emcyFIFOIn][i] = canMessage[i];
                                }

                                emcyFIFO[emcyFIFOIn][8] = (byte)canMessage.Identifier;

                                // increment fifo in with wrap around
                                if (++emcyFIFOIn >= sizeOfFIFOs)
                                {
                                    emcyFIFOIn = 0;
                                }
                            }
                        }
                        #endregion

                        #region SDOs
                        else if ((canMessage.Identifier > 0x580) && (canMessage.Identifier <= 0x5FF))
                        {
                            if (VCI.messageType == CANMessageType.IDList)
                            {
                                #region Connecting to system
                                if
                                    (                                                      // already tested identifier above
                                       (canMessage[1] == VCI.txMsg.data[1])                // }
                                    && (canMessage[2] == VCI.txMsg.data[2])                // } OD Index and Sub
                                    && (canMessage[3] == VCI.txMsg.data[3])                // }
                                    )
                                {
                                    int nodeID = (ushort)(canMessage.Identifier - 0x580);
                                    if (VCI.nodeIDsFound.Contains(nodeID) == true)
                                    {
                                        //                                                        MAIN_WINDOW.duplicateNodeIDNo = (ushort)nodeID;
                                        SystemInfo.errorSB.Append("\nDuplicate Node ID found: ");
                                        SystemInfo.errorSB.Append(nodeID.ToString());
                                    }
                                    else
                                    {
                                        VCI.nodeIDsFound.Add(nodeID);
                                    }
                                }
                                #endregion Connecting to system
                            }
                            else
                            {
                                #region if expected COBID for SDO reply to last message sent

                                #region comments
                                /* 
                       * SDO response with matching node ID and index & sub.
                       * COB ID response is 0x580 + NODE_ID and request is 0x600 + NODE_ID
                       * Hence the response is always 0x080 less than that transmitted.
                       */
                                #endregion comments
                                if
                                    (
                                    ((canMessage.Identifier > 0x580) && (canMessage.Identifier <= 0x5FF)) //default SDO cOB ID ( 0x580) plus node ID in range 1 to 127
                                    && (canMessage.Identifier == (VCI.txMsg.id - SCCorpStyle.NodeCOBIDToNodeIDOffset)) //is response to SDO we sent ie came back form correct Node ID
                                    )
                                #endregion
                                {
                                    SDOMessageTypes tmpMsgType = VCI.rxResponseType;
                                    #region extract the response type from data byte 0
                                    byte scs = (byte)(canMessage[0] & 0xe0);
                                    //DR38000239 JW _rxResponseType should not be set to default here - can be polled before switch is done
                                    #endregion

                                    #region check rx data by scs code
                                    switch (scs)
                                    {
                                        #region initiate upload response
                                        case 0x40:
                                            {
                                                // check that the index and sub-index match the tx packet
                                                if
                                                    (
                                                    (VCI.txMsg.data[1] == canMessage[1])
                                                    && (VCI.txMsg.data[2] == canMessage[2])
                                                    && (VCI.txMsg.data[3] == canMessage[3])
                                                    )
                                                {
                                                    // Is it an initiate upload expedited response?
                                                    if ((canMessage[0] & 0x02) == 0x02)
                                                    {
                                                        tmpMsgType = SDOMessageTypes.InitiateUploadResponseExpedited;
                                                    }
                                                    // Is it an initiate upload segmented response?
                                                    else if ((canMessage[0] & 0x01) == 0x01)
                                                    {
                                                        tmpMsgType = SDOMessageTypes.InitiateUploadResponse;
                                                    }
                                                    //Jude 22/Feb/08 - else should indicate invalid response not noResponse
                                                    else
                                                    {
                                                        tmpMsgType = SDOMessageTypes.InvalidResponse;
                                                    }
                                                }
                                                //Jude 22/Feb/08 a response to a message that DW did not send 
                                                //should be ignored with no error set 
                                                //since it could be a display/calibrator message
                                                //hence no else clause
                                                break;
                                            }
                                        #endregion

                                        #region upload segment response
                                        case 0x00:
                                            {
                                                // check the toggle bit matches the last tx packet
                                                if ((canMessage[0] & 0x10) == (VCI.txMsg.data[0] & 0x10))
                                                {
                                                    tmpMsgType = SDOMessageTypes.UploadSegmentResponse;
                                                }
                                                break;
                                            }
                                        #endregion upload segment response

                                        #region initiate download response
                                        case 0x60:
                                            {
                                                // check that the index and sub-index match the tx packet
                                                if
                                                    (
                                                    (VCI.txMsg.data[1] == canMessage[1])
                                                    && (VCI.txMsg.data[2] == canMessage[2])
                                                    && (VCI.txMsg.data[3] == canMessage[3])
                                                    )
                                                {
                                                    tmpMsgType = SDOMessageTypes.InitiateDownloadResponse;
                                                }
                                                break;
                                            }
                                        #endregion initiate download response

                                        #region download segment response
                                        case 0x20:
                                            {
                                                // check the toggle bit matches the last tx packet
                                                if ((canMessage[0] & 0x10) == (VCI.txMsg.data[0] & 0x10))
                                                {
                                                    tmpMsgType = SDOMessageTypes.DownloadSegmentResponse;
                                                }
                                                break;
                                            }
                                        #endregion download segment response

                                        #region download block response
                                        case 0xa0:
                                            {
                                                switch (canMessage[0] & 0x03)
                                                {
                                                    case 0x00:
                                                        {
                                                            #region case 0x00
                                                            // check that the index and sub-index match the tx packet
                                                            if
                                                                (
                                                                (VCI.txMsg.data[1] == canMessage[1])
                                                                && (VCI.txMsg.data[2] == canMessage[2])
                                                                && (VCI.txMsg.data[3] == canMessage[3])
                                                                )
                                                            {
                                                                tmpMsgType = SDOMessageTypes.InitiateBlockDownloadResponse;
                                                            }
                                                            break;
                                                            #endregion case 0x00
                                                        }

                                                    case 0x01:
                                                        {
                                                            tmpMsgType = SDOMessageTypes.EndDownloadBlockResponse;
                                                            break;
                                                        }

                                                    case 0x02:
                                                        {
                                                            VCI.BlockSgmentResponses++;
                                                            break;
                                                        }
                                                }

                                                break;
                                            }
                                        #endregion

                                        #region upload block response (not tested)
                                        //								case 0xa0:
                                        //								{
                                        //									switch ( txMsg.data[ 0 ] & 0x03  )
                                        //									{
                                        //										case 0x00:
                                        //										{
                                        #region 0x00
                                        //											// check scs of reply
                                        //											if ( ( pObj[ iCanNo ].a_data[ 0 ] & 0x03 ) == 0xa0 )
                                        //											{
                                        //												// check that the index and sub-index match the tx packet
                                        //												if
                                        //													(
                                        //													( txMsg.data[ 1 ] == pObj[ iCanNo ].a_data[ 1 ] )
                                        //													&& ( txMsg.data[ 2 ] == pObj[ iCanNo ].a_data[ 2 ] )
                                        //													&& ( txMsg.data[ 3 ] == pObj[ iCanNo ].a_data[ 3 ] )
                                        //													)
                                        //												{
                                        //													_rxResponseType = SDOMessageTypes.InitiateBlockUploadResponse;
                                        //												}
                                        //											}
                                        //											break;
                                        #endregion 0x00
                                        //										}
                                        //
                                        //										case 0x02:
                                        //										case 0x03:
                                        //										{
                                        //											_rxResponseType = SDOMessageTypes.UploadBlockSegmentResponse;
                                        //											break;
                                        //										}
                                        //									}
                                        //									break;
                                        //								}
                                        #endregion

                                        #region abort response
                                        case 0x80:
                                            {
                                                tmpMsgType = SDOMessageTypes.AbortResponse;
                                                break;
                                            }
                                        #endregion

                                        #region default (unknown response type)
                                        default:
                                            {
                                                tmpMsgType = SDOMessageTypes.UnknownResponseType;
                                                break;
                                            }
                                        #endregion
                                    } // end of switch statement
                                    #endregion

                                    #region copy received data over
                                    if (tmpMsgType != SDOMessageTypes.NoResponse)
                                    {
                                        for (int b = 0; b < SCCorpStyle.CANMessageDataLengthMax; b++)
                                        {
                                            VCI.rxMsg.data[b] = canMessage[b];
                                        }
                                    #endregion

                                        #region save id and data length into rxMsg
                                        VCI.rxMsg.id = canMessage.Identifier;
                                        VCI.rxMsg.length = canMessage.DataLength;
                                        VCI.rxResponseType = tmpMsgType;
                                    }
                                        #endregion

                                } // end of if COB-ID matches expected response

                            }
                        }
                        #endregion SDOs

                        else if ((VCI.MonitoringCOBs != null)
                            && (canMessage.Identifier > SCCorpStyle.COBForEmergencyMaximum)  //above Sync
                            && (canMessage.Identifier < 0x580)) //lower than SDODs  - PDO range
                        {
                            #region PDO - calbrated graphing
                            foreach (COBObject COB in VCI.MonitoringCOBs)
                            {
                                if (COB.COBID == canMessage.Identifier)
                                {
                                    #region convert PDO CAN frame databytes from little endian bytes to a long value
                                    long PDOdata = 0;
                                    for (int i = 0; i < SCCorpStyle.CANMessageDataLengthMax; i++)
                                    {
                                        PDOdata <<= 8;
                                        PDOdata += canMessage[7 - i];
                                    }
                                    #endregion  convert PDO CAN frame databytes from little endian bytes to a long value

                                    #region overwrite all those items that are contained in this recevied PDO
                                    foreach (ODItemAndNode itemAndNode in VCI.OdItemsBeingMonitored)
                                    {
                                        if (itemAndNode.ODparam.monPDOInfo.COB == COB)  //this OD item is contained in this received PDO
                                        {//extract the corrent PDO mapping bits and convert to parameter (unscaled) vlaue

                                            long measuredVal = (PDOdata & itemAndNode.ODparam.monPDOInfo.mask) >> itemAndNode.ODparam.monPDOInfo.shift;

                                            if (itemAndNode.ODparam.bitSplit != null)
                                            { //get the vlaue of just the bitsplit part
                                                long bsVal = (measuredVal & itemAndNode.ODparam.bitSplit.bitMask) >> itemAndNode.ODparam.bitSplit.bitShift;

                                                itemAndNode.ODparam.measuredDataPoints.Add(new DataPoint(canMessage.TimeStamp, bsVal));//store history for plotting
                                                itemAndNode.ODparam.currentValue = bsVal;  //set the current value - this is going to make text monitoring much easier
                                            }
                                            else
                                            {
                                                itemAndNode.ODparam.measuredDataPoints.Add(new DataPoint(canMessage.TimeStamp, measuredVal)); //store history for plotting
                                                itemAndNode.ODparam.currentValue = measuredVal;//set the current value - this is going to make text monitoring much easier
                                            }
                                        }
                                    }
                                    #endregion overwrite all those items that are contained in this recevied PDO
                                }
                                break; //found it so get out
                            }
                            #endregion PDO - calbrated graphing
                        }

                        #region diagnostics (which slow it down significantly)
#if CAN_TRAFFIC_DEBUG
                                    StringBuilder wrSB = new StringBuilder();
                                    wrSB.Append("Rx: 0x");
                                    wrSB.Append(canMessage.Identifier.ToString("X"));
                                    wrSB.Append(" : ");
                                    for (int temp = 0; temp < 8; temp++)
                                    {
                                        wrSB.Append("0x");
                                        string tempStr = canMessage[temp].ToString("X").PadLeft(2, '0');
                                        wrSB.Append(tempStr);
                                        wrSB.Append(" ");
                                    }
                                    wrSB.Append(" Time: ");
                                    wrSB.Append(System.DateTime.Now.TimeOfDay.ToString());
                                    System.Console.Out.WriteLine(wrSB.ToString());
                                    //				System.Diagnostics.Debug.WriteLine( writeString);
#endif
                        #endregion

                        #endregion switch message type
                    }
                }
            }
        }
        #endregion VCI3 Receive handler

        #endregion receive handlers

        #region transmit handler

        //-------------------------------------------------------------------------
        //  Name			: transmit()
        //  Description     : This function transmits the build up message passed as
        //					  parameters onto the CANbus at the latest configured
        //					  baud rate.
        //  Parameters      : id - COB-ID used to transmit the message
        //					  length - data field length (maximum of 8)
        //					  data[] - array of the data bytes
        //  Used Variables  : BoardHdl - handle given when IXXAT adapter was opened
        //					  TxQueHdl - transmit buffer previously configured on the
        //								  IXXAT adapter
        //  Preconditions   : THe IXXAT adapter has been opened & configured and
        //					  started (put into the running state).
        //  Post-conditions : The message sent as parameters has been transmitted 
        //					  onto the CANbus or there is a failure reason.
        //  Return value    : fbc - indicating success or giving a failure reason.
        //----------------------------------------------------------------------------
        ///<summary>Transmits a previously build up message on the CAN using the IXXAT adapter.</summary>
        /// <param name="id">COB-ID used to transmit the message</param>
        /// <param name="length">data field length (maximum of 8)</param>
        /// <param name="data">array of data bytes</param>
        /// <returns>feedback indicates success or gives a failure reason</returns>
        public VCIFeedbackCode transmit(uint id, byte length, byte[] data)
        {
            VCIFeedbackCode fbc = VCIFeedbackCode.VCI_OK;
         
            //Jude 22/Feb/08 we should reset the response type before 
            //we copy data to txMsg. This prevents the Receive handler being 
            //out of sync with the transmit handler
            // Reset response type variable back to no response, ready to record the next reply.
            _rxResponseType = SDOMessageTypes.NoResponse;

            if (mWriter == null)
            {
                fbc = VCIFeedbackCode.VCI_ERR;
            }
            else
            {
                int retries = 0;


                CanMessage txCanMsg = new CanMessage();             //tsn - create can message

                txCanMsg.TimeStamp = 0;                             //tsn
                txCanMsg.Identifier = id;                           //tsn
                txCanMsg.FrameType = CanMsgFrameType.Data;          //tsn
                txCanMsg.DataLength = length;                       //tsn

                for (Byte i = 0; i < txCanMsg.DataLength; i++)      //tsn
                {
                    txCanMsg[i] = data[i];                          //tsn
                }
                #region port debugging
#if PORT_ACCESS	//timing test code
				    this.setPin( 0 );
#endif
                #endregion port debugging
                bool TxOK = false;
                do
                {
                    // Write the CAN message into the transmit FIFO         //tsn
                    TxOK = mWriter.SendMessage(txCanMsg);
                    if (TxOK == false)
                    {
                        retries++;
                    }
                }
                while ((TxOK == false) && (retries < 10));
                #region port debugging
#if PORT_ACCESS	//timing test code
				    this.clearPin( 0 );
#endif
                #endregion port debugging

#if CAN_TRAFFIC_DEBUG
                #region diagnostics (takes too long to have in all the time)
                if (TxOK == true)
                {
                    StringBuilder wrSB = new StringBuilder();
                    wrSB.Append("Tx: ");
                    wrSB.Append(id.ToString("X"));
                    wrSB.Append(":");
                    for (int temp = 0; temp < data.Length; temp++)
                    {
                        string tempStr = data[temp].ToString("X");
                        if (tempStr.Length < 2)
                        {
                            tempStr = "0" + tempStr;
                        }
                        wrSB.Append(tempStr);
                    }
                    wrSB.Append(" Time: ");
                    wrSB.Append(System.DateTime.Now.TimeOfDay.ToString());
                    System.Console.Out.WriteLine(wrSB.ToString());
                    //System.Diagnostics.Debug.WriteLine( writeString);
                }
                #endregion
#endif
            }

            #region Keep copy of last message transmitted (use to check for correct response).
            txMsg.id = id;
            txMsg.length = length;
            for (int i = 0; i < SCCorpStyle.CANMessageDataLengthMax; i++)
            {
                txMsg.data[i] = data[i];
            }
            #endregion

            return (fbc);
        }

        #endregion  transmit handlers

        #region CAN debug
#if PORT_ACCESS	//timing test code
		public void resetPins() // Makes all the data pins low so the LED's turned off
		{
			PortAccess.Output(888, 0);
		}

		public void setPin( int pin )
		{
			if ( pin < 8 )
			{
				switch( pin )
				{
					case 0:
						pinValue |= 0x01;
						break;
					case 1:
						pinValue |= 0x02;
						break;
					case 2:
						pinValue |= 0x04;
						break;
					case 3:
						pinValue |= 0x08;
						break;
					case 4:
						pinValue |= 0x10;
						break;
					case 5:
						pinValue |= 0x20;
						break;
					case 6:
						pinValue |= 0x40;
						break;
					case 7:
						pinValue |= 0x80;
						break;
				}

				PortAccess.Output( 888, pinValue );
			}
		}

		public void clearPin( int pin )
		{
			if ( pin < 8 )
			{
				switch( pin )
				{
					case 0:
						pinValue &= ~0x01;
						break;
					case 1:
						pinValue &= ~0x02;
						break;
					case 2:
						pinValue &= ~0x04;
						break;
					case 3:
						pinValue &= ~0x08;
						break;
					case 4:
						pinValue &= ~0x10;
						break;
					case 5:
						pinValue &= ~0x20;
						break;
					case 6:
						pinValue &= ~0x40;
						break;
					case 7:
						pinValue &= ~0x80;
						break;
				}

				PortAccess.Output( 888, pinValue );
			}
		}
#endif
        #endregion

      }
}

