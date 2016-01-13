using ocNet.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ocNet.Designer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		ocNet.Lib.App currentApp;

		public MainWindow()
		{
			InitializeComponent();

			AddTableTest();
/*
			currentApp = new Lib.App(@"..\..\..\..\octobase_dev\octobase_S3\S3Client\S3Client.ocappx");
			FillTreeView();
			*/
		}

		void AddTableTest()
		{
			TabItem tabItem = new TabItem();
			tabControl.Items.Add(tabItem);
			tabItem.Header = "Test";

			Grid grid = new Grid();
			grid.HorizontalAlignment = HorizontalAlignment.Stretch;
			grid.VerticalAlignment = VerticalAlignment.Stretch;
			tabItem.Content = grid;

			SmartTableControl tc = new SmartTableControl();
			tc.AddColumn(20, "PIDThatIsMuchToLong");
			tc.AddColumn(50, "FieldTypeForAnotherMuchToLongHeaderField");
			tc.AddColumn(20, "AnotherField");

			tc.Margin = new Thickness(10.1, 10.1, 10, 10);

			tc.AddRow();
			tc.AddRow();
			tc.AddRow();
			tc.AddRow();
			tc.AddRow();
			tc.AddRow();

			tc.SnapsToDevicePixels = true;

			grid.Children.Add(tc);

			tc.GetCellText = GetCellTextHandler;
		}

		string GetCellTextHandler(object sender, int colIdx, int rowIdx)
		{
			if (colIdx == 0 && rowIdx >= 0)
				return rowIdx.ToString();

			return null;
		}

		void AddTab(object res)
		{
			TabItem tabItem = new TabItem();
			tabControl.Items.Add(tabItem);

			ScrollViewer scroller = new ScrollViewer();
			scroller.HorizontalAlignment = HorizontalAlignment.Stretch;
			scroller.VerticalAlignment = VerticalAlignment.Stretch;
			scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
			tabItem.Content = scroller;

			StackPanel panel = new StackPanel();
			panel.HorizontalAlignment = HorizontalAlignment.Stretch;
			panel.VerticalAlignment = VerticalAlignment.Stretch;
			panel.Orientation = Orientation.Horizontal;
			scroller.Content = panel;


			if (res is AppResource<ocNet.Lib.DBTable.Table>)
			{
				var appRes = res as AppResource<ocNet.Lib.DBTable.Table>;

				TableForm tab = new TableForm(appRes.Resource);
				tabItem.Header = appRes.ID;
				tab.VerticalAlignment = VerticalAlignment.Stretch;
				panel.Children.Add(tab);
			}





		}


		void TreeViewAddItems<U>(TreeViewItem parentTvi, List<AppResource<U>> list) where U : class
		{
			TreeViewItem tvi;

			foreach (AppResource<U> appRes in list)
			{
				if (typeof(U) == typeof(ocNet.Lib.App))
					TreeViewAddApp(parentTvi, (appRes.Resource as ocNet.Lib.App));
				else
				{
					parentTvi.Items.Add(tvi = new TreeViewItem());
					tvi.Header = appRes.ID;
					tvi.Tag = appRes;
					tvi.MouseDoubleClick += Tvi_MouseDoubleClick;
				}
			}
		}

		private void Tvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			TreeViewItem tvi = e.Source as TreeViewItem;
			AddTab(tvi.Tag);
		}

		void TreeViewAddType<U>(TreeViewItem parentTvi, AppResourceCollection<U> collection) where U : class
		{
			TreeViewItem typeTvi;
			TreeViewItem folderTvi;

			parentTvi.Items.Add(typeTvi = new TreeViewItem());
			typeTvi.Header = collection.Type.ToString();

			// Add folders
			foreach (var folder in collection.FolderList)
			{
				typeTvi.Items.Add(folderTvi = new TreeViewItem());
				folderTvi.Header = folder.Name;
				TreeViewAddItems<U>(folderTvi, folder.ItemList);
			}

			// Add items w/o folder
			TreeViewAddItems<U>(typeTvi, collection.ItemWOFolder);
		}

		void TreeViewAddApp(TreeViewItem parentTvi, ocNet.Lib.App app)
		{
			TreeViewItem tvi;

			if (parentTvi == null)
				treeView.Items.Add(tvi = new TreeViewItem());
			else
				parentTvi.Items.Add(tvi = new TreeViewItem());
			tvi.Header = app.ID;

			TreeViewAddType<ocNet.Lib.DBTable.Table>(tvi, app.TableCollection);
			TreeViewAddType<ocNet.Lib.DBView.View>(tvi, app.ViewCollection);
			TreeViewAddType<ocNet.Lib.App>(tvi, app.SubAppCollection);
		}

		void FillTreeView()
		{
			treeView.Items.Clear();
			TreeViewAddApp(null, currentApp);
		}
	}
}
