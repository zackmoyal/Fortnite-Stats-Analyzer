// Loading overlay functionality
const LoadingOverlay = {
    overlay: null,
    messageElement: null,
    tipElement: null,
    progressBar: null,
    messageInterval: null,
    progressInterval: null,
    currentMessageIndex: 0,
    currentProgress: 0,

    // Loading messages that cycle through
    messages: [
        "Fetching player stats...",
        "Analyzing performance data...",
        "Calculating statistics...",
        "Generating AI feedback...",
        "Almost ready..."
    ],

    // Fun Fortnite tips to display
    tips: [
        "Did you know? The average Fortnite match lasts 20 minutes!",
        "Pro tip: Always carry shields for better survival!",
        "The storm moves faster in later circles - plan ahead!",
        "Building is just as important as aiming in Fortnite!",
        "Landing in hot zones improves your combat skills!",
        "Practice building in Creative mode to improve!",
        "High ground advantage is key in Fortnite battles!",
        "Keep moving to avoid becoming an easy target!",
        "Listen for footsteps to detect nearby enemies!",
        "Harvesting materials early game is crucial!"
    ],

    // Initialize the overlay
    init: function () {
        this.overlay = document.getElementById('loadingOverlay');
        this.messageElement = document.getElementById('loadingMessage');
        this.tipElement = document.getElementById('loadingTip');
        this.progressBar = document.getElementById('progressBar');
    },

    // Show the loading overlay
    show: function () {
        if (!this.overlay) this.init();

        this.overlay.classList.add('active');
        this.currentMessageIndex = 0;
        this.currentProgress = 0;

        // Set initial message and tip
        this.messageElement.textContent = this.messages[0];
        this.tipElement.textContent = this.getRandomTip();
        this.progressBar.style.width = '0%';

        // Start cycling messages every 5 seconds
        this.messageInterval = setInterval(() => {
            this.updateMessage();
        }, 5000);

        // Start progress bar animation
        this.progressInterval = setInterval(() => {
            this.updateProgress();
        }, 100);
    },

    // Hide the loading overlay
    hide: function () {
        if (this.overlay) {
            this.overlay.classList.remove('active');
        }

        // Clear intervals
        if (this.messageInterval) {
            clearInterval(this.messageInterval);
            this.messageInterval = null;
        }
        if (this.progressInterval) {
            clearInterval(this.progressInterval);
            this.progressInterval = null;
        }
    },

    // Update the loading message
    updateMessage: function () {
        this.currentMessageIndex++;
        if (this.currentMessageIndex < this.messages.length) {
            this.messageElement.textContent = this.messages[this.currentMessageIndex];
            // Change tip when message changes
            this.tipElement.textContent = this.getRandomTip();
        }
    },

    // Update progress bar (simulated progress)
    updateProgress: function () {
        // Slow down as we get closer to 90% (we don't know actual progress)
        const increment = this.currentProgress < 30 ? 1 :
            this.currentProgress < 60 ? 0.5 :
                this.currentProgress < 80 ? 0.3 : 0.1;

        this.currentProgress += increment;

        // Cap at 90% (we'll jump to 100% when actually done)
        if (this.currentProgress > 90) {
            this.currentProgress = 90;
        }

        this.progressBar.style.width = this.currentProgress + '%';
    },

    // Complete the progress bar
    complete: function () {
        this.currentProgress = 100;
        this.progressBar.style.width = '100%';

        // Hide after a brief moment
        setTimeout(() => {
            this.hide();
        }, 500);
    },

    // Get a random tip
    getRandomTip: function () {
        return this.tips[Math.floor(Math.random() * this.tips.length)];
    }
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    LoadingOverlay.init();

    // Attach to form submission
    const statsForm = document.getElementById('statsForm');
    if (statsForm) {
        statsForm.addEventListener('submit', function (e) {
            // Show loading overlay
            LoadingOverlay.show();

            // Also disable the submit button
            const submitButton = document.getElementById('submitButton');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Loading...';
            }
        });
    }
});