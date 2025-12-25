# 使用MediaPipe插件在Unity中进行人脸检测

使用unity2022.3及以上版本

1. 导入Release里的unitypackage和SimpleFacePresenceChecker.cs脚本

2. Assets\MediaPipeUnity\Samples\Scenes\Face Detection里的FaceDetection场景为示例场景

3. 把上述文件夹里的FaceDetectorRunner.cs脚本的最后157-160行
   
   ```c#
       private void OnFaceDetectionsOutput(FaceDetectionResult result, Image image, long timestamp)
       {
         _detectionResultAnnotationController.DrawLater(result);
       }
   
   ```
   
   改为（其实就是加了一行）
   
   ```c#
       private void OnFaceDetectionsOutput(FaceDetectionResult result, Image image, long timestamp)
       {
         _detectionResultAnnotationController.DrawLater(result);
         SimpleFacePresenceChecker.Instance?.QueueDetectionResult(result);
       }
   
   ```

4. 在示例场景中的Solution物体上挂载SimpleFacePresenceChecker脚本

5. 即可实现检测人脸是否在摄像头内


