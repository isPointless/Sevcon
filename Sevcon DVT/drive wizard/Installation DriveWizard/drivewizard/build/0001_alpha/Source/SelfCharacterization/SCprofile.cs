/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.3$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:28:30$
	$ModDate:05/12/2007 22:27:06$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  64405: SCprofile.cs 

   Rev 1.3    05/12/2007 22:28:30  ak
 TC keywords added to source for version control.



*******************************************************************************/
using System;
using System.Xml.Serialization; 
using System.Drawing;
using System.Collections;

namespace DriveWizard
{
	/// <summary>
	/// Summary description for SCprofile.
	/// </summary>
	[XmlRootAttribute( "SCProfile", Namespace="", IsNullable=false )]
	public class SCprofile
	{
		#region SCprofile
		public SCprofile()
		{
			general = new SCProf_gen();
			openloop = new SCProf_OpenLoop();
			closedloop = new SCProf_ClosedLoop();
			noloadtest = new SCProf_NoLoad();
			powerframe = new SCProf_PowerFrame();
		}
		public void applyDefaults(byte passed_productVoltage, byte passed_productCurren)
		{
			this.openloop.testPoints = new ArrayList();
			PointF pt = new PointF(12,0.33F);//OL.Vd1 = 0.33,OL.Ns1 = 12
			this.openloop.testPoints.Add(pt);
			pt = new PointF(90,0);//OL.Vd2 = 0.0,OL.Ns2 = 90
			this.openloop.testPoints.Add(pt);
			pt = new PointF(12,-0.33F); //OL.Vd3 = -0.33, OL.Ns3 = 12
			this.openloop.testPoints.Add(pt);
			pt = new PointF(120, 0);    //OL.Vd4 = 0.0, OL.Ns4 = 120
			this.openloop.testPoints.Add(pt);

			this.closedloop.CLtests = new ArrayList();
			#region CLA
			SCProf_ClosedLoop.SCProf_CLtest CLA = new SCProf_ClosedLoop.SCProf_CLtest();
			CLA.testName = "CLA";
			pt = new PointF(0.33F,3000F);
			CLA.testpoints.Add(pt);
			pt = new PointF(-0.33F,3000F);
			CLA.testpoints.Add(pt);
			this.closedloop.CLtests.Add(CLA);
			#endregion CLA

			#region  CLB
			SCProf_ClosedLoop.SCProf_CLtest CLB = new SCProf_ClosedLoop.SCProf_CLtest();
			CLB.testName = "CLB";
			pt = new PointF(-0.32F,2000F);
			CLB.testpoints.Add(pt);
			pt = new PointF(0.32F,3000F);
			CLB.testpoints.Add(pt);
			this.closedloop.CLtests.Add(CLB);
			#endregion CLB

			this.powerframe.nomBattVolts = System.Convert.ToUInt16( passed_productVoltage);
			this.powerframe.minBattVolts = (ushort) Math.Floor(this.powerframe.nomBattVolts * 0.55);
			this.powerframe.maxBattVolts = (ushort) Math.Floor(this.powerframe.nomBattVolts * 1.48);
			float tmp = 0.3F;
			this.noloadtest.speedpoints = new ArrayList();
			this.noloadtest.speedpoints.Add(tmp); //NLT.ws1 = 0.3
			tmp = 0.5F;
			this.noloadtest.speedpoints.Add(tmp); //NLT.ws2 = 0.5
			tmp = 0.7F;
			this.noloadtest.speedpoints.Add(tmp); //NLT.ws3 = 0.7
			tmp = 1.0F;
			this.noloadtest.speedpoints.Add(tmp); //NLT.ws4 = 1.0
		}
		//only load these parameters from DW values
		//allows up to reload with correct defualts
		//on user request
		[XmlIgnore]
		public byte productVoltage;

		[XmlIgnore]
		public byte productCurrent;

		[XmlElement("general",typeof(SCProf_gen))]
		public SCProf_gen general;

		[XmlElement("openloop",typeof(SCProf_OpenLoop))]
		public SCProf_OpenLoop openloop;

		[XmlElement("closedloop",typeof(SCProf_ClosedLoop))]
		public SCProf_ClosedLoop closedloop;

		[XmlElement("noloadtest",typeof(SCProf_NoLoad))]
		public SCProf_NoLoad noloadtest;

		[XmlElement("powerframe",typeof(SCProf_PowerFrame))]
		public SCProf_PowerFrame powerframe;

		public void applyVoltageOffSets( double Vs_Rated)
		{
			for(int i = 0; i< this.openloop.testPoints.Count;i++)
			{
					PointF myPt = (PointF) this.openloop.testPoints[i];
					myPt.Y = (float) (myPt.Y * Vs_Rated);
					this.openloop.testPoints[i] = myPt;
			}
		}
		#endregion SCprofile
	}
	public class SCProf_gen
	{
		#region general
		public SCProf_gen()
		{
		}
		#region maxSpeedRatio
		private ushort _maxSpeedRatio = 2;
		[XmlAttribute]
		public ushort maxSpeedRatio
		{
			get
			{
				return _maxSpeedRatio;
			}
			set
			{
				_maxSpeedRatio = value;
			}
		}
		#endregion maxSpeedRatio

		#region maxTorqueRatio
		private ushort _maxTorqueRatio = 2;
		//note we can only serialize public properties and fields
		[XmlAttribute]
		public ushort maxTorqueRatio
		{
			get
			{
				return _maxTorqueRatio;
			}
			set
			{
				_maxTorqueRatio = value;
			}
		}
		#endregion maxTorqueRatio

		#region maxPowerRatio
		private ushort _maxPowerRatio = 1;
		[XmlAttribute]
		public ushort maxPowerRatio
		{
			get
			{
				return _maxPowerRatio;
			}
			set
			{
				_maxPowerRatio = value;
			}
		}
		#endregion maxPowerRatio
		#endregion general
	}
	public class SCProf_OpenLoop
	{
		#region Open Loop
		public SCProf_OpenLoop()
		{
			
		}

		private ushort _percentOverCurrent = 20; //express as numeric percent
		[XmlAttribute]
		public ushort percentOverCurrent
		{
			get
			{
				return _percentOverCurrent;
			}
			set
			{
				_percentOverCurrent = value;
			}
		}
		#region number of test iterations
		private ushort _numTestIterations = 2;
		[XmlAttribute]
		public ushort numTestIterations
		{
			get
			{
				return _numTestIterations;
			}
			set
			{
				_numTestIterations = value;
			}
		}
		#endregion number of test iterations

		#region number of times DSP should apply datapoints
		private ushort _numTestPointApplications = 3;
		[XmlAttribute]
		public ushort numTestPointApplications
		{
			get
			{
				return _numTestPointApplications;
			}
			set
			{
				_numTestPointApplications = value;
			}
		}
		#endregion number of times DSP should apply datapoints

		#region test points
		[XmlArray( "testPts" ), XmlArrayItem("testPt", typeof(PointF))] 
		public ArrayList testPoints = new ArrayList();
		#endregion test poitns
		#endregion Open Loop
	}
	public class SCProf_ClosedLoop
	{
		#region closed loop
		public SCProf_ClosedLoop()
		{
		}
	
		[XmlArray( "cltests" ), XmlArrayItem("cltest" ,typeof(SCProf_CLtest))] 
		public ArrayList CLtests = new ArrayList();
		
		public class SCProf_CLtest
		{
			public SCProf_CLtest()
			{
			}
			[XmlAttribute]
			public string testName;

			[XmlArray( "testPts" ), XmlArrayItem("testPt", typeof(PointF))] 
			public ArrayList testpoints = new ArrayList();
		}
		#endregion closed loop
	}
	public class SCProf_PowerFrame
	{
		#region power frame
		#region constructor for Xml Read
		public SCProf_PowerFrame()
		{
		}
		#endregion constructor for Xml Read

		#region minimum battery volts
		private ushort _minBattVolts = 21;//		Batt.Vb_min = 21 
		[XmlAttribute]
		public ushort minBattVolts
		{
			get
			{
				return _minBattVolts;
			}
			set
			{
				_minBattVolts = value;
			}
		}
		#endregion minimum battery volts

		#region nominal Battery volts
		private ushort _nomBattVolts = 48; //		Batt.Vb_nom = 48 
		[XmlAttribute]
		public ushort nomBattVolts
		{
			get
			{
				return _nomBattVolts;
			}
			set
			{
				_nomBattVolts = value;
			}
		}
		#endregion nominal Battery volts

		#region max Battery Volts
		private ushort _maxBattVolts = 69; //		Batt.Vb_max = 69
		[XmlAttribute]
		public ushort maxBattVolts 
		{
			get
			{
				return _maxBattVolts;
			}
			set
			{
				_maxBattVolts = value;
			}
		}
		#endregion max Battery Volts

		#region maxIs
		private ushort _maxIs = 300;	  //		PFrame.Is_max = 300
		[XmlAttribute]
		public ushort maxIs
		{
			get
			{
				return _maxIs;
			}
			set
			{
				_maxIs = value;
			}
		}
		#endregion maxIs
		#endregion power frame
	}
	public class SCProf_NoLoad
	{
		#region No Load Test
		#region constructor for loading from Xml
		public SCProf_NoLoad()
		{
		}

		#endregion constructor for loading from Xml
		#region constructor for loading defualt internal vlaues
		public SCProf_NoLoad(string throwaway)
		{
			float tmp = 0.3F;
			speedpoints.Add(tmp); //NLT.ws1 = 0.3
			tmp = 0.5F;
			speedpoints.Add(tmp); //NLT.ws2 = 0.5
			tmp = 0.7F;
			speedpoints.Add(tmp); //NLT.ws3 = 0.7
			tmp = 1.0F;
			speedpoints.Add(tmp); //NLT.ws4 = 1.0
		}
		#endregion constructor for loading defualt internal vlaues

		#region w_rate
		private float _w_rate = 0.15F;   //NLT.w_rate = 0.15
		[XmlAttribute]
		public float w_rate
		{
			get
			{
				return _w_rate;
			}
			set
			{
				_w_rate = value;
			}
		}
		#endregion w_rate

		#region num samples
		private ushort _numSamples = 2000;  //NLT.nsamples = 2000
		[XmlAttribute]
		public ushort numSamples
		{
			get
			{
				return _numSamples;
			}
			set
			{
				_numSamples = value;
			}
		}
		#endregion num samples

		#region number of Samples required For Flux Change Settle
		private ushort _numSamplesForFluxChangeSettle = 5000; //NLT.settle1 = 5000
		[XmlAttribute]
		public ushort numSamplesForFluxChangeSettle
		{
			get
			{
				return _numSamplesForFluxChangeSettle;
			}
			set
			{
				_numSamplesForFluxChangeSettle = value;
			}
		}
		#endregion number of Samples required For Flux Change Settle

		#region  number of Samples required For Steady State Settle
		private ushort _numSamplesForSteadyStateSettle = 4000; //NLT.settle2 = 4000
		[XmlAttribute]
		public ushort numSamplesForSteadyStateSettle
		{
			get
			{
				return _numSamplesForSteadyStateSettle;
			}
			set
			{
				_numSamplesForSteadyStateSettle =value;
			}
		}
		#endregion  number of Samples required For Steady State Settle

		#region Max Id to Iq ratio
		private float _maxIdIqRatio = 0.6F;  //NLT.maxIdq_ratio = 0.6
		[XmlAttribute]
		public float maxIdIqRatio
		{
			get
			{
				return _maxIdIqRatio;
			}
			set
			{
				_maxIdIqRatio = value;
			}
		}
		#endregion Max Id to Iq ratio

		#region speed points
		[XmlArray( "speedpts" ), XmlArrayItem("speedpt" ,typeof(float))] 
		public ArrayList speedpoints = new ArrayList();
		#endregion speed points
		#endregion No Load Test
	}
}
