using Linphone;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Effects;
using Windows.UI.Core;
using static Linphone.CoreListener;
using static Linphone.LoggingServiceListener;

namespace VoiceX.Services
{
    public class CoreService
    {
        private Timer Timer;
        private static readonly CoreService instance = new CoreService();
        public static bool NatIgnore;
        public static CoreService Instance
        {
            get
            {
                return instance;
            }
        }

        private Core core;

        public Core Core
        {
            get
            {
                if (core == null)
                {
                    Factory factory = Factory.Instance;
                    string assetsPath = Path.Combine(Package.Current.InstalledLocation.Path, "share");
                    factory.TopResourcesDir = assetsPath;
                    factory.DataResourcesDir = assetsPath;
                    factory.SoundResourcesDir = Path.Combine(assetsPath, "sounds", "linphone");
                    factory.RingResourcesDir = Path.Combine(factory.SoundResourcesDir, "rings");
                    factory.ImageResourcesDir = Path.Combine(assetsPath, "images");

                    factory.MspluginsDir = ".";

                    core = factory.CreateCore("", "", IntPtr.Zero);
                    core.CallkitEnabled = false;
                    core.MaxCalls = 15;
                    core.SetAudioPortRange(10000, 20000);
                    var transports = core.Transports;
                    transports.UdpPort = -1;
                    transports.TcpPort = -1;
                    transports.TlsPort = -1;
                    core.Transports = transports;
                    core.DnsSrvEnabled = !NatIgnore;
                    core.SipTransportTimeout = 25000;
                    core.EchoCancellationEnabled = true;
                    core.EchoLimiterEnabled = true;
                    
                    core.DelayedTimeout = 30;
                    core.Config.SetInt("net", "enable_nat_helper", 0);
                    if (!NatIgnore)
                    {
                        core.NatPolicy.IceEnabled = true;
                        core.NatPolicy.StunEnabled = true;
                        core.NatPolicy.StunServer = "ice.x-cloud.info:3478";
                    }
                    core.PushNotificationEnabled = false;
                    core.KeepAliveEnabled = true;
                    core.Ipv6Enabled = false;
                }
                return core;
            }
        }
        public bool CheckCoreOnNull()
        {
            if (core == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void CoreStart(CoreDispatcher dispatcher)
        {
            try
            {
                Core.Start();
                Timer = new Timer(OnTimedEvent, dispatcher, 20, 20);
            }
            catch
            {
                return;
            }

        }

        private async void OnTimedEvent(object state)
        {
            await ((CoreDispatcher)state).RunIdleAsync((args) =>
            {
                try
                {
                    core.Iterate();
                }
                catch
                {
                    
                }

            });
        }
        public void AddOnLog(OnLogMessageWrittenDelegate OnLog)
        {
            LoggingService.Instance.LogLevel = LogLevel.Debug;
            LoggingService.Instance.Listener.OnLogMessageWritten = OnLog;
        }
        public void AddOnAccountRegistrationStateChangedDelegate(OnAccountRegistrationStateChangedDelegate OnAccountRegistrationStateChanged)
        {
            Core.Listener.OnAccountRegistrationStateChanged += OnAccountRegistrationStateChanged;
        }

        public void LogIn(string sipAccount, string sipPassword, string sipDomain, string sipProxy, TransportType type)
        {
            var sipAddress = $"sip:{sipAccount}@{sipDomain}";
            //Create Adress
            Address address = Factory.Instance.CreateAddress("sip:" + sipProxy);
            AuthInfo authInfo = Factory.Instance.CreateAuthInfo(sipAccount, "", sipPassword, "", "", "");
            
            address.Transport = type;

            //Params
            AccountParams accountParams = core.CreateAccountParams();
            accountParams.IdentityAddress = Factory.Instance.CreateAddress(sipAddress);
            
            accountParams.RegisterEnabled = true;
            accountParams.PushNotificationAllowed = false;
            accountParams.ServerAddress = address;

            Account account = core.CreateAccount(accountParams);


            core.AddAuthInfo(authInfo);
            core.AddAccount(account);
            
            core.DefaultAccount = account;
        }
        public void LogOut()
        {
            try
            {

                Account account = core.DefaultAccount;
                if (account != null)
                {
                    AccountParams accountParams = account.Params.Clone();
                    accountParams.RegisterEnabled = false;
                    account.Params = accountParams;
                    core.ClearAllAuthInfo();
                    core.ClearAccounts();
                }
            }
            catch
            {

            }
        }
        public bool ToggleMic()
        {
            return core.MicEnabled = !core.MicEnabled;
        }

        public bool ToggleSpeaker()
        {
           return core.CurrentCall.SpeakerMuted = !core.CurrentCall.SpeakerMuted;
        }
        public void Call(string uriToCall)
        {
            Address address = core.InterpretUrl(uriToCall);
            address.Transport = core.DefaultAccount.Transport;
            address.Domain = core.DefaultAccount.Params.ServerAddress.Domain;
            core.InviteAddress(address);

        }
        public void CreateConference(params string[] Adresses)
        {
            Conference conference = new Conference();
            if (core.Conference == null)
            {
                var paramsConf = core.CreateConferenceParams();
                paramsConf.AudioEnabled = true;
                paramsConf.VideoEnabled = false;
                paramsConf.LocalParticipantEnabled = true;
                paramsConf.ChatEnabled = false;
                conference = core.CreateConferenceWithParams(paramsConf);
            }
            else
            {
                conference = core.Conference;
            }
            if (conference != null)
            {
                foreach (var item in Adresses)
                {
                    var address = core.InterpretUrl(item);
                    address.Transport = core.DefaultAccount.Transport;
                    address.Domain = core.DefaultAccount.Params.ServerAddress.Domain;
                    conference.AddParticipant(address);
                }
                conference.InviteParticipants(conference.Participants, core.CreateCallParams(null));
            }
        }

        public async Task OpenMicrophonePopup()
        {
            PropertySet p = new PropertySet();
            p.Add("Mix", 0.5);
            AudioGraphSettings settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);
            
            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
            AudioGraph audioGraph = result.Graph;
            
            CreateAudioDeviceInputNodeResult resultNode = await audioGraph.CreateDeviceInputNodeAsync(Windows.Media.Capture.MediaCategory.Media);
            AudioDeviceInputNode deviceInputNode = resultNode.DeviceInputNode;
            
            deviceInputNode.Dispose();
            audioGraph.Dispose();
        }
    }
}