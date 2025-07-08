using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VoiceX.Items
{
    public partial class LangItem : ListBoxItem
    {
        public bool IsSelect;
        public CultureInfo cultureInfo;
        public LangItem(CultureInfo cultureInfo, bool check)
        {
            this.InitializeComponent();
            this.cultureInfo = cultureInfo;
            Language.Content = cultureInfo.TwoLetterISOLanguageName.ToUpper();
            Language.IsChecked = check;
            IsSelect = check;
        }
    }
}