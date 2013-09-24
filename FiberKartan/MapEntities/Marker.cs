﻿using System;
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
    public class Marker
    {
        [Flags]
        public enum MARKER_SETTINGS : int {
            payedStake = 0x1,       // Har betalat insats.
            extraHouse = 0x2,       // Avser flygelavtal.
            wantDigHelp = 0x4,      // Vill ha hjälp av föreningen att gräva in på fastigheten.
            noISPsubscription = 0x8 // Vill inte ha avtal med ISP, vilande abonnemang.
        };

        /// <summary>
        /// Unikt Id, är olika mellan markörerna MEN ÄVEN mellan versionerna. 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unikt Id, är olika mellan markörerna MEN SAMMA mellan versionerna. 
        /// </summary>
        public int Uid { get; set; }

        /// <summary>
        /// Namn, fastighetsbeteckning eller liknande. Behöver inte vara unikt.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Latitud.
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// Longitud.
        /// </summary>
        public double Long { get; set; }

        /// <summary>
        /// Markörtyp
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Inställningar/egenskapar i form av en bitmask, se enum MARKER_SETTINGS.
        /// </summary>
        public int Settings { get; set; }

        /// <summary>
        /// Övriga inställningar/egenskapar.
        /// </summary>
        public OptionalInfo OptionalInfo { get; set; }
    }

    public class OptionalInfo
    {
        public int KS { get; set; }
        public bool ShowPublic { get; set; }
    }
}