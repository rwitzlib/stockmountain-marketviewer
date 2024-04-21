var clock;

function startTime(element, time) {
    if (time === null) {
        let liveTimeString = new Date().toLocaleTimeString('en-US', { hour: 'numeric', hour12: false, minute: 'numeric', second: 'numeric', second: 'numeric' });
        element.value = liveTimeString;


        clock = setTimeout(startTime.bind(null, element, null), 1000);
    }
    else {
        let date = new Date(time);

        let backtestTimeString = date.toLocaleTimeString('en-US', { hour: 'numeric', hour12: false, minute: 'numeric', second: 'numeric', second: 'numeric' });
        element.value = backtestTimeString;
        date.setSeconds(date.getSeconds() + 1);

        clock = setTimeout(startTime.bind(null, element, date), 1000);
    }
}

function stopTime() {
    clearTimeout(clock);
}

function getTime(element) {
    return element.value;
}