using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class GitHubService
{
    private readonly HttpClient githubClient;

    public GitHubService(HttpClient githubClient)
    {
        this.githubClient = githubClient;
    }

    public async Task<IEnumerable<GitHubIssue>> GetAspNetDocsIssues()
    {
        var response = await githubClient.GetAsync("/repos/aspnet/AspNetCore.Docs/issues?state=open&sort=created&direction=desc");
        response.EnsureSuccessStatusCode();
        using var responseStream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<IEnumerable<GitHubIssue>>(responseStream);
    }
}

public class GitHubIssue
{
    [JsonPropertyName("html_url")]
    public string Url { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime Created { get; set; }
}
