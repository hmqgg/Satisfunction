# Satisfunction
Azure Functions .NET Core web api to manage azure vms.

## How to deploy
1. Create Azure VM, get its Resource ID.
2. Create Azure AD app, get its Client ID, Client Secret and Tenant ID.
3. Grant Azure subscriptions roles `Contributor` to the app.
4. Create Azure Functions, set `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`, `AZURE_TENANT_ID` and `AZURE_VM_ID` environment variable.
5. Deploy this project to the Azure Functions service, set `AuthorizationLevel` before deployment.

## How to use
 * Post `action=start` or `action=stop` to `https://YOUR_SERVICE_NAME.azurewebsites.net/api/Manage` in order to start/deallocate the VM.
