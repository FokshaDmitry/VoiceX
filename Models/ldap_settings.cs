namespace VoiceX.Models
{
    public class Ldap_settings
    {
        public string Type { get; set; }
        public string Base { get; set; }
        public string Dn { get; set; }
        public string Pass { get; set; }
        public string Server { get; set; }
        public Ldap_settings()
        {
            Base = "";
            Dn = "";
            Pass = "";
            Type = "";
            Server = "";
        }
    }
}
