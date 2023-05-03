using System;
namespace ApiBA.Services
{
	public interface ILoginConfigService
	{
        Task<string> GetTokenAsync();
        Task<string> RefreshTokenAsync();
    }
}

