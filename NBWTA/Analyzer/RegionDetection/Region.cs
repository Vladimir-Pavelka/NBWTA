namespace Analyzer.RegionDetection
{
    using System.Collections.Generic;

    public class Region<TNode>
    {
        public HashSet<TNode> Nodes { get; }

        public Region(HashSet<TNode> nodes)
        {
            Nodes = nodes;
        }
    }
}