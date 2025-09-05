using TodoApi.Repositories;
using TodoApiTests.Mocks;
using TodoApi.Enum;
using FluentAssertions;

namespace TodoApiTests.Repositories
{
    public class TodoTaskRepositoryTests
    {
        [Fact]
        public async Task GetPagedAsync_Filters_Sorts_And_Paginates()
        {
            // Arrange
            using var ctx = MockDbContext.Create(nameof(GetPagedAsync_Filters_Sorts_And_Paginates));
            var repo = new TodoTaskRepository(ctx);

            // Act
            var (total, items) = await repo.GetPagedAsync("Задача", TodoTaskStatus.Active, page: 1, pageSize: 2);

            // Assert
            total.Should().Be(2); // из мок-данных 2 активные
            items.Should().HaveCountLessThanOrEqualTo(2);
            items.Should().BeInDescendingOrder(x => x.CreatedAt);
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Entity_AsNoTracking()
        {
            // Arrange
            using var ctx = MockDbContext.Create(nameof(GetByIdAsync_Returns_Entity_AsNoTracking));
            var repo = new TodoTaskRepository(ctx);

            // Act
            var entity = await repo.GetByIdAsync(1, asNoTracking: true);

            // Assert
            entity.Should().NotBeNull();
            entity!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Add_Update_Remove_Works()
        {
            // Arrange
            using var ctx = MockDbContext.Create(nameof(Add_Update_Remove_Works));
            var repo = new TodoTaskRepository(ctx);

            // Act - Add
            var added = new TodoApi.Models.TodoTask 
            { 
                Title = "Новая",
                Description = "Описание задачи",
                Status = TodoTaskStatus.Active, 
                CreatedAt = DateTime.UtcNow 
            };
            await repo.AddAsync(added);
            
            // Assert - After Add
            (await repo.GetByIdAsync(added.Id)).Should().NotBeNull();

            // Act - Update
            added.Title = "Обновленная";
            await repo.UpdateAsync(added);
            
            // Assert - After Update
            (await repo.GetByIdAsync(added.Id))!.Title.Should().Be("Обновленная");

            // Act - Remove
            await repo.RemoveAsync(added);
            
            // Assert - After Remove
            (await repo.GetByIdAsync(added.Id)).Should().BeNull();
        }

        [Fact]
        public async Task CountByStatusAsync_Returns_Correct_Count()
        {
            // Arrange
            using var ctx = MockDbContext.Create(nameof(CountByStatusAsync_Returns_Correct_Count));
            var repo = new TodoTaskRepository(ctx);

            // Act
            var active = await repo.CountByStatusAsync(TodoTaskStatus.Active);
            var completed = await repo.CountByStatusAsync(TodoTaskStatus.Completed);

            // Assert
            active.Should().Be(2);
            completed.Should().Be(1);
        }
    }
}
