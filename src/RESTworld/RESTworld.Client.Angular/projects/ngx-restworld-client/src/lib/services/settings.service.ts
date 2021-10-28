import { HttpBackend, HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ClientSettings } from "../models/client-settings";
import { RESTworldOptions } from "../models/restworld-options";
import { RESTworldClient } from "./restworld-client";
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

  constructor(backend: HttpBackend, private _clients: RESTworldClientCollection) {
    this._client = new HttpClient(backend);
  }

  public async initialize(): Promise<void> {
    await this.ensureSettingsAreLoaded();
    await this.populateRESTworldClientCollectionFromSettings();
  }

  private async ensureSettingsAreLoaded(): Promise<void> {
    this._settings = await this._client
      .get<ClientSettings>('/settings')
      .toPromise();
  }

  private populateRESTworldClientCollectionFromSettings(): Promise<RESTworldClient[]> {
    if (!this._settings?.apiUrls)
      return Promise.resolve([]);

    return Promise.all(this._settings.apiUrls
      .map(api => this._clients.addClient(api.name, new RESTworldOptions(api.url, api.version))));
  }

}
