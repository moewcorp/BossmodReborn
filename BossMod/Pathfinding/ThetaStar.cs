﻿using System.Buffers;

namespace BossMod.Pathfinding;

public sealed class ThetaStar
{
    public enum Score
    {
        JustBad, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), destination is unsafe and has same or lower max-g than starting cell
        UltimatelyBetter, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), destination is unsafe but has larger max-g than starting cell
        UltimatelySafe, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), however destination is safe
        UnsafeAsStart, // the path is unsafe (there are cells along the path with negative leeway, but no max-g lower than starting cell), destination is unsafe with same max-g as starting cell (starting cell will have this score if its max-g is <= 0)
        SemiSafeAsStart, // the path is semi-safe (no cell along the path has negative leeway or max-g lower than starting cell), destination is unsafe with same max-g as starting cell (starting cell will have this score if its max-g is > 0)
        UnsafeImprove, // the path is unsafe (there are cells along the path with negative leeway, but no max-g lower than starting cell), destination is at least better than start
        SemiSafeImprove, // the path is semi-safe (no cell along the path has negative leeway or max-g lower than starting cell), destination is unsafe but better than start
        Safe, // the path reaches safe cell and is fully safe (no cell along the path has negative leeway) (starting cell will have this score if it's safe)
        SafeBetterPrio, // the path reaches safe cell with a higher goal priority than starting cell (but less than max) and is fully safe (no cell along the path has negative leeway)
        SafeMaxPrio // the path reaches safe cell with max goal priority and is fully safe (no cell along the path has negative leeway)
    }

    public struct Node
    {
        public float GScore;
        public float HScore;
        public float PathLeeway; // min diff along path between node's g-value and cell's g-value
        public float PathMinG; // minimum 'max g' value along path
        public int ParentIndex;
        public int OpenHeapIndex; // -1 if in closed list, 0 if not in any lists, otherwise (index+1)
        public Score Score;

        public readonly float FScore => GScore + HScore;
    }

    private Map _map = new();
    private Node[] _nodes = [];
    private readonly List<int> _openList = [];
    private float _deltaGSide;
    private float _deltaGDiag;

    private float _mapResolution;
    private float _mapHalfResolution;
    public int StartNodeIndex;
    private float _startMaxG;
    private float _startPrio;
    private Score _startScore;

    private int _bestIndex; // node with best score
    private int _fallbackIndex; // best 'fallback' node: node that we don't necessarily want to go to, but might want to move closer to it (to the parent)

    // statistics
    public int NumSteps;
    public int NumReopens;

    public ref Node NodeByIndex(int index) => ref _nodes[index];
    public WPos CellCenter(int index) => _map.GridToWorld(index % _map.Width, index / _map.Width, 0.5f, 0.5f);

    // gMultiplier is typically inverse speed, which turns g-values into time
    public void Start(Map map, WPos startPos, float gMultiplier)
    {
        _map = map;
        var numPixels = map.Width * map.Height;
        if (_nodes.Length < numPixels)
        {
            _nodes = new Node[numPixels];
        }
        else
        {
            new Span<Node>(_nodes, 0, numPixels).Clear();
        }
        _openList.Clear();
        _deltaGSide = map.Resolution * gMultiplier;
        _deltaGDiag = _deltaGSide * 1.414214f;
        _mapResolution = map.Resolution;
        _mapHalfResolution = map.Resolution * 0.5f;

        PrefillH();

        var startFrac = map.WorldToGridFrac(startPos);
        var start = map.ClampToGrid(Map.FracToGrid(startFrac));
        StartNodeIndex = _bestIndex = _fallbackIndex = _map.GridToIndex(start.x, start.y);
        _startMaxG = _map.PixelMaxG[StartNodeIndex];
        _startPrio = _map.PixelPriority[StartNodeIndex];
        //if (_startMaxG < 0)
        //    _startMaxG = float.MaxValue; // TODO: this is a hack that allows navigating outside the obstacles, reconsider...
        _startScore = CalculateScore(_startMaxG, _startMaxG, _startMaxG, StartNodeIndex);
        NumSteps = NumReopens = 0;

        startFrac.X -= start.x + 0.5f;
        startFrac.Y -= start.y + 0.5f;
        ref var startNode = ref _nodes[StartNodeIndex];
        _nodes[StartNodeIndex] = new()
        {
            GScore = 0f,
            HScore = startNode.HScore, //HeuristicDistance(start.x, start.y),
            ParentIndex = StartNodeIndex, // start's parent is self
            PathLeeway = _startMaxG,
            PathMinG = _startMaxG,
            Score = _startScore,
        };
        AddToOpen(StartNodeIndex);
    }

    // returns whether search is to be terminated; on success, first node of the open list would contain found goal
    private static readonly (int dx, int dy, float step)[] _nbrs =
    [
        ( 0,-1, 1f), (-1,-1, 1.414214f), ( 1,-1, 1.414214f),
        (-1, 0, 1f),                  ( 1, 0, 1f),
        ( 0, 1, 1f), (-1, 1, 1.414214f), ( 1, 1, 1.414214f),
    ];

    public bool ExecuteStep()
    {
        if (_openList.Count == 0)
        {
            return false;
        }

        ++NumSteps;
        var pIdx = PopMinOpen();
        var width = _map.Width;
        var widthu = (uint)width;
        var height = (uint)_map.Height;
        var px = pIdx % width;
        var py = pIdx / width;

        ref var p = ref _nodes[pIdx];

        // update best & fallback
        if (CompareNodeScores(ref p, ref _nodes[_bestIndex]) < 0)
        {
            _bestIndex = pIdx;
        }
        if (p.Score == Score.UltimatelySafe && (_fallbackIndex == StartNodeIndex || CompareNodeScores(ref p, ref _nodes[_fallbackIndex]) < 0))
        {
            _fallbackIndex = pIdx;
        }

        // neighbor loop with bounds checks and packed cost
        for (var i = 0; i < 8; ++i)
        {
            var (dx, dy, stepMul) = _nbrs[i];
            var nx = px + dx;
            var ny = py + dy;
            if ((uint)nx >= widthu || (uint)ny >= height)
            {
                continue;
            }

            var nIdx = ny * width + nx;
            VisitNeighbour(pIdx, nx, ny, nIdx, stepMul == 1f ? _deltaGSide : _deltaGDiag);
        }
        return true;
    }

    public int Execute()
    {
        while (_nodes[_bestIndex].HScore > 0f && _fallbackIndex == StartNodeIndex && ExecuteStep())
            ;
        return BestIndex();
    }

    public int BestIndex()
    {
        ref var nd = ref _nodes[_bestIndex];
        if (nd.Score > _startScore)
            return _bestIndex; // we've found something better than start

        if (_fallbackIndex != StartNodeIndex)
        {
            // find first parent of best-among-worst that is at least as good as start
            var destIndex = _fallbackIndex;
            ref var ndp = ref _nodes[destIndex];
            var parentIndex = ndp.ParentIndex;
            while (_nodes[parentIndex].Score < _startScore)
            {
                destIndex = parentIndex;
                ref var ndd = ref _nodes[destIndex];
                parentIndex = ndd.ParentIndex;
            }

            // TODO: this is very similar to LineOfSight, try to unify implementations...
            ref var startNode = ref _nodes[parentIndex];
            ref var destNode = ref _nodes[destIndex];
            var (x2, y2) = _map.IndexToGrid(destIndex);
            var (x1, y1) = _map.IndexToGrid(parentIndex);
            var dx = x2 - x1;
            var dy = y2 - y1;
            var sx = dx > 0 ? 1 : -1;
            var sy = dy > 0 ? 1 : -1;
            var hsx = 0.5f * sx;
            var hsy = 0.5f * sy;
            var indexDeltaX = sx;
            var indexDeltaY = sy * _map.Width;

            var ab = new Vector2(dx, dy);
            ab /= ab.Length();
            var invx = ab.X != 0f ? 1f / ab.X : float.MaxValue; // either can be infinite, but not both; we want to avoid actual infinities here, because 0*inf = NaN (and we'd rather have it be 0 in this case)
            var invy = ab.Y != 0f ? 1f / ab.Y : float.MaxValue;

            while (x1 != x2 || y1 != y2)
            {
                var tx = hsx * invx; // if negative, we'll never intersect it
                var ty = hsy * invy;
                if (tx < 0f || x1 == x2)
                    tx = float.MaxValue;
                if (ty < 0f || y1 == y2)
                    ty = float.MaxValue;

                var nextIndex = parentIndex;
                if (tx < ty)
                {
                    x1 += sx;
                    nextIndex += indexDeltaX;
                }
                else
                {
                    y1 += sy;
                    nextIndex += indexDeltaY;
                }

                if (_nodes[nextIndex].Score < _startScore)
                {
                    return parentIndex;
                }
                parentIndex = nextIndex;
            }
        }

        return _bestIndex;
    }

    public Score CalculateScore(float pixMaxG, float pathMinG, float pathLeeway, int pixelIndex)
    {
        var destSafe = pixMaxG == float.MaxValue;
        var pathSafe = pathLeeway > 0f;
        var destBetter = pixMaxG > _startMaxG;

        if (destSafe && pathSafe)
        {
            var prio = _map.PixelPriority[pixelIndex];
            return prio == _map.MaxPriority ? Score.SafeMaxPrio : prio > _startPrio ? Score.SafeBetterPrio : Score.Safe;
        }

        if (pathMinG == _startMaxG) // TODO: some small threshold? should be solved by preprocessing...
            return pathSafe
                ? (destBetter ? Score.SemiSafeImprove : Score.SemiSafeAsStart) // note: if pix.MaxG is < _startMaxG, then PathMinG will be < too
                : (destBetter ? Score.UnsafeImprove : Score.UnsafeAsStart);

        return destSafe ? Score.UltimatelySafe : destBetter ? Score.UltimatelyBetter : Score.JustBad;
    }

    // return a 'score' difference: 0 if identical, -1 if left is somewhat better, -2 if left is significantly better, +1/+2 when right is better
    public static int CompareNodeScores(ref Node nodeL, ref Node nodeR)
    {
        if (nodeL.Score != nodeR.Score)
            return nodeL.Score > nodeR.Score ? -2 : +2;

        const float Eps = 1e-5f;
        // TODO: should we use leeway here or distance?..
        //return nodeL.PathLeeway > nodeR.PathLeeway;
        var gl = nodeL.GScore;
        var gr = nodeR.GScore;
        var fl = gl + nodeL.HScore;
        var fr = gr + nodeR.HScore;
        if (fl + Eps < fr)
            return -1;
        if (fr + Eps < fl)
            return +1;
        if (gl != gr)
            return gl > gr ? -1 : 1; // tie-break towards larger g-values
        return 0;
    }

    public bool LineOfSight(int x0, int y0, int x1, int y1, float parentGScore, out float lineOfSightLeeway, out float lineOfSightDist, out float lineOfSightMinG)
    {
        lineOfSightLeeway = float.MaxValue;
        lineOfSightMinG = float.MaxValue;

        var dx = x1 - x0;
        var dy = y1 - y0;

        var shiftdx = dx >> 31;
        var shiftdy = dy >> 31;

        var stepX = dx == 0 ? 0 : (shiftdx | 1); // Sign of dx
        var stepY = dy == 0 ? 0 : (shiftdy | 1); // Sign of dy

        dx = (dx ^ shiftdx) - shiftdx;  // Absolute value of dx
        dy = (dy ^ shiftdy) - shiftdy;  // Absolute value of dy

        // grid distance in cells (Euclidean) – used only at the end
        lineOfSightDist = MathF.Sqrt(dx * dx + dy * dy);

        // Precompute inverse (avoid div-by-zero branching via MaxValue)
        var invdx = dx != 0 ? 1f / dx : float.MaxValue;
        var invdy = dy != 0 ? 1f / dy : float.MaxValue;

        var tMaxX = _mapHalfResolution * invdx;
        var tMaxY = _mapHalfResolution * invdy;
        var tDeltaX = _mapResolution * invdx;
        var tDeltaY = _mapResolution * invdy;

        int x = x0, y = y0, w = _map.Width;
        var pixG = _map.PixelMaxG;
        var cumulativeG = parentGScore;

        // Quick bound: if parent already unsafe and we dip further, bail early.
        // (Keeps correctness because we only use LOS as an optional improvement.)
        while (true)
        {
            var maxG = pixG[y * w + x];
            if (maxG < 0f)
            {
                return false; // blocked
            }

            if (maxG < lineOfSightMinG)
            {
                lineOfSightMinG = maxG;
            }
            var leeway = maxG - cumulativeG;
            if (leeway < lineOfSightLeeway)
            {
                lineOfSightLeeway = leeway;
            }

            if (x == x1 && y == y1)
            {
                break;
            }

            if (tMaxX < tMaxY)
            {
                tMaxX += tDeltaX;
                x += stepX;
                cumulativeG += _deltaGSide;
            }
            else if (tMaxY < tMaxX)
            {
                tMaxY += tDeltaY;
                y += stepY;
                cumulativeG += _deltaGSide;
            }
            else
            {
                tMaxX += tDeltaX;
                tMaxY += tDeltaY;
                x += stepX;
                y += stepY;
                cumulativeG += _deltaGDiag;
            }
        }
        return true;
    }

    private void VisitNeighbour(int parentIndex, int nodeX, int nodeY, int nodeIndex, float deltaGrid)
    {
        ref var currentParentNode = ref _nodes[parentIndex];
        ref var destNode = ref _nodes[nodeIndex];

        if (destNode.OpenHeapIndex < 0 && destNode.Score >= Score.SemiSafeAsStart)
        {
            return;
        }

        var pixelMaxG = _map.PixelMaxG;
        var destPixG = pixelMaxG[nodeIndex];
        var parentPixG = pixelMaxG[parentIndex];
        if (destPixG < 0f && parentPixG >= 0f)
        {
            return; // impassable
        }

        var stepCost = deltaGrid; // either _deltaGSide or _deltaGDiag
        var candidateG = currentParentNode.GScore + stepCost;

        var candidateLeeway = MathF.Min(currentParentNode.PathLeeway, Math.Min(destPixG, parentPixG) - candidateG);
        var candidateMinG = MathF.Min(currentParentNode.PathMinG, destPixG);

        var altNode = new Node
        {
            GScore = candidateG,
            HScore = destNode.HScore, // or init if first time
            ParentIndex = parentIndex,
            OpenHeapIndex = destNode.OpenHeapIndex,
            PathLeeway = candidateLeeway,
            PathMinG = candidateMinG,
            Score = CalculateScore(destPixG, candidateMinG, candidateLeeway, nodeIndex)
        };
        ref var altnode = ref altNode;
        if (currentParentNode.Score >= Score.UnsafeAsStart && altnode.PathLeeway < 0f && altnode.Score == Score.JustBad) // don't leave safe cells if it requires going through bad cells
        {
            return;
        }

        var grandParentIndex = currentParentNode.ParentIndex;
        ref var grandparentnode = ref _nodes[grandParentIndex];
        if (grandParentIndex != nodeIndex && grandparentnode.PathMinG >= currentParentNode.PathMinG)
        {
            var (gx, gy) = _map.IndexToGrid(grandParentIndex);

            // Attempt to see if we can go directly from grandparent to (nodeX, nodeY)
            if (LineOfSight(gx, gy, nodeX, nodeY, grandparentnode.GScore, out var losLeeway, out var losDist, out var losMinG))
            {
                var losScore = CalculateScore(destPixG, losMinG, losLeeway, nodeIndex);
                if (losScore >= altnode.Score)
                {
                    parentIndex = grandParentIndex;
                    altnode.GScore = _nodes[parentIndex].GScore + _deltaGSide * losDist;
                    altnode.ParentIndex = grandParentIndex;
                    altnode.PathLeeway = losLeeway;
                    altnode.PathMinG = losMinG;
                    altnode.Score = losScore;
                }
            }
        }

        var visit = destNode.OpenHeapIndex == 0 || CompareNodeScores(ref altnode, ref destNode) < (destNode.OpenHeapIndex < 0 ? -1 : 0);
        if (visit)
        {
            if (destNode.OpenHeapIndex < 0)
                ++NumReopens;
            destNode = altnode;
            AddToOpen(nodeIndex);
        }
    }

    private void PrefillH()
    {
        var width = _map.Width;
        var height = _map.Height;
        var maxPriority = _map.MaxPriority;

        var pixelPriority = _map.PixelPriority;
        var nodes = _nodes;

        const float INFf = 1e18f;
        const double INFd = 1e18d;
        var deltaGSide = _deltaGSide;

        // temporary storage for column-pass squared distances
        var colDist = ArrayPool<float>.Shared.Rent(width * height);
        try
        {
            // 1) Column-wise 1D EDT (parallel over columns)
            var partitioner = Partitioner.Create(0, width);
            Parallel.ForEach(partitioner, range =>
            {
                var x1 = range.Item1;
                var x2 = range.Item2;
                var n = height;
                Span<float> f = stackalloc float[n];
                Span<float> d = stackalloc float[n];
                Span<int> v = stackalloc int[n];
                Span<double> z = stackalloc double[n + 1];
                for (var x = x1; x < x2; ++x)
                {
                    for (var y = 0; y < n; ++y)
                    {
                        var idx = y * width + x;
                        f[y] = pixelPriority[idx] == maxPriority ? 0f : INFf;
                    }

                    DistanceTransform1D(f, d, v, z, n, INFd);

                    for (var y = 0; y < n; ++y)
                    {
                        colDist[y * width + x] = d[y];
                    }
                }
            });

            // 2) Row-wise 1D EDT (parallel over rows) using column results -> final distances
            partitioner = Partitioner.Create(0, height);
            Parallel.ForEach(partitioner, range =>
            {
                var y1 = range.Item1;
                var y2 = range.Item2;
                var n = width;
                Span<float> f = stackalloc float[n];
                Span<float> d = stackalloc float[n];
                Span<int> v = stackalloc int[n];
                Span<double> z = stackalloc double[n + 1];

                for (var y = y1; y < y2; ++y)
                {
                    var rowBase = y * width;
                    for (var x = 0; x < n; ++x)
                    {
                        f[x] = colDist[rowBase + x];
                    }
                    DistanceTransform1D(f, d, v, z, n, INFd);

                    for (var x = 0; x < n; ++x)
                    {
                        var iCell = rowBase + x;
                        if (pixelPriority[iCell] < maxPriority)
                        {
                            ref var nd = ref nodes[iCell];
                            nd.HScore = MathF.Sqrt(d[x]) * deltaGSide;
                        }
                    }
                }
            });
        }
        finally
        {
            ArrayPool<float>.Shared.Return(colDist, false);
        }
        // 1D squared distance transform (Felzenszwalb) using provided buffers
        static void DistanceTransform1D(Span<float> f, Span<float> d, Span<int> v, Span<double> z, int n, double INF)
        {
            var k = 0;
            v[0] = 0;
            z[0] = -INF;
            z[1] = INF;

            for (var q = 1; q < n; ++q)
            {
                double s;
                while (true)
                {
                    var vk = v[k];
                    // compute intersection
                    // (f[q] + q*q) - (f[vk] + vk*vk)
                    var num = f[q] + (float)q * q - (f[vk] + (float)vk * vk);
                    var den = 2d * (q - vk);
                    s = num / den;
                    if (s <= z[k])
                    {
                        --k;
                        if (k < 0)
                        {
                            break;
                        }
                        continue;
                    }
                    break;
                }
                ++k;
                v[k] = q;
                z[k] = s;
                z[k + 1] = INF;
            }

            k = 0;
            for (var q = 0; q < n; ++q)
            {
                while (z[k + 1] < q)
                {
                    ++k;
                }
                var diff = q - v[k];
                d[q] = diff * diff + f[v[k]];
            }
        }
    }

    private void AddToOpen(int nodeIndex)
    {
        ref var nd = ref _nodes[nodeIndex];

        if (nd.OpenHeapIndex <= 0)
        {
            _openList.Add(nodeIndex);
            nd.OpenHeapIndex = _openList.Count;
        }
        // update location
        PercolateUp(nd.OpenHeapIndex - 1);
    }

    // remove first (minimal) node from open heap and mark as closed
    private int PopMinOpen()
    {
        var nodeIndex = _openList[0];
        _openList[0] = _openList[^1];
        _nodes[nodeIndex].OpenHeapIndex = -1;
        _openList.RemoveAt(_openList.Count - 1);
        if (_openList.Count > 0)
        {
            _nodes[_openList[0]].OpenHeapIndex = 1;
            PercolateDown(0);
        }
        return nodeIndex;
    }

    private void PercolateUp(int heapIndex)
    {
        var openSpan = CollectionsMarshal.AsSpan(_openList);
        var nodeIndex = openSpan[heapIndex];
        ref var node = ref _nodes[nodeIndex];
        while (heapIndex > 0)
        {
            var parentHeapIndex = (heapIndex - 1) >> 1;
            ref var parentNodeIndex = ref openSpan[parentHeapIndex];
            ref var parent = ref _nodes[parentNodeIndex];
            if (CompareNodeScores(ref node, ref parent) >= 0)
                break; // parent is 'less' (same/better), stop

            openSpan[heapIndex] = parentNodeIndex;
            parent.OpenHeapIndex = heapIndex + 1;
            heapIndex = parentHeapIndex;
        }
        openSpan[heapIndex] = nodeIndex;
        node.OpenHeapIndex = heapIndex + 1;
    }

    private void PercolateDown(int heapIndex)
    {
        var openSpan = CollectionsMarshal.AsSpan(_openList);
        var nodeIndex = openSpan[heapIndex];
        ref var node = ref _nodes[nodeIndex];

        var maxSize = openSpan.Length;
        while (true)
        {
            // find 'better' child
            var childHeapIndex = (heapIndex << 1) + 1;
            if (childHeapIndex >= maxSize)
                break; // node is already a leaf

            var childNodeIndex = openSpan[childHeapIndex];
            ref var child = ref _nodes[childNodeIndex];
            var altChildHeapIndex = childHeapIndex + 1;
            if (altChildHeapIndex < maxSize)
            {
                var altChildNodeIndex = openSpan[altChildHeapIndex];
                ref var altChild = ref _nodes[altChildNodeIndex];
                if (CompareNodeScores(ref altChild, ref child) < 0)
                {
                    childHeapIndex = altChildHeapIndex;
                    childNodeIndex = altChildNodeIndex;
                    child = ref altChild;
                }
            }

            if (CompareNodeScores(ref node, ref child) < 0)
                break; // node is better than best child, so should remain on top

            openSpan[heapIndex] = childNodeIndex;
            child.OpenHeapIndex = heapIndex + 1;
            heapIndex = childHeapIndex;
        }
        openSpan[heapIndex] = nodeIndex;
        node.OpenHeapIndex = heapIndex + 1;
    }
}
