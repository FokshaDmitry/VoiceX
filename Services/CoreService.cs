
using pj;
using System.Diagnostics;
using System.Security.Principal;
using System.Xml.Linq;
using Windows.UI;

namespace VoiceX.Services
{
    public class CoreService : Account
    {
        public static PjsipLogger? writer;
        public static CallService? activeCall;
        private static readonly CoreService instance = new CoreService();
        private Endpoint? core;
        public delegate void IncomingCall(); 
        public event IncomingCall? IncomingCallEvent;
        public event IncomingCall? OutgoingCallEvent;
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
                        core.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_TCP, tcfg);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    // Start library
                    //try
                    //{
                    //    tcfg.port = 5061;
                    //    core.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_TLS, tcfg);
                    //}
                    //catch (Exception e)
                    //{
                    //    Debug.WriteLine(e.Message);
                    //}
                    core.libStart();
                }
                return core; 
            } 
        }
       
        public void Login(string username, string domain, string proxy, string password, int transport)
        {
            try
            {
                AccountConfig accCfg = new AccountConfig();
                accCfg.sipConfig.transportId = 0;
                accCfg.presConfig.publishEnabled = true;
                //REG
                accCfg.idUri = $"sip:{username}@{domain}";
                accCfg.regConfig.registrarUri = $"sip:{proxy}";
                accCfg.regConfig.timeoutSec = 180;
                
                //NAT
                accCfg.natConfig.iceEnabled = true;
                
                //CREATE
                accCfg.sipConfig.authCreds.Add(new AuthCredInfo("digest", "*", username, 0, password));
                
                instance.create(accCfg, true);
            }
            catch
            {

            }
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
            if (entry.msg.Contains("EC suspended because of inactivity"))
            {
                if (CoreService.activeCall != null)
                {
                    if (!CoreService.activeCall.isMute)
                    {
                        CoreService.activeCall.hangup(new CallOpParam());
                    }
                }
            }
        }
    }
}
