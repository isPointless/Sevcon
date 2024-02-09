/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.6$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 21:26:40$
	$ModDate:05/12/2007 21:24:00$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	Drive Wizard specific form controls definitions.
    sixtyFourBitsAsPanel and autoscrollPanel are defined.

REFERENCES    

MODIFICATION HISTORY
    $Log:  70231: formControls.cs 

   Rev 1.6    05/12/2007 21:26:40  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections;

namespace DriveWizard
{
	/// <summary>
	/// Summary description for formControls.
	/// </summary>
	public class sixtyFourBitsAsPanel: System.Windows.Forms.Panel
	{
		public ArrayList fillParams = new ArrayList();
		public fillParam currfillParam = null;
		public int bitWidth = 1;  //we don't want any divide by zero 
		private Color [] fillColors = {Color.LightBlue, Color.LightCoral, Color.LightCyan, Color.LightGreen, Color.LightGoldenrodYellow,
										  Color.Lavender, Color.LightSeaGreen, Color.LightSlateGray};
		public bool isTx = false;
		public int pnlIndex = 0;
		public class fillParam
		{
			public fillParam( int passed_startBit, int passed_numbits, Color passed_fillColor, string passed_ParamName)
			{
				this.startBit = passed_startBit;
				this.numBits = passed_numbits;
				this.fillColour = passed_fillColor;
				paramName = passed_ParamName;
			}
			public int startBit;
			public int numBits;
			public Color fillColour;
			public string paramName;
		}
		public void addMapping(PDOMapping map)
		{
			int numBitsInMap = (int) (map.mapValue & 0xFF);
			int startBit = 0;
			if(this.fillParams.Count>0)
			{
				sixtyFourBitsAsPanel.fillParam fp = (sixtyFourBitsAsPanel.fillParam) fillParams[this.fillParams.Count-1];
				startBit = fp.startBit + fp.numBits;
			}
			Color col = this.fillColors[this.fillParams.Count%fillColors.Length];  //should cycle round correcty
			sixtyFourBitsAsPanel.fillParam newMap = new sixtyFourBitsAsPanel.fillParam(startBit,numBitsInMap, col, map.mapName + " (0x" + map.mapValue.ToString("X").PadLeft(8, '0') + ")");  //judetmep
			this.fillParams.Add(newMap);
		}
		public sixtyFourBitsAsPanel()
		{
			this.Size = new Size(768, 8);
			this.BorderStyle = BorderStyle.FixedSingle;
			this.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.AllowDrop = true;
		}
		protected override void OnResize(EventArgs e)
		{
			base.OnResize (e);
			int remainder = this.Width%64;
			if(remainder !=0)  
			{//round to nereast multiple of 64 
				if(remainder<=32)
				{ //round down
					this.Width -= remainder;
				}
				else
				{ //round up
					this.Width += (64-remainder);
				}
			}
			this.Height = this.Width/64;
			this.bitWidth = this.Width/64;
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			base.OnPaint (e);
			int cumulativeBits = 0;	
			foreach(fillParam myFill in this.fillParams)
			{				//colour the bits filled
				Brush myBrush = new SolidBrush(myFill.fillColour);
				if(myFill == this.currfillParam)
				{
					myBrush = Brushes.Red;
				}
				e.Graphics.FillRectangle(myBrush,(cumulativeBits * bitWidth), 0, (myFill.numBits * bitWidth), this.Height);
				cumulativeBits += myFill.numBits;
			}
			for(int i = 1;i<64;i++)
			{
				int horizOffset = (int)(i* this.Width/64);
				if(this.isTx == true)
				{
					e.Graphics.DrawLine(Pens.Green, horizOffset,0, horizOffset, this.Height);
					this.ForeColor = Color.Green;
				}
				else
				{
					e.Graphics.DrawLine(Pens.DarkViolet, horizOffset,0, horizOffset, this.Height);
					this.ForeColor = Color.DarkViolet;
				}
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave (e);
			this.currfillParam  = null;
			this.Invalidate();
		}

	}
//	public class autoscrollPanel: Panel
//	{
//		private const int WM_VSCROLL = 277;
//		public autoscrollPanel()
//		{
//		}
//		protected override void WndProc(ref System.Windows.Forms.Message m)
//		{
//			base.WndProc (ref m);
////			if( m.Msg == WM_VSCROLL)
//			{
////				this.Invalidate();
//			}
//		}
//
//
//	}
}


