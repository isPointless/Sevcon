/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.10$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:12:44$
	$ModDate:05/12/2007 21:43:26$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	This window is the warning splash screen shown when DW starts up and 
    displays the DW & espAC logos.

REFERENCES    

MODIFICATION HISTORY
    $Log:  53423: Form3.cs 

   Rev 1.10    05/12/2007 22:12:44  ak
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
	/// Summary description for Form3.
	/// </summary>
	public class Form3 : System.Windows.Forms.Form
	{
		#region Control Definitions
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.ComponentModel.IContainer components = null;
		#endregion Control Definitions


		public Form3()
		{
			InitializeComponent();
			foreach(Control c in this.Controls)
			{
				formatControls(c);
			}
			this.label1.Text = "Improper use of this application can seriously affect vehicle performance.\nDo you wish to continue?";
		}

		private void formatControls(System.Windows.Forms.Control topControl )
		{
			#region format individual controls
			topControl.ForeColor = SCCorpStyle.SCForeColour;
			topControl.Font = new System.Drawing.Font("Arial", 10F);
			topControl.BackColor = Color.White;
			if ( topControl.GetType().Equals( typeof( Button ) ) ) 
			{
				topControl.BackColor = SCCorpStyle.buttonBackGround;
			}
			#endregion format individual controls
			foreach(Control control in topControl.Controls) 
			{
				formatControls(control);
			}
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form3));
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.button1.BackColor = System.Drawing.Color.Transparent;
			this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.button1.Location = new System.Drawing.Point(10, 425);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(134, 46);
			this.button1.TabIndex = 1;
			this.button1.Text = "C&ontinue";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.BackColor = System.Drawing.Color.Transparent;
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.button2.Location = new System.Drawing.Point(403, 425);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(135, 46);
			this.button2.TabIndex = 2;
			this.button2.Text = "E&xit";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.pictureBox2);
			this.panel1.Controls.Add(this.pictureBox1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(540, 367);
			this.panel1.TabIndex = 5;
			// 
			// pictureBox2
			// 
			this.pictureBox2.BackColor = System.Drawing.Color.White;
			this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Left;
			this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
			this.pictureBox2.Location = new System.Drawing.Point(0, 0);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(38, 367);
			this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox2.TabIndex = 8;
			this.pictureBox2.TabStop = false;
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(244)), ((System.Byte)(244)), ((System.Byte)(251)));
			this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(540, 367);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 6;
			this.pictureBox1.TabStop = false;
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.White;
			this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label1.Location = new System.Drawing.Point(10, 378);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(528, 37);
			this.label1.TabIndex = 6;
			this.label1.Text = "label1";
			// 
			// Form3
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(540, 479);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.button2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form3";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "DriveWizard";
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			this.Owner.Close();
		}
	}
}
