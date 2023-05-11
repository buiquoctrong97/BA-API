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

namespace ApiBA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
	
    public class BookingController : ControllerBase
	{
		private ILoginConfigService _loginConfigService;
		private IRequestLogsService _requestLogsService;
		private readonly ApiUrlOption _apiUrlOption;
		private IHttpContextAccessor _httpContextAccessor;
        private IUserService _userService;
        public BookingController(ILoginConfigService loginConfigService,
			IRequestLogsService requestLogsService,
			IOptionsSnapshot<ApiUrlOption> optionsSnapshot,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService
            )
		{
			_loginConfigService = loginConfigService;
			_requestLogsService = requestLogsService;
			_apiUrlOption = optionsSnapshot.Value;
			_httpContextAccessor = httpContextAccessor;
            _userService = userService;
		}
		[HttpPost("air_availability_transit")]
        [Authorize(Roles = "AirAvailabilityTransit")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
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
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            

            StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, httpContent);

			if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
			{
                token = await _loginConfigService.RefreshTokenAsync();
                HttpClient client1 = new HttpClient();
                client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response1 = await client1.PostAsync(url, httpContent);
                var result1 = await response1.Content.ReadAsStringAsync();
				log.ResponseResult = result1;

				//create log
				await _requestLogsService.CreateAsync(log);
				
				return Ok(result1);
            }

            var result = await response.Content.ReadAsStringAsync();
            log.ResponseResult = result;

            await _requestLogsService.CreateAsync(log);

            return Ok(result);

        }

        [HttpPost("create_booking")]
        [Authorize(Roles = "CreateBooking")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> CreateBooking(CreateBooking model)
        {
            model.pnr_on_hold_indicator = true;
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
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);


            StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, httpContent);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                token = await _loginConfigService.RefreshTokenAsync();
                HttpClient client1 = new HttpClient();
                client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
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


        [HttpPost("create_booking_paynow")]
        [Authorize(Roles = "CreateBookingPaynow")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> CreateBookingPaynow(CreateBooking model)
        {
            model.pnr_on_hold_indicator = false;
            var userName = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            var url = _apiUrlOption.CreateBooking;
            string json = JsonConvert.SerializeObject(model);


            //check max payment
            var paymentTotal = model.guest_payment_info.Sum(a => a.payment_amount);
            var user = await _userService.GetUserAsync(userName);
            var maxPayment = user?.MaxPayment ?? 0;
            if (paymentTotal > maxPayment)
            {
                return Ok($"the amount of payment exceeds the allowable limit");
            }
            //add log
            var log = new RequestLogs
            {
                ApiUrl = url,
                CreatedUser = userName,
                Parameters = json,
                CreatedDate = DateTimeOffset.Now
            };


            var token = await _loginConfigService.GetTokenAsync();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);


            StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, httpContent);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                token = await _loginConfigService.RefreshTokenAsync();
                HttpClient client1 = new HttpClient();
                client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
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
        [HttpPost("confirm_price")]
        [Authorize(Roles = "ConfirmPrice")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> ConfirmPrice(ConfirmPrice model)
        {
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
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);


            StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, httpContent);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                token = await _loginConfigService.RefreshTokenAsync();
                HttpClient client1 = new HttpClient();
                client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response1 = await client1.PostAsync(url, httpContent);
                var result1 = await response1.Content.ReadAsStringAsync();
                log.ResponseResult = result1;

                //create log
                await _requestLogsService.CreateAsync(log);

                return Ok(result1);
            }

            var result = await response.Content.ReadAsStringAsync();
            log.ResponseResult = result;

            await _requestLogsService.CreateAsync(log);

            return Ok(result);

        }

        [HttpPost("booking_paynow")]
        [Authorize(Roles = "BookingPaynow")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> BookingPaynow(BookingPaynow model)
        {
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
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);


            StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, httpContent);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                token = await _loginConfigService.RefreshTokenAsync();
                HttpClient client1 = new HttpClient();
                client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response1 = await client1.PostAsync(url, httpContent);
                var result1 = await response1.Content.ReadAsStringAsync();
                log.ResponseResult = result1;

                //create log
                await _requestLogsService.CreateAsync(log);

                return Ok(result1);
            }

            var result = await response.Content.ReadAsStringAsync();
            log.ResponseResult = result;

            await _requestLogsService.CreateAsync(log);

            return Ok(result);

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
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);


            StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, httpContent);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                token = await _loginConfigService.RefreshTokenAsync();
                HttpClient client1 = new HttpClient();
                client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response1 = await client1.PostAsync(url, httpContent);
                var result1 = await response1.Content.ReadAsStringAsync();
                log.ResponseResult = result1;

                //create log
                await _requestLogsService.CreateAsync(log);

                return Ok(result1);
            }

            var result = await response.Content.ReadAsStringAsync();
            log.ResponseResult = result;

            await _requestLogsService.CreateAsync(log);

            return Ok(result);

        }
        [HttpPost("retrieve_booking")]
        [Authorize(Roles = "RetrieveBooking")]
        [Authorize(Policy = "SameIpPolicy")]
        [Authorize(Policy = "MaxRequest")]
        public async Task<IActionResult> RetrieveBooking(RetrieveBooking model)
        {
            var userName = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
            var hasPnr = await _requestLogsService.HasPnrNumberAsync(model.pnr_number);
            if (!hasPnr)
            {
                return Ok(JsonConvert.SerializeObject(new {
                    Message = "Reservation code is not in the system"
                }));
            }
            var checkAuthor = await _requestLogsService.CheckPnrNumberAsync(userName, model.pnr_number);
            if (checkAuthor)
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
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);


                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _loginConfigService.RefreshTokenAsync();
                    HttpClient client1 = new HttpClient();
                    client1.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    var response1 = await client1.PostAsync(url, httpContent);
                    var result1 = await response1.Content.ReadAsStringAsync();
                    log.ResponseResult = result1;

                    //create log
                    await _requestLogsService.CreateAsync(log);

                    return Ok(result1);
                }

                var result = await response.Content.ReadAsStringAsync();
                log.ResponseResult = result;

                await _requestLogsService.CreateAsync(log);

                return Ok(result);
            }
            else
            {
                return Ok(JsonConvert.SerializeObject(new
                {
                    Message = "You do not have permission to view this pnr_number"
                }));
            }

        }

        //[HttpPost("test")]
        //public async Task<IActionResult> Test(LoginModel model) {
        //    try
        //    {
        //        HttpClient client = new HttpClient();

        //        var login = new Login(_apiLoginOption.Email, _apiLoginOption.Password, _apiLoginOption.ClientId, _apiLoginOption.ClientSecret);
        //        //var test = new LoginModel
        //        //{
        //        //    UserName = userName,
        //        //    Password = password
        //        //};
        //        string json = JsonConvert.SerializeObject(login);

        //        StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        //        var response = await client.PostAsync(_apiLoginOption.ApiUrl, httpContent);

        //        var result = await response.Content.ReadAsStringAsync();
        //        try
        //        {
        //            var value = JsonConvert.DeserializeObject<ResultLogin>(result);
        //            return Ok(value.token);
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

