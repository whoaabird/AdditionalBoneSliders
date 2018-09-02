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

        public float X
        {
            get { return Values.X; }
            set
            {
                Values.X = value;
                Values.Enabled = Values.X != DefaultValues.X || DefaultValues.Enabled;
            }
        }

        public float Y
        {
            get { return Values.Y; }
            set
            {
                Values.Y = value;
                Values.Enabled = Values.Y != DefaultValues.Y || DefaultValues.Enabled;
            }
        }

        public float Z
        {
            get { return Values.Z; }
            set
            {
                Values.Z = value;
                Values.Enabled = Values.Z != DefaultValues.Z || DefaultValues.Enabled;
            }
        }

        public float Scale
        {
            get { return Values.Scale; }
            set
            {
                Values.Scale = value;
                Values.Enabled = Values.Scale != DefaultValues.Scale || DefaultValues.Enabled;
            }
        }

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
