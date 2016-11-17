using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Geonorge.NedlastingApi.V2;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly DownloadContext _dbContext;

        public DownloadService(DownloadContext dbContext)
        {
            _dbContext = dbContext;
        }

        public HttpResponse CreateResponseFromRemoteFile(string url)
        {
            //Create a stream for the file
            Stream stream = null;

            //This controls how many bytes to read at a time and send to the client
            var bytesToRead = 10000;

            // Buffer to read bytes in chunk size specified above
            var buffer = new byte[bytesToRead];

            // The number of bytes read
            try
            {
                //Create a WebRequest to get the file
                var fileReq = (HttpWebRequest) WebRequest.Create(url);

                //Create a response for this request
                var fileResp = (HttpWebResponse) fileReq.GetResponse();

                if (fileReq.ContentLength > 0)
                    fileResp.ContentLength = fileReq.ContentLength;

                //Get the Stream returned from the response
                stream = fileResp.GetResponseStream();

                // prepare the response to the client. resp is the client Response
                var response = HttpContext.Current.Response;

                //Indicate the type of data being sent
                response.ContentType = "application/octet-stream";

                //Name the file 

                var fileName = url.Substring(url.LastIndexOf('/') + 1);

                response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
                response.AddHeader("Content-Length", fileResp.ContentLength.ToString());

                int length;
                do
                {
                    // Verify that the client is connected.
                    if (response.IsClientConnected)
                    {
                        // Read data into the buffer.
                        length = stream.Read(buffer, 0, bytesToRead);

                        // and write it out to the response's output stream
                        response.OutputStream.Write(buffer, 0, length);

                        // Flush the data
                        response.Flush();

                        //Clear the buffer
                        buffer = new byte[bytesToRead];
                    }
                    else
                    {
                        // cancel the download if client has disconnected
                        length = -1;
                    }
                } while (length > 0); //Repeat until no data is read

                return response;
            }
            finally
            {
                //Close the input stream
                stream?.Close();
            }
        }
    }
}
