using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CleverFunctionApp.Functions
{
    public class Ping
    {
        [Function("ping")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] string? o)
        {
            return new OkObjectResult("im alive");
        }
    }
}
