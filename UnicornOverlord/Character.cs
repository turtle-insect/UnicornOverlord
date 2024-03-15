using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class Character : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private readonly uint mAddress;

		public Character(uint address)
		{
			mAddress = address;
		}

		public uint ID
		{
			get => SaveData.Instance().ReadNumber(mAddress, 4);
		}

		public uint Class
		{
			get => SaveData.Instance().ReadNumber(mAddress + 40, 1);
			set
			{
				SaveData.Instance().WriteNumber(mAddress + 40, 1, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Class)));
			}
		}

		public uint Exp
		{
			get => SaveData.Instance().ReadNumber(mAddress + 56, 4);
			set => SaveData.Instance().WriteNumber(mAddress + 56, 4, value);
		}

		public uint HPPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 64, 1);
			set => SaveData.Instance().WriteNumber(mAddress + 64, 1, value);
		}

		public uint AttackPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 65, 1);
			set => SaveData.Instance().WriteNumber(mAddress + 65, 1, value);
		}

		public uint DefensePlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 66, 1);
			set => SaveData.Instance().WriteNumber(mAddress + 66, 1, value);
		}

		public uint MagicAttackPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 67, 1);
			set => SaveData.Instance().WriteNumber(mAddress + 67, 1, value);
		}

		public uint MagicDefensePlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 68, 1);
			set => SaveData.Instance().WriteNumber(mAddress + 68, 1, value);
		}

		public uint HitRatePlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 69, 1);
			set => SaveData.Instance().WriteNumber(mAddress + 69, 1, value);
		}

		public uint AVoidPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 70, 1);
			set => SaveData.Instance().WriteNumber(mAddress + 70, 1, value);
		}

		public uint CriticalPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 71, 1);
			set => SaveData.Instance().WriteNumber(mAddress + 71, 1, value);
		}

		public uint GuardPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 72, 1);
			set => SaveData.Instance().WriteNumber(mAddress + 72, 1, value);
		}

		public uint SpeedPlus
		{
			get => SaveData.Instance().ReadNumber(mAddress + 73, 1);
			set => SaveData.Instance().WriteNumber(mAddress + 73, 1, value);
		}

		public bool Use
		{
			get => !SaveData.Instance().ReadBit(mAddress + 460, 5);
			set => SaveData.Instance().WriteBit(mAddress + 460, 5, !value);
		}
	}
}
