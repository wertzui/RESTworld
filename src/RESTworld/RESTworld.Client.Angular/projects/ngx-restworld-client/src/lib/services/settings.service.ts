import { HttpBackend, HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ClientSettings } from "../models/client-settings";
import { RESTworldOptions } from "../models/restworld-options";
import { RESTworldClientCollection } from "./restworld-client-collection";

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private static _settings: ClientSettings | undefined;
  public get settings(): ClientSettings | undefined {
    return SettingsService._settings;
  }

  private initializing = false;
  private initialized = false;

  constructor(
    private _clients: RESTworldClientCollection) {
  }

  public async ensureInitialized(): Promise<void> {
    if (this.initializing || this.initialized)
      return;

    this.initializing = true;

    await SettingsService.ensureSettingsAreLoaded();
    await this.populateRESTworldClientCollectionFromSettings();

    this.initialized = true;
    this.initializing = false;

  }

  /**
   * Call this method in your main.ts before calling bootstrapModule(...)
   * 
   * Example:
   * async function main() {
   *   try {
   *     await SettingsService.ensureSettingsAreLoaded();
   * 
   *     const providers : StaticProvider[] = [
   *       { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
   *     ];
   * 
   *     await platformBrowserDynamic(providers).bootstrapModule(AppModule);
   *   }
   *   catch (e) {
   *     console.error(e);
   *   }
   * }
   * 
   * main();
   * */
  public static async ensureSettingsAreLoaded(): Promise<void> {
    if (SettingsService._settings === undefined) {
      const response = await fetch('/settings');
      const settings: ClientSettings = await response.json();
      SettingsService._settings = settings;
    }
  }

  private async populateRESTworldClientCollectionFromSettings(): Promise<void> {
    if (!this.settings?.apiUrls)
      return;

    await Promise.all(this.settings.apiUrls
      .map(api =>  this._clients.addOrGetExistingClient(api.name, new RESTworldOptions(api.url, api.version))));
  }

}
