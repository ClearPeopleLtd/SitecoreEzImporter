using System.Collections.Generic;

namespace EzImporter.Map
{
    public class InputField
    {
        public string Name { get; set; }
        
    public string XsltSelector { get; set; }
    public string Property { get; set; }
    public bool TextOnly { get; set; }
    public string ReplacementText { get; set; }
    public IList<InputField> Fields { get; set; }
    }
}
