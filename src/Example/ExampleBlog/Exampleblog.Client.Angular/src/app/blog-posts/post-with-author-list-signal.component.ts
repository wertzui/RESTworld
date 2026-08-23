import { Component, signal } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { RESTworldSignalListViewComponent } from "../ngx-restworld-client/views/restworld-signal-list-view/restworld-signal-list-view.component";
import { DialogModule } from "primeng/dialog";

/**
 * This is the Signal Forms equivalent of {@link PostWithAuthorListComponent}.
 * It uses `<rw-signal-list>` instead of `<rw-list>`, with a custom `editLink` pointing at
 * {@link PostWithAuthorSignalComponent} instead of the default `<rw-signal-edit>`.
 */
@Component({
  selector: 'app-post-with-author-list-signal',
  templateUrl: './post-with-author-list-signal.component.html',
  standalone: true,
  imports: [RESTworldSignalListViewComponent, DialogModule]
})
export class PostWithAuthorListSignalComponent {
  public readonly display = signal(false);
  public readonly menu: MenuItem[] = [
    {
      routerLink: ['/'],
      icon: 'fas fa-home',
      label: 'Home',
      title: 'Go to home',
      tooltipOptions: { tooltipPosition: 'left'}
    },
    {
      command: () => this.showDialog(),
      icon: 'fas fa-window-maximize',
      label: 'Dialog',
      title: 'Show a dialog',
      tooltipOptions: { tooltipPosition: 'left' }
    }
  ];

  public showDialog() {
    this.display.set(true);
  }
}
