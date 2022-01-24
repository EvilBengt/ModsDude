using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core.Services;

internal class Remote
{
    const string _baseurl = "http://localhost/dev/modsdude";
    const string _authString = "ZHVkZTphc2Rhc2Rhc2Rhc2Q=";


    public async Task<IEnumerable<string>> FetchProfiles()
    {
        HttpRequestMessage request = new(HttpMethod.Get, _baseurl + "/profiles");

        request.Headers.Authorization = new("Basic", _authString);

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);

        RemoteProfilesResponse? responseContent = await response.Content.ReadFromJsonAsync<RemoteProfilesResponse>();

        if (responseContent?.Profiles is null)
        {
            throw new Exception("Invalid response from server when fetching profiles.");
        }

        return responseContent.Profiles;
    }
}
