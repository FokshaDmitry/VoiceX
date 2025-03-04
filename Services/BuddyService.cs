using pj;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceX.Services
{
    public class BuddyService : Buddy
    {
        public delegate void ChangeStatus(string uri, bool isOnline);
        public event ChangeStatus? onlineStatusChange;
        public BuddyService() : base() 
        {

        }
        public override void onBuddyState()
        {
            BuddyInfo info = getInfo();
            Debug.WriteLine($"[SIP] Статус {info.uri} изменился: {info.presStatus.statusText}");

            if (info.presStatus.status == pjsua_buddy_status.PJSUA_BUDDY_STATUS_ONLINE)
            {
                onlineStatusChange?.Invoke(info.uri, true);
                Debug.WriteLine($"[SIP] {info.uri} теперь Online ✅");
            }
            else if (info.presStatus.status == pjsua_buddy_status.PJSUA_BUDDY_STATUS_OFFLINE)
            {
                onlineStatusChange?.Invoke(info.uri, false);
                Debug.WriteLine($"[SIP] {info.uri} теперь Offline ❌");
            }
            else
            {
                Debug.WriteLine($"[SIP] {info.uri} статус: {info.presStatus.statusText}");
            }
        }
    }
}
