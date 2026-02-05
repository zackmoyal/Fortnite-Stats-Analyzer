# Fortnite Stats Analyzer

**Fortnite Stats Analyzer** is a full-stack web application that provides detailed real-time statistics and AI-powered insights for Fortnite players. By integrating with the [FortniteAPI.io](https://fortniteapi.io) and OpenAI, users can enter any Fortnite username to instantly view performance data and receive personalized coaching feedback.

## 🚀 Features

- 🎯 **Player Stats Lookup**: View key performance metrics like K/D ratio, win rate, and total kills across Solo, Duo, and Squad modes
- 🤖 **AI-Powered Feedback**: Get personalized coaching insights using OpenAI's GPT model with structured analysis:
  - 🎯 **Performance Analysis**: Skill level assessment and current performance evaluation
  - 💡 **Key Improvements**: Specific areas to focus on with actionable bullet points
  - 🚀 **Action Plan**: Concrete next steps for improvement
- 📱 **Responsive UI**: Clean Fortnite-inspired design with blue-to-yellow gradient theme and glowing border animations
- 🌎 **Global Stats Comparison**: See how individual players stack up against global Fortnite trends
- ⚡ **Instant Search**: Real-time data retrieval with minimal delay
- 🔐 **Secure API Management**: Environment-based API key security for both Fortnite and OpenAI APIs
- 🎨 **Enhanced Visual Design**: Transparent table backgrounds and improved feedback formatting

## 🛠 Technologies Used

- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Backend**: ASP.NET Core 8 MVC
- **APIs**: 
  - [FortniteAPI.io](https://fortniteapi.io) - Player statistics
  - [OpenAI API](https://openai.com/api/) - AI-powered feedback generation
- **Hosting**: Render (Dockerized Deployment)
- **Development Tools**: Visual Studio 2022, Git, GitHub

## 🧩 Getting Started (Local Setup)

### ✅ Prerequisites

- [.NET Core SDK 8.0+](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (or any compatible IDE)
- FortniteAPI.io API key
- OpenAI API key

### 📥 Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/zackmoyal/FortniteStatsAnalyzer.git
   ```

2. Navigate into the project directory:

   ```bash
   cd FortniteStatsAnalyzer
   ```

3. Set up your API keys (see below)

4. Run the app:

   - Using Visual Studio: Press `F5`
   - Or from the terminal:
     ```bash
     dotnet run
     ```

### 🔑 API Key Setup

To run this project locally, you'll need API keys from both FortniteAPI.io and OpenAI.

#### FortniteAPI.io Setup
- Sign up at [FortniteAPI.io](https://fortniteapi.io) and get your API key from the dashboard

#### OpenAI Setup
- Get your API key from [OpenAI Platform](https://platform.openai.com/api-keys)

#### Configuration
Create a file named `appsettings.Development.json` in the root directory (this file is ignored by Git):

```json
{
  "FortniteAPI": {
    "Key": "your-fortnite-api-key-here"
  },
  "OpenAISettings": {
    "ApiKey": "your-openai-api-key-here"
  }
}
```

## 🌐 Deployment

This app is deployed using Docker on [Render](https://render.com).

### Environment Variables for Production
Set the following environment variables in your hosting platform:
- `FortniteAPI__Key`: Your FortniteAPI.io key
- `OpenAISettings__ApiKey`: Your OpenAI API key

## 🎨 Recent Updates

### Version 2.0 Features
- **Enhanced AI Feedback**: Completely redesigned feedback system with structured, concise analysis
- **Improved Visual Design**: Removed white boxes from stats display for cleaner appearance
- **Better UX**: Left-aligned feedback text with proper bullet point formatting
- **Optimized Performance**: Reduced AI response length for faster loading

## 💡 Future Improvements

- Add search history tracking and charts for long-term player progress
- Integrate OAuth login to allow players to save favorite usernames
- Global leaderboard by game mode
- Dark mode toggle
- API usage dashboard
- Advanced AI coaching with match-specific recommendations
- Player comparison tools

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

## 🧑‍💻 Author

Built by [Zack Moyal](https://github.com/zackmoyal)

