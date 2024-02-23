using System;
using ApiBA.Data;
using ApiBA.Models;
using ApiBA.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace ApiBA.Repositories
{
	public class TokenWebService : ITokenWebService
    {
        private BaseDBContext _dBContext;
        public TokenWebService(BaseDBContext dBContext)
		{
            _dBContext = dBContext;
		}

        public async Task<(OperationResult state, TokenWeb value)> CreateOrUpdate(string name, string token)
        {
            try
            {
                var tokenItem = _dBContext.TokenWeb.Where(a => a.Name == name).FirstOrDefault();
                if (tokenItem != null)
                {
                    tokenItem.Token = token;
                    tokenItem.LastModified = DateTimeOffset.Now;
                    var update = _dBContext.TokenWeb.Update(tokenItem);
                    await _dBContext.SaveChangesAsync();
                    return (OperationResult.Success, tokenItem);
                }
                else {
                    var newToken = new TokenWeb
                    {
                        Name = name,
                        Token = token
                    };
                    var value = await _dBContext.TokenWeb.AddAsync(newToken);
                    await _dBContext.SaveChangesAsync();
                    return (OperationResult.Success, value.Entity);
                }
                
            }
            catch (Exception ex)
            {
                return (OperationResult.Failed(ex), null);
            }
        }

        public async Task<string> GetTokenAsync(string name)
        {
            var tokenItem = await _dBContext.TokenWeb.Where(a => a.Name == name).FirstOrDefaultAsync();
            return tokenItem?.Token;
        }
    }
}

