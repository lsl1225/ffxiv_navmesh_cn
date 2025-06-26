﻿using DotRecast.Detour;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Navmesh.Customizations;

[CustomizationTerritory(1252)]
internal class Z1252OccultCrescent : NavmeshCustomization
{
    public override int Version => 2;

    public override void CustomizeScene(SceneExtractor scene)
    {
        if (scene.Meshes.TryGetValue("bg/ex5/03_ocn_o6/btl/o6b1/collision/o6b1_a5_stc02.pcb", out var mesh))
        {
            // bottom stair of second-tier staircase around SW tower is too steep even though it's <55 degrees, probably because of rasterization bs, extend it outward by 1y to make slope more gradual
            var verts = CollectionsMarshal.AsSpan(mesh.Parts[221].Vertices);
            verts[8].X += 1;
            verts[16].X += 1;
        }
    }

    public override void CustomizeSettings(DtNavMeshCreateParams config)
    {
        // eldergrowth to south fates
        config.AddOffMeshConnection(new Vector3(295.64f, 101.79f, 322.61f), new Vector3(293.91f, 82.02f, 355.45f));

        // eldergrowth to south fates (2)
        config.AddOffMeshConnection(new Vector3(307.39f, 102.88f, 311.06f), new Vector3(339.73f, 69.75f, 321.51f));

        // eldergrowth to south fates (3)
        config.AddOffMeshConnection(new Vector3(309.04f, 102.88f, 314.50f), new Vector3(321.17f, 76.74f, 335.64f));

        // eldergrowth to middle east
        config.AddOffMeshConnection(new Vector3(331.43f, 96.00f, 111.11f), new Vector3(342.42f, 88.90f, 91.92f));

        // purple chest route
        config.AddOffMeshConnection(new Vector3(-337.27f, 47.34f, -419.95f), new Vector3(-333.29f, 7.06f, -451.97f));
    }
}
