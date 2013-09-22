using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using FiberKartan;
using FiberKartan.Kml;

/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/
namespace FiberKartan.Admin
{
    public class FileTransferHandler : IHttpHandler
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            //context.Response.AddHeader("Pragma", "no-cache");
            //context.Response.AddHeader("Cache-Control", "private, no-cache");

            HandleMethod(context);
        }

        // Handle request based on method
        private void HandleMethod(HttpContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "HEAD":
                case "GET":
                    DeliverFile(context);
                    break;

                case "OPTIONS":
                    ReturnOptions(context);
                    break;

                default:
                    context.Response.ClearHeaders();
                    context.Response.StatusCode = 405;
                    break;
            }
        }

        private static void ReturnOptions(HttpContext context)
        {
            context.Response.AddHeader("Allow", "HEAD,GET,OPTIONS");
            context.Response.StatusCode = 200;
        }

        private void DeliverFile(HttpContext context)
        {
            var mapfileId = context.Request["map"];

            if (!string.IsNullOrEmpty(mapfileId))
            {
                Utils.Log("Request to download mapfile with mapfileId=" + mapfileId, System.Diagnostics.EventLogEntryType.Information, 190);

                MapFile mapFile = null;
                var mapFileGuid = Guid.Empty;

                if (Guid.TryParse(mapfileId, out mapFileGuid))
                {
                    mapFile = new FiberDataContext().MapFiles.Where(mf => mf.Id == mapFileGuid).FirstOrDefault();
                }

                if (mapFile != null)
                {
                    Utils.Log("Found requested mapfile, sending map(mapfileId=" + mapfileId + ", name=\"" + mapFile.MapType.Title + "\") to user on IP-adress:" + context.Request.ServerVariables["REMOTE_ADDR"].ToString(), System.Diagnostics.EventLogEntryType.Information, 190);

                    context.Response.ClearHeaders();
                    context.Response.ClearContent();
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/vnd.google-earth.kmz";
                    context.Response.AddHeader("Content-Disposition", "attachment; filename=fastighetsgranser_" + mapFile.MapTypeId + ".kmz");

                    context.Response.BinaryWrite(mapFile.MapData.ToArray());
                    context.Response.Flush();
                }
                else
                {
                    Utils.Log("Failed to lookup requested mapfile to download,  mapfileId=" + mapfileId, System.Diagnostics.EventLogEntryType.Warning, 190);
                    context.Response.StatusCode = 404;
                }
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        }
    }
}