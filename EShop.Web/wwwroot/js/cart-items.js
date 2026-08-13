import {debounce, getElementHtml, notifyError} from "./utilities.js";
import {ProductForm} from './product-form.js';

class CartItems extends HTMLElement {
    #abortController;

    constructor() {
        super();
        this.updateCartUrl = this.closest('.js-cart-form').dataset.updateCartUrl;
        this.removeCartItemUrl = this.closest('.js-cart-form').dataset.removeCartItemUrl;
        this.cartDrawer = document.getElementById('cart-drawer');
        this.init();
    }

    init() {
        this.addEventListener('on:quantity-selector-update', debounce(this.handleChange.bind(this)));
        this.addEventListener('click', this.handleRemove.bind(this));
    }

    async handleRemove(e) {
        const btn = e.target.closest(`[name="remove"]`);
        if (!btn) return;
        const cartItem = btn.closest('.js-cart-item');
        await this.updateQuantity(cartItem.dataset.cartItemId, 0, document.activeElement.name, true)
    }

    async handleChange(e) {
        const cartItem = e.target.closest('.js-cart-item');
        if (cartItem && cartItem.dataset.cartItemId && cartItem.dataset.cartItemId > 0) {
            await this.updateQuantity(cartItem.dataset.cartItemId, e.quantity, document.activeElement.name);
        }
    }


    async updateQuantity(cartItemId, newQuantity, name, isRemoved = false) {
        const cartItemErrorId = `cart-item-error-${cartItemId}`;
        document.querySelectorAll(`.cart-item__error:not([id="${cartItemErrorId}"])`)
            .forEach(errorEl => {
                errorEl.hidden = true;
                errorEl.innerHTML = '';
            });

        const urlParams = new URLSearchParams({cartItemId, newQuantity});
        const partials = this.getPartialsToRender();
        partials.forEach(partial => {
            if (urlParams.has("requestedPartials", partial.name)) return;
            urlParams.append("requestedPartials", partial.name);
        })
        this.#abortController?.abort();
        this.#abortController = new AbortController();
        const fetchOptions = {
            method: 'POST',
            body: urlParams,
            signal: this.#abortController.signal
        };
        try {
            let response;
            if (!isRemoved) {
                response = await fetch(this.updateCartUrl, fetchOptions);
            } else {
                response = await fetch(this.removeCartItemUrl, fetchOptions);
            }

            const data = await response.json();
            if (!response.ok) {
                notifyError(response.statusText)
            }

            this.finalizeUpdating(data, cartItemId, name);
            this.updateNotifications(cartItemErrorId, data.message);


        } catch (error) {
            if (error.name === 'AbortError') {
                console.warn('Fetch has been aborted by user.');
            } else {
                console.error(error);
            }
        }
    }

    finalizeUpdating(data, cartItemId, name) {

        const drawerContent = this.cartDrawer ? this.cartDrawer.querySelector('.drawer__content') : null;

        const cartCount = data.cartCount;
        if (this.cartDrawer) {
            drawerContent.classList.toggle('drawer__content--flex', cartCount === 0);
        } else if (cartCount === 0) {
            this.closest('.cart').classList.add('cart--empty');
        }

        // Redisplay partials
        this.getPartialsToRender().forEach(partialSection => {
            const pEl = document.getElementById(partialSection.id);
            if (!pEl) return;
            const el = pEl.querySelector(partialSection.selector) ?? pEl;
            const replaceWith = getElementHtml(data.partials[partialSection.name], partialSection.selector);
            el.innerHTML = replaceWith.trim();
        })
        ProductForm.updateCartIcon(data);

        if (this.cartDrawer && cartCount === 0) {
            drawerContent.classList.add('items-center');
        }

        // We set focus on the number input because the plus and minus buttons aren't accessible for assistive technologies.
        this.setFocus(cartItemId)
    }


    getPartialsToRender() {
        let partials = []
        if (this.cartDrawer) {
            partials = [...partials,
                {
                    id: "cart-drawer",
                    name: "cartDrawer",
                    selector: ".cart-drawer__summary",
                },
                {
                    id: "cart-items",
                    name: "cartDrawer",
                    selector: "#cart-items",
                },
            ]
        } else {
            partials = [...partials,
                {
                    id: "cart-summary",
                    name: "cart",
                    selector: "#cart-summary",
                },
                {
                    id: "cart-items",
                    name: "cart",
                    selector: "#cart-items",
                },
            ]
        }
        return partials;
    }

    updateNotifications(carItemErrorId, message) {
        const cartItemErrorBox = document.getElementById(carItemErrorId);
        if (!cartItemErrorBox) return;
        if (!message) {
            cartItemErrorBox.hidden = true;
            cartItemErrorBox.innerHTML = '';
        } else if (message) {
            cartItemErrorBox.textContent = message;
            cartItemErrorBox.hidden = false;
        }
    }

    setFocus(cartItemId) {
        const numberInput = this.querySelector(`.quantity-selector > input[id="quantity-${cartItemId}"]`)
        if (!numberInput) return;
        numberInput.focus({focusVisible: true});
        numberInput.select();
    }

}

if (!customElements.get('cart-items')) {
    customElements.define('cart-items', CartItems);
}