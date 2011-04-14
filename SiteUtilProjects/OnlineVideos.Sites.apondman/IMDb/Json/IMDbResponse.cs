﻿using System;

namespace OnlineVideos.Sites.Pondman.IMDb.Json {
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    
    public class IMDbResponse {

        [JsonProperty("data")]
        public JObject Data { get; set; }

        [JsonProperty("exp")]
        public int Expires { get; set; }
    }
}