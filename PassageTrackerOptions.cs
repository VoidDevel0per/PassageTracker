using System.Numerics;
using UnityEngine;
using Menu.Remix.MixedUI;
using Vector2 = UnityEngine.Vector2;

namespace PassageTrackerMod;

public class PassageTrackerOptions : OptionInterface
{    
    public override void Initialize()
    {
        base.Initialize();
        Tabs = new OpTab[]
        {
            new OpTab(this, "Options")
        };

        Vector2 vector = new Vector2(25f, 600f);
        vector.y -= 30f;

        //Passage sound
        OpCheckBox opCheckBox = new OpCheckBox(playPassageSound, vector)
        {
            description = playPassageSound.info.description
        };
        OpLabel opLabel = new OpLabel(vector.x + 30f, vector.y + 3f, playPassageSound.info.Tags[0] as string, false)
        {
            description = playPassageSound.info.description
        };
        Tabs[0].AddItems(new UIelement[]
        {
            opCheckBox,
            opLabel
        });


        vector.y -= 30f;

        //Failure sound
        opCheckBox = new OpCheckBox(playFailSound, vector)
        {
            description = playFailSound.info.description
        };
        opLabel = new OpLabel(vector.x + 30f, vector.y + 3f, playFailSound.info.Tags[0] as string, false)
        {
            description = playFailSound.info.description
        };
        Tabs[0].AddItems(new UIelement[]
        {
            opCheckBox,
            opLabel
        });


        vector.y -= 40f;

        //Display time
        OpUpdown opUpdown = new OpUpdown(passageDisplayTime, vector, 70f, 1)
        {
            description = passageDisplayTime.info.description
        };
        opLabel = new OpLabel(vector.x + 80f, vector.y + 3f, passageDisplayTime.info.Tags[0] as string, false)
        {
            description = passageDisplayTime.info.description
        };
        Tabs[0].AddItems(new UIelement[]
        {
            opUpdown,
            opLabel
        });


        //vector.y -= 40f;

        //OpRadioButtonGroup opRadioButtonGroup = new OpRadioButtonGroup(passageDisplayTime)

    }

    public static PassageTrackerOptions instance = new();

    public static Configurable<bool> playPassageSound = instance.config.Bind<bool>("playPassageSound", true, new ConfigurableInfo("Play a sound upon gaining progress in certain passages.", null, "", new object[]
    {
        "Passage Sound"
    }));

    public static Configurable<bool> playFailSound = instance.config.Bind<bool>("playFailSound", true, new ConfigurableInfo("Play a sound upon failing certain passages.", null, "", new object[]
    {
        "Failure Sound"
    }));

    public static Configurable<float> passageDisplayTime = instance.config.Bind<float>("passageDisplayTime", 5f, new ConfigurableInfo("How long a passage will be displayed upon gaining progress.", new ConfigAcceptableRange<float>(1f, 20f), "", new object[]
    {
        "Passage display time in seconds"
    }));
}



