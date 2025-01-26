import { Component, Input, signal } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { RESTworldListViewComponent } from "../ngx-restworld-client/views/restworld-list-view/restworld-list-view.component";
import { DialogModule } from "primeng/dialog";

@Component({
  selector: 'app-post-with-author-list',
  templateUrl: './post-with-author-list.component.html',
  standalone: true,
  imports: [RESTworldListViewComponent, DialogModule]
})
export class PostWithAuthorListComponent {
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
