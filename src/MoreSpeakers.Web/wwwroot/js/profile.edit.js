document.addEventListener('DOMContentLoaded', function() {
    // Initialize any additional functionality

    jQuery.validator.addMethod("isimgurl", function (value, element) {
        
        let isValid = false;
        
        $.ajax({
            type: "HEAD",
            async: false,
            url: value,
            success: function(data, textStatus, jqXHR) {
                  let header = jqXHR.getResponseHeader('content-type');
                  isValid = !!header.includes('image');
            },
            error: function(jqXHR, textStatus, errorThrown) {
                isValid = false;
            }}
        );
        return isValid;
    }, "Could not verify that this URL is a valid image");
    
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
    // Update active tab state
    const tabs = document.querySelectorAll('#profileTabs .nav-link');
    tabs.forEach(tab => tab.classList.remove('active'));

    if (event.detail.xhr.responseURL.includes('tab=password')) {
        tabs[1].classList.add('active');
    } else {
        tabs[0].classList.add('active');
    }
});

async function isImgUrl(url) {
    
}
