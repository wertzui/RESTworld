import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { form } from '@angular/forms/signals';

import { getChildFieldTree, getFieldTreeAtPath, getFieldTreeItemAt } from './field-tree';

interface TestModel {
  name: string;
  author: {
    name: string;
  };
  items: ReadonlyArray<{ name: string }>;
}

describe('getFieldTreeAtPath', () => {
  const createTestForm = () => TestBed.runInInjectionContext(() => {
    const model = signal<TestModel>({
      name: 'Post 1',
      author: { name: 'John Doe' },
      items: [{ name: 'Item 1' }, { name: 'Item 2' }]
    });
    return form(model);
  });

  it('should resolve a top-level property', () => {
    const testForm = createTestForm();

    const result = getFieldTreeAtPath(testForm as any, 'name');

    expect(result).toBe(getChildFieldTree(testForm as any, 'name'));
  });

  it('should resolve a nested property using dot notation', () => {
    const testForm = createTestForm();

    const result = getFieldTreeAtPath(testForm as any, 'author.name');

    const authorField = getChildFieldTree(testForm as any, 'author');
    expect(result).toBe(getChildFieldTree(authorField as any, 'name'));
  });

  it('should ignore a leading $ that indicates the root', () => {
    const testForm = createTestForm();

    const result = getFieldTreeAtPath(testForm as any, '$.author.name');

    const authorField = getChildFieldTree(testForm as any, 'author');
    expect(result).toBe(getChildFieldTree(authorField as any, 'name'));
  });

  it('should resolve an array item using bracket notation', () => {
    const testForm = createTestForm();

    const result = getFieldTreeAtPath(testForm as any, 'items[0].name');

    const itemsField = getChildFieldTree(testForm as any, 'items');
    const itemField = getFieldTreeItemAt(itemsField as any, 0);
    expect(result).toBe(getChildFieldTree(itemField as any, 'name'));
  });

  it('should return undefined when a segment cannot be resolved', () => {
    const testForm = createTestForm();

    const result = getFieldTreeAtPath(testForm as any, 'doesNotExist.name');

    expect(result).toBeUndefined();
  });

  it('should return the root field for an empty path', () => {
    const testForm = createTestForm();

    const result = getFieldTreeAtPath(testForm as any, '');

    expect(result).toBe(testForm as any);
  });
});
