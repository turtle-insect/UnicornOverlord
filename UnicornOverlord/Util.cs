using System;

namespace UnicornOverlord
{
	internal static class Util
	{
		public const uint ItemBaseAddress = 0xA0;
		public const uint ItemEntrySize = 20;
		public const uint MaxItemSlots = 3800;

		public const uint CharacterBaseAddress = 0x2AF40;
		public const uint CharacterEntrySize = 464;
		public const uint MaxCharacters = 500;

		public const uint BondBaseAddress = 0x1B5830;
		public const uint BondEntrySize = 1316;
		public const uint MaxBonds = 164;

		public const uint UnitBaseAddress = 0x10D89A;
		public const uint UnitEntrySize = 1720;
		public const uint MaxUnits = 10;

		public static byte[] Resize(byte[] bytes, uint length)
		{
			byte[] buffer = new byte[length];
			Array.Copy(bytes, buffer, Math.Min(bytes.Length, length));
			return buffer;
		}

		public static void WriteNumber(uint address, uint size, uint value, uint min, uint max)
		{
			if (value < min) value = min;
			if (value > max) value = max;
			SaveData.Instance().WriteNumber(address, size, value);
		}

		public static uint calcCharacterAddress(uint index)
		{
			return CharacterBaseAddress + index * CharacterEntrySize;
		}

		public static uint calcBondAddress(uint index)
		{
			return BondBaseAddress + index * BondEntrySize;
		}

		public static uint calcItemAddress(uint index)
		{
			return ItemBaseAddress + index * ItemEntrySize;
		}

		public static uint calcUnitAddress(uint index)
		{
			return UnitBaseAddress + index * UnitEntrySize;
		}
	}
}
