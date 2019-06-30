using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBazrPeer.Helpers
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Fedago specific application logic
        /// </summary>
        /// <param name="id">GUID before published or int after published</param>
        /// <returns>True if published, false otherwise</returns>
        public static bool IsPublished(this string id)
        {
            bool isNotPublished = string.IsNullOrEmpty(id) || id == "0" || id.Length > 10;
            return !isNotPublished;
        }

        //https://stackoverflow.com/questions/7230383/c-convert-dictionary-to-namevaluecollection
        public static NameValueCollection ToNameValueCollection<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            var nameValueCollection = new NameValueCollection();

            foreach (var kvp in dict)
            {
                string value = null;
                if (kvp.Value != null)
                    value = kvp.Value.ToString();

                nameValueCollection.Add(kvp.Key.ToString(), value);
            }

            return nameValueCollection;
        }

        public static IDictionary<string, string> ToDictionary(this NameValueCollection nameValueCollection)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var k in nameValueCollection.AllKeys)
            {
                if (k == null)
                {
                    dict.Add(string.Empty, nameValueCollection[k]);
                }
                else
                {
                    dict.Add(k, nameValueCollection[k]);
                }
            }
            return dict;
        }

        public static string ToCsv<T>(this IEnumerable<T> values, bool forceInQuotes = false)
        {
            if (forceInQuotes)
            {
                return ToFlatFile(values.Select(lmb => string.Format("\"{0}\"", lmb)), ",");
            }
            else
            {
                return ToFlatFile(values, ",");
            }
        }

        public static string ToFlatFile<T>(this IEnumerable<T> values, string deliminator)
        {
            StringBuilder tmp = null;
            foreach (T value in values)
            {
                if (tmp == null)
                {
                    tmp = new StringBuilder();
                    if (value == null)
                    {
                        tmp.Append(string.Empty);
                    }
                    else
                    {
                        tmp.Append(value.ToString());
                    }
                }
                else
                {
                    if (value == null)
                    {
                        tmp.Append(string.Format("{0}", deliminator));
                    }
                    else
                    {
                        tmp.Append(string.Format("{1}{0}", value.ToString().Contains(deliminator) ? string.Format("\"{0}\"", value) : value.ToString(), deliminator));
                    }
                }
            }
            return tmp == null ? string.Empty : tmp.ToString();
        }

        public static string ToShortName(this string name, int length = 10)
        {
            string shortName = name;
            bool shorten = name.Length > 10;
            if (shorten)
            {
                shortName = string.Format("{0}...", name.Substring(0, 10));
            }
            return shortName;
        }
    }
}
