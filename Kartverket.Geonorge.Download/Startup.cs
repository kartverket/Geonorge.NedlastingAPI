using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Kartverket.Geonorge.Download.Startup))]

namespace Kartverket.Geonorge.Download
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        
        }
    }
}
