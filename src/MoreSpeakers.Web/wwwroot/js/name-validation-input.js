document.addEventListener('DOMContentLoaded', function() {
    handleNameValidation();
});

function handleNameValidation() {
    document.body.addEventListener('htmx:afterRequest', function(event) {

        if (event.detail.elt.className.indexOf('new-validation-input') === -1 || event.detail.successful !== true) {
            return;
        }

        try {
            const jsonData = JSON.parse(event.detail.xhr.responseText);
            const messageDiv = event.detail.target;
            const nameValidationInput = event.detail.elt;
            
            if (jsonData.$values.length === 0) { // TODO: Need to add an or if the id is the same, in case of an edit
                messageDiv.innerHTML = '';
                messageDiv.classList.remove('text-danger');
                messageDiv.classList.add('text-success');
                nameValidationInput.classList.remove('is-invalid');
                nameValidationInput.classList.add('is-valid');
            } else {
                const names = formatSectorNames(jsonData.$values);
                messageDiv.innerHTML = '<i class="bi bi-exclamation-triangle me-1"></i>There are some names that match this... ' + names + '.';
                messageDiv.classList.add('text-danger');
                messageDiv.classList.remove('text-success');
                nameValidationInput.classList.remove('is-valid');
                nameValidationInput.classList.add('is-invalid');
            }
        }
        catch {
            console.log("Failed to process JSON response from server.");
        }
    });
}


/**
 * Formats an array of Sector objects into a comma-separated string of their names.
 * @param {Array} values - The array of Sector objects (typically from $values property in JSON).
 * @returns {string} - A comma-separated string of sector names.
 */
function formatSectorNames(values) {
    if (!values || !Array.isArray(values)) {
        return "";
    }
    return values.map(sector => sector.name || sector.Name).filter(name => name).join(", ");
}