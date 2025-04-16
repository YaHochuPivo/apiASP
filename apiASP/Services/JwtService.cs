using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using apiASP.Models;
using Microsoft.IdentityModel.Tokens;

namespace apiASP.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public JwtToken GenerateToken(User user)
        {
            try
            {
                _logger.LogInformation($"Начало генерации токена для пользователя: {user.Email}");

                var jwtKey = _configuration["Jwt:Key"];
                var jwtIssuer = _configuration["Jwt:Issuer"];
                var jwtAudience = _configuration["Jwt:Audience"];

                if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
                {
                    throw new InvalidOperationException("JWT configuration is missing or incomplete");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim("userId", user.UserId.ToString()),
                    new Claim("email", user.Email)
                };

                _logger.LogInformation($"Созданы claims для токена: {string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}"))}");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(24),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation($"Токен успешно сгенерирован для пользователя: {user.Email}");

                return new JwtToken { Token = tokenString };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации JWT токена");
                throw;
            }
        }
    }

    public class JwtToken
    {
        public string Token { get; set; }
    }
} 