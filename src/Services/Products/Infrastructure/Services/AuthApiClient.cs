// ============================================================
// FILE: src/Services/Products/Products.Infrastructure/Services/AuthApiClient.cs
// PURPOSE: HTTP client to call Auth microservice
// USE CASE: When Products needs user information
// ============================================================

using System.Net.Http.Json;

namespace Products.Infrastructure.Services
{
    /// <summary>
    /// HTTP Client to communicate with Auth microservice
    /// 
    /// WHY?
    /// - Products microservice doesn't have access to User database
    /// - When we need user info (e.g., who created a product)
    /// - We call Auth API instead of direct database access
    /// 
    /// PATTERN: Service-to-Service Communication
    /// </summary>
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Base URL configured in DI registration
        }

        /// <summary>
        /// Get user info by ID from Auth microservice
        /// </summary>
        /// <param name="userId">User ID to look up</param>
        /// <returns>User info or null if not found</returns>
        public async Task<UserInfo?> GetUserByIdAsync(int userId)
        {
            try
            {
                // Call Auth microservice API
                // URL: http://auth-api:8080/api/auth/users/{userId}
                var response = await _httpClient.GetAsync($"api/auth/users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserInfo>();
                }

                return null;
            }
            catch (HttpRequestException)
            {
                // Auth service unavailable - handle gracefully
                return null;
            }
        }
    }

    /// <summary>
    /// DTO for user info received from Auth service
    /// </summary>
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}