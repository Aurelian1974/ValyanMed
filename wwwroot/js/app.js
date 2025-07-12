// Simple test function to verify JS interop is working
window.appInterop = {
    sayHello: function (name) {
        console.log("Hello, " + name);
        return "Hello, " + name;
    },
    
    // Basic logout function that doesn't rely on complex interop
    logoutOnClose: function() {
        console.log("Browser closing - clearing authentication data");
        localStorage.removeItem('authToken');
        localStorage.removeItem('userName');
        localStorage.removeItem('userFullName');
        return true;
    }
};

// Add a direct unload handler that doesn't rely on .NET interop
window.addEventListener('beforeunload', function (e) {
    console.log("Browser is closing - clearing auth data directly");
    localStorage.removeItem('authToken');
    localStorage.removeItem('userName');
    localStorage.removeItem('userFullName');
});