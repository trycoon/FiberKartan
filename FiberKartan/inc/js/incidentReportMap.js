(function (fk) {
    var map;
    var mapContent = fk.mapContent;
    var serverRoot = fk.serverRoot;
    var serviceProvider = fk.serviceProvider;
    var markersArray = [];
    var lineArray = [];

    var MARKERTYPE = { HouseYes: 'HouseYes', FiberNode: 'FiberNode', FiberBox: 'FiberBox', RoadCrossing_Existing: 'RoadCrossing_Existing', RoadCrossing_ToBeMade: 'RoadCrossing_ToBeMade' };
    var markerTypeLookup = new Object();
    var mapBounds = new google.maps.LatLngBounds();

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

        if (typeof mapContent !== 'undefined') {
            createMarkerTypeLookupTable();
            plotMapContent();

            map.setOptions({ draggableCursor: 'crosshair' });
            showDialog('<div class="mapPopup">För att rapportera en incident klicka med korshåret på den plats på kartan som skall rapporteras, ni kan zooma in med scrollhjulet på musen för att närmare specificera den exakta positionen. Fyll därefter i formuläret och klicka på "Skicka" för att sända en incidentrapport till ' + serviceProvider + '.</div>', 'Beskrivning');
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
                addMarker(mapContent.Markers[i].TypeId, new google.maps.LatLng(mapContent.Markers[i].Lat, mapContent.Markers[i].Long));
            }
        }
        if (mapContent.Cables != null) {
            for (var i = 0, length = mapContent.Cables.length; i < length; i++) {
                addCable(mapContent.Cables[i].LineColor, mapContent.Cables[i].Width, mapContent.Cables[i].Coordinates);
            }
        }
    }

    function addMarker(typeId, location) {
        var markerType = markerTypeLookup[typeId];
        if (markerType != undefined) {  // Lägg bara till markörer som vi definierat.
            var marker = new google.maps.Marker({
                position: location,
                map: map,
                clickable: false,
                draggable: false,
                icon: markerType.Icon
            });
            mapBounds.extend(location);

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

})(fk);