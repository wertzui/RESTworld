import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import * as _ from 'lodash';

@Pipe({
  name: 'safeUrl'
})
export class SafeUrlPipe implements PipeTransform {

  constructor(private _domSanitizer: DomSanitizer) {
  }

  transform(url: unknown) {
    if (_.isString(url))
      throw new Error(`The given url '${url}' is not a string.`)

    return this._domSanitizer.bypassSecurityTrustResourceUrl(url as string);
  }
}
