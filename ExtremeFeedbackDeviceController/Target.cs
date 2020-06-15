using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ExtremeFeedbackDeviceController
{
    public class Target
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("state")]
        public State Status { get; set; }
        [JsonProperty("updatedTime")]
        public DateTime UpdatedDate { get; set; }
        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; }
        [JsonProperty("fileName")] 
        public string FileName { get; set; }
    }
}
