import { Component, Input, OnInit } from '@angular/core';
import { SafeUrl } from '@angular/platform-browser';
import { AvatarGenerator } from '../../services/avatar-generator';

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
  styleUrls: ['./restworld-avatar.component.css']
})
export class RestWorldAvatarComponent implements OnInit{
  @Input({required: true})
  /**
   * The username of the user to display an avatar for.
   */
  user?: string;

  private _image: SafeUrl = '';
  public get image(): string { return this._image as string; }
  private _label: string = '';
  public get label(): string { return this._label; }
  private _style?: Record<string, string>;
  public get style(): Record<string, string> | undefined { return this._style; }
  public get tooltip(): string { return this.user ?? ''; }

  constructor(
    private readonly _generator: AvatarGenerator
  ) {

  }

  async ngOnInit(): Promise<void> {
    if(this.user === undefined || this.user === null)
      return;

    this._image = await this._generator.getImageAsync(this.user);
    this._label = await this._generator.getLabelAsync(this.user);
    this._style = await this._generator.getStyleAsync(this.user);
  }
}
