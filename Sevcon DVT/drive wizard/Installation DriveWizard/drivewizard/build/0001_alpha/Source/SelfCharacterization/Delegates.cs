/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.1$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:28:26$
	$ModDate:05/12/2007 22:27:38$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  66277: Delegates.cs 

   Rev 1.1    05/12/2007 22:28:26  ak
 TC keywords added to source for version control.



*******************************************************************************/
using System;

namespace NumericalRecipes
{
	/// <summary>
	/// Delegates are gathered here.
	/// </summary>
	public class Delegates
	{
		public Delegates()
		{
		}
		public delegate int FunctionIntToInt(int x);
		public delegate double FunctionIntToDouble(int x);
		public delegate int FunctionDoubleToInt(double x);
		public delegate double FunctionDoubleToDouble(double x);
		public delegate double Function2DToDouble(double x, double y);
		public delegate double Function3DToDouble(double x, double y, double z);
		public delegate double Function4DToDouble(double x, double y, double z, double t);

		public delegate double FunctionIntDoubleToDouble(int a, double x);
		public delegate double[] FunctionIntDoubleToDoubleA(int a, double x);
		public delegate double Function2I2DToDouble(int a, int b, double x, double y);

		public delegate int FunctionIntAToInt(int[] x);
		public delegate double FunctionIntAToDouble(int[] x);
		public delegate int FunctionDoubleAToInt(double[] x);
		public delegate double FunctionDoubleAToDouble(double[] x);

		public delegate int[] FunctionIntToIntA(int x);
		public delegate double[] FunctionIntToDoubleA(int x);
		public delegate int[] FunctionDoubleToIntA(double x);
		public delegate double[] FunctionDoubleToDoubleA(double x);

		public delegate int[] FunctionIntAToIntA(int[] x);
		public delegate double[] FunctionIntAToDoubleA(int[] x);
		public delegate int[] FunctionDoubleAToIntA(double[] x);
		public delegate double[] FunctionDoubleAToDoubleA(double[] x);
		
		public delegate double FunctionDoubleAIntToDouble(double[] x, int y);
		public delegate double[] FunctionDoubleDoubleAToDoubleA(double x, double[] y);
		public delegate double[] FunctionDoubleDoubleAIntToDoubleA(double x, double[] y, int n);
		public delegate double[] FunctionDoubleDoubleAIntIntToDoubleA(double x, double[] y, int nvar, int n);
		public delegate double[,] FunctionDoubleDoubleAToDoubleM(double x, double[] y);

		public delegate void FunctionVoid(double[] x, ulong y, int z);
	}
}
