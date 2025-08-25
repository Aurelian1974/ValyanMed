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

  // Eliminat handlerul global care ?terge storage la pagehide/beforeunload/visibilitychange
  function register(options) {
    // Nu mai ata??m niciun handler automat
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
