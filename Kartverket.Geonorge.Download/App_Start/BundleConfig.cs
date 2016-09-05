using System.Web;
using System.Web.Optimization;

namespace Kartverket.Geonorge.Download
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/bower_components/kartverket-felleskomponenter/assets/css/styles").Include(
               "~/Content/bower_components/kartverket-felleskomponenter/assets/css/vendor.min.css",
               "~/Content/bower_components/kartverket-felleskomponenter/assets/css/vendorfonts.min.css",
               "~/Content/bower_components/kartverket-felleskomponenter/assets/css/main.min.css"
               ));

            bundles.Add(new ScriptBundle("~/Content/bower_components/kartverket-felleskomponenter/assets/js/scripts").Include(
               "~/Content/bower_components/kartverket-felleskomponenter/assets/js/vendor.min.js",
               "~/Content/bower_components/kartverket-felleskomponenter/assets/js/main.min.js"
               ));
        }
    }
}
