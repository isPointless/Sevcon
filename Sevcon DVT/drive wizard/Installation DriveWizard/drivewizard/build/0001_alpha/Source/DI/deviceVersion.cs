/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.16$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 21:13:46$
	$ModDate:05/12/2007 21:09:24$

ORIGINAL AUTHOR
    Alison King

DESCRIPTION
	Device Version class.
    Reads the mandatory object 0x1018 and it's subs from a
    physical device connected via the CAN to ascertain it's
    vendor ID, product code and revision code.  These are
    then stored as properties.
    (Needed to find a matching EDS file).

REFERENCES    

MODIFICATION HISTORY
    $Log:  36689: deviceVersion.cs 

   Rev 1.16    05/12/2007 21:13:46  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;

namespace DriveWizard
{

	/// <summary>
	/// Reads the mandatory object 0x1018 and it's subs from a physical device connected 
	/// via the CAN to ascertain it's vendor ID, product code and revision code.  These 
	/// are then stored as properties. (Needed to find a matching EDS file).	
	/// </summary>
	public class deviceVersion
	{
		#region property definitions
		/* Vendor ID, used to check whether it is a Sevcon node or a 3rd party.
		 * This is read from the controller and stored in this object with all
		 * other objects only having read access.
		 */
		private uint	_vendorID = 0xFFFFFFFF;
		///<summary>Vendor specific identifier (read from object 0x1018)</summary>
		public uint		vendorID
		{
			get
			{
				return ( _vendorID );
			}

			set
			{
				_vendorID = value;
			}
		}

		/* Product code, used to check whether DW has a matching EDS file available.
		 * This is read from the controller and stored in this object with all
		 * other objects only having read access.
		 */
		private uint	_productCode = 0xFFFFFFFF;
		///<summary>Unique product code (read from object 0x1018)</summary>
		public uint		productCode
		{
			get
			{
				return ( _productCode );
			}

			set
			{
				_productCode = value;
			}
		}

		/* Revision number, again used to check whether DW has a matching EDS file available.
		 * This is read from the controller and stored in this object with all
		 * other objects only having read access.
		 */
		private uint	_revisionNumber = 0xFFFFFFFF;
		///<summary>Product revision number (from object 0x1018)</summary>
		public uint		revisionNumber
		{
			get
			{
				return ( _revisionNumber );
			}

			set
			{
				_revisionNumber = value;
			}
		}

		///<summary>Product variant number (from object 0x1018)</summary>
		public byte		productVariant
		{
			get
			{
				byte code = 0x00;
				if(_productCode != 0xFFFFFFFF)  //we need to exclude unknown ( and thus defualt) product code
				{
					uint intCode = _productCode & 0x00ff0000;
					intCode = intCode >> 16;
					code = (byte)intCode;
				}
				return ( code );
			}
		}

		///<summary>Product range number (from object 0x1018)</summary>
		public byte		productRange
		{
			get
			{
				byte code = 0x00;
					uint intCode = _productCode & 0xff000000;
					intCode = intCode >> 24;
					code = (byte)intCode;
				return ( code );
			}
		}

		///<summary>Product voltage number (from object 0x1018)</summary>
		public byte		productVoltage
		{
			get
			{
				byte code = 0x00;
				uint intCode = _productCode & 0x0000ff00;
				intCode = intCode >> 8;
				code = (byte)intCode;

				return ( code );
			}
		}

		///<summary>Product current number (from object 0x1018)</summary>
		public byte		productCurrent
		{
			get
			{
				byte code = 0x00;
				uint intCode = _productCode & 0x000000ff;
				code = (byte)intCode;

				return ( code );
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
		#endregion

		#region constructor, destructor etc
		//-------------------------------------------------------------------------
		//  Name			: constructor
		//  Description     : Handles any object specific initialisation required
		//					  when an object of this type is instantiated. None
		//					  is currently required.
		//  Parameters      : None
		//  Used Variables  : None
		//  Preconditions   : An object of this type is being instantiated.
		//  Post-conditions : None
		//  Return value    : None
		//--------------------------------------------------------------------------
		///<summary>Creates a new instance of the deviceVersion class and performs specific initialisation.</summary>
		public deviceVersion()
		{
			this._productCode = 0xFFFFFFFF;
			this._revisionNumber = 0xFFFFFFFF;
			this._vendorID = 0xFFFFFFFF;
			// No specific initialisation required for this object.
		}
		#endregion

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
		//  Return value    : fbc - feedback to indicate whether the the device data
		//						    request was successful or a failure reason
		//--------------------------------------------------------------------------
		///<summary>Reads the vendor ID, product code and revision node from the connected device of the given nodeID.</summary>
		/// <param name="nodeID">the node ID of the connected controller for which
		/// the device information is to be retrieved (needed by CANcomms to build up the right COBID for SDOs)</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode readDeviceIdentity( int nodeID, communications CANcomms )
		{
			byte [] rxData = new byte[8];
			DIFeedbackCode	fbc = DIFeedbackCode.DISuccess;
			if ( MAIN_WINDOW.isVirtualNodes == false )
			{
				/* Device identity is mandatory and always index 0x1018
				 * Request the vendor ID from the controller.
				 * Use default communications timeouts for all SDOReads called here as we have not
				 * got an EDS for this device yet to specify otherwise.
				 */
				fbc = CANcomms.SDORead( nodeID, SCCorpStyle.identityObjectIndex, SCCorpStyle.vendorIDObjectSubIndex, SCCorpStyle.sizeOfVendorID, ref rxData, SCCorpStyle.TimeoutDefault );

				// Only format the vendorID data and request product code if controller replied OK.
				if ( fbc == DIFeedbackCode.DISuccess )
				{
					// little endian format so format the raw bytes to reconstruct the vendorID
					_vendorID = (uint)rxData[ 0 ];
					_vendorID += ((uint)rxData[ 1 ] << 8 );
					_vendorID += ((uint)rxData[ 2 ] << 16 );
					_vendorID += ((uint)rxData[ 3 ] << 24 );
				}
				else
				{
					_vendorID = 0xFFFFFFFF;  //use default - to indicate not available = for minimal ducitunoary etc
				}
				// request the product code from the controller.
				fbc = CANcomms.SDORead( nodeID, SCCorpStyle.identityObjectIndex, SCCorpStyle.productCodeObjectSubIndex, SCCorpStyle.sizeOfProductCode, ref rxData, SCCorpStyle.TimeoutDefault );
				if(fbc == DIFeedbackCode.DISuccess )
				{
					// little endian format so format the raw bytes to reconstruct the product code.
					_productCode = (uint)rxData[ 0 ];
					_productCode += ((uint)rxData[ 1 ] << 8 );
					_productCode += ((uint)rxData[ 2 ] << 16 );
					_productCode += ((uint)rxData[ 3 ] << 24 );
				}
				else
				{
					_productCode = 0xFFFFFFFF;
				}
				// request the product revision from the controller.
				fbc = CANcomms.SDORead( nodeID, SCCorpStyle.identityObjectIndex, SCCorpStyle.revisionNumberObjectSubIndex, SCCorpStyle.sizeOfRevisionNumber, ref rxData, SCCorpStyle.TimeoutDefault  );
				if ( fbc == DIFeedbackCode.DISuccess )
				{
					// little endian format so format the raw bytes to reconstruct the product revision
					_revisionNumber = (uint)rxData[ 0 ];
					_revisionNumber += ((uint)rxData[ 1 ] << 8 );
					_revisionNumber += ((uint)rxData[ 2 ] << 16 );
					_revisionNumber += ((uint)rxData[ 3 ] << 24 );
				}
				else
				{
					_revisionNumber = 0xFFFFFFFF;
				}

				// No current requirement for productName
			}
			return ( fbc );  //fbc is needed because if it is CaANGeneral error then furthe rhansdling is performed in the callign method
		}

		///<summary>Sets the device identity etc for DWMockup.</summary>
		///<param name="product">product number to set this to</param>
		///<param name="revision">revision number to set this to</param>
		///<param name="vendor">vendor number to set this to</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode readDeviceIdentity( uint vendor, uint product, uint revision )
		{
			_vendorID = vendor;
			_productCode = product;
			_revisionNumber = revision;

			return ( DIFeedbackCode.DISuccess );
		}
	}
}
