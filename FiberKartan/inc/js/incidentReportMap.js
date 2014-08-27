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
(function (fk) {
    var map;
    var drawingManager;
    var mapContent = fk.mapContent;
    var serverRoot = fk.serverRoot;
    var serviceProvider = fk.serviceProvider;
    var markersArray = [];
    var lineArray = [];

    var MARKERTYPE = { HouseYes: 'HouseYes', HouseMaybe: 'HouseMaybe', HouseNo: 'HouseNo', HouseNotContacted: 'HouseNotContacted', FiberNode: 'FiberNode', FiberBox: 'FiberBox', RoadCrossing_Existing: 'RoadCrossing_Existing', RoadCrossing_ToBeMade: 'RoadCrossing_ToBeMade', Fornlamning: 'Fornlamning', Observe: 'Observe', Note: 'Note', Unknown: 'Unknown' };
    var markerTypeLookup = new Object();
    var mapBounds = new google.maps.LatLngBounds();
    var incidentInfoWindow = new google.maps.InfoWindow({});

    // Deklarera ny funktion i jQuery för att hämta ut querystring-parametrar. Används: $.QueryString["param"]
    (function ($) {
        $.QueryString = (function (a) {
            if (a == "") return {};
            var b = {};
            for (var i = 0; i < a.length; ++i) {
                var p = a[i].split('=');
                if (p.length != 2) continue;
                b[p[0]] = decodeURIComponent(p[1].replace(/\+/g, " "));
            }
            return b;
        })(window.location.search.substr(1).split('&'))
    })(jQuery);

    $(document).ready(function () {

        var mapOptions = {
            zoom: 9.0,
            center: new google.maps.LatLng(mapContent.DefaultLatitude > 0 ? mapContent.DefaultLatitude : 57.47614, mapContent.DefaultLongitude > 0 ? mapContent.DefaultLongitude : 18.45059), // Fallback till mitt på Gotland.
            mapTypeId: google.maps.MapTypeId.HYBRID
        };
        map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);

        drawingManager = new google.maps.drawing.DrawingManager({
            drawingControl: false,
            drawingControlOptions: {
                position: google.maps.ControlPosition.TOP_CENTER,
                drawingModes: [google.maps.drawing.OverlayType.POLYGON]
            },           
            polygonOptions: {
                strokeColor: '#000000',
                fillColor: '#FF9999',
                fillOpacity: 0.3,
                strokeWeight: 2,
                strokeOpacity: 0.7
            }
        });
        drawingManager.setMap(map);

        if (typeof mapContent !== 'undefined') {
            createMarkerTypeLookupTable();
            plotMapContent();

            map.setOptions({ draggableCursor: 'crosshair' });
            showDialog('<div class="mapPopup">För att rapportera en incident klicka med korshåret på den plats på kartan som skall rapporteras, ni kan zooma in med scrollhjulet på musen för att närmare specificera den exakta positionen. Fyll därefter i formuläret och klicka på "Skicka" för att sända en incidentrapport till ' + serviceProvider + '.</div>', 'Beskrivning');

            google.maps.event.addListener(map, 'click', function (e) {
                clickedSpot(e);
            });
        }

        var centerPos = $.QueryString["center"];
        if (centerPos !== undefined) {
            map.setCenter(new google.maps.LatLng(centerPos.split('x')[0], centerPos.split('x')[1]));

            var centerZoom = $.QueryString["zoom"];
            if (centerZoom !== undefined) {
                map.setZoom(parseFloat(centerZoom));
            }
        }
        else if (typeof mapContent !== 'undefined' && markersArray.length > 0) {
            map.fitBounds(mapBounds);   // Sätt rätt zooom-nivå för att få med alla markörer.
        }
    });

    function createMarkerTypeLookupTable() {
        if (mapContent.MarkerTypes != null) {
            for (var i = 0, length = mapContent.MarkerTypes.length; i < length; i++) {
                markerTypeLookup[mapContent.MarkerTypes[i].Id] = mapContent.MarkerTypes[i];
            }
        }
    }

    function plotMapContent() {
        if (mapContent.Markers != null) {
            for (var i = 0, length = mapContent.Markers.length; i < length; i++) {
                addMarker(mapContent.Markers[i]);
            }
        }
        if (mapContent.Cables != null) {
            for (var i = 0, length = mapContent.Cables.length; i < length; i++) {
                addCable(mapContent.Cables[i].LineColor, mapContent.Cables[i].Width, mapContent.Cables[i].Coordinates);
            }
        }
    }

    function addMarker(markerInfo) {
        var markerType = markerTypeLookup[markerInfo.TypeId];
        var location = new google.maps.LatLng(markerInfo.Lat, markerInfo.Long);

        if (markerType != undefined) {  // Lägg bara till markörer som vi definierat.
            var estate;

            if (markerType.Name == MARKERTYPE.HouseYes || markerType.Name == MARKERTYPE.HouseMaybe || markerType.Name == MARKERTYPE.HouseNotContacted || markerType.Name == MARKERTYPE.HouseNo) {
                estate = markerInfo.Name;
            }

            var marker = new google.maps.Marker({
                position: location,
                map: map,
                estate: estate, //TODO skall vi verkligen besudla denna, eller skall vi inte alltid använda en wrapper-class???
                clickable: true,
                draggable: false,
                icon: markerType.Icon
            });
            mapBounds.extend(location);

            google.maps.event.addListener(marker, 'click', function (event) {
                clickedSpot(event, marker);
            });

            markersArray.push({ markerType: markerType, marker: marker });
        }
    }

    function addCable(lineColor, width, coordinatesString) {
        var path = new google.maps.MVCArray();
        var coordinates = coordinatesString.split('|');

        for (var i = 0, length = coordinates.length; i < length; i++) {
            var coordinatePair = coordinates[i].split(':');
            path.push(new google.maps.LatLng(parseFloat(coordinatePair[0]), parseFloat(coordinatePair[1])));
        }
        var line = new google.maps.Polyline({
            map: map,
            path: path,
            clickable: false,
            strokeColor: '#' + lineColor,
            strokeOpacity: 0.7,
            strokeWeight: width
        });

        lineArray.push({ cable: line });
    }

    function clickedSpot(event, marker) {
        incidentInfoWindow.close(); // Close possible already open one.
        var estate = marker && marker.estate;
        
        var position = event.latLng;
        if (marker) {
            position = marker.getPosition()
        }

        incidentInfoWindow.setContent(Handlebars.templates['incidentForm']({ estate: estate, lat: position.lat().toFixed(7), lng: position.lng().toFixed(6) }));
        incidentInfoWindow.setPosition(position);
        incidentInfoWindow.open(map);

        $('#sendbutton').on('click', function () {
            var incidentReport = {
                MapTypeId: mapContent.MapTypeId,
                Ver: mapContent.MapVer,
                Position: { Lat: position.lat().toFixed(7), Lng: position.lng().toFixed(6) },
                Estate: estate,
                Description: $('#desc').val()
            };

            $.ajax({
                type: 'POST',
                url: serverRoot + '/REST/FKService.svc/ReportIncident',
                data: JSON.stringify(incidentReport),
                contentType: 'application/json',
                dataType: 'json',
                success:
            function (result) {
                if (result.ErrorCode > 0) {
                    hideLoader();
                    alert(result.ErrorMessage);
                } else {
                    window.location.href = 'ShowMaps.aspx';
                }
            },
                error:
            function (XMLHttpRequest, textStatus, errorThrown) {
                hideLoader();
                alert("Ett fel uppstod vid rapportering av incident.");
            }
            });
        });
    }
})(fk);