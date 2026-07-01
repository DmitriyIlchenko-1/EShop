class VariantSelector extends HTMLElement {
    #abortController;

    constructor() {
        super();
        this.addListeners();
        this.updateHiddenInputs();
        
         
    }

    addListeners() {
        this.addEventListener('change', this.handleVariantChange.bind(this));
    }
    
    updateHiddenInputs(){
        const form = document.querySelector('.js-product-form');
        if (!form) return;
        const updatedSelection = this.getSelectedValues();
        for (const [key, value] of Object.entries(updatedSelection)) {
            const input = document.createElement('input');
            input.setAttribute('type', 'hidden');
            input.setAttribute('required', '');
            input.setAttribute('name', key);
            input.setAttribute('value', value);
            form.prepend(input);
        }
    }
    
    async handleVariantChange(e) {
        if (!(e.target instanceof HTMLInputElement)) return;
        const urlParams = this.createUrlParameters();
        const url = this.buildVariantUrl();
        this.#abortController?.abort();
        this.#abortController = new AbortController();
       try {
           const response = await fetch(url.toString(), {
               signal: this.#abortController.signal,
               method: 'POST',
               mode: 'cors',
               body: urlParams
           });
           const data = await response.json();
           document.querySelectorAll('[data-partial]').forEach(toReplace => {
               const partialName = toReplace.dataset.partial;
               const replaceWith = data.partials[partialName];
               if (replaceWith === undefined || replaceWith === null) return;
               toReplace.innerHTML = replaceWith.trim();

               if (partialName === "addToCart") {
                   this.updateHiddenInputs();
               }
           })

       }
       catch (error){
           if (error.name === 'AbortError')
               console.warn('Fetch aborted by user');
           else {
               console.error(error);
           }
       }
    }
    
    createUrlParameters(){
        const selection = this.getSelectedValues();
        const urlParams = new URLSearchParams(selection);
        this.quantitySelector = document.getElementById('quantity-selector');
        urlParams.append('quantity', this.quantitySelector.getQuantity())
        return urlParams;
    }
    getSelectedValues() {
        const selectedValues = {};
        Array.from(this.querySelectorAll('input:checked')).forEach(input => {
            selectedValues[input.name] = input.value;
        })
        return selectedValues;
    }

    buildVariantUrl() {
        // /product/updateproductdetails/?productId=1;
        return new URL(this.dataset.url, window.location.origin);  
    }
}

if (!customElements.get('variant-selector')) {
    customElements.define('variant-selector', VariantSelector);
}
