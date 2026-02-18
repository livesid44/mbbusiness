# mbbusiness

A .NET web application repository.

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

All configuration is in `appsettings.json`. For MB Block functionality:
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

## Security

- ✅ No sensitive data exposed to clients
- ✅ Configuration externalized from code
- ✅ Strong encryption (AES-256-GCM)
- ✅ Server-side logging only
- ✅ CodeQL security scan passed (0 alerts)

See detailed documentation for more information.