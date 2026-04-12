import { Component, computed, input } from '@angular/core';
import { AvatarGenerator } from '../../services/avatar-generator';
import { AvatarModule } from "primeng/avatar";
import { TooltipModule } from "primeng/tooltip";
import { AsyncPipe } from "@angular/common";

/**
 * This component displays an avatar for a given user.
 * The avatar consists of an image or a 2 character label in a circle, based on what the `AvatarGenerator` returns for that user.
 * If you have not overridden the `AvatarGenerator`'s `getImageAsyncOverride` method, the image will always be empty and the label will be generated from the user's name or email.
 * @example
 * <rw-avatar [user]="'John Doe'"></rw-avatar>
 */
@Component({
    selector: 'rw-avatar',
    templateUrl: './restworld-avatar.component.html',
    styleUrls: ['./restworld-avatar.component.css'],
    standalone: true,
    imports: [AvatarModule, TooltipModule, AsyncPipe]
})
export class RestWorldAvatarComponent {
    /**
     * The username of the user to display an avatar for.
     */
    public readonly user = input.required<string>();
    public readonly image = computed(async () => await this._generator.getImageAsync(this.user()));
    public readonly label = computed(async () => await this._generator.getLabelAsync(this.user()));
    public readonly style = computed(async () => await this._generator.getStyleAsync(this.user()));
    public readonly tooltip = computed(() => this.user() ?? '');

    constructor(
        private readonly _generator: AvatarGenerator
    ) {
    }
}
