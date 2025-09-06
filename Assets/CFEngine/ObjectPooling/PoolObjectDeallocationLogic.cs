using System;

namespace CrystalFrost.ObjectPooling
{
    /// <summary>
    /// Defines the logic for determining when a pooled object should be deallocated.
    /// </summary>
    public interface IPoolObjectDeallocationLogic
    {
        /// <summary>
        /// Determines whether the object requires deallocation.
        /// </summary>
        /// <returns>True if the object should be deallocated; otherwise, false.</returns>
        public bool RequiresDeallocation();
    }


    /// <summary>
    /// An implementation of <see cref="IPoolObjectDeallocationLogic"/> that deallocates objects based on their age.
    /// </summary>
    public class AgeBasedDeallocationLogic : IPoolObjectDeallocationLogic
    {
        /// <summary>
        /// Gets or sets the maximum age of the object in seconds before it requires deallocation.
        /// </summary>
        public float MaxAge { get; set; } = 10f;

        private DateTime startingTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgeBasedDeallocationLogic"/> class.
        /// </summary>
        public AgeBasedDeallocationLogic()
        {
            startingTime =  DateTime.Now;
        }

        /// <summary>
        /// Determines whether the object's age has exceeded the maximum allowed age.
        /// </summary>
        /// <returns>True if the object's age is greater than the maximum age; otherwise, false.</returns>
        public bool RequiresDeallocation()
        {
            return (DateTime.Now - startingTime).TotalSeconds > MaxAge;
        }
    }
    


}

