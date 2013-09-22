using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/
namespace FiberKartan.MapEntities
{
    public class MapContent
    {
        private Settings settings;

        public int MapTypeId { get; set; }
        public int MapVer { get; set; }
        public string MapName { get; set; }
        public string Created { get; set; }
        public int Views { get; set; }
        public double DefaultLatitude { get; set; }
        public double DefaultLongitude { get; set; }
        public double DefaultZoom { get; set; }
        public string PropertyBoundariesFile { get; set; }

        public List<MarkerType> MarkerTypes { get; set; }
        public List<Marker> Markers { get; set; }
        public List<Region> Regions { get; set; }
        public List<Cable> Cables { get; set; }
        public Settings Settings
        {
            get
            {
                if (settings == null)
                    settings = new Settings();
                return settings;
            }
        }
    }

    /// <summary>
    /// Klass som serialiseras ut till klienten och som innehåller olika inställningar som klienten skall ta hänsyn till.
    /// </summary>
    public class Settings
    {
        public bool ShowTotalDigLength { get; set; }
        public bool HasWritePrivileges { get; set; } // Ifall användaren har rättighet att modifiera kartan. Har man inte Write-access skall man t.ex. inte få flytta markörer.
    }
}