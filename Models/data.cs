namespace VoiceX.Models
{
    public class Data
    {
        public int Enable_log { get; set; }
        public int Is_mobile { get; set; }
        public Ldap_settings Ldap_Settings { get; set; }
        public Sip_settings Sip_Settings { get; set; }
        public User_data User_Data { get; set; }
        public string Device_type { get; set; }
        public custom_data Custom_Data { get; set; }

        public Data()
        {
            Enable_log = 0;
            Is_mobile = 0;
            Device_type = "";
            Ldap_Settings = new Ldap_settings
            {
                Base = "",
                Dn = "",
                Pass = "",
                Type = "",
                Server = ""
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
            Custom_Data = new custom_data
            {
                url = ""
            };
        }
    }

}
