﻿using System;
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
	/// ChoiceWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class ChoiceWindow : Window
	{
		public enum eType
		{
			eItem,
			eEquipment,
			eClass,
		};

		public uint ID { get; set; }
		public eType Type { get; set; } = eType.eItem;
		public ChoiceWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			CreateItemList("");
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
		}

		private void TextBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			CreateItemList(TextBoxFilter.Text);
		}

		private void ListBoxItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ButtonDecision.IsEnabled = ListBoxItem.SelectedIndex >= 0;
		}

		private void ButtonDecision_Click(object sender, RoutedEventArgs e)
		{
			if (ListBoxItem.SelectedItem is not NameValueInfo info) return;
			ID = info.Value;
			Close();
		}

		private void CreateItemList(String filter)
		{
			ListBoxItem.Items.Clear();
			List<NameValueInfo> items = Info.Instance().Item;
			if (Type == eType.eClass) items = Info.Instance().Class;

			foreach (var item in items)
			{
				if (String.IsNullOrEmpty(filter) || item.Name.IndexOf(filter) >= 0)
				{
					if (Type == eType.eItem)
					{
						var kind = Info.Instance().Search(Info.Instance().Kind, item.Value);
						if (kind == null)
						{
							ListBoxItem.Items.Add(item);
						}
					}
					else if (Type == eType.eEquipment)
					{
						var kind = Info.Instance().Search(Info.Instance().Kind, item.Value);
						if (kind != null)
						{
							ListBoxItem.Items.Add(item);
						}
					}
					else
					{
						ListBoxItem.Items.Add(item);
					}
				}
			}
		}
	}
}
