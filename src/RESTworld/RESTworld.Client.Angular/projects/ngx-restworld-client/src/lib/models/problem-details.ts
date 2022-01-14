import * as _ from "lodash";
import { Resource, ResourceOfDto } from "@wertzui/ngx-hal-client"
/*
 * ProblemDetails is what an ASP.net Core backend returns in case of an error.
 * */
export interface ProblemDetailsDto {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  [key: string]: unknown;
}

export class ProblemDetails extends Resource implements ResourceOfDto<ProblemDetailsDto> {
  public type?: string;
  public title?: string;
  public status?: number;
  public detail?: string;
  public instance?: string;
  [key: string]: unknown;

  public static isProblemDetails(resource: unknown): resource is ProblemDetails {
    return resource instanceof ProblemDetails;
  }

  public static containsProblemDetailsInformation(resource: unknown) {
    return resource && (resource instanceof ProblemDetails || (resource instanceof Resource && 'status' in resource && _.isNumber(resource['status']) && resource['status'] >= 100 && resource['status'] < 600));
  }

  public static fromResource(resource: Resource | null | undefined): ProblemDetails {
    if (!ProblemDetails.containsProblemDetailsInformation(resource))
      throw new Error(`The resource ${resource} does not have problem details.`);

    return Object.assign(new ProblemDetails(), resource);
  }
}
