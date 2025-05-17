using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Study
{
    [ExcludeFromCodeCoverage]
    public class Line
    {
        public Line()
        {
            Id = Guid.NewGuid().ToString();
            Series = new List<LineEntry>();
            Width = 1;
        }

        public string Id { get; }
        public string Color { get; set; }
        public int Width { get; set; }
        public List<LineEntry> Series { get; set; }
    }
}
