/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.33$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 21:13:48$
	$ModDate:05/12/2007 21:08:30$

ORIGINAL AUTHOR
    Alison King

DESCRIPTION
	This class provides the functionality to read a
    CANopen compliant DCF file and extract the object
    descriptions and parameter values into the EDSObjectInfo 
    data structure which is a suitable format for DW to build 
    the replica of the object dictionary.
    This class also allows a DCF file to be generated from
    an EDS after the device's entire object dictionary
    has been read.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36687: DCFFile.cs 

   Rev 1.33    05/12/2007 21:13:48  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DriveWizard
{
	/// <summary>This class provides the functionality to read a CANopen compliant DCF 
	/// file and extract the object descriptions and parameter values into the 
	/// EDSObjectInfo data structure which is a suitable format for DW to build 
	/// the replica of the object dictionary. This class also allows a DCF file to be 
	/// generated from an EDS after the device's entire object dictionary has been read.
	/// </summary>
	public class DCFFile : EDSFile
	{
		//-------------------------------------------------------------------------
		//  Name			: DCFFile()
		//  Description     : Constructor
		//  Parameters      : None
		//  Used Variables  : None
		//  Preconditions   : Called when a new instance of DCFFile is created.
		//  Post-conditions : None
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Creates a new instance of the DCFFile class and performs specific initialisation.</summary>
		public DCFFile()
		{
		}

	}
}
