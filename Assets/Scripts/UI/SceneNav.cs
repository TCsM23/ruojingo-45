using UnityEngine.SceneManagement;

namespace RuoJingo
{
    public static class SceneNav
    {
        public static void Go(string sceneName) => SceneManager.LoadScene(sceneName);
    }
}
