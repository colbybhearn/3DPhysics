﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper.Physics;
using Helper.Camera.Cameras;
using Microsoft.Xna.Framework;

namespace Helper.Camera
{
    public class CameraManager
    {
        SortedList<string, BaseCamera> Cameras = new SortedList<string, BaseCamera>();
        public BaseCamera currentCamera = null;
        SortedList<string, SortedList<string, ViewProfile>> Views = new SortedList<string, SortedList<string, ViewProfile>>();

        #region Initialization
        public CameraManager()
        {
        }

        public void AddCamera(string alias, BaseCamera newCam)
        {
            if (GetCamera(alias) != null) return;

            Cameras.Add(alias, newCam);

            /// if this is the first camera, make it the default
            if (Cameras.Count == 1)
                SetCurrentCamera(alias);
        }
        public void AddProfile(ViewProfile vp)
        {
            if (!Views.ContainsKey(vp.CameraAlias))
                Views.Add(vp.CameraAlias, new SortedList<string, ViewProfile>());
            SortedList<string, ViewProfile> camViews = Views[vp.CameraAlias];
            if (camViews.ContainsKey(vp.assetAlias))
                return;
            camViews.Add(vp.assetAlias, vp);

        }

        public void SetGobjectList(string camAlias, List<Gobject> gobs)
        {
            BaseCamera cam = GetCamera(camAlias);
            if (cam == null) return;

            cam.SetGobjectList(gobs);

            if (Views.ContainsKey(camAlias))
            {
                SortedList<string, ViewProfile> camViews = Views[camAlias];
                foreach (Gobject gob in gobs)
                    if (!camViews.ContainsKey(gob.Asset))
                        camViews.Add(gob.Asset, camViews[gob.Asset]);
                cam.SetProfiles(camViews);
            }
            else
            {
                cam.SetProfiles(new SortedList<string,ViewProfile>());
            }
            
        }
        #endregion

        #region Current Camera
        
        public void SetCurrentCamera(string alias)
        {
            BaseCamera cam = GetCamera(alias);
            if (cam == null) return;
            currentCamera = cam;
        }

        public void Update()
        {
            if (currentCamera == null) return;
            currentCamera.Update();
        }

        public Matrix ViewMatrix()
        {
            if (currentCamera == null) return Matrix.Identity;
            return currentCamera.GetViewMatrix();
        }

        public Matrix ProjectionMatrix()
        {
            if (currentCamera == null) return Matrix.Identity;
            return currentCamera.GetProjectionMatrix();
        }
        #endregion

        #region Utility
        private BaseCamera GetCamera(string alias)
        {
            if (!Cameras.ContainsKey(alias))
                return null;
            return Cameras[alias];
        }
        #endregion

        public void IncreaseMovementSpeed()
        {
            currentCamera.IncreaseMovementSpeed();
        }

        public void AdjustTargetOrientation(float p, float y)
        {
            currentCamera.AdjustTargetOrientation(p,y);
        }
        public void DecreaseMovementSpeed()
        {
            currentCamera.DecreaseMovementSpeed();
        }
        public void ZoomIn()
        {
            currentCamera.ZoomIn();
        }
        public void ZoomOut()
        {
            currentCamera.ZoomOut();

        }
        public void MoveUp()
        {
            currentCamera.MoveUp();
        }

        public void MoveDown()
        {
            currentCamera.MoveDown();
        }

        public void MoveForward()
        {
            currentCamera.MoveForward();
        }

        public void MoveBackward()
        {
            currentCamera.MoveBackward();
        }

        public void MoveLeft()
        {
            currentCamera.MoveLeft();
        }

        public void MoveRight()
        {
            currentCamera.MoveRight();
        }

    }
}