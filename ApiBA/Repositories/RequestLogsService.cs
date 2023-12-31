using System;
using ApiBA.Data;
using ApiBA.Models;
using ApiBA.Options;
using ApiBA.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace ApiBA.Repositories
{
	public class RequestLogsService : IRequestLogsService
	{
		private BaseDBContext _dBContext;
		private readonly ApiUrlOption _apiUrlOption;
		public RequestLogsService(BaseDBContext dBContext, IOptionsSnapshot<ApiUrlOption> optionsSnapshot)
		{
			_dBContext = dBContext;
			_apiUrlOption = optionsSnapshot.Value;
		}
		public async Task<(OperationResult state, RequestLogs value)> CreateAsync(RequestLogs requestLogs)
		{
			try
			{
				var value = await _dBContext.RequestLogs.AddAsync(requestLogs);
				await _dBContext.SaveChangesAsync();
				return (OperationResult.Success, value.Entity);
            }catch(Exception ex)
            {
				return (OperationResult.Failed(ex), null);
            }
		}

		public async Task<bool> CheckPnrNumberAsync(string userName,string pnrNumber)
		{
			var url = _apiUrlOption.CreateBooking;
			var check = await _dBContext.RequestLogs.AnyAsync(a => a.CreatedUser == userName && a.PnrNumber == pnrNumber && a.ApiUrl == url);
			return check;
		}

        public async Task<bool> HasPnrNumberAsync(string pnrNumber)
        {
            var url = _apiUrlOption.CreateBooking;
            var check = await _dBContext.RequestLogs.AnyAsync(a => a.PnrNumber == pnrNumber && a.ApiUrl == url);
            return check;
        }

        public async Task<int> GetTotalRequestTodayAsync(string userName)
        {
			var today = DateTimeOffset.Now;
            var startDate =  new DateTimeOffset(today.Date);
            var endDate = new DateTimeOffset(today.AddDays(1).Date);
            var count = await _dBContext.RequestLogs.Where(a => a.CreatedUser == userName)
				 .Where(a => a.CreatedDate >= startDate)
                 .Where(a => a.CreatedDate < endDate)
                 .CountAsync();
			return count;
        }

        public IQueryable<RequestLogs> Query() => _dBContext.RequestLogs.AsQueryable();
    }
}

