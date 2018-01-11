using Rocket.API.Collections;
using Rocket.Core.Plugins;

namespace FuckAviAirdropPro
{
    public class AirdropPlugin : RocketPlugin<AirdropConfig>
    {
        public static AirdropPlugin Instance;
        public static AirdropConfig Conf;
        public AirdropManager ManagerInstance;
        
        protected override void Load()
        {
            Instance = this;
            Conf = Configuration.Instance;

            ManagerInstance = gameObject.AddComponent<AirdropManager>();
        }

        protected override void Unload()
        {
           Destroy(ManagerInstance);
        }
        
        public override TranslationList DefaultTranslations => new TranslationList()
        {
            {"airdrop_land", "The airdrop has landed at {0}!"},
            {"vehicle_land", "The vehicle has landed at {0}!"},
            
            {"airdrop_launch", "An airdrop has been launched at {0}!"},
            {"random_airdrop_launch", "An airdrop has been launched at a random location!"},
            {"vehicle_launch", "A vehicle has been launched at {0}!"},
            {"random_vehicle_launch", "A vehiclehas been launched at a random location!"}
        };
    }
}