namespace TaskManagerApi.Models.ViewModels
{
    public class TarefaViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public string Category { get; set; }
    }
}
