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

namespace AutoFilterDataGrid
{
    /// <summary>
    /// Interaction logic for NewDataGrid.xaml
    /// </summary>
    public partial class NewDataGrid : DataGrid, INotifyPropertyChanged
    {
        private List<FilterValue> filterList;
        readonly ObservableCollection<CheckBox> filterPopupContent;
        private IEnumerable itemsSource;

        public event PropertyChangedEventHandler PropertyChanged;

        public new IEnumerable ItemsSource
        {
            get
            {
                return itemsSource;
            }
            set
            {
                itemsSource = value;
                NotifyPropertyChanged("ItemsSource");
            }
        }
        public ObservableCollection<CheckBox> FilterPopupContent
        {
            get
            {
                return filterPopupContent;
            }
        }
        public NewDataGrid()
        {
            ItemsSource = new ArrayList();
            Binding tempBinding = new Binding("ItemsSource")
            {
                Source = this,
                Converter = new CollectionViewFromIEnumerableConverter()
            };
            this.SetBinding(ItemsSourceProperty, tempBinding);
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
    }
    public class CollectionViewFromIEnumerableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            else if (!(value is System.Collections.IEnumerable))
                throw new ArgumentException("value must be of type System.Collections.IEnumerable", "value");
            else if (!targetType.IsAssignableFrom(typeof(CollectionView)))
                throw new ArgumentException("targetType must be assignable from System.Windows.Data.CollectionView", "targetType");
            else
                return new CollectionView(value as System.Collections.IEnumerable);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            else if (!(value is System.Windows.Data.CollectionView))
                throw new ArgumentException("value must be of type System.Windows.Data.CollectionView", "value");
            else if (!targetType.IsAssignableFrom(typeof(System.Collections.IEnumerable)))
                throw new ArgumentException("targetType must be assignable from System.Collections.IEnumerable", "targetType");
            else
                return value as System.Collections.IEnumerable;
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
