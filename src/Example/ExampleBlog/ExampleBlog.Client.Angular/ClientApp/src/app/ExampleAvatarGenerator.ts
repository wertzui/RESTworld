import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { Link } from '@wertzui/ngx-hal-client';
import { lastValueFrom } from 'rxjs';
import { AvatarGenerator } from './ngx-restworld-client/services/avatar-generator';
import { RestWorldClientCollection } from './ngx-restworld-client/services/restworld-client-collection';

@Injectable({
  providedIn: 'root',
})
export class ExampleAvatarGenerator extends AvatarGenerator {

  private readonly _client: HttpClient;
  private _photoLink: Link;

  constructor(
    clients: RestWorldClientCollection,
    private readonly _sanitizer: DomSanitizer
  ) {
    super();
    this.getImageAsyncOverride = this.getPhotoAsync;
    const client = clients.getClient("ExampleBlog");
    this._client = client.halClient.httpClient;
    this._photoLink = client.getLinkFromHome("Photo");
  }

  async getPhotoAsync(email: string): Promise<SafeUrl> {
    const uri = this._photoLink.fillTemplate({ email: email });
    try {
      const response = await lastValueFrom(this._client.get(uri, { responseType: 'blob', observe: 'response' }));

      if (!response.ok || response.body === null)
        return "";

      const unsafeUrl = window.URL.createObjectURL(response.body);
      const safeUrl = this._sanitizer.bypassSecurityTrustUrl(unsafeUrl);

      return safeUrl;
    }
    catch (error) {
      return "";
    }
  }
}
