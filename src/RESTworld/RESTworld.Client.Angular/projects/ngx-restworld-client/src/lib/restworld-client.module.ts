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
import { ContextMenuModule } from 'primeng/contextmenu';
import { ChipModule } from 'primeng/chip';

import { RESTworldListViewComponent } from './views/restworld-list-view/restworld-list-view.component';
import { RESTworldEditViewComponent } from './views/restworld-edit-view/restworld-edit-view.component';
import { RestWorldClientCollection } from './services/restworld-client-collection';
import { AvatarGenerator } from './services/avatar-generator';
import { SettingsService } from './services/settings.service';
import { RestWorldImageComponent } from './components/restworld-image/restworld-image.component';
import { ButtonModule } from 'primeng/button';
import { RestWorldFileComponent } from './components/restworld-file/restworld-file.component';
import { SafeUrlPipe } from './pipes/safe-url.pipe';
import { AsPipe } from './pipes/as.pipe';
import { SplitButtonModule } from 'primeng/splitbutton';
import { RestWorldAvatarComponent } from './components/restworld-avatar/restworld-avatar.component';
import { RestWorldLabelComponent } from './components/restworld-label/restworld-label.component';
import { RestWorldValidationErrorsComponent } from './components/restworld-validation-errors/restworld-validation-errors.component';
import { RestWorldFormComponent } from './components/restworld-form/restworld-form.component';
import { RestWorldTableComponent } from './components/restworld-table/restworld-table.component';
import { RestWorldMenuButtonComponent } from './components/restworld-menu-button/restworld-menu-button.component';
import { RestWorldIdNavigationComponent } from './components/restworld-id-navigation/restworld-id-navigation.component';
import { RestWorldFormElementComponent, RestWorldInputCollectionComponent, RestWorldInputComponent, RestWorldInputDropdownComponent, RestWorldInputObjectComponent, RestWorldInputSimpleComponent, RestWorldInputTemplateComponent } from './components/restworld-inputs/restworld-inputs';
import { RestWorldDisplayElementComponent, RestWorldDisplayCollectionComponent, RestWorldDisplayComponent, RestWorldDisplayDropdownComponent, RestWorldDisplayObjectComponent, RestWorldDisplaySimpleComponent, RestWorldDisplayTemplateComponent } from './components/restworld-displays/restworld-displays';
import { HalClientModule } from '@wertzui/ngx-hal-client';
import { SearchIcon } from 'primeng/icons/search';
import { SpinnerIcon } from 'primeng/icons/spinner';
import { ClrFormatPipe } from './pipes/clr-format.pipe';
import { PropertyTypeFormatPipe } from './pipes/property-type-format.pipe';

export function initializeSettings(settingsService: SettingsService): () => Promise<void> {
  return async () => await settingsService.ensureInitialized();
}

@NgModule({
  declarations: [
    AsPipe,
    ClrFormatPipe,
    PropertyTypeFormatPipe,
    RestWorldAvatarComponent,
    RestWorldAvatarComponent,
    RestWorldDisplayCollectionComponent,
    RestWorldDisplayComponent,
    RestWorldDisplayDropdownComponent,
    RestWorldDisplayElementComponent,
    RestWorldDisplayObjectComponent,
    RestWorldDisplaySimpleComponent,
    RestWorldDisplayTemplateComponent,
    RESTworldEditViewComponent,
    RestWorldFileComponent,
    RestWorldFormComponent,
    RestWorldFormElementComponent,
    RestWorldIdNavigationComponent,
    RestWorldImageComponent,
    RestWorldInputCollectionComponent,
    RestWorldInputComponent,
    RestWorldInputDropdownComponent,
    RestWorldInputObjectComponent,
    RestWorldInputSimpleComponent,
    RestWorldInputTemplateComponent,
    RestWorldLabelComponent,
    RESTworldListViewComponent,
    RestWorldMenuButtonComponent,
    RestWorldTableComponent,
    RestWorldValidationErrorsComponent,
    SafeUrlPipe,
  ],
  imports: [
    AvatarModule,
    AvatarModule,
    ButtonModule,
    CalendarModule,
    CheckboxModule,
    ChipModule,
    ColorPickerModule,
    CommonModule,
    ConfirmDialogModule,
    ContextMenuModule,
    DialogModule,
    DragDropModule,
    DropdownModule,
    FileUploadModule,
    FormsModule,
    ReactiveFormsModule,
    HalClientModule,
    ImageCropperModule,
    InputNumberModule,
    InputTextModule,
    MessagesModule,
    MultiSelectModule,
    PanelModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    RippleModule,
    RouterModule,
    ScrollingModule,
    SearchIcon,
    SkeletonModule,
    SpinnerIcon,
    SplitButtonModule,
    TableModule,
    TabViewModule,
    ToastModule,
    TooltipModule,
    TriStateCheckboxModule,
    ValdemortModule,
  ],
  exports: [
    AsPipe,
    ClrFormatPipe,
    RestWorldAvatarComponent,
    RestWorldDisplayCollectionComponent,
    RestWorldDisplayComponent,
    RestWorldDisplayDropdownComponent,
    RestWorldDisplayElementComponent,
    RestWorldDisplayObjectComponent,
    RestWorldDisplaySimpleComponent,
    RestWorldDisplayTemplateComponent,
    RESTworldEditViewComponent,
    RestWorldFileComponent,
    RestWorldFormComponent,
    RestWorldFormElementComponent,
    RestWorldIdNavigationComponent,
    RestWorldImageComponent,
    RestWorldInputCollectionComponent,
    RestWorldInputComponent,
    RestWorldInputDropdownComponent,
    RestWorldInputObjectComponent,
    RestWorldInputSimpleComponent,
    RestWorldInputTemplateComponent,
    RestWorldLabelComponent,
    RESTworldListViewComponent,
    RestWorldMenuButtonComponent,
    RestWorldTableComponent,
    RestWorldValidationErrorsComponent,
    SafeUrlPipe,
  ],
  providers: [
    AvatarGenerator,
    ClrFormatPipe,
    ConfirmationService,
    MessageService,
    RestWorldClientCollection,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeSettings,
      deps: [SettingsService],
      multi: true,
    },
  ]
})
export class RestworldClientModule { }
