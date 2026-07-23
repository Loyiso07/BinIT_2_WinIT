(function () {
    'use strict';

    const darkModeToggle = document.getElementById('darkModeToggle');
    const darkModeIcon = document.getElementById('darkModeIcon');

    // Check saved preference
    const isDarkMode = localStorage.getItem('darkMode') === 'true';

    // Apply saved preference
    if (isDarkMode) {
        document.body.classList.add('dark-mode');
        if (darkModeIcon) {
            darkModeIcon.classList.remove('fa-moon');
            darkModeIcon.classList.add('fa-sun');
            darkModeIcon.style.color = '#FFD54F';
        }
    }

    // Toggle dark mode
    if (darkModeToggle) {
        darkModeToggle.addEventListener('click', function () {
            document.body.classList.toggle('dark-mode');
            const isDark = document.body.classList.contains('dark-mode');

            localStorage.setItem('darkMode', isDark);

            if (darkModeIcon) {
                if (isDark) {
                    darkModeIcon.classList.remove('fa-moon');
                    darkModeIcon.classList.add('fa-sun');
                    darkModeIcon.style.color = '#FFD54F';
                } else {
                    darkModeIcon.classList.remove('fa-sun');
                    darkModeIcon.classList.add('fa-moon');
                    darkModeIcon.style.color = '';
                }
            }
        });
    }

    // Auto-detect system preference (optional - uncomment to enable)
    /*
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)');
    if (prefersDark.matches && !localStorage.getItem('darkMode')) {
        document.body.classList.add('dark-mode');
        localStorage.setItem('darkMode', 'true');
        if (darkModeIcon) {
            darkModeIcon.classList.remove('fa-moon');
            darkModeIcon.classList.add('fa-sun');
            darkModeIcon.style.color = '#FFD54F';
        }
    }
    */
})();