using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IFileService
    {
        Dataset GetDataset(string id);
        
    }
    public class FileService : IFileService
    {



        public Dataset GetDataset(string id)
        {
            return null;
        }

    }
}