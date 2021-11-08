using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

namespace RestaurantSlots
{
    public class CosmosDbConnector
    {
        private static readonly string EndpointUri = Environment.GetEnvironmentVariable("CosmosDbEndpoint", EnvironmentVariableTarget.Process);

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = Environment.GetEnvironmentVariable("CosmosPrimaryKey", EnvironmentVariableTarget.Process);

        // The Cosmos client instance
        private CosmosClient _cosmosClient;

        // The database we will create
        private Database _database;

        // The container we will create.
        private Container _container;

        private static CosmosDbConnector _instance;
        public static CosmosDbConnector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CosmosDbConnector();
                }

                return _instance;
            }
        }

        private CosmosDbConnector()
        {
            _cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "RestaurantSlotsAzureFunction" });
            _database = _cosmosClient.GetDatabase("Restaurants");
            _container = _database.GetContainer("Bookings");
        }

        public string GetEndpoint()
        {
            return EndpointUri;
        }

        public async Task<IList<Bookings>> GetBookingsForDateAsync(string date)
        {
            using (FeedIterator<Bookings> setIterator = _container.GetItemLinqQueryable<Bookings>()
                      .Where(b => b.Date == date)
                      .ToFeedIterator<Bookings>())
            {
                IList<Bookings> bookings = new List<Bookings>();
                //Asynchronous query execution
                while (setIterator.HasMoreResults)
                {
                    foreach (var item in await setIterator.ReadNextAsync())
                    {
                        bookings.Add(item);
                    }
                }

                return bookings;
            }
        }

        public async Task<bool> UpdateBookingAsync(BookingRequest bookingRequest)
        {
            ItemResponse<Bookings> bookings = await _container.ReadItemAsync<Bookings>(bookingRequest.Id, new PartitionKey(bookingRequest.Date));
            var bookingsBody = bookings.Resource;

            if (bookingsBody.SpacesRemaining >= 1)
            {
                bookingsBody.SpacesRemaining--;
                bookingsBody.ExistingBookings.Add(new Booking()
                {
                    Name = bookingRequest.Name,
                    Seats = bookingRequest.Seats
                });
            }
            else
            {
                return false;
            }

            // replace the item with the updated content
            bookings = await _container.ReplaceItemAsync<Bookings>(bookingsBody, bookingsBody.Id, new PartitionKey(bookingsBody.Date));
            return true;
        }
    }
}
