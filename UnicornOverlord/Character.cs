using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace UnicornOverlord
{
	internal class Character : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public ObservableCollection<Bond>? Bonds { get; set; }

		private readonly uint mAddress;
		public uint SlotIndex { get; set; } = 0;

		public Character(uint address, uint slotIndex = 0)
		{
			mAddress = address;
			SlotIndex = slotIndex;
		}

		public void RefreshAll()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
		}

		public uint ID => SaveData.Instance().ReadNumber(mAddress, 4);

		public uint Class
		{
			get => SaveData.Instance().ReadNumber(mAddress + 40, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 40, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Class)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClassName)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTitle)));
			}
		}

		public string ClassName => Info.Instance().GetClassName(Class);

		public bool IsMercenary => SaveData.Instance().ReadNumber(mAddress + 38, 2) == 0;
		public uint HiredNameId => SaveData.Instance().ReadNumber(mAddress + 36, 2);
		public uint UniqueNameId => SaveData.Instance().ReadNumber(mAddress + 52, 2);

		public uint Name
		{
			get
			{
				uint uid = UniqueNameId;
				if (uid != 0) return uid;
				return HiredNameId;
			}
		}

		public string DisplayName
		{
			get
			{
				uint nameId = Name;
				string resolved = Info.Instance().GetCharacterName(nameId);
				if (IsMercenary)
				{
					return string.IsNullOrEmpty(resolved) ? $"[Merc] Unit #{SlotIndex + 1}" : $"[Merc] {resolved}";
				}
				return resolved;
			}
		}

		public string DisplayTitle => $"[#{SlotIndex + 1:D2}] {DisplayName} — {ClassName} (Lv. {Lv})";

		public uint Exp
		{
			get => SaveData.Instance().ReadNumber(mAddress + 56, 4);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 56, 4, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Exp)));
			}
		}

		public uint Lv
		{
			get => SaveData.Instance().ReadNumber(mAddress + 60, 2);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 60, 2, Math.Min(value, 50));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Lv)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTitle)));
			}
		}

		public uint GrowthType1
		{
			get => SaveData.Instance().ReadNumber(mAddress + 41, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 41, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GrowthType1)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GrowthType1Info)));
			}
		}

		public GrowthTypeInfo GrowthType1Info => GrowthTypeInfo.Get(GrowthType1);

		public uint GrowthType2
		{
			get => SaveData.Instance().ReadNumber(mAddress + 42, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 42, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GrowthType2)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GrowthType2Info)));
			}
		}

		public GrowthTypeInfo GrowthType2Info => GrowthTypeInfo.Get(GrowthType2);

		// Appearance
		public uint BaseColor
		{
			get => SaveData.Instance().ReadNumber(mAddress + 44, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 44, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BaseColor)));
			}
		}

		public uint HairColor
		{
			get => SaveData.Instance().ReadNumber(mAddress + 45, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 45, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HairColor)));
			}
		}

		public uint AccentColor1
		{
			get => SaveData.Instance().ReadNumber(mAddress + 46, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 46, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccentColor1)));
			}
		}

		public uint AccentColor2
		{
			get => SaveData.Instance().ReadNumber(mAddress + 47, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 47, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccentColor2)));
			}
		}

		public uint PromotionSprite
		{
			get => SaveData.Instance().ReadNumber(mAddress + 48, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 48, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PromotionSprite)));
			}
		}

		// Unit Assignment
		public uint Unit
		{
			get => SaveData.Instance().ReadNumber(mAddress + 32, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 32, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Unit)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnitName)));
			}
		}

		public string UnitName
		{
			get
			{
				uint u = Unit;
				if (u >= 10 || u == 0xFF) return "Unassigned";
				return $"Unit {u + 1}";
			}
		}

		// Dew Stat Bonuses (0x40 - 0x4A)
		public uint HPPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 64, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 64, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HPPlus)));
			}
		}

		public uint AttackPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 65, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 65, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AttackPlus)));
			}
		}

		public uint DefensePlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 66, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 66, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DefensePlus)));
			}
		}

		public uint MagicAttackPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 67, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 67, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MagicAttackPlus)));
			}
		}

		public uint MagicDefensePlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 68, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 68, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MagicDefensePlus)));
			}
		}

		public uint HitRatePlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 69, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 69, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HitRatePlus)));
			}
		}

		public uint AVoidPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 70, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 70, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AVoidPlus)));
			}
		}

		public uint CriticalPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 71, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 71, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CriticalPlus)));
			}
		}

		public uint GuardPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 72, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 72, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GuardPlus)));
			}
		}

		public uint SpeedPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 73, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 73, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpeedPlus)));
			}
		}

		public uint IllusionPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 74, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 74, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IllusionPlus)));
			}
		}

		// Status Flags (0x1CC = 460)
		public bool Use
		{
			get => !SaveData.Instance().ReadBit(mAddress + 460, 5);
			set
			{
				SaveData.Instance().WriteBit(mAddress + 460, 5, !value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Use)));
			}
		}

		public bool IsInFormation
		{
			get => SaveData.Instance().ReadBit(mAddress + 460, 1);
			set
			{
				SaveData.Instance().WriteBit(mAddress + 460, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInFormation)));
			}
		}

		public bool IsJoined
		{
			get => SaveData.Instance().ReadBit(mAddress + 460, 3);
			set
			{
				SaveData.Instance().WriteBit(mAddress + 460, 3, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsJoined)));
			}
		}

		// Helper batch methods
		public void SetMaxLevel()
		{
			Lv = 50;
			Exp = 1900000;
		}

		public void SetMaxDews(uint amount = 5)
		{
			HPPlus = amount;
			AttackPlus = amount;
			DefensePlus = amount;
			MagicAttackPlus = amount;
			MagicDefensePlus = amount;
			HitRatePlus = amount;
			AVoidPlus = amount;
			CriticalPlus = amount;
			GuardPlus = amount;
			SpeedPlus = amount;
		}

		public void ResetDews()
		{
			SetMaxDews(0);
		}

		public void MaximizeAllBonds()
		{
			if (Bonds == null) return;
			foreach (var bond in Bonds)
			{
				bond.Maximize();
			}
		}

		public override string ToString() => DisplayTitle;
	}
}
