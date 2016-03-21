//----------------------------------------------------------------------------
//  Copyright (C) 2004-2016 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
#if !(__IOS__ || NETFX_CORE)
using Emgu.CV.Cuda;
#endif

namespace FaceController
{
   public class DetectFace
   {

        private CascadeClassifier face; private CascadeClassifier eye;
        public DetectFace(String face_file_name, String eye_file_name)
        {
            face = new CascadeClassifier(face_file_name);
            eye = new CascadeClassifier(eye_file_name);
        }
      public void Detect(
        Mat image,
        List<Rectangle> faces, List<Rectangle> eyes, 
        out long detectionTime)
      {
         Stopwatch watch;
         
            //Read the HaarCascade objects
            
               watch = Stopwatch.StartNew();
               using (UMat ugray = new UMat())
               {
                  CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                  //normalizes brightness and increases contrast of the image
                  CvInvoke.EqualizeHist(ugray, ugray);

                  //Detect the faces  from the gray scale image and store the locations as rectangle
                  //The first dimensional is the channel
                  //The second dimension is the index of the rectangle in the specific channel
                  Rectangle[] facesDetected = face.DetectMultiScale(
                     ugray,
                     1.1,
                     10,
                     new Size(20, 20));
                     
                  faces.AddRange(facesDetected);

                  foreach (Rectangle f in facesDetected)
                  {
                     //Get the region of interest on the faces
                     using (UMat faceRegion = new UMat(ugray, f))
                     {
                        Rectangle[] eyesDetected = eye.DetectMultiScale(
                           faceRegion,
                           1.1,
                           10,
                           new Size(20, 20));
                        
                        foreach (Rectangle e in eyesDetected)
                        {
                           Rectangle eyeRect = e;
                           eyeRect.Offset(f.X, f.Y);
                           eyes.Add(eyeRect);
                        }
                     }
                  }
               }
               watch.Stop();
            
         detectionTime = watch.ElapsedMilliseconds;
      }
   }
}
