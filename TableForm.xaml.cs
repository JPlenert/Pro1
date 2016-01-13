using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ocNet.Designer
{
	public class Test
	{
		public string FieldID { get; set; }
		public ocNet.Lib.DBTable.TableFieldType FieldType { get; set; }
		public int Length { get; set; }
		public int Decimals { get; set; }
		public bool PID { get; set; }
		public bool Zerofill { get; set; }

		public Test(string item1, ocNet.Lib.DBTable.TableFieldType item2, int item3)
		{
			FieldID = item1;
			FieldType = item2;
			Length = item3;
		}
	}

	/// <summary>
	/// Interaction logic for TableForm.xaml
	/// </summary>
	public partial class TableForm : UserControl
	{
		ocNet.Lib.DBTable.Table table;

		List<Test> testList;


		void AddColumn(int type, string headerText, string binding)
		{
			if (type == 0 || type == 1)
			{
				DataGridBoundColumn col = null;

				if (type == 0)
					col = new DataGridTextColumn();
				else
					col = new DataGridCheckBoxColumn();

				col.Header = headerText;
				col.Binding = new Binding(binding);
				dataGrid.Columns.Add(col);
			}
			else if (type == 2)
			{
				DataGridComboBoxColumn col = new DataGridComboBoxColumn();

				col.Header = headerText;
				//col.SelectedValueBinding = new Binding(binding);
				//col.SelectedValuePath = "Value";
				//col.DisplayMemberPath = "Value";
				col.TextBinding = new Binding(binding);
				col.ItemsSource = Enum.GetNames(typeof(ocNet.Lib.DBTable.TableFieldType));
				dataGrid.Columns.Add(col);
			}
		}

		public TableForm(ocNet.Lib.DBTable.Table table)
		{
			this.table = table;

			InitializeComponent();

			//(dataGrid.Columns[0] as DataGridTextColumn).Binding = new Binding("FieldID");


			testList = new List<Test>();
			foreach(var field in table.Fields)
			{
				Test curTest = new Test(field.ID, field.Type, field.Length);
				testList.Add(curTest);
				//dataGrid.Items.Add(curTest);
			}


			dataGrid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;

			//dataGrid.ItemsSource = testList;
			/*


			AddColumn(0, "FieldID", "FieldID");
			AddColumn(2, "FieldType", "FieldType");
			AddColumn(0, "Length", "Length");
			AddColumn(0, "Decimals", "Decimals");
			AddColumn(1, "PID", "PID");
			AddColumn(1, "Zerofill", "Zerofill");
			*/

		dataGrid.PreviewMouseLeftButtonDown += DataGrid_PreviewMouseLeftButtonDown;
			dataGrid.Drop += DataGrid_Drop;
			dataGrid.DragOver += DataGrid_DragOver;
			dataGrid.MouseDoubleClick += DataGrid_MouseDoubleClick;
			dataGrid.BeginningEdit += DataGrid_BeginningEdit;
			dataGrid.LoadingRow += DataGrid_LoadingRow;
			dataGrid.LoadingRowDetails += DataGrid_LoadingRowDetails;
			
			foreach(var di in testList)
			{
				dataGrid.Items.Add(di);
			}
			/*
				

			DataTrigger trigger = new DataTrigger();			
			trigger.Value = "Decimals";
			Setter set = new Setter();
			set.Property = Control.ForegroundProperty;
			set.Value = "Gray";
			trigger.Setters.Add(set);



			dataGrid.Columns[0].CellStyle.Triggers.Add(trigger);

			*/
			/*
			SmartTableControl tc = new SmartTableControl();
			tc.AddColumn(20, "PIDThatIsMuchToLong");
			tc.AddColumn(50, "FieldTypeForAnotherMuchToLongHeaderField");
			tc.AddColumn(20, "AnotherField");

			tc.AddRow();
			tc.AddRow();
			tc.AddRow();
			tc.AddRow();
			tc.AddRow();
			tc.AddRow();

			(Content as Grid).Children.Add(tc);

			tc.GetCellText = GetCellTextHandler;
			*/
		}

		string GetCellTextHandler(object sender, int colIdx, int rowIdx)
		{
			if (colIdx == 0 && rowIdx >= 0)
				return rowIdx.ToString();

			return null;
		}

		private void DataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
		{
		}

		private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
		{
		}

		private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
		{
			int a = 0;
			a++;
		}

		private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			Test item = (e.Row.Item as Test);

			if (item.FieldType == Lib.DBTable.TableFieldType.VarChar)
			{
				if ((e.Column.Header as string) == "Decimals")
					e.Cancel = true;
			}
		}

		private void DataGrid_DragOver(object sender, DragEventArgs e)
		{
			//DataGridRow r = (DataGridRow)sender;
			//dataGrid.SelectedItem = r.Item;

			// Scroll while dragging...
			DependencyObject d = dataGrid;
			while (!(d is ScrollViewer))
			{
				d = VisualTreeHelper.GetChild(d, 0);
			}
			ScrollViewer scroll = d as ScrollViewer;

		
			Point position = e.GetPosition(dataGrid);

			System.Diagnostics.Debug.WriteLine("{0} / {1}", position.X, position.Y);

			// create this textbox to test in real time the value of gridOrigin
			//textBox1.Text = position.ToString();

			double topMargin = scroll.ActualHeight * .15;
			double bottomMargin = scroll.ActualHeight * .80;

			if (position.Y < 5)
				scroll.LineUp(); // <-- needs a mechanism to control the speed of the scroll.
			if (position.Y > scroll.ActualHeight - 5)
				scroll.LineDown(); // <-- needs a mechanism to control the speed of the scroll.

		}

		private void DataGrid_Drop(object sender, DragEventArgs e)
		{
			if (e.Source == dataGrid)
			{
				DependencyObject dep = e.OriginalSource as DependencyObject;
				while (dep != null && !(dep is DataGridRow))
					dep = VisualTreeHelper.GetParent(dep);
				if (dep is DataGridRow)
				{
					DataGridRow gridRow = (dep as DataGridRow);
					if (gridRow.BindingGroup != null && gridRow.BindingGroup.Items != null && gridRow.BindingGroup.Items.Count > 0)
					{
						var dest = gridRow.BindingGroup.Items[0];
					}
				}

				for (int rowIdx = 0; rowIdx < dataGrid.Items.Count; rowIdx++)
				{
					var a = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIdx) as DataGridRow;
					if (a == null)
						continue;
					var r = VisualTreeHelper.GetDescendantBounds(a);
					var p = e.GetPosition(dataGrid);
					if (r.Contains(p))
					{
						int b = 0;
						b++;
					}
				}

			}
		}

		private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DependencyObject dep = e.OriginalSource as DependencyObject;
			while (dep != null && !(dep is DataGridRow))
				dep = VisualTreeHelper.GetParent(dep);

			if (dep is DataGridRow)
				DragDrop.DoDragDrop(dataGrid, 1, DragDropEffects.Move);
			//throw new NotImplementedException();
			
		}
		
		private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			TableFieldForm tff = new TableFieldForm();
			tff.VerticalAlignment = VerticalAlignment.Stretch;
			tff.HorizontalAlignment = HorizontalAlignment.Stretch;

			(this.Parent as StackPanel).Children.Add(tff);
			/*
			FrameworkElement item = ((FrameworkElement)e.MouseDevice.DirectlyOver);
			int a = 0;
			a++;
			*/
		}

		private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			conDelete.IsEnabled = false;
		}

		/*
			Grid grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions[0].Width = new GridLength(50);
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions[1].Width = new GridLength(50);

			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());

			TextBox tbNew = new TextBox();
			tbNew.Width = 50;
			grid.Children.Add(tbNew);
			Grid.SetRow(tbNew, 1);
			Grid.SetColumn(tbNew, 1);

			testPanel.Children.Add(grid);
		
		*/
	}
}
