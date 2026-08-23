import type { FieldTree } from '@angular/forms/signals';

/**
 * Gets the child `FieldTree` for the given property name from a parent `FieldTree` that wraps an object.
 * @remarks
 * Signal Forms schemas are normally written against a statically known model shape (`schemaPath.someProperty`).
 * RESTworld builds forms dynamically from HAL-FORMS templates returned by the server, so property names are
 * only known at runtime. This helper bridges that gap by looking up a child field by its runtime name.
 * @param parent The parent `FieldTree` to look up the child in.
 * @param name The name of the property to look up.
 * @returns The child `FieldTree`, or `undefined` if the parent does not have a field with that name.
 * @example
 * const nameField = getChildFieldTree<string>(form, 'name');
 */
export function getChildFieldTree<TValue = unknown>(parent: FieldTree<Record<string, unknown>>, name: string): FieldTree<TValue> | undefined {
    return (parent as unknown as Record<string, FieldTree<TValue> | undefined>)[name];
}

/**
 * Gets the `FieldTree` for the item at the given index from a parent `FieldTree` that wraps an array.
 * @param parent The parent `FieldTree` to look up the item in.
 * @param index The index of the item to look up.
 * @returns The item `FieldTree`, or `undefined` if the parent does not have an item at that index.
 * @example
 * const firstItemField = getFieldTreeItemAt<MyItem>(form.items, 0);
 */
export function getFieldTreeItemAt<TValue = unknown>(parent: FieldTree<ReadonlyArray<unknown>>, index: number): FieldTree<TValue> | undefined {
    return (parent as unknown as ReadonlyArray<FieldTree<TValue>>)[index];
}

/**
 * Resolves a `FieldTree` at the given dotted/bracketed path, starting from the given root `FieldTree`.
 * @remarks
 * This is the Signal Forms equivalent of walking a Reactive Forms `AbstractControl` tree by path segments
 * (e.g. as used to attach server-side validation errors returned in a `ProblemDetails.errors` object, where
 * keys are paths like `"$.author.name"` or `"items[0].name"`).
 * @param root The root `FieldTree` to start resolving the path from.
 * @param path The path to resolve, e.g. `"$.author.name"` or `"items[0].name"`. A leading `$` (indicating the
 * root) is ignored.
 * @returns The resolved `FieldTree`, or `undefined` if any segment of the path could not be resolved.
 * @example
 * const nameField = getFieldTreeAtPath(form, '$.author.name');
 */
export function getFieldTreeAtPath(root: FieldTree<Record<string, unknown>>, path: string): FieldTree<unknown> | undefined {
    const segments = path.split(/\.|\[/).map(e => e.replace(']', '')).filter(e => e !== '');

    if (segments.length > 0 && segments[0] === '$')
        segments.shift();

    return segments.reduce<FieldTree<unknown> | undefined>((current, segment) => {
        if (current === undefined)
            return undefined;

        const index = Number.parseInt(segment);
        if (Number.isInteger(index) && String(index) === segment)
            return getFieldTreeItemAt(current as unknown as FieldTree<ReadonlyArray<unknown>>, index);

        return getChildFieldTree(current as unknown as FieldTree<Record<string, unknown>>, segment);
    }, root);
}
