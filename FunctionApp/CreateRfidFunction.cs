using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CleverFunctionApp.Functions
{
    public class CreateRfidFunction
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger<CreateRfidFunction> _logger;

        public CreateRfidFunction(TableServiceClient tableServiceClient, ILogger<CreateRfidFunction> logger)
        {
            _tableServiceClient = tableServiceClient ?? throw new ArgumentNullException(nameof(tableServiceClient)); ;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
        }

        [Function("Create")]
        public async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/create")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string? rfid;
            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogError("RequestBody cannot be null or empty.");
                return new BadRequestObjectResult("RequestBody cannot be null or empty.");
            }
            else
            {
                rfid = JsonSerializer.Deserialize<string>(requestBody);
            }

            if (string.IsNullOrEmpty(rfid))
            {
                _logger.LogError("RFID cannot be null or empty.");
                return new BadRequestObjectResult("RFID cannot be null or empty.");
            }

            TableClient tableClient = _tableServiceClient.GetTableClient("Rfids");
            await tableClient.CreateIfNotExistsAsync();

            var entity = new TableEntity("PartitionKey", rfid) { { "Rfid", rfid } };

            var existingEntity = await tableClient.GetEntityIfExistsAsync<TableEntity>("PartitionKey", rfid);

            if (existingEntity == null || !existingEntity.HasValue)
            {
                await tableClient.AddEntityAsync(entity);
                return new OkResult();
            }

            return new ConflictResult();
        }
    }
}
