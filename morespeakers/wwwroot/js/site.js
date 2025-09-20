// MoreSpeakers.com JavaScript Utilities
// This file contains minimal JavaScript to enhance the user experience

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeTooltips();
    initializeFileUploads();
    initializeFormValidation();
    initializeSearchEnhancements();
    initializeHtmxEnhancements();
});

// Bootstrap tooltips initialization
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Enhanced file upload handling
function initializeFileUploads() {
    const fileUploadAreas = document.querySelectorAll('.file-upload-area');

    fileUploadAreas.forEach(area => {
        const input = area.querySelector('input[type="file"]');
        if (!input) return;

        // Click to upload
        area.addEventListener('click', (e) => {
            if (e.target === input) return;
            input.click();
        });

        // Drag and drop
        area.addEventListener('dragover', (e) => {
            e.preventDefault();
            area.classList.add('dragover');
        });

        area.addEventListener('dragleave', (e) => {
            e.preventDefault();
            area.classList.remove('dragover');
        });

        area.addEventListener('drop', (e) => {
            e.preventDefault();
            area.classList.remove('dragover');

            const files = e.dataTransfer.files;
            if (files.length > 0) {
                input.files = files;
                handleFileSelection(input, files[0]);
            }
        });

        // File selection change
        input.addEventListener('change', function() {
            if (this.files.length > 0) {
                handleFileSelection(this, this.files[0]);
            }
        });
    });
}

// Handle file selection display
function handleFileSelection(input, file) {
    const container = input.closest('.file-upload-area');
    if (!container) return;

    // Validate file type and size
    const maxSize = 5 * 1024 * 1024; // 5MB
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];

    if (!allowedTypes.includes(file.type)) {
        showAlert('Please select a valid image file (JPG, PNG, GIF).', 'danger');
        input.value = '';
        return;
    }

    if (file.size > maxSize) {
        showAlert('File size must be less than 5MB.', 'danger');
        input.value = '';
        return;
    }

    // Show preview for images
    if (file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = function(e) {
            container.innerHTML = `
                <div class="file-preview">
                    <img src="${e.target.result}" alt="Preview" class="preview-image mb-2">
                    <p class="mb-2"><strong>${file.name}</strong></p>
                    <p class="text-muted small">${formatFileSize(file.size)}</p>
                    <button type="button" class="btn btn-sm btn-outline-secondary" onclick="clearFileUpload(this)">
                        <i class="bi bi-trash"></i> Remove
                    </button>
                </div>
            `;
        };
        reader.readAsDataURL(file);
    }
}

// Clear file upload
function clearFileUpload(button) {
    const container = button.closest('.file-upload-area');
    const input = container.querySelector('input[type="file"]');

    input.value = '';
    container.innerHTML = `
        <i class="bi bi-cloud-upload display-4 text-muted mb-2"></i>
        <p class="mb-2">Click to upload or drag and drop</p>
        <p class="small text-muted">PNG, JPG, GIF up to 5MB</p>
    `;

    // Re-add the input
    const newInput = input.cloneNode(true);
    container.appendChild(newInput);
}

// Format file size for display
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

// Enhanced form validation
function initializeFormValidation() {
    const forms = document.querySelectorAll('form[data-validate]');

    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            if (!validateForm(this)) {
                e.preventDefault();
                e.stopPropagation();
            }
            this.classList.add('was-validated');
        });

        // Real-time validation
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            input.addEventListener('blur', function() {
                validateField(this);
            });
        });
    });
}

// Validate individual form field
function validateField(field) {
    const isValid = field.checkValidity();
    const feedback = field.parentNode.querySelector('.invalid-feedback');

    if (!isValid && feedback) {
        feedback.style.display = 'block';
    } else if (feedback) {
        feedback.style.display = 'none';
    }

    return isValid;
}

// Validate entire form
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

// Custom validation for registration form
function validateRegistrationForm(form) {
    let isValid = true;

    // Check if at least one expertise is selected
    const expertiseInputs = form.querySelectorAll('input[name="Input.SelectedExpertiseIds"]:checked, input[name="Input.CustomExpertise"]:checked');
    if (expertiseInputs.length === 0) {
        showAlert('Please select at least one area of expertise.', 'warning');
        isValid = false;
    }

    // Check if at least one social media link is provided
    const socialPlatforms = form.querySelectorAll('select[name^="Input.SocialMediaPlatforms"]');
    const socialUrls = form.querySelectorAll('input[name^="Input.SocialMediaUrls"]');
    let hasSocialMedia = false;

    for (let i = 0; i < socialPlatforms.length; i++) {
        if (socialPlatforms[i].value && socialUrls[i] && socialUrls[i].value) {
            hasSocialMedia = true;
            break;
        }
    }

    if (!hasSocialMedia) {
        showAlert('Please provide at least one social media link.', 'warning');
        isValid = false;
    }

    return isValid;
}

// Search enhancements
function initializeSearchEnhancements() {
    const searchInputs = document.querySelectorAll('input[type="search"], input[data-search]');

    searchInputs.forEach(input => {
        let timeout;

        input.addEventListener('input', function() {
            clearTimeout(timeout);
            const form = this.closest('form');

            if (form && form.hasAttribute('data-auto-submit')) {
                timeout = setTimeout(() => {
                    if (typeof htmx !== 'undefined') {
                        htmx.trigger(form, 'submit');
                    } else {
                        form.submit();
                    }
                }, 500);
            }
        });
    });
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
                btn.innerHTML = '<span class="loading me-2"></span>' + btn.textContent;
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
                btn.innerHTML = btn.textContent.replace(/^.*?(\w)/, '$1');
            });
        }
    });

    document.body.addEventListener('htmx:responseError', function(evt) {
        showAlert('An error occurred. Please try again.', 'danger');
    });

    document.body.addEventListener('htmx:afterSwap', function(evt) {
        // Reinitialize components for new content
        initializeTooltips();
        initializeFileUploads();

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

// Create alert container if it doesn't exist
function createAlertContainer() {
    const container = document.createElement('div');
    container.id = 'alert-container';
    container.className = 'position-fixed top-0 end-0 p-3';
    container.style.zIndex = '1055';
    document.body.appendChild(container);
    return container;
}

// Utility function for debouncing
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Smooth scroll to element
function scrollToElement(elementId, offset = 0) {
    const element = document.getElementById(elementId);
    if (element) {
        const top = element.offsetTop - offset;
        window.scrollTo({
            top: top,
            behavior: 'smooth'
        });
    }
}

// Copy text to clipboard
async function copyToClipboard(text) {
    try {
        await navigator.clipboard.writeText(text);
        showAlert('Copied to clipboard!', 'success', 2000);
    } catch (err) {
        // Fallback for older browsers
        const textArea = document.createElement('textarea');
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
        showAlert('Copied to clipboard!', 'success', 2000);
    }
}

// Format date for display
function formatDate(dateString, options = {}) {
    const date = new Date(dateString);
    const defaultOptions = {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    };
    return date.toLocaleDateString('en-US', { ...defaultOptions, ...options });
}

// Export functions for global use
window.MoreSpeakers = {
    showAlert,
    copyToClipboard,
    formatDate,
    scrollToElement,
    debounce,
    clearFileUpload
};