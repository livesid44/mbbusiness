# MB Business Block Check - Technical Documentation

## Overview

The MB Business Block Check functionality provides a secure way to check mobile number blocking status through the middleware API. It implements AES-GCM encryption matching the Java implementation and integrates with the BOB World Business Block APIs.

## Architecture

```
┌─────────────┐         ┌──────────────┐         ┌─────────────────┐
│   Browser   │────────▶│  .NET API    │────────▶│   Middleware    │
│ mbblock.html│         │  /api/mbblock│         │  10.255.234.21  │
└─────────────┘         └──────────────┘         └─────────────────┘
      │                        │
      │                        ├─ Encrypt JSON
      │                        ├─ Generate HMAC
      │                        └─ POST to middleware
      │
      └─ Display results in table
```

## Workflow

### 1. Page Load
- User navigates to `mbblock.html` with query parameters
- Parameters: `mobileNumber` and `callId`
- JavaScript extracts parameters and calls backend API

### 2. Backend Processing
1. **Validate Input**: Ensure mobile number and call ID are provided
2. **Create Payload**: Generate JSON with mobile number and channel code
   ```json
   {
     "MOBILE_NUMBER": "9467223387",
     "CHANNEL_CODE": "21"
   }
   ```
3. **Generate Key**: Create encryption key with format `MBBOB12#yyyyMMdd` (UTC)
4. **Encrypt**: Use AES-256-GCM to encrypt the JSON payload
5. **Hash**: Generate HMAC-SHA256 hash of encrypted data
6. **Build Request**: Create middleware request payload
   ```json
   {
     "requestId": "<callId>",
     "channel": "CISCO",
     "data": {
       "encData": "<base64_encrypted_data>",
       "hash": "<hex_hash>",
       "clientId": "900001"
     }
   }
   ```
7. **Call Middleware**: POST to `http://10.255.234.21:2000/mb/mbBlockChk`
8. **Return Response**: Send response back to frontend

### 3. Frontend Display
- Show input parameters
- Display encrypted request payload
- Show API response in table format
- Display debug information (non-sensitive)

## Encryption Details

### Algorithm: AES-256-GCM (Galois/Counter Mode)

**Parameters:**
- **Key Size**: 256 bits (32 bytes)
- **IV Length**: 12 bytes (96 bits)
- **Tag Length**: 16 bytes (128 bits)
- **Mode**: GCM (Galois/Counter Mode) provides both encryption and authentication

**Key Generation:**
```
Encryption Key = Base Key + Current UTC Date
Example: MBBOB12#20260218
```

**Encrypted Message Structure:**
```
[IV: 12 bytes][Ciphertext: variable][Tag: 16 bytes]
```
All encoded as Base64 for transmission.

### HMAC-SHA256 Hash

**Purpose:** Data integrity verification

**Input:** Base64-encoded encrypted data
**Key:** Same as encryption key
**Output:** 64-character hex string

**Process:**
```
hash = HMAC-SHA256(encryptedData, encryptionKey)
```

## API Endpoints

### POST /api/mbblock

**Query Parameters:**
- `mobileNumber` (required): Mobile phone number
- `callId` (required): Unique call identifier
- `testMode` (optional): Set to `true` for test mode

**Test Mode Response:**
```json
{
  "success": true,
  "request": { /* request payload */ },
  "response": { /* simulated response */ },
  "statusCode": 200,
  "originalPayload": "{ /* unencrypted payload */ }"
}
```

**Production Mode Response:**
```json
{
  "success": true,
  "request": { /* request payload */ },
  "response": { /* actual middleware response */ },
  "statusCode": 200
}
```

Note: `originalPayload` is only included in test mode for security reasons.

## Configuration

All configuration is stored in `appsettings.json`:

```json
{
  "MBBlock": {
    "EncryptionBaseKey": "MBBOB12#",
    "MiddlewareUrl": "http://10.255.234.21:2000/mb/mbBlockChk",
    "ChannelCode": "21",
    "Channel": "CISCO",
    "ClientId": "900001"
  }
}
```

**Environment Variables:**
Configuration can be overridden using environment variables:
```bash
export MBBlock__EncryptionBaseKey="YOUR_KEY"
export MBBlock__MiddlewareUrl="https://api.example.com/endpoint"
```

## Security Considerations

### Implemented Security Measures

1. **No Sensitive Data Exposure**
   - Encryption keys never sent to clients
   - Stack traces logged server-side only
   - Original payload only in test mode

2. **Secure Configuration**
   - Sensitive values in configuration files
   - Can use environment variables or Azure Key Vault
   - No hardcoded secrets in source code

3. **Strong Encryption**
   - AES-256-GCM provides authenticated encryption
   - Random IV for each encryption operation
   - HMAC-SHA256 for data integrity

4. **Time Consistency**
   - Uses UTC time for key generation
   - Prevents timezone-related issues
   - Ensures consistency across distributed systems

5. **Input Validation**
   - Required parameters validated
   - Proper error handling without information leakage
   - Clear error messages for users

### Recommendations for Production

1. **Key Management**
   - Store base key in Azure Key Vault or similar
   - Rotate keys periodically
   - Use longer, randomly generated base keys

2. **Logging**
   - Implement structured logging (e.g., Serilog)
   - Log to centralized system (e.g., Application Insights)
   - Never log sensitive data (keys, full mobile numbers)

3. **Transport Security**
   - Use HTTPS for all communications
   - Implement certificate pinning if possible
   - Configure TLS 1.2 or higher

4. **Monitoring**
   - Monitor API response times
   - Track encryption failures
   - Alert on unusual patterns

## Testing

### Test Mode Usage

Access the page with `testMode=true`:
```
http://localhost:5001/mbblock.html?mobileNumber=9467223387&callId=TEST123&testMode=true
```

Benefits:
- No middleware connection required
- Returns simulated success response
- Shows original unencrypted payload
- Useful for development and debugging

### Manual Testing

1. **Test Encryption**:
   ```bash
   curl "http://localhost:5001/api/mbblock?mobileNumber=9467223387&callId=TEST123&testMode=true"
   ```

2. **Verify Response Structure**:
   - Check that `encData` is base64-encoded
   - Verify `hash` is 64-character hex string
   - Confirm `requestId` matches input `callId`

3. **Test Error Handling**:
   ```bash
   curl "http://localhost:5001/api/mbblock?mobileNumber=9467223387"
   # Should return error about missing callId
   ```

## Troubleshooting

### Common Issues

1. **"Mobile number and Call ID are required"**
   - Ensure both query parameters are provided
   - Check parameter names: `mobileNumber` and `callId`

2. **Encryption Failures**
   - Verify encryption key length (should be 16, 24, or 32 bytes after padding)
   - Check date format (yyyyMMdd)
   - Ensure UTC time is used

3. **Middleware Connection Timeout**
   - Verify network connectivity to 10.255.234.21
   - Check firewall rules
   - Use test mode for development

4. **Hash Mismatch**
   - Ensure same key used for encryption and hashing
   - Verify encrypted data is not modified before hashing
   - Check that base64 encoding is correct

## Code Examples

### Calling the API from JavaScript

```javascript
async function checkMobileBlock(mobileNumber, callId) {
    const response = await fetch(
        `/api/mbblock?mobileNumber=${encodeURIComponent(mobileNumber)}&callId=${encodeURIComponent(callId)}`
    );
    
    if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
    }
    
    const data = await response.json();
    return data;
}
```

### Calling the API from C#

```csharp
var httpClient = new HttpClient();
var response = await httpClient.GetAsync(
    $"/api/mbblock?mobileNumber={Uri.EscapeDataString(mobileNumber)}&callId={Uri.EscapeDataString(callId)}"
);

var json = await response.Content.ReadAsStringAsync();
var result = JsonSerializer.Deserialize<MBBlockResponse>(json);
```

## Maintenance

### Updating Encryption Key

1. Update `appsettings.json` or environment variable
2. Restart application
3. Verify with test mode
4. Test actual middleware connection

### Changing Middleware URL

1. Update `MBBlock:MiddlewareUrl` in configuration
2. Restart application
3. Test connectivity

### Logging Configuration

For production, configure proper logging in `Program.cs`:

```csharp
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights(); // If using Azure
```

## References

- **Java Reference**: `wwwroot/EncryptionDecryptionMB.txt`
- **API Documentation**: `wwwroot/Middleware Document for BOB World Business Block APIs.docx`
- **AES-GCM**: [NIST SP 800-38D](https://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38d.pdf)
- **HMAC-SHA256**: [RFC 2104](https://tools.ietf.org/html/rfc2104)
