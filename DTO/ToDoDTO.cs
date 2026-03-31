namespace Backend.DTO
{
    public class CreateToDoDTO
    {
        public string Title { get; set; }
        public string Details { get; set; }
        public string Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsPublic { get; set; }
    }

    public class UpdateToDoDTO
    {
        public string Title { get; set; }
        public string Details { get; set; }
        public string Priority { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPublic { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class SetCompletedToDoDTO
    {
        public bool IsCompleted { get; set; }
    }
}