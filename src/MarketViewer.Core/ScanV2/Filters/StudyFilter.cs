using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Responses;
using MarketViewer.Studies;

namespace MarketViewer.Core.ScanV2.Filters;

public class StudyFilter : IFilterV2
{
    public float[] Compute(IScanOperand operand, StocksResponse stocksResponse, Timeframe timeframe)
    {
        var studyOperand = operand as StudyOperand;

        var parameters = studyOperand.Parameters.Split(',');
        var studyResponse = StudyService.ComputeStudy(studyOperand.Type, parameters, stocksResponse.Results.ToArray());

        if (studyResponse is null || studyResponse.Results.Count == 0)
        {
            return [];
        }

        var results = studyResponse.Results.First().Select(entry => entry.Value);

        var values = studyOperand.Modifier switch
        {
            OperandModifier.Value => results,
            OperandModifier.Slope => GetSlope(results.ToArray()),
            _ => []
        };

        return values.TakeLast(timeframe.Multiplier).ToArray();
    }

    private static float[] GetSlope(float[] values)
    {
        if (values.Length < 2)
        {
            return [];
        }

        var results = new float[values.Length - 1];

        for (int i = 0; i < values.Length - 1; i++)
        {
            results[i] = values[i + 1] - values[i];
        }

        return results;
    }
}
