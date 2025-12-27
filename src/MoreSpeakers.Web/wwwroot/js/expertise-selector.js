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

function filter(container, term, sectorId){
    const t = (term || '').trim().toLowerCase();
    const groups = container.querySelectorAll('.expertise-group');
    groups.forEach(group => {
        const groupSectorId = group.getAttribute('data-sector-id');
        const sectorMatch = !sectorId || sectorId === 'all' || groupSectorId === sectorId;
        
        let anyVisible = false;
        group.querySelectorAll('.expertise-item').forEach(li => {
            const name = (li.getAttribute('data-expertise-name') || '').toLowerCase();
            const termMatch = !t || name.indexOf(t) !== -1;
            const match = sectorMatch && termMatch;
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
        const sectorButtons = container.querySelectorAll('.sector-filter-btn');
        let currentSectorId = 'all';

        const updateFilter = () => filter(container, search?.value, currentSectorId);

        if (search) {
            search.addEventListener('input', updateFilter);
        }

        sectorButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                sectorButtons.forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                currentSectorId = btn.getAttribute('data-sector-id');
                updateFilter();
            });
        });

        container.addEventListener('click', (e) => {
            const btn = e.target.closest('button[data-select-all], button[data-clear-all]');
            if (!btn) return;
            const group = closest(btn, '.expertise-group');
            if (!group) return;
            setAllInGroup(group, btn.hasAttribute('data-select-all'));
        });
    });
}