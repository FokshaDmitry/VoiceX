using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace VoiceX.Services
{
    public class LocalStoreService
    {
        public LocalStoreService() { }
        public async Task SaveDataAsync(string key, string value)
        {
            using (var isoStore = IsolatedStorageFile.GetUserStoreForAssembly())
            using (var writer = new StreamWriter(new IsolatedStorageFileStream(key, FileMode.Create, isoStore)))
            {
                await writer.WriteAsync(value);
            }
        }
        public async Task<string> LoadDataAsync(string key)
        {
            using (var isoStore = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (!isoStore.FileExists(key)) return null;

                using (var reader = new StreamReader(new IsolatedStorageFileStream(key, FileMode.Open, isoStore)))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
        public void ClearIsolatedStorage()
        {
            try
            {
                IsolatedStorageFile.Remove(IsolatedStorageScope.User); 
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Clining error: {ex.Message}");
            }
        }
    }
}
