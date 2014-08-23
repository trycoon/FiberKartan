using System;
using System.IO;
using System.Web;
using System.Web.Optimization;
using Jurassic;
using Microsoft.Ajax.Utilities;

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
    public class CssBundleTransform : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            string content = string.Empty;
            foreach (var assetFile in response.Files)
            {
                var result = GetViewTuple(context, assetFile);
                content += result.Item2;
            }


            #if !DEBUG
                var minifier = new Minifier();
                var c = minifier.MinifyStyleSheet(content);
                if (minifier.ErrorList.Count <= 0)
                {
                    content = c;
                }
            # endif

            response.ContentType = "text/css";
            response.Cacheability = HttpCacheability.Public;
            response.Content = content;
        }

        public Tuple<String, String> GetViewTuple(BundleContext context, BundleFile assetFile)
        {
            var path = context.HttpContext.Server.MapPath(assetFile.IncludedVirtualPath);
            var virtualroot = path.Substring(0, path.LastIndexOf("\\")) + "\\";
            var file_extension = path.Substring(path.LastIndexOf("."));
            var template_file_name = path.Replace(virtualroot, string.Empty).Replace(file_extension, string.Empty);

            string template = File.ReadAllText(path);
            string templateName = template_file_name.Replace(
               this.fileNameSeparator,
               this.resourceNameSeparator
           );
            return Tuple.Create(templateName, template);
        }

        public string resourceNameSeparator = "/";
        public string fileNameSeparator = "-";
    }
}