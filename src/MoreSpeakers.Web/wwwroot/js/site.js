// MoreSpeakers.com JavaScript Utilities

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeTooltips();
    initializeFormValidation();
    initializeHtmxEnhancements();
    initializeFadeInAnimations();
    $("img.speaker-img").on("error", function() {
        fixMissingSpeakerImage(this);
    })
});

// Bootstrap tooltips initialization
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Enhanced form validation
function initializeFormValidation() {
    const forms = document.querySelectorAll('form.data-validate');

    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            if (!validateForm(this)) {
                e.preventDefault();
                e.stopPropagation();
            }
            this.classList.add('was-validated');
        });

        // Real-time validation
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            input.addEventListener('blur', function () {
                validateField(this);
            });
        });
    });

    if ($.validator) {
        jQuery.validator.addMethod("is-img-url", function (value, element) {
    
            let isValid = false;
    
            $.ajax({
                    type: "HEAD",
                    async: false,
                    url: value,
                    success: function (data, textStatus, jqXHR) {
                        let header = jqXHR.getResponseHeader('content-type');
                        isValid = header && header.includes('image');
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        isValid = false;
                    }
                }
            );
            return this.optional(element) || isValid;
        }, "Could not verify that this URL is a valid image");
    }


}

// Validate individual form field
function validateField(field) {
    const isValid = field.checkValidity();
    const feedback = field.parentNode.querySelector('.invalid-feedback');

    if (!isValid && feedback) {
        feedback.classList.add('d-block');
        feedback.classList.remove('d-none');
    } else if (feedback) {
        feedback.classList.add('d-none');
        feedback.classList.remove('d-block');
    }

    const submitBtn = feedback.closest('form').querySelector('button[type="submit"]');
    if (isValid) {
        submitBtn.disabled = false;
    } else {
        submitBtn.disabled = true;
    }

    return isValid;
}

// Validate the entire form
function validateForm(form) {
    let isValid = true;
    const fields = form.querySelectorAll('input, textarea, select');

    fields.forEach(field => {
        if (!validateField(field)) {
            isValid = false;
        }
    });

    // Custom validations
    if (form.id === 'registerForm') {
        isValid = validateRegistrationForm(form) && isValid;
    }

    return isValid;
}

// HTMX enhancements
function initializeHtmxEnhancements() {
    if (typeof htmx === 'undefined') return;

    // Global HTMX event handlers
    document.body.addEventListener('htmx:beforeRequest', function(evt) {
        // Show loading indicators
        const indicator = document.querySelector('#loading, .loading-indicator');
        if (indicator) {
            indicator.style.display = 'block';
        }

        // Disable form buttons to prevent double submission
        const form = evt.detail.elt.closest('form');
        if (form) {
            const buttons = form.querySelectorAll('button[type="submit"]');
            buttons.forEach(btn => {
                btn.disabled = true;
                btn.innerHTML = '<span class="loading me-2"></span>' + btn.innerHTML;
            });
        }
    });

    document.body.addEventListener('htmx:afterRequest', function(evt) {
        // Hide loading indicators
        const indicator = document.querySelector('#loading, .loading-indicator');
        if (indicator) {
            indicator.style.display = 'none';
        }

        // Re-enable form buttons
        const form = evt.detail.elt.closest('form');
        if (form) {
            const buttons = form.querySelectorAll('button[type="submit"]');
            buttons.forEach(btn => {
                btn.disabled = false;
                btn.removeChild(btn.firstChild);
            });
        }
    });

    document.body.addEventListener('htmx:responseError', function(evt) {
        showAlert('An error occurred. Please try again.', 'danger');
    });

    document.body.addEventListener('htmx:afterSwap', function(evt) {
        // Reinitialize components for new content
        initializeTooltips();

        // Animate new content
        if (evt.detail.target) {
            evt.detail.target.classList.add('fade-in');
        }
    });
}

// Utility function to show alerts
function showAlert(message, type = 'info', duration = 5000) {
    const alertContainer = document.getElementById('alert-container') || createAlertContainer();

    const alertId = 'alert-' + Date.now();
    const alertHtml = `
        <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    alertContainer.insertAdjacentHTML('beforeend', alertHtml);

    // Auto-dismiss after duration
    if (duration > 0) {
        setTimeout(() => {
            const alert = document.getElementById(alertId);
            if (alert) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, duration);
    }
}

// Create an alert container if it doesn't exist
function createAlertContainer() {
    const container = document.createElement('div');
    container.id = 'alert-container';
    container.className = 'position-fixed top-0 end-0 p-3';
    container.style.zIndex = '1055';
    document.body.appendChild(container);
    return container;
}

// Initialize fade-in animations using Intersection Observer
function initializeFadeInAnimations() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
            }
        });
    }, observerOptions);

    // Observe cards and sections for fade-in effect
    document.querySelectorAll('.card, section').forEach(element => {
        observer.observe(element);
    });
}

function fixMissingSpeakerImage(image) {

    if (image.nodeName.toLowerCase() !== "img" || image.naturalWidth > 0 || image.naturalHeight > 0) {
        return;
    }

    const imageParent = image.parentNode;
    const placeHolderDiv = document.createElement('div');
    placeHolderDiv.className = "align-items-center justify-content-center";
    const placeHolder = document.createElement('i');
    placeHolder.className = "bi bi-person-fill d-flex justify-content-center align-items-center";
    placeHolder.style.fontSize = "8rem";
    placeHolderDiv.appendChild(placeHolder);
    imageParent.replaceChild(placeHolderDiv, image);
}
