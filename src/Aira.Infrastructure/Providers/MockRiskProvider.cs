namespace Aira.Infrastructure.Providers;

using Aira.Core.Interfaces;
using Aira.Core.Models.DTOs;

/// <summary>
/// Mock implementation of risk provider with deterministic risk assessments.
/// Provides realistic NVIDIA risks and generic fallback for other tickers.
/// </summary>
public class MockRiskProvider : IRiskProvider
{
    /// <inheritdoc/>
    public Task<List<RiskItem>> GetKnownRisksAsync(string ticker)
    {
        var risks = ticker.ToUpperInvariant() switch
        {
            "NVDA" => CreateNvidiaRisks(),
            _ => CreateGenericRisks(ticker)
        };

        return Task.FromResult(risks);
    }

    private static List<RiskItem> CreateNvidiaRisks()
    {
        return new List<RiskItem>
        {
            new RiskItem
            {
                Title = "Valuation Risk - High P/E Ratio",
                Description = "NVIDIA trades at premium valuation multiples reflecting high growth expectations. Any slowdown in AI adoption or revenue growth could lead to significant multiple compression and stock price correction.",
                Severity = "high",
                Category = "valuation"
            },
            new RiskItem
            {
                Title = "Competitive Pressure from AMD and Custom Silicon",
                Description = "Intensifying competition from AMD's MI300 series and custom AI chips developed by hyperscalers (Google TPU, Amazon Trainium, Microsoft Maia) could erode market share and pricing power.",
                Severity = "high",
                Category = "market"
            },
            new RiskItem
            {
                Title = "Geopolitical and Export Control Risks",
                Description = "U.S.-China technology restrictions limit access to Chinese market. Further regulatory tightening or geopolitical tensions could significantly impact revenue from international markets.",
                Severity = "high",
                Category = "regulatory"
            },
            new RiskItem
            {
                Title = "Customer Concentration Risk",
                Description = "Heavy reliance on small number of large cloud and tech customers. Loss of major customer or reduced spending could materially impact financial performance.",
                Severity = "medium",
                Category = "operational"
            },
            new RiskItem
            {
                Title = "Supply Chain and Manufacturing Dependencies",
                Description = "Dependence on TSMC for advanced chip manufacturing creates concentration risk. Geopolitical issues affecting Taiwan or manufacturing disruptions could severely impact supply.",
                Severity = "medium",
                Category = "operational"
            },
            new RiskItem
            {
                Title = "AI Market Sustainability Questions",
                Description = "Debate over sustainability and ROI of massive AI infrastructure investments by tech companies. If AI monetization disappoints, could lead to sharp demand correction for AI chips.",
                Severity = "medium",
                Category = "market"
            },
            new RiskItem
            {
                Title = "Technology Obsolescence Risk",
                Description = "Rapid pace of innovation in AI hardware. Breakthrough alternative technologies (e.g., neuromorphic computing, quantum computing) could disrupt current GPU-centric architecture.",
                Severity = "low",
                Category = "technology"
            }
        };
    }

    private static List<RiskItem> CreateGenericRisks(string ticker)
    {
        return new List<RiskItem>
        {
            new RiskItem
            {
                Title = "Market Competition",
                Description = $"{ticker} faces competition from established players and new entrants in its market segment.",
                Severity = "medium",
                Category = "market"
            },
            new RiskItem
            {
                Title = "Regulatory Compliance",
                Description = $"{ticker} must navigate evolving regulatory requirements across multiple jurisdictions.",
                Severity = "medium",
                Category = "regulatory"
            },
            new RiskItem
            {
                Title = "Economic Sensitivity",
                Description = $"{ticker} business performance may be affected by macroeconomic conditions and market cycles.",
                Severity = "low",
                Category = "market"
            }
        };
    }
}
