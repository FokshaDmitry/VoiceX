//using pj;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace VoiceX.Services
//{
//    public class SipManager
//    {
//        private static Endpoint ep;
//        private static MyAccount account;
//        private static bool isRunning = false;
//        private static PjsipLogger writer;
//        public SipManager()
//        {
//            writer = new PjsipLogger();
//        }
//        public static void StartSIP(string domain, string server, string username, string password)
//        {
//            if (isRunning) return;

//            try
//            {
//                ep = new Endpoint();
//                ep.libCreate();
//;
//                EpConfig epConfig = new EpConfig();
//                epConfig.logConfig.level = 6;
//                epConfig.logConfig.writer = writer;
//                ep.libInit(epConfig);

//                // Создаём TCP-транспорт и слушаем порт
//                TransportConfig tcpTransportConfig = new TransportConfig();
//                tcpTransportConfig.port = 5060;
//                int transportId = ep.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_TCP, tcpTransportConfig);

//                ep.libStart();

//                // Создаём аккаунт
//                account = new MyAccount();
//                account.Register(domain, server, username, password, transportId);

//                isRunning = true;

//                Debug.WriteLine("[SIP] SIP-сервер запущен и слушает порт 5060...");
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine($"[SIP] Ошибка при запуске: {ex.Message}");
//            }
//        }

//        public static void StopSIP()
//        {
//            if (ep != null)
//            {
//                ep.libDestroy();
//                ep = null;
//                isRunning = false;
//                Debug.WriteLine("[SIP] SIP-сервер остановлен.");
//            }
//        }
//    }

//    // Класс SIP-аккаунта
//    public class MyAccount : Account
//    {
//        public void Register(string domain, string server, string username, string password, int transportId)
//        {
//            AccountConfig accCfg = new AccountConfig();
//            accCfg.idUri = $"sip:{username}@{domain}";
//            accCfg.regConfig.registrarUri = $"sip:{server}";
//            accCfg.regConfig.registerOnAdd = true;
//            accCfg.regConfig.timeoutSec = 300; // Обновление регистрации раз в 5 минут
//            accCfg.sipConfig.transportId = transportId;

//            AuthCredInfo cred = new AuthCredInfo("digest", "*", username, 0, password);
//            accCfg.sipConfig.authCreds.Add(cred);

//            create(accCfg);
//            Debug.WriteLine("[SIP] Аккаунт зарегистрирован.");
//        }

//        public override void onRegState(OnRegStateParam prm)
//        {
//            AccountInfo ai = getInfo();
//            Debug.WriteLine($"[SIP] Регистрация: {(ai.regIsActive ? "Успешна" : "Потеряна")}");

//            if (!ai.regIsActive)
//            {
//                Debug.WriteLine("[SIP] Перерегистрируемся через 5 секунд...");
//                Task.Delay(5000).ContinueWith(_ => setRegistration(true));
//            }
//        }

//        public override void onIncomingCall(OnIncomingCallParam prm)
//        {
//            Debug.WriteLine("[CALL] Входящий звонок!");

//            MyCall call = new MyCall(this, prm.callId);
//            CallOpParam answerPrm = new CallOpParam();
//            answerPrm.statusCode = pjsip_status_code.PJSIP_SC_OK;

//            call.answer(answerPrm);
//            Debug.WriteLine("[CALL] Вызов принят.");
//        }
//        public void MakeCall()
//        {
//            if (account == null)
//            {
//                Debug.WriteLine("Ошибка: SIP-аккаунт не зарегистрирован!");
//                return;
//            }

//            // Закрываем предыдущий вызов перед созданием нового
//            if (activeCall != null)
//            {
//                Debug.WriteLine("Завершаем предыдущий вызов.");
//                activeCall.hangup(new CallOpParam());
//                activeCall.Dispose();
//                activeCall = null;
//            }

//            // Создаём новый вызов
//            string sipUri = $"sip:1214@pbx51.x-cloud.info;transport=tcp";
//            Debug.WriteLine($"Попытка вызова через TCP: {sipUri}");

//            try
//            {
//                activeCall = new MyCall(acc);
//                CallOpParam prm = new CallOpParam(true);
//                prm.opt.audioCount = 1;
//                prm.opt.videoCount = 0;

//                activeCall.makeCall(sipUri, prm);
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine($"Ошибка при вызове: {ex.Message}");
//            }
//        }
//    }

//    // Класс для управления вызовами
//    public class MyCall : Call
//    {
//        public MyCall(Account acc, int call_id = -1) : base(acc, call_id) { }

//        public override void onCallState(OnCallStateParam prm)
//        {
//            CallInfo ci = getInfo();
//            Debug.WriteLine($"[CALL] Статус вызова: {ci.stateText}");

//            if (ci.state == pjsip_inv_state.PJSIP_INV_STATE_DISCONNECTED)
//            {
//                Debug.WriteLine("[CALL] Вызов завершён.");
//                this.Dispose();
//            }
//        }
//    }
//    public class PjsipLogger : LogWriter
//    {
//        public override void write(LogEntry entry)
//        {
//            Debug.WriteLine($"[{entry.level}] {entry.msg}");
//        }
//    }
//}
