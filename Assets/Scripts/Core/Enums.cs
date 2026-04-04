// =============================================================================
// Enums.cs  |  Scripts/Core
// WaifuGarden — Phase 0
// All shared enumerations. Every enum used by more than one system lives here.
// =============================================================================

/// <summary>The type of passive bonus a discovered character provides.</summary>
public enum PassiveBonusType
{
    GrowthSpeed,     // Multiplies all plant growth speed by (1 + value)
    MutationChance,  // Increases per-tick modifier probability (additive)
    SellValue        // Multiplies all harvest sell value by (1 + value)
}

/// <summary>The three growth stages of a planted crop.</summary>
public enum GrowthStage
{
    Seed,
    Sprout,
    Mature
}

/// <summary>Visual and functional state of a single garden grid slot.</summary>
public enum SlotState
{
    Empty,             // Bare ground — no farm plot
    FarmPlot_Empty,    // Farm plot placed, nothing planted
    FarmPlot_Growing,  // Plant actively growing (Seed or Sprout)
    FarmPlot_Ready,    // Plant at Mature stage, ready to harvest
    FarmPlot_Glowing   // Plant at Mature with a pending evolution
}

/// <summary>Category of a purchasable shop item.</summary>
public enum ShopItemType
{
    Seed,
    Tool,
    FarmPlot
}

/// <summary>Sub-type of a tool. Controls hotbar interaction logic.</summary>
public enum ToolType
{
    Shovel,
    WateringCan,
    Fertilizer
}

/// <summary>All discrete steps of the tutorial sequence.</summary>
public enum TutorialStep
{
    Idle,
    OpenShop,
    BuyStrangeSeed,
    CloseShop,
    PlantSeed,
    BuyFertilizer,
    ApplyFertilizer,
    BuyWateringCan,
    ApplyWateringCan,
    WaitForHeatwave,
    WaitForEvolution,
    ClickGlowingPlant,
    WatchEvolutionScreen,
    MeetSunflowerGirl,
    Complete
}
