import { Injectable } from "@angular/core";
import { HalClient } from "@wertzui/ngx-hal-client";
import { RestWorldOptions } from "../models/restworld-options";
import { RestWorldClient } from "./restworld-client";

@Injectable({
  providedIn: 'root'
})
export class RestWorldClientCollection {
  private readonly _clients: {
    [name: string]: RestWorldClient;
  };
  constructor(private _halClient: HalClient) {
    this._clients = {};
  }

  public containsClient(name: string): boolean {
    return Object.keys(this._clients).includes(name);
  }

  public async addOrGetExistingClient(name: string, options: RestWorldOptions): Promise<RestWorldClient> {
    if (Object.keys(this._clients).includes(name))
      return this.getClient(name);

    const client = new RestWorldClient(this._halClient, options);
    await client.ensureHomeResourceIsSet();
    this._clients[name] = client;
    return client;
  }

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
