using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Helper.Physics;

namespace Helper.Objects
{
    public class AssetManager
    {
        /*
         * (Object -> Client) Gobject should have an owner field for the client. 
         * (Client -> Object) AssetManager needs a sortedList<id, List<int>>
         * (Object owned?) Gobject owner == -1;
         * 
         */
        
        public SortedList<int, Asset> Assets = new SortedList<int, Asset>();
        private SortedList<int, Gobject> gameObjects;
        private SortedList<int, Gobject> objectsToAdd;
        private List<int> objectsToDelete;
        private SortedList<int, List<int>> ObjectIdsByOwningClient = new SortedList<int, List<int>>();
        private SortedList<int, int> OwningClientsByObjectId = new SortedList<int, int>();

        public AssetManager(ref SortedList<int, Gobject> gObjects, ref SortedList<int, Gobject> nObjects, ref List<int> dObjects)
        {
            gameObjects = gObjects;
            objectsToAdd = nObjects;
            objectsToDelete = dObjects;
        }

        /// <summary>
        /// Adds an asset
        /// </summary>
        /// <param name="a"></param>
        public void AddAsset(Asset a)
        {
            if (Assets.ContainsKey(a.Name))
                return;
            Assets.Add(a.Name, a);
            
        }
        /// <summary>
        /// Adds an asset
        /// </summary>
        /// <param name="name"></param>
        /// <param name="CreateCallback"></param>
        /// <param name="scale"></param>
        public void AddAsset(Enum e, GetGobjectDelegate CreateCallback, Vector3 scale)
        {
            int id = (int)Convert.ChangeType(e, e.GetTypeCode());
            AddAsset(new Asset(id, CreateCallback, scale));
        }
        /// <summary>
        /// Adds an asset with a scale of X = Y = Z = scale
        /// </summary>
        /// <param name="name"></param>
        /// <param name="CreateCallback"></param>
        /// <param name="scale"></param>
        public void AddAsset(Enum e, GetGobjectDelegate CreateCallback, float scale)
        {
            AddAsset(e, CreateCallback, new Vector3(scale, scale, scale));
        }
        /// <summary>
        /// Adds an asset with a default scale of 1
        /// </summary>
        /// <param name="name"></param>
        /// <param name="CreateCallback"></param>
        public void AddAsset(Enum e, GetGobjectDelegate CreateCallback)
        {
            AddAsset(e, CreateCallback, 1.0f);
        }
        /// <summary>
        /// returns an instance of the specified asset type
        /// Assumes no client owns the object
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Gobject GetNewInstance(Enum e)
        {
            return GetNewInstance(e, -1);
        }
        
        /// <summary>
        /// returns an instance of the specified asset type
        /// </summary>
        /// <param name="e"></param>
        /// <param name="owningClientId"></param>
        /// <returns></returns>
        public Gobject GetNewInstance(Enum e, int owningClientId)
        {
            int id = (int)Convert.ChangeType(e, e.GetTypeCode());
            return GetNewInstance(id, owningClientId);
        }


        //public Gobject GetNewInstance<T>(T type, int owningClientId) where T : int
        //{
        //    return GetNewInstance(type, owningClientId);
        //}

        /// <summary>
        /// returns an instance of the specified asset type
        /// </summary>
        /// <param name="e"></param>
        /// <param name="owningClientId"></param>
        /// <returns></returns>
        private Gobject GetNewInstance(int id, int owningClientId)
        {
            if (!Assets.ContainsKey(id))
            {
                Debug.WriteLine("Aborting load of asset unkown to AssetManager: " + id);
                return null;
            }

            Asset a = Assets[id];
            if (a == null)
                return null;

            Gobject go = a.GetNewGobject();
            go.OwningClientId = owningClientId;
            go.type = id; // THIS IS WRONG BUT SO CHEAP! - Colby.
            go.ID = GetAvailableObjectId();

            if (!ObjectIdsByOwningClient.ContainsKey(owningClientId))
                ObjectIdsByOwningClient.Add(owningClientId, new List<int>());

            List<int> ownedIds = ObjectIdsByOwningClient[owningClientId];
            ownedIds.Add(go.ID);

            OwningClientsByObjectId.Add(go.ID, owningClientId);

            // we should centralize all object addition here. (but still return the Gobject)
            // Maybe the baseGame should call PostIntegrate and PreIntegrate methods overridden in the specific game so that Specific-Game can call AssetManager.GetNewInstance() at the appropriate time.
            return go;
        }

        public void RemoveInstance(int objectId)
        {
            if (isObjectOwnedByAnyClient(objectId))
                RemoveObjectIdFromOwnerList(objectId);

            if (OwningClientsByObjectId.ContainsKey(objectId))
                OwningClientsByObjectId.Remove(objectId);

            // we should centralize all object removal here
            // Maybe the baseGame should call PostIntegrate and PreIntegrate methods overridden in the specific game so that Specific-Game can call AssetManager.RemoveInstance() at the appropriate time.
        }

        /// <summary>
        /// Selects an unused object ID
        /// </summary>
        /// <returns></returns>
        public int GetAvailableObjectId()
        {
            int id = 1;
            bool found = true;
            // locks are expensive, I think.
            // We probably don't want to lock inside a loop.
            lock (gameObjects)
            {
                lock (objectsToAdd)
                {
                    lock (objectsToDelete)
                    {
                        while (found)
                        {
                            if (isObjectIdInUse_Unprotected(id))
                                id++;
                            else
                                found = false;
                        }
                    }
                }
            }
            return id;
        }

        /// <summary>
        /// Does NOT lock on purpose. Must only be called from a method that locks gameObjects, ObjectsToAdd, and then ObjectsToDelete
        /// Allows for good performance when being called iteratively. 
        /// Allows for centralized logic to be called from methods that do the required locking 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool isObjectIdInUse_Unprotected(int id)
        {
            return gameObjects.ContainsKey(id) || objectsToAdd.ContainsKey(id) || objectsToDelete.Contains(id); ;
        }

        /// <summary>
        /// Checks to see if an id is in use already
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool isObjectIdInUse(int id)
        {
            lock (gameObjects)
            {
                lock (objectsToAdd)
                {
                    lock (objectsToDelete)
                    {
                        return isObjectIdInUse_Unprotected(id);
                    }
                }
            }
        }

        public bool isObjectOwnedByAnyClient(int objectId)
        {
            if (!gameObjects.ContainsKey(objectId))
                return false;

            if (gameObjects[objectId].OwningClientId == -1)
                return false;
            return true;
        }

        public bool isObjectOwnedByClient(int objectId, int clientId)
        {
            if(!OwningClientsByObjectId.ContainsKey(objectId))
                return false;
            if (OwningClientsByObjectId[objectId] == clientId)
                return true;
            return false;
        }

        public List<int> GetObjectsOwnedByClient(int clientId)
        {
            if(!ObjectIdsByOwningClient.ContainsKey(clientId))
                return new List<int>();
            return ObjectIdsByOwningClient[clientId];
        }

        private void RemoveObjectIdFromOwnerList(int id)
        {
            foreach (List<int> idList in ObjectIdsByOwningClient.Values)
                if (idList.Contains(id))
                    idList.Remove(id);
        }

        
    }
}
