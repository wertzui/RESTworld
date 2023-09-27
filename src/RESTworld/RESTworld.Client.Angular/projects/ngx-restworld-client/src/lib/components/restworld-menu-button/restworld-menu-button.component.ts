import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { MenuItem } from 'primeng/api';

/**
 * Displays one button for every `MenuItem` given in the items array.
 * If a `MenuItem` has nested items, a button with a dropdown menu will be displayed.
 */
@Component({
  selector: 'rw-menu-button',
  templateUrl: './restworld-menu-button.component.html',
  styleUrls: ['./restworld-menu-button.component.css']
})
export class RestWorldMenuButtonComponent {
  /**
   * An array of menu items to be displayed.
   */
  @Input({ required: true })
  public items: MenuItem[]= [];

  constructor(private readonly _router: Router) {

  }

  onClick(item: MenuItem) {
    if (item.url) {
      window.location.href = item.url;
    } else if (item.routerLink) {
      this._router.navigate(item.routerLink, {
        queryParams: item.queryParams,
        fragment: item.fragment,
        queryParamsHandling: item.queryParamsHandling,
        preserveFragment: item.preserveFragment,
        skipLocationChange: item.skipLocationChange,
        replaceUrl: item.replaceUrl
      });
    } else if (item.command) {
      item.command({ item: item });
    }
  }
}
