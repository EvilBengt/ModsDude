using ModsDude.Core.Models.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core.Services;

public class Remote
{
    const string _baseurl = "http://localhost/dev/modsdude";
    const string _authString = "ZHVkZTphc2Rhc2Rhc2Rhc2Q=";


    public async Task<IEnumerable<string>> FetchProfiles()
    {
        HttpRequestMessage request = CreateGet("/profiles/index.php");

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        RemoteProfilesResponse? responseContent = await response.Content.ReadFromJsonAsync<RemoteProfilesResponse>();

        if (responseContent?.Profiles is null)
        {
            throw new Exception("Invalid response from server when fetching profiles.");
        }

        return responseContent.Profiles;
    }

    public async Task<IEnumerable<string>> FetchProfile(string name)
    {
        HttpRequestMessage request = CreateGet("/profiles/profile/" + name);

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        ProfileResponse? responseContent = await response.Content.ReadFromJsonAsync<ProfileResponse>();

        if (responseContent?.Mods is null)
        {
            throw new Exception("Invalid response from server when fetching profile.");
        }

        return responseContent.Mods;
    }

    public async Task CreateProfile(string name)
    {
        HttpRequestMessage request = CreatePost("/profiles/create.php");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "filename", name }
        });

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new Exception("Name already taken.");
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveProfile(string name)
    {
        HttpRequestMessage request = CreatePost("/profiles/remove.php");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "filename", name }
        });

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateProfile(string name, IEnumerable<string> mods)
    {
        HttpRequestMessage request = CreatePost("/profiles/update.php?filename=" + name);

        request.Content = JsonContent.Create(new ProfileUpdateRequest(mods));

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    public async Task RenameProfile(string oldName, string newName)
    {
        HttpRequestMessage request = CreatePost("/profiles/rename.php");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "oldName", oldName },
            { "newName", newName }
        });

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new Exception("Name already taken.");
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<NeededMods> FetchNeededModsList()
    {
        HttpRequestMessage request = CreateGet("/mods/needed.php");

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        NeededMods? responseContent = await response.Content.ReadFromJsonAsync<NeededMods>();

        if (responseContent?.Needed is null || responseContent.Unneeded is null)
        {
            throw new Exception("Invalid response from server when fetching needed mods.");
        }

        return responseContent;
    }

    public async Task<Dictionary<string, ModInfo>> FetchModIndex()
    {
        HttpRequestMessage request = CreateGet("/mods/index.json");

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        Dictionary<string, ModInfo>? responseContent = await response.Content.ReadFromJsonAsync<Dictionary<string, ModInfo>>();

        if (responseContent is null)
        {
            throw new Exception("Invalid response from server when fetching mod index.");
        }

        return responseContent;
    }

    public async Task UploadMod(FileStream fileStream, string? hash)
    {
        HttpRequestMessage request = CreatePost("/mods/upload.php");

        MultipartFormDataContent content = new();

        content.Add(new StreamContent(fileStream), "file", Path.GetFileName(fileStream.Name));

        if (hash is not null)
        {
            content.Add(new StringContent(hash), "hash");
        
        }
        content.Add(new StringContent(fileStream.Length.ToString()), "size");
        request.Content = content;

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveMod(string name)
    {
        HttpRequestMessage request = CreatePost("/mods/remove.php");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "filename", name }
        });

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }


    private HttpRequestMessage CreateGet(string endpoint)
    {
        return CreateRequest(HttpMethod.Get, endpoint);
    }

    private HttpRequestMessage CreatePost(string endpoint)
    {
        return CreateRequest(HttpMethod.Post, endpoint);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
    {
        HttpRequestMessage request = new(method, _baseurl + endpoint);

        request.Headers.Authorization = new("Basic", _authString);

        return request;
    }
}
