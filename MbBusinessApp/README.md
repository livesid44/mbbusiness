# MB Business Web Application

A simple ASP.NET Core web application demonstrating API integration with query string parameters and button-triggered API calls.

## Features

- **First API**: Automatically called on page load, accepts a `message` parameter from the query string
- **Second API**: Triggered by button click, returns dynamic data including a random number
- Clean, responsive UI with real-time API response display

## Prerequisites

- .NET SDK 10.0 or later

## How to Run

1. Navigate to the MbBusinessApp directory:
   ```bash
   cd MbBusinessApp
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser and navigate to:
   - Default: `http://localhost:5000`
   - With custom message: `http://localhost:5000/?message=Your%20Custom%20Message`

## API Endpoints

### GET /api/first
Returns data based on the query string parameter.

**Query Parameters:**
- `message` (optional): Custom message to display in response

**Example:**
```
http://localhost:5000/api/first?message=Hello%20World
```

**Response:**
```json
{
  "success": true,
  "timestamp": "2026-02-17T14:37:44.177Z",
  "message": "Hello World",
  "source": "First API",
  "description": "This response was generated from the query string parameter"
}
```

### GET /api/second
Returns dynamic data when called from the button click.

**Example:**
```
http://localhost:5000/api/second
```

**Response:**
```json
{
  "success": true,
  "timestamp": "2026-02-17T14:37:57.282Z",
  "message": "This is the second API response",
  "source": "Second API",
  "description": "This response was generated from a button click",
  "randomNumber": 567
}
```

## Project Structure

- `Program.cs`: Main application entry point with API endpoint definitions
- `wwwroot/index.html`: Frontend HTML page with JavaScript for API calls
- `appsettings.json`: Application configuration

## Technology Stack

- ASP.NET Core 10.0 (Minimal API)
- HTML5
- JavaScript (ES6+)
- CSS3
