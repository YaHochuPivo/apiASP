using System.Net.Http.Json;
using TodoClient.Models;

namespace TodoClient.Services
{
    public class TodoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7164/api/Todo";
        private readonly ILogger<TodoService> _logger;

        public TodoService(HttpClient httpClient, ILogger<TodoService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<TodoItem>> GetTodoItemsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<TodoItem>>() ?? new List<TodoItem>();
                }
                _logger.LogError($"Error getting todo items: {response.StatusCode}");
                return new List<TodoItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting todo items");
                return new List<TodoItem>();
            }
        }

        public async Task<TodoItem?> GetTodoItemAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TodoItem>();
                }
                _logger.LogError($"Error getting todo item {id}: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting todo item {id}");
                return null;
            }
        }

        public async Task<bool> CreateTodoItemAsync(TodoItem todoItem)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_baseUrl, todoItem);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error creating todo item: {response.StatusCode}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating todo item");
                return false;
            }
        }

        public async Task<bool> UpdateTodoItemAsync(int id, TodoItem todoItem)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{id}", todoItem);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error updating todo item {id}: {response.StatusCode}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating todo item {id}");
                return false;
            }
        }

        public async Task<bool> DeleteTodoItemAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error deleting todo item {id}: {response.StatusCode}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting todo item {id}");
                return false;
            }
        }
    }
} 