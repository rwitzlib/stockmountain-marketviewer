using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Scan.Operands;
using MarketViewer.Contracts.Presentation.Responses;
using MarketViewer.Studies;

namespace MarketViewer.Core.Scan.Filters;

public class StudyFilter(StudyFactory studyFactory) : IFilterV2
{
    public float[] Compute(IScanOperand operand, StocksResponse stocksResponse, Timeframe timeframe)
    {
        var studyOperand = operand as StudyOperand;

        var parameters = studyOperand.Parameters is null ? [] : studyOperand.Parameters.Split(',');
        var studyResponse = studyFactory.Compute(studyOperand.Study, parameters, stocksResponse);

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
