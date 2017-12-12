﻿using System;
using System.Configuration;

namespace Kartverket.Geonorge.Download.Models
{
    public class DownloadUrlBuilder
    {
        private string _orderUuid;
        private string _fileId;

        public DownloadUrlBuilder OrderId(Guid orderUuid)
        {
            _orderUuid = orderUuid.ToString();
            return this;
        }

        public DownloadUrlBuilder FileId(Guid fileId)
        {
            _fileId = fileId.ToString();
            return this;
        }

        public string Build()
        {
            return BaseUrl() + $"{_fileId}";
        }

        public string AsBundle()
        {
            return BaseUrl() + "bundle";
        }

        private string BaseUrl()
        {
            string server = ConfigurationManager.AppSettings["DownloadUrl"];
            return $"{server}api/download/order/{_orderUuid}/";
        }
    }
}