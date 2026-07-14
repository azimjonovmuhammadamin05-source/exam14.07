using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TasksController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetMyTasks()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tasks = await _context.TaskItems.Where(t => t.OwnerId == userId).ToListAsync();
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TaskItem task)
    {
        task.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();
        return Ok(task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TaskItem task)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existingTask = await _context.TaskItems.FindAsync(id);

        if (existingTask == null) return NotFound();
        if (existingTask.OwnerId != userId) return Forbid();

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.IsCompleted = task.IsCompleted;

        _context.TaskItems.Update(existingTask);
        await _context.SaveChangesAsync();
        return Ok(existingTask);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var task = await _context.TaskItems.FindAsync(id);

        if (task == null) return NotFound();
        if (task.OwnerId != userId) return Forbid();

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();
        return Ok("Task deleted");
    }
}
