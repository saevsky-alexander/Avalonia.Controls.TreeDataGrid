﻿using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridCellsPresenter : TreeDataGridColumnarPresenterBase<IColumn>, IChildIndexProvider
    {
        public static readonly DirectProperty<TreeDataGridCellsPresenter, IRows?> RowsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridCellsPresenter, IRows?>(
                nameof(Rows),
                o => o.Rows,
                (o, v) => o.Rows = v);

        private IRows? _rows;

        public event EventHandler<ChildIndexChangedEventArgs>? ChildIndexChanged;

        public IRows? Rows
        {
            get => _rows;
            set => SetAndRaise(RowsProperty, ref _rows, value);
        }

        public int RowIndex { get; private set; } = -1;

        protected override Orientation Orientation => Orientation.Horizontal;

        public void Realize(int index)
        {
            if (RowIndex != -1)
                throw new InvalidOperationException("Row is already realized.");
            UpdateRowIndex(index);
            InvalidateMeasure();
        }

        public void Unrealize()
        {
            if (RowIndex == -1)
                throw new InvalidOperationException("Row is not realized.");
            RowIndex = -1;
            RecycleAllElements();
        }

        public void UpdateRowIndex(int index)
        {
            if (index < 0 || Rows is null || index >= Rows.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            RowIndex = index;
        }

        protected override Size MeasureElement(int index, Control element, Size availableSize)
        {
            element.Measure(availableSize);
            return ((IColumns)Items!).CellMeasured(index, RowIndex, element.DesiredSize);
        }

        protected override Control GetElementFromFactory(IColumn column, int index)
        {
            var model = _rows!.RealizeCell(column, index, RowIndex);
            var cell = (TreeDataGridCell)GetElementFromFactory(model, index, this);
            cell.Realize(ElementFactory!, model, index, RowIndex);
            return cell;
        }

        protected override void RealizeElement(Control element, IColumn column, int index)
        {
            var cell = (TreeDataGridCell)element;

            if (cell.ColumnIndex == index && cell.RowIndex == RowIndex)
            {
                ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element));
            }
            else if (cell.ColumnIndex == -1 && cell.RowIndex == -1)
            {
                var model = _rows!.RealizeCell(column, index, RowIndex);
                ((TreeDataGridCell)element).Realize(ElementFactory!, model, index, RowIndex);
                ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element));
            }
            else
            {
                throw new InvalidOperationException("Cell already realized");
            }
        }

        protected override void UnrealizeElement(Control element)
        {
            var cell = (TreeDataGridCell)element;
            _rows!.UnrealizeCell(cell.Model!, cell.ColumnIndex, cell.RowIndex);
            cell.Unrealize();
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element));
        }

        protected override void UpdateElementIndex(Control element, int index)
        {
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element));
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == BackgroundProperty)
                InvalidateVisual();
        }

        public int GetChildIndex(ILogical child)
        {
            if (child is TreeDataGridCell cell)
            {
                return cell.ColumnIndex;
            }

            return -1;
        }

        public new bool TryGetTotalCount(out int count)
        {
            if (Items is null)
            {
                count = 0;
                return false;
            }

            count = Items.Count;
            return true;
        }
    }
}
