using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Concurrent;

namespace AutoFilterDataGrid
{
    /// <summary>
    /// Interaction logic for NewDataGrid.xaml
    /// </summary>
    public partial class NewDataGrid : DataGrid, INotifyPropertyChanged
    {
        //public static Style DefaultHeaderStyle = new Func<Style>(() =>
        //{
        //   Style tempStyle = new Style(typeof(DataGridColumnHeader));
        //    tempStyle.Setters.Add
        //    return tempStyle;
        //}).Invoke();
        private List<FilterValue> filterList;
        readonly ObservableCollection<CheckBox> filterPopupContent;
        //public static readonly DependencyProperty ColumnHeaderStyleProperty = DependencyProperty.Register(
          //  "ColumnHeaderStyle",
           // typeof(Style),
           // typeof(NewDataGrid),
           // new FrameworkPropertyMetadata(new Style(, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender)
           // );
        public event PropertyChangedEventHandler PropertyChanged;
        public new event DataGridSortingEventHandler Sorting;
        public ObservableCollection<CheckBox> FilterPopupContent
        {
            get
            {
                return filterPopupContent;
            }
        }
        //public new Style ColumnHeaderStyle
        //{
        //    get
        //    {
        //        return base.ColumnHeaderStyle;
        //    }
        //    set
        //    {
        //        throw new InvalidOperationException();
        //    }
        //}
        public NewDataGrid()
        {
            filterPopupContent = new ObservableCollection<CheckBox>();
            CheckBox tempCheck = new CheckBox
            {
                Content = "All",
                FontWeight = FontWeights.Bold,
                IsThreeState = true
            };
            tempCheck.Checked += AllCheckBox_Checked;
            tempCheck.Unchecked += AllCheckBox_Unchecked;
            filterPopupContent.Add(tempCheck);
            filterList = new List<FilterValue>();
            this.Loaded += AutoFilterDataGridLoaded;
            this.Items.Filter = new Predicate<object>(this.Contains);
            foreach (DataGridColumn thisColumn in this.Columns)
            {
                thisColumn.HeaderStyle = ColumnHeaderStyle;
            }
            this.Columns.CollectionChanged += Columns_CollectionChanged;
            
        }

        private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach(DataGridColumn thisColumn in e.NewItems)
                {
                    thisColumn.HeaderStyle = ColumnHeaderStyle;
                }
            }
        }

        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        private void AutoFilterDataGridLoaded(object sender, RoutedEventArgs e)
        {
            GenerateFilterList();
        }
        private void GenerateFilterList()
        {
            filterList = new List<FilterValue>();
            foreach (DataGridBoundColumn thisColumn in this.Columns)
            {
                filterList.Add(new FilterValue(((Binding)thisColumn.Binding).Path.Path.ToString(), new List<string>()));
            }
        }
        private void AllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            for (int x = 1; x < filterPopupContent.Count; x++)
            {
                CheckBox thisCheck = filterPopupContent[x];
                thisCheck.IsChecked = true;
            }
        }

        private void AllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int x = 1; x < filterPopupContent.Count; x++)
            {
                CheckBox thisCheck = filterPopupContent[x];
                thisCheck.IsChecked = false;
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool all = true;
            for (int x = 1; x < filterPopupContent.Count; x++)
            {
                CheckBox thisCheck = filterPopupContent[x];
                if (thisCheck.IsChecked == false)
                {
                    all = false;
                }
            }
            filterPopupContent[0].IsChecked = all ? true : new bool?();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            bool all = true;
            for (int x = 1; x < filterPopupContent.Count; x++)
            {
                CheckBox thisCheck = filterPopupContent[x];
                if (thisCheck.IsChecked != false)
                {
                    all = false;
                }
            }
            filterPopupContent[0].IsChecked = all ? false : new bool?();
        }
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            Button filterButton = (Button)sender;
            int columnIndex = ((DataGridColumnHeader)filterButton.TemplatedParent).DisplayIndex;
            FilterValue thisColumnFilter = new FilterValue();
            foreach (FilterValue thisFilter in filterList)
            {
                if (thisFilter.PropertyName == ((Binding)((DataGridBoundColumn)this.Columns[columnIndex]).Binding).Path.Path.ToString())
                    thisColumnFilter = thisFilter;
            }
            List<string> columnValues = new List<string>();
            Popup filterPopup = (Popup)filterButton.Tag;
            for (int x = 1; x < filterPopupContent.Count; x = 1)
            {
                filterPopupContent.RemoveAt(x);
            }
            if (this.HasItems)
            {
                PropertyPath propertyPath = ((Binding)((DataGridBoundColumn)this.Columns[columnIndex]).Binding).Path;
                Type itemsType = this.Items[0].GetType();
                foreach (object item in ((ListCollectionView)this.ItemsSource).SourceCollection)
                {
                    if (item.GetType().ToString() != "MS.Internal.NamedObject")
                    {
                        string thisValue = itemsType.GetProperty(propertyPath.Path).GetMethod.Invoke(item, new object[] { }).ToString();
                        if (!columnValues.Contains(thisValue))
                            columnValues.Add(thisValue);
                    }
                }
                columnValues.Sort();
                foreach (string thisValue in columnValues)
                {
                    CheckBox tempCheck = new CheckBox
                    {
                        Content = thisValue
                    };
                    if (!thisColumnFilter.FilteredValues.Contains(thisValue))
                    {
                        tempCheck.IsChecked = true;
                    }
                    tempCheck.Checked += CheckBox_Checked;
                    tempCheck.Unchecked += CheckBox_Unchecked;
                    filterPopupContent.Add(tempCheck);
                }
            }
            filterPopup.IsOpen = true;
            if (filterPopupContent.Count > 1)
            {
                bool? all = true;
                for (int x = 1; x < filterPopupContent.Count; x++)
                {
                    CheckBox thisCheck = filterPopupContent[x];
                    if (thisCheck.IsChecked == false && all.HasValue && all.Value == true)
                    {
                        all = false;
                    }
                    if (thisCheck.IsChecked == true && all.HasValue && all.Value == false)
                    {
                        all = new bool?();
                    }
                }
                filterPopupContent[0].IsChecked = all;
            }
            else
            {
                filterPopupContent[0].IsChecked = true;
            }
        }

        private void FilterPopup_Closed(object sender, EventArgs e)
        {
            Popup filterPopup = (Popup)sender;
            int columnIndex = ((DataGridColumnHeader)filterPopup.TemplatedParent).DisplayIndex;
            FilterValue thisColumnFilter = new FilterValue();
            foreach (FilterValue thisFilter in filterList)
            {
                if (thisFilter.PropertyName == ((Binding)((DataGridBoundColumn)this.Columns[columnIndex]).Binding).Path.Path.ToString())
                    thisColumnFilter = thisFilter;
            }
            thisColumnFilter.FilteredValues = new List<string>();
            for (int x = 1; x < FilterPopupContent.Count; x++)
            {
                CheckBox tempCheck = FilterPopupContent[x];
                if (!tempCheck.IsChecked.HasValue || !tempCheck.IsChecked.Value)
                    thisColumnFilter.FilteredValues.Add(tempCheck.Content.ToString());

            }
            (base.ItemsSource as CollectionView).Refresh();
        }
        private bool Contains(object testObject)
        {
            bool filtered = false;
            foreach (FilterValue thisFilter in filterList)
            {
                string testObjectValue = testObject.GetType().GetProperty(thisFilter.PropertyName).GetMethod.Invoke(testObject, new object[] { }).ToString();
                foreach (string filterValue in thisFilter.FilteredValues)
                {
                    if (filterValue == testObjectValue)
                    {
                        filtered = true;
                        return !filtered;
                    }
                }
            }
            return !filtered;
        }

        protected override void OnCopyingRowClipboardContent(DataGridRowClipboardEventArgs args)
        {
            List<DataGridClipboardCellContent> originalContent = new List<DataGridClipboardCellContent>(args.ClipboardRowContent.AsEnumerable());
            args.ClipboardRowContent.Clear();
            for (int x = 0; x < originalContent.Count; x++)
            {
                args.ClipboardRowContent.Add(new DataGridClipboardCellContent(originalContent[x].Item, originalContent[x].Column, originalContent[x].Content.ToString().Replace('\0', ' ')));
            }
            base.OnCopyingRowClipboardContent(args);
        }

        private void DataGridColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (this.CanSelectMultipleItems)
            {
                System.Windows.Controls.Primitives.DataGridColumnHeader columnHeader = (System.Windows.Controls.Primitives.DataGridColumnHeader)sender;
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl) && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                    this.SelectedCells.Clear();
                int startIndex = columnHeader.DisplayIndex;
                int numCols = 1;
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) && this.SelectedCells.Count > 0)
                {
                    if (this.CurrentCell == null || this.CurrentCell.Column == null)
                    {
                        ConcurrentBag<DataGridColumn> tempSelectionColumns = new ConcurrentBag<DataGridColumn>();
                        List<DataGridCellInfo> selectedCells = this.SelectedCells.ToList();
                        Parallel.For(0, selectedCells.Count, (i) =>
                        {
                            if (!tempSelectionColumns.Contains(selectedCells[(int)i].Column))
                                tempSelectionColumns.Add(selectedCells[(int)i].Column);
                        }
                        );
                        List<DataGridColumn> selectionColumns = tempSelectionColumns.GroupBy(i => i.DisplayIndex).Select(group => group.First()).OrderBy(i => i.DisplayIndex).ToList();
                        if (columnHeader.DisplayIndex - selectionColumns[0].DisplayIndex == 0)
                            numCols = 1;
                        else if (columnHeader.DisplayIndex - selectionColumns[0].DisplayIndex < 0)
                            numCols = selectionColumns[0].DisplayIndex - columnHeader.DisplayIndex;
                        else
                        {
                            startIndex = selectionColumns[0].DisplayIndex + 1;
                            numCols = columnHeader.DisplayIndex - selectionColumns[0].DisplayIndex;
                        }
                    }
                    else
                    {
                        if (columnHeader.DisplayIndex - this.CurrentCell.Column.DisplayIndex == 0)
                            numCols = 1;
                        else if (columnHeader.DisplayIndex - this.CurrentCell.Column.DisplayIndex < 0)
                            numCols = this.CurrentCell.Column.DisplayIndex - columnHeader.DisplayIndex;
                        else
                        {
                            startIndex = this.CurrentCell.Column.DisplayIndex + 1;
                            numCols = columnHeader.DisplayIndex - this.CurrentCell.Column.DisplayIndex;
                        }
                    }
                }
                for (int x = startIndex; x < startIndex + numCols; x++)
                {
                    SelectColumn(this.Columns[x]);
                }
                this.Focus();
            }
        }
        private void SelectColumn(DataGridColumn column)
        {
            foreach (object item in this.Items)
            {
                DataGridCellInfo dataGridCellInfo = new DataGridCellInfo(item, column);
                if (!this.SelectedCells.Contains(dataGridCellInfo))
                    this.SelectedCells.Add(dataGridCellInfo);
            }
        }
        private void DataGridColumnHeader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.CanUserSortColumns)
            {
                System.Windows.Controls.Primitives.DataGridColumnHeader columnHeader = (System.Windows.Controls.Primitives.DataGridColumnHeader)sender;
                columnHeader.Column.SortDirection = columnHeader.Column.SortDirection == null || columnHeader.Column.SortDirection == ListSortDirection.Descending ? ListSortDirection.Ascending : ListSortDirection.Descending;
                foreach (DataGridColumn thisColumn in this.Columns)
                {
                    if (thisColumn != columnHeader.Column)
                        thisColumn.SortDirection = null;
                }
                this.Items.SortDescriptions.Clear();
                this.Items.SortDescriptions.Add(new SortDescription(columnHeader.Column.SortMemberPath, columnHeader.Column.SortDirection.Value));
                this.Focus();
            }
        }
        private void AllButton_Click(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }
        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            Sorting?.Invoke(this, eventArgs);
        }
        
    }
    public class ICollectionViewFromIListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            else if (!(value is ICollectionView) && !(value is System.Collections.IList))
                throw new ArgumentException("value must be of type System.Windows.Data.ICollectionView or System.Collections.IList", "value");
            else if (!targetType.IsAssignableFrom(typeof(System.Collections.IList)) && !targetType.IsAssignableFrom(typeof(ICollectionView)))
                throw new ArgumentException("targetType must be assignable from System.Windows.Data.ICollectionView or System.Collections.IList", "targetType");
            else
            {
                if (value is System.Collections.IList)
                {
                    if (targetType.IsAssignableFrom(typeof(ICollectionView)))
                        return new ListCollectionView(value as System.Collections.IList);
                    else
                        return value as System.Collections.IList;
                }
                else
                {
                    return value as ICollectionView;
                }

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            else if (!(value is ICollectionView) && !(value is System.Collections.IList))
                throw new ArgumentException("value must be of type System.Windows.Data.ICollectionView or System.Collections.IList", "value");
            else if (!targetType.IsAssignableFrom(typeof(System.Collections.IList)) && !targetType.IsAssignableFrom(typeof(ICollectionView)))
                throw new ArgumentException("targetType must be assignable from System.Windows.Data.ICollectionView or System.Collections.IList", "targetType");
            else
            {
                if (value is System.Collections.IList)
                {
                    if (targetType.IsAssignableFrom(typeof(ICollectionView)))
                        return new ListCollectionView(value as System.Collections.IList);
                    else
                        return value as System.Collections.IList;
                }
                else
                {
                    return value as ICollectionView;
                }

            }
        }
    }
    public class FilterValue
    {
        string propertyName;
        List<string> filteredValues;

        public FilterValue()
        {
            PropertyName = "";
            FilteredValues = new List<string>();
        }

        public FilterValue(string propertyName, List<string> permittedValues)
        {
            this.PropertyName = propertyName;
            this.FilteredValues = permittedValues;
        }

        public string PropertyName { get => propertyName; set => propertyName = value; }
        public List<string> FilteredValues { get => filteredValues; set => filteredValues = value; }
    }
}
