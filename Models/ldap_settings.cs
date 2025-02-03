

namespace VoiceX.Models
{
    public class Ldap_settings
    {
        public string Type { get; set; }
        public string Bs { get; set; }
        public string Dn { get; set; }
        public string Pass { get; set; }
        public Ldap_settings()
        {
            Bs = "";
            Dn = "";
            Pass = "";
            Type = "";
        }
    }
}
