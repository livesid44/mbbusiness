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
    try
    {
        // Validate inputs
        if (string.IsNullOrEmpty(mobileNumber) || string.IsNullOrEmpty(callId))
        {
            return Results.BadRequest(new { error = "Mobile number and Call ID are required" });
        }

        // Get configuration values
        string channelCode = configuration["MBBlock:ChannelCode"] ?? "21";
        string channel = configuration["MBBlock:Channel"] ?? "CISCO";
        string clientId = configuration["MBBlock:ClientId"] ?? "900001";
        string baseKey = configuration["MBBlock:EncryptionBaseKey"] ?? "MBBOB12#";
        string middlewareUrl = configuration["MBBlock:MiddlewareUrl"] ?? "http://10.255.234.21:2000/mb/mbBlockChk";
        string tokenUrl = configuration["MBBlock:TokenUrl"] ?? "http://10.255.233.28:2000/token";
        string username = configuration["MBBlock:Username"] ?? "test1";
        string password = configuration["MBBlock:Password"] ?? "test1@123";

        // Step 1: Create the JSON payload with mobile number and channel code
        var payload = new
        {
            MOBILE_NUMBER = mobileNumber,
            CHANNEL_CODE = channelCode
        };
        string jsonPayload = JsonSerializer.Serialize(payload);

        // Step 2: Generate encryption key (base key + current date)
        string encryptionKey = EncryptionUtils.GetEncryptionKey(baseKey);

        // Step 3: Encrypt the payload
        string encryptedData = EncryptionUtils.Encrypt(jsonPayload, encryptionKey);

        // Step 4: Generate HMAC hash
        string hash = EncryptionUtils.GetHmac(encryptedData, encryptionKey);

        // Step 5: Create the final request payload
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

        string responseBody;
        int statusCode;
        string? authToken = null;

        // Step 6: Call the middleware API (or use test mode)
        if (testMode == true)
        {
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
            var httpClient = httpClientFactory.CreateClient();
            // Set timeout to 30 seconds
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            // Step 6a: Generate authentication token first
            try
            {
                var tokenPayload = new
                {
                    username = username,
                    password = password
                };
                var tokenJson = JsonSerializer.Serialize(tokenPayload);
                var tokenContent = new StringContent(tokenJson, System.Text.Encoding.UTF8, "application/json");
                
                var tokenResponse = await httpClient.PostAsync(tokenUrl, tokenContent);
                var tokenResponseBody = await tokenResponse.Content.ReadAsStringAsync();
                
                if (tokenResponse.IsSuccessStatusCode)
                {
                    // Parse token from response
                    var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenResponseBody);
                    if (tokenData.TryGetProperty("token", out var tokenElement))
                    {
                        authToken = tokenElement.GetString();
                    }
                    else if (tokenData.TryGetProperty("access_token", out var accessTokenElement))
                    {
                        authToken = accessTokenElement.GetString();
                    }
                    
                    Console.WriteLine($"Token generated successfully: {authToken?.Substring(0, Math.Min(20, authToken?.Length ?? 0))}...");
                }
                else
                {
                    Console.WriteLine($"Token generation failed with status: {tokenResponse.StatusCode}");
                    Console.WriteLine($"Token response: {tokenResponseBody}");
                }
            }
            catch (Exception tokenEx)
            {
                Console.WriteLine($"Token generation error: {tokenEx.Message}");
                // Continue without token - the middleware might still work or return appropriate error
            }
            
            // Step 6b: Call the MB Block API with token
            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            // Add Authorization header if token was obtained
            if (!string.IsNullOrEmpty(authToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }

            var apiResponse = await httpClient.PostAsync(middlewareUrl, content);
            responseBody = await apiResponse.Content.ReadAsStringAsync();
            statusCode = (int)apiResponse.StatusCode;
            
            // Return response without sensitive debug info for production
            return Results.Ok(new
            {
                success = true,
                request = requestPayload,
                response = responseBody,
                statusCode = statusCode,
                tokenGenerated = !string.IsNullOrEmpty(authToken)
            });
        }
    }
    catch (Exception ex)
    {
        // Log error server-side
        Console.WriteLine($"MB Block API Error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        
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

app.Run();
