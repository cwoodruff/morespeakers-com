// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    handleNewExpertiseValidation();
    handleNewExpertiseCreated();
    handlePageSettle();
    handleHtmlError();
});

function handleNewExpertiseValidation() {
    document.body.addEventListener('htmx:afterRequest', function(event) {

        if (event.detail.elt.name !== 'Input.NewExpertise' || event.detail.successful !== true) {
            return;
        }

        try {
            const jsonData = JSON.parse(event.detail.xhr.responseText);
            const messageDiv = event.detail.target;
            const newExpertiseInput = event.detail.elt;

            let submitButton = document.getElementById('submitExpertise');
            if (!submitButton) {
                console.log ("Could not find submit button!");
                return;
            }
            submitButton.disabled = !jsonData.isValid;
            if (jsonData.isValid) {
                messageDiv.innerHTML = jsonData.message ? '<i class="bi bi-check-circle me-1"></i>' + jsonData.message : '';
                messageDiv.classList.remove('text-danger');
                messageDiv.classList.add('text-success');
                newExpertiseInput.classList.remove('is-invalid');
                newExpertiseInput.classList.add('is-valid');
            } else {
                messageDiv.innerHTML = '<i class="bi bi-exclamation-triangle me-1"></i>' + jsonData.message;
                messageDiv.classList.add('text-danger');
                messageDiv.classList.remove('text-success');
                newExpertiseInput.classList.remove('is-valid');
                newExpertiseInput.classList.add('is-invalid');
            }
        }
        catch {
            console.log("Failed to process JSON response from server.");
        }
    });
}

function handleNewExpertiseCreated() {
    document.body.addEventListener('htmx:afterRequest', function(event) {

        if (event.detail.elt.id !== 'submitExpertise' || event.detail.successful !== true) {
            return;
        }

        try {
            const jsonData = JSON.parse(event.detail.xhr.responseText);
            const messageDiv = event.detail.target;
            const newExpertiseInput = document.getElementById('Input.NewExpertise');

            let submitButton = document.getElementById('submitExpertise');
            if (!submitButton) {
                console.log ("Could not find submit button!");
                return;
            }
            // if the save was successful, disable the submit button
            submitButton.disabled = jsonData.isValid;
            if (jsonData.isValid) {
                messageDiv.innerHTML = 'Created!';
                newExpertiseInput.classList.remove('is-valid');
                newExpertiseInput.placeholder="Enter a new expertise area"
                newExpertiseInput.value = '';
            } else {
                messageDiv.innerHTML = '<i class="bi bi-exclamation-triangle me-1"></i>' + jsonData.message;
                messageDiv.classList.add('text-danger');
                messageDiv.classList.remove('text-success');
                newExpertiseInput.classList.remove('is-valid');
                newExpertiseInput.classList.add('is-invalid');
            }
        }
        catch {
            console.log("Failed to process JSON response from server.");
        }
    });
}

function handlePageSettle() {
    document.body.addEventListener('htmx:afterSettle', function (event) {

        const step = document.getElementById('CurrentStep');
        if (!step) return;
        
        if (event.detail.elt.id !== 'registrationContainer' || step.value !== "3") {
            return;
        }

        const expertiseLists = document.getElementById('expertise-list-display');
        expertiseLists.setAttribute('hx-swap-oob', 'true');
        expertiseLists.setAttribute('hx-swap', 'outerHTML');
        
    });
}

function handleHtmlError() {
    document.body.addEventListener('htmx:onLoadError', function(event) {
        console.log("Failed to process HTML response from server.");
    });
    document.body.addEventListener('htmx:oobErrorNoTarget', function(event) {
        console.log("Failed to process HTML response from server.");
    });
}