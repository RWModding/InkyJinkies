using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InkyJinkies
{
    public class LandmineData : Pom.Pom.ManagedData
    {
        [Pom.Pom.BooleanField("Hi :3", false)]
        public bool hi;

        public LandmineData(PlacedObject owner) : base(owner, null)
        {
        }
    }

    public class Landmine : UpdatableAndDeletable, IDrawable
    {
        bool lit;
        int litTime;
        const int detTime = 40;
        static int crashCounter = 0;
        const int crashReq = 4;

        private PlacedObject pObj;
        private LightSource light;

        public Landmine(PlacedObject pObj, Room room)
        {
            this.pObj = pObj;
            this.room = room;

            light = new LightSource(pObj.pos, false, Color.red, this);
            light.HardSetRad(200);
            light.HardSetAlpha(0);
            light.flat = true;

            room.AddObject(light);
        }

        // :33
        public static void Apply()
        {
            Futile.atlasManager.LoadImage("atlases/Mine");
            Pom.Pom.RegisterManagedObject<Landmine, LandmineData, Pom.Pom.ManagedRepresentation>("Landmine", "Inky Jinkies");
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("atlases/Mine");

            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].SetPosition(pObj.pos - camPos);

            light.HardSetPos(pObj.pos - camPos + new Vector2(0, 5));

            light.HardSetAlpha((litTime % 5) / 5f);

            if (!sLeaser.deleteMeNextFrame && (slatedForDeletetion || room != rCam.room))
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {

        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            foreach (FSprite sprite in sLeaser.sprites)
            {
                sprite.RemoveFromContainer();
                newContatiner.AddChild(sprite);
            }
        }

        public override void Update(bool eu)
        {
            if (room.PlayersInRoom.Count == 0) return;

            if (!lit)
                foreach (Player p in room.PlayersInRoom)
                {
                    //Debug.Log($":33 < You're {Vector2.Distance(p.firstChunk.pos, this.pObj.pos)} units away!");
                    if (Vector2.Distance(p.firstChunk.pos, this.pObj.pos) < 40)
                    {
                        //Debug.Log(":33 < oopsies!");
                        lit = true;
                    }
                }
            else
            {
                litTime++;
                //Debug.Log($":33 < *pouncing in {detTime - litTime} ticks! >w<*");


                if (litTime == detTime)
                {
                    Debug.Log(":33 < *explodes your slugcat to smitherenes*");

                    this.room.AddObject(new SootMark(this.room, this.pObj.pos, 80f, true));
                    this.room.AddObject(new Explosion(this.room, null, this.pObj.pos, 7, 250f, 6.2f, 2f, 280f, 0.25f, null, 0.7f, 160f, 1f));
                    this.room.AddObject(new Explosion.ExplosionLight(this.pObj.pos, 280f, 1f, 7, new Color(1f, 0.4f, 0.3f)));
                    this.room.AddObject(new Explosion.ExplosionLight(this.pObj.pos, 230f, 1f, 3, new Color(1f, 1f, 1f)));
                    this.room.AddObject(new ExplosionSpikes(this.room, this.pObj.pos, 14, 30f, 9f, 7f, 170f, new Color(1f, 0.4f, 0.3f)));
                    this.room.AddObject(new ShockWave(this.pObj.pos, 330f, 0.045f, 5, false));
                    this.room.PlaySound(SoundID.Bomb_Explode, this.pObj.pos);

                    crashCounter++;
                    if (crashCounter != crashReq)
                        Destroy();
                }

                if (crashCounter == crashReq && litTime == detTime + 5)
                {
                    Debug.Log(":33 < *crashes your game* X33");
                    Crash();
                }

                if(litTime % 5 == 4)
                {
                    room.PlaySound(SoundID.Vulture_Grub_Red_Blink, pObj.pos, 1.1f, 0.8f);
                }
            }
        }

        public void Crash() => Crash();
    }
}
