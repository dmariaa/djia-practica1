using UnityEngine;

namespace Assets.Scripts
{
    public class Loader : MonoBehaviour
    {
        public GameObject gameManager;          //GameManager prefab to instantiate.
        public bool Planner=false;
        public int seed = 2016;
        public int numEnemies = 0;
        void Awake()
        {
            //Check if a GameManager has already been assigned to static variable GameManager.instance or if it's still null
            if (GameManager.instance == null)
            {
                //Instantiate gameManager prefab
                var obj = Instantiate(gameManager) as GameObject;
                obj.name = "GameManager";
                var manager=obj.GetComponent<GameManager>();
                manager.ForPlanner = Planner;
                manager.seed = seed;
                manager.numEnemies = numEnemies;
                manager.InitGame();
            }


        }
    }
}