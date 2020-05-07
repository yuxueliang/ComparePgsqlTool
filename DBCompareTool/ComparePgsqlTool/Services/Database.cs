using Npgsql;
using System;

namespace ComparePgsqlTool.Services
{
    public class Database
    {

        public string Server { get; set; }
        public string Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Schema { get; set; } = "public";

        private string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string ConnectionString()
        {
            if (string.IsNullOrEmpty(_connectionString) == false) return _connectionString;

            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();
            builder.ConvertInfinityDateTime = true;
            builder.Database = Name;
            builder.Host = Server;
            builder.Password = Password;
            builder.Username = Login;
            builder.Port = Convert.ToInt32(Port);
            return builder.ConnectionString;
        }

    }
}
