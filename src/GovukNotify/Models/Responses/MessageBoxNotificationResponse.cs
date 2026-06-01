using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GovukNotify.Models.Responses
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
