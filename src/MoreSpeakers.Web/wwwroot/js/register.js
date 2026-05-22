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
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Validating...'; // Safe: static string, no user data
        } else if (button.id === 'submitBtn') {
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Creating Account...'; // Safe: static string, no user data
        } else if (button.id === 'prevBtn') {
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Loading...'; // Safe: static string, no user data
        } else if (button.id === 'submitExpertise') {
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Creating Expertise Area...'; // Safe: static string, no user data
        }
    });

    document.addEventListener('htmx:afterRequest', function (event) {
        const button = event.detail.elt;
        const originalContent = button.getAttribute('data-original-content');

        if (originalContent) {
            if (button.id !== 'submitExpertise') {
                button.disabled = false;
            }
            button.innerHTML = originalContent; // Safe: restores original server-rendered button markup
            button.removeAttribute('data-original-content');
        }
        initializeHeadshotProcessing();
        updatePageHeader();
    });

    document.addEventListener('htmx:configRequest', function (event) {
        const element = event.detail.elt;
        const stepNumber = Number(document.getElementById('CurrentStep').value);
        if (element && element.id === 'nextBtn' && stepNumber === 1)
        {
            const phoneInput = document.getElementById('Input_PhoneNumber');
            const phoneParameter = event.detail.parameters['Input.PhoneNumber']
            if (phoneInput && !(phoneInput.type.toLowerCase() === 'hidden') && phoneParameter) {
                event.detail.parameters['Input.PhoneNumber'] = phoneInput.iti.getNumber(intlTelInput.utils.numberFormat.E164);
            }
        }
    })

    document.addEventListener('htmx:afterSettle', function(event) {
        initializeTelephoneInput("#nextBtn");
    });
}

function handleEmailValidation() {
    document.body.addEventListener('htmx:afterRequest', function(event) {
        if (event.detail.target.id === 'email-validation-message') {
            const response = JSON.parse(event.detail.xhr.responseText);
            const messageDiv = event.detail.target;
            const emailInput = event.detail.elt;

            if (response.isValid) {
                messageDiv.textContent = '';
                if (response.message) {
                    const icon = document.createElement('i');
                    icon.className = 'bi bi-check-circle me-1';
                    messageDiv.appendChild(icon);
                    messageDiv.appendChild(document.createTextNode(response.message));
                }
                messageDiv.classList.remove('text-danger');
                messageDiv.classList.add('text-success');
                emailInput.classList.remove('is-invalid');
                emailInput.classList.add('is-valid');
            } else {
                const icon = document.createElement('i');
                icon.className = 'bi bi-exclamation-triangle me-1';
                messageDiv.textContent = '';
                messageDiv.appendChild(icon);
                messageDiv.appendChild(document.createTextNode(response.message));
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
        pageHeader.textContent = '';
        const h1 = document.createElement('h1');
        const p = document.createElement('p');
        p.className = 'text-muted';
        if (stepNumber === 5) {
            h1.className = 'h3 fw-bold text-success';
            h1.textContent = 'Welcome to MoreSpeakers.com!';
            p.textContent = 'Your registration has been completed successfully';
        } else {
            h1.className = 'h3 fw-bold text-primary';
            h1.textContent = 'Create Your Speaker Profile';
            p.textContent = 'Tell us about yourself and join the community';
        }
        pageHeader.appendChild(h1);
        pageHeader.appendChild(p);
    }
}
