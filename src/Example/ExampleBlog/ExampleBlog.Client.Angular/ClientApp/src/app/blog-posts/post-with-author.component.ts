import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-post-with-author',
  templateUrl: './post-with-author.component.html',
  styleUrls: ['./post-with-author.component.css']
})
export class PostWithAuthorComponent implements OnInit {
  @Input()
  public apiName?: string

  @Input()
  public get uri() {
    return PostWithAuthorComponent.unModifyUri(this.modifiedUri);
  }
  public set uri(value: string | undefined) {
    this.modifiedUri = PostWithAuthorComponent.modifyUri(value);
  }

  public modifiedUri?: string;

  constructor() { }

  ngOnInit(): void {
  }

  private static modifyUri(uri?: string): string | undefined {
    if (!uri)
      return undefined;

    return uri.replace(/\/post\//gmi, '/mycustom/postwithauthor/',);
  }

  private static unModifyUri(uri?: string): string | undefined {
    if (!uri)
      return undefined;

    return uri.replace(/\/mycustom\/postwithauthor\//gmi, '/post/');
  }

}
