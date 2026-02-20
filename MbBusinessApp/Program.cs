using MbBusinessApp.Utils;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add HttpClient for making API calls
builder.Services.AddHttpClient();

var app = builder.Build();

// Enable static files (for serving index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

// Get API endpoint paths from configuration
var firstApiPath = app.Configuration["ApiEndpoints:FirstApi"] ?? "/api/first";
var secondApiPath = app.Configuration["ApiEndpoints:SecondApi"] ?? "/api/second";
var mbBlockApiPath = app.Configuration["ApiEndpoints:MbBlockApi"] ?? "/api/mbblock";

// First API endpoint - receives message from query string
app.MapGet(firstApiPath, (string? message) =>
{
    var response = new
    {
        success = true,
        timestamp = DateTime.UtcNow,
        message = message ?? "Default message",
        source = "First API",
        description = "This response was generated from the query string parameter"
    };
    return Results.Json(response);
});

// Second API endpoint - called from button click
app.MapGet(secondApiPath, () =>
{
    var response = new
    {
        success = true,
        timestamp = DateTime.UtcNow,
        message = "This is the second API response",
        source = "Second API",
        description = "This response was generated from a button click",
        randomNumber = Random.Shared.Next(1, 1000)
    };
    return Results.Json(response);
});

// MB Block Check API endpoint
app.MapGet(mbBlockApiPath, async (string? mobileNumber, string? callId, IHttpClientFactory httpClientFactory, IConfiguration configuration, bool? testMode) =>
{
    var requestId = callId ?? Guid.NewGuid().ToString();
    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
    
    Console.WriteLine($"\n{'='*80}");
    Console.WriteLine($"[{timestamp}] MB BLOCK API REQUEST STARTED");
    Console.WriteLine($"Request ID: {requestId}");
    Console.WriteLine($"{'='*80}");
    
    try
    {
        // Validate inputs
        Console.WriteLine($"[{timestamp}] STEP 1: Validating inputs");
        if (string.IsNullOrEmpty(mobileNumber) || string.IsNullOrEmpty(callId))
        {
            Console.WriteLine($"[{timestamp}] ERROR: Validation failed - Mobile number or Call ID missing");
            Console.WriteLine($"  Mobile Number: {(string.IsNullOrEmpty(mobileNumber) ? "MISSING" : "PROVIDED")}");
            Console.WriteLine($"  Call ID: {(string.IsNullOrEmpty(callId) ? "MISSING" : "PROVIDED")}");
            return Results.BadRequest(new { error = "Mobile number and Call ID are required" });
        }
        Console.WriteLine($"  Mobile Number: {mobileNumber} (length: {mobileNumber.Length})");
        Console.WriteLine($"  Call ID: {callId}");
        Console.WriteLine($"  Test Mode: {testMode ?? false}");

        // Get configuration values
        Console.WriteLine($"\n[{timestamp}] STEP 2: Loading configuration");
        string channelCode = configuration["MBBlock:ChannelCode"] ?? "21";
        string channel = configuration["MBBlock:Channel"] ?? "CISCO";
        string clientId = configuration["MBBlock:ClientId"] ?? "900001";
        string baseKey = configuration["MBBlock:EncryptionBaseKey"] ?? "MBBOB12#";
        string hashKey = configuration["MBBlock:HashKey"] ?? "MB@nking";
        string middlewareUrl = configuration["MBBlock:MiddlewareUrl"] ?? "http://10.255.234.21:2000/mb/mbBlockChk";
        string tokenUrl = configuration["MBBlock:TokenUrl"] ?? "http://10.255.233.28:2000/token";
        string username = configuration["MBBlock:Username"] ?? "test1";
        string password = configuration["MBBlock:Password"] ?? "test1@123";
        
        Console.WriteLine($"  Channel Code: {channelCode}");
        Console.WriteLine($"  Channel: {channel}");
        Console.WriteLine($"  Client ID: {clientId}");
        Console.WriteLine($"  Base Key: {baseKey}");
        Console.WriteLine($"  Hash Key: {hashKey}");
        Console.WriteLine($"  Middleware URL: {middlewareUrl}");
        Console.WriteLine($"  Token URL: {tokenUrl}");
        Console.WriteLine($"  Username: {username}");
        Console.WriteLine($"  Password: {new string('*', password.Length)}");

        // Step 1: Create the JSON payload with mobile number and channel code
        Console.WriteLine($"\n[{timestamp}] STEP 3: Creating JSON payload");
        var payload = new
        {
            MOBILE_NUMBER = mobileNumber,
            CHANNEL_CODE = channelCode
        };
        string jsonPayload = JsonSerializer.Serialize(payload);
        Console.WriteLine($"  JSON Payload: {jsonPayload}");
        Console.WriteLine($"  Payload Length: {jsonPayload.Length} bytes");

        // Step 2: Generate encryption key (base key + current date)
        Console.WriteLine($"\n[{timestamp}] STEP 4: Generating encryption key");
        string encryptionKey = EncryptionUtils.GetEncryptionKey(baseKey);
        Console.WriteLine($"  Encryption Key: {encryptionKey}");
        Console.WriteLine($"  Key Length: {encryptionKey.Length} characters");

        // Step 3: Encrypt the payload
        Console.WriteLine($"\n[{timestamp}] STEP 5: Encrypting payload");
        Console.WriteLine($"  Algorithm: AES-256-GCM");
        string encryptedData = EncryptionUtils.Encrypt(jsonPayload, encryptionKey);
        Console.WriteLine($"  Encrypted Data (Base64): {encryptedData}");
        Console.WriteLine($"  Encrypted Length: {encryptedData.Length} characters");

        // Step 4: Generate HMAC hash using the hash key (MB@nking)
        Console.WriteLine($"\n[{timestamp}] STEP 6: Generating HMAC hash");
        Console.WriteLine($"  Hash Algorithm: HMAC-SHA256");
        Console.WriteLine($"  Hash Key: {hashKey}");
        string hash = EncryptionUtils.GetHmac(encryptedData, hashKey);
        Console.WriteLine($"  Hash (Hex): {hash}");
        Console.WriteLine($"  Hash Length: {hash.Length} characters");

        // Step 5: Create the final request payload
        Console.WriteLine($"\n[{timestamp}] STEP 7: Building final request payload");
        var requestPayload = new
        {
            requestId = callId,
            channel = channel,
            data = new
            {
                encData = encryptedData,
                hash = hash,
                clientId = clientId
            }
        };
        string requestJson = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine($"  Request Payload:\n{requestJson}");

        string responseBody;
        int statusCode;
        string? authToken = null;

        // Step 6: Call the middleware API (or use test mode)
        if (testMode == true)
        {
            Console.WriteLine($"\n[{timestamp}] STEP 8: Test mode enabled - returning simulated response");
            // Test mode - return simulated response
            responseBody = JsonSerializer.Serialize(new
            {
                success = true,
                message = "Test mode - middleware not called",
                data = new
                {
                    status = "SUCCESS",
                    blockStatus = "NOT_BLOCKED",
                    responseCode = "00",
                    responseMessage = "Transaction Successful"
                }
            });
            statusCode = 200;
            Console.WriteLine($"  Simulated Response: {responseBody}");
            Console.WriteLine($"\n[{timestamp}] REQUEST COMPLETED SUCCESSFULLY (TEST MODE)");
            Console.WriteLine($"{'='*80}\n");
            
            // Return response with debug info for test mode
            return Results.Ok(new
            {
                success = true,
                request = requestPayload,
                response = responseBody,
                statusCode = statusCode,
                originalPayload = jsonPayload // Show original payload only in test mode
            });
        }
        else
        {
            Console.WriteLine($"\n[{timestamp}] STEP 8: Production mode - calling real APIs");
            var httpClient = httpClientFactory.CreateClient();
            // Set timeout to 30 seconds
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            Console.WriteLine($"  HTTP Client timeout set to: 30 seconds");
            
            // Step 6a: Generate authentication token first
            Console.WriteLine($"\n[{timestamp}] STEP 9: Generating authentication token");
            Console.WriteLine($"  Token URL: {tokenUrl}");
            try
            {
                var tokenPayload = new
                {
                    username = username,
                    password = password
                };
                var tokenJson = JsonSerializer.Serialize(tokenPayload);
                Console.WriteLine($"  Token request payload: {{\"username\":\"{username}\",\"password\":\"***\"}}");
                var tokenContent = new StringContent(tokenJson, System.Text.Encoding.UTF8, "application/json");
                
                Console.WriteLine($"  Sending POST request to token API...");
                var tokenStartTime = DateTime.UtcNow;
                var tokenResponse = await httpClient.PostAsync(tokenUrl, tokenContent);
                var tokenElapsed = (DateTime.UtcNow - tokenStartTime).TotalMilliseconds;
                var tokenResponseBody = await tokenResponse.Content.ReadAsStringAsync();
                
                Console.WriteLine($"  Token API response received in {tokenElapsed}ms");
                Console.WriteLine($"  Status Code: {(int)tokenResponse.StatusCode} ({tokenResponse.StatusCode})");
                
                if (tokenResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"  Token Response Body: {tokenResponseBody}");
                    // Parse token from response
                    var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenResponseBody);
                    if (tokenData.TryGetProperty("token", out var tokenElement))
                    {
                        authToken = tokenElement.GetString();
                        Console.WriteLine($"  Token extracted from 'token' field");
                    }
                    else if (tokenData.TryGetProperty("access_token", out var accessTokenElement))
                    {
                        authToken = accessTokenElement.GetString();
                        Console.WriteLine($"  Token extracted from 'access_token' field");
                    }
                    
                    if (!string.IsNullOrEmpty(authToken))
                    {
                        Console.WriteLine($"  Token generated successfully: {authToken.Substring(0, Math.Min(20, authToken.Length))}... (truncated)");
                        Console.WriteLine($"  Token length: {authToken.Length} characters");
                    }
                    else
                    {
                        Console.WriteLine($"  WARNING: Token response successful but token not found in response");
                    }
                }
                else
                {
                    Console.WriteLine($"  ERROR: Token generation failed with status: {tokenResponse.StatusCode}");
                    Console.WriteLine($"  Token response body: {tokenResponseBody}");
                }
            }
            catch (Exception tokenEx)
            {
                Console.WriteLine($"  ERROR: Token generation exception: {tokenEx.Message}");
                Console.WriteLine($"  Exception type: {tokenEx.GetType().Name}");
                Console.WriteLine($"  Stack trace: {tokenEx.StackTrace}");
                // Continue without token - the middleware might still work or return appropriate error
            }
            
            // Step 6b: Call the MB Block API with token
            Console.WriteLine($"\n[{timestamp}] STEP 10: Calling MB Block middleware API");
            Console.WriteLine($"  Middleware URL: {middlewareUrl}");
            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            // Add Authorization header if token was obtained
            if (!string.IsNullOrEmpty(authToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                Console.WriteLine($"  Authorization header added: Bearer {authToken.Substring(0, Math.Min(20, authToken.Length))}... (truncated)");
            }
            else
            {
                Console.WriteLine($"  WARNING: No authorization token available - proceeding without token");
            }

            Console.WriteLine($"  Sending POST request to middleware API...");
            var apiStartTime = DateTime.UtcNow;
            var apiResponse = await httpClient.PostAsync(middlewareUrl, content);
            var apiElapsed = (DateTime.UtcNow - apiStartTime).TotalMilliseconds;
            responseBody = await apiResponse.Content.ReadAsStringAsync();
            statusCode = (int)apiResponse.StatusCode;
            
            Console.WriteLine($"  Middleware API response received in {apiElapsed}ms");
            Console.WriteLine($"  Status Code: {statusCode} ({apiResponse.StatusCode})");
            Console.WriteLine($"  Response Body: {responseBody}");
            Console.WriteLine($"  Response Length: {responseBody.Length} characters");
            
            // Step 11: Decrypt the data.response field if present
            Console.WriteLine($"\n[{timestamp}] STEP 11: Processing response and decrypting data.response field");
            object decryptedResponse = responseBody;
            
            try
            {
                // Parse the response JSON
                var responseJson = JsonSerializer.Deserialize<JsonElement>(responseBody);
                
                // Check if data.response field exists and contains encrypted data
                if (responseJson.TryGetProperty("data", out var dataElement) && 
                    dataElement.TryGetProperty("response", out var responseElement))
                {
                    var encryptedResponseData = responseElement.GetString();
                    if (!string.IsNullOrEmpty(encryptedResponseData))
                    {
                        Console.WriteLine($"  Found encrypted data.response field");
                        Console.WriteLine($"  Encrypted data length: {encryptedResponseData.Length} characters");
                        Console.WriteLine($"  Encrypted data (first 50 chars): {encryptedResponseData.Substring(0, Math.Min(50, encryptedResponseData.Length))}...");
                        
                        // Decrypt the data.response field
                        Console.WriteLine($"  Decrypting data.response using encryption key: {encryptionKey}");
                        var decryptedData = EncryptionUtils.Decrypt(encryptedResponseData, encryptionKey);
                        Console.WriteLine($"  Decryption successful!");
                        Console.WriteLine($"  Decrypted data length: {decryptedData.Length} characters");
                        Console.WriteLine($"  Decrypted data: {decryptedData}");
                        
                        // Try to parse the decrypted data as JSON
                        try
                        {
                            var decryptedJson = JsonSerializer.Deserialize<JsonElement>(decryptedData);
                            decryptedResponse = decryptedJson;
                            Console.WriteLine($"  Decrypted data is valid JSON");
                        }
                        catch
                        {
                            // If not JSON, return as string
                            decryptedResponse = decryptedData;
                            Console.WriteLine($"  Decrypted data is plain text (not JSON)");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  data.response field is empty or null");
                        Console.WriteLine($"  Returning original response as-is");
                    }
                }
                else
                {
                    Console.WriteLine($"  No encrypted data.response field found in response");
                    Console.WriteLine($"  Returning original response as-is");
                }
            }
            catch (Exception decryptEx)
            {
                Console.WriteLine($"  ERROR during decryption: {decryptEx.Message}");
                Console.WriteLine($"  Returning original response");
                decryptedResponse = responseBody;
            }
            
            Console.WriteLine($"\n[{timestamp}] REQUEST COMPLETED");
            Console.WriteLine($"  Success: {apiResponse.IsSuccessStatusCode}");
            Console.WriteLine($"  Token Generated: {!string.IsNullOrEmpty(authToken)}");
            Console.WriteLine($"{'='*80}\n");
            
            // Return only the decrypted response
            return Results.Ok(new
            {
                success = true,
                response = decryptedResponse,
                statusCode = statusCode
            });
        }
    }
    catch (Exception ex)
    {
        // Log error server-side
        var errorTimestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        Console.WriteLine($"\n[{errorTimestamp}] *** ERROR OCCURRED ***");
        Console.WriteLine($"  Request ID: {requestId}");
        Console.WriteLine($"  Error Message: {ex.Message}");
        Console.WriteLine($"  Exception Type: {ex.GetType().Name}");
        Console.WriteLine($"  Stack Trace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
            Console.WriteLine($"  Inner Stack Trace: {ex.InnerException.StackTrace}");
        }
        Console.WriteLine($"{'='*80}\n");
        
        return Results.Ok(new
        {
            success = false,
            error = ex.Message
        });
    }
});

// API Configuration endpoint - returns configured API paths for frontend
app.MapGet("/api/config", (IConfiguration configuration) =>
{
    var config = new
    {
        endpoints = new
        {
            firstApi = configuration["ApiEndpoints:FirstApi"] ?? "/api/first",
            secondApi = configuration["ApiEndpoints:SecondApi"] ?? "/api/second",
            mbBlockApi = configuration["ApiEndpoints:MbBlockApi"] ?? "/api/mbblock"
        }
    };
    return Results.Json(config);
});

// Encrypt API endpoint - for testing encryption independently
app.MapPost("/api/encrypt", (IConfiguration configuration, EncryptRequest request) =>
{
    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
    Console.WriteLine($"\n[{timestamp}] ENCRYPT API CALLED");
    
    try
    {
        if (string.IsNullOrEmpty(request.Data))
        {
            Console.WriteLine($"  ERROR: Data is required");
            return Results.BadRequest(new { error = "Data is required" });
        }
        
        string baseKey = configuration["MBBlock:EncryptionBaseKey"] ?? "MBBOB12#";
        string encryptionKey = EncryptionUtils.GetEncryptionKey(baseKey);
        
        Console.WriteLine($"  Data to encrypt: {request.Data}");
        Console.WriteLine($"  Encryption key: {encryptionKey}");
        
        string encryptedData = EncryptionUtils.Encrypt(request.Data, encryptionKey);
        
        Console.WriteLine($"  Encrypted data: {encryptedData}");
        Console.WriteLine($"  SUCCESS");
        
        return Results.Ok(new
        {
            success = true,
            originalData = request.Data,
            encryptedData = encryptedData,
            encryptionKey = encryptionKey,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ERROR: {ex.Message}");
        Console.WriteLine($"  Stack trace: {ex.StackTrace}");
        return Results.Ok(new
        {
            success = false,
            error = ex.Message
        });
    }
});

// Decrypt API endpoint - for testing decryption independently
app.MapPost("/api/decrypt", (IConfiguration configuration, DecryptRequest request) =>
{
    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
    Console.WriteLine($"\n[{timestamp}] DECRYPT API CALLED");
    
    try
    {
        if (string.IsNullOrEmpty(request.EncryptedData))
        {
            Console.WriteLine($"  ERROR: Encrypted data is required");
            return Results.BadRequest(new { error = "Encrypted data is required" });
        }
        
        string baseKey = configuration["MBBlock:EncryptionBaseKey"] ?? "MBBOB12#";
        string encryptionKey = EncryptionUtils.GetEncryptionKey(baseKey);
        
        Console.WriteLine($"  Encrypted data to decrypt: {request.EncryptedData}");
        Console.WriteLine($"  Decryption key: {encryptionKey}");
        
        string decryptedData = EncryptionUtils.Decrypt(request.EncryptedData, encryptionKey);
        
        Console.WriteLine($"  Decrypted data: {decryptedData}");
        Console.WriteLine($"  SUCCESS");
        
        return Results.Ok(new
        {
            success = true,
            encryptedData = request.EncryptedData,
            decryptedData = decryptedData,
            encryptionKey = encryptionKey,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ERROR: {ex.Message}");
        Console.WriteLine($"  Stack trace: {ex.StackTrace}");
        return Results.Ok(new
        {
            success = false,
            error = ex.Message
        });
    }
});

app.Run();

// Request models for encrypt/decrypt
record EncryptRequest(string Data);
record DecryptRequest(string EncryptedData);
