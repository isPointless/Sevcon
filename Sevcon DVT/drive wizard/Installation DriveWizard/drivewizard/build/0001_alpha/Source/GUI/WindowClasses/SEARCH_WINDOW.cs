/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.2$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:23/09/2008 23:17:42$
	$ModDate:22/09/2008 23:08:12$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	Search Window class definition.
    There should be one instance of this object created when the user selects to search the 
    MAIN_WINDOW treeView. This object is responsible for obtaining the user's search criteria 
    and for initiating each search via the delegate in MAIN_WINDOW.

REFERENCES    

MODIFICATION HISTORY
    $Log:  128352: SEARCH_WINDOW.cs 

   Rev 1.2    23/09/2008 23:17:42  ak
 UK0139.03 updates for CRR COD0013, ready for testing


   Rev 1.1    05/12/2007 22:12:42  ak
 TC keywords added to source for version control.


*******************************************************************************/
//  Description     : Search Window class. There should be one instance 
//					  of this object created when the user selects to search
//                    the MAIN_WINDOW treeView. This object is responsible for
//                    obtaining the user's search criteria and for initiating
//                    each search via the delegate in MAIN_WINDOW.
//                                                                          
//  Modification History                                                    
//    AJK,	26/11/07,	UK0139,		- Original
// 
//-------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DriveWizard
{
    /// <summary>Potential areas for the user to search</summary>
    public enum SearchArea { PARAMETER_NAME, SEVCON_OBJECT, SEVCON_SECTION, INDEX, XML_SECTION, ALL };

    /// <summary>
    /// Delegate called when the user selects to perform the next search (calls in MAIN_WINDOW).
    /// </summary>
    /// <param name="searchType">Parameters within MAIN_WINDOW.treeView1 nodes to search.</param>
    /// <param name="searchString">String to search given parameters for.</param>
    /// <param name="searchInstance">Which match instance is required i.e. if some have already been found.</param>
    /// <param name="forceUpdateFromDevice">Force displayed OD items to be read from the physical device first.</param>
    public delegate void SearchChangeListener(SearchArea searchType, string searchString, int searchInstance, bool forceUpdateFromDevice);

    /// <summary>Search Form Window Class displays a small window to all the user to enter the required
    /// search area and search string to find in the MAIN_WINDOW.treeView1 nodes.
    /// </summary>
    public partial class SearchForm : System.Windows.Forms.Form
    {
        /// <summary>Handle to delegate function in MAIN_WINDOW to be called when a search is initiated.</summary>
        private SearchChangeListener notifyMainForm = null;
        /// <summary>Contains all user entered search strings during this execution of DW.</summary>
        private static ArrayList searchString = new ArrayList();
        /// <summary>Handle to notify the MAIN_WINDOW when a search is required.</summary>
        private static int searchInstance = 0;
        /// <summary>True if OD items to be read from connected physical device before displayed on MAIN_WINDOW data grid.</summary>
        private static bool forceUpdateFromDevice = false;
        /// <summary>Form initialises searchStringcb to the string last selected by the user.</summary>
        private static string lastSearchString = "";
        /// <summary>Form initialises searchAreacb to the area last selected by the user.</summary>
        private static int lastSearchArea = (int)SearchArea.ALL;

        /// <summary>
        /// Constructor for the SearchForm (no special handling).
        /// </summary>
        public SearchForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loading the SearchForm and initialising all components and parameters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchForm_Load(object sender, EventArgs e)
        {
            // Clear searchString combo box.
            searchInstance = 0;
            searchStringcb.Items.Clear();

            #region add in previously used search strings to drop down check box
            for (int i = searchString.Count-1; i >= 0; i--)
            {
                searchStringcb.Items.Add(searchString[i]);
            }
            #endregion add in previously used search strings to drop down check box

            #region add in search area options to drop-down list box
            for (SearchArea i = SearchArea.PARAMETER_NAME; i <= SearchArea.ALL; i++)
            {
                this.searchAreacb.Items.Add(i.ToString());
            }
            #endregion add in search area options to drop-down list box

            #region initialise search form controls to the last selected by the user (if any)
            // Initialise searchString combo box to last or "" if none & select item focus.
            if (lastSearchString != "")
            {
                searchStringcb.Text = lastSearchString;
                findButton.Select();
            }
            else //first time opened
            {
                searchStringcb.Text = "";
                searchStringcb.Select();
            }

            // If last search area out of bounds, default setting is to search ALL areas.
            if ((lastSearchArea < 0) || (lastSearchArea >= searchAreacb.Items.Count))
            {
                lastSearchArea = (int)SearchArea.ALL;
            }

            // Initialise search area combo box to the last selected.
            this.searchAreacb.SelectedIndex = lastSearchArea;
            this.searchAreacb.Text = ((SearchArea)lastSearchArea).ToString();

            // Initialise the force read of shown values from physical device to the last selected, default false.
            if (forceUpdateFromDevice == true)
            {
                readValuescb.Checked = true;
            }
            #endregion initialise search form controls to the last selected by the user (if any)

            #region apply Sevcon formatting to controls
            foreach (Control myControl in this.Controls)
			{
				if((myControl.GetType().ToString()) == "System.Windows.Forms.Button")
				{
					myControl.BackColor =  SCCorpStyle.buttonBackGround;
				}
			}
            #endregion aqpply Sevcon formatting to controls
        }

        /// <summary>Disables any further searches until the current search is completed.
        /// Called by the MAIN_WINDOW if busy performing comms with physical devices.
        /// </summary>
        public void disableNextSearch()
        {
            this.findButton.Enabled = false;
        }

        /// <summary>Re-enables further searches, called by the MAIN_WINDOW.
        public void enableNextSearch()
        {
            this.findButton.Enabled = true;
        }

        /// <summary>
        /// Initiate a search with the current user criteria entered in SearchForm, by
        /// calling the MAIN_WINDOW delegate function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void findButton_Click(object sender, EventArgs e)
        {
            // If there is something to search for
            if (searchStringcb.Text != "")
            {
                #region trim any leading or trailing spaces
                searchStringcb.Text = searchStringcb.Text.TrimStart();
                searchStringcb.Text = searchStringcb.Text.TrimEnd();
                #endregion trim any leading or trailing spaces

                #region if new string, add to array list for future drop down in combo box
                if (searchString.Contains(searchStringcb.Text) == false)
                {
                    searchString.Add(searchStringcb.Text);
                    searchStringcb.Items.Insert(0, searchStringcb.Text);
                    searchInstance = 0;                         // new string, start a new search
                    lastSearchString = searchStringcb.Text;
                }
                #endregion if new string, add to array list for future drop down in combo box

                #region if search is on OD index, convert into an integer and check if valid
                if ((SearchArea)searchAreacb.SelectedIndex == SearchArea.INDEX)
                {
                    try
                    {
                        if (searchStringcb.Text.ToUpper().Contains("0X") == true)
                        {   //hex 
                            System.Convert.ToInt32(searchStringcb.Text.Trim(), 16);
                        }
                        else //dec
                        {
                            System.Convert.ToInt32(searchStringcb.Text.Trim());
                        }
                    }
                    catch
                    {
                        // Error to warn the user.
                        MessageBox.Show("Search string is not a valid number.", "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
                #endregion if search is on OD index, convert into an integer and check if valid

                #region if a delegate is setup then notify the main form to perform the actual search
                if (notifyMainForm != null)
                {
                    // Find the next search instance.
                    searchInstance++;
                    notifyMainForm((SearchArea)searchAreacb.SelectedIndex, searchStringcb.Text, searchInstance, forceUpdateFromDevice);
                }
                #endregion if a delegate is setup then notify the main form to perform the actual search
            }
        }

        /// <summary>
        /// Closes the SearchForm window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Add a listener function delegate of the passed handle.
        /// </summary>
        /// <param name="mainListener">listener delegate function handle passed from calling form (MAIN_WINDOW).</param>
        public void addSearchChangeListener(SearchChangeListener mainListener)
        {
            notifyMainForm = mainListener;
        }

        /// <summary>
        /// Called by the MAIN_WINDOW to reset any searches e.g. when no more instances
        /// are found, treeview has changed, or there is an error etc.
        /// </summary>
        public void resetSearchInstance()
        {
            searchInstance = 0;
        }

        /// <summary>
        /// Reset the search instance when a new searchArea is selected by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchAreacb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lastSearchArea != searchAreacb.SelectedIndex)
            {
                searchInstance = 0;
                lastSearchArea = searchAreacb.SelectedIndex;
            }
        } 

        /// <summary>
        /// Reset the search instance when a new searchString is selected by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchStringcb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lastSearchString != searchStringcb.Text)
            {
                searchInstance = 0;
                lastSearchString = searchStringcb.Text;
            }
        }

        /// <summary>
        /// Update whether all the displayed values on MAIN_WINDOW's selected tree node
        /// need to be read from the physical device first.
        /// NOTE: if slow communications, a search could be tedious if large numbers of
        /// values are read from the device between each search. But on the other hand, 
        /// some people will want the values to automatically be refreshed & shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void readValuescb_CheckedChanged(object sender, EventArgs e)
        {
            forceUpdateFromDevice = readValuescb.Checked;
        }

        //DR38000178 add return key invokation of search inline with common search window functionality
        private void searchStringcb_KeyUp(object sender, KeyEventArgs e)
        {
            // if it's a return key press then simulate a find button click
            if (e.KeyCode == Keys.Return)
            {
                findButton_Click(sender, e);
            }
        }
    }
}
