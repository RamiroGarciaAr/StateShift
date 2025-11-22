using UnityEngine;

namespace Managers
{
    public sealed class ObjectPoolManager : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _poolSize;

        private GameObject[] _pool;
        private int _nextIndex = 0;

        private void Awake()
        {
            _pool = new GameObject[_poolSize];

            for (int i = 0; i < _pool.Length; i++)
            {
                GameObject gameObject = Instantiate(_prefab, transform);
                gameObject.SetActive(false);

                _pool[i] = gameObject;
            }
        }

        private void Start()
        {
            transform.parent = null;
        }

        public GameObject GetPooledObject()
        {
            GameObject gameObject = _pool[_nextIndex];
            gameObject.SetActive(false);

            _nextIndex++;
            if (_nextIndex == _pool.Length) _nextIndex = 0;

            return gameObject;
        }
    }
}