import { Injectable, Input } from '@angular/core';
import { SafeUrl } from '@angular/platform-browser';

@Injectable({
  providedIn: 'root',
})
/**
 * A class that generates avatars based on a given name or email.
 */
export class AvatarGenerator {
  private static _nonWordRegex = new RegExp('\\W');
  private static _imageCache: Map<string, SafeUrl> = new Map<string, SafeUrl>();

  /**
   * Override this method to provide a custom implementation for retrieving the image for a user.
   * If this method is not overridden, the image will always be empty, resulting in the avatar always be shown as label.
   * @param nameOrEmail The name or email of the user whose image should be retrieved.
   * @returns A Promise that resolves with a SafeUrl object representing the user's image, or an empty string if no image is available.
   */
  public getImageAsyncOverride: (nameOrEmail: string) => Promise<SafeUrl> = () => Promise.resolve('');

  /**
   * Retrieves the image for the given name or email address from the cache if it exists,
   * otherwise retrieves it from the server and caches it for future use.
   * @param nameOrEmail The name or email address of the user to retrieve the image for.
   * @returns A Promise that resolves to a SafeUrl representing the retrieved image.
   */
  public async getImageAsync(nameOrEmail: string): Promise<SafeUrl> {
    let promise = AvatarGenerator._imageCache.get(nameOrEmail);

    if (promise === undefined) {
      promise = this.getImageAsyncOverride(nameOrEmail);
      AvatarGenerator._imageCache.set(nameOrEmail, promise);
    }

    return promise;
  }

  /**
   * Gets the label for the avatar based on the given name or email.
   * If an image is available for the given name or email, an empty string is returned.
   * Otherwise, the label is generated from the name or email.
   * The label always consists of 2 uppercase letters.
   * @param nameOrEmail The name or email to generate the label from.
   * @returns The label for the avatar.
   */
  public async getLabelAsync(nameOrEmail: string): Promise<string> {
    if (!nameOrEmail)
      return '';

    if (await this.getImageAsync(nameOrEmail))
      return '';

    const name = AvatarGenerator.getLocalPartOfEmailAddress(nameOrEmail);
    const initials = AvatarGenerator.getTwoUppercaseLettersFromName(name);

    return initials;
  }

  /**
   * Gets the style object for the avatar based on the given name or email.
   * @param nameOrEmail - The name or email of the user.
   * @returns A Promise that resolves to the style object containing the background color and foreground color of the avatar.
   * If the image for the user is found, returns undefined.
   */
  public async getStyleAsync(nameOrEmail: string,): Promise<{ 'background-color': string; 'color': string } | undefined> {
    if (await this.getImageAsync(nameOrEmail))
      return undefined;

    const foregroundColor = '#ffffff';
    const backgroundColor = AvatarGenerator.getBackgroundColor(nameOrEmail);

    const style = {
      'background-color': backgroundColor,
      'color': foregroundColor,
    };

    return style;
  }

  private static getLocalPartOfEmailAddress(email: string): string {
    const indexOfAt = email.indexOf('@');
    if (indexOfAt < 2) {
      if (indexOfAt >= 0) {
        if (indexOfAt === email.length - 1)
          return email.substring(0, indexOfAt);
        return email.substring(indexOfAt + 1);
      }
    } else {
      return email.substring(0, indexOfAt);
    }

    return email;
  }

  private static getTwoUppercaseLettersFromName(name: string): string {
    // Too short to do anything besides just returning the name
    if (name.length <= 2) {
      return name.toUpperCase();
    }

    // Try to split by non word characters
    const splittedByNonWord = name.split(AvatarGenerator._nonWordRegex);
    if (splittedByNonWord.length > 1) {
      return (splittedByNonWord[0][0] + splittedByNonWord[1][0]).toUpperCase();
    }

    // Try to split by upper case letters
    const upperCaseLetters = [...name]
      .filter((c) => c.toUpperCase() === c && !AvatarGenerator._nonWordRegex.test(c))
      .join();
    if (upperCaseLetters.length > 1) {
      return upperCaseLetters.substring(0, 2);
    }

    // Just return the first 2 letters
    return name.substring(0, 2).toUpperCase();
  }

  // from https://stackoverflow.com/a/66494926/1378307
  private static getBackgroundColor(
    text: string,
    minLightness = 40,
    maxLightness = 80,
    minSaturation = 30,
    maxSaturation = 100,
  ): string {
    if (!text) return '#aaa';

    const hash = [...text].reduce((acc, char) => {
      return char.charCodeAt(0) + ((acc << 5) - acc);
    }, 0);

    return (
      'hsl(' +
      (hash % 360) +
      ', ' +
      ((hash % (maxSaturation - minSaturation)) + minSaturation) +
      '%, ' +
      ((hash % (maxLightness - minLightness)) + minLightness) +
      '%)'
    );
  }
}
