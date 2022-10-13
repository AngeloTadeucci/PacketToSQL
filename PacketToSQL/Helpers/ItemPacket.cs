using System.Runtime.InteropServices;
using System.Xml.Serialization;
using PacketToSQL.MapleShark2_Files;

namespace PacketToSQL.Helpers;

public static class ItemPacket
{
    private static readonly int[] TemplateIds =
    {
        11050005,
        11050006,
        11300041,
        11300042,
        11300043,
        11300044,
        11300045,
        11300046,
        11300155,
        11300156,
        11300157,
        11300158,
        11300697,
        11300698,
        11300699,
        11300700,
        11400024,
        11400025,
        11400026,
        11400027,
        11400080,
        11400081,
        11400082,
        11400083,
        11400120,
        11400121,
        11400418,
        11400419,
        11400550,
        11400551,
        11400607,
        11400608,
        11401077,
        11401078,
        11500024,
        11500025,
        11500026,
        11500027,
        11500081,
        11500082,
        11500338,
        11500339,
        11500522,
        11500523,
        11500971,
        11500972,
        11600034,
        11600035,
        11700048,
        11700049,
        11700173,
        11700174,
        11850005,
        11850008,
        11850180,
        12200016,
        12200017,
        12200092,
        12200093,
        12200327,
        12200328,
        12200331,
        12200332,
        13000070,
        13100203,
        13100319,
        13200198,
        13200314,
        13300197,
        13300310,
        13400196,
        13400309,
        14000162,
        14000272,
        14100171,
        14100284,
        15000202,
        15000315,
        15100194,
        15100310,
        15200201,
        15200314,
        15300197,
        15300310,
        15400112,
        15400296,
        15500509,
        15600509,
        15600532,
        50100189,
        50100385,
        50200381,
        50200382,
        50200398,
        50200399,
        50200440,
        50200486,
        50200487,
        50200488,
        50200489,
        50200490,
        50200649,
        50200700,
        50200701,
        50200702,
        50200703,
        50200738,
        50200739,
        50200740,
        50200741,
        50200742,
        50200743,
        50200744,
        50200745,
        50200746,
        50200747,
        50200748,
        50200749,
        50200750,
        50200751,
        50200752,
        50200753,
        50200754,
        50200755,
        50200756,
        50200787,
        50200878,
        50200879,
        50200880,
        50200881,
        50200882,
        50200883,
        50200884,
        50200885,
        50400088,
        50400089,
        50400228,
        50600045,
        50600054,
        50600060,
        50600066,
        50600072,
        50600078,
        50600084,
        50600089,
        50600090,
        50600278
    };
    private const int BlueprintId = 35200000;

    public static void ReadKmsItem(this MaplePacket reader, int id)
    {
        int amount = reader.ReadInt(); // Amount
        reader.ReadInt(); // Unknown
        reader.ReadInt(); // Unknown
        reader.ReadLong(); // CreationTime
        reader.ReadLong(); // ExpiryTime
        reader.ReadLong(); // Unknown
        reader.ReadInt(); // TimesChangedAttribute
        reader.ReadInt(); // RemainingUses
        reader.ReadByte(); // IsLocked
        reader.ReadLong(); // UnlockTime
        reader.ReadShort(); // GlamorForges

        reader.ReadItemExtraData(id);
        reader.ReadItemStats();
        reader.ReadItemEnchant();
        reader.ReadItemLimitBreak();

        if (TemplateIds.Contains(id) || id == BlueprintId)
        {
            reader.ReadUgcItemLook();
            reader.ReadBlueprintItemData();
        }

        if (id / 100000 == 600 || id / 100000 == 610 || id / 100000 == 611 || id / 100000 == 629)
        {
            reader.ReadUnicodeString(); // PetName
            reader.ReadLong(); // PetExp
            reader.ReadInt(); // Unknown
            reader.ReadInt(); // PetLevel
            reader.ReadByte(); // Unknown
        }

        if (id / 100000 == 351)
        {
            reader.ReadInt(); // ScoreLength
            reader.ReadInt(); // Instrument
            reader.ReadUnicodeString(); // ScoreTitle
            reader.ReadUnicodeString(); // Author
            reader.ReadInt(); // Unknown (1)
            reader.ReadLong(); // AuthorCharacterId
            reader.ReadByte(); // IsLocked
            reader.ReadLong(); // Unknown
            reader.ReadLong(); // Unknown
        }

        if (id / 1000000 == 70)
        {
            reader.ReadByte(); // Unknown
            reader.ReadByte(); // BadgeType
            var badgeId = reader.ReadInt(); // BadgeId

            switch (id)
            {
                case 70100000:
                    reader.ReadInt(); // PetSkinId
                    break;
                // transparency
                case 70100001:
                    reader.ReadByte(); // Headgear
                    reader.ReadByte(); // Eyewear
                    reader.ReadByte(); // Top
                    reader.ReadByte(); // Bottom
                    reader.ReadByte(); // Cape
                    reader.ReadByte(); // Earrings
                    reader.ReadByte(); // Face
                    reader.ReadByte(); // Gloves
                    reader.ReadByte(); // Unknown
                    reader.ReadByte(); // Shoes
                    break;
            }
        }

        var flag = reader.ReadInt(); // TransferFlag
        reader.ReadBool(); // Unknown
        reader.ReadInt(); // Remaining Trades
        reader.ReadInt(); // Remaining Repackage Count
        reader.ReadByte(); // Unknown
        var one = reader.ReadBool(); // Unknown
        bool isBound = reader.ReadBool(); // IsBound
        if (isBound)
        {
            reader.ReadLong(); // BoundToCharId
            reader.ReadUnicodeString(); // BoundToName
        }

        reader.ReadByte(); // MaxSockets
        byte totalSockets = reader.ReadByte(); // TotalSockets
        for (int i = 0; i < totalSockets; i++)
        {
            bool isUnlocked = reader.ReadBool(); // unlocked socket
            if (!isUnlocked) continue;
            reader.ReadInt(); // GemstoneItemId
            isBound = reader.ReadBool(); // Bound
            if (isBound)
            {
                reader.ReadLong(); // BoundToCharId
                reader.ReadUnicodeString(); // BoundToName
            }

            bool isLocked = reader.ReadBool(); // IsLocked
            if (!isLocked) continue;
            reader.ReadBool(); // IsLocked
            reader.ReadLong(); // UnlockTime
        }

        long pairedCharacterId = reader.ReadLong(); // PairedCharacterId
        if (pairedCharacterId != 0)
        {
            reader.ReadUnicodeString(); // PairedName
            reader.ReadBool(); // Unknown
        }

        reader.ReadLong(); // BoundToCharId
        var name = reader.ReadUnicodeString(); // BoundToName
    }

    private static void ReadBlueprintItemData(this MaplePacket reader)
    {
        reader.ReadLong(); // Unknown
        reader.ReadInt(); // Unknown
        reader.ReadInt(); // Unknown
        reader.ReadInt(); // Unknown
        reader.ReadLong(); // Unknown
        reader.ReadInt(); // Unknown
        reader.ReadLong(); // Unknown
        reader.ReadLong(); // Unknown
        reader.ReadUnicodeString(); // Unknown
    }

    private static void ReadUgcItemLook(this MaplePacket reader)
    {
        reader.ReadLong(); // Uid
        reader.ReadUnicodeString(); // UUID
        reader.ReadUnicodeString(); // ItemName
        reader.ReadByte(); // Unknown
        reader.ReadInt(); // Unknown
        reader.ReadLong(); // AccountId
        reader.ReadLong(); // CharacterId
        reader.ReadUnicodeString(); // CharacterName
        reader.ReadLong(); // CreationTime
        reader.ReadUnicodeString(); // UGC Url
        reader.ReadByte(); // Unknown
    }

    private static void ReadItemLimitBreak(this MaplePacket reader)
    {
        reader.ReadInt(); // LimitBreakLevel
        int count = reader.ReadInt(); // LimitBreakStatOptionCount
        for (int i = 0; i < count; i++)
        {
            reader.ReadShort(); // StatType
            reader.ReadFloat();
        }

        count = reader.ReadInt(); // LimitBreakSpecialOptionCount
        for (int i = 0; i < count; i++)
        {
            reader.ReadShort(); // StatType
            reader.ReadFloat();
        }
    }

    private static void ReadItemEnchant(this MaplePacket reader)
    {
        reader.ReadInt(); // Enchants
        reader.ReadInt(); // EnchantExp
        reader.ReadByte(); // EnchantBasedChargeExp
        reader.ReadLong(); // Unknown
        reader.ReadInt(); // Unknown
        reader.ReadInt(); // Unknown
        var canRepackage = reader.ReadBool(); // CanRepackage
        reader.ReadInt(); // EnchantCharges

        byte count = reader.ReadByte(); // EnchantStatCount
        for (int i = 0; i < count; i++)
        {
            reader.ReadInt(); // StatType
            reader.ReadFloat(); // Value
        }
    }

    private static void ReadItemStats(this MaplePacket reader)
    {
        reader.ReadBool(); // Unknown

        for (int i = 0; i < 3; i++) // {"Constant", "Static", "Random"}
        {
            short count = reader.ReadShort(); // StatOptionCount
            for (int j = 0; j < count; j++)
            {
                reader.ReadShort(); // StatType
                reader.ReadFloat(); // Value
            }

            count = reader.ReadShort(); // SpecialOptionCount
            for (int j = 0; j < count; j++)
            {
                reader.ReadShort(); // StatType
                reader.ReadFloat(); // Value
            }

            reader.ReadInt(); // Unknown
        }
    }

    private static void ReadItemExtraData(this MaplePacket reader, int id)
    {
        reader.ReadEquipColor();
        if (id / 100000 == 113)
        {
            reader.Read<CoordF>(); // Position1
            reader.Read<CoordF>(); // Position2
            reader.Read<CoordF>(); // Position3
            reader.Read<CoordF>(); // Position4
            reader.ReadFloat(); // Unknown
        }
        else if (id / 100000 == 102)
        {
            reader.ReadFloat(); // BackLength
            reader.Read<CoordF>(); // BackPosition1
            reader.Read<CoordF>(); // BackPosition2
            reader.ReadFloat(); // FrontLength
            reader.Read<CoordF>(); // FrontPosition1
            reader.Read<CoordF>(); // FrontPosition2
        }
        else if (id / 100000 == 104)
        {
            reader.ReadFloat(); // Position1
            reader.ReadFloat(); // Position2
            reader.ReadFloat(); // Position3
            reader.ReadFloat(); // Position4
        }
    }

    private static void ReadEquipColor(this MaplePacket reader)
    {
        reader.ReadInt(); // Color1
        reader.ReadInt(); // Color2
        reader.ReadInt(); // Color3
        reader.ReadInt(); // ColorIndex
        reader.ReadInt(); // Unknown
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2, Size = 6)]
    public struct CoordS
    {
        [XmlElement(Order = 1)]
        public short X { get; set; }
        [XmlElement(Order = 2)]
        public short Y { get; set; }
        [XmlElement(Order = 3)]
        public short Z { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Size = 12)]
    public struct CoordF
    {
        [XmlElement(Order = 1)]
        public float X { get; set; }
        [XmlElement(Order = 2)]
        public float Y { get; set; }
        [XmlElement(Order = 3)]
        public float Z { get; set; }
    }
}