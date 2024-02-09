/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.219$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/09/2008 23:14:54$
	$ModDate:23/09/2008 10:07:20$

ORIGINAL AUTHOR
    Alison King

DESCRIPTION
	Node Info class. An instance of this object is created
    for every device found on the connected system via the
    IXXAT USB-CAN adapter.  Also, an instance is created to
    hold the programming object dictionary (Sevcon device's
    have a separate OD when in boot mode) and an instance
    for holding a read in DCF file.  This separates out
    very clearly active data received from the controller
    which could be updated at any read or write request from
    the static data read from the DCF file.
    The NodeInfo objects have defined within them the object
    dictionary object (defining in the data object the OD),
    an EDS object for reading EDS files and a DCF object
    for reading or writing DCF files. It also contains
    this device's nodeID, manufacture, whether it is a master
    node, commsOK flag, access level awarded by this device,
    device version object (0x1018 details) and the EDS filename
    that was used to construct this node's object dictionary.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36695: nodeInfo.cs 

   Rev 1.219    23/09/2008 23:14:54  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.218.1.0    23/09/2008 23:10:12  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.218    09/07/2008 21:52:14  ak
 manufacturer & deviceType added to XML files to allow correct re-opening of
 monitor files; setupAndWriteMonitorPDOs() altered to allow monitor mappings
 to cross over different COBs and to call updatePDOMappings() to write
 calculated mon maps to the controller; readCommsProfileAreaOfOD() handling of
 bit split items; MonitoringPDOInfo added to XML files to allow correct saving
 & re-opening of calibrated monitoring data


   Rev 1.217    27/05/2008 14:55:22  jw
 DR38000247


   Rev 1.216    08/04/2008 21:04:46  ak
 configChksumSub added to commonly accessed subs (now refreshed each time
 device panel is shown).


   Rev 1.215    18/03/2008 23:47:08  ak
 Allow domainupload retries when timeout is standard or less.


   Rev 1.214    17/03/2008 13:13:16  jw
 Logs null obejct handling complete. Still needs progrees bar steps linking to
 odSub timeout values


   Rev 1.213    14/03/2008 10:56:50  jw
 Log handling revised, Read of Log releated CAN items now done from GUI ( to
 allow later DI /Ixxat unlinking) . Processing of recevied logs code and event
 lists moved to sysInfo - reduces memory usage ( one method per application
 rather than per node)  Some newer Sevocn devices do not hav ethese logs. Also
 event list methods now have product code input - to allow us to have seperate
 Event lists for eg displays etc. 


   Rev 1.212    13/03/2008 08:43:34  jw
 Redundant Reset activ efault methods removed ( controller doe snot allow
 external reset any more) .
 Reset log method  replaced by single ODWrote calls for simplicitiy andto
 allow progrees bar update to relate to odSub timeout for better feedback.
 Loglength checks removed. Have cuased rogue log length faults to be flagged
 in the past and not needed now we access logs via domains ( loglengt hisd
 domain length/log entry size) 


   Rev 1.211    12/03/2008 13:02:34  ak
 Now performs retries for a block download.


   Rev 1.210    06/03/2008 13:09:42  jw
 Double new lines in error strings changed to single new lines. Makes Error
 window more readable


   Rev 1.209    25/02/2008 16:19:46  jw
 Ensure that if not all Blck segment responses are received then feedback code
 is passed upwards but no user feedback is generated.


   Rev 1.208    15/02/2008 12:38:30  jw
 TxData and the Monitiring params were static in VCI3. By passing VCI as
 object into the received message event handler we can make then instance
 variables as per VCI2 - closer to back compatibility


   Rev 1.207    15/02/2008 11:43:06  jw
 Reduncadnt code line commented out to reduce compiler warnigns


   Rev 1.206    29/01/2008 21:33:38  ak
 SCCorpStyle.SDOMaxConsecutiveNoResponses constant used and
 readDeviceIdentity() updated for comms error handling.


   Rev 1.205    25/01/2008 10:46:40  jw
 VCI3 and Vista functionality files merge - more testing required


   Rev 1.204    18-01-2008 10:44:08  jw
 DR000235 Remove DW support for bitstrings. ConvertToFloat ( inc remove
 redundant input parameter)  and ConverToDouble modified


   Rev 1.203    15/01/2008 12:35:06  ak
 Domain uploads no longer perform retries (to prevent DW looking as if it has
 hung for lengthy timeouts).


   Rev 1.202    14/01/2008 21:06:12  ak
 Bug fix: objectNameString and sectionTypeString now populated and used for
 XML serialization (since integer equivalents reference into arrays that are
 built dynamically and subject to change).


   Rev 1.201    05/12/2007 21:13:28  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using System.Xml.Serialization;  
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using Ixxat;


namespace DriveWizard
{
	#region enumerated types

	#region nodeState
	public enum NodeState 
	{ 
		Bootup = 0x00,
		Stopped = 0x04,
		Operational = 0x05,
		PreOperational = 0x7f,
		Unknown = 0xffff
	};
	#endregion nodeState

	#endregion enumerated types

	#region structures
	#region data limits structure definition
	/// <summary>
	///DataLimits is a structure which will be populated with constant values to represent
	///the minimum and maximum values for this given data type and the length of the data
	///type given in bytes (required for transmitting and receiving from the controller).
	///The min and max is useful to display to the user for each object dictionary item
	///so that they can more easily select a valid value.  Sometimes the EDS file will
	///provide a more specific min and max value and the objectDictionary will use these
	///rather than the constants defined by the data type.
	/// </summary>
	internal struct DataLimits
	{
		/// <summary>minimum value valid for this data type instance</summary>
		internal long	lowLimit;
		
		/// <summary>maximum value valid for this data type instance</summary>
		internal long	highLimit;
		
		/// <summary>size in bytes of this data type instance</summary>
		internal uint	sizeOf;

		/// <summary>constructor to set the values when the object is initialised with data </summary>
		/// <param name="lowLimit">minimum value valid for this data type instance</param>
		/// <param name="highLimit">maximum value valid for this data type instance</param>
		/// <param name="sizeOf">size in bytes of this data type instance</param>
		internal DataLimits( long lowLimit, long highLimit, uint sizeOf )
		{
			this.lowLimit = lowLimit;
			this.highLimit = highLimit;
			this.sizeOf = sizeOf;
		}
	}

	#endregion

	#region Real32 and Real64 structure definitions
	/// <summary>High, low, default and current values for a real 32 data type</summary>
	internal class Real32Values
	{
		internal float	lowLimit;
		internal float	highLimit;
		internal float	currentValue;
		internal float	defaultValue;
	};

	/// <summary>High, low, default and current values for a real 64 data type</summary>
	internal class Real64Values
	{
		internal double	lowLimit;
		internal double	highLimit;
		internal double	currentValue;
		internal double	defaultValue;
	};
	#endregion

	#region BitSplit structure definition
	public class BitSplit
	{
		public BitSplit()
		{
		}
		private long _bitMask;
		[XmlElement("bitMask",typeof(long ))] 
		public long bitMask
		{
			get 
			{
				return ( _bitMask );
			}
			set
			{
				_bitMask = value;
			}
		}

		private int _bitShift;
		[XmlElement("bitShift",typeof(int ))] 
		public int bitShift
		{
			get 
			{
				return ( _bitShift );
			}
			set
			{
				_bitShift = value;
			}
		}

		private int _realSubNo;
		[XmlElement("realSubNo",typeof(int ))] 
		public int realSubNo
		{
			get 
			{
				return ( _realSubNo );
			}
			set
			{
				_realSubNo = value;
			}
		}

		private string _realSubParamName = "";
		[XmlElement("realSubName",typeof(string ))] 
		public string realSubParamName
		{
			get 
			{
				return ( _realSubParamName );
			}
			set
			{
				_realSubParamName = value;
			}
		}

		private long _highLimit;
		[XmlElement("highLimit",typeof(long ))] 
		public long highLimit
		{
			get 
			{
				return ( _highLimit );
			}
			set
			{
				_highLimit = value;
			}
		}

		private long _lowLimit;
		[XmlElement("lowLimit",typeof(long ))] 
		public long lowLimit
		{
			get 
			{
				return ( _lowLimit );
			}
			set
			{
				_lowLimit = value;
			}
		}
	}
	#endregion

	#endregion structures


	/// <summary>Enumerated type for the NMT state of a node, received in db0 
	/// of a heartbeat message</summary>
	public class AvailableNodesWithEDS
	{
		public AvailableNodesWithEDS()
		{}
		/// <summary>
		/// Used by MAIN_WINDOW.availableEDSInfo, for user update of DCF files and setting up vitual systems
		/// </summary>
		public string EDSFilePath = "";
		/// <summary>
		/// Used for virtaul nodes
		/// </summary>
		public bool isNMTMaster = false;

		/// <summary> string not used for EDS/controller match</summary>
		public string	vendorName = "";				 

		/// <summary> EDS equivalent of 0x1018 sub 1</summary>
		public uint		vendorNumber = 0;			
		
		/// <summary> EDS equivalent of 0x1018 sub 2</summary>
		public uint		productNumber = 0xFFFFFFFF;			
		
		/// <summary> EDS equivalent of 0x1018 sub 3</summary>
		public uint		revisionNumber = 0xFFFFFFFF;	
		
		/// <summary>string used to describe 3rd party devices (Sevcon has more specific info in productNumber) </summary>
		public string	productName = "";		
	}

	/// <summary>
	/// NodeInfo: one instance of this class per device found on the connected CAN system.
	/// It contains all the information about the status of the node (nodeID, status etc)
	/// and contains an objectDictionary instance to recreate the OD of the physical device.
	/// </summary>
	public class nodeInfo
	{
		[XmlIgnore]
		internal static int currentODItem = 0;  
		protected int numOfSubsBackup = 0;
		protected bool usingBackDoor = false;

		/// <summary>
		/// Forces recalculation of Product Range etc
		/// </summary>
		private bool identityUpdated = true; 
		#region public variable declarations

		#region node centric PDO parameters
		/// <summary>
		/// max number of PDOs this node can transmit - includes disabled and fixed
		/// </summary>
		[XmlIgnore]
		internal int maxTxPDOs = 0;
		/// <summary>
		/// max number of PDOs this node can receive
		/// </summary>
		[XmlIgnore]
		internal int maxRxPDOs = 0;
		[XmlIgnore]
		internal ArrayList disabledTxPDOIndexes;  //disabled but NOT fixed ie we can use these in new/modified PDOs
		[XmlIgnore]
		internal ArrayList disabledRxPDOIndexes;  //disabled but NOT fixed ie we can use these in new/modified PDOs
		[XmlIgnore]
		internal ArrayList fixedTxPDOIndexes;  //cannot be changed on node = denoted by num or maps sub index has access Type RO
		[XmlIgnore]
		internal ArrayList fixedRxPDOIndexes;  //cannot be changed on node = denoted by num or maps sub index has access Type RO
		[XmlIgnore]
		internal VPDOObject intPDOMaps;
		[XmlIgnore]
		internal ArrayList preFilledTxMotorSPDOMappings = new ArrayList(), preFilledRxMotorSPDOMappings = new ArrayList();
		[XmlIgnore]
		internal bool EVASRequired = false;
		#endregion node centric PDO parameters
		[XmlIgnore]
		internal static ArrayList nonAssigedEDSSections = new ArrayList();
		/// <summary>Dictionary object instance aggregated with this node instance.
		/// Contains the constructed object dictionary from the EDS after the system
		/// has been found successfully.</summary>

		/// <summary>EDS file reader instance object to parse the EDS file for
		/// pertinent information in order to construct the dictionary object.</summary>
		[XmlIgnore]
		internal	EDSorDCF				EDS_DCF;

		[XmlIgnore]
		internal bool hasLogs = false;
		#region commonly accessed odSubs - asaves searhcing dictionary each time
		[XmlIgnore]
		internal ODItemData emergencyodSub = null;
		[XmlIgnore]
		internal ODItemData timeStampodSub = null;
		[XmlIgnore]
		internal ODItemData syncTimeSub = null;
		[XmlIgnore]
		internal ODItemData syncCobIDSub = null;
		[XmlIgnore]
		internal ODItemData preOpodSub = null;
		[XmlIgnore]
		internal ODItemData nmtStateodSub  = null;
		internal ODItemData readAbortsub = null;

		//the folowing are the ODitems that are to be displayed on Vehicle status screen
		[XmlIgnore]
		internal ODItemData capvoltSub = null;
		[XmlIgnore]
		internal ODItemData temperatureSub = null;
		internal ODItemData displayDataLogDomainSub = null;
		internal ODItemData displayDatalogIntervalSub = null;
		internal ODItemData displayFaultlogDomainSub = null;
		[XmlIgnore]
		internal ODItemData battVoltSub = null;
        [XmlIgnore]
        internal ODItemData configChksumSub = null;
		#endregion commonly accessed odSubs - asaves searhcing dictionary each time

		#endregion

		[XmlArray( "ODitems" ), XmlArrayItem( "ODitem", typeof( ObjDictItem ) )]
		public  ArrayList objectDictionary = new ArrayList();
		#region private variable declarations

		[XmlIgnore]
		protected SystemInfo sysInfo;
		#endregion

		#region property declarations
		
		#region nodeID property (actual node number used to construct COB-ID to communicate with device

		private int		_nodeID;
		///<summary>node ID of the device this nodeInfo object instance represents on the system</summary>

		[XmlAttribute]
		public int		nodeID
		{
			get 
			{
				return ( _nodeID );
			}

			// check nodeID is in valid range (1..127) before setting.
			set
			{
				if ( ( value > 0 ) && ( value < SCCorpStyle.MaximumNumberOfNodes ) )
				{
					_nodeID = value;
				}
			}
		}
		#endregion

		#region manufacturer property (determines what functionality allowed by GUI)

        [XmlIgnore]
		protected Manufacturer _manufacturer = Manufacturer.UNKNOWN;
		///<summary>Manufacturer of the device on the system represented by this object.</summary>

        [XmlAttribute]
		public Manufacturer manufacturer
		{
			set
			{
				_manufacturer = value;
			}
			get
			{
				return ( _manufacturer );
			}
		}
		#endregion

		#region master status property
		/* Master status property (needed to force into pre-op state on 
		 * the master node to write any OD items value on any of the system nodes).
		 */

		[XmlIgnore]
		private bool	_masterStatus;
		///<summary>Indicates if this device on the system represented by this object is the system master.</summary>

		[XmlIgnore]
		public bool		masterStatus
		{
			get
			{
				return ( _masterStatus );
			}
			set
			{
				if(MAIN_WINDOW.isVirtualNodes == true)
				{
					_masterStatus = value;
				}
			}
		}
		#endregion

		#region item in Object Dictionary being read property
		/* Current item being read on this device's OD (not including no. of subs).
		 * Used to update the progress bar when reading/writing entire OD.
		 * i.e. progress bar shows itemBeingRead of noOfItemsInOD as a percentage.
		 */
		[XmlIgnore]
		protected int		_itemBeingRead;
		///<summary>Current item being read from this device's OD when readEntireOD is called.</summary>

		[XmlIgnore]
		public int		itemBeingRead
		{
			get
			{
				return ( _itemBeingRead );
			}
            set
            {
                _itemBeingRead = value;
            }
		}
		#endregion

		#region access level property - OK
		/* Access level property.  Non Sevcon nodes are set to 255 as unknown if need
		 * to log on so assume not and give max access level.  For Sevcon nodes, this
		 * is set to the actual access level granted when DW has logged on to this 
		 * device with the user's entered ID and password.
		 */
		protected byte	_accessLevel = 0;		// default level
		///<summary>Access level granted by this system device when DW logged in (Sevcon device's only).</summary>

		[XmlIgnore]
		public byte		accessLevel
		{
			get
			{
				if 
					(
					( manufacturer == Manufacturer.SEVCON )
					&& ( productRange != SCCorpStyle.ProductRange.PST.GetHashCode() )
					&& ( productVariant > SCCorpStyle.App_variant_lowlimit )
					&& ( productVariant < SCCorpStyle.App_variant_highlimit )
					)
				{
					return ( _accessLevel );
				}
				else
				{
					return ( 255 );
				}
			}

			set
			{
				_accessLevel = value;
			}
		}
		#endregion

		#region Sevcon controller type or 3rd party unknown
		[XmlIgnore]
		protected string	_deviceType = "";
		///<summary>Whether it is a Sevcon controller or a 3rd party unknown.</summary>

        [XmlAttribute]
		public string	deviceType
		{
			get
			{
				if ( _deviceType == "" )  //only calculate it once
				{
					switch ( _manufacturer )
					{
						case Manufacturer.THIRD_PARTY:
						{
							_deviceType = EDS_DCF.EDSdeviceInfo.productName;
							break;
						}

						case Manufacturer.UNKNOWN:
						{
							#region Unknown
							if(vendorID == SCCorpStyle.SevconID)
							{
								_deviceType = "Sevcon node: Invalid EDS";
							}
							else
							{
								_deviceType = "CANopen node: Missing EDS";
							}
							#endregion Unknown
							break;
						}

						case Manufacturer.SEVCON:
						{
							#region handle missing product code
							if(productCode == 0xFFFFFFFF)
							{
								return "Sevcon node: Missing product code";
							}
							#endregion handle missing product code
							#region product Range
                            //DR38000256 prod desc now imported from ProductIDs.XML file
                            if (productRange < sysInfo.sevconProductDescriptions.sevconProductRanges.Count)
                            {
                                _deviceType = ((SevconProductRange)sysInfo.sevconProductDescriptions.sevconProductRanges[productRange]).productRange;
							}
							#endregion product Range
							#region product variant
							if ( this.productVariant == SCCorpStyle.bootloader_variant )
							{
								_deviceType += " Bootloader";
							}
							else if ( (productVariant== SCCorpStyle.selfchar_variant_old) ||(productVariant == SCCorpStyle.selfchar_variant_new) )
							{
								if(_deviceType == SCCorpStyle.SevconProductRanges[0])  //unknown
								{
									_deviceType = "espAC";  //bakc compatibility with self char zero product code
								}
								_deviceType += " Self Char.";
							}
							else if ( ( productVariant > SCCorpStyle.App_variant_lowlimit ) && ( productVariant < SCCorpStyle.App_variant_highlimit ) )
							{
                                //DR38000256
                                SevconProductRange range = (SevconProductRange)sysInfo.sevconProductDescriptions.sevconProductRanges[this.productRange];
								if (productVariant < range.sevconProductVariants.Count)
								{
									_deviceType += " " + ((SevconProductVariant)(range.sevconProductVariants[productVariant])).productVariant;
								}
								else
								{
									_deviceType += " Application";
								}
							}
							else
							{
								_deviceType += "<Unknown>"; 
							}
							#endregion product variant
							#region product voltage
                            // DR38000256 calibrator handling added
                            if ((productRange != SCCorpStyle.ProductRange.SEVCONDISPLAY.GetHashCode())
                              &&(productRange != SCCorpStyle.ProductRange.CALIBRATOR.GetHashCode())
                              &&(productRange != SCCorpStyle.ProductRange.PST.GetHashCode())
                              && (productRange != SCCorpStyle.ProductRange.UNKNOWN.GetHashCode())
                                )
							{
								if ( productVoltage != 0 )
								{
									_deviceType	+= " " + productVoltage.ToString() + "V";
								}
								#endregion product voltage
								#region product current
								if ( productCurrent != 0 )
								{
									_deviceType	+= " " + productCurrent.ToString() + "0A";
								}
							}
							#endregion product current
							break;
						}
					}
				}
				
				return ( _deviceType );
			}
			set
			{
				_deviceType = value;
			}
		}
		#endregion

		#region EDS filename property
		/* Name of the EDS file which matches the retrieved device information (0x1018).
		 * This is set to "" if no match was found. If this is the case, DW cannot interact 
		 * with this node.
		 */
		[XmlIgnore]
		protected string	_EDSorDCFfilepath = ""; //protected so DCFNode can inherit it
		///<summary>The name of the EDS file, if found, which matches the physical device on the system represented by this object.</summary>

		[XmlIgnore]
		public string	EDSorDCFfilepath
		{
			get
			{
				return ( _EDSorDCFfilepath );
			}
			set
			{
				_EDSorDCFfilepath = value;
			}
		}
		#endregion EDS filename property

		#region XML filename property
		// Name of the XML file which matches the retrieved device information (0x1018).

		[XmlIgnore]
		protected string	_XMLfilepath;
		///<summary>The name of the XML file, if found, which matches the physical device on the system represented by this object.</summary>

		[XmlAttribute]
		public string	XMLfilepath
		{
			get
			{
				return ( _XMLfilepath );
			}
			set
			{
				_XMLfilepath = value;
			}
		}

        [XmlIgnore]
        //DR38000265 flag to use CANopen.XML when no match found
        public bool XMLFileMatchFound = false;
		#endregion XML filename propery

		#region node state (read from the last heartbeat message received)
		[XmlIgnore]
		private NodeState _nodeState = NodeState.Unknown;
		///<summary>Contains the last known NMT state, read from the heartbeat message.</summary>

		[XmlIgnore]
		public NodeState nodeState
		{
			get
			{
				return ( _nodeState );
			}

			set
			{
				_nodeState = value;
			}
		}
		#endregion

		#endregion
		
		#region monitoring related

		private string _MNvendorID = "0xFFFFFFFF";
		//strings due to these being in hex format for readability in the xml file
		///string value read in from monitoring XML file
		[XmlAttribute]
		public string MNvendorID
		{
			get
			{
				return _MNvendorID;
			}
			set
			{
				_MNvendorID = value;
				try
				{
					this._vendorID = System.Convert.ToUInt32(value, 16);
				}
				catch
				{
					SystemInfo.errorSB.Append("Invalid vendor ID in XML file");
					this._vendorID = 0xFFFFFFFF;
				}
			}
		}

		private string _MNproductCode = "0xFFFFFFFF";
		[XmlAttribute]
			///string value read in from monitoring XML file
		public string MNproductCode
		{
			get
			{
				return _MNproductCode;
			}
			set
			{
				_MNproductCode = value;
				try
				{
					this._productCode = System.Convert.ToUInt32(value, 16);
				}
				catch
				{
					SystemInfo.errorSB.Append("Invalid product code in XML file");
					this._productCode = 0xFFFFFFFF;
				}
			}
		}

		private string _MNrevisionNumber = "0xFFFFFFFF";
		[XmlAttribute]
			///string value read in from monitoring XML file
		public string MNrevisionNumber
		{
			get
			{
				return _MNrevisionNumber;
			}
			set
			{
				_MNrevisionNumber = value;
				try
				{
					this._revisionNumber = System.Convert.ToUInt32(value, 16);
				}
				catch
				{
					SystemInfo.errorSB.Append("Invalid revision number in XML file");
					this._revisionNumber = 0xFFFFFFFF;
				}
			}
		}
		[XmlIgnore]
		internal bool isInCurrentSystem = false; //assume false at first


		#endregion

		/// <summary>
		/// The table index in DWDataSet
		/// </summary>
		[XmlIgnore]
		public int nodeOrTableIndex = -1;
        internal ushort numConsecutiveNoResponse = 0;
        
		#region constructor and destructor
		//parameterless constructor for non-real can nodes eg DCF, monitoring
		public nodeInfo()
		{
//			this._deviceType = "";
//			this.EDS_DCF = new EDSorDCF(this, this.sysInfo);
		}
		//-------------------------------------------------------------------------
		//  Name			: constructor
		//  Description     : Performs any specific initialisation required when
		//					  an object instance is created.  None currently required.
		//  Parameters      : None
		//  Used Variables  : None
		//  Preconditions   : An NodeInfo object is being instantiated.
		//  Post-conditions : None
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Node information class object specific initialisation.</summary>
		public nodeInfo(SystemInfo passed_sysInfo, int nodesIndex, SortedList passed_eventIDList )
		{
			this.sysInfo = passed_sysInfo;
			nodeOrTableIndex = nodesIndex;
			this.EDS_DCF = new EDSorDCF(this, this.sysInfo);
		}
		public nodeInfo(SystemInfo passed_sysInfo, int tableIndex )  
		{
			this.sysInfo = passed_sysInfo;
			nodeOrTableIndex = tableIndex;
			this.EDS_DCF = new EDSorDCF(this, this.sysInfo);
		}
        // DR38000256 needs sysInfo passed, giving access to ProductIDs list to calculate deviceType for monitor store file
        public nodeInfo(int tableIndex, int passedNodeID, uint passedVendorID, uint passedProductCode, uint passedRevNo, SystemInfo passed_sysInfo)
		{
			nodeOrTableIndex =tableIndex;
			this._nodeID = passedNodeID;
			this._vendorID = passedVendorID;
			this.MNvendorID = "0x" + passedVendorID.ToString("X").PadLeft(8,'0');
			this._productCode= passedProductCode;
			this.MNproductCode = "0x" + passedProductCode.ToString("X").PadLeft(8,'0');
			this._revisionNumber = passedRevNo;
			this.MNrevisionNumber = "0x" + passedRevNo.ToString("X").PadLeft(8,'0');
            this.sysInfo = passed_sysInfo;
			this.findMatchingEDSFile();
			this.findMatchingXMLFile();
			this.EDS_DCF = new EDSorDCF(this, this.sysInfo);
		}
		#endregion

		#region device checks properties
		//-------------------------------------------------------------------------
		//  Name			: isSevconApplication()
		//  Description     : Used to determine if this is a genuine Sevcon node 
		//					  with application code which requires unique handling.
		//  Parameters      : None
		//  Preconditions   : deviceVersion and findSystem() must have already been found.
		//  Post-conditions : None
		//  Return value    : Returns if this is a genuine Sevcon node with application code.
		//--------------------------------------------------------------------------
		/// <summary>Used to determine if this is a genuine Sevcon node with application code 
		/// which requires unique handling.</summary>
		/// <returns>Returns if this is a genuine Sevcon node with application code.</returns>
		public bool isSevconApplication()
		{
			bool sevApp = false;
			if 
				(
				( manufacturer == Manufacturer.SEVCON )
				&& ( productRange != SCCorpStyle.ProductRange.PST.GetHashCode() )
				&& ( productVariant > SCCorpStyle.App_variant_lowlimit )
				&& ( productVariant < SCCorpStyle.App_variant_highlimit )
				)
			{
				sevApp = true;

			}

			return ( sevApp );
		}

		//-------------------------------------------------------------------------
		//  Name			: isSevconDevice()
		//  Description     : Used to determine if this is a genuine Sevcon node 
		//					  with any kind of code running on it.
		//  Parameters      : None
		//  Preconditions   : deviceVersion and findSystem() must have already been found.
		//  Post-conditions : None
		//  Return value    : Returns if this is a genuine Sevcon node with any code.
		//--------------------------------------------------------------------------
		/// <summary>Used to determine if this is a genuine Sevcon node with any code 
		/// running on it.</summary>
		/// <returns>Returns if this is a genuine Sevcon node with any code.</returns>
		public bool isSevconDevice()
		{
			bool sevDev = false;

			if 
				(
				( manufacturer == Manufacturer.SEVCON )
				&& ( productRange != SCCorpStyle.ProductRange.PST.GetHashCode() )
				)
			{
				sevDev = true;
			}

			return ( sevDev );
		}

		//-------------------------------------------------------------------------
		//  Name			: isSevconBootloader()
		//  Description     : Used to determine if this is a genuine Sevcon node 
		//					  runniong bootloader code
		//  Parameters      : None
		//  Preconditions   : deviceVersion and findSystem() must have already been found.
		//  Post-conditions : None
		//  Return value    : Returns if this is a genuine Sevcon node with Bootloader code.
		//--------------------------------------------------------------------------
		/// <summary>Used to determine if this is a genuine Sevcon node with any code 
		/// running on it.</summary>
		/// <returns>Returns if this is a genuine Sevcon node with any code.</returns>
		public bool isSevconBootloader()
		{
			bool sevBoot = false;

			if  (
				( manufacturer == Manufacturer.SEVCON )
				&& ( productRange != SCCorpStyle.ProductRange.PST.GetHashCode() )
				&& ( productVariant == SCCorpStyle.bootloader_variant )
				)
			{
				sevBoot = true;
			}

			return ( sevBoot );
		}

		//-------------------------------------------------------------------------
		//  Name			: isSevconSelfChar()
		//  Description     : Used to determine if this is a genuine Sevcon node 
		//					  running Self Characterisation code
		//  Parameters      : None
		//  Preconditions   : deviceVersion and findSystem() must have already been found.
		//  Post-conditions : None
		//  Return value    : Returns if this is a genuine Sevcon node with Self Char code.
		//--------------------------------------------------------------------------
		/// <summary>Used to determine if this is a genuine Sevcon node with any code 
		/// running on it.</summary>
		/// <returns>Returns if this is a genuine Sevcon node with any code.</returns>
		public bool isSevconSelfChar()
		{
			bool sevSelfChar = false;

			if  (
				( manufacturer == Manufacturer.SEVCON )
				&& ( productRange != SCCorpStyle.ProductRange.PST.GetHashCode() )
				&& (( productVariant == SCCorpStyle.selfchar_variant_old )
				|| ( productVariant == SCCorpStyle.selfchar_variant_new ))
				)
			{
				sevSelfChar = true;
			}

			return ( sevSelfChar );
		}
		#endregion

		#region reading of specifics ( master status, device version, logon, force in & out of pre-op)
		//-------------------------------------------------------------------------
		//  Name			: readMasterStatus()
		//  Description     : For this node's device, read the master/slave status object
		//					  and save to the _masterStatus property.
		//  Used Variables  : _nodeID - nodeID of controller to communicate with
		//					  dictionary - DW's constructed copy of the OD for this node
		//				      _masterStatus - nodeInfo property, true if this node is
		//							a CAN system master
		//  Preconditions   : The system must have successfully been found and the node
		//					  constructed from the EDS.  For Sevcon nodes, the controller
		//					  must have already been logged in to sufficient access level.
		//					  This operation is only valid for Sevcon nodes.
		//  Post-conditions : _masterStatus property set to true if a master, false if slave.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		///<summary>Reads whether this Sevcon node is a CAN system master from the physical device.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode readMasterStatus( )
		{
			#region local variable declaration and variable initialisation
			DIFeedbackCode	feedback = DIFeedbackCode.DIInvalidObjectType;
			#endregion
			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				feedback = DIFeedbackCode.DISuccess;
			}
			else
			{
				_masterStatus = false; // assume this is a slave until found otherwise
				// read the master/slave object from the controller
				ODItemData masterSlaveConfigSub = this.getODSubFromObjectType(SevconObjectType.MASTER_SLAVE_CONFIG, 0);
				if(masterSlaveConfigSub != null)  //exists in OD
				{
					feedback = this.readODValue(masterSlaveConfigSub);
					// if read OD item OK, retrieve from DW's copy of the controller's OD
					if ( feedback == DIFeedbackCode.DISuccess )
					{
						if ( masterSlaveConfigSub.currentValue > 0 )
						{
							_masterStatus = true;
						}
					}
				}
			}
			return ( feedback );
		}

		//-------------------------------------------------------------------------
		//  Name			: logonToDevice()
		//  Description     : Logs on to this device (of _nodeID) using the user ID
		//					  and password entered by the user.
		//  Parameters      : userID - user entered identifier
		//					  password - user entered password
		//  Used Variables  : accessLevel - access level granted by the Sevcon controller
		//									 for user's ID and password entered
		//					  dictionary - DW's constructed copy of the OD for this node
		//  Preconditions   : The system must have successfully been found (EDS read),
		//					  this node must be a Sevcon node & user must have entered
		//					  a user ID and password.
		//  Post-conditions : If feedback is DISuccess, successfully logged onto the
		//					  controller and the accessLevel has been awarded.
		//					  If feedback is failure code, failed to log on & accessLevel
		//					  not found.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		///<summary>Attempts to log onto the Sevcon node represented by this nodeInfo object.</summary>
		/// <param name="userID">user entered identifier</param>
		/// <param name="password">user entered password</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode logonToDevice( uint userID, uint password)
		{
			DIFeedbackCode	feedback = DIFeedbackCode.DIFailedToLogonToDevice;
			if(MAIN_WINDOW.isVirtualNodes == true)
			{
				password = Math.Min(password, 5);  //judetemp override new password algorthim for virtual system
				_accessLevel = (byte)Math.Min(5, password);
				return ( DIFeedbackCode.DISuccess );
			}
			else
			{
				if (( productVariant>0x00) &&( productVariant<0xF0))	
				{
					#region sevcon application - login required
					// temporarily set access level to 5 so that sufficient access to log in!
					_accessLevel = 255;
					/* For a proper controller, should write the userID to the PASSWORD sub object, then
					* write the password to the PASSWORD sub object. Finally, read the awarded
					* access level by reading back the sub object of PASSWORD.
					*/
					ODItemData odUserIDSub = this.getODSubFromObjectType(SevconObjectType.PASSWORD, 0x3);
					ODItemData odPasswordIDSub = this.getODSubFromObjectType(SevconObjectType.PASSWORD, 0x2);
					ODItemData odAccLevelSub = this.getODSubFromObjectType(SevconObjectType.PASSWORD, 0x1);
					if(( odUserIDSub != null) && (odPasswordIDSub != null) && (odAccLevelSub != null))
					{
						feedback = this.writeODValue(odUserIDSub, (long)userID );
						if ( feedback == DIFeedbackCode.DISuccess )
						{
							feedback = this.writeODValue(odPasswordIDSub, (long)password);
						}
						_accessLevel = 0;
						if ( feedback == DIFeedbackCode.DISuccess )
						{
							feedback = this.readODValue(odAccLevelSub);
							if ( feedback == DIFeedbackCode.DISuccess )
							{
								_accessLevel = (byte)odAccLevelSub.currentValue;
							}
						}
					}
					#endregion sevcon application - login required
				}
				else
				{
					#region bootloader or self char - set access level to 255 so can read/write OK
					_accessLevel = 255;
					feedback = DIFeedbackCode.DISuccess;
					#endregion
				}
				return ( feedback );
			}

		}

		//		public DIFeedbackCode logonToDevice( uint password )
		//		{
		//			password = Math.Min(password, 5);  //judetemp override new password algorthim for virtual system
		//			_accessLevel = (byte)password;
		//			return ( DIFeedbackCode.DISuccess );
		//		}

		//-------------------------------------------------------------------------
		//  Name			: forceIntoPreOpMode()
		//  Description     : Finds the force pre op object in the OD and sets the
		//					  value to a 1 to request all nodes enter the pre-op
		//					  state.  A reply is expected when the request was
		//					  successfully completed by the controller.
		//  Used Variables  : dictionary - DW's constructed copy of the OD for this node
		//  Preconditions   : The system must have successfully been found (EDS read),
		//					  this node must be a Sevcon node & user must have entered
		//					  >=1 new value for an object (verified by GUI) requiring
		//					  to be written to the controller.
		//					  This node must also be the master node on the system.
		//  Post-conditions : The system is in pre-op if feedback is DISuccess.  System
		//					  is left in the previous unknown state if feedback is failure.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		///<summary>Attempts to force the physical Sevcon device represented by this object into pre-operational mode.</summary>
		/// <param name="CANComms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode forceIntoPreOpMode()
		{
			DIFeedbackCode feedback = DIFeedbackCode.DIUnableToForceIntoPreOpMode;
			const int forcePreOp = 1;
			/* Write a 1 to the NMT_STATE object sub 0 to request the system enters pre-op.
			 * N.B. This is just a request and is not necessarily granted by the controller.
			 */
			if(this.preOpodSub != null)
			{
				feedback = this.writeODValue(preOpodSub, forcePreOp);
				if (( feedback == DIFeedbackCode.DISuccess )   && (this.nmtStateodSub != null))
				{
					#region Read back the NMT state to confirm if it did make it into the pre-op state
					feedback = this.readODValue(this.nmtStateodSub);
					//but don't flag any errors - it's OK for the node to take some time - our feedback is in the hearbeat messae
					#endregion
				}
			}
			else
			{
				feedback = DIFeedbackCode.DIInvalidIndexOrSub;
			}
			return ( feedback );
		}

		//-------------------------------------------------------------------------
		//  Name			: releaseFromPreOpMode()
		//  Description     : Finds the force pre op object in the OD and sets the
		//					  value to a 0 to request all nodes leave the pre-op
		//					  state (& go to operational).  A reply is expected when 
		//					  the request was successfully completed by the controller.
		//  Used Variables  : dictionary - DW's constructed copy of the OD for this node
		//  Preconditions   : The system must have successfully been found (EDS read),
		//					  this node must be a Sevcon node & no more objects in OD
		//					  need to be written to the controller.
		//					  This node must also be the master node on the system.
		//  Post-conditions : The system has left pre-op if feedback is DISuccess.  System
		//					  is left in the previous unknown state if feedback is failure.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		///<summary>Attempts to force the physical Sevcon device represented by this object into operational mode.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode releaseFromPreOpMode( )
		{
			DIFeedbackCode feedback = DIFeedbackCode.DIUnableToReleaseFromPreOpMode;
			const int releaseFromPreOp = 0;
			if(this.preOpodSub != null) 
			{
				/* Write a 0 to FORCE_TO_PREOP object sub 0 to request a release from the pre-op state.
				 * N.B. This is just a request and is not necessarily granted by the NMT master/individual nodes */
				feedback = this.writeODValue(this.preOpodSub, releaseFromPreOp );
				if (( feedback == DIFeedbackCode.DISuccess ) && (this.nmtStateodSub != null))
				{
					#region read back the NMT state to confirm made it into pre-op state
					feedback = this.readODValue(this.nmtStateodSub);
					//but don't flag any errors - our feedback comes from the heartbeat message
					#endregion
				}
			}
			return ( feedback );
		}

		//-------------------------------------------------------------------------
		//  Name			: readNodeState()
		//  Description     : If the current node state is unknown, then if it is possible
		//					  to surmise what state it is in via the product code this is
		//					  done so.  Alternatively, if it is possible to manually read
		//					  the NMT state from the device this is performed.
		//  Parameters      : 
		//  Used Variables  : _nodeState - contains the current known state of this device
		//					  _manufacturer - manufacturer of this device
		//					  deviceVersion.productCode - product code of this devvice
		//  Preconditions   : The node is known to exist on the system, the EDS has been read 
		//					  and communications is healthy.
		//  Post-conditions : nodeState is updated with the current state of the device,
		//					  if possible to do so.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		/// <summary>Checks the device product code and manually reads/updates the state if unknown 
		/// and it's possible to do so. </summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode readNodeState(  )
		{
			DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
			if ( _nodeState == NodeState.Unknown )
			{
				switch ( _manufacturer )
				{
						#region SEVCON device
					case Manufacturer.SEVCON:
					{
						if ( productRange != SCCorpStyle.ProductRange.PST.GetHashCode() )
						{
							switch ( productVariant )
							{
								case SCCorpStyle.bootloader_variant:
								{
									#region bootloader
									_nodeState = NodeState.Bootup;
									break;
									#endregion bootloader
								}
								case SCCorpStyle.selfchar_variant_old:
								case SCCorpStyle.selfchar_variant_new:
								{
									#region self Char
									_nodeState = NodeState.Bootup;
									break;
									#endregion self Char
								}
								default:
								{
									#region sevcon app
									// temp stop DI from thinking can't read NMT_STATE because not logged in
									_nodeState = NodeState.Unknown; 
									if(this.nmtStateodSub != null)
									{
										byte originalAccessLevel = accessLevel;
										byte originalLocalAccessLevel = _accessLevel;

										accessLevel = 1;
										_accessLevel = 1;
										feedback = readODValue( this.nmtStateodSub);
										if(feedback == DIFeedbackCode.DISuccess)
										{
											try
											{
												_nodeState = ( NodeState )this.nmtStateodSub.currentValue;
											}
											catch ( Exception )
											{
												_nodeState = NodeState.Unknown;
											}
										}
										// put access level back to what it really is
										accessLevel = originalAccessLevel;
										_accessLevel = originalLocalAccessLevel;
									}
									break;
									#endregion sevcon app
								}
							}
						}
						else // PST
						{
							// remains unknown as nothing we can do manually to find it out
							// without receiving a heartbeat message
						}
						break;
					}
						#endregion
						#region third party or unknown device
					case Manufacturer.THIRD_PARTY:
					case Manufacturer.UNKNOWN:
					default:
					{
						// remains unknown as nothing we can do manually to find it out
						// without receiving a heartbeat message
						break;
					}
						#endregion
				}
			}

			return ( feedback );
		}
		#endregion

		#region create pointers to commonly used odItems
		internal void identifyCommonlyAccessedOdSubs()
		{
			#region emergency
			MAIN_WINDOW.appendErrorInfo = false;
			emergencyodSub = getODSub(SCCorpStyle.EmcyCOBIDItem, 0);
			#endregion emergency
			#region sync
			syncTimeSub = this.getODSub(0x1006, 0);
			syncCobIDSub = this.getODSub(0x1005,0);
			#endregion sync
			#region Time Stamp
			timeStampodSub  = this.getODSub(0x1012,0);
			#endregion Time Stamp
			if(this.isSevconDevice() == true)
			{ //maybe application - but this gives us more strethc for pseudo sSevcon devices
				this.nmtStateodSub  = this.getODSubFromObjectType(SevconObjectType.NMT_STATE, 0);
				this.preOpodSub = this.getODSubFromObjectType(SevconObjectType.FORCE_TO_PREOP, 0);
				this.readAbortsub = this.getODSubFromObjectType(SevconObjectType.GENERAL_ABORT_CODE, 0);
			}
			
			if(this.isSevconApplication() == true)
			{
				//the follwoing are the items dsipalyed and updated on vehicle status screen
				this.battVoltSub = getODSubFromObjectType(SevconObjectType.DEVICE_MEASUREMENTS, 1);
				this.capvoltSub = getODSubFromObjectType(SevconObjectType.DEVICE_MEASUREMENTS, 3);
				this.temperatureSub = getODSubFromObjectType(SevconObjectType.DEVICE_MEASUREMENTS, 4);
                this.configChksumSub = getODSubFromObjectType(SevconObjectType.CONFIG_CHECKSUM,0);
			}
			if((SCCorpStyle.ProductRange)this.productRange == (SCCorpStyle.ProductRange.SEVCONDISPLAY))
			{
				this.displayDataLogDomainSub = this.getODSub(0x4000, 3);
				this.displayDatalogIntervalSub = this.getODSub(0x4000, 2);
				this.displayFaultlogDomainSub = this.getODSub(0x4001, 2);
			}

			MAIN_WINDOW.appendErrorInfo = true;
			

		}

		#endregion create pointers to commonly used odItems

		#region read values from OD
		//		//-------------------------------------------------------------------------
		//		//  Name			: readODValue()
		//		//  Description     : Reads the current value from the controller of _nodeID
		//		//					  and of object index and subIndex in the OD.  This then
		//		//					  updates the relevant currentValue in the DW's replica
		//		//					  copy of the device's OD (held in the dictionary object).
		//		//  Parameters      : index - OD index of the object to retrieve current value of
		//		//					  subIndex - OD sub of object to retrieve current value
		//		//  Used Variables  : dictionary - object containing a replica OD of the device's
		//		//								   constructed according to the EDS
		//		//  Preconditions   : System has been found & dictionary constructed from the EDS
		//		//					  and _nodeID is found and the user has selected the index & sub
		//		//					  required to be read from the controller.
		//		//  Post-conditions : The relevant objectDictionary[][] array element in the dictionary object
		//		//					  has had it's currentValue updated with the new value read 
		//		//					  from the controller.  Unreliable if feedback not DISuccess.
		//		//  Return value    : feedback - feedback code indicating success or reason of failure
		//		//--------------------------------------------------------------------------
		//		///<summary>Reads the current value of item of given index and subIndex from the physical device on the CANbus.</summary>
		//		/// <param name="index">OD index of the object to retrieve current value of</param>
		//		/// <param name="subIndex">OD sub of object to retrieve current value</param>
		//		/// <returns>feedback code indicating success or reason of failure</returns>
		//		internal DIFeedbackCode readODValue( ODItemData odSub)
		//		{
		//			/* Read the value from the controller using the _nodeID to construct the correct COB-ID,
		//			 * the accessLevel to check the user has correct access to retrieve this item,
		//			 * the index and sub to construct the upload request and the CANComms object to
		//			 * communicate with the device via the IXXAT.
		//			 */
		//
		//			// If CAN general error see if we can get more detail if it's a Sevcon node.
		//			return readObjectValue( odSub );
		//		}

		//-------------------------------------------------------------------------
		//  Name			: readODValue()
		//  Description     : Reads the current value from the controller of _nodeID
		//					  and of all objects in the Sevcon Section in the OD.  This then
		//					  updates the relevant currentValues in the DW's replica
		//					  copy of the device's OD (held in the dictionary object).
		//  Parameters      : section - SevconSection type (selecting all objects & their
		//								subs within) for which current values need read 
		//  Used Variables  : dictionary - object containing a replica OD of the device's
		//								   constructed according to the EDS
		//  Preconditions   : System has been found & dictionary constructed from the EDS
		//					  and _nodeID is found and the user has selected a Sevcon
		//					  section required to be read from the controller. Only valid
		//					  for a Sevcon controller.
		//  Post-conditions : The relevant objectDictionary[][] array elements in the dictionary object
		//					  has had it's currentValue updated with the new values read 
		//					  from the controller.  Unreliable if feedback not DISuccess.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		///<summary>Reads the current values of all items of given Sevcon section from the physical device on the CANbus.</summary>
		/// <param name="section">SevconSection type (selecting all objects and their
		/// subs within) for which current values need read</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode readODValue( int section, bool readDomains )
		{
			DIFeedbackCode firstfail = DIFeedbackCode.DISuccess;
			currentODItem = 0;
			// if dictionary and objectDictionary are valid and defined
			#region comments
			/* For each object within this device's objectDictionary dictionary, if the section is the
				 * same as the one requested by the user then read the current value for this
				 * object and all it's sub objects. Updated the DW's replica of the OD.
				 */
			#endregion comments
			foreach(ObjDictItem odItem in this.objectDictionary)
			{
				currentODItem++;
				// if thisItem is valid
				// if it is an array of something, check length as stated in EDS or resize if needed
				if ( MAIN_WINDOW.isVirtualNodes == false )
				{
					if ( ((ODItemData) odItem.odItemSubs[0]).displayType >= CANopenDataType.ARRAY )
					{
						dynamicallyResizeArrayType( odItem );
					}
				}
				foreach ( ODItemData odSub in odItem.odItemSubs )
				{
					#region for each sub object within this object
					// if matched section to that requested by the user, read the value from the device
					if (( odSub.sectionType == section ) 
						&& ((odSub.displayType != CANopenDataType.DOMAIN) || (readDomains == true))
                        && (this.numConsecutiveNoResponse < SCCorpStyle.SDOMaxConsecutiveNoResponses)) //this is needed to prevent re-entry before the numConsectVlaues is set to zero by the GUI
					{
						DIFeedbackCode feedback = readODValue( odSub);
						if(feedback != DIFeedbackCode.DISuccess) 
						{
							if(firstfail == DIFeedbackCode.DISuccess)
							{ //feedback can revert to success eg nothing to tranmsit
								firstfail = feedback;
							}
						}
					}
                    if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
					{
						return DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice;
					}
					#endregion for each sub object within this object
				} // end of for each sub object within this object
			}// end of for each object within the dictionary
			return firstfail;
		}

		//-------------------------------------------------------------------------
		//  Name			: readODValue()
		//  Description     : Reads the current value from the controller of _nodeID
		//					  and of all sub objects in the Sevcon Object in the OD. This
		//					  then updates the relevant currentValues in the DW's replica
		//					  copy of the device's OD (held in the dictionary object).
		//  Parameters      : sevconObject - SevconObject type (selecting object & their
		//								subs within) for which current values need read 
		//  Used Variables  : dictionary - object containing a replica OD of the device's
		//								   constructed according to the EDS
		//  Preconditions   : System has been found & dictionary constructed from the EDS
		//					  and _nodeID is found and the user has selected a Sevcon
		//					  section required to be read from the controller. Only valid
		//					  for a Sevcon controller.
		//  Post-conditions : The relevant objectDictionary[][] array elements in the dictionary object
		//					  has had it's currentValue updated with the new values read 
		//					  from the controller.  Unreliable if feedback not DISuccess.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		///<summary>Reads the current values of all item of given Sevcon object type from the physical device on the CANbus.</summary>
		/// <param name="sevconObject">SevconObject type (selecting object and their
		/// subs within) for which current values need read</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode readODValue( SevconObjectType sevconObject, bool readDomains )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode	firstFail = DIFeedbackCode.DISuccess;
			#endregion
			ArrayList odItemsOfObjectType =  this.getODItemAndSubsFromObjectType(sevconObject);
			foreach(ObjDictItem odItem in odItemsOfObjectType)
			{
				foreach ( ODItemData odSub in odItem.odItemSubs )
				{
					#region read each sub
					if( ((odSub.displayType != CANopenDataType.DOMAIN) || (readDomains == true))
                    && (this.numConsecutiveNoResponse < SCCorpStyle.SDOMaxConsecutiveNoResponses)) //prevwnt re-entry befor eGUI has ahd chance to reset numConsecutiveNoResponse
					{
						DIFeedbackCode feedback = readODValue( odSub );
						if(feedback != DIFeedbackCode.DISuccess)
						{
							if(firstFail == DIFeedbackCode.DISuccess)
							{
								firstFail = feedback;
							}
						}
					}
                    if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
					{
						return DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice;
					}
					#endregion read each sub
				}
			}
			if(odItemsOfObjectType.Count <= 0)
			{
				return DIFeedbackCode.DIInvalidIndexOrSub;
			}
			return ( firstFail );
		}

		/// <summary>Reads the current values of all items of given CANopen section from the physical device on the CANbus.</summary>
		/// <param name="CANSection">CANopen section whose objects and subs all need to be re-read from the device</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode readODValue( CANSectionType CANSection, bool readDomains )
		{
			DIFeedbackCode firstFail = DIFeedbackCode.DISuccess;
			currentODItem = 0;
			#region comments
			/* For each object within this device's objectDictionary dictionary, if the section is the
				 * same as the one requested by the user then read the current value for this
				 * object and all it's sub objects. Updated the DW's replica of the OD.
				 */
			#endregion comments
			foreach(ObjDictItem odItem in this.objectDictionary)
			{
				currentODItem++;  //for progress bar
				// if this object is defined and valid
				#region for each sub object within this object
				foreach ( ODItemData odSub in odItem.odItemSubs )
				{
					#region if matched section to that requested by the user, read the value from the device
					if (( CANSection.GetHashCode() < sysInfo.CANSectionODRange.Length ) //this CAN section is valid
						&& ( odSub.indexNumber >= sysInfo.CANSectionODRange[ CANSection.GetHashCode() ].minimum ) //within range
						&& ( odSub.indexNumber <= sysInfo.CANSectionODRange[ CANSection.GetHashCode() ].maximum ) //within range
						&& ((odSub.displayType != DriveWizard.CANopenDataType.DOMAIN) || (readDomains == true)) //are we reading domains?
                        && (this.numConsecutiveNoResponse < SCCorpStyle.SDOMaxConsecutiveNoResponses)) 
					{
						DIFeedbackCode feedback = this.readODValue(odSub);
						if(feedback != DIFeedbackCode.DISuccess)
						{
							if(firstFail == DIFeedbackCode.DISuccess) 
							{
								firstFail = feedback;
							}
						}
					}
                    if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
					{
						return DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice;
					}
					#endregion if matched section to that requested by the user, read the value from the device
				} // end of for each sub object within this object
				#endregion for each sub object within this object

			} // end of for each object within the dictionary
			return ( firstFail );
		}

		//-------------------------------------------------------------------------
		//  Name			: readObjectValue()
		//  Description     : This function checks that the node represented by this OD
		//					  contains an object of the given index and subIndex whose
		//					  current value has been requested.  It then checks the 
		//					  accessType, objFlags and accessType to check that this is
		//					  a valid data type to try to read from the controller.  Then
		//					  it checks the accessLevel of the user is sufficient to read
		//					  this item from the controller and that the data size means
		//					  there is data to be read from it.  Given this is all OK,
		//					  the CANcomms object is then called to build and transmit an
		//					  SDO read request to retrieve this information.  If a 
		//					  successful reply is received, the data[][] element is updated
		//					  the new currentValue (of the correct data type).
		//  Parameters      : nodeID - node ID of the controller, used to determine the 
		//							   COBID for the SDO read request
		//					  accessLevel - the access level (0-255) awarded to the user
		//									when they logged on to this controller
		//					  index - object index which the current value is to be read
		//					  subIndex - object sub index for which the value is to be read
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//  Used Variables  : data[][] - array containing all object & sub definitions and
		//								 their current values
		//  Preconditions   : The controller of the given nodeID has been found and has
		//					  a nodeInfo object defined with the data[][] populated with
		//					  the OD as defined in the found, matching EDS file. 
		//					  The user has selected an index/sub for which the current 
		//					  value is to be read from the the controller.
		//  Post-conditions : The relevant data[][] element's currentValue has been
		//					  updated with the new value as read in from the controller.
		//  Return value    : feedback - DISuccess if the object was read from the controller
		//							OK or a failure code indicating a reason for the failure.
		//----------------------------------------------------------------------------
		///<summary>Reads the item of given index and sub from device nodeID if a readable item, sufficient access level and obj flags OK.</summary>
		/// <param name="nodeID">node ID of the controller, used to determine the COBID for the SDO read request</param>
		/// <param name="accessLevel">the access level (0-255) awarded to the user when they logged on to this controller</param>
		/// <param name="index">object index which the current value is to be read</param>
		/// <param name="subIndex">object sub index for which the value is to be read</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback code indicates success or gives a failure reason</returns>
		internal DIFeedbackCode readODValue( ODItemData odSub  )
		{
			if(odSub == null)
			{
				return DIFeedbackCode.DIGeneralFailure;
			}
			int subIndex = odSub.subNumber;
			#region comments
			/* If a match for the index & sub was found in the data[][] array, check that
			 * the access type is not DisplayOnly.  This is the data type for header objects
			 * for record and array types and there is no real data associated with this to
			 * be read from the controller.  Return a success code as nothing to be done but
			 * it was successfully achieved.
			 */
			#endregion comments

			#region header row
			if ( odSub.subNumber == -1) 
			{
				return ( DIFeedbackCode.DISuccess );
			}
			#endregion header row

			#region if bit split item then get the real underlying sub 
			if ( odSub.bitSplit != null )
			{
				subIndex = odSub.bitSplit.realSubNo;
			}
			#endregion

			#region object ignore flags
			/* If the previous was OK and the ignore object flags property is false (ie don't ignore)
			 * then check this object within the data[][] array is not set to refuse to read on a
			 * scan.  If it is, return a success code as nothing is to be done but it has been
			 * done successfully.
			 */
			if ( this.EDS_DCF.ignoreObjFlags == false ) 
			{
				if ( ( odSub.objFlags & SCCorpStyle.RefuseReadOnScan )== SCCorpStyle.RefuseReadOnScan )
				{
					return ( DIFeedbackCode.DISuccess );
				}
			}
			#endregion object ignore flags
			
			#region write only objects		
			/* If all the previous was OK, check the access type of this data[][] element.  If it
			 * is a write only object then return a failure code to that effect and do not attempt
			 * to communicate with the controller to read the value as it is an invalid operation.
			 */											
			if 	(( odSub.accessType == ObjectAccessType.WriteOnly )
				|| ( odSub.accessType == ObjectAccessType.WriteOnlyInPreOp ))
			{
				return DIFeedbackCode.DISuccess;  //not really an error - this can happen when reading whole section - nothing to do with user actions
			}
			#endregion write only objects		

			#region access level
			/* If all the previous was OK, check that the user when they logged on has the
			 * correct level of access in order to read this data[][] element from the controller.
			 * If they don't, there is no point communcating with the controller to read the value
			 * as a CAN abort code will be received.  Return a failure code to this effect.
			 */
			if ((odSub.accessLevel > 0) &&( accessLevel < SCCorpStyle.ReadAccessLevelMin ))
			{
				return DIFeedbackCode.DIInsufficientAccessLevel;
			}
			#endregion access level

			#region get data size
			/* If all the previous was OK, get the data type of this data[]][] element
			 * and determine the data size of this type. If the data size is more than
			 * zero then there is some data to be retrieved from the controller so
			 * using the CANcomms object, transmit an SDO to read this object's current
			 * value.  If the SDO was sent and a valid packet was received (no abort codes
			 * etc.) then update this object dictionary with the new value.
			 */
			// get the data size for this object's display type.
			CANopenDataType datatype = (CANopenDataType) odSub.dataType;
			uint dataSize = (uint) datatype.GetHashCode();
			dataSize = sysInfo.dataLimits[ dataSize ].sizeOf;
			#endregion get data size

			// Only sent an SDO if the data size indicates there is data to read.
			if ( dataSize > 0 )
			{
				#region read value and, if successful update DI affected OD subs
				byte [] retArray = new byte [ dataSize ];
				if(MAIN_WINDOW.isVirtualNodes == false)
				{// Build and transmit an SDO to request the latest value of this object.
					DIFeedbackCode feedback = DIFeedbackCode.DIGeneralFailure;
					for(ushort i = 0;i<SCCorpStyle.SDONoResponseRetries;i++)
					{
						feedback = sysInfo.CANcomms.SDORead( nodeID, odSub.indexNumber, subIndex, dataSize, ref retArray, odSub.commsTimeout );
                        if
                        (
                            (feedback != DIFeedbackCode.DINoResponseFromController)
                         || (odSub.commsTimeout < SCCorpStyle.TimeoutDefault)
                         || ((datatype == CANopenDataType.DOMAIN) && (odSub.commsTimeout > SCCorpStyle.TimeoutDefault))
                        )
						{
							break;
						}
					}
				
					if ( feedback == DIFeedbackCode.DISuccess )
					{// If a successful reply was received, update the affected OD items
						this.updateItemValue( odSub, retArray );
						#region if a bit split object then update all the simulated subs with the new value
						if ( odSub.bitSplit != null )
						{
							ObjDictItem odItem = this.getODItemAndSubs(odSub.indexNumber);
							long backupVal = odSub.currentValue;  //we will overwirte the objects curren tval - so we need a backup
							for ( int i = 0; i <  odItem.odItemSubs.Count; i++ ) 
							{
								ODItemData otherODSub = (ODItemData) odItem.odItemSubs[ i ];
								if (( otherODSub.bitSplit != null )
									&& (odSub.bitSplit.realSubNo == otherODSub.bitSplit.realSubNo))
								{
									otherODSub.currentValue = ( backupVal & otherODSub.bitSplit.bitMask )>> otherODSub.bitSplit.bitShift;
								}
							}
						}
						#endregion
						this.numConsecutiveNoResponse = 0;
					}
					else if( (odSub.displayType ==CANopenDataType.DOMAIN) && (odSub.currentValue == SCCorpStyle.NothingToTransmit)) 
					{
						#region DOMAINS that report NothingToTransmit are not Real errors - report success
						feedback = DIFeedbackCode.DISuccess;
						#endregion DOMAINS that report nothing to transmit are not Real errors - report success
						this.numConsecutiveNoResponse = 0;
					}
					else if(feedback == DIFeedbackCode.DINoResponseFromController)
					{
						feedback = handleNoResponse(odSub);
					}
					if(feedback != DIFeedbackCode.DISuccess)
					{
						feedback = this.processfbc(odSub,feedback,false);
					}
					return feedback;
				}
				#endregion read value and, if successful update DI affected OD subs
			}
			return DIFeedbackCode.DISuccess;
		}

		/// <summary>
		/// Retrieve a whoel OD item comprising heade rand all subs
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>

		//-------------------------------------------------------------------------
		//  Name			: updateItemValue()
		//  Description     : Updates the current value of the element in data[][]
		//					  (item and subItem locates the correct element) with
		//					  the receive bytes formatted according to the data type.
		//  Parameters      : item - locate the relevant element in data[][] (1st dimension)
		//					  subItem - locate the relevant element in data[][] (2nd dimension)
		//					  rxData - array of raw data bytes received from controller
		//							   that needs to be formatted
		//  Used Variables  : data[][] - array containing all object & sub definitions and
		//								 their current values
		//  Preconditions   : The node's OD has already been constructed from an EDS or DCF
		//					  file and an object has been read from the controller and the
		//					  relevant element in data[][] is found by item & subItem.
		//  Post-conditions : The rxData has been formatted according to the display data
		//					  type and the relevant currentValue updated with it in the
		//					  data[][] array element of item & subItem indices.
		//  Return value    : feedback - indicates a success if the currentValue was formatted OK
		//							and assigned to the data[][] element or an invalid code 
		//							otherwise.
		//----------------------------------------------------------------------------
		///<summary>Updates the currentValue of the data[item][subItem] from the rxData array after converting it to the required data type.</summary>
		/// <param name="item">locate the relevant element in data[][] (1st dimension)</param>
		/// <param name="subItem">locate the relevant element in data[][] (2nd dimension</param>
		/// <param name="rxData">array of raw data bytes received from controller
		/// that needs to be formatted</param>
		/// <returns>feedback code indicates success or gives a failure reason</returns>
		/// 
		private DIFeedbackCode updateItemValue( ODItemData odSub, byte[] rxData )
		{
			DIFeedbackCode feedback = DIFeedbackCode.DIFailedToUpdateItemValue;

			/* Update the current value of the given element of the data[][] array.
				 * Must be converted according to the data type to ensure correct formatting.
				 * Check the item and subItem are in range.
				 */
			#region convert the raw rxData byte array into the correct data format
			CANopenDataType datatype = (CANopenDataType) odSub.dataType;
			switch(datatype)
			{
					#region boolean and unsigned integers
				case CANopenDataType.BOOLEAN:
				case CANopenDataType.UNSIGNED8:
				case CANopenDataType.UNSIGNED16:
				case CANopenDataType.UNSIGNED24:
				case CANopenDataType.UNSIGNED32:
				case CANopenDataType.UNSIGNED40:
				case CANopenDataType.UNSIGNED48:
				case CANopenDataType.UNSIGNED56:
				case CANopenDataType.UNSIGNED64:
				{
					long retValue = 0;
					if ( MAIN_WINDOW.isVirtualNodes == true )
					{
						retValue = odSub.defaultValue;
					}
					else
					{
						// Little endian format so can use the same conversion for all integers.
						for ( int i = rxData.Length - 1; i >= 0; i-- )
						{
							retValue <<= 8;
							retValue += rxData[ i ];
						}
					}
					odSub.currentValue = retValue;
					feedback = DIFeedbackCode.DISuccess;
					break;
				}
					#endregion

					#region signed integers
				case CANopenDataType.INTEGER8:
				case CANopenDataType.INTEGER16:
				case CANopenDataType.INTEGER24:
				case CANopenDataType.INTEGER32:
				case CANopenDataType.INTEGER40:
				case CANopenDataType.INTEGER48:
				case CANopenDataType.INTEGER56:
				case CANopenDataType.INTEGER64:
				{
					long retValue = 0;
					if ( MAIN_WINDOW.isVirtualNodes == true )
					{
						retValue = odSub.defaultValue;
					}
					else
					{
						// Little endian format so can use the same conversion for all integers.
						for ( int i = rxData.Length - 1; i >= 0; i-- )
						{
							retValue <<= 8;
							retValue += rxData[ i ];
						}
					}
					/* 
							 * Move the signed bit to the correct part when converting to store 
							 * in a long.
							 */
					retValue = sysInfo.convertToSignedLong( odSub.displayType, retValue );
					odSub.currentValue = retValue;
					feedback = DIFeedbackCode.DISuccess;
					break;
				}
					#endregion

					#region visible string
				case CANopenDataType.VISIBLE_STRING:
				{
					if ( MAIN_WINDOW.isVirtualNodes == true )
					{
						odSub.currentValueString = odSub.defaultValueString;
					}
					else
					{
						/*
								 * Strip of trailing nulls as Encoding.ASCII converts them into string as well.
								 */
						int i;
						for ( i = 0; i < rxData.Length; i++ )
						{
							if ( rxData[ i ] ==  0x00 )	// end of visible string
							{
								break;
							}
						}

						// Allocate a new byte array of the correct size and copy over the data.
						byte [] temp = new byte[ i ];
						for ( int j = 0; j < i; j++ )
						{
							temp[ j ] = rxData[ j ];
						}

						odSub.currentValueString = Encoding.ASCII.GetString( temp );
					}
					feedback = DIFeedbackCode.DISuccess;
					break;
				}
					#endregion

					#region domain data type
				case CANopenDataType.DOMAIN:
				{

					if ( MAIN_WINDOW.isVirtualNodes == true )
					{
						//do nothing - domains
					}
					else
					{
						// Make the current value point to the received byte array.
						odSub.currentValueDomain = rxData;
					}
					feedback = DIFeedbackCode.DISuccess;
					break;
				}
					#endregion

					#region real data types
				case CANopenDataType.REAL32:
				{
					if ( MAIN_WINDOW.isVirtualNodes == true )
					{
						odSub.real32.currentValue = odSub.real32.defaultValue;
						feedback = DIFeedbackCode.DISuccess;
					}
					else
					{
						if ( ( odSub.real32 != null ) && ( rxData.Length == 4 ) )
						{
							System.UInt32 retValue = 0;

							// Little endian format
							for ( int i = rxData.Length - 1; i >= 0; i-- )
							{
								retValue <<= 8;
								retValue += rxData[ i ];
							}
							//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
							//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
							float retValueAsFloat = sysInfo.convertToFloat( retValue.ToString() );
							odSub.real32.currentValue = retValueAsFloat;
							feedback = DIFeedbackCode.DISuccess;
						}
						else
						{
							feedback = DIFeedbackCode.DIUnexpectedDataLength;
						}
					}
					break;
				}
				case CANopenDataType.REAL64:
				{
					if ( MAIN_WINDOW.isVirtualNodes == true )
					{
						odSub.real64.currentValue = odSub.real32.defaultValue;
						feedback = DIFeedbackCode.DISuccess;
					}
					else
					{
						if ( ( odSub.real64 != null ) && ( rxData.Length == 8 ) )
						{
							System.UInt64 retValue = 0;

							// Little endian format
							for ( int i = rxData.Length - 1; i >= 0; i-- )
							{
								retValue <<= 8;
								retValue += rxData[ i ];
							}

							double retValueAsDouble = sysInfo.convertToDouble( retValue.ToString() );
							odSub.real64.currentValue = (double)retValueAsDouble;
							feedback = DIFeedbackCode.DISuccess;
						}
						else
						{
							feedback = DIFeedbackCode.DIUnexpectedDataLength;
						}
					}
					break;
				}
					#endregion

					#region CANopen data types currently not handled by DW
				case CANopenDataType.IDENTITY:
				case CANopenDataType.OCTET_STRING:
				case CANopenDataType.PDO_COMMUNICATION_PARAMETER:
				case CANopenDataType.PDO_MAPPING:
				case CANopenDataType.SDO_PARAMETER:
				case CANopenDataType.TIME_DIFFERENCE:
				case CANopenDataType.TIME_OF_DAY:
				case CANopenDataType.UNICODE_STRING:
				{
					// Not currently handled by V1 of DW.
					feedback = DIFeedbackCode.DIUnhandledDataType;
					break;
				}
					#endregion
			}
			#endregion
			return ( feedback );
		}

		#endregion
		
		#region write value to OD
		//-------------------------------------------------------------------------
		//  Name			: writeODValueViaBackDoor() 
		//  Description     : Writes a single new value to the object dictionary item of
		//					  specified index and sub on the device connected of _nodeID.
		//					  If successful, the DW's copy of the device's OD is updated.
		//					  Note: this function is overloaded for data types of integer,
		//					  string and byte array (domain).
		//  Parameters      : index - OD index of the object to write the new value to
		//					  subIndex - OD sub of object to write new value to
		//					  newValue - new value to be written to the device (long, str, byte[])
		//  Used Variables  : dictionary - contains a replica OD of the device's,
		//								   constructed according to the EDS
		//  Preconditions   : System has been found & dictionary constructed from the EDS
		//					  and _nodeID is found and the user has selected the index & sub
		//					  required to be written to on the device & entered a new value
		//					  which has been verified to be of right data type and in range.
		//  Post-conditions : The relevant data[][] array element in the dictionary object
		//					  has had it's currentValue updated with the new value when 
		//					  successfully written to the controller.  Remains as it was if
		//					  feedback is not a success code.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		///<summary>Writes one new integer value to the physical device's OD item of index and sub.</summary>
		/// <param name="index">OD index of the object to write the new value to</param>
		/// <param name="subIndex">OD sub of object to write new value to</param>
		/// <param name="newValue">new value to be written to the device as a long</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		internal DIFeedbackCode writeODValueViaBackDoor( ODItemData odSub, long newValue)
		{
			usingBackDoor = true;
			#region local variable declarations and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DIInvalidIndexOrSub;
			ushort seed = 0;
			ushort key = 0;
			#endregion
			ODItemData bdIndexSub = this.getODSubFromObjectType(SevconObjectType.INDIRECT_OBJECT_UPDATE, SCCorpStyle.BackDoorSubForIndex);
			ODItemData bdSubindexSub = this.getODSubFromObjectType(SevconObjectType.INDIRECT_OBJECT_UPDATE, SCCorpStyle.BackDoorSubForSubIndex);
			ODItemData bdNewValSub = this.getODSubFromObjectType(SevconObjectType.INDIRECT_OBJECT_UPDATE, SCCorpStyle.BackDoorSubForNewValue);
			ODItemData bdSeedSub  = this.getODSubFromObjectType(SevconObjectType.INDIRECT_OBJECT_UPDATE, SCCorpStyle.BackDoorSubForKey);
			ODItemData bdResultSub = this.getODSubFromObjectType(SevconObjectType.INDIRECT_OBJECT_UPDATE, SCCorpStyle.BackDoorSubForFeedbackCode);

			bool OKToContinue = false;
			feedback = checkNonWriteConditions(odSub,  out OKToContinue);
			if(OKToContinue == false)
			{
				usingBackDoor = false;
				return feedback;
			}
				if((bdIndexSub != null) && (bdSubindexSub != null) && (bdNewValSub != null) 
					&& (bdSeedSub != null)&& (bdResultSub != null))
				{
					this.checkEVAS(odSub);
					#region if insufficient access for this item then write using backdoor method
					#region  0/ ignore the obj flags for this backdoor DCF object
					EDS_DCF.ignoreObjFlags = true;
					#endregion  0/ ignore the obj flags for this backdoor DCF object

					#region 2/ write the index of the object to be written to the backdoor object
					feedback = writeODValue(bdIndexSub, (long)odSub.indexNumber);
					if(feedback != DIFeedbackCode.DISuccess)
					{
						usingBackDoor = false;
						return feedback;
					}
					#endregion 2/ write the index of the object to be written to the backdoor object

					#region 3/ write the sub index of the object to be written to the backdoor object
					feedback = this.writeODValue(bdSubindexSub,  (long)odSub.subNumber);
					if(feedback != DIFeedbackCode.DISuccess)
					{
						usingBackDoor = false;
						return feedback;
					}
					#endregion 3/ write the sub index of the object to be written to the backdoor object

					#region 4/ write the new value to be written to the backdoor object
					feedback = writeODValue(bdNewValSub, newValue);
					if(feedback != DIFeedbackCode.DISuccess)
					{
						usingBackDoor = false;
						return feedback;
					}
					#endregion 4/ write the new value to be written to the backdoor object

					#region 5/ read the seed value from the device's backdoor object
					feedback = this.readODValue(bdSeedSub);
					if(feedback != DIFeedbackCode.DISuccess)
					{
						usingBackDoor = false;
						return feedback;
					}
					#endregion 5/ read the seed value from the device's backdoor object

					#region 6/ read the seed value back from the device's backdoor
					seed = (ushort)bdSeedSub.currentValue;
					#endregion 6/ read the seed value back from the device's backdoor

					#region 7/ calculate the encrypted key value based on the seed received from the device
					key = scramble( seed, (ushort)0xffff );
					#endregion 7/ calculate the encrypted key value based on the seed received from the device
			
					#region 8/ write the key back to the device seed/key sub of backdoor object
					feedback = writeODValue(bdSeedSub, (long)key );
					if(feedback != DIFeedbackCode.DISuccess)
					{
						usingBackDoor = false;
						return feedback;
					}
					#endregion 8/ write the key back to the device seed/key sub of backdoor object

					#region  9/ wait for the controller to complete the write to EEPROM
					bool finished = false;
					do
					{
						#region wait for controller to write to EEPROM
						feedback = this.readODValue(bdResultSub);
						if ( feedback == DIFeedbackCode.DISuccess )
						{
							long fbcFromController = bdResultSub.currentValue;
							if ( (DIFeedbackCode)fbcFromController == DIFeedbackCode.CANUnknownAbortCode )
							{//success
								finished = true;
								feedback = DIFeedbackCode.DISuccess;
							}
							else if ( fbcFromController == (long)0xffff ) 
							{
								finished = false;
							}
							else
							{// failed with failure code
								finished = true;
								feedback = (DIFeedbackCode)fbcFromController;
							}
						}
						else
						{
							usingBackDoor = false;
							return feedback;
						}
						#endregion wait for controller to write to EEPROM
					}
					while ( finished == false );
					#endregion  9/ wait for the controller to complete the write to EEPROM

					#region 10/ set back to adhering to objFlags for any further DCF object download
					EDS_DCF.ignoreObjFlags = false;
					#endregion 10/ set back to adhering to objFlags for any further DCF object download
					#endregion
				}
				else
				{
					#region one of the Backdoor OD items we need does not exist
					feedback = processfbc(odSub,feedback,  true);
					usingBackDoor = false;
					return feedback;
					#endregion one of the Backdoor OD items we need does not exist
				}
			usingBackDoor = false; //B&B
			return feedback;
		}


		internal DIFeedbackCode checkNonWriteConditions(ODItemData odSub, out bool OKToContinue)
		{
			OKToContinue = false;

			if(odSub == null)
			{
				return (this.processfbc(odSub,DIFeedbackCode.DIInvalidItemOrSub,true));
			}
			#region header row
			/* If a match for the index & sub was found in the data[][] array, check that
			 * the access type is not DisplayOnly.  This is the data type for header objects
			 * for record and array types and there is no real data associated with this to
			 * be read from the controller.  Return a success code as nothing to be done but
			 * it was successfully achieved.
			 */
			if (odSub.subNumber == -1)
			{
				return DIFeedbackCode.DISuccess;
			}
			#endregion header row

			#region downlaoding DCF - ignore NMT state error
			/* Don't try to write to an item if it requires the device to be in pre-operational
			 * and it isn't AND we're downloading a DCF (ie ignoreObjFlags is false).
			 * Just return a feedback code indicating success as user has already been warned
			 * by the GUI.
			 */
			if ( ( nodeState == NodeState.Operational ) && ( this.EDS_DCF.ignoreObjFlags == false ) )
			{
				if (( odSub.accessType == ObjectAccessType.WriteOnlyInPreOp )
					|| ( odSub.accessType == ObjectAccessType.ReadReadWriteInPreOp )
					|| ( odSub.accessType == ObjectAccessType.ReadWriteInPreOp )
					|| ( odSub.accessType == ObjectAccessType.ReadWriteWriteInPreOp ))
				{
					return DIFeedbackCode.DISuccess;
				}
			}
			#endregion downlaoding DCF - ignore NMT state error

			#region ignore object flags
			/* If the previous was OK and the ignore object flags property is false (ie don't ignore)
			 * then check this object within the data[][] array is not set to refuse to write on a
			 * download.  If it is, return a success code as nothing is to be done but it has been
			 * done successfully.
			 */
			if ( this.EDS_DCF.ignoreObjFlags == false )
			{
				if ( ( odSub.objFlags & SCCorpStyle.RefuseWriteOnDownload )== SCCorpStyle.RefuseWriteOnDownload )
				{
					return DIFeedbackCode.DISuccess ;
				}
			}
			#endregion ignore object flags

			#region read only object
			/* If all the previous was OK, check the access type of this data[][] element.  If it
			 * is a read only (or a constant) object then return a failure code to that effect 
			 * and do not attempt to communicate with the controller to write the new value 
			 * as it is an invalid operation.
			 */							
			if 
				(( odSub.accessType == ObjectAccessType.ReadOnly )
				|| ( odSub.accessType == ObjectAccessType.Constant ))
			{
				//return DIFeedbackCode.DIUnableToWriteToReadOnlyObject;
				return DIFeedbackCode.DISuccess;
			}
			#endregion read only object

			#region access level => non back door only
			if(this.usingBackDoor == false)
			{
				/* If all the previous was OK, check that the user when they logged on has the
				 * correct level of access in order to write to this data[][] element from the controller.
				 * If they don't, there is no point communcating with the controller to write the value
				 * as a CAN abort code will be received.  Return a failure code to this effect.
				 */
				if ( odSub.accessLevel > accessLevel )
				{
					return (this.processfbc(odSub,DIFeedbackCode.DIInsufficientAccessLevel,true));
				}
			}
			#endregion access level => non back door only

			OKToContinue = true; //if we ge tthis far then we should try an dwrite value
			return DIFeedbackCode.DISuccess;
		}
		//-------------------------------------------------------------------------
		//  Name			: writeObjectValue()
		//  Description     : Writes a new value to the object of index and subIndex
		//					  in the object dictionary on the controller attached which
		//					  has a node ID of nodeID.  The index & sub are converted
		//					  to find the array element within data[][] which represents
		//					  this object so that the data type, display type, access level 
		//					  etc. are checked before the new value is written in a CAN
		//					  SDO packet which is transmitted to the controller.  If 
		//					  successful, the relevant object in the DW's copy of the
		//					  controller's OD is updated with the new value.
		//					  This function is overloaded for newValues of data type
		//					  long, string and byte arrays.
		//  Parameters      : nodeID - node ID of the controller, used to determine the 
		//							   COBID for the SDO download request
		//					  accessLevel - the access level (0-255) awarded to the user
		//									when they logged on to this controller
		//					  nodeState - current NMT state for this node
		//					  index - object index which the current value is to be written
		//					  subIndex - object sub index for which the value is to be written
		//					  newValue - long, string or byte array containing the new value
		//								 which is to be written to the controller (already
		//							     been error checked by the GUI)
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//  Used Variables  : data[][] - array containing all object & sub definitions and
		//								 their current values
		//  Preconditions   : The controller of the given nodeID has been found and has
		//					  a nodeInfo object defined with the data[][] populated with
		//					  the OD as defined in the found, matching EDS file. 
		//					  The user has selected an index/sub and set a new value which 
		//					  has been error checked by the GUI to be written to the controller.
		//  Post-conditions : The relevant data[][] element's currentValue has been
		//					  updated with the new value after it has successfully been
		//					  written to the relevant object in the controller. Otherwise,
		//					  it remains at the last value read/written from the controller.
		//  Return value    : feedback - DISuccess if the object was written to the controller
		//							OK or a failure code indicating a reason for the failure.
		//----------------------------------------------------------------------------
		///<summary>Writes the item of given index and sub from device nodeID with newValue (long) if a writable item, sufficient access level and obj flags OK.</summary>
		/// <param name="nodeID">node ID of the controller, used to determine the COBID for the SDO write request</param>
		/// <param name="accessLevel">the access level (0-255) awarded to the user when they logged on to this controller</param>
		/// <param name="nodeState">current NMT state of this node</param>
		/// <param name="index">object index which the new value is to be written to</param>
		/// <param name="subIndex">object sub index for which the new value is to be written to</param>
		/// <param name="newValue">new value which is to be written to the controller as a long value</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback code indicates success or gives a failure reason</returns>
		internal DIFeedbackCode writeODValue( ODItemData odSub, long newValue )
		{
			#region local variable declaration and variable initialisation
			long newValueToTx = 0;
			int subIndex =  odSub.subNumber;
			DIFeedbackCode feedback;
			#endregion

			#region don't write to devic e- if we know that the write will be rejected
			bool OKToContinue = false;
			feedback = checkNonWriteConditions(odSub, out OKToContinue);
			if(OKToContinue == false)
			{
				return feedback;
			}
			#endregion don't write to devic e- if we know that the write will be rejected

			#region if a bit split item then need to reconstruct the total value from all the simulated subs
			newValueToTx = newValue;
			if ( odSub.bitSplit != null )
			{
				subIndex = odSub.bitSplit.realSubNo; //wrtie to real sub - not psedo one
				newValueToTx = 0;
				ObjDictItem odItem = this.getODItemAndSubs(odSub.indexNumber);
				for ( int i = 0; i < odItem.odItemSubs.Count; i++ )
				{
					ODItemData otherODSub = (ODItemData) odItem.odItemSubs[i];
					if ((otherODSub.bitSplit != null ) && (otherODSub.bitSplit.realSubNo == odSub.bitSplit.realSubNo))
					{
						if (otherODSub.subNumber == odSub.subNumber )
						{
							newValueToTx += ((newValue << ((ODItemData)odItem.odItemSubs[ i ]).bitSplit.bitShift ) & otherODSub.bitSplit.bitMask );
						}
						else
						{
							newValueToTx += (( otherODSub.currentValue << otherODSub.bitSplit.bitShift )& otherODSub.bitSplit.bitMask );									
						}
					}
				}
			}
			#endregion

			#region send to device
			/* If all the previous was OK, get the data type of this data[]][] element
			 * and determine the data size of this type. If the data size is more than
			 * zero then there is some data to be written to the controller using the 
			 * CANcomms object. This is also used to format the new value into the correct
			 * data length for the controller to accept. i.e. DW uses a long to repreent
			 * every single integer type (8,16,24,32,40,48,56 or 64) but this needs to
			 * be converted back into the correct size for the controller to recognise.
			 * Build and transmit an SDO to write the new value to the object.  
			 */

			// AJK, 12/05/05 conversion not needed as performed naturally by truncation of leading bytes
			// eg -2 as a byte is 0xfe and as a long is 0xfffffffffffffffe so when truncated to two
			// lower nibbles becomes 0xfe also.
			CANopenDataType datatype = (CANopenDataType) odSub.dataType;
			uint dataSize = (uint) datatype.GetHashCode();
			dataSize = sysInfo.dataLimits[ dataSize ].sizeOf;
			if ( dataSize > 0 )
			{
				for(ushort i = 0;i<SCCorpStyle.SDONoResponseRetries;i++)
				{
					feedback = sysInfo.CANcomms.SDOWrite( nodeID, odSub.indexNumber, subIndex, dataSize, newValueToTx, odSub.commsTimeout );
					if((feedback != DIFeedbackCode.DINoResponseFromController) || (odSub.commsTimeout< SCCorpStyle.TimeoutDefault))
					{
						break;
					}
				}
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					odSub.currentValue = newValue;
				}	
				else
				{
					if( feedback == DIFeedbackCode.DINoResponseFromController)
					{
						feedback = this.handleNoResponse(odSub);
					}
					if(feedback != DIFeedbackCode.DISuccess)
					{
						feedback = processfbc(odSub,feedback, true);
					}
				}
				this.checkEVAS(odSub);
				return feedback;
			}
			else
			{
				return DIFeedbackCode.DISuccess;
			}
			#endregion send to device
		}

		//-------------------------------------------------------------------------
		///<summary>Writes the item of given index and sub from device nodeID with newValue (string) if a writable item, sufficient access level and obj flags OK.</summary>
		/// <param name="nodeID">node ID of the controller, used to determine the COBID for the SDO write request</param>
		/// <param name="accessLevel">the access level (0-255) awarded to the user when they logged on to this controller</param>
		/// <param name="nodeState">current NMT state of this node</param>
		/// <param name="index">object index which the new value is to be written to</param>
		/// <param name="subIndex">object sub index for which the new value is to be written to</param>
		/// <param name="newValue">new value which is to be written to the controller as a string value</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback code indicates success or gives a failure reason</returns>
		//-------------------------------------------------------------------------
		internal DIFeedbackCode writeODValue( ODItemData odSub, string newValue)
		{
			DIFeedbackCode feedback;
			#region don't write to devic e- if we know that the write will be rejected
			bool OKToContinue = false;
			feedback = checkNonWriteConditions(odSub, out OKToContinue);
			if(OKToContinue == false)
			{
				return feedback;
			}
			#endregion don't write to devic e- if we know that the write will be rejected

			#region write to device
			/* If all the previous was OK, get the data type of this data[]][] element
			 * and determine the data size of this type. If the data size is more than
			 * zero then there is some data to be written to the controller using the 
			 * CANcomms object. This is also used to format the new value into the correct
			 * data length for the controller to accept. i.e. DW uses a long to repreent
			 * every single integer type (8,16,24,32,40,48,56 or 64) but this needs to
			 * be converted back into the correct size for the controller to recognise.
			 * Build and transmit an SDO to write the new value to the object.  
			 * This may be a segmented SDO transfer dependent on the data length.
			 */
			CANopenDataType datatype = (CANopenDataType) odSub.dataType;
			uint dataSize = (uint) (datatype.GetHashCode());
			dataSize = sysInfo.dataLimits[ dataSize ].sizeOf;
			if ( dataSize > 0 )
			{
				dataSize = (uint) newValue.Length;

				for(ushort i = 0;i<SCCorpStyle.SDONoResponseRetries;i++)
				{
					feedback = sysInfo.CANcomms.SDOWrite( nodeID, odSub.indexNumber, odSub.subNumber, dataSize, newValue, odSub.commsTimeout );
					if((feedback != DIFeedbackCode.DINoResponseFromController) || (odSub.commsTimeout< SCCorpStyle.TimeoutDefault))
					{
						break;
					}
				}
				
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					odSub.currentValueString = newValue;//update this object dictionary with the new value
				}
				else
				{
					if( feedback == DIFeedbackCode.DINoResponseFromController)
					{
						feedback = this.handleNoResponse(odSub);
					}
					if(feedback != DIFeedbackCode.DISuccess)
					{
						feedback = processfbc(odSub,feedback, true);
					}
				}
				this.checkEVAS(odSub);
				return feedback;
			}
			else
			{
				return DIFeedbackCode.DISuccess;
			}
			#endregion write to device
		}
		//-------------------------------------------------------------------------
		///<summary>Writes the item of given index and sub from device nodeID with newValue (float) if a writable item, sufficient access level and obj flags OK.</summary>
		/// <param name="nodeID">node ID of the controller, used to determine the COBID for the SDO write request</param>
		/// <param name="accessLevel">the access level (0-255) awarded to the user when they logged on to this controller</param>
		/// <param name="nodeState">current NMT state of this node</param>
		/// <param name="index">object index which the new value is to be written to</param>
		/// <param name="subIndex">object sub index for which the new value is to be written to</param>
		/// <param name="newValue">new value which is to be written to the controller as a float value</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback code indicates success or gives a failure reason</returns>
		//-------------------------------------------------------------------------
		internal DIFeedbackCode writeODValue( ODItemData odSub,float newValue)
		{
			#region don't write to devic e- if we know that the write will be rejected
			bool OKToContinue = false;
			DIFeedbackCode feedback = checkNonWriteConditions(odSub, out OKToContinue);
			if(OKToContinue == false)
			{
				return feedback;
			}
			#endregion don't write to devic e- if we know that the write will be rejected

			#region write to device
			/* If all the previous was OK, get the data type of this data[]][] element
			 * and determine the data size of this type. If the data size is more than
			 * zero then there is some data to be written to the controller using the 
			 * CANcomms object. This is also used to format the new value into the correct
			 * data length for the controller to accept. i.e. DW uses a long to repreent
			 * every single integer type (8,16,24,32,40,48,56 or 64) but this needs to
			 * be converted back into the correct size for the controller to recognise.
			 * Build and transmit an SDO to write the new value to the object.  
			 * This may be a segmented SDO transfer dependent on the data length.
			 */
			CANopenDataType datatype =  (CANopenDataType) odSub.dataType;
			if ( datatype == CANopenDataType.REAL32 )
			{
				uint dataSize = (uint) (datatype.GetHashCode());
				dataSize = sysInfo.dataLimits[ dataSize ].sizeOf;
				if ( dataSize > 0 )
				{
					System.UInt32 floatAsUnsignedBits = (System.UInt32)newValue;
					long newValueAsLong = (long)floatAsUnsignedBits;

					for(ushort i = 0;i<SCCorpStyle.SDONoResponseRetries;i++)
					{
						feedback = sysInfo.CANcomms.SDOWrite( nodeID, odSub.indexNumber, odSub.subNumber, dataSize, newValueAsLong, odSub.commsTimeout );
						if((feedback != DIFeedbackCode.DINoResponseFromController) || (odSub.commsTimeout< SCCorpStyle.TimeoutDefault))
						{
							break;
						}
					}
					
					if( feedback == DIFeedbackCode.DISuccess )
					{
						if( odSub.real32 != null )
						{
							odSub.real32.currentValue = newValue;
						}
						else
						{
							feedback = DIFeedbackCode.DIReal32SetToNullValue;
						}
					}
					else
					{
						if( feedback == DIFeedbackCode.DINoResponseFromController)
						{
							feedback = this.handleNoResponse(odSub);
						}
					}
					if(feedback != DIFeedbackCode.DISuccess)
					{
						feedback = processfbc(odSub,feedback, true);
					}
					this.checkEVAS(odSub);
					return feedback;
				}
				else
				{
					return DIFeedbackCode.DISuccess;
				}
			}
			else
			{
				return DIFeedbackCode.DIFailedToUpdateItemValue;
			}
			#endregion write to device
		}

		//-------------------------------------------------------------------------
		///<summary>Writes the item of given index and sub from device nodeID with newValue (double) if a writable item, sufficient access level and obj flags OK.</summary>
		/// <param name="nodeID">node ID of the controller, used to determine the COBID for the SDO write request</param>
		/// <param name="accessLevel">the access level (0-255) awarded to the user when they logged on to this controller</param>
		/// <param name="nodeState">current NMT state of this node</param>
		/// <param name="index">object index which the new value is to be written to</param>
		/// <param name="subIndex">object sub index for which the new value is to be written to</param>
		/// <param name="newValue">new value which is to be written to the controller as a float value</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback code indicates success or gives a failure reason</returns>
		//-------------------------------------------------------------------------
		internal DIFeedbackCode writeODValue( ODItemData odSub,double newValue )
		{
			#region don't write to devic e- if we know that the write will be rejected
			bool OKToContinue = false;
			DIFeedbackCode feedback = checkNonWriteConditions(odSub, out OKToContinue);
			if(OKToContinue == false)
			{
				return feedback;
			}
			#endregion don't write to devic e- if we know that the write will be rejected
			
			#region write to device
			/* If all the previous was OK, get the data type of this data[]][] element
			 * and determine the data size of this type. If the data size is more than
			 * zero then there is some data to be written to the controller using the 
			 * CANcomms object. This is also used to format the new value into the correct
			 * data length for the controller to accept. i.e. DW uses a long to repreent
			 * every single integer type (8,16,24,32,40,48,56 or 64) but this needs to
			 * be converted back into the correct size for the controller to recognise.
			 * Build and transmit an SDO to write the new value to the object.  
			 * This may be a segmented SDO transfer dependent on the data length.
			 */
			CANopenDataType datatype = (CANopenDataType) odSub.dataType;
			if ( datatype == CANopenDataType.REAL64 )
			{
				uint dataSize = (uint) (datatype.GetHashCode());
				dataSize = sysInfo.dataLimits[ dataSize ].sizeOf;
				if ( dataSize > 0 )
				{
					System.UInt64 doubleAsUnsignedBits = (System.UInt64)newValue;
					long newValueAsLong = (long)doubleAsUnsignedBits;

					for(ushort i = 0;i<SCCorpStyle.SDONoResponseRetries;i++)
					{
						feedback = sysInfo.CANcomms.SDOWrite( nodeID, odSub.indexNumber, odSub.subNumber, dataSize, newValueAsLong, odSub.commsTimeout );
						if((feedback != DIFeedbackCode.DINoResponseFromController) || (odSub.commsTimeout< SCCorpStyle.TimeoutDefault))
						{
							break;
						}
					}
					
					if( feedback == DIFeedbackCode.DISuccess )  
					{
						if(odSub.real64 != null )
						{
							odSub.real64.currentValue = newValue;
						}
						else
						{
							feedback = DIFeedbackCode.DIReal64SetToNullValue;
						}
					}
					else if( feedback == DIFeedbackCode.DINoResponseFromController)
					{
						feedback = this.handleNoResponse(odSub);
					}
					if(feedback != DIFeedbackCode.DISuccess)
					{
						feedback = processfbc(odSub,feedback, true);
					}
					this.checkEVAS(odSub);
				}
				return feedback;
			}
			else
			{
				return DIFeedbackCode.DISuccess;
			}
			#endregion write to device
		}

		//-------------------------------------------------------------------------
		///<summary>Writes the item of given index and sub from device nodeID with newValue (byte array) if a writable item, sufficient access level and obj flags OK.</summary>
		/// <param name="nodeID">node ID of the controller, used to determine the COBID for the SDO write request</param>
		/// <param name="accessLevel">the access level (0-255) awarded to the user when they logged on to this controller</param>
		/// <param name="nodeState">current NMT state of this node</param>
		/// <param name="index">object index which the new value is to be written to</param>
		/// <param name="subIndex">object sub index for which the new value is to be written to</param>
		/// <param name="newValue">new value which is to be written to the controller as a byte array value</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback code indicates success or gives a failure reason</returns>
		//-------------------------------------------------------------------------
		internal DIFeedbackCode writeODValue( ODItemData odSub, byte[] newValue)
		{
			#region don't write to devic e- if we know that the write will be rejected
			bool OKToContinue = false;
			DIFeedbackCode feedback = checkNonWriteConditions(odSub, out OKToContinue);
			if(OKToContinue == false)
			{
				return feedback;
			}
			#endregion don't write to devic e- if we know that the write will be rejected

			
			/* If all the previous was OK, get the data type of this data[]][] element
			 * and determine the data size of this type. If the data size is more than
			 * zero then there is some data to be written to the controller using the 
			 * CANcomms object. This is also used to format the new value into the correct
			 * data length for the controller to accept. i.e. DW uses a long to repreent
			 * every single integer type (8,16,24,32,40,48,56 or 64) but this needs to
			 * be converted back into the correct size for the controller to recognise.
			 * Build and transmit an SDO to write the new value to the object.  
			 * This may be a segmented SDO transfer dependent on the data length.
			 */
			CANopenDataType datatype = (CANopenDataType) odSub.dataType;
			if (  datatype == CANopenDataType.DOMAIN )
			{
				for(ushort i = 0;i<SCCorpStyle.SDONoResponseRetries;i++)
				{
				feedback = sysInfo.CANcomms.SDOWrite( nodeID, odSub.indexNumber, odSub.subNumber, ref newValue, odSub.commsTimeout);
					if((feedback != DIFeedbackCode.DINoResponseFromController) || (odSub.commsTimeout< SCCorpStyle.TimeoutDefault))
					{
						break;
					}
				}
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					odSub.currentValueDomain = new byte[ newValue.Length ];
					newValue.CopyTo( odSub.currentValueDomain, 0 );
				}	
				else
				{
					if( feedback == DIFeedbackCode.DINoResponseFromController)
					{
						feedback = this.handleNoResponse(odSub);
					}
					if(feedback != DIFeedbackCode.DISuccess)
					{
						feedback = processfbc(odSub,feedback, true);
					}
				}
				this.checkEVAS(odSub);
				return feedback;
			}
			else
			{
				feedback = DIFeedbackCode.DIDomainDataTypeExpected;
				return ( processfbc(odSub,feedback, true) );
			}
		}

		//-------------------------------------------------------------------------
		//  Name			: writeObjectValueBlock()
		//  Description     : Writes a new value to the object of Sevcon object type and subIndex
		//					  in the object dictionary on the controller attached which
		//					  has a node ID of nodeID using the SDO block download protocol.  
		//					  This is intended for longer byte arrays requiring a faster download
		//					  time.  The Sevcon object is converted to an index and sub
		//					  which is then converted to find the array element 
		//					  within data[][] which represents this object. This allows the 
		//					  data type, display type, access level etc. to be checked before 
		//					  the new value is written in a CAN SDO packet which is transmitted 
		//					  to the controller.  If successful, the relevant object in the DW's 
		//					  copy of the controller's OD is updated with the new value.
		//					  This function is overloaded for newValues of data type
		//					  long, string and byte arrays.
		//  Parameters      : nodeID - node ID of the controller, used to determine the 
		//							   COBID for the SDO download request
		//					  accessLevel - the access level (0-255) awarded to the user
		//									when they logged on to this controller
		//					  index  -	 object index for which the value it to be written 			  
		//					  subIndex - object sub index for which the value is to be written
		//					  newValue - long, string or byte array containing the new value
		//								 which is to be written to the controller (already
		//							     been error checked by the GUI)
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//  Used Variables  : data[][] - array containing all object & sub definitions and
		//								 their current values
		//  Preconditions   : The controller of the given nodeID has been found and has
		//					  a nodeInfo object defined with the data[][] populated with
		//					  the OD as defined in the found, matching EDS file. 
		//					  The user has selected an object/sub and set a new value which 
		//					  has been error checked by the GUI to be written to the controller.
		//  Post-conditions : The relevant data[][] element's currentValue has been
		//					  updated with the new value after it has successfully been
		//					  written to the relevant object in the controller. Otherwise,
		//					  it remains at the last value read/written from the controller.
		//  Return value    : feedback - DISuccess if the object was written to the controller
		//							OK or a failure code indicating a reason for the failure.
		//----------------------------------------------------------------------------
		///<summary>Writes the item of given index and sub to device nodeID with newValue byte array if a writable item, 
		///sufficient access level and obj flags OK using SDO block download protocol. Intended for faster download
		///of longer byte arrays.</summary>
		/// <param name="nodeID">node ID of the controller, used to determine the COBID for the SDO download request</param>
		/// <param name="accessLevel">the access level (0-255) awarded to the user when they logged on to this controller</param>
		/// <param name="index">object index for which the value it to be written 			  </param>
		/// <param name="subIndex">object sub index for which the value is to be written</param>
		/// <param name="newValue">new value which is to be written to the controller as a byte array</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback code indicates success or gives a failure reason</returns>
		internal DIFeedbackCode writeODValueBlock( ODItemData odSub, byte[] newValue)
		{
			#region don't write to device if we know that the write will be rejected
			bool OKToContinue = false;
			DIFeedbackCode feedback = checkNonWriteConditions(odSub,  out OKToContinue); //use pre-op to jump over the non-required code in this method
			if(OKToContinue == false)
			{
				return feedback;
			}
			#endregion don't write to device if we know that the write will be rejected

			/* * Build and transmit an SDO to write the new value to the object.  
			 * This may be a segmented SDO transfer dependent on the data length.	 */
			if ( (CANopenDataType) odSub.dataType == CANopenDataType.DOMAIN )
			{// only intended and suitable for byte array domain downloads
				// Only use block downloads for reasonable length arrays that are not too long for the max block size data
				if  ( ( newValue.Length > 8 ) && ( newValue.Length <= ( SCCorpStyle.maxSDOBlockSize * 7 ) ) )
				{
					#region block download
                    // allow retries for a block download
                    for (ushort i = 0; i < SCCorpStyle.SDONoResponseRetries; i++)
                    {
					feedback = sysInfo.CANcomms.SDOBlockWrite( nodeID, odSub.indexNumber, odSub.subNumber, ref newValue, odSub.commsTimeout );

                        if (feedback != DIFeedbackCode.DINoResponseFromController)
                        {
                            break;
                        }
                    }

					/* If the SDO was sent and a valid packet was received (no abort codes
			 * etc.) then update this object dictionary with the new value.	 */
					if ( feedback == DIFeedbackCode.DISuccess )
					{
						odSub.currentValueDomain = new byte[ newValue.Length ];
						newValue.CopyTo(odSub.currentValueDomain, 0 );
					}
					else
					{
						if( feedback == DIFeedbackCode.DINoResponseFromController)
						{
							feedback = this.handleNoResponse(odSub);
						}
                        //Jude if we are missing one or more sequence responses then we need to 
                        //pass this back up to ensure blcok is resent as segmented transfer 
                        //but there is not need to tell the user. 
						if((feedback != DIFeedbackCode.DISuccess)
                            && (feedback !=  DIFeedbackCode.DIMissingSequenceResponse))
						{
							feedback = processfbc(odSub,feedback, true);
						}
					}
					this.checkEVAS(odSub);
					return  feedback ;
					#endregion block download
				}
				else
				{	
					#region normal SDO segmented download

					for(ushort i = 0;i<SCCorpStyle.SDONoResponseRetries;i++)
					{
						feedback = sysInfo.CANcomms.SDOWrite( nodeID, odSub.indexNumber, odSub.subNumber, ref newValue, odSub.commsTimeout );
						if((feedback != DIFeedbackCode.DINoResponseFromController) || (odSub.commsTimeout< SCCorpStyle.TimeoutDefault))
						{
							break;
						}
					}
					
					/* If the SDO was sent and a valid packet was received (no abort codes
						 * etc.) then update this object dictionary with the new value.	 */
					if ( feedback == DIFeedbackCode.DISuccess )
					{
						odSub.currentValueDomain = new byte[ newValue.Length ];
						newValue.CopyTo( odSub.currentValueDomain, 0 );
					}	
					else
					{
						if( feedback == DIFeedbackCode.DINoResponseFromController)
						{
							feedback = this.handleNoResponse(odSub);
						}
						if(feedback != DIFeedbackCode.DISuccess)
						{
							feedback = processfbc(odSub,feedback, true);
						}
					}
					this.checkEVAS(odSub);
					return  feedback;
					#endregion normal SDO segmented download
				}
			}
			else
			{
				feedback = DIFeedbackCode.DIDomainDataTypeExpected;
				feedback = processfbc(odSub,feedback, true);
				return feedback;
			}
		}

		protected void checkEVAS(ODItemData odSub)
		{
			if((odSub.indexNumber>=0x1200) && (odSub.indexNumber<=0x1fff))
			{
				this.EVASRequired = true;  //needs defining for Third party Nodes - see CiA spec judetemp TODO
			}
		}
		#endregion

		#region EDS & DCF handling
		//-------------------------------------------------------------------------
		//  Name			: readEDSfile()
		//  Description     : Reads the _EDSorDCFfilepath EDS file (if valid) and constructs
		//					  this nodes dictionary object from the EDS file.
		//  Parameters      : None
		//  Used Variables  : EDS - object used to read an EDS file
		//					  dictionary - object containing a replica OD of the device's
		//								   constructed according to the EDS
		//  Preconditions   : The device has been found on the network and the device
		//					  version (0x1018 object) has been read and used to find
		//					  a matching EDS file of the given filename.
		//  Post-conditions : The EDS file of the given filename has been read and 
		//					  used to construct a replica of the object dictionary held
		//					  in the connected device that this node[] is representing.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		///<summary>Reads the EDS file to construct DW's replica OD while checking the EDS file integrity.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode readEDSfile()
		{
			DIFeedbackCode feedback = DIFeedbackCode.DINoMatchingEDSFileFound;

			// clear out any old data (in case re-reading an EDS file when 0x1018 changes)
			#region check EDS file exists
			if ( _EDSorDCFfilepath  == "")
			{// Only read the EDS file if a matching file has been found.
				SystemInfo.errorSB.Append("\nNo matching EDS file found");
				return DIFeedbackCode.DINoMatchingEDSFileFound;
			}
			#endregion check EDS file exists

			#region open the EDS file
			feedback = EDS_DCF.open( _EDSorDCFfilepath,FileAccess.Read, _nodeID);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				SystemInfo.errorSB.Append("\nFailed to open file");
				return feedback;
			}
			#endregion open the EDS file
			#region read file information section.
			feedback = EDS_DCF.readFileInfoSection(this._EDSorDCFfilepath,out this.EDS_DCF.FileInfo);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				SystemInfo.errorSB.Append("\nFailed to read File Info section");
				return feedback;
			}
			#endregion

			#region read the device information section.
			uint vendorId, productcode, revNo;
			feedback = EDS_DCF.readDeviceInfo(this._EDSorDCFfilepath, out vendorId, out productcode, out revNo);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				SystemInfo.errorSB.Append("\nFailed to read Device Info section");
				return feedback;
			}
			#endregion

			#region read no of objects defined in EDS (Mandatory, Optional and Manufacturer)
			feedback = EDS_DCF.readNoOfObjects(this._EDSorDCFfilepath);
			if(feedback != DIFeedbackCode.DISuccess)
			{
				SystemInfo.errorSB.Append("\nFailed to read num Objects in file");
				return feedback;
			}
			#endregion

			#region read object descriptions
			/* If last section read OK, read all of the object & subs definitions
				 * from the EDS file and use this to construct this node's dictionary
				 * object which replicates the device's OD.
				 */
			feedback = EDS_DCF.readAllObjectDescriptions(this._EDSorDCFfilepath, this ); 

			#region if not read EDS as expected then resize OD 
			/* If failed to read all the expected objects in from the EDS,
					 * resize the dictionary object to eliminate nulls because this
					 * causes exceptions in the GUI code. feedback warns the user of 
					 * the problems reading the EDS.
					 */
//			if ( feedback != DIFeedbackCode.DISuccess )
//			{
//				// some objects misread or missed when reading EDS so resize
//				dictionary.resizeData();
//			}
			#endregion
				
			#region apply access level to header items for ease of GUI processing
			byte prodVariant = 0x00;
			uint intCode = this.EDS_DCF.EDSdeviceInfo.productNumber & 0x00ff0000;
			intCode = intCode >> 16;
			prodVariant = (byte)intCode;

			if((this.EDS_DCF.EDSdeviceInfo.vendorNumber == SCCorpStyle.SevconID)
				&& ( prodVariant > SCCorpStyle.App_variant_lowlimit )
				&& ( prodVariant < SCCorpStyle.App_variant_highlimit ))
			{
				foreach(ObjDictItem odItem in this.objectDictionary)
				{
					if(odItem.odItemSubs.Count>1)
					{
						byte lowestAccessLevel=5;
						foreach(ODItemData odSub in odItem.odItemSubs)
						{
							if((odSub.subNumber!=-1)&&(odSub.subNumber!=0))
							{
								lowestAccessLevel = System.Math.Min(lowestAccessLevel, odSub.accessLevel);
							}
						}
						((ODItemData)odItem.odItemSubs[0]).accessLevel = lowestAccessLevel;
					}
				}
			}
			#endregion apply access level to header items for ease of GUI processing
			#endregion read object descriptions

			#region if the file was opened, close the file stream etc.
			this.EDS_DCF.closeEDSorDCF();
			#endregion
		
			return  ( feedback );
		}

		//-------------------------------------------------------------------------
		//  Name			: findMatchingEDSFile()
		//  Description     : This function is called to open each EDS file in the
		//					  given directory, read the device information section and
		//					  try and find a matching EDS file for the device found
		//					  on the connected system (the device information 0x1018
		//					  is mandatory and has already been read from the connected
		//					  device).  If a match is found, the _EDSorDCFfilepath property
		//					  is set to the matching EDS filename.
		//  Parameters      : None
		//  Used Variables  : _EDSorDCFfilepath - set to "" if no match found and set to the
		//									 matching EDS filename if one is found
		//					  EDS - object used to read an EDS file
		//					  _device - object used to read object 0x1018 from the device
		//  Preconditions   : A device has been found on the connected network and the 
		//					  _nodeID is known and mandatory object 0x1018 has already
		//					  been read from the controller.  Also, any EDS file to be
		//					  used by DriveWizard must be held in the \EDS directory.
		//  Post-conditions : _EDSorDCFfilepath set to "" if no match & to matching filename
		//					  if one is found.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		///<summary>Checks all EDS files in \EDS sub-directory to find a match for the device information read from the physical device.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode findMatchingEDSFile()
		{
            int fileMatchesFound = 0;
			DIFeedbackCode feedback = DIFeedbackCode.DINoMatchingEDSFileFound;
			this._manufacturer = Manufacturer.UNKNOWN;

			foreach(object obj in MAIN_WINDOW.availableEDSInfo)
			{
				AvailableNodesWithEDS devInfo = (AvailableNodesWithEDS) obj;
				if((this.vendorID == devInfo.vendorNumber)
					&& (this.productCode == devInfo.productNumber)
					&& (this.revisionNumber == devInfo.revisionNumber))
				{
					if((this.vendorID == SCCorpStyle.SevconID)
						&& ((this.productCode == 0xFFFFFFFF)
						||(this.revisionNumber == 0xFFFFFFFF)))
					{
						//do not match the EDS - Sevocn devcies should match all 3 params
					}
					else
					{
                        fileMatchesFound++; // no longer break - count number of matches

                        // Keep a record of the first match.
                        if (fileMatchesFound == 1)
					{
						this._EDSorDCFfilepath = devInfo.EDSFilePath;
						feedback = DIFeedbackCode.DISuccess;

						if(this.vendorID == SCCorpStyle.SevconID)
						{
							this._manufacturer = Manufacturer.SEVCON;
						}
						else
						{
							this._manufacturer = Manufacturer.THIRD_PARTY;
                            }
                        }
					}
				}
						}

            if (fileMatchesFound > 1)
            {
                feedback = DIFeedbackCode.DIMultipleMatchingEDSFilesFound;
                System.Windows.Forms.Form manageEDS = new ManageEDSsWindow(vendorID, productCode, revisionNumber);
                manageEDS.ShowDialog();

                // To close manageEDS window, the user has either fixed the problem or
                // selected a file to use. Either way, the first matching file can be used
                // from now on in DW.
                #region Do another search through the list because the contents may have changed.
                foreach (object obj in MAIN_WINDOW.availableEDSInfo)
                {
                    AvailableNodesWithEDS devInfo = (AvailableNodesWithEDS)obj;
                    if ((this.vendorID == devInfo.vendorNumber)
                        && (this.productCode == devInfo.productNumber)
                        && (this.revisionNumber == devInfo.revisionNumber))
                    {
                        if ((this.vendorID == SCCorpStyle.SevconID)
                            && ((this.productCode == 0xFFFFFFFF)
                            || (this.revisionNumber == 0xFFFFFFFF)))
                        {
                            //do not match the EDS - Sevocn devcies should match all 3 params
                        }
                        else
                        {
                            if (this.vendorID == SCCorpStyle.SevconID)
                            {
                                this._manufacturer = Manufacturer.SEVCON;
                            }
                            else
                            {
                                this._manufacturer = Manufacturer.THIRD_PARTY;
					}

                            this._EDSorDCFfilepath = devInfo.EDSFilePath;
                            feedback = DIFeedbackCode.DISuccess;
					break;
                        }
                        }
				}
                #endregion Do another search through the list because the contents may have changed.
			}

			if(feedback != DIFeedbackCode.DISuccess)
			{
				#region user feedback
                if (feedback == DIFeedbackCode.DIMultipleMatchingEDSFilesFound)
                {
                    SystemInfo.errorSB.Append("\nMultiple matching EDS files for:");
                }
                else
                {
                    //DR38000260 improved fault reporting
				    SystemInfo.errorSB.Append("\nNo matching EDS file was found for device:");
                }

				SystemInfo.errorSB.Append("\nVendor ID: 0x");
				SystemInfo.errorSB.Append( this.vendorID.ToString("X").PadLeft(8,'0'));
				if(this.productCode != 0xFFFFFFFF)
				{
					SystemInfo.errorSB.Append("\nProduct Code: 0x");
					SystemInfo.errorSB.Append( this.productCode.ToString("X").PadLeft(8,'0'));
				}
				if(this.revisionNumber != 0xFFFFFFFF)
				{
					SystemInfo.errorSB.Append("\nRevision Number: 0x");
					SystemInfo.errorSB.Append( this.revisionNumber.ToString("X").PadLeft(8,'0'));
				}

                if (vendorID == SCCorpStyle.SevconID) //DR38000260
                {
                    SystemInfo.errorSB.Append("\nPlease contact Sevcon to obtain the EDS file, quoting the above product code and revision number.\n");
                }
                else
                {
                    SystemInfo.errorSB.Append("\nPlease contact the device manufacturer to obtain the EDS file, quoting the above product code and revision number.\n");
                }
                #endregion user feedback
			}
			return ( feedback );
		}

		public DIFeedbackCode findMatchingXMLFile()
		{
			DirectoryInfo di = new DirectoryInfo( MAIN_WINDOW.UserDirectoryPath + @"\EDS");
			FileInfo[] fileInfos = di.GetFiles("*.xml");
//			string [] treeviewXMLFIles = Directory.GetFiles(MAIN_WINDOW.UserDirectoryPath + @"\EDS","*.xml");
			char [] wildcards = "*".ToCharArray();
			SevconTree treeObject = new SevconTree();
			ObjectXMLSerializer objectXMLSerializer = new ObjectXMLSerializer();			

			foreach(FileInfo fi in fileInfos)
			{
				if(fi.Name.ToUpper() == SCCorpStyle.DefaultXMLFile) //DR38000265
				{
					continue;
				}
				#region search for mathcing XML file
				treeObject = ( SevconTree ) objectXMLSerializer.Load( treeObject, fi.FullName );
				if(treeObject == null)
				{
			//		int stopHere = 0;
				}
				foreach(VendorIDType xmlvendor in treeObject.vendors)
				{
					string vendStr = "0x" + this.vendorID.ToString("X").ToLower().PadLeft(8,'0');
					if(xmlvendor.vendorNoValue == vendStr)
					{
						foreach(ProductCodeType productcode in xmlvendor.productCodes)
						{//now test the product codes
							#region product code and rev No compare
							//remove wildcards from product code in xml file
							string XMLprodVal = productcode.prodCodeValue.TrimEnd(wildcards).ToLower();
							string prodStr = "0x" + this.productCode.ToString("X").ToLower().PadLeft(8,'0');
							prodStr = prodStr.Substring(0,XMLprodVal.Length); //trim off the wildcard characters
							if(XMLprodVal == prodStr)
							{
								//check the array list of mathicn revision numbers
								foreach(revisionNumberType revNo in productcode.revisionNumbers)
								{
									#region revision number compare
									string XMLrevVal = revNo.revNoValue.TrimEnd(wildcards).ToLower();
									string revStr = revStr = "0x" + this.revisionNumber.ToString("X").ToLower().PadLeft(8,'0');
									revStr = revStr.Substring(0,XMLrevVal.Length); //remove any values set to wild cards
									if(XMLrevVal == revStr)
									{
										this.XMLfilepath = fi.FullName;
                                        this.XMLFileMatchFound = true; //DR38000265 flag XML match found
										return DIFeedbackCode.DISuccess;
									}
									#endregion revision number compare
								}
							}
							#endregion product code and rev No compare
						}
					}
				}
				#endregion search for mathcing XML file

			}

//			foreach(string xmlFile in treeviewXMLFIles)
//			{
//				#region search for mathcing XML file
//				#endregion search for mathcing XML file
//			}
			//we did not find a match so use default if we can
			#region append error if this is a Sevocn device with no XML
			if(this.vendorID == SCCorpStyle.SevconID)
			{
                //DR38000265 & DR38000260
				SystemInfo.errorSB.Append("\nExpected XML Tree not found for Sevcon device: \nProduct code: 0x");
				SystemInfo.errorSB.Append(this.productCode.ToString("X").PadLeft(8, '0'));
				SystemInfo.errorSB.Append("\nRevision number 0x");
				SystemInfo.errorSB.Append(this.revisionNumber.ToString("X").PadLeft(8, '0'));
                SystemInfo.errorSB.Append("\nPlease contact Sevcon to obtain the XML file, quoting the above product code and revision number.\n");
				return DIFeedbackCode.DINoMatchingXMLFileFound;  //we were expecting an XML file - this is a SEvocn device
			}
			#endregion append error if this is a Sevocn device with no XML
            //DR38000265
			if(File.Exists(MAIN_WINDOW.UserDirectoryPath + @"\EDS\" + SCCorpStyle.DefaultXMLFile) == true)
			{
                this.XMLfilepath = MAIN_WINDOW.UserDirectoryPath + @"\EDS\" + SCCorpStyle.DefaultXMLFile;  //we did not find a match - use default
				return DIFeedbackCode.DISuccess; //was third party so we used correct
			}
			else
			{ 
				this.XMLfilepath = "";
				return DIFeedbackCode.DINoMatchingXMLFileFound; //was third party so we used correct
			}
		}
		#endregion


		#region read Sevcon CANGeneralError abort code string and write save command to device
		//-------------------------------------------------------------------------
		//  Name			: readSevconGeneralAbortCode()
		//  Description     : This function reads the abort code associated with the
		//					  last CANGeneralError which was received due to the
		//					  last message the DW communicated to this node.  This node
		//					  must also be a Sevcon node which is not in boot mode.
		//					  This abort code is read and the EDS is checked to extract
		//					  an abort string message associated with this.
		//  Parameters      : abortMessage - out value with the abort string message 
		//								associated with the last CANGeneralError rx'd
		//  Used Variables  : dictionary - the object containing the replica object
		//								dictionary for this node.
		//  Preconditions   : Node was found with an EDS match to construct the object
		//					  dictionary and some prior communication generated a
		//					  CANGeneralError as a response (must be a Sevcon node).
		//  Post-conditions : abortMessage contains the text string description of
		//					  why the last CAN communications was giving a CANGeneralError.
		//  Return value    : F is set to DISuccess if this CANGeneralError
		//				      is not to be displayed to the user or it remains as a
		//					  CANGeneralError if the user must be notified.  
		//					  AbortMessage is returned as an out parameter.
		//--------------------------------------------------------------------------	
		///<summary>If a CANgeneral error is given from a Sevcon device, an additional text string is read to give a more specific failure reason.</summary>
		/// <param name="abortMessage">string with CANGeneralError specific reason from Sevcon node</param>
		/// <returns>feedback code indicating whether it was a CANGeneralError to be ignored or not.</returns>
		public DIFeedbackCode readSevconGeneralAbortCode( out string abortMessage )
		{
			#region local variable declarations/initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
			DIFeedbackCode returnFbc = DIFeedbackCode.CANGeneralError;
			abortMessage = "";
			ODItemData currAbortodSub = null;
			#endregion local variable declarations/initialisation

			#region only read Sevcon abort code if this is a Sevcon Application node 
			
			if(this.isSevconBootloader() == true)
			{
				#region  set abort sub to the bootloader abort sub
				// default message in case abort code cannot be read
				abortMessage = "<Unable to read Sevcon abort code>";
				currAbortodSub = getODSubFromObjectType(SevconObjectType.BOOT_ERROR_CODE, 0x00);
				#endregion  set abort sub to the bootloader abort sub
			}
			else if ( this.isSevconApplication() == true )
			{
				#region set abort sub to the application abort sub
				// default message in case abort code cannot be read
				abortMessage = "<Unable to read Sevcon abort code>";
				currAbortodSub = readAbortsub;
				#endregion set abort sub to the application abort sub
			}
			if(currAbortodSub != null)
			{
				#region read the current value of the ABORT_CODE (number) from the controller
				feedback = readODValue(currAbortodSub);
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					if ( currAbortodSub.formatList == "" ) //back compatible with bootloader
					{
						#region if no strings assoc. with ABORT_CODE from the EDS then build default string
						abortMessage = "<abort code " + currAbortodSub.currentValue.ToString() + " - no description available>";
						#endregion
					}
					else
					{
						#region get the text string equivalent for the ABORT_CODE number read
						// Find if there is an abort code match in the formatList read from the EDS
                        //2 indicates empty domain - this is NOT an erro condittion so prevent reproting it as such.
                        if (currAbortodSub.currentValue == 2)
                        {
                            returnFbc = DIFeedbackCode.DISuccess;
                        }
                        else
                        {
                            abortMessage = sysInfo.getEnumeratedValue(currAbortodSub.formatList, currAbortodSub.currentValue);
                        }
						#endregion get the text string equivalent for the ABORT_CODE number read
					}
				}
				#endregion
			}
			#endregion
			return ( returnFbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: saveCommunicationParameters()
		//  Description     : Indicates to the physical device to save it's new 
		//					  communication profile parameters.
		//  Parameters      : CANComms - CAN communications object (tx & rx on CANbus)
		//					  abortMessage - string identifier sent by a Sevcon node to
		//  Used Variables  : None
		//  Preconditions   : The system has been found and DW has written some new values
		//					  to objects in the communication profile area that the 
		//					  controller needs to save (otherwise lost after a power
		//					  cycle).
		//  Post-conditions : The device has saved it's current commuication profile objects
		//					  to EEPROM.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		/// <summary>Indicates to the physical device to save it's new communication profile parameters.</summary>
		/// <param name="CANComms">CAN communications object (tx and rx on CANbus)</param>
		/// <param name="abortMessage">string with CANGeneralError specific reason from Sevcon node</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode saveCommunicationParameters( )
		{
			if ( this.isSevconApplication() == false )
			{
				MAIN_WINDOW.appendErrorInfo = false;//switch off error info - we are just checking
			}
			#region comments
			//if this is a Sevcon app then this this sub should be in OD 
			//- everything esle we are just chekcing for its existence
			// if the object doesn't exist in the 3rd party EDS then perhaps it doesn't need this
			// save backwards to save the 1200 to 1fff objects.  
			/* Write 'save' backwards to STORE object in order to save, 
			 *  objects 1200 to 1fff to EEPROM, after checking this object is valid for this device.
			 */
			#endregion comments
			ODItemData evasSub = getODSub(0x1010, 1);
			MAIN_WINDOW.appendErrorInfo = true; //ensure it is back on
			if ( evasSub != null) //exists in OD
			{
				#region write EVAS
				DIFeedbackCode feedback = writeODValue( evasSub, SCCorpStyle.SaveBackwardsValue );
				if(feedback == DIFeedbackCode.DISuccess)
				{
					this.EVASRequired = false;
				}
				return feedback;
				#endregion write EVAS
			}
			else
			{
				return DIFeedbackCode.DIInvalidIndexOrSub;
			}
		}
		#endregion
		
		#region private functions
		//-------------------------------------------------------------------------
		//  Name			: dynamicallyResizeArrayType()
		//  Description     : Array lengths are specified by sub 0 & if the read 
		//					  value from the device is of a different length to that 
		//					  read in from the EDS (or last read from device) then this
		//					  function is called to dynamically resize the DW's copy of
		//					  this object to match the device.
		//  Parameters      : arrayData[] - dictionary array of elements currently
		//								    representing the DW's representation for
		//									this object and it's subs
		//  Used Variables  : dictionary - contains a replica OD of the device's,
		//								   constructed according to the EDS
		//  Preconditions   : System has been found & dictionary constructed from the EDS
		//					  and _nodeID is found and the array object type length has been
		//					  read from the device & is of a different size to that stored
		//					  in the DW's copy.
		//  Post-conditions : The relevant arrayData[] in the dictionary object
		//					  has been resized to match the length of the device object,
		//					  and has been populated with data to match the other elements
		//					  in the array (e.g. data type).
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Resizes an array object in DW's OD if the physical device indicates more items in the array than previously expected from the EDS.</summary>
		/// <param name="arrayData">dictionary array of elements currently representing the 
		/// DW's representation for this object and it's subs</param>
		/// <param name="CANComms">CAN communications object (tx and rx on CANbus)</param>
		private void dynamicallyResizeArrayType(  ObjDictItem odItemToresize )
		{
			DIFeedbackCode feedback;
			// initialise to invalid values & keep the compiler happy

			/* Read the array length from the controller.  This is always held in sub 0 and 
			 * of an unsigned integer 8 type.
			 */
			ODItemData numItemsSub = this.getODSub(((ODItemData)odItemToresize.odItemSubs[ 0 ]).indexNumber, 0);
			if(numItemsSub != null)
			{
				feedback = this.readODValue(numItemsSub);
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					if (numItemsSub.currentValue< 256)
					{
						/* Only resize if bigger than the existing array.  Add 2 to currentValue to
						 * account for DW's header object and sub0 is not counted in the length
						 * returned by itself.	 */
						if ( ( numItemsSub.currentValue + 2 ) > odItemToresize.odItemSubs.Count )
						{
							// must update the number of subs to the match resizing

							// cpy over existing data elements of the array
							ODItemData odSubtoCopy = null;
							foreach(ODItemData odSub in odItemToresize.odItemSubs)
							{
								// make new array point to original data
								#region save off index of a representative sample of the array element (used below)
								if ( odSub.subNumber >=1)
								{
									/* DS spec says subs in arrays should be numbered consecutively from 0.
									 * Renumber here if what was read in the EDS does not conform. */
									odSub.subNumber = odItemToresize.odItemSubs.IndexOf(odSub) - 1;
									odSubtoCopy = odSub;
									break; //found it - get out 
								}
								#endregion save off index of a representative sample of the array element (used below)
							}
							((ODItemData) odItemToresize.odItemSubs[ 0 ]).subNumber = (int)(numItemsSub.currentValue + 2);
							/* If found suitable element of the array to copy over, copy over the required 
							 * elements to fill the remainder of the array. */
							if ( odSubtoCopy!= null)
							{
								for ( int sub = odItemToresize.odItemSubs.Count; sub < numItemsSub.currentValue + 2; sub++ )
								{
									ODItemData newodsub = new ODItemData(); 
									#region copy over common elements into the new sub
									newodsub.indexNumber = odSubtoCopy.indexNumber; 
									newodsub.subNumber = sub - 1 ; 
									newodsub.accessLevel = odSubtoCopy.accessLevel; 
									newodsub.accessType = odSubtoCopy.accessType; 
									newodsub.dataType = odSubtoCopy.dataType; 
									newodsub.defaultValue = odSubtoCopy.defaultValue; 
									newodsub.displayType = odSubtoCopy.displayType; 
									newodsub.highLimit = odSubtoCopy.highLimit; 
									newodsub.lowLimit = odSubtoCopy.lowLimit; 
									newodsub.objectName = odSubtoCopy.objectName; 
                                    newodsub.objectNameString = getObjectNameAsString(odSubtoCopy.objectName);
									newodsub.objectType = odSubtoCopy.objectType; 
									newodsub.objFlags = odSubtoCopy.objFlags; 
									newodsub.parameterName = odSubtoCopy.parameterName + " " + newodsub.subNumber.ToString(); 
									newodsub.PDOmappable = odSubtoCopy.PDOmappable; 
									newodsub.scaling = odSubtoCopy.scaling; 
									newodsub.sectionType = odSubtoCopy.sectionType; 
                                    newodsub.sectionTypeString = getSectionTypeAsString(odSubtoCopy.sectionType);
									newodsub.units = odSubtoCopy.units; 
									odItemToresize.odItemSubs.Add(newodsub);
									#endregion
								} // end of for rest of elements in new array
							} // end of if valid representative element in array has been found
							// now make the existing array reference point to the new data
						} // end of if need to resize the array
					} // end of if successfully found array length 
				}
			}
			else
			{
				feedback = DIFeedbackCode.DIInvalidIndexOrSub;
			}
			#region comments
			/* Retrieve from the DW's OD the current size of this array (in sub 0).
			 */

			/* If the size is not the same as array length stated in the EDS ( or previously read)
			 * then we need to resize to match data.
			 * Copy all the existing subs information over then find a representative element of the 
			 * existing array to copy over all the details for the new additional elements.
			 */
			#endregion comments
		}
		//-------------------------------------------------------------------------
		//  Name			: removeInvalidItemsFromOD()
		//  Description     : Removes any items that when they were read from the physical
		//					  device got a CANObjectDoesNotExistInOD or sub equivalent
		//					  from the DW's replica copy of the object dictionary.
		//					  This may arise when the EDS does not match exactly the
		//					  physical device (eg an array of values has the most ever
		//					  that could be present but the physical device only has a
		//					  smaller subset).  By removing from DW's OD, it prevents
		//					  getting this message repeatedly throughout every
		//					  following DW action on the missing items.
		//  Parameters      : None
		//  Used Variables  : dictionary - contains a replica OD of the device's,
		//								   constructed according to the EDS
		//  Preconditions   : The physical device is found, the EDS used to construct
		//					  DW's OD, readEntireOD() has been called and there has been
		//					  items found which do not exist in the physical device but
		//					  are in the EDS.
		//  Post-conditions : DW's OD has been trimmed to remove items that don't exist
		//					  so that it's OD should now match the physical device.
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Removes any objects that do not exist in the physical device from DW's replica OD.</summary>
		private void removeInvalidItemsFromOD()
		{
			#region find out how many objects are still valid
			ArrayList itemsToGo = new ArrayList();
			foreach(ObjDictItem odItem in objectDictionary)
			{
				ArrayList subsToGo = new ArrayList();
				foreach(ODItemData odSub in odItem.odItemSubs)
				{
					if ( odSub.invalidItem == true)
					{
						subsToGo.Add(odSub);
					}
				}
				foreach(ODItemData subToGo in subsToGo)
				{
					odItem.odItemSubs.Remove(subToGo);
					if(odItem.odItemSubs.Count<=2) 
					{
						ODItemData numItemsSub = this.getODSub(subToGo.indexNumber, 0);
						if((numItemsSub != null) && (numItemsSub.isNumItems == true))
						{
							itemsToGo.Add(odItem);
						}
					}
				}
			}
			foreach(ArrayList itemTogo in itemsToGo)
			{
				this.objectDictionary.Remove(itemTogo);
			}
			#endregion
		}

		//-------------------------------------------------------------------------
		//  Name			: 
		//  Description     : 
		//  Parameters      : 
		//  Used Variables  : 
		//  Preconditions   : 
		//  Post-conditions : 
		//  Return value    : 
		//--------------------------------------------------------------------------
		/// <summary></summary>
		/// <param name="dataValue"></param>
		/// <param name="scrambledValue"></param>
		/// <returns></returns>
		static ushort scramble ( ushort dataValue, ushort scrambledValue )
		{
			ushort input = 0;    

			for ( ushort i = 0; i < 16; i++ )
			{
				input = (ushort)( (ushort)( dataValue ^ scrambledValue ) & (ushort)1 );
    
				scrambledValue >>= 1;
				dataValue      >>= 1;
    
				if ( input != 0 )
				{
					scrambledValue ^= 0x8C8C;
				}
			}

			return ( scrambledValue );
		}


		protected DIFeedbackCode handleNoResponse(ODItemData odSub)
		{
			#region test for expected non response
			if(odSub.commsTimeout < SCCorpStyle.TimeoutDefault)
			{
				//we are expecting a timeout - report success
				return DIFeedbackCode.DISuccess;
			}
			else
			{
				this.numConsecutiveNoResponse++;
                if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
				{
					return DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice;
				}
				else
				{
					return DIFeedbackCode.DINoResponseFromController;
				}
			}
			#endregion test for expected non response
		}

        /// <summary>
        /// Converts a Sevcon object name to the equivalent string value, checking it is within bounds
        /// of the dynamically build sevconObjectIDList. 
        /// </summary>
        /// <param name="objectName">object name as an integer</param>
        /// <returns>string equivalent of objectName</returns>
        protected string getObjectNameAsString(int objectName)
        { 
            string retVal = "NONE";

            if (objectName < sysInfo.sevconObjectIDList.Count)
            {
                retVal = sysInfo.sevconObjectIDList[objectName].ToString(); 
            }

            return (retVal);
        }

        /// <summary>
        /// Converts a Sevcon section type to the equivalent string value, checking it is within bounds
        /// of the dynamically build sevconSectionIDList. 
        /// </summary>
        /// <param name="sectionType">Sevcon section type as an integer</param>
        /// <returns>string equivalent of sectionType</returns>
        protected string getSectionTypeAsString(int sectionType)
        { 
            string retVal = "NONE";

            if (sectionType < sysInfo.sevconSectionIDList.Count)
            {
                retVal = sysInfo.sevconSectionIDList[sectionType].ToString(); 
		}

            return (retVal);
        }
		#endregion

		#region PDO Mapping public functions
		public DIFeedbackCode checkPDOsAlreadyMapped( ArrayList COBsInSystem )
		{
            foreach (ODItemAndNode itemAndNode in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
			{
				if (itemAndNode.node == this )
				{
					#region only check if this item in the list is on this node
					ushort numTimesMapped = 0;  
					//list[ i ].COBID = 0;
					foreach(COBObject COB in COBsInSystem)
					{
						if(COB.messageType == COBIDType.PDO)
						{
							#region seach the COBsInSystem to see if this tiem has alreaded bee put into a Tx map
							foreach(COBObject.PDOMapData txData in COB.transmitNodes)
							{
								foreach (PDOMapping map in txData.SPDOMaps)
								{
									//extract the Index and sub
									long mapValCopy = map.mapValue;
									int mappedIndex = (int) (mapValCopy & 0xFFFF0000) >>32;
									mapValCopy = map.mapValue;
									int mappedSub = (int) (mapValCopy & 0x0000FF00) >>16;
									int realSubIndex = itemAndNode.ODparam.subNumber;
									if( itemAndNode.ODparam.bitSplit != null)
									{
										realSubIndex = itemAndNode.ODparam.bitSplit.realSubNo;
									}
									if(( itemAndNode.ODparam.indexNumber == mappedIndex) 
										&& (realSubIndex == mappedSub))
									{
										numTimesMapped++;
									}
								}
							}
							#endregion seach the COBsInSystem to see if this tiem has alreaded bee put into a Tx map
						}
					}
					if ( ( numTimesMapped >= 2 ) && ( this.isSevconApplication() == true ) )
					{
						#region error user feedback and exit
						SystemInfo.errorSB.Append("Unable to map item 0x");
						SystemInfo.errorSB.Append(itemAndNode.ODparam.indexNumber.ToString( "X" ));
						SystemInfo.errorSB.Append(" sub ");
						SystemInfo.errorSB.Append(itemAndNode.ODparam.subNumber.ToString( "X" ));
						SystemInfo.errorSB.Append(" as already mapped ");
						SystemInfo.errorSB.Append(numTimesMapped.ToString() );
						SystemInfo.errorSB.Append(" times");
						return DIFeedbackCode.DIPDOAlreadyMappedTwiceOnSevconNode;
						#endregion error user feedback and exit
					}
					#endregion
				}
			}
			return ( DIFeedbackCode.DISuccess );
		}

		//-------------------------------------------------------------------------
		//  Name			: setupAndWriteMonitorTxPDOs()
		//  Description     : Builds up the transmit COB routing and PDO mapping objects
		//					  needed for the graphical monitoring of the list of items.
		//					  It checks to see if there are enough transmit PDOs free
		//					  so that all items in the list can be mapped (ideal secenario
		//					  as DW can control the timebase of the PDO). If this isn't
		//					  possible, then we can use the PDO maps that exist already
		//					  which contain some items in the list (but we have no control
		//					  over the timebase) and just map the other items into new
		//					  transmit PDOs.  This still provides a better timebase accuracy
		//					  than using SDOs.
		//  Parameters      : list - the list of items the user has selected to monitor
		//						(which lists it by index and sub index).  The structure
		//						contains other items which are found by the DI during
		//						the checks.
		//					  selectedCOBIDs - list of COBIDs selected for use for these
		//						new monitor transmit PDO map & comms objects
		//					  txPDOCommsAvailable - number of cofigurable transmit PDOs
		//						are available
		//					  maxNoPDOMapsRequired - previously found ideal number of transmit
		//						PDOs needed for graphical monitoring
		//					  minNoPDOMapsRequired - previously found minimum acceptable
		//						number of PDOs needed for graphical monitoring
		//					  monitorTimebase - user requested timebase for graphical
		//						monitoring (ie frequency PDOs to be transmitted at)
		//  Used Variables  : monitorTxPDO - used to hold the calculated transmit PDO maps
		//						needed for this node
		//  Preconditions   : Previous checks that the items in the list are all PDO mappable
		//					  has been made, COBIDs have been selected based on the user-selected
		//					  priority and the min and max PDOs needed has been calculated.
		//  Post-conditions : monitorTxPDO contains the new maps needed for this controller
		//					  to graph the items in the list.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		/// <summary>Calculates the transmit PDO map and comms objects needed for monitoring of this list.</summary>
		/// <param name="list">he list of items the user has selected to monitor
		/// (which lists it by index and sub index).  The structure contains other items which 
		/// are found by the DI during the checks</param>
		/// <param name="txPDOCommsAvailable">number of cofigurable transmit PDOs are available</param>
		/// <param name="selectedCOBIDs">list of COBIDs selected for use for these
		/// new monitor transmit PDO map AND comms objects</param>
		/// <param name="maxNoPDOMapsRequired">number of cofigurable transmit PDOs
		/// are available</param>
		/// <param name="minNoPDOMapsRequired">previously found minimum acceptable
		/// number of PDOs needed for graphical monitoring</param>
		/// <param name="monitorTimebase">user requested timebase for graphical
		/// monitoring (ie frequency PDOs to be transmitted at)</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode setupAndWriteMonitorTxPDOs( int [] selectedCOBIDs, uint monitorTimebase, ArrayList COBsInSystem )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
			long itemMapping;
			int maxNumMappingsPerPDO = 0;
			#endregion

			#region work out the maximum number of items that can be mapped in a PDO on this node type
			if ( this.isSevconApplication() == true )
			{
				maxNumMappingsPerPDO = 8;
			}
			else
			{
				maxNumMappingsPerPDO = 64;
			}
			#endregion

			int listPtr = 0;
            sysInfo.CANcomms.VCI.MonitoringCOBs = new ArrayList();
			foreach(int availCOBID in selectedCOBIDs)
			{
				COBObject newMonitoringCOB = new COBObject();
				#region setup COB level data for this COB
				newMonitoringCOB.createdForCalibratedGraphing = true; //mark it so we remove it later
				newMonitoringCOB.COBID = availCOBID;
				newMonitoringCOB.requestedCOBID = availCOBID;
				newMonitoringCOB.messageType = COBIDType.PDO;
				
				#region asynchronous transmit PDO
				/* Need to decide what these will be for monitoring.
									 * Asynchronous, transmitted on change (dependent on timebase) and
									 * an event timer guaranteed to transmit (dependent on timebase) max.
									 */
				newMonitoringCOB.TxType = 255;  //will get chenged to be in Sync  range and flags set acccordingly
				#endregion

				#region calculate the event time based on the monitorTimebase and ceiling limit
				newMonitoringCOB.eventTime =  (int) Math.Min(monitorTimebase, SCCorpStyle.eventTimeMax);
				#endregion

				#region calculate the inhibit time based on the monitorTimebase and ceiling limit
				newMonitoringCOB.inhibitTime = (int) Math.Min(( monitorTimebase * 10 ), SCCorpStyle.inhibitTimeMax) ;
				#endregion
				#endregion setup COB level data for this COB

				#region now stuff some of the monitoiring list into this COB
				//create a Tx mapping object
				COBObject.PDOMapData txData = new DriveWizard.COBObject.PDOMapData();
				txData.nodeID = this._nodeID;
				txData.mapODIndex = (int) this.disabledTxPDOIndexes[0] + SCCorpStyle.PDOToCOBIDObjectOffset;
				int mappedItemsInThisPDO = 0;
				int bitsInThisMapping = 0;
                //foreach (ODItemAndNode itemAndNode in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
                while ( listPtr < sysInfo.CANcomms.VCI.OdItemsBeingMonitored.Count )
				{
                    ODItemAndNode itemAndNode = (ODItemAndNode)sysInfo.CANcomms.VCI.OdItemsBeingMonitored[listPtr];
					if(itemAndNode.node == this)
					{
						itemAndNode.ODparam.monPDOInfo = new MonitoringPDOInfo();
						int realSubNumber = itemAndNode.ODparam.subNumber;
						short realDataSize =  itemAndNode.ODparam.dataSizeInBits;
						if(itemAndNode.ODparam.bitSplit!= null)
						{
							realDataSize = (short) this.getOrigDataSizeInBits((CANopenDataType) itemAndNode.ODparam.dataType);
							realSubNumber = itemAndNode.ODparam.bitSplit.realSubNo;
						}
						itemMapping = ( itemAndNode.ODparam.indexNumber << 16 ) + ( realSubNumber << 8 ) + realDataSize;
						itemAndNode.ODparam.monPDOInfo.COB = newMonitoringCOB;
						itemAndNode.ODparam.monPDOInfo.startBitInPDO = bitsInThisMapping;
						PDOMapping map = new PDOMapping((long) itemMapping, itemAndNode.ODparam.parameterName);
						if(txData.SPDOMaps.Contains(map) == false)
						{ //if the mapping is already in then this will be a bitsplit item
							//where another member of the bitsplit is also in the monitoring list 
							// we only add the real underlying sub to the mappings
							//a dn we only add it once
                            // only add new mapping if it will fit
                            if (((bitsInThisMapping + realDataSize) <= 64) && (mappedItemsInThisPDO < maxNumMappingsPerPDO))
                            {
							    txData.SPDOMaps.Add(map);
							    bitsInThisMapping += realDataSize;
							    mappedItemsInThisPDO++;
                            }
						}
						#region check whether any more items can be fitted into this mapping
						if ( ( mappedItemsInThisPDO >= maxNumMappingsPerPDO ) 
							|| (bitsInThisMapping >= 64) )
						{
                            listPtr++; //inc before we leave so we don't map it twice
							break;
						}
						#endregion check whether any more items can be fitted into this mapping
					}
                    listPtr++;  //point to the next monitoring item
				}
				#endregion now stuff some more of the monitoiring list into this COB

				#region finally add this COB to COBsInSystem and wirte data to the CAN node
				newMonitoringCOB.transmitNodes.Add(txData);  //add the mapping to the COBObject tranmitnodes List
				COBsInSystem.Add(newMonitoringCOB);  //add this to overall picture
                sysInfo.CANcomms.VCI.MonitoringCOBs.Add(newMonitoringCOB);
                updatePDOMappings( newMonitoringCOB, true, 0);     //write mappings to controller
				this.addTxNodeToPDO(newMonitoringCOB);
                if (disabledTxPDOIndexes.Count > 0) //DR38000270 check made to prevent exception when no tx pdos free
                {
                    this.disabledTxPDOIndexes.RemoveAt(0); //we are going to be using this one soknow it off the list
                }
				#endregion finally add this COB to COBsInSystem and wirte data to the CAN node
			}
			return ( feedback );
		}

		//-------------------------------------------------------------------------
		//  Name			: updateMonitoringTimebase()
		//  Description     : Writes the new values for the event time and inhibit time
		//					  on the physical device for every transmit PDO comms object
		//					  which was written there specifically for graphing by DW.
		//					  Other tx PDOs cannot be changed as this could affect the
		//					  system performance.
		//  Parameters      : newMonitorTimebase - new user selected timebase required 
		//					  CANComms - CAN communications object (tx & rx on CANbus)
		//  Used Variables  : monitorTxPDO - used to hold the calculated transmit PDO maps
		//						needed for this node
		//					  existingTxPDO - holds the save, compacted version of
		//						all the existing transmit PDO & comms on this node
		//                    dictionary - contains a replica OD of the device's,
		//								   constructed according to the EDS
		//  Preconditions   : monitorTxPDOs previously been calculated and written to
		//					  the physical device, existingTxPDOs contains the saved
		//					  copy of the original transmit PDO & comms for this node.
		//  Post-conditions : The monitorTxPDOs has had the event & inhibit times modified
		//					  to reflect the new timebase and these have also been written
		//					  to the physical device.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		/// <summary>Updates the event and inhibit time on device to achieve the newMonitorTimebase.</summary>
		/// <param name="CANComms">CAN communications object (tx and rx on CANbus)</param>
		/// <param name="newMonitorTimebase">new user selected timebase required</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode updateMonitoringTimebase( COBObject COBToModify, uint newMonitorTimebase)
		{
			DIFeedbackCode feedback = DIFeedbackCode.DIInvalidIndexOrSub;
			#region get the relevent OD subs
			COBObject.PDOMapData txData  = (COBObject.PDOMapData) COBToModify.transmitNodes[0];
			int COBObjectIndex = txData.mapODIndex - SCCorpStyle.PDOToCOBIDObjectOffset;
			ODItemData COBIDODsub = getODSub( COBObjectIndex, SCCorpStyle.PDOCommsCOBIDSubIndex);
			ODItemData inhibitSub = this.getODSub(COBObjectIndex, SCCorpStyle.PDOCommsInhibitTimeSubIndex);
			ODItemData eventSub = this.getODSub(COBObjectIndex, SCCorpStyle.PDOCommsEventTimeSubIndex);
			#endregion get the relevent OD subs

			if((COBIDODsub != null) && (inhibitSub != null) && ( eventSub!= null))
			{
				#region update the inhibit an dmontiring time for this COB
				#region set the inhibit time based on the newMonitorTimebase
				if ( ( newMonitorTimebase * 10 ) <= SCCorpStyle.inhibitTimeMax )
				{
					COBToModify.inhibitTime = (int)( newMonitorTimebase * 10 );
				}
				else
				{
					COBToModify.inhibitTime = SCCorpStyle.inhibitTimeMax;
				}
				#endregion

				#region set the event time based on the newMonitorTimebase
				if ( newMonitorTimebase <= SCCorpStyle.eventTimeMax )
				{
					COBToModify.eventTime = (int)newMonitorTimebase;
				}
				else
				{
					COBToModify.eventTime = SCCorpStyle.eventTimeMax;
				}
				#endregion

				#endregion

				// update on the controller if we calculated some monitorTxPDOs before that were written to the device

				#region only update if this map was written by DriveWizard ie it's a monitor transmit PDO
				long COBIDOnNode = 0x00;

				#region write new communication object

				#region invalidate sub 1 which is COB-ID (cannot write to other subs if COB is enabled)
				if ( COBIDODsub != null)  //object exists in OD
				{
					feedback = this.readODValue(COBIDODsub);
					COBIDOnNode = COBIDODsub.currentValue;
					COBIDOnNode |= (uint)SCCorpStyle.Bit31Mask; 
					feedback = writeODValue( COBIDODsub, COBIDOnNode );
				}
				#endregion

				#region write the inhibit time to the device
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					feedback = writeODValue(inhibitSub, (long)COBToModify.inhibitTime );
				}
				#endregion

				#region write the event time to the device
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					feedback = writeODValue( eventSub, (long)COBToModify.eventTime );
				}
				#endregion

				#region enable the COBID for this transmit object to the device
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					long newValue = (uint)COBIDOnNode & (uint)~SCCorpStyle.Bit31Mask; 
					feedback = writeODValue( COBIDODsub, newValue);
				}
				#endregion

				#endregion
				#endregion
			}
			return ( feedback );
		}

		//########
		//-------------------------------------------------------------------------
		//  Name			: changeCOBID()
		//  Description     : Determines a unique COBID that reflects the requested priority
		//					  on the physical device for every transmit PDO comms object
		//					  which was written there specifically for graphing by DW.
		//					  Other tx PDOs cannot be changed as this could affect the
		//					  system performance.
		//  Parameters      : newMonitorPriority - new user selected priority required 
		//					  CANComms - CAN communications object (tx & rx on CANbus)
		//  Used Variables  : monitorTxPDO - used to hold the calculated transmit PDO maps
		//						needed for this node
		//					  existingTxPDO - holds the save, compacted version of
		//						all the existing transmit PDO & comms on this node
		//                    dictionary - contains a replica OD of the device's,
		//								   constructed according to the EDS
		//  Preconditions   : monitorTxPDOs previously been calculated and written to
		//					  the physical device, existingTxPDOs contains the saved
		//					  copy of the original transmit PDO & comms for this node.
		//  Post-conditions : The monitorTxPDOs has had the event & inhibit times modified
		//					  to reflect the new timebase and these have also been written
		//					  to the physical device.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		/// <summary>Updates the COBID on device to achieve a unique COBID with the newMonitorPriority.</summary>
		/// <param name="CANComms">CAN communications object (tx and rx on CANbus)</param>
		/// <param name="newMonitorPriority">new user selected priority required</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode changeCOBID( COBObject COBToModify, bool isTx, int rxIndex )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
			#endregion
			#region extract the correct CAN node data from the COB
			COBObject.PDOMapData txOrRxData;
			if(isTx == true)
			{
				txOrRxData = (COBObject.PDOMapData) COBToModify.transmitNodes[0];
			}
			else
			{
				txOrRxData = (COBObject.PDOMapData) COBToModify.receiveNodes[rxIndex];
			}
			int COBObjectIndex = 0;
			ODItemData COBIDSub = null;
			switch(COBToModify.messageType)
			{
				case COBIDType.EmergencyNoInhibit:
				case COBIDType.EmergencyWithInhibit:
					COBIDSub = emergencyodSub;
					break;

				case COBIDType.PDO:
					COBObjectIndex = txOrRxData.mapODIndex - SCCorpStyle.PDOToCOBIDObjectOffset;
					COBIDSub = getODSub(COBObjectIndex, SCCorpStyle.PDOCommsCOBIDSubIndex);
					break;

				case COBIDType.ProducerHeartBeat:
					COBObjectIndex = txOrRxData.mapODIndex; //just use this one for non-PDOs
					break;

				case COBIDType.SDOClient:
					break;

				case COBIDType.SDOServer:
					break;

				case COBIDType.Sync:
					COBIDSub = syncCobIDSub;
					break;

				case COBIDType.TimeStamp:
					COBIDSub = this.timeStampodSub;
					break;

				default:
					//user feedback here
					break;

			}
			#endregion extatract the correct CAN node data from the COB
			if(COBIDSub == null)
			{
				SystemInfo.errorSB.Append("\nUnable to change COBID");
				return DIFeedbackCode.DIGeneralFailure;
			}
			#region invalidate sub 1 which is COB-ID (cannot write to other subs if COB is enabled)
			if ( feedback == DIFeedbackCode.DISuccess )
			{
				feedback = writeODValue( COBIDSub, ((uint)COBToModify.COBID|(uint)SCCorpStyle.Bit31Mask));
			}
			#endregion

			#region write new COB ID
			if ( feedback == DIFeedbackCode.DISuccess )
			{
				feedback = writeODValue( COBIDSub, 	COBToModify.requestedCOBID );
			}
			#endregion
			if(COBToModify.createdForCalibratedGraphing == false)
			{
				this.EVASRequired = true;
				if(SystemInfo.CommsProfileItemChanged == false)
				{
					SystemInfo.errorSB.Append(SCCorpStyle.SaveCommsWarning);
					SystemInfo.CommsProfileItemChanged = true;
				}
			}
			return ( feedback );
		}

		public DIFeedbackCode changeTxType(COBObject COBToModify, bool isTx, int rxIndex)
		{
			#region local variable declarations and variable initialisation
			
			#endregion
			#region extract the correct CAN node data from the COB
			COBObject.PDOMapData txOrRxData;
			if(isTx == true)
			{
				txOrRxData = (COBObject.PDOMapData) COBToModify.transmitNodes[0];
			}
			else
			{
				txOrRxData = (COBObject.PDOMapData) COBToModify.receiveNodes[rxIndex];
			}
			int COBObjectIndex = txOrRxData.mapODIndex - SCCorpStyle.PDOToCOBIDObjectOffset;
			ODItemData COBIDSub = getODSub(COBObjectIndex, SCCorpStyle.PDOCommsCOBIDSubIndex);
			ODItemData txTypeSub = this.getODSub(COBObjectIndex, SCCorpStyle.PDOCommsTxTypeSubIndex);
			#endregion extract the correct CAN node data from the COB
			DIFeedbackCode feedback = DIFeedbackCode.DIInvalidIndexOrSub;
			if((COBIDSub != null) && (txTypeSub != null))
			{
				#region invalidate sub 1 which is COB-ID (cannot write to other subs if COB is enabled)
				feedback = writeODValue(COBIDSub, ((uint)COBToModify.requestedCOBID |(uint)SCCorpStyle.Bit31Mask) );
				#endregion

				#region write new Tx Type
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					feedback = writeODValue( txTypeSub, COBToModify.TxType );
				}
				#endregion

				#region re-validate sub 1 which is COB-ID (cannot write to other subs if COB is enabled)
				//always do this - we are putting system back to what it was
				feedback = writeODValue( COBIDSub, COBToModify.requestedCOBID );
				#endregion
				if(COBToModify.createdForCalibratedGraphing == false)
				{
					this.EVASRequired = true;
					if(SystemInfo.CommsProfileItemChanged == false)
					{
						SystemInfo.errorSB.Append(SCCorpStyle.SaveCommsWarning);
						SystemInfo.CommsProfileItemChanged = true;
					}
				}
			}
			return ( feedback );
		}

		public void ChangeInhibitTime(COBObject COBToModify)
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
			#endregion
			if(COBToModify.messageType == COBIDType.EmergencyWithInhibit)
			{
				#region write new Emergency Inhibit time
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					ODItemData odSub = this.getODSub(0x1015, 0);
					feedback = writeODValue(odSub, COBToModify.inhibitTime );  //CANopen fixed address
					this.EVASRequired = true;
					if(SystemInfo.CommsProfileItemChanged == false)
					{
						SystemInfo.errorSB.Append(SCCorpStyle.SaveCommsWarning);
						SystemInfo.CommsProfileItemChanged = true;
					}
				}
				#endregion write new Emergency Inhibit time
			}
			else if ( COBToModify.messageType == COBIDType.PDO)
			{
				#region extract the correct CAN node data from the COB
				COBObject.PDOMapData txData = (COBObject.PDOMapData) COBToModify.transmitNodes[0];
				int COBObjectIndex = txData.mapODIndex - SCCorpStyle.PDOToCOBIDObjectOffset;
				ODItemData COBIDSub = this.getODSub(COBObjectIndex, SCCorpStyle.PDOCommsCOBIDSubIndex);
				ODItemData inhibitSub = this.getODSub( COBObjectIndex, SCCorpStyle.PDOCommsInhibitTimeSubIndex);
				#endregion extract the correct CAN node data from the COB
				if((COBIDSub != null) && (inhibitSub != null))
				{
					#region invalidate sub 1 which is COB-ID (cannot write to other subs if COB is enabled)
					feedback = writeODValue(COBIDSub, ((uint)COBToModify.requestedCOBID |(uint)SCCorpStyle.Bit31Mask) );
					#endregion

					#region write new Inhibit time
					if ( feedback == DIFeedbackCode.DISuccess )
					{
						feedback = writeODValue( inhibitSub, COBToModify.inhibitTime );
					}
					#endregion

					#region re-validate sub 1 which is COB-ID (cannot write to other subs if COB is enabled)
					//always do this - we are putting system back to what it was
					feedback = writeODValue(COBIDSub, COBToModify.requestedCOBID );
					if(COBToModify.createdForCalibratedGraphing == false)
					{
						this.EVASRequired = true;
						if(SystemInfo.CommsProfileItemChanged == false)
						{
							SystemInfo.errorSB.Append(SCCorpStyle.SaveCommsWarning);
							SystemInfo.CommsProfileItemChanged = true;
						}
					}
				}
				else
				{
					feedback = DIFeedbackCode.DIInvalidIndexOrSub;
				}
				#endregion
			}

		}

		public void ChangeEventTime(COBObject COBToModify )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
			#endregion

			#region extract the correct tx CAN node data from the COB
			COBObject.PDOMapData txData = (COBObject.PDOMapData) COBToModify.transmitNodes[0];
			int COBObjectIndex = txData.mapODIndex - SCCorpStyle.PDOToCOBIDObjectOffset;
			#endregion extract the correct tx CAN node data from the COB

			if(COBToModify.messageType == COBIDType.ProducerHeartBeat)
			{
				this.EVASRequired = true;
				if(SystemInfo.CommsProfileItemChanged == false)
				{
					SystemInfo.errorSB.Append(SCCorpStyle.SaveCommsWarning);
					SystemInfo.CommsProfileItemChanged = true;
				}
			}
			else if(COBToModify.messageType == COBIDType.PDO)
			{
				ODItemData COBIDSub = this.getODSub(COBObjectIndex, SCCorpStyle.PDOCommsCOBIDSubIndex);
				ODItemData inhibitTimeSub =  this.getODSub(COBObjectIndex, SCCorpStyle.PDOCommsInhibitTimeSubIndex);
				if((COBIDSub != null) && (inhibitTimeSub != null))
				{
					#region invalidate sub 1 which is COB-ID (cannot write to other subs if COB is enabled)
					feedback = writeODValue(COBIDSub, ((uint) COBToModify.requestedCOBID |(uint)SCCorpStyle.Bit31Mask) );
					#endregion

					#region write new Inhibit time
					if ( feedback == DIFeedbackCode.DISuccess )
					{
						feedback = writeODValue( inhibitTimeSub, COBToModify.eventTime );
					}
					#endregion

					#region re-validate sub 1 which is COB-ID (cannot write to other subs if COB is enabled)
					//always do this - we are putting system back to what it was
					feedback = writeODValue( COBIDSub, COBToModify.requestedCOBID );

					if(COBToModify.createdForCalibratedGraphing == false)
					{
						this.EVASRequired = true;
						if(SystemInfo.CommsProfileItemChanged == false)
						{
							SystemInfo.errorSB.Append(SCCorpStyle.SaveCommsWarning);
							SystemInfo.CommsProfileItemChanged = true;
						}
					}
				}
				#endregion
			}
		}
		//-------------------------------------------------------------------------
		//  Name			: numMonitorPDOsThisNodeNeedsToTx()
		//  Description     : Looks at the list items needing to be PDO mapped and
		//					  determines the minimum and maximum number of transmit
		//					  PDOs are needed on this node to graph all objects on this
		//					  node.
		//  Parameters      : list - the list of items the user has selected to monitor
		//						(which lists it by index and sub index).  The structure
		//						contains other items which are found by the DI during
		//						the checks.
		//					  maxNoMapsRequired - maximum, ideal number of transmit PDOs
		//						needed to map every item in list for this node to a PDO
		//					  minNoMapsRequired - minimum can get away with number of transmit
		//						PDOs needed to map every item in list for this node that
		//						is not already PDO mapped in the device's original 
		//						configuration
		//  Used Variables  : dictionary - contains a replica OD of the device's,
		//						constructed according to the EDS
		//  Preconditions   : list contains a list of objects required to be PDO mapped
		//						to a transmit (for every node), all items have already
		//						been checked and confirmed to be PDO mappable
		//  Post-conditions : maxNoMapsRequired and minNoMapsRequired out parameters
		//					  have had their values calculated for return
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		/// <summary>Determines how many transmit PDOs are required for the monitoring list, ideally and minimally.</summary>
		/// <param name="list">he list of items the user has selected to monitor
		/// (which lists it by index and sub index).  The structure contains other items which 
		/// are found by the DI during the checks</param>
		/// <param name="maxNoMapsRequired">maximum, ideal number of transmit PDOs
		/// needed to map every item in list for this node to a PDO</param>
		/// <param name="minNoMapsRequired">minimum can get away with number of transmit
		/// PDOs needed to map every item in list for this node that is not already PDO 
		/// mapped in the device's original configuration </param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode numMonitorPDOsThisNodeNeedsToTx(out int numPDOsRequired)
		{
			#region local variable declarations/initialisation
			int numMapsAssignedInThisPDO = 0;
			int maxTotalDataSize = 0;
			numPDOsRequired = 1;
			#endregion

			#region comments
			/* For each item in the list, calculate how many bits it is and update the min 
			 * and max total data sizes needed to map (a) all list items (b) only those items
			 * not already PDO mapped.
			 */
			#endregion comments

            foreach (ODItemAndNode ODparamAndNode in sysInfo.CANcomms.VCI.OdItemsBeingMonitored)
			{
				if(ODparamAndNode.node == this)
				{
					//at this point pseudo subs have been reduced to only the first pseod sub in a bitsplit 
					//being in the VCI list
					#region datasize in bits of real item
					int dataSize = ODparamAndNode.ODparam.dataSizeInBits;
					if(ODparamAndNode.ODparam.bitSplit != null)
					{ //we have a bitsplit so so get the size of the underlyinf real sub
						dataSize = this.getOrigDataSizeInBits((CANopenDataType) ODparamAndNode.ODparam.dataType);
					}
					#endregion datasize in bits of real item
					if(((maxTotalDataSize + dataSize) > 64) 
						|| ((this.isSevconApplication() == true) && (numMapsAssignedInThisPDO>8)) )
					{ //this ODitem cannot be mapped into this PDO
						numPDOsRequired++;
						maxTotalDataSize = dataSize;  //so start the next PDO with the bits used in this one
					}
					else
					{
						maxTotalDataSize += dataSize;// update maximum, ideal number of bits required to be PDO mapped
						numMapsAssignedInThisPDO++;
					}
				}
			}
			if(numPDOsRequired > this.disabledTxPDOIndexes.Count)
			{
				SystemInfo.errorSB.Append("\nNode " + this.nodeID.ToString() + " does not have enough unused Transmit PDOs");
				return DIFeedbackCode.DIInsufficientFreeDynamicPDOs;
			}
			return DIFeedbackCode.DISuccess;
		}
		
		//-------------------------------------------------------------------------
		//  Name			: readCommsProfileAreaOfOD()
		//  Description     : Reads every item in the device's OD from the 
		//					  communication profile area.
		//  Parameters      : CANComms - CAN communications object (tx & rx on CANbus)
		//  Used Variables  : dictionary - contains a replica OD of the device's,
		//						constructed according to the EDS
		//  Preconditions   : The system has been found and this node's OD constructed
		//					  from the EDS and CANComms via the adapter has been started.
		//  Post-conditions : dictionary has every item in the OD communication profile
		//					  updated with the current value read from the physical device.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		/// <summary>Reads every item in the device's OD from the communication profile area.</summary>
		/// <param name="CANComms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public void readCommsProfileAreaOfOD( ArrayList COBsInSystem)
		{
			int SDOServerNum = 1, SDOClientNum = 1;
			ArrayList TxPDOCOBs = new ArrayList();
			this.maxTxPDOs = 0;  //reset ready to add them up here
			this.maxRxPDOs = 0;
			int numTxEnabledTPDOs = 0;
			int numRxEnabledRPDOs = 0;
			this.disabledTxPDOIndexes = new ArrayList();
			this.disabledRxPDOIndexes = new ArrayList();
			//then we stuff it into the TxPDOCOBs arraylist radey to creat ehte next one
			#region local variable declarations and variable initialisation
			DIFeedbackCode	feedback = DIFeedbackCode.DIGeneralFailure;
			_itemBeingRead = 0;
			COBObject currCOBObj = null;
			#endregion

			if(( this.masterStatus == true) && (this.isSevconApplication() == true))
			{ //clear this out bvefdore rereading the cooms Profile
				this.intPDOMaps = new VPDOObject();
			}
			foreach(ObjDictItem odItem in this.objectDictionary)
			{// For every object within the dictionary
				_itemBeingRead++; // _itemBeingRead used to update the progress bar
				ODItemData firstODSub = (ODItemData) odItem.odItemSubs[0];
				if (firstODSub.indexNumber == SCCorpStyle.SyncCOBIDIndex )
				{
					#region SYNC COB
					ODItemData syncSub = this.getODSub(SCCorpStyle.SyncCOBIDIndex, 0);
					if(syncSub != null)
					{
						feedback = this.readODValue(syncSub);
						if(feedback == DIFeedbackCode.DISuccess)
						{
							currCOBObj = new  COBObject();
							currCOBObj.messageType = COBIDType.Sync;
							currCOBObj.name = "Sync (NodeID " + this._nodeID.ToString() + ")";
							#region get the COBID for Sync messages
							// Does the device generate SYNC messages? Yes, extract transmit COBID & add to transmit list
							if ( ( firstODSub.currentValue & SCCorpStyle.Bit30Mask ) == 0 )
							{
								if ( ( firstODSub.currentValue & SCCorpStyle.Bit29Mask ) > 0 )// 29 bit ID
								{
									currCOBObj.COBID = (int)( firstODSub.currentValue & SCCorpStyle.Bits28To0Mask );
								}
								else // 11 bit ID
								{
									currCOBObj.COBID = (int)( firstODSub.currentValue & SCCorpStyle.Bits10To0Mask );
								}
								if ( currCOBObj.COBID > 0 )
								{
									COBObject.PDOMapData txData = new DriveWizard.COBObject.PDOMapData();
									txData.nodeID = this.nodeID;
									currCOBObj.transmitNodes.Add(txData);
									COBsInSystem.Add(currCOBObj);
									//the sync is enabled on this node - sso read the sync time
									if(this.syncTimeSub != null)
									{
										this.readODValue(syncTimeSub);  //ignore the feedback
									}
								}
							}
							#endregion get the COBID for Sync messages
						}
						else
						{
							#region append error feedback information
							if(feedback != DIFeedbackCode.DISuccess)
							{
								feedback = processfbc(syncSub, feedback, false);
                                if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
								{
									return;
								}
							}
							#endregion append error feedback information
						}
					}
					#endregion SYNC COB
				}
				else if ( firstODSub.indexNumber == SCCorpStyle.TimeStampCOBIDIndex ) 
				{
					#region TIMESTAMP COB
					ODItemData timeStampSub = getODSub(SCCorpStyle.TimeStampCOBIDIndex,0);
					if(timeStampSub != null)
					{
						feedback = this.readODValue(timeStampSub);
						if(feedback == DIFeedbackCode.DISuccess)
						{
							currCOBObj = new  COBObject();
							currCOBObj.messageType = COBIDType.TimeStamp;
							currCOBObj.name = "TimeStamp (NodeID " + this._nodeID.ToString() + ")";
							#region get the TimeStamp COBID
							// Does the device produce TIMESTAMP messages? Yes, extract COBID and add to transmit list
							if ( ( firstODSub.currentValue & SCCorpStyle.Bit30Mask ) > 0 )
							{
								if ( ( firstODSub.currentValue & SCCorpStyle.Bit29Mask ) > 0 )// 29 bit ID
								{
									currCOBObj.COBID = (int)( firstODSub.currentValue & SCCorpStyle.Bits28To0Mask );
								}
								else // 11 bit ID
								{
									currCOBObj.COBID = (int)( firstODSub.currentValue & SCCorpStyle.Bits10To0Mask );
								}
								if ( currCOBObj.COBID > 0 )
								{
									COBObject.PDOMapData txData = new DriveWizard.COBObject.PDOMapData();
									txData.nodeID = this.nodeID;
									currCOBObj.transmitNodes.Add(txData);
									COBsInSystem.Add(currCOBObj);
								}
							}
							#endregion get the TimeStamp COBID
						}
						else
						{
							#region append error feedback information
							if(feedback != DIFeedbackCode.DISuccess)
							{
								feedback = processfbc(timeStampSub, feedback, false );
                                if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
								{
									return;
								}
							}
							#endregion append error feedback information
						}
					}
					#endregion  TIMESTAMP COB
				}
				else if ( odItem.odItemSubs[0] == this.emergencyodSub ) 
				{
					#region Emergency COB
					if(this.emergencyodSub != null)
					{
						feedback = this.readODValue(this.emergencyodSub);
						if(feedback == DIFeedbackCode.DISuccess)
						{
							currCOBObj = new COBObject();
							#region set messageType and inhibit time dependent and whether Inhibit time is supported
							currCOBObj.messageType = COBIDType.EmergencyNoInhibit;
							MAIN_WINDOW.appendErrorInfo = false; //switch off error info - we are just checking - will be switched back on in method
							ODItemData odSub = this.getODSub(0x1015, 0);// we are just checking if emgency inhibit exists - it's optional
							MAIN_WINDOW.appendErrorInfo = true; //ensure it is back on
							if(odSub != null) //object exists
							{
								feedback = this.readODValue(odSub);  //0x1015 is emergency inhibit
								if(feedback == DIFeedbackCode.DISuccess)
								{
									currCOBObj.inhibitTime = (int)firstODSub.currentValue;
									currCOBObj.messageType = COBIDType.EmergencyWithInhibit;
								}
							}
							#endregion set message Tpye and inhibit time dependent and whether Inhibit time is supported
							currCOBObj.name = "Emergency (NodeID " + this._nodeID.ToString() + ")";
							#region get the Emergency Msg COBID
							// Does EMCY messages exist/valid? Yes, extract COBID and add to transmit list
							if ( ( firstODSub.currentValue & SCCorpStyle.Bit31Mask ) == 0 )
							{
								if ( ( firstODSub.currentValue & SCCorpStyle.Bit29Mask ) > 0 ) // 29 bit ID
								{
									currCOBObj.COBID = (int)( firstODSub.currentValue & SCCorpStyle.Bits28To0Mask );
								}
								else // 11 bit ID
								{
									currCOBObj.COBID = (int)( firstODSub.currentValue & SCCorpStyle.Bits10To0Mask );
								}
								if ( currCOBObj.COBID > 0 )
								{
									COBObject.PDOMapData txData = new DriveWizard.COBObject.PDOMapData();
									txData.nodeID = this.nodeID;
									currCOBObj.transmitNodes.Add(txData);
									COBsInSystem.Add(currCOBObj);
								}
							}
							#endregion get the Emergency Msg COBID
						}
						else
						{
							#region append error feedback information
							if(feedback != DIFeedbackCode.DISuccess)
							{
								feedback = processfbc(this.emergencyodSub, feedback, false);
                                if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
								{
									return;
								}
							}
							#endregion append error feedback information
						}
					}
					#endregion Emergency COB
				}
				else if  ( firstODSub.indexNumber == SCCorpStyle.ProducerHeartBeatTime )
				{
					#region Producer HeartBeat COB
					ODItemData prodHBeatSub = this.getODSub(SCCorpStyle.ProducerHeartBeatTime, 0);
					if(prodHBeatSub != null)
					{
						feedback = this.readODValue(prodHBeatSub);
						if(feedback == DIFeedbackCode.DISuccess)
						{
							currCOBObj = new COBObject();
							currCOBObj.messageType = COBIDType.ProducerHeartBeat;
							currCOBObj.eventTime = (int) prodHBeatSub.currentValue;
							currCOBObj.name = "Producer HeartBeat (NodeID " + this._nodeID.ToString() + ")";
							#region get heartbeat COBID
							if ( ( prodHBeatSub.currentValue & SCCorpStyle.Bit15To0Mask ) > 0 )
							{
								currCOBObj.COBID = (int)( prodHBeatSub.currentValue & 0x00ff0000 >> 16 );
								currCOBObj.COBID += 0x700;

								if ( currCOBObj.COBID > 0 )
								{
									COBObject.PDOMapData txData = new DriveWizard.COBObject.PDOMapData();
									txData.nodeID = this.nodeID;
									currCOBObj.transmitNodes.Add(txData);
									COBsInSystem.Add(currCOBObj);
								}
							}
							#endregion get heartbeat COBID
						}
						else
						{
							#region append error feedback information
							if(feedback != DIFeedbackCode.DISuccess)
							{
								feedback = processfbc(prodHBeatSub,  feedback, false);
                                if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
								{
									return;
								}
							}
							#endregion append error feedback information
						}
					}
					#endregion Producer HeartBeat COB
				}
				else if ((firstODSub.indexNumber >= SCCorpStyle.ServerSDOSetupMin ) 
					&& ( firstODSub.indexNumber <= SCCorpStyle.ServerSDOSetupMax ))
				{
					#region Server SDO COB
					foreach ( ODItemData sub in odItem.odItemSubs )
					{// if sub object which defines the server SDO transmit COBID
						if ( sub.subNumber ==  SCCorpStyle.ServerSDOTransmitCOBIDSubIndex )
						{
							feedback = this.readODValue(sub);
							if(feedback == DIFeedbackCode.DISuccess)
							{
								currCOBObj = new COBObject();
								currCOBObj.messageType = COBIDType.SDOServer;
								currCOBObj.name = "Server SDO " + SDOServerNum.ToString() + " (Node ID " + this._nodeID.ToString() + ")";
								SDOServerNum++;
								#region get the server SDO COBID
								// If SDO exists and is valid, extract COBID and add to transmit list
								if ( ( sub.currentValue & SCCorpStyle.Bit31Mask ) == 0 )
								{
									if ( ( sub.currentValue & SCCorpStyle.Bit29Mask ) > 0 )// 29 bit ID
									{
										currCOBObj.COBID = (int)( sub.currentValue & SCCorpStyle.Bits28To0Mask );
									}
									else // 11 bit ID
									{
										currCOBObj.COBID = (int)( sub.currentValue & SCCorpStyle.Bits10To0Mask );
									}
									if ( currCOBObj.COBID > 0 )
									{
										COBObject.PDOMapData txData = new DriveWizard.COBObject.PDOMapData();
										txData.nodeID = this.nodeID;
										currCOBObj.transmitNodes.Add(txData);
										COBsInSystem.Add(currCOBObj);
									}
								}
								#endregion get the server SDO COBID
							}
							else
							{
								#region append error feedback information
								if(feedback != DIFeedbackCode.DISuccess)
								{
									feedback = processfbc(sub, feedback, false);
                                    if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
									{
										return;
									}
								}
								#endregion append error feedback information
							}
						}
					}
					#endregion Server SDO COB
				}
				else if ( ( firstODSub.indexNumber >= SCCorpStyle.ClientSDOSetupMin ) 
					&& ( firstODSub.indexNumber <= SCCorpStyle.ClientSDOSetupMax ) )
				{
					#region client SDO transmit OD Area
					foreach ( ODItemData sub in odItem.odItemSubs )
					{// if sub object which defines the client SDO transmit COBID
						if ( sub.subNumber == SCCorpStyle.ClientSDOTransmitCOBIDSubIndex )
						{
							feedback = this.readODValue(sub);
							if(feedback == DIFeedbackCode.DISuccess)
							{// If SDO exists and is valid, extract COBID and add to transmit list
								currCOBObj = new COBObject();
								currCOBObj.messageType = COBIDType.SDOClient;
								currCOBObj.name = "Client SDO " + SDOClientNum.ToString() + " (Node ID " + this._nodeID.ToString() + ")";
								SDOClientNum++;
								#region get client SDO COBID
								if ( ( sub.currentValue & SCCorpStyle.Bit31Mask ) == 0 )
								{
									if ( ( sub.currentValue & SCCorpStyle.Bit29Mask ) > 0 )// 29 bit ID
									{
										currCOBObj.COBID = (int)( sub.currentValue & SCCorpStyle.Bits28To0Mask );
									}
									else // 11 bit ID
									{
										currCOBObj.COBID = (int)( sub.currentValue & SCCorpStyle.Bits10To0Mask );
									}
									if ( currCOBObj.COBID > 0 )
									{
										COBObject.PDOMapData txData = new DriveWizard.COBObject.PDOMapData();
										txData.nodeID = this.nodeID;
										currCOBObj.transmitNodes.Add(txData);
										COBsInSystem.Add(currCOBObj);
									}
								}
								#endregion get client SDO COBID
							}
							else
							{
								#region append error feedback information
								if(feedback != DIFeedbackCode.DISuccess)
								{
									feedback = processfbc(sub, feedback,  false);
                                    if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
									{
										return;
									}
								}
								#endregion append error feedback informat
							}
						}
					}
					#endregion client SDO transmit OD Area
				}
				else if ( ( firstODSub.indexNumber >= SCCorpStyle.PDOTxCommsSetupMin ) 
					&& (firstODSub.indexNumber <= SCCorpStyle.PDOTxCommsSetupMax ) )
				{
					#region Tx PDO COB definition area of OD
					currCOBObj = new  COBObject();
					currCOBObj.messageType = COBIDType.PDO;
					this.maxTxPDOs++; //this node could Tx this PDO if it wasvlaid and enabled
					foreach ( ODItemData sub in odItem.odItemSubs )
					{
						feedback = this.readODValue(sub);
						if(feedback == DIFeedbackCode.DISuccess)  //read all the items in the PDO area
						{
							#region get this Tx PDO COBID
							if ( sub.subNumber == SCCorpStyle.PDOCommsCOBIDSubIndex ) 
							{// sub object 1 defines the PDO transmit COBID
								if ( ( sub.currentValue & SCCorpStyle.Bit31Mask ) == 0 )
								{
									#region enabled Tx PDO
									if ( ( sub.currentValue & SCCorpStyle.Bit29Mask ) > 0 )// 29 bit ID
									{
										currCOBObj.COBID = (int)( sub.currentValue & SCCorpStyle.Bits28To0Mask );
									}
									else // 11 bit ID
									{
										currCOBObj.COBID = (int)( sub.currentValue & SCCorpStyle.Bits10To0Mask );
									}
									if ( currCOBObj.COBID > 0 )
									{
										if ( (currCOBObj.COBID & SCCorpStyle.Bit31Mask ) == 0 )  //only add enabled ones for now
										{//just add this to the Tx arrayList - we can handle the duplicate error later
											numTxEnabledTPDOs++;
											COBObject COBToBeReplaced = null;
											foreach(COBObject existCOB in COBsInSystem)
											{
												if(existCOB.COBID == currCOBObj.COBID)
												{
													//we need to merge this with the existing COB - may already have Rx data in it
													//so copy over any tx and Rx node data already found for this COB
													currCOBObj.transmitNodes = existCOB.transmitNodes;
													currCOBObj.receiveNodes = existCOB.receiveNodes;
													COBToBeReplaced = existCOB;  //mark the COB that we are going to replace with the currCOB
												}
											}
											if(COBToBeReplaced != null)
											{
												COBsInSystem.Remove(COBToBeReplaced);
											}
											COBsInSystem.Add(currCOBObj); //and then add the this (possibly merged) COB to the System Wide ArrayList of COBObjects
										}
									}
									#endregion enabled Tx PDO
								}
								else
								{
									#region disabled Tx PDO - which we can use
									//this is for ease of graphics - nothing should need more that 9 - if they do we will just have to do them manually
									this.disabledTxPDOIndexes.Add(firstODSub.indexNumber);
									#endregion disabled Tx PDO - which we can use
								}
							}
							else if ( sub.subNumber == SCCorpStyle.PDOCommsTxTypeSubIndex )
							{
								currCOBObj.TxType = (int)sub.currentValue;
							}
							else if ( sub.subNumber == SCCorpStyle.PDOCommsInhibitTimeSubIndex )
							{// save current inhibit time value
								currCOBObj.inhibitTime = (int)sub.currentValue;
							}
							else if ( sub.subNumber == SCCorpStyle.PDOCommsEventTimeSubIndex )
							{// save current event time value
								currCOBObj.eventTime = (int) sub.currentValue;
							}
							#endregion get this Tx PDO COBID
						}
						else
						{
							#region append error feedback information
							if(feedback != DIFeedbackCode.DISuccess)
							{
								feedback = processfbc(sub, feedback, false);
                                if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
								{
									return;
								}
							}
							#endregion append error feedback informat								
						}
					}
					#endregion Tx PDO COB definition area of OD
				}
				else if (  ( firstODSub.indexNumber >= SCCorpStyle.PDOTxMappingMin)
					&& ( firstODSub.indexNumber <= SCCorpStyle.PDOTxMappingMax ) )
				{
					#region Tx PDO Mapping area
					#region  first get hold of the corresponding COBID sub which will alwayts be at this odItem minus 0x200
					ODItemData CobIDSub = this.getODSub(firstODSub.indexNumber - SCCorpStyle.PDOToCOBIDObjectOffset, SCCorpStyle.PDOCommsCOBIDSubIndex);
					#endregion  first get hold of the corresponding COBID which will alwayts be at this odItem minus 0x200
					//we must have already read the COB area by now so we can grap the local value of corresponding COBID
					if(CobIDSub != null) //B&B
					{
						if ( (CobIDSub.currentValue & SCCorpStyle.Bit31Mask ) == 0 )  //only add enabled ones
						{//first determine if there is a Tx PDO COB enabled for this mapping
							foreach(COBObject COB in COBsInSystem)
							{
								if( (COB.messageType == COBIDType.PDO)	&& (COB.COBID == CobIDSub.currentValue))
								{  //this is mapping is associated with an enabled Tx PDO COB
									int numValidMaps = 0;
									#region create PDOMapData object
									COBObject.PDOMapData mapData = new COBObject.PDOMapData();
									mapData.nodeID = this.nodeID;
									mapData.mapODIndex = firstODSub.indexNumber;
									#endregion create PDOMapData object
									foreach ( ODItemData mappingsub in odItem.odItemSubs ) 
									{
										#region mapping subs
										if(mappingsub.subNumber>=0) //not a header row
										{
											feedback = this.readODValue(mappingsub);
											if(feedback == DIFeedbackCode.DISuccess)
											{
												#region get the number of valid PDO maps for this PDO and the map values
												if ( mappingsub.subNumber == SCCorpStyle.PDOCommsNoSubsSubIndex )
												{
													#region num valid Mappings Subindex
													numValidMaps = (int) mappingsub.currentValue;
													#region test for Dynamic PDO Mapping
													if((mappingsub.accessType == ObjectAccessType.ReadOnly)
														|| (mappingsub.accessType == ObjectAccessType.Constant))
													{
														this.fixedTxPDOIndexes.Add(firstODSub.indexNumber);
													}
													#endregion test for Dynamic PDO Mapping
													#endregion num valid Mappings Subindex
												}
												else if (mappingsub.subNumber <= numValidMaps) 
												{//remaining subs contain the mappings 
													#region determine and verify the OD object that has been  mapped into mappingsub
													int mappedODIndex = (int) (mappingsub.currentValue >>16);
													int mappedODsub = (int) (mappingsub.currentValue & 0xFFFF);
													mappedODsub = mappedODsub >> 8;
													//get the real single sub - or series of psuedo subs
													ObjDictItem OdObjAndSubs = this.getODItemAndSubs(mappedODIndex);
													#endregion verify the OD object that has been  mapped into mappingsub
													if(OdObjAndSubs != null)
													{ //mapped object exists in this OD so is valid
														int bitusedInBitSplit = 0;
														int bitUsedInOrigSub = 0;
                                                        ODItemData firstSub = (ODItemData)OdObjAndSubs.odItemSubs[0];
														foreach(ODItemData testMappedSub in OdObjAndSubs.odItemSubs)
														{
                                                            //If bit split, just show the original OD item without showing all the splits
                                                            if ((firstSub != null) && (firstSub.format == SevconNumberFormat.BIT_SPLIT))
                                                            {
                                                                ODItemData ODsub = this.getODSub(mappedODIndex, -1); //use the header
                                                                if (ODsub != null)  //exists in OD
                                                                {
                                                                    PDOMapping map = new PDOMapping(mappingsub.currentValue, ODsub.parameterName);
                                                                    mapData.SPDOMaps.Add(map);
                                                                }
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                if (testMappedSub.subNumber >= 0)//not the header
                                                                {
                                                                    // this doesn't work properly
                                                                    //if((testMappedSub.bitSplit != null) && (testMappedSub.bitSplit.realSubNo == mappedODsub))
                                                                    //{ 
                                                                    //    #region bitsplit mapping
                                                                    //    #region expand real mapping into its constituent pseud mappings and add all the pseudo mappings
                                                                    //    StringBuilder pseudoMapValString = new StringBuilder();
                                                                    //    pseudoMapValString.Append(testMappedSub.indexNumber.ToString("X").PadLeft(4, '0'));
                                                                    //    pseudoMapValString.Append(testMappedSub.subNumber.ToString("X").PadLeft(2, '0'));
                                                                    //    pseudoMapValString.Append(testMappedSub.dataSizeInBits.ToString("X").PadLeft(2, '0'));
                                                                    //    long pseudoMapVal = System.Convert.ToInt64(pseudoMapValString.ToString(), 16);
                                                                    //    PDOMapping pseudomap = new PDOMapping(pseudoMapVal, testMappedSub.parameterName);
                                                                    //    mapData.SPDOMaps.Add(pseudomap);
                                                                    //    bitusedInBitSplit += testMappedSub.dataSizeInBits; //need this to add our 'spacer' for non defined bits
                                                                    //    #endregion expand rela mapping into its constituent pseud mappings and add all the pseudo mappings
                                                                    //    bitUsedInOrigSub = this.getOrigDataSizeInBits((CANopenDataType) testMappedSub.dataType);
                                                                    //    #endregion bitsplit mapping
                                                                    //}
                                                                    //else 
                                                                    if (testMappedSub.subNumber == mappedODsub)
                                                                    {
                                                                        #region non-bitsplit mapping
                                                                        ODItemData ODsub = this.getODSub(mappedODIndex, mappedODsub);
                                                                        if (ODsub != null)  //exists in OD
                                                                        {
                                                                            PDOMapping map = new PDOMapping(mappingsub.currentValue, ODsub.parameterName);
                                                                            mapData.SPDOMaps.Add(map);
                                                                        }
                                                                        break;
                                                                        #endregion non-bitsplit mapping
                                                                    }
                                                                }
															}
														}
														if((bitUsedInOrigSub - bitusedInBitSplit) >0)
														{ //now add the bits not defined spacer as required
															#region add the not defined spacer mapping if required
															long mapVal = mappingsub.currentValue;  //first grab the Index and orgi sub for use when dereferencing this later
															mapVal = mapVal & 0xFFFFFF00;  //dith the orig num bits
															mapVal += (bitUsedInOrigSub - bitusedInBitSplit); //replac enum of bits with the ones we have left over
															PDOMapping map = new PDOMapping(mapVal, SCCorpStyle.nonDefinedBitsText);
															mapData.SPDOMaps.Add(map);
															#endregion add the not defined spacer mapping if required
														}
													}
													else
													{
														#region mapped item does not exist in OD
														StringBuilder tempSB = new StringBuilder();
														tempSB.Append("0x");
														tempSB.Append(mappedODIndex.ToString("X"));
														tempSB.Append(" sub 0x");
														tempSB.Append(mappedODsub.ToString("X"));
														tempSB.Append(" not in OD");
														PDOMapping map = new PDOMapping(mappingsub.currentValue, tempSB.ToString());
														mapData.SPDOMaps.Add(map);
														#endregion mapped item does not exist in OD
													}
												}
												if ( mappingsub.subNumber >= numValidMaps)
												{
													#region add the mappings to COB.transmitNodes
													COB.transmitNodes.Add(mapData);  //add the completed map to our new COB
													break;
													#endregion add the mappings to COB.transmitNodes
												}
												#endregion get the number of valid PDO maps for this PDO and the map values
											}
											else
											{
												#region append error feedback information
												if(feedback != DIFeedbackCode.DISuccess)
												{
													feedback = processfbc(mappingsub, feedback, false);
                                                    if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
													{
														return;
													}
												}
												#endregion append error feedback informat								
											}
										}
										#endregion mapping subs
									}
								}
							}
						}
					}
					#endregion Tx PDO mapping area
				}
				else if ( ( firstODSub.indexNumber >= SCCorpStyle.PDORxCommsSetupMin ) 
					&& ( firstODSub.indexNumber <= SCCorpStyle.PDORxCommsSetupMax ) )
				{
					#region Rx PDO COB definition area of OD
					currCOBObj = new  COBObject();
					currCOBObj.messageType = COBIDType.PDO;  // use Trsnmitt for all PDS - provblably schang eto single PDO indicator later
					this.maxRxPDOs++; //
					foreach ( ODItemData sub in odItem.odItemSubs )
					{
						feedback = this.readODValue(sub);
						if(feedback == DIFeedbackCode.DISuccess)  //read all the items in the PDO area
						{
							#region get this Rx PDO COBID
							if ( sub.subNumber == SCCorpStyle.PDOCommsCOBIDSubIndex ) 
							{// sub object 1 defines the PDO transmit COBID
								if ( ( sub.currentValue & SCCorpStyle.Bit31Mask ) == 0 )
								{
									#region enabled RxPDO
									if ( ( sub.currentValue & SCCorpStyle.Bit29Mask ) > 0 )// 29 bit ID
									{
										currCOBObj.COBID = (int)( sub.currentValue & SCCorpStyle.Bits28To0Mask );
									}
									else // 11 bit ID
									{
										currCOBObj.COBID = (int)( sub.currentValue & SCCorpStyle.Bits10To0Mask );
									}
									if ( currCOBObj.COBID > 0 )
									{
										if ( (currCOBObj.COBID & SCCorpStyle.Bit31Mask ) == 0 )  //only add enabled ones for now
										{
											numRxEnabledRPDOs++;
											#region add currCOBObj to COBsInSystem only if it isn't already represented
											bool thisCOBAlreadyAdded = false;
											foreach(COBObject existCOB in COBsInSystem)
											{
												if(existCOB.COBID == currCOBObj.COBID)
												{
													thisCOBAlreadyAdded = true;  //do nothing since htis is Rx and we have no event time etc - effectively just a refence to the COBID
													break;
												}
											}
											if(thisCOBAlreadyAdded == false)
											{  //all we do here is iensure that if the COB isn't in our list then add it
												currCOBObj.requestedCOBID = currCOBObj.COBID;  //set them the same for now
												COBsInSystem.Add(currCOBObj); //and then add the this (possibly merged) COB to the System Wide ArrayList of COBObjects
											}
											#endregion add currCOBObj to COBsInSystem only if it isn't already represented
										}
									}
									#endregion enabled RxPDO
								}
								else
								{
									#region disabled Rx PDo which we can use
									this.disabledRxPDOIndexes.Add(firstODSub.indexNumber);
									#endregion disabled Rx PDo which we can use
								}
							}
							#endregion get this Rx PDO COBID
						}
						else
						{
							#region append error feedback information

							feedback = processfbc(sub, feedback,  false);
                            if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
							{
								return;
							}
							#endregion append error feedback informat								
						}
					}

					#endregion Rx PDO COB definition area of OD
				}
				else if ( ( firstODSub.indexNumber >= SCCorpStyle.PDORxMappingMin ) 
					&& ( firstODSub.indexNumber <= SCCorpStyle.PDORxMappingMax ) )
				{
					#region Rx PDO Mapping area
					ODItemData cobIDSub = this.getODSub(firstODSub.indexNumber - SCCorpStyle.PDOToCOBIDObjectOffset, SCCorpStyle.PDOCommsCOBIDSubIndex);
					if(cobIDSub != null)
					{
						//first get hold of the corresponding COBID which will alwayts be at this odItem minus 0x200
						//						long correspCOBID;
						//we must have already read the COB area by now so we can grap the local value of corresponding COBID
						//					this.getODValue(odItem[0].indexNumber - SCCorpStyle.PDOToCOBIDObjectOffset, SCCorpStyle.PDOCommsCOBIDSubIndex, out correspCOBID);
						//frist determine if there is a Tx PDO COB enabled for this mapping
						foreach(COBObject COB in COBsInSystem)
						{
							if( (COB.messageType == COBIDType.PDO)	&& (COB.COBID == cobIDSub.currentValue))  //PDOTransmit is correct!
							{  //this is mapping is associated with an enabled Tx PDO COB
								int numValidMaps = 0;
								#region create PDOMapData object
								COBObject.PDOMapData mapData = new COBObject.PDOMapData();
								mapData.nodeID = this.nodeID;
								mapData.mapODIndex = firstODSub.indexNumber;
								#endregion create PDOMapData object
								foreach ( ODItemData mappingsub in odItem.odItemSubs )
								{
									#region mapping subs
									if(mappingsub.subNumber>=0) //not a header row
									{
										feedback = this.readODValue(mappingsub);
										if(feedback == DIFeedbackCode.DISuccess)
										{
											#region get the number of valid PDO maps for this PDO and the map values
											if ( mappingsub.subNumber == SCCorpStyle.PDOCommsNoSubsSubIndex )
											{
												#region num valid Mappings Subindex
												numValidMaps = (int) mappingsub.currentValue;
												#region test for Dynamic PDO Mapping
												if((mappingsub.accessType == ObjectAccessType.ReadOnly)
													|| (mappingsub.accessType == ObjectAccessType.Constant))
												{
													this.fixedRxPDOIndexes.Add(firstODSub.indexNumber);
												}
												#endregion test for Dynamic PDO Mapping
												#endregion num valid Mappings Subindex
											}
											else if(mappingsub.subNumber <= numValidMaps)  
											{//remaining subs contain the mappings 
												#region if this is a dummy spacer mapping
												if(mappingsub.currentValue == SCCorpStyle.spacer32bit)
												{
													#region add 32 bit spacer mapping
													PDOMapping map = new PDOMapping(mappingsub.currentValue, "32 bit spacer");
													mapData.SPDOMaps.Add(map);
													#endregion add 32 bit spacer mapping
												}
												else if(mappingsub.currentValue == SCCorpStyle.spacer16bit)
												{
													#region add 16 bit spacer mapping
													PDOMapping map = new PDOMapping(mappingsub.currentValue, "16 bit spacer");
													mapData.SPDOMaps.Add(map);
													#endregion add 16 bit spacer mapping
												}
												else if(mappingsub.currentValue == SCCorpStyle.spacer8bit)
												{
													#region add 8 bit spacer mapping
													PDOMapping map = new PDOMapping(mappingsub.currentValue, "8 bit spacer");
													mapData.SPDOMaps.Add(map);
													#endregion add 8 bit spacer mapping
												}
												else if(mappingsub.currentValue == SCCorpStyle.spacer1bit)
												{
													#region add 1 bit spacer mapping
													PDOMapping map = new PDOMapping(mappingsub.currentValue, "1 bit spacer");
													mapData.SPDOMaps.Add(map);
													#endregion add 1 bit spacer mapping
												}
													#endregion if this is a dummy spacer mapping
												else
												{
													#region determine and verify the OD object that has been  mapped into mappingsub
													int mappedODIndex = (int) (mappingsub.currentValue >>16);
													int mappedODsub = (int) (mappingsub.currentValue & 0xFFFF);
													mappedODsub = mappedODsub >> 8;
													//get the real single sub - or series of psuedo subs
													ObjDictItem OdItemAndSubs = this.getODItemAndSubs(mappedODIndex);
													#endregion determine and verify the OD object that has been  mapped into mappingsub
													if(OdItemAndSubs != null)
													{
														int bitusedInBitSplit = 0;
														int bitUsedInOrigSub = 0;
                                                        ODItemData firstSub = (ODItemData)OdItemAndSubs.odItemSubs[0];
														foreach(ODItemData testMappedSub in OdItemAndSubs.odItemSubs)
														{
                                                            //If bit split, just show the original OD item without showing all the splits
                                                            if ((firstSub != null) && (firstSub.format == SevconNumberFormat.BIT_SPLIT))
                                                            {
																ODItemData ODsub = this.getODSub(mappedODIndex, -1);
																if(ODsub != null)
																{
																	PDOMapping map = new PDOMapping(mappingsub.currentValue, ODsub.parameterName);
																	mapData.SPDOMaps.Add(map);
																}
                                                                break;
                                                            }
                                                            // this doesn't work properly
                                                            //if((testMappedSub.bitSplit != null) && (testMappedSub.bitSplit.realSubNo == mappedODsub))
                                                            //{	
                                                            //    #region bitsplit mapping
                                                            //    #region expand real mappings into its constituent pseudomaps and add them
                                                            //    StringBuilder pseudoMapValString = new StringBuilder();
                                                            //    pseudoMapValString.Append(testMappedSub.indexNumber.ToString("X").PadLeft(4, '0'));
                                                            //    pseudoMapValString.Append(testMappedSub.subNumber.ToString("X").PadLeft(2, '0'));
                                                            //    pseudoMapValString.Append(testMappedSub.dataSizeInBits.ToString("X").PadLeft(2, '0'));
                                                            //    long pseudoMapVal = System.Convert.ToInt64(pseudoMapValString.ToString(), 16);
                                                            //    PDOMapping pseudomap = new PDOMapping(pseudoMapVal, testMappedSub.parameterName);
                                                            //    mapData.SPDOMaps.Add(pseudomap);
                                                            //    #endregion expand real mappings into its constituent pseudomaps and add them
                                                            //    bitUsedInOrigSub = this.getOrigDataSizeInBits((CANopenDataType) testMappedSub.dataType);
                                                            //    #endregion bitsplit mapping
                                                            //}
                                                            //else 
                                                            if(testMappedSub.subNumber == mappedODsub)
															{
																#region non-bitsplit mapping
																ODItemData ODsub = this.getODSub(mappedODIndex,mappedODsub);
																if(ODsub != null)
																{
																	PDOMapping map = new PDOMapping(mappingsub.currentValue, ODsub.parameterName);
																	mapData.SPDOMaps.Add(map);
																}
																break;
																#endregion non-bitsplit mapping
															}
														}
														if((bitUsedInOrigSub - bitusedInBitSplit) >0)
														{ //now add the bits not defined spacer as required
															#region add the not defined spacer mapping if required
															long mapVal = mappingsub.currentValue;  //first grab the Index and orgi sub for use when dereferencing this later
															mapVal = mapVal & 0xFFFFFF00;  //dith the orig num bits
															mapVal += (bitUsedInOrigSub - bitusedInBitSplit); //replac enum of bits with the ones we have left over
															PDOMapping map = new PDOMapping(mapVal, SCCorpStyle.nonDefinedBitsText);
															mapData.SPDOMaps.Add(map);
															#endregion add the not defined spacer mapping if required
														}
													}
													else
													{ 
														#region mapped item does not exist in OD
														StringBuilder tempSB = new StringBuilder();
														tempSB.Append("0x");
														tempSB.Append(mappedODIndex.ToString("X"));
														tempSB.Append(" sub 0x");
														tempSB.Append(mappedODsub.ToString("X"));
														tempSB.Append(" not in OD");
														PDOMapping map = new PDOMapping(mappingsub.currentValue, tempSB.ToString());
														mapData.SPDOMaps.Add(map);
														break;
														#endregion mapped item does not exist in OD
													}
												}
											}
											if ( mappingsub.subNumber >= numValidMaps)
											{
												COB.receiveNodes.Add(mapData);  //add the completed map to our new COB
												break;
											}
											#endregion get the number of valid PDO maps for this PDO and the map values
										}
										else
										{
											#region append error feedback information
											if(feedback != DIFeedbackCode.DISuccess)
											{
												feedback = processfbc(mappingsub, feedback,  false);
                                                if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
												{
													return;
												}
											}
											#endregion append error feedback informat								
										}
									}
									#endregion mapping subs
								}
							}
						}
					}
					#endregion Rx PDO Mapping area
				}
				else if(( this.masterStatus == true) && (this.isSevconApplication() == true))
				{
					#region read the VPDO information if this is a Sevcon App bus master 
					if(firstODSub.objectName == (int)SevconObjectType.LOCAL_DIG_IN_MAPPING)
					{
						#region digital Inputs
						foreach ( ODItemData mappingSub in odItem.odItemSubs )
						{
							#region skip header
							if(mappingSub.subNumber == -1)
							{
								continue;
							}
							#endregion skip header
							feedback = this.readODValue(mappingSub);
							if(feedback == DIFeedbackCode.DISuccess)
							{
								if ( mappingSub.subNumber == 0)
								{
									#region get num enabled
									intPDOMaps.numEnabledDigIPMaps = (int) mappingSub.currentValue;
									#endregion get num enabled
								}
								else   
								{
									#region store maps
									PDOMapping map;
									if(mappingSub.currentValue == SCCorpStyle.dummyValue_DigIP) //dummy value for digital inputs
									{
										map = new PDOMapping( mappingSub.currentValue, "not mapped");
									}
									else
									{
										ODItemData mappedSub = this.getODSub((int) mappingSub.currentValue, 0);
										if(mappedSub != null)
										{
											map = new PDOMapping( mappingSub.currentValue, mappedSub.parameterName);
										}
										else
										{
											map = new PDOMapping( mappingSub.currentValue,  "OD item not found: 0x" + mappingSub.currentValue.ToString("X"));
										}
									}
									this.intPDOMaps.digIPMaps.Add(map);
									#endregion store maps
								}
							}
							else
							{
								#region append error feedback information
								if(feedback != DIFeedbackCode.DISuccess)
								{
									feedback = processfbc(mappingSub, feedback, false);
                                    if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
									{
										return;
									}
								}
								#endregion append error feedback informat								
							}
						}
						#endregion digital Inputs
					}
					else if (firstODSub.objectName == (int)SevconObjectType.LOCAL_DIG_OUT_MAPPING)
					{
						#region Digital Outputs
						foreach ( ODItemData mappingSub in odItem.odItemSubs )
						{
							#region skip header
							if(mappingSub.subNumber == -1)
							{
								continue;
							}
							#endregion skip header
							feedback = this.readODValue(mappingSub);
							if(feedback == DIFeedbackCode.DISuccess)
							{
								if ( mappingSub.subNumber == 0)
								{
									intPDOMaps.numEnabledDigOPMaps = (int) mappingSub.currentValue;
								}
								else   
								{
									#region store maps
									PDOMapping map;
									if(mappingSub.currentValue == SCCorpStyle.dummyValue_DigOP) //dummy value for digital outputs
									{
										map = new PDOMapping( mappingSub.currentValue, "not mapped");
									}
									else
									{
										ODItemData odMappedSub = this.getODSub((int) mappingSub.currentValue, 0);
										if(odMappedSub != null)
										{ //mapped item exists in OD
											map = new PDOMapping( mappingSub.currentValue,  odMappedSub.parameterName);
										}
										else
										{
											map = new PDOMapping( mappingSub.currentValue,  "OD item not found: 0x" + mappingSub.currentValue.ToString("X"));
										}
									}
									this.intPDOMaps.digOPMaps.Add(map);
									#endregion store maps
								}
							}
							else
							{
								#region append error feedback information
								if(feedback != DIFeedbackCode.DISuccess)
								{
									feedback = processfbc(mappingSub, feedback,  false);
                                    if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
									{
										return;
									}
								}
								#endregion append error feedback informat								
							}
						}
						#endregion Digital Outputs
					}
					else if (firstODSub.objectName == (int)SevconObjectType.LOCAL_ALG_IN_MAPPING)
					{
						#region Analogue inputs
						foreach ( ODItemData mappingSub  in odItem.odItemSubs )
						{
							#region skip header
							if(mappingSub.subNumber == -1)
							{
								continue;
							}
							#endregion skip header
							feedback = this.readODValue(mappingSub);
							if(feedback == DIFeedbackCode.DISuccess)
							{
								if ( mappingSub.subNumber == 0)
								{
									intPDOMaps.numEnabledAlgIPMaps = (int) mappingSub.currentValue;
								}
								else
								{
									#region get maps
									PDOMapping map;
									if(mappingSub.currentValue == SCCorpStyle.dummyValue_AlgIP) //dummy value for analogue inputs
									{
										map = new PDOMapping( mappingSub.currentValue, "not mapped");
									}
									else
									{
										ODItemData mappedSub = this.getODSub((int) mappingSub.currentValue, 0);
										if(mappedSub != null)
										{
											map = new PDOMapping( mappingSub.currentValue,mappedSub.parameterName);
										}
										else
										{
											map = new PDOMapping( mappedSub.currentValue,"OD item not found: 0x" + mappedSub.currentValue.ToString("X").PadLeft(8, '0'));
										}
									}
									this.intPDOMaps.algIPMaps.Add(map);
									#endregion get maps
								}
							}
							else
							{
								#region append error feedback information
								if(feedback != DIFeedbackCode.DISuccess)
								{
									processfbc(mappingSub, feedback, false);
                                    if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
									{
										return;
									}
								}
								#endregion append error feedback informat								
							}
						}
						#endregion Analogue inputs
					}
					else if (firstODSub.objectName == (int)SevconObjectType.LOCAL_ALG_OUT_MAPPING)
					{
						#region Analogue Outputs
						foreach ( ODItemData mappingSub in odItem.odItemSubs )
						{
							#region skip header
							if(mappingSub.subNumber == -1)
							{
								continue;
							}
							#endregion skip header
							feedback = this.readODValue(mappingSub);
							if(feedback == DIFeedbackCode.DISuccess)
							{
								if ( mappingSub.subNumber == 0)
								{
									intPDOMaps.numEnabledAlgOPMaps = (int) mappingSub.currentValue;
								}
								else 
								{
									#region get maps
									PDOMapping map;
									if(mappingSub.currentValue == SCCorpStyle.dummyValue_AlgOP) //dummy value for analogue outputs
									{
										map = new PDOMapping( mappingSub.currentValue, "not mapped");
									}
									else
									{
										ODItemData odMappedSub = this.getODSub((int) mappingSub.currentValue, 0);
										if(odMappedSub != null)
										{
											map = new PDOMapping( mappingSub.currentValue,  odMappedSub.parameterName);
										}
										else
										{
											map = new PDOMapping( mappingSub.currentValue, "OD item not found: 0x" + mappingSub.currentValue.ToString("X"));
										}
									}
									this.intPDOMaps.algOPMaps.Add(map);
									#endregion get maps
								}
							}
							else
							{
								#region append error feedback information
								if(feedback != DIFeedbackCode.DISuccess)
								{
									feedback = processfbc(mappingSub, feedback,  false);
                                    if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
									{
										return;
									}
								}
								#endregion append error feedback informat								
							}
						}
						#endregion Analogue outputs
					}
					else if (firstODSub.objectName == (int)SevconObjectType.LOCAL_MOTOR_MAPPING)
					{
						#region motor 
						foreach ( ODItemData mappingSub in odItem.odItemSubs )
						{
							#region skip header
							if(mappingSub.subNumber == -1)
							{
								continue;
							}
							#endregion skip header
							feedback = this.readODValue(mappingSub);
							if(feedback == DIFeedbackCode.DISuccess)
							{
								if ( mappingSub.subNumber == 0)
								{
									intPDOMaps.numEnabledMotorMaps = (int) mappingSub.currentValue;
								}
								else
								{
									#region get maps
									PDOMapping map;
									if(mappingSub.currentValue == SCCorpStyle.dummyValue_Motor) //dummy value for motor maps
									{
										map = new PDOMapping(mappingSub.currentValue, "not mapped");
									}
									else
									{
										//note for motr we HAVE to use -1 , DW is faced with multiple subs but controller sorts this - we are just arfter the parameter name here
										ODItemData ODMappedsub = this.getODSub((int) mappingSub.currentValue, -1); //always zero for VPDOs
										if(ODMappedsub != null)
										{
											map = new PDOMapping(mappingSub.currentValue,ODMappedsub.parameterName);
										}
										else
										{
											map = new PDOMapping(mappingSub.currentValue, "OD item not found: 0x" + mappingSub.currentValue.ToString("X"));
										}
									}
									this.intPDOMaps.MotorMaps.Add(map);
									#endregion get maps
								}
							}
							else
							{
								#region append error feedback information
								if(feedback != DIFeedbackCode.DISuccess)
								{
									feedback = processfbc(mappingSub, feedback,  false);
                                    if (this.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
									{
										return;
									}
								}
								#endregion append error feedback informat								
							}
						}
						#endregion motor
					}
					#endregion read the VPDO information if this is a Sevcon App bus master 
				}
				#region comments
				// Ignore all other error feedback codes (eg cannot read a write only object)
				// as we need to read as much of the OD as possible.  Only if there is no
				/* response do we check for 3 consecutive fails.  If this occurs, assume there
						* is a comms problem and quit the loop as this would take forever to complete
						* if every SDO request waited for the timeout period.
						*/
				#endregion comments
			}
			//once all the PDOSs relting to this CANnode have been found we need to apply
			// a clamp of 9 to the total of enabled and disabled PDOs
			if(numTxEnabledTPDOs>=9)
			{
				this.disabledTxPDOIndexes.Clear();  //we cannot relaistically clamp enabled PDOs - we have to dispaly thenm and accept that the GUI will look bad
			}
			else 
			{ 
				int clamp = 9 - numTxEnabledTPDOs;
				if((disabledTxPDOIndexes.Count-clamp) >0)
				{
					this.disabledTxPDOIndexes.RemoveRange(clamp, disabledTxPDOIndexes.Count-clamp);
				}
			}
			if(numRxEnabledRPDOs >= 9)
			{
				this.disabledRxPDOIndexes.Clear();
			}
			else
			{
				int clamp = 9 - numRxEnabledRPDOs;
				if((disabledRxPDOIndexes.Count-clamp)>0)
				{
					this.disabledRxPDOIndexes.RemoveRange(clamp, disabledRxPDOIndexes.Count-clamp);
				}
			}
		}
		
		public void addTxNodeToPDO( COBObject COBToModify )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DIInsufficientFreeDynamicPDOs;
			#endregion
			COBObject.PDOMapData txData  = (COBObject.PDOMapData)  COBToModify.transmitNodes[0];

			if(this.disabledTxPDOIndexes.Count>0)  //check is needed on tx side - for calibrated monitoring
			{//grab the first disabled Tx PDO slot for this node - use it and update the arraylist
				int COBODIndex = (int) this.disabledTxPDOIndexes[0]; //first availalbe COBPDOIndex;
				txData.mapODIndex = COBODIndex + SCCorpStyle.PDOToCOBIDObjectOffset;
				this.disabledTxPDOIndexes.RemoveAt(0); //update the list
				#region Write COB comms params to device

				#region write new communication object
				// check this item exists in this device's OD
				ODItemData cobIDSub = getODSub(COBODIndex, SCCorpStyle.PDOCommsCOBIDSubIndex);
				if ( cobIDSub != null)
				{	// invalidate sub 1 which is COB-ID (cannot write to other subs if COB is enabled)
					long newValue = (uint)COBToModify.COBID | (uint)SCCorpStyle.Bit31Mask; 
					feedback = writeODValue( cobIDSub, newValue );
				}
				// write the transmission type to the device

				if ( feedback == DIFeedbackCode.DISuccess )
				{
					ODItemData ODsub = getODSub(COBODIndex, SCCorpStyle.PDOCommsTxTypeSubIndex);
					if ( ODsub != null)
					{
						feedback = writeODValue( ODsub, (long)COBToModify.TxType );
					}
				}
				// write the inhibit time to the device
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					ODItemData ODsub = getODSub(COBODIndex, SCCorpStyle.PDOCommsInhibitTimeSubIndex);
					if ( ODsub != null)
					{
						feedback = writeODValue( ODsub, (long)COBToModify.inhibitTime );
					}
				}
				// write the event time to the device
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					ODItemData ODsub = getODSub(COBODIndex, SCCorpStyle.PDOCommsEventTimeSubIndex);
					if ( ODsub != null)
					{
						feedback = writeODValue(ODsub, (long)COBToModify.eventTime);
					}
				}
				// enable the COBID and write to the device
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					feedback = writeODValue( cobIDSub, (long)COBToModify.requestedCOBID );
				}
				#endregion
				if(COBToModify.createdForCalibratedGraphing == false)
				{
					this.EVASRequired = true;
					if(SystemInfo.CommsProfileItemChanged == false)
					{
						SystemInfo.errorSB.Append(SCCorpStyle.SaveCommsWarning);
						SystemInfo.CommsProfileItemChanged = true;
					}
				}
				#endregion Write COB comms params to device
			}
		}
		/// <summary>
		/// This method is called by GUI and adds an Rx leg to a PDO
		/// Only the commas parts are done here - the mapping is added after
		/// </summary>
		/// <param name="COBToModify"></param>
		/// <param name="CANComms"></param>
		/// <returns></returns>
		public void addRxNodeToPDO(COBObject COBToModify )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DIGeneralFailure;
			#endregion
			//to get here we must hav eadded at least one item to the receive nodes ArrayList
			COBObject.PDOMapData rxData = (COBObject.PDOMapData) COBToModify.receiveNodes[COBToModify.receiveNodes.Count-1];
			int COBIDIndex = (int) this.disabledRxPDOIndexes[0];
			rxData.mapODIndex = COBIDIndex + SCCorpStyle.PDOToCOBIDObjectOffset;
			this.disabledRxPDOIndexes.RemoveAt(0);  //update the unused list - Note: if write fails we done' want tro try this one again

			// Update the COB Object
			#region write new communication object
			ODItemData COBIDSub = this.getODSub( COBIDIndex, SCCorpStyle.PDOCommsCOBIDSubIndex);
			ODItemData txTypeSub = this.getODSub(COBIDIndex, SCCorpStyle.PDOCommsTxTypeSubIndex);
			if((COBIDSub != null) && (txTypeSub != null))
			{
				// invalidate sub 1 which is COB-ID (cannot write to other subs if COB is enabled)
				long COBDisableValue = (uint)COBToModify.requestedCOBID | (uint)SCCorpStyle.Bit31Mask; 
				feedback = writeODValue(COBIDSub, COBDisableValue);
				if ( feedback == DIFeedbackCode.DISuccess )
				{// write transmission type to device
					feedback = writeODValue(txTypeSub, (long)COBToModify.TxType);
				}
				// enable and write COBID to device
				if ( feedback == DIFeedbackCode.DISuccess )
				{
					feedback = writeODValue( COBIDSub, (long)COBToModify.requestedCOBID);
				}
				#endregion
				//if fcb is not suucessful in this method we should call a method that tries to disable the affected COB an dassoc mappings
				this.EVASRequired = true;
				if(SystemInfo.CommsProfileItemChanged == false)
				{
					SystemInfo.errorSB.Append(SCCorpStyle.SaveCommsWarning);
					SystemInfo.CommsProfileItemChanged = true;
				}
			}
		}

		public void updatePDOMappings(COBObject COBToModify, bool isTx, int rxDataIndex)
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
			long numberOfRealMapsWritten = 0;
			#endregion
			#region get correct CAN node data
			COBObject.PDOMapData txOrRxData;
			if(isTx == true)
			{
				txOrRxData = (COBObject.PDOMapData) COBToModify.transmitNodes[0];
			}
			else
			{
				txOrRxData = (COBObject.PDOMapData) COBToModify.receiveNodes[rxDataIndex];
			}
			#endregion get correct CAN node data
			#region Write PDO mapping & COB to device
			
			#region check the mapping object exists in this OD and invalidate the PDO mapping
			ObjDictItem odMappingItem = this.getODItemAndSubs(txOrRxData.mapODIndex);
			ODItemData odNumMapsSub = getODSub(txOrRxData.mapODIndex, SCCorpStyle.PDOMapNoSubsSubIndex);
			
			#endregion
			if(odNumMapsSub != null)
			{
				#region write number of PDO maps and the maps to the controller
				//start by writing zero mappings - to cover scearion where chnage is to remove all maps
				feedback = writeODValue( odNumMapsSub, (long)0 );
				if(feedback == DIFeedbackCode.DISuccess)
				{
					int offset = 0;
					foreach( ODItemData mappingSub in odMappingItem.odItemSubs)
					{
						#region write each mapping to CANnode
						ArrayList realSubsAlreadyWritten = new ArrayList();
						if(mappingSub.subNumber>0) //not header and not num of maps
						{
							if(offset>= txOrRxData.SPDOMaps.Count)
							{// could be zero map length - so check at start
								break;  //end of maps we want to write so get out
							}
							long temp = (long)((PDOMapping)txOrRxData.SPDOMaps[offset]).mapValue;
							if( (isTx == false) 
								&& (
								(temp == SCCorpStyle.spacer32bit)
								|| (temp == SCCorpStyle.spacer16bit)
								|| (temp == SCCorpStyle.spacer8bit)
								|| (temp == SCCorpStyle.spacer1bit)
								)
								)
							{ 
								#region RPDO spacer
								feedback = writeODValue( mappingSub,temp );
								numberOfRealMapsWritten++;
								if(feedback != DIFeedbackCode.DISuccess)
								{
									return;
								}
								#endregion RPDO spacer
							}
							else
							{
								#region normal OD item
								int mappedInd = (int) (temp >> 16);
								int mappedSub = (int) ((temp & 0xFFFF) >> 8);
								//verify that the the OD obejct exists - by checking for sub zero
								ObjDictItem mappedodItem = this.getODItemAndSubs(mappedInd);
								if(mappedodItem != null)
								{
									foreach(ODItemData possibleBitSplitSub in mappedodItem.odItemSubs)
									{//step trough each sub in the mapped object 
										//- to identify the original sub from pseudo sub if necessary
										if(possibleBitSplitSub.bitSplit != null) 
										{
											if((possibleBitSplitSub.bitSplit.realSubNo == mappedSub)
												&& (realSubsAlreadyWritten.Contains(mappedSub) == false))
											{
												#region pseudo mapping - using bitSplit subs
												//this mapping is a psedo mapping - we need to gather all the 
												//pseudo mappigns for this real sub and convert them
												//to asingle real mapping
												realSubsAlreadyWritten.Add(mappedSub);
												#region create the real mapping
												long newMapVal = (long)((PDOMapping)txOrRxData.SPDOMaps[offset]).mapValue;
												newMapVal = newMapVal & 0xFFFF0000; //strip out sub and datalength
												int newSub = (int) (mappedSub <<8);
												newMapVal += newSub;
												newMapVal += getOrigDataSizeInBits((CANopenDataType) possibleBitSplitSub.dataType);
												feedback = writeODValue( mappingSub,newMapVal );
												numberOfRealMapsWritten++;
												break;
												#endregion create the real mapping
												#endregion pseudo mapping - using bitSplit subs
											}
										}
										else if( possibleBitSplitSub.subNumber == mappedSub)
										{
											#region normal real mapping
											feedback = writeODValue( mappingSub,temp );
											numberOfRealMapsWritten++;
											break; //only one to write so leave
											#endregion normal real mapping
										}
									}
								}
								else
								{
									return;
								}
								#endregion normal OD item
							}
							offset++;
						}
						#endregion write each mapping to CANnode
					}
				}
				else
				{
					SystemInfo.errorSB.Append(" Unable to invalidate Tx PDO map prior to overwriting");
					return;
				}
				#region validate the map by writing the currect number of maps to sub 0

				feedback = writeODValue( odNumMapsSub, numberOfRealMapsWritten );													
				if ( feedback == DIFeedbackCode.CANDataCannotBeStoredToApplicationByLocalControl )
				{
					feedback = writeODValue( odNumMapsSub, (long)0 );
					if(feedback == DIFeedbackCode.DISuccess)
					{
						SystemInfo.errorSB.Append("CAN node rejected values, mapping successfully disabled");
					}
					else
					{
						SystemInfo.errorSB.Append("CAN node rejected values, failed to disable mappings written");
					}
					return;
				}
				#endregion  validate the map by writing the currect number of maps to sub 0
				#endregion  write number of PDO maps and the maps to the controller
			}
			if(COBToModify.createdForCalibratedGraphing == false)
			{
				#region mark for EEPORM save 
				this.EVASRequired = true;
				if(SystemInfo.CommsProfileItemChanged == false)
				{
					SystemInfo.errorSB.Append(SCCorpStyle.SaveCommsWarning);
					SystemInfo.CommsProfileItemChanged = true;
				}
				#endregion mark for EEPORM save 
			}
			#endregion if valid config data then write PDO mapping & COB to device
		}

		public DIFeedbackCode removeCANNodeFromPDO(COBObject COBToModify, bool isTx, int rxDataOrtxDataIndex)
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
			#endregion

			#region extract the correct CAN node data from the COB
			COBObject.PDOMapData txOrRxData;
			if(isTx == true)
			{
				txOrRxData = (COBObject.PDOMapData) COBToModify.transmitNodes[rxDataOrtxDataIndex];
			}
			else
			{
				txOrRxData = (COBObject.PDOMapData) COBToModify.receiveNodes[rxDataOrtxDataIndex];
			}
			int COBObjectIndex = txOrRxData.mapODIndex - SCCorpStyle.PDOToCOBIDObjectOffset;
			ODItemData COBIDSub = this.getODSub(COBObjectIndex, SCCorpStyle.PDOCommsCOBIDSubIndex);
			ODItemData numMapsSub = this.getODSub(txOrRxData.mapODIndex, SCCorpStyle.PDOMapNoSubsSubIndex);
			#endregion extract the correct CAN node data from the COB

			#region set COBID to disabled
			//instead of just disabling we are now using all bit sset basically invlaid
			// disabledValue will be set to 0x8FFFFFFF;
			feedback = writeODValue( COBIDSub, 0x8FFFFFFF );
            //JudeWood 27 May 08 DR38000247
            //Since we are removing either a Transmit or Received branch
            //we should add the COBID OD index to our list of disabled OD indexes to 
            //indicate that we have another PDO 'slot' available
            if (feedback == DIFeedbackCode.DISuccess)
            {
                if (isTx == true)
                {
                    this.disabledTxPDOIndexes.Add(COBIDSub.indexNumber);
                }
                else
                {
                    this.disabledRxPDOIndexes.Add(COBIDSub.indexNumber);
                }
            }
			#endregion set COBID to disabled

			#region write zero to num of Maps 
			feedback = writeODValue( numMapsSub, (long)0 );	//continue even if one failed
			#endregion write zero to num of Maps 
			if(COBToModify.createdForCalibratedGraphing == false)
			{
				this.EVASRequired = true;
				if(SystemInfo.CommsProfileItemChanged == false)
				{
					SystemInfo.errorSB.Append(SCCorpStyle.SaveCommsWarning);
					SystemInfo.CommsProfileItemChanged = true;
				}
			}
			return feedback;
		}

		#endregion

		#region bitsplit related methods
		public int getOrigDataSizeInBits(CANopenDataType datatype)
		{
			switch (datatype)
			{
				case CANopenDataType.UNSIGNED8:
				case CANopenDataType.INTEGER8:
					return 8;

				case CANopenDataType.UNSIGNED16:
				case CANopenDataType.INTEGER16:
					return 16;

				case CANopenDataType.UNSIGNED24:
				case CANopenDataType.INTEGER24:
					return 24;

				case CANopenDataType.UNSIGNED32:
				case CANopenDataType.INTEGER32:
					return 32;

				case CANopenDataType.UNSIGNED40:
				case CANopenDataType.INTEGER40:
					return 40;	

				case CANopenDataType.UNSIGNED48:
				case CANopenDataType.INTEGER48:
					return 48;

				case CANopenDataType.UNSIGNED56:
				case CANopenDataType.INTEGER56:
					return 56;

				case CANopenDataType.UNSIGNED64:
				case CANopenDataType.INTEGER64:
					return 64;

				default:
					return 0;
			}
		}
		internal DIFeedbackCode updateSubNumsForBitSplitSubItems(ObjDictItem odItemToSplit)
		{
			DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
			foreach(ODItemData sub in odItemToSplit.odItemSubs)
			{
				if(sub.subNumber == -2)
				{
					sub.subNumber = getFirstFreeSub(odItemToSplit, 1);//nextAvailSubNum++;
				}
			}
			return feedback;
		}
		private int getFirstFreeSub( ObjDictItem odItemToSplit, int reqSub)
		{
			int subAssigned = reqSub;
			foreach(ODItemData sub in odItemToSplit.odItemSubs)
			{
				if(sub.subNumber == reqSub)
				{
					subAssigned = getFirstFreeSub(odItemToSplit, reqSub + 1);
				}
			}
			return subAssigned;
		}

		#endregion bitsplit related methods

		#region User feedback Text generation
		/// <summary>
		/// This is for a single write/read so number of consecutive no responses is not relelvent
		/// </summary>
		/// <param name="IndexNo"></param>
		/// <param name="subNo"></param>
		/// <param name="errorStrB"></param>
		/// <param name="feedback"></param>
		private DIFeedbackCode processfbc(ODItemData odSub, DIFeedbackCode	feedback, bool isWrite )
		{
			if(MAIN_WINDOW.appendErrorInfo == false)
			{
				return DIFeedbackCode.DISuccess;
			}
			string abortMessage = "";
			DIFeedbackCode returnFBC = feedback;
			#region check that this is genuine error
			if ( feedback == DIFeedbackCode.CANGeneralError )
			{
				if(readSevconGeneralAbortCode( out abortMessage) == DIFeedbackCode.DISuccess)
				{
					return DIFeedbackCode.DISuccess;  //this is perfectly OK - just maens there was nothing to tranmit - don't flage any erro to user
				}
			}
			#endregion check that this is genuine error
			if(isWrite == true)
			{
				SystemInfo.errorSB.Append("\nFailed to write nodeID: ");
			}
			else
			{
				SystemInfo.errorSB.Append("\nFailed to read nodeID: ");
			}
			SystemInfo.errorSB.Append(this.nodeID.ToString());
			SystemInfo.errorSB.Append(" Param: ");
			SystemInfo.errorSB.Append(odSub.parameterName);
			SystemInfo.errorSB.Append(", OD Index:0x");
			SystemInfo.errorSB.Append(odSub.indexNumber.ToString("X"));
			SystemInfo.errorSB.Append(", sub:");
			SystemInfo.errorSB.Append(odSub.subNumber.ToString());
			#region add value
			if(isWrite == true)
			{
				CANopenDataType datatype = (CANopenDataType) odSub.dataType;
				switch(datatype)
				{
					case CANopenDataType.INTEGER16:
					case CANopenDataType.INTEGER24:
					case CANopenDataType.INTEGER32:
					case CANopenDataType.INTEGER40:
					case CANopenDataType.INTEGER48:
					case CANopenDataType.INTEGER56:
					case CANopenDataType.INTEGER64:
					case CANopenDataType.UNSIGNED16:
					case CANopenDataType.UNSIGNED24:
					case CANopenDataType.UNSIGNED32:
					case CANopenDataType.UNSIGNED40:
					case CANopenDataType.UNSIGNED48:
					case CANopenDataType.UNSIGNED56:
					case CANopenDataType.UNSIGNED64:
					case CANopenDataType.UNSIGNED8:
					case CANopenDataType.INTEGER8:
					case CANopenDataType.BOOLEAN:
					{
						SystemInfo.errorSB.Append(", value:");
						SystemInfo.errorSB.Append(odSub.currentValue.ToString());
						break;
					}
					default:
					{
						break;
					}
				}
			}
			#endregion add value
			SystemInfo.errorSB.Append("\nDW Error code: ");
			SystemInfo.errorSB.Append(feedback.ToString());
			if(abortMessage != "")
			{
				SystemInfo.errorSB.Append("\nCAN node reported: ");
				SystemInfo.errorSB.Append(abortMessage);
			}
			if ( feedback == DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice )
			{
				SystemInfo.errorSB.Append("\n3 consecutive response failures from node. Process aborted.\nCheck connections and power supply. \nYou may need to reconnect to system");
			}
			return returnFBC;
		}

		private void processfbc(ODItemData odSub, DIFeedbackCode feedback)
		{
			if(MAIN_WINDOW.appendErrorInfo == false)
			{
				return;
			}
			//this one is only called for internal - ie non device comms = no need to check for CNA general error here
			SystemInfo.errorSB.Append("\nFailed to read nodeID: ");
			SystemInfo.errorSB.Append(this.nodeID.ToString());
			SystemInfo.errorSB.Append(", Param: ");
			SystemInfo.errorSB.Append(odSub.parameterName);
			SystemInfo.errorSB.Append(", OD Index:0x");
			SystemInfo.errorSB.Append(odSub.indexNumber.ToString("X"));
			SystemInfo.errorSB.Append(", sub:");
			SystemInfo.errorSB.Append(odSub.subNumber.ToString());
			SystemInfo.errorSB.Append("\nDW Error code: ");
			SystemInfo.errorSB.Append(feedback.ToString());
		}

		#endregion User feedback Text generation

		#region adding and removing items from dictionary
		//-------------------------------------------------------------------------
		//  Name			: 
		//  Description     : 
		//					  
		//  Parameters      : None
		//  Used Variables  : 
		//					  
		//					
		//  Preconditions   : 
		//					  
		//					  
		//  Post-conditions : 
		//					  
		//					  
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		///<summary>Reads the EDS file to construct DW's replica OD while checking the EDS file integrity.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public void createMinimalDictionary()
		{
			ObjDictItem odItem;
			ODItemData odSub;
		
			//A minimal OD is created containing the following itmes 
			//0x1008, 0x1009, 0x100A, 0x1018
			this.objectDictionary = new ArrayList();
			#region object 0x1008
			odItem = new ObjDictItem();
			odSub = new ODItemData();
			odSub.dataType = 0x09;
			odSub.indexNumber = 0x1008;
			odSub.parameterName = "Product name";
			odSub.invalidItem = false;  //this object is now OK to read
			odSub.currentValueString = "Not available";
			odItem.odItemSubs.Add(odSub); //add sub to item
			this.objectDictionary.Add(odItem); //add item to dictionary
			#endregion object 0x1008 

			#region 0x1009
			odSub = new ODItemData();
			odSub.dataType = 0x09;
			odSub.displayType = (CANopenDataType)odSub.dataType; //since object type is 7
			odSub.indexNumber = 0x1009;
			odSub.parameterName = "Hardware version";
			odSub.currentValueString = "Not available";
			odItem.odItemSubs.Add(odSub); //add sub to item
			this.objectDictionary.Add(odItem); //add item to dictionary
			#endregion 0x1009

			#region 0x100A 
			odItem = new ObjDictItem();
			odSub = new ODItemData();
			odSub.dataType = 0x09;
			odSub.displayType = (CANopenDataType)odSub.dataType; //since object type is 7
			odSub.indexNumber = 0x100A;
			odSub.parameterName = "Software version";
			odSub.currentValueString = "Not available";
			odItem.odItemSubs.Add(odSub); //add sub to item
			this.objectDictionary.Add(odItem); //add item to dictionary
			#endregion 0x100A 
				
			#region 0x1018
			odItem = new ObjDictItem(); //create the odItem
			#region header 
			odSub = new ODItemData(); //create the sub
			odSub.dataType = 0x5;
			odSub.displayType = CANopenDataType.RECORD;
			odSub.indexNumber = 0x1018;
			odSub.subNumber = -1;
			odSub.parameterName = "Identity object";
			odSub.objectType = 9;
			odItem.odItemSubs.Add(odSub); //add sub to item
			#endregion header 

			#region number of entries
			odSub = new ODItemData(); //create the sub
			odSub.dataType = 0x05;
			odSub.indexNumber = 0x1018;
			odSub.displayType = (CANopenDataType)odSub.dataType; //since object type is 7
			odSub.parameterName = "Number of entries";
			odSub.accessType = ObjectAccessType.ReadOnly;
			odSub.highLimit = 0xFF;
			odSub.defaultValue = 4;
			odItem.odItemSubs.Add(odSub); //add sub to item
			#endregion number of entries

			#region Vendor ID
			odSub = new ODItemData(); //create the sub
			odSub.dataType = 0x07;
			odSub.displayType = (CANopenDataType)odSub.dataType; //since object type is 7
			odSub.indexNumber = 0x1018;
			odSub.subNumber = 1;
			odSub.parameterName = "Vendor ID";
			odSub.accessType = ObjectAccessType.ReadOnly;
			odSub.highLimit = 0xFFFFFFFF;
			odSub.currentValue=0xFFFFFFFF;
			odItem.odItemSubs.Add(odSub); //add sub to item
			#endregion Vendor ID

			#region Product code
			odSub = new ODItemData(); //create the sub
			odSub.dataType = 0x07;
			odSub.displayType = (CANopenDataType)odSub.dataType; //since object type is 7
			odSub.indexNumber = 0x1018;
			odSub.subNumber = 2;
			odSub.parameterName = "Product code";
			odSub.objectType = 7;
			odSub.accessType = ObjectAccessType.ReadOnly;
			odSub.highLimit = 0xFFFFFFFF;
			odSub.currentValue=0xFFFFFFFF;
			odItem.odItemSubs.Add(odSub); //add sub to item
			#endregion Product code

			#region revision Number
			odSub = new ODItemData(); //create the sub
			odSub.dataType = 0x07;
			odSub.displayType = (CANopenDataType)odSub.dataType; //since object type is 7
			odSub.indexNumber = 0x1018;
			odSub.subNumber = 3;
			odSub.parameterName = "Revision number";
			odSub.accessType = ObjectAccessType.ReadOnly;
			odSub.highLimit = 0xFFFFFFFF;
			odSub.currentValue=0xFFFFFFFF;
			odItem.odItemSubs.Add(odSub); //add sub to item
			#endregion revision Number

			#region serial Number
			odSub = new ODItemData(); //create the sub
			odSub.dataType = 0x07;
			odSub.displayType = (CANopenDataType)odSub.dataType; //since object type is 7
			odSub.indexNumber = 0x1018;
			odSub.subNumber = 4;
			odSub.parameterName = "Serial number";
			odSub.accessType = ObjectAccessType.ReadOnly;
			odSub.highLimit = 0xFFFFFFFF;
			odSub.currentValue=0xFFFFFFFF;
			odItem.odItemSubs.Add(odSub); //add sub to item
			#endregion Serial Number

			this.objectDictionary.Add(odItem); //add item to dictionary
			#endregion 0x1018
		}
		//add a single sub to the dictionary - inserting it in correct place
		internal void addSubToDictionary(ODItemData odSubToAdd)
		{
			ObjDictItem itemToModify;
			#region check that dictionary does not already contain this odSub
			MAIN_WINDOW.appendErrorInfo = false; //switch off error info - we are just checking 
			if(this.getODSub(odSubToAdd.indexNumber, odSubToAdd.subNumber, out itemToModify) != null)
			{
				MAIN_WINDOW.appendErrorInfo = true; //ensure it is back on
				if(odSubToAdd.subNumber != -1)
				{
					SystemInfo.errorSB.Append("\nAttempt to add duplicate sub to DCF dictionary");
					SystemInfo.errorSB.Append("\nParam: 0x");
					SystemInfo.errorSB.Append(odSubToAdd.parameterName);
					SystemInfo.errorSB.Append(", Index: 0x");
					SystemInfo.errorSB.Append(odSubToAdd.indexNumber.ToString("X"));
					SystemInfo.errorSB.Append(", Sub: 0x");
					SystemInfo.errorSB.Append(odSubToAdd.subNumber.ToString("X"));
				}
				return;
			}
			#endregion check that DCf does not already contain this odSub
			#region check whether to add new ODItemData or to extend an existing one
			MAIN_WINDOW.appendErrorInfo = true; //ensure it is back on
			#endregion check whether to add new ODItemData or to extend an existing one
			//now 
			if(itemToModify == null)
			{
				#region create new ODItemData and add this odSub
				ObjDictItem odItem = new ObjDictItem();
				odItem.odItemSubs.Add(odSubToAdd);
				this.objectDictionary.Add(odItem);
				#endregion create new ODItemData and add this odSub
			}
			else
			{
				//we need to insert in correct place esp for DCF PDO mappings - other wire it is displayed wrong in tabel and can be written wrong in 
				bool inserted = false;
				foreach (ODItemData existSub in itemToModify.odItemSubs)
				{
					if(existSub.subNumber>odSubToAdd.subNumber)
					{
						int insertIndex = Math.Max(0, itemToModify.odItemSubs.IndexOf(existSub)-1);
						itemToModify.odItemSubs.Insert(insertIndex,odSubToAdd);
						inserted = true;
						break; //leave
					}
				}
				if(inserted == false) 
				{ //append to end
					itemToModify.odItemSubs.Add(odSubToAdd);
				}
				#region finally update the numOfItems sub value if required
				MAIN_WINDOW.appendErrorInfo = false;
				ODItemData numItemsSub = this.getODSub(odSubToAdd.indexNumber, 0, out itemToModify);
				MAIN_WINDOW.appendErrorInfo = true;
				if((numItemsSub != null) && (numItemsSub.isNumItems == true))
				{
					numItemsSub.currentValue = itemToModify.odItemSubs.Count -1;
				}
				#endregion finally update the numOfItems sub value if required
			}
		}
		///adds entire odItem to dictionary
		///replacing any parts of this item that were there before
		internal void addODItemToDictionary(ObjDictItem odItemToAdd)
		{
            if (odItemToAdd == null) //needed to prevent exception
            {
                return;
            }

			foreach(ObjDictItem existOdItem in this.objectDictionary)
			{
				if(((ODItemData) existOdItem.odItemSubs[0]).indexNumber == ((ODItemData)odItemToAdd.odItemSubs[0]).indexNumber)
				{
					#region replace the part of this IDItem already in the dictionary
					int insertIndex = this.objectDictionary.IndexOf(existOdItem);
					this.objectDictionary.Remove(existOdItem); // have to do it this way cannot just equate to new one
					this.objectDictionary.Insert(insertIndex, odItemToAdd);
					return;
					#endregion replace the part of thie IDItem already in the dictionary
				}
			}
			//if we get to here the item does not exist in objectDictionary
			#region add odItemToAdd as new arrayList an dsort objectDictionary by index number
			this.objectDictionary.Add(odItemToAdd);
//			IComparer myComparer = new dictionaryDataSortClass("indexNumber");
//			this.objectDictionary.Sort(myComparer);
			#endregion add odItemToAdd as new arrayList an dsort data by index number
		}

		internal void removeSubFromDictionary(ODItemData subToRemove)
		{
			ObjDictItem affectedOdItem = this.getODItemAndSubs(subToRemove.indexNumber);
			if(affectedOdItem.odItemSubs.Count == 1)
			{ 
				#region remove whole odItem
				this.objectDictionary.Remove(affectedOdItem);
				#endregion remove whole odItem
			}
			else
			{
				#region remove just this sub
				affectedOdItem.odItemSubs.Remove(subToRemove);
				#endregion remove just this sub
			}
		}
		/// <summary>
		/// removes OdItem plus any associated subs form the dictionary
		/// </summary>
		internal void removeODItemFromDictionary(ObjDictItem itemToRemove)
		{
			if(this.objectDictionary.Contains(itemToRemove) == true)
			{
				this.objectDictionary.Remove(itemToRemove);
			}
//			IComparer myComparer = new dictionaryDataSortClass("indexNumber");
//			this.objectDictionary.Sort(myComparer);
		}
		#endregion adding and removing items from dictionary

		#region finding Sevcon object matches and getting the index for it
		internal ObjDictItem getODItemAndSubs(int index)
		{
			#region local variable declaration and variable initialisation
			#endregion
			foreach(ObjDictItem odItem in this.objectDictionary)
			{
				if( ((ODItemData) odItem.odItemSubs[0]).indexNumber == index)
				{
					return odItem;
				}
			}
			if(MAIN_WINDOW.appendErrorInfo == true)
			{ //we don't always wnat ot append the error - sometimes we are just veryfying that an object does not exsit 
				//eg motpr profiles, monitor store
				#region handle item not found
				SystemInfo.errorSB.Append("\nIndex 0x");
				SystemInfo.errorSB.Append(index.ToString("X"));
				SystemInfo.errorSB.Append(" does not exist in object dictionary. CAN node ID");
                SystemInfo.errorSB.Append(this.nodeID.ToString());      //DR38000260
				#endregion handle item not found
			}
			return null;
		}

		internal ArrayList getODItemAndSubsFromObjectType(SevconObjectType objectName)
		{
            int objectType = (int)objectName;
			ArrayList odItemsOfObjectType = new ArrayList();
			foreach ( ObjDictItem odItem in objectDictionary )
			{ //step through the dictionary
				ODItemData firstODSub = (ODItemData)odItem.odItemSubs[0]; 
				if ( firstODSub.objectName == objectType )
				{
					odItemsOfObjectType.Add(odItem);
				}
			}
			return odItemsOfObjectType;
		}
	
		internal ODItemData getODSub(int index, int subIndex, out ObjDictItem returnedOdItem)
		{
			#region comments
			/* Check each object and sub within the OD defined in data[][] to find
			 * the first match to the index and subIndex passed as parameters.
			 * If a match is found, set the out parameter with it's item & sub number
			 * and set the feedback to indicate success.  This allows the correct element
			 * within the data[][] to be found.
			 */
			#endregion comments
			returnedOdItem = null;
			foreach (ObjDictItem odItem in objectDictionary)
			{
				if(((ODItemData)odItem.odItemSubs[ 0 ]).indexNumber == index) 
				{ //correct index - so search the subs = judetemp will speed up processing of this frequently called method
					returnedOdItem = (ObjDictItem) odItem;
					foreach(ODItemData odSub in odItem.odItemSubs)
					{
						if ( odSub.subNumber == subIndex )
						{
							return odSub; 
						}
					}
					break;  //even if we didn't find the correct sub leave now - we found the index
				}
			}
			if(MAIN_WINDOW.appendErrorInfo == true)
			{//we don't always wnat ot append the error - sometimes we are just veryfying that an object does not exsit 
				//eg motpr profiles, monitor store
				#region handle item not found
				SystemInfo.errorSB.Append("\nIndex 0x");
				SystemInfo.errorSB.Append(index.ToString("X"));
				SystemInfo.errorSB.Append(" sub 0x");
				SystemInfo.errorSB.Append(subIndex.ToString("X"));
				SystemInfo.errorSB.Append(" does not exist in object dictionary. CAN node ID");
				SystemInfo.errorSB.Append(this.nodeID.ToString());
				#endregion handle item not found
			}
			return null;
		}

		internal ODItemData getODSub(int index, int subIndex)
		{
			#region comments
			/* Check each object and sub within the OD defined in objectDictionary[][] to find
			 * the first match to the index and subIndex passed as parameters.
			 * If a match is found, set the out parameter with it's item & sub number
			 * and set the feedback to indicate success.  This allows the correct element
			 * within the objectDictionary[][] to be found.
			 */
			#endregion comments
			foreach (ObjDictItem odItem in objectDictionary)
			{
				if(((ODItemData)odItem.odItemSubs[ 0 ]).indexNumber == index) 
				{ //correct index - so search the subs = judetemp will speed up processing of this frequently called method
					foreach(ODItemData odSub in odItem.odItemSubs)
					{
						if ( odSub.subNumber == subIndex )
						{
							return odSub; 
						}
					}
					break;  //even if we didn't find the correct sub leave now - we found the index
				}
			}
			if(MAIN_WINDOW.appendErrorInfo == true)
			{//we don't always wnat ot append the error - sometimes we are just veryfying that an object does not exsit 
				//eg motpr profiles, monitor store
				#region handle item not found
				SystemInfo.errorSB.Append("\nIndex 0x");
				SystemInfo.errorSB.Append(index.ToString("X"));
				SystemInfo.errorSB.Append(" sub 0x");
				SystemInfo.errorSB.Append(subIndex.ToString("X"));
				SystemInfo.errorSB.Append(" does not exist in object dictionary. CAN node ID");
				SystemInfo.errorSB.Append(this.nodeID.ToString());
				#endregion handle item not found
			}
			return null;
		}

		internal ODItemData getODSubFromObjectType( SevconObjectType objectName, int subIndex, out ObjDictItem returnedOdItem)
		{
            int objectType = (int)objectName;
			returnedOdItem = null;
			foreach (ObjDictItem odItem in objectDictionary )
			{ //step through the dictionary
				if ( ((ODItemData)odItem.odItemSubs[0]).objectName == objectType )
				{
					returnedOdItem = odItem;
					foreach(ODItemData odSub in odItem.odItemSubs)
					{
						if(odSub.subNumber == subIndex)
						{
							return odSub;
						}
					}
					break; //get out if correct sub wasn't found = we already loaated the correct odItem
				}
			}
			if(MAIN_WINDOW.appendErrorInfo == true)
			{//we don't always wnat ot append the error - sometimes we are just veryfying that an object does not exsit 
				//eg motpr profiles, monitor store
				#region handle item not found
				SystemInfo.errorSB.Append("\nObject type 0x");
				SystemInfo.errorSB.Append(objectType.ToString("X"));
                SystemInfo.errorSB.Append(" " + objectName.ToString()); //DR38000260
				SystemInfo.errorSB.Append(" sub 0x");
				SystemInfo.errorSB.Append(subIndex.ToString("X"));
				SystemInfo.errorSB.Append(" does not exist in object dictionary");
				#endregion handle item not found
			}
			return null; //if we get here the the requested Od sub does not exist
		}

        internal ODItemData getODSubFromObjectType(SevconObjectType objectName, int subIndex)
		{
            int objectType = (int)objectName;

			foreach ( ObjDictItem odItem in objectDictionary )
			{ //step through the dictionary
				ODItemData firstODSub = (ODItemData) odItem.odItemSubs[0];
				if ( firstODSub.objectName == objectType )
				{
					foreach(ODItemData odSub in odItem.odItemSubs)
					{
						if(odSub.subNumber == subIndex)
						{
							return odSub;
						}
					}
					break; //get out if correct sub wasn't found = we already loaated the correct odItem
				}
			}
			if(MAIN_WINDOW.appendErrorInfo == true)
			{//we don't always wnat ot append the error - sometimes we are just veryfying that an object does not exsit 
				//eg motpr profiles, monitor store
				#region handle item not found
				SystemInfo.errorSB.Append("\nObject type 0x");
				SystemInfo.errorSB.Append(objectType.ToString());
                SystemInfo.errorSB.Append(" " + objectName.ToString()); //DR38000260
				SystemInfo.errorSB.Append(" sub 0x");
				SystemInfo.errorSB.Append(subIndex.ToString("X"));
				SystemInfo.errorSB.Append(" does not exist in object dictionary");
				#endregion handle item not found
			}
			return null; //if we get here the the requested Od sub does not exist
		}

		#endregion

		#region set OD descriptions
		//-------------------------------------------------------------------------
		//  Name			: setObjectDescription()
		//  Description     : This function takes the information read in from the EDS 
		//					  which defines one object and uses this to populate one
		//					  object in the DW's replica of the controller's OD.
		//					  Some infomation is directly copied over from what was read
		//				      in the EDS (or DCF) file while other information is 
		//					  formatted to suit the DW.
		//					  NB This function does not poputlate sub objects.
		//  Parameters      : edsInfo - objectDictionary read in from the EDS or DCF file for this
		//							    object (including default values if not set in
		//							    the file)
		//					  thisEDS - calling EDS object, to allow to distinguish between
		//							    calls from an EDS object from those of a DCF object.
		//							    This is needed to set the current value for a DCF file.
		//  Used Variables  : objectDictionary[][] - array containing all object & sub definitions and
		//								 their current values
		//					  _noOfItemsInOD - current item being set in the objectDictionary[][] dictionary
		//					  sizeOfOD - number of objects in the objectDictionary[][] array (first 
		//								 dimension) to ensure only writing to allocated memory
		//  Preconditions   : If online, the controller has been found and a matching EDS file
		//					  is available.  The File info and the device info have been read
		//					  in OK and now the objectDictionary dictionary is being constructed for this
		//					  node.  If offline, a valid DCF file has been selected by the 
		//					  user which is now being used to construct objectDictionary[][].
		//  Post-conditions : The _noOfItemsInOD-th object in the objectDictionary[][] object dictionary
		//					  has been populated with the information read in from the EDS file
		//					  which is held in edsInfo.  _noOfItemsInOD is then incremented
		//					  ready to set the next object.
		//  Return value    : feedback - DISuccess if the object was set OK in the objectDictionary array
		//					  or a failure code indicating a reason for the failure.
		//----------------------------------------------------------------------------
		///<summary>Sets a single object description from the details read from the EDS in edsInfo.</summary>
		/// <param name="edsInfo">objectDictionary read in from the EDS or DCF file for this 
		/// object (including default values if not set in the file)</param>
		/// <param name="thisEDS">calling EDS object, to allow to distinguish between 
		/// calls from an EDS object from those of a DCF object. This is needed to set the current value for a DCF file.</param>
		/// <returns>feedback code indicates success or gives a failure reason</returns>
		public DIFeedbackCode setObjectDescription( EDSObjectInfo edsInfo, EDSorDCF thisEDS )
		{
			#region local variable declaration and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DIFailedToSetObjectDescription;
			int rangeIndex;
			#endregion

			/* If the object whose description is to be set is within the valid range i.e.
			 * within the memory allocated for the OD, then allow the EDS/DCF objectDictionary to be set.
			 */
			ObjDictItem odItem = new ObjDictItem();
			ODItemData odHeaderSub = new ODItemData();
			
			#region set sumNumber
			/* Objects use the subNumber to indicate how many subs are for this object.
				 * Create memory to store all objectDictionary for this object and it's subs.  Add one
				 * to the number of entries to contain the object header information.  This
				 * is not a real object in the controller's OD but is in the EDS and gives
				 * an overall descriptor etc. for this object & all it's subs which is
				 * useful information to the user.
				 * Compact sub entries are of a different format in the EDS and must be
				 * handled differently.
				 */
			numOfSubsBackup = edsInfo.subNumber; //DO FIRST
			odHeaderSub.subNumber = -1;  //assume header row to start with
			if ( ( edsInfo.format == SevconNumberFormat.BIT_SPLIT ) && ( edsInfo.split != null ) )
			{
				odHeaderSub.subNumber = edsInfo.split.Length + 1;
			}
			else if ( edsInfo.subNumber == 0 )
			{
				odHeaderSub.subNumber = 0;
				#region comments
				/* No sub-objects for this object so make this sub number 0.  Although
					 * objects without subs don't have a sub number in the EDS, when 
					 * communicating to the controller using an SDO it always requires a 
					 * sub number. So in this case it is still sub zero so set here so no
					 * special handling elsewhere is required.
					 */
					
				/*
						 * If an object has subs then there is no real objectDictionary associated with this item.
						 * Set sub number to (-1)an invalid value to ensure no erroneous sub matches
						 * are made when trying to write to a specific index/sub selected by the user.
						 */
				#endregion comments
			}
			#endregion set subNumber

			#region copy simple objectDictionary over
			/* No special handling associated with the following data so simply
				 * copy it over.
				 */
			odHeaderSub.CANnode = this;
			odHeaderSub.indexNumber = edsInfo.indexNumber;
			odHeaderSub.accessLevel = edsInfo.accessLevel;
			odHeaderSub.accessType = edsInfo.accessType;
			odHeaderSub.dataType = edsInfo.dataType;
			odHeaderSub.objectName = edsInfo.objectName;
            odHeaderSub.objectNameString = getObjectNameAsString(edsInfo.objectName);
			odHeaderSub.objectType = edsInfo.objectType;
			odHeaderSub.objFlags = edsInfo.objFlags;
			odHeaderSub.PDOmappable = edsInfo.PDOmappable;
			odHeaderSub.scaling = edsInfo.scaling;
			odHeaderSub.sectionType = edsInfo.sectionType;
            odHeaderSub.sectionTypeString = getSectionTypeAsString(edsInfo.sectionType);
			odHeaderSub.CANopenSectionType = CANSectionType.NONE;
			odHeaderSub.displayOnMasterOnly = edsInfo.displayOnMasterOnly;
			for(int i = 0;i<sysInfo.CANSectionODRange.Length;i++)
			{
				if ( ( odHeaderSub.indexNumber >= sysInfo.CANSectionODRange[i].minimum ) 
					&& ( odHeaderSub.indexNumber <= sysInfo.CANSectionODRange[i].maximum ))
				{
					odHeaderSub.CANopenSectionType = (CANSectionType)i;
				}
			}
			odHeaderSub.commsTimeout = edsInfo.commsTimeout;
			odHeaderSub.EDSObjectType = edsInfo.EDSObjectType;

			odHeaderSub.parameterName = edsInfo.parameterName;
			odHeaderSub.objectVersion = edsInfo.object_version;
			odHeaderSub.tooltip = edsInfo.toolTip;
			odHeaderSub.units = edsInfo.units;
			odHeaderSub.format = edsInfo.format;
			if((odHeaderSub.indexNumber ==	SCCorpStyle.SyncCOBIDIndex)
				|| (odHeaderSub.indexNumber == SCCorpStyle.TimeStampCOBIDIndex)
				|| (odHeaderSub.indexNumber == SCCorpStyle.EmcyCOBIDItem))
			{
				odHeaderSub.format = SevconNumberFormat.BASE16;  //COBIDs should be in hex
			}

			odHeaderSub.formatList = edsInfo.formatList;
			if( ((CANopenDataType) odHeaderSub.dataType == CANopenDataType.BOOLEAN)
				&& (odHeaderSub.formatList == ""))
			{
				odHeaderSub.formatList = "0=false:1=true";
			}
			odHeaderSub.eepromString = edsInfo.eepromString;
			odHeaderSub.defaultType = edsInfo.defaultType;

			// when first reading the OD from the EDS, assume all items in the EDS exist on the device
			odHeaderSub.invalidItem = false;
			#endregion

			#region set low and high limits from EDS or give default range (determined by data type)
			/* Set the low & high limits initially on min and max range that
				 * the actual data type can handle i.e. nothing to do with the EDS low & high level.
				 */
			if ( ( odHeaderSub.displayType & CANopenDataType.ARRAY ) != 0 )
			{
				rangeIndex = odHeaderSub.displayType.GetHashCode();
				rangeIndex -= CANopenDataType.ARRAY.GetHashCode();
			}
			else
			{
				rangeIndex = odHeaderSub.displayType.GetHashCode();
			}
			#endregion set low and high limits from EDS or give default range (determined by data type)
				
			#region calculate high, low and default vlaues 
			string valueSubStringMinusNodeID  = "";
			CANopenDataType dataType = (CANopenDataType) edsInfo.dataType;	
			switch (dataType)
			{
				case CANopenDataType.REAL32:
					#region REAL32
					odHeaderSub.real32 = new Real32Values();
					#region low limit
					//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
					//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
					odHeaderSub.real32.lowLimit = sysInfo.convertToFloat( edsInfo.lowLimit );
					if(this.sysInfo.conversionOK == false)
					{  //low limit is either missing or faulty - default to data type limit
						odHeaderSub.real32.lowLimit = float.MinValue;
					}
					#endregion low limit
					#region high limit
					//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
					//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
					odHeaderSub.real32.highLimit = sysInfo.convertToFloat( edsInfo.highLimit );
					if(this.sysInfo.conversionOK == false)
					{ //high limit is either missing or faulty - default to data type limit
						odHeaderSub.real32.highLimit = float.MaxValue; 
					}
					#endregion high limit
					//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
					//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
					odHeaderSub.real32.defaultValue = sysInfo.convertToFloat( edsInfo.defaultValue );
					#endregion REAL32
					break;
				case CANopenDataType.REAL64:
					#region REAL64
					odHeaderSub.real64 = new Real64Values();
					#region low limit
					odHeaderSub.real64.lowLimit = sysInfo.convertToDouble( edsInfo.lowLimit );
					if(this.sysInfo.conversionOK == false)
					{ 
						odHeaderSub.real64.lowLimit = double.MinValue;
					}
					#endregion low limit
					#region high limit
					odHeaderSub.real64.highLimit = sysInfo.convertToDouble( edsInfo.highLimit );
					if(this.sysInfo.conversionOK == false)
					{//
						odHeaderSub.real64.highLimit = double.MaxValue; 
					}
					#endregion high limit
					odHeaderSub.real64.defaultValue = sysInfo.convertToDouble( edsInfo.defaultValue );
					#endregion REAL64
					break;

					//Note DW was not handling signed values correctly
					//because we were conveting directly to long - thus in hex an dOctal we would not 
					//see the sign bit - this code may render parameterlimtsSet redundant - since if the 
					//conversion fails we will use the datatype min , max or zero as appropriate

				case CANopenDataType.INTEGER8:
				case CANopenDataType.INTEGER16:
				case CANopenDataType.INTEGER24:
				case CANopenDataType.INTEGER32:
				case CANopenDataType.INTEGER40:
				case CANopenDataType.INTEGER48:
				case CANopenDataType.INTEGER56:
				case CANopenDataType.INTEGER64:
					#region signed ints
					#region high limit
					valueSubStringMinusNodeID = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.highLimit);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odHeaderSub.highLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odHeaderSub.highLimit = sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						odHeaderSub.highLimit 
							= sysInfo.convertToSignedLong(odHeaderSub.displayType,odHeaderSub.highLimit);
						#endregion hex format
					}
					else if ((valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) && (valueSubStringMinusNodeID.Length>1))//leading 0 - ie is Octal 
					{
						#region octal format
						try
						{
							odHeaderSub.highLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odHeaderSub.highLimit = this.sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						#endregion octal format
					}
					else
					{
						#region base 10
						try
						{
							odHeaderSub.highLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odHeaderSub.highLimit = sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						#endregion base 10
					}
					//if our high limit value string did contain $NODEID then we need to add our nodeID in here
					if(valueSubStringMinusNodeID != edsInfo.highLimit) //there was no $NODEID in parameter vlaue
					{
						odHeaderSub.highLimit += odHeaderSub.CANnode.nodeID;
					}
					#endregion high limit
					#region low limit
					valueSubStringMinusNodeID = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.lowLimit);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odHeaderSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odHeaderSub.lowLimit = sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						odHeaderSub.lowLimit 
							= sysInfo.convertToSignedLong(odHeaderSub.displayType, odHeaderSub.lowLimit);
						#endregion hex format
					}
					else if(( valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) &&  (valueSubStringMinusNodeID.Length>1))//octal 
					{
						#region octal format
						try
						{
							odHeaderSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odHeaderSub.lowLimit = sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						#endregion octal format
					}
					else
					{
						#region base 10
						try
						{
							odHeaderSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odHeaderSub.lowLimit = sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						#endregion base 10
					}
					//if our low limit value string did contain $NODEID then we need to add our nodeID in here
					if(valueSubStringMinusNodeID != edsInfo.lowLimit) //there was no $NODEID in parameter vlaue
					{//JW 16Nov07 Correction to above line was highLimit
						odHeaderSub.lowLimit += odHeaderSub.CANnode.nodeID;
					}
					#endregion Low limit
					#region defualt value
					if(edsInfo.defaultValue == "")
					{
						break; //we default to zero which is fine
					}
					valueSubStringMinusNodeID = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.defaultValue);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odHeaderSub.defaultValue = System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odHeaderSub.defaultValue = 0; //same for all datatypes
						}
						odHeaderSub.defaultValue 
							= sysInfo.convertToSignedLong(odHeaderSub.displayType, odHeaderSub.defaultValue);
						#endregion hex format
					}
					else if ((valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) && (valueSubStringMinusNodeID.Length>1))// ie Octal
					{
						#region octal format
						try
						{
							odHeaderSub.defaultValue = System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odHeaderSub.defaultValue = 0; //same for all datatypes
						}
						#endregion octal format
					}
					else
					{
						#region base 10 
						try
						{
							odHeaderSub.defaultValue = System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odHeaderSub.defaultValue = 0; //same for all datatypes
						}
						#endregion base 10
					}
					//if our default value string did contain $NODEID then we need to add our nodeID in here
					if(valueSubStringMinusNodeID != edsInfo.defaultValue) //there was no $NODEID in parameter vlaue
					{
						odHeaderSub.defaultValue += odHeaderSub.CANnode.nodeID;
					}
					#endregion defualt value
					break;
					#endregion signed ints

				case CANopenDataType.UNSIGNED8:
				case CANopenDataType.UNSIGNED16:
				case CANopenDataType.UNSIGNED24:
				case CANopenDataType.UNSIGNED32:
				case CANopenDataType.UNSIGNED40:
				case CANopenDataType.UNSIGNED48:
				case CANopenDataType.UNSIGNED56:
				case CANopenDataType.UNSIGNED64:
					#region unsigned ints
					#region high limit
					valueSubStringMinusNodeID = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.highLimit);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odHeaderSub.highLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odHeaderSub.highLimit = this.sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						#endregion hex format
					}
					else if ((valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) && (valueSubStringMinusNodeID.Length>1))//leading 0 - ie is Octal 
					{
						#region octal format
						try
						{
							odHeaderSub.highLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odHeaderSub.highLimit = sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						#endregion octal format
					}
					else
					{
						#region base 10
						try
						{
							odHeaderSub.highLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odHeaderSub.highLimit = sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						#endregion base 10
					}
					//if our high value string did contain $NODEID then we need to add our nodeID in here
					if(valueSubStringMinusNodeID != edsInfo.highLimit) //there was no $NODEID in parameter vlaue
					{
						odHeaderSub.highLimit += odHeaderSub.CANnode.nodeID;
					}
					#endregion high limit
					#region low limit
					valueSubStringMinusNodeID = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.lowLimit);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odHeaderSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odHeaderSub.lowLimit = sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						#endregion hex format
					}
					else if(( valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) &&  (valueSubStringMinusNodeID.Length>1))//octal 
					{
						#region octal format
						try
						{
							odHeaderSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odHeaderSub.lowLimit = sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						#endregion octal format
					}
					else
					{
						#region base 10
						try
						{
							odHeaderSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odHeaderSub.lowLimit = this.sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						#endregion base 10
					}
					//if our low value string did contain $NODEID then we need to add our nodeID in here
					if(valueSubStringMinusNodeID != edsInfo.lowLimit) //there was no $NODEID in parameter vlaue
					{
						odHeaderSub.lowLimit += odHeaderSub.CANnode.nodeID;
					}

					#endregion Low limit
					#region defualt value
					if(edsInfo.defaultValue == "")
					{
						break; //avoid unnecessary try catches.
					}
					valueSubStringMinusNodeID = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.defaultValue);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odHeaderSub.defaultValue = System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odHeaderSub.defaultValue = 0; //same for all datatypes
						}
						#endregion hex format
					}
					else if ((valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) && (valueSubStringMinusNodeID.Length>1))// ie Octal
					{
						#region octal format
						try
						{
							odHeaderSub.defaultValue = System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odHeaderSub.defaultValue = 0; //same for all datatypes
						}
						#endregion octal format
					}
					else
					{
						#region base 10 
						try
						{
							odHeaderSub.defaultValue = System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odHeaderSub.defaultValue = 0; //same for all datatypes
						}
						#endregion base 10
						//if our default value string did contain $NODEID then we need to add our nodeID in here
						if(valueSubStringMinusNodeID != edsInfo.defaultValue) //there was no $NODEID in parameter vlaue
						{
							odHeaderSub.defaultValue += odHeaderSub.CANnode.nodeID;
						}
					}
					#endregion defualt value
					break;
					#endregion unsigned ints

				case CANopenDataType.BOOLEAN:
					#region boolean
					if( edsInfo.defaultValue.ToUpper() == "TRUE")
					{
						odHeaderSub.defaultValue = 1;
					}
					odHeaderSub.lowLimit = 0;
					odHeaderSub.highLimit = 1;
					break;
					#endregion boolean

				case CANopenDataType.VISIBLE_STRING:
					#region strings
					odHeaderSub.defaultValueString  = edsInfo.defaultValue;
					break;
					#endregion strings

				default:
					//Headers end up here
					//judetemp do nothing for now - but this could be an error condition and should be handled
					break;
			}
			#endregion calculate high, low and default vlaues 

			#region initialise current values
			/* Set the default current values for initial display prior to reading 
				 * from controller or reading in from the DCF. Should actually be read 
				 * before the user sees them on the screen.
				 */
			odHeaderSub.currentValueString = "";
			odHeaderSub.currentValue = 0;
			#endregion

			#region if compact sub objs used, setup OD data items from this format
			/* Compact sub objects allow the EDS to keep the file size small for
				 * arrays of the same data type by listing only the differences. Eg
				 * if the parameter names are different to default.
				 */
			if ( edsInfo.compactObj == true )
			{
				setCompactSubObjs( edsInfo, thisEDS );
			}
			#endregion

			#region if a DCF then read the currentValue from file and populate in OD
			/* If the parameterValue is non-null, this must have been called by a
				 * DCF file so read the parameter value from file.  It is necessary
				 * to convert the data according to the CANopen data type because 
				 * DW stores multiple CANopen data types in a single DW data type.
				 * Eg all integers (8,16,24,32,40,48,56 and 64 bit) are all stored in
				 * a long on DW.  A conversion is necessary so that sign bits are not
				 * misinterpreted or lost. 
				 * Also, domain items can be stored in a separate text file.  If this
				 * is the case (logs are stored this way) then this file must be read in
				 * using the known format to read in the currentValue.
				 * Also if arrays are stored in the compact format (more than one parameter
				 * value is listed) then this must also be handled here.
				 */
			if ( edsInfo.parameterValue.Length>0)
			{
				#region switch to read in the currentValue dependent on the display type
				this.sysInfo.conversionOK = true; //some data types don't call the converison method
				switch ( odHeaderSub.displayType  )
				{
						#region visible string
					case CANopenDataType.VISIBLE_STRING:
					{
						odHeaderSub.currentValueString = edsInfo.parameterValue[0];
						break;
					}
						#endregion

						#region boolean
					case CANopenDataType.BOOLEAN:
					{ //no need to look for $NODEID on booleans
						if ( sysInfo.convertToLong( edsInfo.parameterValue[0] ) > 0 )
						{
							odHeaderSub.currentValue = (long)1;
						}
						else
						{
							odHeaderSub.currentValue = (long)0;
						}
						break;
					}
						#endregion

						#region unsigned integer
					case CANopenDataType.UNSIGNED8:
					case CANopenDataType.UNSIGNED16:
					case CANopenDataType.UNSIGNED24:
					case CANopenDataType.UNSIGNED32:
					case CANopenDataType.UNSIGNED40:
					case CANopenDataType.UNSIGNED48:
					case CANopenDataType.UNSIGNED56:
					case CANopenDataType.UNSIGNED64:
					{

						valueSubStringMinusNodeID  = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.parameterValue[0]);
						if(valueSubStringMinusNodeID == edsInfo.parameterValue[0]) //there was no $NODEID in parameter vlaue
						{
							odHeaderSub.currentValue = sysInfo.convertToLong( edsInfo.parameterValue[0] );
						}
						else
						{  //we need to add in the node ID to our parameter value;
							odHeaderSub.currentValue = sysInfo.convertToLong( valueSubStringMinusNodeID ) + this.nodeID;
						}
						break;
					}
						#endregion

						#region signed integer
					case CANopenDataType.INTEGER8:
					case CANopenDataType.INTEGER16:
					case CANopenDataType.INTEGER24:
					case CANopenDataType.INTEGER32:
					case CANopenDataType.INTEGER40:
					case CANopenDataType.INTEGER48:
					case CANopenDataType.INTEGER56:
					case CANopenDataType.INTEGER64:
					{
						/* Ensure the sign bit is not misinterpreted when converting over
							 * to a long.
							 */
						long temp;
						valueSubStringMinusNodeID  = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.parameterValue[0]);
						if(valueSubStringMinusNodeID == edsInfo.parameterValue[0]) //there was no $NODEID in parameter vlaue
						{
							temp = sysInfo.convertToLong( edsInfo.parameterValue[0] );
						}
						else
						{  //we need to add in the node ID to our parameter value;
							temp = sysInfo.convertToLong( valueSubStringMinusNodeID ) + this.nodeID;
						}
						odHeaderSub.currentValue = sysInfo.convertToSignedLong( odHeaderSub.displayType, temp );
						break;
					}
						#endregion

						#region domain data type
					case CANopenDataType.DOMAIN:
					{
						/* If a filename is in the parameter value for a domain object and
							 * this file exists, then this file must be read to populate the
							 * currentValue byte array.  This currently only works for the Sevcon
							 * logs which are of a known format to read back.
							 * Additional infomation on the Sevcon object and Sevcon section type
							 * were saved with the log text file and so this is added to the
							 * data[][] object as it could be of use to the GUI.
							 */
						if ( File.Exists( edsInfo.parameterValue[ 0 ] ) )
						{
							feedback = this.EDS_DCF.readDomainFile( edsInfo,thisEDS, odHeaderSub.currentValueDomain);
							if ( feedback == DIFeedbackCode.DISuccess )
							{
								// Update these as these have been read in from domain file also.
								odHeaderSub.objectType = edsInfo.objectType;
								odHeaderSub.sectionType = edsInfo.sectionType;
                                odHeaderSub.sectionTypeString = getSectionTypeAsString(edsInfo.sectionType);
							}
						}
						break;
					}
						#endregion

						#region array
					case CANopenDataType.ARRAY:
					{
						/* Compact object storage for arrays must be read in if multiple
							 * sub-object parameter values are stored in the compact format.
							 */
						if ( ( edsInfo.compactObj == true ) && ( edsInfo.subNumber == 0 ) )
						{
							setCompactSubObjValues( edsInfo, thisEDS, odHeaderSub );
						}							
						break;
					}
						#endregion

						#region real data types
					case CANopenDataType.REAL32:
					{
						if  ( odHeaderSub.real32 != null )
						{
							//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
							//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
							odHeaderSub.real32.currentValue = sysInfo.convertToFloat( edsInfo.parameterValue[0] );
						}

						break;
					}
					case CANopenDataType.REAL64:
					{
						if (odHeaderSub.real64 != null )
						{
							odHeaderSub.real64.currentValue = sysInfo.convertToDouble( edsInfo.parameterValue[0] );
						}

						break;
					}
						#endregion

						#region currently unhandled CANopen data types by DW
					case CANopenDataType.IDENTITY:
					case CANopenDataType.OCTET_STRING:
					case CANopenDataType.PDO_COMMUNICATION_PARAMETER:
					case CANopenDataType.PDO_MAPPING:
					case CANopenDataType.RECORD:
					case CANopenDataType.SDO_PARAMETER:
					case CANopenDataType.TIME_DIFFERENCE:
					case CANopenDataType.TIME_OF_DAY:
					case CANopenDataType.UNICODE_STRING:
					case CANopenDataType.NULL:
					default:
					{
						/* These special data types are not currently handled by V1 of DW.
							 */
						odHeaderSub.currentValue = 0;
						break;
					}
						#endregion
				}
				#region user feedback
				if(this.sysInfo.conversionOK == false)
				{
					SystemInfo.errorSB.Append("\nFailed to read DCF value for Index:0x");
					SystemInfo.errorSB.Append(edsInfo.indexNumber.ToString("X"));
					SystemInfo.errorSB.Append("subIndex 0x");
					SystemInfo.errorSB.Append(edsInfo.subNumber.ToString("X").PadLeft(3, '0'));
					SystemInfo.errorSB.Append("Value: ");
					SystemInfo.errorSB.Append(edsInfo.parameterValue[0]);
				}
				#endregion user feedback
				#endregion
			}
			#endregion

			#region if this object is defined as a bit split then populate all the simulated subs here
			if ( edsInfo.format == SevconNumberFormat.BIT_SPLIT ) 
			{
				// make the 'real' object just a display type
				odHeaderSub.accessType = ObjectAccessType.DWDisplayOnly; 
				odHeaderSub.PDOmappable = edsInfo.PDOmappable;
				odHeaderSub.subNumber = -1;

				ODItemData numItemsSub = new ODItemData();
				numItemsSub.accessType = ObjectAccessType.ReadOnly;
				numItemsSub.subNumber = 0;
                numItemsSub.dataType = (int)CANopenDataType.UNSIGNED8;
				numItemsSub.isNumItems = true;
				numItemsSub.currentValue = edsInfo.split.Length;
				numItemsSub.parameterName = "Number of entries";
                numItemsSub.CANnode = odHeaderSub.CANnode; // required for monitoring (prevent exception)

				odItem.odItemSubs.Add(numItemsSub);

				// copy over all of the real object's definition into all simulated subs
				for ( int i = 1; i <= edsInfo.split.Length; i++ )
				{
					ODItemData BSodsub = new ODItemData();
                    BSodsub.CANnode = odHeaderSub.CANnode; // required for monitoring
					BSodsub.accessLevel =  odHeaderSub.accessLevel;
					BSodsub.CANopenSectionType = odHeaderSub.CANopenSectionType;
					BSodsub.dataType = odHeaderSub.dataType;
					BSodsub.commsTimeout = odHeaderSub.commsTimeout;
					BSodsub.sectionType = odHeaderSub.sectionType;
                    BSodsub.sectionTypeString = getSectionTypeAsString(odHeaderSub.sectionType);
					BSodsub.EDSObjectType = odHeaderSub.EDSObjectType;
					BSodsub.indexNumber = odHeaderSub.indexNumber;
					BSodsub.invalidItem =  odHeaderSub.invalidItem;
					BSodsub.scaling = odHeaderSub.scaling;
					BSodsub.objFlags = odHeaderSub.objFlags;
					BSodsub.objectName =  odHeaderSub.objectName;
                    BSodsub.objectNameString = getObjectNameAsString(odHeaderSub.objectName); 
					BSodsub.objectType =  odHeaderSub.objectType;
					//the default value is given for whole item in EDS not the indifvidual bits
					BSodsub.defaultValue  = odHeaderSub.defaultValue & edsInfo.split[ i - 1 ].bitMask;
					BSodsub.defaultValue >>= edsInfo.split[ i - 1 ].bitShift;
					BSodsub.accessType = edsInfo.accessType;
					BSodsub.subNumber = edsInfo.split[ i- 1 ].split + 1;
					BSodsub.parameterName = edsInfo.split[ i - 1 ].parameterName;
					BSodsub.format = edsInfo.split[ i - 1 ].format;
					BSodsub.formatList = edsInfo.split[ i - 1 ].formatList;
					BSodsub.units = edsInfo.split[ i - 1].units;
					BSodsub.bitSplit = new BitSplit();
					BSodsub.bitSplit.bitMask = edsInfo.split[ i - 1 ].bitMask;
					BSodsub.bitSplit.bitShift = edsInfo.split[ i - 1].bitShift;
					BSodsub.bitSplit.realSubNo = 0;  //needed for splitting sub indexes
					BSodsub.lowLimit = edsInfo.split[ i - 1 ].lowLimit;
					BSodsub.highLimit = edsInfo.split[ i - 1 ].highLimit;
					//the following are needed for DCFs
					BSodsub.bitSplit.lowLimit = edsInfo.split[ i - 1 ].lowLimit;
					BSodsub.bitSplit.highLimit = edsInfo.split[ i - 1 ].highLimit;
					BSodsub.PDOmappable = false;
					if(MAIN_WINDOW.isVirtualNodes == true)
					{//if we are talking virtual - the the current value is set to the default
						BSodsub.currentValue = BSodsub.defaultValue;
					}
					else
					{
						// work out the simulated sub equivalent currentValue if defined in DCF
						if ( edsInfo.parameterValue != null )
						{
							BSodsub.currentValue = odHeaderSub.currentValue; //Jude 081007
							//BSodsub.currentValue *= edsInfo.split[ i - 1 ].bitMask;
							BSodsub.currentValue &= edsInfo.split[ i - 1 ].bitMask;
							BSodsub.currentValue >>= edsInfo.split[ i - 1 ].bitShift;
						}
					}
					odItem.odItemSubs.Add(BSodsub);
				}
			}
			#endregion

			#region if this object is defied as dispaly for Master node only
			if(edsInfo.displayOnMasterOnly == true)
			{
				//judetmep - what do we do??
			}
			#endregion if this object is defied as dispaly for Master node only

			#region update feedback accordingly

			//Jude 260907 We now add Domains to OD - Domain subs we already added - so this was a bug
			#endregion

				odItem.odItemSubs.Insert(0,odHeaderSub);
				this.objectDictionary.Add(odItem);
			return ( DIFeedbackCode.DISuccess );
		}

		//-------------------------------------------------------------------------
		//  Name			: setSubDescription()
		//  Description     : This function takes the information read in from the EDS 
		//					  which defines one sub-object and uses this to populate one
		//					  sub-object in the DW's replica of the controller's OD.
		//					  Some infomation is directly copied over from what was read
		//				      in the EDS (or DCF) file while other information is 
		//					  formatted to suit the DW.
		//					  NB This function does not poputlate an object.
		//  Parameters      : edsInfo - data read in from the EDS or DCF file for this
		//							    sub-object (including default values if not set in
		//							    the file)
		//					  thisEDS - calling EDS object, to allow to distinguish between
		//							    calls from an EDS object from those of a DCF object.
		//							    This is needed to set the current value for a DCF file.
		//  Used Variables  : data[][] - array containing all object & sub definitions and
		//								 their current values
		//					  currentSub - current sub-item being set in the 
		//					  data[][] dictionary
		//  Preconditions   : If online, the controller has been found and a matching EDS file
		//					  is available.  The File info and the device info have been read
		//					  in OK and now the data dictionary is being constructed for this
		//					  node. The object has already been defined before this sub-object
		//					  is set by calling this function.
		//					  If offline, a valid DCF file has been selected by the user which
		//					  is now being used to construct data[][].
		//  Post-conditions : The currentSub-th sub-object in the data[][] OD
		//					  has been populated with the information read in from the EDS file
		//					  which is held in edsInfo.  currentSub is then incremented
		//					  ready to set the next sub-object.
		//  Return value    : feedback - DISuccess if the object was set OK in the data array
		//					  or a failure code indicating a reason for the failure.
		//----------------------------------------------------------------------------
		///<summary>Sets a single sub-object description from the details read from the EDS in edsInfo.</summary>
		/// <param name="edsInfo">data read in from the EDS or DCF file for this 
		/// object (including default values if not set in the file)</param>
		/// <param name="thisEDS">calling EDS object, to allow to distinguish between 
		/// calls from an EDS object from those of a DCF object. This is needed to set the current value for a DCF file.</param>
		/// <returns>feedback code indicates success or gives a failure reason</returns>
		public DIFeedbackCode setSubDescription( EDSObjectInfo edsInfo, EDSorDCF thisEDS )
		{
			#region local variable declaration and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DIFailedToSetSubObjectDescription;
			#endregion

			ObjDictItem odItem = (ObjDictItem) objectDictionary[objectDictionary.Count-1];
			if(odItem.odItemSubs.Count<1)
			{
				return DIFeedbackCode.DIGeneralFailure;
			}
			ODItemData odHeaderSub = (ODItemData) odItem.odItemSubs[0];
			ODItemData odSubToAdd = new ODItemData();
			/* If the sub-object to be set is within the number of subs expected for this
			 * object then allow it's data to be set from the edsInfo data structure.
			 */
			if ( edsInfo.format == SevconNumberFormat.BIT_SPLIT ) 
			{
				if(odItem.odItemSubs.Count>1)
				{//now update the 'number of subs' item value
					((ODItemData)odItem.odItemSubs[1]).currentValue += (edsInfo.split.Length - 1);//odItem.odItemSubs.Count - 1;
				}
				else
				{
					return DIFeedbackCode.DINumberOfItemsSubIsMissing;
				}
				for(int i = 0;i< edsInfo.split.Length;i++)
				{
					odSubToAdd = new ODItemData();
					feedback = fillSub(odSubToAdd, odHeaderSub,edsInfo, thisEDS, i);
					if(feedback == DIFeedbackCode.DISuccess)
					{
						odItem.odItemSubs.Add(odSubToAdd);
					}
					if(feedback != DIFeedbackCode.DISuccess)
					{
						return feedback;
					}

				}
			}
			else
			{
				feedback = fillSub(odSubToAdd, odHeaderSub,edsInfo, thisEDS, -1);
				if(feedback == DIFeedbackCode.DISuccess)
				{
					odItem.odItemSubs.Add(odSubToAdd);
					//sort on subNumber
				
				}
			}

			return ( feedback );
		}

		private DIFeedbackCode fillSub(ODItemData odSub, ODItemData odHeaderSub, EDSObjectInfo edsInfo, EDSorDCF thisEDS, int splitPtr)
		{
			#region local variable declaration and variable initialisation
			DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
			int rangeIndex;
			#endregion local variable declaration and variable initialisation

			odSub.CANnode = this;
			#region items directly copied from edsInfo
			odSub.indexNumber = edsInfo.indexNumber;
			odSub.subNumber = edsInfo.subNumber;
			odSub.accessLevel = edsInfo.accessLevel;
			odSub.accessType = edsInfo.accessType;
			odSub.dataType = edsInfo.dataType;
			odSub.objectType = edsInfo.objectType;
			odSub.objectVersion = edsInfo.object_version;
			odSub.objFlags = edsInfo.objFlags;
			odSub.scaling = edsInfo.scaling;
			odSub.commsTimeout = edsInfo.commsTimeout;
			odSub.EDSObjectType = edsInfo.EDSObjectType;
			odSub.PDOmappable = edsInfo.PDOmappable;
			odSub.displayOnMasterOnly = edsInfo.displayOnMasterOnly;
			odSub.eepromString = edsInfo.eepromString;
			#endregion items directly copied from edsInfo
			// when first reading the OD from the EDS, assume all items in the EDS exist on the device
			odSub.invalidItem = false;
			#region items copied from header object
			odSub.CANopenSectionType = odHeaderSub.CANopenSectionType;
			odSub.sectionType = odHeaderSub.sectionType;
            odSub.sectionTypeString = getSectionTypeAsString(odHeaderSub.sectionType);
			odSub.objectName = 	odHeaderSub.objectName;
            odSub.objectNameString = getObjectNameAsString(odHeaderSub.objectName);
			odSub.defaultType = edsInfo.defaultType;
			#endregion items copied from header object
			if ( edsInfo.format == SevconNumberFormat.BIT_SPLIT ) 
			{
				#region bit split specfic items
				//set the sub number for all splits to -2 to start with - when all 'real' sub number are in  we determine the correct sub numbers to apply
				odSub.subNumber = -2;
				odSub.parameterName = edsInfo.split[ splitPtr ].parameterName;
				odSub.format = edsInfo.split[splitPtr].format;
				odSub.formatList = edsInfo.split[ splitPtr].formatList;
				odSub.units = edsInfo.split[ splitPtr].units;
				odSub.bitSplit = new BitSplit();
				odSub.bitSplit.bitMask = edsInfo.split[splitPtr].bitMask;
				odSub.bitSplit.bitShift = edsInfo.split[ splitPtr].bitShift;
				//folowing is used when putting pseudo subs back together to write to a CAN node
				odSub.bitSplit.realSubNo = edsInfo.subNumber;
				odSub.bitSplit.realSubParamName = edsInfo.parameterName;
				odSub.bitSplit.lowLimit = edsInfo.split[ splitPtr].lowLimit;
				odSub.bitSplit.highLimit = edsInfo.split[ splitPtr ].highLimit;
				#endregion bit split specfic items
			}
			else
			{
				#region this sub is not bit split
				odSub.subNumber = edsInfo.subNumber;
					
				if(((odSub.accessType == ObjectAccessType.ReadOnly)
					|| (odSub.accessType == ObjectAccessType.Constant)) 
					&& (odSub.subNumber == 0)
					&& (((CANopenDataType) odSub.dataType) == CANopenDataType.UNSIGNED8)
					&& (odHeaderSub.subNumber == -1))
				{
					odSub.isNumItems = true;
					odSub.currentValue = this.numOfSubsBackup;
				}
				odSub.parameterName = edsInfo.parameterName;
				odSub.objectVersion = edsInfo.object_version;
				odSub.tooltip = edsInfo.toolTip;
				odSub.units = edsInfo.units;
				odSub.format = edsInfo.format;
				#region if this is a PDO mapping then force it to be in hex
				if( 
					((odSub.indexNumber>= SCCorpStyle.PDORxMappingMin)&& (odSub.indexNumber<= SCCorpStyle.PDORxMappingMax))
					|| ((odSub.indexNumber>= SCCorpStyle.PDOTxMappingMin) && (odSub.indexNumber<= SCCorpStyle.PDOTxMappingMax))
					)
				{
					if(odSub.subNumber != SCCorpStyle.PDOMapNoSubsSubIndex) //everything execept number of nmaps should be in hex
					{
						odSub.format = SevconNumberFormat.BASE16;  //judetamp - all params in this area should be shown in base16
					}
				}
				#endregion if this is a PDO mapping then force it to be in hex
				#region put PDO COBIDs in hex
				if( 
					((odSub.indexNumber>= SCCorpStyle.PDORxCommsSetupMin)&& (odSub.indexNumber<= SCCorpStyle.PDORxCommsSetupMax))
					|| ((odSub.indexNumber>= SCCorpStyle.PDOTxCommsSetupMin) && (odSub.indexNumber<= SCCorpStyle.PDOTxCommsSetupMax))
					)
				{
					if(odSub.subNumber == SCCorpStyle.PDOCommsCOBIDSubIndex) 
					{
						odSub.format = SevconNumberFormat.BASE16;  //COBIDs should be in hex
					}
				}
				#endregion put PDO COBIDs in hex
				#region put all other COBIDs in hex
				if((odSub.indexNumber >= SCCorpStyle.ServerSDOSetupMin) && (odSub.indexNumber <= SCCorpStyle.ServerSDOSetupMax))
				{
					if((odSub.subNumber == SCCorpStyle.ServerSDOReceiveCOBIDSubIndex) || (odSub.subNumber == SCCorpStyle.ServerSDOTransmitCOBIDSubIndex))
					{
						odSub.format = SevconNumberFormat.BASE16;  //COBIDs should be in hex
					}
				}
				if((odSub.indexNumber >= SCCorpStyle.ClientSDOSetupMin) && (odSub.indexNumber <= SCCorpStyle.ClientSDOSetupMax))
				{
					if((odSub.subNumber == SCCorpStyle.ClientSDOReceiveCOBIDSubIndex) || (odSub.subNumber == SCCorpStyle.ClientSDOTransmitCOBIDSubIndex))
					{
						odSub.format = SevconNumberFormat.BASE16;  //COBIDs should be in hex
					}
				}
				#endregion put all other COBIDs in hex




				odSub.formatList = edsInfo.formatList;
				if( ((CANopenDataType) odSub.dataType == CANopenDataType.BOOLEAN)
					&& (odSub.formatList == ""))
				{
					odSub.formatList = "0=false:1=true";
				}
				#endregion this sub is not bit split
			}
			#region set low and high limits from EDS or give default range (determined by data type)
			/* Set the low & high limits initially on min and max range that
						 * the actual data type can handle i.e. nothing to do with the EDS low & high level. */
			if ( ( odSub.displayType & CANopenDataType.ARRAY ) != 0 )
			{				
				rangeIndex = odSub.displayType.GetHashCode();
				rangeIndex -= CANopenDataType.ARRAY.GetHashCode();
			}
			else
			{
				rangeIndex = odSub.displayType.GetHashCode();
			}
			//Numerical values can contain the Node ID string $NODEID - this must be handled here
			string valueSubStringMinusNodeID  = "";
			switch (odSub.displayType)
			{
				case CANopenDataType.REAL32:
					#region REAL32
					odSub.real32 = new Real32Values();
					#region low limit
					//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
					//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
					odSub.real32.lowLimit = sysInfo.convertToFloat( edsInfo.lowLimit );
					if(this.sysInfo.conversionOK == false)
					{  //either was omitted or failed to convert
						odSub.real32.lowLimit = float.MinValue;
					}
					#endregion low limit
					#region high limit
					//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
					//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
					odSub.real32.highLimit = sysInfo.convertToFloat( edsInfo.highLimit );
					if(this.sysInfo.conversionOK == false)
					{
						odSub.real32.highLimit = float.MaxValue; 
					}
					#endregion high limit
					//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
					//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
					odSub.real32.defaultValue = sysInfo.convertToFloat( edsInfo.defaultValue );
					break;
					#endregion REAL32
				case CANopenDataType.REAL64:
					#region REAL64
					odSub.real64 = new Real64Values();
					#region low limt
					odSub.real64.lowLimit =  sysInfo.convertToDouble( edsInfo.lowLimit );
					if(this.sysInfo.conversionOK == false)
					{
						odSub.real64.lowLimit = double.MinValue;
					}
					#endregion low limit
					#region high limit
					odSub.real64.highLimit =  sysInfo.convertToDouble( edsInfo.highLimit );
					if(this.sysInfo.conversionOK == false)
					{
						odSub.real64.highLimit = double.MaxValue; 
					}
					#endregion high limit
					odSub.real64.defaultValue = sysInfo.convertToDouble( edsInfo.defaultValue );
					break;
					#endregion REAL64
				case CANopenDataType.UNSIGNED8:
				case CANopenDataType.UNSIGNED16:
				case CANopenDataType.UNSIGNED24:
				case CANopenDataType.UNSIGNED32:
				case CANopenDataType.UNSIGNED40:
				case CANopenDataType.UNSIGNED48:
				case CANopenDataType.UNSIGNED56:
				case CANopenDataType.UNSIGNED64:
					#region unsigned ints
					#region high limit
					valueSubStringMinusNodeID = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.highLimit);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odSub.highLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odSub.highLimit = this.sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						#endregion hex format
					}
					else if ((valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) && (valueSubStringMinusNodeID.Length>1))//leading 0 - ie is Octal 
					{
						#region octal format
						try
						{
							odSub.highLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odSub.highLimit = this.sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						#endregion octal format
					}
					else
					{
						#region base 10
						try
						{
							odSub.highLimit 
								= System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odSub.highLimit = sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						#endregion base 10
					}
					//if our high limit value string did contain $NODEID then we need to add our nodeID in here
					if(valueSubStringMinusNodeID != edsInfo.highLimit) //there was no $NODEID in parameter vlaue
					{
						odSub.highLimit += odSub.CANnode.nodeID;
					}
					#endregion high limit
					#region low limit
					valueSubStringMinusNodeID = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.lowLimit);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odSub.lowLimit = sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						#endregion hex format
					}
					else if(( valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) &&  (valueSubStringMinusNodeID.Length>1))//octal 
					{
						#region octal format
						try
						{
							odSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odSub.lowLimit = sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						#endregion octal format
					}
					else
					{
						#region base 10
						try
						{
							odSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odSub.lowLimit = sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						#endregion base 10
					}
					//if our lowlimt value string did contain $NODEID then we need to add our nodeID in here
					if(valueSubStringMinusNodeID != edsInfo.lowLimit) //there was no $NODEID in parameter vlaue
					{
						odSub.lowLimit += odSub.CANnode.nodeID;
					}

					#endregion Low limit
					#region defualt value
					if(edsInfo.defaultValue == "")
					{
						break; //avoid unnecessary try catch
					}
					valueSubStringMinusNodeID  = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.defaultValue);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odSub.defaultValue= System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odSub.defaultValue = 0; //same for all datatypes
						}
						#endregion hex format
					}
					else if ((valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) && (valueSubStringMinusNodeID.Length>1))// ie Octal
					{
						#region octal format
						try
						{
							odSub.defaultValue = System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odSub.defaultValue = 0; //same for all datatypes
						}
						#endregion octal format
					}
					else
					{
						#region base 10 
						try
						{
							odSub.defaultValue = System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odSub.defaultValue = 0; //same for all datatypes
						}
						#endregion base 10
					}
					//if our default value string did contain $NODEID then we need to add our nodeID in here
					if(valueSubStringMinusNodeID != edsInfo.defaultValue) //there was no $NODEID in parameter vlaue
					{
						odSub.defaultValue += odSub.CANnode.nodeID;
					}
					if ( edsInfo.format == SevconNumberFormat.BIT_SPLIT ) 
					{//do the conversion
						odSub.defaultValue  = odSub.defaultValue & edsInfo.split[splitPtr].bitMask;
						odSub.defaultValue >>= edsInfo.split[ splitPtr ].bitShift;
					}
					#endregion defualt value
					break;
					#endregion unsigned ints
				case CANopenDataType.INTEGER8:
				case CANopenDataType.INTEGER16:
				case CANopenDataType.INTEGER24:
				case CANopenDataType.INTEGER32:
				case CANopenDataType.INTEGER40:
				case CANopenDataType.INTEGER48:
				case CANopenDataType.INTEGER56:
				case CANopenDataType.INTEGER64:
					#region signed ints
					#region high limit
					valueSubStringMinusNodeID = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.highLimit);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odSub.highLimit	= System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odSub.highLimit = sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						odSub.highLimit = sysInfo.convertToSignedLong(odSub.displayType, odSub.highLimit);
						#endregion hex format
					}
					else if ((valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) && (edsInfo.highLimit.Length>1))//leading 0 - ie is Octal 
					{
						#region octal format
						try
						{
							odSub.highLimit 
								= System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odSub.highLimit = sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						#endregion octal format
					}
					else
					{
						#region base 10
						try
						{
							odSub.highLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odSub.highLimit = sysInfo.dataLimits[ rangeIndex ].highLimit;
						}
						#endregion base 10
					}
					//if our high limit value string did contain $NODEID then we need to add our nodeID in here
					if(valueSubStringMinusNodeID != edsInfo.highLimit) //there was no $NODEID in parameter vlaue
					{
						odSub.highLimit += odSub.CANnode.nodeID;
					}
					#endregion high limit
					#region low limit
					valueSubStringMinusNodeID = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.lowLimit);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odSub.lowLimit = sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						odSub.lowLimit 	= sysInfo.convertToSignedLong(odSub.displayType, odSub.lowLimit);
						#endregion hex format
					}
					else if(( valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) &&  (valueSubStringMinusNodeID.Length>1))//octal 
					{
						#region octal format
						try
						{
							odSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odSub.lowLimit = sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						#endregion octal format
					}
					else
					{
						#region base 10
						try
						{
							odSub.lowLimit = System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odSub.lowLimit = sysInfo.dataLimits[ rangeIndex ].lowLimit;
						}
						#endregion base 10
					}
					//if our low limit value string did contain $NODEID then we need to add our nodeID in here
					if(valueSubStringMinusNodeID != edsInfo.lowLimit) //there was no $NODEID in parameter vlaue
					{
						odSub.lowLimit += odSub.CANnode.nodeID;
					}
					#endregion Low limit
					#region defualt value
					if(edsInfo.defaultValue == "")
					{
						break; //avoid unecessary try catch - which are slow to run
					}
					valueSubStringMinusNodeID = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.defaultValue);
					if(valueSubStringMinusNodeID.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
					{
						#region hex format
						try
						{
							odSub.defaultValue = System.Convert.ToInt64(valueSubStringMinusNodeID, 16);
						}
						catch
						{
							odSub.defaultValue = 0; //same for all datatypes
						}
						odSub.defaultValue = sysInfo.convertToSignedLong(odSub.displayType, odSub.defaultValue);
						#endregion hex format
					}
					else if ((valueSubStringMinusNodeID.ToUpper().IndexOf("0") == 0) && (valueSubStringMinusNodeID.Length>1))// ie Octal
					{
						#region octal format
						try
						{
							odSub.defaultValue = System.Convert.ToInt64(valueSubStringMinusNodeID, 8);
						}
						catch
						{
							odSub.defaultValue = 0; //same for all datatypes
						}
						#endregion octal format
					}
					else
					{
						#region base 10 
						try
						{
							odSub.defaultValue = System.Convert.ToInt64(valueSubStringMinusNodeID, 10);
						}
						catch
						{
							odSub.defaultValue = 0; //same for all datatypes
						}
						#endregion base 10
					}
					//if our low limit value string did contain $NODEID then we need to add our nodeID in here
					if(valueSubStringMinusNodeID != edsInfo.defaultValue) //there was no $NODEID in parameter vlaue
					{
						odSub.defaultType += odSub.CANnode.nodeID;
					}

					#endregion defualt value
					break;
					#endregion signed ints
				case CANopenDataType.BOOLEAN:
					#region boolean

					if( edsInfo.defaultValue.ToUpper() == "TRUE")
					{
						odSub.defaultValue = 1;
					}
					else if ( edsInfo.defaultValue.ToUpper() == "FALSE")
					{
						//do nothing - will defula tot zero
					}
					else //try a numeric check 
					{
						//TODO tomorrow
						//just need default - high and low will be fixed at 0,1
						#region defualt value
						if(edsInfo.defaultValue == "")
						{
							break; //avoid unecessary try catch - which are slow to run
						}
						if(edsInfo.defaultValue.ToUpper().IndexOf("0X") == 0) //starts with 0x - ie is hex 
						{
							#region hex format
							try
							{
								odSub.defaultValue =  System.Convert.ToInt64(edsInfo.defaultValue, 16);
							}
							catch
							{
								odSub.defaultValue = 0; //same for all datatypes
							}
							odSub.defaultValue = sysInfo.convertToSignedLong(odSub.displayType, odSub.defaultValue);
							#endregion hex format
						}
						else
						{
							#region base 10 
							try
							{
								odSub.defaultValue = System.Convert.ToInt64(edsInfo.defaultValue, 10);
							}
							catch
							{
								odSub.defaultValue = 0; //same for all datatypes
							}
							#endregion base 10
						}
						#endregion defualt value
						//end TODO
					}
					odSub.lowLimit = 0;
					odSub.highLimit = 1;
					break;
					#endregion boolean
				case CANopenDataType.VISIBLE_STRING:
					#region strings
					odSub.defaultValueString  = edsInfo.defaultValue;
					break;
					#endregion strings
				default:
					break; //do nothing for now - consider error handling judetemp
			}
			#endregion

			#region else read the currentValue from DCF file and populate in OD
			
			if ( edsInfo.parameterValue.Length > 0) //DCF or virtual
			{ //we MUST use displayType here - we look at ARARAYS etc
				switch ( odSub.displayType )
				{
						#region visible string
					case CANopenDataType.VISIBLE_STRING:
					{
						odSub.currentValueString = 
							edsInfo.parameterValue[0];
						break;
					}
						#endregion

						#region boolean
					case CANopenDataType.BOOLEAN:
					{ //booleans shouldn't have $NodeID in paramstring
						if ( sysInfo.convertToLong( edsInfo.parameterValue[0]) > 0)
						{
							odSub.currentValue = (long)1;
						}
						else
						{
							odSub.currentValue = (long)0;
						}
						break;
					}
						#endregion

						#region unsigned integers
					case CANopenDataType.UNSIGNED8:
					case CANopenDataType.UNSIGNED16:
					case CANopenDataType.UNSIGNED24:
					case CANopenDataType.UNSIGNED32:
					case CANopenDataType.UNSIGNED40:
					case CANopenDataType.UNSIGNED48:
					case CANopenDataType.UNSIGNED56:
					case CANopenDataType.UNSIGNED64:
					{
						valueSubStringMinusNodeID  = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.parameterValue[0]);
						if(valueSubStringMinusNodeID == edsInfo.parameterValue[0]) //there was no $NODEID in parameter vlaue
						{
							odSub.currentValue = sysInfo.convertToLong( edsInfo.parameterValue[0] );
						}
						else
						{  //we need to add in the node ID to our parameter value;
							odSub.currentValue = sysInfo.convertToLong( valueSubStringMinusNodeID ) + this.nodeID;
						}
						break;
					}
						#endregion

						#region signed integers
					case CANopenDataType.INTEGER8:
					case CANopenDataType.INTEGER16:
					case CANopenDataType.INTEGER24:
					case CANopenDataType.INTEGER32:
					case CANopenDataType.INTEGER40:
					case CANopenDataType.INTEGER48:
					case CANopenDataType.INTEGER56:
					case CANopenDataType.INTEGER64:
					{
						/* Ensure the sign bit is not misinterpreted when converting over
									 * to a long.
									 */
						long temp;
						valueSubStringMinusNodeID  = sysInfo.checkIfEDSParamValueContainsNodeID(edsInfo.parameterValue[0]);
						if(valueSubStringMinusNodeID == edsInfo.parameterValue[0]) //there was no $NODEID in parameter vlaue
						{
							temp = sysInfo.convertToLong( edsInfo.parameterValue[0] );
						}
						else
						{  //we need to add in the node ID to our parameter value;
							temp = sysInfo.convertToLong( valueSubStringMinusNodeID ) + this.nodeID;
						}
						odSub.currentValue = sysInfo.convertToSignedLong( odSub.displayType, temp );
						break;
					}
						#endregion

						#region domain data type
					case CANopenDataType.DOMAIN:
					{
						/* If a filename is in the parameter value for a domain object and
									 * this file exists, then this file must be read to populate the
									 * currentValue byte array.  This currently only works for the Sevcon
									 * logs which are of a known format to read back.
									 * Additional infomation on the Sevcon object and Sevcon section type
									 * were saved with the log text file and so this is added to the
									 * data[][] object as it could be of use to the GUI.
									 */
						if ( File.Exists( edsInfo.parameterValue[ 0 ] ) )
						{
							feedback = this.EDS_DCF.readDomainFile( edsInfo, thisEDS, odSub.currentValueDomain);

							// Update these as modified according to that read from the domain file.
							if ( feedback == DIFeedbackCode.DISuccess )
							{
								odSub.objectName = edsInfo.objectName;
                                odSub.objectNameString = getObjectNameAsString(edsInfo.objectName);
								odSub.sectionType = edsInfo.sectionType;
                                odSub.sectionTypeString = getSectionTypeAsString(edsInfo.sectionType);
							}
						}
						break;
					}
						#endregion

						#region arrays
					case CANopenDataType.ARRAY:
					{
						/* Compact object storage for arrays must be read in if multiple
									 * sub-object parameter values are stored in the compact format.
									 */
						if ( ( edsInfo.compactObj == true ) && ( edsInfo.subNumber == 0 ) )
						{
							setCompactSubObjValues( edsInfo, thisEDS, odSub );
						}							
						break;
					}
						#endregion

						#region real data types
					case CANopenDataType.REAL32:
					{
						if ( odSub.real32 != null )
						{
							//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
							//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
							odSub.real32.currentValue =	sysInfo.convertToFloat( edsInfo.parameterValue[ 0 ] );
						}

						break;
					}
					case CANopenDataType.REAL64:
					{
						if (odSub.real64 != null )
						{
							odSub.real64.currentValue = sysInfo.convertToDouble( edsInfo.parameterValue[ 0 ] );
						}

						break;
					}
						#endregion

						#region currently unhandled CANopen data types by DW
					case CANopenDataType.IDENTITY:
					case CANopenDataType.OCTET_STRING:
					case CANopenDataType.PDO_COMMUNICATION_PARAMETER:
					case CANopenDataType.PDO_MAPPING:
					case CANopenDataType.RECORD:
					case CANopenDataType.SDO_PARAMETER:
					case CANopenDataType.TIME_DIFFERENCE:
					case CANopenDataType.TIME_OF_DAY:
					case CANopenDataType.UNICODE_STRING:
					case CANopenDataType.NULL:
					default:
					{
						/* These special data types are not currently handled by V1 of DW.
									 */
						odSub.currentValue = 0;
						break;
					}
						#endregion
				}
			}
			#endregion
			
			if ( edsInfo.format == SevconNumberFormat.BIT_SPLIT ) 
			{
				if(MAIN_WINDOW.isVirtualNodes == true)
				{//if we are talking virtual - the the current value is set to the default
					odSub.currentValue = odSub.defaultValue;
				}
			}
			if ( edsInfo.format == SevconNumberFormat.BIT_SPLIT ) 
			{
				odSub.currentValue  = odSub.currentValue & edsInfo.split[splitPtr].bitMask;
				odSub.currentValue >>= edsInfo.split[ splitPtr ].bitShift;
			}

			#region set feedback code appropriately
			/* Assume it was successful to define this sub-object in the OD unless it
						 * was a domain type which will return it's only feedback code.
						 */
			if ( odSub.displayType != CANopenDataType.DOMAIN )
			{
				feedback = DIFeedbackCode.DISuccess;
			}
			#endregion
			return feedback;
		}
	
		//-------------------------------------------------------------------------
		//  Name			: setCompactSubObjs()
		//  Description     : This function takes the compact sub object format EDS
		//					  definition and uses this to construct sub 0 and all
		//					  the other required sub objects for a compact array
		//					  object.  This populates the required number of objects
		//					  for this array in data[][] so that DW can treat these
		//					  in the same manner that all other objects read in from
		//					  the EDS or DCF are dealt with.
		//  Parameters      : edsInfo - representative sample of one of the objects
		//							    within the array (not sub 0)
		//					  thisEDS - EDS object needed to allow the setting of sub
		//								objects within the data[][] of this DW's OD.
		//  Used Variables  : None
		//  Preconditions   : The EDS or DCF file is being read and an object
		//					  is an array which is defined in the compact format.
		//  Post-conditions : sub 0 has been defined and set in data[][] and the 
		//					  number of subs defined in the compact sub object have
		//					  also been defined in data[][].
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>Takes the compact sub object EDS format and constructs the required number of subs in the data[][] array.</summary>
		/// <param name="edsInfo">the eds infomation which contains the domain filename as the parameter 
		/// value for a domain object </param>
		/// <param name="thisEDS">EDSorDCF object reference so that the EDS conversion routines for 
		/// Sevcon object and section type can be used</param>
		private void setCompactSubObjs( EDSObjectInfo edsInfo, EDSorDCF thisEDS )
		{
			#region local variable declaration and variable initialisation
			EDSObjectInfo sub0 = new EDSObjectInfo();
			string [] split = new string[0];
			string originalParameterName = edsInfo.parameterName;
			SortedList parameterNamesList = new SortedList();
			int index = 0;
			#endregion

			#region set array sub item 0 to contain the number of subs in this object & initialise other data items
			/*
			 * Set array sub item 0 which always is an unsigned 8 var containing the
			 * number of subs, always called NrOfObjects and is read only.
			 */
			sub0.accessLevel = edsInfo.accessLevel;
			sub0.accessType = ObjectAccessType.ReadOnly;
			sub0.compactNames = null;
			sub0.compactNoOfEntries = 0;
			sub0.compactObj = false;
			sub0.dataType = 5;							// unsigned 8
			sub0.defaultValue = edsInfo.defaultValue;
			sub0.displayType = CANopenDataType.NULL;
			sub0.highLimit = "";
			sub0.indexNumber = edsInfo.indexNumber;
			sub0.lowLimit = "";
			sub0.objectName = (int)SevconObjectType.NONE;
            sub0.objectNameString = "NONE";
			sub0.objFlags = edsInfo.objFlags;
			sub0.objectType = 7;						// VAR
			sub0.parameterName = "NrOfObjects";			// sub 0 always number of entries
			sub0.parameterValue = null;
			sub0.PDOmappable = false;
			sub0.scaling = edsInfo.scaling;
			sub0.sectionType = (int)SevconSectionType.NONE;
            sub0.sectionTypeString = "NONE";
			sub0.subNumber = 0;
			sub0.units = edsInfo.units;
			sub0.format = edsInfo.format;
			sub0.formatList = edsInfo.formatList;
			sub0.writeOnlyInPreOp = edsInfo.writeOnlyInPreOp;
			sub0.commsTimeout = edsInfo.commsTimeout;
			sub0.EDSObjectType = edsInfo.EDSObjectType;
			sub0.split = null;
			sub0.dummyVPDO = 0x00;
			sub0.nodeID = edsInfo.nodeID;
			sub0.displayOnMasterOnly = edsInfo.displayOnMasterOnly;
			#endregion

			// Set sub-object 0 in the object dictionary for the compact array.
			setSubDescription( sub0, thisEDS );

			#region set remaining array elements to have consecutive sub numbers
			/*
			 * Now set the remaining array elements which are all VAR of the main objects
			 * data type, have consecutive sub numbers and optional individual or
			 * constructed default parameter names.
			 */
			for ( int i = 0; i < edsInfo.compactNoOfEntries; i++ )
			{
				// Array list containing the default parameter names of all other subs
				parameterNamesList.Add( i, ( edsInfo.parameterName + i.ToString() ) );
			}
			#endregion

			#region convert extract the sub number and parameter name from compact format
			/* If the compact names specifically certain sub-objects then replace in array
			 * list with the parameter name read in from the EDS or DCF file.
			 */
			if ( edsInfo.compactNames != null )
			{
				System.UInt16 sub;

				/* For each parameter name specified in the EDS file, extract from the
				 * file, remove the default name from the array list then add the new
				 * string read from the file.
				 * Expected format is 1=a nice name so assume = delimited.
				 */
				for ( int i = 0; i < edsInfo.compactNames.Length; i++ )
				{
					split = edsInfo.compactNames[ i ].Split( '=' );

					if ( split.Length >= 2 )
					{
						sub = System.Convert.ToUInt16( split[ 0 ], 16 );
						index = parameterNamesList.IndexOfKey( sub );

						if ( index != -1 )
						{
							parameterNamesList.RemoveAt( index );
						}

						parameterNamesList.Add( sub, split[ 1 ] );
					}
				}
			}
			#endregion

			#region set object type, sub number and parameter names in edsInfo then set in data[][]
			/* For each sub-object defined in the compact EDS, set the object type and 
			 * sub number to the default values and retrieve the parameter name from the
			 * array list.  Then use these to set the sub description for this sub
			 * object in the object dictionary.  All other parameters in the subs of
			 * the array are assumed to be the same as the edsInfo object which was 
			 * passed as a parameter.
			 */
			for ( int i = 0; i < edsInfo.compactNoOfEntries; i++ )
			{
				edsInfo.objectType = 7;
				edsInfo.subNumber = i + 1;			// subs must always be consecutively numbered
				index = parameterNamesList.IndexOfKey( i + 1 );

				if ( index != -1 )
				{
					edsInfo.parameterName = parameterNamesList.GetByIndex( index ).ToString();
				}
				else
				{
					edsInfo.parameterName = originalParameterName + (i + 1).ToString();
				}

				// Set the sub object description in the DW's object dictionary.
				setSubDescription( edsInfo, thisEDS );
			}
			#endregion
		}

		//-------------------------------------------------------------------------
		//  Name			: setCompactSubObjValues()
		//  Description     : Sets the currentValue for each of the sub objects 
		//					  defined in the compactSubObj format in the DCF file.
		//					  NOTE: all compact sub objects have their subs numbered
		//					  consecutively from 0 where 0 is an unsigned 8 defining
		//					  how many subs are in the array.
		//  Parameters      : edsInfo - contains all the parameter values in the
		//							    compact sub object format
		//					  thisEDS - EDS object needed to allow access to the
		//								convertToLong() function
		//					  odSub-  od Sub item
		//  Used Variables  : None
		//  Preconditions   : The DCF file being read has the parameter value 
		//					  for an array defined in the compact format.
		//  Post-conditions : thisItem ODItemData array has already been constructed
		//					  from the EDS or DCF, ready to add the currentValue
		//					  read in from file.
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>Sets the currentValue for each sub object defined in the compact format of the DCF file.</summary>
		/// <param name="edsInfo">the eds infomation which contains the domain filename as the parameter 
		/// value for a domain object </param>
		/// <param name="thisEDS">EDSorDCF object reference so that the EDS conversion routines for 
		/// Sevcon object and section type can be used</param>
		/// <param name="thisItem">ODItemData array reference for the sub objects so can set the 
		/// currentValue as read in from the file</param>
		private void setCompactSubObjValues( EDSObjectInfo edsInfo, EDSorDCF thisEDS, ODItemData odSub )
		{
			ObjDictItem odItem = this.getODItemAndSubs(odSub.indexNumber);
			#region local variable declaration and variable initialisation
			string [] split;
			SortedList parameterValuesList = new SortedList();
			int index = 0;
			#endregion

			#region Setup the default parameter values in a sorted list.
			for ( int i = 0; i < edsInfo.compactNoOfEntries; i++ )
			{
				parameterValuesList.Add( i, ( edsInfo.defaultValue ) );
			}
			#endregion

			#region extract current value from DCF text format
			/* Read in the parameter value from the EDS or DCF file and replace the
			 * default value in the array list with the new value read from file.
			 * Expected format: 1=1234 i.e. = delimited in hex format for sub 1 default value.
			 */
			if ( edsInfo.parameterValue != null )
			{
				System.UInt16 sub = 0;

				for ( int i = 0; i < edsInfo.parameterValue.Length; i++ )
				{
					split = edsInfo.parameterValue[ i ].Split( '=' );

					if ( split.Length >= 2 )
					{
						try
						{
							sub = System.Convert.ToUInt16( split[ 0 ], 16 );
							index = parameterValuesList.IndexOfKey( sub );
						}
						catch ( Exception )
						{
						}

						// Already in array list so remove to avoid an exception.
						if ( index != -1 )
						{
							parameterValuesList.RemoveAt( index );
						}

						// Add in new default value now it's been read from the file.
						parameterValuesList.Add( sub, split[ 1 ] );
					}
				}
			}
			#endregion

			#region Convert the text value into a data format suitable for DriveWizard
			/* For each compact object, convert the default value (still stored as a text 
			 * string) into the format required for the display type suitable for DW to display.
			 */
			for ( int i = 0; i < edsInfo.compactNoOfEntries; i++ )
			{
				// retrieve the value of this sub index from the array list.
				index = parameterValuesList.IndexOfKey( i + 1 );

				ODItemData compODSub = (ODItemData)  odItem.odItemSubs[ i + 1 ];
				/* If default value found in the array list OK, then convert to the suitable
				 * data format for display on DW.
				 */
				if ( index != -1 )
				{
					#region convert from CANopen data type to DW data type
					switch ( ((ODItemData) odItem.odItemSubs[ i + 1]).displayType )
					{
							#region visible string
						case CANopenDataType.VISIBLE_STRING:
						{
							// No conversion needed so keep as a text string.
							compODSub.currentValueString = (string)parameterValuesList.GetByIndex( index );
							break;
						}
							#endregion

							#region boolean
						case CANopenDataType.BOOLEAN:
						{ //no need to sear ch for $NODEID on booleans
							// Convert from ASCII hex to an integer then into a boolean.
							if ( sysInfo.convertToLong( (string)parameterValuesList.GetByIndex( index )) > 0 )
							{
								compODSub.currentValue = (long)1;
							}
							else
							{
								compODSub.currentValue = (long)0;
							}
							break;
						}
							#endregion

							#region unsigned integer
						case CANopenDataType.UNSIGNED8:
						case CANopenDataType.UNSIGNED16:
						case CANopenDataType.UNSIGNED24:
						case CANopenDataType.UNSIGNED32:
						case CANopenDataType.UNSIGNED40:
						case CANopenDataType.UNSIGNED48:
						case CANopenDataType.UNSIGNED56:
						case CANopenDataType.UNSIGNED64:
						{
							// Convert from an ASCII hex data format into an unsigned integer.
							compODSub.currentValue =
								sysInfo.convertToLong( (string)parameterValuesList.GetByIndex( index ));
							break;
						}
							#endregion

							#region signed integer
						case CANopenDataType.INTEGER8:
						case CANopenDataType.INTEGER16:
						case CANopenDataType.INTEGER24:
						case CANopenDataType.INTEGER32:
						case CANopenDataType.INTEGER40:
						case CANopenDataType.INTEGER48:
						case CANopenDataType.INTEGER56:
						case CANopenDataType.INTEGER64:
						{
							// Convert from an ASCII hex data format into a signed integer.
							long temp = sysInfo.convertToLong( (string)parameterValuesList.GetByIndex( index ));
							temp = sysInfo.convertToSignedLong(compODSub.displayType, temp );
							compODSub.currentValue = temp;
							break;
						}
							#endregion

							#region domain
						case CANopenDataType.DOMAIN:
						{
							// not handled at present - ever going to happen?
							break;
						}
							#endregion

							#region array
						case CANopenDataType.ARRAY:
						{
							// do nothing - we are already dealing with arrays here!
							break;
						}
							#endregion

							#region real
						case CANopenDataType.REAL32:
						{
							if (compODSub.real32 != null )
							{
								//Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
								//a bitstring and a base 10 number contianing onyl 1s and 0s - redundant input parameter
								compODSub.real32.currentValue = sysInfo.convertToFloat( (string)parameterValuesList.GetByIndex( index ) );
							}
							break;
						}
						case CANopenDataType.REAL64:
						{
							if (compODSub.real64 != null )
							{
								compODSub.real64.currentValue =	sysInfo.convertToDouble( (string)parameterValuesList.GetByIndex( index ) );
							}
							break;
						}
							#endregion

							#region CANopen data types currently not handled by DW
						case CANopenDataType.IDENTITY:
						case CANopenDataType.OCTET_STRING:
						case CANopenDataType.PDO_COMMUNICATION_PARAMETER:
						case CANopenDataType.PDO_MAPPING:
						case CANopenDataType.RECORD:
						case CANopenDataType.SDO_PARAMETER:
						case CANopenDataType.TIME_DIFFERENCE:
						case CANopenDataType.TIME_OF_DAY:
						case CANopenDataType.UNICODE_STRING:
						case CANopenDataType.NULL:
						default:
						{
							// unhandled data types for V1 of DW.
							compODSub.currentValue = 0;
							break;
						}
							#endregion
					} // end of switch statement
					#endregion
				} // end of if the index was found in the array list
			} // end of for each sub object defined in the compact sub object format
			#endregion
		}
		

		#endregion

		#region identity related
		/* Vendor ID, used to check whether it is a Sevcon node or a 3rd party.
		 * This is read from the controller and stored in this object with all
		 * other objects only having read access.
		 */
		protected uint	_vendorID = 0xFFFFFFFF;
		///<summary>Vendor specific identifier (read from object 0x1018)</summary>
		[XmlIgnore]
		public uint		vendorID
		{
			get
			{
				return ( _vendorID );
			}
//			set
//			{
//				_vendorID = value;
//			}
		}

		/* Product code, used to check whether DW has a matching EDS file available.
		 * This is read from the controller and stored in this object with all
		 * other objects only having read access.
		 */
		protected uint	_productCode = 0xFFFFFFFF;
		///<summary>Unique product code (read from object 0x1018)</summary>
		[XmlIgnore]
		public uint		productCode
		{
			get
			{
				return ( _productCode );
			}

//			set
//			{
//				_productCode = value;
//			}
		}

		/* Revision number, again used to check whether DW has a matching EDS file available.
		 * This is read from the controller and stored in this object with all
		 * other objects only having read access.
		 */
		
		protected uint	_revisionNumber = 0xFFFFFFFF;
		///<summary>Product revision number (from object 0x1018)</summary>
		[XmlIgnore]
		public uint		revisionNumber
		{
			get
			{
				return ( _revisionNumber );
			}

//			set
//			{
//				_revisionNumber = value;
//			}
		}

		private byte _productVariant = 0x00;
		///<summary>Product variant number (from object 0x1018)</summary>
		[XmlIgnore]
		public byte		productVariant
		{
			get
			{
				if(identityUpdated == true)
				{
					updateIdentityRelatedParameters();
					identityUpdated = false;
				}
				return ( _productVariant );
			}
		}

		private byte _productRange = 0x00;
		///<summary>Product range number (from object 0x1018)</summary>
[XmlIgnore]
		public byte	productRange
		{
			get
			{
				if(identityUpdated == true)
				{ //needto do them all together
					updateIdentityRelatedParameters();
					identityUpdated = false;
				}
				return ( _productRange );
			}
		}

		private byte _productVoltage = 0x00;
		///<summary>Product voltage number (from object 0x1018)</summary>
		[XmlIgnore]
		public byte		productVoltage
		{
			get
			{
				if(identityUpdated == true)
				{ //needto do them all together
					updateIdentityRelatedParameters();
					identityUpdated = false;
				}
				return ( _productVoltage );
			}
		}

		private byte _productCurrent = 0x00;
		///<summary>Product current number (from object 0x1018)</summary>
[XmlIgnore]
		public byte		productCurrent
		{
			get
			{
				if(identityUpdated == true)
				{ //needto do them all together
					updateIdentityRelatedParameters();
					identityUpdated = false;
				}
				return ( _productCurrent );
			}
		}

		#region	No current requirement for productName in DW
		//private string	_productName;
		//public string productName
		//{
		//	get
		//	{
		//		return ( _productName );
		//	}
		//}
		#endregion

		private void updateIdentityRelatedParameters()
		{
			uint intCode = 0;
			#region productVariant
			if(_productCode != 0xFFFFFFFF)  //we need to exclude unknown ( and thus defualt) product code
			{
				intCode = _productCode & 0x00ff0000;
				intCode = intCode >> 16;
				_productVariant = (byte)intCode;
			}
			#endregion productVariant

			#region product Range
			intCode = _productCode & 0xff000000;
			intCode = intCode >> 24;
			_productRange = (byte)intCode;
			#endregion productrange

			#region product voltage
			intCode = _productCode & 0x0000ff00;
			intCode = intCode >> 8;
			_productVoltage = (byte)intCode;
			#endregion product voltage

			#region product current
			intCode = _productCode & 0x000000ff;
			_productCurrent = (byte)intCode;
			#endregion product current
		}
		//-------------------------------------------------------------------------
		//  Name			: readDeviceIdentity()
		//  Description     : This function communicates with a device already detected
		//					  on the connected CANbus of the given node ID and requests
		//					  the three items which determine what the product is:
		//					  vendor ID, product code and revision number.  These will
		//					  then be used later by DW to find a matching EDS file
		//					  for this controller.
		//  Parameters      : nodeID - the node ID of the connected controller for which
		//							   the device information is to be retrieved (needed
		//							   by CANcomms to build up the right COBID for SDOs)
		//	Used Variables	: CANcomms - CAN communications object (tx & rx on CANbus)
		//  Used Variables  : _vendorID - formatted vendor ID retrieved from the controller
		//					  _productCode - formatted product code retrieved from controller
		//					  _revisionNumber - formatted product revision retrieved from
		//										the controller
		//  Preconditions   : A controller has been detected on the connected CANbus and
		//					  has a node ID of nodeID.
		//  Post-conditions : The 3 properties defining the attached product are assigned
		//					  the values read back from the controller.
		//  Return value    : feedback - feedback to indicate whether the the device data
		//						    request was successful or a failure reason
		//--------------------------------------------------------------------------
		///<summary>Reads the vendor ID, product code and revision node from the connected device of the given nodeID.</summary>
		/// <param name="nodeID">the node ID of the connected controller for which
		/// the device information is to be retrieved (needed by CANcomms to build up the right COBID for SDOs)</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>

		public DIFeedbackCode readDeviceIdentity()
		{
            //note this Method MUST do an explict call to SDORead - we always read identity even if it is not in and EDS
            //readODValeu pre-assumes that the object exists in the EDS
            byte[] rxData = new byte[8];
            DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
            int noOfReadFailures = 0;

            if (MAIN_WINDOW.isVirtualNodes == false)
            {
                identityUpdated = true; //force recalculation of product ranges etc
                /* Device identity is mandatory and always index 0x1018
                 * Request the vendor ID from the controller.
                 * Use default communications timeouts for all SDOReads called here as we have not
                 * got an EDS for this device yet to specify otherwise.
                 */
                for (ushort i = 0; i < SCCorpStyle.SDONoResponseRetries; i++)
                {
                    feedback = sysInfo.CANcomms.SDORead(nodeID, SCCorpStyle.identityObjectIndex, SCCorpStyle.vendorIDObjectSubIndex, SCCorpStyle.sizeOfVendorID, ref rxData, SCCorpStyle.TimeoutDefault);
                    if (feedback != DIFeedbackCode.DINoResponseFromController)
                    {
                        break;
                    }
                }

                // Only format the vendorID data and request product code if controller replied OK.
                // vendor ID is a mandatory OD item.
                //DR38000270 check length to prevent exception
                if ((feedback == DIFeedbackCode.DISuccess) && (rxData.Length >= 4))
                {
                    // little endian format so format the raw bytes to reconstruct the vendorID
                    _vendorID = (uint)rxData[0];
                    _vendorID += ((uint)rxData[1] << 8);
                    _vendorID += ((uint)rxData[2] << 16);
                    _vendorID += ((uint)rxData[3] << 24);
                }
                else
                {
                    noOfReadFailures++;
                    _vendorID = 0xFFFFFFFF;  //use default - to indicate not available = for minimal ducitunoary etc
                    SystemInfo.errorSB.Append("Failed to read Vendor ID for Node ID: ");
                    SystemInfo.errorSB.Append(this.nodeID.ToString());
                    SystemInfo.errorSB.Append("\n");
                }

                // request the product code from the controller.
                // this is an optional OD item.
                for (ushort i = 0; i < SCCorpStyle.SDONoResponseRetries; i++)
                {
                    feedback = sysInfo.CANcomms.SDORead(nodeID, SCCorpStyle.identityObjectIndex, SCCorpStyle.productCodeObjectSubIndex, SCCorpStyle.sizeOfProductCode, ref rxData, SCCorpStyle.TimeoutDefault);
                    if (feedback != DIFeedbackCode.DINoResponseFromController)
                    {
                        break;
                    }
                }

                if ((feedback == DIFeedbackCode.DISuccess) && (rxData.Length >= 4)) //DR38000270
                {
                    // little endian format so format the raw bytes to reconstruct the product code.
                    _productCode = (uint)rxData[0];
                    _productCode += ((uint)rxData[1] << 8);
                    _productCode += ((uint)rxData[2] << 16);
                    _productCode += ((uint)rxData[3] << 24);
                }
                else
                {
                    _productCode = 0xFFFFFFFF;
                    if ((feedback == DIFeedbackCode.DINoResponseFromController) || (this.vendorID == SCCorpStyle.SevconID))
                    {
                        noOfReadFailures++;
                        SystemInfo.errorSB.Append("Failed to read product code for node ID: ");
                        SystemInfo.errorSB.Append(this.nodeID.ToString());
                        SystemInfo.errorSB.Append("\n");
                    }
                }

                // request the product revision from the controller.
                // this is an optional OD item.
                for (ushort i = 0; i < SCCorpStyle.SDONoResponseRetries; i++)
                {
                    feedback = sysInfo.CANcomms.SDORead(nodeID, SCCorpStyle.identityObjectIndex, SCCorpStyle.revisionNumberObjectSubIndex, SCCorpStyle.sizeOfRevisionNumber, ref rxData, SCCorpStyle.TimeoutDefault);
                    if (feedback != DIFeedbackCode.DINoResponseFromController)
                    {
                        break;
                    }
                }


                if ((feedback == DIFeedbackCode.DISuccess) && (rxData.Length >= 4)) //DR38000270
                {
                    // little endian format so format the raw bytes to reconstruct the product revision
                    _revisionNumber = (uint)rxData[0];
                    _revisionNumber += ((uint)rxData[1] << 8);
                    _revisionNumber += ((uint)rxData[2] << 16);
                    _revisionNumber += ((uint)rxData[3] << 24);
                }
                else
                {
                    _revisionNumber = 0xFFFFFFFF;
                    if ((feedback == DIFeedbackCode.DINoResponseFromController) || (this.vendorID == SCCorpStyle.SevconID))
                    {
                        noOfReadFailures++;
                        SystemInfo.errorSB.Append("Failed to read Revision number code for node ID: ");
                        SystemInfo.errorSB.Append(this.nodeID.ToString());
                        SystemInfo.errorSB.Append("\n");
                    }
                }

                if (noOfReadFailures >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
                {
                    SystemInfo.errorSB.Append("Process aborted.\nThere has been three consecutive read failures.\n");
                    SystemInfo.errorSB.Append("Please check the connections and power supply for node ID: ");
                    SystemInfo.errorSB.Append(this.nodeID.ToString());
                    SystemInfo.errorSB.Append("\nYou may need to reconnect to the system.");
                }
                // No current requirement for productName
            }
            return (feedback);  //feedback is needed because if it is CaANGeneral error then furthe rhansdling is performed in the callign method
        }

		///<summary>Sets the device identity etc for DWMockup.</summary>
		///<param name="product">product number to set this to</param>
		///<param name="revision">revision number to set this to</param>
		///<param name="vendor">vendor number to set this to</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public void setDeviceIdentity( uint vendor, uint product, uint revision )
		{
			_vendorID = vendor;
			_productCode = product;
			_revisionNumber = revision;
			identityUpdated = true; //force recalculation of product ranges etc
		}
		#endregion identitiy related
	}

	#region DCF node class
	public class DCFNode: nodeInfo
	{
		#region DCF checksum OK flag
		/* Does the checksum embedded within the DCF file match that calculated by DW?
		 * If so, then the backdoor DCF method will be allowed to download a DCF to
		 * a user level higher than was logged in at. This is only allowed for known
		 * verified DCF files created by DW or the Sevcon Excel sheet.  ANy changes
		 * in the DCF file at all will result in a wrong checksum and the backdoor
		 * method will be denied. It can still be used for normal DCF downloads to
		 * write all parameters that the user has the required login access for.
		 */
		private bool _DCFChecksumOK = false;
		/// <summary>Indicates if DCF file checksum corresponds to DriveWizard calculated checksum.</summary>
		public bool DCFChecksumOK
		{
			get
			{
				return ( _DCFChecksumOK );
			}
		}
		#endregion
		internal bool includeChecksum = false;

		public nodeInfo DCFSourceNode = null;

		public DCFNode( SystemInfo passed_sysInfo, int dcfTableIndex) : base(passed_sysInfo,dcfTableIndex)
		{
			this.sysInfo = passed_sysInfo;
			this._deviceType = "";
			this.EDS_DCF = new EDSorDCF(this, this.sysInfo);
		}

		public void cloneSourceToDCF(nodeInfo sourceNode)
		{
			this.DCFSourceNode = sourceNode;
			this._accessLevel = sourceNode.accessLevel;
			this._deviceType = sourceNode.deviceType;
			this._productCode = sourceNode.productCode;
			this._revisionNumber = sourceNode.revisionNumber;
			this._vendorID = sourceNode.vendorID;
			this._EDSorDCFfilepath   = sourceNode.EDSorDCFfilepath;
			this._manufacturer = sourceNode.manufacturer;
			this.nodeID = sourceNode.nodeID;
			this.EDS_DCF.EDSdeviceInfo = sourceNode.EDS_DCF.EDSdeviceInfo;
			this.EDS_DCF.FileInfo = sourceNode.EDS_DCF.FileInfo;
			applyDCFSpecificDefaults();
		}
		public void applyDCFSpecificDefaults()
		{
			this.masterStatus = true;  //DCF is always a master to ensure that all DCF items can be shown

		}
		//-------------------------------------------------------------------------
		//  Name			: readDCFfile()
		//  Description     : This function reads in the DCF file of the given filename
		//					  passed as a parameter and constructs an object dictionary
		//					  to match the DCF (based closely on an EDS) then populates
		//					  each object's currentValue in DW's OD with the parameter
		//					  value from the DCF.  This will completely redefine any OD
		//					  already constructed in this nodeInfo array element but is
		//					  expected to be called from the DCFnode instance of NodeInfo.
		//  Parameters      : filename - DCF filename with extension .dcf, complying to 
		//							     the EDS/DCF CANopen standard.
		//  Used Variables  : DCF - DCF object (inherited from the EDS object with the
		//						    addition of parameter value handling) which is
		//							designed to parse the DCF file to create the OD.
		//  Preconditions   : The DCF filename has been selected by the user and complies
		//					  to the CANopen EDS/DCF standard.
		//  Post-conditions : The calling instance of NodeInfo has it's dictionary
		//					  constructed with the objects defined in the DCF and the 
		//					  currentValues are set to the parameter values in the file.
		//					  If an invalid file is selected, the dictionary constructed
		//					  should be ignored because the feedback should indicate failure.
		//  Return value    : feedback - feedback code indicating success or reason of failure
		//--------------------------------------------------------------------------
		///<summary>Reads a DCF file and constructs the DCF node object dictionary to represent it.</summary>
		/// <param name="filename">DCF filename with extension .dcf, complying to the EDS/DCF 
		/// CANopen standard</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode readDCFfile(  )
		{
			DIFeedbackCode feedback = DIFeedbackCode.DIUnableToReadDCFFile;
			this.DCFSourceNode = null;
			// If a filename has been selected by the user, try and open the DCF file for read only.
			_DCFChecksumOK = false;
			#region open DCF streamreader
			feedback = EDS_DCF.open( this._EDSorDCFfilepath, FileAccess.Read, nodeID );
			// If file opened OK, read the file info section into the DCF object. DR38000260 error reporting
			if ( (feedback != DIFeedbackCode.DISuccess) && (MAIN_WINDOW.appendErrorInfo == true))
            {
				SystemInfo.errorSB.Append("\nFailed to open DCF file:");
				SystemInfo.errorSB.Append(_EDSorDCFfilepath);
				return feedback; 
			}
			#endregion open DCFstreamreaders

			#region extract the DCF node device 0x1018 params from DCF file
			uint vendorID, productCode, revNo;
			feedback = EDS_DCF.readDeviceInfo(this._EDSorDCFfilepath, out vendorID, out productCode, out revNo);
			this._vendorID = vendorID;
			this._productCode = productCode;
			this._revisionNumber = revNo;
			if ( feedback != DIFeedbackCode.DISuccess )
			{
				return feedback; 
			}
			#endregion extract the DCF node device 0x1018 params from DCF file
			#region read the commissioning section (baud & nodeID).
			feedback = EDS_DCF.readDCFCommissioningInfo(this._EDSorDCFfilepath);
			if ( feedback != DIFeedbackCode.DISuccess )
			{
				return feedback; 
			}
			this.nodeID = this.EDS_DCF.comInfo.nodeID;
			#endregion read the commissioning section (baud & nodeID).

			#region read the total number of DCF objects
			feedback = EDS_DCF.readNoOfObjects(this._EDSorDCFfilepath );
			if ( feedback != DIFeedbackCode.DISuccess )
			{
				return feedback; 
			}
			#endregion read the total number of DCF objects

			#region read all object descriptions
			/* If read number OK, read all the objects in turn to populate each odItem
				 * element. Sub-objects are created when their definition is read in the DCF file,
				 * ODItemData being a two-dimensional jagged array.
				 */
			feedback = EDS_DCF.readAllObjectDescriptions(this._EDSorDCFfilepath, this );
			if ( feedback != DIFeedbackCode.DISuccess )
			{
				return feedback; 
			}
			#endregion read all object descriptions

			#region verify checksum
			_DCFChecksumOK = EDS_DCF.verifyDCFChecksumOK(this._EDSorDCFfilepath);
			#endregion verify checksum

			#region apply access level to header items for ease of GUI processing
			byte prodVariant = 0x00;
			uint intCode = this.EDS_DCF.EDSdeviceInfo.productNumber & 0x00ff0000;
			intCode = intCode >> 16;
			prodVariant = (byte)intCode;

			if((this.EDS_DCF.EDSdeviceInfo.vendorNumber == SCCorpStyle.SevconID)
				&& ( prodVariant > SCCorpStyle.App_variant_lowlimit )
				&& ( prodVariant < SCCorpStyle.App_variant_highlimit ))
			{
				foreach(ObjDictItem odItem in this.objectDictionary)
				{
					if(odItem.odItemSubs.Count>1)
					{
						byte lowestAccessLevel=5;
						foreach(ODItemData odSub in odItem.odItemSubs)
						{
							if((odSub.subNumber!=-1)&&(odSub.subNumber!=0))
							{
								lowestAccessLevel = System.Math.Min(lowestAccessLevel, odSub.accessLevel);
							}
						}
						((ODItemData)odItem.odItemSubs[0]).accessLevel = lowestAccessLevel;
					}
				}
			}
			#endregion apply access level to header items for ease of GUI processing
			EDS_DCF.closeEDSorDCF();
			return ( feedback );
		}
		///<summary>Writes a partial DCF text file of filename using the current values in DW's replica of the physical device's OD.
		///It only writes the OD items in the objectList that the user has sufficient access to.</summary>
		/// <param name="filename">user selected name of the DCF file</param>
		/// <param name="baud">baud rate as found when connecting to the system
		/// (this and the node ID are in commissioning section)</param>
		/// <param name="objectList">list of object index and subs to be written to partial DCF</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode writeDCFfile(BaudRate baud)
		{
			DIFeedbackCode feedback = DIFeedbackCode.DISuccess;
			if (this.includeChecksum == true)
			{ //need to read back to calculte checksum
				feedback = EDS_DCF.open( this._EDSorDCFfilepath, FileAccess.ReadWrite,this.nodeID);
			}
			else
			{
				feedback = EDS_DCF.open( this._EDSorDCFfilepath, FileAccess.Write,this.nodeID);
			}
			// If EDS opened OK, writes DCF file info section.
            if (feedback == DIFeedbackCode.DISuccess)
            {
                #region write [FileInfo], [devcieInfo] and Commissioning sections
                EDS_DCF.writeDCFFileInfoSection(this);
                EDS_DCF.writeDCFDeviceInfoSection(this);
                EDS_DCF.writeDCFCommissioningInfoSection(this, baud);
                #endregion write [FileInfo], [devcieInfo] n dCommisioning sections
                #region split the DCF OD into mandatory, optional and manufacturer items in numerical order
                ArrayList mandatoryItems = new ArrayList();
                ArrayList optionalItems = new ArrayList();
                ArrayList manufacturerItems = new ArrayList();
                if (this.accessLevel > 0) //judetemp ??why
                {
                    foreach (ObjDictItem odItem in this.objectDictionary)
                    {
                        ODItemData firstodSub = (ODItemData)odItem.odItemSubs[0];
                        if (firstodSub.EDSObjectType == CANObjectType.Mandatory)
                        {
                            mandatoryItems.Add(firstodSub.indexNumber);
                        }
                        else if (firstodSub.EDSObjectType == CANObjectType.Optional)
                        {
                            optionalItems.Add(firstodSub.indexNumber);
                        }
                        else if (firstodSub.EDSObjectType == CANObjectType.Manufacturer)
                        {
                            manufacturerItems.Add(firstodSub.indexNumber);
                        }
                    }
                }
                mandatoryItems.Sort(); //put into numerical order for readability
                optionalItems.Sort();
                manufacturerItems.Sort();
                #endregion  split the DCF OD into mandatory, optional and manufacturer items in numerical order
                #region write the section headers and the individual OD headers and subs
                this._itemBeingRead = 0;
                this.EDS_DCF.writeObjectHeaderSection("[MandatoryObjects]", mandatoryItems);
                foreach (int mandIndex in mandatoryItems)
                {
                    this._itemBeingRead++;
                    this.EDS_DCF.writeDCFItemAndSubs(mandIndex, this._EDSorDCFfilepath, this.accessLevel, this);
                }
                this.EDS_DCF.writeObjectHeaderSection("[OptionalObjects]", optionalItems);
                foreach (int optIndex in optionalItems)
                {
                    this._itemBeingRead++;
                    this.EDS_DCF.writeDCFItemAndSubs(optIndex, this._EDSorDCFfilepath, this.accessLevel, this);
                }
                this.EDS_DCF.writeObjectHeaderSection("[ManufacturerObjects]", manufacturerItems);
                foreach (int manfIndex in manufacturerItems)
                {
                    this._itemBeingRead++;
                    this.EDS_DCF.writeDCFItemAndSubs(manfIndex, this._EDSorDCFfilepath, this.accessLevel, this);
                }
                #endregion write the section headers and the individual OD headers and subs
                #region write checksum if required
                // only write the checksum to file if a Sevcon engineer (prevents use of backdoor method)
                if (this.includeChecksum == true)
                {
                    this.EDS_DCF.DCFsw.Flush(); //gets - recreated if we do Rewind
                    feedback = EDS_DCF.writeDCFChecksum(this._EDSorDCFfilepath);
                }
                #endregion write checksum if required
            }
			this.EDS_DCF.closeEDSorDCF(); // Close the DCF file
			return ( feedback );
		}

	}
	#endregion DCF node class

	#region ObjDictItem class
	public class ObjDictItem 
	{
		#region OD item
		public ObjDictItem()
		{
		}
		#region values read in from xml file
		[XmlArray( "ODsubs" ), XmlArrayItem( "ODsub", typeof( ODItemData ) )]
		public ArrayList odItemSubs = new ArrayList();
		#endregion values read in from xml file
		#endregion OD item
	}
	#endregion ObjDictItem class


	#region ODItemData class
	/// <summary>
	///  The ODItemData structure is the basic data structure used to represent an individual
	///  item in the object dictionary i.e. the data[][] jagged array used to represent the
	///  entire object dictionary of a node consists of instances of this data structure.
	///  A single object dictionary item of a given index and sub is represented by
	///  this.  Occasionally, when an OD item represents a record (with subs) or an array 
	///  (with subs), a header object is needed to contain this data.
	///  Note this NEEDS to be opubli because we serialize it in from XML
	/// </summary>
	public class ODItemData
	{
		#region constructor
		public ODItemData()
		{}
		#endregion constructor

		#region CANnode
		[XmlIgnore]
		internal nodeInfo CANnode = null;
		#endregion CANnode

		#region indexNumber
		private int _indexNumber = 0;
		/// <summary>index number of OD item this represents</summary>
		[XmlElement("indexNumber",typeof(int))]
		public int					indexNumber
		{
			get
			{
				return _indexNumber;
			}
			set
			{
				_indexNumber = value;
			}
		}
		#endregion indexNumber

		#region subNumber
		private int _subNumber = 0;
		/// <summary>sub index of OD item this represents</summary>
		[XmlElement("subNumber",typeof(int))]
		public int	subNumber
		{
			get
			{
				return _subNumber;
			}
			set
			{
				_subNumber = value;
			}
		}
		#endregion subNumber
		
		#region parameterName
		private string _parameterName = "";
		/// <summary>text string identifier of this object</summary>
		[XmlElement("parameterName",typeof(string))]
		public string				parameterName
		{
			get 
			{
				return _parameterName;
			}
			set
			{
				_parameterName = value;
			}
		}
		#endregion parameterName
	
		#region tooltip
		private string _tooltip = "";
		[XmlElement("tooltip",typeof(string))]
		public string				tooltip
		{
			get 
			{
				//				if(_tooltip == "")
				//				{//do once only 
				//					_tooltip = "Tooltip for 0x" + this.indexNumber.ToString("X") + ", sub 0x" + this.subNumber.ToString("X").PadLeft(3, '0');
				//				}
				return _tooltip;
			}
			set
			{
				_tooltip = value;
			}
		}
		#endregion tooltip

		#region accessType
		private ObjectAccessType _accessType = ObjectAccessType.Constant;
		/// <summary>read only, write only etc.</summary>
		[XmlElement("accessType",typeof(ObjectAccessType))]
		public ObjectAccessType		accessType
		{
			get
			{
				return _accessType;
			}
			set
			{
				_accessType = value;
			}
		}
		#endregion accessType

		#region PDOmappable
		private bool _PDOmappable = false;
		/// <summary>whether it is possible to map this item to a PDO</summary>
		[XmlElement("PDOmappable",typeof(bool))]
		public bool	PDOmappable
		{
			get
			{
				return _PDOmappable;
			}
			set
			{
				_PDOmappable = value;
			}
		}
		#endregion PDOmappable

		#region objFlags
		private byte _objFlags = 0;
		/// <summary>special info associated with DCF downloads and uploads for DCFs</summary>
		[XmlElement("objFlags",typeof(byte))]
		public byte	objFlags
		{
			get
			{
				return _objFlags;
			}
			set
			{
				_objFlags = value;
			}
		}
		#endregion objFlags									

		#region dataType
		private byte _dataType = 0;
		/// <summary>data type (as read from EDS or DCF file)</summary>
		[XmlElement("dataType",typeof(byte))]
		public byte					dataType
		{
			get
			{
				return _dataType;
			}
			set
			{
				_dataType = value;
			}
		}
		#endregion dataType
	
		#region objectVersion
		//no need to put this in a monitoring file
		private ushort _objectVersion = 0;
		/// <summary>data type (as read from EDS or DCF file)</summary>
		[XmlIgnore]
		public ushort					objectVersion
		{
			get
			{
				return _objectVersion;
			}
			set
			{
				_objectVersion = value;
			}
		}
		#endregion objectVersion
	
	
		private short _dataSizeInbits = 0;
		[XmlElement("dataSizeInBits",typeof(short))]
		public short dataSizeInBits
		{
			get
			{
				if(_dataSizeInbits == 0)  //only calculate once
				{
					if(this.bitSplit != null)
					{
						#region determine dataIsze in bits from the bit mask and bitshift parameters
						long temp = 0xffff;
						temp = temp & this.bitSplit.bitMask;
						temp = temp >> (this.bitSplit.bitShift + 1);
						_dataSizeInbits = (short)(temp + 1);  //gives us number of bits
						#endregion determine dataIsze in bits from the bit mask and bitshift parameters
					}
					else
					{
						#region determine datasize in bits from the dataType
						switch(this._dataType)
						{
							case 0x1:  //boolean
								_dataSizeInbits = 1;
								break;
							case 0x2://intetger 8
							case 0x5: //unsigned 8
								_dataSizeInbits = 8;
								break;
							case 0x3: //integer 16:
							case 0x6: //unsigned 16
								_dataSizeInbits = 16;
								break;
							case 0x4:  //integer 32
							case 0x7: //unsigned 32
							case 0x8: //real32
								_dataSizeInbits = 32;
								break;
							case 0x10:  //integer 24
							case 0x16: //unsigned 24
								_dataSizeInbits = 24;
								break;
							case 0x15: //integer 64
							case 0x11:  //real64
							case 0x1B: //unsigned 64
								_dataSizeInbits = 64;
								break;
							case 0x12: //Integer 40
							case 0x18: //unsigned 40
								_dataSizeInbits = 40;
								break;
							case 0x13: //integer 48
							case 0x19: //unsigned 48
								_dataSizeInbits =48;
								break;
							case 0x14: //integer 56
							case 0x1A: //unsigned 56
								_dataSizeInbits =56;
								break;
							default:
								_dataSizeInbits = -1;  //deliberate out of range
								break;
						}
						#endregion determine datasize in bits from the dataType
					}
				}
				return _dataSizeInbits;
			}
		}
		#region objectType
		private byte _objectType = 7;
		/// <summary>object type (as read from EDS or DCF file)</summary>
		[XmlElement("objectType",typeof(byte))]
		public byte					objectType
		{
			get
			{
				return _objectType;
			}
			set
			{
				_objectType = value;
			}
		}
		#endregion objectType

		#region scaling
		//Note this must be a double to cope with sclaings in controller EDS files eg 0x6410 objects
		private double _scaling = 1F;
		/// <summary>scaling to be applied to this instance if it is an integer data types </summary>
		[XmlElement("scaling",typeof(double))] 
		public double				scaling
		{
			get
			{
				return _scaling;
			}
			set
			{
				_scaling = value;
			}
		}
		#endregion scaling
		
		#region units
		private string _units = "";
		/// <summary>units (eg Amps, Volts, mV etc) to be displayed with this OD item</summary>
		[XmlElement("units",typeof(string))] 
		public string				units
		{
			get
			{
				return _units;
			}
			set
			{
				_units = value;
			}
		}
		#endregion units										

		#region  displayType
		private CANopenDataType _displayType = CANopenDataType.NULL;
		/// <summary>derived from the dataType and objectType above this says what the actual data type of this item is</summary>
		[XmlElement("displayType",typeof(CANopenDataType))] 
		public CANopenDataType		displayType
		{
			get
			{
				if(_displayType == CANopenDataType.NULL)
				{
					switch ( this._objectType )
					{
							#region simple data type definitions
						case 2:
						case 5:
						case 7:
						{
							/* 5,7 Denotes a type definition such as a boolean, unsigned16, float etc.
							 * Convert the data type into a CANopenDataType directly.  See the
							 * CANopenDataType enumerated type for the simple data definition types.
							 */
							/* 2 denotes domain*/
							_displayType = (CANopenDataType) this._dataType;
							break;
						}
							#endregion

							#region array data type
						case 8:
						{
							/* A multiple data field object where the data fields may be any combination
							 * of simple basic data types e.g. an array of unsigned16 etc.  Sub index
							 * 0 is an unsigned 8 and therefore not part of the array data.
							 * Convert the data type into a CANopenDataType directly then OR in to
							 * retain the fact that it is also an array.
							 */
							_displayType =  (CANopenDataType.ARRAY) | ((CANopenDataType) dataType);
							break;
						}
							#endregion

							#region multiple data field data type
						case 9:
						{
							/* A multiple data field object where the data fields may be any
							 * combination of simple variables.  Sub index 0 is of unsigned8
							 * and therefore is not part of the record data.
							 * The data type of each item within the record is held within
							 * the subs definition to indicate it's simple data type.
							 */
							_displayType =  CANopenDataType.RECORD;
							break;
						}
							#endregion

							#region default
						default:
							// A dictionary entry with no data fields. - not yet implemented
							_displayType =  CANopenDataType.NULL;
							break;
							#endregion default
					}
				}
				return _displayType;
			}
			set
			{
				_displayType= value; //here is wher we should merge dataTpye and Object Type into displayType
			}
		}
		#endregion  displayType					

		#region accessLevel
		private byte _accessLevel = 0;
		/// <summary>access level required to read or write to this item (a level is given when the user has logged in) </summary>
		[XmlElement("accessLevel",typeof(byte))]  
		public byte	accessLevel
		{
			get
			{
				return _accessLevel;
			}
			set
			{
				_accessLevel = value;
			}
		}
		#endregion accessLevel								 

		#region sectionType
        // To allow Sevcon special handling and flexibility of these objects moving, 
        // each object (not subs) is given a section type (filtering for display).
        
        private int _sectionType = (int)SevconSectionType.NONE;

        /// <summary>Integer equivalent of sectionTypeString, used by DW. </summary>
        [XmlIgnore]
        public int sectionType
		{
			get
			{
				return _sectionType;
			}
			set
			{
				_sectionType = value;
			}
		}

        private string _sectionTypeString = "NONE";
        /// <summary>
        /// string equivalent of sectionType - used to store to XML file (as int value may change, 
        /// depending on sevconSectionIDList which is dynamically build).
        /// </summary>
        [XmlElement("sectionType", typeof(string))]
        public string sectionTypeString
        {
            get
            {
                return (_sectionTypeString);
			}
            set
            {
                _sectionTypeString = value;
			}
		}
		#endregion sectionType
	
		#region CANopenSectionType
        // each object (not subs) is given a CANopen section type (filtering for display) 
        // for third party devices.

		private CANSectionType _CANopenSectionType  = CANSectionType.NONE;
		[XmlElement("CANopenSectionType",typeof(CANSectionType))]  
		public CANSectionType		CANopenSectionType
		{
			get
			{
				return _CANopenSectionType;
			}
			set
			{
				_CANopenSectionType = value;
			}
		}
		#endregion CANopenSectionType

		#region objectName
		private int _objectName = (int)SevconObjectType.NONE;
        /// <summary>Integer equivalent of objectNameString, used by DW. </summary>
        [XmlIgnore]
        public int objectName
		{
			get
			{
				return _objectName;
			}
			set
			{
				_objectName = value;
			}
		}

        private string _objectNameString = "NONE";
        /// <summary>
        /// string equivalent of sectionType - used to store to XML file (as int value may change, 
        /// depending on sevconObjectIDList which is dynamically build). 
        /// </summary>
        [XmlElement("objectName", typeof(string))]
        public string objectNameString
        {
            get
            {
                return (_objectNameString);
            }
            set
            {
                _objectNameString = value;
            }
        }
		#endregion objectName

		#region format
		private SevconNumberFormat _format = SevconNumberFormat.BASE10;
		/// <summary>display as special string, base 10 or base 16 format</summary>
		[XmlElement("format",typeof(SevconNumberFormat))]  
		public SevconNumberFormat	format
		{
			get
			{
				return _format;
			}
			set
			{
				_format = value;
			}
		}
		#endregion format	

		#region format list
		private string _formatList = "";
		/// <summary>text strings associated with special string format</summary>
		[XmlElement("formatList",typeof(string))]  
		public string				formatList
		{
			get
			{
				return _formatList;
			}
			set
			{
				_formatList = value;
			}
		}
		#endregion format list

		#region invalidItem
		private bool _invalidItem = true;
		/// <summary>item doesn't exist when tried to read from the CAN device</summary>
		[XmlElement("invalidItem",typeof(bool))]  
		public bool	invalidItem
		{
			get
			{
				return _invalidItem;
			}
			set
			{
				_invalidItem = value;
			}
		}
		#endregion invalidItem
		
		#region commsTimeout
		private int _commsTimeout = SCCorpStyle.TimeoutDefault;
		/// <summary>communication timeout to be used with transmissions relating to this object</summary>
		[XmlElement("commsTimeout",typeof(int))]  
		public int	commsTimeout
		{
			get
			{
				return _commsTimeout;
			}
			set
			{
				_commsTimeout = value;
			}
		}
		#endregion commsTimeout

		#region EDSObjectType
		private CANObjectType _EDSObjectType = CANObjectType.Mandatory;
		/// <summary>Section of the EDS that this object was defined in </summary>
		[XmlElement("EDSObjectType",typeof(CANObjectType))]  
		public CANObjectType	EDSObjectType
		{
			get
			{
				return _EDSObjectType;
			}
			set
			{
				_EDSObjectType = value;
			}
		}
		#endregion EDSObjectType

		#region bitSplit
		private BitSplit _bitSplit = null;
		/// <summary>If a bit split display type, how to extract the data for display from 
		/// overall data type </summary>
		[XmlElement("bitSplit",typeof(BitSplit ))]  
		public BitSplit bitSplit
		{
			get
			{
				return _bitSplit;
			}
			set
			{
				_bitSplit = value;  //may need to do for loop here = copy by value?
			}
		}
		#endregion bitSplit

		#region lowLimit
		private long _lowLimit = 0;
		/// <summary>minimum value of this item (only if integer data type)</summary>
		[XmlElement("lowLimit",typeof(long))]  
		public long					lowLimit
		{
			get
			{
				return _lowLimit;
			}
			set
			{
				_lowLimit = value;
			}
		}
		#endregion lowLimit

		#region highLimit
		private long _highLimit = 0;
		/// <summary>maximum value of this item (only if integer data type)</summary>
		[XmlElement("highLimit",typeof(long))]  
		public long	highLimit
		{
			get
			{
				return _highLimit;
			}
			set
			{
				_highLimit = value;
			}
		}
		#endregion highLimit

		#region defualtValue
		private long _defaultValue = 0;
		/// <summary>default value (only if integer data type)</summary>
		[XmlElement("defaultValue",typeof(long))]  
		public long					defaultValue
		{
			get
			{
				return _defaultValue;
			}
			set
			{
				_defaultValue = value;
			}
		}
		internal defaultValType defaultType = defaultValType.NONE;
//		internal bool hasDefault = false;
		#endregion defualtValue

		#region defaultValueString
		private string _defaultValueString = "";
		/// <summary>
		/// 3rd party EDSs sometimes add a default value for non-numeric types
		/// </summary>
		[XmlElement("defaultValueString",typeof(string))]  
		public string				defaultValueString
		{
			get
			{
				return _defaultValueString;
			}
			set
			{
				_defaultValueString = value;
			}
		}
		#endregion defaultValueString

		#region currentValue
		private long _currentValue = 0;
		/// <summary>current value (received from controller or DCF file) if the data type is any of the integer types</summary>
		[XmlIgnore]
		internal long					currentValue
		{
			get
			{
				return _currentValue;
			}
			set
			{
				_currentValue = value;
			}
		}
		#endregion currentValue

		#region currentValueString 
		private string _currentValueString = "";
		/// <summary>current value if data type is any string data type</summary>
		[XmlIgnore]
		internal string	currentValueString
		{
			get
			{
				return _currentValueString;
			}
			set
			{
				_currentValueString = value;
			}
		}
		#endregion currentValueString
		
		#region real32
		private Real32Values		_real32 = null;
		/// <summary>high,low,default and current value if the data type is a real32 value</summary>
		[XmlIgnore]
		internal Real32Values			real32
		{
			get
			{
				return _real32;
			}
			set
			{
				_real32 = value;
			}
		}
		#endregion real32

		#region real64
		private Real64Values  _real64 = null;
		/// <summary>high,low,default and current value if the data type is a real64 value</summary>
		[XmlIgnore]
		internal Real64Values 	real64
		{
			get
			{
				return _real64;
			}
			set
			{
				_real64 = value;
			}
		}
		#endregion real64

		#region currentValueDomain
		//		private byte [] _currentValueDomain = null;
		/// <summary>current value if data is a domain data type</summary>
		[XmlIgnore]
		internal byte[] currentValueDomain = null;  //when you get time encapusltae the reaidn go fa domain within this property jude100206
		//		{
		//			get
		//			{
		//				return _currentValueDomain;
		//			}
		//			set
		//			{
		//				_currentValueDomain = value;
		//			}
		//		}
		#endregion currentValueDomain

		#region eepromString
		[XmlIgnore]
		internal string eepromString = "";
		#endregion eepromString

		#region displayOnMasterOnly
		private bool _displayOnMasterOnly = false;
		/// <summary>denotes whether this item should be displayed on all SEVOCN nodes or on Master node only </summary>
		[XmlElement("displayOnMasterOnly",typeof(bool))]  
		public bool displayOnMasterOnly
		{
			get
			{
				return _displayOnMasterOnly;
			}
			set
			{
				_displayOnMasterOnly = value;
			}
		}
		#endregion displayOnMasterOnly

		#region bool is numItems
		[XmlElement("isNumItems",typeof(bool))]  
		public bool isNumItems = false;
		#endregion bool is numItems
		//note this is too be rplaced by DataPoint below
		#region monitoring dataPoints
		[XmlArrayItem(typeof(PointF))] public ArrayList screendataPoints = new ArrayList();
		#endregion monitoring  dataPoints

		#region monitoring dataPoints
		[XmlArrayItem(typeof(DataPoint))] public ArrayList measuredDataPoints = new ArrayList();
		#endregion monitoring  dataPoints

        [XmlElement("monPDOInfo", typeof(MonitoringPDOInfo))] //need this for calibrated monitoring
		public MonitoringPDOInfo monPDOInfo = null;

        [XmlElement("lastPlottedIndex", typeof(int))]
		public int lastPlottedPtIndex = 0;
	}
	#endregion ODItemData class

	public class MonitoringPDOInfo
	{
		public MonitoringPDOInfo()
		{
		}
		///<summary>start bit in the PDO where this item will reside</summary>
		public int		startBitInPDO;				
		///<summary>bit mask needed to extract this monitor item from the PDO data</summary>
		public long		mask;						 
		///<summary>number of left shifts needed to extract this monitor item from the PDO data</summary>
		public int		shift;	
		public COBObject COB;
	}
}
