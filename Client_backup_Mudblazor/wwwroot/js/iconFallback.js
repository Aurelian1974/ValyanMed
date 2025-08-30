// Icon fallback handler for patient list
(function() {
    'use strict';
    
    // Check if Material Icons are loaded
    function checkMaterialIconsLoaded() {
        const testElement = document.createElement('span');
        testElement.className = 'material-icons';
        testElement.style.position = 'absolute';
        testElement.style.left = '-9999px';
        testElement.style.visibility = 'hidden';
        testElement.textContent = 'home';
        document.body.appendChild(testElement);
        
        const computed = window.getComputedStyle(testElement);
        const fontFamily = computed.fontFamily;
        
        document.body.removeChild(testElement);
        
        // Check if Material Icons font is actually loaded
        return fontFamily.toLowerCase().includes('material icons');
    }
    
    // Fallback icon mappings
    const iconFallbacks = {
        'visibility': '??',
        'edit': '??',
        'event_available': '??',
        'assignment': '??',
        'search': '??',
        'clear': '?',
        'file_download': '??',
        'view_compact': '?',
        'view_agenda': '??'
    };
    
    // Apply fallback icons
    function applyIconFallbacks() {
        const materialIcons = document.querySelectorAll('.material-icons');
        materialIcons.forEach(icon => {
            const iconText = icon.textContent.trim();
            if (iconFallbacks[iconText]) {
                icon.textContent = iconFallbacks[iconText];
                icon.style.fontFamily = 'inherit';
                icon.style.fontSize = '16px';
            }
        });
    }
    
    // Enhanced patient action buttons with text labels
    function enhancePatientActionButtons() {
        const actionContainers = document.querySelectorAll('.patient-action-buttons');
        actionContainers.forEach(container => {
            const buttons = container.querySelectorAll('.mud-icon-button');
            buttons.forEach((button, index) => {
                const tooltipText = button.closest('[data-mud-tooltip]')?.getAttribute('data-mud-tooltip') || '';
                const labels = ['View', 'Edit', 'Schedule', 'History'];
                
                // Add data attribute for CSS fallback
                button.setAttribute('data-label', labels[index] || 'Action');
                
                // If icons are missing, add text content
                const icon = button.querySelector('.material-icons');
                if (icon && (!icon.textContent || icon.textContent.trim() === '')) {
                    button.innerHTML = `<span style="font-size: 10px; font-weight: bold;">${labels[index] || 'ACT'}</span>`;
                }
            });
        });
    }
    
    // Initialize when DOM is ready
    function initialize() {
        console.log('Checking Material Icons availability...');
        
        if (!checkMaterialIconsLoaded()) {
            console.warn('Material Icons not loaded, applying fallbacks...');
            applyIconFallbacks();
            
            // Mark containers as having failed icons
            const actionContainers = document.querySelectorAll('.patient-action-buttons');
            actionContainers.forEach(container => {
                container.classList.add('icons-failed');
            });
        }
        
        // Enhance buttons regardless
        enhancePatientActionButtons();
    }
    
    // Wait for fonts to load or timeout after 3 seconds
    if (document.fonts && document.fonts.ready) {
        document.fonts.ready.then(() => {
            setTimeout(initialize, 100);
        }).catch(() => {
            console.warn('Font loading failed, applying fallbacks immediately');
            initialize();
        });
    } else {
        // Fallback for older browsers
        setTimeout(initialize, 1000);
    }
    
    // Re-check periodically in case content is dynamically loaded
    setInterval(() => {
        const iconsWithoutContent = document.querySelectorAll('.material-icons:empty, .material-icons[style*="font-family: inherit"]');
        if (iconsWithoutContent.length > 0) {
            console.log('Found empty icons, re-applying fallbacks...');
            applyIconFallbacks();
            enhancePatientActionButtons();
        }
    }, 2000);
    
    // Export functions for manual use
    window.iconFallback = {
        check: checkMaterialIconsLoaded,
        apply: applyIconFallbacks,
        enhance: enhancePatientActionButtons,
        init: initialize
    };
    
})();