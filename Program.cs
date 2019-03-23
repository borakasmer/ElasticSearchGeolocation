using System;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace ElasticService
{

    class Program
    {

        private static readonly ConnectionSettings connSettings =
        new ConnectionSettings(new Uri("http://localhost:9200/"))
                        .DefaultIndex("customerlocation")
                        //Optionally override the default index for specific types
                        .DefaultMappingFor<CustomerModel>(m => m
                        .IndexName("customerlocation")
    );

        private static readonly ElasticClient elasticClient = new ElasticClient(connSettings);

        // private static IDbConnection _db = new SqlConnection("");

        static void Main(string[] args)
        {
            //Indexing();
            Search();
        }

        public static void Search()
        {            
            var geoResult = elasticClient.Search<CustomerModel>(s => s.From(0).Size(10000).Query(query => query.Bool(b => b.Filter(filter => filter
               .GeoDistance(geo => geo
                   .Field(f => f.Location)
                   .Distance("250km").Location(41, 28)
                   .DistanceType(GeoDistanceType.Plane)
               ))
               )));
            foreach (var customer in geoResult.Documents)
            {
                Console.WriteLine(customer.CustomerName + ":" + customer.LocationName + " = " + GetDistanceFromLatLonInKm(41, 28, customer.Location.Latitude, customer.Location.Longitude).ToString() + "km");
            }

            Console.ReadLine();
        }

        public static void Indexing()
        {
            using (LocationContext dbContext = new LocationContext())
            {
                var customerLocationList = dbContext.CustomerLocations.Where(s => !s.IsDeleted)
                .Include(s => s.Customer)
                .Select(s => new CustomerModel
                {
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer.Name,
                    LocationId = s.Id,
                    LocationName = s.Name,
                    Location = new GeoLocation(Convert.ToDouble(s.Latitude), Convert.ToDouble(s.Longitude))
                }).ToList();


                var defaultIndex = "customerlocation";
                var client = new ElasticClient();

                if (client.IndexExists(defaultIndex).Exists)
                {
                    client.DeleteIndex(defaultIndex);
                }

                if (!elasticClient.IndexExists("location_alias").Exists)
                {
                    client.CreateIndex(defaultIndex, c => c
                        .Mappings(m => m
                            .Map<CustomerModel>(mm => mm
                                .AutoMap()
                            )
                        ).Aliases(a => a.Alias("location_alias"))
                    );
                }

                // Insert Data Classic
                // for (int i = 0; i < customerLocationList.Count; i++)
                //     {
                //         var item = customerLocationList[i];
                //         elasticClient.Index<CustomerModel>(item, idx => idx.Index("customerlocation").Id(item.LocationId));
                //     }

                // Bulk Insert
                var bulkIndexer = new BulkDescriptor();

                foreach (var document in customerLocationList)
                {
                    bulkIndexer.Index<CustomerModel>(i => i
                        .Document(document)
                        .Id(document.LocationId)
                        .Index("customerlocation"));
                }

                elasticClient.Bulk(bulkIndexer);
            }
        }

        public static double GetDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = deg2rad(lat2 - lat1);  // deg2rad below
            var dLon = deg2rad(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        public static double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}
