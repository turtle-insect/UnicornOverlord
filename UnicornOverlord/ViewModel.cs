using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace UnicornOverlord
{
	internal class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private readonly Info Info = Info.Instance();

		// Commands
		public ICommand OpenFileCommand { get; set; }
		public ICommand SaveFileCommand { get; set; }
		public ICommand SaveAsFileCommand { get; set; }
		public ICommand CloseFileCommand { get; set; }

		// Basic Tab Commands
		public ICommand MaxGoldCommand { get; set; }
		public ICommand MaxRenownCommand { get; set; }
		public ICommand MaxMedalsCommand { get; set; }
		public ICommand MaxCoinsCommand { get; set; }
		public ICommand MaxDivineShardsCommand { get; set; }
		public ICommand MaxAllCurrenciesCommand { get; set; }

		// Character Commands
		public ICommand ChoiceClassCommand { get; set; }
		public ICommand SetMaxLevelCommand { get; set; }
		public ICommand SetMaxLevelAllCommand { get; set; }
		public ICommand MaxDewsCommand { get; set; }
		public ICommand MaxDewsUncappedCommand { get; set; }
		public ICommand ResetDewsCommand { get; set; }
		public ICommand MaxDewsAllCommand { get; set; }
		public ICommand MaxBondsSelectedCommand { get; set; }
		public ICommand MaxBondsAllCommand { get; set; }
		public ICommand ExportCharacterCommand { get; set; }
		public ICommand ImportCharacterCommand { get; set; }
		public ICommand InsertCharacterCommand { get; set; }
		public ICommand CloneCharacterCommand { get; set; }

		// Item Commands
		public ICommand ChoiceItemCommand { get; set; }
		public ICommand AppendItemCommand { get; set; }
		public ICommand DeleteItemCommand { get; set; }
		public ICommand ChangeItemCount99Command { get; set; }
		public ICommand ChangeItemCount999Command { get; set; }
		public ICommand AddAllConsumablesCommand { get; set; }
		public ICommand AddAllMaterialsCommand { get; set; }

		// Equipment Commands
		public ICommand ChoiceEquipmentCommand { get; set; }
		public ICommand AppendEquipmentCommand { get; set; }
		public ICommand DeleteEquipmentCommand { get; set; }
		public ICommand UnequipItemCommand { get; set; }
		public ICommand AddAllWeaponsCommand { get; set; }
		public ICommand AddAllShieldsCommand { get; set; }
		public ICommand AddAllAccessoriesCommand { get; set; }
		public ICommand UpgradeAllEquipmentCommand { get; set; }
		public ICommand UpgradeAllWeaponsCommand => UpgradeAllEquipmentCommand;

		// Unit Commands
		public ICommand UnlockAllUnitsCommand { get; set; }

		// Collections
		public Basic Basic { get; set; } = new Basic();
		public ObservableCollection<Character> Characters { get; set; } = new ObservableCollection<Character>();
		public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
		public ObservableCollection<Item> Equipments { get; set; } = new ObservableCollection<Item>();
		public ObservableCollection<Unit> Units { get; set; } = new ObservableCollection<Unit>();

		public ICollectionView FilteredCharacters { get; private set; }
		public ICollectionView FilteredItems { get; private set; }
		public ICollectionView FilteredEquipments { get; private set; }

		public List<GrowthTypeInfo> GrowthTypes => GrowthTypeInfo.All;
		public List<NameValueInfo> Classes => Info.Class;

		private Character? mSelectedCharacter;
		public Character? SelectedCharacter
		{
			get => mSelectedCharacter;
			set
			{
				mSelectedCharacter = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCharacter)));
			}
		}

		private Item? mSelectedItem;
		public Item? SelectedItem
		{
			get => mSelectedItem;
			set
			{
				mSelectedItem = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
			}
		}

		private Item? mSelectedEquipment;
		public Item? SelectedEquipment
		{
			get => mSelectedEquipment;
			set
			{
				mSelectedEquipment = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedEquipment)));
			}
		}

		private string mCharacterFilterText = string.Empty;
		public string CharacterFilterText
		{
			get => mCharacterFilterText;
			set
			{
				mCharacterFilterText = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharacterFilterText)));
				FilteredCharacters.Refresh();
			}
		}

		private string mItemFilterText = string.Empty;
		public string ItemFilterText
		{
			get => mItemFilterText;
			set
			{
				mItemFilterText = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemFilterText)));
				FilteredItems.Refresh();
			}
		}

		private string mEquipmentFilterText = string.Empty;
		public string EquipmentFilterText
		{
			get => mEquipmentFilterText;
			set
			{
				mEquipmentFilterText = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EquipmentFilterText)));
				FilteredEquipments.Refresh();
			}
		}

		private string mStatusMessage = "Ready. Please open a Unicorn Overlord save file (UCSAVEFILE*.DAT).";
		public string StatusMessage
		{
			get => mStatusMessage;
			set
			{
				mStatusMessage = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
			}
		}

		public bool SuppressDialogs { get; set; } = false;

		public bool IsFileLoaded => SaveData.Instance().IsLoaded;
		public string LoadedFileName => Path.GetFileName(SaveData.Instance().FileName);
		public string LoadedFilePath => SaveData.Instance().FileName;

		public string WindowTitle
		{
			get
			{
				if (!IsFileLoaded) return "Unicorn Overlord Save Editor (Nintendo Switch)";
				string dirty = SaveData.Instance().IsDirty ? " *" : "";
				return $"{LoadedFileName} (Slot {Basic.SaveSlot}) — Unicorn Overlord Save Editor{dirty}";
			}
		}

		public string SummaryInfo
		{
			get
			{
				if (!IsFileLoaded) return "No file loaded";
				return $"Characters: {Characters.Count} | Items: {Items.Count} | Equipment: {Equipments.Count} | Time: {Basic.PlayTimeFormatted}";
			}
		}

		public ViewModel()
		{
			// Initialize CollectionViews for search filtering
			FilteredCharacters = CollectionViewSource.GetDefaultView(Characters);
			FilteredCharacters.Filter = FilterCharacterPredicate;

			FilteredItems = CollectionViewSource.GetDefaultView(Items);
			FilteredItems.Filter = FilterItemPredicate;

			FilteredEquipments = CollectionViewSource.GetDefaultView(Equipments);
			FilteredEquipments.Filter = FilterEquipmentPredicate;

			// Commands
			OpenFileCommand = new ActionCommand(OpenFile);
			SaveFileCommand = new ActionCommand(SaveFile);
			SaveAsFileCommand = new ActionCommand(SaveAsFile);
			CloseFileCommand = new ActionCommand(CloseFile);

			MaxGoldCommand = new ActionCommand(_ => { Basic.Money = 9999999; UpdateStatus("Gold set to 9,999,999"); });
			MaxRenownCommand = new ActionCommand(_ => { Basic.Fame = 5000; Basic.RenownRank = 5; UpdateStatus("Renown set to 5,000 (Rank S)"); });
			MaxMedalsCommand = new ActionCommand(_ => { Basic.Medals = 9999; ReloadItemsFromSave(); UpdateStatus("Medals set to 9,999"); });
			MaxCoinsCommand = new ActionCommand(_ => { Basic.ColiseumCoins = 99999; ReloadItemsFromSave(); UpdateStatus("Coliseum Coins set to 99,999"); });
			MaxDivineShardsCommand = new ActionCommand(_ => { Basic.DivineShards = 999; ReloadItemsFromSave(); UpdateStatus("Divine Shards set to 999"); });
			MaxAllCurrenciesCommand = new ActionCommand(MaxAllCurrencies);

			ChoiceItemCommand = new ActionCommand(ChoiceItem);
			ChoiceEquipmentCommand = new ActionCommand(ChoiceEquipment);
			ChoiceClassCommand = new ActionCommand(ChoiceClass);
			AppendItemCommand = new ActionCommand(AppendItem);
			DeleteItemCommand = new ActionCommand(DeleteItem);
			ChangeItemCount99Command = new ActionCommand(_ => ChangeAllItemsCount(99));
			ChangeItemCount999Command = new ActionCommand(_ => ChangeAllItemsCount(999));
			AddAllConsumablesCommand = new ActionCommand(AddAllConsumables);
			AddAllMaterialsCommand = new ActionCommand(AddAllMaterials);

			AppendEquipmentCommand = new ActionCommand(AppendEquipment);
			DeleteEquipmentCommand = new ActionCommand(DeleteEquipment);
			UnequipItemCommand = new ActionCommand(UnequipEquipment);
			AddAllWeaponsCommand = new ActionCommand(AddAllWeapons);
			AddAllShieldsCommand = new ActionCommand(AddAllShields);
			AddAllAccessoriesCommand = new ActionCommand(AddAllAccessories);
			UpgradeAllEquipmentCommand = new ActionCommand(UpgradeAllEquipment);

			SetMaxLevelCommand = new ActionCommand(SetMaxLevelSelected);
			SetMaxLevelAllCommand = new ActionCommand(SetMaxLevelAll);
			MaxDewsCommand = new ActionCommand(_ => SetDewsSelected(5));
			MaxDewsUncappedCommand = new ActionCommand(_ => SetDewsSelected(255));
			ResetDewsCommand = new ActionCommand(_ => SetDewsSelected(0));
			MaxDewsAllCommand = new ActionCommand(SetDewsAll);
			MaxBondsSelectedCommand = new ActionCommand(MaxBondsSelected);
			MaxBondsAllCommand = new ActionCommand(MaxBondsAll);

			ExportCharacterCommand = new ActionCommand(ExportCharacter);
			ImportCharacterCommand = new ActionCommand(ImportCharacter);
			InsertCharacterCommand = new ActionCommand(InsertCharacter);
			CloneCharacterCommand = new ActionCommand(CloneCharacter);

			UnlockAllUnitsCommand = new ActionCommand(UnlockAllUnits);
		}

		private bool FilterCharacterPredicate(object obj)
		{
			if (string.IsNullOrWhiteSpace(CharacterFilterText)) return true;
			if (obj is not Character ch) return false;

			string filter = CharacterFilterText.Trim();
			return ch.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
				   ch.ClassName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
				   ch.ID.ToString().Contains(filter) ||
				   ch.Lv.ToString().Equals(filter, StringComparison.OrdinalIgnoreCase);
		}

		private bool FilterItemPredicate(object obj)
		{
			if (string.IsNullOrWhiteSpace(ItemFilterText)) return true;
			if (obj is not Item it) return false;

			string filter = ItemFilterText.Trim();
			return it.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
				   it.CategoryName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
				   it.ID.ToString().Contains(filter) ||
				   it.Count.ToString().Equals(filter);
		}

		private bool FilterEquipmentPredicate(object obj)
		{
			if (string.IsNullOrWhiteSpace(EquipmentFilterText)) return true;
			if (obj is not Item eq) return false;

			string filter = EquipmentFilterText.Trim();
			return eq.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
				   eq.CategoryName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
				   eq.EquippedCharacterName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
				   eq.ID.ToString().Contains(filter);
		}

		public void Initialize()
		{
			Characters.Clear();
			Items.Clear();
			Equipments.Clear();
			Units.Clear();

			var sd = SaveData.Instance();
			if (!sd.IsLoaded) return;

			// 1. Read Bonds
			var bondDictionary = new Dictionary<uint, ObservableCollection<Bond>>();
			for (uint index = 0; index < Util.MaxBonds; index++)
			{
				uint baseAddress = Util.calcBondAddress(index);
				uint id = sd.ReadNumber(baseAddress, 4);
				if (id == 0xFFFFFFFF) break;

				var bonds = new ObservableCollection<Bond>();
				bondDictionary[id] = bonds;
				for (uint count = 0; count < Util.MaxBonds; count++)
				{
					uint address = baseAddress + 4 + count * 8;
					uint partnerId = sd.ReadNumber(address, 4);
					if (partnerId == 0xFFFFFFFF) break;

					bonds.Add(new Bond(address));
				}
			}

			// 2. Read Characters
			var characterMap = new Dictionary<uint, string>();
			for (uint i = 0; i < Util.MaxCharacters; i++)
			{
				uint charAddr = Util.calcCharacterAddress(i);
				uint charId = sd.ReadNumber(charAddr, 4);
				if (charId == 0xFFFFFFFF) break;

				var ch = new Character(charAddr, i);
				if (bondDictionary.TryGetValue(ch.ID, out var bonds))
				{
					ch.Bonds = bonds;
				}

				Characters.Add(ch);
				characterMap[ch.ID] = ch.DisplayName;
			}

			// 3. Resolve partner names on bonds
			foreach (var ch in Characters)
			{
				if (ch.Bonds == null) continue;
				foreach (var bond in ch.Bonds)
				{
					if (characterMap.TryGetValue(bond.ID, out var pName))
					{
						bond.PartnerName = pName;
					}
					else
					{
						bond.PartnerName = Info.GetCharacterName(bond.ID);
					}
				}
			}

			// 4. Read Items & Equipment
			for (uint i = 0; i < Util.MaxItemSlots; i++)
			{
				uint itemAddr = Util.calcItemAddress(i);
				uint index = sd.ReadNumber(itemAddr + 4, 4);
				if (index == 0) break;

				var item = new Item(itemAddr);
				if (item.Count == 0 && Info.IsEquipment(item.ID))
				{
					Equipments.Add(item);
				}
				else
				{
					Items.Add(item);
				}
			}

			// 5. Read Units
			for (uint i = 0; i < Util.MaxUnits; i++)
			{
				var unit = new Unit(Util.calcUnitAddress(i), i);
				Units.Add(unit);
			}

			// Select first items if available
			if (Characters.Count > 0) SelectedCharacter = Characters[0];
			if (Items.Count > 0) SelectedItem = Items[0];
			if (Equipments.Count > 0) SelectedEquipment = Equipments[0];

			Basic.RefreshAll();
			UpdateStatus($"Successfully loaded save: {LoadedFileName} (Slot {Basic.SaveSlot})");
			NotifyHeaderProperties();
		}

		private void ReloadItemsFromSave()
		{
			Items.Clear();
			Equipments.Clear();
			var sd = SaveData.Instance();
			for (uint i = 0; i < Util.MaxItemSlots; i++)
			{
				uint itemAddr = Util.calcItemAddress(i);
				uint index = sd.ReadNumber(itemAddr + 4, 4);
				if (index == 0) break;

				var item = new Item(itemAddr);
				if (item.Count == 0 && Info.IsEquipment(item.ID))
					Equipments.Add(item);
				else
					Items.Add(item);
			}
			FilteredItems.Refresh();
			FilteredEquipments.Refresh();
			NotifyHeaderProperties();
		}

		private void NotifyHeaderProperties()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Basic)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFileLoaded)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadedFileName)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadedFilePath)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowTitle)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SummaryInfo)));
		}

		private void UpdateStatus(string message)
		{
			StatusMessage = message;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowTitle)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SummaryInfo)));
		}

		public void RefreshLanguage()
		{
			Info.Initialize();
			foreach (var ch in Characters) ch.RefreshAll();
			foreach (var item in Items) item.RefreshAll();
			foreach (var eq in Equipments) eq.RefreshAll();
			FilteredCharacters.Refresh();
			FilteredItems.Refresh();
			FilteredEquipments.Refresh();
			NotifyHeaderProperties();
		}

		#region File Operations
		private void OpenFile(object? parameter)
		{
			if (SaveData.Instance().IsDirty)
			{
				var res = MessageBox.Show("You have unsaved changes. Do you want to save before opening another file?",
					"Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
				if (res == MessageBoxResult.Yes)
				{
					if (!SaveData.Instance().Save())
					{
						MessageBox.Show("Failed to save the current file.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}
				}
				else if (res == MessageBoxResult.Cancel)
				{
					return;
				}
			}

			var dlg = new OpenFileDialog
			{
				Filter = "Unicorn Overlord Save (*.DAT)|UCSAVEFILE*.DAT|All Files (*.*)|*.*",
				Title = "Open Unicorn Overlord Save File"
			};
			if (dlg.ShowDialog() != true) return;

			if (!SaveData.Instance().Open(dlg.FileName))
			{
				MessageBox.Show("Failed to open save file. Please ensure it is a valid decrypted Unicorn Overlord Switch save with 'UCSD' header.",
					"Invalid Save File", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			Initialize();
		}

		private void SaveFile(object? parameter)
		{
			if (!SaveData.Instance().IsLoaded) return;

			if (SaveData.Instance().Save())
			{
				UpdateStatus($"Successfully saved {LoadedFileName} (Backup created in backup folder).");
				if (!SuppressDialogs)
				{
					MessageBox.Show($"Save file '{LoadedFileName}' saved successfully!\nA safe timestamped backup was created in the 'backup' folder.",
						"File Saved", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			else
			{
				if (!SuppressDialogs)
				{
					MessageBox.Show("Error saving file. Please ensure the file is not locked or write-protected.",
						"Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void SaveAsFile(object? parameter)
		{
			if (!SaveData.Instance().IsLoaded) return;

			var dlg = new SaveFileDialog
			{
				Filter = "Unicorn Overlord Save (*.DAT)|UCSAVEFILE*.DAT|All Files (*.*)|*.*",
				FileName = LoadedFileName,
				Title = "Save Unicorn Overlord Save As"
			};
			if (dlg.ShowDialog() != true) return;

			if (SaveData.Instance().SaveAs(dlg.FileName))
			{
				UpdateStatus($"Saved as {Path.GetFileName(dlg.FileName)}");
				NotifyHeaderProperties();
				if (!SuppressDialogs)
				{
					MessageBox.Show("Save file saved successfully!", "File Saved", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			else
			{
				if (!SuppressDialogs)
				{
					MessageBox.Show("Error saving file.", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void CloseFile(object? parameter)
		{
			if (!SaveData.Instance().IsLoaded) return;
			if (SaveData.Instance().IsDirty)
			{
				var res = MessageBox.Show("Save changes before closing?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
				if (res == MessageBoxResult.Yes) SaveFile(null);
				else if (res == MessageBoxResult.Cancel) return;
			}

			Characters.Clear();
			Items.Clear();
			Equipments.Clear();
			Units.Clear();
			UpdateStatus("File closed.");
			NotifyHeaderProperties();
		}
		#endregion

		#region Currency Operations
		private void MaxAllCurrencies(object? parameter)
		{
			if (!SaveData.Instance().IsLoaded) return;

			Basic.Money = 9999999;
			Basic.Fame = 5000;
			Basic.RenownRank = 5;
			Basic.Medals = 9999;
			Basic.ColiseumCoins = 99999;
			Basic.DivineShards = 999;
			Basic.HallowedAsh = 99;
			Basic.LuminousAsh = 99;

			ReloadItemsFromSave();
			UpdateStatus("Maxed all currencies, renown (Rank S), medals, and coins!");
		}
		#endregion

		#region Character Operations
		private void ChoiceClass(object? parameter)
		{
			var ch = parameter as Character ?? SelectedCharacter;
			if (ch == null) return;

			var dlg = new ChoiceWindow
			{
				Type = ChoiceWindow.eType.eClass,
				ID = ch.Class,
				Owner = Application.Current.MainWindow
			};
			if (dlg.ShowDialog() == true)
			{
				ch.Class = dlg.ID;
				UpdateStatus($"Changed {ch.DisplayName}'s class to {ch.ClassName}");
			}
		}

		private void SetMaxLevelSelected(object? parameter)
		{
			var ch = parameter as Character ?? SelectedCharacter;
			if (ch == null) return;

			ch.SetMaxLevel();
			UpdateStatus($"Set {ch.DisplayName} to Lv. 50 with max exp.");
		}

		private void SetMaxLevelAll(object? parameter)
		{
			if (Characters.Count == 0) return;
			if (!SuppressDialogs)
			{
				var res = MessageBox.Show($"Are you sure you want to set ALL {Characters.Count} characters to Lv. 50?",
					"Confirm Max Level All", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (res != MessageBoxResult.Yes) return;
			}

			foreach (var ch in Characters)
			{
				ch.SetMaxLevel();
			}
			UpdateStatus($"Set all {Characters.Count} characters to Lv. 50.");
		}

		private void SetDewsSelected(uint amount)
		{
			var ch = SelectedCharacter;
			if (ch == null) return;

			ch.SetMaxDews(amount);
			UpdateStatus($"Set all 10 dew stats to {amount} for {ch.DisplayName}.");
		}

		private void SetDewsAll(object? parameter)
		{
			if (Characters.Count == 0) return;
			if (!SuppressDialogs)
			{
				var res = MessageBox.Show($"Set all dew stats to 5 (Legit Max) for ALL {Characters.Count} characters?",
					"Confirm Max Dews All", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (res != MessageBoxResult.Yes) return;
			}

			foreach (var ch in Characters)
			{
				ch.SetMaxDews(5);
			}
			UpdateStatus($"Applied max dews (5) to all {Characters.Count} characters.");
		}

		private void MaxBondsSelected(object? parameter)
		{
			var ch = parameter as Character ?? SelectedCharacter;
			if (ch == null) return;

			ch.MaximizeAllBonds();
			UpdateStatus($"Maxed all rapport bonds (1000) for {ch.DisplayName}.");
		}

		private void MaxBondsAll(object? parameter)
		{
			if (Characters.Count == 0) return;
			if (!SuppressDialogs)
			{
				var res = MessageBox.Show("Maximize rapport bonds (1000 pts) for EVERY character relationship in the entire army?",
					"Confirm Max All Bonds", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (res != MessageBoxResult.Yes) return;
			}

			foreach (var ch in Characters)
			{
				ch.MaximizeAllBonds();
			}
			UpdateStatus("Maximized all rapport bonds across the entire army!");
		}

		private void ExportCharacter(object? parameter)
		{
			var ch = SelectedCharacter;
			if (ch == null) return;

			var dlg = new SaveFileDialog
			{
				Filter = "Unicorn Overlord Character Dump (*.uocd)|*.uocd",
				FileName = $"{ch.DisplayName}_{ch.ClassName}.uocd",
				Title = $"Export Character ({ch.DisplayName})"
			};
			if (dlg.ShowDialog() != true) return;

			uint address = Util.calcCharacterAddress(ch.SlotIndex);
			byte[] buffer = SaveData.Instance().ReadValue(address, Util.CharacterEntrySize);
			File.WriteAllBytes(dlg.FileName, buffer);
			UpdateStatus($"Exported character '{ch.DisplayName}' to {Path.GetFileName(dlg.FileName)}");
		}

		private void ImportCharacter(object? parameter)
		{
			var ch = SelectedCharacter;
			if (ch == null) return;

			var dlg = new OpenFileDialog
			{
				Filter = "Unicorn Overlord Character Dump (*.uocd)|*.uocd",
				Title = $"Import/Replace Character ({ch.DisplayName})"
			};
			if (dlg.ShowDialog() != true) return;

			byte[] buffer = File.ReadAllBytes(dlg.FileName);
			if (buffer.Length != Util.CharacterEntrySize)
			{
				MessageBox.Show($"Invalid character dump file size. Expected {Util.CharacterEntrySize} bytes.",
					"Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			buffer = ProcessingCharacter(buffer);
			uint address = Util.calcCharacterAddress(ch.SlotIndex);

			// Preserve existing character ID
			uint id = SaveData.Instance().ReadNumber(address, 4);
			Array.Copy(BitConverter.GetBytes(id), buffer, 4);
			SaveData.Instance().WriteValue(address, buffer);

			int idx = (int)ch.SlotIndex;
			var updated = new Character(address, (uint)idx);
			if (ch.Bonds != null) updated.Bonds = ch.Bonds;
			Characters[idx] = updated;
			SelectedCharacter = updated;

			UpdateStatus($"Replaced character at slot {idx + 1} with dump file.");
		}

		private void InsertCharacter(object? parameter)
		{
			uint count = (uint)Characters.Count;
			if (count >= Util.MaxCharacters)
			{
				MessageBox.Show("Maximum character capacity (500) reached.", "Cannot Insert", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			var dlg = new OpenFileDialog
			{
				Multiselect = true,
				Filter = "Unicorn Overlord Character Dump (*.uocd)|*.uocd",
				Title = "Insert Character Dumps"
			};
			if (dlg.ShowDialog() != true) return;

			uint inserted = 0;
			foreach (string filename in dlg.FileNames)
			{
				count = (uint)Characters.Count;
				if (count >= Util.MaxCharacters) break;

				byte[] buffer = File.ReadAllBytes(filename);
				if (buffer.Length != Util.CharacterEntrySize) continue;

				buffer = ProcessingCharacter(buffer);
				uint id = SaveData.Instance().ReadNumber(0x63980, 4) + 1;
				Array.Copy(BitConverter.GetBytes(id), buffer, 4);

				uint address = Util.calcCharacterAddress(count);
				SaveData.Instance().WriteValue(address, buffer);

				SaveData.Instance().WriteNumber(0x63980, 4, id);
				uint activeCount = SaveData.Instance().ReadNumber(0x63984, 4);
				SaveData.Instance().WriteNumber(0x63984, 4, activeCount + 1);

				InsertFriendship(id);

				var ch = new Character(address, count);
				Characters.Add(ch);
				inserted++;
			}

			UpdateStatus($"Inserted {inserted} new character(s) into your army.");
		}

		private void CloneCharacter(object? parameter)
		{
			var ch = parameter as Character ?? SelectedCharacter;
			if (ch == null) return;

			uint count = (uint)Characters.Count;
			if (count >= Util.MaxCharacters)
			{
				if (!SuppressDialogs)
					MessageBox.Show($"Maximum character capacity ({Util.MaxCharacters}) reached.", "Army Full", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			uint srcAddr = Util.calcCharacterAddress(ch.SlotIndex);
			byte[] buffer = SaveData.Instance().ReadValue(srcAddr, Util.CharacterEntrySize);
			buffer = ProcessingCharacter(buffer);

			uint id = SaveData.Instance().ReadNumber(0x63980, 4) + 1;
			Array.Copy(BitConverter.GetBytes(id), buffer, 4);

			uint newAddr = Util.calcCharacterAddress(count);
			SaveData.Instance().WriteValue(newAddr, buffer);

			SaveData.Instance().WriteNumber(0x63980, 4, id);
			uint activeCount = SaveData.Instance().ReadNumber(0x63984, 4);
			SaveData.Instance().WriteNumber(0x63984, 4, activeCount + 1);

			InsertFriendship(id);

			var newCh = new Character(newAddr, count);
			Characters.Add(newCh);
			SelectedCharacter = newCh;
			FilteredCharacters.Refresh();
			UpdateStatus($"Cloned '{ch.DisplayName}' into slot #{count + 1} (Total army: {Characters.Count}/{Util.MaxCharacters}).");
		}

		private byte[] ProcessingCharacter(byte[] buffer)
		{
			// Clear formation assignment
			Array.Copy(BitConverter.GetBytes(0xFFFFFFFF), 0, buffer, 4, 4);
			buffer[32] = 0xFF; // Not in unit

			// Status byte 460: clear formation join bit
			buffer[460] &= 0xFE;

			// Clear equipped items slots (76..91: 16 bytes)
			Array.Clear(buffer, 76, 16);

			return buffer;
		}

		private void InsertFriendship(uint id)
		{
			var sd = SaveData.Instance();
			for (uint index = 0; index < Util.MaxBonds; index++)
			{
				uint baseAddress = Util.calcBondAddress(index);
				var currentId = sd.ReadNumber(baseAddress, 4);

				if (currentId == 0xFFFFFFFF)
				{
					// Insert new character row
					sd.WriteNumber(baseAddress, 4, id);
					for (uint count = 0; count < Characters.Count; count++)
					{
						uint address = baseAddress + 4 + count * 8;
						sd.WriteNumber(address, 4, Characters[(int)count].ID);
					}
					return;
				}

				// Append to existing character's column
				for (uint count = 0; count < Util.MaxBonds; count++)
				{
					uint address = baseAddress + 4 + count * 8;
					if (sd.ReadNumber(address, 4) == 0xFFFFFFFF)
					{
						sd.WriteNumber(address, 4, id);
						break;
					}
				}
			}
		}
		#endregion

		#region Item Operations
		private void ChoiceItem(object? parameter)
		{
			var item = parameter as Item ?? SelectedItem;
			if (item == null) return;

			var dlg = new ChoiceWindow
			{
				Type = ChoiceWindow.eType.eItem,
				ID = item.ID,
				Owner = Application.Current.MainWindow
			};
			if (dlg.ShowDialog() == true && dlg.ID != 0)
			{
				item.ID = dlg.ID;
				item.Status = 2;
				if (Info.KindDict.TryGetValue(item.ID, out var kind) && uint.TryParse(kind.Name, out uint st))
				{
					item.Status = st;
				}
				UpdateStatus($"Changed item to {item.Name}");
			}
		}

		private void AppendItem(object? parameter)
		{
			uint totalSlots = (uint)(Items.Count + Equipments.Count);
			if (totalSlots >= Util.MaxItemSlots)
			{
				MessageBox.Show("Inventory capacity reached (3,800 slots).", "Inventory Full", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			var dlg = new ChoiceWindow
			{
				Type = ChoiceWindow.eType.eItem,
				Owner = Application.Current.MainWindow
			};
			if (dlg.ShowDialog() != true || dlg.ID == 0) return;

			uint maxIndex = GetMaxItemIndex();
			uint slotAddr = Util.calcItemAddress(totalSlots);
			var sd = SaveData.Instance();

			sd.WriteNumber(slotAddr, 4, dlg.ID);
			sd.WriteNumber(slotAddr + 4, 4, maxIndex + 1);
			sd.WriteNumber(slotAddr + 8, 3, 1);
			sd.WriteNumber(slotAddr + 11, 1, 0xFF);
			sd.WriteNumber(slotAddr + 12, 4, 0xFFFFFFFF);

			uint status = 2;
			if (Info.KindDict.TryGetValue(dlg.ID, out var kind) && uint.TryParse(kind.Name, out uint st)) status = st;
			sd.WriteNumber(slotAddr + 16, 4, status);

			var newItem = new Item(slotAddr);
			Items.Add(newItem);
			SelectedItem = newItem;
			FilteredItems.Refresh();
			UpdateStatus($"Added '{newItem.Name}' to inventory.");
		}

		private void DeleteItem(object? parameter)
		{
			var item = parameter as Item ?? SelectedItem;
			if (item == null) return;

			if (!SuppressDialogs)
			{
				var res = MessageBox.Show($"Delete '{item.Name}' (Count: {item.Count}) from inventory?",
					"Confirm Delete Item", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (res != MessageBoxResult.Yes) return;
			}

			DeleteSlotAtAddress(item.MemoryAddress);
			ReloadItemsFromSave();
			UpdateStatus($"Deleted item '{item.Name}'.");
		}

		private void ChangeAllItemsCount(uint count)
		{
			foreach (var item in Items)
			{
				if (item.ID <= 7) continue; // Keep currencies & ash intact
				item.Count = count;
			}
			UpdateStatus($"Set all consumable & material counts to {count}.");
		}

		private void AddAllConsumables(object? parameter)
		{
			var consumableIds = Info.Item
				.Where(it => it.Category == "Consumables" || it.Category == "Recovery / Herbs")
				.Select(it => it.Value)
				.ToList();

			AddBatch(consumableIds, isEquipment: false, defaultCount: 99, "Consumables");
		}

		private void AddAllMaterials(object? parameter)
		{
			var matIds = Info.Item
				.Where(it => it.Category == "Valuables / Materials")
				.Select(it => it.Value)
				.ToList();

			AddBatch(matIds, isEquipment: false, defaultCount: 99, "Materials & Valuables");
		}
		#endregion

		#region Equipment Operations
		private void ChoiceEquipment(object? parameter)
		{
			var eq = parameter as Item ?? SelectedEquipment;
			if (eq == null) return;

			var dlg = new ChoiceWindow
			{
				Type = ChoiceWindow.eType.eEquipment,
				ID = eq.ID,
				Owner = Application.Current.MainWindow
			};
			if (dlg.ShowDialog() == true && dlg.ID != 0)
			{
				eq.ID = dlg.ID;
				uint st = 5;
				if (Info.KindDict.TryGetValue(eq.ID, out var kind) && uint.TryParse(kind.Name, out uint k)) st = k;
				eq.Status = st;
				UpdateStatus($"Changed equipment to {eq.Name}");
			}
		}

		private void AppendEquipment(object? parameter)
		{
			uint totalSlots = (uint)(Items.Count + Equipments.Count);
			if (totalSlots >= Util.MaxItemSlots)
			{
				MessageBox.Show("Inventory capacity reached (3,800 slots).", "Inventory Full", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			var dlg = new ChoiceWindow
			{
				Type = ChoiceWindow.eType.eEquipment,
				Owner = Application.Current.MainWindow
			};
			if (dlg.ShowDialog() != true || dlg.ID == 0) return;

			uint maxIndex = GetMaxItemIndex();
			uint slotAddr = Util.calcItemAddress(totalSlots);
			var sd = SaveData.Instance();

			sd.WriteNumber(slotAddr, 4, dlg.ID);
			sd.WriteNumber(slotAddr + 4, 4, maxIndex + 1);
			sd.WriteNumber(slotAddr + 8, 3, 0); // Equipment has count 0
			sd.WriteNumber(slotAddr + 11, 1, 0xFF);
			sd.WriteNumber(slotAddr + 12, 4, 0xFFFFFFFF); // Not equipped

			uint status = 5;
			if (Info.KindDict.TryGetValue(dlg.ID, out var kind) && uint.TryParse(kind.Name, out uint st)) status = st;
			sd.WriteNumber(slotAddr + 16, 4, status);

			var newEq = new Item(slotAddr);
			Equipments.Add(newEq);
			SelectedEquipment = newEq;
			FilteredEquipments.Refresh();
			UpdateStatus($"Added '{newEq.Name}' to equipment inventory.");
		}

		private void DeleteEquipment(object? parameter)
		{
			var eq = parameter as Item ?? SelectedEquipment;
			if (eq == null) return;

			if (!SuppressDialogs)
			{
				var res = MessageBox.Show($"Delete '{eq.Name}' from inventory?",
					"Confirm Delete Equipment", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (res != MessageBoxResult.Yes) return;
			}

			DeleteSlotAtAddress(eq.MemoryAddress);
			ReloadItemsFromSave();
			UpdateStatus($"Deleted equipment '{eq.Name}'.");
		}

		private void UnequipEquipment(object? parameter)
		{
			var eq = parameter as Item ?? SelectedEquipment;
			if (eq == null) return;

			eq.Unequip();
			UpdateStatus($"Unequipped '{eq.Name}'. Returned to bag inventory.");
		}

		private void AddAllWeapons(object? parameter)
		{
			var weaponIds = Info.Item
				.Where(it => it.Category == "Weapons")
				.Select(it => it.Value)
				.ToList();

			AddBatch(weaponIds, isEquipment: true, defaultCount: 0, "Weapons");
		}

		private void AddAllShields(object? parameter)
		{
			var shieldIds = Info.Item
				.Where(it => it.Category == "Shields")
				.Select(it => it.Value)
				.ToList();

			AddBatch(shieldIds, isEquipment: true, defaultCount: 0, "Shields");
		}

		private void AddAllAccessories(object? parameter)
		{
			var accIds = Info.Item
				.Where(it => it.Category == "Accessories")
				.Select(it => it.Value)
				.ToList();

			AddBatch(accIds, isEquipment: true, defaultCount: 0, "Accessories");
		}

		private void UpgradeAllEquipment(object? parameter)
		{
			if (!SaveData.Instance().IsLoaded) return;

			// Gather every piece of equipment in the save file (both equipped and in bag, unique and duplicates)
			var allEquipment = Equipments.Concat(Items.Where(it => Info.IsEquipment(it.ID))).ToList();

			if (allEquipment.Count == 0)
			{
				if (!SuppressDialogs)
					MessageBox.Show("No equipment found in inventory or equipped slots.", "No Equipment", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			int count = 0;
			foreach (var eq in allEquipment)
			{
				eq.Upgraded = true;
				count++;
			}

			FilteredEquipments.Refresh();
			FilteredItems.Refresh();

			string msg = $"Upgraded {count} equipment item(s) to maximum forge tier (★).";
			UpdateStatus(msg);

			if (!SuppressDialogs)
			{
				MessageBox.Show(
					$"Successfully upgraded {count} equipment item(s)!\n\n" +
					"• Weapon attack stats are boosted to max forge tier (25 Phys/Mag ATK).\n" +
					"• Shield defense and guard rates are maximized.\n\n" +
					"IMPORTANT NOTE FOR IN-GAME:\n" +
					"Unicorn Overlord does NOT display a star icon next to items in menus. " +
					"To verify in-game, open your inventory and look at the item's Attack or Defense stat (e.g. Bronze Sword will show 25 ATK instead of 4 ATK).",
					"Equipment Upgraded", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}
		#endregion

		#region Batch Helpers
		private uint GetMaxItemIndex()
		{
			uint max = 0;
			foreach (var it in Items) if (it.Index > max) max = it.Index;
			foreach (var eq in Equipments) if (eq.Index > max) max = eq.Index;
			return max;
		}

		private void DeleteSlotAtAddress(uint memoryAddress)
		{
			var sd = SaveData.Instance();
			uint totalSlots = (uint)(Items.Count + Equipments.Count);
			if (totalSlots == 0) return;

			uint targetSlot = (memoryAddress - Util.ItemBaseAddress) / Util.ItemEntrySize;
			if (targetSlot >= totalSlots) return;

			// Shift all subsequent slots left by 1 entry
			for (uint i = targetSlot; i < totalSlots - 1; i++)
			{
				uint src = Util.calcItemAddress(i + 1);
				uint dst = Util.calcItemAddress(i);
				sd.Copy(src, dst, Util.ItemEntrySize);
			}

			// Clear the last slot
			uint last = Util.calcItemAddress(totalSlots - 1);
			sd.Fill(last, Util.ItemEntrySize, 0);
		}

		private void AddBatch(List<uint> targetIds, bool isEquipment, uint defaultCount, string categoryName)
		{
			if (!SaveData.Instance().IsLoaded) return;

			var ownedIds = new HashSet<uint>(Items.Select(i => i.ID).Concat(Equipments.Select(e => e.ID)));
			var missingIds = targetIds.Where(id => !ownedIds.Contains(id)).ToList();

			if (missingIds.Count == 0)
			{
				if (!SuppressDialogs)
					MessageBox.Show($"You already have all {categoryName} in your inventory!", "All Owned", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			uint totalSlots = (uint)(Items.Count + Equipments.Count);
			uint available = Util.MaxItemSlots > totalSlots ? Util.MaxItemSlots - totalSlots : 0;
			uint toAddCount = Math.Min((uint)missingIds.Count, available);

			if (toAddCount == 0)
			{
				if (!SuppressDialogs)
					MessageBox.Show("Inventory is full (3,800 slots).", "Cannot Add", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (!SuppressDialogs)
			{
				var res = MessageBox.Show($"Add {toAddCount} missing {categoryName} to your inventory?",
					$"Confirm Add {categoryName}", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (res != MessageBoxResult.Yes) return;
			}

			uint maxIdx = GetMaxItemIndex();
			var sd = SaveData.Instance();

			for (int i = 0; i < toAddCount; i++)
			{
				uint id = missingIds[i];
				uint slotAddr = Util.calcItemAddress(totalSlots + (uint)i);

				sd.WriteNumber(slotAddr, 4, id);
				sd.WriteNumber(slotAddr + 4, 4, ++maxIdx);
				sd.WriteNumber(slotAddr + 8, 3, defaultCount);
				sd.WriteNumber(slotAddr + 11, 1, 0xFF);
				sd.WriteNumber(slotAddr + 12, 4, 0xFFFFFFFF);

				uint status = 2;
				if (Info.KindDict.TryGetValue(id, out var kind) && uint.TryParse(kind.Name, out uint st)) status = st;
				else if (isEquipment) status = (Info.GetItemCategory(id) == "Accessories") ? 3U : 5U;
				else status = 4U;

				sd.WriteNumber(slotAddr + 16, 4, status);
			}

			ReloadItemsFromSave();
			UpdateStatus($"Successfully added {toAddCount} {categoryName} to inventory!");
		}
		#endregion

		#region Unit Operations
		private void UnlockAllUnits(object? parameter)
		{
			if (Units.Count == 0) return;

			if (!SuppressDialogs)
			{
				var res = MessageBox.Show("Unlock all 10 units and set capacity to 5 members each?",
					"Confirm Unlock Units", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (res != MessageBoxResult.Yes) return;
			}

			foreach (var u in Units)
			{
				u.UnlockAndMaximize();
			}

			UpdateStatus("Unlocked all 10 units with 5 member capacity each.");
		}
		#endregion
	}
}
