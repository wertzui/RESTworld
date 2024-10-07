/**
 * Creates a debounced function that delays invoking the provided function until after the specified delay in milliseconds has elapsed since the last time the debounced function was invoked.
 *
 * @param fn - The function to debounce.
 * @param delayMilliseconds - The number of milliseconds to delay. Defaults to 300 milliseconds.
 * @returns A new debounced function.
 */
export const debounce = (fn: Function, delayMilliseconds = 300) => {
    let timeoutId: number | undefined;
    return function (this: any, ...args: any[]) {
      window.clearTimeout(timeoutId);
      timeoutId = window.setTimeout(() => fn.apply(this, args), delayMilliseconds);
    };
  };
