using System.Net;

namespace VoiceX.Models
{
    public class Account_data : Responce_model
    {
        public Data Data { get; set; }
        public Account_data()
        {
            ResponseCode = new HttpStatusCode();
            ResponseMessage = "";
            Data = new Data
            {
                Enable_log = 0,
                Is_mobile = 0,
                Ldap_Settings = new Ldap_settings()
            };
            Data.Ldap_Settings.Bs = "";
            Data.Ldap_Settings.Dn = "";
            Data.Ldap_Settings.Pass = "";
            Data.Ldap_Settings.Type = "";

        }
    }
}
