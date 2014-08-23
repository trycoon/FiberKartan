using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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