using System.Collections.ObjectModel;
using System.Data.Common;
using System.Reflection.Metadata;
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

namespace UnicornOverlord
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            if (ListBoxItem.SelectedItems.Count > 0)
            {
                var removeList = new List<Item>();
                foreach (Item selectedItem in ListBoxItem.SelectedItems)
                {
                    removeList.Add(selectedItem);
                    selectedItem.Delete();
                }

                foreach (var item in removeList)
                {
                    (DataContext as ViewModel).Items.Remove(item);
                }
                (DataContext as ViewModel).Items = new ObservableCollection<Item>((DataContext as ViewModel).Items.Where(item => item.ID != 0).ToList());
            }
        }

        private void EditItem(object sender, RoutedEventArgs e)
        {
            Item? item = (Item)ListBoxItem.SelectedItems[0];
            if (item == null) return;

            var dlg = new ChoiceWindow();
            dlg.ID = item.ID;
            dlg.ShowDialog();
            foreach (Item selectedItem in ListBoxItem.SelectedItems)
            {
                selectedItem.ID = dlg.ID;
                selectedItem.Count = dlg.Count;
            }
        }


        private void DeleteEquipment(object sender, RoutedEventArgs e)
        {
            if (ListBoxEquipment.SelectedItems.Count > 0)
            {
                var removeList = new List<Item>();
                foreach (Item selectedItem in ListBoxEquipment.SelectedItems)
                {
                    removeList.Add(selectedItem);
                    selectedItem.Delete();
                }

                foreach (var item in removeList)
                {
                    (DataContext as ViewModel).Items.Remove(item);
                }
                (DataContext as ViewModel).Items = new ObservableCollection<Item>((DataContext as ViewModel).Items.Where(item => item.ID != 0).ToList());
            }
        }

        private void EditEquipment(object sender, RoutedEventArgs e)
        {
            Item? item = (Item)ListBoxEquipment.SelectedItems[0];
            if (item == null) return;

            var dlg = new ChoiceWindow();
            dlg.ID = item.ID;
            dlg.ShowDialog();
            foreach (Item selectedItem in ListBoxEquipment.SelectedItems)
            {
                selectedItem.ID = dlg.ID;
            }
        }
    }
}