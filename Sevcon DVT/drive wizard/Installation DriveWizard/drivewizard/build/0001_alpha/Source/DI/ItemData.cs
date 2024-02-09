/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.4$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 21:13:48$
	$ModDate:05/12/2007 21:12:24$

ORIGINAL AUTHOR
    Alison King

DESCRIPTION
	ItemData class definition. 
    For future expansion.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36693: ItemData.cs 

   Rev 1.4    05/12/2007 21:13:48  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;

#if all_data_types
namespace DriveWizard
{
	#region enumerated data type definitions
	public enum ValueErrorType
	{
		valueOK,
		valueTooHigh,
		valueTooLow,
		invalidCharacters,
		unhandledDataType
	};
	#endregion

	#region itemData class
	/// <summary>
	/// Summary description for ItemData.
	/// </summary>
	public class ItemData
	{
		public int					indexNumber;
		public int					subNumber;
		public string				parameterName;
		public ObjectAccessType		accessType;
		public byte					dataType;
		public byte					objectType;		
		public bool					PDOmappable;
		public byte					objFlags;
		public float				scaling;
		public string				units;
		public CANOpenDataType		displayType;
		public byte					accessLevel;
		public SevconSectionType	sectionType;
		public SevconObjectType		objectName;

		public ItemData()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public DIFeedbackCode getCurrentValue( out string currentValue )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			currentValue = "";

			switch ( displayType )
			{
				case CANOpenDataType.BOOLEAN:
				case CANOpenDataType.INTEGER8:
				case CANOpenDataType.INTEGER16:
				case CANOpenDataType.INTEGER24:
				case CANOpenDataType.INTEGER32:
				case CANOpenDataType.INTEGER40:
				case CANOpenDataType.INTEGER48:
				case CANOpenDataType.INTEGER56:
				case CANOpenDataType.INTEGER64:
				case CANOpenDataType.UNSIGNED8:
				case CANOpenDataType.UNSIGNED16:
				case CANOpenDataType.UNSIGNED24:
				case CANOpenDataType.UNSIGNED32:
				case CANOpenDataType.UNSIGNED40:
				case CANOpenDataType.UNSIGNED48:
				case CANOpenDataType.UNSIGNED56:
				case CANOpenDataType.UNSIGNED64:
				{
					integerData data = (integerData)this;
					data.getCurrentValue( out currentValue );
					break;
				}

				case CANOpenDataType.REAL32:
				case CANOpenDataType.REAL64:
				{
					realData data = (realData)this;
					data.getCurrentValue( out currentValue );
					break;
				}

				case CANOpenDataType.VISIBLE_STRING:
				{
					stringData data = (stringData)this;
					data.getCurrentValue( out currentValue );
					break;
				}

				case CANOpenDataType.RECORD:
				case CANOpenDataType.ARRAY:
				{
					// data dictionary type header with no data associated
					break;
				}

				case CANOpenDataType.DOMAIN:
				{
					// not applicable overloaded function for this data type
					break;
				}

				case CANOpenDataType.IDENTITY:
				case CANOpenDataType.OCTET_STRING:
				case CANOpenDataType.PDO_COMMUNICATION_PARAMETER:
				case CANOpenDataType.PDO_MAPPING:
				case CANOpenDataType.SDO_PARAMETER:
				case CANOpenDataType.TIME_DIFFERENCE:
				case CANOpenDataType.TIME_OF_DAY:
				case CANOpenDataType.UNICODE_STRING:
				default:
				{
					// unhandled data type
					fbc = DIFeedbackCode.DIUnhandledDataType;
					break;
				}
			}

			return ( fbc );
		}

		public DIFeedbackCode getCurrentValue( ref byte[] currentValue )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;

			switch ( displayType )
			{
				case CANOpenDataType.DOMAIN:
				{
					domainData data = (domainData)this;
					data.getCurrentValue( ref currentValue );
					break;
				}

				default:
				{
					// not applicable to this data type
					fbc = DIFeedbackCode.DIGeneralFailure;
					break;
				}
			}

			return ( fbc );
		}

		public DIFeedbackCode setCurrentValue( ref byte[] newValue )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;

			switch ( displayType )
			{
				case CANOpenDataType.DOMAIN:
				{
					domainData data = (domainData)this;
					data.setCurrentValue( ref newValue );
					break;
				}

				default:
				{
					// not applicable to this data type
					fbc = DIFeedbackCode.DIGeneralFailure;
					break;
				}
			}

			return ( fbc );
		}

		public DIFeedbackCode setCurrentValue( string newValue )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;

			switch ( displayType )
			{
				case CANOpenDataType.BOOLEAN:
				case CANOpenDataType.INTEGER8:
				case CANOpenDataType.INTEGER16:
				case CANOpenDataType.INTEGER24:
				case CANOpenDataType.INTEGER32:
				case CANOpenDataType.INTEGER40:
				case CANOpenDataType.INTEGER48:
				case CANOpenDataType.INTEGER56:
				case CANOpenDataType.INTEGER64:
				case CANOpenDataType.UNSIGNED8:
				case CANOpenDataType.UNSIGNED16:
				case CANOpenDataType.UNSIGNED24:
				case CANOpenDataType.UNSIGNED32:
				case CANOpenDataType.UNSIGNED40:
				case CANOpenDataType.UNSIGNED48:
				case CANOpenDataType.UNSIGNED56:
				case CANOpenDataType.UNSIGNED64:
				{
					integerData data = (integerData)this;

					if ( data.inRange( newValue ) == ValueErrorType.valueOK )
					{
						data.setCurrentValue( newValue );
						fbc = DIFeedbackCode.DISuccess;
					}
					break;
				}

				case CANOpenDataType.REAL32:
				case CANOpenDataType.REAL64:
				{
					realData data = (realData)this;

					if ( data.inRange( newValue ) == ValueErrorType.valueOK )
					{
						data.setCurrentValue( newValue );
						fbc = DIFeedbackCode.DISuccess;
					}
					break;
				}

				case CANOpenDataType.VISIBLE_STRING:
				{
					stringData data = (stringData)this;

					if ( data.inRange( newValue ) == ValueErrorType.valueOK )
					{
						data.setCurrentValue( newValue );
						fbc = DIFeedbackCode.DISuccess;
					}
					break;
				}

				case CANOpenDataType.RECORD:
				case CANOpenDataType.ARRAY:
				{
					// data dictionary type header with no data associated
					break;
				}

				case CANOpenDataType.DOMAIN:
				{
					// not applicable overloaded function for this data type
					break;
				}

				case CANOpenDataType.IDENTITY:
				case CANOpenDataType.OCTET_STRING:
				case CANOpenDataType.PDO_COMMUNICATION_PARAMETER:
				case CANOpenDataType.PDO_MAPPING:
				case CANOpenDataType.SDO_PARAMETER:
				case CANOpenDataType.TIME_DIFFERENCE:
				case CANOpenDataType.TIME_OF_DAY:
				case CANOpenDataType.UNICODE_STRING:
				default:
				{
					// unhandled data type
					fbc = DIFeedbackCode.DIUnhandledDataType;
					break;
				}
			}

			return ( fbc );
		}

		public DIFeedbackCode getLowLimit( out string lowLimit )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			lowLimit = "";

			switch ( displayType )
			{
				case CANOpenDataType.BOOLEAN:
				case CANOpenDataType.INTEGER8:
				case CANOpenDataType.INTEGER16:
				case CANOpenDataType.INTEGER24:
				case CANOpenDataType.INTEGER32:
				case CANOpenDataType.INTEGER40:
				case CANOpenDataType.INTEGER48:
				case CANOpenDataType.INTEGER56:
				case CANOpenDataType.INTEGER64:
				case CANOpenDataType.UNSIGNED8:
				case CANOpenDataType.UNSIGNED16:
				case CANOpenDataType.UNSIGNED24:
				case CANOpenDataType.UNSIGNED32:
				case CANOpenDataType.UNSIGNED40:
				case CANOpenDataType.UNSIGNED48:
				case CANOpenDataType.UNSIGNED56:
				case CANOpenDataType.UNSIGNED64:
				{
					integerData data = (integerData)this;
					data.getLowLimit( out lowLimit );
					break;
				}

				case CANOpenDataType.REAL32:
				case CANOpenDataType.REAL64:
				{
					realData data = (realData)this;
					data.getLowLimit( out lowLimit );
					break;
				}

				case CANOpenDataType.VISIBLE_STRING:
				{
					stringData data = (stringData)this;
					data.getLowLimit( out lowLimit );
					break;
				}

				case CANOpenDataType.RECORD:
				case CANOpenDataType.ARRAY:
				{
					// data dictionary type header with no data associated
					break;
				}

				case CANOpenDataType.DOMAIN:
				{
					domainData data = (domainData)this;
					data.getLowLimit( out lowLimit );
					break;
				}

				case CANOpenDataType.IDENTITY:
				case CANOpenDataType.OCTET_STRING:
				case CANOpenDataType.PDO_COMMUNICATION_PARAMETER:
				case CANOpenDataType.PDO_MAPPING:
				case CANOpenDataType.SDO_PARAMETER:
				case CANOpenDataType.TIME_DIFFERENCE:
				case CANOpenDataType.TIME_OF_DAY:
				case CANOpenDataType.UNICODE_STRING:
				default:
				{
					// unhandled data type
					fbc = DIFeedbackCode.DIUnhandledDataType;
					break;
				}
			}

			return ( fbc );
		}

		public DIFeedbackCode getHighLimit( out string highLimit )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;
			highLimit = "";

			switch ( displayType )
			{
				case CANOpenDataType.BOOLEAN:
				case CANOpenDataType.INTEGER8:
				case CANOpenDataType.INTEGER16:
				case CANOpenDataType.INTEGER24:
				case CANOpenDataType.INTEGER32:
				case CANOpenDataType.INTEGER40:
				case CANOpenDataType.INTEGER48:
				case CANOpenDataType.INTEGER56:
				case CANOpenDataType.INTEGER64:
				case CANOpenDataType.UNSIGNED8:
				case CANOpenDataType.UNSIGNED16:
				case CANOpenDataType.UNSIGNED24:
				case CANOpenDataType.UNSIGNED32:
				case CANOpenDataType.UNSIGNED40:
				case CANOpenDataType.UNSIGNED48:
				case CANOpenDataType.UNSIGNED56:
				case CANOpenDataType.UNSIGNED64:
				{
					integerData data = (integerData)this;
					data.getHighLimit( out highLimit );
					break;
				}

				case CANOpenDataType.REAL32:
				case CANOpenDataType.REAL64:
				{
					realData data = (realData)this;
					data.getHighLimit( out highLimit );
					break;
				}

				case CANOpenDataType.VISIBLE_STRING:
				{
					stringData data = (stringData)this;
					data.getHighLimit( out highLimit );
					break;
				}

				case CANOpenDataType.RECORD:
				case CANOpenDataType.ARRAY:
				{
					// data dictionary type header with no data associated
					break;
				}

				case CANOpenDataType.DOMAIN:
				{
					domainData data = (domainData)this;
					data.getHighLimit( out highLimit );
					break;
				}

				case CANOpenDataType.IDENTITY:
				case CANOpenDataType.OCTET_STRING:
				case CANOpenDataType.PDO_COMMUNICATION_PARAMETER:
				case CANOpenDataType.PDO_MAPPING:
				case CANOpenDataType.SDO_PARAMETER:
				case CANOpenDataType.TIME_DIFFERENCE:
				case CANOpenDataType.TIME_OF_DAY:
				case CANOpenDataType.UNICODE_STRING:
				default:
				{
					// unhandled data type
					fbc = DIFeedbackCode.DIUnhandledDataType;
					break;
				}
			}

			return ( fbc );
		}

		public ValueErrorType inRange( string newValue )
		{
			ValueErrorType error = ValueErrorType.valueOK;

			switch ( displayType )
			{
				case CANOpenDataType.BOOLEAN:
				case CANOpenDataType.INTEGER8:
				case CANOpenDataType.INTEGER16:
				case CANOpenDataType.INTEGER24:
				case CANOpenDataType.INTEGER32:
				case CANOpenDataType.INTEGER40:
				case CANOpenDataType.INTEGER48:
				case CANOpenDataType.INTEGER56:
				case CANOpenDataType.INTEGER64:
				case CANOpenDataType.UNSIGNED8:
				case CANOpenDataType.UNSIGNED16:
				case CANOpenDataType.UNSIGNED24:
				case CANOpenDataType.UNSIGNED32:
				case CANOpenDataType.UNSIGNED40:
				case CANOpenDataType.UNSIGNED48:
				case CANOpenDataType.UNSIGNED56:
				case CANOpenDataType.UNSIGNED64:
				{
					integerData data = (integerData)this;
					error = data.inRange( newValue );
					break;
				}

				case CANOpenDataType.REAL32:
				case CANOpenDataType.REAL64:
				{
					realData data = (realData)this;
					error = data.inRange( newValue );
					break;
				}

				case CANOpenDataType.VISIBLE_STRING:
				{
					stringData data = (stringData)this;
					error = data.inRange( newValue );
					break;
				}

				case CANOpenDataType.RECORD:
				case CANOpenDataType.ARRAY:
				{
					// data dictionary type header with no data associated
					break;
				}

				case CANOpenDataType.DOMAIN:
				{
					domainData data = (domainData)this;
					error = data.inRange( newValue );
					break;
				}

				case CANOpenDataType.IDENTITY:
				case CANOpenDataType.OCTET_STRING:
				case CANOpenDataType.PDO_COMMUNICATION_PARAMETER:
				case CANOpenDataType.PDO_MAPPING:
				case CANOpenDataType.SDO_PARAMETER:
				case CANOpenDataType.TIME_DIFFERENCE:
				case CANOpenDataType.TIME_OF_DAY:
				case CANOpenDataType.UNICODE_STRING:
				default:
				{
					// unhandled data type
					error = ValueErrorType.unhandledDataType;
					break;
				}
			}

			return ( error );
		}
	}
	#endregion

	#region integerData class
	/**************************************************************************
	 *  integerData class
	 * 
	 * 
	 * 
	 **************************************************************************/
	public class integerData : ItemData
	{
		private long	currentValue;
		private long	lowLimit;
		private long	highLimit;
		private long	defaultValue;
		
		public integerData()
		{
		}

		private string getCurrentValue()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = currentValue.ToString();
			}
			else
			{
				float scaledValue = currentValue * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		private new ValueErrorType setCurrentValue( string newValue )
		{
			ValueErrorType error = inRange( newValue );

			// assign new value
			if ( error == ValueErrorType.valueOK )
			{
				// convert newValue into int
				currentValue = 0;
			}

			return ( error );
		}

		private string getLowLimit()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = lowLimit.ToString();
			}
			else
			{
				float scaledValue = lowLimit * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		private string getHighLimit()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = highLimit.ToString();
			}
			else
			{
				float scaledValue = highLimit * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		private string getDefaultValue()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = defaultValue.ToString();
			}
			else
			{
				float scaledValue = defaultValue * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		public new ValueErrorType inRange( string newValue )
		{
			ValueErrorType error = ValueErrorType.valueOK;
			
			return( error );
		}
	}
	#endregion

	#region real64Data class
	/**************************************************************************
	 *  real64Data class
	 * 
	 * 
	 * 
	 **************************************************************************/
	public class real64Data : ItemData
	{
		private double	currentValue;
		private double	lowLimit;
		private double	highLimit;
		private double	defaultValue;
		
		public real64Data()
		{
		}

		private string getCurrentValue()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = currentValue.ToString();
			}
			else
			{
				double scaledValue = currentValue * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		private new ValueErrorType setCurrentValue( string newValue )
		{
			ValueErrorType error = inRange( newValue );

			// assign new value
			if ( error == ValueErrorType.valueOK )
			{
				// convert newValue into double
				currentValue = 0.0D;
			}

			return ( error );
		}

		private string getLowLimit()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = lowLimit.ToString();
			}
			else
			{
				double scaledValue = lowLimit * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		private string getHighLimit()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = highLimit.ToString();
			}
			else
			{
				double scaledValue = highLimit * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		private string getDefaultValue()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = defaultValue.ToString();
			}
			else
			{
				double scaledValue = defaultValue * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		public new ValueErrorType inRange( string newValue )
		{
			ValueErrorType error = ValueErrorType.valueOK;
			
			return( error );
		}
	}
	#endregion

	#region real32Data class
	/**************************************************************************
	 *  real32Data class
	 * 
	 * 
	 * 
	 **************************************************************************/
	public class real32Data : ItemData
	{
		private float	currentValue;
		private float	lowLimit;
		private float	highLimit;
		private float	defaultValue;
		
		public real32Data()
		{
		}

		private string getCurrentValue()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = currentValue.ToString();
			}
			else
			{
				float scaledValue = currentValue * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		private new ValueErrorType setCurrentValue( string newValue )
		{
			ValueErrorType error = inRange( newValue );

			// assign new value
			if ( error == ValueErrorType.valueOK )
			{
				// convert newValue into double
				currentValue = 0.0F;
			}

			return ( error );
		}

		private string getLowLimit()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = lowLimit.ToString();
			}
			else
			{
				float scaledValue = lowLimit * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		private string getHighLimit()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = highLimit.ToString();
			}
			else
			{
				float scaledValue = highLimit * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		private string getDefaultValue()
		{
			string valueString;

			if ( scaling != 1.0F )
			{
				valueString = defaultValue.ToString();
			}
			else
			{
				float scaledValue = defaultValue * scaling;
				valueString = scaledValue.ToString();
			}

			return ( valueString );
		}

		public new ValueErrorType inRange( string newValue )
		{
			ValueErrorType error = ValueErrorType.valueOK;
			
			return( error );
		}
	}
	#endregion

	#region stringData class
	/**************************************************************************
	 *  stringData class
	 * 
	 * 
	 * 
	 **************************************************************************/
	public class stringData : ItemData
	{
		private string	currentValue;
		private string	lowLimit;
		private string	highLimit;
		private string	defaultValue;
		
		public stringData()
		{
		}

		private string getCurrentValue()
		{
			return ( currentValue );
		}

		private new DIFeedbackCode setCurrentValue( string newValue )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DIGeneralFailure;

			// assign new value
			currentValue = newValue;
			return ( fbc );
		}

		private string getLowLimit()
		{
			return ( lowLimit );
		}

		private string getHighLimit()
		{
			return ( highLimit );
		}

		private string getDefaultValue()
		{
			return ( defaultValue );
		}

		public new ValueErrorType inRange( string newValue )
		{
			ValueErrorType error = ValueErrorType.valueOK;
			
			return( error );
		}
	}
	#endregion

	#region domainData class
	/**************************************************************************
	 *  domainData class
	 * 
	 * 
	 * 
	 **************************************************************************/
	public class domainData : ItemData
	{
		public byte[]	currentValue;
		private byte[]	lowLimit;
		private byte[]	highLimit;
		private byte[]	defaultValue;
		
		public domainData()
		{
		}

		private new void getCurrentValue( ref byte[] cvalue )
		{
			if ( currentValue != null )
			{
				cvalue = currentValue;
			}
			else
			{
				cvalue = null;
			}
		}

		private new DIFeedbackCode setCurrentValue( ref byte[] newValue )
		{
			DIFeedbackCode fbc = DIFeedbackCode.DISuccess;

			// assign new value
			currentValue = new byte [ newValue.Length ];
			newValue.CopyTo( currentValue, newValue.Length );

			return ( fbc );
		}

		private string getLowLimit()
		{
			if ( lowLimit != null )
			{
				return ( lowLimit.ToString() );
			}
			else
			{
				return ( "" );
			}
		}

		private string getHighLimit()
		{
			if ( highLimit != null )
			{
				return ( highLimit.ToString() );
			}
			else
			{
				return ( "" );
			}
		}

		private string getDefaultValue()
		{
			if ( defaultValue != null )
			{
				return ( defaultValue.ToString() );
			}
			else
			{
				return ( "" );
			}
		}

		public new ValueErrorType inRange( string newValue )
		{
			ValueErrorType error = ValueErrorType.valueOK;
			
			return( error );
		}
	}
	#endregion
}
#endif
