namespace AndromedaTM1Sharp
{
    /// <summary>
    /// Represents a cell reference in TM1.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CellReference"/> class with the specified parameters.
    /// </remarks>
    /// <param name="elements">The list of element references associated with the cell.</param>
    /// <param name="value">The value of the cell.</param>
    public class CellReference(List<ElementReference> elements, object? value = null)
    {
        /// <summary>
        /// Gets or sets the list of element references associated with the cell.
        /// </summary>
        public List<ElementReference> Elements { get; private set; } = elements;

        /// <summary>
        /// Gets or sets the value of the cell.
        /// </summary>
        public object? Value { get; set; } = value;
    }
}
