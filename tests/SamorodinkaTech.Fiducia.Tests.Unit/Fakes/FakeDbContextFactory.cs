using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Fakes;

/// <summary>
/// Фабрика для создания FiduciaDbContext с in-memory провайдером.
/// Каждый вызов создаёт новую изолированную БД в памяти.
/// </summary>
public static class FakeDbContextFactory
{
    private static int _dbCounter;

    /// <summary>
    /// Создаёт новый контекст с уникальной in-memory БД.
    /// </summary>
    public static FiduciaDbContext Create()
    {
        var dbName = $"fiducia-test-{Interlocked.Increment(ref _dbCounter)}";

        var options = new DbContextOptionsBuilder<FiduciaDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new FiduciaDbContext(options);
    }

    /// <summary>
    /// Создаёт контекст и заполняет его тестовыми данными через переданный action.
    /// После заполнения вызывается SaveChangesAsync.
    /// </summary>
    public static async Task<FiduciaDbContext> CreateSeededAsync(
        Func<FiduciaDbContext, Task> seed)
    {
        var ctx = Create();
        await seed(ctx);
        await ctx.SaveChangesAsync();
        return ctx;
    }
}
