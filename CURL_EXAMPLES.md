# CURL Command Examples - MB Business APIs

This document provides confirmed CURL commands for all MB Business APIs with detailed explanations, parameters, and expected responses.

## 📋 Table of Contents

- [Prerequisites](#prerequisites)
- [MB Block Check API - Get Details](#mb-block-check-api---get-details)
- [MB Temp Block API - Temporary Block](#mb-temp-block-api---temporary-block)
- [Parameter Descriptions](#parameter-descriptions)
- [Response Format](#response-format)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

Ensure the application is running:

```bash
cd MbBusinessApp
dotnet run
```

The application will start on `http://localhost:5000` by default.

---

## MB Block Check API - Get Details

**Purpose:** Check if a mobile number is blocked in the system.

### Test Mode (No Middleware Call)

```bash
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123456&testMode=true"
```

**With Pretty Print:**
```bash
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123456&testMode=true" | jq .
```

**Expected Response (Test Mode):**
```json
{
  "success": true,
  "request": {
    "requestId": "TEST123456",
    "channel": "CISCO",
    "data": {
      "encData": "iBhGcfO+ynbxsOnCuB035wD/fOfrnXxch6bQzWCJh2/WnL5mGdTRn3I3t8BVKbBLxw...",
      "hash": "1ff534166b1cf0984d05ce87ad63f77055ce11f49bee4d1dae97c3358f3d4056",
      "clientId": "900001"
    }
  },
  "response": "{\"success\":true,\"message\":\"Test mode - middleware not called\",\"data\":{\"status\":\"SUCCESS\",\"blockStatus\":\"NOT_BLOCKED\",\"responseCode\":\"00\",\"responseMessage\":\"Transaction Successful\"}}",
  "statusCode": 200,
  "originalPayload": "{\"MOBILE_NUMBER\":\"9467223387\",\"CHANNEL_CODE\":\"21\"}"
}
```

**Note:** In test mode, the `response` field contains the full request details. The actual response data is embedded as a JSON string.

### Production Mode (Calls Actual Middleware)

```bash
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=A9EA8FD3FAA711F08BBEDE22482A56FD"
```

**With Headers and Verbose Output:**
```bash
curl -v "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=A9EA8FD3FAA711F08BBEDE22482A56FD"
```

**Save Response to File:**
```bash
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=A9EA8FD3FAA711F08BBEDE22482A56FD" \
  -o block_check_response.json
```

**Expected Response (Production Mode):**
```json
{
  "success": true,
  "response": {
    "status": "SUCCESS",
    "blockStatus": "NOT_BLOCKED",
    "responseCode": "00",
    "responseMessage": "Transaction Successful"
  },
  "statusCode": 200
}
```

### What Happens Internally (Production Mode):

1. **Payload Creation**: Creates `{"MOBILE_NUMBER": "9467223387", "CHANNEL_CODE": "21"}`
2. **Encryption**: Encrypts with AES-256-GCM using key `MBBOB12#yyyyMMdd`
3. **Hash Generation**: Creates HMAC-SHA256 hash using key `MB@nking`
4. **Token Generation**: Calls `http://10.255.233.28:2000/token` with credentials
5. **API Call**: POSTs to `http://10.255.234.21:2000/mb/mbBlockChk` with bearer token
6. **Response Decryption**: Decrypts `data.response` field if present
7. **Return**: Returns decrypted response

---

## MB Temp Block API - Temporary Block

**Purpose:** Temporarily block a mobile number with a specific reason.

### Test Mode (No Middleware Call)

```bash
curl "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=TEST123456&reason=fraud&testMode=true"
```

**With Pretty Print:**
```bash
curl "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=TEST123456&reason=fraud&testMode=true" | jq .
```

**Expected Response (Test Mode):**
```json
{
  "success": true,
  "response": {
    "status": "SUCCESS",
    "blockStatus": "TEMPORARILY_BLOCKED",
    "responseCode": "00",
    "responseMessage": "Mobile number temporarily blocked"
  },
  "statusCode": 200
}
```

### Production Mode (Calls Actual Middleware)

```bash
curl "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=A9EA8FD3FAA711F08BBEDE22482A56FD01&reason=suspicious_activity"
```

**With Different Reasons:**
```bash
# Fraud detection
curl "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=REQ123&reason=fraud"

# Suspicious activity
curl "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=REQ124&reason=suspicious_activity"

# Customer request
curl "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=REQ125&reason=customer_request"

# Security concern
curl "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=REQ126&reason=security_concern"
```

**Save Response to File:**
```bash
curl "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=REQ123&reason=fraud" \
  -o temp_block_response.json
```

**Expected Response (Production Mode):**
```json
{
  "success": true,
  "response": {
    "status": "SUCCESS",
    "blockStatus": "TEMPORARILY_BLOCKED",
    "responseCode": "00",
    "responseMessage": "Mobile number temporarily blocked",
    "blockReason": "fraud",
    "blockedUntil": "2026-02-21T14:00:00Z"
  },
  "statusCode": 200
}
```

### What Happens Internally (Production Mode):

1. **Payload Creation**: Creates `{"channel_CODE": "21", "mobile_NUMBER": "9467223387", "reason": "fraud"}`
2. **Encryption**: Encrypts with AES-256-GCM using key `MBBOB12#yyyyMMdd`
3. **Hash Generation**: Creates HMAC-SHA256 hash using key `MB@nking`
4. **Token Generation**: Calls `http://10.255.233.28:2000/token` with credentials
5. **API Call**: POSTs to `http://10.255.234.21:2000/mb/mbTempBlock` with bearer token
6. **Response Decryption**: Decrypts `data.response` field if present
7. **Return**: Returns decrypted response

---

## Parameter Descriptions

### MB Block Check API Parameters

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| `mobileNumber` | Yes | Mobile number to check (10 digits) | `9467223387` |
| `callId` | Yes | Unique request identifier | `TEST123456` or `A9EA8FD3FAA711F08BBEDE22482A56FD` |
| `testMode` | No | If `true`, skips middleware call and returns simulated response | `true` or `false` (default: `false`) |

### MB Temp Block API Parameters

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| `mobileNumber` | Yes | Mobile number to block (10 digits) | `9467223387` |
| `callId` | Yes | Unique request identifier | `TEST123456` or `A9EA8FD3FAA711F08BBEDE22482A56FD01` |
| `reason` | Yes | Reason for blocking | `fraud`, `suspicious_activity`, `customer_request`, etc. |
| `testMode` | No | If `true`, skips middleware call and returns simulated response | `true` or `false` (default: `false`) |

---

## Response Format

### Success Response

```json
{
  "success": true,
  "response": {
    "status": "SUCCESS",
    "blockStatus": "NOT_BLOCKED" | "TEMPORARILY_BLOCKED" | "PERMANENTLY_BLOCKED",
    "responseCode": "00",
    "responseMessage": "Transaction Successful"
  },
  "statusCode": 200
}
```

### Error Response

```json
{
  "success": false,
  "error": "Error message describing what went wrong",
  "statusCode": 400 | 500
}
```

### Response Fields

| Field | Description | Possible Values |
|-------|-------------|-----------------|
| `success` | Indicates if the request was successful | `true`, `false` |
| `response.status` | Status of the operation | `SUCCESS`, `FAILURE`, `ERROR` |
| `response.blockStatus` | Block status of the mobile number | `NOT_BLOCKED`, `TEMPORARILY_BLOCKED`, `PERMANENTLY_BLOCKED` |
| `response.responseCode` | Response code from middleware | `00` (success), `01` (error), etc. |
| `response.responseMessage` | Human-readable message | Various messages |
| `statusCode` | HTTP status code | `200`, `400`, `500`, etc. |

---

## Advanced CURL Examples

### 1. Batch Testing Multiple Numbers

```bash
#!/bin/bash
# Test multiple mobile numbers

NUMBERS=("9467223387" "9876543210" "9123456789")

for NUMBER in "${NUMBERS[@]}"; do
  echo "Testing: $NUMBER"
  curl -s "http://localhost:5000/api/mbblock?mobileNumber=$NUMBER&callId=BATCH_$(date +%s)&testMode=true" | jq '.response.blockStatus'
  echo "---"
done
```

### 2. Timing API Calls

```bash
# Measure response time
time curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=PERF_TEST&testMode=true"
```

### 3. Save Request and Response Headers

```bash
# Save all headers
curl -v "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=DEBUG_TEST&testMode=true" \
  --stderr - | tee curl_debug.log
```

### 4. Silent Mode (No Progress Bar)

```bash
# Useful for scripts
curl -s "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=SILENT_TEST&testMode=true"
```

### 5. Extract Specific Field from Response

```bash
# Get only block status
curl -s "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST&testMode=true" | \
  jq -r '.response.blockStatus'

# Output: NOT_BLOCKED
```

---

## Troubleshooting

### Issue: Connection Refused

**Error:**
```
curl: (7) Failed to connect to localhost port 5000: Connection refused
```

**Solution:**
- Ensure the application is running: `dotnet run` in the MbBusinessApp directory
- Check if the port is correct (default is 5000)
- Check if another application is using the port

### Issue: Invalid Mobile Number

**Error:**
```json
{
  "success": false,
  "error": "Invalid mobile number. Must be 10 digits.",
  "statusCode": 400
}
```

**Solution:**
- Ensure mobile number is exactly 10 digits
- Remove any spaces, dashes, or country codes
- Example: Use `9467223387` not `+91-9467223387`

### Issue: Missing Required Parameter

**Error:**
```json
{
  "success": false,
  "error": "Missing required parameter: reason",
  "statusCode": 400
}
```

**Solution:**
- Check that all required parameters are provided
- For MB Temp Block API, `reason` parameter is mandatory

### Issue: Middleware Not Responding

**Error:**
```json
{
  "success": false,
  "error": "Middleware API request failed with status: 500",
  "statusCode": 500
}
```

**Solution:**
- Check middleware connectivity
- Use `testMode=true` to test without middleware
- Check server logs for detailed error information

### Issue: Token Generation Failed

**Error:**
```json
{
  "success": false,
  "error": "Failed to generate authentication token",
  "statusCode": 500
}
```

**Solution:**
- Check token API connectivity at `http://10.255.233.28:2000/token`
- Verify credentials in `appsettings.json`
- Check server logs for detailed token generation errors

---

## Complete Example Workflow

### Workflow 1: Check Status Then Block

```bash
#!/bin/bash
# 1. Check if number is blocked
MOBILE="9467223387"
CALL_ID="WORKFLOW_$(date +%s)"

echo "Step 1: Checking block status..."
STATUS=$(curl -s "http://localhost:5000/api/mbblock?mobileNumber=$MOBILE&callId=$CALL_ID&testMode=true" | \
  jq -r '.response.blockStatus')

echo "Current status: $STATUS"

# 2. If not blocked, temporarily block it
if [ "$STATUS" = "NOT_BLOCKED" ]; then
  echo "Step 2: Temporarily blocking..."
  curl -s "http://localhost:5000/api/mbTempBlock?mobileNumber=$MOBILE&callId=${CALL_ID}_BLOCK&reason=fraud&testMode=true" | \
    jq '.response'
else
  echo "Already blocked. Skipping temp block."
fi
```

### Workflow 2: Production Deployment Test

```bash
#!/bin/bash
# Test production APIs (without test mode)

echo "=== Production API Test ==="
echo ""

# Block Check
echo "1. Block Check API:"
curl -s "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=PROD_TEST_$(date +%s)" | \
  jq '{status: .response.status, blockStatus: .response.blockStatus}'
echo ""

# Temp Block
echo "2. Temp Block API:"
curl -s "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=PROD_TEST_$(date +%s)&reason=test" | \
  jq '{status: .response.status, blockStatus: .response.blockStatus}'
echo ""

echo "=== Test Complete ==="
```

---

## Quick Reference

### Block Check (Test Mode)
```bash
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123&testMode=true"
```

### Temp Block (Test Mode)
```bash
curl "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=TEST123&reason=fraud&testMode=true"
```

### Block Check (Production)
```bash
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=A9EA8FD3FAA711F08BBEDE22482A56FD"
```

### Temp Block (Production)
```bash
curl "http://localhost:5000/api/mbTempBlock?mobileNumber=9467223387&callId=A9EA8FD3FAA711F08BBEDE22482A56FD01&reason=fraud"
```

---

## Additional Resources

- **Setup Guide**: See [SETUP.md](SETUP.md) for initial setup instructions
- **Testing Guide**: See [TESTING.md](TESTING.md) for comprehensive testing scenarios
- **API Documentation**: See [MBBLOCK_TECHNICAL_DOC.md](MbBusinessApp/MBBLOCK_TECHNICAL_DOC.md) for technical details
- **Configuration**: See [API_ENDPOINTS_CONFIG.md](MbBusinessApp/API_ENDPOINTS_CONFIG.md) for endpoint configuration

---

## Support

For issues or questions:
1. Check server logs in the console where `dotnet run` is executed
2. Review error messages in the response
3. Verify configuration in `appsettings.json`
4. Use `testMode=true` to isolate issues

---

**Last Updated**: 2026-02-20
