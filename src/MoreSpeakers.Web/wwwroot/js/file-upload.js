// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeFileUploads();
});

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
            container.textContent = '';
            const preview = document.createElement('div');
            preview.className = 'file-preview';

            const img = document.createElement('img');
            img.src = e.target.result; // Safe: base64 data URL from FileReader, not server data
            img.alt = 'Preview';
            img.className = 'preview-image mb-2';

            const nameP = document.createElement('p');
            nameP.className = 'mb-2';
            const strong = document.createElement('strong');
            strong.textContent = file.name;
            nameP.appendChild(strong);

            const sizeP = document.createElement('p');
            sizeP.className = 'text-muted small';
            sizeP.textContent = formatFileSize(file.size);

            const removeBtn = document.createElement('button');
            removeBtn.type = 'button';
            removeBtn.className = 'btn btn-sm btn-outline-secondary';
            removeBtn.setAttribute('onclick', 'clearFileUpload(this)');
            const trashIcon = document.createElement('i');
            trashIcon.className = 'bi bi-trash';
            removeBtn.appendChild(trashIcon);
            removeBtn.appendChild(document.createTextNode(' Remove'));

            preview.appendChild(img);
            preview.appendChild(nameP);
            preview.appendChild(sizeP);
            preview.appendChild(removeBtn);
            container.appendChild(preview);
        };
        reader.readAsDataURL(file);
    }
}

// Clear file upload
function clearFileUpload(button) {
    const container = button.closest('.file-upload-area');
    const input = container.querySelector('input[type="file"]');

    input.value = '';
    container.textContent = ''; // Safe: static string, no user data
    const uploadIcon = document.createElement('i');
    uploadIcon.className = 'bi bi-cloud-upload display-4 text-muted mb-2';
    const clickP = document.createElement('p');
    clickP.className = 'mb-2';
    clickP.textContent = 'Click to upload or drag and drop';
    const hintP = document.createElement('p');
    hintP.className = 'small text-muted';
    hintP.textContent = 'PNG, JPG, GIF up to 5MB';
    container.appendChild(uploadIcon);
    container.appendChild(clickP);
    container.appendChild(hintP);

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