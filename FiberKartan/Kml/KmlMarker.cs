using System;

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
namespace FiberKartan.Kml
{
    public class KmlMarker
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public KmlCoordinate Point { get; set; }
        public int MarkerTypeId { get; set; }
        public int Settings { get; set; }
        public string OptionalInfo { get; set; }

        public KmlMarker()
        {
            OptionalInfo = "{}";
        }
    }
}