using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人生成器：定时在生成点生成敌人，控制场上敌人最大数量
public class EnemySpawner : MonoBehaviour
{
    [Header("敌人")]
    // 敌人预制体
    public GameObject enemyPrefab;
    // 每一波生成敌人数量
    public int enemyCount = 5;
    // 场上允许存在敌人的最大上限
    public int maxEnemyCount = 20;

    [Header("敌人生成点")]
    // 全部生成点Transform数组，场景中拖入
    public Transform[] spawnPoints;

    [Header("生成间隔")]
    // 生成一波敌人的时间间隔，单位秒
    public float spawnInterval = 180f;

    // 生成计时器
    private float timer;

    void Start()
    {
        // 游戏启动立刻生成第一波敌人
        SpawnEnemy();
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        // 计时到达，执行生成
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    // 执行敌人生成逻辑
    void SpawnEnemy()
    {
        // 获取场景中所有标记Enemy标签的物体
        GameObject[] currentEnemies =
            GameObject.FindGameObjectsWithTag("Enemy");
        int currentEnemyCount = currentEnemies.Length;

        // 场上敌人达到上限，不再生成
        if (currentEnemyCount >= maxEnemyCount)
        {
            return;
        }

        // 计算还可以生成多少个敌人
        int canSpawnCount = maxEnemyCount - currentEnemyCount;
        int spawnCount = Mathf.Min(enemyCount, canSpawnCount);

        for (int i = 0; i < spawnCount; i++)
        {
            // 随机选取一个生成点
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];

            // 在生成点实例化敌人
            Instantiate(
                enemyPrefab,
                spawnPoint.position,
                Quaternion.identity
            );
        }
    }
}
