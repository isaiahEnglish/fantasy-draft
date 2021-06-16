using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace FantasyDraft.Converters
{
    class EnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEnabled = true;

            string fantasyDraftName = values[0].ToString();
            List<string> draftNameList = new List<string>();
            foreach (var item in (List<string>)values[1])
            {
                draftNameList.Add(item);
            }

            if (draftNameList.Contains(fantasyDraftName))
            {
                isEnabled = false;
            }

            return isEnabled;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
