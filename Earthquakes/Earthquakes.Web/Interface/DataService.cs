using ServiceStack.ServiceInterface;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Earthquakes.Web.Model;
using ArcGIS.ServiceModel.Common;
using Earthquake;
using ArcGIS.ServiceModel;
using Earthquake.Logic;
using ArcGIS.ServiceModel.GeoJson;
using ArcGIS.ServiceModel.Operation;

namespace Earthquakes.Web.Interface
{
    [Restrict(VisibleInternalOnly = true)]
    [DefaultView("Earthquakes")]
    public class DataService : Service
    {
        const String dataUrl = "http://geonet.org.nz/quakes/services/felt.json";
        ArcGISOnlineGateway _gateway = new ArcGISOnlineGateway();

        public object Get(DataGet request)
        {
            var cacheKey = UrnId.Create<EarthquakeData>("quakes");

            return RequestContext.ToOptimizedResultUsingCache(Cache, cacheKey, TimeSpan.FromMinutes(1), () =>
            {
                var data = dataUrl.GetJsonFromUrl();

                return JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonPoint>>(data).ToFeatures<GeoJsonPoint>();
            });
        }

        public object Get(RingsGet request)
        {
            var cacheKey = UrnId.Create<List<Feature<Polygon>>>(request.Id);

            return RequestContext.ToOptimizedResultUsingCache(Cache, cacheKey, TimeSpan.FromMinutes(1), () =>
            {
                var result = _gateway.Project<Point>(new List<Feature<Point>> { new Feature<Point> { Geometry = new Point { X = request.X, Y = request.Y } } }, new SpatialReference { Wkid = 2193 }).Result;

                var func = new Isoseismals();
                // this would be more realistic with better input params
                var rings = func.Generate(result.First().Geometry, request.Depth, request.Magnitude);
                rings.First().Geometry.SpatialReference = new SpatialReference { Wkid = 2193 };

                var result2 = _gateway.Project<Polygon>(rings, SpatialReference.WGS84).Result;

                return result2;
            });
        }
    }

    public class DataGet { }

    [Route("/eq")]
    public class RingsGet
    {
        public String Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Depth { get; set; }
        public double Magnitude { get; set; }
    }
}