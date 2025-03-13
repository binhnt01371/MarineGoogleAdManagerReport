using Microsoft.AspNetCore.Hosting.Server;

namespace MarineGoogleAdManagerReport.Models
{
    public class ImpressionModel
    {
        public string ItemId { get; set; }
        public DateTime Date { get; set; }
        public string ItemName { get; set; }
        public int AdServerImpressions { get; set; }
        public int AdServerClicks { get; set; }
    }
}
