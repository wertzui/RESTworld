import { Component, input } from '@angular/core';
import { Router } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from "primeng/button";
import { SplitButtonModule } from "primeng/splitbutton";
import { TooltipModule } from "primeng/tooltip";

/**
 * Displays one button for every `MenuItem` given in the items array.
 * If a `MenuItem` has nested items, a button with a dropdown menu will be displayed.
 * @example
 * <rw-menu-button [items]="items"></rw-menu-button>
 */
@Component({
    selector: 'rw-menu-button',
    templateUrl: './restworld-menu-button.component.html',
    styleUrls: ['./restworld-menu-button.component.css'],
    standalone: true,
    imports: [ButtonModule, TooltipModule, SplitButtonModule]
})
export class RestWorldMenuButtonComponent {
  /**
   * An array of menu items to be displayed.
   */
  public readonly items = input.required<MenuItem[]>();

  constructor(private readonly _router: Router) {

  }

  onClick(event: MouseEvent, item: MenuItem) {
    const openInNewWindow = event.ctrlKey || event.button === 1 || event.metaKey;
    if (item.url) {
      window.open(item.url, openInNewWindow ? '_blank' : '_self');
    }
    else if (item.routerLink) {
      if (openInNewWindow) {
        const tree = this._router.createUrlTree(item.routerLink, {
          queryParams: item.queryParams,
          fragment: item.fragment,
          queryParamsHandling: item.queryParamsHandling,
          preserveFragment: item.preserveFragment,
        });
        const url = this._router.serializeUrl(tree);
        window.open(url, '_blank');
      }
      else {
        this._router.navigate(item.routerLink, {
          queryParams: item.queryParams,
          fragment: item.fragment,
          queryParamsHandling: item.queryParamsHandling,
          preserveFragment: item.preserveFragment,
          skipLocationChange: item.skipLocationChange,
          replaceUrl: item.replaceUrl,
        });
      }
    }
    else if (item.command) {
      item.command({ item: item });
    }
  }
}
