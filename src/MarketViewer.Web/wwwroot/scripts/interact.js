const pages = {};

function initializePage(pageId, dotnetObjectReference) {
    let page = {
        dotnetObjectReference: dotnetObjectReference
    };
    pages[pageId] = page;
}

function getPageIdFromArgumentId(argumentId) {
    let pageIds = Object.keys(pages);

    for (let i = 0; i < pageIds.length; i++) {
        let pageId = pageIds[i];

        let page = pages[pageId];

        let initialArgument = page.argument;

        let argument = findArgumentById(initialArgument, argumentId)

        if (argument != undefined || argument != null) {
            return pageId;
        }
    }
    return null;
}

function addInitialArgument(pageId, argumentId) {
    let page = pages[pageId];
    page.argument = {
        id: argumentId,
        filters: {}
    }
}

function removeInitialArgument(pageId) {
    delete pages[pageId].argument;
}

function addArgumentToArgument(pageId, outerArgumentId, innerArgumentId) {
    let initialArgument = pages[pageId].argument;

    let argument = findArgumentById(initialArgument, outerArgumentId);

    if (argument === null) {
        // TODO throw error?
        return;
    }

    argument.argument = {
        id: innerArgumentId,
        filters: {}
    };
}

function findArgumentById(argument, argumentIdToFind) {
    if (argument === undefined || argument.id === undefined) {
        // TODO throw error?
        return null;
    }

    if (argument.id === argumentIdToFind) {
        return argument;
    }

    if (argument.argument === undefined || argument.argument.id === undefined) {
        //TODO throw error?
        return null;
    }

    
    if (argument.argument.id === argumentIdToFind) {
        return argument.argument;
    }

    return findArgumentById(argument.argument, argumentIdToFind);    
}

function removeArgumentFromArgument(pageId, outerArgumentId) {
    let initialArgument = pages[pageId].argument;

    let argument = findArgumentById(initialArgument, outerArgumentId);

    if (argument === null) {
        // TODO throw error?
        return;
    }

    delete argument.argument
}

function addFilterToArgument(pageId, argumentId, filterId) {
    let initialArgument = pages[pageId].argument;

    let argument = findArgumentById(initialArgument, argumentId);

    if (argument === null) {
        // TODO throw error?
        return;
    }

    argument.filters[filterId] = filterId;
}

function moveFilter(pageId, sourceArgumentId, destinationArgumentId, filterId) {
    addFilterToArgument(pageId, destinationArgumentId, filterId);
    removeFilterFromArgument(pageId, sourceArgumentId, filterId);

    let destinationElement = document.getElementById(destinationArgumentId);
    let filterElement = document.getElementById(filterId);

    destinationElement.appendChild(filterElement);

    filterElement.style.transform = 'translate(0px, 0px)';

    filterElement.setAttribute('data-x', 0);
    filterElement.setAttribute('data-y', 0);

    pages[pageId].dotnetObjectReference.invokeMethod("MoveFilter", sourceArgumentId, destinationArgumentId, filterId);

    return;
}

function removeFilterFromArgument(pageId, argumentId, filterId) {
    let initialArgument = pages[pageId].argument;

    let argument = findArgumentById(initialArgument, argumentId);

    if (argument === null) {
        // TODO throw error?
        return;
    }

    delete argument.filters[filterId];
}

interact('.draggable')
    .draggable({
        inertia: true,
        modifiers: [],
        autoScroll: true,

        listeners: {
            // call this function on every dragmove event
            move: dragMoveListener,

            // call this function on every dragend event
            end(event) {
                var textEl = event.target.querySelector('p')

                textEl && (textEl.textContent =
                    'moved a distance of ' +
                    (Math.sqrt(Math.pow(event.pageX - event.x0, 2) +
                        Math.pow(event.pageY - event.y0, 2) | 0))
                        .toFixed(2) + 'px')
            }
        }
    })

function dragMoveListener(event) {
    var target = event.target
    // keep the dragged position in the data-x/data-y attributes
    var x = (parseFloat(target.getAttribute('data-x')) || 0) + event.dx
    var y = (parseFloat(target.getAttribute('data-y')) || 0) + event.dy

    // translate the element
    target.style.transform = 'translate(' + x + 'px, ' + y + 'px)'

    // update the posiion attributes
    target.setAttribute('data-x', x)
    target.setAttribute('data-y', y)
}

// this function is used later in the resizing and gesture demos
window.dragMoveListener = dragMoveListener

// enable draggables to be dropped into this
interact('.dropzone').dropzone({
    // only accept elements matching this CSS selector
    accept: '.filter',
    // Require a 75% element overlap for a drop to be possible
    overlap: 0.75,

    // listen for drop related events:

    ondropactivate: function (event) {
        // add active dropzone feedback

        var draggableElement = event.relatedTarget
        var dropzoneElement = event.target

        event.target.classList.add('drop-active')

        //if (draggableElement.parentElement.id != dropzoneElement.id) {
        //}
    },
    ondragenter: function (event) {
        // feedback the possibility of a drop

        var draggableElement = event.relatedTarget
        var dropzoneElement = event.target

        dropzoneElement.classList.add('drop-target')
        draggableElement.classList.add('can-drop')

        //if (event.relatedTarget.parentElement.id != dropzoneElement.id) {
            
        //}

    },
    ondragleave: function (event) {
        // remove the drop feedback style
        event.target.classList.remove('drop-target')
        event.relatedTarget.classList.remove('can-drop')
    },
    ondrop: function (event) {
        var draggableElement = event.relatedTarget
        var startDropZone = event.relatedTarget.parentElement
        var endDropZone = event.target


        if (event.relatedTarget.classList.contains("filter")) {
            let pageId = getPageIdFromArgumentId(startDropZone.id);

            moveFilter(pageId, startDropZone.id, endDropZone.id, draggableElement.id)
        }
    },
    ondropdeactivate: function (event) {
        // remove active dropzone feedback
        event.target.classList.remove('drop-active')
        event.target.classList.remove('drop-target')
    }
})

interact('.drag-drop')
    .draggable({
        inertia: true,
        modifiers: [
        ],
        autoScroll: true,
        // dragMoveListener from the dragging demo above
        listeners: { move: dragMoveListener }
    })