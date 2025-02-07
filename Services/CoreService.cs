
using pj;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    epConfig.logConfig.level = 6;
                    epConfig.logConfig.writer = writer;
                    epConfig.uaConfig.stunServer.Add("ice.x-cloud.info:3478"); 
                    epConfig.uaConfig.natTypeInSdp = 1;

                    core.libInit(epConfig);

                    // Create transport
                    TransportConfig tcfg = new TransportConfig();
                    tcfg.port = 5060;
                    core.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_TCP, tcfg);

                    // Start library
                    core.libStart();
                }
                return core; 
            } 
        }
        public void Login(string username, string domain, string proxy, string password, int transport)
        {
            AccountConfig accCfg = new AccountConfig();
            accCfg.idUri = $"sip:{username}@{domain}";
            accCfg.regConfig.registrarUri = $"sip:{proxy}";
            accCfg.regConfig.registerOnAdd = true;
            accCfg.sipConfig.transportId = 0;
            accCfg.presConfig.publishEnabled = true; 
            accCfg.regConfig.timeoutSec = 300;
            accCfg.sipConfig.authCreds.Add(new AuthCredInfo("digest", "*", username, 0, password));
            create(accCfg);
        }

        public void EnsureRegistration()
        {
            if (this == null) return;

            AccountInfo accInfo = getInfo();

            if (accInfo.regIsActive)
            {
                Debug.WriteLine("[SIP] Регистрация уже активна, повторная регистрация не требуется.");
                return;
            }

            try
            {
                Debug.WriteLine("[SIP] Начинаем повторную регистрацию...");
                Task.Run(() =>
                {
                    try
                    {
                        setRegistration(true);
                        Debug.WriteLine("[SIP] Регистрация выполнена успешно.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[SIP] Ошибка при повторной регистрации: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SIP] Ошибка в EnsureRegistration: {ex.Message}");
            }
        }
        public CallService MakeCall(string phone, string pbx)
        {
            if (this == null)
            {
                Debug.WriteLine("Ошибка: SIP-аккаунт не зарегистрирован!");
                return null;
            }

            // Закрываем предыдущий вызов перед созданием нового
            if (activeCall == null)
            {
                string sipUri = $"sip:{phone}@{pbx}";

                try
                {
                    activeCall = new CallService(this);
                    CallOpParam prm = new CallOpParam(true);
                    prm.opt.audioCount = 1;
                    prm.opt.videoCount = 0;

                    activeCall.makeCall(sipUri, prm);
                    return activeCall;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при вызове: {ex.Message}");
                    return null;
                }
            }
            return null;
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
