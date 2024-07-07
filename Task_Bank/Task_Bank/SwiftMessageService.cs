using Microsoft.Data.Sqlite;
using Task_Bank.Data;
using Dapper;

namespace Task_Bank
{
    public class SwiftMessageService
    {
        private readonly string _connectionString;           
        public SwiftMessageService(string connectionString)
        {
            _connectionString = connectionString;

        }
        public void SaveMessage(SwiftMessage message)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var sql = @"INSERT INTO SwiftMessages (Field_1, Field_2, Field_20, Field_21, Field_79, Mac, Chk) 
                        VALUES (@Field_1,  @Field_2, @Field_20, @Field_21, @Field_79, @Mac, @Chk)";

            connection.Execute(sql, message);
        }

        public void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var sql = @"
        CREATE TABLE IF NOT EXISTS SwiftMessages (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Field_1 TEXT,
            Field_2 TEXT,
            Field_20 TEXT NOT NULL,
            Field_21 TEXT NOT NULL,
            Field_79 TEXT NOT NULL,
            Mac TEXT,
            Chk TEXT
        )";

            connection.Execute(sql);
        }

        public void EnsureDatabaseSchema()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var tableInfo = connection.Query("PRAGMA table_info(SwiftMessages)").ToList();

            var requiredColumns = new List<string> { "Field_1", "Field_2", "Field_20", "Field_21", "Field_79", "Mac", "Chk" };
            var existingColumns = tableInfo.Select(row => (string)row.name).ToList();

            var missingColumns = requiredColumns.Except(existingColumns).ToList();

            foreach (var column in missingColumns)
            {
                var alterTableSql = $"ALTER TABLE SwiftMessages ADD COLUMN {column} TEXT";
                connection.Execute(alterTableSql);
            }
        }

        public async Task<string> ReadFileAsync(IFormFile file)
        {

            string content;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                content = await reader.ReadToEndAsync();
            }
           
            ProccesMessage(content);

            return content;
        }

        private void ProccesMessage(string content)
        {
            try
            {

                string field_1 = ExtractValue(content, "1:");
                string field_2 = ExtractValue(content, "2:");
                string field_20 = ExtractValue(content, ":20:");
                string field_21 = ExtractValue(content, ":21:");
                string field_79 = ExtractValue(content, ":79:");
                string Mac = ExtractValue(content, "MAC:");
                string Chk = ExtractValue(content, "CHK:");

                var swiftFields = new SwiftMessage
                {
                    Field_1 = field_1,
                    Field_2 = field_2,
                    Field_20 = field_20,
                    Field_21 = field_21,
                    Field_79 = field_79,
                    Mac = Mac,
                    Chk = Chk
                };
                SaveMessage(swiftFields);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while processing the message.", ex);
            }            
        }
        private string ExtractValue(string swiftMessageText, string fieldTag)
        {
            if (fieldTag == ":20:" || fieldTag == ":21:" || fieldTag == ":79:")
            {
                int startIndex = swiftMessageText.IndexOf(fieldTag);
                if (startIndex == -1)
                {
                    return null;
                }

                startIndex += fieldTag.Length;
                int endIndex = swiftMessageText.IndexOf("\n", startIndex);
                if (endIndex == -1)
                {
                    return swiftMessageText.Substring(startIndex).Trim();
                }

                return swiftMessageText.Substring(startIndex, endIndex - startIndex).Trim();
            }
            else
            {
                int startIndex = swiftMessageText.IndexOf(fieldTag);
                if (startIndex == -1)
                {
                    return null;
                }

                startIndex += fieldTag.Length;
                int endIndex = swiftMessageText.IndexOf("}", startIndex);
                if (endIndex == -1)
                {
                    return swiftMessageText.Substring(startIndex).Trim();
                }

                return swiftMessageText.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }
    }
}
