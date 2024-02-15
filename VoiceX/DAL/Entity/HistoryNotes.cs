using System;
using VoiceX.Enums;

namespace VoiceX.DAL.Entity
{
    public class HistoryNotes
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
        public String Phone { get; set; }
        public StatusCall StatusCall { get; set; }
        public DateTime StartDialog { get; set; }
        public DateTime EndDialog { get; set; }
    }
}
