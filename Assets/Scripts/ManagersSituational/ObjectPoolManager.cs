using System.Collections.Generic;
using UnityEngine;

namespace CaptainHindsight
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance;

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject gameObjectPrefab;
            public int size;
        }

        public List<Pool> pools;
        public Dictionary<string, Queue<GameObject>> poolDictionary;
        [SerializeField] private GameObject objectPoolParent;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.gameObjectPrefab);
                    obj.SetActive(false);
                    obj.transform.parent = objectPoolParent.transform.parent;
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        public GameObject SpawnFromPool(string tag, Vector2 position, Quaternion rotation)
        {
            if (poolDictionary.ContainsKey(tag) == false)
            {
                Helper.LogWarning("ObjectPoolManager: Couldn't find objects in a pool with tag '" + tag + "'.");
                return null;
            }

            GameObject objectToSpawn = poolDictionary[tag].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            poolDictionary[tag].Enqueue(objectToSpawn);

            return objectToSpawn;
        }
    }
}
