import { Injectable, Input } from '@angular/core';
import { SafeUrl } from '@angular/platform-browser';

@Injectable({
  providedIn: 'root',
})
export class AvatarGenerator {
  private static _nonWordRegex = new RegExp('\\W');
  private static _imageCache: Map<string, SafeUrl> = new Map<string, SafeUrl>();

  public getImageAsyncOverride: (nameOrEmail: string) => Promise<SafeUrl | null | undefined> = () => Promise.resolve(undefined);

  public async getImageAsync(nameOrEmail: string): Promise<SafeUrl | null | undefined> {
    let promise = AvatarGenerator._imageCache.get(nameOrEmail);

    if (promise === undefined) {
      promise = this.getImageAsyncOverride(nameOrEmail);
      AvatarGenerator._imageCache.set(nameOrEmail, promise);
    }

    return promise;
  }

  public async getLabelAsync(nameOrEmail: string): Promise<string> {
    if (!nameOrEmail)
      return '';

    if (await this.getImageAsync(nameOrEmail))
      return '';

    const name = AvatarGenerator.getLocalPartOfEmailAddress(nameOrEmail);
    const initials = AvatarGenerator.getTwoUppercaseLettersFromName(name);

    return initials;
  }

  public async getStyleAsync(nameOrEmail: string,): Promise<'' | { 'background-color': string; color: string }> {
    if (await this.getImageAsync(nameOrEmail))
      return '';

    const foregroundColor = '#ffffff';
    const backgroundColor = AvatarGenerator.getBackgroundColor(nameOrEmail);

    const style = {
      'background-color': backgroundColor,
      color: foregroundColor,
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
