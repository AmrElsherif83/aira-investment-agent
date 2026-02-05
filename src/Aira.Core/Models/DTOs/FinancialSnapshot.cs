namespace Aira.Core.Models.DTOs;

/// <summary>
/// Structured financial data snapshot for a company.
/// </summary>
public class FinancialSnapshot
{
    /// <summary>
    /// Stock ticker symbol.
    /// </summary>
    public required string Ticker { get; set; }

    /// <summary>
    /// Company name.
    /// </summary>
    public required string CompanyName { get; set; }

    /// <summary>
    /// Annual revenue in USD.
    /// </summary>
    public decimal Revenue { get; set; }

    /// <summary>
    /// Net income (profit) in USD.
    /// </summary>
    public decimal NetIncome { get; set; }

    /// <summary>
    /// Earnings per share.
    /// </summary>
    public decimal EarningsPerShare { get; set; }

    /// <summary>
    /// Profit margin as a percentage (0-100).
    /// </summary>
    public double ProfitMargin { get; set; }

    /// <summary>
    /// Revenue growth rate year-over-year as a percentage.
    /// </summary>
    public double RevenueGrowthYoY { get; set; }

    /// <summary>
    /// Total assets in USD.
    /// </summary>
    public decimal TotalAssets { get; set; }

    /// <summary>
    /// Total liabilities in USD.
    /// </summary>
    public decimal TotalLiabilities { get; set; }

    /// <summary>
    /// Free cash flow in USD.
    /// </summary>
    public decimal FreeCashFlow { get; set; }

    /// <summary>
    /// Return on equity as a percentage.
    /// </summary>
    public double ReturnOnEquity { get; set; }

    /// <summary>
    /// Fiscal period for this data (e.g., "FY2023", "Q4 2023").
    /// </summary>
    public string? FiscalPeriod { get; set; }

    /// <summary>
    /// Data source reference (e.g., "mock://financials/NVDA").
    /// </summary>
    public string? SourceRef { get; set; }
}
