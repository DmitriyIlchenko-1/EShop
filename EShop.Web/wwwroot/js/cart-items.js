import {debounce} from "./utilities.js";

class CartItems extends HTMLElement {
// do it in here, not WebStorm.
    constructor() {
        super();
        this.updateCartUrl = this.closest('.js-cart-form').dataset.updateCartUrl;
        this.init();
    }

    init() {
        this.addEventListener('change', debounce(this.handleChange.bind(this)));
    }

    handleChange(e) {
        const cartItem = e.target.closest('.js-cart-item');
        if (cartItem && cartItem.dataset.cartItemId && cartItem.dataset.cartItemId > 0) {
            this.updateQuantity(cartItem.dataset.cartItemId, e.target.value, document.activeElement.name);
        }
    }

    async updateQuantity(cartItemId, newQuantity, name) {
        const urlParams = new URLSearchParams({
            cartItemId,
            newQuantity
        });
        const fetchOptions = {
            method: 'POST',
            body: urlParams
        };
        try {
            const response = await fetch(this.updateCartUrl, fetchOptions);
            const data = await response.json();
            if (!response.ok) {
                //finish off
                throw new Error()
            }

            if (data.cartHtml) {
                this.querySelector('.js-cart-body').innerHTML = data.cartHtml;
            }
            if (data.totalSummaryHtml) {
                document.querySelector('.js-cart-summary').innerHTML = data.totalSummaryHtml;
            }
            if (data.cartCountHtml) {
                document.getElementById('cart-icon-count').innerHTML = data.cartCountHtml;
            }

            this.setFocus(cartItemId, name)
        } catch (error) {

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