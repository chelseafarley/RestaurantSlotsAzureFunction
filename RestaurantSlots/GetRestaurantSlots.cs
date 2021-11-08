using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RestaurantSlots
{
    public static class GetRestaurantSlots
    {
        [FunctionName("GetRestaurantSlots")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string date = req.Query["date"];
            if (string.IsNullOrEmpty(date))
            {
                throw new Exception("Date not found");
            }

            log.LogInformation("getting env");
            log.LogInformation(CosmosDbConnector.Instance.GetEndpoint());
            log.LogInformation("got env");

            IList<Bookings> bookings = await CosmosDbConnector.Instance.GetBookingsForDateAsync(date);
            return new OkObjectResult(bookings);
        }
    }
}
