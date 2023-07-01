using RWCustom;

namespace InkyJinkies;

public static class Overlay
{
    public static FContainer AlwaysOnTopContainer;
    public static FSprite ActivateWindowsSprite;

    public static void Apply()
    {
        On.FContainer.AddChild += FContainer_AddChild;
        On.Futile.UpdateScreenWidth += Futile_UpdateScreenWidth;
        On.RainWorld.Update += RainWorld_Update;

        Futile.atlasManager.LoadImage("atlases/activate_windows");

        AlwaysOnTopContainer = new FContainer(); 
        Futile.stage.AddChild(AlwaysOnTopContainer);
            
        ActivateWindowsSprite = new FSprite("atlases/activate_windows");
        ActivateWindowsSprite.SetAnchor(1, 0);
        ActivateWindowsSprite.scale = 1.1f;
        AlwaysOnTopContainer.AddChild(ActivateWindowsSprite);
        
        ReadjustSpritePositions();
    }

    private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
    {
        orig(self);

        AlwaysOnTopContainer.isVisible = self.processManager.currentMainLoop is RainWorldGame { world.region.name: "OWO" };
    }

    private static void Futile_UpdateScreenWidth(On.Futile.orig_UpdateScreenWidth orig, Futile self, int newwidth)
    {
        orig(self, newwidth);

        ReadjustSpritePositions();
    }

    public static void ReadjustSpritePositions()
    {
        var screenOffsets = Custom.GetScreenOffsets();
        var leftAnchor = screenOffsets[0];
        var rightAnchor = screenOffsets[1];

        ActivateWindowsSprite.x = rightAnchor;
        ActivateWindowsSprite.y = 50;
    }
        
    public static void FContainer_AddChild(On.FContainer.orig_AddChild orig, FContainer self, FNode node)
    {
        orig(self, node);
        if (self != Futile.stage || node == AlwaysOnTopContainer)
        {
            return;
        }
            
        AlwaysOnTopContainer.MoveToFront();
        self.AddChild(AlwaysOnTopContainer);
    }
}