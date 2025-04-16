using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apiASP.Data;
using apiASP.Models;
using Microsoft.Extensions.Logging;

namespace apiASP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PsychologistController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PsychologistController> _logger;

        public PsychologistController(ApplicationDbContext context, ILogger<PsychologistController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Psychologist>>> GetPsychologists()
        {
            try
            {
                _logger.LogInformation("Начало получения списка психологов");
                
                // Проверяем, есть ли психологи в базе
                if (!await _context.Psychologists.AnyAsync())
                {
                    _logger.LogInformation("База данных пуста. Создаем тестового психолога...");
                    await CreateTestPsychologist();
                }
                
                var query = await _context.Psychologists
                    .Where(p => p.IsActive)
                    .Select(p => new
                    {
                        p.PsychologistId,
                        p.FullName,
                        p.Specialization,
                        p.Biography,
                        p.YearsOfExperience,
                        p.PhotoUrl,
                        p.IsActive,
                        AverageRating = p.Reviews.Any() ? 
                            Math.Round(p.Reviews.Average(r => r.Rating), 1) : 0,
                        ReviewsCount = p.Reviews.Count()
                    })
                    .OrderBy(p => p.FullName)
                    .ToListAsync();

                _logger.LogInformation($"Получено {query.Count} психологов");

                if (!query.Any())
                {
                    _logger.LogWarning("Список психологов пуст");
                    return Ok(new List<object>()); // Возвращаем пустой список вместо null
                }

                return Ok(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка психологов");
                return StatusCode(500, new { message = "Ошибка при получении списка психологов", error = ex.Message });
            }
        }

        private async Task CreateTestPsychologist()
        {
            try
            {
                var testPsychologist = new Psychologist
                {
                    FullName = "Анна Иванова",
                    Specialization = "Семейный психолог",
                    Biography = "Опыт работы более 10 лет. Помогает парам и семьям.",
                    YearsOfExperience = 10,
                    PhotoUrl = "https://avatars.mds.yandex.net/i?id=ffaf3e631d134cab4beb31d369414a557bfade48-4268363-images-thumbs&n=13",
                    IsActive = true
                };

                _context.Psychologists.Add(testPsychologist);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Тестовый психолог успешно создан");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании тестового психолога");
                throw;
            }
        }
    }
} 