using ArcGIS.ServiceModel.Common;
using Earthquake;
using System;
using System.Collections;
using System.Collections.Generic;
using ArcGIS.ServiceModel;
using System.Linq;
using ServiceStack.Text;
using ArcGIS.ServiceModel.Operation;

namespace Earthquake.Logic
{
    public class Isoseismals
    {
        int _indexer;
        int _maxIsoseismal;
        bool _isoSeismalDone;

        readonly int _isoseismalMax = 10;
        readonly int _isoseismalMin = 4;

        readonly double[] _a1 = new double[] { 4.87, 4.25, 4.00 };
        readonly double[] _a2 = new double[] { 1.25, 1.28, 1.63 };
        readonly double[] _a3 = new double[] { -3.77, -3.73, -4.03 };
        readonly double[] _a4 = new double[] { 0.0083, 0.017, 0.0044 };
        readonly double[] _a5 = new double[] { -0.68, 0.54, 0.0 };
        readonly double[] _ad = new double[] { 9.63, 10.05, 0.0 };
        readonly double[] _b1 = new double[] { 4.97, 4.20, 10.08 };
        readonly double[] _b2 = new double[] { 1.03, 1.11, 1.77 };
        readonly double[] _b3 = new double[] { -3.39, -3.27, -8.02 };
        readonly double[] _b4 = new double[] { 0.015, 0.021, 0.012 };
        readonly double[] _b5 = new double[] { -0.41, 0.31, 0.0 };
        readonly double[] _bd = new double[] { 5.74, 5.65, 0.0 };
        readonly double[] _a2r = new double[] { 0.048, 0.0, 0.0 };
        readonly double[] _a2v = new double[] { 0.21, 0.0, 0.0 };
        readonly double[] _a3s = new double[] { 0.16, 0.0, 0.0 };
        readonly double[] _a3v = new double[] { -1.47, 0.0, 0.0 };
        readonly double[] _b2r = new double[] { 0.058, 0.0, 0.0 };
        readonly double[] _b2v = new double[] { 0.18, 0.0, 0.0 };
        readonly double[] _b3s = new double[] { 0.23, 0.0, 0.0 };
        readonly double[] _b3v = new double[] { -1.35, 0.0, 0.0 };
        double[] _ra = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        double[] _rb = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        double[] _xrel = new double[] { -1.0, -0.95, -0.9, -0.8, -0.7, -0.6, -0.5, -0.4, -0.3, -0.2, -0.1, 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 0.95, 1.0, 0.95, 0.9, 0.8, 0.7, 0.6, 0.5, 0.4, 0.3, 0.2, 0.1, 0.0, -0.1, -0.2, -0.3, -0.4, -0.5, -0.6, -0.7, -0.8, -0.9, -0.95, -1.0 };

        double[][] _xr = RectangularArrays.ReturnRectangularDoubleArray(10, 45);
        double[][] _yr = RectangularArrays.ReturnRectangularDoubleArray(10, 45);

        ArrayList[] _isoseismalList = new ArrayList[10];

        public List<Feature<Polygon>> Generate(Point epicentreNZTM, double depthToFaultTop, double magnitude)
        {
            return Generate(epicentreNZTM, depthToFaultTop, magnitude, 'S');
        }

        public List<Feature<Polygon>> Generate(Point epicentreNZTM, double depthToFaultTop, double magnitude, char mechanism)
        {
            double ca, sa, d2r, x, y, xa, xb;
            double depthToFaultCentriodKm = 10;
            double strikeDegNorthOfE = 45;
            int isoseimalId;
            double crint = 0;
            double ss = 0;
            double rev = 0;
            double volc = 0;

            switch (mechanism)
            {
                case 'D': // deep earthquake, no specific mechanism allowed for
                    _indexer = 2;
                    if (depthToFaultTop < 50) depthToFaultTop = depthToFaultCentriodKm;
                    break;

                case 'X': // unknown mechanism, use main seismic region model, crustal
                    _indexer = 1;
                    crint = 1.0;
                    break;

                case 'S':
                    _indexer = 0;
                    ss = 1.0;
                    break;

                case 'R':
                    _indexer = 0;
                    rev = 1.0;
                    break;
                case 'N': // crustal normal
                    _indexer = 0;
                    break;
                case 'I': // Interface, reverse mechanism assumed
                    _indexer = 0;
                    crint = 1.0;
                    rev = 1.0;
                    break;
                case 'V': // central volcanic region, normal mechanism assumed
                    _indexer = 0;
                    volc = 1.0;
                    break;
                default:
                    // mech is a non-standard mechanism dummy. "X" will be assumed
                    _indexer = 1;
                    crint = 1.0;
                    break;
            }

            // Calculate major and minor axes of the isoseismals (D&R unit is km)
            _a2[_indexer] = _a2[_indexer] + _a2r[_indexer] * rev + _a2v[_indexer] * volc;
            _b2[_indexer] = _b2[_indexer] + _b2r[_indexer] * rev + _b2v[_indexer] * volc;
            _a3[_indexer] = _a3[_indexer] + _a3s[_indexer] * ss + _a3v[_indexer] * volc;
            _b3[_indexer] = _b3[_indexer] + _b3s[_indexer] * ss + _b3v[_indexer] * volc;

            for (isoseimalId = _isoseismalMin; isoseimalId <= _isoseismalMax; isoseimalId++)
            {
                xa = 2.0
                    *
                    (isoseimalId - _a1[_indexer]
                    - _a2[_indexer] * magnitude
                    - _a4[_indexer]
                    * depthToFaultCentriodKm - _a5[_indexer]
                    * crint) / _a3[_indexer];

                xa = Math.Pow(xa, 10) - _ad[_indexer]
                    * _ad[_indexer] - depthToFaultTop
                    * depthToFaultTop;

                if (xa > 0.0) _ra[isoseimalId - 1] = Math.Sqrt(xa);

                _ra[isoseimalId - 1] = _ra[isoseimalId - 1] * 1000; // convert km to m
                xb = 2.0 * (isoseimalId - _b1[_indexer] - _b2[_indexer] * magnitude - _b4[_indexer] * depthToFaultCentriodKm - _b5[_indexer] * crint) / _b3[_indexer];
                xb = Math.Pow(xb, 10) - _bd[_indexer] * _bd[_indexer] - depthToFaultTop * depthToFaultTop;

                if (xb > 0.0) _rb[isoseimalId - 1] = Math.Sqrt(xb);

                _rb[isoseimalId - 1] = _rb[isoseimalId - 1] * 1000; // convert km to m
            }

            // Generate isoseismal plot data
            d2r = Math.Atan(1.0) / 45.0; // ! degrees to radians
            ca = Math.Cos(strikeDegNorthOfE * d2r);
            sa = Math.Sin(strikeDegNorthOfE * d2r);


            for (int j = 0; j < 23; j++)
            {
                for (isoseimalId = _isoseismalMin; isoseimalId <= _isoseismalMax; isoseimalId++)
                {
                    x = _rb[isoseimalId - 1] * _xrel[j];
                    y = _ra[isoseimalId - 1] * Math.Sqrt(1 - Math.Pow(_xrel[j], 2));
                    _xr[isoseimalId - 1][j] = x * ca + y * sa + epicentreNZTM.X;
                    _yr[isoseimalId - 1][j] = -x * sa + y * ca + epicentreNZTM.Y;
                    CreateIsoseismal(isoseimalId, Math.Round(_xr[isoseimalId - 1][j]), Math.Round(_yr[isoseimalId - 1][j]));
                }
            }

            for (int j = 23; j < 45; j++)
            {
                for (isoseimalId = _isoseismalMin; isoseimalId <= _isoseismalMax; isoseimalId++)
                {
                    x = _rb[isoseimalId - 1] * _xrel[j];
                    y = -_ra[isoseimalId - 1] * Math.Sqrt(1 - Math.Pow(_xrel[j], 2));
                    _xr[isoseimalId - 1][j] = x * ca + y * sa + epicentreNZTM.X;
                    _yr[isoseimalId - 1][j] = -x * sa + y * ca + epicentreNZTM.Y;
                    CreateIsoseismal(isoseimalId, Math.Round(_xr[isoseimalId - 1][j]), Math.Round(_yr[isoseimalId - 1][j]));
                }
            }

            return MakePolygons();
        }

        void CreateIsoseismal(int isoseismalId, double x, double y)
        {
            var tempList = (ArrayList)_isoseismalList[isoseismalId - 1];

            if (tempList == null) tempList = new ArrayList();

            tempList.Add(new Point { X = x, Y = y });
            _isoseismalList[isoseismalId - 1] = tempList;
        }

        List<Feature<Polygon>> MakePolygons()
        {
            var polygonCollection = new List<Feature<Polygon>>();

            var polygon = new Polygon();
            polygon.Rings = new PointCollectionList();

            for (int isoseimalId = _isoseismalMin; isoseimalId <= _isoseismalMax; isoseimalId++)
            {
                ArrayList points = (ArrayList)_isoseismalList[isoseimalId - 1];
                var pointCollection = new PointCollection();
                for (int p = 0; p < points.Count; p++)
                {
                    var pt = points[p] as Point;
                    if (pt != null) pointCollection.Add(new[] { pt.X, pt.Y });
                }
                polygon.Rings.Add(pointCollection); 

                polygonCollection.Add(new Feature<Polygon> { Geometry = polygon, Attributes = new Dictionary<string, object> { { "MM", isoseimalId } } });
                if (!_isoSeismalDone)
                {
                    _maxIsoseismal = isoseimalId;
                    _isoSeismalDone = true;
                }
            }

            var gateway = new ArcGISGateway();
            var op = new SimplifyGeometry<Polygon>("/Utilities/Geometry/GeometryServer".AsEndpoint())
            {
                SpatialReference = new SpatialReference { Wkid = 2193 },
                Geometries = new GeometryCollection<Polygon>()
            };
            op.Geometries.Geometries = polygonCollection.Select(f => f.Geometry).ToList();

            var simplified = gateway.Simplify<Polygon>(op).Geometries;

            var result = polygonCollection;
            for (int i = 0; i < result.Count; i++)
                result[i].Geometry = simplified[i];

            var response = new List<Feature<Polygon>>();
            foreach (var feature in result)
                response.AddIfNotExists(feature);

            return response;
        }
    }

}