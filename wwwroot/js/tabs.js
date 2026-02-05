function showStats(type, event) {
    const globalStats = document.getElementById("global-stats");
    const inputStats = document.getElementById("input-stats");
    const feedbackGlobal = document.getElementById("feedback-global");
    const feedbackInput = document.getElementById("feedback-input");
    const globalFeedbackBtn = document.querySelector(".explore-feedback-btn-global");
    const inputFeedbackBtn = document.querySelector(".explore-feedback-btn-input");

    // Reset visibility
    globalStats.style.display = "none";
    inputStats.style.display = "none";
    feedbackGlobal.style.display = "none";
    feedbackInput.style.display = "none";
    globalFeedbackBtn.style.display = "none";
    inputFeedbackBtn.style.display = "none";

    // Show the correct section based on the tab
    if (type === 'global') {
        globalStats.style.display = "block";
        globalFeedbackBtn.style.display = "block";
    } else {
        inputStats.style.display = "block";
        inputFeedbackBtn.style.display = "block";
    }

    // Update active tab button
    const buttons = document.getElementsByClassName("tab-button");
    for (let button of buttons) {
        button.classList.remove("active");
    }
    event.currentTarget.classList.add("active");
}

function showFeedback(type) {
    let feedbackSection = type === 'global' ? document.getElementById("feedback-global") : document.getElementById("feedback-input");

    if (feedbackSection) {
        feedbackSection.style.display = "block";
        setTimeout(() => feedbackSection.classList.add('fade-in'), 10);
        feedbackSection.scrollIntoView({ behavior: 'smooth' });
    }
}

window.onload = function () {
    // Set initial state
    showStats('global', { currentTarget: document.querySelector(".tab-button.active") });
};
