/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.14$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/09/2008 23:15:34$
	$ModDate:22/09/2008 22:59:56$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	Vehicle Profile class definition, along with sub-class definitions of
    VPLogin, VPBaud, VPCanNode, VPMotor, VPNamePlateData, VPMotorLimits and
    VPLineContactor.

REFERENCES    

MODIFICATION HISTORY
    $Log:  51845: VehicleProfile.cs 

   Rev 1.14    23/09/2008 23:15:34  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.13    05/12/2007 21:26:42  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.Collections;

namespace DriveWizard
{
	/// <summary>
	/// Summary description for VehicleProfile.
	/// </summary>
	#region Vehicle Profile Class
	[XmlRootAttribute( "vehicleProfile", Namespace="", IsNullable=false )]
	public class VehicleProfile
	{
		public VehicleProfile()
		{
			login = new VPLogin();
			baud = new VPBaud();
		}

		#region Profile Name property
		// Set serialization to IGNORE this field (ie. not add it to the XML).
		private string _ProfileName;
		[XmlIgnore]
		public string ProfileName
		{
			get
			{
				return this._ProfileName;
			}
			//no set method 
			//profile name is automatically determenied 
			//when the profile path is set
		}
		#endregion Profile Name property

		#region ProfilePath property
		private  string			_ProfilePath;
		///<summary>the user selected baud rate</summary>
		// Set serialization to IGNORE this field (ie. not add it to the XML).
		[XmlIgnore]
		public  string			ProfilePath
		{
			get
			{
				return ( _ProfilePath );
			}
			set
			{
				_ProfilePath = value;
				string filepath = (string) _ProfilePath;
				int fileNamestart = filepath.LastIndexOf(@"\");
				this._ProfileName = 	 filepath.Substring(fileNamestart+ 1);
				this._ProfileName = 	 _ProfileName.TrimEnd(".xml".ToCharArray());
			}
		}
		#endregion ProfilePath property

		#region Connection and Login related properties
		[XmlIgnore]
		public string XMLFilePath;

		[XmlIgnore]
		public bool connectAsVirtual = false;

        //DR38000263 "show master objects on slave" check box on Options window saved to profile
        [XmlElement]
        public bool showMasterObjectsOnSlave = false;

		// 
		[XmlElement("login",typeof(VPLogin))]
		public VPLogin login;

		// 
		[XmlElement("baud",typeof(VPBaud))]
		public VPBaud baud;

		// 
		[XmlArray ("cannodes"), XmlArrayItem("cannode", typeof(VPCanNode))]
		public ArrayList myCANNodes = new ArrayList();
		#endregion Connection and Login related proerties

		#region System cofiguration related
		private bool _displayIsSlave = false;
		[XmlAttribute ]
		public bool displayIsSlave
		{
			get
			{
				return _displayIsSlave;
			}
			set
			{
				_displayIsSlave = value;
			}
		}
		#endregion System cofiguration related

		#region Self characterisation relatedproperties
		[XmlArray ("motors"), XmlArrayItem("motor", typeof(VPMotor))]
		public ArrayList connectedMotors = new ArrayList();

		[XmlArray ("lineContactors"), XmlArrayItem("lineContactor", typeof(VPLineContactor))]
		public ArrayList lineContactors = new ArrayList();

		#endregion Self characterisation relatedproperties

	}
	#endregion Vehicle Profile CLass

	#region sub-classes
	public class VPLogin
	{
		#region login
		public VPLogin()
		{
		}

		[XmlAttribute]
		public string userid = "";

		[XmlAttribute]
		public string password = "";
		#endregion login
	}

	public class VPBaud
	{
		#region baud
		public VPBaud()
		{
		}
		[XmlIgnoreAttribute]
		public BaudRate baudrate = BaudRate._1M;

		[XmlAttribute]
		public string rate = "_1M";

		[XmlAttribute]
		public string rateString= "1 MHz"; 
		[XmlAttribute]
		public bool searchAll = true;  //default
		#endregion baud
	}
	public class VPCanNode
	{
		#region CANnode
		public VPCanNode()
		{
		}
		[XmlAttribute]
		public string EDSorDCFfilepath="";
		[XmlAttribute]
		public uint vendorid;
		[XmlAttribute]
		public uint productcode; 
		[XmlAttribute]
		public uint nodeid;
		[XmlAttribute]
		public uint revisionnumber;
		[XmlAttribute]
		public bool master; 
		#endregion CANnode
	}

	public class VPMotor
	{
		#region motor
		public VPMotor()
		{
			platedata = new VPNamePlateData();
			motorLimits = new VPMotorLimits();
		}
		[XmlElement("platedata",typeof(VPNamePlateData))]
		public VPNamePlateData platedata;

		[XmlElement("motorLimits",typeof(VPMotorLimits))]
		public VPMotorLimits motorLimits;

		#region Test profile File
		private string _testProfileFilepath = "";
		[XmlAttributeAttribute]
		public string testProfileFilepath
		{
			get
			{
				return _testProfileFilepath;
			}
			set
			{
				_testProfileFilepath = value;
			}
		}
		#endregion Test profile File

		#region Power frame profile file
		private string _powerframeProfileFilepath = "";
		[XmlAttributeAttribute]
		public string powerframeProfileFilepath
		{
			get
			{
				return _powerframeProfileFilepath;
			}
			set
			{
				_powerframeProfileFilepath = value;
			}
		}
		#endregion Power frame profile file
		#endregion motor
	}

	public class VPNamePlateData
	{
		#region name plate data
		public VPNamePlateData()
		{
		}

		private bool _encoderInvert = false;
		[XmlAttribute ]
		public bool encoderInvert
		{
			get
			{
				return _encoderInvert;
			}
			set
			{
				_encoderInvert = value;
			}
		}

		#region encoder pulses per rev
		private ushort _encoderPulsesPerRev = 80;
		[XmlAttributeAttribute]
		public ushort encoderPulsesPerRev 
		{
			get
			{
				return _encoderPulsesPerRev;
			}
			set
			{
				_encoderPulsesPerRev = value;
			}
		}
		#endregion encoder pulses per rev

		#region rated line voltages
		private ushort _ratedLineVoltageRMS = 34;
		[XmlAttributeAttribute]
		public ushort ratedLineVoltageRMS 
		{
			get
			{
				return _ratedLineVoltageRMS;
			}
			set
			{
				_ratedLineVoltageRMS = value;
			}
		}
		#endregion rated line voltages

		#region rated machanical speed
		private ushort _ratedSpeedMechanicalRPM = 3372;
		[XmlAttributeAttribute]
		public ushort ratedSpeedMechanicalRPM 
		{
			get
			{
				return _ratedSpeedMechanicalRPM;
			}
			set
			{
				_ratedSpeedMechanicalRPM = value;
			}
		}
		#endregion rated machanical speed

		#region rated power
		private ushort _ratedPowerkW = 48;
		[XmlAttributeAttribute]
		public ushort ratedPowerkW 
		{
			get
			{
				return _ratedPowerkW;
			}
			set
			{
				_ratedPowerkW = value;
			}
		}
		#endregion rated power

		#region No of pole pairs
		private ushort _numOfPolePairs = 2;
		[XmlAttributeAttribute]
		public ushort numOfPolePairs 
		{
			get
			{
				return _numOfPolePairs;
			}
			set
			{
				_numOfPolePairs = value;
			}
		}

		#endregion No of pole pairs

		#region rated phase current
		private ushort _ratedPhaseCurrentrmsA = 110;
		[XmlAttributeAttribute]
		public ushort ratedPhaseCurrentrmsA 
		{
			get
			{
				return _ratedPhaseCurrentrmsA;
			}
			set
			{
				_ratedPhaseCurrentrmsA = value;
			}
		}

		#endregion rated phase current

		#region rated electical frequency
		private ushort _ratedFrequencyElectricalHz = 115;
		[XmlAttributeAttribute]
		public ushort ratedFrequencyElectricalHz 
		{
			get
			{
				return _ratedFrequencyElectricalHz;
			}
			set
			{
				_ratedFrequencyElectricalHz = value;
			}
		}

		#endregion rated electical frequency

		#region rated power factor
		private float _ratedPowerFactor = 0.86F;
		[XmlAttributeAttribute]
		public float ratedPowerFactor 
		{
			get
			{
				return _ratedPowerFactor;
			}
			set
			{
				_ratedPowerFactor = value;
			}
		}

		#endregion rated power factor
		#endregion name plate data
	}

	public class VPMotorLimits
	{
		#region motor limits
		public VPMotorLimits()
		{
		}

		#region max speed
		private ushort _maxSpeedrpm = 5000;
		[XmlAttributeAttribute]
		public ushort maxSpeedrpm
		{
			get
			{
				return _maxSpeedrpm;
			}
			set
			{
				_maxSpeedrpm = value;
			}
		}
		#endregion max speed

		#region max torque
		private ushort _maxTorqueNm = 100;
		[XmlAttributeAttribute]
		public ushort maxTorqueNm
		{
			get
			{
				return _maxTorqueNm;
			}
			set
			{
				_maxTorqueNm = value;
			}
		}
		#endregion max torque
		#endregion motor limits
	}
	public class VPLineContactor
	{
		#region line contactor
		public VPLineContactor()
		{
		}
		#region pull in Volts
		private ushort _pullInVolts = 48;
		[XmlAttributeAttribute]
		public ushort pullInVolts
		{
			get
			{
				return _pullInVolts;
			}
			set
			{
				_pullInVolts = value;
			}
		}
		#endregion pull in Volts

		#region pull in time
		private ushort _pullInms = 1000;
		[XmlAttributeAttribute]
		public ushort pullInms
		{
			get
			{
				return _pullInms;
			}
			set
			{
				_pullInms = value;
			}
		}
		#endregion pull in time

		#region hold volts
		private ushort _holdVolts = 36;
		[XmlAttributeAttribute]
		public ushort holdVolts
		{
			get
			{
				return _holdVolts;
			}
			set
			{
				_holdVolts = value;
			}
		}
		#endregion hold volts

		#region LC output
		//Calculated entirely by Drive WIzard once the Node ID(s) for line contactors are known
		[XmlIgnoreAttribute]
		public ushort LCoutput = 0;
		#endregion LC output

		#region node ID controlling this line contactor
		//calculted by Drive Wizard and 'confirmed' by user.
		private string _nodeID = "Node 0";
		[XmlAttributeAttribute]
		public string nodeID
		{
			get
			{
				return _nodeID;
			}
			set
			{
				_nodeID = value;
			}
		}
		#endregion node ID controlling this line contactor

		#endregion line contactor
	}
	#endregion sub-classes
}
