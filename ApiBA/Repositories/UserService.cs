using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using ApiBA.Data;
using ApiBA.Models;
using ApiBA.Options;
using ApiBA.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace ApiBA.Repositories
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        private IHttpContextAccessor HttpContextAccessor { get; }
        private HttpContext HttpContext => HttpContextAccessor.HttpContext;
        public UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IOptions<JWT> jwt, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt.Value;
            HttpContextAccessor = httpContextAccessor;
        }
        public async Task<(OperationResult state, User value)> RegisterAsync(RegisterModel model)
        {
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
            };
            var userWithSameEmail = await _userManager.FindByNameAsync(model.UserName);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Options.Authorization.default_role.ToString());
                    return (OperationResult.Success, user);
                }
                else
                {
                    return (OperationResult.Failed(null, result.Errors.FirstOrDefault()?.Description), null);
                }
            }
            else
            {
                return (OperationResult.Failed(null, $"User {user.Email} is already registered."), null);
            }
        }
        public async Task<AuthenticationModel> GetTokenAsync(TokenRequestModel model)
        {
            var authenticationModel = new AuthenticationModel();
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                //authenticationModel.IsAuthenticated = false;
                authenticationModel.Message = $"No Accounts Registered with {model.UserName}.";
                return authenticationModel;
            }
            var userClaims = (await _userManager.GetClaimsAsync(user)).Where(a => a.Type == "ipaddress");
            if(userClaims == null || userClaims.Count() == 0 || !userClaims.Select(a => a.Value).Any(a => HttpContext.Connection.RemoteIpAddress.ToString() == a))
            {
                authenticationModel.Message = $"Your IP address is not allowed";
                return authenticationModel;
            }
            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                //authenticationModel.IsAuthenticated = true;
                JwtSecurityToken jwtSecurityToken = await CreateJwtToken(user);
                authenticationModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                authenticationModel.Expires = new DateTimeOffset(jwtSecurityToken.ValidTo).ToUnixTimeSeconds();
                //authenticationModel.Email = user.Email;
                //authenticationModel.UserName = user.UserName;
                //var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
                //authenticationModel.Roles = rolesList.ToList();
                //authenticationModel.Claims = jwtSecurityToken.Claims.Where(a => a.Type == "ipaddress").ToList();
                return authenticationModel;
            }
            //authenticationModel.IsAuthenticated = false;
            authenticationModel.Message = $"Incorrect Credentials for user {user.UserName}.";
            return authenticationModel;
        }
        private async Task<JwtSecurityToken> CreateJwtToken(User user)
        {
            //var userClaims = (await _userManager.GetClaimsAsync(user)).Where(a => a.Type != "ipaddress");
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();
            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id),
                new Claim("ipaddress", HttpContext.Connection.RemoteIpAddress.ToString())
            }
            //.Union(userClaims)
            .Union(roleClaims);
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }

        public async Task<OperationResult> AddRoleAsync(AddRoleModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return OperationResult.Failed(null, $"No Accounts Registered with {model.UserName}.");
            }

            var roleExists = Enum.GetNames(typeof(Options.Authorization.Roles)).Any(x => x.ToLower() == model.Role.ToLower());
            if (roleExists)
            {
                var validRole = Enum.GetValues(typeof(Options.Authorization.Roles)).Cast<Options.Authorization.Roles>().Where(x => x.ToString().ToLower() == model.Role.ToLower()).FirstOrDefault();
                var state = await _userManager.AddToRoleAsync(user, validRole.ToString());
                if (state.Succeeded)
                {
                    return OperationResult.Success;
                }
                return OperationResult.Failed(null, string.Join(", ", state.Errors.Select(e => e.Description)));
            }
            return OperationResult.Failed(null, $"Role {model.Role} not found.");


        }

        public async Task<OperationResult> AddRolesAsync(AddRolesModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return OperationResult.Failed(null, $"No Accounts Registered with {model.UserName}.");
            }
            if (model.Roles != null && model.Roles.Count() > 0)
            {

                //var roleExists = Enum.GetNames(typeof(Authorization.Roles)).Any(x => x.ToLower() == role.ToLower());
                //if (roleExists)
                //{
                //    var validRole = Enum.GetValues(typeof(Authorization.Roles)).Cast<Authorization.Roles>().Where(x => x.ToString().ToLower() == role.ToLower()).FirstOrDefault();
                //    var state = await _userManager.AddToRoleAsync(user, validRole.ToString());
                //    return new IdentityResult() { Succeeded = true} $"Added {role} to user {model.UserName}.";
                //}
                var state = await _userManager.AddToRolesAsync(user, model.Roles);
                if (state.Succeeded)
                {
                    return OperationResult.Success;
                }
                return OperationResult.Failed(null, string.Join(", ", state.Errors.Select(e => e.Description)));

            }
            return OperationResult.Failed(null, $"Incorrect Credentials for user {user.UserName}.");
        }

        public async Task<OperationResult> RemoveRolesAsync(AddRolesModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return OperationResult.Failed(null, $"No Accounts Registered with {model.UserName}.");
            }
            if (model.Roles != null && model.Roles.Count() > 0)
            {
                var state = await _userManager.RemoveFromRolesAsync(user, model.Roles);
                if (state.Succeeded)
                {
                    return OperationResult.Success;
                }
                return OperationResult.Failed(null, string.Join(", ", state.Errors.Select(e => e.Description)));

            }
            return OperationResult.Failed(null, $"Incorrect Credentials for user {user.UserName}.");
        }

        public async Task<OperationResult> SetMaxPaymentUserAsync(MaxPaymentModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    return OperationResult.Failed(null, $"No Accounts Registered with {model.UserName}.");
                }
                user.MaxPayment = model.MaxPayment;
                var state = await _userManager.UpdateAsync(user);
                if (state.Succeeded)
                {
                    return OperationResult.Success;
                }
                return OperationResult.Failed(null, string.Join(", ", state.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                return OperationResult.Failed(ex);
            }
        }
        public async Task<User> GetUserAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }
        public async Task<OperationResult> ChangePasswordAsync(ChangePasswordModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    return OperationResult.Failed(null, $"No Accounts Registered with {model.UserName}.");
                }
                if (await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
                {
                    var state = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    if (state.Succeeded)
                    {
                        return OperationResult.Success;
                    }
                    return OperationResult.Failed(null, string.Join(", ", state.Errors.Select(e => e.Description)));
                }
                return OperationResult.Failed(null, $"Incorrect password");
            }
            catch (Exception ex)
            {
                return OperationResult.Failed(ex);
            }
        }

        public async Task<OperationResult> ResetPasswordAsync(ResetPasswordModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    return OperationResult.Failed(null, $"No Accounts Registered with {model.UserName}.");
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var state = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (state.Succeeded)
                {
                    return OperationResult.Success;
                }
                return OperationResult.Failed(null, string.Join(", ", state.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                return OperationResult.Failed(ex);
            }
        }

        public async Task<OperationResult> AddClaimsAsync(ClaimsModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return OperationResult.Failed(null, $"No Accounts Registered with {model.UserName}.");
            }
            if (model.ClaimValues != null && model.ClaimValues.Count() > 0)
            {
                var claims = new List<Claim>();
                foreach(var item in model.ClaimValues)
                {
                    claims.Add(new Claim(model.ClaimType, item));
                }
                var state = await _userManager.AddClaimsAsync(user, claims);
                if (state.Succeeded)
                {
                    return OperationResult.Success;
                }
                return OperationResult.Failed(null, string.Join(", ", state.Errors.Select(e => e.Description)));

            }
            return OperationResult.Failed(null, $"Incorrect Credentials for user {user.UserName}.");
        }

        public async Task<OperationResult> RemoveClaimsAsync(ClaimsModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return OperationResult.Failed(null, $"No Accounts Registered with {model.UserName}.");
            }
            if (model.ClaimValues != null && model.ClaimValues.Count() > 0)
            {
                var claims = new List<Claim>();
                foreach (var item in model.ClaimValues)
                {
                    claims.Add(new Claim(model.ClaimType, item));
                }
                var state = await _userManager.RemoveClaimsAsync(user, claims);
                if (state.Succeeded)
                {
                    return OperationResult.Success;
                }
                return OperationResult.Failed(null, string.Join(", ", state.Errors.Select(e => e.Description)));

            }
            return OperationResult.Failed(null, $"Incorrect Credentials for user {user.UserName}.");
        }

        public async Task<OperationResult> SetMaxRequestUserAsync(MaxRequestModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    return OperationResult.Failed(null, $"No Accounts Registered with {model.UserName}.");
                }
                user.MaxRequest = model.MaxRequest;
                var state = await _userManager.UpdateAsync(user);
                if (state.Succeeded)
                {
                    return OperationResult.Success;
                }
                return OperationResult.Failed(null, string.Join(", ", state.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                return OperationResult.Failed(ex);
            }
        }
    }
}

