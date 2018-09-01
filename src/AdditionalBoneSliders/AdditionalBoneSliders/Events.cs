using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdditionalBoneSliders
{
    public class BoneValueChangedEvent : EventArgs
    {
        public Bone Bone { get; set; }

        public string Value { get; set; }

        public BoneValueChangedEvent(Bone bone)
        {
            Bone = bone;
        }
    }
}
