using System.Collections;
using UnityEngine;

public class SceneControllerMine : MonoBehaviour{
    [SerializeField] private GameObject enemyprefabMine;
    private GameObject enemyMine;

    void Start()
    {
        // Instead of running the loop directly, we start the Coroutine
        StartCoroutine(SpawnEnemiesRoutine());
    }

    // Coroutines must return IEnumerator
    IEnumerator SpawnEnemiesRoutine()
    {
        for (int i = 0; i < 10; i++)
        {
            // RANDOM SPAWN & OVERLAP FIX
            Vector3 spawnPos = transform.position;
            bool validPositionFound = false;
            int attempts = 0;
            
            while (!validPositionFound && attempts < 10)
            {
                // Random position in 10m radius
                Vector2 randomCircle = Random.insideUnitCircle * 10f;
                Vector3 proposedPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
                
                // Check if space is clear (assuming humanoid radius ~0.5f)
                if (!Physics.CheckSphere(proposedPos, 0.5f)) // Only spawns if empty space
                {
                    spawnPos = proposedPos;
                    validPositionFound = true;
                }
                attempts++;
            }
            
            // If failed to find specific spot, default to random anyway to avoid exact stack
            if(!validPositionFound) 
            {
                 Vector2 randomCircle = Random.insideUnitCircle * 10f;
                 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            }

            enemyMine = Instantiate(enemyprefabMine, spawnPos, Quaternion.identity);
            
            // Assegna il tag "Enemy" allo zombie spawnato
            if (enemyMine != null)
            {
                enemyMine.tag = "Enemy";
            }
            
            // This tells Unity to pause this specific function for 0.1 seconds,
            // but continue rendering the game and running other scripts.
            yield return new WaitForSeconds(2f);
        }
    }

    void Update()
    {

        
    }

}