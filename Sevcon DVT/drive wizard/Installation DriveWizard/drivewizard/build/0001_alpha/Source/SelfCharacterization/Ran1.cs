/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.1$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:28:28$
	$ModDate:05/12/2007 22:26:20$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  66283: Ran1.cs 

   Rev 1.1    05/12/2007 22:28:28  ak
 TC keywords added to source for version control.



*******************************************************************************/
using System;

namespace NumericalRecipes.RandomNumbers
{
	/// <summary>
	/// "Minimal" random number generator of Park and Miller with Bays-Durham shuffle and added
	/// safeguards. Returns a uniform random deviate between 0.0 and 1.0 (exclusive of the endpoint
	/// values). Call with idum a negative integer to initialize; thereafter, do not alter idum between
	/// successive deviates in a sequence. RNMX should approximate the largest floating value that is
	/// less than 1.
	/// </summary>
	public class Ran1
	{
		long IA = 16807;
		long IM = 2147483647;
		double AM = 1.0/2147483647.0;
		long IQ = 127773;
		long IR = 2836;
		int NTAB = 32;
		double NDIV = 1.0+(2147483647.0-1.0)/32.0;
		double RNMX = 1.0-1.2e-7;
		long iy=0;  
		long[] iv = new long[32];
		private long idum = 1;
		public long Idum
		{
			get { return idum;}
			set { idum = value;}
		}
		public Ran1()
		{			
		}
		public Ran1(long a)
		{			
			idum = a;
		}
		public double ran1()
		{
			int j;
			long k;
		    double temp;
		    if (idum <= 0 || iy == 0) 
	        {
		    	 if (-idum < 1) idum=1;  
		         else idum = -idum;
		         for (j=NTAB+7;j>=0;j--) 
	             {   
					 k=idum/IQ;
		             idum=IA*(idum-k*IQ)-IR*k;
		             if (idum < 0) idum += IM;
		             if (j < NTAB) iv[j] = idum;
	             }
	             iy=iv[0];
            }
            k=idum/IQ;  
            idum=IA*(idum-k*IQ)-IR*k; 
			if (idum < 0) idum += IM;
            j=Convert.ToInt32(iy/NDIV)%NTAB; 
            iy=iv[j];                  
			iv[j] = idum;
		    if ((temp=AM*iy) > RNMX) return RNMX;  
            else return temp;
		}
	}
}
/*private void button1_Click(object sender, System.EventArgs e)
		{
			NR.RandomNumbers.Ran1 dd = new NR.RandomNumbers.Ran1();
			dd.Idum = 10;
			for(int k = 0; k < 100; k++)
			textBox1.Text += Convert.ToString(dd.ran1())+"\r\n";
		}
		*/
