import { Property, Options } from '@wertzui/ngx-hal-client'
import { RestWorldImage } from './restworld-image';

export class PropertyWithImage extends Property {
  restWorldImage!: RestWorldImage;
}

export class PropertyWithOptions extends Property {
  declare options: Options;
}
