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
            var gateway = new ArcGISOnlineGateway();

            var result = gateway.Project<Point>(new List<Feature<Point>> { new Feature<Point> { Geometry = new Point { X = 177.32756, Y = -37.32321 } } },
                new SpatialReference { Wkid = 2193 }).Result;

            var func = new Isoseismals();
            var rings = func.Generate(result.First().Geometry, 154.76562, 4.5990443);

            Assert.True(true);
        }
    }  
}
