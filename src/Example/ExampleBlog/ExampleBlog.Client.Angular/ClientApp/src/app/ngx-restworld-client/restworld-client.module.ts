import { APP_INITIALIZER, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { RouterModule } from '@angular/router';

import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip'
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CalendarModule } from 'primeng/calendar';
import { CheckboxModule } from 'primeng/checkbox';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { MessagesModule } from 'primeng/messages';
import { PanelModule } from 'primeng/panel';
import { TabViewModule } from 'primeng/tabview';
import { SkeletonModule } from 'primeng/skeleton';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { RippleModule } from 'primeng/ripple';
import { TriStateCheckboxModule } from 'primeng/tristatecheckbox';
import { AvatarModule } from 'primeng/avatar';

import { RESTworldListViewComponent } from './views/restworld-list-view/restworld-list-view.component';
import { RESTworldEditViewComponent } from './views/restworld-edit-view/restworld-edit-view.component';
import { RESTworldClientCollection } from './services/restworld-client-collection';
import { AvatarGenerator } from './services/avatar-generator';
import { SettingsService } from './services/settings.service';

export function initializeSettings(settingsService: SettingsService): () => Promise<void> {
  return async () => await settingsService.initialize();
}

@NgModule({
  declarations: [
    RESTworldListViewComponent,
    RESTworldEditViewComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    TooltipModule,
    ScrollingModule,
    InputTextModule,
    InputNumberModule,
    CalendarModule,
    CheckboxModule,
    ConfirmDialogModule,
    ToastModule,
    MessagesModule,
    PanelModule,
    TabViewModule,
    SkeletonModule,
    ProgressSpinnerModule,
    RippleModule,
    TriStateCheckboxModule,
    AvatarModule
  ],
  exports: [
    RESTworldListViewComponent,
    RESTworldEditViewComponent
  ],
  providers: [
    RESTworldClientCollection,
    AvatarGenerator,
    ConfirmationService,
    MessageService,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeSettings,
      deps: [SettingsService],
      multi: true,
    }
  ]
})
export class RestworldClientModule { }