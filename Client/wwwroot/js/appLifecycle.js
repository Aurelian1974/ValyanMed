(function () {
  function clearStorage(keys) {
    try {
      if (!keys || !Array.isArray(keys) || keys.length === 0) {
        localStorage.clear();
        sessionStorage.clear();
      } else {
        keys.forEach(k => localStorage.removeItem(k));
      }
    } catch (e) { /* no-op */ }
  }

  async function postLogout(logoutUrl, token) {
    try {
      if (!logoutUrl) return;
      // Try sendBeacon first (works on unload)
      if (navigator.sendBeacon) {
        const blob = new Blob([JSON.stringify({ reason: 'page_exit' })], { type: 'application/json' });
        navigator.sendBeacon(logoutUrl, blob);
        return;
      }
      // Fallback fetch (may not complete on unload)
      await fetch(logoutUrl, { method: 'POST', keepalive: true, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ reason: 'page_exit' }) });
    } catch { /* ignore */ }
  }

  function register(options) {
    // Handler pentru închiderea aplica?iei/browser-ului
    window.addEventListener('beforeunload', function(event) {
      try {
        // Cur???m localStorage înainte de închidere
        if (options && options.storageKeys && Array.isArray(options.storageKeys)) {
          clearStorage(options.storageKeys);
        } else {
          // Cur???m specific key-urile ValyanMed
          clearStorage(['valyanmed_auth_token', 'valyanmed_user_info', 'currentUser', 'auth_token', 'auth_user']);
        }
        
        // Opcional: trimitem logout request
        if (options && options.logoutUrl) {
          postLogout(options.logoutUrl);
        }
      } catch (e) {
        console.log('Error during beforeunload cleanup:', e);
      }
    });

    // Handler pentru schimbarea vizibilit??ii (tab switch, minimize)
    document.addEventListener('visibilitychange', function() {
      if (document.hidden) {
        // Salv?m starea pentru a ?ti c? aplica?ia a fost minimizat?
        localStorage.setItem('valyanmed_was_hidden', Date.now().toString());
      }
    });
  }

  async function logoutNow(logoutUrl, storageKeys) {
    await postLogout(logoutUrl);
    clearStorage(storageKeys);
  }

  window.appLifecycle = {
    register,
    logoutNow
  };
})();
