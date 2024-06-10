import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OpenTelemetryService } from './ngx-restworld-client/services/opentelemetry.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  title = 'app';
  constructor(
    private readonly _router: Router,
    private readonly _openTelemetry: OpenTelemetryService,
  ) {

  }
  public async ngOnInit(): Promise<void> {
      await this._openTelemetry.initialize();
    }
}
