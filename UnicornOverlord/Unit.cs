using System;
using System.ComponentModel;

namespace UnicornOverlord
{
	internal class Unit : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private readonly uint mAddress;
		public uint Index { get; set; } = 0;

		public Unit(uint address, uint index = 0)
		{
			mAddress = address;
			Index = index;
		}

		public void RefreshAll()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
		}

		public uint UnitNumber => Index + 1;
		public string Name => $"Unit {UnitNumber}";

		public uint Count
		{
			get => SaveData.Instance().ReadNumber(mAddress, 1);
			set
			{
				uint clamped = Math.Clamp(value, 1, 6);
				SaveData.Instance().WriteNumber(mAddress, 1, clamped);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Capacity)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayText)));
			}
		}

		public uint Capacity
		{
			get => Count;
			set => Count = value;
		}

		public bool Valid
		{
			get => SaveData.Instance().ReadNumber(mAddress + 1670, 1) == 1;
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 1670, 1, value ? 1U : 0);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Valid)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayText)));
			}
		}

		public string DisplayText => $"{Name}: {(Valid ? "Unlocked" : "Locked")} — Capacity: {Count}/5 members";

		public void UnlockAndMaximize()
		{
			Valid = true;
			Count = 5;
		}

		public override string ToString() => DisplayText;
	}
}
