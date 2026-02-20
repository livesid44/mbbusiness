# Testing Guide - Sample URLs with Query Parameters

This guide provides comprehensive sample URLs for testing all endpoints in the MB Business application.

## 🚀 Prerequisites

Before testing, ensure the application is running:

```bash
cd MbBusinessApp
dotnet restore
dotnet run
```

The application will start on `http://localhost:5000` by default.

---

## 📋 Table of Contents

- [Web Interface URLs](#web-interface-urls)
- [API Endpoint URLs](#api-endpoint-urls)
- [Curl Commands for API Testing](#curl-commands-for-api-testing)
- [Testing Scenarios](#testing-scenarios)

---

## 🌐 Web Interface URLs

### 1. Main API Demo Page

**Basic Page Load (No Parameters):**
```
http://localhost:5000
```
- Displays default message from first API
- Shows button to trigger second API

**With Custom Message Parameter:**
```
http://localhost:5000/?message=Hello%20World
```
- First API displays: "Hello World"

**With Different Messages:**
```
http://localhost:5000/?message=Welcome%20to%20MB%20Business
http://localhost:5000/?message=Testing%20Query%20Parameters
http://localhost:5000/?message=Custom%20API%20Response
```

### 2. MB Business Block Check Page

**Test Mode (Recommended for Development):**
```
http://localhost:5000/mbblock.html?mobileNumber=9467223387&callId=A9EA8FD3FAA711F08BBEDE22482A56FD&testMode=true
```
- Simulates middleware response without actual API call
- Shows encrypted payload and hash
- Displays response in table format

**Test Mode with Different Parameters:**
```
http://localhost:5000/mbblock.html?mobileNumber=1234567890&callId=TEST123&testMode=true
http://localhost:5000/mbblock.html?mobileNumber=9876543210&callId=ABC456DEF789&testMode=true
http://localhost:5000/mbblock.html?mobileNumber=5555555555&callId=DEMO001&testMode=true
```

**Production Mode (Requires Middleware Access):**
```
http://localhost:5000/mbblock.html?mobileNumber=9467223387&callId=A9EA8FD3FAA711F08BBEDE22482A56FD
```
- Calls actual middleware API at `http://10.255.234.21:2000/mb/mbBlockChk`
- Requires network access to middleware server

**Missing Parameters (Error Testing):**
```
http://localhost:5000/mbblock.html
```
- Shows error: "Mobile number and Call ID are required"
- Displays example URL in error message

---

## 🔌 API Endpoint URLs

### 1. First API - `/api/first`

**Purpose:** Returns a JSON response with a custom message from query parameter.

**Basic Request:**
```
http://localhost:5000/api/first
```
Response:
```json
{
  "success": true,
  "timestamp": "2026-02-18T02:00:00.000Z",
  "message": "Default message",
  "source": "First API",
  "description": "This response was generated from the query string parameter"
}
```

**With Message Parameter:**
```
http://localhost:5000/api/first?message=Hello
```
Response:
```json
{
  "success": true,
  "timestamp": "2026-02-18T02:00:00.000Z",
  "message": "Hello",
  "source": "First API",
  "description": "This response was generated from the query string parameter"
}
```

**Sample Test URLs:**
```
http://localhost:5000/api/first?message=Testing
http://localhost:5000/api/first?message=API%20Working%20Correctly
http://localhost:5000/api/first?message=Custom%20Query%20String
http://localhost:5000/api/first?message=Special%20Characters%20%21%40%23
```

### 2. Second API - `/api/second`

**Purpose:** Returns a JSON response with a random number (no parameters required).

**Request:**
```
http://localhost:5000/api/second
```

Response:
```json
{
  "success": true,
  "timestamp": "2026-02-18T02:00:00.000Z",
  "message": "This is the second API response",
  "source": "Second API",
  "description": "This response was generated from a button click",
  "randomNumber": 567
}
```

### 3. MB Block Check API - `/api/mbblock`

**Purpose:** Encrypts mobile number and call ID, generates HMAC hash, and calls middleware API.

**Test Mode (Recommended for Development):**
```
http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=A9EA8FD3FAA711F08BBEDE22482A56FD&testMode=true
```

Response:
```json
{
  "success": true,
  "request": {
    "requestId": "A9EA8FD3FAA711F08BBEDE22482A56FD",
    "channel": "CISCO",
    "data": {
      "encData": "base64_encrypted_data_here",
      "hash": "hmac_sha256_hash_here",
      "clientId": "900001"
    }
  },
  "response": "{\"success\":true,\"message\":\"Test mode - middleware not called\",\"data\":{\"status\":\"SUCCESS\",\"blockStatus\":\"NOT_BLOCKED\",\"responseCode\":\"00\",\"responseMessage\":\"Transaction Successful\"}}",
  "statusCode": 200,
  "originalPayload": "{\"MOBILE_NUMBER\":\"9467223387\",\"CHANNEL_CODE\":\"21\"}"
}
```

**Sample Test Mode URLs:**
```
http://localhost:5000/api/mbblock?mobileNumber=1234567890&callId=TEST001&testMode=true
http://localhost:5000/api/mbblock?mobileNumber=9876543210&callId=TEST002&testMode=true
http://localhost:5000/api/mbblock?mobileNumber=5555555555&callId=DEMO123&testMode=true
```

**Production Mode (Requires Middleware Access):**
```
http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=A9EA8FD3FAA711F08BBEDE22482A56FD
```

**Error Cases:**
```
http://localhost:5000/api/mbblock
http://localhost:5000/api/mbblock?mobileNumber=9467223387
http://localhost:5000/api/mbblock?callId=TEST123
```
Response (400 Bad Request):
```json
{
  "error": "Mobile number and Call ID are required"
}
```

---

## 💻 Curl Commands for API Testing

### Testing First API

**Basic Request:**
```bash
curl "http://localhost:5000/api/first"
```

**With Message Parameter:**
```bash
curl "http://localhost:5000/api/first?message=Hello%20World"
```

**Pretty Print JSON:**
```bash
curl "http://localhost:5000/api/first?message=Testing" | python -m json.tool
# Or using jq
curl "http://localhost:5000/api/first?message=Testing" | jq
```

### Testing Second API

```bash
curl "http://localhost:5000/api/second"
```

**Pretty Print:**
```bash
curl "http://localhost:5000/api/second" | python -m json.tool
```

### Testing MB Block API

**Test Mode:**
```bash
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123&testMode=true"
```

**Pretty Print:**
```bash
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123&testMode=true" | python -m json.tool
```

**Save Response to File:**
```bash
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123&testMode=true" -o response.json
```

**Check Headers:**
```bash
curl -i "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123&testMode=true"
```

**Verbose Output:**
```bash
curl -v "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123&testMode=true"
```

---

## 🧪 Testing Scenarios

### Scenario 1: Basic Functionality Test

1. **Start the application**
   ```bash
   cd MbBusinessApp
   dotnet run
   ```

2. **Test main page**
   - Open browser: `http://localhost:5000`
   - Verify first API loads with default message
   - Click "Call Second API" button
   - Verify second API response appears

3. **Test with custom message**
   - Open: `http://localhost:5000/?message=My%20Custom%20Message`
   - Verify "My Custom Message" appears in first API response

### Scenario 2: MB Block Check - Test Mode

1. **Open MB Block page in test mode**
   ```
   http://localhost:5000/mbblock.html?mobileNumber=9467223387&callId=TEST123&testMode=true
   ```

2. **Verify display**
   - Input parameters section shows mobile number and call ID
   - Request payload section shows encrypted data and hash
   - API response section shows simulated success response in table
   - Debug information shows statusCode: 200

3. **Test with different parameters**
   ```
   http://localhost:5000/mbblock.html?mobileNumber=1234567890&callId=ABC456&testMode=true
   ```
   - Verify encryption changes with different mobile number
   - Verify hash changes with different data

### Scenario 3: API Direct Testing with Curl

1. **Test all endpoints**
   ```bash
   # First API
   curl "http://localhost:5000/api/first?message=Test1" | python -m json.tool
   
   # Second API
   curl "http://localhost:5000/api/second" | python -m json.tool
   
   # MB Block API
   curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST1&testMode=true" | python -m json.tool
   ```

2. **Verify responses**
   - All should return HTTP 200
   - All should return valid JSON
   - MB Block should show encrypted data and hash

### Scenario 4: Error Handling Test

1. **Test missing parameters**
   ```
   http://localhost:5000/mbblock.html
   ```
   - Should show error message with example URL

2. **Test API with missing parameters**
   ```bash
   curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387"
   ```
   - Should return 400 Bad Request
   - Should show error message about missing callId

### Scenario 5: Character Encoding Test

Test special characters in query parameters:

```
http://localhost:5000/api/first?message=Hello%20%26%20Welcome%21
http://localhost:5000/api/first?message=Test%20%2B%20Sign
http://localhost:5000/api/first?message=Email%3A%20test%40example.com
http://localhost:5000/api/first?message=Price%3A%20%2499.99
```

---

## 📊 Expected Results Quick Reference

| Endpoint | Parameters | Expected Status | Success Indicator |
|----------|-----------|-----------------|-------------------|
| `/` | none | 200 | Page loads with "Default message" |
| `/` | `?message=Test` | 200 | Page shows "Test" |
| `/api/first` | none | 200 | JSON with "Default message" |
| `/api/first` | `?message=Hello` | 200 | JSON with "Hello" |
| `/api/second` | none | 200 | JSON with random number |
| `/api/mbblock` | none | 400 | Error message about missing params |
| `/api/mbblock` | `?mobileNumber=xxx&callId=yyy&testMode=true` | 200 | JSON with encrypted data and simulated response |
| `/mbblock.html` | none | 200 | Page loads but shows error |
| `/mbblock.html` | `?mobileNumber=xxx&callId=yyy&testMode=true` | 200 | Page shows encrypted data and response table |

---

## 🔍 Advanced Testing

### Testing with Postman

Import these requests into Postman:

1. **GET First API**
   - URL: `http://localhost:5000/api/first`
   - Params: `message=Hello World`

2. **GET Second API**
   - URL: `http://localhost:5000/api/second`
   - No params

3. **GET MB Block API**
   - URL: `http://localhost:5000/api/mbblock`
   - Params: 
     - `mobileNumber=9467223387`
     - `callId=TEST123`
     - `testMode=true`

### Load Testing

Using Apache Bench (ab):

```bash
# Test first API with 100 requests, 10 concurrent
ab -n 100 -c 10 "http://localhost:5000/api/first?message=LoadTest"

# Test second API
ab -n 100 -c 10 "http://localhost:5000/api/second"

# Test MB Block API (test mode)
ab -n 100 -c 10 "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST&testMode=true"
```

### Automated Testing Script

Save as `test-all-endpoints.sh`:

```bash
#!/bin/bash

echo "Testing MB Business Application Endpoints"
echo "=========================================="
echo ""

BASE_URL="http://localhost:5000"

echo "1. Testing First API (no params)..."
curl -s "$BASE_URL/api/first" | python -m json.tool
echo ""

echo "2. Testing First API (with message)..."
curl -s "$BASE_URL/api/first?message=Test" | python -m json.tool
echo ""

echo "3. Testing Second API..."
curl -s "$BASE_URL/api/second" | python -m json.tool
echo ""

echo "4. Testing MB Block API (test mode)..."
curl -s "$BASE_URL/api/mbblock?mobileNumber=9467223387&callId=TEST123&testMode=true" | python -m json.tool
echo ""

echo "5. Testing MB Block API (missing params - should error)..."
curl -s "$BASE_URL/api/mbblock"
echo ""

echo "=========================================="
echo "Testing Complete!"
```

Run with:
```bash
chmod +x test-all-endpoints.sh
./test-all-endpoints.sh
```

---

## 🛠️ Troubleshooting

### Application Not Running

**Symptom:** `curl: (7) Failed to connect to localhost port 5000`

**Solution:**
```bash
cd MbBusinessApp
dotnet restore
dotnet run
```

### Port Already in Use

**Symptom:** `Address already in use`

**Solution:** Use a different port
```bash
dotnet run --urls "http://localhost:5001"
```
Then update URLs to use port 5001.

### CORS Errors (Browser Console)

If testing from a different origin, CORS might block requests. The application should allow same-origin requests by default.

---

## 📚 Additional Resources

- [SETUP.md](../SETUP.md) - Detailed setup instructions
- [MBBLOCK_TECHNICAL_DOC.md](../MbBusinessApp/MBBLOCK_TECHNICAL_DOC.md) - Technical documentation
- [README.md](../README.md) - Project overview

---

## 🆘 Need Help?

If you encounter issues:

1. Verify the application is running: `curl http://localhost:5000/api/second`
2. Check the console for error messages
3. Review the [SETUP.md](../SETUP.md) troubleshooting section
4. Ensure .NET 9.0 SDK is installed: `dotnet --version`

---

**Last Updated:** February 2026  
**Application Version:** 1.0  
**Target Framework:** .NET 9.0
