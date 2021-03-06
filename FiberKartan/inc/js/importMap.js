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
$(document).ready(function() {
    if (Modernizr.draganddrop) {
        $('.fileUpload').addClass('dropZone');
        $('.fileUpload .dropZoneText').show();
    }

    $('#mergeVersion').change(function() {
        if ($(this).val() === "0") {
            $('#includeOldLines').attr("disabled", "disabled");
            $('#includeOldMarkers').attr("disabled", "disabled");
        } else {
            $('#includeOldLines').removeAttr("disabled");
            $('#includeOldMarkers').removeAttr("disabled");
        }
    });

    $('#ImportButton').click(function() {
        showLoader('Importerar karta...');
    });    
});