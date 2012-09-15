using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Input
{
    /*Vision statement for Multiplayer communication for physics
         * 
         * The scheme is:
         *  - input information goes from client to the server in an ActionUpdatePacket
         *  - physics information goes from the server to the client in an ObjectUpdatePacket
         *  
         *  - Gobject has an AssetName.
         *  - Gobject has an actionManager
         *  - ActionManager has a list of ActionBindings SortedList<string, ActionBinding>   by ActionAlias
         *  - ActionManager has the list of ActionValues List<Object>
         *  
         * the ActionBinding datastructure associates each action alias with a delegate and indices to be access/set in the ActionValues list list which the action uses or requires
         *  - an Action Binding has
         *     - Alias
         *     - Delegate
         *     - Set of ActionValueIndices
         * 
         *   - there is a global list of ActionValues for current frame input.  List<object>
         *   
         *  
         *  An Example
         *   - user presses forward on client side
         *   - car's setAcceleration method is called with the appropriate game-specific new values for acceleration
         *   - there may need to be two version of that setAcceleration method
         *     - one for accepting the ActionValues list when called as a delegate
         *     - one for accepting game-specifc individual, strongly-typed parameters for a clean interface
         *     - the generic, delegate version may call the strongly-typed version.
         *   - update the ActionValues list based on this input
         *   - set the InputApplied boolean to true
         *   - Many updates may occur before an integration is done.
         *   - Before client integration is done, a ActionUpdatePacket is sent for the car (if InputApplied is true)
         *   - This ActionUpdatePacket includes the Gobject ID and all ActionValues 
         *   - Client sends ActionUpdatePacket to the server
         *   - Client proceeds with integration using the physics systems values that match the ActionValues
         *   - Server receives ActionUpdatePacket
         *   - Server queues the ActionUpdatePacket packet
         *   - Before server integrates, ActionUpdatePackets are processed.
         *   - Server calls Gobject.ActionManager.ProcessActionValues on the correct object and provides the ActionValues from the ActionUpdatePacket
         *   - ProcessActionValues iterates through ActionBinding datastructure (this is why ActionManager exists, really)
         *     - Call each delegate for each ActionAlias, using the ActionValues
         *     - All the ActionValues provided in the ActionUpdatePacket get used in this way.
         *     - Use a GetAliasDelegateValues() method to extract the appropriate ActionValues from the ActionValues list  (also a good reason for ActionManager!)
         *     - this Delegate-specific ActionValue list (a subset of the full ActionValues list) is passed to the setAcceleration delegate
         *   - The ActionDelegate assigns the physics system values based on the delegate specific ActionValues 
         *      - (This is ServerSide, but the same method as above, about step 6)
         *      - The delegate/generic setAcceleration() will accept the short List<object>, and call the specific setAcceleration() with appropriately casted parameters.
         *      - The ActionValues have to be cast when passed to the specific setAcceleration method
         *      - the InputApplied boolean doesn't mean anything for the server 
         *        - this will be set because this method has double-duty, as it is used in the client from input and in the server for synchronization with client input
         *   - Any number of ActionUpdatePackets for a single Gobject can be processed by the server before integration
         *   - After server integration is done, an ObjectUpdatePacket is sent for the car to all clients (if the object is moveable)
         *     - this ObjectUpdatePacket includes at least Position, Orientation, and Velocity 
         *   - Client receive an ObjectUpdatePacket
         *   - Client queue an ObjectUpdatePackets
         *   - Before client Integration, process ObjectUpdatePacket queue
         *     - use MoveTo, and other appropriate Gobject/JigLibX methods
         *   - Do client Integration to apply the server's information.
         * 
         * Questions:
         *  - How are UserInput detection, the Physics TimeStep, the XNA Panel refresh rate, and the Multiplayer Update rate related?
         *    - The user input detection is done in BaseGame.ProcessUserInput, which is called on Application.Idle 
         *    - The Physics TimeStep is .01 (simFactor would adjust that, but simFactor is unused at the moment)
         *    - Physics.Integrate is called every 10 ms.
         *    - The XNA Panel refreshes when Windows invalidates the Xna Panel
         *    - Multiplayer updates are sent every 10 ms.
         *    - Ping can be estimated at 100 ms (round trip)
         *   
         * So... every 20 ms, we send an update that may take 50 to get applied.
         * as long as we receive packets at a constant delay, things should be "normal"
         * Lag compenstation steps:
         *   - Forward Prediction
         *   - Interpolation
         * 
         * Jeffrey:
         *  - Made packet
         *  - wiring up in Comm...
         *  
         *  Colby:
         *  - ActionBinding
         *  - ActionManager
         * 
         * 
         */

    public delegate void ActionBindingDelegate(object[] actionValues);

    public class ActionManager
    {
        SortedList<int, ActionBinding> Bindings = new SortedList<int, ActionBinding>();
        List<object> ActionValues = new List<object>();
        private int currDelegateArgIndex = 0;

        public ActionManager()
        {
        }

        public void AddBinding(int uniq, ActionBindingDelegate d, int numDelegateArgs)
        {
            List<int> indicies = new List<int>();
            for(int i=0;i<numDelegateArgs;i++)
            {
                indicies.Add(currDelegateArgIndex);
                currDelegateArgIndex++;
                ActionValues.Add((float)0);
            }
            int index = uniq;//GetAvailableActionID();
            Bindings.Add(index, new ActionBinding(index, d, indicies));
        }

        //private int GetAvailableActionID()
        //{
        //    int id = 1;
        //    bool found = true;
        //    while (found)
        //    {

        //        if (Bindings.ContainsKey(id))
        //            id++;
        //        else
        //            found = false;
        //    }
        //    return id;
        //}

        /// <summary>
        /// This is called ServerSide only to simulate the user input
        /// </summary>
        /// <param name="actionVals"></param>
        public void ProcessActionValues(object[] actionVals)
        {
            ActionValues.Clear();
            ActionValues.AddRange(actionVals);
            foreach (ActionBinding ab in Bindings.Values)
                ab.Callback(GetAliasDelegateValues(ab.ID));
        }

        public object[] GetAliasDelegateValues(int actionId)
        {
            List<object> values = new List<object>();
            ActionBinding ab = Bindings[actionId];
            foreach (int index in ab.Indices)
                values.Add(ActionValues[index]);
            return values.ToArray();
        }

        public object[] GetActionValues()
        {
            return ActionValues.ToArray();
        }

        /// <summary>
        /// this is called ClientSide only for tracking actual user input
        /// </summary>
        /// <param name="uniqId"></param>
        /// <param name="newValues"></param>
        public void SetActionValues(int uniqId, object[] newValues)
        {
            if (!Bindings.ContainsKey(uniqId))
                return;
            ActionBinding ab = Bindings[uniqId];
            int currentNewValueIndex = -1;
            foreach (int index in ab.Indices)
                ActionValues[index] = newValues[++currentNewValueIndex];
            
        }
    }
}
