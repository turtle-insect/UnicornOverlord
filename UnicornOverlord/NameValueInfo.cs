using System;
using System.Collections.Generic;
using System.Globalization;

namespace UnicornOverlord
{
	internal class NameValueInfo : IComparable
	{
		public uint Value { get; private set; }
		private readonly List<string> mNames = new List<string>();
		public string Category { get; set; } = string.Empty;

		public string Name
		{
			get
			{
				if (mNames.Count == 0) return Value.ToString();

				var index = Properties.Settings.Default.Language;
				if (index >= mNames.Count) index = 0;

				var value = mNames[index];
				if (string.IsNullOrEmpty(value))
				{
					value = mNames[0];
				}
				return string.IsNullOrEmpty(value) ? Value.ToString() : value;
			}
		}

		public string DisplayNameWithId => $"[{Value:D3}] {Name}";

		public int CompareTo(object? obj)
		{
			if (obj is not NameValueInfo dist) return 0;
			return Value.CompareTo(dist.Value);
		}

		public virtual bool Line(string[] oneLine)
		{
			if (oneLine.Length == 0) return false;

			string idStr = oneLine[0].Trim();
			if (string.IsNullOrEmpty(idStr)) return false;

			try
			{
				if (idStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
					Value = Convert.ToUInt32(idStr, 16);
				else
					Value = Convert.ToUInt32(idStr, 10);
			}
			catch
			{
				return false;
			}

			mNames.Clear();
			for (int index = 1; index < oneLine.Length; index++)
			{
				mNames.Add(oneLine[index].Trim());
			}
			return true;
		}

		public override string ToString() => Name;
	}
}
