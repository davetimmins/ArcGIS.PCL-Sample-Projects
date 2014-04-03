using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Text;
using ArcGIS.ServiceModel.GeoJson;
using ServiceStack.Common;

namespace Converter.Web.Interface
{
    public class ConverterService : Service
    {
        static ISerializer _serializer = new ServiceStackSerializer();
        static Dictionary<String, Func<String, FeatureCollection<IGeoJsonGeometry>>> _funcMap = new Dictionary<String, Func<String, FeatureCollection<IGeoJsonGeometry>>>
        {
            { GeometryTypes.Point, (uri) => new ProxyGateway(uri, _serializer).GetGeoJson<Point>(uri) },
            { GeometryTypes.MultiPoint, (uri) => new ProxyGateway(uri, _serializer).GetGeoJson<MultiPoint>(uri) },
            { GeometryTypes.Envelope, (uri) => new ProxyGateway(uri, _serializer).GetGeoJson<Extent>(uri) },
            { GeometryTypes.Polygon, (uri) => new ProxyGateway(uri, _serializer).GetGeoJson<Polygon>(uri) },
            { GeometryTypes.Polyline, (uri) => new ProxyGateway(uri, _serializer).GetGeoJson<Polyline>(uri) }
        };

        public object Get(GeoJson request)
        {
            var uri = HttpUtility.UrlDecode(request.Uri);
            var cacheKey = UrnId.Create<GeoJson>(uri.Substring(uri.IndexOf(':') + 3));

            return RequestContext.ToOptimizedResultUsingCache(Cache, cacheKey, TimeSpan.FromMinutes(1), () =>
            {
                int layerId;
                if (!int.TryParse(uri.Split('/').Last(), out layerId)) throw new HttpException("You must enter a valid layer url.");

                var layer = new ProxyGateway(uri, _serializer).GetAnything(uri.AsEndpoint());
                if (layer == null || !layer.ContainsKey("geometryType")) throw new HttpException("You must enter a valid layer url.");
                return _funcMap[layer["geometryType"]](uri);
            });
        }

        static Dictionary<String, Func<String, List<Feature<IGeometry>>>> _funcyMap = new Dictionary<String, Func<String, List<Feature<IGeometry>>>>
        {
            { "Point", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonPoint>>(data).ToFeatures<GeoJsonPoint>() },
            { "MultiPoint", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonLineString>>(data).ToFeatures<GeoJsonLineString>() },
            { "LineString", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonLineString>>(data).ToFeatures<GeoJsonLineString>() },
            { "MultiLineString", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonLineString>>(data).ToFeatures<GeoJsonLineString>() },
            { "Polygon", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonPolygon>>(data).ToFeatures<GeoJsonPolygon>() },
            { "MultiPolygon", (data) => JsonSerializer.DeserializeFromString<FeatureCollection<GeoJsonMultiPolygon>>(data).ToFeatures<GeoJsonMultiPolygon>() }
        };

        public object Get(ArcGISFeatures request)
        {
            var uri = HttpUtility.UrlDecode(request.Uri);
            var cacheKey = UrnId.Create<ArcGISFeatures>(uri.Substring(uri.IndexOf(':') + 3));

            return RequestContext.ToOptimizedResultUsingCache(Cache, cacheKey, TimeSpan.FromMinutes(1), () =>
            {
                var data = uri.GetJsonFromUrl();

                var geoJsonType = JsonSerializer.DeserializeFromString<TempFeatureCollection>(data);
                if (!String.Equals(geoJsonType.type, "FeatureCollection", StringComparison.InvariantCultureIgnoreCase)) return null;

                var comparison = geoJsonType.features.First().geometry.type;

                return _funcyMap[comparison](data);
            });
        }
    }

    [Route("/geojson")]
    public class GeoJson
    {
        public String Uri { get; set; }
    }

    [Route("/arcgis")]
    public class ArcGISFeatures
    {
        public String Uri { get; set; }
    }

    public class ProjectGateway : PortalGateway
    {
        public ProjectGateway(ISerializer serializer)
            : base(@"http://services.arcgisonline.co.nz/ArcGIS/", serializer)
        { }

        public List<Feature<T>> Project<T>(List<Feature<T>> features, SpatialReference outputSpatialReference) where T : IGeometry
        {
            var op = new ProjectGeometry<T>("/Utilities/Geometry/GeometryServer".AsEndpoint(), features, outputSpatialReference);
            var projected = Post<GeometryOperationResponse<T>, ProjectGeometry<T>>(op).Result;

            var result = features.UpdateGeometries<T>(projected.Geometries);
            if (result.First().Geometry.SpatialReference == null) result.First().Geometry.SpatialReference = outputSpatialReference;
            return result;
        }
    }

    public class AgsObject : JsonObject, IPortalResponse
    {
        [System.Runtime.Serialization.DataMember(Name = "error")]
        public ArcGISError Error { get; set; }
    }

    public class ProxyGateway : PortalGateway
    {
        public ProxyGateway(String rootUrl, ISerializer serializer)
            : base(rootUrl, serializer)
        { }

        public QueryResponse<T> Query<T>(Query queryOptions) where T : IGeometry
        {
            return Get<QueryResponse<T>, Query>(queryOptions).Result;
        }

        public AgsObject GetAnything(ArcGISServerEndpoint endpoint)
        {
            return Get<AgsObject>(endpoint).Result;
        }

        public FeatureCollection<IGeoJsonGeometry> GetGeoJson<T>(String uri) where T : IGeometry
        {
            var result = Query<T>(new Query(uri.AsEndpoint()));
            result.Features.First().Geometry.SpatialReference = result.SpatialReference;
            var features = result.Features.ToList();
            if (result.SpatialReference.Wkid != SpatialReference.WGS84.Wkid)
                features = new ProjectGateway(Serializer).Project<T>(features, SpatialReference.WGS84);
            return features.ToFeatureCollection();
        }
    }

    public class ServiceStackSerializer : ISerializer
    {
        public ServiceStackSerializer()
        {
            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;
            ServiceStack.Text.JsConfig.IncludeTypeInfo = false;
            ServiceStack.Text.JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            ServiceStack.Text.JsConfig.IncludeNullValues = false;
        }

        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            return ServiceStack.Text.TypeSerializer.ToStringDictionary<T>(objectToConvert);
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
        {
            return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(dataToConvert);
        }
    }


    public class TempFeatureCollection
    {
        public string type { get; set; }
        public TempFeature[] features { get; set; }
    }

    public class TempFeature
    {
        public TempGeometry geometry { get; set; }
    }

    public class TempGeometry
    {
        public string type { get; set; }
    }
}