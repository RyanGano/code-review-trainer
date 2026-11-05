import type { Configuration, PopupRequest } from "@azure/msal-browser";

/**
 * MSAL 5 sends every auth response (popups included) to a dedicated page that
 * only runs the redirect bridge. See redirect.html at the project root.
 */
export const redirectBridgeUri =
  import.meta.env.VITE_REDIRECT_URI || `${window.location.origin}/redirect.html`;

/**
 * Configuration object to be passed to MSAL instance on creation.
 * For a full list of MSAL.js configuration parameters, visit:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/configuration.md
 */
export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID || "YOUR_CLIENT_ID", // This is the ONLY mandatory field that you need to supply.
    authority: `https://login.microsoftonline.com/common`, // Defaults to "https://login.microsoftonline.com/common"
    redirectUri: redirectBridgeUri, // You must register this URI on Azure Portal/App Registration.
    postLogoutRedirectUri:
      import.meta.env.VITE_POST_LOGOUT_REDIRECT_URI || redirectBridgeUri, // Popup logout lands here, so it must also run the redirect bridge.
  },
  cache: {
    cacheLocation: "sessionStorage", // Configures cache location. "sessionStorage" is more secure, but "localStorage" gives you SSO between tabs.
  },
};

console.log("API Base URL:", import.meta.env.VITE_API_BASE_URL);

/**
 * Scopes you add here will be prompted for user consent during sign-in.
 * By default, MSAL.js will add OIDC scopes (openid, profile, email) to any login request.
 * For more information about OIDC scopes, visit:
 * https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent#openid-connect-scopes
 */
export const loginRequest: PopupRequest = {
  scopes: ["api://e8f9c30f-3840-4ac5-9fd2-f6fc5d8c0ae1/access_as_user"],
};

/**
 * Add here the scopes to request when obtaining an access token for MS Graph API. For more information, see:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/resources-and-scopes.md
 */
export const graphConfig = {
  graphMeEndpoint: "https://graph.microsoft.com/v1.0/me",
};

/**
 * API configuration
 */
// Ensure base API URL always has a trailing slash so concatenations like `${webApi}tests/...` are valid.
const rawApiBase = import.meta.env.VITE_API_BASE_URL || "http://localhost:5137";
const normalizedApiBase = rawApiBase.endsWith("/")
  ? rawApiBase
  : rawApiBase + "/";

export const apiConfig = {
  b2cScopes: ["api://e8f9c30f-3840-4ac5-9fd2-f6fc5d8c0ae1/access_as_user"],
  webApi: normalizedApiBase,
};

if (rawApiBase !== normalizedApiBase) {
  console.log(
    "Normalized API Base URL (added trailing slash):",
    normalizedApiBase
  );
}
