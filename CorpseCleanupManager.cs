using System;
using System.Collections.Generic;
using UnityEngine;

namespace VaynaSocial
{
    /// <summary>
    /// This Land Is My Land - Ceset Temizleme ve Gömme Yöneticisi (VaynaSocial)
    /// Düşman cesetlerinin haritada sonsuza kadar kalmasını engeller.
    /// Kampa yakın cesetleri, içi boş/yağmalanmış cesetleri ve 45 saniyeden eski cesetleri otomatik temizler.
    /// Tüm kalan ganimetleri güvenle oyuncunun en yakın dost kampına teslim eder.
    /// F8 tuşu veya Kamp Ateşinde Dinlenme ile tüm haritadaki cesetleri anında süpürür.
    /// </summary>
    public class CorpseCleanupManager : MonoBehaviour
    {
        public static CorpseCleanupManager Instance { get; private set; }

        private float lastPeriodicCleanup = 0f;
        private const float PeriodicInterval = 15f; // Her 15 saniyede bir kontrol et

        // Cesetlerin spawn olma zamanlarını takip eder (GetInstanceID -> spawn time)
        private static readonly Dictionary<int, float> CorpseSpawnTimes = new Dictionary<int, float>();

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            // F8 tuşu: Manuel olarak anında tüm cesetleri temizle / göm
            if (Input.GetKeyDown(KeyCode.F8))
            {
                int cleaned = CleanupAllDeadBodies(forceAll: true);
                string msg = cleaned > 0 
                    ? $"[VaynaSocial] {cleaned} adet ceset toplanıp gömüldü. Ganimetler kampa aktarıldı." 
                    : "[VaynaSocial] Çevrede temizlenecek ölü beden bulunamadı.";
                
                VaynaSocialPlugin.Log?.LogInfo(msg);
// HUD message suppressed per user preference
            }

            // Periyodik otomatik temizleme
            if (Time.time - lastPeriodicCleanup > PeriodicInterval)
            {
                lastPeriodicCleanup = Time.time;
                CleanupAllDeadBodies(forceAll: false);
            }
        }

        private static List<WorldGroup> GetFriendlyCamps()
        {
            List<WorldGroup> camps = new List<WorldGroup>();
            try
            {
                if (WorldGroup.FriendlyGroups != null && WorldGroup.FriendlyGroups.Count > 0)
                {
                    foreach (var g in WorldGroup.FriendlyGroups)
                    {
                        if (g != null && !camps.Contains(g)) camps.Add(g);
                    }
                }
                if (WorldGroup.AllGroups != null && WorldGroup.AllGroups.Count > 0)
                {
                    Faction playerFaction = (PlayerActions.Instance != null && PlayerActions.Instance.entity != null) 
                        ? PlayerActions.Instance.entity.Faction : null;

                    foreach (var g in WorldGroup.AllGroups)
                    {
                        if (g == null || camps.Contains(g)) continue;
                        if (g.Mode == WorldGroup.EMode.Camp)
                        {
                            if (playerFaction != null && g.Faction == playerFaction)
                            {
                                camps.Add(g);
                            }
                        }
                    }
                }
            }
            catch {}
            return camps;
        }

        /// <summary>
        /// Sahnedeki ve dünyadaki ölü bedenleri temizler.
        /// forceAll = true ise mesafe veya süreye bakılmaksızın tüm cesetleri kaldırır.
        /// forceAll = false ise kamp içindekileri, içi boş olanları veya 45 saniyeden eski olanları kaldırır.
        /// </summary>
        public static int CleanupAllDeadBodies(bool forceAll = false)
        {
            int totalRemoved = 0;
            try
            {
                // CRITICAL SAFETY: Yeni oyun kurma (SandboxFlow), ana menu veya oyun henuz tam baslamamisken ASLA CALISMA!
                if (SandboxFlow.Instance != null || PlayerActions.Instance == null || PlayerActions.Instance.entity == null || WorldState.Instance == null)
                {
                    return 0;
                }

                Vector3 playerPos = Vector3.zero;
                bool hasPlayerPos = false;
                if (PlayerActions.Instance != null && PlayerActions.Instance.cachedTransform != null)
                {
                    playerPos = PlayerActions.Instance.cachedTransform.position;
                    hasPlayerPos = true;
                }

                List<WorldGroup> friendlyCamps = GetFriendlyCamps();

                // 1. PersistentDeadBody (Ganimet taşıyan veya kalıcı ceset nesneleri)
                if (PersistentDeadBody.AllPersistentDeadBodies != null && PersistentDeadBody.AllPersistentDeadBodies.Count > 0)
                {
                    PersistentDeadBody[] corpses = PersistentDeadBody.AllPersistentDeadBodies.ToArray();
                    foreach (var corpse in corpses)
                    {
                        if (corpse == null || corpse.gameObject == null) continue;

                        int id = corpse.GetInstanceID();
                        if (!CorpseSpawnTimes.TryGetValue(id, out float spawnTime))
                        {
                            spawnTime = Time.time;
                            CorpseSpawnTimes[id] = spawnTime;
                        }

                        Vector3 pos = corpse.cachedTransform != null ? corpse.cachedTransform.position : corpse.transform.position;
                        float age = Time.time - spawnTime;
                        bool isLooted = (corpse.AvailableItems == null || corpse.AvailableItems.Count == 0);

                        bool shouldClean = false;
                        if (forceAll)
                        {
                            shouldClean = true;
                        }
                        else if (isLooted)
                        {
                            // Yağmalanmış/içi boş ceset yerde durmasın
                            shouldClean = true;
                        }
                        else if (age >= 45f)
                        {
                            // 45 saniyeden eski cesetler her yerde temizlensin (kalan eşyalar OnDestroy ile kampa uçar)
                            shouldClean = true;
                        }
                        else if (hasPlayerPos && Vector3.Distance(pos, playerPos) >= 35f)
                        {
                            // Oyuncudan 35 metreden uzaktaki cesetler
                            shouldClean = true;
                        }
                        else
                        {
                            // Dost kamp sınırları içinde mi? (150 metre kamp yarıçapı)
                            foreach (var camp in friendlyCamps)
                            {
                                if (camp == null) continue;
                                Vector3 cPos = camp.cachedTransform != null ? camp.cachedTransform.position : camp.Center;
                                if (cPos != Vector3.zero && Vector3.Distance(pos, cPos) <= 150f)
                                {
                                    shouldClean = true;
                                    break;
                                }
                            }
                        }

                        if (shouldClean)
                        {
                            CorpseSpawnTimes.Remove(id);
                            UnityEngine.Object.Destroy(corpse.gameObject);
                            totalRemoved++;
                        }
                    }
                }

                // 2. DeadBody (Ragdoll fizik nesneleri)
                if (DeadBody.AllBodies != null && DeadBody.AllBodies.Count > 0)
                {
                    DeadBody[] bodies = DeadBody.AllBodies.ToArray();
                    foreach (var body in bodies)
                    {
                        if (body == null || body.gameObject == null) continue;

                        int id = body.GetInstanceID();
                        if (!CorpseSpawnTimes.TryGetValue(id, out float spawnTime))
                        {
                            spawnTime = Time.time;
                            CorpseSpawnTimes[id] = spawnTime;
                        }

                        Vector3 pos = body.Center != Vector3.zero ? body.Center : body.transform.position;
                        float age = Time.time - spawnTime;

                        bool shouldClean = false;
                        if (forceAll || age >= 45f)
                        {
                            shouldClean = true;
                        }
                        else if (hasPlayerPos && Vector3.Distance(pos, playerPos) >= 35f)
                        {
                            shouldClean = true;
                        }
                        else
                        {
                            foreach (var camp in friendlyCamps)
                            {
                                if (camp == null) continue;
                                Vector3 cPos = camp.cachedTransform != null ? camp.cachedTransform.position : camp.Center;
                                if (cPos != Vector3.zero && Vector3.Distance(pos, cPos) <= 150f)
                                {
                                    shouldClean = true;
                                    break;
                                }
                            }
                        }

                        if (shouldClean)
                        {
                            CorpseSpawnTimes.Remove(id);
                            UnityEngine.Object.Destroy(body.gameObject);
                            totalRemoved++;
                        }
                    }
                }

                // 3. DraggableBody (Sürüklenebilir cesetler)
                if (DraggableBody.AllDraggables != null && DraggableBody.AllDraggables.Count > 0)
                {
                    DraggableBody[] draggables = DraggableBody.AllDraggables.ToArray();
                    foreach (var d in draggables)
                    {
                        if (d == null || d.gameObject == null) continue;

                        int id = d.GetInstanceID();
                        if (!CorpseSpawnTimes.TryGetValue(id, out float spawnTime))
                        {
                            spawnTime = Time.time;
                            CorpseSpawnTimes[id] = spawnTime;
                        }

                        Vector3 pos = d.Center != Vector3.zero ? d.Center : d.transform.position;
                        float age = Time.time - spawnTime;

                        bool shouldClean = false;
                        if (forceAll || age >= 45f)
                        {
                            shouldClean = true;
                        }
                        else if (hasPlayerPos && Vector3.Distance(pos, playerPos) >= 35f)
                        {
                            shouldClean = true;
                        }
                        else
                        {
                            foreach (var camp in friendlyCamps)
                            {
                                if (camp == null) continue;
                                Vector3 cPos = camp.cachedTransform != null ? camp.cachedTransform.position : camp.Center;
                                if (cPos != Vector3.zero && Vector3.Distance(pos, cPos) <= 150f)
                                {
                                    shouldClean = true;
                                    break;
                                }
                            }
                        }

                        if (shouldClean)
                        {
                            CorpseSpawnTimes.Remove(id);
                            var parentH = d.GetComponentInParent<AINavMeshHumanoid>();
                            if (parentH != null && parentH.gameObject != null)
                            {
                                Entity ent = parentH.GetComponent<Entity>() ?? parentH.GetComponentInParent<Entity>();
                                if (ent != null && ent.IsAlive() && parentH.State != AINavMeshHumanoid.EState.Stunned)
                                {
                                    // Canli ve baygin olmayan insanlari ASLA yok etme!
                                    continue;
                                }
                                UnityEngine.Object.Destroy(parentH.gameObject);
                            }
                            else
                            {
                                UnityEngine.Object.Destroy(d.gameObject);
                            }
                            totalRemoved++;
                        }
                    }
                }

                // 4. Stunned / Unconscious & Dead Hostile Entities (Baygın veya Ölü Düşmanlar)
                if (Entity.AllEntities != null && Entity.AllEntities.Count > 0)
                {
                    Entity playerEnt = (PlayerActions.Instance != null) ? PlayerActions.Instance.entity : null;
                    Faction playerFaction = (playerEnt != null) ? playerEnt.Faction : null;

                    Entity[] entities = Entity.AllEntities.ToArray();
                    foreach (var ent in entities)
                    {
                        if (ent == null || ent.gameObject == null || ent == playerEnt) continue;

                        bool isEnemy = false;
                        if (playerFaction != null && ent.Faction != null)
                        {
                            if (playerFaction != ent.Faction)
                            {
                                if (playerFaction.EnemyFactions != null && ArrayExtensions.Has(playerFaction.EnemyFactions, ent.Faction))
                                    isEnemy = true;
                                else if (ent.Faction.EnemyFactions != null && ArrayExtensions.Has(ent.Faction.EnemyFactions, playerFaction))
                                    isEnemy = true;
                                else
                                    isEnemy = true;
                            }
                        }

                        if (!isEnemy) continue;

                        var h = ent.GetComponent<AINavMeshHumanoid>();
                        bool isDead = !ent.IsAlive();
                        bool isStunned = (h != null && (h.State == AINavMeshHumanoid.EState.Stunned || h.LeaveStunModeAt > Time.time || h.StunnedDraggableBody != null));

                        if (!isDead && !isStunned) continue;

                        Vector3 entPos = ent.cachedTransform != null ? ent.cachedTransform.position : ent.transform.position;
                        bool nearCamp = false;
                        WorldGroup targetCamp = null;
                        foreach (var camp in friendlyCamps)
                        {
                            if (camp == null) continue;
                            Vector3 cPos = camp.cachedTransform != null ? camp.cachedTransform.position : camp.Center;
                            if (cPos != Vector3.zero && (Vector3.Distance(entPos, cPos) <= 150f || camp.ContainsPoint(entPos, 1.5f)))
                            {
                                nearCamp = true;
                                targetCamp = camp;
                                break;
                            }
                        }

                        bool nearPlayer = hasPlayerPos && Vector3.Distance(entPos, playerPos) <= 100f;

                        if (forceAll || nearCamp || (isDead && nearPlayer))
                        {
                            if (targetCamp != null)
                            {
                                try
                                {
                                    List<WorldGroup.ItemAmountDescription> owned = new List<WorldGroup.ItemAmountDescription>();
                                    ent.AddOwnedItemsToList(owned, true);
                                    if (owned.Count > 0)
                                    {
                                        targetCamp.AddItems(owned);
                                    }
                                }
                                catch {}
                            }

                            UnityEngine.Object.Destroy(ent.gameObject);
                            totalRemoved++;
                        }
                    }
                }

                // 4. WorldState üzerindeki kayıtlı ceset listesini temizle (Save dosyasına ölü cesetlerin yazılmasını engeller)
                if (WorldState.Instance != null && WorldState.Instance.PersistentDeadBodies != null)
                {
                    if (forceAll || WorldState.Instance.PersistentDeadBodies.Count > 0)
                    {
                        WorldState.Instance.PersistentDeadBodies.Clear();
                    }
                }

                if (totalRemoved > 0)
                {
                    VaynaSocialPlugin.Log?.LogInfo($"[CorpseCleanup] Toplam {totalRemoved} adet ölü beden başarıyla kaldırıldı/gömüldü (forceAll={forceAll}).");
                }
            }
            catch (Exception ex)
            {
                VaynaSocialPlugin.Log?.LogWarning("CleanupAllDeadBodies hatası: " + ex.Message);
            }
            return totalRemoved;
        }

        // Geriye dönük uyumluluk için eski metodlar
        public static int CleanupCampCorpses()
        {
            return CleanupAllDeadBodies(forceAll: false);
        }

        public static int CleanupDistantCorpses(float minDistance = 45f)
        {
            return CleanupAllDeadBodies(forceAll: false);
        }
    }
}
