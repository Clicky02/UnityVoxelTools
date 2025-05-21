using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// Based on implementation by Giawa https://www.giawa.com/magicavoxel-c-importer/
public class VoxReader
{
    // this is the default palette of voxel colors (the RGBA chunk is only included if the palette is differe)
    // custom voxel format by Giava (16 bits, 0RRR RRGG GGGB BBBB)
    // (ushort)(((r & 0x1f) << 10) | ((g & 0x1f) << 5) | (b & 0x1f));
    private static uint[] DEFAULT_PALETTE = new uint[256]
            {
                0x00000000, 0xffffffff, 0xffccffff, 0xff99ffff, 0xff66ffff, 0xff33ffff, 0xff00ffff, 0xffffccff, 0xffccccff, 0xff99ccff, 0xff66ccff, 0xff33ccff, 0xff00ccff, 0xffff99ff, 0xffcc99ff, 0xff9999ff,
                0xff6699ff, 0xff3399ff, 0xff0099ff, 0xffff66ff, 0xffcc66ff, 0xff9966ff, 0xff6666ff, 0xff3366ff, 0xff0066ff, 0xffff33ff, 0xffcc33ff, 0xff9933ff, 0xff6633ff, 0xff3333ff, 0xff0033ff, 0xffff00ff,
                0xffcc00ff, 0xff9900ff, 0xff6600ff, 0xff3300ff, 0xff0000ff, 0xffffffcc, 0xffccffcc, 0xff99ffcc, 0xff66ffcc, 0xff33ffcc, 0xff00ffcc, 0xffffcccc, 0xffcccccc, 0xff99cccc, 0xff66cccc, 0xff33cccc,
                0xff00cccc, 0xffff99cc, 0xffcc99cc, 0xff9999cc, 0xff6699cc, 0xff3399cc, 0xff0099cc, 0xffff66cc, 0xffcc66cc, 0xff9966cc, 0xff6666cc, 0xff3366cc, 0xff0066cc, 0xffff33cc, 0xffcc33cc, 0xff9933cc,
                0xff6633cc, 0xff3333cc, 0xff0033cc, 0xffff00cc, 0xffcc00cc, 0xff9900cc, 0xff6600cc, 0xff3300cc, 0xff0000cc, 0xffffff99, 0xffccff99, 0xff99ff99, 0xff66ff99, 0xff33ff99, 0xff00ff99, 0xffffcc99,
                0xffcccc99, 0xff99cc99, 0xff66cc99, 0xff33cc99, 0xff00cc99, 0xffff9999, 0xffcc9999, 0xff999999, 0xff669999, 0xff339999, 0xff009999, 0xffff6699, 0xffcc6699, 0xff996699, 0xff666699, 0xff336699,
                0xff006699, 0xffff3399, 0xffcc3399, 0xff993399, 0xff663399, 0xff333399, 0xff003399, 0xffff0099, 0xffcc0099, 0xff990099, 0xff660099, 0xff330099, 0xff000099, 0xffffff66, 0xffccff66, 0xff99ff66,
                0xff66ff66, 0xff33ff66, 0xff00ff66, 0xffffcc66, 0xffcccc66, 0xff99cc66, 0xff66cc66, 0xff33cc66, 0xff00cc66, 0xffff9966, 0xffcc9966, 0xff999966, 0xff669966, 0xff339966, 0xff009966, 0xffff6666,
                0xffcc6666, 0xff996666, 0xff666666, 0xff336666, 0xff006666, 0xffff3366, 0xffcc3366, 0xff993366, 0xff663366, 0xff333366, 0xff003366, 0xffff0066, 0xffcc0066, 0xff990066, 0xff660066, 0xff330066,
                0xff000066, 0xffffff33, 0xffccff33, 0xff99ff33, 0xff66ff33, 0xff33ff33, 0xff00ff33, 0xffffcc33, 0xffcccc33, 0xff99cc33, 0xff66cc33, 0xff33cc33, 0xff00cc33, 0xffff9933, 0xffcc9933, 0xff999933,
                0xff669933, 0xff339933, 0xff009933, 0xffff6633, 0xffcc6633, 0xff996633, 0xff666633, 0xff336633, 0xff006633, 0xffff3333, 0xffcc3333, 0xff993333, 0xff663333, 0xff333333, 0xff003333, 0xffff0033,
                0xffcc0033, 0xff990033, 0xff660033, 0xff330033, 0xff000033, 0xffffff00, 0xffccff00, 0xff99ff00, 0xff66ff00, 0xff33ff00, 0xff00ff00, 0xffffcc00, 0xffcccc00, 0xff99cc00, 0xff66cc00, 0xff33cc00,
                0xff00cc00, 0xffff9900, 0xffcc9900, 0xff999900, 0xff669900, 0xff339900, 0xff009900, 0xffff6600, 0xffcc6600, 0xff996600, 0xff666600, 0xff336600, 0xff006600, 0xffff3300, 0xffcc3300, 0xff993300,
                0xff663300, 0xff333300, 0xff003300, 0xffff0000, 0xffcc0000, 0xff990000, 0xff660000, 0xff330000, 0xff0000ee, 0xff0000dd, 0xff0000bb, 0xff0000aa, 0xff000088, 0xff000077, 0xff000055, 0xff000044,
                0xff000022, 0xff000011, 0xff00ee00, 0xff00dd00, 0xff00bb00, 0xff00aa00, 0xff008800, 0xff007700, 0xff005500, 0xff004400, 0xff002200, 0xff001100, 0xffee0000, 0xffdd0000, 0xffbb0000, 0xffaa0000,
                0xff880000, 0xff770000, 0xff550000, 0xff440000, 0xff220000, 0xff110000, 0xffeeeeee, 0xffdddddd, 0xffbbbbbb, 0xffaaaaaa, 0xff888888, 0xff777777, 0xff555555, 0xff444444, 0xff222222, 0xff111111
            };

    public static VoxModel Load(string path)
    {
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read) ?? throw new System.Exception("Failed to open file for FileStream.");
        using var stream = new BinaryReader(fileStream);

        VoxModel voxModel = VoxModel.NewInstance();


        string magic = new(stream.ReadChars(4));
        if (magic != "VOX ")
        {
            throw new System.Exception("Failed to open file for FileStream. Magic header not found.");
        }

        int version = stream.ReadInt32();
        if (version != 200) // only version 200 is supported
        {
            throw new System.Exception("Failed to open file for FileStream.");
        }

        // a MagicaVoxel .vox file starts with a 'magic' 4 character 'VOX ' identifier

        Dictionary<int, VoxNode> nodeMap = new();
        Dictionary<int, VoxShape> shapeMap = new();
        VoxShape curShape = null;
        uint[] colors = null;
        int currentId = 0;

        while (stream.BaseStream.Position < stream.BaseStream.Length)
        {
            // each chunk has an ID, size and child chunks
            string chunkName = new string(stream.ReadChars(4));
            int chunkSize = stream.ReadInt32();
            int childSize = stream.ReadInt32();

            // there are only 2 chunks we only care about, and they are SIZE and XYZI
            if (chunkName == "MAIN")
            {
                stream.ReadBytes(chunkSize);
            }
            else if (chunkName == "SIZE")
            {

                sbyte x = (sbyte)stream.ReadInt32();
                sbyte y = (sbyte)stream.ReadInt32();
                sbyte z = (sbyte)stream.ReadInt32();

                curShape = new VoxShape(new VoxCoord(x, y, z));
                shapeMap[currentId] = curShape;

                stream.ReadBytes(chunkSize - 4 * 3); // ???
            }
            else if (chunkName == "XYZI")
            {
                // XYZI contains n voxels
                int numVoxels = stream.ReadInt32();

                curShape.NumVoxels = numVoxels;
                for (int i = 0; i < numVoxels; i++)
                    curShape.SetVoxel(new VoxCoord(stream.ReadSByte(), stream.ReadSByte(), stream.ReadSByte()), stream.ReadByte());

                shapeMap[currentId] = curShape;
            }
            else if (chunkName == "RGBA")
            {
                var bytePalette = new byte[chunkSize];
                stream.Read(bytePalette, 0, chunkSize);

                colors = new uint[chunkSize / 4];

                for (int i = 4; i < bytePalette.Length; i += 4)
                    colors[i / 4] = System.BitConverter.ToUInt32(bytePalette, i - 4);
            }
            else if (chunkName == "nTRN")
            {
                int nodeId = stream.ReadInt32();
                var attributes = ReadVoxDictionary(stream);
                int childNodeId = stream.ReadInt32();
                int reservedId = stream.ReadInt32();
                int layerId = stream.ReadInt32();
                int numFrames = stream.ReadInt32();

                VoxNode curNode;
                // I collapse the transform and the node below it
                if (!nodeMap.TryGetValue(nodeId, out curNode) && !nodeMap.TryGetValue(childNodeId, out curNode))
                {
                    curNode = new VoxNode();
                    nodeMap[nodeId] = curNode;
                    nodeMap[childNodeId] = curNode;
                }

                for (int i = 0; i < numFrames; i++)
                {
                    var frameAttributes = ReadVoxDictionary(stream);

                    if (!shapeMap.TryGetValue(nodeId, out curShape)) { continue; }

                    if (frameAttributes.TryGetValue("_t", out string value))
                    {
                        curNode.SetTranslation(ParseVector3(value));
                    }

                    // TODO: Add support for rotation (_r)
                }
            }
            else if (chunkName == "nGRP")
            {
                int nodeId = stream.ReadInt32();
                var attributes = ReadVoxDictionary(stream);
                int numNodes = stream.ReadInt32();

                VoxNode curNode;
                if (!nodeMap.TryGetValue(nodeId, out curNode))
                {
                    curNode = new VoxNode();
                    nodeMap[nodeId] = curNode;
                }

                List<VoxNode> children = new();
                for (int i = 0; i < numNodes; i++)
                {
                    var childNodeId = stream.ReadInt32();

                    if (!nodeMap.TryGetValue(childNodeId, out var childNode))
                    {
                        childNode = new VoxNode();
                        nodeMap[childNodeId] = childNode;
                    }

                    children.Add(childNode);
                }

                curNode.Children = children;
            }
            else if (chunkName == "nSHP")
            {
                int nodeId = stream.ReadInt32();
                var attributes = ReadVoxDictionary(stream);
                int numShapes = stream.ReadInt32();

                if (!nodeMap.TryGetValue(nodeId, out VoxNode curNode))
                {
                    curNode = new VoxNode();
                    nodeMap[nodeId] = curNode;
                }

                for (int i = 0; i < numShapes; i++)
                {
                    // TODO: Add support for multiple shapes?
                    var shapeId = stream.ReadInt32();
                    var shapeAttrs = ReadVoxDictionary(stream);
                    curNode.Shape = shapeMap[shapeId];
                }
            }
            else
            {
                // read any excess 
                stream.ReadBytes(chunkSize);
                stream.ReadBytes(childSize);
            }

            currentId += 1;
        }

        voxModel.Colortable = CreateColor32FromPalette(colors ?? DEFAULT_PALETTE);

        return voxModel;
    }


    public static Color32[] CreateColor32FromPalette(uint[] palette)
    {
        Debug.Assert(palette.Length == 256);

        Color32[] colors = new Color32[256];

        for (uint j = 0; j < 256; j++)
        {
            uint rgba = palette[j];

            Color32 color = new Color32();
            color.r = (byte)((rgba >> 0) & 0xFF);
            color.g = (byte)((rgba >> 8) & 0xFF);
            color.b = (byte)((rgba >> 16) & 0xFF);
            color.a = (byte)((rgba >> 24) & 0xFF);

            colors[j] = color;
        }

        return colors;
    }

    private static Dictionary<string, string> ReadVoxDictionary(BinaryReader stream)
    {
        Dictionary<string, string> dict = new();
        int entryCount = stream.ReadInt32();

        for (int i = 0; i < entryCount; i++)
        {
            int keySize = stream.ReadInt32();
            string key = new(stream.ReadChars(keySize));

            int valueSize = stream.ReadInt32();
            string value = new(stream.ReadChars(valueSize));

            dict[key] = value;
        }

        return dict;
    }

    private static Vector3 ParseVector3(string value)
    {
        string[] parts = value.Split(' ');
        if (parts.Length != 3)
        {
            throw new System.Exception("Invalid vector format: " + value);
        }

        return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
    }
}
