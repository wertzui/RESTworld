export class RestWorldOptions {
  constructor(public readonly BaseUrl: string, public readonly Version?: number) {
    if (!BaseUrl.endsWith('/'))
      throw new Error(`The provided BaseUrl '${BaseUrl}' does not end with a slash '/'.`);
  }
}
