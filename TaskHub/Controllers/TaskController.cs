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
}
