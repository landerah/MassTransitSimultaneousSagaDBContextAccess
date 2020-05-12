using System.IO;
using GreenPipes.Specifications;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace MassTransitDuplicateSagaHandlersIssue
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    hostContext.HostingEnvironment.EnvironmentName =
                        System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                })
                .ConfigureAppConfiguration((context, cb) => cb.SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables()
                    .AddJsonFile("appsettings.json", true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true)
                )
                .ConfigureLogging((hostingContext, logging) =>
                {

                    logging.AddDebug();
                    logging.AddConsole();

                })
                .ConfigureServices(
                    (hostContext, services) =>
                    {
                        services.AddMassTransit(scc =>
                        {

                            scc.AddBus(sp =>
                                Bus.Factory.CreateUsingInMemory(cfg =>
                                {
                                    var queue = $"test-queue";



                                    cfg.ReceiveEndpoint(queue, e => { e.StateMachineSaga<TestSaga>(sp); });

                                }));



                            scc.AddSagaStateMachine<TestSagaStateMachine, TestSaga>()
                                .EntityFrameworkRepository(r =>
                                {
                                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                                    r.AddDbContext<TestSagaDbContext, TestSagaDbContext>((provider, builder) =>
                                    {
                                        builder.UseSqlite("Data Source=TestDB.sqlite");
                                    });
                                });

                        });



                        services.AddHostedService<ServiceBusListenerHost>();
                        services.AddHostedService<ServiceBusPublisherHost>();
                    });
    }
}
