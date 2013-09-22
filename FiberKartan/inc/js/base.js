
function showLoader(message) {
    hideLoader(); // Remove any existing loaders.
    $(document.body).append('<div class="loaderMask"></div><div class="loader"><p><strong>' + message + '</strong></p></div>');

    $('.loader').fadeIn(2000);
}

function hideLoader() {
    $('.loaderMask').remove();
    $('.loader').remove();
}