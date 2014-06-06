using ArcGIS.ServiceModel.Common;
using ArcGIS.ServiceModel;
using ArcGIS.ServiceModel.Operation;
using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using UnifiedSearch.Nancy.Model;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Hosting;

namespace UnifiedSearch.Nancy.Interface
{
    public class SearchModule : NancyModule
    {
        static ArcGIS.ServiceModel.ISerializer _serializer = new JsonDotNetSerializer();
        const int CacheMinutesTimeout = 30;

        public SearchModule()
        {
            // Used by ArcGIS Online when adding this service as a custom locator it does a check that it is a valid Esri locator
            // so we will cheat and just return one that we know works
            Get[@"/GeocodeServer"] = _ =>
            {
                var client = new HttpClient();
                var content = client.GetStringAsync("http://geocode.arcgis.com/arcgis/rest/services/World/geocodeserver?f=json").Result;
               
                return Response.AsText(content).WithContentType("application/json");
            };

Get[@"/GeocodeServer/findAddressCandidates", true] = async (x, ct) =>
{
    return await EsriSearch();
};

            Get[@"/GeocodeServer/suggest", true] = async (x, ct) =>
            {
                return await EsriSuggest();
            };
        }

        async Task<List<dynamic>> DoSearch(SearchRequest searchRequest)
        {
            String queryString = searchRequest.SearchString;
            if (String.IsNullOrWhiteSpace(queryString)) return null;

            var searchConfig = CheckConfig();

            var results = new List<object>();
            foreach (var queryObject in searchConfig.QuerySearches)
            {
                if (!String.IsNullOrEmpty(queryObject.Regex) && !new Regex(queryObject.Regex).IsMatch(queryString)) continue;

                var query = new Query(queryObject.Endpoint.AsEndpoint())
                {
                    Where = String.Format(queryObject.Expression, queryString),
                    OutputSpatialReference = new SpatialReference { Wkid = searchRequest.Wkid ?? searchConfig.OutputWkid },
                    ReturnGeometry = searchRequest.ReturnGeometry
                };
                System.Diagnostics.Debug.WriteLine(String.Format("SearchService where clause for query endpoint {0}, {1}", query.RelativeUrl, query.Where));

                switch (queryObject.GeometryType)
                {
                    case GeometryTypes.Point:
                        var pointResult = await SingleQuery<Point>(new ArcGISGateway(queryObject.Endpoint, _serializer), query);
                        if (pointResult != null) results.AddRange(pointResult);
                        break;
                    case GeometryTypes.Envelope:
                        var extentResult = await SingleQuery<Extent>(new ArcGISGateway(queryObject.Endpoint, _serializer), query);
                        if (extentResult != null) results.AddRange(extentResult);
                        break;
                    case GeometryTypes.MultiPoint:
                        var multiPointResult = await SingleQuery<MultiPoint>(new ArcGISGateway(queryObject.Endpoint, _serializer), query);
                        if (multiPointResult != null) results.AddRange(multiPointResult);
                        break;
                    case GeometryTypes.Polyline:
                        var polylineResult = await SingleQuery<Polyline>(new ArcGISGateway(queryObject.Endpoint, _serializer), query);
                        if (polylineResult != null) results.AddRange(polylineResult);
                        break;
                    case GeometryTypes.Polygon:
                        var polygonResult = await SingleQuery<Polygon>(new ArcGISGateway(queryObject.Endpoint, _serializer), query);
                        if (polygonResult != null) results.AddRange(polygonResult);
                        break;
                };
            }

            foreach (var findObject in searchConfig.FindSearches)
            {
                var find = new Find(findObject.Endpoint.AsEndpoint())
                {
                    SearchText = queryString,
                    OutputSpatialReference = new SpatialReference { Wkid = searchRequest.Wkid ?? searchConfig.OutputWkid },
                    ReturnGeometry = searchRequest.ReturnGeometry,
                    SearchFields = findObject.SearchFields.ToList(),
                    LayerIdsToSearch = findObject.LayerIds.ToList()
                };
                System.Diagnostics.Debug.WriteLine(String.Format("SearchService search text for find endpoint {0}, {1}", find.RelativeUrl, find.SearchText));

                var findResults = await SingleFind(new ArcGISGateway(findObject.Endpoint, _serializer), find);
                if (findResults != null) results.AddRange(findResults);
            }

            return results;             
        }

        async Task<SuggestGeocodeResponse> EsriSuggest()
        {
            var queryString = base.Context.Request.Query["text"];
            if (String.IsNullOrWhiteSpace(queryString)) return null;

            var searchConfig = CheckConfig();

            var response = new SuggestGeocodeResponse();
            var results = await DoSearch(new SearchRequest { SearchString = queryString, ReturnGeometry = false });
            if (results == null || !results.Any()) return response;

            var suggestions = new List<Suggestion>();
            // convert results to SuggestGeocodeResponse since that is what the Esri control expects
            foreach (var result in results)
            {
                Dictionary<String, object> resultAttributes = result.Attributes;
                suggestions.Add(new Suggestion
                {
                    Text = resultAttributes[searchConfig.ReturnFields.Intersect(resultAttributes.Select(r => r.Key)).FirstOrDefault()].ToString()
                });
            }
            response.Suggestions = suggestions.ToArray();
            return response;
        }

        async Task<SingleInputCustomGeocodeResponse> EsriSearch()
        {
            var queryString = base.Context.Request.Query["SingleLine"];
            var outSR = base.Context.Request.Query["outSR"];
            int? wkid = null;

            if (!String.IsNullOrWhiteSpace(outSR))
            {
                var sr = (SpatialReference)JsonConvert.DeserializeObject<SpatialReference>(outSR);
                wkid = sr.LatestWkid; 
            }
            if (String.IsNullOrWhiteSpace(queryString)) return null;

            var searchConfig = CheckConfig();

            var results = await DoSearch(new SearchRequest { SearchString = queryString, Wkid = wkid });
            if (results == null || !results.Any()) return new SingleInputCustomGeocodeResponse();

            var response = new SingleInputCustomGeocodeResponse { SpatialReference = results.First().Geometry.SpatialReference };
            var candidates = new List<Candidate>();
            // convert results to SingleInputCustomGeocodeResponse since that is what the Esri control expects
            foreach (var result in results)
            {
                var candidate = new Candidate { Score = 100, Attributes = new Dictionary<String, object> { { "Loc_name", "Eagle Unified Search" } } };
                Dictionary<String, object> resultAttributes = result.Attributes;
                candidate.Location = result.Geometry.GetCenter();
                candidate.Address = resultAttributes[searchConfig.ReturnFields.Intersect(resultAttributes.Select(r => r.Key)).FirstOrDefault()].ToString();
                candidates.Add(candidate);
            }
            response.Candidates = candidates.ToArray();
            return response;        
        }

        async Task<List<Feature<T>>> SingleQuery<T>(ArcGISGateway gateway, Query query) where T : IGeometry
        {     
            var queryResult = await gateway.DoQuery<T>(query);
            if (queryResult != null && queryResult.Features != null)
            {
                foreach (var feature in queryResult.Features)
                    if (feature != null && feature.Geometry != null) feature.Geometry.SpatialReference = queryResult.SpatialReference;
                return queryResult.Features.ToList();
            }

            return null;
        }

        async Task<List<Feature<IGeometry>>> SingleFind(ArcGISGateway gateway, Find find)
        {
            var findResult = await gateway.DoFind(find);
            if (findResult != null && findResult.Results != null && findResult.Results.Any())
                return findResult.Results.ToFeatures();    

            return null;
        }

        SearchConfigurationData CheckConfig()
        {     
            System.Diagnostics.Debug.WriteLine("Loading search configuration from file");
            FileInfo configFile = new FileInfo(HostingEnvironment.MapPath("~/bin/Json/searchConfig.json"));
            if (!configFile.Exists) throw new FileNotFoundException(configFile.Name);
            String json = File.ReadAllText(configFile.FullName);
            
            return JsonConvert.DeserializeObject<SearchConfigurationData>(json);            
        }
    }

    public class SearchRequest
    {
        public SearchRequest()
        {
            ReturnGeometry = true;
        }

        public string SearchString { get; set; }

        public int? Wkid { get; set; }

        public bool ReturnGeometry { get; set; }
    }
}
