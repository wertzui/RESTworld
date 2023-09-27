import { Injectable } from "@angular/core";
import { HalClient } from "@wertzui/ngx-hal-client";
import { RestWorldOptions } from "../models/restworld-options";
import { RestWorldClient } from "./restworld-client";

@Injectable({
  providedIn: 'root'
})
/**
 * Represents a collection of RestWorld clients accessible by their API name.
 * This is the main class that you want to inject into all your components that are making calls to your backend APIs.
 */
export class RestWorldClientCollection {
  private readonly _clients: {
    [name: string]: RestWorldClient;
  };
  
  /**
   * Creates an instance of RestworldClientCollection.
   * @param _halClient The HalClient instance to use for making requests.
   */
  constructor(private _halClient: HalClient) {
    this._clients = {};
  }

  /**
   * Determines whether the collection contains a client with the specified name.
   * @param name The name of the client to search for.
   * @returns True if the collection contains a client with the specified name; otherwise, false.
   */
  public containsClient(name: string): boolean {
    return Object.keys(this._clients).includes(name);
  }

  /**
   * Adds a new RestWorldClient to the collection with the given name and options, or returns an existing client with the same name.
   * @param name - The name of the client to add or retrieve.
   * @param options - The options to use when creating a new client.
   * @returns A Promise that resolves to the newly added or existing RestWorldClient.
   */
  public async addOrGetExistingClient(name: string, options: RestWorldOptions): Promise<RestWorldClient> {
    if (Object.keys(this._clients).includes(name))
      return this.getClient(name);

    const client = new RestWorldClient(this._halClient, options);
    await client.ensureHomeResourceIsSet();
    this._clients[name] = client;
    return client;
  }

  /**
   * Returns the RestWorldClient instance with the specified name.
   * This is the main method of this class that you want to use in your components.
   * @param name The API name of the RestWorldClient instance to retrieve.
   * @returns The RestWorldClient instance with the specified name.
   * @throws An error if no client with the specified name exists.
   */
  public getClient(name: string): RestWorldClient {
    const client = this._clients[name];
    if (!client)
      throw new Error(`No client with the name '${name}' exists.`);

    return client;
  }

  public get all(): { [name: string]: RestWorldClient } {
    return this._clients;
  }
}
