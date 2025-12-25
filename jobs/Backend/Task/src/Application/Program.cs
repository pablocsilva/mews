using ExchangeRateUpdater.Application.Configuration;
using ExchangeRateUpdater.Domain;
using ExchangeRateUpdater.Domain.Entities;
using ExchangeRateUpdater.Domain.Interfaces;
using ExchangeRateUpdater.Infrastructure.Providers.CzechNationalBank;
using ExchangeRateUpdater.Infrastructure.Providers.CzechNationalBank.Clients;
using ExchangeRateUpdater.Infrastructure.Providers.CzechNationalBank.Configuration;
using ExchangeRateUpdater.Infrastructure.Providers.CzechNationalBank.Parsers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace ExchangeRateUpdater.Application;

public static class Program
{
    private static IEnumerable<Currency> _currencies =
    [
        new Currency("USD"),
        new Currency("EUR"),
        new Currency("CZK"),
        new Currency("JPY"),
        new Currency("KES"),
        new Currency("RUB"),
        new Currency("THB"),
        new Currency("TRY"),
        new Currency("XYZ")
    ];

    public static async Task<int> Main(string[] args)
    {
        ConfigureLogging();

        try
        {
            Log.Information("Starting Exchange Rate Updater Application");

            var host = CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<ExchangeRateApp>();
            await app.RunAsync(_currencies);

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            Console.WriteLine($"Could not retrieve exchange rates: '{ex.Message}'.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .UseDefaultServiceProvider(options =>
                {
                    options.ValidateScopes = true;
                    options.ValidateOnBuild = true;
                }
            )
            .ConfigureServices((context, services) =>
                {
                    services
                        .AddOptions<ProviderOptions>()
                        .Bind(context.Configuration.GetSection(ProviderOptions.ConfigurationSectionName))
                        .Validate(opts =>
                            {
                                opts.Validate();
                                return true;
                            }
                        );

                    services.Configure<ProviderOptions>(context.Configuration.GetSection("CnbProvider"));

                    services.AddSingleton(sp => sp.GetRequiredService<IOptions<ProviderOptions>>().Value);

                    services
                        .AddHttpClient<ICzechNationalBankClient, CzechNationalBankHttpClient>()
                        .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
                        .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy())
                        .AddPolicyHandler(PollyPolicies.GetTimeoutPolicy());

                    services.AddSingleton<IDailyExchangeRatesResponseParser, PipeSeparatedDailyExchangeResponseParser>();
                    services.AddScoped<IExchangeRateProvider, ExchangeRateProvider>();
                    services.AddTransient<ExchangeRateApp>();
                }
            );

}