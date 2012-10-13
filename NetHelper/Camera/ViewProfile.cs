using Microsoft.Xna.Framework;

namespace Helper.Camera
{

    /* Vision statement
     *  - We need something to hold the various poperties of how the camera moves
     *  - may or may not have anything to do with an object.
     *  - 3 methods that return cam position and cam look at
     *  
     * 
     * What would a ViewManager do?
     * What would ViewProfile do
     * What does camera do?
     * 
     * Game needs to know about camera modes. Camera manager could handle with that.
     * CameraManager could allow multiple cameras to be added, with an id.
     * SpecificCameras could be overridden classes from a base camera
     * SpecificCamera is where the custom logic goes.
     * You can tell the camera manager which camera to use
     * you can tell the camera manager which object to watch
     * 
     * Degree of freedom
     *  Different cam types / with different attributes per object
     *  
     * 
     * How will the watch camera know which object to watch?
     * Base camera could have a list of Gobjects.
     * Camera controls
     *  
     * 
     */

    public class ViewProfile
    {
        public int assetAlias;
        public string CameraAlias;
        public float PositionLagFactor;
        public float OrientationLagFactor;
        public Vector3 MaximumShakiness;
        public Vector3 PositionOffset;
        public Vector3 OrientationOffset;

        public ViewProfile(string camAlias, int asset, Vector3 posOffset, float posLag, Vector3 orientOffsetXYZ, float orientLag)
        {
            assetAlias = asset;
            CameraAlias = camAlias;
            PositionOffset = posOffset;
            PositionLagFactor = posLag;
            OrientationOffset = orientOffsetXYZ;
            OrientationLagFactor = orientLag;
            MaximumShakiness = Vector3.Zero;
        }
    }
}
