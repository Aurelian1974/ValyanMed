window.browserInterop = {
    registerBeforeUnload: function (dotnetHelper) {
        console.log("Registering browser close event handlers");
        
        window.addEventListener('beforeunload', function (event) {
            console.log("Browser beforeunload event triggered");
            dotnetHelper.invokeMethodAsync('OnBeforeUnload');
        });
        
        window.addEventListener('unload', function (event) {
            console.log("Browser unload event triggered");
            // Direct cleanup as a fallback since async calls might not complete during page unload
            localStorage.removeItem('authToken');
            localStorage.removeItem('userName');
            localStorage.removeItem('userFullName');
        });
    }
};