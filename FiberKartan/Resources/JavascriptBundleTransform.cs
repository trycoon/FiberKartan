using System;
using System.IO;
using System.Web;
using System.Web.Optimization;
using Jurassic;
using Microsoft.Ajax.Utilities;

namespace FiberKartan.Resources
{
    public class JavascriptBundleTransform : IBundleTransform
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
                var c = minifier.MinifyJavaScript(content);
                if (minifier.ErrorList.Count <= 0)
                {
                    content = c;
                }
            # endif

            response.ContentType = "text/javascript";
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