﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using ApiBA.Data;
using ApiBA.Options;
using ApiBA.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;

using ApiBA.Models.Booking;
using Newtonsoft.Json;
using ApiBA.Models;

namespace ApiBA.Repositories
{
    public class LoginConfigService : ILoginConfigService
    {
        private BaseDBContext _dbContext;
        private ApiLoginOption _apiLoginOption;
        public LoginConfigService(BaseDBContext dBContext, IOptionsSnapshot<ApiLoginOption> options)
        {
            _dbContext = dBContext;
            _apiLoginOption = options.Value;
        }

        public async Task<string> GetTokenAsync()
        {
            var login = _dbContext.LoginConfig.FirstOrDefault();
            if (login == null)
            {
                //create
                var loginNew = new LoginConfig
                {
                    UserName = _apiLoginOption.UserName,
                    Email = _apiLoginOption.Email,
                    ClientId = _apiLoginOption.ClientSecret,
                    ClientSecret = _apiLoginOption.ClientSecret,
                    Password = _apiLoginOption.Password
                };
                var token = await LoginApiAsync(_apiLoginOption.Email, _apiLoginOption.Password, _apiLoginOption.ClientId, _apiLoginOption.ClientSecret, _apiLoginOption.ApiUrl);
                loginNew.Token = token;
                //get token api

                await _dbContext.LoginConfig.AddAsync(loginNew);
                await _dbContext.SaveChangesAsync();

                return token;
            }
            else
            {
                if (string.IsNullOrEmpty(login.Token))
                {
                    var token = await LoginApiAsync(login.UserName, login.Password, login.ClientId, login.ClientSecret, _apiLoginOption.ApiUrl);
                    login.Token = token;
                    _dbContext.LoginConfig.Update(login);
                    await _dbContext.SaveChangesAsync();
                    return token;
                }
                else
                {
                    return login.Token;
                }
            }
        }

        private async Task<string> LoginApiAsync(string userName, string password, string clientId, string clientSecret, string url)
        {
            HttpClient client = new HttpClient();
            var login = new Login(userName, password, clientId, clientSecret);
            //var test = new LoginModel
            //{
            //    UserName = userName,
            //    Password = password
            //};
            string json = JsonConvert.SerializeObject(login);

            StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, httpContent);

            var result = await response.Content.ReadAsStringAsync();
            try
            {
                var value = JsonConvert.DeserializeObject<ResultLogin>(result);
                return value.token;
            }
            catch
            {
                return "";
            }
        }

        public async Task<string> RefreshTokenAsync()
        {
            var login = _dbContext.LoginConfig.FirstOrDefault();
            if (login == null)
            {
                //create
                var loginNew = new LoginConfig
                {
                    UserName = _apiLoginOption.UserName,
                    Email = _apiLoginOption.Email,
                    ClientId = _apiLoginOption.ClientSecret,
                    ClientSecret = _apiLoginOption.ClientSecret,
                    Password = _apiLoginOption.Password
                };
                var token = await LoginApiAsync(_apiLoginOption.Email, _apiLoginOption.Password, _apiLoginOption.ClientId, _apiLoginOption.ClientSecret, _apiLoginOption.ApiUrl);
                loginNew.Token = token;
                //get token api

                await _dbContext.LoginConfig.AddAsync(loginNew);
                await _dbContext.SaveChangesAsync();

                return token;
            }
            else
            {

                var token = await LoginApiAsync(login.Email, login.Password, login.ClientId, login.ClientSecret, _apiLoginOption.ApiUrl);
                login.Token = token;
                _dbContext.LoginConfig.Update(login);
                await _dbContext.SaveChangesAsync();
                return token;

            }
        }
    }
}

