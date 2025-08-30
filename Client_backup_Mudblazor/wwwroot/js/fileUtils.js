window.fileUtils = {
  save: function (filename, content, mime) {
    try {
      const blob = new Blob([content], { type: mime || 'text/plain;charset=utf-8' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = filename;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch (e) {
      console.error('fileUtils.save error', e);
    }
  },
  setLocal: function (key, value) {
    try { localStorage.setItem(key, value); } catch (e) { console.warn('localStorage set error', e); }
  },
  getLocal: function (key) {
    try { return localStorage.getItem(key); } catch (e) { console.warn('localStorage get error', e); return null; }
  }
};
