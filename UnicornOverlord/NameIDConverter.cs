using System;
using System.Globalization;
using System.Windows.Data;

namespace UnicornOverlord
{
	internal class NameIDConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return string.Empty;
			uint id;
			if (value is uint u) id = u;
			else if (uint.TryParse(value.ToString(), out uint parsed)) id = parsed;
			else return value.ToString() ?? string.Empty;

			return Info.Instance().GetCharacterName(id);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
