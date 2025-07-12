// Direct browser close handler - no .NET interop needed
window.addEventListener('beforeunload', function (e) {
    console.log("Browser closing - clearing auth data directly");
    localStorage.removeItem('authToken');
    localStorage.removeItem('userName');
    localStorage.removeItem('userFullName');
});

// Simple test function for verifying JS interop
window.testInterop = function() {
    console.log("JS interop is working!");
    return true;
};