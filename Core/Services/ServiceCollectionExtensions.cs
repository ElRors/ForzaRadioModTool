using ForzaRadioModTool.Configuration;
using ForzaRadioModTool.Core.Interfaces;
using ForzaRadioModTool.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace ForzaRadioModTool.Core.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddForzaRadioServices(this IServiceCollection services)
        {
            // Configuration
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton(provider => provider.GetRequiredService<IConfigurationService>().Settings);

            // Configure Serilog
            services.AddLogging(builder =>
            {
                var configService = services.BuildServiceProvider().GetRequiredService<IConfigurationService>();
                var settings = configService.Settings;

                var loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.Is(ParseLogLevel(settings.Logging.LogLevel))
                    .WriteTo.Console()
                    .Enrich.FromLogContext();

                if (settings.Logging.EnableFileLogging)
                {
                    var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "ForzaRadioModTool-.log");
                    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                    
                    loggerConfig.WriteTo.File(
                        logPath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: settings.Logging.MaxLogFiles,
                        outputTemplate: settings.Logging.LogFilePattern
                    );
                }

                Log.Logger = loggerConfig.CreateLogger();
                builder.ClearProviders();
                builder.AddSerilog();
            });

            // Core services
            services.AddScoped<IFileManager, FileManager>();
            services.AddScoped<IXmlProcessor, XmlProcessor>();
            services.AddScoped<IAudioProcessor, AudioProcessor>();
            services.AddScoped<IRadioManager, RadioManager>();

            return services;
        }

        private static LogEventLevel ParseLogLevel(string logLevel)
        {
            return logLevel.ToLowerInvariant() switch
            {
                "verbose" => LogEventLevel.Verbose,
                "debug" => LogEventLevel.Debug,
                "information" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }
    }
}