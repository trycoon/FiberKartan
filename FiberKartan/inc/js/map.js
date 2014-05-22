/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/
(function (fk) {
    var map;
    var mapContent = fk.mapContent;
    var serverRoot = fk.serverRoot;
    var welcomeMessage = fk.welcomeMessage;
    var markersArray = [];
    var regionsArray = [];
    var lineArray = [];

    var MARKERTYPE = { HouseYes: 'HouseYes', HouseMaybe: 'HouseMaybe', HouseNo: 'HouseNo', HouseNotContacted: 'HouseNotContacted', FiberNode: 'FiberNode', FiberBox: 'FiberBox', RoadCrossing_Existing: 'RoadCrossing_Existing', RoadCrossing_ToBeMade: 'RoadCrossing_ToBeMade', Fornlamning: 'Fornlamning', Unknown: 'Unknown' };
    var markerTypeLookup = new Object();
    var mapBounds = new google.maps.LatLngBounds();
    var regionInfoWindow = new google.maps.InfoWindow({});
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
            zoom: 9.0,
            center: new google.maps.LatLng(mapContent.DefaultLatitude > 0 ? mapContent.DefaultLatitude : 57.47614, mapContent.DefaultLongitude > 0 ? mapContent.DefaultLongitude : 18.45059), // Fallback till mitt på Gotland.
            mapTypeId: google.maps.MapTypeId.HYBRID,
            scaleControl: true,
            scaleControlOptions: {
                position: google.maps.ControlPosition.BOTTOM_LEFT
            }
        };
        map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);

        $(window).bind('orientationchange', function (event) {
            if (window.orientation == 0) {
                $('.palette').fadeOut();
            } else {
                $('.palette').fadeIn();
            }
        });

        // Sätter upp krussrutorna och kartans "state" utifrån parametrar på querystring.
        var mapType = $.QueryString["mapType"];
        if (mapType === "ROADMAP") {
            map.setMapTypeId(google.maps.MapTypeId.ROADMAP);
        }

        var showHouseYes = $.QueryString["houseyes"];
        if (showHouseYes !== undefined) {
            if (showHouseYes == "true") {
                $('input[name=show_house_to_install]').prop('checked', true);
            } else {
                $('input[name=show_house_to_install]').removeProp('checked');
            }
        }
        var showHouseNo = $.QueryString["houseno"];
        if (showHouseNo !== undefined) {
            if (showHouseNo == "true") {
                $('input[name=show_house_no_dice]').prop('checked', true);
            } else {
                $('input[name=show_house_no_dice]').removeProp('checked');
            }
        }
        var showNetwork = $.QueryString["network"];
        if (showNetwork !== undefined) {
            if (showNetwork == "true") {
                $('input[name=show_network]').prop('checked', true);
            } else {
                $('input[name=show_network]').removeProp('checked');
            }
        }
        var showFiberNodes = $.QueryString["fibernodes"];
        if (showFiberNodes !== undefined) {
            if (showFiberNodes == "true") {
                $('input[name=show_fibernodes]').prop('checked', true);
            } else {
                $('input[name=show_fibernodes]').removeProp('checked');
            }
        }
        var showFiberBoxes = $.QueryString["fiberboxes"];
        if (showFiberBoxes !== undefined) {
            if (showFiberBoxes == "true") {
                $('input[name=show_fiberboxes]').prop('checked', true);
            } else {
                $('input[name=show_fiberboxes]').removeProp('checked');
            }
        }
        var showCrossings = $.QueryString["crossings"];
        if (showCrossings !== undefined) {
            if (showCrossings == "true") {
                $('input[name=show_crossings]').prop('checked', true);
            } else {
                $('input[name=show_crossings]').removeProp('checked');
            }
        }
        var showRegions = $.QueryString["regions"];
        if (showRegions !== undefined) {
            if (showRegions == "true") {
                $('input[name=show_regions]').prop('checked', true);
            } else {
                $('input[name=show_regions]').removeProp('checked');
            }
        }

        if (typeof welcomeMessage !== 'undefined' && welcomeMessage.length > 0) {
            showDialog('<div class="mapPopup">' + welcomeMessage + '</div>', 'Meddelande');
        }

        if (typeof mapContent !== 'undefined') {
            createMarkerTypeLookupTable();
            plotMapContent();
            setupPalette();

            if (mapContent.Settings.ShowTotalDigLength) {
                $('#totalDigLength').html(calculateTotalDigLength());
            }

            $("#mapInfoIcon").click(function () {
                showDialog(
                'Namn: ' + mapContent.MapName + '<br />'
                + 'Version: ' + mapContent.MapVer + '<br />'
                + 'Skapad: ' + mapContent.Created + '<br />'
                + 'Antal visningar: ' + mapContent.Views + '<br /><br />'
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
            // Om vi skall visa upp någon speciell markör eller linje på kartan, detta skickas i så fall in som parameter på querystringen.
            var markerId = $.QueryString["markerId"];
            var lineId = $.QueryString["lineId"];

            if (markerId !== undefined) {
                var specialMarker = getMarkerById(markerId);
                if (specialMarker != null) {
                    map.setCenter(specialMarker.marker.getPosition());
                    map.setZoom(16.0);
                    specialMarker.marker.setAnimation(google.maps.Animation.BOUNCE);
                }
            } else if (lineId !== undefined) {
                var specialLine = getLineById(lineId);
                if (specialLine != null) {
                    map.setCenter(specialLine.cable.getPath().getAt(0));
                    map.setZoom(16.0);
                    specialLine.cable.originalStrokeColor = specialLine.cable.get('strokeColor');
                    // Blink line.
                    setInterval(function () {
                        if (specialLine.cable.get('strokeColor') == specialLine.cable.originalStrokeColor) {
                            specialLine.cable.setOptions({ strokeColor: '#FFFF00' });
                        } else {
                            specialLine.cable.setOptions({ strokeColor: specialLine.cable.originalStrokeColor });
                        }
                    }, 1000);
                }
            } else {
                map.fitBounds(mapBounds);   // Sätt rätt zooom-nivå för att få med alla markörer.
            }
        }
        // Avslutar uppsättning efter querystring.

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

            // Kör dessa först så att utseendet stämmer överens med kryssrutornas initialvärde.
            toggleShowHouseToInstall();
            toggleShowHouseNotToInstall();
            toggleShowNetwork();
            toggleShowCrossings();
            toggleShowRegions();

            // Lägger till lyssnare till kryssrutorna.
            $("#show_house_to_install").change(function () {
                toggleShowHouseToInstall();
            });
            $("#show_house_no_dice").change(function () {
                toggleShowHouseNotToInstall();
            });
            $("#show_network").change(function () {
                toggleShowNetwork();
            });
            $("#show_fibernodes").change(function () {
                toggleShowFiberNodes();
            });
            $("#show_fiberboxes").change(function () {
                toggleShowFiberBoxes();
            });
            $("#show_crossings").change(function () {
                toggleShowCrossings();
            });
            $("#show_regions").change(function () {
                toggleShowRegions();
            });
            $("#snapshotButton").click(function () {
                var mailLink = 'mailto:?body=' + escape('http://fiberkartan.se/' + mapContent.MapTypeId + '/' + mapContent.MapVer
                + '?center=' + map.getCenter().lat() + 'x' + map.getCenter().lng() +
                '&zoom=' + map.getZoom() +
                '&houseyes=' + $("#show_house_to_install").is(":checked") +
                '&houseno=' + $("#show_house_no_dice").is(":checked") +
                '&network=' + $("#show_network").is(":checked") +
                '&fibernodes=' + $("#show_fibernodes").is(":checked") +
                '&fiberboxes=' + $("#show_fiberboxes").is(":checked") +
                '&crossings=' + $("#show_crossings").is(":checked") +
                '&regions=' + $("#show_regions").is(":checked")
                );
                $(this).attr('href', mailLink);
            });
            $("#resetMapButton").click(function () {
                map.fitBounds(mapBounds);
            });
        }
    }

    function toggleShowHouseToInstall() {
        if ($("#show_house_to_install").is(":checked")) {
            if (markersArray) {
                for (var i = 0, length = markersArray.length; i < length; i++) {
                    if (markersArray[i].markerType.Name == MARKERTYPE.HouseYes || markersArray[i].markerType.Name == MARKERTYPE.HouseMaybe) {
                        markersArray[i].marker.setVisible(true);
                    }
                }
            }
        } else {
            if (markersArray) {
                for (var i = 0, length = markersArray.length; i < length; i++) {
                    if (markersArray[i].markerType.Name == MARKERTYPE.HouseYes || markersArray[i].markerType.Name == MARKERTYPE.HouseMaybe) {
                        markersArray[i].marker.setVisible(false);
                    }
                }
            }
        }
    }

    function toggleShowHouseNotToInstall() {
        if ($("#show_house_no_dice").is(":checked")) {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.HouseNo || markersArray[i].markerType.Name == MARKERTYPE.HouseNotContacted) {
                    markersArray[i].marker.setVisible(true);
                }
            }
        } else {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.HouseNo || markersArray[i].markerType.Name == MARKERTYPE.HouseNotContacted) {
                    markersArray[i].marker.setVisible(false);
                }
            }
        }
    }

    function toggleShowNetwork() {
        if ($("#show_network").is(":checked")) {
            $("#subOption_fibernodes").show();
            $("#subOption_fiberboxes").show();

            var showNodes = $("#show_fibernodes").is(":checked");
            var showBoxes = $("#show_fiberboxes").is(":checked");

            // Visa noder och kopplingsskåp om dessa kryssrutor är ifyllda.
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.FiberNode) {
                    if (showNodes) {
                        markersArray[i].marker.setVisible(true);
                    } else {
                        markersArray[i].marker.setVisible(false);
                    }
                }
                else if (markersArray[i].markerType.Name == MARKERTYPE.FiberBox) {
                    if (showBoxes) {
                        markersArray[i].marker.setVisible(true);
                    } else {
                        markersArray[i].marker.setVisible(false);
                    }
                }
            }

            // Visa linjer
            for (var i = 0, length = lineArray.length; i < length; i++) {
                lineArray[i].cable.setMap(map);
            }
        } else {
            $("#subOption_fibernodes").hide();
            $("#subOption_fiberboxes").hide();

            // Dölj noder och kopplingsskåp.
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if ((markersArray[i].markerType.Name == MARKERTYPE.FiberNode) || (markersArray[i].markerType.Name == MARKERTYPE.FiberBox)) {
                    markersArray[i].marker.setVisible(false);
                }
            }
            // Dölj linjer
            for (var i = 0, length = lineArray.length; i < length; i++) {
                lineArray[i].cable.setMap(null);
            }
        }
    }

    function toggleShowFiberNodes() {
        if ($("#show_network").is(":checked") && $("#show_fibernodes").is(":checked")) {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.FiberNode)
                    markersArray[i].marker.setVisible(true);
            }
        } else {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.FiberNode)
                    markersArray[i].marker.setVisible(false);
            }
        }
    }

    function toggleShowFiberBoxes() {
        if ($("#show_network").is(":checked") && $("#show_fiberboxes").is(":checked")) {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.FiberBox) {
                    markersArray[i].marker.setVisible(true);
                }
            }
        } else {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.FiberBox) {
                    markersArray[i].marker.setVisible(false);
                }
            }
        }
    }

    function toggleShowCrossings() {
        if ($("#show_crossings").is(":checked")) {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.RoadCrossing_Existing || markersArray[i].markerType.Name == MARKERTYPE.RoadCrossing_ToBeMade) {
                    markersArray[i].marker.setVisible(true);
                }
            }
        } else {
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name == MARKERTYPE.RoadCrossing_Existing || markersArray[i].markerType.Name == MARKERTYPE.RoadCrossing_ToBeMade) {
                    markersArray[i].marker.setVisible(false);
                }
            }
        }
    }

    function toggleShowRegions() {
        if ($("#show_regions").is(":checked")) {
            for (var i = 0, length = regionsArray.length; i < length; i++) {
                regionsArray[i].region.setMap(map);
            }
        } else {
            for (var i = 0, length = regionsArray.length; i < length; i++) {
                regionsArray[i].region.setMap(null);
            }
        }
    }

    function plotMapContent() {
        if (mapContent.Markers != null) {
            for (var i = 0, length = mapContent.Markers.length; i < length; i++) {
                addMarker(mapContent.Markers[i].Id, mapContent.Markers[i].Name, mapContent.Markers[i].TypeId, new google.maps.LatLng(mapContent.Markers[i].Lat, mapContent.Markers[i].Long), mapContent.Markers[i].OptionalInfo);
            }
        }
        if (mapContent.Regions != null) {
            for (var i = 0, length = mapContent.Regions.length; i < length; i++) {
                addRegion(mapContent.Regions[i].Id, mapContent.Regions[i].Name, mapContent.Regions[i].BorderColor, mapContent.Regions[i].FillColor, mapContent.Regions[i].Coordinates);
            }
        }
        if (mapContent.Cables != null) {
            for (var i = 0, length = mapContent.Cables.length; i < length; i++) {
                addCable(mapContent.Cables[i].Id, mapContent.Cables[i].Name, mapContent.Cables[i].LineColor, mapContent.Cables[i].Width, mapContent.Cables[i].Coordinates);
            }
        }
    }

    function addMarker(id, name, typeId, location, optionalInfo) {
        var markerType = markerTypeLookup[typeId];
        if (markerType != undefined) {  // Lägg bara till markörer som vi definierat.

            // Gröna skall ligga ovanpå gula som skall ligga ovanpå röda...
            var zIndex = 0;
            switch (markerType.Name) {
                case MARKERTYPE.HouseNo: zIndex = 0; break;
                case MARKERTYPE.HouseMaybe: zIndex = 1; break;
                case MARKERTYPE.HouseNotContacted: zIndex = 1; break;
                case MARKERTYPE.HouseYes: zIndex = 2; break;
                case MARKERTYPE.FiberNode: zIndex = 3; break;
                default: zIndex = 0; break;
            }

            var marker = new google.maps.Marker({
                position: location,
                map: map,
                clickable: true,
                draggable: false,
                icon: markerType.Icon,
                title: markerType.Description + ". " + name,
                zIndex: zIndex,
                // Sparar ner denna också, vi behöver den när vi laddar upp info.
                id: id
            });
            mapBounds.extend(location);

            google.maps.event.addListener(marker, 'click', function (event) {
                if (optionalInfo !== null && optionalInfo.ShowPublic) {
                    // Laddar upp beskrivning om vi skall visa den och om vi inte redan har gjort det.
                    if (marker.desc == null) {
                        // Hämtar upp beskrivningen dynamiskt, det kostar för mycket bandbredd att ladda upp alla beskrivningar från början.
                        $.get(serverRoot + '/REST/FKService.svc/MarkerDescription/' + marker.id, function (markerDescription) {
                            marker.desc = markerDescription.Desc;  // Sparar undan orginalbeskrivningen.
                            var info = new google.maps.InfoWindow({ content: name + marker.desc });
                            info.open(map, marker);
                        });
                    } else {
                        var info = new google.maps.InfoWindow({ content: name + marker.desc });
                        info.open(map, marker);
                    }
                }
                else {
                    var info = new google.maps.InfoWindow({ content: name });
                    info.open(map, marker);
                }
            });

            markersArray.push({ markerType: markerType, marker: marker });
        }
    }

    function addRegion(id, name, borderColor, fillColor, coordinatesString) {
        var paths = new google.maps.MVCArray();
        var coordinates = coordinatesString.split('|');

        for (var i = 0, length = coordinates.length; i < length; i++) {
            var coordinatePair = coordinates[i].split(':');
            paths.push(new google.maps.LatLng(parseFloat(coordinatePair[0]), parseFloat(coordinatePair[1])));
        }
        var polygon = new google.maps.Polygon({
            map: null,  // Visas inte till att börja med.
            paths: paths,
            strokeColor: '#' + borderColor,
            fillColor: '#' + fillColor,
            fillOpacity: 0.3,
            strokeWeight: 2,
            strokeOpacity: 0.5,
            id: id
        });

        google.maps.event.addListener(polygon, 'click', function (event) {
            // Laddar upp beskrivning om vi inte redan har gjort det.
            if (polygon.content == null) {
                // Hämtar upp beskrivningen dynamiskt, det kostar för mycket bandbredd att ladda upp alla beskrivningar från början.
                $.get(serverRoot + '/REST/FKService.svc/RegionDescription/' + this.id, function (regionDescription) {
                    polygon.content = '<strong>' + htmlEncode(name) + '</strong><br/><div style="font-size:small">' + regionDescription.Desc + '</div>';
                    regionInfoWindow.setContent(polygon.content);
                    regionInfoWindow.setPosition(event.latLng);
                    regionInfoWindow.open(map);
                });
            } else {
                regionInfoWindow.setContent(polygon.content);
                regionInfoWindow.setPosition(event.latLng);
                regionInfoWindow.open(map);
            }
        });

        regionsArray.push({ id: id, region: polygon });
    }

    function addCable(id, name, lineColor, width, coordinatesString) {
        var path = new google.maps.MVCArray();
        var coordinates = coordinatesString.split('|');

        for (var i = 0, length = coordinates.length; i < length; i++) {
            var coordinatePair = coordinates[i].split(':');
            path.push(new google.maps.LatLng(parseFloat(coordinatePair[0]), parseFloat(coordinatePair[1])));
        }
        var line = new google.maps.Polyline({
            map: null,  // Visas inte till att börja med.
            path: path,
            strokeColor: '#' + lineColor,
            strokeOpacity: 0.7,
            strokeWeight: width
        });

        lineArray.push({ id: id, cable: line });
    }

    function calculateTotalDigLength() {
        var lineLength = 0;
        for (var i = 0, length = lineArray.length; i < length; i++) {
            lineLength += google.maps.geometry.spherical.computeLength(lineArray[i].cable.getPath());
        }

        return lineLength.toFixed();
    }

    function htmlEncode(value) {
        if (value) {
            return jQuery('<div />').text(value).html();
        } else {
            return '';
        }
    }

    function htmlDecode(value) {
        if (value) {
            return $('<div />').html(value).text();
        } else {
            return '';
        }
    }
})(fk);