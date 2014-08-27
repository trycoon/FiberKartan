using System;
using System.Collections.Generic;

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
    public class AnalyzedKml
    {
        public AnalyzedKml()
        {
            FoundMarkers = new List<FoundMarker>();
            FoundLines = new List<FoundLine>();
            FoundPolygons = new List<FoundPolygon>();
        }

        public List<FoundMarker> FoundMarkers { get; set; }
        public List<FoundLine> FoundLines { get; set; }
        public List<FoundPolygon> FoundPolygons { get; set; }
    }

    public class FoundMarker
    {
        public string MarkerHref { get; set; }
        public int NrFound { get; set; }
        public string SuggestedMarkerTranslation { get; set; }
    }

    public class FoundLine
    {
        public int NrFound { get; set; }
        public int SuggestedLineTranslation { get; set; }
    }

    public class FoundPolygon
    {
        public int NrFound { get; set; }
        public int SuggestedPolygonTranslation { get; set; }
    }

}