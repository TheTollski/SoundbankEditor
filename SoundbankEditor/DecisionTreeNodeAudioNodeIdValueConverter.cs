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
	public class DecisionTreeNodeAudioNodeIdValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			uint shortId = (uint)value;
			if (shortId == 0)
			{
				return string.Empty;
			}

			return $" - AudioNodeId: {WwiseShortIdUtility.ConvertShortIdToReadableString(shortId)}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
