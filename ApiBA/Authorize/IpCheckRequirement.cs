using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ApiBA.Data;
using Microsoft.AspNetCore.Identity;

namespace ApiBA.Authorize
{
    public class IpCheckRequirement : IAuthorizationRequirement
    {
        public bool IpClaimRequired { get; set; } = true;
    }

    public class IpCheckHandler : AuthorizationHandler<IpCheckRequirement>
    {
        public IpCheckHandler(IHttpContextAccessor httpContextAccessor,UserManager<User> userManager)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userManager = userManager;
        }

        private IHttpContextAccessor HttpContextAccessor { get; }
        private HttpContext HttpContext => HttpContextAccessor.HttpContext;
        private readonly UserManager<User> _userManager;

        protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context, IpCheckRequirement requirement)
        {
            Claim ipClaim = context.User.FindFirst(claim => claim.Type == "ipaddress");
            var userName = context.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            var user = await _userManager.FindByNameAsync(userName);
            if(user == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var listIpAddress = (await _userManager.GetClaimsAsync(user))
                .Where(a => a.Type == "ipaddress")
                .Select(a => a.Value)
                .ToList();
            // No claim existing set and and its configured as optional so skip the check
            if (ipClaim == null && !requirement.IpClaimRequired)
            {
                // Optional claims (IsClaimRequired=false and no "ipaddress" in the claims principal) won't call context.Fail()
                // This allows next Handle to succeed. If we call Fail() the access will be denied, even if handlers
                // evaluated after this one do succeed
                return Task.CompletedTask;
            }

            if (listIpAddress.Contains(ipClaim?.Value))
            {
                context.Succeed(requirement);
            }
            else
            {
                // Only call fail, to guarantee a failure, even if further handlers may succeed
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}

