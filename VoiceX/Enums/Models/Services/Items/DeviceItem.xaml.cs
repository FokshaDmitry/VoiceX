using Linphone;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VoiceX.Items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeviceItem : ComboBoxItem
    {
        public AudioDevice AudioDevice { get; set; }
        public bool Checked { get; set; }
        public DeviceItem(AudioDevice audioDevice)
        {
            this.InitializeComponent();
            this.AudioDevice = audioDevice;
            this.DeviceName.Text = audioDevice.DeviceName.Replace("?", "").TrimStart(' ');
        }
    }
}
