# PDM Rentals Azure Deployment Guide

This guide outlines the steps to deploy the RentalBlazorApp to Azure App Service without exposing secrets or local configurations.

## Prerequisites & Configuration
- **Azure Resource Group**: `rg-pdm-rentals`
- **App Service Name**: `pdm-rentals-app-2026`
- **Region**: `Central India`
- **OS**: `Windows`
- **Runtime Stack**: `.NET 10 (LTS)`

## 1. Configure Environment Variables in Azure

Before publishing the code, configure your production secrets in the Azure Portal so the application can read them securely.

1. Go to the Azure Portal.
2. Navigate to your App Service: `pdm-rentals-app-2026`.
3. Under the **Settings** section on the left, click **Environment variables** (or **Configuration** depending on the portal version).
4. Under the **App settings** tab, add the following variables:

### Database & Environment
| Name | Value |
|------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | `Data Source=Data/pdmrentals.db` |

### Supabase Settings
| Name | Value |
|------|-------|
| `Supabase__Url` | `https://uslbuyaouymccltdwbmm.supabase.co` |
| `Supabase__AnonKey` | *(Your actual Supabase Anon Key)* |

### Groq AI Settings
| Name | Value |
|------|-------|
| `Groq__ApiKey` | *(Your actual Groq API Key)* |
| `Groq__BaseUrl` | `https://api.groq.com/openai/v1/` |
| `Groq__ModelName` | `llama-3.1-8b-instant` |

### Email Settings
| Name | Value |
|------|-------|
| `EmailSettings__SmtpServer` | `smtp.gmail.com` |
| `EmailSettings__SmtpPort` | `587` |
| `EmailSettings__SenderName` | `PDM Rentals` |
| `EmailSettings__SenderEmail` | `no-reply@pdmrentals.com` |
| `EmailSettings__SmtpUsername` | `no-reply@pdmrentals.com` |
| `EmailSettings__SmtpPassword` | *(Your App Password)* |

**Important Note on Configuration**: ASP.NET Core translates double underscores (`__`) into colons for nested JSON configuration (e.g., `Supabase__Url` maps to `Supabase:Url`).

## 2. Publish the Application Locally

Open your terminal, navigate to the `RentalBlazorApp` directory, and run:

```shell
dotnet restore
dotnet build
dotnet publish -c Release
```

This will compile the application and output the deployment-ready files to:
`bin\Release\net10.0\publish\`

## 3. Deploy to Azure

Since Continuous Deployment is disabled, you can manually deploy the application using the "Zip Deploy" method (the simplest approach for a single App Service):

1. Navigate to your `bin\Release\net10.0\publish\` folder.
2. Select **all** the files and folders inside (including `wwwroot`, `appsettings.json`, `.dll` files, etc.).
3. Right-click and compress them into a `.zip` file (e.g., `publish.zip`).
4. Go to the Kudu ZipDeploy endpoint for your App Service in your browser:
   `https://pdm-rentals-app-2026.scm.azurewebsites.net/ZipDeployUI`
5. Drag and drop your `publish.zip` file onto the page.
6. Azure will automatically extract the files and restart the application.

## 4. Verification

1. Open a browser and visit: `https://pdm-rentals-app-2026.azurewebsites.net`
2. **Database Verification**: The SQLite database will automatically be created in the `Data` folder and seeded with the initial cars and the Admin account on the first startup.
3. **AI Verification**: Try sending a message in the chat to confirm `Groq` environment variables are working.
4. **Email & PDF Verification**: Try completing a booking to ensure the PDF is generated and emailed correctly.

## 5. Troubleshooting & Logs

If the application throws a 500 Internal Server Error, you can inspect the startup logs:

### Viewing Logs via Log Stream
1. Go to your App Service in the Azure Portal.
2. On the left menu, under **Monitoring**, click **Log stream**.
3. Under **App Service logs**, ensure "Application Logging (Filesystem)" is turned ON.
4. View the real-time logs to see if a missing environment variable or connection string caused an error.

### Common Errors
- **"HTTP Error 500.30 - ASP.NET Core app failed to start"**: Usually caused by a missing environment variable, causing a null reference during startup configuration.
- **"Directory not found" / SQLite exceptions**: If you removed the directory creation code in `Program.cs`, SQLite cannot create its `.db` file because the `Data/` directory does not exist. (We fixed this in the deployment preparation!)
- **"Connection Refused" (Email)**: Ensure you are using an App Password for Gmail, not your standard password, and that the SMTP port (`587`) matches your security settings.
