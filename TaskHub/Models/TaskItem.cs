public class TaskItem
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public string? OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }
}
