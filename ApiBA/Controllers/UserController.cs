using System;
using ApiBA.Models;
using ApiBA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
	{
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterAsync(RegisterModel model)
        {
            var result = await _userService.RegisterAsync(model);
            return Ok(result);
        }
        [HttpPost("login")]
        
        public async Task<IActionResult> GetTokenAsync(TokenRequestModel model)
        {
            var result = await _userService.GetTokenAsync(model);
            return Ok(result);
        }
        [HttpPost("addrole")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddRoleAsync(AddRoleModel model)
        {
            var result = await _userService.AddRoleAsync(model);
            return Ok(result);
        }
        [HttpPost("addroles")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddRolesAsync(AddRolesModel model)
        {
            var result = await _userService.AddRolesAsync(model);
            return Ok(result);
        }

        [HttpPost("removeroles")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemovRolesAsync(AddRolesModel model)
        {
            var result = await _userService.RemoveRolesAsync(model);
            return Ok(result);
        }
        [HttpPost("set_max_payment")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SetMaxPaymentAsync(MaxPaymentModel model)
        {
            var result = await _userService.SetMaxPaymentUserAsync(model);
            return Ok(result);
        }
        [HttpPost("set_max_request")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SetMaxRequestAsync(MaxRequestModel model)
        {
            var result = await _userService.SetMaxRequestUserAsync(model);
            return Ok(result);
        }
        [HttpPost("add_ipaddress")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddClaimsAsync(IpAddressModel model)
        {
            var claimModel = new ClaimsModel
            {
                UserName = model.UserName,
                ClaimType = "ipaddress",
                ClaimValues = model.IpAddress
            };
            var result = await _userService.AddClaimsAsync(claimModel);
            return Ok(result);
        }

        [HttpPost("remove_ipaddress")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemovClaimsAsync(IpAddressModel model)
        {
            var claimModel = new ClaimsModel
            {
                UserName = model.UserName,
                ClaimType = "ipaddress",
                ClaimValues = model.IpAddress
            };
            var result = await _userService.RemoveClaimsAsync(claimModel);
            return Ok(result);
        }



    }
}

