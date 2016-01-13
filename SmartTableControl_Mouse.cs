using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ocNet.Designer
{
	public partial class SmartTableControl
	{
		int colIdxResizeInProgress = -1;
		double colWidthBeforeResize;

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			var curPos = e.GetPosition(this);

			HitTestResult hitTestResult = HitTest(curPos);
			if (hitTestResult.ColIdxHeaderResizeHit != -99)
			{
				colWidthBeforeResize = ColumnList[hitTestResult.ColIdxHeaderResizeHit].Width;
				colIdxResizeInProgress = hitTestResult.ColIdxHeaderResizeHit;
			}

			CaptureMouse();
		}

		protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if (colIdxResizeInProgress != -1)
			{
				colIdxResizeInProgress = -1;
			}

			ReleaseMouseCapture();
		}

		class HitTestResult
		{
			public int ColIdxHit = -99;
			public int ColIdxHeaderResizeHit = -99;
			public int RowIdxHit = -99;
		}

		HitTestResult HitTest(Point pos)
		{
			HitTestResult hitTestResult = new HitTestResult();

			for (int colIdx = 0; colIdx < ColumnList.Count; colIdx++)
			{
				Column col = ColumnList[colIdx];

				if (hitTestResult.ColIdxHit == -99 && pos.X - RowSelectionButtonWidth >= col.StartX - scrollXOffset && pos.X - RowSelectionButtonWidth <= col.StartX - scrollXOffset + col.Width)
					hitTestResult.ColIdxHit = colIdx;

				// If col header
				if (hitTestResult.RowIdxHit == -99)
				{
					if (pos.Y >= 0 && pos.Y <= ColumnHeaderHeight) // Y
						hitTestResult.RowIdxHit = -1;
				}

				if (hitTestResult.ColIdxHeaderResizeHit == -99 &&
					(pos.X - RowSelectionButtonWidth >= col.StartX - scrollXOffset + col.Width - ColumnHeaderMouseThreshold && pos.X - RowSelectionButtonWidth <= col.StartX - scrollXOffset + col.Width + ColumnHeaderMouseThreshold) &&   // X
					hitTestResult.RowIdxHit == -1)
					hitTestResult.ColIdxHeaderResizeHit = colIdx;

			}

			return hitTestResult;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			var curPos = e.GetPosition(this);

			if (colIdxResizeInProgress != -1)
			{
				ColumnList[colIdxResizeInProgress].Width = Math.Max(10, curPos.X - RowSelectionButtonWidth + scrollXOffset - ColumnList[colIdxResizeInProgress].StartX);
				RecalcColumns();
				this.InvalidateVisual();
			}

			HitTestResult hitTestResult = HitTest(curPos);

			if (colIdxResizeInProgress != -1 || hitTestResult.ColIdxHeaderResizeHit != -99)
			{
				Cursor = Cursors.SizeWE;
			}
			else
			{
				Cursor = Cursors.Arrow;
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			if (colIdxResizeInProgress != -1)
			{
				ColumnList[colIdxResizeInProgress].Width = colWidthBeforeResize;
				colIdxResizeInProgress = -1;

				RecalcColumns();
				this.InvalidateVisual();
			}
			Cursor = Cursors.Arrow;
		}
	}
}
