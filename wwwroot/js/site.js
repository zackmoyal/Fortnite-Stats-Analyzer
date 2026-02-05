document.addEventListener('DOMContentLoaded', function () {
    const form = document.querySelector('#statsForm');
    const usernameInput = document.querySelector('#username');
    const submitButton = document.querySelector('#submitButton');
    const errorModalElement = document.getElementById('errorModal');
    const errorMessageElement = document.querySelector('#errorMessage');
    const allowedUsernames = ["ZbiZniZ", "Tfue", "Bugha", "Ninja"]; // Move to config if possible

    // Debounce function to limit API calls
    const debounce = (func, delay) => {
        let timeout;
        return (...args) => {
            clearTimeout(timeout);
            timeout = setTimeout(() => func(...args), delay);
        };
    };

    // Show error modal
    const showError = (message) => {
        if (errorMessageElement) {
            errorMessageElement.textContent = message;
        }

        const errorModal = new bootstrap.Modal(errorModalElement);
        errorModal.show();

        errorModalElement.addEventListener('hidden.bs.modal', function handler() {
            window.location.href = '/';
            errorModal.dispose();
            errorModalElement.removeEventListener('hidden.bs.modal', handler);
        }, { once: true });
    };

    // Set loading state
    const setLoadingState = (isLoading) => {
        submitButton.disabled = isLoading;
        if (isLoading) {
            submitButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...';
            submitButton.classList.add('loading');
        } else {
            submitButton.innerHTML = 'Get Stats';
            submitButton.classList.remove('loading');
        }
    };

    // Form submission logic
    if (form) {
        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            // Show loading state
            setLoadingState(true);

            try {
                const username = usernameInput.value.trim();

                if (!username) {
                    showError("Please enter a username.");
                    setLoadingState(false);
                    return;
                }

                // Validate username
                const validateResponse = await fetch(`/Home/ValidateUsername?username=${encodeURIComponent(username)}`);
                const validateData = await validateResponse.json();

                if (!validateData.success) {
                    showError(validateData.message);
                    setLoadingState(false);
                    return;
                }

                // Submit form if validation passes
                form.submit();
                // Keep the loading state active - it will be reset when the new page loads
            } catch (error) {
                showError('An error occurred. Please try again.');
                setLoadingState(false);
            }
        });
    }

    // Username suggestion logic
    if (usernameInput) {
        usernameInput.addEventListener('input', debounce(() => {
            const entered = usernameInput.value.trim();
            const closeMatch = allowedUsernames.find(name => name.toLowerCase() === entered.toLowerCase());

            if (closeMatch && closeMatch !== entered) {
                alert(`Did you mean '${closeMatch}'? Please enter it with exact casing.`);
            }
        }, 300));
    }
});