namespace PacketToSQL.Types;

public class Shop
{
    public  int Id;
    public int Category;
    public string Name;
    public ShopType ShopType;
    public bool HideUnuseable;
    public bool HideStats;
    public bool DisableBuyback;
    public bool OpenWallet;
    public bool DisplayNew;
    public bool RandomizeOrder;
    public bool CanRestock;
    public long NextRestock;
    public int RestockMinInterval;
    public byte RestockInterval;
    public byte RestockCurrencyType;
    public byte ExcessRestockCurrencyType;
    public int RestockCost;
    public bool EnableRestockCostMultiplier;
    public int TotalRestockCount;
    public bool DisableInstantRestock;
    public bool PersistantInventory;
    public int PullCount;
    public List<ShopItem> Items;

    public Shop() { }

    public Shop(int id, int category, string name, byte shop_type, bool restrict_sales, bool can_restock, long next_restock, bool allow_buyback)
    {
        Id = id;
        Category = category;
        Name = name;
        ShopType = (ShopType) shop_type;
        DisableBuyback = restrict_sales;
        CanRestock = can_restock;
        NextRestock = next_restock;
        HideUnuseable = allow_buyback;
    }
}

public class ShopItem
{
    public int Uid;
    public int ItemId;
    public ShopCurrencyType CurrencyType;
    public int RequiredItemId;
    public int Price;
    public int SalePrice;
    public byte Rarity;
    public int StockCount;
    public int StockPurchased;
    public int GuildTrophy;
    public string Category;
    public int RequiredAchievementId;
    public int RequiredAchievementGrade;
    public byte RequiredChampionshipGrade;
    public short RequiredChampionshipJoinCount;
    public byte RequiredGuildMerchantType;
    public short RequiredGuildMerchantLevel;
    public short Quantity;
    public ShopItemLabel Label;
    public string CurrencyId;
    public short RequiredQuestAlliance;
    public int RequiredFameGrade;
    public bool AutoPreviewEquip;

    public ShopItem() { }

    public ShopItem(dynamic data)
    {
        Uid = data.uid;
        AutoPreviewEquip = data.auto_preview_equip;
        Category = data.category;
        Label = (ShopItemLabel) data.label;
        GuildTrophy = data.guild_trophy;
        ItemId = data.item_id;
        Rarity = data.rarity;
        Price = data.price;
        RequiredAchievementGrade = data.required_achievement_grade;
        RequiredAchievementId = data.required_achievement_id;
        RequiredChampionshipGrade = data.required_championship_grade;
        RequiredChampionshipJoinCount = data.required_championship_join_count;
        RequiredFameGrade = data.required_fame_grade;
        RequiredGuildMerchantLevel = data.required_guild_merchant_level;
        RequiredGuildMerchantType = data.required_guild_merchant_type;
        RequiredItemId = data.required_item_id;
        RequiredQuestAlliance = data.required_quest_alliance;
        SalePrice = data.sale_price;
        StockCount = data.stock_count;
        StockPurchased = data.stock_purchased;
        CurrencyId = data.template_name;
        CurrencyType = (ShopCurrencyType) data.currency_type;
        Quantity = data.quantity;
    }
}

public enum ShopCurrencyType : byte
{
    Meso = 0,
    Item = 1,
    ValorToken = 2,
    Treva = 3,
    Meret = 4,
    Rue = 5,
    HaviFruit = 6,
    GuildCoin = 7,
    ReverseCoin = 8,
    EventMeret = 9,
    GameMeret = 10,
    MentorPoints = 11,
    MenteePoints = 12,
    EventToken = 13
}

public enum ShopType : byte
{
    Default = 0,
    Unk = 1,
    Star = 2,
    StyleCrate = 3,
    Capsule = 4
}

public enum ShopItemLabel : byte
{
    None = 0,
    New = 1,
    Event = 2,
    HalfPrice = 3,
    Special = 4
}