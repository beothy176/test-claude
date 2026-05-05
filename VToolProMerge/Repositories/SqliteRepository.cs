using Microsoft.Data.Sqlite;

namespace VToolProMerge.Repositories;

/// <summary>
/// Khung repository SQLite. Khi muốn lưu lịch sử/audit/log/placeholder vào DB
/// thì tạo bảng và mapping ở đây. Hiện tại để khung vì JSON đã đủ cho UI demo.
/// </summary>
public class SqliteRepository
{
    private readonly string _connectionString;

    public SqliteRepository(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public void EnsureSchema()
    {
        using var con = new SqliteConnection(_connectionString);
        con.Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Templates (
                Id TEXT PRIMARY KEY,
                Name TEXT,
                Category TEXT,
                Version TEXT,
                FilePath TEXT,
                UpdatedAt TEXT
            );
            CREATE TABLE IF NOT EXISTS Placeholders (
                Id TEXT PRIMARY KEY,
                FieldName TEXT,
                DisplayName TEXT,
                ""Group"" TEXT,
                DataType TEXT,
                Required INTEGER
            );
            CREATE TABLE IF NOT EXISTS AuditLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Time TEXT,
                Action TEXT,
                Status TEXT,
                Profile TEXT
            );";
        cmd.ExecuteNonQuery();
    }
}
