// MoreSpeakers.com JavaScript Utilities
// This file contains minimal JavaScript to enhance the user experience

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeTooltips();
    initializeFileUploads();
    initializeFormValidation();
    initializeSearchEnhancements();
    initializeHtmxEnhancements();
    initializeFadeInAnimations();
    
    // Initialize page-specific functionality
    if (document.getElementById('registerForm')) {
        initializeRegistrationForm();
    }
    if (document.querySelector('.edit-btn')) {
        initializeAccountPage();
    }
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

// Registration form specific functions
function handleEmailValidation() {
    document.body.addEventListener('htmx:afterRequest', function(evt) {
        if (evt.detail.target.id === 'email-validation-message') {
            const response = JSON.parse(evt.detail.xhr.responseText);
            const messageDiv = document.getElementById('email-validation-message');
            const emailInput = document.querySelector('input[name="Input.Email"]');
            
            if (response.isValid) {
                messageDiv.innerHTML = response.message ? '<i class="bi bi-check-circle me-1"></i>' + response.message : '';
                messageDiv.className = 'small mt-1 text-success';
                emailInput.classList.remove('is-invalid');
                emailInput.classList.add('is-valid');
            } else {
                messageDiv.innerHTML = '<i class="bi bi-exclamation-triangle me-1"></i>' + response.message;
                messageDiv.className = 'small mt-1 text-danger';
                emailInput.classList.remove('is-valid');
                emailInput.classList.add('is-invalid');
            }
        }
    });
}

function initializeCustomExpertise() {
    window.expertiseCounter = 1;
}

function addCustomExpertise() {
    const container = document.getElementById('customExpertiseContainer');
    const newInputGroup = document.createElement('div');
    newInputGroup.className = 'input-group mb-2 custom-expertise-input';
    newInputGroup.setAttribute('data-expertise-index', window.expertiseCounter);
    
    newInputGroup.innerHTML = `
        <input type="text" name="Input.CustomExpertise" class="form-control custom-expertise-field" 
               placeholder="Enter a custom expertise area"
               hx-post="/Identity/Account/Register?handler=ValidateCustomExpertise"
               hx-trigger="blur, keyup changed delay:500ms"
               hx-target="next .custom-expertise-validation"
               hx-include="this"
               hx-indicator="next .custom-expertise-validation .htmx-indicator" />
        <button type="button" class="btn btn-outline-danger" onclick="removeCustomExpertise(this)">
            <i class="bi bi-trash"></i>
        </button>
        <div class="custom-expertise-validation position-relative">
            <div class="htmx-indicator position-absolute" style="top: 8px; right: 8px;">
                <div class="spinner-border spinner-border-sm text-primary" role="status">
                    <span class="visually-hidden">Checking...</span>
                </div>
            </div>
        </div>
    `;
    
    container.appendChild(newInputGroup);
    
    const validationDiv = document.createElement('div');
    validationDiv.id = `customExpertiseValidation${window.expertiseCounter}`;
    validationDiv.className = 'custom-expertise-message small mt-1';
    container.appendChild(validationDiv);
    
    if (typeof htmx !== 'undefined') {
        htmx.process(newInputGroup);
    }
    window.expertiseCounter++;
}

function removeCustomExpertise(button) {
    const inputGroup = button.closest('.custom-expertise-input');
    const expertiseIndex = inputGroup.getAttribute('data-expertise-index');
    const validationDiv = document.getElementById(`customExpertiseValidation${expertiseIndex}`);
    
    inputGroup.remove();
    if (validationDiv) {
        validationDiv.remove();
    }
}

function addSocialMediaRow() {
    const container = document.getElementById('socialMediaContainer');
    const newRow = document.createElement('div');
    newRow.className = 'row mb-3 social-media-row';
    newRow.innerHTML = `
    <div class="col-md-4">
        <select name="Input.SocialMediaPlatforms" class="form-select">
            <option value="">Select Platform</option>
            <option value="LinkedIn">LinkedIn</option>
            <option value="Twitter">Twitter</option>
            <option value="GitHub">GitHub</option>
            <option value="YouTube">YouTube</option>
            <option value="Website">Personal Website</option>
            <option value="Blog">Blog</option>
            <option value="Other">Other</option>
        </select>
    </div>
    <div class="col-md-6">
        <input type="url" name="Input.SocialMediaUrls" class="form-control" placeholder="https://..." />
    </div>
    <div class="col-md-2">
        <button type="button" class="btn btn-outline-danger w-100" onclick="removeSocialMediaRow(this)">
            <i class="bi bi-trash"></i>
        </button>
    </div>
`;
    container.appendChild(newRow);
}

function removeSocialMediaRow(button) {
    if (document.querySelectorAll('.social-media-row').length > 1) {
        button.closest('.social-media-row').remove();
    }
}

function initializeRegistrationForm() {
    // Initialize custom expertise counter
    initializeCustomExpertise();
    
    // Handle email validation
    handleEmailValidation();
    
    // Enhanced HTMX handlers for registration
    document.addEventListener('htmx:beforeRequest', function (event) {
        const button = event.detail.elt;
        const originalContent = button.innerHTML;
        button.setAttribute('data-original-content', originalContent);
        
        if (button.id === 'nextBtn') {
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Validating...';
        } else if (button.id === 'submitBtn') {
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Creating Account...';
        } else if (button.id === 'prevBtn') {
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Loading...';
        }
    });

    document.addEventListener('htmx:afterRequest', function (event) {
        const button = event.detail.elt;
        const originalContent = button.getAttribute('data-original-content');
        
        if (originalContent) {
            button.disabled = false;
            button.innerHTML = originalContent;
            button.removeAttribute('data-original-content');
        }
    });
}

function updatePageHeader(step) {
    const pageHeader = document.getElementById('pageHeader');
    if (pageHeader) {
        if (step === 5) {
            pageHeader.innerHTML = '<h1 class="h3 fw-bold text-success">Welcome to MoreSpeakers.com!</h1><p class="text-muted">Your registration has been completed successfully</p>';
        } else {
            pageHeader.innerHTML = '<h1 class="h3 fw-bold text-primary">Create Your Speaker Profile</h1><p class="text-muted">Tell us about yourself and join the community</p>';
        }
    }
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

// Initialize account page functionality
function initializeAccountPage() {
    // HTMX event listeners for account page edit buttons
    document.addEventListener('htmx:beforeRequest', function (event) {
        const button = event.detail.elt;
        if (button.classList.contains('edit-btn')) {
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Loading...';
            button.disabled = true;
        }
    });

    document.addEventListener('htmx:afterRequest', function (event) {
        // Re-enable buttons after request
        const buttons = document.querySelectorAll('.edit-btn[disabled]');
        buttons.forEach(btn => {
            btn.disabled = false;
            if (btn.textContent.includes('Loading')) {
                btn.innerHTML = '<i class="bi bi-pencil me-1"></i>Edit';
            }
        });
    });

    // Auto-focus on edit fields
    document.addEventListener('htmx:afterSwap', function (event) {
        const input = event.target.querySelector('input, textarea');
        if (input) {
            input.focus();
            if (input.type === 'text' || input.type === 'email') {
                input.select();
            }
        }
    });
}


// Cookie utility functions
function setCookie(name, value, days) {
    const expires = new Date();
    expires.setTime(expires.getTime() + (days * 24 * 60 * 60 * 1000));
    document.cookie = `${name}=${value};expires=${expires.toUTCString()};path=/;SameSite=Lax`;
}

function getCookie(name) {
    const nameEQ = name + "=";
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function deleteCookie(name) {
    document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;`;
}


// Export functions for global use
window.MoreSpeakers = {
    showAlert,
    copyToClipboard,
    formatDate,
    scrollToElement,
    debounce,
    clearFileUpload,
    addCustomExpertise,
    removeCustomExpertise,
    addSocialMediaRow,
    removeSocialMediaRow,
    updatePageHeader,
    setCookie,
    getCookie,
    deleteCookie
};