import { ApiUrl } from "./api-url";

/**
 * These are the settings that the client is retrieving from its client-backend.
 * It is then used by the `SettingsService` to set everything up.
 */
export interface ClientSettings {
  apiUrls: ApiUrl[];
  extensions: { [key: string]: string; }
}
