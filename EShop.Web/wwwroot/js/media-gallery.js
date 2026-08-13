class MediaGallery extends HTMLElement {
    constructor() {
        super();
        this.init();
    }

    init() {
        this.viewer = this.querySelector('.media-viewer');
        this.controls = this.querySelector('.media-controls');
        this.prevBtn = this.querySelector('.media-controls__btn--previous');
        this.nextBtn = this.querySelector('.media-controls__btn--next');
        this.thumbs = this.querySelector('.media-thumbs');
        this.dots = this.querySelector('.media-dots');
        this.initGallery();
    }

    addListeners() {
        if (this.controls) {
            this.controls.addEventListener('click', this.handleControlClick.bind(this));

        }

        if (this.thumbs || this.dots) {
            const thumbOrDotClickHandler = this.handleThumbOrDotClick.bind(this);
            if (this.thumbs) {
                this.thumbs.addEventListener('click', thumbOrDotClickHandler);

            }
            if (this.dots) {
                this.dots.addEventListener('click', thumbOrDotClickHandler);
            }
        }

        this.viewer.addEventListener('scroll', this.handleScroll.bind(this));
        this.resizeHandler = this.resizeHandler || this.handleResize.bind(this);
        window.addEventListener('on:debounced-resize', this.resizeHandler)
    }

    disconnectedCallback() {
        window.removeEventListener('on:debounced-resize', this.resizeHandler);
    }

    initGallery() {
        this.setVisibleItems();
        if (this.visibleItems.length === 0) return;
        this.viewerItemOffset = this.visibleItems[1].offsetLeft - this.visibleItems[0].offsetLeft;
        this.currentIndex = Math.round(this.viewer.scrollLeft / this.viewerItemOffset);
        this.currentItem = this.visibleItems[this.currentIndex];
        if (this.thumbs && this.currentItem) {
            this.setActiveThumb();
        }
        this.addListeners();
    }

    /**
     * Makes the mediaItem passed to it active by scrolling to it and assign a new value to 'this.currentIndex'.
     * Ensures control, thumb, dot states are updated accordingly.
     * @param mediaItem - media item to make active and scroll to.
     * @param scrollToItem - true, if we want to scroll to the passed media item. Otherwise - false.
     */
    setActiveMedia(mediaItem, scrollToItem = true) {
        if (mediaItem === this.currentItem) return;
        this.currentItem = mediaItem;
        this.currentIndex = this.visibleItems.indexOf(this.currentItem);

        if (scrollToItem) {
            this.viewer.scrollTo({left: mediaItem.offsetLeft});
        }
        if (this.controls) {
            this.prevBtn.disabled = this.currentIndex === 0;
            this.nextBtn.disabled = this.currentIndex === this.visibleItems.length - 1;
        }
        if (this.thumbs) {
            this.setActiveThumb();
        }
        if (this.dots) {
            this.setActiveDot();
        }
    }


    setVisibleItems() {
        this.visibleItems = Array.from(this.querySelectorAll('.media-viewer__item'));
    }

    handleControlClick(e) {
        const clickedBtn = e.target.closest('.media-controls__btn');
        if (!clickedBtn) return;
        const itemToShow = clickedBtn === this.nextBtn
            ? this.visibleItems[this.currentIndex + 1]
            : this.visibleItems[this.currentIndex - 1];
        this.viewer.scrollTo({left: itemToShow.offsetLeft, behavior: 'smooth'});
    }

    /**
     * Handles 'scroll' events on the main media container.
     */
    handleScroll() {

        // We get the left slide if a value before rounding is < 0.5 and the right one if >= 0.5. 
        const newIndex = Math.round(this.viewer.scrollLeft / this.viewerItemOffset);
        // console.log(`this.viewer.scrollLeft ${this.viewer.scrollLeft}`);
        // console.log(`this.viewerItemOffset: ${this.viewerItemOffset}`);

        // The scroll event fires twice during a snap meaning the first time it fires, the index is the same. 
        // We do nothing the first time this method runs.
        // console.log(`handleScroll(): Has been called. A new index (newIndex) is ${newIndex}. The current one (this.currentIndex) is ${this.currentIndex}`);
        // This check sometimes also stops 'resizing' from calling 'this.setActiveMedia' during resizing.
        if (newIndex !== this.currentIndex) {
            const viewerItemOffset = this.visibleItems[1].offsetLeft - this.visibleItems[0].offsetLeft;
            // The event also fires during resize, which is when we don't need to take any action because resnap happens automatically, 
            // the same item that there was before resizing will still be in the view.
            // handleResize() changes viewerItemOffset during resizing so that this line still returns true.
            // console.log(`handleScroll(): Calculated viewerItemOffset is ${viewerItemOffset}. this.viewerItemOffset is ${this.viewerItemOffset}`);
            if (viewerItemOffset === this.viewerItemOffset) {
                // console.log(`handleScroll() the most inner check has been passed`);
                this.setActiveMedia(this.visibleItems[newIndex], false);

            }

        }

    }

    /**
     * Called during resizing to change the value of 'viewerItemOffset' to prevent 'handleScroll()' from doing anything during resizing.
     */
    handleResize() {
        // const was = this.viewerItemOffset;
        this.viewerItemOffset = this.visibleItems[1].offsetLeft - this.visibleItems[0].offsetLeft;
        // console.log(`handleResize(): Has been called. A new viewerItemOffset is ${this.visibleItems[1].offsetLeft - this.visibleItems[0].offsetLeft}. Was ${was}`);
        /* We need to manually place the current thumb into the view if the thumb wasn't snapped to meaning it was activated without calling 'scrollTo()'. */
        if (this.thumbs) {
            this.checkThumbVisibility(this.currentThumb);
        }
    }


    setActiveDot() {
        this.dots.querySelectorAll('.media-dots__btn').forEach(dot => {
            dot.setAttribute('aria-selected', 'false');
        })
        this.activeDot = this.dots.querySelector(`[data-media-id="${this.currentItem.dataset.mediaId}"]`);
        this.activeDot.setAttribute('aria-selected', 'true');
    }


    /**
     * Selects media item to show by reading a data attribute and calling 'this.setActiveMedia()' that finalizes the action.
     * @param e - an instance of PointerEvent.
     */
    handleThumbOrDotClick(e) {
        const thumbOrDot = e.target.closest('[data-media-id]');
        if (!thumbOrDot) return;
        const itemToShow = this.querySelector(`[data-media-id="${thumbOrDot.dataset.mediaId}"]`);
        this.setActiveMedia(itemToShow, true);
    }

    setActiveThumb() {
        this.currentThumb = this.thumbs.querySelector(`[data-media-id="${this.currentItem.dataset.mediaId}"]`);
        const btn = this.currentThumb.querySelector('button');
        this.thumbs.querySelectorAll('.media-thumbs__btn').forEach(el => {
            el.classList.remove('is-active');
            el.removeAttribute('aria-current');
        });
        btn.classList.add('is-active');
        btn.setAttribute('aria-current', 'true');
        this.checkThumbVisibility(this.currentThumb);
    }

    checkThumbVisibility(thumb) {
        const scrolledBy = this.thumbs.scrollLeft;
        const lastVisibleThumbOffset = this.thumbs.clientWidth + scrolledBy;
        const thumbOffset = thumb.offsetLeft;
        if ((thumbOffset + thumb.clientWidth) > lastVisibleThumbOffset || scrolledBy > thumbOffset) {
            this.thumbs.scrollTo({left: thumbOffset, behavior: 'smooth'});
        }
    }


}


if (!customElements.get('media-gallery')) {
    customElements.define('media-gallery', MediaGallery);
}

