using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using MonoMod.Cil;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace InkyJinkies
{
    [BepInPlugin("inkyjinkies", "Inky Jinkies", "1.0.0")]
    public partial class Plugin : BaseUnityPlugin
    {
        private bool IsInit;

        private void OnEnable()
        {
            try
            {
                if (IsInit) return;
                IsInit = true;

                IL.Expedition.ChallengeTools.ValidRegionPearl += BigAcronymFix;
                IL.Menu.FastTravelScreen.ctor += BigAcronymFix;
                IL.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += BigAcronymFix;
                IL.PlayerProgression.MiscProgressionData.ConditionalShelterData.GetShelterRegion += BigAcronymFix;
                IL.PlayerProgression.MiscProgressionData.SaveDiscoveredShelter += BigAcronymFix;
                IL.SaveState.SaveToString += BigAcronymFix;

                Debug.Log($"Plugin inkyjinkies is loaded!");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void BigAcronymFix(ILContext il)
        {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(0),
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
}