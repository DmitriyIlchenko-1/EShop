import {SideDrawer} from "./main.js";
import {ProductForm} from './product-form.js';

class CartDrawer extends SideDrawer{
    constructor() {
        super();
        this.init();
        
    }
    
    init(){
        const cartIcon = document.getElementById('cart-icon');
        if (cartIcon){
            cartIcon.setAttribute('role', 'button');
            cartIcon.setAttribute('aria-haspopup', 'dialog');
            cartIcon.addEventListener('click', e => {
                e.preventDefault();
                this.open(cartIcon);
            });
            cartIcon.addEventListener('keydown', e => {
                if (e.key !== ' ') return;
                e.preventDefault();
                this.open(cartIcon);
            });
        }
    }
    
      
    async renderContents(data, openDrawer = true){
        this.getSectionsToRender().forEach(section => {
            const el = document.getElementById(section.id);
            if (!el) return;
            const replaceWith = CartDrawer.getElementHtml(data.partials[section.name], section.selector);
            el.innerHTML = replaceWith;
        })
        ProductForm.updateCartIcon(data);
        if (openDrawer && this.getAttribute('open') === null){
            this.open();
        }
    }
    
    getSectionsToRender(){
        return [
            {
                id: "cart-drawer",
                name: "cartDrawer",
                selector: "#cart-drawer",
            }
        ]
    }
    
    static getElementHtml(html, selector){
        const template = document.createElement('template');
        template.innerHTML = html;
        return template.content.querySelector(selector).innerHTML;
    }

   
    
     
}

if (!customElements.get('cart-drawer')) {
    customElements.define('cart-drawer', CartDrawer);
}
 