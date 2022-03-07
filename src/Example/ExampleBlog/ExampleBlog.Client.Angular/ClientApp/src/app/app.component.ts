import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ApplicationInsights, DistributedTracingModes } from '@microsoft/applicationinsights-web';
import { AngularPlugin } from '@microsoft/applicationinsights-angularplugin-js';
import { SettingsService } from './ngx-restworld-client/services/settings.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'app';
  constructor(
    private router: Router,
    private settings: SettingsService,
  ) {
    const aiInstrumentationKey = 'ApplicationInsights_InstrumentationKey';
    var angularPlugin = new AngularPlugin();
    const appInsights = new ApplicationInsights({
      config: {
        instrumentationKey: settings.settings.extensions[aiInstrumentationKey],
        distributedTracingMode: DistributedTracingModes.W3C,
        disableFetchTracking: false,
        enableCorsCorrelation: true,
        enableDebug: true,
        enableDebugExceptions: true,
        enableRequestHeaderTracking: true,
        enableResponseHeaderTracking: true,
        extensions: [angularPlugin],
        extensionConfig: {
          [angularPlugin.identifier]: { router: this.router }
        }
      }
    });
    appInsights.loadAppInsights();
  }
}
