/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.5$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:12:56$
	$ModDate:05/12/2007 22:01:30$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	

REFERENCES    

MODIFICATION HISTORY
    $Log:  36795: PERSONALITY_PARAMS_GRAPHICS.cs 

   Rev 1.5    05/12/2007 22:12:56  ak
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
	/// Summary description for PERSONALITY_PARAMS_GRAPHICS.
	/// </summary>
	public class PERSONALITY_PARAMS_GRAPHICS : System.Windows.Forms.Form
	{
		#region controls declarations
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		#region my declarations
		private SystemInfo localSystemInfo;
		private int selectednodeID = 0;
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(872, 800);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(120, 25);
			this.button1.TabIndex = 0;
			this.button1.Text = "&Close window";
			this.button1.Click += new System.EventHandler(this.closeBtn_Click);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Location = new System.Drawing.Point(26, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(960, 30);
			this.label1.TabIndex = 1;
			this.label1.Text = "Graphicla Personality Parameter setting. Not implemeted in this version. Close th" +
				"is window";
			// 
			// PERSONALITY_PARAMS_GRAPHICS
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
			this.ClientSize = new System.Drawing.Size(1013, 852);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button1);
			this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "PERSONALITY_PARAMS_GRAPHICS";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "PERSONALITY_PARAMS_GRAPHICS";
			this.Load += new System.EventHandler(this.PERSONALITY_PARAMS_GRAPHICS_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
		/*--------------------------------------------------------------------------
		 *  Name			: PERSONALITY_PARAMS_GRAPHICS()
		 *  Description     : Constructor function for form. Set up of any initial variables
		 *					  that are available prior to th eform load event.
		 *  Parameters      : systemInfo class, CANopen node number and a descriptive
		 *					  string about the current CANopen node.
		 *  Used Variables  : none
		 *  Preconditions   : This form is only available when at least one SEVCON or 3rd party node is 
		 *					  connected.  SEVCON nodes can only be selected when the user has logged in.
		 *  Return value    : none
		 *--------------------------------------------------------------------------*/
		public PERSONALITY_PARAMS_GRAPHICS(ref SystemInfo systemInfo, int nodeNum, string nodeText)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			localSystemInfo = systemInfo;
			selectednodeID = nodeNum;

			label1.Text = "Personality Parameters for " + nodeText;
		}
		#endregion

		#region user interaction zone
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

		private void PERSONALITY_PARAMS_GRAPHICS_Load(object sender, System.EventArgs e)
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
