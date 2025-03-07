

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

using pj;
using System.Text;
using System.Windows.Controls;

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeviceItem : ComboBoxItem
    {
        public bool Checked { get; set; }
        public int caps { get; set; }
        public DeviceItem(string name, int caps)
        {
            this.InitializeComponent();
            this.caps = caps;
            this.DeviceName.Text = Encoding.UTF8.GetString(Encoding.Default.GetBytes(name)).Replace("?", "");
        }
    }
}
