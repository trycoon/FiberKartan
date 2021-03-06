﻿/*
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
(function(fk) {
    // Const och enum
    var STATETYPE = { None: 0, PlaceMarker: 1, EditMarker: 2, PlaceLine: 3, EditLine: 4, PlaceRegion: 5, EditRegion: 6, BindingMarkerToFiberBox: 7, UseRuler: 8 }; //TODO: Se till att state sätts till dessa värden i alla funktioner.
    var MARKERTYPE = { HouseYes: 'HouseYes', HouseMaybe: 'HouseMaybe', HouseNo: 'HouseNo', HouseNotContacted: 'HouseNotContacted', FiberNode: 'FiberNode', FiberBox: 'FiberBox', RoadCrossing_Existing: 'RoadCrossing_Existing', RoadCrossing_ToBeMade: 'RoadCrossing_ToBeMade', Fornlamning: 'Fornlamning', Observe: 'Observe', Note: 'Note', Unknown: 'Unknown' };
    var MARKER_SETTINGS = { payedStake: 0x1, extraHouse: 0x2, wantDigHelp: 0x4, noISPsubscription: 0x8 };

    var map,
    mapContent = fk.mapContent,
    serverRoot = fk.serverRoot,
    privateFuncs = {},
    mapOverlay,
    placesService,
    kmlLayer,
    drawingManager,
    markersArray = [],
    markerTypeLookup = {},
    lineArray = [],
    regionsArray = [],
    youMarker,
    gpsWatchId,
    currentSelectedObject = null,
    temporaryMarkerId = 0,  // Id på markörer som ännu inte har sparats. Är alltid ett negativt heltal.
    temporaryLineId = 0,    // Id på grävsträcka som ännu inte har sparats. Är alltid ett negativt heltal.
    temporaryRegionId = 0,  // Id på område som ännu inte har sparats. Är alltid ett negativt heltal.
    mapBounds = new google.maps.LatLngBounds(),
    geocoder = new google.maps.Geocoder(),
    ruler = {   // Initalisera linjalen
        rulerLine: new google.maps.Polyline({
            strokeColor: '#FF3333',
            strokeOpacity: 0.7,
            strokeWeight: 3
        }),
        vertexImage: {
            url: 'http://maps.google.com/mapfiles/kml/pal4/icon57.png',
            size: new google.maps.Size(32, 32),
            anchor: new google.maps.Point(16, 16)
        },
        vertexMarkers: []
    };

    (function(fk) {
        var currentState = STATETYPE.None;

        fk.setState = function(state) {
            currentState = state;
        };

        fk.getState = function() {
            return currentState;
        };
    })(fk);

    // Deklarera ny funktion i jQuery för att hämta ut querystring-parametrar. Används: $.QueryString["param"]
    (function($) {
        $.QueryString = (function(a) {
            if (a === "") return {};
            var b = {};
            for (var i = 0; i < a.length; ++i) {
                var p = a[i].split('=');
                if (p.length !== 2) continue;
                b[p[0]] = decodeURIComponent(p[1].replace(/\+/g, " "));
            }
            return b;
        })(window.location.search.substr(1).split('&'));
    })(jQuery);

    setupHandlebarsHelpers();

    $(document).ready(function() {
        if (typeof mapContent === 'undefined' || !mapContent) {
            showDialog('<div class="mapPopup">Kartan kunde inte hittas.</div>', 'Meddelande');
        } else {
            setupView();
        }
    });

    function setupView() {
        // Skicka initial ping för att visa att vi är anslutna.
        setTimeout(function() {
            $.get("../REST/FKService.svc/Ping");
        }, 400);

        // Startar ping-funktion för att visa att vi fortfarande är anslutna.
        setInterval(function() {
            $.get("../REST/FKService.svc/Ping").fail(function() {
                //TODO: Visa nått här för användaren, att tjänsten är nere. 
            });
        }, 5000);

        setupAccordion();

        var mapOptions = {
            zoom: 9.0,
            center: new google.maps.LatLng(mapContent.DefaultLatitude > 0 ? mapContent.DefaultLatitude : 57.47614, mapContent.DefaultLongitude > 0 ? mapContent.DefaultLongitude : 18.45059), // Fallback till mitt på Gotland.
            mapTypeId: google.maps.MapTypeId.HYBRID,
            disableDoubleClickZoom: true,
            scaleControl: true,
            scaleControlOptions: {
                position: google.maps.ControlPosition.BOTTOM_LEFT
            }
        };
        map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);
        mapOverlay = new google.maps.OverlayView(); // För att kunna konvertera mellan pixelposition på skärmen och kartpositioner.
        mapOverlay.draw = function() { };   // Måste sätta denna, men vi behöver den inte.
        mapOverlay.setMap(map);

        // Google Places, to search for addresses.
        placesService = new google.maps.places.PlacesService(map);

        // Skapa en statusrad längst ner på kartan.
        map.controls[google.maps.ControlPosition.BOTTOM_LEFT].push($('<div class="bottomBar"><span class="pointerPosition"></span><span class="bottomBarText"></span></div>')[0]);
        google.maps.event.addListener(map, 'mousemove', function(event) {
            showPointerPosition(event.latLng.lat(), event.latLng.lng());
        });

       $(document).keydown(function(e) {
            if (e.keyCode === 27) { // Esc-knapp
                // Om man trycker på Esc så avbryter man pågående operation (t.ex. bindning av kopplingskåp).
                if (fk.getState() === STATETYPE.BindingMarkerToFiberBox) {
                    // Vi återställer state, och skickar sedan ett event till eventuella lyssnare.
                    exitFiberboxBindingMode();
                    google.maps.event.trigger(document, "currentStateChange", e);
                }
            }
        });

        if (mapContent.Settings.HasWritePrivileges) {
            // Visa ritverktyg för linjer och polygoner(områden)
            drawingManager = new google.maps.drawing.DrawingManager({
                drawingControl: true,
                drawingControlOptions: {
                    position: google.maps.ControlPosition.TOP_CENTER,
                    drawingModes: [google.maps.drawing.OverlayType.POLYLINE, google.maps.drawing.OverlayType.POLYGON]
                },
                polylineOptions: {
                    strokeColor: '#0000FF',
                    strokeOpacity: 0.7,
                    strokeWeight: 5
                },
                polygonOptions: {
                    strokeColor: '#000000',
                    fillColor: '#9999FF',
                    fillOpacity: 0.3,
                    strokeWeight: 4,
                    strokeOpacity: 0.7
                }
            });
            drawingManager.setMap(map);

            google.maps.event.addListener(drawingManager, 'polylinecomplete', function(polyline) {
                // Tar först bort linjen vi skapade och skapar sedan om den med addCable()-funktionen så den läggs till korrekt och får alla eventlyssnare kopplade till sig.
                polyline.setMap(null);
                addCable(--temporaryLineId, 0, "Ny sträcka", "0000FF", 4, polyline.getPath(), 0);

                // Har man börjat rita en linje så skall nätverket visas, annars försvinner linjer bara spårlöst(visuellt) när man är klar.
                if (!$("#show_network").is(":checked")) {
                    $('input[name=show_network]').prop('checked', true);
                    toggleShowNetwork();
                }

                $('#totalDigLength').html(calculateTotalLineLength(0).toLocaleString());    // toLocaleString() fixar tusen avgränsare.
            });

            google.maps.event.addListener(drawingManager, 'polygoncomplete', function(polygon) {
                // Tar först bort området vi skapade och skapar sedan om den med addRegion()-funktionen så den läggs till korrekt och får alla eventlyssnare kopplade till sig.
                polygon.setMap(null);
                addRegion(--temporaryRegionId, 0, "Nytt område", "000000", "9999FF", polygon.getPath());

                // Har man börjat rita ett område så skall områden visas, annars försvinner området bara spårlöst(visuellt) när man är klar.
                if (!$("#show_regions").is(":checked")) {
                    $('input[name=show_regions]').prop('checked', true);
                    toggleShowRegions();
                }
            });

            google.maps.event.addListener(drawingManager, 'drawingmode_changed', function(event) {

                rulerStopMeasure(); // Ta bort linjal ifall denna används.
            });

            addMapRuler();
        }

        $(window).on('orientationchange', function(event) {
            if (window.orientation === 0) {
                $('.palette').fadeOut();
            } else {
                $('.palette').fadeIn();
            }
        });

        setupCheckboxesFromQuerystring();
        createMarkerTypeLookupTable();
        plotMapContent();
        setupPalette();
        setupShowMyPosition();

        var centerPos = $.QueryString["center"];
        if (centerPos !== undefined) {
            map.setCenter(new google.maps.LatLng(centerPos.split('x')[0], centerPos.split('x')[1]));

            var centerZoom = $.QueryString["zoom"];
            if (centerZoom !== undefined) {
                map.setZoom(parseFloat(centerZoom));
            }
        }
        else if (markersArray.length > 0) {
            // Om vi skall visa upp någon speciell markör eller linje på kartan, detta skickas i så fall in som parameter på querystringen.

            // Utmärka någon redan befintlig markör.
            var markerId = $.QueryString["markerId"];

            // Sätt ut en ny markör utifrå querystring, denna sparas aldrig.
            var marker = $.QueryString["marker"];

            // Utmärka någon redan befintlig linje.
            var lineId = $.QueryString["lineId"];

            var specialMarker;

            if (markerId !== undefined) {
                specialMarker = getMarkerById(markerId);
                if (specialMarker !== null) {
                    map.setCenter(specialMarker.marker.getPosition());
                    map.setZoom(16.0);
                    specialMarker.marker.setAnimation(google.maps.Animation.BOUNCE);
                }
            }
            else if (marker !== undefined) {
                specialMarker = new google.maps.Marker({
                    position: new google.maps.LatLng(marker.split('x')[0], marker.split('x')[1]),
                    icon: 'http://maps.google.com/mapfiles/kml/paddle/red-stars.png',
                    map: map
                });
                map.setCenter(specialMarker.getPosition());
                map.setZoom(16.0);
            } else if (lineId !== undefined) {
                var specialLine = getLineById(lineId);
                if (specialLine !== null) {
                    map.setCenter(specialLine.cable.getPath().getAt(0));
                    map.setZoom(16.0);
                    specialLine.cable.originalStrokeColor = specialLine.cable.get('strokeColor');
                    // Blink line.
                    setInterval(function() {
                        if (specialLine.cable.get('strokeColor') === specialLine.cable.originalStrokeColor) {
                            specialLine.cable.setOptions({ strokeColor: '#FFFF00' });
                        } else {
                            specialLine.cable.setOptions({ strokeColor: specialLine.cable.originalStrokeColor });
                        }
                    }, 1000);
                }
            } else {
                // För kartor i allmänhet som inte anropats med några speciella direktiv på querystringen.
                map.fitBounds(mapBounds);   // Sätt rätt zooom-nivå för att få med alla markörer.
            }
        }

        // Initialiserar editor EN gång här, istället för varje markör, linje och region.
        tinyMCE.init({
            mode: "none",
            language: "sv",
            width: "100%",
            entity_encoding: "raw",
            theme: "advanced",
            plugins: "autolink,lists,style,advlink,inlinepopups,noneditable,wordcount,media,table",

            // Theme options
            theme_advanced_buttons1: "bold,italic,underline,strikethrough,bullist,numlist,|,undo,redo,|,link,unlink,charmap,image,media,|,delete_row",
            theme_advanced_buttons2: "",
            theme_advanced_buttons3: "",
            theme_advanced_buttons4: "",
            theme_advanced_toolbar_location: "top",
            theme_advanced_toolbar_align: "left",
            theme_advanced_statusbar_location: "bottom",
            theme_advanced_resizing: false,
            content_css: "/inc/css/base.css?ver=1.8"
        });

        if (mapContent.Settings.HasWritePrivileges) {
            setupContextMenues();
        }

        if ($("#saveButton").length > 0) {
            $("#saveButton").click(function(event) {
                event.preventDefault();
                saveChanges();
            });
        }
        if ($("#saveAndPublishButton").length > 0) {
            $("#saveAndPublishButton").click(function(event) {
                event.preventDefault();
                saveChanges(true);
            });
        }
    }

    function setupAccordion() {
        // Återställer state på accordion.
        var accord_state = JSON.parse($.cookie('palette_accordion')) || { show: true, markers: true, search: false, printing: false };
        if (accord_state.show) {
            $("#togglePanels .showPanel").show();
        } else {
            $("#togglePanels .showPanel").hide();
        }
        if (accord_state.markers) {
            $("#togglePanels #markerTypes").show();
        } else {
            $("#togglePanels #markerTypes").hide();
        }
        if (accord_state.search) {
            $("#togglePanels .searchPanel").show();
        } else {
            $("#togglePanels .searchPanel").hide();
        }
        if (accord_state.printing) {
            $("#togglePanels .viewSettingsBox").show();
        } else {
            $("#togglePanels .viewSettingsBox").hide();
        }

        // Sätter upp accordion.
        $("#togglePanels")
        .find("h3")
        .click(function() {
            $(this).next().slideToggle();

            // Dessa går att läsa av först efter panelerna har fällts ihop.
            setTimeout(function() {
                accord_state.show = $("#togglePanels .showPanel").is(":visible");
                accord_state.markers = $("#togglePanels #markerTypes").is(":visible");
                accord_state.search = $("#togglePanels .searchPanel").is(":visible");
                accord_state.printing = $("#togglePanels .viewSettingsBox").is(":visible");
                $.cookie('palette_accordion', JSON.stringify(accord_state), { expires: 100 });
            }, 600);

            return false;
        }).next();
    }

    function setupCheckboxesFromQuerystring() {

        // Om man har laddat upp en karta med fastighetsgränser, visa kryssruta för att visa och dölja denna.
        if (mapContent.PropertyBoundariesFile) {
            $('#propertyBoundaries').show();

            $('#show_propertyBoundaries').click(function() {
                toggleShowPropertyBoundaries();
            });
        }

        // Sätter upp krussrutorna och kartans "state" utifrån parametrar på querystring.
        var mapType = $.QueryString["mapType"];
        if (mapType === "ROADMAP") {
            map.setMapTypeId(google.maps.MapTypeId.ROADMAP);
        }

        var showHouseYes = $.QueryString["houseyes"];
        if (showHouseYes !== undefined) {
            if (showHouseYes === "true") {
                $('input[name=show_house_to_install]').prop('checked', true);
            } else {
                $('input[name=show_house_to_install]').removeProp('checked');
            }
        }
        var showHouseNo = $.QueryString["houseno"];
        if (showHouseNo !== undefined) {
            if (showHouseNo === "true") {
                $('input[name=show_house_no_dice]').prop('checked', true);
            } else {
                $('input[name=show_house_no_dice]').removeProp('checked');
            }
        }
        var showNetwork = $.QueryString["network"];
        if (showNetwork !== undefined) {
            if (showNetwork === "true") {
                $('input[name=show_network]').prop('checked', true);
            } else {
                $('input[name=show_network]').removeProp('checked');
            }
        }
        var showFiberNodes = $.QueryString["fibernodes"];
        if (showFiberNodes !== undefined) {
            if (showFiberNodes === "true") {
                $('input[name=show_fibernodes]').prop('checked', true);
            } else {
                $('input[name=show_fibernodes]').removeProp('checked');
            }
        }
        var showFiberBoxes = $.QueryString["fiberboxes"];
        if (showFiberBoxes !== undefined) {
            if (showFiberBoxes === "true") {
                $('input[name=show_fiberboxes]').prop('checked', true);
            } else {
                $('input[name=show_fiberboxes]').removeProp('checked');
            }
        }
        var showCrossings = $.QueryString["crossings"];
        if (showCrossings !== undefined) {
            if (showCrossings === "true") {
                $('input[name=show_crossings]').prop('checked', true);
            } else {
                $('input[name=show_crossings]').removeProp('checked');
            }
        }
        var showRegions = $.QueryString["regions"];
        if (showRegions !== undefined) {
            if (showRegions === "true") {
                $('input[name=show_regions]').prop('checked', true);
            } else {
                $('input[name=show_regions]').removeProp('checked');
            }
        }
    }

    function setupShowMyPosition() {
        // Kolla om webbläsaren är utrustad med GPS, och visar i så fall kryssruta för det valet.
        if (navigator.geolocation) {
            $('#myCurrentPosition').show();

            $('#myCurrentPositionCheckbox').click(function() {
                if ($(this).is(':checked')) {
                    navigator.geolocation.getCurrentPosition(function(position) {
                        var image = new google.maps.MarkerImage("/inc/img/markers/man.png",
                        new google.maps.Size(32.0, 32.0),
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
                            zIndex: 9999,
                            title: 'lat(' + position.coords.latitude.toFixed(7) + ') long(' + position.coords.longitude.toFixed(6) + ').'
                        });
                        gpsWatchId = navigator.geolocation.watchPosition(function(position) {
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
    }

    function setupContextMenues() {
        $.contextMenu({
            selector: '#contextMenuLinePlaceholder',
            trigger: 'none',
            build: function($trigger, e) {
                // this callback is executed every time the menu is to be shown
                // its results are destroyed every time the menu is hidden
                // e is the original contextmenu event, containing e.pageX and e.pageY (amongst other data)
                return {
                    callback: function(key, options) {
                        var menuInfo = this.data('menuInfo'),
                            linePath;

                        switch (key) {
                            case "removeVertex":
                                if (menuInfo.vertex) {  // Kollar ifall vi verkligen klickat på en punkt.
                                    var line = getLineById(menuInfo.id);
                                    linePath = line.cable.getPath();
                                    // Ifall färre än två punkter på linjen blir kvar så tar vi bort hela linjen.
                                    if (linePath.length > 2) {
                                        linePath.removeAt(menuInfo.vertex);
                                    } else {
                                        removeLineById(menuInfo.id);
                                    }
                                }
                                break;
                            case "addVertex":
                                linePath = getLineById(menuInfo.id).cable.getPath();
                                // Räkna ut i vilken ordning av punkterna som den nya punkten skall läggas till.
                                var insertOrder = getPointInsertOrder(linePath, menuInfo.latLng);
                                // Lägg till nya punkten på linjen.
                                linePath.insertAt(insertOrder, menuInfo.latLng);
                                break;
                            case "removeLine":
                                removeLineById(menuInfo.id);
                                break;
                        }
                    },
                    items: {
                        "addVertex": { name: "Lägg till punkt", icon: "add" },
                        "removeVertex": { name: "Ta bort punkt", icon: "cut" },
                        "sep1": "---------",
                        "removeLine": { name: "Ta bort linje", icon: "delete" }
                    }
                };
            }
        });

        $.contextMenu({
            selector: '#contextMenuRegionPlaceholder',
            trigger: 'none',
            build: function($trigger, e) {
                // this callback is executed every time the menu is to be shown
                // its results are destroyed every time the menu is hidden
                // e is the original contextmenu event, containing e.pageX and e.pageY (amongst other data)
                return {
                    callback: function(key, options) {
                        var menuInfo = this.data('menuInfo'),
                            regionPath;

                        switch (key) {
                            case "removeVertex":
                                if (menuInfo.vertex) {  // Kollar ifall vi verkligen klickat på en punkt.
                                    var region = getRegionById(menuInfo.id);
                                    regionPath = region.region.getPath();
                                    // Ifall färre än tre punkter på området blir kvar så tar vi bort hela området.
                                    if (regionPath.length > 3) {
                                        regionPath.removeAt(menuInfo.vertex);
                                    } else {
                                        removeRegionById(menuInfo.id);
                                    }
                                }
                                break;
                            case "addVertex":
                                regionPath = getRegionById(menuInfo.id).region.getPath();
                                regionPath.insertAt(regionPath.length, menuInfo.latLng);
                                break;
                            case "removeRegion":
                                removeRegionById(menuInfo.id);
                                break;
                        }
                    },
                    items: {
                        "addVertex": { name: "Lägg till punkt", icon: "add" },
                        "removeVertex": { name: "Ta bort punkt", icon: "cut" },
                        "sep1": "---------",
                        "removeRegion": { name: "Ta bort område", icon: "delete" }
                    }
                };
            }
        });
    }

    function addMapRuler() {
        addToolToDrawManager('useRuler', 'Mät sträcka', 'https://maps.google.com/mapfiles/kml/pal5/icon5.png', function(button) {
            button.click(function() {
                button.css('background-color', '#ebebeb');   // Visa att mätverktyget är valt.
                rulerStartMeasure();
            });
        });
    }

    function countAndCleanRulerVertex() {
        count = 0;
        for (var i = ruler.vertexMarkers.length - 1; i >= 0; i--) {
            if (ruler.vertexMarkers[i].getMap() === null) {
                google.maps.event.clearInstanceListeners(ruler.vertexMarkers[i]); // Ta bort alla eventlyssnare.
                ruler.vertexMarkers.splice(i, 1);
            } else {
                count++;
            }
        }

        return count;
    }

    function drawRulerPath() {
        countAndCleanRulerVertex(); // Ta bort raderade vertex.

        var coords = [];
        for (var i = 0; i < ruler.vertexMarkers.length; i++) {
            coords.push(ruler.vertexMarkers[i].getPosition());
        }
        ruler.rulerLine.setPath(coords);

        var meters = google.maps.geometry.spherical.computeLength(coords);
        setStatusbarText('Total sträcka: ' + Math.ceil(meters).toLocaleString() + ' meter.');

    }

    function rulerStartMeasure() {

        rulerStopMeasure(); // Ta bort eventuell gammal mätning.
        drawingManager.setDrawingMode(google.maps.drawing.OverlayType.None);    // Avbryt ifall vi håller på att rita något annat.
        map.setOptions({ draggableCursor: 'crosshair' });   // Byt utseende på markören för att påvisa att vi är i mätläge.

        ruler.rulerLine.setMap(map);    // Visa linje.

        // Klick på kartan skall rita ut ny vertex (punkt) på linjen.
        ruler.clickListener = google.maps.event.addListener(map, 'click', function(event) {

            // Lägg till ny vertex med markör.
            var marker = new google.maps.Marker({
                position: event.latLng,
                icon: ruler.vertexImage,
                draggable: true,
                map: map
            });
            ruler.vertexMarkers.push(marker);

            google.maps.event.addListener(marker, 'dblclick', function(event) {
                marker.setMap(null);    // Ta bort vertex från kartan, den rensas bort från arrayen via drawRulerPath() -> countAndCleanRulerVertex()

                drawRulerPath();
            });

            google.maps.event.addListener(marker, 'drag', function(event) {
                drawRulerPath();    // Flytta befintlig vertex
            });

            drawRulerPath();
        });
    }

    function rulerStopMeasure() {
        // Ta bort lyssnare på klickevent för att lägga till nya punkter.
        google.maps.event.removeListener(ruler.clickListener);

        map.setOptions({ draggableCursor: null });  // Sätt tillbaka utseende på pekarmarkör till standard.

        // Ta bort vertex-markörer.
        for (var i = 0; i < ruler.vertexMarkers.length; i++) {
            ruler.vertexMarkers[i].setMap(null);
        }
        drawRulerPath();
        ruler.vertexMarkers.length = 0;

        // Ta bort linje från karta.
        ruler.rulerLine.setMap(null);

        setStatusbarText(''); // Töm statuslisten.
    }

    /**
     * Adds a custom button to the Draw Manager toolbar.
     * @param {string} buttonClass DOM class to use on button.
     * @param {string} title Title of button.
     * @param {string} imgSrc Uri to button image.
     * @param {function} callback Callback to be executed when button has been created and added, new button is provided as a parameter.
     * @param {number} imageMapHeight [optional] If image is an imagemap then specify its height.
     * @param {number} imageMapTop [optional] If image is an imagemap then specify the number of pixels from the top to the image we shall render.
     */
    function addToolToDrawManager(buttonClass, title, imgSrc, callback, imageMapHeight, imageMapTop) {
        if (!imageMapHeight) {
            imageMapHeight = 16;
        }
        if (!imageMapTop) {
            imageMapTop = 0;
        }

        var pollForButton;

        var newButton = $('<div style="float: left; line-height: 0;"><div class="' + buttonClass + '" style="direction: ltr; overflow: hidden; text-align: left; position: relative; color: rgb(51, 51, 51); font-family: Arial,sans-serif; -moz-user-select: none; font-size: 13px; background: none repeat scroll 0% 0% rgb(255, 255, 255); padding: 4px; border-width: 1px 1px 1px 0px; border-style: solid solid solid none; border-color: rgb(113, 123, 135) rgb(113, 123, 135) rgb(113, 123, 135) -moz-use-text-color; -moz-border-top-colors: none; -moz-border-right-colors: none; -moz-border-bottom-colors: none; -moz-border-left-colors: none; border-image: none; box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.4); font-weight: normal;" title="' + title + '"><span style="display: inline-block;"><div style="width: 16px; height: 16px; overflow: hidden; position: relative;"><img style="position: absolute; left: 0px; top: ' + imageMapTop + 'px; -moz-user-select: none; border: 0px none; padding: 0px; margin: 0px; width: 16px; height: ' + imageMapHeight + 'px;" src="' + imgSrc + '" draggable="false"></div></span></div></div>');
        // Om vi inte hittar ritverktygen så beror det på att Googles script ännu inte har renderat dessa, polla kontinuerligt i väntan på att denna har laddats in av Google.
        if ($('.gmnoprint').find("[title='Sluta rita']").length === 0) {
            pollForButton = setInterval(function() {
                if ($('.gmnoprint').find("[title='Sluta rita']").length > 0) {  // Kolla om knappen lagts till ännu.
                    clearInterval(pollForButton);
                    var drawManagerToolbar = $('.gmnoprint').find("[title='Sluta rita']").parent().parent();
                    drawManagerToolbar.append(newButton);

                    if (callback) {
                        callback(newButton);
                    }
                }
            }, 50);
        } else {
            var drawManagerToolbar = $('.gmnoprint').find("[title='Sluta rita']").parent().parent();
            drawManagerToolbar.append(newButton);

            if (callback) {
                setTimeout( // Se till att vi returnerar asynkront även då Google har satt upp ritverktygen.
                    callback(newButton),
                1);
            }
        }
    }

    /**
     * Uppdaterar positionen för pekaren i statusraden längst ner på sidan.
     * @param {number} lat latitude
     * @param {number} long longitude
     */
    function showPointerPosition(lat, long) {
        $('.pointerPosition').html('Pos lat: ' + lat.toFixed(7) + ', long: ' + long.toFixed(6));
    }

    function setStatusbarText(text) {
        $('.bottomBarText').text(text);
    }

    function createMarkerTypeLookupTable() {
        if (mapContent.MarkerTypes) {
            for (var i = 0, length = mapContent.MarkerTypes.length; i < length; i++) {
                markerTypeLookup[mapContent.MarkerTypes[i].Id] = mapContent.MarkerTypes[i];
            }
        }
    }
    function getMarkerById(id) {
        var marker = null;
        for (var i = 0, length = markersArray.length; i < length; i++) {
            if (markersArray[i].id === id) {
                marker = markersArray[i];
                break;
            }
        }
        return marker;
    }
    function getMarkerTypeByName(name) {
        var markerType = null;
        $.each(markerTypeLookup, function(key, value) {
            if (value.Name === name) {
                markerType = value;
                return false;
            }
        });

        return markerType;
    }
    function getLineById(id) {
        var line = null;
        for (var i = 0, length = lineArray.length; i < length; i++) {
            if (lineArray[i].id === id) {
                line = lineArray[i];
                break;
            }
        }
        return line;
    }
    function removeLineById(id) {
        for (var i = 0, length = lineArray.length; i < length; i++) {
            if (lineArray[i].id === id) {
                google.maps.event.clearInstanceListeners(lineArray[i].cable);    // Ta bort alla eventlyssnare.
                lineArray[i].cable.setMap(null);
                lineArray[i].cable = null;
                lineArray.splice(i, 1); // Ta bort linje.
                break;
            }
        }
    }
    function getRegionById(id) {
        var region = null;
        for (var i = 0, length = regionsArray.length; i < length; i++) {
            if (regionsArray[i].id === id) {
                region = regionsArray[i];
                break;
            }
        }
        return region;
    }
    function removeRegionById(id) {
        for (var i = 0, length = regionsArray.length; i < length; i++) {
            if (regionsArray[i].id === id) {
                google.maps.event.clearInstanceListeners(regionsArray[i].region);    // Ta bort alla eventlyssnare.
                regionsArray[i].region.setMap(null);
                regionsArray[i].region = null;
                regionsArray.splice(i, 1); // Ta bort område.
                break;
            }
        }
    }
    function enterFiberboxBindingMode() {
        fk.setState(STATETYPE.BindingMarkerToFiberBox);
        $('#mapForm').append('<div class="blackMask"></div>');  // Visa mask, för att få focus på det väsentliga.
        $('.palette').fadeOut("slow"); // Dölj palett som bara skulle vara i vägen.

        // Placera bilder på kopplingsskåp på de platser där de finns på kartan, det behövs för det går inte att få de befintliga kopplingsskåps-markörerna att ligga framför en svart mask.
        var markerHtml = '';
        for (var i = 0, length = markersArray.length; i < length; i++) {
            if (markersArray[i].markerType.Name === MARKERTYPE.FiberBox) {
                var point = getCanvasXY(markersArray[i].marker.getPosition());
                markerHtml += '<img class="tempBindingFiberbox" src="../inc/img/markers/FiberBox.png" style="top:' + (point.y - 32) + 'px; left: ' + (point.x - 16) + 'px;" data-fiberBoxId="' + markersArray[i].name + '" title="Bind till kopplingsskåp ' + markersArray[i].name + '" />';
            }
        }
        $('#mapForm').append(markerHtml);

        // Om man klickar på en bild så binds den tidigare valda markören till detta kopplingsskåp.
        $('.tempBindingFiberbox').click(function() {
            var markerToBind = getMarkerById(currentSelectedObject);
            markerToBind.optionalInfo.KS = $(this).attr('data-fiberBoxId').toString();            // Sätter markörens bindning till kopplingsskåpets namn(nummer).
            markerToBind.marker.set("labelContent", $(this).attr('data-fiberBoxId').toString());  // Uppdaterar markörens label.

            // Städa efter oss.
            exitFiberboxBindingMode();
        });
    }
    function exitFiberboxBindingMode() {
        fk.setState(STATETYPE.None);
        currentSelectedObject = null;
        $('.blackMask').remove();
        $('.tempBindingFiberbox').remove();
        $('.palette').fadeIn("slow");
    }

    function setupPalette() {
        if ($(".palette").length > 0) {
            $(".palette").draggable({ handle: ".paletteheader" });
            //$(".palette input[type=checkbox]:disabled").removeAttr("disabled"); // Fixar en skum bugg som gör att tredje checkboxen blir disabled av någon anledning.

            // Kör dessa först så att utseendet stämmer överens med kryssrutornas initialvärde.
            toggleShowHouseToInstall();
            toggleShowHouseNotToInstall();
            toggleShowNetwork();
            toggleShowCrossings();
            toggleShowRegions();

            $('#totalDigLength').html(calculateTotalLineLength(0).toLocaleString());    // toLocaleString() fixar tusen avgränsare.

            $("#mapInfoIcon").click(function() {
                showDialog(
            'Namn: ' + mapContent.MapName + '<br />'
            + 'Version: ' + mapContent.MapVer + '<br />'
            + 'Skapad: ' + mapContent.Created + '<br />'
            + 'Antal visningar: ' + mapContent.Views + '<br /><br />'
            + 'Skapad i <a href="http://fiberkartan.se" target="_blank">Fiberkartan</a>',
             'Kartinformation');
            });

            // Lägger till lyssnare till kryssrutorna.
            $("#show_house_to_install").change(function() {
                toggleShowHouseToInstall();
            });
            $("#show_house_no_dice").change(function() {
                toggleShowHouseNotToInstall();
            });
            $("#show_network").change(function() {
                toggleShowNetwork();
            });
            $("#show_fibernodes").change(function() {
                toggleShowFiberNodes();
            });
            $("#show_fiberboxes").change(function() {
                toggleShowFiberBoxes();
            });
            $("#show_fiberboxbindings").change(function() {
                toggleShowFiberboxbindings();
            });
            $("#show_crossings").change(function() {
                toggleShowCrossings();
            });
            $("#show_regions").change(function() {
                toggleShowRegions();
            });
            $("#snapshotButton").click(function() {
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
            $("#resetMapButton").click(function() {
                map.fitBounds(mapBounds);
            });

            $("#searchmode").on('change', function() {
                switch (this.value) {
                    case 'name': $("#searchBox").val('').attr('placeholder', ''); break;
                    case 'address': $("#searchBox").val('').attr('placeholder', 'ex. Hörsne Dibjärs 503'); break;
                    case 'position': $("#searchBox").val('').attr('placeholder', 'ex. 57.5081 18.6512'); break;
                }
            });
            $("#searchBox").autocomplete({
                delay: 600,     // Wait 600 ms before searching, waiting for user to finish typing.
                minLength: 2,   // Two characters must be entered before search begins.
                source: function(value, result) {
                    var criterion = value.term.toLowerCase();
                    var choices = [];

                    try {
                        switch ($("#searchmode").val()) {
                            case 'name':
                                for (var i = 0, length = markersArray.length; i < length; i++) {
                                    // Lite specialfix för kopplingsskåp, man vill kunna söka upp dom med prefixet "KS" istället för bara ett nummer, här lägger vi till prefixet just för denna lista.
                                    if (markersArray[i].markerType.Name === MARKERTYPE.FiberBox) {
                                        if (("KS" + markersArray[i].name).slice(0, criterion.length).toLowerCase() === criterion) { // Välj bara de som matchar.
                                            choices.push({ label: "KS" + markersArray[i].name, value: { fkMarker: markersArray[i] } });
                                        }
                                    } else {
                                        if (markersArray[i].name.slice(0, criterion.length).toLowerCase() === criterion) { // Välj bara de som matchar.
                                            choices.push({ label: markersArray[i].name, value: { fkMarker: markersArray[i] } });
                                        }
                                    }
                                }

                                result(choices);
                                break;

                            case 'address':
                                var request = {
                                    location: map.getCenter(),
                                    radius: '50000', // meter.
                                    query: criterion
                                };

                                placesService.textSearch(request, function(results, status) {

                                    if (status === google.maps.places.PlacesServiceStatus.OK) {

                                        for (var i = 0; i < results.length; i++) {

                                            choices.push({
                                                label: results[i].name, value: {
                                                    addressMarker: {
                                                        place_id: results[i].place_id,
                                                        location: results[i].geometry.location
                                                    }
                                                }
                                            });

                                        }
                                    }

                                    result(choices);
                                });

                                break;

                            case 'position':
                                // Check for valid format before setting position.
                                if (/^(\d+\.\d+)\s(\d+\.\d+)$/g.test(criterion)) {
                                    var pos = new google.maps.LatLng(criterion.split(' ')[0], criterion.split(' ')[1]);
                                    map.setCenter(pos);
                                    map.setZoom(15.0);

                                    var positionMarker = new google.maps.Marker({
                                        map: map,
                                        position: pos,
                                        zIndex: 900
                                    });

                                    setTimeout(function() {
                                        positionMarker.setMap(null);    // Dölj markör efter en stund, så att den inte är i vägen.
                                    }, 8000);
                                }

                                result();
                                break;

                        }
                    } catch (error) {
                        // Tom.
                    }
                },
                select: function(event, ui) {
                    event.preventDefault();
                    $("#searchBox").val(ui.item.label);

                    var item = ui.item.value;

                    if (item.fkMarker) {
                        // Om det är en placerad fiberkartan markör.
                        map.setCenter(item.fkMarker.marker.getPosition());
                        map.setZoom(18.0);
                        google.maps.event.trigger(item.fkMarker.marker, 'click');    // Tvinga fram ett klick för att öppna inforutan för en markör.

                    } else if (item.addressMarker) {
                        // Om det är en Google uppsökt adress.
                        getMoreAddressDetails(item.addressMarker.place_id, function(info) {

                            var infowindow = new google.maps.InfoWindow({
                                position: info.location,
                                content: Handlebars.templates['addressSearchResult'](info)
                            });

                            infowindow.open(map);

                        });
                    }
                }
            });
            $("#emptySearch").click(function() {
                $("#searchBox").val('');
            });

            // Anpassa karta efter önskad storlek, behövs för utskrifter.
            $('#viewSettings').change(function(event) {
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
            $('#viewSettingsHorizontal').change(function(event) {
                var body = $("body");
                var tmp = body.height();
                body.height(body.width());
                body.width(tmp);

                google.maps.event.trigger(map, 'resize');   // Anpassa canvas efter nya storleken.
            });

            // Möjlighet att dra ut nya markörer från paletten till kartan.
            if ($("#markerTypes").length > 0) {
                $("#markerTypes img").draggable({
                    helper: 'clone',
                    start: function(e) {
                        $(this).data("imgDragOffsetX", e.originalEvent.clientX - $(this).offset().left > e.target.width / 2 ? 1 - (e.originalEvent.clientX - $(this).offset().left - e.target.width / 2) : e.target.width / 2 - (e.originalEvent.clientX - $(this).offset().left));
                        $(this).data("imgDragOffsetY", e.target.height - (e.originalEvent.clientY - $(this).offset().top));
                    },
                    stop: function(e) {
                        var imgDragOffsetX = $(this).data("imgDragOffsetX");
                        var imgDragOffsetY = $(this).data("imgDragOffsetY");
                        var point = new google.maps.Point(e.originalEvent.pageX + imgDragOffsetX, e.originalEvent.pageY + imgDragOffsetY);
                        var latLong = mapOverlay.getProjection().fromContainerPixelToLatLng(point);
                        for (var x = 0; x < mapContent.MarkerTypes.length; x++) {
                            if (mapContent.MarkerTypes[x].Name === e.target.dataset.markertype) {
                                addMarker(--temporaryMarkerId, 0, '', mapContent.MarkerTypes[x].Id, latLong, 0, {});
                                break;
                            }
                        }
                    }
                });
            }
        }
    }

    function toggleShowHouseToInstall() {
        var i, length;
        if ($("#show_house_to_install").is(":checked")) {
            if (markersArray) {
                for (i = 0, length = markersArray.length; i < length; i++) {
                    if (markersArray[i].markerType.Name === MARKERTYPE.HouseYes || markersArray[i].markerType.Name === MARKERTYPE.HouseMaybe) {
                        markersArray[i].marker.setVisible(true);
                    }
                }
            }
        } else {
            if (markersArray) {
                for (i = 0, length = markersArray.length; i < length; i++) {
                    if (markersArray[i].markerType.Name === MARKERTYPE.HouseYes || markersArray[i].markerType.Name === MARKERTYPE.HouseMaybe) {
                        markersArray[i].marker.setVisible(false);
                    }
                }
            }
        }
    }

    function toggleShowHouseNotToInstall() {
        var i, length;
        if ($("#show_house_no_dice").is(":checked")) {
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name === MARKERTYPE.HouseNo || markersArray[i].markerType.Name === MARKERTYPE.HouseNotContacted) {
                    markersArray[i].marker.setVisible(true);
                }
            }
        } else {
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name === MARKERTYPE.HouseNo || markersArray[i].markerType.Name === MARKERTYPE.HouseNotContacted) {
                    markersArray[i].marker.setVisible(false);
                }
            }
        }
    }

    function toggleShowNetwork() {
        var i, length;
        if ($("#show_network").is(":checked")) {
            $("#subOption_fibernodes").show();
            $("#subOption_fiberboxes").show();
            if ($("#show_fiberboxes").is(":checked")) {
                $("#subOption_fiberboxbindings").show();
            }

            var showNodes = $("#show_fibernodes").is(":checked");
            var showBoxes = $("#show_fiberboxes").is(":checked");
            var showBoxBindings = $("#show_fiberboxbindings").is(":checked");

            // Visa noder, kopplingsskåp och kopplingsskåpsbindningar om dessa kryssrutor är ifyllda.
            if (showNodes || showBoxes || showBoxBindings) {
                for (i = 0, length = markersArray.length; i < length; i++) {
                    if (showNodes && markersArray[i].markerType.Name === MARKERTYPE.FiberNode || showBoxes && markersArray[i].markerType.Name === MARKERTYPE.FiberBox) {
                        markersArray[i].marker.setVisible(true);
                    } else if (showBoxes && showBoxBindings && markersArray[i].marker instanceof MarkerWithLabel) {
                        markersArray[i].marker.set("labelVisible", true);
                    }
                }
            }
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name === MARKERTYPE.FiberNode) {
                    if (showNodes) {
                        markersArray[i].marker.setVisible(true);
                    } else {
                        markersArray[i].marker.setVisible(false);
                    }
                }
                else if (markersArray[i].markerType.Name === MARKERTYPE.FiberBox) {
                    if (showBoxes) {
                        markersArray[i].marker.setVisible(true);
                    } else {
                        markersArray[i].marker.setVisible(false);
                    }
                }

                if (showBoxes && showBoxBindings && markersArray[i].marker instanceof MarkerWithLabel) {
                    markersArray[i].marker.set("labelVisible", true);
                }
            }

            // Visa linjer
            for (i = 0, length = lineArray.length; i < length; i++) {
                lineArray[i].cable.setMap(map);
            }
        } else {
            $("#subOption_fibernodes").hide();
            $("#subOption_fiberboxes").hide();
            $("#subOption_fiberboxbindings").hide();

            // Dölj noder, kopplingsskåp och kopplingsskåpsbindningar.
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name === MARKERTYPE.FiberNode || markersArray[i].markerType.Name === MARKERTYPE.FiberBox) {
                    markersArray[i].marker.setVisible(false);
                } else if (markersArray[i].marker instanceof MarkerWithLabel) {
                    markersArray[i].marker.set("labelVisible", false);
                }
            }
            // Dölj linjer
            for (i = 0, length = lineArray.length; i < length; i++) {
                lineArray[i].cable.setMap(null);
            }
        }
    }

    function toggleShowFiberNodes() {
        var i, length;
        if ($("#show_network").is(":checked") && $("#show_fibernodes").is(":checked")) {
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name === MARKERTYPE.FiberNode)
                    markersArray[i].marker.setVisible(true);
            }
        } else {
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name === MARKERTYPE.FiberNode)
                    markersArray[i].marker.setVisible(false);
            }
        }
    }

    function toggleShowFiberBoxes() {
        var i, length;
        if ($("#show_network").is(":checked") && $("#show_fiberboxes").is(":checked")) {
            $("#subOption_fiberboxbindings").show();
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name === MARKERTYPE.FiberBox) {
                    markersArray[i].marker.setVisible(true);
                }
            }
            toggleShowFiberboxbindings();
        } else {
            $("#subOption_fiberboxbindings").hide();
            toggleShowFiberboxbindings();
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name === MARKERTYPE.FiberBox) {
                    markersArray[i].marker.setVisible(false);
                }
            }
        }
    }

    function toggleShowFiberboxbindings() {
        var i, length;
        if ($("#show_network").is(":checked") && $("#show_fiberboxes").is(":checked") && $("#show_fiberboxbindings").is(":checked")) {
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].marker instanceof MarkerWithLabel) {
                    markersArray[i].marker.set("labelVisible", true);
                }
            }
        } else {
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].marker instanceof MarkerWithLabel) {
                    markersArray[i].marker.set("labelVisible", false);
                }
            }
        }
    }

    function toggleShowCrossings() {
        var i, length;
        if ($("#show_crossings").is(":checked")) {
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name === MARKERTYPE.RoadCrossing_Existing || markersArray[i].markerType.Name === MARKERTYPE.RoadCrossing_ToBeMade) {
                    markersArray[i].marker.setVisible(true);
                }
            }
        } else {
            for (i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name === MARKERTYPE.RoadCrossing_Existing || markersArray[i].markerType.Name === MARKERTYPE.RoadCrossing_ToBeMade) {
                    markersArray[i].marker.setVisible(false);
                }
            }
        }
    }

    function toggleShowRegions() {
        var i, length;
        if ($("#show_regions").is(":checked")) {
            for (i = 0, length = regionsArray.length; i < length; i++) {
                regionsArray[i].region.setMap(map);
            }
        } else {
            for (i = 0, length = regionsArray.length; i < length; i++) {
                regionsArray[i].region.setMap(null);
            }
        }
    }

    function toggleShowPropertyBoundaries() {
        if ($("#show_propertyBoundaries").is(":checked")) {
            if (kmlLayer instanceof google.maps.KmlLayer) {
                kmlLayer.setMap(null);  // Rensa bort eventuellt redan inladdad KML-fil.
            }

            kmlLayer = new google.maps.KmlLayer(mapContent.PropertyBoundariesFile,
                                {
                                    map: map,
                                    clickable: true,
                                    preserveViewport: false,
                                    suppressInfoWindows: false
                                });
        } else {
            if (kmlLayer instanceof google.maps.KmlLayer) {
                kmlLayer.setMap(null);  // Rensa bort eventuellt redan inladdad KML-fil.
                kmlLayer = null;
            }
        }
    }

    function plotMapContent() {
        var i;
        if (mapContent.Markers) {
            for (i = 0; i < mapContent.Markers.length; i++) {
                addMarker(mapContent.Markers[i].Id, mapContent.Markers[i].Uid, mapContent.Markers[i].Name, mapContent.Markers[i].TypeId, new google.maps.LatLng(mapContent.Markers[i].Lat, mapContent.Markers[i].Long), mapContent.Markers[i].Settings, mapContent.Markers[i].OptionalInfo);
            }
        }
        if (mapContent.Regions) {
            for (i = 0; i < mapContent.Regions.length; i++) {
                addRegion(mapContent.Regions[i].Id, mapContent.Regions[i].Uid, mapContent.Regions[i].Name, mapContent.Regions[i].BorderColor, mapContent.Regions[i].FillColor, mapContent.Regions[i].Coordinates);
            }
        }
        if (mapContent.Cables) {
            for (i = 0; i < mapContent.Cables.length; i++) {
                addCable(mapContent.Cables[i].Id, mapContent.Cables[i].Uid, mapContent.Cables[i].Name, mapContent.Cables[i].LineColor, mapContent.Cables[i].Width, mapContent.Cables[i].Coordinates, mapContent.Cables[i].Type);
            }
        }
    }

    function getMarkerTitle(marker, name, markerType) {
        return markerType.Description + ". " + name;
    }

    function addMarker(id, uid, name, markerTypeId, location, settings, optionalInfo) {
        var markerType = markerTypeLookup[markerTypeId];
        if (markerType) {  // Lägg bara till markörer som vi definierat.
            var marker = null;

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

            // Ta reda på vilka markörer som skall renderas med en inforuta med nummer för kopplingsskåp.
            if (markerType.Name === MARKERTYPE.FiberBox ||
            markerType.Name === MARKERTYPE.HouseYes ||
            markerType.Name === MARKERTYPE.HouseMaybe ||
            markerType.Name === MARKERTYPE.HouseNo ||
            markerType.Name === MARKERTYPE.HouseNotContacted) {
                var labelText;

                if (markerType.Name === MARKERTYPE.FiberBox) {
                    if (id < 0) { // Detta är ett kopplingsskåp som vi nu lägger till, hitta ett unikt id/namn för denna. Det behöver inte alltid vara det sista numret i serien, det kan finnas luckor som måste fyllas igen.
                        name = getNextAvailableFiberBoxId();
                    } else {
                        name = parseInt(name, 10).toString();  // remove leading zeros. older versions allowed names like "07", but they should now be converted to "7".
                    }

                    labelText = name;
                } else {
                    // set label on marker to the name of the fiberbox its bound to.
                    if (optionalInfo === null || optionalInfo.KS === 0 || optionalInfo.KS === '0') {
                        labelText = "?";
                    } else {
                        labelText = optionalInfo.KS;
                    }
                }

                marker = new MarkerWithLabel({
                    position: location,
                    map: map,
                    clickable: mapContent.Settings.HasWritePrivileges,
                    draggable: mapContent.Settings.HasWritePrivileges,
                    icon: markerType.Icon,
                    zIndex: zIndex,
                    labelContent: labelText,
                    labelAnchor: new google.maps.Point(25, 32),
                    labelTitle: markerType.Name === MARKERTYPE.FiberBox ? "Kopplingsskåpets nummer" : "Klicka på denna för att binda markören till ett kopplingsskåp",
                    labelClass: markerType.Name === MARKERTYPE.FiberBox ? "markerLabel fiberboxLabel" : "markerLabel",
                    labelInBackground: false,
                    labelVisible: false,
                    labelClickHandler: markerType.Name === MARKERTYPE.HouseYes ||
                    markerType.Name === MARKERTYPE.HouseMaybe ||
                    markerType.Name === MARKERTYPE.HouseNo ||
                    markerType.Name === MARKERTYPE.HouseNotContacted ?
                    function(e) {
                        if (mapContent.Settings.HasWritePrivileges) {
                            // Om man klickar på bubblan som visar vilket kopplingsskåp markören skall tillhöra så går man in i bindningsläget, nu skall man klicka på det kopplingsskåp som man vill binda markören till.
                            currentSelectedObject = id; // Sparar denna så vi vet vilken markör som skall kopplas.
                            enterFiberboxBindingMode();
                        }
                    } : undefined,
                    // Sparar ner denna också, vi behöver den när vi editerar markören.
                    id: id
                });
            } else {
                marker = new google.maps.Marker({
                    position: location,
                    map: map,
                    clickable: mapContent.Settings.HasWritePrivileges,
                    draggable: mapContent.Settings.HasWritePrivileges,
                    icon: markerType.Icon,
                    zIndex: zIndex,
                    // Sparar ner denna också, vi behöver den när vi editerar markören.
                    id: id
                });
            }

            marker.setTitle(getMarkerTitle(marker, name, markerType));
            mapBounds.extend(location);

            google.maps.event.addListener(marker, 'click', function(event) {
                editMarker(this);
            });
            google.maps.event.addListener(marker, 'drag', function(event) {
                showPointerPosition(event.latLng.lat(), event.latLng.lng());
            });

            markersArray.push({ id: id, uid: uid, name: name, desc: null, markerType: markerType, settings: settings, optionalInfo: optionalInfo, marker: marker });
        }
    }
    function editMarker(self) {
        if (currentSelectedObject) {
            // Om ett annat object redan är vald, se till att den inte längre går att modifiera(om den har en sådan function).
            if (typeof currentSelectedObject.setEditable === 'function') {
                currentSelectedObject.setEditable(false);
            }
            currentSelectedObject = null;   // Och att inget objekt längre är valt.
        }

        var clickedMarker = getMarkerById(self.id);
        clickedMarker.marker.setAnimation(google.maps.Animation.BOUNCE);    // Gör så att markören hoppar, så att man lättare hittar den.

        var nameFieldHtml;
        var markerTypesHtml;
        switch (clickedMarker.markerType.Name) {
            case MARKERTYPE.HouseYes:
            case MARKERTYPE.HouseMaybe:
            case MARKERTYPE.HouseNo:
            case MARKERTYPE.HouseNotContacted:
                nameFieldHtml = '<label for="name">Fastighetsbeteckning</label><br/>' +
                        '<input id="name" type="text" maxlength="100" title="Ange ort och fastighetsbeteckning, ex. Hörsne Mattsarve 1:14" /><br/><br/>';

                markerTypesHtml = '<img title="Fastighet som vill ha fiber." src="../inc/img/markers/HouseYes.png" data-markerType="HouseYes" />' +
                            '<img title="Fastighet som ännu inte har bestämt sig." src="../inc/img/markers/HouseNotDecided.png" data-markerType="HouseMaybe" />' +
                            '<img title="Fastighet som ännu inte har kontaktats." src="../inc/img/markers/HouseNotContacted.png" data-markerType="HouseNotContacted" />' +
                            '<img title="Fastighet som inte är intresserad av fiber." src="../inc/img/markers/HouseNo.png" data-markerType="HouseNo" />';
                break;

            case MARKERTYPE.FiberNode:
                nameFieldHtml = '<label for="name">Namn</label><br/>' +
                        '<input id="name" type="text" maxlength="100" title="Ange namn på noden, ex. Nod1" /><br/><br/>';

                markerTypesHtml = '<img title="Fibernod/överlämningspunkt." src="../inc/img/markers/FiberNode.png" data-markerType="FiberNode" />';
                break;

            case MARKERTYPE.FiberBox:
                nameFieldHtml = '<label for="name">Kopplingsskåp</label>' +
                        '<input id="name" class="oneLine" type="text" maxlength="3" pattern="\\d+" required x-validator-func="fiberboxValidator"/><br/><br/>';

                markerTypesHtml = '<img title="Kopplingsskåp." src="../inc/img/markers/FiberBox.png" data-markerType="FiberBox" />';
                break;

            case MARKERTYPE.RoadCrossing_Existing:
            case MARKERTYPE.RoadCrossing_ToBeMade:
                nameFieldHtml = '<label for="name">Väg- kanalundergång</label><br/>' +
                        '<input id="name" type="text" maxlength="100" title="Ange namn på väg- kanalundergången" /><br/><br/>';

                markerTypesHtml = '<img title="Befintlig väg/kanal-undergång." src="../inc/img/markers/RoadCrossing_Existing.png" data-markerType="RoadCrossing_Existing" />' +
                        '<img title="Väg/kanal-undergång som måste göras." src="../inc/img/markers/RoadCrossing_ToBeMade.png" data-markerType="RoadCrossing_ToBeMade" />';
                break;

            case MARKERTYPE.Fornlamning:
                nameFieldHtml = '<label for="name">Namn</label><br/>' +
                        '<input id="name" type="text" maxlength="100" title="Ange lämpligt namn, ex. Stensättning" /><br/><br/>';

                markerTypesHtml = '<img title="Fornlämning." src="../inc/img/markers/Fornlamning.png" data-markerType="Fornlamning" />';
                break;

            case MARKERTYPE.Observe:
                nameFieldHtml = '<label for="name">Namn</label><br/>' +
                        '<input id="name" type="text" maxlength="100" title="Ange lämpligt namn, ex. Elkabel" /><br/><br/>';

                markerTypesHtml = '<img title="Känslig plats(dränering, avlopp, elkabel, m.m.)." src="../inc/img/markers/Observe.png" data-markerType="Observe" />';
                break;
            case MARKERTYPE.Note:
                nameFieldHtml = '<label for="name">Rubrik</label><br/>' +
                        '<input id="name" type="text" maxlength="100" title="Ange lämplig rubrik" /><br/><br/>';

                markerTypesHtml = '<img title="Textruta." src="../inc/img/markers/Note.png" data-markerType="Note" />';
                break;

            default: nameFieldHtml = '<label for="name">Fastighetsbeteckning/namn</label><br/>' +
                                                '<input id="name" type="text" maxlength="100" /><br/><br/>';
        }

        var $dialog = $('<div id="edit"></div>')
                        .html(
                                nameFieldHtml +
                                '<label for="desc">Beskrivning</label><br/>' +
                                '<textarea id="desc" rows="6" cols="58"></textarea><br/>' +
                                '<fieldset id="markerTypes"><legend>Typ av mark&ouml;r</legend>' +
                                    markerTypesHtml +
                                '</fieldset>' +
                                (clickedMarker.markerType.Name === MARKERTYPE.HouseYes || clickedMarker.markerType.Name === MARKERTYPE.HouseMaybe ?
                                '<fieldset id="other_settings"><legend>Övrigt</legend>' +
                                    '<input type="checkbox" id="payedStake" name="payedStake" /><label for="payedStake">Har betalat insats</label><br />' +
                                    '<input type="checkbox" id="extraHouse" name="extraHouse" /><label for="extraHouse">Avser flygelavtal</label><br />' +
                                    '<input type="checkbox" id="wantDigHelp" name="wantDigHelp" /><label for="wantDigHelp">Önskar att förening ordnar grävning på fastighet</label><br />' +
                                    '<input type="checkbox" id="noISPsubscription" name="noISPsubscription" /><label for="noISPsubscription">Önskar inget abonnemang med operatör (vilande)</label>' +
                                '</fieldset>'
                                : "") +
                                (clickedMarker.markerType.Name === MARKERTYPE.Note ?
                                '<fieldset id="other_settings"><legend>Övrigt</legend>' +
                                    '<input type="checkbox" id="showNotePublic" name="showNotePublic" /><label for="showNotePublic">Visa i publika vyn</label>' +
                                '</fieldset>'
                                : "") +
                                '<p>Position(WGS84) lat: ' + clickedMarker.marker.getPosition().lat().toFixed(7) + ', long: ' + clickedMarker.marker.getPosition().lng().toFixed(6) + '</p>' +
                                '<div id="reverseGeocode_' + clickedMarker.id + '" class="reverseGeocode clickable">Visa närmsta belägenhetsadress</div>' +
                                '<input id="okButton" type="button" value="Ok" /><input id="deleteButton" type="button" value="Radera" />'
                        )
                        .dialog({
                            autoOpen: false,
                            title: 'Redigerar ' + clickedMarker.name,
                            close: function() {
                                if (clickedMarker && clickedMarker.marker) {
                                    clickedMarker.marker.setAnimation(null);    // Sluta hoppa markör.
                                }
                                if (tinyMCE.getInstanceById('desc')) {
                                    tinyMCE.execCommand('mceFocus', false, 'desc'); // IE fix.                   
                                    tinyMCE.execCommand('mceRemoveControl', false, 'desc');   // Ta bort editor.
                                }
                                $(this).remove();
                            },
                            width: 430,
                            modal: true,
                            resizable: false,
                            open: function() {
                                $("#name").val(clickedMarker.name);
                                tinyMCE.execCommand('mceAddControl', false, 'desc');  // Initialiserar editor.
                                // Om bit 0 är satt, kryssa i ruta för "Har betalat insats".
                                if ((clickedMarker.settings & MARKER_SETTINGS.payedStake) !== 0) {
                                    $('input[name=payedStake]').prop('checked', true);
                                }
                                // Om bit 1 är satt, kryssa i ruta för "Avser flygelavtal".
                                if ((clickedMarker.settings & MARKER_SETTINGS.extraHouse) !== 0) {
                                    $('input[name=extraHouse]').prop('checked', true);
                                }
                                // Om bit 2 är satt, kryssa i ruta för "Önskar att förening grävning på fastighet".
                                if ((clickedMarker.settings & MARKER_SETTINGS.wantDigHelp) !== 0) {
                                    $('input[name=wantDigHelp]').prop('checked', true);
                                }
                                // Om bit 3 är satt, kryssa i ruta för "Önskar inget abonnemang med operatör (vilande)".
                                if ((clickedMarker.settings & MARKER_SETTINGS.noISPsubscription) !== 0) {
                                    $('input[name=noISPsubscription]').prop('checked', true);
                                }

                                // Sätter nuvarande status för om info-rutor skall visas på den publika vyn.
                                if (clickedMarker.optionalInfo.ShowPublic) {
                                    $('input[name=showNotePublic]').prop('checked', true);
                                }

                                // Laddar upp beskrivning om vi inte redan har gjort det.
                                if (clickedMarker.desc === null) {
                                    //tinyMCE.getInstanceById('desc').setProgressState(true); // Visar loader.
                                    // Hämtar upp beskrivningen dynamiskt, det kostar för mycket bandbredd att ladda upp alla beskrivningar från början.
                                    $.get(serverRoot + '/REST/FKService.svc/MarkerDescription/' + clickedMarker.id, function(markerDescription) {
                                        clickedMarker.desc = markerDescription.Desc;  // Sparar undan orginalbeskrivningen.
                                        $('#desc').val(markerDescription.Desc); // Sätt beskrivning i textarea och visa editor.
                                        var instance = tinyMCE.getInstanceById('desc');
                                        if (instance) {
                                            instance.setContent(markerDescription.Desc);  // Sätter beskrivning i editor.
                                        }
                                        //tinyMCE.getInstanceById('desc').setProgressState(false);   // Döljer loader.
                                    });
                                } else {
                                    tinyMCE.getInstanceById('desc').setContent(clickedMarker.desc);
                                }

                                // Sättar markörens nuvarande typ som vald i listan över markörtyper.
                                switch (clickedMarker.markerType.Name) {
                                    case MARKERTYPE.HouseYes: $('#edit #markerTypes img[data-markerType="HouseYes"]').addClass('selected'); break;
                                    case MARKERTYPE.HouseMaybe: $('#edit #markerTypes img[data-markerType="HouseMaybe"]').addClass('selected'); break;
                                    case MARKERTYPE.HouseNo: $('#edit #markerTypes img[data-markerType="HouseNo"]').addClass('selected'); break;
                                    case MARKERTYPE.HouseNotContacted: $('#edit #markerTypes img[data-markerType="HouseNotContacted"]').addClass('selected'); break;
                                    case MARKERTYPE.FiberNode: $('#edit #markerTypes img[data-markerType="FiberNode"]').addClass('selected'); break;
                                    case MARKERTYPE.FiberBox: $('#edit #markerTypes img[data-markerType="FiberBox"]').addClass('selected'); break;
                                    case MARKERTYPE.RoadCrossing_Existing: $('#edit #markerTypes img[data-markerType="RoadCrossing_Existing"]').addClass('selected'); break;
                                    case MARKERTYPE.RoadCrossing_ToBeMade: $('#edit #markerTypes img[data-markerType="RoadCrossing_ToBeMade"]').addClass('selected'); break;
                                    case MARKERTYPE.Fornlamning: $('#edit #markerTypes img[data-markerType="Fornlamning"]').addClass('selected'); break;
                                    case MARKERTYPE.Observe: $('#edit #markerTypes img[data-markerType="Observe"]').addClass('selected'); break;
                                    case MARKERTYPE.Note: $('#edit #markerTypes img[data-markerType="Note"]').addClass('selected'); break;
                                    default: $('#edit #markerTypes img[data-markerType="HouseYes"]').addClass('selected');
                                }
                                $('#edit #markerTypes img').each(function() {
                                    $(this).click(function() {
                                        $('#edit #markerTypes img').removeClass('selected');
                                        $(this).addClass('selected');
                                    });
                                });
                                $("#okButton").click(function(event) {

                                    // loop all input-elements and call each validator function if available.
                                    var allValid = true, result;
                                    $("#edit input").each(function(i, el) {
                                        $(this).removeClass("invalid");
                                        var validator = $(this).attr("x-validator-func");
                                        if (validator && privateFuncs[validator]) {
                                            result = privateFuncs[validator]($(this), clickedMarker);
                                            if (!result) {
                                                allValid = false;
                                            }
                                        }
                                    });

                                    // don't proceed unless all validations has passed.
                                    if (!allValid) {
                                        return false;
                                    }
      
                                    // if the marker that was changed was a FiberBox and the name was changed,
                                    // then we we need to update its bindinginformation and all other markers bound to it.
                                    if (clickedMarker.markerType.Name === MARKERTYPE.FiberBox && clickedMarker.name !== $("#name").val()) {
                                        var newName = $("#name").val();
                                        updateFiberboxBinding(clickedMarker, clickedMarker.name, newName);
                                        clickedMarker.name = newName;
                                    } else {
                                        clickedMarker.name = $("#name").val();
                                    }

                                    clickedMarker.desc = tinyMCE.get('desc').getContent();

                                    // Spara undan kryssrutor under Övrigt till settings-propertyn.
                                    clickedMarker.settings = 0;
                                    if ($("#payedStake").prop("checked")) {
                                        clickedMarker.settings |= MARKER_SETTINGS.payedStake;
                                    }
                                    if ($("#extraHouse").prop("checked")) {
                                        clickedMarker.settings |= MARKER_SETTINGS.extraHouse;
                                    }
                                    if ($("#wantDigHelp").prop("checked")) {
                                        clickedMarker.settings |= MARKER_SETTINGS.wantDigHelp;
                                    }
                                    if ($("#noISPsubscription").prop("checked")) {
                                        clickedMarker.settings |= MARKER_SETTINGS.noISPsubscription;
                                    }

                                    if ($("#showNotePublic").prop("checked")) {
                                        clickedMarker.optionalInfo.ShowPublic = true;
                                    } else {
                                        clickedMarker.optionalInfo.ShowPublic = false;
                                    }

                                    var markerTypeName = $('#edit #markerTypes img.selected').attr('data-markerType');
                                    clickedMarker.markerType = getMarkerTypeByName(markerTypeName);
                                    clickedMarker.marker.setTitle(getMarkerTitle(clickedMarker.marker, clickedMarker.name, clickedMarker.markerType));
                                    clickedMarker.marker.setIcon(clickedMarker.markerType.Icon);

                                    $('#edit').dialog('close');
                                });
                                $("#deleteButton").click(function(event) {
                                    for (var i = 0, length = markersArray.length; i < length; i++) {
                                        if (markersArray[i].id === clickedMarker.id) {
                                            google.maps.event.clearInstanceListeners(clickedMarker);    // Ta bort alla eventlyssnare.
                                            clickedMarker.marker.setMap(null);
                                            clickedMarker.marker = null;
                                            clickedMarker = null;
                                            markersArray.splice(i, 1); // Ta bort markör.
                                            $('#edit').dialog('close');
                                            break;
                                        }
                                    }
                                });

                                $('#reverseGeocode_' + clickedMarker.id).on("click", function() {
                                    reverseGeocodeMarker($(this), clickedMarker.marker.getPosition());
                                });
                            }
                        });
        $dialog.dialog('open');
    }

    /**
     * Update FiberBox binding information and all other markers bound to it.
     * @param {Object} fiberboxMarker markerobject of fiberbox we are updating.
     * @param {String} oldId old id of fiberbox
     * @param {String} newId new id of fiberbox
     */
    function updateFiberboxBinding(fiberboxMarker, oldId, newId) {
        fiberboxMarker.marker.set("labelContent", newId);  // update fiberbox markers label.
        // Make sure that they are strings, some old markers may use integers.
        oldId = oldId.toString();
        newId = newId.toString();
        // loop and update all markers bound to this fiberbox and update their bindingvalue.
        markersArray.forEach(function(marker) {
            if (marker.optionalInfo &&
                marker.optionalInfo.KS &&
                marker.id !== fiberboxMarker.id &&  // make sure to not change self.
                marker.optionalInfo.KS.toString() === oldId) { // markers with our old id is bound to us, lets give them the new id.
                    marker.optionalInfo.KS = newId;
                    marker.marker.set("labelContent", newId);
            }
        });
    }

    /**
     * Called upon to validate editing of fiberbox before allowed to be persisted.
     * @param {Object} el DOM-element
     * @param {Object} validateMarker marker object
     * @return {boolean} whether the validation has passed.
     */
    privateFuncs.fiberboxValidator = function fiberboxValidator(el, validateMarker) {
        // pattern check.
        if (!el[0].validity.valid) {
            el.addClass("invalid");
            return false;
        }

        // check that no other KS has the same number("name"), they must all be unique.
        for (var i = 0, length = markersArray.length; i < length; i++) {
            if (markersArray[i].markerType.Name === MARKERTYPE.FiberBox) {
                if (markersArray[i].id !== validateMarker.id && markersArray[i].name === el.val()) {
                    el.addClass("invalid");
                    return false;
                }
            }
        }

        return true;
    };

    function addCable(id, uid, name, lineColor, width, coordinatesString, type) {
        var path = coordinatesString;
        // coordinatesString kan antingen vara ett färdigt MVCArray-objekt eller en sträng som skall konverteras till ett MVCArray-objekt.
        if (typeof coordinatesString === "string") {
            path = new google.maps.MVCArray();
            var coordinates = coordinatesString.split('|');

            for (var i = 0, length = coordinates.length; i < length; i++) {
                var coordinatePair = coordinates[i].split(':');
                path.push(new google.maps.LatLng(parseFloat(coordinatePair[0]), parseFloat(coordinatePair[1])));
            }
        }
        var line = new google.maps.Polyline({
            map: $("#show_network").is(":checked") ? map : null,
            path: path,
            strokeColor: '#' + lineColor,
            strokeOpacity: 0.7,
            strokeWeight: width,
            zIndex: type * 10,
            // Sparar ner dessa också, vi behöver dom när vi editerar linjen.
            id: id,
            type: type
        });

        if (mapContent.Settings.HasWritePrivileges) {
            google.maps.event.addListener(line, 'click', function(event) {
                // Vid första klicket på linjen (linjen väljs).
                if (currentSelectedObject !== this) {
                    if (currentSelectedObject) {
                        // Om ett annat object redan är vald, se till att den inte längre går att modifiera(om den har en sådan function).
                        if (typeof currentSelectedObject.setEditable === 'function') {
                            currentSelectedObject.setEditable(false);
                        }
                    }
                    currentSelectedObject = this;    // Och gör detta object till den valda.
                    currentSelectedObject.setEditable(true);    // Tillåt att flytta på linjens vertices.
                }
                    // Vid andra klicket på linjen (linjen är redan vald).
                else {
                    editCable(this);
                }
            });
            google.maps.event.addListener(line, 'dblclick', function(event) {
                // Om denna linje inte redan är den valda, välj linjen.
                if (currentSelectedObject !== this) {
                    if (currentSelectedObject) {
                        // Om ett annat object redan är vald, se till att den inte längre går att modifiera(om den har en sådan function).
                        if (typeof currentSelectedObject.setEditable === 'function') {
                            currentSelectedObject.setEditable(false);
                        }
                    }
                    currentSelectedObject = this;    // Och gör detta object till den valda.
                    currentSelectedObject.setEditable(true);    // Tillåt att flytta på linjens vertices.
                }

                editCable(this);
            });
            google.maps.event.addListener(line, 'rightclick', function(event) {
                // Visa contextmenyn bara om objektet är valt.
                if (currentSelectedObject === this) {
                    var positionXY = getCanvasXY(event.latLng);
                    $('#contextMenuLinePlaceholder').data('menuInfo', { id: this.id, vertex: event.vertex, latLng: event.latLng });
                    $('#contextMenuLinePlaceholder').contextMenu({ x: positionXY.x, y: positionXY.y });
                }
            });
        }

        lineArray.push({ id: id, uid: uid, name: name, desc: null, cable: line });
    }
    function editCable(self) {
        var clickedLine = getLineById(self.id);
        // Visa dialogruta för linjen.
        var $dialog = $('<div id="edit"></div>')
            .html(
                '<label for="name">Beteckning</label><br/>' +
                '<input id="name" type="text" maxlength="100" /><br/><br/>' +
                '<label for="desc">Beskrivning</label><br/>' +
                '<textarea id="desc" rows="6" cols="58"></textarea><br/>' +
                'Linjetyp:&nbsp;<select id="lineType" name="lineType"><option value="0">Schaktsträcka</option></select>' +
                '<p>Längd: ' + Math.ceil(google.maps.geometry.spherical.computeLength(clickedLine.cable.getPath())) + ' meter.</p>' +
                '<input id="okButton" type="button" value="Ok" /><input id="deleteButton" type="button" value="Radera" />'
            )
            .dialog({
                autoOpen: false,
                title: 'Redigerar ' + clickedLine.name,
                close: function() {
                    if (tinyMCE.getInstanceById('desc')) {
                        tinyMCE.execCommand('mceFocus', false, 'desc'); // IE fix.                   
                        tinyMCE.execCommand('mceRemoveControl', false, 'desc');   // Ta bort editor.
                    }
                    $(this).remove();
                },
                width: 430,
                modal: true,
                resizable: false,
                open: function() {
                    $("#name").val(clickedLine.name);
                    $("#lineType").val(clickedLine.cable.type);
                    tinyMCE.execCommand('mceAddControl', false, 'desc');  // Initialiserar editor.

                    // Laddar upp beskrivning om vi inte redan har gjort det.
                    if (clickedLine.desc === null) {
                        // Hämtar upp beskrivningen dynamiskt, det kostar för mycket bandbredd att ladda upp alla beskrivningar från början.
                        $.get(serverRoot + '/REST/FKService.svc/LineDescription/' + clickedLine.id, function(lineDescription) {
                            clickedLine.desc = lineDescription.Desc;  // Sparar undan orginalbeskrivningen.
                            $('#desc').val(lineDescription.Desc); // Sätt beskrivning i textarea och visa editor.
                            var instance = tinyMCE.getInstanceById('desc');
                            if (instance) {
                                instance.setContent(lineDescription.Desc);  // Sätter beskrivning i editor.
                            }
                        });
                    } else {
                        tinyMCE.getInstanceById('desc').setContent(clickedLine.desc);
                    }

                    $("#okButton").click(function(event) {
                        clickedLine.name = $("#name").val();
                        clickedLine.desc = tinyMCE.get('desc').getContent();
                        clickedLine.cable.type = $("#lineType").val();

                        $('#edit').dialog('close');
                    });
                    $("#deleteButton").click(function(event) {
                        removeLineById(clickedLine.id);
                        $('#edit').dialog('close');
                    });
                }
            });
        $dialog.dialog('open');
    }

    function addRegion(id, uid, name, borderColor, fillColor, coordinatesString) {
        var paths = coordinatesString;
        // coordinatesString kan antingen vara ett färdigt MVCArray-objekt eller en sträng som skall konverteras till ett MVCArray-objekt.
        if (typeof coordinatesString === "string") {
            paths = new google.maps.MVCArray();
            var coordinates = coordinatesString.split('|');

            for (var i = 0, length = coordinates.length; i < length; i++) {
                var coordinatePair = coordinates[i].split(':');
                paths.push(new google.maps.LatLng(parseFloat(coordinatePair[0]), parseFloat(coordinatePair[1])));
            }
        }
        var polygon = new google.maps.Polygon({
            map: $("#show_regions").is(":checked") ? map : null,
            paths: paths,
            strokeColor: '#' + borderColor,
            fillColor: '#' + fillColor,
            fillOpacity: 0.3,
            strokeWeight: 2,
            strokeOpacity: 0.5,
            // Sparar ner denna också, vi behöver den när vi editerar området.
            id: id
        });

        google.maps.event.addListener(polygon, 'mousemove', function(event) {
            showPointerPosition(event.latLng.lat(), event.latLng.lng());
        });

        if (mapContent.Settings.HasWritePrivileges) {
            google.maps.event.addListener(polygon, 'click', function(event) {
                // Vid första klicket på området (polygonen väljs).
                if (currentSelectedObject !== this) {
                    if (currentSelectedObject) {
                        // Om ett annat object redan är vald, se till att den inte längre går att modifiera(om den har en sådan function).
                        if (typeof currentSelectedObject.setEditable === 'function') {
                            currentSelectedObject.setEditable(false);
                        }
                    }
                    currentSelectedObject = this;    // Och gör detta object till den valda.
                    currentSelectedObject.setEditable(true);    // Tillåt att flytta på polygonens vertices.
                }
                    // Vid andra klicket på området (polygonen är redan vald).
                else {
                    editRegion(this);
                }
            });

            google.maps.event.addListener(polygon, 'dblclick', function(event) {
                // Om detta område inte redan är den valda, välj polygon.
                if (currentSelectedObject !== this) {
                    if (currentSelectedObject) {
                        // Om ett annat object redan är vald, se till att den inte längre går att modifiera(om den har en sådan function).
                        if (typeof currentSelectedObject.setEditable === 'function') {
                            currentSelectedObject.setEditable(false);
                        }
                    }
                    currentSelectedObject = this;    // Och gör detta object till den valda.
                    currentSelectedObject.setEditable(true);    // Tillåt att flytta på linjens vertices.
                }

                editRegion(this);
            });
            google.maps.event.addListener(polygon, 'rightclick', function(event) {
                // Visa contextmenyn bara om objektet är valt.
                if (currentSelectedObject === this) {
                    var positionXY = getCanvasXY(event.latLng);
                    $('#contextMenuRegionPlaceholder').data('menuInfo', { id: this.id, vertex: event.vertex, latLng: event.latLng });
                    $('#contextMenuRegionPlaceholder').contextMenu({ x: positionXY.x, y: positionXY.y });
                }
            });
        }

        regionsArray.push({ id: id, uid: uid, name: name, desc: null, region: polygon });
    }
    function editRegion(self) {
        var clickedRegion = getRegionById(self.id);
        // Visa dialogruta för området.
        var $dialog = $('<div id="edit"></div>')
            .html(
                '<label for="name">Namn på område</label><br/>' +
                '<input id="name" type="text" maxlength="100" /><br/><br/>' +
                '<label for="desc">Beskrivning</label><br/>' +
                '<textarea id="desc" rows="6" cols="58"></textarea>' +
                '<fieldset class="marginTop10px"><legend>Statistik</legend>' +
                '<p>Area: ' + Math.ceil(google.maps.geometry.spherical.computeArea(clickedRegion.region.getPath())) + ' meter&#178;.</p>' +
                '<p>' + rendertMarkerStatisticsWithinRegion(getMarkerStatisticsWithinRegion(clickedRegion.region)) + '</p></fieldset>' +
                '<div class="marginTop10px"><input id="okButton" type="button" value="Ok" /><input id="deleteButton" type="button" value="Radera" /></div>'
            )
            .dialog({
                autoOpen: false,
                title: 'Redigerar ' + clickedRegion.name,
                close: function() {
                    if (tinyMCE.getInstanceById('desc')) {
                        tinyMCE.execCommand('mceFocus', false, 'desc'); // IE fix.                   
                        tinyMCE.execCommand('mceRemoveControl', false, 'desc');   // Ta bort editor.
                    }
                    $(this).remove();
                },
                width: 430,
                modal: true,
                resizable: false,
                open: function() {
                    $("#name").val(clickedRegion.name);
                    tinyMCE.execCommand('mceAddControl', false, 'desc');  // Initialiserar editor.

                    // Laddar upp beskrivning om vi inte redan har gjort det.
                    if (clickedRegion.desc === null) {
                        // Hämtar upp beskrivningen dynamiskt, det kostar för mycket bandbredd att ladda upp alla beskrivningar från början.
                        $.get(serverRoot + '/REST/FKService.svc/RegionDescription/' + clickedRegion.id, function(regionDescription) {
                            clickedRegion.desc = regionDescription.Desc;  // Sparar undan orginalbeskrivningen.
                            $('#desc').val(regionDescription.Desc); // Sätt beskrivning i textarea och visa editor.
                            var instance = tinyMCE.getInstanceById('desc');
                            if (instance) {
                                instance.setContent(regionDescription.Desc);  // Sätter beskrivning i editor.
                            }
                        });
                    } else {
                        tinyMCE.getInstanceById('desc').setContent(clickedRegion.desc);
                    }

                    $("#okButton").click(function(event) {
                        clickedRegion.name = $("#name").val();
                        clickedRegion.desc = tinyMCE.get('desc').getContent();

                        $('#edit').dialog('close');
                    });
                    $("#deleteButton").click(function(event) {
                        removeRegionById(clickedRegion.id);
                        $('#edit').dialog('close');
                    });
                }
            });
        $dialog.dialog('open');
    }

    /**
     * Returns the next available id for a fiberbox.
     * @returns {String} id 
     */
    function getNextAvailableFiberBoxId() {
        var possibleFreeId = 1, foundFreeId = false;

        while (!foundFreeId) {
            foundFreeId = true;
            for (var i = 0, length = markersArray.length; i < length; i++) {
                if (markersArray[i].markerType.Name === MARKERTYPE.FiberBox) {
                    if (parseInt(markersArray[i].name, 10) === possibleFreeId) {
                        possibleFreeId++;
                        foundFreeId = false;
                        break;
                    }
                }
            }
        }
        return possibleFreeId.toString();
    }

    function calculateTotalLineLength(type) {
        var lineLength = 0;
        for (var i = 0, length = lineArray.length; i < length; i++) {
            if (lineArray[i].cable.type === type) {
                lineLength += google.maps.geometry.spherical.computeLength(lineArray[i].cable.getPath());
            }
        }

        return Math.ceil(lineLength);
    }

    function getMarkerStatisticsWithinRegion(region) {
        var result = {};

        for (var i = 0, length = markersArray.length; i < length; i++) {
            if (google.maps.geometry.poly.containsLocation(markersArray[i].marker.getPosition(), region)) {
                switch (markersArray[i].markerType.Name) {
                    case MARKERTYPE.HouseYes:
                        if (result.hasOwnProperty(MARKERTYPE.HouseYes)) {
                            result[MARKERTYPE.HouseYes].counter++;
                        } else {
                            result[MARKERTYPE.HouseYes] = { type: markersArray[i].markerType, counter: 1 };
                        }
                        break;
                    case MARKERTYPE.HouseMaybe:
                        if (result.hasOwnProperty(MARKERTYPE.HouseMaybe)) {
                            result[MARKERTYPE.HouseMaybe].counter++;
                        } else {
                            result[MARKERTYPE.HouseMaybe] = { type: markersArray[i].markerType, counter: 1 };
                        }
                        break;
                    case MARKERTYPE.HouseNo:
                        if (result.hasOwnProperty(MARKERTYPE.HouseNo)) {
                            result[MARKERTYPE.HouseNo].counter++;
                        } else {
                            result[MARKERTYPE.HouseNo] = { type: markersArray[i].markerType, counter: 1 };
                        }
                        break;
                    case MARKERTYPE.HouseNotContacted:
                        if (result.hasOwnProperty(MARKERTYPE.HouseNotContacted)) {
                            result[MARKERTYPE.HouseNotContacted].counter++;
                        } else {
                            result[MARKERTYPE.HouseNotContacted] = { type: markersArray[i].markerType, counter: 1 };
                        }
                        break;
                    case MARKERTYPE.FiberBox:
                        if (result.hasOwnProperty(MARKERTYPE.FiberBox)) {
                            result[MARKERTYPE.FiberBox].counter++;
                        } else {
                            result[MARKERTYPE.FiberBox] = { type: markersArray[i].markerType, counter: 1 };
                        }
                        break;
                }
            }
        }

        return result;
    }

    function rendertMarkerStatisticsWithinRegion(statistics) {
        var result = '';

        if (statistics[MARKERTYPE.HouseYes]) {
            result += 'Fastigheter som vill ha fiber: <strong>' + statistics[MARKERTYPE.HouseYes].counter + '</strong><br/>';
        }
        if (statistics[MARKERTYPE.HouseMaybe]) {
            result += 'Fastigheter som ännu inte har bestämt sig: <strong>' + statistics[MARKERTYPE.HouseMaybe].counter + '</strong><br/>';
        }
        if (statistics[MARKERTYPE.HouseNotContacted]) {
            result += 'Fastigheter som ännu inte har kontaktats: <strong>' + statistics[MARKERTYPE.HouseNotContacted].counter + '</strong><br/>';
        }
        if (statistics[MARKERTYPE.HouseNo]) {
            result += 'Fastigheter som inte är intresserade av fiber: <strong>' + statistics[MARKERTYPE.HouseNo].counter + '</strong><br/>';
        }
        if (statistics[MARKERTYPE.FiberBox]) {
            result += 'Kopplingsskåp/skarvbrunnar: <strong>' + statistics[MARKERTYPE.FiberBox].counter + '</strong><br/>';
        }

        return result;
    }

    /**
     * Sparar ändringar av karta.
     * @param {bool} publish Publicerar kartan efter den har sparats.
     */
    function saveChanges(publish) {

        var markers = [],
            lines = [],
            regions = [],
            i, length, coord, pathArray, pathIndex;

        showLoader('Sparar ' + (publish ? "och publicerar " : "") + 'karta...');
        try {
            for (i = 0, length = markersArray.length; i < length; i++) {
                var marker = {
                    Id: markersArray[i].id,
                    Uid: markersArray[i].uid,
                    Name: markersArray[i].name,
                    Desc: markersArray[i].desc,
                    MarkId: markersArray[i].markerType.Id,
                    Settings: markersArray[i].settings,
                    OptionalInfo: JSON.stringify(markersArray[i].optionalInfo),
                    Lat: markersArray[i].marker.getPosition().lat(),
                    Lng: markersArray[i].marker.getPosition().lng()
                };

                markers[markers.length] = marker;
            }

            for (i = 0, length = lineArray.length; i < length; i++) {
                coord = [];
                pathArray = lineArray[i].cable.getPath().getArray();

                // Två punkter/vertex behövs för att bilda en linje, filtrera bort de "linjer" som bara har en.
                if (pathArray.length > 1) {
                    // TODO: Lägg till någon kod som filtrerar bort noder som ligger för nära varandra (typ uppå varandra).
                    // Sådana får man lätt till när man ritar och det tar bara onödig lagringsplats och processorkraft när man ritar ut dessa igen.
                    for (pathIndex = 0; pathIndex < pathArray.length; pathIndex++) {
                        coord[coord.length] = {
                            Lat: pathArray[pathIndex].lat().toString(),
                            Lng: pathArray[pathIndex].lng().toString()
                        };
                    }
                    var line = {
                        Id: lineArray[i].id,
                        Uid: lineArray[i].uid,
                        Name: lineArray[i].name,
                        Desc: lineArray[i].desc,
                        Color: lineArray[i].cable.strokeColor.substring(1),
                        Width: lineArray[i].cable.strokeWeight,
                        Coord: coord,
                        Type: lineArray[i].cable.type
                    };

                    lines[lines.length] = line;
                }
            }

            for (i = 0, length = regionsArray.length; i < length; i++) {
                coord = [];
                pathArray = regionsArray[i].region.getPath().getArray();
                for (pathIndex = 0; pathIndex < pathArray.length; pathIndex++) {
                    coord[coord.length] = {
                        Lat: pathArray[pathIndex].lat().toString(),
                        Lng: pathArray[pathIndex].lng().toString()
                    };
                }
                var region = {
                    Id: regionsArray[i].id,
                    Uid: regionsArray[i].uid,
                    Name: regionsArray[i].name,
                    Desc: regionsArray[i].desc,
                    BorderColor: regionsArray[i].region.strokeColor.substring(1),
                    FillColor: regionsArray[i].region.fillColor.substring(1),
                    Coord: coord
                };

                regions[regions.length] = region;
            }

            var saveMapContent = {
                MapTypeId: mapContent.MapTypeId,
                Ver: mapContent.MapVer,
                Markers: markers,
                Cables: lines,
                Regions: regions
            };

            $.ajax({
                type: 'POST',
                url: serverRoot + '/REST/FKService.svc/SaveMap',
                data: '{"mapContent": ' + JSON.stringify(saveMapContent) + ', "publish": ' + !!publish + '}',
                contentType: 'application/json',
                dataType: 'json'
            })
            .done(function(result) {
                if (result.ErrorCode > 0) {
                    hideLoader();
                    alert(result.ErrorMessage);
                    ga('send', 'exception', {
                        'exDescription': result.ErrorMessage,
                        'exFatal': false
                    });
                } else {
                    var url = 'MapAdmin.aspx?mid=' + mapContent.MapTypeId +
                    '&ver=' + result.NewVersionNumber +
                    '&center=' + map.getCenter().lat() + 'x' + map.getCenter().lng() +
                    '&zoom=' + map.getZoom() +
                    '&houseyes=' + $("#show_house_to_install").is(":checked") +
                    '&houseno=' + $("#show_house_no_dice").is(":checked") +
                    '&network=' + $("#show_network").is(":checked") +
                    '&fibernodes=' + $("#show_fibernodes").is(":checked") +
                    '&fiberboxes=' + $("#show_fiberboxes").is(":checked") +
                    '&crossings=' + $("#show_crossings").is(":checked") +
                    '&regions=' + $("#show_regions").is(":checked");

                    window.location.href = url;
                }
            })
            .fail(function(XMLHttpRequest, textStatus, errorThrown) {
                hideLoader();
                alert("Ett fel uppstod vid sparningen av kartan.");
                ga('send', 'exception', {
                    'exDescription': errorThrown.message,
                    'exFatal': false
                });
            });
        }
        catch (error) {
            hideLoader();
            alert("Ett fel uppstod vid sparningen av kartan.");
            ga('send', 'exception', {
                'exDescription': error.message,
                'exFatal': false
            });
        }
    }

    // Konvertera latitud och logitud-objekt till skärm koordinater.
    function getCanvasXY(currentLatLng) {
        var scale = Math.pow(2, map.getZoom());
        var nw = new google.maps.LatLng(
          map.getBounds().getNorthEast().lat(),
          map.getBounds().getSouthWest().lng()
      );
        var worldCoordinateNW = map.getProjection().fromLatLngToPoint(nw);
        var worldCoordinate = map.getProjection().fromLatLngToPoint(currentLatLng);
        var currentLatLngOffset = new google.maps.Point(
          Math.floor((worldCoordinate.x - worldCoordinateNW.x) * scale),
          Math.floor((worldCoordinate.y - worldCoordinateNW.y) * scale)
    );

        return currentLatLngOffset;
    }

    function getPointInsertOrder(linePath, point) {
        var distance = function(a, b) {
            return Math.sqrt(Math.pow(a.lat() - b.lat(), 2) + Math.pow(a.lng() - b.lng(), 2));
        };
        var closestPoint = { distance: null, index: 0 };
        linePath.forEach(function(vertex, index) {
            var testDistance = distance(vertex, point);
            if (closestPoint.distance === null || testDistance < closestPoint.distance) {
                closestPoint.distance = testDistance;
                closestPoint.index = index;
            }
        });

        return closestPoint.index + 1;  // TODO: Här borde vi kolla om testpunkten är närmare föregående och efterföljande punkt på linjen för att avgöra om den nya punkten skall läggas in före eller efter testpunkten.
    }

    /**
     * Registrera Handlebars helpers som vi använder.
     */
    function setupHandlebarsHelpers() {

        /* För getMoreAddressDetails() photos
        Handlebars.registerHelper("photos", function(array) { 
            var list = '',
                smallImage,
                fullImage;

            if (array && array.length > 0) {
                for (var i = 0; i < array.length; i++) {
                    console.dir(array[i]);
                    console.log(array[i].getUrl());
                    fullImage = array[i].getUrl();
                    smallImage = array[i].getUrl(); //array[i].getUrl({'maxWidth': 35, 'maxHeight': 35});

                    list += '<a href="' + fullImage + '"><img src="' + smallImage + '" /></a>';
                }
            }

            return new Handlebars.SafeString(list);
        });*/

        Handlebars.registerHelper("gpsposition", function(latlng) {
            var position = '';

            if (latlng) {
                position = 'lat: ' + latlng.lat().toFixed(7) + ', long: ' + latlng.lng().toFixed(6);
            }

            return position;
        });
    }

    function reverseGeocodeMarker(statusLine, postion) {
        statusLine.html('Söker, var god vänta...');

        geocoder.geocode({ 'latLng': postion },
          function(results, status) {
              if (status === google.maps.GeocoderStatus.OK) {
                  statusLine.off(); // Ser till att man inte kan klicka flera gånger.
                  statusLine.removeClass('clickable');

                  if (results[0]) {
                      statusLine.html(results[0].formatted_address);
                  }
                  else {
                      statusLine.html('Sökning misslyckades.');
                  }
              }
              else {
                  statusLine.html('Sökning misslyckades.') + status;
              }
          });

        return false;
    }

    /**
     * Hämtar mer utömmande detaljer om en specifik  adress. Ett Google Places place_id behövs som paramer.
     * @param {string} placeId Google Places place_id.
     * @param {function} callback Callback som skall anropas för att leverera svaret.
     */
    function getMoreAddressDetails(placeId, callback) {

        placesService.getDetails({ placeId: placeId }, function(place, status) {

            if (status === google.maps.places.PlacesServiceStatus.OK) {
                var result = {};

                result.name = place.name;
                result.location = place.geometry.location;
                result.address = place.adr_address;
                result.phonenumber = place.formatted_phone_number;
                //result.photos = place.photos;
                result.website = place.website;

                callback(result);

            } else {
                callback();
            }
        });
    }
})(fk);
