using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class Unit
	{
		private readonly uint mAddress;

		public Unit(uint address)
		{
			mAddress = address;
		}

		public uint Count
		{
			get => SaveData.Instance().ReadNumber(mAddress, 1);
			set => Util.WriteNumber(mAddress, 1, value, 1, 6);
		}

		public bool Valid
		{
			get => SaveData.Instance().ReadNumber(mAddress + 1670, 1) == 1;
			set => SaveData.Instance().WriteNumber(mAddress + 1670, 1, value == true ? 1U : 0);
		}
	}
}
