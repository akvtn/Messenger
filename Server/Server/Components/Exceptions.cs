using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Exceptions
    {
        class PositiveNumberExeption : Exception
        {
            PositiveNumberExeption(string message) : base(message) { }
        }

        class NotAvailableValueException : Exception
        {
            NotAvailableValueException(string message) : base(message) { }
        }

        class InvalidOperationExeption : Exception
        {
            InvalidOperationExeption(string message) : base(message) { }
        }

    }
}
