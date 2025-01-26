import { ApiUrl } from "./api-url";

/**
 * These are the settings that the client is retrieving from its client-backend.
 * It is then used by the {@link SettingsService} to set everything up.
 */
export interface ClientSettings {
    /**
     * The URLs to the APIs that the client can call.
     */
    apiUrls: ApiUrl[];
    /**
     * Any extra settings that the client may need.
     * RESTworld itself does not need these, but get's them from the frontend-backend so you can transport additional when needed.
     */
    extensions: { [key: string]: string; }
}
