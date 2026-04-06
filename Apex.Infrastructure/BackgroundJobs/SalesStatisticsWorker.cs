using Apex.Domain.Entities;
using Apex.Domain.Interfaces;
using Apex.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apex.Infrastructure.BackgroundJobs;

public class SalesStatisticsWorker(IServiceScopeFactory scopeFactory, ILogger<SalesStatisticsWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running: Calculating sales stats...");

            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var stats = (await unitOfWork.SalesStatistics.GetAllAsync()).FirstOrDefault();

                    DateTime? lastRun = stats?.LastUpdated;

                    var (newRevenue, newCount) = await unitOfWork.Orders.GetSalesSummaryAsync(lastRun);

                    if (stats == null)
                    {
                        logger.LogInformation("Creating NEW statistics row...");

                        var newStats = new SalesStatistic
                        {
                            TotalRevenue = newRevenue,
                            TotalOrders = newCount,
                            LastUpdated = DateTime.UtcNow
                        };
                        await unitOfWork.SalesStatistics.AddAsync(newStats);
                    }
                    else if (newCount > 0)
                    {
                        // UPDATE: Add the new data to the existing totals instead of overwriting
                        stats.TotalRevenue += newRevenue;
                        stats.TotalOrders += newCount;
                        stats.LastUpdated = DateTime.UtcNow;
                        unitOfWork.SalesStatistics.Update(stats);
                    }

                    var result = await unitOfWork.CompleteAsync();
                    logger.LogInformation("Database Save Result: {Rows} rows affected.", result);
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Worker task was cancelled during execution.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "WORKER ERROR: {Message}", ex.Message);
                }
            }

            // Wait 5 minutes (300,000ms) before the next run
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}