let resizeScanner = new ResizeObserver(function () {
    var mainContainer = document.getElementsByClassName("mud-main-content")[0];
    var containerHeight = mainContainer.offsetHeight;

    var settingsContainer = document.getElementById("scannerSettings");
    var settingsContainerHeight = settingsContainer.offsetHeight;

    var tableContainerHeight = containerHeight - settingsContainerHeight;

    var tableContainer = document.getElementById("scannerTable");
    tableContainer.style.height = tableContainerHeight + "px";
});

function AddScannerResize() {
    var settingsContainer = document.getElementById("scannerSettings");
    resizeScanner.observe(settingsContainer);

    var settingsContainer = document.getElementById("scannerTable");
    resizeScanner.observe(settingsContainer);
}