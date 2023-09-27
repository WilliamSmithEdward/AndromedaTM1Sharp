namespace AndromedaTM1Sharp
{
    /// <summary>
    /// Represents a reference to an element.
    /// </summary>
    public class ElementReference
    {
        /// <summary>
        /// Gets or sets the dimension of the element.
        /// </summary>
        public string Dimension { get; set; }

        /// <summary>
        /// Gets or sets the hierarchy of the element.
        /// </summary>
        public string Hierarchy { get; set; }

        /// <summary>
        /// Gets or sets the name of the element.
        /// </summary>
        public string Element { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementReference"/> class with the specified parameters.
        /// </summary>
        /// <param name="dimension">The dimension of the element.</param>
        /// <param name="hierarchy">The hierarchy of the element.</param>
        /// <param name="element">The name of the element.</param>
        public ElementReference(string dimension, string hierarchy, string element)
        {
            Dimension = dimension;
            Hierarchy = hierarchy;
            Element = element;
        }
    }
}
