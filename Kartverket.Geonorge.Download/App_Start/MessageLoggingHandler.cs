﻿using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Kartverket.Geonorge.Download
{
    public class MessageLoggingHandler : MessageHandler
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override async Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() =>
            {
                string logMessage = $"[{correlationId}] Request: {requestInfo}";

                if (message != null)
                {
                    string messageAsString = Encoding.UTF8.GetString(message);
                    if (!string.IsNullOrWhiteSpace(messageAsString))
                        logMessage = logMessage + "\r\n" + messageAsString;
                }
                
                Log.Info(logMessage);
            });
        }


        protected override async Task OutgoingMessageAsync(string correlationId, string requestInfo, HttpStatusCode responseStatusCode, byte[] message)
        {
            await Task.Run(() =>
            {
                var logMessage = $"[{correlationId}] Response: {requestInfo} {(int)responseStatusCode} {responseStatusCode}";
                if (message != null)
                    logMessage = logMessage + "\r\n" + Encoding.UTF8.GetString(message);

                if ((int)responseStatusCode >= 200 && (int) responseStatusCode < 400)
                    Log.Debug(logMessage);
                else
                    Log.Error(logMessage);
            });
        }
    }

    public abstract class MessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var corrId = $"{DateTime.Now.Ticks}{Thread.CurrentThread.ManagedThreadId}";
            var requestInfo = $"{request.Method} {request.RequestUri}";

            var requestMessage = await request.Content.ReadAsByteArrayAsync();

            await IncommingMessageAsync(corrId, requestInfo, requestMessage);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            byte[] responseMessage = null;

            if (response.IsSuccessStatusCode)
            {
                if (response.Content != null)
                    responseMessage = await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);
            }

            await OutgoingMessageAsync(corrId, requestInfo, response.StatusCode, responseMessage);

            return response;
        }


        protected abstract Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message);
        protected abstract Task OutgoingMessageAsync(string correlationId, string requestInfo, HttpStatusCode responseStatusCode, byte[] message);
    }
}