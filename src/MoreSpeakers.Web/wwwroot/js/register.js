// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeRegistrationForm();
    initializeTelephoneInput();
    handleEmailValidation();
});

function initializeRegistrationForm() {

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
        } else if (button.id === 'submitExpertise') {
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Creating Expertise Area...';
        }

        if (button.id === 'nextBtn') {
            // update the phone number
            const phoneInput = document.querySelector("#Input_PhoneNumber");
            if (phoneInput && !phoneInput.hidden) {
                phoneInput.value = phoneInput.iti.getNumber(intlTelInput.utils.numberFormat.E164);
            }
        }
    });

    document.addEventListener('htmx:afterRequest', function (event) {
        const button = event.detail.elt;
        const originalContent = button.getAttribute('data-original-content');

        if (originalContent) {
            if (button.id !== 'submitExpertise') {
                button.disabled = false;
            }
            button.innerHTML = originalContent;
            button.removeAttribute('data-original-content');
        }
        initializeHeadshotProcessing();
        initializeTelephoneInput();
        updatePageHeader();
    });

    document.addEventListener('htmx:afterSettle', function(event) {
        initializeTelephoneInput("#submitButton");
    });
}

function handleEmailValidation() {
    document.body.addEventListener('htmx:afterRequest', function(event) {
        if (event.detail.target.id === 'email-validation-message') {
            const response = JSON.parse(event.detail.xhr.responseText);
            const messageDiv = event.detail.target;
            const emailInput = event.detail.elt;

            if (response.isValid) {
                messageDiv.innerHTML = response.message ? '<i class="bi bi-check-circle me-1"></i>' + response.message : '';
                messageDiv.classList.remove('text-danger');
                messageDiv.classList.add('text-success');
                emailInput.classList.remove('is-invalid');
                emailInput.classList.add('is-valid');
            } else {
                messageDiv.innerHTML = '<i class="bi bi-exclamation-triangle me-1"></i>' + response.message;
                messageDiv.classList.add('text-danger');
                messageDiv.classList.remove('text-success');
                emailInput.classList.remove('is-valid');
                emailInput.classList.add('is-invalid');
            }
        }
    });
}

// Custom validation for the registration form
function validateRegistrationForm(form) {
    let isValid = true;

    // Check if at least one expertise is selected
    const expertiseInputs = form.querySelectorAll('input[name="Input.SelectedExpertiseIds"]:checked');
    if (expertiseInputs.length === 0) {
        showAlert('Please select at least one area of expertise.', 'warning');
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
