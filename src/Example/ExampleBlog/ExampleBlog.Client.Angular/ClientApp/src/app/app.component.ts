import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApplicationInsights, DistributedTracingModes, ITelemetryPlugin } from '@microsoft/applicationinsights-web';
import { AngularPlugin } from '@microsoft/applicationinsights-angularplugin-js';
import { SettingsService } from './ngx-restworld-client/services/settings.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  title = 'app';
  constructor(
    private readonly _router: Router,
    private readonly _settings: SettingsService,
  ) {

  }
  public async ngOnInit(): Promise<void> {
      await this._settings.ensureInitialized();
      const aiInstrumentationKey = 'ApplicationInsights_InstrumentationKey';
      const angularPlugin = new AngularPlugin();
      const appInsights = new ApplicationInsights({
        config: {
          instrumentationKey: this._settings.settings.extensions[aiInstrumentationKey],
          distributedTracingMode: DistributedTracingModes.W3C,
          disableFetchTracking: false,
          enableCorsCorrelation: true,
          enableDebug: true,
          enableDebugExceptions: true,
          enableRequestHeaderTracking: true,
          enableResponseHeaderTracking: true,
          extensions: [angularPlugin],
          extensionConfig: {
            [angularPlugin.identifier]: { router: this._router }
          }
        }
      });
      appInsights.loadAppInsights();
    }
}
