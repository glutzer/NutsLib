using MareLib;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace Equimancy;

/// <summary>
/// Keeps track of pipe groups.
/// </summary>
[GameSystem(0, EnumAppSide.Universal)]
public class PipeSystem : GameSystem, IRenderer
{
    public QueuedArray<PipeGroup> pipeGroups = new(1024);

    public PipeDebugGui debugGui = null!;

    public PipeSystem(bool isServer, ICoreAPI api) : base(isServer, api)
    {
        if (api is ICoreClientAPI capi)
        {
            debugGui = new PipeDebugGui();
        }
    }

    public void OnPipeAdded(BlockEntityPipe pipe)
    {
        // Debug.
        currentMousedGroup = -1;

        // Pipe group has already been set.
        if (pipe.groupId != -1) return;

        // Get pipes around pipe.
        GetPipesAround(out List<BlockEntityPipe> pipes, pipe.Pos);

        int foundGroup = -1;
        bool multipleGroups = false;

        foreach (BlockEntityPipe pipeEntity in pipes)
        {
            if (pipeEntity.groupId == -1) continue;

            if (foundGroup != pipeEntity.groupId && foundGroup != -1)
            {
                multipleGroups = true;
                break;
            }

            foundGroup = pipeEntity.groupId;
        }

        if (!multipleGroups)
        {
            if (foundGroup == -1)
            {
                // Build a new pipe group.
                BuildPipeGroup(pipe);
            }
            else
            {
                // Add to the single nearby group.
                pipeGroups[foundGroup].AddPipe(pipe);
            }
        }
        else
        {
            // Remove every pipe group around.
            foreach (BlockEntityPipe pipeEntity in pipes)
            {
                if (pipeGroups[pipeEntity.groupId] != null)
                {
                    pipeGroups.Remove(pipeEntity.groupId);
                }
            }

            // Build a new one.
            BuildPipeGroup(pipe);
        }
    }

    public void OnPipeRemoved(BlockEntityPipe pipe)
    {
        // Debug.
        currentMousedGroup = -1;

        PipeGroup group = pipeGroups[pipe.groupId];
        if (group.pipePositions.Count == 0) pipeGroups.Remove(pipe.groupId);
        group.RemovePipe(pipe);

        GetPipesAround(out List<BlockEntityPipe> pipes, pipe.Pos);

        // No disconnection.
        if (pipes.Count < 2) return;

        HashSet<GridPos> visited = FloodFillUtility.FloodFillBlocks(pipes[0].Pos, vector =>
        {

        }, block => block is BlockPipe, api);

        bool allConnected = true;
        foreach (BlockEntityPipe pipeEntity in pipes)
        {
            if (!visited.Contains(new GridPos(pipeEntity.Pos)))
            {
                allConnected = false;
                break;
            }
        }

        if (allConnected) return;

        // Remove every pipe group around.
        foreach (BlockEntityPipe pipeEntity in pipes)
        {
            if (pipeEntity.groupId == -1) continue;

            if (pipeGroups[pipeEntity.groupId] != null)
            {
                pipeGroups.Remove(pipeEntity.groupId);
            }

            pipeEntity.groupId = -1;
        }

        // Build a new pipe group at each block. If it already has a group, it was handled by a previous build.
        foreach (BlockEntityPipe pipeEntity in pipes)
        {
            if (pipeEntity.groupId != -1) continue;
            BuildPipeGroup(pipeEntity);
        }
    }

    public void GetPipesAround(out List<BlockEntityPipe> pipes, BlockPos pos)
    {
        List<BlockEntityPipe> pipeList = new();

        BlockFaces.IterateBlocksAtFaces(pos, api.World.BlockAccessor, (face, block, blockPos) =>
        {
            if (block is BlockPipe)
            {
                if (api.World.BlockAccessor.GetBlockEntity(blockPos) is not BlockEntityPipe pipe) return;
                pipeList.Add(pipe);
            }
        });

        pipes = pipeList;
    }

    /// <summary>
    /// Build a pipe system from the location of a pipe.
    /// </summary>
    public void BuildPipeGroup(BlockEntityPipe pipe)
    {
        PipeGroup group = new();
        int groupId = pipeGroups.Add(group);
        group.SetGroupId(groupId);

        // Flood fill does not find the pipe because it is not yet added.
        group.AddPipe(pipe);

        BlockPos tempPos = pipe.Pos.Copy();

        FloodFillUtility.FloodFillBlocks(pipe.Pos, vector =>
        {
            tempPos.Set(vector.X, vector.Y, vector.Z);
            if (api.World.BlockAccessor.GetBlockEntity(tempPos) is not BlockEntityPipe blockEntityPipe) return;
            group.AddPipe(blockEntityPipe);
        }, block => block is BlockPipe, api);
    }

    public override void Initialize()
    {
        if (api is ICoreClientAPI capi)
        {
            capi.Event.RegisterRenderer(this, EnumRenderStage.OIT);
        }
    }

    // Rendering pipe fluids.
    public double RenderOrder => 100;
    public int RenderRange => 0;

    private int currentMousedGroup = -1;
    private MeshHandle? currentMousedMesh;
    private GridPos currentMousedOrigin;

    public void OnRenderFrame(float dt, EnumRenderStage stage)
    {
        BlockSelection blockSelection = MainAPI.Capi.World.Player.CurrentBlockSelection;

        // Nothing selected.
        if (blockSelection == null || blockSelection.Block is not BlockPipe)
        {
            currentMousedGroup = -1;
            currentMousedMesh?.Dispose();
            currentMousedMesh = null;
            debugGui.TryClose();
            return;
        }

        // Invalid block.
        if (api.World.BlockAccessor.GetBlockEntity(blockSelection.Position) is not BlockEntityPipe blockEntityPipe)
        {
            currentMousedGroup = -1;
            currentMousedMesh?.Dispose();
            currentMousedMesh = null;
            debugGui.TryClose();
            return;
        }

        // Remake mesh.
        if (blockEntityPipe.groupId != currentMousedGroup)
        {
            currentMousedGroup = blockEntityPipe.groupId;
            currentMousedMesh?.Dispose();

            PipeGroup group = pipeGroups[currentMousedGroup];
            currentMousedOrigin = group.minPos;

            FloodFillUtility.CreateFloodFillMesh(group.pipePositions, currentMousedOrigin, new Vector4(0, 1, 1, 0.2f), out currentMousedMesh);
            debugGui.TryOpen();
        }

        if (currentMousedMesh == null) return;

        IShaderProgram currentShader = ShaderProgramBase.CurrentShaderProgram;

        MareShader oitShader = MareShaderRegistry.Get("oitdebug");
        oitShader.Use();

        oitShader.Uniform("modelMatrix", RenderTools.CameraRelativeTranslation(currentMousedOrigin.X, currentMousedOrigin.Y, currentMousedOrigin.Z));

        oitShader.Uniform("time", MainAPI.Capi.World.ElapsedMilliseconds / 1000f);
        oitShader.Uniform("resolution", new Vector2(MainAPI.RenderWidth, MainAPI.RenderHeight));

        RenderTools.DisableDepthTest();

        RenderTools.RenderMesh(currentMousedMesh);

        RenderTools.EnableDepthTest();

        currentShader?.Use();

        // Update gui.
        PipeGroup pipeGroup = pipeGroups[currentMousedGroup];
    }

    public void Dispose()
    {
        currentMousedMesh?.Dispose();
        GC.SuppressFinalize(this);
    }
}