/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.2$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:28:28$
	$ModDate:05/12/2007 22:27:30$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  66279: Form1.cs 

   Rev 1.2    05/12/2007 22:28:28  ak
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
	public class SC_TESTPOINT_DIALOG : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.TextBox TBVoltsOrSpeed;
		private System.Windows.Forms.TextBox TBtime;
		private System.Windows.Forms.Label LblVoltsOrSpeed;
		private System.Windows.Forms.Label LblTime;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label unitsLabel1;
		private System.Windows.Forms.Label unitsLabel2;

		public static PointF testPointData = new PointF();

		public SC_TESTPOINT_DIALOG(DriveWizard.TabPages profileTestType)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			switch(profileTestType)
			{
				case TabPages.OPEN_LOOP:
					break;
				case TabPages.CLOSED_LOOP:
					this.LblVoltsOrSpeed.Text = "Current";
					this.unitsLabel1.Text = "A";
					this.unitsLabel2.Text = "rpm";
					this.LblTime.Text =  "Speed";
					break;
				case TabPages.NO_LOAD_TEST:
					this.LblVoltsOrSpeed.Text = "Speed:";
					this.unitsLabel1.Text = "rpm";
					this.TBtime.Visible = false;
					this.LblTime.Visible = false;
					break;
				default:
					return;
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.unitsLabel2 = new System.Windows.Forms.Label();
			this.unitsLabel1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.LblTime = new System.Windows.Forms.Label();
			this.LblVoltsOrSpeed = new System.Windows.Forms.Label();
			this.TBtime = new System.Windows.Forms.TextBox();
			this.TBVoltsOrSpeed = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.unitsLabel2);
			this.groupBox1.Controls.Add(this.unitsLabel1);
			this.groupBox1.Controls.Add(this.button1);
			this.groupBox1.Controls.Add(this.LblTime);
			this.groupBox1.Controls.Add(this.LblVoltsOrSpeed);
			this.groupBox1.Controls.Add(this.TBtime);
			this.groupBox1.Controls.Add(this.TBVoltsOrSpeed);
			this.groupBox1.Location = new System.Drawing.Point(0, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(312, 200);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Enter test point data";
			// 
			// unitsLabel2
			// 
			this.unitsLabel2.Location = new System.Drawing.Point(272, 110);
			this.unitsLabel2.Name = "unitsLabel2";
			this.unitsLabel2.Size = new System.Drawing.Size(16, 16);
			this.unitsLabel2.TabIndex = 7;
			this.unitsLabel2.Text = "s";
			// 
			// unitsLabel1
			// 
			this.unitsLabel1.Location = new System.Drawing.Point(272, 62);
			this.unitsLabel1.Name = "unitsLabel1";
			this.unitsLabel1.Size = new System.Drawing.Size(16, 16);
			this.unitsLabel1.TabIndex = 6;
			this.unitsLabel1.Text = "%";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(96, 152);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(96, 32);
			this.button1.TabIndex = 5;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// LblTime
			// 
			this.LblTime.AutoSize = true;
			this.LblTime.Location = new System.Drawing.Point(16, 80);
			this.LblTime.Name = "LblTime";
			this.LblTime.Size = new System.Drawing.Size(161, 18);
			this.LblTime.TabIndex = 4;
			this.LblTime.Text = "Number of 500us samples";
			// 
			// LblVoltsOrSpeed
			// 
			this.LblVoltsOrSpeed.AutoSize = true;
			this.LblVoltsOrSpeed.Location = new System.Drawing.Point(16, 32);
			this.LblVoltsOrSpeed.Name = "LblVoltsOrSpeed";
			this.LblVoltsOrSpeed.Size = new System.Drawing.Size(239, 18);
			this.LblVoltsOrSpeed.TabIndex = 3;
			this.LblVoltsOrSpeed.Text = "Percentage of  Battery voltage to apply:";
			// 
			// TBtime
			// 
			this.TBtime.Location = new System.Drawing.Point(16, 104);
			this.TBtime.Name = "TBtime";
			this.TBtime.Size = new System.Drawing.Size(256, 22);
			this.TBtime.TabIndex = 2;
			this.TBtime.Text = "";
			// 
			// TBVoltsOrSpeed
			// 
			this.TBVoltsOrSpeed.Location = new System.Drawing.Point(16, 56);
			this.TBVoltsOrSpeed.Name = "TBVoltsOrSpeed";
			this.TBVoltsOrSpeed.Size = new System.Drawing.Size(256, 22);
			this.TBVoltsOrSpeed.TabIndex = 1;
			this.TBVoltsOrSpeed.Text = "";
			// 
			// SC_TESTPOINT_DIALOG
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(314, 215);
			this.Controls.Add(this.groupBox1);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SC_TESTPOINT_DIALOG";
			this.Text = "Form1";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.SC_TESTPOINT_DIALOG_Closing);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void SC_TESTPOINT_DIALOG_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				testPointData.X = float.Parse(this.TBtime.Text);
				testPointData.Y = float.Parse(this.TBVoltsOrSpeed.Text);
				testPointData.Y = testPointData.Y/100;
			}
			catch
			{
				testPointData = new PointF(0,0);
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
