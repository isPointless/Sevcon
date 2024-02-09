/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.5$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:28:28$
	$ModDate:05/12/2007 22:27:18$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  64401: EditProfile.cs 

   Rev 1.5    05/12/2007 22:28:28  ak
 TC keywords added to source for version control.



*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace DriveWizard
{
	/// <summary>
	/// Summary description for EditProfile.
	/// </summary>
	//public enum TabPages {GENERAL,  OPEN_LOOP, CLOSED_LOOP, NO_LOAD_TEST, POWER_FRAME};
	public class EditProfile : System.Windows.Forms.Form
	{
		#region form controls declarations
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage General;
		private System.Windows.Forms.TabPage Openloop;
		private System.Windows.Forms.TabPage NoLoad;
		private System.Windows.Forms.TabPage ClosedLoop;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.MenuItem MIopen;
		private System.Windows.Forms.MenuItem MISave;
		private System.Windows.Forms.NumericUpDown NUD_SpeedFactor;
		private System.Windows.Forms.NumericUpDown NUD_torquefactor;
		private System.Windows.Forms.NumericUpDown NUD_PowerFactor;
		private System.Windows.Forms.NumericUpDown NUD_OL_nmuTImesToApplyTestPts;
		private System.Windows.Forms.NumericUpDown NUD_OL_testIterations;
		private System.Windows.Forms.NumericUpDown NUD_OL_percentOverCurrent;
		private System.Windows.Forms.TreeView TV_OL_testpoints;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.NumericUpDown NUD_PF_maxV;
		private System.Windows.Forms.NumericUpDown NUD_PF_maxIs;
		private System.Windows.Forms.NumericUpDown NUD_PF_nomV;
		private System.Windows.Forms.NumericUpDown NUD_PF_minV;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.NumericUpDown NUD_NLT_numSamples;
		private System.Windows.Forms.TextBox TB_NLT_W_rate;
		private System.Windows.Forms.NumericUpDown NUD_NLT_numsamplesSteadyState;
		private System.Windows.Forms.NumericUpDown NUD_NLT_numsamplesFlux;
		private System.Windows.Forms.TextBox TB_NLT_maxIdToIqRatio;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TreeView TV_NLT_testPts;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.TreeView TV_CL_TestPoints;
		private System.Windows.Forms.MenuItem MI_useDefaults;
		private System.Windows.Forms.TabPage PowerFrame;
		private System.Windows.Forms.TabPage scwizProfiles;
		private System.Windows.Forms.Panel PnlTestFiles;
		private System.Windows.Forms.Label Lbl_PowerFrameProfile;
		private System.Windows.Forms.Label Lbl_TestProfile;
		private System.Windows.Forms.Button Btn_TestProfile;
		private System.Windows.Forms.Button Btn_PwrFrameProfile;
		private System.Windows.Forms.Label LblPowerFrameProfile;
		private System.Windows.Forms.Label LblTestProfile;
		private System.Windows.Forms.ContextMenu CM_OLoop_and_CLoop;
		#endregion form controls declarations

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.General = new System.Windows.Forms.TabPage();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.NUD_torquefactor = new System.Windows.Forms.NumericUpDown();
			this.NUD_PowerFactor = new System.Windows.Forms.NumericUpDown();
			this.NUD_SpeedFactor = new System.Windows.Forms.NumericUpDown();
			this.Openloop = new System.Windows.Forms.TabPage();
			this.NUD_OL_nmuTImesToApplyTestPts = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.TV_OL_testpoints = new System.Windows.Forms.TreeView();
			this.NUD_OL_testIterations = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.NUD_OL_percentOverCurrent = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.ClosedLoop = new System.Windows.Forms.TabPage();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.TV_CL_TestPoints = new System.Windows.Forms.TreeView();
			this.NoLoad = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.TV_NLT_testPts = new System.Windows.Forms.TreeView();
			this.TB_NLT_maxIdToIqRatio = new System.Windows.Forms.TextBox();
			this.NUD_NLT_numsamplesFlux = new System.Windows.Forms.NumericUpDown();
			this.NUD_NLT_numsamplesSteadyState = new System.Windows.Forms.NumericUpDown();
			this.TB_NLT_W_rate = new System.Windows.Forms.TextBox();
			this.NUD_NLT_numSamples = new System.Windows.Forms.NumericUpDown();
			this.label16 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.PowerFrame = new System.Windows.Forms.TabPage();
			this.NUD_PF_minV = new System.Windows.Forms.NumericUpDown();
			this.NUD_PF_nomV = new System.Windows.Forms.NumericUpDown();
			this.NUD_PF_maxIs = new System.Windows.Forms.NumericUpDown();
			this.NUD_PF_maxV = new System.Windows.Forms.NumericUpDown();
			this.label11 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.MIopen = new System.Windows.Forms.MenuItem();
			this.MISave = new System.Windows.Forms.MenuItem();
			this.MI_useDefaults = new System.Windows.Forms.MenuItem();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.CM_OLoop_and_CLoop = new System.Windows.Forms.ContextMenu();
			this.CM_NLT = new System.Windows.Forms.ContextMenu();
			this.scwizProfiles = new System.Windows.Forms.TabPage();
			this.PnlTestFiles = new System.Windows.Forms.Panel();
			this.Lbl_PowerFrameProfile = new System.Windows.Forms.Label();
			this.Lbl_TestProfile = new System.Windows.Forms.Label();
			this.Btn_TestProfile = new System.Windows.Forms.Button();
			this.Btn_PwrFrameProfile = new System.Windows.Forms.Button();
			this.LblPowerFrameProfile = new System.Windows.Forms.Label();
			this.LblTestProfile = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.General.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NUD_torquefactor)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_PowerFactor)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_SpeedFactor)).BeginInit();
			this.Openloop.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NUD_OL_nmuTImesToApplyTestPts)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NUD_OL_testIterations)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_OL_percentOverCurrent)).BeginInit();
			this.ClosedLoop.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.NoLoad.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NUD_NLT_numsamplesFlux)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_NLT_numsamplesSteadyState)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_NLT_numSamples)).BeginInit();
			this.PowerFrame.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NUD_PF_minV)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_PF_nomV)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_PF_maxIs)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_PF_maxV)).BeginInit();
			this.scwizProfiles.SuspendLayout();
			this.PnlTestFiles.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.General);
			this.tabControl1.Controls.Add(this.Openloop);
			this.tabControl1.Controls.Add(this.ClosedLoop);
			this.tabControl1.Controls.Add(this.NoLoad);
			this.tabControl1.Controls.Add(this.PowerFrame);
			this.tabControl1.Controls.Add(this.scwizProfiles);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(608, 421);
			this.tabControl1.TabIndex = 0;
			// 
			// General
			// 
			this.General.Controls.Add(this.label3);
			this.General.Controls.Add(this.label2);
			this.General.Controls.Add(this.label1);
			this.General.Controls.Add(this.NUD_torquefactor);
			this.General.Controls.Add(this.NUD_PowerFactor);
			this.General.Controls.Add(this.NUD_SpeedFactor);
			this.General.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.General.Location = new System.Drawing.Point(4, 25);
			this.General.Name = "General";
			this.General.Size = new System.Drawing.Size(600, 392);
			this.General.TabIndex = 0;
			this.General.Text = "General";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(200, 24);
			this.label3.TabIndex = 5;
			this.label3.Text = "Power Factor";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(200, 24);
			this.label2.TabIndex = 4;
			this.label2.Text = "Torque Factor";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(200, 24);
			this.label1.TabIndex = 3;
			this.label1.Text = "Speed Factor";
			// 
			// NUD_torquefactor
			// 
			this.NUD_torquefactor.Location = new System.Drawing.Point(248, 56);
			this.NUD_torquefactor.Name = "NUD_torquefactor";
			this.NUD_torquefactor.Size = new System.Drawing.Size(48, 22);
			this.NUD_torquefactor.TabIndex = 2;
			// 
			// NUD_PowerFactor
			// 
			this.NUD_PowerFactor.Location = new System.Drawing.Point(248, 88);
			this.NUD_PowerFactor.Name = "NUD_PowerFactor";
			this.NUD_PowerFactor.Size = new System.Drawing.Size(48, 22);
			this.NUD_PowerFactor.TabIndex = 1;
			// 
			// NUD_SpeedFactor
			// 
			this.NUD_SpeedFactor.Location = new System.Drawing.Point(248, 24);
			this.NUD_SpeedFactor.Name = "NUD_SpeedFactor";
			this.NUD_SpeedFactor.Size = new System.Drawing.Size(48, 22);
			this.NUD_SpeedFactor.TabIndex = 0;
			// 
			// Openloop
			// 
			this.Openloop.Controls.Add(this.NUD_OL_nmuTImesToApplyTestPts);
			this.Openloop.Controls.Add(this.label7);
			this.Openloop.Controls.Add(this.groupBox1);
			this.Openloop.Controls.Add(this.NUD_OL_testIterations);
			this.Openloop.Controls.Add(this.label6);
			this.Openloop.Controls.Add(this.label5);
			this.Openloop.Controls.Add(this.NUD_OL_percentOverCurrent);
			this.Openloop.Controls.Add(this.label4);
			this.Openloop.Location = new System.Drawing.Point(4, 25);
			this.Openloop.Name = "Openloop";
			this.Openloop.Size = new System.Drawing.Size(600, 392);
			this.Openloop.TabIndex = 1;
			this.Openloop.Text = "Open Loop";
			// 
			// NUD_OL_nmuTImesToApplyTestPts
			// 
			this.NUD_OL_nmuTImesToApplyTestPts.Location = new System.Drawing.Point(208, 104);
			this.NUD_OL_nmuTImesToApplyTestPts.Name = "NUD_OL_nmuTImesToApplyTestPts";
			this.NUD_OL_nmuTImesToApplyTestPts.Size = new System.Drawing.Size(56, 22);
			this.NUD_OL_nmuTImesToApplyTestPts.TabIndex = 9;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(8, 104);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(200, 24);
			this.label7.TabIndex = 8;
			this.label7.Text = "No of times to apply Test Points";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.TV_OL_testpoints);
			this.groupBox1.Location = new System.Drawing.Point(296, 24);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(304, 352);
			this.groupBox1.TabIndex = 5;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Test Points (Right Click to Edit)";
			// 
			// TV_OL_testpoints
			// 
			this.TV_OL_testpoints.ImageIndex = -1;
			this.TV_OL_testpoints.Location = new System.Drawing.Point(8, 24);
			this.TV_OL_testpoints.Name = "TV_OL_testpoints";
			this.TV_OL_testpoints.SelectedImageIndex = -1;
			this.TV_OL_testpoints.Size = new System.Drawing.Size(288, 320);
			this.TV_OL_testpoints.TabIndex = 5;
			this.TV_OL_testpoints.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
			// 
			// NUD_OL_testIterations
			// 
			this.NUD_OL_testIterations.Location = new System.Drawing.Point(208, 64);
			this.NUD_OL_testIterations.Name = "NUD_OL_testIterations";
			this.NUD_OL_testIterations.Size = new System.Drawing.Size(56, 22);
			this.NUD_OL_testIterations.TabIndex = 4;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 64);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(200, 24);
			this.label6.TabIndex = 3;
			this.label6.Text = "Number of OL test iterations";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(272, 24);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(32, 23);
			this.label5.TabIndex = 2;
			this.label5.Text = "%";
			// 
			// NUD_OL_percentOverCurrent
			// 
			this.NUD_OL_percentOverCurrent.Location = new System.Drawing.Point(208, 24);
			this.NUD_OL_percentOverCurrent.Name = "NUD_OL_percentOverCurrent";
			this.NUD_OL_percentOverCurrent.Size = new System.Drawing.Size(56, 22);
			this.NUD_OL_percentOverCurrent.TabIndex = 1;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(184, 24);
			this.label4.TabIndex = 0;
			this.label4.Text = "Max percantage overcurrent";
			// 
			// ClosedLoop
			// 
			this.ClosedLoop.Controls.Add(this.groupBox3);
			this.ClosedLoop.Location = new System.Drawing.Point(4, 25);
			this.ClosedLoop.Name = "ClosedLoop";
			this.ClosedLoop.Size = new System.Drawing.Size(600, 392);
			this.ClosedLoop.TabIndex = 3;
			this.ClosedLoop.Text = "Closed Loop";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.TV_CL_TestPoints);
			this.groupBox3.Location = new System.Drawing.Point(16, 32);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(464, 320);
			this.groupBox3.TabIndex = 4;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Tesp Points ( Right click to edit)";
			// 
			// TV_CL_TestPoints
			// 
			this.TV_CL_TestPoints.ImageIndex = -1;
			this.TV_CL_TestPoints.Location = new System.Drawing.Point(8, 24);
			this.TV_CL_TestPoints.Name = "TV_CL_TestPoints";
			this.TV_CL_TestPoints.SelectedImageIndex = -1;
			this.TV_CL_TestPoints.Size = new System.Drawing.Size(448, 288);
			this.TV_CL_TestPoints.TabIndex = 4;
			this.TV_CL_TestPoints.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
			// 
			// NoLoad
			// 
			this.NoLoad.Controls.Add(this.groupBox2);
			this.NoLoad.Controls.Add(this.TB_NLT_maxIdToIqRatio);
			this.NoLoad.Controls.Add(this.NUD_NLT_numsamplesFlux);
			this.NoLoad.Controls.Add(this.NUD_NLT_numsamplesSteadyState);
			this.NoLoad.Controls.Add(this.TB_NLT_W_rate);
			this.NoLoad.Controls.Add(this.NUD_NLT_numSamples);
			this.NoLoad.Controls.Add(this.label16);
			this.NoLoad.Controls.Add(this.label15);
			this.NoLoad.Controls.Add(this.label14);
			this.NoLoad.Controls.Add(this.label13);
			this.NoLoad.Controls.Add(this.label12);
			this.NoLoad.Location = new System.Drawing.Point(4, 25);
			this.NoLoad.Name = "NoLoad";
			this.NoLoad.Size = new System.Drawing.Size(600, 392);
			this.NoLoad.TabIndex = 2;
			this.NoLoad.Text = "No Load Test";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.TV_NLT_testPts);
			this.groupBox2.Location = new System.Drawing.Point(344, 16);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(248, 360);
			this.groupBox2.TabIndex = 11;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Speed Points (Right click to edit)";
			// 
			// TV_NLT_testPts
			// 
			this.TV_NLT_testPts.ImageIndex = -1;
			this.TV_NLT_testPts.Location = new System.Drawing.Point(16, 32);
			this.TV_NLT_testPts.Name = "TV_NLT_testPts";
			this.TV_NLT_testPts.SelectedImageIndex = -1;
			this.TV_NLT_testPts.Size = new System.Drawing.Size(224, 312);
			this.TV_NLT_testPts.TabIndex = 0;
			this.TV_NLT_testPts.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
			// 
			// TB_NLT_maxIdToIqRatio
			// 
			this.TB_NLT_maxIdToIqRatio.Location = new System.Drawing.Point(240, 168);
			this.TB_NLT_maxIdToIqRatio.Name = "TB_NLT_maxIdToIqRatio";
			this.TB_NLT_maxIdToIqRatio.Size = new System.Drawing.Size(96, 22);
			this.TB_NLT_maxIdToIqRatio.TabIndex = 10;
			this.TB_NLT_maxIdToIqRatio.Text = "";
			// 
			// NUD_NLT_numsamplesFlux
			// 
			this.NUD_NLT_numsamplesFlux.Location = new System.Drawing.Point(240, 104);
			this.NUD_NLT_numsamplesFlux.Maximum = new System.Decimal(new int[] {
																				   10000,
																				   0,
																				   0,
																				   0});
			this.NUD_NLT_numsamplesFlux.Name = "NUD_NLT_numsamplesFlux";
			this.NUD_NLT_numsamplesFlux.Size = new System.Drawing.Size(96, 22);
			this.NUD_NLT_numsamplesFlux.TabIndex = 9;
			// 
			// NUD_NLT_numsamplesSteadyState
			// 
			this.NUD_NLT_numsamplesSteadyState.Location = new System.Drawing.Point(240, 136);
			this.NUD_NLT_numsamplesSteadyState.Maximum = new System.Decimal(new int[] {
																						  10000,
																						  0,
																						  0,
																						  0});
			this.NUD_NLT_numsamplesSteadyState.Name = "NUD_NLT_numsamplesSteadyState";
			this.NUD_NLT_numsamplesSteadyState.Size = new System.Drawing.Size(96, 22);
			this.NUD_NLT_numsamplesSteadyState.TabIndex = 8;
			// 
			// TB_NLT_W_rate
			// 
			this.TB_NLT_W_rate.AcceptsReturn = true;
			this.TB_NLT_W_rate.AcceptsTab = true;
			this.TB_NLT_W_rate.Location = new System.Drawing.Point(240, 40);
			this.TB_NLT_W_rate.MaxLength = 10;
			this.TB_NLT_W_rate.Name = "TB_NLT_W_rate";
			this.TB_NLT_W_rate.Size = new System.Drawing.Size(96, 22);
			this.TB_NLT_W_rate.TabIndex = 6;
			this.TB_NLT_W_rate.Text = "";
			// 
			// NUD_NLT_numSamples
			// 
			this.NUD_NLT_numSamples.Location = new System.Drawing.Point(240, 72);
			this.NUD_NLT_numSamples.Maximum = new System.Decimal(new int[] {
																			   10000,
																			   0,
																			   0,
																			   0});
			this.NUD_NLT_numSamples.Name = "NUD_NLT_numSamples";
			this.NUD_NLT_numSamples.Size = new System.Drawing.Size(96, 22);
			this.NUD_NLT_numSamples.TabIndex = 5;
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(16, 168);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(122, 18);
			this.label16.TabIndex = 4;
			this.label16.Text = "Maximum Id/Iq ratio";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(16, 136);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(218, 18);
			this.label15.TabIndex = 3;
			this.label15.Text = "Num samples for steady state settle";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(16, 104);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(214, 18);
			this.label14.TabIndex = 2;
			this.label14.Text = "Num samples for flux change settle";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(16, 72);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(121, 18);
			this.label13.TabIndex = 1;
			this.label13.Text = "Number of samples";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(16, 40);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(45, 18);
			this.label12.TabIndex = 0;
			this.label12.Text = "w_rate";
			// 
			// PowerFrame
			// 
			this.PowerFrame.Controls.Add(this.NUD_PF_minV);
			this.PowerFrame.Controls.Add(this.NUD_PF_nomV);
			this.PowerFrame.Controls.Add(this.NUD_PF_maxIs);
			this.PowerFrame.Controls.Add(this.NUD_PF_maxV);
			this.PowerFrame.Controls.Add(this.label11);
			this.PowerFrame.Controls.Add(this.label10);
			this.PowerFrame.Controls.Add(this.label9);
			this.PowerFrame.Controls.Add(this.label8);
			this.PowerFrame.Location = new System.Drawing.Point(4, 25);
			this.PowerFrame.Name = "PowerFrame";
			this.PowerFrame.Size = new System.Drawing.Size(600, 392);
			this.PowerFrame.TabIndex = 4;
			this.PowerFrame.Text = "PowerFrame";
			// 
			// NUD_PF_minV
			// 
			this.NUD_PF_minV.Location = new System.Drawing.Point(240, 80);
			this.NUD_PF_minV.Maximum = new System.Decimal(new int[] {
																		200,
																		0,
																		0,
																		0});
			this.NUD_PF_minV.Name = "NUD_PF_minV";
			this.NUD_PF_minV.Size = new System.Drawing.Size(56, 22);
			this.NUD_PF_minV.TabIndex = 7;
			// 
			// NUD_PF_nomV
			// 
			this.NUD_PF_nomV.Location = new System.Drawing.Point(240, 120);
			this.NUD_PF_nomV.Maximum = new System.Decimal(new int[] {
																		200,
																		0,
																		0,
																		0});
			this.NUD_PF_nomV.Name = "NUD_PF_nomV";
			this.NUD_PF_nomV.Size = new System.Drawing.Size(56, 22);
			this.NUD_PF_nomV.TabIndex = 6;
			// 
			// NUD_PF_maxIs
			// 
			this.NUD_PF_maxIs.Location = new System.Drawing.Point(240, 160);
			this.NUD_PF_maxIs.Maximum = new System.Decimal(new int[] {
																		 1000,
																		 0,
																		 0,
																		 0});
			this.NUD_PF_maxIs.Name = "NUD_PF_maxIs";
			this.NUD_PF_maxIs.Size = new System.Drawing.Size(56, 22);
			this.NUD_PF_maxIs.TabIndex = 5;
			// 
			// NUD_PF_maxV
			// 
			this.NUD_PF_maxV.Location = new System.Drawing.Point(240, 40);
			this.NUD_PF_maxV.Maximum = new System.Decimal(new int[] {
																		200,
																		0,
																		0,
																		0});
			this.NUD_PF_maxV.Name = "NUD_PF_maxV";
			this.NUD_PF_maxV.Size = new System.Drawing.Size(56, 22);
			this.NUD_PF_maxV.TabIndex = 4;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(24, 168);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(152, 32);
			this.label11.TabIndex = 3;
			this.label11.Text = "Maximum Is current";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(24, 128);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(144, 24);
			this.label10.TabIndex = 2;
			this.label10.Text = "Nominal Battery Volts";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(24, 80);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(168, 32);
			this.label9.TabIndex = 1;
			this.label9.Text = "Maximum Battery Volts";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(24, 40);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(168, 32);
			this.label8.TabIndex = 0;
			this.label8.Text = "Minimum Battery volts";
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.MIopen,
																					  this.MISave,
																					  this.MI_useDefaults});
			this.menuItem1.Text = "File";
			// 
			// MIopen
			// 
			this.MIopen.Index = 0;
			this.MIopen.Text = "&Open";
			this.MIopen.Click += new System.EventHandler(this.MIopen_Click);
			// 
			// MISave
			// 
			this.MISave.Index = 1;
			this.MISave.Text = "&Save";
			this.MISave.Click += new System.EventHandler(this.MISave_Click);
			// 
			// MI_useDefaults
			// 
			this.MI_useDefaults.Index = 2;
			this.MI_useDefaults.Text = "Use &Defualt values";
			this.MI_useDefaults.Click += new System.EventHandler(this.MI_useDefaults_Click);
			// 
			// scwizProfiles
			// 
			this.scwizProfiles.Controls.Add(this.PnlTestFiles);
			this.scwizProfiles.Location = new System.Drawing.Point(4, 25);
			this.scwizProfiles.Name = "scwizProfiles";
			this.scwizProfiles.Size = new System.Drawing.Size(600, 392);
			this.scwizProfiles.TabIndex = 5;
			this.scwizProfiles.Text = "SCWiz Profiles";
			// 
			// PnlTestFiles
			// 
			this.PnlTestFiles.BackColor = System.Drawing.SystemColors.Control;
			this.PnlTestFiles.Controls.Add(this.Lbl_PowerFrameProfile);
			this.PnlTestFiles.Controls.Add(this.Lbl_TestProfile);
			this.PnlTestFiles.Controls.Add(this.Btn_TestProfile);
			this.PnlTestFiles.Controls.Add(this.Btn_PwrFrameProfile);
			this.PnlTestFiles.Controls.Add(this.LblPowerFrameProfile);
			this.PnlTestFiles.Controls.Add(this.LblTestProfile);
			this.PnlTestFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PnlTestFiles.Location = new System.Drawing.Point(0, 0);
			this.PnlTestFiles.Name = "PnlTestFiles";
			this.PnlTestFiles.Size = new System.Drawing.Size(600, 392);
			this.PnlTestFiles.TabIndex = 70;
			// 
			// Lbl_PowerFrameProfile
			// 
			this.Lbl_PowerFrameProfile.Location = new System.Drawing.Point(12, 208);
			this.Lbl_PowerFrameProfile.Name = "Lbl_PowerFrameProfile";
			this.Lbl_PowerFrameProfile.Size = new System.Drawing.Size(150, 19);
			this.Lbl_PowerFrameProfile.TabIndex = 5;
			this.Lbl_PowerFrameProfile.Text = "Power Frame Profile:";
			// 
			// Lbl_TestProfile
			// 
			this.Lbl_TestProfile.Location = new System.Drawing.Point(12, 38);
			this.Lbl_TestProfile.Name = "Lbl_TestProfile";
			this.Lbl_TestProfile.Size = new System.Drawing.Size(138, 19);
			this.Lbl_TestProfile.TabIndex = 4;
			this.Lbl_TestProfile.Text = "Test Profile:";
			// 
			// Btn_TestProfile
			// 
			this.Btn_TestProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Btn_TestProfile.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Btn_TestProfile.Location = new System.Drawing.Point(390, 120);
			this.Btn_TestProfile.Name = "Btn_TestProfile";
			this.Btn_TestProfile.Size = new System.Drawing.Size(192, 25);
			this.Btn_TestProfile.TabIndex = 3;
			this.Btn_TestProfile.Text = "Change Test Profile";
			// 
			// Btn_PwrFrameProfile
			// 
			this.Btn_PwrFrameProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Btn_PwrFrameProfile.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Btn_PwrFrameProfile.Location = new System.Drawing.Point(390, 291);
			this.Btn_PwrFrameProfile.Name = "Btn_PwrFrameProfile";
			this.Btn_PwrFrameProfile.Size = new System.Drawing.Size(192, 25);
			this.Btn_PwrFrameProfile.TabIndex = 2;
			this.Btn_PwrFrameProfile.Text = "Change Power Frame Profile";
			// 
			// LblPowerFrameProfile
			// 
			this.LblPowerFrameProfile.Location = new System.Drawing.Point(12, 227);
			this.LblPowerFrameProfile.Name = "LblPowerFrameProfile";
			this.LblPowerFrameProfile.Size = new System.Drawing.Size(570, 57);
			this.LblPowerFrameProfile.TabIndex = 1;
			this.LblPowerFrameProfile.Text = "not found";
			// 
			// LblTestProfile
			// 
			this.LblTestProfile.Location = new System.Drawing.Point(12, 57);
			this.LblTestProfile.Name = "LblTestProfile";
			this.LblTestProfile.Size = new System.Drawing.Size(570, 57);
			this.LblTestProfile.TabIndex = 0;
			this.LblTestProfile.Text = "not found";
			// 
			// EditProfile
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(608, 421);
			this.Controls.Add(this.tabControl1);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Menu = this.mainMenu1;
			this.Name = "EditProfile";
			this.Text = "Edit Self Characterization Test Profiles";
			this.tabControl1.ResumeLayout(false);
			this.General.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.NUD_torquefactor)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_PowerFactor)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_SpeedFactor)).EndInit();
			this.Openloop.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.NUD_OL_nmuTImesToApplyTestPts)).EndInit();
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.NUD_OL_testIterations)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_OL_percentOverCurrent)).EndInit();
			this.ClosedLoop.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.NoLoad.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.NUD_NLT_numsamplesFlux)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_NLT_numsamplesSteadyState)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_NLT_numSamples)).EndInit();
			this.PowerFrame.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.NUD_PF_minV)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_PF_nomV)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_PF_maxIs)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUD_PF_maxV)).EndInit();
			this.scwizProfiles.ResumeLayout(false);
			this.PnlTestFiles.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region my parameters
		private SCprofile scprofile;
		private System.Windows.Forms.ContextMenu CM_NLT;
		private TreeNode selectedTreeNode = null;
		#endregion my parameters

		#region Initialisation
		public EditProfile(ref SCprofile passed_scprofile)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.scprofile = passed_scprofile;
			bindControls();
			setupContextMenus();
		}

		private void bindControls()
		{
			#region general Tab
			this.NUD_SpeedFactor.DataBindings.Clear();
			this.NUD_SpeedFactor.DataBindings.Add(new Binding("Value",this.scprofile.general,"SpeedFactor"));
			this.NUD_torquefactor.DataBindings.Clear();
			this.NUD_torquefactor.DataBindings.Add(new Binding("Value",this.scprofile.general,"TorqueFactor"));
			this.NUD_PowerFactor.DataBindings.Clear();
			this.NUD_PowerFactor.DataBindings.Add(new Binding("Value",this.scprofile.general,"PowerFactor"));
			#endregion general Tab

			#region Open Loop Tab
			this.NUD_OL_percentOverCurrent.DataBindings.Clear();
			this.NUD_OL_percentOverCurrent.DataBindings.Add(new Binding("Value", this.scprofile.openloop, "percentOverCurrent"));
			this.NUD_OL_testIterations.DataBindings.Clear();
			this.NUD_OL_testIterations.DataBindings.Add(new Binding("Value", this.scprofile.openloop, "numTestIterations"));
			this.NUD_OL_nmuTImesToApplyTestPts.DataBindings.Clear();
			this.NUD_OL_nmuTImesToApplyTestPts.DataBindings.Add(new Binding("Value", this.scprofile.openloop, "numTestPointApplications"));
			this.updateOLTestPoints();
			#endregion Open Loop Tab

			#region closed loop
			updateCLTestPoints();
			#endregion closed loop

			#region No Load Test
			this.NUD_NLT_numSamples.DataBindings.Clear();
			this.NUD_NLT_numSamples.DataBindings.Add(new Binding("Value", this.scprofile.noloadtest, "numSamples"));
			this.NUD_NLT_numsamplesFlux.DataBindings.Clear();
			this.NUD_NLT_numsamplesFlux.DataBindings.Add(new Binding("Value", this.scprofile.noloadtest, "numSamplesForFluxChangeSettle"));
			this.NUD_NLT_numsamplesSteadyState.DataBindings.Clear();
			this.NUD_NLT_numsamplesSteadyState.DataBindings.Add(new Binding("Value", this.scprofile.noloadtest, "numSamplesForSteadyStateSettle"));
			this.TB_NLT_maxIdToIqRatio.DataBindings.Clear();
			this.TB_NLT_maxIdToIqRatio.DataBindings.Add(new Binding("Text", this.scprofile.noloadtest, "maxIdIqRatio"));
			this.TB_NLT_W_rate.DataBindings.Clear();
			this.TB_NLT_W_rate.DataBindings.Add(new Binding("Text", this.scprofile.noloadtest, "w_rate"));

			updateNLTTestPoints();
			#endregion No Load test

			#region Power Frame
			this.NUD_PF_minV.DataBindings.Clear();
			this.NUD_PF_minV.DataBindings.Add(new Binding("Value", this.scprofile.powerframe, "minBattVolts"));
			this.NUD_PF_nomV.DataBindings.Clear();
			this.NUD_PF_nomV.DataBindings.Add(new Binding("Value", this.scprofile.powerframe, "nomBattVolts"));
			this.NUD_PF_maxV.DataBindings.Clear();
			this.NUD_PF_maxV.DataBindings.Add(new Binding("Value", this.scprofile.powerframe, "maxBattVolts"));
			this.NUD_PF_maxIs.DataBindings.Clear();
			this.NUD_PF_maxIs.DataBindings.Add(new Binding("Value", this.scprofile.powerframe, "maxIs"));
			#endregion Power Frame
		}
		#endregion Initilisation

		#region hide/show user controls
		private void hideUserControls()
		{
		}
		private void showUserControls()
		{
		}

		#endregion hide/show user controls

		#region finalization
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

		#endregion finalization

		#region user control chaged event handlers
		#endregion user control chaged event handlers

		#region main menu event handlers
		private void MISave_Click(object sender, System.EventArgs e)
		{
			saveFileDialog1.FileName = "";  //endsure that we opena t correct directory
			if ( Directory.Exists( MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES" ) == false )
			{
				try
				{
					Directory.CreateDirectory( MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES" );
				}
				catch{}
			}
			saveFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES";
			saveFileDialog1.Title = "Save Data to Self CHar Test Profile";
			saveFileDialog1.Filter = "data files (*.xml)|*.xml" ;
			saveFileDialog1.DefaultExt = "xml";
			saveFileDialog1.ShowDialog(this);	

			if(saveFileDialog1.FileName != "")
			{
				this.hideUserControls();
				ObjectXMLSerializer xmlSerializer = new ObjectXMLSerializer();
				try
				{
					xmlSerializer.Save(this.scprofile, this.saveFileDialog1.FileName);
				}
				catch(Exception ex)
				{
					Message.Show("Unable to save Self Char Test profile. Ensure file is closed and retry \nError code: " + ex.Message); 
					this.showUserControls();
					return;
				}
				this.showUserControls();
			}
		}

		private void MIopen_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.FileName = "";  //ensure that initial directory works correctly
			if ( Directory.Exists( MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES" ) == false )
			{
				try
				{
					Directory.CreateDirectory( MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES" );
				}
				catch{}
			}
			openFileDialog1.InitialDirectory = MAIN_WINDOW.UserDirectoryPath + @"\SELFCHAR\TEST PROFILES";
			openFileDialog1.Title = "Open existing Self Char Test Profile";
			openFileDialog1.Filter = "Data files (*.xml)|*.xml" ;
			openFileDialog1.DefaultExt = "xml";
			openFileDialog1.ShowDialog(this);

			if(openFileDialog1.FileName != "")
			{
				this.hideUserControls();
				ObjectXMLSerializer xmlSerializer = new ObjectXMLSerializer();
				try
				{
					this.scprofile = (SCprofile) xmlSerializer.Load(this.scprofile, openFileDialog1.FileName);
				}
				catch(Exception ex)
				{
					Message.Show("Unable to load Self CHar Test profile. Ensure file is closed and retry \nErrocr code: " + ex.Message); 
					this.showUserControls();
					return;
				}
				//we MUST re-bind after opening XML
				bindControls();
				this.showUserControls();
			}
		}

		private void MI_useDefaults_Click(object sender, System.EventArgs e)
		{
			byte prodVolts = this.scprofile.productVoltage;
			byte prodCurr = this.scprofile.productCurrent;
			this.scprofile.applyDefaults(prodVolts, prodCurr);
			this.bindControls();
		}

		#endregion main menu event handlers

		#region profile data reading/editing/saving
		private void updateOLTestPoints()
		{
			this.TV_OL_testpoints.Nodes.Clear(); //empty the TreeView 
			ushort ptCtr = 1;
			foreach(PointF tstPt in this.scprofile.openloop.testPoints)
			{
				this.TV_OL_testpoints.Nodes.Add(new TreeNode("Volts: " + tstPt.Y.ToString() + "V, " + "Time: " + tstPt.X.ToString()) +"s");
				ptCtr++;
			}
		}
		private void updateCLTestPoints()
		{
			this.TV_CL_TestPoints.Nodes.Clear();
			foreach(SCProf_ClosedLoop.SCProf_CLtest cltest in this.scprofile.closedloop.CLtests)
			{
				TreeNode testName = new TreeNode(cltest.testName);  //will be CLA or CLB 
				ushort ptNum = 1;
				foreach(PointF tstPt in cltest.testpoints)
				{
					testName.Nodes.Add(new TreeNode("Current: " + tstPt.X.ToString() + "A, " + "Speed: " + tstPt.Y.ToString()) + "rpm");
					ptNum++;
				}
				TV_CL_TestPoints.Nodes.Add(testName);
			}
			foreach(TreeNode node in this.TV_CL_TestPoints.Nodes)
			{
				node.Expand();
			}
		}
		private void updateNLTTestPoints()
		{
			this.TV_NLT_testPts.Nodes.Clear();
			ushort ptCtr = 1;
			foreach( float spdPt in this.scprofile.noloadtest.speedpoints)
			{
				this.TV_NLT_testPts.Nodes.Add(new TreeNode("speed pt " + ptCtr.ToString() + ": " + spdPt.ToString())+"rpm");
				ptCtr++;
			}
		}
		private void setupContextMenus()
		{
			#region Open and CLosed Loop
			MenuItem editTpt = new MenuItem("Edit this test point");
			editTpt.Click +=new EventHandler(editTpt_Click);
			MenuItem addTptBel = new MenuItem("Add new test point below");
			addTptBel.Click +=new EventHandler(addTpt_Click);
			MenuItem addTptAbv = new MenuItem("Add new test point above");
			addTptAbv.Click +=new EventHandler(addTpt_Click);
			MenuItem delTpt = new MenuItem("Delete this test point");
			delTpt.Click +=new EventHandler(delTpt_Click);
			this.CM_OLoop_and_CLoop.MenuItems.Add(editTpt);
			this.CM_OLoop_and_CLoop.MenuItems.Add(addTptBel);
			this.CM_OLoop_and_CLoop.MenuItems.Add(addTptAbv);
			this.CM_OLoop_and_CLoop.MenuItems.Add(delTpt);
			#endregion Open and CLosed Loop

			#region No Load Test
			MenuItem NLTeditTpt = new MenuItem("Edit this speed point");
			NLTeditTpt.Click +=new EventHandler(editTpt_Click);
			MenuItem NLTaddTptBel = new MenuItem("Add new speed point below");
			NLTaddTptBel.Click +=new EventHandler(addTpt_Click);
			MenuItem NLTaddTptAbv = new MenuItem("Add new speed point above");
			NLTaddTptAbv.Click +=new EventHandler(addTpt_Click);
			MenuItem NLTdelTpt = new MenuItem("Delete this speed point");
			NLTdelTpt.Click +=new EventHandler(delTpt_Click);
			this.CM_NLT.MenuItems.Add(NLTeditTpt);
			this.CM_NLT.MenuItems.Add(NLTaddTptBel);
			this.CM_NLT.MenuItems.Add(NLTaddTptAbv);
			this.CM_NLT.MenuItems.Add(NLTdelTpt);
			#endregion No Load Test
		}


		#region context menu event handlers
		private void TreeView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{
				selectedTreeNode = null;
				switch (this.tabControl1.SelectedIndex)
				{
					case (int)TabPages.OPEN_LOOP:
						#region open loop
						selectedTreeNode = this.TV_OL_testpoints.GetNodeAt(this.TV_OL_testpoints.PointToClient(Cursor.Position));
						if(selectedTreeNode != null)  //we clicked over a node
						{
							this.TV_OL_testpoints.SelectedNode = selectedTreeNode;
							this.CM_OLoop_and_CLoop.Show(this.selectedTreeNode.TreeView, this.selectedTreeNode.TreeView.PointToClient(Cursor.Position));
						}
						#endregion open loop
						break;
					case (int) TabPages.CLOSED_LOOP:
						#region closed loop
						selectedTreeNode = this.TV_CL_TestPoints.GetNodeAt(this.TV_CL_TestPoints.PointToClient(Cursor.Position));
						if(selectedTreeNode != null)  //we clicked over a node
						{
							this.TV_CL_TestPoints.SelectedNode = selectedTreeNode;
							if(this.selectedTreeNode.Parent!= null) //only show for 2nd levle test points
							{
								this.CM_OLoop_and_CLoop.Show(this.selectedTreeNode.TreeView, this.selectedTreeNode.TreeView.PointToClient(Cursor.Position));
							}
						}
						break;
						#endregion closed loop

					case (int)TabPages.NO_LOAD_TEST:
						#region no load test
						selectedTreeNode = this.TV_NLT_testPts.GetNodeAt(this.TV_NLT_testPts.PointToClient(Cursor.Position));
						if(selectedTreeNode != null)  //we clicked over a node
						{
							this.TV_NLT_testPts.SelectedNode = selectedTreeNode;
							this.CM_NLT.Show(this.TV_NLT_testPts, this.TV_NLT_testPts.PointToClient(Cursor.Position));
						}
						break;
						#endregion no load test
					default:
						return;
				}
			}
		}

		private void editTpt_Click(object sender, EventArgs e)
		{
			switch (this.tabControl1.SelectedIndex)
			{
				case (int)TabPages.OPEN_LOOP:
					Form frm = new SC_TESTPOINT_DIALOG(TabPages.OPEN_LOOP);
					frm.ShowDialog();
					this.scprofile.openloop.testPoints[this.selectedTreeNode.Index] = DriveWizard.SC_TESTPOINT_DIALOG.testPointData;
					this.updateOLTestPoints();
					break;
				case (int) TabPages.CLOSED_LOOP:
					frm = new SC_TESTPOINT_DIALOG(TabPages.CLOSED_LOOP);
					frm.ShowDialog();
					ArrayList CLtests = (ArrayList) this.scprofile.closedloop.CLtests;
					SCProf_ClosedLoop.SCProf_CLtest cltst = (SCProf_ClosedLoop.SCProf_CLtest) CLtests[this.selectedTreeNode.Parent.Index];
					ArrayList CLtestPoints = (ArrayList) cltst.testpoints;
					CLtestPoints[this.selectedTreeNode.Index] = DriveWizard.SC_TESTPOINT_DIALOG.testPointData;
					this.updateCLTestPoints();
					break;
				case (int)TabPages.NO_LOAD_TEST:
					frm = new SC_TESTPOINT_DIALOG(TabPages.NO_LOAD_TEST);
					frm.ShowDialog();
					this.scprofile.noloadtest.speedpoints[this.selectedTreeNode.Index] = DriveWizard.SC_TESTPOINT_DIALOG.testPointData.Y;
					updateNLTTestPoints();
					break;
			}
		}
		private void delTpt_Click(object sender, EventArgs e)
		{
			switch (this.tabControl1.SelectedIndex)
			{
				case (int)TabPages.OPEN_LOOP:
					this.scprofile.openloop.testPoints.RemoveAt(this.selectedTreeNode.Index);
					this.updateOLTestPoints();
					break;
				case (int) TabPages.CLOSED_LOOP:
					ArrayList clTests = (ArrayList)this.scprofile.closedloop.CLtests;
					SCProf_ClosedLoop.SCProf_CLtest cltst = (SCProf_ClosedLoop.SCProf_CLtest) clTests[this.selectedTreeNode.Parent.Index];
					ArrayList CLtestPoints = (ArrayList) cltst.testpoints;
					CLtestPoints.RemoveAt(this.selectedTreeNode.Index);
					this.updateCLTestPoints();
					break;
				case (int)TabPages.NO_LOAD_TEST:
					this.scprofile.noloadtest.speedpoints.RemoveAt(this.selectedTreeNode.Index);
					this.updateNLTTestPoints();
					break;
			}
		}

		private void addTpt_Click(object sender, EventArgs e)
		{
			bool insertBelowSelectedTestPoint = false;
			MenuItem senderMI = (MenuItem) sender;
			if(senderMI.Text.ToLower().IndexOf("below") != -1)
			{
				insertBelowSelectedTestPoint = true;
			}
			switch (this.tabControl1.SelectedIndex)
			{
				case (int)TabPages.OPEN_LOOP:
					#region open loop
					Form frm = new SC_TESTPOINT_DIALOG(TabPages.OPEN_LOOP);
					frm.ShowDialog();
					if(insertBelowSelectedTestPoint == true) 
					{
						if(this.selectedTreeNode.Index>=this.TV_OL_testpoints.Nodes.Count)
						{//append
							this.scprofile.openloop.testPoints.Add(DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
						}
						else 
						{//insert below
							this.scprofile.openloop.testPoints.Insert(this.selectedTreeNode.Index+1, DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
						}
					}
					else
					{ //insert above
						this.scprofile.openloop.testPoints.Insert(this.selectedTreeNode.Index, DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
					}
					updateOLTestPoints();
					break;
					#endregion open loop
				case (int) TabPages.CLOSED_LOOP:
					#region closed loop
					frm = new SC_TESTPOINT_DIALOG(TabPages.CLOSED_LOOP);
					frm.ShowDialog();
					ArrayList CLtests = (ArrayList) this.scprofile.closedloop.CLtests;
					SCProf_ClosedLoop.SCProf_CLtest cltst = (SCProf_ClosedLoop.SCProf_CLtest) CLtests[this.selectedTreeNode.Parent.Index];
					ArrayList CLtestPoints = (ArrayList) cltst.testpoints;
					if(insertBelowSelectedTestPoint == true)
					{
						if(this.selectedTreeNode.Index>=this.selectedTreeNode.Parent.Nodes.Count)
						{//append
							CLtestPoints.Add(DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
						}
						else
						{//insert below
							CLtestPoints.Insert(this.selectedTreeNode.Index+1, DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
						}
					}
					else
					{ //insert above
						CLtestPoints.Insert(this.selectedTreeNode.Index, DriveWizard.SC_TESTPOINT_DIALOG.testPointData);
					}
					updateCLTestPoints();
					break;
					#endregion closed loop
				case (int)TabPages.NO_LOAD_TEST:
					#region no load test
					frm = new SC_TESTPOINT_DIALOG(TabPages.NO_LOAD_TEST);
					frm.ShowDialog();
					if(insertBelowSelectedTestPoint ==true)
					{
						if(this.selectedTreeNode.Index>=this.TV_NLT_testPts.Nodes.Count)
						{ //append
							this.scprofile.noloadtest.speedpoints.Add(DriveWizard.SC_TESTPOINT_DIALOG.testPointData.Y);
						}
						else
						{ //insert below
							this.scprofile.noloadtest.speedpoints.Insert(this.selectedTreeNode.Index+1, DriveWizard.SC_TESTPOINT_DIALOG.testPointData.Y);
						}
					}
					else
					{ //insert above
						this.scprofile.noloadtest.speedpoints.Insert(this.selectedTreeNode.Index, DriveWizard.SC_TESTPOINT_DIALOG.testPointData.Y);
					}
					this.updateNLTTestPoints();
					break;
					#endregion no load test
			}
		}
		#endregion context menu event handlers
		#endregion profile data reading/editing/saving
	}
}
