#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AC;

namespace MCPTools.AdventureCreator.Editor
{
    public partial class Tool_AdventureCreator
    {
        [McpPluginTool("ac-list-inventory", Title = "Adventure Creator / List Inventory")]
        [Description(@"Lists all inventory items defined in the Adventure Creator Inventory Manager.
Shows item ID, label, category, and count. Also lists categories and recipes.
Does not require play mode.")]
        public string ListInventory()
        {
            return MainThread.Instance.Run(() =>
            {
                var inv = KickStarter.inventoryManager;
                if (inv == null)
                    return "ERROR: No Inventory Manager assigned in Adventure Creator.";

                var sb = new StringBuilder();

                // Categories
                sb.AppendLine("=== INVENTORY CATEGORIES ===");
                if (inv.bins.Count > 0)
                {
                    foreach (var bin in inv.bins)
                        sb.AppendLine($"  [{bin.id}] \"{bin.label}\"");
                }
                else
                    sb.AppendLine("  (none)");

                // Items
                sb.AppendLine($"\n=== INVENTORY ITEMS ({inv.items.Count}) ===");
                int limit = System.Math.Min(inv.items.Count, 100);
                for (int i = 0; i < limit; i++)
                {
                    var item = inv.items[i];
                    string category = "";
                    if (item.binID >= 0)
                    {
                        var bin = inv.GetCategory(item.binID);
                        if (bin != null) category = $" | Category: {bin.label}";
                    }
                    sb.AppendLine($"  [{item.id}] \"{item.label}\"{category}");
                }
                if (inv.items.Count > 100)
                    sb.AppendLine($"  ... and {inv.items.Count - 100} more items.");

                // Recipes
                sb.AppendLine($"\n=== RECIPES ({inv.recipes.Count}) ===");
                int recipeLimit = System.Math.Min(inv.recipes.Count, 50);
                for (int i = 0; i < recipeLimit; i++)
                {
                    var recipe = inv.recipes[i];
                    sb.AppendLine($"  [{recipe.id}] \"{recipe.label}\"");
                }

                // Documents
                sb.AppendLine($"\n=== DOCUMENTS ({inv.documents.Count}) ===");
                int docLimit = System.Math.Min(inv.documents.Count, 50);
                for (int i = 0; i < docLimit; i++)
                {
                    var doc = inv.documents[i];
                    sb.AppendLine($"  [{doc.ID}] \"{doc.title}\"");
                }

                return sb.ToString();
            });
        }
    }
}
