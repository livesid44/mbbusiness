# API Endpoints Configuration Guide

This document explains how to configure API endpoints in the MB Business application.

## Overview

All API endpoints are now configurable through `appsettings.json`, allowing you to:
- Change endpoint paths without modifying code
- Use different endpoints per environment
- Easily switch between different API configurations

## Configuration Structure

### appsettings.json

```json
{
  "ApiEndpoints": {
    "FirstApi": "/api/first",
    "SecondApi": "/api/second",
    "MbBlockApi": "/api/mbblock"
  }
}
```

### Configuration Keys

| Key | Default Value | Description |
|-----|---------------|-------------|
| `FirstApi` | `/api/first` | Endpoint for the first API (query parameter demo) |
| `SecondApi` | `/api/second` | Endpoint for the second API (button click demo) |
| `MbBlockApi` | `/api/mbblock` | Endpoint for MB Business Block Check API |

## How to Change Endpoints

### Option 1: Update appsettings.json

Edit the `appsettings.json` file:

```json
{
  "ApiEndpoints": {
    "FirstApi": "/api/v1/first",
    "SecondApi": "/api/v1/second",
    "MbBlockApi": "/api/v1/mbblock"
  }
}
```

### Option 2: Environment-Specific Configuration

Edit `appsettings.Development.json` for development environment:

```json
{
  "ApiEndpoints": {
    "FirstApi": "/dev/api/first",
    "SecondApi": "/dev/api/second",
    "MbBlockApi": "/dev/api/mbblock"
  }
}
```

### Option 3: Environment Variables

Set environment variables to override configuration:

**Windows:**
```cmd
set ApiEndpoints__FirstApi=/custom/first
set ApiEndpoints__SecondApi=/custom/second
set ApiEndpoints__MbBlockApi=/custom/mbblock
```

**Linux/Mac:**
```bash
export ApiEndpoints__FirstApi=/custom/first
export ApiEndpoints__SecondApi=/custom/second
export ApiEndpoints__MbBlockApi=/custom/mbblock
```

## Configuration Discovery Endpoint

The application exposes a `/api/config` endpoint that returns the current API endpoint configuration:

**Request:**
```
GET /api/config
```

**Response:**
```json
{
  "endpoints": {
    "firstApi": "/api/first",
    "secondApi": "/api/second",
    "mbBlockApi": "/api/mbblock"
  }
}
```

The frontend automatically loads this configuration on page load to use the correct endpoints.

## Frontend Integration

The JavaScript code automatically fetches the endpoint configuration:

```javascript
// API endpoints are loaded from server on page load
async function loadApiConfig() {
    const response = await fetch('/api/config');
    const config = await response.json();
    apiEndpoints = config.endpoints;
}

// Then used in API calls
fetch(`${apiEndpoints.firstApi}?message=test`)
```

## Testing with Different Endpoints

### Example: Testing with Versioned Endpoints

**appsettings.json:**
```json
{
  "ApiEndpoints": {
    "FirstApi": "/api/v2/first",
    "SecondApi": "/api/v2/second",
    "MbBlockApi": "/api/v2/mbblock"
  }
}
```

**Program.cs** automatically registers endpoints at the configured paths.

### Example: Using Different API Prefixes

For microservices or gateway scenarios:

```json
{
  "ApiEndpoints": {
    "FirstApi": "/demo-service/first",
    "SecondApi": "/demo-service/second",
    "MbBlockApi": "/block-service/check"
  }
}
```

## Benefits

### 1. Environment Flexibility
Different environments can use different endpoint paths:
- Development: `/dev/api/*`
- Staging: `/staging/api/*`
- Production: `/api/*`

### 2. API Versioning
Easy to switch between API versions:
- v1: `/api/v1/*`
- v2: `/api/v2/*`

### 3. Gateway Integration
Can prefix with service names for API gateways:
- `/demo-service/*`
- `/block-service/*`

### 4. Testing
Easily switch to test endpoints without code changes.

## Migration Notes

### Breaking Changes
None. If configuration is not provided, default values (`/api/first`, `/api/second`, `/api/mbblock`) are used.

### Backward Compatibility
The application maintains backward compatibility:
- Existing URLs continue to work
- Frontend automatically adapts to configured endpoints
- Default values match previous hardcoded paths

## Troubleshooting

### Endpoints Not Working After Configuration Change

1. **Verify Configuration:**
   ```bash
   curl http://localhost:5000/api/config
   ```

2. **Check Application Logs:**
   Look for any configuration loading errors.

3. **Clear Browser Cache:**
   Frontend might cache old configuration.

4. **Restart Application:**
   Changes to `appsettings.json` require application restart.

### Frontend Not Using New Endpoints

1. **Check Browser Console:**
   Look for "API endpoints loaded" message.

2. **Verify Config Endpoint:**
   Ensure `/api/config` returns correct values.

3. **Check Network Tab:**
   Verify frontend is calling the correct endpoints.

## Examples

### Scenario 1: API Gateway with Service Prefix

**Configuration:**
```json
{
  "ApiEndpoints": {
    "FirstApi": "/demo/api/first",
    "SecondApi": "/demo/api/second",
    "MbBlockApi": "/block/api/check"
  }
}
```

**Result:**
- First API available at: `http://localhost:5000/demo/api/first`
- Second API available at: `http://localhost:5000/demo/api/second`
- MB Block API available at: `http://localhost:5000/block/api/check`

### Scenario 2: Version-Based Routing

**Configuration:**
```json
{
  "ApiEndpoints": {
    "FirstApi": "/api/v2/first",
    "SecondApi": "/api/v2/second",
    "MbBlockApi": "/api/v2/mbblock"
  }
}
```

**Result:**
All APIs accessible under `/api/v2/` prefix.

### Scenario 3: Custom Paths

**Configuration:**
```json
{
  "ApiEndpoints": {
    "FirstApi": "/custom/demo",
    "SecondApi": "/custom/random",
    "MbBlockApi": "/custom/block-check"
  }
}
```

**Result:**
Completely custom endpoint paths.

## See Also

- [appsettings.json](appsettings.json) - Main configuration file
- [appsettings.Development.json](appsettings.Development.json) - Development overrides
- [Program.cs](Program.cs) - Endpoint registration implementation
- [TESTING.md](../../TESTING.md) - Testing guide with sample URLs
