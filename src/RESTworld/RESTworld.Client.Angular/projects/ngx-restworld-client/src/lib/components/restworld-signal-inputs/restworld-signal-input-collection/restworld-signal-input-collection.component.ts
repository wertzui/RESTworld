import { Component, computed, forwardRef } from '@angular/core';
import type { FieldTree } from '@angular/forms/signals';
import { Property, SimpleValue, SignalFormService, Template } from '@wertzui/ngx-hal-client';
import { MenuItem } from 'primeng/api';
import { RestWorldSignalInputLazyLoadBaseComponent } from '../restworld-signal-inputs';
import { RestWorldSignalTableComponent } from '../../restworld-signal-table/restworld-signal-table.component';

/**
 * A collection that is automatically created from the given property, using Signal Forms.
 * The collection can also be nested. Items can be added and removed by the user.
 * This is the Signal Forms equivalent of {@link RestWorldInputCollectionComponent} `<rw-input-collection>`.
 * @remarks
 * Mirrors `<rw-input-collection>` exactly: nests {@link RestWorldSignalTableComponent} `<rw-signal-table>`,
 * bound to this collection's own `FieldTree` via its `field` input in "bound mode" (the Signal Forms
 * equivalent of `<rw-table>`'s `[formArrayName]`/`ControlContainer` binding). This gives nested collections
 * the same full table experience (columns derived from the default template, inline editing, header/row
 * menus for add/delete) as the Reactive Forms version, rather than a simplified list editor.
 *
 * It is advised to use {@link RestWorldSignalInputComponent} `<rw-signal-input>` and control the rendered
 * input with the passed in property instead of using this component directly.
 * @example
 * <rw-signal-input-collection [property]="property" [apiName]="apiName" [field]="field"></rw-signal-input-collection>
 */
@Component({
    selector: 'rw-signal-input-collection',
    templateUrl: './restworld-signal-input-collection.component.html',
    styleUrls: ['./restworld-signal-input-collection.component.css'],
    imports: [forwardRef(() => RestWorldSignalTableComponent)]
})
export class RestWorldSignalInputCollectionComponent<TProperty extends Property<SimpleValue, string, string> & { _templates: { default: Template } } = Property<SimpleValue, string, string> & { _templates: { default: Template } }> extends RestWorldSignalInputLazyLoadBaseComponent<TProperty> {
    constructor(private readonly _signalFormService: SignalFormService) {
        super();
    }

    /**
     * The template that describes the shape of each item in the collection.
     */
    public readonly defaultTemplate = computed(() => this.property()._templates.default);

    /**
     * The `FieldTree` for the collection, re-typed as an array of `Record<string, unknown>` models so it can be
     * passed down to {@link RestWorldSignalTableComponent} `<rw-signal-table>`'s `field` input.
     * @remarks The base class types `field` as `FieldTree<ExtractValueType<TProperty>>`, which resolves to the
     * wide `SimpleValue` union for a `Collection` property (the generic parameter does not model array-of-object
     * shapes). This getter narrows it to the shape this component actually needs.
     */
    protected get innerField(): FieldTree<ReadonlyArray<Record<string, unknown>>> {
        return this.field() as unknown as FieldTree<ReadonlyArray<Record<string, unknown>>>;
    }

    /**
     * The current items in the collection, read directly from the {@link innerField}'s value signal.
     * @remarks Unlike the Reactive Forms version (which needs `toSignal(merge(toObservable(...).pipe(...), ...))`
     * to bridge a `FormArray`'s `valueChanges` into a signal), this is trivial with Signal Forms: `FieldState.value`
     * is already a plain `Signal`, so no RxJS bridge is needed.
     */
    public readonly rows = computed(() => this.innerField().value());

    /**
     * A menu, displayed at the top right of the nested {@link RestWorldSignalTableComponent} `<rw-signal-table>`,
     * with a single "add" entry (hidden when the property is read-only).
     */
    public readonly headerMenu = computed(() => {
        if (this.property().readOnly)
            return [];

        return [
            {
                icon: "fas fa-plus",
                styleClass: "p-button-outlined p-button-info",
                command: () => this.addItem()
            } as MenuItem
        ];
    });

    /**
     * A per-row menu for the nested {@link RestWorldSignalTableComponent} `<rw-signal-table>`, with a single
     * "delete" entry (hidden when the property is read-only).
     */
    public readonly rowMenu = computed(() => (row: Record<string, unknown>, openedByRightClick: boolean) => {
        if (this.property().readOnly)
            return [];

        return [
            {
                icon: "fas fa-trash-alt",
                label: openedByRightClick ? "Delete" : undefined,
                tooltip: !openedByRightClick ? "Delete" : undefined,
                tooltipPosition: "left",
                styleClass: "p-button-outlined p-button-danger",
                command: () => this.deleteItem(row)
            } as MenuItem
        ];
    });

    /**
     * Appends a new item to the collection, using the default values from the {@link defaultTemplate}.
     */
    public addItem(): void {
        const newItem = this._signalFormService.buildModelFromTemplate(this.defaultTemplate());
        this.innerField().value.update(current => [...current, newItem]);
    }

    /**
     * Removes the given row from the collection (looked up by reference in {@link rows}), mirroring
     * `RestWorldInputCollectionComponent.deleteItemFromCollection`.
     * @param row The row to remove.
     */
    public deleteItem(row: Record<string, unknown>): void {
        const index = this.rows().indexOf(row);
        this.removeItem(index);
    }

    /**
     * Removes the item at the given index from the collection.
     * @param index The index of the item to remove.
     */
    public removeItem(index: number): void {
        this.innerField().value.update(current => current.filter((_, i) => i !== index));
    }
}
