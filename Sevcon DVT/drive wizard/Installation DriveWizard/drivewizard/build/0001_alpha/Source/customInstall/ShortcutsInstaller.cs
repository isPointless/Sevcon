/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.8$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:34:00$
	$ModDate:05/12/2007 22:33:30$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  62503: ShortcutsInstaller.cs 

   Rev 1.8    05/12/2007 22:34:00  ak
 TC keywords added to source for version control.



*******************************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using System.Text;

namespace DriveWizard
{
	/// <summary>
	/// The installation of the shortcuts is done this way as the VS.NET Setup Project
	/// does not permit a desktop shortcut to be conditionally created and it does not
	/// provide for creating a Quick Launch shortcut.
	/// The caption for the shortcut is set to the AssemblyTitle attribute for the 
	/// current assembly. If the AssemblyTitle attribute is empty, the caption is just
	/// set to the exe name.
	/// The description for the shortcut is set to the AssemblyDescription attribute
	/// for the current assembly. If the AssemblyDescription attribute is empty, the 
	/// description is set to "Launch {caption}".
	/// </summary>
	[RunInstaller(true)]            
	public class ShortcutsInstaller : Installer
	{
		const string ASSEMBLYPATH = "ASSEMBLYPATH";
		//important info!!!
		//this follwoing need to be put in Custom Actions, Primary output etc properties, customActionData filed
		//otherwisr the ALLUSERS etc is not passed over to here
		///ALLUSERS=[ALLUSERS] /DESKTOP_SHORTCUT=[DESKTOP_SHORTCUT] /QUICKLAUNCH_SHORTCUT=[QUICKLAUNCH_SHORTCUT] /e1=[LANGUAGEBUTTONS]
		public ShortcutsInstaller()
		{
		// before install event is useless - tests show that it is handled
		//AFTER the packaged files are copied into EDs directory
			//we need an event that fires before this - so we can strip out all EDS/XML as requested
			//BEFORE the installer bungs the packged ones in - currently I cann't find such an event
//			this.BeforeInstall +=new InstallEventHandler(ShortcutsInstaller_BeforeInstall);
		}
		#region Private Instance Variables
		private string _location = null;
		private string _name = null;
		private string _description = null;
		private string AllUsrsConfigDirPath = "";
		private string thisUsrConfigDirPath = "";
		private string assemblyPath = "";
		private StringBuilder sb =new StringBuilder();
		/// <summary>
		/// if files have deen deleted when we need to run recusive detetion  to pick up now redundant higher level directories
		/// </summary>
		ArrayList filesToGoRecursive = new ArrayList();
		#endregion

		#region Private Properties
		private string QuickLaunchFolder
		{
			get
			{
				return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + 
					"\\Microsoft\\Internet Explorer\\Quick Launch";
			}
		}

		private string ShortcutTarget
		{
			get
			{
				if (_location == null)
					_location = Assembly.GetExecutingAssembly().Location;
				return _location;
			}
		}

		private string ShortcutName
		{
			get
			{
				if (_name == null)
				{
					Assembly myAssembly = Assembly.GetExecutingAssembly();

					try
					{
						object titleAttribute = myAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0];
						_name = ((AssemblyTitleAttribute)titleAttribute).Title;
					}
					catch {}

					if ((_name == null) || (_name.Trim() == string.Empty))
						_name = myAssembly.GetName().Name;
				}
				return _name;
			}
		}

		private string ShortcutDescription
		{
			get
			{
				if (_description == null)
				{
					Assembly myAssembly = Assembly.GetExecutingAssembly();

					try
					{
						object descriptionAttribute = myAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0];
						_description = ((AssemblyDescriptionAttribute)descriptionAttribute).Description;
					}
					catch {}

					if ((_description == null) || (_description.Trim() == string.Empty))
						_description = "Launch " + ShortcutName;
				}
				return _description;
			}
		}

		#endregion

		#region Override Methods
		public override void Install(IDictionary savedState)
		{
			base.Install(savedState);
			#region debug - display parames passed from dotnets Installation dialogs
			sb = new StringBuilder();
#if INSTALLATION_DEBUG
			foreach(string str in Context.Parameters.Keys)
			{
				sb.Append("\n" + str);
			}
			Message.Show(sb.ToString());
			sb = new StringBuilder();
#endif
			#endregion debug - display parames passed from dotnets Installation dialogs
			//eg if we are not installing a particular EDS this time 
			//then it won't go into program files area
			getUserDirectories();
			const string DESKTOP_SHORTCUT_PARAM = "DESKTOP_SHORTCUT";
			const string QUICKLAUNCH_SHORTCUT_PARAM = "QUICKLAUNCH_SHORTCUT";
			const string ALLUSERS_PARAM = "ALLUSERS";

			// The installer will pass the ALLUSERS, DESKTOP_SHORTCUT and QUICKLAUNCH_SHORTCUT   
			// parameters. These have been set to the values of radio buttons and checkboxes from the
			// MSI user interface.
			// ALLUSERS is set according to whether the user chooses to install for all users (="1") 
			// or just for themselves (="").
			// If the user checked the checkbox to install one of the shortcuts, then the corresponding 
			// parameter value is "1".  If the user did not check the checkbox to install one of the 
			// desktop shortcut, then the corresponding parameter value is an empty string.

			// First make sure the parameters have been provided.
			if (!Context.Parameters.ContainsKey(ASSEMBLYPATH))
				throw new Exception(string.Format("The {0} parameter has not been provided for the {1} class.", ASSEMBLYPATH, this.GetType())); 
			if (!Context.Parameters.ContainsKey(ALLUSERS_PARAM))
				throw new Exception(string.Format("The {0} parameter has not been provided for the {1} class.", ALLUSERS_PARAM, this.GetType())); 
			if (!Context.Parameters.ContainsKey(DESKTOP_SHORTCUT_PARAM))
				throw new Exception(string.Format("The {0} parameter has not been provided for the {1} class.", DESKTOP_SHORTCUT_PARAM, this.GetType())); 
			if (!Context.Parameters.ContainsKey(QUICKLAUNCH_SHORTCUT_PARAM))
				throw new Exception(string.Format("The {0} parameter has not been provided for the {1} class.", QUICKLAUNCH_SHORTCUT_PARAM, this.GetType())); 

			bool allusers = Context.Parameters[ALLUSERS_PARAM] != string.Empty;
			bool installDesktopShortcut = Context.Parameters[DESKTOP_SHORTCUT_PARAM] != string.Empty;
			bool installQuickLaunchShortcut = Context.Parameters[QUICKLAUNCH_SHORTCUT_PARAM] != string.Empty;
			this.assemblyPath = Context.Parameters[ASSEMBLYPATH].Substring(0,Context.Parameters[ASSEMBLYPATH].LastIndexOf(@"\"));

			string desktopFolder = null;
			string configFilePath = null;
			string programsFolder = null;
			#region get the All users paths regrdless of Installer first
			try
			{
				// This is in a Try block in case AllUsersDesktop is not supported
				object allUsersDesktop = "AllUsersDesktop";
				object allusersprograms = "AllUsersPrograms";
				WshShell shell = new WshShellClass();
				desktopFolder = shell.SpecialFolders.Item(ref allUsersDesktop).ToString();
				programsFolder = shell.SpecialFolders.Item(ref allusersprograms).ToString(); 
				// If this is an All Users install then we need to install the desktop shortcut for 
				// all users.  .Net does not give us access to the All Users Desktop special folder,
				// but we can get this using the Windows Scripting Host.
				if (desktopFolder == null)
				{
					desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
				}
				else
				{
					configFilePath = desktopFolder;
					int desktopInd = configFilePath.IndexOf(@"\Desktop");
					if(desktopInd != -1)
					{
						configFilePath = configFilePath.Substring(0,desktopInd);
						configFilePath = configFilePath + @"\Application Data\SEVCON\Drive Wizard\DWConfig.xml";
					}
				}
			}
			catch 
			{
				allusers = false;
			}
			#endregion get the All users paths regrdless of Installer first

			if(allusers == false) //iether Installer chose 'just me' or we failed ot find the all users locations
			{
				#region find the current user's config file and desktop locations
				//ie get location of the current users program list and desktop
				programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
				desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
				try
				{  //we are talkign 'just me' here either by choice or defualt so delete the all users config file
					//Then when DW starts it knows to use the local language setting
					System.IO.File.Delete(configFilePath);
				}
				catch{};
				configFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SEVCON\Drive Wizard\DWConfig.xml";
				#endregion find the current user's config file and desktop locations
			}

			//add the programs shortcut regradless of installer options
			CreateShortcut(programsFolder,"DriveWizard",ShortcutTarget,ShortcutDescription);
			if (installDesktopShortcut)
			{
				#region add the desktop shortcut
				try
				{
					CreateShortcut(desktopFolder, "DriveWizard", ShortcutTarget, ShortcutDescription);
				}
				catch
				{
					DriveWizard.Message.Show("Unable to create desktop shotcut");
				}
				#endregion add the desktop shortcut
			}
			if (installQuickLaunchShortcut)
			{
				#region add Start menu shortcut
				try
				{
					CreateShortcut(QuickLaunchFolder, ShortcutName, ShortcutTarget, ShortcutDescription);
				}
				catch
				{
					DriveWizard.Message.Show("Unable to add DriveWizrd to QuickLaunch toolbar");
				}
				#endregion add Start menu shortcut
			}
			#region add Installer selections to either the All Users config file or the current Users config file
			//first create a DWConfig object and attmept ot serialize in from the existing DWConfig file
			#region create Configruation object from file if possible
			DriveWizard.DWConfig DWConfigFile = new DWConfig();
			ObjectXMLSerializer DWconfigXMLSerializer = new ObjectXMLSerializer();
			//we have to load the existing DWConfig file to retain 
			//its current settings
			try
			{
				if(System.IO.File.Exists(configFilePath)== false)
				{
					string dirPath = configFilePath.Replace(@"\DWConfig.xml", "");
					if(Directory.Exists(dirPath) == false)
					{
						Directory.CreateDirectory(dirPath);
					}
				}
				else
				{
					DWConfigFile = (DriveWizard.DWConfig) DWconfigXMLSerializer.Load(DWConfigFile,configFilePath);
				}
			}
			catch(Exception e)
			{
				DriveWizard.Message.Show("Unable to create/open configuration file: " + configFilePath + "\nError msg: " + e.Message);  
			}
			#endregion create Configruation object from file if possible
			#region update the language selection
			if(Context.Parameters["e1"] == "1")
			{
				DWConfigFile.DWlanguage.lang = "English";
			}
			else if(Context.Parameters["e1"] == "2")
			{
				DWConfigFile.DWlanguage.lang = "French";
			}
			else if(Context.Parameters["e1"] == "3")
			{
				DWConfigFile.DWlanguage.lang = "Japanese";
			}
			else if(Context.Parameters["e1"] == "4")
			{
				DWConfigFile.DWlanguage.lang = "Korean";
			}
			#endregion update the language selection
			#region save the updated config file
			try
			{
				DWconfigXMLSerializer.Save(DWConfigFile,configFilePath);
			}
			catch(Exception ex)
			{
				DriveWizard.Message.Show("cannot save the file \n" + configFilePath + "\n" + ex.Message);
			}
			#endregion save the updated config file
			#endregion add Installer selections to either the All Users config file or the current Users config file

			#region my custom instaer dialogs
			requestDeletionOFAdditionalFiles();
			#endregion my custom instaer dialogs
		}
		public override void Uninstall(IDictionary savedState)
		{  
			base.Uninstall(savedState);
			if (!Context.Parameters.ContainsKey(ASSEMBLYPATH))
				throw new Exception(string.Format("The {0} parameter has not been provided for the {1} class.", ASSEMBLYPATH, this.GetType())); 
			this.assemblyPath = Context.Parameters[ASSEMBLYPATH].Substring(0,Context.Parameters[ASSEMBLYPATH].LastIndexOf(@"\"));
			DeleteShortcuts();
			uninstallFileOptions();
		}

		public override void Rollback(IDictionary savedState)
		{  
			base.Rollback(savedState);

			DeleteShortcuts();
		}

		#endregion

		#region Private Helper Methods
		private void CreateShortcut(string folder, string name, string target, string description)
		{
			string shortcutFullName = Path.Combine(folder, name + ".lnk");

			try
			{
				WshShell shell = new WshShellClass();
				IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shortcutFullName);
				link.TargetPath = target;
				link.Description = description;
				link.IconLocation = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\SEVCON\DriveWizard\resources\icons\DriveWizard.ico";
				link.Save();
			}
			catch //(Exception ex)
			{
				//judetmep this exception occurs if the Windows OS does not suppot the creation of the shortcut
				//it is confusing for users - so we'll just not display it - DriveWizard will work just fine - see EMail from Jonathon Sperandio 12/04/06
//				MessageBox.Show(string.Format("The shortcut \"{0}\" could not be created.\n\n{1}", shortcutFullName, ex.ToString()),
//					"Create Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
		private void requestDeletionOFAdditionalFiles()
		{
			string [] temp = Enum.GetNames(typeof (DriveWizard.UninstFileTypes));
			bool [] fileTypesToGo = new bool[temp.Length];
			System.Windows.Forms.Form uninstall = new DriveWizard.Install_File_Options(ref fileTypesToGo);
			uninstall.ShowDialog();
			sb = new StringBuilder();

			if(fileTypesToGo[(int)UninstFileTypes.ALL] == true)
			{
				this.removeAllFiles();
			}
			else
			{
				#region remove selected file tpyes
				if(fileTypesToGo[(int)UninstFileTypes.EDS] == true)
				{
					removeFilesInDirectory( thisUsrConfigDirPath + @"\Drive Wizard\EDS", "EDS", sb);
					//judetemp in theory we should be able to strip them all out - and installer will put them back in 
					//sinc efiles ar emarked permanent - but in practive the filesare never restored;
					//
//					removeFilesInDirectory( this.assemblyPath + @"\EDS", "EDS", ref sb);
				}
				if(fileTypesToGo[(int)UninstFileTypes.XML] == true)
				{
					removeFilesInDirectory( thisUsrConfigDirPath + @"\Drive Wizard\EDS", "XML", sb);

					//removeFilesInDirectory( this.assemblyPath + @"\EDS", "XML", ref sb);
				}
				if(fileTypesToGo[(int) UninstFileTypes.DCF] == true)
				{
					removeFilesInDirectory( thisUsrConfigDirPath + @"\Drive Wizard\DCF", "DCF", sb);
					removeFilesInDirectory( this.assemblyPath + @"\DCF", "DCF", sb);
				}
				if(fileTypesToGo[(int)UninstFileTypes.VehProfiles] == true)
				{
					removeFilesInDirectory( thisUsrConfigDirPath + @"\Drive Wizard\profiles", "XML", sb);
				}
				if(fileTypesToGo[(int)UninstFileTypes.DLD] == true)
				{
					removeFilesInDirectory( thisUsrConfigDirPath + @"\Drive Wizard\DLD", "DLD", sb);
					removeFilesInDirectory( this.assemblyPath + @"\DLD", "DLD", sb);
				}
				if(fileTypesToGo[(int)UninstFileTypes.Monitor] == true)
				{
					removeFilesInDirectory( thisUsrConfigDirPath + @"\Drive Wizard\MONITOR", "XML", sb);
					removeFilesInDirectory( this.assemblyPath + @"\MONITOR", "TXT", sb);
					removeFilesInDirectory( this.assemblyPath + @"\MONITOR", "XML", sb);
				}
				if(fileTypesToGo[(int)UninstFileTypes.EventIds] == true)
				{
					removeFilesInDirectory( thisUsrConfigDirPath + @"\Drive Wizard\IDS", "TXT", sb);
					removeFilesInDirectory( this.assemblyPath + @"\IDS", "TXT", sb);
				}
				#endregion remove selected file tpyes
			}
			if(sb.Length>0)
			{
				Message.Show(sb.ToString());
			}
		}

		private void getUserDirectories()
		{
			string desktopFolder = null;
			object allUsersDesktop = "AllUsersDesktop";
			WshShell shell = new WshShellClass();
			desktopFolder = shell.SpecialFolders.Item(ref allUsersDesktop).ToString();
			if (desktopFolder != null)
			{
				AllUsrsConfigDirPath = desktopFolder;
				int desktopInd = AllUsrsConfigDirPath.IndexOf(@"\Desktop");
				if(desktopInd != -1)
				{
					this.AllUsrsConfigDirPath = AllUsrsConfigDirPath.Substring(0,desktopInd);
					this.AllUsrsConfigDirPath = AllUsrsConfigDirPath + @"\Application Data" + @"\SEVCON";
				}
			}
			this.thisUsrConfigDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+ @"\SEVCON";
		}
		private void DeleteShortcuts()
		{
			// Just try and delete all possible shortcuts that may have been
			// created during install

			try
			{
				// This is in a Try block in case AllUsersDesktop is not supported
				object allUsersDesktop = "AllUsersDesktop";
				WshShell shell = new WshShellClass();
				string desktopFolder = shell.SpecialFolders.Item(ref allUsersDesktop).ToString();
				DeleteShortcut(desktopFolder, ShortcutName);

				object allusersprograms = "AllUsersPrograms";
				string programsFolder = shell.SpecialFolders.Item(ref allusersprograms).ToString(); 
				DeleteShortcut(programsFolder, ShortcutName);
			}
			catch {}

			DeleteShortcut(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), ShortcutName);
			DeleteShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Programs), ShortcutName);
			DeleteShortcut(QuickLaunchFolder, ShortcutName);
		}

		private void DeleteShortcut(string folder, string name)
		{
			string shortcutFullName = Path.Combine(folder, name + ".lnk");
			FileInfo shortcut = new FileInfo(shortcutFullName);
			if (shortcut.Exists)
			{
				try
				{
					shortcut.Delete();
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format("The shortcut \"{0}\" could not be deleted.\n\n{1}", shortcutFullName, ex.ToString()),
						"Delete Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		private void uninstallFileOptions()
		{
			sb = new StringBuilder();
			bool [] removeAll =new bool[1];
			removeAll[0] = false;
			System.Windows.Forms.Form uninstall = new DriveWizard.UnInstallFileOptions(ref removeAll);
			uninstall.ShowDialog();
			if(removeAll[0] == true)
			{
				getUserDirectories();
				try //try needed
				{
					this.uninstallAllDriveWIzard();
				}
				catch
				{} //we will get an exception when we detelete the IwshRuntimeLibrary.dll
				//but we don't care so igneor eht eexceoitn 
				DirectoryInfo assPathInfo = new DirectoryInfo(this.assemblyPath);
				assPathInfo.Refresh();
				FileSystemInfo [] temp = assPathInfo.GetFileSystemInfos();
				DirectoryInfo usrPathInfo = new DirectoryInfo(this.thisUsrConfigDirPath + @"\DriveWizard");
				usrPathInfo.Refresh();
				DirectoryInfo allUsrPathInfo = new DirectoryInfo(this.AllUsrsConfigDirPath + @"\DriveWizard");
				allUsrPathInfo.Refresh();
				if((temp.Length<= 2)  //only the exe and the IWsh....dll remain - as expected - removed at end of uninstall
					&& (usrPathInfo.Exists == false)
					&& (allUsrPathInfo.Exists == false)) 
				{//MessageBox => not Message here I think - in theory DW should be all gone from this place ( the exe is binned and I think we are running form memory here)
					MessageBox.Show("DriveWizard has been sucessfully removed from your system");
				}
				else
				{
					MessageBox.Show("Not all files were removed. You should remove these manually");
				}
			}
		}
		private void uninstallAllDriveWIzard()
		{ //we should do this during unistall only!! - in stall it wipes the files we just legitimately installed
			//Windows installaiton order is dodgy - this occurs even if base.Install is called last
			
			if(Directory.Exists(this.assemblyPath))
			{
				#region application exe directory and sub disrectories
				this.filesToGoRecursive = new ArrayList();
				recursivelyRemoveFiles(this.assemblyPath);
				this.filesToGoRecursive.Reverse(); //ensure lowest levle if first - we cannot delte a non-empty directory
				foreach(System.IO.FileSystemInfo fileSysInfo in this.filesToGoRecursive)
				{
					try
					{
						fileSysInfo.Attributes = FileAttributes.Normal;
						fileSysInfo.Refresh();
						fileSysInfo.Delete();
					}
					catch{} //do nothing - so fiels still 'remain' but are mopped up autmoatically at end by WIndows installer
				}
				#endregion application exe directory and sub disrectories
			}
			if(Directory.Exists(this.thisUsrConfigDirPath))
			{
				#region current users Sevcon application data area
				this.filesToGoRecursive = new ArrayList();
				recursivelyRemoveFiles(this.thisUsrConfigDirPath);
				this.filesToGoRecursive.Reverse(); //ensure lowest levle if first - we cannot delte a non-empty directory
				foreach(System.IO.FileSystemInfo fileSysInfo in this.filesToGoRecursive)
				{
					try
					{
						fileSysInfo.Attributes = FileAttributes.Normal;
						fileSysInfo.Refresh();
						fileSysInfo.Delete();
					}
					catch{} //do nothing - so fiels still 'remain' but are mopped up autmoatically at end by WIndows installer
				}
				#endregion current users Sevcon application data area
			}
			if(Directory.Exists(this.AllUsrsConfigDirPath))
			{
				#region All users Sevcon application data area
				this.filesToGoRecursive = new ArrayList();
				recursivelyRemoveFiles(this.AllUsrsConfigDirPath);
				this.filesToGoRecursive.Reverse(); //ensure lowest levle if first - we cannot delte a non-empty directory
				foreach(System.IO.FileSystemInfo fileSysInfo in this.filesToGoRecursive)
				{
					try
					{
						fileSysInfo.Attributes = FileAttributes.Normal;
						fileSysInfo.Refresh();
						fileSysInfo.Delete();
					}
					catch{} //do nothing - so fiels still 'remain' but are mopped up autmoatically at end by WIndows installer
				}
				#endregion All users Sevcon application data area
			}
		}
		private void removeAllFiles()
		{
			#region current user user's directory
			if(Directory.Exists(thisUsrConfigDirPath))
			{
				this.filesToGoRecursive = new ArrayList();
				recursivelyRemoveFiles(thisUsrConfigDirPath);
				this.filesToGoRecursive.Reverse(); //ensure lowest levle if first - we cannot delte a non-empty directory
				foreach(System.IO.FileSystemInfo fileSysInfo in this.filesToGoRecursive)
				{
					try
					{
						fileSysInfo.Refresh();
						fileSysInfo.Delete();
					}
					catch(Exception e)
					{
						sb.Append("\nUnable to delete: " + fileSysInfo.FullName + ", " + e.Message);
					}
				}
			}
			#endregion current user user's directory
			#region All users directory
			this.filesToGoRecursive = new ArrayList();
			if(Directory.Exists(AllUsrsConfigDirPath))
			{
				recursivelyRemoveFiles(AllUsrsConfigDirPath);
				this.filesToGoRecursive.Reverse(); //ensure lowest levle if first - we cannot delte a non-empty directory
				foreach(System.IO.FileSystemInfo fileSysInfo in this.filesToGoRecursive)
				{
					try
					{
						fileSysInfo.Refresh();
						fileSysInfo.Delete();
					}
					catch(Exception e)
					{
						sb.Append("\nUnable to delete file: " + fileSysInfo.FullName + ", " + e.Message);
					}
				}
			}
			#endregion All users directory
			//Followign code for ver 1.12 onnly
			//Once users have 1.12 then any new user files go in 
			//the user's directory - any say EDS files that we insallted last time - but don't want this time
			//will be automatically remvoed by installer since they ar emarked as having 
			//been installed by installer. Normal users will not be able to add any further files
			#region ver 1.12 only
			removeFilesInDirectory( this.assemblyPath + @"\EDS", "XML", sb);
			removeFilesInDirectory( this.assemblyPath + @"\DLD", "DLD", sb);
			removeFilesInDirectory( this.assemblyPath + @"\DCF", "DCF", sb);
			removeFilesInDirectory( this.assemblyPath + @"\MONITOR", "XML", sb);
			//ver 1.12 only - txt files for monitiring
			removeFilesInDirectory( this.assemblyPath + @"\MONITOR", "TXT", sb);
			removeFilesInDirectory( this.assemblyPath + @"\IDS", "TXT", sb);
			#endregion ver 1.12 only
		}

		private void removeFilesInDirectory(string directoryPath, string fileExt, StringBuilder sb)
		{
			sb = new StringBuilder();
			string [] files = new string[0];
			ArrayList filesToGoInThisDir = new ArrayList();
			if(Directory.Exists(directoryPath))
			{
				System.IO.DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
				System.IO.FileSystemInfo [] fileInfos = dirInfo.GetFileSystemInfos();
				foreach(System.IO.FileSystemInfo fileInfo in fileInfos)
				{
					if(fileInfo.Extension.ToUpper() == "." + fileExt)  
					{
						fileInfo.Attributes = FileAttributes.Normal;
						filesToGoInThisDir.Add(fileInfo);
					}
				}
				filesToGoInThisDir.Reverse();
				foreach(System.IO.FileSystemInfo fileSysInfo in filesToGoInThisDir)
				{
					try
					{
						fileSysInfo.Refresh();
						fileSysInfo.Delete();
#if INSTALLATION_DEBUG
						sb.Append("\n file deleted");
						sb.Append(fileSysInfo.FullName);
#endif
					}
					catch(Exception e)
					{
						sb.Append("\nUnable to delete file: " + fileSysInfo.FullName + ", " + e.Message);
					}
				}
#if INSTALLATION_DEBUG
												Message.Show(sb.ToString());
#endif
			}
		}

		private void recursivelyRemoveFiles(string directoryPath)
		{
			System.IO.DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
			System.IO.FileSystemInfo [] fileInfos = dirInfo.GetFileSystemInfos();
			foreach(System.IO.FileSystemInfo fileInfo in fileInfos)
			{//note GetFiles() does not get the sub directories - to check for empty we need to use GetFileSystemInfos()
				fileInfo.Attributes = FileAttributes.Normal;
				this.filesToGoRecursive.Add(fileInfo);
				if((fileInfo.Attributes & FileAttributes.Directory) >0)
				{
					#region handle sub -directories
					if(Directory.Exists(directoryPath + @"\" + fileInfo.Name))
					{
						recursivelyRemoveFiles(directoryPath + @"\" + fileInfo.Name);
					}
					#endregion handle sub -directories
				}
			}
		}
		#endregion
	}
}
