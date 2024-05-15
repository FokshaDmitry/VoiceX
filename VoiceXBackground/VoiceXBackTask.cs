
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using Windows.Storage;

namespace VoiceXBackground
{ 
    public sealed class VoiceXBackTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        //List<string> AutoCall;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            ApplicationData.Current.LocalSettings.Values["MessageRecive"] = "Recive";
            _deferral.Complete();
        }
    }
}
