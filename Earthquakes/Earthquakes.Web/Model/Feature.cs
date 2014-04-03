using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Earthquakes.Web.Model
{
    public class EarthquakeData
    {
        public string type { get; set; }
        public Feature[] features { get; set; }
        public Crs crs { get; set; }
    }

    public class Crs
    {
        public string type { get; set; }
        public Properties properties { get; set; }
    }

    public class Properties
    {
        public string code { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public string id { get; set; }
        public Geometry geometry { get; set; }
        public string geometry_name { get; set; }
        public FeatureProperties properties { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public float[] coordinates { get; set; }
    }

    public class FeatureProperties
    {
        public string publicid { get; set; }
        public string origintime { get; set; }
        public float depth { get; set; }
        public float magnitude { get; set; }
        public string status { get; set; }
        public string agency { get; set; }
    }

}