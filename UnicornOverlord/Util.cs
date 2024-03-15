using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class Util
	{
		public static Byte[] Resize(Byte[] bytes, uint length)
		{
			Byte[] buffer = new Byte[length];
			Array.Copy(bytes, buffer, length);
			return buffer;
		}

		public void WriteNumber(uint address, uint size, uint value, uint min, uint max)
		{
			if (value < min) value = min;
			if (value > max) value = max;
			SaveData.Instance().WriteNumber(address, size, value);
		}
	}
}
