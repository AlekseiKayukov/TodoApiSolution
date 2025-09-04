using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace TodoApi.Services
{
    /// <summary>
    /// Сервис для работы с RabbitMQ, реализующий публикацию сообщений о событиях.
    /// </summary>
    public class RabbitMqService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="RabbitMqService"/>.
        /// Создает подключение и открывает канал для взаимодействия с RabbitMQ.
        /// </summary>
        /// <param name="configuration">Конфигурация приложения с настройками
        /// подключения к RabbitMQ.</param>
        public RabbitMqService(IConfiguration configuration)
        {
            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMq:Host"],
                UserName = configuration["RabbitMq:Username"],
                Password = configuration["RabbitMq:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "task_events",
                                     type: ExchangeType.Fanout,
                                     durable: true);
        }

        /// <summary>
        /// Публикует событие изменения статуса задачи в RabbitMQ.
        /// </summary>
        /// <param name="taskId">Идентификатор задачи, статус который изменился.</param>
        /// <param name="newStatus">Новый статус задачи.</param>
        public void PublishTaskStatusChanged(int taskId, string newStatus)
        {
            var eventData = new { TaskId = taskId, NewStatus = newStatus };
            var messageBody = JsonSerializer.Serialize(eventData);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: "task_events",
                                  routingKey: "task.status.changed",
                                  basicProperties: properties,
                                  body: body);
        }

        /// <summary>
        /// Освобождает ресурсы: закрывает канал и соединение с RabbitMQ.
        /// </summary>
        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
