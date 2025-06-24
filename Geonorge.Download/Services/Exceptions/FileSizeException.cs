using System;

namespace Geonorge.Download.Services.Exceptions
{
    public class FileSizeException : Exception
    {
        public FileSizeException(string message) : base(message)
        {
        }
    }
}