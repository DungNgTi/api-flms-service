﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;

namespace api_auth_service.Service
{

    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;
        private readonly string _apiAuthUrl;

        public AuthService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiBaseUrl = configuration["ApiBaseUrl"]; // Read base URL from appsettings.json
            _apiAuthUrl = configuration["ApiAuthUrl"]; // Read auth URL from appsettings.json
        }

        public async Task<CurrentUser?> GetCurrentUserAsync()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["googleToken"];
            if (string.IsNullOrEmpty(token)) return null;

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiAuthUrl}/api/auth/current-user")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
            };

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            return JsonSerializer.Deserialize<ApiResponseDto>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.UserInfo;
        }

        public async Task<string> GetLoginUrl(string redirect)
        {
            var baseUri = new Uri(_apiAuthUrl);
            var loginUri = new Uri(baseUri, "/login");

            // Use UriBuilder to modify the URL
            var uriBuilder = new UriBuilder(loginUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["url"] = redirect;
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }

        public async Task<string> GetLogoutUrl(string redirect)
        {
            var baseUri = new Uri(_apiAuthUrl);
            var loginUri = new Uri(baseUri, "/logout");

            // Use UriBuilder to modify the URL
            var uriBuilder = new UriBuilder(loginUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["url"] = redirect;
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }

        public void HandleLogin(HttpRequest? Request, HttpResponse Response, string? Token)
        {
            if (Request == null) return;
            if (Token == null || string.IsNullOrEmpty(Token)) return;

            var domain = new Uri(Request.GetEncodedUrl()).Host;
            var isLocalHost = Request.Host.Host.Contains("localhost") || domain == "127.0.0.1";
            Response.Cookies.Append("googleToken", Token, new CookieOptions
            {
                HttpOnly = true,  // Prevents JavaScript access
                Secure = !isLocalHost, // Secure=True only in production
                SameSite = isLocalHost ? SameSiteMode.Lax : SameSiteMode.None, // Allows cross-origin cookie sending
                Domain = isLocalHost ? null : domain, // ✅ Allows sharing cookies across subdomains only in production
                Expires = DateTime.UtcNow.AddDays(7)
            });
        }

        public void HandleLogout(HttpRequest? Request, HttpResponse Response)
        {
            if (Request == null) return;

            var domain = new Uri(Request.GetEncodedUrl()).Host;
            var isLocalHost = Request.Host.Host.Contains("localhost") || domain == "127.0.0.1";

            if (Request.Cookies.ContainsKey("googleToken"))
            {
                Response.Cookies.Delete("googleToken");
            }
        }
    }

    // DTO for API response
    public class ApiResponseDto
    {
        public CurrentUser UserInfo { get; set; }
    }


    // DTO for User Data
    public class CurrentUser
    {

        public string Picture {  get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Issuer { get; set; } // Example extra claim
        public DateTime Expiration { get; set; }
    }
}