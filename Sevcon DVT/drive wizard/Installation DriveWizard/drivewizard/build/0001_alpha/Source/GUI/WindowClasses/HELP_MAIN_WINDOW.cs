/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.6$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:13:00$
	$ModDate:05/12/2007 22:01:04$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	

REFERENCES    

MODIFICATION HISTORY
    $Log:  36751: HELP_MAIN_WINDOW.cs 

   Rev 1.6    05/12/2007 22:13:00  ak
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
	/// Summary description for Form2.
	/// </summary>
	public class HELP_MAIN_WINDOW : System.Windows.Forms.Form
	{
		#region form controls defintions

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button hlp_open_btn;
		private System.Windows.Forms.Button hlp_print_btn;
		private System.Windows.Forms.Button close1_btn;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion form controls defintions

		#region initialisation
		public HELP_MAIN_WINDOW()
		{
			InitializeComponent();
			this.BackColor = SCCorpStyle.SCBackColour;
			this.ForeColor = SCCorpStyle.SCForeColour;
		}
		#endregion initialisation

		#region user interaction
		private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			//string mystring = (string)e.Node.Text;
			MessageBox.Show(e.Node.Text);
		}
		#endregion user interaction

		#region finalisation methods
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
		private void close1_btn_Click(object sender, System.EventArgs e)
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

		#endregion finalisation methods

		#region minor methods
		#endregion minor methods

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.panel1 = new System.Windows.Forms.Panel();
			this.close1_btn = new System.Windows.Forms.Button();
			this.hlp_print_btn = new System.Windows.Forms.Button();
			this.hlp_open_btn = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Location = new System.Drawing.Point(30, 24);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(384, 480);
			this.tabControl1.TabIndex = 6;
			// 
			// tabPage1
			// 
			this.tabPage1.Location = new System.Drawing.Point(4, 27);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(376, 449);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Index";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.treeView1);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(376, 454);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Contents";
			// 
			// treeView1
			// 
			this.treeView1.ImageIndex = -1;
			this.treeView1.Location = new System.Drawing.Point(16, 24);
			this.treeView1.Name = "treeView1";
			this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
																				  new System.Windows.Forms.TreeNode("Node0", new System.Windows.Forms.TreeNode[] {
																																									 new System.Windows.Forms.TreeNode("Node1")}),
																				  new System.Windows.Forms.TreeNode("Node2", new System.Windows.Forms.TreeNode[] {
																																									 new System.Windows.Forms.TreeNode("Node3"),
																																									 new System.Windows.Forms.TreeNode("Node4")})});
			this.treeView1.SelectedImageIndex = -1;
			this.treeView1.Size = new System.Drawing.Size(280, 296);
			this.treeView1.TabIndex = 0;
			this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
			// 
			// tabPage3
			// 
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(376, 454);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Search";
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.close1_btn);
			this.panel1.Controls.Add(this.hlp_print_btn);
			this.panel1.Controls.Add(this.hlp_open_btn);
			this.panel1.Location = new System.Drawing.Point(32, 512);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(384, 56);
			this.panel1.TabIndex = 7;
			// 
			// close1_btn
			// 
			this.close1_btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.close1_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.close1_btn.Location = new System.Drawing.Point(280, 16);
			this.close1_btn.Name = "close1_btn";
			this.close1_btn.Size = new System.Drawing.Size(104, 32);
			this.close1_btn.TabIndex = 6;
			this.close1_btn.Text = "&Close";
			this.close1_btn.Click += new System.EventHandler(this.close1_btn_Click_1);
			// 
			// hlp_print_btn
			// 
			this.hlp_print_btn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.hlp_print_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.hlp_print_btn.Location = new System.Drawing.Point(140, 16);
			this.hlp_print_btn.Name = "hlp_print_btn";
			this.hlp_print_btn.Size = new System.Drawing.Size(104, 32);
			this.hlp_print_btn.TabIndex = 5;
			this.hlp_print_btn.Text = "&Print";
			// 
			// hlp_open_btn
			// 
			this.hlp_open_btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.hlp_open_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.hlp_open_btn.Location = new System.Drawing.Point(0, 16);
			this.hlp_open_btn.Name = "hlp_open_btn";
			this.hlp_open_btn.Size = new System.Drawing.Size(104, 32);
			this.hlp_open_btn.TabIndex = 2;
			this.hlp_open_btn.Text = "&Open";
			// 
			// HELP_MAIN_WINDOW
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
			this.ClientSize = new System.Drawing.Size(444, 576);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.tabControl1);
			this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "HELP_MAIN_WINDOW";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Main Help";
			this.Load += new System.EventHandler(this.HELP_MAIN_WINDOW_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void close1_btn_Click_1(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void HELP_MAIN_WINDOW_Load(object sender, System.EventArgs e)
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
