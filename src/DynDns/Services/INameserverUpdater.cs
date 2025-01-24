using Ardalis.Result;

namespace DynDns.Services;

public interface INameserverUpdater
{
	Task<Result> UpdateAsync(string domain, string ip);
}