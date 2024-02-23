using System;
using ApiBA.Models;
using ApiBA.Repositories;
using ApiBA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIController : ControllerBase
	{
        private readonly ITokenWebService _tokenWebService;
        public APIController(ITokenWebService tokenWebService)
        {
            _tokenWebService = tokenWebService;
        }
        [HttpPost("CreateOrUpdateToken")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateOrUpdateTokenAsync(string name,string token)
        {
            if(string.IsNullOrEmpty(name) && string.IsNullOrEmpty(token))
            {
                return Ok();
            }
            var result = await _tokenWebService.CreateOrUpdate(name, token);
            return Ok(result);
        }
        [HttpGet("GetToken")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTokenAsync(string name)
        {
            var token = await _tokenWebService.GetTokenAsync(name);
            return Content(token);
        }
    }
}

