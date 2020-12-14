using Newtonsoft.Json.Linq;
using System.Linq;

namespace Omnia.CLI.Commands.Model.Apply
{
    public class FormElementPathFinder
    {
        private readonly JObject _metatada;
        public FormElementPathFinder(JObject metadata)
        {
            _metatada = metadata;
        }

        public string Find(string definition, string element)
            => FindRecursive(_metatada, definition, element);

        private static string FindRecursive(JObject metadata, string definition, string element)
        {
            if (!metadata.ContainsKey("elements"))
                return null;

            var searchDefinition = definition;
            if (IsRootUIDefinition(metadata) && NameIsEqualTo(metadata, definition))
                searchDefinition = null;

            foreach (var item in metadata["elements"].Children().Select((item, index) => new { Item = item, Index = index }))
            {
                if (string.IsNullOrEmpty(searchDefinition) &&
                    item.Item.Children<JProperty>().Any(e => NameIsEqualTo(e, element)))
                    return $"/elements/{item.Index}";

                if (item.Item.Children<JProperty>().Any(e => e.Name.Equals("type") && e.Value.ToString().Equals("List", System.StringComparison.InvariantCultureIgnoreCase)))
                {
                    var itemAsObject = item.Item.ToObject<JObject>();

                    string listSearchDefinition = null;

                    if (!itemAsObject.ContainsKey("name") || !NameIsEqualTo(itemAsObject, searchDefinition))
                        listSearchDefinition = searchDefinition;

                    var path = FindRecursive(itemAsObject, listSearchDefinition, element);

                    if (path == null)
                        continue;
                    return $"/elements/{item.Index}{path}";
                }

                if (item.Item is JObject itemObject && itemObject.ContainsKey("elements"))
                {
                    var path = FindRecursive(item.Item.ToObject<JObject>(), searchDefinition, element);
                    if (path == null)
                        continue;
                    return $"/elements/{item.Index}{path}";
                }
            }
            return null;
        }

        private static bool NameIsEqualTo(JProperty e, string element)
            => e.Name.Equals("name") && e.Value.ToString().Equals(element, System.StringComparison.InvariantCultureIgnoreCase);

        private static bool NameIsEqualTo(JObject metadata, string definition)
            => metadata["name"].Value<string>().Equals(definition, System.StringComparison.InvariantCultureIgnoreCase);

        private static bool IsRootUIDefinition(JObject metadata)
            => IsForm(metadata) || IsDashboard(metadata) || IsMenu(metadata);

        private static bool IsForm(JObject metadata)
            => metadata.ContainsKey("type") && metadata["type"].Value<string>().Equals("Form", System.StringComparison.InvariantCultureIgnoreCase);

        private static bool IsDashboard(JObject metadata)
            => metadata.ContainsKey("type") && metadata["type"].Value<string>().Equals("Dashboard", System.StringComparison.InvariantCultureIgnoreCase);

        private static bool IsMenu(JObject metadata)
            => metadata.ContainsKey("type") && metadata["type"].Value<string>().Equals("Menu", System.StringComparison.InvariantCultureIgnoreCase);
    }
}
