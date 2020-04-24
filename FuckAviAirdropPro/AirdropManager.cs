using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Rocket.Unturned.Chat;
using SDG.Framework.Water;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Random = System.Random;

namespace FuckAviAirdropPro
{
    public enum AirdropType
    {
        Normal,
        Random,
        Custom,
        Vehicle
    }

    public enum LocationType
    {
        Normal,
        Node,
        Random
    }
    
    public class CustomAirdropInfo
    {
        public AirdropType Type;
        
        public Vector3 State;
        public Vector3 FinalPos;
        public Vector3 Direction;
        
        public float Speed;
        public float Delay;
        public float Force;
    }
    
    public class AirdropManager : MonoBehaviour
    {
        private Timer AirdropTimer;
        private static Random Rand = new Random();
        private static AirdropConfig cf;
        private static List<CustomAirdropInfo> Airdrops = new List<CustomAirdropInfo>();
        
        private void Start()
        {
            cf = AirdropPlugin.Conf;
            
            AirdropTimer = new Timer(Rand.Next(cf.MinIntervalSec, cf.MaxIntervalSec + 1));
            AirdropTimer.Elapsed += OnAirdrop;
        }

        private void OnDestroy()
        {
            AirdropTimer.Dispose();
            AirdropTimer.Elapsed -= OnAirdrop;
        }

        private void Update()
        {
            List<CustomAirdropInfo> remQueue = new List<CustomAirdropInfo>();
            foreach (CustomAirdropInfo ai in Airdrops)
            {
                ai.State += ai.Direction * ai.Speed * Time.deltaTime;
                ai.Delay -= Time.deltaTime;
                
                if (ai.Delay > 0)
                    continue;
                

                if (ai.Type != AirdropType.Vehicle)
                {
                    //TODO: Test against newer Unturned
                    
                    Transform t = Instantiate(new MasterBundleReference<GameObject>("core.masterbundle", "Level/Dropship.prefab").loadAsset(true)).transform;

                    t.name = "Dropship";
                    t.parent = Level.effects;
                    t.position = ai.State;
                    t.rotation = Quaternion.identity;
                    t.GetComponent<ConstantForce>().force = new Vector3(0f, ai.Force, 0f);
                    t.gameObject.AddComponent<CustomAirdrop>().type = ai.Type;
                }
                else
                {
                    VehicleManager.spawnVehicle(GetVehicle(ai.FinalPos), ai.State, Quaternion.identity);
                    EffectManager.sendEffectReliable((ushort)cf.VehicleDropEffectId, EffectManager.INSANE, transform.position);
                }
                
                remQueue.Add(ai);
            }

            foreach (CustomAirdropInfo ai in remQueue)
                Airdrops.Remove(ai);
        }
        
        private void OnAirdrop(object o, ElapsedEventArgs e)
        {
            AirdropTimer.Interval =
                Rand.Next(cf.MinIntervalSec, cf.MaxIntervalSec + 1);

            if (Provider.clients.Count < cf.MinPlayers)
                return;
            
            int max = cf.VanillaDropWeight + cf.CustomDropWeight + cf.RandomDropWeight + cf.VehicleDropWeight;
            int rnd = Rand.Next(0, max);

            AirdropType typ = GetAirdropType(rnd);

            Vector3 point = GetAirdropPos(typ);
            Vector3 vector = Vector3.zero;
            float speed = Provider.modeConfigData.Events.Airdrop_Speed;
            
            if (UnityEngine.Random.value < 0.5f)
            {
                vector.x = (Level.size / 2) * -Mathf.Sign(point.x);
                vector.z = UnityEngine.Random.Range(0, (int)(Level.size / 2)) * -Mathf.Sign(point.z);
            }
            else
            {
                vector.x = UnityEngine.Random.Range(0, (int)(Level.size / 2)) * -Mathf.Sign(point.x);
                vector.z = (Level.size / 2) * -Mathf.Sign(point.z);
            }
            point.y = 0f;
            
            Vector3 normalized = (point - vector).normalized;
            vector += normalized * -2048f;
            
            float delay = (point - vector).magnitude / speed;
            
            vector.y = 1024f;
            float force = Provider.modeConfigData.Events.Airdrop_Force;


            CustomAirdropInfo airdropInfo = new CustomAirdropInfo
            {
                State = vector,
                Direction = normalized,
                Speed = speed,
                Force = force,
                Delay = delay,
                FinalPos = point
            };

            Airdrops.Add(airdropInfo);
            LevelManager.instance.channel.send("tellAirdropState", ESteamCall.OTHERS, ESteamPacket.UPDATE_RELIABLE_BUFFER, vector, normalized, speed, force, delay);
        }

        public static List<AirdropItem> GetAirdropItems(AirdropType type)
        {
            List<AirdropItem> finItems = new List<AirdropItem>();
            
            switch (type)
            {
                case AirdropType.Normal:
                {
                    for (int i = 0; i < 8; i++)
                    {
                        ushort num = SpawnTableTool.resolve(1374);
                        
                        if (num == 0)
                            break;
                        
                        finItems.Add(new AirdropItem()
                        {
                            amount = 1,
                            durability = 100,
                            id = num
                        });
                    }
                    break;
                }

                case AirdropType.Custom:
                {
                    int max = cf.CustomItemBatches.Sum(b => b.weight); 
                    int rnd = Rand.Next(0, max);
                    
                    ItemBatch finalBatch = null;
                    foreach (ItemBatch b in cf.CustomItemBatches)
                    {
                        if (rnd < b.weight)
                        {
                            finalBatch = b;
                            break;
                        }

                        rnd -= b.weight;
                    }

                    finItems = finalBatch.items;
                    break;
                }

                case AirdropType.Random:
                {
                    int max = cf.RandomItemBatches.Sum(b => b.weight);

                    for (int i = cf.MinRandomItems; i <= cf.MaxRandomItems; i++)
                    {
                        int rnd = Rand.Next(0, max);
                        foreach (RandomItem r in cf.RandomItemBatches)
                        {
                            if (rnd < r.weight)
                            {
                                finItems.Add(new AirdropItem()
                                {
                                    amount = r.amount,
                                    durability = r.durability,
                                    id = r.id
                                });
                                break;
                            }

                            rnd -= r.weight;
                        }
                    }

                    break;
                }
                    
                case AirdropType.Vehicle:
                    finItems = null;
                    break;
            }

            return finItems;
        }

        public static Vector3 GetAirdropPos(AirdropType type)
        {
            LocationType locType;

            int max = cf.AirdropNodeWeight + cf.NamedLocationsWeight + cf.RandomLocationWeight;
            int weight = Rand.Next(0, max);

            if (weight < cf.AirdropNodeWeight)
                locType = LocationType.Normal;

            weight -= cf.AirdropNodeWeight;
            if (weight < cf.NamedLocationsWeight)
                locType = LocationType.Node;

            locType = LocationType.Random;
            Vector3 locPos = Vector3.zero;
            string locName;
            
            switch (locType)
            {
                case LocationType.Normal:
                {
                    Node node = LevelNodes.nodes.Where(n => n.type == ENodeType.AIRDROP).OrderBy(n => Rand.Next())
                        .First();
                    
                    locPos = node.point;
                    locName = null;
                    
                    break;
                }
                case LocationType.Node:
                {
                    Node node = LevelNodes.nodes.Where(n => n.type == ENodeType.LOCATION).OrderBy(n => Rand.Next())
                        .First();

                    locPos = node.point;
                    locName = ((LocationNode) node).name;
                    
                    break;
                }
                case LocationType.Random:
                {
                    int maxS = Level.size / 2;
                    int minS = -Level.size / 2;

                    int x = Rand.Next(minS, maxS);
                    int z = Rand.Next(minS, maxS);
                    
                    Vector3 vec = new Vector3(x, 2048, z);
                    Physics.Raycast(vec, Vector3.down, out RaycastHit hit, 4096, RayMasks.GROUND);

                    locPos = hit.point;
                    
                    break;
                }
            }

            return locPos;
        }

        public static ushort GetVehicle(Vector3 airdropPos)
        {
            int max = cf.AirdropVehicles.Sum(v => v.weight);
            int rnd = Rand.Next(0, max);
            foreach (Vehicle v in cf.AirdropVehicles)
            {
                
                VehicleAsset va = (VehicleAsset)Assets.find(EAssetType.VEHICLE, (ushort)v.id);
                if (va.engine == EEngine.BOAT) //TODO: Find a better replacement, temporary fix
                    if (WaterUtility.isPointUnderwater(airdropPos) && !cf.AllowDropInWater)
                        continue;
                
                if (rnd < v.weight)
                    return (ushort)v.id;

                rnd -= v.weight;
            }

            return 0;
        }
        
        public static AirdropType GetAirdropType(int weight)
        {
            if (weight < cf.VanillaDropWeight)
                return AirdropType.Normal;
            weight -= cf.VanillaDropWeight;
            
            if (weight < cf.RandomDropWeight)
                return AirdropType.Random;
            weight -= cf.RandomDropWeight;
            
            if (weight < cf.VehicleDropWeight)
                return AirdropType.Vehicle;
            weight -= cf.VehicleDropWeight;
            
            return AirdropType.Custom;
        }
    }
}