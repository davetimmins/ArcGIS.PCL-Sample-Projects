using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel.Operation;
using ArcGIS.ServiceModel;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Earthquake.Logic;

namespace Earthquake
{
    public class testes
    {
        [Fact]
        public void DoIt()
        {
            var gateway = new ArcGISGateway();
            var op = new ProjectGeometry<Point>(
                "/Utilities/Geometry/GeometryServer".AsEndpoint(),
                new List<Feature<Point>> { new Feature<Point> { Geometry = new Point { X = 177.32756, Y = -37.32321 } } },
                new SpatialReference { Wkid = 2193 });
                           
            var result = gateway.Project<Point>(op);

            var func = new Isoseismals();
            var rings = func.Generate(result.Geometries.First(), 154.76562, 4.5990443);

            Assert.True(true);
        }
    }

    internal class ArcGISGateway : PortalGateway
    {
        public ArcGISGateway()
            : base(@"http://services.arcgisonline.co.nz/ArcGIS/", new ServiceStackSerializer())
        { }

        public GeometryOperationResponse<T> Project<T>(ProjectGeometry<T> endpoint) where T : IGeometry
        {
            return Post<GeometryOperationResponse<T>, ProjectGeometry<T>>(endpoint).Result;
        }

        public GeometryOperationResponse<T> Simplify<T>(SimplifyGeometry<T> endpoint) where T : IGeometry
        {
            return Post<GeometryOperationResponse<T>, SimplifyGeometry<T>>(endpoint).Result;
        }
    }

    public class ServiceStackSerializer : ISerializer
    {
        public ServiceStackSerializer()
        {
            JsConfig.EmitCamelCaseNames = true;
            JsConfig.IncludeTypeInfo = false;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            JsConfig.IncludeNullValues = false;
        }

        public Dictionary<String, String> AsDictionary<T>(T objectToConvert) where T : CommonParameters
        {
            return TypeSerializer.ToStringDictionary<T>(objectToConvert);
        }

        public T AsPortalResponse<T>(String dataToConvert) where T : IPortalResponse
        {
            return JsonSerializer.DeserializeFromString<T>(dataToConvert);
        }
    }


}
