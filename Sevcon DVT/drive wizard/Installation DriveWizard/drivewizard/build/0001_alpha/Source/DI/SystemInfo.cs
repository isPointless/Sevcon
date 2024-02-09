/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.229$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:08/10/2008 14:02:52$
	$ModDate:08/10/2008 11:57:18$

ORIGINAL AUTHOR
    Alison King

DESCRIPTION
    System Info class.  Should be one instance of this object
    per connected CAN system via the IXXAT USB-CAN adapter.
    This describes the nodes on the connected bus, defining
    their object dictionaries, the comms baud rate and status.
    This is responsible for all reading and writing of items
    within the connected controller's object dictionaries and
    keeps an updated copy within DW of the OD.  A programming
    node is used to read in the booteds file for FLASH
    programming and a DCFnode is used to store the OD defined
    in a DCF file with all the current values set as the DCF.
    Communications with any device is via the CANcomms object
    which accesses the CANbus via the IXXAT USB-CAN adapter.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36699: SystemInfo.cs 

   Rev 1.229    08/10/2008 14:02:52  ak
 TRR COD0013 post-test fixes


   Rev 1.228    23/09/2008 23:14:56  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.227.1.0    23/09/2008 23:10:16  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.227    09/07/2008 21:19:26  ak
 DCF fixes (progress bar and don't write a PDO mapping if insufficient access)


   Rev 1.226    08/04/2008 21:03:20  ak
 Prevent exception when processActivefaultLog() called for all Sevcon nodes.


   Rev 1.225    17/03/2008 13:13:18  jw
 Logs null obejct handling complete. Still needs progrees bar steps linking to
 odSub timeout values


   Rev 1.224    14/03/2008 10:57:00  jw
 Log handling revised, Read of Log releated CAN items now done from GUI ( to
 allow later DI /Ixxat unlinking) . Processing of recevied logs code and event
 lists moved to sysInfo - reduces memory usage ( one method per application
 rather than per node)  Some newer Sevocn devices do not hav ethese logs. Also
 event list methods now have product code input - to allow us to have seperate
 Event lists for eg displays etc. 


   Rev 1.223    13/03/2008 08:45:58  jw
 Some common ErrorSB handling  tasks moved form GUI files to here to reduce
 code size and complexity


   Rev 1.222    25/02/2008 16:21:16  jw
 New Feedback code added for not all Block Segments received. Some redundant
 Feedback codes removed


   Rev 1.221    21/02/2008 09:30:46  jw
 removal of VCI2 methods


   Rev 1.220    19/02/2008 15:28:00  jw
 Heartbeat and Emergency timer thread should only run once connection is
 complete to give DW best chance  of correct connection


   Rev 1.219    18/02/2008 14:18:28  jw
 VCI2 CAN methods renamed and merged for consistency with VCI3 - towards back
 compatibility


   Rev 1.218    18/02/2008 09:28:54  jw
 VCI2 CAN methods moved form communications to VCI 2 and renamed for
 consistency with VCI3 - towards back compatibility


   Rev 1.217    15/02/2008 12:43:22  jw
 TxData and the Monitiring params were static in VCI3. By passing VCI as
 object into the received message event handler we can make then instance
 variables as per VCI2 - closer to back compatibility


   Rev 1.216    15/02/2008 11:43:32  jw
 Reduncadnt code line removed to reduce compiler warnigns


   Rev 1.215    12/02/2008 08:49:20  jw
 Ongoing VCI3 work. Options and Select profiel windows changed to simplify
 threading and improve feedback.  Prog bar vlaue determination line made
 exception proof. Max and current values used by progress bars determined
 within DI for encapsulation and values reflect activitiy better.


   Rev 1.214    06/02/2008 08:14:18  jw
 DR38000239. One of SDORead methode renamed and simlipied for clarity


   Rev 1.213    29/01/2008 21:20:14  ak
 SCCorpStyle.SDOMaxConsecutiveNoResponses constant used when checking retries
 counts.


   Rev 1.212    25/01/2008 10:46:04  jw
 VCI3 and Vista functionality files merge - more testing required


   Rev 1.211    21/01/2008 12:03:12  jw
 File merge for VCI3/ Vista. These changes are those to go in all builds


   Rev 1.210    18-01-2008 10:44:00  jw
 DR000235 Remove DW support for bitstrings. ConvertToFloat ( inc remove
 redundant input parameter)  and ConverToDouble modified


   Rev 1.209    13/12/2007 20:40:34  ak
 nodeUnderTest property added.


   Rev 1.208    05/12/2007 21:13:34  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Text;
using Ixxat;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;


namespace DriveWizard
{
	#region enumerated type definitions
	#region Manufacturer
	/// <summary>Manufacturer: all controllers found on the connected network are handled
	/// in a slightly different manner depending on whether they are Sevcon nodes
	/// or 3rd party nodes.  Unknown implies no matching EDS could be found.
	/// </summary>
	public enum Manufacturer
	{
		///<summary>Sevcon device</summary>
		SEVCON,

		///<summary>Third party device</summary>
		THIRD_PARTY,

		///<summary>Unknown manufacturer device</summary>
		UNKNOWN
	};
	#endregion

	#region Device Interface feedback code types	
	/// <summary>
	/// Feedback codes.  The vast majority of functions return a feedback code to determine
	/// whether the function was successful or not.  If it fails, a reason is given.  This is
	/// a mixture of abort messages received from the controller (complying to CANOpen spec) 
	/// which are all prefixed CANxxxx and internal failure messages generated by the DI which
	/// are all prefixed with DIxxxx.
	/// </summary>
	public enum DIFeedbackCode
	{
		DICodeUnset = -1,
		CANUnknownAbortCode = 0x00000000,
	
		// DI generated failure codes
		DISuccess = 0x1000000,
		DIEmptyLog, //this is not an error as such - but we probalbly need to tell user
		DIGeneralFailure,
		DIInvalidIndexOrSub,
		DIInvalidItemOrSub,
		DIOutOfODMemory,
		DIInvalidSectionType,
		DIInvalidObjectType,
        DIInvalidCompactObjectFormat,       //DR38000260 error reporting
		DIUnhandledDataType,
		DIFailedToLogonToDevice,
		DIFailedToReadDeviceVersion,
		DIFailedToOpenEDSorDCFFile,
		DINoMatchingEDSFileFound,
		DINoMatchingXMLFileFound,
		DIFailedToWriteDCFFile,
		DIFailedToSetObjectDescription,
		DIFailedToSetSubObjectDescription,
		DIFailedToDetectBaudRate,
		DIInvalidNodeNumber,
		DIUnableToReadObjectValue,
		DIUnableToReadSectionValue,
		DINoResponseFromController,
		DIInsufficientAccessLevel,
		DIUnableToReadDCFFile,
		DIFailedToReadDCFCommissioningInfo,
        DIFailedToUpdateItemValue,
		DIMasterNodeNotASevconNode,
		DINumberOfItemsSubIsMissing, 
		DIFailedToReadNodeMasterStatus,
		DIUnableToForceIntoPreOpMode,
		DIUnableToReleaseFromPreOpMode,
		DIUnableToWriteToReadOnlyObject,
		DIDomainDataTypeExpected,
		DILogOfUnexpectedLength,
		DIInvalidOperationForLogType,
		DIUnableToRefindLastSystem,
		DIInvalidResponse,
		DILSSFailIncorrectBaudRate,
		DITransmitCommsFailure,
		DILSSNodeIDSetErrorCode,
		DILSSDeviceUnableToSave,
		DILSSSaveErrorCode,
		DILSSBaudSetErrorCode,
		DILSSInadmissableBaudRate,
		DILSSDeviceUnableToAccessStorage,
		DIUnableToAllocateCOBIDForPDOMap,
		DIInsufficientFreeDynamicPDOs,
		DISDOBlockTooLarge,
		DIReal32SetToNullValue,
		DIReal64SetToNullValue,
		DIThreeConsecutiveNonResponseFromDevice,
		DIFailedToDeletePDO,
		DIUnexpectedNoOfNodeIDs,
		DIUnableToWriteToAllParameters,
		DIFailedToWriteDCFChecksum,
		DICannotWriteAStringViaBackDoor,
		DIFailedToReadFileInformationSectionInEDS,
		DIFailedToReadFileInformationSectionInDCF,
		DIFailedToReadDeviceInformationSectionInEDS,
		DIFailedToReadDeviceInformationSectionInDCF,
		DIFailedToReadObjectListsInDCF,
		DIFailedToReadObjectListsInEDS,
		DIFailedToReadObjectDescriptionInEDS,
        DIMissingSupportedObjectsInEDSorDCF, //DR38000260
		DIUnexpectedDataLength,
		DIPDOAlreadyMappedTwiceOnSevconNode,
		DIFailedToWriteItemViaDCFBackdoorInvalidKey,
		DIUnableToWritePDOMapToDevice,
		DISomeProblemsDownloadingDCF,
		DISomeProblemsWhenUploadingLog,
		DINoExistingSystemToRefind,
		DINodeDeviceTypesDoNotMatch,
		DIInvalidFilename,
        DIMissingSequenceResponse,
        DIMultipleMatchingEDSFilesFound,

		// controller generated failure codes
		CANToggleBitNotAlternated = 0x5030000,
		CANSDOProtocolTimeOut = 0x5040000,
		CANCommandSpecifierUnknown = 0x5040001,
		CANInvalidBlockSize = 0x5040002,
		CANInvalidSequenceNumber = 0x5040003,
		CANCRCError = 0x5040004,
		CANOutOfMemory = 0x5040005,
		CANUnsupportedAccessToObject = 0x6010000,
		CANAttemptToReadWriteOnlyObject = 0x6010001,
		CANAttemptToWriteReadOnlyObject = 0x6010002,
		CANObjectDoesNotExistInOD = 0x6020000,
		CANObjectCannotBeMappedToPDO = 0x6040041,
		CANObjectsMappedExceedPDOLength = 0x6040042,
		CANGeneralParameterIncompatibilityReason = 0x6040043,
		CANGeneralInternalIncompatibilityOfDevice = 0x6040047,
		CANAccessFailedDueToHardware = 0x6060000,
		CANDataTypeLengthMismatch = 0x6070010,
		CANDataTypeLengthTooHigh = 0x6070012,
		CANDataTypeLengthTooLow = 0x6070013,
		CANSubIndexDoesNotExist = 0x6090011,
		CANParameterValueExceededRange = 0x6090030,
		CANParameterValueWrittenTooHigh = 0x6090031,
		CANParameterValueWrittenTooLow = 0x6090032,
		CANMaxValueLessThanMinValue = 0x6090036,
		CANGeneralError = 0x8000000,
		CANDataCannotBeStoredToApplication = 0x8000020,
		CANDataCannotBeStoredToApplicationByLocalControl = 0x8000021,
		CANDataCannotBeStoredToApplicationByDeviceState = 0x8000022,
		CANObjectDictionaryGenerationFail = 0x8000023
	};
	#endregion

	#region emergency messages
	/* Enumerated types for the CANOpen emergency messages.  These can be converted to a text string
	 * to form a useful message display to the user to help them interpret the emergency message.
	 */
	#region emergency messages for a motor from DS402
	/// <summary>
	/// Enumerated types for emergency messages for a motor, defined in DS402.
	/// </summary>
	public enum EMCYMessagesMotor
	{
		//NoError = 0x0000,
		GeneralError = 0x1000,
		
		CurrentGeneral = 0x2000,
		CurrentOnDeviceInputSide = 0x2100,
		CurrentShortCircuitOrEarthLeakage = 0x2110,
		CurrentEarthLeakage = 0x2120,
		CurrentEarthLeakagePhase1 = 0x2121,
		CurrentEarthLeakagePhase2 = 0x2122,
		CurrentEarthLeakagePhase3 = 0x2123,
		CurrentShortCircuit = 0x2130,
		CurrentShortCircuitPhasesL1ToL2 = 0x2131,
		CurrentShortCircuitPhasesL2ToL3 = 0x2132,
		CurrentShortCircuitPhasesL3ToL1 = 0x2133,
		CurrentInternal = 0x2200,
		CurrentInternalCurrentNumber1 = 0x2211,
		CurrentInternalCurrentNumber2 = 0x2212,
		CurrentInternalOverCurrentInRampFunction = 0x2213,
		CurrentInternalOverCurrentInTheSequence = 0x2214,
		CurrentInternalCurrentContinuousOverCurrent = 0x2220,
		CurrentInternalContinuousOverCurrentNumber1 = 0x2221,
		CurrentInternalContinuousOverCurrentNumber2 = 0x2222,
		CurrentInternalShortCircuitOrCurrentLeakage = 0x2230,
		CurrentInternalEarthLeakage = 0x2240,
		CurrentInternalEarthLeakageShortCircuit = 0x2250,
		CurrentOnDeviceOutputSide = 0x2300,
		CurrentOnDeviceOutputSideContinuousOverCurrent = 0x2310,
		CurrentOnDeviceOutputSideContinuousOverCurrentNumber1 = 0x2311,
		CurrentOnDeviceOutputSideContinuousOverCurrentNumber2 = 0x2312,
		CurrentOnDeviceOutputSideShortCircuitOrEarthLeakage = 0x2320,
		CurrentOnDeviceOutputSideEarthLeakage = 0x2330,
		CurrentOnDeviceOutputSideEarthLeakagePhaseU = 0x2331,
		CurrentOnDeviceOutputSideEarthLeakagePhaseV = 0x2332,
		CurrentOnDeviceOutputSideEarthLeakagePhaseW = 0x2333,
		CurrentOnDeviceOutputSideShortCircuit = 0x2340,
		CurrentOnDeviceOutputSideShortCircuitPhasesUV = 0x2341,
		CurrentOnDeviceOutputSideShortCircuitPhasesVW = 0x2342,
		CurrentOnDeviceOutputSideShortCircuitPhasesWU = 0x2343,
		
		VoltageGeneral = 0x3000,
		VoltageMainsVoltage = 0x3100,
		VoltageMainsOverVoltage = 0x3110,
		VoltageMainsOverVoltagePhaseL1 = 0x3111,
		VoltageMainsOverVoltagePhaseL2 = 0x3112,
		VoltageMainsOverVoltagePhaseL3 = 0x3113,
		VoltageMainsUnderVoltage = 0x3120,
		VoltageMainsUnderVoltagePhaseL1 = 0x3121,
		VoltageMainsUnderVoltagePhaseL2 = 0x3122,
		VoltageMainsUnderVoltagePhaseL3 = 0x3123,
		VoltagePhaseFailure = 0x3130,
		VoltagePhaseFailureL1 = 0x3131,
		VoltagePhaseFailureL2 = 0x3132,
		VoltagePhaseFailureL3 = 0x3133,
		VoltagePhaseFailurePhaseSequence = 0x3134,
		VoltageMainsFrequency = 0x3140,
		VoltageMainsFrequencyTooGreat = 0x3141,
		VoltageMainsFrequencyTooSmall = 0x3142,
		VoltageDCLinkVoltage = 0x3200,
		VoltageDCLinkVoltageOverVoltage = 0x3210,
		VoltageDCLinkVoltageOverVoltageNumber1 = 0x3211,
		VoltageDCLinkVoltageOverVoltageNumber2 = 0x3212,
		VoltageLoadError = 0x3230,
		VoltageOutputVoltage = 0x3300,
		VoltageOutputOverVoltage = 0x3310,
		VoltageOutputOverVoltagePhaseU = 0x3311,
		VoltageOutputOverVoltagePhaseV = 0x3312,
		VoltageOutputOverVoltagePhaseW = 0x3313,
		VoltageArmatureCircuit = 0x3320,
		VoltageArmatureCircuitInterrupted = 0x3321,
		VoltageFieldCircuit = 0x3330,
		VoltageFieldCircuitInterrupted = 0x3331,

		TemperatureGeneral = 0x4000,
		TemperatureAmbientTemperature = 0x40100,
		TemperatureAmbientTemperatureExcessAmbient = 0x40110,
		TemperatureAmbientTemperatureTooLowAmbient = 0x40120,
		TemperatureAmbientTemperatureSupplyAir = 0x40130,
		TemperatureAmbientTemperatureAirOutlet = 0x40140,
		TemperatureDevice = 0x4200,
		TemperatureDeviceExcess = 0x4210,
		TemperatureDeviceTooLow = 0x4220,
		TemperatureDrive = 0x4300,
		TemperatureDriveExcess = 0x4310,
		TemperatureDriveTooLow = 0x4320,
		TemperatureSupply = 0x4400,
		TemperatureSupplyExcess = 0x4410,
		TemperatureSupplyTooLow = 0x4420,
		
		DeviceHardwareGeneral = 0x5000,
		DeviceHardwareSupply = 0x5100,
		DeviceHardwareSupplyLowVoltage = 0x5110,
		DeviceHardwareSupplyLowVoltageU1SupplyPlusMinus15V = 0x5111,
		DeviceHardwareSupplyLowVoltageU2SupplyPlus24V = 0x5112,
		DeviceHardwareSupplyLowVoltageU3SupplyPlus5V = 0x5113,
		DeviceHardwareSupplyLowVoltageU4ManufacturerSpecific = 0x5114,
		DeviceHardwareSupplyLowVoltageU5ManufacturerSpecific = 0x5115,
		DeviceHardwareSupplyLowVoltageU6ManufacturerSpecific = 0x5116,
		DeviceHardwareSupplyLowVoltageU7ManufacturerSpecific = 0x5117,
		DeviceHardwareSupplyLowVoltageU8ManufacturerSpecific = 0x5118,
		DeviceHardwareSupplyIntermediateCircuit = 0x5120,
		DeviceHardwareControl = 0x5200,
		DeviceHardwareControlMeasurementCircuit = 0x5210,
		DeviceHardwareControlControlCircuit = 0x5220,
		DeviceHardwareOperatingUnit = 0x5300,
		DeviceHardwarePowerSection = 0x5400,
		DeviceHardwareOutputStates = 0x5410,
		DeviceHardwareChopper = 0x5420,
		DeviceHardwareInputStates = 0x5430,
		DeviceHardwareContacts = 0x5440,
		DeviceHardwareContact1ManufacturerSpecific = 0x5441,
		DeviceHardwareContact2ManufacturerSpecific = 0x5442,
		DeviceHardwareContact3ManufacturerSpecific = 0x5443,
		DeviceHardwareContact4ManufacturerSpecific = 0x5444,
		DeviceHardwareContact5ManufacturerSpecific = 0x5445,
		DeviceHardwareFuses = 0x5450,
		DeviceHardwareFusesS1_I1 = 0x5451,
		DeviceHardwareFusesS2_I2 = 0x5452,
		DeviceHardwareFusesS3_I3 = 0x5453,
		DeviceHardwareFusesS4ManufacturerSpecific = 0x5454,
		DeviceHardwareFusesS5ManufacturerSpecific = 0x5455,
		DeviceHardwareFusesS6ManufacturerSpecific = 0x5456,
		DeviceHardwareFusesS7ManufacturerSpecific = 0x5457,
		DeviceHardwareFusesS8ManufacturerSpecific = 0x5458,
		DeviceHardwareFusesS9ManufacturerSpecific = 0x5459,
		DeviceHardwareDataStorage = 0x5500,
		DeviceHardwareDataStorageWorkingMemory = 0x5510,
		DeviceHardwareDataStorageProgramMemory = 0x5520,
		DeviceHardwareDataStorageNonVolatileMemory = 0x5530,

		DeviceSoftwareGeneral = 0x6000,
		DeviceSoftwareSoftwareReset = 0x6010,
		DeviceSoftwareInternalSoftware = 0x6100,
		DeviceSoftwareUserSoftware = 0x6200,
		DeviceSoftwareDataRecord = 0x6300,
		DeviceSoftwareDataRecord1 = 0x6301,
		DeviceSoftwareDataRecord2 = 0x6302,
		DeviceSoftwareDataRecord3 = 0x6303,
		DeviceSoftwareDataRecord4 = 0x6304,
		DeviceSoftwareDataRecord5 = 0x6305,
		DeviceSoftwareDataRecord6 = 0x6306,
		DeviceSoftwareDataRecord7 = 0x6307,
		DeviceSoftwareDataRecord8 = 0x6308,
		DeviceSoftwareDataRecord9 = 0x6309,
		DeviceSoftwareDataRecord10 = 0x630a,
		DeviceSoftwareDataRecord11 = 0x630b,
		DeviceSoftwareDataRecord12 = 0x630c,
		DeviceSoftwareDataRecord13 = 0x630d,
		DeviceSoftwareDataRecord14 = 0x630e,
		DeviceSoftwareDataRecord15 = 0x630f,
		DeviceSoftwareLossOfParameters = 0x6310,
		DeviceSoftwareParameterError = 0x6320,

		AdditionalModulesGeneral = 0x7000,
		AdditionalModulesPower = 0x7100,
		AdditionalModulesBrakeChopper = 0x7110,
		AdditionalModulesBrakeChopperFailure = 0x7111,
		AdditionalModulesBrakeChopperOvercurrent = 0x7112,
		AdditionalModulesBrakeChopperProtectiveCircuit = 0x7113,
		AdditionalModulesMotor = 0x7120,
		AdditionalModulesMotorBlocked = 0x7121,
		AdditionalModulesMotorOrCommutationMalfunction = 0x7122,
		AdditionalModulesMotorTilted = 0x7123,
		AdditionalModulesMeasurementCircuit = 0x7200,
		AdditionalModulesSensor = 0x7300,
		AdditionalModulesSensorTachoFault = 0x7301,
		AdditionalModulesSensorTachoWrongPolarity = 0x7302,
		AdditionalModulesSensorResolver1Fault = 0x7303,
		AdditionalModulesSensorResolver2Fault = 0x7304,
		AdditionalModulesSensorIncrementalSensor1Fault = 0x7305,
		AdditionalModulesSensorIncrementalSensor2Fault = 0x7306,
		AdditionalModulesSensorIncrementalSensor3Fault = 0x7307,
		AdditionalModulesSensorSpeed = 0x7310,
		AdditionalModulesSensorPosition = 0x7320,
		AdditionalModulesComputationCircuit = 0x7400,
		AdditionalModulesCommunication = 0x7500,
		AdditionalModulesCommunicationSerialInterface1 = 0x7510,
		AdditionalModulesCommunicationSerialInterface2 = 0x7520,
		AdditionalModulesDataStorage = 0x7600,

		MonitoringGeneral = 0x8000,
		MonitoringCommunication = 0x8100,
		MonitoringCommunicationCANOverrun = 0x8110,
		MonitoringCommunicationCANInErrorPassiveMode = 0x8120,
		MonitoringCommunicationLifeGuardOrHeartbeatError = 0x8130,
		MonitoringCommunicationRecoveredFromOffBus = 0x8140,
		MonitoringCommunicationTransmitCOBID = 0x8150,
		MonitoringProtocolError = 0x8200,
		MonitoringProtocolErrorPDONotProcessedDueToLengthError = 0x8210,
		MonitoringProtocolErrorPDOLengthExceeded = 0x8220,
		MonitoringTorqueControl = 0x8300,
		MonitoringTorqueControlExcessTorque = 0x8311,
		MonitoringTorqueControlDifficultStartUp = 0x8312,
		MonitoringTorqueControlStandstillTorque = 0x8313,
		MonitoringTorqueControlInsufficientTorque = 0x8321,
		MonitoringTorqueControlTorqueFault = 0x8331,
		MonitoringVelocitySpeedController = 0x8400,
		MonitoringPositionController = 0x8500,
		MonitoringPPositioningController = 0x8600,
		MonitoringPPositioningControllerFollowingError = 0x8611,
		MonitoringPPositioningControllerReferenceLimit = 0x8612,
		MonitoringSyncController = 0x8700,
		MonitoringWindingController = 0x8800,
		MonitoringProcessDataMonitoring = 0x8900,
		MonitoringControl = 0x8a00,
		
		ExternalGeneral = 0x9000,

		AdditionalFunctionsGeneral = 0xF000,
		AdditionalFunctionsDeceleration = 0xF001,
		AdditionalFunctionsSubSynchronousRun = 0xF002,
		AdditionalFunctionsStrokeOperations = 0xF003,
		AdditionalFunctionsControl = 0xF004,
		ManufacturerSpecific0 = 0xff00,
		// :
		// :
		// :
		ManufacturerSpecific255 = 0xffff
	};
	#endregion

	#region emergency messages for an IO module - from DS301
	/// <summary>
	/// Enumerated types for emergency messages for an IO module, defined in DS301.
	/// </summary>
	public enum EMCYMessagesIO
	{
		CurrentAtOutputsTooHigh = 0x2310,
		ShortCircuitAtOutputs = 0x2320,
		LoadDumpAtOutputs = 0x2330,
		InputVoltageTooHigh = 0x3110,
		InputVoltageTooLow = 0x3120,
		InternalVoltageTooHigh = 0x3210,
		InternalVoltageTooLow = 0x3220,
		OutputVoltageTooHigh  = 0x3310,
		OutputVoltageTooLow = 0x3320
	};
	#endregion

	#region emergency messages general - from DS401
	/// <summary>
	/// Enumerated types for general emergency, defined in DS401.
	/// </summary>
	public enum EMCYMessagesGeneral
	{
		ErrorResetOrNoError = 0x00,

		GenericError = 0x10,

		CurrentGeneral = 0x20,
		CurrentDeviceInputSide = 0x21,
		CurrentInsideTheDevice = 0x22,
		CurrentDeviceOutputSide = 0x23,
		
		VoltageGeneral = 0x30,
		VoltageMains = 0x31,
		VoltageInsideTheDevice = 0x32,
		VoltageOutputVoltage = 0x33,
		
		TemperatureGeneral = 0x40,
		TemperatureAmbient = 0x41,
		TemperatureDevice = 0x42,

		DeviceHardwareGeneral = 0x50,

		DeviceSoftwareGeneral = 0x60,
		DeviceSoftwareInternal = 0x61,
		DeviceSoftwareUserSoftware = 0x62,
		DeviceSoftwareDataSet = 0x63,

		AdditionalModulesGeneral = 0x70,
		
		MonitoringGeneral = 0x80,
		MonitoringCommunication = 0x81,
		MonitoringProtocolError = 0x82,

		ExternalErrorGeneral = 0x90,
		
		AdditionalFunctionsGeneral = 0xf0,

		DeviceSpecificGeneral = 0xff
	};
	#endregion

	#region error register bit meanings contained in an emergency message
	/// <summary>
	/// Emergency messages error regist bit interpretations (more than one bit can be set).
	/// </summary>
	public enum ErrorRegister
	{
		/// <summary>Controller generic error </summary>
		Generic = 0x01,
		
		/// <summary>Controller current error </summary>
		Current = 0x02,
		
		/// <summary>Controller voltage error </summary>
		Voltage = 0x04,
		
		/// <summary>Controller temperature error </summary>
		Temperature = 0x08,
		
		/// <summary>Controller communication error </summary>
		CommunicationError = 0x10,
		
		/// <summary>Controller device profile error </summary>
		DeviceProfileSpecific = 0x20,
		
		/// <summary>Controller reserved error </summary>
		Reserved = 0x40,
		
		/// <summary>Controller manufacturer specific error </summary>
		ManufacturerSpecific = 0x80
	};
	#endregion

	#endregion

	///<summary>Enumerated type for all possible baud rates that are defined in CANopen. </summary>
	public enum BaudRate { _1M, _800K, _500K, _250K, _125K, _50K, _20K, _10K, _100K, _unknown };

	#endregion

	#region structure definitions

	internal struct  DisplayDataLogEntry
	{
		internal System.DateTime dateAndTime;
		internal float Param1;
		internal float Param2;
		internal float Param3;
		internal float Param4;
		internal float Param5;
		internal float Param6;
	}

	internal struct DisplayFaultLogEntry
	{
		internal System.DateTime dateAndTime;
		internal ushort	faultID;
		internal byte 	nodeID;
		internal byte    count; //doubles as fault count
	}
	#region fault log entry
	/// <summary>
	/// Fault Log Entry data structure.  This data structure is used to reconstruct the data 
	/// retrieved using the DOMAIN_UPLOAD object for display.
	/// </summary>
	public class FIFOLogEntry
	{
		///<summary>unique ID of event</summary>
		public System.UInt16	eventID = 0;			

		///<summary>text string description of event</summary>
		public System.String	description = "";		

		///<summary>key hours of when fault occurred</summary>
		public System.UInt16	hours = 0;				

		///<summary>key mins and secs (1 bit = 15s)</summary>
		public System.Byte		minsAndSecs = 0;		

		///<summary>first byte of additional information</summary>
		public System.Byte		db1 = 0;				

		///<summary>second byte of additional information</summary>
		public System.Byte		db2 = 0;

		///<summary>third byte of additional information</summary>
		public System.Byte		db3 = 0;
	};
	#endregion

	#region event log entry
	/// <summary>
	/// Event Log Entry data structure.  This data structure is used to reconstruct the data 
	/// retrieved using the DOMAIN_UPLOAD object for display.
	/// </summary>
	public class EventLogEntry
	{
		///<summary>unique ID of event</summary>
		public System.UInt16	eventID;			

		///<summary>text string desciptor of event</summary>
		public System.String	description;		
		
		///<summary>time of first event (hours)</summary>
		public System.UInt16	firstHours;			
		
		///<summary>mins and secs (1 bit = 15s)</summary>
		public System.Byte		firstMinsAndSecs;	
		
		///<summary>time of last event (hours)</summary>
		public System.UInt16	lastHours;			
		
		///<summary>mins and secs (1 bit = 15s)</summary>
		public System.Byte		lastMinsAndSecs;	
		
		///<summary>no. of events counted since last reset</summary>
		public System.UInt16	counter;			
	};
	#endregion

	#region operational log entry
	/// <summary>
	/// Operational Log Entry data structure.  This data structure is used to reconstruct the data 
	/// retrieved using the DOMAIN_UPLOAD object for display.
	/// </summary>
	public class OperationalLog
	{
		///<summary> minimum battery voltage since the op log was last reset</summary>
		public System.Int16	batteryVoltsMin;		

		///<summary> maximum battery voltage since the op log was last reset</summary>
		public System.Int16	batteryVoltsMax;
		
		///<summary> minimum capacitor voltage since the op log was last reset</summary>
		public System.Int16	capacitorVoltsMin;
		
		///<summary> maximum capacitor voltage since the op log was last reset</summary>
		public System.Int16	capacitorVoltsMax;
		
		///<summary> minimum motor current 1 since the op log was last reset</summary>
		public System.Int16	motorCurrent1Min;
		
		///<summary> maximum motor current 1 since the op log was last reset</summary>
		public System.Int16	motorCurrent1Max;
		
		///<summary> minimum motor current 2 since the op log was last reset</summary>
		public System.Int16	motorCurrent2Min;
		
		///<summary> maximum motor current 2 since the op log was last reset</summary>
		public System.Int16	motorCurrent2Max;
		
		///<summary> minimum motor speed since the op log was last reset</summary>
		public System.Int16	motorSpeedMin;
		
		///<summary> maximum motor speed since the op log was last reset</summary>
		public System.Int16	motorSpeedMax;
		
		///<summary> minimum temperature since the op log was last reset</summary>
		public System.Int16	temperatureMin;
		
		///<summary> maximum temperature since the op log was last reset</summary>
		public System.Int16	temperatureMax;
	};
	#endregion

	#region operational log formatting (collated from OD for ease of display of op logs)
	/// <summary>
	/// Need to pass all the operational log item scalings and units to the GUI 
	/// so that the raw data can be translated into something meaningful to
	/// the user.  Collated here from the OD of given node for convenience.
	/// </summary>
	public struct OperationalLogFormatting
	{
		///<summary>battery voltage scaling found from device's EDS</summary>
		public double			batteryVoltsScaling;

		///<summary>capacitor voltage scaling found from device's EDS</summary>
		public double			capacitorVoltsScaling;
		
		///<summary>motor current 1 scaling found from device's EDS</summary>
		public double			motorCurrent1Scaling;
		
		///<summary>motor current 2 scaling found from device's EDS</summary>
		public double			motorCurrent2Scaling;
		
		///<summary>motor speed scaling found from device's EDS</summary>
		public double			motorSpeedScaling;
		
		///<summary>temperature scaling found from device's EDS</summary>
		public double			temperatureScaling;

		///<summary>battery voltage units found from device's EDS</summary>
		public string			batteryVoltsUnits;
		
		///<summary>capcitor voltage units found from device's EDS</summary>
		public string			capacitorVoltsUnits;
		
		///<summary>motor current 1 units found from device's EDS</summary>
		public string			motorCurrent1Units;
		
		///<summary>motor current 2 units found from device's EDS</summary>
		public string			motorCurrent2Units;
		
		///<summary>motor speed units found from device's EDS</summary>
		public string			motorSpeedUnits;
		
		///<summary>temperature units found from device's EDS</summary>
		public string			temperatureUnits;
	};
	#endregion

	#region active node faults
	/// <summary>
	/// Current active faults data structure. Used to reconstruct data from DOMAIN_UPLOAD. 
	/// Strings associated with a fault (description) are held in a "IDs\EventIDs.txt" file.
	/// This gets updated if DW retrieves a faultID (or eventID) which is unknown to it and
	/// updates this file.
	/// </summary>
	public class NodeFaultEntry
	{
		///<summary>Sevcon fault identifier code for a fault or event log entry</summary>
		public System.UInt16	eventID;

		///<summary>Sevcon text string description associated with the fault identifier.</summary>
		public System.String	description;
	};
	#endregion

	#region COBID with communication parameters and which nodes use this COB
	/// <summary>
	/// This structure is used to hold an entire system mapping of a specific COBID. 
	/// Instances are held in a Hashtable with the COBID being the key.  This struct
	/// holds the comms data, which nodes (if any) that transmit this COBID and which
	/// nodes (if any) which receive this COBID.
	/// Two booleans are calculated based on the fact exactly one node should transmit
	/// the COBID and one or more nodes should receive it.
	/// </summary>
	public class COBIDMapping
	{
		public COBIDMapping()
		{
		}
		///<summary>sized array holding all node IDs which rx this COBID</summary>
		public int []	receiveNodes = new int[0];		

		///<summary>sized array holding all node IDs which tx this COBID</summary>
		public int []	transmitNodes = new int[0];		

		///<summary>transmission type of COBID</summary>
		public int		type = 0;				
		
		///<summary>inhibit time for transmission (if asynchronous)</summary>
		public int		inhibitTime = 0;		
		
		///<summary>event time for transmission</summary>
		public int		eventTimer = 0;			

		///<summary>message type of this COBID (needed by GUI)</summary>
		public COBIDType messageType = COBIDType.Unknown;		
	};
	#endregion

	#endregion

	#region delegate event declarations
	///<summary>declaration of the heartbeat message delegate function - passed up from the VCI</summary>
	public delegate void onNewHeartbeat( int COBID, int status );

	///<summary>declaration of the emergency message delegate function - passed up from the VCI</summary>
	public delegate void onNewEmergency( byte [] data );

	///<summary>declaration of the heartbeat and emergency delegate function - passed up to the main window</summary>
	public delegate void StateChangeListener( COBIDType messageType, int nodeID, int CANNodeIndex, NodeState newNodeState, string emergencyMessage );
	#endregion

	///<summary>
	/// System Info class.  Should be one instance of this object
	/// per connected CAN system via the IXXAT USB-CAN adapter.
	/// This describes the nodes on the connected bus, defining
	/// their object dictionaries, the comms baud rate and status.
	/// This is responsible for all reading and writing of items
	/// within the connected controller's object dictionaries and
	/// keeps an updated copy within DW of the OD.  A programming
	/// node is used to read in the booteds file for FLASH
	/// programming and a DCFnode is used to store the OD defined
	/// in a DCF file with all the current values set as the DCF.
	/// Communications with any device is via the CANcomms object
	/// which accesses the CANbus via the IXXAT USB-CAN adapter.
	///</summary>
	public class SystemInfo :IDisposable 
	{
		#region constants
		#region data limits constants definition
		/// <summary>
		/// Data limits and data size (in bytes) for each CANopen specified data type
		/// is given here as a constant.  This is used to provide the valid ranges
		/// for a given data type and to provide the expected data size for when items
		/// are transmitted (to allow contsruction of the CANopen data packets)
		/// and received from a controller (to allow memory allocation in the DW DI).
		/// N.B. 38 is the length of CANopenDataType enumerated type.
		/// If any data types are added, this structure must be updated accordingly.
		/// </summary>
		internal DataLimits [] dataLimits = 
		{
			new DataLimits( 0,					0,						0	),	//NULL = 0,
			new DataLimits( 0,					1,						1	),	//BOOLEAN = 1,
			new DataLimits( ((long)1<<7)*-1,	((long)1<<7)-1,			1	),	//INTEGER8 = 2,
			new DataLimits( ((long)1<<15)*-1,	((long)1<<15)-1,		2	),	//INTEGER16 = 3, 
			new DataLimits( ((long)1<<31)*-1,	((long)1<<31)-1,		4	),	//INTEGER32 = 4,
			new DataLimits( 0,					((long)1<<8)-1,			1	),	//UNSIGNED8 = 5,
			new DataLimits( 0,					((long)1<<16)-1,		2	),	//UNSIGNED16 = 6, 
			new DataLimits( 0,					((long)1<<32)-1,		4	),	//UNSIGNED32 = 7, 
			new DataLimits( 0,					0,						4	),	//REAL32 = 8,
			new DataLimits( 0,					0,						256 ),	//VISIBLE_STRING = 9,
			new DataLimits( 0,					0,						256 ),	//OCTET_STRING = 10,
			new DataLimits( 0,					0,						256 ),	//UNICODE_STRING = 11,
			new DataLimits( 0,					0,						10 	),	//TIME_OF_DAY = 12,
			new DataLimits( 0,					0,						10  ),	//TIME_DIFFERENCE = 13,
			new DataLimits( 0,					0,						0	),	//RESERVED1 = 14,
			new DataLimits( 0,					0,						0xffffff),	//DOMAIN = 15,
			new DataLimits( ((long)1<<23)*-1,	((long)1<<23)-1,		3	),	//INTEGER24 = 16, 
			new DataLimits( 0,					0,						8	),	//REAL64 = 17,
			new DataLimits( ((long)1<<39)*-1,	((long)1<<39)-1,		5	),	//INTEGER40 = 18,
			new DataLimits( ((long)1<<47)*-1,	((long)1<<47)-1,		6	),	//INTEGER48 = 19,
			new DataLimits( ((long)1<<55)*-1,	((long)1<<55)-1,		7	),	//INTEGER56 = 20,
			new DataLimits( (((long)1<<62)*-1 <<1),(long)((((ulong)1<<62)<<1)-1),	8 ),	//INTEGER64 = 21,
			new DataLimits( 0,					((long)1<<24)-1,		3	),	//UNSIGNED24 = 22,
			new DataLimits( 0,					0,						0	),	//RESERVED2 = 23,
			new DataLimits( 0,					((long)1<<40)-1,		5	),	//UNSIGNED40 = 24,
			new DataLimits( 0,					((long)1<<48)-1,		6	),	//UNSIGNED48 = 25,
			new DataLimits( 0,					((long)1<<56)-1,		7	),	//UNSIGNED56 = 26,
			new DataLimits( 0,					((long)1<<64)-1,		8	),	//UNSIGNED64 = 27,
			new DataLimits( 0,					0,						0	),	//RESERVED3 = 28,
			new DataLimits( 0,					0,						0	),	//RESERVED4 = 29,
			new DataLimits( 0,					0,						0	),	//RESERVED5 = 30,
			new DataLimits( 0,					0,						0	),	//RESERVED6 = 31,
			new DataLimits( 0,					0,						100 ),	//PDO_COMMUNICATION_PARAMETER = 32,
			new DataLimits( 0,					0,						100 ),	//PDO_MAPPING = 33,
			new DataLimits( 0,					0,						100 ),	//SDO_PARAMETER = 34,
			new DataLimits( 0,					0,						100 ),	//IDENTITY = 35,
			new DataLimits( 0,					0,						0   ),	//RECORD = 36,
			new DataLimits( 0,					0,						0	),	//ARRAY = 128,
		};	

		#endregion
		#region CANopen OD section limits
		/// <summary>Defines the valid range of object dictionary indices for each CANopen section</summary>
		internal CANSectionLimits [] CANSectionODRange = 
		{
			new CANSectionLimits( 0x0000,			0x0fff	),	//None = 0
			new CANSectionLimits( 0x1000,			0x1fff	),	//CommunicationProfile = 1
			new CANSectionLimits( 0x2000,			0x5fff	),	//ManufacturerSpecificProfile = 2
			new CANSectionLimits( 0x6000,			0x67ff	),	//StandardisedDeviceProfile0 = 4
			new CANSectionLimits( 0x6800,			0x6fff	),	//StandardisedDeviceProfile1 = 5
			new CANSectionLimits( 0x7000,			0x77ff	),	//StandardisedDeviceProfile2 = 6
			new CANSectionLimits( 0x7800,			0x7fff	),	//StandardisedDeviceProfile3 = 7
			new CANSectionLimits( 0x8000,			0x87ff	),	//StandardisedDeviceProfile4 = 8
			new CANSectionLimits( 0x8800,			0x8fff	),	//StandardisedDeviceProfile5 = 9
			new CANSectionLimits( 0x9000,			0x97ff	),	//StandardisedDeviceProfile6 = 10
			new CANSectionLimits( 0x9800,			0x9fff	),	//StandardisedDeviceProfile7 = 11
		};
		#endregion CANopen OD section limits
		#endregion constants

		#region variable declarations

		#region statics
		internal static StringBuilder errorSB = new StringBuilder();
		internal static bool CommsProfileItemChanged = false;
        internal static int itemCounter1 = 0;
        internal static int itemCounterMax = 0;
		#endregion statics

		private int _totalItemsInAllODs = 0;
		public int totalItemsInAllODs
		{
			get  //read only
			{// recaluculate this every time - we create System Info once per DriveWizard app run
				//the count in each node is done once per bus connection so this block runs quickly
				_totalItemsInAllODs = 0;  //first reset 
				foreach(nodeInfo node in this.nodes)
				{
					if(node.EDSorDCFfilepath != "")  //igonre Unknowns
					{
						_totalItemsInAllODs += node.objectDictionary.Count;  //then add up the length of each dictionary in each node = note htis is itmes only - subs not need at this level
					}
				}
				return _totalItemsInAllODs;
			}
		}


        //private int _itemCounter = 0;
        //public int itemCounter
        //{
        //    get  //readonly
        //    {
        //        _itemCounter = 0;
        //        foreach(nodeInfo node in this.nodes)
        //        {
        //            _itemCounter += node.itemBeingRead;
        //        }
        //        return _itemCounter;
        //    }
        //}

		#region communications class instance used to talk on the CAN via the USB adapter
		/// <summary>communications class instance used to talk on the CAN via the USB adapter</summary>
		public  communications		CANcomms;// = new communications(this);
		#endregion

		#region class instances represeting current system nodes, DCFs and a nodes in programming mode
		///<summary>Node class instances to represent each node found on the connected CANbus system.</summary>
		public  nodeInfo	[]		nodes = new nodeInfo[0];

		///<summary>A node class instance to represent data dictionary and parameter.
		///DCFNode inherits from nodeInfo since it is now just a specialised node
		///values found in a DCF file.</summary>
		public	DCFNode			DCFnode; 
		public  ArrayList       MonNodesAL;

		#endregion
		
		// delegate function handle to notify the Main Window GUI of an emergency or heartbeat event

		#region event IDs for logs
		/* Used for logs, read from a text file it contains the description as a text string
		 * associated with each event ID or fault ID retrieved from the controller.
		 */
		private SortedList eventIDList = new SortedList(); //will have length of zero - prevents exception
        public ArrayList sevconSectionIDList = new ArrayList();
        public ArrayList sevconObjectIDList = new ArrayList();
		#endregion

        //DR38000256 product descriptions read from ProductIDs.xml (future extensibility)
        public SevconProductDescriptions sevconProductDescriptions = null;
		private StateChangeListener notifyGUI = null;
		private bool NMTStateChangeRequest = false;
		#endregion

		#region System level commonly used odITems
		internal ODItemData syncTimeOdSub = null;
		#endregion System level commonly used odITems
		public ArrayList ignoreEmerFromNodeID = new ArrayList();
		public ArrayList COBsInSystem  = new ArrayList();
		internal bool conversionOK = true;
        internal int insertPoint = 0;
        private bool eventIDUpdated = false;
		#region property declarations
		
		#region Is the Bus Master a SEVCON application
		private bool _sevconAppIsMaster = false;
		public bool sevconAppIsMaster
		{
			get 
			{
				_sevconAppIsMaster = false;
				foreach ( nodeInfo thisNode in nodes )
				{
					if (( thisNode.isSevconApplication() == true ) 
						&& ( thisNode.masterStatus == true))
					{
						_sevconAppIsMaster = true;
						break;
					}
				}
				return _sevconAppIsMaster;
			}
		}
		#endregion Is the Bus Master a SEVCON application

		#region system access level (calculated after logging in to Sevcon nodes)
		private  uint	_systemAccess = 0; 
		///<summary>indicates the overall system access property (lowest access level of all Sevcon nodes awarded)</summary>
		public   uint	systemAccess 
		{
			get 
			{
				uint systemLevel = 255;
				_systemAccess = 0;
				// recalc the system access level which is the lowest of all accesses
				foreach ( nodeInfo thisNode in nodes )
				{
					if ( thisNode.isSevconApplication() == true )
					{
						if ( systemLevel > thisNode.accessLevel )
						{
							systemLevel = thisNode.accessLevel;
						}
					}
				}
				if ( systemLevel <= 5 )
				{
					_systemAccess = systemLevel;
				}
				else
				{
					_systemAccess = 0;
				}
				return ( _systemAccess ); 
			}
		}

		#endregion

		#region progress for writing a node's entire object dictionary
		private int _itemBeingWritten;
		///<summary>indicates progress when writing the entire OD to a controller</summary>
		public int itemBeingWritten
		{
			get
			{
				return ( _itemBeingWritten );
			}

			set
			{
				_itemBeingWritten = 0;
			}
		}

		#endregion

		#region nodes in current found system which have a valid EDS
		private int _noOfNodesWithValidEDS = 0;
		///<summary>number of nodes with a valid EDS</summary>
		public  int noOfNodesWithValidEDS
		{
			get 
			{
				return ( _noOfNodesWithValidEDS ); 
			}
		}

		#endregion

		#endregion
		
		#region variable declarations (not implemented yet for DW)
		private  uint	_accessFilter;
		///<summary>not implemented for DW at present</summary>
		public   uint	accessFilter
		{
			get 
			{
				return _accessFilter; 
			}
			set
			{
				_accessFilter = value;
			}
		}

		private  String	_language = "English"; //temp forced value
		///<summary>not implemented for DW at present</summary>
		public   String	language
		{
			get 
			{
				return _language; 
			}
			set
			{
				_language = value;
			}
		}
		#endregion

		#region constructor & destructor
		//-------------------------------------------------------------------------
		//  Name			: constructor
		//  Description     : Specific action required when constructing an
		//					  instance of the SystemInfo class ie read the Event ID
		//					  file and setup the heartbeat and emergency message
		//					  delegates.
		//  Parameters      : None
		//  Used Variables  : None
		//  Preconditions   : None
		//  Post-conditions : None
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>system information class constructor.</summary>		
        public SystemInfo()
        {
            this.CANcomms = new communications(this);
            //we can safely do for and foreach loops after this point
            //even if no nodes are found - reduces code in loops - imprvoes performance 
            // and reduces possiblity of exceptions
            readEventIDFile(out eventIDList);
            readSevconProductsFile();       //DR38000256
            addSevconSectionAndObjectEnumsToIDLists(out sevconSectionIDList, out sevconObjectIDList);
            this.DCFnode = new DCFNode(this, MAIN_WINDOW.DCFTblIndex);
        }

		//-------------------------------------------------------------------------
		//  Name			: destructor
		//  Description     : Specific action required when destroying an instance
		//					  of the SystemInfo class.  In this case, the event &
		//					  fault ID text file is updated if required (thus 
		//					  ensuring it is performed only once). The existing
		//					  system found on the CAN is saved to file.
		//  Parameters      : None
		//  Used Variables  : bool eventIDUpdated	- true if new event/fault ID
		//											  been uploaded from controller
		//  Preconditions   : None
		//  Post-conditions : event ID text file is updated if required.
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>system information class destructor.</summary>	
        public void Dispose()
        {
            /*
             * If the event IDs sorted list was updated during the DW run, then the text
             * file containing the string descriptions for all fault & event IDs must be 
             * updated with the newest acquistions read from the controller.
             * Done here so only done once.
             */
            if (eventIDUpdated == true)
            {
                writeEventIDFile();
            }

            this.CANcomms.VCI.closeCANAdapterHW();
        }
		#endregion

		#region delegate function definitions

		//-------------------------------------------------------------------------
		//  Name			: addStateChangeListener()
		//  Description     : A delegate function must be called when systemInfo receives
		//					  a heartbeat or emergency message.  This is used to 
		//					  use the interpreted data to update the GUI and inform 
		//					  the user.
		//  Parameters      : guiListener - indicates which function has to be called on the
		//						heartbeat or emergency event
		//  Used Variables  : notifyGUI - keeps a copy of guiListener 
		//  Preconditions   : guiListener has been created and there the delegate function
		//					  it points to has been defined elsewhere in the code
		//  Post-conditions : notifyGUI set to indicate the function to be called
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>Sets up which function must be called when a heartbeat message is received.</summary>
		/// <param name="guiListener">indicates which function has to be called on the heartbeat or emergency event</param>
		public void addStateChangeListener( StateChangeListener guiListener )
		{
			notifyGUI = guiListener;
		}

		//-------------------------------------------------------------------------
		//  Name			: onNewHeartbeat()
		//  Description     : Delegate function called when the VCI receives a new
		//					  heartbeat message.  This function extracts the node I D
		//					  from the COBID, checks it is a valid node on the system 
		//					  then updates the nodeInfo nodeState from the NMT newState
		//					  data extracted from the heartbeat message.
		//  Parameters      : COBID - COBID of the heartbeat message just received
		//					  newState - NMT state extracted from the heartbeat message
		//  Used Variables  : nodes[] - array of objects which contains the last
		//						known state of the nodes on the system
		//  Preconditions   : The CAN adapter is initialised and running and a heartbeat
		//					  message was received by the VCI receive module which extracted
		//					  the COBID and newState data from the message.
		//  Post-conditions : nodeState of the node represented by COBID has been updated
		//					  and a user indication of the node's change of state
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>Analyses any new heartbeat messages received to update a node's current status.</summary>
		/// <param name="COBID">COBID of the heartbeat message just received</param>
		/// <param name="newState">NMT state extracted from the heartbeat message</param>
		public void onNewHeartbeat( int COBID, int newState )
		{
			#region local variable declaration and variable initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			int node = COBID - SCCorpStyle.COBIDToNodeIDOffsetHeartbeat;
			int CANNodeIndex = 0;
			NodeState newNodeState = NodeState.Unknown;
			
			#endregion

			#region if the node no is valid & the system's found then extract the node state from the heartbeat data
			if   ( node >= 0 )  
			{
				// check it's for a node number already found on the system
				try
				{
					fbc = getNodeNumber( node, out CANNodeIndex );
				}
				catch{} //needed - we look at heartbeats BEFROE we have full created each nodeInfo

				if  ( fbc == DIFeedbackCode.DISuccess ) 
				{
					#region handle heartbeat from existing node
					#region if node guarding instead of heartbeats, remove the toggle bit from the state
					if ( newState >= 0x80 )
					{
						newState -= 0x80;
					}
					#endregion

					#region convert from integer data to enumerated NodeState type
					switch ( newState )
					{
						case 0x00: newNodeState = NodeState.Bootup; break;
						case 0x04: newNodeState = NodeState.Stopped; break;
						case 0x05: newNodeState = NodeState.Operational; break;
						case 0x7f: newNodeState = NodeState.PreOperational; break;
						default: newNodeState = NodeState.Unknown; break;
					}
					#endregion

					#region determine the new nodeState
					// if the node state changed then update nodeInfo and indicate to the user
					//judetemp GUI needs to see two conscutive bootup messages for the same node since
					// this could be a node coming out of bootloader into application eg if user has
					//commanded exot bootlaoder via 0x5400 sub 0 - so ALWAYS flag bootup messages to GUI
					if ( (nodes[ CANNodeIndex ].nodeState == NodeState.Bootup) ||
						(nodes[ CANNodeIndex ].nodeState != newNodeState )
						||(this.NMTStateChangeRequest == true))
					{
						nodes[ CANNodeIndex ].nodeState = newNodeState;
						// notify the GUI so it can update the display to notify the user
						if ( notifyGUI != null )
						{
							notifyGUI( COBIDType.ProducerHeartBeat, node, CANNodeIndex, newNodeState, "" );
						}
						if(this.NMTStateChangeRequest == true)
						{//only reset once we have handled this specific heartbeat
							NMTStateChangeRequest = false;
						}
					}
					#endregion determine the new nodeState
					#endregion handle heartbeat from existing node
				}
				else if(fbc == DIFeedbackCode.DIInvalidNodeNumber) 
				{
					if(MAIN_WINDOW.isVirtualNodes ==false)
					{ //ignore heartbeats form real nodes that ar enot also virtual in mixed systems
						#region handle heartbeat from a new node that has joined the bus
						//this.nodes.Length is used to denote that the node has joined at the end of current
						//list - in reality GUI will force reconnection
						if ( notifyGUI != null )
						{
							notifyGUI( COBIDType.ProducerHeartBeat, node, this.nodes.Length, NodeState.Bootup, "" );
						}
						#endregion handle heartbeat from a new node that has joined the system
					}
				}
			}
			#endregion
		}

		//-------------------------------------------------------------------------
		//  Name			: onNewEmergency()
		//  Description     : Delegate function called when the VCI receives a new
		//					  EMCY message.  This function extracts the data bytes
		//					  from the CAN message and converts the error codes into
		//					  a message meaningful to the user.
		//  Parameters      : data[] - 8 data bytes from the CAN EMCY message
		//  Used Variables  : None
		//  Preconditions   : The CAN adapter is initialised and running and an EMCY
		//					  message was received by the VCI receive module which extracted
		//					  the associated data bytes from the message.
		//  Post-conditions : The user is warned of what the EMCY message means.
		//  Return value    : None
		//----------------------------------------------------------------------------
		///<summary>Analyses any new emeregency messages received to update the user.</summary>
		/// <param name="data">8 data bytes from the CAN EMCY message</param>
		public void onNewEmergency( byte [] data )
		{
			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				return;  //shouldn't be able to get - but managed it!!
			}
			#region local variable declarations and initialisation
			int errorCode;
			int errorRegister;
			UInt16 faultID;
			EMCYMessagesMotor motorErrorCode;
			EMCYMessagesGeneral generalErrorCode;
			EMCYMessagesIO ioErrorCode;
			string IOErrorCodeString = "";
			string motorErrorCodeString = "";
			string generalErrorCodeString = "";
			string lowerString = "";
			string errorRegString= "";
			string faultString = "";
			string emergencyErrorString = "";
			char [] letters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
								  'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
			int emcyCOBID = 0;
			int emcyNode = -1;
			#endregion

			#region try and ascertain which node generated this emergency message (affects the translation)
			if ( data.Length >= 9 )
			{
				emcyCOBID = data[ 8 ];
				for ( int nodeNum = 0; nodeNum < nodes.Length; nodeNum++ )
				{ //this needs to be in a try - if the node kicks out an emergeny in response to login attempt then 
					//nodes.Length gets set to zero between these two lines - I've seen it happen
					try
					{
						if ((nodes[ nodeNum ] != null) && (nodes[ nodeNum ].emergencyodSub != null) &&  ( emcyCOBID == nodes[ nodeNum ].emergencyodSub.currentValue ))//we locate this once and keep a pointer to it
						{
							emcyNode = nodeNum;
							if(ignoreEmerFromNodeID.Contains(nodes[emcyNode].nodeID))
							{
								return;  //do nothing
							}
							break;
						}
					}
					catch{}
				}
			}
			#endregion

			// check EMCY has expected data length
			if ( data.Length == ( SCCorpStyle.CANMessageDataLengthMax + 1 ) )
			{
				#region translate the CANOpen error code to a string
				errorCode = ( data[ 1 ] << 8 ) + data[ 0 ];

				#region try converting it to a motor emergency message
				motorErrorCode = ( EMCYMessagesMotor )errorCode;
				motorErrorCodeString = motorErrorCode.ToString();
				lowerString = motorErrorCodeString.ToLower();

				if ( lowerString.IndexOfAny( letters ) == -1 )
				{
					motorErrorCodeString = "";

					if ( 
						( errorCode > EMCYMessagesMotor.ManufacturerSpecific0.GetHashCode() ) &&
						( errorCode < EMCYMessagesMotor.ManufacturerSpecific255.GetHashCode() )
						)
					{
						int code = errorCode - EMCYMessagesMotor.ManufacturerSpecific0.GetHashCode();
						motorErrorCodeString = "ManufacturerSpecific" + code.ToString();
					}
						
				}
				#endregion

				#region try to convert to an IO EMCY message
				ioErrorCode = ( EMCYMessagesIO )errorCode;
				IOErrorCodeString = ioErrorCode.ToString();
				lowerString = IOErrorCodeString.ToLower();

				if ( lowerString.IndexOfAny( letters ) == -1 )
				{
					IOErrorCodeString = "";
				}

				#region try to convert to a general EMCY message only if other attempts failed
				if ( ( motorErrorCodeString == "" ) && ( IOErrorCodeString == "" ) )
				{
					errorCode = errorCode >> 8;
					generalErrorCode = ( EMCYMessagesGeneral )errorCode;
					generalErrorCodeString = generalErrorCode.ToString();
					lowerString = generalErrorCodeString.ToLower();

					if ( lowerString.IndexOfAny( letters ) == -1 )
					{
						generalErrorCodeString = "";
					}
				}
				#endregion
				#endregion
				#endregion

				#region Sevcon nodes convert the error register to a string, one string per bit set (as can be ORed together)
				if ( ( emcyNode != -1 ) && ( nodes[ emcyNode ].isSevconDevice() == true ) )
				{
					errorRegister = (int)data[ 2 ];
					errorRegString = "";

					for ( int i = 1; i <= ErrorRegister.ManufacturerSpecific.GetHashCode(); i =  i * 2  )
					{
						int val = errorRegister & i;

						if ( val > 0 )
						{
							errorRegString += ( ( ErrorRegister )val ).ToString() + " ";
						}
					}
				}
				#endregion

				#region Sevcon nodes convert fault ID to a string by checking the event ID list
				if ( ( emcyNode != -1 ) && ( nodes[ emcyNode ].isSevconDevice() == true ) )
				{
					faultID = (UInt16)( ( ( data[ 4 ] << 8 ) + data[ 3 ] ) & 0x7fff );
					int faultIDIndex = eventIDList.IndexOfKey( faultID );
			
					// already known eventID so get string from array list
					if ( faultIDIndex != -1 )
					{
						faultString = eventIDList.GetByIndex( faultIDIndex ).ToString();
					}
				}
				#endregion

				#region build up final emergency string message from relevant contingent parts
				if ( motorErrorCodeString != "" )
				{
					emergencyErrorString += "CAN motor error code: " + motorErrorCodeString + "\n";
				}

				if ( IOErrorCodeString != "" )
				{
					emergencyErrorString += "CAN IO error code: " + IOErrorCodeString + "\n";
				}

				if ( generalErrorCodeString != "" )
				{
					emergencyErrorString += "CAN general error code: " + generalErrorCodeString + "\n";
				}
				
				if ( errorRegString != "" )
				{
					emergencyErrorString += "Error register: " + errorRegString + "\n";
				}

				if ( faultString != "" )
				{
					emergencyErrorString += "Fault ID: " + faultString + 
						"\nData bytes: 0x" + data[ 5 ].ToString("X") + " 0x" + data[ 6 ].ToString("X") + " 0x" + data[ 7 ].ToString("X");
				}
				#endregion

				#region notify the GUI so it can update the display to notify the user
				if ( notifyGUI != null )
				{

					if ( emcyNode != -1 )
					{//it doesn't matter which emergeny messag etpy ewe use here - the distingtion is for COB set up only
						notifyGUI( COBIDType.EmergencyNoInhibit, nodes[ emcyNode ].nodeID, emcyNode, NodeState.Unknown, emergencyErrorString );
					}
					else
					{//it doesn't matter which emergeny messag etpy ewe use here - the distingtion is for COB set up only

						//				//do best guess based on COBID
						int calculatedNodeID = 0;
						if((emcyCOBID >0x80) ||( emcyCOBID <=0xFF)) //nominal emergency COBID range
						{
							calculatedNodeID = emcyCOBID - 0x80;
						}
						notifyGUI( COBIDType.EmergencyNoInhibit, calculatedNodeID, 99, NodeState.Unknown, emergencyErrorString );
					}

				}
				#endregion
			}
		}

		#endregion

		#region clearing, finding, connecting & logging onto system or node & forcing/releasing pre-op
		//-------------------------------------------------------------------------
		//  Name			: listenInForBaudRate()
		//  Description     : This function passively listens in at each of the CANopen
		//					  defined baud rates to see if there is any traffic to
		//					  allow the system baud rate to be detected non-invasively.
		//  Parameters      : None
		//  Used Variables  : _noOfNodes - set to zero to clear out any previously found
		//								   systems
		//  Preconditions   : None
		//  Post-conditions : _baudRate - set to found baud rate or to _uknown if
		//								  it couldn't be ascertained
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Listens for valid CAN comms traffic for 500ms at each valid baud rate.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>5
        public void listenInForBaudRate()
        {
            this.ignoreEmerFromNodeID.Clear();
            if (MAIN_WINDOW.isVirtualNodes == true)
            {
                CANcomms.systemBaud = BaudRate._1M;
            }
            else
            {
                CANcomms.detectBaud();
            }
        }
		
		//-------------------------------------------------------------------------
		//  Name			: findSystemAtUserBaudRate()
		//  Description     : Uses an intrusive manner to try and find the nodes on
		//					  the connected system at the baud rate selected by the
		//					  user. For the nodes found, a matching EDS file is found
		//					  and read in to populate the object dictionary associated
		//					  with the node.
		//  Used Variables  : _noOfNodes - set to 0 to clear out any previously
		//								   found systems
		//					  _baudRate - set equal to _userBaudRate to store baud rate
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//  Preconditions   : The user must have selected a _userBaudRate.
		//  Post-conditions : nodes - must instantiate req'd found nodes & contain 
		//							  node ID, device info, EDSfilename of matching
		//							  EDS file, master status & manufacturer ID.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Trys to find the system connected at the user selected baud rate.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public void findSystemAtUserBaudRate()
		{
			/* Transmit an error register message request to all possible node IDs
			 * at the user selected baud rate.
			 */
			#region determine CANcomms.nodeList
			if ( MAIN_WINDOW.isVirtualNodes == true )
			{
				#region virtual nodes
				CANcomms.nodeList = new int[ SCCorpStyle.VirtualCANnodes.Count];
				for (int i = 0;i< SCCorpStyle.VirtualCANnodes.Count;i++)
				{
					VPCanNode cannode = (VPCanNode) SCCorpStyle.VirtualCANnodes[i];
					CANcomms.nodeList[i] = (int) cannode.nodeid;
				}
				#region search for duplicate nodes
				for(int i = 0;i<(CANcomms.nodeList.Length-1);i++)
				{
					if((CANcomms.nodeList[i]<128) && (CANcomms.nodeList[i]>0)) //we use out of range defaults for virtual node set up
					{
						for(int j = (i+1);j<CANcomms.nodeList.Length;j++)
						{
							if(CANcomms.nodeList[i] == CANcomms.nodeList[j])
							{
                                SystemInfo.errorSB.Append("\n Duplicate node ID:" + CANcomms.nodeList[i].ToString());
								break;
							}
						}
					}
				}
				#endregion search for duplicate nodes
				#endregion virtual nodes
			}
			else
			{
                CANcomms.findNodeIDs();
			}
			#endregion determine CANcomms.nodeList

				// If any nodes were found
            if (CANcomms.nodeList.Length > 0)
            {
                createNodesFromEDS();
                //heartbeat  and emergency handling looks at node[i].nodeID 
                //- so we should never invoke it prior to setting up the connected nodes
                CANcomms.addNewHeartbeatDelegate(new onNewHeartbeat(onNewHeartbeat));
                CANcomms.addNewEmergencyDelegate(new onNewEmergency(onNewEmergency));
                CANcomms.VCI.startHbeatAndEmerThreadTimer();
                if (MAIN_WINDOW.isVirtualNodes == true)
                {
                    for (int i = 0; i < SCCorpStyle.VirtualCANnodes.Count; i++)
                    {
                        VPCanNode cannode = (VPCanNode)SCCorpStyle.VirtualCANnodes[i];
                        nodes[i].masterStatus = cannode.master;
                    }
                }
            }
			#region read all emcy COBIDs for use with emergency messages
				foreach ( nodeInfo node in nodes )
				{
					if (node.manufacturer != Manufacturer.UNKNOWN)
					{
						MAIN_WINDOW.appendErrorInfo = false; //we often see emergencies before we have logged in - do NOT flag error
						ODItemData emerCOBIDSub = node.getODSub(SCCorpStyle.EmcyCOBIDItem, 0);
						if((emerCOBIDSub != null) && (MAIN_WINDOW.isVirtualNodes == false))
						{
							node.readODValue( emerCOBIDSub ); //may no have loggged in yet - so hold off flagging the error
							MAIN_WINDOW.appendErrorInfo = true; //ensure it is back on
						}
						MAIN_WINDOW.appendErrorInfo = true; //ensure it is back on
					}
				}
			#endregion

			setSevconMasterToBeFirstCANnodeinNodesArray();
		}

		private void setSevconMasterToBeFirstCANnodeinNodesArray()
		{
			//at this point we need to 'shuffle' nodes to force any sevocn master to the top of the list
			nodeInfo [] temp = new nodeInfo[nodes.Length];
			int masterIndex = -1;
			for (int i = 0;i< this.nodes.Length;i++)
			{
				if((this.nodes[i].isSevconApplication() == true) 
					&& (this.nodes[i].masterStatus == true))
				{
					temp[0] = this.nodes[i];
					masterIndex = i;
					break;
				}
			}
			if(masterIndex == -1)
			{
				return; //no Sevon master s 
			}
			for (int i = 1;i<this.nodes.Length;i++)
			{
				if(i<masterIndex)
				{
					temp[i] = this.nodes[i-1];
				}
				else if( i == masterIndex)
				{
					continue;//skip the first Sevcon master we saw
				}
				else
				{
					temp[i] = this.nodes[i];
				}
			}
		}


		//-------------------------------------------------------------------------
		//  Name			: loginToSystem()
		//  Description     : Takes the userID and password and logs in to all Sevcon
		//					  nodes on the connected CAN system.  The overall lowest
		//					  awarded access level of all the Sevcon nodes is taken
		//					  as the system access level.
		//                    userID - user selected ID
		//					  password - user entered password
		//  Used Variables  : nodes[] - defines the connected CAN system
		//					  nodes[].accessLevel - access level awarded after logon
		//											for each node
		//  Preconditions   : The user must have entered a userID and password and the
		//					  system must have been found (i.e. nodes[] populated with
		//					  the EDS file read to construct the object dictionary).
		//  Post-conditions : All Sevcon nodes are logged into and the _systemAccess
		//					  property is set to the lowest of all awarded access levels.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Logs onto all devices that are Sevcon nodes using the userID and password.</summary>
		/// <param name="userID">user selected ID</param>
		/// <param name="password">user entered password</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode loginToSystem( uint userID, uint password )
		{
			DIFeedbackCode	fbc = DIFeedbackCode.DIFailedToLogonToDevice;
			byte			systemLevel = 255;		// start with an invalid value
			bool			failedToLogon = false;
			bool			failedToCheckForMaster = false;
			foreach ( nodeInfo thisNode in nodes )
			{
				// Only log on to those nodes that are Sevcon devices, not in boot mode
				if ( thisNode.isSevconApplication() == true )
				{	
					if ( MAIN_WINDOW.isVirtualNodes == true )
					{
						fbc = thisNode.logonToDevice( 0,password);
					}
					else
					{
                        //DR38000256 calibrator handling
						if(((SCCorpStyle.ProductRange)thisNode.productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY)
                            || ((SCCorpStyle.ProductRange)thisNode.productRange == SCCorpStyle.ProductRange.CALIBRATOR))
						{
							//TODO judetemp - display currently has no login object - so assum level 5
							thisNode.accessLevel = 5;
							fbc = DIFeedbackCode.DISuccess;
						}
						else
						{
							fbc = thisNode.logonToDevice( userID, password );
						}
					}

					#region Take the lowest of all the node access levels as the system access level.
					if ( fbc == DIFeedbackCode.DISuccess )
					{
						if ( systemLevel > thisNode.accessLevel )
						{
							systemLevel = thisNode.accessLevel;
						}

						fbc = thisNode.readMasterStatus();

						if ( fbc != DIFeedbackCode.DISuccess )
						{
							failedToCheckForMaster = true;
						}

						#region read this node's emcy COBIDs (for use with emergency messages)
						if ( fbc == DIFeedbackCode.DISuccess )
						{
							ODItemData emerCOBIDSub = thisNode.getODSub(SCCorpStyle.EmcyCOBIDItem, 0);
							if(emerCOBIDSub != null)
							{
								fbc = thisNode.readODValue( emerCOBIDSub);
							}
						}
						#endregion
					}
					else
					{
						failedToLogon = true;
					}
					#endregion Take the lowest of all the node access levels as the system access level.
				}
			}

			#region determine system access
			/* If failed to logon to any Sevcon device then take the lowest
					 * access level of those nodes successfully logged onto.  If no
					 * Sevcon nodes were logged onto (ie _systemAccess > 5) then take
					 * the system access level as 0 (lowest available for Sevcon nodes).
					 */
			if ( failedToLogon == false )
			{
				fbc = DIFeedbackCode.DISuccess;
					
				if ( systemLevel <= 5 )
				{
					_systemAccess = systemLevel;
				}
				else
				{
					_systemAccess = 0;
				}
			}
			else if ( failedToCheckForMaster == true )
			{
				fbc = DIFeedbackCode.DIFailedToReadNodeMasterStatus;
			}
			#endregion determine system access
			return ( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: loginToNode()
		//  Description     : Allows logging in to a specific individual node on the
		//					  connected CANbus system.
		//  Parameters      : nodeID - the nodeID of the specific node to be logged into
		//					  userID - user selected ID
		//					  password - user entered password
		//  Used Variables  : nodes[] - defines the connected CAN system
		//					  nodes[].accessLevel - access level awarded after logon
		//											for each node
		//  Preconditions   : The user must have entered a userID and password and the
		//					  system must have been found (i.e. nodes[] populated with
		//					  the EDS file read to construct the object dictionary).
		//  Post-conditions : The specific node has been logged into and the access level
		//					  awarded by the controller is held in nodes[].accessLevel
		//				      and the overall systemLevel is updated.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Logs onto a single node which must be a Sevcon device with the given userID and password.</summary>
		/// <param name="nodeID">the nodeID of the specific node to be logged into</param>
		/// <param name="userID">user selected ID</param>
		/// <param name="password">user entered password</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode loginToNode( int CANNodeIndex, uint userID, uint password )
		{
			DIFeedbackCode	fbc = DIFeedbackCode.DIFailedToLogonToDevice;
			// can only logon to a node if it is a Sevcon node
			if ( nodes[ CANNodeIndex ].isSevconApplication() == true )
			{
				fbc = nodes[ CANNodeIndex ].logonToDevice( userID, password );
				_systemAccess = Math.Min(_systemAccess, nodes[ CANNodeIndex ].accessLevel);
			}
			else
			{
				SystemInfo.errorSB.Append("\nCannot login to node. Node is not running Sevcon application software");
				fbc = DIFeedbackCode.DIFailedToLogonToDevice;
			}
			return ( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: forceSystemIntoPreOpMode()
		//  Description     : For safety, it is best to put all the controllers on the
		//					  system into pre-op mode before writing any new values to
		//					  OD parameters.  This function finds the master node of
		//					  the system and provided it is a Sevcon node, requests the
		//					  system goes into pre-op mode.
		//  Used Variables  : nodes[] - defines the connected CAN system
		//  Preconditions   : The system connected must have been found (nodes[] populated).
		//  Post-conditions : If master node is a Sevcon node, system is in pre-op mode.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>If the system master node is a Sevcon device, request the preoperational state.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode forceSystemIntoPreOpMode( )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DIMasterNodeNotASevconNode;

			if(this.sevconAppIsMaster == true)
			{
				// check each node to find the Sevcon master node.
				for( int n = 0; n < nodes.Length; n++ )
				{
					// can only force pre-op if master is a Sevcon node
					if ( nodes[ n ].isSevconApplication() == true ) 
					{
						#region Sevocn master
						if( nodes[ n ].masterStatus == true )
						{
							// tx request to put system into pre-op mode.
							fbc = nodes[ n ].forceIntoPreOpMode( );
						}
							#endregion Sevocn master
                        #region Sevcon display or calibrator
                        else if (((SCCorpStyle.ProductRange) nodes[n].productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY)
                            || ((SCCorpStyle.ProductRange)nodes[n].productRange == SCCorpStyle.ProductRange.CALIBRATOR)) //DR38000256
						{
							if(MAIN_WINDOW.currentProfile.displayIsSlave == false)
							{
								fbc = nodes[n].forceIntoPreOpMode();
							}
						}
						#endregion Sevcon display or calibrator
					}
				}
				#region virtual nodes
				if (( MAIN_WINDOW.isVirtualNodes == true ) && ( fbc == DIFeedbackCode.DISuccess ))
				{  //the above ensures that vrtual nodes still fail if the 'master' is not a Sevcon application
					for( int n = 0; n < nodes.Length; n++ )
					{
						onNewHeartbeat( nodes[ n ].nodeID + 0x700, 0x7f );
					}
				}
				#endregion virtual nodes
				this.NMTStateChangeRequest = true;  //ensure we flag the next hearbeat to the GUI
			}
			else
			{
				#region all we can do is ask individual Sevcon apps to enter pre-op
				foreach(nodeInfo node in this.nodes)
				{
					if(node.isSevconApplication() == true)
					{
						fbc = node.forceIntoPreOpMode( );
					}
				}
				#region virtual nodes
				if (( MAIN_WINDOW.isVirtualNodes == true ) && ( fbc == DIFeedbackCode.DISuccess ))
				{  //the above ensures that vrtual nodes still fail if the 'master' is not a Sevcon application
					foreach(nodeInfo node in this.nodes)
					{
						if(node.isSevconApplication() == true)
						{//virtual must match what a real system would do - ie only the sevcon apps would enter pre-op
							onNewHeartbeat(node.nodeID + 0x700, 0x7f );
						}
					}
				}
				#endregion virtual nodes
				this.NMTStateChangeRequest = true;  //ensure we flag the next hearbeat to the GUI
				#endregion all we can do is ask individual Sevcon apps to enter pre-op
			}
			return ( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: releaseSystemFromPreOpMode()
		//  Description     : This function is called once all new OD parameters have
		//					  been written to the controller's OD.  This allows the
		//					  master node to return to operational when it deems it
		//					  suitable.
		//  Used Variables  : nodes[] - defines the connected CAN system
		//  Preconditions   : All OD parameter writes to the controller's OD are completed.
		//  Post-conditions : If the master is a Sevcon node, the request for pre-op mode
		//					  has been cleared, allowing the master to return the system
		//					  to the operational mode when it is OK to do so.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>If the system master is a Sevcon device, request operational mode.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode releaseSystemFromPreOpMode( )
		{
			#region local params
			DIFeedbackCode fbc = DIFeedbackCode.DIMasterNodeNotASevconNode;
			#endregion local params

			if(this.sevconAppIsMaster == true)
			{
				#region Master is Sevcon App
				// Check each node on the connected system to find the Sevcon master node.
				foreach(nodeInfo CANnode in this.nodes)
				{
					//pretent to release System from pre-op
					if ( MAIN_WINDOW.isVirtualNodes == true )
					{
						onNewHeartbeat(CANnode.nodeID + 0x700, 0x05 );
					}
					// can only force pre-op if master is a Sevcon node
					if ( CANnode.isSevconApplication() == true )
					{
						#region Sevcon master
						if( CANnode.masterStatus == true )
						{
							// clear the request for the pre-op state
							fbc = CANnode.releaseFromPreOpMode( );
						}
							#endregion Sevcon master
							#region Sevcon display
						else if (((SCCorpStyle.ProductRange) CANnode.productRange == SCCorpStyle.ProductRange.SEVCONDISPLAY)
                            || ((SCCorpStyle.ProductRange)CANnode.productRange == SCCorpStyle.ProductRange.CALIBRATOR)) //DR38000256
						{
							if(MAIN_WINDOW.currentProfile.displayIsSlave == false)
							{
								fbc = CANnode.releaseFromPreOpMode();
							}
						}
						#endregion Sevcon display
					}
				}
				#endregion Master is Sevcon App
			}
			else
			{ 
				#region Master ( if any) is not a Sevcon app - just release the Sevcon nodes
				foreach(nodeInfo CANnode in this.nodes)
				{
					if(CANnode.isSevconApplication() == true)
					{
						fbc = CANnode.releaseFromPreOpMode( );
						if ( MAIN_WINDOW.isVirtualNodes == true )
						{
							onNewHeartbeat( CANnode.nodeID + 0x700, 0x05 );
						}
					}
				}
				#endregion Master ( if any) is not a Sevcon app - just release the Sevcon nodes
			}
			this.NMTStateChangeRequest = true;  //ensure we flag the next hearbeat to the GUI
			return ( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: clearSystem()
		//  Description     : Clears the number of nodes on the system (used by GUI
		//					  to find the system again, typically after programming).
		//  Parameters      : None
		//  Used Variables  : nodes[] - defines the connected CAN system
		//					  _noOfNodes - number of nodes on the system
		//  Preconditions   : None
		//  Post-conditions : Any previously found nodes on the system have been cleared.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Clears out any previously found systems that were connected on the CANbus.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode clearSystem()
		{
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;

			CANcomms.nodeList = new int[0];
			_noOfNodesWithValidEDS = 0;
			nodes = new nodeInfo[0];
			_systemAccess = 0;
			this.ignoreEmerFromNodeID.Clear();
			return ( fbc );
		}
		
		//-------------------------------------------------------------------------
		//  Name			: refindLastSystem()
		//  Description     : This system refinds the last connected, caches CANbus
		//					  system by reading the baud rate and the node numbers
		//					  from the dw.ini file.  This is then used to connect
		//					  to these real nodes on the bus to read the device
		//					  information object (0x1018) to match the EDS and construct
		//					  the replica object dictionary for each node.
		//					  THIS WILL NOT FIND ANY NEW NODES ADDED TO THE CANbus
		//					  SINCE THE SYSTEM WAS CACHED.
		//	Used Variables  : _baudRate - set equal to _userBaudRate to store baud rate
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//					  _noOfNodes - number of nodes on the system
		//  Preconditions   : The user is trying to reconnect to a previous system.
		//  Post-conditions : The previous system if refound and the OD's 
		//					  constructed to their EDS.  The baud rate and node
		//					  numbers on the system were checked to be valid.
		//  Return value    : fbc - success code or failure reason
		//----------------------------------------------------------------------------
		///<summary>Tries to redetect the last connected system which was cached.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
        public void refindLastSystem()
        {
            #region local variable declarations
            DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
            #endregion

            SystemInfo.itemCounter1 = 0;


            int[] nodeList = new int[MAIN_WINDOW.currentProfile.myCANNodes.Count];  //judetemp - get these from profile
            for (int i = 0; i < MAIN_WINDOW.currentProfile.myCANNodes.Count; i++)
            {
                VPCanNode cannode = (VPCanNode)MAIN_WINDOW.currentProfile.myCANNodes[i];
                nodeList[i] = (int)cannode.nodeid;
            }
            #region check the baud rate and nodes and setup
            if (nodeList.Length == 0)
            {
                fbc = DIFeedbackCode.DINoExistingSystemToRefind;
                return;
            }
            CANcomms.nodeList = nodeList;
            #endregion

            #region check the nodes are still there as expected
            ArrayList nodesFound = new ArrayList();

            if (MAIN_WINDOW.isVirtualNodes == true)
            {
                #region virtual nodes
                SystemInfo.itemCounterMax = MAIN_WINDOW.currentProfile.myCANNodes.Count;
                for (int node = 0; node < CANcomms.nodeList.Length; node++)
                {
                    nodesFound.Add(CANcomms.nodeList[node]);  //we always find virtual nodes
                    SystemInfo.itemCounter1++;
                }
                #endregion virtual nodes
            }
            else
            {
                #region real nodes
                // initialise & start IXXAT adapter at required baud rate to tx/rx SDOs
                byte[] rxData = new byte[1] { 0 };
                SystemInfo.itemCounterMax = MAIN_WINDOW.currentProfile.myCANNodes.Count * SCCorpStyle.SDONoResponseRetries;
                for (ushort _nodeUnderTest = 0; _nodeUnderTest < CANcomms.nodeList.Length; _nodeUnderTest++)
                {
                    for (ushort i = 0; i < SCCorpStyle.SDONoResponseRetries; i++)
                    {
                        //DR38000239 JW method name changed for clarity and simplified
                        fbc = CANcomms.checkIfNodeConnected(CANcomms.nodeList[_nodeUnderTest], ref rxData, true);
                        if (fbc != DIFeedbackCode.DINoResponseFromController)
                        {
                            break;
                        }
                        SystemInfo.itemCounter1++;
                    }

                    if (fbc == DIFeedbackCode.DISuccess)
                    {
                        nodesFound.Add(CANcomms.nodeList[_nodeUnderTest]);
                    }
                    else if (fbc == DIFeedbackCode.DINoResponseFromController)
                    {
                        SystemInfo.errorSB.Append("\nUnable to find node ID ");
                        SystemInfo.errorSB.Append(CANcomms.nodeList[_nodeUnderTest].ToString());
                    }
                }

                #endregion real nodes
            }

            #endregion  check the nodes are still there as expected
            CANcomms.nodeList = new int[nodesFound.Count];
            for (int i = 0; i < nodesFound.Count; i++)
            {
                CANcomms.nodeList[i] = (int)nodesFound[i];
            }
            #region define each node from it's EDS
            if ((fbc == DIFeedbackCode.DISuccess) || (nodesFound.Count > 0))
            {
                createNodesFromEDS();
            }
            #endregion

            setSevconMasterToBeFirstCANnodeinNodesArray();  //B&B
            #region read all emcy COBIDs for use with emergency messages
            if (fbc == DIFeedbackCode.DISuccess)
            {
                foreach (nodeInfo node in nodes)
                {//the following is applicable for third party units only
                    if ((node.isSevconDevice() == false) && (node.EDSorDCFfilepath != ""))
                    {
                        ODItemData emerCOBIDSub = node.getODSub(SCCorpStyle.EmcyCOBIDItem, 0);
                        if (emerCOBIDSub != null)
                        {
                            fbc = node.readODValue(emerCOBIDSub);
                        }
                    }
                }
            }
            #endregion
            return;
        }
		#endregion

		#region DCF related
		//-------------------------------------------------------------------------
		//  Name			: writePartialDCFNodeToDevice()
		//  Description     : Writes part of the DCF file stored in DCFnode to
		//					  the device of nodeID. Which items to be written are 
		//					  listed in the userObjectList.
		//  Parameters      : nodeID - the node ID of the device to have it's OD updated
		//					  CANNodeIndex - index into nodes[] array for this nodeID
		//					  userObjectList - array of odItem which lists the index and
		//							sub-index of all the items required to be updated
		//  Used Variables  : DCFnode - contains the OD constructed from the DCF file,
		//						which contains all the values to be written to the device.
		//  Preconditions   : The device of nodeID is already found on the system, the
		//				      DCF file has been read into DCFnode and there has been a
		//					  check made that these both refer to the same device type
		//					  ie same vendor, product code and product revision.
		//  Post-conditions : The device of nodeID has it's OD updated with all those
		//					  items that can be written using a DCF (obj flag) and
		//					  that the user has the required access to write to.
		//  Return			: feedback indicates success or gives a failure reason
		//----------------------------------------------------------------------------
		/// <summary>Writes part of the DCF file stored in DCFnode to the device of
		/// nodeID.  The items to be written are listed in the userObjectList.</summary>
		/// <param name="nodeID">the node ID of the device to have it's OD updated</param>
		/// <param name="CANNodeIndex">index into nodes[] array for this nodeID</param>
		/// <param name="userObjectList">array of odItem which lists the index and
		/// sub-index of all the items required to be updated</param>
		/// from the device when trying to updated the OD</param>
		/// <returns>feedback indicates success or gives a failure reason</returns>
		public DIFeedbackCode writePartialDCFNodeToDevice( nodeInfo destNode)
		{
#if DEBUGGING_DCF
			this.CANcomms.tempItemsInDCF = new ArrayList();
#endif
			#region local variable declarations and variable initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			bool canDownloadViaBackDoor = false;
			int lastIndex = 0;
			#endregion
			#region check For Empty DCF
			if ( this.DCFnode.objectDictionary.Count <= 0 )
			{
				SystemInfo.errorSB.Append("\nNo downloadable parameters in DCF store");
				return DIFeedbackCode.DISomeProblemsDownloadingDCF;
			}
			#endregion check For Empty DCF
            DCFnode.itemBeingRead = 0;      //reset for progress bar
			#region obj flags
			/* The objFlags in each objectDictionary object has flags to refuse a read
			 * or write on scan which are usually ignored but must be adhered to
			 * for a OD download.
			 */
			destNode.EDS_DCF.ignoreObjFlags = false;
			#endregion obj flags

			#region check if it is possible to write the DCF via the backdoor method
			MAIN_WINDOW.appendErrorInfo = false;
			ODItemData bdIndexSub = destNode.getODSubFromObjectType(SevconObjectType.INDIRECT_OBJECT_UPDATE, SCCorpStyle.BackDoorSubForIndex);
			ODItemData bdSubindexSub = destNode.getODSubFromObjectType(SevconObjectType.INDIRECT_OBJECT_UPDATE, SCCorpStyle.BackDoorSubForSubIndex);
			ODItemData bdNewValSub = destNode.getODSubFromObjectType(SevconObjectType.INDIRECT_OBJECT_UPDATE, SCCorpStyle.BackDoorSubForNewValue);
			ODItemData bdSeedSub  = destNode.getODSubFromObjectType(SevconObjectType.INDIRECT_OBJECT_UPDATE, SCCorpStyle.BackDoorSubForKey);
			ODItemData bdResultSub = destNode.getODSubFromObjectType(SevconObjectType.INDIRECT_OBJECT_UPDATE, SCCorpStyle.BackDoorSubForFeedbackCode);
			MAIN_WINDOW.appendErrorInfo = true;
			if (( destNode.isSevconApplication() == true ) 
				&& (DCFnode.DCFChecksumOK == true ) 
				&& (bdIndexSub != null) && (bdSubindexSub != null) && (bdNewValSub != null) 
				&& (bdSeedSub != null)&& (bdResultSub != null))
			{
				canDownloadViaBackDoor = true;
			}
			#endregion check if it is possible to write the DCF via the backdoor method
			
			foreach(ObjDictItem dcfodItem in this.DCFnode.objectDictionary)
			{
                DCFnode.itemBeingRead++;        // update for progress bar

                if (destNode.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
				{
					break;
				}
                ODItemData dcfHeaderSub = (ODItemData)dcfodItem.odItemSubs[0];
				if ( 
					((dcfHeaderSub.indexNumber>= SCCorpStyle.PDORxMappingMin) && (dcfHeaderSub.indexNumber<=SCCorpStyle.PDORxMappingMax))
					|| ((dcfHeaderSub.indexNumber>= SCCorpStyle.PDOTxMappingMin) && (dcfHeaderSub.indexNumber<=SCCorpStyle.PDOTxMappingMax))
					)
				{
					#region if object is a PDO_MAPPING, use special downloading sequence for PDO maps
					/* Can't write a partial PDO map as don't know the new sub0 number so write 
							 * the entire map. Only perform if lastIndex is not the same as this one to
							 * prevent writing the entire map for every occurrence in the userObjectList.	 */
					ObjDictItem destodItem = destNode.getODItemAndSubs(dcfHeaderSub.indexNumber);
                    //DR38000261 prevent exception
                    if (destodItem != null) //only do if found equiv item in controller being downloaded to
                    {
                        int highestSubAccessLevel = 0;
                        #region first read each sub in case DW has default zero for a parmaeter currentValue
                        foreach (ODItemData odSub in destodItem.odItemSubs)
                        {
                            if (odSub.subNumber < 0)  //header -read the num Itmes because this is number of mappings
                            {
                                continue;
                            }
                            if (odSub.accessLevel > highestSubAccessLevel)
                            {
                                highestSubAccessLevel = odSub.accessLevel;
                            }
                            destNode.readODValue(odSub);
                        }
                        foreach (ODItemData odDCFSub in dcfodItem.odItemSubs)
                        {
                            if (odDCFSub.subNumber >= 0)
                            {
                                foreach (ODItemData odDestSub in destodItem.odItemSubs) //must do seperately after ALL dest subs have been read
                                {
                                    if ((odDestSub.subNumber >= 0) && (odDCFSub.subNumber == odDestSub.subNumber))
                                    {
                                        odDestSub.currentValue = odDCFSub.currentValue; //change local copy
                                        break; //break out of inner loop
                                    }
                                }
                            }
                        }
                        #endregion first read each sub in case DW has default zero for a parmaeter currentValue

                        if ((destNode.accessLevel < SCCorpStyle.HighestAccessLevel)
                            && (canDownloadViaBackDoor == true)
                            && (highestSubAccessLevel > destNode.accessLevel)
                            && (destNode.accessLevel > 0))
                        {
                            #region use the backdoor method as insufficient access if possible
                            fbc = writePDOMappingViaBackDoor(destNode, destodItem);
                            if (destNode.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
                            {
                                break;
                            }
                            #endregion use the backdoor method as insufficient access if possible
                        }
                        else
                        {
                            #region use the normal method as no alternative or it's quicker if got required access
                            // don't try & write mapping if insufficient access level
                            if (destNode.accessLevel >= highestSubAccessLevel)
                            {
                                fbc = writePDOMapping(destNode, destodItem);
                                if (destNode.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
                                {
                                    break;
                                }
                            }
                            #endregion use the normal method as no alternative or it's quicker if got required access
                        }
                    }
                    else //DR38000261
                    {
                        fbc = DIFeedbackCode.DIInvalidIndexOrSub;
                        SystemInfo.errorSB.Append("\nCannot download object from DCF file to connected device.");
                        SystemInfo.errorSB.Append("\nObject " + dcfHeaderSub.indexNumber.ToString("x") + " was found in the DCF file does not exist in the connected node's EDS." );
                    }
					#endregion if object is a PDO_MAPPING, use special downloading sequence for PDO maps
				}
				else
				{
					foreach(ODItemData dcfSub in dcfodItem.odItemSubs)
					{
						#region header sub
						if(dcfSub.subNumber == -1)
						{
							continue;
						}
						#endregion header sub
						#region read only
						if((dcfSub.accessType == ObjectAccessType.Constant) || (dcfSub.accessType == ObjectAccessType.ReadOnly))
						{
							continue;  //we cannot write
						}
						#endregion read only
						#region non write in current NMT state
						if((destNode.nodeState != NodeState.PreOperational)
							&& (destNode.nodeState != NodeState.Unknown)
							&& ((dcfSub.accessType == ObjectAccessType.ReadReadWriteInPreOp)
							|| (dcfSub.accessType == ObjectAccessType.ReadWriteInPreOp)
							|| (dcfSub.accessType == ObjectAccessType.ReadWriteWriteInPreOp)
							|| (dcfSub.accessType == ObjectAccessType.WriteOnlyInPreOp)))
						{
							continue;
						}
						#endregion non write in current NMT state
						#region check if item exists in destination node
						ODItemData destSub =  destNode.getODSub(dcfSub.indexNumber, dcfSub.subNumber);
						if(destSub == null)
						{
                            fbc = DIFeedbackCode.DIInvalidIndexOrSub;   //DR38000261
                            SystemInfo.errorSB.Append("\nCannot download object from DCF file to connected device.");
                            SystemInfo.errorSB.Append("\nObject " + dcfSub.indexNumber.ToString("x") + " sub " + dcfSub.subNumber.ToString("x") + " was found in the DCF file does not exist in the connected node's EDS.");
							continue;			
						}
						#endregion check if item exists in destination node
						#region use normal downloading sequence
						ODItemData COBIDSubToDisable = null; //null is default
						#region See if this sub is a COBID that requires adisable write prior to normal write
						if((dcfSub.indexNumber== SCCorpStyle.TimeStampCOBIDIndex) || (dcfSub.indexNumber == SCCorpStyle.EmcyCOBIDItem))
						{
							#region mark Timestamp/Emergency/Server SDO COBID sub to be disabled(CiA requirement)
							fbc = destNode.readODValue(destSub); //get latest value
							if ( (( destSub.currentValue & SCCorpStyle.Bit31Mask ) == 0) //value on node is not disabled
								&& ((dcfSub.currentValue &  SCCorpStyle.Bit31Mask) == 0) ) //and we are not writing a disabling valaue to it
							{
								COBIDSubToDisable = destSub;  //we have to disable this before we can write to it
							}
							#endregion mark Timestamp/Emergency/Server SDO COBID sub to be disabled(CiA requirement)
						}
						else if((dcfSub.indexNumber>=SCCorpStyle.ServerSDOSetupMin) && (dcfSub.indexNumber<=SCCorpStyle.ServerSDOSetupMax)) //server SDO
						{
							if(dcfSub.subNumber == SCCorpStyle.ServerSDOTransmitCOBIDSubIndex)
							{
								#region See if SDO Server Tx COBID needs disabling before overwriting
								ODItemData TxCOBIDSub = destNode.getODSub(dcfSub.indexNumber, SCCorpStyle.ServerSDOTransmitCOBIDSubIndex);
								fbc = destNode.readODValue(TxCOBIDSub);
								if ( (( TxCOBIDSub.currentValue & SCCorpStyle.Bit31Mask ) == 0 ) //value on node is not disabled
									&& ((dcfSub.currentValue &  SCCorpStyle.Bit31Mask) == 0) ) //and we are not writing a disabling valaue to it
								{
									COBIDSubToDisable = TxCOBIDSub;
								}
								#endregion See if SDO Server Tx COBID needs disabling before overwriting

							}
							else if(dcfSub.subNumber == SCCorpStyle.ServerSDOReceiveCOBIDSubIndex)
							{
								#region See if SDO server Rx COBID needs disabling before overwriting
								ODItemData RxCOBIDSub = destNode.getODSub(dcfSub.indexNumber, SCCorpStyle.ServerSDOReceiveCOBIDSubIndex);
                                fbc = destNode.readODValue(RxCOBIDSub);
                                if (((RxCOBIDSub.currentValue & SCCorpStyle.Bit31Mask) == 0) //value on node is not disabled
                                    && ((dcfSub.currentValue & SCCorpStyle.Bit31Mask) == 0)) //and we are not writing a disabling valaue to it
                                {
                                    COBIDSubToDisable = RxCOBIDSub;
                                }
								#endregion See if SDO server Rx COBID needs disabling before overwriting
							}
						}
						else if((dcfSub.indexNumber>=SCCorpStyle.ClientSDOSetupMin) && (dcfSub.indexNumber <= SCCorpStyle.ClientSDOSetupMax))
						{
							if(dcfSub.subNumber == SCCorpStyle.ClientSDOTransmitCOBIDSubIndex)
							{
								#region See if SDO Client Tx COBID needs disabling before overwriting
								ODItemData TxCOBIDSub = destNode.getODSub(dcfSub.indexNumber, SCCorpStyle.ClientSDOTransmitCOBIDSubIndex);
								fbc = destNode.readODValue(TxCOBIDSub);
								if ( (( TxCOBIDSub.currentValue & SCCorpStyle.Bit31Mask ) == 0 ) //value on node is not disabled
									&& ((dcfSub.currentValue &  SCCorpStyle.Bit31Mask) == 0) ) //and we are not writing a disabling valaue to it
								{
									COBIDSubToDisable = TxCOBIDSub;
								}
								#endregion See if SDO Client Tx COBID needs disabling before overwriting
							}
							else if(dcfSub.subNumber == SCCorpStyle.ClientSDOReceiveCOBIDSubIndex)
							{
								#region See if SDO Client Rx COBID needs disabling before overwriting
								ODItemData RxCOBIDSub = destNode.getODSub(dcfSub.indexNumber, SCCorpStyle.ClientSDOReceiveCOBIDSubIndex);
								fbc = destNode.readODValue(RxCOBIDSub);
								if ((( RxCOBIDSub.currentValue & SCCorpStyle.Bit31Mask ) == 0 ) //value on node is not disabled
									&& ((dcfSub.currentValue &  SCCorpStyle.Bit31Mask) == 0) ) //and we are not writing a disabling valaue to it
								{
									COBIDSubToDisable =  RxCOBIDSub; 
								}
								#endregion See if SDO Client Tx COBID needs disabling before overwriting
							}
						}
						else if (((dcfSub.indexNumber >= SCCorpStyle.PDORxCommsSetupMin) && (dcfSub.indexNumber<= SCCorpStyle.PDORxCommsSetupMax))
							||((dcfSub.indexNumber>= SCCorpStyle.PDOTxCommsSetupMin) && (dcfSub.indexNumber<= SCCorpStyle.PDOTxCommsSetupMax)))
						{
							if(dcfSub.subNumber == SCCorpStyle.PDOCommsCOBIDSubIndex)
							{
								#region mark PDO COBID sub to be disabled if required
								ODItemData COBIDSub = destNode.getODSub(dcfSub.indexNumber, SCCorpStyle.PDOCommsCOBIDSubIndex);
								destNode.readODValue(COBIDSub);
								if( (( COBIDSub.currentValue & SCCorpStyle.Bit31Mask ) == 0 ) //value on node is not disabled
									&& ((dcfSub.currentValue &  SCCorpStyle.Bit31Mask) == 0) ) //and we are not writing a disabling valaue to it
								{
									COBIDSubToDisable = COBIDSub; 
								}
								#endregion mark PDO COBID sub to be disabled if required
							}
						}
						#endregion See if this sub is a COBID that requires adisable write prior to normal write
					
						//Now do the actual writing of values
						if 
							( ( destNode.accessLevel < SCCorpStyle.HighestAccessLevel ) 
							&& ( canDownloadViaBackDoor == true ) 
							&& ( destSub.accessLevel > destNode.accessLevel )
							&& ( destNode.accessLevel > 0 ))
						{
							#region use back door
							// can't do visible strings via the backdoor
							if ( dcfSub.displayType != CANopenDataType.VISIBLE_STRING )
							{
								#region write using back door
								#region disable COBID subs first as required 
								if(COBIDSubToDisable != null)
								{
									fbc = destNode.writeODValueViaBackDoor( destSub, 0x8fffffff);  //default disabled value
								}
								#endregion disable COBID subs first as required 
								fbc = destNode.writeODValueViaBackDoor( destSub, dcfSub.currentValue);
								#endregion write using back door
							}
							else
							{
								#region no backdoor for strings so access level applies
								if(dcfSub.accessLevel> destNode.accessLevel)
								{
									continue;
								}
								else
								{
									fbc = destNode.writeODValue( destSub, dcfSub.currentValueString);
								}
								#endregion no backdoor for strings so access level applies
							}
							#endregion use back door
						}
							
						else //not using back door
						{
							#region normal write to device
							#region skip items with insufficient access - we are not using back door here
							/* Call the relevant overloaded write routine dependent on displayed data type.	*/
							if(destSub.accessLevel> destNode.accessLevel)
							{
								continue;
							}
							#endregion skip items with insufficient access - we are not using back door here
							#region call correct write method accoding to the data tppe of dcfSub
							CANopenDataType datatype = (CANopenDataType)dcfSub.dataType;
							switch (datatype)
							{
								case CANopenDataType.INTEGER16:
								case CANopenDataType.INTEGER24:
								case CANopenDataType.INTEGER32:
								case CANopenDataType.INTEGER40:
								case CANopenDataType.INTEGER48:
								case CANopenDataType.INTEGER56:
								case CANopenDataType.INTEGER64:
								case CANopenDataType.INTEGER8:
								case CANopenDataType.UNSIGNED16:
								case CANopenDataType.UNSIGNED24:
								case CANopenDataType.UNSIGNED32:
								case CANopenDataType.UNSIGNED40:
								case CANopenDataType.UNSIGNED48:
								case CANopenDataType.UNSIGNED56:
								case CANopenDataType.UNSIGNED64:
								case CANopenDataType.UNSIGNED8:
								case CANopenDataType.BOOLEAN:
									#region disable COBID subs first as required
									if(COBIDSubToDisable != null)
									{ 
										fbc = destNode.writeODValue( destSub, 0x8fffffff);
									}
									#endregion disable COBID subs first as required 
									fbc = destNode.writeODValue(destSub, dcfSub.currentValue);
									break;

								case CANopenDataType.OCTET_STRING:
								case CANopenDataType.UNICODE_STRING:
								case CANopenDataType.VISIBLE_STRING:
									fbc = destNode.writeODValue(destSub, dcfSub.currentValueString);
									break;

								case CANopenDataType.REAL32:
									fbc = destNode.writeODValue(destSub, dcfSub.real32.currentValue);
									break;

								case CANopenDataType.REAL64:
									fbc = destNode.writeODValue(destSub, dcfSub.real64.currentValue);
									break;
										
								default:
									break;
							}
							#endregion call correct write method accoding to the data tppe of dcfSub
							#endregion normal write to device
						}
						#region check if 3 consecutive no repsonses and if so quit download
                        if (destNode.numConsecutiveNoResponse >= SCCorpStyle.SDOMaxConsecutiveNoResponses)
						{
							fbc = DIFeedbackCode.DIThreeConsecutiveNonResponseFromDevice;
							break;
						}
						#endregion
						#endregion
						lastIndex = dcfSub.indexNumber;
					}
				}
			}
			#region obj flags
			/*	 * Finished DCF download so set objectDictionary back to being ignored.	 */
			destNode.EDS_DCF.ignoreObjFlags = true;
			#endregion obj flags

			if(destNode.EVASRequired == true)
			{ //cannot just call saveCommunicationParameters here - we need to take account of backdoor
				#region save objects 1200 to 1fff to EEPROM. - regrdless of whether we had previous fialure 
				if (( destNode.isSevconApplication() == true ) 
					&& (destNode.accessLevel<5) 
					&& (canDownloadViaBackDoor == true)
					&& ( DCFnode.DCFChecksumOK == true ))
				{
					#region do EVAS via bakcdoor
					ODItemData evasSub = destNode.getODSub(0x1010, 1);
					if(evasSub != null)
					{
						fbc = destNode.writeODValueViaBackDoor(evasSub, SCCorpStyle.SaveBackwardsValue);
						if(fbc == DIFeedbackCode.DISuccess)
						{
							destNode.EVASRequired = false;
						}
					}
					#endregion do EVAS via bakcdoor
				}
				else
				{
					fbc = destNode.saveCommunicationParameters();
				}
				#endregion
			}
#if DEBUGGING_DCF
			sendDCFDatatoFile();
#endif
			return ( fbc );
		}

#if DEBUGGING_DCF
//this method is temprary for testing any reported DCf issues
		public void sendDCFDatatoFile(  )
		{
			FileStream fs;
			StreamWriter sr;
			string filepath = MAIN_WINDOW.UserDirectoryPath + @"\DVT_DCFCompare10Apr1325";
			try
			{
				fs = new FileStream( filepath, System.IO.FileMode.Create,  FileAccess.Write,FileShare.Read );
				sr = new StreamWriter( fs );
			}
			catch
			{
				Message.Show("Unable to create file compare");
				return;
			}
			foreach(odRef item in this.CANcomms.tempItemsInDCF)
			{
//				sr.WriteLine("0x" + item.index.ToString("X") + ", 0x" + item.sub.ToString("X") + ", 0x" + item.val.ToString("X"));
//				sr.WriteLine("0x" + item.index.ToString("X") + ", 0x" + item.sub.ToString("X") + ", 0x" + item.data[0].ToString("X") + " 0x" + item.data[1].ToString("X")+ " 0x" + item.data[2].ToString("X")+ " 0x" + item.data[2].ToString("X"));
				sr.WriteLine("0x" + item.index.ToString("X") 
					+ ", 0x" + item.sub.ToString("X") 
					+ ", " + item.data[0].ToString() 
					+ " " + item.data[1].ToString() 
					+ " " + item.data[2].ToString() 
					+ " " + item.data[3].ToString()
					+ " value: 0x" + item.val.ToString("X"));
			}
			sr.Flush();
			sr.Close();
			fs.Close();
		}
#endif
		//-------------------------------------------------------------------------
		//  Name			: readODFromDCF()
		//  Description     : Reads the format of the OD and values of all objects held in
		//					  the DCF of DCFfilename passed as a parameter and stored them
		//					  in the DCFnode.
		//  Parameters      : DCFfilename - user selected filename to save OD to
		//  Used Variables  : DCFnode - nodeInfo class used to hold replica OD of DCF file
		//  Preconditions   : The user must have selected the DCF filename of interest
		//  Post-conditions : The DCFnode should have contstructed the OD as defined in the
		//					  EDS file and populated all the currentValues with the
		//					  value stored for each object in the DCF or a failure reason given.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Constructs and populates the DCF node OD with data from the DCF file.</summary>
		/// <param name="DCFfilename">user selected filename to save OD to</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode readODFromDCF(string passed_DCFfilepath)
		{
			// instantiate a new nodeInfo object (which will clear out any previous)
			DCFnode = new DCFNode(this, MAIN_WINDOW.DCFTblIndex);
			DCFnode.EDSorDCFfilepath = passed_DCFfilepath;  //we have to write this back in
			this.DCFnode.applyDCFSpecificDefaults();
			/* Read the DCF file of given filename to construct the dictionary contained within
			 * the DCFnode.
			 */
			DIFeedbackCode fbc = DCFnode.readDCFfile();
			return ( fbc );
		}

		#endregion DCF related

		#region nodeID to Node Array element translation
		//-------------------------------------------------------------------------
		//  Name			: getNodeNumber()
		//  Description     : Converts the nodeID of a real controller (used for all
		//					  CAN communications) to the nodes[] array element contained
		//					  within DW to replicated the node's OD.  This is because
		//					  node IDs need not be consecutive with valid range 1..127
		//					  whereas nodes[] array elements will be 0... no of nodes.
		//  Parameters      : nodeID - ID of node to convert
		//					  CANNodeIndex - element of the nodes array replicating this 
		//							   controller
		//  Used Variables  : None
		//  Preconditions   : nodes array must have been instantiated and populated with
		//					  relevant EDS information for each node on the attached CAN
		//					  network.
		//  Post-conditions : CANNodeIndex contains element index or failure reason given
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Converts a node of nodeID into the DI array element of nodes.</summary>
		/// <param name="nodeID">ID of node to have new value written into</param>
		/// <param name="CANNodeIndex">element of the nodes array replicating this controller</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode getNodeNumber( int nodeID, out int CANNodeIndex )
		{
			CANNodeIndex = 0;
			// for each node in the nodes array, check if the nodeID matches the one passed.
			for ( int i = 0; i < this.nodes.Length; i ++ )
			{
				if (  nodes[ i ].nodeID == nodeID  )
				{// match found so set CANNodeIndex to it to return & indicate success feedback code
					CANNodeIndex = i;
					return  DIFeedbackCode.DISuccess;
				}
			}
			return DIFeedbackCode.DIInvalidNodeNumber;
		}
		#endregion

		#region LSS functionality (to configure node IDs and baud rates)

		//-------------------------------------------------------------------------
		//  Name			: setSystemBaudRate()
		//  Description     : This function attempts to set the baud rate object in the
		//					  OD of every node on the system that is a Sevcon node and
		//					  has this object in it's OD.  3rd party devices cannot be
		//					  automatically altered like this because the OD item (if
		//					  it exists) is not known (manufacturer specific area).
		//  Parameters      : requestedBaudRate - user's required new baud rate for
		//							all devices on the CANbus system
		//  Used Variables  : nodes[] - array of NodeInfo objects which are used to
		//								define the controllers found on the system
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//  Preconditions   : There is a CAN system connected to DW via the USB-CAN
		//					  adapter which has already been found.
		//  Post-conditions : All Sevcon nodes have had their baud rate OD item written
		//					  via SDOs to change them to the requestedBaudRate.
		//  Return value    : fbc - indicates success or gives a failure reason
		//----------------------------------------------------------------------------
		///<summary>Sets all found Sevcon nodes on the CANbus to the requested baud rate. </summary>
		/// <param name="requestedBaudRate">user's required new baud rate for all devices on the CANbus system</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode setSystemBaudRate( BaudRate requestedBaudRate )
		{
			#region local variable declarations & initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			DIFeedbackCode firstFail = DIFeedbackCode.DISuccess;
			#endregion
			foreach ( nodeInfo node in nodes )
			{
				#region try and set this node's baud rate to the new value if it is a Sevcon node
				if ( node.isSevconApplication() == true )
				{
					ODItemData baudRateSub = node.getODSubFromObjectType(SevconObjectType.PHYSICAL_LAYER_SETTINGS,SCCorpStyle.BaudRateSubItem);
					if(baudRateSub != null)
					{
						// if the object exists, convert to the device equivalent baud value and write to the device's OD item
						long baudAsLong = 0;
						#region determine enumeration vlaue for baudrate
						//NUMBER_FORMAT=8 - from espAC OD
						//0=1Mb:1=500kb:2=250kb:3=125kb
						//4=100kb:5=50kb:6=20kb:7=10kb
						switch(requestedBaudRate)
						{
							case BaudRate._500K:
								baudAsLong = 1;
								break;
							case BaudRate._250K:
								baudAsLong = 2;
								break;
							case BaudRate._125K:
								baudAsLong = 3;
								break;
							case BaudRate._100K:
								baudAsLong = 4;
								break;
							case BaudRate._50K:
								baudAsLong = 5;
								break;
							case BaudRate._20K:
								baudAsLong = 6;
								break;
							case BaudRate._10K:
								baudAsLong = 7;
								break;
							default:
								break;
						}
						#endregion determine enumeration vlaue for baudrate
						fbc = node.writeODValue( baudRateSub, baudAsLong);
					}
					else
					{
						fbc =  DIFeedbackCode.DIInvalidIndexOrSub;
					}
				}
				#endregion

				#region if there is a failure, save off & allow to continue to set other nodes baud rate
				if (( fbc != DIFeedbackCode.DISuccess ) && ( firstFail == DIFeedbackCode.DISuccess ))
				{
					firstFail = fbc;
				}
				#endregion
			}
			return ( fbc );
		}
		


		//-------------------------------------------------------------------------
		//  Name			: setSystemNodes()
		//  Description     : This function tries to set all SEVCON nodes on the system
		//					  from their current node ID to those in the requestedNodeIDs
		//					  array.
		//  Parameters      : requestedNodeIDs - array of new node IDs that each node in the
		//								system is to get set to (this assumes the same order
		//								of those in the nodes[] array on the system).
		//  Used Variables  : nodes[] - array of NodeInfo objects which are used to
		//								define the controllers found on the system
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//  Preconditions   : There is a CAN system connected to DW via the USB-CAN
		//					  adapter which has already been found.
		//  Post-conditions : All Sevcon nodes have had their node ID OD item written
		//					  via SDOs to change them to the requestedNodeIDs.
		//  Return value    : fbc - indicates success or gives a failure reason
		//----------------------------------------------------------------------------
		///<summary>Sets all Sevcon nodes on the system to the new requestedNodeIDs.</summary>
		/// <param name="requestedNodeIDs">array of new node IDs that each node in the 
		/// system is to get set to (this assumes the same order of those in the nodes[] array on the system)</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode setSystemNodes( ushort [] requestedNodeIDs )
		{
			#region local variable declarations and initialsing of variables
			DIFeedbackCode fbc = DIFeedbackCode.DIUnexpectedNoOfNodeIDs;
			#endregion
			if ( requestedNodeIDs.Length == nodes.Length )
			{
				#region for each node in the system, if it is a Sevcon device then try to change the node ID to the new requested value
				for ( int CANNodeIndex = 0; CANNodeIndex < requestedNodeIDs.Length; CANNodeIndex++ )
				{
					if ( nodes[ CANNodeIndex ].isSevconApplication() == true )
					{
						ODItemData odsub = nodes[ CANNodeIndex ].getODSubFromObjectType(SevconObjectType.PHYSICAL_LAYER_SETTINGS, SCCorpStyle.NodeIDSubItem);
						if ( odsub != null)
						{// if it exists, write the new value to the device's OD index & sub
							fbc =  nodes[ CANNodeIndex ].writeODValue(odsub, (long)requestedNodeIDs[ CANNodeIndex ]); //returrns the feedback code
						}
						else
						{
							fbc = DIFeedbackCode.DIInvalidIndexOrSub;
						}
						if ( fbc != DIFeedbackCode.DISuccess )
						{
							// something went wrong so abort the entire process
							break;
						}
					}
				}
				#endregion
			}
			return ( fbc );
		}
		#endregion

		#region PDO mapping functions
		//-------------------------------------------------------------------------
		//  Name			: readAllCOBItemsAndCreateCOBsInSystem() 
		//  Description     : Reads allCOBReleated items on all nodes with EDS
		//					  The determines the vlaid an dacitve Txx COBIDs for them
		//  Parameters      : None
		//  Used Variables  : COBIDsInSystem - property containing valid, used COBIDs
		//  Preconditions   : None
		//  Post-conditions : COBIDsInSystem is populated with the valid used COBIDs
		//  Return value    : fbc - indicates success or gives a failure reason
		//----------------------------------------------------------------------------
		///<summary>Calculates which COBIDs are defined and valid on the connected CANbus system.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		/// 
		public void readAllCOBItemsAndCreateCOBsInSystem()
		{
			COBsInSystem = new ArrayList();
			foreach( nodeInfo node in this.nodes )
			{ //read all the node swith an EDS here - PDOableNodes does not contain the read only items
				if ( node.EDSorDCFfilepath != "" ) //anything with an EDS filepath has anOD
				{
					node.readCommsProfileAreaOfOD( COBsInSystem);
				}
			}
			//we now have an ArrayList representing all COBs and their routing in the system
			//name the PDOs with defualt names 
			int PDONum = 1;
			foreach(COBObject COB in this.COBsInSystem)
			{
				if(COB.transmitNodes.Count>1)
				{
					SystemInfo.errorSB.Append("\nCOB ID 0x" + COB.COBID.ToString("X").PadLeft(4, '0') + " has multiple transmit nodes");
				}
				if(COB.messageType == COBIDType.PDO)
				{
					COB.name = "PDO " + PDONum.ToString();
					PDONum++;
				}
			}
			//now sort the order
			IComparer myComparer = new COBsInSystemSortClass("name");
			this.COBsInSystem.Sort(myComparer);
		}
		//-------------------------------------------------------------------------
		//  Name			: getUnusedCOBIDs()
		//  Description     : This function selects the number of COBIDs required
		//					  dependent on the priority the user requires, ensuring
		//					  that the COBIDs suggested are not already used in the
		//					  system connected.  The lower the COBID, the higher the
		//					  priority.
		//  Parameters      : noCOBIDsRequired - number of COBIDs the user needs
		//					  priority - the priority the user requires for these new
		//							COBIDs
		//					  selectedCOBIDs - return list of suggested COBIDs to use
		//  Used Variables  : _COBIDsUsed - list of all valid COBIDs already used
		//  Preconditions   : The connected system has been found and all ODs constructed
		//					  from the EDSs, the commnication profile of each node has been
		//					  read and getCOBIDsUsed() has already been called.
		//  Post-conditions : selectedCOBIDs contains a list of suggest COBs to use
		//  Return value    : fbc - indicates success or gives a failure reason
		//----------------------------------------------------------------------------
		///<summary>Selects the required number of COBIDs which are not already used dependent on the priority required.</summary>
		/// <param name="noCOBIDsRequired">number of COBIDs the user needs</param>
		/// <param name="priority">the priority the user requires for these new COBIDs</param>
		/// <param name="selectedCOBIDs">return list of suggested COBIDs to use</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode getUnusedCOBIDs( COBIDPriority priority, int [] selectedCOBIDs )
		{
			#region local variable declarations & initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			int [] baseAddress = { 0x180, 0x280, 0x380, 0x480 };
			int baseCOBIDAddress = baseAddress[ priority.GetHashCode() ];
			int offset = 0;
			int cobIndex = 0;
			#endregion

			#region pull out the number of suggested COBIDs required, using the priority to select the base COBID
			for ( offset = 1; offset < 0x7f; offset++ )
			{
				#region calculate suggested COBID
				int suggestedCOBID = baseCOBIDAddress + offset; 
				#endregion
				bool COBSuggestionOk = true;
				foreach(COBObject COB in this.COBsInSystem)
				{
					if(COB.requestedCOBID == suggestedCOBID)
					{
						COBSuggestionOk = false;
						break;
					}
				}
				#region if suggested COBID not already used then add to the list
				if ( COBSuggestionOk == true)
				{ 
					selectedCOBIDs[ cobIndex++ ] = suggestedCOBID;		
					if ( cobIndex >= selectedCOBIDs.Length )
					{
						break;		// found desired no. of COBIDs so quit loop
					}
				}
				#endregion
			}
			#region check and assign failure code if couldn't allocate all COBIDs
			if ( offset >= 0x7f )
			{
				// couldn't allocate a COB ID
				fbc = DIFeedbackCode.DIUnableToAllocateCOBIDForPDOMap;
			}
			#endregion

			#endregion

			return ( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: setupMonitorPDOs()
		//  Description     : Checks the list of objects can be monitored using
		//					  PDOs and if they all can, then it checks that there are
		//					  enough free PDOs on each node to monitor the required
		//					  items.  If this is so, it then sets up the new transmit PDOs
		//					  and associated COBIDs ready for monitoring.
		//					  A priority is passed as a parameter so that the new
		//					  transmit monitoring PDOs can be chosed to avoid
		//					  disrupting the existing vehicle performance e.g. by
		//					  flooding the CANbus with new PDOs which stops the timely
		//					  arrival of the original PDOs on the system.  The lower the
		//					  COBID, the higher the priority.
		//  Parameters      : list - the list of objects & the nodes they are on that
		//							are needed for graphical monitoring (selected by
		//							the user)
		//					  priority - the priority the user requires for these new
		//							COBIDs
		//  Used Variables  : _COBIDsUsed - list of all valid COBIDs already used
		//  Preconditions   : The system has been found and the ODs constructed, the
		//					  communication profiles of each node have already been
		//					  read and the already used COBIDs have been calculated.
		//  Post-conditions : The system has had it's nodes setup with new transmit
		//					  PDOs to allow the list of objects to be monitored via
		//					  PDOs or there is a failure reason indicated.  If new
		//					  PDOs are mapped, the original setup is saved to RAM.
		//  Return value    : fbc indicates success or gives a failure reason.
		//----------------------------------------------------------------------------
		///<summary>Sets up the PDOs and COBIDs required for graphical monitoring.</summary>
		/// <param name="list">the list of objects and the nodes they are on that
		/// are needed for graphical monitoring (selected by the user)</param>
		/// <param name="priority">the priority the user requires for these new COBIDs</param>
		/// <param name="monitorTimebase"></param>

		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode setupMonitorPDOs( COBIDPriority priority, uint monitorTimebase )
		{
			#region local variable declarations & initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			int CANNodeIndex = 0;
			int [] noOfConfigurableTxPDOs = new int [ nodes.Length ];
			int [] selectedCOBIDs = new int[0];
			bool [] insufficientMaxPDOMaps = new bool[ nodes.Length ];
			#endregion

			#region check which CANnodes we want to set up TxPDos from ( who cares what state the othe rnodes are in)
			ArrayList TxCANNodes = new ArrayList();
            foreach (ODItemAndNode monItem in CANcomms.VCI.OdItemsBeingMonitored)
			{
				if(TxCANNodes.Contains(monItem.node.nodeID) == false)
				{
					TxCANNodes.Add(monItem.node.nodeID);
				}
			}
			#endregion check which CANnodes we want to set up TxPDos from ( who cares what state the othe rnodes are in)

			#region calculate the number of Tx PDO maps required on each node & whether they are available
			for ( CANNodeIndex = 0; CANNodeIndex < nodes.Length; CANNodeIndex++ )
			{
				#region skip nodes not represented in monitring list
				if(TxCANNodes.Contains(nodes[CANNodeIndex].nodeID) == false)
				{
					continue;  //not interested in this node
				}
				#endregion skip nodes not represented in monitring list

				#region comments
				/* Calculate the number of PDO maps required for the monitoring list (ignoring
					 * items already in a PDO map and calculated using items already in a PDO map
					 * if applicable). Gives a min and max number of PDO maps required for
					 * graphical monitoring.
					 */
				#endregion comments

				int numPDOsRequired;
				fbc = nodes[ CANNodeIndex ].numMonitorPDOsThisNodeNeedsToTx( out numPDOsRequired);
				#region select COB IDs for the monitoring PDOs required
				if  ( fbc == DIFeedbackCode.DISuccess )
				{
					selectedCOBIDs = new int[numPDOsRequired];
					fbc = getUnusedCOBIDs(priority, selectedCOBIDs );
					if(fbc != DIFeedbackCode.DISuccess)
					{
						return fbc;
					}
					fbc = nodes[ CANNodeIndex ].setupAndWriteMonitorTxPDOs( selectedCOBIDs, monitorTimebase , this.COBsInSystem);
					if ( fbc != DIFeedbackCode.DISuccess )
					{// something went wrong so there is no point in continuing
						this.restorePDOAndCOBIDConfiguration();
						return ( fbc );
					}
				}
				else
				{
					return fbc;
				}
				#endregion
			}
			#endregion
			return ( fbc );
		}
	
		//-------------------------------------------------------------------------
		//  Name			: changeMonitoringTimebase()
		//  Description     : This function writes the new monitor timebase required by the
		//					  user to all the transmit PDOs on the system specifically for
		//					  DW's monitoring purposes.
		//  Parameters      : newMonitorTimebase - new timebase to be used for all transmit
		//						monitoring PDOs (in milliseconds)
		//  Used Variables  : nodes[] - defines the connected CAN system
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//  Preconditions   : The system has been found & defined by EDSs, the user has
		//					  already selected OD items to be monitored graphically and
		//					  the DI has previously setup and written these transmit PDOs
		//					  for graphing purposes.
		//  Post-conditions : The timebase (inhibit time and event time) for the DW written
		//					  PDOs for graphing have the timebase changed to the 
		//					  newMonitorTimebase passed as a parameter.
		//  Return value    : fbc indicates success or gives a failure reason.
		//----------------------------------------------------------------------------
		///<summary>Changes the timebase of all monitoring transmit PDOs to newMonitorTimebase.</summary>
		/// <param name="newMonitorTimebase">new timebase to be used for all transmit monitoring PDOs (in milliseconds)</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode changeMonitoringTimebase( uint newMonitorTimebase )
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			#endregion
			foreach(COBObject COB in this.COBsInSystem)
			{
				if(COB.createdForCalibratedGraphing == true)
				{
					COBObject.PDOMapData txData  = (COBObject.PDOMapData) COB.transmitNodes[0];
					int CANNodeIndex = 0;
					this.getNodeNumber(txData.nodeID, out CANNodeIndex);
					fbc = nodes[CANNodeIndex].updateMonitoringTimebase( COB, newMonitorTimebase);
					if(fbc != DIFeedbackCode.DISuccess)
					{
						return ( fbc );//it's all gone pear shaped so leave now
					}
				}
			}
			return ( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: changeMonitoringPriority()
		//  Description     : This function writes the new monitor priority required by the
		//					  user to all the transmit PDOs on the system specifically for
		//					  DW's monitoring purposes.
		//  Parameters      : newPriority - new priority to be used for all transmit
		//						monitoring PDOs (in milliseconds)
		//  Used Variables  : nodes[] - defines the connected CAN system
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//  Preconditions   : The system has been found & defined by EDSs, the user has
		//					  already selected OD items to be monitored graphically and
		//					  the DI has previously setup and written these transmit PDOs
		//					  for graphing purposes.
		//  Post-conditions : The COBID priority for the DW written
		//					  PDOs for graphing have the priority changed to the 
		//					  newMonitorPriority passed as a parameter.
		//  Return value    : fbc indicates success or gives a failure reason.
		//----------------------------------------------------------------------------
		///<summary>Changes the timebase of all monitoring transmit PDOs to newMonitorTimebase.</summary>
		/// <param name="newMonitorTimebase">new timebase to be used for all transmit monitoring PDOs (in milliseconds)</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode changeMonitoringPriority( COBIDPriority newMonitorPriority)
		{
			#region local variable declarations and variable initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			#endregion

			foreach(COBObject COB in this.COBsInSystem)
			{
				if(COB.createdForCalibratedGraphing == true) //this PDO was changed by Driv eWIzard for monitring = so we can change its interval
				{
					COBObject.PDOMapData txData = (COBObject.PDOMapData) COB.transmitNodes[0];
					int CANNodeIndex = 0;
					fbc = this.getNodeNumber(txData.nodeID, out CANNodeIndex);
					if(fbc != DIFeedbackCode.DISuccess)
					{
						SystemInfo.errorSB.Append("NodeID ");
						SystemInfo.errorSB.Append(txData.nodeID.ToString());
						SystemInfo.errorSB.Append(" does not exist on vehicle");
						return fbc;
					}
					else
					{
						int [] localselectedCOBIDs = new int[1];  //we only need one here 
						this.getUnusedCOBIDs(newMonitorPriority, localselectedCOBIDs);
						COB.requestedCOBID = localselectedCOBIDs[0];  //mark the COBIB we want - just like in SystemPDOs
						fbc = this.nodes[CANNodeIndex].changeCOBID(COB, true, 0 );
						if(fbc == DIFeedbackCode.DISuccess)
						{
							#region update COBIDs in monitring list fo rthe Rx interupt handler
                            for (int i = 0; i < CANcomms.VCI.OdItemsBeingMonitored.Count; i++)
							{
								//if(list[i].COB.COBID == COB.COBID)
                                if (((ODItemAndNode)CANcomms.VCI.OdItemsBeingMonitored[i]).ODparam.monPDOInfo.COB == COB)
								{
                                    ((ODItemAndNode)CANcomms.VCI.OdItemsBeingMonitored[i]).ODparam.monPDOInfo.COB.COBID = COB.requestedCOBID;
								}
							}


							#endregion update COBIDs in monitring list fo rthe Rx interupt handler
							COB.COBID = COB.requestedCOBID;  //update the COB in COBsInSystem

						}
						else
						{
							COB.requestedCOBID = COB.COBID;
						}
					}
				}
			}
			return ( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: restorePDOAndCOBIDConfiguration()
		//  Description     : This function restores the PDO mappings and COBID comms setup
		//					  back to their original value at the end of a graphical 
		//					  monitoring session.
		//  Used Variables  : nodes - array of object representing each node on the system
		//  Preconditions   : The system has been found with the ODs constructed from the EDSs,
		//					  the communication profile of each node was read, the user selected
		//					  to perform graphical monitoring and all objects selected were
		//					  PDO mappable, and the system was reconfigured with extra PDO
		//					  maps and COBIDs to transmit the graphing information.
		//  Post-conditions : The PDO maps and COBIDs comms parameters have been restored
		//					  back to their original values prior to the graphical monitoring
		//					  on EVERY node in the connected system.
		//  Return value    : fbc indicates success or gives a failure reason
		//----------------------------------------------------------------------------
		///<summary>Restores the original PDO and COBID config after graphical monitoring.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode restorePDOAndCOBIDConfiguration( )
		{
			#region local variable declarations and initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess, firstFailfbc = DIFeedbackCode.DISuccess;
			#endregion

			ArrayList COBsToGO = new ArrayList(); //use a seperate list becuace we must not remove items for a collectionwhilst we are looping through it
			foreach(COBObject COB in this.COBsInSystem)
			{
				if(COB.createdForCalibratedGraphing == true)
				{
					COBsToGO.Add(COB);
				}
			}
			foreach(COBObject delCOB in COBsToGO)
			{  //this is OK because we loop round one collection - but delete from the other
				fbc = this.deletePDOMapAndComms(delCOB);
				if(fbc != DIFeedbackCode.DISuccess)
				{
					SystemInfo.errorSB.Append("Failed to remove COBID");
					SystemInfo.errorSB.Append(delCOB.requestedCOBID.ToString());
					firstFailfbc = fbc;
				}
			}
			return ( firstFailfbc );
		}
		
		//-------------------------------------------------------------------------
		//  Name			: deletePDOMapAndComms()
		//  Description     : This function deletes from the system the COBID passed
		//					  as a parameter and all it's associated PDO maps.  This
		//					  is performed on the transmit node and for all the receive
		//					  nodes.  In reality, the COBID is simply invalidated and
		//					  the PDO mapping lengths are set to 0.  No other parameters
		//					  are deleted.
		//  Parameters      : COBID - COBID of the PDO map and communications object that
		//						is to be deleted.
		//  Used Variables  : nodes - array of objects representing each node on the system
		//  Preconditions   : The system has been found with the ODs constructed from the EDSs,
		//					  the communication profile of each node was read, the existing
		//					  COBIDs used on the system with their associated PDO mappings 
		//					  for the transmit node and all receive nodes have been 
		//					  displayed to the user and the user has selected an existing
		//					  COBID to be deleted from the system.
		//  Post-conditions : The PDO maps and COBIDs comms parameters associated with
		//					  this COBID has been invalidated on every node that it appears
		//					  on ie transmit nodes and all the receive nodes.
		//  Return value    : fbc indicates success or gives a failure reason
		//----------------------------------------------------------------------------
		///<summary>Deletes the COBID communication parameters and all the associated PDO maps.</summary>
		/// <param name="COBID">COBID of the PDO map and communications object that is to be deleted</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public DIFeedbackCode deletePDOMapAndComms( COBObject COBToDelete )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			#region local variable declarations & initialisation
			int CANNodeIndex = 0;
			StringBuilder localErrStr = new StringBuilder();
			#endregion local variable declarations & initialisation

			foreach(COBObject.PDOMapData txData in COBToDelete.transmitNodes)
			{
				#region delete Tx data on CANnodes for this this COB
				this.getNodeNumber(txData.nodeID, out CANNodeIndex);
				fbc = nodes[ CANNodeIndex ].removeCANNodeFromPDO(COBToDelete, true, 0);
				if(fbc != DIFeedbackCode.DISuccess)
				{
					localErrStr.Append("Error feedback -- TODO");
				}
				#endregion delete Tx data on CANnodes for this this COB
			}
			foreach(COBObject.PDOMapData rxData in COBToDelete.receiveNodes)
			{
				#region delete Rx data on CANnodes for this this COB
				this.getNodeNumber(rxData.nodeID, out CANNodeIndex);
				fbc = nodes[ CANNodeIndex ].removeCANNodeFromPDO(COBToDelete, false, COBToDelete.receiveNodes.IndexOf(rxData));
				if(fbc != DIFeedbackCode.DISuccess)
				{
					localErrStr.Append("Error feedback -- TODO");
				}
				#endregion delete Rx data on CANnodes for this this COB
			}
			if(localErrStr.Length <= 0)
			{
				COBsInSystem.Remove(COBToDelete);
			}
			else
			{
				SystemInfo.errorSB.Append(localErrStr.ToString());
				return DIFeedbackCode.DIFailedToDeletePDO;
			}
			return ( fbc );
		}
		
		public void changeCOBIDOnAllNodes(COBObject COBToModify)
		{
			int CANNodeIndex;
			foreach(COBObject.PDOMapData txData in COBToModify.transmitNodes)
			{
				this.getNodeNumber(txData.nodeID, out CANNodeIndex);
				this.nodes[CANNodeIndex].changeCOBID(COBToModify, true,0);
			}
			foreach(COBObject.PDOMapData rxData in COBToModify.receiveNodes)
			{
				this.getNodeNumber(rxData.nodeID, out CANNodeIndex);
				this.nodes[CANNodeIndex].changeCOBID(COBToModify, false, COBToModify.receiveNodes.IndexOf(rxData));
			}
		}

		public void changeTxTypeOnAllNodes(COBObject COBToModify)
		{
			int CANNodeIndex;
			foreach(COBObject.PDOMapData txData in COBToModify.transmitNodes)
			{
				this.getNodeNumber(txData.nodeID, out CANNodeIndex);
				this.nodes[CANNodeIndex].changeTxType(COBToModify, true,0);
			}
			foreach(COBObject.PDOMapData rxData in COBToModify.receiveNodes)
			{
				this.getNodeNumber(rxData.nodeID, out CANNodeIndex);
				this.nodes[CANNodeIndex].changeTxType(COBToModify, false, COBToModify.receiveNodes.IndexOf(rxData));
			}
		}
		//-------------------------------------------------------------------------
		//  Name			: enterPDOMonitoring() 
		//  Description     : This function indicates to the CANcomms and VCI objects
		//					  to start PDO monitoring (instead of SDOs). Before starting,
		//					  it sets up a hash table with those PDOs which contain the
		//					  data of interest for the graphical monitoring (used as a 
		//					  filter in VCI).
		//  Parameters      : list - the list of objects & the nodes they are on that
		//							are needed for graphical monitoring (selected by
		//							the user)
		//  Used Variables  : CANcomms - CAN communications object (tx & rx on CANbus)
		//  Preconditions   : The system has been found and DW constructed it from the EDSs,
		//					  the user has selected calibrated graphical monitoring for
		//					  selected items in the OD, the new transmit PDOs have been
		//					  written to the relevant devices and everything is ready
		//					  for starting the monitoring of the PDOs.
		//  Post-conditions : None (infinite process).
		//  Return value    : fbc indicates success or gives a failure reason
		//----------------------------------------------------------------------------
		///<summary>Changes monitoring from SDOs to PDOs (only COBIDs of items on the monitoring list).</summary>
		/// <param name="list">the list of objects and the nodes they are on that 
		/// are needed for graphical monitoring (selected by the user)</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		public void enterPDOMonitoring()
		{
			#region endless loop called on a thread by the GUI, aborted by GUI when no longer required
			bool monitoring = true;
			while ( monitoring == true )
			{
				// all handled in VCI comms module for speed
			}
			#endregion
		}

		public DIFeedbackCode setupCOBReceivePArameters( )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			#region save the monitorCOBIDs list to the CANcomms object (& in turn the VCI object)
			if ( fbc == DIFeedbackCode.DISuccess )
			{
				//calculate and save what bitmask & bit shifts are required to extract data from PDOs data
				setupMonitoringMaskAndShift( );
				if ( fbc == DIFeedbackCode.DISuccess )
				{
					#region start the PDO monitoring (listen to PDOs instead of SDOs)
					CANcomms.startPDOMonitoring( );
					#endregion
				}
			}
			#endregion
			return fbc;
		}

		#region private PDO mapping functions
		//-------------------------------------------------------------------------
		//  Name			: writePDOMapping()
		//  Description     : Handles the specific format required for writing a
		//					  new mapping to a PDO_MAPPING object.  This requires that
		//					  sub 0 (number of mapped objects) is set to zero prior to
		//					  writing all the other subs otherwise CAN error messages
		//					  are issued and the new value is not written.  Then all
		//					  the other subs are written with their new value and
		//					  finally sub 0 is then set with the actual value required.
		//  Parameters      : nodeID - the nodeID of the specific controller required
		//					  thisItem - object and all subs associated with this
		//							     PDO_MAPPING object
		//  Used Variables  : None
		//  Preconditions   : System must be found (with EDS read successfully) and the
		//					  thisObject passed as a parameter must be a PDO_MAPPING object
		//					  type, with the currentValue for each sub containing the new
		//					  value to be written to the controller.
		//  Post-conditions : PDO_MAPPING object on the controller has been updated to match
		//					  that contained in thisObject's currentValue.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Handles the specific format and order required for writing a new PDO MAPPING object.</summary>
		/// <param name="nodeID">the nodeID of the specific controller required</param>
		/// <param name="thisItem">object and all subs associated with this PDO_MAPPING object</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		private DIFeedbackCode writePDOMapping( nodeInfo destNode, ObjDictItem odItem )
		{
			//SystemInfo.errorSB = new StringBuilder(); //don't clear out all previous errors found
			#region local variable declarations & variable initialisation
			DIFeedbackCode fbc;
			#endregion

			ODItemData numMapsSub = destNode.getODSub(((ODItemData)odItem.odItemSubs[0]).indexNumber, SCCorpStyle.PDOMapNoSubsSubIndex);
			#region if sub zero was found then set no. mapped objects to 0 else set fbc code to failure
			/* Write a value of 0 to sub 0 of this object so that no errors are given by 
			 * the controller when writing all the new values for the other subs
			 */
			if ( numMapsSub != null)
			{				
				long numMaps = numMapsSub.currentValue; //take backup for writing after
				fbc = destNode.writeODValue(numMapsSub, 0 );	
				if ( fbc == DIFeedbackCode.DISuccess )
				{
					#region write each of the new values (except sub 0) to the device
					/* Now write the new values as contained within the DW's object to the 
					 * controller (without errors now), ensuring that sub object 0 is not
					 * written.
					 */

					foreach( ODItemData odSub in odItem.odItemSubs )
					{
						#region skip Header and num subs 
						if ( odSub.subNumber <= 0 )
						{
							continue;
						}
						#endregion
						if(odSub.currentValue != 0) //unused TPOD mappings ar eoften zero - but th enode will reject and attempt to write zero to it
						//We need to write ALL non zero mappings because some may be just 'switched off' rather than not needed
						{
						fbc = destNode.writeODValue( odSub, odSub.currentValue ); //can't use currentVlaue - it is now zero
						}
					}
					#endregion
				}
				#region Lastly, write sub 0 item to the new value it needs to be.
				fbc = destNode.writeODValue(numMapsSub, numMaps);	
			}
			else
			{
				return DIFeedbackCode.DIInvalidIndexOrSub;
			}
			#endregion


			#endregion

			#region update feedback code to indicate if there were any failures
			if (SystemInfo.errorSB.Length>0)
			{
				fbc = DIFeedbackCode.DIUnableToWritePDOMapToDevice;
			}
			#endregion

			return ( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: writePDOMappingViaBackDoor()
		//  Description     : Handles the specific format required for writing a
		//					  new mapping to a PDO_MAPPING object using the backdoor
		//					  method for writing a PDO map when the user has insufficient
		//					  access to write to the actual object but the DCF file
		//					  has been verified with a valid checksum.  This requires that
		//					  sub 0 (number of mapped objects) is set to zero prior to
		//					  writing all the other subs otherwise CAN error messages
		//					  are issued and the new value is not written.  Then all
		//					  the other subs are written with their new value and
		//					  finally sub 0 is then set with the actual value required.
		//  Parameters      : nodeID - the nodeID of the specific controller required
		//					  CANNodeIndex - index into nodes[] array for this nodeID
		//					  thisItem - object and all subs associated with this
		//							     PDO_MAPPING object
		//  Used Variables  : None
		//  Preconditions   : System must be found (with EDS read successfully) and the
		//					  thisObject passed as a parameter must be a PDO_MAPPING object
		//					  type, with the currentValue for each sub containing the new
		//					  value to be written to the controller.
		//  Post-conditions : PDO_MAPPING object on the controller has been updated to match
		//					  that contained in thisObject's currentValue.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Handles the specific format and order required for writing a new PDO MAPPING object 
		/// via the Sevcon backdoor object when the user has an insuffient access level.</summary>
		/// <param name="nodeID">the nodeID of the specific controller required</param>
		/// <param name="CANNodeIndex">array element index into nodes[] array for given nodeID</param>
		/// <param name="thisItem">object and all subs associated with this PDO_MAPPING object</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		private DIFeedbackCode writePDOMappingViaBackDoor( nodeInfo destNode, ObjDictItem odItem)
		{
			#region local variable declarations & variable initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIInvalidIndexOrSub;
			#endregion

			ODItemData numMapsSub = destNode.getODSub(((ODItemData) odItem.odItemSubs[0]).indexNumber, SCCorpStyle.PDOMapNoSubsSubIndex);
			if(numMapsSub != null)
			{
				#region set no. mapped objects to 0 
				long numMaps = numMapsSub.currentValue;
				fbc = destNode.writeODValueViaBackDoor(numMapsSub, 0 );
				#endregion

				if ( fbc == DIFeedbackCode.DISuccess )
				{
					#region write the maps
					/* Now write the new values as contained within the DW's object to the 
					 * controller (without errors now), ensuring that sub object 0 is not
					 * written.
					 */

					foreach( ODItemData subItem in odItem.odItemSubs )
					{
						#region skip header and sub 0
						if ( subItem.subNumber <= 0 )
						{
							continue;
						}
						#endregion

						#region select the relevant overloaded function, dependent on data type
						if ( subItem.displayType == CANopenDataType.VISIBLE_STRING )
						{
							fbc = DIFeedbackCode.DICannotWriteAStringViaBackDoor;
						}
						else
						{
							fbc = destNode.writeODValueViaBackDoor(subItem, subItem.currentValue );//judetemp - was zero I thinkit was worng
						}
						#endregion
					}
					#endregion
				}

				#region write num of maps
				fbc = destNode.writeODValueViaBackDoor( numMapsSub, numMaps );
				#endregion  write num of maps

				#region update feedback code to indicate if there were any failures
				if (SystemInfo.errorSB.Length>0)
				{
					fbc = DIFeedbackCode.DIUnableToWritePDOMapToDevice;
				}
				#endregion
			}
			return ( fbc );
		}

		//-------------------------------------------------------------------------
		//  Name			: setupMonitoringMaskAndShift()
		//  Description     : This function takes each item on the monitor list and 
		//					  (previously checked to be PDO mappable) and calculates
		//					  what bit mask and bit shifting is needed to extract this
		//					  list item's value from the PDO.  PDOs always contain 8 bytes
		//					  of raw data and this is to allow the speedy extraction of
		//					  data from the PDOs as they are received, once PDO monitoring
		//				      has started.
		//  Parameters      : list - an array of all the items that the user has selected
		//						to monitor using PDOs (given by index and sub).
		//  Used Variables  : None
		//  Preconditions   : The system has been found, constructed from EDSs, the user
		//					  has selecte graphical monitoring and the items in the list
		//					  have all been checked and are PDO mappable.
		//  Post-conditions : list has been updated to contain the bit mask and the bit
		//					  shifting required to extract this item's value from the PDO
		//					  when it is received.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Calculates the bit shift and mask required to extract the list item's data from PDOs.</summary>
		/// <param name="list">an array of all the items that the user has selected
		/// to monitor using PDOs (given by index and sub).</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		private void setupMonitoringMaskAndShift()
		{
			#region local variable declaration and initialisation
			long mask = 0;
			#endregion

			// for each item in the list, calculate what bit mask & shift is needed to extract data from the PDO
            foreach (ODItemAndNode itemAndNode in CANcomms.VCI.OdItemsBeingMonitored)
                			{
				#region calculate what bit mask is needed to extract this list item from the PDO data
				mask = 0;		// clear out bit mask for next item in list

				// OR with 1 and shift for the number of bits in the item
				for ( int bit = 0; bit < itemAndNode.ODparam.dataSizeInBits; bit++ )
				{
					mask |= 0x01;
					mask <<= 1;
				}

				// undo last shift (as it was one too many before the above loop ended)
				mask >>= 1;

				// now shift the mask to align with the start bit in the PDO (from LSB bits)
				mask <<= itemAndNode.ODparam.monPDOInfo.startBitInPDO; 
				#endregion

				#region copy over into the list data structure to be used later when receiving PDOs
				itemAndNode.ODparam.monPDOInfo.mask = mask;
				itemAndNode.ODparam.monPDOInfo.shift = itemAndNode.ODparam.monPDOInfo.startBitInPDO;
				#endregion
			}

			return ;
		}

	
		#endregion

		#endregion PDO mapping functions

		#region private functions
		
		#region private for read/write OD and creating & saving the connected system
		//-------------------------------------------------------------------------
		//  Name			: createNodesFromEDS()
		//  Description     : Given the list of node IDs that exist on the attached system,
		//					  this function reads the device version data and looks for
		//					  a matching EDS file.  These EDS's are then read in to construct 
		//					  the object dictionarys for each node.
		//  Parameters      : None
		//  Used Variables  : nodes[] - array of NodeInfo objects which are used to
		//								define the controllers found on the system
		//					  CANcomms - CAN communications object (tx & rx on CANbus)
		//  Preconditions   : The CANcomms.nodeList must have been populated with the
		//				      node IDs which were found on the attached CANbus.
		//  Post-conditions : nodes[] has the OD populated by the matched EDS file or
		//					  an error is given if one is not found.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Constructs the OD from the EDS file for each known node on the system.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		private void createNodesFromEDS(  )
		{
			#region variable declarations and initialisation
            DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			// create the number of nodes required
			nodes = new nodeInfo[ CANcomms.nodeList.Length ];
			#endregion

			/* For each node found on the system, create an instance of NodeInfo
			* and populate with basic infomation. 
			*/
			for ( int n = 0; n < CANcomms.nodeList.Length; n++ )
			{
				#region create nodeInfo object instance for this node & initialise
				nodes[ n ] = new nodeInfo(this, n, this.eventIDList);

				// copy over node ID found by comms object
				nodes[ n ].nodeID = CANcomms.nodeList[ n ];
				#endregion

				#region read the device info (mandatory 0x1018) from controller
				if ( MAIN_WINDOW.isVirtualNodes == true )
				{
					#region virtual nodes
					VPCanNode cannode = (VPCanNode) SCCorpStyle.VirtualCANnodes[n];
					nodes[ n ].setDeviceIdentity( cannode.vendorid, cannode.productcode, cannode.revisionnumber);
					if(cannode.master == true)
					{
						nodes[n].masterStatus = true;
					}
					else
					{
						nodes[n].masterStatus = false;
					}
					#endregion virtual nodes
				}
				else
				{
					fbc = nodes[ n ].readDeviceIdentity( );
				}
				#endregion

				#region try and find a matching EDS which defines this node's OD
				if ( fbc == DIFeedbackCode.DISuccess )
				{					
					// find the matching EDS filename
					fbc = nodes[ n ].findMatchingEDSFile();
			
					if ( fbc != DIFeedbackCode.DISuccess )
					{
						nodes[ n ].manufacturer = Manufacturer.UNKNOWN;
						//create a minimal OD here judetemp
						//nodes[n].dictionary = new
						nodes[ n ].createMinimalDictionary();
					}
				}
				#endregion
				if(nodes[ n ].EDSorDCFfilepath != "")
				{  //do not retrun - we may have more nodes to create 
					//but don't read the EDS unless it exists
					#region read the EDS file to contruct the node's OD
					/* If a matching EDS file is found, read it to construct the object
												   * dictionary (contained within the node object).
												   */
					if ( fbc == DIFeedbackCode.DISuccess )
					{
						fbc = nodes[ n ].readEDSfile();
					}
					#endregion

					#region set property to show if the node is a Sevcon node or not
					if ( fbc == DIFeedbackCode.DISuccess )
					{
						_noOfNodesWithValidEDS++;

						if ( nodes[ n ].vendorID == SCCorpStyle.SevconID )
						{
							nodes[ n ].accessLevel = 0;		// minimum access until logged on to a Sevcon node
							nodes[ n ].manufacturer = Manufacturer.SEVCON;
					
							if ( nodes[ n ].productRange == SCCorpStyle.ProductRange.PST.GetHashCode() )
							{
								nodes[ n ].accessLevel = 255;		// default for PST as can't log on
							}
						}
						else
						{
							nodes[ n ].accessLevel = 255;		// default for a 3rd party as can't log on
							nodes[ n ].manufacturer = Manufacturer.THIRD_PARTY;
						}
					}
					#endregion

					#region notify GUI bits
					if ( fbc == DIFeedbackCode.DISuccess )
					{
						fbc = nodes[ n ].readNodeState( );

						if( fbc == DIFeedbackCode.DISuccess )
						{
							// notify the GUI so it can update the display to notify the user
							if( notifyGUI != null )
							{
								notifyGUI( COBIDType.ProducerHeartBeat, nodes[ n ].nodeID, n, nodes[ n ].nodeState, "" );
							}
						}
					}
					#endregion notify GUI bits
				}
			}
		}

		#endregion

		#region private for logs and eventID identifiers, Sevcon section & object identifiers

		//-------------------------------------------------------------------------
		//  Name			: readEventIDFile()
		//  Description     : Reads in the text file containing all the known fault and
		//					  event IDs with their relevant text string description.
		//					  These are stored in the eventIDList sorted array list.
		//  Parameters      : IDs - sorted array list or all known IDs and strings to add to
		//  Used Variables  : None
		//  Preconditions   : None
		//  Post-conditions : eventIDList contains all IDs and associated strings contained
		//					  within the EventsID.txt file.
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Reads in the DW text file containing all currently known event and fault IDs.</summary>
		/// <param name="IDs">sorted array list or all known IDs and strings to add to</param>
		/// <returns>feedback code indicating success or reason of failure</returns>
		private DIFeedbackCode readEventIDFile( out SortedList IDs )
		{
			#region local variable delcarations and initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			StreamReader sr;
			FileStream	fs;
			string input;
			string [] split = null;
			// create a new sorted list which replaces any older ones
			SortedList eventIDs = new SortedList();
			#endregion

			#region if the read the file it it already exists
			if(Directory.Exists(MAIN_WINDOW.UserDirectoryPath + @"\IDS"))
			{
				if ( File.Exists(MAIN_WINDOW.UserDirectoryPath + @"\IDS\EventIDs.txt" ))
				{
					try
					{
						#region read the eveitIDs.txt file and place data into the eventIDs sorted list
						#region open the file stream & stream reader for reading the text file
						fs = new FileStream( MAIN_WINDOW.UserDirectoryPath + @"\IDS\EventIDs.txt", System.IO.FileMode.Open, System.IO.FileAccess.Read );
						sr = new StreamReader( fs );
						#endregion

						#region while the end of the file has not been reached
						while ( ( input = sr.ReadLine() ) != null )
						{
							// in case other data is kept here, look for the ID SECTION header string
							if ( input.IndexOf( "[EVENT_ID_STRINGS]" ) != -1 )
							{
								fbc = DIFeedbackCode.DISuccess;
								break;
							}
						}
						#endregion

						#region while the end of the file has not been reached
						while ( ( input = sr.ReadLine() ) != null )
						{
							UInt16 ID;

							// quit when the END_SECTION footer string is found
							if ( input.IndexOf( "[END_SECTION]" ) != -1 )
							{
								break;
							}
								/* Expected format is  "xxxx=aaaaaaaaaaaa" where xxxx is a 
																								   * hex number and aaaaa is the text string of any length.
																								   * Invalid format if no = sign in line.
																								   */
							else if ( input.IndexOf( "=" ) != -1 )
							{
								/* A valid format so split into two strings, one with xxxx ID
																								   * and the other with the text descriptor.
																								   */
								split = input.Split( '=' );

								/* Convert event ID from hex text string to UInt16 and leave
																								   * descriptor as a text string.
																								   */
								if ( split.Length >= 2 )
								{
									try
									{
										ID = System.Convert.ToUInt16( split[ 0 ], 16 );
										eventIDs.Add( ID, split[ 1 ] );
									}
									catch
									{
										// incorrect format (not a number) so don't add to list.
									}
								}
							}
						}
						#endregion

						#region close the stream reader and file stream reader
						// Close the stream reader if it was opened.
						if ( sr != null )
						{
							sr.Close();
						}

						// Close the file stream if it was opened.
						if ( fs != null )
						{
							fs.Close();
						}
						#endregion
						#endregion
					}
					catch ( Exception e )
					{
						SystemInfo.errorSB.Append("Failed to read known Event Names file. Error code:");
						SystemInfo.errorSB.Append(e.Message);
					}
				}
			}
			#endregion
			
			#region make IDs object passed by ref point to the newly constructed eventIDs sorted list
			IDs = eventIDs;
			#endregion

			return ( fbc );
		}
        /// <summary>
        /// Read ProductIDs.xml file to get the product ranges and variants strings for Sevcon devices
        /// </summary>
        // DR38000256 calibrator handling, also allow forwards extensibility by moving strings to XML file
        private void readSevconProductsFile()
        {
            string filename = MAIN_WINDOW.UserDirectoryPath + @"\IDS\ProductIDs.xml";
            SevconProductDescriptions defaultDescriptions = new SevconProductDescriptions();

            if ((Directory.Exists(MAIN_WINDOW.UserDirectoryPath + @"\IDS"))
                && (File.Exists(filename))
                )
            {
                #region read sevcon product descriptions file in
                try
                {
                    ObjectXMLSerializer objectXMLSerializer = new ObjectXMLSerializer();
                    try
                    {
                        defaultDescriptions = (SevconProductDescriptions)objectXMLSerializer.Load(defaultDescriptions, filename);
                    }
                    catch (Exception e)
                    {
                        SystemInfo.errorSB.Append("\nFailed to load Sevcon Product descriptions from file");
                        SystemInfo.errorSB.Append(filename);
                        SystemInfo.errorSB.Append("Exception: ");
                        SystemInfo.errorSB.Append(e.Message);
                        defaultDescriptions = null;
                    }
                }
                catch (Exception e)
                {
                    SystemInfo.errorSB.Append("\nFailed to load Sevcon Product descriptions from file");
                    SystemInfo.errorSB.Append(filename);
                    SystemInfo.errorSB.Append("Exception: ");
                    SystemInfo.errorSB.Append(e.Message);
                    defaultDescriptions = null;
                }
                #endregion read sevcon product descriptions file in
            }

            if ((defaultDescriptions == null) 
             || (Directory.Exists(MAIN_WINDOW.UserDirectoryPath + @"\IDS") == false) 
             || (File.Exists(filename) == false)
                )
            {
                #region create default list
                for (int i = 0; i < SCCorpStyle.SevconProductRanges.Length; i++)
                {
                    SevconProductRange dfltRange = new SevconProductRange(SCCorpStyle.SevconProductRanges[i], i);
                    SevconProductVariant dfltVariant;

                    for (int j = 0; j < SCCorpStyle.SevconProductVariants.Length; j++)
                    {
                        dfltVariant = new SevconProductVariant(SCCorpStyle.SevconProductVariants[j], j);
                        dfltRange.sevconProductVariants.Add(dfltVariant);
                    }
                    defaultDescriptions.sevconProductRanges.Add(dfltRange);
                }
                #endregion create default list

                #region write default list to file
                if (Directory.Exists(MAIN_WINDOW.UserDirectoryPath + @"\IDS") == false)
                {
                    Directory.CreateDirectory(MAIN_WINDOW.UserDirectoryPath + @"\IDS");
                }

                if (File.Exists(filename) == false)
                {
                    File.Delete(filename);
                }

                try
                {
                    ObjectXMLSerializer objectXMLSerializer = new ObjectXMLSerializer();
                    try
                    {
                        objectXMLSerializer.Save(defaultDescriptions, filename);
                    }
                    catch (Exception e)
                    {
                        SystemInfo.errorSB.Append("\nFailed to save default Sevcon product descriptions from file");
                        SystemInfo.errorSB.Append(filename);
                        SystemInfo.errorSB.Append("Exception: ");
                        SystemInfo.errorSB.Append(e.Message);
                        defaultDescriptions = null;
                    }
                }
                catch (Exception e)
                {
                    SystemInfo.errorSB.Append("\nFailed to save default Sevcon product descriptions from file");
                    SystemInfo.errorSB.Append(filename);
                    SystemInfo.errorSB.Append("Exception: ");
                    SystemInfo.errorSB.Append(e.Message);
                    defaultDescriptions = null;
                }
                #endregion write default list to file
            }

            if (defaultDescriptions != null)
            {
                sevconProductDescriptions = defaultDescriptions;
            }
        }
		//-------------------------------------------------------------------------
		//  Name			: writeEventIDFile()
		//  Description     : If the eventIDList has been updated during this run of DW,
		//					  update the text file.  This minimises the amount of text
		//					  strings needing to be retrieved from the Sevcon controller.
		//  Parameters      : None
		//  Used Variables  : eventIDList - sorted array list containing all event and
		//								    fault IDs (from controller & previously
		//									held values in the eventIDs.txt file)
		//  Preconditions   : Any previous EventIDs.txt file has been successfully read
		//					  into the eventIDList (otherwise all IDs will be lost)
		//					  and at least one new item has been retrieved from a controller.
		//  Post-conditions : EventIDs.txt file is rewritten with all currently known
		//					  fault & event IDs (as contained in the eventIDList).
		//  Return value    : fbc - feedback code indicating success or reason of failure
		//----------------------------------------------------------------------------
		///<summary>Writes the updated list of ASCII definitions for fault/event IDs to text file.</summary>
		/// <returns>feedback code indicating success or reason of failure</returns>
		private DIFeedbackCode writeEventIDFile()
		{
			#region local variable declarations and initialisation
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;
			StreamWriter sw;
			FileStream	fs;
			#endregion

			#region check the IDs directory exists and if not then create it
			if ( Directory.Exists( MAIN_WINDOW.UserDirectoryPath + @"\IDS" ) == false )
			{
				try
				{
					Directory.CreateDirectory( MAIN_WINDOW.UserDirectoryPath + @"\IDS" );
				}
				catch{}
			}
			#endregion

			try
			{
				#region open or create the eventIDs.txt file as necessary
				fs = new FileStream( MAIN_WINDOW.UserDirectoryPath + @"\IDS\EventIDs.txt", System.IO.FileMode.Create, System.IO.FileAccess.Write );
				sw = new StreamWriter( fs );
				#endregion

				// section header string written to file
				sw.WriteLine( "[EVENT_ID_STRINGS]" );

				#region write each ID and associated text string in eventIDList to file
				/* For each item in the eventIDList, write the values to the file if
				 * the descriptor string is not the default <unknown ID> (so that next
				 * time maybe retrieve proper descriptor).
				 * Note: the event ID is written as a hex value.
				 */
				for ( int i = 0; i < eventIDList.Count; i++ )
				{
					if ( eventIDList.GetByIndex( i ).ToString() != SCCorpStyle.UnknownIDDescriptor )
					{
						UInt16 id = (UInt16)eventIDList.GetKey( i );
						sw.WriteLine( id.ToString("X") + "=" + eventIDList.GetByIndex( i ).ToString() );
					}
				}
				#endregion

				// section footer string written to file
				sw.WriteLine( "[END_SECTION]" );

				#region close file stream and stream writer
				// close the stream writer if opened OK
				if ( sw != null )
				{
					sw.Close();
				}

				// close the file stream is opened OK
				if ( fs != null )
				{
					fs.Close();
				}
				#endregion
			}
			catch
			{
				//do nothing at this point we are disposing
				//of System Info and closing main form
				//would be unwise to plase a call the MessageBox
			}

			return ( fbc );
		}

        //-------------------------------------------------------------------------
        //  Name			: addSevconSectionAndObjectEnumsToIDLists()
        //  Description     : Adds all the known Sevcon Section and Object enumerations
        //                    to the start of the SevconSectionIDList and SevconObjectIDList
        //                    array lists, along with their relevant text string descriptor.
        //  Parameters      : sectionIDList - array list with SevconSectionType enumerations
        //                                    added as strings to it
        //                    objectIDList  - array list with SevconObjectType enumerations
        //                                    added as strings to it
        //  Used Variables  : None
        //  Preconditions   : Must be called before the system is found; before EDS files are read.
        //  Post-conditions : sectionIDList and objectIDList populated with enumerations.
        //  Return value    : fbc - feedback code indicating success or reason of failure
        //----------------------------------------------------------------------------
        ///<summary>Adds Sevcon Section and Object enumerations to the start of sectionIDList and objectIDList along with the relevant text string identifier.</summary>
        /// <param name="sectionIDList">array list with all SevconSectionType enumerations added as text strings</param>
        /// <param name="objectIDList">array list with all SevconObjectType enumerations added as text strings</param>
        /// <returns>feedback code indicating success or reason of failure</returns>
        private DIFeedbackCode addSevconSectionAndObjectEnumsToIDLists(out ArrayList sectionIDList, out ArrayList objectIDList)
        {
            #region local variable delcarations and initialisation
            DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
            // create a new array list which replaces any older ones
            ArrayList sectionIDs = new ArrayList();
            ArrayList objectIDs = new ArrayList();
            #endregion

            #region add those section and objects with special, specific DW handling
            // These are added at the start of the array list so that the enumerations
            // used in the code match their array list index, thus allowing reference
            // throughout the code in enumerations. The remainder of the sections and
            // objects will be added dynamically at run-time when the EDS files are
            // read. To avoid duplication, all new items are first checked to see if
            // they are in the array list (see EDSFile.cs).
            // NOTE: DW IS TRANSPARENT TO ALL SECTIONS & OBJECTS THAT ARE NOT ENUMERATED.
            //       THE XML FILE & EDS FILE SECTION & OBJECT NAMES MUST MATCH.
            for ( int i = 0; i < ((int)SevconSectionType.LAST_SEVCON_SECTION_TYPE); i++ )
            {
                sectionIDs.Add(((SevconSectionType)i).ToString());
            }

            for (int i = 0; i < ((int)SevconObjectType.LAST_SEVCON_OBJECT_TYPE); i++)
            {
                objectIDs.Add(((SevconObjectType)i).ToString());
			}
            #endregion

            #region make IDs object passed by ref point to the newly constructed array lists
            sectionIDList = sectionIDs;
            objectIDList = objectIDs;
            #endregion

			return ( fbc );
		}

       
		#endregion

		#endregion

		#region utility methods used by numerous Forms
		internal string getEnumeratedValue(string formatString, long Value)
		{
			string [] enumStrs = formatString.Split(':');
			for(int i = 0;i<enumStrs.Length;i++)
			{
				string textStr= "";
				string valueStr = "";
				int indexEquals =  enumStrs[i].IndexOf("=");
				if((indexEquals != -1) && ((indexEquals+1)<(enumStrs[i].Length-1)))
				{ //we can extract text
					textStr = enumStrs[i].Substring(indexEquals+1);
					valueStr = enumStrs[i].Substring(0,indexEquals).Trim();
				}
				else
				{
					return Value.ToString();
				}
				if(enumStrs[i].ToUpper().IndexOf("0X") != -1)
				{
					#region hex number
					long equivValue = System.Convert.ToInt64(valueStr, 16);//..su.Remove(enumStrs[i].Length-18, 5); //16 chars fo long plus '0x' - padded in EDS to ensure we can fit all possible values
					if(equivValue == Value)
					{ //hex vlaues ar eall padded to 16 chars plus '0x' in EDS file - eliminate need for delimiter = could use : if this proves too cumbersome = 
						return textStr;
					}
					#endregion hex number
				}
				else
				{
					#region base 10
					long equivValue = System.Convert.ToInt64(valueStr);//textOnly = enumStrs[i].Remove(enumStrs[i].Length-20, 5);
					if(equivValue == Value)
					{
						return textStr;
					}
					#endregion base 10
				}
			}
			return Value.ToString();
		}

		internal long getValueFromEnumeration(string inputString, string formatString, out bool dereferencedOK)
		{
			long enumValue = 0;
			dereferencedOK = false;
			string [] enumStrings = formatString.Split(':');
			for(int i = 0;i<enumStrings.Length;i++)
			{
				int equalsIndex = enumStrings[i].IndexOf("=");
				if((equalsIndex != -1) && ((equalsIndex+1) <(enumStrings[i].Length-1)))
				{
					string textStr = enumStrings[i].Substring(equalsIndex + 1);
					string valStr = enumStrings[i].Substring(0, equalsIndex).Trim();

					if(textStr.Equals(inputString) == true)
					{
						if(valStr.ToUpper().IndexOf("0X") != -1)
						{
							dereferencedOK = true;
							return System.Convert.ToInt64(valStr, 16);
						}
						else
						{
							dereferencedOK = true;
							return System.Convert.ToInt64(valStr);
						}
					}
				}
			}
			return enumValue; 
		}

        /// <summary>
        /// Inserts a stringg indicating what the GUI was doing if an error occured
        /// </summary>
        /// <param name="insertString"></param>
        internal void insertErrorType(string insertString)
        {
            if (SystemInfo.errorSB.Length > 0)
            {
                if (insertPoint > 0)
                {
                    string addLines = "\n\n";
                    SystemInfo.errorSB.Insert(insertPoint, addLines);
                    insertPoint += addLines.Length;
                }
                SystemInfo.errorSB.Insert(insertPoint, insertString);
                insertPoint = SystemInfo.errorSB.Length;
            }
        }
		internal void displayErrorFeedbackToUser(string insertText)
		{
			if(SystemInfo.errorSB.ToString() != SCCorpStyle.SaveCommsWarning)
			{
				SystemInfo.errorSB.Insert(0, insertText); //only insert the error additon if the contains more that just the Save Comms warning
			}
			else
			{
				SystemInfo.errorSB.Insert(0, "No Errors");
			}
            if (SystemInfo.errorSB.Length > 0)
            {
                Form errForm = new ErrorMessageWindow(SystemInfo.errorSB.ToString());
                SystemInfo.errorSB.Length = 0;
                insertPoint = 0;
                errForm.ShowDialog();
            }
		}

        //Used when GUI neeed to see if erro occured but no t inform user eg repeat failures of monitored items
        internal void clearErrorSB()
        {
            SystemInfo.errorSB.Length = 0;
            insertPoint = 0;
        }

		internal int [] calculateColumnWidths(DataGrid dgrid, float [] passed_percents, int VscrollWidth, int defaultHeight)
		{
			float [] percents = new float[passed_percents.Length];
			passed_percents.CopyTo(percents,0);
			int _VScrollWidth = Math.Max(0, VscrollWidth);
			if(dgrid.Tag !=null)
			{
				try
				{
					ArrayList temp = new ArrayList();
					temp =(ArrayList)  dgrid.Tag;
					percents = new float[temp.Count];
					temp.CopyTo(percents);
				}
				catch
				{
					percents = new float[passed_percents.Length];
					passed_percents.CopyTo(percents,0);
				}
			}
			int [] colWidths = new int[percents.Length];
			float availableWidth = Math.Max(10,dgrid.ClientRectangle.Width) - _VScrollWidth - 4; 
			float totalWidthInt = 0;
			for (int i=0;i<percents.Length;i++)
			{
				colWidths[i] = System.Convert.ToInt32(availableWidth * (percents[i]));
				totalWidthInt +=colWidths[i];
			}
			int overlap = (int) (totalWidthInt - availableWidth);
			colWidths[colWidths.Length-1] -= overlap;  //mop up any difference in the last column
			return colWidths;
		}

		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: formatDataGrid
		///		 *  Description     : Applies SEVCON style to datagrid. These are all the parameters
		///		 *					  that cannot be set in the TableStyle
		///		 *  Parameters      : System.Controls.DataGrid
		///		 *  Used Variables  : 
		///		 *  Preconditions   :  
		///		 *  Post-conditions : 
		///		 *  Return value    : none
		///		 *--------------------------------------------------------------------------*/
		/// </summary>

		internal void formatDataGrid(DataGrid dataGrid1)
		{
			#region colours
			dataGrid1.AlternatingBackColor = SCCorpStyle.dgRowBackColour;
			dataGrid1.BackgroundColor = SCCorpStyle.dgBackColour;
			dataGrid1.BackColor = SCCorpStyle.dgRowBackColour;
			dataGrid1.CaptionBackColor = SCCorpStyle.dgHeaderColour;
			dataGrid1.CaptionForeColor = SCCorpStyle.dgForeColour;
			dataGrid1.ForeColor = SCCorpStyle.dgForeColour;
			dataGrid1.GridLineColor =  SCCorpStyle.headerRow;
			dataGrid1.HeaderBackColor = SCCorpStyle.dgblockColour;
			dataGrid1.HeaderForeColor  = SCCorpStyle.dgForeColour;
			dataGrid1.SelectionBackColor = SCCorpStyle.dgBackColour; //we overwrite this to allow simultanious selection of multiple rows
			dataGrid1.SelectionForeColor = SCCorpStyle.dgForeColour;
			#endregion colours
			dataGrid1.ParentRowsVisible = false;
			dataGrid1.BorderStyle = BorderStyle.FixedSingle;
			dataGrid1.RowHeadersVisible = false;
			dataGrid1.AllowNavigation = false;
			dataGrid1.CaptionVisible = true;
			dataGrid1.ColumnHeadersVisible = true;
			dataGrid1.FlatMode = true;
			dataGrid1.GridLineStyle = DataGridLineStyle.Solid;
			dataGrid1.BackColor = SCCorpStyle.dgBackColour;
			dataGrid1.BackgroundColor = SCCorpStyle.dgBackColour;

			Graphics g = dataGrid1.CreateGraphics();
			SizeF mySize = g.MeasureString("test",dataGrid1.Font);
			g.Dispose();
			dataGrid1.PreferredRowHeight = (int) (mySize.Height + 4);
		}
		//		internal void formatControls(System.Windows.Forms.Control topControl, string FormName )
		//		{
		//			#region format individual controls
		//			topControl.ForeColor = SCCorpStyle.SCForeColour;
		//			topControl.Font = new System.Drawing.Font("Arial", 8F);
		//			if ( topControl.GetType().Equals( typeof( Button ) ) ) 
		//			{
		//				topControl.BackColor = SCCorpStyle.buttonBackGround;
		//			}
		//			else if ( topControl.GetType().Equals( typeof( DataGrid ) ) ) 
		//			{
		//				DataGrid myDG = (DataGrid) topControl;
		//				SCCorpStyle.formatDataGrid(ref myDG);
		//				myDG.Resize +=new EventHandler(myDG_Resize);
		//			}
		//			else if ( topControl.GetType().Equals( typeof( VScrollBar ) ) ) 
		//			{
		//				topControl.VisibleChanged +=new EventHandler(topControl_VisibleChanged); 
		//			}
		//			else if ( topControl.GetType().Equals( typeof( HScrollBar ) ) ) 
		//			{
		//				//judetemp				topControl.Height = 0;
		//			}
		//			else if ( topControl.GetType().Equals( typeof( GroupBox ) ) ) 
		//			{
		//				topControl.Font = new System.Drawing.Font("Arial", 8F, FontStyle.Bold);
		//			}
		//			#endregion format individual controls
		//			foreach(Control control in topControl.Controls) 
		//			{
		//				formatControls(control);
		//			}
		//		}
		#endregion utility methods used by numerous Forms

        #region event related
        internal void updateEventList(ushort eventID, string descriptor, uint productCode)
        {
            int eventIDIndex = -1;
            eventIDIndex = eventIDList.IndexOfKey(eventID);

            #region if already known eventID so check if it is still the same text descriptor
            if (eventIDIndex != -1)
            {
                // If the text descriptor has changed, remove the old one and add the new.
                if (descriptor != eventIDList.GetByIndex(eventIDIndex).ToString())
                {
                    eventIDList.RemoveAt(eventIDIndex);
                    eventIDList.Add(eventID, descriptor);
                    eventIDUpdated = true;						// marker to update text file at end of DW run
                }
            }
            #endregion
            #region else brand new text descriptor added to the event ID list.
            else
            {
                eventIDList.Add(eventID, descriptor);
                eventIDUpdated = true;						// marker to update text file at end of DW run
            }
        }
        #endregion event related
        internal string getEventDescriptionFromEventID(ushort eventID, uint productCode)
        {
            string eventDescription = SCCorpStyle.UnknownIDDescriptor;
            int eventIDIndex = eventIDList.IndexOfKey(eventID);
            // already known eventID so get string from array list
            if (eventIDIndex != -1)
            {
                eventDescription = eventIDList.GetByIndex(eventIDIndex).ToString();
            }
            return eventDescription;
        }
        //createEventListFromConnectedDevice() & getEventList() removed as never called in code
        #endregion

        #region number conversions

        #region convert to bool
        //-------------------------------------------------------------------------
		//  Name			: convertToBool()
		//  Description     : This function converts a text string into an unsigned
		//					  integer, taking into account whether it is a decimal
		//					  or hexidecimal number.
		//  Parameters      : charString - text string to be converted to an uint
		//  Used Variables  : None
		//  Preconditions   : charString is expected to contain an unsigned int but
		//				      in text format.
		//  Post-conditions : retValue returned should contain the boolean equivalent
		//					  or charString.
		//  Return value    : boolean value equivalent of charString passed as a parameter
		//--------------------------------------------------------------------------
		///<summary>Converts a text string integer (dec or hex) into a boolean</summary>
		/// <param name="charString">text string to be converted to an uint</param>
		/// <returns>boolean value equivalent of charString passed as a parameter</returns>
    	internal bool convertToBool( string charString)
		{
			this.conversionOK = true;
			bool retValue = false;		// must initialise value for return
			if( charString == "")
			{
				return retValue;  //CiA require empty strings to be treated as what ever we would defualt to 
			}
			uint interimValue = 0;
			// Remove any leading or trailing spaces & make case insensitive.
			charString = charString.ToUpper();
			try
			{	
				// If it is a non-null string then convert from a string to an integer.
				// If string is not prefixed with 0x then convert using base 10
				if ( charString.IndexOf( "0X", 0 ) == -1 )
				{
					interimValue = System.Convert.ToUInt32(charString);
					retValue = System.Convert.ToBoolean(interimValue);
				}
					// Else it's hexadecimal so convert using base 16.
				else
				{
					interimValue = System.Convert.ToUInt32( charString, 16 );
					retValue = System.Convert.ToBoolean(interimValue);
				}
			}
			catch
			{
				this.conversionOK = false;  //if we ge tto her ewe passed
				return false; //aasuume false if we got an exception
			}
			return ( retValue );
		}
        #endregion convert to bool

        #region convert to uint
		//-------------------------------------------------------------------------
		//  Name			: convertToUint()
		//  Description     : This function converts a text string into an unsigned
		//					  integer, taking into account whether it is a decimal
		//					  or hexidecimal number.
		//  Parameters      : charString - text string to be converted to an uint
		//  Used Variables  : None
		//  Preconditions   : charString is expected to contain an unsigned int but
		//				      in text format.
		//  Post-conditions : retValue returned should contain the integer equivalent
		//					  or charString.
		//  Return value    : uint - unsigned integer value equivalent of charString
		//					  passed as a parameter
		//--------------------------------------------------------------------------
		///<summary>Converts a text string integer into an unsigned integer, either hexidecimal or decimal.</summary>
		/// <param name="charString">text string to be converted to an uint</param>
		/// <returns>unsigned integer value equivalent of charString passed as a parameter</returns>
		internal uint convertToUint( string charString)
		{
			uint retValue = 0;		// 
			conversionOK = true;
			// Remove any leading or trailing whitespace & make case insensitive.
			charString = charString.Trim().ToUpper();
			if( charString == "")
			{
				return ( retValue );  //pass
			}
			try
			{	
				if ( charString.IndexOf( "0X" ) == 0 )
				{
					#region hexadecimal
					retValue = System.Convert.ToUInt32( charString, 16 );
					#endregion hexadecimal
				}
				else
				{
					#region base 10
					retValue = System.Convert.ToUInt32( charString, 10 );
					#endregion base 10
				}
			}
			catch 
			{
				this.conversionOK = false;
				return 0;
			}
			conversionOK = true;
			return ( retValue );
		}

        #endregion convert to uint
		//-------------------------------------------------------------------------
		//  Name			: convertToSignedLong()
		//  Description     : This function converts a signed value (8/16/24/32/40/48/56/64 
		//					  bits) to a signed long (64 bits): from the CANopenDataType to
		//					  a common format used by DW to deal with all integers.
		//  Parameters      : dataType - CANopen data type which must be a signed 
		//							     integer (8/16/24/32/40/48/56/64 bits)
		//					  signedValue - signed value in CANopenDataType format which
		//									must be converted to a 64 bit signed value
		//  Used Variables  : None
		//  Preconditions   : The CANopenDataType to be converted from must be known and
		//					  the value for conversion must be in the CANopenDataType 
		//					  format.
		//  Post-conditions : The return value is the signedValue converted into
		//					  a signed 64 bit format.
		//  Return value    : signed long value with sign bit moved to correct position
		//----------------------------------------------------------------------------
		///<summary>Converts a signed value of 8,16,24,32,40,48,56 or 64 bits into a signed long value (64 bits).</summary>
		/// <param name="dataType">CANopen data type which must be a signed integer (8/16/24/32/40/48/56/64 bits)</param>
		/// <param name="signedValue">signed value in CANopenDataType format which must be converted to a 64 bit signed value</param>
		/// <returns>feedback code indicates success or gives a failure reason</returns>
		internal long convertToSignedLong( CANopenDataType dataType, long signedValue )
		{
			#region local variable declaration and variable initialisation
			const long signBitINT24 = 0x800000;
			const long signBitINT40 = 0x8000000000;
			const long signBitINT48 = 0x800000000000;
			const long signBitINT56 = 0x80000000000000;
			#endregion

			#region convert the signed data value into a signed long (by moving the signed bit)
			switch ( dataType )
			{
					#region 8 bit integer
				case CANopenDataType.INTEGER8: 
				{
					// Use the system conversion to convert to a signed 8 bit number.
					signedValue = (System.SByte)signedValue;
					break;
				}
					#endregion
				
					#region 16 bit integer
				case CANopenDataType.INTEGER16:
				{
					// Use the system conversion to convert to a signed 16 bit number.
					signedValue = (System.Int16)signedValue;
					break;
				}
					#endregion
				
					#region 24 bit integer
				case CANopenDataType.INTEGER24:
				{
					// Convert a negative 24 bit number to a negative 64 bit number.
					// Positive numbers need no conversion in 2's complement format.
					if ( ( signedValue & signBitINT24 ) == signBitINT24 )
					{
						// convert back to a positive value 24 bit value (2's complement)
						signedValue -= 1;
						signedValue = ~signedValue;
						signedValue &= 0xffffff;

						// Now convert to a 2's complement 64 bit negative number.
						signedValue = ~signedValue;
						signedValue += 1;
					}
					break;
				}
					#endregion
				
					#region 32 bit integer
				case CANopenDataType.INTEGER32:
				{
					// Use the system conversion to convert to a signed 32 bit number.
					signedValue = (System.Int32)signedValue;
					break;
				}
					#endregion
				
					#region 40 bit integer
				case CANopenDataType.INTEGER40:
				{
					// Convert a negative 40 bit number to a negative 64 bit number
					// Positive numbers need no conversion in 2's complement format.
					if ( ( signedValue & signBitINT40 ) == signBitINT40 )// is negative
					{
						// convert back to a positive value 40 bit value (2's complement)
						signedValue -= 1;
						signedValue = ~signedValue;
						signedValue &= 0xffffffffff;

						// Now convert to a 2's complement 64 bit negative number.
						signedValue = ~signedValue;
						signedValue += 1;
					}
					break;
				}
					#endregion
				
					#region 48 bit integer
				case CANopenDataType.INTEGER48:
				{
					// Convert a negative 48 bit number to a negative 64 bit number
					// Positive numbers need no conversion in 2's complement format.
					if ( ( signedValue & signBitINT48 ) == signBitINT48 )// is negative
					{
						// convert back to a positive value 48 bit value (2's complement)
						signedValue -= 1;
						signedValue = ~signedValue;
						signedValue &= 0xffffffffffff;

						// Now convert to a 2's complement 64 bit negative number.
						signedValue = ~signedValue;
						signedValue += 1;
					}
					break;
				}
					#endregion
				
					#region 56 bit integer
				case CANopenDataType.INTEGER56:
				{
					// Convert a negative 56 bit number to a negative 64 bit number
					// Positive numbers need no conversion in 2's complement format.
					if ( ( signedValue & signBitINT56 ) == signBitINT56 )// is negative
					{
						// convert back to a positive value 56 bit value (2's complement)
						signedValue -= 1;
						signedValue = ~signedValue;
						signedValue &= 0xffffffffffffff;
						
						// Now convert to a 2's complement 64 bit negative number.
						signedValue = ~signedValue;
						signedValue += 1;
					}
					break;
				}
					#endregion

					#region 64 bit integer
				case CANopenDataType.INTEGER64:
				{
					// Use the system conversion to convert to a signed 64 bit number.
                    //No conversion required, removal of compiler warning
					break;
				}
					#endregion
			}// end of switch for signed integers
			#endregion

			return ( signedValue );
		}

		//-------------------------------------------------------------------------
		//  Name			: convertToFloat()
		//  Description     : This function converts a text string into an float.
		//  Parameters      : charString - text string to be converted to an float
		//  Used Variables  : None
		//  Preconditions   : charString is expected to contain a float but
		//				      in text format.
		//  Post-conditions : retValue returned should contain the float equivalent
		//					  of charString.
		//  Return value    : float value equivalent of the charString passed as a parameter 
		//--------------------------------------------------------------------------
		///<summary>Converts a text string integer into an float equivalent.</summary>
		/// <param name="charString">text string to be converted to an float</param>
		/// <returns>retValue returned should contain the float equivalent of charString</returns>
        //Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
        //a bitstring and a base 10 number contianing onyl1s and 0s
        internal float convertToFloat(string charString)
		{
            float retValue = 0F;
            charString = charString.Trim().ToUpper();
            if (charString != "")  //charstring of "" is OK but cannot be converted
			{
                //determine the input format, bit string in decimal, hex or exponential format
			if(charString.IndexOf("0X") == 0) 
			{
				#region hexadecimal
				try
				{
                        //cannot convert directly from base 16 to float
                        uint temp = System.Convert.ToUInt32(charString, 16);
                        retValue = System.Convert.ToSingle(temp);
				}
				catch
				{
                        retValue = 0F;
				}
				#endregion hexadecimal
			}
			else 
			{
				#region base 10
					try
				{
                        //Jude DR000234 Apply InvariantCulture to all convertsions between strings and floats/doubles/reals
                        retValue = System.Convert.ToSingle(charString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
						}
						catch
						{
                        retValue = 0F;
				}
				#endregion base 10
				}
			}
			return ( retValue );
		}

		//-------------------------------------------------------------------------
		//  Name			: convertToLong()
		//  Description     : This function converts a text string into an long,
		//					  taking into account whether it is a decimal
		//					  or hexidecimal number.
		//  Parameters      : charString - text string to be converted to an uint
		//  Used Variables  : None
		//  Preconditions   : charString is expected to contain an long but
		//				      in text format.
		//  Post-conditions : retValue returned should contain the long equivalent
		//					  of charString.
		//  Return value    : long value equivalent of the charString passed as a parameter 
		//--------------------------------------------------------------------------
		///<summary>Converts a text string integer into an long, either hexidecimal or decimal.</summary>
		/// <param name="charString">text string to be converted to an uint</param>
		/// <returns>retValue returned should contain the long equivalent pf cjarStromg</returns>
		internal long convertToLong( string charString)
		{
			conversionOK = true;
			long	retValue = 0;				// must initialise value for return
			charString = charString.ToUpper().Trim();
			if( charString == "")
			{
				charString = "0";  //if empty assume to be zero 
			}
			// If it is a non-null string then convert from a string to a long.
			// Make case insensitive and remove any leading or trailing spaces.
			try
			{
				// If string is prefixed with 0x then convert using base 16
				if ( charString.IndexOf( "0X" ) == 0 )
				{
					retValue = System.Convert.ToInt64( charString, 16 ); //use += we may need to add node Id in
				}
					// Else convert string using base 10.
				else
				{
					retValue = System.Convert.ToInt64( charString, 10 ); //use += we may need to add node Id in
				}
			}
			catch 
			{
				conversionOK = false; //if we get here it converted OK
				return 0;
			}
			return ( retValue );
		}


		//-------------------------------------------------------------------------
		//  Name			: convertToDouble()
		//  Description     : This function converts a text string into an double.
		//  Parameters      : charString - text string to be converted to an double
		//  Used Variables  : None
		//  Preconditions   : charString is expected to contain a double but
		//				      in text format.
		//  Post-conditions : retValue returned should contain the double equivalent
		//					  of charString.
		//  Return value    : double value equivalent of the charString passed as a parameter 
		//--------------------------------------------------------------------------
		///<summary>Converts a text string integer into an double equivalent.</summary>
		/// <param name="charString">text string to be converted to an double</param>
		/// <returns>retValue returned should contain the double equivalent of charString</returns>
        //Jude DR000235 Remove DW support for bitstrings CANopen spec does not distinguish between 
        //a bitstring and a base 10 number contianing onyl1s and 0s
		internal double convertToDouble( string charString )
		{
			double retValue = 0;
			charString = charString.Trim().ToUpper();
            if (charString != "") //an empty string is valid in CANopen files but cannot be converted.
			{
			if(charString.ToUpper().IndexOf("0X") == 0)
			{
				#region hex entry
				try
				{
                        // we cannont convert directly from hex string to float
                        ulong temp = System.Convert.ToUInt64(charString, 16);
                        retValue = System.Convert.ToDouble(temp);
				}
				catch
				{
                        retValue = 0;
				}
				#endregion hex entry
			}
			else
				{
                    #region base 10
					try
					{
                        //Jude DR000234 Apply InvariantCulture to all convertsions between strings and floats/doubles/reals
                        retValue = System.Convert.ToDouble(charString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					}
					catch(Exception e)
					{
						SystemInfo.errorSB.Append("\nFailed to convert string to double. Text: " );
						SystemInfo.errorSB.Append(charString);
						SystemInfo.errorSB.Append(" ,error code ");
						SystemInfo.errorSB.Append(e.Message);
                        retValue = 0;
				}
                    #endregion base 10
				}
			}
			return ( retValue );
		}


		/// <summary>
		/// U
		/// </summary>
		/// <param name="valueSubString"></param>
		/// <returns></returns>
		internal string checkIfEDSParamValueContainsNodeID(string valueSubString)
		{
			if( 
				( valueSubString.ToUpper().IndexOf( "$NODEID" ) != -1 ) //it has $NODEID
				&& ( valueSubString.ToUpper().IndexOf( "+" ) != -1 ) //we have a split character
				&&  (valueSubString.ToUpper().IndexOf( "+" ) < (valueSubString.Length -1))  //not last char in string
				)
			{			
				return valueSubString.Trim().Substring(valueSubString.IndexOf( "+" ) + 1);
			}
			return valueSubString; //retrun the orignal string
		}
		#endregion number conversions

        #region log domains processing
        //DR38000269 maintain single list of all unknown fault IDs
        internal void processFIFOLog(ODItemData odSub, out FIFOLogEntry[] faultLog, out ArrayList unknownFaultIDs)
        {
            faultLog = new FIFOLogEntry[0];
            unknownFaultIDs = new ArrayList();  // DR38000269 clear out any old unknowns

            /* If retrieved the domain OK and log length indicates data in it, reconstruct the structure
                         * from the raw byte stream retrieved. This is dependent on a fixed, known data
                         * structure sent in the little endian format.
                         */
            if (odSub.currentValueDomain == null)
            {
                return;
            }
            int logLength = (int)(odSub.currentValueDomain.Length / SCCorpStyle.sizeOfFaultLogEntry);
            FIFOLogEntry[] tempLog = new FIFOLogEntry[logLength];

            // for each fault log entry, reconstruct the data
            for (int i = 0; i < logLength; i++)
            {
                tempLog[i] = new FIFOLogEntry();
                #region extract relevant bytes from raw domain data to contruct FIFOLogEntry data
                tempLog[i].eventID = (UInt16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfFaultLogEntry) + 0];
                tempLog[i].eventID += (UInt16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfFaultLogEntry) + 1]) << 8);
                tempLog[i].hours = (UInt16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfFaultLogEntry) + 2];
                tempLog[i].hours += (UInt16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfFaultLogEntry) + 3]) << 8);
                tempLog[i].minsAndSecs = (Byte)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfFaultLogEntry) + 4];
                tempLog[i].db1 = (Byte)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfFaultLogEntry) + 5];
                tempLog[i].db2 = (Byte)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfFaultLogEntry) + 6];
                tempLog[i].db3 = (Byte)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfFaultLogEntry) + 7];
                #endregion

                UInt16 eventID = (UInt16)(tempLog[i].eventID & 0x7fff);

                tempLog[i].description = getEventDescriptionFromEventID(eventID, odSub.CANnode.productCode);

                //DR38000269 add unknown eventID to list
                if (tempLog[i].description == SCCorpStyle.UnknownIDDescriptor)
                {
                    unknownFaultIDs.Add(tempLog[i].eventID);
                }
            }

            // Note - we will retrieve any missing descriptions on a seperate thread later.

            // make ref value (for log return) point to the newly constructed log
            faultLog = tempLog;
        }

        //-------------------------------------------------------------------------
        ///<summary>Retrieves the active fault log from node of nodeID if it is a Sevcon node.</summary>
        /// <param name="nodeID">ID of node to have new value written into</param>
        /// <param name="logtype">type of log to be retrieved from the controller</param>
        /// <param name="activeFaults">reference to data structure to return the log into</param>
        /// <returns>feedback code indicating success or reason of failure</returns>
        //-------------------------------------------------------------------------
        //DR38000269
        public void processActivefaultLog(ODItemData odSub, out NodeFaultEntry[] activeFaults, out ArrayList unknownFaultIDs)
        {
            #region local variable declarations and variable initialisation
            activeFaults = new NodeFaultEntry[0];
            #endregion
            unknownFaultIDs = new ArrayList();  //DR38000269 clear out any old unknowns

            if ((odSub == null) || (odSub.currentValueDomain == null))
            {
                return; //empty log 
            }

            #region convert raw byte stream to NodeFaultEntry data format
            int logLength = (int)(odSub.currentValueDomain.Length / SCCorpStyle.sizeOfActiveFaultEntry);
            activeFaults = new NodeFaultEntry[logLength];

            // for each active fault log entry, reconstruct the data
            for (int i = 0; i < logLength; i++)
            {
                #region reconstruct raw bytes back to faultID
                activeFaults[i] = new NodeFaultEntry(); 
                activeFaults[i].eventID = (UInt16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfActiveFaultEntry) + 0];
                activeFaults[i].eventID += (UInt16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfActiveFaultEntry) + 1]) << 8);
                #endregion

                #region get text descriptor associated with this faultID
                /* A text descriptor of what the eventID number means is useful to the user.
						 * Known IDs are held in a text file and read in on startup into an array
						 * list.  Check this to get the descriptor for this fault/event.  If the
						 * ID is unknown, retrieve the descriptor text string from the controller
						 * and update the array list. NOTE: the text file will be updated once 
						 * when DW is shut down.
						 * Mask off the save bit (bit 15) as this is not part of the event identifier.
						 */
                UInt16 faultID = (UInt16)(activeFaults[i].eventID & 0x7fff);
                activeFaults[i].description = getEventDescriptionFromEventID(faultID, odSub.CANnode.productCode);

                if (activeFaults[i].description == SCCorpStyle.UnknownIDDescriptor)
                {
                    unknownFaultIDs.Add(activeFaults[i].eventID); //DR38000269
                }
            }
                #endregion

            #endregion convert raw byte stream to NodeFaultEntry data format
        }

        //-------------------------------------------------------------------------
        ///<summary>Retrieves an event log from node of nodeID if it is a Sevcon node.</summary>
        /// <param name="nodeID">ID of node to have new value written into</param>
        /// <param name="logtype">type of log to be retrieved from the controller</param>
        /// <param name="eventLog">reference to data structure to return the log into</param>
        /// <returns>feedback code indicating success or reason of failure</returns>
        //-------------------------------------------------------------------------
        //DR38000269
        public void processEventLog(ODItemData odSub, out EventLogEntry[] eventLog, out ArrayList unknownFaultIDs)
        {
            #region comment
            /* Known size of the each event log entry (as defined in EventLogEntry data structure).
			 * However, the controller pads so that 16 or 32 bit numbers always start on an
			 * even address so the data structure received is:
			 *			Event ID           2 bytes
			 *			1st Hours          2 bytes
			 *			1st Mins           1 byte
			 *			Padding            1 byte		(to be ignored)
			 *			Last Hours         2 bytes
			 *			Last Mins          1 byte
			 *			Padding            1 byte		(to be ignored)
			 *			Counter			   2 bytes
			 * Hence size is 12 bytes and not the expected 10.
			 */
            #endregion

            #region local variable declarations and variable initialisation
            eventLog = new EventLogEntry[0];
            #endregion
            unknownFaultIDs = new ArrayList();  //DR38000269 clear out any old unknowns
            #region check fo rempty log
            if (odSub.currentValueDomain == null)
            {
                return;//empty log
            }
            #endregion check fo rempty log

            #region reconstruct raw data into EventLogEntry data structure
            /* If retrieved the domain OK and log length indicates data there, reconstruct the structure
									 * from the raw byte stream retrieved. This is dependent on a fixed, known data
									 * structure sent in the little endian format.
									 */
            int logLength = (int)(odSub.currentValueDomain.Length / SCCorpStyle.sizeOfEventLogEntry);
            EventLogEntry[] tempLog = new EventLogEntry[logLength];

            // for each event log entry, reconstruct the data
            for (int i = 0; i < logLength; i++)
            {
                #region reconstruct the eventLogEntry data form the domain data
                tempLog[i] = new EventLogEntry();
                tempLog[i].eventID = (UInt16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventLogEntry) + 0];
                tempLog[i].eventID += (UInt16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventLogEntry) + 1]) << 8);
                tempLog[i].firstHours = (UInt16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventLogEntry) + 2];
                tempLog[i].firstHours += (UInt16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventLogEntry) + 3]) << 8);
                tempLog[i].firstMinsAndSecs = (Byte)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventLogEntry) + 4];
                tempLog[i].lastHours = (UInt16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventLogEntry) + 6];
                tempLog[i].lastHours += (UInt16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventLogEntry) + 7]) << 8);
                tempLog[i].lastMinsAndSecs = (Byte)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventLogEntry) + 8];
                tempLog[i].counter = (UInt16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventLogEntry) + 10];
                tempLog[i].counter += (UInt16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventLogEntry) + 11]) << 8);
                #endregion

                #region get the associated text descriptor for this eventID number
                /* A text descriptor of what the eventID number means is useful to the user.
											 * Known IDs are held in a text file and read in on startup into an array
											 * list.  Check this to get the descriptor for this fault/event.  If the
											 * ID is unknown, retrieve the descriptor text string from the controller
											 * and update the array list. NOTE: the text file will be updated once 
											 * when DW is shut down.
											 *
											 * Mask off the save bit (bit 15) as this is not part of the event identifier.
											 */
                UInt16 eventID = (UInt16)(tempLog[i].eventID & 0x7fff);
                tempLog[i].description = getEventDescriptionFromEventID(eventID, odSub.CANnode.productCode);

                if (tempLog[i].description == SCCorpStyle.UnknownIDDescriptor)
                {
                    unknownFaultIDs.Add(tempLog[i].eventID);    //DR38000269
                }
                #endregion
            }

            // make ref value (for log return) point to the newly constructed log
            eventLog = tempLog;
            #endregion reconstruct raw data into EventLogEntry data structure
        }


        //-------------------------------------------------------------------------
        ///<summary>Retrieves an operational log from node of nodeID if it is a Sevcon node.</summary>
        /// <param name="nodeID">ID of node to have new value written into</param>
        /// <param name="logtype">type of log to be retrieved from the controller</param>
        /// <param name="opLog">reference to data structure to return the log into</param>
        /// <param name="formatting">scaling and units to be applied to the opLog before display</param>
        /// <returns>feedback code indicating success or reason of failure</returns>
        //-------------------------------------------------------------------------
        public void processOperationalLog(ODItemData odSub, LogType logtype, out OperationalLog[] opLog, out OperationalLogFormatting formatting)
        {
            #region local variable declarations and variable initialisation
            formatting = new OperationalLogFormatting();
            formatting.batteryVoltsScaling = 1F;
            opLog = new OperationalLog[0];
            #endregion

            #region Only worth retrieving the log domain if the length is more than zero.

            if (odSub.currentValueDomain == null)
            {
                return;  //empty log
            }
            #region comments
            /* Reconstruct the structure
				 * from the raw byte stream retrieved. This is dependent on a fixed, known data
				 * structure sent in the little endian format.
				 */
            #endregion comments

            int logLength = (int)(odSub.currentValueDomain.Length / SCCorpStyle.sizeOfOpLogEntry);
            OperationalLog[] tempLog = new OperationalLog[logLength];
            // for each op log entry, reconstruct the data (only expecting one entry)
            for (int i = 0; i < logLength; i++)
            {
                #region reconstruct the operational log data from the raw domain data
                tempLog[i] = new OperationalLog();
                tempLog[i].batteryVoltsMin = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 0];
                tempLog[i].batteryVoltsMin += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 1]) << 8);
                tempLog[i].batteryVoltsMax = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 2];
                tempLog[i].batteryVoltsMax += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 3]) << 8);
                tempLog[i].capacitorVoltsMin = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 4];
                tempLog[i].capacitorVoltsMin += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 5]) << 8);
                tempLog[i].capacitorVoltsMax = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 6];
                tempLog[i].capacitorVoltsMax += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 7]) << 8);
                tempLog[i].motorCurrent1Min = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 8];
                tempLog[i].motorCurrent1Min += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 9]) << 8);
                tempLog[i].motorCurrent1Max = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 10];
                tempLog[i].motorCurrent1Max += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 11]) << 8);
                tempLog[i].motorCurrent2Min = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 12];
                tempLog[i].motorCurrent2Min += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 13]) << 8);
                tempLog[i].motorCurrent2Max = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 14];
                tempLog[i].motorCurrent2Max += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 15]) << 8);
                tempLog[i].motorSpeedMin = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 16];
                tempLog[i].motorSpeedMin += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 17]) << 8);
                tempLog[i].motorSpeedMax = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 18];
                tempLog[i].motorSpeedMax += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 19]) << 8);
                tempLog[i].temperatureMin = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 20];
                tempLog[i].temperatureMin += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 21]) << 8);
                tempLog[i].temperatureMax = (Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 22];
                tempLog[i].temperatureMax += (Int16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfOpLogEntry) + 23]) << 8);
                #endregion
            }
            // no event IDs associated with an operational log.
            #region extract the operational log items scaling for ease of display by the GUI
            #region comments
            /*
									 * Pull out all the scaling from the relevant sub objects of the OPERATIONAL_MONITOR
									 * log.  Bit clumsy pulling out scaling but subs not guaranteed to be in order or
									 * contigous so no other option available.
									 */
            #endregion comments
            SevconObjectType logObjectType;
            #region get the log type object
            if (logtype == LogType.SEVCONOpLog)
            {
                logObjectType = SevconObjectType.SEVCON_OPERATIONAL_MONITOR;
            }
            else
            {
                logObjectType = SevconObjectType.CUST_OPERATIONAL_MONITOR;
            }
            #endregion

            for (int subObject = 2; subObject < 13; subObject += 2)
            {
                #region scale each log entry
                //Jude this odSus
                ODItemData odScalingSub = odSub.CANnode.getODSubFromObjectType(logObjectType, subObject);
                if (odScalingSub != null)
                {
                    switch (subObject)
                    {
                        #region battery volts scaling
                        case 2:
                            {
                                formatting.batteryVoltsScaling = odScalingSub.scaling;
                                formatting.batteryVoltsUnits = odScalingSub.units;
                                break;
                            }
                        #endregion

                        #region capacitor volts scaling
                        case 4:
                            {
                                formatting.capacitorVoltsScaling = odScalingSub.scaling;
                                formatting.capacitorVoltsUnits = odScalingSub.units;
                                break;
                            }
                        #endregion

                        #region motor current scaling
                        case 6:
                            {
                                formatting.motorCurrent1Scaling = odScalingSub.scaling;
                                formatting.motorCurrent1Units = odScalingSub.units;
                                break;
                            }

                        case 8:
                            {
                                formatting.motorCurrent2Scaling = odScalingSub.scaling;
                                formatting.motorCurrent2Units = odScalingSub.units;
                                break;
                            }
                        #endregion

                        #region motor speed scaling
                        case 10:
                            {
                                formatting.motorSpeedScaling = odScalingSub.scaling;
                                formatting.motorSpeedUnits = odScalingSub.units;
                                break;
                            }
                        #endregion

                        #region temperature scaling
                        case 12:
                            {
                                formatting.temperatureScaling = odScalingSub.scaling;
                                formatting.temperatureUnits = odScalingSub.units;
                                break;
                            }
                        #endregion

                        #region default
                        default:
                            {
                                break;
                            }
                        #endregion
                    } // end of switch statement
                } // end of if feedback = success
                #endregion scale each log entry
            } // end of for each scaling required
            #endregion

            // make ref value (for log return) point to the newly constructed log
            opLog = tempLog;
            #endregion Only worth retrieving the log domain if the length is more than zero.
        }


        //-------------------------------------------------------------------------
        //  Name			: getAllEventIDs()
        //  Description     : This function retrieves all event IDs from the controller
        //					  from the relevant sub of the DOMAIN_UPLOAD object.
        //					  When uploaded, a raw array of bytes is received and the 
        //					  re-construction of the array or eventIDs is dependent on DW 
        //					  knowing the format of the bytes being uploaded.  The DW 
        //					  verifies a valid data length of the event ID list.
        //					  Text descriptors for every event ID in the list is then
        //					  uploaded (even if it already has it in it's event ID array
        //					  list just in case this controller has a different meaning
        //					  for the event ID) and is used to update the eventID array list.
        //  Parameters      : nodeID - ID of node to retrieve the log from
        //					  completeEventIDList - reference to data structure to return the 
        //							complete list of valid event IDs and text descriptors
        //						    received from this controller
        //  Used Variables  : nodes[] - nodeInfo object representing the connected controller 
        //								with node ID of nodeID.
        //  Preconditions   : The system has been found and the node array has the OD 
        //					  constructed from the relevant EDS for each node and the node
        //					  for which the eventID list is requested MUST BE A SEVCON NODE.
        //  Post-conditions : The ref structure passed as a parameter is populated with the
        //					  latest entire list of event IDs and text descriptors as 
        //					  received from the controller.
        //  Return value    : feedback - feedback code indicating success or reason of failure
        //----------------------------------------------------------------------------
        ///<summary>Retrieves all possible event IDs used by the Sevcon controller of nodeID.</summary>
        /// <param name="nodeID">ID of node to have new value written into</param>
        /// <param name="completeEventIDList">complete list of valid event IDs and text descriptors received from this controller</param>
        /// <returns>feedback code indicating success or reason of failure</returns>
        //DR38000269
        public void processAllEventIDs(ODItemData odSub, out SortedList nodeEventList, out ArrayList unknownFaultIDs)
        {
            #region local variable declarations and variable initialisation
            int nodeEventIDListLength = 0;
            nodeEventList = new SortedList();
            #endregion
            unknownFaultIDs = new ArrayList();  //DR38000269 clear out any old unknowns

            #region obtain a copy of the log from the DW's updated representation of the controller OD
            if (odSub.currentValueDomain == null)
            {
                return;
            }
            #endregion obtain a copy of the log from the DW's updated representation of the controller OD

            // If all the above is OK then get the text string for each event ID.
            UInt16 eventID;
            string descriptor = "";
            #region For each valid event ID entry, retrieve the text descriptor from the controller.
            nodeEventIDListLength = (int)(odSub.currentValueDomain.Length / SCCorpStyle.sizeOfEventIDEntry);
            for (int i = 0; i < nodeEventIDListLength; i++)
            {
                #region extract the core eventID (mask off irrelevant bits)
                eventID = (UInt16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventIDEntry) + 0];
                eventID += (UInt16)(((Int16)odSub.currentValueDomain[(i * SCCorpStyle.sizeOfEventIDEntry) + 1]) << 8);
                eventID = (UInt16)(eventID & 0x7fff);
                #endregion

                #region retrieve the text descriptor for this eventID
                //see if event descrption is already in our list
                descriptor = getEventDescriptionFromEventID(eventID, odSub.CANnode.productCode);

                if (descriptor == SCCorpStyle.UnknownIDDescriptor)
                {
                    //Jude when creating an event list from node use "" NOT SCCorpStyle.UnknownIDDescriptor
                    // tihs allows the GUI to locate next instance of "" indicating that it must attempt
                    //to read this event description from the node
                    descriptor = "";
                    unknownFaultIDs.Add(eventID);   //DR38000269
                }
                nodeEventList.Add(eventID, descriptor);
                #endregion
            }
            #endregion

        }
        #endregion log domains processing
    }


       

    public class COBsInSystemSortClass : IComparer  
	{// Calls CaseInsensitiveComparer.Compare with the parameters reversed.

		string propertyName = "";
		public COBsInSystemSortClass(string passed_PropertyName)
		{
			propertyName = passed_PropertyName.ToUpper();
		}
		int IComparer.Compare( Object x, Object y )  
		{
			COBObject _x = (COBObject) x;
			COBObject _y = (COBObject) y;
			switch (propertyName)
			{
				case "COBID":
					return( (new CaseInsensitiveComparer()).Compare( _x.COBID , _y.COBID) );

				case "NAME":
				default:
					return( (new CaseInsensitiveComparer()).Compare( _x.name , _y.name) );
			}
		
		}
	}

	/// <summary>
	/// Message class: used to override the Windows provided MessageBox class.
	/// This is needed so that during the auto validation testing, all user
	/// output message boxes can be redirected to be output to a text file.
	/// </summary>
	public class Message
	{
		//-------------------------------------------------------------------------
		/// <summary>Displays a message box to the user if auto validation is not running, otherwise
		/// it redirects the message to the debug output text file.</summary>
		/// <param name="text">Text string to be displayed in the message box</param>
		//-------------------------------------------------------------------------
		public static void Show( string text )
		{
//			if ( AutoValidate.staticValidationRunning == false )
			{
					System.Windows.Forms.MessageBox.Show( text );
			}
//			else
//			{
//				text = text.Replace( "\n", " " );
//				Debug.Write( "MESSAGEBOX.SHOW : " + text );
//			}
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// Displays a message box to the user if auto validation is not running, otherwise
		/// it redirects the message to the debug output text file and sets the dialog result
		/// to Yes.
		/// </summary>
		/// <param name="owner">Window form who is the owner of the message</param>
		/// <param name="message">Text string to be displayed in the message box</param>
		/// <param name="caption">Text string caption to be displayed at the top of the message box</param>
		/// <param name="buttons">Which buttons need to be displayed at the bottom of the message box</param>
		/// <param name="icons">Which icons are required to be displayed at the side of the message box</param>
		/// <param name="defaultButton">The default selected button on the message box</param>
		/// <returns>The dialog result ie which button the selection the user has selected</returns>
		//-------------------------------------------------------------------------
		public static DialogResult Show( System.Windows.Forms.IWin32Window owner, string message, string caption, System.Windows.Forms.MessageBoxButtons buttons, System.Windows.Forms.MessageBoxIcon icons, System.Windows.Forms.MessageBoxDefaultButton defaultButton )
		{
			DialogResult result = DialogResult.Abort;

//			if ( AutoValidate.staticValidationRunning == false )
//			{
					result = MessageBox.Show( owner, message, caption, buttons,
						icons, defaultButton );
//			}
//			else
//			{
//				message = message.Replace( "\n", " " );
//				Debug.Write( "MESSAGEBOX.SHOW : " + message );
//				result = DialogResult.Yes;
//			}

			return ( result );
		}
	}

}
