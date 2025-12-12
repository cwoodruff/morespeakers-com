// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    registerExpertiseFunction();
    document.body.addEventListener('htmx:afterRequest', function(event) {

        if (event.detail.elt.id !== 'registrationContainer') {
            return;
        }
        let currentStepElement = document.getElementById('CurrentStep');
        if (!currentStepElement) {
            return;
        }
        if (currentStepElement.value !== "3") {
            return;
        }
        registerExpertiseFunction();
    });
});

// for the register page, we need to register the "registerExpertiseFunction" after the Step3 is loaded


function closest(el, selector){
while (el && el.nodeType === 1) {
  if (el.matches(selector)) return el;
  el = el.parentElement;
}
return null;
}

function filter(container, term){
const t = (term || '').trim().toLowerCase();
const groups = container.querySelectorAll('.expertise-group');
groups.forEach(group => {
  let anyVisible = false;
  group.querySelectorAll('.expertise-item').forEach(li => {
    const name = (li.getAttribute('data-expertise-name') || '').toLowerCase();
    const match = !t || name.indexOf(t) !== -1;
    li.style.display = match ? '' : 'none';
    if (match) anyVisible = true;
  });
  group.style.display = anyVisible ? '' : 'none';
});
}

function setAllInGroup(group, checked){
group.querySelectorAll('input[type="checkbox"]').forEach(cb => {
  cb.checked = checked;
});
}

function registerExpertiseFunction() {
    document.querySelectorAll('[data-expertise-selector]').forEach(container => {
        const search = container.querySelector('[data-expertise-search]');
        if (search) {
            search.addEventListener('input', () => filter(container, search.value));
        }

        container.addEventListener('click', (e) => {
            const btn = e.target.closest('button[data-select-all], button[data-clear-all]');
            if (!btn) return;
            const group = closest(btn, '.expertise-group');
            if (!group) return;
            setAllInGroup(group, btn.hasAttribute('data-select-all'));
        });
    });
}