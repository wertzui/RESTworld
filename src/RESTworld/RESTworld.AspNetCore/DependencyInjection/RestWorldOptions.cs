namespace RESTworld.AspNetCore.DependencyInjection
{
    public class RestWorldOptions
    {
        /// <summary>
        /// Gets or sets the maximum number items to return at the list endpoint.
        /// </summary>
        public int MaxNumberForListEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the curie name which is used to generate the prefix (the part before the ':') for your links on the home controller.
        /// </summary>
        public string CurieName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating wether the total count should be calculated for list endpoints. Calculating the total count means a second roundtrip to the database when calling the list endpoint.
        /// </summary>
        public bool CalculateTotalCountForListEndpoint { get; set; }
    }
}