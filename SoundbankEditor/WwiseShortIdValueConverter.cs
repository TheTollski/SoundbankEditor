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
	[ValueConversion(typeof(uint), typeof(string))]
	public class WwiseShortIdValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			uint shortId = (uint)value;
			//return WwiseShortIdUtility.KnownShortIdsMap.ContainsKey(shortId)
			//	? WwiseShortIdUtility.KnownShortIdsMap[shortId]
			//	: "?";

			string? name = WwiseShortIdUtility.KnownShortIdsMap.ContainsKey(shortId)
				? WwiseShortIdUtility.KnownShortIdsMap[shortId]
				: "?";

			return $"{value} [{name}]";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
			//string strValue = value as string;
			//DateTime resultDateTime;
			//if (DateTime.TryParse(strValue, out resultDateTime))
			//{
			//	return resultDateTime;
			//}
			//return DependencyProperty.UnsetValue;
		}
	}
}
