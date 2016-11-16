using System;

namespace Kartverket.Geonorge.Download.Services
{
    public class AccessRestrictionException : Exception
    {
        public AccessRestrictionException(string message) : base(message)
        {
        }
    }
}