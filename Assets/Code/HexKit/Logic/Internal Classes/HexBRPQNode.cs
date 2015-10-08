namespace Settworks.Hexagons
{
    internal class HexBRPQNode : HexPathNode
    {
        private uint cost, costHistory, turnHistory;
        private bool turning;
        private readonly uint heuristic;
        private readonly HexCoord hex;
        private int from;

        public HexBRPQNode(HexCoord hex, int from, uint cost, HexBRPQNode ancestor = null, uint heuristic = 0)
        {
            this.hex = hex;
            this.from = from;
            this.cost = cost;
            this.heuristic = heuristic;
            if (ancestor != null)
                AssignAncestor(ancestor, from);
        }

        private void AssignAncestor(HexBRPQNode ancestor, int from)
        {
            costHistory = ancestor.costHistory + ancestor.cost;
            turnHistory = ancestor.turnHistory + (uint)(ancestor.turning ? 2 : 0);
            turning = from != ancestor.from;
        }

        public void ReconsiderAncestor(HexBRPQNode ancestor, int from, uint cost, bool requeue = false)
        {
            if (ancestor == null) return;
            ulong P = (((ulong)heuristic + cost + ancestor.costHistory + ancestor.cost) << 32)
                + ancestor.turnHistory + (ulong)(ancestor.turning ? 2 : 0)
                + (ulong)(from == ancestor.from ? 0 : 1);
            if (P < Priority)
            {
                this.from = from;
                this.cost = cost;
                AssignAncestor(ancestor, from);
                if (Queue != null)
                {
                    if (Queue.Contains(this))
                        Queue.OnNodeUpdated(this);
                    else if (requeue)
                        Queue.Enqueue(this);
                }
            }
        }

        // HexPathNode stuff
        public override HexCoord Location { get { return hex; } }

        public override int FromDirection { get { return from; } }

        public override int PathCost
        {
            get { return (int)(cost + costHistory); }
        }

        // BRPQ stuff
        public HexBRPQ Queue;

        public uint QueueIndex;
        public uint ParentIndex { get { return QueueIndex == 0 ? 0 : (QueueIndex - 1) >> 1; } }
        public ulong InsertionIndex;

        public ulong Priority
        {
            get
            {
                return (((ulong)heuristic + cost + costHistory) << 32) + turnHistory + (ulong)(turning ? 1 : 0);
            }
        }
    }
}