using System;
using System.ComponentModel.DataAnnotations;

namespace TodoClient.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название задачи")]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Укажите приоритет")]
        [Range(1, 5, ErrorMessage = "Приоритет должен быть от 1 до 5")]
        [Display(Name = "Приоритет")]
        public int Priority { get; set; }

        [Required(ErrorMessage = "Укажите дедлайн")]
        [Display(Name = "Дедлайн")]
        public DateTime Deadline { get; set; }

        [Display(Name = "Выполнено")]
        public bool IsCompleted { get; set; }
    }
} 