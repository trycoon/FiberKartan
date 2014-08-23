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