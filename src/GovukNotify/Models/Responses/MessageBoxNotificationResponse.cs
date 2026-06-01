using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GovukNotify.Models.Responses
{
    public class MessageBoxNotificationResponse
    {
        public string body;
        [JsonProperty("from_number")]
        public string fromNumber;
    }
}
