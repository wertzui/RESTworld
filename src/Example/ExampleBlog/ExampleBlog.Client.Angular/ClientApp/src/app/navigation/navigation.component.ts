import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { AppMenu } from '../app.routes';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.css']
})
export class NavigationComponent implements OnInit {

  public readonly menu: MenuItem[];

  constructor() {
    this.menu = AppMenu;
  }

  ngOnInit(): void {
  }

}
