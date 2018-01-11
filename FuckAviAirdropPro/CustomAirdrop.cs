using SDG.Unturned;
using UnityEngine;

namespace FuckAviAirdropPro
{
    public class CustomAirdrop : MonoBehaviour
    {
        private bool isExploded;
        public AirdropType type;
        
        private void OnCollisionEnter(Collision collision)
        {
            if (isExploded)
                return;
            
            if (collision.collider.isTrigger)
                return;
            
            isExploded = true;
            
            Transform t = BarricadeManager.dropBarricade(new Barricade(1374), null, base.transform.position, 0f, 0f, 0f, 0UL, 0UL);
            
            if (t == null) 
                return;
            
            InteractableStorage component = transform.GetComponent<InteractableStorage>();
            component.despawnWhenDestroyed = true;
            if (component.items != null)
            {
                foreach (AirdropItem ai in AirdropManager.GetAirdropItems(type))
                    for (int i = 0; i < ai.amount; i++)
                        component.items.tryAddItem(new Item((ushort) ai.id, EItemOrigin.ADMIN, (byte) ai.durability), false);
                
                component.items.onStateUpdated();
            }
            
            EffectManager.sendEffectReliable((ushort)AirdropPlugin.Conf.AirdropEffectId, EffectManager.INSANE, transform.position);
        }
    }
}