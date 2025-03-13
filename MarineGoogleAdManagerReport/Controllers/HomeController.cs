using Google.Ads.AdManager.V1;
using Google.Api.Ads.AdManager.Lib;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Grpc.Auth;
using MarineGoogleAdManagerReport.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;
namespace MarineGoogleAdManagerReport.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static readonly string[] Scopes = { "https://www.googleapis.com/auth/dfp" };
        private static readonly string ApplicationName = "Google Ad Manager API .NET Example";
        private static HttpClient _httpClient = new HttpClient();
        private static string _accessToken;
        private List<ImpressionModel> _impressions;
        private List<DeviceModal> _devices;
        private List<DeviceModal> _browsers;
        private List<ContinentModel> _continents;
        private List<CountryModel> _countries;
        private static List<string> _countryCategory;
        private static List<string> _deviceCategory;
        private static List<string> _browserCategory;
        private static List<string> _continentCategory;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _impressions = new List<ImpressionModel>();
            _devices = new List<DeviceModal>();
            _countries = new List<CountryModel>();
            _continents = new List<ContinentModel>();
            _browsers = new List<DeviceModal>();
            _countryCategory = new List<string>() {
                "United States",
                "China",
                "United Kingdom",
                "India",
                "Germany",
                "Canada"
            };
            _deviceCategory = new List<string>() {
                "Desktop",
                "Set-top box",
                "Smartphone",
                "Tablet"
            };
            _browserCategory = new List<string>() {
                "Firefox",
                "Google Chrome",
                "In-app browser",
                "Microsoft Edge",
                "Other",
                "Safari"
            };
            _continentCategory = new List<string>() {
                "Africa",
                "Americas",
                "Asia",
                "Europe",
                "Oceania",
                "Unknown"
            };
        }

        public async Task<IActionResult> Index()
        {
            //// Set the path to the service account key file
            //string webRootPath = _webHostEnvironment.WebRootPath;
            //string contentRootPath = _webHostEnvironment.ContentRootPath;
            //string keyFilePath = Path.Combine(contentRootPath, "reports-project-449915-6113fb700cd2.json");
            ////C:\Users\binht\source\repos\MarineGoogleAdManagerReport\MarineGoogleAdManagerReport\reports-project-449915-6113fb700cd2.json
            //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyFilePath);

            //// Authenticate using the service account key file
            //GoogleCredential credential = GoogleCredential.FromFile(keyFilePath);

            //// Create the Ad Manager client
            //NetworkServiceClient networkServiceClient = new NetworkServiceClientBuilder
            //{
            //    ChannelCredentials = credential.ToChannelCredentials()
            //}.Build();

            //// Make the request to list all networks
            //ListNetworksRequest request = new ListNetworksRequest();
            //var response = networkServiceClient.ListNetworks(request);

            //// Output the response
            //foreach (var network in response.Networks)
            //{
            //    Console.WriteLine($"Network code: {network.NetworkCode}, Display name: {network.DisplayName}");
            //}
            //await AuthenticateAndCreateServiceAsync();
            //await GetAdUnitsAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<JsonResult> GetChartData()
        {
            try
            {
                await AuthenticateAndCreateServiceAsync();
                await GetAdUnitsAsync();
            }
            catch (Exception)
            {
                await MockData();
            }
            var impressions = _impressions
                .Select(s=> new ChartData { Label = s.Date.ToString("dd/MM/yyyy"), Value = s.AdServerImpressions});

            var countriesTotalImpressions = _countries.Sum(s => s.AdServerImpressions);
            var countries = _countries
                .Select(s => new ChartData 
                { 
                    Label = s.Country, 
                    Value = Math.Round((s.AdServerImpressions / (double)countriesTotalImpressions) * 100, 2) 
                });

            var devicesTotalImpressions = _devices.Sum(s => s.AdServerImpressions);
            var devices = _devices
                .Select(s => new ChartData 
                { 
                    Label = s.Name, 
                    Value = Math.Round((s.AdServerImpressions / (double)devicesTotalImpressions) * 100, 2)
                });

            var browsersTotalImpressions = _browsers.Sum(s => s.AdServerImpressions);
            var browsers = _browsers
                .Select(s => new ChartData 
                { 
                    Label = s.Name, 
                    Value = Math.Round((s.AdServerImpressions / (double)browsersTotalImpressions) * 100, 2)
                });

            var continentTotalImpressions = _browsers.Sum(s => s.AdServerImpressions);
            var continents = _continents
                .Select(s => new ChartData
                {
                    Label = s.Continent,
                    Value = Math.Round((s.AdServerImpressions / (double)continentTotalImpressions) * 100, 2)
                });

            return Json(new { impressions, countries, devices, browsers, continents });
        }

        public async Task<IActionResult> Details(string category)
        {
            await MockData();
            ViewBag.Category = category.ToUpper();

            var data = new List<ChartData>();

            switch (category.ToLower())
            {
                case "impressions":
                    data = _impressions.Select(s => new ChartData 
                    { 
                        Label = s.Date.ToString("dd/MM/yyyy"), 
                        Value = s.AdServerImpressions 
                    }).ToList();
                    break;

                case "countries":
                    var countriesTotalImpressions = _countries.Sum(s => s.AdServerImpressions);
                    data = _countries
                    .Select(s => new ChartData
                    {
                        Label = s.Country,
                        Value = Math.Round((s.AdServerImpressions / (double)countriesTotalImpressions) * 100, 2)
                    }).ToList();
                    break;

                case "devices":
                    var devicesTotalImpressions = _devices.Sum(s => s.AdServerImpressions);
                    data = _devices
                        .Select(s => new ChartData
                        {
                            Label = s.Name,
                            Value = Math.Round((s.AdServerImpressions / (double)devicesTotalImpressions) * 100, 2)
                        }).ToList();
                    break;

                case "browsers":
                    var browsersTotalImpressions = _browsers.Sum(s => s.AdServerImpressions);
                    data = _browsers
                        .Select(s => new ChartData
                        {
                            Label = s.Name,
                            Value = Math.Round((s.AdServerImpressions / (double)browsersTotalImpressions) * 100, 2)
                        }).ToList();
                    break;

                case "continents":
                    var continentTotalImpressions = _browsers.Sum(s => s.AdServerImpressions);
                    data = _continents
                        .Select(s => new ChartData
                        {
                            Label = s.Continent,
                            Value = Math.Round((s.AdServerImpressions / (double)continentTotalImpressions) * 100, 2)
                        }).ToList();
                    break;
            }

            return View(data);
        }

        private static async Task AuthenticateAndCreateServiceAsync()
        {
            UserCredential credential;

            // Load client secrets (the path to your credentials JSON file)
            using (var stream = new FileStream("reports-project-449915-6113fb700cd2.json", FileMode.Open, FileAccess.Read))
            {
                var test = GoogleClientSecrets.Load(stream).Secrets;
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore("token.json", true)
                );
            }

            // Get the OAuth2 token
            _accessToken = credential.Token.AccessToken;
        }

        //Get the Ad Units from Google Ad Manager
        private static async Task GetAdUnitsAsync()
        {
            var url = "https://dfp.googleapis.com/v202202/InventoryService/getAdUnits"; // This is an example endpoint, adjust accordingly

            // Add Authorization header
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);

            // Make the HTTP GET request
            var response = await _httpClient.GetStringAsync(url);

            // Deserialize JSON response
            var adUnits = JsonConvert.DeserializeObject(response);

            Console.WriteLine("Ad Units Retrieved:");
            Console.WriteLine(adUnits);
        }

        private async Task MockData()
        {
            Random r = new Random();
            var date = DateTime.Now;
            for (int i = 0; i < 30; i++)
            {
                _impressions.Add(new ImpressionModel()
                {
                    ItemId = i.ToString(),
                    ItemName = $"Items {i.ToString()}",
                    Date = date.AddDays(i*-1),
                    AdServerClicks = r.Next(0, 100),
                    AdServerImpressions = r.Next(100, 500)
                });
            }

            foreach (var item in _deviceCategory)
            {
                _devices.Add(new DeviceModal()
                {
                    Name = item,
                    Category = $"3000{r.Next(0, 9)}",
                    AdServerImpressions = r.Next(100, 5000)
                });
            }

            foreach (var item in _browserCategory)
            {
                _browsers.Add(new DeviceModal()
                {
                    Name = item,
                    Category = $"{r.Next(0, 9)}",
                    AdServerImpressions = r.Next(100, 8000)
                });
            }

            foreach (var item in _continentCategory)
            {
                _continents.Add(new ContinentModel()
                {
                    Continent = item,
                    ContinentId = $"5000{r.Next(0, 9)}",
                    ItemId = $"9000{r.Next(0, 9)}",
                    ItemName = $"Items {r.Next(0, 100)}",
                    AdServerImpressions = r.Next(100, 10000)
                });
            }

            int range = 1000;
            foreach (var item in _countryCategory) {
                _countries.Add(new CountryModel() {
                    Country = item,
                    CountryId = $"2000{r.Next(0, 9)}",
                    ItemId = $"9000{r.Next(0, 9)}",
                    ItemName = $"Items {r.Next(0, 100)}",
                    AdServerImpressions = r.Next(100, 10000),
                    Perc = r.NextDouble() * range
                });
            }
        }
    }
}
