const chartList = [];
let log = console.log;
let linesDisplayedOnChart = {};
let mainContent;

document.addEventListener('contextmenu', function (event) {
    var mainContent = document.getElementsByClassName("mud-main-content")[0];
    if (event.clientY >= mainContent.offsetTop) {
        event.preventDefault();
    }
});

let resizeObject = new ResizeObserver(function () {
    chartList.forEach(entry => {
        let id = entry.id;
        let chartSize = getElementSize(id);

        entry.chart.applyOptions({
            width: chartSize.width,
            height: chartSize.height
        });
    });
});

function getElementSize(id) {
    let width = 400;
    let height = 400;

    try {
        let container = document.getElementById(id + "-container");

        width = container.offsetWidth;
        height = container.offsetHeight;

        let parameters = document.getElementById(id + "-parameters");

        if (parameters != null)
        {
            height = height - parameters.offsetHeight;
        }
    }
    catch (err) {
        console.log(err);
    }

    if (height < 400) {
        height = 400;
    }

    return {
        width: width,
        height: height
    };

}

function BuildChart(id, dotnetReference, enableScroll) {
    if (chartList.find(q => q.id === id) !== undefined) {
        RemoveChart(id);
    }

    let chartElement = document.getElementById(id);
    let chartSize = getElementSize(id);

    chartElement.addEventListener('contextmenu', function (event) {
        event.preventDefault();

        var up = new MouseEvent('mouseup', {
            'view': document.defaultView,
            'bubbles': false,
            'cancelable': true,
            'clientX': event.clientX,
            'clientY': event.clientY,
            'button': 2
        });

        var els = document.elementsFromPoint(event.pageX, event.pageY).filter(e => e.tagName === "CANVAS");

        for (let i = 0; i < els.length; i++) {
            let el = els[i];

            el.dispatchEvent(up);
        };
        return false;
    });

    let chartOptions = {
        width: chartSize.width,
        height: chartSize.height,
        // Dark Mode
        layout: {
            background: {
                type: 'solid',
                color: '#2B2B43',
            },
            lineColor: '#2B2B43',
            textColor: '#D9D9D9',
        },
        timeScale: {
            timeVisible: true,
            secondsVisible: false
        },
        crosshair: {
            mode: LightweightCharts.CrosshairMode.Normal
        },
        handleScroll: {
            mouseWheel: enableScroll
        },
        handleScale: {
            mouseWheel: enableScroll
        },
        // Dark mode
        grid: {
            vertLines: {
                color: '#2B2B43',
            },
            horzLines: {
                color: '#363C4E',
            },
        },
        pane: 0
    }

    let legend = document.createElement('div');
    legend.id = id + "-legend";
    legend.classList.add('legend');
    legend.style.position = 'absolute';
    legend.style.left = '12px';
    legend.style.zIndex = '1000';
    legend.style.backgroundColor = "white";
    chartElement.appendChild(legend);

    let chart = LightweightCharts.createChart(chartElement, chartOptions);
    let candlestickSeries = chart.addCandlestickSeries();

    mainContent = document.getElementsByClassName("mud-main-content")[0];
    resizeObject.observe(mainContent);

    let handler = function clickHandler(param) {
        if (!param.point) {
            return;
        }

        HandleClick(param, id);
    };

    chart.subscribeClick(handler);

    let entry = {
        id: id,
        chart: chart,
        candlestickSeries: candlestickSeries,
        studies: {},
        drawings: {},
        handler: handler,
        drawingEnabled: false,
        dotnetReference: dotnetReference
    }

    chartList.push(entry);
}

async function HandleClick(param, id) {
    if (!param.point) {
        return;
    }

    let entry = chartList.find(q => q.id === id);

    await entry.dotnetReference.invokeMethod("TogglePopover", false, null);

    if (param.sourceEvent.button === 2) {
        let drawingId = findHoveredDrawing(id, param)

        let popoverId = "popover-spawn-" + id;
        let popoverParent = document.getElementById(popoverId);
        popoverParent.style.position = "absolute";
        popoverParent.style.left = param.sourceEvent.clientX + "px";
        popoverParent.style.top = param.sourceEvent.clientY + "px";

        // Show Popup
        let element = await entry.dotnetReference.invokeMethod("TogglePopover", true, drawingId);
    }
    else {
        if (entry.drawingEnabled) {
            let price = entry.candlestickSeries.coordinateToPrice(param.point.y);

            await entry.dotnetReference.invokeMethod("DrawLine", price, param.time);
        }
    }
}

function findHoveredDrawing(id, param) {
    let entry = chartList.find(q => q.id === id);

    // TODO: Eventually we may want to sort thru which drawings are within range and select the closest one
    for (const drawingId of Object.keys(entry.drawings)) {
        let drawing = entry.drawings[drawingId];

        let drawingPoint = drawing.data().find(point => point.time === param.time);

        let price = drawingPoint.value;

        var coordinate = drawing.priceToCoordinate(price);

        const pixelDistance = 3;
        if ((param.point.y - pixelDistance) < coordinate && (param.point.y + pixelDistance) > coordinate) {
            return drawingId;
        }
    }
}

function UpdateChart(id, aggregateResponse) {
    let entry = chartList.find(q => q.id === id);

    let aggregateResponseJson = JSON.parse(aggregateResponse);

    let candlestickData = aggregateResponseJson.Results.map(entry =>
    {
        return { time: entry.t, open: entry.o, high: entry.h, low: entry.l, close: entry.c }
    });

    entry.candlestickSeries.setData(candlestickData);
}

function RemoveChart(id) {
    let chartEntry = chartList.find(q => q.id === id);
    let index = chartList.indexOf(chartEntry);
    chartList.splice(index, 1);
}

function PlotVolume(id, volumeChart) {
    let entry = chartList.find(q => q.id === id);

    let volumeOptions = {
        priceFormat: {
            type: 'volume',
        },
        priceScaleId: ''
    };

    let volumeChartJson = JSON.parse(volumeChart);
    let volumeData = volumeChartJson.VolumeSeries.map(entry =>
    {
        let rgbaColor = hex2rgba(entry.Color, .5);
        return { time: entry.Timestamp, value: entry.Value, color: rgbaColor }
    });

    entry.volumeSeries = entry.chart.addHistogramSeries(volumeOptions);
    entry.chart.priceScale('').applyOptions({
        scaleMargins: {
            top: 0.3,
            bottom: 0
        }
    });
    entry.volumeSeries.setData(volumeData);
}

function UpdateVolume(id, volumeChart) {
    let entry = chartList.find(q => q.id === id);

    let volumeChartJson = JSON.parse(volumeChart);
    
    let volumeData = volumeChartJson.VolumeSeries.map(entry =>
    {
        let rgbaColor = hex2rgba(entry.Color, .5);
        return { time: entry.Timestamp, value: entry.Value, color: rgbaColor }
    });

    entry.volumeSeries.setData(volumeData);
}

function RemoveVolume(id) {
    let entry = chartList.find(q => q.id === id);
    entry.chart.removeSeries(entry.volumeSeries);
    entry.volumeSeries = null;
}

function CreateLineChart(id, lineChart, pane, priceVisible = true) {
    let chartEntry = chartList.find(q => q.id === id);
    
    let lineChartJson = JSON.parse(lineChart);
    
    let lineOptions = {
        color: lineChartJson.Color,
        lineWidth: lineChartJson.Width,
        pane: pane,
        priceLineVisible: priceVisible,
        lastValueVisible: priceVisible
    }
    
    let series = chartEntry.chart.addLineSeries(lineOptions)

    const lineData = lineChartJson.Series.map(entry =>
    {
        return { time: entry.Timestamp, value: entry.Value, color: entry.Color }
    });

    series.setData(lineData);

    chartEntry.studies[lineChartJson.Id] = series;
}

function UpdateLineChart(id, lineChart) {
    let entry = chartList.find(q => q.id === id);

    const lineChartJson = JSON.parse(lineChart);
    const lineData = lineChartJson.Series.map(entry => 
    {
        return { time:entry.Timestamp, value: entry.Value }
    });

    let series = entry.studies[lineChartJson.Id];
    series.setData(lineData);
}

function DeleteLineChart(id, lineChart) {
    let entry = chartList.find(q => q.id === id);
    const lineChartJson = JSON.parse(lineChart);
    let seriesToRemove = entry.studies[lineChartJson.Id];
    entry.chart.removeSeries(seriesToRemove);
}

function RemovePane(id, paneToRemove){
    let entry = chartList.find(q => q.id === id);
    entry.chart.removePane(paneToRemove);
}

function AddStudyToLegend(id, title, color) {
    let chartEntry = chartList.find(q => q.id === id);

    let legend = document.getElementById(id + "-legend");

    let firstRow = document.createElement('div');
    firstRow.id = title;
    firstRow.innerText = title;
    firstRow.style.color = color;
    legend.style.top = '10px';

    legend.appendChild(firstRow);
}

function RemoveStudyFromLegend(id, title) {
    let legend = document.getElementById(id + "-legend");

    let rowToRemove = document.getElementById(title);
    legend.removeChild(rowToRemove);
}

// ----- Drawing Functions -----
function EnableDrawing(id, enabled) {
    var entry = chartList.find(q => q.id === id);

    entry.drawingEnabled = enabled;
}

function CreateDrawing(id, lineChart, pane, priceVisible = false) {
    let chartEntry = chartList.find(q => q.id === id);

    let lineChartJson = JSON.parse(lineChart);

    let lineOptions = {
        color: lineChartJson.Color,
        lineWidth: lineChartJson.Width,
        pane: pane,
        priceLineVisible: priceVisible,
        lastValueVisible: priceVisible
    }

    let series = chartEntry.chart.addLineSeries(lineOptions);

    const lineData = lineChartJson.Series.map(entry => {
        return { time: entry.Timestamp, value: entry.Value, color: entry.Color }
    });

    series.setData(lineData);

    chartEntry.drawings[lineChartJson.Id] = series;
}

function RemoveDrawing(chartId, drawingId) {
    let chartEntry = chartList.find(chart => chart.id === chartId);

    let drawing = chartEntry.drawings[drawingId];

    chartEntry.chart.removeSeries(drawing);
    delete chartEntry.drawings[drawingId];


}

function RemoveAllDrawings(id) {
    let chartEntry = chartList.find(q => q.id === id);

    for (const key of Object.keys(chartEntry.drawings)) {
        let drawing = chartEntry.drawings[key];
        chartEntry.chart.removeSeries(drawing);
    }
    chartEntry.drawings = {};
}