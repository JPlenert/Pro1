using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ocNet.Designer
{
	public partial class SmartTableControl : FrameworkElement
	{
		UIElementCollection childs; // Does handle AddVisualChild and AddLogicalChild

		ScrollBar vScroll;
		ScrollBar hScroll;

		const double ColumnHeaderMouseThreshold = 5;

		public class Column
		{
			public double Width { get; set; }
			public Brush HeaderBackgroundBrush { get; set; }
			public Brush CellsBackgroundBrush { get; set; }
			public string HeaderText { get; set; }

			internal double StartX { get; set; }			// Calculated
		}

		public class Cell
		{
			public Brush BackgroundBrush { get; set; }
			public string Text { get; set; }
		}

		public class RowBase
		{
			public object Tag { get; set; }
			public bool Selected { get; set; }
			public double Height { get; set; }

			internal double StartY { get; set; }            // Calculated

			public RowBase()
			{
				Height = Double.NaN;
			}
		}

		public class RowWContent : RowBase
		{
			public List<Cell> CellList { get; set; }
		}

		List<Column> ColumnList;
		List<RowBase> RowList;

		double scrollXOffset;
		double scrollYOffset;

		public Brush ColumnHeaderDefaultBackgroundBrush { get; set; }
		public Brush CellDefaultBackgroundBrush { get; set; }
		public Brush RowSelectionButtonDefaultBackgroudBrush { get; set; }

		public double _columnHeaderHeight;
		public double ColumnHeaderHeight { get { return _columnHeaderHeight; } set { _columnHeaderHeight = value; RecalcColumns(); } }

		public double _rowSelectionButtonWidth;
		public double RowSelectionButtonWidth { get { return _rowSelectionButtonWidth; } set { _rowSelectionButtonWidth = value; RecalcColumns(); } }

		public double _rowDefaultHeight;
		public double RowDefaultHeight { get { return _rowDefaultHeight; } set { _rowDefaultHeight = value; } }

		public Thickness ColumnHeaderMargin { get; set; }
		public Thickness CellMargin { get; set; }

		Pen smallBlackPen;
		Pen smallLtGrayPen;
		Pen smallGrayPen;
		Brush dkLLtGrayToGrayVBrush;
		Pen smallLLtGrayToGrayVPen;
		Brush dkLLtGrayToGrayHBrush;
		Pen smallLLtGrayToGrayHPen;

		public SmartTableControl()
		{
			ColumnList = new List<Column>();
			RowList = new List<RowBase>();
			ColumnHeaderDefaultBackgroundBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230));
			RowSelectionButtonDefaultBackgroudBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230));
			CellDefaultBackgroundBrush = Brushes.White;
			_columnHeaderHeight = 20;
			ColumnHeaderMargin = new Thickness(2, 2, 2, 2);
			CellMargin = new Thickness(2, 2, 2, 2);
			RowDefaultHeight = 20;
			_rowSelectionButtonWidth = 20;

			childs = new UIElementCollection(this, this);

			UseLayoutRounding = true;
			SnapsToDevicePixels = true;

			this.Margin = new Thickness(6, 120, 5, 5);
			this.Width = 250;
			this.Height = 100;
			this.HorizontalAlignment = HorizontalAlignment.Left;
			this.VerticalAlignment = VerticalAlignment.Top;

			hScroll = new ScrollBar();
			childs.Add(hScroll);
			hScroll.Orientation = Orientation.Horizontal;
			hScroll.Margin = new Thickness(0, 0, 17, 0);
			hScroll.HorizontalAlignment = HorizontalAlignment.Stretch;
			hScroll.VerticalAlignment = VerticalAlignment.Bottom;
			hScroll.Minimum = 0;
			hScroll.Maximum = 0;
			hScroll.SmallChange = 1;
			hScroll.LargeChange = 5d;
			hScroll.ValueChanged += HScroll_ValueChanged;

			vScroll = new ScrollBar();
			childs.Add(vScroll);
			vScroll.Margin = new Thickness(0, 0, 0, 17);
			vScroll.HorizontalAlignment = HorizontalAlignment.Right;
			vScroll.VerticalAlignment = VerticalAlignment.Stretch;
			vScroll.Minimum = 0;
			vScroll.Maximum = 0;
			vScroll.SmallChange = 1;
			vScroll.LargeChange = 5d;
			vScroll.ValueChanged += VScroll_ValueChanged;

			smallBlackPen = new Pen(Brushes.Black, 1);
			smallLtGrayPen = new Pen(Brushes.LightGray, 1);
			smallGrayPen = new Pen(Brushes.Gray, 1);

			LinearGradientBrush lb = new LinearGradientBrush();
			lb.GradientStops.Add(new GradientStop(Color.FromRgb(230, 230, 230), 0));
			lb.GradientStops.Add(new GradientStop(Color.FromRgb(0, 0, 0), 0.8));
			lb.MappingMode = BrushMappingMode.RelativeToBoundingBox;
			lb.SpreadMethod = GradientSpreadMethod.Pad;

			dkLLtGrayToGrayVBrush = new LinearGradientBrush(Color.FromRgb(230, 230, 230), Colors.Gray, 90);
			smallLLtGrayToGrayVPen = new Pen(dkLLtGrayToGrayVBrush, 1);

			dkLLtGrayToGrayHBrush = new LinearGradientBrush(Color.FromRgb(230, 230, 230), Colors.Gray, 0);
			smallLLtGrayToGrayHPen = new Pen(dkLLtGrayToGrayHBrush, 1);

			InvalidateMeasure();
		}

		private void HScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			int colIdx = (int)Math.Ceiling(e.NewValue);
			hScroll.Value = colIdx;
			scrollXOffset = ColumnList[colIdx].StartX;

			this.InvalidateVisual();
		}

		private void VScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			int rowIdx = (int)Math.Ceiling(e.NewValue);
			vScroll.Value = rowIdx;
			scrollYOffset = RowList[rowIdx].StartY;
			
			this.InvalidateVisual();
		}

		void RecalcColumns()
		{
			double curX = 0;
			for (int colIdx = 0; colIdx < ColumnList.Count; colIdx++)
			{
				Column column = ColumnList[colIdx];

				column.StartX = curX;
				if (column.HeaderText == null)
					column.HeaderText = colIdx.ToString();
				curX += ColumnList[colIdx].Width + 1;
			}

			curX += RowSelectionButtonWidth;

			// Calculate the HScroll
			hScroll.Visibility = curX > Width - vScroll.Width ? Visibility.Visible : Visibility.Hidden;
			vScroll.Margin = new Thickness(0, 0, 0, hScroll.Visibility == Visibility.Visible ? 17 : 0);
			hScroll.Maximum = ColumnList.Count-1;
			if (hScroll.Visibility == Visibility.Hidden)
				scrollXOffset = 0;
		}

		void RecalcRows()
		{
			double curY = 0;
			for(int rowIdx = 0; rowIdx < RowList.Count; rowIdx++)
			{
				RowBase row = RowList[rowIdx];

				row.StartY = curY;
				curY += OnGetRowHeight(rowIdx) + 1;
			}
		}

		public void AddColumn(int width, string headerText = null)
		{
			Column newCol = new Column();
			newCol.Width = width;
			newCol.HeaderText = headerText;

			ColumnList.Add(newCol);

			RecalcColumns();
		}

		public void AddRow()
		{
			RowBase row = new RowBase();
			RowList.Add(row);
			vScroll.Maximum = (RowList.Count-1) * 1d;
			RecalcRows();
		}

		protected override Visual GetVisualChild(int index)
		{
			return childs[index];
		}

		protected override int VisualChildrenCount
		{
			get
			{
				return childs.Count;
			}
		}

		protected override IEnumerator LogicalChildren
		{
			get
			{
				return childs.GetEnumerator();
			}
		}

		protected override Size MeasureOverride(Size constraint)
		{
			for (int i = 0; i < VisualChildrenCount; i++)
				(GetVisualChild(i) as UIElement).Measure(constraint);

			return constraint;
		}

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			for (int i = 0; i < VisualChildrenCount; i++)
				(GetVisualChild(i) as UIElement).Arrange(new Rect(arrangeBounds));

			return arrangeBounds;
		}

		public delegate Brush GetBackgroundBrushHandler(object sender, int colIdx, int rowIdx);
		public GetBackgroundBrushHandler GetBackgroundBrush;
		protected Brush OnGetBackgroundBrush(int colIdx, int rowIdx)
		{
			// First always check the delegate
			if (GetBackgroundBrush != null)
				return GetBackgroundBrush(this, colIdx, rowIdx);

			// On ColHeader
			if (rowIdx == -1)
				return ColumnHeaderDefaultBackgroundBrush;
			else if (colIdx == -1)
				return RowSelectionButtonDefaultBackgroudBrush;
			else
			{
				if (RowList[rowIdx] is RowWContent)
					return ((RowList[rowIdx] as RowWContent).CellList[colIdx]).BackgroundBrush;
				if (ColumnList[colIdx].CellsBackgroundBrush != null)
					return ColumnList[colIdx].CellsBackgroundBrush;

				return CellDefaultBackgroundBrush;
			}
		}

		public delegate string GetCellTextHandler(object sender, int colIdx, int rowIdx);
		public GetCellTextHandler GetCellText;
		protected string OnGetCellText(int colIdx, int rowIdx)
		{
			if (GetCellText != null)
			{
				string retStr = GetCellText(this, colIdx, rowIdx);

				if (retStr != null)
					return retStr;
			}

			// On ColHeader
			if (rowIdx == -1)
				return ColumnList[colIdx].HeaderText;
			else if (RowList[rowIdx] is RowWContent)
				return ((RowList[rowIdx] as RowWContent).CellList[colIdx]).Text;

			return "";
		}

		public delegate double GetRowHeightHandler(object sender, int rowIdx);
		public GetRowHeightHandler GetRowHeight;
		protected double OnGetRowHeight(int rowIdx)
		{
			double height;

			if (GetRowHeight != null)
			{
				height = GetRowHeight(this, rowIdx);
				if (!Double.IsNaN(height))
					return height;
			}

			height = RowList[rowIdx].Height;
			if (!Double.IsNaN(height))
				return height;

			return RowDefaultHeight;
		}

		/// <summary>
		/// Draws the box in the upper left corner (pos -1, -1)
		/// </summary>
		/// <param name="dc"></param>
		void DrawCornerBox(DrawingContext dc)
		{
			Brush backgroundBrush;
			Rect drawRect;

			drawRect = new Rect(0, 0, RowSelectionButtonWidth, ColumnHeaderHeight);

			// Get Bk-Color
			backgroundBrush = OnGetBackgroundBrush(-1, -1);

			dc.DrawRectangleGuided(backgroundBrush, null, drawRect);
			dc.DrawLineGuided(smallLLtGrayToGrayVPen, drawRect.Right, drawRect.Top, drawRect.Right, drawRect.Bottom + 1);
			dc.DrawLineGuided(smallLLtGrayToGrayHPen, drawRect.BottomLeft, drawRect.BottomRight);
		}

		/// <summary>
		/// Draws a column header over the first data row
		/// In the default case the right and button line will be drawn by this method.
		/// The size of the drawrect is not including this both lines.
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="colIdx"></param>
		/// <param name="drawRect"></param>
		void DrawColumnHeader(DrawingContext dc, int colIdx, Rect drawRect)
		{
			Brush backgroundBrush;
			Column col = ColumnList[colIdx];
			string headerText;
			Rect textRect;

			// Get Bk-Color
			backgroundBrush = OnGetBackgroundBrush(colIdx, -1);

			// Get Header-Text
			headerText = OnGetCellText(colIdx, -1);

			dc.DrawRectangleGuided(backgroundBrush, null, drawRect);
			dc.DrawLineGuided(smallLLtGrayToGrayVPen, drawRect.Right, drawRect.Top, drawRect.Right, drawRect.Bottom+1);
			dc.DrawLineGuided(smallGrayPen, drawRect.BottomLeft, drawRect.BottomRight);

			textRect = new Rect(drawRect.Left + ColumnHeaderMargin.Left, drawRect.Top + ColumnHeaderMargin.Top, drawRect.Width - (ColumnHeaderMargin.Left + ColumnHeaderMargin.Right), drawRect.Height - (ColumnHeaderMargin.Top + ColumnHeaderMargin.Bottom));

			FormattedText text = new FormattedText(headerText, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.Black);
			text.MaxTextWidth = textRect.Width;
			text.Trimming = TextTrimming.None;
			text.MaxLineCount = 1;
			dc.DrawText(text, new Point(textRect.Left + (textRect.Width - text.Width) / 2, 2));

		}

		/// <summary>
		/// Draws a selection button right to the first data column.
		/// In the default case the right and button line will be drawn by this method.
		/// The size of the drawrect is not including this both lines.
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="rowIdx"></param>
		/// <param name="drawRect"></param>
		void DrawRowSelectionButton(DrawingContext dc, int rowIdx, Rect drawRect)
		{
			Brush backgroundBrush;

			// Get Bk-Color
			backgroundBrush = OnGetBackgroundBrush(-1, rowIdx);

			dc.DrawRectangleGuided(backgroundBrush, null, drawRect);
			dc.DrawLineGuided(smallLLtGrayToGrayHPen, drawRect.Left, drawRect.Bottom, drawRect.Right+1, drawRect.Bottom);
			dc.DrawLineGuided(smallGrayPen, drawRect.TopRight, drawRect.BottomRight);
		}

		/// <summary>
		/// Draws a data cell 
		/// In the default case the right and button line will be drawn by this method.
		/// The size of the drawrect is not including this both lines.
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="cell"></param>
		/// <param name="colIdx"></param>
		/// <param name="rowIdx"></param>
		/// <param name="drawRect"></param>
		void DrawCell(DrawingContext dc, Cell cell, int colIdx, int rowIdx, Rect drawRect)
		{
			Brush backgroundBrush;
			string cellText;
			Column column = ColumnList[colIdx];
			Rect textRect;

			// Get Bk-Color
			backgroundBrush = OnGetBackgroundBrush(colIdx, rowIdx);

			// Get Text
			cellText = OnGetCellText(colIdx, rowIdx);

			dc.DrawRectangleGuided(backgroundBrush, null, drawRect);
			dc.DrawLineGuided(smallLtGrayPen, drawRect.Left, drawRect.Bottom, drawRect.Right + 1, drawRect.Bottom);
			dc.DrawLineGuided(smallLtGrayPen, drawRect.Right, drawRect.Top, drawRect.Right, drawRect.Bottom);

			textRect = new Rect(drawRect.Left + ColumnHeaderMargin.Left, drawRect.Top + ColumnHeaderMargin.Top, drawRect.Width - (ColumnHeaderMargin.Left + ColumnHeaderMargin.Right), drawRect.Height - (ColumnHeaderMargin.Top + ColumnHeaderMargin.Bottom));

			FormattedText text = new FormattedText(cellText, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.Black);
			text.MaxTextWidth = textRect.Width;
			text.Trimming = TextTrimming.None;
			text.MaxLineCount = 1;
			dc.DrawText(text, new Point(textRect.Left + (textRect.Width - text.Width) / 2, drawRect.Top + ColumnHeaderMargin.Top));
		}

		/// <summary>
		/// Draws all column header
		/// </summary>
		/// <param name="dc"></param>
		void DrawColumnHeaders(DrawingContext dc)
		{           
			double curX = RowSelectionButtonWidth + 1 - scrollXOffset;
			for (int colIdx = 0; colIdx < ColumnList.Count; colIdx++)
			{
				//drawingContext.DrawLine(myPen, new Point(curX, 0), new Point(curX, this.Height));
				DrawColumnHeader(dc, colIdx, new Rect(curX, 0, ColumnList[colIdx].Width, ColumnHeaderHeight));
				curX += ColumnList[colIdx].Width + 1;
			}

			// Check if rest table header (right) must be dawn
			if (curX < Width)
			{
				dc.DrawRectangleGuided(ColumnHeaderDefaultBackgroundBrush, null, new Rect(curX, 0, Width - curX, ColumnHeaderHeight));
				dc.DrawLineGuided(smallGrayPen, curX, ColumnHeaderHeight, curX + Width - curX, ColumnHeaderHeight);
			}
		}


		protected override void OnRender(DrawingContext drawingContext)
		{
			double rowHeight;
			double curY;
			double curX;

			// Draw ColumnHeaders and CornerBox
			RectangleGeometry completeDrawGeo = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight));
			drawingContext.PushClip(completeDrawGeo);
			DrawColumnHeaders(drawingContext);
			DrawCornerBox(drawingContext);
			drawingContext.Pop();
			
			// If needed, draw SelectionButtons
			if (RowSelectionButtonWidth > 0)
			{
				RectangleGeometry rowSelectionButtonGeo = new RectangleGeometry(new Rect(0, ColumnHeaderHeight, ActualWidth, ActualHeight - ColumnHeaderHeight));
				drawingContext.PushClip(rowSelectionButtonGeo);

				curY = ColumnHeaderHeight + 1 - scrollYOffset;

				for (int rowIdx = 0; rowIdx < RowList.Count; rowIdx++)
				{
					rowHeight = OnGetRowHeight(rowIdx);

					DrawRowSelectionButton(drawingContext, rowIdx, new Rect(0, curY, RowSelectionButtonWidth, rowHeight));

					curY += rowHeight + 1;
				}

				drawingContext.Pop();
			}

			// Draw rows (with cells)
			RectangleGeometry completeDataGeo = new RectangleGeometry(new Rect(RowSelectionButtonWidth+1, ColumnHeaderHeight+1, ActualWidth - RowSelectionButtonWidth, ActualHeight - ColumnHeaderHeight));
			drawingContext.PushClip(completeDataGeo);

			curY = ColumnHeaderHeight + 1 - scrollYOffset;
			for (int rowIdx = 0; rowIdx < RowList.Count; rowIdx++)
			{
				curX = RowSelectionButtonWidth + 1 - scrollXOffset;

				rowHeight = OnGetRowHeight(rowIdx);

				// Draw Cells
				for (int colIdx = 0; colIdx < ColumnList.Count; colIdx++)
				{
					Column column = ColumnList[colIdx];
					Rect cellRect = new Rect(curX, curY, column.Width, rowHeight);
					DrawCell(drawingContext, null, colIdx, rowIdx, cellRect);
					curX += column.Width + 1;
				}

				curY += rowHeight + 1;
			}

			drawingContext.Pop();

			base.OnRender(drawingContext);
		}
	}

	public static class DrawingContextExtensions
	{
		static public void DrawRectangleGuided(this DrawingContext dc, Brush brush, Pen pen, Rect rect)
		{
			GuidelineSet gs = new GuidelineSet();
			gs.GuidelinesY.Add(rect.Top);
			gs.GuidelinesY.Add(rect.Bottom-1);
			gs.GuidelinesX.Add(rect.Left);
			gs.GuidelinesX.Add(rect.Right-1);
			// Why Right - 1?!
			// Rect Left=0 and Width=10 has Right = 10.
			// DrawRect will draw with a width of 10 starting from 0 -> Ends at 9!

			dc.PushGuidelineSet(gs);
			dc.DrawRectangle(brush, null, rect);
			dc.Pop();
		}

		static public void DrawLineGuided(this DrawingContext dc, Pen pen, Point point0, Point point1)
		{
			GuidelineSet gs = new GuidelineSet();
			if (point0.X == point1.X)       // vertical line
			{
				gs.GuidelinesY.Add(point0.Y);
				gs.GuidelinesY.Add(point1.Y);
				gs.GuidelinesX.Add(point0.X);

				// Middle of the line should be between the points!
				point0.Offset(pen.Thickness / 2, 0);
				point1.Offset(pen.Thickness / 2, 0);
			}
			else if (point0.Y == point1.Y)  // horizontal line
			{
				gs.GuidelinesX.Add(point0.X);
				gs.GuidelinesX.Add(point1.X);
				gs.GuidelinesY.Add(point0.Y);

				// Middle of the line should be between the points!
				point0.Offset(0, pen.Thickness / 2);
				point1.Offset(0, pen.Thickness / 2);
			}
			else
				throw new NotImplementedException();    // Makes no sense !?

			dc.PushGuidelineSet(gs);
			dc.DrawLine(pen, point0, point1);
			dc.Pop();
		}

		static public void DrawLineGuided(this DrawingContext dc, Pen pen, double x0, double y0, double x1, double y1)
		{
			dc.DrawLineGuided(pen, new Point(x0, y0), new Point(x1, y1));
		}
	}

}
