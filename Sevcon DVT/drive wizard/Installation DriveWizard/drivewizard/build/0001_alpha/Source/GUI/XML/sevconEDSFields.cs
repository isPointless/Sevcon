/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.21$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:07/10/2008 17:04:04$
	$ModDate:17/09/2008 10:56:02$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
    Defines the SevconSectionType and SevconObjectType enumerations for runtime 
    identification of re-locatable objects in the OD with a specific meaning. 
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  62870: sevconEDSFields.cs 

   Rev 1.21    07/10/2008 17:04:04  ak
 SECURITY & BOOT_MEMORY_SPACE_5 onwards added


   Rev 1.20    05/12/2007 22:24:44  ak
 TC keywords added to source for version control.


*******************************************************************************/
namespace DriveWizard
{
	#region Sevcon section type
	
	/// <summary>
	/// Sevcon controllers have additional fields in the EDS file.  The SECTION_TYPE splits
	/// the objects in the dictionary into user-comprehensible sections.  DriveWizard uses
	/// the sections to filter and display only certain objects (as selected by the user).
	/// This is to help the user find the objects of interest at the time, for ease of use.
	/// All SECTION name types NO LONGER NEED to be known in advance.
	/// 
	/// ONLY ADD ANY NEW SECTION_TYPE NAMES THAT DRIVE WIZARD EXPLICITLY REFERS TO
	/// IN THE CODE. ALL OTHERS WILL BE ADDED AT RUN-TIME WHEN READING THE EDS FILES.
	///		**** SPELLING MUST BE AN EXACT MATCH TO THE EDS FILE ****
	/// </summary>
	public enum SevconSectionType 
	{
		// SEVCON CONTROLLER SECTIONS LISTED HERE
        // ONLY THOSE EXPLICITLY REFERED TO IN THE DRIVE WIZARD CODE ARE REQUIRED.

		// ALWAYS KEEP THIS AS THE FIRST ENUM
		NONE,

        CANOPENSETUP,
        FAULTLOG,
        DATALOGGING,
        SECURITY,
		UNASSIGNED,  //used for sections we don't recognise

        // ALWAYS KEEP THIS AS THE LAST ENUM
		LAST_SEVCON_SECTION_TYPE
	};
	#endregion

	#region Sevcon object type
	
	/// <summary>
	/// Sevcon controllers have additional fields in the EDS file.  The OBJECT_TYPE 
	/// is used by DW to read or write to specific objects (instead of using the index
	/// and sub-index). This makes DW robust to changes in the Sevcon EDS but cannot
	/// be applied to 3rd party controllers.
	/// All OBJECT name types NO LONGER NEED TO BE known in advance.
	/// 
    /// ONLY ADD ANY NEW OBJECT_TYPE NAMES THAT DRIVE WIZARD EXPLICITLY REFERS TO
    /// IN THE CODE. ALL OTHERS WILL BE ADDED AT RUN-TIME WHEN READING THE EDS FILES.
    ///		**** SPELLING MUST BE AN EXACT MATCH TO THE EDS FILE ****
    /// </summary>
	public enum SevconObjectType
	{
		// SEVCON CONTROLLER OBJECTS LISTED HERE
        // ONLY THOSE EXPLICITLY REFERED TO IN THE DRIVE WIZARD CODE ARE REQUIRED.

		// ALWAYS KEEP THIS AS THE FIRST ENUM
		NONE,

        ANALOGUE_SIGNAL_IN,
        ANALOGUE_SIGNAL_OUT,
        BOOTLOADER_INIT,
        CLEAR_FAULT,
        CLEAR_TO_FAULT_SEVERITY,
        CONFIG_CHECKSUM,
        CONTACTOR_PARAM,
        CONTROLLER_HOURS,
        CUST_OPERATIONAL_MONITOR,
        DEVICE_MEASUREMENTS,
        DIGITAL_SIGNAL_IN,
        DIGITAL_SIGNAL_OUT,
        DOMAIN_UPLOAD,
        ERROR_REGISTER,
        EVENT_COUNTER,
        EVENT_ID_DESCRIPTION,
        EXTERNAL_ROM_CHECKSUM,
        FAULTS_FIFO_CTRL,
        FORCE_TO_PREOP,
        GENERAL_ABORT_CODE,
        INDIRECT_OBJECT_UPDATE,
        INTERNAL_ROM_CHECKSUM,
        LOCAL_ALG_IN_MAPPING,
        LOCAL_ALG_OUT_MAPPING,
        LOCAL_DIG_IN_MAPPING,
        LOCAL_DIG_OUT_MAPPING,
        LOCAL_MOTOR_MAPPING,
        MANU_SW_VERSION,
        MASTER_SLAVE_CONFIG,
        MOTOR_DRIVE,
        NMT_STATE,
        NODE_FAULT_INFO,
        OTHER_EVENTLOG_CTRL,
        PASSWORD,
        PASSWORD_KEY,
        PHYSICAL_LAYER_SETTINGS,
        SERVICE_CONFIG,
        SEVCON_OPERATIONAL_MONITOR,
        SYSTEM_FIFO_CTRL,

        #region BOOTLOADER
        AVAIL_PROCESSORS,
        AVAIL_MEM_SPACES,
        BOOT_HW_VERSION,
        BOOT_SW_VERSION,
        BOOT_MEMORY_SPACE_HOST_INT_ROM,
        BOOT_MEMORY_SPACE_DSP_ROM,
        BOOT_MEMORY_SPACE_EEPROM,
        BOOT_MEMORY_SPACE_HOST_EXT_ROM,
        BOOT_MEMORY_SPACE_5,        //for future compatibility
        BOOT_MEMORY_SPACE_6,
        BOOT_MEMORY_SPACE_7,
        BOOT_MEMORY_SPACE_8,
        BOOT_MEMORY_SPACE_9,
        BOOT_MEMORY_SPACE_10,
        BOOT_MEMORY_SPACE_11,
        BOOT_MEMORY_SPACE_12,
        BOOT_MEMORY_SPACE_13,
        BOOT_MEMORY_SPACE_14,
        BOOT_MEMORY_SPACE_15,
        BOOT_MEMORY_SPACE_16,
        BOOT_MEMORY_SPACE_17,
        BOOT_MEMORY_SPACE_18,
        BOOT_MEMORY_SPACE_19,
        BOOT_MEMORY_SPACE_20,
        BOOT_MEMORY_SPACE_21,
        BOOT_MEMORY_SPACE_22,
        BOOT_MEMORY_SPACE_23,
        BOOT_MEMORY_SPACE_24,
        BOOT_MEMORY_SPACE_25,
        BOOT_MEMORY_SPACE_26,
        BOOT_MEMORY_SPACE_27,
        BOOT_MEMORY_SPACE_28,
        BOOT_MEMORY_SPACE_29,
        BOOT_MEMORY_SPACE_30,
        BOOT_MEMORY_SPACE_31,
        BOOT_MEMORY_SPACE_32,
        BOOT_MEMORY_SPACE_33,
        BOOT_MEMORY_SPACE_34,
        BOOT_MEMORY_SPACE_35,
        BOOT_MEMORY_SPACE_36,
        BOOT_MEMORY_SPACE_37,
        BOOT_MEMORY_SPACE_38,
        BOOT_MEMORY_SPACE_39,
        BOOT_MEMORY_SPACE_40,
        BOOT_MEMORY_SPACE_41,
        BOOT_MEMORY_SPACE_42,
        BOOT_MEMORY_SPACE_43,
        BOOT_MEMORY_SPACE_44,
        BOOT_MEMORY_SPACE_45,
        BOOT_MEMORY_SPACE_46,
        BOOT_MEMORY_SPACE_47,
        BOOT_MEMORY_SPACE_48,
        BOOT_MEMORY_SPACE_49,
        BOOT_MEMORY_SPACE_50,
        BOOT_ERROR_CODE,
        BOOT_COMPLETE,
        BOOT_RESET,
        BOOT_EEPROM_FORMAT,
        #endregion BOOTLOADER

        #region Self Char
        SELFCHAR_LINECONTACTOR,
        SELFCHAR_TESTSTATUS,
        SELFCHAR_TESTREQUEST,
        SELFCHAR_TESTRESPONSE,
        #endregion Self Char

		// ALWAYS KEEP THIS AS THE LAST ENUM
		LAST_SEVCON_OBJECT_TYPE
	};
	#endregion

}
