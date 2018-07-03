import json
from socket import AF_INET, socket, SOCK_STREAM
from threading import Thread


class AuthorizationException(Exception):
    pass


class RegistrationException(Exception):
    pass


class Connection:
    _SOCKET = socket(AF_INET, SOCK_STREAM)
    _BUFFER_SIZE = 1024

    def __init__(self, _host, _port):
        self._host = _host
        self._port = _port
        self._state = "Authorization"
        self._SOCKET.connect((self._host, self._port))

    def request(self, **data):
        self.send(**data)
        return self.receive()

    def send(self, **data):
        self._SOCKET.send(bytes(json.dumps(data), "utf8"))

    def receive(self):
        return json.loads(self._SOCKET.recv(self._BUFFER_SIZE).decode("utf8"))

    def authorize(self, _username, _password):
        response = self.request(Type="Authorize", Data={"username": _username, "password": _password})
        if response["Status"] == "Successfully authorized":
            return response
        else:
            raise AuthorizationException("Invalid username or password")

    def register(self, _username, _password):
        response = self.request(Type="Register", Data={"username": _username, "password": _password})
        if response["Status"] == "Registration Error":
            raise RegistrationException()


host = input("Enter host - ")
port = int(input("Enter port - "))

client = Connection(host, port)

while True:
    print("----------------------")
    print("1 - Registration - ")
    print("2 - Authorization - ")
    print("----------------------")
    choice = int(input("Enter choice(1,2) - "))
    print("----------------------")
    username = input("Enter username - ")
    password = input("Enter password - ")
    if choice == 1:
        client.request(Type="Register", Data={"username": username,
                                              "password": password})
    elif choice == 2:
        auth_response = client.request(Type="Authorize", Data={"username": username,
                                                               "password": password})
        if auth_response["Status"] == "Successfully authorized":
            break


def receive():
    while True:
        data = client.receive()
        if data["Status"] == "Successfully closed":
            return
        if data["Status"] == "New message":
            if data["Data"]["Type"] == "Text":
                print("{0} - {1}".format(data["Data"]["Author"], data["Data"]["Message"]))


while True:
    print("----------------------")
    for chat in json.loads(auth_response["Data"]["Chats"]):
        print("{0}: {1}".format(chat["Id"], chat["Title"]))
    print("----------------------")
    chat_id = int(input("Enter chat id - "))
    open_chat_response = client.request(Type="Open Chat", Data={"Id": chat_id})
    print("----------------------")
    for message in json.loads(open_chat_response["Data"]["Messages"]):
        print("{0}: {1}".format(message["Author"], message["Content"]))
    print("----------------------")
    Thread(target=receive).start()
    while True:
        message = input("Enter new message - ")
        if message == "?close?":
            client.send(Type="Close Chat")
            break
        client.send(Type="Send Message", Data={"Type": "Text", "Message": message})
