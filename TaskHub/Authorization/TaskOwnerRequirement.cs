using Microsoft.AspNetCore.Authorization;

namespace TaskHub.Authorization
{
    public class TaskOwnerRequirement : IAuthorizationRequirement
    {
    }
}