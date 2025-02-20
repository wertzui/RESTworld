import { Component } from '@angular/core';
import { NavigationComponent } from "./navigation/navigation.component";
import { RouterOutlet } from "@angular/router";
import { ToastModule } from "primeng/toast";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: true,
  imports: [NavigationComponent, RouterOutlet, ToastModule]
})
export class AppComponent {
  title = 'app';
}
