/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.4$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 21:26:44$
	$ModDate:05/12/2007 21:25:38$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	DWConfig class definition, along with supporting classes    
    DWconfigLanguage and DWconfigDefaultVehicleProfile.

REFERENCES    

MODIFICATION HISTORY
    $Log:  51847: DWConfigFileManager.cs 

   Rev 1.4    05/12/2007 21:26:44  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.Collections;
using System.IO;


namespace DriveWizard
{
	/// <summary>
	/// Summary description for DWCOnfig.
	/// These settign are setup during installation  and
	/// can be changed at runtime by the user
	/// 
	/// This file is used by BOTH DriveWizard and the custom installer
	/// DO not use project sepecifes parameters in this file
	/// </summary>
	#region DWConfig Class
	[XmlRootAttribute( "DWConfig", Namespace="", IsNullable=false )]
	public class DWConfig
	{
		[XmlIgnore]
		public ArrayList vehicleprofiles= null;

		[XmlIgnore]
		public ArrayList failedVehicleprofiles= null;

		[XmlIgnore]
		private string [] availProfiles;
		public DWConfig()
		{
			//must use local variable here - cannot use DriveWizard.MAIN_WINDOW one 
			//since this cannot be seen by the custom installer
			string UsrDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SEVCON\Drive Wizard\";
			DWlanguage = new DWconfigLanguage(); 
			activeProfilePath = new DWconfigDefaultVehicleProfile();
			#region get the list of available profiles
			if ( Directory.Exists( UsrDirectoryPath + @"profiles\") == false )
			{
				Directory.CreateDirectory( UsrDirectoryPath + @"profiles\");
			}
			else
			{
				availProfiles = Directory.GetFiles(UsrDirectoryPath + @"profiles\","*.xml");
				vehicleprofiles = new ArrayList();
				failedVehicleprofiles = new ArrayList();
				for(int i = 0;i<availProfiles.Length;i++)
				{
					try
					{
						FileStream tempfs = new FileStream( availProfiles[i], System.IO.FileMode.Open, FileAccess.Read);
						StreamReader tempsr = new StreamReader( tempfs );
					}
					catch
					{
						failedVehicleprofiles.Add(availProfiles[i]);
						continue;
					}
					vehicleprofiles.Add(availProfiles[i]);
				}
			}
			#endregion get the list of available profiles
		}


		// Serializes an XML element
		[XmlElement("DWlanguage",typeof(DWconfigLanguage))]
		public DWconfigLanguage DWlanguage;

		// Serializes an XML element
		[XmlElement("DWDefProfile",typeof(DWconfigDefaultVehicleProfile))]
		public DWconfigDefaultVehicleProfile activeProfilePath;
	}
	#endregion DWConfig Class

	public class DWconfigLanguage
	{
		public DWconfigLanguage()
		{} 
		[XmlAttribute]
		public string lang = "English"; //will be overwirtten from XML file at runtime
	}

	public class DWconfigDefaultVehicleProfile
	{
		public DWconfigDefaultVehicleProfile()
		{}
		[XmlAttribute]
		public string profile = "";  //will be overwirtten from XML file at runtime
	}
}
