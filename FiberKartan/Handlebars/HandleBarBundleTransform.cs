using System.Web;
using Jurassic;
using System.IO;
using System.Web.Optimization;
using System;
using Microsoft.Ajax.Utilities;
namespace Controllers.Transform
{
    public class HandleBarBundleTransform : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            string content = "Handlebars.templates = {};";
            foreach (var assetFile in response.Files)
            {
                var result = GetViewTuple(context, assetFile);
                content += PrecompileHandlebarsTemplate(result.Item1, result.Item2);
            }

            if (minifyTemplates)
            {
                var minifier = new Minifier();
                var c = minifier.MinifyJavaScript(content);
                if (minifier.ErrorList.Count <= 0)
                {
                    content = c;
                }
            }
            content += ";";
            content += @"Handlebars.partials = Handlebars.templates;
                          templates = Handlebars.templates;";
            response.ContentType = "text/javascript";
            response.Cacheability = HttpCacheability.Public;
            response.Content = content;
        }

        public string PrecompileHandlebarsTemplate(string name, string template)
        {
            var engine = new ScriptEngine();

            engine.ExecuteFile(jsPath);
            engine.Execute(@"var precompile = Handlebars.precompile;");
            return string.Format("Handlebars.templates[\"{0}\"] = Handlebars.template({1});",
                name, engine.CallGlobalFunction("precompile", template).ToString());
        }

        public Tuple<String, String> GetViewTuple(BundleContext context, BundleFile assetFile)
        {
            string template;
            string templateName;

            var virtual_root = assetFile.IncludedVirtualPath.Substring(0, assetFile.IncludedVirtualPath.IndexOf("\\"));

            var virtual_file_path = string.Format("~{0}", assetFile.VirtualFile.VirtualPath);

            var application_path = HttpContext.Current != null ? HttpContext.Current.Request.ApplicationPath : "/";

            var app_less_virtual_file_path = virtual_file_path.Replace("~" + application_path, String.Empty).TrimStart('/');

            virtual_file_path = string.Format("~/{0}", app_less_virtual_file_path).Replace(virtual_root + "/", string.Empty);

            var file_extension = virtual_file_path.Substring(virtual_file_path.LastIndexOf("."));

            var template_directory_name = string.Empty;

            var template_file_name = virtual_file_path
                                    .Replace("~", string.Empty)
                                    .Replace(file_extension, string.Empty);

            if (template_file_name.IndexOf(this.resourceNameSeparator) >= 0)
            {
                var segments = template_file_name.Split(this.resourceNameSeparator[0]);
                if (1 < segments.Length)
                {
                    var template_file_name_temp = string.Empty;
                    if (2 == segments.Length)
                    {

                        template_directory_name = segments[0];
                        template_file_name_temp = segments[1];

                        if (template_directory_name.Equals(segments[1]) ||
                             segments[1].Equals(this.defaultTemplateName))
                        {
                            template_file_name = template_directory_name;
                        }

                    }
                    else
                    {
                        template_directory_name = template_file_name.Substring(
                            0, template_file_name
                           .LastIndexOf(this.resourceNameSeparator + segments[segments.Length - 1]));

                        var template_directory_name_temp = segments[segments.Length - 2];
                        template_file_name_temp = segments[segments.Length - 1];

                        if (template_directory_name_temp.Equals(template_file_name_temp) ||
                             template_file_name_temp.Equals(this.defaultTemplateName))
                        {
                            template_file_name = template_file_name_temp;
                        }
                        else
                        {
                            template_file_name = template_directory_name_temp +
                                                 this.resourceNameSeparator +
                                                 template_file_name_temp;
                        }
                    }
                }
            }

            var path = context.HttpContext.Server.MapPath(virtual_root +
                                                          this.resourceNameSeparator +
                                                          virtual_file_path);

            template =  File.ReadAllText(path);
            templateName = template_file_name.Replace(
               this.fileNameSeparator,
               this.resourceNameSeparator
           );
            return Tuple.Create(templateName, template);
        }

        public string defaultTemplateName = "default";
        public string resourceNameSeparator = "/";
        public string fileNameSeparator = "-";
         #if DEBUG
            public bool minifyTemplates = false;
         #else
            public bool minifyTemplates = true;
        #endif
        public string jsPath { get; set; }
    }
}