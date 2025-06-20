namespace VoiceX.Models
{
    public class Sip_settings
    {
        public string Sip_server { get; set; }
        public string Sip_proxy { get; set; }
        public string Sip_username { get; set; }
        public string Sip_secret { get; set; }
        public string Stun_server { get; set; }
        public Sip_settings()
        {
            Sip_server = "";
            Sip_proxy = "";
            Sip_secret = "";
            Sip_username = "";
        }
    }
}
