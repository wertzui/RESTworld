import { ApiUrl } from "./api-url";

export interface ClientSettings {
  apiUrls: ApiUrl[];
  extensions: { [key: string]: string; }
}
