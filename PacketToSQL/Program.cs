using Maple2.PacketLib.Tools;
using PacketToSQL.MapleShark2_Files;
using PacketToSQL.Types;

Console.WriteLine("Welcome to the MapleShark2 sniff to MapleServer 2 Shop SQL.");

Begin:

string appPath = Path.GetFullPath(AppContext.BaseDirectory);
Console.WriteLine($"Enter the folder path: (leave blank to use current folder '{appPath}')");

string path = Console.ReadLine();
if (string.IsNullOrEmpty(path))
{
    path = appPath;
}

if (!Directory.Exists(path))
{
    Console.WriteLine("Could not find path. Press Y to try again or N to leave");
    ConsoleKeyInfo key = Console.ReadKey();
    if (key.Key is ConsoleKey.Y)
    {
        goto Begin;
    }

    return;
}

string[] files = Directory.GetFiles(path, "*.msb", SearchOption.AllDirectories);
if (files.Length == 0)
{
    Console.WriteLine("No .msb files found. Press any key to close...");
    Console.ReadKey();
    return;
}

Console.WriteLine($"Found {files.Length} .msb files. Starting conversion now...");

foreach (string file in files)
{
    (MsbMetadata metadata, IEnumerable<MaplePacket> packets) = FileLoader.ReadMsbFile(file);

    int opCode = metadata.Build == 12 ? 82 : 81;

    Shop shop = null;
    List<ShopItem> shopItems = new();
    foreach (MaplePacket packet in packets.Where(x => x.Opcode == opCode && !x.Outbound))
    {
        byte mode = packet.ReadByte();
        switch (mode)
        {
            case 0:
                shop = ReadShop(packet);
                break;
            case 1:
                ShopItem item = ReadShopItem(packet);
                shopItems.Add(item);
                break;
        }
    }

    if (shop is null || shopItems.Count == 0)
    {
        continue;
    }

    CreateShopSqlFile(shop);
    CreateShopItemsSqlFile(shopItems, shop.Id);

    Console.Write(".");
}

Console.WriteLine();
Console.WriteLine("Done. Press any key to close...");
Console.ReadKey();

Shop ReadShop(MaplePacket packet)
{
    int npcId = packet.ReadInt();
    int id = packet.ReadInt();
    long nextRestock = packet.ReadLong();
    packet.ReadInt();
    packet.ReadShort();
    int category = packet.ReadInt();
    packet.ReadBool();
    bool restrictSales = packet.ReadBool();
    bool canRestock = packet.ReadBool();
    packet.ReadBool();
    ShopType shopType = (ShopType) packet.ReadByte();
    bool allowBuyback = packet.ReadBool();
    packet.ReadBool();
    packet.ReadBool();
    packet.ReadBool();
    string name = packet.ReadString();

    return new()
    {
        AllowBuyback = allowBuyback,
        CanRestock = canRestock,
        Category = category,
        Id = id,
        Name = name,
        NextRestock = nextRestock,
        RestrictSales = restrictSales,
        ShopType = shopType
    };
}

ShopItem ReadShopItem(MaplePacket packet)
{
    packet.ReadByte();
    int uid = packet.ReadInt();
    int itemId = packet.ReadInt();
    byte tokenType = packet.ReadByte();
    int requiredItemId = packet.ReadInt();
    packet.ReadInt();
    int price = packet.ReadInt();
    int salePrice = packet.ReadInt();
    byte itemRank = packet.ReadByte();
    packet.ReadInt();
    int stockCount = packet.ReadInt();
    int stockPurchased = packet.ReadInt();
    int guildTrophy = packet.ReadInt();
    string category = packet.ReadString();
    int requiredAchievementId = packet.ReadInt();
    int requiredAchievementGrade = packet.ReadInt();
    byte requiredChampionshipGrade = packet.ReadByte();
    short requiredChampionshipJoinCount = packet.ReadShort();
    byte requiredGuildMerchantType = packet.ReadByte();
    short requiredGuildMerchantLevel = packet.ReadShort();
    packet.ReadBool();
    short quantity = packet.ReadShort();
    packet.ReadByte();
    byte flag = packet.ReadByte();
    string templateName = packet.ReadString();
    short requiredQuestAlliance = packet.ReadShort();
    int requiredFameGrade = packet.ReadInt();
    bool autoPreviewEquip = packet.ReadBool();
    packet.ReadByte();

    return new()
    {
        AutoPreviewEquip = autoPreviewEquip,
        Category = category,
        Flag = (ShopItemFlag) flag,
        GuildTrophy = guildTrophy,
        ItemId = itemId,
        ItemRank = itemRank,
        Price = price,
        RequiredAchievementGrade = requiredAchievementGrade,
        RequiredAchievementId = requiredAchievementId,
        RequiredChampionshipGrade = requiredChampionshipGrade,
        RequiredChampionshipJoinCount = requiredChampionshipJoinCount,
        RequiredFameGrade = requiredFameGrade,
        RequiredGuildMerchantLevel = requiredGuildMerchantLevel,
        RequiredGuildMerchantType = requiredGuildMerchantType,
        RequiredItemId = requiredItemId,
        RequiredQuestAlliance = requiredQuestAlliance,
        SalePrice = salePrice,
        StockCount = stockCount,
        StockPurchased = stockPurchased,
        TemplateName = templateName,
        TokenType = (ShopCurrencyType) tokenType,
        Quantity = quantity
    };
}

async void CreateShopSqlFile(Shop shop)
{
    string sqlFolder = Path.Combine(appPath, "SQL Files");
    Directory.CreateDirectory(sqlFolder);

    string shopsFilePath = Path.Combine(sqlFolder, "Shops.sql");

    if (!File.Exists(shopsFilePath))
    {
        File.WriteAllLines(shopsFilePath, new[]
        {
            "-- SQL File created by PacketToSQL app by tDcc#0568", "INSERT INTO `shops` VALUES"
        });
    }

    await using StreamWriter file = new(shopsFilePath, append: true);

    string[] lines =
    {
        $"({shop.Id}, {shop.Category}, '{shop.Name}', {(byte) shop.ShopType}, {(shop.RestrictSales ? "1" : "0")}, {(shop.CanRestock ? "1" : "0")}, " +
        $"1630605560, {(shop.AllowBuyback ? "1" : "0")}),",
        "" // new line
    };

    foreach (string line in lines)
    {
        await file.WriteLineAsync(line);
    }
}

async void CreateShopItemsSqlFile(List<ShopItem> items, int shopId)
{
    string sqlFolder = Path.Combine(appPath, "SQL Files");
    Directory.CreateDirectory(sqlFolder);

    string shopsItemsFilePath = Path.Combine(sqlFolder, "ShopsItems.sql");

    if (!File.Exists(shopsItemsFilePath))
    {
        File.WriteAllLines(shopsItemsFilePath, new[]
        {
            "-- SQL File created by PacketToSQL app by tDcc#0568",
            @"INSERT INTO `shop_items` (`auto_preview_equip`, `category`, `flag`, `guild_trophy`, `item_id`, `item_rank`, `price`,
                            `quantity`, `required_achievement_grade`, `required_achievement_id`,
                            `required_championship_grade`, `required_championship_join_count`, `required_fame_grade`,
                            `required_guild_merchant_level`, `required_guild_merchant_type`, `required_item_id`,
                            `required_quest_alliance`, `sale_price`, `shop_id`, `stock_count`, `stock_purchased`,
                            `template_name`, `token_type`)
VALUES"
        });
    }

    await using StreamWriter file = new(shopsItemsFilePath, append: true);

    List<string> lines = new()
    {
        $"-- Shop ID {shopId}",
    };

    foreach (ShopItem shopItem in items)
    {
        lines.Add($"({(shopItem.AutoPreviewEquip ? "1" : "0")}, '{shopItem.Category}', {(int) shopItem.Flag}, {shopItem.GuildTrophy}, {shopItem.ItemId}, " +
                  $"{shopItem.ItemRank}, {shopItem.Price}, {shopItem.Quantity}, {shopItem.RequiredAchievementGrade}, {shopItem.RequiredAchievementId}, " +
                  $"{shopItem.RequiredChampionshipGrade}, {shopItem.RequiredChampionshipJoinCount}, {shopItem.RequiredFameGrade}, " +
                  $"{shopItem.RequiredGuildMerchantLevel}, {shopItem.RequiredGuildMerchantType}, {shopItem.RequiredItemId}, {shopItem.RequiredQuestAlliance}, " +
                  $"{shopItem.SalePrice}, {shopId}, 0, 0, '{shopItem.TemplateName}', {(int) shopItem.TokenType}),");
    }

    lines.Add("");
    lines.Add("");

    foreach (string line in lines)
    {
        await file.WriteLineAsync(line);
    }
}