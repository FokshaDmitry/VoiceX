using pj;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VoiceX.Services
{
    public class CallService : Call
    {

        AudioMediaPlayer? ringTonePlayer;
        public bool isMute;
        public List<string> CallAdtess {get; set; }
        DateTime startTime;
        public bool EndAllCalls;
        public delegate void EndCall(string Name, string Phone, DateTime StartCall);
        public event EndCall? EndCallEvent;
        public CallService(Account acc, int call_id = -1) : base(acc, call_id)
        {
            CallAdtess = new List<string>();
            startTime = new DateTime();
            startTime = DateTime.MinValue;
            isMute = false;
            EndAllCalls = false;
        }
        public void Accept()
        {
            if (CoreService.activeCall != null)
            {
                CallOpParam answerPrm = new CallOpParam();
                answerPrm.statusCode = pjsip_status_code.PJSIP_SC_OK;
                CoreService.activeCall.answer(answerPrm);
            }
        }
        public override void onCallState(OnCallStateParam prm)
        {
            CallInfo ci = getInfo();
            Debug.WriteLine($"[CALL] Статус: {ci.stateText}, Причина: {ci.lastReason}");
            switch (ci.state)
            {
                case pjsip_inv_state.PJSIP_INV_STATE_NULL:
                    Debug.WriteLine("[CALL] Абонент NULL");
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_CALLING:
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_INCOMING:
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_EARLY:
                    Debug.WriteLine("[CALL] Абонент звонит, проигрываем гудок...");
                    PlayRingTone("Outcmoing");
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_CONNECTING:
                    startTime = DateTime.Now;
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_CONFIRMED:
                    Debug.WriteLine("[CALL] Абонент ответил, отключаем гудок...");
                    StopRingTone();
                    //SetupAudio();
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_DISCONNECTED:
                    Debug.WriteLine($"[CALL] Вызов завершён: {ci.lastReason}");
                    try
                    {
                        StopRingTone();
                        
                        if (CoreService.activeCall != null)
                        {
                            if (EndAllCalls && CoreService.activeCalls != null && CoreService.activeCalls.Count != 0)
                            {
                                foreach (var call in CoreService.activeCalls)
                                {
                                    if (call != null)
                                    {
                                        var info = call.getInfo();
                                        if (info != null)
                                        {
                                            if (info.remoteContact != ci.remoteContact)
                                            {
                                                call.hangup(new CallOpParam());
                                            }
                                        }
                                    }
                                }
                                CoreService.activeCalls.Clear();
                                var phones = CoreService.activeCall.CallAdtess.Count() == 1 ? ExtractValue(CoreService.activeCall.CallAdtess.First()) : CoreService.activeCall.CallAdtess.Aggregate((current, next) => ExtractValue(current) + ", " + ExtractValue(next)).TrimEnd(' ', ',');
                                CoreService.activeCall = null;
                                CoreService.activeCalls = null;
                                EndCallEvent?.Invoke(phones, phones, startTime);
                            }
                            else
                            {
                                if (CoreService.activeCall.CallAdtess.Count() == 1)
                                {
                                    EndCallEvent?.Invoke(ExtractValue(CoreService.activeCall.CallAdtess.First()), ExtractValue(CoreService.activeCall.CallAdtess.First()), startTime);
                                    CoreService.activeCall = null;
                                    return;
                                }
                                var info = CoreService.activeCall.getInfo();
                                if (CoreService.activeCall.CallAdtess != null && CoreService.activeCall.CallAdtess.Count() != 0)
                                {
                                    var phone = ExtractValue(info.remoteContact);
                                    var address = CoreService.activeCall.CallAdtess.FirstOrDefault(c => c.Contains(phone));
                                    if (address != null)
                                    {
                                        CoreService.activeCall.CallAdtess.Remove(address);
                                    }
                                }
                                if (CoreService.activeCall.CallAdtess != null && CoreService.activeCall.CallAdtess.Count() == 0)
                                {
                                    if (info.remoteContact == ci.remoteContact)
                                    {
                                        EndCallEvent?.Invoke(ExtractValue(info.remoteUri), ExtractValue(info.remoteContact), startTime);
                                        CoreService.activeCall?.Dispose();
                                        CoreService.activeCall = null;
                                        DisableMicrophone();
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                    break;
                default:
                    break;
            }

        }

        public override void onCallMediaState(OnCallMediaStateParam prm)
        {
            CallInfo ci = getInfo();
            for (int i = 0; i < ci.media.Count(); i++)
            {
                if (ci.media[i].type == pjmedia_type.PJMEDIA_TYPE_AUDIO)
                {
                    try
                    {
                        AudioMedia aud_med = CoreService.activeCall!.getAudioMedia(i);
                        
                        AudDevManager mgr = CoreService.Instance.Core.audDevManager();
                        Debug.WriteLine($"[CALL] Включаем аудио... ");
                        aud_med.startTransmit(mgr.getPlaybackDevMedia());
                        Debug.WriteLine("[CALL] Включаем микрофон...");
                        if (!isMute)
                        {
                            mgr.getCaptureDevMedia().startTransmit(aud_med);
                        }
                    }
                    catch { }
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
        public bool MuteMicrophone(bool mute)
        {
            
            try
            {
                isMute = !mute;
                AudioMedia mic = CoreService.Instance.Core.audDevManager().getCaptureDevMedia();
                if (mic != null)
                {
                    if (!mute)
                    {
                        Debug.WriteLine("[AUDIO] Отключаем микрофон...");
                        AudioMedia remoteAudio = getAudioMedia(0);
                        mic.stopTransmit(remoteAudio);
                        return false;
                    }
                    else
                    {
                        Debug.WriteLine("[AUDIO] Включаем микрофон...");
                        AudioMedia remoteAudio = getAudioMedia(0);
                        mic.startTransmit(remoteAudio);
                        return true;

                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUDIO] Ошибка при изменении состояния микрофона: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Отключает или включает динамики (true = отключить, false = включить)
        /// </summary>
        public bool MuteSpeaker(bool mute)
        {
            try
            {
                AudioMedia speaker = CoreService.Instance.Core.audDevManager().getPlaybackDevMedia();
                AudioMedia remoteAudio = getAudioMedia(0);
                if (speaker != null)
                {
                    if (!mute)
                    {
                        Debug.WriteLine("[AUDIO] Отключаем динамики...");
                        if (remoteAudio != null)
                        {
                            remoteAudio.stopTransmit(speaker);
                        }
                        CoreService.Instance.Core.audDevManager().setPlaybackDev(-1);
                        isMute = true;
                        return false;
                    }
                    else
                    {
                        Debug.WriteLine("[AUDIO] Включаем динамики...");
                        CoreService.Instance.Core.audDevManager().setPlaybackDev(0);
                        if (remoteAudio != null)
                        {
                            remoteAudio.startTransmit(speaker);
                        }
                        isMute = false;
                        return true;

                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUDIO] Ошибка при изменении состояния динамиков: {ex.Message}");
                return false;
            }
        }
        public void SetHold(bool hold)
        {

            CallOpParam param = new CallOpParam(true);

            try
            {
                if (hold)
                {
                    setHold(param);
                }
                else
                {
                    CallSetting opt = param.opt;
                    opt.audioCount = 1;
                    opt.videoCount = 0;
                    opt.flag = ((int)pjsua_call_flag.PJSUA_CALL_UNHOLD);
                    reinvite(param);
                }
            }
            catch { }
        }
        public void PlayRingTone(string state)
        {
            try
            {
                if (ringTonePlayer == null)
                {
                    string ringtonePath = "";
                    switch (state)
                    {
                        case "Outcmoing":
                            ringtonePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Ring", "telefon-poshli-gudki-24931.wav");
                            break;
                        case "Incoming":
                            ringtonePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Ring", "iphone-11-pro.wav");
                            break;
                        case "Pause":
                            ringtonePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Ring", "hold.wav");
                            break;
                    }
                    ringTonePlayer = new AudioMediaPlayer();
                    ringTonePlayer.createPlayer(ringtonePath);
                    AudioMedia speaker = CoreService.Instance.Core.audDevManager().getPlaybackDevMedia();

                    if (speaker != null)
                    {
                        ringTonePlayer.startTransmit(speaker);
                        Debug.WriteLine("[CALL] Гудок запущен.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при запуске гудка: {ex.Message}");
            }
        }
        

        /// <summary>
        /// Объединить два вызова в конференцию
        /// </summary>
        
        public void TransferCall(string phone, string server)
        {
            try
            {
                string targetUri = $"sip:{phone}@{server}";
                Debug.WriteLine($"[CALL] Переводим вызов на {targetUri}...");

                CallOpParam param = new CallOpParam();
                xfer(targetUri, param); // Переводим вызов

                Debug.WriteLine("[CALL] Вызов успешно переведён.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при переводе вызова: {ex.Message}");
            }
        }
        public void StopRingTone()
        {
            try
            {
                if (ringTonePlayer != null)
                {
                    AudioMedia speaker = CoreService.Instance.Core.audDevManager().getPlaybackDevMedia();
                    if (speaker != null)
                    {
                        if (ringTonePlayer.getPortId() != -1 && speaker.getPortId() != -1) 
                        {
                            ringTonePlayer.stopTransmit(speaker);
                            ringTonePlayer.Dispose();
                            ringTonePlayer = null;
                        } 
                    }
                    Debug.WriteLine("[CALL] Гудок остановлен.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при остановке гудка: {ex.Message}");
            }
        }
        public string ExtractValue(string input)
        {
            Match match = Regex.Match(input, "\"([^\"]+)\"");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            match = Regex.Match(input, @"sip:([^@]+)@");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }
    }
}
