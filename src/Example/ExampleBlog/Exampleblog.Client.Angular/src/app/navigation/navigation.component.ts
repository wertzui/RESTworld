import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { AppMenu } from '../app.routes';
import { MenubarModule } from "primeng/menubar";

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.css'],
  standalone: true,
  imports: [MenubarModule]
})
export class NavigationComponent implements OnInit {
  public readonly menu: MenuItem[];

  constructor() {
    this.menu = AppMenu;
  }

  public ngOnInit(): void {
  }
}
