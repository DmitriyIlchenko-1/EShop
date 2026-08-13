/*
* TODO: temp. move to the .cshtml to get the values from the settings / theme variables.
* */
import {Notyf} from "../lib/notyf/notyf.es.js";

window.theme = {
    mediaQueries: {
        sm: '(min-width: 600px)',
        md: '(min-width: 769px)',
        lg: '(min-width: 1024px)',
        xl: '(min-width: 1280px)',
        xxl: '(min-width: 1536px)'
    },
    device: {
        hasTouch: window.matchMedia('(any-pointer: coarse)').matches,
        hasHover: window.matchMedia(('(hover:hover)')).matches
    },
};


export const mediaQueryLarge = matchMedia('(width >= 769px)');

export function isMobileBreakpoint() {
    return !mediaQueryLarge.matches;
}

export function isDesktopBreakpoint() {
    return mediaQueryLarge.matches;
}

export const hoverAvailable = matchMedia("(hover:hover)");

export function canHover() {
    return hoverAvailable.matches;
}


(() => {
    const {mediaQueries} = theme;
    if (!mediaQueries)
        return;
    const mqKeys = Object.keys(mediaQueries);
    const mqLists = {};
    theme.mediaMatches = {};
    const handleMqChange = () => {
        const newMatches = mqKeys.reduce((acc, media) => {
            acc[media] = (mqLists[media] && mqLists[media].matches);
            return acc;
        }, {});
        Object.keys(newMatches).forEach(mqName => {
            theme.mediaMatches[mqName] = newMatches[mqName];
        });

        window.dispatchEvent(new CustomEvent('on:breakpoint-change'));
    };

    mqKeys.forEach(mq => {
        // populate mqLists with MediaQueryList for each mq.
        mqLists[mq] = window.matchMedia(mediaQueries[mq]);
        //do initial matching
        theme.mediaMatches[mq] = mqLists[mq].matches;
        try {
            mqLists[mq].addEventListener('change', handleMqChange);
        } catch (err) {
            mqLists[mq].addEventListener(handleMqChange);
        }
    })

})();


export function debounce(fn, wait = 300) {
    let tId;
    return (...args) => {
        clearTimeout(tId);
        tId = setTimeout(() => fn.apply(this, args), wait);
    };
}


export class ElementError extends Error {
    constructor(messageOrOptions) {
        let msg = typeof messageOrOptions === 'string' ? messageOrOptions : '';
        if (typeof messageOrOptions === 'object') {
            const {component, identifier, element, expectedType} = messageOrOptions;
            msg = identifier;
            msg += element ? ` is not of type ${expectedType ?? 'HTMLElement'}` : ' not found';
            if (component) {
                msg = `${component.name}: ${msg}`;
            }
        }
        super(msg);
    }
}

window.addEventListener('resize', debounce(() => {
    window.dispatchEvent(new CustomEvent('on:debounced-resize'))
}))

export function createAndSubmitForm(options) {
    const formId = `DynamicForm_${Math.random().toString().substring(2)}`;
    const form = Object.assign(document.createElement('form'), {
        method: options.method,
        action: options.url,
        id: formId,
    })
    const atfInput = Object.assign(document.createElement('input'), {
        type: 'hidden',
        name: '__RequestVerificationToken',
        value: getAntiforgeryToken(),
    })
    form.append(atfInput)
    document.querySelector('body').append(form);
    document.getElementById(formId).requestSubmit();
}

export function getAntiforgeryToken() {
    return document
        .querySelector(`meta[name="csrf-token"]`)
        .getAttribute('content');
}

export function getElementHtml(html, selector) {
    const template = document.createElement('template');
    template.innerHTML = html;
    const el = template.content.querySelector(selector);
    return el?.innerHTML ?? '';
}


export const notyf = new Notyf({
    duration: 5000,
    dismissible: true,
});

export function notifySuccess(msg) {
    if(msg === typeof(String) && !msg) return;
    notyf.success(msg);
    
}

export function notifyError(msg) {
    if(msg === typeof(String) && !msg) return;
    notyf.error(msg);
}
 
 
