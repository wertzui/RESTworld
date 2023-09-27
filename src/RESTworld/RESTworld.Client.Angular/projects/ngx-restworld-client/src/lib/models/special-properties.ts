import { Property, Options, SimpleValue } from '@wertzui/ngx-hal-client'
import { RestWorldImage } from './restworld-image';

/**
 * A property that represents an image.
 */
export class PropertyWithImage extends Property<string, never, never>  { 
    restWorldImage!: RestWorldImage 
}

/**
 * A property that represents a dropdown.
 */
export class PropertyWithOptions<TValue extends SimpleValue = SimpleValue, OptionsPromptField extends string = "prompt", OptionsValueField extends string = "value"> extends Property<TValue, OptionsPromptField, OptionsValueField> { 
    declare options: Options<TValue, OptionsPromptField, OptionsValueField> 
};
