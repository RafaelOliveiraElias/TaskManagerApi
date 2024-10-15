using System;


namespace TaskManagerApi.Models
{
    public class Tarefa
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; } = false;
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Foreign key
        public int UserId { get; set; }
        public Usuario User { get; set; }
    }
}
