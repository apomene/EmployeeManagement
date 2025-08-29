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

    if (skillSelect && newSkillDiv) {
        skillSelect.addEventListener("change", () => {
            const selectedValues = Array.from(skillSelect.selectedOptions).map(opt => opt.value);
            newSkillDiv.style.display = selectedValues.includes("-1") ? "block" : "none";
        });
    }

    
});

