using System.Collections;

namespace Import.Settings.Interfaces;

public interface IShopImportSettings
{
    public bool? Perfomance { get; set; }

    IDictionary Services { get; }

    string ShopName { get; set; }

    string ShopUrl { get; set; }
}
