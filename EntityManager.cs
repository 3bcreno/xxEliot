using EnsoulSharp;
using System.Collections.Generic;
using System.Linq;

namespace MrMundo
{
    internal class EntityManager
    {
        public static class Heroes
        {
            internal static List<AIHeroClient> _enemies = new List<AIHeroClient>();
            public static List<AIHeroClient> Enemies
            {
                get { return new List<AIHeroClient>(_enemies); }
            }
        }
        public static class MinionsAndMonsters
        {
            public static IEnumerable<AIMinionClient> Minions
            {
                get
                {
                    return
                        ObjectManager.Get<AIMinionClient>()
                            .Where(o => o.IsValid && o.IsMinion && (o.Team == GameObjectTeam.Chaos || o.Team == GameObjectTeam.Order) && o.MaxHealth > 6)
                            .ToArray();
                }
            }
            public static IEnumerable<AIMinionClient> EnemyMinions
            {
                get { return Minions.Where(o => o.IsEnemy).ToArray(); }
            }
        }
        }
}