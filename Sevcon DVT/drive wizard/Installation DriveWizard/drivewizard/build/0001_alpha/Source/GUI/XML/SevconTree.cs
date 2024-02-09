/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.4$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:24:44$
	$ModDate:05/12/2007 22:16:10$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
    SevconTree class definition, and corresponding sub-class definitions of
    XMLTreeLevel, VendorIDType, ProductCodeType and revisionNumberType.
    
REFERENCES    

MODIFICATION HISTORY
    $Log:  105643: SevconTree.cs 

   Rev 1.4    05/12/2007 22:24:44  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Xml.Serialization;  
using System.IO;		
using System.Collections;
using System.Windows.Forms;	     

namespace DriveWizard
{
	/// <summary>
	/// </summary>	
	//Set this 'SevconTree' class as the root node of any XML file its serialized to.
	[XmlRootAttribute( "DWTree", Namespace="", IsNullable=false )]
	public class SevconTree
	{
		/// <summary>
		/// Default constructor for this class (required for serialization).
		/// </summary>
		public SevconTree()
		{
		}

		// Set this 'DateTimeValue' field to be an attribute of the root node.
		[XmlAttributeAttribute( DataType="date" )]
		public System.DateTime DateTimeValue;

		[XmlElement]
		public XMLTreeLevel treeStruct = new XMLTreeLevel();

		// Serializes an ArrayList as a "vendors" array of XML elements of custom type VendorIDType named "vendors".
		[XmlArray( "VendorIDs" ), XmlArrayItem( "vendorID", typeof( VendorIDType ) )]
		public ArrayList vendors = new ArrayList();

	}
	/// <summary>
	/// Custom class used to store a tree view containg up to six levels.
	/// </summary>	
	// Mark class as serializable.
	[Serializable]
	public class XMLTreeLevel
	{
		#region heading 1 class
		/// <summary>
		/// Default constructor for this class (required for serialization).
		/// </summary>
		public XMLTreeLevel()
		{
		}
		// Specify that this field should be serialized as an XML attribute 
		// instead of an element to demonstrate the formatting differences in an XML file. 
		[XmlAttribute]
		public string Title = "";
		// Serializes an ArrayList as a "HeadingsLevel2" array of XML elements of custom type Heading2Type named "HeadingsLevel2".
		[XmlArray( "XMLSections" ), XmlArrayItem( "XMLSection", typeof( XMLTreeLevel ) )]
		public System.Collections.ArrayList nextLevelAL;
		#endregion heading 1 class
	}
	public class VendorIDType
	{
		#region Vendor ID class
		/// <summary>
		/// Default constructor for this class (required for serialization).
		/// </summary>
		public VendorIDType()
		{
		}

		[XmlElement]
		public string vendorNoValue;

		// Serializes an ArrayList as a "HeadingsLevel2" array of XML elements of custom type Heading2Type named "HeadingsLevel2".
		[XmlArray( "productCodeGroup" ), XmlArrayItem( "productCode", typeof( ProductCodeType ) )]
		public System.Collections.ArrayList productCodes = new System.Collections.ArrayList();
		#endregion Vendor ID class
	}

	public class ProductCodeType
	{
		#region Product Codes class
		/// <summary>
		/// Default constructor for this class (required for serialization).
		/// </summary>
		public ProductCodeType()
		{
		}
		[XmlElement]
		public string prodCodeValue;

		// Serializes an ArrayList as a "HeadingsLevel3" array of XML elements of custom type Heading3Type named "HeadingsLevel3".
		[XmlArray( "revsionNumberGroup" ), XmlArrayItem( "revisionNumber", typeof( revisionNumberType ) )]
		public System.Collections.ArrayList revisionNumbers = new System.Collections.ArrayList();
		#endregion Product Codes class
	}

	public class revisionNumberType
	{
		#region rev NO
		/// <summary>
		/// Default constructor for this class (required for serialization).
		/// </summary>
		public revisionNumberType()
		{
		}
		[XmlElement]
		public string revNoValue;
		#endregion rev NO
	}
}
