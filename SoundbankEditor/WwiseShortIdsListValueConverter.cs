using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using SoundbankEditor.Core;

namespace SoundbankEditor
{
	[ValueConversion(typeof(List<uint>), typeof(string))]
	public class WwiseShortIdsListValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			List<uint> shortIds = (List<uint>)value;
			return string.Join(',', shortIds.Select(id => WwiseShortIdUtility.ConvertShortIdToReadableString(id)));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
