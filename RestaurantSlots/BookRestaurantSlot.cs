using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RestaurantSlots
{
    public static class BookRestaurantSlot
    {
        [FunctionName("BookRestaurantSlot")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            BookingRequest data = JsonConvert.DeserializeObject<BookingRequest>(requestBody);

            bool success = await CosmosDbConnector.Instance.UpdateBookingAsync(data);

            return new OkObjectResult(success);
        }
    }
}
