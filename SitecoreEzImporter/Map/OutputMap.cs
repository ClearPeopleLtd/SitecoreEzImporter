using Sitecore.Data;
using System.Collections.Generic;

namespace EzImporter.Map
{
    public class OutputMap
    {
        public ID TemplateId { get; set; }
        public string NameInputField { get; set; }
    public string PathPattern { get; set; }
        public List<OutputField> Fields { get; set; }
        public List<OutputMap> ChildMaps { get; set; }
        public OutputMap ParentMap { get; set; }
        public DataType Type { get; set; }

        public OutputMap()
        {
            Fields = new List<OutputField>();
            ChildMaps = new List<OutputMap>();
            ParentMap = null;
      Type = DataType.Hierarchichal;
        }
    }

  public enum DataType
  {
    Tabular,
    Hierarchichal
  }
}