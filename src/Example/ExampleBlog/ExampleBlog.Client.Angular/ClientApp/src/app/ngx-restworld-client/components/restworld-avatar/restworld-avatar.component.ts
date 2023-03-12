import { Component, Input, OnInit } from '@angular/core';
import { SafeUrl } from '@angular/platform-browser';
import { AvatarGenerator } from '../../services/avatar-generator';

@Component({
  selector: 'rw-avatar',
  templateUrl: './restworld-avatar.component.html',
  styleUrls: ['./restworld-avatar.component.css']
})
export class RestWorldAvatarComponent implements OnInit{
  @Input()
  user?: string;

  private _image?: SafeUrl;
  public get image(): SafeUrl | undefined { return this._image; }
  private _label?: string;
  public get label(): string | undefined { return this._label; }
  private _style?: '' | { 'background-color': string; color: string };
  public get style(): '' | { 'background-color': string; color: string } | undefined { return this._style; }

  constructor(
    private readonly _generator: AvatarGenerator
  ) {

  }

  async ngOnInit(): Promise<void> {
    if(this.user === undefined || this.user === null)
      return;

    this._image = (await this._generator.getImageAsync(this.user)) ?? undefined;
    this._label = await this._generator.getLabelAsync(this.user);
    this._style = await this._generator.getStyleAsync(this.user);
  }
}
