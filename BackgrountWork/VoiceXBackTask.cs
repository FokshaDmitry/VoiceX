using Windows.System;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace BackgrountWork
{ 
    public sealed class VoiceXBackTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            var builder = new ToastContentBuilder()
                    .AddText("Incoming Call Start To VoiceX", hintStyle: AdaptiveTextStyle.Title, hintMaxLines: 2)
                    .AddText("Ok", hintMaxLines: 1)
                    .AddAppLogoOverride(new Uri("ms-appx:///BackgrountWork/Person.png"), ToastGenericAppLogoCrop.Circle);
            builder.Show();
            _deferral.Complete();
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            ApplicationData.Current.LocalSettings.Values["TaskActive"] = "Delete";
        }

    }
}
