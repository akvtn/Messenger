using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Collections;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Server
{
    public class DBHandler
    {
        private String ConnectionString;

        public DBHandler(string connectionStringName)
        {
            ConnectionString = String.Format(ConfigurationManager.AppSettings.Get(connectionStringName));
        }

        public List<List<string>> Execute(String sqlRequest)
        {
            List<List<string>> result = new List<List<string>>();

            MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();

            MySqlCommand command = new MySqlCommand(sqlRequest, connection);

            MySqlDataReader reader = command.ExecuteReader();
            if (reader != null)
            {
                while (reader.Read())
                {
                    List<string> temp = new List<string> { };
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        temp.Add(reader[i].ToString());
                    }
                    result.Add(temp);
                }
            }
            connection.Close();
            return result;
        }
}

    public class DBItem
    {
        public int Id { get; }
        protected DBHandler db = new DBHandler(connectionStringName: "ConnectionStringMySQL");

        public DBItem(int _id)
        {
            Id = _id;
        }
    }

    public class User : DBItem
    {
        public string Username
        {
            get { return db.Execute(String.Format("SELECT username FROM Users WHERE id = '{0}'",Id))[0][0]; }
        }

        public string Password
        {
            get { return db.Execute(String.Format("SELECT password FROM Users WHERE id = '{0}'", Id))[0][0]; }
        }

        public User(int _id) : base(_id) { }

        public List<Chat> GetChats()
        {
            List<Chat> result = new List<Chat>();
            List<List<string>> data = db.Execute(String.Format("SELECT chats.id FROM chats JOIN userchat ON userchat.chat_id = chats.id WHERE userchat.user_id = '{0}'", Id));
            foreach(List<string> row in data)
                    result.Add(new Chat(int.Parse(row[0])));
            return result;
        }

        public string SerializedChats()
        {
            List<Dictionary<string, string>> Chats = new List<Dictionary<string, string>> ();
            foreach (Chat chat in GetChats())
                Chats.Add(new Dictionary<string, string> { ["Id"] = chat.Id.ToString(), ["Title"] = chat.Title});
            return JsonConvert.SerializeObject(Chats);
        }

        public bool IsParticipant(Chat chat)
        {
            return db.Execute(String.Format("SELECT id FROM userchat WHERE user_id = '{0}' and chat_id = '{1}'", Id, chat.Id)).Any();
        }
    }

    public class Chat : DBItem
    {
        public string Title
        {
            get { return db.Execute(String.Format("SELECT title FROM chats WHERE id = '{0}'", Id))[0][0]; }
        }

        public Chat(int _id) : base(_id) { }

        public List<Message> GetMessages()
        {
            List<Message> result = new List<Message>();
            List<List<string>> data = db.Execute(String.Format("SELECT id FROM messages WHERE chat = '{0}' ORDER BY id ", Id));
            foreach (List<string> row in data)
                result.Add(new Message(int.Parse(row[0])));
            return result;
        }

        public string SerializedMessages()
        {
            List<Dictionary<string, string>> Messages = new List<Dictionary<string, string>>();
            foreach (Message message in GetMessages())
                Messages.Add(new Dictionary<string, string>
                {
                    ["Id"] = message.Id.ToString(),
                    ["Author"] = message.Author,
                    ["Content"] = message.Content
                });
            return JsonConvert.SerializeObject(Messages);
        }

        public void AddMessage(User user, string content)
        {
            db.Execute(String.Format("INSERT INTO messages (author,chat,content,date) VALUES ('{0}','{1}','{2}',now())", user.Id, Id, content));
        }
    }

    public class Message : DBItem
    {

        public string Content
        {
            get { return db.Execute(String.Format("SELECT content FROM messages WHERE id = '{0}'", Id))[0][0]; }
        }

        public string Author
        {
            get
            {
                string request = String.Format("SELECT username FROM users JOIN messages ON users.id = messages.author WHERE messages.id = '{0}'", Id);
                return db.Execute(request)[0][0]; 
            }
        }

        public Message(int _id) : base(_id) { }

    }
}
