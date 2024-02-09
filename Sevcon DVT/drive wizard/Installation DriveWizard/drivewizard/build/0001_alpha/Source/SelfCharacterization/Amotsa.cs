/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.1$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:28:26$
	$ModDate:05/12/2007 22:26:56$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  66275: Amotsa.cs 

   Rev 1.1    05/12/2007 22:28:26  ak
 TC keywords added to source for version control.



*******************************************************************************/
using System;

namespace NumericalRecipes.MinimizationOrMaximizationOfFunctions
{
	/// <summary>
	/// Extrapolates by a factor fac through the face of the simplex across from the high point, tries
	/// it, and replaces the high point if the new point is better.
	/// </summary>
	public class Amotsa 
	{
		public Amotsa()
		{
		}
		private double yb, yhi;
		public double Yb
		{
			get{return yb;}
			set 
			{
				yb = value; //judetemp
			}
		}
		public double Yhi
		{
			get{return yhi;}
		}
		NumericalRecipes.RandomNumbers.Ran1 obj 
			= new NumericalRecipes.RandomNumbers.Ran1();
		public double amotsa(double[,] p, double[] y, double[] psum, int ndim, double[] pb, double ybt,
			NumericalRecipes.Delegates.FunctionDoubleAToDouble funk, int ihi, double yhit, double fac, double tt)
		{		   
	   		int j;
			double fac1,fac2,yflu,ytry;
			double[] ptry = new Double[ndim];

			fac1=(1.0-fac)/(double)ndim;
			fac2=fac1-fac;
			for (j=0;j<ndim;j++)
				ptry[j]=psum[j]*fac1-p[ihi,j]*fac2;
			ytry=funk(ptry);
			if (ytry <= ybt) 
			{
				for (j=0;j<ndim;j++) pb[j]=ptry[j];
				yb=ytry;
			}
			yflu=ytry-tt*Math.Log(obj.ran1());
			if (yflu < yhit) 
			{
				y[ihi]=ytry;
				yhi=yflu;
				for (j=0;j<ndim;j++) 
				{
					psum[j] += ptry[j]-p[ihi,j];
					p[ihi,j]=ptry[j];
				}
			}
			return yflu;
		}  
	}
}
