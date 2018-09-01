using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdditionalBoneSliders
{
    public class BoneInfo
    {
        public readonly int Index;
        public readonly string Name;
        public bool Enabled;
        public float X;
        public float Y;
        public float Z;
        public float Scale;

        public BoneInfo(int index, string name, bool enabled, float x, float y, float z, float scale)
        {
            Index = index;
            Name = name;
            Enabled = enabled;
            X = x;
            Y = y;
            Z = z;
            Scale = scale;
        }

        public BoneInfo(BoneInfo boneInfo) :
            this(boneInfo.Index, boneInfo.Name, boneInfo.Enabled, boneInfo.X, boneInfo.Y, boneInfo.Z, boneInfo.Scale)
        {
        }

        public static BoneInfo FromString(string boneInfo)
        {
            var info = boneInfo.Split(',');

            if (info.Length != 7)
                return null;

            if (!int.TryParse(info[0], out int index))
                return null;

            if (!bool.TryParse(info[2], out bool enabled))
                return null;

            if (!float.TryParse(info[3], out float x))
                return null;

            if (!float.TryParse(info[4], out float y))
                return null;

            if (!float.TryParse(info[5], out float z))
                return null;

            if (!float.TryParse(info[6], out float scale))
                return null;

            return new BoneInfo(index, info[1], enabled, x, y, z, scale);
        }

        public override string ToString()
        {
            return $"{Index},{Name},{Enabled},{X},{Y},{Z},{Scale}";
        }
    }
}
