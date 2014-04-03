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
        ProjectGateway _gatewayProject = new ProjectGateway();

        public object Get(GeoJson request)
        {
            var uri = HttpUtility.UrlDecode(Request.QueryString.ToString());

            var gateway = new ProxyGateway(uri);
            var result = gateway.Query<Polyline>(new Query(uri.AsEndpoint()));

            var features = result.Features.ToList();
            features.First().Geometry.SpatialReference = result.SpatialReference;
            if (result.SpatialReference.Wkid != SpatialReference.WGS84.Wkid)
                features = _gatewayProject.Project<Polyline>(features, SpatialReference.WGS84);
                
            return features.ToFeatureCollection();
        }
    }

    [Route("/geojson")]
    public class GeoJson
    { }

    public class ProjectGateway : PortalGateway
    {
        public ProjectGateway()
            : base(@"http://services.arcgisonline.co.nz/ArcGIS/", new ServiceStackSerializer())
        { }

        public List<Feature<T>> Project<T>(List<Feature<T>> features, SpatialReference outputSpatialReference) where T : IGeometry
        {
            var op = new ProjectGeometry<T>("/Utilities/Geometry/GeometryServer".AsEndpoint(), features, outputSpatialReference);
            var projected = Post<GeometryOperationResponse<T>, ProjectGeometry<T>>(op).Result;
            for (int i = 0; i < projected.Geometries.Count; i++)
                features[i].Geometry = projected.Geometries[i];

            return features;
        }
    }

    public class ProxyGateway : PortalGateway
    {
        public ProxyGateway(String rootUrl)
            : base(rootUrl, new ServiceStackSerializer())
        { }

        public QueryResponse<T> Query<T>(Query queryOptions) where T : IGeometry
        {
            return Get<QueryResponse<T>, Query>(queryOptions).Result;
        }
    }
}