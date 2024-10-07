/**
 * The URL to a backend API that may be called.
 */
export interface ApiUrl {
  /**
   * The name of the API.
   */
  name: string;
  /**
   * The URL of the API.
   */
  url: string;
  /**
   * The version of the API.
   */
  version?: number;
}
