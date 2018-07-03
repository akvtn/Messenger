using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class AuthorizationException : Exception
    {
        public AuthorizationException(string message) : base(message) { }

        public AuthorizationException() { }
    }

    class RegistraionException : Exception
    {
        public RegistraionException(string message) : base(message) { }

        public RegistraionException() { }
    }

    class InvalidChatException : Exception
    {
        public InvalidChatException(string message) : base(message) { }

        public InvalidChatException() { }
    }

    class MultipleReadThreadsExecption : Exception
    {
        public MultipleReadThreadsExecption(string message) : base(message) { }

        public MultipleReadThreadsExecption() { }
    }

    class DatabaseAccessException : Exception
    {
        public DatabaseAccessException() { }

        public DatabaseAccessException(string message) : base(message) { }
    }
}
