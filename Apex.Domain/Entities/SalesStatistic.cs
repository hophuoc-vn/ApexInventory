using Apex.Domain.Common;

namespace Apex.Domain.Entities;

public class SalesStatistic : BaseEntity
{
    public SalesStatistic()
    {
        Id = Guid.NewGuid();
        LastUpdated = DateTime.UtcNow;
    }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public string TopSellingSku { get; set; } = string.Empty;

    public decimal AverageOrderValue => TotalOrders > 0 ? TotalRevenue / TotalOrders : 0;
}