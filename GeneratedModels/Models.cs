using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TopGGPrune.GeneratedModels
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Stats
    {
        public int count { get; set; }
        public int maxUsers { get; set; }
        public int maxUserGrowth { get; set; }
        public int maxActivity { get; set; }
        public long _date { get; set; }
    }

    public class ScoreDistribution
    {
        public int _0 { get; set; }
        public int _20 { get; set; }
        public int _60 { get; set; }
        public int _80 { get; set; }
        public int _100 { get; set; }
        public int _40 { get; set; }
    }
    public class Result
    {
        public string id { get; set; }
        public string icon { get; set; }
        public string name { get; set; }
        public string shortDesc { get; set; }
        public string websiteURL { get; set; }
        public int memberCount { get; set; }
        public int monthlyPoints { get; set; }
        public int points { get; set; }
        public string joinDate { get; set; }
        public bool published { get; set; }
        public List<string> tags { get; set; }
        public bool blacklisted { get; set; }
        public object lockReason { get; set; }
        public string type { get; set; }
        public string platform { get; set; }
        public string lockAuthor { get; set; }
    }

    public class SearchResults
    {
        public Stats stats { get; set; }
        public JsonElement reviewStats { get; set; }
        public int count { get; set; }
        public List<Result> results { get; set; }
        public List<object> promoted { get; set; }
    }

}
