namespace EzImporter.Configuration
{
    public interface IImportOptions
    {
        InvalidLinkHandling InvalidLinkHandling { get; set; }
        ExistingItemHandling ExistingItemHandling { get; set; }
        DataStructureType DataStructureType { get; set; }
        string MultipleValuesImportSeparator { get; set; }
        string TreePathValuesImportSeparator { get; set; }
        string[] CsvDelimiter { get; set; }
        string QuotationMark { get; set; }
        bool FirstRowAsColumnNames { get; set; }
    }
}
