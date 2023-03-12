/*
 * Public API Surface of ngx-restworld-client
 */

export * from './lib/constants/link-names';

export * from './lib/components/restworld-avatar/restworld-avatar.component'

export * from './lib/models/api-url';
export * from './lib/models/client-settings';
export * from './lib/models/problem-details';
export * from './lib/models/restworld-options';

export * from './lib/pipes/safe-url.pipe'
export * from './lib/pipes/as.pipe'

export * from './lib/services/avatar-generator';
export * from './lib/services/form.service';
export * from './lib/services/o-data.service';
export * from './lib/services/restworld-client';
export * from './lib/services/restworld-client-collection';
export * from './lib/services/settings.service';

export * from './lib/views/restworld-edit-form/restworld-edit-form.component';
export * from './lib/views/restworld-edit-view/restworld-edit-view.component';
export * from './lib/views/restworld-file-view/restworld-file-view.component'
export * from './lib/views/restworld-image-view/restworld-image-view.component'
export * from './lib/views/restworld-list-view/restworld-list-view.component'

export * from './lib/restworld-client.module';