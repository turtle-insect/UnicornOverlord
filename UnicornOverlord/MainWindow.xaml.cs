using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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

		private void ComboBoxLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsLoaded) return;
			Properties.Settings.Default.Save();
			if (DataContext is ViewModel vm)
			{
				vm.RefreshLanguage();
			}
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
		{
			string aboutMessage =
				"Unicorn Overlord Save Editor (Nintendo Switch)\n" +
				"Enhanced Edition v1.0\n\n" +
				"Key Features:\n" +
				"• Gold, Renown (Rank S), Medals, Coliseum Coins, Divine Shards, Corne Ash\n" +
				"• Character Editing: Level, Exp, Class, Growth Types (All 16 profiles), Dew Bonuses, Appearance\n" +
				"• Rapport Bonds: Partner name resolution, 1000 Max Bond points & Hearts, Army-wide maxing\n" +
				"• Inventory: Real-time search, Add/Delete items, Batch Add Weapons, Shields, Accessories, Consumables\n" +
				"• Equipment: Equip slot indicators, Equipped Character names, Unequip button, Upgrade toggles\n" +
				"• Units: Unlock all 10 combat units and set 5-member capacity\n" +
				"• Safe Saving: Atomic temp-file saves + automatic timestamped backups\n\n" +
				"Credits:\n" +
				"• turtle-insect (Original Author)\n" +
				"• pauljames80 & GBAtemp Community (Offsets & Data Mapping)\n" +
				"• Atlus & Vanillaware (Unicorn Overlord)";

			MessageBox.Show(aboutMessage, "About Unicorn Overlord Save Editor", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (SaveData.Instance().IsDirty)
			{
				var res = MessageBox.Show(
					"You have unsaved changes. Do you want to save before exiting?",
					"Unsaved Changes",
					MessageBoxButton.YesNoCancel,
					MessageBoxImage.Warning);

				if (res == MessageBoxResult.Yes)
				{
					if (!SaveData.Instance().Save())
					{
						MessageBox.Show("Failed to save the file. Exit aborted.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
						e.Cancel = true;
						return;
					}
				}
				else if (res == MessageBoxResult.Cancel)
				{
					e.Cancel = true;
					return;
				}
			}

			base.OnClosing(e);
		}
	}
}