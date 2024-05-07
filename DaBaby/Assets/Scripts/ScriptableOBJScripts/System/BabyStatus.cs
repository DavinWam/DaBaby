using UnityEngine;

[CreateAssetMenu(fileName = "NewStatus", menuName = "Game/Status")]
public class BabyStatus : ScriptableObject
{
    public float hunger = 100f;
    public float happiness = 100f;
    public float energy = 100f;
    public float overallStatus = 100f;  // New overall status field

    public float hungerDecayRate = 0.01f;
    public float happinessDecayRate = 0.01f;
    public float energyDecayRate = 0.01f;

    public bool shouldDecay = true;

    public void UpdateStatus()
    {
        if (shouldDecay)
        {
            hunger -= hungerDecayRate * Time.deltaTime;
            happiness -= happinessDecayRate * Time.deltaTime;
            energy -= energyDecayRate * Time.deltaTime;

            hunger = Mathf.Clamp(hunger, 0, 100);
            happiness = Mathf.Clamp(happiness, 0, 100);
            energy = Mathf.Clamp(energy, 0, 100);

            UpdateOverallStatus();  // Update the overall status based on other factors
        } //needs to handle being feed to entertained
    }

    private void UpdateOverallStatus()
    {
        // Simple average of the three statuses for now
        overallStatus = (hunger + happiness + energy) / 3;
    }
}
