using System;
using System.Linq;
using Menu;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InkyJinkies;

public static class RollingMinigame
{
    public enum Phase
    {
        Start,
        InGrinder,
        Grinding,
        InGrinderReady,
        Ground,
        Rolling,
        Finished
    }

    public static void Apply()
    {
        On.RainWorldGame.Update += (orig, self) =>
        {
            orig(self);
            if (Input.GetKey(KeyCode.Keypad9) && !self.GamePaused)
            {
                TryStart(self);
            }
        };

        Futile.atlasManager.LoadImage("atlases/blunt_wrap");
        Futile.atlasManager.LoadImage("atlases/blunt_wrap0");
        Futile.atlasManager.LoadImage("atlases/blunt_wrap1");
        Futile.atlasManager.LoadImage("atlases/blunt_wrap2");
        Futile.atlasManager.LoadImage("atlases/blunt_wrap3");
        Futile.atlasManager.LoadImage("atlases/blunt_wrap4");
        Futile.atlasManager.LoadImage("atlases/finished_blunt");
        Futile.atlasManager.LoadImage("atlases/ground_leaves");
        Futile.atlasManager.LoadImage("atlases/weed_grinder0");
        Futile.atlasManager.LoadImage("atlases/weed_grinder1");
        Futile.atlasManager.LoadImage("atlases/weed_grinder2");
        Futile.atlasManager.LoadImage("atlases/weed_grinder3");
        Futile.atlasManager.LoadImage("atlases/open_weed_grinder");
        Futile.atlasManager.LoadImage("atlases/open_weed_grinder_leaves");
        Futile.atlasManager.LoadImage("atlases/open_weed_grinder_finished");
        Futile.atlasManager.LoadImage("atlases/weed_leaves");
    }

    public static bool TryStart(RainWorldGame game)
    {
        if (game.pauseMenu != null)
        {
            return false;
        }
        
        game.ShowPauseMenu();

        if (game.pauseMenu != null)
        {
            game.manager.ShowDialog(new Interface(game.pauseMenu));
            game.pauseMenu.container.alpha = 0;
        }
        
        return false;
    }

    public class Interface : Dialog
    {
        public PauseMenu pauseMenu;
        public RainWorldGame game;
        public ProcessManager manager;
        
        public float blackFade;
        public float lastBlackFade;
        public bool wantToExit;

        public FSprite blackSprite;

        public FSprite weedLeaves;
        public FSprite weedGrinder;
        public FSprite bluntWrap;

        public FSprite[] miniGameSprites;

        public FSprite draggedSprite;
        public Vector2 draggedLastPos;
        public Vector2 draggedPos;
        public Vector2 draggedOffset;

        public float[] borders;

        public Phase phase;
        public float progress;
        public int grinderState;

        public Interface(PauseMenu pauseMenu) : base(pauseMenu.manager)
        {
            this.pauseMenu = pauseMenu;
            game = pauseMenu.game;
            manager = pauseMenu.manager;

            borders = Custom.GetScreenOffsets();
            
            pages.Add(new Page(this, null, "main", 0));
            blackSprite = new FSprite("pixel")
            {
                color = MenuRGB(MenuColors.Black),
                scaleX = 1400f,
                scaleY = 800f,
                x = manager.rainWorld.options.ScreenSize.x / 2f,
                y = manager.rainWorld.options.ScreenSize.y / 2f,
                alpha = 0.5f
            };
            pages[0].Container.AddChild(blackSprite);

            weedGrinder = new("atlases/open_weed_grinder")
            {
                x = borders[1] - 200,
                y = 400
            };
            pages[0].Container.AddChild(weedGrinder);

            bluntWrap = new("atlases/blunt_wrap")
            {
                x = (borders[1] / 2) - 125,
                y = 225
            };
            pages[0].Container.AddChild(bluntWrap);

            weedLeaves = new("atlases/weed_leaves")
            {
                x = borders[0] + 250,
                y = 400
            };
            pages[0].Container.AddChild(weedLeaves);

            miniGameSprites = new[] { weedLeaves, weedGrinder, bluntWrap };
            phase = Phase.Start;
        }

        public void ProgressPhase()
        {
            switch (phase)
            {
                case Phase.Start:
                    weedLeaves.isVisible = false;
                    weedGrinder.element = Futile.atlasManager.GetElementWithName("atlases/open_weed_grinder_leaves");
                    NextPhase();
                    break;
                case Phase.InGrinder:
                    weedGrinder.element = Futile.atlasManager.GetElementWithName("atlases/weed_grinder0");
                    NextPhase();
                    break;
                case Phase.Grinding:
                    progress += Random.Range(0, 0.03f);
                    grinderState++;
                    if (grinderState > 3)
                    {
                        grinderState = 0;
                    }
                    if (progress >= 1)
                    {
                        PlaySound(SoundID.Slugcat_Throw_Spear);
                        weedGrinder.element = Futile.atlasManager.GetElementWithName("atlases/open_weed_grinder_finished");
                        NextPhase();
                    }
                    else
                    {
                        PlaySound(SoundID.Vulture_Peck);
                        weedGrinder.element = Futile.atlasManager.GetElementWithName($"atlases/weed_grinder{grinderState}");
                    }
                    break;
                case Phase.InGrinderReady:
                    weedLeaves.isVisible = true;
                    weedLeaves.element = Futile.atlasManager.GetElementWithName("atlases/ground_leaves");
                    weedGrinder.element = Futile.atlasManager.GetElementWithName("atlases/open_weed_grinder");
                    NextPhase();
                    break;
                case Phase.Ground:
                    weedLeaves.isVisible = false;
                    bluntWrap.element = Futile.atlasManager.GetElementWithName("atlases/blunt_wrap0");
                    NextPhase();
                    break;                    
                case Phase.Rolling:
                    progress += Random.Range(0, 0.03f);
                    if (progress >= 1)
                    {
                        PlaySound(SoundID.SS_AI_Give_The_Mark_Boom);
                        bluntWrap.element = Futile.atlasManager.GetElementWithName("atlases/finished_blunt");
                        NextPhase();
                    }
                    else
                    {
                        PlaySound(SoundID.Slugcat_Throw_Puffball);
                        var bluntState = Mathf.Floor(progress * 5f);
                        bluntWrap.element = Futile.atlasManager.GetElementWithName($"atlases/blunt_wrap{bluntState}");
                    }
                    break;
                case Phase.Finished:
                    break;
            }
        }
        
        public void NextPhase()
        {
            progress = 0;
            phase++;
        }

        public override void Update()
        {
            base.Update();

            if (RWInput.CheckPauseButton(0, manager.rainWorld))
            {
                Singal(null, "EXIT");
            }
            
            lastBlackFade = blackFade;
            if (wantToExit)
            {
                blackFade = Mathf.Max(0f, blackFade - 0.125f);
                if (blackFade <= 0f)
                {
                    Exit();
                }
            }
            else
            {
                blackFade = Mathf.Min(1f, blackFade + 0.0625f);
            }

            if (mouseDown && !lastMouseDown)
            {
                foreach (var sprite in miniGameSprites)
                {
                    if (!sprite.GetTextureRectRelativeToContainer().Contains(mousePosition))
                    {
                        continue;
                    }

                    switch (phase)
                    {
                        case Phase.InGrinder:
                            if (sprite == weedLeaves) continue;
                            ProgressPhase();
                            break;
                        case Phase.Grinding:
                            if (sprite == weedLeaves) continue;
                            if (sprite == weedGrinder)
                            {
                                ProgressPhase();
                            }
                            break;
                        case Phase.InGrinderReady:
                            if (sprite == weedLeaves) continue;
                            ProgressPhase();
                            break;
                        case Phase.Rolling:
                            if (sprite == weedLeaves) continue;
                            ProgressPhase();
                            break;
                        default:
                            draggedSprite = sprite;
                            draggedOffset = sprite.GetPosition() - mousePosition;
                            break;
                    }

                    break;
                }
            }

            if (mouseDown && draggedSprite != null)
            {
                draggedLastPos = draggedPos;
                draggedPos = mousePosition + draggedOffset;
                if (!lastMouseDown)
                {
                    draggedLastPos = draggedSprite.GetPosition();
                }
            }

            if (!mouseDown && lastMouseDown && draggedSprite != null)
            {
                switch (phase)
                {
                    case Phase.Start:
                        if (draggedSprite == weedLeaves && weedGrinder.GetTextureRectRelativeToContainer().Contains(mousePosition))
                        {
                            ProgressPhase();
                        }
                        break;
                    case Phase.Ground:
                        if (draggedSprite == weedLeaves && bluntWrap.GetTextureRectRelativeToContainer().Contains(mousePosition))
                        {
                            ProgressPhase();
                        }
                        break;
                }

                draggedSprite = null;
            }
        }

        public override void GrafUpdate(float timeStacker)
        {
            var num = Custom.SCurve(Mathf.Lerp(lastBlackFade, blackFade, timeStacker), 0.6f);
            blackSprite.alpha = num * 0.25f;

            draggedSprite?.SetPosition(Vector2.Lerp(draggedLastPos, draggedPos, timeStacker)); 

            base.GrafUpdate(timeStacker);
        }

        public override void Singal(MenuObject sender, string message)
        {
            switch (message)
            {
                case "EXIT":
                    wantToExit = true;
                    break;
            }
        }

        public void Exit()
        {
            manager.StopSideProcess(this);
            game.ContinuePaused();
            game.lastPauseButton = true;
        }
    }
}