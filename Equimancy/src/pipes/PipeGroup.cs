using MareLib;
using System;
using System.Collections.Generic;

namespace Equimancy;

/// <summary>
/// Contiguous length of pipe entities.
/// </summary>
public class PipeGroup
{
    public int GroupId { get; private set; } = -1;

    public HashSet<GridPos> pipePositions = new();

    public GridPos minPos = new(int.MaxValue, int.MaxValue, int.MaxValue);
    public GridPos maxPos = new(int.MinValue, int.MinValue, int.MinValue);

    public PipeGroup()
    {

    }

    public void SetGroupId(int id)
    {
        if (GroupId == -1)
        {
            GroupId = id;
        }
    }

    /// <summary>
    /// Adds a pipe and sets its id.
    /// </summary>
    public void AddPipe(BlockEntityPipe pipe)
    {
        pipePositions.Add(new GridPos(pipe.Pos));
        pipe.groupId = GroupId;

        minPos = new GridPos(Math.Min(minPos.X, pipe.Pos.X), Math.Min(minPos.Y, pipe.Pos.Y), Math.Min(minPos.Z, pipe.Pos.Z));
        maxPos = new GridPos(Math.Max(maxPos.X, pipe.Pos.X), Math.Max(maxPos.Y, pipe.Pos.Y), Math.Max(maxPos.Z, pipe.Pos.Z));
    }

    /// <summary>
    /// Removes a pipe and resets its id.
    /// </summary>
    public void RemovePipe(BlockEntityPipe pipe)
    {
        GridPos position = new(pipe.Pos);

        pipePositions.Remove(position);
        pipe.groupId = -1;

        if (IsTouchingEdge(position))
        {
            // Rebuild the min/max positions.
            minPos = new GridPos(int.MaxValue, int.MaxValue, int.MaxValue);
            maxPos = new GridPos(int.MinValue, int.MinValue, int.MinValue);

            foreach (GridPos pos in pipePositions)
            {
                minPos = new GridPos(Math.Min(minPos.X, pos.X), Math.Min(minPos.Y, pos.Y), Math.Min(minPos.Z, pos.Z));
                maxPos = new GridPos(Math.Max(maxPos.X, pos.X), Math.Max(maxPos.Y, pos.Y), Math.Max(maxPos.Z, pos.Z));
            }
        }
    }

    /// <summary>
    /// Is this on the edge of the min/max position?
    /// </summary>
    public bool IsTouchingEdge(GridPos position)
    {
        return position.X == minPos.X || position.X == maxPos.X || position.Y == minPos.Y || position.Y == maxPos.Y || position.Z == minPos.Z || position.Z == maxPos.Z;
    }
}