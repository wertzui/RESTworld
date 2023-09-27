/**
 * Represents the OData query parameters that can be used to filter, sort, and paginate data.
 * It is normally used to query list resources in conjunction with the `ODataService`.
 */
export interface ODataParameters {
  /**
   * Filters the data based on a Boolean expression.
   */
  $filter?: string;
  /**
   * Sorts the data in ascending or descending order.
   */
  $orderby?: string;
  /**
   * Limits the number of items returned in the result set.
   */
  $top?: number;
  /**
   * Skips a specified number of items in the result set.
   */
  $skip?: number;
}
