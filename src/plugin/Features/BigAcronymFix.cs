using MonoMod.Cil;

namespace InkyJinkies;

public static class BigAcronymFix
{
    public static void Apply()
    {
        IL.Menu.FastTravelScreen.ctor += BigAcronymFixILHook;
        IL.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += BigAcronymFixILHook;
        IL.PlayerProgression.MiscProgressionData.ConditionalShelterData.GetShelterRegion += BigAcronymFixILHook;
        IL.PlayerProgression.MiscProgressionData.SaveDiscoveredShelter += BigAcronymFixILHook;
        IL.SaveState.SaveToString += BigAcronymFixILHook;
    }

    public static void ApplyExpedition()
    {
        IL.Expedition.ChallengeTools.ValidRegionPearl += BigAcronymFixILHook;
    }
    
    public static void BigAcronymFixILHook(ILContext il)
    {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.Before, 
                   i => i.MatchLdcI4(0), 
                   i => i.MatchLdcI4(2),
                   i => i.MatchCallOrCallvirt<string>(nameof(string.Substring))))
        {
            cursor.Index += 2;
            cursor.Remove();
            cursor.EmitDelegate((string text, int start, int length) =>
            {
                var underscorePos = text.IndexOf('_', start);
                return text.Substring(start, underscorePos >= 0 ? underscorePos - start : length);
            });
        }
    }
}