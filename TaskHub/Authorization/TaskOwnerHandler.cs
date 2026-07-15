using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TaskHub.Authorization
{
    public class TaskOwnerHandler : AuthorizationHandler<TaskOwnerRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TaskOwnerRequirement requirement)
        {
            if (context.User.IsInRole("Manager"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (context.User.HasClaim(c => c.Type == "CanEditOwnTask"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}