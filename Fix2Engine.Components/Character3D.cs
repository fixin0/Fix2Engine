using System;
using System.Collections.Generic;

namespace Fix2Engine.Components;

public class Character3D
{
    public Guid _guid
    {
        get; private set; 
        
    }

    public string Name { get; set; }

    public Character3D()
    {
        _guid = new Guid();
    }
    
    
}