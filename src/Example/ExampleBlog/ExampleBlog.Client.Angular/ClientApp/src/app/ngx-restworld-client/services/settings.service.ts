import { HttpBackend, HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ClientSettings } from "../models/client-settings";
import { RESTworldOptions } from "../models/restworld-options";
import { RESTworldClientCollection } from "./restworld-client-collection";

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private readonly _client: HttpClient;

  private _settings: ClientSettings | undefined;
  public get settings(): ClientSettings | undefined {
    return this._settings;
  }

  private initializing = false;
  private initialized = false;

  constructor(
    backend: HttpBackend,
    private _clients: RESTworldClientCollection) {
    this._client = new HttpClient(backend);
  }

  public async ensureInitialized(): Promise<void> {
    if (this.initializing || this.initialized)
      return;

    this.initializing = true;

    await this.loadSettings();
    await this.populateRESTworldClientCollectionFromSettings();

    this.initialized = true;
    this.initializing = false;

  }

  private async loadSettings(): Promise<void> {
    this._settings = await this._client
      .get<ClientSettings>('/settings')
      .toPromise();
  }

  private async populateRESTworldClientCollectionFromSettings(): Promise<void> {
    if (!this._settings?.apiUrls)
      return;

    await Promise.all(this._settings.apiUrls
      .map(api =>  this._clients.addOrGetExistingClient(api.name, new RESTworldOptions(api.url, api.version))));
  }

}
