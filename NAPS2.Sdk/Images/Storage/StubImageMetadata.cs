﻿using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images.Transforms;
using NAPS2.Scan;

namespace NAPS2.Images.Storage
{
    public class StubImageMetadata : IImageMetadata
    {
        public List<Transform> TransformList { get; set; } = new List<Transform>();

        public int Index { get; set; }

        public ScanBitDepth BitDepth { get; set; }

        public bool Lossless { get; set; }

        public void Commit()
        {
        }

        public bool CanSerialize => false;

        public byte[] Serialize(IStorage storage) => throw new NotSupportedException();

        public IStorage Deserialize(byte[] serializedData) => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }
}
