/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.5$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:13:18$
	$ModDate:05/12/2007 22:11:28$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	This window is currently not used in the project. 
    A default viewer is used to display retrieved EEPROM contents.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36731: EEPROM_VIEW_WINDOW.cs 

   Rev 1.5    05/12/2007 22:13:18  ak
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
	/// Summary description for EEPROM_VIEW_WINDOW.
	/// </summary>
	public class EEPROM_VIEW_WINDOW : System.Windows.Forms.Form
	{
		#region controls declarations
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.Button button1;
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
			this.dataGrid1 = new System.Windows.Forms.DataGrid();
			this.button1 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGrid1
			// 
			this.dataGrid1.DataMember = "";
			this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid1.Location = new System.Drawing.Point(72, 32);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.Size = new System.Drawing.Size(608, 400);
			this.dataGrid1.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(600, 472);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 25);
			this.button1.TabIndex = 3;
			this.button1.Text = "&Close";
			// 
			// EEPROM_VIEW_WINDOW
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
			this.ClientSize = new System.Drawing.Size(760, 542);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.dataGrid1);
			this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ForeColor = System.Drawing.Color.MidnightBlue;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EEPROM_VIEW_WINDOW";
			this.Text = "View EEPROM contents";
			this.Load += new System.EventHandler(this.EEPROM_VIEW_WINDOW_Load);
			this.Closed += new System.EventHandler(this.EEPROM_VIEW_WINDOW_Closed);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
		public EEPROM_VIEW_WINDOW()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}
		#endregion

		#region user interaction zone
		#endregion

		#region finalisation/exit
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

		private void EEPROM_VIEW_WINDOW_Load(object sender, System.EventArgs e)
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

		private void EEPROM_VIEW_WINDOW_Closed(object sender, System.EventArgs e)
		{
		
		}
	}
}
