using System;
using ApiBA.Data;
using ApiBA.Models;
using Microsoft.AspNetCore.Identity;

namespace ApiBA.Services
{
	public interface IUserService
	{

        Task<(OperationResult state,User value)> RegisterAsync(RegisterModel model);
        Task<AuthenticationModel> GetTokenAsync(TokenRequestModel model);
        Task<OperationResult> AddRoleAsync(AddRoleModel model);
        Task<OperationResult> AddRolesAsync(AddRolesModel model);
        Task<OperationResult> RemoveRolesAsync(AddRolesModel model);
        Task<OperationResult> SetMaxPaymentUserAsync(MaxPaymentModel model);
        Task<OperationResult> SetMaxRequestUserAsync(MaxRequestModel model);
        Task<User> GetUserAsync(string userName);
        Task<OperationResult> ResetPasswordAsync(ResetPasswordModel model);
        Task<OperationResult> ChangePasswordAsync(ChangePasswordModel model);
        Task<OperationResult> AddClaimsAsync(ClaimsModel model);
        Task<OperationResult> RemoveClaimsAsync(ClaimsModel model);
    }
}

