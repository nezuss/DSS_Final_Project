using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ToDoModel
    {
        [Key]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsPublic { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}