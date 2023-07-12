namespace AndromedaTM1Sharp
{
    public class ElementReference
    {
        public string Dimension { get; set; }
        public string Hierarchy { get; set; }
        public string Element { get; set; }

        public ElementReference(string dimension, string hierarchy, string element) 
        {
            Dimension = dimension;
            Hierarchy = hierarchy;
            Element = element;
        }
    }
}
