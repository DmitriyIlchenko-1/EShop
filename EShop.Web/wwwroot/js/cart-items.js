import {debounce} from "./utilities";

class CartItems extends HTMLElement{
// do it in here, not WebStorm.
    constructor() {
        super();
        this.updateCartUrl = this.closest('.js-cart-form').dataset.updateCartUrl;
    }

    init(){
       this.addEventListener('change', debounce(this.handleChange.bind(this)));
    }

    async handleChange(e){
        const cartItem = e.target.closest('.js-cart-item');
        if (cartItem && cartItem.dataset.cartItemId && cartItem.dataset.cartItemId > 0){
            await this.updateQuantity(cartItem.dataset.cartItemId, e.target.getQuantity(), e.target.name);
        }
    }

    async updateQuantity(cartItemId, newQuantity, name){

        
        const fetchOptions = {
            method: 'POST',
            body: JSON.stringify({
                cartItemId,
                newQuantity
            })
        };
        try {
             const response = await fetch(this.updateCartUrl, fetchOptions);
             const data = await response.json();
             if (!response.ok){
                 //finish off
                 throw new Error()
             }
            this.getSectionsToRender().forEach(section => {
                const sectionEl = document.getElementById(section.id);
                if (!sectionEl) return;
                sectionEl.innerHTML = data[section.section];
            });
             this.setFocus(cartItemId,name )
        }
        catch(error){
            
        }
    }
    
    setFocus(cartItemId, controlName){
        const cartItem = this.querySelector(`.js-cart-item[data-cart-item-id="${cartItemId}"]`)
        if (!cartItem) return;
        const controlEl = cartItem.querySelector(`[name="${controlName}"]`);
        if (controlEl){
            controlEl.focus();
        }
    }
   

    getSectionsToRender(){
        let sections = [
            {
                id: "cart-icon-count",
                section: "cart-icon-count"
            },
            {
                id: "cart-items",
                section: this.dataset.section,
            },
            {
                id: "cart-summary",
                section: document.getElementById("cart-summary").dataset.section
            }
        ]
        return sections;
    }
}

if (!customElements.get('cart-items')) {
    customElements.define('cart-items', CartItems);
}