/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.1$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:34:00$
	$ModDate:05/12/2007 22:33:22$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  66464: UnInstallFileOptions.cs 

   Rev 1.1    05/12/2007 22:34:00  ak
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
	public class UnInstallFileOptions : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.RadioButton RBkeepUser;
		private System.Windows.Forms.RadioButton RBRemoveAll;
		private bool [] _removeAll;  //use an array because it is a reference type

		public UnInstallFileOptions( ref bool [] removeAll)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			_removeAll = removeAll; //equate the refenrences
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(UnInstallFileOptions));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.RBkeepUser = new System.Windows.Forms.RadioButton();
			this.RBRemoveAll = new System.Windows.Forms.RadioButton();
			this.button1 = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.RBkeepUser);
			this.groupBox1.Controls.Add(this.RBRemoveAll);
			this.groupBox1.Location = new System.Drawing.Point(0, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(352, 160);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Options";
			// 
			// RBkeepUser
			// 
			this.RBkeepUser.Checked = true;
			this.RBkeepUser.Location = new System.Drawing.Point(24, 24);
			this.RBkeepUser.Name = "RBkeepUser";
			this.RBkeepUser.Size = new System.Drawing.Size(312, 72);
			this.RBkeepUser.TabIndex = 1;
			this.RBkeepUser.TabStop = true;
			this.RBkeepUser.Text = "I want to uninstall DriveWizard but  to keep my DriveWizard user files";
			// 
			// RBRemoveAll
			// 
			this.RBRemoveAll.Location = new System.Drawing.Point(24, 96);
			this.RBRemoveAll.Name = "RBRemoveAll";
			this.RBRemoveAll.Size = new System.Drawing.Size(304, 56);
			this.RBRemoveAll.TabIndex = 0;
			this.RBRemoveAll.Text = "I want to completely uninstall DriveWIzard";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(232, 184);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(120, 32);
			this.button1.TabIndex = 2;
			this.button1.Text = "&OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// UnInstallFileOptions
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(362, 231);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.groupBox1);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UnInstallFileOptions";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Uninstall File Options";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.UnInstallFileOptions_Closing);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void UnInstallFileOptions_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if( this.RBRemoveAll.Checked == true)
			{
				this._removeAll[0] = true;
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
