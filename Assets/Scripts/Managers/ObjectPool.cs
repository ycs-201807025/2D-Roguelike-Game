using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간단한 오브젝트 풀링 시스템
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size = 10;
    }

    [Header("Pools")]
    [SerializeField] private List<Pool> pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    /// <summary>
    /// 풀에서 오브젝트 가져오기
    /// </summary>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    /// <summary>
    /// 오브젝트를 풀로 반환
    /// </summary>
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}
