using System.Collections.Generic;
using Rocket.API;

namespace FuckAviAirdropPro
{
    public class ItemBatch
    {
        public int weight;
        public List<AirdropItem> items;
    }
    
    public class AirdropItem
    {
        public int id;
        public int amount;
        public int durability;
    }

    public class RandomItem
    {
        public int weight;
        public int id;
        public int amount;
        public int durability;
    }
    
    public class Vehicle
    {
        public int id;
        public int weight;
    }

    public class AirdropConfig : IRocketPluginConfiguration
    {
        public int NamedLocationsWeight;
        public int AirdropNodeWeight;
        public int RandomLocationWeight;
        
        public bool AllowDropInWater;
        public bool NoScheduledAirdrops;
        
        public int MinIntervalSec;
        public int MaxIntervalSec;

        public int MinPlayers;
        
        public bool BroadcastAirdropLandedDetails;
        public string BroadcastAirdropLandedDetailsColor;

        public bool BroadcastAirdrop;
        public string BroadcastAirdropColor;
        
        public bool BroadcastVehicleAirdrop;
        public string BroadcastVehicleAirdropColor;

        public int VanillaDropWeight;
        public int CustomDropWeight;
        public int RandomDropWeight;
        public int VehicleDropWeight;

        public List<ItemBatch> CustomItemBatches;
        public List<RandomItem> RandomItemBatches;
        public List<Vehicle> AirdropVehicles;

        public bool ExplodeItems;
        public float MinExplodeRange;
        public float MaxExplodeRange;

        public int MinRandomItems;
        public int MaxRandomItems;

        public int AirdropEffectId;
        public int VehicleDropEffectId;
        
        public void LoadDefaults()
        {
            NamedLocationsWeight = 1;
            AirdropNodeWeight = 1;
            RandomLocationWeight = 1;
            AllowDropInWater = true;
            NoScheduledAirdrops = true;
            MinIntervalSec = 10;
            MaxIntervalSec = 30;
            MinPlayers = 3;
            BroadcastAirdropLandedDetails = false;
            BroadcastAirdropLandedDetailsColor = "red";
            BroadcastAirdrop = false;
            BroadcastAirdropColor = "red";
            BroadcastVehicleAirdrop = false;
            BroadcastVehicleAirdropColor = "red";
            VanillaDropWeight = 1;
            CustomDropWeight = 1;
            RandomDropWeight = 1;
            VehicleDropWeight = 1;
            CustomItemBatches = new List<ItemBatch>()
            {
                new ItemBatch()
                {
                    weight = 1,
                    items = new List<AirdropItem>()
                    {
                        new AirdropItem()
                        {
                            amount =  4,
                            durability = 100,
                            id = 420
                        }   
                    }
                }
            };
            RandomItemBatches = new List<RandomItem>();
            AirdropVehicles = new List<Vehicle>();
            ExplodeItems = false;
            MinExplodeRange = 1;
            MaxExplodeRange = 1;
            MinRandomItems = 1;
            MaxRandomItems = 1;
            AirdropEffectId = 1;
            VehicleDropEffectId = 1;
        }
    }
}