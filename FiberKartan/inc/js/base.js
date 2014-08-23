
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