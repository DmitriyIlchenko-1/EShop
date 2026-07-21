import {ElementError, canHover, isDesktopBreakpoint, isMobileBreakpoint} from "./utilities.js";

/*TODO:
*  1. Read about progressive einhansment. is showHideButton.removeAttribute('hidden'); all what's needed to be done?
* 2. Get back to the UK gov and read the rest (no matter what it is about, not only about autofill or autocomplete) about this component after reading about autofill.
  */
class PasswordInputComponent extends HTMLElement {
    constructor() {
        super();
        const input = this.querySelector('.js-password-input-input');
        if (!(input instanceof HTMLInputElement)) {
            throw new ElementError({
                component: PasswordInputComponent,
                element: input,
                expectedType: "HTMLInputElement",
                identifier: 'Form control `.js-password-input-input`'
            });
        }
        if (input.type !== 'password') {
            throw new ElementError('Password input: Form control .js-password-input-input must be of type `password`');
        }
        const showHideButton = this.querySelector('.js-password-input-toggle');
        if (!(showHideButton instanceof HTMLButtonElement)) {
            throw new ElementError({
                component: PasswordInputComponent,
                element: showHideButton,
                expectedType: "HTMLButtonElement",
                identifier: 'Button `.js-password-input-toggle`'
            });
        }
        if (showHideButton.type !== 'button') {
            throw new ElementError('Password input: Button .js-password-input-toggle must be of type `password`');
        }
        showHideButton.removeAttribute('hidden');
        this.quantityInput = input;
        this.showHideButton = showHideButton;
        const screenReaderStatusMessage = document.createElement('div');
        screenReaderStatusMessage.setAttribute('aria-live', 'polite');
        screenReaderStatusMessage.classList.add('visually-hidden');
        this.screenReaderStatusMessage = screenReaderStatusMessage;
        this.quantityInput.insertAdjacentElement('afterend', screenReaderStatusMessage);
        this.showHideButton.addEventListener('click', this.toggle.bind(this));
        if (this.quantityInput.form) {
            this.quantityInput.form.addEventListener('submit', () => this.hide());
        }
        window.addEventListener('pageshow', event => {
            if (event.persisted && this.quantityInput.type !== 'password') {
                this.hide();
            }
        });
        this.hide();

    }

    toggle(e) {
        e.preventDefault();
        if (this.quantityInput.type === 'password') {
            this.show();
        } else {
            this.hide();
        }
    }

    show() {
        this.setType('text');
    }

    hide() {
        this.setType('password');
    }

    setType(type) {
        if (type === this.quantityInput.type) return;
        this.quantityInput.setAttribute('type', type);
        const isHidden = type === 'password';
        this.showHideButton.setAttribute('aria-label', `${isHidden ? 'Show' : 'Hide'} password.`)
        this.screenReaderStatusMessage.innerText = `Your password is ${isHidden ? 'hidden' : 'visible'}`;


    }
}

if (!customElements.get('password-input-component')) {
    customElements.define('password-input-component', PasswordInputComponent);
}

/*TODO:
1. Implement a focus trap inside the mobile navigation when opened. 
*/

class MainMenu extends HTMLElement {
    constructor() {
        super();

        this.mainToggle = this.querySelector('.main-menu__toggle');
        this.mainContent = this.querySelector('.main-menu__content');
        this.nav = this.querySelector('.main-nav');
        this.firstLevelMenuLinks = this.querySelectorAll('.js-nav-hover');
        this.overlay = document.querySelector('.js-overlay');
        this.firstLevelSingleLinks = this.querySelectorAll('.main-nav__link--orphan');
        this.elementsClosingMenus = document.querySelectorAll('.js-closes-menu');
        this.sidebarLinks = this.querySelectorAll('.js-sidebar-hover');

        this.init();
        this.addListeners();
    }


    disconnectedCallback(){
        window.removeEventListener('focusin', this.focusOutHandler);
        window.removeEventListener('on:breakpoint-change', this.breakpointChangeHandler);
    }

    addListeners() {
        this.breakpointChangeHandler = this.breakpointChangeHandler || this.init.bind(this);
        window.addEventListener('on:breakpoint-change', this.breakpointChangeHandler);
        this.mainToggle.addEventListener('click', this.handleMainMenuToggle.bind(this))
        this.mainToggle.addEventListener('transitionend', this.handleMainContentTransition.bind(this))
        this.nav.addEventListener('click', this.handleNavClick.bind(this));
        this.nav.addEventListener('transitionend', this.handleTransition.bind(this));
        window.addEventListener('focusin', this.focusOutHandler);
    }


    init(event) {

        if (!event) {
            if (!theme.mediaMatches.md) {
                this.mainContent.classList.remove('is-open', 'is-visible');
                this.mainToggle.setAttribute('aria-expanded', 'false');
            }
        } else {
            //event isn't null when called by the breakpoint-change event. We close the nav altogether.
            this.closeMainMenu(false);
            this.childNavOpen = false;
            const activeDisclosure = this.nav.querySelector('.main-nav__item--dropdown.is-open');
            if (activeDisclosure)
                this.close(activeDisclosure, false);
            if (this.overlayOpen)
                this.toggleOverlay(false);

        }

        if (theme.device.hasHover) {
            this.mouseEnterNavDropdownHandler = this.mouseEnterNavDropdownHandler
                || (e => {
                    this.menuLinkTimeout = setTimeout(this.openMenuFromMouseEnter.bind(this, e.target), Number.parseInt(this.dataset.menuSensitivity, 10));
                });
            //if a mouse leave earlier than the specified timeout value.
            this.mouseLeaveNavDropdownHandler = this.mouseLeaveNavDropdownHandler
                || (e => {
                    if (this.menuLinkTimeout) {
                        clearTimeout(this.menuLinkTimeout);
                    }
                });
            this.mouseEnterNavCloserHandler = this.mouseEnterNavCloserHandler
                || this.handleClose.bind(this);

            this.mouseEntersSingleLinkHandler = this.mouseEntersSingleLinkHandler
                || this.handleMouseEntersSingleLink.bind(this);
            this.mouseLeavesSingleLinkHandler = this.mouseLeavesSingleLinkHandler
                || this.handleMouseLeavesSingleLink.bind(this);

            this.mouseEnterMenuCloserHandler = this.mouseEnterMenuCloserHandler
                || this.handleClose.bind(this);

            this.focusOutHandler = this.focusOutHandler
                || this.handleFocusOut.bind(this);
        }

        //Bind event handles for mouse enter & leave a main menu link for desktop 
        if (!this.mouseOverListening && theme.mediaMatches.md) {
            this.firstLevelMenuLinks.forEach(l => {
                l.addEventListener('mouseenter', this.mouseEnterNavDropdownHandler);
                l.addEventListener('mouseleave', this.mouseLeaveNavDropdownHandler);
            });
            this.firstLevelSingleLinks.forEach(l => {
                l.addEventListener('mouseenter', this.mouseEntersSingleLinkHandler);
                l.addEventListener('mouseleave', this.mouseLeavesSingleLinkHandler);
            });
            this.elementsClosingMenus.forEach(el => {
                el.addEventListener('mouseenter', this.mouseEnterMenuCloserHandler);
            });
            this.mouseOverListening = true;
        } else if (this.mouseOverListening && !theme.mediaMatches.md) {
            this.firstLevelMenuLinks.forEach(l => {
                l.removeEventListener('mouseenter', this.mouseEnterNavDropdownHandler);
                l.removeEventListener('mouseleave', this.mouseLeaveNavDropdownHandler);
            });
            this.firstLevelSingleLinks.forEach(l => {
                l.removeEventListener('mouseenter', this.mouseEntersSingleLinkHandler);
                l.removeEventListener('mouseleave', this.mouseLeavesSingleLinkHandler);
            });
            this.elementsClosingMenus.forEach(el => {
                el.removeEventListener('mouseenter', this.mouseEnterMenuCloserHandler);
            });
            this.mouseOverListening = false;
        }


        if (this.sidebarLinks) {
            if (!this.mouseOverSidebarListening && theme.mediaMatches.md) {
                this.sidebarLinks.forEach(el => {
                    el.addEventListener('mouseenter', MainMenu.handleSidenavMenuToggle);
                    el.addEventListener('focusin', MainMenu.handleSidenavFocusIn);
                });
                this.mouseOverSidebarListening = true;
            } else if (this.mouseOverSidebarListening && !theme.mediaMatches.md) {
                this.sidebarLinks.forEach(el => {
                    el.removeEventListener('mouseenter', MainMenu.handleSidenavMenuToggle);
                    el.removeEventListener('focusin', MainMenu.handleSidenavFocusIn);
                });
                this.mouseOverSidebarListening = false;
            }
        }

    }

    /*
    * Called to finalize the transition when the mobile nav menu closes. 
    * If the is-visible isn't present anymore, 
    * we can finally remove is-open to remove the main menu from the dom
    *  - that's what this method does.
    *We do this in this separate method to give the transition some time to play
    *  before removing is-open.
    */
    handleMainContentTransition(event) {
        //console.log('handleMainContentTransition');
        const parentToggle = event.target.closest('.main-menu__toggle');
        if (parentToggle !== this.mainToggle || event.propertyName !== 'opacity')
            return;

        if (!this.mainContent.classList.contains('is-visible')) {
            this.mainContent.classList.remove('is-open');
            this.opener = null;
        }
    }

    /*
    * Invoked when nav toggle button is clicked to open/close the menu navigation on mobile.
    */
    handleMainMenuToggle(event) {
        console.log('handleMainMenuToggle fired');
        this.opener = this.mainToggle;
        if (!this.mainContent.classList.contains('is-open')) {
            this.openMainMenu();
        } else {
            console.log('calling closeMainMenu from handleMainMenuToggle')
            this.closeMainMenu(true);
        }
    }

    /**
     * Opens the main menu content hidden in the mobile view.
     * (the one you open with the hamburger button)
     */
    openMainMenu() {
        this.mainContent.classList.add('is-open');

        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                this.mainContent.classList.add('is-visible');
            })
        });


        this.mainToggle.setAttribute('aria-expanded', 'true');
        document.body.classList.add('overflow-hidden');

    }


    /**
     * Closes the main menu content hidden in the mobile view.
     * (the one you closes with the hamburger button)
     */
    closeMainMenu(transition = true) {
        console.log('closeMainMenu has been called');
        console.log(`transition value passed is ${transition}`);
        this.mainContent.classList.remove('is-visible');
        this.mainToggle.setAttribute('aria-expanded', 'false');
        document.body.classList.remove('overflow-hidden');
        //if transition is true, handleMainContentTransition will remove .is-open after the transition's over.
        if (!transition) {
            console.log('remove(is-open)');
            this.mainContent.classList.remove('is-open');
            this.opener = null;
        }

    }

    /**
     * Updates sidebar items found in a child nav.
     */
    static handleSidenavMenuToggle(event, listElem = event.target) {
        const container = listElem.closest('.child-nav');
        const lastSidenavElem = container.querySelector('.is-visible');
        if (lastSidenavElem) {
            lastSidenavElem.classList.remove('is-visible');
        }
        listElem.classList.add('is-visible');

        const openPanel = listElem.querySelector('.child-nav__panel');
        if (openPanel) {
            container.style.setProperty(
                '--sidebar-height',
                `${Number.parseInt(openPanel.getBoundingClientRect().height, 10)}px`
            );
        }

    }

    static handleSidenavFocusIn(event) {
        if (event.currentTarget.querySelector('.child-nav__panel')) {
            MainMenu.handleSidenavMenuToggle(null, event.currentTarget);
        }
    }

    /**
     * Closes the menu if the nav looses focus.
     */
    handleFocusOut(event) {
        if (!this.contains(event.target) && this.overlayOpen) {
            console.log('handleFocusOut fired!')
            this.handleClose();
        }
    }

    /**
     * Toggles visibility of the background overlay
     * @param show - Show the background overlay.
     */
    toggleOverlay(show) {
        this.overlayOpen = show;
        this.overlay.classList.toggle('is-visible', show);
        if (show) {
            this.closeHandler = this.closeHandler || this.handleClose.bind(this);

            //add event listeners for closing the nav with ESC which is's open
            this.nav.addEventListener('keyup', this.closeHandler);
            this.overlay.addEventListener('click', this.closeHandler);

            if (theme.mediaMatches.md) {
                this.overlay.addEventListener('mouseenter', this.closeHandler);
            }
        } else {
            this.nav.addEventListener('keyup', this.closeHandler);
            this.overlay.removeEventListener('click', this.closeHandler);
            if (!theme.mediaMatches.md) {
                this.overlay.removeEventListener('mouseenter', this.closeHandler);
            }
        }
    }

    openMenuFromMouseEnter(hoverElement) {
        const disclosure = hoverElement.closest('.main-nav__item--dropdown');
        if (!disclosure.classList.contains('is-open')) {
            const activeDisclosure = this.nav.querySelector('.main-nav__item--dropdown.is-open');
            if (activeDisclosure && activeDisclosure !== disclosure) {
                this.close(activeDisclosure);
            } else {
                this.toggleOverlay(!this.overlayOpen);
            }

            MainMenu.open(disclosure);
        }
    }

    handleMouseEntersSingleLink() {
        this.singleLinkTimeout = setTimeout(() => {
            this.handleClose();
        }, Number.parseInt(this.dataset.menuSensitivity, 10));
    }

    handleMouseLeavesSingleLink() {
        if (this.singleLinkTimeout) {
            clearTimeout(this.singleLinkTimeout);
        }
    }


    handleClose(event) {
        if (event && event.type === 'keyup' && event.key !== 'Escape')
            return;

        if (isDesktopBreakpoint()) {
            const disclosure = this.nav.querySelector('.main-nav__item--dropdown.is-open')
            if (disclosure) {
                this.close(disclosure);
                this.toggleOverlay(false);
                this.childNavOpen = false;
            }

        }
    }

    handleNavClick(event) {
        const mainMenuContent = event.target.closest('.main-menu__content');
        let el = event.target;
        el = event.target.closest('.js-toggle', '.js-back') || el;

        if (!el.matches('.js-toggle, .js-back'))
            return;


        if (el.matches('.js-toggle')) {
            const childToggle = el.closest('.child-nav__item--toggle');
            if (childToggle) {
                this.opener = el;
                if (!childToggle.classList.contains('is-open')) {
                    console.log(`handleNavClick: open`)
                    MainMenu.open(childToggle, false);
                } else {
                    console.log(`handleNavClick: closed`)
                    this.close(childToggle, true);
                }
                return;
            }
            const navDropdown = el.closest('.main-nav__item--dropdown');
            if (navDropdown) {
                this.opener = el;
                if (!navDropdown.classList.contains('is-open')) {
                    const activeDisclosure = this.nav.querySelector('.main-nav__item--dropdown.is-open');
                    if (activeDisclosure && activeDisclosure !== navDropdown) {
                        this.close(activeDisclosure);
                    } else if (isDesktopBreakpoint()) {
                        this.toggleOverlay(!this.overlayOpen);
                    }
                    MainMenu.open(navDropdown);
                } else {
                    this.close(navDropdown, true);
                    this.toggleOverlay(false);
                }
            }

        } else if (el.matches('.js-back')) {
            const navDropdown = el.closest('.main-nav__item--dropdown');
            if (navDropdown)
                this.close(navDropdown, true);
        }
    }

    handleTransition(event) {
        if (event.target.matches('.main-nav__child')) {
            const dropdown = event.target.closest('.main-nav__item--dropdown');
            if (dropdown && dropdown.classList.contains('is-open') && !dropdown.classList.contains('is-visible')) {
                console.log('handleTransition: remove(is-open)');
                dropdown.classList.remove('is-open');
                this.opener = null;
            }
        }
    }

    static open(el, isMainMenuOpen = true) {
        console.log("OPEN: is called")
        el.classList.add('is-open');
        el.querySelector('.main-nav__toggle')
            ?.setAttribute('aria-expanded', 'true');

        // We call this first rAF because we first want the display value to change from none to block and only then apply styles to get the transition work.
        // This is simply because when the value of display is changed from none to block, no transition will run the first time e.g. in the same frame.
        requestAnimationFrame(() => {

            const panel = el.querySelector(':scope > .child-nav__panel');
            const needsHeight = panel && isMobileBreakpoint();
            if (needsHeight) {
                console.log(`OPEN: Setting panel's height to 0 was ${panel.style.height}`);
                panel.style.height = '0';

                requestAnimationFrame(() => {
                    console.log(`OPEN: Setting panel's height to scrollHeight ${panel.scrollHeight} before was ${panel.style.height}`);
                    panel.style.height = `${panel.scrollHeight}px`;
                    panel.addEventListener('transitionend', (e) => {
                        if (e.target !== panel || e.propertyName !== "height") {
                            return;
                        }
                        console.log(`OPEN: open -> transition-end: ${e.propertyName}`);
                        panel.style.height = '';
                    }, {once: true});
                })
            } else {
                const sidebarContainer = el.querySelector('.child-nav');
                const openPanel = sidebarContainer?.querySelector('.js-sidebar-hover.is-visible .child-nav__panel')
                if (openPanel) {
                    sidebarContainer.style.setProperty('--sidebar-height', `${openPanel.scrollHeight}px`);
                }
                requestAnimationFrame(() => {
                    el.classList.add('is-visible')
                })
            }


        });

        if (isMainMenuOpen) {
            if (isDesktopBreakpoint()) {
                document.body.classList.add('overflow-hidden');
            }
        }

    }

    close(el, transition = true) {
        el.querySelector('.main-nav__toggle')
            ?.setAttribute('aria-expanded', 'false');
        console.log("CLOSE: is called")
        const panel = el.querySelector(':scope > .child-nav__panel');
        const needsHeight = panel && isMobileBreakpoint();
        // will the browser set any classes / styles on an element
        // before the call to requestAnimationFrame. 
        // Is it just to run a transition,
        // that we need to call requestAnimationFrame?,
        // meaning that the dom gets updated always 
        // as we append new classes / styles in js as we go 
        // [no need to wait till requestAnimationFrame is running to see a css class etc get applied to an element]?? 

        if (transition && needsHeight) {
            console.log(`CLOSE: panel gets ${panel.scrollHeight}px of height, before was ${panel.style.height}`);
            panel.style.height = `${panel.scrollHeight}px`;
            panel.offsetHeight;
            requestAnimationFrame(() => {
                console.log(`CLOSE: panel gets \`0\` of height, before was ${panel.style.height}`);
                panel.style.height = '0';
            });


            panel.addEventListener('transitionend', (e) => {
                if (e.target !== panel || e.propertyName !== "height") {
                    return;
                }
                console.log(`CLOSE: open -> transition-end: ${e.propertyName}`);
                console.log(`CLOSE: (inside transitionend): panel gets a \`\` of height`);
                panel.style.height = '';
                el.classList.remove('is-open');
                this.opener = null;
            }, {once: true});
        } else if (isDesktopBreakpoint() && transition) {
            el.classList.remove('is-visible');
        } else {
            el.classList.remove('is-visible', 'is-open');
            this.opener = null;
        }


        if (isDesktopBreakpoint() && !el.closest('.main-nav__child')) {
            document.body.classList.remove('overflow-hidden');
        }
    }


}


if (!customElements.get('main-menu')) {
    customElements.define('main-menu', MainMenu);
}


class QuantitySelector extends HTMLElement {
    constructor() {
        super();
        this.quantityInput = this.querySelector('input[type="number"]');
        this.quantityInput.addEventListener('blur', this.setQuantity.bind(this));
        this.minusBtn = this.querySelector('.quantity-minus');
        this.plusBtn = this.querySelector('.quantity-plus');
        this.minusBtn.addEventListener('click', this.decreaseQuantity.bind(this));
        this.plusBtn.addEventListener('click', this.increaseQuantity.bind(this));
        this.init();
    }

    init() {
        const {min, max, step} = this.getCurrentValues();
        this.updateConstraints(min, max, step);
    }


    updateQuantity(stepMultiplier) {
        const {min, step, value} = this.getCurrentValues();
        const eMax = this.getEffectiveMax();
        const nextValue = Math.min(
            eMax ?? Infinity,
            Math.max(min, value + step * stepMultiplier)
        );
        this.quantityInput.value = nextValue.toString();
        this.updateButtonStates();
    }

    getEffectiveMax() {
        const {min, max} = this.getCurrentValues();
        if (max === null) return null;
        return Math.max(max, min);
    }

    increaseQuantity(e) {
        if (!e.target.closest('.quantity-plus')) return;
        e.preventDefault();
        this.updateQuantity(1);
    }

    decreaseQuantity(e) {
        if (!e.target.closest('.quantity-minus')) return;
        e.preventDefault();
        this.updateQuantity(-1);
    }

    updateConstraints(min, max, step) {
        const currentValue = parseInt(this.quantityInput.value) || 0;

        this.quantityInput.min = min;
        if (max) {
            this.quantityInput.max = max;
        } else {
            this.quantityInput.removeAttribute('max');
        }
        this.quantityInput.step = step;

        const newMin = parseInt(min) || 1;
        const newStep = parseInt(step) || 1;
        const effectiveMax = this.getEffectiveMax();

        // Snap to valid increment if not already aligned
        let newValue = currentValue;
        if ((currentValue - newMin) % newStep !== 0) {
            // Snap DOWN to closest valid increment
            newValue = newMin + Math.floor((currentValue - newMin) / newStep) * newStep;
        }

        // Ensure value is within bounds
        newValue = Math.max(newMin, Math.min(effectiveMax ?? Infinity, newValue));

        if (newValue !== currentValue) {
            this.quantityInput.value = newValue.toString();
        }

        this.updateButtonStates();
    }

    /**
     * Assigns the entered value to the input after validating it
     */
    setQuantity(e){
        if (!(e.target instanceof HTMLInputElement)) return;
        e.preventDefault();
        const {min, step} = this.getCurrentValues() ;
        const eMax = this.getEffectiveMax();
        const quantity = Math.min(
            eMax ?? Infinity,
            Math.max(min, parseInt(e.target.value))
        );
        if ((quantity - min) % step !== 0) {
            // if the step increment validation fails, we still assign the value, 
            // though, we also trigger native HTML navigation.
            this.quantityInput.value = quantity.toString();
            this.quantityInput.reportValidity();
            return;
        }

        this.quantityInput.value = quantity.toString();
        this.updateButtonStates();
    }

    updateButtonStates() {
        const {min, value} = this.getCurrentValues();
        const eMax = this.getEffectiveMax();
        this.minusBtn.disabled = value <= min;
        this.plusBtn.disabled = value >= eMax && eMax !== null;
    }

    getQuantity(){
        return this.quantityInput.value;
    }

    getCurrentValues() {
        return {
            min: parseInt(this.quantityInput.min) || 1,
            max: parseInt(this.quantityInput.max) || null,
            step: parseInt(this.quantityInput.step) || 1,
            value: parseInt(this.quantityInput.value) || 0
        }
    }
}

if (!customElements.get('quantity-selector')) {
    customElements.define('quantity-selector', QuantitySelector);
}

class TabbedContent extends HTMLElement {
    constructor() {
        super();
        this.tablist = this.querySelector('[role="tablist"]');
        this.isVerticalTablist = this.tablist.getAttribute('aria-orientation') === 'vertical';
        this.activeTab = this.tablist.querySelector('[aria-selected="true"]');
        this.tabs = this.querySelectorAll('[role="tab"]');
        this.panels = this.querySelectorAll('[role="tabpanel"]');


        if (!this.activeTab) {
            this.activateTab(this.tabs[0]);
        }
        //tabindex="-1" is set when a tab isn't selected so that only the selected tab is in the page TAB sequence, which is always only one at any given  moment.
        //We also deselect all the other tabs so we don't have to rely on the HTML markup to properly add hidden and other attributes to each inactive tab.
        this.tabs.forEach(x => {
            if (x !== this.activeTab) {
                x.setAttribute('tabindex', '-1');
                TabbedContent.setTabState(x, false);
            }
        });

        this.addListeners();
    }

    addListeners() {
        this.tablist.addEventListener('keydown', this.handleKeydown.bind(this));
        this.tablist.addEventListener('click', this.handleClick.bind(this));
    }


    /**
     * Activates the clicked tab.
     * @param e - the click event
     */
    handleClick(e) {
        const tab = e.target.closest('[role="tab"]');
        if (!tab || tab === this.activeTab) return;
        this.activateTab(tab);
    }

    /**
     * Responds to the 'keydown' event on the tablist by orchestrating which tab becomes active.
     * @param e - the 'keydown' event carrying info like what key has been pressed.
     */
    handleKeydown(e) {
        switch (e.key) {
            case 'ArrowLeft':
            case 'ArrowRight':
                e.preventDefault();
                if (!this.isVerticalTablist) {
                    this.switchTabOnKeyPress(e.key);
                }
                break;
            case 'ArrowUp':
            case 'ArrowDown':
                e.preventDefault();
                if (this.isVerticalTablist) {
                    this.switchTabOnKeyPress(e.key);
                }
                break;
            case 'Home':
                e.preventDefault();
                if (this.activeTab !== this.tabs[0]) {
                    this.activateTab(this.tabs[0]);
                }
                break;
            case 'End':
                e.preventDefault();
                if (this.activeTab !== this.tabs[this.tabs.length - 1]) {
                    this.activateTab(this.tabs[this.tabs.length - 1]);
                }

        }
    }

    /**
     * Determines which particular tab becomes active and calls the corresponding methods to activate it.
     * @param key - the key pressed.
     */
    switchTabOnKeyPress(key) {
        if (key === 'ArrowRight' || key === 'ArrowDown') {
            // Loop around if adjacent tab doesn't exist.
            if (this.activeTab === this.tabs[this.tabs.length - 1]) {
                // move to the very first tab from the vary last one.
                this.activateTab(this.tabs[0]);
            } else {
                // move to the next tab to the right since it's present
                this.activateTab(this.activeTab.nextElementSibling);
            }
        } else if (key === 'ArrowLeft' || key === 'ArrowUp') {
            // Loop around if adjacent tab doesn't exist.
            if (this.activeTab === this.tabs[0]) {
                this.activateTab(this.tabs[this.tabs.length - 1]);
            } else {
                // move to the next tab to the left since it's present
                this.activateTab(this.activeTab.previousElementSibling);
            }
        }
    }

    /**
     * Moves focus to tabToActivate, activates the newly focused tab (tabToActivate).
     * @param tabToActivate - tab to activate
     */
    activateTab(tabToActivate) {
        //first deactivate the currently active tab.
        this.deactivateTab();

        TabbedContent.setTabState(tabToActivate, true);
        // make the active tab part of the page Tab sequence by simply removing tabindex since buttons are focusable by default.
        tabToActivate.removeAttribute('tabindex');
        this.activeTab = tabToActivate;

        //this check is to prevent the active tab from receiving focus when the page is first loaded.
        // When a tabbed interface is initialized, we only need to activate one of the tabs, not focus it to adhere to the initial page's TAB sequence
        if (document.activeElement.matches('.tablist__tab')) {
            //move focus to this newly focused tab
            this.activeTab.focus();
        }
    }

    /**
     * Deactivates the currently active tab.
     */
    deactivateTab() {
        if (!this.activeTab) return;
        TabbedContent.setTabState(this.activeTab, false);

        /*
        * Only the currently selected tab is part of the page Tab sequence.
        * It is to make it possible to focus down on the active tab's tabpanel right away
        * rather than have to go through the rest of the tab using TAB to finally reach the tabpanel's content,
        * which is more convenient for people using screen readers or relaying on keyboard navigation only. 
        * This is why ArrowLeft & ArrowRight are the only keys used to navigate through the tabs at any time as it doesn't stop users from focusing on the tabpanel's content.
        * */
        this.activeTab.setAttribute('tabindex', '-1');
        this.activeTab = null;
    }

    /**
     * Toggles the state of the given tab and its corresponding tabpanel.
     * @param tab - the given tab
     * @param active - the state to switch to.
     */
    static setTabState(tab, active) {
        tab.setAttribute('aria-selected', active);
        const panelId = tab.getAttribute('aria-controls');
        document.getElementById(panelId).hidden = !active;
    }
}

if (!customElements.get('tabbed-content')) {
    customElements.define('tabbed-content', TabbedContent);
}

class ProductComparisonGrid extends HTMLElement {

}

if (!customElements.get('product-comparison-grid')) {
    customElements.define('product-comparison-grid', ProductComparisonGrid);
}
 


