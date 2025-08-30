(function(){
  function getDropdown(){
    return document.querySelector('.pagination-dropdown');
  }
  function isOpen() {
    const dd = getDropdown();
    return dd && dd.style.display !== 'none' && dd.offsetParent !== null;
  }
  function position(anchor){
    const dd = getDropdown();
    if(!dd || !anchor) return;
    const r = anchor.getBoundingClientRect();
    const top = r.bottom + window.scrollY + 4; // 4px offset
    const left = r.left + window.scrollX;
    dd.style.position = 'absolute';
    dd.style.top = `${top}px`;
    dd.style.left = `${left}px`;
    dd.style.minWidth = `${r.width}px`;
    dd.style.zIndex = 3000;
  }
  function setup(anchor){
    if(!anchor) return;
    const handler = () => {
      setTimeout(() => position(anchor), 0);
    };
    anchor.addEventListener('click', handler, true);

    const onScroll = () => { if(isOpen()) position(anchor); };
    window.addEventListener('scroll', onScroll, true);
    window.addEventListener('resize', onScroll);
  }

  window.paginationDropdown = {
    init: function(anchorSelector){
      const anchor = typeof anchorSelector === 'string' ? document.querySelector(anchorSelector) : anchorSelector;
      if(!anchor) return;
      setup(anchor);
    }
  };
})();
