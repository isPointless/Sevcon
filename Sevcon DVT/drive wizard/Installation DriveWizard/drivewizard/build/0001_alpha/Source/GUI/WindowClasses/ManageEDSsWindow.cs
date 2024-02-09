/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.2$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:22/04/2008 22:40:26$
	$ModDate:22/04/2008 22:35:56$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	Manage EDSs window class. There should be one instance 
    of this object created when the user connects to a system
    and multiple EDS files are found to match the device.

REFERENCES    

MODIFICATION HISTORY
    $Log:  128953: ManageEDSsWindow.cs 

   Rev 1.2    22/04/2008 22:40:26  ak
 Deleting of multiple files fixed. Cannot re-size window.


   Rev 1.1    05/12/2007 22:12:42  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace DriveWizard
{
    /// <summary>
    /// Displays a small window showing all the matching EDS filenames in a list box
    /// then allows the user to delete selected files, select a file to use as a match
    /// or ignore the problem and use the default first matching file.
    /// </summary>
    public partial class ManageEDSsWindow : Form
    {
        /// <summary>Vendor ID of device with multiple EDS file matches.</summary>
        private uint problemVendorID;
        
        /// <summary>Product code of device with multiple EDS file matches.</summary>
        private uint problemProductCode;

        /// <summary>Revision number of device with multiple EDS file matches.</summary>
        private uint problemRevisionNumber;

        /// <summary>After a count is performed, this contains the number of files the user 
        /// has checked in the EDS file check list box.</summary>
        private int noOfFilesSelected = 0;
        /// <summary>After a count is performed, this contains the number of files the user 
        /// has left unchecked in the EDS file check list box.</summary>
        private int noOfFilesUnselected = 0;

        /// <summary>Array list containing all the filenames of the problem device's 
        /// matching EDS files (copied from MAIN_WINDOW.availableEDSInfo).</summary>
        private ArrayList EDSfiles = new ArrayList();

        /// <summary>
        /// Manage EDS Window constructor. Saves a copy of the device information
        /// outlining the problem EDSs.
        /// </summary>
        /// <param name="vendorID">Vendor ID of device with multiple EDS file matches.</param>
        /// <param name="productCode">Product code of device with multiple EDS file matches</param>
        /// <param name="revisionNumber">Revision number of device with multiple EDS file matches</param>
        public ManageEDSsWindow(uint vendorID, uint productCode, uint revisionNumber)
        {
            InitializeComponent();

            // Save device info details of problematic device.
            problemVendorID = vendorID;
            problemProductCode = productCode;
            problemRevisionNumber = revisionNumber;
        }

        /// <summary>
        /// Loads and initialises the Manage EDS window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManageEDSsWindow_Load(object sender, EventArgs e)
        {
            #region apply Sevcon formatting to controls
            foreach (Control myControl in this.Controls)
            {
                if ((myControl.GetType().ToString()) == "System.Windows.Forms.Button")
                {
                    myControl.BackColor = SCCorpStyle.buttonBackGround;
                }
            }
            #endregion aqpply Sevcon formatting to controls

            // Update the window with the device information this window relates to.
            vendorNumbertb.Text = "0x" + problemVendorID.ToString("X").PadLeft(8, '0');
            productNumbertb.Text = "0x" + problemProductCode.ToString("X").PadLeft(8, '0');
            revisionNumbertb.Text = "0x" + problemRevisionNumber.ToString("X").PadLeft(8, '0');

            statusBarPanel1.Text = "Check files in the listbox before clicking on the required button to action.";

            // Fill the list box with all EDS filenames that match this device.
            fillEDSFileListBox();
        }

        /// <summary>
        /// Checkes the user selection and, if OK, performs the system deletion of
        /// the selected files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteButton_Click(object sender, EventArgs e)
        {
            countFileSelections();

            if (noOfFilesUnselected < 1)
            {
                statusBarPanel1.Text = "No files were deleted from the system. At least one EDS file must remain.";
            }
            else if (noOfFilesSelected == 0)
            {
                statusBarPanel1.Text = "No files have been selected for deletion.";
            }
            // Only try to delete if there are files selected for deletion & at least one file left
            // unchecked to use for the matching device.
            else
            {
                string currentFile;
                try
                {
                    for(int i= 0;i<this.EDSFileclb.Items.Count;i++)
			        {
                        if (EDSFileclb.GetItemCheckState(i) == CheckState.Checked)
                        {
                            currentFile = EDSfiles[i].ToString();
                            File.Delete(currentFile);

                            // Remove deleted file from MAIN_WINDOW.availableEDSInfo.
                            for (int j = 0; j < MAIN_WINDOW.availableEDSInfo.Count; j++)
                            {
                                AvailableNodesWithEDS devInfo = (AvailableNodesWithEDS)MAIN_WINDOW.availableEDSInfo[j];

                                if (devInfo.EDSFilePath == currentFile)
                                {
                                    MAIN_WINDOW.availableEDSInfo.RemoveAt(j);
                                    break; // only one file to match
                                }
                            }
                        }
                    }

                    // Now remove corresponding files from this form's EDSfile list.
                    // Don't do in above loop as this changes EDSfiles and indices then become incorrect.
                    for (int i = 0; i < this.EDSFileclb.Items.Count; i++)
                    {
                        if (EDSFileclb.GetItemCheckState(i) == CheckState.Checked)
                        {
                            EDSfiles.RemoveAt(i);
                        }
                    }

                    statusBarPanel1.Text = "Selected files successfully deleted from the system.";
                }
                catch (Exception mye)
                {
                    statusBarPanel1.Text = "Could not delete file. Exception: " + mye.Message.ToString();
                }
            }

            // Re-populate the list file box to reflect the user's changes.
            fillEDSFileListBox();

            // If there's only one file left, quit the window since the user's fixed the problem.
            if (EDSFileclb.Items.Count == 1)
            {
                Close();
            }
        }

        /// <summary>
        /// Checkes the user selection and, if OK, updates the MAIN_WINDOW.availableEDSInfo
        /// so that only the selected file is in there to represent this device.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectButton_Click(object sender, EventArgs e)
        {
            countFileSelections();

            if (noOfFilesSelected == 0)
            {
                statusBarPanel1.Text = "No file was selected for use as a match for this device.";
            }
            else if (noOfFilesSelected > 1)
            {
                statusBarPanel1.Text = "Only one file must be selected for use as a match for this device.";

                // uncheck all items ready for the user to re-select
                for (int i = 0; i < EDSFileclb.Items.Count; i++)
                {
                    EDSFileclb.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            // Else one file has been selected for use.
            else
            {
                statusBarPanel1.Text = "The selected file will be used as a match for this device.";
                updateAvailableEDSInfo();
                Close();
            }
        }

        /// <summary>
        /// Updates the MAIN_WINDOW.availableEDSInfo so that only the first matching EDS
        /// file for this device remains i.e. pretend other matching files don't exist 
        /// after this point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            //Ignore button on screen
            statusBarPanel1.Text = "Use first matching file as a match for this device.";
            EDSFileclb.SetItemCheckState(0, CheckState.Checked);
            updateAvailableEDSInfo();
            Close();
        }

        /// <summary>
        /// Finds and deletes all superflous EDS file matches for this device in
        /// MAIN_WINDOW.availableEDSInfo.
        /// </summary>
        private void updateAvailableEDSInfo()
        {
            string selectedFile = "";
            int fileSelectedIndex = -1;

            #region Find the index of file we want to use in availableEDSInfo
            for (int i = 0; i < this.EDSFileclb.Items.Count; i++)
            {
                if (EDSFileclb.GetItemCheckState(i) == CheckState.Checked)
                {
                    selectedFile = EDSfiles[i].ToString();

                    for (int j = 0; j < MAIN_WINDOW.availableEDSInfo.Count; j++)
                    {
                        AvailableNodesWithEDS devInfo = (AvailableNodesWithEDS)MAIN_WINDOW.availableEDSInfo[j];

                        if (devInfo.EDSFilePath == selectedFile)
                        {
                            fileSelectedIndex = j;
                            break;
                        }
                    }

                    break;  // only one file is checked
                }
            }
            #endregion Find the index of file we want to use in availableEDSInfo

            #region Delete all other matching files from MAIN_WINDOW.availableEDSInfo
            // i.e. during this run of DW, pretend that the other files don't exist.
            if ((fileSelectedIndex >= 0) && (selectedFile != ""))
            {
                for (int j = 0; j < MAIN_WINDOW.availableEDSInfo.Count; j++)
                {
                    AvailableNodesWithEDS devInfo = (AvailableNodesWithEDS)MAIN_WINDOW.availableEDSInfo[j];

                    // If this EDS file's device information matches ours
                    if
                    (
                        (problemVendorID == devInfo.vendorNumber)
                     && (problemProductCode == devInfo.productNumber)
                     && (problemRevisionNumber == devInfo.revisionNumber)
                    )
                    {
                        if
                        (
                            (problemVendorID == SCCorpStyle.SevconID)
                         && ((problemProductCode == 0xFFFFFFFF) || (problemRevisionNumber == 0xFFFFFFFF))
                        )
                        {
                            //do not match the EDS - Sevcon devcies should match all 3 params
                        }
                        else // match found
                        {
                            //If it's not the file we wish to use, remove from availableEDSInfo.
                            if (devInfo.EDSFilePath != selectedFile)
                            {
                                MAIN_WINDOW.availableEDSInfo.RemoveAt(j);
                                j = 0;  // start again since the list has changed
                            }
                        }
                    }
                }
            }
            else
            {
                // This should never happen.
                statusBarPanel1.Text = "Couldn't use selected file as a match for this device.";
            }
            #endregion Delete all other matching files from MAIN_WINDOW.availableEDSInfo
        }

        /// <summary>
        /// Counts the number of EDS files which have been checked and left unchecked
        /// in the EDS file list box.
        /// </summary>
        private void countFileSelections()
        {
            noOfFilesSelected = 0;
            noOfFilesUnselected = 0;

            for (int i = 0; i < this.EDSFileclb.Items.Count; i++)
            {
                if (EDSFileclb.GetItemCheckState(i) == CheckState.Checked)
                {
                    noOfFilesSelected++;
                }
                else // Not checked or don't know assumed to be unselected.
                {
                    noOfFilesUnselected++;
                }
            }
        }

        /// <summary>
        /// Clears and updates the EDS file list box with the names of all EDS files in
        /// MAIN_WINDOW.availableEDSInfo that match our device information.
        /// </summary>
        private void fillEDSFileListBox()
        {
            // Clear out any previous data.
            EDSfiles.Clear();   
            EDSFileclb.Items.Clear();

            #region Find all matching EDS files in availableEDSInfo and add to the list box.
            foreach (object obj in MAIN_WINDOW.availableEDSInfo)
            {
                AvailableNodesWithEDS devInfo = (AvailableNodesWithEDS)obj;

                if
                (
                    (problemVendorID == devInfo.vendorNumber)
                 && (problemProductCode == devInfo.productNumber)
                 && (problemRevisionNumber == devInfo.revisionNumber)
                )
                {
                    if 
                    (
                        (problemVendorID == SCCorpStyle.SevconID)
                     && ((problemProductCode == 0xFFFFFFFF) || (problemRevisionNumber == 0xFFFFFFFF))
                    )
                    {
                        //do not match the EDS - Sevocn devcies should match all 3 params
                    }
                    else
                    {
                        EDSfiles.Add(devInfo.EDSFilePath);
                        EDSFileclb.Items.Add(devInfo.EDSFilePath, CheckState.Unchecked);
                    }
                }
            }
            #endregion Find all matching EDS files in availableEDSInfo and add to the list box.

            // If there's no matching EDS files, what are we doing here?
            if (EDSFileclb.Items.Count == 0)
            {
                Close();
            }
        }

        private void ManageEDSsWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Make close equivalent of an ignore button press on screen
            statusBarPanel1.Text = "Use first matching file as a match for this device.";
            EDSFileclb.SetItemCheckState(0, CheckState.Checked);
            updateAvailableEDSInfo();
        }
    }
}
