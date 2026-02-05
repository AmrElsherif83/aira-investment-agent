namespace Aira.Core.Agent;

using Aira.Core.Agent.Helpers;
using Aira.Core.Agent.Models;
using Aira.Core.Interfaces;
using Aira.Core.Models;
using Aira.Core.Models.DTOs;
using Aira.Core.Models.Enums;

/// <summary>
/// Multi-step agentic orchestrator implementing observable analysis workflow.
/// Demonstrates decomposition, data gathering, synthesis, and reflection.
/// </summary>
public class AnalysisAgent : IAnalysisAgent
{
    private readonly IFinancialDataProvider _financialProvider;
    private readonly INewsProvider _newsProvider;
    private readonly IRiskProvider _riskProvider;

    public AnalysisAgent(
        IFinancialDataProvider financialProvider,
        INewsProvider newsProvider,
        IRiskProvider riskProvider)
    {
        _financialProvider = financialProvider ?? throw new ArgumentNullException(nameof(financialProvider));
        _newsProvider = newsProvider ?? throw new ArgumentNullException(nameof(newsProvider));
        _riskProvider = riskProvider ?? throw new ArgumentNullException(nameof(riskProvider));
    }

    /// <inheritdoc/>
    public async Task<(InvestmentReport Report, List<AgentStepResult> Steps)> ExecuteAsync(
        string ticker,
        CancellationToken cancellationToken)
    {
        var steps = new List<AgentStepResult>();

        // Step 1: Planning
        var planningResult = await ExecutePlanningStepAsync(ticker, cancellationToken);
        steps.Add(planningResult);

        // Step 2: Data Gathering
        var gatheringResult = await ExecuteGatheringStepAsync(ticker, cancellationToken);
        steps.Add(gatheringResult);

        // Extract gathered data from artifacts
        var gatheringOutput = gatheringResult.Artifacts["gathering_output"] as GatheringStepOutput
            ?? throw new InvalidOperationException("Gathering step did not produce expected output.");

        // Step 3: Scoring and Synthesis
        var scoringResult = await ExecuteScoringStepAsync(
            ticker,
            gatheringOutput,
            cancellationToken);
        steps.Add(scoringResult);

        // Extract scoring output
        var scoringOutput = scoringResult.Artifacts["scoring_output"] as ScoringStepOutput
            ?? throw new InvalidOperationException("Scoring step did not produce expected output.");

        // Step 4: Reflection and Finalization
        var reflectionResult = await ExecuteReflectionStepAsync(
            ticker,
            gatheringOutput,
            scoringOutput,
            cancellationToken);
        steps.Add(reflectionResult);

        // Extract final report
        var report = reflectionResult.Artifacts["final_report"] as InvestmentReport
            ?? throw new InvalidOperationException("Reflection step did not produce final report.");

        return (report, steps);
    }

    /// <summary>
    /// Step 1: Planning - Determine analysis requirements and approach.
    /// </summary>
    private Task<AgentStepResult> ExecutePlanningStepAsync(
        string ticker,
        CancellationToken cancellationToken)
    {
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            // Create structured plan
            var plan = new PlanningStepOutput
            {
                Ticker = ticker,
                PlannedSteps = new List<string>
                {
                    "Data Gathering - Collect financial, news, and risk data",
                    "Scoring - Calculate component and composite scores",
                    "Synthesis - Generate thesis and insights",
                    "Reflection - Validate data quality and adjust confidence"
                },
                RequiredTools = new List<string>
                {
                    "IFinancialDataProvider - Financial statements and metrics",
                    "INewsProvider - Recent news and sentiment",
                    "IRiskProvider - Risk factors and severity assessment"
                },
                KeyMetrics = new List<string>
                {
                    "Revenue growth YoY",
                    "Profit margins",
                    "Return on Equity",
                    "News sentiment score",
                    "Risk severity distribution",
                    "Data completeness"
                },
                FocusAreas = new List<string>
                {
                    "Financial performance and growth trajectory",
                    "Market sentiment and public perception",
                    "Risk factors and competitive positioning",
                    "Valuation considerations"
                }
            };

            var stepResult = new AgentStepResult
            {
                StepName = "Planning",
                Status = AgentStepStatus.Succeeded,
                StartedAt = startTime,
                FinishedAt = DateTimeOffset.UtcNow,
                Summary = $"Created structured analysis plan for {ticker}. " +
                          $"Identified {plan.RequiredTools.Count} data sources and {plan.KeyMetrics.Count} key metrics to evaluate.",
                Artifacts = new Dictionary<string, object>
                {
                    ["planning_output"] = plan,
                    ["ticker"] = ticker
                }
            };

            return Task.FromResult(stepResult);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new AgentStepResult
            {
                StepName = "Planning",
                Status = AgentStepStatus.Failed,
                StartedAt = startTime,
                FinishedAt = DateTimeOffset.UtcNow,
                Summary = $"Planning step failed: {ex.Message}",
                Artifacts = new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Step 2: Data Gathering - Collect data from all providers.
    /// </summary>
    private async Task<AgentStepResult> ExecuteGatheringStepAsync(
        string ticker,
        CancellationToken cancellationToken)
    {
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            var output = new GatheringStepOutput();

            // Gather financial data
            try
            {
                output.FinancialData = await _financialProvider.GetFinancialSnapshotAsync(ticker);
                if (output.FinancialData?.SourceRef != null)
                {
                    output.SourcesAccessed.Add(output.FinancialData.SourceRef);
                }
            }
            catch (Exception ex)
            {
                output.DataWarnings.Add($"Financial data retrieval failed: {ex.Message}");
            }

            // Gather news data
            try
            {
                output.NewsItems = await _newsProvider.GetRecentHeadlinesAsync(ticker);
                if (output.NewsItems.Count == 0)
                {
                    output.DataWarnings.Add("No news items retrieved.");
                }
                else
                {
                    output.SourcesAccessed.Add($"mock://news/{ticker}");
                }
            }
            catch (Exception ex)
            {
                output.DataWarnings.Add($"News data retrieval failed: {ex.Message}");
            }

            // Gather risk data
            try
            {
                output.Risks = await _riskProvider.GetKnownRisksAsync(ticker);
                if (output.Risks.Count == 0)
                {
                    output.DataWarnings.Add("No risk factors identified.");
                }
                else
                {
                    output.SourcesAccessed.Add($"mock://risks/{ticker}");
                }
            }
            catch (Exception ex)
            {
                output.DataWarnings.Add($"Risk data retrieval failed: {ex.Message}");
            }

            // Calculate data completeness
            double completeness = 0.0;
            if (output.FinancialData != null) completeness += 0.4;
            if (output.NewsItems.Count > 0) completeness += 0.3;
            if (output.Risks.Count > 0) completeness += 0.3;
            output.DataCompleteness = completeness;

            var summary = $"Gathered data from {output.SourcesAccessed.Count} sources. " +
                          $"Financial: {(output.FinancialData != null ? "?" : "?")}, " +
                          $"News: {output.NewsItems.Count} items, " +
                          $"Risks: {output.Risks.Count} factors. " +
                          $"Data completeness: {output.DataCompleteness:P0}.";

            if (output.DataWarnings.Count > 0)
            {
                summary += $" {output.DataWarnings.Count} warning(s) recorded.";
            }

            var stepResult = new AgentStepResult
            {
                StepName = "Data Gathering",
                Status = AgentStepStatus.Succeeded,
                StartedAt = startTime,
                FinishedAt = DateTimeOffset.UtcNow,
                Summary = summary,
                Artifacts = new Dictionary<string, object>
                {
                    ["gathering_output"] = output,
                    ["financial_snapshot"] = output.FinancialData ?? new object(),
                    ["news_count"] = output.NewsItems.Count,
                    ["risk_count"] = output.Risks.Count,
                    ["data_warnings"] = output.DataWarnings,
                    ["sources"] = output.SourcesAccessed
                }
            };

            return stepResult;
        }
        catch (Exception ex)
        {
            return new AgentStepResult
            {
                StepName = "Data Gathering",
                Status = AgentStepStatus.Failed,
                StartedAt = startTime,
                FinishedAt = DateTimeOffset.UtcNow,
                Summary = $"Data gathering step failed: {ex.Message}",
                Artifacts = new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                }
            };
        }
    }

    /// <summary>
    /// Step 3: Scoring and Synthesis - Calculate scores and generate thesis.
    /// </summary>
    private Task<AgentStepResult> ExecuteScoringStepAsync(
        string ticker,
        GatheringStepOutput gatheringOutput,
        CancellationToken cancellationToken)
    {
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            // Analyze sentiment
            var sentimentScore = SentimentAnalyzer.AnalyzeSentiment(gatheringOutput.NewsItems);
            var sentimentCategory = SentimentAnalyzer.CategorizeSentiment(sentimentScore);
            var (posCount, negCount, neutCount) = SentimentAnalyzer.CountSentiments(gatheringOutput.NewsItems);

            // Calculate scores
            var scoreBreakdown = ScoreCalculator.CalculateScores(
                gatheringOutput.FinancialData,
                sentimentScore,
                gatheringOutput.Risks);

            // Calculate initial confidence
            var confidence = ScoreCalculator.CalculateConfidence(
                scoreBreakdown,
                gatheringOutput.DataCompleteness);

            // Generate preliminary signal
            var signal = SignalGenerator.GenerateSignal(scoreBreakdown.CompositeScore, confidence);

            // Determine company name
            var companyName = gatheringOutput.FinancialData?.CompanyName ?? $"{ticker} Corporation";

            // Generate thesis
            var thesis = SignalGenerator.GenerateThesis(
                ticker,
                companyName,
                scoreBreakdown,
                signal,
                confidence);

            // Generate insights
            var highSeverityRisks = gatheringOutput.Risks.Count(r =>
                r.Severity?.ToLowerInvariant() is "critical" or "high");

            var insights = SignalGenerator.GenerateInsights(
                scoreBreakdown,
                sentimentScore,
                gatheringOutput.Risks.Count,
                highSeverityRisks);

            // Create scoring output
            var output = new ScoringStepOutput
            {
                ScoreBreakdown = scoreBreakdown,
                PreliminarySignal = signal,
                PreliminaryConfidence = confidence,
                Thesis = thesis,
                Insights = insights,
                ScoringDetails = new Dictionary<string, object>
                {
                    ["sentiment_score"] = sentimentScore,
                    ["sentiment_category"] = sentimentCategory,
                    ["positive_news"] = posCount,
                    ["negative_news"] = negCount,
                    ["neutral_news"] = neutCount,
                    ["high_severity_risks"] = highSeverityRisks,
                    ["data_completeness"] = gatheringOutput.DataCompleteness
                }
            };

            var summary = $"Calculated composite score: {scoreBreakdown.CompositeScore:F1}/100. " +
                          $"Preliminary signal: {signal} (Confidence: {confidence:P0}). " +
                          $"Generated {insights.Count} insights and investment thesis.";

            var stepResult = new AgentStepResult
            {
                StepName = "Scoring and Synthesis",
                Status = AgentStepStatus.Succeeded,
                StartedAt = startTime,
                FinishedAt = DateTimeOffset.UtcNow,
                Summary = summary,
                Artifacts = new Dictionary<string, object>
                {
                    ["scoring_output"] = output,
                    ["score_breakdown"] = scoreBreakdown,
                    ["composite_score"] = scoreBreakdown.CompositeScore,
                    ["preliminary_signal"] = signal,
                    ["preliminary_confidence"] = confidence,
                    ["sentiment_analysis"] = new
                    {
                        score = sentimentScore,
                        category = sentimentCategory,
                        positive = posCount,
                        negative = negCount,
                        neutral = neutCount
                    }
                }
            };

            return Task.FromResult(stepResult);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new AgentStepResult
            {
                StepName = "Scoring and Synthesis",
                Status = AgentStepStatus.Failed,
                StartedAt = startTime,
                FinishedAt = DateTimeOffset.UtcNow,
                Summary = $"Scoring step failed: {ex.Message}",
                Artifacts = new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Step 4: Reflection - Validate data quality and adjust confidence.
    /// </summary>
    private Task<AgentStepResult> ExecuteReflectionStepAsync(
        string ticker,
        GatheringStepOutput gatheringOutput,
        ScoringStepOutput scoringOutput,
        CancellationToken cancellationToken)
    {
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            var limitations = new List<string>();
            var adjustmentInsights = new List<Insight>();
            var finalConfidence = scoringOutput.PreliminaryConfidence;
            var finalSignal = scoringOutput.PreliminarySignal;

            // Check for incomplete data
            if (gatheringOutput.DataCompleteness < 1.0)
            {
                limitations.Add($"Data completeness at {gatheringOutput.DataCompleteness:P0}. " +
                               "Some data sources were incomplete or unavailable.");
                
                adjustmentInsights.Add(new Insight
                {
                    Type = "limitation",
                    Detail = "Incomplete data coverage reduces confidence in analysis conclusion.",
                    Impact = -0.3
                });

                // Reduce confidence proportionally
                finalConfidence *= gatheringOutput.DataCompleteness;
            }

            // Check for data warnings
            if (gatheringOutput.DataWarnings.Count > 0)
            {
                limitations.Add($"{gatheringOutput.DataWarnings.Count} data retrieval warning(s) recorded.");
                finalConfidence *= 0.9; // Reduce by 10%
            }

            // Check for contradictory signals
            var scoreVariance = Math.Abs(scoringOutput.ScoreBreakdown.FinancialScore - 
                                        scoringOutput.ScoreBreakdown.SentimentScore);
            
            if (scoreVariance > 30)
            {
                limitations.Add("Significant variance between financial fundamentals and market sentiment suggests conflicting signals.");
                
                adjustmentInsights.Add(new Insight
                {
                    Type = "risk",
                    Detail = "Conflicting signals between financial strength and market sentiment create uncertainty.",
                    Impact = -0.4
                });

                finalConfidence *= 0.85; // Reduce by 15%
            }

            // If confidence drops too low, adjust to Neutral
            if (finalConfidence < 0.45 && scoringOutput.PreliminarySignal != "Neutral")
            {
                limitations.Add("Low confidence due to data quality issues. Adjusting signal to Neutral.");
                finalSignal = "Neutral";
                
                adjustmentInsights.Add(new Insight
                {
                    Type = "limitation",
                    Detail = "Insufficient confidence to maintain directional bias. Neutral stance recommended.",
                    Impact = 0.0
                });
            }

            // Combine all insights
            var allInsights = new List<Insight>(scoringOutput.Insights);
            allInsights.AddRange(adjustmentInsights);

            // Collect all sources
            var allSources = new List<SourceRef>();
            foreach (var sourceRef in gatheringOutput.SourcesAccessed)
            {
                allSources.Add(new SourceRef
                {
                    Type = sourceRef.Contains("financials") ? "financial" :
                           sourceRef.Contains("news") ? "news" :
                           sourceRef.Contains("risks") ? "risk" : "unknown",
                    Ref = sourceRef,
                    Notes = null
                });
            }

            // Create final report
            var companyName = gatheringOutput.FinancialData?.CompanyName ?? $"{ticker} Corporation";
            
            var report = new InvestmentReport
            {
                Company = companyName,
                Thesis = scoringOutput.Thesis ?? "Analysis incomplete.",
                Signal = finalSignal,
                Insights = allInsights,
                Sources = allSources,
                Confidence = Math.Max(0.0, Math.Min(1.0, finalConfidence)),
                Limitations = limitations.Count > 0 ? limitations : null,
                ScoreBreakdown = scoringOutput.ScoreBreakdown,
                GeneratedAt = DateTimeOffset.UtcNow
            };

            var adjustmentSummary = adjustmentInsights.Count > 0
                ? $" Applied {adjustmentInsights.Count} reflection adjustment(s)."
                : " No adjustments needed.";

            var summary = $"Reflection complete. Final signal: {finalSignal} (Confidence: {finalConfidence:P0}). " +
                          $"Data completeness: {gatheringOutput.DataCompleteness:P0}. " +
                          $"Limitations: {limitations.Count}.{adjustmentSummary}";

            var stepResult = new AgentStepResult
            {
                StepName = "Reflection and Finalization",
                Status = AgentStepStatus.Succeeded,
                StartedAt = startTime,
                FinishedAt = DateTimeOffset.UtcNow,
                Summary = summary,
                Artifacts = new Dictionary<string, object>
                {
                    ["final_report"] = report,
                    ["final_signal"] = finalSignal,
                    ["final_confidence"] = finalConfidence,
                    ["adjustments_applied"] = adjustmentInsights.Count,
                    ["limitations_count"] = limitations.Count,
                    ["confidence_adjustment"] = finalConfidence - scoringOutput.PreliminaryConfidence
                }
            };

            return Task.FromResult(stepResult);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new AgentStepResult
            {
                StepName = "Reflection and Finalization",
                Status = AgentStepStatus.Failed,
                StartedAt = startTime,
                FinishedAt = DateTimeOffset.UtcNow,
                Summary = $"Reflection step failed: {ex.Message}",
                Artifacts = new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                }
            });
        }
    }
}
