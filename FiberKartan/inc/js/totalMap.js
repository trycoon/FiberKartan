(function (fk) {
    var map;
    var mapContent = fk.mapContent;
    var serverRoot = fk.serverRoot;
    var markersArray = [];
    var lineArray = [];

    var MARKERTYPE = { HouseYes: 'HouseYes', FiberNode: 'FiberNode', FiberBox: 'FiberBox', RoadCrossing_Existing: 'RoadCrossing_Existing', RoadCrossing_ToBeMade: 'RoadCrossing_ToBeMade' };
    var markerTypeLookup = new Object();
    var mapBounds = new google.maps.LatLngBounds();
    var houseyes = true;
    var youMarker,
        gpsWatchId;

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
            zoom: mapContent.DefaultZoom,
            center: new google.maps.LatLng(57.47614, 18.45059), // Fallback till mitt på Gotland.
            mapTypeId: google.maps.MapTypeId.HYBRID
        };
        map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);

        $(window).bind('orientationchange', function (event) {
            if (window.orientation == 0) {
                $('.palette').fadeOut();
            } else {
                $('.palette').fadeIn();
            }
        });

        if ($.QueryString["houseyes"] == "true") {
            houseyes = true;
            $('input[name=show_houses]').prop('checked', true);
        } else {
            houseyes = false;
            $('input[name=show_houses]').removeProp('checked');
        }

        if (typeof welcomeMessage !== 'undefined' && welcomeMessage.length > 0) {
            showDialog('<div class="mapPopup">' + welcomeMessage + '</div>', 'Meddelande');
        }

        if (typeof mapContent !== 'undefined') {
            createMarkerTypeLookupTable();
            plotMapContent();
            setupPalette();

            // Kör dessa så att utseendet stämmer överens med kryssrutornas initialvärde.
            toggleShowHouses();
            toggleShowCrossings();

            $('#totalDigLength').html(calculateTotalDigLength());

            $("#mapInfoIcon").click(function () {
                showDialog(
                'Senast uppdaterad: ' + mapContent.Created + '<br /><br />'
                + 'Skapad i <a href="http://fiberkartan.se" target="_blank">Fiberkartan</a>',
                 'Kartinformation');
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

        // Anpassa karta efter önskad storlek, behövs för utskrifter.
        $('#viewSettings').change(function (event) {
            var body = $("body");
            switch (this.value) {
                case "screen": body.width("100%"); body.height("100%"); break;
                case "a0": body.width("2384pt"); body.height("3370pt"); break;
                case "a1": body.width("1684pt"); body.height("2384pt"); break;
                case "a2": body.width("1190pt"); body.height("1684pt"); break;
                case "a3": body.width("842pt"); body.height("1190pt"); break;
                case "a4": body.width("595pt"); body.height("842pt"); break;
            }
            // Om pappret är i liggande storlek så skiftar vi på axlarna.
            if ($("#viewSettingsHorizontal").is(':checked')) {
                var tmp = body.height();
                body.height(body.width());
                body.width(tmp);
            }
            google.maps.event.trigger(map, 'resize');   // Anpassa canvas efter nya storleken.
        });
        $('#viewSettingsHorizontal').change(function (event) {
            var body = $("body");
            var tmp = body.height();
            body.height(body.width());
            body.width(tmp);

            google.maps.event.trigger(map, 'resize');   // Anpassa canvas efter nya storleken.
        });

        // Kolla om webbläsaren är utrustad med GPS, och visar i så fall kryssruta för det valet.
        if (navigator.geolocation) {
            $('#myCurrentPosition').show();

            $('#myCurrentPositionCheckbox').click(function () {
                if ($(this).is(':checked')) {
                    navigator.geolocation.getCurrentPosition(function (position) {
                        var image = new google.maps.MarkerImage("/inc/img/markers/man.png",
			                new google.maps.Size(32.0, 32.0),
			                new google.maps.Point(0, 0),
			                new google.maps.Point(16.0, 16.0)
			            );
                        var shadow = new google.maps.MarkerImage("/inc/img/markers/man-shadow.png",
				            new google.maps.Size(49.0, 32.0),
				            new google.maps.Point(0, 0),
				            new google.maps.Point(16.0, 16.0)
			            );
                        var latLng = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
                        map.setCenter(latLng);
                        youMarker = new google.maps.Marker({
                            position: latLng,
                            map: map,
                            clickable: false,
                            draggable: false,
                            icon: image,
                            shadow: shadow,
                            zIndex: 9999,
                            title: 'lat(' + position.coords.latitude.toFixed(7) + ') long(' + position.coords.longitude.toFixed(6) + ').'
                        });
                        gpsWatchId = navigator.geolocation.watchPosition(function (position) {
                            latLng = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
                            map.setCenter(latLng);
                            youMarker.setPosition(latLng);
                            youMarker.setTitle('lat(' + position.coords.latitude.toFixed(7) + ') long(' + position.coords.longitude.toFixed(6) + ').');
                        });
                    });
                } else {
                    if (gpsWatchId !== undefined) {
                        navigator.geolocation.clearWatch(gpsWatchId);
                        gpsWatchId = undefined;
                    }
                    if (youMarker !== undefined) {
                        youMarker.setMap(null);
                        youMarker = undefined;
                    }
                }
            });
        }
    });

    function createMarkerTypeLookupTable() {
        if (mapContent.MarkerTypes != null) {
            for (var i = 0, length = mapContent.MarkerTypes.length; i < length; i++) {
                markerTypeLookup[mapContent.MarkerTypes[i].Id] = mapContent.MarkerTypes[i];
            }
        }
    }

    function setupPalette() {
        if ($(".palette").length > 0) {
            $(".palette").draggable({ handle: ".paletteheader" });
            $(".palette input[type=checkbox]:disabled").removeAttr("disabled"); // Fixar en skum bugg som gör att tredje checkboxen blir disabled av någon anledning.

            // Lägger till lyssnare till kryssrutorna.
            $("#show_houses").change(function () {
                houseyes = $("#show_houses").is(":checked");
                toggleShowHouses();
            });
            $("#show_crossings").change(function () {
                toggleShowCrossings();
            });
            $("#snapshotButton").click(function () {
                var mailLink = "mailto:?body=" + escape("http://fiberkartan.se/TotalMap.aspx?center=" + map.getCenter().lat() + "x" + map.getCenter().lng()
                + "&zoom=" + map.getZoom()
                );
                $(this).attr("href", mailLink);
            });
            $("#resetMapButton").click(function () {
                map.fitBounds(mapBounds);
            });
        }
    }

    function toggleShowHouses() {
        if (houseyes) {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.HouseYes)
                    markersArray[i].marker.setVisible(true);
            }
        } else {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.HouseYes)
                    markersArray[i].marker.setVisible(false);
            }
        }
    }

    function toggleShowCrossings() {
        if ($("#show_crossings").is(":checked")) {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.RoadCrossing_Existing || markersArray[i].markerType.Name == MARKERTYPE.RoadCrossing_ToBeMade)
                    markersArray[i].marker.setVisible(true);
            }
        } else {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.RoadCrossing_Existing || markersArray[i].markerType.Name == MARKERTYPE.RoadCrossing_ToBeMade)
                    markersArray[i].marker.setVisible(false);
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
            strokeColor: '#' + lineColor,
            strokeOpacity: 0.7,
            strokeWeight: width
        });

        lineArray.push({ cable: line });
    }

    function calculateTotalDigLength() {
        var lineLength = 0;
        for (var i = 0, length = lineArray.length; i < length; i++) {
            lineLength += google.maps.geometry.spherical.computeLength(lineArray[i].cable.getPath());
        }

        return lineLength.toFixed();
    }
})(fk);