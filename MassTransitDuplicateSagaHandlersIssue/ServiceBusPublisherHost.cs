using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MassTransitDuplicateSagaHandlersIssue
{
    internal class ServiceBusPublisherHost : IHostedService
    {
        private IBusControl _busLazy;
        private ILogger<ServiceBusPublisherHost> _logger;
        private IHostApplicationLifetime _appLifetime;
        private IBusControl _busControl;

        public ServiceBusPublisherHost(IBusControl busLazy, IHostApplicationLifetime appLifetime, ILogger<ServiceBusPublisherHost> logger)
        {
            _busLazy = busLazy;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _busControl = _busLazy;
            _logger.LogInformation("Bus ready");

            await _busControl.Publish(new CreateSaga() {CollectionId = "Coll1",Name = "Saga1"});
            await _busControl.Publish(new CreateSaga() {CollectionId = "Coll1",Name = "Saga2"});

            await _busControl.Publish(new UpdateSagaState() {CollectionId = "Coll1"});

           

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}