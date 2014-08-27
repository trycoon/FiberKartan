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
    public class SimpleMapContent
    {
        public string Created { get; set; }     
        public double DefaultLatitude { get; set; }
        public double DefaultLongitude { get; set; }
        public double DefaultZoom { get; set; }

        public List<MarkerType> MarkerTypes { get; set; }
        public List<SimpleMarker> Markers { get; set; }
        public List<SimpleCable> Cables { get; set; }       
    }
}