/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.7$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:34:00$
	$ModDate:05/12/2007 22:33:36$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  66460: InstallFileOptions.cs 

   Rev 1.7    05/12/2007 22:34:00  ak
 TC keywords added to source for version control.



*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DriveWizard
{
	public enum UninstFileTypes {  ALL, EDS, XML,DCF, Monitor, VehProfiles, DLD, EventIds};
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Install_File_Options : System.Windows.Forms.Form
	{
		#region Controls declarations
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.CheckBox ChkDlds;
		private System.Windows.Forms.CheckBox ChkVehProfs;
		private System.Windows.Forms.CheckBox ChkDCF;
		private System.Windows.Forms.CheckBox ChkEDS;
		private System.Windows.Forms.CheckBox ChkEvents;
		private System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.CheckBox ChkAllFiles;
		private System.Windows.Forms.CheckBox chkMonitor;
		private System.Windows.Forms.CheckBox chkXML;
		#endregion Controls declarations

		#region my parameters
		private bool [] localChks;
		#endregion my parameters

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ChkEvents = new System.Windows.Forms.CheckBox();
			this.chkXML = new System.Windows.Forms.CheckBox();
			this.chkMonitor = new System.Windows.Forms.CheckBox();
			this.ChkAllFiles = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.ChkDlds = new System.Windows.Forms.CheckBox();
			this.ChkVehProfs = new System.Windows.Forms.CheckBox();
			this.ChkDCF = new System.Windows.Forms.CheckBox();
			this.ChkEDS = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.ChkEvents);
			this.groupBox1.Controls.Add(this.chkXML);
			this.groupBox1.Controls.Add(this.chkMonitor);
			this.groupBox1.Controls.Add(this.ChkAllFiles);
			this.groupBox1.Controls.Add(this.button1);
			this.groupBox1.Controls.Add(this.ChkDlds);
			this.groupBox1.Controls.Add(this.ChkVehProfs);
			this.groupBox1.Controls.Add(this.ChkDCF);
			this.groupBox1.Controls.Add(this.ChkEDS);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.groupBox1.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(7, 14);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(446, 306);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Select file types to remove prior to installation ";
			// 
			// label1
			// 
			this.label1.ForeColor = System.Drawing.Color.Brown;
			this.label1.Location = new System.Drawing.Point(16, 176);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(400, 80);
			this.label1.TabIndex = 10;
			this.label1.Text = "NOTE: Verison 1.12 has improved file formats and so affected files on yoursystem " +
				"are incompatible and will be automatically removed. \nThis is a one off change fo" +
				"r this DriveWizard release only";
			this.label1.Click += new System.EventHandler(this.label1_Click);
			// 
			// ChkEvents
			// 
			this.ChkEvents.Location = new System.Drawing.Point(216, 40);
			this.ChkEvents.Name = "ChkEvents";
			this.ChkEvents.Size = new System.Drawing.Size(187, 28);
			this.ChkEvents.TabIndex = 9;
			this.ChkEvents.Text = "Event Keys file";
			// 
			// chkXML
			// 
			this.chkXML.ForeColor = System.Drawing.SystemColors.ControlText;
			this.chkXML.Location = new System.Drawing.Point(8, 64);
			this.chkXML.Name = "chkXML";
			this.chkXML.Size = new System.Drawing.Size(187, 27);
			this.chkXML.TabIndex = 8;
			this.chkXML.Text = "XML files";
			// 
			// chkMonitor
			// 
			this.chkMonitor.Enabled = false;
			this.chkMonitor.ForeColor = System.Drawing.Color.DimGray;
			this.chkMonitor.Location = new System.Drawing.Point(216, 72);
			this.chkMonitor.Name = "chkMonitor";
			this.chkMonitor.Size = new System.Drawing.Size(187, 28);
			this.chkMonitor.TabIndex = 7;
			this.chkMonitor.Text = "Monitoring files";
			// 
			// ChkAllFiles
			// 
			this.ChkAllFiles.Enabled = false;
			this.ChkAllFiles.ForeColor = System.Drawing.Color.DimGray;
			this.ChkAllFiles.Location = new System.Drawing.Point(8, 264);
			this.ChkAllFiles.Name = "ChkAllFiles";
			this.ChkAllFiles.Size = new System.Drawing.Size(274, 21);
			this.ChkAllFiles.TabIndex = 6;
			this.ChkAllFiles.Text = "Remove All User files";
			this.ChkAllFiles.CheckedChanged += new System.EventHandler(this.ChkAllFiles_CheckedChanged);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(336, 264);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(100, 28);
			this.button1.TabIndex = 5;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// ChkDlds
			// 
			this.ChkDlds.Location = new System.Drawing.Point(216, 104);
			this.ChkDlds.Name = "ChkDlds";
			this.ChkDlds.Size = new System.Drawing.Size(187, 28);
			this.ChkDlds.TabIndex = 4;
			this.ChkDlds.Text = "Download files";
			// 
			// ChkVehProfs
			// 
			this.ChkVehProfs.Location = new System.Drawing.Point(8, 128);
			this.ChkVehProfs.Name = "ChkVehProfs";
			this.ChkVehProfs.Size = new System.Drawing.Size(187, 28);
			this.ChkVehProfs.TabIndex = 2;
			this.ChkVehProfs.Text = "Vehicle profiles";
			// 
			// ChkDCF
			// 
			this.ChkDCF.Location = new System.Drawing.Point(8, 96);
			this.ChkDCF.Name = "ChkDCF";
			this.ChkDCF.Size = new System.Drawing.Size(187, 28);
			this.ChkDCF.TabIndex = 1;
			this.ChkDCF.Text = "DCF files";
			// 
			// ChkEDS
			// 
			this.ChkEDS.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ChkEDS.Location = new System.Drawing.Point(8, 35);
			this.ChkEDS.Name = "ChkEDS";
			this.ChkEDS.Size = new System.Drawing.Size(187, 27);
			this.ChkEDS.TabIndex = 0;
			this.ChkEDS.Text = "EDS files";
			// 
			// Install_File_Options
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(459, 327);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Install_File_Options";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "DriveWizard File Replacement options";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
		public Install_File_Options(ref bool [] checkstates)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			localChks = checkstates;
		}
		#endregion initialisation

		#region user interaction
		private void button1_Click(object sender, System.EventArgs e)
		{
			localChks[(int)UninstFileTypes.ALL] = this.ChkAllFiles.Checked;
			localChks[(int)UninstFileTypes.EDS] = this.ChkEDS.Checked;
			localChks[(int)UninstFileTypes.DCF] = this.ChkDCF.Checked;
			localChks[(int) UninstFileTypes.VehProfiles] = this.ChkVehProfs.Checked;
			localChks[(int)UninstFileTypes.DLD] = this.ChkDlds.Checked;
			localChks[(int)UninstFileTypes.Monitor] = this.chkMonitor.Checked;
			localChks[(int)UninstFileTypes.XML] = this.chkXML.Checked;
			localChks[(int)UninstFileTypes.EventIds] = this.ChkEvents.Checked;
			this.Close();
		}

		private void ChkAllFiles_CheckedChanged(object sender, System.EventArgs e)
		{
			if(ChkAllFiles.Checked == true)
			{
				#region check all enabled checkboxes - ignnore disabled ones
				if(this.ChkEDS.Enabled == true)
				{
					this.ChkEDS.Checked = true;
				}
				if(this.ChkDCF.Enabled == true)
				{
					this.ChkDCF.Checked = true;
				}
				if(this.ChkVehProfs.Enabled == true)
				{
					this.ChkVehProfs.Checked = true;
				}
				if(this.ChkDlds.Enabled == true)
				{
					this.ChkDlds.Checked = true;
				}
				if(this.chkMonitor.Enabled == true)
				{
					this.chkMonitor.Checked = true;
				}
				if(this.chkXML.Enabled == true)
				{
					this.chkXML.Checked = true;
				}
				if(this.ChkEvents.Enabled == true)
				{
					this.ChkEvents.Checked = true;
				}
				#endregion check all enabled checkboxes - ignnore disabled ones
			}
			else
			{
				#region uncheck all enabled checkboxes
				if(this.ChkEDS.Enabled == true)
				{
					this.ChkEDS.Checked = false;
				}
				if(this.ChkDCF.Enabled == true)
				{
					this.ChkDCF.Checked = false;
				}
				if(this.ChkVehProfs.Enabled == true)
				{
					this.ChkVehProfs.Checked = false;
				}
				if(this.ChkDlds.Enabled == true)
				{
					this.ChkDlds.Checked = false;
				}
				if(this.chkMonitor.Enabled == true)
				{
					this.chkMonitor.Checked = false;
				}
				if(this.chkXML.Enabled == true)
				{
					this.chkXML.Checked = false;
				}
				if(this.ChkEvents.Enabled == true)
				{
					this.ChkEvents.Checked = false;
				}
				#endregion uncheck all enabled checkboxes
			}
		}
		#endregion user interaction
		#region finalisation
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


#endregion finalisation

		private void label1_Click(object sender, System.EventArgs e)
		{
		
		}

		
	}
}
