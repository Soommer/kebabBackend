using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;

namespace kebabBackend.Services
{
    public class AzureMapsDistanceService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;


        public AzureMapsDistanceService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }
        public async Task<bool> IsWithinDeliveryRangeAsync(string destinationAddress)
        {
            string subKey = _configuration["AzureMaps:Key"];
            string orgin = _configuration["AzureMaps:Origin"];

            // destination to coordinates
            try
            {
                Console.WriteLine($"SubKey: {subKey}, Origin: {orgin}");
                string geoUrl = $"https://atlas.microsoft.com/search/address/json?api-version=1.0&subscription-key={subKey}&query={Uri.EscapeDataString(destinationAddress)}";
                Console.WriteLine(geoUrl);
                var geoResponse = await _httpClient.GetStringAsync(geoUrl);
                var geoJson = JObject.Parse(geoResponse);

                var position = geoJson["results"]?[0]?["position"];
                Console.WriteLine(position);

                if (position == null) return false;

                string lat = position["lat"]?.ToString().Replace(",", ".");
                string lon = position["lon"]?.ToString().Replace(",", ".");
                if (lat == null || lon == null) return false;

                // distance
                string routeUrl = $"https://atlas.microsoft.com/route/directions/json?api-version=1.0&subscription-key={subKey}&query={orgin}:{lat},{lon}";
                Console.WriteLine($"{routeUrl}");
                var routeResponse = await _httpClient.GetStringAsync(routeUrl);
                var routeJson = JObject.Parse(routeResponse);

                double? distance = routeJson["routes"]?[0]?["summary"]?["lengthInMeters"]?.ToObject<double>();
                if (distance == null) return false;

                return distance <= 30000;
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message );
                throw new Exception(ex.Message);
            }
        }
    }
}
