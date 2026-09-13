namespace DarkSunWakeRedux.Core;

public readonly record struct GridPoint(int X, int Y);

public sealed record GridPathResult(bool Found, IReadOnlyList<GridPoint> Points)
{
    public static GridPathResult Unreachable { get; } = new(false, []);
}

public static class GridPathfinder
{
    public const int MaximumCellCount = 1_000_000;
    private const int CardinalCost = 10;
    private const int DiagonalCost = 14;
    private static readonly (int X, int Y, int Cost)[] Neighbors =
    [
        (0, -1, CardinalCost), (-1, 0, CardinalCost), (1, 0, CardinalCost),
        (0, 1, CardinalCost), (-1, -1, DiagonalCost), (1, -1, DiagonalCost),
        (-1, 1, DiagonalCost), (1, 1, DiagonalCost)
    ];

    public static GridPathResult FindPath(
        int width,
        int height,
        GridPoint start,
        GridPoint destination,
        Func<GridPoint, bool> isPassable)
    {
        ArgumentNullException.ThrowIfNull(isPassable);
        if (width <= 0 || height <= 0 || (long)width * height > MaximumCellCount)
            throw new ArgumentOutOfRangeException(nameof(width),
                $"Pathfinding grid must contain 1 through {MaximumCellCount} cells.");
        if (!InBounds(start, width, height) || !InBounds(destination, width, height))
            throw new ArgumentOutOfRangeException(nameof(start),
                "Pathfinding endpoints must be within the grid.");
        if (!isPassable(start) || !isPassable(destination)) return GridPathResult.Unreachable;
        if (start == destination) return new(true, [start]);

        var cellCount = checked(width * height);
        var costs = Enumerable.Repeat(int.MaxValue, cellCount).ToArray();
        var previous = Enumerable.Repeat(-1, cellCount).ToArray();
        var closed = new bool[cellCount];
        var open = new PriorityQueue<GridPoint, (int Total, int Estimate, int Sequence)>();
        var sequence = 0;
        costs[Index(start, width)] = 0;
        var initialEstimate = Estimate(start, destination);
        open.Enqueue(start, (initialEstimate, initialEstimate, sequence++));

        while (open.TryDequeue(out var current, out _))
        {
            var currentIndex = Index(current, width);
            if (closed[currentIndex]) continue;
            if (current == destination)
                return new(true, Reconstruct(previous, currentIndex, width));
            closed[currentIndex] = true;

            foreach (var neighbor in Neighbors)
            {
                var next = new GridPoint(current.X + neighbor.X, current.Y + neighbor.Y);
                if (!InBounds(next, width, height) || !isPassable(next)) continue;
                if (neighbor.X != 0 && neighbor.Y != 0 &&
                    (!isPassable(new(current.X + neighbor.X, current.Y)) ||
                     !isPassable(new(current.X, current.Y + neighbor.Y))))
                    continue;
                var nextIndex = Index(next, width);
                if (closed[nextIndex]) continue;
                var candidateCost = checked(costs[currentIndex] + neighbor.Cost);
                if (candidateCost >= costs[nextIndex]) continue;
                costs[nextIndex] = candidateCost;
                previous[nextIndex] = currentIndex;
                var estimate = Estimate(next, destination);
                open.Enqueue(next, (checked(candidateCost + estimate), estimate, sequence++));
            }
        }
        return GridPathResult.Unreachable;
    }

    private static IReadOnlyList<GridPoint> Reconstruct(int[] previous, int current, int width)
    {
        var reversed = new List<GridPoint>();
        while (current >= 0)
        {
            reversed.Add(new(current % width, current / width));
            current = previous[current];
        }
        reversed.Reverse();
        return reversed;
    }

    private static int Estimate(GridPoint from, GridPoint to)
    {
        var deltaX = Math.Abs(from.X - to.X);
        var deltaY = Math.Abs(from.Y - to.Y);
        var diagonal = Math.Min(deltaX, deltaY);
        return checked(diagonal * DiagonalCost + (Math.Max(deltaX, deltaY) - diagonal) * CardinalCost);
    }

    private static bool InBounds(GridPoint point, int width, int height) =>
        point.X >= 0 && point.Y >= 0 && point.X < width && point.Y < height;

    private static int Index(GridPoint point, int width) => point.Y * width + point.X;
}
