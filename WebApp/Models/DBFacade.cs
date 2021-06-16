using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

namespace WebApp.Models
{
    public class DBFacade : IDisposable
    {
        string connString = new NpgsqlConnectionStringBuilder()
        {
            Host = AppInfo.DbHost,
            Port = AppInfo.DbPort,
            Database = AppInfo.DbName,
            Username = AppInfo.DbLogin,
            Password = AppInfo.DbPassword,
        }.ConnectionString;
        NpgsqlConnection conn;

        public DBFacade()
        {
            conn = new NpgsqlConnection(connString);
        }

        ~DBFacade()
        {
            Dispose();
        }

        public void Dispose()
        {
            CloseConn();
        }

        // when conn != null
        private int AddCommonUser(string fio = "", string phone = "", string context = "")
        {
            OpenConn();
            var command = new NpgsqlCommand(
                    "INSERT INTO users (fio, phone_number,context) VALUES(@fio, @phone_number, @context)",
                    conn
                );
            command.Parameters.Add(new NpgsqlParameter("@fio", fio));
            command.Parameters.Add(new NpgsqlParameter("@phone_number", phone));
            command.Parameters.Add(new NpgsqlParameter("@context", context));
            return command.ExecuteNonQuery();   //returns new id
        }

        public int AddUser(Sources source, string username, long chatId, 
                            string fio = "", string phone = "", string context = "")
        {
                var command = new NpgsqlCommand(
                        "INSERT INTO @table VALUES(@id_user)",
                        conn    // доделать
                    );
                command.Parameters.Add(new NpgsqlParameter("@table", Dictionaries.sourcesDic[source] + "_users"));
                var test = AddCommonUser("test", "123", "{asd: asd}");
                return 0;
                return command.ExecuteNonQuery();
        }

        private void OpenConn()
        {
            if (conn != null && conn.State != System.Data.ConnectionState.Open) conn.Open();
        }
        private void CloseConn()
        {
            if (conn != null && conn.State == System.Data.ConnectionState.Open) conn.Close();
        }
    }
}
