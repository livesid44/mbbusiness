using MbBusinessApp.Utils;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add HttpClient for making API calls
builder.Services.AddHttpClient();

var app = builder.Build();

// Enable static files (for serving index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

// First API endpoint - receives message from query string
app.MapGet("/api/first", (string? message) =>
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
app.MapGet("/api/second", () =>
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
app.MapGet("/api/mbblock", async (string? mobileNumber, string? callId, IHttpClientFactory httpClientFactory, bool? testMode) =>
{
    try
    {
        // Validate inputs
        if (string.IsNullOrEmpty(mobileNumber) || string.IsNullOrEmpty(callId))
        {
            return Results.BadRequest(new { error = "Mobile number and Call ID are required" });
        }

        // Step 1: Create the JSON payload with mobile number and channel code
        var payload = new
        {
            MOBILE_NUMBER = mobileNumber,
            CHANNEL_CODE = "21"
        };
        string jsonPayload = JsonSerializer.Serialize(payload);

        // Step 2: Generate encryption key (base key + current date)
        string baseKey = "MBBOB12#";
        string encryptionKey = EncryptionUtils.GetEncryptionKey(baseKey);

        // Step 3: Encrypt the payload
        string encryptedData = EncryptionUtils.Encrypt(jsonPayload, encryptionKey);

        // Step 4: Generate HMAC hash
        string hash = EncryptionUtils.GetHmac(encryptedData, encryptionKey);

        // Step 5: Create the final request payload
        var requestPayload = new
        {
            requestId = callId,
            channel = "CISCO",
            data = new
            {
                encData = encryptedData,
                hash = hash,
                clientId = "900001"
            }
        };

        string responseBody;
        int statusCode;

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
        }
        else
        {
            var httpClient = httpClientFactory.CreateClient();
            // Set timeout to 30 seconds
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var apiUrl = "http://10.255.234.21:2000/mb/mbBlockChk";
            
            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var apiResponse = await httpClient.PostAsync(apiUrl, content);
            responseBody = await apiResponse.Content.ReadAsStringAsync();
            statusCode = (int)apiResponse.StatusCode;
        }

        // Return the response
        return Results.Ok(new
        {
            success = true,
            request = requestPayload,
            response = responseBody,
            statusCode = statusCode,
            encryptionKey = encryptionKey, // For debugging purposes
            originalPayload = jsonPayload // Show original unencrypted payload for verification
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            success = false,
            error = ex.Message,
            stackTrace = ex.StackTrace
        });
    }
});

app.Run();
