import {notifyError} from "./utilities.js";


export class ProductForm extends HTMLElement {
    constructor() {
        super();
        this.form = this.querySelector('.js-product-form');
        this.cartDrawer = document.querySelector('#cart-drawer');
        this.form.addEventListener('submit', this.handleSubmit.bind(this));
    }


    async handleSubmit(e) {
        e.preventDefault();
        const submitter =
            e.submitter.matches('button[name="add"]') ? e.submitter : null;
        if (submitter.disabled) return;
        submitter.disabled = true;
        submitter.classList.add('is-loading');
        const formData = new FormData(this.form);
        const response = await fetch(this.form.action, {
            method: 'POST',
            body: formData,
            headers: {
                "Accept": "application/json",
            },
        });

        try {
            if (!response.ok) {
                notifyError(response.statusText);
            } else {
                if (response.redirected) {
                    location.replace(response.url);
                }

                const data = await response.json();
                const warnings = data.warnings;
                if (this.cartDrawer) {
                    await this.cartDrawer.renderContents(data,warnings.length == 0);
                }
                
                data.warnings.forEach(warning => {
                    notifyError(warning);
                })
            }
        } catch (error) {
            console.error(error);

        } finally {
            submitter.disabled = false;
        }
    }

     

    static updateCartIcon(data) {
        const cartIconCount = document.getElementById('cart-icon-count');
        if (data.partials.addToCartCount && cartIconCount) {
            cartIconCount.innerHTML = data.partials.addToCartCount;
        }
    }
}


if (!customElements.get('product-form')) {
    customElements.define('product-form', ProductForm);
}
 
