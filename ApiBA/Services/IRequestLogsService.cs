using System;
using ApiBA.Data;
using ApiBA.Models;

namespace ApiBA.Services
{
	public interface IRequestLogsService
	{
        Task<(OperationResult state, RequestLogs value)> CreateAsync(RequestLogs requestLogs);
        Task<bool> CheckPnrNumberAsync(string userName, string pnrNumber);
        Task<int> GetTotalRequestTodayAsync(string userName);
    }
}

