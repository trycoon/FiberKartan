﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using FiberKartan.Database.Models;
using Newtonsoft.Json;

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
namespace FiberKartan.Database
{
    public class MsSQL : IDatabase
    {
        // http://www.sidarok.com/web/blog/content/2008/05/02/10-tips-to-improve-your-linq-to-sql-application-performance.html

        public MsSQL()
        {
            // Nothing here.
        }

        /*public static User GetUserByUsername(string username)
        {
            var user = (from u in fiberDb.Users where (u.Username == username) select u).First();

            return user;
        }*/

        /// <summary>
        /// Metoden används för att returnera vilken tillträdesnivå en användare har till en karta.
        /// </summary>
        /// <param name="userId">Id på användare</param>
        /// <param name="mapTypeId">Id på karta</param>
        /// <returns>Vilken tillträdesnivå användaren har till kartan</returns>
        public MapAccessRights GetMapAccessRights(int userId, int mapTypeId)
        {
            if (mapTypeId < 1 || userId < 1)
            {
                return MapAccessRights.None;
            }

            var fiberDb = new FiberDataContext();
            var user = fiberDb.Users.SingleOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return MapAccessRights.None;
            }

            if (user.IsAdmin)
            {
                // Adminusers har full access, så att vi kan hjälpa användare som har problem.
                return MapAccessRights.Read | MapAccessRights.Export | MapAccessRights.Write | MapAccessRights.FullAccess;
            }

            var access = fiberDb.MapTypeAccessRights.SingleOrDefault(mtar => mtar.MapTypeId == mapTypeId && mtar.UserId == user.Id);

            if (access == null)
            {
                return MapAccessRights.None;
            }

            return (MapAccessRights)access.AccessRight;
        }


        /// <summary>
        /// Metod som returnerar en lista på tillgängliga kartor.
        /// </summary>
        /// <param name="userId">Id på användare som gör anropet.</param>
        /// <param name="orderBy">Fält som vi skall sortera efter, "Title" är standard</param>
        /// <param name="sortOrder">Sorteringsordning(asc|desc)</param>
        /// <param name="offset">Från vilken post vi vill börja listan</param>
        /// <param name="count">Hur många poster vi är intresserade av</param>
        /// <returns>Lista på kartor</returns>
        public List<ViewMapType> GetMapTypes(int userId, string orderBy = "Title", string sortOrder = "asc", int offset = 0, int count = 20)
        {
            var fiberDb = new FiberDataContext();

            // TODO, sorteringsordning bör vara Title, men för administratörer bör de egna kartorna ligga först.
            var mapTypes = fiberDb.MapTypeAccessRights.Where("(UserId == @0 && AccessRight > 0) || User.IsAdmin", userId).Select(mt => new ViewMapType(mt.MapType)).OrderBy("@0 @1", orderBy, sortOrder).Skip(offset).Take(count).ToList();

            return mapTypes;
        }

        /// <summary>
        /// Metod som returnerar en efterfrågad karta.
        /// </summary>
        /// <param name="userId">Id på användare som gör anropet.</param>
        /// <param name="mapTypeId">Karta som efterfrågas</param>

        /// <returns>Efterfrågad karta, null om den inte finns</returns>
        public ViewMapType GetMapType(int userId, int mapTypeId)
        {
            var fiberDb = new FiberDataContext();
            ViewMapType mapType = null;

            // Kontrollera om användaren har rättigheter att granska karta.
            if (this.GetMapAccessRights(userId, mapTypeId).HasFlag(MapAccessRights.Read))
            {
                mapType = fiberDb.MapTypes.Where(mt => mt.Id == mapTypeId).Select(mt => new ViewMapType(mt)).FirstOrDefault();
            }

            return mapType;
        }

        /// <summary>
        /// Metod som returnerar en lista på versioner av en specifik karta.
        /// </summary>
        /// <param name="userId">Id på användare som gör anropet.</param>
        /// <param name="mapTypeId">Karta som efterfrågas</param>
        /// <param name="orderBy">Fält som vi skall sortera efter, "Ver" är standard</param>
        /// <param name="sortOrder">Sorteringsordning(asc|desc)</param>
        /// <param name="offset">Från vilken post vi vill börja listan</param>
        /// <param name="count">Hur många poster vi är intresserade av</param>
        /// <returns>Lista på kartversioner</returns>
        public List<ViewMap> GetMapVersions(int userId, int mapTypeId, string orderBy = "Ver", string sortOrder = "asc", int offset = 0, int count = 20)
        {
            var fiberDb = new FiberDataContext();

            var mapVersions = new List<ViewMap>();

            // Kontrollera om användaren har rättigheter att granska karta.
            if (this.GetMapAccessRights(userId, mapTypeId).HasFlag(MapAccessRights.Read))
            {
                mapVersions = fiberDb.Maps.Where(m => m.MapTypeId == mapTypeId).OrderBy("@0 @1", orderBy, sortOrder).Skip(offset).Take(count).Select(m => new ViewMap(m, false)).ToList();
            }

            return mapVersions;
        }

        /// <summary>
        /// Metod som returnerar en specifik version av en specifik karta.
        /// </summary>
        /// <param name="userId">Id på användare som gör anropet. Om inget id anges så kan man bara hämta upp publicerade kartor</param>
        /// <param name="mapTypeId">Karta som efterfrågas</param>
        /// <param name="version">Version av efterfrågad karta, 0=senast publicerad</param>
        /// <returns>Efterfrågad kartversion, eller null om denna inte kunde hittas</returns>
        public ViewMap GetMapVersion(int userId, int mapTypeId, int version = 0)
        {
            var fiberDb = new FiberDataContext();

            ViewMap map = null;
            if (version < 0)
            {
                version = 0;
            }

            var hasReadAccess = this.GetMapAccessRights(userId, mapTypeId).HasFlag(MapAccessRights.Read);

            map = fiberDb.Maps.Where(m => m.MapTypeId == mapTypeId && m.Ver == version).Select(m => new ViewMap(m, false)).SingleOrDefault();

            if (map != null)
            {
                // Om användaren inte har läs-access till kartan och kartversionen inte är publicerad så skall kartanversionen inte visas.
                if (!hasReadAccess && !map.Published.HasValue)
                {
                    map = null;
                }
            }

            return map;
        }

        /// <summary>
        /// Metod som returnerar ett eller flera lager för en karta.
        /// </summary>
        /// <param name="userId">Id på användare som gör anropet. Om inget id anges så kan man bara hämta upp publicerade kartor</param>
        /// <param name="mapTypeId">Karta som efterfrågas</param>
        /// <param name="ids">Id på lagret som skall hämtas, kommaseparerad för flera</param>
        /// <param name="version">[Frivilligt] Version av kartan som skall användas, om inget versionsnummer anges så antas den senaste versionen</param>
        /// <returns>Lista med kartlager</returns>
        public List<Layer> GetLayers(int userId, int mapTypeId, string ids, int version = 0)
        {
            var fiberDb = new FiberDataContext();
            var response = new List<Layer>();

            if (mapTypeId < 1 || string.IsNullOrEmpty(ids))
            {
                throw new DataException("mapTypeId och/eller id på lager saknas i anrop.");
            }

            Map map = null;

            if (version > 0)
            {
                map = (from m in fiberDb.Maps where (m.MapTypeId == mapTypeId && m.Ver == version) select m).FirstOrDefault();
            }
            else
            {
                map = (from m in fiberDb.Maps.OrderByDescending(m => m.Ver) where m.MapTypeId == mapTypeId select m).FirstOrDefault();
            }

            if (!this.GetMapAccessRights(userId, mapTypeId).HasFlag(MapAccessRights.Read))
            {
                throw new DatabaseException("Du saknar behörighet för att hämta kartlager från denna karta.");
            }

            ids = ids.Trim();
            var layersIds = ids.Split(',').Select(name => name.Trim()).ToArray();

            if (string.IsNullOrEmpty(map.Layers)) return response;
            var availableLayers = JsonConvert.DeserializeObject<List<Layer>>(map.Layers);
            availableLayers = availableLayers.Where(layer => ids.Contains(layer.Id)).ToList<Layer>();

            return response;
        }
    }
}
