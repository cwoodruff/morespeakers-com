// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    handleNewExpertiseValidation();
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
                messageDiv.textContent = '';
                if (jsonData.message) {
                    const icon = document.createElement('i');
                    icon.className = 'bi bi-check-circle me-1';
                    messageDiv.appendChild(icon);
                    messageDiv.appendChild(document.createTextNode(jsonData.message));
                }
                messageDiv.classList.remove('text-danger');
                messageDiv.classList.add('text-success');
                newExpertiseInput.classList.remove('is-invalid');
                newExpertiseInput.classList.add('is-valid');
            } else {
                const icon = document.createElement('i');
                icon.className = 'bi bi-exclamation-triangle me-1';
                messageDiv.textContent = '';
                messageDiv.appendChild(icon);
                messageDiv.appendChild(document.createTextNode(jsonData.message));
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