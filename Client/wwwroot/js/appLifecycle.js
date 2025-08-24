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
    const cfg = Object.assign({
      logoutUrl: '',
      storageKeys: ['valyanmed_auth_token', 'valyanmed_user_info', 'currentUser']
    }, options || {});

    // Handle pagehide/visibilitychange for mobile Safari reliability
    const handler = () => {
      try {
        clearStorage(cfg.storageKeys);
        postLogout(cfg.logoutUrl);
      } catch { /* ignore */ }
    };

    window.addEventListener('pagehide', handler, { capture: true });
    window.addEventListener('beforeunload', handler, { capture: true });
    document.addEventListener('visibilitychange', function () {
      if (document.visibilityState === 'hidden') handler();
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
