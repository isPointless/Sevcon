/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.14$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:09/07/2008 21:27:04$
	$ModDate:01/07/2008 11:50:36$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
    XML class definition for the monitor store file format. It is used for the store/retrieval 
    of a user’s selected list of OD items for monitoring along with any collated data points.
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  64020: MonitorStore.cs 

   Rev 1.14    09/07/2008 21:27:04  ak
 Series legend strings added (hard to recalculate if the node is not
 connected) and dataInFile added, to indicate whether "plot from file" option
 should be enabled


   Rev 1.13    05/12/2007 22:24:44  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Xml.Serialization;  
using System.IO;
using System.Collections;			     

namespace DriveWizard
{
	/// <summary>
	/// Summary description for MonitorFileObject.
	/// </summary>
	[XmlRootAttribute( "DWMonitorStore", Namespace="", IsNullable=false )]
	public class myMonitorStore
	{
		public myMonitorStore()
		{
			graph = new MNgraphformat();
		}
		[XmlElement("graph",typeof(MNgraphformat))]
		public MNgraphformat graph;
        //serializes monitor data legends as hard to recalculate if node is offline when file reopened
        [XmlArray("seriesLegends"), XmlArrayItem("seriesLegend", typeof(string))]
        public ArrayList myLegends = new ArrayList();

		// Serializes an ArrayList as a "MonitoredNodes" array of XML elements of custom type VendorIDType named "vendors".
		//items in quotation marks are takes verbatim from the XML file
		[XmlArray( "monitorednodes" ), XmlArrayItem( "monitorednode", typeof( nodeInfo ) )]
		public ArrayList myMonNodes = new ArrayList();
		[XmlIgnore]
		internal bool existsInCurrentSystem = true;
		[XmlIgnore]
		internal string filename = "";
		[XmlIgnore]
		internal bool fromFile = false;
        [XmlIgnore]
        internal bool dataInFile = false;   //true if any plot data is in the stored file
	}

	public class MNgraphformat
	{
		public MNgraphformat() 
		{
			Yaxis = new MNAxis();
			Yaxis.AxisLabel = "Parameter value";
			Xaxis = new MNAxis();
		}
		[XmlAttribute]
		public string MainTitle = "Untitled";

		[XmlAttribute]
		public bool ShowDataMarkers = true;

		[XmlElement("XAxis",typeof(MNAxis))]
		public MNAxis Xaxis;

		[XmlElement("YAxis",typeof(MNAxis))]
		public MNAxis Yaxis;
	}
	public class MNAxis
	{
		public MNAxis()
		{
		}
		private string _AxisLabel = "Elapsed Time /s";
		[XmlAttribute]
		public string AxisLabel
		{
			get
			{
				return _AxisLabel;
			}
			set
			{
				_AxisLabel = value;
			}
		}
		private float _Max = 10F;
		[XmlAttribute]
		public float Max
		{
			get
			{
				return _Max;
			}
			set
			{
				_Max = value;
			}
		}

		private float _Min = 0F;
		[XmlAttribute]
		public float Min
		{
			get
			{
					return _Min;
			}
			set
			{
				_Min = value;
			}
		}
		private float _DivValue = 1F; //should never be zero to prevent divide by zero possibility
		[XmlAttribute]
		public float DivValue
		{
			get
			{
				if(_DivValue == 0)
				{
					_DivValue =1F;  //this prevents us ever dividing by zero
				}
				return _DivValue;
			}
			set
			{
				_DivValue = value;
			}
		}

		private bool _LinesVisible = true;
		[XmlAttribute]
		public bool LinesVisible
		{
			get
			{
return _LinesVisible;
			}
			set
			{
				_LinesVisible = value;
			}
		}
	}

}

