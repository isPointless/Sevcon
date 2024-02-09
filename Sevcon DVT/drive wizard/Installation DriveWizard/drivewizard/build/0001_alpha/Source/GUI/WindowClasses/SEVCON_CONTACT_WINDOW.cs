/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.12$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:13:18$
	$ModDate:05/12/2007 22:11:24$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION

REFERENCES    

MODIFICATION HISTORY
    $Log:  36807: SEVCON_CONTACT_WINDOW.cs 

   Rev 1.12    05/12/2007 22:13:18  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace DriveWizard
{
	/// <summary>
	/// Summary description for SEVCON_CONTACT_WINDOW.
	/// </summary>
	public class SEVCON_CONTACT_WINDOW : System.Windows.Forms.Form
	{
		#region controls declaratinos
		private System.Windows.Forms.Label contact_lbl;
		private System.Windows.Forms.LinkLabel contact_lbl2;
		private System.Windows.Forms.Button closeBtn;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		#region my declarations
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.contact_lbl = new System.Windows.Forms.Label();
			this.contact_lbl2 = new System.Windows.Forms.LinkLabel();
			this.closeBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// contact_lbl
			// 
			this.contact_lbl.Location = new System.Drawing.Point(24, 48);
			this.contact_lbl.Name = "contact_lbl";
			this.contact_lbl.Size = new System.Drawing.Size(232, 23);
			this.contact_lbl.TabIndex = 0;
			// 
			// contact_lbl2
			// 
			this.contact_lbl2.LinkArea = new System.Windows.Forms.LinkArea(0, 100);
			this.contact_lbl2.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
			this.contact_lbl2.Location = new System.Drawing.Point(16, 104);
			this.contact_lbl2.Name = "contact_lbl2";
			this.contact_lbl2.Size = new System.Drawing.Size(248, 23);
			this.contact_lbl2.TabIndex = 1;
			this.contact_lbl2.TabStop = true;
			this.contact_lbl2.Text = "contact: SEVCON";
			this.contact_lbl2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.contact_lbl2_LinkClicked);
			// 
			// closeBtn
			// 
			this.closeBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.closeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.closeBtn.Location = new System.Drawing.Point(86, 216);
			this.closeBtn.Name = "closeBtn";
			this.closeBtn.Size = new System.Drawing.Size(120, 25);
			this.closeBtn.TabIndex = 2;
			this.closeBtn.Text = "&Close";
			this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
			// 
			// SEVCON_CONTACT_WINDOW
			// 
			this.AcceptButton = this.closeBtn;
			this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.closeBtn);
			this.Controls.Add(this.contact_lbl2);
			this.Controls.Add(this.contact_lbl);
			this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SEVCON_CONTACT_WINDOW";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Contact SEVCON";
			this.Load += new System.EventHandler(this.SEVCON_CONTACT_WINDOW_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
		public SEVCON_CONTACT_WINDOW()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			this.Font = SCCorpStyle.SCfont;
			this.BackColor = SCCorpStyle.SCBackColour;
			this.ForeColor = SCCorpStyle.SCForeColour;
            		
		}
		#endregion

		#region user interaction zone
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
				Message.Show("Unable to connect to internet");
			}
		}
		#endregion

		#region finalisation/exit
		/// <summary>
		///		/*--------------------------------------------------------------------------
		///		 *  Name			: closeBtn_Click
		///		 *  Description     : Event handler for the button used to close this window
		///		 *  Parameters      : system generated
		///		 *  Used Variables  : none
		///		 *  Preconditions   : none - any will be dealt with in a window closing event handler 
		///		 *  Post-conditions : 
		///		 *  Return value    : n/a
		///		 *--------------------------------------------------------------------------*/
		///		 </summary>
		private void closeBtn_Click(object sender, System.EventArgs e)
		{
			this.Close();
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
		#endregion

		private void SEVCON_CONTACT_WINDOW_Load(object sender, System.EventArgs e)
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
	}
}
