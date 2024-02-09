/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.19$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/09/2008 23:14:52$
	$ModDate:22/09/2008 22:40:18$

ORIGINAL AUTHOR
    Alison King

DESCRIPTION
    Layer service setting class. There should be one instance 
    of this object per connected CAN system via the IXXAT 
    USB-CAN adapter. This object is responsible for setting
    the node ID and the baud rate of a device when it has
    not already been configured and, as such, cannot be
    communicated via the regular SDOs.	

REFERENCES    

MODIFICATION HISTORY
    $Log:  43448: LSS.cs 

   Rev 1.19    23/09/2008 23:14:52  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.18.1.0    23/09/2008 23:10:12  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.18    18/02/2008 14:18:28  jw
 VCI2 CAN methods renamed and merged for consistency with VCI3 - towards back
 compatibility


   Rev 1.17    18/02/2008 09:28:54  jw
 VCI2 CAN methods moved form communications to VCI 2 and renamed for
 consistency with VCI3 - towards back compatibility


   Rev 1.16    13/02/2008 14:58:58  jw
 More VCI3. Now works generally OK but more testing/optimisation required. 


   Rev 1.15    25/01/2008 10:46:42  jw
 VCI3 and Vista functionality files merge - more testing required


   Rev 1.14    05/12/2007 21:13:44  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Text;

namespace DriveWizard
{
	/// <summary>
	/// The Layer Setting Services object is responsible for setting the node ID
	///	and baud rate of a single connected device when it has not got these already
	///	configured i.e. SDOs will only work if the baud rate is set up and known and
	///	the device has a valid node ID (which defined the COBID of the SDO response).
	///	For this to work, there must be a single device connected to DriveWizard
	///	otherwise the global message commands will result in several responses which
	///	could crash the CANbus.
	/// </summary>
	public class LSS
	{
		#region local variable declarations
		// current baud rate being used by the LSS
		private BaudRate LSSBaud = BaudRate._unknown;
		#endregion

		#region constructor and destructor
		//-------------------------------------------------------------------------
		//  Name			: LSS() constructor
		//  Description     : Initialises the LSS object when it is created. Currently
		//					  there is no special action that needs to be taken.
		//  Parameters      : None
		//  Used Variables  : None
		//  Preconditions   : None
		//  Post-conditions : None
		//  Return value    : None
		//--------------------------------------------------------------------------
		/// <summary>Layer Setting Services constructor.</summary>
		public LSS()
		{
		}
		#endregion

		#region public functions

		//-------------------------------------------------------------------------
		//  Name			: setNodeLayerSettings()
		//  Description     : This function communicates to the single connected device
		//					  on the CAN using LSS commands to try and set the baud rate
		//					  and the node ID to those values passed as parameters. This
		//					  is done using the CANcomms object to communicate using the
		//					  IXXAT adapter.
		//  Parameters      : requestedNodeID - node ID that this device is required to be
		//					  requestedBaudRate - baud rate that this device is required to
		//						operate at
		//				      CANcomms - CAN communications object (tx & rx on CANbus)
		//  Used Variables  : None
		//  Preconditions   : LSS has been requested and only one single device is
		//					  connected to DriveWizard via the CAN and IXXAT adapter.
		//  Post-conditions : If successful, the connected device has the node ID and
		//					  baud rate that was requested saved to it's configuration.
		//  Return value    : fbc - indicates success or gives a failure reason
		//--------------------------------------------------------------------------
		///<summary>Sets the node ID and baud rate to the requested values using LSS commands.</summary>
		/// <param name="requestedNodeID">node ID that this device is required to be</param>
		/// <param name="requestedBaudRate">baud rate that this device is required to operate at</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode setNodeLayerSettings( ushort requestedNodeID, BaudRate requestedBaudRate, communications CANcomms )
		{
			#region local variable declarations and initialisation
			LSSBaud = BaudRate._unknown;
			#endregion

			// ensure reasonable timeout for LSS commands
			CANcomms.setCommsTimeout(SCCorpStyle.TimeoutDefault);

			#region try and configure the node ID to the requested value on the single device using LSS
			DIFeedbackCode fbc = CANcomms.LSSConfigureNodeID( requestedNodeID );
			#endregion

			#region try and configure the baud rate to the requested value on the single device using LSS
			if ( fbc == DIFeedbackCode.DISuccess )
			{
				fbc = configureBaudRate( requestedBaudRate , CANcomms );
			}
			#endregion

			#region instruct the single device to store it's new nodeID and baud settings using LSS
			if ( fbc == DIFeedbackCode.DISuccess )
			{
				fbc = CANcomms.LSSStoreConfiguration();
			}
			#endregion

			#region switch the single device back to normal operational mode (from LSS mode)
			if ( fbc == DIFeedbackCode.DISuccess )
			{
				fbc = switchModeGlobal( LSSBaud, SCCorpStyle.LSSNormalMode, CANcomms );
			}
			#endregion

			return ( fbc );
		}
		#endregion

		#region private functions
		//-------------------------------------------------------------------------
		//  Name			: searchForNodeIDAtSpecifiedBaud()
		//  Description     : This function trys to switch the connected device into
		//					  the global mode using LSS commands. Because the device has
		//					  an unconfigured or unknown baud rate, it is necessary to 
		//					  repeat at each valid CAN baud rate until a valid response
		//					  is received.  Because transmitting at the wrong baud rate
		//					  can cause CAN errors, it is necessary to ask the user to
		//					  cycle power on the device between each failed attempt to
		//					  clear out these errors. 
		//  Parameters      : nodeID - the node ID of the device, found by the LSS 
		//						switch global command if communications are established.
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//  Used Variables  : None
		//  Preconditions   : LSS has been requested and only one single device is
		//					  connected to DriveWizard via the CAN and IXXAT adapter.
		//					  No communications has been established with the device yet.
		//  Post-conditions : If successful, communications with the device has been
		//					  established and the baud rate and node ID are known.
		//  Return value    : fbc - indicates success or gives a failure reason
		//--------------------------------------------------------------------------
		///<summary>Establishes ommunications with the device to find the node ID and baud rate.</summary>
		/// <param name="nodeID">the node ID of the device, found by the LSS switch global command 
		/// if communications are established </param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode searchForNodeIDAtSpecifiedBaud(BaudRate b,  out ushort nodeID, communications CANcomms )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			nodeID = 0;
			#endregion

			// unknown device baud rate so try the LSS commands at each valid CAN baud rate
				#region try to switch the connected device into LSS global mode
				fbc = switchModeGlobal( b, SCCorpStyle.LSSConfigureMode, CANcomms );
				#endregion

				#region use LSS commands to inquire what the device's current nodeID is
				if ( fbc == DIFeedbackCode.DISuccess )
				{
					fbc = CANcomms.LSSInquireNodeID( out nodeID );
				}
				#endregion

				#region if got an LSS response OK then we have the correct baud rate so quit this loop

				#endregion


			return ( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: switchModeGlobal()
		//  Description     : This function uses the LSS command to request the single
		//					  connected device to enter the LSS global mode ie it
		//					  will reply to any LSS command and does not require the
		//					  use of a unique node ID.
		//  Parameters      : baud - baud rate previously found to be valid for the
		//						connected device in LSS mode
		//				      LSSConfigMode - whether to put the device into configuration
		//						mode or to put it back to normal mode
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//  Used Variables  : LSSBaud - stores the baud rate for subsequent use
		//  Preconditions   : LSS has been requested and only one single device is
		//					  connected to DriveWizard via the CAN and IXXAT adapter.
		//					  DW requires to change the device's operational mode
		//					  to/from global mode or normal mode.
		//  Post-conditions : The device's mode has been changed to LSSConfigMode state.
		//  Return value    : fbc - indicates success or gives a failure reason
		//--------------------------------------------------------------------------
		///<summary>Sends an LSS command to switch the connected device to global or normal mode.</summary>
		/// <param name="baud">baud rate previously found to be valid for the connected device in LSS mode</param>
		/// <param name="LSSConfigMode">whether to put the device into configuration mode or to 
		/// put it back to normal mode</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		private DIFeedbackCode switchModeGlobal( BaudRate baud, bool LSSConfigMode, communications CANcomms )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;

			#region restart the CAN for the LSS acceptance mask & filter (only accept 0x7e4 and 0x7e5)
            // DR38000268 were wrong way round
            CANcomms.VCI.SetupCAN(baud, 0xfffffffe, 0x0000007e4);
            LSSBaud = baud;
			#endregion

			#region try and set the device into LSS configuration mode 
            //DR38000268 SetupCAN() no longer returns fbc => always DIGeneralFailure & never switches mode
			//if ( fbc == DIFeedbackCode.DISuccess ) 
			{
				fbc = CANcomms.LSSReadSwitchModeGlobal( LSSConfigMode );
			}
			#endregion

			return ( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: configureBaudRate()
		//  Description     : This function uses the LSS command to set the baud rate
		//					  on the device to that requested by the user.
		//  Parameters      : requestedBaudRate - baud rate that this device is required to
		//						operate at
		//				      CANcomms - CAN communications object (tx & rx on CANbus)
		//  Used Variables  : None
		//  Preconditions   : LSS has been requested and only one single device is
		//					  connected to DriveWizard via the CAN and IXXAT adapter
		//					  and the device is in LSS global mode.
		//  Post-conditions : If successful, the device has the baud rate set to
		//					  that requested by the user.
		//  Return value    : fbc - indicates success or gives a failure reason
		//--------------------------------------------------------------------------
		///<summary>Uses the LSS commands to set the baud rate on the physical device via the CAN.</summary>
		/// <param name="requestedBaudRate">baud rate that this device is required to operate at</param>
		/// <param name="CANcomms">CAN communications object (tx and rx on CANbus)</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		private DIFeedbackCode configureBaudRate( BaudRate requestedBaudRate, communications CANcomms )
		{
			#region local variable declarations and initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			int index = 0;
			int [] LSSIndex = new int [ 9 ] 
				//enum BaudRate { _1M, _800K, _500K, _250K, _125K, _50K, _20K, _10K, _unknown };
							    {   0,     1,     2,     3,     4,    6,    7,    8,        0 };
			#endregion

			// convert from baud rate enumerated type to index number recognised by the LSS device
			index = LSSIndex[ requestedBaudRate.GetHashCode() ];

			// set the bit timing parameter for the required baud rate on the device using LSS commands
			fbc = CANcomms.LSSConfigureBitTimingParameters( index );

			return ( fbc );
		}

		#endregion
	}
}
