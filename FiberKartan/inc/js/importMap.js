$(document).ready(function () {
    if (Modernizr.draganddrop) {
        $('.fileUpload').addClass('dropZone');
        $('.fileUpload .dropZoneText').show();
    }

    $('#mergeVersion').change(function () {
        if ($(this).val() == "0") {
            $('#includeOldLines').attr("disabled", "disabled");
            $('#includeOldMarkers').attr("disabled", "disabled");
        } else {
            $('#includeOldLines').removeAttr("disabled");
            $('#includeOldMarkers').removeAttr("disabled");
        }
    });

    $('#ImportButton').click(function () {
        showLoader('Importerar karta...');
    });    
});