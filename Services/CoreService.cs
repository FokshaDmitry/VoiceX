
using pj;
using System.Diagnostics;

namespace VoiceX.Services
{
    public class CoreService : Account
    {
        public static PjsipLogger? writer;
        public static CallService? activeCall;
        //public static BuddyService? buddy;
        private static readonly CoreService instance = new CoreService();
        private Endpoint? core;
        public delegate void IncomingCall(); 
        public event IncomingCall? IncomingCallEvent;
        public event IncomingCall? OutgoingCallEvent;

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
                    accCfg = new AccountConfig();
                    core.libCreate();
                    // Init library
                    EpConfig epConfig = new EpConfig();
                    epConfig.logConfig.level = 5;
                    epConfig.logConfig.writer = writer;
                    epConfig.uaConfig.stunServer.Add("ice.x-cloud.info:3478");
                    epConfig.uaConfig.natTypeInSdp = 1;
                    epConfig.uaConfig.maxCalls = 15;
                    
                    core.libInit(epConfig);

                    // Create transport
                    TransportConfig tcfg = new TransportConfig();
                    tcfg.port = 5060;
                    try
                    {
                        core.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_UDP, tcfg);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    try
                    {
                        core.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_TCP, tcfg);
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
       
        public void Login(string username, string domain, string proxy, string password, int transport, bool useProxy, bool iceEnabled, bool useIpRewrite)
        {
            try
            {
                accCfg!.sipConfig.transportId = transport;
                //REG
                accCfg.idUri = $"sip:{username}@{domain}";
                if (useProxy)
                {
                    accCfg.regConfig.registrarUri = $"sip:{proxy}";
                }
                else
                {
                    accCfg.regConfig.registrarUri = $"sip:{domain}";
                }
                accCfg.natConfig.iceEnabled = iceEnabled;
                accCfg.natConfig.sdpNatRewriteUse = useIpRewrite ? 1 : 0;
                //CREATE
                accCfg.sipConfig.authCreds.Add(new AuthCredInfo("digest", "*", username, 0, password));
                
                instance.create(accCfg, true);
            }
            catch
            {

            }
        }
        public async Task ChangeTransport(int id)
        {
            accCfg!.sipConfig.transportId = id;
            await ReloadCore();
        }
        public async Task UseIceEnabled(bool flag)
        {
            accCfg!.natConfig.iceEnabled = flag;
            await ReloadCore();
        }
        public async Task UseIpRewrite(bool flag)
        {
            accCfg!.natConfig.sdpNatRewriteUse = flag == true ? 1 : 0;
            await ReloadCore();
        }
        public async Task UseProxy(string? proxy)
        {
            accCfg!.regConfig.registrarUri = $"sip:{proxy}";
            await ReloadCore();
        }
        private async Task ReloadCore()
        {
            this.shutdown();
            await Task.Delay(2000);
            instance.create(accCfg, true);
        }
        public void Logout()
        {
            try
            {
                Debug.WriteLine("[SIP] Выход из аккаунта...");
                setRegistration(false);

                Thread.Sleep(500);
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
                    IncomingCallEvent?.Invoke();
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при ответе на вызов: {ex.Message}");
            }
        }
    }
    public class PjsipLogger : LogWriter
    {
        public override void write(LogEntry entry)
        {
            Debug.WriteLine($"[{entry.level}] {entry.msg}");
        }
    }
}
