// Manual Stats Entry Functions

function showManualEntry() {
    const form = document.getElementById('manualStatsForm');
    if (form) {
        form.style.display = form.style.display === 'none' ? 'block' : 'none';
    }
}

function processManualStats() {
    const gameMode = document.getElementById('gameMode').value;
    const wins = parseInt(document.getElementById('wins').value) || 0;
    const kills = parseInt(document.getElementById('kills').value) || 0;
    const matches = parseInt(document.getElementById('matches').value) || 1;
    
    // Calculate derived stats
    const winRate = matches > 0 ? ((wins / matches) * 100).toFixed(1) : 0;
    const kd = matches > 0 ? (kills / Math.max(matches - wins, 1)).toFixed(2) : 0;
    const killsPerMatch = matches > 0 ? (kills / matches).toFixed(1) : 0;
    
    // Create stats display
    const statsHtml = `
        <div class="card mt-3">
            <div class="card-header bg-success text-white">
                <h5><i class="fas fa-chart-bar"></i> Manual Stats Analysis - ${gameMode.replace('_', ' ').toUpperCase()}</h5>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-2">
                        <div class="stat-box">
                            <div class="stat-value">${wins}</div>
                            <div class="stat-label">Wins</div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="stat-box">
                            <div class="stat-value">${kills}</div>
                            <div class="stat-label">Total Kills</div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="stat-box">
                            <div class="stat-value">${matches}</div>
                            <div class="stat-label">Matches</div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="stat-box">
                            <div class="stat-value">${kd}</div>
                            <div class="stat-label">K/D Ratio</div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="stat-box">
                            <div class="stat-value">${winRate}%</div>
                            <div class="stat-label">Win Rate</div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="stat-box">
                            <div class="stat-value">${killsPerMatch}</div>
                            <div class="stat-label">Kills/Match</div>
                        </div>
                    </div>
                </div>
                
                <div class="mt-3">
                    <button class="btn btn-primary" onclick="getAIAnalysis('${gameMode}', ${wins}, ${kills}, ${matches}, ${kd}, ${winRate})">
                        <i class="fas fa-robot"></i> Get AI Analysis
                    </button>
                </div>
                
                <div id="aiAnalysisResult" class="mt-3"></div>
            </div>
        </div>
    `;
    
    // Insert the stats display after the manual form
    const form = document.getElementById('manualStatsForm');
    const existingResult = document.getElementById('manualStatsResult');
    if (existingResult) {
        existingResult.remove();
    }
    
    const resultDiv = document.createElement('div');
    resultDiv.id = 'manualStatsResult';
    resultDiv.innerHTML = statsHtml;
    form.parentNode.insertBefore(resultDiv, form.nextSibling);
}

function getAIAnalysis(gameMode, wins, kills, matches, kd, winRate) {
    const analysisDiv = document.getElementById('aiAnalysisResult');
    analysisDiv.innerHTML = '<div class="text-center"><i class="fas fa-spinner fa-spin"></i> Generating AI analysis...</div>';
    
    // Create a summary for AI analysis
    const statsText = `Player Stats for ${gameMode.replace('_', ' ')}: ${wins} wins out of ${matches} matches (${winRate}% win rate), ${kills} total kills (${kd} K/D ratio)`;
    
    // Call the existing AI analysis endpoint
    fetch('/Home/GetAIFeedback', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            playerName: 'Manual Entry',
            statsType: 'Manual ' + gameMode,
            statsText: statsText
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            analysisDiv.innerHTML = `
                <div class="alert alert-success">
                    <h6><i class="fas fa-robot"></i> AI Analysis:</h6>
                    <p>${data.feedback}</p>
                </div>
            `;
        } else {
            analysisDiv.innerHTML = `
                <div class="alert alert-warning">
                    <h6>Analysis Unavailable</h6>
                    <p>AI analysis is currently unavailable, but your stats look good!</p>
                </div>
            `;
        }
    })
    .catch(error => {
        analysisDiv.innerHTML = `
            <div class="alert alert-info">
                <h6>Quick Analysis</h6>
                <p>Based on your ${gameMode.replace('_', ' ')} stats:</p>
                <ul>
                    <li><strong>Win Rate:</strong> ${winRate}% ${winRate > 10 ? '(Excellent!)' : winRate > 5 ? '(Good!)' : '(Keep practicing!)'}</li>
                    <li><strong>K/D Ratio:</strong> ${kd} ${kd > 2 ? '(Outstanding!)' : kd > 1 ? '(Above average!)' : '(Room for improvement!)'}</li>
                    <li><strong>Total Matches:</strong> ${matches} games played</li>
                </ul>
            </div>
        `;
    });
}

