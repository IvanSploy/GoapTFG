using UnityEngine;
using System.Collections.Generic;

namespace Panda.Examples.Shooter
{
    public class ShooterGameController : MonoBehaviour
    {

        public Unit player = null;
        List<Unit> enemies = new List<Unit>();

        static ShooterGameController _instance = null;

        public static ShooterGameController instance
        {
            get
            {
                if(!_instance)
                {
                    _instance = FindFirstObjectByType<ShooterGameController>();
                }
                return _instance;
            }
        }


        public void OnUnitDestroy( Unit unit )
        {
            if (enemies.Contains(unit))
                enemies.Remove(unit);
        }

        [Task]
        bool IsPlayerDead()
        {
            return player == null;
        }

        [Task]
        bool IsLevelCompleted()
        {
            return enemies.Count == 0;
        }

        [Task]
        bool ReloadLevel()
        {
            Debug.Log($"[SHOOTER] Enemies left: {enemies.Count}");
#if UNITY_5_2 
            Application.LoadLevel(Application.loadedLevel);
#else
            UnityEngine.SceneManagement.SceneManager.LoadScene( UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
#endif
            return true;
        }

        // Use this for initialization
        void Start()
        {
            enemies.AddRange(FindObjectsOfType<Unit>());
            enemies.RemoveAll( (u) => !u.enabled || u.team == player.team) ;
        }

        public int GetCurrentEnemies()
        {
            return enemies.Count;
        }

        // SetParent is called once per frame
        void Update()
        {

        }
    }
}
