import {createAndSubmitForm} from "./utilities.js";

class ShippingAddressFlow extends HTMLElement{
    constructor() {
        super();
        this.addEventListener('click', this.handleClick.bind(this));
    }
    
    handleClick(e){
        if (!e.target.matches('button[name="save-address"]')) return;
        e.preventDefault();
        createAndSubmitForm({
            url: e.target.dataset.url, 
            method: "POST",
        });
    }
}

if (!customElements.get('shipping-address-flow')) {
    customElements.define('shipping-address-flow', ShippingAddressFlow);
}

