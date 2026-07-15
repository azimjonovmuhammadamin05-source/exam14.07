using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(string email, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return BadRequest("User not found.");

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Any())
            await _userManager.RemoveFromRolesAsync(user, roles);

        var result = await _userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new
        {
            Message = $"Role '{role}' assigned successfully.",
            User = email
        });
    }

    [HttpPost("add-claim")]
    public async Task<IActionResult> AddClaim(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return BadRequest("User not found.");

        var claim = new Claim("CanEditOwnTask", "true");

        var result = await _userManager.AddClaimAsync(user, claim);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Claim added successfully.");
    }
}