using System;

namespace Geonorge.Download.Services.Exceptions
{
    public class AccessRestrictionException : Exception
    {
        public AccessRestrictionException(string message) : base(message)
        {
        }
    }
}