using System;
using System.ComponentModel;

namespace UnicornOverlord
{
	internal class Basic : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public void RefreshAll()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
		}

		public uint Money
		{
			get => SaveData.Instance().ReadNumber(0x20, 4);
			set
			{
				SaveData.Instance().WriteNumber(0x20, 4, Math.Min(value, 9999999));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Money)));
			}
		}

		public uint Fame
		{
			get => SaveData.Instance().ReadNumber(0x24, 4);
			set
			{
				SaveData.Instance().WriteNumber(0x24, 4, Math.Min(value, 5000));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Fame)));
			}
		}

		public uint RenownRank
		{
			get => SaveData.Instance().ReadNumber(0x28, 4);
			set
			{
				SaveData.Instance().WriteNumber(0x28, 4, Math.Min(value, 5));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RenownRank)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RenownRankName)));
			}
		}

		public string RenownRankName
		{
			get
			{
				return RenownRank switch
				{
					0 => "E",
					1 => "D",
					2 => "C",
					3 => "B",
					4 => "A",
					5 => "S",
					_ => $"Rank {RenownRank}"
				};
			}
		}

		public uint PlayTimeSeconds
		{
			get => SaveData.Instance().ReadNumber(0x1C, 4);
			set
			{
				SaveData.Instance().WriteNumber(0x1C, 4, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayTimeSeconds)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayTimeFormatted)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayTimeHours)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayTimeMinutes)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayTimeSecondsPart)));
			}
		}

		public string PlayTimeFormatted
		{
			get
			{
				uint total = PlayTimeSeconds;
				uint hours = total / 3600;
				uint minutes = (total % 3600) / 60;
				uint seconds = total % 60;
				return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
			}
		}

		public uint PlayTimeHours
		{
			get => PlayTimeSeconds / 3600;
			set => PlayTimeSeconds = (value * 3600) + (PlayTimeMinutes * 60) + PlayTimeSecondsPart;
		}

		public uint PlayTimeMinutes
		{
			get => (PlayTimeSeconds % 3600) / 60;
			set => PlayTimeSeconds = (PlayTimeHours * 3600) + (Math.Min(value, 59) * 60) + PlayTimeSecondsPart;
		}

		public uint PlayTimeSecondsPart
		{
			get => PlayTimeSeconds % 60;
			set => PlayTimeSeconds = (PlayTimeHours * 3600) + (PlayTimeMinutes * 60) + Math.Min(value, 59);
		}

		public uint SaveSlot
		{
			get => SaveData.Instance().ReadNumber(0x10, 4);
			set
			{
				SaveData.Instance().WriteNumber(0x10, 4, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SaveSlot)));
			}
		}

		public bool ZENOIRA
		{
			get => SaveData.Instance().ReadNumber(0x4DA39E, 2) == 0x4040;
			set
			{
				SaveData.Instance().WriteNumber(0x4DA39E, 2, value ? 0x4040U : 0);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZENOIRA)));
			}
		}

		// Core Currencies stored as special items
		public uint Medals
		{
			get => GetItemCount(3);
			set
			{
				SetItemCount(3, value, 2);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Medals)));
			}
		}

		public uint ColiseumCoins
		{
			get => GetItemCount(4);
			set
			{
				SetItemCount(4, value, 2);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColiseumCoins)));
			}
		}

		public uint DivineShards
		{
			get => GetItemCount(5);
			set
			{
				SetItemCount(5, value, 2);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DivineShards)));
			}
		}

		public uint HallowedAsh
		{
			get => GetItemCount(6);
			set
			{
				SetItemCount(6, value, 2);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HallowedAsh)));
			}
		}

		public uint LuminousAsh
		{
			get => GetItemCount(7);
			set
			{
				SetItemCount(7, value, 2);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LuminousAsh)));
			}
		}

		private uint GetItemCount(uint itemId)
		{
			var sd = SaveData.Instance();
			if (!sd.IsLoaded) return 0;

			for (uint i = 0; i < 3800; i++)
			{
				uint addr = 0xA0 + i * 20;
				uint idx = sd.ReadNumber(addr + 4, 4);
				if (idx == 0) break;

				uint id = sd.ReadNumber(addr, 4);
				if (id == itemId)
				{
					return sd.ReadNumber(addr + 8, 3);
				}
			}
			return 0;
		}

		private void SetItemCount(uint itemId, uint count, uint status)
		{
			var sd = SaveData.Instance();
			if (!sd.IsLoaded) return;

			uint maxIdx = 0;
			uint emptySlot = uint.MaxValue;

			for (uint i = 0; i < 3800; i++)
			{
				uint addr = 0xA0 + i * 20;
				uint idx = sd.ReadNumber(addr + 4, 4);
				if (idx == 0)
				{
					emptySlot = i;
					break;
				}

				if (idx > maxIdx) maxIdx = idx;

				uint id = sd.ReadNumber(addr, 4);
				if (id == itemId)
				{
					sd.WriteNumber(addr + 8, 3, count);
					return;
				}
			}

			// Not found and count > 0: append item
			if (count > 0 && emptySlot < 3800)
			{
				uint addr = 0xA0 + emptySlot * 20;
				sd.WriteNumber(addr, 4, itemId);
				sd.WriteNumber(addr + 4, 4, maxIdx + 1);
				sd.WriteNumber(addr + 8, 3, count);
				sd.WriteNumber(addr + 11, 1, 0xFF);
				sd.WriteNumber(addr + 12, 4, 0xFFFFFFFF);
				sd.WriteNumber(addr + 16, 4, status);
			}
		}
	}
}
