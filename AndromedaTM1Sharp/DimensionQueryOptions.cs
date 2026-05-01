namespace AndromedaTM1Sharp
{
    /// <summary>
    /// Controls optional attribute behavior for dimension member and rollup queries.
    /// </summary>
    public class DimensionQueryOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include all attributes.
        /// </summary>
        public bool IncludeAttributes { get; set; } = false;

        /// <summary>
        /// Gets or sets specific attribute names to include.
        /// Missing names are ignored.
        /// </summary>
        public List<string> AttributeNames { get; set; } = [];
    }
}
