using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Web.Optimization;
using FiberKartan.Resources;

/*
Copyright (c) 2012, Henrik Östman.

This file is part of FiberKartan.

FiberKartan is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

FiberKartan is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with FiberKartan.  If not, see <http://www.gnu.org/licenses/>.
*/
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
                "~/inc/js/label.js",
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