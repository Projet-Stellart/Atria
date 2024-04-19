public partial class vortex : player
{
    //Class Data
    private static new int energyBar = 200;
    protected static Soldier _soldier = new Soldier(
        "Vortex",
        "Desc of Vortex",
        energyBar,
        new ModuleInfo(
            "Force Field",
            "Deploys an energy shield in front of the player that absorbs incoming projectiles and produces energy from the radioactive ones.",
            "FIRE to charge an energy field in front of the player that protects from incoming projectiles and converts radioactivity projectiles to energy. HOLD MODULE {A} to keep the field standing until it runs out of energy.",
            0
        ),
        new ModuleInfo(
            "Warp",
            "Creates a zone where gravity is modified in the desired direction, altering any players and projectiles in its radius.",
            "PRESS MODULE {C} to instantly place a gravity warp zone altering the gravity of players and projectiles within it. HOLD FIRE to spawn further away. HOLD ALT FIRE to spawn closer. HOLD {R} to rotate the gravitational direction.",
            0
        ),
        new ModuleInfo(
            "Pulsar",
            "Emits a shockwave that pushes all nearby enemies away from the point of impact, dealing high amount of damage and knocking ennemies down.",
            "Launch a projectile, producing a shockwave on impact. FIRE to shoot the projectile. ALT FIRE to throw it by hands.",
            0
        ),
        new ModuleInfo(
            "Supernova",
            "Produces a strong blast affecting any entities depending on the range distance. Effects varies from module deffect to instant death.",
            "FIRE to create a blast at your feet, dealing different effects to players based on the distance, from instant death to module deffect.",
            energyBar
        )
    );
    public override Soldier soldier => _soldier;

    public override void _lowModule()
    {
        
    }

    public override void _mediumModule()
    {

    }

    public override void _highModule()
    {

    }

    public override void _coreModule() 
    {
        
    }
}
