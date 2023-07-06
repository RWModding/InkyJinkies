using Menu.Remix.MixedUI;
using UnityEngine;

namespace InkyJinkies;

public sealed class ModOptions : OptionsTemplate
{
    public static readonly ModOptions Instance = new();

    public readonly Color WarnRed = new(0.85f, 0.35f, 0.4f);

    public const int TAB_COUNT = 1;

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[TAB_COUNT];
        int tabIndex = -1;

        InitGeneral(ref tabIndex);
    }


    private void InitGeneral(ref int tabIndex)
    {
        AddTab(ref tabIndex, "General");

        AddTextLabel("congrats you get nothing", bigText: true);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddTextLabel("well actually here are some credits i guess");
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine(1);

        AddTextLabel("person a");
        AddTextLabel("person b");
        AddTextLabel("person c");
        DrawTextLabels(ref Tabs[tabIndex]);

        AddTextLabel("person d");
        AddTextLabel("person e");
        AddTextLabel("person f");
        DrawTextLabels(ref Tabs[tabIndex]);

        AddTextLabel("person g");
        AddTextLabel("person h");
        AddTextLabel("person i");
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine(3);
        DrawBox(ref Tabs[tabIndex]);
    }
}