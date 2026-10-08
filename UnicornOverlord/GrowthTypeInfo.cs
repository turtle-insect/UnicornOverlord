using System;
using System.Collections.Generic;

namespace UnicornOverlord
{
	internal class GrowthTypeInfo
	{
		public uint ID { get; }
		public string Name { get; }
		public string Description { get; }
		public string DisplayText => $"{ID:D2} - {Name} ({Description})";

		public GrowthTypeInfo(uint id, string name, string description)
		{
			ID = id;
			Name = name;
			Description = description;
		}

		public override string ToString() => DisplayText;

		public static readonly List<GrowthTypeInfo> All = new()
		{
			new(0, "Default / None", "Base stats only"),
			new(1, "Hardy", "Increases HP"),
			new(2, "Offensive", "Increases Physical Attack"),
			new(3, "Defensive", "Increases Physical Defense"),
			new(4, "Precise", "Increases Accuracy"),
			new(5, "Lucky", "Increases Evasion"),
			new(6, "Keen", "Increases Critical Rate"),
			new(7, "Guardian", "Increases Guard Rate"),
			new(8, "Go-Getter", "Increases Initiative"),
			new(9, "All-Rounder", "Balanced stat distribution"),
			new(10, "All-Rounder+ (Super)", "Super Growth: +100 to ALL stats at Lv 50!"),
			new(11, "Grunt", "Standard enemy profile"),
			new(12, "Standard", "Standard profile"),
			new(13, "Powerful", "High offense boost"),
			new(14, "Boss", "Boss multiplier"),
			new(15, "Josef", "Josef's veteran profile"),
			new(16, "Mercenary", "Hired mercenary profile")
		};

		public static GrowthTypeInfo Get(uint id)
		{
			var found = All.Find(g => g.ID == id);
			return found ?? new GrowthTypeInfo(id, $"Custom ({id})", "Unknown growth type");
		}
	}
}
