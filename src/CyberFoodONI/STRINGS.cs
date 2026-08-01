namespace CyberFoodONI;

public static class STRINGS
{
    public static class UI
    {
        public static class SETTINGS
        {
            public static LocString TITLE = "Cyber Food Settings";

            public static LocString MANAGE_TOOLTIP = "Configure Cyber Food";

            public static LocString OPTIONS_BUTTON = "Options";

            public static LocString DESCRIPTION =
                "Choose which original dining bonuses Bionic Duplicants receive in addition to Synthetic Dining Experience.\n\n" +
                "Original food effects: {0}\n" +
                "Includes food quality morale, spices, garnish and food-specific effects.\n\n" +
                "Dining room effects: {1}\n" +
                "Includes Mess Hall and Great Hall meal effects.\n\n" +
                "Enabled food and dining bonuses last 3 cycles for Bionic Duplicants.\n\n" +
                "Changes apply to future meals and are saved globally.";

            public static LocString FOOD_EFFECTS_BUTTON = "Food Effects: {0}";

            public static LocString DINING_ROOM_BUTTON = "Dining Rooms: {0}";

            public static LocString ENABLED = "ON";

            public static LocString DISABLED = "OFF";

            public static LocString DONE = "Done";
        }
    }

    public static class DUPLICANTS
    {
        public static class MODIFIERS
        {
            public static class CYBERFOODONI_SENSORYDINING
            {
                public static LocString NAME = "Synthetic Dining Experience";

                public static LocString TOOLTIP =
                    "Taste, aroma and texture simulators provide the pleasure of dining without supplying usable energy. " +
                    "Bionic Duplicants insist that this is not wasting food, but high-precision sensory calibration.";
            }
        }
    }
}
