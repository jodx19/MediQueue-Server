import { Directive, ElementRef, OnInit, inject } from '@angular/core';

@Directive({
  selector: '[scrollReveal]',
  standalone: true
})
export class ScrollRevealDirective implements OnInit {
  private el = inject(ElementRef);

  ngOnInit() {
    const obs = new IntersectionObserver(([entry]) => {
      if (entry.isIntersecting) {
        this.el.nativeElement.classList.add('revealed');
        obs.disconnect();
      }
    }, { threshold: 0.15 });
    obs.observe(this.el.nativeElement);
  }
}
