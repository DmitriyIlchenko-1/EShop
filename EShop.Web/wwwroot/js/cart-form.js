//
// export class CartForm extends HTMLElement{
//     constructor() {
//         super();
//        
//         this.init();
//     }
//    
//     init(){
//         this.form = this.querySelector('.js-cart-form');
//         if (this.form){
//             this.form.addEventListener('submit', this.handleSubmit.bind(this));
//         }
//     }
//    
//     async handleSubmit(e){
//         e.preventDefault();
//         this.submitBtn = this.querySelector('[name="add"]');
//        
//        
//       
//        
//         this.submitBtn.disabled = true;
//         this.submitBtn.classList.add('is-loading');
//         const formData = new FormData(this.form);
//        
//         const fetchOptions = {
//             method: "POST",
//             body: formData
//         };
//        
//         try {
//             const action = this.form.getAttribute('action');
//             const response = await fetch(action, fetchOptions)
//             const data = await response.json();
//             CartForm.updateCartIcon(data);
//             this.displayNotifications(data.warnings);
//            
//            
//         }
//         catch (error) {
//            
//         }
//         finally {
//             this.submitBtn.disabled = false;
//             this.submitBtn.classList.remove('is-loading');
//         }
//        
//     }
//    
//     displayNotifications(notifications) {
//         const notyf = new Notyf();
//         notifications.forEach(n => {
//             notyf.error(n);
//         })
//     }
//
//
//     static updateCartIcon(data){
//         const cartIconCount = document.getElementById('cart-icon-count');
//         if (data.partials.addToCartCount && cartIconCount){
//             cartIconCount.innerHTML = data.partials.addToCartCount;
//         }
//     }
//
//
//
//
// }
//
// if (!customElements.get('cart-form')) {
//     customElements.define('cart-form', CartForm);
// }
