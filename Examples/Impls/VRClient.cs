using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using JsonUtility;
using System.Threading.Tasks;
using VRClient.Abstractions;
using VRClient.Models;

namespace VRClient.Impls
{
    public class VRClient : IVRClient
    {
        const string BaseUrl = "https://localhost:7089";

        public bool IsAuthenticated { get; private set; }

        private readonly HttpClient _httpClient;

        public VRClient() 
        {
            _httpClient = new HttpClient();
        }

        public async Task AuthAsync(string username, string password)
        {
            var url = $"{BaseUrl}/api/user/login";
            var body = new
            {
                username,
                password
            };
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadAsStringAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            IsAuthenticated = true;
        }

        public async Task<UserInfo> GetMeAsync()
        {
            if (!IsAuthenticated)
                throw new InvalidOperationException("Not authorized");
            var url = $"{BaseUrl}/api/user/getme";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var userInfo = await JsonUtility.FromJson<UserInfo>(content);
            return userInfo;
        }
    }
}
