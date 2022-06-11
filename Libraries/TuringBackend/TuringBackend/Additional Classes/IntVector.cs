using System;

namespace TuringBackend
{
    //Not in use, to be added, will allow for tape of any dimensions.
    class IntVector
    {
        int[] Data;
        public IntVector(int Dimensions)
        {
            Data = new int[Dimensions];
        }

        public int this[int Index]
        {
            get
            {
                return Data[Index];
            }
            set
            {
                Data[Index] = value;
            }
        }

        public static IntVector operator +(IntVector Vector, IntVector OtherVector)
        {
            if (Vector.Data.Length != OtherVector.Data.Length)
            {
                throw new Exception("Attempting to add vectors of different dimensions!");
            }

            IntVector ResultVector = new IntVector(Vector.Data.Length);

            for (int i = 0; i < Vector.Data.Length; i++)
            {
                ResultVector[i] = Vector.Data[i] + OtherVector.Data[i];
            }

            return ResultVector;

        }
    }
}
