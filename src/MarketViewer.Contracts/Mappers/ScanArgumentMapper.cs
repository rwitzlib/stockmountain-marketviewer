using MarketViewer.Contracts.Entities.Scan;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Scan.Operands;
using MarketViewer.Contracts.Presentation.Models;

namespace MarketViewer.Contracts.Mappers;

public static class ScanArgumentMapper
{
    public static ScanArgument ConvertFromScanArgumentDetails(ScanArgumentDetails scanArgumentDetails)
    {
        if (scanArgumentDetails == null)
        {
            return null;
        }

        var scanArgument = new ScanArgument
        {
            Operator = scanArgumentDetails.Operator,
            Argument = scanArgumentDetails.Argument != null ? ConvertFromScanArgumentDetails(scanArgumentDetails.Argument) : null,
            Filters = []
        };

        foreach (var filterReq in scanArgumentDetails.Filters)
        {
            var newFilter = new Filter
            {
                CollectionModifier = filterReq.CollectionModifier,
                FirstOperand = ConvertFromOperandDetails(filterReq.FirstOperand),
                Operator = filterReq.Operator,
                SecondOperand = ConvertFromOperandDetails(filterReq.SecondOperand),
                Timeframe = filterReq.Timeframe
            };
            scanArgument.Filters.Add(newFilter);
        }

        return scanArgument;
    }

    public static ScanArgumentDetails ConvertToScanArgumentDetails(ScanArgument scanArgument)
    {
        if (scanArgument == null)
        {
            return null;
        }

        var scanArgumentDetails = new ScanArgumentDetails
        {
            Operator = scanArgument.Operator,
            Argument = scanArgument.Argument != null ? ConvertToScanArgumentDetails(scanArgument.Argument) : null,
            Filters = []
        };

        foreach (var filter in scanArgument.Filters)
        {
            var newFilter = new FilterDetails
            {
                CollectionModifier = filter.CollectionModifier,
                FirstOperand = ConvertToOperandDetails(filter.FirstOperand),
                Operator = filter.Operator,
                SecondOperand = ConvertToOperandDetails(filter.SecondOperand),
                Timeframe = filter.Timeframe
            };
            scanArgumentDetails.Filters.Add(newFilter);
        }

        return scanArgumentDetails;
    }

    private static IScanOperand ConvertFromOperandDetails(OperandDetails operandDetails)
    {
        if (operandDetails == null)
        {
            return null;
        }

        switch (operandDetails.Type)
        {
            case OperandType.PriceAction:
                return new PriceActionOperand
                {
                    PriceAction = Enum.Parse<PriceActionType>(operandDetails.Name),
                    Modifier = operandDetails.Modifier.Value,
                    Timeframe = operandDetails.Timeframe
                };
            case OperandType.Study:
                {
                    var studyOperand = new StudyOperand
                    {
                        Study = Enum.Parse<StudyType>(operandDetails.Name),
                        Parameters = operandDetails.Parameters,
                        Modifier = operandDetails.Modifier.Value,
                        Timeframe = operandDetails.Timeframe
                    };
                    return studyOperand;
                }
            case OperandType.Property:
                return new PropertyOperand
                {
                    Property = operandDetails.Parameters,
                };
            case OperandType.Fixed:
                return new FixedOperand
                {
                    Value = operandDetails.Value.Value
                };
            case OperandType.Custom:
                throw new NotSupportedException($"Unsupported operand type: {operandDetails.Type}");
            default:
                throw new NotSupportedException($"Unsupported operand type: {operandDetails.Type}");
        }
    }

    private static OperandDetails ConvertToOperandDetails(IScanOperand scanOperand)
    {
        if (scanOperand == null)
        {
            return null;
        }

        return scanOperand switch
        {
            PriceActionOperand priceActionOperand => new OperandDetails
            {
                Type = OperandType.PriceAction,
                Name = priceActionOperand.PriceAction.ToString(),
                Modifier = priceActionOperand.Modifier,
                Timeframe = priceActionOperand.Timeframe
            },
            StudyOperand studyOperand => new OperandDetails
            {
                Type = OperandType.Study,
                Name = studyOperand.Study.ToString(),
                Parameters = studyOperand.Parameters,
                Modifier = studyOperand.Modifier,
                Timeframe = studyOperand.Timeframe
            },
            FixedOperand fixedOperand => new OperandDetails
            {
                Type = OperandType.Fixed,
                Value = fixedOperand.Value
            },
            _ => throw new NotImplementedException()
        };
    }
}
