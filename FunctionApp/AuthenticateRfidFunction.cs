using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CleverFunctionApp.Functions
{
    public class AuthenticateRfidFunction
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger<AuthenticateRfidFunction> _logger;

        public AuthenticateRfidFunction(TableServiceClient tableServiceClient, ILogger<AuthenticateRfidFunction> logger)
        {
            _tableServiceClient = tableServiceClient ?? throw new ArgumentNullException(nameof(tableServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("Authenticate")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/authenticate")] HttpRequest req)
        {
            string? rfid = req.Query["rfid"];
            _logger.LogInformation($"Processing AuthenticateRfid request with rfid: {rfid}");

            if (string.IsNullOrEmpty(rfid))
            {
                return new BadRequestObjectResult("RFID query parameter is required.");
            }

            var tableClient = _tableServiceClient.GetTableClient("Rfids");
            await tableClient.CreateIfNotExistsAsync();

            var entity = await tableClient.GetEntityIfExistsAsync<TableEntity>("PartitionKey", rfid);

            return new OkObjectResult(entity.HasValue);
        }
    }
}
