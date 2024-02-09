/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.5$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:24:46$
	$ModDate:05/12/2007 22:17:36$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
    Custom class used as a wrapper to the XML serialization of an object to/from an XML file.
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  105642: ObjectXMLSerializer.cs 

   Rev 1.5    05/12/2007 22:24:46  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Xml.Serialization;	 
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;				 
using System.ComponentModel;	 
using System.IO.IsolatedStorage; 

namespace DriveWizard
{
	public enum SerializedFormatType
	{
		Binary, Document
	}	

	/// <summary>
	/// Custom class used as a wrapper to the XML serialization of an object to/from an XML file.
	/// See method calls 'Load' and 'Save' for usage.
	/// </summary>
	public class ObjectXMLSerializer
	{
		/// <summary>
		/// Constructor for this class.
		/// </summary>
		public ObjectXMLSerializer()
		{
		}

		/// <summary>
		/// Load an object from an XML file that is in an XML Document format.
		/// <newpara></newpara>
		/// <example>
		/// See Load method that uses the SerializedFormatType argument for more information.
		/// </example>
		/// </summary>
		public virtual Object Load( Object ObjectToLoad, string XMLFilePathName )
		{   		
			try
			{
				ObjectToLoad = LoadFromDocumentFormat( ObjectToLoad, XMLFilePathName, null );
			}
			catch(Exception e)
			{
//				SystemInfo.errorSB.Append("Failed to load XML document. Exception data = ");
//				SystemInfo.errorSB.Append(e.Message);
//				SystemInfo.errorSB.Append(e.InnerException);
			}

			return ( ObjectToLoad );
		}

		/// <summary>
		/// Load an object from an XML file that is in the specified format.
		/// <newpara></newpara>
		/// <example>
		/// The following example loads serialized data (XML Document format) into a 'Test' class object,
		/// from the XML file 'Objects as XML.xml':
		/// <newpara></newpara>
		/// <code>
		/// Test test = new Test(); //Must use new to create the object - cannot set to null.
		/// ObjectXMLSerializer objectXMLSerializer = new ObjectXMLSerializer();
		/// test = (Test) objectXMLSerializer.Load(test, "Objects as XML.xml", SerializedFormatType.Document);
		/// </code>
		/// </example>
		/// </summary>
		/// <param name="ObjectToLoad">Object to be loaded.</param>
		/// <param name="XMLFilePathName">File Path name of the XML file containing object(s) serialized to XML.</param>
		/// <param name="SerializedFormat">XML serialized format to load the object from.</param>
		/// <returns>Returns an Object loaded from the XML file. If the Object could not be loaded returns null.</returns>
		public virtual Object Load( Object ObjectToLoad, string XMLFilePathName, SerializedFormatType SerializedFormat )
		{   
			switch ( SerializedFormat )
			{
				case SerializedFormatType.Binary:
				{
					ObjectToLoad = LoadFromBinaryFormat( ObjectToLoad, XMLFilePathName, null );
					break;
				}

				case SerializedFormatType.Document:
				default:
				{
					ObjectToLoad = LoadFromDocumentFormat( ObjectToLoad, XMLFilePathName, null );
					break; 
				}
			}
		
			return ( ObjectToLoad );
		}

		public virtual Object Load( Object ObjectToLoad, string XMLFilePathName, 
			SerializedFormatType SerializedFormat, IsolatedStorageFile isolatedStorageFolder )
		{   
			switch ( SerializedFormat )
			{
				case SerializedFormatType.Binary:
				{
					ObjectToLoad = this.LoadFromBinaryFormat(ObjectToLoad, XMLFilePathName, isolatedStorageFolder);
					break;
				}

				case SerializedFormatType.Document:
				default:
				{
					ObjectToLoad = this.LoadFromDocumentFormat(ObjectToLoad, XMLFilePathName, isolatedStorageFolder);
					break; 
				}
			}
		
			return ( ObjectToLoad );
		}

		/// <summary>
		/// Load an object from an XML file that is in an XML Document format, at a Isolated storage location.
		/// </summary>
		/// <param name="ObjectToLoad">Object to be loaded.</param>
		/// <param name="XMLFilePathName">File name (no path) of the XML file containing object(s) serialized to XML.</param>
		/// <param name="isolatedStorageFolder">Isolated Storage object that is a user and assembly specific folder location
		/// from which to Load the XML file.</param>
		/// <returns>Returns an Object loaded from the XML file. If the Object could not be loaded returns null.</returns>
		public virtual Object Load( Object ObjectToLoad, string XMLFilePathName, IsolatedStorageFile isolatedStorageFolder )
		{
			ObjectToLoad = LoadFromDocumentFormat( ObjectToLoad, XMLFilePathName, isolatedStorageFolder );

			return ( ObjectToLoad );
		}

		private Object LoadFromBinaryFormat( Object ObjectToLoad, string XMLFilePathName, IsolatedStorageFile isolatedStorageFolder )
		{   	
			FileStream fileStream = null;

			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();

				if ( isolatedStorageFolder == null )
				{
					fileStream = new FileStream( XMLFilePathName, FileMode.Open );
				}
				else
				{
					fileStream = new IsolatedStorageFileStream( XMLFilePathName, FileMode.Open, isolatedStorageFolder );
				}

				ObjectToLoad = binaryFormatter.Deserialize( fileStream );
			}
			finally
			{
				//Make sure to close the file even if an exception is raised...
				if ( fileStream != null )
				{
					fileStream.Close();				
				}
			}			

			return ( ObjectToLoad );
		}

		private Object LoadFromDocumentFormat( Object ObjectToLoad, string XMLFilePathName, IsolatedStorageFile isolatedStorageFolder )
		{   	
			TextReader txrTextReader = null;
			try
			{
				Type ObjectType = ObjectToLoad.GetType();
				XmlSerializer xserDocumentSerializer = null;
				try
				{
					xserDocumentSerializer = new XmlSerializer( ObjectType );
				}
				catch(Exception e1)
				{
//					SystemInfo.errorSB.Append("\nError in inner document serialization.Exception");
//					SystemInfo.errorSB.Append(e1.Message);
//					SystemInfo.errorSB.Append("\n InnerException: ");
//					SystemInfo.errorSB.Append(e1.InnerException);
				}

				if ( isolatedStorageFolder == null )
				{
					txrTextReader = new StreamReader( XMLFilePathName );
				}
				else
				{
					txrTextReader = new StreamReader( new IsolatedStorageFileStream( XMLFilePathName, FileMode.Open, isolatedStorageFolder ) );
				}

				try
				{
					ObjectToLoad = xserDocumentSerializer.Deserialize( txrTextReader );
				}
				catch(Exception e2)
				{
//					SystemInfo.errorSB.Append("\nError in inner document serialization.Exception");
//					SystemInfo.errorSB.Append(e2.Message);
//					SystemInfo.errorSB.Append("\n InnerException: ");
//					SystemInfo.errorSB.Append(e2.InnerException);
				}
			}
			catch(Exception e4)
			{
//				SystemInfo.errorSB.Append("\nError in inner document serialization.Exception");
//				SystemInfo.errorSB.Append(e4.Message);
//				SystemInfo.errorSB.Append("\n InnerException: ");
//				SystemInfo.errorSB.Append(e4.InnerException);
			}
			finally
			{
				//Make sure to close the file even if an exception is raised...
				if ( txrTextReader != null )
				{
					txrTextReader.Close();				
				}
			}			

			return ( ObjectToLoad );
		}

		/// <summary>
		/// Save an object to an XML file that is in an XML Document format.
		/// <newpara></newpara>
		/// <example>
		/// See Save method that uses the SerializedFormatType argument for more information.
		/// </example>
		/// </summary>
		public virtual bool Save( Object ObjectToSave, string XMLFilePathName )
		{
			bool success = false;
			success = this.SaveToDocumentFormat( ObjectToSave, XMLFilePathName, null );			

			return ( success );
		}

		/// <summary>
		/// Save an object to an XML file that is in the specified format.
		/// <newpara></newpara>
		/// <example>
		/// The following example saves a 'Test' class object (XML Document format) to the XML 
		/// file 'Objects as XML.xml':
		/// <newpara></newpara>
		/// <code>
		/// Test objTest = new Test();  //Must use new to create the object - cannot set to null.
		/// ObjectXMLSerializer objObjectXMLSerializer = new ObjectXMLSerializer();
		/// bool success = objObjectXMLSerializer.Save(objTest, "Objects as XML.xml", SerializedFormatType.Document);
		/// </code>
		/// </example>
		/// </summary>
		/// <param name="ObjectToSave">Object to be saved.</param>
		/// <param name="XMLFilePathName">File Path name of the XML file to contain the object serialized to XML.</param>
		/// <param name="SerializedFormat">XML serialized format to load the object from.</param>
		/// <returns>Returns success of the object save.</returns>
		public virtual bool Save( Object ObjectToSave, string XMLFilePathName, SerializedFormatType SerializedFormat )
		{
			bool success = false;

			switch ( SerializedFormat )
			{
				case SerializedFormatType.Binary:
				{
					success = SaveToBinaryFormat( ObjectToSave, XMLFilePathName, null );
					break;
				}

				case SerializedFormatType.Document:
				default:
				{
					success = SaveToDocumentFormat( ObjectToSave, XMLFilePathName, null );
					break; 
				}
			}
		
			return ( success );
		}

		public virtual bool Save( Object ObjectToSave, string XMLFilePathName, 
			SerializedFormatType SerializedFormat, IsolatedStorageFile isolatedStorageFolder )
		{
			bool success = false;

			switch ( SerializedFormat )
			{
				case SerializedFormatType.Binary:
				{
					success = SaveToBinaryFormat( ObjectToSave, XMLFilePathName, isolatedStorageFolder );
					break;
				}

				case SerializedFormatType.Document:
				default:
				{
					success = SaveToDocumentFormat( ObjectToSave, XMLFilePathName, isolatedStorageFolder );
					break; 
				}
			}
		
			return ( success );
		}


		/// <summary>
		/// Save an object to an XML file that is in an XML Document forward, at a Isolated storage location.
		/// </summary>
		/// <param name="ObjectToSave">Object to be saved.</param>
		/// <param name="XMLFilePathName">File name (no path) of the XML file to contain the object serialized to XML.</param>
		/// <param name="isolatedStorageFolder">Isolated Storage object that is a user and assembly specific folder location
		/// from which to save the XML file.</param>
		/// <returns></returns>
		public virtual bool Save(  Object ObjectToSave, string XMLFilePathName, IsolatedStorageFile isolatedStorageFolder )
		{
			bool success = false;
			success = SaveToDocumentFormat( ObjectToSave, XMLFilePathName, isolatedStorageFolder );

			return ( success );
		}

		private bool SaveToDocumentFormat( Object ObjectToSave, 
			string XMLFilePathName, IsolatedStorageFile isolatedStorageFolder )
		{
			TextWriter textWriter = null;
			bool success = false;
			Type ObjectType = ObjectToSave.GetType();
			try
			{
				XmlSerializer xmlSerializer = null;
				//Create serializer object using the type name of the Object to serialize.
				try
				{
					xmlSerializer = new XmlSerializer( ObjectType );
				}
				catch(Exception e1)
				{
					//TODO error message eneeded here
					string temp = e1.ToString();
					string temp1 = e1.InnerException.ToString();
				}

				if (isolatedStorageFolder == null)
				{
					try
					{
						textWriter = new StreamWriter( XMLFilePathName );
					}
					catch
					{
#if DEBUG
						//TODO error message eneeded here
#endif						
					}
				}
				else
				{
					textWriter = new StreamWriter( new IsolatedStorageFileStream( XMLFilePathName, FileMode.OpenOrCreate, isolatedStorageFolder ) );
				}
				try
				{
					xmlSerializer.Serialize( textWriter, ObjectToSave );
				}
				catch(Exception e)
				{
#if DEBUG
					//TODO error message eneeded here
					string temp = e.ToString();
					string inner = e.InnerException.ToString();
#endif
				}

				success = true;
			}
			finally
			{
				//Make sure to close the file even if an exception is raised...
				if ( textWriter != null )
				{
					textWriter.Close();								
				}
			}

			return ( success );
		}

		private bool SaveToBinaryFormat( Object ObjectToSave, 
			string XMLFilePathName, IsolatedStorageFile isolatedStorageFolder )
		{
			FileStream fileStream = null;
			bool success = false;

			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();

				if ( isolatedStorageFolder == null )
				{
					fileStream = new FileStream( XMLFilePathName, FileMode.OpenOrCreate );
				}
				else
				{
					fileStream = new IsolatedStorageFileStream( XMLFilePathName, FileMode.OpenOrCreate, isolatedStorageFolder );
				}
				binaryFormatter.Serialize( fileStream, ObjectToSave );
				success = true;
			}
			finally
			{
				//Make sure to close the file even if an exception is raised...
				if ( fileStream != null )
				{
					fileStream.Close();								
				}
			}
			return ( success );
		}
	}
}
