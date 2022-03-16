import { enableProdMode, StaticProvider } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { SettingsService } from './app/ngx-restworld-client/services/settings.service';
import { environment } from './environments/environment';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

if (environment.production) {
  enableProdMode();
}

async function main() {
  try {
    await SettingsService.ensureSettingsAreLoaded();

    const providers: StaticProvider[] = [
      { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
    ];

    await platformBrowserDynamic(providers).bootstrapModule(AppModule);
  }
  catch (e) {
    console.error(e);
  }
}

main();
