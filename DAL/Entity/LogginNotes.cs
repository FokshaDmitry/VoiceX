
using System;

namespace VoiceX.DAL.Entity
{
    public class LogginNotes
    {
        public Guid Id { get; set; }
        public String? Domain { get; set; }
        public String? Message { get; set; }
        public int? Level { get; set; }
        public DateTime Created { get; set; }
    }
}
