using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace VoiceX.Services
{
    public class CertificateService
    {
        public CertificateService() { }
        public X509Certificate2 GetCertificateByFriendlyName(string friendlyName)
        {
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates.Cast<X509Certificate2>().FirstOrDefault(cert => cert.FriendlyName == friendlyName)!;
            }
        }
        public void SaveCertificate(string base64Cert, string base64Key, string friendlyName, string password)
        {
            try
            {
                byte[] pfxBytes = Convert.FromBase64String(base64Cert);

                // **2. Создаем объект сертификата**
                X509Certificate2 cert = new X509Certificate2(
                    pfxBytes,
                    password, // Пароль, если он есть
                    X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                // **3. Устанавливаем FriendlyName**
                cert.FriendlyName = friendlyName;

                // **4. Открываем хранилище и добавляем сертификат**
                using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                    Console.WriteLine($"✅ Сертификат {friendlyName} успешно добавлен в хранилище.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка импорта сертификата: {ex.Message}");
            }
        }
        public bool CheckCertificate(string friendlyName)
        {
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates.Select(c => c.FriendlyName).Contains(friendlyName);
            }
        }
        public void SaveCertificate(string path, string password,string friendlyName)
        {
            try
            {
                // Загружаем сертификат из файла
                X509Certificate2 cert = new X509Certificate2(path, password, X509KeyStorageFlags.PersistKeySet);
                cert.FriendlyName = friendlyName;
                // Открываем хранилище и добавляем сертификат
                using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении сертификата: {ex.Message}");
            }
        }
    }
}
