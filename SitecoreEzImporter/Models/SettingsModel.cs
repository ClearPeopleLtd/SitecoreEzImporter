namespace EzImporter.Models
{
    public class SettingsModel
    {
        public string ExistingItemHandling { get; set; }
        public string InvalidLinkHandling { get; set; }
        public string DataStructureType { get; set; }
        public string CsvDelimiter { get; set; }
        public string QuotationMark { get; set; }
        public string MultipleValuesSeparator { get; set; }
        public bool FirstRowAsColumnNames { get; set; }
    }
}