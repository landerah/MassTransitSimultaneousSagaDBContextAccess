using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MassTransitDuplicateSagaHandlersIssue
{
    internal class ServiceBusListenerHost : IHostedService
    {
        private readonly IBusControl _busLazy;
        private readonly ILogger<ServiceBusPublisherHost> _logger;
        private IBusControl _busControl;
        private IHostApplicationLifetime _appLifetime;
        private SQLiteConnection m_dbConnection;

        public ServiceBusListenerHost(IBusControl busLazy, IHostApplicationLifetime appLifetime, ILogger<ServiceBusPublisherHost> logger)
        {
            _busLazy = busLazy;
            _logger = logger;
            _appLifetime = appLifetime;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStopping);
            File.Delete("MyDatabase.sqlite");
            SQLiteConnection.CreateFile("TestDB.sqlite");

            m_dbConnection = new SQLiteConnection("Data Source=TestDB.sqlite;Version=3;");
            m_dbConnection.Open();

            string sql = "create table TestSaga (Name varchar(20), CurrentState varchar(64), CollectionId varchar(64), CorrelationId varchar(20))";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();


            m_dbConnection.Close();

            _busControl = _busLazy;
            await _busControl.StartAsync().ConfigureAwait(true);
            _logger.LogInformation("Bus ready");
        }

        private void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
            _busControl.Stop();
            m_dbConnection.Close();
            m_dbConnection.Dispose();
            //File.Delete("TestDB.sqlite");
            // Perform on-stopping activities here
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}