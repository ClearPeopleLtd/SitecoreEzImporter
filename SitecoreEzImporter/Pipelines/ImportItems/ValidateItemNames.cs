using System.Linq;
using EzImporter.Map;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EzImporter.Pipelines.ImportItems
{
    public class ValidateItemNames : ImportItemsProcessor
    {
        private const string DEFAULT_ITEM_NAME_VALIDATION = @"^[\w\*\$][\w\s\-\$]*(\(\d{1,}\)){0,1}$";

        public List<string> Errors { get; protected set; }
        private Regex ItemNameValidationRegex;

        public ValidateItemNames()
        {
            Errors = new List<string>();
            var itemNameValidationPattern = Sitecore.Configuration.Settings.GetSetting("ItemNameValidation", DEFAULT_ITEM_NAME_VALIDATION);
            ItemNameValidationRegex = new System.Text.RegularExpressions.Regex(itemNameValidationPattern, RegexOptions.ECMAScript);
        }

        public override void Process(ImportItemsArgs args)
        {
            Errors = new List<string>();
            foreach (var item in args.ImportItems)
            {
                ValidateName(item);
            }
            if (Errors.Any())
            {
                args.AddMessage("Invalid item name(s) in import data.");
                args.ErrorDetail = string.Join("\n\n", Errors);
                //args.AbortPipeline();
            }
        }

        public void ValidateName(ItemDto item)
        {
            if (!ItemNameValidationRegex.IsMatch(item.Name))
            {
                Errors.Add(string.Format("Invalid item name '{0}'.", item.Name));
            }
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    ValidateName(child);
                }
            }
        }
    }
}