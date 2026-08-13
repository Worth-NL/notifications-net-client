using Newtonsoft.Json;


namespace Notify.Models.Responses
{
    public class MessageBoxNotificationResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("content")]
        public object Content { get; set; } // adjust if API returns more fields
    }
}
