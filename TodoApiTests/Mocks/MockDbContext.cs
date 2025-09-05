using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Enum;

namespace TodoApiTests.Mocks
{
    /// <summary>
    /// Мок-версия <see cref="TodoDbContext"/> для unit-тестов с InMemory базой данных.
    /// Позволяет создавать изолированное тестовое окружение с фейковыми данными.
    /// </summary>
    public class MockDbContext
    {
        /// <summary>
        /// Создает и возвращает новый инстанс <see cref="TodoDbContext"/>,
        /// использующий InMemory базу данных с уникальным именем.
        /// </summary>
        /// <param name="dbName">Уникальное имя базы данных для изоляции тестов.</param>
        /// <returns>Экземпляр контекста базы данных для тестирования.</returns>
        public static TodoDbContext Create(string dbName)
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new TodoDbContext(options);

            context.Tasks.AddRange(new[]
            {
                new TodoTask 
                {
                    Id = 1,
                    Title = "Задача 1",
                    Description = "Описание тестовой задачи 1",
                    Status = TodoTaskStatus.Active,
                    CreatedAt = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new TodoTask 
                {
                    Id = 2,
                    Title = "Задача 2",
                    Description = "Описание тестовой задачи 2",
                    Status = TodoTaskStatus.Completed,
                    CreatedAt = new DateTime(2025, 9, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new TodoTask 
                {
                    Id = 3,
                    Title = "Задача 3",
                    Description = "Описание тестовой задачи 3",
                    Status = TodoTaskStatus.Active,
                    CreatedAt = new DateTime(2025, 8, 1, 10, 0, 0, DateTimeKind.Utc)
                }
            });

            context.SaveChanges();

            return context;
        }
    }
}
