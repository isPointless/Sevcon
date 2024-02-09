/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.4$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:13:16$
	$ModDate:05/12/2007 21:45:28$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
    In all windows that result with communication with a physical device, 
    this pop-up window is used to display any error messages. Interpreted fault 
    information is displayed for Sevcon nodes. Multiple errors are collated into 
    one error message window.
 * 
REFERENCES    

MODIFICATION HISTORY
    $Log:  61694: ErrorMessageWindow.cs 

   Rev 1.4    05/12/2007 22:13:16  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;

namespace DriveWizard
{
	/// <summary>
	/// Summary description for ErrorMessageWindow.
	/// </summary>
	public class ErrorMessageWindow : System.Windows.Forms.Form
	{
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ErrorMessageWindow(string errMessage)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.richTextBox1.Text = errMessage;
			SystemInfo.errorSB = new StringBuilder();
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
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// richTextBox1
			// 
			this.richTextBox1.Location = new System.Drawing.Point(27, 42);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(526, 228);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "richTextBox1";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(207, 291);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(140, 35);
			this.button1.TabIndex = 1;
			this.button1.Text = "&Close";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(33, 7);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(380, 28);
			this.label1.TabIndex = 2;
			this.label1.Text = "Reported errors:";
			// 
			// ErrorMessageWindow
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(573, 331);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.richTextBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ErrorMessageWindow";
			this.Text = "ErrorMessageWindow";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
