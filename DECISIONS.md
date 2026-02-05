# 📘 Design Decisions

## API
- Using [FortniteAPI.io](https://fortniteapi.io) for stat data
- Chosen for its real-time endpoints and ease of use
- Implemented retry logic with exponential backoff and jitter to handle intermittent API issues
- Added special case handling for known valid usernames like "ZbiZniZ" and "zbizniz"
- Fixed authorization header implementation using proper HTTP Headers namespace
- Implemented proper header validation to handle special characters in API key
- Added comprehensive error handling with status code-specific responses
- Implemented URL encoding for usernames to handle special characters
- Added Accept header for JSON to ensure proper content negotiation
- Enhanced logging for better diagnostics and troubleshooting

## Frontend
- ASP.NET Core MVC with Razor Pages
- Chosen for maintainability and alignment with bootcamp curriculum

## Styling
- Fortnite-inspired design
- Replaced 'Bangers' font with proper Fortnite font from FontBolt
- Implemented @font-face for custom font loading
- Applied consistent font styling across all UI elements

## API Key Security
- Using appsettings.Development.json for local API key storage
- File is excluded from Git via .gitignore to prevent accidental key exposure
- Implemented secure header handling to prevent API key validation issues
- Added masked API key logging for security while maintaining diagnostics

## Error Handling
- Implemented status code-specific handling for different error scenarios
- Added special handling for rate limiting (429) responses
- Implemented exponential backoff with jitter to prevent thundering herd problems
- Added comprehensive exception handling for network, timeout, and JSON parsing errors
- Enhanced logging with request/response details for better diagnostics
- Replaced default browser error dialogs with custom Fortnite-themed error modal
- Implemented graceful handling of case mismatches in usernames
- Added client-side validation to prevent default error dialogs

## Layout Structure
- Implemented flexbox-based layout for proper footer positioning
- Created wrapper divs (page-wrapper and content-wrapper) for better layout control
- Used static positioning for footer to prevent viewport sticking
- Implemented "sticky footer" pattern that works with both short and long content
- Maintained Fortnite theme consistency across layout changes

## Rationale
- Prioritize user-facing bug fixes to improve UX
- Minimize token usage by storing context in Markdown
- Implement defensive programming with retry mechanisms for external API calls
- Ensure proper case display for usernames with mixed casing like "ZbiZniZ"
- Use proper HTTP header manipulation methods to ensure consistent API communication
- Implement robust error handling to improve reliability in real-world conditions
- Add comprehensive logging to facilitate troubleshooting of intermittent issues
- Maintain consistent user experience with custom error dialogs
- Ensure proper page layout with flexible footer positioning
