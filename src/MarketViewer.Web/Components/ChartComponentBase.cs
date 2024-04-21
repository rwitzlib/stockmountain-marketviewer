using Blazored.Modal;
using Blazored.Modal.Services;
using MarketViewer.Contracts.Responses;
using MarketViewer.Web.Contracts;
using MarketViewer.Web.Services;
using MarketViewer.Web.Studies;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;
using MarketViewer.Contracts.Requests;
using MarketViewer.Web.Contracts.Studies;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models;
using MarketViewer.Web.Enums;

namespace MarketViewer.Web.Components
{
    public class ChartComponentBase : ComponentBase
    {
        #region Inject
        [Inject] public ChartService AggregateService { get; set; }
        [Inject] public IJSRuntime JsRuntime { get; set; }
        #endregion

        #region Parameters
        [Parameter] public StocksRequest StocksRequest { get; set; }
        [Parameter] public string Id { get; set; }
        [Parameter] public string Height { get; set; } = "100%";
        [Parameter] public string Width { get; set; } = "100%";
        [Parameter] public bool DisplayParameters { get; set; } = true;
        [Parameter] public bool EnableScroll { get; set; } = true;
        [CascadingParameter] public IModalService Modal { get; set; } = default!;
        [CascadingParameter] public ClockComponent ClockComponent { get; set; }
        #endregion

        #region Protected Fields
        protected bool IsVolumeEnabled { get; set; } = false;
        protected bool IsDrawSegmentEnabled { get; set; } = false;
        protected bool IsDrawTrendEnabled { get; set; } = false;
        protected bool IsDrawResistSuppEnabled { get; set; } = false;
        protected bool IsPopoverOpen { get; set; } = false;
        protected string ClickedDrawingId { get; set; }
        #endregion

        #region Private Fields
        private VolumeParams VolumeParams { get; set; } = new VolumeParams();
        private StocksResponse Value { get; set; }
        private List<Study> Studies { get; set; } = new List<Study>();
        private List<Line> Drawings { get; set; } = new List<Line>();
        private List<(float Price, long Timestamp)> Points = new();
        private DotNetObjectReference<ChartComponentBase> ObjectReference;
        private IJSInProcessRuntime JsInProcessRuntime;
        #endregion

        #region Override Methods
        protected override void OnInitialized()
        {
            if (StocksRequest != null)
            {
                return;
            }

            var date = ClockComponent.GetTime();
            var newDate = (DateTimeOffset)date.UtcDateTime;

            StocksRequest = new StocksRequest
            {
                Ticker = "SPY",
                Multiplier = 1,
                Timespan = Timespan.minute,
                From = new DateTimeOffset(newDate.Year, newDate.Month, newDate.Day, 0, 0, 0, newDate.Offset),
                To = newDate
            };

            JsInProcessRuntime = (IJSInProcessRuntime)JsRuntime;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {

                JsInProcessRuntime = (IJSInProcessRuntime)JsRuntime;
                ObjectReference = DotNetObjectReference.Create(this);
                JsInProcessRuntime.InvokeVoid("BuildChart", Id, ObjectReference, EnableScroll);
            }

            GetChartData();
        }
        #endregion

        #region Protected Methods
        protected async void GetChartData()
        {
            var timestamp = ClockComponent.GetTime();
            SetTimeOnRequest(timestamp);
            try
            {

                Value = await AggregateService.GetStockData(StocksRequest);

                if (Value is null)
                {
                    return;
                }

                if (Value.Studies is not null)
                {
                    foreach (var study in Value.Studies)
                    {
                        var title = study.Parameters == null ? $"{study.Name}" : $"{study.Name} ({string.Join(',', study.Parameters)})";

                        var studyOnChart = Studies.FirstOrDefault(q =>
                            q.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase));

                        if (studyOnChart is not null)
                        {
                            continue;
                        }

                        var studyName = study.Name.Split(" (")[0];

                        var paramType = typeof(StudyParams).Assembly.GetTypes()
                            .Where(q => q.BaseType == typeof(StudyParams))
                            .FirstOrDefault(q => q.Name.Contains(studyName, StringComparison.InvariantCultureIgnoreCase));

                        var studyType = typeof(Study).Assembly.GetTypes()
                            .Where(q => q.BaseType == typeof(Study))
                            .FirstOrDefault(q => q.Name.Equals(studyName, StringComparison.InvariantCultureIgnoreCase));

                        if (paramType == null || studyType == null)
                        {
                            continue;
                        }

                        var studyParam = Activator.CreateInstance(paramType, study.Parameters == null ? null : new object[] { study.Parameters });
                        var newStudy = (Study)Activator.CreateInstance(studyType, studyParam);

                        if (newStudy is null)
                        {
                            continue;
                        }

                        Studies.Add(newStudy);
                        await newStudy.AddStudyToChart(JsInProcessRuntime, Id);
                    }
                }

                var responseJson = JsonSerializer.Serialize(Value);
                JsInProcessRuntime.InvokeVoid("UpdateChart", Id, responseJson);

                foreach (var study in Studies)
                {
                    study.Compute(Value);
                    await study.UpdateStudy(JsInProcessRuntime, Id);
                }

                if (IsVolumeEnabled)
                {
                    var volumeResponse = AggregateService.CreateVolumeChart(VolumeParams, Value);

                    var volumeJson = JsonSerializer.Serialize(volumeResponse);
                    JsInProcessRuntime.InvokeVoid("UpdateVolume", Id, volumeJson);
                }
            }
            catch (Exception e)
            {
                
            }
        }

        protected void SetTimeOnRequest(DateTimeOffset timestamp)
        {
            var days = (int)(StocksRequest.To - StocksRequest.From).TotalDays;

            StocksRequest.From = new DateTimeOffset(timestamp.Year, timestamp.Month, timestamp.AddDays(-days).Day, 0, 0, 0, timestamp.Offset);
            StocksRequest.To = timestamp;
        }

        protected void SetDateFromRange(ChangeEventArgs eventArgs)
        {
            var days = int.Parse(eventArgs.Value?.ToString() ?? string.Empty);
            var end = StocksRequest.To.AddDays(-days);

            StocksRequest.From = new DateTimeOffset(end.Year, end.Month, end.Day, 0, 0, 0, end.Offset);
            GetChartData();
        }

        protected async Task ViewStudies()
        {
            var options = new ModalOptions
            {
                DisableBackgroundCancel = true
            };

            var parameters = new ModalParameters
            {
                { nameof(StudyViewComponent.Studies), Studies },
                { nameof(StudyViewComponent.AggregateResponse), Value },
                { nameof(StudyViewComponent.Id), Id }
            };

            var modal = Modal.Show<StudyViewComponent>(title: "Studies", parameters: parameters, options: options);
            var result = await modal.Result;
        }

        protected void ShowOrHideVolume()
        {
            IsVolumeEnabled = !IsVolumeEnabled;

            if (IsVolumeEnabled)
            {
                var volumeResponse = AggregateService.CreateVolumeChart(VolumeParams, Value);

                var volumeJson = JsonSerializer.Serialize(volumeResponse);
                JsInProcessRuntime.InvokeVoid("PlotVolume", Id, volumeJson);
            }
            else
            {
                JsInProcessRuntime.InvokeVoid("RemoveVolume", Id);
            }
        }

        protected void ToggleDrawSegment(bool toggle)
        {
            if (toggle)
            {
                IsDrawTrendEnabled = false;
                IsDrawResistSuppEnabled = false;
            }
            IsDrawSegmentEnabled = toggle;
            JsInProcessRuntime.InvokeVoid("EnableDrawing", Id, toggle);
        }

        protected void ToggleDrawTrend(bool toggle)
        {
            if (toggle)
            {
                IsDrawSegmentEnabled = false;
                IsDrawResistSuppEnabled = false;
            }
            IsDrawTrendEnabled = toggle;
            JsInProcessRuntime.InvokeVoid("EnableDrawing", Id, toggle);
        }

        protected void ToggleDrawResistSupport(bool toggle)
        {
            if (toggle)
            {
                IsDrawSegmentEnabled = false;
                IsDrawTrendEnabled = false;
            }
            IsDrawResistSuppEnabled = toggle;
            JsInProcessRuntime.InvokeVoid("EnableDrawing", Id, toggle);
        }

        protected void RemoveAllDrawings()
        {
            Drawings.Clear();
            JsInProcessRuntime.InvokeVoid("RemoveAllDrawings", Id);
        }


        protected void RemoveDrawing()
        {
            JsInProcessRuntime.InvokeVoid("RemoveDrawing", Id, ClickedDrawingId);
            ClickedDrawingId = null;
            IsPopoverOpen = false;
            StateHasChanged();
        }
        #endregion

        #region JavaScript Methods
        [JSInvokable]
        public void DrawLine(float price, long timestamp)
        {
            Points.Add((price, timestamp));

            if (IsDrawSegmentEnabled)
            {
                if (Points.Count == 2)
                {
                    var orderedPoints = Points.OrderBy(q => q.Timestamp).ToList();
                    var line = DrawLineSegment(orderedPoints);
                    Drawings.Add(line);
                    Points = new List<(float, long)>();
                }
                else if (Points.Count > 2)
                {
                    Points = new List<(float, long)>();
                }
                return;
            }

            if (IsDrawTrendEnabled)
            {
                if (Points.Count == 2)
                {
                    var orderedPoints = Points.OrderBy(q => q.Timestamp).ToList();
                    var line = DrawTrendLine(orderedPoints);
                    Drawings.Add(line);
                    Points = new List<(float, long)>();
                }
                else if (Points.Count > 2)
                {
                    Points = new List<(float, long)>();
                }
                return;
            }

            if (IsDrawResistSuppEnabled)
            {
                if (Points.Count == 1)
                {
                    var line = DrawResistSuppLine(Points);
                    Drawings.Add(line);
                }
                Points = new List<(float, long)>();
                return;
            }
        }

        [JSInvokable]
        public void TogglePopover(bool isPopoverOpen, string drawingId = null)
        {
            IsPopoverOpen = isPopoverOpen;

            if (!IsPopoverOpen)
            {
                StateHasChanged();
                return;
            }

            if (drawingId is not null)
            {
                ClickedDrawingId = drawingId;
            }

            StateHasChanged();
        }
        #endregion

        #region Private Methods
        private Line DrawLineSegment(List<(float Price, long Timestamp)> points)
        {
            var startCandle = Value.Results.First(q => q.Timestamp == points[0].Timestamp);
            var endCandle = Value.Results.First(q => q.Timestamp == points[1].Timestamp);

            var startIndex = Value.Results.ToList().IndexOf(startCandle);
            var endIndex = Value.Results.ToList().IndexOf(endCandle);

            var range = Value.Results.ToList().GetRange(startIndex, endIndex - startIndex);

            var priceDiff = points[1].Price - points[0].Price;

            var slope = priceDiff / range.Count;

            var series = new List<LineEntry>
            {
                new()
                {
                    Value = points[0].Price,
                    Timestamp = startCandle.Timestamp
                }
            };
            for (int i = 1; i < range.Count; i++)
            {
                series.Add(new LineEntry
                {
                    Value = series[^1].Value + slope,
                    Timestamp = range[i].Timestamp
                });
            }
            series.Add(new LineEntry
            {
                Value = series[^1].Value + slope,
                Timestamp = endCandle.Timestamp
            });

            var line = new Line
            {
                Color = StudyColor.red.ToString(),
                Series = series
            };

            var json = JsonSerializer.Serialize(line);

            JsInProcessRuntime.InvokeVoid("CreateDrawing", Id, json, 0, false);

            return line;
        }

        private Line DrawTrendLine(List<(float Price, long Timestamp)> points)
        {
            var startCandle = Value.Results.First(q => q.Timestamp == points[0].Timestamp);
            var endCandle = Value.Results.First(q => q.Timestamp == points[1].Timestamp);

            var startIndex = Value.Results.ToList().IndexOf(startCandle);
            var endIndex = Value.Results.ToList().IndexOf(endCandle);

            var range = Value.Results.ToList().GetRange(startIndex, endIndex - startIndex);

            var priceDiff = points[1].Price - points[0].Price;

            var slope = priceDiff / range.Count;


            var startValue = points[0].Price - (startIndex * slope);

            var series = new List<LineEntry>
            {
                new LineEntry()
                {
                    Value = startValue,
                    Timestamp = Value.Results.ToArray()[0].Timestamp,
                }
            };
            for (int i = 1; i < Value.Results.Count(); i++)
            {
                series.Add(new LineEntry
                {
                    Value = series[^1].Value + slope,
                    Timestamp = Value.Results.ToArray()[i].Timestamp
                });
            }

            var line = new Line
            {
                Color = StudyColor.red.ToString(),
                Series = series
            };

            var json = JsonSerializer.Serialize(line);

            JsInProcessRuntime.InvokeVoid("CreateDrawing", Id, json, 0, false);

            return line;
        }

        private Line DrawResistSuppLine(List<(float Price, long Timestamp)> points)
        {
            var series = new List<LineEntry>();
            for (int i = 0; i < Value.Results.Count(); i++)
            {
                series.Add(new LineEntry
                {
                    Value = points[0].Price,
                    Timestamp = Value.Results.ToArray()[i].Timestamp
                });
            }

            var line = new Line
            {
                Color = StudyColor.red.ToString(),
                Series = series
            };

            var json = JsonSerializer.Serialize(line);

            JsInProcessRuntime.InvokeVoid("CreateDrawing", Id, json, 0, false);

            return line;
        }
        #endregion
    }
}
