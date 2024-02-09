/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.10$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:13:20$
	$ModDate:05/12/2007 21:42:42$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	This window displays the DW version and provides a contact link to 
    the Sevcon website.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36747: HELP_ABOUT_WINDOW.cs 

   Rev 1.10    05/12/2007 22:13:20  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DriveWizard
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class HELP_ABOUT_WINDOW : System.Windows.Forms.Form
	{
		#region form declarations
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label about_lbl1;
		private System.Windows.Forms.Button closeBtn;
		private System.Windows.Forms.LinkLabel contact_lbl2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion form declarations

		#region my declarations
		#endregion my declarations

		#region initialisation
		public HELP_ABOUT_WINDOW(string version, string release, string versionDescription)
		{
			InitializeComponent();
			this.about_lbl1.Text = "Version: " + version + ":" + release + "\n" +  versionDescription 
				+ "\n\nThis software is intended for use with Sevcon motor controllers only . All rights reserved";
		}
		#endregion initialisation

		#region user interaction
		#endregion user interaction

		#region finalisation/exit methods
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
		#endregion finalisation/exit methods

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(HELP_ABOUT_WINDOW));
			this.about_lbl1 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.closeBtn = new System.Windows.Forms.Button();
			this.contact_lbl2 = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// about_lbl1
			// 
			this.about_lbl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.about_lbl1.ForeColor = System.Drawing.Color.MidnightBlue;
			this.about_lbl1.Location = new System.Drawing.Point(8, 64);
			this.about_lbl1.Name = "about_lbl1";
			this.about_lbl1.Size = new System.Drawing.Size(376, 192);
			this.about_lbl1.TabIndex = 0;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(262, 8);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(127, 48);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// closeBtn
			// 
			this.closeBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.closeBtn.ForeColor = System.Drawing.Color.MidnightBlue;
			this.closeBtn.Location = new System.Drawing.Point(140, 318);
			this.closeBtn.Name = "closeBtn";
			this.closeBtn.Size = new System.Drawing.Size(115, 25);
			this.closeBtn.TabIndex = 3;
			this.closeBtn.Text = "&Close";
			this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
			// 
			// contact_lbl2
			// 
			this.contact_lbl2.AutoSize = true;
			this.contact_lbl2.LinkArea = new System.Windows.Forms.LinkArea(0, 100);
			this.contact_lbl2.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
			this.contact_lbl2.Location = new System.Drawing.Point(8, 280);
			this.contact_lbl2.Name = "contact_lbl2";
			this.contact_lbl2.Size = new System.Drawing.Size(157, 22);
			this.contact_lbl2.TabIndex = 4;
			this.contact_lbl2.TabStop = true;
			this.contact_lbl2.Text = "Visit Sevcon website";
			this.contact_lbl2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.contact_lbl2_LinkClicked);
			// 
			// HELP_ABOUT_WINDOW
			// 
			this.AcceptButton = this.closeBtn;
			this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
			this.ClientSize = new System.Drawing.Size(394, 358);
			this.Controls.Add(this.contact_lbl2);
			this.Controls.Add(this.closeBtn);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.about_lbl1);
			this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "HELP_ABOUT_WINDOW";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "About DriveWizard";
			this.Load += new System.EventHandler(this.HELP_ABOUT_WINDOW_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void closeBtn_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void HELP_ABOUT_WINDOW_Load(object sender, System.EventArgs e)
		{
			#region button colouring
			foreach (Control myControl in this.Controls)
			{
				if((myControl.GetType().ToString()) == "System.Windows.Forms.Button")
				{
					myControl.BackColor = SCCorpStyle.buttonBackGround;
				}
			}
			#endregion button colouring
		}

		private void contact_lbl2_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			try 
			{
				// Call the Process.Start method to open the default browser with a URL:
				System.Diagnostics.Process.Start("http://www.sevcon.com");
				contact_lbl2.LinkVisited = true;
			}
			catch//(System.Security.SecurityException err)
			{
				SystemInfo.errorSB.Append("\nUnable to connect to internet");
			}
		}
	}
}
