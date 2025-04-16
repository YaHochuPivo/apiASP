using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using apiASP.Data;
using apiASP.Models;
using apiASP.Services;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace apiASP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, JwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation($"Попытка регистрации пользователя с email: {request.Email}");

                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    _logger.LogWarning($"Пользователь с email {request.Email} уже существует");
                    return BadRequest(new { message = "Пользователь с таким email уже существует" });
                }

                var user = new User
                {
                    Email = request.Email,
                    FullName = request.FullName,
                    PasswordHash = HashPassword(request.Password),
                    PhoneNumber = request.PhoneNumber,
                    RegisteredAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Пользователь успешно зарегистрирован: {user.Email}");

                var token = _jwtService.GenerateToken(user);
                
                return Ok(new
                {
                    token = token.Token,
                    user = new
                    {
                        user.UserId,
                        user.Email,
                        user.FullName,
                        user.PhoneNumber
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации пользователя");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Попытка входа для пользователя: {request.Email}");

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    _logger.LogWarning($"Пользователь не найден: {request.Email}");
                    return Unauthorized(new { message = "Неверный email или пароль" });
                }

                _logger.LogInformation($"Проверка пароля для пользователя: {request.Email}");
                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Неверный пароль для пользователя: {request.Email}");
                    return Unauthorized(new { message = "Неверный email или пароль" });
                }

                var token = _jwtService.GenerateToken(user);
                _logger.LogInformation($"Токен сгенерирован для пользователя: {user.Email}");

                var response = new
                {
                    token = token.Token,
                    user = new
                    {
                        user.UserId,
                        user.Email,
                        user.FullName,
                        user.PhoneNumber
                    }
                };

                _logger.LogInformation($"Успешный вход пользователя: {user.Email}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе пользователя");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult> GetProfile()
        {
            try
            {
                _logger.LogInformation("Получение профиля пользователя");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogError("ID пользователя не найден в токене");
                    return Unauthorized(new { message = "ID пользователя не найден" });
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogError($"Не удалось преобразовать ID пользователя: {userIdClaim.Value}");
                    return BadRequest(new { message = "Некорректный ID пользователя" });
                }

                var user = await _context.Users
                    .Include(u => u.Orders)
                        .ThenInclude(o => o.OrderItems)
                            .ThenInclude(oi => oi.Service)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    _logger.LogError($"Пользователь с ID {userId} не найден");
                    return NotFound(new { message = "Пользователь не найден" });
                }

                var orders = user.Orders.Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.Status,
                    o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.ServiceId,
                        ServiceTitle = oi.Service.Title,
                        oi.Quantity,
                        oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    })
                }).ToList();

                var result = new
                {
                    user.UserId,
                    user.Email,
                    user.FullName,
                    user.PhoneNumber,
                    user.RegisteredAt,
                    Orders = orders
                };

                _logger.LogInformation($"Профиль пользователя {user.Email} успешно загружен");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля пользователя");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке пароля");
                return false;
            }
        }

        [HttpPost("updatetestpassword")]
        public async Task<IActionResult> UpdateTestPassword()
        {
            try
            {
                var testUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == "mona2012@list.ru");

                if (testUser == null)
                {
                    _logger.LogWarning("Тестовый пользователь не найден");
                    return NotFound(new { message = "Тестовый пользователь не найден" });
                }

                testUser.PasswordHash = HashPassword("123456");
                await _context.SaveChangesAsync();

                _logger.LogInformation("Пароль тестового пользователя успешно обновлен");
                return Ok(new { message = "Пароль тестового пользователя обновлен на '123456'" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении пароля тестового пользователя");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
} 