using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using apiASP.Data;
using apiASP.Models;
using System.Security.Claims;

namespace apiASP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(ApplicationDbContext context, ILogger<ReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Получение всех отзывов
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<object>>> GetReviews()
        {
            try
            {
                _logger.LogInformation("Начало получения отзывов");
                
                if (_context.Reviews == null)
                {
                    _logger.LogWarning("DbSet Reviews не инициализирован");
                    return NotFound(new { message = "База данных отзывов недоступна" });
                }

                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Psychologist)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.ReviewId,
                        r.Rating,
                        r.Comment,
                        r.CreatedAt,
                        User = new
                        {
                            r.User.UserId,
                            r.User.FullName
                        },
                        Psychologist = new
                        {
                            r.Psychologist.PsychologistId,
                            r.Psychologist.FullName,
                            r.Psychologist.Specialization
                        }
                    })
                    .ToListAsync();

                _logger.LogInformation($"Успешно получено {reviews.Count} отзывов");
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении отзывов");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // Создание нового отзыва
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Review>> CreateReview(CreateReviewRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Пользователь не авторизован" });
                }

                var psychologist = await _context.Psychologists.FindAsync(request.PsychologistId);
                if (psychologist == null)
                {
                    return NotFound(new { message = "Психолог не найден" });
                }

                var review = new Review
                {
                    UserId = userId,
                    PsychologistId = request.PsychologistId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Отзыв успешно создан" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании отзыва");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }

    public class CreateReviewRequest
    {
        public int PsychologistId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
} 