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
	}
}
