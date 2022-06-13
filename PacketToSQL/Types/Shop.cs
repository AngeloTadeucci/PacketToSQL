namespace PacketToSQL.Types;

public class Shop
{
    public int Id { get; set; }
    public int Category { get; set; }
    public string Name { get; set; }
    public ShopType ShopType { get; set; }
    public bool RestrictSales { get; set; }
    public bool CanRestock { get; set; }
    public long NextRestock { get; set; }
    public bool AllowBuyback { get; set; }
    public List<ShopItem> Items { get; set; }

    public Shop() { }

    public Shop(int id, int category, string name, byte shop_type, bool restrict_sales, bool can_restock, long next_restock, bool allow_buyback)
    {
        Id = id;
        Category = category;
        Name = name;
        ShopType = (ShopType) shop_type;
        RestrictSales = restrict_sales;
        CanRestock = can_restock;
        NextRestock = next_restock;
        AllowBuyback = allow_buyback;
    }
}

public class ShopItem
{
    public int Uid;
    public int ItemId;
    public ShopCurrencyType TokenType;
    public int RequiredItemId;
    public int Price;
    public int SalePrice;
    public byte ItemRank;
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
    public ShopItemFlag Flag;
    public string TemplateName;
    public short RequiredQuestAlliance;
    public int RequiredFameGrade;
    public bool AutoPreviewEquip;

    public ShopItem() { }

    public ShopItem(dynamic data)
    {
        Uid = data.uid;
        AutoPreviewEquip = data.auto_preview_equip;
        Category = data.category;
        Flag = (ShopItemFlag) data.flag;
        GuildTrophy = data.guild_trophy;
        ItemId = data.item_id;
        ItemRank = data.item_rank;
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
        TemplateName = data.template_name;
        TokenType = (ShopCurrencyType) data.token_type;
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

public enum ShopItemFlag : byte
{
    None = 0,
    New = 1,
    Event = 2,
    HalfPrice = 3,
    Special = 4
}