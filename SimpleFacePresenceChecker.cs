using UnityEngine;
using Mediapipe.Tasks.Components.Containers;
using System.Collections.Generic;

namespace Mediapipe.Unity.Sample.FaceDetection
{
    /// <summary>
    /// 简化版人脸存在性检测器
    /// 使用单例模式，线程安全地处理 MediaPipe 的回调
    /// 
    /// 使用方法：
    /// 1. 将此脚本添加到 Face Detection 场景中的任意 GameObject
    /// 2. 在 FaceDetectorRunner.cs 中调用 Instance.QueueDetectionResult(result)
    /// </summary>
    public class SimpleFacePresenceChecker : MonoBehaviour
    {
        // 单例实例
        private static SimpleFacePresenceChecker _instance;
        public static SimpleFacePresenceChecker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SimpleFacePresenceChecker>();
                }
                return _instance;
            }
        }

        [Header("调试设置")]
        [SerializeField]
        [Tooltip("是否在屏幕上显示检测状态")]
        private bool showOnScreenStatus = true;

        [SerializeField]
        [Tooltip("是否在Console输出详细日志")]
        private bool showConsoleLog = true;

        [Header("检测参数")]
        [SerializeField, Range(0f, 1f)]
        [Tooltip("置信度阈值，高于此值才算检测到人脸")]
        private float confidenceThreshold = 0.5f;

        // 当前状态
        private bool isFacePresent = false;
        private bool previousFacePresent = false;

        // 统计信息
        private int totalFrames = 0;
        private int facePresentFrames = 0;
        private float lastConfidence = 0f;

        // 线程安全的结果队列
        private Queue<DetectionResult> resultQueue = new Queue<DetectionResult>();
        private readonly object queueLock = new object();

        #region Unity 生命周期

        private void Awake()
        {
            // 确保单例
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Debug.LogWarning("[SimpleFacePresenceChecker] 场景中存在多个实例，销毁重复的实例");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Debug.Log("═══════════════════════════════════════════════════════════");
            Debug.Log("? [SimpleFacePresenceChecker] 初始化完成");
            Debug.Log($"   置信度阈值: {confidenceThreshold}");
            Debug.Log($"   显示屏幕状态: {showOnScreenStatus}");
            Debug.Log($"   显示控制台日志: {showConsoleLog}");
            Debug.Log("═══════════════════════════════════════════════════════════");
        }

        private void Update()
        {
            // 在主线程中处理队列中的检测结果
            ProcessQueuedResults();
        }

        #endregion

        #region 公共方法 - 供 FaceDetectorRunner 调用（线程安全）

        /// <summary>
        /// 将检测结果加入队列（线程安全）
        /// 
        /// ?? 在 FaceDetectorRunner.cs 的 OnFaceDetectionsOutput 方法中调用：
        /// SimpleFacePresenceChecker.Instance?.QueueDetectionResult(result);
        /// </summary>
        public void QueueDetectionResult(DetectionResult result)
        {
            lock (queueLock)
            {
                resultQueue.Enqueue(result);
            }
        }

        #endregion

        #region 结果处理

        /// <summary>
        /// 在主线程中处理队列中的检测结果
        /// </summary>
        private void ProcessQueuedResults()
        {
            DetectionResult result;

            lock (queueLock)
            {
                if (resultQueue.Count == 0)
                {
                    return;
                }
                result = resultQueue.Dequeue();
            }

            // 处理检测结果
            ProcessDetectionResult(result);
        }

        /// <summary>
        /// 处理单个检测结果
        /// </summary>
        private void ProcessDetectionResult(DetectionResult result)
        {
            totalFrames++;

            // 检查是否检测到人脸
            bool hasFace = !result.Equals(default(DetectionResult)) &&
                          result.detections != null &&
                          result.detections.Count > 0;

            if (hasFace)
            {
                // 获取第一个检测结果的置信度
                var detection = result.detections[0];
                lastConfidence = detection.categories != null && detection.categories.Count > 0
                    ? detection.categories[0].score
                    : 0f;

                // 根据置信度判断是否真的检测到人脸
                isFacePresent = lastConfidence >= confidenceThreshold;
            }
            else
            {
                isFacePresent = false;
                lastConfidence = 0f;
            }

            // 更新统计
            if (isFacePresent)
            {
                facePresentFrames++;
            }

            // 检查状态变化
            CheckStateChange();

            // 输出日志
            if (showConsoleLog && totalFrames % 30 == 0) // 每30帧输出一次，避免刷屏
            {
                string status = isFacePresent ? " 存在" : " 不存在";
                Debug.Log($"[SimpleFacePresenceChecker] 人脸{status} | 置信度: {lastConfidence:F2} | 检测率: {GetDetectionRate():F1}%");
            }
        }

        #endregion

        #region 状态检查

        /// <summary>
        /// 检查状态是否改变
        /// </summary>
        private void CheckStateChange()
        {
            // 状态没变化，直接返回
            if (isFacePresent == previousFacePresent)
            {
                return;
            }

            // 状态改变了
            if (isFacePresent)
            {
                OnFaceEntered();
            }
            else
            {
                OnFaceLeft();
            }

            previousFacePresent = isFacePresent;
        }

        #endregion

        #region 事件回调 - 在这里添加你的自定义逻辑

        /// <summary>
        /// ?? 当人脸进入摄像头时触发
        /// 
        /// ??? 在这里添加你的自定义事件代码！???
        /// </summary>
        private void OnFaceEntered()
        {
            Debug.Log("════════════════════════════════════════");
            Debug.Log("?? [事件] 人脸进入摄像头！");
            Debug.Log($"   置信度: {lastConfidence:F2}");
            Debug.Log("════════════════════════════════════════");

            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            // ?? 在下方添加你的自定义代码
            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

            // 示例1：播放音效
            // GetComponent<AudioSource>()?.Play();

            // 示例2：调用其他脚本
            // GameManager.Instance?.OnPlayerDetected();

            // 示例3：改变UI
            // UIController.Instance?.ShowWelcomeMessage();

            // 示例4：触发动画
            // GetComponent<Animator>()?.SetTrigger("FaceDetected");

            // 示例5：发送事件
            // EventManager.TriggerEvent("FaceEntered");

            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            // ?? 在上方添加你的自定义代码
            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        }

        /// <summary>
        /// ?? 当人脸离开摄像头时触发
        /// 
        /// ??? 在这里添加你的自定义事件代码！???
        /// </summary>
        private void OnFaceLeft()
        {
            Debug.Log("════════════════════════════════════════");
            Debug.Log("?? [事件] 人脸离开摄像头！");
            Debug.Log("════════════════════════════════════════");

            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            // 在下方添加自定义代码
            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

            // 示例1：播放警告音效
            // GetComponent<AudioSource>()?.PlayOneShot(warningClip);

            // 示例2：调用其他脚本
            // GameManager.Instance?.OnPlayerLost();

            // 示例3：显示警告UI
            // UIController.Instance?.ShowWarning("请回到摄像头前");

            // 示例4：暂停游戏
            // Time.timeScale = 0f;

            // 示例5：发送事件
            // EventManager.TriggerEvent("FaceLeft");

            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            // 在上方添加自定义代码
            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        }

        #endregion

        #region 获取状态的公共方法

        /// <summary>
        /// 获取当前是否检测到人脸
        /// </summary>
        public bool IsFacePresent()
        {
            return isFacePresent;
        }

        /// <summary>
        /// 获取最近的检测置信度
        /// </summary>
        public float GetLastConfidence()
        {
            return lastConfidence;
        }

        /// <summary>
        /// 获取人脸检测率（百分比）
        /// </summary>
        public float GetDetectionRate()
        {
            return totalFrames > 0 ? (float)facePresentFrames / totalFrames * 100f : 0f;
        }

        /// <summary>
        /// 重置统计数据
        /// </summary>
        public void ResetStatistics()
        {
            totalFrames = 0;
            facePresentFrames = 0;
        }

        #endregion

        #region 屏幕显示

        private void OnGUI()
        {
            if (!showOnScreenStatus) return;

            // 显示当前状态
            GUIStyle statusStyle = new GUIStyle();
            statusStyle.fontSize = 28;
            statusStyle.fontStyle = FontStyle.Bold;
            statusStyle.normal.textColor = isFacePresent ? new UnityEngine.Color(0.2f, 1f, 0.2f, 1f) : new UnityEngine.Color(1f, 0.3f, 0.3f, 1f);
            statusStyle.alignment = TextAnchor.UpperLeft;

            string statusText = isFacePresent ? "人脸存在" : "无人脸检测";
            GUI.Label(new UnityEngine.Rect(20f, 20f, 400f, 40f), statusText, statusStyle);

            // 显示详细信息
            GUIStyle infoStyle = new GUIStyle();
            infoStyle.fontSize = 16;
            infoStyle.normal.textColor = UnityEngine.Color.white;
            infoStyle.alignment = TextAnchor.UpperLeft;

            string infoText = $"置信度: {lastConfidence:F2}\n" +
                            $"检测率: {GetDetectionRate():F1}%\n" +
                            $"总帧数: {totalFrames}";

            // 添加半透明背景
            GUI.color = new UnityEngine.Color(0f, 0f, 0f, 0.7f);
            GUI.Box(new UnityEngine.Rect(15f, 65f, 250f, 85f), "");
            GUI.color = UnityEngine.Color.white;

            GUI.Label(new UnityEngine.Rect(20f, 70f, 300f, 80f), infoText, infoStyle);
        }

        #endregion
    }
}