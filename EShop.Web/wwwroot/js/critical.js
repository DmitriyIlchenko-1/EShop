import {debounce} from "./utilities.js";


function setViewportHeight() {
    document.documentElement.style.setProperty('--viewport-height', `${window.innerHeight}px`);
}

function setScrollbarWidth() {
    document.documentElement.style.setProperty('--scrollbar-width',
        `${window.innerWidth - document.documentElement.clientWidth}px`);
}

function setDimensionVariables() {
    setViewportHeight();
    setScrollbarWidth();
}


document.addEventListener('DOMContentLoaded', setDimensionVariables);
window.addEventListener('resize', debounce(setDimensionVariables, 300));
setTimeout(setViewportHeight, 3000);