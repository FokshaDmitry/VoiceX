
using pj;
using System.Diagnostics;
using System.Security.Principal;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VoiceX.Services
{
    public class CoreService : Account
    {
        public static PjsipLogger writer;
        public static MyCall activeCall;
        private static readonly CoreService instance = new CoreService();
        private Endpoint core;
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
                    epConfig.uaConfig.stunServer.Add("ice.x-cloud.info:3478"); // STUN-сервер
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
            accCfg.regConfig.timeoutSec = 60;
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
        public MyCall MakeCall(string phone, string pbx)
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
                    activeCall = new MyCall(this);
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

            //if (activeCall != null)
            //{
            //    Debug.WriteLine("[CALL] Есть активный вызов, отклоняем новый...");
            //    CallOpParam rejectPrm = new CallOpParam();
            //    rejectPrm.statusCode = pjsip_status_code.PJSIP_SC_BUSY_HERE;
            //    Call call = new Call(this, prm.callId);
            //    call.answer(rejectPrm);
            //    return;
            //}

            try
            {
                activeCall = new MyCall(this, prm.callId);
                CallOpParam answerPrm = new CallOpParam();
                answerPrm.statusCode = pjsip_status_code.PJSIP_SC_ACCEPTED;

                Debug.WriteLine("[CALL] Принимаем вызов...");
                activeCall.answer(answerPrm);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при ответе на вызов: {ex.Message}");
            }
        }
    }
    public class MyCall : Call
    {

        public MyCall(Account acc, int call_id = -1) : base(acc, call_id)
        {

        }

        public override void onCallState(OnCallStateParam prm)
        {
            CallInfo ci = getInfo();
            Debug.WriteLine($"[CALL] Статус: {ci.stateText}, Причина: {ci.lastReason}");
            switch (ci.state)
            {
                case pjsip_inv_state.PJSIP_INV_STATE_NULL:
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_CALLING:
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_INCOMING:
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_EARLY:
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_CONNECTING:
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_CONFIRMED:
                    Debug.WriteLine("[CALL] Вызов подключён, пробуем настроить аудио...");
                    SetupAudio();
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_DISCONNECTED:
                    Debug.WriteLine($"[CALL] Вызов завершён: {ci.lastReason}");
                    try {
                        DisableMicrophone();
                        this.Dispose(); 
                    } catch { }
                    break;
                default:
                    break;
            }

        }

        public override void onCallMediaState(OnCallMediaStateParam prm)
        {
            CallInfo ci = getInfo();
            if (ci.media.Count > 0 && ci.media[0].type == pjmedia_type.PJMEDIA_TYPE_AUDIO)
            {
                Debug.WriteLine("[CALL] Аудио обнаружено, настраиваем вход и выход...");

                try
                {
                    // Получаем удалённый аудиопоток (голос собеседника)
                    AudioMedia remoteAudio = getAudioMedia(0);
                    if (remoteAudio == null)
                    {
                        Debug.WriteLine("[CALL] Ошибка: `remoteAudio` == null!");
                        return;
                    }

                    // Получаем локальные аудиоустройства
                    AudioMedia speaker = CoreService.Instance.Core.audDevManager().getPlaybackDevMedia();
                    AudioMedia microphone = CoreService.Instance.Core.audDevManager().getCaptureDevMedia();

                    if (speaker == null || microphone == null)
                    {
                        Debug.WriteLine("[CALL] Ошибка: Аудиоустройства не найдены!");
                        return;
                    }

                    // Подключаем микрофон и динамик
                    remoteAudio.startTransmit(speaker);   // Воспроизведение звука в динамики
                    microphone.startTransmit(remoteAudio); // Передача звука в вызов

                    Debug.WriteLine("[CALL] Аудио подключено!");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CALL] Ошибка при подключении аудио: {ex.Message}");
                }
            }
        }
        public void DisableMicrophone()
        {
            try
            {
                AudioMedia mic = CoreService.Instance.Core.audDevManager().getCaptureDevMedia();
                AudioMedia speaker = CoreService.Instance.Core.audDevManager().getPlaybackDevMedia();

                if (mic != null)
                {
                    Debug.WriteLine("[CALL] Отключаем передачу звука с микрофона...");
                    mic.stopTransmit(mic);
                }
                if (speaker != null)
                {
                    Debug.WriteLine("[CALL] Отключаем передачу звука с микрофона...");
                    speaker.stopTransmit(speaker);
                }
                Debug.WriteLine("[CALL] Отключаем микрофон полностью...");
                CoreService.Instance.Core.audDevManager().setCaptureDev(-1);
                CoreService.Instance.Core.audDevManager().setPlaybackDev(-1); 
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при отключении микрофона: {ex.Message}");
            }
        }
        private void SetupAudio()
        {
            try
            {
                AudioMedia mic = CoreService.Instance.Core.audDevManager().getCaptureDevMedia();
                AudioMedia speaker = CoreService.Instance.Core.audDevManager().getPlaybackDevMedia();

                if (mic == null || speaker == null)
                {
                    Debug.WriteLine("[CALL] Ошибка: Не найдены аудиоустройства!");
                    return;
                }

                mic.startTransmit(speaker);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при настройке аудио: {ex.Message}");
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
