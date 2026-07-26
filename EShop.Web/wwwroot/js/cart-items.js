import {debounce} from "./utilities.js";


class CartItems extends HTMLElement {
    #abortController;

// do it in here, not WebStorm.
    constructor() {
        super();
        this.updateCartUrl = this.closest('.js-cart-form').dataset.updateCartUrl;
        this.removeCartItemUrl = this.closest('.js-cart-form').dataset.removeCartItemUrl;
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

    handleChange(e) {
        const cartItem = e.target.closest('.js-cart-item');
        if (cartItem && cartItem.dataset.cartItemId && cartItem.dataset.cartItemId > 0) {
            this.updateQuantity(cartItem.dataset.cartItemId, e.quantity, document.activeElement.name);
        }
    }


    async updateQuantity(cartItemId, newQuantity, name, isRemoved = false) {
        const urlParams = new URLSearchParams({
            cartItemId,
            newQuantity
        });

        const cartItemErrorId = `cart-item-error-${cartItemId}`;
        document.querySelectorAll(`.cart-item__error:not([id="${cartItemErrorId}"])`)
            .forEach(errorEl => {
                errorEl.hidden = true;
                errorEl.innerHTML = '';
            });

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
                throw new Error(`Request failed with status ${response.status}: ${response.statusText}`);
            }

            this.finalizeUpdating(data);
            this.updateNotifications(cartItemErrorId, data.message);


        } catch (error) {
            if (error.name === 'AbortError') {
                console.warn('Fetch has been aborted by user.');
            }
           else{
                console.log(error);
            }
        }
    }

    finalizeUpdating(data, name) {

        if (data.success){
            const cartContent = document.querySelector('.js-cart-content');
            if (data.cartCount === 0) {
                const emptyMessage = document.querySelector('.js-cart-empty-message');
                cartContent.innerHTML = emptyMessage.outerHTML;
                cartContent.querySelector('.js-cart-empty-message').classList.remove('hidden');
            }

            const cartBody = this.querySelector('.js-cart-body');
            if (data.cartHtml && cartBody) {
                cartBody.innerHTML = data.cartHtml;
            }

            const cartSummary = document.querySelector('.js-cart-summary');
            if (data.totalSummaryHtml && cartSummary) {
                cartSummary.innerHTML = data.totalSummaryHtml;
            }

            if (data.cartCountHtml) {
                document.getElementById('cart-icon-count').innerHTML = data.cartCountHtml;
            }
        }

        if (name) {
            this.setFocus(cartItemId, name)
        }

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

    setFocus(cartItemId, controlName) {
        const cartItem = this.querySelector(`.js-cart-item[data-cart-item-id="${cartItemId}"]`)
        if (!cartItem) return;
        const controlEl = cartItem.querySelector(`[name="${controlName}"]`);
        if (controlEl) {
            controlEl.focus();
        }
    }

}

if (!customElements.get('cart-items')) {
    customElements.define('cart-items', CartItems);
}