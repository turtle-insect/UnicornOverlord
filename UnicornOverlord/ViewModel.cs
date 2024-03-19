using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        public ICommand EditItemCommand { get; set; }
        public ICommand DeleteItemCommand { get; set; }
        public ICommand AppendEquipmentCommand { get; set; }
        public ICommand EditEquipmentCommand { get; set; }
        public ICommand DeleteEquipmentCommand { get; set; }
        public ICommand ImportCharacterCommand { get; set; }
        public ICommand ExportCharacterCommand { get; set; }
        public ICommand InsertCharacterCommand { get; set; }
        public ICommand ChangeItemCountMaxCommand { get; set; }

        public Basic Basic { get; set; } = new Basic();
        public ObservableCollection<Character> Characters { get; set; } = new ObservableCollection<Character>();
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> Equipments { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Unit> Units { get; set; } = new ObservableCollection<Unit>();
        public ObservableCollection<string> Languages { get; set; } = new ObservableCollection<string>();

        public ViewModel()
        {
            OpenFileCommand = new ActionCommand(OpenFile);
            SaveFileCommand = new ActionCommand(SaveFile);
            ChoiceItemCommand = new ActionCommand(ChoiceItem);
            ChoiceClassCommand = new ActionCommand(ChoiceClass);
            AppendItemCommand = new ActionCommand(AppendItem);
            EditItemCommand = new ActionCommand(EditItem);
            DeleteItemCommand = new ActionCommand(DeleteItem);
            EditEquipmentCommand = new ActionCommand(EditEquipment);
            DeleteEquipmentCommand = new ActionCommand(DeleteEquipment);
            AppendEquipmentCommand = new ActionCommand(AppendEquipment);
            ImportCharacterCommand = new ActionCommand(ImportCharacter);
            ExportCharacterCommand = new ActionCommand(ExportCharacter);
            InsertCharacterCommand = new ActionCommand(InsertCharacter);
            ChangeItemCountMaxCommand = new ActionCommand(ChangeItemCountMax);
        }

        private void Initialize()
        {
            Characters.Clear();
            Items.Clear();
            Equipments.Clear();
            Units.Clear();
            Languages.Clear();

            // create bond
            var bondDictionary = new Dictionary<uint, ObservableCollection<Bond>>();
            for (uint index = 0; index < 164; index++)
            {
                uint baseAddress = 0x1B5830 + index * 1316;
                uint id = SaveData.Instance().ReadNumber(baseAddress, 4);
                if (id == 0xFFFFFFFF) break;

                var bonds = new ObservableCollection<Bond>();
                bondDictionary.Add(id, bonds);
                for (uint count = 0; count < 164; count++)
                {
                    uint address = baseAddress + 4 + count * 8;
                    id = SaveData.Instance().ReadNumber(address, 4);
                    if (id == 0xFFFFFFFF) break;

                    bonds.Add(new Bond(address));
                }
            }

            // counter ??
            for (uint i = 0; i < 500; i++)
            {
                var ch = new Character(0x2AF40 + i * 464);
                if (ch.ID == 0xFFFFFFFF) break;

                if (bondDictionary.ContainsKey(ch.ID))
                {
                    ch.Bonds = bondDictionary[ch.ID];
                }

                Characters.Add(ch);
            }

            for (uint i = 0; i < 3800; i++)
            {
                var item = new Item(0xA0 + i * 20);
                if (item.Index == 0) break;

                if (item.Count == 0)
                    Equipments.Add(item);
                else
                    Items.Add(item);
            }

            for (uint i = 0; i < 10; i++)
            {
                var unit = new Unit(0x10D89A + i * 1720);
                Units.Add(unit);
            }

            readLanguage();

            OnPropertyChanged(nameof(Basic));
            OnPropertyChanged(nameof(Languages));
        }

        private void readLanguage()
        {
            foreach (var languageOption in Info.Instance().Languages)
            {
                Languages.Add(languageOption);
            }
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
            if (item == null) return;

            var dlg = new ChoiceWindow();
            dlg.ID = item.ID;
            dlg.ShowDialog();
            if (!dlg.Confirmed)
                return;
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
            if (!dlg.Confirmed)
                return;
            ch.Class = dlg.ID;
        }

        private void AppendItem(object? parameter)
        {
            AppendItem(0);
        }

        private void EditItem(object? parameter)
        {
            if (parameter == null)
                return;
            List<Item> itemsToEdit = null;
            if (parameter is Item)
            {
                itemsToEdit = new List<Item> { parameter as Item };
            }
            else
            {
                itemsToEdit = (parameter as IEnumerable<object>).Select(item => item as Item).ToList();
            }
            if (itemsToEdit != null && itemsToEdit.Count > 0)
            {
                Item? item = itemsToEdit[0];
                if (item == null) return;

                var dlg = new ChoiceWindow();
                dlg.ID = item.ID;
                dlg.ShowDialog();
                if (!dlg.Confirmed)
                    return;
                foreach (Item selectedItem in itemsToEdit)
                {
                    selectedItem.ID = dlg.ID;
                    selectedItem.Count = dlg.Count;
                }
            }
        }

        private void DeleteItem(object? parameter)
        {
            if (parameter == null)
                return;
            List<Item> itemsToDelete = null;
            if (parameter is Item)
            {
                itemsToDelete = new List<Item> { parameter as Item };
            }
            else
            {
                itemsToDelete = (parameter as IEnumerable<object>).Select(item => item as Item).ToList();
            }

            if (itemsToDelete != null && itemsToDelete.Count > 0)
            {
                List<Item> temp = new List<Item>();
                foreach (Item selectedItem in itemsToDelete)
                {
                    temp.Add(selectedItem);
                }

                for (int i = 0; i < temp.Count; i++)
                {
                    temp[i].Empty();
                    Items.Remove(temp[i]);
                }

                var reOrderList = new ObservableCollection<Item>(Items.OrderBy(item => item.Index));
                var newList = new ObservableCollection<Item>();
                for (int i = 0; i < reOrderList.Count; i++)
                {
                    reOrderList[i].Index = (uint)(newList.Count + Equipments.Count + 1);
                    newList.Add(reOrderList[i]);
                }
                Items = newList;
                OnPropertyChanged("Items");
            }
        }

        private void DeleteEquipment(object? parameter)
        {
            if (parameter == null)
                return;
            List<Item> itemsToDelete = null;
            if (parameter is Item)
            {
                itemsToDelete = new List<Item> { parameter as Item };
            }
            else
            {
                itemsToDelete = (parameter as IEnumerable<object>).Select(item => item as Item).ToList();
            }

            if (itemsToDelete != null && itemsToDelete.Count > 0)
            {
                List<Item> temp = new List<Item>();
                foreach (Item selectedItem in itemsToDelete)
                {
                    temp.Add(selectedItem);
                }

                for (int i = 0; i < temp.Count; i++)
                {
                    temp[i].Empty();
                    Equipments.Remove(temp[i]);
                }

                var reOrderList = new ObservableCollection<Item>(Equipments.OrderBy(item => item.Index));
                var newList = new ObservableCollection<Item>();
                // 更新每个元素的Index属性值
                for (int i = 0; i < reOrderList.Count; i++)
                {
                    reOrderList[i].Index = (uint)(newList.Count + Equipments.Count + 1);
                    newList.Add(reOrderList[i]);
                }
                Equipments = newList;
                OnPropertyChanged("Equipments");
            }
        }

        private void EditEquipment(object? parameter)
        {
            if (parameter == null)
                return;
            List<Item> itemsToEdit = null;
            if (parameter is Item)
            {
                itemsToEdit = new List<Item> { parameter as Item };
            }
            else
            {
                itemsToEdit = (parameter as IEnumerable<object>).Select(item => item as Item).ToList();
            }
            if (itemsToEdit == null || itemsToEdit.Count == 0)
                return;
            Item? item = itemsToEdit[0];
            var dlg = new ChoiceWindow();
            dlg.ID = item.ID;
            dlg.ShowDialog();
            if (!dlg.Confirmed)
                return;
            foreach (Item selectedItem in itemsToEdit)
            {
                selectedItem.ID = dlg.ID;
            }
        }

        private void AppendEquipment(object? parameter)
        {
            AppendItem(1);
        }

        private void AppendItem(int type)
        {
            uint index = (uint)(Items.Count + Equipments.Count);
            if (index >= 3800) return;

            var dlg = new ChoiceWindow();
            dlg.ShowDialog();
            if (dlg.ID == 0)
                return;
            var selectedItems = dlg.ListBoxItem.SelectedItems.Count == 1 ? new List<NameValueInfo> { dlg.ListBoxItem.SelectedItem as NameValueInfo } : dlg.ListBoxItem.SelectedItems;

            uint count = dlg.Count;
            switch (type)
            {
                case 0:
                    for (int i = 0; i < selectedItems.Count; i++)
                    {
                        index = (uint)(Items.Count + Equipments.Count);
                        if (index >= 3800) return;
                        var item1 = new Item(0xA0 + index * 20);
                        item1.Status = 2;
                        item1.ID = ((NameValueInfo)selectedItems[i]).Value;
                        item1.Index = index + 1;
                        item1.Count = count;
                        Items.Add(item1);
                    }
                    break;
                case 1:
                    for (int i = 0; i < selectedItems.Count; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            index = (uint)(Items.Count + Equipments.Count);
                            if (index >= 3800) return;
                            var itemtemp = new Item(0xA0 + index * 20);
                            itemtemp.Status = 3;
                            itemtemp.ID = ((NameValueInfo)selectedItems[i]).Value;
                            itemtemp.Index = index + 1;
                            itemtemp.Equipment1 = 255;
                            itemtemp.Equipment2 = 255;
                            Equipments.Add(itemtemp);
                        }
                    }
                    break;
                default:
                    break;
            }

            return;
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
            buffer = ProcessingCharacter(buffer);

            uint address = 0x2AF40 + (uint)index * 464;

            uint id = SaveData.Instance().ReadNumber(address, 4);
            Array.Copy(BitConverter.GetBytes(id), buffer, 4);
            SaveData.Instance().WriteValue(address, buffer);
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

            System.IO.File.WriteAllBytes(dlg.FileName, buffer);
        }

        private void InsertCharacter(object? parameter)
        {
            uint count = (uint)Characters.Count;
            if (count >= 500) return;

            var dlg = new OpenFileDialog();
            dlg.Filter = "Unicorn Overlord Character's Dump|*.uocd";
            if (dlg.ShowDialog() == false) return;

            Byte[] buffer = System.IO.File.ReadAllBytes(dlg.FileName);
            if (buffer.Length != 464) return;

            buffer = ProcessingCharacter(buffer);
            uint id = SaveData.Instance().ReadNumber(0x63980, 4) + 1;
            Array.Copy(BitConverter.GetBytes(id), buffer, 4);
            uint address = 0x2AF40 + count * 464;
            SaveData.Instance().WriteValue(address, buffer);

            SaveData.Instance().WriteNumber(0x63980, 4, id);
            count = SaveData.Instance().ReadNumber(0x63984, 4);
            SaveData.Instance().WriteNumber(0x63984, 4, count + 1);

            InsertFriendship(id);

            Initialize();
        }

        private void ChangeItemCountMax(object? parameter)
        {
            foreach (var item in Items)
            {
                if (item.ID <= 4) continue;
                item.Count = 99;
            }
        }

        private Byte[] ProcessingCharacter(Byte[] buffer)
        {
            // formation clear
            Array.Copy(BitConverter.GetBytes(0xFFFFFFFF), 0, buffer, 4, 4);
            buffer[32] = 0xFF;

            // buffer[460]
            // character's status
            // 1Bit => formation join
            // 3Bit => join
            // 4Bit => mercenary?
            // 5Bit => use
            buffer[460] &= 0xFE;

            // equipment clear
            // elements => 4Byte
            // count => 4
            // (or Append Item)
            Array.Clear(buffer, 76, 16);
            return buffer;
        }

        private void InsertFriendship(uint id)
        {
            for (uint index = 0; index < 164; index++)
            {
                uint baseAddress = 0x1B5830 + index * 1316;
                var current_id = SaveData.Instance().ReadNumber(baseAddress, 4);

                // chack blank character
                if (current_id == 0xFFFFFFFF)
                {
                    // insert new character
                    SaveData.Instance().WriteNumber(baseAddress, 4, id);
                    for (uint count = 0; count < Characters.Count; count++)
                    {
                        uint address = baseAddress + 4 + count * 8;
                        // insert existing character
                        SaveData.Instance().WriteNumber(address, 4, Characters[(int)count].ID);
                    }
                    return;
                }

                // existing character
                for (uint count = 0; count < 164; count++)
                {
                    uint address = baseAddress + 4 + count * 8;
                    if (SaveData.Instance().ReadNumber(address, 4) == 0xFFFFFFFF)
                    {
                        // insert new character
                        SaveData.Instance().WriteNumber(address, 4, id);
                        break;
                    }
                }
            }
        }

        public void ChangeLanguage(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            var selectedItem = comboBox.SelectedItem.ToString();
            if (Languages.Contains(selectedItem))
            {
                Info.Instance().CurrentSelectedLanguage = Languages.IndexOf(selectedItem);
            }
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Equipments));
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
