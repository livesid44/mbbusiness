# Setup Guide

## Quick Start

If you're seeing the error:
```
Error NETSDK1004: Assets file 'obj\project.assets.json' not found.
Run a NuGet package restore to generate this file.
```

**Solution:** Run the following command in the project directory:

### Windows
```batch
cd MbBusinessApp
dotnet restore
dotnet build
```

Or simply double-click `restore.bat` in the root directory.

### Linux/Mac
```bash
cd MbBusinessApp
dotnet restore
dotnet build
```

Or run `./restore.sh` from the root directory.

---

## First-Time Setup

### Prerequisites
- .NET 9.0 SDK or later
- Visual Studio 2022, VS Code, or Rider (optional)

### Step-by-Step Instructions

1. **Clone the repository**
   ```bash
   git clone https://github.com/livesid44/mbbusiness.git
   cd mbbusiness
   ```

2. **Restore NuGet packages**
   ```bash
   cd MbBusinessApp
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Main API Demo: http://localhost:5000
   - MB Block Check: http://localhost:5000/mbblock.html

---

## Troubleshooting

### Issue: "Assets file not found" Error

**Cause:** NuGet packages need to be restored after cloning the repository.

**Solution:**
1. Navigate to the `MbBusinessApp` directory
2. Run `dotnet restore`
3. Run `dotnet build`

### Issue: Build Fails After Restore

**Possible Causes:**
- Incorrect .NET SDK version
- Corrupted NuGet cache

**Solutions:**

1. **Check .NET SDK version**
   ```bash
   dotnet --version
   ```
   Should be 9.0.x or later.

2. **Clear NuGet cache**
   ```bash
   dotnet nuget locals all --clear
   dotnet restore --force
   ```

3. **Clean and rebuild**
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

### Issue: Port Already in Use

**Solution:** Specify a different port
```bash
dotnet run --urls "http://localhost:5001"
```

---

## Development Workflow

### Using Visual Studio 2022

1. Open `MbBusinessApp.csproj` or the solution file
2. Visual Studio will automatically restore packages on open
3. Press F5 to build and run

### Using VS Code

1. Open the `MbBusinessApp` folder in VS Code
2. Install the C# extension
3. Run restore command: `dotnet restore`
4. Press F5 or use the terminal: `dotnet run`

### Using Command Line

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run

# Run with specific configuration
dotnet run --configuration Release

# Run with custom URL
dotnet run --urls "http://localhost:8080"
```

---

## Project Structure

```
mbbusiness/
├── MbBusinessApp/
│   ├── Program.cs              # Main application entry point
│   ├── MbBusinessApp.csproj    # Project file
│   ├── Utils/                  # Encryption utilities
│   ├── wwwroot/                # Static files (HTML, CSS, JS)
│   ├── appsettings.json        # Configuration
│   └── .gitignore              # Git ignore rules
├── README.md                   # Project overview
├── SETUP.md                    # This file
├── restore.bat                 # Windows restore script
└── restore.sh                  # Linux/Mac restore script
```

---

## Configuration

### MB Block Configuration

Edit `appsettings.json` to configure MB Block settings:

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

### Environment Variables

You can override settings using environment variables:
```bash
# Windows
set MBBlock__EncryptionBaseKey=YOUR_KEY
set MBBlock__MiddlewareUrl=https://api.example.com/endpoint

# Linux/Mac
export MBBlock__EncryptionBaseKey=YOUR_KEY
export MBBlock__MiddlewareUrl=https://api.example.com/endpoint
```

---

## Testing

### Test Mode

For testing without middleware connection:
```
http://localhost:5000/mbblock.html?mobileNumber=9467223387&callId=TEST123&testMode=true
```

### API Testing

Use curl or Postman to test endpoints:

```bash
# Test first API
curl "http://localhost:5000/api/first?message=Hello"

# Test second API
curl "http://localhost:5000/api/second"

# Test MB Block API (test mode)
curl "http://localhost:5000/api/mbblock?mobileNumber=9467223387&callId=TEST123&testMode=true"
```

---

## Additional Resources

- [.NET 9.0 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [NuGet Package Manager](https://docs.microsoft.com/en-us/nuget/)
- [MB Block Technical Documentation](MbBusinessApp/MBBLOCK_TECHNICAL_DOC.md)

---

## Getting Help

If you encounter issues not covered in this guide:

1. Check the [Technical Documentation](MbBusinessApp/MBBLOCK_TECHNICAL_DOC.md)
2. Review the [README](README.md)
3. Open an issue on GitHub with:
   - Error message
   - .NET SDK version (`dotnet --version`)
   - Operating system
   - Steps to reproduce
