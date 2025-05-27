namespace VoiceX.Models
{
    public class Pause
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Type { get; set; }
        public int Subtype { get; set; }
        public int Dnd { get; set; }
        public int Duration { get; set; }
    }
}
