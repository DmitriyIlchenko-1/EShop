class CheckoutFlow extends HTMLElement {

    constructor() {
        super();
        this.form = this.querySelector('.js-checkout-form');
        this.init();
    }
    
    init(){
        this.form.addEventListener('submit', this.handleSubmit.bind(this));
    }
    
    async handleSubmit(e){
        e.preventDefault();
        const fetchUrl =  this.form.action;
        const formData = new FormData(this.form);
        const response = await fetch(fetchUrl, {
            method: "POST",
            body: formData
        });
        if (!response.ok){
            
        }
        const data = await response.json();
        if (data.success){
            window.location.replace(data.redirectUrl);
        }
        else{
            
        }
    }

    
}


if (!customElements.get('checkout-flow')) {
    customElements.define('checkout-flow', CheckoutFlow);
}

class AddressFlow extends HTMLElement {
    constructor() {
        super();
        this.getAddressUrl = this.dataset.getAddressUrl;
        this.addEventListener("click", this.handleClick.bind(this));
        this.init(true);
    }

    init(completeInit) {

        this.formPanel = this.querySelector(`.js-form-panel`);
        this.selectPanel = this.querySelector(`.js-select-panel`);
        this.addSubmitBtn = this.querySelector(`[name='addSubmit']`);
        this.updateSubmitBtn = this.querySelector(`[name='updateSubmit']`);
        this.cancelBtn = this.querySelector(`[name='cancel']`);
        if (completeInit) {
            this.initializeActivePanel();

            this.submitHandler = this.submitHandler || this.handleSubmit.bind(this);
            this.form = this.querySelector('.js-checkout-address-form');
            this.form.addEventListener("submit", this.submitHandler);
        }


        //select active panel that arrived from the server.
        //this.activePanel = this.querySelector('.js-panel.is-open');

        //this.newOrUpdateAddressSection = this.querySelector('.checkout__new-address');
    }

    enableLoading(){

    }

    initializeActivePanel() {
        const addrCount = Number.parseInt(this.querySelector(`[data-existing-address-count]`)
            .dataset.existingAddressCount);
        if (addrCount) {
            this.activePanel = this.selectPanel;
        } else {
            this.activePanel = this.formPanel;
            this.cancelBtn.classList.add('hidden');
            this.updateSubmitBtn.classList.add('hidden');
            this.addSubmitBtn.classList.remove('hidden');
        }
        this.activePanel.classList.add('is-open');
        //console.log(`Eventually the active panel in initializeActivePanel() is ${this.activePanel.classList}`);
    }

    async handleRemove(e) {
        const target = (e.target instanceof HTMLButtonElement) ?
            e.target : e.target.closest('button[name="remove"]');
        if (!target) return;
        const addressId = this.querySelector(`#existing-addresses`).value;
        if (!(addressId > 0)) return;
        const params = new URLSearchParams({addressId});
        const response = await fetch(target.dataset.removeAddressUrl + `?${params}`, {
            method: 'DELETE'
        });
        console.log(`handleRemove's called`)

        await this.handleResponse(response, (data) => {
            //reparse the validator after fetching the form as a piece of HTML via fetch().
            const form = $('.js-checkout-address-form');
            form.data('validator', null);
            $.validator.unobtrusive.parse(form);
            form.valid();

            // completeInit because we need to reAppend the event handlers to the newly received DOM elements.
            this.init(true);
        });
    }

    async handleClick(e) {

        const target = (e.target instanceof HTMLButtonElement) ?
            e.target : e.target.closest('button');
        const targetName = target?.name;

        if (targetName === 'add' || targetName === 'update' || targetName === 'cancel') {

            const lastActivePanel = this.activePanel;
            let fetchData = false;
            if (targetName === 'add' || targetName === 'update' && (lastActivePanel !== this.formPanel)) {
                this.activePanel = this.formPanel;
                fetchData = true;
                this.cancelBtn.classList.remove('hidden');
                this.toggleFormButtons(targetName)
            }
            else if (targetName === 'cancel' && (lastActivePanel === this.formPanel)) {
                this.activePanel = this.selectPanel;

                //We do need to reset form between the add form and update form because otherwise the validation details (successes and errors) 
                // from one are displayed for the other as well when it opens.
                CheckoutFlow.resetForm(this.form);
            }
            else{
                return;
            }

            this.close(lastActivePanel);
            if (fetchData) await this.fetchEditOrAddAddress(targetName === 'add');
        } else if (targetName === 'remove') {
            await this.handleRemove(e);
        }
    }

    async fetchEditOrAddAddress(add) {
        const addressId = add ? 0 : this.querySelector(`#existing-addresses`).value;
        const params = new URLSearchParams({addressId});
        const response = await fetch(this.getAddressUrl + `?${params}`, {
            method: 'GET'
        });

        if (!response.ok) console.error(response.statusText);

        const data = JSON.parse(await response.json());
        const inputPrefix = "NewAddress_";
        for (const [key, value] of Object.entries(data)) {
            const control = this.form.querySelector(`#${inputPrefix + key}`);
            if (control) {
                control.value = value;
            }
        }


        //TODO: come back to the issue of Jquery not validating prefilled from controls when you're empty. 
        // 1.Remove the prefilled value 2. Leave the control 3. no error message is displayed, 
        // no validation takes place until you enter the field again and do enter a new value and then remove that newly entered value.
        // temp solution cuz the fields will get validated twice: here and in the framework.
        // $(".js-checkout-address-form").on('change', function (e) {
        //     const validator = $(".js-checkout-address-form").validate();
        //     validator.element(`#${e.target.id}`);
        // });

    }

    async handleSubmit(e) {
        // if (!($('.js-checkout-address-form').valid())) return;
        e.preventDefault();
        const submitter = e.submitter;
        const fetchUrl = submitter.getAttribute('name') === 'addSubmit'
            ? submitter.dataset.addNewUrl
            : submitter.dataset.updateUrl;

        const formData = new FormData(this.form);
        const response = await fetch(fetchUrl, {
            method: "POST",
            body: formData
        });
        await this.handleResponse(response, (data) => {
            //reselect the necessary dom elements again after inserting the html
            this.init(false);

            if (data.success) {
                if (this.activePanel === this.formPanel) {
                    CheckoutFlow.resetForm(this.form);
                    const panelToClose = this.formPanel;
                    this.activePanel = this.selectPanel;
                    this.close(panelToClose);
                }
                else{
                    this.activePanel.classList.add('is-open');
                }
            }
            this.cancelBtn.classList.add('hidden');
            this.toggleFormButtons(submitter.getAttribute('name') === 'addSubmit' ? "add" : "update");
        });


        // console.log(`submitter: ${submitter.name}`);
        // console.log(`BEFORE CLOSING: this is where is-open is: ${this.querySelector('.is-open').classList}`);
        // 
        // console.log(`AFTER CLOSING: this is where is-open is: ${this.querySelector('.is-open').classList}`);
    }

    toggleFormButtons(operation){
        if (operation === 'add') {
            this.addSubmitBtn.classList.remove('hidden');
            this.updateSubmitBtn.classList.add('hidden');
        }
        if(operation === 'update') {
            this.addSubmitBtn.classList.add('hidden');
            this.updateSubmitBtn.classList.remove('hidden');
        }

    }

    async handleResponse(response, callback = null) {
        if (!response.ok) {

        }
        const data = await response.json();

        if (data.renderSections) {
            Object.keys(data.renderSections).forEach(name => {
                const r = this.querySelector(`#checkout-${name}-load`);
                this.querySelector(`#checkout-${name}-load`).innerHTML = data.renderSections[name];
            });
        }

        callback?.(data);
    }


    static resetForm(form) {
        form.querySelectorAll('input:is([type=radio], [type=checkbox]), select').forEach(input => {
            input.removeAttribute('checked');
            input.removeAttribute('selected');
            input.selectedIndex = 0;
        });
        form.querySelectorAll('input:not([type=radio], [type=checkbox])').forEach(input => {
            input.value = '';
        });
        CheckoutFlow.clearValidation(form);
    }

    static clearValidation(formElement) {
        //Internal $.validator is exposed through $(form).validate()
        const validator = $(formElement).validate();
        //Iterate through named elements inside of the form, and mark them as error free
        $('[name]', formElement).each(function () {
            validator.successList.push(this);//mark as error free
        });
        validator.showErrors(); //remove error messages if present
        validator.resetForm(); //remove error class on name elements and clear history
        validator.reset(); //remove all error and success data
        $(formElement).find(".valid").removeClass('valid');
    }

    static open(tab) {
        tab.classList.add('is-open');
        requestAnimationFrame(() => {
            tab.style.height = '0';
            requestAnimationFrame(() => {
                tab.style.height = `${tab.scrollHeight}px`;

                const handler = e => {
                    if (e.target !== tab || e.propertyName !== "height") {
                        return;
                    }

                    tab.removeEventListener('transitionend', handler);
                    tab.style.height = '';
                };
                tab.addEventListener('transitionend', handler);
            })
        })
    }

    close(tab) {
        tab.style.height = `${tab.scrollHeight}px`;
        tab.offsetHeight;

        requestAnimationFrame(() => {
            tab.style.height = '0';
        });

        const handler = (e) => {
            if (e.target !== tab || e.propertyName !== "height") {
                return;
            }

            tab.removeEventListener('transitionend', handler);
            tab.style.height = '';
            console.log('closed')
            tab.classList.remove('is-open');
            CheckoutFlow.open(this.activePanel);
        };
        tab.addEventListener('transitionend', handler);
    }
}

if (!customElements.get('address-flow')) {
    customElements.define('address-flow', AddressFlow);
}