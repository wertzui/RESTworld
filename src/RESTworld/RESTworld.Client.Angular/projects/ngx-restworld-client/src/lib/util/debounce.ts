export const debounce = (fn: Function, delayMilliseconds = 300) => {
    let timeoutId: number | undefined;
    return function (this: any, ...args: any[]) {
      window.clearTimeout(timeoutId);
      timeoutId = window.setTimeout(() => fn.apply(this, args), delayMilliseconds);
    };
  };