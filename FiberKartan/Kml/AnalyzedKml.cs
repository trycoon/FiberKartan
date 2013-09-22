using System;
using System.Collections.Generic;

/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
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