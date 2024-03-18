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
            }
        }

        private void EditItemCount(object sender, RoutedEventArgs e)
        {

        }
    }
}