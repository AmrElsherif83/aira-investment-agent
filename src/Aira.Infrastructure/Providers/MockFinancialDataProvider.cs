namespace Aira.Infrastructure.Providers;

using Aira.Core.Interfaces;
using Aira.Core.Models.DTOs;

/// <summary>
/// Mock implementation of financial data provider with deterministic data.
/// Provides detailed NVIDIA data and generic fallback for other tickers.
/// </summary>
public class MockFinancialDataProvider : IFinancialDataProvider
{
    /// <inheritdoc/>
    public Task<FinancialSnapshot> GetFinancialSnapshotAsync(string ticker)
    {
        var snapshot = ticker.ToUpperInvariant() switch
        {
            "NVDA" => CreateNvidiaSnapshot(),
            _ => CreateGenericSnapshot(ticker)
        };

        return Task.FromResult(snapshot);
    }

    private static FinancialSnapshot CreateNvidiaSnapshot()
    {
        return new FinancialSnapshot
        {
            Ticker = "NVDA",
            CompanyName = "NVIDIA Corporation",
            Revenue = 60_922_000_000m, // $60.9B FY2024
            NetIncome = 29_760_000_000m, // $29.76B
            EarningsPerShare = 11.93m,
            ProfitMargin = 48.9, // 48.9% - exceptional
            RevenueGrowthYoY = 126.0, // 126% YoY growth - AI boom
            TotalAssets = 65_728_000_000m,
            TotalLiabilities = 20_989_000_000m,
            FreeCashFlow = 27_090_000_000m, // Strong cash generation
            ReturnOnEquity = 123.8, // Very high ROE
            FiscalPeriod = "FY2024",
            SourceRef = "mock://financials/NVDA"
        };
    }

    private static FinancialSnapshot CreateGenericSnapshot(string ticker)
    {
        return new FinancialSnapshot
        {
            Ticker = ticker,
            CompanyName = $"{ticker} Corporation",
            Revenue = 10_000_000_000m,
            NetIncome = 1_200_000_000m,
            EarningsPerShare = 2.45m,
            ProfitMargin = 12.0,
            RevenueGrowthYoY = 5.5,
            TotalAssets = 25_000_000_000m,
            TotalLiabilities = 15_000_000_000m,
            FreeCashFlow = 1_500_000_000m,
            ReturnOnEquity = 15.2,
            FiscalPeriod = "FY2024",
            SourceRef = $"mock://financials/{ticker}"
        };
    }
}
