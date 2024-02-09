/*******************************************************************************
(C) COPYRIGHT Sevcon 2004

Project Reference UK0139

FILE
	$Revision:1.38$
	$Version:$
	$Author:ak$
	$ProjectName:DriveWizard$ 

	$RevDate:05/12/2007 21:26:42$
	$ModDate:05/12/2007 21:24:06$

ORIGINAL AUTHOR
    Jude Wood

DESCRIPTION
	Definition of the GUI data grid classes used throughout DW.
    i.e. DCF tables, main data tables, log data tables, master counter
    table, CAN comms tables, virtual node setup table.

REFERENCES    

MODIFICATION HISTORY
    $Log:  36704: GUIdataGrid_Classes.cs 

   Rev 1.38    05/12/2007 21:26:42  ak
 TC keywords added to source for version control.


*******************************************************************************/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
//using System.IO;
using System.Data; 

namespace DriveWizard
{
	#region enumerated types
	public enum TblCols 
	{
		param, actValue, Index, sub, defVal, lowVal, highVal,  units, accessType, 
		accessLevel, NodeID, odSub, Monitor};
	public enum tableStyleColumns {param, actValue, units, Index, sub,defVal, lowVal, highVal,};
	public enum CANCommsCol {Manuf , Type, NodeID, Master, NMTState};
	public enum EmerCols {NodeID, Message};
	public enum devEmerCols {Message};
	public enum devActiveFaults { Description};
	public enum DCFCompCol {Index, sub, param, DCFValue, Value0, Value1, Value2, Value3, Value4, Value5, Value6, Value7};
	internal enum CounterCol {Name, Group,FirstTime, LastTime, Count, SaveBit};
	internal enum MasterCols {NamesList, eventIDList};
	internal enum OpLogCol {OpName, CustMin, CustMax, SCMin, SCMax};
	internal enum virtualSetupCols {nodeID,EDS, vendorID, productCode,revisionNo, Master}

	#endregion enumerated tpyes

	#region Classes for DCF Tables, Table Styles and Column Styles
	#region DataTable for DCF
	public class DCFDataTable : DataTable
	{
		public DCFDataTable()
		{
			this.Columns.Add(TblCols.Index.ToString(),typeof(System.String));
			this.Columns[TblCols.Index.ToString()].DefaultValue = "";  

			this.Columns.Add(TblCols.sub.ToString(),typeof(System.String));
			this.Columns[TblCols.sub.ToString()].DefaultValue = "";  

			this.Columns.Add(TblCols.param.ToString(), typeof(System.String));
			this.Columns[TblCols.param.ToString()].DefaultValue = "";  

			this.Columns.Add(TblCols.defVal.ToString(), typeof(System.String));
			this.Columns[TblCols.defVal.ToString()].DefaultValue = "";  

			this.Columns.Add(TblCols.lowVal.ToString(),  typeof(System.String));
			this.Columns[TblCols.lowVal.ToString()].DefaultValue = "";  
 
			this.Columns.Add(TblCols.highVal.ToString(),typeof(System.String));
			this.Columns[TblCols.highVal.ToString()].DefaultValue = "";  

			this.Columns.Add(TblCols.actValue.ToString(), typeof(System.String));
			this.Columns[TblCols.actValue.ToString()].DefaultValue = "";  

			this.Columns.Add(TblCols.units.ToString(), typeof(System.String));
			this.Columns[TblCols.units.ToString()].DefaultValue = "";  

			this.Columns.Add(TblCols.accessType.ToString(), typeof(DriveWizard.ObjectAccessType)); //non visible column
			this.Columns.Add(TblCols.accessLevel.ToString(), typeof(System.String)); //non visible column
		}
	}
	#endregion

	#region DCFtablestyle for comparison table
	public class DCFCompareTableStyle : SCbaseTableStyle
	{
		public DCFCompareTableStyle(string passed_mappingName, int [] ColWidths, ArrayList nodeIDS)
		{
			this.MappingName = passed_mappingName;
			this.AllowSorting = false;
			int NoCompDevices = ColWidths.Length - 4;
			createIndexCol(ColWidths);
			createSubIndexCol(ColWidths);
			createParamCol(ColWidths);
			createFileValueCol(ColWidths);

			if(ColWidths.Length >= 5)
			{
				createNodeCol1(ColWidths, nodeIDS[0]);
			}
			if(ColWidths.Length >= 6)
			{
				createNodeCol2(ColWidths, nodeIDS[1]);
			}
			if(ColWidths.Length >= 7)
			{
				createNodeCol3(ColWidths, nodeIDS[2]);
			}

			if(ColWidths.Length >= 8)
			{
				createNodeCol4(ColWidths, nodeIDS[3]);
			}

			if(ColWidths.Length >= 9)
			{
				createNodeCol5(ColWidths,nodeIDS[4]);
			}

			if(ColWidths.Length >= 10)
			{
				createNodeCol6(ColWidths, nodeIDS[5]);
			}

			if(ColWidths.Length >= 11)
			{
				createNodeCol7(ColWidths, nodeIDS[6]);
			}
			if(ColWidths.Length >= 12)
			{
				createNodeCol8(ColWidths, nodeIDS[7]);
			}
			//Call this to ensure corrent alignmentof 
			//RH aligned columns
			//work aound for known dotnet bug
			this.adjustRightAlignedColumns();
		}
		private void createIndexCol(int [] ColWidths)
		{
			DCFCompareTextBoxColumn IndexCol = new DCFCompareTextBoxColumn((int) DCFCompCol.Index);
			IndexCol.MappingName = DCFCompCol.Index.ToString();
			IndexCol.HeaderText = "Index";
			IndexCol.Width = ColWidths[(int) DCFCompCol.Index];
			IndexCol.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(IndexCol);
		}
		private void createSubIndexCol(int [] ColWidths)
		{
			DCFCompareTextBoxColumn subIndexCol = new DCFCompareTextBoxColumn((int) DCFCompCol.sub);
			subIndexCol.MappingName = DCFCompCol.sub.ToString();
			subIndexCol.HeaderText = "Sub";
			subIndexCol.Width = ColWidths[(int) DCFCompCol.sub];
			subIndexCol.Alignment = HorizontalAlignment.Right;
			subIndexCol.NullText = "";
			GridColumnStyles.Add(subIndexCol);
		}
		private void createParamCol(int [] ColWidths)
		{
			DCFCompareTextBoxColumn paramName = new DCFCompareTextBoxColumn((int) DCFCompCol.param);
			paramName.MappingName = DCFCompCol.param.ToString();
			paramName.HeaderText = "Parameter name";
			paramName.Width = ColWidths[(int) DCFCompCol.param];
			GridColumnStyles.Add(paramName);
		}
		private void createFileValueCol(int [] ColWidths)
		{
			DCFCompareTextBoxColumn col = new DCFCompareTextBoxColumn((int) DCFCompCol.DCFValue);
			col.MappingName = DCFCompCol.DCFValue.ToString();
			col.HeaderText = "DCF value";
			col.Width = ColWidths[(int) DCFCompCol.DCFValue];
			col.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(col);
		}
		private void createNodeCol1(int [] ColWidths, object passed_nodeID)
		{
			int nodeID = (int)passed_nodeID;
			DCFCompareTextBoxColumn colValue0 = new DCFCompareTextBoxColumn((int) DCFCompCol.Value0);
			colValue0.MappingName = DCFCompCol.Value0.ToString();
			colValue0.HeaderText = "Node ID " + nodeID.ToString();
			colValue0.Width = ColWidths[(int) DCFCompCol.Value0];
			colValue0.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(colValue0);
		}

		private void createNodeCol2(int [] ColWidths, object passed_nodeID)
		{
			int nodeID = (int)passed_nodeID;
			DCFCompareTextBoxColumn colValue1 = new DCFCompareTextBoxColumn((int) DCFCompCol.Value1);
			colValue1.MappingName = DCFCompCol.Value1.ToString();
			colValue1.HeaderText =  "Node ID " + nodeID.ToString();
			colValue1.Width = ColWidths[(int) DCFCompCol.Value1];
			colValue1.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(colValue1);
		}

		private void createNodeCol3(int [] ColWidths, object passed_nodeID)
		{
			int nodeID = (int)passed_nodeID;
			DCFCompareTextBoxColumn colValue2 = new DCFCompareTextBoxColumn((int) DCFCompCol.Value2);
			colValue2.MappingName = DCFCompCol.Value2.ToString();
			colValue2.HeaderText = "Node ID " + nodeID.ToString();
			colValue2.Width = ColWidths[(int) DCFCompCol.Value2];
			colValue2.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(colValue2);
		}
		private void createNodeCol4(int [] ColWidths, object passed_nodeID)
		{
			int nodeID = (int)passed_nodeID;
			DCFCompareTextBoxColumn colValue3 = new DCFCompareTextBoxColumn((int) DCFCompCol.Value3);
			colValue3.MappingName = DCFCompCol.Value3.ToString();
			colValue3.HeaderText =  "Node ID " + nodeID.ToString();
			colValue3.Width = ColWidths[(int) DCFCompCol.Value3];
			colValue3.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(colValue3);
		}
		private void createNodeCol5(int [] ColWidths,object passed_nodeID)
		{
			int nodeID = (int)passed_nodeID;
			DCFCompareTextBoxColumn colValue4 = new DCFCompareTextBoxColumn((int) DCFCompCol.Value4);
			colValue4.MappingName = DCFCompCol.Value4.ToString();
			colValue4.HeaderText =  "Node ID " + nodeID.ToString();
			colValue4.Width = ColWidths[(int) DCFCompCol.Value4];
			colValue4.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(colValue4);
		}
		private void createNodeCol6(int [] ColWidths, object passed_nodeID)
		{
			int nodeID = (int)passed_nodeID;
			DCFCompareTextBoxColumn colValue5 = new DCFCompareTextBoxColumn((int) DCFCompCol.Value5);
			colValue5.MappingName = DCFCompCol.Value5.ToString();
			colValue5.HeaderText =  "Node ID " + nodeID.ToString();
			colValue5.Width = ColWidths[(int) DCFCompCol.Value5];
			colValue5.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(colValue5);
		}
		private void createNodeCol7(int [] ColWidths, object passed_nodeID)
		{
			int nodeID = (int)passed_nodeID;
			DCFCompareTextBoxColumn colValue6 = new DCFCompareTextBoxColumn((int) DCFCompCol.Value6);
			colValue6.MappingName = DCFCompCol.Value6.ToString();
			colValue6.HeaderText = "Node ID " + nodeID.ToString();
			colValue6.Width = ColWidths[(int) DCFCompCol.Value6];
			colValue6.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(colValue6);
		}
		private void createNodeCol8(int [] ColWidths, object passed_nodeID)
		{
			int nodeID = (int)passed_nodeID;
			DCFCompareTextBoxColumn colValue7 = new DCFCompareTextBoxColumn((int) DCFCompCol.Value7);
			colValue7.MappingName = DCFCompCol.Value7.ToString();
			colValue7.HeaderText =  "Node ID " + nodeID.ToString();
			colValue7.Width = ColWidths[(int) DCFCompCol.Value7];
			colValue7.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(colValue7);
		}
	}
	#endregion DCFtablestyle for comparison table

	#region DCF Compare Text Box Column Class
	public class DCFCompareTextBoxColumn : SCbaseRODataGridTextBoxColumn
	{
		int compIndex;
		int columnIndex;
		public DCFCompareTextBoxColumn(int passed_columnIndex) : base(passed_columnIndex)
		{
			compIndex = passed_columnIndex -((int) DCFCompCol.Value0);
			columnIndex = passed_columnIndex;
		}
		//used to fire an event to retrieve formatting info and then draw the cell with this formatting info
		protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
		{
			if(columnIndex < 4)//(int) DCFCompCol.Value0)
			{
				#region use colours for DCF 'device'
				if((DriveWizard.MAIN_WINDOW.colArray != null) && (rowNum<DriveWizard.MAIN_WINDOW.colArray.Length)) //also takes care of zero length ColArray
				{
					foreBrush = new SolidBrush(MAIN_WINDOW.colArray[rowNum]);
					if(DriveWizard.MAIN_WINDOW.colArray[rowNum].ToString() == SCCorpStyle.headerRow.ToString())
					{
						backBrush = new SolidBrush(SCCorpStyle.headerRowBackcol);
					}
					else if (DriveWizard.MAIN_WINDOW.colArray[rowNum].ToString() == SCCorpStyle.readOnly.ToString())
					{
						backBrush = new SolidBrush(SCCorpStyle.readOnlyRowBackCol);
					}
				}
				base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
				#endregion use colours for DCF 'device'
			}
			else
			{
				#region use colurs sourced form the connecte device
				if(DriveWizard.MAIN_WINDOW.compColArray != null) 
				{
					foreBrush = new SolidBrush(MAIN_WINDOW.compColArray[compIndex,rowNum]);

					if(DriveWizard.MAIN_WINDOW.compColArray[compIndex,rowNum].ToString() == SCCorpStyle.headerRow.ToString())
					{
						backBrush = new SolidBrush(SCCorpStyle.headerRowBackcol);
					}
					else if (DriveWizard.MAIN_WINDOW.compColArray[compIndex,rowNum].ToString() == SCCorpStyle.readOnly.ToString())
					{
						backBrush = new SolidBrush(SCCorpStyle.readOnlyRowBackCol);
					}
				}
				base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
				#endregion use colurs sourced form the connecte device
			}
		}
	}

	#endregion DCF Compare Text Box Column Class
	#endregion Classes for DCF Tables, Table Styles and column Styles

	#region Classes for Main Data Table, Table Style and column Styles
	
	#region DataTable for OD Data Monitoring 
	public class DWdatatable : DataTable
	{
		public DWdatatable()
		{
			//param, actValue, Index, sub,  PDOMap, defVal, lowVal, highVal,  units,
			this.Columns.Add(TblCols.param.ToString(), typeof(System.String));
			this.Columns[TblCols.param.ToString()].DefaultValue = "";

			this.Columns.Add(TblCols.actValue.ToString(),typeof(System.String));
			this.Columns[TblCols.actValue.ToString()].DefaultValue = "";

			this.Columns.Add(TblCols.Index.ToString(),typeof(System.String));
			this.Columns[TblCols.Index.ToString()].DefaultValue = "";

			this.Columns.Add(TblCols.sub.ToString(),typeof(System.String));
			this.Columns[TblCols.sub.ToString()].DefaultValue = "";

			this.Columns.Add(TblCols.defVal.ToString(), typeof(System.String));
			this.Columns[TblCols.defVal.ToString()].DefaultValue = "";

			this.Columns.Add(TblCols.lowVal.ToString(),  typeof(System.String));
			this.Columns[TblCols.lowVal.ToString()].DefaultValue = "";

			this.Columns.Add(TblCols.highVal.ToString(),typeof(System.String));
			this.Columns[TblCols.highVal.ToString()].DefaultValue = "";

			this.Columns.Add(TblCols.units.ToString(), typeof(System.String));
			this.Columns[TblCols.units.ToString()].DefaultValue = "";
			// accessType, accessLevel, displayType, sectionType, objectType, scaling, 
			this.Columns.Add(TblCols.accessType.ToString(), typeof(System.String)); //non visible column
			this.Columns[TblCols.accessType.ToString()].DefaultValue = "";

			this.Columns.Add(TblCols.accessLevel.ToString(), typeof(System.String)); //non visible column
			this.Columns[TblCols.accessLevel.ToString()].DefaultValue = "0";

			this.Columns.Add(TblCols.NodeID.ToString(), typeof(System.Int16)); //non visible column - used to store non-truncated controller value
			this.Columns[TblCols.NodeID.ToString()].DefaultValue = 0;

			this.Columns.Add( TblCols.odSub.ToString(), typeof(DriveWizard.ODItemData));
			this.Columns[TblCols.odSub.ToString()].DefaultValue = null;

			this.Columns.Add( TblCols.Monitor.ToString(), typeof(System.Boolean));
			this.Columns[TblCols.Monitor.ToString()].DefaultValue = false;

			DataColumn[] keys = new DataColumn[3];
			keys[0] = this.Columns[TblCols.Index.ToString()];
			keys[1] = this.Columns[TblCols.sub.ToString()];
			this.PrimaryKey = keys;
		}
	}
	#endregion

	#region TableStyle for Personality Parameter setting (text)
	public class PPTableStyle : SCbaseTableStyle
	{
		public PPTableStyle(string tableName, int [] ColWidths, bool PreOpInvoked, uint passed_Access)
		{
			this.MappingName = tableName;

			#region param name column - all levels
			PPFormattableTextBoxColumn paramName = new PPFormattableTextBoxColumn((int) tableStyleColumns.param,PreOpInvoked, passed_Access);
			paramName.MappingName = TblCols.param.ToString();
			paramName.HeaderText = "Parameter name";
			paramName.Width = ColWidths[(int) tableStyleColumns.param];
			paramName.ReadOnly = true;
			GridColumnStyles.Add(paramName);
			#endregion param name column

			#region actual value column
			PPFormattableTextBoxColumn actualValue = new PPFormattableTextBoxColumn((int) tableStyleColumns.actValue, PreOpInvoked, passed_Access);
			actualValue.MappingName = TblCols.actValue.ToString();
			actualValue.HeaderText = "Value";
			actualValue.Width =ColWidths[(int) tableStyleColumns.actValue];
			actualValue.Alignment = HorizontalAlignment.Right;
			actualValue.ReadOnly = false;
			GridColumnStyles.Add(actualValue);
			#endregion actual value column

			#region units column
			PPFormattableTextBoxColumn units = new PPFormattableTextBoxColumn((int) tableStyleColumns.units, PreOpInvoked, passed_Access);
			units.MappingName = TblCols.units.ToString();  
			units.HeaderText = "Units";
			units.Width = ColWidths[(int) tableStyleColumns.units];
			units.ReadOnly = true;
			GridColumnStyles.Add(units);
			#endregion units column

			if(passed_Access>=3)
			{
				#region index column
				PPFormattableTextBoxColumn IndexCol = new PPFormattableTextBoxColumn((int) tableStyleColumns.Index, PreOpInvoked, passed_Access);
				IndexCol.MappingName = TblCols.Index.ToString();
				IndexCol.HeaderText = "Index";
				IndexCol.Width = ColWidths[(int) tableStyleColumns.Index];
				IndexCol.Alignment = HorizontalAlignment.Right;
				IndexCol.ReadOnly = true;
				GridColumnStyles.Add(IndexCol);
				#endregion index column

				#region sub-index column
				PPFormattableTextBoxColumn subIndexCol = new PPFormattableTextBoxColumn((int) tableStyleColumns.sub, PreOpInvoked, passed_Access);
				subIndexCol.MappingName = TblCols.sub.ToString();
				subIndexCol.HeaderText = "Sub";
				subIndexCol.Width = ColWidths[(int) tableStyleColumns.sub];
				subIndexCol.Alignment = HorizontalAlignment.Right;
				GridColumnStyles.Add(subIndexCol);
				subIndexCol.ReadOnly = true;
				#endregion sub-index column
			}
			if(passed_Access >=5)
			{
				#region default value column
				PPFormattableTextBoxColumn defaultValue = new PPFormattableTextBoxColumn((int) tableStyleColumns.defVal, PreOpInvoked, passed_Access);
				defaultValue.MappingName = TblCols.defVal.ToString();
				defaultValue.HeaderText = "Default";
				defaultValue.Width = ColWidths[(int) tableStyleColumns.defVal];
				defaultValue.Alignment = HorizontalAlignment.Right;
				defaultValue.ReadOnly = true;
				GridColumnStyles.Add(defaultValue);
				#endregion default value column
				
				#region min value column
				PPFormattableTextBoxColumn minValue =  new PPFormattableTextBoxColumn((int) tableStyleColumns.lowVal, PreOpInvoked, passed_Access);
				minValue.MappingName = TblCols.lowVal.ToString();
				minValue.HeaderText = "Min";
				minValue.Width = ColWidths[(int) tableStyleColumns.lowVal];
				minValue.Alignment = HorizontalAlignment.Right;
				minValue.ReadOnly = true;
				GridColumnStyles.Add(minValue);
				#endregion min value column
				
				#region max value column
				PPFormattableTextBoxColumn maxValue = new PPFormattableTextBoxColumn((int) tableStyleColumns.highVal, PreOpInvoked, passed_Access);
				maxValue.MappingName = TblCols.highVal.ToString();
				maxValue.HeaderText = "Max";
				maxValue.Width = ColWidths[(int) tableStyleColumns.highVal];
				maxValue.Alignment = HorizontalAlignment.Right;
				maxValue.ReadOnly = true;
				GridColumnStyles.Add(maxValue);
				#endregion max value column
			}
			//this method should alwayys be calle dat the end of the constructor		
			//work around for known dotnet bug
			adjustRightAlignedColumns();
		}
	}
	#endregion
	
	#region PP Formattable TextBox Column
	//This class is overridden to allow both text box and Combo box style entry of values. 
	//Combo box is suitable for enumerated types
	// text entry is used for all others
	public class PPFormattableTextBoxColumn : DataGridTextBoxColumn
	{
		private ComboBox enumCombo = null;
		private bool _bIsComboBound = false; // remember if combobox is bound to datagrid
		private int _iRowNum = 0;  //the row number
		private CurrencyManager _cmSource = null;
		public bool _preOpInvoked = false;
		private uint _systemAccess = 0;
		private int _colNum = 0;
		public PPFormattableTextBoxColumn(int colNum, bool PreOpInvoked, uint systemAccess)
		{
			_colNum = colNum;
			_preOpInvoked = PreOpInvoked;
			_systemAccess = systemAccess;
			if(this.ReadOnly == false)
			{
				enumCombo = new ComboBox();
				enumCombo.DropDownStyle = ComboBoxStyle.DropDownList;
				enumCombo.Visible = false;
			}
		}
		protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
		{
			if((DriveWizard.MAIN_WINDOW.colArray != null) && (rowNum<DriveWizard.MAIN_WINDOW.colArray.Length)) //also takes care of zero length ColArray
			{
				if(DriveWizard.MAIN_WINDOW.colArray[rowNum].ToString() == SCCorpStyle.headerRow.ToString())
				{
					backBrush = new SolidBrush(SCCorpStyle.headerRowBackcol);
					if(_colNum != (int) tableStyleColumns.Index)
					{ //we overpaint the index number in th eheader row
						foreBrush = new SolidBrush(MAIN_WINDOW.colArray[rowNum]);
					}
					else
					{
						foreBrush = new SolidBrush(SCCorpStyle.headerRowBackcol);
					}
				}
				else if (DriveWizard.MAIN_WINDOW.colArray[rowNum].ToString() == SCCorpStyle.readOnly.ToString())
				{
					backBrush = new SolidBrush(SCCorpStyle.readOnlyRowBackCol);
					foreBrush = new SolidBrush(MAIN_WINDOW.colArray[rowNum]);
				}
				else if((DriveWizard.MAIN_WINDOW.currTblIndex!=MAIN_WINDOW.GraphTblIndex) 
					&& (DriveWizard.MAIN_WINDOW.currTblIndex!=MAIN_WINDOW.DCFTblIndex))
				{

					if(_colNum != (int) tableStyleColumns.actValue)
					{
						foreBrush = new SolidBrush(MAIN_WINDOW.colArray[rowNum]);
						backBrush = new SolidBrush(Color.WhiteSmoke);  //judetemp
					}
					else
					{
						foreBrush = new SolidBrush(MAIN_WINDOW.colArray[rowNum]);
						if(MAIN_WINDOW.canWriteNow[rowNum] == true)
						{
							backBrush = new SolidBrush(Color.LightCyan);  //judetemp
						}
						else
						{
							backBrush = new SolidBrush(Color.WhiteSmoke);  //judetemp
						}
					}
				}
				if((DriveWizard.MAIN_WINDOW.currTblIndex==MAIN_WINDOW.GraphTblIndex) 
					||(DriveWizard.MAIN_WINDOW.currTblIndex==MAIN_WINDOW.DCFTblIndex))
				{
					if(DriveWizard.MAIN_WINDOW.colArray[rowNum].ToString() != SCCorpStyle.headerRow.ToString())
					{
						backBrush = new SolidBrush(Color.WhiteSmoke); 
					}
				}
				else if(DriveWizard.MAIN_WINDOW.colArray[rowNum].ToString() == SCCorpStyle.dgRowSelected.ToString())
				{
					backBrush = new SolidBrush(SCCorpStyle.dgRowSelected);
					foreBrush = new SolidBrush(SCCorpStyle.dgForeColour);
				}
			}
			base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			if((DriveWizard.MAIN_WINDOW.currTblIndex==MAIN_WINDOW.GraphTblIndex) || (DriveWizard.MAIN_WINDOW.currTblIndex==MAIN_WINDOW.DCFTblIndex))
			{
				return;
			}
			DataRowView myView = (DataRowView) source.Current;
			_iRowNum = rowNum;
			_cmSource = source;
			if(MAIN_WINDOW.UserInputInhibit == true)
			{
				return;
			}
			if((this.ReadOnly == true) ||(MAIN_WINDOW.canWriteNow[rowNum] ==false))
			{
				//judetempcalculateNewDGCell(myView, rowNum);
				return;
			}
			ODItemData odSub = (ODItemData) myView.Row[(int) TblCols.odSub];
			CANopenDataType dataType = (CANopenDataType)odSub.dataType;
			if((odSub.format == SevconNumberFormat.SPECIAL) || (dataType == CANopenDataType.BOOLEAN))
			{
				#region handle enumerated types
				this.ReadOnly = true;  //make this column at this row read only since we are placing combo on top
				#region one time only settings
				// navigation path to the datagrid only exists after the column is added to the Styles
				if (_bIsComboBound == false) 
				{
					_bIsComboBound = true; //set the indicator 
					enumCombo.Font = this.TextBox.Font;				// synchronize the font size to the text box
					this.DataGridTableStyle.DataGrid.Controls.Add(enumCombo); // and bind combo to its datagrid 
					enumCombo.Leave +=new EventHandler(enumCombo_Leave); 
					enumCombo.SelectionChangeCommitted +=new EventHandler(enumCombo_SelectionChangeCommitted);
					enumCombo.MouseLeave +=new EventHandler(enumCombo_MouseLeave);
				}
				#endregion one time only settings
				#region create combo drop down list for this item
				string [] enumStrs = odSub.formatList.Split(':');
				long [] enumValues = new long[enumStrs.Length];
				ArrayList comboData = new ArrayList()    ;
				for (int i = 0;i<enumValues.Length;i++)
				{
					string textOnly = "Invalid enum";
					long enumVal = 0;
					int indexEquals = enumStrs[i].IndexOf("=");
					if((indexEquals != -1) && (indexEquals+1)<(enumStrs[i].Length-1))
					{
						textOnly = enumStrs[i].Substring(indexEquals + 1); 
						string enumString = enumStrs[i].Substring(0,indexEquals).Trim();
						if(enumString.ToUpper().IndexOf("0X") != -1)
						{
							//hex
							enumVal = System.Convert.ToInt64(enumString, 16);
						}
						else
						{
							//base 10
							enumVal = System.Convert.ToInt64(enumString);
						}
					}
					comboData.Add(new comboSource(textOnly, enumVal));
				}
				enumCombo.DataSource = comboData;
				enumCombo.DisplayMember = "enumStr";
				enumCombo.ValueMember = "enumValue";
				#endregion create combo drop down list for this item
				enumCombo.Bounds = bounds;  //size combobox to current grid cell size
				enumCombo.SelectedIndex = 0;
				enumCombo.BeginUpdate();  //suspend cell painting
				enumCombo.Visible = true;
				enumCombo.EndUpdate(); //resume cell painting
				enumCombo.Focus();
				this.ReadOnly = false;//reset
				#endregion handle enumerated types
			}
			else  //not boolean or enumerated
			{
				base.Edit(source, rowNum, bounds, this.ReadOnly,instantText,true);
			}
			//judetempcalculateNewDGCell(myView, rowNum);
		}
		private void calculateNewDGCell(DataRowView myRowView, int currRowNum)
		{
			int cellRow = 1000;
			for (int rowInd =currRowNum;rowInd<myRowView.DataView.Count;rowInd++)
			{
				if(MAIN_WINDOW.canWriteNow[rowInd] == true)
				{
					cellRow = rowInd;
				}
				else
				{
					rowInd++;
				}
				if(cellRow != 1000)
				{
					break;  //comeout of the loop
				}
			}
			if( cellRow != 1000)  //no writeable cells found in current view
			{
				this.DataGridTableStyle.DataGrid.CurrentCell = new DataGridCell(cellRow,1);  
			}
		}
		private void enumCombo_Leave(object sender, EventArgs e)
		{
			if(enumCombo.Visible== true) //we were editing a combo 
			{
				this.ReadOnly = false;//reset
				enumCombo.Visible = false;
				this.EndEdit();  //nedded otherwise clickign back on the same cell prevent re-entry into edit method
			}
		}

		private void enumCombo_SelectionChangeCommitted(object sender, EventArgs e)
		{
			#region extract correct text value
			ArrayList _comboData = new ArrayList();
			_comboData = (ArrayList) enumCombo.DataSource;
			object objValue = _comboData[enumCombo.SelectedIndex].ToString();
			#endregion extract correct text value
			try
			{
				this.SetColumnValueAtRow(_cmSource, _iRowNum, objValue);
			}
			catch(Exception ex)
			{
				//TODO
#if DEBUG
				Message.Show(ex.Message);
#endif

			}
			enumCombo.Visible = false;
			if(enumCombo.Visible== true) //we were editing a combo 
			{
				this.ReadOnly = false;//reset
				enumCombo.Visible = false;
				this.EndEdit();  //nedded otherwise clickign back on the same cell prevent re-entry into edit method
			}
		}
		private void enumCombo_MouseLeave(object sender, EventArgs e)
		{
			//this is needed to prevent secario where user leaves combo 
			//and then say switches to operational which should render the cell non-editable
			//unfortunately if the combo is ALREADY visible then the combo handlers are called
			//and edit method is bypassed ( since user clicked on combo NOT cell)
			//By forcing the combo to 'disappear' whenever user moves mouse outside its bounds
			// we can easilt prevent this scenario with no adverse affects
			//It is not easily possible to make the combo magically appear whenever the mouse heover sover it
			//At this time the combo is not visilbe so combo Mouse Enter event would never be called
			//We could make the coumn boounds static and handle entering this are but this would get messy quickly 
			//The column class has no mouse events since it is not physically drawn on the screen
			//Possbily we could use mouse enter on data grid and chekc if enumerated tpye there. Add
			//feature request to TC
			enumCombo.Visible = false;
		}

	}
	#endregion

	#region combo class
	public class comboSource
	{
		private string _enumStr ;
		private long _enumValue ;
    
		public  comboSource(string enumStr, long enumValue)
		{
			this._enumStr = enumStr;
			this._enumValue = enumValue;
		}
		public string enumStr
		{
			get
			{
				return _enumStr;
			}
		}

		public long enumValue
		{
        
			get
			{
				return _enumValue ;
			}
		}

		public override string ToString()
		{
			return this.enumStr;
		}
	}
	#endregion combo class
	#endregion Classes for Main Data Table, Table Style and column Styles

	#region Counters Log DataTable
	public class CountersTable : DataTable
	{
		public CountersTable(string name)
		{
			this.TableName = name;
			this.Columns.Add(CounterCol.Name.ToString(),typeof(System.String));
			this.Columns.Add(CounterCol.Group.ToString(), typeof(System.String));
			this.Columns.Add(CounterCol.FirstTime.ToString(),typeof(System.String));
			this.Columns.Add(CounterCol.LastTime.ToString(), typeof(System.String));
			this.Columns.Add(CounterCol.Count.ToString(),typeof(System.String));
			this.Columns.Add(CounterCol.SaveBit.ToString(),typeof(System.Boolean));
		}
	}
	#endregion

	#region master counterTbl
	public class MasterTable : DataTable
	{
		public MasterTable()
		{
			this.Columns.Add(MasterCols.NamesList.ToString(), typeof(System.String));
			this.Columns.Add(MasterCols.eventIDList.ToString(),typeof(System.UInt16));
		}
	}
	#endregion master counterTbl

	#region Counters Log TableStyle
	public class TSCounters : SCbaseTableStyle
	{
		public TSCounters (int [] ColWidths, MasterTable mastertable, string mappingName, bool viewOnly, string [] defaultNames)
		{
			this.ReadOnly = true;
			this.MappingName = mappingName;
			if(viewOnly == false)
			{
				#region read only
				CCComboColumn c0 = new  CCComboColumn(mastertable, MasterCols.NamesList.ToString(), MasterCols.eventIDList.ToString(), defaultNames);
				c0.MappingName = CounterCol.Name.ToString();
				c0.HeaderText = "Event Name";
				c0.Width = ColWidths[(int)CounterCol.Name];
				c0.NullText = "";
				GridColumnStyles.Add(c0);
				#endregion
			}
			else
			{
				SCbaseRODataGridTextBoxColumn c0 = new SCbaseRODataGridTextBoxColumn((int)CounterCol.Name);
				c0.MappingName = CounterCol.Name.ToString();
				c0.HeaderText = "Event Name";
				c0.Width = ColWidths[(int)CounterCol.Name];
				c0.NullText = "EMPTY SLOT";
				GridColumnStyles.Add(c0);
			}

			SCbaseRODataGridTextBoxColumn c1 = new SCbaseRODataGridTextBoxColumn((int)CounterCol.Group);
			c1.MappingName = CounterCol.Group.ToString();
			c1.HeaderText = "Group";
			c1.Width = ColWidths[(int)CounterCol.Group];
			c1.NullText = "";
			GridColumnStyles.Add(c1);

			SCbaseRODataGridTextBoxColumn firstTimeCol = new SCbaseRODataGridTextBoxColumn((int)CounterCol.FirstTime);
			firstTimeCol.MappingName = CounterCol.FirstTime.ToString();
			firstTimeCol.HeaderText = "First Time"; 
			firstTimeCol.NullText = "";
			firstTimeCol.Width = ColWidths[(int)CounterCol.FirstTime];
			firstTimeCol.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(firstTimeCol);

			SCbaseRODataGridTextBoxColumn EvLastTimeCol =  new SCbaseRODataGridTextBoxColumn((int)CounterCol.LastTime);
			EvLastTimeCol.MappingName = CounterCol.LastTime.ToString();
			EvLastTimeCol.HeaderText = "Last Time";
			EvLastTimeCol.Width = ColWidths[(int)CounterCol.LastTime];
			EvLastTimeCol.NullText = "";
			EvLastTimeCol.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(EvLastTimeCol);

			SCbaseRODataGridTextBoxColumn EvCountCol = new SCbaseRODataGridTextBoxColumn((int)CounterCol.Count);
			EvCountCol.MappingName = CounterCol.Count.ToString();
			EvCountCol.HeaderText = "Count";
			EvCountCol.NullText = "";
			EvCountCol.Width = ColWidths[(int)CounterCol.Count];
			EvCountCol.Alignment = HorizontalAlignment.Right;
			GridColumnStyles.Add(EvCountCol);

			SCbaseRODataGridBoolColumn saveBitCol = new SCbaseRODataGridBoolColumn((int)CounterCol.SaveBit);
			saveBitCol.MappingName = CounterCol.SaveBit.ToString();
			saveBitCol.HeaderText = "SaveBit";
			saveBitCol.Width = ColWidths[(int)CounterCol.SaveBit];
			saveBitCol.AllowNull = false;
			saveBitCol.NullValue = false;
			GridColumnStyles.Add(saveBitCol);

			//call this to force correct aliognment of any Right aligned columns
			this.adjustRightAlignedColumns();
		}
	}
	#endregion

	#region Establish CAN Comms Data Table
	public class EstCommsTable : DataTable
	{
		public EstCommsTable()
		{
			this.Columns.Add(CANCommsCol.Manuf.ToString(), typeof(System.String));// Add the Column to the table //ie pump, LH controller etc
			this.Columns[CANCommsCol.Manuf.ToString()].DefaultValue = "Unknown";  
			this.Columns.Add(CANCommsCol.Type.ToString(), typeof(System.String));// Add the column to the table. Node baud rate
			this.Columns[CANCommsCol.Manuf.ToString()].DefaultValue = "Unknown";  
			this.Columns.Add(CANCommsCol.NodeID.ToString(), typeof(System.String));// Add the column to the table. 
			this.Columns[CANCommsCol.Manuf.ToString()].DefaultValue = "Unknown";  
			this.Columns.Add(CANCommsCol.Master.ToString(), typeof(System.String));// Add the column to the table. 
			this.Columns[CANCommsCol.Manuf.ToString()].DefaultValue = "slave";  
			this.Columns.Add(CANCommsCol.NMTState.ToString(), typeof(System.String));// Add the column to the table. 
			this.Columns[CANCommsCol.Manuf.ToString()].DefaultValue = "Unknown";  
		}
	}
	#endregion

	#region Establish CAN Comms Table Style
	public class CANCommsTableStyle : SCbaseTableStyle
	{
		float [] percents = {0.25F, 0.35F, 0.1F, 0.15F, 0.15F};
		public CANCommsTableStyle (int [] ColWidths, bool fullCols)
		{
			//table style level parameters
			this.AllowSorting = false;
			//Tag this instance with the column percentages for this table style
			columnPercents = new float[percents.Length];
			percents.CopyTo(this.columnPercents,0);
			//			this.calculateColumnWidths(500); 
			SCbaseRODataGridTextBoxColumn c1 = new SCbaseRODataGridTextBoxColumn((int) (CANCommsCol.Manuf));
			c1.MappingName = CANCommsCol.Manuf.ToString();
			c1.HeaderText = "Manufacturer";
			c1.Width = ColWidths[(int) (CANCommsCol.Manuf)];
			GridColumnStyles.Add(c1);

			SCbaseRODataGridTextBoxColumn c2 = new SCbaseRODataGridTextBoxColumn((int) (CANCommsCol.Type));
			c2.MappingName = CANCommsCol.Type.ToString();
			c2.HeaderText = "Node Type";
			c2.Width = ColWidths[(int) (CANCommsCol.Type)];
			GridColumnStyles.Add(c2);

			SCbaseRODataGridTextBoxColumn c3 = new SCbaseRODataGridTextBoxColumn((int) (CANCommsCol.NodeID));
			c3.MappingName = CANCommsCol.NodeID.ToString();
			c3.HeaderText = "Node ID"; 
			c3.Width = ColWidths[(int) (CANCommsCol.NodeID)];
			GridColumnStyles.Add(c3);

			if(fullCols == true)
			{
				SCbaseRODataGridTextBoxColumn c4 = new SCbaseRODataGridTextBoxColumn((int) (CANCommsCol.NodeID));
				c4.MappingName = CANCommsCol.Master.ToString();
				c4.HeaderText = "Master/Slave"; 
				c4.Width = ColWidths[(int) (CANCommsCol.Master)];
				GridColumnStyles.Add(c4);

				SCbaseRODataGridTextBoxColumn c5 = new SCbaseRODataGridTextBoxColumn((int) (CANCommsCol.NodeID));
				c5.MappingName = CANCommsCol.NMTState.ToString();
				c5.HeaderText = "NMT State"; 
				c5.Width = ColWidths[(int) (CANCommsCol.NMTState)];
				GridColumnStyles.Add(c5);
			}
		}
	}

	#endregion

	#region SCBaseComboColumn: DataGridTextBoxColumn 
	public class SCBaseComboColumn: DataGridTextBoxColumn 
	{
		protected ComboBox enumCombo = null;
		protected bool _bIsComboBound = false; // remember if combobox is bound to datagrid
		protected int _iRowNum = 0;  //the row number
		protected CurrencyManager _cmSource = null;
		public bool _preOpInvoked = false;
		protected int _colNum = 0;
		protected ArrayList _objSource;
		public SCBaseComboColumn(int colNum, object objSource)
		{
			_colNum = colNum;
			_objSource = (ArrayList) objSource;
			if(this.ReadOnly == false)
			{
				enumCombo = new ComboBox();
				//inherited classes need to set the drop down style
				enumCombo.Visible = false;
			}
		}
		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			DataRowView myView = (DataRowView) source.Current;
			_iRowNum = rowNum;
			_cmSource = source;
			this.ReadOnly = true;  //make this column at this row read only since we are placing combo on top
				#region one time only settings
				// navigation path to the datagrid only exists after the column is added to the Styles
				if (_bIsComboBound == false) 
				{
					_bIsComboBound = true; //set the indicator 
					enumCombo.Font = this.TextBox.Font;				// synchronize the font size to the text box
					this.DataGridTableStyle.DataGrid.Controls.Add(enumCombo); // and bind combo to its datagrid 
					enumCombo.Leave +=new EventHandler(enumCombo_Leave); 
					enumCombo.SelectionChangeCommitted +=new EventHandler(enumCombo_SelectionChangeCommitted);
					enumCombo.MouseLeave +=new EventHandler(enumCombo_MouseLeave);
				}
				#endregion one time only settings
				#region create combo drop down list for this item
			enumCombo.Bounds = bounds;  //size combobox to current grid cell size
			ArrayList tempAL = (ArrayList) _objSource;
			Graphics gc = enumCombo.CreateGraphics();
			int reqWidth = 0;
			for(int i = 0;i<tempAL.Count;i++)
			{
				comboSource src = (comboSource) tempAL[i];
				SizeF temp = gc.MeasureString(src.enumStr, enumCombo.Font);
				//required width is the max of the widest string and the datagrid cell width
				reqWidth = Math.Max((int)temp.Width+17,reqWidth);//add in scrollbar allowance
				reqWidth = Math.Max(bounds.Width,reqWidth);
			}
			//prevent combo trying to extend beyond the datagrid
			enumCombo.Width = Math.Min(reqWidth, 
				this.DataGridTableStyle.DataGrid.ClientRectangle.Right-enumCombo.Left-17);  //ADD SCROLLBAR ALLOWANCE??
			enumCombo.DataSource = (ArrayList) _objSource;
			enumCombo.DisplayMember = "enumStr";
			enumCombo.ValueMember = "enumValue";

				#endregion create combo drop down list for this item
				enumCombo.SelectedIndex = 0;
				enumCombo.BeginUpdate();  //suspend cell painting
				enumCombo.Visible = true;
				enumCombo.EndUpdate(); //resume cell painting
				enumCombo.Focus();
				this.ReadOnly = false;//reset
		}
		protected void enumCombo_Leave(object sender, EventArgs e)
		{
			if(enumCombo.Visible== true) //we were editing a combo 
			{
				this.ReadOnly = false;//reset
				enumCombo.Visible = false;
				this.EndEdit();  //needed otherwise clickign back on the same cell prevent re-entry into edit method
			}
		}
		protected void enumCombo_SelectionChangeCommitted(object sender, EventArgs e)
		{
			//we call a seperate method here because we are not allowed to override event handlers
			//if we just add a new event handler in the inherited class then it 'masks' this one which is
			//then never called
			comboSource srceVal = (comboSource) _objSource[enumCombo.SelectedIndex];
			this.SetColumnValueAtRow(_cmSource, _iRowNum, srceVal.enumValue);
			enumCombo.Visible = false;
			if(enumCombo.Visible== true) //we were editing a combo 
			{
				this.ReadOnly = false;//reset
				enumCombo.Visible = false;
				this.EndEdit();  //nedded otherwise clickign back on the same cell prevent re-entry into edit method
			}
		}
		protected void enumCombo_MouseLeave(object sender, EventArgs e)
		{
			//this is needed to prevent secario where user leaves combo 
			//and then say switches to operational which should render the cell non-editable
			//unfortunately if the combo is ALREADY visible then the combo handlers are called
			//and edit method is bypassed ( since user clicked on combo NOT cell)
			//By forcing the combo to 'disappear' whenever user moves mouse outside its bounds
			// we can easilt prevent this scenario with no adverse affects
			//It is not easily possible to make the combo magically appear whenever the mouse heover sover it
			//At this time the combo is not visilbe so combo Mouse Enter event would never be called
			//We could make the coumn boounds static and handle entering this are but this would get messy quickly 
			//The column class has no mouse events since it is not physically drawn on the screen
			//Possbily we could use mouse enter on data grid and chekc if enumerated tpye there. Add
			//feature request to TC
			enumCombo.Visible = false;
		}
	}

	#endregion SCBaseComboColumn: DataGridTextBoxColumn 

	#region ComboColumn CLass
	public class CCComboColumn : DataGridTextBoxColumn 
	{
		// each column shares a single combobox
		private ComboBox _cboColumn;
		// data we save when the column is instantiated (no provision for subsequent rebinds)
		private object _objSource;
		private string _strMember;
		private string _strValue;
		private string [] _defaultNames;
		// remember if we have bound the combobox to the parent datagrid control
		private bool _bIsComboBound = false;
		// data that describes the background and foreground colors used to paint the cell when not in edit mode
		private Brush _backBrush = null;
		private Brush _foreBrush = null;
		private DataRowView _DVSource = null;
		private int _rowNum = 0;
		// information picked up and held when we start to edit the source table
		private int _iRowNum;
		private CurrencyManager _cmSource;

		/// <summary>
		/// initialize the combobox column and take note of the data source/member/value used to fill the combobox
		/// </summary>
		/// <param name="objSource">bind Source for the combobox (typical is a DataTable object)</param>
		/// <param name="strMember">bind for the combobox DisplayMember (typical is a Column Name within the Source)</param>
		/// <param name="strValue">bind for the combobox ValueMember (typical is a Column Name within the Source)</param>
		public CCComboColumn(object objSource, string strMember, string strValue, string [] defaultNames)
		{
			_objSource = objSource;
			_strMember = strMember;
			_strValue = strValue;

			// create a new combobox object
			_cboColumn = new ComboBox();
			// set the data link to the source, member and value displayed by this combobox
			_cboColumn.DataSource = _objSource;
			_cboColumn.DisplayMember = _strMember;
			_cboColumn.ValueMember = _strValue;
			// we cannot create new countries through this column so disallow editing by making the combo a drop-down list
			_cboColumn.DropDownStyle = ComboBoxStyle.DropDownList;
			// Setting ReadOnly changes the behavior of the column so the 'leave' event fires whenever we 
			// change cell. The default behavior will not fire the 'leave' event when we up-arrow or 
			// down-arrow to the next row.
			this.ReadOnly = true;
			// we need to know when the combo box is getting closed so we can update the source data and
			// hide the combobox control
			_cboColumn.Leave += new EventHandler(cboColumn_Leave);
			_cboColumn.SelectionChangeCommitted += new EventHandler(cboColumn_ChangeCommit);

			// make sure the combobox is invisible until we've set its correct position and dimensions
			_cboColumn.Visible = false;
			//copy
			_defaultNames = new string[defaultNames.Length];
			defaultNames.CopyTo(_defaultNames, 0);
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool viewOnly, string instantText, bool cellIsVisible)
		{
			_DVSource = (DataRowView) source.Current;
			_rowNum = rowNum;
			// the navigation path to the datagrid only exists after the column is added to the Styles
			if (_bIsComboBound == false) 
			{
				_bIsComboBound = true;
				// important step here if we want to properly handle key events! the next step cannot 
				// be performed until the object is bound to the DataGrid (or an Exception must occur)
				this.DataGridTableStyle.DataGrid.Controls.Add(_cboColumn);
			}
			// this data is used when the combo box loses focus
			_iRowNum = rowNum;
			_cmSource = source;
			// synchronize the font size to the text box
			_cboColumn.Font = this.TextBox.Font;
			// we need to retrieve the current value and use this to set the combo box ahead of displaying it
			object tempObject = this.GetColumnValueAtRow(source, rowNum);
			// set the combobox to the dimensions of the cell (do this each time because the user may have resized this column)
			_cboColumn.Bounds = bounds;
			// do not paint the control until we've set the correct position in the items list
			_cboColumn.BeginUpdate();
			// note: on the very first time this routine is called you MUST set the column as visible 
			// ahead of setting a position in the items collection. otherwise the combobox will not be
			// populated and the call to set the SelectedValue cannot succeed
			_cboColumn.Visible = true;
			// use the object to set the combobox. the null detection is primarily aimed at the addition of a
			// new row (where it is possible a default column-row content has not been defined)
			if (tempObject.GetType() != typeof(System.DBNull)) 
			{
				_cboColumn.SelectedValue = tempObject;
			} 
			else 
			{
				_cboColumn.SelectedIndex = 0;
			}
			// we've set the combobox so we can now paint the control and move focus onto it
			_cboColumn.EndUpdate();
			_cboColumn.Focus();
		}

		public void cboColumn_Leave(object sender, EventArgs e) 
		{
			// We are going to write back the ValueMember from combobox into the current column-row in the 
			// table displayed by the DataGrid control. Finally we hide the combobox. note the source and 
			// row were saved when the edit began - see Edit()
			object objValue = _cboColumn.SelectedValue;
			// we can write System.DBNull back to a database but we cannot write null (which would
			// cause an exception). if the combobox is defined as a dropdownlist we cannot see the
			// null value. However if the combobox is defined as a dropdown then editing into the
			// combobox will, by default, generate a null value. For this possibility we translate
			// null to the System.DBNull value
			if (objValue == null) 
			{ 
				objValue = DBNull.Value; 
			}
			this.SetColumnValueAtRow(_cmSource, _iRowNum, objValue);
			_cboColumn.Visible = false;
		}

		// this method is called to draw the box without a highlight (ie when the cell is in unselected state)
		public void cboColumn_ChangeCommit(object sender, EventArgs e)
		{
			_DVSource = (DataRowView) this._cmSource.Current;
			object objValue = _cboColumn.SelectedValue;
			this.SetColumnValueAtRow(_cmSource, _iRowNum, objValue);
			ushort _EventID = 0;  //Out of range evlaue
			try 
			{
				_EventID = System.Convert.ToUInt16(objValue);
			}
			catch
			{
			}
			if(_EventID != 0)
			{
				int groupIDMask = 0x03C0, FIFOMask = 0x6000;
				ushort temp = (ushort) ((_EventID & groupIDMask) >> 6);
				ushort FIFO = _EventID;
				FIFO = (ushort) ((FIFO & FIFOMask) >> 13);
				switch (FIFO)
				{
					case 0:  //Non-Fifo
						_DVSource.Row[(int)CounterCol.Group] = DriveWizard.SCCorpStyle.NonFifoGrpNames[temp];//.ToString();
						break;

					case 1:  //System FIFO
						_DVSource.Row[(int)CounterCol.Group] = DriveWizard.SCCorpStyle.SystemFifoGrpNames[temp];//.ToString();
						break;

					case 2: //Fault FIFO
						_DVSource.Row[(int)CounterCol.Group] = DriveWizard.SCCorpStyle.FaultFifoGrpNames[temp];//.ToString();
						break;

					case 3: //Other FIFO - currently SPARE in controller
						_DVSource.Row[(int)CounterCol.Group] = DriveWizard.SCCorpStyle.SpareFifoGrpNames[temp];//.ToString();
						break;
				}
			}
			else
			{
				_DVSource.Row[(int)CounterCol.Group] = "Unknown";
			}
			_cboColumn.Visible = false;
		}
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			string defaultStr = "";
			DataRow[] aRowA;
			// retrieve the value at the current column-row within the source for this column
			object tempObject = this.GetColumnValueAtRow(source, rowNum);
			// use this value to access the datasource. again, we must allow that a null object
			// is returned; this typically only happens when adding a new row to the DataGrid host
			Type aType = tempObject.GetType();
			try
			{
				aRowA = ((DataTable)_objSource).Select(_strValue + " = " + tempObject.ToString());
				defaultStr = aRowA[0][_strMember].ToString();  
			}
			catch
			{
				defaultStr = _defaultNames[rowNum];
			}
			// Now paint the cell. 
			Rectangle rect = bounds;
			// use custom background color if the property was set by the User
			if (this._backBrush == null) 
				g.FillRectangle(backBrush, rect); 
			else 
				g.FillRectangle(_backBrush, rect);
			// vertical offset to account for frame of combobox
			rect.Y += 2;
			if (this._foreBrush == null) 
				g.DrawString(defaultStr, this.TextBox.Font, foreBrush, rect); 
			else
				g.DrawString(defaultStr, this.TextBox.Font, _foreBrush, rect);
		}

		public System.Drawing.Color backgroundColour 
		{
			set { if (value == System.Drawing.Color.Transparent) this._backBrush = null; else this._backBrush = new SolidBrush(value);  }
		}
		public System.Drawing.Color foregroundColour 
		{
			set { if (value == System.Drawing.Color.Transparent) this._foreBrush = null; else this._foreBrush = new SolidBrush(value);  }
		}
	}
	#endregion ComboColumn CLass

	#region tables and styles for virtual node setup
	public class virtualComboCol:SCBaseComboColumn
	{
		public virtualComboCol(int colNum, object objSource) : base (colNum, objSource)
		{
		}
		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			DataRowView myRowView = (DataRowView) source.Current;
			//user must enter a valid node ID first
			uint nodeIDVal = System.Convert.ToUInt32(myRowView.Row[(int) virtualSetupCols.nodeID]);
			if( ((this._colNum != (int) virtualSetupCols.nodeID) && (nodeIDVal == 0xFFFFFFFF))
				|| (MAIN_WINDOW.isVirtualNodes == false))
				{
					return;
				}
			base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
			enumCombo.SelectionChangeCommitted +=new EventHandler(enumCombo_SelectionChangeCommitted);
		}
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			string defaultStr = "not found";
			ArrayList myList;
			comboSource item;
			uint myVal;
			// retrieve the value at the current column-row within the source for this column
			object tempObject = this.GetColumnValueAtRow(source, rowNum);
			// use this value to access the datasource. again, we must allow that a null object
			// is returned; this typically only happens when adding a new row to the DataGrid host
			myList = (ArrayList)_objSource;
			myVal = System.Convert.ToUInt32(this.GetColumnValueAtRow(source, rowNum));
			for(int i= 0;i<myList.Count;i++)
				{
					item = (comboSource) myList[i];
					if(item.enumValue == myVal)
					{
						defaultStr = item.enumStr;
						break;
					}
				}
			// Now paint the cell. 
			Rectangle rect = bounds;
			// use custom background color if the property was set by the User
			g.FillRectangle(backBrush, rect); 
			// vertical offset to account for frame of combobox
			rect.Y += 2;
			g.DrawString(defaultStr, this.TextBox.Font, foreBrush, rect); 
		}

		protected new void enumCombo_SelectionChangeCommitted(object sender, EventArgs e)
		{
			//new used to remove compiler warning
			//we cannot override an event handler and both this and thebase handler are called 
			//(base is called first) 
			if((this._colNum >  (int) virtualSetupCols.nodeID)
				|| ((this._colNum==(int) virtualSetupCols.nodeID) && (enumCombo.SelectedIndex == 0)))  //node ID changed to none
			{
				DataRowView myRowView = (DataRowView) this._cmSource.Current;
				//now change all the others to tie up
				enumCombo.SelectionChangeCommitted -=new EventHandler(enumCombo_SelectionChangeCommitted);
				if(enumCombo.SelectedIndex>0) //index is OK here 
				{
					AvailableNodesWithEDS localDevInfo = (AvailableNodesWithEDS) MAIN_WINDOW.availableEDSInfo[enumCombo.SelectedIndex-1];
					myRowView.Row[(int) virtualSetupCols.EDS] = enumCombo.SelectedIndex;  //just correlates to the index number
					myRowView.Row[(int) virtualSetupCols.revisionNo] = localDevInfo.revisionNumber;
					myRowView.Row[(int) virtualSetupCols.vendorID] = localDevInfo.vendorNumber;
					myRowView.Row[(int) virtualSetupCols.productCode] = localDevInfo.productNumber;
				}
				else
				{
					myRowView.Row[(int) virtualSetupCols.EDS] = 0xFFFFFFFF;  //just correlates to the index number
					myRowView.Row[(int) virtualSetupCols.revisionNo] = 0xFFFFFFFF;
					myRowView.Row[(int) virtualSetupCols.vendorID] = 0xFFFFFFFF;
					myRowView.Row[(int) virtualSetupCols.productCode] = 0xFFFFFFFF;
					myRowView.Row[(int) virtualSetupCols.Master] = false;
				}
				enumCombo.SelectionChangeCommitted +=new EventHandler(enumCombo_SelectionChangeCommitted);
			}
			else
			{
			}

		}
	}
	#endregion tables and styles for virtual node setup
}
