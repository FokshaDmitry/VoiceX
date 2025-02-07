using pj;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceX.Services
{
    public class CallService : Call
    {

        AudioMediaPlayer? ringTonePlayer;
        private bool isOnHold;

        public CallService(Account acc, int call_id = -1) : base(acc, call_id)
        {

        }
        public void Accept()
        {
            if (CoreService.activeCall != null)
            {
                CallOpParam answerPrm = new CallOpParam();
                answerPrm.statusCode = pjsip_status_code.PJSIP_SC_ACCEPTED;
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
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_CALLING:
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_INCOMING:
                    PlayRingTone("Incoming");
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_EARLY:
                    Debug.WriteLine("[CALL] Абонент звонит, проигрываем гудок...");
                    PlayRingTone("Outcmoing");
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_CONNECTING:
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_CONFIRMED:
                    Debug.WriteLine("[CALL] Абонент ответил, отключаем гудок...");
                    StopRingTone();
                    break;
                case pjsip_inv_state.PJSIP_INV_STATE_DISCONNECTED:
                    Debug.WriteLine($"[CALL] Вызов завершён: {ci.lastReason}");
                    try
                    {
                        StopRingTone();
                        DisableMicrophone();
                        if (CoreService.activeCall != null)
                        {
                            CoreService.activeCall.Dispose();
                            CoreService.activeCall = null;
                        }
                        this.Dispose();
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
        public bool MuteMicrophone(bool mute)
        {
            try
            {
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
        public void PlayHoldMusic()
        {
            try
            {
                var ringtonePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Ring", "hold.wav");
                ringTonePlayer = new AudioMediaPlayer();
                ringTonePlayer.createPlayer(ringtonePath); // Файл должен быть в корне проекта или указан полный путь
                AudioMedia remoteAudio = getAudioMedia(0); // Аудио поток звонка

                if (remoteAudio != null)
                { // Отключаем микрофон

                    Debug.WriteLine("[CALL] Включаем музыку для собеседника...");
                    ringTonePlayer.startTransmit(remoteAudio); // Передаём музыку в звонок
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при запуске музыки ожидания: {ex.Message}");
            }
        }
        private void PlayRingTone(string state)
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

        public void StopRingTone()
        {
            try
            {
                if (ringTonePlayer != null)
                {
                    AudioMedia speaker = CoreService.Instance.Core.audDevManager().getPlaybackDevMedia();
                    if (speaker != null)
                    {
                        ringTonePlayer.stopTransmit(speaker);
                        ringTonePlayer.Dispose();
                        ringTonePlayer = null;
                    }
                    Debug.WriteLine("[CALL] Гудок остановлен.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Ошибка при остановке гудка: {ex.Message}");
            }
        }
    }
}
