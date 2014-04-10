using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Operation;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UnifiedSearch.Nancy
{
    internal sealed class ArcGISGateway : PortalGateway
    {
        public ArcGISGateway(String serviceUrl, ISerializer serializer) 
            : base(serviceUrl, serializer) 
        { }

        public Task<QueryResponse<T>> DoQuery<T>(Query queryOptions) where T : IGeometry
        {        
            return Get<QueryResponse<T>, Query>(queryOptions);
        }

        public async Task<FindResponse> DoFind(Find findOptions)
        {      
            var response = await Get<FindResponse, Find>(findOptions);
            if (response == null || response.Results == null || !response.Results.Any()) return null;

            foreach (var result in response.Results.Where(r => r.Geometry != null))
            {
                result.Geometry = JsonConvert.DeserializeObject(
                    JsonConvert.SerializeObject(result.Geometry, JsonDotNetSerializer.Settings), 
                    TypeMap[result.GeometryType]());

                (result.Geometry as IGeometry).SpatialReference = findOptions.OutputSpatialReference;
            }
            return response;
        }

        internal readonly static Dictionary<String, Func<Type>> TypeMap = new Dictionary<String, Func<Type>>
        {
            { GeometryTypes.Point, () => typeof(Point) },
            { GeometryTypes.MultiPoint, () => typeof(MultiPoint) },
            { GeometryTypes.Envelope, () => typeof(Extent) },
            { GeometryTypes.Polygon, () => typeof(Polygon) },
            { GeometryTypes.Polyline, () => typeof(Polyline) }
        };
    }
    
    public static class FindResultExtensions
    {
        public static List<Feature<IGeometry>> ToFeatures(this FindResult[] findResults)
        {
            var result = new List<Feature<IGeometry>>();
            foreach (var findResult in findResults)
            {
                result.Add(new Feature<IGeometry> { Attributes = findResult.Attributes, Geometry = (IGeometry) findResult.Geometry });
            }
            return result;
        }
    }

    public class JsonDotNetSerializer : ISerializer
    {
        internal static readonly Newtonsoft.Json.JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };
       
        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            var stringValue = JsonConvert.SerializeObject(objectToConvert, Settings);

            var jobject = Newtonsoft.Json.Linq.JObject.Parse(stringValue);
            var dict = new Dictionary<String, String>();
            foreach (var item in jobject)
            {
                dict.Add(item.Key, item.Value.ToString());
            }
            return dict;
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
        {
            return JsonConvert.DeserializeObject<T>(dataToConvert, Settings);
        }
    }

}
