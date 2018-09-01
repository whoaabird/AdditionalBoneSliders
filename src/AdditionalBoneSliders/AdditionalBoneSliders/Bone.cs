using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdditionalBoneSliders
{
    public class Bone
    {
        public BoneInfo DefaultValues { get; }

        public BoneInfo Values { get; private set; }

        public float Scale { get { return Values.Scale; } set { Values.Scale = value; } }

        public Bone(BoneInfo boneInfo)
        {
            DefaultValues = new BoneInfo(boneInfo);
            Values = new BoneInfo(boneInfo);
        }

        public void Reset()
        {
            Values = new BoneInfo(DefaultValues);
        }

        public override string ToString()
        {
            return $"{Values}";
        }
    }
}
