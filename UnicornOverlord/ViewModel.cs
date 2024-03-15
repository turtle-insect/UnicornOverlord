using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UnicornOverlord
{
	internal class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private readonly Info Info = Info.Instance();
		public ICommand OpenFileCommand { get; set; }
		public ICommand SaveFileCommand { get; set; }
		public ICommand ChoiceItemCommand { get; set; }
		public ICommand ChoiceClassCommand { get; set; }
		public ICommand AppendItemCommand { get; set; }
		public ICommand AppendEquipmentCommand { get; set; }
		public ICommand ImportCharacterCommand { get; set; }
		public ICommand ExportCharacterCommand { get; set; }
		public ICommand InsertCharacterCommand { get; set; }

		public Basic Basic { get; set; } = new Basic();
		public ObservableCollection<Character> Characters { get; set; } = new ObservableCollection<Character>();
		public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
		public ObservableCollection<Item> Equipments { get; set; } = new ObservableCollection<Item>();

		public ViewModel()
		{
			OpenFileCommand = new ActionCommand(OpenFile);
			SaveFileCommand = new ActionCommand(SaveFile);
			ChoiceItemCommand = new ActionCommand(ChoiceItem);
			ChoiceClassCommand = new ActionCommand(ChoiceClass);
			AppendItemCommand = new ActionCommand(AppendItem);
			AppendEquipmentCommand = new ActionCommand(AppendEquipment);
			ImportCharacterCommand = new ActionCommand(ImportCharacter);
			ExportCharacterCommand = new ActionCommand(ExportCharacter);
			InsertCharacterCommand = new ActionCommand(InsertCharacter);
		}

		private void Initialize()
		{
			Characters.Clear();
			Items.Clear();
			Equipments.Clear();

			// counter ??
			for(uint i = 0; i < 500; i++)
			{
				var ch = new Character(0x2AF40 + i * 464);
				if (ch.ID == 0xFFFFFFFF) break;

				Characters.Add(ch);
			}

			for (uint i = 0; i < 3500; i++)
			{
				var item = new Item(0xA0 + i * 20);
				if (item.Index == 0) break;

				if(item.Count== 0)
					Equipments.Add(item);
				else
					Items.Add(item);
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Basic)));
		}

		private void OpenFile(object? parameter)
		{
			var dlg = new OpenFileDialog();
			dlg.Filter = "UCSAVEFILE|UCSAVEFILE*.DAT";
			if (dlg.ShowDialog() == false) return;

			SaveData.Instance().Open(dlg.FileName);
			Initialize();
		}

		private void SaveFile(object? parameter)
		{
			SaveData.Instance().Save();
		}

		private void ChoiceItem(object? parameter)
		{
			Item? item = parameter as Item;
			if(item == null) return;

			var dlg = new ChoiceWindow();
			dlg.ID = item.ID;
			dlg.ShowDialog();
			item.ID = dlg.ID;
		}

		private void ChoiceClass(object? parameter)
		{
			Character? ch = parameter as Character;
			if (ch == null) return;

			var dlg = new ChoiceWindow();
			dlg.Type = ChoiceWindow.eType.eClass;
			dlg.ID = ch.Class;
			dlg.ShowDialog();
			ch.Class = dlg.ID;
		}

		private void AppendItem(object? parameter)
		{
			var item = AppendItem();
			if (item == null) return;

			item.Status = 2;
			item.Count = 1;
			Items.Add(item);
		}

		private void AppendEquipment(object? parameter)
		{
			var item = AppendItem();
			if (item == null) return;

			item.Status = 3;
			Equipments.Add(item);
		}

		private Item? AppendItem()
		{
			var dlg = new ChoiceWindow();
			dlg.ShowDialog();
			if (dlg.ID == 0) return null;

			uint index = (uint)(Items.Count + Equipments.Count);
			var item = new Item(0xA0 + index * 20);
			item.ID = dlg.ID;
			item.Index = index + 1;
			return item;
		}

		private void ImportCharacter(object? parameter)
		{
			if (parameter == null) return;

			int index = Convert.ToInt32(parameter);
			if (index == -1) return;

			var dlg = new OpenFileDialog();
			dlg.Filter = "Unicorn Overlord Character's Dump|*.uocd";
			if (dlg.ShowDialog() == false) return;

			Byte[] buffer = System.IO.File.ReadAllBytes(dlg.FileName);
			if (buffer.Length != 464) return;

			uint address = 0x2AF40 + (uint)index * 464;

			// buffer[456 ~ 464] use original
			buffer = Util.Resize(buffer, 456);

			uint id = SaveData.Instance().ReadNumber(0x63980, 4) + 1;
			Array.Copy(BitConverter.GetBytes(id), buffer, 4);
			SaveData.Instance().WriteValue(address, buffer);

			SaveData.Instance().WriteNumber(0x63980, 4, id);
		}

		private void ExportCharacter(object? parameter)
		{
			if (parameter == null) return;

			int index = Convert.ToInt32(parameter);
			if (index == -1) return;

			var dlg = new SaveFileDialog();
			dlg.Filter = "Unicorn Overlord Character's Dump|*.uocd";
			if (dlg.ShowDialog() == false) return;

			uint address = 0x2AF40 + (uint)index * 464;
			Byte[] buffer = SaveData.Instance().ReadValue(address, 464);

			// initialize
			// -------------------------------------------------------------------------
			// formation clear
			Array.Copy(BitConverter.GetBytes(0xFFFFFFFF), 0, buffer, 4, 4);
			buffer[32] = 0xFF;

			// buffer[460]
			// character's status
			// 3Bit => join
			// 4Bit => formation join
			// 5Bit => use
			buffer[460] = 0x08;

			// equipment clear
			// elements => 4Byte
			// count => 4
			// (or Append Item)
			Array.Clear(buffer, 76, 16);
			// -------------------------------------------------------------------------

			System.IO.File.WriteAllBytes(dlg.FileName, buffer);
		}

		private void InsertCharacter(object? parameter)
		{
			if (parameter == null) return;

			uint count = Convert.ToUInt32(parameter);
			if (count >= 500) return;

			var dlg = new OpenFileDialog();
			dlg.Filter = "Unicorn Overlord Character's Dump|*.uocd";
			if (dlg.ShowDialog() == false) return;

			Byte[] buffer = System.IO.File.ReadAllBytes(dlg.FileName);
			if (buffer.Length != 464) return;

			uint address = 0x2AF40 + count * 464;
			SaveData.Instance().WriteValue(address, buffer);

			uint id = SaveData.Instance().ReadNumber(0x63980, 4) + 1;
			Array.Copy(BitConverter.GetBytes(id), buffer, 4);
			SaveData.Instance().WriteNumber(0x63980, 4, id);
			SaveData.Instance().WriteNumber(0x63984, 4, count + 1);

			for (uint index = 0; index < count + 1; index++)
			{
				address = 0x2AF40 + index * 464;
				SaveData.Instance().WriteNumber(address + 456, 2, count + 2);
				SaveData.Instance().WriteNumber(address + 458, 2, count + 2);
			}
			Initialize();
		}
	}
}
