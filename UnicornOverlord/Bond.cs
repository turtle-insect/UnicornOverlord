using System;
using System.ComponentModel;

namespace UnicornOverlord
{
	internal class Bond : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private readonly uint mAddress;
		private string? mPartnerName = null;

		public Bond(uint address)
		{
			mAddress = address;
		}

		public uint ID => SaveData.Instance().ReadNumber(mAddress, 4);

		public string PartnerName
		{
			get
			{
				if (mPartnerName != null) return mPartnerName;
				return $"Character #{ID}";
			}
			set
			{
				mPartnerName = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PartnerName)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayText)));
			}
		}

		public uint Value
		{
			get => SaveData.Instance().ReadNumber(mAddress + 4, 2);
			set
			{
				uint clamped = Math.Min(value, 1000);
				SaveData.Instance().WriteNumber(mAddress + 4, 2, clamped);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayText)));
			}
		}

		public uint HeartLevel
		{
			get => SaveData.Instance().ReadNumber(mAddress + 6, 1);
			set
			{
				uint clamped = Math.Min(value, 3);
				SaveData.Instance().WriteNumber(mAddress + 6, 1, clamped);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HeartLevel)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HeartsFormatted)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayText)));
			}
		}

		public string HeartsFormatted
		{
			get
			{
				return HeartLevel switch
				{
					0 => "0 Hearts",
					1 => "1 Heart (♥)",
					2 => "2 Hearts (♥♥)",
					3 => "3 Hearts (♥♥♥)",
					_ => $"{HeartLevel} Hearts"
				};
			}
		}

		public string DisplayText => $"{PartnerName} — {Value}/1000 ({HeartsFormatted})";

		public void Maximize()
		{
			Value = 1000;
			HeartLevel = 3;
		}

		public override string ToString() => DisplayText;
	}
}
