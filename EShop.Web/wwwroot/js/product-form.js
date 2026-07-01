
class ProductForm extends HTMLElement{
    constructor() {
        super();
        
        this.init();
    }
    
    init(){
        this.form = this.querySelector('.js-product-form');
        if (this.form){
            this.form.addEventListener('submit', this.handleSubmit.bind(this));
        }
    }
    
    async handleSubmit(e){
        e.preventDefault();
        this.submitBtn = this.querySelector('[name="add"]');
        
        
       
        
        this.submitBtn.disabled = true;
        this.submitBtn.classList.add('is-loading');
        const formData = new FormData(this.form);
        
        const fetchOptions = {
            method: "POST",
            body: formData
        };
        
        try {
            const action = this.form.getAttribute('action');
            const response = await fetch(action, fetchOptions)
            const data = await response.json();
            if (!data.success){
                
            }
            const errors = data.errors;
            ProductForm.updateCartIcon(data);
            
        }
        catch (error) {
            
        }
        finally {
            this.submitBtn.disabled = false;
            this.submitBtn.classList.remove('is-loading');
        }
        
    }
    
    static updateCartIcon(response){
        const cartIconCount = document.getElementById('cart-icon-count');
        if (response.partials.addToCartCount && cartIconCount){
            cartIconCount.innerHTML = response.partials.addToCartCount;
        }
    }
    
    
}

if (!customElements.get('product-form')) {
    customElements.define('product-form', ProductForm);
}
