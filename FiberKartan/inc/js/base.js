
function showLoader(message) {
    hideLoader(); // Remove any existing loaders.
    $(document.body).append('<div class="loaderMask"></div><div class="loader"><p><strong>' + message + '</strong></p></div>');

    $('.loader').fadeIn(2000);
}

function hideLoader() {
    $('.loaderMask').remove();
    $('.loader').remove();
}

function showDialog(content, title) {
    $('<div id="popup">' + content + '</div>').dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        title: title,
        minWidth: 450,
        buttons: {
            "Ok": function () {
                $(this).dialog("close");
            }
        },
        close: function () {
            // Rensa bort rutan ifall vi vill visa en ny.
            $(this).dialog("destroy");
            $(this).remove();
        },
        overlay: {
            opacity: 0.5,
            background: "black"
        }
    }).dialog("open");
}