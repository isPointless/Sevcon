/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.4$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:12:40$
	$ModDate:05/12/2007 22:02:14$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	

REFERENCES    

MODIFICATION HISTORY
    $Log:  36759: LOGIN_WINDOW.cs 

   Rev 1.4    05/12/2007 22:12:40  ak
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
	/// Summary description for LOGIN_WINDOW.
	/// </summary>
	public class LOGIN_WINDOW : System.Windows.Forms.Form
	{
        #region window controls declarations

		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.ToolTip psswrdtip;
		private System.Windows.Forms.ErrorProvider errorProvider1;
		private System.Windows.Forms.ToolTip Pb_tooltip;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Timer timer1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ToolTip nodeTip;
		private System.Windows.Forms.ToolTip userTip;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ErrorProvider errorProvider2;
		#endregion

		#region my declarations
		uint requestUserIDNo;
		uint passwordNumericValue;
		string [] LoginIDs;
		private SystemInfo localSystemInfo;
		private int selectednodeID = 201;
		private System.Timers.Timer timer2;
		private DIFeedbackCode feedback = DIFeedbackCode.DICodeUnset;
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.nodeTip = new System.Windows.Forms.ToolTip(this.components);
			this.psswrdtip = new System.Windows.Forms.ToolTip(this.components);
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.Pb_tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.label4 = new System.Windows.Forms.Label();
			this.errorProvider2 = new System.Windows.Forms.ErrorProvider();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.userTip = new System.Windows.Forms.ToolTip(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.timer2 = new System.Timers.Timer();
			((System.ComponentModel.ISupportInitialize)(this.timer2)).BeginInit();
			this.SuspendLayout();
			// 
			// comboBox2
			// 
			this.comboBox2.Location = new System.Drawing.Point(152, 112);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(256, 25);
			this.comboBox2.TabIndex = 1;
			this.userTip.SetToolTip(this.comboBox2, "Enter or select your User Identification");
			this.comboBox2.Validating += new System.ComponentModel.CancelEventHandler(this.comboBox1_Validating);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(152, 200);
			this.textBox1.MaxLength = 5;
			this.textBox1.Name = "textBox1";
			this.textBox1.PasswordChar = '*';
			this.textBox1.Size = new System.Drawing.Size(256, 25);
			this.textBox1.TabIndex = 2;
			this.textBox1.Text = "";
			this.nodeTip.SetToolTip(this.textBox1, "Numeric Password between 1 and 65535");
			this.textBox1.Validating += new System.ComponentModel.CancelEventHandler(this.textBox1_Validating);
			// 
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
			this.errorProvider1.DataMember = "";
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(8, 744);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(992, 25);
			this.progressBar1.TabIndex = 3;
			this.Pb_tooltip.SetToolTip(this.progressBar1, "Progress indicator");
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(8, 704);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(80, 25);
			this.button1.TabIndex = 3;
			this.button1.Text = "&Submit";
			this.button1.Click += new System.EventHandler(this.submitBtn_Click);
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.CausesValidation = false;
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button2.Location = new System.Drawing.Point(856, 704);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(144, 25);
			this.button2.TabIndex = 4;
			this.button2.Text = "&Close this window";
			this.button2.Click += new System.EventHandler(this.closeBtn_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 200);
			this.label3.Name = "label3";
			this.label3.TabIndex = 6;
			this.label3.Text = "Password";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 112);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 25);
			this.label2.TabIndex = 7;
			this.label2.Text = "User ID";
			// 
			// timer1
			// 
			this.timer1.Interval = 400;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label4.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(16, 664);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(984, 25);
			this.label4.TabIndex = 8;
			// 
			// errorProvider2
			// 
			this.errorProvider2.ContainerControl = this;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(960, 25);
			this.label1.TabIndex = 9;
			// 
			// timer2
			// 
			this.timer2.Enabled = true;
			this.timer2.Interval = 250;
			this.timer2.SynchronizingObject = this;
			this.timer2.Elapsed += new System.Timers.ElapsedEventHandler(this.timer2_Elapsed);
			// 
			// LOGIN_WINDOW
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleBaseSize = new System.Drawing.Size(7, 18);
			this.BackColor = System.Drawing.Color.LightGray;
			this.CancelButton = this.button2;
			this.ClientSize = new System.Drawing.Size(1013, 780);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.comboBox2);
			this.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LOGIN_WINDOW";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Login";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.LOGIN_WINDOW_Load);
			((System.ComponentModel.ISupportInitialize)(this.timer2)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region initialisation
		public LOGIN_WINDOW(int nodeNum, string nodeText, string [] UserIDs, ref SystemInfo systemInfo)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			LoginIDs = new string[5];
			for (int i = 1;i<UserIDs.Length;i++)
			{
				LoginIDs[i-1] = UserIDs[i];
			}
			localSystemInfo = systemInfo;
			comboBox2.Items.AddRange(LoginIDs);
			comboBox2.Text = LoginIDs[0];
			this.label1.Text = "Re-attempting login to the " + nodeText;
			this.selectednodeID = nodeNum;
		}

		public LOGIN_WINDOW(string [] UserIDs, ref SystemInfo systemInfo)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			localSystemInfo = systemInfo;
			LoginIDs = new string[5];
			for (int i = 1;i<UserIDs.Length;i++)
			{
				LoginIDs[i-1] = UserIDs[i];
			}
			comboBox2.Items.AddRange(LoginIDs);
			comboBox2.Text = LoginIDs[0];
		}

		private void LOGIN_WINDOW_Load(object sender, System.EventArgs e)
		{
			requestUserIDNo = 1; //reflects default comboBox Text display 
			//reset password to invlaid value, so even if user make no change to a control the validation will
			//occur by default
			passwordNumericValue = 0;
		}
		#endregion

		#region user interaction zone
		private void submitBtn_Click(object sender, System.EventArgs e)
		{
			label4.Text = "Requesting access. Please wait.";
			label4.Update();
			if (this.selectednodeID == 201)  //system login
			{
				feedback = this.localSystemInfo.loginToSystem(requestUserIDNo, passwordNumericValue);
				if ( feedback != DIFeedbackCode.DISuccess )
				{
					Message.Show("Login failed. \nError code: " + feedback.ToString());
				}
			}
			else 
			{
				feedback = this.localSystemInfo.loginToNode(this.selectednodeID, requestUserIDNo, passwordNumericValue);
				if ( feedback != DIFeedbackCode.DISuccess )
				{
					Message.Show("Login failed. \nError code: " + feedback.ToString());
				}
			}
			if(feedback == DIFeedbackCode.DISuccess)
			{
				if(this.localSystemInfo.systemAccess ==0)
				{
					StatusBar tmpsb = (StatusBar)this.MdiParent.Controls[0]; 
					tmpsb.Panels[1].Text = "Login to Access SEVCON nodes";
					try
					{
						Icon icon = new Icon(@"resources\accessGrey.ico");
						tmpsb.Panels[1].Icon = icon;
					}
					catch
					{
						//do nothing, just don't display the icon
					}
					MenuItem tmpMI = (MenuItem)this.MdiParent.Menu.MenuItems[1].MenuItems[1];  //login
					tmpMI.Checked = false;
					label4.Text = "Login Failure, please retry or Cancel";
				}
				else
				{
					StatusBar tmpsb = (StatusBar)this.MdiParent.Controls[0]; 
					tmpsb.Panels[1].Text = DriveWizard.MAIN_WINDOW.UserIDs[this.localSystemInfo.systemAccess];
					try
					{
						Icon icon = new Icon(@"resources\access.ico");
						tmpsb.Panels[1].Icon = icon;
					}
					catch 
					{
						//do nothing , just don't display the icon
					}
					MenuItem tmpMI = (MenuItem)this.MdiParent.Menu.MenuItems[1].MenuItems[1];  //login
					tmpMI.Checked = true;
					this.Close();
				}
				button2.Text = "&Close";
			}
		}
		private void timer1_Tick(object sender, System.EventArgs e)
		{
			progressBar1.PerformStep();
		}
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

		#region password validation code
		private void textBox1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				validatingPasswordCode();
			}

			catch(Exception ex)
			{
				// Cancel the event and select the text to be corrected by the user.
				e.Cancel = true;
				textBox1.Select(0, textBox1.Text.Length);
				// Set the ErrorProvider error with the text to display. 
				this.errorProvider1.SetError(textBox1, ex.Message);
			}
		
		}

		private void validatingPasswordCode()
		{
			// Confirm that user has entered something into textbox
			if (textBox1.Text.Length == 0)
			{
				label4.Text = "Password Required";
				throw new Exception("Password Required");
				
			}
			// confirm that all characters are numeric
			else
			{
				textBox1.Text.ToCharArray();
				for(int i = 0;i<textBox1.Text.Length;i++)
				{
					if((textBox1.Text[i]<'0') || (textBox1.Text[i]>'9'))	
					{
						label4.Text = "Invalid character";
						throw new Exception("Invalid character");
					}
				}
			passwordNumericValue = System.UInt32.Parse(textBox1.Text);
			if ((passwordNumericValue<1) || (passwordNumericValue>65535))
				{
					label4.Text = "Password value out of range";	
					throw new Exception("Password value out of range");
				}
			}
            //If we get this far the password must be valid
			label4.Text = "";
			errorProvider1.SetError(textBox1, "");
		}
		#endregion

		#region User ID validation
		private void comboBox1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				validatingUserIDCode();
			}

			catch
			{
				// Cancel the event and select the text to be corrected by the user.
				e.Cancel = true;
				// Set the ErrorProvider error with the text to display. 
				errorProvider2.SetError(comboBox2, "Select a User ID");
			}
		
		}
	
		private void validatingUserIDCode()
		{
			//check that entered ID matches a predefined one
			bool validUser = false;
			for(uint i=0;i<(LoginIDs.Length);i++)
			{
				if(comboBox2.Text == LoginIDs[i])
				{
					validUser = true;
					requestUserIDNo = i + 1;  //since zero corresponds to No access
				}
			}
			if(validUser == false)
			{
				label4.Text = "Select or enter a valid User ID";
				requestUserIDNo = 0;
				throw new Exception("Select or enter a valid User ID");
			}
		//if we get this far then a valid User ID has been selected
		label4.Text = "";
		errorProvider2.SetError(comboBox2, "");
		}
		#endregion

		private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if ( AutoValidate.staticValidationRunning == false )
			{
				timer2.Enabled = false;
			}

			timer2Elapsed();
		}

		[Conditional ("AUTOVALIDATE")]
		private void timer2Elapsed()
		{
			if ( this.localSystemInfo.autoTest.validationRunning == true )
			{
				this.localSystemInfo.autoTest.testFeedback = feedback;

				switch ( this.localSystemInfo.autoTest.testState )
				{
					case ValidateState.LOGIN_WINDOW:
					{
						Debug.WriteLine( "Simulating entering password level 5 to text box." );
						this.comboBox2.SelectedIndex = 0;
						this.textBox1.Focus();
						this.textBox1.Text = "5";
						Debug.WriteLine( "Simulating validation of entered password." );
						validatingPasswordCode();
						Debug.WriteLine( "Simulating submit button click." );
						this.button1.Focus();
						this.button1.PerformClick();
						this.localSystemInfo.autoTest.testState = ValidateState.ATTEMPT_LOGIN;
						this.localSystemInfo.autoTest.testFeedback = DIFeedbackCode.DICodeUnset;
						break;
					}

					case ValidateState.ATTEMPT_LOGIN:
					{
						if ( this.localSystemInfo.autoTest.testFeedback != DIFeedbackCode.DICodeUnset )
						{
							if ( this.localSystemInfo.autoTest.testFeedback != DIFeedbackCode.DISuccess )
							{
								this.localSystemInfo.autoTest.testState = ValidateState.FAILED_TO_LOGIN;
								Debug.WriteLine( "Failed to login: failure reason is " + feedback.ToString() );
							}
							else
							{
								this.localSystemInfo.autoTest.testState = ValidateState.LOGGED_IN;
								Debug.WriteLine( "Logged in at level 5 OK." );
							}

							this.localSystemInfo.autoTest.testFeedback = DIFeedbackCode.DICodeUnset;
							this.button2.Focus();
							this.button2.PerformClick();
							Debug.WriteLine( "Simulating close window button click." );
							this.timer2.Enabled = false;
						}
						break;
					}
				}
			}
			else
			{
				this.timer2.Enabled = false;
			}
		}
	}
}
