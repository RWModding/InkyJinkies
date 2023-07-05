using System;
using System.Linq;
using System.Runtime.CompilerServices;
using RWCustom;
using UnityEngine;

namespace InkyJinkies;
    
public static class BobMarley
{
    public class BobMarleyData
    {
        public int JointSprite;
    } 
 
    public static ConditionalWeakTable<Ghost, BobMarleyData> _CWT = new();

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
        
        On.GoldFlakes.GoldFlake.InitiateSprites += GoldFlake_InitiateSprites;
        On.GoldFlakes.GoldFlake.DrawSprites += GoldFlake_DrawSprites;
    }

    private static void GhostOnctor(On.Ghost.orig_ctor orig, Ghost self, Room room, PlacedObject placedobject, GhostWorldPresence worldghost)
    {
        orig(self, room, placedobject, worldghost);

        if (!Plugin.ACRONYM.Equals(room.world?.region?.name, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

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
        var a = Custom.HSL2RGB(100/255f, 0.65f, Mathf.Lerp(0.53f, 0f, ghostMode));
        var b = Custom.HSL2RGB(100/255f, Mathf.Lerp(1f, 0.65f, ghostMode), Mathf.Lerp(1f, 0.53f, ghostMode));
        
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

    private static void Ghost_DrawSprites(On.Ghost.orig_DrawSprites orig, Ghost self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sLeaser, rcam, timestacker, campos);

        if (!Plugin.ACRONYM.Equals(self.room.world?.region?.name, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }
        var joint = sLeaser.sprites[self.GetBobMarleyData().JointSprite];
        var head = (TriangleMesh)sLeaser.sprites[self.HeadMeshSprite];

        joint.rotation = Custom.AimFromOneVectorToAnother(head.vertices.Last(), head.vertices.First());
        joint.SetPosition(head.vertices.Last());
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
        self.GetBobMarleyData().JointSprite = self.totalSprites-1;

        sLeaser.sprites[self.LightSprite].color = new Color(0.3f, 0.7f, 0.3f);
        sLeaser.sprites[self.totalSprites - 1] = new FSprite("atlases/echo_joint")
        {
            anchorX = 0,
            anchorY = 1,
            scale = 0.3f
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