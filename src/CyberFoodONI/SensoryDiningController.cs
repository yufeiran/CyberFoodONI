using Klei.AI;

namespace CyberFoodONI;

/// <summary>
/// Bridges a gap in the vanilla ration monitor: food created directly in
/// sandbox can be visible to ClosestEdibleSensor without producing the colony
/// ration event that normally moves a duplicant into the edible-available
/// state.
/// </summary>
public sealed class SensoryDiningController : KMonoBehaviour, ISim1000ms
{
    private string lastDiagnosticState;
    private bool loggedMissingStateMachines;

    public void Sim1000ms(float dt)
    {
        RationMonitor.Instance rationMonitor = gameObject.GetSMI<RationMonitor.Instance>();
        CalorieMonitor.Instance calorieMonitor = gameObject.GetSMI<CalorieMonitor.Instance>();

        if (rationMonitor == null || calorieMonitor == null)
        {
            if (!loggedMissingStateMachines)
            {
                loggedMissingStateMachines = true;
                Debug.LogWarning(
                    $"[{ModInfo.StaticId}] {name}: dining state machines are not available");
            }
            return;
        }

        loggedMissingStateMachines = false;

        Effects effects = GetComponent<Effects>();
        bool hasDiningEffect =
            effects != null && effects.HasEffect(ModInfo.SensoryDiningEffectId);

        Edible edible = rationMonitor.GetEdible();
        Schedulable schedulable = GetComponent<Schedulable>();
        bool isEatTime =
            schedulable != null &&
            schedulable.IsAllowed(Db.Get().ScheduleBlockTypes.Eat);

        string diagnosticState =
            $"effect={hasDiningEffect}, eatTime={isEatTime}, " +
            $"food={(edible == null ? "none" : edible.FoodInfo.Id)}, " +
            $"ration={rationMonitor.GetCurrentState()?.name ?? "none"}, " +
            $"calorie={calorieMonitor.GetCurrentState()?.name ?? "none"}";

        if (diagnosticState != lastDiagnosticState)
        {
            lastDiagnosticState = diagnosticState;
            Debug.Log($"[{ModInfo.StaticId}] {name}: {diagnosticState}");
        }

        if (hasDiningEffect)
        {
            if (!rationMonitor.IsInsideState(
                    rationMonitor.sm.rationsavailable.noediblesavailable))
            {
                rationMonitor.GoTo(
                    rationMonitor.sm.rationsavailable.noediblesavailable);
            }

            if (!calorieMonitor.IsInsideState(calorieMonitor.sm.satisfied))
                calorieMonitor.GoTo(calorieMonitor.sm.satisfied);

            return;
        }

        if (isEatTime)
        {
            if (!calorieMonitor.IsInsideState(calorieMonitor.sm.hungry.normal))
                calorieMonitor.GoTo(calorieMonitor.sm.hungry.normal);
        }
        else if (!calorieMonitor.IsInsideState(calorieMonitor.sm.hungry.working))
        {
            calorieMonitor.GoTo(calorieMonitor.sm.hungry.working);
        }

        if (edible != null &&
            !rationMonitor.IsInsideState(
                rationMonitor.sm.rationsavailable.edibleavailable))
        {
            rationMonitor.GoTo(rationMonitor.sm.rationsavailable.edibleavailable);
            Debug.Log(
                $"[{ModInfo.StaticId}] {name}: requested a sensory dining chore");
        }
    }
}
