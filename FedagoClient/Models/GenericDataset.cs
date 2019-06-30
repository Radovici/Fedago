using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DataBazrPeer.Models
{
    public class GenericDataset
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("values")]
        public List<List<object>> Values { get; set; }

        public static IEnumerable<IDictionary<string, object>> ConvertResponse(string response)
        {
            List<IDictionary<string, object>> results = new List<IDictionary<string, object>>();
            GenericDataset dataset = JsonConvert.DeserializeObject<GenericDataset>(response); // GenericDataset.FromJson(response);
            if (dataset.Values != null && dataset.Values.Any())
            {
                foreach (var values in dataset.Values)
                {
                    Dictionary<string, object> result = new Dictionary<string, object>();
                    for (int index = 0; index < values.Count; index++)
                    {
                        result[dataset.Columns[index]] = values[index];
                    }
                    results.Add(result);
                }
            }
            return results;
        }
    }
}