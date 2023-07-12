namespace AndromedaTM1Sharp
{
    public class CellReference
    {
        public List<ElementReference> Elements { get; private set; }
        public string Value { get; set; }

        public CellReference(List<ElementReference> elements, string value) 
        { 
            Elements = elements;
            Value = value;
        }
    }
}
