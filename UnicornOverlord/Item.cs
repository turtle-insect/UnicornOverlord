using System;
using System.ComponentModel;

namespace UnicornOverlord
{
	internal class Item : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private readonly uint mAddress;
		public uint MemoryAddress => mAddress;

		public Item(uint address)
		{
			mAddress = address;
		}

		public void RefreshAll()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
		}

		public uint ID
		{
			get => SaveData.Instance().ReadNumber(mAddress, 4);
			set
			{
				SaveData.Instance().WriteNumber(mAddress, 4, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ID)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CategoryName)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTitle)));
			}
		}

		public string Name => Info.Instance().GetItemName(ID);
		public string CategoryName => Info.Instance().GetItemCategory(ID);

		public uint Index
		{
			get => SaveData.Instance().ReadNumber(mAddress + 4, 4);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 4, 4, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Index)));
			}
		}

		public uint Count
		{
			get => SaveData.Instance().ReadNumber(mAddress + 8, 3);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 8, 3, Math.Min(value, 999999));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTitle)));
			}
		}

		public uint EquipSlot
		{
			get => SaveData.Instance().ReadNumber(mAddress + 11, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 11, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EquipSlot)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EquipSlotName)));
			}
		}

		public string EquipSlotName
		{
			get
			{
				return EquipSlot switch
				{
					0 => "Weapon",
					1 => "Shield / Acc 1",
					2 => "Accessory 2",
					3 => "Accessory 3",
					0xFF => "Bag / Item",
					_ => $"Slot {EquipSlot}"
				};
			}
		}

		public uint EquippedCharacterId
		{
			get => SaveData.Instance().ReadNumber(mAddress + 12, 4);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 12, 4, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EquippedCharacterId)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EquippedCharacterName)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEquipped)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTitle)));
			}
		}

		// Backward compatibility for original bindings
		public uint Equipment1 => EquipSlot;
		public uint Equipment2 => EquippedCharacterId & 0xFF;

		public bool IsEquipped => EquippedCharacterId != 0xFFFFFFFF && EquippedCharacterId != 0;

		public string EquippedCharacterName
		{
			get
			{
				if (!IsEquipped) return "In Inventory";
				return $"Character #{EquippedCharacterId}";
			}
		}

		public uint Status
		{
			get => SaveData.Instance().ReadNumber(mAddress + 16, 4);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 16, 4, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Upgraded)));
			}
		}

		public bool Upgraded
		{
			get
			{
				byte statusByte = (byte)SaveData.Instance().ReadNumber(mAddress + 16, 1);
				return (statusByte >> 4) != 0;
			}
			set
			{
				byte statusByte = (byte)SaveData.Instance().ReadNumber(mAddress + 16, 1);
				byte type = (byte)(statusByte & 0x0F);
				if (type == 0)
				{
					if (Info.Instance().KindDict.TryGetValue(ID, out var kind) && byte.TryParse(kind.Name, out byte t))
					{
						type = t;
					}
					else
					{
						type = 5;
					}
				}

				byte newStatusByte = value ? (byte)(type | 0x10) : type;
				SaveData.Instance().WriteNumber(mAddress + 16, 1, newStatusByte);
				// Reset byte 17 back to 0 to repair any previously bugged writes
				SaveData.Instance().WriteNumber(mAddress + 17, 1, 0);

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Upgraded)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTitle)));
			}
		}

		public void Unequip()
		{
			EquippedCharacterId = 0xFFFFFFFF;
			EquipSlot = 0xFF;
		}

		public string DisplayTitle
		{
			get
			{
				if (Count > 0)
				{
					return $"[{ID:D3}] {Name} (x{Count})";
				}
				string upg = Upgraded ? " ★" : "";
				string eq = IsEquipped ? $" [Equipped: {EquippedCharacterName}]" : " [Inventory]";
				return $"[{ID:D3}] {Name}{upg} — {CategoryName}{eq}";
			}
		}

		public override string ToString() => DisplayTitle;
	}
}
