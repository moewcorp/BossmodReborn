namespace BossMod.Pathfinding;

public class ThetaStar
{
    public struct Node
    {
        public float GScore;
        public float HScore;
        public int ParentX;
        public int ParentY;
        public int OpenHeapIndex; // -1 if in closed list, 0 if not in any lists, otherwise (index+1)
        public float PathLeeway;
    }

    private Map _map = new();
    private readonly List<(int x, int y)> _goals = [];
    private Node[] _nodes = [];
    private float[] _distances = [];
    private readonly List<int> _openList = [];
    private float _deltaGSide;
    private float _deltaGDiag;
    private const float Epsilon = 1e-5f;
    private readonly object _lock = new();
    private (int dx, int dy, float cost)[] NeighborOffsets = [];
    public ref Node NodeByIndex(int index) => ref _nodes[index];
    public int CellIndex(int x, int y) => y * _map.Width + x;
    public WPos CellCenter(int index) => _map.GridToWorld(index % _map.Width, index / _map.Width, 0.5f, 0.5f);
    private float _mapResolution;
    private float _mapHalfResolution;

    // gMultiplier is typically inverse speed, which turns g-values into time
    public void Start(Map map, IEnumerable<(int x, int y)> goals, (int x, int y) start, float gMultiplier)
    {
        lock (_lock)
        {
            _map = map;
            _goals.Clear();
            _goals.AddRange(goals);
            var numPixels = map.Width * map.Height;
            if (_nodes == null || _nodes.Length < numPixels)
                _nodes = new Node[numPixels];
            Array.Fill(_nodes, default, 0, numPixels);
            if (_distances == null || _distances.Length < numPixels)
                _distances = new float[numPixels];
            _openList.Clear();
            _deltaGSide = map.Resolution * gMultiplier;
            _deltaGDiag = _deltaGSide * 1.414214f;
            _mapResolution = map.Resolution;
            _mapHalfResolution = map.Resolution * 0.5f;
            NeighborOffsets =
            [
                (-1, 0, _deltaGSide),
                (1, 0, _deltaGSide),
                (0, -1, _deltaGSide),
                (0, 1, _deltaGSide),
                (-1, -1, _deltaGDiag),
                (-1, 1, _deltaGDiag),
                (1, -1, _deltaGDiag),
                (1, 1, _deltaGDiag),
            ];
            DijkstraDistance();

            start = map.ClampToGrid(start);
            var startIndex = CellIndex(start.x, start.y);
            _nodes[startIndex].GScore = 0;
            _nodes[startIndex].HScore = _distances[startIndex];
            _nodes[startIndex].ParentX = start.x; // start's parent is self
            _nodes[startIndex].ParentY = start.y;
            _nodes[startIndex].PathLeeway = float.MaxValue; // min diff along path between node's g-value and cell's g-value
            AddToOpen(startIndex);
        }
    }

    public void Start(Map map, int goalPriority, WPos startPos, float gMultiplier) => Start(map, map.Goals().Where(g => g.priority >= goalPriority).Select(g => (g.x, g.y)), map.WorldToGrid(startPos), gMultiplier);

    // returns whether search is to be terminated; on success, first node of the open list would contain found goal
    public bool ExecuteStep()
    {
        lock (_lock)
        {
            if (_goals.Count == 0 || _openList.Count == 0 || _nodes[_openList[0]].HScore <= 0)
                return false;

            var nextNodeIndex = PopMinOpen();
            var nextNodeX = nextNodeIndex % _map.Width;
            var nextNodeY = nextNodeIndex / _map.Width;
            var haveN = nextNodeY > 0;
            var haveS = nextNodeY < _map.Height - 1;
            var haveE = nextNodeX > 0;
            var haveW = nextNodeX < _map.Width - 1;
            if (haveN)
            {
                VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY - 1, nextNodeIndex - _map.Width, _deltaGSide);
                if (haveE)
                    VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX - 1, nextNodeY - 1, nextNodeIndex - _map.Width - 1, _deltaGDiag);
                if (haveW)
                    VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX + 1, nextNodeY - 1, nextNodeIndex - _map.Width + 1, _deltaGDiag);
            }
            if (haveE)
                VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX - 1, nextNodeY, nextNodeIndex - 1, _deltaGSide);
            if (haveW)
                VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX + 1, nextNodeY, nextNodeIndex + 1, _deltaGSide);
            if (haveS)
            {
                VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY + 1, nextNodeIndex + _map.Width, _deltaGSide);
                if (haveE)
                    VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX - 1, nextNodeY + 1, nextNodeIndex + _map.Width - 1, _deltaGDiag);
                if (haveW)
                    VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX + 1, nextNodeY + 1, nextNodeIndex + _map.Width + 1, _deltaGDiag);
            }
            return true;
        }
    }

    public int CurrentResult()
    {
        lock (_lock)
        {
            return _openList.Count > 0 && _nodes[_openList[0]].HScore <= 0 ? _openList[0] : -1;
        }
    }

    public int Execute()
    {
        while (ExecuteStep())
            ;
        return CurrentResult();
    }

    private void VisitNeighbour(int parentX, int parentY, int parentIndex, int nodeX, int nodeY, int nodeIndex, float deltaG)
    {
        ref var node = ref _nodes[nodeIndex];
        if (node.OpenHeapIndex < 0)
            return; // in closed list already

        ref var parentnode = ref _nodes[parentIndex];

        var tentativeG = parentnode.GScore + deltaG;
        var nodeLeeway = _map.Pixels[nodeIndex].MaxG - tentativeG;
        if (nodeLeeway < 0)
            return; // node is blocked along this path

        var grandParentX = parentnode.ParentX;
        var grandParentY = parentnode.ParentY;
        var grandParentIndex = CellIndex(grandParentX, grandParentY);

        var newG = tentativeG;
        if (LineOfSight(grandParentX, grandParentY, nodeX, nodeY, parentnode.GScore))
        {
            // If there is line of sight, set the parent to the grandparent
            parentX = grandParentX;
            parentY = grandParentY;
            parentIndex = grandParentIndex;

            // Recalculate newG based on the grandparent
            var distance = MathF.Sqrt(DistanceSq(nodeX, nodeY, parentX, parentY));
            newG = _nodes[parentIndex].GScore + _deltaGSide * distance;
        }

        if (node.OpenHeapIndex == 0)
        {
            node.GScore = float.MaxValue;
            node.HScore = _distances[nodeIndex];
        }

        if (newG + Epsilon < node.GScore)
        {
            node.GScore = newG;
            node.ParentX = parentX;
            node.ParentY = parentY;
            node.PathLeeway = MathF.Min(parentnode.PathLeeway, nodeLeeway);

            if (node.OpenHeapIndex == 0)
            {
                AddToOpen(nodeIndex);
            }
            else
            {
                PercolateUp(node.OpenHeapIndex - 1);
            }
        }
    }

    private bool LineOfSight(int x0, int y0, int x1, int y1, float parentGScore)
    {
        var dx = x1 - x0;
        var dy = y1 - y0;

        var stepX = Math.Sign(dx);
        var stepY = Math.Sign(dy);

        dx = Math.Abs(dx);
        dy = Math.Abs(dy);

        var invdx = (dx != 0) ? 1f / dx : float.MaxValue;
        var invdy = (dy != 0) ? 1f / dy : float.MaxValue;

        var tMaxX = _mapHalfResolution * invdx;
        var tMaxY = _mapHalfResolution * invdy;

        var tDeltaX = _mapResolution * invdx;
        var tDeltaY = _mapResolution * invdy;

        var x = x0;
        var y = y0;
        var cumulativeG = parentGScore;

        while (x != x1 || y != y1)
        {
            var nodeIndex = CellIndex(x, y);

            // Check if the node is entirely blocked
            if (_map.Pixels[nodeIndex].MaxG <= 0)
                return false;

            // Determine the movement cost based on the direction
            float movementCost;

            if (tMaxX < tMaxY)
            {
                tMaxX += tDeltaX;
                x += stepX;
                movementCost = _deltaGSide;
            }
            else if (tMaxY < tMaxX)
            {
                tMaxY += tDeltaY;
                y += stepY;
                movementCost = _deltaGSide;
            }
            else // tMaxX == tMaxY, moving diagonally
            {
                tMaxX += tDeltaX;
                tMaxY += tDeltaY;
                x += stepX;
                y += stepY;
                movementCost = _deltaGDiag;
            }

            cumulativeG += movementCost;

            // Check if the cumulative G-score exceeds MaxG
            if (cumulativeG - Epsilon > _map.Pixels[nodeIndex].MaxG)
                return false;
        }

        // Check the last node
        return cumulativeG - Epsilon <= _map.Pixels[CellIndex(x1, y1)].MaxG;
    }

    private void DijkstraDistance()
    {
        var numPixels = _map.Width * _map.Height;
        Array.Fill(_distances, float.MaxValue, 0, numPixels);

        var openList = new List<int>();
        var inOpenHeapIndex = new int[numPixels];
        Array.Fill(inOpenHeapIndex, 0, 0, numPixels);

        for (var i = 0; i < _goals.Count; ++i)
        {
            var goal = _goals[i];
            var goalIndex = CellIndex(goal.x, goal.y);
            _distances[goalIndex] = 0;
            openList.Add(goalIndex);
            inOpenHeapIndex[goalIndex] = openList.Count;
        }

        while (openList.Count != 0)
        {
            var currentIndex = PopMinOpen(openList, inOpenHeapIndex);
            var currentX = currentIndex % _map.Width;
            var currentY = currentIndex / _map.Width;

            for (var i = 0; i < 8; ++i)
            {
                var (dx, dy, cost) = NeighborOffsets[i];
                var neighborX = currentX + dx;
                var neighborY = currentY + dy;

                if (neighborX < 0 || neighborX >= _map.Width || neighborY < 0 || neighborY >= _map.Height)
                    continue;

                var neighborIndex = CellIndex(neighborX, neighborY);
                var neighborMaxG = _map.Pixels[neighborIndex].MaxG;

                if (neighborMaxG <= 0)
                    continue;

                var newDistance = _distances[currentIndex] + cost;

                // If the new distance exceeds MaxG, skip this neighbor
                if (newDistance > neighborMaxG)
                    continue;

                if (newDistance < _distances[neighborIndex])
                {
                    _distances[neighborIndex] = newDistance;
                    if (inOpenHeapIndex[neighborIndex] == 0)
                    {
                        openList.Add(neighborIndex);
                        inOpenHeapIndex[neighborIndex] = openList.Count;
                    }
                    PercolateUp(openList, inOpenHeapIndex, neighborIndex);
                }
            }
        }
    }

    private int PopMinOpen(List<int> openList, int[] inOpenHeapIndex)
    {
        var nodeIndex = openList[0];
        openList[0] = openList[^1];
        inOpenHeapIndex[nodeIndex] = -1;
        openList.RemoveAt(openList.Count - 1);
        if (openList.Count > 0)
        {
            inOpenHeapIndex[openList[0]] = 1;
            PercolateDown(openList, inOpenHeapIndex, 0);
        }
        return nodeIndex;
    }

    private void PercolateUp(List<int> openList, int[] inOpenHeapIndex, int nodeIndex)
    {
        var heapIndex = inOpenHeapIndex[nodeIndex] - 1;
        var parent = (heapIndex - 1) >> 1;
        while (heapIndex > 0 && _distances[openList[heapIndex]] < _distances[openList[parent]])
        {
            (openList[parent], openList[heapIndex]) = (openList[heapIndex], openList[parent]);
            inOpenHeapIndex[openList[heapIndex]] = heapIndex + 1;
            inOpenHeapIndex[openList[parent]] = parent + 1;

            heapIndex = parent;
            parent = (heapIndex - 1) >> 1;
        }
    }

    private void PercolateDown(List<int> openList, int[] inOpenHeapIndex, int heapIndex)
    {
        var maxSize = openList.Count;
        while (true)
        {
            var leftChild = (heapIndex << 1) + 1;
            if (leftChild >= maxSize)
                break;
            var rightChild = leftChild + 1;
            var smallest = heapIndex;

            if (_distances[openList[leftChild]] < _distances[openList[smallest]])
                smallest = leftChild;
            if (rightChild < maxSize && _distances[openList[rightChild]] < _distances[openList[smallest]])
                smallest = rightChild;
            if (smallest == heapIndex)
                break;

            (openList[smallest], openList[heapIndex]) = (openList[heapIndex], openList[smallest]);
            inOpenHeapIndex[openList[heapIndex]] = heapIndex + 1;
            inOpenHeapIndex[openList[smallest]] = smallest + 1;

            heapIndex = smallest;
        }
    }

    private static float DistanceSq(int x1, int y1, int x2, int y2)
    {
        var dx = x1 - x2;
        var dy = y1 - y2;
        return dx * dx + dy * dy;
    }

    private void AddToOpen(int nodeIndex)
    {
        if (_nodes[nodeIndex].OpenHeapIndex <= 0)
        {
            _openList.Add(nodeIndex);
            _nodes[nodeIndex].OpenHeapIndex = _openList.Count;
        }
        // update location
        PercolateUp(_nodes[nodeIndex].OpenHeapIndex - 1);
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
        var nodeIndex = _openList[heapIndex];
        var parent = (heapIndex - 1) >> 1;
        while (heapIndex > 0 && HeapLess(nodeIndex, _openList[parent]))
        {
            _openList[heapIndex] = _openList[parent];
            _nodes[_openList[heapIndex]].OpenHeapIndex = heapIndex + 1;
            heapIndex = parent;
            parent = (heapIndex - 1) >> 1;
        }
        _openList[heapIndex] = nodeIndex;
        _nodes[nodeIndex].OpenHeapIndex = heapIndex + 1;
    }

    private void PercolateDown(int heapIndex)
    {
        var nodeIndex = _openList[heapIndex];
        var maxSize = _openList.Count;
        while (true)
        {
            var child1 = (heapIndex << 1) + 1;
            if (child1 >= maxSize)
                break;
            var child2 = child1 + 1;
            if (child2 == maxSize || HeapLess(_openList[child1], _openList[child2]))
            {
                if (HeapLess(_openList[child1], nodeIndex))
                {
                    _openList[heapIndex] = _openList[child1];
                    _nodes[_openList[heapIndex]].OpenHeapIndex = heapIndex + 1;
                    heapIndex = child1;
                }
                else
                {
                    break;
                }
            }
            else if (HeapLess(_openList[child2], nodeIndex))
            {
                _openList[heapIndex] = _openList[child2];
                _nodes[_openList[heapIndex]].OpenHeapIndex = heapIndex + 1;
                heapIndex = child2;
            }
            else
            {
                break;
            }
        }
        _openList[heapIndex] = nodeIndex;
        _nodes[nodeIndex].OpenHeapIndex = heapIndex + 1;
    }

    private bool HeapLess(int nodeIndexLeft, int nodeIndexRight)
    {
        ref var nodeL = ref _nodes[nodeIndexLeft];
        ref var nodeR = ref _nodes[nodeIndexRight];
        var fl = nodeL.GScore + nodeL.HScore;
        var fr = nodeR.GScore + nodeR.HScore;
        if (fl + Epsilon < fr)
            return true;
        else if (fr + Epsilon < fl)
            return false;
        else
            return nodeL.GScore > nodeR.GScore; // tie-break towards larger g-values
    }
}
