var UnoAppManifest = {

    splashScreenImage: "Assets/SplashScreen.png",
    splashScreenColor: "#050505",
    displayName: "DCSSTV"

}

function toggleFullScreen() {
    if (!document.fullscreenElement &&    // alternative standard method
        !document.mozFullScreenElement && !document.webkitFullscreenElement && !document.msFullscreenElement) {  // current working methods
        if (document.documentElement.requestFullscreen) {
            document.documentElement.requestFullscreen();
        } else if (document.documentElement.msRequestFullscreen) {
            document.documentElement.msRequestFullscreen();
        } else if (document.documentElement.mozRequestFullScreen) {
            document.documentElement.mozRequestFullScreen();
        } else if (document.documentElement.webkitRequestFullscreen) {
            document.documentElement.webkitRequestFullscreen(Element.ALLOW_KEYBOARD_INPUT);
        }
    } else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        } else if (document.mozCancelFullScreen) {
            document.mozCancelFullScreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        }
    }
}

function viewportSet(id) {
    var viewports = {
        default: "width=device-width, initial-scale=1",
        zoom: "width=device-width, initial-scale=0.86, maximum-scale=5.0, minimum-scale=0.86"
    };
    var viewport_meta = document.getElementsByName("viewport")[0];
    if (id == 0)
        viewport_meta.setAttribute('content', viewports.default);
    else
        viewport_meta.setAttribute('content', viewports.zoom);
}



