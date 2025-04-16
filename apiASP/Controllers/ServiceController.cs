using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apiASP.Data;
using apiASP.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace apiASP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(ApplicationDbContext context, ILogger<ServiceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            try
            {
                _logger.LogInformation("Начало получения списка услуг");

                // Проверяем подключение к БД
                if (!_context.Database.CanConnect())
                {
                    _logger.LogError("Не удалось подключиться к БД");
                    return StatusCode(500, new { message = "Ошибка подключения к базе данных" });
                }

                try
                {
                    // Проверяем количество услуг в базе
                    var count = await _context.Services.CountAsync();
                    _logger.LogInformation($"Текущее количество услуг в базе: {count}");

                    // Загружаем все услуги с информацией о психологах
                    var services = await _context.Services
                        .Include(s => s.Psychologist)
                        .AsNoTracking()
                        .ToListAsync();

                    _logger.LogInformation($"Загружено {services.Count} услуг");

                    // Если услуг меньше 3, пересоздаем тестовые данные
                    if (services.Count < 3)
                    {
                        _logger.LogInformation("Недостаточно услуг, создаем тестовые данные...");
                        await RecreateTestServices();
                        
                        // Загружаем услуги повторно после создания тестовых данных
                        services = await _context.Services
                            .Include(s => s.Psychologist)
                            .AsNoTracking()
                            .ToListAsync();
                    }

                    // Логируем каждую найденную услугу
                    foreach (var service in services)
                    {
                        _logger.LogInformation($"Услуга: ID={service.ServiceId}, " +
                            $"Название={service.Title}, " +
                            $"Психолог={service.Psychologist?.FullName ?? "не назначен"}, " +
                            $"Цена={service.Price}");
                    }

                    var result = services.Select(s => new
                    {
                        s.ServiceId,
                        s.Title,
                        s.Description,
                        s.DurationMinutes,
                        s.Price,
                        s.PsychologistId,
                        Psychologist = s.Psychologist != null ? new
                        {
                            s.Psychologist.PsychologistId,
                            s.Psychologist.FullName,
                            s.Psychologist.Specialization
                        } : null
                    }).ToList();

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при выполнении запроса к БД");
                    return StatusCode(500, new { message = "Ошибка при получении данных из БД", error = ex.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Необработанная ошибка в GetServices");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера", error = ex.Message });
            }
        }

        private async Task RecreateTestServices()
        {
            try
            {
                // Получаем существующих психологов
                var psychologists = await _context.Psychologists.ToListAsync();
                
                if (!psychologists.Any())
                {
                    _logger.LogWarning("Нет доступных психологов для создания тестовых услуг");
                    return;
                }

                // Удаляем все существующие услуги
                var existingServices = await _context.Services.ToListAsync();
                if (existingServices.Any())
                {
                    _context.Services.RemoveRange(existingServices);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Существующие услуги удалены");
                }

                // Создаем новые услуги
                var testServices = new List<Service>
                {
                    new Service
                    {
                        Title = "Индивидуальная консультация",
                        Description = "Личная беседа с психологом для решения внутренних конфликтов.",
                        DurationMinutes = 60,
                        Price = 3000.00M,
                        PsychologistId = psychologists[0].PsychologistId
                    },
                    new Service
                    {
                        Title = "Семейная терапия",
                        Description = "Сеанс для пар или семей.",
                        DurationMinutes = 90,
                        Price = 5000.00M,
                        PsychologistId = psychologists[0].PsychologistId
                    }
                };

                if (psychologists.Count > 1)
                {
                    testServices.Add(new Service
                    {
                        Title = "Консультация для подростков",
                        Description = "Работа с подростками 12-18 лет.",
                        DurationMinutes = 45,
                        Price = 2500.00M,
                        PsychologistId = psychologists[1].PsychologistId
                    });
                }

                await _context.Services.AddRangeAsync(testServices);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Создано {testServices.Count} новых тестовых услуг");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании тестовых услуг");
                throw;
            }
        }
    }
} 