export class QuantitySelectorUpdateEvent extends Event{
    constructor(quantity) {
        super('on:quantity-selector-update', {bubbles: true});
        this.quantity = quantity;
    }
}