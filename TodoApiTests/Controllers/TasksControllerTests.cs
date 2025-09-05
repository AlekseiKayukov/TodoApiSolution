using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.DTO;
using TodoApi.Enum;
using TodoApi.Interface;
using TodoApi.Models;

namespace TodoApiTests.Controllers
{
    public class TasksControllerTests
    {
        private static TodoDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TodoDbContext(options);
        }

        private static TasksController CreateController(
            ITodoTaskService? service = null,
            IDistributedCache? cache = null,
            TodoDbContext? context = null)
        {
            var db = context ?? CreateInMemoryContext();
            var mockCache = cache ?? new Mock<IDistributedCache>().Object;
            var mockService = service ?? new Mock<ITodoTaskService>().Object;
            return new TasksController(db, mockCache, mockService);
        }

        [Fact]
        public async Task GetTasks_ReturnsOk_WithPagedResult()
        {
            // Arrange
            var expected = new PagedResultDto<TodoTaskDto>
            {
                TotalItems = 1,
                Page = 1,
                PageSize = 10,
                Items = new List<TodoTaskDto>
                {
                    new TodoTaskDto { Id = 1, Title = "t", Status = TodoTaskStatus.Active, CreatedAt = DateTime.UtcNow }
                }
            };
            var svc = new Mock<ITodoTaskService>();
            svc.Setup(s => s.GetTasksAsync(null, null, 1, 10))
               .ReturnsAsync(expected);

            var controller = CreateController(svc.Object);

            // Act
            var result = await controller.GetTasks(null, null, 1, 10) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
            var payload = Assert.IsType<PagedResultDto<TodoTaskDto>>(result.Value);
            Assert.Equal(expected.TotalItems, payload.TotalItems);
            Assert.Single(payload.Items);
            svc.Verify(s => s.GetTasksAsync(null, null, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetTask_Found_ReturnsOk()
        {
            // Arrange
            var dto = new TodoTaskDto { Id = 42, Title = "task", Status = TodoTaskStatus.Active, CreatedAt = DateTime.UtcNow };
            var svc = new Mock<ITodoTaskService>();
            svc.Setup(s => s.GetTaskByIdAsync(42)).ReturnsAsync(dto);
            var controller = CreateController(svc.Object);

            // Act
            var result = await controller.GetTask(42) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var payload = Assert.IsType<TodoTaskDto>(result!.Value);
            Assert.Equal(42, payload.Id);
            svc.Verify(s => s.GetTaskByIdAsync(42), Times.Once);
        }

        [Fact]
        public async Task GetTask_NotFound_Returns404()
        {
            // Arrange
            var svc = new Mock<ITodoTaskService>();
            svc.Setup(s => s.GetTaskByIdAsync(99)).ReturnsAsync((TodoTaskDto?)null);
            var controller = CreateController(svc.Object);

            // Act
            var result = await controller.GetTask(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateTask_ReturnsCreated_WithDto()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var input = new TodoTask { Title = "new", Status = TodoTaskStatus.Active, CreatedAt = now };
            var created = new TodoTask { Id = 7, Title = "new", Status = TodoTaskStatus.Active, CreatedAt = now };
            var svc = new Mock<ITodoTaskService>();
            svc.Setup(s => s.CreateTaskAsync(It.IsAny<TodoTask>()))
               .ReturnsAsync(created);
            var controller = CreateController(svc.Object);

            // Act
            var result = await controller.CreateTask(input) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(TasksController.GetTask), result!.ActionName);
            var dto = Assert.IsType<TodoTaskDto>(result.Value);
            Assert.Equal(7, dto.Id);
            svc.Verify(s => s.CreateTaskAsync(It.Is<TodoTask>(t => t.Title == "new")), Times.Once);
        }

        [Fact]
        public async Task UpdateTask_Updated_ReturnsNoContent()
        {
            // Arrange
            var svc = new Mock<ITodoTaskService>();
            svc.Setup(s => s.UpdateTaskAsync(5, It.IsAny<TodoTask>())).ReturnsAsync(true);
            var controller = CreateController(svc.Object);

            // Act
            var result = await controller.UpdateTask(5, new TodoTask { Id = 5 });

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateTask_NotFound_Returns404()
        {
            // Arrange
            var svc = new Mock<ITodoTaskService>();
            svc.Setup(s => s.UpdateTaskAsync(5, It.IsAny<TodoTask>())).ReturnsAsync(false);
            var controller = CreateController(svc.Object);

            // Act
            var result = await controller.UpdateTask(5, new TodoTask { Id = 5 });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTask_Deleted_ReturnsNoContent()
        {
            // Arrange
            var svc = new Mock<ITodoTaskService>();
            svc.Setup(s => s.DeleteTaskAsync(3)).ReturnsAsync(true);
            var controller = CreateController(svc.Object);

            // Act
            var result = await controller.DeleteTask(3);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTask_NotFound_Returns404()
        {
            // Arrange
            var svc = new Mock<ITodoTaskService>();
            svc.Setup(s => s.DeleteTaskAsync(3)).ReturnsAsync(false);
            var controller = CreateController(svc.Object);

            // Act
            var result = await controller.DeleteTask(3);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
