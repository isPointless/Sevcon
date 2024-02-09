/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.1$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 22:28:30$
	$ModDate:05/12/2007 22:26:44$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
 
REFERENCES    

MODIFICATION HISTORY
    $Log:  66273: amebsa.cs 

   Rev 1.1    05/12/2007 22:28:30  ak
 TC keywords added to source for version control.



*******************************************************************************/
using System;

namespace NumericalRecipes.MinimizationOrMaximizationOfFunctions
{
	/// <summary>
	/// Multidimensional minimization of the function funk(x) where x[0..ndim-1] is a vector in
	/// ndim dimensions, by simulated annealing combined with the downhill simplex method of Nelder
	/// and Mead. The input matrix p[0..ndim,0..ndim-1] has ndim+1 rows, each an ndim-dimensional
	/// vector which is a vertex of the starting simplex. Also input are the following: the
	/// vector y[0..ndim], whose components must be pre-initialized to the values of funk evaluated
	/// at the ndim+1 vertices (rows) of p; ftol, the fractional convergence tolerance to be
	/// achieved in the function value for an early return; iter, and temptr. The routine makes iter
	/// function evaluations at an annealing temperature temptr, then returns. You should then decrease 
	/// temptr according to your annealing schedule, reset iter, and call the routine again
	/// (leaving other arguments unaltered between calls). If iter is returned with a positive value,
	/// then early convergence and return occurred. If you initialize yb to a very large value on the first
	/// call, then yb and pb[0..ndim-1] will subsequently return the best function value and point ever
	/// encountered (even if it is no longer a point in the simplex).
	/// </summary>
	public class Amebsa
	{
		public Amebsa()
		{
		}
		private void GET_PSUM(double[,] p, double[] psum, int ndim)
		{
			int n,m;
			double sum;
			for (n=0;n<ndim;n++) 
			{
				for (sum=0.0, m=0;m<(ndim+1);m++) sum += p[m,n];
				psum[n]=sum;
			}
		}
		private double yb=100000.0;
		public double Best
		{
			get{return yb;}
			set{yb=value;}
		}
		private int iter=5000;
		public int Iteration
		{
			get{return iter;}
			set{iter=value;}
		}
        public void amebsa(double[,] p, double[] y, int ndim, double[] pb, double ftol,
			NumericalRecipes.Delegates.FunctionDoubleAToDouble funk, double temptr)
		{
			Amotsa obj = new Amotsa();
			obj.Yb = this.yb; //judetemp
			NumericalRecipes.RandomNumbers.Ran1 obj2
				= new NumericalRecipes.RandomNumbers.Ran1();
			int i,ihi,ilo,j,n,mpts=ndim+1;
			double rtol,swap,yhi,ylo,ynhi,ysave,yt,ytry,tt;
			double[] psum = new Double[ndim];

			tt = -temptr;
			GET_PSUM(p,psum,ndim);
				for (;;) 
				{
					ilo=0;
					ihi=1;
					ynhi=ylo=y[0]+tt*Math.Log(obj2.ran1());
					yhi=y[1]+tt*Math.Log(obj2.ran1());
					if (ylo > yhi) 
					{
						ihi=0;
						ilo=1;
						ynhi=yhi;
						yhi=ylo;
						ylo=ynhi;
					}
					for (i=2;i<mpts;i++) 
					{
						yt=y[i]+tt*Math.Log(obj2.ran1());
						if (yt <= ylo) 
						{
							ilo=i;
							ylo=yt;
						}
						if (yt > yhi) 
						{
							ynhi=yhi;
							ihi=i;
							yhi=yt;
						} 
						else if (yt > ynhi) 
						{
							ynhi=yt;
						}
					}
					rtol=2.0*Math.Abs(yhi-ylo)/(Math.Abs(yhi)+Math.Abs(ylo));
					if (rtol < ftol || iter < 0) 
					{
						swap=y[0];
						y[0]=y[ilo];
						y[ilo]=swap;
						for (n=0;n<ndim;n++) 
						{
							swap=p[0,n];
							p[0,n]=p[ilo,n];
							p[ilo,n]=swap;
						}
						break;
					}
					iter -= 2;
					ytry=obj.amotsa(p,y,psum,ndim,pb,yb,funk,ihi,yhi,-1.0,tt);
					yb=obj.Yb;
					yhi=obj.Yhi;
					if (ytry <= ylo) 
					{
						ytry=obj.amotsa(p,y,psum,ndim,pb,yb,funk,ihi,yhi,2.0,tt);
						yb=obj.Yb;
						yhi=obj.Yhi;
					}
					else if (ytry >= ynhi) 
					{
						ysave=yhi;
						ytry=obj.amotsa(p,y,psum,ndim,pb,yb,funk,ihi,yhi,0.5,tt);
						yb=obj.Yb;
						yhi=obj.Yhi;
						if (ytry >= ysave) 
						{
							for (i=0;i<mpts;i++) 
							{
								if (i != ilo) 
								{
									for (j=0;j<ndim;j++) 
									{
										psum[j]=0.5*(p[i,j]+p[ilo,j]);
										p[i,j]=psum[j];
									}
									y[i]=funk(psum);
								}
							}
							iter -= ndim;
							GET_PSUM(p,psum,ndim);
						}
					} 
					else ++iter;
			}
		}  
	}
}
        /*
       private void button1_Click(object sender, System.EventArgs e)
		{
			double[] pb = new Double[3];
			double[,] p = new Double[4,3];
			double[] y = new Double[4];

			p[0,0]=0.0;
			p[0,1]=0.0;
			p[0,2]=0.0;

			p[1,0]=1.0;
			p[1,1]=0.0;
			p[1,2]=0.0;

			p[2,0]=0.0;
			p[2,1]=1.0;
			p[2,2]=0.0;

			p[3,0]=0.0;
			p[3,1]=0.0;
			p[3,2]=1.0;


			y[0]=14.0;
			y[1]=11.0;
			y[2]=21.0;
			y[3]=17.0;

			NR.Delegates.FunctionDoubleAToDouble dele 
				= new NR.Delegates.FunctionDoubleAToDouble(func);
			NR.MinimizationOrMaximizationOfFunctions.Amebsa obj 
				= new NR.MinimizationOrMaximizationOfFunctions.Amebsa();

			obj.Best = 100.0;
			obj.amebsa(p,y,3,pb,0.000000001,dele,0.0000000001);
			
			for(int k = 0; k<pb.Length; k++)
				textBox1.Text += Convert.ToString(pb[k])+"\r\n";
		}
		private double func(double[] x)
		{
			return (x[0]-2.0)*(x[0]-2.0)+(x[1]+3.0)*(x[1]+3.0)+(x[2]+1.0)*(x[2]+1.0);
		}  
		*/
