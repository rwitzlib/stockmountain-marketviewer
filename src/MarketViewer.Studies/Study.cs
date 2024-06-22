using MarketViewer.Contracts.Models.Study;
using Polygon.Client.Models;

namespace MarketViewer.Studies;

public abstract class Study<T> where T : Study<T>, new()
{
    public List<List<LineEntry>> Lines { get; set; }
    public readonly List<string> ErrorMessages = [];

    public static T Compute(Bar[] candleData, params object[] parameters)
    {
        var study = new T();

        if (candleData is null || candleData.Length == 0)
        {
            study.ErrorMessages.Add("No candle data.");
        }

        if (!study.ValidateParameters(parameters) || study.ErrorMessages.Count != 0)
        {
            study.Lines = null;
            return study;
        }

        var lines = study.Initialize(candleData);
        study.Lines = lines;

        return study;
    }

    protected abstract List<List<LineEntry>> Initialize(Bar[] candleData);

    protected abstract bool ValidateParameters(IReadOnlyList<object> parameters);
}