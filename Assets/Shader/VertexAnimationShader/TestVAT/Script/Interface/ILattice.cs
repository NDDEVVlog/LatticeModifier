using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILattice 
{
    public abstract void InitializeLattice(Lattice customBox3D);
    public abstract void ApplyDeformation();
}
