using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UnicornOverlord
{
	class ClassIDConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			uint id = (uint)value;
			var cls = Info.Instance().Search(Info.Instance().Class, id);
			if (cls == null) return id.ToString();
			return cls.Name;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
