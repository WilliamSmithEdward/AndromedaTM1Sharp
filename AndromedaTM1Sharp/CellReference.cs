namespace AndromedaTM1Sharp
{
    /// <summary>
    /// Represents a cell reference in TM1.
    /// </summary>
    public class CellReference
    {
        /// <summary>
        /// Gets or sets the list of element references associated with the cell.
        /// </summary>
        public List<ElementReference> Elements { get; private set; }

        /// <summary>
        /// Gets or sets the value of the cell.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CellReference"/> class with the specified parameters.
        /// </summary>
        /// <param name="elements">The list of element references associated with the cell.</param>
        /// <param name="value">The value of the cell.</param>
        public CellReference(List<ElementReference> elements, string? value = null)
        {
            Elements = elements;
            Value = value;
        }
    }
}
