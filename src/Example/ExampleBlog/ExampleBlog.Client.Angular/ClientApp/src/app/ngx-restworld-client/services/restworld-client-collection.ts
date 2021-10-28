import { Injectable } from "@angular/core";
import { HalClient } from "@wertzui/ngx-hal-client";
import { RESTworldOptions } from "../models/restworld-options";
import { RESTworldClient } from "./restworld-client";

@Injectable({
  providedIn: 'root'
})
export class RESTworldClientCollection {
  private readonly _clients: {
    [name: string]: RESTworldClient
  };
  constructor(private _halClient: HalClient) {
    this._clients = {};
  }

  public async addClient(name: string, options: RESTworldOptions): Promise<RESTworldClient> {
    if (Object.keys(this._clients).includes(name))
      throw new Error(`A client with the name '${name}' already exists.`);

    const client = new RESTworldClient(this._halClient, options);
    await client.ensureHomeResourceIsSet();
    this._clients[name] = client;
    return client;
  }

  public getClient(name: string): RESTworldClient {
    const client = this._clients[name];
    if (!client)
      throw new Error(`No client with the name '${name}' exists.`);

    return client;
  }

  public get all(): { [name: string]: RESTworldClient } {
    return this._clients;
  }
}