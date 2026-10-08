using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnicornOverlord
{
	internal class Info
	{
		private static readonly Info mThis = new Info();
		public List<NameValueInfo> Item { get; private set; } = new List<NameValueInfo>();
		public List<NameValueInfo> Kind { get; private set; } = new List<NameValueInfo>();
		public List<NameValueInfo> Class { get; private set; } = new List<NameValueInfo>();
		public List<NameValueInfo> Name { get; private set; } = new List<NameValueInfo>();

		public Dictionary<uint, NameValueInfo> ItemDict { get; private set; } = new Dictionary<uint, NameValueInfo>();
		public Dictionary<uint, NameValueInfo> KindDict { get; private set; } = new Dictionary<uint, NameValueInfo>();
		public Dictionary<uint, NameValueInfo> ClassDict { get; private set; } = new Dictionary<uint, NameValueInfo>();
		public Dictionary<uint, NameValueInfo> NameDict { get; private set; } = new Dictionary<uint, NameValueInfo>();

		private Info() { }

		static Info()
		{
			mThis.Initialize();
		}

		public static Info Instance()
		{
			return mThis;
		}

		public void Initialize()
		{
			Item.Clear();
			Kind.Clear();
			Class.Clear();
			Name.Clear();
			ItemDict.Clear();
			KindDict.Clear();
			ClassDict.Clear();
			NameDict.Clear();

			AppendList("item.txt", Item, ItemDict);
			AppendList("kind.txt", Kind, KindDict);
			AppendList("class.txt", Class, ClassDict);
			AppendList("name.txt", Name, NameDict);

			// Populate categories for items
			foreach (var item in Item)
			{
				item.Category = DetermineCategory(item.Value);
			}
		}

		private string DetermineCategory(uint id)
		{
			if (KindDict.TryGetValue(id, out var kind))
			{
				if (kind.Name == "3") return "Accessories";
				if (kind.Name == "5")
				{
					if (id >= 642 && id <= 771) return "Shields";
					return "Weapons";
				}
			}

			if (id >= 282 && id <= 630) return "Weapons";
			if (id >= 642 && id <= 771) return "Shields";
			if (id >= 784 && id <= 970) return "Accessories";

			if (id == 3 || id == 4 || id == 5) return "Currency / Tokens";
			if (id >= 6 && id <= 40) return "Consumables";
			if (id >= 41 && id <= 53) return "Recovery / Herbs";
			if (id >= 54 && id <= 281) return "Valuables / Materials";

			return "Items";
		}

		public string GetItemName(uint id)
		{
			if (ItemDict.TryGetValue(id, out var info)) return info.Name;
			return $"Item #{id}";
		}

		public string GetClassName(uint id)
		{
			if (ClassDict.TryGetValue(id, out var info)) return info.Name;
			return $"Class #{id}";
		}

		public string GetCharacterName(uint id)
		{
			if (NameDict.TryGetValue(id, out var info)) return info.Name;
			return $"Character #{id}";
		}

		public string GetItemCategory(uint id)
		{
			if (ItemDict.TryGetValue(id, out var info) && !string.IsNullOrEmpty(info.Category))
				return info.Category;
			return DetermineCategory(id);
		}

		public bool IsEquipment(uint id)
		{
			if (KindDict.TryGetValue(id, out var kind))
			{
				return kind.Name == "3" || kind.Name == "5";
			}
			return (id >= 282 && id <= 970);
		}

		public NameValueInfo? Search<Type>(List<Type> list, uint id)
			where Type : NameValueInfo, new()
		{
			if (ReferenceEquals(list, Item) && ItemDict.TryGetValue(id, out var it)) return it as Type;
			if (ReferenceEquals(list, Class) && ClassDict.TryGetValue(id, out var cl)) return cl as Type;
			if (ReferenceEquals(list, Name) && NameDict.TryGetValue(id, out var nm)) return nm as Type;
			if (ReferenceEquals(list, Kind) && KindDict.TryGetValue(id, out var kd)) return kd as Type;

			int min = 0;
			int max = list.Count;
			for (; min < max;)
			{
				int mid = (min + max) / 2;
				if (list[mid].Value == id) return list[mid];
				else if (list[mid].Value > id) max = mid;
				else min = mid + 1;
			}
			return null;
		}

		private string? ResolvePath(string relativePath)
		{
			string[] candidates = new[]
			{
				Path.Combine(AppContext.BaseDirectory, "info", relativePath),
				Path.Combine(AppContext.BaseDirectory, relativePath),
				Path.Combine(Directory.GetCurrentDirectory(), "info", relativePath),
				Path.Combine(Directory.GetCurrentDirectory(), relativePath),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "info", relativePath),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath)
			};

			foreach (var path in candidates)
			{
				if (File.Exists(path)) return path;
			}
			return null;
		}

		private void AppendList<Type>(string filename, List<Type> items, Dictionary<uint, Type> dict)
			where Type : NameValueInfo, new()
		{
			string? resolved = ResolvePath(filename);
			if (resolved == null || !File.Exists(resolved)) return;

			string[] lines;
			try
			{
				lines = File.ReadAllLines(resolved);
			}
			catch
			{
				return;
			}

			foreach (string rawLine in lines)
			{
				string line = rawLine.Trim();
				if (line.Length < 2) continue;
				if (line[0] == '#') continue;

				string[] values = line.Split('\t');
				if (values.Length < 2) continue;
				if (string.IsNullOrWhiteSpace(values[0])) continue;

				Type type = new Type();
				if (type.Line(values))
				{
					items.Add(type);
					dict[type.Value] = type;
				}
			}

			items.Sort();
		}
	}
}
