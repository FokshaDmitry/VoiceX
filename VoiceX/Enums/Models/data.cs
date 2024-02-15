

namespace VoiceX.Models
{
    public class Data
    {
        public int Enable_log { get; set; }
        public int Is_mobile { get; set; }
        public Ldap_settings Ldap_Settings { get; set; }
        public Sip_settings Sip_Settings { get; set; }
        public User_data User_Data { get; set; }
        public Data()
        {
            Enable_log = 0;
            Is_mobile = 0;
            Ldap_Settings = new Ldap_settings
            {
                Bs = "",
                Dn = "",
                Pass = "",
                Type = ""
            };
            Sip_Settings = new Sip_settings
            {
                Sip_proxy = "",
                Sip_secret = "",
                Sip_server = "",
                Sip_username = ""
            };
            User_Data = new User_data
            {
                CompanyID = "",
                UserID = "",
                Email = "",
                Name = ""
            };
        }
    }

}
