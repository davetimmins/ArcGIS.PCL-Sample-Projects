using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UnifiedSearch.Nancy.Model
{
    [DataContract]
    public class SearchConfigurationData
    {
        [DataMember(Name="querySearches")]
        public List<QueryObject> QuerySearches { get; set; }
        [DataMember(Name = "findSearches")]
        public List<FindObject> FindSearches { get; set; }
        [DataMember(Name = "returnFields")]
        public List<String> ReturnFields { get; set; }
        [DataMember(Name = "outputWkid")]
        public int? OutputWkid { get; set; }
    }

    [DataContract]
    public class QueryObject
    {
        [DataMember(Name = "endpoint")]
        public String Endpoint { get; set; }
        [DataMember(Name = "expression")]
        public String Expression { get; set; }
        [DataMember(Name = "type")]
        public String GeometryType { get; set; }
        [DataMember(Name = "regex")]
        public String Regex { get; set; }
    }

    [DataContract]
    public class FindObject
    {
        [DataMember(Name = "endpoint")]
        public String Endpoint { get; set; }
        [DataMember(Name = "searchFields")]
        public String[] SearchFields { get; set; }
        [DataMember(Name = "layerIds")]
        public int[] LayerIds { get; set; }
    }
}
