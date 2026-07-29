namespace CyberFoodONI;

internal static class ModInfo
{
    internal const string StaticId = "CyberFoodONI";
    internal const string SensoryDiningEffectId = "CyberFoodONI_SensoryDining";

    // ONI stores food energy in calories: 200 kcal is represented as 200,000.
    internal const float TastingCalories = 200_000f;
    internal const float MinimumSuccessfulTastingCalories = TastingCalories * 0.9f;

    internal const float EffectDurationSeconds = 3f * 600f;
    internal const float MoraleBonus = 4f;
}
