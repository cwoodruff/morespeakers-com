document.addEventListener('DOMContentLoaded', function() {
    // Initialize any additional functionality
    
    // Auto-hide success messages after 5 seconds
    setTimeout(function() {
        const successAlerts = document.querySelectorAll('.alert-success');
        successAlerts.forEach(function(alert) {
            if (alert) {
                alert.style.transition = 'opacity 0.5s ease';
                alert.style.opacity = '0';
                setTimeout(() => alert.remove(), 500);
            }
        });
    }, 5000);
});

// Handle tab switching with HTMX
document.addEventListener('htmx:afterRequest', function(event) {
    const trigger = event.detail.elt;

    // Only update tabs if the request was successful and triggered by a tab link
    if (event.detail.successful && trigger && trigger.classList.contains('nav-link') && trigger.closest('#profileTabs')) {
        // Remove active class from all tabs
        const tabs = document.querySelectorAll('#profileTabs .nav-link');
        tabs.forEach(tab => tab.classList.remove('active'));

        // Add active class to the clicked tab
        trigger.classList.add('active');
    }
});

