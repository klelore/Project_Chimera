using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class LLMCommander : MonoBehaviour
{
    public static LLMCommander Instance;

    [Header("API 配置")]
    public string apiKey = "YOUR_DEEPSEEK_API_KEY";
    public string apiUrl = "https://api.deepseek.com/chat/completions";
    public string model = "deepseek-chat";

    public Chimera_SearchPartAgent agentScript;

    // 用于定义 LLM 的角色和行为规范
    [TextArea(5, 10)]
    public string systemPrompt = "你是一个机器人指挥官。请根据用户指令，输出 JSON 格式的指令";

    [TextArea(5, 10)]
    public string userInstruction;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

    }

    // 发送指令给 DeepSeek
    public void SendCommand()
    {
        StartCoroutine(PostRequest(userInstruction));
    }

    private IEnumerator PostRequest(string userText)
    {
        // 构建请求体 (兼容 OpenAI 格式)
        ChatRequest requestBody = new ChatRequest();
        requestBody.model = model;
        requestBody.messages = new List<Message>
        {
            new Message { role = "system", content = systemPrompt },
            new Message { role = "user", content = userText }
        };

        string jsonPayload = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("LLM 响应: " + request.downloadHandler.text);
                // 这里解析返回的 JSON 并调用你的 Agent 接口
                ParseResponse(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("请求失败: " + request.error);
            }
        }
    }

    void ParseResponse(string json)
    {
        try
        {
            // 1. 解析 JSON
            LLMResponse response = JsonUtility.FromJson<LLMResponse>(json);

            // 2. 打印思考过程 (Debug 或 UI 显示)
            Debug.Log($"<color=cyan>AI 思考: {response.reasoning}</color>");

            // 3. 应用模式 (利用 Enum.Parse 自动匹配字符串)
            // 假设你有对 Chimera_SearchPartAgent 的引用叫 agentScript
            if (!string.IsNullOrEmpty(response.parameters.agent_mode))
            {
                Chimera_SearchPartAgent.AgentMode newMode =
                    (Chimera_SearchPartAgent.AgentMode)System.Enum.Parse(
                        typeof(Chimera_SearchPartAgent.AgentMode),
                        response.parameters.agent_mode
                    );

                agentScript.currentMode = newMode;
            }

            // 4. 应用坐标
            if (response.command_type == "NAVIGATE")
            {
                agentScript.targetTransform.position = response.parameters.target_position.ToVector3();
            }
            else if (response.command_type == "RESET")
            {
                agentScript.areaController.ResetArea();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON 解析炸了: {e.Message}");
        }
    }


}
// --- 数据结构定义 ---
[System.Serializable]
public class Message { public string role; public string content; }
[System.Serializable]
public class ChatRequest { public string model; public List<Message> messages; }

[System.Serializable]
public class LLMResponse
{
    public string reasoning;      // 思考过程
    public string command_type;   // 指令类型
    public AgentParams parameters; // 参数包
}

[System.Serializable]
public class AgentParams
{
    // 注意：JsonUtility 解析嵌套对象时，类必须加 [Serializable]
    public Vector3Data target_position;
    public string agent_mode;
    public bool reset_environment;
}

[System.Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;

    // 方便转为 Unity 的 Vector3
    public UnityEngine.Vector3 ToVector3()
    {
        return new UnityEngine.Vector3(x, y, z);
    }
}