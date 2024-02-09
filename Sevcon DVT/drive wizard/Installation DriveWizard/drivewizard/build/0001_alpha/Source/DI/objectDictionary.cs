/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.82$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 21:13:46$
	$ModDate:05/12/2007 21:11:36$

ORIGINAL AUTHOR
    Alison King

DESCRIPTION
	Object Dictionary class.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36697: objectDictionary.cs 

   Rev 1.82    05/12/2007 21:13:46  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using System.Xml.Serialization;  

using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace DriveWizard
{
	/// <summary>
	/// Each instance of the objectDictionary class is aggregated within the nodeInfo class instance.
	/// The object dictionary constructs and contains the replica copy of the object dictionary for
	/// the physical device found on the CAN system, constructed from the matching EDS file.
	/// All communications with the physical device to read or write specific OD items is performed
	/// via the object dictionary.
	/// </summary>
	public class objectDictionary
	{


		public nodeInfo CANnode;
	
		#region constructor and destructor & create OD
		//-------------------------------------------------------------------------
		//  Name			: constructor
		//  Description     : Performs any specific initialisation required when
		//					  an object instance is created.  None currently required.
		//  Parameters      : None
		//  Used Variables  : None
		//  Preconditions   : An objectDictionary object is being instantiated.
		//  Post-conditions : None
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>Called to instantiate an instance of the objectDictionary class.</summary>
		public objectDictionary(nodeInfo node)
		{
			this.CANnode = node;
			// no constructor logic required
		}

		//-------------------------------------------------------------------------
		//  Name			: destructor
		//  Description     : A objectDictionary object is being destructed.
		//  Parameters      : None
		//  Used Variables  : None
		//  Preconditions   : An objectDictionary object is being destructed.
		//  Post-conditions : None
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>Called to destruct an instance of the objectDictionary class.</summary>
		#endregion



	}
}
