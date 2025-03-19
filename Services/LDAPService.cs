using System.Collections.Generic;
using System;
using VoiceX.Models;
using System.Linq;
using System.Net;
using System.DirectoryServices.Protocols;
using System.Diagnostics;
using SearchScope = System.DirectoryServices.Protocols.SearchScope;

namespace VoiceX.Services
{
    public class LDAPService
    {
        LdapConnection? ldapConnection;
        public LDAPService() 
        {
            
        }
        public void Authenticate(string distinguishedName, string password)
        {
            if (!String.IsNullOrEmpty(distinguishedName) && !String.IsNullOrEmpty(password))
            {
                try
                {
                    // Подключение
                    var identifier = new LdapDirectoryIdentifier("pb.i.voicex.center", 389);

                    var credential = new NetworkCredential(distinguishedName, password);
                    ldapConnection = new LdapConnection(identifier, credential, AuthType.Basic);


                    ldapConnection.SessionOptions.ProtocolVersion = 3;

                    ldapConnection.Bind(); // Аутентификация

                    Console.WriteLine("✅ Успешно подключено к LDAP!");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            else
            {
                ldapConnection = null;
            }

        }
        public List<LdapUser> GetLdapUsers(int limit, string baseDN)
        {
            var ldapUsers = new List<LdapUser>();
            if (ldapConnection != null)
            {
                var descriptions = new List<string>();

                // Алфавит латиницы a-z и иврит א–ת (юникод от 1488 до 1514)
                List<string> searchPrefixes = new List<string>();

                for (char c = 'a'; c <= 'z'; c++)
                    searchPrefixes.Add(c + "*");

                for (int i = 1488; i <= 1514; i++) // א–ת
                    searchPrefixes.Add(((char)i) + "*");

                foreach (string prefix in searchPrefixes)
                {
                    Console.WriteLine($"🔍 Поиск: description={prefix}");

                    var filter = $"(description={prefix})";
                    var searchRequest = new SearchRequest(
                        baseDN,
                        filter,
                        SearchScope.Subtree,
                        "description", "number"
                    );

                    try
                    {

                        var response = (SearchResponse)ldapConnection.SendRequest(searchRequest);
                        foreach (SearchResultEntry entry in response.Entries)
                        {
                            var num = entry.Attributes["number"]?[0]?.ToString() ?? "-";
                            var des = entry.Attributes["description"]?[0]?.ToString() ?? "-";
                            ldapUsers.Add(new LdapUser() { Name = des, Phone = num });
                            if (ldapUsers.Count() >= limit)
                            {
                                return ldapUsers;
                            }
                        }
                    }
                    catch (DirectoryOperationException ex)
                    {
                        if (ex.Response is SearchResponse searchResponse)
                        {
                            foreach (SearchResultEntry entry in searchResponse.Entries)
                            {
                                var num = entry.Attributes["number"]?[0]?.ToString() ?? "-";
                                var des = entry.Attributes["description"]?[0]?.ToString() ?? "-";
                                ldapUsers.Add(new LdapUser() { Name = des, Phone = num });
                                if (ldapUsers.Count() >= limit)
                                {
                                    return ldapUsers;
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"❌ Другая ошибка: {ex.Message}");
                        }
                    }
                }

                Debug.WriteLine($"\n✅ Найдено записей: {descriptions.Count}");
            }
            
            return ldapUsers;
        } 
        public List<LdapUser> SearchLdaps(string baseDN, string search)
        {
            var ldapUsers = new List<LdapUser>();
            if (ldapConnection != null)
            {

                var filter = $"(|(number={search}*)(description={search}*))";
                var searchRequest = new SearchRequest(
                    baseDN,
                    filter,
                    SearchScope.Subtree,
                    "description", "number"
                );
                searchRequest.SizeLimit = 50;
                try
                {

                    var response = (SearchResponse)ldapConnection.SendRequest(searchRequest);
                    foreach (SearchResultEntry entry in response.Entries)
                    {
                        var num = entry.Attributes["number"]?[0]?.ToString() ?? "-";
                        var des = entry.Attributes["description"]?[0]?.ToString() ?? "-";
                        ldapUsers.Add(new LdapUser() { Name = des, Phone = num });
                    }
                }
                catch (DirectoryOperationException ex)
                {
                    if (ex.Response is SearchResponse searchResponse)
                    {
                        foreach (SearchResultEntry entry in searchResponse.Entries)
                        {
                            var num = entry.Attributes["number"]?[0]?.ToString() ?? "-";
                            var des = entry.Attributes["description"]?[0]?.ToString() ?? "-";
                            ldapUsers.Add(new LdapUser() { Name = des, Phone = num });
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"❌ Другая ошибка: {ex.Message}");
                    }
                }
            }
            return ldapUsers;
        }
    }
}
