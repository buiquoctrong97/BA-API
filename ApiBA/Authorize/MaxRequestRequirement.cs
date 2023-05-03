using System;
using ApiBA.Authorize;
using ApiBA.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ApiBA.Services;

namespace ApiBA.Authorize
{
	public class MaxRequestRequirement : IAuthorizationRequirement
    {
		
	}
    public class MaxRequestHandler : AuthorizationHandler<MaxRequestRequirement>
    {
        public MaxRequestHandler(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager, IRequestLogsService requestLogsService)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userManager = userManager;
            _requestLogsService = requestLogsService;
        }

        private IHttpContextAccessor HttpContextAccessor { get; }
        private HttpContext HttpContext => HttpContextAccessor.HttpContext;
        private readonly UserManager<User> _userManager;
        private IRequestLogsService _requestLogsService;

        protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context, MaxRequestRequirement requirement)
        {
            var userName = context.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            var totalRequest = await _requestLogsService.GetTotalRequestTodayAsync(userName);
            if (!user.MaxPayment.HasValue)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            if(totalRequest > user.MaxRequest.Value)
            {
                context.Fail();
            }
            else
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}

