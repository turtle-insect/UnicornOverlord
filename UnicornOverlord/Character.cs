using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class Character
	{
		private readonly uint mAddress;

		public Character(uint address)
		{
			mAddress = address;
		}

		public uint Exp
		{
			get => SaveData.Instance().ReadNumber(mAddress + 56, 4);
			set => SaveData.Instance().WriteNumber(mAddress + 56, 4, value);
		}
	}
}
