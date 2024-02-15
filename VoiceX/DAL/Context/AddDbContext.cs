using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoiceX.DAL.Entity;
using Windows.Storage;
using System.IO;
using VoiceX.Enums;
using Microsoft.Data.Sqlite;

namespace VoiceX.DAL.Context
{
    class AddDbContext 
    {
        public delegate void Update(HistoryNotes historyNote);
        public static event Update ChangeHystory;
        public AddDbContext()
        {
            InitializeDB();
        }
        public Delegate Uadete { get; set; }
        private async void InitializeDB()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("HistoryDB.db", CreationCollisionOption.OpenIfExists);
            string openPathDB = Path.Combine(ApplicationData.Current.LocalFolder.Path + "\\HistoryDB.db");
            using(SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
            {
                await connection.OpenAsync();
                string initCmd = "CREATE TABLE IF NOT EXISTS " +
                    "History(Id TEXT PRIMARY KEY, " +
                    "Name TEXT NOT NULL, " +
                    "Phone TEXT NOT NULL, " +
                    "StatusCall TEXT NOT NULL, " +
                    "StartDialog TEXT NOT NULL, " +
                    "EndDialog TEXT NOT NULL)";
                SqliteCommand command = new SqliteCommand(initCmd, connection);
                await command.ExecuteReaderAsync();
                connection.Close();
                connection.Dispose();
            }
            using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
            {
                await connection.OpenAsync().ConfigureAwait(true);
                string initCmd = "CREATE TABLE IF NOT EXISTS " +
                    "Loggin(Id TEXT PRIMARY KEY, " +
                    "Domain TEXT NOT NULL, " +
                    "Level TEXT NOT NULL, " +
                    "Message TEXT NOT NULL)";
                SqliteCommand command = new SqliteCommand(initCmd, connection);
                await command.ExecuteReaderAsync();
                connection.Close();
                connection.Dispose();
            }
            using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
            {
                await connection.OpenAsync();
                string initCmd = "CREATE TABLE IF NOT EXISTS " +
                    "Certificate(Id TEXT PRIMARY KEY)";
                SqliteCommand command = new SqliteCommand(initCmd, connection);
                await command.ExecuteReaderAsync();
                connection.Close();
                connection.Dispose();
            }
            using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
            {
                await connection.OpenAsync();
                string initCmd = "CREATE TABLE IF NOT EXISTS " +
                    "HotKeyUsers(Id TEXT PRIMARY KEY, " +
                    "Name TEXT NOT NULL, " +
                    "Phone TEXT NOT NULL)";
                SqliteCommand command = new SqliteCommand(initCmd, connection);
                await command.ExecuteReaderAsync();
                connection.Close();
                connection.Dispose();
            }
        }
        public async Task AddLogAsync(LogginNotes logginNotes)
        {
            if (logginNotes != null)
            {
                string openPathDB = Path.Combine(ApplicationData.Current.LocalFolder.Path + "\\HistoryDB.db");
                using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    await connection.OpenAsync().ConfigureAwait(true);
                    SqliteCommand sqliteCommand = new SqliteCommand
                    {
                        Connection = connection,
                        CommandText = "INSERT INTO Loggin VALUES (@Id, @Domain, @Level, @Message)"
                    };
                    sqliteCommand.Parameters.AddWithValue("@Id", logginNotes.Id.ToString());
                    sqliteCommand.Parameters.AddWithValue("@Domain", logginNotes.Domain);
                    sqliteCommand.Parameters.AddWithValue("@Level", logginNotes.Level);
                    sqliteCommand.Parameters.AddWithValue("@Message", logginNotes.Message);
                    await sqliteCommand.ExecuteReaderAsync();
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
        public async Task AddHotKeyUserAsync(HotKeyUser hotKeyUser)
        {
            if (hotKeyUser != null)
            {
                string openPathDB = Path.Combine(ApplicationData.Current.LocalFolder.Path + "\\HistoryDB.db");
                using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    await connection.OpenAsync().ConfigureAwait(true);
                    SqliteCommand sqliteCommand = new SqliteCommand
                    {
                        Connection = connection,
                        CommandText = "INSERT INTO HotKeyUsers VALUES (@Id, @Name, @Phone)"
                    };
                    sqliteCommand.Parameters.AddWithValue("@Id", hotKeyUser.Id.ToString());
                    sqliteCommand.Parameters.AddWithValue("@Name", hotKeyUser.Name);
                    sqliteCommand.Parameters.AddWithValue("@Phone", hotKeyUser.Phone);
                    await sqliteCommand.ExecuteReaderAsync();
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
        public async Task RemoveHotKeyUserAsync(Guid Id)
        {
            if (Id != null)
            {
                string openPathDB = Path.Combine(ApplicationData.Current.LocalFolder.Path + "\\HistoryDB.db");
                using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    await connection.OpenAsync().ConfigureAwait(true);
                    SqliteCommand sqliteCommand = new SqliteCommand
                    {
                        Connection = connection,
                        CommandText = " DELETE FROM HotKeyUsers WHERE Id = @Id"
                    };
                    sqliteCommand.Parameters.AddWithValue("@Id", Id.ToString());
                    await sqliteCommand.ExecuteReaderAsync();
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
        public async Task SaveCertificateAsunc(string b64P12)
        {
            if (!String.IsNullOrEmpty(b64P12))
            {
                string openPathDB = Path.Combine(ApplicationData.Current.LocalFolder.Path + "\\HistoryDB.db");
                using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    await connection.OpenAsync();
                    SqliteCommand sqliteCommand = new SqliteCommand
                    {
                        Connection = connection,
                        CommandText = "INSERT INTO Certificate VALUES (@Id)"
                    };
                    sqliteCommand.Parameters.AddWithValue("@Id", b64P12);
                    await sqliteCommand.ExecuteReaderAsync();
                    connection.Close();
                    connection.Dispose();

                }
            }
        }
        public void AddNote(HistoryNotes historyNotes)
        {
            if (historyNotes != null)
            {
                string openPathDB = Path.Combine(ApplicationData.Current.LocalFolder.Path + "\\HistoryDB.db");
                using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    connection.Open();
                    SqliteCommand sqliteCommand = new SqliteCommand
                    {
                        Connection = connection,
                        CommandText = "INSERT INTO History VALUES (@Id, @Name, @Phone, @StatusCall, @StartDialog, @EndDialog)"
                    };
                    sqliteCommand.Parameters.AddWithValue("@Id", historyNotes.Id.ToString());
                    sqliteCommand.Parameters.AddWithValue("@Name", historyNotes.Name);
                    sqliteCommand.Parameters.AddWithValue("@Phone", historyNotes.Phone);
                    sqliteCommand.Parameters.AddWithValue("@StartDialog", historyNotes.StartDialog);
                    sqliteCommand.Parameters.AddWithValue("@EndDialog", historyNotes.EndDialog);
                    sqliteCommand.Parameters.AddWithValue("@StatusCall", historyNotes.StatusCall.ToString());
                    sqliteCommand.ExecuteReader();
                    connection.Close();
                    connection.Dispose();
                    
                }
            }
        }
        public async Task AddNoteAcync(HistoryNotes historyNotes)
        {
            if (historyNotes != null)
            {
                string openPathDB = Path.Combine(ApplicationData.Current.LocalFolder.Path + "\\HistoryDB.db");
                using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    await connection.OpenAsync();
                    SqliteCommand sqliteCommand = new SqliteCommand
                    {
                        Connection = connection,
                        CommandText = "INSERT INTO History VALUES (@Id, @Name, @Phone, @StatusCall, @StartDialog, @EndDialog)"
                    };
                    sqliteCommand.Parameters.AddWithValue("@Id", historyNotes.Id.ToString());
                    sqliteCommand.Parameters.AddWithValue("@Name", historyNotes.Name);
                    sqliteCommand.Parameters.AddWithValue("@Phone", historyNotes.Phone);
                    sqliteCommand.Parameters.AddWithValue("@StartDialog", historyNotes.StartDialog);
                    sqliteCommand.Parameters.AddWithValue("@EndDialog", historyNotes.EndDialog);
                    sqliteCommand.Parameters.AddWithValue("@StatusCall", historyNotes.StatusCall.ToString());
                    await sqliteCommand.ExecuteReaderAsync();
                    connection.Close();
                    connection.Dispose();

                }
                ChangeHystory?.Invoke(historyNotes);
            }
        }
        public List<HistoryNotes> GetNotes()
        {
            List<HistoryNotes> historyNotes = new List<HistoryNotes>();
            string openPathDB = Path.Combine(ApplicationData.Current.LocalFolder.Path + "\\HistoryDB.db");
            try
            {
                using(SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    connection.Open();
                    string command = "SELECT * FROM History";
                    SqliteCommand sqliteCommand = new SqliteCommand(command, connection);
                    var reader = sqliteCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        historyNotes.Add(new HistoryNotes { Id = Guid.Parse(reader.GetString(0)), Name = reader.GetString(1), Phone = reader.GetString(2), StatusCall = reader.GetString(3) == "Outgoing" ? StatusCall.Outgoing : reader.GetString(3) == "Incoming" ? StatusCall.Incoming : StatusCall.Ignore, StartDialog = DateTime.Parse(reader.GetString(4)), EndDialog = DateTime.Parse(reader.GetString(5)) });
                    }
                    connection.Close();
                    connection.Dispose();
                }
            }
            catch
            {
                return historyNotes;
            }
            return historyNotes;
        }
        public List<HistoryNotes> GetNotes(int NumberItems)
        {
            List<HistoryNotes> historyNotes = new List<HistoryNotes>();
            string openPathDB = Path.Combine(ApplicationData.Current.LocalFolder.Path + "\\HistoryDB.db");
            try
            {
                using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    connection.Open();
                    string command = $"SELECT * FROM History ORDER BY datetime(StartDialog) DESC LIMIT {NumberItems};";
                    SqliteCommand sqliteCommand = new SqliteCommand(command, connection);
                    var reader = sqliteCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        historyNotes.Add(new HistoryNotes { Id = Guid.Parse(reader.GetString(0)), Name = reader.GetString(1), Phone = reader.GetString(2), StatusCall = reader.GetString(3) == "Outgoing" ? StatusCall.Outgoing : reader.GetString(3) == "Incoming" ? StatusCall.Incoming : StatusCall.Ignore, StartDialog = DateTime.Parse(reader.GetString(4)), EndDialog = DateTime.Parse(reader.GetString(5)) });
                    }
                    connection.Close();
                    connection.Dispose();
                }
            }
            catch
            {
                return historyNotes;
            }
            return historyNotes;
        }
        public List<HotKeyUser> GetHotKeyUsers()
        {
            List<HotKeyUser> hotKeyUsers = new List<HotKeyUser>();
            try
            {
                string openPathDB = Path.Combine(ApplicationData.Current.LocalFolder.Path + "\\HistoryDB.db");
                using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    connection.Open();
                    string command = "SELECT * FROM HotKeyUsers";
                    SqliteCommand sqliteCommand = new SqliteCommand(command, connection);
                    var reader = sqliteCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        hotKeyUsers.Add(new HotKeyUser { Id = Guid.Parse(reader.GetString(0)), Name = reader.GetString(1), Phone = reader.GetString(2)});
                    }
                    connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                return hotKeyUsers;
            }
            return hotKeyUsers;
        }
        public async Task DropDatabaseAsync()
        {
            string openPathDB = Path.Combine(ApplicationData.Current.LocalFolder.Path + "\\HistoryDB.db");
            using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
            {
                await connection.OpenAsync().ConfigureAwait(true);
                string command = "DROP TABLE IF EXISTS History";
                SqliteCommand sqliteCommand = new SqliteCommand(command, connection);
                await sqliteCommand.ExecuteReaderAsync().ConfigureAwait(false);
                connection.Close();
                connection.Dispose();
            }
            using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
            {
                await connection.OpenAsync().ConfigureAwait(true);
                string command = "DROP TABLE IF EXISTS Loggin";
                SqliteCommand sqliteCommand = new SqliteCommand(command, connection);
                await sqliteCommand.ExecuteReaderAsync().ConfigureAwait(true);
                connection.Close();
                connection.Dispose();
            }
            using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
            {
                await connection.OpenAsync().ConfigureAwait(true);
                string command = "DROP TABLE IF EXISTS HotKeyUsers";
                SqliteCommand sqliteCommand = new SqliteCommand(command, connection);
                await sqliteCommand.ExecuteReaderAsync().ConfigureAwait(true);
                connection.Close();
                connection.Dispose();
            }
        }
    }
}
