using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Scan.Operands;
using MarketViewer.Contracts.Requests.Scan;

namespace MarketViewer.Contracts.Mappers;

public static class ScanArgumentMapper
{
    public static ScanArgument ConvertFromRequest(ScanArgumentRequest scanArgumentRequest)
    {
        if (scanArgumentRequest == null)
        {
            return null;
        }

        var scanArgument = new ScanArgument
        {
            Operator = scanArgumentRequest.Operator,
            Argument = scanArgumentRequest.Argument != null ? ConvertFromRequest(scanArgumentRequest.Argument) : null,
            Filters = []
        };

        foreach (var filterReq in scanArgumentRequest.Filters)
        {
            var newFilter = new Filter
            {
                CollectionModifier = filterReq.CollectionModifier,
                FirstOperand = ConvertFromOperandRequest(filterReq.FirstOperand),
                Operator = filterReq.Operator,
                SecondOperand = ConvertFromOperandRequest(filterReq.SecondOperand),
                Timeframe = filterReq.Timeframe
            };
            scanArgument.Filters.Add(newFilter);
        }

        return scanArgument;
    }

    private static IScanOperand ConvertFromOperandRequest(OperandRequest operandRequest)
    {
        if (operandRequest == null)
        {
            return null;
        }

        switch (operandRequest.Type)
        {
            case OperandType.PriceAction:
                return new PriceActionOperand
                {
                    PriceAction = Enum.Parse<PriceActionType>(operandRequest.Name),
                    Modifier = operandRequest.Modifier.Value,
                    Timeframe = operandRequest.Timeframe
                };
            case OperandType.Study:
                {
                    var studyOperand = new StudyOperand
                    {
                        Study = Enum.Parse<StudyType>(operandRequest.Name),
                        Parameters = operandRequest.Parameters,
                        Modifier = operandRequest.Modifier.Value,
                        Timeframe = operandRequest.Timeframe
                    };
                    return studyOperand;
                }
            case OperandType.Property:
                return new PropertyOperand
                {
                    Property = operandRequest.Parameters,
                };
            case OperandType.Fixed:
                return new FixedOperand
                {
                    Value = operandRequest.Value.Value
                };
            case OperandType.Custom:
                throw new NotSupportedException($"Unsupported operand type: {operandRequest.Type}");
            default:
                throw new NotSupportedException($"Unsupported operand type: {operandRequest.Type}");
        }
    }
}
