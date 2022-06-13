// From Ochi's MapleShark2 https://github.com/kOchirasu/MapleShark2

using FindMapleShark2Sniffs.MapleShark2_Files;

namespace PacketToSQL.MapleShark2_Files;

public class MsbMetadata
{
    public string LocalEndpoint;
    public ushort LocalPort;
    public string RemoteEndpoint;
    public ushort RemotePort;
    public byte Locale;
    public uint Build;
}

public static class FileLoader
{
    public static (MsbMetadata, IEnumerable<MaplePacket>) ReadMsbFile(string fileName)
    {
        MsbMetadata metadata = new();
        List<MaplePacket> packets = new();

        using FileStream stream = new(fileName, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream);
        ushort version = reader.ReadUInt16();
        if (version < 0x2000)
        {
            metadata.Build = version;
            metadata.LocalPort = reader.ReadUInt16();
            metadata.Locale = MapleLocale.Unknown;
        }
        else
        {
            byte v1 = (byte) ((version >> 12) & 0xF),
                v2 = (byte) ((version >> 8) & 0xF),
                v3 = (byte) ((version >> 4) & 0xF),
                v4 = (byte) ((version >> 0) & 0xF);

            switch (version)
            {
                case 0x2012:
                    metadata.Locale = (byte) reader.ReadUInt16();
                    metadata.Build = reader.ReadUInt16();
                    metadata.LocalPort = reader.ReadUInt16();
                    break;
                case 0x2014:
                    metadata.LocalEndpoint = reader.ReadString();
                    metadata.LocalPort = reader.ReadUInt16();
                    metadata.RemoteEndpoint = reader.ReadString();
                    metadata.RemotePort = reader.ReadUInt16();

                    metadata.Locale = (byte) reader.ReadUInt16();
                    metadata.Build = reader.ReadUInt16();
                    break;
                case 0x2015:
                case >= 0x2020:
                    metadata.LocalEndpoint = reader.ReadString();
                    metadata.LocalPort = reader.ReadUInt16();
                    metadata.RemoteEndpoint = reader.ReadString();
                    metadata.RemotePort = reader.ReadUInt16();

                    metadata.Locale = reader.ReadByte();
                    metadata.Build = reader.ReadUInt32();
                    break;
                default:
                    string message = $"Invalid msb file, version: {v1}.{v2}.{v3}.{v4}";
                    Console.WriteLine(message);
                    return (metadata, new List<MaplePacket>());
            }
        }


        while (stream.Position < stream.Length)
        {
            long timestamp = reader.ReadInt64();
            int size = version < 0x2027 ? reader.ReadUInt16() : reader.ReadInt32();
            ushort opcode = reader.ReadUInt16();
            bool outbound;

            if (version >= 0x2020)
            {
                outbound = reader.ReadBoolean();
            }
            else
            {
                outbound = (size & 0x8000) != 0;
                size = (ushort) (size & 0x7FFF);
            }

            byte[] buffer = reader.ReadBytes(size);
            if (version is >= 0x2025 and < 0x2030)
            {
                reader.ReadUInt32(); // preDecodeIV
                reader.ReadUInt32(); // postDecodeIV
            }

            MaplePacket packet = new(new(timestamp), outbound, metadata.Build, opcode, new(buffer));
            packets.Add(packet);
        }

        return (metadata, packets);
    }
}
