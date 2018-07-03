using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Server
{
    enum ResponseErrorType
    {
        InvalidChatError,
        DatabaseAccessError,
        InvalidUsernamePasswordError,
        UsedUsernameError,
    }

    static class ResponseError
    {
        static private Dictionary<ResponseErrorType, Tuple<string, string>> Mapper = new Dictionary<ResponseErrorType, Tuple<string, string>>
        {
            [ResponseErrorType.InvalidChatError] = Tuple.Create("InvalidChat", "User is not a participant of this chat"),
            [ResponseErrorType.DatabaseAccessError] = Tuple.Create("DatabaseAccessError", "Cannot get access to database"),
            [ResponseErrorType.InvalidUsernamePasswordError] = Tuple.Create("AuthorizationError", "Invalid username or password"),
            [ResponseErrorType.UsedUsernameError] = Tuple.Create("RegistrationError", "Already used username")
        };

        static public Response DefineError(Request request, ResponseErrorType type)
        {
            return new Response(status: ResponseStatus.Error, onRequest: request.Type, data: new Dictionary<string, string>
            {
                ["Error Type"] = Mapper[type].Item1,
                ["Error Message"] = Mapper[type].Item2
            });
        }
    }


    interface IRequestHandler
    {
        Response Handle(Request request);
    }

    static class RequestHandlerMapper
    {
        public static Dictionary<RequestType,RequestHandler> handlers = new Dictionary<RequestType,RequestHandler>()
        {
            [RequestType.Authorize]       = new AuthorizeHandler(),
            [RequestType.Register]        = new RegisterHanlder(),
            [RequestType.GetChats]        = new GetChatsHandler(),
            [RequestType.SetChat]         = new SetChatHandler(),
            [RequestType.UnsetChat]       = new UnsetChatHandler(),
            [RequestType.SendMessage]     = new SendMessageHandler(),
            [RequestType.GetMessages]     = new GetMessagesHandler(),
            [RequestType.CreateChat]      = new CreateChatHandler(),
            [RequestType.AddToChat]       = new AddToChatHandler(),
            [RequestType.RemoveFromChat]  = new RemoveFromChatHandler(),
            [RequestType.Disconnect]      = new DisconnectHandler()
        };

        public static IRequestHandler DefineHandler(RequestType RequestType)
        {
            return handlers[RequestType];
        }
    }

    abstract class RequestHandler : IRequestHandler
    {
        protected DBHandler db = new DBHandler(connectionStringName: "ConnectionStringMySQL");
        public RequestType Type { get; protected set; }
        protected bool UserIsInChat(Chat chat)
        { 
            return ClientStatus.GetInstance(Thread.CurrentThread.ManagedThreadId.ToString()).User.GetChats().Find(_chat => _chat.Id == chat.Id) != null;
        }

        public virtual Response Handle(Request request) { return new Response(ResponseStatus.Error, request.Type); }
    }

    class AuthorizeHandler : RequestHandler
    {
        public override Response Handle(Request request)
        {
            
            ClientStatus status = ClientStatus.GetInstance(Thread.CurrentThread.ManagedThreadId.ToString());
            try
            {
                List<List<string>> data = db.Execute(String.Format("SELECT id FROM users WHERE username = '{0}' and password = '{1}'", request.Data["username"], request.Data["password"]));
                if (data.Count != 0)
                {
                    status.User = new User(int.Parse(data[0][0]));
                    Dictionary<string, string> ResponseData = new Dictionary<string, string>
                    {
                        ["Username"] = status.User.Username
                    };
                    return new Response(status: ResponseStatus.Success, onRequest: request.Type, data: ResponseData);
                }
                else
                {
                    return ResponseError.DefineError(request, ResponseErrorType.InvalidUsernamePasswordError);
                }
            } catch (MySqlException)
            {
                return ResponseError.DefineError(request,ResponseErrorType.DatabaseAccessError);
            }
        }
    }

    class RegisterHanlder : RequestHandler
    {
        public override Response Handle(Request request)
        {
            try
            {

                List<List<string>> ExistingUser = db.Execute(String.Format("SELECT username from users WHERE username = '{0}'", request.Data["username"]));
                if (ExistingUser.Count != 0)
                {
                    return ResponseError.DefineError(request,ResponseErrorType.UsedUsernameError);
                }

                db.Execute(String.Format("INSERT INTO users (username,password) VALUES ('{0}','{1}')", request.Data["username"], request.Data["password"]));
                return new Response(status: ResponseStatus.Success, onRequest: request.Type);
            }
            catch (MySqlException)
            {
                return ResponseError.DefineError(request, ResponseErrorType.DatabaseAccessError);
            }
        }
    }

    class GetChatsHandler : RequestHandler
    {
        public override Response Handle(Request request)
        {
            ClientStatus status = ClientStatus.GetInstance(Thread.CurrentThread.ManagedThreadId.ToString());
            return new Response(status: ResponseStatus.Success, onRequest: request.Type, data: new Dictionary<string, string>
            {
                ["Chats"] = status.User.SerializedChats()
            });
        }
    }

    class SetChatHandler : RequestHandler
    {
        public override Response Handle(Request request)
        {
            ClientStatus status = ClientStatus.GetInstance(Thread.CurrentThread.ManagedThreadId.ToString());
            Chat chat = new Chat(int.Parse(request.Data["NewChatId"]));
            if (!UserIsInChat(chat)) return ResponseError.DefineError(request, ResponseErrorType.InvalidChatError); 
            else
            {
                status.Chat = chat;
                return new Response(ResponseStatus.Success, onRequest: request.Type);
            }
        }
    }

    class UnsetChatHandler : RequestHandler
    {
        public override Response Handle(Request request)
        {
            ClientStatus status = ClientStatus.GetInstance(Thread.CurrentThread.ManagedThreadId.ToString());
            status.Chat = null;
            return new Response(ResponseStatus.Success, onRequest: request.Type);
        }
    }

    class SendMessageHandler : RequestHandler
    {
        public override Response Handle(Request request)
        {
            Chat Chat = new Chat(int.Parse(request.Data["Chat"]));
            ClientStatus status = ClientStatus.GetInstance(Thread.CurrentThread.ManagedThreadId.ToString());
            if (!UserIsInChat(Chat)) return ResponseError.DefineError(request, ResponseErrorType.InvalidChatError);
            else
            {
                Dictionary<string, string> ResponseData = new Dictionary<string, string>
                {
                    ["Author"] = status.User.Username,
                    ["Message"] = request.Data["Message"]
                };

                foreach (KeyValuePair<SocketHandler, ClientStatus> ClientData in Server.Clients)
                {
                    if (ClientData.Value.User.IsParticipant(Chat) && ClientData.Value.Chat != null && ClientData.Value.Chat.Id == Chat.Id)
                    {
                        Console.WriteLine("SENDED TO {0}", ClientData.Value.User.Username);
                        ClientData.Key.Send(new Response(status: ResponseStatus.Success, onRequest: RequestType.SpreadMessage, data: ResponseData));
                    }
                }
                try
                {
                    Chat.AddMessage(status.User, request.Data["Message"]);
                }
                catch (MySqlException)
                {
                    return ResponseError.DefineError(request, ResponseErrorType.DatabaseAccessError);
                }
                return new Response(ResponseStatus.Success, onRequest: request.Type);
            }
        }
    }

    class GetMessagesHandler : RequestHandler
    {
        public override Response Handle(Request request)
        {
            Chat Chat = new Chat(int.Parse(request.Data["ChatId"]));

            if (!UserIsInChat(Chat)) return ResponseError.DefineError(request, ResponseErrorType.InvalidChatError);
            else
            {
                try
                {
                    Dictionary<string, string> ResponseData = new Dictionary<string, string>
                    {
                        ["Messages"] = Chat.SerializedMessages()
                    };

                    return new Response(status: ResponseStatus.Success, onRequest: request.Type, data: ResponseData);
                }
                catch (MySqlException)
                {
                    return ResponseError.DefineError(request, ResponseErrorType.DatabaseAccessError);
                }
            };
        }
    }

    class CreateChatHandler : RequestHandler
    {
        public override Response Handle(Request request)
        {
            try
            {
                string user_id = ClientStatus.GetInstance(Thread.CurrentThread.ManagedThreadId.ToString()).User.Id.ToString();
                db.Execute(String.Format("INSERT INTO chats (title, creator,creation_date) VALUES ('{0}','{1}',now())", request.Data["Title"], user_id));
                List<List<string>> chat = db.Execute(String.Format("SELECT id, title FROM chats WHERE title = '{0}' and creator = '{1}' ORDER BY id DESC LIMIT 1",
                                                           request.Data["Title"],
                                                           user_id));
                db.Execute(String.Format("INSERT INTO userchat (user_id,chat_id) VALUES ('{0}','{1}')", user_id, chat[0][0]));
                return new Response(status: ResponseStatus.Success, onRequest: request.Type, data: new Dictionary<string, string>
                {
                    ["Chat"] = JsonConvert.SerializeObject(chat[0])
                });
            }
            catch (MySqlException)
            {
                return ResponseError.DefineError(request, ResponseErrorType.DatabaseAccessError);
            }
        }
    }

    class AddToChatHandler : RequestHandler
    {
        public override Response Handle(Request request)
        {
            Chat Chat = new Chat(int.Parse(request.Data["ChatId"]));
            if (!UserIsInChat(Chat)) return ResponseError.DefineError(request, ResponseErrorType.InvalidChatError);
            else
            {
                try
                {
                    db.Execute(String.Format("INSERT INTO userchat (user_id, chat_id) VALUES ({0},{1})", request.Data["UserId"], request.Data["ChatId"]));
                }
                catch (MySqlException)
                {
                    return ResponseError.DefineError(request, ResponseErrorType.DatabaseAccessError);
                }         
                return new Response(ResponseStatus.Success, onRequest: request.Type);
            }
        }
    }

    class RemoveFromChatHandler : RequestHandler
    {
        public override Response Handle(Request request)
        {
            Chat Chat = new Chat(int.Parse(request.Data["ChatId"]));
            if (!UserIsInChat(Chat)) return ResponseError.DefineError(request, ResponseErrorType.InvalidChatError);
            else
            {
                try
                {
                    db.Execute(String.Format("DELETE INTO userchat WHERE user_id = '{0}' and chat_id = '{1}'", request.Data["UserId"], request.Data["ChatId"]));
                }
                catch (MySqlException)
                {
                    return ResponseError.DefineError(request, ResponseErrorType.DatabaseAccessError);
                }
                return new Response(ResponseStatus.Success, onRequest: request.Type);
            }
        }
    }

    class DisconnectHandler : RequestHandler
    {
        public override Response Handle(Request request)
        {
            return new Response(status: ResponseStatus.Success, onRequest: request.Type);
        }
    }
}
