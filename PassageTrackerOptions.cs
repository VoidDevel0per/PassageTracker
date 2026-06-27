using UnityEngine;
using Menu.Remix.MixedUI;

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

        // Passage sound
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

        // Failure sound
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

        // Display time
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


        vector.y -= 40f;

        // Show map tracker
        OpCheckBox opCheckBox3 = new OpCheckBox(showMapTracker, vector)
        {
            description = showMapTracker.info.description
        };
        opLabel = new OpLabel(vector.x + 30f, vector.y + 3f, showMapTracker.info.Tags[0] as string, false)
        {
            description = showMapTracker.info.description
        };
        Tabs[0].AddItems(new UIelement[]
        {
            opCheckBox3,
            opLabel
        });

        vector.y -= 30f;

        // Show HUD popups
        OpCheckBox opCheckBox4 = new OpCheckBox(showHudDisplay, vector)
        {
            description = showHudDisplay.info.description
        };
        opLabel = new OpLabel(vector.x + 30f, vector.y + 3f, showHudDisplay.info.Tags[0] as string, false)
        {
            description = showHudDisplay.info.description
        };
        Tabs[0].AddItems(new UIelement[]
        {
            opCheckBox4,
            opLabel
        });
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

    public static Configurable<bool> showMapTracker = instance.config.Bind<bool>("showMapTracker", true, new ConfigurableInfo("Show passage tracker icons on the in-game map.", null, "", new object[]
    {
        "Show on Map"
    }));

    public static Configurable<bool> showHudDisplay = instance.config.Bind<bool>("showHudDisplay", true, new ConfigurableInfo("Show passage progress/fail popups on the HUD.", null, "", new object[]
    {
        "Show HUD Popups"
    }));
}



