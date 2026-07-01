/*
* TODO: temp. move to the .cshtml to get the values from the settings / theme variables.
* */

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
    routes: {
        addToCart: '/cart/addproduct'
    }
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


export class ElementError extends Error{
    constructor(messageOrOptions) {
       let msg = typeof messageOrOptions === 'string' ? messageOrOptions : '';
       if (typeof messageOrOptions === 'object'){
           const {component, identifier, element, expectedType} = messageOrOptions;
           msg = identifier;
           msg += element ? ` is not of type ${expectedType ?? 'HTMLElement'}` : ' not found';
           if (component){
               msg = `${component.name}: ${msg}`;
           }
       }
        super(msg);
    }
}

window.addEventListener('resize', debounce(() => {
    window.dispatchEvent(new CustomEvent('on:debounced-resize'))
}))
