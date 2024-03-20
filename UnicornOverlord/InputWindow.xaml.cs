using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace UnicornOverlord
{
    /// <summary>
    /// InputWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InputWindow : Window
    {
        public uint Count { get; internal set; }
        public bool Confirmed { get; private set; }

        public InputWindow(string title = "Input the Count", string countName= "Count")
        {
            InitializeComponent();
            Title = title;
            CountHint.Content = countName;
        }

        private void ButtonDecision_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxCount.Text)) return;
            Count = uint.Parse(TextBoxCount.Text);
            Confirmed = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();

        }
    }
}
