using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyNameSpace
{
    public partial class MyClassName
    {
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
        {
        }

        partial void ProcessResponse(HttpClient client, HttpResponseMessage response)
        {
            var result = response.Content.ReadAsStringAsync().Result;
        }
    }
}