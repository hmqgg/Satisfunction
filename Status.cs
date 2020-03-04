using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Satisfunction
{
    public static class Status
    {
        /// <exception cref="System.Security.SecurityException">Ignore.</exception>
        [FunctionName("Status")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed status request.");

            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID", EnvironmentVariableTarget.Process);
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET", EnvironmentVariableTarget.Process);
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID", EnvironmentVariableTarget.Process);
            var vmId = Environment.GetEnvironmentVariable("AZURE_VM_ID", EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(vmId))
            {
                return new StatusCodeResult(StatusCodes.Status501NotImplemented);
            }

            var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
            var azure = await Azure.Authenticate(credentials).WithDefaultSubscriptionAsync();

            var vm = await azure.VirtualMachines.GetByIdAsync(vmId);

            if (vm == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(vm.PowerState.ToString());
        }
    }
}
