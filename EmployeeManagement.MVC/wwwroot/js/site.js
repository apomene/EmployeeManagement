document.addEventListener("DOMContentLoaded", () => {

    // === Employee Index Page ===
    const employeeForm = document.getElementById("employeeIndexForm");
    const sortInput = document.getElementById("sortBy");
    const searchInput = document.getElementById("searchInput");
    const selectAllCheckbox = document.getElementById("select-all");
   


    if (employeeForm) {
        // Sorting links
        document.querySelectorAll(".sort-link").forEach(link => {
            link.addEventListener("click", e => {
                e.preventDefault();
                sortInput.value = link.dataset.sort;
                employeeForm.submit();
            });
        });

        // Debounce function for search
        function debounce(func, wait) {
            let timeout;
            return function (...args) {
                clearTimeout(timeout);
                timeout = setTimeout(() => func.apply(this, args), wait);
            };
        }

        // Submit form when search changes
        if (searchInput) {
            const submitSearch = debounce(() => {
                employeeForm.submit();
            }, 500);
            searchInput.addEventListener("input", submitSearch);
        }

        // Select/Deselect all checkboxes
        if (selectAllCheckbox) {
            selectAllCheckbox.addEventListener("change", function () {
                const checked = this.checked;
                document.querySelectorAll("input[name='selectedIds']").forEach(cb => cb.checked = checked);
            });
        }
       

    }

    // === Create/Edit Employee Page: Show/Hide New Skill Fields ===
    const skillSelect = document.getElementById("SelectedSkillIds");
    const newSkillDiv = document.getElementById("newSkillDiv");

    const employeeEmailForm = document.getElementById("employeeForm"); 
    if (employeeEmailForm) {
       setupEmailValidation("employeeForm", "employeeEmailInput", "employeeMailError"); // Create/Edit Employee view
    }
    if (skillSelect && newSkillDiv) {
        skillSelect.addEventListener("change", () => {
            const selectedValues = Array.from(skillSelect.selectedOptions).map(opt => opt.value);
            newSkillDiv.style.display = selectedValues.includes("-1") ? "block" : "none";
        });
    }
    
    // Email validation for  Audit Logs
    const emailForm = document.getElementById("emailForm"); 
    if (emailForm) {
        setupEmailValidation("emailForm", "logEmail", "emailError");  // Audit Logs view
    }

    // < --Script to clear filters-- >

    document.getElementById('clearFiltersBtn').addEventListener('click', function () {
        document.getElementById('searchInput').value = '';
        const skillSelect = document.getElementById('skillSelect');
        for (let i = 0; i < skillSelect.options.length; i++) {
            skillSelect.options[i].selected = false;
        }
        document.getElementById('employeeIndexForm').submit();
    });
    
});

// Email validation regex
const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/**
 * Adds email validation to a form.
 * @param {string} formId - ID of the form.
 * @param {string} inputId - ID of the email input.
 * @param {string} errorId - ID of the span to show error.
 */
function setupEmailValidation(formId, inputId, errorId) {
    const form = document.getElementById(formId);
    const input = document.getElementById(inputId);
    const errorSpan = document.getElementById(errorId);

    if (!form || !input || !errorSpan) return;

    form.addEventListener("submit", (event) => {
        const email = input.value.trim();
        if (email && !emailPattern.test(email)) {
            event.preventDefault();
            errorSpan.style.display = "inline";
        } else {
            errorSpan.style.display = "none";
        }
    });

    input.addEventListener("input", () => {
        errorSpan.style.display = "none";
    });   
}



   
