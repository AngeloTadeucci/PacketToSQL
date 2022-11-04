using PacketToSQL.Helpers;
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

    bool isGms2 = metadata.Build == 12;
    int opCode = isGms2 ? 82 : 81;
    Dictionary<int, (Shop, List<ShopItem>)> shops = new();

    int lastShopId = 0;
    foreach (MaplePacket packet in packets.Where(x => x.Opcode == opCode && !x.Outbound))
    {
        byte mode = packet.ReadByte();
        switch (mode)
        {
            case 0:
                Shop shop = ReadShop(packet);
                lastShopId = shop.Id;
                shops[lastShopId] = (shop, new());
                break;
            case 1:
                if (lastShopId == -1)
                {
                    Console.WriteLine("Error: Tried to read shop item without reading shop first");
                    continue;
                }

                if (isGms2)
                {
                    ShopItem item = ReadShopItemGms2(packet);
                    shops[lastShopId].Item2.Add(item);
                    break;
                }

                List<ShopItem> items = ReadShopItemKms2(packet);
                shops[lastShopId].Item2.AddRange(items);

                break;
            case 6:
                lastShopId = -1;
                break;
        }
    }

    if (shops.Count == 0)
    {
        continue;
    }

    foreach ((int _, (Shop shop, List<ShopItem> shopItems)) in shops)
    {
        CreateShopSqlFile(shop);
        CreateShopItemsSqlFile(shopItems, shop.Id);
    }

    Console.Write(".");
}

Console.WriteLine();
Console.WriteLine("Done. Press any key to close...");
Console.ReadKey();

Shop ReadShop(MaplePacket packet)
{
    packet.ReadInt();
    int id = packet.ReadInt();
    long nextRestock = packet.ReadLong();
    nextRestock = 1666483200; // change restock of all shops to be at 12 AM GMT
    packet.ReadInt();
    int itemCount = packet.ReadShort();
    int category = packet.ReadInt();
    bool openWallet = packet.ReadBool();
    bool disableBuyBack = packet.ReadBool();
    bool canRestock = packet.ReadBool();
    bool randomizeOrder = packet.ReadBool();
    ShopType shopType = (ShopType) packet.ReadByte();
    bool hideUnuseable = packet.ReadBool();
    bool hideStats = packet.ReadBool();
    packet.ReadBool();
    bool displayNew = packet.ReadBool();
    string name = packet.ReadString();
    byte restockCurrencyType = 0;
    byte excessRestockCurrencyType = 0;
    int restockCost = 0;
    bool enableRestockCostMultiplier = false;
    int totalRestockCount = 0;
    byte restockInterval = 0;
    bool disableInstantRestock = false;
    bool persistantInventory = false;
    int pullCount = 0;
    int restockMinInterval = 0;

    if (canRestock)
    {
        restockCurrencyType = packet.ReadByte();
        excessRestockCurrencyType = packet.ReadByte();
        packet.ReadInt();
        restockCost = packet.ReadInt();
        enableRestockCostMultiplier = packet.ReadBool();
        totalRestockCount = packet.ReadInt();
        restockInterval = packet.ReadByte();
        disableInstantRestock = packet.ReadBool();
        persistantInventory = packet.ReadBool();
        pullCount = itemCount;
        restockMinInterval = restockInterval switch
        {
            0 => // minutes
                1,
            1 => // days
                1440,
            2 => // weeks
                10080,
            3 => 43200,
            _ => restockMinInterval
        };
    }
    else
    {
        nextRestock = 0;
    }

    return new()
    {
        HideUnuseable = hideUnuseable,
        CanRestock = canRestock,
        Category = category,
        Id = id,
        Name = name,
        NextRestock = nextRestock,
        DisableBuyback = disableBuyBack,
        ShopType = shopType,
        OpenWallet = openWallet,
        RandomizeOrder = randomizeOrder,
        HideStats = hideStats,
        DisplayNew = displayNew,
        RestockCurrencyType = restockCurrencyType,
        ExcessRestockCurrencyType = excessRestockCurrencyType,
        RestockCost = restockCost,
        EnableRestockCostMultiplier = enableRestockCostMultiplier,
        TotalRestockCount = totalRestockCount,
        RestockInterval = restockInterval,
        DisableInstantRestock = disableInstantRestock,
        PersistantInventory = persistantInventory,
        PullCount = pullCount,
        RestockMinInterval = restockMinInterval
    };
}

ShopItem ReadShopItemGms2(MaplePacket packet)
{
    packet.ReadByte(); // count
    return ReadItemShop(packet);
}

List<ShopItem> ReadShopItemKms2(MaplePacket packet)
{
    List<ShopItem> shopItems = new();
    byte count = packet.ReadByte();
    for (int i = 0; i < count; i++)
    {
        ShopItem item = ReadItemShop(packet);
        packet.ReadByte();
        packet.ReadKmsItem(item.ItemId);

        shopItems.Add(item);
    }


    return shopItems;
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
        $"({shop.Id}, {shop.Category}, '{shop.Name}', {(byte) shop.ShopType}, {(shop.HideUnuseable ? "1" : "0")}, {(shop.HideStats ? "1" : "0")}, " +
        $"{(shop.DisableBuyback ? "1" : "0")}, {(shop.OpenWallet ? "1" : "0")}, {(shop.DisplayNew ? "1" : "0")}, {(shop.RandomizeOrder ? "1" : "0")}, {(shop.CanRestock ? "1" : "0")}, {shop.NextRestock}, {shop.RestockMinInterval}, " +
        $"{shop.RestockInterval}, {shop.RestockCurrencyType}, {shop.ExcessRestockCurrencyType}, {shop.RestockCost}, {(shop.EnableRestockCostMultiplier ? "1" : "0")}, " +
        $"{(shop.DisableInstantRestock ? "1" : "0")}, {(shop.PersistantInventory ? "1" : "0")}, {shop.PullCount})," +
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
            @"INSERT INTO `shop_items` (`auto_preview_equip`, `category`, `label`, `guild_trophy`, `item_id`, `rarity`, `price`,
                            `quantity`, `required_achievement_grade`, `required_achievement_id`,
                            `required_championship_grade`, `required_championship_join_count`, `required_fame_grade`,
                            `required_guild_merchant_level`, `required_guild_merchant_type`, `required_item_id`,
                            `required_quest_alliance`, `sale_price`, `shop_id`, `stock_count`, `currency_id`, `currency_type`)
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
        lines.Add($"({(shopItem.AutoPreviewEquip ? "1" : "0")}, '{shopItem.Category}', {(int) shopItem.Label}, {shopItem.GuildTrophy}, {shopItem.ItemId}, " +
                  $"{shopItem.Rarity}, {shopItem.Price}, {shopItem.Quantity}, {shopItem.RequiredAchievementGrade}, {shopItem.RequiredAchievementId}, " +
                  $"{shopItem.RequiredChampionshipGrade}, {shopItem.RequiredChampionshipJoinCount}, {shopItem.RequiredFameGrade}, " +
                  $"{shopItem.RequiredGuildMerchantLevel}, {shopItem.RequiredGuildMerchantType}, {shopItem.RequiredItemId}, {shopItem.RequiredQuestAlliance}, " +
                  $"{shopItem.SalePrice}, {shopId}, {shopItem.StockCount}, '{shopItem.CurrencyId}', {(int) shopItem.CurrencyType}),");
    }

    lines.Add("");
    lines.Add("");

    foreach (string line in lines)
    {
        await file.WriteLineAsync(line);
    }
}

ShopItem ReadItemShop(MaplePacket maplePacket)
{
    maplePacket.ReadInt();
    int itemId = maplePacket.ReadInt();
    byte tokenType = maplePacket.ReadByte();
    int requiredItemId = maplePacket.ReadInt();
    maplePacket.ReadInt();
    int price = maplePacket.ReadInt();
    int salePrice = maplePacket.ReadInt();
    byte itemRank = maplePacket.ReadByte();
    maplePacket.ReadInt();
    int stockCount = maplePacket.ReadInt();
    int stockPurchased = maplePacket.ReadInt();
    int guildTrophy = maplePacket.ReadInt();
    string category = maplePacket.ReadString();
    int requiredAchievementId = maplePacket.ReadInt();
    int requiredAchievementGrade = maplePacket.ReadInt();
    byte requiredChampionshipGrade = maplePacket.ReadByte();
    short requiredChampionshipJoinCount = maplePacket.ReadShort();
    byte requiredGuildMerchantType = maplePacket.ReadByte();
    short requiredGuildMerchantLevel = maplePacket.ReadShort();
    maplePacket.ReadBool();
    short quantity = maplePacket.ReadShort();
    maplePacket.ReadByte();
    byte flag = maplePacket.ReadByte();
    string currencyId = maplePacket.ReadString();
    short requiredQuestAlliance = maplePacket.ReadShort();
    int requiredFameGrade = maplePacket.ReadInt();
    bool autoPreviewEquip = maplePacket.ReadBool();
    maplePacket.ReadByte();
    return new()
    {
        AutoPreviewEquip = autoPreviewEquip,
        Category = category,
        Label = (ShopItemLabel) flag,
        GuildTrophy = guildTrophy,
        ItemId = itemId,
        Rarity = itemRank,
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
        CurrencyId = currencyId,
        CurrencyType = (ShopCurrencyType) tokenType,
        Quantity = quantity
    };
}