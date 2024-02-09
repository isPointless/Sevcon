/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.3$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:13:12$
	$ModDate:05/12/2007 22:06:00$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION

REFERENCES    

MODIFICATION HISTORY
    $Log:  36743: GUI_FORCE_VALUES.cs 

   Rev 1.3    05/12/2007 22:13:12  ak
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
	/// Summary description for GUI_FORCE_VALUES.
	/// </summary>
	public class GUI_FORCE_VALUES : System.Windows.Forms.Form
	{
		#region control declarations
		private System.Windows.Forms.Button button1;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		#region my declarations
		//declare logical arrays for controls
		ListBox [] nodeIDs = new ListBox [SCCorpStyle.maxConnectedDevices];
		RadioButton [] masterslave = new RadioButton [SCCorpStyle.maxConnectedDevices];
		ComboBox [] EDSs = new ComboBox [SCCorpStyle.maxConnectedDevices];
		ComboBox [] function = new ComboBox [SCCorpStyle.maxConnectedDevices];
		ComboBox [] manuf = new ComboBox [SCCorpStyle.maxConnectedDevices];

		string [] manufacturers = { "SEVCON", "3rd Party", "Unknown"};
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox comboBox5;
		private System.Windows.Forms.ComboBox comboBox6;
		private System.Windows.Forms.ComboBox comboBox7;
		private System.Windows.Forms.ComboBox comboBox8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.ComboBox comboBox3;
		private System.Windows.Forms.ComboBox comboBox4;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ListBox nodeIDLB6;
		private System.Windows.Forms.ListBox nodeIDLB7;
		private System.Windows.Forms.ListBox nodeIDLB8;
		private System.Windows.Forms.ListBox nodeIDLB5;
		private System.Windows.Forms.ListBox nodeIDLB4;
		private System.Windows.Forms.ListBox nodeIDLB2;
		private System.Windows.Forms.ListBox nodeIDLB3;
		private System.Windows.Forms.ListBox nodeIDLB1;
		private System.Windows.Forms.ComboBox EDS5;
		private System.Windows.Forms.ComboBox EDS6;
		private System.Windows.Forms.ComboBox EDS8;
		private System.Windows.Forms.ComboBox func8;
		private System.Windows.Forms.ComboBox EDS2;
		private System.Windows.Forms.ComboBox EDS3;
		private System.Windows.Forms.ComboBox EDS4;
		private System.Windows.Forms.ComboBox EDS1;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton ms4;
		private System.Windows.Forms.ComboBox manf4;
		private System.Windows.Forms.ComboBox func4;
		private System.Windows.Forms.RadioButton ms3;
		private System.Windows.Forms.ComboBox manf3;
		private System.Windows.Forms.ComboBox func3;
		private System.Windows.Forms.RadioButton ms2;
		private System.Windows.Forms.ComboBox manf2;
		private System.Windows.Forms.ComboBox func2;
		private System.Windows.Forms.RadioButton ms1;
		private System.Windows.Forms.ComboBox manf1;
		private System.Windows.Forms.ComboBox func1;
		private System.Windows.Forms.RadioButton ms8;
		private System.Windows.Forms.ComboBox manf8;
		private System.Windows.Forms.ComboBox EDS7;
		private System.Windows.Forms.RadioButton ms7;
		private System.Windows.Forms.ComboBox manf7;
		private System.Windows.Forms.ComboBox func7;
		private System.Windows.Forms.RadioButton ms6;
		private System.Windows.Forms.ComboBox manf6;
		private System.Windows.Forms.ComboBox func6;
		private System.Windows.Forms.RadioButton ms5;
		private System.Windows.Forms.ComboBox manf5;
		private System.Windows.Forms.ComboBox func5;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ComboBox comboBox9;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.TextBox profileNameTB;
		private System.Windows.Forms.TextBox passwordTB;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.TextBox UserIdTB;
		string [] deviceTypes = {"LH Controller", "RH Controller", "Pump", "Display", "Unknown"};
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.comboBox5 = new System.Windows.Forms.ComboBox();
			this.comboBox6 = new System.Windows.Forms.ComboBox();
			this.comboBox7 = new System.Windows.Forms.ComboBox();
			this.comboBox8 = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.comboBox3 = new System.Windows.Forms.ComboBox();
			this.comboBox4 = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.nodeIDLB6 = new System.Windows.Forms.ListBox();
			this.nodeIDLB7 = new System.Windows.Forms.ListBox();
			this.nodeIDLB8 = new System.Windows.Forms.ListBox();
			this.nodeIDLB5 = new System.Windows.Forms.ListBox();
			this.nodeIDLB4 = new System.Windows.Forms.ListBox();
			this.nodeIDLB2 = new System.Windows.Forms.ListBox();
			this.nodeIDLB3 = new System.Windows.Forms.ListBox();
			this.nodeIDLB1 = new System.Windows.Forms.ListBox();
			this.EDS5 = new System.Windows.Forms.ComboBox();
			this.EDS6 = new System.Windows.Forms.ComboBox();
			this.EDS8 = new System.Windows.Forms.ComboBox();
			this.func8 = new System.Windows.Forms.ComboBox();
			this.EDS2 = new System.Windows.Forms.ComboBox();
			this.EDS3 = new System.Windows.Forms.ComboBox();
			this.EDS4 = new System.Windows.Forms.ComboBox();
			this.EDS1 = new System.Windows.Forms.ComboBox();
			this.label23 = new System.Windows.Forms.Label();
			this.label24 = new System.Windows.Forms.Label();
			this.label25 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.ms4 = new System.Windows.Forms.RadioButton();
			this.manf4 = new System.Windows.Forms.ComboBox();
			this.func4 = new System.Windows.Forms.ComboBox();
			this.ms3 = new System.Windows.Forms.RadioButton();
			this.manf3 = new System.Windows.Forms.ComboBox();
			this.func3 = new System.Windows.Forms.ComboBox();
			this.ms2 = new System.Windows.Forms.RadioButton();
			this.manf2 = new System.Windows.Forms.ComboBox();
			this.func2 = new System.Windows.Forms.ComboBox();
			this.ms1 = new System.Windows.Forms.RadioButton();
			this.manf1 = new System.Windows.Forms.ComboBox();
			this.func1 = new System.Windows.Forms.ComboBox();
			this.ms8 = new System.Windows.Forms.RadioButton();
			this.manf8 = new System.Windows.Forms.ComboBox();
			this.EDS7 = new System.Windows.Forms.ComboBox();
			this.ms7 = new System.Windows.Forms.RadioButton();
			this.manf7 = new System.Windows.Forms.ComboBox();
			this.func7 = new System.Windows.Forms.ComboBox();
			this.ms6 = new System.Windows.Forms.RadioButton();
			this.manf6 = new System.Windows.Forms.ComboBox();
			this.func6 = new System.Windows.Forms.ComboBox();
			this.ms5 = new System.Windows.Forms.RadioButton();
			this.manf5 = new System.Windows.Forms.ComboBox();
			this.func5 = new System.Windows.Forms.ComboBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.comboBox9 = new System.Windows.Forms.ComboBox();
			this.label19 = new System.Windows.Forms.Label();
			this.label20 = new System.Windows.Forms.Label();
			this.profileNameTB = new System.Windows.Forms.TextBox();
			this.passwordTB = new System.Windows.Forms.TextBox();
			this.label21 = new System.Windows.Forms.Label();
			this.label22 = new System.Windows.Forms.Label();
			this.UserIdTB = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(224, 632);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(104, 20);
			this.button1.TabIndex = 16;
			this.button1.Text = "&Submit";
			this.button1.Click += new System.EventHandler(this.submitBtn_Click);
			// 
			// button2
			// 
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button2.Location = new System.Drawing.Point(576, 632);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(104, 20);
			this.button2.TabIndex = 101;
			this.button2.Text = "&Close";
			this.button2.Click += new System.EventHandler(this.closeBtn_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.comboBox5);
			this.groupBox1.Controls.Add(this.comboBox6);
			this.groupBox1.Controls.Add(this.comboBox7);
			this.groupBox1.Controls.Add(this.comboBox8);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.label18);
			this.groupBox1.Controls.Add(this.comboBox1);
			this.groupBox1.Controls.Add(this.comboBox2);
			this.groupBox1.Controls.Add(this.comboBox3);
			this.groupBox1.Controls.Add(this.comboBox4);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.nodeIDLB6);
			this.groupBox1.Controls.Add(this.nodeIDLB7);
			this.groupBox1.Controls.Add(this.nodeIDLB8);
			this.groupBox1.Controls.Add(this.nodeIDLB5);
			this.groupBox1.Controls.Add(this.nodeIDLB4);
			this.groupBox1.Controls.Add(this.nodeIDLB2);
			this.groupBox1.Controls.Add(this.nodeIDLB3);
			this.groupBox1.Controls.Add(this.nodeIDLB1);
			this.groupBox1.Controls.Add(this.EDS5);
			this.groupBox1.Controls.Add(this.EDS6);
			this.groupBox1.Controls.Add(this.EDS8);
			this.groupBox1.Controls.Add(this.func8);
			this.groupBox1.Controls.Add(this.EDS2);
			this.groupBox1.Controls.Add(this.EDS3);
			this.groupBox1.Controls.Add(this.EDS4);
			this.groupBox1.Controls.Add(this.EDS1);
			this.groupBox1.Controls.Add(this.label23);
			this.groupBox1.Controls.Add(this.label24);
			this.groupBox1.Controls.Add(this.label25);
			this.groupBox1.Controls.Add(this.label14);
			this.groupBox1.Controls.Add(this.label15);
			this.groupBox1.Controls.Add(this.label16);
			this.groupBox1.Controls.Add(this.label17);
			this.groupBox1.Controls.Add(this.label12);
			this.groupBox1.Controls.Add(this.label13);
			this.groupBox1.Controls.Add(this.label11);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.ms4);
			this.groupBox1.Controls.Add(this.manf4);
			this.groupBox1.Controls.Add(this.func4);
			this.groupBox1.Controls.Add(this.ms3);
			this.groupBox1.Controls.Add(this.manf3);
			this.groupBox1.Controls.Add(this.func3);
			this.groupBox1.Controls.Add(this.ms2);
			this.groupBox1.Controls.Add(this.manf2);
			this.groupBox1.Controls.Add(this.func2);
			this.groupBox1.Controls.Add(this.ms1);
			this.groupBox1.Controls.Add(this.manf1);
			this.groupBox1.Controls.Add(this.func1);
			this.groupBox1.Controls.Add(this.ms8);
			this.groupBox1.Controls.Add(this.manf8);
			this.groupBox1.Controls.Add(this.EDS7);
			this.groupBox1.Controls.Add(this.ms7);
			this.groupBox1.Controls.Add(this.manf7);
			this.groupBox1.Controls.Add(this.func7);
			this.groupBox1.Controls.Add(this.ms6);
			this.groupBox1.Controls.Add(this.manf6);
			this.groupBox1.Controls.Add(this.func6);
			this.groupBox1.Controls.Add(this.ms5);
			this.groupBox1.Controls.Add(this.manf5);
			this.groupBox1.Controls.Add(this.func5);
			this.groupBox1.Location = new System.Drawing.Point(8, 160);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(672, 456);
			this.groupBox1.TabIndex = 148;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Node Details";
			// 
			// comboBox5
			// 
			this.comboBox5.Location = new System.Drawing.Point(488, 424);
			this.comboBox5.Name = "comboBox5";
			this.comboBox5.Size = new System.Drawing.Size(120, 23);
			this.comboBox5.TabIndex = 216;
			// 
			// comboBox6
			// 
			this.comboBox6.Location = new System.Drawing.Point(368, 424);
			this.comboBox6.Name = "comboBox6";
			this.comboBox6.Size = new System.Drawing.Size(120, 23);
			this.comboBox6.TabIndex = 215;
			// 
			// comboBox7
			// 
			this.comboBox7.Location = new System.Drawing.Point(248, 424);
			this.comboBox7.Name = "comboBox7";
			this.comboBox7.Size = new System.Drawing.Size(120, 23);
			this.comboBox7.TabIndex = 214;
			// 
			// comboBox8
			// 
			this.comboBox8.Location = new System.Drawing.Point(128, 424);
			this.comboBox8.Name = "comboBox8";
			this.comboBox8.Size = new System.Drawing.Size(120, 23);
			this.comboBox8.TabIndex = 213;
			// 
			// label7
			// 
			this.label7.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label7.Location = new System.Drawing.Point(8, 424);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(120, 20);
			this.label7.TabIndex = 212;
			this.label7.Text = "revision No";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label9.Location = new System.Drawing.Point(8, 376);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(120, 20);
			this.label9.TabIndex = 211;
			this.label9.Text = "Vendor ID";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label18
			// 
			this.label18.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label18.Location = new System.Drawing.Point(8, 400);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(120, 20);
			this.label18.TabIndex = 210;
			this.label18.Text = "product code";
			this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboBox1
			// 
			this.comboBox1.Location = new System.Drawing.Point(488, 224);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(120, 23);
			this.comboBox1.TabIndex = 209;
			// 
			// comboBox2
			// 
			this.comboBox2.Location = new System.Drawing.Point(368, 224);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(120, 23);
			this.comboBox2.TabIndex = 208;
			// 
			// comboBox3
			// 
			this.comboBox3.Location = new System.Drawing.Point(248, 224);
			this.comboBox3.Name = "comboBox3";
			this.comboBox3.Size = new System.Drawing.Size(120, 23);
			this.comboBox3.TabIndex = 207;
			// 
			// comboBox4
			// 
			this.comboBox4.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.comboBox4.ItemHeight = 15;
			this.comboBox4.Location = new System.Drawing.Point(128, 224);
			this.comboBox4.Name = "comboBox4";
			this.comboBox4.Size = new System.Drawing.Size(120, 23);
			this.comboBox4.TabIndex = 206;
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(8, 224);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(120, 20);
			this.label4.TabIndex = 205;
			this.label4.Text = "revision No";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// nodeIDLB6
			// 
			this.nodeIDLB6.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.nodeIDLB6.ItemHeight = 15;
			this.nodeIDLB6.Location = new System.Drawing.Point(248, 304);
			this.nodeIDLB6.Name = "nodeIDLB6";
			this.nodeIDLB6.Size = new System.Drawing.Size(120, 19);
			this.nodeIDLB6.TabIndex = 204;
			// 
			// nodeIDLB7
			// 
			this.nodeIDLB7.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.nodeIDLB7.ItemHeight = 15;
			this.nodeIDLB7.Location = new System.Drawing.Point(368, 304);
			this.nodeIDLB7.Name = "nodeIDLB7";
			this.nodeIDLB7.Size = new System.Drawing.Size(120, 19);
			this.nodeIDLB7.TabIndex = 203;
			// 
			// nodeIDLB8
			// 
			this.nodeIDLB8.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.nodeIDLB8.ItemHeight = 15;
			this.nodeIDLB8.Location = new System.Drawing.Point(488, 304);
			this.nodeIDLB8.Name = "nodeIDLB8";
			this.nodeIDLB8.Size = new System.Drawing.Size(120, 19);
			this.nodeIDLB8.TabIndex = 202;
			// 
			// nodeIDLB5
			// 
			this.nodeIDLB5.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.nodeIDLB5.ItemHeight = 15;
			this.nodeIDLB5.Location = new System.Drawing.Point(128, 304);
			this.nodeIDLB5.Name = "nodeIDLB5";
			this.nodeIDLB5.Size = new System.Drawing.Size(120, 19);
			this.nodeIDLB5.TabIndex = 201;
			// 
			// nodeIDLB4
			// 
			this.nodeIDLB4.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.nodeIDLB4.ItemHeight = 15;
			this.nodeIDLB4.Location = new System.Drawing.Point(488, 96);
			this.nodeIDLB4.Name = "nodeIDLB4";
			this.nodeIDLB4.Size = new System.Drawing.Size(120, 19);
			this.nodeIDLB4.TabIndex = 200;
			// 
			// nodeIDLB2
			// 
			this.nodeIDLB2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.nodeIDLB2.ItemHeight = 15;
			this.nodeIDLB2.Location = new System.Drawing.Point(248, 96);
			this.nodeIDLB2.Name = "nodeIDLB2";
			this.nodeIDLB2.Size = new System.Drawing.Size(120, 19);
			this.nodeIDLB2.TabIndex = 199;
			// 
			// nodeIDLB3
			// 
			this.nodeIDLB3.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.nodeIDLB3.ItemHeight = 15;
			this.nodeIDLB3.Location = new System.Drawing.Point(368, 96);
			this.nodeIDLB3.Name = "nodeIDLB3";
			this.nodeIDLB3.Size = new System.Drawing.Size(120, 19);
			this.nodeIDLB3.TabIndex = 198;
			// 
			// nodeIDLB1
			// 
			this.nodeIDLB1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.nodeIDLB1.ItemHeight = 15;
			this.nodeIDLB1.Location = new System.Drawing.Point(128, 96);
			this.nodeIDLB1.Name = "nodeIDLB1";
			this.nodeIDLB1.Size = new System.Drawing.Size(120, 19);
			this.nodeIDLB1.TabIndex = 197;
			// 
			// EDS5
			// 
			this.EDS5.Location = new System.Drawing.Point(128, 352);
			this.EDS5.Name = "EDS5";
			this.EDS5.Size = new System.Drawing.Size(120, 23);
			this.EDS5.TabIndex = 196;
			// 
			// EDS6
			// 
			this.EDS6.Location = new System.Drawing.Point(248, 352);
			this.EDS6.Name = "EDS6";
			this.EDS6.Size = new System.Drawing.Size(120, 23);
			this.EDS6.TabIndex = 195;
			// 
			// EDS8
			// 
			this.EDS8.Location = new System.Drawing.Point(488, 352);
			this.EDS8.Name = "EDS8";
			this.EDS8.Size = new System.Drawing.Size(120, 23);
			this.EDS8.TabIndex = 194;
			// 
			// func8
			// 
			this.func8.Location = new System.Drawing.Point(488, 376);
			this.func8.Name = "func8";
			this.func8.Size = new System.Drawing.Size(120, 23);
			this.func8.TabIndex = 193;
			// 
			// EDS2
			// 
			this.EDS2.Location = new System.Drawing.Point(248, 152);
			this.EDS2.Name = "EDS2";
			this.EDS2.Size = new System.Drawing.Size(120, 23);
			this.EDS2.TabIndex = 192;
			// 
			// EDS3
			// 
			this.EDS3.Location = new System.Drawing.Point(368, 152);
			this.EDS3.Name = "EDS3";
			this.EDS3.Size = new System.Drawing.Size(120, 23);
			this.EDS3.TabIndex = 191;
			// 
			// EDS4
			// 
			this.EDS4.Location = new System.Drawing.Point(488, 152);
			this.EDS4.Name = "EDS4";
			this.EDS4.Size = new System.Drawing.Size(120, 23);
			this.EDS4.TabIndex = 190;
			// 
			// EDS1
			// 
			this.EDS1.Location = new System.Drawing.Point(128, 152);
			this.EDS1.Name = "EDS1";
			this.EDS1.Size = new System.Drawing.Size(120, 23);
			this.EDS1.TabIndex = 189;
			// 
			// label23
			// 
			this.label23.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label23.Location = new System.Drawing.Point(8, 352);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(120, 20);
			this.label23.TabIndex = 188;
			this.label23.Text = "EDS filename";
			this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label24
			// 
			this.label24.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label24.Location = new System.Drawing.Point(8, 328);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(120, 20);
			this.label24.TabIndex = 187;
			this.label24.Text = "master/slave";
			this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label25
			// 
			this.label25.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label25.Location = new System.Drawing.Point(8, 304);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(120, 20);
			this.label25.TabIndex = 186;
			this.label25.Text = "Node ID";
			this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label14
			// 
			this.label14.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label14.Location = new System.Drawing.Point(488, 272);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(120, 20);
			this.label14.TabIndex = 185;
			this.label14.Text = "8th node";
			// 
			// label15
			// 
			this.label15.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label15.Location = new System.Drawing.Point(368, 272);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(120, 20);
			this.label15.TabIndex = 184;
			this.label15.Text = "7th node";
			// 
			// label16
			// 
			this.label16.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label16.Location = new System.Drawing.Point(248, 272);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(120, 20);
			this.label16.TabIndex = 183;
			this.label16.Text = "6th node";
			// 
			// label17
			// 
			this.label17.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label17.Location = new System.Drawing.Point(128, 272);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(120, 20);
			this.label17.TabIndex = 182;
			this.label17.Text = "5th node";
			// 
			// label12
			// 
			this.label12.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label12.Location = new System.Drawing.Point(488, 64);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(120, 20);
			this.label12.TabIndex = 181;
			this.label12.Text = "4th node";
			// 
			// label13
			// 
			this.label13.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label13.Location = new System.Drawing.Point(368, 64);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(120, 20);
			this.label13.TabIndex = 180;
			this.label13.Text = "3rd node";
			// 
			// label11
			// 
			this.label11.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label11.Location = new System.Drawing.Point(248, 64);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(120, 20);
			this.label11.TabIndex = 179;
			this.label11.Text = "2nd node";
			// 
			// label10
			// 
			this.label10.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label10.Location = new System.Drawing.Point(128, 64);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(120, 20);
			this.label10.TabIndex = 178;
			this.label10.Text = "1st node";
			// 
			// label8
			// 
			this.label8.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label8.Location = new System.Drawing.Point(8, 176);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(120, 20);
			this.label8.TabIndex = 177;
			this.label8.Text = "Vendor ID";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label6.Location = new System.Drawing.Point(8, 200);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(120, 20);
			this.label6.TabIndex = 176;
			this.label6.Text = "product code";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.Location = new System.Drawing.Point(8, 152);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(120, 20);
			this.label5.TabIndex = 175;
			this.label5.Text = "EDS filename";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(8, 120);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(120, 20);
			this.label3.TabIndex = 174;
			this.label3.Text = "master/slave";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(8, 96);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 20);
			this.label2.TabIndex = 173;
			this.label2.Text = "Node ID";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(16, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(464, 24);
			this.label1.TabIndex = 172;
			this.label1.Text = "Setup up the virtual CAN nodes";
			// 
			// ms4
			// 
			this.ms4.Location = new System.Drawing.Point(488, 120);
			this.ms4.Name = "ms4";
			this.ms4.Size = new System.Drawing.Size(120, 20);
			this.ms4.TabIndex = 171;
			this.ms4.Text = "Master?";
			// 
			// manf4
			// 
			this.manf4.Location = new System.Drawing.Point(488, 200);
			this.manf4.Name = "manf4";
			this.manf4.Size = new System.Drawing.Size(120, 23);
			this.manf4.TabIndex = 170;
			// 
			// func4
			// 
			this.func4.Location = new System.Drawing.Point(488, 176);
			this.func4.Name = "func4";
			this.func4.Size = new System.Drawing.Size(120, 23);
			this.func4.TabIndex = 169;
			// 
			// ms3
			// 
			this.ms3.Location = new System.Drawing.Point(368, 120);
			this.ms3.Name = "ms3";
			this.ms3.Size = new System.Drawing.Size(120, 20);
			this.ms3.TabIndex = 168;
			this.ms3.Text = "Master?";
			// 
			// manf3
			// 
			this.manf3.Location = new System.Drawing.Point(368, 200);
			this.manf3.Name = "manf3";
			this.manf3.Size = new System.Drawing.Size(120, 23);
			this.manf3.TabIndex = 167;
			// 
			// func3
			// 
			this.func3.Location = new System.Drawing.Point(368, 176);
			this.func3.Name = "func3";
			this.func3.Size = new System.Drawing.Size(120, 23);
			this.func3.TabIndex = 166;
			// 
			// ms2
			// 
			this.ms2.Location = new System.Drawing.Point(248, 120);
			this.ms2.Name = "ms2";
			this.ms2.Size = new System.Drawing.Size(120, 20);
			this.ms2.TabIndex = 165;
			this.ms2.Text = "Master?";
			// 
			// manf2
			// 
			this.manf2.Location = new System.Drawing.Point(248, 200);
			this.manf2.Name = "manf2";
			this.manf2.Size = new System.Drawing.Size(120, 23);
			this.manf2.TabIndex = 164;
			// 
			// func2
			// 
			this.func2.Location = new System.Drawing.Point(248, 176);
			this.func2.Name = "func2";
			this.func2.Size = new System.Drawing.Size(120, 23);
			this.func2.TabIndex = 163;
			// 
			// ms1
			// 
			this.ms1.Checked = true;
			this.ms1.Location = new System.Drawing.Point(128, 120);
			this.ms1.Name = "ms1";
			this.ms1.Size = new System.Drawing.Size(120, 20);
			this.ms1.TabIndex = 162;
			this.ms1.TabStop = true;
			this.ms1.Text = "Master?";
			// 
			// manf1
			// 
			this.manf1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.manf1.ItemHeight = 15;
			this.manf1.Location = new System.Drawing.Point(128, 200);
			this.manf1.Name = "manf1";
			this.manf1.Size = new System.Drawing.Size(120, 23);
			this.manf1.TabIndex = 161;
			// 
			// func1
			// 
			this.func1.Location = new System.Drawing.Point(128, 176);
			this.func1.Name = "func1";
			this.func1.Size = new System.Drawing.Size(120, 23);
			this.func1.TabIndex = 160;
			// 
			// ms8
			// 
			this.ms8.Location = new System.Drawing.Point(488, 328);
			this.ms8.Name = "ms8";
			this.ms8.Size = new System.Drawing.Size(120, 20);
			this.ms8.TabIndex = 159;
			this.ms8.Text = "Master?";
			// 
			// manf8
			// 
			this.manf8.Location = new System.Drawing.Point(488, 400);
			this.manf8.Name = "manf8";
			this.manf8.Size = new System.Drawing.Size(120, 23);
			this.manf8.TabIndex = 158;
			// 
			// EDS7
			// 
			this.EDS7.Location = new System.Drawing.Point(368, 352);
			this.EDS7.Name = "EDS7";
			this.EDS7.Size = new System.Drawing.Size(120, 23);
			this.EDS7.TabIndex = 157;
			// 
			// ms7
			// 
			this.ms7.Location = new System.Drawing.Point(368, 328);
			this.ms7.Name = "ms7";
			this.ms7.Size = new System.Drawing.Size(120, 20);
			this.ms7.TabIndex = 156;
			this.ms7.Text = "Master?";
			// 
			// manf7
			// 
			this.manf7.Location = new System.Drawing.Point(368, 400);
			this.manf7.Name = "manf7";
			this.manf7.Size = new System.Drawing.Size(120, 23);
			this.manf7.TabIndex = 155;
			// 
			// func7
			// 
			this.func7.Location = new System.Drawing.Point(368, 376);
			this.func7.Name = "func7";
			this.func7.Size = new System.Drawing.Size(120, 23);
			this.func7.TabIndex = 154;
			// 
			// ms6
			// 
			this.ms6.Location = new System.Drawing.Point(248, 328);
			this.ms6.Name = "ms6";
			this.ms6.Size = new System.Drawing.Size(120, 20);
			this.ms6.TabIndex = 153;
			this.ms6.Text = "Master?";
			// 
			// manf6
			// 
			this.manf6.Location = new System.Drawing.Point(248, 400);
			this.manf6.Name = "manf6";
			this.manf6.Size = new System.Drawing.Size(120, 23);
			this.manf6.TabIndex = 152;
			// 
			// func6
			// 
			this.func6.Location = new System.Drawing.Point(248, 376);
			this.func6.Name = "func6";
			this.func6.Size = new System.Drawing.Size(120, 23);
			this.func6.TabIndex = 151;
			// 
			// ms5
			// 
			this.ms5.Location = new System.Drawing.Point(128, 328);
			this.ms5.Name = "ms5";
			this.ms5.Size = new System.Drawing.Size(120, 20);
			this.ms5.TabIndex = 150;
			this.ms5.Text = "Master?";
			// 
			// manf5
			// 
			this.manf5.Location = new System.Drawing.Point(128, 400);
			this.manf5.Name = "manf5";
			this.manf5.Size = new System.Drawing.Size(120, 23);
			this.manf5.TabIndex = 149;
			// 
			// func5
			// 
			this.func5.Location = new System.Drawing.Point(128, 376);
			this.func5.Name = "func5";
			this.func5.Size = new System.Drawing.Size(120, 23);
			this.func5.TabIndex = 148;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label20);
			this.groupBox2.Controls.Add(this.profileNameTB);
			this.groupBox2.Controls.Add(this.passwordTB);
			this.groupBox2.Controls.Add(this.label21);
			this.groupBox2.Controls.Add(this.label22);
			this.groupBox2.Controls.Add(this.UserIdTB);
			this.groupBox2.Controls.Add(this.label19);
			this.groupBox2.Controls.Add(this.comboBox9);
			this.groupBox2.Location = new System.Drawing.Point(8, 24);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(672, 120);
			this.groupBox2.TabIndex = 149;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "System";
			// 
			// comboBox9
			// 
			this.comboBox9.Location = new System.Drawing.Point(136, 24);
			this.comboBox9.Name = "comboBox9";
			this.comboBox9.Size = new System.Drawing.Size(120, 23);
			this.comboBox9.TabIndex = 190;
			// 
			// label19
			// 
			this.label19.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label19.Location = new System.Drawing.Point(8, 24);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(120, 20);
			this.label19.TabIndex = 191;
			this.label19.Text = "Baud rate";
			this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label20
			// 
			this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label20.Location = new System.Drawing.Point(48, 56);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(104, 16);
			this.label20.TabIndex = 198;
			this.label20.Text = "Profile name";
			// 
			// profileNameTB
			// 
			this.profileNameTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.profileNameTB.Location = new System.Drawing.Point(160, 56);
			this.profileNameTB.Name = "profileNameTB";
			this.profileNameTB.Size = new System.Drawing.Size(280, 20);
			this.profileNameTB.TabIndex = 197;
			this.profileNameTB.Text = "";
			// 
			// passwordTB
			// 
			this.passwordTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.passwordTB.Location = new System.Drawing.Point(440, 88);
			this.passwordTB.MaxLength = 5;
			this.passwordTB.Name = "passwordTB";
			this.passwordTB.Size = new System.Drawing.Size(136, 20);
			this.passwordTB.TabIndex = 196;
			this.passwordTB.Text = "";
			// 
			// label21
			// 
			this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label21.Location = new System.Drawing.Point(328, 88);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(104, 16);
			this.label21.TabIndex = 195;
			this.label21.Text = "Passcode";
			// 
			// label22
			// 
			this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label22.Location = new System.Drawing.Point(56, 88);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(96, 16);
			this.label22.TabIndex = 194;
			this.label22.Text = "User ID";
			// 
			// UserIdTB
			// 
			this.UserIdTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.UserIdTB.Location = new System.Drawing.Point(176, 88);
			this.UserIdTB.MaxLength = 5;
			this.UserIdTB.Name = "UserIdTB";
			this.UserIdTB.Size = new System.Drawing.Size(136, 20);
			this.UserIdTB.TabIndex = 193;
			this.UserIdTB.Text = "";
			// 
			// GUI_FORCE_VALUES
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(698, 655);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.groupBox1);
			this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GUI_FORCE_VALUES";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "GUI_FORCE_VALUES";
			this.Load += new System.EventHandler(this.GUI_FORCE_VALUES_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
		public GUI_FORCE_VALUES( )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			fillGUIfromvirtualNodesXML();
			setUpControlArrays();
			for(int i = 0;i<SCCorpStyle.maxConnectedDevices;i++)
			{
				nodeIDs[i].Text = ""; //default value
			}
		}

		private void GUI_FORCE_VALUES_Load(object sender, System.EventArgs e)
		{
		}

		private void fillGUIfromvirtualNodesXML()
		{
			VehicleProfile profile = null;
			for(int i = 0;i<MAIN_WINDOW.DWConfigFile.vehicleprofiles.Count;i++)
			{
				profile = (VehicleProfile) MAIN_WINDOW.DWConfigFile.vehicleprofiles[i];
				if (profile.ProfileName == "virtualNodes.xml")
				{
					break;
				}
				else
				{
					profile = null;
				}
			}
			if(profile != null)
			{
				//fill the GUI
				profile.readStoredProfile(); //get all other info about this profile
			}
		}

		private void setUpControlArrays( ) 
		{
			//add node text boxes to their array
			nodeIDs[0] = this.nodeIDLB1;
			nodeIDs[1] = this.nodeIDLB2;
			nodeIDs[2] = this.nodeIDLB3;
			nodeIDs[3] = this.nodeIDLB4;
			nodeIDs[4] = this.nodeIDLB5;
			nodeIDs[5] = this.nodeIDLB6;
			nodeIDs[6] = this.nodeIDLB7;
			nodeIDs[7] = this.nodeIDLB8;

			string [] allIds = new string[128];
			allIds[0] = "none";
			for(int i = 1;i<128;i++)
			{
				allIds[i] = i.ToString();
			}
			for(int j = 0;j<nodeIDs.Length;j++)
			{
				nodeIDs[j].DataSource = allIds;
			}
			//nor master/slave radio buttons
			masterslave[0] = ms1;
			masterslave[1] = ms2;
			masterslave[2] = ms3;
			masterslave[3] = ms4;
			masterslave[4] = ms5;
			masterslave[5] = ms6;
			masterslave[6] = ms7;
			masterslave[7] = ms8;
			
			EDSs[0] = EDS1;
			EDSs[1] = EDS2;
			EDSs[2] = EDS3;
			EDSs[3] = EDS4;
			EDSs[4] = EDS5;
			EDSs[5] = EDS6;
			EDSs[6] = EDS7;
			EDSs[7] = EDS8;
		
			function[0] = func1;
			function[1] = func2;
			function[2] = func3;
			function[3] = func4;
			function[4] = func5;
			function[5] = func6;
			function[6] = func7;
			function[7] = func8;

			manuf[0] = manf1;
			manuf[1] = manf2;
			manuf[2] = manf3;
			manuf[3] = manf4;
			manuf[4] = manf5;
			manuf[5] = manf6;
			manuf[6] = manf7;
			manuf[7] = manf8;
			
			
			for(int i=0; i<SCCorpStyle.maxConnectedDevices;i++)
			{
				manuf[i].Items.AddRange(manufacturers);
				function[i].Items.AddRange(deviceTypes);
			}
		
		}

		#endregion

		#region user interaction zone
		private void submitBtn_Click(object sender, System.EventArgs e)
		{
#if !REALCONTROLLER
			uint noOfNodes = 0;

			for ( int i = 0; i <= 7; i++ )
			{
				if ( nodeIDs[i].Text != "" ) //this node is fitted
				{
					noOfNodes++;
				}
			}

//			localSystemInfo.noOfNodes = noOfNodes;
//			localSystemInfo.nodes = new nodeInfo[ noOfNodes ];
			
			noOfNodes = 0;

			for ( int i = 0; i <= 7; i++ )
			{
				if ( nodeIDs[i].Text != "" ) //this node is fitted
				{
//					localSystemInfo.nodes[ i ] = new nodeInfo();
//					localSystemInfo.nodes[ noOfNodes ].nodeID = Int16.Parse(nodeIDs[i].Text); 
//					localSystemInfo.nodes[ noOfNodes ].masterStatus = masterslave[i].Checked;
//					localSystemInfo.nodes[ noOfNodes ].EDSfilename = EDSs[i].Text;
//					localSystemInfo.nodes[ noOfNodes ].commsOK = CANComms[i].Checked;

					if ( function[i].SelectedItem != null )
					{
//						localSystemInfo.nodes[ noOfNodes ].deviceType = function[i].SelectedItem.ToString();
					}
					
					if ( manuf[i].Text != "" )
					{
//						localSystemInfo.nodes[ noOfNodes ].manufacturer = manuf[i].Text;
					}
					
					noOfNodes++;
				}
			}

			this.Close();
#endif
		} //end evemt handler

		#endregion

		#region finalisation/exit

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

		private void label2_Click(object sender, System.EventArgs e)
		{
		

		}


	}
}
