import { Component, computed, input } from '@angular/core';
import { RESTworldEditViewComponent } from "../ngx-restworld-client/views/restworld-edit-view/restworld-edit-view.component";
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-post-with-author',
  templateUrl: './post-with-author.component.html',
  styleUrls: ['./post-with-author.component.css'],
  standalone: true,
  imports: [RESTworldEditViewComponent, RouterLink]
})
export class PostWithAuthorComponent {
  public readonly apiName = input.required<string>();
  public readonly modifiedUri = computed(() => PostWithAuthorComponent.modifyUri(this.uri()));
  public readonly rel = input.required<string>();
  public readonly uri = input.required<string>();

  private static modifyUri(uri?: string): string | undefined {
    if (!uri)
      return undefined;

    return uri.replace(/\/post\//gmi, '/postwithauthor/',);
  }
}
