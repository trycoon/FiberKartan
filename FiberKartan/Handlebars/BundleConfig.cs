using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Web.Optimization;
using Controllers.Transform;

namespace FiberKartan
{
    public class BundleConfig
    {
        public static void RegisterHandlBarBundles(BundleCollection bundles, string path)
        {
            HandleBarBundleTransform transform = new HandleBarBundleTransform();
            transform.jsPath = path;
            bundles.Add(new Bundle("~/inc/views", transform).IncludeDirectory("~/Handlebars/views", "*.hbs", true));
            #if !DEBUG
                BundleTable.EnableOptimizations = true;
            #endif
        }

        public static void RegisterBundles(BundleCollection bundles)
        {
            // http://www.codeproject.com/Articles/748849/ASP-NET-Web-Optimization-Framework

            bundles.Add(new StyleBundle("~/inc/adminCss").Include(
                 "~/inc/css/base.css",
                "~/inc/css/jquery-ui.min.css",
                "~/inc/css/map.css",
                "~/inc/css/jquery.contextMenu.css"
                ).ForceOrdered());

            bundles.Add(new StyleBundle("~/inc/userCss").Include(
                "~/inc/css/base.css",
                "~/inc/css/jquery-ui.min.css",
                "~/inc/css/map.css"
               ).ForceOrdered());

            bundles.Add(new ScriptBundle("~/inc/adminJs").Include(
               "~/inc/js/jquery.min.js",
               "~/inc/js/jquery-ui.min.js",
               "~/inc/js/jquery.ui.touch-punch.min.js",
               "~/inc/js/base.js",
                "~/inc/js/jquery.contextMenu.js",
                "~/inc/js/mapAdmin.js",
                "~/inc/js/markerwithlabel.js",
                "~/inc/js/jquery.fileupload.js",
                "~/inc/js/jquery.iframe-transport.js",
                "~/inc/js/jquery.cookie.js"
                ).ForceOrdered());

            bundles.Add(new ScriptBundle("~/inc/userJs").Include(
               "~/inc/js/jquery.min.js",
               "~/inc/js/jquery-ui.min.js",
               "~/inc/js/jquery.ui.touch-punch.min.js",
               "~/inc/js/base.js",
                "~/inc/js/map.js"
                ).ForceOrdered());

            bundles.Add(new ScriptBundle("~/inc/regionJs").Include(
               "~/inc/js/jquery.min.js",
               "~/inc/js/jquery-ui.min.js",
               "~/inc/js/jquery.ui.touch-punch.min.js",
               "~/inc/js/base.js",
                "~/inc/js/totalMap.js"
                ).ForceOrdered());

            bundles.Add(new ScriptBundle("~/inc/incidentReportJs").Include(
               "~/inc/js/jquery.min.js",
               "~/inc/js/jquery-ui.min.js",
               "~/inc/js/jquery.ui.touch-punch.min.js",
               "~/inc/js/base.js",
                "~/inc/js/incidentReportMap.js"
                ).ForceOrdered());

            #if !DEBUG
                BundleTable.EnableOptimizations = true;
            #endif
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