# mbbusiness

A .NET web application repository.

## 🚀 Quick Start

### First Time Setup

If you get an error like `NETSDK1004: Assets file 'obj\project.assets.json' not found`, you need to restore NuGet packages:

**Windows:**
```batch
restore.bat
```

**Linux/Mac:**
```bash
chmod +x restore.sh
./restore.sh
```

**Or manually:**
```bash
cd MbBusinessApp
dotnet restore
dotnet build
dotnet run
```

📖 **For detailed setup instructions, see [SETUP.md](SETUP.md)**

---

## Projects

### MbBusinessApp

A simple ASP.NET Core web application with multiple features:

#### 1. API Demo (index.html)
Demonstrates basic API integration with query string parameters and button-triggered API calls.

**Features:**
- First API automatically called on page load with query string parameter support
- Second API triggered by button click
- Clean, responsive UI with real-time JSON response display

**Quick Start:**
```bash
cd MbBusinessApp
dotnet restore
dotnet run
```
Visit `http://localhost:5000` or `http://localhost:5000/?message=Your%20Custom%20Message`

#### 2. MB Business Block Check (mbblock.html)
Secure mobile number block status checking with AES-GCM encryption and middleware integration.

**Features:**
- AES-256-GCM encryption matching Java implementation
- HMAC-SHA256 hash generation for data integrity
- Query string parameter extraction (mobile number, call ID)
- Middleware API integration
- Professional HTML interface with table display
- Test mode for development without middleware access

**Quick Start:**
```bash
cd MbBusinessApp
dotnet restore
dotnet run
```

**Test Mode (Development):**
```
http://localhost:5000/mbblock.html?mobileNumber=9467223387&callId=ABC123&testMode=true
```

**Production Mode:**
```
http://localhost:5000/mbblock.html?mobileNumber=9467223387&callId=ABC123
```

**Technical Documentation:**
See [MBBLOCK_TECHNICAL_DOC.md](MbBusinessApp/MBBLOCK_TECHNICAL_DOC.md) for complete technical details, security considerations, and API documentation.

## Configuration

### API Endpoints

All API endpoints are now configurable in `appsettings.json`:

```json
{
  "ApiEndpoints": {
    "FirstApi": "/api/first",
    "SecondApi": "/api/second",
    "MbBlockApi": "/api/mbblock"
  }
}
```

**Benefits:**
- Change endpoint paths without code changes
- Different endpoints per environment
- Support for API versioning and gateway integration

See [API_ENDPOINTS_CONFIG.md](MbBusinessApp/API_ENDPOINTS_CONFIG.md) for complete configuration guide.

### MB Block Settings

MB Block functionality configuration:
```json
{
  "MBBlock": {
    "TokenUrl": "http://10.255.233.28:2000/token",
    "Username": "test1",
    "Password": "test1@123",
    "EncryptionBaseKey": "MBBOB12#",
    "MiddlewareUrl": "http://10.255.234.21:2000/mb/mbBlockChk",
    "ChannelCode": "21",
    "Channel": "CISCO",
    "ClientId": "900001"
  }
}
```

**Token Authentication:**
- Automatically generates authentication token before API calls
- Token added as Bearer token in Authorization header
- Configurable credentials per environment

See [TOKEN_AUTH_CONFIG.md](MbBusinessApp/TOKEN_AUTH_CONFIG.md) for token authentication guide.

## Troubleshooting

### "Assets file not found" Error (NETSDK1004)

This error occurs when NuGet packages haven't been restored. **Solution:**

1. Run the restore script: `restore.bat` (Windows) or `./restore.sh` (Linux/Mac)
2. Or manually: `cd MbBusinessApp && dotnet restore`

See [SETUP.md](SETUP.md) for more troubleshooting help.

## Security

- ✅ No sensitive data exposed to clients
- ✅ Configuration externalized from code
- ✅ Strong encryption (AES-256-GCM)
- ✅ Server-side logging only
- ✅ CodeQL security scan passed (0 alerts)

See detailed documentation for more information.

## Requirements

- .NET 9.0 SDK or later
- Windows, Linux, or macOS

## Documentation

- [SETUP.md](SETUP.md) - Detailed setup and troubleshooting guide
- [TESTING.md](TESTING.md) - **Sample URLs and testing guide with query parameters**
- [CURL_EXAMPLES.md](CURL_EXAMPLES.md) - **📋 Confirmed CURL commands for all APIs** ⭐
- [API_ENDPOINTS_CONFIG.md](MbBusinessApp/API_ENDPOINTS_CONFIG.md) - **API endpoints configuration guide**
- [TOKEN_AUTH_CONFIG.md](MbBusinessApp/TOKEN_AUTH_CONFIG.md) - **Token authentication configuration guide**
- [MBBLOCK_TECHNICAL_DOC.md](MbBusinessApp/MBBLOCK_TECHNICAL_DOC.md) - Technical documentation for MB Block feature

## 🧪 Quick Testing

### Browser Testing
Want to test the APIs in browser? See **[TESTING.md](TESTING.md)** for comprehensive sample URLs with query parameters!

**Quick test URLs:**
- Main page: `http://localhost:5000/?message=Hello%20World`
- First API: `http://localhost:5000/api/first?message=Test`
- Second API: `http://localhost:5000/api/second`
- MB Block (test mode): `http://localhost:5000/mbblock.html?mobileNumber=9467223387&callId=TEST123&testMode=true`

### CURL Testing
Need CURL commands? See **[CURL_EXAMPLES.md](CURL_EXAMPLES.md)** for confirmed CURL commands!

**Quick CURL commands:**
```bash
# MB Block Check API (Get Details)
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123&testMode=true"

# MB Temp Block API (Temporary Block)
curl "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=TEST123&reason=fraud&testMode=true"
```