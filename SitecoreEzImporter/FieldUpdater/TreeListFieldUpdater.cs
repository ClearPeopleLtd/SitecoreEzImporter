using EzImporter.Configuration;
using System;
using Sitecore.Data;
using System.Collections.Generic;

namespace EzImporter.FieldUpdater
{
  public class TreeListFieldUpdater : IFieldUpdater
  {
    static private Dictionary<ID, Dictionary<string, ID>> CachedValues;
    static public void ClearCache()
    {
      if (CachedValues != null)
        CachedValues.Clear();
    }
    public void UpdateField(Sitecore.Data.Fields.Field field, string importValue, IImportOptions importOptions)
    {
      if(!string.IsNullOrWhiteSpace(importValue))
      { 
      if (CachedValues == null)
        CachedValues = new Dictionary<ID, Dictionary<string, ID>>();
        try
        {
          Dictionary<string, ID> values;
          if (!CachedValues.TryGetValue(field.ID, out values))
          {
            values = new Dictionary<string, ID>();
            CachedValues.Add(field.ID, values);
          }

          var separator = new[] { importOptions.MultipleValuesImportSeparator };
          var selectionSource = field.Item.Database.SelectSingleItem(field.Source);
          var importValues = importValue != null
              ? importValue.Split(separator, StringSplitOptions.RemoveEmptyEntries)
              : new string[] { };
          var idListValue = "";
          foreach (var value in importValues)
          {
            ID target;
            if (!values.TryGetValue(value, out target))
            {
              var query = ID.IsID(value)
                  ? ".//*[@@id='" + ID.Parse(value) + "']"
                  : ".//*[@Value='" + value + "']";
              var item = selectionSource.Axes.SelectSingleItem(query);
              if (item != null)
              {
                target = item.ID;
                values.Add(value, target);
              }
            }
            if (!ID.IsNullOrEmpty(target))
            {
              idListValue += "|" + target;
            }
            else
            {
              if (importOptions.InvalidLinkHandling == InvalidLinkHandling.SetBroken)
              {
                idListValue += "|" + value;
              }
              else if (importOptions.InvalidLinkHandling == InvalidLinkHandling.SetEmpty)
              {
                idListValue += "|";
              }
            }
          }
          if (idListValue.StartsWith("|"))
          {
            idListValue = idListValue.Substring(1);
          }

          field.Value = idListValue;
        }
        catch (Exception ex)
        {
          if (importOptions.InvalidLinkHandling == InvalidLinkHandling.SetBroken)
          {
            field.Value = importValue;
          }
          else if (importOptions.InvalidLinkHandling == InvalidLinkHandling.SetEmpty)
          {
            field.Value = string.Empty;
          }
        }
      }
    }
  }
}