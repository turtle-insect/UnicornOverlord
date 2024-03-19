using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class Basic
	{
		public uint Money
		{
			get => SaveData.Instance().ReadNumber(0x20, 4);
			set => SaveData.Instance().WriteNumber(0x20, 4, value);
		}

		public uint Fame
		{
			get => SaveData.Instance().ReadNumber(0x24, 4);
			set => SaveData.Instance().WriteNumber(0x24, 4, value);
		}

		public bool ZENOIRA
		{
			get => SaveData.Instance().ReadNumber(0x4DA39E, 2) == 0x4040;
			set => SaveData.Instance().WriteNumber(0x4DA39E, 2, value ? 0x4040U : 0);
		}
	}
}
