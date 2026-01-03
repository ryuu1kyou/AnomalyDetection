using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;

namespace AnomalyDetection;

public abstract class SystemIntegrationTestBase : IClassFixture<AnomalyDetectionWebApplicationFactory>
{
    protected HttpClient Client { get; }
    protected AnomalyDetectionWebApplicationFactory Factory { get; }
    protected IServiceProvider ServiceProvider => Factory.Services;

    protected SystemIntegrationTestBase(AnomalyDetectionWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("en-US"));
    }

    protected async Task AuthenticateAsync(string username = "admin", string password = "1q2w3E*")
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("client_id", "AnomalyDetection_App"),
            new KeyValuePair<string, string>("scope", "AnomalyDetection")
        });

        var response = await Client.PostAsync("/connect/token", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        dynamic tokenObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        string accessToken = tokenObj.access_token;

        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    }
}
