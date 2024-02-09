/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.64$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:13:16$
	$ModDate:05/12/2007 22:04:54$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION

REFERENCES    

MODIFICATION HISTORY
    $Log:  36723: DCF_WINDOW.cs 

   Rev 1.64    05/12/2007 22:13:16  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace DriveWizard
{
//this file is no longer used - and should be excluded form the project
	#region DCF Form Class
	/// <summary>
	/// Summary description for DCF_WINDOW.
	/// </summary>
	public class DCF_WINDOW : System.Windows.Forms.Form
	{
		#region controls declarations

		private System.Timers.Timer timer2;
		private System.ComponentModel.IContainer components;
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.timer2 = new System.Timers.Timer();
			((System.ComponentModel.ISupportInitialize)(this.timer2)).BeginInit();
			// 
			// timer2
			// 
			this.timer2.Enabled = true;
			this.timer2.Interval = 250;
			this.timer2.SynchronizingObject = this;
			// 
			// DCF_WINDOW
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(1013, 760);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Name = "DCF_WINDOW";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "DCF_WINDOW";
			((System.ComponentModel.ISupportInitialize)(this.timer2)).EndInit();

		}
		#endregion

		#region initialisation
		public DCF_WINDOW()
		{
			InitializeComponent();
		}
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

		#region auto validation
//		private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
//		{
//			if( (this.localSystemInfo.autoTest != null) && ( AutoValidate.staticValidationRunning == true ) )
//			{
//				timer2Elapsed();		
//			}
//			else
//			{
//				timer2.Enabled = false;
//			}
//		}
//
//		[Conditional ("AUTOVALIDATE")]
//		private void timer2Elapsed()
//		{
//			if ( this.localSystemInfo.autoTest.validationRunning == false )
//			{
//				return;
//			}
//
//			this.timer2.Enabled = false;
//			this.localSystemInfo.autoTest.testFeedback = feedback;
//
//			switch ( this.localSystemInfo.autoTest.dcfState )
//			{
//				case DCFState.TEST_INITIATE:
//				{
//					Debug.WriteLine( "TEST (3a) UPLOAD DCF FROM CONTROLLER " );
//					Debug.Write( "Upload DCF from device.," );
//					this.localSystemInfo.autoTest.testFeedback = DIFeedbackCode.DICodeUnset;
//					this.localSystemInfo.autoTest.dcfState = DCFState.UPLOAD_FROM_CONTROLLER;
//					break;
//				}
//
//				case DCFState.UPLOAD_FROM_CONTROLLER:
//				{
//					if ( ( timer1.Enabled == false ) && ( this.localSystemInfo.autoTest.testFeedback != DIFeedbackCode.DICodeUnset ) )
//					{
//						if ( this.localSystemInfo.autoTest.testFeedback == DIFeedbackCode.DISuccess )
//						{
//							feedback = 	this.localSystemInfo.saveODtoDCF(SaveNode, "test.dcf");
//							if(feedback == DIFeedbackCode.DISuccess)
//							{
//								Debug.WriteLine( "OK,Saved to 'test.dcf'." );
//							}
//							else
//							{
//								Message.Show("FAIL,Unable to save file: " + feedback.ToString());
//							}							
//						}
//						else
//						{
//							Debug.WriteLine( "FAIL," + feedback.ToString() );
//						}
//					
//						Debug.WriteLine( "" );
//						Debug.WriteLine( "TEST (3b) OPEN 'SHIROKO.DCF' FILE." );
//						Debug.WriteLine( "Open 'shiroko.dcf' file., OK" );
//						fileName = "dcf\\shiroko.dcf";
//						openDCFFile();
//						this.localSystemInfo.autoTest.dcfState = DCFState.OPEN_DCF_FILE;
//						this.localSystemInfo.autoTest.testFeedback = DIFeedbackCode.DICodeUnset;
//					}
//
//					break;
//				}
//
//				case DCFState.OPEN_DCF_FILE:
//				{
//					if ( ( timer1.Enabled == false ) && ( this.localSystemInfo.autoTest.testFeedback != DIFeedbackCode.DICodeUnset ) )
//					{
//						if ( this.localSystemInfo.autoTest.testFeedback == DIFeedbackCode.DISuccess )
//						{
//							Debug.WriteLine( "" );
//							Debug.WriteLine( "TEST (3c) DOWNLOADING DCF FILE TO CONTROLLER " );
//							Debug.Write( "Download DCF to controller.," );
//							this.feedback = DIFeedbackCode.DICodeUnset;
//							this.localSystemInfo.autoTest.dcfState = DCFState.DOWNLOAD_DCF_TO_CONTROLLER;
//						}
//						else
//						{
//							Debug.WriteLine( "Download DCF to controller.,FAIL, failed to open DCF file. Reason: " + this.localSystemInfo.autoTest.testFeedback.ToString() );
//							this.localSystemInfo.autoTest.dcfState = DCFState.END_TEST;
//						}
//					}
//					break;
//				}
//
//				case DCFState.DOWNLOAD_DCF_TO_CONTROLLER:
//				{
//					if 
//						( 
//						( this.localSystemInfo.autoTest.testFeedback != DIFeedbackCode.DICodeUnset ) 
//						)
//					{
//						if ( this.localSystemInfo.autoTest.testFeedback == DIFeedbackCode.DISuccess )
//						{
//							Debug.WriteLine( "OK" );
//						}
//						else
//						{
//							Debug.WriteLine( "FAIL,Reason: " + this.localSystemInfo.autoTest.testFeedback.ToString() );
//						}
//
//						this.localSystemInfo.autoTest.dcfState = DCFState.END_TEST;
//					}
//
//					break;
//				}
//
//				case DCFState.END_TEST:
//				{
//					Debug.WriteLine( "Close DCF window." );
//					Debug.WriteLine( "" );
//					this.timer2.Enabled = false;
//					this.localSystemInfo.autoTest.testState = ValidateState.COMPLETED_DCF_TEST;
//					this.closeBtn.Visible = true;
//					this.closeBtn.Focus();
//					this.closeBtn.PerformClick();
//					this.localSystemInfo.autoTest.dcfState = DCFState.WAIT_FOR_WINDOW_TO_CLOSE;
//					break;
//				}
//			
//				case DCFState.WAIT_FOR_WINDOW_TO_CLOSE:
//				{
//					break;
//				}
//			}
//
//			if ( this.localSystemInfo.autoTest.dcfState != DCFState.WAIT_FOR_WINDOW_TO_CLOSE )
//			{
//				this.timer2.Enabled = true;
//			}
//		}
		#endregion
	}
	#endregion DCF Form Class

}
