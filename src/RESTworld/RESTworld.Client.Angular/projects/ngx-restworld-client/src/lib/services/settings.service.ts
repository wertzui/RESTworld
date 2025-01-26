import { Injectable } from "@angular/core";
import { ClientSettings } from "../models/client-settings";
import { RestWorldOptions } from "../models/restworld-options";
import { RestWorldClientCollection } from "./restworld-client-collection";
import { provideRestWorld } from "../provide-restworld";

/**
 * This service is responsible for loading the settings from the client-backend and setting up the RestWorldClientCollection.
 * It is automatically provided by the {@link provideRestWorld()} function.
 */
@Injectable({
    providedIn: 'root'
})
export class SettingsService {
    private static _settings: ClientSettings | undefined;

    private initialized = false;
    private initializing = false;

    constructor(
        private _clients: RestWorldClientCollection) {
    }

    public get settings(): ClientSettings | undefined {
        return SettingsService._settings;
    }

    private static async ensureSettingsAreLoaded(): Promise<void> {
        if (SettingsService._settings === undefined) {
            const response = await fetch('/settings');
            const settings: ClientSettings = await response.json();
            SettingsService._settings = settings;
        }
    }

    /**
     * Ensures that the settings are loaded and the RestWorldClientCollection is set up.
     * This method is called automatically by {@link provideRestWorld()}
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

    private async populateRESTworldClientCollectionFromSettings(): Promise<void> {
        if (!this.settings?.apiUrls)
            return;

        await Promise.all(this.settings.apiUrls
            .map(api => this._clients.addOrGetExistingClient(api.name, new RestWorldOptions(api.url, api.version))));
    }
}
