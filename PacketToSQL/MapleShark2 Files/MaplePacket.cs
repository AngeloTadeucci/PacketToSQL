// From Ochi's MapleShark2 https://github.com/kOchirasu/MapleShark2

using FindMapleShark2Sniffs.MapleShark2_Files;
using Maple2.PacketLib.Tools;

namespace PacketToSQL.MapleShark2_Files;

public class MaplePacket
{
    public DateTime Timestamp { get; }
    public bool Outbound { get; }
    public uint Version { get; }
    public byte Locale { get; }
    public ushort Opcode { get; }

    private readonly ArraySegment<byte> Buffer;
    private readonly ByteReader Reader;

    public int Position => Reader.Position - Buffer.Offset;
    public int Offset => Buffer.Offset;
    public int Length => Buffer.Count;
    public int Available => Reader.Available;

    internal MaplePacket(DateTime timestamp, bool outbound, uint version, ushort opcode, ArraySegment<byte> buffer)
    {
        Timestamp = timestamp;
        Outbound = outbound;
        Version = version;
        Locale = MapleLocale.Unknown;
        Opcode = opcode;
        Buffer = buffer;
        Reader = new(Buffer.Array, Buffer.Offset);
    }

    public void Reset() => Reader.Skip(-Reader.Position + Buffer.Offset);

    public long Search(byte[] pattern, long start = 0)
    {
        if (pattern == null || Buffer.Array == null || start < 0)
        {
            return -1;
        }

        long startIndex = Buffer.Offset + start;
        for (long i = startIndex; i <= Buffer.Array.Length - pattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; match && j < pattern.Length; j++)
            {
                match = Buffer.Array[i + j] == pattern[j];
            }

            if (match)
            {
                return i - Buffer.Offset;
            }
        }

        return -1;
    }

    public ArraySegment<byte> GetReadSegment(int length)
    {
        return new(Reader.Buffer, Reader.Position, length);
    }

    public ArraySegment<byte> GetSegment(int offset, int length)
    {
        return new(Reader.Buffer, offset, length);
    }

    public T Read<T>() where T : struct => Reader.Read<T>();

    public byte ReadByte() => Reader.ReadByte();
    public int ReadInt() => Reader.ReadInt();
    public short ReadShort() => Reader.ReadShort();
    public long ReadLong() => Reader.ReadLong();
    public bool ReadBool() => Reader.ReadBool();
    public string ReadString() => Reader.ReadString();
    public float ReadFloat() => Reader.ReadFloat();
    public string ReadUnicodeString() => Reader.ReadUnicodeString();

    public byte[] Read(int count) => Reader.ReadBytes(count);
    public void Skip(int count) => Reader.Skip(count);

    private unsafe string ToHexString()
    {
        fixed (byte* bytesPtr = Buffer.AsSpan())
        {
            return HexEncoding.ToHexString(bytesPtr, Buffer.Count, ' ');
        }
    }

    public override string ToString()
    {
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}][{(Outbound ? "OUT" : "IN ")}][{Opcode:X4}] {ToHexString()}";
    }
}