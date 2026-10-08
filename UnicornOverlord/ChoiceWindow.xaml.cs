using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UnicornOverlord
{
	public partial class ChoiceWindow : Window
	{
		public enum eType
		{
			eItem,
			eEquipment,
			eClass,
			eWeapon,
			eShield,
			eAccessory,
			eAll
		};

		public uint ID { get; set; }
		public eType Type { get; set; } = eType.eItem;

		public ChoiceWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Title = Type switch
			{
				eType.eClass => "Select Character Class",
				eType.eEquipment => "Select Equipment",
				eType.eWeapon => "Select Weapon",
				eType.eShield => "Select Shield",
				eType.eAccessory => "Select Accessory",
				_ => "Select Item"
			};

			if (Type == eType.eClass)
			{
				CategoryPanel.Visibility = Visibility.Collapsed;
			}
			else
			{
				CategoryPanel.Visibility = Visibility.Visible;
				if (Type == eType.eEquipment)
				{
					// Default to all equipment types
				}
			}

			CreateItemList(TextBoxFilter.Text);

			foreach (var item in ListBoxItem.Items)
			{
				if (item is not NameValueInfo info) continue;
				if (info.Value == ID)
				{
					ListBoxItem.SelectedItem = item;
					ListBoxItem.ScrollIntoView(item);
					break;
				}
			}

			TextBoxFilter.Focus();
		}

		private void TextBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			CreateItemList(TextBoxFilter.Text);
		}

		private void ComboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				CreateItemList(TextBoxFilter.Text);
			}
		}

		private void ListBoxItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ButtonDecision.IsEnabled = ListBoxItem.SelectedIndex >= 0;
		}

		private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (ListBoxItem.SelectedItem is NameValueInfo info)
			{
				ID = info.Value;
				DialogResult = true;
				Close();
			}
		}

		private void ButtonDecision_Click(object sender, RoutedEventArgs e)
		{
			if (ListBoxItem.SelectedItem is not NameValueInfo info) return;
			ID = info.Value;
			DialogResult = true;
			Close();
		}

		private void CreateItemList(string filter)
		{
			ListBoxItem.Items.Clear();

			List<NameValueInfo> sourceList;
			if (Type == eType.eClass)
			{
				sourceList = Info.Instance().Class;
			}
			else
			{
				sourceList = Info.Instance().Item;
			}

			string selectedCategory = (ComboBoxCategory?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Types";

			foreach (var item in sourceList)
			{
				// Type constraints
				if (Type == eType.eItem && Info.Instance().IsEquipment(item.Value)) continue;
				if (Type == eType.eEquipment && !Info.Instance().IsEquipment(item.Value)) continue;
				if (Type == eType.eWeapon && item.Category != "Weapons") continue;
				if (Type == eType.eShield && item.Category != "Shields") continue;
				if (Type == eType.eAccessory && item.Category != "Accessories") continue;

				// Category dropdown filter
				if (Type != eType.eClass && selectedCategory != "All Types")
				{
					if (selectedCategory == "Weapons" && item.Category != "Weapons") continue;
					if (selectedCategory == "Shields" && item.Category != "Shields") continue;
					if (selectedCategory == "Accessories" && item.Category != "Accessories") continue;
					if (selectedCategory == "Consumables" && item.Category != "Consumables" && item.Category != "Recovery / Herbs") continue;
					if (selectedCategory == "Valuables / Materials" && item.Category != "Valuables / Materials" && item.Category != "Currency / Tokens") continue;
				}

				// Search text filter
				if (!string.IsNullOrWhiteSpace(filter))
				{
					bool matchesName = item.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
					bool matchesId = item.Value.ToString().Contains(filter) || $"0x{item.Value:X}".Contains(filter, StringComparison.OrdinalIgnoreCase);
					if (!matchesName && !matchesId) continue;
				}

				ListBoxItem.Items.Add(item);
			}

			TextBlockCount.Text = $"Showing {ListBoxItem.Items.Count} item(s)";
		}
	}
}
