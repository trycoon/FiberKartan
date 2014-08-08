using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Web.Optimization;
using FiberKartan.Resources;

namespace FiberKartan.Resources
{
    public class BundleConfig
    {
        public static void RegisterHandlBarBundles(BundleCollection bundles, string path)
        {
            var transform = new HandleBarBundleTransform();
            transform.jsPath = path;
            bundles.Add(new Bundle("~/inc/views", transform).IncludeDirectory("~/Resources/views", "*.hbs", true));
        }

        public static void RegisterBundles(BundleCollection bundles)
        {
            var jsTransform = new JavascriptBundleTransform();
            var cssTransform = new CssBundleTransform();
            // http://www.codeproject.com/Articles/748849/ASP-NET-Web-Optimization-Framework

            bundles.Add(new Bundle("~/inc/css/adminCss", cssTransform).Include(
                 "~/inc/css/base.css",
                "~/inc/css/jquery-ui.min.css",
                "~/inc/css/map.css",
                "~/inc/css/jquery.contextMenu.css"
                ).ForceOrdered());

            bundles.Add(new Bundle("~/inc/css/userCss", cssTransform).Include(
                "~/inc/css/base.css",
                "~/inc/css/jquery-ui.min.css",
                "~/inc/css/map.css"
               ).ForceOrdered());

            bundles.Add(new Bundle("~/inc/js/adminJs", jsTransform).Include(
               "~/inc/js/jquery.min.js",
               "~/inc/js/jquery-ui.min.js",
               "~/inc/js/jquery.ui.touch-punch.min.js",
               "~/inc/js/handlebars.runtime-v1.3.0.js",
               "~/inc/js/base.js",
                "~/inc/js/jquery.contextMenu.js",                
                "~/inc/js/markerwithlabel.js",
                "~/inc/js/jquery.fileupload.js",
                "~/inc/js/jquery.iframe-transport.js",
                "~/inc/js/jquery.cookie.js",
                "~/inc/js/mapAdmin.js"
                ).ForceOrdered());

            bundles.Add(new Bundle("~/inc/js/userJs", jsTransform).Include(
               "~/inc/js/jquery.min.js",
               "~/inc/js/jquery-ui.min.js",
               "~/inc/js/jquery.ui.touch-punch.min.js",
               "~/inc/js/base.js",
                "~/inc/js/map.js"
                ).ForceOrdered());

            bundles.Add(new Bundle("~/inc/js/regionJs", jsTransform).Include(
               "~/inc/js/jquery.min.js",
               "~/inc/js/jquery-ui.min.js",
               "~/inc/js/jquery.ui.touch-punch.min.js",
               "~/inc/js/base.js",
                "~/inc/js/totalMap.js"
                ).ForceOrdered());

            bundles.Add(new Bundle("~/inc/js/incidentReportJs", jsTransform).Include(
               "~/inc/js/jquery.min.js",
               "~/inc/js/jquery-ui.min.js",
               "~/inc/js/jquery.ui.touch-punch.min.js",
               "~/inc/js/handlebars.runtime-v1.3.0.js",
               "~/inc/js/base.js",
                "~/inc/js/incidentReportMap.js"
                ).ForceOrdered());
        }
    }

    internal class AsIsBundleOrderer : IBundleOrderer
    {
        public virtual IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }

    internal static class BundleExtensions
    {
        public static Bundle ForceOrdered(this Bundle sb)
        {
            sb.Orderer = new AsIsBundleOrderer();
            return sb;
        }
    }
}