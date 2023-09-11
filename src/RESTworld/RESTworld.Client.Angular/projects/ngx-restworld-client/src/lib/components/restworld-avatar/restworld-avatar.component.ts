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
