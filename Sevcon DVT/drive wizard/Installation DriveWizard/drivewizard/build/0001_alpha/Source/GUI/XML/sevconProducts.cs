/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.0$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:29/09/2008 21:12:58$
	$ModDate:15/07/2008 20:36:56$

ORIGINAL AUTHOR
    Alison King

DESCRIPTION
    XML class definition for the sevcon products description file format.
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  144894: sevconProducts.cs 

   Rev 1.0    29/09/2008 21:12:58  ak
 Original, product range and variant text descriptions now read in from XML
 file instead of defined as const strings



*******************************************************************************/
using System;
using System.Xml.Serialization;  
using System.IO;
using System.Collections;			     

namespace DriveWizard
{
	/// <summary>
	/// Summary description for sevconProducts descriptions.
	/// </summary>
	[XmlRootAttribute( "DWSevconProducts", Namespace="", IsNullable=false )]
    public class SevconProductDescriptions
	{
        public SevconProductDescriptions()
        {
        }

        [XmlArray("sevconProductRanges"), XmlArrayItem("ProductRange", typeof(SevconProductRange))]
        public ArrayList sevconProductRanges = new ArrayList();
	}

	public class SevconProductRange
	{
        public SevconProductRange()
        {
            productRange = "Unknown";
            productRangeID = 0;
        }

        public SevconProductRange(string name, int id)
        {
            productRange = name;
            productRangeID = id;
        }

		[XmlElement("Range",typeof(string))]
		public string productRange;

        [XmlElement("RangeID", typeof(int))]
        public int productRangeID;

        [XmlArray("sevconProductVariants"), XmlArrayItem("ProductVariant", typeof(SevconProductVariant))]
        public ArrayList sevconProductVariants = new ArrayList();
	}

    public class SevconProductVariant
    {
        public SevconProductVariant()
        {
            productVariant = "Unknown";
            productVariantID = 0;
        }
        public SevconProductVariant(string name, int id)
        {
            productVariant = name;
            productVariantID = id;
        }

        [XmlElement("Variant", typeof(string))]
        public string productVariant;

        [XmlElement("VariantID", typeof(int))]
        public int productVariantID;
    }
}

