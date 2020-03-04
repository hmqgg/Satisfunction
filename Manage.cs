using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace Satisfunction
{
    public static class Manage
    {
        /// <exception cref="System.Security.SecurityException">Ignore.</exception>
        [FunctionName("Manage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed management request.");

            var action = req.Query["action"];

            if (string.IsNullOrEmpty(action) || !action.Equals("start") && !action.Equals("stop"))
            {
                return new BadRequestResult();
            }

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

            if (action.Equals("stop") && vm.PowerState == PowerState.Running)
            {
                log.LogInformation("C# HTTP trigger function processed stop request.");

                try
                {
                    await azure.VirtualMachines.Inner.BeginDeallocateWithHttpMessagesAsync(vm.ResourceGroupName, vm.Name);
                    return new AcceptedResult();
                }
                catch (RestException re)
                {
                    log.LogError(re, "Stop and deallocate failed.");
                }

                return new InternalServerErrorResult();
            }

            if (action.Equals("start") && vm.PowerState == PowerState.Deallocated || vm.PowerState == PowerState.Stopped)
            {
                log.LogInformation("C# HTTP trigger function processed start request.");
                
                try
                {
                    await azure.VirtualMachines.Inner.BeginStartWithHttpMessagesAsync(vm.ResourceGroupName, vm.Name);
                    return new AcceptedResult();
                }
                catch (RestException re)
                {
                    log.LogError(re, "Start failed.");
                }

                return new InternalServerErrorResult();
            }

            return new Microsoft.AspNetCore.Mvc.ConflictResult();
        }
    }
}
