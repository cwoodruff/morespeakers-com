// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeRegistrationForm();
});

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

function initializeRegistrationForm() {

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
        updatePageHeader();
    });
}

// Custom validation for the registration form
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

function updatePageHeader() {
    
    const currentStep = document.getElementById('CurrentStep');
    let stepNumber = 1;
    if (currentStep) {
        stepNumber = Number(currentStep.value);
    }
    
    const pageHeader = document.getElementById('pageHeader');
    if (pageHeader) {
        if (stepNumber === 5) {
            pageHeader.innerHTML = '<h1 class="h3 fw-bold text-success">Welcome to MoreSpeakers.com!</h1><p class="text-muted">Your registration has been completed successfully</p>';
        } else {
            pageHeader.innerHTML = '<h1 class="h3 fw-bold text-primary">Create Your Speaker Profile</h1><p class="text-muted">Tell us about yourself and join the community</p>';
        }
    }
}