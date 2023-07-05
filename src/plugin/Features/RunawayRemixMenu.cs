using Menu.Remix;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Menu.Remix.MenuModList;

namespace InkyJinkies;

// yeah -forthbridge
public static class RunawayRemixMenu
{
    public static void Apply() => On.Menu.Remix.MenuModList.Update += MenuModList_Update;

    public class MenuModListModule
    {
        public int Timer { get; set; }
        public int MoveCounter { get; set; }
        public int Dir { get; set; }
        public float Hue { get; set; }
        public Queue<float> MouseVel { get; set; } = new();
    }

    public static readonly ConditionalWeakTable<MenuModList, MenuModListModule> MenuModListData = new();

    public static MenuModListModule GetMenuModListModule(this MenuModList self) => MenuModListData.GetOrCreateValue(self);

    private static void MenuModList_Update(On.Menu.Remix.MenuModList.orig_Update orig, MenuModList self)
    {
        orig(self);

        var module = self.GetMenuModListModule();

        var thisModButton = self.modButtons.FirstOrDefault(x => x.ModID == Plugin.MOD_ID);
        if (thisModButton == null) return;

        module.Hue += 0.01f;

        if (module.Hue > 1.0f)
            module.Hue = 0.0f;

        thisModButton.SetColor(Custom.HSL2RGB(module.Hue, thisModButton.selectEnabled ? 1.0f : 0.15f, 0.5f));

        if (!thisModButton.selectEnabled) return;

        var activeModCount = self._currentSelections.Length;

        if (activeModCount <= 2) return;

        var buttonPos = thisModButton.ScreenPos;
        var mousePos = self.Menu.mousePosition;
        var lastMousePos = self.Menu.lastMousePos;

        var mouseVel = Mathf.Abs((mousePos - lastMousePos).magnitude);

        module.MouseVel.Enqueue(mouseVel);

        if (module.MouseVel.Count > 5)
            module.MouseVel.Dequeue();

        var avgMouseVel = module.MouseVel.Count > 0 ? module.MouseVel.Average() : 0.0f;

        if (avgMouseVel < 2.0f) return;

        var xDist = Mathf.Abs(buttonPos.x + 100.0f - mousePos.x);
        var yDist = Mathf.Abs(buttonPos.y - mousePos.y);

        var inRange = yDist < Custom.LerpMap(activeModCount, 0, 5, 20.0f, 50.0f);

        if (inRange && xDist < 130.0f)
        {
            if (activeModCount <= 2)
                module.MoveCounter = 0;

            else if (activeModCount <= 8)
                module.MoveCounter = 1;

            else if (activeModCount <= 15)
                module.MoveCounter = 2;

            else
                module.MoveCounter = 3;
        }

        var wait = Custom.LerpMap(yDist, 75.0f, 5.0f, 5, 0);
        module.Timer++;

        if (module.Timer <= wait) return;
        module.Timer = 0;

        if (module.MoveCounter <= 0) return;
        module.MoveCounter--;

        var yDiff = buttonPos.y - mousePos.y;

        if (inRange)
            module.Dir = yDiff <= 0 ? 1 : -1;

        var index = thisModButton.viewIndex + module.Dir;

        if (activeModCount > 4)
        {
            if (thisModButton.selectOrder + module.Dir > activeModCount - 1)
            {
                index = 0;
                module.MoveCounter++;
            }
            else if (thisModButton.selectOrder + module.Dir < 0)
            {
                index = activeModCount - 1;
                module.MoveCounter++;
            }
        }

        var nextModButton = self.visibleModButtons[index];

        (thisModButton.selectOrder, nextModButton.selectOrder) = (nextModButton.selectOrder, thisModButton.selectOrder);
        self.FakeRefreshAllButtons();
    }

    public static void FakeRefreshAllButtons(this MenuModList self)
    {
        self.sortedModButtons.Clear();

        ConfigMenuTab._countModEnabled = 0;
        ConfigMenuTab._countModTotal = 0;
        
        List<ModButton> btns = new();
        List<ModButton> list = new();
        
        for (int i = 1; i < self.modButtons.Length; i++)
        {
            if (self.modButtons[i].selectEnabled)
            {
                btns.Add(self.modButtons[i]);
                ConfigMenuTab._countModEnabled++;
            }
            else
            {
                list.Add(self.modButtons[i]);
            }

            if (!self.modButtons[i].itf.mod.DLCMissing)
            {
                ConfigMenuTab._countModTotal++;
            }
        }

        self._CheckRequirementsOrder(ref btns);
        btns.Sort(CompareBySelectOrder);
        for (int j = 0; j < btns.Count; j++)
        {
            btns[j].selectOrder = j;
        }

        self.sortedModButtons.AddRange(btns);
        if (btns.Count > 0)
        {
            ModButton._ButtonAboveSeparator = btns.Last();
        }
        else
        {
            ModButton._ButtonAboveSeparator = null;
        }

        list.Sort(CompareByModName);
        if (list.Count > 0)
        {
            ModButton._ButtonBelowSeparator = list[0];
        }
        else
        {
            ModButton._ButtonBelowSeparator = null;
        }

        self.sortedModButtons.AddRange(list);
        self.RefreshVisibleButtons();

        self._currentSelections = new string[btns.Count];

        for (int k = 0; k < btns.Count; k++)
        {
            self._currentSelections[k] = btns[k].ModID;
        }
    }
}
