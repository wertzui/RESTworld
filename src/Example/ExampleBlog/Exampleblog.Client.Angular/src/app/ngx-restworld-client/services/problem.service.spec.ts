import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { form } from '@angular/forms/signals';
import { ProblemDetails } from '@wertzui/ngx-hal-client';
import { MessageService } from 'primeng/api';

import { ProblemService } from './problem.service';
import { getChildFieldTree } from '../util/field-tree';

interface TestModel {
  name: string;
  author: {
    name: string;
  };
}

describe('ProblemService', () => {
  let service: ProblemService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ProblemService, MessageService]
    });
    service = TestBed.inject(ProblemService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('problemDetailsToSignalFormValidationErrors', () => {
    const createTestForm = () => TestBed.runInInjectionContext(() => {
      const model = signal<TestModel>({ name: 'Post 1', author: { name: 'John Doe' } });
      return form(model);
    });

    it('should return an empty array when there is no detail and no errors', () => {
      const testForm = createTestForm();
      const problemDetails = new ProblemDetails({ _links: { self: [{ href: '' }] } });

      const result = service.problemDetailsToSignalFormValidationErrors(problemDetails, testForm as any);

      expect(result).toEqual([]);
    });

    it('should include a root-level error for the problem detail', () => {
      const testForm = createTestForm();
      const problemDetails = new ProblemDetails({ _links: { self: [{ href: '' }] }, detail: 'Something went wrong' });

      const result = service.problemDetailsToSignalFormValidationErrors(problemDetails, testForm as any);

      expect(result.some(e => e.kind === 'remote' && e.message === 'Something went wrong' && e.fieldTree === undefined)).toBe(true);
    });

    it('should resolve field-specific errors to the correct field tree', () => {
      const testForm = createTestForm();
      const problemDetails = new ProblemDetails({
        _links: { self: [{ href: '' }] },
        errors: { 'author.name': ['The name is required.'] }
      });

      const result = service.problemDetailsToSignalFormValidationErrors(problemDetails, testForm as any);

      const authorField = getChildFieldTree(testForm as any, 'author');
      const nameField = getChildFieldTree(authorField as any, 'name');

      const fieldError = result.find(e => e.fieldTree === nameField);
      expect(fieldError).toBeTruthy();
      expect(fieldError?.kind).toBe('remote');
      expect(fieldError?.message).toContain('The name is required.');
    });

    it('should default to the root field when a path cannot be resolved', () => {
      const testForm = createTestForm();
      const problemDetails = new ProblemDetails({
        _links: { self: [{ href: '' }] },
        errors: { 'doesNotExist': ['Unknown error.'] }
      });

      const result = service.problemDetailsToSignalFormValidationErrors(problemDetails, testForm as any);

      const error = result.find(e => e.message === 'Unknown error.');
      expect(error).toBeTruthy();
      expect(error?.fieldTree).toBeUndefined();
    });
  });

  describe('scrollToFirstValidationError', () => {
    // Regression test: the selector used to only match the Reactive Forms `<rw-validation-errors>` DOM
    // structure (ngx-valdemort's `<val-errors><div>...`), so it silently found nothing - and never scrolled -
    // for forms built with `<rw-signal-form>`/`<rw-signal-validation-errors>`, which renders its own
    // `<div class="rw-signal-validation-errors"><p-message>...` structure instead.
    let scrollIntoViewSpy: ReturnType<typeof vi.fn>;

    beforeEach(() => {
      vi.useFakeTimers();
      document.body.innerHTML = '';
      scrollIntoViewSpy = vi.fn();
    });

    afterEach(() => {
      document.body.innerHTML = '';
      vi.useRealTimers();
    });

    it('should scroll to the first error for the Reactive Forms <rw-validation-errors> DOM structure', () => {
      document.body.innerHTML = '<rw-validation-errors><val-errors><div>The field is required.</div></val-errors></rw-validation-errors>';
      const errorDiv = document.querySelector('rw-validation-errors>val-errors>div') as Element;
      (errorDiv as unknown as { scrollIntoView: typeof scrollIntoViewSpy }).scrollIntoView = scrollIntoViewSpy;

      ProblemService.scrollToFirstValidationError();
      vi.advanceTimersByTime(100);

      expect(scrollIntoViewSpy).toHaveBeenCalledWith({ behavior: 'smooth', block: 'center' });
    });

    it('should scroll to the first error for the Signal Forms <rw-signal-validation-errors> DOM structure', () => {
      document.body.innerHTML = '<rw-signal-validation-errors><div class="rw-signal-validation-errors"><p-message>The field is required.</p-message></div></rw-signal-validation-errors>';
      const errorMessage = document.querySelector('rw-signal-validation-errors>div.rw-signal-validation-errors>p-message') as Element;
      (errorMessage as unknown as { scrollIntoView: typeof scrollIntoViewSpy }).scrollIntoView = scrollIntoViewSpy;

      ProblemService.scrollToFirstValidationError();
      vi.advanceTimersByTime(100);

      expect(scrollIntoViewSpy).toHaveBeenCalledWith({ behavior: 'smooth', block: 'center' });
    });

    it('should not throw when there are no validation errors in the DOM', () => {
      expect(() => {
        ProblemService.scrollToFirstValidationError();
        vi.advanceTimersByTime(100);
      }).not.toThrow();
    });
  });
});
