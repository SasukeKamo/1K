using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerTools
{
    public static Player.Position NextPosition(this Player.Position position)
    {
        return position switch
        {
            Player.Position.down => Player.Position.left,
            Player.Position.left => Player.Position.up,
            Player.Position.up => Player.Position.right,
            Player.Position.right => Player.Position.down,
            _ => Player.Position.down
        };
    }
}
