using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Operation;
using ArcGIS.ServiceModel;

namespace ArcGISLocationMapper.Web.Controllers
{
    public class HomeController : Controller
    {
        readonly GeocodeGateway _gateway = new GeocodeGateway();
        //
        // GET: /Home/

        public async Task<ActionResult> Index()
        {
            var text = Request.QueryString["text"];
            var x = Request.QueryString["x"];
            var y = Request.QueryString["y"];
            
            if (String.IsNullOrWhiteSpace(text) && (x == null || y == null)) throw new InvalidOperationException();

            if (String.IsNullOrWhiteSpace(text))
            {
                var wkid = (Request.QueryString["wkid"] == null ? 0 : int.Parse(Request.QueryString["wkid"]));
                var reverseGeocode = new ReverseGeocode("/World/GeocodeServer/".AsEndpoint())
                {
                    Location = new Point
                    {
                        X = double.Parse(x),
                        Y = double.Parse(y),
                        SpatialReference = new SpatialReference { Wkid = (wkid != 0) ? wkid : SpatialReference.WGS84.LatestWkid }
                    }
                };
                var resultReverseGeocode = await _gateway.ReverseGeocode(reverseGeocode);

                var feature = new Feature<Point>
                {
                    Attributes = new Dictionary<String, object> { { "Address", resultReverseGeocode.Address.AddressText } },
                    Geometry = resultReverseGeocode.Location
                };
                return View(feature);
            }

            var geocode = new SingleInputGeocode("/World/GeocodeServer/".AsEndpoint())
            {
                Text = text
            };
            var resultGeocode = await _gateway.Geocode(geocode);
            var result = resultGeocode.Results.First().Feature;
            return View(result);
        }

    }

    public class GeocodeGateway : PortalGateway
    {
        public GeocodeGateway()
            : base("http://geocode.arcgis.com/arcgis", new JsonDotNetSerializer())
        { }

        public async Task<ReverseGeocodeResponse> ReverseGeocode(ReverseGeocode reverseGeocode)
        {
            return await Get<ReverseGeocodeResponse, ReverseGeocode>(reverseGeocode);
        }

        public async Task<SingleInputGeocodeResponse> Geocode(SingleInputGeocode geocode)
        {
            return await Get<SingleInputGeocodeResponse, SingleInputGeocode>(geocode);
        }
    }

    public class JsonDotNetSerializer : ISerializer
    {
        readonly Newtonsoft.Json.JsonSerializerSettings _settings;


        public JsonDotNetSerializer()
        {
            _settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
        }

        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            var stringValue = Newtonsoft.Json.JsonConvert.SerializeObject(objectToConvert, _settings);

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
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dataToConvert, _settings);
        }
    }

}
