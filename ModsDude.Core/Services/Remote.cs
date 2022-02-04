using ModsDude.Core.Models;
using ModsDude.Core.Models.Remote;
using ModsDude.Core.Models.Settings;
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
    private readonly ApplicationSettings _settings;


    public Remote(ApplicationSettings settings)
    {
        _settings = settings;
    }


    public FileOperation FileOperation { get; set; } = new();


    // Profiles

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

    // Needed mods

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

    // Mods

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

    public async Task UploadMod(FileStream stream, string hash)
    {
        long totalSize = stream.Length;
        int bufferSize = 10 * 1024 * 1024;
        byte[] buffer = new byte[bufferSize];
        int totalReadBytes = 0;

        bool isLastChunk = false;

        FileOperation.OnStart(totalSize);

        while (!isLastChunk)
        {
            bool isFirstChunk = totalReadBytes == 0;
            int readBytes = stream.Read(buffer, 0, bufferSize);

            HttpRequestMessage request = CreatePost("/mods/uploadchunk.php");

            MultipartFormDataContent content = new();
            content.Add(new ByteArrayContent(buffer, 0, readBytes), "file", Path.GetFileName(stream.Name));

            if (totalReadBytes + bufferSize >= totalSize)
            {
                content.Add(new StringContent(""), "isLastChunk");
                content.Add(new StringContent(totalSize.ToString()), "size");
                content.Add(new StringContent(hash), "hash");

                isLastChunk = true;
            }
            else if (isFirstChunk)
            {
                content.Add(new StringContent(""), "isFirstChunk");
            }

            request.Content = content;

            using HttpClient client = new();
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            totalReadBytes += readBytes;
            FileOperation.OnIncrement(readBytes);
        }
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

    public async Task<Stream> DownloadMod(string name)
    {
        HttpRequestMessage request = CreateGet("/mods/mod/" + name);

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        return response.Content.ReadAsStream();
    }

    // Savegames

    public async Task<IEnumerable<string>> FetchSavegames()
    {
        HttpRequestMessage request = CreateGet("/savegames/index.php");

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        SavegamesResponse? responseContent = await response.Content.ReadFromJsonAsync<SavegamesResponse>();

        if (responseContent?.Savegames is null)
        {
            throw new Exception("Invalid response from server when fetching savegames.");
        }

        return responseContent.Savegames;
    }

    public async Task<SavegameInfo> FetchSavegameInfo(string name)
    {
        HttpRequestMessage request = CreateGet("/savegames/info.php?name=" + name);

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        SavegameInfo? responseContent = await response.Content.ReadFromJsonAsync<SavegameInfo>();

        if (responseContent is null)
        {
            throw new Exception("Invalid response from server when fetching savegame information.");
        }

        return responseContent;
    }

    public async Task CheckoutSavegame(string name)
    {
        HttpRequestMessage request = CreatePost("/savegames/checkout.php");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "name", name },
        });

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    public async Task UploadSavegame(Stream stream, string name)
    {
        HttpRequestMessage request = CreatePost("/savegames/upload.php");

        MultipartFormDataContent content = new();

        content.Add(new StreamContent(stream), "file", "upload.zip");
        content.Add(new StringContent(name), "name");
        request.Content = content;

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Stream> DownloadSavegame(string name)
    {
        HttpRequestMessage request = CreateGet("/savegames/savegame/" + name);

        using HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return response.Content.ReadAsStream();
    }

    public async Task RemoveSavegame(string name)
    {
        HttpRequestMessage request = CreatePost("/savegames/remove.php");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "name", name }
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
        string baseUrl = _settings.GetValidRemoteUrl();
        string username = _settings.GetValidRemoteUsername();
        string password = _settings.GetValidRemotePassword();

        string authString = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));


        HttpRequestMessage request = new(method, baseUrl + endpoint);

        request.Headers.Authorization = new("Basic", authString);

        return request;
    }
}
