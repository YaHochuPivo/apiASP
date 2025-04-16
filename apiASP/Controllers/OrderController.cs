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
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ApplicationDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Создание нового заказа
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                _logger.LogInformation("Начало создания заказа. Данные запроса: {@Request}", request);

                if (request?.Items == null || !request.Items.Any())
                {
                    _logger.LogWarning("Получен пустой список товаров");
                    return BadRequest(new { message = "Список товаров пуст" });
                }

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

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogError($"Пользователь с ID {userId} не найден");
                    return NotFound(new { message = "Пользователь не найден" });
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    TotalAmount = 0
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in request.Items)
                {
                    var service = await _context.Services.FindAsync(item.ServiceId);
                    if (service == null)
                    {
                        _logger.LogError($"Услуга с ID {item.ServiceId} не найдена");
                        return BadRequest(new { message = $"Услуга с ID {item.ServiceId} не найдена" });
                    }

                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ServiceId = service.ServiceId,
                        Quantity = item.Quantity,
                        UnitPrice = service.Price
                    };

                    orderItems.Add(orderItem);
                    totalAmount += service.Price * item.Quantity;
                }

                await _context.OrderItems.AddRangeAsync(orderItems);
                
                order.TotalAmount = totalAmount;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Заказ {order.OrderId} успешно создан. Общая сумма: {totalAmount}");

                return Ok(new
                {
                    orderId = order.OrderId,
                    totalAmount = order.TotalAmount,
                    status = order.Status,
                    message = "Заказ успешно создан"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании заказа");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера: " + ex.Message });
            }
        }

        // Получение списка заказов пользователя
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetUserOrders()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "ID пользователя не найден" });
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(new { message = "Некорректный ID пользователя" });
                }

                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Service)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка заказов");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }

    public class CreateOrderRequest
    {
        public List<CreateOrderItemRequest> Items { get; set; }
    }

    public class CreateOrderItemRequest
    {
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
    }
} 