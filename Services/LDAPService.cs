using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VoiceX.Models;
using SearchScope = System.DirectoryServices.Protocols.SearchScope;

namespace VoiceX.Services
{
    public class LDAPService
    {
        LdapConnection? ldapConnection;
        public LDAPService() 
        {
            
        }
        public async Task Authenticate(string distinguishedName, string password, string server)
        {
            await Task.Run(() =>
            {
                if (!String.IsNullOrEmpty(distinguishedName) && !String.IsNullOrEmpty(password) && !String.IsNullOrEmpty(server))
                {
                    try
                    {
                        // Подключение
                        var identifier = new LdapDirectoryIdentifier(server, 389);

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
            });
        }
        public async Task<List<LdapUser>> GetLdapUsersAsync(int limit, string baseDN)
        {
            return await Task.Run(() =>
            {
                var ldapUsers = new List<LdapUser>();

                if (ldapConnection != null)
                {
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
                                if (ldapUsers.Count >= limit)
                                {
                                    return ldapUsers.OrderByDescending(l => l.Name).ToList();
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
                                    if (ldapUsers.Count >= limit)
                                    {
                                        return ldapUsers.OrderByDescending(l => l.Name).ToList();
                                    }
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"❌ Другая ошибка: {ex.Message}");
                            }
                        }
                    }
                }

                return ldapUsers.OrderByDescending(l => l.Name).ToList();
            });
        }
        public List<LdapUser> GetLdapUsers(int limit, string baseDN)
        {
            var ldapUsers = new List<LdapUser>();
            if (ldapConnection != null)
            {
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
                                return ldapUsers.OrderByDescending(l => l.Name).ToList();
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
                                    return ldapUsers.OrderByDescending(l => l.Name).ToList();
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"❌ Другая ошибка: {ex.Message}");
                        }
                    }
                }
            }
            
            return ldapUsers.OrderByDescending(l => l.Name).ToList();
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
