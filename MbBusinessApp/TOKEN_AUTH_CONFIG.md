# Token Authentication Configuration Guide

This document explains how token-based authentication works for the MB Business Block Check API.

## Overview

The MB Business Block Check API now supports token-based authentication. Before calling the status API, the application automatically:
1. Calls a token generation API
2. Receives an authentication token
3. Adds the token as a Bearer token in the Authorization header
4. Makes the status API call with the token

## Configuration

### appsettings.json

Add the following configuration to enable token authentication:

```json
{
  "MBBlock": {
    "TokenUrl": "http://10.255.233.28:2000/token",
    "Username": "test1",
    "Password": "test1@123",
    "MiddlewareUrl": "http://10.255.234.21:2000/mb/mbBlockChk",
    "EncryptionBaseKey": "MBBOB12#",
    "ChannelCode": "21",
    "Channel": "CISCO",
    "ClientId": "900001"
  }
}
```

### Configuration Keys

| Key | Required | Default | Description |
|-----|----------|---------|-------------|
| `TokenUrl` | Yes | `http://10.255.233.28:2000/token` | URL for token generation API |
| `Username` | Yes | `test1` | Username for authentication |
| `Password` | Yes | `test1@123` | Password for authentication |
| `MiddlewareUrl` | Yes | `http://10.255.234.21:2000/mb/mbBlockChk` | MB Block status API URL |

## How It Works

### Token Generation Flow

1. **Token Request**
   ```json
   POST http://10.255.233.28:2000/token
   Content-Type: application/json
   
   {
     "username": "test1",
     "password": "test1@123"
   }
   ```

2. **Token Response**
   The API should return a token in one of these formats:
   ```json
   {
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
   }
   ```
   or
   ```json
   {
     "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
   }
   ```

3. **Status API Call**
   ```
   POST http://10.255.234.21:2000/mb/mbBlockChk
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   Content-Type: application/json
   
   {
     "requestId": "...",
     "channel": "CISCO",
     "data": { ... }
   }
   ```

## Implementation Details

### Automatic Token Generation

The application automatically generates tokens in production mode (when `testMode` is not set to `true`):

```csharp
// Token generation happens automatically before status API call
var tokenPayload = new
{
    username = configuration["MBBlock:Username"],
    password = configuration["MBBlock:Password"]
};

// Token is added to Authorization header
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", authToken);
```

### Error Handling

If token generation fails:
- The error is logged to the console
- The application continues and attempts the status API call without the token
- The middleware may return an authentication error if token is required

### Test Mode

In test mode (`?testMode=true`), token generation is skipped:
- No token API call is made
- No Authorization header is added
- Returns simulated response immediately

## Configuration Examples

### Example 1: Development Environment

```json
{
  "MBBlock": {
    "TokenUrl": "http://localhost:3000/token",
    "Username": "dev_user",
    "Password": "dev_pass123",
    "MiddlewareUrl": "http://localhost:3000/api/status"
  }
}
```

### Example 2: Production Environment

```json
{
  "MBBlock": {
    "TokenUrl": "https://api.production.com/auth/token",
    "Username": "prod_user",
    "Password": "prod_secure_password",
    "MiddlewareUrl": "https://api.production.com/mb/status"
  }
}
```

### Example 3: Environment Variables

Override configuration using environment variables:

**Windows:**
```cmd
set MBBlock__TokenUrl=http://custom.token.api/token
set MBBlock__Username=custom_user
set MBBlock__Password=custom_password
```

**Linux/Mac:**
```bash
export MBBlock__TokenUrl=http://custom.token.api/token
export MBBlock__Username=custom_user
export MBBlock__Password=custom_password
```

## Security Considerations

### Credential Storage

⚠️ **Important Security Notes:**

1. **Never commit passwords to source control**
   - Use environment variables for production credentials
   - Use secure key vaults (Azure Key Vault, AWS Secrets Manager, etc.)
   - Keep `appsettings.json` credentials for development only

2. **Use Strong Passwords**
   - Production passwords should be complex and unique
   - Rotate passwords regularly
   - Follow your organization's password policy

3. **Secure Communication**
   - Always use HTTPS in production (`https://` URLs)
   - Validate SSL certificates
   - Use TLS 1.2 or higher

4. **Token Security**
   - Tokens are not logged or exposed to frontend
   - Tokens are only used for the duration of the request
   - Tokens are not cached between requests

### Best Practices

1. **Configuration Management**
   ```json
   // Development (appsettings.Development.json)
   {
     "MBBlock": {
       "TokenUrl": "http://localhost:3000/token",
       "Username": "dev_user",
       "Password": "dev_pass"
     }
   }
   
   // Production (Environment Variables)
   MBBlock__Username=prod_user
   MBBlock__Password=$(secure_vault_reference)
   ```

2. **Logging**
   - Token values are never logged in full
   - Only the first 20 characters are logged for debugging
   - Failed authentication attempts are logged

3. **Error Messages**
   - Generic error messages are shown to users
   - Detailed errors are logged server-side only

## Testing

### Test Mode

Test the MB Block API without calling the real token API:

```bash
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123&testMode=true"
```

### Production Mode Testing

Test with token generation (ensure token API is accessible):

```bash
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123"
```

Check server logs for token generation status:
```
Token generated successfully: eyJhbGciOiJIUzI1Ni...
```

### Verify Authorization Header

Check that the Authorization header is being sent:

```bash
# Enable verbose logging in appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

## Troubleshooting

### Token Generation Fails

**Symptom:** "Token generation failed with status: 401"

**Solutions:**
1. Verify username and password in configuration
2. Check token API URL is correct and accessible
3. Verify network connectivity to token API
4. Check token API logs for authentication errors

### Token Not Accepted by Status API

**Symptom:** Status API returns 401 Unauthorized

**Solutions:**
1. Verify token is being generated successfully
2. Check token format (JWT, opaque token, etc.)
3. Verify token expiration time
4. Check if status API expects specific token format

### Token API Timeout

**Symptom:** "Token generation error: The operation has timed out"

**Solutions:**
1. Check network connectivity to token API
2. Increase timeout in configuration (default is 30 seconds)
3. Verify token API is responding

### Token Not Found in Response

**Symptom:** "Token generated successfully: ..."

**Solutions:**
1. Verify token response format matches expected format
2. Check if token is in `token` or `access_token` field
3. Update code to parse different token response format

## API Response

When token is successfully generated, the API response includes:

```json
{
  "success": true,
  "request": { ... },
  "response": { ... },
  "statusCode": 200,
  "tokenGenerated": true
}
```

The `tokenGenerated` field indicates whether a token was successfully obtained and used.

## Monitoring

### Success Indicators

✅ Token generated successfully
✅ Authorization header added to request
✅ Status API returns 200 OK
✅ Response includes `"tokenGenerated": true`

### Failure Indicators

❌ Token generation fails (logged)
❌ Status API returns 401 Unauthorized
❌ Response includes `"tokenGenerated": false`

### Logging

Server logs show token generation status:
```
Token generated successfully: eyJhbGciOiJIUzI1Ni...
```

Or on failure:
```
Token generation failed with status: 401
Token response: {"error":"Invalid credentials"}
```

## See Also

- [appsettings.json](appsettings.json) - Main configuration file
- [MBBLOCK_TECHNICAL_DOC.md](MBBLOCK_TECHNICAL_DOC.md) - Technical documentation
- [API_ENDPOINTS_CONFIG.md](API_ENDPOINTS_CONFIG.md) - Endpoint configuration guide
