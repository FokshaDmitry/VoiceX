using pj;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using VoiceX.DAL.Context;

namespace VoiceX.Services
{
    public class CoreService : Account
    {
        public static PjsipLogger? writer;
        public static CallService? activeCall;
        public static string StunServer { get; set; } = "";
        public static string Version { get; set; } = "";
        public static bool useStunSetver { get; set; } = false;
        private static readonly CoreService instance = new CoreService();
        private Endpoint? core;
        public delegate void IncomingCall(); 
        public event IncomingCall? IncomingCallEvent;
        public event IncomingCall? OutgoingCallEvent;
        public static List<CallService>? activeCalls;
        AccountConfig? accCfg;
        public static CoreService Instance
        {
            get
            {
                return instance;
            }
        }
        public Endpoint Core 
        {   get 
            {
                if (core == null)
                {
                    core = new Endpoint();
                    writer = new PjsipLogger();
                    if (accCfg == null)
                    {
                        accCfg = new AccountConfig();
                    }
                    core.libCreate();
                    // Init library
                    var epConfig = new EpConfig();
                    epConfig.logConfig.level = 6;
                    epConfig.logConfig.writer = writer;
                    epConfig.uaConfig.stunServer = new StringVector();
                    if (!String.IsNullOrEmpty(StunServer) && useStunSetver)
                    {
                        epConfig.uaConfig.stunServer.Add(StunServer);
                    }
                    epConfig.uaConfig.userAgent = $"VoiceX_{Version}/{App.FirstLoginDate}";
                    
                    epConfig.uaConfig.maxCalls = 15;
                    core.libInit(epConfig);
                    var codecs = core.codecEnum2();
                    foreach (var codec in codecs)
                    {
                        if (!codec.codecId.ToLower().Contains("pcmu") && !codec.codecId.ToLower().Contains("pcma"))
                        {
                            core.codecSetPriority(codec.codecId, (byte)0);
                        }
                    }
                    var tpConf = new TransportConfig();
                    tpConf.port = 0;
                    try
                    {
                        var res = core.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_TCP, tpConf);

                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    try
                    {
                        var res = core.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_UDP, tpConf);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    // Start library
                    core.libStart();
                }
                return core; 
            } 
        }
       
        public async void Login(string username, string domain, string proxy, string password, int transport, bool iceEnabled, bool useIpRewrite)
        {
            try
            {
                var transportStr = transport == 0 ? ";transport=udp" : ";transport=tcp";
                //REG
                accCfg?.idUri = $"sip:{username}@{domain}" + transportStr;
                if (!String.IsNullOrEmpty(proxy))
                {
                    accCfg?.regConfig.registrarUri = $"sip:{domain}" + transportStr;
                    accCfg?.sipConfig.proxies.Clear();
                    accCfg?.sipConfig.proxies.Add($"<sip:{proxy}{transportStr};lr>");
                }
                else
                {
                    accCfg?.regConfig.registrarUri = $"sip:{domain}" + transportStr;
                }
                accCfg?.natConfig.iceEnabled = iceEnabled;
                
                accCfg?.natConfig.sdpNatRewriteUse = useIpRewrite ? 1 : 0;
                //CREATE
                accCfg?.sipConfig.authCreds.Clear();
                accCfg?.sipConfig.authCreds.Add(new AuthCredInfo("digest", "*", username, 0, password));
                
                instance.create(accCfg, true);
            }
            catch
            {

            }
        }
        public void reloadCore()
        {
            try
            {
                Logout();
                core?.libDestroy();
                core?.Dispose();
                core = null;
                core = Core;
                instance.create(accCfg, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        public async Task ChangeTransport(int id, string proxy, string username, string domain)
        {
            var transportStr = id == 0 ? ";transport=tcp" : ";transport=udp";
            accCfg?.idUri = $"sip:{username}@{domain}" + transportStr;
            if (!String.IsNullOrEmpty(proxy))
            {
                accCfg?.regConfig.registrarUri = $"sip:{domain}" + transportStr;
                accCfg?.sipConfig.proxies.Clear();
                accCfg?.sipConfig.proxies.Add($"<sip:{proxy}{transportStr};lr>");
            }
            else
            {
                accCfg?.regConfig.registrarUri = $"sip:{domain}" + transportStr;
            }
            instance.modify(accCfg);
        }
        public async Task UseIceEnabled(bool flag)
        {
            accCfg?.natConfig.iceEnabled = flag;
            instance.modify(accCfg);
        }
        public async Task UseIpRewrite(bool flag)
        {
            accCfg?.natConfig.sdpNatRewriteUse = flag == true ? 1 : 0;
            instance.modify(accCfg);
        }
        public void Logout()
        {
            try
            {
                Debug.WriteLine("[SIP] Выход из аккаунта...");
                instance.setRegistration(false);
                shutdown();
                Debug.WriteLine("[SIP] Аккаунт успешно разлогинен.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SIP] Ошибка выхода из аккаунта: {ex.Message}");
            }
        }
        public CallService MakeCall(string phone, string pbx)
        {
            if (this == null)
            {
                Debug.WriteLine("Ошибка: SIP-аккаунт не зарегистрирован!");
                return null!;
            }

            // Закрываем предыдущий вызов перед созданием нового
            if (activeCall == null)
            {
                string sipUri = $"sip:{phone}@{pbx}";

                try
                {
                    activeCall = new CallService(this);
                    CallOpParam prm = new CallOpParam(true);
                    
                    activeCall.makeCall(sipUri, prm);
                    OutgoingCallEvent?.Invoke();
                    return activeCall;
                }
                catch (Exception ex)
                {
                    activeCall = null;
                    Debug.WriteLine($"Ошибка при вызове: {ex.Message}");
                    return null!;
                }
            }
            return null!;
            // Создаём новый вызов
            
        }
        public override void onIncomingCall(OnIncomingCallParam prm)
        {
            Debug.WriteLine("[CALL] Входящий вызов...");
            try
            {
                if (activeCall == null)
                {
                    
                    activeCall = new CallService(this, prm.callId);
                    activeCall.prmMess = prm.rdata.wholeMsg;
                    Debug.WriteLine(prm.rdata.wholeMsg);
                    IncomingCallEvent?.Invoke();
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при ответе на вызов: {ex.Message}");
            }
        }
        public async static void AddParticipant(string phone, string server)
        {
            try
            {
                string participantUri = $"sip:{phone}@{server}";
                Debug.WriteLine($"[CALL] Звоним новому участнику: {participantUri}...");

                CallService newCall = new CallService(CoreService.Instance); // Создаём новый вызов
                CallOpParam callParam = new CallOpParam();
                newCall.makeCall(participantUri, callParam);

                Debug.WriteLine($"[CALL] Ожидаем соединение с {participantUri}...");
                // Ожидаем подключения нового участника
                while (true)
                {
                    await Task.Delay(1000);
                    CallInfo newCallInfo = newCall.getInfo();

                    if (newCallInfo.state == pjsip_inv_state.PJSIP_INV_STATE_CONFIRMED)
                    {
                        Debug.WriteLine($"[CALL] Новый участник {participantUri} подключен. Добавляем в конференцию...");
                        
                        if (activeCalls == null)
                        {
                            activeCalls = new List<CallService>();
                            activeCalls.Add(activeCall!);
                        }
                        else if (activeCalls.Count == 0)
                        {
                            activeCalls.Add(activeCall!);
                        }
                        activeCalls.Add(newCall);
                        MergeCalls(activeCalls);
                        return;
                    }
                    if (newCallInfo.state == pjsip_inv_state.PJSIP_INV_STATE_DISCONNECTED)
                    {
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при добавлении участника: {ex.Message}");
            }
        }
        private static void MergeCalls(List<CallService> callServices)
        {
            try
            {
                AudioMedia speaker = CoreService.Instance.Core.audDevManager().getPlaybackDevMedia();
                foreach (CallService call in callServices) 
                {
                    AudioMedia callAudio = call.getAudioMedia(0);
                    if (callAudio != null)
                    {
                        callAudio.startTransmit(callAudio);
                        callAudio.startTransmit(speaker);
                    }  
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при объединении звонков: {ex.Message}");
            }
        }
    }
    public class PjsipLogger : LogWriter
    {
        AddDbContext dbContext;
        public PjsipLogger()
        {
            dbContext = new AddDbContext();
            dbContext.InitializeDB();
        }
        public async override void write(LogEntry entry)
        {
            Debug.WriteLine($"[{entry.level}] {entry.msg}");
            await dbContext.AddLogAsync(new DAL.Entity.LogginNotes { Id = Guid.NewGuid(), Level = entry.level, Domain = entry.threadId.ToString(), Message = entry.msg, Created = DateTime.Now});
        }
    }
}
