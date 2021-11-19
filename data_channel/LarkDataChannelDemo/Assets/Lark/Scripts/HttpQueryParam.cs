using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lark {
    public class HttpQueryParam
    {
        private Dictionary<string, string> queryParams = new Dictionary<string, string>();

        public void Add(string key, string val)
        {
            queryParams.Add(key, val);
        }

        public void Remove(string key)
        {
            queryParams.Remove(key);
        }

        public string Get(string key)
        {
            return queryParams[key];
        }

        // to formdata string to MultipartFormDataSection
        public string ToFormDataString()
        {
            return ToString();
        }

        public override string ToString()
        {
            string res = "";
            foreach (string key in queryParams.Keys)
            {
                string val = queryParams[key];
                res += string.Format("{0:G}={1:G}&", key, val);
            }
            return res;
        }
    }
}
