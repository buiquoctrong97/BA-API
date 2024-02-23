using System;
using ApiBA.Data;
using ApiBA.Models;

namespace ApiBA.Services
{
	public interface ITokenWebService
	{
        Task<(OperationResult state, TokenWeb value)> CreateOrUpdate(string name, string token);
        Task<string> GetTokenAsync(string name);
    }
}

