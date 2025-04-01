using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using VoiceX.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VoiceX.Services
{
    public class WebService
    {
        private Account_data account;
        Get_certificate certificate;
        response_data responseData;
        user_dbinfo user_dbinfo;
        CertificateService certificateService;
        public WebService()
        {
            account = new Account_data();
            certificate = new Get_certificate();
            responseData = new response_data();
            responseData.data = new List<user_info>();
            user_dbinfo = new user_dbinfo();
            user_dbinfo.data = new data(); 
            certificateService = new CertificateService();
        }
        public async Task<string> PostToFax(string user_id, string message, string[] phones, byte[] fax_file, string pbxCode)
        {
            var ms = new MemoryStream();
            var content = new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(phones)), nameof(phones) },
                { new StringContent(user_id), nameof(user_id) },

                { new ByteArrayContent(fax_file), nameof(fax_file), "document.pdf" }
            };
            await content.CopyToAsync(ms);
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            HttpClient httpClient = new HttpClient(httpClientHandler);
            string raw = "";
            bool isRetry = false;
        retry:;
            try
            {

                var httprsp = await httpClient.PostAsync(new Uri($"https://app.voicex.biz/{pbxCode}/stats/api/fax/send.php"), content);
                raw = await httprsp.Content.ReadAsStringAsync();

            }
            catch
            {
                if (!isRetry)
                {
                    isRetry = true;
                    goto retry;
                }
            }
            var result = JObject.Parse(raw)?["message"]?.ToString() + " " + JObject.Parse(raw)?["type"]?.ToString();
            return result;
        }
        public async Task<Get_certificate> GetCertificateAsync(string pbxCode, string os)
        {
            if (pbxCode.Where(char.IsDigit).Count() != 6)
            {
                certificate.Error = "Wrong pbxCode";
                return certificate;
            }
            if (String.IsNullOrEmpty(os))
            {
                certificate.Error = "OS is empty";
                return certificate;
            }
            #region POST 
            string responseBody = "";
            try
            {
                X509Certificate2 clientCertificate = certificateService.GetCertificateByFriendlyName("default-windowsrsa");
                if (clientCertificate == null)
                {
                    certificate.Error = "Certificate don't found";
                    return certificate;
                }
                var content = new StringContent("{" + $"\"auth_code\": \"{pbxCode}\",\"uuid_device\": \"{Guid.NewGuid()}\",\"device_os\": \"{os}\"" + "}", Encoding.UTF8, "application/json");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(clientCertificate);
                using (var httpClient = new HttpClient(handler))
                {
                    var response = await httpClient.PutAsync(new Uri($"https://appauth.voicex.biz/{pbxCode.Substring(0, 3)}/stats/api_v2/app/auth.php"), content);
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                certificate.Error = ex.Message;
                return certificate;
            }
            if (!String.IsNullOrEmpty(responseBody))
            {
                try
                {
                    certificate = JsonConvert.DeserializeObject<Get_certificate>(responseBody)!;
                }
                catch
                {
                    certificate.Error = "JSON convert error";
                }
            }
            else
            {
                certificate.Error = "Answer is Empty";
            }
            #endregion
            return certificate;
        }

        //public async Task<string> ClickToCall(string phone, string companyID, string userID, string pbxCode)
        //{
        //    if (String.IsNullOrEmpty(userID))
        //    {
        //        return "Empty User Id";
        //    }
        //    if (String.IsNullOrEmpty(companyID))
        //    {

        //        return "Company ID is 0";
        //    }
        //    if (String.IsNullOrEmpty(pbxCode))
        //    {
        //        return "PBX not exist";
        //    }
        //    if (pbxCode.Where(char.IsDigit).Count() != 3)
        //    {
        //        return "Wrong PBX";
        //    }
        //    string responseBody = "";
        //    try
        //    {
        //        //var content = new HttpStringContent("{" + $"\"user_id\":\"{userID}\",\"phone\":\"{phone}\", \"company_id\":\"{companyID}\"" + "}", UnicodeEncoding.Utf8);
        //        //content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/json");
        //        //var certificate = await CertificateStores.FindAllAsync(new CertificateQuery() { FriendlyName = "app-cert" });
        //        //var clientCertificate = certificate.First();
        //        //var filter = new HttpBaseProtocolFilter();
        //        //filter.ClientCertificate = clientCertificate;
        //        //using (var httpClient = new Windows.Web.Http.HttpClient(filter))
        //        //{
        //        //    httpClient.DefaultRequestHeaders.Add("X-APP-TOKEN", userToken);
        //        //    var response = await httpClient.PostAsync(new Uri($"https://pbx{pbxCode.TrimStart('0')}.x-cloud.info/stats/api/CrossPlatform/click2call/call.php"), content);
        //        //    responseBody = await response.Content.ReadAsStringAsync();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //    return responseBody;
        //}
        public async Task<contacts_list> GetcontactsList(string sip_username, string companyID, string pbxCode, string userToken)
        {
            contacts_list contacts = new contacts_list
            {
                contacts = new List<Contact>()
            };
            if (String.IsNullOrEmpty(sip_username))
            {
                contacts.ResponseCode = System.Net.HttpStatusCode.NotFound;
                contacts.ResponseMessage = "Empty User Name";
                return contacts;
            }
            if (String.IsNullOrEmpty(companyID))
            {
                contacts.ResponseCode = System.Net.HttpStatusCode.NotFound;
                contacts.ResponseMessage = "Company ID is 0";
                return contacts;
            }
            if (String.IsNullOrEmpty(pbxCode))
            {
                contacts.ResponseCode = System.Net.HttpStatusCode.NotFound;
                contacts.ResponseMessage = "PBX not exist";
                return contacts;
            }
            if (pbxCode.Where(char.IsDigit).Count() != 3)
            {
                contacts.ResponseCode = System.Net.HttpStatusCode.NotFound;
                contacts.ResponseMessage = "Wrong PBX";
                return contacts;
            }
            string responseBody = "";
            try
            {
                X509Certificate2 clientCertificate = certificateService.GetCertificateByFriendlyName("app-cert");
                if (clientCertificate == null)
                {
                    contacts.ResponseCode = System.Net.HttpStatusCode.NotFound;
                    contacts.ResponseMessage = "Certificate don't found";
                    return contacts;
                }
                var content = new StringContent("{" + $"\"sip_account\":\"{sip_username}\",\"companyID\":\"{companyID}\"" + "}", Encoding.UTF8, "application/json");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(clientCertificate);
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-APP-TOKEN", userToken);
                    var response = await httpClient.PutAsync(new Uri($"https://app.voicex.biz/{pbxCode}/stats/api_v2/app/get_redirect_list.php"), content);
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                contacts.ResponseCode = System.Net.HttpStatusCode.NotFound;
                contacts.ResponseMessage = ex.Message;
                return contacts;
            }
            if (!String.IsNullOrEmpty(responseBody))
            {
                try
                {
                    var contactsList = JObject.Parse(responseBody)?["data"];
                    if (contacts != null)
                    {
                        contacts = JsonConvert.DeserializeObject<contacts_list>(responseBody)!;
                        if (contacts.contacts != null)
                        {
                            contacts.contacts = JsonConvert.DeserializeObject<List<Contact>>(contactsList?.ToString()!)!;
                        }
                        else
                        {
                            contacts.contacts = new List<Contact>();
                            contacts.contacts = JsonConvert.DeserializeObject<List<Contact>>(contactsList!.ToString())!;
                        }
                    }
                }
                catch (Exception ex)
                {
                    contacts.ResponseCode = System.Net.HttpStatusCode.Conflict;
                    contacts.ResponseMessage = ex.Message;
                    return contacts;
                }
            }
            
            return contacts!;
        }
        public async Task<System.Net.HttpStatusCode> ChangeCallType(string callType, string pbxCode, string userToken)
        {
            string responseBody = "";
            try
            {
                X509Certificate2 clientCertificate = certificateService.GetCertificateByFriendlyName("app-cert");
                if (clientCertificate == null)
                {
                    return System.Net.HttpStatusCode.NotFound;
                }
                var content = new StringContent("{" + $"\"call_type\":\"{callType}\"" + "}", Encoding.UTF8, "application/json");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(clientCertificate);
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-APP-TOKEN", userToken);
                    var response = await httpClient.PostAsync(new Uri($"https://app.voicex.biz/{pbxCode}/stats/api_v2/app/change_call_type.php"), content);
                    responseBody = await response.Content.ReadAsStringAsync();
                }
                return JsonConvert.DeserializeObject<System.Net.HttpStatusCode>(JObject.Parse(responseBody)["responseCode"]?.ToString()!);
            }
            catch
            {
                return System.Net.HttpStatusCode.BadRequest;
            }
        }
        public async Task LogOut(string pbxCode, string userToken)
        {
            string responseBody = "";
            try
            {
                X509Certificate2 clientCertificate = certificateService.GetCertificateByFriendlyName("app-cert");
               
                var content = new StringContent("", Encoding.UTF8, "application/json");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(clientCertificate);
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-APP-TOKEN", userToken);
                    var response = await httpClient.PutAsync(new Uri($"https://app.voicex.biz/{pbxCode}/stats/api_v2/app/logout.php"), content);
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch
            {
                return;
            }
        }
        public async Task<Account_data> GetAccountSettings(string pbxCode, string userToken)
        {
            #region GET
            string responseBody = "";
            try
            {
                X509Certificate2 clientCertificate = certificateService.GetCertificateByFriendlyName("app-cert");
                if (clientCertificate == null)
                {
                    account.ResponseCode = System.Net.HttpStatusCode.BadRequest;
                    account.ResponseMessage = "Certificate don't found";
                    return account;
                }
                var content = new StringContent("", Encoding.UTF8, "application/json");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(clientCertificate);
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-APP-TOKEN", userToken);
                    var response = await httpClient.PutAsync(new Uri($"https://app.voicex.biz/{pbxCode}/stats/api_v2/app/get_account_settings.php"), content);
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                account.ResponseCode = System.Net.HttpStatusCode.BadRequest;
                account.ResponseMessage = ex.Message;
                return account;
            }
            #endregion
            if (!String.IsNullOrEmpty(responseBody))
            {
                try
                {
                    account = JsonConvert.DeserializeObject<Account_data>(responseBody)!;
                    var sipSettings = JObject.Parse(responseBody)?["data"]?["sip_settings"];
                    var usedData = JObject.Parse(responseBody)?["data"]?["user_data"];
                    var appData = JObject.Parse(responseBody)?["data"];
                    var ldapData = JObject.Parse(responseBody)?["data"]?["ldap_settings"];
                    account.Data = JsonConvert.DeserializeObject<Data>(appData?.ToString()!)!;
                    account.Data.Sip_Settings = JsonConvert.DeserializeObject<Sip_settings>(sipSettings?.ToString()!)!;
                    account.Data.Sip_Settings.Sip_secret = Encoding.UTF8.GetString(Convert.FromBase64String(account.Data.Sip_Settings.Sip_secret));
                    account.Data.Sip_Settings.Sip_server = $"pbx{pbxCode.Substring(1, 2)}.x-cloud.info";
                    account.Data.User_Data = JsonConvert.DeserializeObject<User_data>(usedData?.ToString()!)!;
                    account.Data.Ldap_Settings = JsonConvert.DeserializeObject<Ldap_settings>(ldapData?.ToString()!)!;
                }
                catch (Exception ex)
                {
                    account.ResponseCode = System.Net.HttpStatusCode.BadRequest;
                    account.ResponseMessage = ex.Message;
                    return account;
                }

            }
            return account;
        }
        public async Task<Get_pauses> GetPauses(string sipUsername, string pbxCode, string userToken)
        {
            Get_pauses getPauses = new Get_pauses
            {
                ResponseData = new Status_pause
                {
                    Pauses = new List<Pause>()
                }
            };
            getPauses.ResponseMessage = "";
            if (String.IsNullOrEmpty(sipUsername))
            {
                getPauses.ResponseCode = System.Net.HttpStatusCode.NotFound;
                getPauses.ResponseMessage = "Empty User Name";
                return getPauses;
            }
            if (String.IsNullOrEmpty(pbxCode))
            {
                getPauses.ResponseCode = System.Net.HttpStatusCode.NotFound;
                getPauses.ResponseMessage = "PBX not exist";
                return getPauses;
            }
            if (pbxCode.Where(char.IsDigit).Count() != 3)
            {
                getPauses.ResponseCode = System.Net.HttpStatusCode.NotFound;
                getPauses.ResponseMessage = "Wrong pbxCode";
                return getPauses;
            }
            string responseBody = "";
            try
            {
                X509Certificate2 clientCertificate = certificateService.GetCertificateByFriendlyName("app-cert");
                if (clientCertificate == null)
                {
                    getPauses.ResponseCode = System.Net.HttpStatusCode.BadRequest;
                    getPauses.ResponseMessage = "Certificate not found";
                    return getPauses;
                }
                var content = new StringContent("{" + $"\"account\":\"{sipUsername}\"" + "}", Encoding.UTF8, "application/json");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(clientCertificate);
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-APP-TOKEN", userToken);
                    var response = await httpClient.PutAsync(new Uri($"https://app.voicex.biz/{pbxCode}/stats/api_v2/app/pauses/get_pauses.php"), content);
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                getPauses.ResponseCode = System.Net.HttpStatusCode.NotFound;
                getPauses.ResponseMessage = ex.Message;
                return getPauses;
            }
            if (!String.IsNullOrEmpty(responseBody))
            {
                try
                {
                    getPauses = JsonConvert.DeserializeObject<Get_pauses>(responseBody)!;
                    getPauses.ResponseData = JsonConvert.DeserializeObject<Status_pause>(JObject.Parse(responseBody)?["responseData"]?.ToString()!)!;
                    getPauses.ResponseData!.Pauses = new List<Pause>();
                    getPauses.ResponseData.Pauses = JsonConvert.DeserializeObject<List<Pause>>(JObject.Parse(responseBody)?["responseData"]?["pauses"]?.ToString()!)!;
                    return getPauses;
                }
                catch (Exception ex)
                {
                    getPauses.ResponseCode = System.Net.HttpStatusCode.NotFound;
                    getPauses.ResponseMessage = ex.Message;
                    return getPauses;
                }
            }
            else
            {
                getPauses.ResponseCode = System.Net.HttpStatusCode.NotFound;
                getPauses.ResponseMessage = "Responce is Empty";
                return getPauses;
            }
        }
        public async Task<Responce_model> SetPause(string sipUsername, int PauseId, string pbxCode, string userToken)
        {
            Responce_model responceModel = new Responce_model
            {
                ResponseMessage = ""
            };
            if (String.IsNullOrEmpty(sipUsername))
            {
                responceModel.ResponseCode = System.Net.HttpStatusCode.NotFound;
                responceModel.ResponseMessage = "Empty User Name";
                return responceModel;
            }
            if (String.IsNullOrEmpty(pbxCode))
            {
                responceModel.ResponseCode = System.Net.HttpStatusCode.NotFound;
                responceModel.ResponseMessage = "PBX not exist";
                return responceModel;
            }
            if (pbxCode.Where(char.IsDigit).Count() != 3)
            {
                responceModel.ResponseCode = System.Net.HttpStatusCode.NotFound;
                responceModel.ResponseMessage = "Wrong pbxCode";
                return responceModel;
            }
            string responseBody = "";
            try
            {
                X509Certificate2 clientCertificate = certificateService.GetCertificateByFriendlyName("app-cert");
                if (clientCertificate == null)
                {
                    responceModel.ResponseCode = System.Net.HttpStatusCode.NotFound;
                    responceModel.ResponseMessage = "Certificate not found";
                    return responceModel;
                }
                var content = new StringContent("{" + $"\"account\":\"{sipUsername}\", \"pause_id\": \"{PauseId}\"" + "}", Encoding.UTF8, "application/json");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(clientCertificate);
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-APP-TOKEN", userToken);
                    var response = await httpClient.PostAsync(new Uri($"https://app.voicex.biz/{pbxCode}/stats/api_v2/app/pauses/set_pause.php"), content);
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                responceModel.ResponseCode = System.Net.HttpStatusCode.NotFound;
                responceModel.ResponseMessage = ex.Message;
                return responceModel;
            }
            if (!String.IsNullOrEmpty(responseBody))
            {
                try
                {
                    responceModel.ResponseCode = JsonConvert.DeserializeObject<System.Net.HttpStatusCode>(JObject.Parse(responseBody)?["responseCode"]?.ToString()!);
                }
                catch
                {
                    responceModel.ResponseCode = System.Net.HttpStatusCode.NoContent;
                }
            }
            else
            {
                responceModel.ResponseCode = System.Net.HttpStatusCode.NoContent;
            }
            return responceModel;
        }
    }
}