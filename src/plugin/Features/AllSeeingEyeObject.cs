using UnityEngine;

namespace InkyJinkies;

public class AllSeeingEyeData : Pom.Pom.ManagedData
{
    [Pom.Pom.BooleanField("Eye Has Light", false)]
    public bool EyeHasLight;
    
    [Pom.Pom.ColorField("Eye Color", 1, 1, 1)]
    public Color EyeColor;
    
    [Pom.Pom.ColorField("Eye Light Color", 1, 1, 1)]
    public Color EyeLightColor;
    
    [Pom.Pom.FloatField("Eye Light Lightness", 0, 1, 0.7f)]
    public float EyeLightLightness;

    [Pom.Pom.FloatField("Max Eye Radius", 1, 50, 0.5f)]
    public float EyeRadius;
    
    [Pom.Pom.IntegerField("Eye Light Radius", 1, 450, 50)]
    public int EyeLightRadius;

    [Pom.Pom.FloatField("Max Pupil Size", 0, 2, 0.5f)]
    public float PupilSize;

    public AllSeeingEyeData(PlacedObject owner) : base(owner, null)
    {
    }
}

public class AllSeeingEye : UpdatableAndDeletable, IDrawable
{
    private Player selectedPlayer;

    private LightSource light;
    
    private PlacedObject eye;
    private bool hasLight;
    private Color eyeColor;
    private Color lightColor;
    private float lightLightness;
    private float eyeRadius;
    private float eyeLightRadius;
    private float pupilSize;

    public AllSeeingEye(PlacedObject eye, Room room)
    {
        this.eye = eye;
        AllSeeingEyeData data = (AllSeeingEyeData)eye.data;
        this.hasLight = data.EyeHasLight;
        this.eyeColor = data.EyeColor;
        this.lightColor = data.EyeLightColor;
        this.lightLightness = data.EyeLightLightness;
        this.eyeRadius = data.EyeRadius;
        this.eyeLightRadius = data.EyeLightRadius;
        this.pupilSize = data.PupilSize;
        this.room = room;

        if (!hasLight) return;

        light = new LightSource(eye.pos, false, lightColor, this);
        light.HardSetRad(eyeLightRadius);
        light.HardSetAlpha(lightLightness);

        room.AddObject(light);
    }
    
    // :3
    public static void Apply()
    {
        Pom.Pom.RegisterManagedObject<AllSeeingEye, AllSeeingEyeData, Pom.Pom.ManagedRepresentation>("All Seeing Eye", "Inky Jinkies");
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Circle4")
        {
            color = eyeColor,
            scale = pupilSize
        };
        
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Bloom"));
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (room.PlayersInRoom.Count == 0) return;

        selectedPlayer ??= room.PlayersInRoom[Random.Range(0, room.PlayersInRoom.Count)];

        Vector2 clampedEyePos = eye.pos + Vector2.ClampMagnitude(selectedPlayer.mainBodyChunk.pos - eye.pos, eyeRadius);
        
        sLeaser.sprites[0].SetPosition(clampedEyePos - camPos);;

        if (hasLight)
        {
            light.HardSetPos(clampedEyePos - camPos);
        }
        
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
        newContatiner ??= rCam.ReturnFContainer("Midground");
        foreach (FSprite sprite in sLeaser.sprites)
        {
            sprite.RemoveFromContainer();
            newContatiner.AddChild(sprite);
        }
    }
}