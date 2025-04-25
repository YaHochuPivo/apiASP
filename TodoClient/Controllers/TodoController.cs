using Microsoft.AspNetCore.Mvc;
using TodoClient.Models;
using TodoClient.Services;

namespace TodoClient.Controllers
{
    public class TodoController : Controller
    {
        private readonly TodoService _todoService;

        public TodoController(TodoService todoService)
        {
            _todoService = todoService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _todoService.GetTodoItemsAsync();
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new TodoItem { Deadline = DateTime.Now.Date.AddDays(1) });
        }

        [HttpPost]
        public async Task<IActionResult> Create(TodoItem todoItem)
        {
            if (ModelState.IsValid)
            {
                var result = await _todoService.CreateTodoItemAsync(todoItem);
                if (result)
                {
                    TempData["Success"] = "Задача успешно создана";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Не удалось создать задачу");
            }
            return View(todoItem);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _todoService.GetTodoItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _todoService.UpdateTodoItemAsync(id, todoItem);
                if (result)
                {
                    TempData["Success"] = "Задача успешно обновлена";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Не удалось обновить задачу");
            }
            return View(todoItem);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _todoService.GetTodoItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _todoService.DeleteTodoItemAsync(id);
            if (result)
            {
                TempData["Success"] = "Задача успешно удалена";
            }
            else
            {
                TempData["Error"] = "Не удалось удалить задачу";
            }
            return RedirectToAction(nameof(Index));
        }
    }
} 