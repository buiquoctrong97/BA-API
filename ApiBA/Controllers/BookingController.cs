using System;
using Microsoft.AspNetCore.Mvc;
using ApiBA.Services;
using ApiBA.Options;
using Microsoft.Extensions.Options;
using ApiBA.Models.Booking;
using System.Security.Claims;
using Newtonsoft.Json;
using ApiBA.Data;
using Microsoft.AspNetCore.Authorization;
using ApiBA.Models;
using System.Collections.Generic;
using Azure;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

namespace ApiBA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
	
    public class BookingController : ControllerBase
	{
		private ILoginConfigService _loginConfigService;
		private IRequestLogsService _requestLogsService;
		private readonly ApiUrlOption _apiUrlOption;
        private readonly ApiLoginOption _apiLoginOption;
        private IHttpContextAccessor _httpContextAccessor;
        private IUserService _userService;
        public BookingController(ILoginConfigService loginConfigService,
			IRequestLogsService requestLogsService,
			IOptionsSnapshot<ApiUrlOption> optionsSnapshot,
            IOptionsSnapshot<ApiLoginOption> apiLoginOption,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService
            )
		{
			_loginConfigService = loginConfigService;
			_requestLogsService = requestLogsService;
			_apiUrlOption = optionsSnapshot.Value;
            _apiLoginOption = apiLoginOption.Value;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
		}
		[HttpPost("air_availability")]
        [Authorize(Roles = "AirAvailabilityTransit")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        //[AllowAnonymous]
        public async Task<IActionResult> AirAvailabilityTransit(AirAvailabilityTransit model)
		{
			var userName = _httpContextAccessor.HttpContext?.User.Claims
				.FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            var url = _apiUrlOption.AirAvailabilityTransit;
            string json = JsonConvert.SerializeObject(model);
            //add log
            var log = new RequestLogs
			{
				ApiUrl = url,
				CreatedUser = userName,
				Parameters = json,
				CreatedDate = DateTimeOffset.Now
			};

			
			var token = await _loginConfigService.GetTokenAsync();

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("x-api-key", _apiLoginOption.ClientId);
                request.Headers.Add("x-api-secret", _apiLoginOption.ClientSecret);
                request.Headers.Add("Authorization", "Bearer " + token);
                var content = new StringContent(json, null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _loginConfigService.RefreshTokenAsync();
                    using (var client1 = new HttpClient())
                    {
                        
                    var request1 = new HttpRequestMessage(HttpMethod.Post, url);
                    request1.Headers.Add("x-api-key", _apiLoginOption.ClientId);
                    request1.Headers.Add("x-api-secret", _apiLoginOption.ClientSecret);
                    request1.Headers.Add("Authorization", "Bearer " + token);
                    request1.Content = content;
                    var response1 = await client1.SendAsync(request1);

                    var result1 = await response1.Content.ReadAsStringAsync();
                    log.ResponseResult = result1;

                    //create log
                    await _requestLogsService.CreateAsync(log);

                    return Ok(result1);
                        }
                }

                var result = await response.Content.ReadAsStringAsync();
                log.ResponseResult = result;

                await _requestLogsService.CreateAsync(log);

                return Ok(result);
            }

        }

        [HttpPost("create_booking_hold_on")]
        [Authorize(Roles = "CreateBooking")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> CreateBooking(CreateBooking model)
        {
            model.pnr_on_hold = true;
            var userName = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            var url = _apiUrlOption.CreateBooking;


            string json = JsonConvert.SerializeObject(model);
            //add log
            var log = new RequestLogs
            {
                ApiUrl = url,
                CreatedUser = userName,
                Parameters = json,
                CreatedDate = DateTimeOffset.Now
            };


            var token = await _loginConfigService.GetTokenAsync();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                client.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);

                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _loginConfigService.RefreshTokenAsync();
                    using (var client1 = new HttpClient())
                    {
                        client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                        client1.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                        client1.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);
                        var response1 = await client1.PostAsync(url, httpContent);
                        var result1 = await response1.Content.ReadAsStringAsync();
                        log.ResponseResult = result1;
                        log.StatusCode = ((int)response1.StatusCode);
                        try
                        {
                            var objectResult = JsonConvert.DeserializeObject<ApiResult>(result1);
                            var data = objectResult.data;
                            string pnrNubmer = null;
                            if (data != null)
                            {
                                pnrNubmer = data.ContainsKey("pnr_number") ? data["pnr_number"].ToString() : null;
                            }
                            log.PnrNumber = pnrNubmer;
                        }
                        catch
                        {

                        }
                        //create log
                        await _requestLogsService.CreateAsync(log);

                        return Ok(result1);
                    }
                }
                else
                {

                    var result = await response.Content.ReadAsStringAsync();
                    log.ResponseResult = result;
                    log.StatusCode = ((int)response.StatusCode);
                    try
                    {
                        var objectResult = JsonConvert.DeserializeObject<ApiResult>(result);
                        var data = objectResult.data;
                        string pnrNubmer = null;
                        if (data != null)
                        {
                            pnrNubmer = data.ContainsKey("pnr_number") ? data["pnr_number"].ToString() : null;
                        }
                        log.PnrNumber = pnrNubmer;
                    }
                    catch
                    {

                    }
                    await _requestLogsService.CreateAsync(log);

                    return Ok(result);
                }
            }

        }


        [HttpPost("create_booking_paynow")]
        [Authorize(Roles = "CreateBookingPaynow")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> CreateBookingPaynow(CreateBooking model)
        {
            //model.pnr_on_hold_indicator = false;
            var userName = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            var url = _apiUrlOption.CreateBooking;
            string json = JsonConvert.SerializeObject(model);


            //check max payment
            //var paymentTotal = model.guest_payment_info.Sum(a => a.payment_amount);
            //var user = await _userService.GetUserAsync(userName);
            //var maxPayment = user?.MaxPayment ?? 0;
            //if (paymentTotal > maxPayment)
            //{
            //    return Ok($"the amount of payment exceeds the allowable limit");
            //}
            //add log
            var log = new RequestLogs
            {
                ApiUrl = url,
                CreatedUser = userName,
                Parameters = json,
                CreatedDate = DateTimeOffset.Now
            };


            var token = await _loginConfigService.GetTokenAsync();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                client.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);

                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _loginConfigService.RefreshTokenAsync();
                    using (var client1 = new HttpClient())
                    {
                        client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                        client1.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                        client1.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);
                        var response1 = await client1.PostAsync(url, httpContent);
                        var result1 = await response1.Content.ReadAsStringAsync();
                        log.ResponseResult = result1;
                        try
                        {
                            var objectResult = JsonConvert.DeserializeObject<ApiResult>(result1);
                            var data = objectResult.data;
                            string pnrNubmer = null;
                            if (data != null)
                            {
                                pnrNubmer = data.ContainsKey("pnr_number") ? data["pnr_number"].ToString() : null;
                            }
                            log.PnrNumber = pnrNubmer;
                        }
                        catch
                        {

                        }
                        //create log
                        await _requestLogsService.CreateAsync(log);

                        return Ok(result1);
                    }
                }
                else
                {

                    var result = await response.Content.ReadAsStringAsync();
                    log.ResponseResult = result;
                    try
                    {
                        var objectResult = JsonConvert.DeserializeObject<ApiResult>(result);
                        var data = objectResult.data;
                        string pnrNubmer = null;
                        if (data != null)
                        {
                            pnrNubmer = data.ContainsKey("pnr_number") ? data["pnr_number"].ToString() : null;
                        }
                        log.PnrNumber = pnrNubmer;
                    }
                    catch
                    {

                    }
                    await _requestLogsService.CreateAsync(log);

                    return Ok(result);
                }
            }
        }

        private async Task<string> CreateBookingHoldOnAsync(CreateBooking model)
        {
            model.pnr_on_hold = true;
            var userName = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            var url = _apiUrlOption.CreateBooking;


            string json = JsonConvert.SerializeObject(model);
            //add log
            var log = new RequestLogs
            {
                ApiUrl = url,
                CreatedUser = userName,
                Parameters = json,
                CreatedDate = DateTimeOffset.Now
            };


            var token = await _loginConfigService.GetTokenAsync();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                client.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);

                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _loginConfigService.RefreshTokenAsync();
                    using (var client1 = new HttpClient())
                    {
                        client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                        client1.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                        client1.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);
                        var response1 = await client1.PostAsync(url, httpContent);
                        var result1 = await response1.Content.ReadAsStringAsync();
                        log.ResponseResult = result1;
                        log.StatusCode = ((int)response1.StatusCode);
                        try
                        {
                            var objectResult = JsonConvert.DeserializeObject<ApiResult>(result1);
                            var data = objectResult.data;
                            string pnrNubmer = null;
                            if (data != null)
                            {
                                pnrNubmer = data.ContainsKey("pnr_number") ? data["pnr_number"].ToString() : null;
                            }
                            log.PnrNumber = pnrNubmer;
                        }
                        catch
                        {

                        }
                        //create log
                        await _requestLogsService.CreateAsync(log);

                        return result1;
                    }
                }
                else
                {

                    var result = await response.Content.ReadAsStringAsync();
                    log.ResponseResult = result;
                    log.StatusCode = ((int)response.StatusCode);
                    try
                    {
                        var objectResult = JsonConvert.DeserializeObject<ApiResult>(result);
                        var data = objectResult.data;
                        string pnrNubmer = null;
                        if (data != null)
                        {
                            pnrNubmer = data.ContainsKey("pnr_number") ? data["pnr_number"].ToString() : null;
                        }
                        log.PnrNumber = pnrNubmer;
                    }
                    catch
                    {

                    }
                    await _requestLogsService.CreateAsync(log);

                    return result;
                }
            }
        }

        [HttpPost("Confirm_Price_create_booking")]
        [Authorize(Roles = "CreateBooking")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> ConfirmPrice(ConfirmPrice model)
        {
            var responseCreate = "";
            var userName = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            var url = _apiUrlOption.ConfirmPrice;
            string json = JsonConvert.SerializeObject(model);
            //add log
            var log = new RequestLogs
            {
                ApiUrl = url,
                CreatedUser = userName,
                Parameters = json,
                CreatedDate = DateTimeOffset.Now
            };


            var token = await _loginConfigService.GetTokenAsync();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                client.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);

                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _loginConfigService.RefreshTokenAsync();
                    using (var client1 = new HttpClient())
                    {
                        client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                        client1.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                        client1.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);
                        var response1 = await client1.PostAsync(url, httpContent);
                        var result1 = await response1.Content.ReadAsStringAsync();
                        log.StatusCode = ((int)response1.StatusCode);
                        if (result1 != null)
                        {
                            try
                            {
                                var objectResult = JsonConvert.DeserializeObject<ApiResult>(result1);
                                var data = objectResult.data;

                                if (data != null)
                                {
                                    var id = data.ContainsKey("id") ? data["id"].ToString() : null;
                                    log.AirBookingId = id;
                                }
                            }
                            catch
                            {

                            }
                        }
                        log.ResponseResult = result1;

                        //create log
                        await _requestLogsService.CreateAsync(log);
                        var modelBooking1 = new CreateBooking
                        {
                            pnr_on_hold = true,
                            air_booking_id = model.air_booking_id

                        };
                        responseCreate = await CreateBookingHoldOnAsync(modelBooking1);
                        return Ok(responseCreate);
                    }
                }

                var result = await response.Content.ReadAsStringAsync();
                log.ResponseResult = result;
                log.StatusCode = ((int)response.StatusCode);
                if (result != null)
                {
                    try
                    {
                        var objectResult = JsonConvert.DeserializeObject<ApiResult>(result);
                        var data = objectResult.data;

                        if (data != null)
                        {
                            var id = data.ContainsKey("id") ? data["id"].ToString() : null;
                            log.AirBookingId = id;
                        }
                    }
                    catch
                    {

                    }
                }
                await _requestLogsService.CreateAsync(log);
                var modelBooking = new CreateBooking
                {
                    pnr_on_hold = true,
                    air_booking_id = model.air_booking_id

                };
                responseCreate = await CreateBookingHoldOnAsync(modelBooking);
                return Ok(responseCreate);
            }
        }
        [HttpPost("pre_confirm_price")]
        [Authorize(Roles = "ConfirmPrice")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> PreConfirmPrice(PreConfirmPrice model)
        {
            var userName = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            var url = _apiUrlOption.PreConfirmPrice;
            string json = JsonConvert.SerializeObject(model);
            //add log
            var log = new RequestLogs
            {
                ApiUrl = url,
                CreatedUser = userName,
                Parameters = json,
                CreatedDate = DateTimeOffset.Now
            };


            var token = await _loginConfigService.GetTokenAsync();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                client.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);

                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _loginConfigService.RefreshTokenAsync();
                    using (var client1 = new HttpClient())
                    {
                        client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                        client1.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                        client1.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);
                        var response1 = await client1.PostAsync(url, httpContent);
                        var result1 = await response1.Content.ReadAsStringAsync();
                        log.ResponseResult = result1;

                        //create log
                        await _requestLogsService.CreateAsync(log);

                        return Ok(result1);
                    }
                }

                var result = await response.Content.ReadAsStringAsync();
                log.ResponseResult = result;

                await _requestLogsService.CreateAsync(log);

                return Ok(result);
            }
        }

        [HttpPost("booking_paynow")]
        [Authorize(Roles = "BookingPaynow")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> BookingPaynow(CreateBooking model)
        {
            model.pnr_on_hold = false;
            var userName = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            var url = _apiUrlOption.BookingPaynow;
            string json = JsonConvert.SerializeObject(model);
            //add log
            var log = new RequestLogs
            {
                ApiUrl = url,
                CreatedUser = userName,
                Parameters = json,
                CreatedDate = DateTimeOffset.Now
            };


            var token = await _loginConfigService.GetTokenAsync();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                client.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);

                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _loginConfigService.RefreshTokenAsync();
                    using (var client1 = new HttpClient())
                    {
                        client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                        client1.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                        client1.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);
                        var response1 = await client1.PostAsync(url, httpContent);
                        var result1 = await response1.Content.ReadAsStringAsync();
                        log.ResponseResult = result1;

                        //create log
                        await _requestLogsService.CreateAsync(log);

                        return Ok(result1);
                    }
                }

                var result = await response.Content.ReadAsStringAsync();
                log.ResponseResult = result;

                await _requestLogsService.CreateAsync(log);

                return Ok(result);
            }

        }

        [HttpPost("list_baggage")]
        [Authorize(Roles = "ListBaggage")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> ListBaggage(ListBaggage model)
        {
            var userName = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            var url = _apiUrlOption.ListBaggage;
            string json = JsonConvert.SerializeObject(model);
            //add log
            var log = new RequestLogs
            {
                ApiUrl = url,
                CreatedUser = userName,
                Parameters = json,
                CreatedDate = DateTimeOffset.Now
            };


            var token = await _loginConfigService.GetTokenAsync();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                client.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);

                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _loginConfigService.RefreshTokenAsync();
                    using (var client1 = new HttpClient())
                    {
                        client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                        client1.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                        client1.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientSecret);
                        var response1 = await client1.PostAsync(url, httpContent);
                        var result1 = await response1.Content.ReadAsStringAsync();
                        log.ResponseResult = result1;

                        //create log
                        await _requestLogsService.CreateAsync(log);

                        return Ok(result1);
                    }
                }

                var result = await response.Content.ReadAsStringAsync();
                log.ResponseResult = result;

                await _requestLogsService.CreateAsync(log);

                return Ok(result);

            }

        }
        [HttpPost("retrieve_booking")]
        [Authorize(Roles = "RetrieveBooking")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> RetrieveBooking(RetrieveBooking model)
        {
            var userName = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;

            var isAdministrator = _httpContextAccessor.HttpContext?.User.Claims
                .Where(a => a.Type == ClaimTypes.Role).Any(a => a.Value == "Administrator") ?? false;
            if (!isAdministrator)
            {
                var hasPnr = await _requestLogsService.HasPnrNumberAsync(model.pnr_number);
                if (!hasPnr)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Message = "Reservation code is not in the system"
                    }));
                }
            }
            var checkAuthor = await _requestLogsService.CheckPnrNumberAsync(userName, model.pnr_number);
            if (checkAuthor || isAdministrator)
            {
                var url = _apiUrlOption.RetrieveBooking;
                string json = JsonConvert.SerializeObject(model);
                //add log
                var log = new RequestLogs
                {
                    ApiUrl = url,
                    CreatedUser = userName,
                    Parameters = json,
                    CreatedDate = DateTimeOffset.Now
                };


                var token = await _loginConfigService.GetTokenAsync();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    client.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                    client.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);

                    StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, httpContent);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        token = await _loginConfigService.RefreshTokenAsync();
                        using (var client1 = new HttpClient())
                        {
                            client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                            client1.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
                            client1.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);
                            var response1 = await client1.PostAsync(url, httpContent);
                            var result1 = await response1.Content.ReadAsStringAsync();
                            log.ResponseResult = result1;

                            //create log
                            await _requestLogsService.CreateAsync(log);

                            return Ok(result1);
                        }
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    log.ResponseResult = result;

                    await _requestLogsService.CreateAsync(log);

                    return Ok(result);
                }
                }
            else
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Message = "You do not have permission to view this pnr_number"
                    }));
                }
            

        }

        [HttpPost("get_data_response")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetDataResponseAsync(ResponseFilter model)
        {
            var data = await _requestLogsService.Query()
                .Where(a => string.IsNullOrEmpty(model.ActionName) || a.ApiUrl.Contains(model.ActionName))
                .Where(a => string.IsNullOrEmpty(model.AirBookingId) || a.AirBookingId == model.AirBookingId)
                .Where(a => string.IsNullOrEmpty(model.PnrNumber) || a.PnrNumber == model.PnrNumber)
                .Where(a => string.IsNullOrEmpty(model.UserName) || a.CreatedUser == model.UserName)
                .OrderByDescending(a => a.CreatedDate)
                .Skip(model.Skip).Take(model.Take)
                .ToListAsync();
            return Ok(data);
        }
        //[HttpGet("test2")]
        //[AllowAnonymous]
        //public async Task<IActionResult> Test2(string token )
        //{
        //    try
        //    {
        //        var client = new HttpClient();
        //        var request = new HttpRequestMessage(HttpMethod.Post, _apiUrlOption.AirAvailabilityTransit);
        //        request.Headers.Add("x-api-key", _apiLoginOption.ClientId);
        //        request.Headers.Add("x-api-secret", _apiLoginOption.ClientSecret);
        //        request.Headers.Add("Authorization", "Bearer " + token);
        //        var content = new StringContent("{\r\n    \"availability_searches\": [\r\n        {\r\n            \"origin\": \"SGN\",\r\n            \"destination\": \"DAD\",\r\n            \"flight_date\": \"2023-05-30\"\r\n        }\r\n    ],\r\n    \"pax_types\": [\r\n        {\r\n            \"type\": \"ADULT\",\r\n            \"count\": 1\r\n        }\r\n    ],\r\n    \"trip_type\":\"OW\",\r\n    \"point_of_purchase\":\"VND\"\r\n}", null, "application/json");
        //        request.Content = content;
        //        var response = await client.SendAsync(request);
        //        //response.EnsureSuccessStatusCode();

        //        var result = await response.Content.ReadAsStringAsync();

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(ex.Message);
        //    }
        //}

        //[HttpPost("test")]
        //[AllowAnonymous]
        //public async Task<IActionResult> Test()
        //{
        //    try
        //    {
        //        HttpClient client = new HttpClient();

        //        var login = new Login(_apiLoginOption.Email, _apiLoginOption.Password, _apiLoginOption.IataCode);
        //        //var test = new LoginModel
        //        //{
        //        //    UserName = userName,
        //        //    Password = password
        //        //};
        //        string json = JsonConvert.SerializeObject(login);

        //        StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        //        client.DefaultRequestHeaders.Add("x-api-key", _apiLoginOption.ClientId);
        //        client.DefaultRequestHeaders.Add("x-api-secret", _apiLoginOption.ClientSecret);
        //        //var request = new HttpRequestMessage(HttpMethod.Post, _apiLoginOption.ApiUrl);
        //        //request.Content = httpContent;
        //        //request.Headers.Add("x-api-key", _apiLoginOption.ClientId);
        //        //request.Headers.Add("x-api-secret", _apiLoginOption.ClientSecret);
        //        var response = await client.PostAsync(_apiLoginOption.ApiUrl, httpContent);

        //        var result = await response.Content.ReadAsStringAsync();
        //        try
        //        {
        //            var data = JsonConvert.DeserializeObject<APIResponse>(result);
        //            var val = JsonConvert.SerializeObject(data.data);
        //            var resultLogin = JsonConvert.DeserializeObject<ResultLogin>(val);
        //            return Ok(resultLogin.access_token);
        //        }
        //        catch (Exception ex)
        //        {
        //            return Ok();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok();
        //    }
        //}
        //[HttpPost("test")]
        //[Authorize(Roles = "CreateBooking")]

        //[Authorize(Policy = "SameIpPolicy")]
        //[Authorize(Policy = "MaxRequest")]
        //public async Task<IActionResult> Test(LoginModel model)
        //{
        //    var userName = _httpContextAccessor.HttpContext?.User.Claims
        //        .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
        //    var url = _apiUrlOption.Test;

        //    string json = JsonConvert.SerializeObject(model);
        //    //add log
        //    var log = new RequestLogs
        //    {
        //        ApiUrl = url,
        //        CreatedUser = userName,
        //        Parameters = json,
        //        CreatedDate = DateTimeOffset.Now
        //    };


        //    var token = await _loginConfigService.GetTokenAsync();
        //    HttpClient client = new HttpClient();
        //    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);


        //    StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        //    var response = await client.PostAsync(url, httpContent);
        //    string pnrNubmer = null;
        //    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        //    {
        //        token = await _loginConfigService.RefreshTokenAsync();
        //        HttpClient client1 = new HttpClient();
        //        client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        //        var response1 = await client1.PostAsync(url, httpContent);
        //        var result1 = await response1.Content.ReadAsStringAsync();
        //        log.ResponseResult = result1;
        //        var objectResult1 = JsonConvert.DeserializeObject<ApiResult>(result1);
        //        var data1 = objectResult1?.data;

        //        if (data1 != null)
        //        {
        //            pnrNubmer = data1.ContainsKey("pnr_number") ? data1["pnr_number"].ToString() : null;
        //        }
        //        log.PnrNumber = pnrNubmer;
        //        //create log
        //        var state1 = await _requestLogsService.CreateAsync(log);

        //        return Ok(result1);
        //    }

        //    var result = await response.Content.ReadAsStringAsync();
        //    log.ResponseResult = result;
        //    var objectResult = JsonConvert.DeserializeObject<ApiResult>(result);
        //    var data = objectResult?.data;
        //    if (data != null)
        //    {
        //        pnrNubmer = data.ContainsKey("pnr_number") ? data["pnr_number"].ToString() : null;
        //    }
        //    log.PnrNumber = pnrNubmer;
        //    var state = await _requestLogsService.CreateAsync(log);

        //    return Ok(result);

        //}

        //[HttpPost("api_test")]
        //[Authorize(Roles = "Administrator")]
        //public async Task<IActionResult> ApiTest(LoginModel model)
        //{

        //    var url = _apiUrlOption.Test;

        //    string json = JsonConvert.SerializeObject(model);
        //    var result = _apiUrlOption.Data;
        //    return Ok(result);

        //}
    }
}

