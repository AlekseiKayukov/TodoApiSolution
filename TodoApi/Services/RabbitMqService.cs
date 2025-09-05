using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace TodoApi.Services
{
    /// <summary>
    /// Сервис публикации событий в RabbitMQ через fanout‑exchange `task_events`.
    /// Управляет соединением и каналом, реализует <see cref="IDisposable"/>.
    /// </summary>
    public class RabbitMqService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="RabbitMqService"/>.
        /// Создаёт соединение и открывает канал для взаимодействия с RabbitMQ.
        /// </summary>
        /// <param name="configuration">
        /// Конфигурация приложения (ожидаются ключи: RabbitMq:Host, RabbitMq:Username, RabbitMq:Password).
        /// </param>
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
        /// Публикует событие изменения статуса задачи в exchange `task_events` (тип fanout).
        /// Примечание: для fanout routingKey брокером игнорируется, сообщение доставляется всем привязанным очередям.
        /// Сообщение помечается как persistent; для гарантированной доставки могут потребоваться publisher confirms/транзакции.
        /// </summary>
        /// <param name="taskId">Идентификатор задачи, статус которой изменился.</param>
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
        /// Вызывать однократно из управляемого жизненного цикла; канал не потокобезопасен.
        /// </summary>
        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
