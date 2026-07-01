class HeaderComponent extends HTMLElement {
    constructor() {
        super();
        this.mobNavToggle = this.querySelector('.main-menu__toggle');
        this.headerGroupSections = document.getElementById('header-group').children;
        this.updateHeaderHeights();
        this.setHeaderEnd();
        this.bindEvent();
    }

    bindEvent() {
        this.mobNavToggle.addEventListener('click', this.setHeaderEnd.bind(this));
    }

    updateHeaderHeights() {
        if (this.headerGroupSections && this.headerGroupSections.length > 0) {
            let groupHeight = 0;
            for (let i = 0; i < this.headerGroupSections.length; i++) {
                groupHeight += this.headerGroupSections[i].getBoundingClientRect().height;
            }
            if (groupHeight > 0) {
                document.documentElement.style.setProperty('--content-start', `${groupHeight.toFixed(1)}px`);
            }
        }
    }

    setHeaderEnd() {
        const headerEnd = Number(this.getBoundingClientRect().top + this.clientHeight);
        document.documentElement.style.setProperty('--header-end', `${headerEnd.toFixed(1)}px`);
        document.documentElement.style.setProperty('--header-end-with-padding', `${(headerEnd +( theme.mediaMatches.md ? 55 : 20)).toFixed(1)}px`);

    }


}

if (!customElements.get('header-component')) {
    customElements.define('header-component', HeaderComponent);
}