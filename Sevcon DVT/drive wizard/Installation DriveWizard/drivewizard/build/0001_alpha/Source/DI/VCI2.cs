/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.62$
	$Version:$
	$Author:jw$
	$ProjectName:DriveWizard$ 

	$RevDate:18/02/2008 15:39:34$
	$ModDate:18/02/2008 15:32:22$

ORIGINAL AUTHOR
    Alison King

DESCRIPTION
	VCI interface class for the IXATT USB-CAN adapter.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36683: VCI2.cs 

   Rev 1.62    18/02/2008 15:39:34  jw
 Merge recovery form hibernation code into single method for VCI2 back
 compatibility. Imporved hibernation recovery ( VCI2 works OK, VCI3 -
 intiSocket throwing exception still)


   Rev 1.61    18/02/2008 14:18:28  jw
 VCI2 CAN methods renamed and merged for consistency with VCI3 - towards back
 compatibility


   Rev 1.60    18/02/2008 11:40:06  jw
 VCI2 CAN methods renamed and merged for consistency with VCI3 - towards back
 compatibility


   Rev 1.59    18/02/2008 09:28:54  jw
 VCI2 CAN methods moved form communications to VCI 2 and renamed for
 consistency with VCI3 - towards back compatibility


   Rev 1.58    18/02/2008 07:48:10  jw
 Static params changed to non-static on VCI for conformity with VCI2. VCI2
 badurate param renamed to be as per VCI3 - step towards full backwards
 compatibility


   Rev 1.57    15/02/2008 11:43:52  jw
 Params for CAN adapter hardware intialised an drunning changed to same ones
 used for VCI3 - step towards full backwards compatibility


   Rev 1.56    12/02/2008 08:49:36  jw
 Ongoing VCI3 work. Options and Select profiel windows changed to simplify
 threading and improve feedback.  Prog bar vlaue determination line made
 exception proof. Max and current values used by progress bars determined
 within DI for encapsulation and values reflect activitiy better.


   Rev 1.55    06/02/2008 08:13:38  jw
 DR38000239. Was setting SRO Command specier to defualt invalid before setting
 to correcxt vlaue - was possilby to poll between the two causing comms
 problems on newer laptops


   Rev 1.54    01/02/2008 07:31:30  jw
 Duplicate node ID check refined and put back in. We need to compare the
 recieved OD index and sub but NOPT the command specifier to accomodate
 devices eg bootloader that do not contain a mandatory item.


   Rev 1.53    25/01/2008 10:46:40  jw
 VCI3 and Vista functionality files merge - more testing required


   Rev 1.52    05/12/2007 21:13:36  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using DriveWizard;

#if USING_VCI3
#else
//jude changed for IXXAT to Ixxat fo rconformity with VCI3 code 
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

	#region VCI DLL import
	
	#region VCI CAN object import structure definition
	/// <summary>
	/// VCI CAN object import structure definition
	/// Specific format to import functions and structures from the Windows API format of
	/// the VCI DLL into a suitable format for C# to call and use.  This is calling
	/// unmanaged and, therefore, unsafe code.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)] 
	public struct VCI_CAN_OBJ
	{
		/// <summary>CAN message time stamp (in 100usec per bit, absolute time)</summary>
		public uint     time_stamp;

		/// <summary>COB ID of the CAN message</summary>
		public uint     id;
		
		/// <summary>CAN message data length</summary>
		public byte     len_rtr;
		
		/// <summary>CAN message data bytes array</summary>
		[MarshalAs (UnmanagedType.ByValArray,SizeConst=8)] public byte[] a_data;   
		
		/// <summary>CAN message sts</summary>
		public byte     sts;
	}
	#endregion

	#region XAT board configuration import structure definition
	/// <summary>XAT board configuration import structure definition</summary>
	[StructLayout (LayoutKind.Sequential,Pack=1)] 
	public class XAT_BoardCFG
	{
		/// <summary>IXXAT board configuration board number handle</summary>
		public short	board_no;

		/// <summary>IXXAT board configuration board type </summary>
		public int		board_type;
		
		/// <summary>IXXAT board configuration board name string identifier</summary>
		[MarshalAs (UnmanagedType.ByValTStr,SizeConst=255)] public String sz_brd_name;   
		
		/// <summary>IXXAT board configuration manufacturer string identifier</summary>
		[MarshalAs (UnmanagedType.ByValTStr,SizeConst=50)] public String sz_manufacturer;
		
		/// <summary>IXXAT board configuration board information string identifier</summary>
		[MarshalAs (UnmanagedType.ByValTStr,SizeConst=50)] public String sz_brd_info;
		
		/// <summary>IXXAT board configuration card add string identifier</summary>
		[MarshalAs (UnmanagedType.ByValTStr,SizeConst=255)] public String sz_CardAddString;
	}
	#endregion

	#region XAT additional information object import structure definition
	/// <summary> XAT additional information object import structure definition </summary>
	[StructLayout (LayoutKind.Sequential,Pack=1)] 
	public class XAT_addinfo
	{
		/// <summary>IXXAT board configuration additional information string identifier </summary>
		[MarshalAs (UnmanagedType.ByValTStr,SizeConst=255)] public String s_addinfo;
	}
	#endregion

	#region VCI mapper object import structure definition
	/// <summary> VCI mapper object import structure definition</summary>
	[StructLayout (LayoutKind.Sequential,Pack=1)] 
	public class VCI_MAPPER
	{
		#region delegate functions
		/// <summary>VCI delegate function to display a message string to the user</summary>
		public delegate void VCI_t_PutS         ( string s );

		/// <summary>VCI user exception error handler</summary>
		public delegate void VCI_t_UsrExcHdlr   ( int func_num, int err_code, ushort ext_err, string s);
		
		/// <summary>VCI callback function </summary>
		public delegate void VCI_t_XatCallback  ( uint iIndex, uint hwKey, string sName, string sValue, string sValueHex);
		
		/// <summary>VCI callback function interrupt when a CAN message is received </summary>
		public delegate void VCI_t_UsrRxIntHdlr ( ushort que_hdl, ushort count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] VCI_CAN_OBJ[] datas);
		#endregion

		#region Import DLL functions
		/// <summary>Initialisation of the VCI structures without board initialisation </summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern void VCIS_Init();

		/// <summary>Requests the use of the PC/CAN interface from the VCI </summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern int VCIS2_PrepareBoard(int VCI_Board_TYPE, short BoardIndex,[Out] XAT_addinfo s_addinfo, byte b_addLength,VCI_t_PutS x,VCI_t_UsrRxIntHdlr y, VCI_t_UsrExcHdlr z);

		/// <summary>Initialisation of the timing register, corresponding to the Philips SJA 1000 </summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern int VCIS_InitCan(ushort boardhdl, byte can_num,byte bt0, byte bt1, byte mode);

		/// <summary>Resets the CAN controllers and thus stops CAN communications via the CANcontroller.</summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern int VCIS_ResetCan(ushort boardhdl, byte can_num );

		/// <summary>Setting of the acceptance mask register of the CAN controllers for global message
		///  filtering in 11 bit or 29 bit mode</summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern int VCIS_SetAccMask(ushort boardhdl, byte can_num,ushort acc_code, ushort acc_mask);

		/// <summary>Creation of a transmit and receive queue. A handle is returned under which
		/// the queues can be addressed. </summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern int VCIS_ConfigQueue(ushort boardhdl, byte can_num, byte que_type, ushort que_Size, ushort int_limit, ushort int_time, ushort ts_res, [In, Out] [MarshalAs(UnmanagedType.LPArray)]  ushort[] p_que_hdl);
		
		/// <summary>Creation of a receive or a remote buffer.Access to this buffer is via
		/// the returned handle. </summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern int VCIS_ConfigBuffer(ushort boardhdl, byte can_num, byte que_type, uint id,[In, Out] [MarshalAs(UnmanagedType.LPArray)]  ushort[] p_buf_hdl);
		
		/// <summary>Assignement/blocking of receive messages to the given receive queue.
		/// Identifier groups are definable via the mask. </summary>
		[DllImport("VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		protected static extern int VCIS_AssignRxQueObj(ushort boardhdl, ushort que_hdl, byte mode, uint id, uint mask);
		
		/// <summary>Starts the given CAN controllers. </summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern int VCIS_StartCan(ushort boardhdl, byte can_num);
		
		/// <summary> Cancel the board with the VCI. This involves resetting the interface
		/// and the CAN controller as well as uninstalling interrupts used.  The board handle
		/// becomes free again.</summary>
		[DllImport("VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		protected static extern int VCIS_CancelBoard(ushort boardhdl);
		
		/// <summary>Reading of the first entry/entries of a Receive queue.  Number of entries
		/// is given by count. </summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern int VCIS_ReadQueObj(ushort boardhdl, ushort que_hdl, ushort count, [In, Out]VCI_CAN_OBJ[] p_obj);

		/// <summary>Sending of a CAN message via the given send queue.  If the queue is full,
		/// the message must be repeated later.  If a transmit error is returned, the CAN
		/// controller was not able to transmit the message. </summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern int VCIS_TransmitObj(ushort board_hdl, ushort que_hdl, uint id, byte len, byte[] a_data);

		/// <summary>Shows a dialog for selection of the PC/CAN interface.  The configuration
		/// selected by the user is deposited in the structure indicated by the parameter pConfig. </summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern int XATS_SelectHardware(int hWndOwner, [Out] XAT_BoardCFG pConfig);

		/// <summary>Enumerates all registered IXXAT PC/CAN interfaces.  For each entry, the callback
		/// function transferred in the parameter fp_callback  is called up. </summary>
		[DllImport( @"resources\VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall )]
		protected static extern int XATS_EnumHwEntry  ( VCI_t_XatCallback fp_callback, uint pClass);

		/// <summary>Reset of the timers for the time stamps of the Receive queues. </summary>
		[DllImport("VCIcsharp.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		protected static extern void VCIS_ResetTimeStamp(ushort boardhdl);
		#endregion
	}
	#endregion
	#endregion

	#region DriveWizard structure definition
	#region CAN message structure definition
	/// <summary>
	/// CAN message data structure definition.
	/// Structure used to hold the received or transmitted CAN message data with formatting. 
	/// </summary>
	public struct CANMessage
	{
		/// <summary>in 100us/bit from time the adapter was started (relative stamping).</summary>
		public  uint	timeStamp;

		/// <summary>COB ID of message received or transmitted.</summary>
		public	uint	id;
		
		/// <summary>data length which is always 8 for all SDO message types.</summary>
		public	byte	length;
		
		/// <summary>data bytes which are received or transmitted.  The format of this is
		/// dependent on the SDO message type. See DS301.</summary>
		public	byte [] data;
	};
	#endregion

	#region heartbeat message information structure definition
	/// <summary>Heartbeat message information. </summary>
	public struct HbInfo
	{
		/// <summary>from the COBID we can extract the node ID of the device that sent the heartbeat message</summary>
		public uint		COBID;

		/// <summary> data byte 1 contains the current state of the device that sent this heartbeat message </summary>
		public int		db1;
	};
	#endregion
	#endregion

	/// <summary>
	/// VCI mapping class.  This class maps in all the IXXAT VCI DLL functions and handles
	/// the interface between the DriveWizard DI and the IXXAT CAN adapter via the VCI DLLs.
	/// </summary>
	public class VCI_V216 : VCI_MAPPER
	{
		#region private variable declarations

		// holds the returned board handle given when the IXXAT adapter was opened.
		private ushort BoardHdl;

		#region transmit and receive buffer memory allocations.
		private const int NumRxBuffers = 1;
		private ushort[] TxQueHdl = new ushort[1];
		private ushort[] RxQueHdl = new ushort[1];
		private ushort[] BufHdl   = new ushort[NumRxBuffers];
		private ushort[] BufHdl2  = new ushort[1];
		#endregion

		#region instances of objects required for VCI DLLs.
		//private VCI_t_PutS			messageHandler;		// not currently used by DW
		private VCI_t_UsrExcHdlr	exceptionHandler;
		private VCI_t_UsrRxIntHdlr	receiveQueuedata;
		private VCI_t_XatCallback	xatCallback;
		#endregion

		// contains the last message that was built up and transmitted to a controller.
		private CANMessage			txMsg;

		// number of node IDs which had a valid SDO reply (when finding the system)

		#region emergency and heartbeat variables for FIFOs and delegates
		private static onNewEmergency emcyEventDelegate = null;
		private static readonly int sizeOfFIFOs = 100;		
		private static int emcyFIFOIn = 0;
		private static int emcyFIFOOut = 0;
		private static byte [][] emcyFIFO = new byte[ sizeOfFIFOs ][];

		private static onNewHeartbeat hbEventDelegate = null;
		private static int hbFIFOIn = 0;
		private static int hbFIFOOut = 0;
		private static HbInfo []  hbFIFO = new HbInfo[ sizeOfFIFOs ];

		// separate timer thread and timeout for processing EMCY and heartbeat FIFO queues
		private static readonly int EMCYAndHBTimeoutCheck = 500;		// in ms
		private static	System.Threading.Timer threadTimer; 
        internal BaudRate CANAdapterBaud = BaudRate._unknown;

        /* Last acceptance mask and acceptance filter set by DI. These are stored so that if
         * the mask and filter need changing for a specific DI activity, it knows what to set
         * them back to afterwards.
         */
        private ushort _acceptanceMask = 0x00;
        private ushort _acceptanceFilter = 0x00;
        #endregion
	
        #endregion

#region CAN adapter Hardware initialised OK
        private bool _CANAdapterHWIntialised = false;
        internal bool CANAdapterHWIntialised
        {
            get
            {
                return (_CANAdapterHWIntialised);
            }
            set
            {
                _CANAdapterHWIntialised = value;
            }
        }
        #endregion  CAN adapter Hardware initialised OK

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
            set
            {
                _CANAdapterRunning = value;
            }
        }
        #endregion CAN adapter running OK


#if POST_ACCESS	//timing test code
		int pinValue = 0x00;
#endif

#region property declarations
#region last received CAN message
        /* Contains the last message that was received from any of the attached controllers.
		 * This is a read only property.
		 */
		private CANMessage			_rxMsg;
		///<summary>Contains the CAN message type of the last valid received message.</summary>
		public CANMessage			rxMsg
		{
			get
			{
				return ( _rxMsg );
			}
		}
        #endregion

#region SDO message type of the last CAN SDO message received by the interrupt
		/* Contains the SDO message type of the last message that was received from any of the
		 * attached controllers. This is a read only property.
		 */
		private SDOMessageTypes	_rxResponseType;
		///<summary>SDO message type of the last CAN SDO message received by the interrupt.</summary>
		public SDOMessageTypes		rxResponseType
		{
			get
			{
				return ( _rxResponseType );
			}
		}
        #endregion

#region receive message data valid
		/* Flag which indicates whether any valid data (of any description) has been received
		 * from any of the controllers attached to DW via the IXXAT adapter.  This is paert of
		 * the baud rate detection method.  This property can be read and written to.
		 */
		private bool	_rxValidData;
		///<summary>Flag to indicate whether any valid data has been received on the CAN.</summary>
		public bool		rxValidData
		{
			get
			{
				return ( _rxValidData );
			}

			set
			{
				_rxValidData = value;
			}
		}
        #endregion

		public ArrayList nodeIDsFound = new ArrayList();

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
				return ( _messageType );
			}

			set
			{
				_messageType = value;
			}
		}
        #endregion

#region monitoring data (fixed array length) containing all PDO received relevant information.
		/* Fixed length data array, used to hold all the relevant data received from the incoming
		 * PDO CAN messages, needed for graphical monitoring.
		 * This data structure is designed to have a timestamp and a variable length array
		 * of long values (as it is unknown until runtime user selection how many objects have
		 * to be monitored).
		 * Memory is allocated at a fixed length for speed as using a hash table was found to
		 * be too slow.
		 */
		public ArrayList MonitoringCOBs;
        #endregion

#region monitoring paused (set so no longer analyse received PDOs to add data to monitorData)
		private bool _monitoringPaused = false;
		///<summary>Monitoring paused (set so no longer analyse received PDOs to add data to monitorData).</summary>
		public bool monitoringPaused
		{
			get
			{
				return ( _monitoringPaused );
			}

			set
			{
				_monitoringPaused = value;
			}
		}
        #endregion

		public ArrayList OdItemsBeingMonitored;
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
		public void addNewHeartbeatDelegate( onNewHeartbeat onh )
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
		public void addNewEmergencyDelegate( onNewEmergency one )
		{
			emcyEventDelegate = one;
		}
        #endregion

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
		public VCI_V216()
		{
			// create memory for CAN transmit & receive message data bytes
			txMsg.data = new byte [ SCCorpStyle.CANMessageDataLengthMax ];
			_rxMsg.data = new byte [ SCCorpStyle.CANMessageDataLengthMax ];

			// set the receive interrupt to detect SDO messages (default)
			_messageType = CANMessageType.SDO;

			// initialise memory for the emergency message FIFO
			for ( int e = 0; e < emcyFIFO.Length; e++ )
			{
				emcyFIFO[ e ] = new byte[ 9 ];
			}

			// start the thread timer (every 500ms) to check the heartbeat and emergency message FIFOs
			threadTimer = new System.Threading.Timer( new System.Threading.TimerCallback( threadTimerExpired ), null, EMCYAndHBTimeoutCheck, EMCYAndHBTimeoutCheck );
		}

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
		~VCI_V216()
		{
			if ( threadTimer != null )
			{
				threadTimer.Dispose();

				if ( threadTimer != null )
				{
					threadTimer = null;
				}
			}
		}
        #endregion

#region VCI initialisation
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
			resetPins();
#endif

            this.CANAdapterHWIntialised = false;

			// Not used at present in DriveWizard.
			//messageHandler = new VCI_t_PutS ( MessageHandler );
			if(MAIN_WINDOW.isVirtualNodes == false)
			{
			//	exceptionHandler = new VCI_t_UsrExcHdlr( ExceptionHandler );
			}
			// Setup CAN message receive interrupt function callback handler.
			receiveQueuedata = new VCI_t_UsrRxIntHdlr( ReceiveQueuedata );
			xatCallback = new VCI_t_XatCallback ( XatCallback );

			// Create board config and additional information structures.
			XAT_BoardCFG Config = new XAT_BoardCFG();
			XAT_addinfo s_addinfo = new XAT_addinfo();

			// Show all active Boards.
			XATS_EnumHwEntry( xatCallback, 0 );

			try
			{
				// openCANAdapterHW the IXXAT adapter board.
				int fbInt = VCIS2_PrepareBoard(	
					Config.board_type,
					Config.board_no,
					s_addinfo,
					0,
					null,			//messageHandler - not used
					receiveQueuedata,
					exceptionHandler
					);

				// Recast returned value to a ushort. Less than zero is a failure.
                if (fbInt >= 0)
				{
					BoardHdl = (ushort) fbInt;
                    this.CANAdapterHWIntialised = true;
				}
			}
			catch ( Exception e )
			{
                this.CANAdapterHWIntialised = false;
                #region user feedback
                SystemInfo.errorSB.Append("Could not open IXXAT adapter.\nIxxat Error Code: ");
                SystemInfo.errorSB.Append(e.Message);
                SystemInfo.errorSB.Append("\nPlease check your USB port connection");
                SystemInfo.errorSB.Append("\nand that the drivers have been installed for this port.");
                SystemInfo.errorSB.Append("\nIf all LEDs on the adapter are extinguished, disconnect and reconnect the adapter\nto the PC");
                SystemInfo.errorSB.Append("\nand then retry.\n");
                #endregion user feedback
			}
		}

		//-------------------------------------------------------------------------
		//  Name			: initialiseBaudAndMask()
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
		//  Preconditions   : The IXXAT adapter must already be in pre-operational state.
		//  Post-conditions : The IXXAT adapter has been configured.
		//  Return value    : fbc - indicates configured OK or gives a failure reason.
		//----------------------------------------------------------------------------
		///<summary>Initialises the IXXAT adapter CAN baud rate, acceptance mask and filter.</summary>
		/// <param name="baud">enumerated type indicating the baud rate which the adapter has to run at</param>
		/// <param name="acceptanceMask">acceptance mask to be used by the adapter to filter out and 
		/// reduce the interrupt loading on the PC processor</param>
		/// <param name="acceptanceFilter">acceptance filter to be used by the adapter</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public VCIFeedbackCode initialiseBaudAndMask( BaudRate baud, ushort acceptanceMask, ushort acceptanceFilter )
		{
#region local variable declaration
			VCIFeedbackCode fbc = VCIFeedbackCode.VCI_OK;
			byte bt0;
			byte bt1;
            #endregion
			

			// Initialise the CAN-Controller baud rate to any of the CANopen possibles.
			switch( baud )
			{
#region 10K baud
				case BaudRate._10K:
				{
					bt0 = 0x31;
					bt1 = 0x1c;
					break;
				}
                    #endregion

#region 20K baud
				case BaudRate._20K:
				{
					bt0 = 0x18;
					bt1 = 0x1c;
					break;
				}
                    #endregion

#region 50K baud
				case BaudRate._50K:
				{
					bt0 = 0x09;
					bt1 = 0x1c;
					break;
				}
                    #endregion

#region 100K baud
				case BaudRate._100K:
				{
					bt0 = 0x04;
					bt1 = 0x1C;
					break;
				}
                #endregion

#region 125K baud
				case BaudRate._125K:
				{
					bt0 = 0x03;
					bt1 = 0x1C;
					break;
				}
                    #endregion

#region 250K baud
				case BaudRate._250K:
				{
					bt0 = 0x01;
					bt1 = 0x1C;
					break;
				}
                    #endregion

#region 500K baud
				case BaudRate._500K:			
				{
					bt0 = 0x00;
					bt1 = 0x1C;
					break;
				}
                    #endregion

#region 800K
				case BaudRate._800K:
				{
					bt0 = 0x00;
					bt1 = 0x16;
					break;
				}
                    #endregion

#region 1M baud and default
				case BaudRate._1M:
				default:
				{
					bt0 = 0x00;
					bt1 = 0x14;
					break;
				}
                    #endregion
			}

			try
			{
				// Initialise to 11B (not 29B) which was tested on CANalyser
				fbc = (VCIFeedbackCode)VCIS_InitCan( 
					BoardHdl,	// board handle
					0x0,		// CAN controller number
					bt0,		// bit timing 0
					bt1,		// bit timing 1
					0x00		// 29 bit extended COB ID
					);

				// Definition of acceptance mask and filter.
				if ( fbc == VCIFeedbackCode.VCI_OK )
				{
					fbc = (VCIFeedbackCode)VCIS_SetAccMask( 
						BoardHdl,		// board handle
						0,				// CAN controller number
						acceptanceMask, // acceptance mask
						acceptanceFilter // acceptance filter
						);
                    this.CANAdapterBaud = baud; //only set on Success that way if we fail then Communications layyer will force another try
				}
			}
			catch ( Exception e )
			{
				SystemInfo.errorSB.Append("Unable to initialise IXXAT dongle.\nException: ");
				SystemInfo.errorSB.Append( e.Message );
			}

            if (fbc == VCIFeedbackCode.VCI_OK)
            {
                _acceptanceFilter = acceptanceFilter;
                _acceptanceMask = acceptanceMask;
            }
			return( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: initialiseBuffers()
		//  Description     : Configures the transmit and receive buffers on the IXXAT
		//					  adapter.
		//  Parameters      : None
		//  Used Variables  : BufHdl[] - array of configured buffers
		//					  BufHdl2[] - temporary place holder for configuring buffers
		//  Preconditions   : The IXXAT adapter has been opened OK & given a handle.
		//  Post-conditions : The transmit and receive buffers have been configured,
		//					  ready for use.
		//  Return value    : fbc - indicates configured OK or gives a failure reason.
		//----------------------------------------------------------------------------
		///<summary>Configures the transmit and receive buffers required by the IXXAT adapter.</summary>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public VCIFeedbackCode initialiseBuffers( )
		{
#region local variable declarations
			uint i;
			VCIFeedbackCode fbc = VCIFeedbackCode.VCI_OK;
            #endregion

			try
			{
#region Definition of the Transmit Queue.
				fbc = (VCIFeedbackCode)VCIS_ConfigQueue( 
					BoardHdl,		// board handle
					0x0,			// CAN controller number
					0x0,			// queue type TX_QUE=transmit
					SCCorpStyle.sizeOfTransmitBuffer, // size of queue
					0,				// no. CAN msgs which trigger interrupt (na for tx)
					0,				// time in ms when trigger interrupt (na for tx)
					0,				// timestamp resolution in us
					TxQueHdl		// handle of queue (transmit in this case)
					);  
                #endregion

#region Definition of the Receive Buffers.
				for ( i = 0; i < NumRxBuffers; i++ )
				{
					fbc = (VCIFeedbackCode)VCIS_ConfigBuffer( 
						BoardHdl,		// board handle
						0x0,			// CAN controller number
						0x0,			// buffer type (RX_QUE)
						(uint)SCCorpStyle.sizeOfTransmitBuffer + i,  // buffer identifier (start after last tx buffer)
						BufHdl2			// handle to buffer (rx in this case)
						);  
					BufHdl[ i ] = BufHdl2[ 0 ];
				}
                #endregion
			
#region Definition of the Receive Queue (interrupt mode) if rx buffers defined OK.
				if ( fbc == VCIFeedbackCode.VCI_OK )
				{
					// CAN_NUM, RX_QUE,SIZE,Limit,Time,timestamp_Resolution, handle of Queue
					fbc = (VCIFeedbackCode)VCIS_ConfigQueue( 
						BoardHdl,		// board handle
						0x0,			// CAN controller number
						0x1,			// queue type RX_QUE=receive
						SCCorpStyle.sizeOfReceiveBuffer, // size of queue
						1,				// no. CAN msgs which trigger interrupt
						0,				// time in ms when trigger interrupt
						100,			// timestamp resolution in us
						RxQueHdl		// handle of queue (receive in this case)
						);  
				}
                #endregion
			
#region Assign the ID to the Receive Que if queues configured OK.
				if ( fbc == VCIFeedbackCode.VCI_OK )
				{
					fbc = (VCIFeedbackCode)VCIS_AssignRxQueObj( 
						BoardHdl,		// board handle
						RxQueHdl[0],	// queue handle
						0x1,			// mode (release/blocking messages)
						0x0,			// identifier of message
						0x0				// mask for relevant identifier (0=don't care)
						);  
				}
                #endregion
			}
			catch ( Exception e )
			{
				SystemInfo.errorSB.Append( "Could not initialise IXXAT buffers.\nException: ");
					SystemInfo.errorSB.Append( e.Message );
			}

			return( fbc );
		}
	  
        #endregion

#region start, restart & closing of CAN adapter


		//-------------------------------------------------------------------------
		//  Name			: resetCAN()
		//  Description     : Calls the DLL's reset CAN method to put the IXXAT 
		//					  adapter back into pre-operational state so that it
		//					  can be reconfigured (e.g. baud rate).
		//  Parameters      : None
		//  Used Variables  : BoardHdl - IXXAT board handle assigned when it was opened.
		//  Preconditions   : The IXXAT adapter has been opened.
		//  Post-conditions : The IXXAT adapter is now in the pre-operational state.
		//  Return value    : fbc - indicating whether the adapter was reset OK or
		//					        returning a failure reason.
		//----------------------------------------------------------------------------
		///<summary>Resets the IXXAT adapter to put it back into the pre-operational state (to allow reconiguration).</summary>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public void resetCAN()
		{
            if (this.CANAdapterRunning == true)
            {
                try
                {
                    VCIFeedbackCode fbc = (VCIFeedbackCode)VCIS_ResetCan(
                        BoardHdl,	// board handle
                        0x0			// CAN controller number
                        );
                    if (fbc == VCIFeedbackCode.VCI_OK)
                    {
                        this.CANAdapterRunning = false;
                    }
                }
                catch (Exception e)
                {
                    this.CANAdapterRunning = true;
                    SystemInfo.errorSB.Append("Could not stop IXXAT adapter.\nIxxat error code: ");
                    SystemInfo.errorSB.Append(e.Message);
                }
            }
		}

		//-------------------------------------------------------------------------
		//  Name			: startCAN()
		//  Description     : Starts the IXXAT adapter and puts it into operational
		//					  mode ie running state after it has been initialised.
		//  Parameters      : None
		//  Used Variables  : BoardHdl - handle given when the IXXAT adapter was opened
		//  Preconditions   : The IXXAT adapter has been opened, the receive interrupt
		//					  handler has been setup, the receive and transmit buffers
		//					  have been configured and the acceptance mask and filter
		//					  has been setup.
		//  Post-conditions : The IXXAT adapter is running.
		//  Return value    : fbc indicating whether the IXXAT was started OK or giving
		//					  a failure reason.
		//----------------------------------------------------------------------------
		///<summary>Starts the IXXAT adapter running (ie puts into the operational mode).</summary>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public void startCAN()
		{
            this.CANAdapterRunning = false;
			try
			{
				// Start the CAN (put from pre-op into operational).
                VCIFeedbackCode fbc = (VCIFeedbackCode)VCIS_StartCan(
					BoardHdl,		// board handle
					0x0				// CAN controller number
					);
                if (fbc == VCIFeedbackCode.VCI_OK)
                {
                    this.CANAdapterRunning = true;
                }
			}
			catch ( Exception e )
			{
                this.CANAdapterRunning = false;
				SystemInfo.errorSB.Append("\nCould not reset IXXAT adapter");
				SystemInfo.errorSB.Append("Error code ");
				SystemInfo.errorSB.Append(e.Message);
			}
		}
	    
		//-------------------------------------------------------------------------
		//  Name			: resetTimeStamp()
		//  Description     : This function calls the VCI function from the IXXAT DLL
		//					  to reset the timer on the IXXAT adapter, which is used
		//					  to provide a timestamp on all received messages.  THis
		//					  is an absolute time in 100us. Resetting puts it back
		//					  to zero.
		//  Parameters      : None
		//  Used Variables  : None
		//  Preconditions   : The IXXAT adapter is initialised.
		//  Post-conditions : The timer used to timestamp received messages on the IXXAT
		//					  adapter has been reset to zero.
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>Resets the IXXAT adapter timer to zero, used for timestamping received CAN messages.</summary>
		public void resetTimeStamp()
		{
			try
			{
				VCIS_ResetTimeStamp( 
					BoardHdl		// board handle
					);
			}
			catch ( Exception e )
			{
				SystemInfo.errorSB.Append("\nFailed to reset IXXAT adaptertimestamp");
				SystemInfo.errorSB.Append(" error code");
				SystemInfo.errorSB.Append(e.Message );
			}
		}


        //---------------------------------------------------------------------------
        // Name			: SetupCAN()
        // Description     : This initialises the IXXAT dongle to the baud rate passed
        //					  as a parameter with the acceptance mask and filter given.
        //					  To do this, it must reset the dongle if it is already running
        //					  (back into pre-op mode as opposed to powering the dongle
        //					  down and back up again).  Or it must initialse the hardware
        //					  if it has not already been opened.  Once in pre-op mode,
        //					  the baud rate and acceptance mask and filter are configured
        //					  to those values passed as parameters.  Finally, the dongle
        //					  is put back into operational mode.
        //  Parameters      : baud - reqiured baud rate to run the CANbus at
        //					  acceptanceMask - acceptance mask to limit rx packets to
        //									   only those of interest to DW (DW runs
        //									   quicker with less rx interrupts)
        //				      acceptanceFilter - acceptance filter for rx packets
        //  Used Variables  : VCIrunning - bool indicating whether the IXXAT dongle is
        //								   in operational mode already
        //					  hardwareOpened - bool indicating whether the IXXAT dongle
        //									   has had it's hardware opened & initialised
        //  Preconditions   : The IXXAT dongle must already have been started & is running
        //  Post-conditions : The IXXAT dongle has been reset, configured with the new baud 
        //					  rage and filters and has started running again (op mode).
        //  Return value    : fbc - indicates success or failure reason
        //---------------------------------------------------------------------------
        ///<summary>Initialises the acceptance mask and filter to accept all messages then restarts the IXXAT adapter.</summary>
        /// <param name="baud">reqiured baud rate to run the CANbus at</param>
        /// <param name="acceptanceMask">acceptance mask to limit rx packets to only those of 
        /// interest to DW (DW runs quicker with less rx interrupts)</param>
        /// <param name="acceptanceFilter">acceptance filter for rx packets</param>
        /// <returns>feedback indicates success or gives a failure reason</returns>
        public void SetupCAN(BaudRate newBaud, ushort newAcceptanceMask, ushort newAcceptanceFilter)
        {
            if
                (
                (CANAdapterBaud == newBaud)
                && (_acceptanceMask == newAcceptanceMask)
                && (_acceptanceFilter == newAcceptanceFilter)
                && (CANAdapterRunning == true)
                )
            {
                return;
            }

            /* If the IXXAT dongle is already in operational mode, put back into pre-op mode
             * by resetting it. Then initialse the dongle with the baud, acceptance mask and
             * filter passed as parameters.  If all is OK, restart the dongle to put it back
             * into operational mode.
             */
            resetCAN(); //equiv stopCan
            if (CANAdapterHWIntialised == true)
            {
                if ((VCIFeedbackCode)initialiseBaudAndMask(newBaud, newAcceptanceMask, newAcceptanceFilter) == VCIFeedbackCode.VCI_OK)
                {
                    startCAN();
                }
            }
            /* Else the IXXAT hardware has not been opened and initialised yet so do this first.
             * If successfule, update the hardwareOpened flag accordingly then initialise the
             * baud rate, mask and filter as the passed parameters before restarting and puttin
             * back into operational mode.
             */
            else
            {
                this.openCANAdapterHW();
                if (this.CANAdapterHWIntialised == true)
                {
                    if ((VCIFeedbackCode)initialiseBaudAndMask(newBaud, newAcceptanceMask, newAcceptanceFilter) == VCIFeedbackCode.VCI_OK)
                    {
                        _acceptanceMask = newAcceptanceMask;
                        _acceptanceFilter = newAcceptanceFilter;

                        if ((VCIFeedbackCode)initialiseBuffers() == VCIFeedbackCode.VCI_OK)
                        {
                            this.startCAN();
                        }
                    }
                }
            }
        }



        //---------------------------------------------------------------------------
        //  Name			: closeCANAdapterHW()
        //  Description     : Closes the IXXAT dongle hardware when DW is closing.
        //					  Only a valid thing to do if the hardware was opened
        //					  as can run DW offline with no dongle at all.
        //  Parameters      : None
        //  Used Variables  : hardwareOpened - bool to indicate whether the hardware
        //									   has already been opened or not
        //					  VCI -	object to talk to the IXXAT dongle via the DLL
        //  Preconditions   : The IXXAT dongle hardware has been closed down.
        //  Post-conditions : None
        //  Return value    : None
        //---------------------------------------------------------------------------
        ///<summary>Closes the IXXAT adapter if it is running.</summary>
        public void closeCANAdapterHW()
        {
            if (CANAdapterHWIntialised == true)
            {
                // close board
                try
                {
                    VCIS_CancelBoard(
                        BoardHdl		// board handle
                        );
                }
                catch (Exception e)
                {
                    SystemInfo.errorSB.Append("\nFailed to close IXXAT adapter.");
                    SystemInfo.errorSB.Append(" error code");
                    SystemInfo.errorSB.Append(e.Message);
                }
                CANAdapterRunning = false;
                CANAdapterHWIntialised = false;
            }
        }
         
        #endregion

#region receive interrupt handler
		//-------------------------------------------------------------------------
		//  Name			: ReceiveQueuedata()
		//  Description     : This function is called when the IXXAT adapter receives
		//					  a valid CAN message which passes the acceptance mask and
		//					  filter settings.  The new messages are processed for
		//					  validity (valid reply to the last transmitted message?)
		//					  and data is stored off in a location for use by the DW
		//					  with the message type.
		//  Parameters      : que_hdl - queue handler
		//					  count - number of messages received in the IXXAT rx buffer
		//					  pObj - array of data holding the recevied message(s)
		//  Used Variables  : _rxValidData - flag set to indicate that any valid message
		//								has been received (used to find the baud & system)
		//					  _idList - list of COB-IDs received (used to find system)
		//					  _getIDList - flag set by DW to indicate trying to find
		//								the system attached to the CANbus
		//					  txMsg - last message transmitted on the CANbus
		//					  _rxResponseType - scs code indicating the type of response
		//								message that this message received is.  Aids
		//								processing in the rest of DW.
		//					  rxMsg - raw data of the receive message calling this interrupt.
		//  Preconditions   : The IXXAT adapter has been opened, configured and is running
		//					  and a valid message has been received that passes the acceptance
		//					  mask and filter settings.
		//  Post-conditions : The received message(s) have been analysed and appropriate flags
		//					  set so that the rest of DW can analyse the message 'offline' to
		//					  the interrupt.
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>CAN message receive interrupt handler, to process SDOs, PDOs, LSSs, EMCYs and heartbeats.</summary>
		/// <param name="que_hdl">queue handler</param>
		/// <param name="count">number of messages received in the IXXAT receive buffer</param>
		/// <param name="pObj">array of data holding the received message(s)</param>
		public void ReceiveQueuedata( ushort que_hdl, ushort count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] VCI_CAN_OBJ[] pObj)
		{
#if PORT_ACCESS	//timing test code
			this.setPin( 1 );
#endif

			_rxValidData = true;			// received some valid data of any description ie comms link OK

#region for each new received message in the buffer
			for ( int iCanNo = 0; iCanNo < count; iCanNo++ )
			{
#region get the data length of the current message
				int iLength = pObj[ iCanNo ].len_rtr & 0x0F;
	        
				if ( iLength > SCCorpStyle.CANMessageDataLengthMax ) 
				{
					iLength = SCCorpStyle.CANMessageDataLengthMax;
				}
                #endregion

#region check heartbeat message for controller state (put in FIFO for processing in timer thread)
				if ( ( pObj[ iCanNo ].id > 0x700 ) && ( pObj[ iCanNo ].id <= 0x7FF ) )
				{
					// if valid data then copy COBID and first data byte into next FIFO slot
					if ( iLength >= 1 )
					{
						int test = (int) (pObj[ iCanNo ].id - 0x700);
						if(nodeIDsFound.Contains(test) == true)  //judetemp we can only detect expected heartbeats - PDOs and SODs can have valie COBIDs in the heartbeat range
						{
							hbFIFO[ hbFIFOIn ].COBID = pObj[ iCanNo ].id;
							hbFIFO[ hbFIFOIn ].db1 = pObj[ iCanNo ].a_data[ 0 ];
							// increment fifo in with wrap around
							if ( ++hbFIFOIn >= sizeOfFIFOs )
							{
								hbFIFOIn = 0;
							}
						}

					}
					continue;
				}
                #endregion
#region check for EMCY messages for faults (put in FIFO for processing in timer thread)
				if ( ( pObj[ iCanNo ].id > SCCorpStyle.COBForEmergencyMinimum ) && ( pObj[ iCanNo ].id <= SCCorpStyle.COBForEmergencyMaximum ) )
				{
					// if valid data then copy 8 data bytes into next FIFO slot
					if ( iLength == 8 )
					{
						for ( byte i = 0; i < 8; i++ )
						{
							emcyFIFO[ emcyFIFOIn ][ i ] = pObj[ iCanNo ].a_data[ i ];
						}

						emcyFIFO[ emcyFIFOIn ][ 8 ] = (byte)pObj[ iCanNo ].id;

						// increment fifo in with wrap around
						if ( ++emcyFIFOIn >= sizeOfFIFOs )
						{
							emcyFIFOIn = 0;
						}
					}
					continue;
				}
                #endregion

#region diagnostics (which slow it down significantly)
#if CAN_TRAFFIC_DEBUG
				StringBuilder wrSB = new StringBuilder();
				wrSB.Append("Rx: ");
				wrSB.Append(pObj[ iCanNo ].id.ToString("X"));
				wrSB.Append(":");
				for(int temp = 0;temp<8;temp++)
				{
					string tempStr = pObj[ iCanNo ].a_data[temp].ToString("X");
					if(tempStr.Length<2)
					{
						tempStr = "0" + tempStr;
					}
					wrSB.Append(tempStr);
				}
				wrSB.Append(" Time: ");
				wrSB.Append(System.DateTime.Now.TimeOfDay.ToString());
				System.Console.Out.WriteLine(wrSB.ToString());
//				System.Diagnostics.Debug.WriteLine( writeString);
#endif
                #endregion

#region switch on expected CAN message type
				switch ( _messageType )
				{
#region PDO - calbrated graphing
					case CANMessageType.PDO:
					{
						foreach(COBObject COB in this.MonitoringCOBs)
						{
							if(COB.COBID == pObj[ iCanNo ].id)
							{
#region convert PDO CAN frame databytes from little endian bytes to a long value
								long PDOdata = 0;
								for( int i = 0; i < SCCorpStyle.CANMessageDataLengthMax; i++ )
								{
									PDOdata <<= 8;
									PDOdata += pObj[ iCanNo ].a_data[ 7 - i ];
								}
                                #endregion  convert PDO CAN frame databytes from little endian bytes to a long value

#region overwrite all those items that are contained in this recevied PDO
								foreach(ODItemAndNode itemAndNode in this.OdItemsBeingMonitored)
								{
									if ( itemAndNode.ODparam.monPDOInfo.COB == COB)  //this OD item is contained in this received PDO
									{//extract the corrent PDO mapping bits and convert to parameter (unscaled) vlaue

										long measuredVal = ( PDOdata & itemAndNode.ODparam.monPDOInfo.mask ) >> itemAndNode.ODparam.monPDOInfo.shift;
										
										if(itemAndNode.ODparam.bitSplit != null)
										{ //get the vlaue of just the bitsplit part
											long bsVal = (measuredVal & itemAndNode.ODparam.bitSplit.bitMask ) >> itemAndNode.ODparam.bitSplit.bitShift;
											itemAndNode.ODparam.measuredDataPoints.Add(new DataPoint(pObj[ iCanNo ].time_stamp, bsVal));//store history for plotting
											itemAndNode.ODparam.currentValue = bsVal;  //set the current value - this is going to make text monitoring much easier
										}
										else
										{
											itemAndNode.ODparam.measuredDataPoints.Add(new DataPoint(pObj[ iCanNo ].time_stamp, measuredVal)); //store history for plotting
											itemAndNode.ODparam.currentValue = measuredVal;//set the current value - this is going to make text monitoring much easier
										}
									}
								}
                                #endregion overwrite all those items that are contained in this recevied PDO
							}
							break; //found it so get out
						}
						break;
					}
                    #endregion PDO - calbrated graphing 

#region verify the SDO response
					case CANMessageType.SDO:
					{
#region if expected COBID for SDO reply to last message sent
						/* 
						 * SDO response with matching node ID and index & sub.
						 * COB ID response is 0x580 + NODE_ID and request is 0x600 + NODE_ID
						 * Hence the response is always 0x080 less than that transmitted.
						 */
						if 
							( 
							( ( pObj[ iCanNo ].id >  0x580 ) && ( pObj[ iCanNo ].id <= 0x5FF ) ) //default SDO cOB ID ( 0x580) plus node ID in range 1 to 127
							&& ( pObj[ iCanNo ].id == ( txMsg.id - SCCorpStyle.NodeCOBIDToNodeIDOffset ) ) //is response to SDO we sent ie came back form correct Node ID
							)
                        #endregion
						{
#region extract the response type from data byte 0
							byte scs = (byte)( pObj[ iCanNo ].a_data[ 0 ] & 0xe0 );
                            //DR38000239 JW _rxResponseType should not be set to default here - can be polled before switch is done
                            #endregion

#region check rx data by scs code
							switch ( scs )
							{
#region initiate upload response
								case 0x40:	
								{
									// check that the index and sub-index match the tx packet
									if
										(
										( txMsg.data[ 1 ] == pObj[ iCanNo ].a_data[ 1 ] )
										&& ( txMsg.data[ 2 ] == pObj[ iCanNo ].a_data[ 2 ] )
										&& ( txMsg.data[ 3 ] == pObj[ iCanNo ].a_data[ 3 ] )
										)
									{
										// Is it an initiate upload expedited response?
										if ( ( pObj[ iCanNo ].a_data[ 0 ] & 0x02 ) == 0x02 )
										{
											_rxResponseType = SDOMessageTypes.InitiateUploadResponseExpedited;
										}
											// Is it an initiate upload segmented response?
										else if ( ( pObj[ iCanNo ].a_data[ 0 ] & 0x01 ) == 0x01 )
										{
											_rxResponseType = SDOMessageTypes.InitiateUploadResponse;
										}
									}
									break;
								}
                                    #endregion

#region upload segment response
								case 0x00: 	
								{
									// check the toggle bit matches the last tx packet
									if ( ( pObj[ iCanNo ].a_data[ 0 ] & 0x10 ) == ( txMsg.data[ 0 ] & 0x10 ) )
									{
										_rxResponseType = SDOMessageTypes.UploadSegmentResponse;
									}
									break;
								}
                                    #endregion

#region initiate download response
								case 0x60:	
								{
									// check that the index and sub-index match the tx packet
									if
										(
										( txMsg.data[ 1 ] == pObj[ iCanNo ].a_data[ 1 ] )
										&& ( txMsg.data[ 2 ] == pObj[ iCanNo ].a_data[ 2 ] )
										&& ( txMsg.data[ 3 ] == pObj[ iCanNo ].a_data[ 3 ] )
										)
									{
										_rxResponseType = SDOMessageTypes.InitiateDownloadResponse;
									}
									break;
								}
                                    #endregion

#region download segment response
								case 0x20:	
								{
									// check the toggle bit matches the last tx packet
									if ( ( pObj[ iCanNo ].a_data[ 0 ] & 0x10 ) == ( txMsg.data[ 0 ] & 0x10 ) )
									{
										_rxResponseType = SDOMessageTypes.DownloadSegmentResponse;
									}
									break;
								}
                                    #endregion

#region download block response
								case 0xa0:
								{
									switch ( pObj[ iCanNo ].a_data[ 0 ] & 0x03 )
									{
										case 0x00:
										{
											// check that the index and sub-index match the tx packet
											if
												(
												( txMsg.data[ 1 ] == pObj[ iCanNo ].a_data[ 1 ] )
												&& ( txMsg.data[ 2 ] == pObj[ iCanNo ].a_data[ 2 ] )
												&& ( txMsg.data[ 3 ] == pObj[ iCanNo ].a_data[ 3 ] )
												)
											{
												_rxResponseType = SDOMessageTypes.InitiateBlockDownloadResponse;
											}
											break;
										}

										case 0x01:
										{
											_rxResponseType = SDOMessageTypes.EndDownloadBlockResponse;
											break;
										}

										case 0x02:
										{
											_rxResponseType = SDOMessageTypes.DownloadBlockSegmentResponse;
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
									_rxResponseType = SDOMessageTypes.AbortResponse;
									break;
								}
                                    #endregion
						
#region default (unknown response type)
								default: 
								{
									_rxResponseType = SDOMessageTypes.UnknownResponseType;
									break;
								}
                                    #endregion
							} // end of switch statement
                            #endregion

#region copy received data over
							for ( int b = 0; b < SCCorpStyle.CANMessageDataLengthMax; b++ )
							{
								_rxMsg.data[ b ] = pObj[ iCanNo ].a_data[ b ];
							}
                            #endregion

#region save id and data length into rxMsg
							_rxMsg.id = pObj[ iCanNo].id;
							_rxMsg.length = pObj[ iCanNo ].len_rtr;
                            #endregion

						} // end of if COB-ID matches expected response
						break;
					}
                        #endregion

#region SDO reply to find system ID list
                case CANMessageType.IDList:
                    {
						// if finding the system then save the ID (node no derivative) to idList
                        //Do NOT chekc the command specifier - bootloader does not contain some mandatory 
                        //object and will return an abort CS. Just checking th eindex and sub will be suffiecient
                        if
                            (
                            ((pObj[iCanNo].id > 0x580) && (pObj[iCanNo].id <= 0x5ff))   //An SDO response
                            && (txMsg.data[1] == pObj[iCanNo].a_data[1])                // }
                            && (txMsg.data[2] == pObj[iCanNo].a_data[2])                // } OD Index and Sub
                            && (txMsg.data[3] == pObj[iCanNo].a_data[3])                // }
                            )
                        {
                            int nodeID = (ushort)(pObj[iCanNo].id - 0x580); 
                            if (this.nodeIDsFound.Contains(nodeID) == true)
                            {
								//MAIN_WINDOW.duplicateNodeIDNo = (ushort)nodeID;
                                SystemInfo.errorSB.Append("\nDuplicate Node ID " + nodeID.ToString() + " found");
                            }
							this.nodeIDsFound.Add(nodeID);  //add it anyway - we have two physicla devices even if they share a nodeID
                        }
                        break;
                    }
                        #endregion

#region LSS response
					case CANMessageType.LSS:
					{
#region check expected COBID of an LSS reply
						if ( pObj[ iCanNo ].id == SCCorpStyle.LSSResponseCOBID )
						{
							_rxResponseType = SDOMessageTypes.LSSResponse;
						}
						else
						{
							_rxResponseType = SDOMessageTypes.InvalidResponse;
						}
                        #endregion

#region copy received data over
						for ( int b = 0; b < SCCorpStyle.CANMessageDataLengthMax; b++ )
						{
							_rxMsg.data[ b ] = pObj[ iCanNo ].a_data[ b ];
						}
                        #endregion
						break;
					}
                        #endregion

#region default - unknown message type is not analysed
					default:
					{
						break;
					}
                        #endregion
				}
                #endregion
			} // end of for each new message in buffer
            #endregion


#if PORT_ACCESS	//timing test code
			this.clearPin( 1 );
#endif

		}

        #endregion

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
		public VCIFeedbackCode transmit( uint id, byte length, byte [] data )
		{
			VCIFeedbackCode fbc = VCIFeedbackCode.VCI_OK;
			int retries = 0;

#region Keep copy of last message transmitted (use to check for correct response).
			txMsg.id	= id;
			txMsg.length = length;
			//judetemp - COnsole write is temp for debugging internal self char
//			System.Console.WriteLine();
//			System.Console.Out.Write("0x" + txMsg.id.ToString("X").PadLeft(4,'0') + "        0  - d-3256.3 0 Unknown { ");
			for ( int i = 0; i < SCCorpStyle.CANMessageDataLengthMax; i++ )
			{
				txMsg.data[ i ] = data[ i ];
//				System.Console.Out.Write("0x" + txMsg.data[i].ToString("X").PadLeft(2,'0') + " ");
			}
            #endregion
			// Reset response type variable back to no response, ready to record the next reply.
			_rxResponseType = SDOMessageTypes.NoResponse;

			try
			{
#if PORT_ACCESS	//timing test code
				this.setPin( 0 );
#endif
				do
				{
					// Transmit the txMsg onto the CAN via the IXXAT adapter.
					fbc = (VCIFeedbackCode)VCIS_TransmitObj( 
						BoardHdl,			// board handle
						TxQueHdl[0],		// queue handle
						id,					// identifier of send message
						length,				// no data bytes in send message
						data				// pointer to send data
						);

					if ( fbc != VCIFeedbackCode.VCI_OK )
					{
						retries++;
						Thread.Sleep( 1 );

						if ( retries > 200 )
						{
							break;
						}
					}
				}
				while ( fbc != VCIFeedbackCode.VCI_OK );
#if PORT_ACCESS	//timing test code
				this.clearPin( 0 );
#endif

#if CAN_TRAFFIC_DEBUG
#region diagnostics (takes too long to have in all the time)
				StringBuilder wrSB = new StringBuilder();
				wrSB.Append("Tx: ");
				wrSB.Append(id.ToString("X"));
				wrSB.Append(":");
				for(int temp = 0;temp<data.Length;temp++)
				{
					string tempStr = data[temp].ToString("X");
					if(tempStr.Length<2)
					{
						tempStr = "0" + tempStr;
					}
					wrSB.Append(tempStr);
				}
				wrSB.Append(" Time: ");
				wrSB.Append(System.DateTime.Now.TimeOfDay.ToString());
				System.Console.Out.WriteLine(wrSB.ToString());
				//System.Diagnostics.Debug.WriteLine( writeString);
                #endregion
#endif
			}
			catch ( Exception e )
			{
				SystemInfo.errorSB.Append("\nFailed to transmit message using IXXAT dongle");
				SystemInfo.errorSB.Append(" error code");
				SystemInfo.errorSB.Append(e.Message );
			}
		
			return ( fbc );
		}

        #endregion

#region exception handling

		//-------------------------------------------------------------------------
		//  Name			: ExceptionHandler()
		//  Description     : This function is called via a function pointer by the
		//					  IXXAT library if the IXXAT adapter generates an
		//					  exception.
		//  Parameters      : funcNum - function type name
		//					  errorCode - standard error code
		//					  extErr - extended error code
		//					  s - error string with function name and further error
		//						  specification
		//  Used Variables  : None
		//  Preconditions   : IXXAT adapter has generated an exception.
		//  Post-conditions : User has been warned of the exception generated.
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>Warns the user if there are any IXXAT adapter exceptions.</summary>
		/// <param name="funcNum">function type name</param>
		/// <param name="errorCode">standard error code</param>
		/// <param name="extErr">extended error code</param>
		/// <param name="s">error string with function name and further error specification</param>
		public void ExceptionHandler( int funcNum, int errorCode, ushort extErr, string s )
		{
			SystemInfo.errorSB.Append("\nIXXAT adapter exception generated.Function numer:");
			SystemInfo.errorSB.Append(funcNum.ToString());
			SystemInfo.errorSB.Append(", error code");
			SystemInfo.errorSB.Append( errorCode.ToString() );
			SystemInfo.errorSB.Append("extended Err code ");
			SystemInfo.errorSB.Append(extErr.ToString());
		}
        #endregion

#region not used
		//-------------------------------------------------------------------------
		//  Name			: XatCallback()
		//  Description     : Callback function.
		//  Parameters      : iIndex -
		//					  hwKey -
		//					  sName -
		//					  sValue -
		//					  sValueHex -
		//  Used Variables  : None
		//  Preconditions   : None
		//  Post-conditions : None
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>IXXAT adapter callback function.></summary>
		/// <param name="iIndex"></param>
		/// <param name="hwKey"></param>
		/// <param name="sName"></param>
		/// <param name="sValue"></param>
		/// <param name="sValueHex"></param>
		public void XatCallback ( uint iIndex, uint hwKey, string sName, string sValue, string sValueHex)
		{
			// Not used by DriveWizard at present.
		}

		//-------------------------------------------------------------------------
		//  Name			: MessageHandler()
		//  Description     : IXXAT message handler.
		//  Parameters      : s -
		//  Used Variables  : None
		//  Preconditions   : None
		//  Post-conditions : None
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>IXXAT adapter message handler.</summary>
		/// <param name="s">string to be displayed to the user</param>
		public void MessageHandler( string s )
		{
			// Not used by DriveWizard at present.
		}

        #endregion

#region thread timer to issue delegate events for heartbeats and emcys
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
		static void threadTimerExpired( Object state )
		{
#region heartbeat FIFO checks and calling of delegate
			if ( hbEventDelegate != null ) 
			{
				// process any new heartbeat messages in the FIFO
				while ( hbFIFOIn != hbFIFOOut )
				{
					int temp = hbFIFOOut;

					/* Increment FIFOOut before calling delegate 
					 * (as gets confused due to time taken in delegate function).
					 */
					if ( ++hbFIFOOut >= sizeOfFIFOs )
					{
						hbFIFOOut = 0;
					}

					// launch delegate to process the new message and notify user
					hbEventDelegate( (int)hbFIFO[ temp ].COBID, hbFIFO[ temp ].db1 );
				}
			}
            #endregion

#region emergency FIFO checks and calling of delegate
			if ( emcyEventDelegate != null )
			{
				// process any new emergency messages in the FIFO
				while ( emcyFIFOIn != emcyFIFOOut )
				{
					int temp = emcyFIFOOut;

					/* Increment FIFOOut before calling delegate 
					 * (as gets confused due to time taken in delegate function).
					 */
					if ( ++emcyFIFOOut >= sizeOfFIFOs )
					{
						emcyFIFOOut = 0;
					}
					// launch delegate to process the new message and notify user
					emcyEventDelegate( emcyFIFO[ temp ] );
				}
			}
            #endregion
		}
        #endregion

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
#endif //USING_VCI3
