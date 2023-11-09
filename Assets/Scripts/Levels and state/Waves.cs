public static class Waves
{
    public struct Wave
    {
        public int[] enemies;
        public float[] spawnTimes;
    }
    
    public static Wave[] waves = new Wave[10];
    public static int currentWave = 0;
    
    // Waves of enemy1, enemy2 and a boss wave with only enemy3, increasing in difficulty

    public static void Init()
    {
        waves[0].enemies = new[] { 1, 1, 1 };
        waves[0].spawnTimes = new[] { 3f, 6f, 9f };

        waves[1].enemies = new[] { 1, 1, 1, 1 };
        waves[1].spawnTimes = new[] { 2f, 4f, 6f, 8f };

        waves[2].enemies = new[] { 1, 1, 1, 1, 1 };
        waves[2].spawnTimes = new[] { 2f, 2f, 4f, 4f, 6f };

        waves[3].enemies = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
        waves[3].spawnTimes = new[] { 1f, 1f, 2f, 2f, 3f, 3f, 4f, 4f };

        waves[4].enemies = new[] { 2, 2 };
        waves[4].spawnTimes = new[] { 3f, 6f };

        waves[5].enemies = new[] { 1, 1, 2, 1, 1, 2 };
        waves[5].spawnTimes = new[] { 1f, 2f, 3f, 4f, 5f, 6f };

        waves[6].enemies = new[] { 1, 1, 2, 2, 1, 1, 1, 1, 2, 2 };
        waves[6].spawnTimes = new[] { 1f, 1f, 2f, 3f, 4f, 4f, 5f, 5f, 6f, 6f };

        waves[7].enemies = new[] { 1, 2, 2, 1, 2, 2, 1, 2, 2, 1 };
        waves[7].spawnTimes = new[] { 1f, 1f, 2f, 2f, 3f, 3f, 4f, 4f, 5f, 5f };

        waves[8].enemies = new[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        waves[8].spawnTimes = new[] { 1f, 1f, 2f, 2f, 3f, 3f, 4f, 4f, 5f, 5f };

        waves[9].enemies = new[] { 3 };
        waves[9].spawnTimes = new[] { 1f };
    }
}