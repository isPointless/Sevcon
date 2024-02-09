/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.3$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:33:58$
	$ModDate:05/12/2007 22:33:44$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  62507: UnistallOptions.cs 

   Rev 1.3    05/12/2007 22:33:58  ak
 TC keywords added to source for version control.



*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DriveWizard
{
	public enum UninstFileTypes {  ALL, EDS, DCF, VehProfiles, UsrPrefs, DLD};
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class UNINSTALL_OPTIONS_WINDOW : System.Windows.Forms.Form
	{

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.CheckBox ChkDlds;
		private System.Windows.Forms.CheckBox ChkUsrPrefs;
		private System.Windows.Forms.CheckBox ChkVehProfs;
		private System.Windows.Forms.CheckBox ChkDCF;
		private System.Windows.Forms.CheckBox ChkEDS;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.CheckBox ChkAllFiles;
		private bool [] localChks;

		public UNINSTALL_OPTIONS_WINDOW(ref bool [] checkstates)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			localChks = checkstates;
		}

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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.ChkAllFiles = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.ChkDlds = new System.Windows.Forms.CheckBox();
			this.ChkUsrPrefs = new System.Windows.Forms.CheckBox();
			this.ChkVehProfs = new System.Windows.Forms.CheckBox();
			this.ChkDCF = new System.Windows.Forms.CheckBox();
			this.ChkEDS = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.ChkAllFiles);
			this.groupBox1.Controls.Add(this.button1);
			this.groupBox1.Controls.Add(this.ChkDlds);
			this.groupBox1.Controls.Add(this.ChkUsrPrefs);
			this.groupBox1.Controls.Add(this.ChkVehProfs);
			this.groupBox1.Controls.Add(this.ChkDCF);
			this.groupBox1.Controls.Add(this.ChkEDS);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.groupBox1.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(7, 14);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(446, 319);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Check the file types to uninstall";
			// 
			// ChkAllFiles
			// 
			this.ChkAllFiles.Location = new System.Drawing.Point(13, 284);
			this.ChkAllFiles.Name = "ChkAllFiles";
			this.ChkAllFiles.Size = new System.Drawing.Size(274, 21);
			this.ChkAllFiles.TabIndex = 6;
			this.ChkAllFiles.Text = "Remove All files";
			this.ChkAllFiles.CheckedChanged += new System.EventHandler(this.ChkAllFiles_CheckedChanged);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(340, 277);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(100, 28);
			this.button1.TabIndex = 5;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// ChkDlds
			// 
			this.ChkDlds.Location = new System.Drawing.Point(13, 173);
			this.ChkDlds.Name = "ChkDlds";
			this.ChkDlds.Size = new System.Drawing.Size(187, 28);
			this.ChkDlds.TabIndex = 4;
			this.ChkDlds.Text = "Download files";
			// 
			// ChkUsrPrefs
			// 
			this.ChkUsrPrefs.Checked = true;
			this.ChkUsrPrefs.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ChkUsrPrefs.Location = new System.Drawing.Point(13, 139);
			this.ChkUsrPrefs.Name = "ChkUsrPrefs";
			this.ChkUsrPrefs.Size = new System.Drawing.Size(187, 27);
			this.ChkUsrPrefs.TabIndex = 3;
			this.ChkUsrPrefs.Text = "User Preferences record";
			// 
			// ChkVehProfs
			// 
			this.ChkVehProfs.Location = new System.Drawing.Point(13, 104);
			this.ChkVehProfs.Name = "ChkVehProfs";
			this.ChkVehProfs.Size = new System.Drawing.Size(187, 28);
			this.ChkVehProfs.TabIndex = 2;
			this.ChkVehProfs.Text = "Vehicle profiles";
			// 
			// ChkDCF
			// 
			this.ChkDCF.Location = new System.Drawing.Point(13, 69);
			this.ChkDCF.Name = "ChkDCF";
			this.ChkDCF.Size = new System.Drawing.Size(187, 28);
			this.ChkDCF.TabIndex = 1;
			this.ChkDCF.Text = "DCF files";
			// 
			// ChkEDS
			// 
			this.ChkEDS.Checked = true;
			this.ChkEDS.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ChkEDS.Location = new System.Drawing.Point(13, 35);
			this.ChkEDS.Name = "ChkEDS";
			this.ChkEDS.Size = new System.Drawing.Size(187, 27);
			this.ChkEDS.TabIndex = 0;
			this.ChkEDS.Text = "EDS files";
			// 
			// UNINSTALL_OPTIONS_WINDOW
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(459, 346);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UNINSTALL_OPTIONS_WINDOW";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "DriveWizard Uninstall options";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
			localChks[(int)UninstFileTypes.ALL] = this.ChkAllFiles.Checked;
			localChks[(int)UninstFileTypes.EDS] = this.ChkEDS.Checked;
			localChks[(int)UninstFileTypes.DCF] = this.ChkDCF.Checked;
			localChks[(int) UninstFileTypes.VehProfiles] = this.ChkVehProfs.Checked;
			localChks[(int)UninstFileTypes.UsrPrefs] = this.ChkUsrPrefs.Checked;
			localChks[(int)UninstFileTypes.DLD] = this.ChkDlds.Checked;
			this.Close();
		}

		private void ChkAllFiles_CheckedChanged(object sender, System.EventArgs e)
		{
			if(ChkAllFiles.Checked == true)
			{
				this.ChkEDS.Checked = true;
				this.ChkDCF.Checked = true;
				this.ChkVehProfs.Checked = true;
				this.ChkUsrPrefs.Checked = true;
				this.ChkDlds.Checked = true;
			}
			else
			{
				this.ChkEDS.Checked = false;
				this.ChkDCF.Checked = false;
				this.ChkVehProfs.Checked = false;
				this.ChkUsrPrefs.Checked = false;
				this.ChkDlds.Checked = false;
			}
		}
	}
}
