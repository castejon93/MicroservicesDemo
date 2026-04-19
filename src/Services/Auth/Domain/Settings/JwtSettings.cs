using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Domain.Settings
{
    /// <summary>
    /// Configuration settings for JWT token generation and validation.
    /// Maps to the "JwtSettings" section in appsettings.json.
    /// </summary>
    public class JwtSettings
    {
        // The configuration section name in appsettings.json
        public const string SectionName = "JwtSettings";

        /// <summary>
        /// Secret key used to sign and validate JWT tokens.
        /// Must be at least 32 characters for HS256 algorithm.
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Identifies who created the token (typically your API URL).
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Identifies the intended recipients of the token.
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// How long the access token remains valid (in minutes).
        /// </summary>
        public int ExpirationInMinutes { get; set; } = 60;

        /// <summary>
        /// How long the refresh token remains valid (in days).
        /// </summary>
        public int RefreshTokenExpirationInDays { get; set; } = 7;
    }
}
