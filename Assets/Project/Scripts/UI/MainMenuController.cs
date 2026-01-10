using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    // UI元素引用
    public GameObject loginModal;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public CanvasGroup loginModalGroup;


    // 初始化
    void Start()
    {
        // 确保登录弹窗默认是关闭的
        if (loginModal != null)
        {
            loginModal.SetActive(false);
            Canvas.ForceUpdateCanvases(); // 防止初始残留
        }
    }

    // 登录按钮点击
    public void OnLoginButtonClick()
    {
      
        loginModal.SetActive(true);

        loginModalGroup.alpha = 1;          // 显示
        loginModalGroup.interactable = true; // 启用交互
        loginModalGroup.blocksRaycasts = true; // 阻挡射线
        Debug.Log("打开登录窗口");
    }

    // 关闭登录窗口
    public void OnCloseLoginModal()
    {
        
        // 清空输入
        if (usernameInput != null) usernameInput.text = "";
        if (passwordInput != null) passwordInput.text = "";

        loginModalGroup.alpha = 0;          // 透明
        loginModalGroup.interactable = false; // 禁用交互
        loginModalGroup.blocksRaycasts = false; // 不阻挡射线
        loginModal.SetActive(false);
        Canvas.ForceUpdateCanvases();

    }



    // 确认登录
    public void OnConfirmLogin()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("用户名或密码不能为空");
            return;
        }

        Debug.Log($"登录尝试: 用户名={username}");
        // 这里应该调用实际的登录API
        // 简化处理：直接关闭弹窗

        OnCloseLoginModal();
    }

    // 默认场景按钮
    public void OnDefaultSceneClick()
    {
        Debug.Log("进入默认场景");
        // 加载默认场景（需提前创建）
        SceneManager.LoadScene("DefaultPhysicsScenes");
    }

    // 自定义场景按钮
    public void OnCustomSceneClick()
    {
        Debug.Log("进入自定义场景编辑器");
        SceneManager.LoadScene("CustomSceneEditor");
    }

    // 场景收藏按钮
    public void OnFavoriteClick()
    {
        Debug.Log("打开场景收藏");
        SceneManager.LoadScene("FavoriteScenes");
    }

    // 导入场景按钮
    public void OnImportClick()
    {
        Debug.Log("选择导入场景文件");
        // 这里可以调用文件选择器
        // 简化：弹出提示
    }

    // 退出按钮
    public void OnExitClick()
    {
        Debug.Log("退出应用");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    

}