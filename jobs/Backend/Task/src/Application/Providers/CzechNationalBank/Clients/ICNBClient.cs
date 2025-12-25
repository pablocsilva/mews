namespace Application.Providers.CzechNationalBank.Clients;

public interface ICNBClient
{
    Task<string> GetDailyRatesAsync(CancellationToken cancellationToken = default);
}