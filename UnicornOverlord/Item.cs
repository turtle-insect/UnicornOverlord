using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class Item : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private readonly uint mAddress;

		public Item(uint address)
		{
			mAddress = address;
		}

		public uint ID
		{
			get => SaveData.Instance().ReadNumber(mAddress, 4);
			set
			{
				SaveData.Instance().WriteNumber(mAddress, 4, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ID)));
			}
		}

		public uint Index
		{
			get => SaveData.Instance().ReadNumber(mAddress + 4, 4);
			set => SaveData.Instance().WriteNumber(mAddress + 4, 4, value);
		}

		public uint Count
		{
			get => SaveData.Instance().ReadNumber(mAddress + 8, 3);
			set => SaveData.Instance().WriteNumber(mAddress + 8, 3, value);
		}

		public uint Equipment1
		{
			get => SaveData.Instance().ReadNumber(mAddress + 11, 1);
		}

		public uint Equipment2
		{
			get => SaveData.Instance().ReadNumber(mAddress + 12, 1);
		}

		public uint Status
		{
			get => SaveData.Instance().ReadNumber(mAddress + 16, 4);
			set => SaveData.Instance().WriteNumber(mAddress + 16, 4, value);
		}
	}
}
