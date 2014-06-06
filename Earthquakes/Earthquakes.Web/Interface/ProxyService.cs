using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Text;
using Earthquake;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Operation;

namespace Earthquakes.Web.Interface
{
    public class ProxyService : Service
    {
        ArcGISOnlineGateway _gateway = new ArcGISOnlineGateway();

        public object Get(GeoJson request)
        {
            var uri = HttpUtility.UrlDecode(Request.QueryString.ToString());

            var gateway = new PortalGateway(uri);
            var result = gateway.Query<Polyline>(new Query(uri.AsEndpoint())).Result;

            var features = result.Features.ToList();
            features.First().Geometry.SpatialReference = result.SpatialReference;
            if (result.SpatialReference.Wkid != SpatialReference.WGS84.Wkid)
                features = _gateway.Project<Polyline>(features, SpatialReference.WGS84).Result;
                
            return features.ToFeatureCollection();
        }
    }

    [Route("/geojson")]
    public class GeoJson
    { }
}