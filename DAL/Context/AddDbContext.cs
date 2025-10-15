using VoiceX.DAL.Entity;
using System.IO;
using VoiceX.Enums;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace VoiceX.DAL.Context
{
    class AddDbContext 
    {
        public delegate void Update(HistoryNotes historyNote);
        public static event Update? ChangeHystory;
        public async void InitializeDB()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string openPathDB = Path.Combine(path + "\\HistoryDB.db");
            using(SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
            {
                try
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
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
            {
                await connection.OpenAsync().ConfigureAwait(true);

                // Имя таблицы
                string tableName = "Loggin";

                // Все нужные поля (ключ — имя поля, значение — SQL-определение)
                var requiredColumns = new Dictionary<string, string>
                {
                    { "Id", "TEXT PRIMARY KEY" },
                    { "Domain", "TEXT NOT NULL" },
                    { "Level", "INT NOT NULL" },
                    { "Created", "TEXT NOT NULL DEFAULT ''" },
                    { "Message", "TEXT NOT NULL" }
                };

                // Создаём таблицу, если она ещё не существует (с базовой структурой)
                string initCmd = $"CREATE TABLE IF NOT EXISTS {tableName} (" +
                                 string.Join(", ", requiredColumns.Select(kvp => $"{kvp.Key} {kvp.Value}")) + ")";
                var createCmd = new SqliteCommand(initCmd, connection);
                await createCmd.ExecuteNonQueryAsync();

                // Получаем текущие колонки в таблице
                var existingColumns = new HashSet<string>();
                string pragmaCmd = $"PRAGMA table_info({tableName});";
                var pragma = new SqliteCommand(pragmaCmd, connection);
                using (var reader = await pragma.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string columnName = reader.GetString(1); // index 1 = name
                        existingColumns.Add(columnName);
                    }
                }

                // Добавляем недостающие поля
                foreach (var kvp in requiredColumns)
                {
                    if (!existingColumns.Contains(kvp.Key))
                    {
                        string alterCmd = $"ALTER TABLE {tableName} ADD COLUMN {kvp.Key} {kvp.Value};";
                        var alter = new SqliteCommand(alterCmd, connection);
                        await alter.ExecuteNonQueryAsync();
                    }
                }

                connection.Close();
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
                var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
                string openPathDB = Path.Combine(path + "\\HistoryDB.db");
                using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    try
                    {
                        await connection.OpenAsync().ConfigureAwait(true);
                        SqliteCommand sqliteCommand = new SqliteCommand
                        {
                            Connection = connection,
                            CommandText = "INSERT INTO Loggin VALUES (@Id, @Domain, @Level, @Message, @Created)"
                        };
                        sqliteCommand.Parameters.AddWithValue("@Id", logginNotes.Id.ToString());
                        sqliteCommand.Parameters.AddWithValue("@Domain", logginNotes.Domain);
                        sqliteCommand.Parameters.AddWithValue("@Level", logginNotes.Level);
                        sqliteCommand.Parameters.AddWithValue("@Message", logginNotes.Message);
                        sqliteCommand.Parameters.AddWithValue("@Created", logginNotes.Created);
                        await sqliteCommand.ExecuteReaderAsync();
                        connection.Close();
                        connection.Dispose();
                    }
                    catch 
                    {

                    }
                }
            }
        }
        public async Task AddHotKeyUserAsync(HotKeyUser hotKeyUser)
        {
            if (hotKeyUser != null)
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
                string openPathDB = Path.Combine(path + "\\HistoryDB.db");
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
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
            string openPathDB = Path.Combine(path + "\\HistoryDB.db");
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
        public void AddNote(HistoryNotes historyNotes)
        {
            if (historyNotes != null)
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
                string openPathDB = Path.Combine(path + "\\HistoryDB.db");
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
                var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
                string openPathDB = Path.Combine(path + "\\HistoryDB.db");
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
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
            List<HistoryNotes> historyNotes = new List<HistoryNotes>();
            string openPathDB = Path.Combine(path + "\\HistoryDB.db");
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
                        historyNotes.Add(new HistoryNotes { Id = Guid.Parse(reader.GetString(0)), Name = reader.GetString(1), Phone = reader.GetString(2), StatusCall = reader.GetString(3) == "Outgoing" ? StatusCall.Outgoing : reader.GetString(3) == "Incoming" ? StatusCall.Incoming : reader.GetString(3) == "Ignore" ? StatusCall.Ignore : StatusCall.IncomeIgnore, StartDialog = DateTime.Parse(reader.GetString(4)), EndDialog = DateTime.Parse(reader.GetString(5)) });
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
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
            List<HistoryNotes> historyNotes = new List<HistoryNotes>();
            string openPathDB = Path.Combine(path + "\\HistoryDB.db");
            try
            {
                using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    connection.Open();
                    string command = $"SELECT * FROM History ORDER BY datetime(EndDialog) DESC LIMIT {NumberItems};";
                    SqliteCommand sqliteCommand = new SqliteCommand(command, connection);
                    var reader = sqliteCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        historyNotes.Add(new HistoryNotes { Id = Guid.Parse(reader.GetString(0)), Name = reader.GetString(1), Phone = reader.GetString(2), StatusCall = reader.GetString(3) == "Outgoing" ? StatusCall.Outgoing : reader.GetString(3) == "Incoming" ? StatusCall.Incoming : reader.GetString(3) == "Ignore" ? StatusCall.Ignore : StatusCall.IncomeIgnore, StartDialog = DateTime.Parse(reader.GetString(4)), EndDialog = DateTime.Parse(reader.GetString(5)) });
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
        public List<HistoryNotes> GetNotes(int NumberItems, StatusCall statusCall)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
            List<HistoryNotes> historyNotes = new List<HistoryNotes>();
            string openPathDB = Path.Combine(path + "\\HistoryDB.db");
            string filter = "";
            switch (statusCall)
            {
                case StatusCall.Outgoing:
                    filter = "'Outgoing'";
                    break;
                case StatusCall.Incoming:
                    filter = "'Incoming'";
                    break;
                case StatusCall.Ignore:
                    filter = "'IncomeIgnore', 'Ignore'";
                    break;
                case StatusCall.IncomeIgnore:
                    filter = "'IncomeIgnore', 'Ignore'";
                    break;
                default:
                    break;
            }
            try
            {
                using (SqliteConnection connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWriteCreate;"))
                {
                    connection.Open();
                    string command = $"SELECT * FROM History WHERE StatusCall IN ({filter}) ORDER BY datetime(EndDialog) DESC LIMIT {NumberItems};";
                    SqliteCommand sqliteCommand = new SqliteCommand(command, connection);
                    var reader = sqliteCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        historyNotes.Add(new HistoryNotes { Id = Guid.Parse(reader.GetString(0)), Name = reader.GetString(1), Phone = reader.GetString(2), StatusCall = reader.GetString(3) == "Outgoing" ? StatusCall.Outgoing : reader.GetString(3) == "Incoming" ? StatusCall.Incoming : reader.GetString(3) == "Ignore" ? StatusCall.Ignore : StatusCall.IncomeIgnore, StartDialog = DateTime.Parse(reader.GetString(4)), EndDialog = DateTime.Parse(reader.GetString(5)) });
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
                var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
                string openPathDB = Path.Combine(path + "\\HistoryDB.db");
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
            catch
            {
                return hotKeyUsers;
            }
            return hotKeyUsers;
        }
        public async Task DeleteOldLogsAsync()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
            string openPathDB = Path.Combine(path + "\\HistoryDB.db");
            using (var connection = new SqliteConnection($@"Data Source={openPathDB};Cache=Shared;Mode=ReadWrite"))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                // Предполагаем, что поле Created содержит дату в формате "yyyy-MM-dd"
                string today = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                // Удаляем все записи, которые не соответствуют сегодняшней дате
                string deleteCmd = "DELETE FROM Loggin WHERE date(Created) != @today;";
                var command = new SqliteCommand(deleteCmd, connection);
                command.Parameters.AddWithValue("@today", today);
                int affectedRows = await command.ExecuteNonQueryAsync();

                Console.WriteLine($"Удалено {affectedRows} записей старше {today}");

                connection.Close();
            }
        }
        public async Task DropDatabaseAsync()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Database";
            string openPathDB = Path.Combine(path + "\\HistoryDB.db");
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
