

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
        public AudioDevInfo AudioDevInfo { get; set; }
        public DeviceItem(AudioDevInfo audioDevInfo)
        {
            this.InitializeComponent();
            AudioDevInfo = audioDevInfo;
            this.DeviceName.Text = Encoding.UTF8.GetString(Encoding.Default.GetBytes(audioDevInfo.name)).Replace("?", "");
        }
    }
}
