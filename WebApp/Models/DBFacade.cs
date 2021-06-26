using System;
using System.Collections.Generic;
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
            conn.Open();
        }

        ~DBFacade()
        {
            Dispose();
        }

        public void Dispose()
        {
            CloseConn();
        }

        private string GetTableName(Sources s)
        {
            return DBDictionaries.sourcesDic[s] + "_users";
        }

        private void CreateParameter(NpgsqlCommand comm, string name, string value)
        {
            if (value != null) comm.Parameters.Add(new NpgsqlParameter(name, value));
            else comm.Parameters.Add(new NpgsqlParameter(name, DBNull.Value));
        }

        private int AddCommonUser(string fio, string phone = "", string context = "", string token = "")
        {
            var command = new NpgsqlCommand(
                    "INSERT INTO users (fio, phone_number, context, token) " +
                    "VALUES(@fio, @phone_number, @context, @token) RETURNING id",
                    conn
                );
            CreateParameter(command, "@fio", fio);
            CreateParameter(command, "@phone_number", phone);
            CreateParameter(command, "@context", context);
            CreateParameter(command, "@token", token);
            return (int)command.ExecuteScalar();   //returns new id
        }

        public DBUser AddUser(Sources source, string username, long chatId,
                            string fio = "", string phone = "", string context = "")
        {
            var command = new NpgsqlCommand(
                    $"INSERT INTO {GetTableName(source)} (id_user, username, chat_id) " +
                    $"VALUES({AddCommonUser(fio, phone, context)}, @username, {chatId})",
                    conn
                );
            CreateParameter(command, "@username", username);
            command.ExecuteNonQuery();

            return GetUser(source, chatId);

        }

        public Tuple<string, string>[] GetNeededEntities(string v)
        {
            var list = new List<Tuple<string, string>>();
            var command = new NpgsqlCommand(
                     $"SELECT e.name, question " +
                     $"FROM entities AS e JOIN intents_entities AS ie ON e.id = ie.id_entity JOIN intents AS i ON ie.id_intent = i.id " +
                     $"WHERE i.name = @intent;",
                     conn
                 );
            CreateParameter(command, "@intent", v);
            using (var reader = command.ExecuteReader())
            {
                while(reader.Read())
                {
                    list.Add(new Tuple<string, string>((string)reader[0], (string)reader[1]));
                }
            }
            return list.ToArray();
        }

        public DBUser GetUser(Sources source, long chatId)
        {
            var command = new NpgsqlCommand(
                    $"SELECT u.id, u.fio, u.phone_number, u.context, u.token, uu.username, uu.chat_id, uu.id " +
                    $"FROM users AS u join {DBDictionaries.sourcesDic[source] + "_users"} AS uu ON u.id = uu.id_user WHERE chat_id = '{chatId}';",
                    conn
                );
            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                if (reader.HasRows)
                {
                    return new DBUser((int)reader[0], (string)reader[1], (string)reader[2], DBContextHandler.StringToContext((string)reader[3]),
                      reader[4] == DBNull.Value ? "" : (string)reader[4], (string)reader[5], (int)reader[6], source, (int)reader[7]);
                }
            }
            return null;
        }

        public Tuple<string, string> GetEntityData(string value)
        {
            var command = new NpgsqlCommand(
                   $"SELECT format, question " +
                   $"FROM entities WHERE name = @name;",
                   conn
               );
            CreateParameter(command, "@name", value);
            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                if (reader.HasRows)
                {
                    return new Tuple<string, string>((string)reader[0], (string)reader[1]);
                }
            }
            return null;
        }

        public DBUser GetOrCreateUser(Sources source, long chatId, string username, string fio, string phone = "", string context = "")
        {
            var user = GetUser(source, chatId);
            if (user != null) return user;
            return AddUser(source, username, chatId, fio, phone, context);
        }

        public void SetContext(int idUser, string context)
        {
            var command = new NpgsqlCommand(
                   $"UPDATE users " +
                   $"SET context = @context " +
                   $"WHERE id = {idUser};",
                   conn
               ); ;
            CreateParameter(command, "@context", context);
            command.ExecuteNonQuery();
        }

        public void SetUser(DBUser user)
        {
            var command = new NpgsqlCommand(
                   $"UPDATE users " +
                   $"SET fio = @fio, phone_number = @phone, context = @context, token = @token " +
                   $"WHERE id = {user.Id};",
                   conn
               ); ;
            CreateParameter(command, "@fio", user.Fio);
            CreateParameter(command, "@phone", user.Phone);
            CreateParameter(command, "@context", user.ContextString);
            CreateParameter(command, "@token", user.Token);
            command.ExecuteNonQuery();
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
