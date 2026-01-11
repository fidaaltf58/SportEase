// ===== GLOBAL VARIABLES =====
const SportEase = {
    init: function () {
        this.initTooltips();
        this.initPopovers();
        this.initAlerts();
        this.initFormValidation();
        this.initImagePreview();
        this.initConfirmDialogs();
    },

    // Initialize Bootstrap tooltips
    initTooltips: function () {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    },

    // Initialize Bootstrap popovers
    initPopovers: function () {
        const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
        popoverTriggerList.map(function (popoverTriggerEl) {
            return new bootstrap.Popover(popoverTriggerEl);
        });
    },

    // Auto-hide alerts after 5 seconds
    initAlerts: function () {
        const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        alerts.forEach(alert => {
            setTimeout(() => {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }, 5000);
        });
    },

    // Form validation enhancement
    initFormValidation: function () {
        const forms = document.querySelectorAll('.needs-validation');
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                form.classList.add('was-validated');
            }, false);
        });
    },

    // Image preview before upload
    initImagePreview: function () {
        const imageInputs = document.querySelectorAll('input[type="file"][accept*="image"]');
        imageInputs.forEach(input => {
            input.addEventListener('change', function (e) {
                const file = e.target.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = function (event) {
                        let preview = document.getElementById('imagePreview');
                        if (!preview) {
                            preview = document.createElement('img');
                            preview.id = 'imagePreview';
                            preview.className = 'img-thumbnail mt-2';
                            preview.style.maxWidth = '300px';
                            input.parentNode.appendChild(preview);
                        }
                        preview.src = event.target.result;
                    };
                    reader.readAsDataURL(file);
                }
            });
        });
    },

    // Confirm dialogs for dangerous actions
    initConfirmDialogs: function () {
        const dangerousForms = document.querySelectorAll('form[data-confirm]');
        dangerousForms.forEach(form => {
            form.addEventListener('submit', function (e) {
                const message = this.getAttribute('data-confirm') || 'Êtes-vous sûr ?';
                if (!confirm(message)) {
                    e.preventDefault();
                }
            });
        });
    }
};

// ===== UTILITY FUNCTIONS =====

// Format currency
function formatCurrency(amount) {
    return parseFloat(amount).toFixed(2) + ' DT';
}

// Format date
function formatDate(dateString) {
    const date = new Date(dateString);
    const options = { year: 'numeric', month: 'long', day: 'numeric' };
    return date.toLocaleDateString('fr-FR', options);
}

// Format time
function formatTime(timeString) {
    return timeString.substring(0, 5);
}

// Show loading spinner
function showLoading(element) {
    const originalContent = element.innerHTML;
    element.setAttribute('data-original-content', originalContent);
    element.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Chargement...';
    element.disabled = true;
}

// Hide loading spinner
function hideLoading(element) {
    const originalContent = element.getAttribute('data-original-content');
    if (originalContent) {
        element.innerHTML = originalContent;
        element.disabled = false;
    }
}

// Show toast notification
function showToast(message, type = 'info') {
    const toastHtml = `
        <div class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    let toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toastContainer';
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    const toastElement = toastContainer.lastElementChild;
    const toast = new bootstrap.Toast(toastElement);
    toast.show();

    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
}

// Debounce function
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// AJAX helper
async function ajaxGet(url, params = {}) {
    const queryString = new URLSearchParams(params).toString();
    const fullUrl = queryString ? `${url}?${queryString}` : url;

    try {
        const response = await fetch(fullUrl);
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return await response.json();
    } catch (error) {
        console.error('AJAX Error:', error);
        showToast('Une erreur est survenue', 'danger');
        return null;
    }
}

async function ajaxPost(url, data) {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return await response.json();
    } catch (error) {
        console.error('AJAX Error:', error);
        showToast('Une erreur est survenue', 'danger');
        return null;
    }
}

// ===== TIME SLOT SELECTION =====
class TimeSlotSelector {
    constructor(containerSelector, options = {}) {
        this.container = document.querySelector(containerSelector);
        this.selectedSlots = [];
        this.options = Object.assign({
            onSelectionChange: null,
            maxConsecutive: 3,
            minConsecutive: 1
        }, options);

        this.init();
    }

    init() {
        if (!this.container) return;

        this.container.addEventListener('click', (e) => {
            const slotBtn = e.target.closest('.time-slot-btn');
            if (slotBtn && !slotBtn.disabled) {
                this.toggleSlot(slotBtn);
            }
        });
    }

    toggleSlot(button) {
        button.classList.toggle('active');
        this.updateSelection();
    }

    updateSelection() {
        const activeSlots = this.container.querySelectorAll('.time-slot-btn.active');
        this.selectedSlots = Array.from(activeSlots).map(btn => btn.dataset.time);

        if (this.options.onSelectionChange) {
            this.options.onSelectionChange(this.selectedSlots);
        }
    }

    getSelection() {
        return this.selectedSlots;
    }

    clearSelection() {
        const activeSlots = this.container.querySelectorAll('.time-slot-btn.active');
        activeSlots.forEach(btn => btn.classList.remove('active'));
        this.selectedSlots = [];
    }
}

// ===== SEARCH FILTERS =====
class SearchFilter {
    constructor(formSelector) {
        this.form = document.querySelector(formSelector);
        this.init();
    }

    init() {
        if (!this.form) return;

        const inputs = this.form.querySelectorAll('input, select');
        inputs.forEach(input => {
            input.addEventListener('change', debounce(() => {
                this.applyFilters();
            }, 300));
        });
    }

    applyFilters() {
        // Submit form via AJAX or regular submit
        if (this.form.dataset.ajax === 'true') {
            this.submitAjax();
        } else {
            this.form.submit();
        }
    }

    async submitAjax() {
        const formData = new FormData(this.form);
        const params = new URLSearchParams(formData);

        showLoading(document.querySelector('.search-results'));

        const results = await ajaxGet(this.form.action, Object.fromEntries(params));

        hideLoading(document.querySelector('.search-results'));

        if (results) {
            this.updateResults(results);
        }
    }

    updateResults(results) {
        const container = document.querySelector('.search-results');
        if (container) {
            container.innerHTML = results.html || '';
        }
    }
}

// ===== RESERVATION CALENDAR =====
class ReservationCalendar {
    constructor(containerId, terrainId) {
        this.container = document.getElementById(containerId);
        this.terrainId = terrainId;
        this.selectedDate = null;
        this.availableSlots = [];

        this.init();
    }

    init() {
        if (!this.container) return;
        this.renderCalendar();
    }

    renderCalendar() {
        // Implementation of calendar rendering
        // This would typically use a library like FullCalendar or custom implementation
    }

    async loadSlots(date) {
        const slots = await ajaxGet('/Reservation/GetAvailableSlots', {
            terrainId: this.terrainId,
            date: date
        });

        if (slots) {
            this.availableSlots = slots;
            this.renderSlots();
        }
    }

    renderSlots() {
        // Render available time slots
    }
}

// ===== STATISTICS CHARTS =====
function initChart(canvasId, data, options = {}) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return null;

    return new Chart(ctx, {
        type: options.type || 'bar',
        data: data,
        options: Object.assign({
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    display: options.showLegend !== false
                }
            }
        }, options)
    });
}

// ===== INITIALIZE ON DOM READY =====
document.addEventListener('DOMContentLoaded', function () {
    SportEase.init();
});

// ===== EXPORT FOR MODULE USAGE =====
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        SportEase,
        TimeSlotSelector,
        SearchFilter,
        ReservationCalendar,
        showToast,
        ajaxGet,
        ajaxPost
    };
}