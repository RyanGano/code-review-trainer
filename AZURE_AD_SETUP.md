# Azure AD Authentication Setup Guide

This application uses Microsoft Authentication Library (MSAL) to provide secure authentication through Azure Active Directory (Azure AD). This guide will help you configure Azure AD and integrate Azure Key Vault for secure credential management.

## Prerequisites

- Azure subscription
- Azure AD tenant
- Application registration in Azure AD
- Azure Key Vault (for production environments)

## Step 1: Azure AD Application Registration

1. Navigate to the [Azure Portal](https://portal.azure.com/)
2. Go to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Fill in the application details:
   - **Name**: Code Review Tester
   - **Supported account types**: Choose based on your needs (typically "Accounts in this organizational directory only")
   - **Redirect URI**: 
     - Type: Single-page application (SPA)
     - URI: `http://localhost:5173` (for development)

5. Click **Register**

## Step 2: Configure Application Settings

After registration, note down:
- **Application (client) ID**
- **Directory (tenant) ID**

### Authentication Settings

1. Go to **Authentication** in your app registration
2. Under **Single-page application**, add these redirect URIs:
   - `http://localhost:5173` (development)
   - `https://yourdomain.com` (production)
3. Under **Logout URLs**, add:
   - `http://localhost:5173` (development)
   - `https://yourdomain.com` (production)
4. Enable **Access tokens** and **ID tokens** under **Implicit grant and hybrid flows**

### API Permissions

1. Go to **API permissions**
2. Add the following permissions:
   - **Microsoft Graph** > **User.Read** (Delegated)
   - Add any additional permissions your application needs

### Expose an API (for backend authentication)

1. Go to **Expose an API**
2. Click **Set** next to Application ID URI (accept the default or customize)
3. Add a scope:
   - **Scope name**: `access_as_user`
   - **Who can consent**: Admins and users
   - **Admin consent display name**: Access Code Review Tester
   - **Admin consent description**: Allow the application to access Code Review Tester on behalf of the signed-in user
   - **User consent display name**: Access Code Review Tester
   - **User consent description**: Allow the application to access Code Review Tester on your behalf

## Step 3: Environment Configuration

### Development Environment

1. Copy `.env.example` to `.env.local` in the frontend directory
2. Fill in your Azure AD details:

```bash
VITE_AZURE_TENANT_ID=your-tenant-id
VITE_AZURE_CLIENT_ID=your-client-id
VITE_REDIRECT_URI=http://localhost:5173
VITE_POST_LOGOUT_REDIRECT_URI=http://localhost:5173
VITE_API_BASE_URL=http://localhost:5000
```

### Backend Configuration

Update `appsettings.Development.json`:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "Audience": "your-client-id"
  }
}
```

## Step 4: Azure Key Vault Setup (Production)

### Create Key Vault

1. In Azure Portal, create a new **Key Vault**
2. Note the **Vault URI**

### Add Secrets

Add the following secrets to your Key Vault:
- `AzureTenantId`
- `AzureClientId`

### Configure Managed Identity

1. Enable **System-assigned managed identity** for your Azure App Service
2. Grant the managed identity **Get** and **List** permissions on Key Vault secrets

### Update Production Configuration

For production, use Key Vault references in your `appsettings.json`:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "@Microsoft.KeyVault(VaultName=your-keyvault-name;SecretName=AzureTenantId)",
    "ClientId": "@Microsoft.KeyVault(VaultName=your-keyvault-name;SecretName=AzureClientId)",
    "Audience": "@Microsoft.KeyVault(VaultName=your-keyvault-name;SecretName=AzureClientId)"
  }
}
```

## Step 5: Testing the Authentication

### Development Testing

1. Start the backend:
   ```bash
   cd code-review-tester-service
   dotnet run
   ```

2. Start the frontend:
   ```bash
   cd code-review-tester-app
   npm run dev
   ```

3. Navigate to `http://localhost:5173`
4. You should see the authentication prompt
5. Sign in with your Azure AD account
6. After successful authentication, you should see user information and be able to call the protected API

### Verification Steps

1. **Unauthenticated Access**: Try accessing the API directly at `http://localhost:5000/` - should return 401 Unauthorized
2. **Authenticated Access**: After signing in through the frontend, the API calls should work
3. **User Information**: The `/user` endpoint should return your user claims
4. **Token Validation**: Check browser network tab to see Bearer tokens being sent

## Security Best Practices

1. **Never commit secrets**: Always use environment variables or Key Vault
2. **Use HTTPS in production**: Ensure all URLs use HTTPS in production
3. **Validate tokens**: The backend validates JWT tokens from Azure AD
4. **Minimal permissions**: Only request the permissions your application needs
5. **Regular rotation**: Regularly rotate secrets and certificates
6. **Monitor access**: Use Azure AD logs to monitor authentication attempts

## Troubleshooting

### Common Issues

1. **Redirect URI mismatch**: Ensure redirect URIs in Azure AD match your application URLs
2. **CORS errors**: Check CORS configuration in the backend
3. **Token validation errors**: Verify tenant ID and client ID configuration
4. **Popup blocked**: Users may need to allow popups for authentication

### Useful Resources

- [MSAL.js Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview)
- [Azure AD App Registration Guide](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
- [Azure Key Vault Integration](https://docs.microsoft.com/en-us/azure/key-vault/general/overview)