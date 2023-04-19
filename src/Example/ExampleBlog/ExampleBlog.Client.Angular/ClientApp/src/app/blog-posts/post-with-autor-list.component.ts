import { Component, Input } from '@angular/core';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-post-with-author-list',
  templateUrl: './post-with-author-list.component.html'
})
export class PostWithAuthorListComponent {
  public display: boolean = false;

  showDialog() {
    this.display = true;
  }

  public menu: MenuItem[] = [
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

  constructor(
  ) { }



}
