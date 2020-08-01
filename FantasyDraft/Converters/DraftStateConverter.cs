using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace FantasyDraft.Converters
{
    class DraftStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int draftState = (int)value;
            int windowType = int.Parse((string)parameter);
            Visibility visibility = Visibility.Hidden;

            if (draftState == 0 && windowType == 0)
            {
                //Pre-Draft
                visibility = Visibility.Visible;
            }
            else if (draftState == 1 && windowType == 1)
            {
                //Mid-Draft
                visibility = Visibility.Visible;
            }
            else if (draftState == 2 && windowType == 2)
            {
                //Post-Draft
                visibility = Visibility.Visible;
            }

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Don't really need to convert back ever, just return null. Necessary implementation from IValueConverter interface
            return null;
        }
    }
}
