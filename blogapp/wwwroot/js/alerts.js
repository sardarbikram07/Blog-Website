// Enhanced Alert System
class AlertManager {
    constructor() {
        this.alertContainer = null;
        this.init();
    }

    init() {
        // Create alert container if it doesn't exist
        if (!document.getElementById('alert-container')) {
            this.alertContainer = document.createElement('div');
            this.alertContainer.id = 'alert-container';
            this.alertContainer.className = 'alert-group';
            document.body.appendChild(this.alertContainer);
        } else {
            this.alertContainer = document.getElementById('alert-container');
        }

        // Initialize existing alerts
        this.initializeExistingAlerts();
    }

    initializeExistingAlerts() {
        // Add animation classes to existing alerts
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
            this.addAlertClasses(alert);
            this.setupAutoDismiss(alert);
        });
    }

    addAlertClasses(alert) {
        // Add animation class
        alert.classList.add('alert-fade-in');
        
        // Add hover effect
        alert.addEventListener('mouseenter', () => {
            alert.style.transform = 'translateY(-2px)';
        });
        
        alert.addEventListener('mouseleave', () => {
            alert.style.transform = 'translateY(0)';
        });
    }

    setupAutoDismiss(alert) {
        // Auto-dismiss after 5 seconds for success/info alerts
        const alertType = this.getAlertType(alert);
        if (alertType === 'success' || alertType === 'info') {
            setTimeout(() => {
                this.dismissAlert(alert);
            }, 5000);
        }
    }

    getAlertType(alert) {
        if (alert.classList.contains('alert-success')) return 'success';
        if (alert.classList.contains('alert-danger')) return 'danger';
        if (alert.classList.contains('alert-warning')) return 'warning';
        if (alert.classList.contains('alert-info')) return 'info';
        if (alert.classList.contains('alert-primary')) return 'primary';
        if (alert.classList.contains('alert-secondary')) return 'secondary';
        return 'info';
    }

    dismissAlert(alert) {
        alert.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(() => {
            if (alert.parentNode) {
                alert.parentNode.removeChild(alert);
            }
        }, 300);
    }

    // Create and show a new alert
    showAlert(message, type = 'info', options = {}) {
        const alert = this.createAlertElement(message, type, options);
        
        // Add to container
        this.alertContainer.appendChild(alert);
        
        // Setup auto-dismiss
        this.setupAutoDismiss(alert);
        
        // Add click to dismiss
        alert.addEventListener('click', () => {
            this.dismissAlert(alert);
        });

        return alert;
    }

    createAlertElement(message, type, options) {
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible alert-fade-in`;
        
        // Add icon based on type
        const icon = this.getIconForType(type);
        
        alert.innerHTML = `
            <i class="${icon}"></i>
            ${message}
            <button type="button" class="btn-close" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        `;

        // Add custom classes
        if (options.classes) {
            options.classes.split(' ').forEach(cls => {
                alert.classList.add(cls);
            });
        }

        // Add progress bar if requested
        if (options.progress) {
            alert.classList.add('alert-progress');
        }

        // Add badge if provided
        if (options.badge) {
            alert.classList.add('alert-badge');
            alert.setAttribute('data-badge', options.badge);
        }

        return alert;
    }

    getIconForType(type) {
        const icons = {
            success: 'fas fa-check-circle',
            danger: 'fas fa-exclamation-circle',
            warning: 'fas fa-exclamation-triangle',
            info: 'fas fa-info-circle',
            primary: 'fas fa-info-circle',
            secondary: 'fas fa-info-circle'
        };
        return icons[type] || icons.info;
    }

    // Show success alert
    success(message, options = {}) {
        return this.showAlert(message, 'success', options);
    }

    // Show error alert
    error(message, options = {}) {
        return this.showAlert(message, 'danger', options);
    }

    // Show warning alert
    warning(message, options = {}) {
        return this.showAlert(message, 'warning', options);
    }

    // Show info alert
    info(message, options = {}) {
        return this.showAlert(message, 'info', options);
    }

    // Show primary alert
    primary(message, options = {}) {
        return this.showAlert(message, 'primary', options);
    }

    // Clear all alerts
    clearAll() {
        const alerts = this.alertContainer.querySelectorAll('.alert');
        alerts.forEach(alert => {
            this.dismissAlert(alert);
        });
    }

    // Show toast notification
    showToast(message, type = 'info', duration = 5000) {
        const toast = this.createAlertElement(message, type);
        toast.classList.add('alert-toast');
        
        document.body.appendChild(toast);
        
        setTimeout(() => {
            this.dismissAlert(toast);
        }, duration);
        
        return toast;
    }

    // Show floating alert
    showFloating(message, type = 'info', duration = 3000) {
        const floating = this.createAlertElement(message, type);
        floating.classList.add('alert-floating');
        
        document.body.appendChild(floating);
        
        setTimeout(() => {
            this.dismissAlert(floating);
        }, duration);
        
        return floating;
    }
}

// Initialize alert manager when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    window.alertManager = new AlertManager();
    
    // Add CSS for slideOutRight animation
    if (!document.getElementById('alert-animations')) {
        const style = document.createElement('style');
        style.id = 'alert-animations';
        style.textContent = `
            @keyframes slideOutRight {
                from {
                    transform: translateX(0);
                    opacity: 1;
                }
                to {
                    transform: translateX(100%);
                    opacity: 0;
                }
            }
        `;
        document.head.appendChild(style);
    }
});

// Global functions for easy access
window.showAlert = (message, type = 'info', options = {}) => {
    if (window.alertManager) {
        return window.alertManager.showAlert(message, type, options);
    }
};

window.showToast = (message, type = 'info', duration = 5000) => {
    if (window.alertManager) {
        return window.alertManager.showToast(message, type, duration);
    }
};

window.showFloating = (message, type = 'info', duration = 3000) => {
    if (window.alertManager) {
        return window.alertManager.showFloating(message, type, duration);
    }
};



