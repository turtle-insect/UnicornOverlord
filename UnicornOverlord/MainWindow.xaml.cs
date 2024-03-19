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
            (DataContext as ViewModel).OpenFileCommand.Execute(this);
            (DataContext as ViewModel).ReadLanguageSetting();
        }

        private void ChangeLanguage(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as ViewModel).ChangeLanguage(sender, e);

            ListBoxItem.ItemsSource = (DataContext as ViewModel).Items;
            ListBoxEquipment.ItemsSource = (DataContext as ViewModel).Equipments;
            ListBoxCharacter.ItemsSource = (DataContext as ViewModel).Characters;
        }
    }
}