import { APP_INITIALIZER, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { DragDropModule } from '@angular/cdk/drag-drop';
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
import { DropdownModule } from 'primeng/dropdown';
import { MultiSelectModule } from 'primeng/multiselect';
import { FileUploadModule } from 'primeng/fileupload';
import { ValdemortModule } from 'ngx-valdemort';
import { ImageCropperModule } from 'ngx-image-cropper';
import { DialogModule } from 'primeng/dialog';
import { ColorPickerModule } from 'primeng/colorpicker';

import { RESTworldListViewComponent } from './views/restworld-list-view/restworld-list-view.component';
import { RESTworldEditViewComponent } from './views/restworld-edit-view/restworld-edit-view.component';
import { RESTworldClientCollection } from './services/restworld-client-collection';
import { AvatarGenerator } from './services/avatar-generator';
import { SettingsService } from './services/settings.service';
import { RESTWorldImageViewComponent } from './views/restworld-image-view/restworld-image-view.component';
import { ButtonModule } from 'primeng/button';
import { RESTWorldFileViewComponent } from './views/restworld-file-view/restworld-file-view.component';
import { SafeUrlPipe } from './pipes/safe-url.pipe';
import { RestworldEditFormComponent } from './views/restworld-edit-form/restworld-edit-form.component'
import { FormService } from './services/form.service';
import { AsPipe } from './pipes/as.pipe';
import { SplitButtonModule } from 'primeng/splitbutton';

export function initializeSettings(settingsService: SettingsService): () => Promise<void> {
  return async () => await settingsService.ensureInitialized();
}

@NgModule({
  declarations: [
    RESTworldListViewComponent,
    RESTworldEditViewComponent,
    RESTWorldImageViewComponent,
    RESTWorldFileViewComponent,
    RestworldEditFormComponent,
    SafeUrlPipe,
    AsPipe,
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
    AvatarModule,
    DropdownModule,
    MultiSelectModule,
    FileUploadModule,
    ValdemortModule,
    ImageCropperModule,
    DialogModule,
    ButtonModule,
    ColorPickerModule,
    DragDropModule,
    SplitButtonModule,
  ],
  exports: [
    RESTworldListViewComponent,
    RESTworldEditViewComponent,
    RESTWorldImageViewComponent,
    RESTWorldFileViewComponent,
    RestworldEditFormComponent,
    SafeUrlPipe,
    AsPipe
  ],
  providers: [
    RESTworldClientCollection,
    AvatarGenerator,
    ConfirmationService,
    FormService,
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
