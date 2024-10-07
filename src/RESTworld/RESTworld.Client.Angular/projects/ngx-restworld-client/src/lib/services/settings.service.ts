import { Injectable } from "@angular/core";
import { ClientSettings } from "../models/client-settings";
import { RestWorldOptions } from "../models/restworld-options";
import { RestWorldClientCollection } from "./restworld-client-collection";

/**
 * This service is responsible for loading the settings from the client-backend and setting up the RestWorldClientCollection.
 * It is important that its ensureInitialized() method is called before angular is initialized.
 * This can be done by calling ensureSettingsAreLoaded() in your main.ts before calling bootstrapModule(...).
 */
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
    private _clients: RestWorldClientCollection) {
  }

  /**
   * Ensures that the settings are loaded and the RestWorldClientCollection is set up.
   * This method is called automatically by the `RestworldClientModule`.
   * You should not need to call this method yourself.
   * */
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
   * @example
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
      .map(api =>  this._clients.addOrGetExistingClient(api.name, new RestWorldOptions(api.url, api.version))));
  }

}
