using System;

namespace CrystalFrost.ObjectPooling
{
    public interface IPoolObjectDeallocationLogic
    {
        public bool RequiresDeallocation();
    }


    // A dummy implementation of IPoolObjectDeallocationLogic
    public class AgeBasedDeallocationLogic : IPoolObjectDeallocationLogic
    {
        // Default values
        public float MaxAge { get; set; } = 10f;

        private DateTime startingTime;

        public AgeBasedDeallocationLogic()
        {
            startingTime =  DateTime.Now;
        }

        public bool RequiresDeallocation()
        {
            return (DateTime.Now - startingTime).TotalSeconds > MaxAge;
        }
    }
    


}

