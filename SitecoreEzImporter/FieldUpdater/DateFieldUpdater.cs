using EzImporter.Configuration; 
using Sitecore.Data.Fields;
using System;
using System.Globalization;

namespace EzImporter.FieldUpdater
{
    public class DateFieldUpdater : IFieldUpdater
    {
        public void UpdateField(Field field, string importValue, IImportOptions importOptions)
        {
            field.Value = importValue;

      DateTime importeddate;
      if (System.DateTime.TryParseExact(importValue, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces,out importeddate))
      {
        field.Value = Sitecore.DateUtil.ToIsoDate(importeddate);
      }
      else if(System.DateTime.TryParse(importValue, out importeddate))
      {
        field.Value = Sitecore.DateUtil.ToIsoDate(importeddate);
      }
      else
      {
        field.Value = importValue;
      }
    }
    }
}