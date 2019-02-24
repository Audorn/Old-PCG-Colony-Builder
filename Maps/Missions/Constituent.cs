using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// A single instance of a ConstituentState.
public class Constituent
{
    // The data for this Constituent's behaviour is based on its state.
    public ConstituentState ConstituentState { get; private set; }

    private uint amount;            // Bitshift between 1 and 31.  Results in 0 - 30.
    public byte Amount              // Translator for uint amount.
    {
        get
        {
            switch (amount)
            {
                case 2: return 1;
                case 4: return 2;
                case 8: return 3;
                case 16: return 4;
                case 32: return 5;
                case 64: return 6;
                case 128: return 7;
                case 256: return 8;
                case 512: return 9;
                case 1024: return 10;
                case 2048: return 11;
                case 4096: return 12;
                case 8192: return 13;
                case 16384: return 14;
                case 32768: return 15;
                case 65536: return 16;
                case 131072: return 17;
                case 262144: return 18;
                case 524288: return 19;
                case 1048576: return 20;
                case 2097152: return 21;
                case 4194304: return 22;
                case 8388608: return 23;
                case 16777216: return 24;
                case 33554432: return 25;
                case 67108864: return 26;
                case 134217728: return 27;
                case 268435456: return 28;
                case 536870912: return 29;
                case 1073741824: return 30;
            }
            return 0; // This should be super rare.
        }
    }

    // Constructors
    public Constituent(ConstituentState constituentState, byte amount)
    {
        ConstituentState = constituentState;
        this.amount = 1; // Sets it to 0 so that it can be bitshifted to whatever 'amount' is.
        this.amount <<= amount;
    }

    // Accessors
    public string Name { get { return ConstituentState.Name; } }
    public Opacity Opacity { get { return ConstituentState.Opacity; } }

    // Behaviours
    public byte RemoveAmount(byte amount = 1)
    {
        byte amountToRemove = Amount > amount ? amount : Amount;    // Find max to remove.
        this.amount >>= amountToRemove;                             // Remove it.
        return (byte)(amount - amountToRemove);                     // Return remaining amount.
    }
    public byte AddAmount(byte amount = 1)
    {
        byte spaceAvailable = (byte)(Voxel.MaxAmount - Amount);                 // Find room.
        byte amountToAdd = spaceAvailable > amount ? amount : spaceAvailable;   // Find max to add.
        this.amount <<= amountToAdd;                                            // Add it.
        return (byte)(amount - amountToAdd);                                    // Return remaining amount.
    }
}
