import { Component, computed, input } from '@angular/core';
import { RESTworldSignalEditViewComponent } from "../ngx-restworld-client/views/restworld-signal-edit-view/restworld-signal-edit-view.component";
import { RouterLink } from "@angular/router";

/**
 * This is the Signal Forms equivalent of {@link PostWithAuthorComponent}.
 * It uses `<rw-signal-edit>` instead of `<rw-edit>`, with the same custom "postwithauthor" URI
 * rewrite and extra "Author" tab.
 */
@Component({
  selector: 'app-post-with-author-signal',
  templateUrl: './post-with-author-signal.component.html',
  styleUrls: ['./post-with-author-signal.component.css'],
  standalone: true,
  imports: [RESTworldSignalEditViewComponent, RouterLink]
})
export class PostWithAuthorSignalComponent {
  public readonly apiName = input.required<string>();
  public readonly modifiedUri = computed(() => PostWithAuthorSignalComponent.modifyUri(this.uri()));
  public readonly rel = input.required<string>();
  public readonly uri = input.required<string>();

  private static modifyUri(uri?: string): string | undefined {
    if (!uri)
      return undefined;

    return uri.replace(/\/post\//gmi, '/postwithauthor/',);
  }
}
