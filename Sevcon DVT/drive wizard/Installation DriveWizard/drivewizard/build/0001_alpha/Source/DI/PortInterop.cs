/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.1$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 21:13:48$
	$ModDate:05/12/2007 21:11:02$

ORIGINAL AUTHOR
    Levent S.

DESCRIPTION
	LED initialization code written by Levent S.

REFERENCES    

MODIFICATION HISTORY
    $Log:  53524: PortInterop.cs 

   Rev 1.1    05/12/2007 21:13:48  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Runtime.InteropServices;

public class PortAccess
{
	[DllImport("inpout32.dll", EntryPoint="Out32")]
	public static extern void Output(int adress, int value);
}
