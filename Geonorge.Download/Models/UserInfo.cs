using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Download.Models
{
    public class UserInfo
    {
        public string OrganizationNumber { get; set; }
        public List<string> _roles { get; set; }
        public string MunicipalityCode { get; set; }
    }
}