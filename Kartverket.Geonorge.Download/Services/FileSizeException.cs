using System;

namespace Kartverket.Geonorge.Download.Services
{
    public class FileSizeException : Exception
    {
        public FileSizeException(string message) : base(message)
        {
        }
    }
}