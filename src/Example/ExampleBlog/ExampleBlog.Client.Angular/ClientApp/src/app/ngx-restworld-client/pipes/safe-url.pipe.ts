import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

/**
 * A pipe that sanitizes a URL and returns a `SafeUrl` object.
 * @example
 * <img [src]="url | safeUrl"></img>
 */
@Pipe({
  name: 'safeUrl'
})
export class SafeUrlPipe implements PipeTransform {

  constructor(private _domSanitizer: DomSanitizer) {
  }

  /**
   * Sanitizes the given URL and returns a `SafeUrl` object.
   * @param url The URL to sanitize.
   * @returns A `SafeUrl` object representing the sanitized URL.
   * @throws An error if the given URL is not a string.
   */
  transform(url: unknown): SafeUrl {
    if (typeof url !== 'string')
      throw new Error(`The given url '${url}' is not a string.`)

    return this._domSanitizer.bypassSecurityTrustResourceUrl(url);
  }
}
