using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.ExtendedExecution;

namespace VoiceX.Services
{
    class BackgroundTaskService
    {
        private ExtendedExecutionSession session;
        public BackgroundTaskService()
        {
            session = null;
        }
        //Close exetretion timeOut background task
        void ClearExtendedExecution()
        {
            if (session != null)
            {
                session.Dispose();
                session = null;
            }
        }
        public async Task<string> StartAsync()
        {
            try
            {
                var backgroundTasks = BackgroundTaskRegistration.AllTasks;
                var result = await BackgroundExecutionManager.RequestAccessAsync();
                if (result == BackgroundAccessStatus.AlwaysAllowed || result == BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
                {
                    var builder = new BackgroundTaskBuilder
                    {
                        Name = "VXBacKTask",
                        TaskEntryPoint = "VoiceXBackground.VoiceXBackTask"
                    };

                    builder.SetTrigger(new PushNotificationTrigger());
                    builder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));
                    builder.Register();
                    return "";
                }
                else
                {
                    return "Access for background tasks is denied";
                }
            }
            catch (Exception)
            {

                return "The request to execute the background task was rejected";
            }

        }
        public void StopTask()
        {
            var backgroundTasks = BackgroundTaskRegistration.AllTasks;
            var task = backgroundTasks.FirstOrDefault(t => t.Value.Name == "VXBacKTask");
            if (task.Value != null)
            {
                task.Value?.Unregister(true);
                ClearExtendedExecution();
            }
        }
    }
}