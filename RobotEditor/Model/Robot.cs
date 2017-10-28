using System.Collections.Generic;

using RobotEditor.ViewModel;

namespace RobotEditor.Model
{

    
    internal class Robot
    {
        public List<Joint> Joints { get; } = new List<Joint>();
        public string Name { get; set; }

        public Robot(int nrOfJoints, string name)
        {
            for(int i = 0; i<nrOfJoints; i++)
            {
                Joints.Add(new Joint());
            }

            Name = name;
        }
    }
}
