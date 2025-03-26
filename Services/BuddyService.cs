using pj;
using System.Diagnostics;
using VoiceX.Enums;

namespace VoiceX.Services
{
    public class BuddyService : Buddy
    {
        public delegate void ChangeStatus(string uri, HotKeyStatus hotKeyStatus);
        public event ChangeStatus? onlineStatusChange;
        public BuddyService() : base() 
        {

        }
        public override void onBuddyState()
        {
            BuddyInfo info = getInfo();
            Debug.WriteLine($"[SIP] Статус {info.uri} изменился: {info.presStatus.statusText}");
            
            if (info.presStatus.statusText.ToLower().Contains("ready"))
            {
                onlineStatusChange?.Invoke(info.uri, HotKeyStatus.Online);
                Debug.WriteLine($"[SIP] {info.uri} теперь Online ✅");
            }
            else if (info.presStatus.statusText.ToLower().Contains("unavailable"))
            {
                onlineStatusChange?.Invoke(info.uri, HotKeyStatus.Offline);
                Debug.WriteLine($"[SIP] {info.uri} теперь Offline ❌");
            }
            else
            {
                if (info.presStatus.statusText.ToLower().Contains("on the phone"))
                {
                    onlineStatusChange?.Invoke(info.uri, HotKeyStatus.Busy);
                    Debug.WriteLine($"[SIP] {info.uri} теперь BUSY ❌");
                }
            }
        }
    }
}
