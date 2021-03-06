﻿namespace NBWTA.RegionDetection
{
    using System.Collections.Generic;

    internal class Region<TNode>
    {
        public HashSet<TNode> Nodes { get; }

        public Region(HashSet<TNode> nodes)
        {
            Nodes = nodes;
        }
    }
}