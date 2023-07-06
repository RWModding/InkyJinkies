using System;
using System.Linq;
using System.Runtime.CompilerServices;
using RWCustom;
using Smoke;
using UnityEngine;

namespace InkyJinkies;

public static class BobMarley
{
    public class BobMarleyData
    {
        public int JointSprite;
        public Vector2 BluntPos;
        public SteamSmoke Smoke;
    }

    public static ConditionalWeakTable<Ghost, BobMarleyData> _CWT = new();
    public static ConditionalWeakTable<SteamSmoke, object> BluntSmoke = new();

    public static BobMarleyData GetBobMarleyData(this Ghost ghost) => _CWT.GetValue(ghost, _ => new());

    public static void Apply()
    {
        Futile.atlasManager.LoadImage("atlases/echo_joint");
        Futile.atlasManager.LoadImage("atlases/innocent_leaf");

        var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/bobmarley"));
        Custom.rainWorld.Shaders["BobMarleySkin"] = FShader.CreateShader("BobMarleySkin", bundle.LoadAsset<Shader>("Assets/shaders 1.9.03/BobMarleySkin.shader"));
        Custom.rainWorld.Shaders["BobMarleyDistortion"] = FShader.CreateShader("BobMarleyDistortion", bundle.LoadAsset<Shader>("Assets/shaders 1.9.03/BobMarleyDistortion.shader"));

        On.Ghost.ctor += GhostOnctor;
        On.Ghost.InitiateSprites += Ghost_InitiateSprites;
        On.Ghost.DrawSprites += Ghost_DrawSprites;
        On.Ghost.Update += Ghost_Update;

        On.GoldFlakes.GoldFlake.InitiateSprites += GoldFlake_InitiateSprites;
        On.GoldFlakes.GoldFlake.DrawSprites += GoldFlake_DrawSprites;

        On.Ghost.Chains.InitiateSprites += Chains_InitiateSprites;
        
        On.Smoke.SteamSmoke.SteamParticle.AddToContainer += SteamParticle_AddToContainer;
    }

    private static void Ghost_Update(On.Ghost.orig_Update orig, Ghost self, bool eu)
    {
        orig(self, eu);

        if (!Plugin.ACRONYM.Equals(self.room.world?.region?.name, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        var bluntPos = self.GetBobMarleyData().BluntPos;
        if (bluntPos != default)
        {
            self.GetBobMarleyData().Smoke.EmitSmoke(bluntPos + new Vector2(0,10), new Vector2(0.1f, 0.25f), new FloatRect(bluntPos.x - 50f, bluntPos.y - 150f, bluntPos.x + 50f, bluntPos.y + 250f), 1f);
        }
    }

    private static void SteamParticle_AddToContainer(On.Smoke.SteamSmoke.SteamParticle.orig_AddToContainer orig, SteamSmoke.SteamParticle self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);

        if (self.owner is SteamSmoke owner && BluntSmoke.TryGetValue(owner, out _))
        {
            var foreground = rCam.ReturnFContainer("Foreground");
            foreach (var fSprite in sLeaser.sprites)
            {
                fSprite.RemoveFromContainer();
                foreground.AddChild(fSprite);
            }
        }
    }

    private static void Chains_InitiateSprites(On.Ghost.Chains.orig_InitiateSprites orig, Ghost.Chains self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (!Plugin.ACRONYM.Equals(self.ghost.room.world?.region?.name, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }
        
        for (var i = 0; i < self.segments.Length; i++)
        {
            for (var j = 0; j < self.segments[i].GetLength(0); j++)
            {
                sLeaser.sprites[self.firstSprite + self.firstSpriteOfChains[i] + j * 2] = new("atlases/innocent_leaf");
                sLeaser.sprites[self.firstSprite + self.firstSpriteOfChains[i] + j * 2 + 1] = new("atlases/innocent_leaf");
            }
        }
    }

    private static void GhostOnctor(On.Ghost.orig_ctor orig, Ghost self, Room room, PlacedObject placedobject, GhostWorldPresence worldghost)
    {
        orig(self, room, placedobject, worldghost);

        if (!Plugin.ACRONYM.Equals(room.world?.region?.name, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        self.GetBobMarleyData().Smoke = new(room);
        BluntSmoke.Add(self.GetBobMarleyData().Smoke, new());

        self.goldColor = new Color(0.2f, 0.85f, 0.2f);
    }

    private static void GoldFlake_DrawSprites(On.GoldFlakes.GoldFlake.orig_DrawSprites orig, GoldFlakes.GoldFlake self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.active || !Plugin.ACRONYM.Equals(self.room.world?.region?.name, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        var t = Mathf.InverseLerp(-1f, 1f, Vector2.Dot(Custom.DegToVec(45f), Custom.DegToVec(Mathf.Lerp(self.lastYRot, self.yRot, timeStacker) * 57.29578f + Mathf.Lerp(self.lastRot, self.rot, timeStacker))));
        var ghostMode = rCam.ghostMode;
        var a = Custom.HSL2RGB(100 / 255f, 0.65f, Mathf.Lerp(0.53f, 0f, ghostMode));
        var b = Custom.HSL2RGB(100 / 255f, Mathf.Lerp(1f, 0.65f, ghostMode), Mathf.Lerp(1f, 0.53f, ghostMode));

        sLeaser.sprites[0].color = Color.Lerp(a, b, t);
        sLeaser.sprites[0].scaleX *= 1.5f;
        sLeaser.sprites[0].scaleY *= 1.5f;
    }

    private static void GoldFlake_InitiateSprites(On.GoldFlakes.GoldFlake.orig_InitiateSprites orig, GoldFlakes.GoldFlake self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (!Plugin.ACRONYM.Equals(self.room.world?.region?.name, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        sLeaser.sprites[0].RemoveFromContainer();
        sLeaser.sprites[0] = new("atlases/innocent_leaf");

        self.AddToContainer(sLeaser, rCam, null);
    }

    private static void Ghost_DrawSprites(On.Ghost.orig_DrawSprites orig, Ghost self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rcam, float timestacker, Vector2 camPos)
    {
        orig(self, sLeaser, rcam, timestacker, camPos);

        if (!Plugin.ACRONYM.Equals(self.room.world?.region?.name, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        var joint = sLeaser.sprites[self.GetBobMarleyData().JointSprite];
        var head = (TriangleMesh)sLeaser.sprites[self.HeadMeshSprite];

        joint.rotation = Custom.AimFromOneVectorToAnother(head.vertices.Last(), head.vertices.First());
        joint.SetPosition(head.vertices.Last());

        var jointAngle = Custom.DegToVec(joint.rotation + 135);
        var smokePos = joint.GetPosition() + (jointAngle * 105) + camPos;
        self.GetBobMarleyData().BluntPos = smokePos;
    }

    private static void Ghost_InitiateSprites(On.Ghost.orig_InitiateSprites orig, Ghost self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (!Plugin.ACRONYM.Equals(self.room.world?.region?.name, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        self.totalSprites++;
        Array.Resize(ref sLeaser.sprites, self.totalSprites);
        self.GetBobMarleyData().JointSprite = self.totalSprites - 1;

        sLeaser.sprites[self.LightSprite].color = new Color(0.3f, 0.7f, 0.3f);
        sLeaser.sprites[self.totalSprites - 1] = new FSprite("atlases/echo_joint")
        {
            anchorX = 0.03f,
            anchorY = 0.97f,
            scale = 0.27f
        };

        for (int i = 0; i < self.legs.GetLength(0); i++)
        {
            sLeaser.sprites[self.ThightSprite(i)].shader = rCam.game.rainWorld.Shaders["BobMarleySkin"];
            sLeaser.sprites[self.LowerLegSprite(i)].shader = rCam.game.rainWorld.Shaders["BobMarleySkin"];
        }

        sLeaser.sprites[self.DistortionSprite].shader = rCam.game.rainWorld.Shaders["BobMarleyDistortion"];
        sLeaser.sprites[self.HeadMeshSprite].shader = rCam.game.rainWorld.Shaders["BobMarleySkin"];

        self.AddToContainer(sLeaser, rCam, null);
    }
}